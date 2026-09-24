using System;
using System.Collections.Generic;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Hosting;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// The game's <see cref="IMusicDirector"/>: endless generated music through <c>Engine.UseGeneratedMusic</c>. One
/// session starts with the title music and plays for the whole run; sectors and bosses move it on with provider
/// follow-ups (never a restart), screen moments use the engine's music bus (duck, fade), and only a change of model or
/// instrument library on the settings screen starts a fresh session.
/// </summary>
/// <remarks>
/// <para>
/// Every <see cref="IMusicDirector"/> call arrives on the engine thread. The provider's <c>StateChanged</c> can arrive
/// on any thread (the audio fill thread included), so the handler only posts to the engine thread, where every state
/// change is logged (<c>[BrixInvaders] music: state Starting -&gt; Playing</c>) and, on the first Playing of each
/// session, what is really playing (<c>[BrixInvaders] music: SkyTNT (SkyTNT) through ModestSynthGm, ...</c>).
/// </para>
/// <para>
/// Volume: the player's music slider drives the engine's music bus (<c>AudioMixer.MusicVolume</c>) and the session's
/// own level stays at 1, so the level is applied exactly once. Ducks are a separate multiplier on that bus, so the
/// slider survives every duck.
/// </para>
/// <para>
/// PAUSE IS A DUCK, NOT A SUSPENSION. The pause menu is a GAME pause (the engine keeps running so the menu can take
/// input). Suspending the music track would stop pulling the stream and leave the model's next phrase waiting, and a
/// resumed stream can come back through a moment of silence; ducking keeps the music breathing quietly under the menu
/// - the calmer feel for a menu a player may sit on - and it is back at full level a moment after Resume.
/// </para>
/// <para>
/// The stingers are the game's own effects on the same events: the boss warning (<c>AssetKeys.Sfx.BossWarning</c>)
/// and the game-over sting (<c>AssetKeys.Sfx.GameOver</c>) are triggered by the sound table on the effects bus at top
/// priority - so they still sound with the music turned down - and this director ducks or fades the music under them.
/// </para>
/// </remarks>
public sealed class GeneratedMusicDirector : IMusicDirector
{
    /// <summary>The music level while the pause menu is up.</summary>
    public const float PauseDuckDepth = 0.35f;

    /// <summary>The music level under the boss warning.</summary>
    public const float BossDuckDepth = 0.3f;

    /// <summary>The music level while the game-over screen is up.</summary>
    public const float GameOverDuckDepth = 0.2f;

    /// <summary>How many Starved changes are logged one by one before only every tenth is.</summary>
    public const int StarvedLinesLoggedInFull = 10;

    /// <summary>How fast the music ducks when the pause menu opens.</summary>
    public static readonly TimeSpan PauseAttack = TimeSpan.FromSeconds(0.25);

    /// <summary>How fast the music comes back after the pause menu.</summary>
    public static readonly TimeSpan PauseRelease = TimeSpan.FromSeconds(0.5);

    /// <summary>How fast the music ducks under the boss warning.</summary>
    public static readonly TimeSpan BossAttack = TimeSpan.FromSeconds(0.15);

    /// <summary>How long the music stays ducked under the boss warning (the warning lasts three seconds).</summary>
    public static readonly TimeSpan BossHold = TimeSpan.FromSeconds(GameSimulation.BossWarningDuration - 0.5);

    /// <summary>How fast the music comes back after the boss warning.</summary>
    public static readonly TimeSpan BossRelease = TimeSpan.FromSeconds(1.0);

    /// <summary>How long the music takes to fade down when the game is over.</summary>
    public static readonly TimeSpan GameOverFade = TimeSpan.FromSeconds(2.0);

    /// <summary>How long the music takes to come back up on the title after a game over.</summary>
    public static readonly TimeSpan GameOverRelease = TimeSpan.FromSeconds(1.5);

    private readonly IMusicEngine _engine;
    private MusicSettings _settings = new MusicSettings();
    private IMusicStream _stream;
    private Action _detach;
    private int _streamNumber;
    private StreamingMusicState _loggedState = StreamingMusicState.Stopped;
    private bool _sourceLogged;
    private IDisposable _pauseDuck;
    private IDisposable _gameOverDuck;
    private int _starvedCount;
    private int _previousStarvationGaps;
    private bool _stopped;

    /// <summary>Creates the director.</summary>
    /// <param name="engine">The engine side (<see cref="EngineMusicEngine"/> in the game).</param>
    public GeneratedMusicDirector(IMusicEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <summary>The player's music choices the running session was started with (a copy).</summary>
    public MusicSettings Settings => _settings.Clone();

    /// <summary>The sector the music currently suits (0 = the title).</summary>
    public int Sector { get; private set; }

    /// <summary>Whether the music is the boss music.</summary>
    public bool Boss { get; private set; }

    /// <summary>How many times any session went Starved (the music waited for the generator) in this run.</summary>
    public int StarvedCount => _starvedCount;

    /// <summary>Whether the pause duck is on.</summary>
    public bool IsPauseDucked => _pauseDuck != null;

    /// <summary>Whether the game-over fade is on.</summary>
    public bool IsGameOverFaded => _gameOverDuck != null;

    /// <inheritdoc />
    public string ActiveSource => _stream?.Summary ?? string.Empty;

    /// <inheritdoc />
    public void Start(MusicSettings settings, double musicVolume)
    {
        _engine.SetMusicVolume(musicVolume);
        _engine.RegisterEverything();
        _settings = Normalize(settings);
        StartSession(SectorMusic.TitleSector, false, "start-up");
    }

    /// <inheritdoc />
    public void OnTitle()
    {
        ReleaseDucks();
        Sector = SectorMusic.TitleSector;
        Boss = false;
        FollowUp(MusicSetup.FollowUpFor(_settings, SectorMusic.TitleSector), "title");
    }

    /// <inheritdoc />
    public void OnSector(int sector)
    {
        ReleaseDucks();
        Sector = Math.Max(1, sector);
        Boss = false;
        FollowUp(MusicSetup.FollowUpFor(_settings, Sector), $"sector {Sector}");
    }

    /// <inheritdoc />
    public void OnBoss(int sector)
    {
        Sector = Math.Max(1, sector);
        Boss = true;
        _engine.Duck(BossDuckDepth, BossAttack, BossHold, BossRelease);
        GameLog.Write($"music: boss of sector {Sector} - ducked to {BossDuckDepth:0.##} under the boss-warning stinger");
        FollowUp(MusicSetup.FollowUpFor(_settings, Sector, boss: true), $"boss of sector {Sector}");
    }

    /// <inheritdoc />
    public void OnGameOver()
    {
        ReleasePauseDuck();
        if (_gameOverDuck == null)
        {
            _gameOverDuck = _engine.PushDuck(GameOverDuckDepth, GameOverFade, GameOverRelease);
        }

        GameLog.Write($"music: game over - fading down to {GameOverDuckDepth:0.##} under the game-over stinger; the session plays on");
    }

    /// <inheritdoc />
    public void OnPause()
    {
        if (_pauseDuck == null)
        {
            _pauseDuck = _engine.PushDuck(PauseDuckDepth, PauseAttack, PauseRelease);
        }

        GameLog.Write($"music: paused - ducked to {PauseDuckDepth:0.##} (the music plays on under the pause menu)");
    }

    /// <inheritdoc />
    public void OnResume()
    {
        ReleasePauseDuck();
        GameLog.Write("music: resumed - back to full level");
    }

    /// <inheritdoc />
    public void SetVolumes(double master, double music, double effects) => _engine.SetMusicVolume(music);

    /// <inheritdoc />
    public void ApplySettings(MusicSettings settings, int sector, bool boss)
    {
        var next = Normalize(settings);
        var sameChoice = string.Equals(next.GeneratorName, _settings.GeneratorName, StringComparison.Ordinal) &&
                         string.Equals(next.InstrumentLibraryName, _settings.InstrumentLibraryName, StringComparison.Ordinal);
        if (sameChoice && _stream != null)
        {
            GameLog.Write($"music: settings unchanged ({next.GeneratorName} through {next.InstrumentLibraryName}) - the session plays on");
            return;
        }

        _settings = next;
        var safeSector = Math.Max(0, sector);
        StartSession(safeSector, boss && safeSector > 0, "settings changed");
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_stopped)
        {
            return;
        }

        _stopped = true;
        var stream = _stream;
        var state = stream?.State ?? StreamingMusicState.Stopped;
        var gaps = _previousStarvationGaps + (stream?.StarvationGapCount ?? 0);
        GameLog.Write($"music: stopping - state {state}, {_starvedCount} starvation(s) seen this run, " +
                      $"{gaps} starvation gap(s) in the diagnostics");
        var diagnostics = stream?.DiagnosticsSummary;
        if (!string.IsNullOrEmpty(diagnostics))
        {
            GameLog.Write($"music: diagnostics: {diagnostics}");
        }

        Detach();
        _pauseDuck = null;
        _gameOverDuck = null;
    }

    /// <summary>The credits screen's music card for what is playing now.</summary>
    /// <returns>The card's lines.</returns>
    public IReadOnlyList<CreditsLine> CreditLines() => MusicCreditsCard.Lines(_stream?.Source, _settings);

    private static MusicSettings Normalize(MusicSettings settings)
    {
        var result = new MusicSettings();
        if (settings != null)
        {
            try
            {
                result.GeneratorName = MusicChoices.ResolveGenerator(settings.GeneratorName);
            }
            catch (ArgumentException)
            {
                GameLog.Write($"music: unknown generator '{settings.GeneratorName}' - using {MusicChoices.DefaultGenerator}");
            }

            try
            {
                result.InstrumentLibraryName = MusicChoices.ResolveInstrumentLibrary(settings.InstrumentLibraryName);
            }
            catch (ArgumentException)
            {
                GameLog.Write($"music: unknown instrument library '{settings.InstrumentLibraryName}' - using {MusicChoices.DefaultInstrumentLibrary}");
            }
        }

        result.GeneratorName = MusicChoices.ResolveGenerator(result.GeneratorName);
        result.InstrumentLibraryName = MusicChoices.ResolveInstrumentLibrary(result.InstrumentLibraryName);

        //The player's slider drives the engine's music bus, so the session's own level stays at 1 (never both)
        result.MusicVolume = 1.0;
        return result;
    }

    private void StartSession(int sector, bool boss, string reason)
    {
        Sector = sector;
        Boss = boss;
        GeneratedMusicOptions options;
        try
        {
            options = MusicSetup.OptionsFor(_settings, sector, boss);
        }
        catch (ArgumentException failure)
        {
            GameLog.Write($"music: no options for sector {sector}: {failure.Message}");
            return;
        }

        Detach();
        _streamNumber++;
        _loggedState = StreamingMusicState.Stopped;
        _sourceLogged = false;
        GameLog.Write($"music: {(_streamNumber == 1 ? "starting" : "fresh session")} ({reason}) - {options.Generator} through " +
                      $"{options.InstrumentLibrary}, {options.Preset} at {options.BeatsPerMinute:0} BPM");

        IMusicStream stream;
        try
        {
            stream = _engine.Start(options);
        }
        catch (Exception failure)
        {
            GameLog.Write($"music: could not start: {failure.GetType().Name}: {failure.Message} - the game plays on in silence");
            return;
        }

        _stream = stream;
        var number = _streamNumber;
        stream.StateChanged += OnStreamStateChanged;
        LogState(stream, number);

        void OnStreamStateChanged(object sender, EventArgs args) => _engine.Post(() => LogState(stream, number));

        _detach = () => stream.StateChanged -= OnStreamStateChanged;
    }

    private void Detach()
    {
        if (_stream != null)
        {
            _previousStarvationGaps += _stream.StarvationGapCount;
        }

        _detach?.Invoke();
        _detach = null;
        _stream = null;
    }

    private void LogState(IMusicStream stream, int number)
    {
        if (_stopped || number != _streamNumber)
        {
            return;
        }

        var state = stream.State;
        if (state == _loggedState)
        {
            return;
        }

        var from = _loggedState;
        _loggedState = state;
        if (state == StreamingMusicState.Starved)
        {
            _starvedCount++;
            if (_starvedCount > StarvedLinesLoggedInFull && _starvedCount % 10 != 0)
            {
                return;
            }
        }
        else if (state == StreamingMusicState.Playing && from == StreamingMusicState.Starved &&
                 _starvedCount > StarvedLinesLoggedInFull)
        {
            return;
        }

        var count = state == StreamingMusicState.Starved ? $" (starvation {_starvedCount})" : string.Empty;
        var fault = state == StreamingMusicState.Faulted && stream.Fault != null ? $": {stream.Fault.Message}" : string.Empty;
        GameLog.Write($"music: state {from} -> {state}{count}{fault}");

        if (state == StreamingMusicState.Playing && !_sourceLogged)
        {
            _sourceLogged = true;
            GameLog.Write($"music: {stream.Summary}");
        }
    }

    private void FollowUp(string preset, string moment)
    {
        var stream = _stream;
        if (stream == null)
        {
            return;
        }

        try
        {
            stream.FollowUp(preset);
            GameLog.Write($"music: {moment} - follow-up {preset} (the session plays on; it takes over at a bar line)");
        }
        catch (Exception failure)
        {
            //A refused follow-up never stops the game: the music carries on as it was
            GameLog.Write($"music: {moment} - follow-up {preset} refused: {failure.GetType().Name}: {failure.Message}");
        }
    }

    private void ReleaseDucks()
    {
        ReleasePauseDuck();
        if (_gameOverDuck != null)
        {
            _gameOverDuck.Dispose();
            _gameOverDuck = null;
        }
    }

    private void ReleasePauseDuck()
    {
        if (_pauseDuck != null)
        {
            _pauseDuck.Dispose();
            _pauseDuck = null;
        }
    }
}
