using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Platform.Simple;
using CodeBrix.Samples.PlayTests;
using SilverAssertions;
using SkiaSharp;
using WebcamViewer.Cameras;
using WebcamViewer.Views;
using Xunit;

namespace WebcamViewer.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string CameraA = "Fixture Camera A";
    private const string CameraB = "Fixture Camera B";
    private static readonly SKColor FrameColor = new(214, 48, 96);

    private Locator Status => Page.GetByTestId("StatusText");
    private Locator CameraList => Page.GetByTestId("CameraList");
    private Locator MonitorAudio => Page.GetByTestId("MonitorAudio");
    private Locator FolderPath => Page.GetByTestId("FolderPath");

    [Fact]
    public async Task No_cameras_reports_status_and_disables_photo_and_audio()
    {
        await Expect(Status).ToHaveTextAsync("Discovering cameras…");
        Fixture.Cameras.Discover();
        await Expect(Status).ToHaveTextAsync("No cameras were found on this machine.");
        await FolderPath.FillAsync(NewFolder());
        await Expect(Button("Photo")).ToBeDisabledAsync();
        await Expect(MonitorAudio).ToBeDisabledAsync();
        await Expect(Button("Browse…")).ToBeEnabledAsync();
        Fixture.Cameras.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task First_camera_auto_starts_and_reports_live()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA), CameraFixture.Camera(CameraB));
        await Expect(CameraList).ToHaveValueAsync(CameraA);
        var session = Fixture.Cameras.Sessions.Single();
        session.Device.FriendlyName.Should().Be(CameraA);
        session.IsStarted.Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.Cameras.Select(c => c.ToString()).ToArray()))
            .Should().Equal(CameraA, CameraB);
        await Expect(Button("Photo")).ToBeDisabledAsync();
        // Every page, the launch page included, resolves the fixture and never the real cameras.
        (await Page.EvaluateAsync(() => SimpleServiceResolver.Instance.GetService<ICameraService>()))
            .Should().BeSameAs(Fixture.Cameras);
        await SnapshotAsync("WebcamViewer-live");
    }

    [Fact]
    public async Task Discovery_failure_is_reported()
    {
        Fixture.Cameras.FailDiscovery("Fixture device list unavailable");
        await Expect(Status).ToHaveTextAsync("Camera discovery failed: Fixture device list unavailable");
        await Expect(Button("Photo")).ToBeDisabledAsync();
        await Expect(MonitorAudio).ToBeDisabledAsync();
        Fixture.Cameras.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Start_failure_is_reported_without_crash()
    {
        Fixture.Cameras.Discover(CameraFixture.Camera(CameraA, microphone: true, startError: "Fixture camera is busy"));
        await Expect(Status).ToHaveTextAsync($"Could not start '{CameraA}': Fixture camera is busy");
        await Expect(CameraList).ToHaveValueAsync(CameraA);
        await Expect(MonitorAudio).ToBeDisabledAsync();
        await FolderPath.FillAsync(NewFolder());
        await Expect(Button("Photo")).ToBeDisabledAsync();
        Fixture.Cameras.Sessions.Single().IsStarted.Should().BeFalse();
    }

    [Fact]
    public async Task Photo_disabled_until_frame_and_valid_folder()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        // A folder alone is not enough.
        await FolderPath.FillAsync(NewFolder());
        await Expect(Button("Photo")).ToBeDisabledAsync();
        // Nor is a frame alone.
        await FolderPath.FillAsync("");
        await SendFrameAsync(64, 48);
        await Expect(Button("Photo")).ToBeDisabledAsync();
        await FolderPath.FillAsync(NewFolder());
        await Expect(Button("Photo")).ToBeEnabledAsync();
        await FolderPath.FillAsync("   ");
        await Expect(Button("Photo")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Browse_sets_folder_from_picker()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        var folder = NewFolder();
        Fixture.Application.FilePickers.EnqueueFolder(folder);
        await Button("Browse…").ClickAsync();
        await Expect(FolderPath).ToHaveValueAsync(folder);
        await Expect(Status).ToHaveTextAsync("Photos will be saved to: " + folder);
        Fixture.Application.FilePickers.FolderRequestCount.Should().Be(1);
    }

    [Fact]
    public async Task Browse_cancel_leaves_folder_unchanged()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        var folder = NewFolder();
        await FolderPath.FillAsync(folder);
        Fixture.Application.FilePickers.EnqueueFolder(null);
        await Button("Browse…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.FolderRequestCount,
            count => count == 1, description: "folder picker request");
        await Expect(FolderPath).ToHaveValueAsync(folder);
        await Expect(Status).ToHaveTextAsync("Live: " + CameraA);
        await Expect(Button("Browse…")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Photo_writes_png_with_fake_frame_pixels()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        await SendFrameAsync(64, 48);
        var folder = NewFolder();
        await FolderPath.FillAsync(folder);
        await Button("Photo").ClickAsync();
        await Expect(Status).ToHaveTextAsync(new Regex(@"^Saved: .+frame_capture_\d{8}_\d{6}_\d{3}\.png$"));
        var file = Directory.GetFiles(folder, "frame_capture_*.png").Single();
        (await Page.EvaluateAsync(() => Fixture.Model.StatusText)).Should().Be("Saved: " + file);
        using (var image = SKBitmap.Decode(file))
        {
            image.Width.Should().Be(64);
            image.Height.Should().Be(48);
            image.GetPixel(0, 0).Should().Be(FrameColor);
            image.GetPixel(32, 24).Should().Be(FrameColor);
            image.GetPixel(63, 47).Should().Be(FrameColor);
        }
        await Expect(Button("Photo")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Typed_nonexistent_folder_keeps_photo_disabled()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        await SendFrameAsync(64, 48);
        var missing = Path.Combine(Fixture.DataDirectory, "missing-" + Guid.NewGuid().ToString("N"));
        await FolderPath.FillAsync(missing);
        await Expect(Button("Photo")).ToBeDisabledAsync();
        Directory.CreateDirectory(missing);
        // The folder is checked again when the path next changes.
        await FolderPath.FillAsync(missing + Path.DirectorySeparatorChar);
        await Expect(Button("Photo")).ToBeEnabledAsync();
        Directory.GetFiles(missing).Should().BeEmpty();
    }

    [Fact]
    public async Task Video_canvas_shows_frame_color_after_frame_arrives()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        var center = await CanvasPointAsync(0.5, 0.5);
        (await PixelAsync(center)).Should().Be(SKColors.Black);
        await SendFrameAsync(64, 48);
        (await WaitForPixelAsync(center, FrameColor)).Should().Be(FrameColor);
        await SnapshotAsync("WebcamViewer-frame");
    }

    [Fact]
    public async Task Switching_camera_disposes_old_session_and_starts_new()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA), CameraFixture.Camera(CameraB));
        await SendFrameAsync(64, 48);
        await FolderPath.FillAsync(NewFolder());
        await Expect(Button("Photo")).ToBeEnabledAsync();
        var center = await CanvasPointAsync(0.5, 0.5);
        await WaitForPixelAsync(center, FrameColor);
        await ChooseCameraAsync(CameraB);
        await Expect(Status).ToHaveTextAsync("Live: " + CameraB);
        var sessions = Fixture.Cameras.Sessions;
        sessions.Select(s => s.Device.FriendlyName).Should().Equal(CameraA, CameraB);
        sessions[0].IsDisposed.Should().BeTrue();
        sessions[1].IsStarted.Should().BeTrue();
        sessions[1].IsDisposed.Should().BeFalse();
        // The old camera's frame is dropped until the new one delivers.
        await Expect(Button("Photo")).ToBeDisabledAsync();
        (await WaitForPixelAsync(center, SKColors.Black)).Should().Be(SKColors.Black);
    }

    [Fact]
    public async Task Monitor_audio_enabled_only_with_mic_and_propagates_to_session()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA, microphone: true), CameraFixture.Camera(CameraB),
            CameraFixture.Camera("Fixture Camera C", microphone: true));
        await Expect(MonitorAudio).ToBeEnabledAsync();
        await MonitorAudio.CheckAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Cameras.Sessions[0].MonitorAudio, on => on,
            description: "monitoring on the first session");
        await ChooseCameraAsync(CameraB);
        await Expect(Status).ToHaveTextAsync("Live: " + CameraB);
        await Expect(MonitorAudio).ToBeDisabledAsync();
        await ChooseCameraAsync("Fixture Camera C");
        await Expect(Status).ToHaveTextAsync("Live: Fixture Camera C");
        await Expect(MonitorAudio).ToBeEnabledAsync();
        await Expect(MonitorAudio).ToBeCheckedAsync();
        // The choice carries over to the new session.
        Fixture.Cameras.Sessions[2].MonitorAudio.Should().BeTrue();
        await MonitorAudio.UncheckAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Cameras.Sessions[2].MonitorAudio, on => !on,
            description: "monitoring off on the current session");
    }

    [Fact]
    public async Task Photo_failure_is_reported_and_busy_clears()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        await SendFrameAsync(64, 48);
        var folder = NewFolder();
        await FolderPath.FillAsync(folder);
        Fixture.Cameras.PhotoError = new InvalidOperationException("Fixture photo timed out");
        await Button("Photo").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Photo failed: Fixture photo timed out");
        await Expect(Button("Photo")).ToBeEnabledAsync();
        await Expect(Button("Browse…")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsBusy)).Should().BeFalse();
        Directory.GetFiles(folder).Should().BeEmpty();
    }

    [Fact]
    public async Task Page_reset_disposes_session()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        var session = Fixture.Cameras.Sessions.Single();
        await Page.SetContentAsync(() => new MainPage());
        await Fixture.Application.WaitForAsync(() => session.IsDisposed, disposed => disposed,
            description: "the old page's session disposed");
        // The new page discovers again and starts its own session.
        await Expect(Status).ToHaveTextAsync("Live: " + CameraA);
        Fixture.Cameras.Sessions.Should().HaveCount(2);
        Fixture.Cameras.Sessions[1].IsDisposed.Should().BeFalse();
    }

    [Fact]
    public async Task Frame_aspect_fit_letterboxes_wide_frame()
    {
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        // Far wider than the canvas in either orientation, so black bars sit above and below.
        await SendFrameAsync(800, 50);
        var center = await CanvasPointAsync(0.5, 0.5);
        (await WaitForPixelAsync(center, FrameColor)).Should().Be(FrameColor);
        (await PixelAsync(await CanvasPointAsync(0.5, 0.1))).Should().Be(SKColors.Black);
        (await PixelAsync(await CanvasPointAsync(0.5, 0.9))).Should().Be(SKColors.Black);
        // The frame fills the full width.
        (await PixelAsync(await CanvasPointAsync(0.02, 0.5))).Should().Be(FrameColor);
        (await PixelAsync(await CanvasPointAsync(0.98, 0.5))).Should().Be(FrameColor);
    }

    private async Task DiscoverAsync(params CameraDevice[] cameras)
    {
        Fixture.Cameras.Discover(cameras);
        await Expect(Status).ToHaveTextAsync("Live: " + cameras[0].FriendlyName);
    }

    private async Task SendFrameAsync(int width, int height)
    {
        await Fixture.Cameras.Sessions.Last().SendFrameAsync(width, height, FrameColor.Red, FrameColor.Green, FrameColor.Blue);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.HasFrame, hasFrame => hasFrame,
            description: "the first frame");
    }

    private async Task ChooseCameraAsync(string name)
    {
        await CameraList.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
    }

    private string NewFolder()
    {
        var folder = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        return folder;
    }

    // A point inside the video canvas, as fractions of its width and height.
    private async Task<SKPointI> CanvasPointAsync(double x, double y)
    {
        var box = await Page.GetByTestId("VideoView").BoundingBoxAsync();
        return new SKPointI((int)(box.X + box.Width * x), (int)(box.Y + box.Height * y));
    }

    private async Task<SKColor> PixelAsync(SKPointI point)
    {
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        return bitmap.GetPixel(point.X, point.Y);
    }

    // Re-captures until the canvas's next paint shows the colour (each capture renders a frame).
    private async Task<SKColor> WaitForPixelAsync(SKPointI point, SKColor expected)
    {
        var elapsed = Stopwatch.StartNew();
        SKColor pixel;
        do pixel = await PixelAsync(point);
        while (pixel != expected && elapsed.Elapsed < TimeSpan.FromSeconds(10));
        return pixel;
    }
}
