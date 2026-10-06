using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using RedisSetupTool.DockerManagement.Models;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private static DaemonEvent Event(string type, string action, string subject) =>
        new() { Type = type, Action = action, ActorName = subject };

    // GetByText also matches the hidden Dashboard's copy of each event row, which comes
    // earlier in the tree; the System section's row is the last match.
    private Locator SystemEvent(string text) => Page.GetByText(text, new() { Exact = true }).Last;
    private Task<int> SystemEventCountAsync() => Page.EvaluateAsync(() => Fixture.Model.System.Events.Count);

    [Fact]
    public async Task Create_and_remove_network_and_volume()
    {
        await Button("Networks & volumes").ClickAsync();
        await Page.GetByTestId("NewNetworkName").FillAsync("playtest-net");
        await Page.GetByTestId("CreateNetwork").ClickAsync();
        await Expect(Row("playtest-net")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("NewNetworkName")).ToHaveValueAsync("");
        await Row("playtest-net").ClickAsync();
        await Page.GetByTestId("RemoveNetwork").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Remove the network playtest-net?");
        await AnswerDialogAsync("Yes");
        await Expect(Row("playtest-net")).ToHaveCountAsync(0);

        await Page.GetByTestId("NewVolumeName").FillAsync("playtest-data");
        await Page.GetByTestId("CreateVolume").ClickAsync();
        await Expect(Row("playtest-data")).ToBeVisibleAsync();
        await Row("playtest-data").ClickAsync();
        await Page.GetByTestId("RemoveVolume").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Remove the volume playtest-data? Its contents go with it.");
        await AnswerDialogAsync("Yes");
        await Expect(Row("playtest-data")).ToHaveCountAsync(0);

        Fixture.Docker.Calls.Should().Equal("CreateNetworkAsync playtest-net", "RemoveNetworkAsync playtest-net",
            "CreateVolumeAsync playtest-data", "RemoveVolumeAsync playtest-data");
    }

    [Theory]
    [InlineData("Prune containers", "PruneContainersAsync")]
    [InlineData("Prune images", "PruneImagesAsync True")]
    [InlineData("Prune networks", "PruneNetworksAsync")]
    [InlineData("Prune volumes", "PruneVolumesAsync")]
    public async Task Prune_buttons_confirm_before_calling(string button, string call)
    {
        await Button("System").ClickAsync();
        await Button(button).ClickAsync();
        await Expect(Dialog).ToContainTextAsync(button);
        await AnswerDialogAsync("No");
        await Expect(Button(button)).ToBeEnabledAsync();
        Fixture.Docker.Calls.Should().BeEmpty();
        await Button(button).ClickAsync();
        await AnswerDialogAsync("Yes");
        await Fixture.Application.WaitForAsync(() => Fixture.Docker.Calls.Length, count => count == 1, description: call);
        Fixture.Docker.Calls.Should().Equal(call);
        await WaitUntilIdleAsync();
    }

    [Fact]
    public async Task Event_stream_fills_system_log_and_filter_narrows_it()
    {
        await Button("System").ClickAsync();
        await Expect(Page.GetByText("waiting for events…", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.EmitEvent(Event("container", "start", "cache-primary"));
        await Expect(Page.GetByText("1 event since the app started", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(SystemEvent("start")).ToBeVisibleAsync();

        await Page.GetByTestId("EventFilter").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "image", Exact = true }).ClickAsync();
        Fixture.Docker.EmitEvent(Event("container", "die", "cache-primary"));
        Fixture.Docker.EmitEvent(Event("image", "pull", "valkey:8"));
        await Expect(SystemEvent("valkey:8")).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 events since the app started", new() { Exact = true })).ToBeVisibleAsync();
        // The dashboard's activity strip is unfiltered and received all three.
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Dashboard.RecentEvents.Count, count => count == 3,
            description: "dashboard events");
        (await SystemEventCountAsync()).Should().Be(2);
        await Expect(SystemEvent("die")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Pause_stops_new_events_and_clear_empties_log()
    {
        await Button("System").ClickAsync();
        Fixture.Docker.EmitEvent(Event("container", "start", "cache-primary"));
        await Expect(Page.GetByText("1 event since the app started", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Pause stream").ClickAsync();
        await Expect(Button("Resume stream")).ToBeVisibleAsync();
        Fixture.Docker.EmitEvent(Event("container", "stop", "cache-primary"));
        // The dashboard proves the paused event arrived; the System log must ignore it.
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Dashboard.RecentEvents.Count, count => count == 2,
            description: "dashboard events");
        (await SystemEventCountAsync()).Should().Be(1);
        await Expect(SystemEvent("stop")).ToBeHiddenAsync();
        await Expect(Page.GetByText("1 event since the app started", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Clear").ClickAsync();
        await Expect(Page.GetByText("cleared", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(SystemEvent("start")).ToBeHiddenAsync();
        (await SystemEventCountAsync()).Should().Be(0);
        await Button("Resume stream").ClickAsync();
        Fixture.Docker.EmitEvent(Event("network", "create", "playtest-net"));
        await Expect(Page.GetByText("1 event since the app started", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Sweep_with_no_instances_shows_information()
    {
        await Button("System").ClickAsync();
        await Button("Sweep RedisSetupTool resources").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("There is nothing to sweep — no instances were found.");
        await AnswerDialogAsync("OK");
        Fixture.Topologies.Calls.Should().BeEmpty();
    }
}
