using System;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Foundation.Extensibility;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using MediaPlayerDemo.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using Windows.Media.Core;
using Xunit;

namespace MediaPlayerDemo.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string Address = "https://example.invalid/a.mp4";

    private Locator AddressBox => Page.GetByTestId("MediaAddress");
    private Locator Status => Page.GetByTestId("Status");
    private Locator StretchPicker => Page.GetByTestId("Stretch");

    [Fact]
    public async Task Startup_loads_default_address_and_reports_it()
    {
        await Expect(AddressBox).ToHaveValueAsync(AppFixture.DefaultAddress);
        await Expect(Status).ToHaveTextAsync("Loaded: " + AppFixture.DefaultAddress);
        await Expect(Button("Load")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => ((MediaSource)Fixture.Model.PlayerSource).Uri))
            .Should().Be(new Uri(AppFixture.DefaultAddress));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Blank_address_disables_load(string blank)
    {
        await AddressBox.FillAsync(blank);
        await Expect(Button("Load")).ToBeDisabledAsync();
        await AddressBox.FillAsync(Address);
        await Expect(Button("Load")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Valid_address_loads_and_reports_status()
    {
        await AddressBox.FillAsync(Address);
        await Expect(Status).ToHaveTextAsync("Loaded: " + AppFixture.DefaultAddress);
        await Button("Load").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Loaded: " + Address);
    }

    [Fact]
    public async Task Invalid_address_reports_error_and_keeps_previous_source()
    {
        var previous = await Page.EvaluateAsync(() => Fixture.Model.PlayerSource);
        await AddressBox.FillAsync("not a uri");
        await Button("Load").ClickAsync();
        await Expect(Status).ToContainTextAsync("Cannot load 'not a uri':");
        (await Page.EvaluateAsync(() => ReferenceEquals(Fixture.Model.PlayerSource, previous))).Should().BeTrue();
        (await Page.EvaluateAsync(() => ((MediaSource)Fixture.Model.PlayerSource).Uri))
            .Should().Be(new Uri(AppFixture.DefaultAddress));
    }

    [Fact]
    public async Task Loading_new_address_replaces_player_source()
    {
        var previous = await Page.EvaluateAsync(() => Fixture.Model.PlayerSource);
        await AddressBox.FillAsync(Address);
        await Button("Load").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Loaded: " + Address);
        await Fixture.Application.WaitForAsync(() => Player().Source, source => !ReferenceEquals(source, previous),
            description: "the player element's new source");
        (await Page.EvaluateAsync(() => ((MediaSource)Fixture.Model.PlayerSource).Uri)).Should().Be(new Uri(Address));
        (await Page.EvaluateAsync(() => ((MediaSource)Player().Source).Uri)).Should().Be(new Uri(Address));
        (await Page.EvaluateAsync(() => ReferenceEquals(Player().Source, Fixture.Model.PlayerSource))).Should().BeTrue();
    }

    [Fact]
    public async Task Load_hands_uri_to_engine_and_autoplays()
    {
        await AddressBox.FillAsync(Address);
        await Button("Load").ClickAsync();
        var loaded = await Fixture.Application.WaitForAsync(() => Fixture.Engine.Loaded.ToArray(),
            calls => calls.Any(call => call.Address == new Uri(Address)), description: "the engine's new source");
        loaded.Select(call => call.Address).Should().Equal(new Uri(AppFixture.DefaultAddress), new Uri(Address));
        // By the time Load is clicked the element has handed its AutoPlay setting to its player.
        loaded[^1].AutoPlay.Should().BeTrue();
    }

    [Fact]
    public async Task Stretch_picker_offers_four_modes_and_defaults_to_uniform()
    {
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedStretch)).Should().Be(Stretch.Uniform);
        await StretchPicker.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Option)).ToHaveCountAsync(4);
        foreach (var name in new[] { "Uniform", "UniformToFill", "Fill", "None" })
        {
            await Expect(Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true })).ToBeVisibleAsync();
        }
        await Page.GetByRole(AriaRole.Option, new() { Name = "Uniform", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Option)).ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedStretch)).Should().Be(Stretch.Uniform);
    }

    [Theory]
    [InlineData("Fill", Stretch.Fill)]
    [InlineData("UniformToFill", Stretch.UniformToFill)]
    [InlineData("None", Stretch.None)]
    public async Task Choosing_stretch_mode_updates_player_element(string name, Stretch expected)
    {
        await StretchPicker.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Player().Stretch, stretch => stretch == expected,
            description: "the player element's stretch");
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedStretch)).Should().Be(expected);
    }

    [Fact]
    public async Task Player_element_and_transport_controls_render_without_the_video_engine()
    {
        // The libVLC add-in's video presenter is never registered: only the fixture's recording engine exists.
        (await Page.EvaluateAsync(() => ApiExtensibility.IsRegistered<IMediaPlayerPresenterExtension>())).Should().BeFalse();
        await Expect(Page.GetByTestId("Player")).ToBeVisibleAsync();
        await Expect(Page.GetByType<MediaTransportControls>()).ToBeVisibleAsync();
        (await Page.GetByType<MediaTransportControls>().GetByRole(AriaRole.Button).CountAsync()).Should().BeGreaterThan(0);
        await SnapshotAsync("MediaPlayerDemo-player");
    }

    private MediaPlayerElement Player() => FindPlayer(Fixture.View);

    private static MediaPlayerElement FindPlayer(DependencyObject root)
    {
        if (root is MediaPlayerElement player) return player;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            if (FindPlayer(VisualTreeHelper.GetChild(root, i)) is { } found) return found;
        }
        return null;
    }
}
