using PdfSideBySide.PdfRender.Viewing;
using SilverAssertions;
using Xunit;

namespace PdfSideBySide.PdfRender.Tests;

public class ComparisonViewTests
{
    [Fact]
    public void at_fit_the_page_nothing_pans()
    {
        //Arrange
        var view = new ComparisonView();

        //Act
        var moved = view.Pan(DocumentSide.Left, PanDirection.Down);

        //Assert
        moved.Should().BeFalse();
        view.PanStepFraction.Should().Be(0);
        view.CanPan(DocumentSide.Left, PanDirection.Down).Should().BeFalse();
        view.CanPan(DocumentSide.Right, PanDirection.Up).Should().BeFalse();
        view.LeftPan.Vertical.Should().Be(PanPosition.Centre);
    }

    [Fact]
    public void the_pan_step_is_a_quarter_viewport_of_the_scrollable_range()
    {
        //Arrange
        var view = new ComparisonView();
        while (view.Zoom.Percent < 200) { view.ZoomIn(); }

        //Act
        var step = view.PanStepFraction;

        //Assert - at 200% the page is two viewports wide, so one viewport is scrollable
        step.Should().BeApproximately(0.25, 1e-9);
    }

    [Fact]
    public void the_pan_step_shrinks_as_the_zoom_grows()
    {
        //Arrange
        var view = new ComparisonView();
        while (view.Zoom.Percent < 500) { view.ZoomIn(); }

        //Act
        var step = view.PanStepFraction;

        //Assert - four viewports scrollable, a quarter viewport is 1/16 of them
        step.Should().BeApproximately(0.0625, 1e-9);
    }

    [Fact]
    public void panning_one_side_leaves_the_other_alone()
    {
        //Arrange
        var view = new ComparisonView();
        while (view.Zoom.Percent < 200) { view.ZoomIn(); }

        //Act
        view.Pan(DocumentSide.Left, PanDirection.Up);
        view.Pan(DocumentSide.Left, PanDirection.Right);
        view.Pan(DocumentSide.Right, PanDirection.Down);
        view.Pan(DocumentSide.Right, PanDirection.Right);

        //Assert - top right on the left, bottom right on the right
        view.LeftPan.Vertical.Should().BeApproximately(0.25, 1e-9);
        view.LeftPan.Horizontal.Should().BeApproximately(0.75, 1e-9);
        view.RightPan.Vertical.Should().BeApproximately(0.75, 1e-9);
        view.RightPan.Horizontal.Should().BeApproximately(0.75, 1e-9);
    }

    [Fact]
    public void pan_of_returns_the_sides_position()
    {
        //Arrange
        var view = new ComparisonView();

        //Assert
        view.PanOf(DocumentSide.Left).Should().BeSameAs(view.LeftPan);
        view.PanOf(DocumentSide.Right).Should().BeSameAs(view.RightPan);
    }

    [Fact]
    public void zooming_in_keeps_each_panes_position()
    {
        //Arrange
        var view = new ComparisonView();
        view.ZoomIn();
        view.Pan(DocumentSide.Left, PanDirection.Up);
        var leftBefore = view.LeftPan.Vertical;

        //Act
        view.ZoomIn();

        //Assert
        view.LeftPan.Vertical.Should().Be(leftBefore);
        view.RightPan.Vertical.Should().Be(PanPosition.Centre);
    }

    [Fact]
    public void zooming_out_keeps_positions_until_fit_the_page_which_recentres()
    {
        //Arrange
        var view = new ComparisonView();
        view.ZoomIn();
        view.ZoomIn();
        view.Pan(DocumentSide.Right, PanDirection.Left);
        var rightBefore = view.RightPan.Horizontal;

        //Act
        view.ZoomOut();
        var stillPanned = view.RightPan.Horizontal;
        view.ZoomOut();

        //Assert
        stillPanned.Should().Be(rightBefore);
        view.Zoom.IsZoomedIn.Should().BeFalse();
        view.RightPan.Horizontal.Should().Be(PanPosition.Centre);
    }

    [Fact]
    public void reset_goes_back_to_fit_the_page_centred()
    {
        //Arrange
        var view = new ComparisonView();
        view.ZoomIn();
        view.ZoomIn();
        view.Pan(DocumentSide.Left, PanDirection.Down);
        view.Pan(DocumentSide.Right, PanDirection.Left);

        //Act
        var changed = view.Reset();
        var changedAgain = view.Reset();

        //Assert
        changed.Should().BeTrue();
        changedAgain.Should().BeFalse();
        view.Zoom.Percent.Should().Be(100);
        view.LeftPan.Vertical.Should().Be(PanPosition.Centre);
        view.RightPan.Horizontal.Should().Be(PanPosition.Centre);
    }
}
