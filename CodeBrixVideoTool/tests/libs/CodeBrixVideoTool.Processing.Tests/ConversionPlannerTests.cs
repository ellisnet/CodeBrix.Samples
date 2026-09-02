using CodeBrixVideoTool.Processing;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using SilverAssertions;
using System;
using System.Linq;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

public class ConversionPlannerTests
{
    private static SourceMediaInfo Source(MediaFormatKind format, int width = 1920, int height = 1080) => new()
    {
        Path = "/tmp/example" + MediaFormats.Extension(format),
        FileName = "example" + MediaFormats.Extension(format),
        Format = format,
        Duration = TimeSpan.FromMinutes(2),
        Width = width,
        Height = height,
        AudioChannels = 2,
        AudioSampleRateHz = 48000,
    };

    [Fact]
    public void an_mp4_source_to_mode2_is_an_import_that_writes_vorbis()
    {
        //Arrange
        var source = Source(MediaFormatKind.Mp4);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.CodeBrixMode2, "/tmp/out.cbv", null);

        //Assert
        plan.Operation.Should().Be(ConversionOperationKind.Import);
        plan.AudioCodec.Should().Be(TargetAudioCodec.Vorbis);
        plan.VideoCodec.Should().Be(TargetVideoCodec.Av1);
        plan.ActionVerb.Should().Be("Import");
    }

    [Fact]
    public void a_mode2_source_needs_demultiplexing_and_says_so_in_its_steps()
    {
        //Arrange
        var source = Source(MediaFormatKind.CodeBrixMode2);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.Matroska, "/tmp/out.mkv", null);

        //Assert
        plan.RequiresMode2Extraction.Should().BeTrue();
        plan.Operation.Should().Be(ConversionOperationKind.Transcode);
        plan.AudioCodec.Should().Be(TargetAudioCodec.Opus);
        plan.Steps.First().Should().Contain("Demultiplex");
    }

    [Fact]
    public void an_ffmpeg_readable_source_needs_no_demultiplexing()
    {
        //Arrange
        var source = Source(MediaFormatKind.CodeBrixMode1);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.CodeBrixMode2, "/tmp/out.cbv", null);

        //Assert
        plan.RequiresMode2Extraction.Should().BeFalse();
    }

    [Fact]
    public void an_export_to_mp4_writes_h264_and_aac()
    {
        //Arrange
        var source = Source(MediaFormatKind.CodeBrixMode2);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.Mp4, "/tmp/out.mp4", null);

        //Assert
        plan.Operation.Should().Be(ConversionOperationKind.Export);
        plan.AudioCodec.Should().Be(TargetAudioCodec.Aac);
        plan.VideoCodec.Should().Be(TargetVideoCodec.H264);
        plan.ActionVerb.Should().Be("Export");
    }

    [Fact]
    public void converting_a_format_to_itself_is_refused()
    {
        //Arrange
        var source = Source(MediaFormatKind.WebM);

        //Act
        var act = () => ConversionPlanner.Create(source, MediaFormatKind.WebM, "/tmp/out.webm", null);

        //Assert
        act.Should().Throw<VideoToolProcessingException>();
    }

    [Fact]
    public void writing_over_the_source_is_refused()
    {
        //Arrange
        var source = Source(MediaFormatKind.WebM);

        //Act
        var act = () => ConversionPlanner.Create(source, MediaFormatKind.Matroska, source.Path, null);

        //Assert
        act.Should().Throw<VideoToolProcessingException>();
    }

    [Fact]
    public void no_chosen_rung_means_the_source_size()
    {
        //Arrange
        var source = Source(MediaFormatKind.Mp4, 1280, 720);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.WebM, "/tmp/out.webm", null);

        //Assert
        plan.IsResized.Should().BeFalse();
        plan.Resolution.Width.Should().Be(1280);
        plan.Resolution.Height.Should().Be(720);
    }

    [Fact]
    public void a_chosen_rung_is_carried_into_the_plan()
    {
        //Arrange
        var source = Source(MediaFormatKind.Mp4);
        var rung = ResolutionLadder.Build(source.Width, source.Height).Single(r => r.Height == 720);

        //Act
        var plan = ConversionPlanner.Create(source, MediaFormatKind.CodeBrixMode1, "/tmp/out.cbv", rung);

        //Assert
        plan.IsResized.Should().BeTrue();
        plan.Resolution.Width.Should().Be(1280);
        plan.Resolution.Height.Should().Be(720);
    }

    [Theory]
    [InlineData(MediaFormatKind.CodeBrixMode1, "example-mode1.cbv")]
    [InlineData(MediaFormatKind.CodeBrixMode2, "example-mode2.cbv")]
    [InlineData(MediaFormatKind.Matroska, "example.mkv")]
    [InlineData(MediaFormatKind.WebM, "example.webm")]
    public void the_suggested_name_says_which_flavour_it_is(MediaFormatKind destination, string expected)
    {
        //Arrange
        var source = Source(MediaFormatKind.Mp4);

        //Act
        var name = ConversionPlanner.SuggestOutputFileName(source, destination);

        //Assert
        name.Should().Be(expected);
    }
}
