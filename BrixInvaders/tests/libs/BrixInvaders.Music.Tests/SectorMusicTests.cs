using System;
using System.Linq;
using CodeBrix.Audio.MusicGeneration.Presets;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Music.Tests;

public class SectorMusicTests
{
    [Fact]
    public void Designs_cover_sectors_one_to_five_in_order()
    {
        //Assert
        SectorMusic.Designs.Should().HaveCount(SectorMusic.DesignCount);
        SectorMusic.Designs.Select(d => d.Design).Should().Equal(1, 2, 3, 4, 5);
        SectorMusic.Designs.Select(d => d.Name).Should()
            .Equal("Outer Picket", "Raider Lanes", "Aegis Belt", "Missile Reach", "Mothership Gate");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void For_sectors_one_to_five_are_complete(int sector)
    {
        //Act
        var entry = SectorMusic.For(sector);

        //Assert
        entry.Design.Should().Be(sector);
        entry.Name.Should().NotBeEmpty();
        entry.Mood.Should().NotBeEmpty();
        entry.SkyTNTPreset.Should().NotBeEmpty();
        entry.MuPTPreset.Should().NotBeEmpty();
        entry.BossSkyTNTPreset.Should().NotBe(entry.SkyTNTPreset);
        entry.BossMuPTPreset.Should().NotBe(entry.MuPTPreset);
        entry.BossBeatsPerMinute.Should().BeGreaterThan(entry.BeatsPerMinute);
    }

    [Theory]
    [InlineData(6, 1)]
    [InlineData(7, 2)]
    [InlineData(8, 3)]
    [InlineData(9, 4)]
    [InlineData(10, 5)]
    [InlineData(11, 1)]
    [InlineData(12, 2)]
    [InlineData(1000, 5)]
    public void For_later_sectors_wrap_to_the_five_designs(int sector, int design)
    {
        //Assert
        SectorMusic.DesignFor(sector).Should().Be(design);
        SectorMusic.For(sector).Should().BeSameAs(SectorMusic.For(design));
    }

    [Fact]
    public void For_sector_zero_is_the_title()
    {
        //Assert
        SectorMusic.DesignFor(SectorMusic.TitleSector).Should().Be(0);
        SectorMusic.For(0).Should().BeSameAs(SectorMusic.Title);
        SectorMusic.Title.Name.Should().Be("Title");
        SectorMusic.Title.Design.Should().Be(0);
    }

    [Fact]
    public void For_negative_sector_throws()
    {
        //Arrange
        Action act = () => SectorMusic.For(-1);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DesignFor_negative_sector_throws()
    {
        //Arrange
        Action act = () => SectorMusic.DesignFor(-5);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void every_SkyTNT_preset_in_the_table_is_a_SkyTNT_preset()
    {
        //Arrange
        var names = SkyTNTPresets.All.Select(p => p.Name).ToArray();
        var entries = SectorMusic.Designs.Append(SectorMusic.Title).ToArray();

        //Assert
        entries.Should().OnlyContain(e => names.Contains(e.SkyTNTPreset) && names.Contains(e.BossSkyTNTPreset));
    }

    [Fact]
    public void every_MuPT_preset_in_the_table_is_a_MuPT_preset()
    {
        //Arrange
        var names = MuPTPresets.All.Select(p => p.Name).ToArray();
        var entries = SectorMusic.Designs.Append(SectorMusic.Title).ToArray();

        //Assert
        entries.Should().OnlyContain(e => names.Contains(e.MuPTPreset) && names.Contains(e.BossMuPTPreset));
    }

    [Fact]
    public void every_tempo_is_in_a_sane_range()
    {
        //Arrange
        var entries = SectorMusic.Designs.Append(SectorMusic.Title).ToArray();

        //Assert
        entries.Should().OnlyContain(e =>
            e.BeatsPerMinute >= 80 && e.BeatsPerMinute <= 160 &&
            e.BossBeatsPerMinute >= 80 && e.BossBeatsPerMinute <= 160);
    }

    [Fact]
    public void neighbouring_sectors_sound_different()
    {
        //Arrange
        var designs = SectorMusic.Designs;

        //Assert
        Enumerable.Range(0, designs.Count - 1).Should().OnlyContain(i =>
            designs[i].SkyTNTPreset != designs[i + 1].SkyTNTPreset && designs[i].MuPTPreset != designs[i + 1].MuPTPreset);
    }
}
