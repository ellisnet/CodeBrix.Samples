using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Imaging.Drawing;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PainDiagram.ViewModels;
using PainDiagram.Views;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PainDiagram.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string InitialStatus = "Select Pain, Numbness, or Tingling - then draw on the body map with the left mouse button.";
    private Locator Canvas => Page.GetByType<DrawingCanvas>();
    private Locator Status => Page.GetByTestId("StatusText");
    private Locator Dialog(string title) => Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = title });

    // The aspect-fit square the body map occupies, in the canvas's own coordinates.
    private Task<SKRect> DrawingRectAsync() =>
        Page.EvaluateAsync(() => Fixture.Session.GetDrawingRect(Fixture.Canvas.GetViewSize()));

    // A vertical stroke down the blank gap between the two figures (normalized x = 0.5),
    // drawn with real pointer input: press, ten moves and release.
    private async Task DrawStrokeAsync(float top = 0.3f, float length = 0.2f)
    {
        var count = await Page.EvaluateAsync(() => Fixture.Session.StrokeCount);
        var rect = await DrawingRectAsync();
        await Canvas.DragByAsync(0, rect.Height * length,
            new() { Position = new() { X = rect.MidX, Y = rect.Top + rect.Height * top } });
        await Fixture.Application.WaitForAsync(() => Fixture.Session.StrokeCount, value => value == count + 1,
            description: "the committed stroke");
    }

    private async Task AnswerAsync(string title, string button)
    {
        await Dialog(title).GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await Expect(Dialog(title)).ToHaveCountAsync(0);
    }

    private async Task<SKColor> ScreenPixelAsync(float normalizedX, float normalizedY)
    {
        var box = await Canvas.BoundingBoxAsync();
        var rect = await DrawingRectAsync();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        return bitmap.GetPixel((int)(box.X + rect.Left + rect.Width * normalizedX),
            (int)(box.Y + rect.Top + rect.Height * normalizedY));
    }

    // Highlighter ink is translucent, so over the white body map each layer keeps its own hue.
    private static bool IsPainInk(SKColor pixel) => pixel.Red > pixel.Green + 40 && pixel.Blue > pixel.Green + 40;
    private static bool IsWhite(SKColor pixel) => pixel.Red > 245 && pixel.Green > 245 && pixel.Blue > 245;

    private string NewPngPath() => Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".png");

    [Fact]
    public async Task Launch_shows_body_map_with_pain_selected_and_save_clear_disabled()
    {
        await Expect(Page.GetByText("Pain Diagram", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("✓ Pain");
        await Expect(Page.GetByTestId("NumbnessLayer")).ToHaveTextAsync("Numbness");
        await Expect(Page.GetByTestId("TinglingLayer")).ToHaveTextAsync("Tingling");
        await Expect(Canvas).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync(InitialStatus);
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ActiveLayerName)).Should().Be(MainViewModel.PainLayerName);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await SnapshotAsync("PainDiagram-start");
    }

    [Fact]
    public async Task Layer_buttons_move_the_check_mark_and_active_layer()
    {
        await Page.GetByTestId("NumbnessLayer").ClickAsync();
        await Expect(Page.GetByTestId("NumbnessLayer")).ToHaveTextAsync("✓ Numbness");
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("Pain");
        (await Page.EvaluateAsync(() => Fixture.Session.ActiveLayer.Name)).Should().Be(MainViewModel.NumbnessLayerName);
        await Page.GetByTestId("TinglingLayer").ClickAsync();
        await Expect(Page.GetByTestId("TinglingLayer")).ToHaveTextAsync("✓ Tingling");
        await Expect(Page.GetByTestId("NumbnessLayer")).ToHaveTextAsync("Numbness");
        (await Page.EvaluateAsync(() => Fixture.Model.ActiveLayerName)).Should().Be(MainViewModel.TinglingLayerName);
        await Page.GetByTestId("PainLayer").ClickAsync();
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("✓ Pain");
        await Expect(Page.GetByTestId("TinglingLayer")).ToHaveTextAsync("Tingling");
        (await Page.EvaluateAsync(() => Fixture.Session.ActiveLayer.Name)).Should().Be(MainViewModel.PainLayerName);
    }

    [Fact]
    public async Task Drag_on_canvas_adds_one_stroke_to_active_layer()
    {
        await DrawStrokeAsync();
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.PainLayerName).ElementCount)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Session.IsPointerActive)).Should().BeFalse();
        await Expect(Button("Save")).ToBeEnabledAsync();
        await Expect(Button("Clear")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.HasDrawing)).Should().BeTrue();
    }

    [Fact]
    public async Task Click_on_canvas_commits_a_single_point_stroke()
    {
        var rect = await DrawingRectAsync();
        await Canvas.ClickAsync(new() { Position = new() { X = rect.MidX, Y = rect.MidY } });
        await Fixture.Application.WaitForAsync(() => Fixture.Session.StrokeCount, value => value == 1,
            description: "the committed dot");
        await Expect(Button("Save")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Mouse_polyline_draws_one_stroke()
    {
        var box = await Canvas.BoundingBoxAsync();
        var rect = await DrawingRectAsync();
        float X(float nx) => box.X + rect.Left + rect.Width * nx;
        float Y(float ny) => box.Y + rect.Top + rect.Height * ny;
        await Page.Mouse.MoveAsync(X(0.48f), Y(0.3f));
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(X(0.52f), Y(0.35f));
        await Page.Mouse.MoveAsync(X(0.48f), Y(0.4f));
        await Page.Mouse.MoveAsync(X(0.52f), Y(0.45f));
        (await Page.EvaluateAsync(() => Fixture.Session.IsPointerActive)).Should().BeTrue();
        await Page.Mouse.UpAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Session.StrokeCount, value => value == 1,
            description: "the committed polyline");
        (await Page.EvaluateAsync(() => Fixture.Session.IsPointerActive)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.PainLayerName).ElementCount)).Should().Be(1);
    }

    [Fact]
    public async Task Strokes_land_on_the_selected_layer()
    {
        await Page.GetByTestId("NumbnessLayer").ClickAsync();
        await Expect(Page.GetByTestId("NumbnessLayer")).ToHaveTextAsync("✓ Numbness");
        await DrawStrokeAsync(0.2f);
        await Page.GetByTestId("TinglingLayer").ClickAsync();
        await Expect(Page.GetByTestId("TinglingLayer")).ToHaveTextAsync("✓ Tingling");
        await DrawStrokeAsync(0.5f);
        await DrawStrokeAsync(0.6f);
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.PainLayerName).ElementCount)).Should().Be(0);
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.NumbnessLayerName).ElementCount)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.TinglingLayerName).ElementCount)).Should().Be(2);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(3);
    }

    [Fact]
    public async Task Press_outside_drawing_area_adds_no_stroke()
    {
        var rect = await DrawingRectAsync();
        // The canvas is never square here, so its top-left corner is in the letterbox
        // (left of the body map in landscape, above it in portrait).
        (rect.Left > 10 || rect.Top > 10).Should().BeTrue();
        await Canvas.DragByAsync(rect.Left > 10 ? 0 : 5, rect.Left > 10 ? 5 : 0, new() { Position = new() { X = 2, Y = 2 } });
        await Canvas.ClickAsync(new() { Position = new() { X = 2, Y = 2 } });
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        (await Page.EvaluateAsync(() => Fixture.Session.IsPointerActive)).Should().BeFalse();
        await Expect(Button("Save")).ToBeDisabledAsync();
        await Expect(Button("Clear")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Clear_with_few_strokes_clears_without_confirmation()
    {
        await Page.GetByTestId("TinglingLayer").ClickAsync();
        await DrawStrokeAsync(0.2f);
        await DrawStrokeAsync(0.5f);
        await Button("Clear").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Cleared - draw a new diagram.");
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("✓ Pain");
        (await Page.EvaluateAsync(() => Fixture.Model.ActiveLayerName)).Should().Be(MainViewModel.PainLayerName);
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Clear_with_many_strokes_asks_and_no_keeps_drawing()
    {
        await Page.GetByTestId("NumbnessLayer").ClickAsync();
        await DrawStrokeAsync(0.1f);
        await DrawStrokeAsync(0.4f);
        await DrawStrokeAsync(0.7f);
        await Button("Clear").ClickAsync();
        await Expect(Dialog("Confirm")).ToContainTextAsync("Are you sure you want to clear and start over?");
        await AnswerAsync("Confirm", "No");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(3);
        await Expect(Page.GetByTestId("NumbnessLayer")).ToHaveTextAsync("✓ Numbness");
        await Expect(Status).ToHaveTextAsync(InitialStatus);
        await Expect(Button("Clear")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Clear_with_many_strokes_asks_and_yes_clears()
    {
        await Page.GetByTestId("NumbnessLayer").ClickAsync();
        await DrawStrokeAsync(0.1f);
        await DrawStrokeAsync(0.4f);
        await DrawStrokeAsync(0.7f);
        await Button("Clear").ClickAsync();
        await AnswerAsync("Confirm", "Yes");
        await Expect(Status).ToHaveTextAsync("Cleared - draw a new diagram.");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("✓ Pain");
        await Expect(Button("Clear")).ToBeDisabledAsync();
        await Expect(Button("Save")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Save_writes_png_and_offers_to_clear()
    {
        await DrawStrokeAsync();
        var path = NewPngPath();
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save").ClickAsync();
        await Expect(Dialog("Image saved")).ToContainTextAsync("Do you want to clear the drawing?");
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("pain_diagram.png");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        await Expect(Status).ToHaveTextAsync("Saved: " + path);
        using (var image = SKBitmap.Decode(path))
        {
            image.Width.Should().Be(1000);
            image.Height.Should().Be(1000);
        }
        await SnapshotAsync("PainDiagram-saved");
        await AnswerAsync("Image saved", "No");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        await Expect(Button("Save")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Save_cancelled_in_picker_changes_nothing()
    {
        await DrawStrokeAsync();
        var files = Directory.GetFiles(Fixture.DataDirectory);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Save").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1,
            description: "the save picker request");
        await Expect(Button("Save")).ToBeEnabledAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(Status).ToHaveTextAsync(InitialStatus);
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        Directory.GetFiles(Fixture.DataDirectory).Should().BeEquivalentTo(files);
    }

    [Fact]
    public async Task Save_over_existing_file_asks_and_no_keeps_file()
    {
        await DrawStrokeAsync();
        var path = NewPngPath();
        await File.WriteAllTextAsync(path, "Keep this file", TestContext.Current.CancellationToken);
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save").ClickAsync();
        await Expect(Dialog("Replace existing file?")).ToContainTextAsync("Do you want to replace it?");
        await AnswerAsync("Replace existing file?", "No");
        await Expect(Status).ToHaveTextAsync("Save cancelled - the existing file was kept.");
        File.ReadAllText(path).Should().Be("Keep this file");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        await Expect(Button("Save")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Save_over_existing_file_yes_replaces_it()
    {
        await DrawStrokeAsync();
        var path = NewPngPath();
        await File.WriteAllTextAsync(path, "Replace this file", TestContext.Current.CancellationToken);
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save").ClickAsync();
        await AnswerAsync("Replace existing file?", "Yes");
        await Expect(Dialog("Image saved")).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Saved: " + path);
        using (var image = SKBitmap.Decode(path))
        {
            image.Width.Should().Be(1000);
            image.Height.Should().Be(1000);
        }
        await AnswerAsync("Image saved", "No");
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
    }

    [Fact]
    public async Task Save_then_yes_clears_drawing()
    {
        await Page.GetByTestId("TinglingLayer").ClickAsync();
        await DrawStrokeAsync();
        Fixture.Application.FilePickers.EnqueueSaveFile(NewPngPath());
        await Button("Save").ClickAsync();
        await AnswerAsync("Image saved", "Yes");
        await Expect(Button("Save")).ToBeDisabledAsync();
        await Expect(Button("Clear")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(0);
        await Expect(Page.GetByTestId("PainLayer")).ToHaveTextAsync("✓ Pain");
        await Expect(Status).ToContainTextAsync("Saved: ");
    }

    [Fact]
    public async Task Save_failure_reports_error_dialog()
    {
        await DrawStrokeAsync();
        // A read-only file cannot be replaced on any OS, so the write itself fails after
        // the user agrees to replace it.
        var path = NewPngPath();
        await File.WriteAllTextAsync(path, "Read-only file", TestContext.Current.CancellationToken);
        File.SetAttributes(path, FileAttributes.ReadOnly);
        try
        {
            Fixture.Application.FilePickers.EnqueueSaveFile(path);
            await Button("Save").ClickAsync();
            await AnswerAsync("Replace existing file?", "Yes");
            var error = Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "Error while saving the Pain Diagram image:" });
            await Expect(error).ToBeVisibleAsync();
            await Expect(Status).ToHaveTextAsync("Saving failed.");
            await error.GetByRole(AriaRole.Button).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
            await Expect(Button("Save")).ToBeEnabledAsync();
            File.ReadAllText(path).Should().Be("Read-only file");
            (await Page.EvaluateAsync(() => Fixture.Session.StrokeCount)).Should().Be(1);
        }
        finally { File.SetAttributes(path, FileAttributes.Normal); }
    }

    [Fact]
    public async Task Drawn_stroke_is_visible_in_screenshot_pixels()
    {
        IsWhite(await ScreenPixelAsync(0.5f, 0.4f)).Should().BeTrue("the gap between the figures is blank body map");
        await DrawStrokeAsync(0.3f, 0.2f);
        var ink = await ScreenPixelAsync(0.5f, 0.4f);
        IsPainInk(ink).Should().BeTrue($"the Pain highlighter should show at the stroke, not {ink}");
        IsWhite(await ScreenPixelAsync(0.5f, 0.8f)).Should().BeTrue("the canvas away from the stroke stays blank");
        await SnapshotAsync("PainDiagram-stroke");
    }

    [Fact]
    public async Task Saved_png_contains_layer_ink()
    {
        await DrawStrokeAsync(0.3f, 0.2f);
        var path = NewPngPath();
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save").ClickAsync();
        await Expect(Dialog("Image saved")).ToBeVisibleAsync();
        using (var image = SKBitmap.Decode(path))
        {
            var ink = image.GetPixel(500, 400);
            IsPainInk(ink).Should().BeTrue($"the saved image should carry the Pain ink, not {ink}");
            IsWhite(image.GetPixel(500, 800)).Should().BeTrue();
            IsWhite(image.GetPixel(2, 2)).Should().BeTrue();
        }
        await AnswerAsync("Image saved", "No");
    }
}
