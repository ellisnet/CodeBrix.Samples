using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class EtsResultTests
{
    [Fact]
    public void IsSupported_means_a_non_zero_effective_interval()
    {
        new EtsResult(EtsMode.Fast, 10, 4, 500).IsSupported.Should().BeTrue();
        new EtsResult(EtsMode.Off, 0, 0, 0).IsSupported.Should().BeFalse();
    }

    [Fact]
    public void EffectiveIntervalNanoseconds_converts_from_picoseconds()
    {
        new EtsResult(EtsMode.Slow, 10, 4, 500).EffectiveIntervalNanoseconds.Should().Be(0.5);
    }
}
