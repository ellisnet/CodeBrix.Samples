using PdfSideBySide.PdfRender.Viewing;
using SilverAssertions;
using System;
using Xunit;

namespace PdfSideBySide.PdfRender.Tests;

public class PanPositionTests
{
    [Fact]
    public void a_new_position_is_centred_and_can_move_every_way()
    {
        //Act
        var pan = new PanPosition();

        //Assert
        pan.Horizontal.Should().Be(PanPosition.Centre);
        pan.Vertical.Should().Be(PanPosition.Centre);
        pan.CanMove(PanDirection.Up).Should().BeTrue();
        pan.CanMove(PanDirection.Down).Should().BeTrue();
        pan.CanMove(PanDirection.Left).Should().BeTrue();
        pan.CanMove(PanDirection.Right).Should().BeTrue();
    }

    [Theory]
    [InlineData(PanDirection.Up, 0.5, 0.3)]
    [InlineData(PanDirection.Down, 0.5, 0.7)]
    [InlineData(PanDirection.Left, 0.3, 0.5)]
    [InlineData(PanDirection.Right, 0.7, 0.5)]
    public void move_shifts_one_axis_by_the_fraction(PanDirection direction, double expectedHorizontal, double expectedVertical)
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var moved = pan.Move(direction, 0.2);

        //Assert
        moved.Should().BeTrue();
        pan.Horizontal.Should().BeApproximately(expectedHorizontal, 1e-9);
        pan.Vertical.Should().BeApproximately(expectedVertical, 1e-9);
    }

    [Fact]
    public void move_stops_at_the_page_edge()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var first = pan.Move(PanDirection.Right, 0.4);
        var second = pan.Move(PanDirection.Right, 0.4);
        var third = pan.Move(PanDirection.Right, 0.4);

        //Assert
        first.Should().BeTrue();
        second.Should().BeTrue(); //Clamped from 1.3 to 1.0, but it did move
        third.Should().BeFalse();
        pan.Horizontal.Should().Be(1.0);
        pan.CanMove(PanDirection.Right).Should().BeFalse();
        pan.CanMove(PanDirection.Left).Should().BeTrue();
    }

    [Fact]
    public void move_by_zero_does_nothing()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var moved = pan.Move(PanDirection.Down, 0);

        //Assert
        moved.Should().BeFalse();
        pan.Vertical.Should().Be(PanPosition.Centre);
    }

    [Fact]
    public void move_rejects_a_negative_fraction()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        Action act = () => pan.Move(PanDirection.Down, -0.1);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void reset_recentres()
    {
        //Arrange
        var pan = new PanPosition();
        pan.Move(PanDirection.Up, 0.5);
        pan.Move(PanDirection.Left, 0.5);

        //Act
        var changed = pan.Reset();
        var changedAgain = pan.Reset();

        //Assert
        changed.Should().BeTrue();
        changedAgain.Should().BeFalse();
        pan.Horizontal.Should().Be(PanPosition.Centre);
        pan.Vertical.Should().Be(PanPosition.Centre);
    }
}
