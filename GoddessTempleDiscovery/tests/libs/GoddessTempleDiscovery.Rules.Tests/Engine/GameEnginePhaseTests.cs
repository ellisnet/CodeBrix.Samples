using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEnginePhaseTests
{
    private static readonly GameAction[] SpendActions =
    {
        new DigAction(0, DiceChoice.DieA, 0, null),
        new RecruitAction(0, DiceChoice.Both),
        new StudyAction(DiceChoice.DieA),
        new SurveyAction(0, DiceChoice.DieA),
        new PublishAction(new[] { "a", "b", "c" }),
        new EndTurnAction(),
        new RerollAction(0),
    };

    [Fact]
    public void Season_start_allows_only_the_roll()
    {
        //Arrange
        var engine = Fixtures.Engine();

        //Act
        var legal = engine.LegalActions();

        //Assert
        legal.Count.Should().Be(1);
        legal[0].Should().BeOfType<RollAction>();
        SpendActions.Any(engine.IsLegal).Should().BeFalse();
        engine.IsLegal(new DiscardAction(engine.State.CurrentTeam.Tablets[0].Id)).Should().BeFalse();
    }

    [Fact]
    public void Await_roll_allows_only_the_roll()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.PassTurn(engine);

        //Act
        var legal = engine.LegalActions();

        //Assert
        engine.State.Phase.Should().Be(GamePhase.AwaitRoll);
        legal.Should().HaveCount(1);
        legal[0].Should().BeOfType<RollAction>();
        SpendActions.Any(engine.IsLegal).Should().BeFalse();
    }

    [Fact]
    public void Rolling_moves_to_spend_with_two_dice()
    {
        //Arrange
        var engine = Fixtures.Engine();

        //Act
        var result = engine.Apply(new RollAction());

        //Assert
        engine.State.Phase.Should().Be(GamePhase.Spend);
        engine.State.Dice.Count.Should().Be(2);
        engine.State.Dice.All(d => d >= 1 && d <= 6).Should().BeTrue();
        engine.State.ChosenDice.Should().Equal(0, 1);
        result.Events.OfType<DiceRolled>().Single().Values.Should().Equal(engine.State.Dice);
    }

    [Fact]
    public void Spend_forbids_a_second_roll_and_discards()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 3, 4);

        //Assert
        engine.IsLegal(new RollAction()).Should().BeFalse();
        engine.IsLegal(new DiscardAction(engine.State.CurrentTeam.Tablets[0].Id)).Should().BeFalse();
        engine.IsLegal(new EndTurnAction()).Should().BeTrue();
        engine.IsLegal(new StudyAction(DiceChoice.Both)).Should().BeTrue();
    }

    [Fact]
    public void Every_listed_spend_action_is_legal()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 5, 6);
        engine.State.CurrentTeam.Workers = 3;
        Harness.GiveDiscovery(engine, Fixtures.Discovery("x1", 1));
        Harness.GiveDiscovery(engine, Fixtures.Discovery("x2", 1));
        Harness.GiveDiscovery(engine, Fixtures.Discovery("x3", 2));

        //Act
        var legal = engine.LegalActions();

        //Assert
        legal.All(engine.IsLegal).Should().BeTrue();
        legal.OfType<DigAction>().Should().NotBeEmpty();
        legal.OfType<RecruitAction>().Should().NotBeEmpty();
        legal.OfType<StudyAction>().Should().HaveCount(3);
        legal.OfType<PublishAction>().Should().HaveCount(1);
        legal.Last().Should().BeOfType<EndTurnAction>();
    }

    [Fact]
    public void A_spent_die_cannot_be_spent_again_and_both_needs_both()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 2, 2);

        //Act
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.DieAUsed.Should().BeTrue();
        engine.IsLegal(new StudyAction(DiceChoice.DieA)).Should().BeFalse();
        engine.IsLegal(new StudyAction(DiceChoice.Both)).Should().BeFalse();
        engine.IsLegal(new StudyAction(DiceChoice.DieB)).Should().BeTrue();
    }

    [Fact]
    public void Turn_end_allows_only_discards()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 1, 2);
        for (var i = 0; i < 7; i++)
        {
            Harness.GiveDiscovery(engine, Fixtures.Discovery("h" + i, 1));
        }

        //Act
        engine.Apply(new EndTurnAction());

        //Assert
        engine.State.Phase.Should().Be(GamePhase.TurnEnd);
        engine.LegalActions().All(a => a is DiscardAction).Should().BeTrue();
        engine.LegalActions().Count.Should().Be(8);
        engine.IsLegal(new EndTurnAction()).Should().BeFalse();
        engine.IsLegal(new RollAction()).Should().BeFalse();
        engine.IsLegal(new PublishAction(new[] { "h0", "h1", "h2" })).Should().BeFalse();
    }

    [Fact]
    public void Game_over_allows_nothing()
    {
        //Arrange
        var engine = Fixtures.Engine(turnsPerSeason: 1);
        while (!engine.State.IsGameOver)
        {
            Harness.PassTurn(engine);
        }

        //Act
        Action act = () => engine.Apply(new RollAction());

        //Assert
        engine.LegalActions().Should().BeEmpty();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void An_illegal_action_throws_and_changes_nothing()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 1, 1);
        var before = engine.StateHash();
        var events = engine.State.Events.Count;

        //Act
        Action act = () => engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));
        Action nothing = () => engine.Apply(null);

        //Assert
        act.Should().Throw<InvalidOperationException>();
        nothing.Should().Throw<InvalidOperationException>();
        engine.StateHash().Should().Be(before);
        engine.State.Events.Count.Should().Be(events);
    }

    [Fact]
    public void Apply_returns_the_events_it_raised_and_clear_events_empties_the_list()
    {
        //Arrange
        var engine = Fixtures.Engine();

        //Act
        var result = engine.Apply(new RollAction());

        //Assert
        result.Action.Should().BeOfType<RollAction>();
        result.Events.Should().HaveCount(1);
        engine.State.Events.Last().Should().BeSameAs(result.Events[0]);
        engine.ClearEvents();
        engine.State.Events.Should().BeEmpty();
    }

    [Fact]
    public void Publish_action_equality_compares_the_ids()
    {
        //Assert
        new PublishAction(new[] { "a", "b", "c" }).Should().Be(new PublishAction(new[] { "a", "b", "c" }));
        new PublishAction(new[] { "a", "b", "c" }).Should().NotBe(new PublishAction(new[] { "a", "c", "b" }));
    }
}
