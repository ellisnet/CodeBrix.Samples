using System;
using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class PowerUpSystemTests
{
    private readonly PowerUpSystem _system = new PowerUpSystem();
    private readonly PlayerShip _player = new PlayerShip(3, 0);
    private readonly Scoring _scoring = new Scoring(150);
    private readonly GameEvents _events = new GameEvents();

    [Theory]
    [InlineData(PowerUpKind.SpreadShot, 12)]
    [InlineData(PowerUpKind.RapidFire, 12)]
    [InlineData(PowerUpKind.PiercingLaser, 10)]
    [InlineData(PowerUpKind.SpeedBoost, 10)]
    [InlineData(PowerUpKind.ShieldBubble, 0)]
    [InlineData(PowerUpKind.ExtraLife, 0)]
    [InlineData(PowerUpKind.Bomb, 0)]
    public void DurationOf_pins_the_timed_power_ups(PowerUpKind kind, double seconds)
    {
        //Act
        var duration = PowerUpSystem.DurationOf(kind);

        //Assert
        duration.Should().Be(seconds);
        PowerUpSystem.IsTimed(kind).Should().Be(seconds > 0);
    }

    [Fact]
    public void WeightOf_adds_up_to_one_hundred()
        => Enum.GetValues<PowerUpKind>().Sum(PowerUpSystem.WeightOf).Should().Be(100);

    [Fact]
    public void Apply_starts_a_timer()
    {
        //Act
        _system.Apply(PowerUpKind.RapidFire, _player, _scoring, _events);

        //Assert
        _system.IsActive(PowerUpKind.RapidFire).Should().BeTrue();
        _system.TimeLeft(PowerUpKind.RapidFire).Should().Be(12);
    }

    [Fact]
    public void collecting_an_active_timed_power_up_refreshes_it_instead_of_adding()
    {
        //Arrange
        _system.Apply(PowerUpKind.SpreadShot, _player, _scoring, _events);
        _system.Update(5, _events);

        //Act
        _system.Apply(PowerUpKind.SpreadShot, _player, _scoring, _events);

        //Assert
        _system.TimeLeft(PowerUpKind.SpreadShot).Should().Be(12);
    }

    [Fact]
    public void different_timed_power_ups_run_together()
    {
        //Arrange
        _system.Apply(PowerUpKind.SpreadShot, _player, _scoring, _events);
        _system.Apply(PowerUpKind.PiercingLaser, _player, _scoring, _events);

        //Act
        _system.Update(1, _events);

        //Assert
        _system.ActiveTimers().Select(t => t.Key).Should().Equal(PowerUpKind.SpreadShot, PowerUpKind.PiercingLaser);
        _system.TimeLeft(PowerUpKind.SpreadShot).Should().BeApproximately(11, 1e-9);
        _system.TimeLeft(PowerUpKind.PiercingLaser).Should().BeApproximately(9, 1e-9);
    }

    [Fact]
    public void timers_expire_once_with_an_event()
    {
        //Arrange
        _system.Apply(PowerUpKind.SpeedBoost, _player, _scoring, _events);
        var expired = 0;

        //Act
        for (var step = 0; step < 60 * 12; step++)
        {
            _events.Clear();
            _system.Update(TestSupport.Dt, _events);
            expired += _events.CountOf(GameEventKind.PowerUpExpired);
        }

        //Assert
        expired.Should().Be(1);
        _system.IsActive(PowerUpKind.SpeedBoost).Should().BeFalse();
    }

    [Fact]
    public void shield_stacks_to_three_then_pays_a_bonus()
    {
        //Act
        for (var i = 0; i < 4; i++)
        {
            _system.Apply(PowerUpKind.ShieldBubble, _player, _scoring, _events);
        }

        //Assert
        _player.ShieldStrength.Should().Be(3);
        _scoring.Score.Should().Be(PowerUpSystem.SurplusShieldBonus * 150 / 100);
    }

    [Fact]
    public void extra_lives_stack_to_nine_then_pay_a_bonus()
    {
        //Act
        for (var i = 0; i < 7; i++)
        {
            _system.Apply(PowerUpKind.ExtraLife, _player, _scoring, _events);
        }

        //Assert
        _player.Lives.Should().Be(PlayerShip.MaxLives);
        _scoring.Score.Should().Be(PowerUpSystem.SurplusLifeBonus * 150 / 100);
        _events.CountOf(GameEventKind.BonusAwarded).Should().Be(1);
    }

    [Fact]
    public void bombs_stack_to_three_then_pay_a_bonus()
    {
        //Act
        for (var i = 0; i < 5; i++)
        {
            _system.Apply(PowerUpKind.Bomb, _player, _scoring, _events);
        }

        //Assert
        _player.Bombs.Should().Be(PlayerShip.MaxBombs);
        _scoring.Score.Should().Be(2 * PowerUpSystem.SurplusBombBonus * 150 / 100);
    }

    [Fact]
    public void drops_drift_down_and_time_out()
    {
        //Arrange
        var drop = _system.Spawn(PowerUpKind.RapidFire, 100, 100, 1, _events);
        var lost = 0;

        //Act
        _system.Update(1, _events);
        var yAfterOneSecond = drop.Y;
        for (var step = 0; step < 60 * 8; step++)
        {
            _events.Clear();
            _system.Update(TestSupport.Dt, _events);
            lost += _events.CountOf(GameEventKind.PowerUpLost);
        }

        //Assert
        yAfterOneSecond.Should().Be(100 + PowerUpSystem.DriftSpeed);
        lost.Should().Be(1);
        _system.Drops.Should().BeEmpty();
    }

    [Fact]
    public void CollectTouching_applies_and_reports_only_touching_drops()
    {
        //Arrange
        _system.Spawn(PowerUpKind.Bomb, _player.X, _player.Y, 1, _events);
        _system.Spawn(PowerUpKind.Bomb, _player.X + 300, _player.Y, 2, _events);

        //Act
        _system.CollectTouching(_player, _scoring, _events);

        //Assert
        _player.Bombs.Should().Be(1);
        _system.Drops.Should().ContainSingle(d => d.Id == 2);
        _events.CountOf(GameEventKind.PowerUpCollected).Should().Be(1);
    }

    [Fact]
    public void CollectTouching_ignores_an_absent_ship()
    {
        //Arrange
        _system.Spawn(PowerUpKind.Bomb, _player.X, _player.Y, 1, _events);
        _player.Destroy(_events);

        //Act
        _system.CollectTouching(_player, _scoring, _events);

        //Assert
        _system.Drops.Should().HaveCount(1);
    }

    [Fact]
    public void TryDrop_respects_the_chance()
    {
        //Arrange
        var random = new GameRandom(4);

        //Act
        var never = Enumerable.Range(0, 200).Select(i => _system.TryDrop(0, 0, 0, random, i, _events)).ToList();
        var always = _system.TryDrop(0, 0, 100, random, 999, _events);

        //Assert
        never.Should().OnlyContain(d => d == null);
        always.Should().NotBeNull();
    }

    [Fact]
    public void ChooseKind_follows_the_weights()
    {
        //Arrange
        var random = new GameRandom(11);
        var counts = new Dictionary<PowerUpKind, int>();

        //Act
        for (var i = 0; i < 100000; i++)
        {
            var kind = PowerUpSystem.ChooseKind(random);
            counts[kind] = counts.GetValueOrDefault(kind) + 1;
        }

        //Assert
        counts.Should().HaveCount(7);
        counts.Should().OnlyContain(kv => Math.Abs((kv.Value / 1000.0) - PowerUpSystem.WeightOf(kv.Key)) < 1.0);
    }
}
