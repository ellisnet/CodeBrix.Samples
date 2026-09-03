using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.ViewModels;
using SilverAssertions;
using System;
using System.Linq;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

/// <summary>
/// The operation panel's own rules, as far as a test process can reach them: a SimpleViewModel cannot
/// be constructed without a running head, so what is proven here is the static rule the view model
/// fills <c>LastRunNotes</c> from. The panel itself is exercised by the scripted run on a real head.
/// </summary>
public class ConversionViewModelTests
{
    private static ConversionOutcome Outcome(string verdict, bool passes, params string[] notes) =>
        ConversionOutcome.Success("/tmp/out.cbv", 1024, TimeSpan.FromSeconds(3), verdict, passes, notes, []);

    [Fact]
    public void a_profile_that_passes_reads_as_pass_and_comes_before_the_notes()
    {
        //Arrange
        var outcome = Outcome("passes the profile", true, "Chapters carried across.");

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.CodeBrixMode2);

        //Assert
        lines.Should().HaveCount(2);
        lines[0].Should().Be("Streamable profile: PASS");
        lines[1].Should().Be("Chapters carried across.");
    }

    [Fact]
    public void a_standard_mkv_that_fails_the_profile_says_the_failure_is_expected()
    {
        //Arrange
        var outcome = Outcome("cues sit before the first cluster", false);

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.Matroska);

        //Assert
        lines.Should().HaveCount(1);
        lines[0].Should().Be(
            "Streamable profile: FAIL - cues sit before the first cluster (expected for a standard MKV)");
    }

    [Fact]
    public void a_failure_anywhere_but_a_standard_mkv_is_not_called_expected()
    {
        //Arrange
        var outcome = Outcome("cues sit before the first cluster", false);

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.WebM);

        //Assert
        lines[0].Should().Be("Streamable profile: FAIL - cues sit before the first cluster");
    }

    [Fact]
    public void an_export_has_no_profile_line_because_nothing_was_checked()
    {
        //Arrange
        var outcome = ConversionOutcome.Success(
            "/tmp/out.mp4", 2048, TimeSpan.FromSeconds(4), null, false, ["Chapters carried across."], []);

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.Mp4);

        //Assert
        lines.Should().HaveCount(1);
        lines[0].Should().Be("Chapters carried across.");
    }

    [Fact]
    public void every_note_the_run_produced_is_shown_in_its_own_order()
    {
        //Arrange
        var outcome = Outcome(null, false,
            "Audio downmixed from 6 channels to stereo: this application writes mono or stereo audio only.",
            "1 chapter-title language(s) dropped: this application carries one title per chapter.",
            "Chapters carried across.");

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.CodeBrixMode2);

        //Assert
        lines.Should().HaveCount(3);
        lines.First().Should().Contain("downmixed from 6 channels");
        lines.Last().Should().Be("Chapters carried across.");
    }

    [Fact]
    public void a_run_that_had_nothing_to_say_shows_nothing()
    {
        //Arrange
        var outcome = Outcome(null, false);

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.WebM);

        //Assert
        lines.Should().BeEmpty();
    }

    [Fact]
    public void a_cancelled_run_shows_whatever_it_had_got_as_far_as_reporting()
    {
        //Arrange
        var outcome = ConversionOutcome.Cancelled(TimeSpan.FromSeconds(1), ["Chapters carried across."]);

        //Act
        var lines = ConversionViewModel.DescribeOutcome(outcome, MediaFormatKind.CodeBrixMode1);

        //Assert
        lines.Should().HaveCount(1);
        lines[0].Should().Be("Chapters carried across.");
    }

    [Fact]
    public void nothing_at_all_describes_to_nothing_at_all()
    {
        //Act
        var lines = ConversionViewModel.DescribeOutcome(null, MediaFormatKind.WebM);

        //Assert
        lines.Should().BeEmpty();
    }
}
