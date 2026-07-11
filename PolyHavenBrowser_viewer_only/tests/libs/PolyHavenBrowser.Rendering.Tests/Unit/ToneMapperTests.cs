using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class ToneMapperTests
{
    [Fact]
    public void zero_and_negative_inputs_map_to_zero()
    {
        //Assert
        ToneMapper.ApplyOperator(0f, ToneMapOperator.AcesFilmic).Should().Be(0f);
        ToneMapper.ApplyOperator(-1f, ToneMapOperator.Reinhard).Should().Be(0f);
        ToneMapper.ToSrgbByte(0f).Should().Be(0);
    }

    [Fact]
    public void reinhard_maps_one_to_a_half()
    {
        //Assert
        ToneMapper.ApplyOperator(1f, ToneMapOperator.Reinhard).Should().BeApproximately(0.5f, 1e-6f);
    }

    [Fact]
    public void clamp_operator_just_clamps()
    {
        //Assert
        ToneMapper.ApplyOperator(0.25f, ToneMapOperator.Clamp).Should().Be(0.25f);
        ToneMapper.ApplyOperator(7f, ToneMapOperator.Clamp).Should().Be(1f);
    }

    [Fact]
    public void aces_is_monotonic_and_bounded()
    {
        //Arrange / Act
        var previous = 0f;
        for (var value = 0.05f; value < 40f; value *= 1.5f)
        {
            var mapped = ToneMapper.ApplyOperator(value, ToneMapOperator.AcesFilmic);

            //Assert
            mapped.Should().BeGreaterThanOrEqualTo(previous);
            mapped.Should().BeInRange(0f, 1f);
            previous = mapped;
        }

        previous.Should().BeGreaterThan(0.95f); // very bright input ends up near white
    }

    [Fact]
    public void srgb_transfer_hits_the_known_anchor_points()
    {
        //Assert
        ToneMapper.ToSrgbByte(1f).Should().Be(255);
        ToneMapper.ToSrgbByte(0.5f).Should().Be(188); // round(255 * (1.055*0.5^(1/2.4)-0.055))
    }

    [Fact]
    public void exposure_scales_the_input_before_the_operator()
    {
        //Act
        ToneMapper.MapPixel(0.25f, 0.25f, 0.25f, exposure: 4f, ToneMapOperator.Clamp, out var r4, out _, out _);
        ToneMapper.MapPixel(1f, 1f, 1f, exposure: 1f, ToneMapOperator.Clamp, out var r1, out _, out _);

        //Assert
        r4.Should().Be(r1);
    }

    [Fact]
    public void to_bitmap_produces_opaque_pixels_of_the_right_size()
    {
        //Arrange - 2x1: black then heavily overexposed white
        var image = new FloatImage(2, 1, [0f, 0f, 0f, 50f, 50f, 50f]);

        //Act
        using var bitmap = ToneMapper.ToBitmap(image, exposure: 1f, ToneMapOperator.AcesFilmic);

        //Assert
        bitmap.Width.Should().Be(2);
        bitmap.Height.Should().Be(1);
        var dark = bitmap.GetPixel(0, 0);
        var bright = bitmap.GetPixel(1, 0);
        dark.Red.Should().Be(0);
        dark.Alpha.Should().Be(255);
        bright.Red.Should().BeGreaterThan(240);
        bright.Alpha.Should().Be(255);
    }
}
