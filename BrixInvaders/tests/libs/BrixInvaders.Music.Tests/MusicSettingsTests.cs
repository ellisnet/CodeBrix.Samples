using SilverAssertions;
using Xunit;

namespace BrixInvaders.Music.Tests;

public class MusicSettingsTests
{
    [Fact]
    public void defaults_are_the_default_choices_at_full_level()
    {
        //Act
        var settings = new MusicSettings();

        //Assert
        settings.GeneratorName.Should().Be(MusicChoices.DefaultGenerator);
        settings.InstrumentLibraryName.Should().Be(MusicChoices.DefaultInstrumentLibrary);
        settings.MusicVolume.Should().Be(1.0);
    }

    [Fact]
    public void Clone_copies_every_field_independently()
    {
        //Arrange
        var settings = new MusicSettings { GeneratorName = "MuPT", InstrumentLibraryName = "FluidR3Gm", MusicVolume = 0.4 };

        //Act
        var copy = settings.Clone();
        settings.GeneratorName = "SkyTNT";

        //Assert
        copy.Should().NotBeSameAs(settings);
        copy.GeneratorName.Should().Be("MuPT");
        copy.InstrumentLibraryName.Should().Be("FluidR3Gm");
        copy.MusicVolume.Should().Be(0.4);
    }
}
