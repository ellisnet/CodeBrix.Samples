using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.CardsAndDice.Assets;
using CodeBrix.Platform.GameEngine.CardsAndDice.Cards;
using CodeBrix.Platform.GameEngine.CardsAndDice.Dice;
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using CodeBrix.Platform.GameEngine.Host.Hosting;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.GameEngine.Input.Actions;
using CodeBrix.Platform.GameEngine.Input.Mouse;
using CodeBrix.Platform.GameEngine.Scenes;
using CodeBrix.Platform.GameEngine.Timers;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Hud;
using GoddessTempleDiscovery.Game.Journal;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using SkiaSharp;
using Windows.System;

namespace GoddessTempleDiscovery.Game.Hosting;

/// <summary>
/// The Goddess Temple Discovery game host: starts the engine over the page's canvas, prepares the card faces, runs
/// one <see cref="TableSession"/> at a time on the engine's fixed step (60 a second), paces the computer teams, takes
/// the human player's clicks and keys, and paints the table and the HUD into one draw list.
/// </summary>
/// <remarks>
/// <para>
/// Rendering: the view model sets the tier before creating the host (GPU unless <c>GODDESSTEMPLE_USE_CPU=1</c>) and
/// pins the render resolution to 1280 x 800; the table widens with the window as the add-on's demo does (never
/// narrower than 1280). One <see cref="DrawListDrawing"/> draws the published HUD list; the table draws into the same
/// list first, so the z-order is the order of the calls.
/// </para>
/// <para>
/// Threads: everything that touches the table, the session or the engine runs on the engine thread. The page's
/// requests (<see cref="StartGame"/>, <see cref="NotifyInspectorClosed"/>, <see cref="ApplySettings"/>) are posted to
/// it; what the page shows is raised through the bridges on the UI thread through the dispatcher the host was given.
/// </para>
/// </remarks>
public sealed class GoddessTempleGameHost : CodeBrixGameHost
{
    /// <summary>The render width the canvas is pinned to.</summary>
    public const int RenderWidth = 1280;

    /// <summary>The render height the canvas is pinned to.</summary>
    public const int RenderHeight = 800;

    /// <summary>The environment variable that switches the game to the CPU render tier when set to 1.</summary>
    public const string CpuTierVariable = "GODDESSTEMPLE_USE_CPU";

    /// <summary>The pause between two computer actions at animation speed 1.</summary>
    public const double ComputerPauseSeconds = 0.8;

    /// <summary>The seconds an inspector opened for a computer team (or a season) stays open by itself.</summary>
    public const double InspectorAutoCloseSeconds = 6;

    private const string ActionRoll = "Roll";
    private const string ActionClose = "Close";
    private const string ActionJournal = "Journal";
    private const string ActionReroll = "Reroll";
    private const string ActionDig = "Dig";
    private const double RegistrationBudgetMs = 12;

    private readonly IInspectorBridge _inspector;
    private readonly ISessionBridge _bridge;
    private readonly Action<Action> _ui;
    private readonly ConcurrentQueue<MouseEventArgs> _mouse = new ConcurrentQueue<MouseEventArgs>();
    private readonly DrawList _hud = new DrawList(capacity: 2048);
    private readonly PlayerControls _controls = new PlayerControls();
    private readonly HudFrame _frame = new HudFrame();
    private readonly Stopwatch _clock = new Stopwatch();
    private readonly bool _autoPlay = AutoPlay.IsRequested();
    private InputActionMap _input;
    private CardsAndDiceTable _table;
    private TableArtwork _artwork;
    private DecoPieces _deco;
    private HudFonts _fonts;
    private HudPainter _painter;
    private DrawListDrawing _drawing;
    private Task<CardFaceLibrary> _facesTask;
    private CardFaceLibrary _faces;
    private IReadOnlyList<string> _pendingFaces;
    private int _registeredFaces;
    private Stopwatch _preparationClock;
    private ArtworkPreparation _cardPreparation;
    private GameSetup _pendingSetup;
    private TableSession _session;
    private Random _brainRandom = new Random();
    private int _tableWidth = RenderWidth;
    private double _idleSeconds;
    private bool _inspectorOpen;
    private double _inspectorSeconds;
    private double _inspectorLimit;
    private bool _gameEndHandled;
    private bool _revealComputer = true;
    private int _tierLogged;
    private bool _rustleLoaded;
    private string _armedButton;
    private string _screenshotPath = AutoPlay.IsRequested() ? AutoPlay.ScreenshotPath() : null;
    private int _settledFrames;
    private volatile bool _panesOpen;
    private volatile int _gameSeed;
    private volatile bool _hasGame;
    private long _stepCount;

    /// <summary>Creates the host.</summary>
    /// <param name="renderSurface">The canvas, already prepared by <see cref="PrepareCanvas"/>.</param>
    /// <param name="inspector">The inspector the view model shows (null for none).</param>
    /// <param name="bridge">The chrome the view model shows (null for none).</param>
    /// <param name="uiDispatch">Runs an action on the UI thread (null runs it at once).</param>
    public GoddessTempleGameHost(GameSurfaceCanvas renderSurface, IInspectorBridge inspector = null, ISessionBridge bridge = null,
        Action<Action> uiDispatch = null)
        : base(renderSurface)
    {
        _inspector = inspector;
        _bridge = bridge;
        _ui = uiDispatch ?? (action => action());
    }

    /// <summary>True when the autoplay switch is on (<see cref="AutoPlay"/>).</summary>
    public bool IsAutoPlay => _autoPlay;

    /// <summary>The game on the table, or null before the first game (read it on the engine thread).</summary>
    public TableSession Session => _session;

    /// <summary>The human player's choices in progress.</summary>
    public PlayerControls Controls => _controls;

    /// <summary>The HUD's ticker and prompt lines as last painted (read-only use: tests and diagnostics).</summary>
    public HudFrame Frame => _frame;

    /// <summary>How many fixed update steps the host has run (tests wait on it: the engine samples input once a step).</summary>
    public long StepCount => Interlocked.Read(ref _stepCount);

    /// <summary>The composed faces, once prepared (the view model builds card views from them).</summary>
    public CardFaceLibrary Faces => _faces;

    /// <summary>
    /// Whether the GPU render tier should be used: yes, unless <c>GODDESSTEMPLE_USE_CPU=1</c>, or an autoplay run asks
    /// for a table screenshot (<see cref="AutoPlay.ScreenshotVariable"/>): the GPU tier draws on the GL side, so only
    /// the CPU tier's backbuffer holds the picture to save.
    /// </summary>
    /// <returns>True for the GPU tier.</returns>
    public static bool UseGpuTier() =>
        Environment.GetEnvironmentVariable(CpuTierVariable) != "1" && !(AutoPlay.IsRequested() && AutoPlay.ScreenshotPath() != null);

    /// <summary>Prepares a canvas before the host is created: the render tier and the pinned render resolution.</summary>
    /// <param name="canvas">The canvas (before its first <c>Host</c> access).</param>
    public static void PrepareCanvas(GameSurfaceCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        canvas.UseGpuRendering = UseGpuTier();
        canvas.SetRenderResolution(RenderWidth, RenderHeight);
        GameLog.Write($"render tier requested: {(canvas.UseGpuRendering ? "GPU" : "CPU")} " +
                      $"({CpuTierVariable}={Environment.GetEnvironmentVariable(CpuTierVariable) ?? "unset"}), " +
                      $"render resolution {RenderWidth}x{RenderHeight}");
    }

    /// <summary>Starts a new game (any thread): it begins as soon as the card faces are ready.</summary>
    /// <param name="setup">The setup.</param>
    public void StartGame(GameSetup setup)
    {
        ArgumentNullException.ThrowIfNull(setup);
        Post(() =>
        {
            _pendingSetup = setup;
            GameLog.Write($"new game requested: {setup.Seats.Count} seats, {setup.TurnsPerSeason} turn(s) a season, " +
                          $"{setup.Difficulty}, seed {(setup.Seed?.ToString(CultureInfo.InvariantCulture) ?? "fresh")}");
        });
    }

    /// <summary>The seed of the game on the table (any thread; read it when <see cref="HasGame"/> is true).</summary>
    public int GameSeed => _gameSeed;

    /// <summary>True once a game has been dealt.</summary>
    public bool HasGame => _hasGame;

    /// <summary>
    /// The page reports whether a pane covers the table (any thread): while one does, the table and the HUD take no
    /// clicks, so nothing behind a pane is played by accident.
    /// </summary>
    /// <param name="open">True while a pane covers the table.</param>
    public void SetPanesOpen(bool open) => _panesOpen = open;

    /// <summary>True while the page reports a pane over the table (the table and the HUD take no clicks).</summary>
    public bool PanesOpen => _panesOpen;

    /// <summary>The page closed the inspector (any thread); a waiting computer turn goes on.</summary>
    public void NotifyInspectorClosed() => Post(() => _inspectorOpen = false);

    /// <summary>Reads the stored settings and applies them to the table (any thread).</summary>
    public void ApplySettings()
    {
        var sound = SettingsService.SoundEnabled;
        var reduced = SettingsService.ReducedMotion;
        var speed = (float)SettingsService.AnimationSpeed;
        var reveal = SettingsService.RevealComputerDiscoveries;
        Post(() =>
        {
            if (_table == null)
            {
                return;
            }

            _table.SoundEnabled = sound;
            _table.ReducedMotion = reduced;
            _table.AnimationSpeed = speed;
            _revealComputer = reveal;
        });
    }

    #region CodeBrixGameHost overrides

    /// <inheritdoc />
    protected override void OnInitializing()
    {
        GameLog.Write("starting");
        GameLog.Write($"settings store: {GameStartup.OpenSettingsStore()}");
        if (_autoPlay)
        {
            GameLog.Write($"autoplay: on ({AutoPlay.Variable}=1), seed {AutoPlay.Seed()?.ToString(CultureInfo.InvariantCulture) ?? "fresh"}");
        }
    }

    /// <inheritdoc />
    protected override void OnMouseAdapterInitialized()
    {
        var mouse = Engine.Input.MouseEventPoller;
        if (mouse == null)
        {
            return;
        }

        mouse.MouseEvent += OnMouseEvent;
        mouse.StartMonitoringMouse(trackMouseMovement: true);
    }

    /// <inheritdoc />
    protected override void LoadAssets()
    {
        _table = new CardsAndDiceTable();
        _artwork = new TableArtwork(_table);
        _deco = new DecoPieces(_artwork);
        _fonts = new HudFonts();
        _painter = new HudPainter(_deco, _fonts);
        _preparationClock = Stopwatch.StartNew();
        _facesTask = Task.Run(CardFaceLibrary.ForCatalog);
    }

    /// <inheritdoc />
    protected override Scene CreateInitialScene()
    {
        var scene = new Scene();
        scene.AddPixelLayer(RenderWidth, RenderHeight);
        return scene;
    }

    /// <inheritdoc />
    protected override void CreateInitialViews() => RenderSurface.Host.ViewManager.ConfigureSingleFullView();

    /// <inheritdoc />
    protected override void OnSceneBound() => RenderSurface.Host.Backbuffer.ClearColor = HudPainter.Night;

    /// <inheritdoc />
    protected override void CreateDirectDrawings()
    {
        var host = RenderSurface.Host;
        _drawing = new DrawListDrawing(host, host.ViewManager.Views[0], new Rectangle(0, 0, RenderWidth, RenderHeight), _hud, "goddesstemple-table")
        {
            ZOrder = 100,
        };
    }

    /// <inheritdoc />
    protected override void OnEngineInitialized()
    {
        Engine.Configuration.TargetFPS = 60;
        Engine.Configuration.FixedUpdateRate = 60;
        _input = new InputActionMap(new InputBindingProfile("GoddessTemple")
            .Bind(ActionRoll, Key(VirtualKey.Space), Key(VirtualKey.Enter))
            .Bind(ActionClose, Key(VirtualKey.Escape))
            .Bind(ActionJournal, Key(VirtualKey.J))
            .Bind(ActionReroll, Key(VirtualKey.R))
            .Bind(ActionDig + "1", Key(VirtualKey.Number1))
            .Bind(ActionDig + "2", Key(VirtualKey.Number2))
            .Bind(ActionDig + "3", Key(VirtualKey.Number3))
            .Bind(ActionDig + "4", Key(VirtualKey.Number4))
            .Bind(ActionDig + "5", Key(VirtualKey.Number5)));
        _input.Attach(Engine);

        _table.CardClicked += OnCardClicked;
        _table.DieClicked += OnDieClicked;
        _table.InspectRequested += OnInspectRequested;
        ApplySettings();
        Engine.CPSCalculated += OnCpsCalculated;
    }

    /// <inheritdoc />
    protected override void OnFixedUpdate(FixedUpdateStep step)
    {
        if (_table == null)
        {
            return;
        }

        Interlocked.Increment(ref _stepCount);
        var dt = step.DeltaSeconds;
        Resize();
        Prepare();
        BeginPendingGame();
        _input?.Update(dt);
        Mouse();
        Keys();
        _session?.Update(dt);
        Pace(dt);
    }

    /// <inheritdoc />
    protected override void OnAfterFixedUpdates(int stepCount)
    {
        if (_painter == null)
        {
            return;
        }

        _hud.Clear();
        _frame.IsHumanTurn = IsHumanTurn();
        _frame.IsGameOver = _session?.Engine.State.IsGameOver ?? false;
        _frame.Preparation = PreparationProgress();
        _painter.Paint(_hud, _table, _cardPreparation == null ? _session : null, _controls, _frame, _tableWidth);
        _hud.Publish();
        AutoPlayScreenshot();
    }

    /// <inheritdoc />
    protected override void UnhookEvents()
    {
        Engine.CPSCalculated -= OnCpsCalculated;
        _input?.Detach();
        if (Engine.Input.MouseEventPoller != null)
        {
            Engine.Input.MouseEventPoller.MouseEvent -= OnMouseEvent;
        }

        if (_table != null)
        {
            _table.CardClicked -= OnCardClicked;
            _table.DieClicked -= OnDieClicked;
            _table.InspectRequested -= OnInspectRequested;
        }
    }

    /// <inheritdoc />
    protected override void OnDisposed()
    {
        _drawing?.Dispose();
        _deco?.Dispose();
        _table?.Dispose();
        _fonts?.Dispose();
    }

    #endregion CodeBrixGameHost overrides

    private static InputBinding Key(VirtualKey key) => InputBinding.Key((int)key, key.ToString());

    private void Post(Action action)
    {
        try
        {
            Engine.EngineDispatcher.Post(action);
        }
        catch (Exception)
        {
            action();
        }
    }

    // ------------------------------------------------------------------ preparation and new games

    private void Resize()
    {
        var canvas = RenderSurface;
        if (canvas.ActualHeight <= 0 || canvas.ActualWidth <= 0)
        {
            return;
        }

        var width = Math.Max(RenderWidth, (int)Math.Round(RenderHeight * canvas.ActualWidth / canvas.ActualHeight));
        if (width == _tableWidth)
        {
            return;
        }

        _tableWidth = width;
        _table.CancelDrag();
        canvas.SetRenderResolution(width, RenderHeight);
        if (_drawing != null)
        {
            _drawing.ScreenBounds = new Rectangle(0, 0, width, RenderHeight);
        }

        _session?.Relayout(width);
    }

    //The faces are composed on a worker; the add-on rasterizes each one as it is registered, so they are registered
    //  a few at a time on the engine thread, within a time budget per step, while the progress bar fills
    private void Prepare()
    {
        if (_faces == null)
        {
            if (_facesTask == null || !_facesTask.IsCompleted)
            {
                return;
            }

            if (_facesTask.IsFaulted)
            {
                GameLog.Write($"art preparation FAILED: {_facesTask.Exception?.GetBaseException().Message}");
                _facesTask = null;
                return;
            }

            _faces = _facesTask.Result;
            _pendingFaces = _faces.Pending(_artwork);
            GameLog.Write($"art preparation: {_faces.Faces.Count} faces composed in {_preparationClock.Elapsed.TotalSeconds:0.00} s");
        }

        if (_registeredFaces >= _pendingFaces.Count)
        {
            return;
        }

        var budget = Stopwatch.StartNew();
        while (_registeredFaces < _pendingFaces.Count && budget.Elapsed.TotalMilliseconds < RegistrationBudgetMs)
        {
            var key = _pendingFaces[_registeredFaces++];
            _artwork.Register(key, _faces.Faces[key]);
        }

        if (_registeredFaces >= _pendingFaces.Count)
        {
            GameLog.Write($"art preparation: {_pendingFaces.Count} faces registered on the table, " +
                          $"{_preparationClock.Elapsed.TotalSeconds:0.00} s in all");
        }
    }

    private bool FacesReady => _faces != null && _registeredFaces >= _pendingFaces.Count;

    private double PreparationProgress()
    {
        if (_pendingSetup == null && _cardPreparation == null)
        {
            return -1;
        }

        if (!FacesReady)
        {
            return _pendingFaces == null || _pendingFaces.Count == 0 ? 0 : 0.9 * _registeredFaces / _pendingFaces.Count;
        }

        return _cardPreparation == null ? 0.9 : 0.9 + (0.1 * _cardPreparation.Progress);
    }

    private void BeginPendingGame()
    {
        if (_cardPreparation != null)
        {
            if (!_cardPreparation.IsComplete)
            {
                return;
            }

            if (_cardPreparation.Error != null)
            {
                GameLog.Write($"card preparation error: {_cardPreparation.Error.Message}");
            }

            _cardPreparation = null;
            _session.Start();
            _clock.Restart();
            GameLog.Write($"game started: {string.Join(", ", _session.Engine.State.Teams.Select(t => t.Name + " (" + t.Kind + ")"))}, " +
                          $"seed {_session.Engine.State.Seed}");
            PushTeams();
            return;
        }

        if (_pendingSetup == null || !FacesReady)
        {
            return;
        }

        var setup = _pendingSetup;
        _pendingSetup = null;
        _session?.Queue.Clear();
        _controls.Reset();
        _inspectorOpen = false;
        _gameEndHandled = false;
        _idleSeconds = 0;
        if (_session != null)
        {
            _session.EventPresented -= OnEventPresented;
        }

        var engine = new GameEngine(setup);
        _gameSeed = engine.State.Seed;
        _hasGame = true;
        _brainRandom = new Random(engine.State.Seed);
        _session = new TableSession(engine, _artwork, null, _tableWidth);
        _session.EventPresented += OnEventPresented;
        _frame.Ticker = Narrator.Dateline + "The expeditions arrive at Warka.";
        _frame.Prompt = string.Empty;

        //The backs (the add-on's celestial back) and anything not yet cached are prepared before the first deal
        var cards = _table.Areas.SelectMany(a => a.Pile.Cards).ToArray();
        _cardPreparation = _table.BeginPrepareCards(cards);
    }

    // ------------------------------------------------------------------ the computer teams

    private void Pace(double dt)
    {
        if (_inspectorOpen)
        {
            _inspectorSeconds += dt;
            if (_inspectorLimit > 0 && _inspectorSeconds > _inspectorLimit + 1.0)
            {
                //The page did not report the inspector closed (no page, or it stayed open under the pointer for long)
                _inspectorOpen = false;
            }
        }

        var session = _session;
        if (session == null || _cardPreparation != null || session.Engine.State.IsGameOver || session.IsBusy || _inspectorOpen)
        {
            _idleSeconds = 0;
            return;
        }

        var team = session.Engine.State.CurrentTeam;
        if (team == null || team.Kind != SeatKind.Computer)
        {
            return;
        }

        _idleSeconds += dt;
        var pause = _table.ReducedMotion ? 0.1 : ComputerPauseSeconds / Math.Max(0.25, _table.AnimationSpeed);
        if (_idleSeconds < pause)
        {
            return;
        }

        _idleSeconds = 0;
        Act(ComputerBrain.Choose(session.Engine, team.Temperament, _brainRandom));
    }

    private bool IsHumanTurn()
    {
        var state = _session?.Engine.State;
        return state != null && _cardPreparation == null && !state.IsGameOver && state.CurrentTeam?.Kind == SeatKind.Human;
    }

    // ------------------------------------------------------------------ applying actions

    private void Act(GameAction action)
    {
        var session = _session;
        if (session == null || action == null)
        {
            return;
        }

        var engine = session.Engine;
        var reason = engine.WhyIllegal(action);
        if (reason != null)
        {
            Prompt(reason);
            return;
        }

        var team = engine.State.CurrentTeam;
        var result = session.Apply(action);
        var line = Narrator.Describe(engine, team?.Name, result);
        _frame.Ticker = Narrator.Dateline + line;
        GameLog.Write($"action: {line}");
        _controls.Prune(engine);
        if (action is DigAction or PublishAction or StudyAction or SurveyAction or RecruitAction)
        {
            _controls.Mode = ControlMode.Normal;
        }

        var prompt = _frame.Ticker;
        _ui(() => _bridge?.PromptChanged(prompt));
        HumanPrompt();
    }

    private void Prompt(string text)
    {
        _frame.Prompt = text ?? string.Empty;
        var line = _frame.Prompt;
        _ui(() => _bridge?.PromptChanged(line));
    }

    private void HumanPrompt()
    {
        if (!IsHumanTurn())
        {
            _frame.Prompt = string.Empty;
            return;
        }

        var state = _session.Engine.State;
        _frame.Prompt = state.Phase switch
        {
            GamePhase.SeasonStart or GamePhase.AwaitRoll => "Your turn: roll the dice (Space).",
            GamePhase.TurnEnd => "Over the hand limit of seven: click a card in your hand to set it aside.",
            _ when _controls.Mode == ControlMode.Publish => "Click three or more finds in your hand, then Publish.",
            _ when _controls.Mode == ControlMode.Survey => "Click a trench of the Site Row to survey it.",
            _ => "Click a die (or both), then a trench to dig or a specialist to recruit. Keys 1-5 dig; Space ends the turn.",
        };
    }

    // ------------------------------------------------------------------ the presentation's events

    private void OnEventPresented(GameEvent gameEvent)
    {
        var session = _session;
        var state = session.Engine.State;
        switch (gameEvent)
        {
            case SeasonStarted started:
                GameLog.Write($"season {started.Season.Year}: {started.Season.Title} ({started.Season.Effect})");
                var index = state.Seasons.ToList().IndexOf(started.Season);
                var count = state.SeasonCount;
                _ui(() => _bridge?.SeasonChanged(started.Season, index, count));
                var seasonSeed = state.Seed;
                ShowCard(() => CardViews.For(started.Season, _faces, AutoCloseFor(false), seasonSeed), AutoCloseFor(false));
                break;
            case TurnStarted:
                _controls.Reset();
                HumanPrompt();
                PushTeams();
                break;
            case SiteExcavated excavated:
                var human = state.Teams.FirstOrDefault(t => t.Name == excavated.TeamName)?.Kind == SeatKind.Human;
                if (human || _revealComputer)
                {
                    var year = state.Season?.Year ?? string.Empty;
                    var close = AutoCloseFor(human);
                    var seed = state.Seed;
                    ShowCard(() => CardViews.For(excavated.Card, _faces, year, excavated.TeamName, "In the hand of " + excavated.TeamName, close, seed), close);
                }

                PushTeams();
                break;
            case FavorDrawn favor:
                var favored = state.Teams.FirstOrDefault(t => t.Name == favor.TeamName)?.Kind == SeatKind.Human;
                if (favored || _revealComputer)
                {
                    var year = state.Season?.Year ?? string.Empty;
                    var close = AutoCloseFor(false);
                    ShowCard(() => CardViews.For(favor.Card, _faces, year, favor.TeamName, close), close);
                }

                break;
            case JournalEntryAdded:
                var entries = state.Journal.ToArray();
                _ui(() => _bridge?.JournalChanged(entries));
                break;
            case GameEnded ended:
                OnGameEnded(ended);
                break;
            default:
                PushTeams();
                break;
        }
    }

    //A person's own discovery stays open until they close it; everything else closes itself (sooner in autoplay)
    private double AutoCloseFor(bool humanOwnFind) =>
        humanOwnFind ? 0 : _autoPlay ? AutoPlay.InspectorHoldSeconds : InspectorAutoCloseSeconds;

    private void ShowCard(Func<CardView> build, double autoClose)
    {
        if (_inspector == null)
        {
            return;
        }

        _inspectorOpen = true;
        _inspectorSeconds = 0;
        Rustle();
        _inspectorLimit = autoClose <= 0 ? 0 : autoClose + 30;
        Task.Run(build).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                GameLog.Write($"inspector: the card view could not be built: {task.Exception?.GetBaseException().Message}");
                NotifyInspectorClosed();
                return;
            }

            var view = task.Result;
            _ui(() => _inspector.ShowCard(view));
        }, TaskScheduler.Default);
    }

    private void PushTeams()
    {
        var session = _session;
        if (session == null || _bridge == null)
        {
            return;
        }

        var state = session.Engine.State;
        var scores = session.Engine.FinalScores();
        var teams = state.Teams.Select(t => new TeamView(
            t.Name,
            t.Profile.Colour,
            scores.FirstOrDefault(s => s.TeamName == t.Name)?.Total ?? 0,
            t.Workers,
            t.Specialists.Count,
            t.Reports.Count,
            t.Kind == SeatKind.Computer,
            !state.IsGameOver && t == state.CurrentTeam)).ToArray();
        _ui(() => _bridge.TeamsChanged(teams));
    }

    private void OnGameEnded(GameEnded ended)
    {
        if (_gameEndHandled)
        {
            return;
        }

        _gameEndHandled = true;
        var elapsed = _clock.Elapsed;
        foreach (var score in ended.Scores.OrderBy(s => s.Rank))
        {
            GameLog.Write(string.Format(CultureInfo.InvariantCulture,
                "final score: #{0} {1}: {2} (published {3}, unpublished half {4}, tablets {5}, sets {6}, star {7})",
                score.Rank, score.TeamName, score.Total, score.Published, score.UnpublishedHalf, score.Tablets, score.SetBonus, score.StarBonus));
        }

        _frame.Ticker = Narrator.Dateline + "FINAL EDITION: the war closes the dig, and the reports are tallied.";
        PushTeams();
        var scores = ended.Scores;
        _ui(() => _bridge?.GameEnded(scores));
        if (!_autoPlay)
        {
            return;
        }

        try
        {
            var state = _session.Engine.State;
            var pdf = new JournalPdfBuilder().Build(new JournalPdfInput(state.Journal.ToArray(), state.Teams.Select(t => t.Name).ToArray(), scores, DateTime.Now, state.Seed));
            var path = Path.Combine(Path.GetTempPath(), "GoddessTempleDiscovery-FieldJournal-autoplay.pdf");
            File.WriteAllBytes(path, pdf.Bytes);
            GameLog.Write($"autoplay: Field Journal written: {path} ({pdf.PageCount} pages, {pdf.Bytes.Length / 1024} KB)");
            GameLog.Write($"{AutoPlay.PassLine} in {elapsed.TotalSeconds:0.0} s ({elapsed:mm\\:ss}), {state.ActionCount} actions, seed {state.Seed}");
        }
        catch (Exception ex)
        {
            GameLog.Write($"autoplay: FAILED to write the Field Journal: {ex}");
        }

        Engine.Stop();
    }

    // ------------------------------------------------------------------ the human player's input

    private void OnMouseEvent(MouseEventArgs args) => _mouse.Enqueue(args);

    //HUD buttons act on the release, as controls do: a press arms the button under it, a release on that same button
    //  fires it, a release anywhere else disarms it. The release therefore reaches the canvas before any pane the
    //  button opens covers it, and the table's next click is a fresh press. While a pane covers the table (the view
    //  model says so through PanesOpen) the table and the HUD take no clicks at all.
    private void Mouse()
    {
        while (_mouse.TryDequeue(out var e))
        {
            var point = new Vector2(e.CurrentPosition.X, e.CurrentPosition.Y);
            _frame.PointerX = point.X;
            _frame.PointerY = point.Y;
            HoverSlot(point);
            if (_armedButton != null && e.LeftButtonJustReleased)
            {
                var armed = _armedButton;
                _armedButton = null;
                if (!_panesOpen && _hud.Published.HitTest(point.X, point.Y) is { } released && released.Id == armed)
                {
                    Button(armed);
                }

                continue;
            }

            if (_panesOpen)
            {
                _table.CancelDrag();
                continue;
            }

            if (e.LeftButtonJustPressed && _hud.Published.HitTest(point.X, point.Y) is { } hit)
            {
                _armedButton = hit.Id;
                continue;
            }

            _table.Pointer(point, e.LeftButtonJustPressed, e.LeftButtonJustReleased, e.RightButtonJustPressed);
        }
    }

    private void HoverSlot(Vector2 point)
    {
        _controls.HoverSlot = -1;
        if (_session == null)
        {
            return;
        }

        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            var c = _session.Layout.SiteSlotCentre(slot);
            if (Math.Abs(point.X - c.X) <= TableLayout.SiteCard.Width / 2 && Math.Abs(point.Y - c.Y) <= TableLayout.SiteCard.Height / 2)
            {
                _controls.HoverSlot = slot;
            }
        }
    }

    private void Keys()
    {
        if (_input == null)
        {
            return;
        }

        if (_input.WasPressed(ActionClose))
        {
            //Escape closes what is on top (the gallery's large view, a pane, the inspector), or leaves a mode
            _controls.Mode = ControlMode.Normal;
            _ui(() => _bridge?.PaneRequested("escape"));
        }

        if (_input.WasPressed(ActionJournal))
        {
            _ui(() => _bridge?.PaneRequested("journal"));
        }

        if (!IsHumanTurn() || _session.IsBusy)
        {
            return;
        }

        var engine = _session.Engine;
        if (_input.WasPressed(ActionRoll))
        {
            Act(engine.State.Phase is GamePhase.AwaitRoll or GamePhase.SeasonStart ? new RollAction() : new EndTurnAction());
        }

        if (_input.WasPressed(ActionReroll))
        {
            Act(_controls.BestReroll(engine));
        }

        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            if (_input.WasPressed(ActionDig + (slot + 1).ToString(CultureInfo.InvariantCulture)))
            {
                Dig(slot);
            }
        }
    }

    private void Button(string id)
    {
        switch (id)
        {
            case HudPainter.Ids.Journal:
                _ui(() => _bridge?.PaneRequested("journal"));
                return;
            case HudPainter.Ids.Settings:
                _ui(() => _bridge?.PaneRequested("settings"));
                return;
            case HudPainter.Ids.Gallery:
                _ui(() => _bridge?.PaneRequested("gallery"));
                return;
        }

        if (!IsHumanTurn() || _session.IsBusy)
        {
            return;
        }

        var engine = _session.Engine;
        switch (id)
        {
            case HudPainter.Ids.Roll:
                Act(new RollAction());
                break;
            case HudPainter.Ids.EndTurn:
                Act(new EndTurnAction());
                break;
            case HudPainter.Ids.Study:
                Act(_controls.BestStudy(engine));
                break;
            case HudPainter.Ids.Reroll:
                Act(_controls.BestReroll(engine));
                break;
            case HudPainter.Ids.Survey:
                _controls.Mode = ControlMode.Survey;
                HumanPrompt();
                break;
            case HudPainter.Ids.Publish:
                _controls.Mode = ControlMode.Publish;
                HumanPrompt();
                break;
            case HudPainter.Ids.Confirm:
                Act(new PublishAction(_controls.PublishSelection.ToArray()));
                break;
            case HudPainter.Ids.Cancel:
                _controls.Mode = ControlMode.Normal;
                HumanPrompt();
                break;
            case HudPainter.Ids.WorkersDown:
                _controls.Workers = Math.Max(0, _controls.Workers - 1);
                break;
            case HudPainter.Ids.WorkersUp:
                _controls.Workers = Math.Min(engine.State.CurrentTeam.Workers, _controls.Workers + 1);
                break;
        }
    }

    private void Dig(int slot)
    {
        var dig = _controls.BestDig(_session.Engine, slot);
        if (dig != null)
        {
            Act(dig);
            return;
        }

        var choice = _controls.Choice(_session.Engine);
        Prompt(choice == null
            ? "No die reaches that trench yet: add Workers or a Tablet, or try another."
            : _session.Engine.WhyIllegal(new DigAction(slot, choice.Value, _controls.Workers, _controls.TabletId)));
    }

    private void OnCardClicked(Card card)
    {
        if (!IsHumanTurn() || _session.IsBusy)
        {
            return;
        }

        var engine = _session.Engine;
        var data = TableSession.RulesCard(card);
        if (engine.State.Phase == GamePhase.TurnEnd)
        {
            if (_session.IsInHand(card))
            {
                Act(new DiscardAction(data is DiscoveryCard d ? d.Id : ((TabletCard)data).Id));
            }

            return;
        }

        var slot = _session.SiteSlotOf(card);
        if (slot >= 0)
        {
            if (_controls.Mode == ControlMode.Survey)
            {
                var survey = _controls.BestSurvey(engine, slot);
                if (survey == null)
                {
                    Prompt("No survey is open: spend a die, or wait for a free survey.");
                }
                else
                {
                    Act(survey);
                }
            }
            else
            {
                Dig(slot);
            }

            return;
        }

        var expedition = _session.ExpeditionSlotOf(card);
        if (expedition >= 0)
        {
            var recruit = _controls.BestRecruit(engine, expedition);
            if (recruit == null)
            {
                Prompt(string.Format(CultureInfo.InvariantCulture, "Recruiting needs a die of {0} or more (and room for a fourth Specialist at most).",
                    engine.RecruitCost(expedition)));
            }
            else
            {
                Act(recruit);
            }

            return;
        }

        if (_session.IsInHand(card))
        {
            switch (data)
            {
                case DiscoveryCard discovery when _controls.Mode == ControlMode.Publish:
                    _controls.TogglePublish(discovery.Id);
                    break;
                case TabletCard tablet:
                    _controls.TabletId = _controls.TabletId == tablet.Id ? null : tablet.Id;
                    break;
            }
        }
    }

    private void OnDieClicked(Die die)
    {
        //The add-on toggles the hold flag on a click; this game uses a click to choose a die, never to hold one
        die.IsHeld = false;
        if (!IsHumanTurn() || _session.IsBusy)
        {
            return;
        }

        var index = _session.DieIndexOf(die);
        if (index >= 0 && !_controls.ToggleDie(_session.Engine, index))
        {
            Prompt("That die is spent or set aside.");
        }
    }

    private void OnInspectRequested(Card card)
    {
        if (_faces == null || card == null || !card.IsFaceUp)
        {
            return;
        }

        var year = _session?.Engine.State.Season?.Year ?? string.Empty;
        var seed = _session?.Engine.State.Seed ?? 0;
        Func<CardView> build = TableSession.RulesCard(card) switch
        {
            DiscoveryCard discovery => () => CardViews.For(discovery, _faces, year, null, null, 0, seed),
            TabletCard tablet => () => CardViews.For(tablet, _faces, year, 0),
            SpecialistCard specialist => () => CardViews.For(specialist, _faces),
            FavorCard favor => () => CardViews.For(favor, _faces, year, null, 0),
            _ => null,
        };
        if (build != null && _inspector != null)
        {
            Rustle();
            Task.Run(build).ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    var view = task.Result;
                    _ui(() => _inspector.ShowCard(view));
                }
            }, TaskScheduler.Default);
        }
    }

    //Autoplay only: one backbuffer PNG of the table during a mid-game turn, dice rolled and the table settled
    private void AutoPlayScreenshot()
    {
        var state = _session?.Engine.State;
        if (_screenshotPath == null || state == null || _session.IsBusy || state.Phase != GamePhase.Spend
            || state.SeasonIndex < state.SeasonCount / 2)
        {
            _settledFrames = 0;
            return;
        }

        //The backbuffer shows a frame or two behind the draw list: wait until the table has stood still a while
        if (++_settledFrames < 20)
        {
            return;
        }

        var path = _screenshotPath;
        _screenshotPath = null;
        try
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllBytes(path, RenderSurface.Host.Backbuffer.ToByteArray());
            GameLog.Write($"autoplay: table screenshot written: {path} (season {state.Season?.Year}, {state.CurrentTeam?.Name})");
        }
        catch (Exception ex)
        {
            GameLog.Write($"autoplay: the table screenshot could not be written: {ex.Message}");
        }
    }

    //The newspaper's paper rustle: the add-on's own card-slide sound, played through the engine's audio
    private void Rustle()
    {
        const string key = "goddesstemple.paper-rustle";
        if (_table == null || !_table.SoundEnabled)
        {
            return;
        }

        try
        {
            if (!_rustleLoaded)
            {
                using var stream = AssetCatalog.Open("sounds/card-slide-1.ogg");
                AudioResourceManager.Instance.LoadFromStream(key, stream, ".ogg");
                _rustleLoaded = true;
            }

            AudioResourceManager.Instance.TryPlaySfx(key, 0.5f);
        }
        catch (Exception ex)
        {
            GameLog.Write($"sound: the paper rustle could not play ({ex.Message})");
            _rustleLoaded = true;
        }
    }

    private void OnCpsCalculated(CyclesPerSecondCalculatedEventArgs args)
    {
        if (Interlocked.Exchange(ref _tierLogged, 1) == 1)
        {
            return;
        }

        var tier = RenderSurface.RenderSurfaceAdapter is not CodeBrixPlatformGpuRenderSurfaceAdapter gpu
            ? "CPU (bitmap backbuffer)"
            : gpu.IsGpuInitialized switch
            {
                true => "GPU (offscreen OpenGL + readback)",
                false => "GPU requested, unavailable - CPU fallback",
                _ => "GPU (initializing)",
            };
        GameLog.Write($"render tier: {tier}, backbuffer {RenderSurface.Host.LogicalWidth}x{RenderSurface.Host.LogicalHeight}");
    }
}
