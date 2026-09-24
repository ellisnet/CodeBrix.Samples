using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Session;
using BrixInvaders.Game.Settings;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Session;

//The host-independent game flow: GameSession driving the screen machine, the simulations, the settings, the
//  music seam and the link seam, with recording fakes instead of the engine.
public class ScreenFlowTests
{
    [Fact]
    public void the_session_starts_on_the_splash_and_the_splash_leads_to_the_title()
    {
        //Arrange
        var driver = new SessionDriver();
        var start = driver.Session.CurrentScreen;

        //Act
        driver.Session.NotifySplashComplete();

        //Assert
        start.Should().Be(GameScreen.Splash);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
        driver.Music.Calls.Should().BeEmpty("the title music is already playing from start-up");
    }

    [Fact]
    public void every_screen_change_is_logged_and_raised()
    {
        //Arrange
        var driver = new SessionDriver();
        var lines = new List<string>();
        var changes = new List<(GameScreen, GameScreen)>();
        driver.Session.ScreenChanged += (from, to) => changes.Add((from, to));
        GameLog.Sink = lines.Add;
        try
        {
            //Act
            driver.ToTitle();
            driver.Press(SessionDriver.Confirm);
        }
        finally
        {
            GameLog.Sink = null;
        }

        //Assert
        lines.Should().Contain("[BrixInvaders] screen: Splash -> Title");
        lines.Should().Contain("[BrixInvaders] screen: Title -> ShipSelect");
        changes.Should().Equal((GameScreen.Splash, GameScreen.Title), (GameScreen.Title, GameScreen.ShipSelect));
    }

    [Fact]
    public void play_goes_through_ship_and_difficulty_to_the_briefing_with_a_new_game()
    {
        //Arrange
        var driver = new SessionDriver(settings => settings.Difficulty = Difficulty.Ace);

        //Act
        driver.ToBriefing();

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.SectorBriefing);
        driver.Session.Game.Should().NotBeNull();
        driver.Session.Game.Setup.Difficulty.Should().Be(Difficulty.Ace);
        driver.Session.Game.Sector.Should().Be(1);
        driver.Music.Calls.Should().Equal("OnSector(1)");
    }

    [Fact]
    public void the_briefing_launches_play_only_after_a_second()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToBriefing();

        //Act
        driver.Press(SessionDriver.Confirm);
        var early = driver.Session.CurrentScreen;
        driver.Wait(ScreenStateMachine.BriefingMinSeconds);
        driver.Press(SessionDriver.Confirm);

        //Assert
        early.Should().Be(GameScreen.SectorBriefing);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Playing);
    }

    [Fact]
    public void playing_steps_the_game_and_collects_its_events_and_sounds()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        var stepsBefore = driver.Session.Game.StepCount;

        //Act
        driver.Wait(3.0, new GameInput(0, true, false));

        //Assert
        driver.Session.Game.StepCount.Should().BeGreaterThan(stepsBefore + 170);
        driver.Session.Events.Should().Contain(gameEvent => gameEvent.Kind == GameEventKind.PlayerFired);
        driver.Sound.Played.Should().Contain(cue => cue.Effect == SoundEffect.PlayerLaser);
    }

    [Fact]
    public void ClearEvents_forgets_what_the_host_has_drawn()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        driver.Wait(1.0, new GameInput(0, true, false));

        //Act
        driver.Session.ClearEvents();

        //Assert
        driver.Session.Events.Should().BeEmpty();
    }

    [Fact]
    public void pause_freezes_the_game_and_tells_the_music()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();

        //Act
        driver.Press(SessionDriver.Pause);
        var frozenAt = driver.Session.Game.StepCount;
        driver.Wait(1.0);
        var paused = driver.Session.CurrentScreen;
        var frozenAfter = driver.Session.Game.StepCount;
        driver.Press(SessionDriver.Pause);

        //Assert
        paused.Should().Be(GameScreen.Paused);
        frozenAfter.Should().Be(frozenAt);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Playing);
        driver.Music.Calls.Should().Equal("OnSector(1)", "OnPause", "OnResume");
    }

    [Fact]
    public void quitting_to_the_title_drops_the_game_and_brings_the_title_music_back()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToPlaying();
        driver.Press(SessionDriver.Pause);

        //Act
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
        driver.Session.Game.Should().BeNull();
        driver.Music.Calls.Last().Should().Be("OnTitle");
    }

    [Fact]
    public void a_lost_game_reaches_game_over_and_a_high_score_is_saved()
    {
        //Arrange
        var driver = new SessionDriver(settings =>
        {
            settings.Difficulty = Difficulty.Legend;
            settings.LastName = "ZED";
        });
        driver.ToPlaying();
        var lines = new List<string>();
        GameLog.Sink = lines.Add;
        try
        {
            //Act
            var over = driver.PlayUntil(session => session.CurrentScreen == GameScreen.GameOver, 600, new GameInput(0, true, false));
            var score = driver.Session.Game.Score;
            driver.Wait(ScreenStateMachine.GameOverMinSeconds);
            driver.Press(SessionDriver.Confirm);
            var entry = driver.Session.CurrentScreen;
            driver.Press(SessionDriver.Confirm);
            driver.Press(SessionDriver.Confirm);
            driver.Press(SessionDriver.Confirm);

            //Assert
            over.Should().BeTrue();
            score.Should().BeGreaterThan(0);
            entry.Should().Be(GameScreen.HighScoreEntry);
            driver.Session.CurrentScreen.Should().Be(GameScreen.HighScores);
            driver.Session.HighScores.EntriesFor(Difficulty.Legend)[0].Name.Should().Be("ZED");
            driver.Session.LastHighScoreRank.Should().Be(0);
            driver.Settings.Stored.EntriesFor(Difficulty.Legend)[0].Score.Should().Be(score);
            driver.Settings.LastName.Should().Be("ZED");
            driver.Music.Calls.Should().Contain("OnGameOver");
            driver.Sound.Played.Should().Contain(cue => cue.Effect == SoundEffect.HighScore);
            lines.Should().Contain(line => line.StartsWith("[BrixInvaders] high scores: saved ZED", System.StringComparison.Ordinal));
        }
        finally
        {
            GameLog.Sink = null;
        }
    }

    [Fact]
    public void the_high_scores_screen_returns_to_the_title_and_the_title_music()
    {
        //Arrange
        var driver = new SessionDriver(settings => settings.Difficulty = Difficulty.Legend);
        driver.ToPlaying();
        driver.PlayUntil(session => session.CurrentScreen == GameScreen.GameOver, 600, new GameInput(0, true, false));

        //Act
        driver.Wait(ScreenStateMachine.GameOverAutoSeconds + SessionDriver.Step);
        var entry = driver.Session.CurrentScreen;
        for (var i = 0; i < 3; i++)
        {
            driver.Press(SessionDriver.Confirm);
        }

        driver.Press(SessionDriver.Back);

        //Assert
        entry.Should().Be(GameScreen.HighScoreEntry);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
        driver.Session.Game.Should().BeNull();
        driver.Music.Calls.Last().Should().Be("OnTitle");
    }

    [Fact]
    public void the_title_goes_to_attract_after_fifteen_idle_seconds_and_back_on_any_input()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();

        //Act
        driver.Wait(ScreenStateMachine.AttractIdleSeconds + SessionDriver.Step);
        var attract = driver.Session.CurrentScreen;
        var demo = driver.Session.Attract;
        driver.Wait(2.0);
        var demoSteps = demo.StepCount;
        driver.Press(new MenuInput(other: true));

        //Assert
        attract.Should().Be(GameScreen.Attract);
        demo.Should().NotBeNull();
        demoSteps.Should().BeGreaterThan(100);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
        driver.Session.Attract.Should().BeNull();
    }

    [Fact]
    public void the_attract_demo_makes_no_sound()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Wait(ScreenStateMachine.AttractIdleSeconds + SessionDriver.Step);
        driver.Sound.Played.Clear();

        //Act
        driver.Wait(5.0);

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.Attract);
        driver.Sound.Played.Should().BeEmpty();
    }

    [Fact]
    public void Quit_on_the_title_raises_QuitRequested()
    {
        //Arrange
        var driver = new SessionDriver();
        var quit = 0;
        driver.Session.QuitRequested += () => quit++;
        driver.ToTitle();

        //Act
        for (var i = 0; i < 4; i++)
        {
            driver.Press(SessionDriver.Down);
        }

        driver.Press(SessionDriver.Confirm);

        //Assert
        quit.Should().Be(1);
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void menu_moves_and_confirms_click()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();

        //Act
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);
        driver.Press(SessionDriver.Back);

        //Assert
        driver.Sound.Played.Select(cue => cue.Effect).Should()
            .Equal(SoundEffect.MenuMove, SoundEffect.MenuConfirm, SoundEffect.MenuBack);
    }

    [Fact]
    public void the_Kenney_key_on_the_briefing_opens_the_bundle_link()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToBriefing();

        //Act
        driver.Press(SessionDriver.Link);

        //Assert
        driver.Links.Opened.Should().Equal(KenneyPacks.BundleUrl);
        driver.Session.Message.Should().BeNull();
    }

    [Fact]
    public void a_link_no_browser_took_shows_the_message_for_a_few_seconds()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.Links.Result = false;
        driver.ToTitle();

        //Act
        driver.Session.OpenLink("https://www.kenney.nl/");
        driver.Press(MenuInput.None);
        var shown = driver.Session.Message;
        driver.Wait(GameSession.LinkMessageSeconds + 0.1);

        //Assert
        shown.Should().Be(GameSession.NoBrowserMessage);
        driver.Session.Message.Should().BeNull();
    }

    [Fact]
    public void volume_changes_reach_the_mixer_and_the_music_director()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);

        //Act
        driver.Press(SessionDriver.Left);

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.Settings);
        driver.Settings.MasterVolume.Should().Be(0.7);
        driver.Sound.Levels.Last().Should().Be((0.7, 0.8));
        driver.Music.Calls.Last().Should().Be("SetVolumes(0.7, 0.6, 0.8)");
    }

    [Fact]
    public void a_music_choice_change_reaches_the_music_director()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);
        for (var i = 0; i < SettingsMenu.MusicModelRow; i++)
        {
            driver.Press(SessionDriver.Down);
        }

        //Act
        driver.Press(SessionDriver.Right);

        //Assert
        driver.Music.Calls.Last().Should().Be("ApplySettings(MuPT, ModestSynthGm, 0, False)");
    }

    [Fact]
    public void a_new_default_difficulty_is_offered_after_leaving_the_settings()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);
        for (var i = 0; i < SettingsMenu.DefaultDifficultyRow; i++)
        {
            driver.Press(SessionDriver.Down);
        }

        //Act
        driver.Press(SessionDriver.Right);
        driver.Press(SessionDriver.Back);

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.Title);
        driver.Session.Screens.Difficulty.Should().Be(Difficulty.Ace);
    }

    [Fact]
    public void resetting_the_high_scores_clears_the_tables()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.Session.HighScores.Insert(Difficulty.Pilot, "OLD", 5, 1);
        driver.ToTitle();
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Down);
        driver.Press(SessionDriver.Confirm);
        driver.Press(SessionDriver.Up);

        //Act
        driver.Press(SessionDriver.Confirm);
        driver.Press(SessionDriver.Confirm);

        //Assert
        driver.Session.HighScores.EntriesFor(Difficulty.Pilot).Should().BeEmpty();
        driver.Settings.ResetCount.Should().Be(1);
    }

    [Fact]
    public void the_start_sector_unlock_comes_from_the_stored_record()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        settings.RecordSectorCleared(Difficulty.Pilot, 2);

        //Act
        var session = new GameSession(settings, new RecordingSoundOutput(), new SilentMusicDirector(), new FakeLinkOpener(), 1);

        //Assert
        session.Screens.UnlockedSector(Difficulty.Pilot).Should().Be(3);
        session.Screens.UnlockedSector(Difficulty.Ace).Should().Be(1);
    }

    [Fact]
    public void starting_a_game_remembers_the_chosen_ship()
    {
        //Arrange
        var driver = new SessionDriver();
        driver.ToTitle();
        driver.Press(SessionDriver.Confirm);
        driver.Press(SessionDriver.Right);
        driver.Press(SessionDriver.Down);

        //Act
        driver.Press(SessionDriver.Confirm);
        driver.Press(SessionDriver.Confirm);

        //Assert
        driver.Session.Game.Setup.ShipShape.Should().Be(1);
        driver.Session.Game.Setup.ShipColour.Should().Be(1);
        driver.Settings.ShipShape.Should().Be(1);
        driver.Settings.ShipColour.Should().Be(1);
    }
}
