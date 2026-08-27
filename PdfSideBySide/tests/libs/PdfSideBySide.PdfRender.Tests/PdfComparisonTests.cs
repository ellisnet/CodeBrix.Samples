using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Tests.Helpers;
using PdfSideBySide.PdfRender.Viewing;
using SilverAssertions;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfSideBySide.PdfRender.Tests;

public class PdfComparisonTests
{
    /// <summary>A comparison of a leftPages-page document with a rightPages-page one, both on page 1.</summary>
    private static async Task<PdfComparison> CreateComparison(int leftPages, int rightPages)
    {
        var folder = TestPdfs.CreateTempFolder();
        var comparison = new PdfComparison();
        await comparison.OpenAsync(DocumentSide.Left,
            TestPdfs.WriteSamplePdf(folder, "left.pdf", leftPages), TestContext.Current.CancellationToken);
        await comparison.OpenAsync(DocumentSide.Right,
            TestPdfs.WriteSamplePdf(folder, "right.pdf", rightPages), TestContext.Current.CancellationToken);
        return comparison;
    }

    private static (int Left, int Right) PagesOf(PdfComparison comparison) =>
        (comparison.Left.CurrentPage, comparison.Right.CurrentPage);

    #region | Opening and closing documents |

    [Fact]
    public void a_new_comparison_holds_no_documents()
    {
        //Act
        var comparison = new PdfComparison();

        //Assert
        comparison.Left.Should().BeNull();
        comparison.Right.Should().BeNull();
        comparison.IsReady.Should().BeFalse();
        comparison.CanMoveBothPrevious.Should().BeFalse();
        comparison.CanMoveBothNext.Should().BeFalse();
        comparison.CanAdjustRightPrevious.Should().BeFalse();
        comparison.CanAdjustRightNext.Should().BeFalse();
    }

    [Fact]
    public async Task opening_one_side_is_not_yet_ready_to_compare()
    {
        //Arrange
        var comparison = new PdfComparison();

        //Act
        var document = await comparison.OpenAsync(DocumentSide.Left, TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Assert
        comparison.Left.Should().BeSameAs(document);
        comparison.GetDocument(DocumentSide.Left).Should().BeSameAs(document);
        comparison.Right.Should().BeNull();
        comparison.IsReady.Should().BeFalse();
        comparison.CanMoveBothNext.Should().BeFalse();
        comparison.MoveBothNext().Should().BeFalse();
        comparison.AdjustRightNext().Should().BeFalse();
        document.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task opening_both_sides_is_ready_to_compare()
    {
        //Act
        var comparison = await CreateComparison(3, 3);

        //Assert
        comparison.IsReady.Should().BeTrue();
        comparison.CanMoveBothNext.Should().BeTrue();
        comparison.CanMoveBothPrevious.Should().BeFalse();
        comparison.CanAdjustRightNext.Should().BeTrue();
        comparison.CanAdjustRightPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task opening_the_other_sides_file_is_rejected_as_a_duplicate()
    {
        //Arrange
        var comparison = new PdfComparison();
        await comparison.OpenAsync(DocumentSide.Left, TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Act
        var exception = await Assert.ThrowsAsync<DuplicateDocumentException>(() =>
            comparison.OpenAsync(DocumentSide.Right, TestPdfs.InannaPath, TestContext.Current.CancellationToken));

        //Assert
        exception.FilePath.Should().Be(TestPdfs.InannaPath);
        exception.AlreadyOpenSide.Should().Be(DocumentSide.Left);
        exception.Message.Should().Contain("Inanna.pdf");
        exception.Message.Should().Contain("Document 1");
        comparison.Right.Should().BeNull();
        comparison.IsReady.Should().BeFalse();
    }

    [Fact]
    public async Task a_duplicate_is_detected_through_a_differently_spelled_path()
    {
        //Arrange
        var comparison = new PdfComparison();
        await comparison.OpenAsync(DocumentSide.Right, TestPdfs.InannaPath, TestContext.Current.CancellationToken);
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);
        var roundabout = Path.Combine(folder, "..", Path.GetFileName(folder), "Inanna.pdf");

        //Act
        var exception = await Assert.ThrowsAsync<DuplicateDocumentException>(() =>
            comparison.OpenAsync(DocumentSide.Left, roundabout, TestContext.Current.CancellationToken));

        //Assert
        exception.AlreadyOpenSide.Should().Be(DocumentSide.Right);
        exception.Message.Should().Contain("Document 2");
        comparison.Left.Should().BeNull();
    }

    [Fact]
    public async Task reopening_the_same_file_on_the_same_side_is_allowed_and_resets_its_page()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        comparison.MoveBothNext();
        comparison.MoveBothNext();
        var leftPath = comparison.Left.FilePath;

        //Act
        var reopened = await comparison.OpenAsync(DocumentSide.Left, leftPath, TestContext.Current.CancellationToken);

        //Assert
        reopened.CurrentPage.Should().Be(1);
        comparison.Left.Should().BeSameAs(reopened);
        comparison.Right.CurrentPage.Should().Be(3); //The other side is left alone
    }

    [Fact]
    public async Task replacing_a_document_leaves_the_other_side_where_it_was()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        comparison.MoveBothNext();
        var replacement = TestPdfs.WriteSamplePdf(TestPdfs.CreateTempFolder(), "replacement.pdf", 2);

        //Act
        await comparison.OpenAsync(DocumentSide.Right, replacement, TestContext.Current.CancellationToken);

        //Assert
        comparison.Right.PageCount.Should().Be(2);
        comparison.Right.CurrentPage.Should().Be(1);
        comparison.Left.CurrentPage.Should().Be(2);
    }

    [Fact]
    public async Task a_failed_open_leaves_the_side_unchanged()
    {
        //Arrange
        var comparison = await CreateComparison(3, 3);
        var previousRight = comparison.Right;
        var missing = Path.Combine(TestPdfs.CreateTempFolder(), "missing.pdf");

        //Act
        Func<Task> act = () => comparison.OpenAsync(DocumentSide.Right, missing, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
        comparison.Right.Should().BeSameAs(previousRight);
        comparison.IsReady.Should().BeTrue();
    }

    [Fact]
    public async Task close_empties_one_side()
    {
        //Arrange
        var comparison = await CreateComparison(3, 3);

        //Act
        comparison.Close(DocumentSide.Right);

        //Assert
        comparison.Right.Should().BeNull();
        comparison.Left.Should().NotBeNull();
        comparison.IsReady.Should().BeFalse();
        comparison.CanMoveBothNext.Should().BeFalse();
    }

    #endregion

    #region | Moving both documents together |

    [Fact]
    public async Task move_both_next_steps_both_documents_forward()
    {
        //Arrange
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext(); //Now on 2/2

        //Act
        var moved = comparison.MoveBothNext();

        //Assert
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((3, 3));
    }

    [Fact]
    public async Task move_both_previous_steps_both_documents_back()
    {
        //Arrange
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext();
        comparison.MoveBothNext(); //Now on 3/3

        //Act
        var moved = comparison.MoveBothPrevious();

        //Assert
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((2, 2));
    }

    [Fact]
    public async Task move_both_keeps_the_offset_set_by_an_adjustment()
    {
        //Arrange
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext();
        comparison.MoveBothNext();  //3/3
        comparison.AdjustRightNext(); //3/4

        //Act
        comparison.MoveBothNext();

        //Assert
        PagesOf(comparison).Should().Be((4, 5));
    }

    [Fact]
    public async Task move_both_previous_on_the_first_pages_stays_put()
    {
        //Arrange
        var comparison = await CreateComparison(3, 3);

        //Act
        var moved = comparison.MoveBothPrevious();

        //Assert
        moved.Should().BeFalse();
        PagesOf(comparison).Should().Be((1, 1));
        comparison.CanMoveBothPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task move_both_next_at_the_shorter_documents_end_still_advances_the_longer_one()
    {
        //Arrange
        var comparison = await CreateComparison(10, 9);
        for (var i = 0; i < 8; i++) { comparison.MoveBothNext(); } //9/9
        var canMoveBefore = comparison.CanMoveBothNext;

        //Act
        var moved = comparison.MoveBothNext();

        //Assert
        canMoveBefore.Should().BeTrue();
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((10, 9));
        comparison.CanMoveBothNext.Should().BeFalse();
        comparison.MoveBothNext().Should().BeFalse();
        PagesOf(comparison).Should().Be((10, 9));
    }

    [Fact]
    public async Task move_both_previous_with_an_offset_clamps_the_document_on_page_one()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        comparison.AdjustRightNext(); //1/2

        //Act
        var moved = comparison.MoveBothPrevious();

        //Assert
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((1, 1));
    }

    #endregion

    #region | Adjusting the right document alone |

    [Fact]
    public async Task adjust_right_next_moves_only_the_right_document()
    {
        //Arrange
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext();
        comparison.MoveBothNext(); //3/3

        //Act
        var moved = comparison.AdjustRightNext();

        //Assert
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((3, 4));
    }

    [Fact]
    public async Task adjust_right_previous_moves_only_the_right_document()
    {
        //Arrange
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext();
        comparison.MoveBothNext();
        comparison.AdjustRightNext();
        comparison.MoveBothNext(); //4/5

        //Act
        var moved = comparison.AdjustRightPrevious();

        //Assert
        moved.Should().BeTrue();
        PagesOf(comparison).Should().Be((4, 4));
    }

    [Fact]
    public async Task adjust_right_is_disabled_at_the_right_documents_ends()
    {
        //Arrange
        var comparison = await CreateComparison(5, 2);

        //Act
        var canPreviousAtStart = comparison.CanAdjustRightPrevious;
        var movedBackAtStart = comparison.AdjustRightPrevious();
        comparison.AdjustRightNext(); //1/2
        var canNextAtEnd = comparison.CanAdjustRightNext;
        var movedForwardAtEnd = comparison.AdjustRightNext();

        //Assert
        canPreviousAtStart.Should().BeFalse();
        movedBackAtStart.Should().BeFalse();
        canNextAtEnd.Should().BeFalse();
        movedForwardAtEnd.Should().BeFalse();
        PagesOf(comparison).Should().Be((1, 2));
        comparison.CanAdjustRightPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task the_full_walkthrough_from_the_specification()
    {
        //Arrange - comparing page 2 of both documents
        var comparison = await CreateComparison(10, 10);
        comparison.MoveBothNext();
        PagesOf(comparison).Should().Be((2, 2));

        //Act + Assert - main Down: 3/3
        comparison.MoveBothNext();
        PagesOf(comparison).Should().Be((3, 3));

        //adjustment Down: 3/4
        comparison.AdjustRightNext();
        PagesOf(comparison).Should().Be((3, 4));

        //main Down: 4/5
        comparison.MoveBothNext();
        PagesOf(comparison).Should().Be((4, 5));

        //adjustment Up: 4/4
        comparison.AdjustRightPrevious();
        PagesOf(comparison).Should().Be((4, 4));
    }

    #endregion

    #region | The view resets on every page change |

    private static void ZoomAndPan(PdfComparison comparison)
    {
        comparison.View.ZoomIn();
        comparison.View.ZoomIn();
        comparison.View.Pan(DocumentSide.Left, PanDirection.Up);
        comparison.View.Pan(DocumentSide.Right, PanDirection.Down);
    }

    private static void ShouldBeFitThePageCentred(ComparisonView view)
    {
        view.Zoom.Percent.Should().Be(100);
        view.LeftPan.Vertical.Should().Be(PanPosition.Centre);
        view.RightPan.Vertical.Should().Be(PanPosition.Centre);
    }

    [Fact]
    public async Task move_both_next_resets_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        ZoomAndPan(comparison);

        //Act
        comparison.MoveBothNext();

        //Assert
        ShouldBeFitThePageCentred(comparison.View);
    }

    [Fact]
    public async Task move_both_previous_resets_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        comparison.MoveBothNext();
        ZoomAndPan(comparison);

        //Act
        comparison.MoveBothPrevious();

        //Assert
        ShouldBeFitThePageCentred(comparison.View);
    }

    [Fact]
    public async Task adjust_right_resets_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        ZoomAndPan(comparison);

        //Act
        comparison.AdjustRightNext();

        //Assert
        ShouldBeFitThePageCentred(comparison.View);
    }

    [Fact]
    public async Task a_move_that_goes_nowhere_keeps_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        ZoomAndPan(comparison);

        //Act - already on the first pages
        var moved = comparison.MoveBothPrevious();
        var adjusted = comparison.AdjustRightPrevious();

        //Assert
        moved.Should().BeFalse();
        adjusted.Should().BeFalse();
        comparison.View.Zoom.Percent.Should().Be(150);
        comparison.View.LeftPan.Vertical.Should().BeLessThan(PanPosition.Centre);
    }

    [Fact]
    public async Task opening_a_document_resets_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        ZoomAndPan(comparison);
        var replacement = TestPdfs.WriteSamplePdf(TestPdfs.CreateTempFolder(), "replacement.pdf", 2);

        //Act
        await comparison.OpenAsync(DocumentSide.Right, replacement, TestContext.Current.CancellationToken);

        //Assert
        ShouldBeFitThePageCentred(comparison.View);
    }

    [Fact]
    public async Task closing_a_document_resets_the_view()
    {
        //Arrange
        var comparison = await CreateComparison(5, 5);
        ZoomAndPan(comparison);

        //Act
        comparison.Close(DocumentSide.Left);

        //Assert
        ShouldBeFitThePageCentred(comparison.View);
    }

    #endregion
}
