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

    public string Av1IvfPath => Path.Combine(Root, "sample.ivf");

    public string VorbisOggPath => Path.Combine(Root, "sample.ogg");

    /// <summary>The sample MP4 with a caption track and chapters muxed in, copied not re-encoded.</summary>
    public string RichMp4Path => Path.Combine(Root, "rich.mp4");

    /// <summary>The rich MP4 imported to a bespoke CBVF file, with its captions and chapters.</summary>
    public string Mode2Path => Path.Combine(Root, "rich-mode2.cbv");

    /// <summary>The rich MP4 imported to a WebM-profile .cbv file, with its captions and chapters.</summary>
    public string Mode1Path => Path.Combine(Root, "rich-mode1.cbv");

    /// <summary>What probing found in the rich MP4.</summary>
    public SourceMediaInfo RichMp4Info { get; private set; }

    /// <summary>What probing found in the imported Mode 2 file.</summary>
    public SourceMediaInfo Mode2Info { get; private set; }

    /// <summary>What probing found in the imported Mode 1 file.</summary>
    public SourceMediaInfo Mode1Info { get; private set; }

    public TimeSpan Duration { get; } = TimeSpan.FromSeconds(2);

    public int Width { get; } = 320;

    public int Height { get; } = 240;

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(Root);

        await SampleClipFactory.WriteMp4Async(Mp4Path, Width, Height, Duration).ConfigureAwait(false);
        SampleClipFactory.WriteWebVtt(CaptionsPath, Duration);
        SampleClipFactory.WriteChapterMetadata(ChaptersPath, Duration);

        await FFMpegArguments
            .FromFileInput(Mp4Path)
            .OutputToFile(Av1IvfPath, true, options => options
                .DisableChannel(Channel.Audio)
                .WithVideoCodec("libsvtav1")
                .WithSpeedPreset(12)
                .WithConstantRateFactor(50)
                .ForcePixelFormat("yuv420p")
                .ForceFormat("ivf"))
            .ProcessAsynchronously()
            .ConfigureAwait(false);

        await FFMpegArguments
            .FromFileInput(Mp4Path)
            .OutputToFile(VorbisOggPath, true, options => options
                .DisableChannel(Channel.Video)
                .WithAudioCodec("libvorbis")
                .WithAudioBitrate(96)
                .ForceFormat("ogg"))
            .ProcessAsynchronously()
            .ConfigureAwait(false);

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

        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode2, Mode2Path).ConfigureAwait(false);
        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode1, Mode1Path).ConfigureAwait(false);

        Mode2Info = await probe.ProbeAsync(Mode2Path, CancellationToken.None).ConfigureAwait(false);
        Mode1Info = await probe.ProbeAsync(Mode1Path, CancellationToken.None).ConfigureAwait(false);
    }

    private async Task ImportAsync(
        MediaProbe probe, ConversionRunner runner, MediaFormatKind destination, string outputPath)
    {
        var plan = ConversionPlanner.Create(RichMp4Info, destination, outputPath, null);
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
