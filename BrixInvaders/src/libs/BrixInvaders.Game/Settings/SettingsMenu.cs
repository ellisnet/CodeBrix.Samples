using System;
using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;

namespace BrixInvaders.Game.Settings;

/// <summary>
/// The meaning of the settings screen's rows (DESIGN.md section 12): the screen machine only moves the cursor and
/// reports Left/Right/Confirm on a row; this class changes the stored value and says what changed.
/// </summary>
public sealed class SettingsMenu
{
    /// <summary>Row: master volume.</summary>
    public const int MasterVolumeRow = 0;

    /// <summary>Row: music volume.</summary>
    public const int MusicVolumeRow = 1;

    /// <summary>Row: effects volume.</summary>
    public const int EffectsVolumeRow = 2;

    /// <summary>Row: music model.</summary>
    public const int MusicModelRow = 3;

    /// <summary>Row: instrument library.</summary>
    public const int InstrumentLibraryRow = 4;

    /// <summary>Row: gamepad profile.</summary>
    public const int GamepadProfileRow = 5;

    /// <summary>Row: default ship.</summary>
    public const int DefaultShipRow = 6;

    /// <summary>Row: default difficulty.</summary>
    public const int DefaultDifficultyRow = 7;

    /// <summary>Row: reset high scores.</summary>
    public const int ResetHighScoresRow = 8;

    /// <summary>The number of rows.</summary>
    public const int RowCount = 9;

    /// <summary>One volume step.</summary>
    public const double VolumeStep = 0.1;

    private static readonly string[] Labels =
    {
        "Master volume", "Music volume", "Effects volume", "Music model", "Instruments", "Gamepad buttons",
        "Default ship", "Default difficulty", "Reset high scores",
    };

    private readonly IGameSettings _settings;

    /// <summary>Creates the menu over the stored settings.</summary>
    /// <param name="settings">The settings.</param>
    public SettingsMenu(IGameSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>Whether "Reset high scores" is armed (the next confirm on that row clears the tables).</summary>
    public bool ResetArmed { get; private set; }

    /// <summary>Whether the tables were cleared since the settings screen opened.</summary>
    public bool ResetDone { get; private set; }

    /// <summary>The label of a row.</summary>
    /// <param name="row">The row.</param>
    /// <returns>The label.</returns>
    public static string LabelOf(int row) => Labels[Math.Clamp(row, 0, RowCount - 1)];

    /// <summary>The name of a ship choice ("Ship 2 - orange").</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="colour">Ship colour, 0..3.</param>
    /// <returns>The name.</returns>
    public static string ShipName(int shape, int colour) =>
        $"Ship {shape + 1} - {Assets.AssetKeys.Ships.Colours[Math.Clamp(colour, 0, 3)]}";

    /// <summary>The binding summary of a gamepad profile.</summary>
    /// <param name="profile">The profile.</param>
    /// <returns>A short description.</returns>
    public static string ProfileName(GamepadProfile profile) => profile == GamepadProfile.Shoulder
        ? "Shoulder - RB fire, LB bomb"
        : "Classic - A fire, B bomb";

    /// <summary>The value shown on a row.</summary>
    /// <param name="row">The row.</param>
    /// <returns>The value text.</returns>
    public string ValueOf(int row) => row switch
    {
        MasterVolumeRow => Percent(_settings.MasterVolume),
        MusicVolumeRow => Percent(_settings.MusicVolume),
        EffectsVolumeRow => Percent(_settings.EffectsVolume),
        MusicModelRow => MusicChoices.GeneratorChoice(_settings.MusicGenerator).Label,
        InstrumentLibraryRow => MusicChoices.InstrumentLibraryChoice(_settings.InstrumentLibrary).Label,
        GamepadProfileRow => ProfileName(_settings.GamepadProfile),
        DefaultShipRow => ShipName(_settings.ShipShape, _settings.ShipColour),
        DefaultDifficultyRow => _settings.Difficulty.ToString(),
        ResetHighScoresRow => ResetDone ? "Cleared" : ResetArmed ? "Confirm again to clear" : "Confirm to clear",
        _ => string.Empty,
    };

    /// <summary>Called when the settings screen opens: disarms the reset.</summary>
    public void Open()
    {
        ResetArmed = false;
        ResetDone = false;
    }

    /// <summary>Left/Right on a row.</summary>
    /// <param name="row">The row.</param>
    /// <param name="delta">-1 or +1.</param>
    /// <returns>What changed.</returns>
    public SettingsChange Adjust(int row, int delta)
    {
        var step = Math.Sign(delta);
        if (step == 0)
        {
            return SettingsChange.None;
        }

        ResetArmed = false;
        switch (row)
        {
            case MasterVolumeRow:
                _settings.MasterVolume = StepVolume(_settings.MasterVolume, step);
                return SettingsChange.Volumes;
            case MusicVolumeRow:
                _settings.MusicVolume = StepVolume(_settings.MusicVolume, step);
                return SettingsChange.Volumes;
            case EffectsVolumeRow:
                _settings.EffectsVolume = StepVolume(_settings.EffectsVolume, step);
                return SettingsChange.Volumes;
            case MusicModelRow:
                _settings.MusicGenerator = MusicChoices.NextGenerator(_settings.MusicGenerator, step);
                return SettingsChange.MusicChoice;
            case InstrumentLibraryRow:
                _settings.InstrumentLibrary = MusicChoices.NextInstrumentLibrary(_settings.InstrumentLibrary, step);
                return SettingsChange.MusicChoice;
            case GamepadProfileRow:
                _settings.GamepadProfile = _settings.GamepadProfile == GamepadProfile.Classic
                    ? GamepadProfile.Shoulder
                    : GamepadProfile.Classic;
                return SettingsChange.GamepadProfile;
            case DefaultShipRow:
                var count = GameSetup.ShipShapeCount * GameSetup.ShipColourCount;
                var index = (_settings.ShipShape * GameSetup.ShipColourCount) + _settings.ShipColour;
                index = (((index + step) % count) + count) % count;
                _settings.ShipShape = index / GameSetup.ShipColourCount;
                _settings.ShipColour = index % GameSetup.ShipColourCount;
                return SettingsChange.Defaults;
            case DefaultDifficultyRow:
                _settings.Difficulty = (Difficulty)((((int)_settings.Difficulty + step) % 4 + 4) % 4);
                return SettingsChange.Defaults;
            default:
                return SettingsChange.None;
        }
    }

    /// <summary>Confirm on a row. Only "Reset high scores" reacts: the first confirm arms it, the second clears.</summary>
    /// <param name="row">The row.</param>
    /// <returns>What changed.</returns>
    public SettingsChange Activate(int row)
    {
        if (row != ResetHighScoresRow)
        {
            ResetArmed = false;
            return SettingsChange.None;
        }

        if (!ResetArmed)
        {
            ResetArmed = true;
            ResetDone = false;
            return SettingsChange.ResetArmed;
        }

        ResetArmed = false;
        ResetDone = true;
        _settings.ResetHighScores();
        return SettingsChange.HighScoresReset;
    }

    private static double StepVolume(double value, int step) =>
        Math.Round(Math.Clamp(value + (step * VolumeStep), 0.0, 1.0), 2);

    private static string Percent(double value) => $"{Math.Round(value * 100):0}%";
}
