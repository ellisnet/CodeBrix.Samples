using System.Linq;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Cards;

public class CardViewsTests
{
    [Fact]
    public void a_discovery_view_is_a_newspaper_page_with_its_pictures()
    {
        //Arrange
        var card = Catalog.Discoveries.First(d => d.IsStarred);

        //Act
        var view = CardViews.For(card, CardFaceLibrary.ForCatalog(), "1930/31", "The Lapis Road Society", null, 6, seed: 2718);

        //Assert - the edition this game prints, with its byline
        view.Kind.Should().Be(CardKind.Discovery);
        view.Headline.Should().Be(Editions.For(2718, card).Headline);
        view.Byline.Should().Be(Editions.For(2718, card).Byline);
        view.Dateline.Should().Be("WARKA, IRAQ — WINTER 1930/31");
        view.IsStarred.Should().BeTrue();
        view.PngFace.Length.Should().BeGreaterThan(1000);
        view.ArtPng.Length.Should().BeGreaterThan(1000);
        view.Facts.Select(f => f.Label).Should().Contain(new[] { "Excavated by", "Dig Number", "Points" });
        view.AutoCloseSeconds.Should().Be(6);
    }

    [Fact]
    public void the_final_season_says_what_stays_in_the_crates_scores_half()
    {
        //Arrange
        var season = Catalog.Seasons.First(s => s.Effect == SeasonEffect.FinalSeason);

        //Act
        var view = CardViews.For(season, CardFaceLibrary.ForCatalog(), 0);

        //Assert
        view.Facts.Single(f => f.Label == "Effect").Value.Should().Contain("publish what you can; what stays in the crates scores half");
    }

    [Fact]
    public void a_tablet_view_prints_its_point_value_from_the_rules()
    {
        //Arrange
        var tablet = Catalog.Tablets.FirstOrDefault();
        if (tablet == null)
        {
            return;
        }

        //Act
        var view = CardViews.For(tablet, CardFaceLibrary.ForCatalog(), "1912/13", 0);

        //Assert
        view.Facts.Single(f => f.Label == "Worth").Value.Should().StartWith(TabletCard.PointValue + " points");
    }
}
