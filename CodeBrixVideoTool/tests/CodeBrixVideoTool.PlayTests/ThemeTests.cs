using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Windows.UI;
using Xunit;

namespace CodeBrixVideoTool.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task App_stays_dark_under_either_system_theme()
    {
        // The simulated OS theme is fixed per process; the runners cover both. App.xaml asks for Dark.
        (await Page.EvaluateAsync(() => Application.Current.RequestedTheme)).Should().Be(ApplicationTheme.Dark);
        (await Page.EvaluateAsync(() => Fixture.View.ActualTheme)).Should().Be(ElementTheme.Dark);
        (await Page.EvaluateAsync(() => ((SolidColorBrush)Fixture.View.Background).Color))
            .Should().Be(Color.FromArgb(0xFF, 0x14, 0x16, 0x1A));
        // The header panel's own brush is what reaches the screen, whichever theme the OS reports.
        using var screen = SKBitmap.Decode(await Page.ScreenshotAsync());
        screen.GetPixel(4, 4).Should().Be(new SKColor(0x1C, 0x1F, 0x26));
        await SnapshotAsync($"CodeBrixVideoTool-theme-{(Fixture.Application.SystemTheme == ApplicationTheme.Dark ? "dark" : "light")}");
    }
}
