using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using PainDiagram.ViewModels;
using SilverAssertions;
using Xunit;

namespace PainDiagram.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_canvas_drawable()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        var box = await Canvas.BoundingBoxAsync();
        box.Height.Should().BeGreaterThan(box.Width);
        var rect = await DrawingRectAsync();
        // Portrait letterboxes the square body map above and below it.
        rect.Left.Should().BeLessThan(1f);
        rect.Top.Should().BeGreaterThan(10f);
        await Page.GetByTestId("NumbnessLayer").ClickAsync();
        await DrawStrokeAsync();
        (await Page.EvaluateAsync(() => Fixture.Session.GetLayer(MainViewModel.NumbnessLayerName).ElementCount)).Should().Be(1);
        await Expect(Button("Save")).ToBeEnabledAsync();
        await Expect(Button("Clear")).ToBeEnabledAsync();
        await SnapshotAsync("PainDiagram-portrait");
    }
}
