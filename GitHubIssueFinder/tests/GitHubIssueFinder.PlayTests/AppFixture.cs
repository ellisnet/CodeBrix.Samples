using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using GitHubIssueFinder.GitHub;
using GitHubIssueFinder.Helpers;
using GitHubIssueFinder.Settings;
using GitHubIssueFinder.ViewModels;
using GitHubIssueFinder.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace GitHubIssueFinder.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public SearchFixture Search { get; } = new();
    public UrlOpenerFixture Links { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<IGitHubIssueSearchService>(Search).AddSingleton<IUrlOpener>(Links),
        Path.Combine(DataDirectory, "settings"));

    protected override Task BeforeResetAsync()
    {
        Search.Fail = Search.Empty = Search.WaitForCancellation = false;
        Search.LastRequest = null;
        Search.Error = null;
        Search.Pages = null;
        Search.Reports = null;
        Search.SearchQuota = Search.CoreQuota = null;
        Search.LastSearchRateLimit = Search.LastCoreRateLimit = null;
        Search.ResetHold();
        Links.Opened.Clear();
        SettingsService.Set(SettingKeys.Owner, "");
        SettingsService.Set(SettingKeys.Assignee, "");
        SettingsService.Set(SettingKeys.IncludeClosed, false);
        SettingsService.Set(SettingKeys.ColorScheme, "SystemDefault");
        return Task.CompletedTask;
    }
    protected override void Cleanup() => SettingsService.Shutdown();
}

public sealed class SearchFixture : IGitHubIssueSearchService
{
    public bool Fail { get; set; }
    public bool Empty { get; set; }
    public bool WaitForCancellation { get; set; }
    public IssueSearchRequest LastRequest { get; set; }
    public RateLimitSnapshot LastSearchRateLimit { get; set; }
    public RateLimitSnapshot LastCoreRateLimit { get; set; }
    // Thrown in place of Fail's generic error, for example a GitHubApiException.
    public Exception Error { get; set; }
    // Scripted pages; null keeps the default single page.
    public IReadOnlyList<IssueSearchPage> Pages { get; set; }
    // Reported before the first page, for example a wait for the quota.
    public IReadOnlyList<SearchProgress> Reports { get; set; }
    // The pools carried by the Fetching report that precedes each page, as the real service sends it.
    public RateLimitSnapshot SearchQuota { get; set; }
    public RateLimitSnapshot CoreQuota { get; set; }
    // The search holds once this many pages have been handed over, until ReleaseHold() or cancellation.
    public int HoldAfterPages { get; set; } = -1;
    private TaskCompletionSource _hold = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void ReleaseHold() => _hold.TrySetResult();

    public void ResetHold()
    {
        HoldAfterPages = -1;
        _hold.TrySetCanceled();
        _hold = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async IAsyncEnumerable<IssueSearchPage> SearchAsync(IssueSearchRequest request,
        IProgress<SearchProgress> progress = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        var hold = _hold;
        await Task.Yield();
        if (WaitForCancellation) await Task.Delay(Timeout.Infinite, cancellationToken);
        if (Error != null) throw Error;
        if (Fail) throw new InvalidOperationException("Fixture service unavailable");
        foreach (var report in Reports ?? Array.Empty<SearchProgress>()) progress?.Report(report);
        if (Empty) yield break;
        var pages = Pages ?? new[]
        {
            new IssueSearchPage
            {
                PageNumber = 1, TotalCount = 2,
                Items = new[] { Item(1, "Improve keyboard navigation"), Item(2, "Add portrait layout coverage") },
            },
        };
        var fetched = 0;
        for (var index = 0; index <= pages.Count; index++)
        {
            if (index == HoldAfterPages) await hold.Task.WaitAsync(cancellationToken);
            if (index == pages.Count) break;
            fetched += pages[index].Items.Count;
            progress?.Report(new SearchProgress(SearchPhase.Fetching, fetched, pages[index].TotalCount,
                pages[index].PageNumber, null, null, SearchQuota, CoreQuota));
            yield return pages[index];
        }
    }

    public static IssueItem Item(int number, string title, string repository = "sample/widgets") => new()
    {
        Id = number, Number = number, Title = title, RepositoryFullName = repository,
        RepositoryHtmlUrl = $"https://example.invalid/{repository}",
        HtmlUrl = $"https://example.invalid/{repository}/issues/{number}",
        AuthorLogin = "fixture-author", State = IssueState.Open, Kind = IssueKind.Issue,
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        UpdatedAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
        AssigneeLogins = Array.Empty<string>(), Labels = Array.Empty<IssueLabel>(),
    };
}

public sealed class UrlOpenerFixture : IUrlOpener
{
    public List<Uri> Opened { get; } = new();

    public Task<bool> OpenAsync(Uri uri)
    {
        Opened.Add(uri);
        return Task.FromResult(true);
    }
}
