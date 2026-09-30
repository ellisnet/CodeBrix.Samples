using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using GitHubIssueFinder.GitHub;
using GitHubIssueFinder.Settings;
using GitHubIssueFinder.ViewModels;
using GitHubIssueFinder.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace GitHubIssueFinder.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public SearchFixture Search { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<IGitHubIssueSearchService>(Search), Path.Combine(DataDirectory, "settings"));

    protected override Task BeforeResetAsync()
    {
        Search.Fail = Search.Empty = Search.WaitForCancellation = false;
        Search.LastRequest = null;
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
    public RateLimitSnapshot LastSearchRateLimit => null;
    public RateLimitSnapshot LastCoreRateLimit => null;

    public async IAsyncEnumerable<IssueSearchPage> SearchAsync(IssueSearchRequest request,
        IProgress<SearchProgress> progress = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        await Task.Yield();
        if (WaitForCancellation) await Task.Delay(Timeout.Infinite, cancellationToken);
        if (Fail) throw new InvalidOperationException("Fixture service unavailable");
        if (Empty) yield break;
        yield return new IssueSearchPage
        {
            PageNumber = 1, TotalCount = 2,
            Items = new[] { Item(1, "Improve keyboard navigation"), Item(2, "Add portrait layout coverage") },
        };
    }

    private static IssueItem Item(int number, string title) => new()
    {
        Id = number, Number = number, Title = title, RepositoryFullName = "sample/widgets",
        RepositoryHtmlUrl = "https://example.invalid/sample/widgets",
        HtmlUrl = $"https://example.invalid/sample/widgets/issues/{number}",
        AuthorLogin = "fixture-author", State = IssueState.Open, Kind = IssueKind.Issue,
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        UpdatedAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
        AssigneeLogins = Array.Empty<string>(), Labels = Array.Empty<IssueLabel>(),
    };
}
