using System;
using System.IO;
using SilverAssertions;
using Xunit;

namespace GameEngineMusicDemo.Game.Tests;

public class MusicAssetFactoryTests
{
    [Fact]
    public void AssetDirectory_is_a_folder_beside_the_executable()
    {
        //Arrange
        var directory = Path.TrimEndingDirectorySeparator(MusicAssetFactory.AssetDirectory);

        //Act
        var parent = Path.GetDirectoryName(directory);

        //Assert
        Path.GetFileName(directory).Should().Be("GeneratedMusic");
        parent.Should().Be(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
    }

    [Fact]
    public void StemNames_and_StemPaths_describe_the_same_layers()
    {
        //Arrange
        var names = MusicAssetFactory.StemNames;
        var paths = MusicAssetFactory.StemPaths;

        //Act
        var namesMatchPaths = true;
        for (var i = 0; i < names.Length; i++)
        {
            namesMatchPaths = namesMatchPaths
                && Path.GetFileName(paths[i]) == $"stem-{names[i]}.wav";
        }

        //Assert
        names.Length.Should().Be(3);
        paths.Length.Should().Be(3);
        namesMatchPaths.Should().Be(true);
    }

    [Fact]
    public void The_stems_export_is_written_at_a_rate_the_device_is_not_pinned_to()
    {
        //Arrange
        var deviceRate = MusicAssetFactory.SampleRate;

        //Act
        var exportRate = MusicAssetFactory.StemsExportSampleRate;

        //Assert
        deviceRate.Should().Be(44100);
        exportRate.Should().Be(48000);
        (exportRate == deviceRate).Should().Be(false);
    }

    [Fact]
    public void The_Decent_Sampler_preset_sits_in_its_own_instrument_folder()
    {
        //Arrange
        var preset = MusicAssetFactory.DecentSamplerPresetPath;

        //Act
        var folder = Path.GetDirectoryName(preset);

        //Assert
        Path.GetExtension(preset).Should().Be(".dspreset");
        Path.GetFileName(folder).Should().Be("DemoSampler");
        Path.GetFullPath(Path.Combine(folder, "..")).Should()
            .Be(Path.GetFullPath(MusicAssetFactory.AssetDirectory));
    }

    [Fact]
    public void The_tempo_change_file_slows_down_after_its_opening_bars()
    {
        //Arrange
        var opening = MusicAssetFactory.BeatsPerMinute;

        //Act
        var later = MusicAssetFactory.SlowerBeatsPerMinute;

        //Assert
        opening.Should().Be(120d);
        later.Should().Be(90d);
        (MusicAssetFactory.BarsBeforeTempoChange > 0).Should().Be(true);
    }
}
