using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace JustBetweenUs.PlayTests;

// Contract checks use the same real application and package as the workflow tests.
public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Multiline_input_exposes_portable_line_endings_and_preserves_caret()
    {
        await Input.FillAsync("first\r\nsecond\nthird\rfourth");
        await Expect(Input).ToHaveValueAsync("first\nsecond\nthird\nfourth");
        (await Input.InputValueAsync()).Should().Be("first\nsecond\nthird\nfourth");
        await Page.Keyboard.InsertTextAsync("\r\nfifth");
        await Expect(Input).ToHaveValueAsync("first\nsecond\nthird\nfourth\nfifth");
    }

    [Fact]
    public async Task Ambiguous_actions_fail_without_clicking_an_arbitrary_control()
    {
        Func<Task> fill = () => Page.GetByRole(AriaRole.Textbox).FillAsync("ambiguous");
        var error = (await fill.Should().ThrowExactlyAsync<PlayTestException>()).Which;
        error.Message.Should().Contain("Strict mode violation");
        await Expect(Input).ToHaveValueAsync("");
        await Expect(Key).ToHaveValueAsync("27544076");
    }

    [Fact]
    public async Task Disabled_click_times_out_with_screenshot_and_never_runs_command()
    {
        Func<Task> click = () => Encrypt.ClickAsync(new() { Timeout = 150 });
        var error = (await click.Should().ThrowExactlyAsync<PlayTestException>()).Which;
        error.Message.Should().Contain("Timeout 150ms");
        error.Message.Should().Contain("Screenshot:");
        error.Message.Should().Contain("UI:");
        var screenshot = error.Message.Split("Screenshot: ")[1].Split('\n')[0].Trim();
        File.Exists(screenshot).Should().BeTrue();
        await Expect(Output).ToHaveValueAsync("");
    }

    [Fact]
    public async Task Missing_and_negated_assertions_do_not_report_false_success()
    {
        var missing = Page.GetByTestId("does-not-exist");
        await Expect(missing).Not.ToBeVisibleAsync();
        await Expect(missing).ToHaveCountAsync(0);
        Func<Task> assertMissingValue = () => Expect(missing).Not.ToHaveValueAsync("anything", new() { Timeout = 100 });
        await assertMissingValue.Should().ThrowExactlyAsync<PlayTestException>();
        Func<Task> assertWrongValue = () => Expect(Input).ToHaveValueAsync("not-the-value", new() { Timeout = 100 });
        await assertWrongValue.Should().ThrowExactlyAsync<PlayTestException>();
    }

    [Fact]
    public async Task Read_only_fill_times_out_without_mutating_output()
    {
        var ciphertext = await EncryptAsync("Keep this result.");
        Func<Task> fill = () => Output.FillAsync("forbidden edit", new() { Timeout = 100 });
        await fill.Should().ThrowExactlyAsync<PlayTestException>();
        await Expect(Output).ToHaveValueAsync(ciphertext);
    }

    [Fact]
    public async Task Locators_resolve_again_after_page_replacement()
    {
        var input = Page.GetByTestId("input-text");
        await input.FillAsync("old page");
        await _fixture.ResetAsync();
        await Expect(input).ToHaveValueAsync("");
        await input.FillAsync("fresh page");
        await Expect(input).ToHaveValueAsync("fresh page");
        (await Page.EvaluateAsync(() => ((ViewModels.MainViewModel)_fixture.View.DataContext).EnteredText)).Should().Be("fresh page");
    }

    [Fact]
    public async Task Open_dialog_intercepts_clicks_behind_it()
    {
        await Input.FillAsync("hello!");
        await Decrypt.ClickAsync();
        await Expect(Dialog).ToBeVisibleAsync();
        Func<Task> click = () => Encrypt.ClickAsync(new() { Timeout = 100 });
        await click.Should().ThrowExactlyAsync<PlayTestException>();
        await Expect(Output).ToHaveValueAsync("");
        await CloseDialogAsync();
    }

    [Fact]
    public async Task Outcome_probe_observes_ui_state_and_reports_timeout()
    {
        await Input.FillAsync("observed");
        var result = await _fixture.Application.WaitForAsync(
            () => ((ViewModels.MainViewModel)_fixture.View.DataContext).EnteredText,
            actual => actual == "observed", description: "bound input value");
        result.Should().Be("observed");
        Func<Task> waitForCompletion = () => _fixture.Application.WaitForAsync(
            () => false, actual => actual, timeout: 100, description: "deliberately absent completion");
        var error = (await waitForCompletion.Should().ThrowExactlyAsync<PlayTestException>()).Which;
        error.Message.Should().Contain("deliberately absent completion");
    }
}
