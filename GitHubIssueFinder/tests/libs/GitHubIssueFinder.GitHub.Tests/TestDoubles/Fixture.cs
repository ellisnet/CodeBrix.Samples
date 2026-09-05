using System;
using System.IO;

namespace GitHubIssueFinder.GitHub.Tests;

//The saved GitHub responses, copied beside the test binary by the project file.
internal static class Fixture
{
    internal const string SearchPage = "search_owner_page1.json";
    internal const string SearchOverCap = "search_over_cap.json";
    internal const string SearchEmpty = "search_empty.json";
    internal const string IssueNotPlanned = "search_issue_not_planned.json";
    internal const string PullRequestsMerged = "search_pr_merged.json";
    internal const string PullRequestDraft = "search_pr_draft.json";
    internal const string RepositoriesPage = "repos_page1.json";
    internal const string UnknownOwnerError = "error_422_unknown_owner.json";
    internal const string RateLimitedError = "error_403_rate_limited.json";

    internal static string Read(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}
