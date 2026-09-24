namespace BrixInvaders.GameLogic;

/// <summary>The kinds of boss section.</summary>
public enum BossSectionKind
{
    /// <summary>The hull core: armoured until every other section is destroyed; destroying it defeats the boss.</summary>
    Core = 0,

    /// <summary>Fires aimed bolts.</summary>
    Turret = 1,

    /// <summary>Fires a fan of five bolts.</summary>
    Cannon = 2,

    /// <summary>Launches homing missiles.</summary>
    MissileBay = 3,
}
