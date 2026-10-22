using System;
using System.Diagnostics;
using SkiaSharp;
using PolyHavenBrowser.Rendering;

namespace PolyHavenBrowser.Display;

/// <summary>
/// Paints an interactive equirectangular HDRI panorama: each frame renders the current view into a
/// reused bitmap and draws it onto the Skia canvas. Pointer drag looks around; the wheel changes the
/// field of view.
/// <para>
/// The view is rendered on the GPU with <see cref="WebGpuPanoramaRenderer"/> when WebGPU is
/// available, and otherwise on the CPU with <see cref="EquirectPanoramaRenderer"/> - the same image
/// either way (the WebGPU shader mirrors the CPU math). <see cref="Create"/> picks the renderer; if
/// the GPU later fails mid-session (a lost device, a driver error), the painter switches to the CPU
/// renderer on the spot, keeping the current view.
/// </para>
/// </summary>
public sealed class PanoramaScenePainter : IScenePainter
{
    private const float LookDegreesPerPixel = 0.2f;
    private const float ZoomDegreesPerNotch = 5f;

    //Cap the CPU ray-traced resolution so a maximized window stays interactive; the result
    //is scaled up to fill the canvas. A lower cap while dragging keeps the look-around smooth;
    //the crisp full cap is used once the drag stops.
    private const int MaxRenderDimension = 1440;
    private const int DragRenderDimension = 960;

    //The GPU renders a frame in a fraction of the CPU's time, so it keeps full resolution while
    //dragging; its cap only bounds the per-frame readback on very large displays.
    private const int MaxGpuRenderDimension = 2560;

    private readonly FloatImage _panorama;
    private WebGpuPanoramaRenderer _gpuRenderer;
    private EquirectPanoramaRenderer _cpuRenderer;

    private SKBitmap _buffer;
    private int _bufferWidth;
    private int _bufferHeight;

    private bool _dragging;
    private double _lastX;
    private double _lastY;
    private bool _disposed;

    private PanoramaScenePainter(FloatImage panorama, WebGpuPanoramaRenderer gpuRenderer)
    {
        _panorama = panorama;
        _gpuRenderer = gpuRenderer;
        if (gpuRenderer == null)
        {
            _cpuRenderer = new EquirectPanoramaRenderer(panorama);
        }
    }

    /// <summary>
    /// Creates a painter over a decoded equirectangular panorama, rendering with WebGPU when it can
    /// and with the CPU otherwise. The WebGPU attempt creates a device, uploads the panorama and
    /// renders one tiny frame, so call this off the UI thread.
    /// </summary>
    public static PanoramaScenePainter Create(FloatImage panorama)
    {
        ArgumentNullException.ThrowIfNull(panorama);

        WebGpuPanoramaRenderer gpuRenderer = null;
        try
        {
            gpuRenderer = new WebGpuPanoramaRenderer(panorama);

            //Fail fast here, not inside the paint callback: one 1x1 frame exercises the whole path.
            using var probe = gpuRenderer.Render(1, 1);
            return new PanoramaScenePainter(panorama, gpuRenderer);
        }
        catch (Exception ex)
        {
            gpuRenderer?.Dispose();
            var painter = new PanoramaScenePainter(panorama, null) { FallbackReason = ex.Message };
            return painter;
        }
    }

    /// <summary>A short name for the renderer in use, for the status line ("WebGPU (D3D12)" or "CPU").</summary>
    public string RendererName => _gpuRenderer != null ? $"WebGPU ({_gpuRenderer.BackendName})" : "CPU";

    /// <summary>Why the WebGPU renderer is not in use, or <see langword="null"/> while it is.</summary>
    public string FallbackReason { get; private set; }

    private PanoramaCamera Camera => _gpuRenderer?.Camera ?? _cpuRenderer.Camera;

    /// <inheritdoc />
    public void Paint(SKSurface surface, SKImageInfo info)
    {
        if (_disposed || info.Width <= 0 || info.Height <= 0)
        {
            return;
        }

        var (renderWidth, renderHeight) = CapResolution(info.Width, info.Height);
        if (_buffer == null || _bufferWidth != renderWidth || _bufferHeight != renderHeight)
        {
            _buffer?.Dispose();
            _buffer = new SKBitmap(new SKImageInfo(renderWidth, renderHeight, SKColorType.Rgba8888, SKAlphaType.Opaque));
            _bufferWidth = renderWidth;
            _bufferHeight = renderHeight;
        }

        if (_gpuRenderer != null)
        {
            try
            {
                _gpuRenderer.RenderTo(_buffer);
            }
            catch (Exception ex)
            {
                //Stay on screen: drop to the CPU renderer for the rest of this panorama.
                FallBackToCpu(ex.Message);
            }
        }
        _cpuRenderer?.RenderTo(_buffer);

        surface.Canvas.DrawBitmap(_buffer, new SKRect(0, 0, info.Width, info.Height), new SKSamplingOptions(SKFilterMode.Linear));
    }

    //Swaps the failed GPU renderer for the CPU one, carrying over where the user is looking.
    private void FallBackToCpu(string reason)
    {
        Debug.WriteLine($"WebGPU panorama rendering failed; continuing on the CPU: {reason}");
        var gpuCamera = _gpuRenderer.Camera;
        _cpuRenderer = new EquirectPanoramaRenderer(_panorama);
        _cpuRenderer.Camera.YawDegrees = gpuCamera.YawDegrees;
        _cpuRenderer.Camera.PitchDegrees = gpuCamera.PitchDegrees;
        _cpuRenderer.Camera.FovDegrees = gpuCamera.FovDegrees;
        _gpuRenderer.Dispose();
        _gpuRenderer = null;
        FallbackReason = reason;
    }

    /// <inheritdoc />
    public void PointerDown(double x, double y)
    {
        _dragging = true;
        _lastX = x;
        _lastY = y;
    }

    /// <inheritdoc />
    public void PointerDrag(double x, double y)
    {
        if (!_dragging) { return; }
        var deltaYaw = (float)(x - _lastX) * LookDegreesPerPixel;
        var deltaPitch = (float)(y - _lastY) * LookDegreesPerPixel;
        _lastX = x;
        _lastY = y;
        //Grab-and-drag feel: dragging right pulls the scene right (turns the view left);
        //dragging up looks up.
        Camera.Rotate(-deltaYaw, deltaPitch);
    }

    private (int Width, int Height) CapResolution(int width, int height)
    {
        var max = _gpuRenderer != null ? MaxGpuRenderDimension
            : _dragging ? DragRenderDimension : MaxRenderDimension;
        var longest = Math.Max(width, height);
        if (longest <= max)
        {
            return (width, height);
        }
        var scale = (double)max / longest;
        return (Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
    }

    /// <inheritdoc />
    public void PointerSkip(double x, double y)
    {
        if (!_dragging) { return; }
        _lastX = x;
        _lastY = y;
    }

    /// <inheritdoc />
    public void PointerUp() => _dragging = false;

    /// <inheritdoc />
    public void Zoom(double wheelDelta) =>
        Camera.Zoom(wheelDelta > 0 ? -ZoomDegreesPerNotch : ZoomDegreesPerNotch);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        _gpuRenderer?.Dispose();
        _gpuRenderer = null;
        _buffer?.Dispose();
        _buffer = null;
    }
}
