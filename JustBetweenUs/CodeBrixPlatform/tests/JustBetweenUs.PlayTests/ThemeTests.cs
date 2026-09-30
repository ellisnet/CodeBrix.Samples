using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Windows.UI.ViewManagement;
using Xunit;

namespace JustBetweenUs.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Simulated_OS_theme_reaches_startup_system_colors_resources_and_pixels()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        _fixture.Application.SystemTheme.Should().Be(expected);
        // The framework assigns Application.RequestedTheme at launch, after the constructor.
        _fixture.AppLaunchTheme.Should().Be(expected);
        var dark = expected == ApplicationTheme.Dark;
        var background = dark ? Colors.Black : Colors.White;
        _fixture.AppConstructionSystemBackground.Should().Be(background);
        var pageBackground = await Page.EvaluateAsync(() =>
        {
            Application.Current.RequestedTheme.Should().Be(expected);
            _fixture.View.ActualTheme.Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
            var settings = new UISettings();
            settings.GetColorValue(UIColorType.Background).Should().Be(background);
            settings.GetColorValue(UIColorType.Foreground).Should().Be(dark ? Colors.White : Colors.Black);
            return ((SolidColorBrush)_fixture.View.Background).Color;
        });
        // Theme resources use their own palette (Dark currently uses #202020),
        // while UISettings reports the OS's black/white background preference.
        foreach (var channel in new[] { pageBackground.R, pageBackground.G, pageBackground.B })
        {
            if (dark) channel.Should().BeLessThan((byte)64);
            else channel.Should().BeGreaterThan((byte)192);
        }
        pageBackground.A.Should().Be((byte)255);
        var png = await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine("TestResults", "PlayTest", $"JustBetweenUs-{expected}.png"),
        });
        using var bitmap = SKBitmap.Decode(png);
        // The page has a 20px content margin, so this pixel is its theme background.
        bitmap.GetPixel(5, 5).Should().Be(new SKColor(pageBackground.R, pageBackground.G, pageBackground.B, pageBackground.A));
        await RoundtripInCurrentOrientationAsync(Aes, $"Encryption works with the {expected} OS theme.");
    }

    [Fact]
    public async Task App_can_override_its_theme_without_changing_the_simulated_OS()
    {
        var systemTheme = _fixture.Application.SystemTheme;
        var opposite = systemTheme == ApplicationTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        try
        {
            await Page.EvaluateAsync(() => _fixture.View.RequestedTheme = opposite);
            await _fixture.Application.WaitForAsync(() => _fixture.View.ActualTheme, value => value == opposite,
                description: "explicit page theme");
            await Page.EvaluateAsync(() =>
            {
                new UISettings().GetColorValue(UIColorType.Background).Should()
                    .Be(systemTheme == ApplicationTheme.Dark ? Colors.Black : Colors.White);
            });
            await Expect(Encrypt).ToBeVisibleAsync();
        }
        finally
        {
            await Page.EvaluateAsync(() => _fixture.View.RequestedTheme = ElementTheme.Default);
        }
    }
}
