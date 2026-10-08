namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>What a report of some cards from the hand would score if published now.</summary>
/// <param name="Kind">The report kind.</param>
/// <param name="Points">The points it would score, bonuses included.</param>
/// <param name="CanPublish">True when the report is legal now.</param>
public sealed record ReportPreview(ReportKind Kind, int Points, bool CanPublish);
