using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEnginePublishTests
{
    private static GameEngine WithHand(SeasonEffect effect, params DiscoveryCard[] cards)
    {
        var engine = Fixtures.Engine(Fixtures.Effects(effect));
        Harness.Roll(engine, 1, 2);
        foreach (var card in cards)
        {
            Harness.GiveDiscovery(engine, card);
        }

        return engine;
    }

    private static string[] Ids(GameEngine engine) => engine.State.CurrentTeam.Hand.Select(c => c.Id).ToArray();

    [Fact]
    public void A_report_needs_three_cards_from_the_hand()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None, Fixtures.Discovery("a", 1), Fixtures.Discovery("b", 2));

        //Assert
        engine.IsLegal(new PublishAction(new[] { "a", "b" })).Should().BeFalse();
        engine.IsLegal(new PublishAction(new[] { "a", "b", "zzz" })).Should().BeFalse();
        engine.IsLegal(new PublishAction(new[] { "a", "b", "a" })).Should().BeFalse();
        engine.IsLegal(new PublishAction(null)).Should().BeFalse();
    }

    [Fact]
    public void A_plain_report_scores_the_printed_points()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None,
            Fixtures.Discovery("a", 1, period: Period.Seleucid),
            Fixtures.Discovery("b", 4, period: Period.UrIII),
            Fixtures.Discovery("c", 9, period: Period.Ubaid));

        //Act
        var result = engine.Apply(new PublishAction(Ids(engine)));

        //Assert
        var report = result.Events.OfType<ReportPublished>().Single().Report;
        report.Kind.Should().Be(ReportKind.Plain);
        report.Points.Should().Be(1 + 2 + 6);
        engine.State.CurrentTeam.Hand.Should().BeEmpty();
        engine.State.CurrentTeam.PublishedPoints.Should().Be(9);
    }

    [Fact]
    public void A_stratigraphy_report_adds_one_per_card()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None,
            Fixtures.Discovery("a", 1, period: Period.Seleucid),
            Fixtures.Discovery("b", 1, period: Period.Seleucid),
            Fixtures.Discovery("c", 1, period: Period.Seleucid),
            Fixtures.Discovery("d", 1, period: Period.Seleucid));

        //Act
        engine.Apply(new PublishAction(Ids(engine)));

        //Assert
        var report = engine.State.CurrentTeam.Reports.Single();
        report.Kind.Should().Be(ReportKind.Stratigraphy);
        report.Points.Should().Be(4 + 4);
    }

    [Fact]
    public void A_sequence_report_adds_two_per_card()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None,
            Fixtures.Discovery("a", 7, period: Period.LateUruk),
            Fixtures.Discovery("b", 6, period: Period.JemdetNasr),
            Fixtures.Discovery("c", 5, period: Period.EarlyDynastic));

        //Act
        engine.Apply(new PublishAction(Ids(engine)));

        //Assert
        var report = engine.State.CurrentTeam.Reports.Single();
        report.Kind.Should().Be(ReportKind.Sequence);
        report.Points.Should().Be(4 + 3 + 3 + 6);
    }

    [Fact]
    public void The_photographer_adds_one_to_every_report()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None, Fixtures.Discovery("a", 1, period: Period.Seleucid), Fixtures.Discovery("b", 2, period: Period.Achaemenid), Fixtures.Discovery("c", 9, period: Period.Ubaid));
        Harness.Give(engine, SpecialistRole.Photographer);

        //Act
        engine.Apply(new PublishAction(Ids(engine)));

        //Assert
        engine.State.CurrentTeam.Reports.Single().Points.Should().Be(1 + 1 + 6 + 1);
    }

    [Fact]
    public void The_publish_bonus_season_adds_one_to_every_report()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.PublishBonus, Fixtures.Discovery("a", 1, period: Period.Seleucid), Fixtures.Discovery("b", 2, period: Period.Achaemenid), Fixtures.Discovery("c", 9, period: Period.Ubaid));
        Harness.Give(engine, SpecialistRole.Photographer);

        //Act
        engine.Apply(new PublishAction(Ids(engine)));

        //Assert
        engine.State.CurrentTeam.Reports.Single().Points.Should().Be(1 + 1 + 6 + 1 + 1);
    }

    [Fact]
    public void Reports_are_numbered_per_team_and_journaled()
    {
        //Arrange
        var engine = WithHand(SeasonEffect.None,
            Fixtures.Discovery("a", 1), Fixtures.Discovery("b", 1), Fixtures.Discovery("c", 1),
            Fixtures.Discovery("d", 2), Fixtures.Discovery("e", 2), Fixtures.Discovery("f", 2));

        //Act
        engine.Apply(new PublishAction(new[] { "a", "b", "c" }));
        engine.Apply(new PublishAction(new[] { "d", "e", "f" }));

        //Assert
        var reports = engine.State.CurrentTeam.Reports;
        reports.Select(r => r.Number).Should().Equal(1, 2);
        reports[0].Title.Should().Be("First Preliminary Report");
        reports[1].Title.Should().Be("Second Preliminary Report");
        reports[0].SeasonYear.Should().Be("1912/13");
        engine.State.Journal.Count(j => j.Kind == JournalEntryKind.Report).Should().Be(2);
        engine.State.Teams.Where(t => t != engine.State.CurrentTeam).All(t => t.Reports.Count == 0).Should().BeTrue();
    }

    [Fact]
    public void Publish_is_open_after_both_dice_are_spent_but_not_before_the_roll()
    {
        //Arrange
        var engine = Fixtures.Engine();
        Harness.GiveDiscovery(engine, Fixtures.Discovery("a", 1));
        Harness.GiveDiscovery(engine, Fixtures.Discovery("b", 1));
        Harness.GiveDiscovery(engine, Fixtures.Discovery("c", 1));
        var publish = new PublishAction(new[] { "a", "b", "c" });

        //Act
        var beforeRoll = engine.IsLegal(publish);
        Harness.Roll(engine, 1, 2);
        engine.Apply(new StudyAction(DiceChoice.Both));

        //Assert
        beforeRoll.Should().BeFalse();
        engine.IsLegal(publish).Should().BeTrue();
        engine.PreviewReport(publish.CardIds).Points.Should().Be(6);
    }
}
