using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class TriggerSettingsTests
{
    [Fact]
    public void Disabled_has_no_source()
    {
        TriggerSettings.Disabled.Source.Should().Be(ChannelId.None);
        TriggerSettings.Disabled.ThresholdAdc.Should().Be((short)0);
    }

    [Fact]
    public void RisingEdge_converts_the_threshold_through_the_range()
    {
        //Act
        TriggerSettings trigger = TriggerSettings.RisingEdge(ChannelId.ChannelA, VoltageRange.Range5V, 1000);

        //Assert
        trigger.Source.Should().Be(ChannelId.ChannelA);
        trigger.Direction.Should().Be(TriggerDirection.Rising);
        trigger.ThresholdAdc.Should().Be((short)6553);
        trigger.AutoTriggerMilliseconds.Should().Be((short)1000);
        trigger.DelaySamples.Should().Be(0f);
    }

    [Fact]
    public void FallingEdge_uses_the_falling_direction_and_the_given_auto_trigger()
    {
        //Act
        TriggerSettings trigger = TriggerSettings.FallingEdge(ChannelId.ChannelB, VoltageRange.Range1V, -250, 50);

        //Assert
        trigger.Source.Should().Be(ChannelId.ChannelB);
        trigger.Direction.Should().Be(TriggerDirection.Falling);
        trigger.ThresholdAdc.Should().Be((short)-8192);
        trigger.AutoTriggerMilliseconds.Should().Be((short)50);
    }
}
