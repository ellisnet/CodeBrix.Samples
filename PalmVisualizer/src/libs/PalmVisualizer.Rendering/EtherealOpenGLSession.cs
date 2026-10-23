using System;
using Microsoft.UI.Xaml;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Ethereal - OpenGL": the Ethereal scene drawn by <see cref="EtherealSkiaRenderer"/> on a
/// <see cref="SkiaGpuCanvas"/> - GPU-accelerated Skia through the platform's
/// <c>SkiaGpuContext</c>, which is OpenGL (or OpenGL ES) on every head except macOS, where it
/// is Metal. Without a GPU context the same frames are drawn on the CPU and
/// <see cref="IVisualizerSession.Problem"/> says so. Set the environment variable
/// <c>PALMVISUALIZER_USE_CPU=1</c> to draw on the CPU deliberately.
/// </summary>
public sealed class EtherealOpenGLSession : VisualizerSession
{
    private SkiaGpuCanvas _canvas;
    private EtherealScene _scene;
    private EtherealSkiaRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealOpenGLSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public EtherealOpenGLSession(IVisualizerCanvasHost host) : base(VisualizerKind.EtherealOpenGL, host) { }

    /// <inheritdoc />
    protected override FrameworkElement CreateView()
    {
        _scene = new EtherealScene(AttractorField);
        _renderer = new EtherealSkiaRenderer();
        _canvas = new SkiaGpuCanvas
        {
            UseGpu = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1",
            TargetFrameRate = 60,
        };
        _canvas.RenderFrame += (_, e) =>
        {
            _scene.Advance(ElapsedSeconds, e.Info.Width, e.Info.Height, e.PixelScale);
            _renderer.Draw(e.Canvas, _scene);
        };
        _canvas.GpuUnavailable += (_, reason) =>
            ReportProblem($"Ethereal - OpenGL is drawing on the CPU: {reason}.");
        return _canvas;
    }

    /// <inheritdoc />
    protected override void SetViewPaused(bool paused) => _canvas.IsPaused = paused;

    /// <inheritdoc />
    protected override void OnStopped()
    {
        _canvas = null;
        _scene = null;
        _renderer?.Dispose();
        _renderer = null;
    }
}
