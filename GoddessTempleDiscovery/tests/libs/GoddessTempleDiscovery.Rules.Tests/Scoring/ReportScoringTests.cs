using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Scoring;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Scoring;

public class ReportScoringTests
{
    private static DiscoveryCard Card(string id, Period period, int tier = 1) => Fixtures.Discovery(id, tier, period: period);

    [Fact]
    public void Three_cards_of_one_period_are_a_stratigraphy_report() =>
        ReportScoring.Classify(new[] { Card("a", Period.Kassite), Card("b", Period.Kassite), Card("c", Period.Kassite) }).Should().Be(ReportKind.Stratigraphy);

    [Fact]
    public void Three_consecutive_periods_in_any_order_are_a_sequence_report() =>
        ReportScoring.Classify(new[] { Card("a", Period.Kassite), Card("b", Period.UrIII), Card("c", Period.OldBabylonian) }).Should().Be(ReportKind.Sequence);

    [Fact]
    public void A_gap_in_the_periods_makes_a_plain_report() =>
        ReportScoring.Classify(new[] { Card("a", Period.Kassite), Card("b", Period.UrIII), Card("c", Period.NeoBabylonian) }).Should().Be(ReportKind.Plain);

    [Fact]
    public void A_repeated_period_in_a_run_makes_a_plain_report() =>
        ReportScoring.Classify(new[] { Card("a", Period.Kassite), Card("b", Period.UrIII), Card("c", Period.OldBabylonian), Card("d", Period.Kassite) }).Should().Be(ReportKind.Plain);

    [Fact]
    public void Fewer_than_three_cards_are_always_plain() =>
        ReportScoring.Classify(new[] { Card("a", Period.Kassite), Card("b", Period.Kassite) }).Should().Be(ReportKind.Plain);

    [Fact]
    public void Score_adds_the_kind_bonus_the_photographer_and_the_season()
    {
        //Arrange
        var cards = new[] { Card("a", Period.LateUruk, 7), Card("b", Period.JemdetNasr, 6), Card("c", Period.EarlyDynastic, 5), Card("d", Period.Akkadian, 5) };

        //Act
        var (kind, points) = ReportScoring.Score(cards, hasPhotographer: true, publishBonusSeason: true);

        //Assert
        kind.Should().Be(ReportKind.Sequence);
        points.Should().Be(4 + 3 + 3 + 3 + 8 + 1 + 1);
    }
}
