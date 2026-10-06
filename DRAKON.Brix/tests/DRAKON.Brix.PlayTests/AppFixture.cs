using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using DRAKON.Brix.Drakon;
using DRAKON.Brix.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DRAKON.Brix.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private const float BootTimeout = 60000;
    private string _home;
    private string _open;
    // Every test page and the launch page quit into this queue, never into Environment.Exit.
    public ConcurrentQueue<int> QuitCodes { get; } = new();
    public ConcurrentQueue<string> Diagnostics { get; } = new();
    // DRAKON keeps drakon_editor.settings under HOME; HOME points here for the whole run.
    public string Home => Path.Combine(DataDirectory, "home");
    public string SettingsFile => Path.Combine(Home, "drakon_editor.settings");
    // Set before ResetAsync to boot the next page with DRAKONBRIX_OPEN pointing at this file.
    public string OpenOnReset { get; set; }
    // Set before ResetAsync to keep the settings (recent files, folders) for the next page.
    public bool KeepSettingsOnReset { get; set; }
    public RuntimeHost Runtime => View.RuntimeHost;
    internal TestApp LaunchApp { get; private set; }

    protected override Application CreateApplication() => LaunchApp = new TestApp(this);

    protected override void Prepare()
    {
        _home = Environment.GetEnvironmentVariable("HOME");
        _open = Environment.GetEnvironmentVariable("DRAKONBRIX_OPEN");
        Directory.CreateDirectory(Home);
        Environment.SetEnvironmentVariable("HOME", Home);
        Environment.SetEnvironmentVariable("DRAKONBRIX_OPEN", null);
    }

    protected override async Task BeforeResetAsync()
    {
        // Every reset boots a fresh interpreter; never start one while the previous boot is still running.
        var current = View?.RuntimeHost ?? LaunchApp.LaunchPage?.RuntimeHost;
        if (current != null)
            await Application.WaitForAsync(() => current.IsReady, ready => ready, BootTimeout, "previous DRAKON boot");
        if (!KeepSettingsOnReset) File.Delete(SettingsFile);
        KeepSettingsOnReset = false;
        Environment.SetEnvironmentVariable("DRAKONBRIX_OPEN", OpenOnReset);
        OpenOnReset = null;
        QuitCodes.Clear();
        Diagnostics.Clear();
    }

    protected override async Task AfterResetAsync()
    {
        var runtime = Runtime;
        await Application.EvaluateAsync(() => Attach(runtime));
        // The boot reads DRAKONBRIX_OPEN on its own thread; clear it only once the boot is done.
        await Application.WaitForAsync(() => runtime.IsReady, ready => ready, BootTimeout, "DRAKON boot");
        Environment.SetEnvironmentVariable("DRAKONBRIX_OPEN", null);
    }

    internal void Attach(RuntimeHost runtime)
    {
        runtime.QuitAction = code =>
        {
            QuitCodes.Enqueue(code);
            throw new QuitRequestedException(code);
        };
        runtime.Diagnostic += Diagnostics.Enqueue;
    }

    protected override void Cleanup()
    {
        Environment.SetEnvironmentVariable("HOME", _home);
        Environment.SetEnvironmentVariable("DRAKONBRIX_OPEN", _open);
    }

    // Evaluates Tcl on the current page's interpreter, after the work already queued on its Tcl thread.
    public Task<string> TclAsync(string script) =>
        Runtime.EvaluateForTestAsync(script).WaitAsync(TimeSpan.FromSeconds(30));

    public Task<string> WaitForTclAsync(string script, string expected) =>
        WaitForTclAsync(script, value => value == expected, $"Tcl [{script}] = {expected}");

    // Polls a Tcl script through PlayTest's own wait: each poll reads the last finished evaluation and queues the next.
    public Task<string> WaitForTclAsync(string script, Func<string, bool> predicate, string description = null)
    {
        Task<string> pending = null;
        string last = null;
        return Application.WaitForAsync(() =>
        {
            if (pending == null || pending.IsCompleted)
            {
                if (pending != null && pending.IsCompletedSuccessfully) last = pending.Result;
                pending = Runtime.EvaluateForTestAsync(script);
            }
            return last;
        }, value => value != null && predicate(value), description: description ?? $"Tcl [{script}]");
    }

    // A private copy of a shipped example diagram file; opening a .drn upgrades it in place.
    public string CopyExample(string name)
    {
        var folder = Path.Combine(DataDirectory, "files", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var copy = Path.Combine(folder, name);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Assets", "drakon", "examples", name), copy);
        return copy;
    }
}

// The real App; the page it builds at launch gets the fixture's quit action before its boot starts.
internal sealed class TestApp(AppFixture fixture) : App
{
    public MainPage LaunchPage { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        LaunchPage = (MainWindow.Content as Frame)?.Content as MainPage;
        if (LaunchPage != null) fixture.Attach(LaunchPage.RuntimeHost);
    }
}

// Thrown by the fixture's quit action so DRAKON's script stops at its exit, as a real exit would.
public sealed class QuitRequestedException(int code) : Exception("DRAKON asked to quit with code " + code + ".")
{
    public int Code { get; } = code;
}
