using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace WebcamViewer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_controls_visible()
    {
        Fixture.Application.Width.Should().Be(1080);
        Fixture.Application.Height.Should().Be(1920);
        await DiscoverAsync(CameraFixture.Camera(CameraA, microphone: true));
        await Expect(CameraList).ToBeVisibleAsync();
        await Expect(MonitorAudio).ToBeEnabledAsync();
        await Expect(FolderPath).ToBeVisibleAsync();
        await Expect(Button("Browse…")).ToBeVisibleAsync();
        await Expect(Button("Photo")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("VideoView")).ToBeVisibleAsync();
        foreach (var control in new[] { CameraList, MonitorAudio, FolderPath, Button("Browse…"), Button("Photo"), Status })
        {
            var box = await control.BoundingBoxAsync();
            box.X.Should().BeGreaterThanOrEqualTo(0);
            (box.X + box.Width).Should().BeLessThanOrEqualTo(1080);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(1920);
        }
        await SendFrameAsync(64, 48);
        var folder = NewFolder();
        await FolderPath.FillAsync(folder);
        await Button("Photo").ClickAsync();
        await Expect(Status).ToHaveTextAsync(new Regex(@"^Saved: "));
        (await WaitForPixelAsync(await CanvasPointAsync(0.5, 0.5), FrameColor)).Should().Be(FrameColor);
        await SnapshotAsync("WebcamViewer-portrait");
    }
}
