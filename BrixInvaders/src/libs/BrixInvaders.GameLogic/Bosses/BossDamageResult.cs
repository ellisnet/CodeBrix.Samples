namespace BrixInvaders.GameLogic;

/// <summary>What a hit on a boss section did.</summary>
public enum BossDamageResult
{
    /// <summary>Nothing: the section is already destroyed or the boss is not fighting.</summary>
    Ignored = 0,

    /// <summary>The hit was absorbed by armour (entering boss, or core while other sections live).</summary>
    Armoured = 1,

    /// <summary>The section lost health.</summary>
    Damaged = 2,

    /// <summary>The section was destroyed.</summary>
    SectionDestroyed = 3,

    /// <summary>The core was destroyed: the boss is defeated.</summary>
    Defeated = 4,
}
