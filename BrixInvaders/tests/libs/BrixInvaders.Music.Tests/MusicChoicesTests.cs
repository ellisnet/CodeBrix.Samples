using System;
using System.Linq;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration.MuPT;
using CodeBrix.Audio.MusicGeneration.SkyTNT;
using CodeBrix.Audio.Samples.FluidR3Gm;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Music.Tests;

public class MusicChoicesTests
{
    [Fact]
    public void GeneratorNames_are_the_exact_registry_names_in_settings_order() =>
        MusicChoices.GeneratorNames.Should().Equal(SkyTNTModel.GeneratorName, MuPTModel.GeneratorName);

    [Fact]
    public void InstrumentLibraryNames_are_the_exact_registry_names_in_settings_order() =>
        MusicChoices.InstrumentLibraryNames.Should().Equal(GeneralMidiInstrumentLibrary.LibraryName, FluidR3GmInstrumentLibrary.LibraryName);

    [Fact]
    public void registry_names_are_the_documented_strings()
    {
        //Assert
        MusicChoices.SkyTNT.Should().Be("SkyTNT");
        MusicChoices.MuPT.Should().Be("MuPT");
        MusicChoices.ModestSynthGm.Should().Be("ModestSynthGm");
        MusicChoices.FluidR3Gm.Should().Be("FluidR3Gm");
    }

    [Fact]
    public void defaults_are_SkyTNT_and_ModestSynthGm()
    {
        //Assert
        MusicChoices.DefaultGenerator.Should().Be("SkyTNT");
        MusicChoices.DefaultInstrumentLibrary.Should().Be("ModestSynthGm");
    }

    [Fact]
    public void every_choice_has_a_label_and_a_description()
    {
        //Arrange
        var choices = MusicChoices.Generators.Concat(MusicChoices.InstrumentLibraries).ToArray();

        //Assert
        choices.Should().HaveCount(4);
        choices.Should().OnlyContain(c => c.Label.Length > 0 && c.Description.Length > 0 && c.ToString() == c.Label);
        choices.Select(c => c.Label).Distinct().Should().HaveCount(4);
        MusicChoices.Generators.Select(c => c.Name).Should().Equal(MusicChoices.GeneratorNames);
        MusicChoices.InstrumentLibraries.Select(c => c.Name).Should().Equal(MusicChoices.InstrumentLibraryNames);
    }

    [Theory]
    [InlineData("SkyTNT", "SkyTNT")]
    [InlineData("skytnt", "SkyTNT")]
    [InlineData(" MUPT ", "MuPT")]
    [InlineData(null, "SkyTNT")]
    [InlineData("", "SkyTNT")]
    [InlineData("   ", "SkyTNT")]
    public void ResolveGenerator_returns_the_canonical_name(string name, string expected) =>
        MusicChoices.ResolveGenerator(name).Should().Be(expected);

    [Theory]
    [InlineData("ModestSynthGm", "ModestSynthGm")]
    [InlineData("fluidr3gm", "FluidR3Gm")]
    [InlineData(null, "ModestSynthGm")]
    [InlineData("", "ModestSynthGm")]
    public void ResolveInstrumentLibrary_returns_the_canonical_name(string name, string expected) =>
        MusicChoices.ResolveInstrumentLibrary(name).Should().Be(expected);

    [Fact]
    public void ResolveGenerator_unknown_name_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => MusicChoices.ResolveGenerator("MuseCoco");

        //Act
        var exception = act.Should().Throw<ArgumentException>().Which;

        //Assert
        exception.Message.Should().Contain("'MuseCoco'");
        exception.Message.Should().Contain("SkyTNT, MuPT");
    }

    [Fact]
    public void ResolveInstrumentLibrary_unknown_name_throws_with_the_valid_names()
    {
        //Arrange
        Action act = () => MusicChoices.ResolveInstrumentLibrary("GeneralUser");

        //Act
        var exception = act.Should().Throw<ArgumentException>().Which;

        //Assert
        exception.Message.Should().Contain("'GeneralUser'");
        exception.Message.Should().Contain("ModestSynthGm, FluidR3Gm");
    }

    [Theory]
    [InlineData("SkyTNT", true)]
    [InlineData("mupt", true)]
    [InlineData("ModestSynthGm", false)]
    [InlineData(null, false)]
    public void IsGenerator_matches_offered_generators_only(string name, bool expected) =>
        MusicChoices.IsGenerator(name).Should().Be(expected);

    [Theory]
    [InlineData("FluidR3Gm", true)]
    [InlineData("MODESTSYNTHGM", true)]
    [InlineData("SkyTNT", false)]
    [InlineData("", false)]
    public void IsInstrumentLibrary_matches_offered_libraries_only(string name, bool expected) =>
        MusicChoices.IsInstrumentLibrary(name).Should().Be(expected);

    [Fact]
    public void GeneratorChoice_and_InstrumentLibraryChoice_find_the_choice()
    {
        //Assert
        MusicChoices.GeneratorChoice("mupt").Name.Should().Be("MuPT");
        MusicChoices.GeneratorChoice(null).Name.Should().Be("SkyTNT");
        MusicChoices.InstrumentLibraryChoice("fluidr3gm").Name.Should().Be("FluidR3Gm");
    }

    [Theory]
    [InlineData("SkyTNT", 1, "MuPT")]
    [InlineData("MuPT", 1, "SkyTNT")]
    [InlineData("SkyTNT", -1, "MuPT")]
    [InlineData("MuPT", -1, "SkyTNT")]
    [InlineData("SkyTNT", 2, "SkyTNT")]
    [InlineData(null, 1, "MuPT")]
    public void NextGenerator_wraps_both_ways(string current, int direction, string expected) =>
        MusicChoices.NextGenerator(current, direction).Should().Be(expected);

    [Theory]
    [InlineData("ModestSynthGm", 1, "FluidR3Gm")]
    [InlineData("FluidR3Gm", 1, "ModestSynthGm")]
    [InlineData("ModestSynthGm", -1, "FluidR3Gm")]
    [InlineData("fluidr3gm", -3, "ModestSynthGm")]
    public void NextInstrumentLibrary_wraps_both_ways(string current, int direction, string expected) =>
        MusicChoices.NextInstrumentLibrary(current, direction).Should().Be(expected);

    [Fact]
    public void MusicChoice_blank_values_throw()
    {
        //Arrange
        Action noName = () => _ = new MusicChoice(" ", "label", "description");
        Action noLabel = () => _ = new MusicChoice("name", "", "description");
        Action noDescription = () => _ = new MusicChoice("name", "label", null);

        //Assert
        noName.Should().Throw<ArgumentException>();
        noLabel.Should().Throw<ArgumentException>();
        noDescription.Should().Throw<ArgumentException>();
    }
}
