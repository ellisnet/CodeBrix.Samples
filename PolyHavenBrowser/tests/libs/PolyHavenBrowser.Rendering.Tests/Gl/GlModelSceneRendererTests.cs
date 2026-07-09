using System.Collections.Generic;
using System.Numerics;
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

    [Fact]
    public void nearer_geometry_occludes_farther_geometry_regardless_of_draw_order()
    {
        //Arrange - two large overlapping triangles centered on the origin: a near red one
        //(z=+0.5) and a far blue one (z=-0.5). Viewed from a ROTATED (non-axis-aligned)
        //camera, the near red triangle must win the center pixel no matter which is drawn
        //first. A rotated view is essential: a bad model-view-projection transpose collapses
        //the depth axis only for non-axis-aligned cameras (an axis-aligned view hides it).
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 32;
        var center = (((int)size / 2) * (int)size + ((int)size / 2)) * 4;

        foreach (var nearDrawnFirst in new[] { false, true })
        {
            var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);
            var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 0f, 1f) };
            try
            {
                //Act
                renderer.Initialize(gl);
                renderer.SetModel(BuildDepthOrderingModel(nearDrawnFirst));
                renderer.Camera.YawDegrees = 35f;
                renderer.Camera.PitchDegrees = 28f;
                renderer.Render(gl, size, size);

                var pixels = new byte[size * size * 4];
                gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

                //Assert - the near (red) triangle occludes the far (blue) one at the center
                pixels[center].Should().BeGreaterThan((byte)128);
                pixels[center + 2].Should().BeLessThan((byte)128);
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

    private static LoadedModel BuildDepthOrderingModel(bool nearDrawnFirst)
    {
        //Large, origin-centered triangles so both still cover the center pixel under the
        //rotated view's parallax; only depth ordering decides which colour wins the center.
        static ModelPrimitive Triangle(float z, int materialIndex) => new()
        {
            Positions = [-3f, -2f, z, 3f, -2f, z, 0f, 3f, z],
            Normals = [0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f],
            TexCoords = [0f, 0f, 1f, 0f, 0.5f, 1f],
            Indices = [0u, 1u, 2u],
            MaterialIndex = materialIndex,
        };

        var near = Triangle(0.5f, 0);   // red
        var far = Triangle(-0.5f, 1);   // blue
        var primitives = nearDrawnFirst
            ? new List<ModelPrimitive> { near, far }
            : new List<ModelPrimitive> { far, near };

        return new LoadedModel
        {
            Primitives = primitives,
            Materials =
            [
                new ModelMaterial { BaseColorFactor = new Vector4(1f, 0f, 0f, 1f) },
                new ModelMaterial { BaseColorFactor = new Vector4(0f, 0f, 1f, 1f) },
            ],
            BoundsMin = new Vector3(-3f, -2f, -0.5f),
            BoundsMax = new Vector3(3f, 3f, 0.5f),
        };
    }
}
