using CodeBrix.Audio.Codecs;
using CodeBrix.Audio.Engine.Interfaces;
using CodeBrix.Audio.Vorbis;
using CodeBrix.VideoPlayback;
using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoPlayback.Containers.Ivf;
using CodeBrix.VideoPlayback.Containers.Matroska;
using CodeBrix.VideoPlayback.Containers.Ogg;
using CodeBrix.VideoPlayback.Decoding;
using CodeBrix.VideoProcessing.Core.Tools;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Tools;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Turns a bespoke CBVF file - Mode 2, or its master form, Mode 3 - into something a video-processing
/// engine can read.
/// </summary>
/// <remarks>
/// <para>
/// No engine can open the bespoke CBVF container - not FFmpeg on the desktop, not the platform's media
/// extractor on Android - so a Mode 2 or Mode 3 file cannot be handed straight to a conversion pass the
/// way a Mode 1, WebM or Matroska file can. What they can read is the streams inside it, which are
/// perfectly ordinary AV1 and Vorbis - or, in a master, FLAC. So this demultiplexes the file with the
/// playback core's own reader, re-wraps the AV1 stream in IVF with the core's own <see cref="IvfWriter" />,
/// re-wraps Vorbis or Opus sound in Ogg with its <see cref="OggAudioWriter" /> - the two containers the
/// authoring library itself writes when it builds a bespoke file, used here in the opposite direction -
/// and muxes the two into one Matroska file with the core's own <see cref="MatroskaMuxer" />, with no
/// re-encoding and no external tool at all.
/// </para>
/// <para>
/// A master's FLAC sound is the one thing that muxer does not take: it writes Opus and Vorbis tracks
/// only. Where the registered engine can run FFmpeg, the FLAC is written out as a plain FLAC stream and
/// FFmpeg remuxes it, still without re-encoding anything. Where it cannot - on Android - the FLAC frames
/// are decoded as they are read and the sound goes into the intermediate as Vorbis at the encoder's
/// highest quality, and the extraction says so. The destination re-encodes the sound in any case; this
/// is one generation more, at a quality chosen to make it as close to nothing as Vorbis gets.
/// </para>
/// <para>
/// The same rewrap serves a Matroska or WebM file the engine cannot time as it stands. Android's
/// media extractor reports no frame rate for a WebM document with its index in front - every Mode 1
/// and WebM file this application writes - although the track states its frame duration, and the
/// engine will not decode a stream whose rate it does not know. The Matroska intermediate, index at
/// the end, it reads. <see cref="Operations.ConversionRunner" /> asks the engine first and rewraps only
/// when it has to.
/// </para>
/// <para>
/// From that point on the conversion is an ordinary conversion. Nothing is decoded, nothing is
/// re-encoded, and the chapters and caption cues come straight out of the header where the format
/// keeps them. A master's alpha-plane track is the one thing left behind: every destination this path
/// leads to is opaque, so only the picture track is carried across, and the result says so.
/// </para>
/// </remarks>
public sealed class Mode2Extractor
{
    //Vorbis at the top of its scale - about 500 kbit/s for 44.1 or 48 kHz stereo - for a master's FLAC
    //sound where nothing but Vorbis or Opus can go into the intermediate.
    private const float BridgeVorbisQuality = VorbisEncoderOptions.MaximumQuality;

    /// <summary>Demultiplexes a bespoke CBVF file and remuxes it into an engine-readable intermediate.</summary>
    /// <param name="source">What probing found in the file.</param>
    /// <param name="workingFolder">A folder the intermediate files may be written into.</param>
    /// <param name="cancellationToken">Stops the extraction.</param>
    /// <returns>The intermediate file, the two elementary streams, and the chapters and captions.</returns>
    /// <exception cref="VideoToolProcessingException">
    /// The file is not a bespoke CBVF file, carries no AV1 video, or could not be re-wrapped.
    /// </exception>
    /// <exception cref="OperationCanceledException">The extraction was cancelled.</exception>
    public async Task<Mode2Extraction> ExtractAsync(
        SourceMediaInfo source, string workingFolder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        Directory.CreateDirectory(workingFolder);

        var ivfPath = Path.Combine(workingFolder, "video.ivf");
        var intermediatePath = Path.Combine(workingFolder, "intermediate.mkv");

        int videoFrames;
        var audioPackets = 0;
        var audioCodecId = string.Empty;
        string audioPath = null;
        var hasAudio = false;
        var audioIsFlac = false;
        bool hasAlphaPlane;
        MediaSidecars sidecars;

        //FLAC goes to FFmpeg as it is where the engine can run FFmpeg, and into Vorbis where it cannot.
        var hasTools = ProcessingEngine.TryGetExternalTools(out var tools);

        using (var reader = MediaContainers.Open(source.Path))
        {
            //A Matroska or WebM file is rewrapped the same way when the engine cannot time it as it stands -
            //see ConversionRunner. Anything else is not this class's business.
            if (reader is not CbvReader and not MatroskaReader)
            {
                throw new VideoToolProcessingException(
                    $"'{source.FileName}' is neither a bespoke CodeBrix Mode 2 or Mode 3 file nor a Matroska or WebM file, so it cannot be rewrapped.");
            }

            //The picture, never a master's alpha plane: that is a second video track beside it.
            var video = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Video && !t.IsAlphaPlane)
                ?? throw new VideoToolProcessingException($"'{source.FileName}' carries no video track.");

            if (!string.Equals(video.CodecId, VideoCodecIds.Av1, StringComparison.OrdinalIgnoreCase))
            {
                throw new VideoToolProcessingException(
                    $"'{source.FileName}' carries '{video.CodecId}' video; only AV1 can be re-wrapped into IVF.");
            }

            hasAlphaPlane = reader.Tracks.Any(t => t.IsAlphaPlane);
            var audio = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Audio);

            audioIsFlac = audio is not null && IsFlac(audio);
            var flacAsVorbis = audioIsFlac && !hasTools;

            using var ivf = IvfWriter.CreateAv1(ivfPath, video.Width, video.Height);
            if (audio is not null)
            {
                //FLAC needs no Ogg around it: a FLAC stream is its header and then its frames, end to end.
                audioPath = Path.Combine(workingFolder, audioIsFlac && !flacAsVorbis ? "audio.flac" : "audio.ogg");
            }

            var audioWriter = audio is null ? null : CreateAudioWriter(audio, audioPath, flacAsVorbis, source.FileName);

            try
            {
                Demultiplex(reader, video.Id, audio?.Id ?? -1, ivf, audioWriter, cancellationToken,
                    out videoFrames, out audioPackets);

                ivf.Complete();
                audioWriter?.Complete();
            }
            finally
            {
                audioWriter?.Dispose();
            }

            if (videoFrames == 0)
            {
                throw new VideoToolProcessingException(
                    $"'{source.FileName}' produced no video frames, so there is nothing to convert.");
            }

            if (audio is not null)
            {
                hasAudio = audioPackets > 0;
                audioCodecId = audio.CodecId ?? string.Empty;
            }

            sidecars = SidecarExtractor.ExtractFromReader(reader, workingFolder);
        }

        var muxedAudioPath = hasAudio ? audioPath : null;
        if (hasAudio && audioIsFlac && hasTools)
        {
            await RemuxFlacWithFfmpegAsync(tools, ivfPath, muxedAudioPath, intermediatePath, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            MuxInProcess(ivfPath, muxedAudioPath, intermediatePath, cancellationToken);
        }

        return new Mode2Extraction(
            intermediatePath, ivfPath, muxedAudioPath, audioCodecId,
            videoFrames, audioPackets, hasAlphaPlane, sidecars,
            soundCarriedAsVorbis: hasAudio && audioIsFlac && !hasTools);
    }

    private static bool IsFlac(MediaTrackInfo audio) =>
        string.Equals(audio.CodecId, VideoCodecIds.Flac, StringComparison.OrdinalIgnoreCase);

    private static IAudioStreamWriter CreateAudioWriter(MediaTrackInfo audio, string path, bool flacAsVorbis, string fileName)
    {
        try
        {
            if (string.Equals(audio.CodecId, VideoCodecIds.Vorbis, StringComparison.OrdinalIgnoreCase))
            {
                return new OggStreamWriter(OggAudioWriter.CreateVorbis(path, audio.CodecPrivate.Span, audio.SampleRate));
            }

            if (string.Equals(audio.CodecId, VideoCodecIds.Opus, StringComparison.OrdinalIgnoreCase))
            {
                return new OggStreamWriter(OggAudioWriter.CreateOpus(path, audio.CodecPrivate.Span, audio.PreSkipSamples));
            }
        }
        catch (VideoPlaybackException exception)
        {
            //The core refuses codec-private data that is not the three Xiph-laced Vorbis headers or an
            //OpusHead; that is this application's "could not be re-wrapped" case, in its own words.
            throw new VideoToolProcessingException(
                $"'{fileName}' carries '{audio.CodecId}' audio whose setup data could not be re-wrapped into an Ogg stream: {exception.Message}",
                exception);
        }

        if (IsFlac(audio))
        {
            return flacAsVorbis
                ? FlacAsVorbisWriter.Create(path, audio.CodecPrivate, fileName)
                : FlacStreamWriter.Create(path, audio.CodecPrivate.Span, fileName);
        }

        throw new VideoToolProcessingException(
            $"'{fileName}' carries '{audio.CodecId}' audio, which cannot be re-wrapped into an intermediate file.");
    }

    private static void Demultiplex(
        IMediaContainerReader reader,
        int videoTrackId,
        int audioTrackId,
        IvfWriter ivf,
        IAudioStreamWriter audioWriter,
        CancellationToken cancellationToken,
        out int videoFrames,
        out int audioPackets)
    {
        videoFrames = 0;
        audioPackets = 0;

        //One packet of lookahead on the audio side: an Ogg granule position says where a packet
        //ENDS, and the next packet's timestamp is the most reliable statement of that.
        byte[] pendingAudio = null;
        var pendingTimestamp = TimeSpan.Zero;
        var pendingDuration = TimeSpan.Zero;

        while (reader.TryReadPacket(out var packet))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (packet.TrackId == videoTrackId)
            {
                ivf.WriteFrame(packet.Data.Span, packet.Timestamp);
                videoFrames++;
                continue;
            }

            if (audioWriter is null || packet.TrackId != audioTrackId)
            {
                continue;
            }

            if (pendingAudio is not null)
            {
                audioWriter.WritePacket(pendingAudio, EndOf(pendingTimestamp, pendingDuration, packet.Timestamp));
                audioPackets++;
            }

            //MediaPacket.Data is borrowed from the reader and is gone on the next read.
            pendingAudio = packet.Data.ToArray();
            pendingTimestamp = packet.Timestamp;
            pendingDuration = packet.Duration;
        }

        if (pendingAudio is not null && audioWriter is not null)
        {
            var end = pendingDuration > TimeSpan.Zero
                ? pendingTimestamp + pendingDuration
                : MaxOf(pendingTimestamp, reader.Duration);
            audioWriter.WritePacket(pendingAudio, end);
            audioPackets++;
        }
    }

    private static TimeSpan EndOf(TimeSpan timestamp, TimeSpan duration, TimeSpan nextTimestamp) =>
        duration > TimeSpan.Zero ? timestamp + duration : MaxOf(timestamp, nextTimestamp);

    private static TimeSpan MaxOf(TimeSpan first, TimeSpan second) => second > first ? second : first;

    /// <summary>
    /// Muxes the IVF picture and the Ogg sound into the Matroska intermediate with the playback core's own
    /// muxer: in process, nothing re-encoded, on every engine.
    /// </summary>
    private static void MuxInProcess(string ivfPath, string oggPath, string outputPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            MatroskaMuxer.Write(new MatroskaAuthoringRequest
            {
                OutputPath = outputPath,
                DocType = MatroskaDocType.Matroska,
                VideoIvfPath = ivfPath,
                AudioOggPath = oggPath,

                //An intermediate is read once, start to finish, by the conversion that follows: where its
                //index sits and whether it would stream are nobody's business.
                CuesToFront = false,
                EvaluateProfile = false,
            });
        }
        catch (Exception exception) when (exception is VideoPlaybackException or IOException or ArgumentException)
        {
            throw new VideoToolProcessingException(
                "The demultiplexed CodeBrix streams could not be muxed into an intermediate file: " + exception.Message,
                exception);
        }
    }

    /// <summary>
    /// Remuxes the IVF picture and a plain FLAC stream into the Matroska intermediate with FFmpeg, copying
    /// both: the one remux the core's muxer cannot do, run through the engine's external tools.
    /// </summary>
    private static async Task RemuxFlacWithFfmpegAsync(
        IExternalTools tools, string ivfPath, string flacPath, string outputPath, CancellationToken cancellationToken)
    {
        var arguments = FfmpegLine.Start()
            .Add("-i").AddPath(ivfPath)
            .Add("-i").AddPath(flacPath)
            .Add("-map 0:v:0")
            .Add("-map 1:a:0")
            .Add("-c copy")
            .Add("-f matroska")
            .AddPath(outputPath);

        var run = await FfmpegLine.RunAsync(tools, arguments, TimeSpan.Zero, null, cancellationToken)
            .ConfigureAwait(false);

        if (!run.Succeeded || !File.Exists(outputPath))
        {
            throw new VideoToolProcessingException(
                "The demultiplexed CodeBrix streams could not be muxed into an intermediate file. " + run.ErrorText);
        }
    }

    //Where the sound goes: an Ogg stream for Vorbis and Opus, a plain FLAC stream for a master's FLAC.
    private interface IAudioStreamWriter : IDisposable
    {
        void WritePacket(ReadOnlySpan<byte> packet, TimeSpan endTimestamp);

        void Complete();
    }

    private sealed class OggStreamWriter(OggAudioWriter writer) : IAudioStreamWriter
    {
        public void WritePacket(ReadOnlySpan<byte> packet, TimeSpan endTimestamp) => writer.WritePacket(packet, endTimestamp);

        public void Complete() => writer.Complete();

        public void Dispose() => writer.Dispose();
    }

    /// <summary>
    /// A FLAC track's codec-private data is a FLAC stream header - "fLaC" and the metadata blocks,
    /// STREAMINFO first - and each of its packets is exactly one complete FLAC frame. Written end to
    /// end, header first, that is a FLAC file, and nothing about it has to be re-timed: every frame
    /// states its own position.
    /// </summary>
    private sealed class FlacStreamWriter : IAudioStreamWriter
    {
        private readonly FileStream stream;

        private FlacStreamWriter(FileStream stream)
        {
            this.stream = stream;
        }

        public static FlacStreamWriter Create(string path, ReadOnlySpan<byte> streamHeader, string fileName)
        {
            if (!streamHeader.StartsWith(CbvFormat.FlacMarker))
            {
                throw new VideoToolProcessingException(
                    $"'{fileName}' carries FLAC audio whose setup data is not a FLAC stream header, so it cannot be written out for FFmpeg.");
            }

            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(streamHeader);
            return new FlacStreamWriter(stream);
        }

        public void WritePacket(ReadOnlySpan<byte> packet, TimeSpan endTimestamp) => stream.Write(packet);

        public void Complete() => stream.Flush();

        public void Dispose() => stream.Dispose();
    }

    /// <summary>
    /// Where there is no FFmpeg to remux FLAC: decodes each FLAC frame as it is read, with the audio core's
    /// own managed decoder - every packet is one complete frame and decodes on its own - and encodes the
    /// samples straight into an Ogg Vorbis file at the highest quality, which the core's muxer does take.
    /// </summary>
    private sealed class FlacAsVorbisWriter : IAudioStreamWriter
    {
        private readonly IPacketSoundDecoder decoder;
        private readonly FileStream stream;
        private readonly OggVorbisEncoder encoder;
        private readonly float[] samples;

        private FlacAsVorbisWriter(IPacketSoundDecoder decoder, FileStream stream, OggVorbisEncoder encoder)
        {
            this.decoder = decoder;
            this.stream = stream;
            this.encoder = encoder;
            samples = new float[decoder.MaxSamplesPerPacket];
        }

        public static FlacAsVorbisWriter Create(string path, ReadOnlyMemory<byte> streamHeader, string fileName)
        {
            var decoder = new FlacPacketCodecFactory().CreateDecoder(VideoCodecIds.Flac, streamHeader, null)
                ?? throw new VideoToolProcessingException(
                    $"'{fileName}' carries FLAC audio whose setup data is not a FLAC stream header, so its sound cannot be decoded.");

            FileStream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                var encoder = new OggVorbisEncoder(stream, decoder.Channels, decoder.SampleRate,
                    new VorbisEncoderOptions { Mode = VorbisEncoderMode.Quality, Quality = BridgeVorbisQuality });
                return new FlacAsVorbisWriter(decoder, stream, encoder);
            }
            catch (Exception exception) when (exception is VorbisEncoderException or ArgumentException)
            {
                stream?.Dispose();
                var described = $"{decoder.Channels}-channel {decoder.SampleRate} Hz";
                decoder.Dispose();
                throw new VideoToolProcessingException(
                    $"'{fileName}' carries {described} FLAC audio that could not be carried across as Vorbis: {exception.Message}",
                    exception);
            }
        }

        public void WritePacket(ReadOnlySpan<byte> packet, TimeSpan endTimestamp)
        {
            var written = decoder.DecodePacket(packet, samples);
            if (written > 0)
            {
                encoder.Encode(samples.AsSpan(0, written));
            }
        }

        public void Complete()
        {
            encoder.Flush();
            stream.Flush();
        }

        public void Dispose()
        {
            encoder.Dispose();
            stream.Dispose();
            decoder.Dispose();
        }
    }
}
