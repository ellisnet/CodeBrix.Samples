using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class ScopeCapabilitiesTests
{
    //These are the limits measured on a physical 2204A; the simulator enforces
    //  them and the real device rediscovers them at open.
    [Fact]
    public void PicoScope2204A_has_two_channels_and_nine_ranges()
    {
        //Arrange
        ScopeCapabilities c = ScopeCapabilities.PicoScope2204A;

        //Assert
        c.Variant.Should().Be("2204A");
        c.ChannelCount.Should().Be(2);
        c.SupportedChannels.Should().Equal(new[] { ChannelId.ChannelA, ChannelId.ChannelB });
        c.SupportedRanges.Count.Should().Be(9);
        c.SupportedRanges[0].Should().Be(VoltageRange.Range50mV);
        c.SupportedRanges[^1].Should().Be(VoltageRange.Range20V);
    }

    [Fact]
    public void PicoScope2204A_has_no_external_trigger_and_no_button()
    {
        //Arrange
        ScopeCapabilities c = ScopeCapabilities.PicoScope2204A;

        //Assert
        c.Supports(ChannelId.External).Should().BeFalse();
        c.SupportedTriggerSources.Should().Equal(new[] { ChannelId.ChannelA, ChannelId.ChannelB, ChannelId.None });
        c.SupportsButton.Should().BeFalse();
        c.SupportsFlashLed.Should().BeTrue();
    }

    [Fact]
    public void PicoScope2204A_timebase_and_buffer_limits_depend_on_the_channel_count()
    {
        //Arrange
        ScopeCapabilities c = ScopeCapabilities.PicoScope2204A;

        //Assert
        c.MinTimebaseFor(1).Should().Be(0);
        c.MinTimebaseFor(2).Should().Be(1);
        c.MaxTimebase.Should().Be(23);
        c.MaxSamplesFor(1).Should().Be(8064);
        c.MaxSamplesFor(2).Should().Be(3968);
        c.MaxOversample.Should().Be(4);
    }

    [Fact]
    public void PicoScope2204A_generator_limits()
    {
        //Arrange
        ScopeCapabilities c = ScopeCapabilities.PicoScope2204A;

        //Assert
        c.SupportsSignalGenerator.Should().BeTrue();
        c.SupportsArbitraryWaveform.Should().BeTrue();
        c.MaxSignalGeneratorFrequencyHz.Should().Be(100_000.0);
        c.MaxSignalGeneratorAmplitudeMicrovolts.Should().Be(4_000_000u);
        c.MaxArbitraryWaveformSize.Should().Be(4096);
        c.SupportedWaveTypes.Count.Should().Be(9);
    }

    [Fact]
    public void Supports_answers_for_ranges_and_channels()
    {
        //Arrange
        ScopeCapabilities c = ScopeCapabilities.PicoScope2204A;

        //Assert
        c.Supports(VoltageRange.Range5V).Should().BeTrue();
        c.Supports(VoltageRange.Range10mV).Should().BeFalse();
        c.Supports(VoltageRange.Range50V).Should().BeFalse();
        c.Supports(ChannelId.ChannelB).Should().BeTrue();
        c.Supports(ChannelId.ChannelC).Should().BeFalse();
    }

    [Fact]
    public void an_empty_capabilities_object_supports_nothing()
    {
        //Arrange
        var c = new ScopeCapabilities();

        //Assert
        c.ChannelCount.Should().Be(0);
        c.Supports(VoltageRange.Range5V).Should().BeFalse();
        c.MaxOversample.Should().Be(1);
    }
}
