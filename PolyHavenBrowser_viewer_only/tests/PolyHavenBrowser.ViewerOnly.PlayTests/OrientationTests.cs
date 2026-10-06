using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.ViewerOnly.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Orientation_change_rerenders_canvas_at_new_size()
    {
        var engine = Fixture.Engines.Current;
        await Fixture.Application.WaitForAsync(() => engine.ModelFrames, frames => frames > 0, description: "the first model frame");
        var landscape = Fixture.Application.Orientation == ScreenOrientation.Landscape;
        (await Page.EvaluateAsync(() => engine.LastFrameSize.Width > engine.LastFrameSize.Height)).Should().Be(landscape);
        // Turn the screen; the next reset restores the launch orientation.
        await Fixture.Application.SetOrientationAsync(landscape ? ScreenOrientation.Portrait : ScreenOrientation.Landscape);
        var box = await Canvas.BoundingBoxAsync();
        (box.Width > box.Height).Should().Be(!landscape);
        await Fixture.Application.WaitForAsync(() => engine.LastFrameSize, size => size.Width > size.Height == !landscape,
            description: "a frame at the turned canvas size");
        await Expect(Status).ToHaveTextAsync(TextureStatus);
    }
}
