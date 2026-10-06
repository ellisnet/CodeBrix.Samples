using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class OrientationTests(AppFixture fixture) : PintaTest(fixture)
{
    private async Task AssertWorkspaceFitsAsync(int width, int height)
    {
        Fixture.Application.Width.Should().Be(width);
        Fixture.Application.Height.Should().Be(height);
        foreach (var part in new[] { Tool("Pencil"), Tool("Recolor"), Page.GetByTestId("LayersList"), Page.GetByTestId("HistoryList"), LayersPadButton("Layer Properties") })
        {
            await Expect(part).ToBeVisibleAsync();
            var box = await part.BoundingBoxAsync();
            (box.X + box.Width).Should().BeLessThanOrEqualTo(width);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(height);
        }
        // The canvas may be wider than its viewport, but it starts on screen, right of the toolbox.
        var canvas = await Canvas.BoundingBoxAsync();
        var toolbox = await Page.GetByTestId("Toolbox").BoundingBoxAsync();
        canvas.X.Should().BeGreaterThan(toolbox.X + toolbox.Width);
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(20, 20, 120, 20);
        IsBlack(await PixelAsync(70, 20)).Should().BeTrue();
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Landscape_shows_toolbox_canvas_and_pads()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Landscape);
        await AssertWorkspaceFitsAsync(1920, 1080);
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_shows_toolbox_canvas_and_pads()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await AssertWorkspaceFitsAsync(1080, 1920);
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Turning_the_screen_keeps_the_document_and_its_history()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await Fixture.Application.SetOrientationAsync(ScreenOrientation.Portrait);
        await Expect(Tabs).ToHaveCountAsync(1);
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Expect(Tool("Pencil")).ToBeCheckedAsync();
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        await DragOnImageAsync(100, 200, 200, 200);
        await Expect(HistoryRows).ToHaveCountAsync(3);
    }
}
