namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The kind of a Preliminary Report, which sets its bonus (DESIGN.md section 5).</summary>
public enum ReportKind
{
    /// <summary>No bonus.</summary>
    Plain,
    /// <summary>Every card from the same period: +1 per card.</summary>
    Stratigraphy,
    /// <summary>A run of three or more consecutive periods: +2 per card.</summary>
    Sequence,
}
