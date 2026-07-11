using System;
using System.Numerics;
using CodeBrix.Platform.OpenGL;
using PolyHavenBrowser.Rendering;

namespace PolyHavenBrowser.Display;

/// <summary>
/// The OpenGL ES implementation of <see cref="IModelRenderEngine"/> - the app's current 3D
/// graphics backend. Each frame it:
/// <list type="number">
///   <item>makes an off-screen EGL/GLES context current (<see cref="EglOffscreenGlContext"/>);</item>
///   <item>draws the model with the shader-based <see cref="GlModelSceneRenderer"/> into a
///   framebuffer object (FBO) with colour + depth attachments;</item>
///   <item>reads the pixels back to CPU memory with <c>glReadPixels</c>;</item>
///   <item>restores the host head's GL context.</item>
/// </list>
/// This whole class is the swappable, API-specific layer: a Vulkan engine would replace the
/// context, the renderer, and the readback here while implementing the same interface, and the
/// painter/camera/loaders/UI above it would not change.
/// <para>
/// The EGL context is created <b>lazily</b> on the first <see cref="RenderFrame"/> call, because
/// a GL context must be created and used on the same thread that renders (the UI thread, inside
/// the Skia paint callback). Constructing the engine itself is cheap and thread-agnostic - it
/// only allocates the CPU-side renderer (and its camera).
/// </para>
/// </summary>
public sealed class OpenGlModelRenderEngine : IModelRenderEngine
{
    // The framework-free shader renderer. It needs a live GL handle and a bound framebuffer;
    // it owns the OrbitCamera and knows nothing about EGL, framebuffers, or Skia.
    private readonly GlModelSceneRenderer _renderer = new();

    // The off-screen EGL/GLES context; null until the first render creates it on the GL thread.
    private EglOffscreenGlContext _context;
    private bool _rendererInitialized;

    // The off-screen framebuffer we render into, plus its colour and depth renderbuffers. These
    // are recreated whenever the requested size changes.
    private uint _framebuffer;
    private uint _colorBuffer;
    private uint _depthBuffer;
    private uint _fbWidth;
    private uint _fbHeight;

    private bool _disposed;

    /// <inheritdoc />
    public OrbitCamera Camera => _renderer.Camera;

    /// <inheritdoc />
    public Vector3? FixedLightDirection
    {
        get => _renderer.FixedLightDirection;
        set => _renderer.FixedLightDirection = value;
    }

    /// <inheritdoc />
    public void SetModel(LoadedModel model) => _renderer.SetModel(model);

    /// <inheritdoc />
    public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        // Make our off-screen context current (creating it on first use). MakeCurrent saves
        // whatever context the host head had current, and DoneCurrent (below) restores it, so
        // this engine never disturbs the head's own renderer even though they share a thread.
        _context ??= EglOffscreenGlContext.Create();
        _context.MakeCurrent();
        try
        {
            var gl = _context.Gl;

            // One-time GL setup: compile shaders and link the program.
            if (!_rendererInitialized)
            {
                _renderer.Initialize(gl);
                _rendererInitialized = true;
            }

            // Make sure we have a framebuffer (colour + depth) of the requested size.
            EnsureFramebuffer(gl, (uint)width, (uint)height);

            // Draw the model into our framebuffer over the requested background colour.
            _renderer.BackgroundColor = background;
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            _renderer.Render(gl, (uint)width, (uint)height);

            // Copy the rendered pixels from the GPU back to CPU memory (RGBA8). This GPU->CPU
            // sync is the main per-frame cost - it is why interaction drops frames rather than
            // lowering resolution.
            var pixels = new byte[width * height * 4];
            gl.ReadPixels(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // OpenGL's first pixel row is the bottom of the image; flag that so the Skia bridge
            // flips it to match Skia's top-down surface.
            return new RenderedFrame(pixels, width, height, isBottomUp: true);
        }
        finally
        {
            // Hand the thread's GL state back to the host head before returning to Skia.
            _context.DoneCurrent();
        }
    }

    // Creates (or re-creates on a size change) the off-screen framebuffer: a colour renderbuffer
    // to draw into and read back, plus a depth renderbuffer so 3D geometry occludes correctly.
    private void EnsureFramebuffer(GL gl, uint width, uint height)
    {
        if (_framebuffer != 0 && _fbWidth == width && _fbHeight == height)
        {
            return;
        }

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
            // GL resources must be freed on the GL thread with the context current.
            _context.MakeCurrent();
            try
            {
                DeleteFramebuffer(_context.Gl);
                if (_rendererInitialized)
                {
                    _renderer.Uninitialize(_context.Gl);
                    _rendererInitialized = false;
                }
            }
            finally
            {
                _context.DoneCurrent();
            }

            _context.Dispose();
            _context = null;
        }
    }
}
