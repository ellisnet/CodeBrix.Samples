using CodeBrix.Platform.OpenGL;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Exercises <see cref="GlModelSceneRenderer"/> against a real (llvmpipe or GPU) OpenGL ES
/// context via the Mesa surfaceless platform. Skips when no GL stack is available.
/// </summary>
[Trait("Category", "RequiresGL")]
public class GlModelSceneRendererTests
{
    private static EglTestContext RequireGl()
    {
        var context = EglTestContext.TryCreate();
        Assert.SkipWhen(context is null, "No EGL/OpenGL stack available on this machine (install Mesa llvmpipe).");
        return context!;
    }

    private static (uint Fbo, uint ColorRb, uint DepthRb) CreateFramebuffer(GL gl, uint width, uint height)
    {
        var fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        var colorRb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRb);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Rgba8, width, height);
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            RenderbufferTarget.Renderbuffer, colorRb);

        var depthRb = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRb);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent16, width, height);
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, depthRb);

        gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
            .Should().Be(GLEnum.FramebufferComplete);
        return (fbo, colorRb, depthRb);
    }

    [Fact]
    public void renderer_initializes_and_renders_a_triangle_onto_the_background()
    {
        //Arrange
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 64;
        var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);

        var renderer = new GlModelSceneRenderer
        {
            BackgroundColor = (0f, 0f, 1f, 1f), // blue background, red triangle
        };

        try
        {
            //Act
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);

            var pixels = new byte[size * size * 4];
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

            //Assert - some pixels show the red triangle, some the blue background
            var sawTriangle = false;
            var sawBackground = false;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i] > 128 && pixels[i + 2] < 100)
                {
                    sawTriangle = true;
                }
                if (pixels[i + 2] > 128 && pixels[i] < 100)
                {
                    sawBackground = true;
                }
            }

            sawTriangle.Should().BeTrue();
            sawBackground.Should().BeTrue();
        }
        finally
        {
            renderer.Uninitialize(gl);
            gl.DeleteRenderbuffer(colorRb);
            gl.DeleteRenderbuffer(depthRb);
            gl.DeleteFramebuffer(fbo);
        }
    }

    [Fact]
    public void clearing_the_model_renders_only_the_background()
    {
        //Arrange
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 16;
        var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);

        var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 1f, 0f, 1f) };

        try
        {
            //Act
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);
            renderer.SetModel(null);
            renderer.Render(gl, size, size);

            var pixels = new byte[size * size * 4];
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

            //Assert - all pixels are the green background
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i].Should().Be(0);
                pixels[i + 1].Should().Be(255);
            }
        }
        finally
        {
            renderer.Uninitialize(gl);
            gl.DeleteRenderbuffer(colorRb);
            gl.DeleteRenderbuffer(depthRb);
            gl.DeleteFramebuffer(fbo);
        }
    }

    [Fact]
    public void full_pipeline_from_glb_to_pixels()
    {
        //Arrange - end to end: SharpGLTF-built .glb → loader → GL renderer → pixels
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 64;
        var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);

        var model = new GltfModelLoader().Load(new MemoryStream(TestAssets.BuildTriangleGlb()));
        var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 0f, 1f) };

        try
        {
            //Act
            renderer.Initialize(gl);
            renderer.SetModel(model);
            renderer.Render(gl, size, size);

            var pixels = new byte[size * size * 4];
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

            //Assert - the red material shows up
            var redSeen = false;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i] > 100)
                {
                    redSeen = true;
                    break;
                }
            }
            redSeen.Should().BeTrue();
        }
        finally
        {
            renderer.Uninitialize(gl);
            gl.DeleteRenderbuffer(colorRb);
            gl.DeleteRenderbuffer(depthRb);
            gl.DeleteFramebuffer(fbo);
        }
    }
}
