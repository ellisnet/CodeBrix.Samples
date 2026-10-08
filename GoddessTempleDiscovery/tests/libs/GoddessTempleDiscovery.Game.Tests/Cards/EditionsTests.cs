using System.Linq;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Rendering;
using GoddessTempleDiscovery.Rules.Content;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Cards;

public class EditionsTests
{
    [Fact]
    public void one_game_always_prints_the_same_edition_of_a_card()
    {
        foreach (var card in Catalog.Discoveries)
        {
            //Act
            var first = Editions.For(2718, card);
            var again = Editions.For(2718, card);

            //Assert
            again.Should().Be(first);
            Editions.Edition(2718, card.Id).Should().BeInRange(0, Catalog.HeadlineVariantCount(card.Id) - 1);
        }
    }

    [Fact]
    public void different_games_print_different_editions()
    {
        //Arrange - only cards that have more than one edition can differ
        var cards = Catalog.Discoveries.Where(c => Catalog.HeadlineVariantCount(c.Id) > 1).ToArray();
        if (cards.Length == 0)
        {
            return;
        }

        //Act
        var differing = Enumerable.Range(1, 20).Count(seed =>
            cards.Select(c => Editions.Edition(seed, c.Id)).SequenceEqual(cards.Select(c => Editions.Edition(seed + 1000, c.Id))) == false);

        //Assert
        differing.Should().BeGreaterThan(15);
    }

    [Fact]
    public void every_edition_has_a_headline_and_a_byline_line()
    {
        foreach (var seed in new[] { 1, 42, 2718 })
        {
            foreach (var card in Catalog.Discoveries)
            {
                var pair = Editions.For(seed, card);
                pair.Headline.Should().NotBeNullOrWhiteSpace(card.Id);
                pair.Byline.Should().NotBeNullOrWhiteSpace(card.Id);
            }

            foreach (var season in Catalog.Seasons)
            {
                Editions.For(seed, season).Byline.Should().NotBeNullOrWhiteSpace(season.Year);
            }
        }
    }

    [Fact]
    public void a_byline_reads_as_a_byline()
    {
        //Assert
        Editions.BylineLine("our correspondent").Should().Be("By our correspondent");
        Editions.BylineLine("From our Baghdad bureau").Should().Be("From our Baghdad bureau");
        Editions.BylineLine("By Gertrude").Should().Be("By Gertrude");
    }

    [Fact]
    public void every_picture_of_the_catalog_has_a_gallery_title_and_category()
    {
        //Act
        var all = ArtInfo.All();

        //Assert
        all.Should().HaveCount(ArtCatalog.Keys.Count);
        all.Should().OnlyContain(a => ArtInfo.Categories.Contains(a.Category));
        all.Should().OnlyContain(a => a.Title.Length > 0);
        all.Count(a => a.Subject.Length > 0).Should().BeGreaterThan(all.Count / 2);
        ArtInfo.Read("deco-card-frame").Title.Should().Be("Deco Card Frame");
        ArtInfo.CategoryOf(ArtCatalog.Keys.First(k => k.StartsWith("icon-"))).Should().Be("icons");
    }
}
