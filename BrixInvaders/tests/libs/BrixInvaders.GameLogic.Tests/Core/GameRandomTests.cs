using System;
using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class GameRandomTests
{
    private static List<T> Draw<T>(int count, Func<T> next)
    {
        var values = new List<T>(count);
        for (var i = 0; i < count; i++)
        {
            values.Add(next());
        }

        return values;
    }

    [Fact]
    public void same_seed_gives_the_same_sequence()
    {
        //Arrange
        var a = new GameRandom(1234);
        var b = new GameRandom(1234);

        //Act
        var first = Draw(1000, a.NextUInt64);
        var second = Draw(1000, b.NextUInt64);

        //Assert
        first.Should().Equal(second);
    }

    [Fact]
    public void different_seeds_give_different_sequences()
    {
        //Arrange
        var a = new GameRandom(1);
        var b = new GameRandom(2);

        //Act
        var first = Draw(100, a.NextUInt64);
        var second = Draw(100, b.NextUInt64);

        //Assert
        first.Where((value, i) => value != second[i]).Count().Should().BeGreaterThan(95);
    }

    [Fact]
    public void seed_zero_still_produces_values()
    {
        //Arrange
        var random = new GameRandom(0);

        //Act
        var first = random.NextUInt64();
        var second = random.NextUInt64();

        //Assert
        first.Should().NotBe(0UL);
        second.Should().NotBe(first);
        random.Seed.Should().Be(0);
    }

    [Fact]
    public void NextDouble_stays_in_zero_to_one()
    {
        //Arrange
        var random = new GameRandom(99);

        //Act
        var values = Draw(10000, random.NextDouble);

        //Assert
        values.Min().Should().BeGreaterThanOrEqualTo(0.0);
        values.Max().Should().BeLessThan(1.0);
    }

    [Fact]
    public void Next_stays_in_range_and_hits_every_value()
    {
        //Arrange
        var random = new GameRandom(5);

        //Act
        var values = Draw(5000, () => random.Next(3, 10));

        //Assert
        values.Distinct().OrderBy(v => v).Should().Equal(3, 4, 5, 6, 7, 8, 9);
    }

    [Fact]
    public void Next_rejects_a_non_positive_bound()
    {
        //Arrange
        var random = new GameRandom(5);

        //Act
        Action act = () => random.Next(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Chance_zero_and_hundred_are_certain()
    {
        //Arrange
        var random = new GameRandom(8);

        //Act
        var never = Draw(1000, () => random.Chance(0));
        var always = Draw(1000, () => random.Chance(100));

        //Assert
        never.Should().OnlyContain(v => !v);
        always.Should().OnlyContain(v => v);
    }

    [Fact]
    public void NextDouble_range_overload_scales()
    {
        //Arrange
        var random = new GameRandom(3);

        //Act
        var values = Draw(1000, () => random.NextDouble(16, 24));

        //Assert
        values.Min().Should().BeGreaterThanOrEqualTo(16);
        values.Max().Should().BeLessThan(24);
    }
}
