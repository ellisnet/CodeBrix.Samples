using System.Linq;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Game.Tests.Support;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Settings;

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
        SettingsService.AppName.Should().Be("GoddessTempleDiscovery");
        SettingsService.SoundEnabledKey.Should().StartWith("GoddessTempleDiscovery.");
    }

    [Fact]
    public void every_value_has_its_documented_default()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Assert
        SettingsService.SoundEnabled.Should().BeTrue();
        SettingsService.ReducedMotion.Should().BeFalse();
        SettingsService.AnimationSpeed.Should().Be(1.0);
        SettingsService.RevealComputerDiscoveries.Should().BeTrue();
        SettingsService.TurnsPerSeason.Should().Be(2);
        SettingsService.Difficulty.Should().Be(Difficulty.Standard);
        SettingsService.LastSeatsJson.Should().BeEmpty();
        SettingsService.LoadLastSeats().Should().BeEmpty();
    }

    [Fact]
    public void every_value_round_trips_through_the_store()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.SoundEnabled = false;
        SettingsService.ReducedMotion = true;
        SettingsService.AnimationSpeed = 2.5;
        SettingsService.RevealComputerDiscoveries = false;
        SettingsService.TurnsPerSeason = 3;
        SettingsService.Difficulty = Difficulty.Hard;
        SettingsService.SaveLastSeats(new[]
        {
            new SeatRecord { TeamName = "Ishtar's Own", IsComputer = false, Temperament = "Scholar" },
            new SeatRecord { TeamProfileId = "lapis-road-society", IsComputer = true, Temperament = "DeepDigger" },
        });
        store.Reopen();

        //Assert
        SettingsService.SoundEnabled.Should().BeFalse();
        SettingsService.ReducedMotion.Should().BeTrue();
        SettingsService.AnimationSpeed.Should().Be(2.5);
        SettingsService.RevealComputerDiscoveries.Should().BeFalse();
        SettingsService.TurnsPerSeason.Should().Be(3);
        SettingsService.Difficulty.Should().Be(Difficulty.Hard);
        var seats = SettingsService.LoadLastSeats();
        seats.Should().HaveCount(2);
        seats[0].TeamName.Should().Be("Ishtar's Own");
        seats[1].TeamProfileId.Should().Be("lapis-road-society");
        seats[1].IsComputer.Should().BeTrue();
        seats.Last().Temperament.Should().Be("DeepDigger");
    }

    [Fact]
    public void out_of_range_values_are_clamped()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.AnimationSpeed = 99;
        SettingsService.TurnsPerSeason = 9;

        //Assert
        SettingsService.AnimationSpeed.Should().Be(SettingsService.MaxAnimationSpeed);
        SettingsService.TurnsPerSeason.Should().Be(GameRules.MaxTurnsPerSeason);
    }

    [Fact]
    public void malformed_last_seats_read_as_none()
    {
        //Arrange
        using var store = new TempSettingsStore();

        //Act
        SettingsService.LastSeatsJson = "{not json";

        //Assert
        SettingsService.LoadLastSeats().Should().BeEmpty();
    }
}
