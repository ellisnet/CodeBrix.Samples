namespace BrixInvaders.GameLogic;

/// <summary>
/// Fixed dimensions of the logical playfield and the fixed simulation step. Everything in GameLogic is in these
/// world units; the Game library pins the engine render resolution to it, and the engine letterboxes it onto the window.
/// </summary>
public static class Playfield
{
    /// <summary>Playfield width in world units.</summary>
    public const double Width = 1280.0;

    /// <summary>Playfield height in world units.</summary>
    public const double Height = 720.0;

    /// <summary>Height of the HUD band at the top of the playfield (drawn over the play area; nothing spawns in it).</summary>
    public const double HudHeight = 48.0;

    /// <summary>Horizontal margin that formations and the player never cross.</summary>
    public const double SideMargin = 24.0;

    /// <summary>The fixed simulation step, in seconds (60 steps per second).</summary>
    public const double FixedStep = 1.0 / 60.0;

    /// <summary>Horizontal centre of the playfield.</summary>
    public const double CenterX = Width / 2.0;

    /// <summary>The whole playfield as a box.</summary>
    public static Box Bounds => new Box(Width / 2.0, Height / 2.0, Width / 2.0, Height / 2.0);
}
