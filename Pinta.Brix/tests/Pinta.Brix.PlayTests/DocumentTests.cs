using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Pinta.Brix.Engine;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class DocumentTests(AppFixture fixture) : PintaTest(fixture)
{
    private Task WaitForFileAsync(string path) => WaitAsync(() => File.Exists(path) && new FileInfo(path).Length > 0,
        written => written, "the saved file");

    [Fact]
    public async Task Open_png_through_picker_adds_named_tab()
    {
        var path = WritePng("red.png", 64, 48, SKColors.Red);
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await MenuAsync("File", "Open...");
        await Expect(Tab("red.png")).ToBeVisibleAsync();
        await Expect(Tabs).ToHaveCountAsync(2);
        (await EvaluateAsync(() => ActiveDocument.ImageSize)).Should().Be(new Size(64, 48));
        (await EvaluateAsync(() => ActiveDocument.File)).Should().Be(path);
        (await EvaluateAsync(() => ActiveDocument.IsDirty)).Should().BeFalse();
        var pixel = await PixelAsync(10, 10);
        (pixel.R, pixel.G, pixel.B, pixel.A).Should().Be(((byte)255, (byte)0, (byte)0, (byte)255));
        await Expect(HistoryRows).ToHaveCountAsync(1);
        await Expect(HistoryRows).ToContainTextAsync("Open Image");
    }

    [Fact]
    public async Task Cancelled_open_leaves_workspace_unchanged()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(null);
        await MenuAsync("File", "Open...");
        await WaitAsync(() => Fixture.Application.FilePickers.OpenFileRequestCount, count => count == 1, "the open picker");
        await Expect(Tabs).ToHaveCountAsync(1);
        await Expect(UnsavedTab()).ToBeVisibleAsync();
        (await EvaluateAsync(() => PintaCore.Workspace.OpenDocuments.Count)).Should().Be(1);
    }

    [Fact]
    public async Task Save_as_png_writes_file_and_clears_dirty_marker()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await Expect(UnsavedTab(dirty: true)).ToBeVisibleAsync();
        var path = TestPath("drawing.png");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await MenuAsync("File", "Save As...");
        await Expect(Tab("drawing.png")).ToBeVisibleAsync();
        await WaitForFileAsync(path);
        (await EvaluateAsync(() => ActiveDocument.IsDirty)).Should().BeFalse();
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().MatchRegex(@"^Unsaved Image \d+$");
        using var saved = SKBitmap.Decode(path);
        (saved.Width, saved.Height).Should().Be((800, 600));
        saved.GetPixel(150, 100).Should().Be(SKColors.Black);
        saved.GetPixel(150, 110).Should().Be(SKColors.White);
    }

    [Fact]
    public async Task Saving_layered_image_to_png_prompts_flatten_and_cancel_keeps_dirty()
    {
        await LayersPadButton("Add New Layer").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(2);
        var path = TestPath("layered.png");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await MenuAsync("File", "Save As...");
        await Expect(Page.GetByText("Flatten Image?", new() { Exact = true })).ToBeVisibleAsync();
        await AnswerAsync("does not support layers", "Cancel");
        await Expect(UnsavedTab(dirty: true)).ToBeVisibleAsync();
        (await EvaluateAsync(() => ActiveDocument.HasFile)).Should().BeFalse();
        (await EvaluateAsync(() => ActiveDocument.IsDirty)).Should().BeTrue();
        await Expect(LayerRows).ToHaveCountAsync(2);
        // The picker made its placeholder; nothing was exported into it.
        new FileInfo(path).Length.Should().Be(0);
    }

    [Fact]
    public async Task Closing_dirty_tab_prompts_and_discard_closes_it()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await UnsavedTab(dirty: true).GetByRole(AriaRole.Button, new() { Name = "Close Tab", Exact = true }).ClickAsync();
        var prompt = new Regex(@"^Save changes to ""Unsaved Image \d+"" before closing\?$");
        await Expect(Page.GetByText(prompt)).ToBeVisibleAsync();
        await AnswerAsync("all changes will be permanently lost", "Close without saving");
        await Expect(Tabs).ToHaveCountAsync(0);
        (await EvaluateAsync(() => PintaCore.Workspace.HasOpenDocuments)).Should().BeFalse();
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(0);
        await Expect(ToolbarButton("Save")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Closing_an_inactive_tab_keeps_the_active_document()
    {
        var path = WritePng("red.png", 64, 48, SKColors.Red);
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await MenuAsync("File", "Open...");
        await Expect(Tab("red.png")).ToBeVisibleAsync();
        // The blank image is first and clean, so it closes without a prompt.
        await UnsavedTab().GetByRole(AriaRole.Button, new() { Name = "Close Tab", Exact = true }).ClickAsync();
        await Expect(Tabs).ToHaveCountAsync(1);
        await Expect(Tab("red.png")).ToBeVisibleAsync();
        (await EvaluateAsync(() => PintaCore.Workspace.ActiveDocumentIndex)).Should().Be(0);
        (await EvaluateAsync(() => ActiveDocument.File)).Should().Be(path);
        await Expect(LayerRows).ToHaveCountAsync(1);
        await Expect(HistoryRows).ToContainTextAsync("Open Image");
    }
}
