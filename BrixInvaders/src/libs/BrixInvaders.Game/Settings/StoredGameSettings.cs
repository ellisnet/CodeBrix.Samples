using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Settings;

/// <summary>The <see cref="IGameSettings"/> the game runs with: everything goes through <see cref="SettingsService"/>.</summary>
public sealed class StoredGameSettings : IGameSettings
{
    /// <inheritdoc />
    public int ShipShape
    {
        get => SettingsService.ShipShape;
        set => SettingsService.ShipShape = value;
    }

    /// <inheritdoc />
    public int ShipColour
    {
        get => SettingsService.ShipColour;
        set => SettingsService.ShipColour = value;
    }

    /// <inheritdoc />
    public Difficulty Difficulty
    {
        get => SettingsService.Difficulty;
        set => SettingsService.Difficulty = value;
    }

    /// <inheritdoc />
    public double MasterVolume
    {
        get => SettingsService.MasterVolume;
        set => SettingsService.MasterVolume = value;
    }

    /// <inheritdoc />
    public double MusicVolume
    {
        get => SettingsService.MusicVolume;
        set => SettingsService.MusicVolume = value;
    }

    /// <inheritdoc />
    public double EffectsVolume
    {
        get => SettingsService.EffectsVolume;
        set => SettingsService.EffectsVolume = value;
    }

    /// <inheritdoc />
    public string MusicGenerator
    {
        get => SettingsService.MusicGenerator;
        set => SettingsService.MusicGenerator = value;
    }

    /// <inheritdoc />
    public string InstrumentLibrary
    {
        get => SettingsService.InstrumentLibrary;
        set => SettingsService.InstrumentLibrary = value;
    }

    /// <inheritdoc />
    public GamepadProfile GamepadProfile
    {
        get => SettingsService.GamepadProfile;
        set => SettingsService.GamepadProfile = value;
    }

    /// <inheritdoc />
    public InputDevice LastInputDevice
    {
        get => SettingsService.LastInputDevice;
        set => SettingsService.LastInputDevice = value;
    }

    /// <inheritdoc />
    public string LastName
    {
        get => SettingsService.LastName;
        set => SettingsService.LastName = value;
    }

    /// <inheritdoc />
    public int GetHighestSectorCleared(Difficulty difficulty) => SettingsService.GetHighestSectorCleared(difficulty);

    /// <inheritdoc />
    public void RecordSectorCleared(Difficulty difficulty, int sector) => SettingsService.RecordSectorCleared(difficulty, sector);

    /// <inheritdoc />
    public HighScoreTable LoadHighScores() => SettingsService.LoadHighScores();

    /// <inheritdoc />
    public void SaveHighScores(HighScoreTable table, Difficulty difficulty) => SettingsService.SaveHighScores(table, difficulty);

    /// <inheritdoc />
    public void ResetHighScores() => SettingsService.ResetHighScores();
}
