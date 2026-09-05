using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;

namespace GitHubIssueFinder.GitHub.Tests;

public class IssueSearchQueryBuilderTests
{
    private static IssueSearchRequest Request(string owner = "ellisnet", string assignee = null,
        bool includeClosed = false) =>
        new IssueSearchRequest { Owner = owner, Assignee = assignee, IncludeClosed = includeClosed };

    [Fact]
    public void an_empty_assignee_searches_for_unassigned_open_items()
    {
        //Arrange
        var request = Request();

        //Act
        var query = IssueSearchQueryBuilder.BuildQuery(request);

        //Assert
        query.Should().Be("user:ellisnet is:open no:assignee");
    }

    [Fact]
    public void a_blank_assignee_is_the_same_as_none_at_all()
    {
        //Assert
        IssueSearchQueryBuilder.BuildQuery(Request(assignee: string.Empty))
            .Should().Be("user:ellisnet is:open no:assignee");
        IssueSearchQueryBuilder.BuildQuery(Request(assignee: "   "))
            .Should().Be("user:ellisnet is:open no:assignee");
    }

    [Fact]
    public void a_named_assignee_searches_for_that_persons_open_items()
    {
        //Arrange
        var request = Request(assignee: "jeremy");

        //Act
        var query = IssueSearchQueryBuilder.BuildQuery(request);

        //Assert
        query.Should().Be("user:ellisnet is:open assignee:jeremy");
    }

    [Fact]
    public void including_closed_items_drops_the_open_qualifier()
    {
        //Assert
        IssueSearchQueryBuilder.BuildQuery(Request(includeClosed: true))
            .Should().Be("user:ellisnet no:assignee");
        IssueSearchQueryBuilder.BuildQuery(Request(assignee: "jeremy", includeClosed: true))
            .Should().Be("user:ellisnet assignee:jeremy");
    }

    [Fact]
    public void naming_a_repository_replaces_the_owner_qualifier()
    {
        //Assert
        IssueSearchQueryBuilder.BuildQuery(Request(), "mono/SkiaSharp")
            .Should().Be("repo:mono/SkiaSharp is:open no:assignee");
        IssueSearchQueryBuilder.BuildQuery(Request(assignee: "jeremy", includeClosed: true), "mono/SkiaSharp")
            .Should().Be("repo:mono/SkiaSharp assignee:jeremy");
    }

    [Fact]
    public void a_blank_repository_name_searches_the_whole_owner()
    {
        //Assert
        IssueSearchQueryBuilder.BuildQuery(Request(), "   ")
            .Should().Be("user:ellisnet is:open no:assignee");
    }

    [Fact]
    public void logins_typed_with_spaces_around_them_are_trimmed()
    {
        //Arrange
        var request = Request(owner: "  ellisnet ", assignee: " jeremy  ");

        //Act
        var query = IssueSearchQueryBuilder.BuildQuery(request, "  mono/skia  ");

        //Assert
        query.Should().Be("repo:mono/skia is:open assignee:jeremy");
    }

    [Fact]
    public void a_search_without_an_owner_is_refused()
    {
        //Act
        Action noOwner = () => IssueSearchQueryBuilder.BuildQuery(Request(owner: string.Empty));
        Action blankOwner = () => IssueSearchQueryBuilder.BuildQuery(Request(owner: "   "));
        Action nullOwner = () => IssueSearchQueryBuilder.BuildQuery(Request(owner: null));

        //Assert
        noOwner.Should().Throw<ArgumentException>();
        blankOwner.Should().Throw<ArgumentException>();
        nullOwner.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void a_missing_request_is_refused()
    {
        //Act
        Action act = () => IssueSearchQueryBuilder.BuildQuery(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void the_search_address_escapes_the_query_and_asks_for_full_pages()
    {
        //Act
        var url = IssueSearchQueryBuilder.BuildSearchUrl(Request(), 1);

        //Assert - spaces and colons are percent-escaped, one rule for the whole parameter
        url.Should().Be("search/issues?q=user%3Aellisnet%20is%3Aopen%20no%3Aassignee"
            + "&sort=updated&order=desc&per_page=100&page=1");
    }

    [Fact]
    public void the_search_address_carries_the_page_number()
    {
        //Act
        var url = IssueSearchQueryBuilder.BuildSearchUrl(Request(assignee: "jeremy"), 7, "mono/skia");

        //Assert
        url.Should().Be("search/issues?q=repo%3Amono%2Fskia%20is%3Aopen%20assignee%3Ajeremy"
            + "&sort=updated&order=desc&per_page=100&page=7");
    }

    [Fact]
    public void a_page_number_below_one_is_refused()
    {
        //Act
        Action act = () => IssueSearchQueryBuilder.BuildSearchUrl(Request(), 0);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void the_repository_listing_address_asks_only_for_the_owners_own_repositories()
    {
        //Act
        var url = IssueSearchQueryBuilder.BuildRepositoryListUrl(" ellisnet ", 2);

        //Assert
        url.Should().Be("users/ellisnet/repos?per_page=100&page=2&type=owner");
    }

    [Fact]
    public void the_repository_listing_address_needs_an_owner_and_a_real_page()
    {
        //Act
        Action noOwner = () => IssueSearchQueryBuilder.BuildRepositoryListUrl("  ", 1);
        Action noPage = () => IssueSearchQueryBuilder.BuildRepositoryListUrl("ellisnet", 0);

        //Assert
        noOwner.Should().Throw<ArgumentException>();
        noPage.Should().Throw<ArgumentOutOfRangeException>();
    }
}
