using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace DRAKON.Brix.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Simulated_theme_leaves_the_Tk_surface_classic()
    {
        // The simulated OS theme is fixed per process; the runners cover both.
        var dark = Fixture.Application.SystemTheme == ApplicationTheme.Dark;
        (await Page.EvaluateAsync(() => Fixture.View.ActualTheme)).Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
        (await Page.EvaluateAsync(() => Host.ThemePalette.Name)).Should().Be("Classic");
        // Empty toolbar background between the Folder button and the Back arrow.
        var point = (await Fixture.TclAsync(
            "set f .root.pnd.left.nav.folder; set b .root.pnd.left.nav.back; " +
            "list [expr {([winfo rootx $f] + [winfo width $f] + [winfo rootx $b]) / 2}] [expr {[winfo rooty $f] + [winfo height $f] / 2}]"))
            .Split(' ').Select(int.Parse).ToArray();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        var scale = (float)bitmap.Width / Fixture.Application.Width;
        var pixel = bitmap.GetPixel((int)(point[0] * scale), (int)(point[1] * scale));
        // Tk's classic background, #d9d9d9, in both themes.
        new[] { pixel.Red, pixel.Green, pixel.Blue }.All(channel => Math.Abs(channel - 0xd9) <= 3).Should().BeTrue();
        await SnapshotAsync($"DRAKON.Brix-classic-{(dark ? "dark" : "light")}");
    }
}
