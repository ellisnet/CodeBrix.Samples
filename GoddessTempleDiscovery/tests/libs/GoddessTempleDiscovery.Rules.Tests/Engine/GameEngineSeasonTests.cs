using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineSeasonTests
{
    [Fact]
    public void The_free_survey_season_gives_every_team_one_free_survey_that_lapses_at_season_end()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.FreeSurvey));
        var teams = engine.State.Teams.ToArray();

        //Act
        engine.Apply(new RollAction());
        var granted = teams.All(t => t.FreeSurveyPending);
        engine.Apply(new SurveyAction(0, null));
        var usedOnce = engine.IsLegal(new SurveyAction(0, null));
        Harness.PassSeason(engine);

        //Assert
        granted.Should().BeTrue();
        usedOnce.Should().BeFalse();
        teams.Any(t => t.FreeSurveyPending).Should().BeFalse();
    }

    [Fact]
    public void The_gain_worker_season_pays_every_team_at_once()
    {
        //Act
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.GainWorker), seats: 3);

        //Assert
        engine.State.Teams.All(t => t.Workers == 3).Should().BeTrue();
        engine.State.Events.OfType<WorkerGained>().Should().HaveCount(3);
    }

    [Fact]
    public void The_kassite_season_makes_tier_three_one_cheaper()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.KassiteCheaper));
        Harness.Roll(engine, 4, 1);
        Harness.PutSite(engine, 0, Fixtures.Discovery("k", 3, period: Period.Kassite));
        Harness.PutSite(engine, 1, Fixtures.Discovery("u", 4, period: Period.UrIII));

        //Assert
        engine.PreviewDig(0, DiceChoice.DieA, 0, null).Needed.Should().Be(4);
        engine.PreviewDig(0, DiceChoice.DieA, 0, null).Modifiers.Should().Contain("Dig Number -1 (season)");
        engine.PreviewDig(1, DiceChoice.DieA, 0, null).Needed.Should().Be(6);
    }

    [Fact]
    public void The_deep_season_makes_tier_nine_one_cheaper()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.DeepCheaper));
        Harness.Roll(engine, 5, 5);
        Harness.PutSite(engine, 0, Fixtures.Discovery("deep", 9));
        Harness.PutSite(engine, 1, Fixtures.Discovery("uruk-v", 8));

        //Assert
        engine.PreviewDig(0, DiceChoice.Both, 0, null).Needed.Should().Be(10);
        engine.PreviewDig(0, DiceChoice.Both, 0, null).CanDig.Should().BeTrue();
        engine.PreviewDig(1, DiceChoice.Both, 0, null).Needed.Should().Be(10);
    }

    [Fact]
    public void The_reroll_season_allows_one_reroll_of_an_unspent_die()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.RerollOnce));
        Harness.Roll(engine, 1, 6);

        //Act
        var rerolls = engine.LegalActions().OfType<RerollAction>().Select(r => r.DieIndex).ToArray();
        var result = engine.Apply(new RerollAction(0));

        //Assert
        rerolls.Should().Equal(0, 1);
        var rolled = result.Events.OfType<DieRerolled>().Single();
        engine.State.Dice[0].Should().Be(rolled.Value);
        engine.State.CurrentTeam.RerollAvailable.Should().BeFalse();
        engine.IsLegal(new RerollAction(1)).Should().BeFalse();
    }

    [Fact]
    public void The_reroll_lapses_at_season_end()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.RerollOnce));

        //Act
        Harness.PassSeason(engine);

        //Assert
        engine.State.Teams.Any(t => t.RerollAvailable).Should().BeFalse();
    }

    [Fact]
    public void A_spent_die_cannot_be_rerolled()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.RerollOnce));
        Harness.Roll(engine, 1, 6);
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.IsLegal(new RerollAction(0)).Should().BeFalse();
        engine.IsLegal(new RerollAction(1)).Should().BeTrue();
        engine.IsLegal(new RerollAction(5)).Should().BeFalse();
    }

    [Fact]
    public void The_workers_capped_season_allows_two_free_workers_per_dig()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.WorkersCapped));
        engine.State.CurrentTeam.Workers = 4;
        Harness.Roll(engine, 4, 1);
        Harness.PutSite(engine, 0, Fixtures.Discovery("s", 4));

        //Act
        var three = engine.PreviewDig(0, DiceChoice.DieA, 3, null);
        var result = engine.Apply(new DigAction(0, DiceChoice.DieA, 2, null));

        //Assert
        three.CanDig.Should().BeFalse();
        engine.State.CurrentTeam.Workers.Should().Be(4);
        result.Events.OfType<WorkersSpent>().Should().BeEmpty();
        engine.LegalActions().OfType<DigAction>().All(d => d.Workers <= 2).Should().BeTrue();
    }

    [Fact]
    public void The_first_team_rotates_each_season()
    {
        //Arrange
        var engine = Fixtures.Engine(seats: 3);
        var firsts = new System.Collections.Generic.List<int>();

        //Act
        for (var i = 0; i < 4; i++)
        {
            firsts.Add(engine.State.CurrentTeam.Index);
            Harness.PassSeason(engine);
        }

        //Assert
        firsts.Should().Equal(0, 1, 2, 0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Each_team_takes_its_turns_in_rounds_each_season(int turnsPerSeason)
    {
        //Arrange
        var engine = Fixtures.Engine(seats: 3, turnsPerSeason: turnsPerSeason);
        engine.ClearEvents();

        //Act
        Harness.PassSeason(engine);

        //Assert
        var order = engine.State.Events.OfType<TurnEnded>().Select(e => e.TeamName).ToArray();
        var teams = engine.State.Teams.Select(t => t.Name).ToArray();
        order.Length.Should().Be(3 * turnsPerSeason);
        order.Should().Equal(Enumerable.Range(0, 3 * turnsPerSeason).Select(i => teams[i % 3]));
    }

    [Fact]
    public void Every_team_gains_one_worker_at_season_end()
    {
        //Arrange
        var engine = Fixtures.Engine(seats: 4);

        //Act
        Harness.PassSeason(engine);

        //Assert
        engine.State.SeasonIndex.Should().Be(1);
        engine.State.Teams.All(t => t.Workers == 3).Should().BeTrue();
        engine.State.Phase.Should().Be(GamePhase.SeasonStart);
    }

    [Fact]
    public void The_final_season_brings_the_mask_into_the_row()
    {
        //Arrange
        var engine = Fixtures.Engine(Fixtures.Effects(SeasonEffect.FinalSeason));

        //Assert
        engine.State.SiteRow.Count(c => c != null && c.Id == Fixtures.MaskId).Should().Be(1);
        engine.State.SiteDeck.Any(c => c.Id == Fixtures.MaskId).Should().BeFalse();
        engine.State.SiteDeckCount.Should().Be(36 - 5);
    }

    [Fact]
    public void The_final_season_leaves_unpublished_finds_in_the_hand_to_score_half()
    {
        //Arrange
        var effects = Fixtures.Effects(SeasonEffect.FinalSeason);
        var engine = Fixtures.Engine(effects);
        var team = engine.State.CurrentTeam;
        Harness.GiveDiscovery(engine, Fixtures.Discovery("a", 9));
        Harness.GiveDiscovery(engine, Fixtures.Discovery("b", 4));

        //Act
        Harness.PassSeason(engine);

        //Assert: nothing is published for the team; the crates score half at the end (DESIGN section 7)
        team.Hand.Count.Should().Be(2);
        team.Reports.Should().BeEmpty();
        var score = engine.FinalScores().Single(f => f.TeamName == team.Name);
        score.Published.Should().Be(0);
        score.UnpublishedHalf.Should().Be((6 + 2) / 2);
    }

    [Fact]
    public void The_game_ends_after_the_last_season()
    {
        //Arrange
        var engine = Fixtures.Engine(turnsPerSeason: 1);

        //Act
        for (var i = 0; i < 12; i++)
        {
            Harness.PassSeason(engine);
        }

        //Assert
        engine.State.IsGameOver.Should().BeTrue();
        engine.State.Events.OfType<SeasonStarted>().Count().Should().Be(12);
        engine.State.Events.OfType<GameEnded>().Single().Scores.Length.Should().Be(2);
    }
}
