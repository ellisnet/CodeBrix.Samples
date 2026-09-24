namespace BrixInvaders.GameLogic;

/// <summary>The seven power-ups.</summary>
public enum PowerUpKind
{
    /// <summary>Timed: three bolts per shot in a fan.</summary>
    SpreadShot = 0,

    /// <summary>Timed: halves the fire interval.</summary>
    RapidFire = 1,

    /// <summary>Timed: bolts pass through targets.</summary>
    PiercingLaser = 2,

    /// <summary>Shield bubble: +1 strength (max 3); each strength absorbs one hit.</summary>
    ShieldBubble = 3,

    /// <summary>Timed: x1.5 movement speed.</summary>
    SpeedBoost = 4,

    /// <summary>+1 life (max 9; surplus pays a bonus).</summary>
    ExtraLife = 5,

    /// <summary>+1 bomb (max 3; surplus pays a bonus).</summary>
    Bomb = 6,
}
