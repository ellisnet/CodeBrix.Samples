using System;
using BrixInvaders.Game.Rendering;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Rendering;

public class PlayfieldTransformTests
{
    [Fact]
    public void Identity_maps_world_to_screen_unchanged()
    {
        //Act
        var transform = PlayfieldTransform.Identity;

        //Assert
        transform.Scale.Should().Be(1.0);
        transform.OffsetX.Should().Be(0);
        transform.OffsetY.Should().Be(0);
        transform.ToScreenX(123).Should().Be(123);
    }

    [Fact]
    public void Fit_pillarboxes_a_wider_target()
    {
        //Act
        var transform = PlayfieldTransform.Fit(1920, 720);

        //Assert
        transform.Scale.Should().Be(1.0);
        transform.OffsetX.Should().Be(320);
        transform.OffsetY.Should().Be(0);
    }

    [Fact]
    public void Fit_letterboxes_a_taller_target()
    {
        //Act
        var transform = PlayfieldTransform.Fit(1280, 1024);

        //Assert
        transform.Scale.Should().Be(1.0);
        transform.OffsetX.Should().Be(0);
        transform.OffsetY.Should().Be(152);
    }

    [Fact]
    public void Fit_scales_uniformly_into_a_larger_window()
    {
        //Act
        var transform = PlayfieldTransform.Fit(2560, 1600);

        //Assert
        transform.Scale.Should().Be(2.0);
        transform.ContentWidth.Should().Be(2560);
        transform.ContentHeight.Should().Be(1440);
        transform.OffsetY.Should().Be(80);
    }

    [Fact]
    public void screen_and_world_round_trip()
    {
        //Arrange
        var transform = PlayfieldTransform.Fit(1000, 900);

        //Act
        var x = transform.ToWorldX(transform.ToScreenX(640.5));
        var y = transform.ToWorldY(transform.ToScreenY(33.25));

        //Assert
        x.Should().BeApproximately(640.5, 1e-9);
        y.Should().BeApproximately(33.25, 1e-9);
    }

    [Fact]
    public void IsOnPlayfield_rejects_the_letterbox_bars()
    {
        //Arrange
        var transform = PlayfieldTransform.Fit(1920, 720);

        //Act
        var bar = transform.IsOnPlayfield(100, 360);
        var field = transform.IsOnPlayfield(960, 360);

        //Assert
        bar.Should().BeFalse();
        field.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 720)]
    [InlineData(1280, -1)]
    [InlineData(double.NaN, 720)]
    [InlineData(double.PositiveInfinity, 720)]
    public void Fit_refuses_an_empty_target(double width, double height) =>
        new Action(() => PlayfieldTransform.Fit(width, height)).Should().Throw<ArgumentOutOfRangeException>();
}
