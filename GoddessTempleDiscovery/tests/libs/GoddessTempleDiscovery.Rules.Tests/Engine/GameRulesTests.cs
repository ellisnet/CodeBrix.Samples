using System;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameRulesTests
{
    [Theory]
    [InlineData(1, "First Preliminary Report")]
    [InlineData(3, "Third Preliminary Report")]
    [InlineData(12, "Twelfth Preliminary Report")]
    [InlineData(21, "Preliminary Report 21")]
    public void Report_titles_follow_the_real_numbering(int number, string title) => GameRules.ReportTitle(number).Should().Be(title);

    [Fact]
    public void Report_numbers_start_at_one()
    {
        //Act
        Action act = () => GameRules.ReportTitle(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(Difficulty.Easy, -1)]
    [InlineData(Difficulty.Standard, 0)]
    [InlineData(Difficulty.Hard, 1)]
    public void Difficulty_shifts_by_one(Difficulty difficulty, int shift) => GameRules.DifficultyShift(difficulty).Should().Be(shift);
}
