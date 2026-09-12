using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class ChannelSettingsTests
{
    [Fact]
    public void Default_is_enabled_dc_coupled_at_5_volts()
    {
        ChannelSettings.Default.Enabled.Should().BeTrue();
        ChannelSettings.Default.Coupling.Should().Be(Coupling.Dc);
        ChannelSettings.Default.Range.Should().Be(VoltageRange.Range5V);
    }

    [Fact]
    public void AsDisabled_keeps_everything_but_the_enabled_flag()
    {
        //Arrange
        var settings = new ChannelSettings(true, Coupling.Ac, VoltageRange.Range200mV);

        //Act
        ChannelSettings disabled = settings.AsDisabled();

        //Assert
        disabled.Enabled.Should().BeFalse();
        disabled.Coupling.Should().Be(Coupling.Ac);
        disabled.Range.Should().Be(VoltageRange.Range200mV);
        settings.Enabled.Should().BeTrue();
    }
}
