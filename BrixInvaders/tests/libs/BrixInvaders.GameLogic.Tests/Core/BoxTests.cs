using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class BoxTests
{
    [Fact]
    public void FromCenter_halves_the_size()
    {
        //Arrange
        var box = Box.FromCenter(100, 50, 40, 20);

        //Assert
        box.HalfWidth.Should().Be(20);
        box.HalfHeight.Should().Be(10);
        box.Left.Should().Be(80);
        box.Right.Should().Be(120);
        box.Top.Should().Be(40);
        box.Bottom.Should().Be(60);
        box.Width.Should().Be(40);
        box.Height.Should().Be(20);
    }

    [Fact]
    public void Intersects_overlapping_boxes() => Box.FromCenter(0, 0, 10, 10).Intersects(Box.FromCenter(8, 8, 10, 10)).Should().BeTrue();

    [Fact]
    public void Intersects_is_false_for_boxes_touching_along_an_edge() => Box.FromCenter(0, 0, 10, 10).Intersects(Box.FromCenter(10, 0, 10, 10)).Should().BeFalse();

    [Fact]
    public void Intersects_is_false_for_separate_boxes() => Box.FromCenter(0, 0, 10, 10).Intersects(Box.FromCenter(0, 30, 10, 10)).Should().BeFalse();

    [Fact]
    public void Intersects_a_contained_box() => Box.FromCenter(0, 0, 100, 100).Intersects(Box.FromCenter(5, 5, 2, 2)).Should().BeTrue();

    [Fact]
    public void IsInside_allows_touching_edges() => Box.FromCenter(5, 5, 10, 10).IsInside(Box.FromCenter(5, 5, 10, 10)).Should().BeTrue();

    [Fact]
    public void IsInside_is_false_when_poking_out() => Box.FromCenter(5, 5, 12, 10).IsInside(Box.FromCenter(5, 5, 10, 10)).Should().BeFalse();
}
