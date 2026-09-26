using System;
using System.Diagnostics;
using System.Linq;
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration;
using CodeBrix.Audio.MusicGeneration.MuPT;
using CodeBrix.Audio.MusicGeneration.SkyTNT;
using CodeBrix.Audio.Samples.FluidR3Gm;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.GeneratedMusic;
using Microsoft.Extensions.Logging;

namespace BrixInvaders.Music;

/// <summary>
/// The game's music set-up: registers the instrument libraries and the models once at start-up, and turns the
/// player's settings plus the current sector into the options for the engine's generated music.
/// </summary>
/// <remarks>
/// Start-up, after <c>AudioSystem.Initialize</c>:
/// <code>
/// MusicSetup.RegisterEverything();
/// var music = Engine.Instance.UseGeneratedMusic(MusicSetup.OptionsFor(settings, SectorMusic.TitleSector));
/// </code>
/// A new sector is a follow-up on the same session, never a restart:
/// <c>music.FollowUp(MusicSetup.FollowUpFor(settings, sector))</c>. A new generator or instrument library from the
/// settings screen is a second <c>UseGeneratedMusic</c> call with new options.
/// </remarks>
public static class MusicSetup
{
    /// <summary>The prefix of every log line this library writes.</summary>
    public const string LogPrefix = "[BrixInvaders] music:";

    /// <summary>The engine music track key the generated music plays under.</summary>
    public const string TrackKey = "brixinvaders-music";

    /// <summary>The output sample rate the game pins with <c>AudioSystem.Initialize</c> (the music renders at whatever it is).</summary>
    public const int RecommendedSampleRate = 48000;

    /// <summary>The output channel count the game pins with <c>AudioSystem.Initialize</c>.</summary>
    public const int RecommendedChannels = 2;

    /// <summary>
    /// The sentence logged when no model generator ended up registered: the music falls back to the music package's
    /// embedded replay.
    /// </summary>
    public const string NoModelFallbackNote =
        "no music model is registered, so the music is the music package's embedded replay (the same recorded piece " +
        "every time); SkyTNTModel.Register() or MuPTModel.Register() brings generated music back.";

    /// <summary>
    /// How long the outgoing and incoming pieces overlap where the music moves on to a fresh piece: four seconds, the
    /// setting listening settled on for both models (BLUEPRINTS-GeneratingMusic, "Crossfade the seams between fresh
    /// pieces").
    /// </summary>
    public static readonly TimeSpan SeamCrossfade = TimeSpan.FromSeconds(4.0);

    private static readonly object _gate = new();
    private static bool _registered;

    /// <summary>Whether <see cref="RegisterEverything"/> has run in this process.</summary>
    public static bool IsRegistered
    {
        get { lock (_gate) { return _registered; } }
    }

    /// <summary>
    /// Registers everything the game's music needs, once: ModestSynthGm FIRST (so it is the default instrument
    /// library), then FluidR3Gm, then the SkyTNT and MuPT generators. Nothing is loaded - the models and the
    /// SoundFont load when the music first asks for them. Logs one line naming what is registered (and
    /// <see cref="NoModelFallbackNote"/> when no model generator is). Later calls do nothing.
    /// </summary>
    /// <param name="log">Where the log lines go; null = <c>Debug.WriteLine</c> and the engine logger.</param>
    /// <returns>True when this call did the registering; false when it had already been done.</returns>
    public static bool RegisterEverything(Action<string> log = null)
    {
        lock (_gate)
        {
            if (_registered) { return false; }

            GeneralMidiInstrumentLibrary.Register();
            FluidR3GmInstrumentLibrary.Register();
            if (!string.Equals(InstrumentLibraryRegistry.DefaultName, MusicChoices.ModestSynthGm, StringComparison.OrdinalIgnoreCase))
            {
                //Something registered a library before this ran: the game's default is still ModestSynthGm
                InstrumentLibraryRegistry.SetDefault(MusicChoices.ModestSynthGm);
            }

            SkyTNTModel.Register();
            MuPTModel.Register();
            _registered = true;
        }

        log ??= WriteLog;
        log(DescribeRegistration());
        if (RegisteredModelNames().Length == 0) { log($"{LogPrefix} {NoModelFallbackNote}"); }

        return true;
    }

    /// <summary>
    /// One log line describing what is registered now: the instrument libraries (default marked), the model
    /// generators, and whether each model's files were found beside the executable.
    /// </summary>
    /// <returns>A line starting with <see cref="LogPrefix"/>.</returns>
    public static string DescribeRegistration()
    {
        var defaultLibrary = InstrumentLibraryRegistry.DefaultName;
        var libraries = MusicChoices.InstrumentLibraryNames
            .Where(InstrumentLibraryRegistry.IsRegistered)
            .Select(n => string.Equals(n, defaultLibrary, StringComparison.OrdinalIgnoreCase) ? n + " (default)" : n)
            .ToArray();
        var models = RegisteredModelNames();

        var modelFiles = $"model files: {MusicChoices.SkyTNT} {(SkyTNTModel.IsAvailable ? "found" : "missing")}, " +
                         $"{MusicChoices.MuPT} {(MuPTModel.IsAvailable ? "found" : "missing")}";

        return $"{LogPrefix} registered instrument libraries {JoinOrNone(libraries)}; " +
               $"generators {JoinOrNone(models)}; {modelFiles}";
    }

    /// <summary>
    /// The generated-music options for the player's settings in a sector: the chosen generator and instrument
    /// library, the sector's preset for that generator, its tempo, the four-second seam crossfade, the music volume,
    /// the track key <see cref="TrackKey"/>, and StartImmediately on.
    /// </summary>
    /// <param name="settings">The player's music settings.</param>
    /// <param name="sector">The sector (0 = the title screen; 6 and on wrap to designs 1..5).</param>
    /// <param name="boss">True for the sector's boss music (its boss preset and faster tempo).</param>
    /// <returns>New options, ready for <c>Engine.UseGeneratedMusic</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentException">An unknown generator or instrument library name (the message lists the valid
    /// names), or a boss asked for on the title screen.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A negative sector, or a music volume outside 0..1.</exception>
    public static GeneratedMusicOptions OptionsFor(MusicSettings settings, int sector, bool boss = false)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var generator = MusicChoices.ResolveGenerator(settings.GeneratorName);
        var library = MusicChoices.ResolveInstrumentLibrary(settings.InstrumentLibraryName);
        if (!(settings.MusicVolume >= 0.0 && settings.MusicVolume <= 1.0))
        {
            throw new ArgumentOutOfRangeException(nameof(settings), settings.MusicVolume, "MusicVolume must be 0 to 1.");
        }

        var entry = EntryFor(sector, boss);
        return new GeneratedMusicOptions
        {
            Generator = generator,
            InstrumentLibrary = library,
            Preset = entry.PresetFor(generator, boss),
            BeatsPerMinute = entry.BeatsPerMinuteFor(boss),
            SeamCrossfade = SeamCrossfade,
            MasterVolume = (float)settings.MusicVolume,
            TrackKey = TrackKey,
            StartImmediately = true,
        };
    }

    /// <summary>
    /// What to pass to the provider's <c>FollowUp(string)</c> when the game moves on to a sector or its boss: the
    /// preset of the generator that is playing. The session keeps its instrument library and its session tempo; the
    /// follow-up piece itself plays at its preset's own tempo, and the fresh pieces after it return to the session tempo.
    /// </summary>
    /// <param name="settings">The player's music settings (only the generator is read).</param>
    /// <param name="sector">The sector (0 = the title screen).</param>
    /// <param name="boss">True for the boss follow-up.</param>
    /// <returns>A preset name of the playing generator's family.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentException">An unknown generator name, or a boss asked for on the title screen.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A negative sector.</exception>
    public static string FollowUpFor(MusicSettings settings, int sector, bool boss = false)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return FollowUpFor(settings.GeneratorName, sector, boss);
    }

    /// <summary>
    /// What to pass to the provider's <c>FollowUp(string)</c> when the game moves on to a sector or its boss, for a
    /// generator named directly (e.g. <c>provider.Options.Generator</c>).
    /// </summary>
    /// <param name="generatorName">SkyTNT or MuPT (null or blank = the default generator).</param>
    /// <param name="sector">The sector (0 = the title screen).</param>
    /// <param name="boss">True for the boss follow-up.</param>
    /// <returns>A preset name of that generator's family.</returns>
    /// <exception cref="ArgumentException">An unknown generator name, or a boss asked for on the title screen.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A negative sector.</exception>
    public static string FollowUpFor(string generatorName, int sector, bool boss = false)
    {
        var generator = MusicChoices.ResolveGenerator(generatorName);
        return EntryFor(sector, boss).PresetFor(generator, boss);
    }

    /// <summary>Forgets that <see cref="RegisterEverything"/> ran (the registries keep what they hold). Tests only.</summary>
    internal static void ResetForTesting()
    {
        lock (_gate) { _registered = false; }
    }

    private static SectorMusicEntry EntryFor(int sector, bool boss)
    {
        var entry = SectorMusic.For(sector);
        if (boss && entry.Design == SectorMusic.TitleSector)
        {
            throw new ArgumentException("The title screen has no boss music; boss music belongs to sectors 1 and on.", nameof(boss));
        }

        return entry;
    }

    private static string[] RegisteredModelNames() =>
        MusicChoices.GeneratorNames.Where(MusicGeneratorRegistry.IsRegistered).ToArray();

    private static string JoinOrNone(string[] names) => names.Length == 0 ? "(none)" : string.Join(", ", names);

    private static void WriteLog(string line)
    {
        Debug.WriteLine(line);
        Engine.Logger.LogInformation("{Line}", line);
    }
}
