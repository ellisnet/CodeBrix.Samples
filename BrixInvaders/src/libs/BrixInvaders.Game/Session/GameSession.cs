using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Links;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;

namespace BrixInvaders.Game.Session;

/// <summary>
/// The whole game with no engine in it: the screen state machine, the running game and the attract-mode demo,
/// the high-score tables and the settings rows, driven one fixed step at a time. The host feeds it input and
/// draws what it holds; everything that is not drawing happens here (and is unit tested).
/// </summary>
/// <remarks>
/// Per step (DESIGN.md section 12): <see cref="ScreenStateMachine.Update"/>, act on its commands, step the
/// simulation while the screen is Playing (or the demo while it is Attract), notify the machine of what the
/// simulation reported, and act on the commands that produced. Game events are kept in <see cref="Events"/> until
/// the host takes them (particles, flashes). Runs on the engine thread only.
/// </remarks>
public sealed class GameSession
{
    /// <summary>How long the "No browser was available." message stays up.</summary>
    public const double LinkMessageSeconds = 4.0;

    /// <summary>The message shown when a link could not be opened.</summary>
    public const string NoBrowserMessage = "No browser was available.";

    private readonly ISoundOutput _sound;
    private readonly IMusicDirector _music;
    private readonly IExternalLinkOpener _links;
    private readonly List<GameEvent> _events = new List<GameEvent>();
    private readonly Random _seeds;
    private AttractPilot _pilot;
    private int _processed;
    private int _linkFailed;
    private bool _rebuildMachine;
    private GameScreen _shownScreen = GameScreen.Splash;

    /// <summary>Creates a session at the splash screen.</summary>
    /// <param name="settings">The stored settings and records.</param>
    /// <param name="sound">Where sounds go.</param>
    /// <param name="music">The music director (use <see cref="SilentMusicDirector"/> for none).</param>
    /// <param name="links">The link opener.</param>
    /// <param name="seed">The seed the game seeds are drawn from.</param>
    public GameSession(IGameSettings settings, ISoundOutput sound, IMusicDirector music, IExternalLinkOpener links, int seed)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _sound = sound ?? throw new ArgumentNullException(nameof(sound));
        _music = music ?? throw new ArgumentNullException(nameof(music));
        _links = links ?? throw new ArgumentNullException(nameof(links));
        _seeds = new Random(seed);
        HighScores = settings.LoadHighScores();
        SettingsMenu = new SettingsMenu(settings);
        Screens = CreateMachine();
    }

    /// <summary>Raised when the player chooses Quit on the title (the host closes the application).</summary>
    public event Action QuitRequested;

    /// <summary>Raised on every screen change (from, to).</summary>
    public event Action<GameScreen, GameScreen> ScreenChanged;

    /// <summary>The stored settings and records.</summary>
    public IGameSettings Settings { get; }

    /// <summary>The screen state machine.</summary>
    public ScreenStateMachine Screens { get; private set; }

    /// <summary>The screen on show.</summary>
    public GameScreen CurrentScreen => Screens.CurrentScreen;

    /// <summary>The running game, or null (none started, or quit/finished and back on the title).</summary>
    public GameSimulation Game { get; private set; }

    /// <summary>The attract-mode demo game, or null when attract mode is off.</summary>
    public GameSimulation Attract { get; private set; }

    /// <summary>The high-score tables.</summary>
    public HighScoreTable HighScores { get; }

    /// <summary>The settings rows.</summary>
    public SettingsMenu SettingsMenu { get; }

    /// <summary>The musical moment (what the music director was last told).</summary>
    public MusicMoment Music { get; private set; } = MusicMoment.Title;

    /// <summary>The sector the music was last told about (0 on the title).</summary>
    public int MusicSector { get; private set; }

    /// <summary>Seconds since the session started.</summary>
    public double Time { get; private set; }

    /// <summary>The rank (0-based) of the high score entered last, or -1.</summary>
    public int LastHighScoreRank { get; private set; } = -1;

    /// <summary>The transient message on screen (a link that could not open), or null.</summary>
    public string Message { get; private set; }

    /// <summary>Seconds the message has left.</summary>
    public double MessageTimeLeft { get; private set; }

    /// <summary>The game events of the steps since the host last called <see cref="ClearEvents"/>.</summary>
    public IReadOnlyList<GameEvent> Events => _events;

    /// <summary>Forgets the collected events (the host has turned them into effects).</summary>
    public void ClearEvents() => _events.Clear();

    /// <summary>Advances everything by one fixed step.</summary>
    /// <param name="dt">The step in seconds (normally <see cref="Playfield.FixedStep"/>).</param>
    /// <param name="menu">The menu presses since the last step.</param>
    /// <param name="play">The held play input.</param>
    public void Update(double dt, MenuInput menu, GameInput play)
    {
        Time += dt;
        UpdateMessage(dt);

        var before = Screens.CurrentScreen;
        _processed = 0;
        Screens.Update(dt, menu);
        PlayMenuSounds(before, menu);
        ProcessCommands();

        if (Screens.CurrentScreen == GameScreen.Playing && Game != null)
        {
            Game.Step(dt, play);
            HandleGameEvents(Game);
            ProcessCommands();
        }
        else if (Screens.CurrentScreen == GameScreen.Attract && Attract != null)
        {
            Attract.Step(dt, _pilot.Decide(Attract));
            _events.AddRange(Attract.Events);
            if (Attract.IsGameOver || Attract.Phase == StagePhase.SectorComplete)
            {
                Screens.NotifyAttractEnded();
                ProcessCommands();
            }
        }
    }

    /// <summary>The splash overlay has finished: go to the title (does nothing once past the splash).</summary>
    public void NotifySplashComplete()
    {
        _processed = Screens.Commands.Count;
        Screens.NotifySplashComplete();
        ProcessCommands();
    }

    /// <summary>Opens a link through the link seam; a failure shows <see cref="NoBrowserMessage"/>.</summary>
    /// <param name="url">The link.</param>
    public void OpenLink(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        GameLog.Write($"link: opening {url}");
        _sound.Play(SoundTable.MenuConfirm);
        try
        {
            _links.Open(url).ContinueWith(
                task =>
                {
                    if (task.IsFaulted || task.IsCanceled || !task.Result)
                    {
                        Interlocked.Exchange(ref _linkFailed, 1);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
        catch (Exception failure)
        {
            GameLog.Write($"link: could not open {url}: {failure.Message}");
            Interlocked.Exchange(ref _linkFailed, 1);
        }
    }

    /// <summary>The music choices as the Music library's settings object.</summary>
    /// <returns>A new settings object.</returns>
    public MusicSettings CreateMusicSettings() => new MusicSettings
    {
        GeneratorName = Settings.MusicGenerator,
        InstrumentLibraryName = Settings.InstrumentLibrary,
    };

    private ScreenStateMachine CreateMachine()
    {
        var machine = new ScreenStateMachine(SettingsMenu.RowCount, Settings.Difficulty, Settings.ShipShape, Settings.ShipColour);
        foreach (var difficulty in DifficultyTable.Levels)
        {
            machine.SetUnlockedSector(difficulty, Math.Min(Settings.GetHighestSectorCleared(difficulty) + 1, ScreenStateMachine.MaxUnlockableSector));
        }

        machine.SetLastName(Settings.LastName);
        return machine;
    }

    private void UpdateMessage(double dt)
    {
        if (Interlocked.Exchange(ref _linkFailed, 0) == 1)
        {
            Message = NoBrowserMessage;
            MessageTimeLeft = LinkMessageSeconds;
            GameLog.Write($"link: {NoBrowserMessage}");
        }

        if (MessageTimeLeft > 0)
        {
            MessageTimeLeft = Math.Max(0, MessageTimeLeft - dt);
            if (MessageTimeLeft <= 0)
            {
                Message = null;
            }
        }
    }

    private void PlayMenuSounds(GameScreen screen, MenuInput menu)
    {
        if (screen is GameScreen.Playing or GameScreen.Attract or GameScreen.Splash)
        {
            return;
        }

        if (menu.Confirm || menu.Start)
        {
            _sound.Play(SoundTable.MenuConfirm);
        }
        else if (menu.Back)
        {
            _sound.Play(SoundTable.MenuBack);
        }
        else if (menu.Up || menu.Down || menu.Left || menu.Right)
        {
            _sound.Play(SoundTable.MenuMove);
        }
    }

    private void ProcessCommands()
    {
        var commands = Screens.Commands;
        while (_processed < commands.Count)
        {
            var command = commands[_processed++];
            Handle(command);
        }

        if (_rebuildMachine && Screens.CurrentScreen == GameScreen.Title)
        {
            //The machine holds the ship and difficulty it offers first; a changed default takes effect by starting a
            //  fresh machine from the stored settings, fast-forwarded to the title the player is already looking at
            _rebuildMachine = false;
            Screens = CreateMachine();
            Screens.NotifySplashComplete();
            _processed = Screens.Commands.Count;
        }
    }

    private void Handle(ScreenCommand command)
    {
        switch (command.Kind)
        {
            case ScreenCommandKind.ScreenChanged:
                OnScreenChanged((GameScreen)command.Value);
                break;
            case ScreenCommandKind.StartNewGame:
                StartNewGame(command.Value);
                break;
            case ScreenCommandKind.BeginSector:
                if (Game != null && Game.Phase == StagePhase.SectorComplete)
                {
                    Game.BeginNextSector();
                }

                GameLog.Write($"sector {Game?.Sector ?? command.Value} begins: {SectorRules.NameOf(Game?.Sector ?? command.Value)}");
                break;
            case ScreenCommandKind.PauseGame:
                _music.OnPause();
                break;
            case ScreenCommandKind.ResumeGame:
                _music.OnResume();
                break;
            case ScreenCommandKind.AbandonGame:
                GameLog.Write($"game abandoned at sector {Game?.Sector ?? 0} with {Game?.Score ?? 0} points");
                Game = null;
                break;
            case ScreenCommandKind.SubmitHighScore:
                SubmitHighScore(command.Text);
                break;
            case ScreenCommandKind.OpenKenneyLink:
                OpenLink(KenneyPacks.BundleUrl);
                break;
            case ScreenCommandKind.AdjustSetting:
                Apply(SettingsMenu.Adjust(command.Value, command.Delta));
                break;
            case ScreenCommandKind.ActivateSetting:
                Apply(SettingsMenu.Activate(command.Value));
                break;
            case ScreenCommandKind.StartAttract:
                _pilot = new AttractPilot();
                Attract = new GameSimulation(new GameSetup(Difficulty.Cadet, 1, _seeds.Next(), _seeds.Next(GameSetup.ShipShapeCount),
                    _seeds.Next(GameSetup.ShipColourCount)));
                GameLog.Write("attract: the demo pilot takes over");
                break;
            case ScreenCommandKind.StopAttract:
                if (Attract != null)
                {
                    GameLog.Write($"attract: demo ended - {Attract.Score} points, wave {Attract.Wave}, {Attract.StepCount} steps");
                }

                Attract = null;
                break;
            case ScreenCommandKind.QuitGame:
                GameLog.Write("quit requested from the title");
                QuitRequested?.Invoke();
                break;
        }
    }

    private void OnScreenChanged(GameScreen screen)
    {
        var from = _shownScreen;
        _shownScreen = screen;
        GameLog.Write($"screen: {from} -> {screen}");

        switch (screen)
        {
            case GameScreen.Title:
                Game = null;
                LastHighScoreRank = -1;
                SetMusic(MusicMoment.Title, 0);
                break;
            case GameScreen.SectorBriefing:
                SetMusic(MusicMoment.Sector, Screens.CurrentSector);
                break;
            case GameScreen.GameOver:
                SetMusic(MusicMoment.GameOver, MusicSector);
                break;
            case GameScreen.Settings:
                SettingsMenu.Open();
                break;
        }

        if (from == GameScreen.Settings && screen == GameScreen.Title && _rebuildMachine)
        {
            GameLog.Write($"settings: new defaults - {SettingsMenu.ShipName(Settings.ShipShape, Settings.ShipColour)}, {Settings.Difficulty}");
        }

        ScreenChanged?.Invoke(from, screen);
    }

    private void SetMusic(MusicMoment moment, int sector)
    {
        if (moment == Music && sector == MusicSector)
        {
            return;
        }

        Music = moment;
        MusicSector = sector;
        switch (moment)
        {
            case MusicMoment.Title:
                _music.OnTitle();
                break;
            case MusicMoment.Sector:
                _music.OnSector(sector);
                break;
            case MusicMoment.Boss:
                _music.OnBoss(sector);
                break;
            case MusicMoment.GameOver:
                _music.OnGameOver();
                break;
        }
    }

    private void StartNewGame(int startSector)
    {
        var setup = new GameSetup(Screens.Difficulty, Math.Max(1, startSector), _seeds.Next(), Screens.ShipShape, Screens.ShipColour);
        Game = new GameSimulation(setup);
        Settings.ShipShape = setup.ShipShape;
        Settings.ShipColour = setup.ShipColour;
        Settings.Difficulty = setup.Difficulty;
        GameLog.Write($"new game: {setup.Difficulty}, sector {setup.StartSector}, " +
                      $"{SettingsMenu.ShipName(setup.ShipShape, setup.ShipColour)}, seed {setup.Seed}");
    }

    private void HandleGameEvents(GameSimulation game)
    {
        foreach (var gameEvent in game.Events)
        {
            _events.Add(gameEvent);
            if (SoundTable.TryGetCue(gameEvent, out var cue))
            {
                _sound.Play(cue);
            }

            switch (gameEvent.Kind)
            {
                case GameEventKind.WaveStarted:
                    GameLog.Write($"sector {game.Sector} wave {gameEvent.Value} - score {game.Score}, lives {game.Player.Lives}");
                    break;
                case GameEventKind.PlayerDestroyed:
                    var where = game.Phase is StagePhase.BossWarning or StagePhase.BossFight ? "the boss fight" : $"wave {game.Wave}";
                    GameLog.Write($"ship lost in sector {game.Sector} {where} - lives left {gameEvent.Value}");
                    break;
                case GameEventKind.BossDefeated:
                    GameLog.Write($"boss of sector {game.Sector} defeated - score {game.Score}");
                    break;
                case GameEventKind.BossIncoming:
                    GameLog.Write($"boss incoming in sector {game.Sector}");
                    SetMusic(MusicMoment.Boss, game.Sector);
                    break;
                case GameEventKind.SectorCleared:
                    Settings.RecordSectorCleared(game.Setup.Difficulty, gameEvent.Value);
                    GameLog.Write($"sector {gameEvent.Value} cleared on {game.Setup.Difficulty} - score {game.Score}");
                    Screens.NotifySectorCleared(gameEvent.Value);
                    break;
                case GameEventKind.GameOver:
                    var qualifies = HighScores.Qualifies(game.Setup.Difficulty, game.Score);
                    GameLog.Write($"game over: {game.Score} points in sector {game.Sector} on {game.Setup.Difficulty}" +
                                  (qualifies ? " - a new high score" : " - no high score"));
                    Screens.NotifyGameOver(game.Score, qualifies);
                    break;
            }
        }
    }

    private void SubmitHighScore(string name)
    {
        var difficulty = Screens.Difficulty;
        var sector = Game?.Sector ?? Screens.CurrentSector;
        LastHighScoreRank = HighScores.Insert(difficulty, name, Screens.FinalScore, sector);
        Settings.SaveHighScores(HighScores, difficulty);
        Settings.LastName = name;
        _sound.Play(SoundTable.HighScore);
        GameLog.Write($"high scores: saved {name} {Screens.FinalScore} (sector {sector}) on {difficulty} at rank {LastHighScoreRank + 1}");
    }

    private void Apply(SettingsChange change)
    {
        switch (change)
        {
            case SettingsChange.Volumes:
                _sound.SetLevels(Settings.MasterVolume, Settings.EffectsVolume);
                _music.SetVolumes(Settings.MasterVolume, Settings.MusicVolume, Settings.EffectsVolume);
                break;
            case SettingsChange.MusicChoice:
                GameLog.Write($"settings: music {Settings.MusicGenerator} through {Settings.InstrumentLibrary}");
                _music.ApplySettings(CreateMusicSettings(), Music is MusicMoment.Sector or MusicMoment.Boss ? MusicSector : 0,
                    Music == MusicMoment.Boss);
                break;
            case SettingsChange.GamepadProfile:
                GameLog.Write($"settings: gamepad profile {Settings.GamepadProfile}");
                break;
            case SettingsChange.Defaults:
                _rebuildMachine = true;
                break;
            case SettingsChange.HighScoresReset:
                HighScores.ClearAll();
                GameLog.Write("settings: every high-score table cleared");
                break;
        }
    }
}
