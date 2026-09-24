namespace BrixInvaders.GameLogic;

/// <summary>The five enemy roles. Each role is drawn with its own ship shape.</summary>
public enum EnemyRole
{
    /// <summary>Plain formation member; fires only through the column-fire rule.</summary>
    Grunt = 0,

    /// <summary>Fires aimed bolts on its own cadence.</summary>
    Shooter = 1,

    /// <summary>Carries a shield: the first hit breaks the shield, the second destroys it.</summary>
    Shielded = 2,

    /// <summary>Leaves the formation, swoops at the player and returns.</summary>
    Diver = 3,

    /// <summary>Launches homing missiles.</summary>
    MissileCarrier = 4,
}
