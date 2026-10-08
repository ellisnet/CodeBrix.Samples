namespace GoddessTempleDiscovery.Rules.Journal;

/// <summary>What a Field Journal entry records.</summary>
public enum JournalEntryKind
{
    /// <summary>A Season card.</summary>
    Season,
    /// <summary>An excavated Discovery.</summary>
    Discovery,
    /// <summary>A Tablet drawn.</summary>
    Tablet,
    /// <summary>A Favor drawn.</summary>
    Favor,
    /// <summary>A Specialist recruited.</summary>
    Specialist,
    /// <summary>A published report.</summary>
    Report,
    /// <summary>A real person of the excavations.</summary>
    Person,
    /// <summary>Any other note.</summary>
    Note,
}
