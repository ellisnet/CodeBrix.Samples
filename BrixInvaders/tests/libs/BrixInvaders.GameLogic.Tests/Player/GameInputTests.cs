using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class GameInputTests
{
    [Theory]
    [InlineData(-3, -1)]
    [InlineData(0.4, 0.4)]
    [InlineData(2, 1)]
    [InlineData(double.NaN, 0)]
    public void MoveAxis_is_clamped(double raw, double expected) => new GameInput(raw, false, false).MoveAxis.Should().Be(expected);

    [Fact]
    public void HasAnyInput_is_false_only_for_no_input()
    {
        //Act
        var none = GameInput.None.HasAnyInput;
        var fire = new GameInput(0, true, false).HasAnyInput;
        var bomb = new GameInput(0, false, true).HasAnyInput;
        var move = new GameInput(-0.2, false, false).HasAnyInput;

        //Assert
        none.Should().BeFalse();
        fire.Should().BeTrue();
        bomb.Should().BeTrue();
        move.Should().BeTrue();
    }
}
