using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class SiteDeckBuilderTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2718)]
    [InlineData(31415)]
    public void Deck_is_dealt_in_three_bands_that_deepen(int seed)
    {
        //Arrange
        var cards = Fixtures.Discoveries();

        //Act
        var deck = SiteDeckBuilder.Build(cards, new GameRandom(seed));

        //Assert
        var third = deck.Count / 3;
        deck.Take(third).Max(c => c.Tier).Should().BeLessThanOrEqualTo(4);
        deck.Skip(third).Take(third).Min(c => c.Tier).Should().BeGreaterThanOrEqualTo(3);
        deck.Skip(third).Take(third).Max(c => c.Tier).Should().BeLessThanOrEqualTo(7);
        deck.Skip(2 * third).Min(c => c.Tier).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Deep_sounding_cards_come_last()
    {
        //Act
        var deck = SiteDeckBuilder.Build(Fixtures.Discoveries(), new GameRandom(5));

        //Assert
        var deepCount = deck.Count(c => c.Tier == DepthTiers.Deepest);
        deck.Skip(deck.Count - deepCount).All(c => c.Tier == DepthTiers.Deepest).Should().BeTrue();
    }

    [Fact]
    public void Deck_holds_every_card_exactly_once()
    {
        //Arrange
        var cards = Fixtures.Discoveries();

        //Act
        var deck = SiteDeckBuilder.Build(cards, new GameRandom(9));

        //Assert
        deck.Select(c => c.Id).OrderBy(i => i).Should().Equal(cards.Select(c => c.Id).OrderBy(i => i));
    }

    [Fact]
    public void Order_within_a_band_depends_on_the_seed()
    {
        //Act
        var a = SiteDeckBuilder.Build(Fixtures.Discoveries(), new GameRandom(1)).Select(c => c.Id).ToArray();
        var b = SiteDeckBuilder.Build(Fixtures.Discoveries(), new GameRandom(2)).Select(c => c.Id).ToArray();
        var again = SiteDeckBuilder.Build(Fixtures.Discoveries(), new GameRandom(1)).Select(c => c.Id).ToArray();

        //Assert
        a.Should().NotEqual(b);
        a.Should().Equal(again);
    }
}
