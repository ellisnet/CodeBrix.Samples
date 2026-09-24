using System;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Rendering;
using BrixInvaders.Game.Screens;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Screens;

public class ScreenDirectorTests
{
    private static readonly string[] PackCredits = { "Space Shooter Remastered by Kenney (kenney.nl) - CC0" };

    private static (ScreenDirector Director, PaintContext Context, FrameBuilder Frame) Create(SessionDriver driver)
    {
        var frame = new FrameBuilder();
        var context = new PaintContext(driver.Session, frame, new PlayfieldPainter(), new KenneyCreditsContent(() => PackCredits), PackCredits);
        return (new ScreenDirector(), context, frame);
    }

    private static RenderFrame Paint(SessionDriver driver, InputDevice device = InputDevice.Keyboard)
    {
        var (director, context, frame) = Create(driver);
        context.Device = device;
        director.Follow(driver.Session.CurrentScreen, context);
        director.Update(0.5);
        frame.Clear();
        director.Paint(context);
        return frame.Build();
    }

    private static bool HasText(RenderFrame frame, string text) =>
        frame.Overlay.Any(command => command.Kind == DrawKind.Text && command.Text.Contains(text, StringComparison.Ordinal));

    [Fact]
    public void CoversEveryScreen_is_true() => new ScreenDirector().CoversEveryScreen().Should().BeTrue();

    [Fact]
    public void Follow_opens_the_painter_of_the_screen_on_show()
    {
        //Arrange
        var driver = new SessionDriver();
        var (director, context, _) = Create(driver);

        //Act
        director.Follow(GameScreen.Credits, context);

        //Assert
        director.Current.Should().Be(GameScreen.Credits);
    }

    [Fact]
    public void the_splash_draws_a_starfield_and_a_skip_prompt()
    {
        //Act
        var frame = Paint(new SessionDriver());

        //Assert
        frame.World.Should().NotBeEmpty();
        HasText(frame, "Skip").Should().BeTrue();
    }

    [Fact]
    public void the_title_draws_the_fleet_the_menu_and_the_Kenney_credit()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, TitleScreen.GameTitle).Should().BeTrue();
        TitleScreen.MenuItems.Should().OnlyContain(item => HasText(frame, item));
        HasText(frame, KenneyPacks.CreditLine).Should().BeTrue();
        frame.World.Count(command => command.Kind == DrawKind.Image && command.Image.Contains("#enemy", StringComparison.Ordinal))
            .Should().BeGreaterThan(10);
    }

    [Fact]
    public void prompts_follow_the_last_device()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();

        //Act
        var keyboard = Paint(driver, InputDevice.Keyboard);
        var gamepad = Paint(driver, InputDevice.Gamepad);

        //Assert
        HasText(keyboard, "[ENTER]").Should().BeTrue();
        HasText(gamepad, "[A]").Should().BeTrue();
        HasText(gamepad, "[ENTER]").Should().BeFalse();
    }

    [Fact]
    public void the_briefing_shows_the_sector_and_a_clickable_Kenney_bundle_card()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToBriefing();

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, SectorRules.NameOf(1).ToUpperInvariant()).Should().BeTrue();
        HasText(frame, KenneyCard.ZipLine).Should().BeTrue();
        HasText(frame, KenneyCard.BundleCaption).Should().BeTrue();
        frame.Overlay.Should().Contain(command => command.Image == SpriteCatalog.PromoCard);
        frame.Hotspots.Should().Contain(hotspot => hotspot.Url == KenneyPacks.BundleUrl);
    }

    [Fact]
    public void play_draws_the_player_the_formation_and_the_hud()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        driver.Wait(0.5);

        //Act
        var frame = Paint(driver);

        //Assert
        frame.World.Should().Contain(command => command.Image == SpriteCatalog.PlayerShip(0, 0));
        frame.World.Should().Contain(command => command.Image == SpriteCatalog.Enemy(EnemyRole.Grunt, EnemyColour.Black));
        HasText(frame, "SCORE").Should().BeTrue();
    }

    [Fact]
    public void the_pause_overlay_offers_resume_and_quit()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        driver.Press(SessionDriver.Pause);

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, "PAUSED").Should().BeTrue();
        HasText(frame, "RESUME").Should().BeTrue();
        HasText(frame, "QUIT TO TITLE").Should().BeTrue();
    }

    [Theory]
    [InlineData(1, "HIGH SCORES")]
    [InlineData(2, "SETTINGS")]
    public void the_title_menu_screens_draw_their_heading(int downs, string heading)
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        for (var i = 0; i < downs; i++)
        {
            driver.Press(SessionDriver.Down);
        }

        driver.Press(SessionDriver.Confirm);

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, heading).Should().BeTrue();
    }

    [Fact]
    public void the_credits_render_the_content_with_clickable_links()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        for (var i = 0; i < 3; i++)
        {
            driver.Press(SessionDriver.Down);
        }

        driver.Press(SessionDriver.Confirm);

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, PackCredits[0]).Should().BeTrue();
        frame.Hotspots.Select(hotspot => hotspot.Url).Should()
            .Contain(new[] { KenneyCreditsContent.PatreonUrl, KenneyCreditsContent.KenneySiteUrl, KenneyPacks.BundleUrl });
    }

    [Fact]
    public void ship_and_difficulty_select_draw_their_choices()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Press(SessionDriver.Confirm);

        //Act
        var ships = Paint(driver);
        driver.Press(SessionDriver.Confirm);
        var levels = Paint(driver);

        //Assert
        HasText(ships, "CHOOSE YOUR SHIP").Should().BeTrue();
        HasText(levels, "LEGEND").Should().BeTrue();
    }

    [Fact]
    public void game_over_and_name_entry_draw_the_score()
    {
        //Arrange
        var driver = new SessionDriver(settings => settings.Difficulty = Difficulty.Legend);
        driver.ToPlaying();
        driver.PlayUntil(session => session.CurrentScreen == GameScreen.GameOver, 600, new GameInput(0, true, false));

        //Act
        var over = Paint(driver);
        driver.Wait(ScreenStateMachine.GameOverAutoSeconds + SessionDriver.Step);
        var entry = Paint(driver);

        //Assert
        HasText(over, "GAME OVER").Should().BeTrue();
        HasText(entry, "NEW HIGH SCORE").Should().BeTrue();
    }

    [Fact]
    public void the_attract_demo_draws_the_demo_banner()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Wait(ScreenStateMachine.AttractIdleSeconds + 1);

        //Act
        var frame = Paint(driver);

        //Assert
        HasText(frame, "DEMO").Should().BeTrue();
    }

    [Fact]
    public void every_drawn_image_is_a_picture_the_game_loads()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        driver.Wait(4.0, new GameInput(0.3, true, false));
        var loadable = SpriteCatalog.AllAtlasFrames.Concat(SpriteCatalog.LoosePictures).Append(SpriteCatalog.PromoCard).ToHashSet();

        //Act
        var frame = Paint(driver);

        //Assert
        frame.World.Concat(frame.Overlay).Where(command => command.Kind == DrawKind.Image).Select(command => command.Image)
            .Should().OnlyContain(image => loadable.Contains(image));
    }
}
