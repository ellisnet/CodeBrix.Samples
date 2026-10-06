using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace GameEngineMusicDemo.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_keeps_both_panes_and_starts_the_demo_once()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        var canvas = await Page.GetByType<GameSurfaceCanvas>().BoundingBoxAsync();
        canvas.Width.Should().Be(1080 - 360);
        canvas.Height.Should().Be(1920);
        await Expect(Page.GetByTestId("MasterSlider")).ToBeVisibleAsync();
        // Re-laying out the canvas in another orientation must not start a second demo.
        (await Page.EvaluateAsync(() => ReferenceEquals(_fixture.Demo, _fixture.FirstDemo))).Should().BeTrue();
        await PlayAsync("Play Track A (fade in)", "Track A");
        await ClickAsync("Pause / Resume the engine");
        (await Page.EvaluateAsync(() => _fixture.Demo.IsPaused)).Should().BeTrue();
        await ClickAsync("Pause / Resume the engine");
        (await Page.EvaluateAsync(() => _fixture.Demo.IsPaused)).Should().BeFalse();
        (await Page.EvaluateAsync(() => MusicManager.Instance.NowPlaying.Key)).Should().Be("Track A");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Landscape_keeps_both_panes_and_starts_the_demo_once()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Landscape);
        var canvas = await Page.GetByType<GameSurfaceCanvas>().BoundingBoxAsync();
        canvas.Width.Should().Be(1920 - 360);
        canvas.Height.Should().Be(1080);
        await Expect(Page.GetByTestId("MasterSlider")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => ReferenceEquals(_fixture.Demo, _fixture.FirstDemo))).Should().BeTrue();
        await PlayAsync("Play Track A (fade in)", "Track A");
    }
}
