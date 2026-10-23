using CodeBrix.VideoPlayback.Color.Luts;
using SilverAssertions;
using SimpleCbxVideoPlayer.SkiaVideo;
using SimpleCbxVideoPlayer.SkiaVideo.Assets;
using SimpleCbxVideoPlayer.SkiaVideo.Effects;
using SimpleCbxVideoPlayer.SkiaVideo.Playback;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace SimpleCbxVideoPlayer.SkiaVideo.Tests;

public class VideoPlaybackControllerTests
{
    [Fact]
    public void with_no_graphics_context_GpuAuto_settles_on_the_processor()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Act
        VideoRenderBackendOption backend = controller.ResolveRenderPath();

        //Assert
        controller.RenderPath.Should().Be(VideoRenderPathOption.GpuAuto);
        controller.HasGraphicsContext.Should().BeFalse();
        backend.Should().Be(VideoRenderBackendOption.Cpu);
        controller.LastError.Should().Be(string.Empty);
    }

    [Fact]
    public void setting_the_render_path_announces_where_the_picture_is_composed()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        List<VideoRenderPathStatusEventArgs> announced = [];
        controller.RenderPathChanged += (_, args) => announced.Add(args);

        //Act
        controller.RenderPath = VideoRenderPathOption.Cpu;

        //Assert
        announced.Count.Should().BeGreaterThan(0);
        announced[^1].Backend.Should().Be(VideoRenderBackendOption.Cpu);
        announced[^1].EffectsActive.Should().BeFalse();
    }

    [Fact]
    public void GpuNoFallback_with_no_graphics_context_reports_the_reason_word_for_word()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        List<string> messages = [];
        controller.Failed += (_, args) => messages.Add(args.Message);

        //Act
        controller.RenderPath = VideoRenderPathOption.GpuNoFallback;

        //Assert
        messages.Count.Should().Be(1);
        messages[0].Should().Contain("GpuNoFallback");
        messages[0].Should().Contain("no graphics context");
        controller.LastError.Should().Be(messages[0]);
        controller.ActiveRenderPath.Should().Be(VideoRenderBackendOption.Cpu);
    }

    [Fact]
    public void the_same_failure_is_reported_once_rather_than_on_every_paint()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        controller.RenderPath = VideoRenderPathOption.GpuNoFallback;
        var reported = 0;
        controller.Failed += (_, _) => reported++;

        //Act
        controller.ResolveRenderPath();
        controller.ResolveRenderPath();

        //Assert
        reported.Should().Be(0);
    }

    [Fact]
    public void effects_are_not_applied_on_the_processor_path()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("luts");
        var cube = temp.CreateCube(folder, "one.cube", "One");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        controller.RenderPath = VideoRenderPathOption.Cpu;

        //Act
        var applied = controller.ApplyLutChain([new LutChainEntry(cube, 40)]);

        //Assert
        applied.Should().BeTrue();
        controller.LutEntries.Count.Should().Be(1);
        controller.EffectsActive.Should().BeFalse();
    }

    [Fact]
    public void ApplyLutChain_rebuilds_only_when_the_chain_actually_changes()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("luts");
        var cube = temp.CreateCube(folder, "one.cube", "One");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Act
        var first = controller.ApplyLutChain([new LutChainEntry(cube, 40)]);
        var repeat = controller.ApplyLutChain([new LutChainEntry(cube, 40)]);
        var louder = controller.ApplyLutChain([new LutChainEntry(cube, 41)]);
        var cleared = controller.ApplyLutChain([]);

        //Assert
        first.Should().BeTrue();
        repeat.Should().BeFalse();
        louder.Should().BeTrue();
        cleared.Should().BeTrue();
        controller.LutEntries.Count.Should().Be(0);
    }

    [Fact]
    public void the_transport_reads_stopped_until_something_is_playing()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Assert
        controller.TransportState.Should().Be(VideoTransportState.Stopped);
    }

    [Fact]
    public void GetChainTitle_names_every_table_and_its_percentage()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("luts");
        var first = temp.CreateCube(folder, "sepia_33.cube", "Sepia 33");
        var second = temp.CreateCube(folder, "cool_33.cube", "Cool 33");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Act
        controller.ApplyLutChain([new LutChainEntry(first, 40), new LutChainEntry(second, 65)]);

        //Assert
        controller.GetChainTitle().Should().Be("SimpleCbxVideoPlayer: sepia_33@40 + cool_33@65");
    }

    [Fact]
    public void GetChainTitle_of_an_empty_chain_is_just_the_application()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Assert
        controller.GetChainTitle().Should().Be(VideoPlaybackController.ChainTitlePrefix);
    }

    [Fact]
    public void BakeChain_writes_a_cube_file_that_reads_back()
    {
        //Arrange - NOTHING is applied to the presenter; the chain is handed straight to the bake
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("luts");
        var cube = temp.CreateCube(folder, "sepia_33.cube", "Sepia 33");
        var baked = Path.Combine(temp.Path, "baked", "chain.cube");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);

        //Act
        BakedLut result = controller.BakeChain([new LutChainEntry(cube, 40)], baked);

        //Assert
        result.Should().NotBeNull();
        result.TableCount.Should().Be(1);
        result.Title.Should().Be("SimpleCbxVideoPlayer: sepia_33@40");
        File.Exists(baked).Should().BeTrue();

        CubeLut readBack = CubeLutFile.ReadFile(baked);
        readBack.Title.Should().Be(result.Title);
        readBack.IsThreeDimensional.Should().BeTrue();
        readBack.Lut3D.Size.Should().Be(result.Size);
    }

    [Fact]
    public void BakeChain_with_no_chain_reports_it_and_writes_nothing()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        var baked = Path.Combine(temp.Path, "chain.cube");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        List<string> messages = [];
        controller.Failed += (_, args) => messages.Add(args.Message);

        //Act
        BakedLut result = controller.BakeChain([], baked);

        //Assert
        result.Should().BeNull();
        File.Exists(baked).Should().BeFalse();
        messages.Count.Should().Be(1);
        messages[0].Should().Contain("no lookup table is ticked");
    }

    [Fact]
    public void BakeChain_bakes_what_it_is_given_and_never_the_applied_chain()
    {
        //Arrange - the presenter is holding one table while a DIFFERENT one is handed to the bake
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("luts");
        var applied = temp.CreateCube(folder, "sepia_33.cube", "Sepia 33");
        var ticked = temp.CreateCube(folder, "cool_33.cube", "Cool 33");
        var baked = Path.Combine(temp.Path, "chain.cube");
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        controller.ApplyLutChain([new LutChainEntry(applied, 40)]);

        //Act
        BakedLut result = controller.BakeChain([new LutChainEntry(ticked, 65)], baked);

        //Assert - the file is the TICKED chain, and the applied chain is left exactly as it was
        result.Should().NotBeNull();
        result.Title.Should().Be("SimpleCbxVideoPlayer: cool_33@65");
        CubeLutFile.ReadFile(baked).Title.Should().Be("SimpleCbxVideoPlayer: cool_33@65");
        controller.LutEntries.Count.Should().Be(1);
        controller.LutEntries[0].FilePath.Should().Be(applied);
    }

    [Fact]
    public void Open_of_a_file_that_is_not_there_reports_it_and_opens_nothing()
    {
        //Arrange
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        List<string> messages = [];
        controller.Failed += (_, args) => messages.Add(args.Message);

        //Act
        var opened = controller.Open("/no/such/file.mkv");

        //Assert
        opened.Should().BeFalse();
        controller.IsOpen.Should().BeFalse();
        controller.CurrentFilePath.Should().BeNull();
        messages.Count.Should().Be(1);
        messages[0].Should().Contain("/no/such/file.mkv");
    }

    [Theory]
    [InlineData("landscape_hd_alpha.cbvmaster", true)]
    [InlineData("landscape_hd.cbvmaster", false)]
    public void a_master_reports_whether_it_has_alpha_and_draws_a_checkerboard_only_behind_one(string fileName, bool alpha)
    {
        //Arrange
        var root = SampleAssets.FindAssetsRoot();
        Assert.SkipWhen(root == null, "This test reads the application's own corpus, which is not beside the test assembly.");
        SkiaVideoRuntime.Initialize();
        var path = Path.Combine(SampleAssets.GetAuthoringFolder(root), "CodeBrix-Mode3", fileName);
        using VideoPlaybackController controller = new VideoPlaybackController(playAudio: false);
        controller.RenderPath = VideoRenderPathOption.Cpu;
        using SKBitmap bitmap = new SKBitmap(new SKImageInfo(1920, 1080, SKColorType.Bgra8888, SKAlphaType.Premul));
        using SKCanvas canvas = new SKCanvas(bitmap);

        //Act - the paused session decodes its first frame ahead; draw until it has been composed.
        var opened = controller.Open(path);
        var deadline = Stopwatch.StartNew();
        do
        {
            canvas.Clear(SKColors.Black);
            controller.Draw(canvas, SKRect.Create(0, 0, 1920, 1080));
            canvas.Flush();
            if (controller.DisplayWidth > 0 && bitmap.GetPixel(960, 540) != SKColors.Black) { break; }
            Thread.Sleep(50);
        } while (deadline.Elapsed < TimeSpan.FromSeconds(20));

        //Assert - the matte leaves the picture's corners fully transparent and its middle opaque.
        opened.Should().BeTrue(controller.LastError);
        controller.HasAlpha.Should().Be(alpha);
        controller.DisplayWidth.Should().Be(1920);
        var corner = bitmap.GetPixel(4, 4);
        if (alpha)
        {
            //The alpha plane is lossy AV1, so "fully transparent" may decode a level or two above zero.
            Near(corner, TransparencyBackdrop.LightColor).Should().BeTrue(corner.ToString());
            var next = bitmap.GetPixel(4 + (int)TransparencyBackdrop.CellSize, 4);
            Near(next, TransparencyBackdrop.DarkColor).Should().BeTrue(next.ToString());
        }
        else
        {
            corner.Should().NotBe(TransparencyBackdrop.LightColor);
        }

        bitmap.GetPixel(960, 540).Should().NotBe(SKColors.Black);
    }

    private static bool Near(SKColor actual, SKColor expected) =>
        Math.Abs(actual.Red - expected.Red) <= 6
        && Math.Abs(actual.Green - expected.Green) <= 6
        && Math.Abs(actual.Blue - expected.Blue) <= 6;
}
