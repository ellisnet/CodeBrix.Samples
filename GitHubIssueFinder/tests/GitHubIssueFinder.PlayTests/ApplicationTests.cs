using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using GitHubIssueFinder.GitHub;
using GitHubIssueFinder.Settings;
using GitHubIssueFinder.Theming;
using GitHubIssueFinder.ViewModels;
using GitHubIssueFinder.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using Xunit;

namespace GitHubIssueFinder.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    [Fact]
    public async Task Blank_owner_disables_search_and_cancel()
    {
        await Expect(Button("Search")).ToBeDisabledAsync();
        await Expect(Button("Cancel")).ToBeDisabledAsync();
        await Page.GetByTestId("Owner").FillAsync("   ");
        await Expect(Button("Search")).ToBeDisabledAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Search_uses_inputs_and_renders_grouped_results(bool includeClosed)
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Page.GetByTestId("IncludeClosed").SetCheckedAsync(includeClosed);
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Add portrait layout coverage", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Search.LastRequest.Owner.Should().Be("sample");
        Fixture.Search.LastRequest.Assignee.Should().Be("developer");
        Fixture.Search.LastRequest.IncludeClosed.Should().Be(includeClosed);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsSearching, busy => !busy);
        (await Page.EvaluateAsync(() => Fixture.Model.Groups[0].Rows.Count)).Should().Be(2);
        await SnapshotAsync("GitHubIssueFinder-results");
    }

    [Fact]
    public async Task Enter_submits_and_Escape_cancels_a_pending_search()
    {
        Fixture.Search.WaitForCancellation = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Owner").PressAsync("Enter");
        await Expect(Button("Cancel")).ToBeEnabledAsync();
        await Page.GetByTestId("Owner").PressAsync("Escape");
        await Expect(Button("Cancel")).ToBeDisabledAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        Fixture.Search.LastRequest.Owner.Should().Be("sample");
    }

    [Fact]
    public async Task Service_error_is_reported_and_search_can_be_retried()
    {
        Fixture.Search.Fail = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Fixture service unavailable")).ToBeVisibleAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        Fixture.Search.Fail = false;
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Empty_search_has_no_result_groups()
    {
        Fixture.Search.Empty = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsSearching, busy => !busy);
        (await Page.EvaluateAsync(() => Fixture.Model.Groups.Count)).Should().Be(0);
        await Expect(Button("Search")).ToBeEnabledAsync();
    }

    [Theory]
    [InlineData("Dark", ColorScheme.Dark)]
    [InlineData("Light High Contrast", ColorScheme.LightHighContrast)]
    public async Task Scheme_picker_changes_the_application_palette(string name, ColorScheme expected)
    {
        await Page.GetByTestId("SelectedScheme").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedScheme.Scheme)).Should().Be(expected);
        await SnapshotAsync("GitHubIssueFinder-scheme");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_header_wraps_and_search_remains_usable()
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("GitHubIssueFinder-portrait");
    }

    [Fact]
    public async Task Results_header_counts_open_closed_repositories_and_issue_pr_split()
    {
        Fixture.Search.Pages = new[]
        {
            PageOf(1, 5,
                Item(1, "Open issue", "zeta/app"),
                Item(2, "Closed issue", "zeta/app", state: IssueState.Closed),
                Item(3, "Draft pull request", "alpha/lib", IssueKind.PullRequest, IssueState.Draft),
                Item(4, "Merged pull request", "alpha/lib", IssueKind.PullRequest, IssueState.Merged),
                Item(5, "Open pull request", "zeta/app", IssueKind.PullRequest)),
        };
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("IncludeClosed").CheckAsync();
        await Button("Search").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 5 items in 2 repositories in .+\.$"));
        await Expect(Page.GetByTestId("HeaderOpenText")).ToHaveTextAsync("3 open");
        await Expect(Page.GetByTestId("HeaderSummary")).ToHaveTextAsync("· 2 closed · 2 repositories");
        await Expect(Page.GetByTestId("IssuePrSplit")).ToHaveTextAsync("Issues 2 · Pull requests 3");
    }

    [Fact]
    public async Task Groups_are_sorted_alphabetically_across_pages()
    {
        Fixture.Search.Pages = new[]
        {
            PageOf(1, 4, Item(1, "First zeta issue", "zeta/app"), Item(2, "Middle issue", "mid/tool")),
            PageOf(2, 4, Item(3, "Alpha issue", "alpha/lib"), Item(4, "Second zeta issue", "zeta/app")),
        };
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 4 items in 3 repositories in "));
        (await Page.EvaluateAsync(() => string.Join(",", Fixture.Model.Groups.Select(g => g.FullName))))
            .Should().Be("alpha/lib,mid/tool,zeta/app");
        (await Page.EvaluateAsync(() => Fixture.Model.Groups[2].CountText)).Should().Be("2");
        await Expect(Page.GetByText("Second zeta issue", new() { Exact = true })).ToBeVisibleAsync();
        var alpha = await Page.GetByText("alpha/lib", new() { Exact = true }).BoundingBoxAsync();
        var mid = await Page.GetByText("mid/tool", new() { Exact = true }).BoundingBoxAsync();
        var zeta = await Page.GetByText("zeta/app", new() { Exact = true }).BoundingBoxAsync();
        alpha.Y.Should().BeLessThan(mid.Y);
        mid.Y.Should().BeLessThan(zeta.Y);
        // Rows stay in arrival order inside their group.
        var first = await Page.GetByText("First zeta issue", new() { Exact = true }).BoundingBoxAsync();
        var second = await Page.GetByText("Second zeta issue", new() { Exact = true }).BoundingBoxAsync();
        zeta.Y.Should().BeLessThan(first.Y);
        first.Y.Should().BeLessThan(second.Y);
    }

    [Fact]
    public async Task Streaming_pages_show_still_loading_then_done_status()
    {
        Fixture.Search.Pages = new[]
        {
            PageOf(1, 3, Item(1, "Arrives first")),
            PageOf(2, 3, Item(2, "Arrives second"), Item(3, "Arrives in another repository", "sample/gadgets")),
        };
        Fixture.Search.HoldAfterPages = 1;
        await SearchForAsync("sample");
        await Expect(Page.GetByText("Arrives first", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("HeaderSummary")).ToHaveTextAsync("· 1 repository · still loading");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Fetched 1 of 3 · page 1");
        await Expect(Button("Cancel")).ToBeEnabledAsync();
        await Expect(Page.GetByText("Arrives second", new() { Exact = true })).ToHaveCountAsync(0);
        Fixture.Search.ReleaseHold();
        await Expect(Page.GetByText("Arrives in another repository", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("HeaderSummary")).ToHaveTextAsync("· 2 repositories");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 3 items in 2 repositories in .+\.$"));
        await Expect(Button("Cancel")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Row_shows_pr_chip_labels_and_comment_count()
    {
        var pullRequest = Item(7, "Add a dimmed scheme", kind: IssueKind.PullRequest, state: IssueState.Draft);
        pullRequest.CommentCount = 17;
        pullRequest.MilestoneTitle = "v2.0";
        pullRequest.Labels = new[]
        {
            new IssueLabel { Name = "enhancement", ColorHex = "a2eeef" },
            new IssueLabel { Name = "good first issue", ColorHex = "7057ff" },
        };
        Fixture.Search.Pages = new[] { PageOf(1, 2, pullRequest, Item(8, "A plain issue")) };
        await SearchForAsync("sample");
        var row = Page.GetByRole(AriaRole.Button, new() { Name = "Add a dimmed scheme" });
        var plain = Page.GetByRole(AriaRole.Button, new() { Name = "A plain issue" });
        await Expect(row.GetByText("PR", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(row.GetByText("enhancement", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(row.GetByText("good first issue", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(row.GetByText("17", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(row.GetByText("#7 opened")).ToHaveTextAsync(
            new Regex(@"^#7 opened .+ by fixture-author · updated .+ · milestone: v2\.0$"));
        await Expect(plain.GetByText("PR", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(plain.GetByText("enhancement", new() { Exact = true })).ToHaveCountAsync(0);
        var drawn = await Page.EvaluateAsync(() => Fixture.Model.Groups[0].Rows[0]);
        drawn.StateGlyph.Should().Be(Glyphs.DraftPullRequest);
        drawn.MetaToolTip.Should().StartWith("Opened ");
        (await Page.EvaluateAsync(() => Fixture.Model.Groups[0].Rows[1].StateGlyph)).Should().Be(Glyphs.OpenIssue);
    }

    [Fact]
    public async Task Empty_result_shows_owner_text_and_include_closed_hint()
    {
        Fixture.Search.Empty = true;
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("EmptyText"))
            .ToHaveTextAsync("No open issues or pull requests without an assignee in sample's public repositories.");
        await Expect(Page.GetByTestId("EmptyHint")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("HeaderOpenText")).ToHaveTextAsync("No results");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 0 items in 0 repositories in .+\.$"));
    }

    [Fact]
    public async Task Empty_hint_is_hidden_when_include_closed_was_searched()
    {
        Fixture.Search.Empty = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Page.GetByTestId("IncludeClosed").CheckAsync();
        await Button("Search").ClickAsync();
        await Expect(Page.GetByTestId("EmptyText"))
            .ToHaveTextAsync("No issues or pull requests assigned to developer in sample's public repositories.");
        await Expect(Page.GetByTestId("EmptyHint")).ToBeHiddenAsync();
        // The hint follows the search that ran, not the checkbox as it is now.
        await Page.GetByTestId("IncludeClosed").UncheckAsync();
        await Expect(Page.GetByTestId("EmptyHint")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Rate_limit_error_shows_reset_time_and_returns_to_welcome()
    {
        Fixture.Search.Empty = true;
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("EmptyText")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Find the issues nobody has picked up")).ToBeHiddenAsync();
        var reset = new DateTimeOffset(2026, 1, 1, 12, 34, 56, TimeSpan.Zero);
        Fixture.Search.Error = new GitHubApiException("API rate limit exceeded", HttpStatusCode.Forbidden,
            "https://example.invalid/search/issues", "API rate limit exceeded", reset);
        await Button("Search").ClickAsync();
        var local = reset.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        await Expect(Page.GetByTestId("StatusText"))
            .ToHaveTextAsync($"GitHub refused the request: search quota exhausted, resets at {local}.");
        await Expect(Page.GetByTestId("StatusGlyph")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Find the issues nobody has picked up")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("EmptyText")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("HeaderOpenText")).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.StatusKind)).Should().Be(SearchStatusKind.Failed);
    }

    [Fact]
    public async Task Cancel_button_stops_search_and_reports_cancelled_status()
    {
        Fixture.Search.Pages = new[]
        {
            PageOf(1, 5, Item(1, "Kept after cancelling"), Item(2, "Also kept after cancelling")),
            PageOf(2, 5, Item(3, "Never arrives")),
        };
        Fixture.Search.HoldAfterPages = 1;
        await SearchForAsync("sample");
        await Expect(Page.GetByText("Kept after cancelling", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Cancel").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Cancelled after 2 of 5.");
        await Expect(Button("Cancel")).ToBeDisabledAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        await Expect(Page.GetByText("Kept after cancelling", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Also kept after cancelling", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("HeaderSummary")).ToHaveTextAsync("· 1 repository");
        (await Page.EvaluateAsync(() => Fixture.Model.StatusKind)).Should().Be(SearchStatusKind.Cancelled);
    }

    [Fact]
    public async Task Enter_in_assignee_box_runs_search()
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Page.GetByTestId("Assignee").PressAsync("Enter");
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Search.LastRequest.Owner.Should().Be("sample");
        Fixture.Search.LastRequest.Assignee.Should().Be("developer");
    }

    [Fact]
    public async Task Helper_text_tracks_owner_assignee_and_scope()
    {
        var helper = Page.GetByTestId("HelperText");
        await Expect(helper).ToHaveTextAsync("Type a GitHub user or organization to search their public repositories.");
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Expect(helper).ToHaveTextAsync(
            "Searching sample's public repositories for open issues and pull requests assigned to no one.");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Expect(helper).ToHaveTextAsync(
            "Searching sample's public repositories for open issues and pull requests assigned to developer.");
        await Page.GetByTestId("IncludeClosed").CheckAsync();
        await Expect(helper).ToHaveTextAsync(
            "Searching sample's public repositories for open and closed issues and pull requests assigned to developer.");
        await Page.GetByTestId("Owner").FillAsync("  ");
        await Expect(helper).ToHaveTextAsync("Type a GitHub user or organization to search their public repositories.");
    }

    [Fact]
    public async Task Search_inputs_persist_and_restore_on_new_page()
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Page.GetByTestId("IncludeClosed").CheckAsync();
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        SettingsService.Get(SettingKeys.Owner, "").Should().Be("sample");
        SettingsService.Get(SettingKeys.Assignee, "").Should().Be("developer");
        SettingsService.Get(SettingKeys.IncludeClosed, false).Should().BeTrue();
        await Page.SetContentAsync(() => new MainPage());
        await Expect(Page.GetByTestId("Owner")).ToHaveValueAsync("sample");
        await Expect(Page.GetByTestId("Assignee")).ToHaveValueAsync("developer");
        await Expect(Page.GetByTestId("IncludeClosed")).ToBeCheckedAsync();
        await Expect(Page.GetByText("Find the issues nobody has picked up")).ToBeVisibleAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Scheme_choice_persists_and_restores_on_new_page()
    {
        await Page.GetByTestId("SelectedScheme").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Dark Dimmed", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => SettingsService.Get(SettingKeys.ColorScheme, ""),
            stored => stored == nameof(ColorScheme.DarkDimmed), description: "stored scheme");
        MainPage restored = null;
        await Page.SetContentAsync(() => restored = new MainPage());
        await Expect(Page.GetByTestId("SelectedScheme")).ToHaveValueAsync("Dark Dimmed");
        (await Page.EvaluateAsync(() => ((MainViewModel)restored.DataContext).SelectedScheme.Scheme))
            .Should().Be(ColorScheme.DarkDimmed);
        (await Page.EvaluateAsync(() => ((MainViewModel)restored.DataContext).CurrentPalette))
            .Should().BeSameAs(ColorSchemes.DarkDimmed);
        (await Page.EvaluateAsync(() => ((FrameworkElement)restored.Content).ActualTheme)).Should().Be(ElementTheme.Dark);
    }

    [Fact]
    public async Task Progress_report_makes_bar_determinate_and_updates_quota_pills()
    {
        var reset = new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero);
        Fixture.Search.Pages = new[]
        {
            PageOf(1, 4, Item(1, "Halfway issue"), Item(2, "Another halfway issue")),
            PageOf(2, 4, Item(3, "Third issue"), Item(4, "Fourth issue")),
        };
        Fixture.Search.SearchQuota = new RateLimitSnapshot(10, 7, 9, reset);
        Fixture.Search.CoreQuota = new RateLimitSnapshot(60, 58, 59, reset);
        Fixture.Search.LastSearchRateLimit = new RateLimitSnapshot(10, 9, 9, reset);
        Fixture.Search.LastCoreRateLimit = new RateLimitSnapshot(60, 59, 59, reset);
        Fixture.Search.HoldAfterPages = 1;
        await Expect(Page.GetByTestId("SearchQuota")).ToHaveTextAsync("Search 9 of 9");
        await Expect(Page.GetByTestId("CoreQuota")).ToHaveTextAsync("Core 59 of 59");
        await Expect(Page.GetByTestId("StatusProgress")).ToBeHiddenAsync();
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Fetched 2 of 4 · page 1");
        await Expect(Page.GetByTestId("StatusProgress")).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => (Fixture.Model.IsProgressIndeterminate, Fixture.Model.ProgressValue),
            bar => !bar.IsProgressIndeterminate && bar.ProgressValue == 50d, description: "determinate progress");
        await Expect(Page.GetByTestId("SearchQuota")).ToHaveTextAsync("Search 7 of 9");
        await Expect(Page.GetByTestId("CoreQuota")).ToHaveTextAsync("Core 58 of 59");
        Fixture.Search.ReleaseHold();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 4 items in 1 repository in .+\.$"));
        await Expect(Page.GetByTestId("StatusProgress")).ToBeHiddenAsync();
        // Once the search stops, the quota timer reads the service's last pools back.
        await Expect(Page.GetByTestId("SearchQuota")).ToHaveTextAsync("Search 9 of 9");
        await Expect(Page.GetByTestId("CoreQuota")).ToHaveTextAsync("Core 59 of 59");
    }

    [Fact]
    public async Task Waiting_for_quota_shows_attention_glyph_and_status()
    {
        var until = new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero);
        Fixture.Search.Reports = new[]
        {
            new SearchProgress(SearchPhase.WaitingForQuota, 0, null, 0, TimeSpan.FromSeconds(42), until,
                new RateLimitSnapshot(10, 0, 9, until), null),
        };
        Fixture.Search.HoldAfterPages = 0;
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Fetched 0 · waiting 42 s for the search quota to reset");
        await Expect(Page.GetByTestId("StatusGlyph")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("SearchQuota")).ToHaveTextAsync("Search 0 of 9");
        var waiting = await Page.EvaluateAsync(() => (Fixture.Model.StatusKind,
            ((SolidColorBrush)Fixture.Model.SearchQuotaBackground).Color,
            PaletteBrushes.ToColor(Fixture.Model.CurrentPalette.AttentionSubtle)));
        waiting.StatusKind.Should().Be(SearchStatusKind.Waiting);
        waiting.Item2.Should().Be(waiting.Item3);
        Fixture.Search.ReleaseHold();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 2 items in 1 repository in .+\.$"));
        await Expect(Page.GetByTestId("StatusGlyph")).ToBeHiddenAsync();
        var done = await Page.EvaluateAsync(() => (
            ((SolidColorBrush)Fixture.Model.SearchQuotaBackground).Color,
            PaletteBrushes.ToColor(Fixture.Model.CurrentPalette.CanvasInset)));
        done.Item1.Should().Be(done.Item2);
    }

    [Fact]
    public async Task Invalid_row_url_reports_link_could_not_be_read()
    {
        var broken = Item(1, "Row with a broken link");
        broken.HtmlUrl = "not a link";
        Fixture.Search.Pages = new[] { PageOf(1, 1, broken) };
        await SearchForAsync("sample");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Row with a broken link" }).ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("That link could not be read: not a link");
        await Expect(Page.GetByTestId("StatusGlyph")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Links.Opened.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Clicking_row_opens_issue_url()
    {
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: "));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add portrait layout coverage" }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Links.Opened.Count, count => count == 1,
            description: "opened link");
        Fixture.Links.Opened[0].Should().Be(new Uri("https://example.invalid/sample/widgets/issues/2"));
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: "));
    }

    [Fact]
    public async Task Clicking_group_header_opens_repository_url()
    {
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: "));
        await Page.GetByRole(AriaRole.Button, new() { Name = "sample/widgets" }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Links.Opened.Count, count => count == 1,
            description: "opened link");
        Fixture.Links.Opened[0].Should().Be(new Uri("https://example.invalid/sample/widgets"));
    }

    [Fact]
    public async Task New_search_replaces_previous_results()
    {
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 2 items in 1 repository"));
        Fixture.Search.Pages = new[] { PageOf(1, 1, Item(9, "Only in the second search", "other/repo")) };
        await Page.GetByTestId("Owner").FillAsync("other");
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Only in the second search", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToHaveCountAsync(0);
        await Expect(Page.GetByText("sample/widgets", new() { Exact = true })).ToHaveCountAsync(0);
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 1 item in 1 repository in .+\.$"));
        await Expect(Page.GetByTestId("HeaderSummary")).ToHaveTextAsync("· 1 repository");
        (await Page.EvaluateAsync(() => Fixture.Model.Groups.Count)).Should().Be(1);
        Fixture.Search.LastRequest.Owner.Should().Be("other");
    }

    private async Task SearchForAsync(string owner)
    {
        await Page.GetByTestId("Owner").FillAsync(owner);
        await Button("Search").ClickAsync();
    }

    private static IssueSearchPage PageOf(int number, int? total, params IssueItem[] items) => new()
    {
        PageNumber = number, TotalCount = total, Items = items,
    };

    private static IssueItem Item(int number, string title, string repository = "sample/widgets",
        IssueKind kind = IssueKind.Issue, IssueState state = IssueState.Open)
    {
        var item = SearchFixture.Item(number, title, repository);
        item.Kind = kind;
        item.State = state;
        return item;
    }
}
