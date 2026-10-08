using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using SilverAssertions;
using Windows.UI.ViewManagement;
using Xunit;
using Color = Windows.UI.Color;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    private static readonly Color Night = Color.FromArgb(255, 0x16, 0x21, 0x3A);
    private static readonly Color Limestone = Color.FromArgb(255, 0xED, 0xE6, 0xD6);

    [Fact]
    public async Task The_panes_keep_their_night_palette_and_light_text_under_either_OS_theme()
    {
        var systemTheme = _fixture.Application.SystemTheme;
        await Page.EvaluateAsync(() =>
        {
            // The simulated OS preference reaches the system colors...
            new UISettings().GetColorValue(UIColorType.Background).Should()
                .Be(systemTheme == ApplicationTheme.Dark ? Colors.Black : Colors.White);
            // ...but the Art Deco night palette is the game's own: the application chooses it at startup.
            Application.Current.RequestedTheme.Should().Be(ApplicationTheme.Dark);
        });

        // The title pane's corner is the night background; its wordmark is limestone on it.
        var screen = PixelStats.FromPng(await Page.ScreenshotAsync());
        screen.GetPixel(4, 4).Should().Be(Night);
        var wordmark = PixelStats.FromPng(await Id("TitleWordmark").ScreenshotAsync());
        wordmark.Coverage(Limestone, 24).Should().BeGreaterThan(0.05);
        wordmark.MeanLuminance().Should().BeGreaterThan(screen.Region(0, 0, 16, 16).MeanLuminance() + 20);

        // The settings pane over it: a dark scrim and panel, its switch labels readable.
        await Id("Settings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        var scrim = PixelStats.FromPng(await Page.ScreenshotAsync()).GetPixel(4, 4);
        (scrim.R + scrim.G + scrim.B).Should().BeLessThan(200);
        var switches = PixelStats.FromPng(await Id("SoundEnabled").ScreenshotAsync());
        switches.Coverage(Limestone, 40).Should().BeGreaterThan(0.005);
        await SnapshotAsync($"GoddessTempleDiscovery-settings-{systemTheme}");
        await Id("CloseSettings").ClickAsync();
    }
}
