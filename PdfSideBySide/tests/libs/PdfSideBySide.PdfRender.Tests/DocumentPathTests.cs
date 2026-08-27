using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Tests.Helpers;
using SilverAssertions;
using Xunit;
using System.IO;

namespace PdfSideBySide.PdfRender.Tests;

public class DocumentPathTests
{
    [Fact]
    public void normalize_makes_the_path_absolute_and_drops_dot_dot_segments()
    {
        //Arrange
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);
        var roundabout = Path.Combine(folder, "..", Path.GetFileName(folder), "Inanna.pdf");

        //Act
        var normalized = DocumentPath.Normalize(roundabout);

        //Assert
        normalized.Should().Be(TestPdfs.InannaPath);
        Path.IsPathRooted(normalized).Should().BeTrue();
    }

    [Fact]
    public void normalize_drops_a_trailing_directory_separator()
    {
        //Arrange
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);

        //Act
        var normalized = DocumentPath.Normalize(folder + Path.DirectorySeparatorChar);

        //Assert
        normalized.Should().Be(folder);
    }

    [Fact]
    public void are_same_is_true_for_two_spellings_of_one_file()
    {
        //Arrange
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);
        var roundabout = Path.Combine(folder, "..", Path.GetFileName(folder), "Inanna.pdf");

        //Act
        var same = DocumentPath.AreSame(TestPdfs.InannaPath, roundabout);

        //Assert
        same.Should().BeTrue();
    }

    [Fact]
    public void are_same_is_false_for_different_files_in_one_folder()
    {
        //Arrange
        var folder = Path.GetDirectoryName(TestPdfs.InannaPath);

        //Act
        var same = DocumentPath.AreSame(TestPdfs.InannaPath, Path.Combine(folder, "Other.pdf"));

        //Assert
        same.Should().BeFalse();
    }
}
