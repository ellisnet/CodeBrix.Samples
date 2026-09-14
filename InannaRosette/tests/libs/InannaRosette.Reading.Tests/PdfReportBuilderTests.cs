// PdfReportBuilderTests.cs - the printable report: that the bytes really are a PDF, that the
// embedded Merriweather subsets travel inside them, and that the suggested file name is one a
// file system will accept.

using System;
using System.Linq;
using System.Text;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class PdfReportBuilderTests
{
    /// <summary>A report of this size could not be an empty or truncated document.</summary>
    private const int PlausibleMinimumBytes = 20_000;

    private static void ShouldBeAPdf(byte[] bytes)
    {
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(PlausibleMinimumBytes);

        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");

        var tail = Encoding.ASCII.GetString(bytes, Math.Max(0, bytes.Length - 32), Math.Min(32, bytes.Length));
        tail.TrimEnd('\r', '\n', ' ').Should().EndWith("%%EOF");
    }

    [Fact]
    public void a_full_reading_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_partial_reading_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.PartialReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void an_empty_reading_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.EmptyReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_reading_with_no_heart_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.HeartlessReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_single_card_reading_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.SingleCardReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_reading_with_no_querent_and_no_question_builds_a_pdf()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading(querent: "", question: ""));

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void the_longest_querent_and_question_still_build_a_pdf()
    {
        //Arrange - as much text as the UI could ever hand the builder
        var interpretation = TestData.Interpret(TestData.ExtremeReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_fully_reversed_rosette_builds_a_pdf()
    {
        //Arrange
        var reading = TestData.EmptyReading();
        for (var i = 0; i < 9; i++) reading.Placements.Add(TestData.Place(i, TestData.FullRosetteCardIds[i], true));

        //Act
        var bytes = TestData.Builder().Build(TestData.Interpret(reading));

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_letter_sized_report_builds_a_pdf()
    {
        //Arrange
        var builder = new PdfReportBuilder(new PdfReportOptions { UseLetter = true });
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Act
        var bytes = builder.Build(interpretation);

        //Assert
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void a_full_report_is_larger_than_a_one_card_report()
    {
        //Arrange
        var builder = TestData.Builder();

        //Act
        var full = builder.Build(TestData.Interpret(TestData.FullReading()));
        var single = builder.Build(TestData.Interpret(TestData.SingleCardReading()));

        //Assert
        full.Length.Should().BeGreaterThan(single.Length);
    }

    [Fact]
    public void the_bytes_carry_the_embedded_merriweather_subsets()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert - the subset font names in the PDF name the face they were cut from
        TestData.ContainsAscii(bytes, "Merriweather").Should().BeTrue();
    }

    [Fact]
    public void the_bytes_declare_a_font_and_a_page()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert
        TestData.ContainsAscii(bytes, "/Font").Should().BeTrue();
        TestData.ContainsAscii(bytes, "/Page").Should().BeTrue();
    }

    [Fact]
    public void building_the_same_reading_twice_produces_two_usable_reports()
    {
        //Arrange
        var builder = TestData.Builder();
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Act
        var first = builder.Build(interpretation);
        var second = builder.Build(interpretation);

        //Assert - the document carries a generated id, so the bytes need not be identical
        ShouldBeAPdf(first);
        ShouldBeAPdf(second);
        second.Length.Should().BeInRange((int)(first.Length * 0.9), (int)(first.Length * 1.1));
    }

    // ---------------------------------------------------------------------------------------
    // The closing blessing
    // ---------------------------------------------------------------------------------------

    /// <remarks>
    /// The blessing cannot be searched for in the bytes: the report embeds subset Merriweather
    /// faces, so its text is written as glyph indexes rather than as characters, and the content
    /// streams are compressed on top of that. What can be proved from outside the builder is that
    /// the line is drawn at all — the same reading, built twice, differs only by the blessing and
    /// the rule above it, and the document that draws them is the longer of the two. The visual
    /// check that it sits after the counsel and before the appendix is made by rendering the page.
    /// </remarks>
    [Fact]
    public void the_report_draws_the_closing_blessing()
    {
        //Arrange
        var builder = TestData.Builder();
        var withBlessing = TestData.Interpret(TestData.FullReading());
        var withoutBlessing = withBlessing with { Closing = string.Empty };

        //Act
        var drawn = builder.Build(withBlessing);
        var omitted = builder.Build(withoutBlessing);

        //Assert
        ShouldBeAPdf(drawn);
        ShouldBeAPdf(omitted);
        drawn.Length.Should().BeGreaterThan(omitted.Length);
    }

    [Fact]
    public void the_closing_blessing_is_drawn_for_an_empty_reading_as_well()
    {
        //Arrange - nothing laid, so there is no appendix and no station section either
        var builder = TestData.Builder();
        var withBlessing = TestData.Interpret(TestData.EmptyReading());
        var withoutBlessing = withBlessing with { Closing = string.Empty };

        //Act
        var drawn = builder.Build(withBlessing);
        var omitted = builder.Build(withoutBlessing);

        //Assert
        ShouldBeAPdf(drawn);
        drawn.Length.Should().BeGreaterThan(omitted.Length);
    }

    [Fact]
    public void a_report_whose_closing_blessing_was_emptied_still_builds()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading()) with { Closing = "   " };

        //Act
        var bytes = TestData.Builder().Build(interpretation);

        //Assert - a blank closing is simply not drawn; it is never a failure
        ShouldBeAPdf(bytes);
    }

    [Fact]
    public void the_closing_blessing_does_not_cost_the_report_a_page()
    {
        //Arrange - one line and a rule must not push the appendix onto a page of its own
        var builder = TestData.Builder();
        var withBlessing = TestData.Interpret(TestData.FullReading());
        var withoutBlessing = withBlessing with { Closing = string.Empty };

        //Act
        var drawn = builder.Build(withBlessing);
        var omitted = builder.Build(withoutBlessing);

        //Assert - /Type /Page appears once per page, so the two documents hold the same count
        TestData.CountAscii(drawn, "/Page\n").Should().Be(TestData.CountAscii(omitted, "/Page\n"));
    }

    [Fact]
    public void Build_rejects_a_missing_interpretation()
    {
        //Arrange
        var builder = TestData.Builder();

        //Act
        Action act = () => builder.Build(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ---------------------------------------------------------------------------------------
    // The suggested file name
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void SuggestedFileName_rejects_a_missing_interpretation()
    {
        //Arrange
        var builder = TestData.Builder();

        //Act
        Action act = () => builder.SuggestedFileName(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestedFileName_names_the_querent_and_the_day()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading("Jeremy", "What now?"));

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert
        name.Should().Be("Rosette-Reading-Jeremy-2026-09-13.pdf");
    }

    [Fact]
    public void SuggestedFileName_leaves_the_querent_out_when_nobody_is_named()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading(querent: "  ", question: ""));

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert
        name.Should().Be("Rosette-Reading-2026-09-13.pdf");
    }

    [Theory]
    [InlineData("Jeremy")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Enheduanna / High Priestess")]
    [InlineData("C:\\Windows\\System32")]
    [InlineData("a<b>c:d\"e|f?g*h")]
    [InlineData("Ereškigal")]
    [InlineData("Ninḫursaĝ")]
    [InlineData(".....")]
    [InlineData("Inanna\tof\nUruk")]
    [InlineData("名前")]
    public void SuggestedFileName_produces_a_name_a_file_system_will_accept(string querent)
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading(querent, "Anything at all?"));

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert
        name.Should().EndWith(".pdf");
        name.Should().StartWith("Rosette-Reading-");
        foreach (var bad in TestData.InvalidFileNameCharacters) name.Should().NotContain(bad.ToString());
        name.Should().NotContain(" ");
    }

    [Fact]
    public void SuggestedFileName_strips_the_accents_off_a_name()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading("Ereškigal", ""));

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert - the caron comes away and the plain letter is kept
        name.Should().Be("Rosette-Reading-Ereskigal-2026-09-13.pdf");
    }

    [Fact]
    public void SuggestedFileName_stamps_the_day_the_reading_was_laid()
    {
        //Arrange
        var reading = new RosetteReading { Created = new DateTime(2019, 3, 7, 22, 0, 0), Querent = "Ur" };
        reading.Placements.Add(TestData.Place(0, 1));

        //Act
        var name = TestData.Builder().SuggestedFileName(TestData.Interpret(reading));

        //Assert
        name.Should().Be("Rosette-Reading-Ur-2019-03-07.pdf");
    }

    [Fact]
    public void SuggestedFileName_never_runs_two_separators_together()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.FullReading("Jeremy   R.   Ellis", ""));

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert
        name.Should().NotContain("--");
    }

    [Fact]
    public void SuggestedFileName_of_the_longest_querent_is_still_one_file_name()
    {
        //Arrange
        var interpretation = TestData.Interpret(TestData.ExtremeReading());

        //Act
        var name = TestData.Builder().SuggestedFileName(interpretation);

        //Assert
        name.Should().EndWith(".pdf");
        foreach (var bad in TestData.InvalidFileNameCharacters) name.Should().NotContain(bad.ToString());
    }

    [Fact]
    public void the_report_options_default_to_A4_and_the_deck_author()
    {
        //Act
        var options = new PdfReportOptions();

        //Assert
        options.UseLetter.Should().BeFalse();
        options.Author.Should().Be("Rosette of Inanna");
    }

    [Fact]
    public void a_builder_made_without_options_still_builds()
    {
        //Arrange
        var builder = new PdfReportBuilder(null);

        //Act
        var bytes = builder.Build(TestData.Interpret(TestData.FullReading()));

        //Assert
        ShouldBeAPdf(bytes);
    }
}
