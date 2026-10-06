using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.ViewerOnly.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Accent_highlight_follows_selected_sample_under_either_theme()
    {
        var expected = new PlayTestOptions { ConfigurationAssembly = typeof(AppFixture).Assembly }.ResolveTheme();
        Fixture.Application.SystemTheme.Should().Be(expected);
        (await Page.EvaluateAsync(() => Fixture.View.ActualTheme)).Should().Be(expected == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light);
        await Expect(Status).ToHaveTextAsync(TextureStatus);
        await ExpectAccentAsync(selected: "Sample Texture", other: "Sample HDRI");
        await Button("Sample HDRI").ClickAsync();
        await Expect(Status).ToHaveTextAsync(HdriStatus);
        await ExpectAccentAsync(selected: "Sample HDRI", other: "Sample Texture");
        await SnapshotAsync($"PolyHavenBrowser_viewer_only-{expected}");
    }

    private async Task ExpectAccentAsync(string selected, string other)
    {
        // Measure resting fills: the buttons enabled again after the load, and the pointer off them.
        await Expect(Button(selected)).ToBeEnabledAsync();
        await Status.HoverAsync();
        var accent = await RestingButtonPixelAsync(selected);
        var plain = await RestingButtonPixelAsync(other);
        IsAccent(accent).Should().BeTrue("the selected button {0} uses the accent fill", accent);
        IsAccent(plain).Should().BeFalse("the other button {0} uses the default fill", plain);
    }

    // A point inside the button, left of its caption: the button's own fill. The fill animates
    // briefly between states, so read frames until two in a row agree.
    private async Task<SKColor> RestingButtonPixelAsync(string name)
    {
        var box = await Button(name).BoundingBoxAsync();
        var previous = SKColors.Transparent;
        for (var frame = 0; frame < 30; frame++)
        {
            using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
            var pixel = bitmap.GetPixel((int)box.X + 8, (int)(box.Y + box.Height / 2));
            if (pixel == previous) return pixel;
            previous = pixel;
        }
        return previous;
    }

    // The accent fill is blue in both themes; the default button fill is a neutral grey.
    private static bool IsAccent(SKColor pixel) => pixel.Blue > pixel.Red + 40;
}
