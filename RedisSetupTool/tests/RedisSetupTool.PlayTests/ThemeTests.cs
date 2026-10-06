using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task App_keeps_its_dark_palette_under_either_os_theme()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        // RootGrid pins Dark, so the simulated OS theme must not reach the page palette.
        (await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.FindName("RootGrid")).ActualTheme)).Should().Be(ElementTheme.Dark);
        await Expect(Page.GetByText("Docker 29.0-fixture · API 1.52", new() { Exact = true })).ToBeVisibleAsync();
        var png = await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine("TestResults", "PlayTest", $"RedisSetupTool-{expected}.png"),
        });
        using var bitmap = SKBitmap.Decode(png);
        // The bottom-right corner is bare page background (#16181D) in both orientations.
        bitmap.GetPixel(bitmap.Width - 4, bitmap.Height - 4).Should().Be(new SKColor(0x16, 0x18, 0x1D));
    }
}
