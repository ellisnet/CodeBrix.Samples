using System.Runtime.InteropServices;
using CodeBrix.Platform.OpenGL;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Creates a headless OpenGL ES context via the Mesa EGL surfaceless platform (hardware
/// render node when available, llvmpipe software rendering otherwise) so the real GL
/// renderer can be exercised in tests with no window system. Returns
/// <see langword="null"/> when no EGL/GL stack is available on the machine.
/// </summary>
internal sealed class EglTestContext : IDisposable
{
    private const string LibEgl = "libEGL.so.1";
    private const int EGL_PLATFORM_SURFACELESS_MESA = 0x31DD;
    private const int EGL_SURFACE_TYPE = 0x3033;
    private const int EGL_PBUFFER_BIT = 0x0001;
    private const int EGL_RENDERABLE_TYPE = 0x3040;
    private const int EGL_OPENGL_ES2_BIT = 0x04;
    private const int EGL_NONE = 0x3038;
    private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

    // The core EGL 1.5 entry point: unlike eglGetPlatformDisplayEXT, this is a real
    // exported symbol even under GLVND's dispatcher libEGL.
    [DllImport(LibEgl)] private static extern IntPtr eglGetPlatformDisplay(int platform, IntPtr nativeDisplay, IntPtr attribs);
    [DllImport(LibEgl)] private static extern bool eglInitialize(IntPtr display, out int major, out int minor);
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

    private EglTestContext(IntPtr display, IntPtr context, IntPtr surface, GL gl)
    {
        _display = display;
        _context = context;
        _surface = surface;
        Gl = gl;
    }

    public GL Gl { get; }

    /// <summary>Tries to create a current GL context; returns <see langword="null"/> when the machine can't.</summary>
    public static EglTestContext? TryCreate()
    {
        if (!OperatingSystem.IsLinux())
        {
            return null;
        }

        try
        {
            var display = eglGetPlatformDisplay(EGL_PLATFORM_SURFACELESS_MESA, IntPtr.Zero, IntPtr.Zero);
            if (display == IntPtr.Zero || !eglInitialize(display, out _, out _))
            {
                return null;
            }

            int[] configAttribs =
            [
                EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
                EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
                EGL_NONE,
            ];
            var configs = new IntPtr[1];
            if (!eglChooseConfig(display, configAttribs, configs, 1, out var numConfig) || numConfig < 1)
            {
                return null;
            }

            var context = eglCreateContext(display, configs[0], IntPtr.Zero, [EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE]);
            if (context == IntPtr.Zero)
            {
                return null;
            }

            var surface = eglCreatePbufferSurface(display, configs[0], [EGL_NONE]);
            if (!eglMakeCurrent(display, surface, surface, context))
            {
                eglDestroyContext(display, context);
                return null;
            }

            var gl = GL.GetApi(name => eglGetProcAddress(name));
            return new EglTestContext(display, context, surface, gl);
        }
        catch (DllNotFoundException)
        {
            return null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
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
