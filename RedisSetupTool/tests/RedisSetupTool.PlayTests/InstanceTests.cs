using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using RedisSetupTool.DockerManagement.Topologies;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private const string Secret = "fixture-secret-42";

    private async Task ShowInstancesAsync(params TopologyInstance[] instances)
    {
        Fixture.Topologies.Instances.AddRange(instances);
        await Refresh.ClickAsync();
        await WaitUntilIdleAsync();
        await Button("Redis instances").ClickAsync();
    }

    [Fact]
    public async Task Discovered_instance_renders_a_card_with_endpoints()
    {
        await ShowInstancesAsync(TopologyFixture.Instance("inst-a", "cache-a", true));
        await Expect(Page.GetByText("cache-a", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("1 of 1 up", new() { Exact = true })).ToBeVisibleAsync();
        // The endpoint row and the connection-string row carry the same text here.
        await Expect(Page.GetByText("127.0.0.1:16379", new() { Exact = true })).ToHaveCountAsync(2);
        await Expect(Page.GetByText("redis-cli -p 16379", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Verify")).ToBeEnabledAsync();
        await Expect(Button("Start")).ToBeDisabledAsync();
        await Button("Copy all").ClickAsync();
        (await Page.ClipboardTextAsync()).Should().Be("127.0.0.1:16379");
        await SnapshotAsync("RedisSetupTool-instance-card");
    }

    [Fact]
    public async Task Verify_shows_the_probe_result_on_the_card()
    {
        await ShowInstancesAsync(TopologyFixture.Instance("inst-a", "cache-a", true));
        await Button("Verify").ClickAsync();
        await Expect(Page.GetByText("Fixture verification passed.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("PONG", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Probe.LastVerified.Endpoints[0].Port.Should().Be(16379);
        Fixture.Probe.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Reveal_toggles_a_masked_password_and_copy_uses_the_secret()
    {
        await ShowInstancesAsync(TopologyFixture.Instance("inst-b", "secure-b", true, Secret));
        await Expect(Page.GetByText(new string('•', 12), new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText(Secret, new() { Exact = true })).ToHaveCountAsync(0);
        await Button("show").ClickAsync();
        await Expect(Page.GetByText(Secret, new() { Exact = true })).ToBeVisibleAsync();
        await Button("hide").ClickAsync();
        await Expect(Page.GetByText(Secret, new() { Exact = true })).ToHaveCountAsync(0);
        // Rows: endpoint, password, connection string, redis-cli; the password row's copy is second.
        await Button("Copy to clipboard").Nth(1).ClickAsync();
        (await Page.ClipboardTextAsync()).Should().Be(Secret);
    }

    [Fact]
    public async Task Destroy_instance_confirmed_calls_the_topology_service()
    {
        await ShowInstancesAsync(TopologyFixture.Instance("inst-a", "cache-a", true));
        await Button("Destroy").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Destroy cache-a? This removes 1 container, 1 volume and 1 network.");
        await AnswerDialogAsync("Yes");
        await Expect(Page.GetByText("No Redis instances yet. Create one from the topology catalog.", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Topologies.Calls.Should().Equal("DestroyAsync inst-a");
    }

    [Fact]
    public async Task Instance_filters_narrow_cards()
    {
        await ShowInstancesAsync(TopologyFixture.Instance("inst-a", "cache-a", true),
            TopologyFixture.Instance("inst-b", "secure-b", false, Secret));
        await Page.GetByTestId("StateFilter").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Stopped", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("1 of 2 instances", new() { Exact = true })).ToBeVisibleAsync();
        // GetByText also matches card elements the repeater keeps for reuse, so read the bound cards.
        await Fixture.Application.WaitForAsync(() => string.Join(",", Fixture.Model.Instances.Instances.Select(c => c.InstanceName)),
            names => names == "secure-b", description: "stopped cards");
        await Button("Clear").ClickAsync();
        await Page.GetByTestId("SearchText").FillAsync("cache");
        await Expect(Page.GetByText("1 of 2 instances", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => string.Join(",", Fixture.Model.Instances.Instances.Select(c => c.InstanceName)),
            names => names == "cache-a", description: "cards matching the search");
        await Page.GetByTestId("SearchText").FillAsync("nothing-matches");
        await Expect(Page.GetByText("No instance matches the current filters.", new() { Exact = true })).ToBeVisibleAsync();
    }
}
