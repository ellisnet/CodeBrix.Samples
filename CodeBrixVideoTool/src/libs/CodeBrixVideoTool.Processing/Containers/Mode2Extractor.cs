using CodeBrix.VideoPlayback;
using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoPlayback.Containers.Ivf;
using CodeBrix.VideoPlayback.Containers.Ogg;
using CodeBrix.VideoPlayback.Decoding;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Enums;
using CodeBrixVideoTool.Processing.Probing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Turns a bespoke CBVF file - Mode 2, or its master form, Mode 3 - into something FFmpeg can read.
/// </summary>
/// <remarks>
/// <para>
/// FFmpeg cannot open the bespoke CBVF container, so a Mode 2 or Mode 3 file cannot be handed straight
/// to a conversion pass the way a Mode 1, WebM or Matroska file can. What it can do is read the streams
/// inside it, which are perfectly ordinary AV1 and Vorbis - or, in a master, FLAC. So this
/// demultiplexes the file with the playback core's own reader, re-wraps the AV1 stream in IVF with the
/// core's own <see cref="IvfWriter" />, re-wraps Vorbis or Opus sound in Ogg with its
/// <see cref="OggAudioWriter" /> - the two containers the authoring library itself writes when it builds
/// a bespoke file, used here in the opposite direction - writes FLAC sound out as a plain FLAC stream,
/// and muxes the two into one Matroska file with no re-encoding at all.
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
    /// <summary>Demultiplexes a bespoke CBVF file and remuxes it into an FFmpeg-readable intermediate.</summary>
    /// <param name="source">What probing found in the file.</param>
    /// <param name="workingFolder">A folder the intermediate files may be written into.</param>
    /// <param name="cancellationToken">Stops the extraction.</param>
    /// <returns>The intermediate file, the two elementary streams, and the chapters and captions.</returns>
    /// <exception cref="VideoToolProcessingException">
    /// The file is not a bespoke CBVF file, carries no AV1 video, or could not be re-wrapped.
    /// </exception>
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
        bool hasAlphaPlane;
        MediaSidecars sidecars;

        using (var reader = MediaContainers.Open(source.Path))
        {
            if (reader is not CbvReader)
            {
                throw new VideoToolProcessingException(
                    $"'{source.FileName}' is not a bespoke CodeBrix Mode 2 or Mode 3 file, so it does not need demultiplexing.");
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

            using var ivf = IvfWriter.CreateAv1(ivfPath, video.Width, video.Height);
            if (audio is not null)
            {
                audioPath = Path.Combine(workingFolder, AudioFileNameFor(audio));
            }

            var audioWriter = audio is null ? null : CreateAudioWriter(audio, audioPath, source.FileName);

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

        await RemuxAsync(ivfPath, hasAudio ? audioPath : null, intermediatePath, cancellationToken)
            .ConfigureAwait(false);

        return new Mode2Extraction(
            intermediatePath, ivfPath, hasAudio ? audioPath : null, audioCodecId,
            videoFrames, audioPackets, hasAlphaPlane, sidecars);
    }

    //FLAC needs no Ogg around it: a FLAC stream is its header and then its frames, end to end.
    private static string AudioFileNameFor(MediaTrackInfo audio) =>
        string.Equals(audio.CodecId, VideoCodecIds.Flac, StringComparison.OrdinalIgnoreCase) ? "audio.flac" : "audio.ogg";

    private static IAudioStreamWriter CreateAudioWriter(MediaTrackInfo audio, string path, string fileName)
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

        if (string.Equals(audio.CodecId, VideoCodecIds.Flac, StringComparison.OrdinalIgnoreCase))
        {
            return FlacStreamWriter.Create(path, audio.CodecPrivate.Span, fileName);
        }

        throw new VideoToolProcessingException(
            $"'{fileName}' carries '{audio.CodecId}' audio, which cannot be re-wrapped for FFmpeg.");
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

    private static async Task RemuxAsync(
        string ivfPath, string audioPath, string outputPath, CancellationToken cancellationToken)
    {
        var arguments = FFMpegArguments.FromFileInput(ivfPath);
        if (audioPath is not null)
        {
            arguments = arguments.AddFileInput(audioPath);
        }

        var errors = new List<string>();
        var succeeded = await arguments
            .OutputToFile(outputPath, true, options =>
            {
                options.SelectStream(0, 0, Channel.Video);
                if (audioPath is not null)
                {
                    options.SelectStream(0, 1, Channel.Audio);
                }

                options.WithCopyCodec().ForceFormat("matroska");
            })
            .NotifyOnError(errors.Add)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(false)
            .ConfigureAwait(false);

        if (!succeeded || !File.Exists(outputPath))
        {
            throw new VideoToolProcessingException(
                "The demultiplexed CodeBrix streams could not be muxed into an intermediate file. " +
                string.Join(" ", errors.TakeLast(5)));
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
}
