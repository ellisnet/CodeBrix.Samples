using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.GameEngine.Rendering;
using CodeBrix.Platform.GameEngine.Rendering.Backbuffers;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The Visualize Mode scene: runs the game engine (Tier B GPU rendering by default, so the
/// SkSL shader executes on the GPU) into a <see cref="GameSurfaceCanvas"/> and fills it with
/// the palm-reactive <see cref="EtherealBackdrop"/> - no stats overlay, just the moving
/// color. The hosting application feeds palm positions in through
/// <see cref="UpdatePalms"/> (from any thread) and pauses the whole visual with
/// <see cref="Pause"/>/<see cref="Resume"/> while the user is back at Camera Mode.
/// Set the environment variable <c>PALMVISUALIZER_USE_CPU=1</c> to run the identical scene
/// on the Tier A (CPU) render path.
/// </summary>
public sealed class VisualizerSession
{
    private readonly GameSurfaceCanvas _canvas;
    private readonly PalmAttractorField _attractorField = new PalmAttractorField();

    private RenderSurfaceHost<BackbufferBase> _renderSurface;
    private EtherealBackdrop _backdrop;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualizerSession"/> class.
    /// </summary>
    /// <param name="canvas">The render surface to draw into.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
    public VisualizerSession(GameSurfaceCanvas canvas)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    }

    /// <summary>Indicates whether <see cref="Start"/> has run.</summary>
    public bool IsStarted { get; private set; }

    /// <summary>
    /// Starts the engine and builds the shader scene. Must be called on the UI thread once
    /// the canvas has a non-zero size (its <c>FirstStarted</c> event), and only once per
    /// process - use <see cref="Pause"/>/<see cref="Resume"/> to leave and re-enter
    /// Visualize Mode.
    /// </summary>
    public void Start()
    {
        if (IsStarted) { return; }

        //Tier B (GPU) by default; must be chosen before the first access to Host. The
        //  render resolution tracks the window (no SetRenderResolution) - the shader
        //  scene is resolution-independent.
        _canvas.UseGpuRendering = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1";

        _renderSurface = _canvas.Host;
        _renderSurface.ViewManager.ConfigureSingleFullView();

        Engine.Instance.Start(SynchronizationContext.Current);
        Engine.Instance.Configuration.TargetFPS = 60;

        var adapter = _renderSurface.RenderSurfaceAdapter;
        var view = _renderSurface.ViewManager.Views[0];

        _backdrop = new EtherealBackdrop(_renderSurface, view,
            new Rectangle(0, 0, adapter.Width, adapter.Height), _attractorField);
        _backdrop.ZOrder = 0;

        //The render resolution tracks the window, so follow adapter resizes
        adapter.Resized += OnAdapterResized;

        IsStarted = true;
    }

    /// <summary>
    /// Parks the visual at ~zero cost (the global engine pause) while the user is back at
    /// Camera Mode, releasing every palm attractor so a later <see cref="Resume"/> starts
    /// from the undisturbed visual. Safe to call when not started or already paused.
    /// </summary>
    public void Pause()
    {
        if (!IsStarted || Engine.Instance.IsPaused) { return; }

        _attractorField.Reset();
        Engine.Instance.Pause();
    }

    /// <summary>
    /// Wakes the visual after a <see cref="Pause"/> - the pause is invisible to engine
    /// time, so the colors resume mid-motion. Safe to call when not started or not paused.
    /// </summary>
    public void Resume()
    {
        if (!IsStarted || !Engine.Instance.IsPaused) { return; }

        Engine.Instance.Resume();
    }

    /// <summary>
    /// Sets the open palms currently attracting the visual. Positions are normalized 0..1
    /// across the visual (already mirrored by the caller when the user is watching a
    /// mirror-style view); palms keep their <see cref="PalmAttractor.Id"/> from update to
    /// update so their influence follows them. Safe to call from any thread - tracking
    /// results are fed straight in from the vision worker.
    /// </summary>
    /// <param name="palms">The attracting palms; empty (or null) releases them all, letting
    /// the visual melt back to its undisturbed motion.</param>
    public void UpdatePalms(IReadOnlyList<PalmAttractor> palms) => _attractorField.SetTargets(palms);

    /// <summary>
    /// Stops the engine. Call when the hosting page is closing.
    /// </summary>
    public void Stop()
    {
        if (!IsStarted) { return; }

        _renderSurface.RenderSurfaceAdapter.Resized -= OnAdapterResized;
        Engine.Instance.Stop();
        IsStarted = false;
    }

    private void OnAdapterResized(RenderSurfaceAdapterResizedEventArgs args)
    {
        if (_backdrop != null)
            _backdrop.ScreenBounds = new Rectangle(0, 0, args.NewWidth, args.NewHeight);
    }
}
