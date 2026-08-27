using PdfSideBySide.PdfRender.Viewing;
using SilverAssertions;
using System;
using Xunit;

namespace PdfSideBySide.PdfRender.Tests;

public class ViewZoomTests
{
    [Fact]
    public void a_new_zoom_fits_the_page()
    {
        //Act
        var zoom = new ViewZoom();

        //Assert
        zoom.Percent.Should().Be(100);
        zoom.Factor.Should().Be(1.0);
        zoom.IsZoomedIn.Should().BeFalse();
        zoom.CanZoomOut.Should().BeFalse();
        zoom.CanZoomIn.Should().BeTrue();
    }

    [Fact]
    public void the_ladder_runs_from_one_hundred_to_one_thousand_percent()
    {
        //Assert
        ViewZoom.Levels[0].Should().Be(ViewZoom.MinimumPercent);
        ViewZoom.Levels[^1].Should().Be(ViewZoom.MaximumPercent);
        for (var i = 1; i < ViewZoom.Levels.Count; i++)
        {
            ViewZoom.Levels[i].Should().BeGreaterThan(ViewZoom.Levels[i - 1]);
        }
    }

    [Fact]
    public void zoom_in_climbs_the_ladder_one_level_at_a_time()
    {
        //Arrange
        var zoom = new ViewZoom();

        //Act
        var changed = zoom.ZoomIn();

        //Assert
        changed.Should().BeTrue();
        zoom.Percent.Should().Be(ViewZoom.Levels[1]);
        zoom.IsZoomedIn.Should().BeTrue();
        zoom.CanZoomOut.Should().BeTrue();
    }

    [Fact]
    public void zoom_in_stops_at_the_top_of_the_ladder()
    {
        //Arrange
        var zoom = new ViewZoom();
        while (zoom.ZoomIn()) { }

        //Act
        var changed = zoom.ZoomIn();

        //Assert
        changed.Should().BeFalse();
        zoom.Percent.Should().Be(ViewZoom.MaximumPercent);
        zoom.CanZoomIn.Should().BeFalse();
    }

    [Fact]
    public void zoom_out_never_goes_below_fit_the_page()
    {
        //Arrange
        var zoom = new ViewZoom();

        //Act
        var changed = zoom.ZoomOut();

        //Assert
        changed.Should().BeFalse();
        zoom.Percent.Should().Be(100);
    }

    [Fact]
    public void zoom_out_retraces_zoom_in()
    {
        //Arrange
        var zoom = new ViewZoom();
        zoom.ZoomIn();
        zoom.ZoomIn();
        zoom.ZoomIn();

        //Act
        zoom.ZoomOut();

        //Assert
        zoom.Percent.Should().Be(ViewZoom.Levels[2]);
    }

    [Fact]
    public void reset_returns_to_one_hundred_percent()
    {
        //Arrange
        var zoom = new ViewZoom();
        zoom.ZoomIn();
        zoom.ZoomIn();

        //Act
        var changed = zoom.Reset();
        var changedAgain = zoom.Reset();

        //Assert
        changed.Should().BeTrue();
        changedAgain.Should().BeFalse();
        zoom.Percent.Should().Be(100);
        zoom.IsZoomedIn.Should().BeFalse();
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(200, 300)]
    [InlineData(400, 600)]
    [InlineData(500, 600)]
    [InlineData(1000, 600)]
    public void render_dpi_scales_with_the_zoom_and_caps_at_the_maximum(int percent, int expectedDpi)
    {
        //Arrange
        var zoom = new ViewZoom();
        while (zoom.Percent < percent) { zoom.ZoomIn(); }

        //Act
        var dpi = zoom.GetRenderDpi(150);

        //Assert
        zoom.Percent.Should().Be(percent);
        dpi.Should().Be(expectedDpi);
    }

    [Fact]
    public void render_dpi_rejects_a_base_dpi_below_one()
    {
        //Arrange
        var zoom = new ViewZoom();

        //Act
        Action act = () => zoom.GetRenderDpi(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
