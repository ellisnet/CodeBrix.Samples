using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace WebcamPainter.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Page_follows_the_OS_theme_and_canvas_keeps_its_photo_colors()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        var dark = expected == ApplicationTheme.Dark;
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
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            // The page has 12px padding, so this pixel is its theme background.
            bitmap.GetPixel(4, 4).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
        }
        // The canvases show the camera's own colours whatever the theme.
        await StartLiveAsync();
        var (left, right) = await FrameHalvesAsync(MainCanvas);
        IsBlue(left).Should().BeTrue();
        IsRed(right).Should().BeTrue();
        await SnapshotAsync("WebcamPainter-" + expected);
    }
}
