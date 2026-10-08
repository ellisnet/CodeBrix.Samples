using System;
using System.Linq;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Session;

public class NarratorAndControlsTests
{
    private static GameEngine Rolled(int seed)
    {
        var engine = new GameEngine(new GameSetup(
            new[]
            {
                new SeatSetup("The Lapis Road Society", null, SeatKind.Human, Temperament.Surveyor),
                new SeatSetup("Beta", null, SeatKind.Computer, Temperament.Scholar),
            },
            Seed: seed));
        engine.Apply(new RollAction());
        return engine;
    }

    [Fact]
    public void a_dig_is_narrated_as_a_wire_line()
    {
        //Arrange
        var engine = Rolled(77);
        var dig = engine.LegalActions().OfType<DigAction>().First();
        var team = engine.State.CurrentTeam.Name;

        //Act
        var line = Narrator.Describe(engine, team, engine.Apply(dig));

        //Assert
        line.Should().StartWith(team + " digs the ");
        line.Should().Contain(" layer with a ");
    }

    [Fact]
    public void the_best_dig_of_a_slot_is_legal_and_spends_least()
    {
        for (var seed = 1; seed < 40; seed++)
        {
            //Arrange
            var engine = Rolled(seed);
            var controls = new PlayerControls();

            for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
            {
                //Act
                var best = controls.BestDig(engine, slot);

                //Assert
                if (best != null)
                {
                    engine.IsLegal(best).Should().BeTrue();
                    if (engine.LegalActions().OfType<DigAction>().Any(d => d.Slot == slot && d.TabletId == null))
                    {
                        best.TabletId.Should().BeNull("a dig that reaches without a Tablet is preferred");
                    }
                }
                else
                {
                    engine.LegalActions().OfType<DigAction>().Any(d => d.Slot == slot).Should().BeFalse();
                }
            }
        }
    }

    [Fact]
    public void selecting_both_dice_chooses_their_sum()
    {
        //Arrange
        var engine = Rolled(5);
        var controls = new PlayerControls();

        //Act
        controls.ToggleDie(engine, engine.State.ChosenDice[0]);
        controls.ToggleDie(engine, engine.State.ChosenDice[1]);

        //Assert
        controls.Choice(engine).Should().Be(DiceChoice.Both);
        controls.ChoiceValue(engine).Should().Be(engine.State.DieA + engine.State.DieB);
    }

    [Fact]
    public void the_fixed_face_random_always_answers_its_face()
    {
        //Arrange
        var random = new FixedFaceRandom(4);

        //Assert
        random.Next(6).Should().Be(4);
        random.Next(3).Should().Be(2);
    }
}
