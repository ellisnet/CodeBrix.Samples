using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class LdrImageDecoderTests
{
    [Fact]
    public void png_decodes_to_a_matching_bitmap()
    {
        //Arrange
        var png = TestAssets.BuildPng(3, 2, new SKColor(10, 200, 30));

        //Act
        using var bitmap = LdrImageDecoder.Decode(png);

        //Assert
        bitmap.Width.Should().Be(3);
        bitmap.Height.Should().Be(2);
        var pixel = bitmap.GetPixel(1, 1);
        pixel.Red.Should().Be(10);
        pixel.Green.Should().Be(200);
        pixel.Blue.Should().Be(30);
    }

    [Fact]
    public void png_decodes_to_rgba_bytes()
    {
        //Arrange
        var png = TestAssets.BuildPng(2, 1, new SKColor(255, 0, 0));

        //Act
        var (rgba, width, height) = LdrImageDecoder.DecodeToRgbaBytes(png);

        //Assert
        width.Should().Be(2);
        height.Should().Be(1);
        rgba.Should().HaveCount(8);
        rgba[0].Should().Be(255); // R
        rgba[1].Should().Be(0);   // G
        rgba[3].Should().Be(255); // A
    }

    [Fact]
    public void non_image_data_is_rejected()
    {
        //Act
        var act = () => LdrImageDecoder.Decode([1, 2, 3, 4]);

        //Assert
        act.Should().Throw<InvalidDataException>();
    }
}
