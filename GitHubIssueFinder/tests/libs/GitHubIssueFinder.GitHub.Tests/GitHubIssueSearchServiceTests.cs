using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

public class GitHubIssueSearchServiceTests
{
    private static string SearchUrl(IssueSearchRequest request, int page, string repository = null) =>
        IssueSearchQueryBuilder.BuildSearchUrl(request, page, repository);

    private static string ReposUrl(string owner, int page) =>
        IssueSearchQueryBuilder.BuildRepositoryListUrl(owner, page);

    [Fact]
    public async Task every_request_carries_the_headers_github_asks_for()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchEmpty));

        //Act
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Assert
        var sent = harness.Stub.Requests.Single();
        sent.Method.Should().Be("GET");
        sent.Header("User-Agent").Should().Be("GitHubIssueFinder/test");
        sent.Header("Accept").Should().Be("application/vnd.github+json");
        sent.Header("X-GitHub-Api-Version").Should().Be("2022-11-28");
    }

    [Fact]
    public async Task the_address_that_goes_out_is_the_escaped_search_address()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet", assignee: "ellisnet");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchEmpty));

        //Act
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Assert
        harness.Stub.Requests.Single().Url.Should().Be(
            "https://api.github.com/search/issues?q=user%3Aellisnet%20is%3Aopen%20assignee%3Aellisnet"
            + "&sort=updated&order=desc&per_page=100&page=1");
    }

    [Fact]
    public async Task a_total_that_fits_one_page_is_read_in_one_call()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchPage));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Count.Should().Be(1);
        pages[0].PageNumber.Should().Be(1);
        pages[0].TotalCount.Should().Be(5);
        pages[0].Items.Count.Should().Be(5);
        pages[0].RepositoryFullName.Should().BeNull();
        harness.Stub.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task paging_carries_on_until_the_total_is_covered()
    {
        //Arrange - a total of 250 needs three pages of a hundred, however few items arrive
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(250, 2, firstNumber: 1));
        harness.Stub.Respond(SearchUrl(request, 2), JsonBuilders.SearchPage(250, 2, firstNumber: 3));
        harness.Stub.Respond(SearchUrl(request, 3), JsonBuilders.SearchPage(250, 2, firstNumber: 5));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Count.Should().Be(3);
        pages.Select(p => p.PageNumber).Should().Equal(new[] { 1, 2, 3 });
        pages.Sum(p => p.Items.Count).Should().Be(6);
        harness.Stub.RequestCount.Should().Be(3);
    }

    [Fact]
    public async Task paging_stops_as_soon_as_a_page_comes_back_empty()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(250, 2));
        harness.Stub.Respond(SearchUrl(request, 2), JsonBuilders.SearchPage(250, 0));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Count.Should().Be(2);
        harness.Stub.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task a_partial_result_is_reported_as_partial()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(3, 3, incompleteResults: true));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Single().IncompleteResults.Should().BeTrue();
    }

    [Fact]
    public async Task progress_runs_from_starting_through_fetching_to_completed()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var progress = new RecordingProgress();
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(250, 2, firstNumber: 1));
        harness.Stub.Respond(SearchUrl(request, 2), JsonBuilders.SearchPage(250, 2, firstNumber: 3));
        harness.Stub.Respond(SearchUrl(request, 3), JsonBuilders.SearchPage(250, 2, firstNumber: 5));

        //Act
        await TestService.CollectAsync(harness.Service, request, progress, TestContext.Current.CancellationToken);

        //Assert
        progress.PhaseSequence().Should().Equal(new[]
        {
            SearchPhase.Starting, SearchPhase.Fetching, SearchPhase.Completed,
        });

        var fetching = progress.Of(SearchPhase.Fetching);
        fetching.Count.Should().Be(3);
        fetching[0].Fetched.Should().Be(2);
        fetching[0].PagesFetched.Should().Be(1);
        fetching[2].Fetched.Should().Be(6);
        fetching[2].PagesFetched.Should().Be(3);
        fetching[2].Total.Should().Be(250);

        var completed = progress.Of(SearchPhase.Completed).Single();
        completed.Fetched.Should().Be(6);
        completed.ToString().Should().Be("Done: 6 items.");
    }

    [Fact]
    public async Task a_search_runs_perfectly_well_with_nobody_watching()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchPage));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Single().Items.Count.Should().Be(5);
    }

    [Fact]
    public void a_search_without_a_request_or_an_owner_is_refused_before_it_starts()
    {
        //Arrange
        using var harness = TestService.Create();

        //Act
        Action noRequest = () => harness.Service.SearchAsync(null);
        Action noOwner = () => harness.Service.SearchAsync(TestService.Request(owner: "  "));

        //Assert
        noRequest.Should().Throw<ArgumentNullException>();
        noOwner.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task cancelling_part_way_through_stops_the_search_where_it_stands()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(250, 2, firstNumber: 1));
        harness.Stub.Respond(SearchUrl(request, 2), JsonBuilders.SearchPage(250, 2, firstNumber: 3));
        using var source = new CancellationTokenSource();
        var enumerator = harness.Service.SearchAsync(request, null, source.Token)
            .GetAsyncEnumerator(source.Token);

        //Act
        var first = await enumerator.MoveNextAsync();
        source.Cancel();
        async Task Act() => await enumerator.MoveNextAsync();

        //Assert
        first.Should().BeTrue();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        harness.Stub.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task an_owner_github_has_never_heard_of_is_said_in_plain_words()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "zzqqnotarealowner12345");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.UnknownOwnerError),
            HttpStatusCode.UnprocessableEntity);

        //Act
        var error = await Assert.ThrowsAsync<GitHubApiException>(() =>
            TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken));

        //Assert
        error.Message.Should().Be("GitHub has no user or organization named 'zzqqnotarealowner12345'.");
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.GitHubMessage.Should().StartWith("Validation Failed; The listed users and repositories");
        error.RequestUrl.Should().StartWith("https://api.github.com/search/issues?q=");
        error.RateLimitResetAt.Should().BeNull();
    }

    [Fact]
    public async Task any_other_refusal_carries_the_status_and_what_github_said()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1),
            "{\"message\":\"Server Error\",\"errors\":[{\"message\":\"try later\"}]}",
            HttpStatusCode.InternalServerError);

        //Act
        var error = await Assert.ThrowsAsync<GitHubApiException>(() =>
            TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken));

        //Assert
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.GitHubMessage.Should().Be("Server Error; try later");
        error.Message.Should().StartWith("GitHub answered 500 (InternalServerError) for ");
        error.Message.Should().EndWith(" Server Error; try later");
    }

    [Fact]
    public async Task a_rate_limit_refusal_is_waited_out_and_tried_once_more()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var url = SearchUrl(request, 1);
        var resetAt = TestService.Start.AddSeconds(30);
        harness.Stub.Respond(url, Fixture.Read(Fixture.RateLimitedError), HttpStatusCode.Forbidden,
            JsonBuilders.RateLimitHeaders(10, 0, resetAt));
        harness.Stub.Respond(url, JsonBuilders.SearchPage(2, 2), HttpStatusCode.OK,
            JsonBuilders.RateLimitHeaders(10, 9, resetAt.AddMinutes(1)));
        var progress = new RecordingProgress();

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, progress,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Single().Items.Count.Should().Be(2);
        harness.Stub.RequestCount.Should().Be(2);
        harness.Clock.Elapsed.Should().Be(TimeSpan.FromSeconds(31));

        var waits = progress.Of(SearchPhase.WaitingForQuota);
        waits.Count.Should().Be(31);
        waits[0].WaitUntil.Should().Be(resetAt.AddSeconds(1));
        waits[0].WaitRemaining.Should().Be(TimeSpan.FromSeconds(31));
        waits[0].ToString().Should().Be("Fetched 0 · waiting 31 s for the search quota to reset");
    }

    [Fact]
    public async Task a_refusal_that_comes_back_a_second_time_gives_up_and_says_when_the_pool_refills()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var url = SearchUrl(request, 1);
        var resetAt = TestService.Start.AddSeconds(30);
        var headers = JsonBuilders.RateLimitHeaders(10, 0, resetAt);
        harness.Stub.Respond(url, Fixture.Read(Fixture.RateLimitedError), HttpStatusCode.Forbidden, headers);
        harness.Stub.Respond(url, Fixture.Read(Fixture.RateLimitedError), HttpStatusCode.Forbidden, headers);

        //Act
        var error = await Assert.ThrowsAsync<GitHubApiException>(() =>
            TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken));

        //Assert - tried exactly twice, then gave up with the moment the pool refills
        harness.Stub.RequestCount.Should().Be(2);
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.RateLimitResetAt.Should().Be(resetAt);
        error.Message.Should().StartWith("GitHub's search rate limit is still exhausted");
        error.GitHubMessage.Should().StartWith("API rate limit exceeded");
    }

    [Fact]
    public async Task a_retry_after_header_decides_how_long_the_wait_is()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var url = SearchUrl(request, 1);
        var refusal = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["retry-after"] = "12",
        };
        harness.Stub.Respond(url, Fixture.Read(Fixture.RateLimitedError), HttpStatusCode.TooManyRequests, refusal);
        harness.Stub.Respond(url, JsonBuilders.SearchPage(1, 1));

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Single().Items.Count.Should().Be(1);
        harness.Clock.Elapsed.Should().Be(TimeSpan.FromSeconds(12));
    }

    [Fact]
    public async Task the_snapshots_come_from_the_response_headers()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var resetAt = TestService.Start.AddSeconds(45);
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(1, 1), HttpStatusCode.OK,
            JsonBuilders.RateLimitHeaders(10, 7, resetAt));

        //Act
        harness.Service.LastSearchRateLimit.Should().BeNull();
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Assert
        var snapshot = harness.Service.LastSearchRateLimit;
        snapshot.Limit.Should().Be(10);
        snapshot.Remaining.Should().Be(7);
        snapshot.Ceiling.Should().Be(9);
        snapshot.ResetAt.Should().Be(resetAt);
        harness.Service.LastCoreRateLimit.Should().BeNull();
    }

    [Fact]
    public async Task a_response_with_no_rate_limit_headers_leaves_the_last_snapshot_alone()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request();
        var resetAt = TestService.Start.AddSeconds(45);
        harness.Stub.Respond(SearchUrl(request, 1), JsonBuilders.SearchPage(250, 2), HttpStatusCode.OK,
            JsonBuilders.RateLimitHeaders(10, 8, resetAt));
        harness.Stub.Respond(SearchUrl(request, 2), JsonBuilders.SearchPage(250, 0));

        //Act
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Assert - the limit and the reset moment came from the first response and survived the
        //second, which carried no headers at all. What is left counts down the application's own
        //ceiling of nine rather than GitHub's ten, and two pages have been fetched.
        harness.Service.LastSearchRateLimit.Limit.Should().Be(10);
        harness.Service.LastSearchRateLimit.Remaining.Should().Be(7);
        harness.Service.LastSearchRateLimit.ResetAt.Should().Be(resetAt);
    }

    [Fact]
    public async Task past_the_thousand_result_cap_the_search_starts_again_one_repository_at_a_time()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap));
        harness.Stub.Respond(ReposUrl("ellisnet", 1), Fixture.Read(Fixture.RepositoriesPage));
        foreach (var repository in new[] { "ellisnet/alpha", "ellisnet/Middle", "ellisnet/Zebra" })
        {
            harness.Stub.Respond(SearchUrl(request, 1, repository), JsonBuilders.SearchPage(2, 2, repository));
        }

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert - the archived one, the one with issues turned off and the quiet one are skipped,
        //and what is left is searched in name order
        pages.Select(p => p.RepositoryFullName).Should().Equal(new[]
        {
            "ellisnet/alpha", "ellisnet/Middle", "ellisnet/Zebra",
        });
        pages.Sum(p => p.Items.Count).Should().Be(6);
        harness.Stub.RequestCount.Should().Be(5);
    }

    [Fact]
    public async Task the_repository_plan_keeps_the_total_the_whole_owner_search_reported()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet");
        var progress = new RecordingProgress();
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap));
        harness.Stub.Respond(ReposUrl("ellisnet", 1), Fixture.Read(Fixture.RepositoriesPage));
        foreach (var repository in new[] { "ellisnet/alpha", "ellisnet/Middle", "ellisnet/Zebra" })
        {
            harness.Stub.Respond(SearchUrl(request, 1, repository), JsonBuilders.SearchPage(2, 2, repository));
        }

        //Act
        await TestService.CollectAsync(harness.Service, request, progress, TestContext.Current.CancellationToken);

        //Assert
        progress.PhaseSequence().Should().Equal(new[]
        {
            SearchPhase.Starting, SearchPhase.ListingRepositories, SearchPhase.Fetching, SearchPhase.Completed,
        });
        progress.Of(SearchPhase.ListingRepositories).Single().PagesFetched.Should().Be(1);
        progress.Of(SearchPhase.Fetching).Last().Total.Should().Be(5340);
        progress.Of(SearchPhase.Completed).Single().ToString().Should().Be("Done: 6 items.");
    }

    [Fact]
    public async Task including_closed_items_keeps_a_repository_with_nothing_open()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet", includeClosed: true);
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap));
        harness.Stub.Respond(ReposUrl("ellisnet", 1), Fixture.Read(Fixture.RepositoriesPage));
        foreach (var repository in new[] { "ellisnet/alpha", "ellisnet/Middle", "ellisnet/quiet", "ellisnet/Zebra" })
        {
            harness.Stub.Respond(SearchUrl(request, 1, repository), JsonBuilders.SearchPage(1, 1, repository));
        }

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Select(p => p.RepositoryFullName).Should().Equal(new[]
        {
            "ellisnet/alpha", "ellisnet/Middle", "ellisnet/quiet", "ellisnet/Zebra",
        });
    }

    [Fact]
    public async Task the_repository_listing_asks_for_the_next_page_while_the_pages_are_full()
    {
        //Arrange - a hundred archived repositories fill the first page and are all dropped
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap));
        harness.Stub.Respond(ReposUrl("ellisnet", 1),
            JsonBuilders.ManyRepositories("ellisnet", 100, 1, archived: true));
        harness.Stub.Respond(ReposUrl("ellisnet", 2), JsonBuilders.Repositories("ellisnet/kept"));
        harness.Stub.Respond(SearchUrl(request, 1, "ellisnet/kept"), JsonBuilders.SearchPage(1, 1, "ellisnet/kept"));
        var progress = new RecordingProgress();

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, progress,
            TestContext.Current.CancellationToken);

        //Assert
        pages.Single().RepositoryFullName.Should().Be("ellisnet/kept");
        harness.Stub.PathsCalled().Should().Equal(new[]
        {
            SearchUrl(request, 1),
            ReposUrl("ellisnet", 1),
            ReposUrl("ellisnet", 2),
            SearchUrl(request, 1, "ellisnet/kept"),
        });
        progress.Of(SearchPhase.ListingRepositories).Select(p => p.PagesFetched).Should().Equal(new[] { 1, 2 });
    }

    [Fact]
    public async Task one_repository_with_more_matches_than_github_will_serve_stops_at_the_last_page()
    {
        //Arrange - the repository reports far more than a thousand matches, but GitHub will
        //only ever serve ten pages of them, and page eleven answers with a refusal
        using var harness = TestService.Create(options => options.SearchCeilingPerMinute = 50);
        var request = TestService.Request(owner: "ellisnet");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap));
        harness.Stub.Respond(ReposUrl("ellisnet", 1), JsonBuilders.Repositories("ellisnet/busy"));
        for (var page = 1; page <= 10; page++)
        {
            harness.Stub.Respond(SearchUrl(request, page, "ellisnet/busy"),
                JsonBuilders.SearchPage(5340, 2, "ellisnet/busy", firstNumber: page * 10));
        }

        //Act
        var pages = await TestService.CollectAsync(harness.Service, request, null,
            TestContext.Current.CancellationToken);

        //Assert - ten pages and no eleventh, so the refusal never happens
        pages.Count.Should().Be(10);
        pages.Last().PageNumber.Should().Be(10);
        harness.Stub.PathsCalled().Should().NotContain(SearchUrl(request, 11, "ellisnet/busy"));
    }

    [Fact]
    public async Task the_repository_listing_spends_the_core_quota_and_the_searches_the_search_quota()
    {
        //Arrange
        using var harness = TestService.Create();
        var request = TestService.Request(owner: "ellisnet");
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchOverCap), HttpStatusCode.OK,
            JsonBuilders.RateLimitHeaders(10, 9, TestService.Start.AddMinutes(1)));
        harness.Stub.Respond(ReposUrl("ellisnet", 1), JsonBuilders.Repositories("ellisnet/kept"),
            HttpStatusCode.OK, JsonBuilders.RateLimitHeaders(60, 58, TestService.Start.AddHours(1), "core"));
        harness.Stub.Respond(SearchUrl(request, 1, "ellisnet/kept"), JsonBuilders.SearchPage(1, 1, "ellisnet/kept"),
            HttpStatusCode.OK, JsonBuilders.RateLimitHeaders(10, 8, TestService.Start.AddMinutes(1)));

        //Act
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Assert - two searches and one repository listing were made, and each pool reports what
        //is left of the application's own ceiling rather than of GitHub's larger allowance
        harness.Service.LastSearchRateLimit.Remaining.Should().Be(7);
        harness.Service.LastSearchRateLimit.Ceiling.Should().Be(9);
        harness.Service.LastCoreRateLimit.Remaining.Should().Be(58);
        harness.Service.LastCoreRateLimit.Ceiling.Should().Be(59);
    }

    [Fact]
    public async Task an_address_nobody_answered_is_reported_with_the_address_in_it()
    {
        //Arrange - nothing is routed, so the stub answers every call with a 404
        using var harness = TestService.Create();
        var request = TestService.Request();

        //Act
        var error = await Assert.ThrowsAsync<GitHubApiException>(() =>
            TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken));

        //Assert
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.GitHubMessage.Should().Contain("search/issues");
    }

    [Fact]
    public async Task the_handler_the_service_was_given_is_never_disposed_by_it()
    {
        //Arrange
        var harness = TestService.Create();
        var request = TestService.Request();
        harness.Stub.Respond(SearchUrl(request, 1), Fixture.Read(Fixture.SearchEmpty));
        await TestService.CollectAsync(harness.Service, request, null, TestContext.Current.CancellationToken);

        //Act
        harness.Service.Dispose();
        harness.Service.Dispose();

        //Assert - the connection pool belongs to whoever handed it in
        harness.Stub.IsDisposed.Should().BeFalse();
        harness.Service.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void searching_after_the_service_is_disposed_is_refused()
    {
        //Arrange
        var harness = TestService.Create();
        harness.Service.Dispose();

        //Act
        Action act = () => harness.Service.SearchAsync(TestService.Request());

        //Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void the_service_built_for_the_live_api_owns_its_own_connection_pool()
    {
        //Arrange
        var service = new GitHubIssueSearchService(new GitHubSearchOptions());

        //Act
        var handler = service.Handler;
        service.Dispose();

        //Assert
        handler.Should().NotBeNull();
        service.OwnsHandler.Should().BeTrue();
        service.Handler.Should().BeNull();
    }
}
