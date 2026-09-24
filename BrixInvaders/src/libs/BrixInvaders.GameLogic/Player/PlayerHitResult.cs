namespace BrixInvaders.GameLogic;

/// <summary>What a hit on the player did.</summary>
public enum PlayerHitResult
{
    /// <summary>Nothing: the ship was invulnerable or not on the playfield.</summary>
    Ignored = 0,

    /// <summary>The shield bubble absorbed it (strength -1).</summary>
    Absorbed = 1,

    /// <summary>The hull lost one point; the ship survives.</summary>
    Damaged = 2,

    /// <summary>The ship was destroyed and a life lost.</summary>
    Destroyed = 3,
}
