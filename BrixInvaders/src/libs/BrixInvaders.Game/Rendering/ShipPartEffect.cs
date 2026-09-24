namespace BrixInvaders.Game.Rendering;

/// <summary>How an upgrade part moves while it is drawn.</summary>
public enum ShipPartEffect
{
    /// <summary>A still picture.</summary>
    None = 0,

    /// <summary>Cycles through its frames (engine flames).</summary>
    Flicker = 1,

    /// <summary>Its opacity pulses (the emitter glow).</summary>
    Pulse = 2,
}
