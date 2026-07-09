using System;
using System.Numerics;
using CodeBrix.Platform.OpenGL;
using SkiaSharp;
using PolyHavenBrowser.Rendering;

namespace PolyHavenBrowser.Display;

/// <summary>
/// Paints a <see cref="LoadedModel"/> (a real 3D model, or a texture wrapped on a cube)
/// using the GL model renderer. Each frame renders the scene into an off-screen framebuffer
/// on the EGL context, reads the pixels back, and draws them onto the Skia canvas. Pointer
/// drag orbits and the wheel zooms. An optional background texture is drawn (darkened,
/// cover-scaled) behind the model, with the model composited over it.
/// </summary>
public sealed class GlModelScenePainter : IScenePainter
{
    private const float OrbitDegreesPerPixel = 0.25f;
    private static readonly (float R, float G, float B, float A) SolidBackground = (0.13f, 0.13f, 0.15f, 1f);

    private readonly GlModelSceneRenderer _renderer = new();

    private EglOffscreenGlContext _context;
    private bool _glInitialized;

    private uint _framebuffer;
    private uint _colorBuffer;
    private uint _depthBuffer;
    private uint _fbWidth;
    private uint _fbHeight;

    private SKBitmap _backgroundBitmap;
    private bool _dragging;
    private double _lastX;
    private double _lastY;
    private bool _disposed;

    /// <summary>The orbit camera; set its yaw/pitch/fov before <see cref="SetModel"/> to frame from a chosen angle.</summary>
    public OrbitCamera Camera => _renderer.Camera;

    /// <summary>A fixed world-space light direction for solid-shape shading, or null for a camera headlight.</summary>
    public Vector3? FixedLightDirection
    {
        get => _renderer.FixedLightDirection;
        set => _renderer.FixedLightDirection = value;
    }

    /// <summary>Sets (or clears) the model to display. Safe to call from any thread.</summary>
    public void SetModel(LoadedModel model) => _renderer.SetModel(model);

    /// <summary>
    /// Sets a background texture drawn (darkened, cover-scaled) behind the model, or
    /// <see langword="null"/> for a solid dark background. Takes ownership of the bitmap.
    /// </summary>
    public void SetBackgroundTexture(SKBitmap bitmap)
    {
        var previous = _backgroundBitmap;
        _backgroundBitmap = bitmap;
        if (!ReferenceEquals(previous, bitmap))
        {
            previous?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Paint(SKSurface surface, SKImageInfo info)
    {
        if (_disposed || info.Width <= 0 || info.Height <= 0)
        {
            return;
        }

        //Render at full canvas resolution so the model stays crisp even when maximized;
        //smoothness under load comes from dropping frames (coalescing + stale-frame skip),
        //not from lowering resolution.
        var width = (uint)info.Width;
        var height = (uint)info.Height;
        var hasBackground = _backgroundBitmap != null;
        byte[] pixels;

        EnsureContext();
        _context.MakeCurrent();
        try
        {
            if (!_glInitialized)
            {
                _renderer.Initialize(_context.Gl);
                _glInitialized = true;
            }

            EnsureFramebuffer(width, height);

            //With a background texture, clear transparent so the model composites over it.
            _renderer.BackgroundColor = hasBackground ? (0f, 0f, 0f, 0f) : SolidBackground;

            var gl = _context.Gl;
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            _renderer.Render(gl, width, height);

            pixels = new byte[width * height * 4];
            gl.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        finally
        {
            //Hand the thread's GL state back to the host head before touching the Skia surface.
            _context.DoneCurrent();
        }

        DrawScene(surface, info, pixels);
    }

    private void DrawScene(SKSurface surface, SKImageInfo info, byte[] pixels)
    {
        var canvas = surface.Canvas;
        var sampling = new SKSamplingOptions(SKFilterMode.Linear);

        DrawBackground(canvas, info, sampling);

        //Straight (unpremultiplied) alpha so the transparent clear lets the background show.
        var imageInfo = new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var image = SKImage.FromPixelCopy(imageInfo, pixels);

        canvas.Save();
        //GL reads pixels bottom-up; flip vertically for Skia's top-down surface.
        canvas.Scale(1f, -1f);
        canvas.Translate(0f, -info.Height);
        canvas.DrawImage(image, new SKRect(0, 0, info.Width, info.Height), sampling);
        canvas.Restore();
    }

    private void DrawBackground(SKCanvas canvas, SKImageInfo info, SKSamplingOptions sampling)
    {
        if (_backgroundBitmap == null)
        {
            return;
        }

        //Cover-scale the texture (uniform, centered, cropped) so it fills without distortion.
        var scale = Math.Max((float)info.Width / _backgroundBitmap.Width, (float)info.Height / _backgroundBitmap.Height);
        var drawWidth = _backgroundBitmap.Width * scale;
        var drawHeight = _backgroundBitmap.Height * scale;
        var left = (info.Width - drawWidth) / 2f;
        var top = (info.Height - drawHeight) / 2f;

        using var paint = new SKPaint
        {
            //Darken the texture so the foreground cube stands out against it.
            ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(0x4D, 0x4D, 0x4D), SKBlendMode.Modulate),
        };
        canvas.DrawBitmap(_backgroundBitmap, new SKRect(left, top, left + drawWidth, top + drawHeight), sampling, paint);
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
        var deltaYaw = (float)(x - _lastX) * OrbitDegreesPerPixel;
        var deltaPitch = (float)(y - _lastY) * OrbitDegreesPerPixel;
        _lastX = x;
        _lastY = y;
        //Grab-and-drag feel: dragging right rolls the model's near face to the right, and
        //dragging up rolls its top toward you.
        _renderer.Camera.Orbit(-deltaYaw, deltaPitch);
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
    public void Zoom(double wheelDelta) => _renderer.Camera.Zoom(wheelDelta > 0 ? 0.9f : 1.1f);

    private void EnsureContext() => _context ??= EglOffscreenGlContext.Create();

    private void EnsureFramebuffer(uint width, uint height)
    {
        if (_framebuffer != 0 && _fbWidth == width && _fbHeight == height)
        {
            return;
        }

        var gl = _context.Gl;
        DeleteFramebuffer(gl);

        _framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _colorBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _colorBuffer);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Rgba8, width, height);
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            RenderbufferTarget.Renderbuffer, _colorBuffer);

        _depthBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent16, width, height);
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _depthBuffer);

        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new InvalidOperationException("The offscreen framebuffer is incomplete.");
        }

        _fbWidth = width;
        _fbHeight = height;
    }

    private void DeleteFramebuffer(GL gl)
    {
        if (_colorBuffer != 0) { gl.DeleteRenderbuffer(_colorBuffer); _colorBuffer = 0; }
        if (_depthBuffer != 0) { gl.DeleteRenderbuffer(_depthBuffer); _depthBuffer = 0; }
        if (_framebuffer != 0) { gl.DeleteFramebuffer(_framebuffer); _framebuffer = 0; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        if (_context != null)
        {
            _context.MakeCurrent();
            try
            {
                DeleteFramebuffer(_context.Gl);
                if (_glInitialized)
                {
                    _renderer.Uninitialize(_context.Gl);
                    _glInitialized = false;
                }
            }
            finally
            {
                _context.DoneCurrent();
            }

            _context.Dispose();
            _context = null;
        }

        _backgroundBitmap?.Dispose();
        _backgroundBitmap = null;
    }
}
