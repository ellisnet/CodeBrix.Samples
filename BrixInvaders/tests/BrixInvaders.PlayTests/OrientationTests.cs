using System.Threading.Tasks;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_letterboxes_the_playfield_and_the_game_keeps_running()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await Expect(_fixture.Canvas).ToBeVisibleAsync();
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        canvas.Width.Should().Be(1080);
        canvas.Height.Should().Be(1920);
        // Re-laying out the canvas must not start a second game host.
        (await Page.EvaluateAsync(() => ReferenceEquals(_fixture.Host, _fixture.FirstHost))).Should().BeTrue();
        await WaitForTheGameToStepAsync();
        // The 16:9 playfield is scaled to the canvas width and centred: dark bands above and below it.
        var height = canvas.Width * Playfield.Height / Playfield.Width;
        var top = canvas.Y + ((canvas.Height - height) / 2);
        (await WaitForLitShareAsync(canvas.X, top, canvas.Width, height, share => share > 0.02)).Should().BeGreaterThan(0.02);
        (await WaitForLitShareAsync(canvas.X, canvas.Y, canvas.Width, top - canvas.Y - 8, share => share < 0.001)).Should().BeLessThan(0.001);
        (await WaitForLitShareAsync(canvas.X, top + height + 8, canvas.Width, canvas.Y + canvas.Height - top - height - 8, share => share < 0.001))
            .Should().BeLessThan(0.001);
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Landscape_fills_the_canvas_with_the_playfield_and_the_game_keeps_running()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Landscape);
        await Expect(_fixture.Canvas).ToBeVisibleAsync();
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        canvas.Width.Should().Be(1920);
        canvas.Height.Should().Be(1080);
        (await Page.EvaluateAsync(() => ReferenceEquals(_fixture.Host, _fixture.FirstHost))).Should().BeTrue();
        await WaitForTheGameToStepAsync();
        // 1920 x 1080 is the playfield's own 16:9 shape: the title fills the canvas from top to bottom.
        (await WaitForLitShareAsync(canvas.X, canvas.Y, canvas.Width, canvas.Height / 4, share => share > 0.01)).Should().BeGreaterThan(0.01);
        (await WaitForLitShareAsync(canvas.X, canvas.Y + (canvas.Height * 3 / 4), canvas.Width, canvas.Height / 4, share => share > 0.01))
            .Should().BeGreaterThan(0.01);
    }
}
