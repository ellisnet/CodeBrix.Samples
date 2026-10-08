using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Scoring;

/// <summary>How a Preliminary Report is classified and scored (DESIGN.md section 5).</summary>
public static class ReportScoring
{
    /// <summary>The kind of a report of these cards.</summary>
    /// <remarks>
    /// Decisions: a report of fewer than three cards (the engine never makes one; the rule is kept for safety) is always
    /// <see cref="ReportKind.Plain"/>. A sequence report needs every card from a different period and those periods
    /// consecutive in <see cref="Period"/> order, three or more of them; a mixed report with repeated periods is plain.
    /// </remarks>
    /// <param name="cards">The Discoveries.</param>
    /// <returns>The kind.</returns>
    public static ReportKind Classify(IReadOnlyList<DiscoveryCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        if (cards.Count < GameRules.MinReportCards)
        {
            return ReportKind.Plain;
        }

        var periods = cards.Select(c => (int)c.Period).ToArray();
        if (periods.Distinct().Count() == 1)
        {
            return ReportKind.Stratigraphy;
        }

        if (periods.Distinct().Count() == periods.Length && periods.Length >= GameRules.MinSequencePeriods)
        {
            var sorted = periods.OrderBy(p => p).ToArray();
            var consecutive = true;
            for (var i = 1; i < sorted.Length; i++)
            {
                if (sorted[i] != sorted[i - 1] + 1)
                {
                    consecutive = false;
                    break;
                }
            }

            if (consecutive)
            {
                return ReportKind.Sequence;
            }
        }

        return ReportKind.Plain;
    }

    /// <summary>The points a report of these cards scores.</summary>
    /// <param name="cards">The Discoveries.</param>
    /// <param name="hasPhotographer">True when the team holds a Photographer (+1).</param>
    /// <param name="publishBonusSeason">True in the PublishBonus season (+1).</param>
    /// <returns>The kind and the points.</returns>
    public static (ReportKind Kind, int Points) Score(IReadOnlyList<DiscoveryCard> cards, bool hasPhotographer, bool publishBonusSeason)
    {
        var kind = Classify(cards);
        var points = cards.Sum(c => c.Points);
        points += kind switch
        {
            ReportKind.Stratigraphy => GameRules.StratigraphyBonusPerCard * cards.Count,
            ReportKind.Sequence => GameRules.SequenceBonusPerCard * cards.Count,
            _ => 0,
        };
        if (hasPhotographer)
        {
            points += GameRules.PhotographerBonus;
        }

        if (publishBonusSeason)
        {
            points += GameRules.SeasonPublishBonus;
        }

        return (kind, points);
    }
}
