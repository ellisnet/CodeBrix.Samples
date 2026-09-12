using System;
using SilverAssertions;
using Xunit;

namespace GameEngineMusicDemo.Game.Tests;

public class GameEngineMusicDemoWalkthroughTests
{
    private const string VariableName = "GAMEENGINEMUSICDEMO_SELFTEST";

    [Fact]
    public void IsRequested_is_false_when_the_variable_is_not_set()
    {
        //Arrange
        Environment.SetEnvironmentVariable(VariableName, null);

        //Act
        var requested = GameEngineMusicDemoWalkthrough.IsRequested;

        //Assert
        requested.Should().Be(false);
    }

    [Fact]
    public void IsRequested_is_true_only_for_the_exact_opt_in_value()
    {
        //Arrange
        Environment.SetEnvironmentVariable(VariableName, "1");

        //Act
        var optedIn = GameEngineMusicDemoWalkthrough.IsRequested;
        Environment.SetEnvironmentVariable(VariableName, "true");
        var somethingElse = GameEngineMusicDemoWalkthrough.IsRequested;
        Environment.SetEnvironmentVariable(VariableName, null);

        //Assert
        optedIn.Should().Be(true);
        somethingElse.Should().Be(false);
    }
}
