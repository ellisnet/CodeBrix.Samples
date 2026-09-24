using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.MusicGeneration;
using CodeBrix.Platform.GameEngine.GeneratedMusic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Music.Tests;

public class MusicSetupTests
{
    private static readonly List<string> _ignoredLog = [];

    [Fact]
    public void RegisterEverything_is_idempotent()
    {
        //Arrange
        MusicSetup.RegisterEverything(_ignoredLog.Add);
        var libraries = InstrumentLibraryRegistry.RegisteredNames.ToArray();
        var generators = MusicGeneratorRegistry.RegisteredNames.ToArray();
        var log = new List<string>();

        //Act
        var registeredAgain = MusicSetup.RegisterEverything(log.Add);

        //Assert
        registeredAgain.Should().BeFalse();
        MusicSetup.IsRegistered.Should().BeTrue();
        log.Should().BeEmpty();
        InstrumentLibraryRegistry.RegisteredNames.Should().Equal(libraries);
        MusicGeneratorRegistry.RegisteredNames.Should().Equal(generators);
    }

    [Fact]
    public void RegisterEverything_registries_hold_the_four_names_with_ModestSynthGm_the_default()
    {
        //Act
        MusicSetup.RegisterEverything(_ignoredLog.Add);
        var libraries = InstrumentLibraryRegistry.RegisteredNames.ToList();

        //Assert
        InstrumentLibraryRegistry.IsRegistered("ModestSynthGm").Should().BeTrue();
        InstrumentLibraryRegistry.IsRegistered("FluidR3Gm").Should().BeTrue();
        MusicGeneratorRegistry.IsRegistered("SkyTNT").Should().BeTrue();
        MusicGeneratorRegistry.IsRegistered("MuPT").Should().BeTrue();
        InstrumentLibraryRegistry.DefaultName.Should().Be("ModestSynthGm");
        libraries.IndexOf("ModestSynthGm").Should().BeLessThan(libraries.IndexOf("FluidR3Gm"));
    }

    [Fact]
    public void RegisterEverything_first_call_logs_one_line_naming_what_was_registered()
    {
        //Arrange
        MusicSetup.RegisterEverything(_ignoredLog.Add);
        MusicSetup.ResetForTesting();
        var log = new List<string>();

        //Act
        var registered = MusicSetup.RegisterEverything(log.Add);

        //Assert
        registered.Should().BeTrue();
        log.Should().ContainSingle();
        log[0].Should().Be(MusicSetup.DescribeRegistration());
    }

    [Fact]
    public void DescribeRegistration_names_libraries_generators_and_model_files()
    {
        //Arrange
        MusicSetup.RegisterEverything(_ignoredLog.Add);

        //Act
        var line = MusicSetup.DescribeRegistration();

        //Assert
        line.Should().StartWith("[BrixInvaders] music: ");
        line.Should().Contain("instrument libraries ModestSynthGm (default), FluidR3Gm;");
        line.Should().Contain("generators SkyTNT, MuPT;");
        line.Should().Contain("model files: SkyTNT ");
        line.Should().Contain(", MuPT ");
    }

    [Fact]
    public void NoModelFallbackNote_names_the_replay_and_the_fix()
    {
        //Assert
        MusicSetup.NoModelFallbackNote.Should().Contain("embedded replay");
        MusicSetup.NoModelFallbackNote.Should().Contain("SkyTNTModel.Register()");
        MusicSetup.NoModelFallbackNote.Should().Contain("MuPTModel.Register()");
    }

    [Fact]
    public void constants_are_the_documented_values()
    {
        //Assert
        MusicSetup.LogPrefix.Should().Be("[BrixInvaders] music:");
        MusicSetup.TrackKey.Should().Be("brixinvaders-music");
        MusicSetup.SeamCrossfade.Should().Be(TimeSpan.FromSeconds(4));
        MusicSetup.RecommendedSampleRate.Should().Be(48000);
        MusicSetup.RecommendedChannels.Should().Be(2);
    }

    [Theory]
    [InlineData("SkyTNT", "ModestSynthGm")]
    [InlineData("SkyTNT", "FluidR3Gm")]
    [InlineData("MuPT", "ModestSynthGm")]
    [InlineData("MuPT", "FluidR3Gm")]
    public void OptionsFor_maps_every_field_for_each_generator_and_library(string generator, string library)
    {
        //Arrange
        var settings = new MusicSettings { GeneratorName = generator, InstrumentLibraryName = library, MusicVolume = 0.75 };
        var defaults = new GeneratedMusicOptions();

        foreach (var sector in Enumerable.Range(0, 13))
        {
            foreach (var boss in sector == 0 ? new[] { false } : new[] { false, true })
            {
                //Act
                var options = MusicSetup.OptionsFor(settings, sector, boss);
                var entry = SectorMusic.For(sector);

                //Assert
                options.Generator.Should().Be(generator);
                options.InstrumentLibrary.Should().Be(library);
                options.Preset.Should().Be(entry.PresetFor(generator, boss));
                options.BeatsPerMinute.Should().Be(entry.BeatsPerMinuteFor(boss));
                options.SeamCrossfade.Should().Be(TimeSpan.FromSeconds(4));
                options.MasterVolume.Should().Be(0.75f);
                options.TrackKey.Should().Be("brixinvaders-music");
                options.StartImmediately.Should().BeTrue();
                options.Text.Should().BeNull();
                options.Seed.Should().BeNull();
                options.SegmentPriming.Should().BeNull();
                options.FadeIn.Should().Be(defaults.FadeIn);
            }
        }
    }

    [Theory]
    [InlineData("SkyTNT", "ModestSynthGm")]
    [InlineData("SkyTNT", "FluidR3Gm")]
    [InlineData("MuPT", "ModestSynthGm")]
    [InlineData("MuPT", "FluidR3Gm")]
    public void OptionsFor_every_option_set_is_accepted_by_the_generated_music_provider(string generator, string library)
    {
        //Arrange
        var settings = new MusicSettings { GeneratorName = generator, InstrumentLibraryName = library };
        var sectors = Enumerable.Range(0, 6).ToArray();

        foreach (var sector in sectors)
        {
            foreach (var boss in sector == 0 ? new[] { false } : new[] { false, true })
            {
                //Act
                //Constructing a provider checks the preset name; it is never started
                using var provider = new GeneratedMusicProvider(MusicSetup.OptionsFor(settings, sector, boss));

                //Assert
                provider.Options.Preset.Should().Be(SectorMusic.For(sector).PresetFor(generator, boss));
            }
        }
    }

    [Fact]
    public void OptionsFor_canonicalises_names_and_defaults_blank_ones()
    {
        //Arrange
        var settings = new MusicSettings { GeneratorName = "mupt", InstrumentLibraryName = null };

        //Act
        var options = MusicSetup.OptionsFor(settings, 1);

        //Assert
        options.Generator.Should().Be("MuPT");
        options.InstrumentLibrary.Should().Be("ModestSynthGm");
        options.Preset.Should().Be("HornpipeInG");
    }

    [Fact]
    public void OptionsFor_title_uses_the_title_entry() =>
        MusicSetup.OptionsFor(new MusicSettings(), SectorMusic.TitleSector).Preset.Should().Be(SectorMusic.Title.SkyTNTPreset);

    [Fact]
    public void OptionsFor_unknown_generator_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(new MusicSettings { GeneratorName = "Orchestra" }, 1);

        //Assert
        act.Should().Throw<ArgumentException>().Which.Message.Should().Contain("Valid names: SkyTNT, MuPT.");
    }

    [Fact]
    public void OptionsFor_unknown_instrument_library_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(new MusicSettings { InstrumentLibraryName = "Piano" }, 1);

        //Assert
        act.Should().Throw<ArgumentException>().Which.Message.Should().Contain("Valid names: ModestSynthGm, FluidR3Gm.");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void OptionsFor_music_volume_out_of_range_throws(double volume)
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(new MusicSettings { MusicVolume = volume }, 1);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void OptionsFor_null_settings_throws()
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(null, 1);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OptionsFor_negative_sector_throws()
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(new MusicSettings(), -1);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void OptionsFor_boss_on_the_title_throws()
    {
        //Arrange
        Action act = () => MusicSetup.OptionsFor(new MusicSettings(), SectorMusic.TitleSector, boss: true);

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("SkyTNT", 1, false, "FourOnTheFloor")]
    [InlineData("SkyTNT", 1, true, "ClubArrangement")]
    [InlineData("MuPT", 3, false, "AirInDMixolydian")]
    [InlineData("MuPT", 8, true, "ReelInGMinor")]
    [InlineData("SkyTNT", 0, false, "AmbientElectronica")]
    public void FollowUpFor_returns_the_playing_generators_preset(string generator, int sector, bool boss, string expected)
    {
        //Arrange
        var settings = new MusicSettings { GeneratorName = generator };

        //Assert
        MusicSetup.FollowUpFor(settings, sector, boss).Should().Be(expected);
        MusicSetup.FollowUpFor(generator, sector, boss).Should().Be(expected);
    }

    [Fact]
    public void FollowUpFor_unknown_generator_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => MusicSetup.FollowUpFor("Choir", 1);

        //Assert
        act.Should().Throw<ArgumentException>().Which.Message.Should().Contain("SkyTNT, MuPT");
    }

    [Fact]
    public void FollowUpFor_null_settings_throws()
    {
        //Arrange
        Action act = () => MusicSetup.FollowUpFor((MusicSettings)null, 1);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
