using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace WebcamViewer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Simulated_OS_theme_reaches_page_background_and_pixels()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        var dark = expected == ApplicationTheme.Dark;
        await DiscoverAsync(CameraFixture.Camera(CameraA));
        var pageBackground = await Page.EvaluateAsync(() =>
        {
            Fixture.View.ActualTheme.Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
            return ((SolidColorBrush)Fixture.View.Background).Color;
        });
        foreach (var channel in new[] { pageBackground.R, pageBackground.G, pageBackground.B })
        {
            if (dark) channel.Should().BeLessThan((byte)64);
            else channel.Should().BeGreaterThan((byte)192);
        }
        pageBackground.A.Should().Be((byte)255);
        var png = await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine("TestResults", "PlayTest", $"WebcamViewer-{expected}.png"),
        });
        using var bitmap = SKBitmap.Decode(png);
        // The page has a 12px padding, so this pixel is its theme background.
        bitmap.GetPixel(5, 5).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
        // The video area stays black in both themes until a frame arrives.
        var center = await CanvasPointAsync(0.5, 0.5);
        bitmap.GetPixel(center.X, center.Y).Should().Be(SKColors.Black);
    }
}
