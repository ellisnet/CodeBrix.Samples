using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class UfoTests
{
    [Fact]
    public void a_right_flying_ufo_starts_off_the_left_edge()
    {
        //Act
        var ufo = new Ufo(1, 1, 100);

        //Assert
        ufo.X.Should().BeLessThan(0);
        ufo.Y.Should().Be(Ufo.LaneY);
        ufo.HasEscaped.Should().BeFalse();
    }

    [Fact]
    public void a_left_flying_ufo_escapes_past_the_left_edge()
    {
        //Arrange
        var ufo = new Ufo(1, -1, 100);

        //Act
        ufo.X = -Ufo.Width - 1;

        //Assert
        ufo.HasEscaped.Should().BeTrue();
    }

    [Fact]
    public void the_ufo_lane_is_below_the_hud()
        => (Ufo.LaneY - (Ufo.Height / 2.0)).Should().BeGreaterThanOrEqualTo(Playfield.HudHeight);
}
