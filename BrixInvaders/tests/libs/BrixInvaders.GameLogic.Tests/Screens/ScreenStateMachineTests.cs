using System;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class ScreenStateMachineTests
{
    private const double Frame = 1.0 / 60.0;

    private static readonly MenuInput Confirm = new MenuInput(confirm: true);
    private static readonly MenuInput Back = new MenuInput(back: true);
    private static readonly MenuInput Up = new MenuInput(up: true);
    private static readonly MenuInput Down = new MenuInput(down: true);
    private static readonly MenuInput Left = new MenuInput(left: true);
    private static readonly MenuInput Right = new MenuInput(right: true);
    private static readonly MenuInput Pause = new MenuInput(pause: true);
    private static readonly MenuInput Start = new MenuInput(start: true);
    private static readonly MenuInput Link = new MenuInput(kenneyLink: true);

    private static ScreenStateMachine AtTitle()
    {
        var machine = new ScreenStateMachine();
        machine.NotifySplashComplete();
        return machine;
    }

    private static void Wait(ScreenStateMachine machine, double seconds)
    {
        var frames = (int)Math.Ceiling(seconds / Frame);
        for (var i = 0; i < frames; i++)
        {
            machine.Update(Frame, MenuInput.None);
        }
    }

    private static ScreenStateMachine AtPlaying()
    {
        var machine = AtTitle();
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);
        Wait(machine, ScreenStateMachine.BriefingMinSeconds);
        machine.Update(Frame, Confirm);
        return machine;
    }

    private static bool Has(ScreenStateMachine machine, ScreenCommandKind kind) => machine.Commands.Any(c => c.Kind == kind);

    [Fact]
    public void the_machine_starts_on_the_splash()
    {
        //Act
        var machine = new ScreenStateMachine();

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Splash);
        machine.Difficulty.Should().Be(Difficulty.Pilot);
    }

    [Fact]
    public void NotifySplashComplete_goes_to_the_title()
    {
        //Arrange
        var machine = new ScreenStateMachine();

        //Act
        machine.NotifySplashComplete();

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        machine.PreviousScreen.Should().Be(GameScreen.Splash);
        machine.Commands.Should().ContainSingle(c => c.Kind == ScreenCommandKind.ScreenChanged && c.Value == (int)GameScreen.Title);
    }

    [Fact]
    public void Confirm_skips_the_splash()
    {
        //Arrange
        var machine = new ScreenStateMachine();

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void the_splash_times_out_to_the_title()
    {
        //Arrange
        var machine = new ScreenStateMachine();

        //Act
        Wait(machine, ScreenStateMachine.SplashMaxSeconds + Frame);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void title_cursor_wraps_both_ways()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        machine.Update(Frame, Up);
        var afterUp = machine.TitleCursor;
        machine.Update(Frame, Down);

        //Assert
        afterUp.Should().Be(TitleMenuItem.Quit);
        machine.TitleCursor.Should().Be(TitleMenuItem.Play);
    }

    [Fact]
    public void Quit_on_the_title_asks_the_host_to_close_and_stays_on_the_title()
    {
        //Arrange
        var machine = AtTitle();
        for (var i = 0; i < 4; i++)
        {
            machine.Update(Frame, Down);
        }

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.TitleCursor.Should().Be(TitleMenuItem.Quit);
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        machine.Commands.Should().ContainSingle(command => command.Kind == ScreenCommandKind.QuitGame);
    }

    [Theory]
    [InlineData(0, GameScreen.ShipSelect)]
    [InlineData(1, GameScreen.HighScores)]
    [InlineData(2, GameScreen.Settings)]
    [InlineData(3, GameScreen.Credits)]
    public void title_menu_items_open_their_screens(int downs, GameScreen expected)
    {
        //Arrange
        var machine = AtTitle();
        for (var i = 0; i < downs; i++)
        {
            machine.Update(Frame, Down);
        }

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.CurrentScreen.Should().Be(expected);
    }

    [Fact]
    public void Start_also_confirms_on_the_title()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        machine.Update(Frame, Start);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.ShipSelect);
    }

    [Fact]
    public void attract_mode_starts_after_fifteen_idle_seconds_on_the_title()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        Wait(machine, ScreenStateMachine.AttractIdleSeconds - 0.1);
        var before = machine.CurrentScreen;
        Wait(machine, 0.2);

        //Assert
        before.Should().Be(GameScreen.Title);
        machine.CurrentScreen.Should().Be(GameScreen.Attract);
    }

    [Fact]
    public void attract_mode_start_is_announced()
    {
        //Arrange
        var machine = AtTitle();
        Wait(machine, ScreenStateMachine.AttractIdleSeconds - Frame);

        //Act
        var commandsSeen = false;
        for (var i = 0; i < 30 && machine.CurrentScreen == GameScreen.Title; i++)
        {
            machine.Update(Frame, MenuInput.None);
            commandsSeen = Has(machine, ScreenCommandKind.StartAttract);
        }

        //Assert
        commandsSeen.Should().BeTrue();
    }

    [Fact]
    public void input_on_the_title_resets_the_idle_timer()
    {
        //Arrange
        var machine = AtTitle();
        Wait(machine, 10);

        //Act
        machine.Update(Frame, new MenuInput(other: true));
        Wait(machine, 10);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void any_input_returns_from_attract_to_the_title()
    {
        //Arrange
        var machine = AtTitle();
        Wait(machine, ScreenStateMachine.AttractIdleSeconds + 0.1);

        //Act
        machine.Update(Frame, new MenuInput(other: true));

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        Has(machine, ScreenCommandKind.StopAttract).Should().BeTrue();
    }

    [Fact]
    public void attract_mode_times_out_to_the_title()
    {
        //Arrange
        var machine = AtTitle();
        Wait(machine, ScreenStateMachine.AttractIdleSeconds + 0.1);

        //Act
        Wait(machine, ScreenStateMachine.AttractMaxSeconds + 0.1);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void NotifyAttractEnded_returns_to_the_title()
    {
        //Arrange
        var machine = AtTitle();
        Wait(machine, ScreenStateMachine.AttractIdleSeconds + 0.1);

        //Act
        machine.NotifyAttractEnded();

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        Has(machine, ScreenCommandKind.StopAttract).Should().BeTrue();
    }

    [Fact]
    public void ship_select_cycles_three_shapes_and_four_colours_with_wrap()
    {
        //Arrange
        var machine = AtTitle();
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Left);
        var shapeAfterLeft = machine.ShipShape;
        machine.Update(Frame, Right);
        machine.Update(Frame, Right);
        machine.Update(Frame, Up);
        var colourAfterUp = machine.ShipColour;
        machine.Update(Frame, Down);
        machine.Update(Frame, Down);

        //Assert
        shapeAfterLeft.Should().Be(2);
        machine.ShipShape.Should().Be(1);
        colourAfterUp.Should().Be(3);
        machine.ShipColour.Should().Be(1);
    }

    [Fact]
    public void ship_select_back_returns_to_the_title_and_confirm_goes_to_difficulty()
    {
        //Arrange
        var backMachine = AtTitle();
        backMachine.Update(Frame, Confirm);
        var confirmMachine = AtTitle();
        confirmMachine.Update(Frame, Confirm);

        //Act
        backMachine.Update(Frame, Back);
        confirmMachine.Update(Frame, Confirm);

        //Assert
        backMachine.CurrentScreen.Should().Be(GameScreen.Title);
        confirmMachine.CurrentScreen.Should().Be(GameScreen.DifficultySelect);
    }

    [Fact]
    public void difficulty_select_cycles_levels_and_back_returns_to_ship_select()
    {
        //Arrange
        var machine = AtTitle();
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Down);
        machine.Update(Frame, Down);
        var afterTwoDowns = machine.Difficulty;
        machine.Update(Frame, Down);
        var wrapped = machine.Difficulty;
        machine.Update(Frame, Back);

        //Assert
        afterTwoDowns.Should().Be(Difficulty.Legend);
        wrapped.Should().Be(Difficulty.Cadet);
        machine.CurrentScreen.Should().Be(GameScreen.ShipSelect);
    }

    [Fact]
    public void the_start_sector_is_limited_to_the_unlocked_sector()
    {
        //Arrange
        var machine = AtTitle();
        machine.SetUnlockedSector(Difficulty.Pilot, 3);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);

        //Act
        for (var i = 0; i < 6; i++)
        {
            machine.Update(Frame, Right);
        }

        var highest = machine.StartSector;
        machine.Update(Frame, Down);
        var afterSwitchingToLockedLevel = machine.StartSector;

        //Assert
        highest.Should().Be(3);
        afterSwitchingToLockedLevel.Should().Be(1);
    }

    [Fact]
    public void confirming_the_difficulty_starts_a_new_game_at_the_briefing()
    {
        //Arrange
        var machine = AtTitle();
        machine.SetUnlockedSector(Difficulty.Pilot, 2);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Right);

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.SectorBriefing);
        machine.CurrentSector.Should().Be(2);
        machine.Commands.Should().Contain(c => c.Kind == ScreenCommandKind.StartNewGame && c.Value == 2);
    }

    [Fact]
    public void the_briefing_ignores_confirm_for_the_first_second_then_begins_the_sector()
    {
        //Arrange
        var machine = AtTitle();
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Confirm);
        var early = machine.CurrentScreen;
        Wait(machine, ScreenStateMachine.BriefingMinSeconds);
        machine.Update(Frame, Confirm);

        //Assert
        early.Should().Be(GameScreen.SectorBriefing);
        machine.CurrentScreen.Should().Be(GameScreen.Playing);
        machine.Commands.Should().Contain(c => c.Kind == ScreenCommandKind.BeginSector && c.Value == 1);
    }

    [Theory]
    [InlineData(GameScreen.SectorBriefing)]
    [InlineData(GameScreen.SectorClear)]
    [InlineData(GameScreen.Credits)]
    public void the_kenney_link_opens_from_the_card_screens(GameScreen screen)
    {
        //Arrange
        var machine = AtTitle();
        if (screen == GameScreen.SectorBriefing)
        {
            machine.Update(Frame, Confirm);
            machine.Update(Frame, Confirm);
            machine.Update(Frame, Confirm);
        }
        else if (screen == GameScreen.SectorClear)
        {
            machine = AtPlaying();
            machine.NotifySectorCleared(1);
        }
        else
        {
            //Up twice: past Quit (the last item) to Credits
            machine.Update(Frame, Up);
            machine.Update(Frame, Up);
            machine.Update(Frame, Confirm);
        }

        //Act
        machine.Update(Frame, Link);

        //Assert
        machine.CurrentScreen.Should().Be(screen);
        Has(machine, ScreenCommandKind.OpenKenneyLink).Should().BeTrue();
    }

    [Fact]
    public void the_kenney_link_does_nothing_on_the_title()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        machine.Update(Frame, Link);

        //Assert
        Has(machine, ScreenCommandKind.OpenKenneyLink).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void pause_or_start_opens_the_pause_overlay(bool useStart)
    {
        //Arrange
        var machine = AtPlaying();

        //Act
        machine.Update(Frame, useStart ? Start : Pause);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Paused);
        machine.PauseCursor.Should().Be(PauseMenuItem.Resume);
        Has(machine, ScreenCommandKind.PauseGame).Should().BeTrue();
    }

    [Theory]
    [InlineData("pause")]
    [InlineData("start")]
    [InlineData("back")]
    [InlineData("confirm")]
    public void the_pause_overlay_resumes(string how)
    {
        //Arrange
        var machine = AtPlaying();
        machine.Update(Frame, Pause);
        var input = how switch
        {
            "pause" => Pause,
            "start" => Start,
            "back" => Back,
            _ => Confirm,
        };

        //Act
        machine.Update(Frame, input);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Playing);
        Has(machine, ScreenCommandKind.ResumeGame).Should().BeTrue();
    }

    [Fact]
    public void quit_to_title_from_the_pause_overlay_abandons_the_game()
    {
        //Arrange
        var machine = AtPlaying();
        machine.Update(Frame, Pause);
        machine.Update(Frame, Down);

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        Has(machine, ScreenCommandKind.AbandonGame).Should().BeTrue();
    }

    [Fact]
    public void a_cleared_sector_shows_the_clear_screen_then_the_next_briefing()
    {
        //Arrange
        var machine = AtPlaying();
        machine.NotifySectorCleared(1);
        var screen = machine.CurrentScreen;

        //Act
        machine.Update(Frame, Confirm);
        var early = machine.CurrentScreen;
        Wait(machine, ScreenStateMachine.SectorClearMinSeconds);
        machine.Update(Frame, Confirm);

        //Assert
        screen.Should().Be(GameScreen.SectorClear);
        early.Should().Be(GameScreen.SectorClear);
        machine.CurrentScreen.Should().Be(GameScreen.SectorBriefing);
        machine.CurrentSector.Should().Be(2);
        machine.UnlockedSector(Difficulty.Pilot).Should().Be(2);
    }

    [Fact]
    public void unlocks_stop_at_sector_five()
    {
        //Arrange
        var machine = AtPlaying();

        //Act
        machine.NotifySectorCleared(9);

        //Assert
        machine.UnlockedSector(Difficulty.Pilot).Should().Be(ScreenStateMachine.MaxUnlockableSector);
    }

    [Fact]
    public void a_qualifying_game_over_goes_through_name_entry_to_the_high_scores()
    {
        //Arrange
        var machine = AtPlaying();
        machine.SetLastName("JER");
        machine.NotifyGameOver(12345, true);

        //Act
        machine.Update(Frame, Confirm);
        var early = machine.CurrentScreen;
        Wait(machine, ScreenStateMachine.GameOverMinSeconds);
        machine.Update(Frame, Confirm);
        var entryScreen = machine.CurrentScreen;
        var prefilled = machine.NameEntry.Name;
        machine.Update(Frame, Up);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);
        machine.Update(Frame, Confirm);

        //Assert
        early.Should().Be(GameScreen.GameOver);
        entryScreen.Should().Be(GameScreen.HighScoreEntry);
        prefilled.Should().Be("JER");
        machine.CurrentScreen.Should().Be(GameScreen.HighScores);
        machine.Commands.Should().Contain(c => c.Kind == ScreenCommandKind.SubmitHighScore && c.Text == "KER");
        machine.LastName.Should().Be("KER");
        machine.FinalScore.Should().Be(12345);
        machine.PendingHighScore.Should().BeFalse();
        machine.HighScoreViewDifficulty.Should().Be(Difficulty.Pilot);
    }

    [Fact]
    public void a_non_qualifying_game_over_returns_to_the_title()
    {
        //Arrange
        var machine = AtPlaying();
        machine.NotifyGameOver(10, false);
        Wait(machine, ScreenStateMachine.GameOverMinSeconds);

        //Act
        machine.Update(Frame, Confirm);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void the_game_over_screen_moves_on_by_itself()
    {
        //Arrange
        var machine = AtPlaying();
        machine.NotifyGameOver(10, true);

        //Act
        Wait(machine, ScreenStateMachine.GameOverAutoSeconds + Frame);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.HighScoreEntry);
    }

    [Fact]
    public void game_over_from_the_pause_overlay_is_accepted()
    {
        //Arrange
        var machine = AtPlaying();
        machine.Update(Frame, Pause);

        //Act
        machine.NotifyGameOver(10, false);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.GameOver);
    }

    [Fact]
    public void notifications_are_ignored_on_unrelated_screens()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        machine.NotifyGameOver(10, true);
        machine.NotifySectorCleared(1);
        machine.NotifyAttractEnded();
        machine.NotifySplashComplete();

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        machine.UnlockedSector(Difficulty.Pilot).Should().Be(1);
    }

    [Fact]
    public void high_scores_screen_cycles_difficulties_and_returns_to_the_title()
    {
        //Arrange
        var machine = AtTitle();
        machine.Update(Frame, Down);
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Left);
        machine.Update(Frame, Left);
        var afterTwoLefts = machine.HighScoreViewDifficulty;
        machine.Update(Frame, Right);
        var afterRight = machine.HighScoreViewDifficulty;
        machine.Update(Frame, Back);

        //Assert
        afterTwoLefts.Should().Be(Difficulty.Legend);
        afterRight.Should().Be(Difficulty.Cadet);
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void settings_moves_the_cursor_and_asks_the_game_to_adjust_and_activate()
    {
        //Arrange
        var machine = new ScreenStateMachine(settingsItemCount: 3);
        machine.NotifySplashComplete();
        machine.Update(Frame, Down);
        machine.Update(Frame, Down);
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Up);
        var wrapped = machine.SettingsCursor;
        machine.Update(Frame, Left);
        var adjustDown = machine.Commands.Single(c => c.Kind == ScreenCommandKind.AdjustSetting);
        machine.Update(Frame, Right);
        var adjustUp = machine.Commands.Single(c => c.Kind == ScreenCommandKind.AdjustSetting);
        machine.Update(Frame, Confirm);
        var activate = machine.Commands.Single(c => c.Kind == ScreenCommandKind.ActivateSetting);
        machine.Update(Frame, Back);

        //Assert
        wrapped.Should().Be(2);
        adjustDown.Value.Should().Be(2);
        adjustDown.Delta.Should().Be(-1);
        adjustUp.Delta.Should().Be(1);
        activate.Value.Should().Be(2);
        machine.CurrentScreen.Should().Be(GameScreen.Title);
    }

    [Fact]
    public void credits_return_to_the_title()
    {
        //Arrange
        var machine = AtTitle();
        machine.Update(Frame, Up);
        machine.Update(Frame, Up);
        machine.Update(Frame, Confirm);

        //Act
        machine.Update(Frame, Back);

        //Assert
        machine.CurrentScreen.Should().Be(GameScreen.Title);
        machine.PreviousScreen.Should().Be(GameScreen.Credits);
    }

    [Fact]
    public void commands_are_cleared_at_the_start_of_every_update()
    {
        //Arrange
        var machine = AtTitle();

        //Act
        machine.Update(Frame, MenuInput.None);

        //Assert
        machine.Commands.Should().BeEmpty();
    }

    [Fact]
    public void constructor_rejects_zero_settings_items()
    {
        //Act
        Action act = () => new ScreenStateMachine(0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void constructor_wraps_ship_defaults()
    {
        //Act
        var machine = new ScreenStateMachine(shipShape: 4, shipColour: -1);

        //Assert
        machine.ShipShape.Should().Be(1);
        machine.ShipColour.Should().Be(3);
    }
}
