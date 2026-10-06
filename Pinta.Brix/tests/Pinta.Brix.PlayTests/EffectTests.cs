using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class EffectTests(AppFixture fixture) : PintaTest(fixture)
{
    // The effect options dialog floats in a Popup rather than a ContentDialog, so it is found by its title.
    private Locator FloatingButton(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private Task WaitForRenderAsync() => WaitAsync(() => PintaCore.LivePreview.IsEnabled, enabled => !enabled, "the live preview finished");

    [Fact]
    public async Task Invert_colors_adjustment_changes_pixels_and_undoes()
    {
        IsWhite(await PixelAsync(10, 10)).Should().BeTrue();
        await MenuAsync("Adjustments", "Invert Colors");
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Expect(HistoryRows.Last).ToContainTextAsync("Invert Colors");
        await WaitForRenderAsync();
        IsBlack(await PixelAsync(10, 10)).Should().BeTrue();
        IsBlack(await PixelAsync(799, 599)).Should().BeTrue();
        await ToolbarButton("Undo").ClickAsync();
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 0, "the inversion undone");
        IsWhite(await PixelAsync(10, 10)).Should().BeTrue();
    }

    [Fact]
    public async Task Gaussian_blur_dialog_ok_applies_and_cancel_restores()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await Expect(HistoryRows).ToHaveCountAsync(2);

        await MenuAsync("Effects", "Blurs", "Gaussian Blur...");
        await Expect(Page.GetByText("Gaussian Blur", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Slider, new() { Name = "Radius", Exact = true })).ToBeVisibleAsync();
        await FloatingButton("Cancel").ClickAsync();
        await Expect(Page.GetByText("Gaussian Blur", new() { Exact = true })).ToBeHiddenAsync();
        await WaitForRenderAsync();
        await Expect(HistoryRows).ToHaveCountAsync(2);
        IsWhite(await PixelAsync(150, 99)).Should().BeTrue();
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();

        await MenuAsync("Effects", "Blurs", "Gaussian Blur...");
        await FloatingButton("OK").ClickAsync();
        await WaitForRenderAsync();
        await Expect(HistoryRows).ToHaveCountAsync(3);
        await Expect(HistoryRows.Last).ToContainTextAsync("Gaussian Blur");
        // The one-pixel line spreads into its neighbours and loses its full strength.
        var above = await PixelAsync(150, 99);
        above.R.Should().BeLessThan(240);
        (await PixelAsync(150, 100)).R.Should().BeGreaterThan(16);
    }

    [Fact]
    public async Task Posterize_dialog_cancel_adds_nothing_and_ok_adds_a_history_row()
    {
        await MenuAsync("Adjustments", "Posterize...");
        await Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Linked", Exact = true })).ToBeCheckedAsync();
        await FloatingButton("Cancel").ClickAsync();
        await WaitForRenderAsync();
        await Expect(HistoryRows).ToHaveCountAsync(1);
        await MenuAsync("Adjustments", "Posterize...");
        await FloatingButton("OK").ClickAsync();
        await WaitForRenderAsync();
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Expect(HistoryRows.Last).ToContainTextAsync("Posterize");
    }

    [Fact]
    public async Task Levels_and_curves_dialogs_open_and_cancel_without_a_change()
    {
        await MenuAsync("Adjustments", "Levels...");
        await Expect(Page.GetByText("Levels Adjustment", new() { Exact = true })).ToBeVisibleAsync();
        await FloatingButton("Cancel").ClickAsync();
        await WaitForRenderAsync();
        await MenuAsync("Adjustments", "Curves...");
        await Expect(Page.GetByText("Transfer Map", new() { Exact = true })).ToBeVisibleAsync();
        await FloatingButton("Cancel").ClickAsync();
        await WaitForRenderAsync();
        await Expect(HistoryRows).ToHaveCountAsync(1);
        IsWhite(await PixelAsync(10, 10)).Should().BeTrue();
    }
}
