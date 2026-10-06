using System.Threading.Tasks;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class SelectionTests(AppFixture fixture) : PintaTest(fixture)
{
    private async Task SelectRectangleAsync()
    {
        await SelectToolAsync("Rectangle Select");
        await DragOnImageAsync(50, 50, 150, 130);
        await WaitAsync(() => ActiveDocument.Selection.Visible, visible => visible, "the rectangle selection");
    }

    [Fact]
    public async Task Rectangle_select_shows_its_size_and_erase_selection_clears_it()
    {
        await SelectRectangleAsync();
        await Expect(Page.GetByTestId("SelectionSize")).ToHaveTextAsync("100 x 80");
        await Expect(ToolbarButton("Deselect All")).ToBeEnabledAsync();
        await MenuAsync("Edit", "Erase Selection");
        await Expect(HistoryRows.Last).ToContainTextAsync("Erase Selection");
        IsTransparent(await PixelAsync(100, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(40, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(100, 140)).Should().BeTrue();
        await ToolbarButton("Deselect All").ClickAsync();
        await Expect(Page.GetByTestId("SelectionSize")).ToHaveTextAsync("");
        await Expect(ToolbarButton("Deselect All")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Fill_selection_paints_the_primary_color_and_invert_selection_flips_it()
    {
        await SelectRectangleAsync();
        await MenuAsync("Edit", "Fill Selection");
        await Expect(HistoryRows.Last).ToContainTextAsync("Fill Selection");
        IsBlack(await PixelAsync(100, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(10, 10)).Should().BeTrue();
        await MenuAsync("Edit", "Invert Selection");
        await Expect(HistoryRows.Last).ToContainTextAsync("Invert Selection");
        await MenuAsync("Edit", "Erase Selection");
        await Expect(HistoryRows.Last).ToContainTextAsync("Erase Selection");
        IsTransparent(await PixelAsync(10, 10)).Should().BeTrue();
        IsBlack(await PixelAsync(100, 100)).Should().BeTrue();
    }

    [Fact]
    public async Task Crop_to_selection_resizes_the_image_and_undo_restores_it()
    {
        await SelectRectangleAsync();
        await ToolbarButton("Crop to Selection").ClickAsync();
        await WaitAsync(() => ActiveDocument.ImageSize, size => size == new Size(100, 80), "the cropped image");
        await Expect(HistoryRows.Last).ToContainTextAsync("Crop to Selection");
        await ToolbarButton("Undo").ClickAsync();
        await WaitAsync(() => ActiveDocument.ImageSize, size => size == new Size(800, 600), "the full image back");
    }
}
