using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Scoring;

/// <summary>The end-of-game tally (DESIGN.md section 7), valid at any moment of the game.</summary>
public static class FinalScoring
{
    /// <summary>Scores every team, returned in seat order with ranks.</summary>
    /// <remarks>
    /// Decisions: unpublished Discoveries score half their summed points, rounded down once for the whole hand (not
    /// card by card). The Star bonus needs at least one star-marked Discovery; when nobody has one, nobody gains it.
    /// Teams still level after both tie-breaks (most Discoveries, then deepest Discovery) share the rank.
    /// </remarks>
    /// <param name="teams">The teams.</param>
    /// <returns>The scores.</returns>
    public static FinalScore[] Compute(IReadOnlyList<TeamState> teams)
    {
        ArgumentNullException.ThrowIfNull(teams);
        var stars = teams.Select(t => t.StarCount).ToArray();
        var mostStars = stars.Length == 0 ? 0 : stars.Max();
        var raw = new List<FinalScore>();
        foreach (var team in teams)
        {
            var published = team.PublishedPoints;
            var unpublishedHalf = team.Hand.Sum(c => c.Points) / 2;
            var tablets = team.Tablets.Count * TabletCard.PointValue;
            var sets = Enum.GetValues<TabletKind>().Min(k => team.Tablets.Count(t => t.Kind == k));
            var setBonus = sets * GameRules.TabletSetBonus;
            var starBonus = mostStars > 0 && team.StarCount == mostStars ? GameRules.StarBonus : 0;
            var all = team.Hand.Concat(team.Reports.SelectMany(r => r.Cards)).ToArray();
            raw.Add(new FinalScore(
                team.Name,
                team.Index,
                published,
                unpublishedHalf,
                tablets,
                setBonus,
                starBonus,
                published + unpublishedHalf + tablets + setBonus + starBonus,
                all.Length,
                all.Length == 0 ? 0 : all.Max(c => c.Tier),
                team.StarCount,
                0));
        }

        return raw.Select(s => s with { Rank = 1 + raw.Count(o => Compare(o, s) < 0) }).ToArray();
    }

    /// <summary>Orders two scores: negative when <paramref name="a"/> places ahead of <paramref name="b"/>.</summary>
    /// <param name="a">One score.</param>
    /// <param name="b">The other.</param>
    /// <returns>Negative, zero (tied after both tie-breaks) or positive.</returns>
    public static int Compare(FinalScore a, FinalScore b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        var c = b.Total.CompareTo(a.Total);
        if (c != 0)
        {
            return c;
        }

        c = b.DiscoveryCount.CompareTo(a.DiscoveryCount);
        return c != 0 ? c : b.DeepestTier.CompareTo(a.DeepestTier);
    }
}
