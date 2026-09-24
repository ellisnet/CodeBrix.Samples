using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class GameEventsTests
{
    [Fact]
    public void Contains_and_CountOf_look_at_the_kind()
    {
        //Arrange
        var events = new GameEvents();

        //Act
        events.Add(GameEventKind.PlayerFired);
        events.Add(new GameEvent(GameEventKind.EnemyDestroyed, 1, 2, 30));
        events.Add(new GameEvent(GameEventKind.EnemyDestroyed, 3, 4, 40));

        //Assert
        events.Count.Should().Be(3);
        events.Contains(GameEventKind.PlayerFired).Should().BeTrue();
        events.Contains(GameEventKind.GameOver).Should().BeFalse();
        events.CountOf(GameEventKind.EnemyDestroyed).Should().Be(2);
        events[2].Points.Should().Be(40);
    }

    [Fact]
    public void Clear_empties_the_list()
    {
        //Arrange
        var events = new GameEvents();
        events.Add(GameEventKind.PlayerFired);

        //Act
        events.Clear();

        //Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void GameEvent_ToString_names_the_kind() => new GameEvent(GameEventKind.WaveStarted, value: 3).ToString().Should().StartWith("WaveStarted(");
}
