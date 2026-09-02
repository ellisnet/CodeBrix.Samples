using CodeBrix.VideoPlayback.Captions;
using CodeBrix.VideoPlayback.Chapters;
using CodeBrixVideoTool.Playback.Services;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Probing;
using SilverAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CodeBrixVideoTool.Playback.Tests;

public class PlaybackSelectionTests
{
    private static SourceMediaInfo Item(MediaFormatKind format) => new()
    {
        Path = "/tmp/example" + MediaFormats.Extension(format),
        FileName = "example" + MediaFormats.Extension(format),
        Format = format,
        Duration = TimeSpan.FromMinutes(1),
        Width = 1280,
        Height = 720,
    };

    private static Chapter Chapter(int index, int startSeconds, string title) =>
        new(index, TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(startSeconds + 10), false,
            new Dictionary<string, string> { [string.Empty] = title });

    [Fact]
    public void an_mp4_is_never_opened_in_the_player()
    {
        //Act
        var canOpen = PlaybackSelection.CanOpen(Item(MediaFormatKind.Mp4));

        //Assert
        canOpen.Should().BeFalse();
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska)]
    [InlineData(MediaFormatKind.WebM)]
    [InlineData(MediaFormatKind.CodeBrixMode1)]
    [InlineData(MediaFormatKind.CodeBrixMode2)]
    public void every_supported_format_can_be_opened(MediaFormatKind format)
    {
        //Act
        var canOpen = PlaybackSelection.CanOpen(Item(format));

        //Assert
        canOpen.Should().BeTrue();
    }

    [Fact]
    public void nothing_selected_cannot_be_opened()
    {
        //Act
        var canOpen = PlaybackSelection.CanOpen(null);

        //Assert
        canOpen.Should().BeFalse();
    }

    [Fact]
    public void the_unplayable_message_says_what_to_do_instead()
    {
        //Act
        var message = PlaybackSelection.DescribeUnplayable(Item(MediaFormatKind.Mp4));

        //Assert
        message.Should().Contain("import it");
    }

    [Fact]
    public void chapter_rows_keep_the_files_own_order()
    {
        //Arrange
        var chapters = new List<Chapter> { Chapter(0, 0, "Opening"), Chapter(1, 95, "Closing") };

        //Act
        var rows = PlaybackSelection.BuildChapterRows(chapters);

        //Assert
        rows.Should().HaveCount(2);
        rows[0].Label.Should().Be("00:00  Opening");
        rows[1].Label.Should().Be("01:35  Closing");
    }

    [Fact]
    public void a_file_with_no_chapters_produces_no_rows_and_hides_the_drop_down()
    {
        //Act
        var rows = PlaybackSelection.BuildChapterRows(null);

        //Assert
        rows.Should().BeEmpty();
        PlaybackSelection.ShouldShowChapters(rows.Count).Should().BeFalse();
    }

    [Fact]
    public void caption_rows_always_start_with_an_off_row()
    {
        //Arrange
        var tracks = new List<CaptionTrack>
        {
            new(0, "en", "English", CaptionTrackFlags.Default, CaptionFormat.WebVtt),
        };

        //Act
        var rows = PlaybackSelection.BuildCaptionRows(tracks);

        //Assert
        rows.Should().HaveCount(2);
        rows[0].IsOff.Should().BeTrue();
        rows[1].Label.Should().Be("English (en)");
        PlaybackSelection.ShouldShowCaptions(rows.Count).Should().BeTrue();
    }

    [Fact]
    public void a_file_with_no_captions_gets_only_the_off_row_and_hides_the_drop_down()
    {
        //Act
        var rows = PlaybackSelection.BuildCaptionRows(null);

        //Assert
        rows.Should().HaveCount(1);
        rows[0].IsOff.Should().BeTrue();
        PlaybackSelection.ShouldShowCaptions(rows.Count).Should().BeFalse();
    }

    [Fact]
    public void the_opened_line_names_the_file_its_length_and_what_it_carries()
    {
        //Act
        var line = PlaybackSelection.DescribeOpened("clip.cbv", TimeSpan.FromSeconds(95), 3, 2);

        //Assert
        line.Should().Be("clip.cbv - 00:01:35, 3 chapters, 1 caption track(s).");
    }

    [Fact]
    public void the_opened_line_says_nothing_about_what_the_file_does_not_carry()
    {
        //Act
        var line = PlaybackSelection.DescribeOpened("clip.webm", TimeSpan.FromSeconds(30), 0, 1);

        //Assert
        line.Should().Be("clip.webm - 00:00:30.");
    }
}
