using CodeBrix.PdfDocCreate.DocumentObjectModel;
using NotionDocumentCreator.CreateDocument.Internal;
using NotionDocumentCreator.CreateDocument.Models;
using SilverAssertions;
using System.Linq;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// The §6.1 sectioning rules: the cover is unnumbered "page 0", printed numbering
/// starts at 1 on the second section, and later sections never restart it.
/// </summary>
public class PageNumberingTests
{
    private static ChapterContent Chapter(string id, string title) =>
        new() { PageId = id, Title = title, AncestorTitles = [title], Blocks = [] };

    private static Document ComposeThreeChapterBook()
    {
        var context = new RenderContext { Theme = BookTheme.For(PageSizeOption.EightByTen) };
        var composer = new BookComposer(
            [Chapter("p0", "The Cover Page"), Chapter("p1", "Chapter One"), Chapter("p2", "Chapter Two")],
            context);
        return composer.Compose();
    }

    [Fact]
    public void one_section_is_created_per_selected_page()
    {
        //Act
        var document = ComposeThreeChapterBook();

        //Assert
        document.Sections.Cast<Section>().Should().HaveCount(3);
    }

    [Fact]
    public void the_cover_section_has_no_folio()
    {
        //Act
        var document = ComposeThreeChapterBook();

        //Assert - no footer content was added to the cover
        var cover = document.Sections.Cast<Section>().First();
        cover.Footers.Primary.Elements.Cast<object>().Should().HaveCount(0);
    }

    [Fact]
    public void printed_numbering_starts_at_1_on_the_second_section()
    {
        //Act
        var document = ComposeThreeChapterBook();

        //Assert
        var second = document.Sections.Cast<Section>().Skip(1).First();
        second.PageSetup.StartingNumber.Should().Be(1);
    }

    [Fact]
    public void later_sections_do_not_restart_the_numbering()
    {
        //Act
        var document = ComposeThreeChapterBook();

        //Assert - the third section keeps the default (continue numbering), which
        //  must differ from the explicit restart the second section carries
        var sections = document.Sections.Cast<Section>().ToList();
        var defaultStartingNumber = new Section().PageSetup.StartingNumber;
        sections[2].PageSetup.StartingNumber.Should().Be(defaultStartingNumber);
        (sections[2].PageSetup.StartingNumber != 1).Should().Be(true);
    }

    [Fact]
    public void content_chapters_carry_running_heads_and_folios()
    {
        //Act
        var document = ComposeThreeChapterBook();

        //Assert
        var chapterSection = document.Sections.Cast<Section>().Skip(1).First();
        chapterSection.Headers.Primary.Elements.Cast<object>().Should().HaveCount(1);
        chapterSection.Footers.Primary.Elements.Cast<object>().Should().HaveCount(1);
        chapterSection.Footers.FirstPage.Elements.Cast<object>().Should().HaveCount(1);
    }
}
