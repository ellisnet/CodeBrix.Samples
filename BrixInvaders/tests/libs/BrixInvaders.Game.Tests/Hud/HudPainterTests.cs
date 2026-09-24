using System;
using System.Linq;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Hud;

public class HudPainterTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(48210, "48,210")]
    [InlineData(1234567, "1,234,567")]
    public void FormatScore_groups_thousands(long score, string expected) => HudPainter.FormatScore(score).Should().Be(expected);

    [Fact]
    public void FormatChain_shows_count_and_multiplier() => HudPainter.FormatChain(23, 3).Should().Be("CHAIN 23  x3");

    [Fact]
    public void every_power_up_has_a_hud_name() =>
        Enum.GetValues<PowerUpKind>().Select(HudPainter.PowerUpName).Should().OnlyContain(name => name.Length > 0);

    [Fact]
    public void BannerFor_announces_the_first_wave()
    {
        //Arrange
        var game = new GameSimulation(new GameSetup(Difficulty.Pilot));

        //Act
        var banner = HudPainter.BannerFor(game);

        //Assert
        game.Phase.Should().Be(StagePhase.WaveIntro);
        banner.Should().Be("WAVE 1");
    }

    [Fact]
    public void BannerFor_is_quiet_while_a_wave_is_active()
    {
        //Arrange
        var game = new GameSimulation(new GameSetup(Difficulty.Pilot));
        for (var i = 0; i < 200; i++)
        {
            game.Step(Playfield.FixedStep, GameInput.None);
        }

        //Act
        var banner = HudPainter.BannerFor(game);

        //Assert
        game.Phase.Should().Be(StagePhase.WaveActive);
        banner.Should().BeNull();
    }

    [Fact]
    public void Paint_draws_the_score_lives_and_sector_in_the_overlay()
    {
        //Arrange
        var game = new GameSimulation(new GameSetup(Difficulty.Cadet, shipShape: 1, shipColour: 2));
        var frame = new FrameBuilder();

        //Act
        HudPainter.Paint(frame, game, 0);

        //Assert
        frame.World.Should().BeEmpty();
        frame.Overlay.Should().Contain(command => command.Kind == DrawKind.Text && command.Text == "SCORE");
        frame.Overlay.Count(command => command.Image == SpriteCatalog.LifeIcon(1, 2)).Should().Be(game.Player.Lives);
        frame.Overlay.Should().Contain(command => command.Kind == DrawKind.Text && command.Text.StartsWith("SECTOR 1", StringComparison.Ordinal));
    }
}
