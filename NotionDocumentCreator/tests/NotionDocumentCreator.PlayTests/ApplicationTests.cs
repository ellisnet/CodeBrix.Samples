using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using NotionDocumentCreator.CreateDocument.Models;
using NotionDocumentCreator.Views;
using SilverAssertions;
using Xunit;

namespace NotionDocumentCreator.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private Locator Include(string title) =>
        Page.GetByRole(AriaRole.Checkbox, new() { Name = "Include " + title, Exact = true });
    private Locator TitleButton(string title) => Page.GetByRole(AriaRole.Button, new() { Name = title });
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);

    private async Task ConnectAsync()
    {
        await Page.GetByTestId("IntegrationToken").FillAsync("secret-token");
        await Page.GetByTestId("PageOrDatabaseId").FillAsync("field-guide");
        await Button("Connect").ClickAsync();
        await Expect(Page.GetByTestId("ConnectionStatus")).ToHaveTextAsync("Connected as fixture-bot");
        await Expect(Include("Forests")).ToBeVisibleAsync();
    }
    private async Task ExpandAsync(string title) =>
        await Page.GetByRole(AriaRole.Treeitem, new() { Name = title, Exact = true }).PressAsync("ArrowRight");
    private async Task SelectOutputAsync(string path)
    {
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Select…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.OutputFilePath, value => value == path);
    }
    private async Task CloseDialogAsync()
    {
        await Dialog.GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Connect_is_disabled_until_token_and_page_id_are_filled()
    {
        await Expect(Button("Connect")).ToBeDisabledAsync();
        await Page.GetByTestId("IntegrationToken").FillAsync("secret-token");
        await Expect(Button("Connect")).ToBeDisabledAsync();
        await Page.GetByTestId("PageOrDatabaseId").FillAsync("   ");
        await Expect(Button("Connect")).ToBeDisabledAsync();
        await Page.GetByTestId("PageOrDatabaseId").FillAsync("field-guide");
        await Expect(Button("Connect")).ToBeEnabledAsync();
        await Page.GetByTestId("IntegrationToken").FillAsync(" ");
        await Expect(Button("Connect")).ToBeDisabledAsync();
        await SnapshotAsync("NotionDocumentCreator-home");
    }

    [Fact]
    public async Task Create_and_load_tree_are_disabled_before_connect()
    {
        await Page.GetByTestId("OutputFilePath").FillAsync(Path.Combine(Fixture.DataDirectory, "home.pdf"));
        await Expect(Button("Create!")).ToBeDisabledAsync();
        await Expect(Button("Load whole tree")).ToBeDisabledAsync();
        await Expect(Button("Select…")).ToBeEnabledAsync();
        await Expect(Page.GetByText("Connect to see your pages", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("ConnectionStatus")).ToHaveTextAsync("Not connected");
    }

    [Fact]
    public async Task Connect_shows_tree_and_connected_status()
    {
        await ConnectAsync();
        await Expect(Page.GetByText("Connect to see your pages", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(Include("Field Guide")).ToBeVisibleAsync();
        await Expect(Include("Rivers")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("CheckedCountText")).ToHaveTextAsync("No pages selected yet");
        await Expect(Button("Load whole tree")).ToBeEnabledAsync();
        await Expect(Button("Create!")).ToBeDisabledAsync();
        Fixture.Notion.LastToken.Should().Be("secret-token");
        (await Page.EvaluateAsync(() => Fixture.Model.RootNodes[0].IsExpanded)).Should().BeTrue();
        await SnapshotAsync("NotionDocumentCreator-connected");
    }

    [Fact]
    public async Task Connect_failure_shows_error_dialog_and_stays_disconnected()
    {
        Fixture.Notion.FailConnect = true;
        await Page.GetByTestId("IntegrationToken").FillAsync("secret-token");
        await Page.GetByTestId("PageOrDatabaseId").FillAsync("field-guide");
        await Button("Connect").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Could not connect: Fixture token rejected");
        await CloseDialogAsync();
        await Expect(Page.GetByTestId("ConnectionStatus")).ToHaveTextAsync("Not connected");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Connection failed.");
        await Expect(Page.GetByText("Connect to see your pages", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Connect")).ToBeEnabledAsync();
        await Expect(Button("Load whole tree")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Checking_pages_updates_selected_count_text()
    {
        await ConnectAsync();
        await Expect(Page.GetByTestId("CheckedCountText")).ToHaveTextAsync("No pages selected yet");
        await Include("Rivers").CheckAsync();
        await Expect(Page.GetByTestId("CheckedCountText")).ToHaveTextAsync("1 page selected");
        await Include("Forests").CheckAsync();
        await Include("Field Guide").CheckAsync();
        await Expect(Page.GetByTestId("CheckedCountText")).ToHaveTextAsync("3 pages selected");
        await Include("Field Guide").UncheckAsync();
        await Expect(Page.GetByTestId("CheckedCountText")).ToHaveTextAsync("2 pages selected");
        await Expect(Include("Rivers")).ToBeCheckedAsync();
    }

    [Fact]
    public async Task Expanding_a_node_replaces_loading_row_with_children()
    {
        var gate = Fixture.Notion.HoldChildren("rivers");
        await ConnectAsync();
        await ExpandAsync("Rivers");
        await Expect(Page.GetByText("Loading…", new() { Exact = true })).ToBeVisibleAsync();
        gate.SetResult();
        await Expect(Include("Springs")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Loading…", new() { Exact = true })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Child_load_failure_is_reported_on_status_line()
    {
        await ConnectAsync();
        Fixture.Notion.FailChildren = true;
        await ExpandAsync("Rivers");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Could not load child pages: Fixture children unavailable");
        await Expect(Include("Springs")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Tapping_a_title_shows_preview()
    {
        await ConnectAsync();
        await Expect(Page.GetByText("Tap a page title to preview it here", new() { Exact = true })).ToBeVisibleAsync();
        await TitleButton("Rivers").ClickAsync();
        await Expect(Page.GetByTestId("PreviewTitle")).ToHaveTextAsync("Rivers");
        await Expect(Page.GetByTestId("PreviewMeta")).ToHaveTextAsync("1 child page · edited 2026-03-14");
        await Expect(Page.GetByTestId("PreviewSnippets")).ToContainTextAsync("The opening paragraph of Rivers.");
        await Expect(Page.GetByText("Tap a page title to preview it here", new() { Exact = true })).ToBeHiddenAsync();
        await SnapshotAsync("NotionDocumentCreator-preview");
    }

    [Fact]
    public async Task Newer_selection_supersedes_slower_preview()
    {
        await ConnectAsync();
        var slow = Fixture.Notion.HoldPreview("rivers");
        await TitleButton("Rivers").ClickAsync();
        await TitleButton("Forests").ClickAsync();
        await Expect(Page.GetByTestId("PreviewTitle")).ToHaveTextAsync("Forests");
        slow.SetResult();
        await Fixture.Application.WaitForAsync(() => slow.Task.IsCompleted, done => done);
        await Expect(Page.GetByTestId("PreviewMeta")).ToHaveTextAsync("0 child pages · edited 2026-03-14");
        (await Page.EvaluateAsync(() => Fixture.Model.PreviewTitle)).Should().Be("Forests");
    }

    [Fact]
    public async Task Select_suggests_first_checked_title_and_removes_empty_placeholder()
    {
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        await Include("Rivers").CheckAsync();
        var path = Path.Combine(Fixture.DataDirectory, "Selected " + Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("Rivers.pdf");
        File.Exists(path).Should().BeFalse("the application removes an empty save-picker placeholder");
        await Expect(Page.GetByTestId("OutputFilePath")).ToHaveValueAsync(path);
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Will save to: " + path);
    }

    [Fact]
    public async Task Cancelled_save_picker_keeps_output_path()
    {
        // A short value: a path wider than the box squeezes Select… out of the bottom bar.
        var path = "previous.pdf";
        await Page.GetByTestId("OutputFilePath").FillAsync(path);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Select…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("NotionBook.pdf");
        (await Page.EvaluateAsync(() => Fixture.Model.OutputFilePath)).Should().Be(path);
    }

    [Fact]
    public async Task Create_sends_checked_pages_in_tree_order_with_chosen_page_size()
    {
        await ConnectAsync();
        await ExpandAsync("Rivers");
        await Include("Springs").CheckAsync();
        await Include("Forests").CheckAsync();
        await Include("Field Guide").CheckAsync();
        var path = Path.Combine(Fixture.DataDirectory, "Book " + Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        await Page.GetByTestId("SelectedPageSize").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = PageSizeInfo.For(PageSizeOption.A4).DisplayName, Exact = true }).ClickAsync();
        await Button("Create!").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Created “Field Guide”");
        await Expect(Dialog).ToContainTextAsync("3 chapters · 12 pages · 0 images · 3 seconds");
        Fixture.Notion.LastCreateRequest.PageIds.Should().Equal("field-guide", "springs", "forests");
        Fixture.Notion.LastCreateRequest.OutputFilePath.Should().Be(path);
        Fixture.Notion.LastCreateRequest.PageSize.Should().Be(PageSizeOption.A4);
        File.ReadAllText(path).Should().StartWith("%PDF-");
        await SnapshotAsync("NotionDocumentCreator-created");
        await CloseDialogAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Saved: " + path);
        await Expect(Button("Create!")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Existing_file_prompts_and_no_keeps_it()
    {
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        var path = Path.Combine(Fixture.DataDirectory, "existing " + Guid.NewGuid().ToString("N") + ".pdf");
        await File.WriteAllTextAsync(path, "Keep this document", TestContext.Current.CancellationToken);
        await SelectOutputAsync(path);
        await Button("Create!").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Replace existing file?");
        await Dialog.GetByRole(AriaRole.Button, new() { Name = "No", Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Creation cancelled — the existing file was kept.");
        File.ReadAllText(path).Should().Be("Keep this document");
        Fixture.Notion.LastCreateRequest.Should().BeNull();
    }

    [Fact]
    public async Task Create_failure_reports_error_and_reenables_controls()
    {
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        Fixture.Notion.FailCreate = true;
        await Button("Create!").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Error while creating the document: Fixture renderer unavailable");
        await CloseDialogAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Creation failed.");
        await Expect(Button("Create!")).ToBeEnabledAsync();
        await Expect(Button("Connect")).ToBeEnabledAsync();
        await Expect(Button("Select…")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Busy_connect_disables_all_actions()
    {
        Fixture.Notion.ConnectGate = new();
        await Page.GetByTestId("IntegrationToken").FillAsync("secret-token");
        await Page.GetByTestId("PageOrDatabaseId").FillAsync("field-guide");
        await Button("Connect").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Connecting to Notion…");
        await Expect(Button("Connect")).ToBeDisabledAsync();
        await Expect(Button("Select…")).ToBeDisabledAsync();
        await Expect(Button("Load whole tree")).ToBeDisabledAsync();
        await Expect(Button("Create!")).ToBeDisabledAsync();
        Fixture.Notion.ConnectGate.SetResult();
        await Expect(Page.GetByTestId("ConnectionStatus")).ToHaveTextAsync("Connected as fixture-bot");
        await Expect(Button("Select…")).ToBeEnabledAsync();
        await Expect(Button("Load whole tree")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Create_progress_updates_status_and_progress_bar()
    {
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        var path = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        Fixture.Notion.CreateGate = new();
        await Button("Create!").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Composing the fixture book…");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ProgressValue, value => value == 40);
        await Expect(Button("Create!")).ToBeDisabledAsync();
        Fixture.Notion.CreateGate.SetResult();
        await Expect(Dialog).ToContainTextAsync("Created “Field Guide”");
        await CloseDialogAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ProgressValue, value => value == 0);
        await Expect(Button("Create!")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Load_whole_tree_expands_all_and_reports_page_count()
    {
        await ConnectAsync();
        await Button("Load whole tree").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Loaded 4 pages.");
        await Expect(Include("Springs")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.RootNodes[0].Children[0].IsExpanded)).Should().BeTrue();
        await Expect(Button("Load whole tree")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Typed_output_path_enables_create()
    {
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        await Expect(Button("Create!")).ToBeDisabledAsync();
        await Page.GetByTestId("OutputFilePath").FillAsync(Path.Combine(Fixture.DataDirectory, "typed.pdf"));
        await Expect(Button("Create!")).ToBeEnabledAsync();
        await Page.GetByTestId("OutputFilePath").FillAsync("  ");
        await Expect(Button("Create!")).ToBeDisabledAsync();
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_header_and_bottom_bar_remain_usable()
    {
        await ConnectAsync();
        await TitleButton("Rivers").ClickAsync();
        await Expect(Page.GetByTestId("PreviewTitle")).ToHaveTextAsync("Rivers");
        await Include("Forests").CheckAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, "portrait.pdf"));
        await Expect(Button("Create!")).ToBeEnabledAsync();
        var connect = await Button("Connect").BoundingBoxAsync();
        var create = await Button("Create!").BoundingBoxAsync();
        (connect.X + connect.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
        (create.X + create.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
        (create.Y + create.Height).Should().BeLessThanOrEqualTo(Fixture.Application.Height);
        await SnapshotAsync("NotionDocumentCreator-portrait");
    }

    [Fact]
    public async Task Success_dialog_lists_warnings_and_overflow_count()
    {
        Fixture.Notion.Warnings = new[] { "Note 1", "Note 2", "Note 3", "Note 4", "Note 5", "Note 6", "Note 7", "Note 8" };
        await ConnectAsync();
        await Include("Forests").CheckAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        await Button("Create!").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("• Note 6");
        await Expect(Dialog).ToContainTextAsync("…and 2 more.");
        (await Dialog.InnerTextAsync()).Should().NotContain("Note 7");
        await CloseDialogAsync();
    }
}
