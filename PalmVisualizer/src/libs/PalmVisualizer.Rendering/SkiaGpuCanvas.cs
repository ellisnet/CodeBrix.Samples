using System;
using System.Diagnostics;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PalmVisualizer.Rendering;

/// <summary>
/// A continuously animating SkiaSharp canvas whose frames are rasterized on the GPU: each
/// frame is drawn into an off-screen GPU surface from the platform's backend-neutral
/// <see cref="SkiaGpuContext"/> (OpenGL/GLES on every head except macOS, where it is Metal),
/// read back to processor memory in one copy, and presented through this ordinary
/// <see cref="SKXamlCanvas"/> - so it clips, overlaps and lays out like any other XAML content.
/// Handle <see cref="RenderFrame"/> to draw.
/// </summary>
/// <remarks>
/// <para>
/// Frames are drawn at the canvas's physical pixel size, so presenting one is a 1:1 copy with
/// no rescaling. A dispatcher timer on the UI thread drives them at
/// <see cref="TargetFrameRate"/>; a frame that takes longer than the interval simply delays
/// the next tick, so a slow frame costs frame rate, never a backlog. All GPU work stays on the
/// UI thread, inside a <see cref="SkiaGpuContext.BeginFrame"/> scope.
/// </para>
/// <para>
/// When the head cannot provide a GPU context (or <see cref="UseGpu"/> is off) the same
/// frames are drawn on the CPU into a raster surface instead, and
/// <see cref="GpuUnavailable"/> says why. The GPU context is released whenever the canvas
/// leaves the visual tree - while its window still exists, which GL teardown requires - and
/// re-created when it returns.
/// </para>
/// </remarks>
public sealed class SkiaGpuCanvas : SKXamlCanvas
{
    private readonly DispatcherTimer _timer;
    private readonly SkiaGpuFrameEventArgs _frameArgs = new();

    private SkiaGpuContext _gpu;
    private bool _gpuInitAttempted;
    private bool _gpuAvailable;
    private SKSurface _gpuSurface;
    private SKImageInfo _gpuSurfaceInfo;

    //The latest finished frame in processor memory - what the canvas paints
    private SKBitmap _frame;

    private bool _isPaused;
    private double _targetFrameRate = 60;

    /// <summary>Initializes a new instance of the <see cref="SkiaGpuCanvas"/> class.</summary>
    public SkiaGpuCanvas()
    {
        _timer = new DispatcherTimer { Interval = FrameInterval(_targetFrameRate) };
        _timer.Tick += (_, _) => RenderOneFrame();

        PaintSurface += OnPaintSurface;
        Loaded += (_, _) => UpdateTimer();
        Unloaded += (_, _) =>
        {
            _timer.Stop();
            ReleaseGpu();
        };
    }

    /// <summary>
    /// Raised once per frame, on the UI thread, to draw it. Fill the whole
    /// <see cref="SkiaGpuFrameEventArgs.Info"/> area: the frame is presented as drawn. The
    /// args object is reused; keep nothing from it.
    /// </summary>
    public event EventHandler<SkiaGpuFrameEventArgs> RenderFrame;

    /// <summary>
    /// Raised once, on the UI thread, when frames are drawn on the CPU instead of the GPU;
    /// the message says why.
    /// </summary>
    public event EventHandler<string> GpuUnavailable;

    /// <summary>
    /// Whether frames should be drawn on the GPU. Defaults to <see langword="true"/>; set
    /// <see langword="false"/> before the first frame to draw on the CPU instead.
    /// </summary>
    public bool UseGpu { get; set; } = true;

    /// <summary>
    /// Whether the GPU context was created: <see langword="null"/> until the first frame
    /// tries, <see langword="false"/> when frames are drawn on the CPU.
    /// </summary>
    public bool? IsGpuInitialized => _gpuInitAttempted ? _gpuAvailable : null;

    /// <summary>Frames per second to draw at. Defaults to 60.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not a finite
    /// positive number.</exception>
    public double TargetFrameRate
    {
        get => _targetFrameRate;
        set
        {
            if (!double.IsFinite(value) || value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The frame rate must be a finite positive number.");
            }
            _targetFrameRate = value;
            _timer.Interval = FrameInterval(value);
        }
    }

    /// <summary>
    /// Stops drawing frames while <see langword="true"/>; the last frame stays on screen and
    /// a paused canvas costs nothing per frame.
    /// </summary>
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;
            UpdateTimer();
        }
    }

    private static TimeSpan FrameInterval(double framesPerSecond) => TimeSpan.FromSeconds(1.0 / framesPerSecond);

    private void UpdateTimer()
    {
        if (IsLoaded && !_isPaused)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
    }

    private void RenderOneFrame()
    {
        double scale = XamlRoot?.RasterizationScale ?? 1.0;
        int width = (int)Math.Round(ActualWidth * scale);
        int height = (int)Math.Round(ActualHeight * scale);
        if (width <= 0 || height <= 0 || RenderFrame == null) { return; }

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        if (_frame == null || _frame.Width != width || _frame.Height != height)
        {
            _frame?.Dispose();
            _frame = new SKBitmap(info);
        }

        try
        {
            if (UseGpu && EnsureGpu())
            {
                using (_gpu.BeginFrame())
                {
                    if (_gpuSurface == null || _gpuSurfaceInfo.Width != width || _gpuSurfaceInfo.Height != height)
                    {
                        _gpuSurface?.Dispose();
                        _gpuSurface = SKSurface.Create(_gpu.GrContext, false, info)
                            ?? throw new InvalidOperationException($"Could not create a {width}x{height} GPU surface.");
                        _gpuSurfaceInfo = info;
                    }

                    DrawFrame(_gpuSurface.Canvas, info, scale);
                    _gpuSurface.Flush();

                    //The one GPU -> processor copy per frame (needs the context current)
                    if (!_gpuSurface.ReadPixels(info, _frame.GetPixels(), _frame.RowBytes, 0, 0))
                    {
                        Debug.WriteLine("SkiaGpuCanvas: the GPU frame could not be read back.");
                        return;
                    }
                }
            }
            else
            {
                using var surface = SKSurface.Create(info, _frame.GetPixels(), _frame.RowBytes);
                DrawFrame(surface.Canvas, info, scale);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"SkiaGpuCanvas: the frame failed: {e}");
            return;
        }

        Invalidate();
    }

    private void DrawFrame(SKCanvas canvas, SKImageInfo info, double scale)
    {
        _frameArgs.Set(canvas, info, (float)scale);
        RenderFrame?.Invoke(this, _frameArgs);
        _frameArgs.Set(null, default, 1f);
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Black);
        if (_frame != null)
        {
            //The frame was drawn at this canvas's pixel size, so this is a straight copy; it is
            //  only rescaled between a resize and the next frame
            canvas.DrawBitmap(_frame, new SKRect(0, 0, e.Info.Width, e.Info.Height), new SKSamplingOptions(SKFilterMode.Linear));
        }
    }

    //Creates the GPU context once the canvas is in a window; false means draw on the CPU
    private bool EnsureGpu()
    {
        if (_gpuInitAttempted) { return _gpuAvailable; }

        //TryCreate needs a live XamlRoot; before that, draw this frame on the CPU and retry
        if (!IsLoaded || XamlRoot == null) { return false; }

        _gpuInitAttempted = true;
        string reason;
        try
        {
            if (SkiaGpuContext.TryCreate(XamlRoot, out _gpu))
            {
                _gpuAvailable = true;
                Debug.WriteLine($"SkiaGpuCanvas: GPU rendering initialized (backend: {_gpu.Backend}).");
                return true;
            }
            reason = "this head provides no GPU context";
        }
        catch (Exception e)
        {
            _gpu?.Dispose();
            _gpu = null;
            reason = e.Message;
        }

        _gpuAvailable = false;
        GpuUnavailable?.Invoke(this, reason);
        return false;
    }

    //Releases the GPU surface (inside a frame scope, as GL requires) and then the context
    private void ReleaseGpu()
    {
        if (_gpu != null)
        {
            try
            {
                using (_gpu.BeginFrame())
                {
                    _gpuSurface?.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SkiaGpuCanvas: GPU surface teardown failed: {e.Message}");
            }
            _gpu.Dispose();
        }

        _gpuSurface = null;
        _gpu = null;
        _gpuInitAttempted = false;
        _gpuAvailable = false;
    }
}

/// <summary>Provides data for the <see cref="SkiaGpuCanvas.RenderFrame"/> event.</summary>
public sealed class SkiaGpuFrameEventArgs : EventArgs
{
    /// <summary>The canvas to draw the frame on: GPU-backed, or raster on the CPU fallback.</summary>
    public SKCanvas Canvas { get; private set; }

    /// <summary>The frame's size in pixels (the canvas's layout size times <see cref="PixelScale"/>).</summary>
    public SKImageInfo Info { get; private set; }

    /// <summary>Pixels per layout unit: the window's rasterization scale.</summary>
    public float PixelScale { get; private set; }

    internal void Set(SKCanvas canvas, SKImageInfo info, float pixelScale)
    {
        Canvas = canvas;
        Info = info;
        PixelScale = pixelScale;
    }
}
