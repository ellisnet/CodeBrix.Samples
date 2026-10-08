using System;
using System.Text;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>
/// Which of a headline's editions a game prints: chosen from the game's seed and the card's id by a stable hash
/// (32-bit FNV-1a of the id's UTF-8 bytes, mixed with the seed), so one game always shows the same edition of a
/// card and different games differ. The count of editions comes from <see cref="Catalog.HeadlineVariantCount"/>,
/// so cards added later need nothing here.
/// </summary>
public static class Editions
{
    private const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;

    /// <summary>The stable hash of an id within a game.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="id">The discovery id or season year.</param>
    /// <returns>The hash.</returns>
    public static uint Hash(int seed, string id)
    {
        var hash = FnvOffset;
        foreach (var b in Encoding.UTF8.GetBytes(id ?? string.Empty))
        {
            hash = (hash ^ b) * FnvPrime;
        }

        //Mix the seed in byte by byte, then once more through the prime so neighbouring seeds spread apart
        var s = unchecked((uint)seed);
        for (var i = 0; i < 4; i++)
        {
            hash = (hash ^ ((s >> (8 * i)) & 0xFF)) * FnvPrime;
        }

        hash ^= hash >> 15;
        return unchecked(hash * FnvPrime);
    }

    /// <summary>The edition a game prints for an id.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="id">The discovery id or season year.</param>
    /// <returns>0 to the variant count less one.</returns>
    public static int Edition(int seed, string id)
    {
        var count = Math.Max(1, Catalog.HeadlineVariantCount(id));
        return (int)(Hash(seed, id) % (uint)count);
    }

    /// <summary>The headline pair a game prints for an id, its byline always filled.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="id">The discovery id or season year.</param>
    /// <param name="title">The title to fall back on.</param>
    /// <param name="subHeadFallback">The sub-head when the edition has none.</param>
    /// <returns>The pair.</returns>
    public static Catalog.HeadlinePair For(int seed, string id, string title, string subHeadFallback)
    {
        var pair = Catalog.HeadlineFor(id, title, Edition(seed, id));
        var subHead = string.IsNullOrWhiteSpace(pair.SubHead) ? subHeadFallback ?? string.Empty : pair.SubHead;
        return new Catalog.HeadlinePair(pair.Headline, subHead, BylineLine(pair.Byline ?? Byline(seed, id)));
    }

    /// <summary>The edition of a discovery a game prints.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="card">The discovery.</param>
    /// <returns>The pair, its byline filled.</returns>
    public static Catalog.HeadlinePair For(int seed, DiscoveryCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (!string.IsNullOrWhiteSpace(card.Headline))
        {
            //A headline written on the card itself is its only edition
            return new Catalog.HeadlinePair(card.Headline,
                string.IsNullOrWhiteSpace(card.SubHead) ? CardText.FirstSentence(card.CardText) : card.SubHead, BylineLine(Byline(seed, card.Id)));
        }

        return For(seed, card.Id, CardText.ShortTitle(card.Title), CardText.FirstSentence(card.CardText));
    }

    /// <summary>The edition of a season a game prints.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="season">The season.</param>
    /// <returns>The pair, its byline filled.</returns>
    public static Catalog.HeadlinePair For(int seed, SeasonCard season)
    {
        ArgumentNullException.ThrowIfNull(season);
        return For(seed, season.Year, season.Title, CardText.FirstSentence(season.Story));
    }

    /// <summary>The paper's byline for an edition that carries none, picked by the same hash.</summary>
    /// <param name="seed">The game's seed.</param>
    /// <param name="id">The id.</param>
    /// <returns>A byline from <see cref="Catalog.Bylines"/>.</returns>
    public static string Byline(int seed, string id)
    {
        var bylines = Catalog.Bylines;
        if (bylines == null || bylines.Count == 0)
        {
            return "From our correspondent at Warka";
        }

        return bylines[(int)((Hash(seed, id) / 7) % (uint)bylines.Count)];
    }

    /// <summary>A byline as the paper sets it under the headline: "By ..." unless it already opens with By or From.</summary>
    /// <param name="byline">The byline.</param>
    /// <returns>The line.</returns>
    public static string BylineLine(string byline)
    {
        var text = (byline ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return string.Empty;
        }

        return text.StartsWith("By ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("From ", StringComparison.OrdinalIgnoreCase)
            ? text
            : "By " + text;
    }
}
