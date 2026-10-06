using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using GitHubIssueFinder.Theming;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using Xunit;

namespace GitHubIssueFinder.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task System_default_follows_launch_theme()
    {
        // The simulated OS theme is fixed per process; the runners cover both.
        var dark = Fixture.Application.SystemTheme == ApplicationTheme.Dark;
        var palette = dark ? ColorSchemes.Dark : ColorSchemes.Light;
        await Expect(Page.GetByTestId("SelectedScheme"))
            .ToHaveValueAsync(dark ? "System default (Dark)" : "System default (Light)");
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedScheme.Scheme)).Should().Be(ColorScheme.SystemDefault);
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentPalette)).Should().BeSameAs(palette);
        (await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.Content).ActualTheme))
            .Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
        (await Page.EvaluateAsync(() => ((SolidColorBrush)Application.Current.Resources["CanvasBrush"]).Color))
            .Should().Be(PaletteBrushes.ToColor(palette.Canvas));
        await Page.GetByTestId("SelectedScheme").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Option,
            new() { Name = dark ? "System default (Dark)" : "System default (Light)", Exact = true })).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await SnapshotAsync($"GitHubIssueFinder-system-{(dark ? "dark" : "light")}");
    }
}
