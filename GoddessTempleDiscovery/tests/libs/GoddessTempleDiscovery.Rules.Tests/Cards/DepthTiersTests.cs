using System;
using GoddessTempleDiscovery.Rules.Cards;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Cards;

public class DepthTiersTests
{
    [Fact]
    public void every_tier_has_a_short_capitalized_name()
    {
        for (var tier = DepthTiers.Shallowest; tier <= DepthTiers.Deepest; tier++)
        {
            var name = DepthTiers.ShortName(tier);
            name.Should().NotBeNullOrWhiteSpace();
            name.Should().Be(name.ToUpperInvariant());
            name.Length.Should().BeLessThanOrEqualTo(DepthTiers.Name(tier).Length);
            name.Length.Should().BeLessThanOrEqualTo(20);
        }

        DepthTiers.ShortName(3).Should().Be("KASSITE / OLD BAB.");
        DepthTiers.ShortName(9).Should().Be("DEEP SOUNDING");
    }

    [Fact]
    public void a_short_name_outside_the_tiers_is_refused()
    {
        //Act
        Action act = () => DepthTiers.ShortName(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
