using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Credits;
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
using CodeBrix.Platform.GameEngine.Drawing.Direct;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using CodeBrix.Platform.GameEngine.Drawing.Direct.Particles;
using CodeBrix.Platform.GameEngine.Host.Hosting;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.GameEngine.Input.Actions;
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
/// Kenney zips, runs the <see cref="GameSession"/> on the engine's fixed-step hook (60 steps a second), and draws it.
/// </summary>
/// <remarks>
/// <para>
/// Rendering: the view model sets the tier before creating the host (GPU unless <c>BRIXINVADERS_USE_CPU=1</c>, see
/// <see cref="UseGpuTier"/>) and pins the render resolution to the 1280 x 720 playfield, which the canvas letterboxes
/// into the window. After each cycle's fixed steps the screens paint two engine draw lists (<see cref="FrameLists"/>)
/// and publish them; two <see cref="DrawListDrawing"/>s draw the published copies (the world under the engine's
/// particles, the HUD and menus over everything).
/// </para>
/// <para>
/// Input: an engine <see cref="InputActionMap"/> over <see cref="GameControls"/>, attached to the engine so it latches
/// every press between steps and claims the game's keys; each step reads it once.
/// </para>
/// <para>
/// Threads: the fixed steps (<see cref="OnFixedUpdate"/>), the frame build and the input polling run on the engine
/// thread; the window hook runs on the UI thread and only latches a pause request. Seams for the
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

    private const int PerformanceLogInterval = 10;

    private readonly IMusicDirector _music;
    private readonly IExternalLinkOpener _links;
    private readonly ICreditsContent _creditsOverride;
    private readonly InputActionMap _input = GameControls.Configure(new InputActionMap(GameControls.Classic));
    private readonly PlayfieldPainter _playfield = new PlayfieldPainter();
    private readonly ScreenDirector _director = new ScreenDirector();
    private readonly DrawImageLibrary _images = new DrawImageLibrary();
    private readonly EngineSoundOutput _sound = new EngineSoundOutput();
    private readonly AutoPilot _autoPilot = AutoPilot.IsRequested() ? new AutoPilot() : null;

    private IGameSettings _settings;
    private GameSession _session;
    private PaintContext _context;
    private SdlGamepadManager _gamepads;
    private string _gamepadStatus = string.Empty;
    private KenneyGameAssetProvider _provider;
    private FrameLists _frame;
    private SceneLayer _worldLayer;
    private ParticleEffects _particles;
    private SplashOverlay _splash;
    private InputDeviceKind _savedDevice;
    private int _tierLogged;
    private long _cpsSamples;
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

    /// <summary>The credit line of every Kenney pack that was read (for credits content); empty before loading.</summary>
    public IReadOnlyList<string> PackCredits => _provider?.CreditLines ?? Array.Empty<string>();

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
        var check = _provider.CheckKeys(AssetKeyCatalog.AllKeys);
        GameLog.Write($"assets: {check}");
        foreach (var key in check.MissingKeys)
        {
            GameLog.Write($"assets: WARNING: key not found: {key}");
        }

        var sounds = BrixInvadersAssets.LoadSounds(Engine);
        GameLog.Write($"sounds: {sounds.Resources.Count} effect(s) loaded, " +
                      $"{sounds.Resources.Values.Count(resource => resource.IsPreloaded)} preloaded for the voice pool");

        var fonts = BrixInvadersAssets.LoadFonts(Engine);
        _frame = new FrameLists(new DrawList(_images, 1024), new DrawList(_images), FontManager.Instance.Get(fonts.TextKey),
            FontManager.Instance.Get(fonts.ThinKey));
        GameLog.Write($"fonts: '{fonts.TextFamilyName}' and '{fonts.ThinFamilyName}' from the remastered pack");
    }

    /// <inheritdoc />
    protected override void LoadTilesheets()
    {
        //The library finds each atlas frame and loose picture through the Kenney provider (the promo card through the
        //  tilesheet registry) and logs any it cannot; each atlas frame is also kept under its "atlas#frame" catalog
        //  key, so the painters draw by that one key
        var resolved = 0;
        foreach (var key in SpriteCatalog.AllAtlasFrames)
        {
            if (SpriteCatalog.TrySplit(key, out var atlas, out var frame) && _images.Get(atlas, frame) is { } image)
            {
                _images.AddImage(key, null, image);
                resolved++;
            }
        }

        BrixInvadersAssets.LoadPromoCard(Engine);
        resolved += SpriteCatalog.LoosePictures.Append(SpriteCatalog.PromoCard).Count(picture => _images.Get(picture) != null);
        GameLog.Write($"pictures: {resolved} resolved from the atlases and the loose images, {_images.Missing.Count} missing");
    }

    /// <inheritdoc />
    protected override Scene CreateInitialScene()
    {
        var scene = new Scene();
        _worldLayer = scene.AddPixelLayer(RenderWidth, RenderHeight);
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

        _ = new DrawListDrawing(host, _worldLayer, bounds, _frame.World, "brixinvaders-world") { ZOrder = 0 };

        var surface = new ParticleSurface(host, _worldLayer, bounds, "brixinvaders-particles", maxParticles: 3000)
        {
            GravityX = 0f,
            GravityY = 0f,
            ZOrder = 50,
        };
        _particles = new ParticleEffects(surface);

        _ = new DrawListDrawing(host, view, bounds, _frame.Overlay, "brixinvaders-overlay") { ZOrder = 100 };
    }

    /// <inheritdoc />
    protected override void OnEngineInitialized()
    {
        Engine.Configuration.TargetFPS = 60;
        Engine.Configuration.FixedUpdateRate = (int)Math.Round(1 / Playfield.FixedStep);
        AudioResourceManager.Instance.SfxPool.CullPolicy = SfxCullPolicy.CullLowestPriority;

        _session = new GameSession(_settings, _sound, _music, _links, Environment.TickCount);
        _session.QuitRequested += () => QuitRequested?.Invoke();
        _session.ScreenChanged += OnScreenChanged;
        var credits = _creditsOverride ?? new KenneyCreditsContent(() => PackCredits);
        _context = new PaintContext(_session, _frame, _playfield, credits);
        _savedDevice = _settings.LastInputDevice;
        _input.LastDevice = _savedDevice;
        _input.Profile = GameControls.ProfileFor(_settings.GamepadProfile);

        //Polled every engine cycle: a tap shorter than a step is latched for the next step, and the active
        //  profile's keys are claimed (no app accelerator or focus move sees them while the canvas has focus)
        _input.Attach(Engine);
        _director.Settings.GamepadStatus = $"Gamepad: {_gamepadStatus}";
        _director.Follow(_session.CurrentScreen, _context);
        GameLog.Write($"screen: {_session.CurrentScreen}");
        if (_autoPilot != null)
        {
            GameLog.Write($"autopilot: on ({AutoPilot.Variable}=1) - it walks the menus and plays with the demo pilot");
        }

        Engine.CPSCalculated += OnCpsCalculated;
    }

    /// <inheritdoc />
    protected override void OnEngineStarted()
    {
        _sound.SetLevels(_settings.MasterVolume, _settings.EffectsVolume);
        _music.Start(_session.CreateMusicSettings(), _settings.MusicVolume);
        _music.SetVolumes(_settings.MasterVolume, _settings.MusicVolume, _settings.EffectsVolume);
    }

    /// <inheritdoc />
    protected override void OnInitialized() => ShowSplash();

    /// <summary>
    /// The window was minimized (the app attached <c>GameWindowLifecycle</c>): a pause request is latched so the game
    /// shows its own pause overlay when the player returns; the engine then pauses itself (loop and audio).
    /// </summary>
    protected override void OnWindowHidden()
    {
        _input.SimulatePress(GameControls.Pause);
        GameLog.Write("window: hidden - engine paused");
    }

    /// <inheritdoc />
    protected override void OnFixedUpdate(FixedUpdateStep step)
    {
        if (_session == null)
        {
            return;
        }

        _input.Update(step.DeltaSeconds);
        Step();
    }

    /// <inheritdoc />
    protected override void OnAfterFixedUpdates(int stepCount)
    {
        if (_session != null)
        {
            BuildFrame();
        }
    }

    /// <inheritdoc />
    protected override void UnhookEvents()
    {
        Engine.CPSCalculated -= OnCpsCalculated;
        _input.Detach();

        if (Engine.Input.MouseEventPoller != null)
        {
            Engine.Input.MouseEventPoller.MouseEvent -= OnMouseEvent;
        }

        _music.Stop();
        _splash?.Dispose();
        _splash = null;
    }

    #endregion CodeBrixGameHost overrides

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
            using var stream = SplashComposer.Compose(_images, _frame.Font, _frame.ThinFont);
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

        //The engine hands the mouse over already in render-resolution pixels, which are playfield pixels
        _clickX = args.CurrentPosition.X;
        _clickY = args.CurrentPosition.Y;
        Interlocked.Exchange(ref _clickPending, 1);
    }

    private void Step()
    {
        var screen = _session.CurrentScreen;
        if (screen == GameScreen.Title)
        {
            foreach (var binding in _input.PressedBindings)
            {
                GameLog.Write($"input: {binding} on the title");
            }
        }

        var clicked = Interlocked.Exchange(ref _clickPending, 0) == 1;
        if (clicked)
        {
            _input.NoteDeviceUsed(InputDeviceKind.KeyboardMouse);
            if (_frame.Overlay.Published.HitTest(_clickX, _clickY) is { } link)
            {
                _session.OpenLink(link.Id);
            }
        }

        _input.Profile = GameControls.ProfileFor(_settings.GamepadProfile);
        GameControls.SetMenuRepeat(_input, screen == GameScreen.HighScoreEntry ? GameControls.NameEntryRepeatInterval : GameControls.RepeatInterval);
        var menu = GameControls.ReadMenu(_input, clicked);
        var play = GameControls.ReadPlay(_input);
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
            _particles.Emit(_session.Events);
            _playfield.AddFlashes(_session.Events);
            _session.ClearEvents();
        }

        if (_splash != null && _session.CurrentScreen != GameScreen.Splash)
        {
            //The player skipped the splash (or it timed out in the machine): take the overlay down
            _splash.Dispose();
            _splash = null;
        }

        if (_input.LastDevice != _savedDevice)
        {
            _savedDevice = _input.LastDevice;
            _settings.LastInputDevice = _savedDevice;
        }
    }

    private void BuildFrame()
    {
        _frame.Clear();
        _context.Device = _input.LastDevice;
        _context.Profile = _settings.GamepadProfile;
        _director.Paint(_context);
        _frame.Publish();
    }

    private void OnCpsCalculated(CyclesPerSecondCalculatedEventArgs args)
    {
        if (++_cpsSamples % PerformanceLogInterval == 0)
        {
            GameLog.Write($"performance: {args.GpuFps ?? args.NetCPS:0} frames/s, {args.GrossCPS:0} cycles/s, " +
                          $"{_frame.World.Published.Commands.Count + _frame.Overlay.Published.Commands.Count} draw commands " +
                          $"on {_session?.CurrentScreen}");
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
