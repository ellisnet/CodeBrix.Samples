using System.Threading.Tasks;
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
}
