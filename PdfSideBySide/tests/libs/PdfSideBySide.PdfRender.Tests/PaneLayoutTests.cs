using PdfSideBySide.PdfRender.Viewing;
using SilverAssertions;
using Xunit;

namespace PdfSideBySide.PdfRender.Tests;

public class PaneLayoutTests
{
    //A tall page and a square viewer, so the fit is decided by the height
    private const double PageWidth = 600;
    private const double PageHeight = 800;
    private const double ViewportSide = 300;

    [Fact]
    public void Create_fits_the_whole_page_in_the_viewer_at_a_factor_of_one()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var layout = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 1, pan);

        //Assert - 800 tall into 300 is a scale of 0.375, so nothing overflows and nothing scrolls
        layout.HasImage.Should().BeTrue();
        layout.ImageWidth.Should().Be(225);
        layout.ImageHeight.Should().Be(300);
        layout.ScrollOffsetX.Should().Be(0);
        layout.ScrollOffsetY.Should().Be(0);
    }

    [Fact]
    public void Create_multiplies_the_fitted_size_by_the_zoom_factor()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var layout = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 2, pan);

        //Assert
        layout.ImageWidth.Should().Be(450);
        layout.ImageHeight.Should().Be(600);
    }

    [Fact]
    public void Create_puts_a_centred_pan_in_the_middle_of_what_overflows()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var layout = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 2, pan);

        //Assert - 450 x 600 in a 300 x 300 viewer overflows by 150 x 300, and half of that shows
        layout.ScrollOffsetX.Should().Be(75);
        layout.ScrollOffsetY.Should().Be(150);
    }

    [Fact]
    public void Create_turns_the_pan_fractions_into_offsets_of_the_overflow()
    {
        //Arrange - hard against the top left corner, then hard against the bottom right
        var topLeft = new PanPosition();
        topLeft.Move(PanDirection.Up, 1);
        topLeft.Move(PanDirection.Left, 1);
        var bottomRight = new PanPosition();
        bottomRight.Move(PanDirection.Down, 1);
        bottomRight.Move(PanDirection.Right, 1);

        //Act
        var atTopLeft = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 2, topLeft);
        var atBottomRight = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 2, bottomRight);

        //Assert
        atTopLeft.ScrollOffsetX.Should().Be(0);
        atTopLeft.ScrollOffsetY.Should().Be(0);
        atBottomRight.ScrollOffsetX.Should().Be(150);
        atBottomRight.ScrollOffsetY.Should().Be(300);
    }

    [Fact]
    public void Create_returns_none_when_there_is_no_page_or_no_viewer()
    {
        //Arrange
        var pan = new PanPosition();

        //Act
        var noPage = PaneLayout.Create(0, 0, ViewportSide, ViewportSide, 1, pan);
        var noViewer = PaneLayout.Create(PageWidth, PageHeight, 0, 0, 1, pan);
        var noZoom = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 0, pan);
        var noPan = PaneLayout.Create(PageWidth, PageHeight, ViewportSide, ViewportSide, 1, null);

        //Assert
        noPage.Should().Be(PaneLayout.None);
        noViewer.Should().Be(PaneLayout.None);
        noZoom.Should().Be(PaneLayout.None);
        noPan.Should().Be(PaneLayout.None);
    }

    [Fact]
    public void None_has_no_image_to_size()
    {
        //Act
        var layout = PaneLayout.None;

        //Assert - NaN is what an image control wants when it is to size itself to its content
        layout.HasImage.Should().BeFalse();
        double.IsNaN(layout.ImageWidth).Should().BeTrue();
        double.IsNaN(layout.ImageHeight).Should().BeTrue();
    }
}
