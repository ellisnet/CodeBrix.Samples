using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Model_view_stacks_in_portrait_and_sits_side_by_side_in_landscape(ScreenOrientation orientation)
    {
        Fixture.Application.Orientation.Should().Be(orientation);
        await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        var stacked = orientation == ScreenOrientation.Portrait;
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsModelViewStacked, value => value == stacked, description: "Model View layout");
        var about = await Page.GetByText("ABOUT THIS MODEL", new() { Exact = true }).BoundingBoxAsync();
        var hint = await Page.GetByText("drag to rotate · scroll to zoom", new() { Exact = true }).BoundingBoxAsync();
        if (stacked)
        {
            // The info pane takes the top half; the 3D viewer and its hint sit below it, full width.
            hint.Y.Should().BeGreaterThan(Fixture.Application.Height / 2f);
            hint.X.Should().BeLessThan(Fixture.Application.Width / 2f);
        }
        else
        {
            // The info pane is a fixed column on the left; the viewer and its hint fill the space beside it.
            hint.X.Should().BeGreaterThan(about.X + 420);
            hint.Y.Should().BeGreaterThan(Fixture.Application.Height / 2f);
        }
        about.X.Should().BeLessThan(Fixture.Application.Width / 4f);
        await SnapshotAsync($"PolyHavenBrowser-model-{orientation}");
    }
}
