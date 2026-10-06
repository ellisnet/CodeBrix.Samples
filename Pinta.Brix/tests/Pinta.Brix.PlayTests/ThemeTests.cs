using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ThemeTests(AppFixture fixture) : PintaTest(fixture)
{
    private async Task<SKBitmap> ScreenAsync() => SKBitmap.Decode(await Page.ScreenshotAsync());

    [Fact]
    public async Task Page_follows_the_simulated_OS_theme()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        var dark = expected == ApplicationTheme.Dark;
        var background = await EvaluateAsync(() =>
        {
            Fixture.View.ActualTheme.Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
            return ((SolidColorBrush)Fixture.View.Background).Color;
        });
        foreach (var channel in new[] { background.R, background.G, background.B })
        {
            if (dark) channel.Should().BeLessThan((byte)64);
            else channel.Should().BeGreaterThan((byte)192);
        }
        // The icon toolbar row has no background of its own; its far right end is bare page.
        var toolbar = await Page.GetByTestId("MainToolbar").BoundingBoxAsync();
        using var screen = await ScreenAsync();
        screen.GetPixel(Fixture.Application.Width - 4, (int)(toolbar.Y + toolbar.Height / 2))
            .Should().Be(new SKColor(background.R, background.G, background.B, background.A));
    }

    [Fact]
    public async Task Effect_dialog_stays_dark_in_either_theme()
    {
        await MenuAsync("Effects", "Blurs", "Gaussian Blur...");
        var title = Page.GetByText("Gaussian Blur", new() { Exact = true });
        await Expect(title).ToBeVisibleAsync();
        var box = await title.BoundingBoxAsync();
        using (var screen = await ScreenAsync())
            screen.GetPixel((int)box.X - 6, (int)(box.Y + box.Height / 2)).Should().Be(new SKColor(0x2B, 0x2B, 0x2B));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Expect(title).ToBeHiddenAsync();
    }
}
