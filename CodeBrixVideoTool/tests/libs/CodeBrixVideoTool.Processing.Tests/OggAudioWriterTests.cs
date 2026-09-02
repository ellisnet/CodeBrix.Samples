using CodeBrix.VideoPlayback.Containers.Ogg;
using CodeBrix.VideoProcessing;
using CodeBrixVideoTool.Processing.Containers;
using SilverAssertions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

[Collection(SampleMediaCollection.Name)]
public class OggAudioWriterTests
{
    private readonly SampleMediaFixture media;

    public OggAudioWriterTests(SampleMediaFixture media) => this.media = media;

    [Fact]
    public void a_written_vorbis_stream_round_trips_through_the_cores_own_reader()
    {
        //Arrange
        var original = ReadPackets(media.VorbisOggPath, out var codecPrivate, out var sampleRate, out var channels);
        var path = Path.Combine(media.Root, "roundtrip.ogg");

        //Act
        using (var writer = OggAudioWriter.CreateVorbis(path, codecPrivate, sampleRate))
        {
            foreach (var packet in original)
            {
                writer.WritePacket(packet.Data, packet.End);
            }

            writer.Complete();
        }

        var readBack = ReadPackets(path, out var readCodecPrivate, out var readSampleRate, out var readChannels);

        //Assert
        readSampleRate.Should().Be(sampleRate);
        readChannels.Should().Be(channels);
        readCodecPrivate.Should().BeEquivalentTo(codecPrivate);
        readBack.Should().HaveCount(original.Count);
        for (var index = 0; index < original.Count; index++)
        {
            readBack[index].Data.Should().BeEquivalentTo(original[index].Data);
        }
    }

    [Fact]
    public async Task a_written_vorbis_stream_probes_clean()
    {
        //Arrange
        var original = ReadPackets(media.VorbisOggPath, out var codecPrivate, out var sampleRate, out _);
        var path = Path.Combine(media.Root, "probe.ogg");

        //Act
        using (var writer = OggAudioWriter.CreateVorbis(path, codecPrivate, sampleRate))
        {
            foreach (var packet in original)
            {
                writer.WritePacket(packet.Data, packet.End);
            }

            writer.Complete();
        }

        var analysis = await FFProbe.AnalyseAsync(path, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        analysis.PrimaryAudioStream.Should().NotBeNull();
        analysis.PrimaryAudioStream.CodecName.Should().Be("vorbis");
        analysis.PrimaryAudioStream.SampleRateHz.Should().Be(sampleRate);
        analysis.ErrorData.Should().BeEmpty();
    }

    [Fact]
    public void granule_positions_never_move_backwards()
    {
        //Arrange
        var original = ReadPackets(media.VorbisOggPath, out var codecPrivate, out var sampleRate, out _);
        var path = Path.Combine(media.Root, "granules.ogg");

        //Act
        using (var writer = OggAudioWriter.CreateVorbis(path, codecPrivate, sampleRate))
        {
            foreach (var packet in original)
            {
                writer.WritePacket(packet.Data, packet.End);
            }

            writer.Complete();
        }

        var granules = new List<long>();
        using (var source = CodeBrix.VideoPlayback.Sources.MediaSources.OpenFile(path))
        using (var reader = new OggReader(source))
        {
            while (reader.TryReadPacket(out var packet))
            {
                if (packet.GranulePosition >= 0)
                {
                    granules.Add(packet.GranulePosition);
                }
            }
        }

        //Assert
        granules.Should().NotBeEmpty();
        granules.Should().BeInAscendingOrder();
    }

    private static List<(byte[] Data, System.TimeSpan End)> ReadPackets(
        string path, out byte[] codecPrivate, out int sampleRate, out int channels)
    {
        using var stream = OggAudioStream.Open(path);
        codecPrivate = stream.CodecPrivate.ToArray();
        sampleRate = stream.SampleRate;
        channels = stream.Channels;

        var packets = new List<(byte[], System.TimeSpan)>();
        foreach (var packet in stream.ReadAllPackets())
        {
            packets.Add((packet.Data.ToArray(), packet.Timestamp + packet.Duration));
        }

        return packets;
    }
}
