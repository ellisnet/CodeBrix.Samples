using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Game.Tests.Support;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Session;

public class TableSessionTests : IClassFixture<SessionFixture>
{
    private readonly SessionFixture _fixture;

    public TableSessionTests(SessionFixture fixture)
    {
        _fixture = fixture;
    }

    private static GameEngine NewEngine(int seed = 2718) =>
        new GameEngine(new GameSetup(
            new[]
            {
                new SeatSetup("Alpha", null, SeatKind.Computer, Temperament.Surveyor),
                new SeatSetup("Beta", null, SeatKind.Computer, Temperament.DeepDigger),
                new SeatSetup("Gamma", null, SeatKind.Computer, Temperament.Scholar),
                new SeatSetup("Delta", null, SeatKind.Computer, Temperament.Surveyor),
            },
            TurnsPerSeason: 1,
            Seed: seed));

    //Plays computer actions until the season index moves on (or the game ends), settling the table after each
    private static HashSet<string> PlayOneSeason(TableSession session, Random random)
    {
        var raised = new HashSet<string>(StringComparer.Ordinal);
        var engine = session.Engine;
        var startSeason = engine.State.SeasonIndex;
        for (var guard = 0; guard < 400 && !engine.State.IsGameOver && engine.State.SeasonIndex == startSeason; guard++)
        {
            var team = engine.State.CurrentTeam;
            var action = ComputerBrain.Choose(engine, team.Temperament, random);
            foreach (var e in session.Apply(action).Events)
            {
                raised.Add(e.GetType().Name);
            }

            session.Settle();
        }

        return raised;
    }

    [Fact]
    public void starting_the_session_deals_the_site_row_and_the_expedition_row()
    {
        //Arrange
        var session = new TableSession(NewEngine(), _fixture.Artwork);

        //Act
        session.Start();
        session.Settle();

        //Assert
        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            session.SiteSlots[slot].Pile.Cards.Should().ContainSingle();
            var card = session.SiteSlots[slot].Pile.Cards[0];
            TableSession.RulesCard(card).Should().BeSameAs(session.Engine.State.SiteRow[slot]);
            card.IsFaceUp.Should().BeFalse();
        }

        for (var slot = 0; slot < GameRules.ExpeditionRowSize; slot++)
        {
            session.ExpeditionSlots[slot].Pile.Cards.Should().ContainSingle();
            TableSession.RulesCard(session.ExpeditionSlots[slot].Pile.Cards[0]).Should().Be(session.Engine.State.ExpeditionRow[slot]);
            session.ExpeditionSlots[slot].Pile.Cards[0].IsFaceUp.Should().BeTrue();
        }

        session.BannerSeason.Should().BeSameAs(session.Engine.State.Seasons[0]);
        session.DisplayedTeam.Should().Be(session.Engine.State.CurrentTeam.Name);
    }

    [Fact]
    public void a_full_season_drives_the_table_headless_and_keeps_it_in_step_with_the_engine()
    {
        //Arrange
        var session = new TableSession(NewEngine(), _fixture.Artwork);
        session.Start();
        session.Settle();
        var list = new DrawList();

        //Act
        var raised = PlayOneSeason(session, new Random(5));
        list.Clear();
        session.Table.Draw(list);

        //Assert
        session.Engine.State.SeasonIndex.Should().Be(1);
        list.Count.Should().BeGreaterThan(10);
        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            var engineCard = session.Engine.State.SiteRow[slot];
            var pile = session.SiteSlots[slot].Pile;
            if (engineCard == null)
            {
                pile.Cards.Should().BeEmpty();
            }
            else
            {
                pile.Cards.Should().ContainSingle();
                TableSession.RulesCard(pile.Cards[0]).Should().BeSameAs(engineCard);
            }
        }

        var team = session.Engine.State.CurrentTeam;
        session.DisplayedTeam.Should().Be(team.Name);
        session.HandArea.Pile.Cards.Select(TableSession.RulesCard)
            .Should().Equal(team.Hand.Cast<object>().Concat(team.Tablets).ToArray());
        raised.Should().BeSubsetOf(session.TranslatedEventKinds);
    }

    [Fact]
    public void the_slot_labels_match_the_engine_site_row()
    {
        //Arrange
        var session = new TableSession(NewEngine(99), _fixture.Artwork);
        session.Start();
        session.Settle();

        //Act
        var labels = session.SlotLabels();

        //Assert
        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            var card = session.Engine.State.SiteRow[slot];
            labels[slot].Layer.Should().Be(DepthTiers.Name(card.Tier));
            labels[slot].Period.Should().Be(CardText.PeriodName(card.Period));
            labels[slot].DigNumber.Should().Be(session.Engine.PreviewDig(slot, DiceChoice.DieA, 0, null).Needed);
            labels[slot].Points.Should().Be(card.Points);
            labels[slot].IsStarred.Should().Be(card.IsStarred);
        }
    }

    [Fact]
    public void the_table_dice_show_the_engine_values_after_a_roll()
    {
        //Arrange
        var session = new TableSession(NewEngine(31), _fixture.Artwork);
        session.Start();
        session.Settle();

        //Act
        session.Apply(new RollAction());
        session.Settle();

        //Assert
        var values = session.Engine.State.Dice;
        session.DiceInPlay.Should().Be(values.Count);
        for (var i = 0; i < values.Count; i++)
        {
            session.Dice[i].Die.Result.Value.Should().Be(values[i]);
        }
    }

    [Fact]
    public void every_event_kind_of_the_engine_is_translated_over_a_whole_game()
    {
        //Arrange
        var session = new TableSession(NewEngine(4242), _fixture.Artwork);
        session.Start();
        session.Settle();
        var random = new Random(11);
        var raised = new HashSet<string>(StringComparer.Ordinal);

        //Act
        while (!session.Engine.State.IsGameOver)
        {
            raised.UnionWith(PlayOneSeason(session, random));
        }

        //Assert
        raised.Should().BeSubsetOf(session.TranslatedEventKinds);
        session.TranslatedEventKinds.Should().Contain(new[]
        {
            nameof(SeasonStarted), nameof(TurnStarted), nameof(DiceRolled), nameof(SiteExcavated), nameof(SiteRefilled),
            nameof(TurnEnded), nameof(GameEnded), nameof(ReportPublished), nameof(WorkerGained), nameof(JournalEntryAdded),
        });
        session.IsBusy.Should().BeFalse();
    }
}
