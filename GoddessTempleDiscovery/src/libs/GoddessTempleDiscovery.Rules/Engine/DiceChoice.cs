namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Which die an action spends, or both together as one sum.</summary>
public enum DiceChoice
{
    /// <summary>The first die (the first of the two kept dice when three were rolled).</summary>
    DieA,
    /// <summary>The second die.</summary>
    DieB,
    /// <summary>Both dice together, their values summed.</summary>
    Both,
}
