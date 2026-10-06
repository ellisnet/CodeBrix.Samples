using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using SilverAssertions;
using SkiaSharp;
using Windows.UI.ViewManagement;
using Xunit;

namespace InannaRosette.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task App_keeps_its_dark_palette_under_either_OS_theme()
    {
        var systemTheme = Fixture.Application.SystemTheme;
        await Page.EvaluateAsync(() =>
        {
            // The simulated OS preference reaches the system colors...
            new UISettings().GetColorValue(UIColorType.Background).Should()
                .Be(systemTheme == ApplicationTheme.Dark ? Colors.Black : Colors.White);
            // ...but the temple palette is always dark: the application chooses it at startup.
            Application.Current.RequestedTheme.Should().Be(ApplicationTheme.Dark);
            Fixture.View.ActualTheme.Should().Be(ElementTheme.Dark);
        });
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        // The header's top-left corner is its own night-blue gradient, whatever the OS theme.
        var pixel = bitmap.GetPixel(5, 5);
        pixel.Red.Should().BeLessThan((byte)96);
        pixel.Green.Should().BeLessThan((byte)96);
        pixel.Blue.Should().BeLessThan((byte)96);
        await Button("Draw").ClickAsync();
        await Expect(Page.GetByTestId("TrayCaption")).ToHaveTextAsync("1 card waiting");
        await SnapshotAsync($"InannaRosette-{systemTheme}");
    }
}
