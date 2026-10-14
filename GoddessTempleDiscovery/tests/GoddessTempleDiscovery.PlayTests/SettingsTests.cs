using System.Threading.Tasks;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Settings;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Settings_open_from_the_title_and_show_the_stored_preferences()
    {
        await Id("Settings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        (await Id("SoundEnabled").IsCheckedAsync()).Should().Be(SettingsService.SoundEnabled);
        (await Id("ReducedMotion").IsCheckedAsync()).Should().Be(SettingsService.ReducedMotion);
        (await Id("RevealComputerDiscoveries").IsCheckedAsync()).Should().Be(SettingsService.RevealComputerDiscoveries);
        (await ReadAsync(() => Model.SelectedSpeed)).Should().Be("2×");
        await Id("CloseSettings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeHiddenAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Sound_motion_speed_and_reveal_toggles_bind_persist_and_reach_the_table()
    {
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        await Id("PlayMenu").ClickAsync();
        await Id("Settings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        try
        {
            await Id("SoundEnabled").SetCheckedAsync(!AppFixture.SeededSound);
            await Id("ReducedMotion").SetCheckedAsync(!AppFixture.SeededReducedMotion);
            await Id("RevealComputerDiscoveries").SetCheckedAsync(!AppFixture.SeededReveal);
            await Id("AnimationSpeed").SelectOptionAsync("3×");
            (await ReadAsync(() => (Model.SoundEnabled, Model.ReducedMotion, Model.RevealComputerDiscoveries, Model.SelectedSpeed)))
                .Should().Be((!AppFixture.SeededSound, !AppFixture.SeededReducedMotion, !AppFixture.SeededReveal, "3×"));
            (SettingsService.SoundEnabled, SettingsService.ReducedMotion, SettingsService.RevealComputerDiscoveries, SettingsService.AnimationSpeed)
                .Should().Be((!AppFixture.SeededSound, !AppFixture.SeededReducedMotion, !AppFixture.SeededReveal, 3.0));
            // The host applies them to the table on the engine thread.
            await WaitOnEngineAsync(() => (Session.Table.SoundEnabled, Session.Table.ReducedMotion, Session.Table.AnimationSpeed),
                table => table == (!AppFixture.SeededSound, !AppFixture.SeededReducedMotion, 3f), "the table's new settings");

            // Persisted: the store, closed and opened again, reads the new values back.
            var reopened = await ReadAsync(() =>
            {
                SettingsService.Shutdown();
                SettingsService.Initialize(_fixture.SettingsDirectory);
                return (SettingsService.SoundEnabled, SettingsService.ReducedMotion, SettingsService.RevealComputerDiscoveries, SettingsService.AnimationSpeed);
            });
            reopened.Should().Be((!AppFixture.SeededSound, !AppFixture.SeededReducedMotion, !AppFixture.SeededReveal, 3.0));
        }
        finally
        {
            // Back to the returning player's choices, through the same switches.
            await Id("SoundEnabled").SetCheckedAsync(AppFixture.SeededSound);
            await Id("ReducedMotion").SetCheckedAsync(AppFixture.SeededReducedMotion);
            await Id("RevealComputerDiscoveries").SetCheckedAsync(AppFixture.SeededReveal);
            await Id("AnimationSpeed").SelectOptionAsync("2×");
        }

        (SettingsService.SoundEnabled, SettingsService.ReducedMotion, SettingsService.RevealComputerDiscoveries, SettingsService.AnimationSpeed)
            .Should().Be((AppFixture.SeededSound, AppFixture.SeededReducedMotion, AppFixture.SeededReveal, AppFixture.SeededSpeed));
        await Id("CloseSettings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Headline_settings_persist_and_drive_the_countdown_bar_and_keep_reading()
    {
        await Id("Settings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        (await Id("HumanHeadlinesAutoDismiss").IsCheckedAsync()).Should().BeTrue();
        (await Id("ComputerHeadlinesAutoDismiss").IsCheckedAsync()).Should().BeTrue();
        await Expect(Id("HumanHeadlinesText")).ToHaveTextAsync("Closes by itself after 30 seconds");
        await Expect(Id("ComputerHeadlinesText")).ToHaveTextAsync("Closes by itself after 10 seconds");
        try
        {
            // Each pair stands alone; the seconds stay within 1 to 120.
            await Id("ComputerHeadlinesAutoDismiss").SetCheckedAsync(false);
            await Expect(Id("ComputerHeadlinesText")).ToHaveTextAsync("Stays open until closed");
            await ReadAsync(() => Model.HumanHeadlinesSeconds = 500);
            (await ReadAsync(() => Model.HumanHeadlinesSeconds)).Should().Be(120);
            (SettingsService.HumanHeadlinesAutoDismiss, SettingsService.HumanHeadlinesSeconds, SettingsService.ComputerHeadlinesAutoDismiss)
                .Should().Be((true, 120, false));

            // The first season's front page counts as the human team's: it counts down from 6 seconds with a bar, and
            // KEEP READING stops the countdown so it stays past its time.
            await ReadAsync(() => Model.HumanHeadlinesSeconds = 6);
            await Id("CloseSettings").ClickAsync();
            await OpenSetupAsync();
            await SetSeatCountAsync(4);
            for (var seat = 0; seat < 4; seat++)
            {
                await Id("SeatComputer").Nth(seat).SetCheckedAsync(seat >= 1);
            }

            await Id("Seed").FillAsync(DefaultSeed.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await Id("Start").ClickAsync();
            await Expect(Id("ProloguePane")).ToBeVisibleAsync();
            await Id("Begin").ClickAsync();
            await WaitAsync(() => Model.IsInspectorOpen && Model.InspectorKind == CardKind.Season, open => open, "the first season's front page", 120000);
            await Expect(Id("InspectorCountdown")).ToBeVisibleAsync();
            await Expect(Id("KeepReading")).ToBeVisibleAsync();
            await Expect(Id("InspectorCountdownText")).ToContainTextAsync("Closes by itself in");
            await Id("KeepReading").ClickAsync();
            await Expect(Id("InspectorCountdown")).ToBeHiddenAsync();
            await Expect(Id("KeepReading")).ToBeHiddenAsync();
            await Task.Delay(7000, TestContext.Current.CancellationToken);
            (await ReadAsync(() => Model.IsInspectorOpen)).Should().BeTrue();
            await CloseInspectorAsync();
        }
        finally
        {
            await ReadAsync(() =>
            {
                Model.HumanHeadlinesSeconds = SettingsService.DefaultHumanHeadlineSeconds;
                Model.ComputerHeadlinesAutoDismiss = true;
                return true;
            });
        }

        (SettingsService.HumanHeadlinesAutoDismiss, SettingsService.HumanHeadlinesSeconds, SettingsService.ComputerHeadlinesAutoDismiss,
            SettingsService.ComputerHeadlinesSeconds).Should().Be((true, 30, true, 10));
    }
}
