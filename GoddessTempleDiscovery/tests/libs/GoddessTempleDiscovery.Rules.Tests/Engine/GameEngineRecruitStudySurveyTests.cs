using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineRecruitStudySurveyTests
{
    private static GameEngine Rolled(int a, int b, SeasonEffect effect = SeasonEffect.None)
    {
        var engine = Fixtures.Engine(Fixtures.Effects(effect));
        Harness.Roll(engine, a, b);
        return engine;
    }

    [Fact]
    public void Recruit_needs_a_die_at_least_the_cost()
    {
        //Arrange
        var engine = Rolled(3, 2);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Architect, 3);

        //Assert
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieA)).Should().BeTrue();
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieB)).Should().BeFalse();
        engine.IsLegal(new RecruitAction(0, DiceChoice.Both)).Should().BeTrue();
    }

    [Fact]
    public void Recruit_takes_the_specialist_refills_the_row_and_writes_the_journal()
    {
        //Arrange
        var engine = Rolled(6, 2);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Architect, 3);
        var next = engine.State.ExpeditionDeck[0];

        //Act
        var result = engine.Apply(new RecruitAction(0, DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Specialists.Single().Role.Should().Be(SpecialistRole.Architect);
        engine.State.ExpeditionRow[0].Should().BeSameAs(next);
        result.Events.OfType<SpecialistRecruited>().Should().HaveCount(1);
        result.Events.OfType<ExpeditionRefilled>().Single().Card.Should().BeSameAs(next);
        engine.State.Journal.Last().Kind.Should().Be(JournalEntryKind.Specialist);
    }

    [Fact]
    public void An_empty_expedition_deck_leaves_the_slot_empty()
    {
        //Arrange
        var engine = Rolled(6, 2);
        engine.State.ExpeditionDeck.Clear();

        //Act
        var result = engine.Apply(new RecruitAction(0, DiceChoice.DieA));

        //Assert
        engine.State.ExpeditionRow[0].Should().BeNull();
        result.Events.OfType<ExpeditionRefilled>().Single().Card.Should().BeNull();
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieB)).Should().BeFalse();
    }

    [Fact]
    public void A_team_holds_at_most_four_specialists()
    {
        //Arrange
        var engine = Rolled(6, 6);
        Harness.Give(engine, SpecialistRole.Architect);
        Harness.Give(engine, SpecialistRole.Epigrapher);
        Harness.Give(engine, SpecialistRole.Foreman);
        Harness.Give(engine, SpecialistRole.Photographer);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Surveyor, 2);

        //Assert
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieA)).Should().BeFalse();
        engine.LegalActions().OfType<RecruitAction>().Should().BeEmpty();
    }

    [Fact]
    public void A_team_may_not_hold_two_of_one_role()
    {
        //Arrange
        var engine = Rolled(6, 6);
        Harness.Give(engine, SpecialistRole.Surveyor);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Surveyor, 2);

        //Assert
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieA)).Should().BeFalse();
    }

    [Fact]
    public void The_recruit_cheaper_season_takes_one_off_the_cost()
    {
        //Arrange
        var engine = Rolled(2, 1, SeasonEffect.RecruitCheaper);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Architect, 3);
        engine.State.ExpeditionRowArray[1] = Fixtures.Specialist(SpecialistRole.Surveyor, 2);

        //Assert
        engine.RecruitCost(0).Should().Be(2);
        engine.IsLegal(new RecruitAction(0, DiceChoice.DieA)).Should().BeTrue();
        engine.RecruitCost(1).Should().Be(1);
        engine.IsLegal(new RecruitAction(1, DiceChoice.DieB)).Should().BeTrue();
    }

    [Fact]
    public void A_recruit_discount_favor_takes_one_off_the_next_recruit_only()
    {
        //Arrange
        var engine = Rolled(4, 4);
        engine.State.ExpeditionRowArray[0] = Fixtures.Specialist(SpecialistRole.Foreman, 5);
        engine.State.ExpeditionRowArray[1] = Fixtures.Specialist(SpecialistRole.Architect, 5);
        engine.State.CurrentTeam.RecruitDiscountPending = true;

        //Act
        engine.Apply(new RecruitAction(0, DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.RecruitDiscountPending.Should().BeFalse();
        engine.IsLegal(new RecruitAction(1, DiceChoice.DieB)).Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 2)]
    public void Study_draws_one_and_a_six_draws_two(int die, int draws)
    {
        //Arrange
        var engine = Rolled(die, 1);
        var before = engine.State.CurrentTeam.Tablets.Count;

        //Act
        var result = engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Tablets.Count.Should().Be(before + draws);
        result.Events.OfType<TabletDrawn>().Should().HaveCount(draws);
        engine.State.Journal.Count(j => j.Kind == JournalEntryKind.Tablet).Should().Be(2 + draws);
    }

    [Fact]
    public void The_epigrapher_makes_study_draw_two()
    {
        //Arrange
        var engine = Rolled(1, 1);
        Harness.Give(engine, SpecialistRole.Epigrapher);

        //Act
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Tablets.Count.Should().Be(3);
    }

    [Fact]
    public void The_study_draws_two_season_draws_two()
    {
        //Arrange
        var engine = Rolled(1, 1, SeasonEffect.StudyDrawsTwo);

        //Act
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Tablets.Count.Should().Be(3);
    }

    [Fact]
    public void Study_never_draws_more_than_two()
    {
        //Arrange
        var engine = Rolled(6, 1, SeasonEffect.StudyDrawsTwo);
        Harness.Give(engine, SpecialistRole.Epigrapher);

        //Act
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Tablets.Count.Should().Be(3);
    }

    [Fact]
    public void Study_reshuffles_spent_tablets_and_is_illegal_when_none_are_left()
    {
        //Arrange
        var engine = Rolled(1, 1);
        var spent = engine.State.TabletDeck.ToList();
        engine.State.TabletDeck.Clear();
        engine.State.TabletDiscard.Add(spent[0]);

        //Act
        engine.Apply(new StudyAction(DiceChoice.DieA));

        //Assert
        engine.State.CurrentTeam.Tablets.Should().Contain(spent[0]);
        engine.State.TabletDiscardCount.Should().Be(0);
        engine.IsLegal(new StudyAction(DiceChoice.DieB)).Should().BeFalse();
    }

    [Fact]
    public void A_paid_survey_spends_a_die_and_sends_the_card_to_the_bottom()
    {
        //Arrange
        var engine = Rolled(1, 2);
        var removed = engine.State.SiteRow[2];
        var top = engine.State.SiteDeck[0];

        //Act
        var result = engine.Apply(new SurveyAction(2, DiceChoice.DieA));

        //Assert
        engine.State.DieAUsed.Should().BeTrue();
        engine.State.SiteRow[2].Should().BeSameAs(top);
        engine.State.SiteDeck.Last().Should().BeSameAs(removed);
        var surveyed = result.Events.OfType<Surveyed>().Single();
        surveyed.Removed.Should().BeSameAs(removed);
        surveyed.Added.Should().BeSameAs(top);
    }

    [Fact]
    public void A_free_survey_needs_a_grant()
    {
        //Arrange
        var engine = Rolled(1, 2);

        //Assert
        engine.FreeSurveyAvailable.Should().BeFalse();
        engine.IsLegal(new SurveyAction(0, null)).Should().BeFalse();
    }

    [Fact]
    public void The_surveyor_surveys_free_once_per_turn()
    {
        //Arrange
        var engine = Rolled(1, 2);
        Harness.Give(engine, SpecialistRole.Surveyor);

        //Act
        engine.Apply(new SurveyAction(0, null));

        //Assert
        engine.State.DieAUsed.Should().BeFalse();
        engine.State.CurrentTeam.SurveyorUsedThisTurn.Should().BeTrue();
        engine.IsLegal(new SurveyAction(1, null)).Should().BeFalse();
    }

    [Fact]
    public void The_surveyor_free_use_returns_next_turn()
    {
        //Arrange
        var engine = Rolled(1, 2);
        Harness.Give(engine, SpecialistRole.Surveyor);
        var team = engine.State.CurrentTeam;
        engine.Apply(new SurveyAction(0, null));

        //Act
        Harness.PassTurn(engine);
        Harness.PassTurn(engine);
        engine.Apply(new RollAction());

        //Assert
        engine.State.CurrentTeam.Should().BeSameAs(team);
        engine.IsLegal(new SurveyAction(0, null)).Should().BeTrue();
    }

    [Fact]
    public void A_favor_free_survey_keeps_until_used()
    {
        //Arrange
        var engine = Rolled(1, 2);
        engine.State.CurrentTeam.FavorFreeSurveys = 1;
        var team = engine.State.CurrentTeam;
        Harness.PassTurn(engine);
        Harness.PassTurn(engine);
        engine.Apply(new RollAction());

        //Act
        engine.Apply(new SurveyAction(0, null));

        //Assert
        team.FreeSurveyPending.Should().BeFalse();
        engine.IsLegal(new SurveyAction(0, null)).Should().BeFalse();
    }

    [Fact]
    public void Survey_is_illegal_when_the_site_deck_is_empty()
    {
        //Arrange
        var engine = Rolled(1, 2);
        engine.State.SiteDeck.Clear();

        //Assert
        engine.IsLegal(new SurveyAction(0, DiceChoice.DieA)).Should().BeFalse();
        engine.LegalActions().OfType<SurveyAction>().Should().BeEmpty();
    }
}
