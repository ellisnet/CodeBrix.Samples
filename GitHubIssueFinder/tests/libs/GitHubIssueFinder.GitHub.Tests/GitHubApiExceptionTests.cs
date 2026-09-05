using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;
using System.Net;

namespace GitHubIssueFinder.GitHub.Tests;

public class GitHubApiExceptionTests
{
    private const string RequestUrl = "https://api.github.com/search/issues?q=user:ellisnet";

    [Fact]
    public void the_message_reaches_the_base_exception()
    {
        //Act
        var exception = new GitHubApiException("GitHub has no user or organisation named nobodyhere.");

        //Assert
        exception.Message.Should().Be("GitHub has no user or organisation named nobodyhere.");
        exception.InnerException.Should().BeNull();
        exception.RequestUrl.Should().BeNull();
        exception.RateLimitResetAt.Should().BeNull();
    }

    [Fact]
    public void the_cause_is_kept_when_one_is_given()
    {
        //Arrange
        var cause = new InvalidOperationException("unparseable");

        //Act
        var exception = new GitHubApiException("GitHub sent something this application could not read.", cause);

        //Assert
        exception.InnerException.Should().BeSameAs(cause);
        exception.Message.Should().Be("GitHub sent something this application could not read.");
    }

    [Fact]
    public void the_response_details_are_set_through_the_constructor()
    {
        //Arrange
        var resetAt = new DateTimeOffset(2026, 9, 4, 11, 4, 26, TimeSpan.Zero);

        //Act
        var exception = new GitHubApiException("GitHub refused the request: search quota exhausted.",
            HttpStatusCode.Forbidden, RequestUrl, "API rate limit exceeded for 203.0.113.4.", resetAt);

        //Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        exception.RequestUrl.Should().Be(RequestUrl);
        exception.GitHubMessage.Should().Be("API rate limit exceeded for 203.0.113.4.");
        exception.RateLimitResetAt.Should().Be(resetAt);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void the_reset_time_is_left_out_when_the_refusal_was_not_a_rate_limit()
    {
        //Act
        var exception = new GitHubApiException("GitHub rejected the search query.",
            HttpStatusCode.UnprocessableContent, RequestUrl, "Validation Failed");

        //Assert
        exception.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        exception.RateLimitResetAt.Should().BeNull();
        exception.GitHubMessage.Should().Be("Validation Failed");
    }

    [Fact]
    public void the_response_details_and_the_cause_can_be_carried_together()
    {
        //Arrange
        var cause = new TimeoutException("no answer");
        var resetAt = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

        //Act
        var exception = new GitHubApiException("GitHub did not answer in time.",
            HttpStatusCode.RequestTimeout, RequestUrl, null, resetAt, cause);

        //Assert
        exception.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        exception.RequestUrl.Should().Be(RequestUrl);
        exception.GitHubMessage.Should().BeNull();
        exception.RateLimitResetAt.Should().Be(resetAt);
        exception.InnerException.Should().BeSameAs(cause);
    }
}
