using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

//Builds the service over the stub handler and the fake clock, which is the only way the
//tests ever reach it.
internal static class TestService
{
    internal static readonly DateTimeOffset Start = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    internal static Harness Create(Action<GitHubSearchOptions> configure = null)
    {
        var options = new GitHubSearchOptions
        {
            UserAgent = "GitHubIssueFinder/test",
            BaseAddress = new Uri("https://api.github.com/"),
        };

        if (configure != null) { configure(options); }

        var stub = new StubHttpMessageHandler();
        var clock = new FakeTimeProvider(Start);
        return new Harness(new GitHubIssueSearchService(options, stub, clock), stub, clock, options);
    }

    internal static IssueSearchRequest Request(string owner = "acme", string assignee = null,
        bool includeClosed = false) =>
        new IssueSearchRequest { Owner = owner, Assignee = assignee, IncludeClosed = includeClosed };

    internal static async Task<List<IssueSearchPage>> CollectAsync(IGitHubIssueSearchService service,
        IssueSearchRequest request, IProgress<SearchProgress> progress, CancellationToken cancellationToken)
    {
        var pages = new List<IssueSearchPage>();
        await foreach (var page in service.SearchAsync(request, progress, cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            pages.Add(page);
        }

        return pages;
    }

    internal sealed class Harness : IDisposable
    {
        internal Harness(GitHubIssueSearchService service, StubHttpMessageHandler stub,
            FakeTimeProvider clock, GitHubSearchOptions options)
        {
            Service = service;
            Stub = stub;
            Clock = clock;
            Options = options;
        }

        internal GitHubIssueSearchService Service { get; }

        internal StubHttpMessageHandler Stub { get; }

        internal FakeTimeProvider Clock { get; }

        internal GitHubSearchOptions Options { get; }

        public void Dispose() => Service.Dispose();
    }
}
