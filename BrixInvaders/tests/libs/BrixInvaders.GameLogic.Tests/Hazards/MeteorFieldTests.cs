using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class MeteorFieldTests
{
    [Fact]
    public void a_shower_spawns_meteors_then_stops_a_second_before_it_ends()
    {
        //Arrange
        var field = new MeteorField();
        var events = new GameEvents();
        var random = new GameRandom(2);
        var ids = TestSupport.Ids();
        var spawnTimes = new List<double>();
        var seen = new HashSet<int>();
        var ended = 0;
        field.StartShower(events);

        //Act
        for (var step = 0; step < 60 * 7; step++)
        {
            events.Clear();
            field.Update(TestSupport.Dt, random, 1.0, ids, events);
            ended += events.CountOf(GameEventKind.MeteorShowerEnded);
            foreach (var meteor in field.Meteors.Where(m => seen.Add(m.Id)))
            {
                spawnTimes.Add(step * TestSupport.Dt);
            }
        }

        //Assert
        spawnTimes.Count.Should().BeInRange(12, 13);
        spawnTimes.Max().Should().BeLessThan(MeteorField.ShowerDuration - MeteorField.QuietTail + TestSupport.Dt);
        ended.Should().Be(1);
        field.IsShowerActive.Should().BeFalse();
    }

    [Fact]
    public void meteors_fall_and_are_culled_below_the_playfield()
    {
        //Arrange
        var field = new MeteorField();
        var events = new GameEvents();
        field.StartShower(events);
        field.Update(TestSupport.Dt, new GameRandom(3), 1.0, TestSupport.Ids(), events);
        var meteor = field.Meteors[0];
        var y = meteor.Y;

        //Act
        field.Update(0.5, new GameRandom(3), 1.0, TestSupport.Ids(), events);
        var fell = meteor.Y - y;
        for (var i = 0; i < 60 * 10; i++)
        {
            field.Update(TestSupport.Dt, new GameRandom(3), 1.0, TestSupport.Ids(), events);
        }

        //Assert
        fell.Should().BeGreaterThan(0);
        field.Meteors.Should().BeEmpty();
    }

    [Fact]
    public void big_meteors_take_two_hits()
    {
        //Act
        var big = new Meteor(1, MeteorSize.Big, 0, 0, 0, 1, 0);
        var small = new Meteor(2, MeteorSize.Small, 0, 0, 0, 1, 0);

        //Assert
        big.Health.Should().Be(2);
        small.Health.Should().Be(1);
        big.Box.Width.Should().Be(Meteor.BigSize);
        small.Box.Width.Should().Be(Meteor.SmallSize);
    }

    [Fact]
    public void ClearAll_removes_every_meteor()
    {
        //Arrange
        var field = new MeteorField();
        var events = new GameEvents();
        field.StartShower(events);
        field.Update(2, new GameRandom(3), 1.0, TestSupport.Ids(), events);

        //Act
        var removed = field.ClearAll();

        //Assert
        removed.Should().BeGreaterThan(0);
        field.Meteors.Should().BeEmpty();
    }
}
