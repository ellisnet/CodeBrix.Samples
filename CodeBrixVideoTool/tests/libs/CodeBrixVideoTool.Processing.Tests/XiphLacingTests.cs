using CodeBrix.VideoPlayback.Containers.Ogg;
using CodeBrixVideoTool.Processing;
using CodeBrixVideoTool.Processing.Containers;
using SilverAssertions;
using System;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

[Collection(SampleMediaCollection.Name)]
public class XiphLacingTests
{
    private readonly SampleMediaFixture media;

    public XiphLacingTests(SampleMediaFixture media) => this.media = media;

    [Fact]
    public void splitting_undoes_the_cores_own_lacing()
    {
        //Arrange
        var identification = new byte[300];
        var comment = new byte[40];
        var setup = new byte[1000];
        Random.Shared.NextBytes(identification);
        Random.Shared.NextBytes(comment);
        Random.Shared.NextBytes(setup);
        var laced = OggAudioStream.BuildXiphCodecPrivate(identification, comment, setup);

        //Act
        XiphLacing.SplitVorbisHeaders(laced, out var first, out var second, out var third);

        //Assert
        first.Should().BeEquivalentTo(identification);
        second.Should().BeEquivalentTo(comment);
        third.Should().BeEquivalentTo(setup);
    }

    [Fact]
    public void a_real_vorbis_tracks_headers_split_into_three()
    {
        //Arrange
        using var stream = OggAudioStream.Open(media.VorbisOggPath);

        //Act
        XiphLacing.SplitVorbisHeaders(stream.CodecPrivate.Span, out var identification, out var comment, out var setup);

        //Assert
        identification.Should().HaveCount(30);
        identification[0].Should().Be(1);
        comment[0].Should().Be(3);
        setup[0].Should().Be(5);
    }

    [Fact]
    public void a_block_that_does_not_declare_three_packets_is_refused()
    {
        //Arrange
        var wrong = new byte[] { 5, 1, 1, 1, 1, 1 };

        //Act
        var act = () => XiphLacing.SplitVorbisHeaders(wrong, out _, out _, out _);

        //Assert
        act.Should().Throw<VideoToolProcessingException>();
    }

    [Fact]
    public void a_truncated_block_is_refused()
    {
        //Arrange
        var truncated = new byte[] { 2 };

        //Act
        var act = () => XiphLacing.SplitVorbisHeaders(truncated, out _, out _, out _);

        //Assert
        act.Should().Throw<VideoToolProcessingException>();
    }
}
