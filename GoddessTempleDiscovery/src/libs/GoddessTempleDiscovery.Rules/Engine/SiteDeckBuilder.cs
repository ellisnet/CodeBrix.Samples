using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Builds the Site deck in three bands so the row starts shallow and deepens (DESIGN.md section 4).</summary>
internal static class SiteDeckBuilder
{
    /// <summary>
    /// Returns the deck, top card first. Decision: the cards are ordered by tier (equal tiers in a random order), cut
    /// into three bands of a third each, and each band is shuffled on its own; in the bottom band the deep-sounding
    /// cards (tier 9) are moved to the end. With the design's spread of tiers the top band then holds tiers 1 to 4,
    /// the cards are sorted by tier (ties in random order) and cut into thirds, so the shallowest third is dealt first
    /// and the deepest last; a lopsided catalog still deals shallowest first.
    /// </summary>
    internal static List<DiscoveryCard> Build(IReadOnlyList<DiscoveryCard> cards, GameRandom random)
    {
        var keyed = cards.Select(c => (Card: c, Key: random.NextUInt64())).ToList();
        var sorted = keyed.OrderBy(k => k.Card.Tier).ThenBy(k => k.Key).Select(k => k.Card).ToList();
        var n = sorted.Count;
        var a = n / 3;
        var b = 2 * n / 3;
        var top = sorted.GetRange(0, a);
        var middle = sorted.GetRange(a, b - a);
        var bottom = sorted.GetRange(b, n - b);
        random.Shuffle(top);
        random.Shuffle(middle);
        random.Shuffle(bottom);
        var deck = new List<DiscoveryCard>(n);
        deck.AddRange(top);
        deck.AddRange(middle);
        deck.AddRange(bottom.Where(c => c.Tier != DepthTiers.Deepest));
        deck.AddRange(bottom.Where(c => c.Tier == DepthTiers.Deepest));
        return deck;
    }
}
