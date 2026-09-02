using CodeBrixVideoTool.Processing;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Probing;
using SilverAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

[Collection(SampleMediaCollection.Name)]
public class MediaProbeTests
{
    private readonly SampleMediaFixture media;

    public MediaProbeTests(SampleMediaFixture media) => this.media = media;

    [Fact]
    public async Task an_mp4_probes_as_an_import_candidate()
    {
        //Arrange
        var probe = new MediaProbe();

        //Act
        var info = await probe.ProbeAsync(media.Mp4Path, TestContext.Current.CancellationToken);

        //Assert
        info.Format.Should().Be(MediaFormatKind.Mp4);
        info.Width.Should().Be(media.Width);
        info.Height.Should().Be(media.Height);
        info.VideoCodec.Should().Be("h264");
        info.AudioCodec.Should().Be("aac");
        info.HasVideo.Should().BeTrue();
        info.IsPlayable.Should().BeFalse();
    }

    [Fact]
    public async Task a_rich_mp4_reports_its_captions_and_chapters()
    {
        //Arrange
        var probe = new MediaProbe();

        //Act
        var info = await probe.ProbeAsync(media.RichMp4Path, TestContext.Current.CancellationToken);

        //Assert
        info.CaptionTrackCount.Should().Be(1);
        info.ChapterCount.Should().Be(3);
    }

    [Fact]
    public async Task a_bespoke_file_probes_through_the_playback_core()
    {
        //Arrange
        var probe = new MediaProbe();

        //Act
        var info = await probe.ProbeAsync(media.Mode2Path, TestContext.Current.CancellationToken);

        //Assert
        info.Format.Should().Be(MediaFormatKind.CodeBrixMode2);
        info.VideoCodec.Should().Be("av01");
        info.AudioCodec.Should().Be("vorbis");
        info.IsPlayable.Should().BeTrue();
    }

    [Fact]
    public async Task a_webm_profile_file_probes_as_mode1()
    {
        //Arrange
        var probe = new MediaProbe();

        //Act
        var info = await probe.ProbeAsync(media.Mode1Path, TestContext.Current.CancellationToken);

        //Assert
        info.Format.Should().Be(MediaFormatKind.CodeBrixMode1);
        info.VideoCodec.Should().Be("av01");
        info.AudioCodec.Should().Be("opus");
    }

    [Fact]
    public async Task a_missing_file_is_refused_with_a_sentence()
    {
        //Arrange
        var probe = new MediaProbe();
        var missing = Path.Combine(media.Root, "not-there.mp4");

        //Act
        var act = async () => await probe.ProbeAsync(missing, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<VideoToolProcessingException>();
    }

    [Fact]
    public async Task a_file_that_is_not_media_is_refused()
    {
        //Arrange
        var probe = new MediaProbe();
        var text = Path.Combine(media.Root, "notes.txt");
        await File.WriteAllTextAsync(text, "not a video", TestContext.Current.CancellationToken);

        //Act
        var act = async () => await probe.ProbeAsync(text, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<VideoToolProcessingException>();
    }

    [Fact]
    public async Task an_audio_only_file_is_refused_because_there_is_no_picture()
    {
        //Arrange
        var probe = new MediaProbe();
        var audioOnly = Path.Combine(media.Root, "audio-only.m4v");
        await CodeBrix.VideoProcessing.FFMpegArguments
            .FromFileInput(media.Mp4Path)
            .OutputToFile(audioOnly, true, options => options
                .DisableChannel(CodeBrix.VideoProcessing.Enums.Channel.Video)
                .WithAudioCodec("aac")
                .ForceFormat("mp4"))
            .ProcessAsynchronously();

        //Act
        var act = async () => await probe.ProbeAsync(audioOnly, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<VideoToolProcessingException>();
    }
}
