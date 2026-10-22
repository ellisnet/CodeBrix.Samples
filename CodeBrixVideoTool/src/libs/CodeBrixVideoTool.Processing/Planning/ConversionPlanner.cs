using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeBrixVideoTool.Processing.Planning;

/// <summary>Settles what a conversion will do before any of it is done.</summary>
public static class ConversionPlanner
{
    /// <summary>Builds the plan for one conversion.</summary>
    /// <param name="source">What probing found in the file being converted.</param>
    /// <param name="destination">The format to write.</param>
    /// <param name="outputPath">Where the result goes.</param>
    /// <param name="resolution">
    /// The rung of the resolution ladder to use, or null to keep the source's own size.
    /// </param>
    /// <param name="quality">
    /// The quality stop to encode at. <see cref="QualityLevel.Good" /> when it is not stated, which is
    /// what the application has always written.
    /// </param>
    /// <returns>The settled plan.</returns>
    /// <exception cref="ArgumentNullException">The source is null.</exception>
    /// <exception cref="VideoToolProcessingException">
    /// The conversion is not one this application offers, the output would overwrite the source, or a
    /// smaller size was asked of a conversion that keeps its source's size.
    /// </exception>
    public static ConversionPlan Create(
        SourceMediaInfo source,
        MediaFormatKind destination,
        string outputPath,
        ResolutionOption resolution,
        QualityLevel quality = QualityLevel.Good)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new VideoToolProcessingException("A conversion needs somewhere to put its result.");
        }

        if (source.Format == destination)
        {
            throw new VideoToolProcessingException(
                $"'{source.FileName}' is already {MediaFormats.DisplayName(destination)}, so there is nothing to convert.");
        }

        ConversionOperationKind operation;
        try
        {
            operation = MediaFormats.OperationFor(source.Format, destination);
        }
        catch (ArgumentException exception)
        {
            throw new VideoToolProcessingException(exception.Message, exception);
        }

        if (PathsMatch(source.Path, outputPath))
        {
            throw new VideoToolProcessingException("A conversion cannot write over the file it is reading.");
        }

        if (resolution is { IsOriginal: false } && !MediaFormats.CanResize(source.Format, destination))
        {
            throw new VideoToolProcessingException(
                $"{MediaFormats.DisplayName(source.Format)} is exported to {MediaFormats.DisplayName(destination)} " +
                "at its own size, so a smaller size cannot be chosen for it.");
        }

        var chosen = resolution ?? ResolutionOption.Original(
            ResolutionLadder.MakeEven(source.Width), ResolutionLadder.MakeEven(source.Height));

        return new ConversionPlan(source, destination, outputPath, chosen, quality, operation,
            DescribeSteps(source, destination, operation, chosen, quality));
    }

    /// <summary>
    /// A sensible file name for the result: the source's name, the destination's own suffix when the
    /// two would otherwise collide, and the destination's extension.
    /// </summary>
    /// <param name="source">The file being converted.</param>
    /// <param name="destination">The format to write.</param>
    /// <returns>A bare file name.</returns>
    /// <exception cref="ArgumentNullException">The source is null.</exception>
    public static string SuggestOutputFileName(SourceMediaInfo source, MediaFormatKind destination)
    {
        ArgumentNullException.ThrowIfNull(source);

        var stem = Path.GetFileNameWithoutExtension(source.FileName);
        var suffix = destination switch
        {
            MediaFormatKind.CodeBrixMode1 => "-mode1",
            MediaFormatKind.CodeBrixMode2 => "-mode2",
            _ => string.Empty,
        };

        return stem + suffix + MediaFormats.Extension(destination);
    }

    private static IReadOnlyList<string> DescribeSteps(
        SourceMediaInfo source,
        MediaFormatKind destination,
        ConversionOperationKind operation,
        ResolutionOption resolution,
        QualityLevel quality)
    {
        var steps = new List<string>();
        var masterExport = source.Format == MediaFormatKind.CodeBrixMaster && destination == MediaFormatKind.Mp4;

        if (MediaFormats.IsBespokeContainer(source.Format) && !masterExport)
        {
            steps.Add(source.Format == MediaFormatKind.CodeBrixMaster
                ? "Demultiplex the bespoke CBVF master, re-wrap its AV1 picture in IVF and write its FLAC " +
                  "sound out as a FLAC stream, and mux those into one intermediate file without re-encoding."
                : "Demultiplex the bespoke CBVF container, re-wrap its AV1 stream in IVF and its " +
                  "audio stream in Ogg, and mux those into one intermediate file without re-encoding.");
        }

        if (source.CaptionTrackCount > 0 || source.ChapterCount > 0)
        {
            steps.Add($"Lift out {source.CaptionTrackCount} caption track(s) and {source.ChapterCount} chapter(s) " +
                      "so the conversion can carry them across.");
        }

        var size = resolution.IsOriginal
            ? "at its own size"
            : $"scaled to {resolution.Width} x {resolution.Height}";

        var audioCodec = MediaFormats.AudioCodecFor(destination);

        //Every destination but the .mp4 export is capped at stereo, whichever codec it carries: this
        //application writes mono or stereo audio only. Nothing is ever upmixed, so a mono source stays
        //mono and a stereo one is never called a downmix.
        var sound = source.HasAudio && MediaFormats.AudioChannelsFor(destination, source.AudioChannels) < source.AudioChannels
            ? $"{audioCodec} audio downmixed to stereo (this application writes mono or stereo audio only)"
            : $"{audioCodec} audio";

        if (masterExport)
        {
            //The exporter decodes the master with the application's own decoders and composites it, so
            //this is one pass with no intermediate; the captions and chapters go in afterwards.
            steps.Add(source.HasAlpha
                ? $"Decode the master, composite its picture over 50% grey by its alpha plane, and encode " +
                  $"H.264 video {size} and AAC audio into an MP4 file, at {quality} quality."
                : $"Decode the master and encode H.264 video {size} and AAC audio into an MP4 file, at {quality} quality.");

            if (source.CaptionTrackCount > 0 || source.ChapterCount > 0)
            {
                steps.Add("Add the caption tracks and chapters to the MP4 without re-encoding.");
            }
        }
        else if (destination == MediaFormatKind.Mp4)
        {
            steps.Add($"Encode H.264 video {size} and AAC audio into an MP4 file, at {quality} quality.");
        }
        else if (destination == MediaFormatKind.CodeBrixMaster)
        {
            var picture = source.HasAlpha
                ? $"AV1 video {size} with its alpha channel as an alpha-plane track"
                : $"AV1 video {size} (with an alpha-plane track if the source turns out to carry one)";
            var lossless = source.HasAudio ? $" and lossless {sound}" : string.Empty;
            steps.Add($"Encode {picture}{lossless} into {MediaFormats.DisplayName(destination)}, at {quality} quality.");
        }
        else
        {
            steps.Add($"Encode AV1 video {size} and {sound} into {MediaFormats.DisplayName(destination)}, at {quality} quality.");
        }

        if (source.HasAlpha && !MediaFormats.CarriesAlpha(destination) && !masterExport)
        {
            steps.Add($"Leave the alpha channel behind: {MediaFormats.DisplayName(destination)} is opaque.");
        }

        if (MediaFormats.IsCodeBrixContainer(destination))
        {
            steps.Add("Check the result against the streamable profile.");
        }

        steps.Add($"({operation} complete.)");
        return steps;
    }

    private static bool PathsMatch(string first, string second)
    {
        try
        {
            return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.Ordinal);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
