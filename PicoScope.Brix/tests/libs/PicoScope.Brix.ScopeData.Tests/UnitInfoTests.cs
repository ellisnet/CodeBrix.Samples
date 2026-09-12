using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class UnitInfoTests
{
    [Fact]
    public void ToString_names_the_variant_serial_and_calibration_date()
    {
        //Arrange
        var info = new UnitInfo("2204A", "10066/1927", "05Feb24", "3.0", "17", "2.0", "n/a", "/opt/picoscope/lib/libps2000.so", "0");

        //Assert
        info.ToString().Should().Be("PicoScope 2204A (serial 10066/1927, calibrated 05Feb24)");
        info.DriverPath.Should().Be("/opt/picoscope/lib/libps2000.so");
    }

    [Fact]
    public void UnitInfoLine_ordinals_match_the_driver()
    {
        ((int)UnitInfoLine.DriverVersion).Should().Be(0);
        ((int)UnitInfoLine.VariantInfo).Should().Be(3);
        ((int)UnitInfoLine.BatchAndSerial).Should().Be(4);
        ((int)UnitInfoLine.DriverPath).Should().Be(8);
    }
}
