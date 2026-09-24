using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class CollisionResolverTests
{
    [Fact]
    public void Overlaps_matches_box_intersection() => CollisionResolver.Overlaps(Box.FromCenter(0, 0, 10, 10), Box.FromCenter(4, 4, 10, 10)).Should().BeTrue();

    [Fact]
    public void Overlaps_is_false_for_touching_edges() => CollisionResolver.Overlaps(Box.FromCenter(0, 0, 10, 10), Box.FromCenter(0, 10, 10, 10)).Should().BeFalse();

    [Fact]
    public void IsInsidePlayfield_accepts_the_player_at_its_extremes()
    {
        //Arrange
        var left = Box.FromCenter(PlayerShip.MinX, PlayerShip.LineY, PlayerShip.Width, PlayerShip.Height);
        var right = Box.FromCenter(PlayerShip.MaxX, PlayerShip.LineY, PlayerShip.Width, PlayerShip.Height);

        //Assert
        CollisionResolver.IsInsidePlayfield(left).Should().BeTrue();
        CollisionResolver.IsInsidePlayfield(right).Should().BeTrue();
    }

    [Fact]
    public void IsInsidePlayfield_rejects_a_box_past_the_edge() => CollisionResolver.IsInsidePlayfield(Box.FromCenter(-1, 100, 10, 10)).Should().BeFalse();

    [Fact]
    public void FirstOverlap_returns_the_first_overlapping_index()
    {
        //Arrange
        var targets = new[] { Box.FromCenter(100, 100, 10, 10), Box.FromCenter(0, 0, 10, 10), Box.FromCenter(2, 2, 10, 10) };

        //Act
        var index = CollisionResolver.FirstOverlap(Box.FromCenter(1, 1, 4, 4), targets);

        //Assert
        index.Should().Be(1);
    }

    [Fact]
    public void FirstOverlap_returns_minus_one_when_nothing_overlaps()
        => CollisionResolver.FirstOverlap(Box.FromCenter(500, 500, 4, 4), new[] { Box.FromCenter(0, 0, 10, 10) }).Should().Be(-1);
}
