using System.Linq;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineHandLimitTests
{
    [Fact]
    public void Seven_cards_need_no_discard()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 1, 2);
        for (var i = 0; i < 6; i++)
        {
            Harness.GiveDiscovery(engine, Fixtures.Discovery("h" + i, 1));
        }

        //Act
        engine.Apply(new EndTurnAction());

        //Assert
        engine.State.Phase.Should().Be(GamePhase.AwaitRoll);
    }

    [Fact]
    public void Over_the_limit_the_team_discards_down_to_seven_then_the_turn_passes()
    {
        //Arrange
        var engine = Fixtures.Engine();
        var team = engine.State.CurrentTeam;
        Harness.Roll(engine, 1, 2);
        for (var i = 0; i < 8; i++)
        {
            Harness.GiveDiscovery(engine, Fixtures.Discovery("h" + i, 1));
        }

        //Act
        engine.Apply(new EndTurnAction());
        var phaseAfterEnd = engine.State.Phase;
        var first = engine.Apply(new DiscardAction("h0"));
        var stillDiscarding = engine.State.Phase;
        var second = engine.Apply(new DiscardAction(team.Tablets[0].Id));

        //Assert
        phaseAfterEnd.Should().Be(GamePhase.TurnEnd);
        stillDiscarding.Should().Be(GamePhase.TurnEnd);
        first.Events.OfType<HandLimitDiscard>().Single().CardId.Should().Be("h0");
        second.Events.OfType<TurnEnded>().Should().HaveCount(1);
        team.HandCount.Should().Be(7);
        team.Tablets.Should().BeEmpty();
        engine.State.TabletDiscardCount.Should().Be(1);
        engine.State.CurrentTeam.Should().NotBeSameAs(team);
    }
}
