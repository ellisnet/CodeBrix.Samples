using CodeBrix.Platform.OpenGL;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Diagnostics for the "renders on GL ES but not on desktop GL" problem: the app used to force a
/// GL ES context (EGL) but now uses the head's native context, which on X11 is desktop GL (GLX).
/// These probe how a desktop-GL core-profile context reacts to the GLCanvasElement framebuffer
/// recipe versus the correct one, so the root cause is pinned before touching platform code.
/// </summary>
[Trait("Category", "RequiresGL")]
public class DesktopGlDiagnosticTests
{
    private readonly ITestOutputHelper _out;

    public DesktopGlDiagnosticTests(ITestOutputHelper output) => _out = output;

    private void WriteLine(string line) => _out.WriteLine(line);

    [Fact]
    public void probe_desktop_gl_framebuffer_binding_and_render()
    {
        using var ctx = GlDesktopTestContext.TryCreate();
        Assert.SkipWhen(ctx is null, "No desktop-GL-over-EGL stack available on this machine.");
        var gl = ctx!.Gl;

        WriteLine($"GL_VERSION  = {gl.GetStringS(StringName.Version)}");
        WriteLine($"GL_RENDERER = {gl.GetStringS(StringName.Renderer)}");
        WriteLine($"GLSL        = {gl.GetStringS(StringName.ShadingLanguageVersion)}");
        WriteLine("");

        const uint size = 64;

        // (A) The GLCanvasElement recipe: a name from glGenBuffers, bound as a framebuffer.
        var bufName = gl.GenBuffer();
        gl.BindFramebuffer(GLEnum.Framebuffer, bufName);
        var errAfterGenBufferBind = gl.GetError();
        WriteLine($"[A] glGenBuffers name bound as framebuffer -> glGetError = {errAfterGenBufferBind} (NoError means the driver tolerated it)");
        gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        // (B) The correct recipe: a name from glGenFramebuffers.
        var fbName = gl.GenFramebuffer();
        gl.BindFramebuffer(GLEnum.Framebuffer, fbName);
        var errAfterGenFramebufferBind = gl.GetError();
        WriteLine($"[B] glGenFramebuffers name bound as framebuffer -> glGetError = {errAfterGenFramebufferBind}");
        gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        gl.DeleteFramebuffer(fbName);
        gl.DeleteBuffer(bufName);

        // (C) Render a triangle into a CORRECT framebuffer (glGenFramebuffers + RGBA8 renderbuffer)
        //     to prove the renderer/shaders themselves work on desktop GL core profile.
        var (rendered, cornerSum, centerSum) = RenderTriangle(gl, size, useGenBufferForFramebuffer: false);
        WriteLine("");
        WriteLine($"[C] correct FBO: render threw? {(rendered ? "no" : "YES")}; corner(bg) sum={cornerSum}, center sum={centerSum}");

        // (D) Render into the GLCanvasElement-style framebuffer (glGenBuffers name) on desktop GL.
        var (renderedD, cornerSumD, centerSumD) = RenderTriangle(gl, size, useGenBufferForFramebuffer: true);
        WriteLine($"[D] GenBuffer FBO: render threw? {(renderedD ? "no" : "YES")}; corner(bg) sum={cornerSumD}, center sum={centerSumD}");
        WriteLine("");
        WriteLine("Interpretation: a correct [C] corner near the blue background (sum ~255) with a differing");
        WriteLine("center = renderer OK on desktop GL. If [A] errors and [D] differs from [C], the glGenBuffers");
        WriteLine("framebuffer name is the culprit on desktop GL core profile.");

        // (E) The EXACT FrameBufferDetails recipe, but with the GenBuffer->GenFramebuffer fix:
        //     RGB texture colour attachment + Depth24Stencil8 renderbuffer + BGRA readback.
        //     Confirms the single fix is sufficient on desktop GL.
        var (renderedE, cornerE, centerE) = RenderTriangleFrameBufferDetailsStyle(gl, size);
        WriteLine("");
        WriteLine($"[E] fixed FrameBufferDetails recipe (RGB tex + D24S8 + BGRA read): render threw? {(renderedE ? "no" : "YES")}; corner(bg) sum={cornerE}, center sum={centerE}");
        WriteLine(centerE != cornerE && renderedE
            ? "    => SUFFICIENT: GenBuffer->GenFramebuffer alone fixes desktop GL."
            : "    => NOT sufficient: another desktop-GL incompatibility remains in the recipe.");

        // The renderer must run on a correct desktop-GL framebuffer, and the GLCanvasElement-style
        // recipe (RGB texture + Depth24Stencil8 + BGRA read-back, with glGenFramebuffers) must too —
        // this is the desktop-GL path the X11/Win32/WPF/macOS heads exercise via GLCanvasElement.
        Assert.True(rendered, "Renderer threw on a correct desktop-GL framebuffer.");
        Assert.True(renderedE, "Renderer threw on the GLCanvasElement-style desktop-GL framebuffer.");
        Assert.True(centerSum > 0, "Nothing rendered into a correct desktop-GL framebuffer.");
        Assert.True(centerE > 0, "Nothing rendered into the GLCanvasElement-style desktop-GL framebuffer.");
    }

    // Renders TestAssets' triangle over a blue background into a fresh framebuffer and returns
    // whether it completed plus the channel sums of a corner (background) and the center pixel.
    private (bool Rendered, int CornerSum, int CenterSum) RenderTriangle(GL gl, uint size, bool useGenBufferForFramebuffer)
    {
        var framebuffer = useGenBufferForFramebuffer ? gl.GenBuffer() : gl.GenFramebuffer();
        gl.BindFramebuffer(GLEnum.Framebuffer, framebuffer);

        var colorRb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GLEnum.Renderbuffer, colorRb);
        gl.RenderbufferStorage(GLEnum.Renderbuffer, InternalFormat.Rgba8, size, size);
        gl.FramebufferRenderbuffer(GLEnum.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Renderbuffer, colorRb);

        var depthRb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GLEnum.Renderbuffer, depthRb);
        gl.RenderbufferStorage(GLEnum.Renderbuffer, InternalFormat.DepthComponent16, size, size);
        gl.FramebufferRenderbuffer(GLEnum.Framebuffer, FramebufferAttachment.DepthAttachment, GLEnum.Renderbuffer, depthRb);

        var status = gl.CheckFramebufferStatus(GLEnum.Framebuffer);
        WriteLine($"    framebuffer status ({(useGenBufferForFramebuffer ? "GenBuffer" : "GenFramebuffer")}) = {status}");

        var rendered = true;
        var pixels = new byte[size * size * 4];
        try
        {
            var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 1f, 1f) };
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
        }
        catch (Exception ex)
        {
            rendered = false;
            WriteLine($"    render threw: {ex.GetType().Name}: {ex.Message}");
        }

        gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        var cornerSum = pixels[0] + pixels[1] + pixels[2];
        var centerIdx = (int)((size / 2 * size + size / 2) * 4);
        var centerSum = pixels[centerIdx] + pixels[centerIdx + 1] + pixels[centerIdx + 2];
        return (rendered, cornerSum, centerSum);
    }

    // Mirrors GLCanvasElement.FrameBufferDetails EXACTLY except for the GenBuffer->GenFramebuffer
    // fix: an RGB texture colour attachment + a Depth24Stencil8 renderbuffer, read back as BGRA.
    private (bool Rendered, int CornerSum, int CenterSum) RenderTriangleFrameBufferDetailsStyle(GL gl, uint size)
    {
        var framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(GLEnum.Framebuffer, framebuffer);

        var colorTex = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, colorTex);
        var texInit = new byte[size * size * 3];
        gl.TexImage2D<byte>(GLEnum.Texture2D, 0, InternalFormat.Rgb, size, size, 0, GLEnum.Rgb, GLEnum.UnsignedByte, (ReadOnlySpan<byte>)texInit);
        uint linear = (uint)GLEnum.Linear;
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, in linear);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, in linear);
        gl.FramebufferTexture2D(GLEnum.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Texture2D, colorTex, 0);
        gl.BindTexture(GLEnum.Texture2D, 0);

        var depthRb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GLEnum.Renderbuffer, depthRb);
        gl.RenderbufferStorage(GLEnum.Renderbuffer, InternalFormat.Depth24Stencil8, size, size);
        gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, depthRb);
        gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        var status = gl.CheckFramebufferStatus(GLEnum.Framebuffer);
        WriteLine($"    [E] framebuffer status = {status}");

        var rendered = true;
        var pixels = new byte[size * size * 4];
        try
        {
            var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 1f, 1f) };
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);
            gl.ReadBuffer(GLEnum.ColorAttachment0);
            gl.ReadPixels(0, 0, size, size, PixelFormat.Bgra, PixelType.UnsignedByte, pixels.AsSpan());
        }
        catch (Exception ex)
        {
            rendered = false;
            WriteLine($"    [E] render threw: {ex.GetType().Name}: {ex.Message}");
        }

        gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        var cornerSum = pixels[0] + pixels[1] + pixels[2];
        var centerIdx = (int)((size / 2 * size + size / 2) * 4);
        var centerSum = pixels[centerIdx] + pixels[centerIdx + 1] + pixels[centerIdx + 2];
        return (rendered, cornerSum, centerSum);
    }
}
