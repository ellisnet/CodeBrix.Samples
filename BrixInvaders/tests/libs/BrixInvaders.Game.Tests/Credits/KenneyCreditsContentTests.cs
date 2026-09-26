using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Credits;
using CodeBrix.Platform.GameEngine.GeneratedMusic;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Credits;

public class KenneyCreditsContentTests
{
    [Fact]
    public void GetLines_credits_every_pack_and_links_to_Kenney()
    {
        //Arrange
        var content = new KenneyCreditsContent(() => new[] { "Planets (1.0) - Kenney (CC0)" });

        //Act
        var lines = content.GetLines();

        //Assert
        lines.Select(line => line.Text).Should().Contain(new[] { KenneyPacks.CreditLine, "Planets (1.0) - Kenney (CC0)" });
        lines.Where(line => line.Style == CreditsLineStyle.Link).Select(line => line.Url).Should()
            .Equal(KenneyCreditsContent.KenneySiteUrl, KenneyCreditsContent.PatreonUrl, KenneyPacks.BundleUrl);
    }

    [Fact]
    public void GetLines_adds_the_music_card_when_there_is_one()
    {
        //Arrange
        var content = new KenneyCreditsContent(() => new string[0], () => new[] { CreditsLine.Body("Music written live") });

        //Act
        var lines = content.GetLines();

        //Assert
        lines.Should().Contain(line => line.Style == CreditsLineStyle.Heading && line.Text == "MUSIC");
        lines.Last().Text.Should().Be("Music written live");
    }

    [Fact]
    public void GetLines_leaves_out_the_music_heading_without_music() =>
        new KenneyCreditsContent(() => new string[0]).GetLines().Should().NotContain(line => line.Text == "MUSIC");

    [Fact]
    public void the_credits_as_the_game_wires_them_carry_every_pack_title_the_Kenney_links_and_the_music_card()
    {
        //Arrange
        var titles = new[]
        {
            "Space Shooter Remastered (1.0)", "Space Shooter Extension (1.0)", "Planets (1.0)", "Sci-Fi Sounds (1.0)",
            "Digital Audio (1.0)",
        };
        var packs = titles.Select((title, i) => new KenneyPackSummary
        {
            Slug = KenneyPacks.Slugs[i],
            DisplayName = title,
            LicenseTitle = title,
            SourcePath = KenneyPacks.FileNames[i],
        }).ToList();
        var music = MusicCreditsCard.Lines(new GeneratedMusicSourceInfo("SkyTNT", "SkyTNT", "ModestSynthGm", false), null);
        var content = new KenneyCreditsContent(() => packs.Select(pack => pack.CreditLine), () => music);

        //Act
        var lines = content.GetLines();

        //Assert
        var texts = lines.Select(line => line.Text).ToList();
        foreach (var title in titles)
        {
            texts.Should().Contain($"{title} - Kenney (CC0)");
        }

        lines.Select(line => line.Url).Should().Contain(new[] { "https://kenney.nl", "https://www.patreon.com/kenney/" });
        texts.Should().Contain(text => text.Contains("kenney.nl") && text.Contains("CC0"));
        texts.Should().Contain("Music written live by SkyTNT through ModestSynthGm");
    }
}
