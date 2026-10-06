using System.Linq;
using System.Threading.Tasks;
using Pinta.Brix.Engine;
using SilverAssertions;
using Windows.ApplicationModel.DataTransfer;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ClipboardTests(AppFixture fixture) : PintaTest(fixture)
{
    private async Task DrawAndCopyAllAsync()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await MenuAsync("Edit", "Select All");
        await Expect(Page.GetByTestId("SelectionSize")).ToHaveTextAsync("800 x 600");
        await MenuAsync("Edit", "Copy");
        await WaitAsync(() => Clipboard.GetContent().Contains(StandardDataFormats.Bitmap), copied => copied, "the copied bitmap");
    }

    [Fact]
    public async Task Select_all_copy_puts_bitmap_on_isolated_clipboard()
    {
        (await EvaluateAsync(() => Clipboard.GetContent().Contains(StandardDataFormats.Bitmap))).Should().BeFalse();
        await DrawAndCopyAllAsync();
        (await EvaluateAsync(() => Clipboard.GetContent().Contains(StandardDataFormats.Text))).Should().BeFalse();
        (await Page.ClipboardTextAsync()).Should().BeEmpty();
        // Copy leaves the document as it was.
        await Expect(HistoryRows).ToHaveCountAsync(3);
        await Expect(HistoryRows.Last).ToContainTextAsync("Select All");
    }

    [Fact]
    public async Task Paste_into_new_image_opens_second_tab()
    {
        await DrawAndCopyAllAsync();
        await MenuAsync("Edit", "Paste Into New Image");
        await Expect(Tabs).ToHaveCountAsync(2);
        await WaitAsync(() => PintaCore.Workspace.ActiveDocumentIndex, index => index == 1, "the pasted image active");
        await Expect(UnsavedTab(dirty: true).Last).ToBeVisibleAsync();
        (await EvaluateAsync(() => ActiveDocument.ImageSize)).Should().Be(new Size(800, 600));
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(150, 110)).Should().BeTrue();
    }

    [Fact]
    public async Task Paste_into_new_layer_adds_layer()
    {
        await DrawAndCopyAllAsync();
        await MenuAsync("Edit", "Paste Into New Layer");
        await Expect(LayerRows).ToHaveCountAsync(2);
        await Expect(HistoryRows.Last).ToContainTextAsync("Paste");
        (await EvaluateAsync(() => ActiveDocument.Layers.CurrentUserLayerIndex)).Should().Be(1);
        IsBlack(await PixelAsync(150, 100, layer: 1)).Should().BeTrue();
        (await EvaluateAsync(() => ActiveDocument.History.Items.Select(item => item.Text).ToArray()))
            .Should().Contain("Paste Into New Layer");
    }
}
