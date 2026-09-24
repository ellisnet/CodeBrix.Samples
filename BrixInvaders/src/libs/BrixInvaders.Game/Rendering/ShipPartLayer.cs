namespace BrixInvaders.Game.Rendering;

/// <summary>Where an upgrade part is drawn relative to the player's hull.</summary>
public enum ShipPartLayer
{
    /// <summary>Under the hull (engines, flames, the nose cannon that pokes out from under the nose).</summary>
    UnderHull = 0,

    /// <summary>Over the hull and its damage overlay (wing pods, emitter barrels).</summary>
    OverHull = 1,
}
