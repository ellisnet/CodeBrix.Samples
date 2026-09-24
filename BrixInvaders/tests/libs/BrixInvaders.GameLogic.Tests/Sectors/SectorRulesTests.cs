using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class SectorRulesTests
{
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(5, 5, 0)]
    [InlineData(6, 1, 1)]
    [InlineData(10, 5, 1)]
    [InlineData(11, 1, 2)]
    [InlineData(23, 3, 4)]
    public void DesignOf_and_LoopOf_cycle_every_five_sectors(int sector, int design, int loop)
    {
        //Act
        var actualDesign = SectorRules.DesignOf(sector);
        var actualLoop = SectorRules.LoopOf(sector);

        //Assert
        actualDesign.Should().Be(design);
        actualLoop.Should().Be(loop);
    }

    [Fact]
    public void DesignOf_rejects_sector_zero()
    {
        //Act
        Action act = () => SectorRules.DesignOf(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, SectorFeatures.Ufo)]
    [InlineData(2, SectorFeatures.Divers)]
    [InlineData(3, SectorFeatures.Shielded | SectorFeatures.MeteorShowers)]
    [InlineData(4, SectorFeatures.HomingMissiles)]
    [InlineData(5, SectorFeatures.SplitFormation)]
    public void IntroducedBy_names_each_designs_new_behaviour(int design, SectorFeatures expected) => SectorRules.IntroducedBy(design).Should().Be(expected);

    [Fact]
    public void FeaturesOf_is_cumulative()
    {
        //Act
        var sector2 = SectorRules.FeaturesOf(2);
        var sector5 = SectorRules.FeaturesOf(5);
        var sector6 = SectorRules.FeaturesOf(6);

        //Assert
        sector2.Should().Be(SectorFeatures.Ufo | SectorFeatures.Divers);
        sector5.Should().Be(SectorFeatures.Ufo | SectorFeatures.Divers | SectorFeatures.Shielded | SectorFeatures.MeteorShowers
            | SectorFeatures.HomingMissiles | SectorFeatures.SplitFormation);
        sector6.Should().Be(SectorFeatures.Ufo);
    }

    [Theory]
    [InlineData(1, "Outer Picket")]
    [InlineData(5, "Mothership Gate")]
    [InlineData(6, "Outer Picket II")]
    [InlineData(12, "Raider Lanes III")]
    public void NameOf_adds_a_roman_numeral_on_loops(int sector, string name) => SectorRules.NameOf(sector).Should().Be(name);

    [Fact]
    public void BriefingOf_mentions_the_loop_after_sector_five()
    {
        //Act
        var first = SectorRules.BriefingOf(1);
        var looped = SectorRules.BriefingOf(6);

        //Assert
        first.Should().Contain("UFO");
        looped.Should().StartWith(first);
        looped.Should().Contain("faster");
    }

    [Theory]
    [InlineData(1, 1.0, 1.0, 1.0, 0)]
    [InlineData(6, 0.85, 1.1, 1.5, 2)]
    [InlineData(11, 0.7225, 1.2, 2.0, 4)]
    public void scaling_tightens_on_every_loop(int sector, double interval, double speed, double bossHealth, int extraBolts)
    {
        //Act
        var actualInterval = SectorRules.IntervalScale(sector);
        var actualSpeed = SectorRules.SpeedScale(sector);
        var actualBoss = SectorRules.BossHealthScale(sector);
        var actualBolts = SectorRules.ExtraEnemyBolts(sector);

        //Assert
        actualInterval.Should().BeApproximately(interval, 1e-9);
        actualSpeed.Should().BeApproximately(speed, 1e-9);
        actualBoss.Should().BeApproximately(bossHealth, 1e-9);
        actualBolts.Should().Be(extraBolts);
    }

    [Theory]
    [InlineData(2, "II")]
    [InlineData(4, "IV")]
    [InlineData(9, "IX")]
    [InlineData(14, "XIV")]
    public void RomanNumeral_converts(int value, string expected) => SectorRules.RomanNumeral(value).Should().Be(expected);
}
