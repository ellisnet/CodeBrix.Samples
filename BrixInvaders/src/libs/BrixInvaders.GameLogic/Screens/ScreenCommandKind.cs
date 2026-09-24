namespace BrixInvaders.GameLogic;

/// <summary>What the screen state machine asks the Game library to do.</summary>
public enum ScreenCommandKind
{
    /// <summary>The screen changed. Value = (int) new <see cref="GameScreen"/>.</summary>
    ScreenChanged,

    /// <summary>Create a new <see cref="GameSimulation"/> (difficulty, ship and start sector from the machine). Value = start sector.</summary>
    StartNewGame,

    /// <summary>Start playing a sector (the first one, or <see cref="GameSimulation.BeginNextSector"/>). Value = sector.</summary>
    BeginSector,

    /// <summary>Stop stepping the simulation (pause overlay shown).</summary>
    PauseGame,

    /// <summary>Resume stepping the simulation.</summary>
    ResumeGame,

    /// <summary>Throw the running game away (quit to title from pause).</summary>
    AbandonGame,

    /// <summary>Store the high score. Text = the three-letter name; the Game supplies score, sector, difficulty.</summary>
    SubmitHighScore,

    /// <summary>Open https://kenney.itch.io/kenney-game-assets in the browser.</summary>
    OpenKenneyLink,

    /// <summary>Change a setting. Value = settings item index, Delta = -1 or +1.</summary>
    AdjustSetting,

    /// <summary>Activate a setting (e.g. "reset high scores"). Value = settings item index.</summary>
    ActivateSetting,

    /// <summary>Start the attract-mode demo simulation.</summary>
    StartAttract,

    /// <summary>Stop the attract-mode demo simulation.</summary>
    StopAttract,

    /// <summary>The player chose Quit on the title: close the application. The screen does not change.</summary>
    QuitGame,
}
