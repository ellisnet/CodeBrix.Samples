using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineFavorTests
{
    private static GameEngine WithFavor(FavorEffect effect)
    {
        var catalog = Fixtures.Catalog(Fixtures.Effects(), favors: new[] { Fixtures.Favor(effect) });
        return Fixtures.Engine(catalog: catalog);
    }

    [Fact]
    public void Doubles_draw_a_favor_once_both_dice_are_spent()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        Harness.Roll(engine, 2, 2);

        //Act
        var first = engine.Apply(new StudyAction(DiceChoice.DieA));
        var second = engine.Apply(new StudyAction(DiceChoice.DieB));

        //Assert
        first.Events.OfType<FavorDrawn>().Should().BeEmpty();
        second.Events.OfType<FavorDrawn>().Single().Card.Effect.Should().Be(FavorEffect.OnePoint);
        engine.State.FavorDrawnThisTurn.Should().BeTrue();
        engine.State.Journal.Last().Kind.Should().Be(JournalEntryKind.Favor);
        engine.Apply(new EndTurnAction()).Events.OfType<FavorDrawn>().Should().BeEmpty();
    }

    [Fact]
    public void Unspent_doubles_draw_their_favor_at_the_end_of_the_turn()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        Harness.Roll(engine, 5, 5);

        //Act
        var result = engine.Apply(new EndTurnAction());

        //Assert
        result.Events.OfType<FavorDrawn>().Should().HaveCount(1);
    }

    [Fact]
    public void A_die_spent_as_a_sum_counts_as_both()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        Harness.Roll(engine, 3, 3);

        //Act
        var result = engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        result.Events.OfType<FavorDrawn>().Should().HaveCount(1);
    }

    [Fact]
    public void Non_doubles_draw_no_favor()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        Harness.Roll(engine, 3, 4);

        //Act
        engine.Apply(new StudyAction(DiceChoice.Both));
        var result = engine.Apply(new EndTurnAction());

        //Assert
        engine.State.Events.OfType<FavorDrawn>().Should().BeEmpty();
        result.Events.OfType<TurnEnded>().Should().HaveCount(1);
    }

    [Fact]
    public void One_point_scores_at_once()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        var team = engine.State.CurrentTeam;
        Harness.Roll(engine, 1, 1);

        //Act
        engine.Apply(new EndTurnAction());

        //Assert
        team.PublishedPoints.Should().Be(1);
    }

    [Fact]
    public void Free_tablet_draws_a_tablet_at_once()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.FreeTablet);
        Harness.Roll(engine, 1, 1);

        //Act
        engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        engine.State.CurrentTeam.Tablets.Count.Should().Be(1 + 1 + 1);
    }

    [Fact]
    public void Free_worker_gains_a_worker_at_once()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.FreeWorker);
        Harness.Roll(engine, 1, 1);

        //Act
        var result = engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        engine.State.CurrentTeam.Workers.Should().Be(3);
        result.Events.OfType<WorkerGained>().Single().Count.Should().Be(1);
    }

    [Fact]
    public void Free_survey_can_be_used_the_same_turn()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.FreeSurvey);
        Harness.Roll(engine, 1, 1);

        //Act
        engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        engine.State.CurrentTeam.FreeSurveyPending.Should().BeTrue();
        engine.IsLegal(new SurveyAction(0, null)).Should().BeTrue();
    }

    [Fact]
    public void Plus_two_lasts_this_turn_and_next_then_lapses()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.PlusTwoNextDig);
        var team = engine.State.CurrentTeam;
        Harness.Roll(engine, 1, 1);
        engine.Apply(new StudyAction(DiceChoice.Both));
        engine.State.FavorDeck.Clear();
        engine.State.FavorDiscard.Clear();

        //Act
        var thisTurn = team.PlusTwoPending;
        Harness.PassTurn(engine);
        var otherTeamsTurn = team.PlusTwoPending;
        Harness.PassTurn(engine);
        var nextTurn = team.PlusTwoPending;
        Harness.PassTurn(engine);

        //Assert
        thisTurn.Should().BeTrue();
        otherTeamsTurn.Should().BeTrue();
        nextTurn.Should().BeTrue();
        team.PlusTwoPending.Should().BeFalse();
    }

    [Fact]
    public void Recruit_discount_sets_the_pending_flag()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.RecruitDiscount);
        Harness.Roll(engine, 1, 1);

        //Act
        engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        engine.State.CurrentTeam.RecruitDiscountPending.Should().BeTrue();
    }

    [Fact]
    public void Extra_die_rolls_three_next_turn_and_keeps_the_best_two()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.ExtraDieNextTurn);
        var team = engine.State.CurrentTeam;
        Harness.Roll(engine, 1, 1);
        engine.Apply(new EndTurnAction());
        Harness.PassTurn(engine);

        //Act
        engine.Apply(new RollAction());

        //Assert
        engine.State.CurrentTeam.Should().BeSameAs(team);
        team.ExtraDieNextTurn.Should().BeFalse();
        var dice = engine.State.Dice;
        dice.Count.Should().Be(3);
        var unkept = Enumerable.Range(0, 3).Single(i => !engine.State.ChosenDice.Contains(i));
        dice[unkept].Should().BeLessThanOrEqualTo(Math.Min(engine.State.DieA, engine.State.DieB));
    }

    [Fact]
    public void No_hand_limit_skips_the_discard_this_turn()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.NoHandLimit);
        Harness.Roll(engine, 4, 4);
        for (var i = 0; i < 8; i++)
        {
            Harness.GiveDiscovery(engine, Fixtures.Discovery("h" + i, 1));
        }

        //Act
        engine.Apply(new EndTurnAction());

        //Assert
        engine.State.Phase.Should().Be(GamePhase.AwaitRoll);
        engine.State.Teams.Sum(t => t.Hand.Count).Should().Be(8);
    }

    [Fact]
    public void The_favor_deck_reshuffles_when_it_runs_out()
    {
        //Arrange
        var engine = WithFavor(FavorEffect.OnePoint);
        var team = engine.State.CurrentTeam;
        Harness.Roll(engine, 1, 1);
        engine.Apply(new EndTurnAction());
        Harness.PassTurn(engine);
        Harness.Roll(engine, 2, 2);

        //Act
        engine.Apply(new EndTurnAction());

        //Assert
        team.PublishedPoints.Should().Be(2);
        engine.State.Journal.Count(j => j.Kind == JournalEntryKind.Favor).Should().Be(1);
    }
}
