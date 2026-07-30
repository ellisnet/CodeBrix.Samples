using CodeBrix.NotionApi;
using CodeBrix.PdfDocCreate.DocumentObjectModel;
using NotionDocumentCreator.CreateDocument.Internal;
using NotionDocumentCreator.CreateDocument.Models;
using SilverAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NText = CodeBrix.NotionApi.Text;

namespace NotionDocumentCreator.CreateDocument.Tests;

public class RichTextWriterTests
{
    private static (Paragraph Paragraph, RichTextWriter Writer, List<string> Warnings) CreateWriter()
    {
        var theme = BookTheme.For(PageSizeOption.EightByTen);
        var document = new Document();
        BookStyles.Define(document, theme);
        var paragraph = document.AddSection().AddParagraph();
        var warnings = new List<string>();
        return (paragraph, new RichTextWriter(theme, warnings), warnings);
    }

    private static RichTextText Run(string text, bool bold = false, bool italic = false,
        bool strike = false, bool underline = false, bool code = false, string link = null) =>
        new()
        {
            PlainText = text,
            Annotations = new Annotations
            {
                IsBold = bold,
                IsItalic = italic,
                IsStrikeThrough = strike,
                IsUnderline = underline,
                IsCode = code
            },
            Text = new NText { Content = text, Link = link is null ? null : new Link { Url = link } }
        };

    [Fact]
    public void bold_annotation_maps_to_a_bold_run()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("strong", bold: true)]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Bold.Should().Be(true);
    }

    [Fact]
    public void italic_annotation_maps_to_an_italic_run()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("leaning", italic: true)]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Italic.Should().Be(true);
    }

    [Fact]
    public void strikethrough_annotation_maps_to_a_struck_run()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("gone", strike: true)]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Strikethrough.Should().Be(Strikethrough.Single);
    }

    [Fact]
    public void underline_annotation_maps_to_an_underlined_run()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("underneath", underline: true)]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Underline.Should().Be(Underline.Single);
    }

    [Fact]
    public void code_annotation_maps_to_the_monospace_face()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("var x", code: true)]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Name.Should().Be(BookFonts.MonoFamily);
    }

    [Fact]
    public void link_maps_to_a_hyperlink_with_accent_color()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("the site", link: "https://example.com")]);

        //Assert
        TestDom.Hyperlinks(paragraph).Should().HaveCount(1);
        TestDom.FormattedRuns(paragraph).Single().Font.Color.Should().Be(BookTheme.Accent);
    }

    [Fact]
    public void null_annotations_are_tolerated()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();
        var bare = new RichTextText { PlainText = "plain", Text = new NText { Content = "plain" } };
        bare.Annotations = null;

        //Act
        writer.Append(paragraph, [bare]);

        //Assert
        TestDom.TextOf(paragraph).Should().Be("plain");
    }

    [Fact]
    public void inline_equation_renders_italic_source_with_a_warning()
    {
        //Arrange
        var (paragraph, writer, warnings) = CreateWriter();
        var equation = new RichTextEquation
        {
            PlainText = "x^2",
            Equation = new Equation { Expression = "x^2" }
        };

        //Act
        writer.Append(paragraph, [equation]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Italic.Should().Be(true);
        warnings.Should().HaveCount(1);
    }

    [Fact]
    public void emoji_routes_to_the_emoji_face()
    {
        //Arrange - U+2600 sun: a BMP emoji the monochrome Noto Emoji build covers
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("☀")]);

        //Assert
        TestDom.FormattedRuns(paragraph).Single().Font.Name.Should().Be(BookFonts.EmojiFamily);
    }

    [Fact]
    public void astral_plane_emoji_are_dropped_not_printed_as_tofu()
    {
        //Arrange - U+1F389 party popper: the font has it, but the PDF text engine
        //  cannot address astral-plane glyphs, so printing it would produce tofu
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("🎉")]);

        //Assert
        TestDom.TextOf(paragraph).Should().Be("");
        writer.DroppedCharacterCount.Should().Be(1);
    }

    [Fact]
    public void unrenderable_characters_are_dropped_and_counted()
    {
        //Arrange - cuneiform is outside every embedded face
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("𒀭")]);

        //Assert
        TestDom.TextOf(paragraph).Should().Be("");
        writer.DroppedCharacterCount.Should().Be(1);
    }

    [Fact]
    public void mixed_text_and_emoji_split_into_separate_runs()
    {
        //Arrange
        var (paragraph, writer, _) = CreateWriter();

        //Act
        writer.Append(paragraph, [Run("Holy ☀ rosette")]);

        //Assert
        var runs = TestDom.FormattedRuns(paragraph);
        runs.Should().HaveCount(3);
        runs[1].Font.Name.Should().Be(BookFonts.EmojiFamily);
    }
}
