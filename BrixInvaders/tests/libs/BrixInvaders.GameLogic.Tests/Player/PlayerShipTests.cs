using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class PlayerShipTests
{
    [Fact]
    public void a_new_ship_is_intact_centred_and_present()
    {
        //Act
        var ship = new PlayerShip(3, 1);

        //Assert
        ship.X.Should().Be(Playfield.CenterX);
        ship.Y.Should().Be(PlayerShip.LineY);
        ship.Hull.Should().Be(PlayerShip.MaxHull);
        ship.DamageTier.Should().Be(0);
        ship.IsPresent.Should().BeTrue();
        ship.Lives.Should().Be(3);
        ship.Bombs.Should().Be(1);
    }

    [Fact]
    public void Move_travels_at_ship_speed()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);

        //Act
        ship.Move(1, 0.1, false);

        //Assert
        ship.X.Should().BeApproximately(Playfield.CenterX + 42, 1e-9);
    }

    [Fact]
    public void Move_with_the_speed_boost_is_half_again_faster()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);

        //Act
        ship.Move(-1, 0.1, true);

        //Assert
        ship.X.Should().BeApproximately(Playfield.CenterX - 63, 1e-9);
    }

    [Fact]
    public void Move_clamps_to_the_playfield_on_both_sides()
    {
        //Arrange
        var left = new PlayerShip(3, 0);
        var right = new PlayerShip(3, 0);

        //Act
        left.Move(-1, 10, true);
        right.Move(5, 10, true);

        //Assert
        left.X.Should().Be(PlayerShip.MinX);
        right.X.Should().Be(PlayerShip.MaxX);
        CollisionResolver.IsInsidePlayfield(left.Box).Should().BeTrue();
        CollisionResolver.IsInsidePlayfield(right.Box).Should().BeTrue();
    }

    [Fact]
    public void Move_does_nothing_while_waiting_to_respawn()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);
        ship.Destroy(new GameEvents());

        //Act
        ship.Move(1, 1, false);

        //Assert
        ship.X.Should().Be(Playfield.CenterX);
    }

    [Fact]
    public void ApplyHit_steps_through_the_damage_tiers_then_destroys()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);
        var events = new GameEvents();

        //Act
        var first = ship.ApplyHit(events);
        var tierAfterFirst = ship.DamageTier;
        ship.InvulnerableTime = 0;
        var second = ship.ApplyHit(events);
        var tierAfterSecond = ship.DamageTier;
        ship.InvulnerableTime = 0;
        var third = ship.ApplyHit(events);

        //Assert
        first.Should().Be(PlayerHitResult.Damaged);
        tierAfterFirst.Should().Be(1);
        second.Should().Be(PlayerHitResult.Damaged);
        tierAfterSecond.Should().Be(2);
        third.Should().Be(PlayerHitResult.Destroyed);
        ship.Lives.Should().Be(2);
        ship.IsPresent.Should().BeFalse();
        events.CountOf(GameEventKind.PlayerHit).Should().Be(2);
        events.CountOf(GameEventKind.PlayerDestroyed).Should().Be(1);
    }

    [Fact]
    public void ApplyHit_is_ignored_during_the_hit_invulnerability_window()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);
        var events = new GameEvents();
        ship.ApplyHit(events);

        //Act
        var during = ship.ApplyHit(events);
        ship.Update(PlayerShip.HitInvulnerability + 0.001, events);
        var after = ship.ApplyHit(events);

        //Assert
        during.Should().Be(PlayerHitResult.Ignored);
        after.Should().Be(PlayerHitResult.Damaged);
        ship.Hull.Should().Be(1);
    }

    [Fact]
    public void ApplyHit_uses_the_shield_first()
    {
        //Arrange
        var ship = new PlayerShip(3, 0) { ShieldStrength = 2 };
        var events = new GameEvents();

        //Act
        var result = ship.ApplyHit(events);

        //Assert
        result.Should().Be(PlayerHitResult.Absorbed);
        ship.ShieldStrength.Should().Be(1);
        ship.Hull.Should().Be(PlayerShip.MaxHull);
        ship.InvulnerableTime.Should().Be(PlayerShip.ShieldInvulnerability);
        TestSupport.Collect(events, GameEventKind.ShieldAbsorbed)[0].Value.Should().Be(1);
    }

    [Fact]
    public void respawn_happens_after_the_delay_with_the_invulnerability_window()
    {
        //Arrange
        var ship = new PlayerShip(2, 0);
        var events = new GameEvents();
        ship.Move(1, 1, false);
        ship.Destroy(events);

        //Act
        ship.Update(PlayerShip.RespawnDelay - 0.01, events);
        var presentEarly = ship.IsPresent;
        ship.Update(0.02, events);

        //Assert
        presentEarly.Should().BeFalse();
        ship.IsPresent.Should().BeTrue();
        ship.X.Should().Be(Playfield.CenterX);
        ship.Hull.Should().Be(PlayerShip.MaxHull);
        ship.IsInvulnerable.Should().BeTrue();
        ship.InvulnerableTime.Should().Be(PlayerShip.RespawnInvulnerability);
        events.Contains(GameEventKind.PlayerRespawned).Should().BeTrue();
    }

    [Fact]
    public void the_respawn_invulnerability_window_ignores_hits_until_it_ends()
    {
        //Arrange
        var ship = new PlayerShip(2, 0);
        var events = new GameEvents();
        ship.Destroy(events);
        ship.Update(PlayerShip.RespawnDelay + 0.001, events);

        //Act
        ship.Update(PlayerShip.RespawnInvulnerability - 0.1, events);
        var during = ship.ApplyHit(events);
        ship.Update(0.2, events);
        var after = ship.ApplyHit(events);

        //Assert
        during.Should().Be(PlayerHitResult.Ignored);
        after.Should().Be(PlayerHitResult.Damaged);
    }

    [Fact]
    public void no_respawn_after_the_last_life()
    {
        //Arrange
        var ship = new PlayerShip(1, 0);
        var events = new GameEvents();
        ship.Destroy(events);

        //Act
        ship.Update(10, events);

        //Assert
        ship.Lives.Should().Be(0);
        ship.IsPresent.Should().BeFalse();
    }

    [Fact]
    public void Destroy_ignores_shield_and_invulnerability()
    {
        //Arrange
        var ship = new PlayerShip(3, 0) { ShieldStrength = 3, InvulnerableTime = 5 };

        //Act
        var destroyed = ship.Destroy(new GameEvents());

        //Assert
        destroyed.Should().BeTrue();
        ship.Lives.Should().Be(2);
        ship.ShieldStrength.Should().Be(0);
    }

    [Fact]
    public void Destroy_does_nothing_while_already_destroyed()
    {
        //Arrange
        var ship = new PlayerShip(3, 0);
        ship.Destroy(new GameEvents());

        //Act
        var again = ship.Destroy(new GameEvents());

        //Assert
        again.Should().BeFalse();
        ship.Lives.Should().Be(2);
    }

    [Fact]
    public void CanFire_waits_for_the_cooldown()
    {
        //Arrange
        var ship = new PlayerShip(3, 0) { FireCooldown = PlayerShip.FireInterval };

        //Act
        var before = ship.CanFire();
        ship.Update(PlayerShip.FireInterval, new GameEvents());

        //Assert
        before.Should().BeFalse();
        ship.CanFire().Should().BeTrue();
    }

    [Fact]
    public void starting_bombs_are_capped_at_three() => new PlayerShip(3, 7).Bombs.Should().Be(PlayerShip.MaxBombs);

    [Fact]
    public void CountDown_snaps_floating_point_residue_to_zero()
    {
        //Arrange
        var value = PlayerShip.RapidFireInterval;

        //Act
        for (var i = 0; i < 9; i++)
        {
            value = PlayerShip.CountDown(value, 1.0 / 60.0);
        }

        //Assert
        value.Should().Be(0);
    }

    [Fact]
    public void CountDown_never_goes_negative() => PlayerShip.CountDown(0.01, 1).Should().Be(0);
}
