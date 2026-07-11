using System;
using System.Runtime.InteropServices;
using CodeBrix.Platform.OpenGL;

namespace PolyHavenBrowser.Display;

/// <summary>
/// A headless OpenGL ES context created through EGL, used to render 3D previews off-screen
/// into a framebuffer whose pixels are then read back and drawn onto the Skia canvas.
/// Uses the Mesa surfaceless platform on Linux (and would use ANGLE's <c>libEGL</c> on
/// Windows). <see cref="MakeCurrent"/> saves whatever context was current and
/// <see cref="DoneCurrent"/> restores it, so this context never disturbs the host head's own
/// renderer, even when they share a thread.
/// </summary>
public sealed class EglOffscreenGlContext : IDisposable
{
    private const string LibEgl = "libEGL.so.1";

    private const int EGL_PLATFORM_SURFACELESS_MESA = 0x31DD;
    private const int EGL_SURFACE_TYPE = 0x3033;
    private const int EGL_PBUFFER_BIT = 0x0001;
    private const int EGL_RENDERABLE_TYPE = 0x3040;
    private const int EGL_OPENGL_ES2_BIT = 0x0004;
    private const int EGL_RED_SIZE = 0x3024;
    private const int EGL_GREEN_SIZE = 0x3023;
    private const int EGL_BLUE_SIZE = 0x3022;
    private const int EGL_ALPHA_SIZE = 0x3021;
    private const int EGL_DEPTH_SIZE = 0x3025;
    private const int EGL_NONE = 0x3038;
    private const int EGL_WIDTH = 0x3057;
    private const int EGL_HEIGHT = 0x3056;
    private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
    private const int EGL_OPENGL_ES_API = 0x30A0;
    private const int EGL_DRAW = 0x3059;
    private const int EGL_READ = 0x305A;

    [DllImport(LibEgl)] private static extern IntPtr eglGetPlatformDisplay(int platform, IntPtr nativeDisplay, IntPtr attribs);
    [DllImport(LibEgl)] private static extern IntPtr eglGetDisplay(IntPtr nativeDisplay);
    [DllImport(LibEgl)] private static extern bool eglInitialize(IntPtr display, out int major, out int minor);
    [DllImport(LibEgl)] private static extern bool eglBindAPI(int api);
    [DllImport(LibEgl)] private static extern bool eglChooseConfig(IntPtr display, int[] attribs, [In][Out] IntPtr[] configs, int configSize, out int numConfig);
    [DllImport(LibEgl)] private static extern IntPtr eglCreateContext(IntPtr display, IntPtr config, IntPtr shareContext, int[] attribs);
    [DllImport(LibEgl)] private static extern IntPtr eglCreatePbufferSurface(IntPtr display, IntPtr config, int[] attribs);
    [DllImport(LibEgl)] private static extern bool eglMakeCurrent(IntPtr display, IntPtr draw, IntPtr read, IntPtr context);
    [DllImport(LibEgl)] private static extern bool eglDestroySurface(IntPtr display, IntPtr surface);
    [DllImport(LibEgl)] private static extern bool eglDestroyContext(IntPtr display, IntPtr context);
    [DllImport(LibEgl)] private static extern IntPtr eglGetProcAddress(string name);
    [DllImport(LibEgl)] private static extern IntPtr eglGetCurrentDisplay();
    [DllImport(LibEgl)] private static extern IntPtr eglGetCurrentContext();
    [DllImport(LibEgl)] private static extern IntPtr eglGetCurrentSurface(int readdraw);

    private readonly IntPtr _display;
    private readonly IntPtr _context;
    private readonly IntPtr _surface;

    private IntPtr _prevDisplay;
    private IntPtr _prevDraw;
    private IntPtr _prevRead;
    private IntPtr _prevContext;
    private bool _disposed;

    private EglOffscreenGlContext(IntPtr display, IntPtr context, IntPtr surface, GL gl)
    {
        _display = display;
        _context = context;
        _surface = surface;
        Gl = gl;
    }

    /// <summary>The GL API bound to this context.</summary>
    public GL Gl { get; }

    /// <summary>Creates the offscreen context, throwing when no EGL/GL stack is available.</summary>
    public static EglOffscreenGlContext Create()
    {
        var display = eglGetPlatformDisplay(EGL_PLATFORM_SURFACELESS_MESA, IntPtr.Zero, IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            display = eglGetDisplay(IntPtr.Zero);
        }
        if (display == IntPtr.Zero || !eglInitialize(display, out _, out _))
        {
            throw new InvalidOperationException("Unable to initialize an EGL display for offscreen rendering.");
        }

        eglBindAPI(EGL_OPENGL_ES_API);

        int[] configAttribs =
        [
            EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
            EGL_RED_SIZE, 8,
            EGL_GREEN_SIZE, 8,
            EGL_BLUE_SIZE, 8,
            EGL_ALPHA_SIZE, 8,
            EGL_DEPTH_SIZE, 16,
            EGL_NONE,
        ];
        var configs = new IntPtr[1];
        if (!eglChooseConfig(display, configAttribs, configs, 1, out var numConfig) || numConfig < 1)
        {
            throw new InvalidOperationException("No suitable EGL framebuffer configuration was found.");
        }

        var context = eglCreateContext(display, configs[0], IntPtr.Zero, [EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE]);
        if (context == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create an OpenGL ES 3 context.");
        }

        //A 1x1 pbuffer keeps drivers that dislike surfaceless make-current happy; rendering
        //  always targets our own framebuffer object, so the pbuffer size is irrelevant.
        var surface = eglCreatePbufferSurface(display, configs[0], [EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE]);

        if (!eglMakeCurrent(display, surface, surface, context))
        {
            eglDestroyContext(display, context);
            throw new InvalidOperationException("Unable to make the offscreen GL context current.");
        }

        GL gl = GL.GetApi(name => eglGetProcAddress(name));

        //Leave nothing current until the first MakeCurrent().
        eglMakeCurrent(display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return new EglOffscreenGlContext(display, context, surface, gl);
    }

    /// <summary>Makes this context current, remembering the previously-current context.</summary>
    public void MakeCurrent()
    {
        _prevDisplay = eglGetCurrentDisplay();
        _prevDraw = eglGetCurrentSurface(EGL_DRAW);
        _prevRead = eglGetCurrentSurface(EGL_READ);
        _prevContext = eglGetCurrentContext();

        if (!eglMakeCurrent(_display, _surface, _surface, _context))
        {
            throw new InvalidOperationException("Unable to make the offscreen GL context current.");
        }
    }

    /// <summary>Restores whatever context was current when <see cref="MakeCurrent"/> was called.</summary>
    public void DoneCurrent()
    {
        if (_prevContext != IntPtr.Zero && _prevDisplay != IntPtr.Zero)
        {
            eglMakeCurrent(_prevDisplay, _prevDraw, _prevRead, _prevContext);
        }
        else
        {
            eglMakeCurrent(_display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
    }

    /// <summary>Destroys the context and its resources.</summary>
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        Gl.Dispose();
        eglMakeCurrent(_display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (_surface != IntPtr.Zero)
        {
            eglDestroySurface(_display, _surface);
        }
        eglDestroyContext(_display, _context);
    }
}
