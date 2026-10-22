using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using System.Collections.Generic;

namespace CodeBrixVideoTool.Processing.Planning;

/// <summary>
/// One settled conversion: what is being converted, into what, at what size, with which codecs, and
/// by which route.
/// </summary>
public sealed class ConversionPlan
{
    /// <summary>Creates the plan. Use <see cref="ConversionPlanner" /> rather than this directly.</summary>
    /// <param name="source">What probing found in the file being converted.</param>
    /// <param name="destination">The format being written.</param>
    /// <param name="outputPath">Where the result goes.</param>
    /// <param name="resolution">The rung of the resolution ladder that was chosen.</param>
    /// <param name="quality">The quality stop that was chosen.</param>
    /// <param name="operation">What the conversion is called.</param>
    /// <param name="steps">The route, in a sentence per step.</param>
    public ConversionPlan(
        SourceMediaInfo source,
        MediaFormatKind destination,
        string outputPath,
        ResolutionOption resolution,
        QualityLevel quality,
        ConversionOperationKind operation,
        IReadOnlyList<string> steps)
    {
        Source = source;
        Destination = destination;
        OutputPath = outputPath;
        Resolution = resolution;
        Quality = quality;
        Operation = operation;
        Steps = steps ?? [];
    }

    /// <summary>What probing found in the file being converted.</summary>
    public SourceMediaInfo Source { get; }

    /// <summary>The format being written.</summary>
    public MediaFormatKind Destination { get; }

    /// <summary>Where the result goes.</summary>
    public string OutputPath { get; }

    /// <summary>The rung of the resolution ladder that was chosen.</summary>
    public ResolutionOption Resolution { get; }

    /// <summary>
    /// The quality stop that was chosen. It settles the encoder's constant rate factor and nothing
    /// else - see <see cref="Operations.ConversionRunner" />.
    /// </summary>
    public QualityLevel Quality { get; }

    /// <summary>What the conversion is called: Import, Transcode or Export.</summary>
    public ConversionOperationKind Operation { get; }

    /// <summary>The route, in a sentence per step, for the status bar and the run notes.</summary>
    public IReadOnlyList<string> Steps { get; }

    /// <summary>The audio codec the destination is written with, chosen from the destination alone.</summary>
    public TargetAudioCodec AudioCodec => MediaFormats.AudioCodecFor(Destination);

    /// <summary>
    /// How many audio channels the destination is written with: the source's own count, capped at what
    /// this application writes to that destination. See <see cref="MediaFormats.AudioChannelsFor" />.
    /// </summary>
    public int AudioChannels => MediaFormats.AudioChannelsFor(Destination, Source.AudioChannels);

    /// <summary>True when the destination carries fewer audio channels than the source does.</summary>
    public bool DownmixesAudio => Source.HasAudio && AudioChannels < Source.AudioChannels;

    /// <summary>The video codec the destination is written with, chosen from the destination alone.</summary>
    public TargetVideoCodec VideoCodec => MediaFormats.VideoCodecFor(Destination);

    /// <summary>
    /// True when the source is a bespoke CBVF file - Mode 2, or a Mode 3 master going anywhere but
    /// <c>.mp4</c> - which FFmpeg cannot open and which therefore has to be demultiplexed and re-wrapped
    /// before anything else can happen.
    /// </summary>
    public bool RequiresMode2Extraction =>
        MediaFormats.IsBespokeContainer(Source.Format) && !ExportsThroughVideoExporter;

    /// <summary>
    /// True when a Mode 3 master is going out to <c>.mp4</c>. That runs through the authoring library's
    /// exporter, which reads the master itself, decodes its picture, alpha plane and FLAC sound with the
    /// decoders the application registered, and composites the picture over a backdrop - because an MP4
    /// has no transparency of its own.
    /// </summary>
    public bool ExportsThroughVideoExporter =>
        Source.Format == MediaFormatKind.CodeBrixMaster && Destination == MediaFormatKind.Mp4;

    /// <summary>True when the destination keeps the source's alpha channel: a master written from a source with one.</summary>
    public bool KeepsAlpha => Source.HasAlpha && MediaFormats.CarriesAlpha(Destination);

    /// <summary>
    /// True when the source has an alpha channel the destination cannot carry and nothing composites:
    /// the picture is written opaque. A master exported to <c>.mp4</c> is composited instead, so it is
    /// not counted here.
    /// </summary>
    public bool DropsAlpha => Source.HasAlpha && !MediaFormats.CarriesAlpha(Destination) && !ExportsThroughVideoExporter;

    /// <summary>True when the size is being reduced rather than kept.</summary>
    public bool IsResized => Resolution is { IsOriginal: false };

    /// <summary>The verb for the action button: "Import", "Transcode" or "Export".</summary>
    public string ActionVerb => MediaFormats.ActionVerb(Operation);

    /// <summary>A one-line summary for the status bar.</summary>
    /// <returns>What is about to happen.</returns>
    public override string ToString() =>
        $"{ActionVerb} {Source.FileName} to {MediaFormats.DisplayName(Destination)} at {Resolution.Width}x{Resolution.Height}";
}
