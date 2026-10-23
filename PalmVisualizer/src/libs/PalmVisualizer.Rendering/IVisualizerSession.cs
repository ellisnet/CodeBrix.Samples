using System;
using System.Collections.Generic;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The Visualize Mode scene's lifecycle as its consumer drives it: start it, park it while
/// the user is elsewhere, wake it again, feed it palms from any thread, and stop it when the
/// user picks another visualizer or the page closes. Obtained from
/// <see cref="IVisualizerSessionFactory"/>, so the view model that drives the scene never
/// constructs a view or a GPU context itself.
/// </summary>
public interface IVisualizerSession
{
    /// <summary>The visualizer this session shows.</summary>
    VisualizerKind Kind { get; }

    /// <summary>Indicates whether <see cref="Start"/> has run (and <see cref="Stop"/> has not).</summary>
    bool IsStarted { get; }

    /// <summary>
    /// Raised when the visualizer cannot render the way it was chosen to - no GPU context, so
    /// it fell back to the CPU, or no WebGPU device at all. Raised on the UI thread; the
    /// message is ready for the status line.
    /// </summary>
    event EventHandler<VisualizerProblemEventArgs> Problem;

    /// <summary>
    /// Puts the visualizer's view into the host's panel and starts rendering. Must be called
    /// on the UI thread; a session starts once - use <see cref="Pause"/> and
    /// <see cref="Resume"/> to leave and re-enter Visualize Mode.
    /// </summary>
    void Start();

    /// <summary>
    /// Parks the visual at ~zero cost while the user is elsewhere, releasing every palm
    /// attractor so a later <see cref="Resume"/> starts from the undisturbed visual. Safe to
    /// call when not started or already paused.
    /// </summary>
    void Pause();

    /// <summary>
    /// Wakes the visual after a <see cref="Pause"/>; the pause is invisible to the visual's
    /// clock, so the colors resume mid-motion. Safe to call when not started or not paused.
    /// </summary>
    void Resume();

    /// <summary>
    /// Sets the open palms currently attracting the visual. Positions are normalized 0..1
    /// across the visual (already mirrored by the caller when the user is watching a
    /// mirror-style view). Safe to call from any thread.
    /// </summary>
    /// <param name="palms">The attracting palms; empty (or null) releases them all.</param>
    void UpdatePalms(IReadOnlyList<PalmAttractor> palms);

    /// <summary>
    /// Stops rendering and takes the view out of the host's panel, releasing its GPU
    /// resources. Call on the UI thread when switching visualizers or closing the page.
    /// </summary>
    void Stop();
}

/// <summary>Provides data for the <see cref="IVisualizerSession.Problem"/> event.</summary>
public sealed class VisualizerProblemEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="VisualizerProblemEventArgs"/> class.</summary>
    /// <param name="message">What went wrong, worded for the status line.</param>
    public VisualizerProblemEventArgs(string message) => Message = message ?? string.Empty;

    /// <summary>What went wrong, worded for the status line.</summary>
    public string Message { get; }
}
