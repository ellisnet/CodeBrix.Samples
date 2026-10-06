using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Links;
using BrixInvaders.Game.Session;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;
using BrixInvaders.Music;
using BrixInvaders.ViewModels;
using BrixInvaders.Views;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.PlayTest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace BrixInvaders.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    // A store a returning player might have: none of these is the game's default.
    public const Difficulty SeededDifficulty = Difficulty.Ace;
    public const int SeededShipShape = 2;
    public const int SeededShipColour = 3;
    public const double SeededMasterVolume = 0.5;
    public const string SeededName = "ZED";
    public const long SeededScore = 4321;
    public const int SeededSector = 2;

    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"));
    public string SettingsDirectory => Path.Combine(DataDirectory, "settings");
    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public IManageGameCanvas Model => (IManageGameCanvas)View.DataContext;
    public BrixInvadersGameHost Host => Model.Host;
    public BrixInvadersGameHost FirstHost { get; private set; }
    public GameSession Session => Host?.Session;
    public RecordingMusicDirector Music { get; } = new();
    public RecordingLinkOpener Links { get; } = new();
    public ConcurrentQueue<string> Log { get; } = new();
    // Every screen change the session reported, in order.
    public ConcurrentQueue<(GameScreen From, GameScreen To)> ScreenChanges { get; } = new();
    public Locator Canvas => Application.Page.GetByType<GameSurfaceCanvas>();
    public GameSurfaceCanvas GameCanvas => (GameSurfaceCanvas)View.FindName("GameCanvas");

    public async ValueTask InitializeAsync()
    {
        // Robot games and the GPU tier stay out of the run; the head has no OpenGL either way.
        Environment.SetEnvironmentVariable(AutoPilot.Variable, null);
        Environment.SetEnvironmentVariable(BrixInvadersGameHost.CpuTierVariable, "1");
        // A throwaway store, opened before the app opens the player's own (which then does nothing).
        Directory.CreateDirectory(SettingsDirectory);
        SettingsService.Initialize(SettingsDirectory);
        SettingsService.Difficulty = SeededDifficulty;
        SettingsService.ShipShape = SeededShipShape;
        SettingsService.ShipColour = SeededShipColour;
        SettingsService.MasterVolume = SeededMasterVolume;
        SettingsService.LastName = SeededName;
        var table = new HighScoreTable();
        table.Insert(SeededDifficulty, SeededName, SeededScore, SeededSector);
        SettingsService.SaveHighScores(table, SeededDifficulty);
        GameLog.Sink = Log.Enqueue;

        PlayTestApp app = null;
        Application = await PlayTestApplication.LaunchAsync(() => app = new PlayTestApp(services => services
            .AddSingleton<IMusicDirector>(Music)
            .AddSingleton<IExternalLinkOpener>(Links)), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });
        View = await Application.EvaluateAsync(() => (MainPage)((Frame)app.Window.Content).Content);
        // The engine is process-wide and the page starts the game once, from the canvas's first layout.
        // Keep this page for the whole run and bring the game back to its title instead of building a new page.
        await Application.WaitForAsync(() => Session, session => session != null, description: "the started game session");
        await Application.EvaluateAsync(() =>
        {
            FirstHost = Host;
            Session.ScreenChanged += (from, to) => ScreenChanges.Enqueue((from, to));
        });
    }

    public async Task ResetAsync(ScreenOrientation? orientation)
    {
        await Application.SetOrientationAsync(orientation);
        // No test can move the game off its title (see README.md); attract mode is the one place it wanders to.
        if (await ReadAsync(() => Session.CurrentScreen) == GameScreen.Attract)
            await HoldClickAsync(() => Session.CurrentScreen, screen => screen == GameScreen.Title, "the title after a click");
        // The splash overlay hands over to the title by itself; the state machine's own limit is longer.
        await WaitForScreenAsync(GameScreen.Title, (float)(ScreenStateMachine.SplashMaxSeconds + 5) * 1000);
        // A click on the canvas leaves it without keyboard focus (see README.md); activating the window gives it back.
        await Application.EvaluateAsync(() => GameCanvas.Focus(FocusState.Programmatic));
    }

    public Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float? timeout = null) =>
        Application.WaitForAsync(probe, ready, timeout, description);

    public Task<T> ReadAsync<T>(Func<T> probe) => Application.EvaluateAsync(probe);

    public Task WaitForScreenAsync(GameScreen screen, float? timeout = null) =>
        WaitAsync(() => Session.CurrentScreen, current => current == screen, $"the {screen} screen", timeout);

    // The engine samples the mouse button once per engine cycle, so the button stays down until the game has seen it.
    public async Task HoldClickAsync<T>(Func<T> probe, Func<T, bool> ready, string description)
    {
        var box = await Canvas.BoundingBoxAsync();
        await Application.Page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));
        await Application.Page.Mouse.DownAsync();
        try { await WaitAsync(probe, ready, description); }
        finally { await Application.Page.Mouse.UpAsync(); }
    }

    public async ValueTask DisposeAsync()
    {
        GameLog.Sink = null;
        if (Application != null)
        {
            // The engine thread reads the settings every step: stop it before the store closes.
            await Task.Run(() => Engine.Instance.StopAndWait());
            await Application.DisposeAsync();
        }
        SettingsService.Shutdown();
        try { Directory.Delete(DataDirectory, true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    // App keeps its window protected; the tests need the page it navigated to, not a second one.
    private sealed class PlayTestApp : App
    {
        public PlayTestApp(Action<IServiceCollection> configureServices) : base(configureServices) { }
        public Window Window => MainWindow;
    }
}

// Stands in for the generated music: no model is loaded, every call the game makes is recorded.
public sealed class RecordingMusicDirector : IMusicDirector
{
    private readonly ConcurrentQueue<string> _calls = new();
    private int _starts;

    public IReadOnlyList<string> Calls => _calls.ToArray();
    public int Starts => _starts;
    public string ActiveSource => string.Empty;
    public void Start(MusicSettings settings, double musicVolume) => Interlocked.Increment(ref _starts);
    public void OnTitle() => _calls.Enqueue("Title");
    public void OnSector(int sector) => _calls.Enqueue($"Sector {sector}");
    public void OnBoss(int sector) => _calls.Enqueue($"Boss {sector}");
    public void OnGameOver() => _calls.Enqueue("GameOver");
    public void OnPause() => _calls.Enqueue("Pause");
    public void OnResume() => _calls.Enqueue("Resume");
    public void SetVolumes(double master, double music, double effects) => _calls.Enqueue($"Volumes {master:0.0} {music:0.0} {effects:0.0}");
    public void ApplySettings(MusicSettings settings, int sector, bool boss) => _calls.Enqueue($"Settings {settings.GeneratorName}");
    public void Stop() => _calls.Enqueue("Stop");
}

// Stands in for the browser launcher: records each link and reports that a browser took it.
public sealed class RecordingLinkOpener : IExternalLinkOpener
{
    private readonly ConcurrentQueue<string> _opened = new();

    public IReadOnlyList<string> Opened => _opened.ToArray();

    public Task<bool> Open(string url)
    {
        _opened.Enqueue(url);
        return Task.FromResult(true);
    }
}
