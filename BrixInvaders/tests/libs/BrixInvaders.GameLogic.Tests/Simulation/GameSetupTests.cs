using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class GameSetupTests
{
    [Fact]
    public void defaults_start_at_sector_one()
    {
        //Act
        var setup = new GameSetup(Difficulty.Ace);

        //Assert
        setup.Difficulty.Should().Be(Difficulty.Ace);
        setup.StartSector.Should().Be(1);
        setup.ShipShape.Should().Be(0);
        setup.ShipColour.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 3, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 0, 4)]
    public void invalid_values_are_rejected(int sector, int shape, int colour)
    {
        //Act
        Action act = () => new GameSetup(Difficulty.Pilot, sector, 1, shape, colour);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
