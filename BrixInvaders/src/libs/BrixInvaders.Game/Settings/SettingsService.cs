using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;
using CodeBrix.Platform.AppSettings;
using CodeBrix.Platform.GameEngine.Input.Actions;

namespace BrixInvaders.Game.Settings;

/// <summary>
/// The game's settings facade over the AppSettings add-in: every persisted value (preferences, volumes, the music
/// choices, high scores, sector unlocks) goes through here, under keys namespaced <c>BrixInvaders.*</c>.
/// </summary>
/// <remarks>
/// <para>
/// Defaults (DESIGN.md section 14): ship shape 0, ship colour 0, difficulty Pilot, master volume 0.8, music volume
/// 0.6, effects volume 0.8, music generator SkyTNT, instrument library ModestSynthGm, gamepad profile Classic,
/// highest sector cleared 0 on every difficulty, last input device Keyboard, last name "AAA", every high-score table
/// empty.
/// </para>
/// <para>
/// The store is process-global: call <see cref="Initialize()"/> once at start-up (tests pass their own folder and
/// call <see cref="Shutdown"/>). The typed properties are created by Initialize, so read them only after it.
/// </para>
/// </remarks>
public static class SettingsService
{
    /// <summary>The application name the store is registered under (its folder name).</summary>
    public const string AppName = "BrixInvaders";

    /// <summary>The key prefix of every value the game stores.</summary>
    public const string KeyPrefix = "BrixInvaders.";

    /// <summary>Key: the chosen ship shape, 0..2.</summary>
    public const string ShipShapeKey = KeyPrefix + "ShipShape";

    /// <summary>Key: the chosen ship colour, 0..3.</summary>
    public const string ShipColourKey = KeyPrefix + "ShipColour";

    /// <summary>Key: the default difficulty name.</summary>
    public const string DifficultyKey = KeyPrefix + "Difficulty";

    /// <summary>Key: the master volume, 0..1.</summary>
    public const string MasterVolumeKey = KeyPrefix + "MasterVolume";

    /// <summary>Key: the music volume, 0..1.</summary>
    public const string MusicVolumeKey = KeyPrefix + "MusicVolume";

    /// <summary>Key: the effects volume, 0..1.</summary>
    public const string EffectsVolumeKey = KeyPrefix + "EffectsVolume";

    /// <summary>Key: the music generator name (SkyTNT or MuPT).</summary>
    public const string MusicGeneratorKey = KeyPrefix + "MusicGenerator";

    /// <summary>Key: the instrument library name (ModestSynthGm or FluidR3Gm).</summary>
    public const string InstrumentLibraryKey = KeyPrefix + "InstrumentLibrary";

    /// <summary>Key: the gamepad binding profile name.</summary>
    public const string GamepadProfileKey = KeyPrefix + "GamepadProfile";

    /// <summary>Key: the device that produced the last input (Keyboard or Gamepad).</summary>
    public const string LastInputDeviceKey = KeyPrefix + "LastInputDevice";

    private const string KeyboardDevice = "Keyboard";
    private const string GamepadDevice = "Gamepad";

    /// <summary>Key: the last name entered on a high-score table.</summary>
    public const string LastNameKey = KeyPrefix + "LastName";

    /// <summary>Key prefix of the per-difficulty high-score tables (<c>BrixInvaders.HighScores.Pilot</c>).</summary>
    public const string HighScoresKeyPrefix = KeyPrefix + "HighScores.";

    /// <summary>Key prefix of the per-difficulty highest sector cleared (<c>BrixInvaders.HighestSectorCleared.Ace</c>).</summary>
    public const string HighestSectorClearedKeyPrefix = KeyPrefix + "HighestSectorCleared.";

    /// <summary>Default master volume.</summary>
    public const double DefaultMasterVolume = 0.8;

    /// <summary>Default music volume.</summary>
    public const double DefaultMusicVolume = 0.6;

    /// <summary>Default effects volume.</summary>
    public const double DefaultEffectsVolume = 0.8;

    /// <summary>Default last name.</summary>
    public const string DefaultLastName = "AAA";

    private static AppSettingProperty<int> _shipShape;
    private static AppSettingProperty<int> _shipColour;
    private static AppSettingProperty<string> _difficulty;
    private static AppSettingProperty<double> _masterVolume;
    private static AppSettingProperty<double> _musicVolume;
    private static AppSettingProperty<double> _effectsVolume;
    private static AppSettingProperty<string> _musicGenerator;
    private static AppSettingProperty<string> _instrumentLibrary;
    private static AppSettingProperty<string> _gamepadProfile;
    private static AppSettingProperty<string> _lastInputDevice;
    private static AppSettingProperty<string> _lastName;

    /// <summary>Whether the store is open.</summary>
    public static bool IsInitialized => AppSettingsService.IsInitialized;

    /// <summary>The folder holding the store (valid after <see cref="Initialize()"/>).</summary>
    public static string DirectoryPath => AppSettingsService.DirectoryPath;

    /// <summary>The folder the store uses when no folder is given.</summary>
    public static string DefaultDirectory => AppSettingsService.GetDefaultDirectory(AppName);

    /// <summary>The chosen ship shape (0..2; default 0).</summary>
    public static int ShipShape
    {
        get => Math.Clamp(Require(_shipShape).Value, 0, GameSetup.ShipShapeCount - 1);
        set => Require(_shipShape).Set(Math.Clamp(value, 0, GameSetup.ShipShapeCount - 1));
    }

    /// <summary>The chosen ship colour (0..3; default 0).</summary>
    public static int ShipColour
    {
        get => Math.Clamp(Require(_shipColour).Value, 0, GameSetup.ShipColourCount - 1);
        set => Require(_shipColour).Set(Math.Clamp(value, 0, GameSetup.ShipColourCount - 1));
    }

    /// <summary>The default difficulty (default Pilot).</summary>
    public static Difficulty Difficulty
    {
        get => Enum.TryParse(Require(_difficulty).Value, true, out Difficulty level) && Enum.IsDefined(level)
            ? level
            : Difficulty.Pilot;
        set => Require(_difficulty).Set(value.ToString());
    }

    /// <summary>The master volume (0..1; default 0.8).</summary>
    public static double MasterVolume
    {
        get => ClampVolume(Require(_masterVolume).Value);
        set => Require(_masterVolume).Set(RoundVolume(value));
    }

    /// <summary>The music volume (0..1; default 0.6).</summary>
    public static double MusicVolume
    {
        get => ClampVolume(Require(_musicVolume).Value);
        set => Require(_musicVolume).Set(RoundVolume(value));
    }

    /// <summary>The effects volume (0..1; default 0.8).</summary>
    public static double EffectsVolume
    {
        get => ClampVolume(Require(_effectsVolume).Value);
        set => Require(_effectsVolume).Set(RoundVolume(value));
    }

    /// <summary>The music generator name (default SkyTNT); an unknown stored name reads as the default.</summary>
    public static string MusicGenerator
    {
        get => MusicChoices.IsGenerator(Require(_musicGenerator).Value)
            ? MusicChoices.ResolveGenerator(_musicGenerator.Value)
            : MusicChoices.DefaultGenerator;
        set => Require(_musicGenerator).Set(MusicChoices.ResolveGenerator(value));
    }

    /// <summary>The instrument library name (default ModestSynthGm); an unknown stored name reads as the default.</summary>
    public static string InstrumentLibrary
    {
        get => MusicChoices.IsInstrumentLibrary(Require(_instrumentLibrary).Value)
            ? MusicChoices.ResolveInstrumentLibrary(_instrumentLibrary.Value)
            : MusicChoices.DefaultInstrumentLibrary;
        set => Require(_instrumentLibrary).Set(MusicChoices.ResolveInstrumentLibrary(value));
    }

    /// <summary>The gamepad binding profile (default Classic).</summary>
    public static GamepadProfile GamepadProfile
    {
        get => Enum.TryParse(Require(_gamepadProfile).Value, true, out GamepadProfile profile) && Enum.IsDefined(profile)
            ? profile
            : GamepadProfile.Classic;
        set => Require(_gamepadProfile).Set(value.ToString());
    }

    /// <summary>The device that produced the last input (default Keyboard), stored as "Keyboard" or "Gamepad".</summary>
    public static InputDeviceKind LastInputDevice
    {
        get => string.Equals(Require(_lastInputDevice).Value, GamepadDevice, StringComparison.OrdinalIgnoreCase)
            ? InputDeviceKind.Gamepad
            : InputDeviceKind.KeyboardMouse;
        set => Require(_lastInputDevice).Set(value == InputDeviceKind.Gamepad ? GamepadDevice : KeyboardDevice);
    }

    /// <summary>The last name entered on a high-score table (default "AAA").</summary>
    public static string LastName
    {
        get => NameEntry.Normalize(Require(_lastName).Value);
        set => Require(_lastName).Set(NameEntry.Normalize(value));
    }

    /// <summary>Opens the store in the default per-user folder. Call once at start-up.</summary>
    public static void Initialize()
    {
        AppSettingsService.Initialize(AppName);
        CreateProperties();
    }

    /// <summary>Opens the store in <paramref name="directoryPath"/> (test hosts, portable installs).</summary>
    /// <param name="directoryPath">The folder to keep the store in.</param>
    public static void Initialize(string directoryPath)
    {
        AppSettingsService.Initialize(AppName, directoryPath);
        CreateProperties();
    }

    /// <summary>Closes the store and permits a later Initialize (test hosts). Safe when never initialized.</summary>
    public static void Shutdown()
    {
        AppSettingsService.Shutdown();
        _shipShape = null;
        _shipColour = null;
        _difficulty = null;
        _masterVolume = null;
        _musicVolume = null;
        _effectsVolume = null;
        _musicGenerator = null;
        _instrumentLibrary = null;
        _gamepadProfile = null;
        _lastInputDevice = null;
        _lastName = null;
    }

    /// <summary>The music choices as the Music library's settings object (the music volume stays at 1: the
    /// player's music level is applied once, by the music director).</summary>
    /// <returns>A new settings object.</returns>
    public static MusicSettings CreateMusicSettings() => new MusicSettings
    {
        GeneratorName = MusicGenerator,
        InstrumentLibraryName = InstrumentLibrary,
    };

    /// <summary>The highest sector cleared on a difficulty (default 0).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>The sector number, 0 when none.</returns>
    public static int GetHighestSectorCleared(Difficulty difficulty)
    {
        RequireStore();
        return Math.Max(0, AppSettingsService.Get(HighestSectorClearedKeyPrefix + difficulty, 0));
    }

    /// <summary>Records a cleared sector; the stored value only ever grows.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="sector">The sector just cleared.</param>
    public static void RecordSectorCleared(Difficulty difficulty, int sector)
    {
        if (sector > GetHighestSectorCleared(difficulty))
        {
            AppSettingsService.Set(HighestSectorClearedKeyPrefix + difficulty, sector);
        }
    }

    /// <summary>The start sector unlocked on a difficulty: min(highest cleared + 1, 5).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>The highest start sector the player may choose.</returns>
    public static int UnlockedStartSector(Difficulty difficulty) =>
        Math.Min(GetHighestSectorCleared(difficulty) + 1, ScreenStateMachine.MaxUnlockableSector);

    /// <summary>Loads every difficulty's high-score table (malformed stored data reads as an empty table).</summary>
    /// <returns>The tables.</returns>
    public static HighScoreTable LoadHighScores()
    {
        RequireStore();
        var table = new HighScoreTable();
        foreach (var difficulty in DifficultyTable.Levels)
        {
            foreach (var record in FromJson(AppSettingsService.Get(HighScoresKeyPrefix + difficulty, "[]")))
            {
                table.Insert(difficulty, record.Name, record.Score, record.Sector);
            }
        }

        return table;
    }

    /// <summary>Saves one difficulty's table as a JSON array of <see cref="HighScoreRecord"/>.</summary>
    /// <param name="table">The tables.</param>
    /// <param name="difficulty">The difficulty to save.</param>
    /// <returns>The JSON written.</returns>
    public static string SaveHighScores(HighScoreTable table, Difficulty difficulty)
    {
        ArgumentNullException.ThrowIfNull(table);
        RequireStore();
        var json = ToJson(table.EntriesFor(difficulty));
        AppSettingsService.Set(HighScoresKeyPrefix + difficulty, json);
        return json;
    }

    /// <summary>Saves every difficulty's table.</summary>
    /// <param name="table">The tables.</param>
    public static void SaveAllHighScores(HighScoreTable table)
    {
        foreach (var difficulty in DifficultyTable.Levels)
        {
            SaveHighScores(table, difficulty);
        }
    }

    /// <summary>Clears every high-score table (the settings screen's "Reset high scores").</summary>
    public static void ResetHighScores()
    {
        RequireStore();
        foreach (var difficulty in DifficultyTable.Levels)
        {
            AppSettingsService.Set(HighScoresKeyPrefix + difficulty, "[]");
        }
    }

    /// <summary>Serializes high-score entries to the stored JSON form.</summary>
    /// <param name="entries">The entries, best first.</param>
    /// <returns>A JSON array of <c>{ "Name", "Score", "Sector" }</c>.</returns>
    public static string ToJson(IEnumerable<HighScoreEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var records = entries
            .Select(entry => new HighScoreRecord { Name = entry.Name, Score = entry.Score, Sector = entry.Sector })
            .ToArray();
        return JsonSerializer.Serialize(records);
    }

    /// <summary>Parses the stored JSON form; anything malformed yields no records.</summary>
    /// <param name="json">The JSON.</param>
    /// <returns>The records that could be read.</returns>
    public static IReadOnlyList<HighScoreRecord> FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<HighScoreRecord>();
        }

        try
        {
            var records = JsonSerializer.Deserialize<HighScoreRecord[]>(json);
            return records == null
                ? Array.Empty<HighScoreRecord>()
                : records.Where(record => record != null && record.Score > 0).ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<HighScoreRecord>();
        }
    }

    private static void CreateProperties()
    {
        _shipShape = AppSettingsService.Wrap(ShipShapeKey, 0);
        _shipColour = AppSettingsService.Wrap(ShipColourKey, 0);
        _difficulty = AppSettingsService.Wrap(DifficultyKey, nameof(Difficulty.Pilot));
        _masterVolume = AppSettingsService.Wrap(MasterVolumeKey, DefaultMasterVolume);
        _musicVolume = AppSettingsService.Wrap(MusicVolumeKey, DefaultMusicVolume);
        _effectsVolume = AppSettingsService.Wrap(EffectsVolumeKey, DefaultEffectsVolume);
        _musicGenerator = AppSettingsService.Wrap(MusicGeneratorKey, MusicChoices.DefaultGenerator);
        _instrumentLibrary = AppSettingsService.Wrap(InstrumentLibraryKey, MusicChoices.DefaultInstrumentLibrary);
        _gamepadProfile = AppSettingsService.Wrap(GamepadProfileKey, nameof(GamepadProfile.Classic));
        _lastInputDevice = AppSettingsService.Wrap(LastInputDeviceKey, KeyboardDevice);
        _lastName = AppSettingsService.Wrap(LastNameKey, DefaultLastName);
    }

    private static T Require<T>(T property) where T : class
    {
        RequireStore();
        return property ?? throw new InvalidOperationException("SettingsService.Initialize has not run.");
    }

    private static void RequireStore()
    {
        if (!AppSettingsService.IsInitialized)
        {
            throw new InvalidOperationException("SettingsService.Initialize has not run.");
        }
    }

    private static double ClampVolume(double value) => double.IsNaN(value) ? 0 : Math.Clamp(value, 0.0, 1.0);

    private static double RoundVolume(double value) => Math.Round(ClampVolume(value), 2);
}
