namespace BrixInvaders.GameLogic;

/// <summary>Boss life cycle.</summary>
public enum BossState
{
    /// <summary>Sliding in from above; every hit is absorbed by armour.</summary>
    Entering = 0,

    /// <summary>Weaving and attacking.</summary>
    Fighting = 1,

    /// <summary>Core destroyed; exploding for <see cref="Boss.DyingDuration"/> seconds (play the explosion chain).</summary>
    Dying = 2,

    /// <summary>Gone; the sector is clear.</summary>
    Gone = 3,
}
