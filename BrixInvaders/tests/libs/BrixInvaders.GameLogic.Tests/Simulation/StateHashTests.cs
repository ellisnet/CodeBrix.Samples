using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class StateHashTests
{
    [Fact]
    public void Compute_is_stable_for_an_unchanged_state()
    {
        //Arrange
        var simulation = TestSupport.Simulation();

        //Act
        var first = StateHash.Compute(simulation);
        var second = simulation.ComputeStateHash();

        //Assert
        first.Should().Be(second);
    }

    [Fact]
    public void Compute_changes_when_the_state_changes()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var before = simulation.ComputeStateHash();

        //Act
        simulation.Step(TestSupport.Dt, new GameInput(1, false, false));

        //Assert
        simulation.ComputeStateHash().Should().NotBe(before);
    }

    [Fact]
    public void Compute_rejects_null()
    {
        //Act
        Action act = () => StateHash.Compute(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
