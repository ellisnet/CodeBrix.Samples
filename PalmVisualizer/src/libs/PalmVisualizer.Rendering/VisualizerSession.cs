using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.UI.Xaml;

namespace PalmVisualizer.Rendering;

/// <summary>
/// What every visualizer session shares, whichever visual it shows and whichever graphics API
/// draws it: the lifecycle <see cref="IVisualizerSession"/> declares, the palm attractors, and
/// the visual's clock - which stops while the session is paused, so the pause is invisible to
/// the visual. A subclass supplies the view that draws the visual and pauses it.
/// </summary>
public abstract class VisualizerSession : IVisualizerSession
{
    private readonly IVisualizerCanvasHost _host;
    private readonly Stopwatch _clock = new();
    private FrameworkElement _view;
    private bool _isPaused;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualizerSession"/> class.
    /// </summary>
    /// <param name="kind">The visualizer this session shows.</param>
    /// <param name="host">The page whose panel the view is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    protected VisualizerSession(VisualizerKind kind, IVisualizerCanvasHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        if (host.Container == null) { throw new ArgumentNullException(nameof(host), "The host has no panel to show the visualizer in."); }

        Kind = kind;
    }

    /// <inheritdoc />
    public VisualizerKind Kind { get; }

    /// <inheritdoc />
    public bool IsStarted { get; private set; }

    /// <inheritdoc />
    public event EventHandler<VisualizerProblemEventArgs> Problem;

    /// <summary>The palm attractors the visual reads; thread-safe.</summary>
    protected PalmAttractorField AttractorField { get; } = new();

    /// <summary>The name the dropdown shows for this visualizer, for status-line messages.</summary>
    protected string DisplayName => VisualizerOption.For(Kind).DisplayName;

    /// <summary>The visual's clock, in seconds; it does not advance while the session is paused.</summary>
    protected float ElapsedSeconds => (float)_clock.Elapsed.TotalSeconds;

    /// <inheritdoc />
    public void Start()
    {
        if (IsStarted) { return; }

        _view = CreateView();
        _host.Container.Children.Add(_view);
        _isPaused = false;
        _clock.Start();
        IsStarted = true;
    }

    /// <inheritdoc />
    public void Pause()
    {
        if (!IsStarted || _isPaused) { return; }

        _isPaused = true;
        AttractorField.Reset();
        _clock.Stop();
        SetViewPaused(true);
    }

    /// <inheritdoc />
    public void Resume()
    {
        if (!IsStarted || !_isPaused) { return; }

        _isPaused = false;
        _clock.Start();
        SetViewPaused(false);
    }

    /// <inheritdoc />
    public void UpdatePalms(IReadOnlyList<PalmAttractor> palms) => AttractorField.SetTargets(palms);

    /// <inheritdoc />
    public void Stop()
    {
        if (!IsStarted) { return; }

        //Cleared first, so whatever the view raises while it unloads (a WebGPU view reports its
        //  device going away) is recognized as the stop rather than a problem
        IsStarted = false;
        _clock.Stop();

        //Leaving the visual tree is what releases the view's GPU context or device
        _host.Container.Children.Remove(_view);
        _view = null;
        OnStopped();
    }

    /// <summary>Creates the view that draws the visual; called once, by <see cref="Start"/>.</summary>
    /// <returns>The view, which the session puts into the host's panel.</returns>
    protected abstract FrameworkElement CreateView();

    /// <summary>Pauses or resumes the view's frames.</summary>
    /// <param name="paused"><see langword="true"/> to stop drawing frames; <see langword="false"/> to resume.</param>
    protected abstract void SetViewPaused(bool paused);

    /// <summary>Releases what the subclass created, after the view has left the panel.</summary>
    protected virtual void OnStopped() { }

    /// <summary>Raises <see cref="Problem"/> - only while the session is running.</summary>
    /// <param name="message">What went wrong, worded for the status line.</param>
    protected void ReportProblem(string message)
    {
        if (IsStarted)
        {
            Problem?.Invoke(this, new VisualizerProblemEventArgs(message));
        }
    }
}
