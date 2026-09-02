using CodeBrix.VideoPlayback.Captions;
using CodeBrix.VideoPlayback.Chapters;
using CodeBrixVideoTool.Playback.Models;
using SilverAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeBrixVideoTool.Playback.Tests;

public class PlaybackModelsTests
{
    [Fact]
    public void a_chapter_row_shows_its_start_time_and_title()
    {
        //Arrange
        var chapter = new Chapter(2, TimeSpan.FromSeconds(95), TimeSpan.FromSeconds(120), false,
            new Dictionary<string, string> { [string.Empty] = "Closing" });

        //Act
        var row = ChapterEntry.From(chapter);

        //Assert
        row.Index.Should().Be(2);
        row.Label.Should().Be("01:35  Closing");
    }

    [Fact]
    public void a_chapter_with_no_title_is_numbered()
    {
        //Arrange
        var chapter = new Chapter(0, TimeSpan.Zero, TimeSpan.FromSeconds(10), false,
            new Dictionary<string, string>());

        //Act
        var row = ChapterEntry.From(chapter);

        //Assert
        row.Label.Should().Be("00:00  Chapter 1");
    }

    [Fact]
    public void a_caption_row_shows_its_name_and_language()
    {
        //Arrange
        var track = new CaptionTrack(0, "en-GB", "Commentary", CaptionTrackFlags.None, CaptionFormat.WebVtt);

        //Act
        var row = CaptionEntry.From(0, track);

        //Assert
        row.Label.Should().Be("Commentary (en-GB)");
        row.IsOff.Should().BeFalse();
    }

    [Fact]
    public void a_caption_row_with_only_a_language_shows_that()
    {
        //Arrange
        var track = new CaptionTrack(0, "de", null, CaptionTrackFlags.None, CaptionFormat.WebVtt);

        //Act
        var row = CaptionEntry.From(0, track);

        //Assert
        row.Label.Should().Be("de");
    }

    [Fact]
    public void a_forced_caption_row_says_so()
    {
        //Arrange
        var track = new CaptionTrack(0, "fr", "Signs", CaptionTrackFlags.Forced, CaptionFormat.WebVtt);

        //Act
        var row = CaptionEntry.From(0, track);

        //Assert
        row.Label.Should().Contain("forced");
    }

    [Fact]
    public void the_off_row_carries_no_track()
    {
        //Act
        var row = CaptionEntry.Off;

        //Assert
        row.IsOff.Should().BeTrue();
        row.Track.Should().BeNull();
        row.Index.Should().Be(-1);
    }
}
