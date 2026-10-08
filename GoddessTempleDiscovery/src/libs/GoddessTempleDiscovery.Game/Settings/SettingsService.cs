using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CodeBrix.Platform.AppSettings;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Settings;

/// <summary>
/// The game's settings facade over the AppSettings add-in: every persisted value (preferences and the last setup)
/// goes through here, under keys namespaced <c>GoddessTempleDiscovery.*</c>.
/// </summary>
/// <remarks>
/// <para>
/// Defaults: sound on, reduced motion off, animation speed 1.0, reveal computer discoveries on, two turns per
/// season, Standard difficulty, no last setup.
/// </para>
/// <para>
/// The store is process-global: call <see cref="Initialize()"/> once at start-up (tests pass their own folder and
/// call <see cref="Shutdown"/>). The typed properties are created by Initialize, so read them only after it.
/// </para>
/// </remarks>
public static class SettingsService
{
    /// <summary>The application name the store is registered under (its folder name).</summary>
    public const string AppName = "GoddessTempleDiscovery";

    /// <summary>The key prefix of every value the game stores.</summary>
    public const string KeyPrefix = "GoddessTempleDiscovery.";

    /// <summary>Key: whether the card and dice sounds play.</summary>
    public const string SoundEnabledKey = KeyPrefix + "SoundEnabled";

    /// <summary>Key: whether animations complete at once.</summary>
    public const string ReducedMotionKey = KeyPrefix + "ReducedMotion";

    /// <summary>Key: the animation speed multiplier.</summary>
    public const string AnimationSpeedKey = KeyPrefix + "AnimationSpeed";

    /// <summary>Key: whether the inspector opens for the computer teams' discoveries.</summary>
    public const string RevealComputerDiscoveriesKey = KeyPrefix + "RevealComputerDiscoveries";

    /// <summary>Key: the default turns per team per season.</summary>
    public const string TurnsPerSeasonKey = KeyPrefix + "TurnsPerSeason";

    /// <summary>Key: the default difficulty's name.</summary>
    public const string DifficultyKey = KeyPrefix + "Difficulty";

    /// <summary>Key: the seats of the last setup, as a JSON array of <see cref="SeatRecord"/>.</summary>
    public const string LastSeatsKey = KeyPrefix + "LastSeats";

    /// <summary>The slowest animation speed.</summary>
    public const double MinAnimationSpeed = 0.25;

    /// <summary>The fastest animation speed.</summary>
    public const double MaxAnimationSpeed = 4.0;

    /// <summary>The default animation speed.</summary>
    public const double DefaultAnimationSpeed = 1.0;

    /// <summary>The default turns per season.</summary>
    public const int DefaultTurnsPerSeason = 2;

    private static AppSettingProperty<bool> _soundEnabled;
    private static AppSettingProperty<bool> _reducedMotion;
    private static AppSettingProperty<double> _animationSpeed;
    private static AppSettingProperty<bool> _revealComputerDiscoveries;
    private static AppSettingProperty<int> _turnsPerSeason;
    private static AppSettingProperty<string> _difficulty;
    private static AppSettingProperty<string> _lastSeats;

    /// <summary>Raised after any value is changed through this facade (on the thread that changed it).</summary>
    public static event Action Changed;

    /// <summary>Whether the store is open.</summary>
    public static bool IsInitialized => AppSettingsService.IsInitialized;

    /// <summary>The folder holding the store (valid after <see cref="Initialize()"/>).</summary>
    public static string DirectoryPath => AppSettingsService.DirectoryPath;

    /// <summary>The folder the store uses when no folder is given.</summary>
    public static string DefaultDirectory => AppSettingsService.GetDefaultDirectory(AppName);

    /// <summary>Whether the card and dice sounds play (default true).</summary>
    public static bool SoundEnabled
    {
        get => Require(_soundEnabled).Value;
        set => Store(_soundEnabled, value);
    }

    /// <summary>Whether animations complete at once (default false).</summary>
    public static bool ReducedMotion
    {
        get => Require(_reducedMotion).Value;
        set => Store(_reducedMotion, value);
    }

    /// <summary>The animation speed multiplier (0.25 to 4; default 1.0).</summary>
    public static double AnimationSpeed
    {
        get => ClampSpeed(Require(_animationSpeed).Value);
        set => Store(_animationSpeed, Math.Round(ClampSpeed(value), 2));
    }

    /// <summary>Whether the inspector opens for the computer teams' discoveries (default true).</summary>
    public static bool RevealComputerDiscoveries
    {
        get => Require(_revealComputerDiscoveries).Value;
        set => Store(_revealComputerDiscoveries, value);
    }

    /// <summary>The default turns per team per season (1 to 3; default 2).</summary>
    public static int TurnsPerSeason
    {
        get => Math.Clamp(Require(_turnsPerSeason).Value, GameRules.MinTurnsPerSeason, GameRules.MaxTurnsPerSeason);
        set => Store(_turnsPerSeason, Math.Clamp(value, GameRules.MinTurnsPerSeason, GameRules.MaxTurnsPerSeason));
    }

    /// <summary>The default difficulty (default Standard).</summary>
    public static Difficulty Difficulty
    {
        get => Enum.TryParse(Require(_difficulty).Value, true, out Difficulty level) && Enum.IsDefined(level)
            ? level
            : Difficulty.Standard;
        set => Store(_difficulty, value.ToString());
    }

    /// <summary>The stored JSON of the last setup's seats (empty when none).</summary>
    public static string LastSeatsJson
    {
        get => Require(_lastSeats).Value ?? string.Empty;
        set => Store(_lastSeats, value ?? string.Empty);
    }

    /// <summary>Opens the store in the per-user default folder.</summary>
    public static void Initialize()
    {
        AppSettingsService.Initialize(AppName);
        CreateProperties();
    }

    /// <summary>Opens the store in <paramref name="directoryPath"/> (test hosts, autoplay).</summary>
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
        _soundEnabled = null;
        _reducedMotion = null;
        _animationSpeed = null;
        _revealComputerDiscoveries = null;
        _turnsPerSeason = null;
        _difficulty = null;
        _lastSeats = null;
    }

    /// <summary>The seats of the last setup (malformed stored data reads as none).</summary>
    /// <returns>The seats, in setup order.</returns>
    public static IReadOnlyList<SeatRecord> LoadLastSeats() => FromJson(LastSeatsJson);

    /// <summary>Stores the seats of a setup as the last setup.</summary>
    /// <param name="seats">The seats.</param>
    /// <returns>The JSON written.</returns>
    public static string SaveLastSeats(IEnumerable<SeatRecord> seats)
    {
        ArgumentNullException.ThrowIfNull(seats);
        var json = ToJson(seats);
        LastSeatsJson = json;
        return json;
    }

    /// <summary>Serializes seats to the stored JSON form.</summary>
    /// <param name="seats">The seats.</param>
    /// <returns>A JSON array.</returns>
    public static string ToJson(IEnumerable<SeatRecord> seats)
    {
        ArgumentNullException.ThrowIfNull(seats);
        return JsonSerializer.Serialize(seats.Where(s => s != null).ToArray());
    }

    /// <summary>Parses the stored JSON form; anything malformed yields no seats.</summary>
    /// <param name="json">The JSON.</param>
    /// <returns>The seats that could be read.</returns>
    public static IReadOnlyList<SeatRecord> FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<SeatRecord>();
        }

        try
        {
            var seats = JsonSerializer.Deserialize<SeatRecord[]>(json);
            return seats == null ? Array.Empty<SeatRecord>() : seats.Where(s => s != null).ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<SeatRecord>();
        }
    }

    private static void CreateProperties()
    {
        _soundEnabled = AppSettingsService.Wrap(SoundEnabledKey, true);
        _reducedMotion = AppSettingsService.Wrap(ReducedMotionKey, false);
        _animationSpeed = AppSettingsService.Wrap(AnimationSpeedKey, DefaultAnimationSpeed);
        _revealComputerDiscoveries = AppSettingsService.Wrap(RevealComputerDiscoveriesKey, true);
        _turnsPerSeason = AppSettingsService.Wrap(TurnsPerSeasonKey, DefaultTurnsPerSeason);
        _difficulty = AppSettingsService.Wrap(DifficultyKey, nameof(Difficulty.Standard));
        _lastSeats = AppSettingsService.Wrap(LastSeatsKey, string.Empty);
    }

    private static void Store<T>(AppSettingProperty<T> property, T value)
    {
        Require(property).Set(value);
        Changed?.Invoke();
    }

    private static T Require<T>(T property) where T : class
    {
        if (!AppSettingsService.IsInitialized)
        {
            throw new InvalidOperationException("SettingsService.Initialize has not run.");
        }

        return property ?? throw new InvalidOperationException("SettingsService.Initialize has not run.");
    }

    private static double ClampSpeed(double value) =>
        double.IsNaN(value) ? DefaultAnimationSpeed : Math.Clamp(value, MinAnimationSpeed, MaxAnimationSpeed);
}
