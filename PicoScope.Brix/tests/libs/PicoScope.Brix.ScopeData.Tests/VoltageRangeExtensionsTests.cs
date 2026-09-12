using System;
using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class VoltageRangeExtensionsTests
{
    [Theory]
    [InlineData(VoltageRange.Range10mV, 10)]
    [InlineData(VoltageRange.Range50mV, 50)]
    [InlineData(VoltageRange.Range500mV, 500)]
    [InlineData(VoltageRange.Range1V, 1000)]
    [InlineData(VoltageRange.Range5V, 5000)]
    [InlineData(VoltageRange.Range50V, 50000)]
    public void FullScaleInMillivolts_matches_the_driver_table(VoltageRange range, int expectedMillivolts)
    {
        range.FullScaleInMillivolts().Should().Be(expectedMillivolts);
    }

    [Fact]
    public void FullScaleInMillivolts_throws_for_an_undefined_range()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((VoltageRange)12).FullScaleInMillivolts());
    }

    [Fact]
    public void FullScaleInVolts_is_the_millivolt_figure_scaled()
    {
        VoltageRange.Range2V.FullScaleInVolts().Should().Be(2.0);
        VoltageRange.Range200mV.FullScaleInVolts().Should().BeApproximately(0.2, 1e-12);
    }

    [Fact]
    public void AdcToVolts_maps_full_scale_counts_to_the_range_limits()
    {
        VoltageRange.Range5V.AdcToVolts(ScopeConstants.MaxAdcValue).Should().BeApproximately(5.0, 1e-9);
        VoltageRange.Range5V.AdcToVolts(ScopeConstants.MinAdcValue).Should().BeApproximately(-5.0, 1e-9);
        VoltageRange.Range5V.AdcToVolts(0).Should().Be(0.0);
    }

    [Fact]
    public void AdcToMillivolts_scales_linearly()
    {
        //Arrange: exactly half of full scale on the 1 V range
        int halfScale = ScopeConstants.MaxAdcValue / 2;

        //Act + Assert
        VoltageRange.Range1V.AdcToMillivolts(halfScale).Should().BeApproximately(499.98, 0.01);
    }

    [Fact]
    public void MillivoltsToAdc_rounds_to_the_nearest_count()
    {
        VoltageRange.Range5V.MillivoltsToAdc(1000).Should().Be((short)6553);
        VoltageRange.Range5V.MillivoltsToAdc(5000).Should().Be((short)ScopeConstants.MaxAdcValue);
        VoltageRange.Range5V.MillivoltsToAdc(-5000).Should().Be((short)ScopeConstants.MinAdcValue);
    }

    [Fact]
    public void MillivoltsToAdc_clips_beyond_full_scale()
    {
        VoltageRange.Range5V.MillivoltsToAdc(6000).Should().Be((short)ScopeConstants.MaxAdcValue);
        VoltageRange.Range5V.MillivoltsToAdc(-6000).Should().Be((short)ScopeConstants.MinAdcValue);
    }

    [Fact]
    public void MillivoltsToAdc_and_AdcToMillivolts_round_trip_at_full_scale()
    {
        foreach (VoltageRange range in Enum.GetValues<VoltageRange>())
        {
            short adc = range.MillivoltsToAdc(range.FullScaleInMillivolts());
            adc.Should().Be((short)ScopeConstants.MaxAdcValue);
            range.AdcToMillivolts(adc).Should().BeApproximately(range.FullScaleInMillivolts(), 1e-9);
        }
    }

    [Theory]
    [InlineData(VoltageRange.Range50mV, "+/-50 mV")]
    [InlineData(VoltageRange.Range500mV, "+/-500 mV")]
    [InlineData(VoltageRange.Range1V, "+/-1 V")]
    [InlineData(VoltageRange.Range5V, "+/-5 V")]
    [InlineData(VoltageRange.Range20V, "+/-20 V")]
    public void ToDisplayString_uses_millivolts_below_one_volt(VoltageRange range, string expected)
    {
        range.ToDisplayString().Should().Be(expected);
    }
}
