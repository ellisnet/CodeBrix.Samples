using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace MediaPlayerDemo.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Simulated_OS_theme_reaches_page_background_and_pixels()
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
        pageBackground.A.Should().Be((byte)255);
        await Expect(Button("Load")).ToBeVisibleAsync();
        await Expect(StretchPicker).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Loaded: " + AppFixture.DefaultAddress);
        var png = await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine("TestResults", "PlayTest", $"MediaPlayerDemo-{expected}.png"),
        });
        using var bitmap = SKBitmap.Decode(png);
        // The page has a 10px content margin, so this pixel is its theme background.
        bitmap.GetPixel(5, 5).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
    }
}
