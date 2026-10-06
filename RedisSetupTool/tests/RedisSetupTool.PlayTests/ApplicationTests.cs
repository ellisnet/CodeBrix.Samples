using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using RedisSetupTool.ViewModels;
using RedisSetupTool.Views;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private Locator Refresh => Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Refresh" }).First;
    // List rows are buttons whose text includes the row's name.
    private Locator Row(string text) => Page.GetByRole(AriaRole.Button).Filter(new() { HasText = text });

    private async Task AnswerDialogAsync(string button)
    {
        await Dialog.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    private Task WaitUntilIdleAsync() => Fixture.Application.WaitForAsync(() => Fixture.Model.IsBusy, busy => !busy,
        description: "refresh finished");

    [Fact]
    public async Task Dashboard_renders_a_daemon_snapshot()
    {
        await Expect(Page.GetByText("Docker 29.0-fixture · API 1.52", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.State.Containers.Count)).Should().Be(2);
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
        await SnapshotAsync("RedisSetupTool-dashboard");
    }

    [Theory]
    [InlineData("Redis instances", SectionKey.Instances, "No Redis instances yet. Create one from the topology catalog.")]
    [InlineData("New instance", SectionKey.CreateInstance, "Topology catalog")]
    [InlineData("Containers", SectionKey.Containers, "Select a container on the left.")]
    [InlineData("Consoles", SectionKey.Consoles, "No consoles are open.")]
    [InlineData("Images", SectionKey.Images, "Select an image on the left.")]
    [InlineData("Networks & volumes", SectionKey.NetworksVolumes, "Networks and volumes")]
    [InlineData("System", SectionKey.System, "SWEEP")]
    public async Task Navigation_rail_opens_each_section(string name, SectionKey expected, string heading)
    {
        await Button(name).ClickAsync();
        await Expect(Page.GetByText(heading, new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("RECENT ACTIVITY", new() { Exact = true })).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentSection)).Should().Be(expected);
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
        await SnapshotAsync("RedisSetupTool-" + expected);
    }

    [Fact]
    public async Task Dashboard_cards_open_their_sections()
    {
        var cards = new (string Card, string Heading)[]
        {
            ("^REDIS INSTANCES ", "No Redis instances yet. Create one from the topology catalog."),
            ("^CONTAINERS ", "Select a container on the left."),
            ("^IMAGES ", "Select an image on the left."),
            ("^VOLUMES ", "Networks and volumes"),
        };
        foreach (var (card, heading) in cards)
        {
            await Button("Dashboard").ClickAsync();
            await Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new(card) }).ClickAsync();
            await Expect(Page.GetByText(heading, new() { Exact = true })).ToBeVisibleAsync();
        }
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Container_filters_update_the_visible_rows()
    {
        await Button("Containers").ClickAsync();
        await Page.GetByTestId("SearchText2").FillAsync("cache");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Containers.Rows.Count, count => count == 1);
        await Expect(Page.GetByText("cache-primary", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByTestId("SearchText2").FillAsync("");
        await Page.GetByTestId("Filter").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Running only", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Containers.Rows.Count, count => count == 1);
        await Expect(Page.GetByText("worker-stopped", new() { Exact = true })).ToHaveCountAsync(0);
        await Page.GetByTestId("SearchText2").FillAsync("missing");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Containers.Rows.Count, count => count == 0);
    }

    [Fact]
    public async Task Console_picker_can_be_filtered_and_cancelled()
    {
        await Button("Consoles").ClickAsync();
        await Button("Open a console").First.ClickAsync();
        await Page.GetByTestId("PickerSearchText").FillAsync("missing");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Consoles.PickerRows.Count, count => count == 0);
        await Button("Cancel").ClickAsync();
        await Expect(Page.GetByTestId("PickerSearchText")).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.IsPickerOpen)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.Tabs.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Refresh_reports_an_unreachable_daemon_and_recovers()
    {
        Fixture.Docker.Reachable = false;
        await Refresh.ClickAsync();
        await Expect(Page.GetByText("daemon unreachable", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.Reachable = true;
        await Refresh.ClickAsync();
        await Expect(Page.GetByText("Docker 29.0-fixture · API 1.52", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_preview_keeps_the_navigation_rail_usable()
    {
        await Button("Images").ClickAsync();
        await Page.GetByTestId("SearchText3").FillAsync("redis");
        await Expect(Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "redis:8-alpine" })).ToBeVisibleAsync();
        await SnapshotAsync("RedisSetupTool-portrait");
    }
}
