using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrixVideoTool.Processing.Formats;
using SilverAssertions;
using System;
using System.IO;
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
    [InlineData(MediaFormatKind.CodeBrixMaster, TargetAudioCodec.Flac)]
    [InlineData(MediaFormatKind.Mp4, TargetAudioCodec.Aac)]
    public void every_destination_has_one_settled_audio_codec(MediaFormatKind kind, TargetAudioCodec expected)
    {
        //Act
        var codec = MediaFormats.AudioCodecFor(kind);

        //Assert
        codec.Should().Be(expected);
    }

    [Fact]
    public void no_supported_format_other_than_the_two_bespoke_ones_leaves_opus()
    {
        //Arrange
        var others = MediaFormats.SupportedFormats.Where(f => !MediaFormats.IsBespokeContainer(f));

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
    [InlineData(MediaFormatKind.CodeBrixMaster, TargetVideoCodec.Av1)]
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
    [InlineData(MediaFormatKind.Mp4, MediaFormatKind.CodeBrixMaster, ConversionOperationKind.Import)]
    [InlineData(MediaFormatKind.CodeBrixMaster, MediaFormatKind.CodeBrixMode2, ConversionOperationKind.Transcode)]
    [InlineData(MediaFormatKind.CodeBrixMaster, MediaFormatKind.Mp4, ConversionOperationKind.Export)]
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
    public void all_five_supported_formats_are_playable()
    {
        //Act
        var playable = MediaFormats.SupportedFormats.Select(MediaFormats.IsPlayable).ToList();

        //Assert
        playable.Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void an_mp4_source_is_offered_the_five_formats_and_not_itself()
    {
        //Act
        var destinations = MediaFormats.DestinationsFor(MediaFormatKind.Mp4);

        //Assert
        destinations.Should().HaveCount(5);
        destinations.Should().Contain(MediaFormatKind.CodeBrixMaster);
        destinations.Should().NotContain(MediaFormatKind.Mp4);
    }

    [Fact]
    public void a_mode2_source_is_offered_the_other_four_formats_and_mp4()
    {
        //Act
        var destinations = MediaFormats.DestinationsFor(MediaFormatKind.CodeBrixMode2);

        //Assert
        destinations.Should().HaveCount(5);
        destinations.Should().Contain(MediaFormatKind.Mp4);
        destinations.Should().Contain(MediaFormatKind.CodeBrixMaster);
        destinations.Should().NotContain(MediaFormatKind.CodeBrixMode2);
    }

    [Fact]
    public void a_master_source_is_offered_the_other_four_formats_and_mp4()
    {
        //Act
        var destinations = MediaFormats.DestinationsFor(MediaFormatKind.CodeBrixMaster);

        //Assert
        destinations.Should().HaveCount(5);
        destinations.Should().Contain(MediaFormatKind.Mp4);
        destinations.Should().NotContain(MediaFormatKind.CodeBrixMaster);
    }

    [Fact]
    public void only_the_master_carries_alpha()
    {
        //Act
        var carriers = MediaFormats.SupportedFormats.Append(MediaFormatKind.Mp4).Where(MediaFormats.CarriesAlpha).ToList();

        //Assert
        carriers.Should().Equal(MediaFormatKind.CodeBrixMaster);
    }

    [Theory]
    [InlineData(MediaFormatKind.CodeBrixMaster, MediaFormatKind.Mp4, false)]
    [InlineData(MediaFormatKind.CodeBrixMaster, MediaFormatKind.CodeBrixMode2, true)]
    [InlineData(MediaFormatKind.CodeBrixMode2, MediaFormatKind.Mp4, true)]
    [InlineData(MediaFormatKind.Mp4, MediaFormatKind.CodeBrixMaster, true)]
    public void only_a_master_exported_to_mp4_keeps_its_own_size(
        MediaFormatKind source, MediaFormatKind destination, bool expected)
    {
        //Act
        var canResize = MediaFormats.CanResize(source, destination);

        //Assert
        canResize.Should().Be(expected);
    }

    [Theory]
    [InlineData(".cbv", 0, MediaFormatKind.CodeBrixMode2)]
    [InlineData(".cbv", 1, MediaFormatKind.CodeBrixMaster)]
    [InlineData(".cbvmaster", 1, MediaFormatKind.CodeBrixMaster)]
    [InlineData(".cbvmaster", 0, MediaFormatKind.CodeBrixMode2)]
    public void a_bespoke_file_is_told_apart_by_its_format_version_never_its_extension(
        string extension, int version, MediaFormatKind expected)
    {
        //Arrange - only the magic and the version are read, so a header that stops there is enough.
        var path = Path.Combine(Path.GetTempPath(), "cbvt-detect-" + Guid.NewGuid().ToString("N") + extension);
        File.WriteAllBytes(path, [(byte)'C', (byte)'B', (byte)'V', (byte)'F', (byte)version, 0, 0, 0]);

        try
        {
            //Act
            var kind = MediaFormats.Detect(path);

            //Assert
            kind.Should().Be(expected);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void a_webm_profile_file_named_cbvmaster_is_still_mode1()
    {
        //Arrange
        var path = Path.Combine(Path.GetTempPath(), "cbvt-detect-" + Guid.NewGuid().ToString("N") + ".cbvmaster");
        File.WriteAllBytes(path, [.. CbvFormat.EbmlMagic, 0, 0]);

        try
        {
            //Act
            var kind = MediaFormats.Detect(path);

            //Assert
            kind.Should().Be(MediaFormatKind.CodeBrixMode1);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void the_open_dialog_lists_every_playable_extension()
    {
        //Act
        var extensions = MediaFormats.PlayableExtensions;

        //Assert
        extensions.Should().Contain(".cbv");
        extensions.Should().Contain(".cbvmaster");
        extensions.Should().Contain(".mkv");
        extensions.Should().Contain(".webm");
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska, ".mkv")]
    [InlineData(MediaFormatKind.WebM, ".webm")]
    [InlineData(MediaFormatKind.CodeBrixMode1, ".cbv")]
    [InlineData(MediaFormatKind.CodeBrixMode2, ".cbv")]
    [InlineData(MediaFormatKind.CodeBrixMaster, ".cbvmaster")]
    [InlineData(MediaFormatKind.Mp4, ".mp4")]
    public void every_destination_has_an_extension(MediaFormatKind kind, string expected)
    {
        //Act
        var extension = MediaFormats.Extension(kind);

        //Assert
        extension.Should().Be(expected);
    }

    [Theory]
    [InlineData(MediaFormatKind.Matroska, 6, 2)]
    [InlineData(MediaFormatKind.WebM, 6, 2)]
    [InlineData(MediaFormatKind.CodeBrixMode1, 6, 2)]
    [InlineData(MediaFormatKind.CodeBrixMode2, 6, 2)]
    [InlineData(MediaFormatKind.CodeBrixMode2, 2, 2)]
    [InlineData(MediaFormatKind.CodeBrixMode1, 1, 1)]
    [InlineData(MediaFormatKind.CodeBrixMode2, 0, 1)]
    [InlineData(MediaFormatKind.CodeBrixMaster, 6, 2)]
    [InlineData(MediaFormatKind.CodeBrixMaster, 1, 1)]
    [InlineData(MediaFormatKind.Mp4, 6, 6)]
    [InlineData(MediaFormatKind.Mp4, 8, 8)]
    [InlineData(MediaFormatKind.Mp4, 10, 8)]
    public void the_five_written_formats_cap_at_stereo_and_nothing_is_ever_upmixed(
        MediaFormatKind destination, int sourceChannels, int expected)
    {
        //Act
        var channels = MediaFormats.AudioChannelsFor(destination, sourceChannels);

        //Assert
        channels.Should().Be(expected);
    }

    [Fact]
    public void every_format_this_application_writes_is_capped_at_stereo()
    {
        //Act
        var ceilings = MediaFormats.SupportedFormats.Select(MediaFormats.MaxAudioChannels).ToList();

        //Assert
        ceilings.Should().AllBeEquivalentTo(2);
    }

    [Fact]
    public void an_mp4_export_is_not_capped_at_stereo()
    {
        //Act
        var ceiling = MediaFormats.MaxAudioChannels(MediaFormatKind.Mp4);

        //Assert
        ceiling.Should().Be(8);
    }

    [Fact]
    public void the_four_quality_stops_run_from_fair_to_best()
    {
        //Act
        var levels = MediaFormats.QualityLevels;

        //Assert
        levels.Should().HaveCount(4);
        levels[0].Should().Be(QualityLevel.Fair);
        levels[1].Should().Be(QualityLevel.Good);
        levels[2].Should().Be(QualityLevel.Better);
        levels[3].Should().Be(QualityLevel.Best);
    }

    [Theory]
    [InlineData(".cbv", "CodeBrix video")]
    [InlineData(".cbvmaster", "CodeBrix master video")]
    [InlineData(".mkv", "Matroska video")]
    [InlineData(".webm", "WebM video")]
    [InlineData(".mp4", "MP4 video")]
    public void DescribeExtension_labels_each_extension_a_save_dialog_is_given(string extension, string expected)
    {
        //Act
        var label = MediaFormats.DescribeExtension(extension);

        //Assert
        label.Should().Be(expected);
    }

    [Fact]
    public void DescribeExtension_labels_every_extension_this_application_writes()
    {
        //Arrange
        var extensions = MediaFormats.SupportedFormats
            .Append(MediaFormatKind.Mp4)
            .Select(MediaFormats.Extension)
            .Distinct();

        //Act
        var labels = extensions.Select(MediaFormats.DescribeExtension).ToList();

        //Assert
        labels.Should().HaveCount(5);
        labels.Should().OnlyHaveUniqueItems();
        labels.Should().NotContain(string.Empty);
    }
}
