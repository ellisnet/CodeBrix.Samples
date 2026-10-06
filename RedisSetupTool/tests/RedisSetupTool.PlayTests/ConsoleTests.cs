using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Platform.UI.TerminalView;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    private async Task<FixtureExecSession> OpenConsoleAsync()
    {
        await Button("Consoles").ClickAsync();
        await Button("Open a console").First.ClickAsync();
        await Row("cache-primary").ClickAsync();
        await Expect(Page.GetByText("running", new() { Exact = true })).ToBeVisibleAsync();
        return await Fixture.Application.WaitForAsync(() => Fixture.Docker.Sessions.Length == 1 ? Fixture.Docker.Sessions[0] : null,
            session => session != null, description: "the console's exec session");
    }

    [Fact]
    public async Task Opening_a_console_adds_a_running_tab_with_its_shell_path()
    {
        var session = await OpenConsoleAsync();
        await Expect(Page.GetByText("/bin/sh", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Tab).Filter(new() { HasText = "cache-primary" })).ToBeVisibleAsync();
        await Expect(Page.GetByText(new Regex(@"^\d+ x \d+$"))).ToBeVisibleAsync();
        await Expect(Button("Reopen")).ToBeHiddenAsync();
        session.ContainerId.Should().Be(DockerFixture.RunningId);
        session.Columns.Should().BeGreaterThan(0);
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.Tabs.Count)).Should().Be(1);
        session.Write("fixture$ ");
        await SnapshotAsync("RedisSetupTool-console");
    }

    [Fact]
    public async Task Console_without_a_shell_shows_the_failure_in_the_status_strip()
    {
        Fixture.Docker.ShellPath = null;
        await Button("Consoles").ClickAsync();
        await Button("Open a console").First.ClickAsync();
        await Row("cache-primary").ClickAsync();
        await Expect(Page.GetByText("Fixture image has no shell.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("failed", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Reopen")).ToBeVisibleAsync();
        Fixture.Docker.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Closing_a_console_tab_disposes_its_session()
    {
        var session = await OpenConsoleAsync();
        await Button("Close").ClickAsync();
        await Expect(Page.GetByText("No consoles are open.", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => session.Disposed, disposed => disposed, description: "session disposed");
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.Tabs.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Reopen_after_exit_starts_a_fresh_session()
    {
        var first = await OpenConsoleAsync();
        first.End(3);
        await Expect(Page.GetByText("exited (code 3)", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Reopen").ClickAsync();
        await Expect(Page.GetByText("running", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Docker.Sessions.Length, count => count == 2, description: "a second session");
        await Fixture.Application.WaitForAsync(() => first.Disposed, disposed => disposed, description: "old session disposed");
        Fixture.Docker.Sessions[1].Disposed.Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Consoles.Tabs.Count)).Should().Be(1);
    }

    [Fact]
    public async Task Typed_console_input_reaches_the_exec_session()
    {
        var session = await OpenConsoleAsync();
        await Page.GetByType<TerminalControl>().ClickAsync();
        await Page.Keyboard.TypeAsync("PING\n");
        await Fixture.Application.WaitForAsync(() => session.SentText, text => text.Contains("PING"), description: "typed input");
        session.SentText.Should().StartWith("PING");
    }
}
