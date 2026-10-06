using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PalmVisualizer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Page_follows_the_OS_theme_and_preview_stays_black()
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
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        var preview = await Page.GetByTestId("PreviewCanvas").BoundingBoxAsync();
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            // The page has 12px padding, so this pixel is its theme background.
            bitmap.GetPixel(4, 4).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
            // With no frame yet, the preview is black whatever the theme.
            IsBlack(bitmap.GetPixel((int)(preview.X + preview.Width / 2), (int)(preview.Y + preview.Height / 2))).Should().BeTrue();
        }
        await SnapshotAsync("PalmVisualizer-" + expected);
    }
}
