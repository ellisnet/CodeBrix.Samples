using BrixInvaders.Game.Input;
using BrixInvaders.Game.Settings;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Settings;

public class SettingsMenuTests
{
    [Fact]
    public void there_is_one_row_per_setting_in_the_design() => SettingsMenu.RowCount.Should().Be(ScreenStateMachine.DefaultSettingsItemCount);

    [Theory]
    [InlineData(SettingsMenu.MasterVolumeRow)]
    [InlineData(SettingsMenu.MusicVolumeRow)]
    [InlineData(SettingsMenu.EffectsVolumeRow)]
    public void Adjust_steps_a_volume_by_a_tenth_within_zero_and_one(int row)
    {
        //Arrange
        var settings = new MemoryGameSettings { MasterVolume = 0.95, MusicVolume = 0.95, EffectsVolume = 0.95 };
        var menu = new SettingsMenu(settings);

        //Act
        var change = menu.Adjust(row, 1);
        var top = menu.ValueOf(row);
        for (var i = 0; i < 15; i++)
        {
            menu.Adjust(row, -1);
        }

        //Assert
        change.Should().Be(SettingsChange.Volumes);
        top.Should().Be("100%");
        menu.ValueOf(row).Should().Be("0%");
    }

    [Fact]
    public void Adjust_cycles_the_music_model_and_the_instrument_library()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        var menu = new SettingsMenu(settings);

        //Act
        var model = menu.Adjust(SettingsMenu.MusicModelRow, 1);
        var library = menu.Adjust(SettingsMenu.InstrumentLibraryRow, -1);

        //Assert
        model.Should().Be(SettingsChange.MusicChoice);
        library.Should().Be(SettingsChange.MusicChoice);
        settings.MusicGenerator.Should().Be("MuPT");
        settings.InstrumentLibrary.Should().Be("FluidR3Gm");
    }

    [Fact]
    public void Adjust_toggles_the_gamepad_profile()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        var menu = new SettingsMenu(settings);

        //Act
        var change = menu.Adjust(SettingsMenu.GamepadProfileRow, 1);

        //Assert
        change.Should().Be(SettingsChange.GamepadProfile);
        settings.GamepadProfile.Should().Be(GamepadProfile.Shoulder);
        menu.ValueOf(SettingsMenu.GamepadProfileRow).Should().Contain("RB fire");
    }

    [Fact]
    public void Adjust_cycles_all_twelve_ships_and_wraps()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        var menu = new SettingsMenu(settings);

        //Act
        menu.Adjust(SettingsMenu.DefaultShipRow, -1);
        var last = (settings.ShipShape, settings.ShipColour);
        for (var i = 0; i < 12; i++)
        {
            menu.Adjust(SettingsMenu.DefaultShipRow, 1);
        }

        //Assert
        last.Should().Be((2, 3));
        (settings.ShipShape, settings.ShipColour).Should().Be((2, 3));
    }

    [Fact]
    public void Adjust_wraps_the_default_difficulty()
    {
        //Arrange
        var settings = new MemoryGameSettings { Difficulty = Difficulty.Legend };
        var menu = new SettingsMenu(settings);

        //Act
        var change = menu.Adjust(SettingsMenu.DefaultDifficultyRow, 1);

        //Assert
        change.Should().Be(SettingsChange.Defaults);
        settings.Difficulty.Should().Be(Difficulty.Cadet);
    }

    [Fact]
    public void Activate_needs_two_confirms_to_reset_the_high_scores()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        var menu = new SettingsMenu(settings);

        //Act
        var first = menu.Activate(SettingsMenu.ResetHighScoresRow);
        var armedText = menu.ValueOf(SettingsMenu.ResetHighScoresRow);
        var second = menu.Activate(SettingsMenu.ResetHighScoresRow);

        //Assert
        first.Should().Be(SettingsChange.ResetArmed);
        armedText.Should().Be("Confirm again to clear");
        second.Should().Be(SettingsChange.HighScoresReset);
        settings.ResetCount.Should().Be(1);
        menu.ValueOf(SettingsMenu.ResetHighScoresRow).Should().Be("Cleared");
    }

    [Fact]
    public void moving_away_disarms_the_reset()
    {
        //Arrange
        var settings = new MemoryGameSettings();
        var menu = new SettingsMenu(settings);
        menu.Activate(SettingsMenu.ResetHighScoresRow);

        //Act
        menu.Adjust(SettingsMenu.MasterVolumeRow, 1);
        var afterAdjust = menu.Activate(SettingsMenu.ResetHighScoresRow);

        //Assert
        afterAdjust.Should().Be(SettingsChange.ResetArmed);
        settings.ResetCount.Should().Be(0);
    }

    [Fact]
    public void Activate_does_nothing_on_the_other_rows() =>
        new SettingsMenu(new MemoryGameSettings()).Activate(SettingsMenu.MasterVolumeRow).Should().Be(SettingsChange.None);

    [Fact]
    public void every_row_has_a_label_and_a_value()
    {
        //Arrange
        var menu = new SettingsMenu(new MemoryGameSettings());

        for (var row = 0; row < SettingsMenu.RowCount; row++)
        {
            //Assert
            SettingsMenu.LabelOf(row).Should().NotBeNullOrWhiteSpace();
            menu.ValueOf(row).Should().NotBeNullOrWhiteSpace();
        }
    }
}
