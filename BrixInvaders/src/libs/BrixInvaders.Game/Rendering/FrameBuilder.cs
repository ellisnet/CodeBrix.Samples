using System.Collections.Generic;

namespace BrixInvaders.Game.Rendering;

/// <summary>Collects one frame's draw commands into the world and overlay lists, then builds a <see cref="RenderFrame"/>.</summary>
public sealed class FrameBuilder
{
    private readonly List<DrawCommand> _world = new List<DrawCommand>(1024);
    private readonly List<DrawCommand> _overlay = new List<DrawCommand>(256);
    private readonly List<Hotspot> _hotspots = new List<Hotspot>();
    private long _number;

    /// <summary>The world commands added so far.</summary>
    public IReadOnlyList<DrawCommand> World => _world;

    /// <summary>The overlay commands added so far.</summary>
    public IReadOnlyList<DrawCommand> Overlay => _overlay;

    /// <summary>Adds a world command (drawn under particles and the boss bar).</summary>
    /// <param name="command">The command.</param>
    public void AddWorld(DrawCommand command) => _world.Add(command);

    /// <summary>Adds an overlay command (HUD and menus, drawn over everything).</summary>
    /// <param name="command">The command.</param>
    public void AddOverlay(DrawCommand command) => _overlay.Add(command);

    /// <summary>The clickable links added so far.</summary>
    public IReadOnlyList<Hotspot> Hotspots => _hotspots;

    /// <summary>Adds a clickable link.</summary>
    /// <param name="hotspot">The hotspot.</param>
    public void AddHotspot(Hotspot hotspot) => _hotspots.Add(hotspot);

    /// <summary>Starts a new frame.</summary>
    public void Clear()
    {
        _world.Clear();
        _overlay.Clear();
        _hotspots.Clear();
    }

    /// <summary>Copies the collected commands into an immutable frame.</summary>
    /// <returns>The frame.</returns>
    public RenderFrame Build() => new RenderFrame(_world.ToArray(), _overlay.ToArray(), _hotspots.ToArray(), ++_number);
}
