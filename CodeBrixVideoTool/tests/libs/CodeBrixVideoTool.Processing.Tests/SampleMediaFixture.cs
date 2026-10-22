using CodeBrix.VideoPlayback.Dav1d;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Enums;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Samples;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

/// <summary>
/// Generates the tiny synthetic media every media-touching test works from, once per test run, in a
/// temporary folder that is removed afterwards. Nothing is copied from any repository.
/// </summary>
public sealed class SampleMediaFixture : IAsyncLifetime
{
    public string Root { get; } = Path.Combine(
        Path.GetTempPath(), "CodeBrixVideoTool.Tests", Guid.NewGuid().ToString("N"));

    public string Mp4Path => Path.Combine(Root, "sample.mp4");

    public string CaptionsPath => Path.Combine(Root, "sample.en.vtt");

    public string ChaptersPath => Path.Combine(Root, "sample.ffmetadata");

    /// <summary>The sample MP4 with a caption track and chapters muxed in, copied not re-encoded.</summary>
    public string RichMp4Path => Path.Combine(Root, "rich.mp4");

    /// <summary>The rich MP4 imported to a bespoke CBVF file, with its captions and chapters.</summary>
    public string Mode2Path => Path.Combine(Root, "rich-mode2.cbv");

    /// <summary>The rich MP4 imported to a WebM-profile .cbv file, with its captions and chapters.</summary>
    public string Mode1Path => Path.Combine(Root, "rich-mode1.cbv");

    /// <summary>The rich MP4 imported to a Mode 3 master: opaque, FLAC sound, captions and chapters.</summary>
    public string MasterPath => Path.Combine(Root, "rich.cbvmaster");

    /// <summary>A QuickTime clip whose picture has an alpha channel - a transparent frame round the pattern.</summary>
    public string AlphaMovPath => Path.Combine(Root, "alpha.mov");

    /// <summary>The alpha clip imported to a Mode 3 master, which keeps its alpha as an alpha-plane track.</summary>
    public string AlphaMasterPath => Path.Combine(Root, "alpha.cbvmaster");

    /// <summary>What probing found in the rich MP4.</summary>
    public SourceMediaInfo RichMp4Info { get; private set; }

    /// <summary>What probing found in the imported Mode 2 file.</summary>
    public SourceMediaInfo Mode2Info { get; private set; }

    /// <summary>What probing found in the imported Mode 1 file.</summary>
    public SourceMediaInfo Mode1Info { get; private set; }

    /// <summary>What probing found in the alpha clip.</summary>
    public SourceMediaInfo AlphaMovInfo { get; private set; }

    /// <summary>What probing found in the imported, opaque Mode 3 master.</summary>
    public SourceMediaInfo MasterInfo { get; private set; }

    /// <summary>What probing found in the imported Mode 3 master with alpha.</summary>
    public SourceMediaInfo AlphaMasterInfo { get; private set; }

    public TimeSpan Duration { get; } = TimeSpan.FromSeconds(2);

    public int Width { get; } = 320;

    public int Height { get; } = 240;

    public async ValueTask InitializeAsync()
    {
        // The imports below author .cbv files, which decodes through the registered video-processing engine.
        CodeBrixVideoProcessing.Register();

        // Exporting a master to MP4 decodes it with the application's own decoders, as the player does.
        CodeBrixVideoPlaybackDav1d.Register();

        Directory.CreateDirectory(Root);

        await SampleClipFactory.WriteMp4Async(Mp4Path, Width, Height, Duration).ConfigureAwait(false);
        SampleClipFactory.WriteWebVtt(CaptionsPath, Duration);
        SampleClipFactory.WriteChapterMetadata(ChaptersPath, Duration);
        await SampleClipFactory.WriteAlphaMovAsync(AlphaMovPath, Width, Height, Duration).ConfigureAwait(false);

        await FFMpegArguments
            .FromFileInput(Mp4Path)
            .AddFileInput(CaptionsPath, false)
            .AddFileInput(ChaptersPath, false)
            .MapMetaData(2)
            .OutputToFile(RichMp4Path, true, options => options
                .SelectStream(0, 0, Channel.Video)
                .SelectStream(0, 0, Channel.Audio)
                .SelectStream(0, 1, Channel.Subtitle)
                .CopyChannel(Channel.Both)
                .WithSubtitleCodec("mov_text")
                .WithStreamMetadata(Channel.Subtitle, 0, "language", "eng")
                .ForceFormat("mp4"))
            .ProcessAsynchronously()
            .ConfigureAwait(false);

        var probe = new MediaProbe();
        var runner = new ConversionRunner();

        RichMp4Info = await probe.ProbeAsync(RichMp4Path, CancellationToken.None).ConfigureAwait(false);
        AlphaMovInfo = await probe.ProbeAsync(AlphaMovPath, CancellationToken.None).ConfigureAwait(false);

        await ImportAsync(runner, RichMp4Info, MediaFormatKind.CodeBrixMode2, Mode2Path).ConfigureAwait(false);
        await ImportAsync(runner, RichMp4Info, MediaFormatKind.CodeBrixMode1, Mode1Path).ConfigureAwait(false);
        await ImportAsync(runner, RichMp4Info, MediaFormatKind.CodeBrixMaster, MasterPath).ConfigureAwait(false);
        await ImportAsync(runner, AlphaMovInfo, MediaFormatKind.CodeBrixMaster, AlphaMasterPath).ConfigureAwait(false);

        Mode2Info = await probe.ProbeAsync(Mode2Path, CancellationToken.None).ConfigureAwait(false);
        Mode1Info = await probe.ProbeAsync(Mode1Path, CancellationToken.None).ConfigureAwait(false);
        MasterInfo = await probe.ProbeAsync(MasterPath, CancellationToken.None).ConfigureAwait(false);
        AlphaMasterInfo = await probe.ProbeAsync(AlphaMasterPath, CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task ImportAsync(
        ConversionRunner runner, SourceMediaInfo source, MediaFormatKind destination, string outputPath)
    {
        var plan = ConversionPlanner.Create(source, destination, outputPath, null);
        var outcome = await runner.RunAsync(plan, null, CancellationToken.None).ConfigureAwait(false);
        if (!outcome.Succeeded)
        {
            throw new InvalidOperationException(
                $"The test fixture could not import to {destination}: {outcome.Failure}");
        }
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, true);
            }
        }
        catch (IOException)
        {
            //A temporary folder that will not delete is not worth failing a test run over.
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>Shares one <see cref="SampleMediaFixture" /> across every test class that needs media.</summary>
[CollectionDefinition(Name)]
public sealed class SampleMediaCollection : ICollectionFixture<SampleMediaFixture>
{
    public const string Name = "sample media";
}
