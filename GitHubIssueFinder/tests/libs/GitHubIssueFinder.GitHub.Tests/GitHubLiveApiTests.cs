using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

//The only tests that touch the real GitHub API. They are off unless the environment variable
//is set, they only ever name the one account this repository belongs to, and they assert
//loosely: the point is that the addresses, the headers and the parsing hold up against the
//live service, not that any particular issue exists today.
[Trait("Category", "LiveApi")]
public class GitHubLiveApiTests
{
    private const string OptInVariable = "GITHUBISSUEFINDER_RUN_LIVE_TESTS";

    private const string Owner = "ellisnet";

    public GitHubLiveApiTests()
    {
        Assert.SkipWhen(Environment.GetEnvironmentVariable(OptInVariable) is null,
            "Set " + OptInVariable + "=1 to run live GitHub tests");
    }

    private static GitHubIssueSearchService CreateService() =>
        new GitHubIssueSearchService(new GitHubSearchOptions
        {
            UserAgent = "GitHubIssueFinder/live-test (CodeBrix.Platform sample)",
        });

    [Fact]
    public async Task the_unassigned_search_answers_from_the_live_api()
    {
        //Arrange
        using var service = CreateService();
        var request = new IssueSearchRequest { Owner = Owner };
        var progress = new RecordingProgress();
        var pages = new List<IssueSearchPage>();

        //Act
        await foreach (var page in service.SearchAsync(request, progress, TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        //Assert - the account may have nothing open, so only the shape is asserted
        pages.Count.Should().BeGreaterThan(0);
        pages[0].TotalCount.Should().NotBeNull();
        service.LastSearchRateLimit.Should().NotBeNull();
        service.LastSearchRateLimit.Limit.Should().BeGreaterThan(0);
        progress.PhaseSequence()[0].Should().Be(SearchPhase.Starting);
    }

    [Fact]
    public async Task the_assignee_search_answers_from_the_live_api()
    {
        //Arrange
        using var service = CreateService();
        var request = new IssueSearchRequest { Owner = Owner, Assignee = Owner };
        var pages = new List<IssueSearchPage>();

        //Act
        await foreach (var page in service.SearchAsync(request, null, TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        //Assert
        pages.Count.Should().BeGreaterThan(0);
        service.LastSearchRateLimit.Should().NotBeNull();
        service.LastSearchRateLimit.ResetAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [Fact]
    public async Task a_search_that_includes_closed_items_answers_from_the_live_api()
    {
        //Arrange
        using var service = CreateService();
        var request = new IssueSearchRequest { Owner = Owner, IncludeClosed = true };
        var items = 0;

        //Act
        await foreach (var page in service.SearchAsync(request, null, TestContext.Current.CancellationToken))
        {
            items += page.Items.Count;
            foreach (var item in page.Items)
            {
                item.RepositoryFullName.Should().StartWith(Owner + "/");
            }
        }

        //Assert
        items.Should().BeGreaterThanOrEqualTo(0);
        service.LastSearchRateLimit.Should().NotBeNull();
    }
}
