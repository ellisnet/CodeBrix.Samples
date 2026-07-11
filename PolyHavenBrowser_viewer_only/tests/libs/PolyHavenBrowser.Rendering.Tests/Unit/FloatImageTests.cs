using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class FloatImageTests
{
    [Fact]
    public void pixel_buffer_length_must_match_dimensions()
    {
        //Act
        var act = () => new FloatImage(2, 2, new float[5]);

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void get_pixel_reads_the_expected_floats()
    {
        //Arrange
        var image = new FloatImage(2, 1, [0.1f, 0.2f, 0.3f, 1f, 2f, 3f]);

        //Act
        image.GetPixel(1, 0, out var r, out var g, out var b);

        //Assert
        r.Should().Be(1f);
        g.Should().Be(2f);
        b.Should().Be(3f);
    }

    [Fact]
    public void from_rgba_drops_the_alpha_channel()
    {
        //Arrange
        float[] rgba = [1f, 2f, 3f, 0.5f, 4f, 5f, 6f, 0.25f];

        //Act
        var image = FloatImage.FromRgba(rgba, 2, 1);

        //Assert
        image.Pixels.Should().Equal(1f, 2f, 3f, 4f, 5f, 6f);
    }
}
