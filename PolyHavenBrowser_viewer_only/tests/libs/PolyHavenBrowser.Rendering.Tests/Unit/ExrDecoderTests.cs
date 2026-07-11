using SilverAssertions;
using TinyEXR;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class ExrDecoderTests
{
    private static byte[] BuildExr(int width, int height, float[] rgba)
    {
        var result = Exr.SaveEXRToMemory(rgba, width, height, 4, false, out var bytes);
        result.Should().Be(ResultCode.Success);
        return bytes;
    }

    [Fact]
    public void exr_roundtrip_preserves_hdr_values()
    {
        //Arrange - 2x1: (2.5, 0.5, 0.125) and (0, 1, 4)
        var exr = BuildExr(2, 1,
        [
            2.5f, 0.5f, 0.125f, 1f,
            0f, 1f, 4f, 1f,
        ]);

        //Act
        var image = ExrDecoder.Decode(exr);

        //Assert
        image.Width.Should().Be(2);
        image.Height.Should().Be(1);
        image.GetPixel(0, 0, out var r0, out var g0, out var b0);
        r0.Should().BeApproximately(2.5f, 1e-4f);
        g0.Should().BeApproximately(0.5f, 1e-4f);
        b0.Should().BeApproximately(0.125f, 1e-4f);
        image.GetPixel(1, 0, out _, out _, out var b1);
        b1.Should().BeApproximately(4f, 1e-4f);
    }

    [Fact]
    public void stream_overload_decodes_too()
    {
        //Arrange
        var exr = BuildExr(1, 1, [1f, 2f, 3f, 1f]);
        using var stream = new MemoryStream(exr);

        //Act
        var image = ExrDecoder.Decode(stream);

        //Assert
        image.GetPixel(0, 0, out var r, out var g, out var b);
        g.Should().BeApproximately(2f, 1e-4f);
    }

    [Fact]
    public void invalid_data_is_rejected()
    {
        //Act
        var act = () => ExrDecoder.Decode([1, 2, 3, 4, 5, 6, 7, 8]);

        //Assert
        act.Should().Throw<InvalidDataException>();
    }
}
