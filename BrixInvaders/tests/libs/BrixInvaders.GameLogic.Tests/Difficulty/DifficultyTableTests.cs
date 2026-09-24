using System;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class DifficultyTableTests
{
    [Fact]
    public void Levels_are_ordered_easiest_first()
        => DifficultyTable.Levels.Should().Equal(Difficulty.Cadet, Difficulty.Pilot, Difficulty.Ace, Difficulty.Legend);

    [Theory]
    [InlineData(Difficulty.Cadet, 100, 5)]
    [InlineData(Difficulty.Pilot, 150, 3)]
    [InlineData(Difficulty.Ace, 200, 3)]
    [InlineData(Difficulty.Legend, 300, 2)]
    public void For_pins_score_multiplier_and_lives(Difficulty level, int percent, int lives)
    {
        //Act
        var settings = DifficultyTable.For(level);

        //Assert
        settings.Level.Should().Be(level);
        settings.ScoreMultiplierPercent.Should().Be(percent);
        settings.Lives.Should().Be(lives);
    }

    [Theory]
    [InlineData(Difficulty.Cadet, "x1")]
    [InlineData(Difficulty.Pilot, "x1.5")]
    [InlineData(Difficulty.Ace, "x2")]
    [InlineData(Difficulty.Legend, "x3")]
    public void MultiplierText_matches_the_percent(Difficulty level, string text) => DifficultyTable.MultiplierText(level).Should().Be(text);

    [Fact]
    public void every_level_is_harder_than_the_one_before()
    {
        //Arrange
        var settings = DifficultyTable.Levels.Select(DifficultyTable.For).ToList();

        //Act
        var pairs = settings.Zip(settings.Skip(1), (easier, harder) => (easier, harder)).ToList();

        //Assert
        pairs.Should().OnlyContain(p =>
            p.harder.ColumnFireInterval < p.easier.ColumnFireInterval
            && p.harder.ShooterInterval < p.easier.ShooterInterval
            && p.harder.MaxEnemyBolts > p.easier.MaxEnemyBolts
            && p.harder.EnemyBoltSpeed > p.easier.EnemyBoltSpeed
            && p.harder.DropHeight > p.easier.DropHeight
            && p.harder.FormationBaseInterval < p.easier.FormationBaseInterval
            && p.harder.DiveInterval < p.easier.DiveInterval
            && p.harder.MaxDivers > p.easier.MaxDivers
            && p.harder.MissileInterval < p.easier.MissileInterval
            && p.harder.MissileCap > p.easier.MissileCap
            && p.harder.BossHealthPercent > p.easier.BossHealthPercent
            && p.harder.BossFireScale < p.easier.BossFireScale
            && p.harder.PowerUpDropPercent < p.easier.PowerUpDropPercent
            && p.harder.ScoreMultiplierPercent > p.easier.ScoreMultiplierPercent
            && p.harder.Lives <= p.easier.Lives
            && p.harder.StartingBombs <= p.easier.StartingBombs);
    }

    [Fact]
    public void For_rejects_an_undefined_level()
    {
        //Act
        Action act = () => DifficultyTable.For((Difficulty)42);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NameOf_is_the_level_name() => DifficultyTable.NameOf(Difficulty.Legend).Should().Be("Legend");
}
