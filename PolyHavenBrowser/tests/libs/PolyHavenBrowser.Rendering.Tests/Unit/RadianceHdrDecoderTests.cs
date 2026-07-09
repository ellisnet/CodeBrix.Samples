using System.Text;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class RadianceHdrDecoderTests
{
    [Fact]
    public void rgbe_conversion_matches_the_shared_exponent_formula()
    {
        //Act - mantissa 128 with exponent 129 → 128 * 2^(129-136) = 1.0
        RadianceHdrDecoder.RgbeToFloat(128, 64, 0, 129, out var r, out var g, out var b);

        //Assert
        r.Should().Be(1.0f);
        g.Should().Be(0.5f);
        b.Should().Be(0f);
    }

    [Fact]
    public void zero_exponent_decodes_to_black()
    {
        //Act
        RadianceHdrDecoder.RgbeToFloat(200, 200, 200, 0, out var r, out var g, out var b);

        //Assert
        r.Should().Be(0f);
        g.Should().Be(0f);
        b.Should().Be(0f);
    }

    [Fact]
    public void flat_scanlines_decode_with_correct_dimensions_and_values()
    {
        //Arrange - 2x2: one bright red pixel, three black
        var hdr = TestAssets.BuildFlatHdr(2, 2,
            (128, 0, 0, 130), // 128 * 2^-6 = 2.0 → HDR value beyond display range
            (0, 0, 0, 0),
            (0, 0, 0, 0),
            (0, 0, 0, 0));

        //Act
        var image = RadianceHdrDecoder.Decode(hdr);

        //Assert
        image.Width.Should().Be(2);
        image.Height.Should().Be(2);
        image.GetPixel(0, 0, out var r, out var g, out var b);
        r.Should().Be(2.0f);
        g.Should().Be(0f);
        b.Should().Be(0f);
    }

    [Fact]
    public void old_style_repeat_runs_are_expanded()
    {
        //Arrange - width 4: one real pixel then (1,1,1,2) meaning "repeat previous twice", then one more
        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 1 +X 4\n"));
        stream.Write([128, 0, 0, 129]); // 1.0 red
        stream.Write([1, 1, 1, 2]);     // repeat previous pixel 2 times
        stream.Write([0, 128, 0, 129]); // 0.5... green 1.0? mantissa 128 → 1.0 green
        stream.Position = 0;

        //Act
        var image = RadianceHdrDecoder.Decode(stream);

        //Assert
        for (var x = 0; x < 3; x++)
        {
            image.GetPixel(x, 0, out var r, out _, out _);
            r.Should().Be(1.0f);
        }
        image.GetPixel(3, 0, out _, out var g3, out _);
        g3.Should().Be(1.0f);
    }

    [Fact]
    public void new_style_rle_scanlines_decode()
    {
        //Arrange - width 8 (minimum for new RLE): magic 2,2,width then 4 planar components,
        // each a single run of 8 bytes.
        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 1 +X 8\n"));
        stream.Write([2, 2, 0, 8]);
        stream.Write([136, 128]); // R plane: run of 8 × 128
        stream.Write([136, 0]);   // G plane: run of 8 × 0
        stream.Write([136, 0]);   // B plane: run of 8 × 0
        stream.Write([136, 129]); // E plane: run of 8 × 129
        stream.Position = 0;

        //Act
        var image = RadianceHdrDecoder.Decode(stream);

        //Assert
        image.Width.Should().Be(8);
        for (var x = 0; x < 8; x++)
        {
            image.GetPixel(x, 0, out var r, out var g, out var b);
            r.Should().Be(1.0f);
            g.Should().Be(0f);
            b.Should().Be(0f);
        }
    }

    [Fact]
    public void new_style_rle_literal_packets_decode()
    {
        //Arrange - width 8, R plane split into literal(3) + run(5); other planes single runs.
        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 1 +X 8\n"));
        stream.Write([2, 2, 0, 8]);
        stream.Write([3, 10, 20, 30, 133, 40]); // R: literals 10,20,30 then run of 5 × 40
        stream.Write([136, 0]);
        stream.Write([136, 0]);
        stream.Write([136, 129]);
        stream.Position = 0;

        //Act
        var image = RadianceHdrDecoder.Decode(stream);

        //Assert - scale at e=129 is 2^-7 = 1/128
        image.GetPixel(0, 0, out var r0, out _, out _);
        image.GetPixel(2, 0, out var r2, out _, out _);
        image.GetPixel(7, 0, out var r7, out _, out _);
        r0.Should().BeApproximately(10f / 128f, 1e-6f);
        r2.Should().BeApproximately(30f / 128f, 1e-6f);
        r7.Should().BeApproximately(40f / 128f, 1e-6f);
    }

    [Fact]
    public void missing_signature_is_rejected()
    {
        //Arrange
        var bogus = Encoding.ASCII.GetBytes("not an hdr file at all");

        //Act
        var act = () => RadianceHdrDecoder.Decode(bogus);

        //Assert
        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void truncated_pixel_data_is_rejected()
    {
        //Arrange - header promises 2x2 but only one pixel follows
        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 2 +X 2\n"));
        stream.Write([128, 0, 0, 129]);
        stream.Position = 0;

        //Act
        var act = () => RadianceHdrDecoder.Decode(stream);

        //Assert
        act.Should().Throw<InvalidDataException>();
    }
}
