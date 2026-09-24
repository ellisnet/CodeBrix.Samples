using System;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Music.Tests;

public class SectorMusicEntryTests
{
    [Theory]
    [InlineData("SkyTNT", false, "ClubArrangement")]
    [InlineData("SkyTNT", true, "FourOnTheFloor")]
    [InlineData("mupt", false, "JigInD")]
    [InlineData("MuPT", true, "ReelInGMinor")]
    [InlineData(null, false, "ClubArrangement")]
    public void PresetFor_picks_the_generator_and_boss_preset(string generator, bool boss, string expected) =>
        SectorMusic.For(2).PresetFor(generator, boss).Should().Be(expected);

    [Fact]
    public void PresetFor_unknown_generator_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => SectorMusic.For(1).PresetFor("Magenta");

        //Assert
        act.Should().Throw<ArgumentException>().Which.Message.Should().Contain("SkyTNT, MuPT");
    }

    [Fact]
    public void BeatsPerMinuteFor_picks_the_sector_or_boss_tempo()
    {
        //Arrange
        var entry = SectorMusic.For(1);

        //Assert
        entry.BeatsPerMinuteFor().Should().Be(entry.BeatsPerMinute);
        entry.BeatsPerMinuteFor(boss: true).Should().Be(entry.BossBeatsPerMinute);
    }

    [Fact]
    public void constructor_canonicalises_preset_names()
    {
        //Act
        var entry = new SectorMusicEntry(1, "Test", "mood", "clubarrangement", "jigind", 120, "FOURONTHEFLOOR", "reelingminor", 128);

        //Assert
        entry.SkyTNTPreset.Should().Be("ClubArrangement");
        entry.MuPTPreset.Should().Be("JigInD");
        entry.BossSkyTNTPreset.Should().Be("FourOnTheFloor");
        entry.BossMuPTPreset.Should().Be("ReelInGMinor");
    }

    [Fact]
    public void constructor_unknown_SkyTNT_preset_throws_with_the_SkyTNT_presets()
    {
        //Arrange
        Action act = () => _ = new SectorMusicEntry(1, "Test", "mood", "JigInD", "JigInD", 120, "ClubArrangement", "ReelInGMinor", 128);

        //Act
        var exception = act.Should().Throw<ArgumentException>().Which;

        //Assert
        exception.Message.Should().Contain("'JigInD' is not a SkyTNT preset");
        exception.Message.Should().Contain("FourOnTheFloor");
        exception.Message.Should().Contain("ClubArrangement");
        exception.Message.Should().Contain("AmbientElectronica");
    }

    [Fact]
    public void constructor_unknown_MuPT_preset_throws_with_the_MuPT_presets()
    {
        //Arrange
        Action act = () => _ = new SectorMusicEntry(1, "Test", "mood", "ClubArrangement", "Polka", 120, "ClubArrangement", "ReelInGMinor", 128);

        //Act
        var exception = act.Should().Throw<ArgumentException>().Which;

        //Assert
        exception.Message.Should().Contain("'Polka' is not a MuPT preset");
        exception.Message.Should().Contain("ReelInGMinor");
        exception.Message.Should().Contain("WaltzDuetInAMinor");
    }

    [Theory]
    [InlineData(59)]
    [InlineData(181)]
    [InlineData(double.NaN)]
    public void constructor_tempo_out_of_range_throws(double beatsPerMinute)
    {
        //Arrange
        Action act = () => _ = new SectorMusicEntry(1, "Test", "mood", "ClubArrangement", "JigInD", beatsPerMinute, "ClubArrangement", "ReelInGMinor", 128);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToString_names_the_design_presets_and_tempos() =>
        SectorMusic.For(3).ToString().Should()
            .Be("3 Aegis Belt: SkyTNT AmbientElectronica / MuPT AirInDMixolydian at 104 bpm (boss SkyTNT ClubArrangement / MuPT ReelInGMinor at 112 bpm)");
}
