using System.Linq;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Credits;
using BrixInvaders.Music;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Credits;

public class MusicCreditsCardTests
{
    [Fact]
    public void Lines_name_the_playing_model_and_instrument_library()
    {
        //Arrange
        var source = new MusicSourceInfo("SkyTNT", "ModestSynthGm", false);

        //Act
        var lines = MusicCreditsCard.Lines(source, null);

        //Assert
        lines[0].Text.Should().Be("Music written live by SkyTNT through ModestSynthGm");
        lines.Should().OnlyContain(line => line.Style == CreditsLineStyle.Body);
    }

    [Fact]
    public void Lines_name_both_package_families_in_words_without_versions()
    {
        //Arrange
        var source = new MusicSourceInfo("MuPT", "FluidR3Gm", false);

        //Act
        var texts = MusicCreditsCard.Lines(source, null).Select(line => line.Text).ToList();

        //Assert
        texts.Should().Contain(text => text.Contains("CodeBrix.Audio.MusicGeneration"));
        texts.Should().Contain(text => text.Contains("CodeBrix.Platform.GameEngine"));
        texts.Skip(1).Should().NotContain(text => text.Any(char.IsDigit), "the family lines carry no versions");
    }

    [Fact]
    public void Lines_before_the_music_plays_name_the_players_choices()
    {
        //Arrange
        var chosen = new MusicSettings { GeneratorName = "mupt", InstrumentLibraryName = "fluidr3gm" };

        //Act
        var first = MusicCreditsCard.Lines(null, chosen)[0].Text;

        //Assert
        first.Should().StartWith("Music written live by MuPT through FluidR3Gm");
        first.Should().Contain("warming up");
    }

    [Fact]
    public void Lines_for_the_embedded_replay_do_not_claim_a_model() =>
        MusicCreditsCard.Lines(new MusicSourceInfo("Replay", "ModestSynthGm", true), null)[0].Text.Should()
            .NotContain("written live");

    [Fact]
    public void WrittenLiveLine_is_the_music_card_format() =>
        MusicCreditsCard.WrittenLiveLine("SkyTNT", "FluidR3Gm").Should().Be("Music written live by SkyTNT through FluidR3Gm");
}
