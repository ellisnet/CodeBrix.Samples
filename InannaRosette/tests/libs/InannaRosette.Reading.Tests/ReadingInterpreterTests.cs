// ReadingInterpreterTests.cs - the prose the library writes: that it is deterministic, that it
// covers every station laid, that orientation changes what is said, and that an empty or partial
// rosette is handled the way the interpreter documents.

using System;
using System.Linq;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class ReadingInterpreterTests
{
    [Fact]
    public void Interpret_rejects_a_missing_reading()
    {
        //Arrange
        var interpreter = TestData.Interpreter();

        //Act
        Action act = () => interpreter.Interpret(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void the_same_rosette_always_produces_the_same_text()
    {
        //Arrange
        var first = TestData.FullReading();
        var second = TestData.FullReading();

        //Act
        var a = TestData.Interpret(first);
        var b = TestData.Interpret(second);

        //Assert
        TestData.AllText(b).Should().Be(TestData.AllText(a));
    }

    [Fact]
    public void interpreting_the_same_reading_twice_gives_identical_text()
    {
        //Arrange
        var reading = TestData.FullReading();
        var interpreter = TestData.Interpreter();

        //Act
        var a = interpreter.Interpret(reading);
        var b = interpreter.Interpret(reading);

        //Assert
        TestData.AllText(b).Should().Be(TestData.AllText(a));
    }

    [Fact]
    public void two_different_rosettes_produce_different_text()
    {
        //Act
        var a = TestData.Interpret(TestData.FullReading());
        var b = TestData.Interpret(TestData.OtherFullReading());

        //Assert
        TestData.AllText(b).Should().NotBe(TestData.AllText(a));
    }

    [Fact]
    public void turning_one_card_over_changes_the_reading()
    {
        //Arrange
        var upright = TestData.FullReading();
        var turned = TestData.FullReading();
        var index = turned.Placements.FindIndex(p => p.Position.Index == 1);
        turned.Placements[index] = turned.Placements[index] with { IsReversed = true };

        //Act
        var a = TestData.Interpret(upright);
        var b = TestData.Interpret(turned);

        //Assert
        TestData.AllText(b).Should().NotBe(TestData.AllText(a));
    }

    [Fact]
    public void every_placed_station_appears_in_the_output()
    {
        //Arrange
        var reading = TestData.FullReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Should().HaveCount(9);
        foreach (var placed in reading.Placements)
        {
            var written = interpretation.Positions.Single(p => p.Placed.Position.Index == placed.Position.Index);
            written.Heading.Should().Contain(placed.Position.Title);
            written.Heading.Should().Contain(placed.Card.Name);
            written.Heading.Should().Contain(placed.OrientationLabel);
        }
    }

    [Fact]
    public void every_placed_station_of_a_partial_rosette_appears_in_the_output()
    {
        //Arrange
        var reading = TestData.PartialReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Select(p => p.Placed.Position.Index).Should().Equal([0, 3, 7]);
    }

    [Fact]
    public void the_stations_come_out_in_spread_order()
    {
        //Arrange - laid out of order, on purpose
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(6, 12));
        reading.Placements.Add(TestData.Place(0, 1));
        reading.Placements.Add(TestData.Place(3, 30));

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Select(p => p.Placed.Position.Index).Should().Equal([0, 3, 6]);
    }

    [Fact]
    public void a_reversed_card_reads_differently_from_the_same_card_upright()
    {
        //Arrange - same card, same station, opposite orientation
        var upright = TestData.SingleCardReading(cardId: 7, reversed: false);
        var reversed = TestData.SingleCardReading(cardId: 7, reversed: true);

        //Act
        var a = TestData.Interpret(upright).Positions[0];
        var b = TestData.Interpret(reversed).Positions[0];

        //Assert
        b.Paragraphs[0].Should().NotBe(a.Paragraphs[0]);
        b.Heading.Should().NotBe(a.Heading);
    }

    [Fact]
    public void a_reversed_card_carries_its_reversed_keywords()
    {
        //Arrange
        var card = DeckData.ById(7);
        var reading = TestData.SingleCardReading(cardId: 7, reversed: true);

        //Act
        var written = TestData.Interpret(reading).Positions[0];

        //Assert
        written.Keywords.Should().Equal(card.ReversedKeywords.ToList());
    }

    [Fact]
    public void an_upright_card_carries_its_upright_keywords()
    {
        //Arrange
        var card = DeckData.ById(7);
        var reading = TestData.SingleCardReading(cardId: 7);

        //Act
        var written = TestData.Interpret(reading).Positions[0];

        //Assert
        written.Keywords.Should().Equal(card.UprightKeywords.ToList());
    }

    [Fact]
    public void a_station_carries_the_meaning_of_the_face_that_is_showing()
    {
        //Arrange
        var card = DeckData.ById(12);

        //Act
        var upright = TestData.Interpret(TestData.SingleCardReading(12)).Positions[0];
        var reversed = TestData.Interpret(TestData.SingleCardReading(12, reversed: true)).Positions[0];

        //Assert
        upright.Paragraphs[0].Should().Contain(card.UprightMeaning);
        reversed.Paragraphs[0].Should().Contain(card.ReversedMeaning);
    }

    [Fact]
    public void every_station_closes_with_the_invocation_of_its_card()
    {
        //Arrange
        var reading = TestData.FullReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        foreach (var written in interpretation.Positions)
        {
            written.Paragraphs[^1].Should().Be(written.Placed.Card.Invocation);
        }
    }

    [Fact]
    public void a_petal_whose_opposite_is_laid_gets_an_axis_paragraph()
    {
        //Arrange - the Descent and the Gift face each other
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(4, 25));
        reading.Placements.Add(TestData.Place(8, 40));

        //Act
        var interpretation = TestData.Interpret(reading);
        var descent = interpretation.Positions.Single(p => p.Placed.Position.Index == 4);

        //Assert - tie, axis, invocation
        descent.Paragraphs.Should().HaveCount(3);
        descent.Paragraphs[1].Should().Contain("The Gift");
    }

    [Fact]
    public void a_petal_whose_opposite_is_empty_gets_no_axis_paragraph()
    {
        //Arrange
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(4, 25));

        //Act
        var written = TestData.Interpret(reading).Positions[0];

        //Assert - tie and invocation only
        written.Paragraphs.Should().HaveCount(2);
    }

    [Fact]
    public void the_title_names_the_querent_and_the_question()
    {
        //Arrange
        var reading = TestData.FullReading("Enheduanna", "Should I finish the hymn?");

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Title.Should().Contain("Enheduanna");
        interpretation.Title.Should().Contain("Should I finish the hymn");
    }

    [Fact]
    public void the_title_falls_back_when_no_one_is_named()
    {
        //Arrange
        var reading = TestData.FullReading(querent: "", question: "");

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Title.Should().Be("The Rosette Reading");
    }

    [Fact]
    public void the_title_still_carries_the_question_when_no_one_is_named()
    {
        //Arrange
        var reading = TestData.FullReading(querent: "", question: "What is rising?");

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Title.Should().StartWith("The Rosette Reading");
        interpretation.Title.Should().Contain("What is rising");
    }

    [Fact]
    public void the_opening_names_the_querent_and_repeats_the_question()
    {
        //Arrange
        var reading = TestData.FullReading("Ninshubur", "Who will come for me?");

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Opening.Should().Contain("Ninshubur");
        interpretation.Opening.Should().Contain("Who will come for me?");
    }

    [Fact]
    public void the_opening_names_the_card_at_the_heart()
    {
        //Arrange
        var reading = TestData.FullReading();
        var heart = reading.At(0);

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Opening.Should().Contain(heart.Card.Name);
    }

    [Fact]
    public void a_single_card_reading_speaks_in_the_singular()
    {
        //Arrange
        var reading = TestData.SingleCardReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Should().HaveCount(1);
        interpretation.Opening.Should().Contain("one card");
    }

    [Fact]
    public void a_reading_with_no_heart_is_still_interpreted()
    {
        //Arrange
        var reading = TestData.HeartlessReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Should().HaveCount(2);
        interpretation.Opening.Should().NotBeNullOrWhiteSpace();
        interpretation.Counsel.Should().NotBeNullOrWhiteSpace();
        interpretation.ClosingInvocation.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void an_empty_reading_is_handled_as_the_interpreter_documents()
    {
        //Arrange
        var reading = TestData.EmptyReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Should().BeEmpty();
        interpretation.Opening.Should().Contain("No cards have been laid");
        interpretation.Counsel.Should().Contain("Begin with the Heart");
        interpretation.ClosingInvocation.Should().NotBeNullOrWhiteSpace();
        interpretation.Insights.Should().HaveCount(3);
    }

    [Fact]
    public void an_empty_reading_still_gets_a_title()
    {
        //Arrange
        var reading = TestData.EmptyReading("Jeremy", "What now?");

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Title.Should().Contain("Jeremy");
        interpretation.Title.Should().Contain("What now");
    }

    [Fact]
    public void a_partial_rosette_is_noted_as_unfinished()
    {
        //Arrange
        var reading = TestData.PartialReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Insights.Select(i => i.Title).Should().Contain("The Rosette Unfinished");
    }

    [Fact]
    public void a_full_rosette_is_not_noted_as_unfinished()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        interpretation.Insights.Select(i => i.Title).Should().NotContain("The Rosette Unfinished");
    }

    [Fact]
    public void the_insights_never_run_past_six()
    {
        //Act
        var full = TestData.Interpret(TestData.FullReading());
        var other = TestData.Interpret(TestData.OtherFullReading());

        //Assert
        full.Insights.Count.Should().BeLessThanOrEqualTo(6);
        other.Insights.Count.Should().BeLessThanOrEqualTo(6);
        full.Insights.Should().NotBeEmpty();
    }

    [Fact]
    public void every_insight_carries_a_title_and_a_body()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        interpretation.Insights.Should().AllSatisfy(i =>
        {
            i.Title.Should().NotBeNullOrWhiteSpace();
            i.Text.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void a_full_rosette_weighs_the_suits_and_the_turned_cards()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());
        var titles = interpretation.Insights.Select(i => i.Title).ToList();

        //Assert
        titles.Should().Contain("The Weight of the Suits");
        titles.Should().Contain("The Turned Cards");
    }

    [Fact]
    public void a_reading_with_a_heart_gets_the_still_centre_insight()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        interpretation.Insights.Select(i => i.Title).Should().Contain("The Still Centre");
    }

    [Fact]
    public void two_axes_of_the_same_kind_are_never_described_in_the_same_words()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());
        var axes = interpretation.Insights.Where(i => i.Title.StartsWith("Axis")).ToList();

        //Assert
        axes.Select(a => a.Text).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void the_counsel_is_taken_from_the_gift_when_the_gift_has_been_laid()
    {
        //Arrange
        var reading = TestData.FullReading();
        var gift = reading.At(8);

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Counsel.Should().Contain(gift.Card.Name);
    }

    [Fact]
    public void the_counsel_falls_back_to_the_heart_when_the_gift_is_empty()
    {
        //Arrange
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(0, 15));

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Counsel.Should().Contain(DeckData.ById(15).Name);
    }

    [Fact]
    public void the_closing_invocation_is_written_as_several_lines()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        interpretation.ClosingInvocation.Split('\n').Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public void a_full_reading_ends_with_the_closing_blessing()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert - the exact line, character for character
        interpretation.Closing.Should().Be("~ Blessed is the Queen of Heaven ~ Inanna Zami ~");
        interpretation.Closing.Should().Be(ReadingInterpreter.ClosingBlessing);
    }

    [Fact]
    public void an_empty_reading_ends_with_the_closing_blessing_too()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.EmptyReading());

        //Assert - nothing was laid, and the blessing is said over it all the same
        interpretation.Positions.Should().BeEmpty();
        interpretation.Closing.Should().Be("~ Blessed is the Queen of Heaven ~ Inanna Zami ~");
    }

    [Fact]
    public void the_closing_blessing_is_the_same_line_for_every_rosette()
    {
        //Act
        var a = TestData.Interpret(TestData.FullReading());
        var b = TestData.Interpret(TestData.OtherFullReading());
        var c = TestData.Interpret(TestData.PartialReading());
        var d = TestData.Interpret(TestData.SingleCardReading());

        //Assert - the blessing is a constant, not part of the deterministic prose
        b.Closing.Should().Be(a.Closing);
        c.Closing.Should().Be(a.Closing);
        d.Closing.Should().Be(a.Closing);
    }

    [Fact]
    public void the_closing_blessing_is_not_the_closing_invocation()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert - the verse the interpreter composes and the fixed blessing are two things
        interpretation.ClosingInvocation.Should().NotBe(interpretation.Closing);
        interpretation.ClosingInvocation.Should().NotContain(interpretation.Closing);
    }

    [Fact]
    public void the_interpretation_carries_the_reading_it_was_written_from()
    {
        //Arrange
        var reading = TestData.FullReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Reading.Should().BeSameAs(reading);
    }

    [Fact]
    public void two_cards_laid_at_one_station_are_read_once()
    {
        //Arrange
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(0, 1));
        reading.Placements.Add(TestData.Place(0, 2));

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert - the first card laid at the station wins
        interpretation.Positions.Should().HaveCount(1);
        interpretation.Positions[0].Placed.Card.Id.Should().Be(1);
    }

    [Fact]
    public void the_longest_querent_and_question_are_still_interpreted()
    {
        //Arrange
        var reading = TestData.ExtremeReading();

        //Act
        var interpretation = TestData.Interpret(reading);

        //Assert
        interpretation.Positions.Should().HaveCount(9);
        interpretation.Title.Should().Contain("Enheduanna-of-Ur");
        interpretation.Opening.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void every_station_of_a_full_rosette_names_its_spread_title()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        foreach (var position in RosetteSpread.Positions)
        {
            interpretation.Positions.Should().Contain(p => p.Heading.Contains(position.Title));
        }
    }

    [Fact]
    public void a_petal_heading_carries_its_roman_numeral_and_the_heart_does_not()
    {
        //Act
        var interpretation = TestData.Interpret(TestData.FullReading());

        //Assert
        interpretation.Positions[0].Heading.Should().StartWith("The Heart");
        interpretation.Positions[1].Heading.Should().StartWith("I · Heaven");
        interpretation.Positions[8].Heading.Should().StartWith("VIII · The Gift");
    }
}
