using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>A published Preliminary Report: safe points, numbered per team like the real ones.</summary>
/// <param name="Number">1 for the First Preliminary Report, 2 for the Second, and so on.</param>
/// <param name="Cards">The Discoveries laid down.</param>
/// <param name="Kind">The report kind.</param>
/// <param name="Points">The points it scored, bonuses included.</param>
/// <param name="SeasonYear">The season it was published in, such as "1930/31".</param>
public sealed record Report(int Number, IReadOnlyList<DiscoveryCard> Cards, ReportKind Kind, int Points, string SeasonYear)
{
    /// <summary>The report's title, such as "Second Preliminary Report".</summary>
    public string Title => GameRules.ReportTitle(Number);
}
