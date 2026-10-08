using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Journal;

public class JournalWriterTests
{
    [Fact]
    public void A_discovery_entry_carries_every_text_and_the_sources()
    {
        //Act
        var entry = JournalWriter.ForDiscovery(Fixtures.Discovery("vase", 7, DiscoveryKind.Object, Period.LateUruk, starred: true), "1933/34", "Team A");

        //Assert
        entry.Kind.Should().Be(JournalEntryKind.Discovery);
        entry.Title.Should().Be("Find vase");
        entry.Text.Should().Contain("Card text of vase.");
        entry.Text.Should().Contain("Long text of vase.");
        entry.Text.Should().Contain("Excavated by: The test excavators");
        entry.Text.Should().Contain("Where it is now: Test museum");
        entry.Text.Should().Contain("one of Her stars");
        entry.ArtKey.Should().Be("art-vase");
        entry.SeasonYear.Should().Be("1933/34");
        entry.TeamName.Should().Be("Team A");
        entry.Sources.Should().Be("Source of vase");
    }

    [Fact]
    public void A_season_entry_carries_the_story_the_director_and_the_effect()
    {
        //Act
        var entry = JournalWriter.ForSeason(Fixtures.Season(3, SeasonEffect.DeepCheaper));

        //Assert
        entry.Title.Should().Be("1930/31 - Season 3");
        entry.Text.Should().Contain("Story 3.");
        entry.Text.Should().Contain("Long story 3.");
        entry.Text.Should().Contain("Director: Director 3");
        entry.Text.Should().Contain("Effect: Effect DeepCheaper.");
        entry.TeamName.Should().BeNull();
        entry.Sources.Should().Be("Source of season 3");
    }

    [Fact]
    public void Tablet_favor_and_specialist_entries_carry_their_texts()
    {
        //Act
        var tablet = JournalWriter.ForTablet(Fixtures.Tablet("t", TabletKind.Goddess), "y", "A");
        var favor = JournalWriter.ForFavor(Fixtures.Favor(FavorEffect.OnePoint), "y", "A");
        var specialist = JournalWriter.ForSpecialist(Fixtures.Specialist(SpecialistRole.Epigrapher, 4), "y", "A");

        //Assert
        tablet.Text.Should().Contain("Long text of t.");
        tablet.Sources.Should().Be("Source of t");
        favor.Text.Should().Contain("Fact OnePoint.");
        favor.Text.Should().Contain("Gift OnePoint.");
        specialist.Text.Should().Contain("Who did it at Uruk.");
        specialist.Kind.Should().Be(JournalEntryKind.Specialist);
    }

    [Fact]
    public void A_report_entry_names_the_report_and_its_finds()
    {
        //Arrange
        var report = new Report(2, new[] { Fixtures.Discovery("a", 1), Fixtures.Discovery("b", 1), Fixtures.Discovery("c", 1) }, ReportKind.Stratigraphy, 6, "1936/37");

        //Act
        var entry = JournalWriter.ForReport(report, "Team A");

        //Assert
        entry.Title.Should().Be("Team A: Second Preliminary Report");
        entry.Text.Should().Contain("A stratigraphy report of 3 finds: Find a, Find b, Find c. It scored 6 points.");
        entry.Sources.Should().Be("Source of a; Source of b; Source of c");
    }
}
