using System.Numerics;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class PanoramaCameraTests
{
    [Fact]
    public void pitch_is_clamped_to_avoid_the_poles()
    {
        //Arrange
        var camera = new PanoramaCamera();

        //Act
        camera.Rotate(0f, 500f);

        //Assert
        camera.PitchDegrees.Should().Be(89f);
    }

    [Fact]
    public void fov_zoom_is_clamped()
    {
        //Arrange
        var camera = new PanoramaCamera();

        //Act
        camera.Zoom(-500f);
        var narrow = camera.FovDegrees;
        camera.Zoom(500f);
        var wide = camera.FovDegrees;

        //Assert
        narrow.Should().Be(20f);
        wide.Should().Be(120f);
    }
}

public class EquirectPanoramaRendererTests
{
    /// <summary>A 4x2 panorama: top row white, bottom row black (values in HDR range).</summary>
    private static FloatImage TopBrightPanorama()
    {
        var pixels = new float[4 * 2 * 3];
        for (var i = 0; i < 4 * 3; i++)
        {
            pixels[i] = 1f; // top row
        }
        return new FloatImage(4, 2, pixels);
    }

    [Fact]
    public void looking_up_samples_the_top_of_the_panorama()
    {
        //Arrange
        var renderer = new EquirectPanoramaRenderer(TopBrightPanorama());

        //Act
        renderer.SampleDirection(new Vector3(0f, 1f, 0f), out var upR, out _, out _);
        renderer.SampleDirection(new Vector3(0f, -1f, 0f), out var downR, out _, out _);

        //Assert
        upR.Should().Be(1f);
        downR.Should().Be(0f);
    }

    [Fact]
    public void horizontal_directions_blend_the_two_rows()
    {
        //Arrange
        var renderer = new EquirectPanoramaRenderer(TopBrightPanorama());

        //Act - the horizon sits exactly between the two rows
        renderer.SampleDirection(new Vector3(0f, 0f, -1f), out var r, out _, out _);

        //Assert
        r.Should().BeApproximately(0.5f, 1e-5f);
    }

    [Fact]
    public void horizontal_sampling_wraps_around_the_seam()
    {
        //Arrange - column 0 red, others black; 4x1
        var pixels = new float[4 * 3];
        pixels[0] = 1f;
        var renderer = new EquirectPanoramaRenderer(new FloatImage(4, 1, pixels));

        //Act - +Z looks at the seam (u = 0/1), whose nearest columns are 3 and 0
        renderer.SampleDirection(new Vector3(0f, 0f, 1f), out var r, out _, out _);

        //Assert - bilinear blend of column 3 (black) and column 0 (red)
        r.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void render_fills_an_opaque_bitmap_and_reacts_to_the_camera()
    {
        //Arrange
        var renderer = new EquirectPanoramaRenderer(TopBrightPanorama());
        renderer.Camera.FovDegrees = 60f;

        //Act - look up (bright) vs look down (dark)
        renderer.Camera.PitchDegrees = 80f;
        using var lookingUp = renderer.Render(16, 12);
        renderer.Camera.PitchDegrees = -80f;
        using var lookingDown = renderer.Render(16, 12);

        //Assert
        lookingUp.Width.Should().Be(16);
        lookingUp.GetPixel(8, 6).Alpha.Should().Be(255);
        lookingUp.GetPixel(8, 6).Red.Should().BeGreaterThan(lookingDown.GetPixel(8, 6).Red);
    }

    [Fact]
    public void render_to_reuses_an_existing_bitmap()
    {
        //Arrange
        var renderer = new EquirectPanoramaRenderer(TopBrightPanorama());
        using var bitmap = new SKBitmap(new SKImageInfo(8, 8, SKColorType.Rgba8888, SKAlphaType.Opaque));

        //Act
        renderer.RenderTo(bitmap);

        //Assert
        bitmap.GetPixel(4, 4).Alpha.Should().Be(255);
    }

    [Fact]
    public void render_to_rejects_non_rgba_bitmaps()
    {
        //Arrange
        var renderer = new EquirectPanoramaRenderer(TopBrightPanorama());
        using var bitmap = new SKBitmap(new SKImageInfo(4, 4, SKColorType.Bgra8888));

        //Act
        var act = () => renderer.RenderTo(bitmap);

        //Assert
        act.Should().Throw<ArgumentException>();
    }
}
