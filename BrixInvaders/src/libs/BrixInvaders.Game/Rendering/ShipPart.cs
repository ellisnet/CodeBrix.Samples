using System.Collections.Generic;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// One upgrade part bolted onto the player's ship while a power-up is active. Positions and sizes are in HULL ART
/// pixels (the ship frame's own pixels, centre = 0,0, y down); the painter scales them with the hull by
/// <see cref="ShipLoadout.HullScale"/> so a part sits on the same spot of the hull at any draw size.
/// </summary>
/// <param name="Name">A short name for the log (for example <c>spread pod left</c>).</param>
/// <param name="PowerUp">The power-up the part shows.</param>
/// <param name="Frames">The image keys (see <see cref="SpriteCatalog"/>); more than one only for <see cref="ShipPartEffect.Flicker"/>.</param>
/// <param name="X">Centre X offset from the hull centre, hull art pixels.</param>
/// <param name="Y">Centre Y offset from the hull centre, hull art pixels (positive = towards the tail).</param>
/// <param name="Width">Draw box width, hull art pixels (unrotated).</param>
/// <param name="Height">Draw box height, hull art pixels (unrotated).</param>
/// <param name="Rotation">Clockwise degrees.</param>
/// <param name="Layer">Under or over the hull.</param>
/// <param name="Order">Draw order inside its layer (lower first).</param>
/// <param name="Effect">How the part moves.</param>
public sealed record ShipPart(string Name, PowerUpKind PowerUp, IReadOnlyList<string> Frames, double X, double Y,
    double Width, double Height, double Rotation, ShipPartLayer Layer, int Order, ShipPartEffect Effect)
{
    /// <summary>The frame name of the part's first picture (without the atlas key), for the log.</summary>
    public string FrameName
    {
        get
        {
            SpriteCatalog.TrySplit(Frames[0], out _, out var frame);
            return frame ?? Frames[0];
        }
    }
}
