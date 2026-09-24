namespace BrixInvaders.GameLogic;

/// <summary>The game's screens.</summary>
public enum GameScreen
{
    /// <summary>The start-up splash overlay.</summary>
    Splash = 0,

    /// <summary>The title screen with its menu.</summary>
    Title = 1,

    /// <summary>The attract-mode demo (the AI plays a wave).</summary>
    Attract = 2,

    /// <summary>Ship shape and colour selection.</summary>
    ShipSelect = 3,

    /// <summary>Difficulty (and start sector) selection.</summary>
    DifficultySelect = 4,

    /// <summary>The sector briefing / loading screen with the Kenney card.</summary>
    SectorBriefing = 5,

    /// <summary>In play.</summary>
    Playing = 6,

    /// <summary>The pause overlay over the frozen game.</summary>
    Paused = 7,

    /// <summary>Sector clear summary.</summary>
    SectorClear = 8,

    /// <summary>Game over.</summary>
    GameOver = 9,

    /// <summary>Three-letter high-score name entry.</summary>
    HighScoreEntry = 10,

    /// <summary>The high-score tables.</summary>
    HighScores = 11,

    /// <summary>Settings.</summary>
    Settings = 12,

    /// <summary>Credits.</summary>
    Credits = 13,
}
