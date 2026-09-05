using GitHubIssueFinder.Settings;
using SilverAssertions;
using Xunit;
using System;
using System.IO;

namespace GitHubIssueFinder.Settings.Tests;

//A stand-in for the application's own colour-scheme enum, which lives in the Core project
//and is therefore out of reach here. Enums are stored by name, so any enum will do.
internal enum TestColorScheme
{
    SystemDefault,
    Light,
    LightHighContrast,
    Dark,
    DarkDimmed,
}

//Every settings test lives in this one class on purpose: the facade is process-global
//state and the tests of a single class run one after another, which keeps the
//initialize and shutdown test out of the round-trip tests' way.
public class SettingsServiceTests
{
    //A key nothing else in the assembly touches, so a "nothing stored yet" assertion holds
    //however the tests are ordered.
    private static string FreshKey() =>
        "GitHubIssueFinder.Settings.Test." + Guid.NewGuid().ToString("N");

    [Fact]
    public void the_store_is_registered_under_the_application_name()
    {
        //Assert
        SettingsService.AppName.Should().Be("GitHubIssueFinder");
        SettingsService.Store.AppName.Should().Be("GitHubIssueFinder");
    }

    [Fact]
    public void every_key_is_prefixed_with_the_application_settings_namespace()
    {
        //Assert
        SettingKeys.Owner.Should().Be("GitHubIssueFinder.Settings.Owner");
        SettingKeys.Assignee.Should().Be("GitHubIssueFinder.Settings.Assignee");
        SettingKeys.IncludeClosed.Should().Be("GitHubIssueFinder.Settings.IncludeClosed");
        SettingKeys.ColorScheme.Should().Be("GitHubIssueFinder.Settings.ColorScheme");
    }

    [Fact]
    public void the_store_is_open_before_any_test_runs()
    {
        //Assert
        SettingsService.IsInitialized.Should().BeTrue();
        SettingsService.Store.Should().NotBeNull();
    }

    [Fact]
    public void the_store_lives_in_the_throwaway_test_folder()
    {
        //Assert - never the user's real settings folder
        SettingsService.Store.DirectoryPath.Should().Be(TestSettingsStore.DirectoryPath);
        TestSettingsStore.DirectoryPath.Should().StartWith(AppContext.BaseDirectory);
        File.Exists(SettingsService.Store.DatabaseFilePath).Should().BeTrue();
    }

    [Fact]
    public void shutdown_closes_the_store_and_initialize_opens_it_again()
    {
        //Arrange - the value must survive the round trip through a closed store
        var key = FreshKey();
        SettingsService.Set(key, "survives");

        try
        {
            //Act
            SettingsService.Shutdown();

            //Assert
            SettingsService.IsInitialized.Should().BeFalse();
        }
        finally
        {
            //The rest of the assembly needs the store open again, in the same folder.
            SettingsService.Initialize(TestSettingsStore.DirectoryPath);
        }

        //Assert
        SettingsService.IsInitialized.Should().BeTrue();
        SettingsService.Get<string>(key).Should().Be("survives");
    }

    [Fact]
    public void the_owner_round_trips_as_a_string()
    {
        //Act
        SettingsService.Set(SettingKeys.Owner, "ellisnet");

        //Assert
        SettingsService.Get<string>(SettingKeys.Owner).Should().Be("ellisnet");
        SettingsService.HasValue(SettingKeys.Owner).Should().BeTrue();
    }

    [Fact]
    public void the_assignee_round_trips_as_a_string()
    {
        //Act
        SettingsService.Set(SettingKeys.Assignee, "octocat");

        //Assert
        SettingsService.Get<string>(SettingKeys.Assignee).Should().Be("octocat");
    }

    [Fact]
    public void include_closed_round_trips_as_a_bool()
    {
        //Act
        SettingsService.Set(SettingKeys.IncludeClosed, true);

        //Assert
        SettingsService.Get<bool>(SettingKeys.IncludeClosed).Should().BeTrue();

        //Act
        SettingsService.Set(SettingKeys.IncludeClosed, false);

        //Assert
        SettingsService.Get(SettingKeys.IncludeClosed, true).Should().BeFalse();
    }

    [Fact]
    public void the_colour_scheme_round_trips_by_enum_name()
    {
        //Act
        SettingsService.Set(SettingKeys.ColorScheme, TestColorScheme.DarkDimmed);

        //Assert - stored as the name, so renumbering the enum never repaints the application
        SettingsService.Get<TestColorScheme>(SettingKeys.ColorScheme).Should().Be(TestColorScheme.DarkDimmed);
        SettingsService.Get<string>(SettingKeys.ColorScheme).Should().Be("DarkDimmed");

        //Act - a name written as text reads back as the enum value
        SettingsService.Set(SettingKeys.ColorScheme, "LightHighContrast");

        //Assert
        SettingsService.Get<TestColorScheme>(SettingKeys.ColorScheme)
            .Should().Be(TestColorScheme.LightHighContrast);
    }

    [Fact]
    public void a_wrapped_setting_round_trips_through_its_typed_handle()
    {
        //Arrange
        var key = FreshKey();

        //Act
        var handle = SettingsService.Wrap(key, "unassigned");

        //Assert - the handle reads its default while nothing is stored
        handle.Value.Should().Be("unassigned");

        //Act
        handle.Value = "octocat";

        //Assert - the value went to the store, not just to the handle
        handle.Value.Should().Be("octocat");
        SettingsService.Get<string>(key).Should().Be("octocat");
    }

    [Fact]
    public void has_value_is_false_until_something_is_stored()
    {
        //Arrange
        var key = FreshKey();

        //Assert
        SettingsService.HasValue(key).Should().BeFalse();

        //Act
        SettingsService.Set(key, "now there is");

        //Assert
        SettingsService.HasValue(key).Should().BeTrue();
    }

    [Fact]
    public void get_returns_the_supplied_default_when_nothing_is_stored()
    {
        //Arrange
        var key = FreshKey();

        //Assert
        SettingsService.Get(key, "fallback").Should().Be("fallback");
        SettingsService.Get(key, 42).Should().Be(42);
        SettingsService.Get<string>(key).Should().BeNull();
    }

    [Fact]
    public void storing_a_null_removes_the_key()
    {
        //Arrange
        var key = FreshKey();
        SettingsService.Set(key, "value");

        //Act
        SettingsService.Set(key, null);

        //Assert
        SettingsService.HasValue(key).Should().BeFalse();
    }
}
