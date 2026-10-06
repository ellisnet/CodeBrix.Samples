using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace SimpleCbxVideoPlayer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Simulated_os_theme_reaches_the_page_background()
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
        var video = await Page.GetByTestId("VideoHost").BoundingBoxAsync();
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            // The page has 16px padding, so this pixel is its theme background.
            bitmap.GetPixel(4, 4).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
            // The video area stays black whatever the theme.
            NonBlackShare(bitmap, video.X + 8, video.Y + 8, video.Width - 16, video.Height - 16).Should().BeLessThan(0.02);
        }
        await SnapshotAsync("SimpleCbxVideoPlayer-" + expected);
    }

    [Fact]
    public async Task Lut_panel_keeps_its_text_readable_under_either_theme()
    {
        // The panel paints its own dark background, so it carries the dark theme whatever the OS prefers.
        (await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.FindName("LutPanel")).ActualTheme))
            .Should().Be(ElementTheme.Dark);
        var firstTitle = await Page.EvaluateAsync(() => Fixture.Model.Luts[0].DisplayName);
        var panel = await Page.EvaluateAsync(() =>
        {
            var border = (FrameworkElement)Fixture.View.FindName("LutPanel");
            var origin = border.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
            return (X: (float)origin.X, Y: (float)origin.Y);
        });
        var heading = await Page.GetByText("Colour lookup tables", new() { Exact = true }).BoundingBoxAsync();
        var title = await Page.GetByText(firstTitle, new() { Exact = true }).First.BoundingBoxAsync();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        var background = bitmap.GetPixel((int)panel.X + 8, (int)panel.Y + 8);
        foreach (var channel in new[] { background.Red, background.Green, background.Blue }) channel.Should().BeLessThan((byte)64);
        // Light glyph pixels on that dark background: the heading and a lookup table's title can be read.
        LightShare(bitmap, heading.X, heading.Y, heading.Width, heading.Height).Should().BeGreaterThan(0.03);
        LightShare(bitmap, title.X, title.Y, title.Width, title.Height).Should().BeGreaterThan(0.03);
    }

    private static double LightShare(SKBitmap bitmap, float x, float y, float width, float height)
    {
        int light = 0, total = 0;
        for (var row = (int)y; row < (int)(y + height); row++)
        {
            for (var column = (int)x; column < (int)(x + width); column++)
            {
                var pixel = bitmap.GetPixel(column, row);
                total++;
                if (pixel.Red > 170 && pixel.Green > 170 && pixel.Blue > 170) light++;
            }
        }
        return total == 0 ? 0 : (double)light / total;
    }
}
