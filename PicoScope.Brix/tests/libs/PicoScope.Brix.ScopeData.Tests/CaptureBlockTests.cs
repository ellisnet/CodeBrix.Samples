using System.Collections.Generic;
using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class CaptureBlockTests
{
    private static CaptureBlock Block(int sampleCount, double intervalNs, short overflow = 0) => new CaptureBlock
    {
        Channels = new Dictionary<ChannelId, ChannelSamples>
        {
            [ChannelId.ChannelA] = new ChannelSamples(ChannelId.ChannelA, VoltageRange.Range5V, new short[sampleCount])
        },
        IntervalNanoseconds = intervalNs,
        SampleCount = sampleCount,
        OverflowFlags = overflow
    };

    [Fact]
    public void TimeSecondsAt_multiplies_the_index_by_the_interval()
    {
        Block(2000, 1000).TimeSecondsAt(3).Should().BeApproximately(3e-6, 1e-15);
        Block(2000, 1000).TimeSecondsAt(0).Should().Be(0.0);
    }

    [Fact]
    public void DurationSeconds_spans_the_whole_block()
    {
        Block(2000, 1000).DurationSeconds.Should().BeApproximately(0.002, 1e-12);
    }

    [Fact]
    public void DidOverflow_reads_one_bit_per_channel()
    {
        //Arrange: bit 1 set means channel B overflowed
        CaptureBlock block = Block(10, 10, overflow: 0b10);

        //Assert
        block.DidOverflow(ChannelId.ChannelA).Should().BeFalse();
        block.DidOverflow(ChannelId.ChannelB).Should().BeTrue();
    }

    [Fact]
    public void defaults_are_an_empty_block_with_nanosecond_timestamps()
    {
        //Arrange
        var block = new CaptureBlock();

        //Assert
        block.Channels.Count.Should().Be(0);
        block.SampleCount.Should().Be(0);
        block.Timestamps.Should().BeNull();
        block.TimestampUnits.Should().Be(TimeUnits.Nanoseconds);
        block.DurationSeconds.Should().Be(0.0);
    }

    [Fact]
    public void ChannelSamples_converts_counts_to_volts_through_the_range()
    {
        //Arrange
        var samples = new ChannelSamples(ChannelId.ChannelA, VoltageRange.Range2V,
            new short[] { ScopeConstants.MaxAdcValue, 0, ScopeConstants.MinAdcValue });

        //Assert
        samples.Count.Should().Be(3);
        samples.VoltsAt(0).Should().BeApproximately(2.0, 1e-9);
        samples.MillivoltsAt(1).Should().Be(0.0);
        samples.VoltsAt(2).Should().BeApproximately(-2.0, 1e-9);
    }

    [Fact]
    public void ChannelSamples_flags_the_lost_data_sentinel()
    {
        //Arrange
        var samples = new ChannelSamples(ChannelId.ChannelB, VoltageRange.Range5V,
            new short[] { 100, unchecked((short)ScopeConstants.LostData) });

        //Assert
        samples.IsLostData(0).Should().BeFalse();
        samples.IsLostData(1).Should().BeTrue();
    }

    [Fact]
    public void ChannelSamples_with_no_array_has_no_samples()
    {
        new ChannelSamples(ChannelId.ChannelA, VoltageRange.Range5V, null).Count.Should().Be(0);
    }
}
