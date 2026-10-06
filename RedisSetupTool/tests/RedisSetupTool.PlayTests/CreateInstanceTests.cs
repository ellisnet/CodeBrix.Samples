using System;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using RedisSetupTool.ViewModels;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private Locator InstanceName => Page.GetByTestId("InstanceName");
    private Locator Create => Button("Create instance");

    private async Task OpenCreateFormAsync(string name)
    {
        await Button("New instance").ClickAsync();
        await Expect(Page.GetByText("Plain standalone", new() { Exact = true }).First).ToBeVisibleAsync();
        // The form suggests a random name; every test types its own.
        await InstanceName.FillAsync(name);
    }

    [Fact]
    public async Task Invalid_instance_name_disables_create_with_a_message()
    {
        await OpenCreateFormAsync("-not a name");
        await Expect(Page.GetByText("NOT READY YET", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("The name may hold only letters, digits, dots, dashes and underscores, must start with a letter or digit, and must be 63 characters or fewer.",
            new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Create).ToBeDisabledAsync();
        await InstanceName.FillAsync("valid-name");
        await Expect(Page.GetByText("NOT READY YET", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(Create).ToBeEnabledAsync();
        Fixture.Topologies.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_shows_progress_then_lands_on_instances()
    {
        await OpenCreateFormAsync("playtest-cache");
        await Create.ClickAsync();
        await Expect(Page.GetByText("1/2  Reserving fixture ports", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Create).ToBeDisabledAsync();
        Fixture.Topologies.Calls.Should().Equal("CreateAsync A1 playtest-cache");
        Fixture.Topologies.CreateGate.SetResult();
        await Expect(Page.GetByText("playtest-cache", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Sweep all")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentSection)).Should().Be(SectionKey.Instances);
        await Expect(Page.GetByText("1 of 1 up", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Create_failure_shows_the_error_and_reenables_create()
    {
        Fixture.Topologies.CreateFailure = new InvalidOperationException("Fixture port 16379 is already allocated.");
        await OpenCreateFormAsync("playtest-fail");
        await Create.ClickAsync();
        await Expect(Create).ToBeDisabledAsync();
        Fixture.Topologies.CreateGate.SetResult();
        await Expect(Page.GetByText("Fixture port 16379 is already allocated.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Failed.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Create).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentSection)).Should().Be(SectionKey.CreateInstance);
        Fixture.Topologies.Instances.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancel_during_create_stops_the_request_and_keeps_the_form()
    {
        await OpenCreateFormAsync("playtest-cancel");
        await Create.ClickAsync();
        await Expect(Page.GetByText("1/2  Reserving fixture ports", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Cancel").ClickAsync();
        await Expect(Page.GetByText("Cancelled.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Create).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentSection)).Should().Be(SectionKey.CreateInstance);
        Fixture.Topologies.Instances.Should().BeEmpty();
    }
}
