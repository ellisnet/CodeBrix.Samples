namespace BrixInvaders.Game.Settings;

/// <summary>What a settings-screen action changed, so the session knows what to re-apply.</summary>
public enum SettingsChange
{
    /// <summary>Nothing changed.</summary>
    None = 0,

    /// <summary>A volume changed.</summary>
    Volumes = 1,

    /// <summary>The music model or instrument library changed.</summary>
    MusicChoice = 2,

    /// <summary>The gamepad profile changed.</summary>
    GamepadProfile = 3,

    /// <summary>The default ship or difficulty changed.</summary>
    Defaults = 4,

    /// <summary>"Reset high scores" is armed: the next confirm clears them.</summary>
    ResetArmed = 5,

    /// <summary>The high-score tables were cleared.</summary>
    HighScoresReset = 6,
}
