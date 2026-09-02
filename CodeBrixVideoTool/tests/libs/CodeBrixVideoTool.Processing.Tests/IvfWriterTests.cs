using CodeBrix.VideoPlayback.Containers.Ivf;
using CodeBrix.VideoPlayback.Sources;
using CodeBrixVideoTool.Processing.Containers;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

[Collection(SampleMediaCollection.Name)]
public class IvfWriterTests
{
    private readonly SampleMediaFixture media;

    public IvfWriterTests(SampleMediaFixture media) => this.media = media;

    [Fact]
    public void a_written_file_round_trips_through_the_cores_own_reader()
    {
        //Arrange
        var frames = ReadFrames(media.Av1IvfPath, out var width, out var height);
        var path = Path.Combine(media.Root, "roundtrip.ivf");

        //Act
        using (var writer = IvfWriter.CreateAv1(path, width, height))
        {
            foreach (var frame in frames)
            {
                writer.WriteFrame(frame.Data, frame.Timestamp);
            }

            writer.Complete();
        }

        var readBack = ReadFrames(path, out var readWidth, out var readHeight);

        //Assert
        readWidth.Should().Be(width);
        readHeight.Should().Be(height);
        readBack.Should().HaveCount(frames.Count);
        for (var index = 0; index < frames.Count; index++)
        {
            readBack[index].Timestamp.Should().Be(frames[index].Timestamp);
            readBack[index].Data.Should().BeEquivalentTo(frames[index].Data);
        }
    }

    [Fact]
    public void the_header_states_a_tick_time_base_and_the_real_frame_count()
    {
        //Arrange
        var path = Path.Combine(media.Root, "timebase.ivf");

        //Act
        using (var writer = IvfWriter.CreateAv1(path, 320, 240))
        {
            writer.WriteFrame([1, 2, 3], TimeSpan.FromMilliseconds(40));
            writer.WriteFrame([4, 5, 6, 7], TimeSpan.FromMilliseconds(80));
            writer.Complete();
        }

        using var source = MediaSources.OpenFile(path);
        using var reader = new IvfReader(source);

        //Assert
        reader.FourCharacterCode.Should().Be("AV01");
        reader.TimeBaseDenominator.Should().Be(IvfWriter.TickTimeBaseDenominator);
        reader.TimeBaseNumerator.Should().Be(1u);
        reader.DeclaredFrameCount.Should().Be(2u);
        reader.Width.Should().Be(320);
        reader.Height.Should().Be(240);
    }

    [Fact]
    public void a_negative_timestamp_is_refused()
    {
        //Arrange
        var path = Path.Combine(media.Root, "negative.ivf");
        using var writer = IvfWriter.CreateAv1(path, 320, 240);

        //Act
        var act = () => writer.WriteFrame([1], TimeSpan.FromSeconds(-1));

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static List<(byte[] Data, TimeSpan Timestamp)> ReadFrames(string path, out int width, out int height)
    {
        using var source = MediaSources.OpenFile(path);
        using var reader = new IvfReader(source);

        width = reader.Width;
        height = reader.Height;

        var frames = new List<(byte[], TimeSpan)>();
        while (reader.TryReadFrame(out var data, out var timestamp, out _))
        {
            frames.Add((data.ToArray(), timestamp));
        }

        return frames;
    }
}
