namespace BrixInvaders.GameLogic;

/// <summary>The three projectile kinds.</summary>
public enum ProjectileKind
{
    /// <summary>A player laser bolt (optionally piercing).</summary>
    PlayerBolt = 0,

    /// <summary>An enemy or boss bolt.</summary>
    EnemyBolt = 1,

    /// <summary>A homing missile (turn-rate limited; can be shot down).</summary>
    Missile = 2,
}
