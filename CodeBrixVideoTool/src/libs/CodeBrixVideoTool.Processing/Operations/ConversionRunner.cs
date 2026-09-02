using CodeBrix.VideoPlayback.Authoring;
using CodeBrix.VideoPlayback.Authoring.Captions;
using CodeBrix.VideoPlayback.Authoring.Encoding;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Enums;
using CodeBrixVideoTool.Processing.Containers;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Planning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Operations;

/// <summary>
/// Carries out one conversion: the four supported formats through the authoring library, an
/// <c>.mp4</c> export through FFmpeg, and a Mode 2 source through the demultiplexer first.
/// </summary>
/// <remarks>
/// Every conversion runs in exactly two stages. The first prepares the source - demultiplexing a
/// Mode 2 file when there is one, and lifting out the chapters and captions the authoring pass has
/// to be handed as separate inputs. The second encodes. Only the second can report a percentage,
/// because only FFmpeg knows how far through the media it has got.
/// </remarks>
public sealed class ConversionRunner : IConversionRunner
{
    //Faster than the authoring library's own default of 6, which matters a great deal for an
    //application a person is sitting in front of, and costs very little at these bit rates.
    private const int Av1SpeedPreset = 8;

    private const int Mp4ConstantRateFactor = 20;
    private const int Mp4AudioKilobitsPerSecond = 192;

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
                notes.Add("Mode 2 source demultiplexed without re-encoding: " + extraction + ".");
            }
            else if (plan.Source.CaptionTrackCount > 0 || plan.Source.ChapterCount > 0)
            {
                sidecars = await sidecarExtractor
                    .ExtractAsync(plan.Source, workingFolder, cancellationToken).ConfigureAwait(false);
            }

            notes.AddRange(sidecars.Notes);
            cancellationToken.ThrowIfCancellationRequested();

            var outcome = plan.Destination == MediaFormatKind.Mp4
                ? await ExportToMp4Async(plan, sourcePath, sidecars, progress, notes, stopwatch, cancellationToken)
                    .ConfigureAwait(false)
                : await AuthorAsync(plan, sourcePath, sidecars, workingFolder, progress, notes, stopwatch, cancellationToken)
                    .ConfigureAwait(false);

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
        var request = BuildAuthoringRequest(plan, sourcePath, sidecars, workingFolder);

        request.ProgressCallback = report =>
        {
            var passes = Math.Max(1, report.PassCount);
            var within = (((report.PassNumber - 1) * 100d) + report.Percent) / passes;
            progress?.Report(new ConversionProgress($"{gerund} - {report.Label}", 2, 2, within));
        };

        progress?.Report(new ConversionProgress(gerund, 2, 2, 0d));

        VideoAuthoringResult result = null;
        Exception failure = null;

        //The authoring library is synchronous and takes no cancellation token, so the pass runs on a
        //worker thread and is awaited to completion. See the report's findings: the only thing a
        //Cancel can do about an authoring pass is discard what it produced.
        await Task.Run(() =>
        {
            try
            {
                result = CbvAuthor.Write(request);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }, CancellationToken.None).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
        {
            DeletePartialOutput(plan.OutputPath);
            notes.Add("The encode could not be stopped part-way, so it ran to the end and its output was discarded.");
            return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
        }

        if (failure is not null)
        {
            return ConversionOutcome.Failed(failure.Message, stopwatch.Elapsed, notes);
        }

        notes.AddRange(result.Notes);
        AddSidecarNotes(plan, sidecars, notes);

        progress?.Report(new ConversionProgress(gerund, 2, 2, 100d));

        return ConversionOutcome.Success(
            result.OutputPath,
            result.SizeInBytes,
            stopwatch.Elapsed,
            result.Profile?.Verdict,
            result.PassesProfile,
            notes,
            result.Commands.Select(c => c.ToString()).ToArray());
    }

    private static VideoAuthoringRequest BuildAuthoringRequest(
        ConversionPlan plan, string sourcePath, MediaSidecars sidecars, string workingFolder)
    {
        var request = new VideoAuthoringRequest
        {
            SourcePath = sourcePath,
            OutputPath = plan.OutputPath,
            SourceDuration = plan.Source.Duration,
            TemporaryFolder = workingFolder,
            ChaptersPath = sidecars.ChaptersPath,

            //The bespoke CBVF container is written by the muxer in the playback core; the other
            //three are written by FFmpeg's own WebM and Matroska muxers.
            Flavour = plan.Destination == MediaFormatKind.CodeBrixMode2
                ? VideoAuthoringFlavour.Bespoke
                : VideoAuthoringFlavour.WebMProfile,

            Container = plan.Destination == MediaFormatKind.Matroska
                ? AuthoringContainerFormat.Matroska
                : AuthoringContainerFormat.WebM,

            //Only the two .cbv flavours are meant to satisfy the streamable profile. A standard MKV
            //is checked and reported on, but its failures are not this application's business.
            CuesToFront = plan.Destination != MediaFormatKind.Matroska,
            ValidateProfile = true,
            FailWhenProfileFails = MediaFormats.IsCodeBrixContainer(plan.Destination),
        };

        request.Video.FrameSize = plan.IsResized
            ? AuthoringFrameSize.Exact(plan.Resolution.Width, plan.Resolution.Height)
            : AuthoringFrameSize.Source;
        request.Video.SpeedPreset = Av1SpeedPreset;

        request.Audio.Include = plan.Source.HasAudio;
        request.Audio.Codec = plan.AudioCodec == TargetAudioCodec.Vorbis
            ? AuthoringAudioCodec.LibVorbis
            : AuthoringAudioCodec.LibOpus;

        if (plan.Source.HasAudio)
        {
            request.Audio.Channels = Math.Clamp(plan.Source.AudioChannels, 1, 8);
            request.Audio.SampleRateHz = plan.AudioCodec == TargetAudioCodec.Vorbis
                ? Math.Clamp(plan.Source.AudioSampleRateHz, 8000, 192000)
                : 48000;
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
                    .WithConstantRateFactor(Mp4ConstantRateFactor)
                    .WithSpeedPreset(Speed.Medium)
                    .ForcePixelFormat("yuv420p");

                if (plan.IsResized)
                {
                    options.WithVideoFilters(filters => filters.Scale(plan.Resolution.Width, plan.Resolution.Height));
                }

                if (plan.Source.HasAudio)
                {
                    options.WithAudioCodec("aac").WithAudioBitrate(Mp4AudioKilobitsPerSecond);
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

        AddSidecarNotes(plan, sidecars, notes);
        progress?.Report(new ConversionProgress(gerund, 2, 2, 100d));

        return ConversionOutcome.Success(
            plan.OutputPath, new FileInfo(plan.OutputPath).Length, stopwatch.Elapsed, null, false, notes, commands);
    }

    private static void AddSidecarNotes(ConversionPlan plan, MediaSidecars sidecars, List<string> notes)
    {
        if (sidecars.CaptionCount > 0)
        {
            notes.Add($"{sidecars.CaptionCount} caption track(s) carried across.");
        }

        if (sidecars.HasChapters)
        {
            notes.Add(plan.Destination == MediaFormatKind.CodeBrixMode2
                ? "Chapters carried across, with every language title kept."
                : "Chapters carried across.");
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
