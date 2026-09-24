namespace BrixInvaders.Game.Rendering;

/// <summary>The game's colours (ARGB), in one place so the screens stay consistent.</summary>
public static class Palette
{
    /// <summary>Body text.</summary>
    public const uint Text = 0xFFF1F5FF;

    /// <summary>Secondary text.</summary>
    public const uint Dim = 0xFF93A4C3;

    /// <summary>Headings and the selected menu item.</summary>
    public const uint Accent = 0xFF5CE1FF;

    /// <summary>Scores and rewards.</summary>
    public const uint Gold = 0xFFFFD35C;

    /// <summary>Warnings and damage.</summary>
    public const uint Danger = 0xFFFF5C5C;

    /// <summary>Good news (sector clear, extra life).</summary>
    public const uint Good = 0xFF7CFF8B;

    /// <summary>Panel fill behind menus and cards.</summary>
    public const uint Panel = 0xD8081226;

    /// <summary>Panel outline.</summary>
    public const uint PanelEdge = 0xFF2E5A8C;

    /// <summary>A dimming veil over the playfield (pause, game over).</summary>
    public const uint Veil = 0xB0000000;

    /// <summary>The letterbox and clear colour.</summary>
    public const uint Space = 0xFF02040C;
}
