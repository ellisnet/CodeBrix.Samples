using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class ScoringTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 2)]
    [InlineData(19, 2)]
    [InlineData(20, 3)]
    [InlineData(39, 4)]
    [InlineData(40, 5)]
    [InlineData(1000, 5)]
    [InlineData(-5, 1)]
    public void MultiplierFor_steps_every_ten_hits_up_to_five(int chain, int multiplier) => Scoring.MultiplierFor(chain).Should().Be(multiplier);

    [Theory]
    [InlineData(EnemyColour.Black, 1, 10)]
    [InlineData(EnemyColour.Blue, 1, 20)]
    [InlineData(EnemyColour.Green, 1, 30)]
    [InlineData(EnemyColour.Red, 1, 40)]
    [InlineData(EnemyColour.Black, 3, 20)]
    [InlineData(EnemyColour.Red, 6, 65)]
    public void ColourPoints_grow_with_tier_and_sector(EnemyColour colour, int sector, int points) => Scoring.ColourPoints(colour, sector).Should().Be(points);

    [Theory]
    [InlineData(EnemyRole.Grunt, 0)]
    [InlineData(EnemyRole.Shooter, 10)]
    [InlineData(EnemyRole.Shielded, 20)]
    [InlineData(EnemyRole.Diver, 30)]
    [InlineData(EnemyRole.MissileCarrier, 40)]
    public void RoleBonus_pins_each_role(EnemyRole role, int bonus) => Scoring.RoleBonus(role).Should().Be(bonus);

    [Fact]
    public void EnemyBasePoints_doubles_out_of_formation()
    {
        //Act
        var inFormation = Scoring.EnemyBasePoints(EnemyRole.Diver, EnemyColour.Green, 2, false);
        var diving = Scoring.EnemyBasePoints(EnemyRole.Diver, EnemyColour.Green, 2, true);

        //Assert
        inFormation.Should().Be(35 + 30);
        diving.Should().Be(130);
    }

    [Fact]
    public void Award_applies_chain_and_difficulty_multipliers_rounding_down()
    {
        //Arrange
        var scoring = new Scoring(150);
        var events = new GameEvents();
        for (var i = 0; i < 10; i++)
        {
            scoring.RegisterHit(events);
        }

        //Act
        var awarded = scoring.Award(25);

        //Assert
        awarded.Should().Be(75);
        scoring.Score.Should().Be(75);
        new Scoring(150).Award(25).Should().Be(37);
    }

    [Fact]
    public void AwardBonus_skips_the_chain_multiplier()
    {
        //Arrange
        var scoring = new Scoring(300);
        var events = new GameEvents();
        for (var i = 0; i < 40; i++)
        {
            scoring.RegisterHit(events);
        }

        //Act
        var awarded = scoring.AwardBonus(1000);

        //Assert
        awarded.Should().Be(3000);
    }

    [Fact]
    public void RegisterHit_reports_each_multiplier_step()
    {
        //Arrange
        var scoring = new Scoring(100);
        var events = new GameEvents();

        //Act
        for (var i = 0; i < 45; i++)
        {
            scoring.RegisterHit(events);
        }

        //Assert
        TestSupport.Collect(events, GameEventKind.ChainMultiplierChanged).ConvertAll(e => e.Value).Should().Equal(2, 3, 4, 5);
        scoring.ChainCount.Should().Be(45);
        scoring.BestChain.Should().Be(45);
    }

    [Fact]
    public void RegisterMiss_resets_the_chain_and_reports_what_was_lost()
    {
        //Arrange
        var scoring = new Scoring(100);
        var events = new GameEvents();
        for (var i = 0; i < 12; i++)
        {
            scoring.RegisterHit(events);
        }

        events.Clear();

        //Act
        scoring.RegisterMiss(events);

        //Assert
        scoring.ChainCount.Should().Be(0);
        scoring.ChainMultiplier.Should().Be(1);
        scoring.BestChain.Should().Be(12);
        TestSupport.Collect(events, GameEventKind.ChainBroken)[0].Value.Should().Be(12);
        TestSupport.Collect(events, GameEventKind.ChainMultiplierChanged)[0].Value.Should().Be(1);
    }

    [Fact]
    public void RegisterMiss_without_a_chain_is_silent()
    {
        //Arrange
        var scoring = new Scoring(100);
        var events = new GameEvents();

        //Act
        scoring.RegisterMiss(events);

        //Assert
        events.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 1, 50)]
    [InlineData(3, 1, 300)]
    [InlineData(1, 6, 200)]
    [InlineData(9, 1, 300)]
    public void UfoPoints_pick_a_value_and_scale_by_loop(int index, int sector, int points) => Scoring.UfoPoints(index, sector).Should().Be(points);

    [Fact]
    public void boss_and_sector_values_are_pinned()
    {
        //Act
        var boss1 = Scoring.BossDefeatPoints(1);
        var boss5 = Scoring.BossDefeatPoints(5);
        var bonus3 = Scoring.SectorClearBonus(3);

        //Assert
        boss1.Should().Be(2500);
        boss5.Should().Be(4500);
        bonus3.Should().Be(3000);
    }

    [Fact]
    public void constructor_rejects_a_non_positive_percent()
    {
        //Act
        Action act = () => new Scoring(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
