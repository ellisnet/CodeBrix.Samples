using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace WebcamPainter.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_canvas_and_side_panel_usable()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await TakePhotoAsync();
        var canvas = await MainCanvas.BoundingBoxAsync();
        var selfView = await SelfView.BoundingBoxAsync();
        canvas.Height.Should().BeGreaterThan(canvas.Width);
        // The side panel stays beside the canvas, never over it.
        selfView.X.Should().BeGreaterThan(canvas.X + canvas.Width);
        await Expect(Button("Violet")).ToBeVisibleAsync();
        await Expect(Button("Back")).ToBeVisibleAsync();
        await Button("Green").ClickAsync();
        await Expect(ActiveColor).ToHaveTextAsync("Painting with: Green");
        await PaintStrokeAsync(0.5f);
        await Expect(Button("Save…")).ToBeEnabledAsync();
        await Expect(Button("Clear")).ToBeEnabledAsync();
        await SnapshotAsync("WebcamPainter-portrait");
    }
}
