using System;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class AttractPilotTests
{
    private readonly AttractPilot _pilot = new AttractPilot();

    [Fact]
    public void Decide_dodges_a_bolt_falling_onto_the_ship()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X + 10, simulation.Player.Y - 150, 0, 300);

        //Act
        var input = _pilot.Decide(simulation);

        //Assert
        input.MoveAxis.Should().Be(-1);
    }

    [Fact]
    public void Decide_dodges_away_from_the_wall_when_cornered()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Player.X = PlayerShip.MinX;
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), PlayerShip.MinX + 10, simulation.Player.Y - 150, 0, 300);

        //Act
        var input = _pilot.Decide(simulation);

        //Assert
        input.MoveAxis.Should().Be(1);
    }

    [Fact]
    public void Decide_ignores_a_bolt_that_will_miss()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X + 300, simulation.Player.Y - 150, 0, 300);

        //Act
        var found = AttractPilot.TryFindThreat(simulation, out _);

        //Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void Decide_predicts_an_aimed_bolt()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X - 100, simulation.Player.Y - 150, 200, 300);

        //Act
        var found = AttractPilot.TryFindThreat(simulation, out var x);

        //Assert
        found.Should().BeTrue();
        x.Should().BeApproximately(simulation.Player.X, 1e-9);
    }

    [Fact]
    public void Decide_lines_up_under_the_nearest_enemy_and_fires_when_aligned()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var nearest = simulation.Enemies.OrderBy(e => Math.Abs(e.X - simulation.Player.X)).First();
        simulation.Player.X = nearest.X + 30;

        //Act
        var far = _pilot.Decide(simulation);
        simulation.Player.X = nearest.X + 5;
        var close = _pilot.Decide(simulation);

        //Assert
        far.MoveAxis.Should().BeLessThan(0);
        far.Fire.Should().BeFalse();
        close.Fire.Should().BeTrue();
        close.Bomb.Should().BeFalse();
    }

    [Fact]
    public void Decide_does_nothing_while_the_ship_is_away()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Player.Destroy(simulation.Events);

        //Act
        var input = _pilot.Decide(simulation);

        //Assert
        input.HasAnyInput.Should().BeFalse();
    }

    [Fact]
    public void Decide_targets_an_unarmoured_boss_section()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.ClearWavesUntilBoss(simulation);
        TestSupport.StepUntil(simulation, s => s.Boss.State == BossState.Fighting);

        //Act
        var found = AttractPilot.TryFindTarget(simulation, out var x);

        //Assert
        found.Should().BeTrue();
        simulation.Boss.Sections.Skip(1).Select(s => simulation.Boss.BoxOf(s).CenterX).Should().Contain(x);
    }

    [Fact]
    public void Decide_rejects_null()
    {
        //Act
        Action act = () => _pilot.Decide(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void the_pilot_plays_a_convincing_demo_wave()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Cadet, seed: 2026);

        //Act
        for (var i = 0; i < 60 * 45 && !simulation.IsGameOver; i++)
        {
            simulation.Step(TestSupport.Dt, _pilot.Decide(simulation));
        }

        //Assert
        simulation.IsGameOver.Should().BeFalse();
        simulation.Score.Should().BeGreaterThan(500);
    }
}
