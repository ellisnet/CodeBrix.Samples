using GitHubIssueFinder.GitHub;
using Microsoft.Extensions.DependencyInjection;
using SilverAssertions;
using Xunit;
using System;

namespace GitHubIssueFinder.GitHub.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void add_github_issue_search_refuses_a_null_collection()
    {
        //Arrange
        IServiceCollection services = null;

        //Act
        Action act = () => services.AddGitHubIssueSearch();

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void add_github_issue_search_returns_the_same_collection()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        var returned = services.AddGitHubIssueSearch();

        //Assert
        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void add_github_issue_search_registers_one_shared_service()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddGitHubIssueSearch();

        //Act
        using var provider = services.BuildServiceProvider();
        var first = provider.GetService<IGitHubIssueSearchService>();
        var second = provider.GetService<IGitHubIssueSearchService>();

        //Assert - one connection pool and one pair of throttles for the whole application
        first.Should().NotBeNull();
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void add_github_issue_search_hands_the_supplied_options_to_the_service()
    {
        //Arrange
        var options = new GitHubSearchOptions { UserAgent = "GitHubIssueFinder/test" };
        var services = new ServiceCollection();
        services.AddGitHubIssueSearch(options);

        //Act
        using var provider = services.BuildServiceProvider();
        var service = (GitHubIssueSearchService)provider.GetService<IGitHubIssueSearchService>();

        //Assert
        service.Options.Should().BeSameAs(options);
    }

    [Fact]
    public void add_github_issue_search_falls_back_to_default_options()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddGitHubIssueSearch();

        //Act
        using var provider = services.BuildServiceProvider();
        var service = (GitHubIssueSearchService)provider.GetService<IGitHubIssueSearchService>();

        //Assert - GitHub refuses a request that carries no User-Agent, so there is always one
        service.Options.Should().NotBeNull();
        service.Options.UserAgent.Should().StartWith("GitHubIssueFinder/");
        service.Options.BaseAddress.Should().Be(new Uri("https://api.github.com/"));
        service.Options.SearchCeilingPerMinute.Should().Be(9);
        service.Options.CoreCeilingPerHour.Should().Be(59);
        service.Options.RequestTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void the_container_disposes_the_registered_service()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddGitHubIssueSearch();
        var provider = services.BuildServiceProvider();
        var service = (GitHubIssueSearchService)provider.GetService<IGitHubIssueSearchService>();

        //Act
        provider.Dispose();

        //Assert
        service.IsDisposed.Should().BeTrue();
    }
}
