using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.ViewModels;
using GoddessTempleDiscovery.Views;
using Microsoft.UI.Xaml.Controls;
using SilverAssertions;
using Xunit;
using GameLoop = CodeBrix.Platform.GameEngine.Engine;

namespace GoddessTempleDiscovery.PlayTests;

// The autoplay switch is read when the game host is created, and PlayTest runs one application per process: the smoke
// test therefore runs this test once more in a child process of its own, with GODDESSTEMPLE_AUTOPLAY=1 and a seed.
public sealed class AutoPlayFixture : IAsyncLifetime
{
    public const string ChildVariable = "GODDESSTEMPLE_PLAYTEST_AUTOPLAY_CHILD";
    public const string LogVariable = "GODDESSTEMPLE_PLAYTEST_AUTOPLAY_LOG";

    public static bool IsChild => Environment.GetEnvironmentVariable(ChildVariable) == "1";

    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", "autoplay-" + Guid.NewGuid().ToString("N"));
    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public ConcurrentQueue<string> Log { get; } = new();

    public async ValueTask InitializeAsync()
    {
        if (!IsChild) return;
        Environment.SetEnvironmentVariable(GoddessTempleGameHost.CpuTierVariable, "1");
        // A throwaway store with the quick table: no animation waits, no inspector for the computers' finds.
        Directory.CreateDirectory(DataDirectory);
        SettingsService.Initialize(DataDirectory);
        SettingsService.SoundEnabled = false;
        SettingsService.ReducedMotion = true;
        SettingsService.RevealComputerDiscoveries = false;
        var logPath = Environment.GetEnvironmentVariable(LogVariable);
        GameLog.Sink = line =>
        {
            Log.Enqueue(line);
            if (!string.IsNullOrEmpty(logPath)) File.AppendAllText(logPath, line + Environment.NewLine);
        };
        Application = await PlayTestApplication.LaunchAsync(() => new App(), new() { ConfigurationAssembly = typeof(AutoPlayFixture).Assembly });
        View = await Application.EvaluateAsync(() => (MainPage)((Frame)Application.Window.Content).Content);
    }

    public async ValueTask DisposeAsync()
    {
        GameLog.Sink = null;
        if (Application != null)
        {
            await Task.Run(() => GameLoop.Instance.StopAndWait());
            await Application.DisposeAsync();
            SettingsService.Shutdown();
        }

        try { Directory.Delete(DataDirectory, true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

public sealed class AutoPlayTests : IClassFixture<AutoPlayFixture>
{
    public const string SkipVariable = "GODDESSTEMPLE_PLAYTEST_SKIP_AUTOPLAY";
    private const int Seed = 1938;
    private const int ChildTimeoutMinutes = 20;

    private readonly AutoPlayFixture _fixture;

    public AutoPlayTests(AutoPlayFixture fixture) => _fixture = fixture;

    public static bool SkipAutoPlay => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(SkipVariable));

    [Fact(Skip = "GODDESSTEMPLE_PLAYTEST_SKIP_AUTOPLAY is set", SkipWhen = nameof(SkipAutoPlay))]
    public async Task Autoplay_plays_a_seeded_game_to_the_game_over_and_logs_the_pass_line()
    {
        if (AutoPlayFixture.IsChild)
        {
            await PlayInThisProcessAsync();
            return;
        }

        var log = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTest", $"autoplay-{Guid.NewGuid():N}.log");
        Directory.CreateDirectory(Path.GetDirectoryName(log));
        var (exitCode, output) = await RunChildAsync(log);
        exitCode.Should().Be(0, output);
        var lines = File.ReadAllLines(log);
        lines.Should().Contain(line => line.Contains($"autoplay: on ({AutoPlay.Variable}=1), seed {Seed}"));
        lines.Should().Contain(line => line.Contains("new game requested: 4 seats, 1 turn(s) a season") && line.EndsWith($"seed {Seed}"));
        lines.Count(line => line.Contains("final score: #")).Should().Be(4);
        lines.Should().Contain(line => line.Contains(AutoPlay.PassLine) && line.EndsWith($"seed {Seed}"));
    }

    // The child: the application starts with the autoplay switch on and plays its four computer teams to the end.
    private async Task PlayInThisProcessAsync()
    {
        var app = _fixture.Application;
        await app.WaitForAsync(() => _fixture.Model.Host?.Session, session => session != null, 120000, "the autoplay game");
        (await app.EvaluateAsync(() => _fixture.Model.Host.IsAutoPlay)).Should().BeTrue();
        await app.WaitForAsync(() => _fixture.Log.Any(line => line.Contains(AutoPlay.PassLine)), passed => passed,
            ChildTimeoutMinutes * 60000 - 120000, "the autoplay pass line");
        // The host stops the engine after the pass line, so the state is read on the UI thread.
        (await app.EvaluateAsync(() => _fixture.Model.Host.Session.Engine.State.IsGameOver)).Should().BeTrue();
        (await app.EvaluateAsync(() => _fixture.Model.Host.Session.Engine.State.Seed)).Should().Be(Seed);
        await app.WaitForAsync(() => _fixture.Model.EpilogueVisibility, v => v == Microsoft.UI.Xaml.Visibility.Visible, 30000, "the epilogue");
        (await app.EvaluateAsync(() => _fixture.Model.ScoreRows.Count)).Should().Be(4);
        File.Exists(Path.Combine(Path.GetTempPath(), "GoddessTempleDiscovery-FieldJournal-autoplay.pdf")).Should().BeTrue();
    }

    private static async Task<(int ExitCode, string Output)> RunChildAsync(string logPath)
    {
        var apphost = Path.Combine(AppContext.BaseDirectory, "GoddessTempleDiscovery.PlayTests" + (OperatingSystem.IsWindows() ? ".exe" : string.Empty));
        var dll = Path.Combine(AppContext.BaseDirectory, "GoddessTempleDiscovery.PlayTests.dll");
        var start = File.Exists(apphost) ? new ProcessStartInfo(apphost) : new ProcessStartInfo("dotnet") { ArgumentList = { "exec", dll } };
        start.ArgumentList.Add("--filter-method");
        start.ArgumentList.Add($"{typeof(AutoPlayTests).FullName}.{nameof(Autoplay_plays_a_seeded_game_to_the_game_over_and_logs_the_pass_line)}");
        start.WorkingDirectory = AppContext.BaseDirectory;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.UseShellExecute = false;
        start.Environment[AutoPlayFixture.ChildVariable] = "1";
        start.Environment[AutoPlayFixture.LogVariable] = logPath;
        start.Environment[AutoPlay.Variable] = "1";
        start.Environment[AutoPlay.SeedVariable] = Seed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        start.Environment["CODEBRIX_PLAYTEST_HEADED"] = null;

        using var child = Process.Start(start);
        var stdout = child.StandardOutput.ReadToEndAsync();
        var stderr = child.StandardError.ReadToEndAsync();
        var exited = child.WaitForExitAsync();
        if (await Task.WhenAny(exited, Task.Delay(TimeSpan.FromMinutes(ChildTimeoutMinutes))) != exited)
        {
            child.Kill(entireProcessTree: true);
            return (-1, "The autoplay child process timed out.\n" + await stdout + await stderr);
        }

        return (child.ExitCode, await stdout + await stderr);
    }
}
