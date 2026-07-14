using SilverAssertions;
using System;
using System.Linq;
using Xunit;
using WebcamPainter.Vision.Internal;

namespace WebcamPainter.Vision.Tests;

public class PalmDetectorTests
{
    [Fact]
    public void Anchor_grid_matches_the_models_2016_output_rows()
    {
        PalmDetector.TestAnchorsX.Length.Should().Be(2016);
        PalmDetector.TestAnchorsY.Length.Should().Be(2016);
    }

    [Fact]
    public void Anchor_centers_stay_inside_the_unit_square()
    {
        //Assert
        PalmDetector.TestAnchorsX.All(x => x > 0f && x < 1f).Should().Be(true);
        PalmDetector.TestAnchorsY.All(y => y > 0f && y < 1f).Should().Be(true);
    }

    [Fact]
    public void Anchor_grid_starts_with_the_stride8_24x24_block()
    {
        //Assert - two anchors per cell, both centered at cell (0,0) of the 24-cell grid
        PalmDetector.TestAnchorsX[0].Should().Be(0.5f / 24f);
        PalmDetector.TestAnchorsX[1].Should().Be(0.5f / 24f);
        PalmDetector.TestAnchorsX[2].Should().Be(1.5f / 24f);
        //...and the stride-16 block (12x12 cells x 6 anchors) fills the remainder
        PalmDetector.TestAnchorsX[24 * 24 * 2].Should().Be(0.5f / 12f);
    }

    [Fact]
    public void Sigmoid_maps_logits_to_probabilities()
    {
        PalmDetector.Sigmoid(0f).Should().Be(0.5f);
        (PalmDetector.Sigmoid(10f) > 0.999f).Should().Be(true);
        (PalmDetector.Sigmoid(-10f) < 0.001f).Should().Be(true);
    }

    [Fact]
    public void NormalizeRadians_wraps_into_the_plus_minus_pi_range()
    {
        //Arrange
        var threePi = (float)(3 * Math.PI);

        //Assert - 3pi wraps to pi (or -pi; both describe the same angle)
        float wrapped = PalmDetector.NormalizeRadians(threePi);
        (Math.Abs(Math.Abs(wrapped) - (float)Math.PI) < 0.001f).Should().Be(true);
        PalmDetector.NormalizeRadians(0.5f).Should().Be(0.5f);
    }
}
