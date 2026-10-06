using System;
using System.Threading.Tasks;
using CodeBrix.Platform.TkCanvas.Hosting;

namespace DRAKON.Brix.Drakon;

/// <summary>
/// The application-facing owner of the DRAKON Tcl runtime. UI code holds one of
/// these and drives its <see cref="Start"/>/<see cref="Dispose"/> lifecycle, so
/// the code-behind never touches <see cref="DrakonRuntime"/> directly — the
/// application starts it one way, tests can drive it another.
/// </summary>
public sealed class RuntimeHost : IDisposable
{
    private DrakonRuntime _runtime;

    /// <summary>
    /// What DRAKON's <c>exit</c> (File &gt; Quit, the intro's Exit, its error
    /// paths) does with the exit code. Defaults to <c>Environment.Exit</c>, the
    /// way tclsh ends the process; read at the moment DRAKON quits, so it can be
    /// replaced at any time.
    /// </summary>
    public Action<int> QuitAction { get; set; } = code => Environment.Exit(code);

    /// <summary>
    /// True once the DRAKON Editor Tcl has been sourced and its first window
    /// (the intro, or the main window with a preloaded file) is up.
    /// </summary>
    public bool IsReady
    {
        get
        {
            DrakonRuntime runtime = _runtime;
            return runtime != null && runtime.IsReady;
        }
    }

    /// <summary>
    /// Raised with the runtime's diagnostic text (startup progress and
    /// failures, Tcl background errors), on the thread that produced it.
    /// </summary>
    public event Action<string> Diagnostic;

    /// <summary>
    /// Creates and starts the DRAKON runtime inside the given host view. Call
    /// once, from the UI thread, after the host has loaded (its tree and
    /// dispatcher exist). Subsequent calls are ignored.
    /// </summary>
    /// <param name="host">The loaded Tk host view.</param>
    public void Start(TkHostView host)
    {
        if (host == null) { throw new ArgumentNullException(nameof(host)); }
        if (_runtime != null) { return; }

        _runtime = new DrakonRuntime();
        _runtime.Diagnostic += OnDiagnostic;
        _runtime.Start(host, code =>
        {
            Action<int> quit = QuitAction;
            if (quit != null) { quit(code); }
        });
    }

    /// <summary>
    /// Test-only: evaluates a Tcl script on the running interpreter's Tcl thread
    /// and completes with its string result.
    /// </summary>
    /// <param name="script">The Tcl script to evaluate.</param>
    /// <returns>The script's string result.</returns>
    internal Task<string> EvaluateForTestAsync(string script)
    {
        DrakonRuntime runtime = _runtime;
        if (runtime == null) { throw new InvalidOperationException("The DRAKON runtime is not running."); }
        return runtime.PostScriptForTest(script);
    }

    private void OnDiagnostic(string message)
    {
        Action<string> handler = Diagnostic;
        if (handler != null) { handler(message); }
    }

    /// <summary>
    /// Stops the Tcl thread and disposes the runtime. Safe to call more than
    /// once, and safe to call when <see cref="Start"/> was never called.
    /// </summary>
    public void Dispose()
    {
        DrakonRuntime runtime = _runtime;
        _runtime = null;
        if (runtime != null) { runtime.Dispose(); }
    }
}
