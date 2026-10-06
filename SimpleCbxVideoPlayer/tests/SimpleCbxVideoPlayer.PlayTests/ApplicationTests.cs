using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using SilverAssertions;
using SimpleCbxVideoPlayer.SkiaVideo.Assets;
using SimpleCbxVideoPlayer.SkiaVideo.Diagnostics;
using SimpleCbxVideoPlayer.SkiaVideo.Playback;
using SimpleCbxVideoPlayer.ViewModels;
using SimpleCbxVideoPlayer.Views;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using Xunit;

namespace SimpleCbxVideoPlayer.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string LandscapeClip = "CodeBrix-Mode1/landscape_720p.cbv";
    private const string PortraitClip = "CodeBrix-Mode1/portrait_720p.cbv";
    private const string CpuStatus = "Render path: CPU · effects off · CPU canvas";
    private Locator Clock => Page.GetByTestId("TimeText");
    private Locator Message => Page.GetByTestId("MessageText");

    private async Task ChooseAsync(string list, string option)
    {
        await Page.GetByTestId(list).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId(list)).ToHaveValueAsync(option);
    }

    // The clip list materializes only the rows near its viewport, so the open drop-down is walked with the
    // arrow keys, which keep the highlighted row in view, and Enter commits the choice.
    private async Task ChooseClipAsync(string clip)
    {
        var from = await Page.EvaluateAsync(() => Fixture.Model.Videos.IndexOf(Fixture.Model.SelectedVideo));
        var to = await Page.EvaluateAsync(() => Fixture.Model.Videos.ToList().FindIndex(video => video.DisplayName == clip));
        to.Should().BeGreaterThanOrEqualTo(0);
        await Page.GetByTestId("VideoList").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Option).First).ToBeVisibleAsync();
        for (var step = 0; step < Math.Abs(to - from); step++)
        {
            await Page.Keyboard.PressAsync(to > from ? "ArrowDown" : "ArrowUp");
        }
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.GetByTestId("VideoList")).ToHaveValueAsync(clip);
    }

    // Plays a 720p clip from the start until the clock has passed the given point.
    private async Task PlayUntilAsync(string clip, double seconds)
    {
        await ChooseClipAsync(clip);
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Player.Position.TotalSeconds,
            position => position >= seconds, timeout: 30000, description: $"playback past {seconds} s");
    }

    // Pauses, then waits until the paused clock and the paused frame are what the page shows.
    private async Task<double> PauseAsync()
    {
        await Button("Pause").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Player.TransportState,
            state => state == VideoTransportState.Paused, description: "the paused transport");
        var position = await Page.EvaluateAsync(() => Fixture.Player.Position.TotalSeconds);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.PositionSeconds,
            shown => Math.Abs(shown - position) < 0.001, description: "the paused clock");
        await FramePaintedAsync();
        return position;
    }

    // Waits until the canvas has painted the frame the paused player is showing.
    private async Task FramePaintedAsync()
    {
        var showing = await Page.EvaluateAsync(() => Fixture.Player.CurrentTimestamp);
        await Fixture.Application.WaitForAsync(() => Fixture.PaintedTimestamp,
            painted => painted == showing, description: "the painted frame");
    }

    // One scrub-bar key, then the frame the paused player seeked to, within a frame or so of the target.
    private async Task SeekWithKeyAsync(string key, double seconds)
    {
        await Page.GetByTestId("ScrubBar").PressAsync(key);
        await Fixture.Application.WaitForAsync(() => Fixture.Player.CurrentTimestamp.TotalSeconds,
            timestamp => Math.Abs(timestamp - seconds) < 0.05, description: $"the frame at {seconds} s");
        await FramePaintedAsync();
    }

    [Fact]
    public async Task Startup_lists_the_bundled_corpus_and_selects_the_first_clip()
    {
        var root = SampleAssets.FindAssetsRoot();
        root.Should().NotBeNull();
        var videos = VideoCorpus.Scan(SampleAssets.GetAuthoringFolder(root));
        var luts = LutCatalog.Scan(SampleAssets.GetLutsFolder(root));
        videos.Count.Should().BeGreaterThan(0);
        await Expect(Page.GetByTestId("CorpusText"))
            .ToHaveTextAsync($"{videos.Count} videos and {luts.Count} lookup tables from {root}");
        await Expect(Page.GetByTestId("VideoList")).ToHaveValueAsync(videos[0].DisplayName);
        (await Page.EvaluateAsync(() => Fixture.Model.Videos.Select(video => video.FullPath).ToList()))
            .Should().Equal(videos.Select(video => video.FullPath));
        (await Page.EvaluateAsync(() => Fixture.Model.Luts.Count)).Should().Be(luts.Count);
        await Expect(Clock).ToHaveTextAsync("00:00 / 00:01");
        await Expect(Message).ToBeHiddenAsync();
        await SnapshotAsync("SimpleCbxVideoPlayer-startup");
    }

    [Fact]
    public async Task Render_path_settles_on_the_cpu_canvas()
    {
        await Expect(Page.GetByTestId("RenderPathText")).ToHaveTextAsync(CpuStatus);
        await Expect(Page.GetByTestId("RenderPathList")).ToHaveValueAsync("GPU (auto)");
        (await Page.EvaluateAsync(() => Fixture.Model.IsGpuCanvasAvailable)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Player.ActiveRenderPath)).Should().Be(VideoRenderBackendOption.Cpu);
        await Expect(Page.GetByType<SKXamlCanvas>()).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Render_path_reads_settling_until_the_page_reports_its_canvas()
    {
        // A page of the test's own, watched from the moment it is built: the fixture's page has already settled.
        var texts = new List<string>();
        MainViewModel model = null;
        await Page.SetContentAsync(() =>
        {
            var page = new MainPage();
            model = (MainViewModel)page.DataContext;
            texts.Add(model.RenderPathText);
            model.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.RenderPathText)) texts.Add(model.RenderPathText);
            };
            return page;
        });

        await Fixture.Application.WaitForAsync(() => model.RenderPathText, text => text == CpuStatus, description: "the settled render path");
        var seen = await Page.EvaluateAsync(() => texts.ToArray());
        seen.First().Should().Be("Render path: settling…");
        seen.Last().Should().Be(CpuStatus);
        // Nothing names a canvas before the page has reported which one it settled on.
        seen.Distinct().Count().Should().Be(2);
        await Expect(Page.GetByTestId("RenderPathText")).ToHaveTextAsync(CpuStatus);
    }

    [Fact]
    public async Task Lut_panel_and_bake_are_disabled_on_the_processor_path()
    {
        await Expect(Page.GetByTestId("LutPanelNote")).ToHaveTextAsync(LutPanelPolicy.CpuNote);
        await Expect(Page.GetByTestId("LutSummaryText")).ToHaveTextAsync("No lookup tables applied");
        var first = await Page.EvaluateAsync(() => Fixture.Model.Luts[0].DisplayName);
        await Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = first, Exact = true })).ToBeDisabledAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = first, Exact = true })).ToBeDisabledAsync();
        await Expect(Button("Bake chain to .cube")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("BakeStatusText")).ToBeHiddenAsync();
        await PlayUntilAsync(LandscapeClip, 0.5);
        await Expect(Page.GetByTestId("LutPanelNote")).ToHaveTextAsync(LutPanelPolicy.CpuNote);
        await Expect(Button("Bake chain to .cube")).ToBeDisabledAsync();
        await Button("Stop").ClickAsync();
    }

    [Fact]
    public async Task Gpu_only_choice_reports_the_failure_in_the_message_line()
    {
        await Expect(Message).ToBeHiddenAsync();
        await ChooseAsync("RenderPathList", "GPU only (no fallback)");
        await Expect(Message).ToBeVisibleAsync();
        var reported = await Page.EvaluateAsync(() => Fixture.Player.LastError);
        reported.Should().NotBeNullOrWhiteSpace();
        await Expect(Message).ToHaveTextAsync(reported);
        await Expect(Message).ToContainTextAsync("GpuNoFallback");
        await Expect(Page.GetByTestId("RenderPathText")).ToHaveTextAsync(CpuStatus);
        await SnapshotAsync("SimpleCbxVideoPlayer-gpu-only");
    }

    [Theory]
    [InlineData("GPU (auto)")]
    [InlineData("CPU")]
    public async Task Leaving_gpu_only_clears_its_failure_message(string choice)
    {
        await ChooseAsync("RenderPathList", "GPU only (no fallback)");
        await Expect(Message).ToContainTextAsync("GpuNoFallback");

        await ChooseAsync("RenderPathList", choice);
        await Expect(Message).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.MessageText)).Should().BeEmpty();
        await Expect(Page.GetByTestId("RenderPathText")).ToHaveTextAsync(CpuStatus);

        await ChooseAsync("RenderPathList", "GPU only (no fallback)");
        await Expect(Message).ToContainTextAsync("GpuNoFallback");
    }

    [Fact]
    public async Task Cpu_choice_keeps_effects_off()
    {
        await ChooseAsync("RenderPathList", "CPU");
        (await Page.EvaluateAsync(() => Fixture.Player.RenderPath)).Should().Be(VideoRenderPathOption.Cpu);
        await Expect(Page.GetByTestId("RenderPathText")).ToHaveTextAsync(CpuStatus);
        await Expect(Page.GetByTestId("LutPanelNote")).ToHaveTextAsync(LutPanelPolicy.CpuNote);
        await Expect(Message).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Play_opens_a_720p_clip_and_reports_its_duration()
    {
        await PlayUntilAsync(LandscapeClip, 0.5);
        await Expect(Clock).ToHaveTextAsync(new Regex(@"^00:0\d / 00:04$"));
        (await Page.EvaluateAsync(() => Fixture.Player.CurrentFilePath))
            .Should().Be(await Page.EvaluateAsync(() => Fixture.Model.SelectedVideo.FullPath));
        await Expect(Message).ToBeHiddenAsync();
        await Button("Stop").ClickAsync();
    }

    [Fact]
    public async Task Playback_reaches_the_end()
    {
        await PlayUntilAsync(LandscapeClip, 0.5);
        await Fixture.Application.WaitForAsync(() => Fixture.Player.TransportState,
            state => state == VideoTransportState.Stopped, timeout: 30000, description: "the end of the clip");
        var duration = await Page.EvaluateAsync(() => Fixture.Model.DurationSeconds);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.PositionSeconds,
            position => position >= duration - 0.25, description: "the clock at the end");
        await Expect(Button("Play")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Stop_returns_the_clock_to_zero()
    {
        await PlayUntilAsync(LandscapeClip, 1);
        await Button("Stop").ClickAsync();
        await Expect(Clock).ToHaveTextAsync("00:00 / 00:04");
        await Fixture.Application.WaitForAsync(() => Fixture.Player.TransportState,
            state => state == VideoTransportState.Stopped, description: "the stopped transport");
        // The session's stopped clock sits on the first audio sample, a few milliseconds in.
        (await Page.EvaluateAsync(() => Fixture.Model.PositionSeconds)).Should().BeLessThan(0.05);
    }

    [Fact]
    public async Task Pause_holds_the_position_and_Play_resumes_from_it()
    {
        await PlayUntilAsync(LandscapeClip, 1);
        var paused = await PauseAsync();
        paused.Should().BeGreaterThanOrEqualTo(1);
        await Expect(Clock).ToHaveTextAsync($"{TimeSpan.FromSeconds(paused):mm\\:ss} / 00:04");
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Player.Position.TotalSeconds,
            position => position >= paused + 0.5, timeout: 30000, description: "playback resumed");
        (await Page.EvaluateAsync(() => Fixture.Model.PositionSeconds)).Should().BeGreaterThanOrEqualTo(paused);
        await Button("Stop").ClickAsync();
    }

    [Fact]
    public async Task Choosing_another_clip_resets_the_transport()
    {
        await PlayUntilAsync(LandscapeClip, 1);
        await ChooseClipAsync(PortraitClip);
        await Expect(Clock).ToHaveTextAsync("00:00 / 00:01");
        (await Page.EvaluateAsync(() => Fixture.Player.IsOpen)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Player.TransportState)).Should().Be(VideoTransportState.Stopped);
    }

    [Fact]
    public async Task Paused_frame_is_drawn_into_the_video_area()
    {
        await PlayUntilAsync(LandscapeClip, 1);
        await PauseAsync();
        var box = await Page.GetByTestId("VideoHost").BoundingBoxAsync();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        // A band across the middle: the picture fills it in either orientation, whatever bars surround it.
        NonBlackShare(bitmap, box.X + box.Width * 0.1f, box.Y + box.Height * 0.45f, box.Width * 0.8f, box.Height * 0.1f)
            .Should().BeGreaterThan(0.5);
        await SnapshotAsync("SimpleCbxVideoPlayer-paused-frame");
    }

    [Theory]
    [InlineData("CodeBrix-Mode1/landscape_720p.cbv")]
    [InlineData("CodeBrix-Mode2/landscape_720p.cbv")]
    [InlineData("MKV/landscape_720p.mkv")]
    [InlineData("WebM/landscape_720p.webm")]
    public async Task Every_container_plays_its_720p_clip(string clip)
    {
        await PlayUntilAsync(clip, 0.5);
        await Expect(Clock).ToHaveTextAsync(new Regex(@"^00:0\d / 00:04$"));
        (await Page.EvaluateAsync(() => Fixture.Player.DisplayWidth)).Should().Be(1280);
        (await Page.EvaluateAsync(() => Fixture.Player.DisplayHeight)).Should().Be(720);
        await Expect(Message).ToBeHiddenAsync();
        await Button("Stop").ClickAsync();
        await Expect(Clock).ToHaveTextAsync("00:00 / 00:04");
    }

    [Fact]
    public async Task Keyboard_seek_on_the_scrub_bar_moves_the_paused_picture()
    {
        await PlayUntilAsync(LandscapeClip, 0.5);
        await PauseAsync();
        // Each step is one second; from either end of the first second a step lands exactly on 0 or 1.
        await SeekWithKeyAsync("Home", 0);
        await SeekWithKeyAsync("ArrowRight", 1);
        await SeekWithKeyAsync("ArrowLeft", 0);
        await SeekWithKeyAsync("ArrowRight", 1);
        (await Page.EvaluateAsync(() => Fixture.Player.TransportState)).Should().Be(VideoTransportState.Paused);
    }

    [Fact]
    public async Task Frame_at_one_second_matches_across_mkv_webm_and_mode1()
    {
        var frames = new List<string>();
        foreach (var clip in new[] { "MKV/landscape_720p.mkv", "WebM/landscape_720p.webm", LandscapeClip })
        {
            await PlayUntilAsync(clip, 0.3);
            await PauseAsync();
            await SeekToOneSecondAsync();
            frames.Add(await SaveVideoAreaAsync());
        }
        foreach (var other in frames.Skip(1))
        {
            var comparison = ImageComparison.Compare(frames[0], other);
            comparison.SizesMatch.Should().BeTrue();
            comparison.MaxChannelDelta.Should().BeLessThanOrEqualTo(8);
        }
        await SnapshotAsync("SimpleCbxVideoPlayer-frame-at-one-second");
    }

    // Home, then one arrow step of one second; the paused player seeks exactly and shows the frame it landed on.
    private async Task SeekToOneSecondAsync()
    {
        await SeekWithKeyAsync("Home", 0);
        await SeekWithKeyAsync("ArrowRight", 1);
    }

    // The middle of the video area, saved as a PNG under the fixture's data folder. The edges are left out:
    // the scrub bar's value tip can overlap the foot of the picture after a keyboard seek.
    private async Task<string> SaveVideoAreaAsync()
    {
        var box = await Page.GetByTestId("VideoHost").BoundingBoxAsync();
        using var screen = SKBitmap.Decode(await Page.ScreenshotAsync());
        using var area = new SKBitmap((int)(box.Width * 0.8f), (int)(box.Height * 0.7f));
        screen.ExtractSubset(area, SKRectI.Create((int)(box.X + box.Width * 0.1f), (int)(box.Y + box.Height * 0.15f),
            area.Width, area.Height)).Should().BeTrue();
        var path = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".png");
        using (var stream = File.Create(path)) area.Encode(stream, SKEncodedImageFormat.Png, 100);
        return path;
    }

    private static double NonBlackShare(SKBitmap bitmap, float x, float y, float width, float height)
    {
        int counted = 0, lit = 0;
        for (var py = (int)y; py < (int)(y + height); py += 4)
        {
            for (var px = (int)x; px < (int)(x + width); px += 4)
            {
                var pixel = bitmap.GetPixel(px, py);
                counted++;
                if (pixel.Red > 16 || pixel.Green > 16 || pixel.Blue > 16) lit++;
            }
        }
        return counted == 0 ? 0 : (double)lit / counted;
    }
}
