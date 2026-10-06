using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task App_stays_dark_under_light_and_dark_system_theme()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        // The page pins its own dark palette, whichever theme the simulated OS prefers.
        (await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.FindName("RootGrid")).ActualTheme)).Should().Be(ElementTheme.Dark);
        await Expect(Page.GetByText("3 models", new() { Exact = true })).ToBeVisibleAsync();
        var png = await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine("TestResults", "PlayTest", $"PolyHavenBrowser-{expected}.png"),
        });
        using var bitmap = SKBitmap.Decode(png);
        // Below the last catalog row, with no download running, the window background shows through.
        bitmap.GetPixel(4, bitmap.Height - 4).Should().Be(new SKColor(0x16, 0x18, 0x1D));
    }
}
