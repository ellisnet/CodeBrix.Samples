using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Input.Actions;

namespace BrixInvaders.Game.Settings;

/// <summary>
/// The persisted preferences and records the game session reads and writes. <see cref="StoredGameSettings"/> keeps
/// them in the AppSettings store through <see cref="SettingsService"/>; tests use an in-memory implementation.
/// </summary>
public interface IGameSettings
{
    /// <summary>The chosen ship shape, 0..2.</summary>
    int ShipShape { get; set; }

    /// <summary>The chosen ship colour, 0..3.</summary>
    int ShipColour { get; set; }

    /// <summary>The default difficulty.</summary>
    Difficulty Difficulty { get; set; }

    /// <summary>Master volume, 0..1.</summary>
    double MasterVolume { get; set; }

    /// <summary>Music volume, 0..1.</summary>
    double MusicVolume { get; set; }

    /// <summary>Effects volume, 0..1.</summary>
    double EffectsVolume { get; set; }

    /// <summary>The music generator name.</summary>
    string MusicGenerator { get; set; }

    /// <summary>The instrument library name.</summary>
    string InstrumentLibrary { get; set; }

    /// <summary>The gamepad binding profile.</summary>
    GamepadProfile GamepadProfile { get; set; }

    /// <summary>The device that produced the last input.</summary>
    InputDeviceKind LastInputDevice { get; set; }

    /// <summary>The last name entered on a high-score table.</summary>
    string LastName { get; set; }

    /// <summary>The highest sector cleared on a difficulty (0 = none).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>The sector.</returns>
    int GetHighestSectorCleared(Difficulty difficulty);

    /// <summary>Records a cleared sector (the stored value only grows).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="sector">The sector cleared.</param>
    void RecordSectorCleared(Difficulty difficulty, int sector);

    /// <summary>Loads every high-score table.</summary>
    /// <returns>The tables.</returns>
    HighScoreTable LoadHighScores();

    /// <summary>Saves one difficulty's table.</summary>
    /// <param name="table">The tables.</param>
    /// <param name="difficulty">The difficulty to save.</param>
    void SaveHighScores(HighScoreTable table, Difficulty difficulty);

    /// <summary>Clears every stored high-score table.</summary>
    void ResetHighScores();
}
