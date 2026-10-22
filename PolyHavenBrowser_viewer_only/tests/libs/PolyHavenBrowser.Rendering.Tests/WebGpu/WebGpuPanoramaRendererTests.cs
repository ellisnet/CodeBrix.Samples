using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Exercises <see cref="WebGpuPanoramaRenderer"/> against a real WebGPU device, chiefly by proving
/// it draws the same picture as the CPU <see cref="EquirectPanoramaRenderer"/> it stands in for.
/// Skips when no WebGPU device can be created.
/// </summary>
[Trait("Category", "RequiresWebGpu")]
public class WebGpuPanoramaRendererTests
{
    private static void RequireWebGpu() =>
        Assert.SkipWhen(!WebGpuSupport.IsRuntimeAvailable(), "No WebGPU device available on this machine.");

    // A smooth HDR panorama (values from 0 to ~4, varying in both directions, wrapping cleanly in x)
    // so bilinear filtering and the seam both matter, with no sharp edges for the comparison to trip on.
    private static FloatImage GradientPanorama(int width = 64, int height = 32)
    {
        var pixels = new float[width * height * 3];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = (y * width + x) * 3;
                var u = x / (float)width;
                var v = y / (float)(height - 1);
                pixels[i] = 2f + 2f * MathF.Sin(u * MathF.PI * 2f);
                pixels[i + 1] = 4f * v;
                pixels[i + 2] = 1f + MathF.Cos(u * MathF.PI * 4f);
            }
        }
        return new FloatImage(width, height, pixels);
    }

    [Theory]
    [InlineData(0f, 0f, 75f, ToneMapOperator.AcesFilmic)]
    [InlineData(170f, 35f, 60f, ToneMapOperator.Reinhard)]
    [InlineData(-95f, -60f, 110f, ToneMapOperator.Clamp)]
    public void gpu_render_matches_the_cpu_renderer(float yaw, float pitch, float fov, ToneMapOperator op)
    {
        //Arrange
        RequireWebGpu();
        var panorama = GradientPanorama();
        using var gpu = new WebGpuPanoramaRenderer(panorama);
        var cpu = new EquirectPanoramaRenderer(panorama);
        foreach (var camera in new[] { gpu.Camera, cpu.Camera })
        {
            camera.YawDegrees = yaw;
            camera.PitchDegrees = pitch;
            camera.FovDegrees = fov;
        }
        gpu.Operator = cpu.Operator = op;
        gpu.Exposure = cpu.Exposure = 0.8f;

        //Act
        using var gpuBitmap = gpu.Render(48, 32);
        using var cpuBitmap = cpu.Render(48, 32);

        //Assert - every pixel within a few levels (half-float texels and GPU filtering precision)
        var worst = 0;
        for (var y = 0; y < 32; y++)
        {
            for (var x = 0; x < 48; x++)
            {
                var a = gpuBitmap.GetPixel(x, y);
                var b = cpuBitmap.GetPixel(x, y);
                worst = Math.Max(worst, Math.Abs(a.Red - b.Red));
                worst = Math.Max(worst, Math.Abs(a.Green - b.Green));
                worst = Math.Max(worst, Math.Abs(a.Blue - b.Blue));
                a.Alpha.Should().Be(255);
            }
        }
        worst.Should().BeLessThanOrEqualTo(4);
    }

    [Fact]
    public void render_to_reuses_an_existing_bitmap_and_reacts_to_the_camera()
    {
        //Arrange
        RequireWebGpu();
        using var renderer = new WebGpuPanoramaRenderer(GradientPanorama());
        using var bitmap = new SKBitmap(new SKImageInfo(40, 20, SKColorType.Rgba8888, SKAlphaType.Opaque));

        //Act
        renderer.RenderTo(bitmap);
        var before = bitmap.GetPixel(20, 10);
        renderer.Camera.Rotate(90f, 0f);
        renderer.RenderTo(bitmap);
        var after = bitmap.GetPixel(20, 10);

        //Assert
        after.Should().NotBe(before);
    }

    [Fact]
    public void render_to_rejects_non_rgba_bitmaps()
    {
        //Arrange
        RequireWebGpu();
        using var renderer = new WebGpuPanoramaRenderer(GradientPanorama());
        using var bitmap = new SKBitmap(new SKImageInfo(8, 8, SKColorType.Bgra8888, SKAlphaType.Opaque));

        //Act
        var act = () => renderer.RenderTo(bitmap);

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void oversized_panoramas_are_halved_until_they_fit_the_device()
    {
        //Arrange - no GPU needed: this is the CPU-side fit done before the upload
        var panorama = GradientPanorama(64, 32);

        //Act
        var fitted = WebGpuPanoramaRenderer.FitToDevice(panorama, 20);
        var untouched = WebGpuPanoramaRenderer.FitToDevice(panorama, 64);

        //Assert
        fitted.Width.Should().Be(16);
        fitted.Height.Should().Be(8);
        untouched.Should().BeSameAs(panorama);
    }
}
