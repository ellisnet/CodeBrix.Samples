using System;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PalmVisualizer.Rendering;
using PalmVisualizer.Views;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PalmVisualizer.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string OpenPalmPrompt = "Show the camera your open palm - the colors will gather toward it.";

    private Locator Status => Page.GetByTestId("StatusText");
    private Locator CameraPicker => Page.GetByTestId("CameraPicker");
    private Locator VisualizerPicker => Page.GetByTestId("VisualizerPicker");

    [Fact]
    public async Task Startup_discovers_cameras_and_auto_selects_the_first()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        await Expect(CameraPicker).ToHaveValueAsync("Fake Cam A");
        await Expect(CameraPicker).ToBeEnabledAsync();
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("PreviewCanvas")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("VisualizerCanvas")).ToBeHiddenAsync();
        Fixture.Capture.Started.Should().Equal("Fake Cam A");
        (await Page.EvaluateAsync(() => Fixture.Model.Cameras.Select(camera => camera.FriendlyName).ToArray()))
            .Should().Equal(CaptureFixture.DefaultCameras);
    }

    [Fact]
    public async Task No_cameras_reports_none_found_and_visualize_stays_disabled()
    {
        Fixture.Capture.Cameras = Array.Empty<string>();
        await Fixture.ReloadAsync();
        await Expect(Status).ToHaveTextAsync("No cameras were found on this machine.");
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedCamera)).Should().BeNull();
        Fixture.Capture.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task Discovery_failure_is_reported_in_the_status_line()
    {
        Fixture.Capture.DiscoveryError = new InvalidOperationException("Fixture discovery unavailable");
        await Fixture.ReloadAsync();
        await Expect(Status).ToHaveTextAsync("Camera discovery failed: Fixture discovery unavailable");
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        Fixture.Capture.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task Camera_start_failure_reports_could_not_start()
    {
        Fixture.Capture.FailingCameras.Add("Fake Cam A");
        await Fixture.ReloadAsync();
        await Expect(Status).ToHaveTextAsync("Could not start 'Fake Cam A': Fixture camera unavailable");
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        Fixture.Capture.Started.Should().Equal("Fake Cam A");
    }

    [Fact]
    public async Task Visualize_is_disabled_until_the_first_frame_arrives()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        PushFrame();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.HasFrame)).Should().BeTrue();
    }

    [Fact]
    public async Task Visualize_switches_mode_starts_tracker_and_creates_one_session()
    {
        await VisualizeAsync();
        await Expect(Button("Visualize!")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("VisualizerCanvas")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("PreviewCanvas")).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);
        Fixture.Tracker.StartCount.Should().Be(1);
        Fixture.Tracker.IsRunning.Should().BeTrue();
        Fixture.Sessions.Created.Count.Should().Be(1);
        var session = Fixture.Sessions.Last;
        session.StartCount.Should().Be(1);
        session.ResumeCount.Should().Be(0);
        (await Page.EvaluateAsync(() => ReferenceEquals(session.Host, Fixture.View))).Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.IsVisualizeMode)).Should().BeTrue();
    }

    [Fact]
    public async Task Back_returns_to_camera_mode_stops_tracker_and_pauses_session()
    {
        await VisualizeAsync();
        await Button("Back").ClickAsync();
        await Expect(Button("Visualize!")).ToBeVisibleAsync();
        await Expect(Button("Back")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("PreviewCanvas")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("VisualizerCanvas")).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        Fixture.Tracker.StopCount.Should().Be(1);
        Fixture.Tracker.IsRunning.Should().BeFalse();
        Fixture.Sessions.Last.PauseCount.Should().Be(1);
        Fixture.Sessions.Last.StopCount.Should().Be(0);
    }

    [Fact]
    public async Task Second_visualize_resumes_the_existing_session()
    {
        await VisualizeAsync();
        await Button("Back").ClickAsync();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        await Button("Visualize!").ClickAsync();
        await Expect(Button("Back")).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);
        Fixture.Sessions.Created.Count.Should().Be(1);
        Fixture.Sessions.Last.StartCount.Should().Be(1);
        Fixture.Sessions.Last.PauseCount.Should().Be(1);
        Fixture.Sessions.Last.ResumeCount.Should().Be(1);
        Fixture.Tracker.StartCount.Should().Be(2);
    }

    [Fact]
    public async Task Visualizer_picker_lists_every_visualizer_and_defaults_to_ethereal_webgpu()
    {
        await Expect(VisualizerPicker).ToHaveValueAsync("Ethereal - WebGPU");
        await Expect(VisualizerPicker).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Visualizers.Select(option => option.DisplayName).ToArray()))
            .Should().Equal("Ethereal - WebGPU", "Ethereal - OpenGL", "Ripple - WebGPU", "Holy Inanna - WebGPU", "Crystals - WebGPU");
        await VisualizeAsync();
        Fixture.Sessions.Last.Kind.Should().Be(VisualizerKind.EtherealWebGpu);
        await Expect(VisualizerPicker).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Choosing_a_visualizer_while_visualizing_replaces_the_session()
    {
        await VisualizeAsync();
        var first = Fixture.Sessions.Last;
        await SelectVisualizerAsync("Ethereal - OpenGL");
        await Fixture.Application.WaitForAsync(() => Fixture.Sessions.Created.Count, count => count == 2,
            description: "the OpenGL visualizer session");
        first.StopCount.Should().Be(1);
        var second = Fixture.Sessions.Last;
        second.Kind.Should().Be(VisualizerKind.EtherealOpenGL);
        second.StartCount.Should().Be(1);
        (await Page.EvaluateAsync(() => ReferenceEquals(second.Host, Fixture.View))).Should().BeTrue();
        await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);

        // Palms now reach the new session only
        Fixture.Tracker.Raise(TrackerFixture.Palm(3, true, 0.5f, 0.5f));
        second.UpdateCount.Should().Be(1);
        first.UpdateCount.Should().Be(0);
    }

    [Theory]
    [InlineData("Ripple - WebGPU", VisualizerKind.RippleWebGpu)]
    [InlineData("Holy Inanna - WebGPU", VisualizerKind.HolyInannaWebGpu)]
    [InlineData("Crystals - WebGPU", VisualizerKind.CrystalsWebGpu)]
    public async Task Each_new_visualizer_gets_a_session_of_its_own_kind(string name, VisualizerKind kind)
    {
        await VisualizeAsync();
        var first = Fixture.Sessions.Last;
        await SelectVisualizerAsync(name);
        await Expect(VisualizerPicker).ToHaveValueAsync(name);
        await Fixture.Application.WaitForAsync(() => Fixture.Sessions.Created.Count, count => count == 2,
            description: "the newly chosen visualizer's session");
        first.StopCount.Should().Be(1);
        Fixture.Sessions.Last.Kind.Should().Be(kind);
        Fixture.Sessions.Last.StartCount.Should().Be(1);
        Fixture.Tracker.Raise(TrackerFixture.Palm(5, true, 0.25f, 0.5f));
        Fixture.Sessions.Last.Palms.Should().HaveCount(1);
        Fixture.Sessions.Last.Palms[0].X.Should().Be(0.75f);
    }

    [Fact]
    public async Task A_visualizer_chosen_in_camera_mode_is_used_by_the_next_visualize()
    {
        await VisualizeAsync();
        var first = Fixture.Sessions.Last;
        await Button("Back").ClickAsync();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        await SelectVisualizerAsync("Ethereal - OpenGL");
        await Expect(VisualizerPicker).ToHaveValueAsync("Ethereal - OpenGL");

        // The paused session is stopped at once; the new one waits for "Visualize!"
        first.StopCount.Should().Be(1);
        Fixture.Sessions.Created.Count.Should().Be(1);
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");

        await Button("Visualize!").ClickAsync();
        await Expect(Button("Back")).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Sessions.Created.Count, count => count == 2,
            description: "the OpenGL visualizer session");
        Fixture.Sessions.Last.Kind.Should().Be(VisualizerKind.EtherealOpenGL);
        Fixture.Sessions.Last.StartCount.Should().Be(1);
        Fixture.Sessions.Last.ResumeCount.Should().Be(0);
    }

    [Fact]
    public async Task A_visualizer_problem_is_shown_in_the_status_line()
    {
        await VisualizeAsync();
        Fixture.Sessions.Last.RaiseProblem("Ethereal - WebGPU has no WebGPU device: no adapter was found.");
        await Expect(Status).ToHaveTextAsync("Ethereal - WebGPU has no WebGPU device: no adapter was found.");
    }

    [Fact]
    public async Task A_replaced_visualizer_can_no_longer_report_problems()
    {
        await VisualizeAsync();
        var first = Fixture.Sessions.Last;
        await SelectVisualizerAsync("Ethereal - OpenGL");
        await Fixture.Application.WaitForAsync(() => Fixture.Sessions.Created.Count, count => count == 2,
            description: "the OpenGL visualizer session");
        first.RaiseProblem("Ethereal - WebGPU has no WebGPU device: stale.");
        await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);
    }

    [Fact]
    public async Task Frames_reach_the_tracker_only_in_visualize_mode()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        PushFrame();
        PushFrame();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        Fixture.Tracker.Submitted.Should().Be(0);
        await Button("Visualize!").ClickAsync();
        await Expect(Button("Back")).ToBeVisibleAsync();
        PushFrame(32, 24);
        Fixture.Tracker.Submitted.Should().Be(1);
        Fixture.Tracker.LastFrameSize.Should().Be((32, 24));
        await Button("Back").ClickAsync();
        await Expect(Button("Visualize!")).ToBeVisibleAsync();
        PushFrame();
        Fixture.Tracker.Submitted.Should().Be(1);
    }

    [Fact]
    public async Task Open_palms_are_mirrored_and_closed_palms_ignored()
    {
        await VisualizeAsync();
        var session = Fixture.Sessions.Last;
        Fixture.Tracker.Raise(TrackerFixture.Palm(7, true, 0.25f, 0.4f), TrackerFixture.Palm(8, false, 0.6f, 0.5f));
        session.Palms.Should().HaveCount(1);
        session.Palms[0].Id.Should().Be(7);
        session.Palms[0].X.Should().Be(0.75f);
        session.Palms[0].Y.Should().Be(0.4f);
        await Expect(Status).ToHaveTextAsync(
            "The colors are chasing your open palm - close your hand to set them free.");
        Fixture.Tracker.Raise(TrackerFixture.Palm(7, true, 0.25f, 0.4f), TrackerFixture.Palm(8, true, 0.875f, 0.5f));
        session.Palms.Select(palm => palm.X).Should().Equal(0.75f, 0.125f);
        await Expect(Status).ToHaveTextAsync(
            "The colors are chasing 2 open palms - close your hands to set them free.");
        Fixture.Tracker.Raise(TrackerFixture.Palm(8, false, 0.875f, 0.5f));
        session.Palms.Should().BeEmpty();
        await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);
        session.UpdateCount.Should().Be(3);
    }

    [Fact]
    public async Task Tracking_results_are_ignored_in_camera_mode()
    {
        await VisualizeAsync();
        var session = Fixture.Sessions.Last;
        await Button("Back").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        Fixture.Tracker.Raise(TrackerFixture.Palm(1, true, 0.5f, 0.5f));
        session.UpdateCount.Should().Be(0);
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
    }

    [Fact]
    public async Task Camera_picker_is_disabled_in_visualize_mode()
    {
        await Expect(CameraPicker).ToBeEnabledAsync();
        await VisualizeAsync();
        await Expect(CameraPicker).ToBeDisabledAsync();
        await Button("Back").ClickAsync();
        await Expect(CameraPicker).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Switching_cameras_restarts_capture_and_reports_the_new_camera()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        PushFrame();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        await SelectCameraAsync("Fake Cam B");
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam B");
        await Expect(CameraPicker).ToHaveValueAsync("Fake Cam B");
        // The new camera has delivered no frame yet.
        await Expect(Button("Visualize!")).ToBeDisabledAsync();
        Fixture.Capture.Started.Should().Equal("Fake Cam A", "Fake Cam B");
        PushFrame();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Late_finishing_camera_switch_does_not_overwrite_newer_status()
    {
        Fixture.Capture.Cameras = new[] { "Fake Cam A", "Fake Cam B", "Fake Cam C" };
        await Fixture.ReloadAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        var slow = Fixture.Capture.HoldStart("Fake Cam B");
        await SelectCameraAsync("Fake Cam B");
        await Fixture.Application.WaitForAsync(() => Fixture.Capture.InFlight, count => count == 1,
            description: "the held start of Fake Cam B");
        await SelectCameraAsync("Fake Cam C");
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam C");
        slow.TrySetResult();
        await Fixture.Application.WaitForAsync(() => Fixture.Capture.InFlight, count => count == 0,
            description: "the late start of Fake Cam B");
        Fixture.Capture.Started.Should().Equal("Fake Cam A", "Fake Cam B", "Fake Cam C");
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam C");
        await Expect(CameraPicker).ToHaveValueAsync("Fake Cam C");
    }

    [Fact]
    public async Task Preview_paints_the_canned_frame_mirrored()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        var box = await Page.GetByTestId("PreviewCanvas").BoundingBoxAsync();
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            IsBlack(bitmap.GetPixel((int)(box.X + box.Width / 2), (int)(box.Y + box.Height / 2))).Should().BeTrue();
        }
        // Red on the camera's left, blue on its right: the selfie-style preview shows them swapped.
        Fixture.Capture.PushFrame(CaptureFixture.SplitFrame(64, 48, (220, 0, 0), (0, 0, 220)), 64, 48);
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        // The repaint is queued on the UI thread right behind the HasFrame update; one more UI round trip runs it.
        await Page.EvaluateAsync(() => { });
        // The frame is aspect-fitted and centred; sample a quarter of its width either side of the centre.
        var scale = Math.Min(box.Width / 64, box.Height / 48);
        var quarter = 64 * scale / 4;
        var centerX = box.X + box.Width / 2;
        var centerY = (int)(box.Y + box.Height / 2);
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            IsBlue(bitmap.GetPixel((int)(centerX - quarter), centerY)).Should().BeTrue();
            IsRed(bitmap.GetPixel((int)(centerX + quarter), centerY)).Should().BeTrue();
        }
        await SnapshotAsync("PalmVisualizer-preview");
    }

    private static bool IsBlack(SKColor color) => color.Red < 40 && color.Green < 40 && color.Blue < 40;
    private static bool IsRed(SKColor color) => color.Red > 160 && color.Green < 60 && color.Blue < 60;
    private static bool IsBlue(SKColor color) => color.Blue > 160 && color.Red < 60 && color.Green < 60;

    private void PushFrame(int width = 16, int height = 12) => Fixture.Capture.PushFrame(
        CaptureFixture.SplitFrame(width, height, (200, 0, 0), (0, 0, 200)), width, height);

    // A first frame enables Visualize; clicking it creates and starts the selected visualizer's session.
    private async Task VisualizeAsync()
    {
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        PushFrame();
        await Expect(Button("Visualize!")).ToBeEnabledAsync();
        await Button("Visualize!").ClickAsync();
        await Expect(Button("Back")).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Sessions.Created.Count, count => count == 1,
            description: "the visualizer session");
    }

    private async Task SelectCameraAsync(string name)
    {
        await CameraPicker.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
    }

    private async Task SelectVisualizerAsync(string name)
    {
        await VisualizerPicker.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
    }
}
