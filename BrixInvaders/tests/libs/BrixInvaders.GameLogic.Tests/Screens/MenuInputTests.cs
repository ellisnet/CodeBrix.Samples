using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class MenuInputTests
{
    [Fact]
    public void None_has_no_input() => MenuInput.None.HasAnyInput.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void every_action_counts_as_input(int which)
    {
        //Act
        var input = new MenuInput(which == 0, which == 1, which == 2, which == 3, which == 4, which == 5, which == 6,
            which == 7, which == 8, which == 9);

        //Assert
        input.HasAnyInput.Should().BeTrue();
    }
}
