using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using Microsoft.UI.Xaml;

namespace PalmVisualizer.Rendering;

/// <summary>
/// A visualizer drawn with WebGPU: a <see cref="WebGpuRenderView"/> (Direct3D 12 or Vulkan on
/// Windows, Vulkan on Linux, Metal on macOS - the WebGPU engine's choice) whose renderer the
/// subclass supplies. The view owns the device and its Continuous driver runs the frames on
/// the UI thread; when no device can be had, <see cref="IVisualizerSession.Problem"/> carries
/// the reason.
/// </summary>
public abstract class WebGpuVisualizerSession : VisualizerSession
{
    private WebGpuRenderView _view;
    private IWebGpuRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebGpuVisualizerSession"/> class.
    /// </summary>
    /// <param name="kind">The visualizer this session shows.</param>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    protected WebGpuVisualizerSession(VisualizerKind kind, IVisualizerCanvasHost host) : base(kind, host) { }

    /// <summary>
    /// Creates the renderer that draws the visual into the view's frames; called once, when
    /// the session starts.
    /// </summary>
    /// <param name="clock">The visual's clock, in seconds (it stops while the session is paused).</param>
    /// <param name="pixelScale">The view's pixels per layout unit.</param>
    /// <param name="format">The view's colour format - every pipeline's colour target.</param>
    /// <returns>The renderer; disposed when the session stops if it is <see cref="IDisposable"/>.</returns>
    protected abstract IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format);

    /// <inheritdoc />
    protected sealed override FrameworkElement CreateView()
    {
        _view = new WebGpuRenderView { TargetFrameRate = 60 };
        _renderer = CreateRenderer(() => ElapsedSeconds, () => _view?.PixelScale ?? 1.0, _view.PreferredFormat);
        _view.Renderer = _renderer;

        //Raised on the frame's thread (the UI thread) - and, during Stop, for the unload, which
        //  ReportProblem ignores because the session is no longer started
        _view.DeviceLost += (_, e) => ReportProblem($"{DisplayName} has no WebGPU device: {e.Reason}");
        return _view;
    }

    /// <inheritdoc />
    protected sealed override void SetViewPaused(bool paused) => _view.IsPaused = paused;

    /// <inheritdoc />
    protected sealed override void OnStopped()
    {
        _view = null;
        (_renderer as IDisposable)?.Dispose();
        _renderer = null;
    }
}
