using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;
using Color = Pinta.Brix.Engine.Drawing.Color;

namespace Pinta.Brix.PlayTests;

public sealed class PaletteTests(AppFixture fixture) : PintaTest(fixture)
{
    private Locator Palette => Page.GetByType<PaletteWidget>();
    private Task<(Color Primary, Color Secondary)> ColorsAsync() =>
        EvaluateAsync(() => (PintaCore.Palette.PrimaryColor, PintaCore.Palette.SecondaryColor));

    [Fact]
    public async Task X_key_and_swap_arrow_exchange_primary_and_secondary_colors()
    {
        var (primary, secondary) = await ColorsAsync();
        primary.Should().NotBe(secondary);
        await SelectToolAsync("Pan");
        await Canvas.ClickAsync(new() { Position = new() { X = 40, Y = 40 } });
        await Page.Keyboard.PressAsync("x");
        (await ColorsAsync()).Should().Be((secondary, primary));
        // The little arrow beside the swatches does the same.
        await Palette.ClickAsync(new() { Position = new() { X = 34, Y = 9 } });
        (await ColorsAsync()).Should().Be((primary, secondary));
    }

    [Fact]
    public async Task Primary_swatch_opens_the_color_dialog_and_cancel_keeps_the_color()
    {
        var before = await ColorsAsync();
        await Palette.ClickAsync(new() { Position = new() { X = 8, Y = 8 } });
        await Expect(Page.GetByText("Primary Color", new() { Exact = true })).ToBeVisibleAsync();
        await AnswerAsync("Recently used", "Cancel");
        (await ColorsAsync()).Should().Be(before);
    }

    [Fact]
    public async Task Palette_size_dialog_resizes_and_save_and_open_round_trip()
    {
        var path = TestPath("palette.gpl");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await MenuAsync("Edit", "Palette", "Save As...");
        await WaitAsync(() => File.Exists(path) && new FileInfo(path).Length > 0, written => written, "the saved palette");
        var count = await EvaluateAsync(() => PintaCore.Palette.CurrentPalette.Colors.Count);

        await MenuAsync("Edit", "Palette", "Set Number of Colors");
        await SetNumberAsync("Number of colors:", 0, "8");
        await AnswerAsync("Number of colors:", "OK");
        await WaitAsync(() => PintaCore.Palette.CurrentPalette.Colors.Count, colors => colors == 8, "the eight-colour palette");

        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await MenuAsync("Edit", "Palette", "Open...");
        await WaitAsync(() => PintaCore.Palette.CurrentPalette.Colors.Count, colors => colors == count, "the saved palette loaded");
    }
}
