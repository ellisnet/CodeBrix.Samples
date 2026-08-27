using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Tests.Helpers;
using SilverAssertions;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfSideBySide.PdfRender.Tests;

public class PdfPageDocumentTests
{
    [Fact]
    public async Task open_reads_the_page_count_and_starts_on_page_one()
    {
        //Act
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Assert
        document.PageCount.Should().Be(TestPdfs.InannaPageCount);
        document.CurrentPage.Should().Be(1);
        document.FileName.Should().Be("Inanna.pdf");
        document.FilePath.Should().Be(TestPdfs.InannaPath);
        document.CanMovePrevious.Should().BeFalse();
        document.CanMoveNext.Should().BeTrue();
    }

    [Fact]
    public async Task open_normalizes_a_roundabout_path()
    {
        //Arrange
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);
        var roundabout = Path.Combine(folder, "..", Path.GetFileName(folder), "Inanna.pdf");

        //Act
        var document = await PdfPageDocument.OpenAsync(roundabout, TestContext.Current.CancellationToken);

        //Assert
        document.FilePath.Should().Be(TestPdfs.InannaPath);
    }

    [Fact]
    public async Task open_counts_the_pages_of_a_synthetic_document()
    {
        //Arrange
        var folder = TestPdfs.CreateTempFolder();
        var path = TestPdfs.WriteSamplePdf(folder, "seven.pdf", 7);

        //Act
        var document = await PdfPageDocument.OpenAsync(path, TestContext.Current.CancellationToken);

        //Assert
        document.PageCount.Should().Be(7);
    }

    [Fact]
    public async Task open_throws_file_not_found_for_a_missing_file()
    {
        //Arrange
        var missing = Path.Combine(TestPdfs.CreateTempFolder(), "missing.pdf");

        //Act
        Func<Task> act = () => PdfPageDocument.OpenAsync(missing, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task open_throws_invalid_data_for_a_file_that_is_not_a_pdf()
    {
        //Arrange
        var notPdf = Path.Combine(TestPdfs.CreateTempFolder(), "notes.pdf");
        await File.WriteAllTextAsync(notPdf, "This is not a PDF document.", TestContext.Current.CancellationToken);

        //Act
        Func<Task> act = () => PdfPageDocument.OpenAsync(notPdf, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task open_rejects_a_blank_path(string path)
    {
        //Act
        Func<Task> act = () => PdfPageDocument.OpenAsync(path, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task move_next_and_previous_step_the_cursor_one_page()
    {
        //Arrange
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Act
        var movedNext = document.MoveNext();
        var pageAfterNext = document.CurrentPage;
        var movedPrevious = document.MovePrevious();

        //Assert
        movedNext.Should().BeTrue();
        pageAfterNext.Should().Be(2);
        movedPrevious.Should().BeTrue();
        document.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task move_previous_on_the_first_page_stays_put()
    {
        //Arrange
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Act
        var moved = document.MovePrevious();

        //Assert
        moved.Should().BeFalse();
        document.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task move_next_on_the_last_page_stays_put()
    {
        //Arrange
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);
        document.GoToPage(document.PageCount);

        //Act
        var moved = document.MoveNext();

        //Assert
        moved.Should().BeFalse();
        document.CurrentPage.Should().Be(TestPdfs.InannaPageCount);
        document.CanMoveNext.Should().BeFalse();
        document.CanMovePrevious.Should().BeTrue();
    }

    [Fact]
    public async Task go_to_page_accepts_the_whole_page_range()
    {
        //Arrange
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Act
        document.GoToPage(20);

        //Assert
        document.CurrentPage.Should().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(TestPdfs.InannaPageCount + 1)]
    public async Task go_to_page_rejects_pages_outside_the_document(int pageNumber)
    {
        //Arrange
        var document = await PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

        //Act
        Action act = () => document.GoToPage(pageNumber);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
        document.CurrentPage.Should().Be(1);
    }
}
