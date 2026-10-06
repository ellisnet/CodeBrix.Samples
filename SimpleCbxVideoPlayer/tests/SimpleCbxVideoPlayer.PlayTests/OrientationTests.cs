using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace SimpleCbxVideoPlayer.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Portrait_clip_is_pillarboxed_in_landscape()
    {
        await PlayUntilAsync(PortraitClip, 0.5);
        await PauseAsync();
        (await Page.EvaluateAsync(() => Fixture.Player.DisplayHeight))
            .Should().BeGreaterThan(await Page.EvaluateAsync(() => Fixture.Player.DisplayWidth));
        var box = await Page.GetByTestId("VideoHost").BoundingBoxAsync();
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            // Black bars either side of the upright picture, the picture itself in the middle.
            NonBlackShare(bitmap, box.X + 4, box.Y + 4, box.Width * 0.15f, box.Height - 8).Should().BeLessThan(0.02);
            NonBlackShare(bitmap, box.X + box.Width * 0.85f, box.Y + 4, box.Width * 0.15f - 4, box.Height - 8).Should().BeLessThan(0.02);
            NonBlackShare(bitmap, box.X + box.Width * 0.45f, box.Y + box.Height * 0.25f, box.Width * 0.1f, box.Height * 0.5f)
                .Should().BeGreaterThan(0.5);
        }
        await SnapshotAsync("SimpleCbxVideoPlayer-portrait-clip");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_transport_and_panel_on_screen()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        foreach (var control in new[] { Button("Play"), Button("Pause"), Button("Stop"), Button("Bake chain to .cube"),
            Page.GetByTestId("ScrubBar"), Page.GetByTestId("TimeText"), Page.GetByTestId("VideoList"),
            Page.GetByTestId("RenderPathList"), Page.GetByTestId("VideoHost") })
        {
            var box = await control.BoundingBoxAsync();
            box.Should().NotBeNull();
            box.X.Should().BeGreaterThanOrEqualTo(0);
            box.Y.Should().BeGreaterThanOrEqualTo(0);
            (box.X + box.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(Fixture.Application.Height);
        }
        await PlayUntilAsync(LandscapeClip, 0.5);
        await PauseAsync();
        var video = await Page.GetByTestId("VideoHost").BoundingBoxAsync();
        using (var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync()))
        {
            NonBlackShare(bitmap, video.X + video.Width * 0.25f, video.Y + video.Height * 0.4f, video.Width * 0.5f, video.Height * 0.2f)
                .Should().BeGreaterThan(0.5);
        }
        await SnapshotAsync("SimpleCbxVideoPlayer-portrait");
    }
}
