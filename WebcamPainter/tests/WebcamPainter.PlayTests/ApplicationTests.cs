using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using SilverAssertions;
using SkiaSharp;
using WebcamPainter.Views;
using Xunit;

namespace WebcamPainter.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string LiveFirst = "Live: " + CameraFixture.FirstCamera;
    private const string PaintStatus = "Show the camera your open palm to spread paint on the photo - " +
                                       "close your hand (or hide it) to stop painting.";
    private Locator Status => Page.GetByTestId("Status");
    private Locator ActiveColor => Page.GetByTestId("ActiveColor");
    private Locator MainCanvas => Page.GetByTestId("MainCanvas");
    private Locator SelfView => Page.GetByTestId("SelfView");
    private Locator Dialog(string title) => Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = title });

    // The first camera running with one frame delivered, so Take Photo is available.
    private async Task StartLiveAsync()
    {
        await Expect(Status).ToHaveTextAsync(LiveFirst);
        Fixture.Camera.EmitFrame();
        await Expect(Button("Take Photo")).ToBeEnabledAsync();
    }

    private async Task TakePhotoAsync()
    {
        await StartLiveAsync();
        await Button("Take Photo").ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: Red");
        await Fixture.Application.WaitForAsync(() => Fixture.Session != null, ready => ready,
            description: "the painting session");
    }

    // An open palm moved from (x0, y0) to (x1, y1), then closed: one committed stroke. Palm
    // positions are the tracker's, across the unmirrored camera frame.
    private async Task PaintStrokeAsync(float x0, float y0, float x1, float y1)
    {
        var count = await Page.EvaluateAsync(() => Fixture.Session.StrokeCount);
        Fixture.Tracker.Report(true, x0, y0);
        Fixture.Tracker.Report(true, (x0 + x1) / 2, (y0 + y1) / 2);
        Fixture.Tracker.Report(true, x1, y1);
        Fixture.Tracker.Report(false, x1, y1);
        await Fixture.Application.WaitForAsync(() => Fixture.Session.StrokeCount, value => value == count + 1,
            description: "the committed stroke");
    }

    private Task PaintStrokeAsync(float y) => PaintStrokeAsync(0.6f, y, 0.4f, y);

    private async Task AnswerAsync(string title, string button)
    {
        await Dialog(title).GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await Expect(Dialog(title)).ToHaveCountAsync(0);
    }

    private async Task<SKColor> ScreenPixelAsync(Locator canvas, float x, float y)
    {
        var box = await canvas.BoundingBoxAsync();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        return bitmap.GetPixel((int)(box.X + x), (int)(box.Y + y));
    }

    // A pixel of the painted still at a normalized position of the (mirrored) still.
    private async Task<SKColor> PaintingPixelAsync(float normalizedX, float normalizedY)
    {
        var box = await MainCanvas.BoundingBoxAsync();
        var point = await Page.EvaluateAsync(() =>
            Fixture.Session.NormalizedToView(normalizedX, normalizedY, box.Width, box.Height));
        return await ScreenPixelAsync(MainCanvas, point.X, point.Y);
    }

    // The two halves of a live frame drawn aspect-fit into a canvas, a quarter of the frame in from each side.
    private async Task<(SKColor Left, SKColor Right)> FrameHalvesAsync(Locator canvas)
    {
        var box = await canvas.BoundingBoxAsync();
        var scale = Math.Min(box.Width / CameraFixture.DefaultWidth, box.Height / CameraFixture.DefaultHeight);
        var quarter = CameraFixture.DefaultWidth * scale / 4;
        var left = await ScreenPixelAsync(canvas, box.Width / 2 - quarter, box.Height / 2);
        var right = await ScreenPixelAsync(canvas, box.Width / 2 + quarter, box.Height / 2);
        return (left, right);
    }

    private static bool IsRed(SKColor pixel) => pixel.Red > 200 && pixel.Green < 60 && pixel.Blue < 60;
    private static bool IsBlue(SKColor pixel) => pixel.Blue > 200 && pixel.Red < 60 && pixel.Green < 60;
    private static bool IsWhite(SKColor pixel) => pixel.Red > 245 && pixel.Green > 245 && pixel.Blue > 245;

    private string NewJpegPath() => Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".jpg");

    [Fact]
    public async Task Startup_lists_canned_cameras_and_starts_the_first()
    {
        await Expect(Page.GetByText("Webcam Painter", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync(LiveFirst);
        (await Page.EvaluateAsync(() => string.Join(",", Fixture.Model.Cameras.Select(camera => camera.FriendlyName))))
            .Should().Be(CameraFixture.FirstCamera + "," + CameraFixture.SecondCamera);
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedCamera.FriendlyName)).Should().Be(CameraFixture.FirstCamera);
        Fixture.Camera.Started.Should().BeEquivalentTo(new[] { CameraFixture.FirstCamera });
        await Expect(Page.GetByTestId("Camera")).ToBeEnabledAsync();
        await Expect(Button("Take Photo")).ToBeVisibleAsync();
        await Expect(Button("Back")).ToBeHiddenAsync();
        await Expect(SelfView).ToBeHiddenAsync();
        await SnapshotAsync("WebcamPainter-start");
    }

    [Fact]
    public async Task No_cameras_reports_status_and_disables_take_photo()
    {
        Fixture.Camera.Cameras.Clear();
        await Fixture.ReloadAsync();
        await Expect(Status).ToHaveTextAsync("No cameras were found on this machine.");
        await Expect(Button("Take Photo")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cameras.Count)).Should().Be(0);
        Fixture.Camera.Started.Should().BeEmpty();
        Fixture.Camera.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Camera_start_failure_is_reported_in_status()
    {
        await StartLiveAsync();
        Fixture.Camera.FailingCamera = CameraFixture.SecondCamera;
        await Page.GetByTestId("Camera").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = CameraFixture.SecondCamera, Exact = true }).ClickAsync();
        await Expect(Status).ToHaveTextAsync($"Could not start '{CameraFixture.SecondCamera}': The fixture camera is busy.");
        await Expect(Button("Take Photo")).ToBeDisabledAsync();
        Fixture.Camera.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Take_photo_is_disabled_until_the_first_frame_arrives()
    {
        await Expect(Status).ToHaveTextAsync(LiveFirst);
        await Expect(Button("Take Photo")).ToBeDisabledAsync();
        Fixture.Camera.EmitFrame();
        await Expect(Button("Take Photo")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Switching_camera_restarts_capture_on_the_new_device()
    {
        await StartLiveAsync();
        await Page.GetByTestId("Camera").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = CameraFixture.SecondCamera, Exact = true }).ClickAsync();
        await Expect(Status).ToHaveTextAsync("Live: " + CameraFixture.SecondCamera);
        string.Join(",", Fixture.Camera.Started).Should().Be(CameraFixture.FirstCamera + "," + CameraFixture.SecondCamera);
        Fixture.Camera.Current.FriendlyName.Should().Be(CameraFixture.SecondCamera);
        await Expect(Button("Take Photo")).ToBeDisabledAsync();
        Fixture.Camera.EmitFrame();
        await Expect(Button("Take Photo")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Capture_preview_shows_the_canned_frame_mirrored()
    {
        await StartLiveAsync();
        // The frame is red on its left and blue on its right; the preview is a mirror.
        var (left, right) = await FrameHalvesAsync(MainCanvas);
        IsBlue(left).Should().BeTrue($"the mirrored preview should be blue on the left, not {left}");
        IsRed(right).Should().BeTrue($"the mirrored preview should be red on the right, not {right}");
        await SnapshotAsync("WebcamPainter-preview");
    }

    [Fact]
    public async Task Take_photo_enters_paint_mode_with_red_active()
    {
        await TakePhotoAsync();
        await Expect(Status).ToHaveTextAsync(PaintStatus);
        await Expect(SelfView).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("Camera")).ToBeDisabledAsync();
        await Expect(Button("Take Photo")).ToBeHiddenAsync();
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save…")).ToBeDisabledAsync();
        await Expect(Button("Back")).ToBeEnabledAsync();
        await Expect(Button("Violet")).ToBeVisibleAsync();
        Fixture.Tracker.IsRunning.Should().BeTrue();
        Fixture.Tracker.StartCount.Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Session.ActiveColorName)).Should().Be("Red");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await SnapshotAsync("WebcamPainter-paint-mode");
    }

    [Fact]
    public async Task Open_palm_report_paints_a_stroke_and_enables_clear_and_save()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Session.IsStrokeActive)).Should().BeFalse();
        await Expect(Button("Clear")).ToBeEnabledAsync();
        await Expect(Button("Save…")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.HasDrawing)).Should().BeTrue();
    }

    [Fact]
    public async Task Closed_hand_moves_crosshair_without_painting()
    {
        await TakePhotoAsync();
        // The still is mirrored, so the palm at 0.75 of the camera frame is over the blue quarter at 0.25.
        IsBlue(await PaintingPixelAsync(0.25f, 0.5f)).Should().BeTrue();
        Fixture.Tracker.Report(false, 0.75f, 0.5f);
        var crosshair = await PaintingPixelAsync(0.25f, 0.5f);
        (crosshair.Red > 150 && crosshair.Green > 150 && crosshair.Blue > 150)
            .Should().BeTrue($"the white crosshair should cross the hand position, not {crosshair}");
        Fixture.Tracker.LoseHand();
        IsBlue(await PaintingPixelAsync(0.25f, 0.5f)).Should().BeTrue("the crosshair goes when the hand is lost");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save…")).ToBeDisabledAsync();
    }

    [Theory]
    [InlineData("Red")]
    [InlineData("Orange")]
    [InlineData("Yellow")]
    [InlineData("Green")]
    [InlineData("Blue")]
    [InlineData("Indigo")]
    [InlineData("Violet")]
    public async Task Selecting_each_highlighter_updates_the_active_color_text(string color)
    {
        await TakePhotoAsync();
        await Button(color).ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: " + color);
        (await Page.EvaluateAsync(() => Fixture.Session.ActiveColorName)).Should().Be(color);
    }

    [Theory]
    [InlineData("Red")]
    [InlineData("Green")]
    [InlineData("Blue")]
    public async Task Stroke_tints_the_canvas_with_the_active_color(string color)
    {
        Fixture.Camera.SetFrame(CameraFixture.DefaultWidth, CameraFixture.DefaultHeight, SKColors.White, SKColors.White);
        await TakePhotoAsync();
        await Button(color).ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: " + color);
        IsWhite(await PaintingPixelAsync(0.5f, 0.5f)).Should().BeTrue();
        await PaintStrokeAsync(0.5f);
        Fixture.Tracker.LoseHand();
        // Highlighter ink is translucent: over the white still only its dominant channel is certain.
        var ink = await PaintingPixelAsync(0.5f, 0.5f);
        var (dominant, others) = color switch
        {
            "Red" => (ink.Red, new[] { ink.Green, ink.Blue }),
            "Green" => (ink.Green, new[] { ink.Red, ink.Blue }),
            _ => (ink.Blue, new[] { ink.Red, ink.Green }),
        };
        others.All(channel => dominant > channel + 40).Should().BeTrue($"the {color} highlighter should show at the stroke, not {ink}");
        IsWhite(await PaintingPixelAsync(0.5f, 0.15f)).Should().BeTrue("the still away from the stroke stays unpainted");
        await SnapshotAsync("WebcamPainter-stroke-" + color);
    }

    [Fact]
    public async Task Save_writes_a_jpeg_at_the_photo_resolution()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        var path = NewJpegPath();
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save…").ClickAsync();
        await Expect(Dialog("Image saved")).ToContainTextAsync("Do you want to clear the painting?");
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("webcam_painting.jpg");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        await Expect(Status).ToHaveTextAsync("Saved: " + path);
        using (var image = SKBitmap.Decode(path))
        {
            image.Width.Should().Be(CameraFixture.DefaultWidth);
            image.Height.Should().Be(CameraFixture.DefaultHeight);
        }
        await SnapshotAsync("WebcamPainter-saved");
        await AnswerAsync("Image saved", "No");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        await Expect(Button("Save…")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Answering_yes_after_save_clears_the_painting()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        Fixture.Application.FilePickers.EnqueueSaveFile(NewJpegPath());
        await Button("Save…").ClickAsync();
        await AnswerAsync("Image saved", "Yes");
        await Expect(Button("Save…")).ToBeDisabledAsync();
        await Expect(Button("Clear")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: Red");
        await Expect(Status).ToContainTextAsync("Saved: ");
    }

    [Fact]
    public async Task Cancelled_save_picker_writes_nothing()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        var files = Directory.GetFiles(Fixture.DataDirectory);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Save…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1,
            description: "the save picker request");
        await Expect(Button("Save…")).ToBeEnabledAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(Status).ToHaveTextAsync(PaintStatus);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        Directory.GetFiles(Fixture.DataDirectory).Should().BeEquivalentTo(files);
    }

    [Fact]
    public async Task Declining_replace_keeps_the_existing_file()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        var path = NewJpegPath();
        await File.WriteAllTextAsync(path, "Keep this file", TestContext.Current.CancellationToken);
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save…").ClickAsync();
        await Expect(Dialog("Replace existing file?")).ToContainTextAsync("Do you want to replace it?");
        await AnswerAsync("Replace existing file?", "No");
        await Expect(Status).ToHaveTextAsync("Save cancelled - the existing file was kept.");
        File.ReadAllText(path).Should().Be("Keep this file");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        await Expect(Button("Save…")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Save_failure_reports_error_and_recovers()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        // A read-only file cannot be replaced on any OS, so the write itself fails after
        // the user agrees to replace it.
        var path = NewJpegPath();
        await File.WriteAllTextAsync(path, "Read-only file", TestContext.Current.CancellationToken);
        File.SetAttributes(path, FileAttributes.ReadOnly);
        try
        {
            Fixture.Application.FilePickers.EnqueueSaveFile(path);
            await Button("Save…").ClickAsync();
            await AnswerAsync("Replace existing file?", "Yes");
            var error = Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "Error while saving the painted photo:" });
            await Expect(error).ToBeVisibleAsync();
            await Expect(Status).ToHaveTextAsync("Saving failed.");
            await error.GetByRole(AriaRole.Button).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
            await Expect(Button("Save…")).ToBeEnabledAsync();
            File.ReadAllText(path).Should().Be("Read-only file");
            (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        }
        finally { File.SetAttributes(path, FileAttributes.Normal); }
    }

    [Fact]
    public async Task Back_without_drawing_returns_to_capture_without_prompt()
    {
        await TakePhotoAsync();
        await Button("Back").ClickAsync();
        await Expect(Status).ToHaveTextAsync(LiveFirst);
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(Button("Take Photo")).ToBeVisibleAsync();
        await Expect(Button("Back")).ToBeHiddenAsync();
        await Expect(SelfView).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("Camera")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Session == null)).Should().BeTrue();
        Fixture.Tracker.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Back_with_drawing_asks_and_no_keeps_the_painting()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        await Button("Back").ClickAsync();
        await Expect(Dialog("Discard painting?")).ToContainTextAsync("Going back to the camera will discard your painting.");
        await AnswerAsync("Discard painting?", "No");
        await Expect(Button("Back")).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync(PaintStatus);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        Fixture.Tracker.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task Back_with_drawing_asks_and_yes_discards_the_painting()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.5f);
        await Button("Back").ClickAsync();
        await AnswerAsync("Discard painting?", "Yes");
        await Expect(Status).ToHaveTextAsync(LiveFirst);
        await Expect(Button("Take Photo")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Session == null)).Should().BeTrue();
        Fixture.Tracker.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Back_then_take_photo_starts_a_fresh_session()
    {
        await TakePhotoAsync();
        await Button("Blue").ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: Blue");
        await PaintStrokeAsync(0.5f);
        await Button("Back").ClickAsync();
        await AnswerAsync("Discard painting?", "Yes");
        await Expect(Button("Take Photo")).ToBeEnabledAsync();
        await Button("Take Photo").ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: Red");
        await Fixture.Application.WaitForAsync(() => Fixture.Session != null, ready => ready,
            description: "the new painting session");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Button("Clear")).ToBeDisabledAsync();
        Fixture.Tracker.StartCount.Should().Be(2);
        Fixture.Tracker.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task Clear_with_two_strokes_needs_no_confirmation()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.3f);
        await PaintStrokeAsync(0.6f);
        await Button("Clear").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Cleared - paint something new.");
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save…")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Clear_with_three_strokes_asks_first()
    {
        await TakePhotoAsync();
        await PaintStrokeAsync(0.2f);
        await PaintStrokeAsync(0.5f);
        await PaintStrokeAsync(0.8f);
        await Button("Clear").ClickAsync();
        await Expect(Dialog("Confirm")).ToContainTextAsync("Are you sure you want to clear your painting and start over?");
        await AnswerAsync("Confirm", "No");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(3);
        await Expect(Status).ToHaveTextAsync(PaintStatus);
        await Button("Clear").ClickAsync();
        await AnswerAsync("Confirm", "Yes");
        await Expect(Status).ToHaveTextAsync("Cleared - paint something new.");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Button("Clear")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Self_view_shows_live_frames_in_paint_mode()
    {
        await TakePhotoAsync();
        Fixture.Tracker.SubmittedFrames.Should().Be(0);
        Fixture.Camera.EmitFrame();
        await Fixture.Application.WaitForAsync(() => Fixture.Tracker.SubmittedFrames, count => count == 1,
            description: "the frame handed to the tracker");
        var (left, right) = await FrameHalvesAsync(SelfView);
        IsBlue(left).Should().BeTrue($"the mirrored self-view should be blue on the left, not {left}");
        IsRed(right).Should().BeTrue($"the mirrored self-view should be red on the right, not {right}");
    }
}
