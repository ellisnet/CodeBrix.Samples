using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using RedisSetupTool.DockerManagement.Models;
using RedisSetupTool.ViewModels;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private async Task SelectContainerAsync(string name)
    {
        await Button("Containers").ClickAsync();
        await Row(name).ClickAsync();
        await Expect(Page.GetByText("CONTAINER", new() { Exact = true })).ToBeVisibleAsync();
    }

    private Task SelectTabAsync(string tab) => Page.GetByRole(AriaRole.Button, new() { Name = tab, Exact = true }).ClickAsync();

    [Fact]
    public async Task Selecting_a_container_shows_overview_and_switches_detail_tabs()
    {
        await SelectContainerAsync("cache-primary");
        await Expect(Page.GetByText("redis:8-alpine · running", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Show environment (1)")).ToBeVisibleAsync();
        await SelectTabAsync("Diagnostics");
        await Expect(Page.GetByText("Fixture diagnostics: nothing throttled.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Show environment (1)")).ToBeHiddenAsync();
        await SelectTabAsync("Advisor");
        await Expect(Page.GetByText("The advisor has nothing to say about this container.", new() { Exact = true })).ToBeVisibleAsync();
        await SelectTabAsync("Overview");
        await Expect(Button("Show environment (1)")).ToBeVisibleAsync();
        await Button("Show environment (1)").ClickAsync();
        await Expect(Button("Hide environment (1)")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Detail.OverviewFacts.Any(f => f.Value == "redis-server, --appendonly, yes"))).Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Detail.Tab)).Should().Be(ContainerTab.Overview);
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
        await SnapshotAsync("RedisSetupTool-container-detail");
    }

    [Fact]
    public async Task Stop_and_start_call_the_daemon_and_refresh_the_row()
    {
        await SelectContainerAsync("cache-primary");
        await Expect(Button("Start")).ToBeDisabledAsync();
        await Button("Stop").ClickAsync();
        await Expect(Row("cache-primary")).ToContainTextAsync("Exited (0)");
        Fixture.Docker.Called("StopContainerAsync " + DockerFixture.RunningId).Should().BeTrue();
        await Expect(Button("Stop")).ToBeDisabledAsync();
        await Button("Start").ClickAsync();
        await Expect(Row("cache-primary")).ToContainTextAsync("Up 5 minutes");
        Fixture.Docker.Called("StartContainerAsync " + DockerFixture.RunningId).Should().BeTrue();
        await Expect(Button("Stop")).ToBeEnabledAsync();
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Remove_container_declined_in_dialog_makes_no_call()
    {
        await SelectContainerAsync("cache-primary");
        await Button("Remove").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Remove cache-primary? Its writable layer goes with it.");
        await AnswerDialogAsync("No");
        await Expect(Button("Remove")).ToBeEnabledAsync();
        Fixture.Docker.Calls.Should().BeEmpty();
        await Expect(Row("cache-primary")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Copy_id_places_the_container_id_on_the_isolated_clipboard()
    {
        await SelectContainerAsync("cache-primary");
        await Button("Copy id").ClickAsync();
        (await Page.ClipboardTextAsync()).Should().Be(DockerFixture.RunningId);
    }

    [Fact]
    public async Task Logs_tab_loads_tail_and_honours_tail_choice()
    {
        await SelectContainerAsync("cache-primary");
        await SelectTabAsync("Logs");
        await Expect(Page.GetByText("Ready to accept connections (tail 500)", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByTestId("LogTail").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "100 lines", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Ready to accept connections (tail 100)", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Switch).Filter(new() { HasText = "Timestamps" }).CheckAsync();
        await Expect(Page.GetByText("2099-01-01T00:00:00Z Ready to accept connections (tail 100)", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.Called("GetLogsAsync " + DockerFixture.RunningId + " tail=100 timestamps=True").Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Detail.LogAutoRefresh)).Should().BeFalse();
    }

    [Fact]
    public async Task Stats_tab_shows_streamed_samples()
    {
        await SelectContainerAsync("cache-primary");
        await SelectTabAsync("Stats");
        Fixture.Docker.EmitStats(new ContainerStatsSample { CpuPercent = 12.5, MemoryUsageBytes = 1024 * 1024, MemoryLimitBytes = 4 * 1024 * 1024, MemoryPercent = 25 });
        await Expect(Page.GetByText("12.5%", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.EmitStats(new ContainerStatsSample { CpuPercent = 40, MemoryUsageBytes = 2 * 1024 * 1024, MemoryLimitBytes = 4 * 1024 * 1024, MemoryPercent = 50 });
        await Expect(Page.GetByText("40.0%", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Containers.Detail.CpuHistory.Count, count => count == 2,
            description: "two sparkline bars");
    }

    [Fact]
    public async Task Kill_sends_the_selected_signal()
    {
        await SelectContainerAsync("cache-primary");
        await Page.GetByTestId("KillSignal").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "SIGTERM", Exact = true }).ClickAsync();
        await Button("Kill").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Docker.Called("KillContainerAsync " + DockerFixture.RunningId + " SIGTERM"),
            called => called, description: "kill with SIGTERM");
        await Expect(Row("cache-primary")).ToContainTextAsync("Exited (0)");
    }
}
