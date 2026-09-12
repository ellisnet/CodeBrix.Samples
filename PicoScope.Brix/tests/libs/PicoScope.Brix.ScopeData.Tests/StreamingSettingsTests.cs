using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class StreamingSettingsTests
{
    [Fact]
    public void defaults_are_fast_streaming_at_100_microseconds()
    {
        //Arrange
        var settings = new StreamingSettings();

        //Assert
        settings.Mode.Should().Be(StreamingMode.Fast);
        settings.SampleInterval.Should().Be(100u);
        settings.IntervalUnits.Should().Be(TimeUnits.Microseconds);
        settings.MaxSamples.Should().Be(1_000_000u);
        settings.AutoStop.Should().BeFalse();
        settings.SamplesPerAggregate.Should().Be(1u);
        settings.OverviewBufferSize.Should().Be(15000u);
        settings.PollIntervalMilliseconds.Should().Be(10);
    }

    [Theory]
    [InlineData(TimeUnits.Femtoseconds, 2_000_000u, 2.0)]
    [InlineData(TimeUnits.Picoseconds, 5000u, 5.0)]
    [InlineData(TimeUnits.Nanoseconds, 250u, 250.0)]
    [InlineData(TimeUnits.Microseconds, 100u, 100_000.0)]
    [InlineData(TimeUnits.Milliseconds, 2u, 2_000_000.0)]
    [InlineData(TimeUnits.Seconds, 1u, 1_000_000_000.0)]
    public void IntervalInNanoseconds_converts_every_unit(TimeUnits units, uint interval, double expectedNs)
    {
        //Arrange
        var settings = new StreamingSettings { SampleInterval = interval, IntervalUnits = units };

        //Assert
        settings.IntervalInNanoseconds().Should().BeApproximately(expectedNs, 1e-9);
    }

    [Fact]
    public void StreamingSamplesEventArgs_defaults_to_an_empty_batch()
    {
        //Arrange
        var args = new StreamingSamplesEventArgs();

        //Assert
        args.Channels.Count.Should().Be(0);
        args.SampleCount.Should().Be(0);
        args.TotalSampleCount.Should().Be(0L);
        args.Triggered.Should().BeFalse();
        args.BufferOverrun.Should().BeFalse();
    }
}
