using BrixInvaders.Game.Rendering;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Rendering;

public class RenderFrameTests
{
    [Fact]
    public void Build_copies_the_lists_and_numbers_the_frames()
    {
        //Arrange
        var builder = new FrameBuilder();
        builder.AddWorld(DrawCommand.Dot(1, 2, 3, 0xFFFFFFFF));
        builder.AddOverlay(DrawCommand.Label("HI", 10, 10, 12, 0xFFFFFFFF));
        builder.AddHotspot(new Hotspot(100, 100, 20, 20, "https://example.org/"));

        //Act
        var first = builder.Build();
        builder.Clear();
        var second = builder.Build();

        //Assert
        first.World.Should().HaveCount(1);
        first.Overlay.Should().HaveCount(1);
        first.Hotspots.Should().HaveCount(1);
        second.World.Should().BeEmpty();
        second.Number.Should().Be(first.Number + 1);
    }

    [Fact]
    public void HitTest_finds_the_link_under_a_point()
    {
        //Arrange
        var builder = new FrameBuilder();
        builder.AddHotspot(new Hotspot(100, 100, 20, 20, "https://a/"));
        var frame = builder.Build();

        //Act
        var hit = frame.HitTest(109, 91);
        var miss = frame.HitTest(111, 100);

        //Assert
        hit.Should().Be("https://a/");
        miss.Should().BeNull();
    }

    [Fact]
    public void Empty_has_nothing_to_draw_or_click()
    {
        //Assert
        RenderFrame.Empty.World.Should().BeEmpty();
        RenderFrame.Empty.Overlay.Should().BeEmpty();
        RenderFrame.Empty.HitTest(0, 0).Should().BeNull();
    }

    [Fact]
    public void DrawCommand_clamps_opacity()
    {
        //Act
        var command = DrawCommand.Sprite("x#y", 0, 0, 10, 10, 0, 3);

        //Assert
        command.Alpha.Should().Be(1f);
        DrawCommand.Rect(0, 0, 1, 1, 0, alpha: -1).Alpha.Should().Be(0f);
    }
}
