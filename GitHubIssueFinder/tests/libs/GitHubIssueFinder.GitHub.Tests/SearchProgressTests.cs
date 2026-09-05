using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;

namespace GitHubIssueFinder.GitHub.Tests;

public class SearchProgressTests
{
    //The middle dot is written as an escape so the test spells the separator
    //independently of the file the sentence is built in.
    private const string Dot = " \u00B7 ";

    private static SearchProgress Report(SearchPhase phase, int fetched = 0, int? total = null,
        int pages = 0, TimeSpan? waitRemaining = null) =>
        new SearchProgress(phase, fetched, total, pages, waitRemaining, null, null, null);

    [Fact]
    public void starting_reports_that_github_is_being_contacted()
    {
        //Act
        var text = Report(SearchPhase.Starting).ToString();

        //Assert
        text.Should().Be("Contacting GitHub...");
    }

    [Fact]
    public void fetching_reports_the_running_count_and_the_page()
    {
        //Act
        var text = Report(SearchPhase.Fetching, fetched: 300, total: 1240, pages: 4).ToString();

        //Assert
        text.Should().Be("Fetched 300 of 1,240" + Dot + "page 4");
    }

    [Fact]
    public void fetching_before_the_total_is_known_reports_the_count_only()
    {
        //Act
        var text = Report(SearchPhase.Fetching, fetched: 100, pages: 1).ToString();

        //Assert
        text.Should().Be("Fetched 100" + Dot + "page 1");
    }

    [Fact]
    public void waiting_reports_the_seconds_left_on_the_quota()
    {
        //Act
        var text = Report(SearchPhase.WaitingForQuota, fetched: 900, total: 1240, pages: 9,
            waitRemaining: TimeSpan.FromSeconds(42)).ToString();

        //Assert
        text.Should().Be("Fetched 900 of 1,240" + Dot + "waiting 42 s for the search quota to reset");
    }

    [Fact]
    public void waiting_rounds_a_part_second_up_so_the_count_never_reads_zero()
    {
        //Act
        var text = Report(SearchPhase.WaitingForQuota, fetched: 900, total: 1240, pages: 9,
            waitRemaining: TimeSpan.FromMilliseconds(41200)).ToString();

        //Assert
        text.Should().Be("Fetched 900 of 1,240" + Dot + "waiting 42 s for the search quota to reset");
    }

    [Fact]
    public void waiting_with_no_known_remainder_leaves_the_seconds_out()
    {
        //Act
        var text = Report(SearchPhase.WaitingForQuota, fetched: 900, total: 1240, pages: 9).ToString();

        //Assert
        text.Should().Be("Fetched 900 of 1,240" + Dot + "waiting for the search quota to reset");
    }

    [Fact]
    public void listing_repositories_reports_how_many_pages_have_been_read()
    {
        //Act
        var one = Report(SearchPhase.ListingRepositories, pages: 1).ToString();
        var several = Report(SearchPhase.ListingRepositories, pages: 2).ToString();

        //Assert - the first page of a repository listing is one page, not one pages
        one.Should().Be("Listing repositories (1 page so far)");
        several.Should().Be("Listing repositories (2 pages so far)");
    }

    [Fact]
    public void completed_reports_the_item_count()
    {
        //Act
        var text = Report(SearchPhase.Completed, fetched: 1240, total: 1240, pages: 13).ToString();

        //Assert - the repository count and the elapsed time belong to the caller
        text.Should().Be("Done: 1,240 items.");
    }

    [Fact]
    public void cancelled_reports_how_far_the_search_got()
    {
        //Act
        var text = Report(SearchPhase.Cancelled, fetched: 300, total: 1240, pages: 4).ToString();

        //Assert
        text.Should().Be("Cancelled after 300 of 1,240.");
    }

    [Fact]
    public void cancelled_before_the_total_is_known_reports_the_count_only()
    {
        //Act
        var text = Report(SearchPhase.Cancelled, fetched: 300, pages: 4).ToString();

        //Assert
        text.Should().Be("Cancelled after 300.");
    }

    [Fact]
    public void failed_reports_a_plain_sentence_with_no_detail()
    {
        //Act
        var text = Report(SearchPhase.Failed, fetched: 300, total: 1240, pages: 4).ToString();

        //Assert - the detail goes on the status line from the exception, not from here
        text.Should().Be("Search failed.");
    }

    [Fact]
    public void thousands_are_grouped_the_same_way_in_every_culture()
    {
        //Act
        var text = Report(SearchPhase.Completed, fetched: 1234567).ToString();

        //Assert
        text.Should().Be("Done: 1,234,567 items.");
    }

    [Fact]
    public void a_rate_limit_snapshot_compares_by_value()
    {
        //Arrange
        var resetAt = new DateTimeOffset(2026, 9, 4, 11, 4, 26, TimeSpan.Zero);

        //Act
        var first = new RateLimitSnapshot(10, 7, 9, resetAt);
        var second = new RateLimitSnapshot(10, 7, 9, resetAt);

        //Assert
        first.Should().Be(second);
        first.Remaining.Should().Be(7);
        first.Ceiling.Should().Be(9);
    }
}
