using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.PlayTest;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Services;
using GoddessTempleDiscovery.ViewModels;
using GoddessTempleDiscovery.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;
using GameLoop = CodeBrix.Platform.GameEngine.Engine;

namespace GoddessTempleDiscovery.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    // A store a returning player might have: none of these is the game's default.
    public const bool SeededSound = false;
    public const bool SeededReducedMotion = true;
    public const bool SeededReveal = false;
    public const double SeededSpeed = 2.0;
    public const Difficulty SeededDifficulty = Difficulty.Easy;
    public static readonly SeatRecord[] SeededSeats =
    {
        new SeatRecord { TeamProfileId = "lapis-road-society", IsComputer = false, Temperament = nameof(Temperament.Surveyor) },
        new SeatRecord { TeamName = "Koldewey's Rivals", IsComputer = true, Temperament = nameof(Temperament.Scholar) },
        new SeatRecord { TeamProfileId = "morning-star-mission", IsComputer = true, Temperament = nameof(Temperament.DeepDigger) },
    };

    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"));
    public string SettingsDirectory => Path.Combine(DataDirectory, "settings");
    public string ExportDirectory => Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTest", "journals");
    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public GoddessTempleGameHost Host => Model.Host;
    public GoddessTempleGameHost FirstHost { get; private set; }
    public TableSession Session => Host?.Session;
    public RecordingJournalFileBridge Files { get; } = new();
    public ConcurrentQueue<string> Log { get; } = new();
    // Every engine event the table presented in the games the tests started, in order.
    public ConcurrentQueue<GameEvent> Events { get; } = new();
    public Locator Canvas => Application.Page.GetByType<GameSurfaceCanvas>();
    public GameSurfaceCanvas GameCanvas => (GameSurfaceCanvas)View.FindName("GameCanvas");

    public async ValueTask InitializeAsync()
    {
        // Robot games and the GPU tier stay out of the run; the head has no OpenGL either way.
        Environment.SetEnvironmentVariable(AutoPlay.Variable, null);
        Environment.SetEnvironmentVariable(GoddessTempleGameHost.CpuTierVariable, "1");
        // A throwaway store, opened before the app opens the player's own (which then does nothing).
        Directory.CreateDirectory(SettingsDirectory);
        Directory.CreateDirectory(ExportDirectory);
        SettingsService.Initialize(SettingsDirectory);
        SettingsService.SoundEnabled = SeededSound;
        SettingsService.ReducedMotion = SeededReducedMotion;
        SettingsService.RevealComputerDiscoveries = SeededReveal;
        SettingsService.AnimationSpeed = SeededSpeed;
        SettingsService.Difficulty = SeededDifficulty;
        SettingsService.SaveLastSeats(SeededSeats);
        GameLog.Sink = Log.Enqueue;

        Application = await PlayTestApplication.LaunchAsync(() => new App(services => services
            .AddSingleton<IJournalFileBridge>(Files)), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });
        View = await Application.EvaluateAsync(() => (MainPage)((Frame)Application.Window.Content).Content);
        // The engine is process-wide and the page starts the game host once, from the canvas's first layout.
        // Keep this page for the whole run and bring it back to its title instead of building a new page.
        await Application.WaitForAsync(() => Host, host => host != null, 30000, "the started game host");
        await Application.EvaluateAsync(() => FirstHost = Host);
    }

    public async Task ResetAsync(ScreenOrientation? orientation)
    {
        Application.FilePickers.Clear();
        Application.Launcher.Clear();
        Files.Clear();
        await Application.SetOrientationAsync(orientation);
        await Application.EvaluateAsync(() =>
        {
            if (Model.IsInspectorOpen) Model.CloseInspectorCommand.Execute(null);
            Model.IsJournalOpen = false;
            Model.IsSettingsOpen = false;
            Model.CloseGalleryCommand.Execute(null);
            Model.ShowTimeline = false;
            Model.TitleCommand.Execute(null);
        });
        await Assertions.Expect(Application.Page.GetByTestId("TitlePane")).ToBeVisibleAsync();
    }

    public Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float? timeout = null) =>
        Application.WaitForAsync(probe, ready, timeout, description);

    public Task<T> ReadAsync<T>(Func<T> probe) => Application.EvaluateAsync(probe);

    // Reads the game on the engine thread, where the host changes it.
    public async Task<T> OnEngineAsync<T>(Func<T> probe)
    {
        var result = default(T);
        await GameLoop.Instance.EngineDispatcher.PostAsync(() =>
        {
            result = probe();
            return Task.CompletedTask;
        });
        return result;
    }

    public async Task<T> WaitOnEngineAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float timeout = 20000)
    {
        var started = DateTime.UtcNow;
        var value = default(T);
        while ((DateTime.UtcNow - started).TotalMilliseconds < timeout)
        {
            value = await OnEngineAsync(probe);
            if (ready(value)) return value;
            await Task.Delay(40);
        }

        throw new TimeoutException($"Timed out after {timeout} ms waiting for {description}; last value: {value}");
    }

    // The engine samples its input once per fixed step: wait until it has run a few more.
    public async Task WaitStepsAsync(int steps = 3)
    {
        var start = Host.StepCount;
        await WaitAsync(() => Host.StepCount, now => now >= start + steps, $"{steps} engine steps");
    }

    // A table point (the 1280-or-wider by 800 render space) in screen pixels: the canvas scales the table to fit and
    // centres it.
    public async Task<(float X, float Y)> ToScreenAsync(Vector2 table)
    {
        var box = await Canvas.BoundingBoxAsync();
        var width = await OnEngineAsync(() => Session?.Layout.Width ?? TableLayout.BaseWidth);
        var scale = Math.Min(box.Width / width, box.Height / TableLayout.Height);
        var left = box.X + ((box.Width - (width * scale)) / 2);
        var top = box.Y + ((box.Height - (TableLayout.Height * scale)) / 2);
        return ((float)(left + (table.X * scale)), (float)(top + (table.Y * scale)));
    }

    // A click the game sees: the button stays down, then up, for whole engine steps each.
    public async Task ClickTableAsync(Vector2 table)
    {
        var (x, y) = await ToScreenAsync(table);
        var mouse = Application.Page.Mouse;
        await mouse.MoveAsync(x, y);
        await WaitStepsAsync(2);
        await mouse.DownAsync();
        try { await WaitStepsAsync(3); }
        finally { await mouse.UpAsync(); }
        await WaitStepsAsync(3);
    }

    public Task ClickHudButtonAsync(int index) => ClickTableAsync(new Vector2(Session.Layout.Button(index).MidX, Session.Layout.Button(index).MidY));

    // An element of the page by its automation id (call it on the UI thread, inside EvaluateAsync).
    public T Find<T>(string automationId) where T : DependencyObject =>
        Descendants(View).OfType<T>().First(e => AutomationProperties.GetAutomationId(e) == automationId);

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    public async ValueTask DisposeAsync()
    {
        GameLog.Sink = null;
        if (Application != null)
        {
            // The engine thread reads the settings every step: stop it before the store closes.
            await Task.Run(() => GameLoop.Instance.StopAndWait());
            await Application.DisposeAsync();
        }

        SettingsService.Shutdown();
        try { Directory.Delete(DataDirectory, true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

// Stands in for the save picker: every export is written into the test output, and every request is recorded.
public sealed class RecordingJournalFileBridge : IJournalFileBridge
{
    private readonly ConcurrentQueue<string> _requests = new();

    public RecordingJournalFileBridge() => PickSavePathAsync = Pick;

    public string NextPath { get; set; }
    public IReadOnlyList<string> Requests => _requests.ToArray();
    public Func<string, string, string, Task<string>> PickSavePathAsync { get; set; }

    public void Clear()
    {
        _requests.Clear();
        NextPath = null;
    }

    private Task<string> Pick(string suggestedFileName, string typeName, string extension)
    {
        _requests.Enqueue($"{suggestedFileName}|{typeName}|{extension}");
        return Task.FromResult(NextPath);
    }
}
