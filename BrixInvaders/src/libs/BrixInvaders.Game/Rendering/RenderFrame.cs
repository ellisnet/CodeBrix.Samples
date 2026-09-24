using System;
using System.Collections.Generic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// One finished frame: the world commands (drawn under the particles and the boss health bar) and the overlay
/// commands (HUD and menus, drawn over everything). Immutable once built, so the render thread can read it while
/// the engine thread builds the next.
/// </summary>
public sealed class RenderFrame
{
    /// <summary>An empty frame.</summary>
    public static readonly RenderFrame Empty =
        new RenderFrame(Array.Empty<DrawCommand>(), Array.Empty<DrawCommand>(), Array.Empty<Hotspot>(), 0);

    /// <summary>Creates a frame.</summary>
    /// <param name="world">The world commands, back to front.</param>
    /// <param name="overlay">The overlay commands, back to front.</param>
    /// <param name="hotspots">The clickable links on this frame.</param>
    /// <param name="number">The frame number.</param>
    public RenderFrame(IReadOnlyList<DrawCommand> world, IReadOnlyList<DrawCommand> overlay, IReadOnlyList<Hotspot> hotspots,
        long number)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
        Hotspots = hotspots ?? throw new ArgumentNullException(nameof(hotspots));
        Number = number;
    }

    /// <summary>The clickable links on this frame.</summary>
    public IReadOnlyList<Hotspot> Hotspots { get; }

    /// <summary>The link under a world point, or null.</summary>
    /// <param name="x">World X.</param>
    /// <param name="y">World Y.</param>
    /// <returns>The link, or null when the point hits none.</returns>
    public string HitTest(double x, double y)
    {
        for (var i = Hotspots.Count - 1; i >= 0; i--)
        {
            if (Hotspots[i].Contains(x, y))
            {
                return Hotspots[i].Url;
            }
        }

        return null;
    }

    /// <summary>The world commands, back to front.</summary>
    public IReadOnlyList<DrawCommand> World { get; }

    /// <summary>The overlay commands, back to front.</summary>
    public IReadOnlyList<DrawCommand> Overlay { get; }

    /// <summary>The frame number (increases by one per built frame).</summary>
    public long Number { get; }
}
