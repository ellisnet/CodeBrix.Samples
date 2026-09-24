using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class ProjectilesTests
{
    [Fact]
    public void Update_moves_projectiles_by_their_velocity()
    {
        //Arrange
        var projectiles = new Projectiles();
        var bolt = projectiles.AddPlayerBolt(1, 100, 600, 0, -900, false);

        //Act
        projectiles.Update(0.1, 0, 0, new GameEvents());

        //Assert
        bolt.Y.Should().BeApproximately(510, 1e-9);
        bolt.Age.Should().BeApproximately(0.1, 1e-12);
    }

    [Fact]
    public void Update_counts_a_bolt_leaving_the_top_without_a_hit_as_a_miss()
    {
        //Arrange
        var projectiles = new Projectiles();
        projectiles.AddPlayerBolt(1, 100, 10, 0, -900, false);

        //Act
        var misses = projectiles.Update(0.1, 0, 0, new GameEvents());

        //Assert
        misses.Should().Be(1);
        projectiles.PlayerBolts.Should().BeEmpty();
    }

    [Fact]
    public void Update_does_not_count_a_piercing_bolt_that_hit_something_as_a_miss()
    {
        //Arrange
        var projectiles = new Projectiles();
        var bolt = projectiles.AddPlayerBolt(1, 100, 10, 0, -900, true);
        bolt.RecordHit(77);

        //Act
        var misses = projectiles.Update(0.1, 0, 0, new GameEvents());

        //Assert
        misses.Should().Be(0);
        projectiles.PlayerBolts.Should().BeEmpty();
    }

    [Fact]
    public void Update_culls_enemy_bolts_below_the_playfield()
    {
        //Arrange
        var projectiles = new Projectiles();
        projectiles.AddEnemyBolt(1, 100, Playfield.Height + 40, 0, 300);
        projectiles.AddEnemyBolt(2, 100, 400, 0, 300);

        //Act
        projectiles.Update(0.01, 0, 0, new GameEvents());

        //Assert
        projectiles.EnemyBolts.Should().ContainSingle(b => b.Id == 2);
    }

    [Fact]
    public void Steer_turns_at_most_the_turn_rate_and_keeps_the_speed()
    {
        //Arrange
        var projectiles = new Projectiles();
        var missile = projectiles.AddMissile(1, 640, 100, 220);
        var before = missile.Heading;

        //Act
        Projectiles.Steer(missile, 0.1, 0, 100);

        //Assert
        Math.Abs(Projectiles.NormalizeAngle(missile.Heading - before)).Should().BeApproximately(Projectiles.MissileTurnRate * 0.1, 1e-9);
        Math.Sqrt((missile.Vx * missile.Vx) + (missile.Vy * missile.Vy)).Should().BeApproximately(220, 1e-9);
    }

    [Fact]
    public void Steer_locks_on_when_the_target_is_within_one_turn()
    {
        //Arrange
        var projectiles = new Projectiles();
        var missile = projectiles.AddMissile(1, 640, 100, 220);

        //Act
        Projectiles.Steer(missile, 0.1, 641, 600);

        //Assert
        missile.Heading.Should().BeApproximately(Math.Atan2(500, 1), 1e-9);
    }

    [Fact]
    public void missiles_expire_after_their_lifetime()
    {
        //Arrange
        var projectiles = new Projectiles();
        projectiles.AddMissile(1, 640, 100, 1);
        var events = new GameEvents();
        var expired = 0;

        //Act
        for (var step = 0; step < 60 * 7; step++)
        {
            events.Clear();
            projectiles.Update(TestSupport.Dt, 640, 90, events);
            expired += events.CountOf(GameEventKind.MissileExpired);
        }

        //Assert
        expired.Should().Be(1);
        projectiles.Missiles.Should().BeEmpty();
    }

    [Theory]
    [InlineData(4.0, 4.0 - (2 * Math.PI))]
    [InlineData(-4.0, -4.0 + (2 * Math.PI))]
    [InlineData(Math.PI, Math.PI)]
    [InlineData(-Math.PI, Math.PI)]
    [InlineData(0.5, 0.5)]
    public void NormalizeAngle_wraps_into_minus_pi_to_pi(double angle, double expected) => Projectiles.NormalizeAngle(angle).Should().BeApproximately(expected, 1e-12);

    [Fact]
    public void ClearEnemyFire_removes_bolts_and_missiles_but_not_player_bolts()
    {
        //Arrange
        var projectiles = new Projectiles();
        projectiles.AddEnemyBolt(1, 0, 0, 0, 1);
        projectiles.AddMissile(2, 0, 0, 1);
        projectiles.AddPlayerBolt(3, 0, 0, 0, -1, false);

        //Act
        var removed = projectiles.ClearEnemyFire();

        //Assert
        removed.Should().Be(2);
        projectiles.EnemyBolts.Should().BeEmpty();
        projectiles.Missiles.Should().BeEmpty();
        projectiles.PlayerBolts.Should().HaveCount(1);
    }

    [Fact]
    public void Box_depends_on_the_kind()
    {
        //Arrange
        var projectiles = new Projectiles();

        //Act
        var player = projectiles.AddPlayerBolt(1, 0, 0, 0, -1, false).Box;
        var enemy = projectiles.AddEnemyBolt(2, 0, 0, 0, 1).Box;
        var missile = projectiles.AddMissile(3, 0, 0, 1).Box;

        //Assert
        player.Height.Should().Be(Projectile.PlayerBoltHeight);
        enemy.Height.Should().Be(Projectile.EnemyBoltHeight);
        missile.Width.Should().Be(Projectile.MissileSize);
    }
}
