using SilverAssertions;
using System;
using System.Linq;
using Xunit;
using PalmVisualizer.Vision.Internal;

namespace PalmVisualizer.Vision.Tests;

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

    [Fact]
    public void IntersectionOverUnion_is_one_for_identical_boxes()
    {
        //Arrange
        var box = (Left: 0.2f, Top: 0.2f, Right: 0.6f, Bottom: 0.6f);

        //Assert
        (Math.Abs(PalmDetector.IntersectionOverUnion(box, box) - 1f) < 0.0001f).Should().Be(true);
    }

    [Fact]
    public void IntersectionOverUnion_is_zero_for_disjoint_boxes()
    {
        //Arrange - two palm-sized boxes on opposite sides of the frame
        var left = (Left: 0.0f, Top: 0.2f, Right: 0.3f, Bottom: 0.5f);
        var right = (Left: 0.6f, Top: 0.2f, Right: 0.9f, Bottom: 0.5f);

        //Assert
        PalmDetector.IntersectionOverUnion(left, right).Should().Be(0f);
    }

    [Fact]
    public void IntersectionOverUnion_measures_partial_overlap()
    {
        //Arrange - unit-square halves overlapping in the middle quarter:
        //  intersection 0.25, union 0.75
        var a = (Left: 0.0f, Top: 0.0f, Right: 0.5f, Bottom: 1.0f);
        var b = (Left: 0.25f, Top: 0.0f, Right: 0.75f, Bottom: 1.0f);

        //Act
        float iou = PalmDetector.IntersectionOverUnion(a, b);

        //Assert - 0.25 / 0.75
        (Math.Abs(iou - (1f / 3f)) < 0.0001f).Should().Be(true);
    }
}
