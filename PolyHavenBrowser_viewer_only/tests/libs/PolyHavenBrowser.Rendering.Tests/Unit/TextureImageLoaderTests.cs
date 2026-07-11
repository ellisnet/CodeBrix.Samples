using SilverAssertions;
using SkiaSharp;
using TinyEXR;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class TextureImageLoaderTests
{
    [Fact]
    public void png_extension_routes_to_the_ldr_decoder()
    {
        //Arrange
        var png = TestAssets.BuildPng(2, 2, new SKColor(0, 0, 255));

        //Act
        using var bitmap = TextureImageLoader.LoadForDisplay(png, ".png");

        //Assert
        bitmap.GetPixel(0, 0).Blue.Should().Be(255);
    }

    [Fact]
    public void exr_extension_routes_to_normalized_display()
    {
        //Arrange - a data map ranging 0..10: normalization should span the full byte range
        Exr.SaveEXRToMemory(
            [0f, 0f, 0f, 1f, 10f, 10f, 10f, 1f],
            2, 1, 4, false, out var exr).Should().Be(ResultCode.Success);

        //Act
        using var bitmap = TextureImageLoader.LoadForDisplay(exr, "exr");

        //Assert
        bitmap.GetPixel(0, 0).Red.Should().Be(0);
        bitmap.GetPixel(1, 0).Red.Should().Be(255);
    }

    [Fact]
    public void hdr_extension_routes_to_tone_mapping()
    {
        //Arrange
        var hdr = TestAssets.BuildFlatHdr(1, 1, (128, 128, 128, 129)); // 1.0 grey

        //Act
        using var bitmap = TextureImageLoader.LoadForDisplay(hdr, ".hdr");

        //Assert
        bitmap.Width.Should().Be(1);
        bitmap.GetPixel(0, 0).Red.Should().BeGreaterThan(150); // 1.0 → bright after ACES+sRGB
    }

    [Fact]
    public void load_float_image_rejects_ldr_extensions()
    {
        //Act
        var act = () => TextureImageLoader.LoadFloatImage([1, 2, 3], ".png");

        //Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void normalization_survives_a_constant_image()
    {
        //Arrange - zero range would divide by zero without the guard
        var image = new FloatImage(2, 1, [3f, 3f, 3f, 3f, 3f, 3f]);

        //Act
        using var bitmap = TextureImageLoader.NormalizeToBitmap(image);

        //Assert
        bitmap.GetPixel(0, 0).Alpha.Should().Be(255);
    }
}
