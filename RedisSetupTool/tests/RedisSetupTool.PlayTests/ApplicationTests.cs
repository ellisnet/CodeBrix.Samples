using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using RedisSetupTool.ViewModels;
using RedisSetupTool.Views;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    [Fact]
    public async Task Dashboard_renders_a_daemon_snapshot()
    {
        await Expect(Page.GetByText("Docker 29.0-fixture · API 1.52", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.State.Containers.Count)).Should().Be(2);
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
        await SnapshotAsync("RedisSetupTool-dashboard");
    }

    [Theory]
    [InlineData("Redis instances", SectionKey.Instances)]
    [InlineData("New instance", SectionKey.CreateInstance)]
    [InlineData("Containers", SectionKey.Containers)]
    [InlineData("Consoles", SectionKey.Consoles)]
    [InlineData("Images", SectionKey.Images)]
    [InlineData("Networks & volumes", SectionKey.NetworksVolumes)]
    [InlineData("System", SectionKey.System)]
    public async Task Navigation_rail_opens_each_section(string name, SectionKey expected)
    {
        await Button(name).ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentSection)).Should().Be(expected);
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
        await SnapshotAsync("RedisSetupTool-" + expected);
    }

    [Fact]
    public async Task Container_filters_update_the_visible_rows()
    {
        await Button("Containers").ClickAsync();
        await Page.GetByTestId("SearchText2").FillAsync("cache");
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Rows.Count)).Should().Be(1);
        await Expect(Page.GetByText("cache-primary", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByTestId("SearchText2").FillAsync("");
        await Page.GetByTestId("Filter").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Running only", Exact = true }).ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Rows.Count)).Should().Be(1);
        await Page.GetByTestId("SearchText2").FillAsync("missing");
        (await Page.EvaluateAsync(() => Fixture.Model.Containers.Rows.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Console_picker_can_be_filtered_and_cancelled()
    {
        await Button("Consoles").ClickAsync();
        await Button("Open a console").First.ClickAsync();
        await Page.GetByTestId("PickerSearchText").FillAsync("missing");
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.PickerRows.Count)).Should().Be(0);
        await Button("Cancel").ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.IsPickerOpen)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.Tabs.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Refresh_reports_an_unreachable_daemon_and_recovers()
    {
        Fixture.Docker.Reachable = false;
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Refresh" }).First.ClickAsync();
        await Expect(Page.GetByText("daemon unreachable", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.Reachable = true;
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Refresh" }).First.ClickAsync();
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
