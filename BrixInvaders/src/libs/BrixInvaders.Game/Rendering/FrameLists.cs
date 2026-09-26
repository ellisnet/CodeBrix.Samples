using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using SkiaSharp;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// One frame's two engine draw lists, in playfield pixels: the world (drawn under the particles) and the overlay (HUD
/// and menus, drawn over everything, and the clickable links as hit regions), plus the two faces of the Kenney future
/// font the painters write with. Built on the engine thread; the render side only reads the published copies.
/// </summary>
/// <param name="World">The world list.</param>
/// <param name="Overlay">The overlay list.</param>
/// <param name="Font">The Kenney future typeface.</param>
/// <param name="ThinFont">The thin face.</param>
public sealed record FrameLists(DrawList World, DrawList Overlay, SKTypeface Font, SKTypeface ThinFont)
{
    /// <summary>Starts a new frame on both lists.</summary>
    public void Clear()
    {
        World.Clear();
        Overlay.Clear();
    }

    /// <summary>Hands both finished lists to their drawings.</summary>
    public void Publish()
    {
        World.Publish();
        Overlay.Publish();
    }
}
