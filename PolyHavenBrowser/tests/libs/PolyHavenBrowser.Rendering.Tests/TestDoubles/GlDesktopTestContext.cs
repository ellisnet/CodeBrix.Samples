using System.Runtime.InteropServices;
using CodeBrix.Platform.OpenGL;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Creates a headless <b>desktop</b> OpenGL (core profile 3.3) context via EGL — matching what
/// the X11/Win32/WPF/macOS heads give through GLX/WGL/CGL, as opposed to the GL ES context
/// <see cref="EglTestContext"/> creates. Used to reproduce the "renders on Linux only" problem,
/// which appeared when the app moved from a forced GL ES context to the head's native desktop GL.
/// Returns <see langword="null"/> when no desktop-GL-over-EGL stack is available.
/// </summary>
internal sealed class GlDesktopTestContext : IDisposable
{
    private const string LibEgl = "libEGL.so.1";
    private const int EGL_PLATFORM_SURFACELESS_MESA = 0x31DD;
    private const int EGL_SURFACE_TYPE = 0x3033;
    private const int EGL_PBUFFER_BIT = 0x0001;
    private const int EGL_RENDERABLE_TYPE = 0x3040;
    private const int EGL_OPENGL_BIT = 0x0008;
    private const int EGL_RED_SIZE = 0x3024;
    private const int EGL_GREEN_SIZE = 0x3023;
    private const int EGL_BLUE_SIZE = 0x3022;
    private const int EGL_ALPHA_SIZE = 0x3021;
    private const int EGL_DEPTH_SIZE = 0x3025;
    private const int EGL_NONE = 0x3038;
    private const int EGL_WIDTH = 0x3057;
    private const int EGL_HEIGHT = 0x3056;
    private const int EGL_OPENGL_API = 0x30A2;
    private const int EGL_CONTEXT_MAJOR_VERSION = 0x3098;
    private const int EGL_CONTEXT_MINOR_VERSION = 0x30FB;
    private const int EGL_CONTEXT_OPENGL_PROFILE_MASK = 0x30FD;
    private const int EGL_CONTEXT_OPENGL_CORE_PROFILE_BIT = 0x0001;

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

    private readonly IntPtr _display;
    private readonly IntPtr _context;
    private readonly IntPtr _surface;

    private GlDesktopTestContext(IntPtr display, IntPtr context, IntPtr surface, GL gl)
    {
        _display = display;
        _context = context;
        _surface = surface;
        Gl = gl;
    }

    public GL Gl { get; }

    public static GlDesktopTestContext? TryCreate()
    {
        if (!OperatingSystem.IsLinux())
        {
            return null;
        }

        try
        {
            var display = eglGetPlatformDisplay(EGL_PLATFORM_SURFACELESS_MESA, IntPtr.Zero, IntPtr.Zero);
            if (display == IntPtr.Zero)
            {
                display = eglGetDisplay(IntPtr.Zero);
            }
            if (display == IntPtr.Zero || !eglInitialize(display, out _, out _))
            {
                return null;
            }

            if (!eglBindAPI(EGL_OPENGL_API))
            {
                return null;
            }

            int[] configAttribs =
            [
                EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
                EGL_RENDERABLE_TYPE, EGL_OPENGL_BIT,
                EGL_RED_SIZE, 8, EGL_GREEN_SIZE, 8, EGL_BLUE_SIZE, 8, EGL_ALPHA_SIZE, 8,
                EGL_DEPTH_SIZE, 16,
                EGL_NONE,
            ];
            var configs = new IntPtr[1];
            if (!eglChooseConfig(display, configAttribs, configs, 1, out var numConfig) || numConfig < 1)
            {
                return null;
            }

            int[] contextAttribs =
            [
                EGL_CONTEXT_MAJOR_VERSION, 3,
                EGL_CONTEXT_MINOR_VERSION, 3,
                EGL_CONTEXT_OPENGL_PROFILE_MASK, EGL_CONTEXT_OPENGL_CORE_PROFILE_BIT,
                EGL_NONE,
            ];
            var context = eglCreateContext(display, configs[0], IntPtr.Zero, contextAttribs);
            if (context == IntPtr.Zero)
            {
                return null;
            }

            var surface = eglCreatePbufferSurface(display, configs[0], [EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE]);
            if (!eglMakeCurrent(display, surface, surface, context))
            {
                eglDestroyContext(display, context);
                return null;
            }

            var gl = GL.GetApi(name => eglGetProcAddress(name));
            return new GlDesktopTestContext(display, context, surface, gl);
        }
        catch (DllNotFoundException) { return null; }
        catch (EntryPointNotFoundException) { return null; }
    }

    public void Dispose()
    {
        Gl.Dispose();
        eglMakeCurrent(_display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (_surface != IntPtr.Zero)
        {
            eglDestroySurface(_display, _surface);
        }
        eglDestroyContext(_display, _context);
    }
}
