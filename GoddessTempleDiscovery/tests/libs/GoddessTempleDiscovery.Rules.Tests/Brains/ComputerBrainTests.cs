using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Brains;

public class ComputerBrainTests
{
    [Theory]
    [InlineData(Temperament.Surveyor)]
    [InlineData(Temperament.DeepDigger)]
    [InlineData(Temperament.Scholar)]
    public void The_brain_never_returns_an_illegal_action_across_two_hundred_turns(Temperament temperament)
    {
        //Arrange
        var turns = 0;
        var seed = 100;
        var chooser = new Random(4242);

        //Act
        while (turns < 200)
        {
            var engine = new GameEngine(Fixtures.Setup(seats: 4, seed: seed++, temperaments: new[] { temperament }), Fixtures.Catalog());
            var brainRandom = new Random(seed);
            while (!engine.State.IsGameOver && turns < 200)
            {
                GameAction action;
                if (engine.State.Phase == GamePhase.Spend && chooser.Next(4) == 0)
                {
                    //Wander off the brain's path now and then so it meets states it would not make itself.
                    var legal = engine.LegalActions();
                    action = legal[chooser.Next(legal.Count)];
                }
                else
                {
                    action = ComputerBrain.Choose(engine, temperament, brainRandom);
                    engine.WhyIllegal(action).Should().BeNull();
                }

                turns += engine.Apply(action).Events.OfType<TurnEnded>().Count();
            }
        }

        //Assert
        turns.Should().Be(200);
    }

    [Fact]
    public void The_brain_always_rolls_when_the_dice_wait()
    {
        //Arrange
        var engine = Fixtures.Engine();

        //Act
        var action = ComputerBrain.Choose(engine, Temperament.Scholar, new Random(1));

        //Assert
        action.Should().BeOfType<RollAction>();
    }

    [Fact]
    public void The_brain_discards_at_the_hand_limit()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 1, 2);
        for (var i = 0; i < 8; i++)
        {
            Harness.GiveDiscovery(engine, Fixtures.Discovery("h" + i, i < 4 ? 1 : 9));
        }

        engine.Apply(new EndTurnAction());

        //Act
        var action = ComputerBrain.Choose(engine, Temperament.DeepDigger, new Random(1));

        //Assert
        var discard = action.Should().BeOfType<DiscardAction>().Subject;
        new[] { "h0", "h1", "h2", "h3" }.Should().Contain(discard.CardId);
    }

    [Fact]
    public void The_brain_digs_a_reachable_site_rather_than_end_the_turn()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 5, 1);
        Harness.PutSite(engine, 0, Fixtures.Discovery("kassite", 3));
        engine.State.ExpeditionDeck.Clear();
        System.Array.Clear(engine.State.ExpeditionRowArray);

        //Act
        var action = ComputerBrain.Choose(engine, Temperament.Surveyor, new Random(3));

        //Assert
        action.Should().Be(new DigAction(0, DiceChoice.DieA, 0, null));
    }

    [Fact]
    public void The_brain_ends_the_turn_when_nothing_useful_remains()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.Roll(engine, 1, 2);
        engine.Apply(new StudyAction(DiceChoice.Both));

        //Act
        var action = ComputerBrain.Choose(engine, Temperament.Scholar, new Random(3));

        //Assert
        action.Should().BeOfType<EndTurnAction>();
    }

    [Fact]
    public void The_brain_has_nothing_to_choose_once_the_game_is_over()
    {
        //Arrange
        var engine = Fixtures.Engine(turnsPerSeason: 1);
        Harness.PlayOut(engine, new Random(5));

        //Act
        Action act = () => ComputerBrain.Choose(engine, Temperament.Scholar, new Random(1));

        //Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    public void A_computer_only_game_ends_within_a_bounded_number_of_actions(int seats, int turnsPerSeason)
    {
        //Arrange
        var engine = new GameEngine(Fixtures.Setup(seats: seats, seed: 77, turnsPerSeason: turnsPerSeason), Fixtures.Catalog());
        var bound = 12 * turnsPerSeason * seats * 20;

        //Act
        var actions = Harness.PlayOut(engine, new Random(77), bound);

        //Assert
        engine.State.IsGameOver.Should().BeTrue();
        actions.Should().BeLessThan(bound);
    }

    [Fact]
    public void Temperaments_play_differently()
    {
        //Arrange
        var scores = Enum.GetValues<Temperament>().Select(t =>
        {
            var engine = new GameEngine(Fixtures.Setup(seats: 2, seed: 11, temperaments: new[] { t }), Fixtures.Catalog());
            Harness.PlayOut(engine, new Random(11));
            return engine.StateHash();
        }).ToArray();

        //Assert
        scores.Distinct().Count().Should().Be(3);
    }
}
