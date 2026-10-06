using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PalmVisualizer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_controls_usable()
    {
        Fixture.Application.Width.Should().Be(1080);
        Fixture.Application.Height.Should().Be(1920);
        await Expect(CameraPicker).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("PreviewCanvas")).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        var status = await Status.BoundingBoxAsync();
        (status.Y + status.Height).Should().BeLessThanOrEqualTo(1920);
        await VisualizeAsync();
        await Expect(Page.GetByTestId("VisualizerCanvas")).ToBeVisibleAsync();
        var canvas = await Page.GetByTestId("VisualizerCanvas").BoundingBoxAsync();
        canvas.Height.Should().BeGreaterThan(canvas.Width);
        await Button("Back").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
        await SnapshotAsync("PalmVisualizer-portrait");
    }

    [Fact]
    public async Task Rotation_keeps_visualize_mode_and_its_session()
    {
        var app = Fixture.Application;
        var opposite = app.PreferredOrientation == ScreenOrientation.Landscape
            ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
        await VisualizeAsync();
        try
        {
            await app.SetOrientationAsync(opposite);
            app.Orientation.Should().Be(opposite);
            await Expect(Button("Back")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("VisualizerCanvas")).ToBeVisibleAsync();
            await Expect(Status).ToHaveTextAsync(OpenPalmPrompt);
        }
        finally
        {
            await app.SetOrientationAsync();
        }
        Fixture.Sessions.Created.Count.Should().Be(1);
        Fixture.Sessions.Last.StartCount.Should().Be(1);
        await Button("Back").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Live: Fake Cam A");
    }
}
