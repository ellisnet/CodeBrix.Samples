namespace BrixInvaders.Game.Rendering;

/// <summary>A clickable rectangle in world units (a link on a card or on the credits screen).</summary>
/// <param name="X">Centre X.</param>
/// <param name="Y">Centre Y.</param>
/// <param name="Width">Width.</param>
/// <param name="Height">Height.</param>
/// <param name="Url">The link it opens.</param>
public sealed record Hotspot(double X, double Y, double Width, double Height, string Url)
{
    /// <summary>Whether a world point is inside.</summary>
    /// <param name="x">World X.</param>
    /// <param name="y">World Y.</param>
    /// <returns>Whether the point hits the hotspot.</returns>
    public bool Contains(double x, double y) =>
        x >= X - (Width / 2) && x <= X + (Width / 2) && y >= Y - (Height / 2) && y <= Y + (Height / 2);
}
