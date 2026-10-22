using System.Collections.Generic;
using System.Numerics;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Exercises <see cref="WebGpuSceneRenderer"/> against a real WebGPU device. Skips when no WebGPU
/// device can be created (no GPU driver for Direct3D 12, Vulkan or Metal). Mirrors
/// <see cref="VulkanSceneRendererTests"/>, <see cref="MetalSceneRendererTests"/> and
/// <see cref="GlModelSceneRendererTests"/> so all four backends prove the same behaviors, plus a
/// check of the top-down readback orientation the WebGPU engine reports.
/// </summary>
[Trait("Category", "RequiresWebGpu")]
public class WebGpuSceneRendererTests
{
    private static void RequireWebGpu() =>
        Assert.SkipWhen(
            !WebGpuSceneRenderer.IsRuntimeAvailable(),
            "No WebGPU device available on this machine.");

    [Fact]
    public void renderer_draws_a_triangle_onto_the_background()
    {
        //Arrange
        RequireWebGpu();
        const int size = 64;
        using var renderer = new WebGpuSceneRenderer();

        //Act - blue background, red triangle
        renderer.SetModel(TestAssets.BuildTriangleModel());
        var pixels = renderer.RenderFrame(size, size, (0f, 0f, 1f, 1f));

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

    [Fact]
    public void clearing_the_model_renders_only_the_background()
    {
        //Arrange
        RequireWebGpu();
        const int size = 16;
        using var renderer = new WebGpuSceneRenderer();

        //Act
        renderer.SetModel(TestAssets.BuildTriangleModel());
        renderer.RenderFrame(size, size, (0f, 1f, 0f, 1f));
        renderer.SetModel(null);
        var pixels = renderer.RenderFrame(size, size, (0f, 1f, 0f, 1f));

        //Assert - all pixels are the green background
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i].Should().Be(0);
            pixels[i + 1].Should().Be(255);
        }
    }

    [Fact]
    public void full_pipeline_from_glb_to_pixels()
    {
        //Arrange - end to end: CodeBrix.Graphics3D.Gltf2-built .glb → loader → Metal renderer → pixels
        RequireWebGpu();
        const int size = 64;
        var model = new GltfModelLoader().Load(new MemoryStream(TestAssets.BuildTriangleGlb()));
        using var renderer = new WebGpuSceneRenderer();

        //Act
        renderer.SetModel(model);
        var pixels = renderer.RenderFrame(size, size, (0f, 0f, 0f, 1f));

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

    [Fact]
    public void nearer_geometry_occludes_farther_geometry_regardless_of_draw_order()
    {
        //Arrange - the same rotated-camera depth-ordering regression as the GL/Vulkan/Metal renderers:
        //a bad MVP transpose collapses the depth axis only for non-axis-aligned cameras, so a
        //near red triangle must beat a far blue one at the center from a rotated view no matter
        //the draw order.
        RequireWebGpu();
        const int size = 32;
        var center = ((size / 2) * size + (size / 2)) * 4;

        foreach (var nearDrawnFirst in new[] { false, true })
        {
            using var renderer = new WebGpuSceneRenderer();

            //Act
            renderer.SetModel(BuildDepthOrderingModel(nearDrawnFirst));
            renderer.Camera.YawDegrees = 35f;
            renderer.Camera.PitchDegrees = 28f;
            var pixels = renderer.RenderFrame(size, size, (0f, 0f, 0f, 1f));

            //Assert - the near (red) triangle occludes the far (blue) one at the center
            pixels[center].Should().BeGreaterThan((byte)128);
            pixels[center + 2].Should().BeLessThan((byte)128);
        }
    }

    [Fact]
    public void textured_material_shows_its_texture_color()
    {
        //Arrange - a triangle whose material carries a solid yellow 2x2 base-color texture,
        //proving the queue texture upload, sampler, and texture binding work.
        RequireWebGpu();
        const int size = 64;
        byte[] yellow = [255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255];
        var model = new LoadedModel
        {
            Primitives =
            [
                new ModelPrimitive
                {
                    Positions = [0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f],
                    Normals = [0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f],
                    TexCoords = [0f, 0f, 1f, 0f, 0f, 1f],
                    Indices = [0u, 1u, 2u],
                    MaterialIndex = 0,
                },
            ],
            Materials =
            [
                new ModelMaterial
                {
                    BaseColorFactor = Vector4.One,
                    BaseColorTextureRgba = yellow,
                    BaseColorTextureWidth = 2,
                    BaseColorTextureHeight = 2,
                },
            ],
            BoundsMin = Vector3.Zero,
            BoundsMax = new Vector3(1f, 1f, 0f),
        };
        using var renderer = new WebGpuSceneRenderer();

        //Act - on a blue background
        renderer.SetModel(model);
        var pixels = renderer.RenderFrame(size, size, (0f, 0f, 1f, 1f));

        //Assert - yellow-ish (red+green, little blue) pixels appear
        var sawYellow = false;
        for (var i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i] > 100 && pixels[i + 1] > 100 && pixels[i + 2] < 100)
            {
                sawYellow = true;
                break;
            }
        }
        sawYellow.Should().BeTrue();
    }

    [Fact]
    public void resizing_between_frames_renders_at_the_new_size()
    {
        //Arrange - the offscreen targets are recreated on a size change
        RequireWebGpu();
        using var renderer = new WebGpuSceneRenderer();
        renderer.SetModel(TestAssets.BuildTriangleModel());

        //Act
        var small = renderer.RenderFrame(16, 16, (0f, 0f, 1f, 1f));
        var large = renderer.RenderFrame(64, 32, (0f, 0f, 1f, 1f));

        //Assert
        small.Length.Should().Be(16 * 16 * 4);
        large.Length.Should().Be(64 * 32 * 4);
    }

    [Fact]
    public void readback_is_top_down()
    {
        //Arrange - a red triangle entirely above the framing center, seen head-on: it must land in
        //the first rows of the readback, because the WebGPU engine reports IsBottomUp: false.
        RequireWebGpu();
        const int size = 32;
        var model = new LoadedModel
        {
            Primitives =
            [
                new ModelPrimitive
                {
                    Positions = [-1f, 0.2f, 0f, 1f, 0.2f, 0f, 0f, 1f, 0f],
                    Normals = [0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f],
                    TexCoords = new float[6],
                    Indices = [0u, 1u, 2u],
                    MaterialIndex = 0,
                },
            ],
            Materials = [new ModelMaterial { BaseColorFactor = new Vector4(1f, 0f, 0f, 1f) }],
            BoundsMin = new Vector3(-1f, -1f, 0f),
            BoundsMax = new Vector3(1f, 1f, 0f),
        };
        using var renderer = new WebGpuSceneRenderer();
        renderer.Camera.YawDegrees = 0f;
        renderer.Camera.PitchDegrees = 0f;

        //Act
        renderer.SetModel(model);
        var pixels = renderer.RenderFrame(size, size, (0f, 0f, 0f, 1f));

        //Assert - red in the top half, none in the bottom half
        static bool IsRed(byte[] p, int i) => p[i] > 100 && p[i + 2] < 50;
        var redTop = false;
        var redBottom = false;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                if (!IsRed(pixels, (y * size + x) * 4)) { continue; }
                if (y < size / 2) { redTop = true; } else { redBottom = true; }
            }
        }
        redTop.Should().BeTrue();
        redBottom.Should().BeFalse();
    }

    private static LoadedModel BuildDepthOrderingModel(bool nearDrawnFirst)
    {
        //Large, origin-centered triangles so both still cover the center pixel under the rotated
        //view's parallax; only depth ordering decides which colour wins the center.
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
