using CodeBrix.VideoPlayback.Authoring;
using CodeBrix.VideoPlayback.Authoring.Captions;
using CodeBrix.VideoPlayback.Authoring.Encoding;
using CodeBrix.VideoPlayback.Authoring.Export;
using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Core.Output;
using CodeBrix.VideoProcessing.Enums;
using CodeBrixVideoTool.Processing.Containers;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Planning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Operations;

/// <summary>
/// Carries out one conversion: the five supported formats through the authoring library, an
/// <c>.mp4</c> export through FFmpeg - or, for a Mode 3 master, through the authoring library's
/// exporter - and a Mode 2 or Mode 3 source through the demultiplexer first.
/// </summary>
/// <remarks>
/// Every conversion runs in exactly two stages. The first prepares the source - demultiplexing a
/// bespoke CBVF file when there is one, and lifting out the chapters and captions the authoring pass
/// has to be handed as separate inputs. The second encodes. Only the second can report a percentage,
/// because only the encoder knows how far through the media it has got.
/// </remarks>
public sealed class ConversionRunner : IConversionRunner
{
    //Faster than the authoring library's own default of 6, which matters a great deal for an
    //application a person is sitting in front of, and costs very little at these bit rates. It is
    //PINNED: the quality knob moves the rate factor only, so an encode takes about as long whichever
    //stop is chosen.
    private const int Av1SpeedPreset = 8;

    private const int Mp4AudioKilobitsPerSecond = 192;

    //Vorbis is rate-controlled by QUALITY, not by bit rate - see BuildAuthoringRequest. Quality 4 is
    //the nominal 128 kbit/s for 44.1 or 48 kHz stereo, the same figure the authoring library uses as
    //its bit-rate default, and it scales with the channel count.
    private const double VorbisQuality = 4d;

    //A master's sound is lossless whatever is chosen; 16 bits is the authoring library's own default and
    //what nearly every source this application is given was recorded at, so it comes out sample for
    //sample rather than being widened for nothing.
    private const int FlacBitDepth = 16;

    //The one note the exporter writes that this application makes untrue: a master's captions and
    //chapters ARE carried into the MP4, by the remux pass after the export. It is matched by its opening
    //words, which are the exporter's own.
    private const string ExporterCaptionsNoteStart = "the source carries captions and/or chapters";

    private readonly Mode2Extractor mode2Extractor = new();
    private readonly SidecarExtractor sidecarExtractor = new();

    /// <inheritdoc />
    public async Task<ConversionOutcome> RunAsync(
        ConversionPlan plan, IProgress<ConversionProgress> progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var stopwatch = Stopwatch.StartNew();
        var notes = new List<string>();
        var workingFolder = Path.Combine(
            Path.GetTempPath(), "CodeBrixVideoTool", Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(workingFolder);

            progress?.Report(new ConversionProgress("Reading the source", 1, 2, null));

            var sourcePath = plan.Source.Path;
            var sidecars = MediaSidecars.None;

            if (plan.RequiresMode2Extraction)
            {
                var extraction = await mode2Extractor
                    .ExtractAsync(plan.Source, workingFolder, cancellationToken).ConfigureAwait(false);
                sourcePath = extraction.IntermediatePath;
                sidecars = extraction.Sidecars;
                notes.Add($"{MediaFormats.ShortName(plan.Source.Format)} source demultiplexed without re-encoding: {extraction}.");

                if (extraction.LeftAlphaPlaneBehind)
                {
                    notes.Add($"The master's alpha plane was left behind: {MediaFormats.DisplayName(plan.Destination)} " +
                              "is opaque, so the picture is written without its transparency.");
                }
            }
            else if (plan.Source.CaptionTrackCount > 0 || plan.Source.ChapterCount > 0)
            {
                sidecars = await sidecarExtractor
                    .ExtractAsync(plan.Source, workingFolder, cancellationToken).ConfigureAwait(false);
            }

            notes.AddRange(sidecars.Notes);
            cancellationToken.ThrowIfCancellationRequested();

            ConversionOutcome outcome;
            if (plan.ExportsThroughVideoExporter)
            {
                outcome = await ExportMasterToMp4Async(plan, sidecars, workingFolder, progress, notes, stopwatch, cancellationToken)
                    .ConfigureAwait(false);
            }
            else if (plan.Destination == MediaFormatKind.Mp4)
            {
                outcome = await ExportToMp4Async(plan, sourcePath, sidecars, progress, notes, stopwatch, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                outcome = await AuthorAsync(plan, sourcePath, sidecars, workingFolder, progress, notes, stopwatch, cancellationToken)
                    .ConfigureAwait(false);
            }

            return outcome;
        }
        catch (OperationCanceledException)
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
        }
        catch (VideoToolProcessingException exception)
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
        }
        catch (Exception exception)
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
        }
        finally
        {
            DeleteFolder(workingFolder);
        }
    }

    private static async Task<ConversionOutcome> AuthorAsync(
        ConversionPlan plan,
        string sourcePath,
        MediaSidecars sidecars,
        string workingFolder,
        IProgress<ConversionProgress> progress,
        List<string> notes,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var gerund = MediaFormats.ActionGerund(plan.Operation);
        var request = BuildAuthoringRequest(plan, sourcePath, sidecars, workingFolder, cancellationToken);

        request.ProgressCallback = report =>
        {
            var passes = Math.Max(1, report.PassCount);
            var within = (((report.PassNumber - 1) * 100d) + report.Percent) / passes;
            progress?.Report(new ConversionProgress($"{gerund} - {report.Label}", 2, 2, within));
        };

        progress?.Report(new ConversionProgress(gerund, 2, 2, 0d));

        //The authoring library is synchronous, so the pass runs on a worker thread. It watches the
        //request's own CancellationToken between its stages and through every FFmpeg pass: a Cancel
        //kills the encoder outright, deletes the part-written output and surfaces here as an
        //OperationCanceledException, which RunAsync turns into a cancelled outcome. Any other failure
        //surfaces the same way and becomes a failed outcome carrying the library's own message.
        var result = await Task.Run(() => CbvAuthor.Write(request), CancellationToken.None).ConfigureAwait(false);

        notes.AddRange(result.Notes);
        AddAudioNotes(plan, notes);
        AddAlphaNotes(plan, notes);
        AddSidecarNotes(plan, sidecars, notes);

        progress?.Report(new ConversionProgress(gerund, 2, 2, 100d));

        return ConversionOutcome.Success(
            result.OutputPath,
            result.SizeInBytes,
            stopwatch.Elapsed,
            DescribeProfile(result.Profile),
            result.PassesProfile,
            notes,
            result.Commands.Select(c => c.ToString()).ToArray());
    }

    private static VideoAuthoringRequest BuildAuthoringRequest(
        ConversionPlan plan, string sourcePath, MediaSidecars sidecars, string workingFolder, CancellationToken cancellationToken)
    {
        var request = new VideoAuthoringRequest
        {
            SourcePath = sourcePath,
            OutputPath = plan.OutputPath,
            SourceDuration = plan.Source.Duration,
            TemporaryFolder = workingFolder,
            ChaptersPath = sidecars.ChaptersPath,
            CancellationToken = cancellationToken,

            //The bespoke CBVF container is written by the muxer in the playback core - at format
            //version 1, the master form, for Mode 3 - and the other three by FFmpeg's own WebM and
            //Matroska muxers.
            Flavour = plan.Destination switch
            {
                MediaFormatKind.CodeBrixMode2 => VideoAuthoringFlavour.Bespoke,
                MediaFormatKind.CodeBrixMaster => VideoAuthoringFlavour.Master,
                _ => VideoAuthoringFlavour.WebMProfile,
            },

            Container = plan.Destination == MediaFormatKind.Matroska
                ? AuthoringContainerFormat.Matroska
                : AuthoringContainerFormat.WebM,

            //Only the three CodeBrix modes are meant to satisfy the streamable profile. A standard MKV
            //is checked and reported on, but its failures are not this application's business.
            CuesToFront = plan.Destination != MediaFormatKind.Matroska,
            ValidateProfile = true,
            FailWhenProfileFails = MediaFormats.IsCodeBrixContainer(plan.Destination),
        };

        request.Video.FrameSize = plan.IsResized
            ? AuthoringFrameSize.Exact(plan.Resolution.Width, plan.Resolution.Height)
            : AuthoringFrameSize.Source;
        request.Video.SpeedPreset = Av1SpeedPreset;
        request.Video.ConstantRateFactor = Av1RateFactor(plan.Quality);

        if (plan.Destination == MediaFormatKind.CodeBrixMaster)
        {
            //A master is encoded IN PROCESS by the AV1 encoder package, which is the reference encoder
            //FFmpeg calls libaom-av1, not SVT-AV1. The rate factor means the same 0-63 scale to it (its
            //cq-level) and the pinned preset is its cpu-used, which the library caps at 8 - exactly where
            //it is pinned already. The stops were calibrated on SVT-AV1, so they are close on a master
            //rather than identical. Alpha is carried whenever the engine finds the source has it, and the
            //alpha plane gets the library's own rate factor: six points better than the picture's.
            request.Video.Alpha = AuthoringAlphaMode.Auto;
        }

        request.Audio.Include = plan.Source.HasAudio;
        request.Audio.Codec = plan.AudioCodec switch
        {
            TargetAudioCodec.Vorbis => AuthoringAudioCodec.LibVorbis,
            TargetAudioCodec.Flac => AuthoringAudioCodec.Flac,
            _ => AuthoringAudioCodec.LibOpus,
        };

        if (plan.Source.HasAudio)
        {
            //The channel count is the plan's decision: the source's own, capped at stereo for every
            //one of the five formats this application writes, because it writes mono or stereo audio
            //only. The quality knob never touches sound.
            request.Audio.Channels = plan.AudioChannels;

            if (plan.AudioCodec == TargetAudioCodec.Flac)
            {
                //Lossless sound keeps the source's own sample rate: resampling it would be the one lossy
                //step in a lossless pass.
                request.Audio.SampleRateHz = Math.Clamp(plan.Source.AudioSampleRateHz, 8000, 192000);
                request.Audio.FlacBitDepth = FlacBitDepth;
            }
            else if (plan.AudioCodec == TargetAudioCodec.Vorbis)
            {
                //Vorbis keeps the source's own sample rate rather than resampling, and is rate-controlled
                //by QUALITY rather than by bit rate. libvorbis's bit-rate mode opens only inside a band
                //that depends on both the sample rate and the channel count - and not at all above
                //48 kHz - so a fixed bit rate is refused for a mono 22.05 kHz source or a 96 kHz one.
                //The quality path has no such band at any rate or channel count.
                request.Audio.SampleRateHz = Math.Clamp(plan.Source.AudioSampleRateHz, 8000, 192000);
                request.Audio.VorbisQuality = VorbisQuality;
            }
            else
            {
                //Opus runs at 48 kHz internally, so everything is resampled there on the way in.
                request.Audio.SampleRateHz = 48000;
            }
        }

        foreach (var caption in sidecars.Captions)
        {
            request.Captions.Add(new AuthoringCaptionInput(
                caption.Path, caption.Language, caption.Name, caption.Flags));
        }

        return request;
    }

    private static async Task<ConversionOutcome> ExportToMp4Async(
        ConversionPlan plan,
        string sourcePath,
        MediaSidecars sidecars,
        IProgress<ConversionProgress> progress,
        List<string> notes,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var gerund = MediaFormats.ActionGerund(plan.Operation);
        progress?.Report(new ConversionProgress(gerund, 2, 2, 0d));

        var arguments = FFMpegArguments.FromFileInput(sourcePath);
        foreach (var caption in sidecars.Captions)
        {
            arguments = arguments.AddFileInput(caption.Path, false);
        }

        if (sidecars.HasChapters)
        {
            arguments = arguments.AddFileInput(sidecars.ChaptersPath, false)
                .MapMetaData(sidecars.Captions.Count + 1);
        }

        var errors = new List<string>();
        var processor = arguments
            .OutputToFile(plan.OutputPath, true, options =>
            {
                options.SelectStream(0, 0, Channel.Video);
                if (plan.Source.HasAudio)
                {
                    options.SelectStream(0, 0, Channel.Audio);
                }

                for (var index = 0; index < sidecars.Captions.Count; index++)
                {
                    options.SelectStream(0, index + 1, Channel.Subtitle);
                    options.WithStreamMetadata(Channel.Subtitle, index, "language", sidecars.Captions[index].Language);
                }

                options
                    .WithVideoCodec("libx264")
                    .WithConstantRateFactor(H264RateFactor(plan.Quality))
                    .WithSpeedPreset(Speed.Medium)
                    .ForcePixelFormat("yuv420p");

                if (plan.IsResized)
                {
                    options.WithVideoFilters(filters => filters.Scale(plan.Resolution.Width, plan.Resolution.Height));
                }

                if (plan.Source.HasAudio)
                {
                    options.WithAudioCodec("aac").WithAudioBitrate(Mp4AudioKilobitsPerSecond);

                    //An .mp4 export is the one destination this application does not cap at stereo: AAC
                    //keeps the source's own layout. It is still capped at the eight channels AAC is
                    //written with here, and a source with more than that has to be told so - left to
                    //itself, FFmpeg's AAC encoder refuses the layout outright and the export fails.
                    if (plan.DownmixesAudio)
                    {
                        options.WithCustomArgument(
                            "-ac " + plan.AudioChannels.ToString(CultureInfo.InvariantCulture));
                    }
                }

                if (sidecars.Captions.Count > 0)
                {
                    //MP4's own timed-text track. Nothing else in the MP4 family carries WebVTT.
                    options.WithSubtitleCodec("mov_text");
                }

                options.WithFastStart().ForceFormat("mp4");
            })
            .NotifyOnProgress(
                percent => progress?.Report(new ConversionProgress(gerund, 2, 2, percent)),
                plan.Source.Duration)
            .NotifyOnError(errors.Add)
            .CancellableThrough(cancellationToken);

        var commands = new[] { "ffmpeg " + processor.Arguments };

        bool succeeded;
        try
        {
            succeeded = await processor.ProcessAsynchronously(false).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
        }

        if (!succeeded || !File.Exists(plan.OutputPath))
        {
            DeletePartialOutput(plan.OutputPath);
            return ConversionOutcome.Failed(
                "The MP4 export failed. " + string.Join(" ", errors.TakeLast(5)), stopwatch.Elapsed, notes);
        }

        AddAudioNotes(plan, notes);
        AddAlphaNotes(plan, notes);
        AddSidecarNotes(plan, sidecars, notes);
        progress?.Report(new ConversionProgress(gerund, 2, 2, 100d));

        return ConversionOutcome.Success(
            plan.OutputPath, new FileInfo(plan.OutputPath).Length, stopwatch.Elapsed, null, false, notes, commands);
    }

    /// <summary>
    /// A Mode 3 master out to <c>.mp4</c>: the authoring library's exporter reads the master itself,
    /// decodes the picture, the alpha plane and the FLAC sound with the decoders the APPLICATION
    /// registered (the same AV1 decoder the player uses; FLAC is built into the audio core), composites
    /// the picture over 50% grey by its alpha - an MP4 has no transparency to carry it in - and writes
    /// H.264 and AAC through the registered video-processing engine. The exporter carries no captions or
    /// chapters, so when the master has any a second, copy-only FFmpeg pass adds them, exactly as the
    /// ordinary export writes them.
    /// </summary>
    private static async Task<ConversionOutcome> ExportMasterToMp4Async(
        ConversionPlan plan,
        MediaSidecars sidecars,
        string workingFolder,
        IProgress<ConversionProgress> progress,
        List<string> notes,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var gerund = MediaFormats.ActionGerund(plan.Operation);
        progress?.Report(new ConversionProgress(gerund, 2, 2, 0d));

        var addsSidecars = sidecars.CaptionCount > 0 || sidecars.HasChapters;
        var exportPath = addsSidecars ? Path.Combine(workingFolder, "exported.mp4") : plan.OutputPath;

        var request = new VideoExportRequest
        {
            SourcePath = plan.Source.Path,
            OutputPath = exportPath,
            Codec = VideoExportCodec.H264,

            //The same quality stops as the FFmpeg export: the exporter's constant quality is x264's crf.
            Quality = VideoQuality.ConstantQuality(H264RateFactor(plan.Quality)),
            IncludeAudio = plan.Source.HasAudio,
            AudioBitrateKilobitsPerSecond = Mp4AudioKilobitsPerSecond,
            FastStart = true,
            ProgressCallback = report => progress?.Report(new ConversionProgress($"{gerund} - {report.Label}", 2, 2, report.Percent)),
            CancellationToken = cancellationToken,
        };

        //The exporter is synchronous, so it runs on a worker thread. Like the authoring library it watches
        //the request's own token, deletes its part-written output when cancelled and surfaces the cancel
        //as an OperationCanceledException, which RunAsync turns into a cancelled outcome.
        var commands = new List<string>();
        var result = await Task.Run(() =>
        {
            commands.AddRange(VideoExporter.DryRun(request).Split('\n').Select(line => "VideoExporter " + line));
            return VideoExporter.Run(request);
        }, CancellationToken.None).ConfigureAwait(false);

        notes.AddRange(addsSidecars
            ? result.Notes.Where(note => !note.StartsWith(ExporterCaptionsNoteStart, StringComparison.OrdinalIgnoreCase))
            : result.Notes);

        if (plan.Source.HasAlpha)
        {
            notes.Add("The master's alpha plane was composited over 50% grey: an MP4 has no transparency of its own.");
        }

        if (addsSidecars)
        {
            commands.Add(await AddSidecarsToMp4Async(
                exportPath, result.AudioSource != VideoExportAudioSource.None, sidecars, plan.OutputPath, cancellationToken)
                .ConfigureAwait(false));
        }

        cancellationToken.ThrowIfCancellationRequested();

        AddAudioNotes(plan, notes);
        AddSidecarNotes(plan, sidecars, notes);
        progress?.Report(new ConversionProgress(gerund, 2, 2, 100d));

        return ConversionOutcome.Success(
            plan.OutputPath, new FileInfo(plan.OutputPath).Length, stopwatch.Elapsed, null, false, notes, commands);
    }

    //Copies an exported MP4's video and sound as they are and adds the caption tracks and chapters the
    //ordinary export would have written into it: the captions as MP4 timed text, the chapters from the
    //FFmpeg metadata file.
    private static async Task<string> AddSidecarsToMp4Async(
        string exportedPath, bool hasAudio, MediaSidecars sidecars, string outputPath, CancellationToken cancellationToken)
    {
        var arguments = FFMpegArguments.FromFileInput(exportedPath);
        foreach (var caption in sidecars.Captions)
        {
            arguments = arguments.AddFileInput(caption.Path, false);
        }

        if (sidecars.HasChapters)
        {
            arguments = arguments.AddFileInput(sidecars.ChaptersPath, false)
                .MapMetaData(sidecars.Captions.Count + 1);
        }

        var errors = new List<string>();
        var processor = arguments
            .OutputToFile(outputPath, true, options =>
            {
                options.SelectStream(0, 0, Channel.Video);
                if (hasAudio)
                {
                    options.SelectStream(0, 0, Channel.Audio);
                }

                for (var index = 0; index < sidecars.Captions.Count; index++)
                {
                    options.SelectStream(0, index + 1, Channel.Subtitle);
                    options.WithStreamMetadata(Channel.Subtitle, index, "language", sidecars.Captions[index].Language);
                }

                options.WithCopyCodec();
                if (sidecars.Captions.Count > 0)
                {
                    options.WithSubtitleCodec("mov_text");
                }

                options.WithFastStart().ForceFormat("mp4");
            })
            .NotifyOnError(errors.Add)
            .CancellableThrough(cancellationToken);

        var command = "ffmpeg " + processor.Arguments;
        var succeeded = await processor.ProcessAsynchronously(false).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        if (!succeeded || !File.Exists(outputPath))
        {
            throw new VideoToolProcessingException(
                "The master was exported, but its captions and chapters could not be added to the MP4. " +
                string.Join(" ", errors.TakeLast(5)));
        }

        return command;
    }

    //THE QUALITY KNOB, IN ITS ENTIRETY. A quality stop moves the encoder's constant rate factor and
    //nothing else: the speed presets above stay pinned, and sound is settled by the destination alone.
    //
    //CALIBRATED 2026-09-02 on this host (ffmpeg 7.1.5, SVT-AV1 and x264 as they ship with it). Two
    //six-second 640 x 360 clips - FFmpeg's own testsrc2 and mandelbrot sources - were written
    //losslessly (x264 -qp 0), encoded at every candidate rate factor with the presets pinned above,
    //and each result compared with its lossless master through FFmpeg's own psnr and ssim filters.
    //Nothing was installed to measure any of it: both filters ship with FFmpeg. Both inputs were
    //re-timestamped by frame index first (settb=AVTB,setpts=N), because a one-frame slip swamps
    //everything a rate factor does. Each row is the encoded video's size, its whole-file PSNR in
    //decibels, and its SSIM.
    //
    //AV1 (SVT-AV1, scale 0 largest/best to 63 smallest/worst). "Good" is 30, the authoring library's
    //own default, stated here rather than left implied.
    //                 testsrc2 (master 2,333,654 B)        mandelbrot (master 12,023,916 B)
    //  crf 18            948,420 B  52.6 dB  0.99882          2,735,904 B  45.5 dB  0.99445   Best
    //  crf 24            820,777 B  50.4 dB  0.99823          2,230,727 B  43.8 dB  0.99341   Better
    //  crf 30            655,999 B  47.4 dB  0.99710          1,622,209 B  41.1 dB  0.99109   Good
    //  crf 36            480,285 B  43.9 dB  0.99457          1,032,966 B  38.1 dB  0.98644
    //  crf 42            292,179 B  40.5 dB  0.98922            562,121 B  35.4 dB  0.97878   Fair
    //  crf 48            172,364 B  38.3 dB  0.98446            270,066 B  33.3 dB  0.96987
    //
    //Good, Better and Best sit six rate-factor points apart - about 3 dB, which is the step at which a
    //difference is there to be seen at all - and Best stops at 18 because below it SVT-AV1 spends a
    //great deal of file for a difference this application's own player cannot show. Fair is twice that
    //step away from Good, at 42: it is the stop whose point is a much smaller file (a third of Good's
    //here), and it is deliberately the only visibly softer one.
    private static int Av1RateFactor(QualityLevel quality) => quality switch
    {
        QualityLevel.Fair => 42,
        QualityLevel.Better => 24,
        QualityLevel.Best => 18,
        _ => 30,
    };

    //H.264 (x264, scale 0 lossless to 51, speed preset medium). "Good" is 20, which is what every
    //export has been written at until now.
    //                 testsrc2 (master 2,333,654 B)        mandelbrot (master 12,023,916 B)
    //  crf 14          1,129,382 B  52.9 dB  0.99912          3,170,130 B  46.1 dB  0.99489   Best
    //  crf 17            944,857 B  50.1 dB  0.99847          2,440,194 B  43.8 dB  0.99327   Better
    //  crf 20            760,946 B  47.1 dB  0.99726          1,805,214 B  41.2 dB  0.99061   Good
    //  crf 23            594,087 B  44.1 dB  0.99512          1,288,734 B  38.7 dB  0.98657
    //  crf 26            419,377 B  41.1 dB  0.99077            850,306 B  36.2 dB  0.98018
    //  crf 27            368,352 B  40.1 dB  0.98870            718,536 B  35.4 dB  0.97726   Fair
    //  crf 28            321,412 B  39.3 dB  0.98658            596,660 B  34.6 dB  0.97398
    //  crf 32            212,542 B  36.8 dB  0.97888            261,650 B  32.0 dB  0.95793
    //
    //The stops were chosen to MATCH the AV1 ones stop for stop rather than to look tidy on x264's own
    //scale: 27/20/17/14 measure within 0.4 dB of 42/30/24/18 on both clips, so picking "Better" gives
    //the same picture whether the destination is one of the five formats or an exported .mp4. x264's
    //scale turns out to move about half as far as SVT-AV1's does in this band, which is why its steps
    //are three points where AV1's are six.
    private static int H264RateFactor(QualityLevel quality) => quality switch
    {
        QualityLevel.Fair => 27,
        QualityLevel.Better => 17,
        QualityLevel.Best => 14,
        _ => 20,
    };

    //A file that passes carries the library's own one-line verdict, "passes the profile", which is the
    //wording every tool in the family prints. A file that FAILS carries the reason instead: the
    //library's own failing line is "DOES NOT pass the profile", which says nothing about why, and the
    //place this text is shown - the operation panel, after "Streamable profile: FAIL - " - has already
    //said that it failed. So a failure names the rules it did not satisfy, and what was found when the
    //rule elaborates.
    private static string DescribeProfile(StreamableProfileReport report)
    {
        if (report is null)
        {
            return null;
        }

        if (report.Passes)
        {
            return report.Verdict;
        }

        var reasons = report.FailedRules()
            .Select(rule => string.IsNullOrWhiteSpace(rule.Detail) ? rule.Rule : $"{rule.Rule} - {rule.Detail}")
            .ToArray();

        return reasons.Length == 0 ? report.Verdict : string.Join("; ", reasons);
    }

    private static void AddAudioNotes(ConversionPlan plan, List<string> notes)
    {
        if (!plan.DownmixesAudio)
        {
            return;
        }

        notes.Add(plan.Destination == MediaFormatKind.Mp4
            ? $"Audio reduced from {plan.Source.AudioChannels} channels to {plan.AudioChannels}: an exported MP4 carries at most eight."
            : $"Audio downmixed from {plan.Source.AudioChannels} channels to stereo: this application writes mono or stereo audio only.");
    }

    private static void AddAlphaNotes(ConversionPlan plan, List<string> notes)
    {
        //A demultiplexed master says so itself, and a master exported to MP4 is composited instead.
        if (plan.DropsAlpha && !plan.RequiresMode2Extraction)
        {
            notes.Add($"The source's alpha channel was left behind: {MediaFormats.DisplayName(plan.Destination)} " +
                      "is opaque. Import to CodeBrix Mode 3 to keep it.");
        }
    }

    private static void AddSidecarNotes(ConversionPlan plan, MediaSidecars sidecars, List<string> notes)
    {
        if (sidecars.CaptionCount > 0)
        {
            notes.Add($"{sidecars.CaptionCount} caption track(s) carried across.");
        }

        if (sidecars.HasChapters)
        {
            //One title per chapter, whatever the destination: the extractor has already collapsed
            //them, and says so in a note of its own when a source carried more than one language.
            notes.Add("Chapters carried across.");
        }
    }

    private static void DeletePartialOutput(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            //A file that will not delete is not worth failing the report over.
        }
        catch (UnauthorizedAccessException)
        {
            //As above.
        }
    }

    private static void DeleteFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (IOException)
        {
            //Temporary files that will not delete are not worth failing the report over.
        }
        catch (UnauthorizedAccessException)
        {
            //As above.
        }
    }
}
