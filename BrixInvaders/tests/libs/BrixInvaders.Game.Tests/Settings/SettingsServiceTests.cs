using System;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Settings;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Input.Actions;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public void Initialize_opens_the_store_in_the_given_folder()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Assert
        SettingsService.IsInitialized.Should().BeTrue();
        SettingsService.DirectoryPath.Should().Be(store.Folder);
        SettingsService.AppName.Should().Be("BrixInvaders");
    }

    [Fact]
    public void every_value_has_its_documented_default()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Assert
        SettingsService.ShipShape.Should().Be(0);
        SettingsService.ShipColour.Should().Be(0);
        SettingsService.Difficulty.Should().Be(Difficulty.Pilot);
        SettingsService.MasterVolume.Should().Be(0.8);
        SettingsService.MusicVolume.Should().Be(0.6);
        SettingsService.EffectsVolume.Should().Be(0.8);
        SettingsService.MusicGenerator.Should().Be("SkyTNT");
        SettingsService.InstrumentLibrary.Should().Be("ModestSynthGm");
        SettingsService.GamepadProfile.Should().Be(GamepadProfile.Classic);
        SettingsService.LastInputDevice.Should().Be(InputDeviceKind.KeyboardMouse);
        SettingsService.LastName.Should().Be("AAA");
        SettingsService.GetHighestSectorCleared(Difficulty.Ace).Should().Be(0);
        SettingsService.LoadHighScores().EntriesFor(Difficulty.Pilot).Should().BeEmpty();
    }

    [Fact]
    public void every_value_round_trips_through_the_store()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.ShipShape = 2;
        SettingsService.ShipColour = 3;
        SettingsService.Difficulty = Difficulty.Legend;
        SettingsService.MasterVolume = 0.3;
        SettingsService.MusicVolume = 0.1;
        SettingsService.EffectsVolume = 1.0;
        SettingsService.MusicGenerator = "mupt";
        SettingsService.InstrumentLibrary = "FluidR3Gm";
        SettingsService.GamepadProfile = GamepadProfile.Shoulder;
        SettingsService.LastInputDevice = InputDeviceKind.Gamepad;
        SettingsService.LastName = "jer";
        store.Reopen();

        //Assert
        SettingsService.ShipShape.Should().Be(2);
        SettingsService.ShipColour.Should().Be(3);
        SettingsService.Difficulty.Should().Be(Difficulty.Legend);
        SettingsService.MasterVolume.Should().Be(0.3);
        SettingsService.MusicVolume.Should().Be(0.1);
        SettingsService.EffectsVolume.Should().Be(1.0);
        SettingsService.MusicGenerator.Should().Be("MuPT");
        SettingsService.InstrumentLibrary.Should().Be("FluidR3Gm");
        SettingsService.GamepadProfile.Should().Be(GamepadProfile.Shoulder);
        SettingsService.LastInputDevice.Should().Be(InputDeviceKind.Gamepad);
        SettingsService.LastName.Should().Be("JER");
    }

    [Fact]
    public void out_of_range_values_are_clamped()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.ShipShape = 9;
        SettingsService.ShipColour = -4;
        SettingsService.MasterVolume = 1.7;
        SettingsService.MusicVolume = -2;

        //Assert
        SettingsService.ShipShape.Should().Be(2);
        SettingsService.ShipColour.Should().Be(0);
        SettingsService.MasterVolume.Should().Be(1.0);
        SettingsService.MusicVolume.Should().Be(0.0);
    }

    [Fact]
    public void high_scores_round_trip_per_difficulty_as_json()
    {
        //Arrange
        using var store = new TempSettingsStore();
        var table = new HighScoreTable();
        table.Insert(Difficulty.Pilot, "JER", 48210, 4);
        table.Insert(Difficulty.Pilot, "BOB", 900, 1);
        table.Insert(Difficulty.Ace, "ACE", 12000, 2);

        //Act
        var json = SettingsService.SaveHighScores(table, Difficulty.Pilot);
        SettingsService.SaveHighScores(table, Difficulty.Ace);
        store.Reopen();
        var loaded = SettingsService.LoadHighScores();

        //Assert
        json.Should().Be("[{\"Name\":\"JER\",\"Score\":48210,\"Sector\":4},{\"Name\":\"BOB\",\"Score\":900,\"Sector\":1}]");
        loaded.EntriesFor(Difficulty.Pilot).Should().HaveCount(2);
        loaded.EntriesFor(Difficulty.Pilot)[0].Name.Should().Be("JER");
        loaded.EntriesFor(Difficulty.Pilot)[0].Score.Should().Be(48210);
        loaded.EntriesFor(Difficulty.Pilot)[0].Sector.Should().Be(4);
        loaded.EntriesFor(Difficulty.Ace)[0].Name.Should().Be("ACE");
        loaded.EntriesFor(Difficulty.Cadet).Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"Name\":\"X\"}")]
    [InlineData("[null]")]
    public void FromJson_reads_malformed_data_as_no_records(string json) => SettingsService.FromJson(json).Should().BeEmpty();

    [Fact]
    public void ToJson_and_FromJson_round_trip()
    {
        //Arrange
        var entries = new[] { new HighScoreEntry("ABC", 100, 2) };

        //Act
        var records = SettingsService.FromJson(SettingsService.ToJson(entries));

        //Assert
        records.Should().HaveCount(1);
        records[0].Name.Should().Be("ABC");
        records[0].Score.Should().Be(100);
        records[0].Sector.Should().Be(2);
    }

    [Fact]
    public void ResetHighScores_clears_every_table()
    {
        //Arrange
        using var store = new TempSettingsStore();
        var table = new HighScoreTable();
        table.Insert(Difficulty.Cadet, "ONE", 10, 1);
        table.Insert(Difficulty.Legend, "TWO", 20, 1);
        SettingsService.SaveAllHighScores(table);

        //Act
        SettingsService.ResetHighScores();

        //Assert
        var loaded = SettingsService.LoadHighScores();
        loaded.EntriesFor(Difficulty.Cadet).Should().BeEmpty();
        loaded.EntriesFor(Difficulty.Legend).Should().BeEmpty();
    }

    [Fact]
    public void RecordSectorCleared_only_ever_grows()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.RecordSectorCleared(Difficulty.Ace, 3);
        SettingsService.RecordSectorCleared(Difficulty.Ace, 2);

        //Assert
        SettingsService.GetHighestSectorCleared(Difficulty.Ace).Should().Be(3);
        SettingsService.GetHighestSectorCleared(Difficulty.Pilot).Should().Be(0);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(4, 5)]
    [InlineData(9, 5)]
    public void UnlockedStartSector_is_one_past_the_best_cleared_up_to_five(int cleared, int expected)
    {
        //Arrange
        using var store = new TempSettingsStore();
        SettingsService.RecordSectorCleared(Difficulty.Cadet, cleared);

        //Act
        var unlocked = SettingsService.UnlockedStartSector(Difficulty.Cadet);

        //Assert
        unlocked.Should().Be(expected);
    }

    [Fact]
    public void CreateMusicSettings_carries_the_choices_at_full_level()
    {
        //Arrange
        using var store = new TempSettingsStore();
        SettingsService.MusicGenerator = "MuPT";
        SettingsService.MusicVolume = 0.2;

        //Act
        var music = SettingsService.CreateMusicSettings();

        //Assert
        music.GeneratorName.Should().Be("MuPT");
        music.InstrumentLibraryName.Should().Be("ModestSynthGm");
        music.MusicVolume.Should().Be(1.0);
    }

    [Fact]
    public void reading_before_Initialize_throws()
    {
        //Arrange
        SettingsService.Shutdown();

        //Act
        var read = new Action(() => _ = SettingsService.ShipShape);

        //Assert
        read.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StoredGameSettings_reads_and_writes_through_the_facade()
    {
        //Arrange
        using var store = new TempSettingsStore();
        var settings = new StoredGameSettings();

        //Act
        settings.Difficulty = Difficulty.Ace;
        settings.RecordSectorCleared(Difficulty.Ace, 2);

        //Assert
        SettingsService.Difficulty.Should().Be(Difficulty.Ace);
        settings.GetHighestSectorCleared(Difficulty.Ace).Should().Be(2);
    }
}
