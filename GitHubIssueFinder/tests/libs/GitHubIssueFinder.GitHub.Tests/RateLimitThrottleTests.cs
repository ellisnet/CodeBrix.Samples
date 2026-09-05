using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

public class RateLimitThrottleTests
{
    private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan Minute = TimeSpan.FromMinutes(1);

    private static HttpResponseHeaders Headers(int limit, int remaining, DateTimeOffset resetAt)
    {
        var response = new HttpResponseMessage();
        foreach (var header in JsonBuilders.RateLimitHeaders(limit, remaining, resetAt))
        {
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return response.Headers;
    }

    private static HttpResponseHeaders NoHeaders() => new HttpResponseMessage().Headers;

    [Fact]
    public async Task calls_under_the_ceiling_are_never_held_back()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 3, Minute, clock);

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Assert
        clock.Elapsed.Should().Be(TimeSpan.Zero);
        throttle.IssuedInWindow.Should().Be(3);
    }

    [Fact]
    public async Task the_ceiling_holds_the_next_call_until_the_window_has_moved_on()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 2, Minute, clock);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Assert - the third call waited for the oldest of the first two to leave the window
        clock.Elapsed.Should().Be(Minute);
        throttle.IssuedInWindow.Should().Be(1);
    }

    [Fact]
    public async Task a_call_that_has_left_the_window_stops_counting()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 1, Minute, clock);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Act - a minute passes with nobody calling
        clock.Advance(TimeSpan.FromSeconds(61));
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Assert - no wait was needed, so the clock only moved by the minute the test gave it
        clock.Elapsed.Should().Be(TimeSpan.FromSeconds(61));
        throttle.IssuedInWindow.Should().Be(1);
    }

    [Fact]
    public async Task one_call_left_in_the_pool_waits_for_the_pool_to_refill()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        throttle.UpdateFrom(Headers(10, 1, Start.AddSeconds(30)));

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Assert - thirty seconds to the reset, and one second of grace after it
        clock.Elapsed.Should().Be(TimeSpan.FromSeconds(31));
    }

    [Fact]
    public async Task a_pool_that_has_already_reset_is_not_waited_for()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        throttle.UpdateFrom(Headers(10, 0, Start.AddSeconds(-5)));

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);

        //Assert
        clock.Elapsed.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task a_wait_says_how_much_of_it_is_left_once_a_second()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        throttle.UpdateFrom(Headers(10, 1, Start.AddSeconds(30)));
        var ticks = new List<TimeSpan>();
        var until = new List<DateTimeOffset>();

        //Act
        await throttle.AcquireAsync((remaining, ends) => { ticks.Add(remaining); until.Add(ends); },
            TestContext.Current.CancellationToken);

        //Assert
        ticks.Count.Should().Be(31);
        ticks[0].Should().Be(TimeSpan.FromSeconds(31));
        ticks[30].Should().Be(TimeSpan.FromSeconds(1));
        until[0].Should().Be(Start.AddSeconds(31));
    }

    [Fact]
    public void the_response_headers_replace_the_snapshot()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);

        //Act
        throttle.UpdateFrom(Headers(10, 7, Start.AddSeconds(45)));

        //Assert
        throttle.Snapshot.Should().NotBeNull();
        throttle.Snapshot.Limit.Should().Be(10);
        throttle.Snapshot.Remaining.Should().Be(7);
        throttle.Snapshot.ResetAt.Should().Be(Start.AddSeconds(45));
    }

    [Fact]
    public void the_snapshot_carries_the_ceiling_the_application_set_itself()
    {
        //Arrange
        var throttle = new RateLimitThrottle("core", 59, TimeSpan.FromHours(1), new FakeTimeProvider(Start));

        //Act
        throttle.UpdateFrom(Headers(60, 60, Start.AddHours(1)));

        //Assert - GitHub's limit and the application's own ceiling are both on the snapshot
        throttle.Snapshot.Limit.Should().Be(60);
        throttle.Snapshot.Ceiling.Should().Be(59);
    }

    [Fact]
    public void a_response_carrying_no_rate_limit_headers_keeps_the_snapshot_it_had()
    {
        //Arrange
        var throttle = new RateLimitThrottle("search", 9, Minute, new FakeTimeProvider(Start));
        throttle.UpdateFrom(Headers(10, 7, Start.AddSeconds(45)));

        //Act
        throttle.UpdateFrom(NoHeaders());

        //Assert
        throttle.Snapshot.Remaining.Should().Be(7);
    }

    [Fact]
    public void the_snapshot_is_empty_until_the_first_response()
    {
        //Arrange
        var throttle = new RateLimitThrottle("search", 9, Minute, new FakeTimeProvider(Start));

        //Assert
        throttle.Snapshot.Should().BeNull();
    }

    [Fact]
    public void a_retry_after_header_is_read_as_seconds_or_as_a_moment()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        var seconds = new HttpResponseMessage();
        seconds.Headers.TryAddWithoutValidation("retry-after", "42");
        var moment = new HttpResponseMessage();
        moment.Headers.TryAddWithoutValidation("retry-after", Start.AddSeconds(20).UtcDateTime.ToString("R"));

        //Act
        var readSeconds = throttle.TryReadRetryAfter(seconds.Headers, out var secondsDelay);
        var readMoment = throttle.TryReadRetryAfter(moment.Headers, out var momentDelay);
        var readNothing = throttle.TryReadRetryAfter(NoHeaders(), out _);

        //Assert
        readSeconds.Should().BeTrue();
        secondsDelay.Should().Be(TimeSpan.FromSeconds(42));
        readMoment.Should().BeTrue();
        momentDelay.Should().Be(TimeSpan.FromSeconds(20));
        readNothing.Should().BeFalse();
    }

    [Fact]
    public async Task cancelling_stops_a_wait_that_is_already_running()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        throttle.UpdateFrom(Headers(10, 0, Start.AddHours(1)));
        using var source = new CancellationTokenSource();
        source.Cancel();

        //Act
        Task Act() => throttle.AcquireAsync(null, source.Token);

        //Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    [Fact]
    public async Task the_snapshot_counts_down_the_ceiling_the_application_keeps_to()
    {
        //Arrange - GitHub allows ten a minute and the application allows itself nine, so what
        //GitHub reports is always one ahead of what the application has actually spent
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        throttle.UpdateFrom(Headers(10, 9, Start.AddSeconds(60)));

        //Assert - one call has been made, so eight of the application's nine are left
        throttle.Reported.Remaining.Should().Be(9);
        throttle.Snapshot.Remaining.Should().Be(8);
        throttle.Snapshot.Ceiling.Should().Be(9);
    }

    [Fact]
    public async Task the_snapshot_reaches_zero_exactly_when_the_throttle_starts_waiting()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 3, Minute, clock);

        //Act - spend the whole self-imposed budget
        for (var call = 0; call < 3; call++)
        {
            await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
            throttle.UpdateFrom(Headers(10, 9 - call, Start.AddSeconds(60)));
        }

        //Assert - GitHub still says seven are left, but the application has none
        throttle.Reported.Remaining.Should().Be(7);
        throttle.Snapshot.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task the_snapshot_climbs_back_as_the_window_slides()
    {
        //Arrange
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 3, Minute, clock);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        throttle.UpdateFrom(Headers(10, 9, Start.AddSeconds(60)));
        throttle.Snapshot.Remaining.Should().Be(2);

        //Act - the call ages out of the window
        clock.Advance(TimeSpan.FromSeconds(61));

        //Assert
        throttle.Snapshot.Remaining.Should().Be(3);
    }

    [Fact]
    public void past_the_reset_moment_the_snapshot_reports_a_full_pool_again()
    {
        //Arrange - GitHub said one call was left, and its window has since refilled
        var clock = new FakeTimeProvider(Start);
        var throttle = new RateLimitThrottle("search", 9, Minute, clock);
        throttle.UpdateFrom(Headers(10, 1, Start.AddSeconds(30)));
        throttle.Snapshot.Remaining.Should().Be(1);

        //Act
        clock.Advance(TimeSpan.FromSeconds(31));

        //Assert - the number the last response carried is stale, so the ceiling is what is left
        throttle.Snapshot.Remaining.Should().Be(9);
    }

    [Fact]
    public async Task a_run_at_the_ceiling_on_the_real_clock_really_waits()
    {
        //Arrange - a one second window, so the wall clock proves the point in a moment
        var throttle = new RateLimitThrottle("search", 1, TimeSpan.FromSeconds(1), TimeProvider.System);
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        var watch = Stopwatch.StartNew();

        //Act
        await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
        watch.Stop();

        //Assert - the second call waited out the window instead of going straight through
        watch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(900));
    }
}
