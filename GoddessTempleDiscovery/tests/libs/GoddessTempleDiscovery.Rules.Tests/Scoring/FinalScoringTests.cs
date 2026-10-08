using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Scoring;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Scoring;

public class FinalScoringTests
{
    private static TeamState Team(string name, int index)
    {
        var team = new TeamState(new TeamProfile("p" + index, name, string.Empty, "#000000", string.Empty), name, SeatKind.Computer, Temperament.Scholar, index, index);
        return team;
    }

    [Fact]
    public void Every_part_of_section_seven_is_counted()
    {
        //Arrange
        var team = Team("A", 0);
        team.PublishedPoints = 10;
        team.HandList.Add(Fixtures.Discovery("a", 1));
        team.HandList.Add(Fixtures.Discovery("b", 2));
        team.HandList.Add(Fixtures.Discovery("c", 3, starred: true));
        team.TabletList.AddRange(new[]
        {
            Fixtures.Tablet("t1", TabletKind.Goddess), Fixtures.Tablet("t2", TabletKind.Pantheon), Fixtures.Tablet("t3", TabletKind.Culture),
            Fixtures.Tablet("t4", TabletKind.Timeline), Fixtures.Tablet("t5", TabletKind.Goddess),
        });
        var other = Team("B", 1);

        //Act
        var scores = FinalScoring.Compute(new[] { team, other });

        //Assert
        var a = scores[0];
        a.Published.Should().Be(10);
        a.UnpublishedHalf.Should().Be((1 + 1 + 2) / 2);
        a.Tablets.Should().Be(5 * TabletCard.PointValue);
        a.SetBonus.Should().Be(3);
        a.StarBonus.Should().Be(5);
        a.Total.Should().Be(10 + 2 + 5 * TabletCard.PointValue + 3 + 5);
        a.Rank.Should().Be(1);
        scores[1].Total.Should().Be(0);
        scores[1].Rank.Should().Be(2);
    }

    [Fact]
    public void Unpublished_halves_round_down_over_the_whole_hand()
    {
        //Arrange
        var team = Team("A", 0);
        team.HandList.Add(Fixtures.Discovery("a", 1));
        team.HandList.Add(Fixtures.Discovery("b", 1));
        team.HandList.Add(Fixtures.Discovery("c", 1));

        //Act
        var score = FinalScoring.Compute(new[] { team, Team("B", 1) })[0];

        //Assert
        score.UnpublishedHalf.Should().Be(1);
    }

    [Fact]
    public void Tied_stars_share_the_bonus_and_no_stars_win_nothing()
    {
        //Arrange
        var a = Team("A", 0);
        var b = Team("B", 1);
        var c = Team("C", 2);
        a.HandList.Add(Fixtures.Discovery("s1", 7, starred: true));
        b.ReportList.Add(new Report(1, new[] { Fixtures.Discovery("s2", 7, starred: true) }, ReportKind.Plain, 4, "1930/31"));

        //Act
        var scores = FinalScoring.Compute(new[] { a, b, c });
        var none = FinalScoring.Compute(new[] { Team("X", 0), Team("Y", 1) });

        //Assert
        scores.Select(s => s.StarBonus).Should().Equal(5, 5, 0);
        none.All(s => s.StarBonus == 0).Should().BeTrue();
    }

    [Fact]
    public void Ties_break_on_most_discoveries_then_the_deepest()
    {
        //Arrange
        var a = Team("A", 0);
        var b = Team("B", 1);
        var c = Team("C", 2);
        var d = Team("D", 3);
        a.PublishedPoints = 10;
        b.PublishedPoints = 10;
        c.PublishedPoints = 10;
        d.PublishedPoints = 10;
        a.ReportList.Add(new Report(1, new[] { Fixtures.Discovery("a1", 1), Fixtures.Discovery("a2", 1) }, ReportKind.Plain, 0, "y"));
        b.ReportList.Add(new Report(1, new[] { Fixtures.Discovery("b1", 9) }, ReportKind.Plain, 0, "y"));
        c.ReportList.Add(new Report(1, new[] { Fixtures.Discovery("c1", 3) }, ReportKind.Plain, 0, "y"));
        d.ReportList.Add(new Report(1, new[] { Fixtures.Discovery("d1", 3) }, ReportKind.Plain, 0, "y"));

        //Act
        var scores = FinalScoring.Compute(new[] { a, b, c, d });

        //Assert
        scores.Select(s => s.Rank).Should().Equal(1, 2, 3, 3);
    }
}
