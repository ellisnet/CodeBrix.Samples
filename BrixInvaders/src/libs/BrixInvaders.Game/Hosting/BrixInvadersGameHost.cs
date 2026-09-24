using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Links;
using BrixInvaders.Game.Rendering;
using BrixInvaders.Game.Screens;
using BrixInvaders.Game.Session;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.Drawing.Coordinates;
using CodeBrix.Platform.GameEngine.Drawing.Direct;
using CodeBrix.Platform.GameEngine.Drawing.Direct.Particles;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.Host.Hosting;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.GameEngine.Input.Keyboard;
using CodeBrix.Platform.GameEngine.Input.Mouse;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using CodeBrix.Platform.GameEngine.Rendering.Text;
using CodeBrix.Platform.GameEngine.Scenes;
using CodeBrix.Platform.GameEngine.Sdl2;
using CodeBrix.Platform.GameEngine.Sdl2.Gamepad;
using CodeBrix.Platform.GameEngine.Timers;
using SkiaSharp;

namespace BrixInvaders.Game.Hosting;

/// <summary>
/// The BrixInvaders game host: starts the engine over the page's <see cref="GameSurfaceCanvas"/>, loads the five
/// Kenney zips, runs the <see cref="GameSession"/> at a fixed 60 steps a second from the engine loop, and draws it.
/// </summary>
/// <remarks>
/// <para>
/// Rendering: the view model sets the tier before creating the host (GPU unless <c>BRIXINVADERS_USE_CPU=1</c>, see
/// <see cref="UseGpuTier"/>) and pins the render resolution to the 1280 x 720 playfield, which the canvas letterboxes
/// into the window. Each step builds an immutable <see cref="RenderFrame"/>; two <see cref="CommandCanvas"/> drawings
/// draw it (the world under the engine's particles and boss health bar, the HUD and menus over everything).
/// </para>
/// <para>
/// Threads: everything here except <see cref="OnCpsCalculated"/> and the start-up hooks runs on the engine thread,
/// in the per-cycle <c>AfterBackgroundTasksExecute</c> handler (after the engine has refreshed input). Seams for the
/// music (<see cref="IMusicDirector"/>), the credits (<see cref="ICreditsContent"/>) and links
/// (<see cref="IExternalLinkOpener"/>) are passed in; the defaults are silent / Kenney-only / log-only.
/// </para>
/// </remarks>
public sealed class BrixInvadersGameHost : CodeBrixGameHost
{
    /// <summary>The render width the canvas is pinned to (the playfield width).</summary>
    public const int RenderWidth = (int)Playfield.Width;

    /// <summary>The render height the canvas is pinned to (the playfield height).</summary>
    public const int RenderHeight = (int)Playfield.Height;

    /// <summary>The environment variable that switches the game to the CPU render tier when set to 1.</summary>
    public const string CpuTierVariable = "BRIXINVADERS_USE_CPU";

    private const int LayerCellSize = 16;
    private const int MaxStepsPerCycle = 5;
    private const double MaxCycleSeconds = 0.25;
    private const int PerformanceLogInterval = 10;

    private readonly IMusicDirector _music;
    private readonly IExternalLinkOpener _links;
    private readonly ICreditsContent _creditsOverride;
    private readonly InputMapper _mapper = new InputMapper();
    private readonly FrameBuilder _frame = new FrameBuilder();
    private readonly PlayfieldPainter _playfield = new PlayfieldPainter();
    private readonly ScreenDirector _director = new ScreenDirector();
    private readonly ImageLibrary _images = new ImageLibrary();
    private readonly EngineSoundOutput _sound = new EngineSoundOutput();
    private readonly List<string> _packCredits = new List<string>();
    private readonly AutoPilot _autoPilot = AutoPilot.IsRequested() ? new AutoPilot() : null;

    private RenderFrame _published = RenderFrame.Empty;
    private IGameSettings _settings;
    private GameSession _session;
    private PaintContext _context;
    private SdlGamepadManager _gamepads;
    private string _gamepadStatus = string.Empty;
    private KenneyGameAssetProvider _provider;
    private SKTypeface _typeface;
    private SKTypeface _thinTypeface;
    private SceneLayer _worldLayer;
    private CommandCanvas _worldCanvas;
    private CommandCanvas _overlayCanvas;
    private ParticleEffects _particles;
    private BossHealthBar _bossBar;
    private SplashOverlay _splash;
    private InputDevice _savedDevice;
    private long _lastTick;
    private double _accumulator;
    private int _tierLogged;
    private long _cpsSamples;
    private bool _bossBarFailed;
    private int _clickPending;
    private double _clickX;
    private double _clickY;

    /// <summary>Creates the host.</summary>
    /// <param name="renderSurface">
    /// The canvas. Its <see cref="GameSurfaceCanvas.UseGpuRendering"/> flag and render resolution must already be set:
    /// the tier cannot change once the scene pipeline exists.
    /// </param>
    /// <param name="music">The music director seam; null for none (<see cref="SilentMusicDirector"/>).</param>
    /// <param name="links">The link opener seam; null to only log links (<see cref="LoggingLinkOpener"/>).</param>
    /// <param name="credits">The credits content seam; null for the Kenney-only default (<see cref="KenneyCreditsContent"/>).</param>
    public BrixInvadersGameHost(GameSurfaceCanvas renderSurface, IMusicDirector music = null, IExternalLinkOpener links = null,
        ICreditsContent credits = null)
        : base(renderSurface)
    {
        _music = music ?? new SilentMusicDirector();
        _links = links ?? new LoggingLinkOpener();
        _creditsOverride = credits;
    }

    /// <summary>Raised (on the engine thread) when the player chooses Quit on the title; the app closes itself.</summary>
    public event Action QuitRequested;

    /// <summary>The game session, once the host has initialized.</summary>
    public GameSession Session => _session;

    /// <summary>The credit line of every Kenney pack that was read (for credits content).</summary>
    public IReadOnlyList<string> PackCredits => _packCredits;

    /// <summary>The Kenney asset provider, once the zips are registered.</summary>
    public KenneyGameAssetProvider Provider => _provider;

    /// <summary>Whether the GPU render tier should be used: yes, unless <c>BRIXINVADERS_USE_CPU=1</c>.</summary>
    /// <returns>True for the GPU tier.</returns>
    public static bool UseGpuTier() => Environment.GetEnvironmentVariable(CpuTierVariable) != "1";

    /// <summary>Prepares a canvas before the host is created: render tier and the pinned render resolution.</summary>
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

    #region CodeBrixGameHost overrides

    /// <inheritdoc />
    protected override void OnInitializing()
    {
        GameLog.Write("starting");
        GameLog.Write($"settings store: {GameStartup.OpenSettingsStore()}");
        _settings = new StoredGameSettings();

        //Pin the output format BEFORE anything plays: effects preload at this rate and the generated music renders at it
        AudioSystem.Initialize(MusicSetup.RecommendedSampleRate, MusicSetup.RecommendedChannels);
        GameLog.Write($"audio: output pinned at {MusicSetup.RecommendedSampleRate} Hz, {MusicSetup.RecommendedChannels} channels");
    }

    /// <inheritdoc />
    protected override void OnConfigureGamepads()
    {
        _gamepads = Engine.InitializeSdlGamepadManager();
        _gamepadStatus = DescribeGamepads();
        GameLog.Write($"gamepad: {_gamepadStatus}");
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
        mouse.StartMonitoringMouse(trackMouseMovement: false);
    }

    /// <inheritdoc />
    protected override void LoadAssets()
    {
        _provider = BrixInvadersAssets.Register(Engine);
        foreach (var pack in _provider.Packs)
        {
            _packCredits.Add(KenneyPacks.PackCreditLine(pack));
        }

        AssetReport.LogAll(Engine);
        var sounds = BrixInvadersAssets.LoadSounds(Engine);
        GameLog.Write($"sounds: {sounds.Resources.Count} effect(s) loaded, " +
                      $"{sounds.Resources.Values.Count(resource => resource.IsPreloaded)} preloaded for the voice pool");

        var fonts = BrixInvadersAssets.LoadFonts(Engine);
        _typeface = FontManager.Instance.Get(fonts.TextKey);
        _thinTypeface = FontManager.Instance.Get(fonts.ThinKey);
        GameLog.Write($"fonts: '{fonts.TextFamilyName}' and '{fonts.ThinFamilyName}' from the remastered pack");
    }

    /// <inheritdoc />
    protected override void LoadTilesheets()
    {
        var frames = SpriteCatalog.AllAtlasFrames;
        _images.AddAtlasFrames(AssetKeys.Atlases.Main, BrixInvadersAssets.LoadMainAtlas(Engine), frames);
        _images.AddAtlasFrames(AssetKeys.Atlases.Extension, BrixInvadersAssets.LoadBossSheet(Engine), frames);
        foreach (var picture in SpriteCatalog.LoosePictures)
        {
            _images.AddPicture(picture, BrixInvadersAssets.LoadImage(Engine, picture));
        }

        _images.AddPicture(SpriteCatalog.PromoCard, BrixInvadersAssets.LoadPromoCard(Engine));
        GameLog.Write($"pictures: {_images.Count} resolved from the atlases and the loose images, {_images.Missing.Count} missing");
    }

    /// <inheritdoc />
    protected override Scene CreateInitialScene()
    {
        var scene = new Scene();
        _worldLayer = scene.AddLayer(
            columnCount: RenderWidth / LayerCellSize,
            rowCount: RenderHeight / LayerCellSize,
            width: LayerCellSize,
            height: LayerCellSize,
            zOrder: 0,
            parallax: 1f,
            coordinateSystem: CoordinateSystemTypes.Orthogonal);
        _worldLayer.ShowGridLines = false;
        return scene;
    }

    /// <inheritdoc />
    protected override void CreateInitialViews() => RenderSurface.Host.ViewManager.ConfigureSingleFullView();

    /// <inheritdoc />
    protected override void OnSceneBound()
    {
        RenderSurface.Host.Backbuffer.ClearColor = new SKColor(Palette.Space);
        if (RenderSurface.Host.ViewManager.Views.Count > 0)
        {
            RenderSurface.Host.ViewManager.Views[0].Camera.SnapTo(PointF.Empty);
        }
    }

    /// <inheritdoc />
    protected override void CreateDirectDrawings()
    {
        var host = RenderSurface.Host;
        var bounds = new Rectangle(0, 0, Math.Max(1, host.LogicalWidth), Math.Max(1, host.LogicalHeight));
        var view = host.ViewManager.Views[0];

        _worldCanvas = new CommandCanvas(host, _worldLayer, bounds, _images, _typeface, _thinTypeface, () => Volatile.Read(ref _published))
        {
            ZOrder = 0,
        };

        var surface = new ParticleSurface(host, _worldLayer, bounds, "brixinvaders-particles", maxParticles: 3000)
        {
            GravityX = 0f,
            GravityY = 0f,
            ZOrder = 50,
        };
        _particles = new ParticleEffects(surface);

        var anchorFrame = BrixInvadersAssets.GetFrame(BrixInvadersAssets.LoadBossSheet(Engine), AssetKeys.Bosses.Core1);
        _bossBar = new BossHealthBar(host, _worldLayer, anchorFrame);

        _overlayCanvas = new CommandCanvas(host, view, bounds, _images, _typeface, _thinTypeface, () => Volatile.Read(ref _published))
        {
            ZOrder = 100,
        };
    }

    /// <inheritdoc />
    protected override void OnEngineInitialized()
    {
        Engine.Configuration.TargetFPS = 60;
        AudioResourceManager.Instance.SfxPool.CullPolicy = SfxCullPolicy.CullLowestPriority;

        _session = new GameSession(_settings, _sound, _music, _links, Environment.TickCount);
        _session.QuitRequested += () => QuitRequested?.Invoke();
        _session.ScreenChanged += OnScreenChanged;
        var credits = _creditsOverride ?? new KenneyCreditsContent(() => _packCredits);
        _context = new PaintContext(_session, _frame, _playfield, credits, _packCredits);
        _savedDevice = _settings.LastInputDevice;
        _mapper.LastDevice = _savedDevice;
        _mapper.Profile = _settings.GamepadProfile;
        _director.Settings.GamepadStatus = $"Gamepad: {_gamepadStatus}";
        _director.Follow(_session.CurrentScreen, _context);
        GameLog.Write($"screen: {_session.CurrentScreen}");
        if (_autoPilot != null)
        {
            GameLog.Write($"autopilot: on ({AutoPilot.Variable}=1) - it walks the menus and plays with the demo pilot");
        }

        Engine.AfterBackgroundTasksExecute += OnCycle;
        Engine.CPSCalculated += OnCpsCalculated;
    }

    /// <inheritdoc />
    protected override void OnEngineStarted()
    {
        _sound.SetLevels(_settings.MasterVolume, _settings.EffectsVolume);
        _music.Start(_session.CreateMusicSettings(), _settings.MusicVolume);
        _music.SetVolumes(_settings.MasterVolume, _settings.MusicVolume, _settings.EffectsVolume);
        _lastTick = HighResTimer.GetCurrentTick();
    }

    /// <inheritdoc />
    protected override void OnInitialized() => ShowSplash();

    /// <inheritdoc />
    protected override void OnEngineResumed() => _lastTick = HighResTimer.GetCurrentTick();

    /// <summary>
    /// The window was minimized or otherwise hidden: the engine's global pause parks the loop and suspends the
    /// audio (music included), and a pause request is latched so the game shows its own pause overlay on the
    /// first cycle after the window comes back. Call on the UI thread; safe before the host has initialized.
    /// </summary>
    public void OnWindowHidden()
    {
        _mapper?.RequestPause();
        GameLog.Write("window: hidden - engine paused");
        Engine.Pause();
    }

    /// <summary>The window is visible again: the engine resumes; the pause overlay waits for the player.</summary>
    public void OnWindowShown()
    {
        GameLog.Write("window: shown - engine resumed");
        Engine.Resume();
    }

    /// <inheritdoc />
    protected override void UnhookEvents()
    {
        Engine.AfterBackgroundTasksExecute -= OnCycle;
        Engine.CPSCalculated -= OnCpsCalculated;
        if (Engine.Input.MouseEventPoller != null)
        {
            Engine.Input.MouseEventPoller.MouseEvent -= OnMouseEvent;
        }

        _music.Stop();
        _splash?.Dispose();
        _splash = null;
    }

    #endregion CodeBrixGameHost overrides

    private PlayfieldTransform Transform =>
        PlayfieldTransform.Fit(Math.Max(1, RenderSurface.Host.LogicalWidth), Math.Max(1, RenderSurface.Host.LogicalHeight));

    private string DescribeGamepads()
    {
        if (_gamepads == null || !_gamepads.IsAvailable)
        {
            return $"not available ({_gamepads?.UnavailableCause}): {_gamepads?.UnavailableReason ?? "no gamepad support"} - keyboard play is unaffected";
        }

        var connected = _gamepads.ConnectedAdapters;
        return connected.Count == 0
            ? $"available, no controller connected ({_gamepads.GetNoControllersHint() ?? "plug one in at any time"})"
            : $"available, {connected.Count} connected: {string.Join(", ", connected.Select(pad => pad.Name))}";
    }

    private void ShowSplash()
    {
        var host = RenderSurface.Host;
        if (host.ViewManager.Views.Count > 0)
        {
            using var stream = SplashComposer.Compose(_images, _typeface, _thinTypeface);
            _splash = SplashOverlay.TryCreate(stream, host, host.ViewManager.Views[0], 0.6f, 2.6f, 0.6f,
                onSplashCompleted: OnSplashCompleted, nickname: "brixinvaders-splash");
        }

        if (_splash == null)
        {
            GameLog.Write("splash: could not be shown; going straight to the title");
            Engine.EngineDispatcher.Post(OnSplashCompleted);
        }
        else
        {
            GameLog.Write($"splash: composed {SplashComposer.Width}x{SplashComposer.Height} from a Kenney planet, the fleet and the Kenney future font");
        }
    }

    private void OnSplashCompleted()
    {
        _splash = null;
        _session?.NotifySplashComplete();
    }

    private void OnScreenChanged(GameScreen from, GameScreen to)
    {
        _director.Follow(to, _context);
        if (to is GameScreen.Playing && from == GameScreen.SectorBriefing)
        {
            _playfield.ClearFlashes();
        }
    }

    private void OnMouseEvent(MouseEventArgs args)
    {
        if (!args.IsButtonJustPressed(MouseButton.Left))
        {
            return;
        }

        var transform = Transform;
        _clickX = transform.ToWorldX(args.CurrentPosition.X);
        _clickY = transform.ToWorldY(args.CurrentPosition.Y);
        Interlocked.Exchange(ref _clickPending, 1);
    }

    private void OnCycle()
    {
        if (_session == null)
        {
            return;
        }

        var now = HighResTimer.GetCurrentTick();
        var elapsed = Math.Clamp(HighResTimer.GetDuration(_lastTick, now), 0, MaxCycleSeconds);
        _lastTick = now;

        _mapper.Sample(ReadRawInput(), elapsed);
        if (Interlocked.Exchange(ref _clickPending, 0) == 1)
        {
            _mapper.RegisterClick(_clickX, _clickY);
        }

        _accumulator += elapsed;
        var steps = 0;
        while (_accumulator >= Playfield.FixedStep && steps < MaxStepsPerCycle)
        {
            Step();
            _accumulator -= Playfield.FixedStep;
            steps++;
        }

        if (steps == MaxStepsPerCycle)
        {
            _accumulator = Math.Min(_accumulator, Playfield.FixedStep);
        }

        if (steps > 0)
        {
            BuildFrame();
        }
    }

    private void Step()
    {
        var screen = _session.CurrentScreen;
        var names = _mapper.TakePressedNames();
        if (screen == GameScreen.Title)
        {
            foreach (var name in names)
            {
                GameLog.Write($"input: {name} on the title");
            }
        }

        if (_mapper.TryTakeClick(out var clickX, out var clickY))
        {
            var url = Volatile.Read(ref _published).HitTest(clickX, clickY);
            if (url != null)
            {
                _session.OpenLink(url);
            }
        }

        _mapper.Profile = _settings.GamepadProfile;
        _mapper.MenuRepeatInterval = screen == GameScreen.HighScoreEntry ? InputMapper.NameEntryRepeatInterval : InputMapper.RepeatInterval;
        var menu = _mapper.TakeMenuInput();
        var play = _mapper.GameInput;
        if (_autoPilot != null)
        {
            menu = menu.HasAnyInput ? menu : _autoPilot.Menu(_session);
            play = play.HasAnyInput ? play : _autoPilot.Play(_session);
        }

        _session.Update(Playfield.FixedStep, menu, play);
        _director.Update(Playfield.FixedStep);
        _playfield.Update(Playfield.FixedStep);

        if (_session.Events.Count > 0)
        {
            _particles.Emit(_session.Events, Transform);
            _playfield.AddFlashes(_session.Events);
            _session.ClearEvents();
        }

        var battle = _session.CurrentScreen switch
        {
            GameScreen.Playing or GameScreen.Paused => _session.Game,
            GameScreen.Attract => _session.Attract,
            _ => null,
        };
        SyncBossBar(battle);

        if (_splash != null && _session.CurrentScreen != GameScreen.Splash)
        {
            //The player skipped the splash (or it timed out in the machine): take the overlay down
            _splash.Dispose();
            _splash = null;
        }

        if (_mapper.LastDevice != _savedDevice)
        {
            _savedDevice = _mapper.LastDevice;
            _settings.LastInputDevice = _savedDevice;
        }
    }

    private void SyncBossBar(GameSimulation battle)
    {
        if (_bossBarFailed)
        {
            return;
        }

        try
        {
            _bossBar.Sync(battle, Transform);
        }
        catch (Exception failure)
        {
            //The HUD draws its own boss bar too, so losing the engine bar costs a decoration, never the game
            _bossBarFailed = true;
            GameLog.Write($"WARNING: boss health bar disabled: {failure.GetType().Name}: {failure.Message}");
        }
    }

    private void BuildFrame()
    {
        _frame.Clear();
        _context.Device = _mapper.LastDevice;
        _context.Profile = _mapper.Profile;
        _director.Paint(_context);
        Volatile.Write(ref _published, _frame.Build());
    }

    private RawInput ReadRawInput()
    {
        var keys = InputKeys.None;
        var keyboard = Engine.Input.KeyboardEventPoller?.Adapter;
        if (keyboard != null)
        {
            foreach (var (key, flag) in KeyBindings.Keys)
            {
                if (keyboard.IsDown((int)key))
                {
                    keys |= flag;
                }
            }
        }

        var buttons = PadButtons.None;
        double stickX = 0;
        double stickY = 0;
        if (_gamepads != null && _gamepads.IsAvailable)
        {
            foreach (var pad in _gamepads.ConnectedAdapters)
            {
                var pressed = pad.PressedButtons;
                foreach (var (button, flag) in KeyBindings.Buttons)
                {
                    if (pressed.Contains(button))
                    {
                        buttons |= flag;
                    }
                }

                if (pad.LeftStick is { } stick && stick.Magnitude > Math.Sqrt((stickX * stickX) + (stickY * stickY)))
                {
                    stickX = stick.X;
                    stickY = stick.Y;
                }
            }
        }

        return new RawInput(keys, buttons, stickX, stickY);
    }

    private void OnCpsCalculated(CyclesPerSecondCalculatedEventArgs args)
    {
        if (++_cpsSamples % PerformanceLogInterval == 0)
        {
            var frame = Volatile.Read(ref _published);
            GameLog.Write($"performance: {args.GpuFps ?? args.NetCPS:0} frames/s, {args.GrossCPS:0} cycles/s, " +
                          $"{frame.World.Count + frame.Overlay.Count} draw commands on {_session?.CurrentScreen}");
        }

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
