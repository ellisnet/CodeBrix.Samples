using SilverAssertions;
using System.Text;
using Xunit;

namespace KenneyAssetBrowser.Rendering.Tests;

public class SvgImageDecoderTests
{
    private const string SquareSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"8\">" +
        "<rect x=\"0\" y=\"0\" width=\"16\" height=\"8\" fill=\"#FF0000\"/></svg>";

    [Fact]
    public void Svg_rasterizes_at_the_requested_size_preserving_aspect()
    {
        //Act
        using var bitmap = SvgImageDecoder.Render(Encoding.UTF8.GetBytes(SquareSvg), maxDimension: 64);

        //Assert
        bitmap.Width.Should().Be(64);
        bitmap.Height.Should().Be(32);
        bitmap.GetPixel(32, 16).Red.Should().Be(0xFF);
        bitmap.GetPixel(32, 16).Alpha.Should().Be(0xFF);
    }

    [Fact]
    public void Svg_renders_to_png_bytes_for_thumbnails()
    {
        //Act
        var png = SvgImageDecoder.RenderToPngBytes(Encoding.UTF8.GetBytes(SquareSvg), maxDimension: 32);

        //Assert — PNG magic header
        png.Length.Should().BeGreaterThan(8);
        png[1].Should().Be((byte)'P');
        png[2].Should().Be((byte)'N');
        png[3].Should().Be((byte)'G');
    }

    [Fact]
    public void Non_svg_bytes_are_rejected()
    {
        //Act + Assert
        Assert.Throws<InvalidDataException>(() =>
            SvgImageDecoder.Render(Encoding.UTF8.GetBytes("this is not svg"), maxDimension: 64));
    }
}
