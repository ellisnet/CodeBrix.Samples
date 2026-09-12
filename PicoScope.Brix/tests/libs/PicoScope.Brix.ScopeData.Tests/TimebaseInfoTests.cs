using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class TimebaseInfoTests
{
    [Fact]
    public void IntervalSeconds_and_SampleRateHz_derive_from_the_nanosecond_interval()
    {
        //Arrange
        var info = new TimebaseInfo(7, true, 1280, TimeUnits.Nanoseconds, 3968, 1);

        //Assert
        info.IntervalSeconds.Should().BeApproximately(1.28e-6, 1e-15);
        info.SampleRateHz.Should().BeApproximately(781_250, 1e-6);
    }

    [Fact]
    public void SampleRateHz_is_zero_for_a_zero_interval()
    {
        new TimebaseInfo(0, true, 0, TimeUnits.Nanoseconds, 0, 1).SampleRateHz.Should().Be(0.0);
    }

    [Fact]
    public void Invalid_carries_the_request_and_nothing_else()
    {
        //Act
        TimebaseInfo invalid = TimebaseInfo.Invalid(24, 2);

        //Assert
        invalid.IsValid.Should().BeFalse();
        invalid.Timebase.Should().Be(24);
        invalid.Oversample.Should().Be(2);
        invalid.IntervalNanoseconds.Should().Be(0);
        invalid.MaxSamples.Should().Be(0);
    }
}
