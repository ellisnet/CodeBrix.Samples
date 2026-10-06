using System;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private async Task SelectImageAsync(string name)
    {
        await Row(name).ClickAsync();
        await Expect(Page.GetByText("LAYERS", new() { Exact = true })).ToBeVisibleAsync();
        // Fact rows hold a hidden twin of each value for the other font, so wait on the bound layers.
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Images.Layers.Count, count => count == 1, description: "image layers");
    }

    [Fact]
    public async Task Pull_tag_remove_image_round_trip()
    {
        await Button("Images").ClickAsync();
        // Tool output shows in the detail pane, so an image is selected first.
        await SelectImageAsync("redis:8-alpine");
        await Expect(Button("Pull image")).ToBeDisabledAsync();
        await Page.GetByTestId("PullReference").FillAsync("valkey:8");
        await Button("Pull image").ClickAsync();
        await Expect(Row("valkey:8")).ToBeVisibleAsync();
        await Expect(Page.GetByText("fixture layer: Pull complete", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Docker.Called("PullImageAsync valkey:8").Should().BeTrue();

        await SelectImageAsync("valkey:8");
        await Page.GetByTestId("NewTag").FillAsync("valkey:latest");
        await Button("Tag").ClickAsync();
        await Expect(Row("valkey:latest")).ToBeVisibleAsync();
        Fixture.Docker.Called("TagImageAsync valkey:8 valkey:latest").Should().BeTrue();

        await Button("Remove").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Remove valkey:8?");
        await AnswerDialogAsync("Yes");
        await Expect(Row("valkey:8")).ToHaveCountAsync(0);
        await Expect(Row("valkey:latest")).ToBeVisibleAsync();
        Fixture.Docker.Called("RemoveImageAsync valkey:8").Should().BeTrue();
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Failed_pull_reports_the_failure_instead_of_done()
    {
        await Button("Images").ClickAsync();
        // Tool output shows in the detail pane, so an image is selected first.
        await SelectImageAsync("redis:8-alpine");
        Fixture.Docker.PullFailure = new InvalidOperationException("pull access denied for nosuch:1");
        await Page.GetByTestId("PullReference").FillAsync("nosuch:1");
        await Button("Pull image").ClickAsync();

        await Fixture.Application.WaitForAsync(() => Fixture.Model.Images.ToolOutput.Contains("failed: pull access denied for nosuch:1"),
            failed => failed, description: "failed pull outcome");
        (await Page.EvaluateAsync(() => Fixture.Model.Images.ToolOutput.Contains("done."))).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Images.ErrorText)).Should().Be("pull access denied for nosuch:1");
        await Expect(Page.GetByText("failed: pull access denied for nosuch:1", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Row("nosuch:1")).ToHaveCountAsync(0);

        Fixture.Docker.PullFailure = null;
        await Button("Pull image").ClickAsync();
        await Expect(Row("nosuch:1")).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Images.ToolOutput.Contains("done."), done => done, description: "successful pull outcome");
        (await Page.EvaluateAsync(() => Fixture.Model.Images.ToolOutput.Any(line => line.StartsWith("failed:")))).Should().BeFalse();
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Pull_without_a_selected_image_shows_its_output_and_errors()
    {
        var hint = Page.GetByText("Select an image on the left.", new() { Exact = true });
        await Button("Images").ClickAsync();
        await Expect(hint).ToBeVisibleAsync();

        // A failed pull: nothing is selected, and the failure still shows.
        Fixture.Docker.PullFailure = new InvalidOperationException("pull access denied for nosuch:1");
        await Page.GetByTestId("PullReference").FillAsync("nosuch:1");
        await Button("Pull image").ClickAsync();
        await Expect(Page.GetByText("failed: pull access denied for nosuch:1", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("pull access denied for nosuch:1", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(hint).ToBeHiddenAsync();

        // A pull that works: its progress and outcome show, and the image's own tools stay out of the pane.
        Fixture.Docker.PullFailure = null;
        await Page.GetByTestId("PullReference").FillAsync("valkey:8");
        await Button("Pull image").ClickAsync();
        await Expect(Row("valkey:8")).ToBeVisibleAsync();
        await Expect(Page.GetByText("fixture layer: Pull complete", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("done.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("pull access denied for nosuch:1", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("NewTag")).ToBeHiddenAsync();
        await Expect(Page.GetByText("LAYERS", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(hint).ToBeHiddenAsync();

        // Clearing the output leaves nothing to show, so the hint returns.
        await Button("Clear").ClickAsync();
        await Expect(hint).ToBeVisibleAsync();
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_efficiency_and_lint_reports_render_and_clear()
    {
        await Button("Images").ClickAsync();
        await SelectImageAsync("redis:8-alpine");

        await Button("Scan (Trivy)").ClickAsync();
        await Expect(Page.GetByText("CVE-2099-0001", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("fixed in 1.1", new() { Exact = true })).ToBeVisibleAsync();

        await Button("Efficiency (Dive)").ClickAsync();
        await Expect(Page.GetByText("Efficiency score 0.987", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("CVE-2099-0001", new() { Exact = true })).ToHaveCountAsync(0);

        await Page.GetByTestId("DockerfilePath").FillAsync("Dockerfile");
        await Button("Lint (Hadolint)").ClickAsync();
        await Expect(Page.GetByText("Linting Dockerfile with Hadolint.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("warning DL3007 line 1 Using latest is prone to errors.", new() { Exact = true })).ToBeVisibleAsync();

        await Button("Clear").ClickAsync();
        await Expect(Page.GetByText("Linting Dockerfile with Hadolint.", new() { Exact = true })).ToBeHiddenAsync();
        Fixture.Docker.UnexpectedCalls.Should().BeEmpty();
    }
}
