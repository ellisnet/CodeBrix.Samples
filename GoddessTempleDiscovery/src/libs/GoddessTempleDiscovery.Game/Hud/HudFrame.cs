namespace GoddessTempleDiscovery.Game.Hud;

/// <summary>What the host hands the HUD painter for one frame besides the table and the session.</summary>
public sealed class HudFrame
{
    /// <summary>The wire-service ticker line.</summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>The prompt for the human player (or why a click did nothing).</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>The card preparation's progress, 0 to 1, or a negative number when nothing is being prepared.</summary>
    public double Preparation { get; set; } = -1;

    /// <summary>True while it is a human team's turn and the table takes its clicks.</summary>
    public bool IsHumanTurn { get; set; }

    /// <summary>The pointer's table X.</summary>
    public float PointerX { get; set; } = -1;

    /// <summary>The pointer's table Y.</summary>
    public float PointerY { get; set; } = -1;

    /// <summary>True when the game is over (the HUD shows the final edition line).</summary>
    public bool IsGameOver { get; set; }
}
