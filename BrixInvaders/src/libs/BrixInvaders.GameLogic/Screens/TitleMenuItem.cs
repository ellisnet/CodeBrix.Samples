namespace BrixInvaders.GameLogic;

/// <summary>The title menu, top to bottom.</summary>
public enum TitleMenuItem
{
    /// <summary>Start a game (ship select).</summary>
    Play = 0,

    /// <summary>High-score tables.</summary>
    HighScores = 1,

    /// <summary>Settings.</summary>
    Settings = 2,

    /// <summary>Credits.</summary>
    Credits = 3,

    /// <summary>Leave the game (the machine raises <see cref="ScreenCommandKind.QuitGame"/>; the host closes the app).</summary>
    Quit = 4,
}
