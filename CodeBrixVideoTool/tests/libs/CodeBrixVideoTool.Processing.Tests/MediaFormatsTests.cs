using CodeBrixVideoTool.Processing.Formats;
using SilverAssertions;
using System.Linq;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

public class MediaFormatsTests
{
    [Fact]
    public void mode2_always_maps_to_vorbis()
    {
        //Arrange
        var mode2 = MediaFormatKind.CodeBrixMode2;

        //Act
        var codec = MediaFormats.AudioCodecFor(mode2);

        //Assert
        codec.Should().Be(TargetAudioCodec.Vorbis);
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska, TargetAudioCodec.Opus)]
    [InlineData(MediaFormatKind.WebM, TargetAudioCodec.Opus)]
    [InlineData(MediaFormatKind.CodeBrixMode1, TargetAudioCodec.Opus)]
    [InlineData(MediaFormatKind.CodeBrixMode2, TargetAudioCodec.Vorbis)]
    [InlineData(MediaFormatKind.Mp4, TargetAudioCodec.Aac)]
    public void every_destination_has_one_settled_audio_codec(MediaFormatKind kind, TargetAudioCodec expected)
    {
        //Act
        var codec = MediaFormats.AudioCodecFor(kind);

        //Assert
        codec.Should().Be(expected);
    }

    [Fact]
    public void no_supported_format_other_than_mode2_uses_vorbis()
    {
        //Arrange
        var others = MediaFormats.SupportedFormats.Where(f => f != MediaFormatKind.CodeBrixMode2);

        //Act
        var codecs = others.Select(MediaFormats.AudioCodecFor).ToList();

        //Assert
        codecs.Should().AllBeEquivalentTo(TargetAudioCodec.Opus);
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska, TargetVideoCodec.Av1)]
    [InlineData(MediaFormatKind.WebM, TargetVideoCodec.Av1)]
    [InlineData(MediaFormatKind.CodeBrixMode1, TargetVideoCodec.Av1)]
    [InlineData(MediaFormatKind.CodeBrixMode2, TargetVideoCodec.Av1)]
    [InlineData(MediaFormatKind.Mp4, TargetVideoCodec.H264)]
    public void every_destination_has_one_settled_video_codec(MediaFormatKind kind, TargetVideoCodec expected)
    {
        //Act
        var codec = MediaFormats.VideoCodecFor(kind);

        //Assert
        codec.Should().Be(expected);
    }

    [Theory]
    [InlineData(MediaFormatKind.Mp4, MediaFormatKind.CodeBrixMode2, ConversionOperationKind.Import)]
    [InlineData(MediaFormatKind.Mp4, MediaFormatKind.Matroska, ConversionOperationKind.Import)]
    [InlineData(MediaFormatKind.CodeBrixMode2, MediaFormatKind.Matroska, ConversionOperationKind.Transcode)]
    [InlineData(MediaFormatKind.WebM, MediaFormatKind.CodeBrixMode1, ConversionOperationKind.Transcode)]
    [InlineData(MediaFormatKind.CodeBrixMode1, MediaFormatKind.Mp4, ConversionOperationKind.Export)]
    public void the_operation_follows_from_the_mp4_boundary(
        MediaFormatKind source, MediaFormatKind destination, ConversionOperationKind expected)
    {
        //Act
        var operation = MediaFormats.OperationFor(source, destination);

        //Assert
        operation.Should().Be(expected);
    }

    [Fact]
    public void mp4_to_mp4_is_not_a_conversion_this_application_offers()
    {
        //Act
        var act = () => MediaFormats.OperationFor(MediaFormatKind.Mp4, MediaFormatKind.Mp4);

        //Assert
        act.Should().Throw<System.ArgumentException>();
    }

    [Fact]
    public void mp4_is_never_playable_in_the_application()
    {
        //Act
        var playable = MediaFormats.IsPlayable(MediaFormatKind.Mp4);

        //Assert
        playable.Should().BeFalse();
    }

    [Fact]
    public void all_four_supported_formats_are_playable()
    {
        //Act
        var playable = MediaFormats.SupportedFormats.Select(MediaFormats.IsPlayable).ToList();

        //Assert
        playable.Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void an_mp4_source_is_offered_the_four_formats_and_not_itself()
    {
        //Act
        var destinations = MediaFormats.DestinationsFor(MediaFormatKind.Mp4);

        //Assert
        destinations.Should().HaveCount(4);
        destinations.Should().NotContain(MediaFormatKind.Mp4);
    }

    [Fact]
    public void a_mode2_source_is_offered_the_other_three_formats_and_mp4()
    {
        //Act
        var destinations = MediaFormats.DestinationsFor(MediaFormatKind.CodeBrixMode2);

        //Assert
        destinations.Should().HaveCount(4);
        destinations.Should().Contain(MediaFormatKind.Mp4);
        destinations.Should().NotContain(MediaFormatKind.CodeBrixMode2);
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska, ".mkv")]
    [InlineData(MediaFormatKind.WebM, ".webm")]
    [InlineData(MediaFormatKind.CodeBrixMode1, ".cbv")]
    [InlineData(MediaFormatKind.CodeBrixMode2, ".cbv")]
    [InlineData(MediaFormatKind.Mp4, ".mp4")]
    public void every_destination_has_an_extension(MediaFormatKind kind, string expected)
    {
        //Act
        var extension = MediaFormats.Extension(kind);

        //Assert
        extension.Should().Be(expected);
    }
}
