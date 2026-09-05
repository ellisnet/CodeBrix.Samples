using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;

namespace GitHubIssueFinder.GitHub.Tests;

public class RelativeTimeTests
{
    //A fixed moment to measure back from, so nothing here depends on the wall clock.
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    private static string Ago(TimeSpan elapsed) => RelativeTime.Describe(Now - elapsed, Now);

    [Fact]
    public void moments_under_a_minute_or_in_the_future_read_just_now()
    {
        //Assert
        Ago(TimeSpan.Zero).Should().Be("just now");
        Ago(TimeSpan.FromSeconds(30)).Should().Be("just now");
        Ago(TimeSpan.FromSeconds(59)).Should().Be("just now");
        RelativeTime.Describe(Now.AddMinutes(10), Now).Should().Be("just now");
    }

    [Fact]
    public void one_minute_is_singular()
    {
        //Assert
        Ago(TimeSpan.FromSeconds(60)).Should().Be("1 minute ago");
        Ago(TimeSpan.FromSeconds(119)).Should().Be("1 minute ago");
    }

    [Fact]
    public void several_minutes_are_counted()
    {
        //Assert
        Ago(TimeSpan.FromMinutes(5)).Should().Be("5 minutes ago");
        Ago(TimeSpan.FromMinutes(59)).Should().Be("59 minutes ago");
    }

    [Fact]
    public void one_hour_is_singular()
    {
        //Assert
        Ago(TimeSpan.FromMinutes(60)).Should().Be("1 hour ago");
        Ago(TimeSpan.FromMinutes(119)).Should().Be("1 hour ago");
    }

    [Fact]
    public void several_hours_are_counted()
    {
        //Assert
        Ago(TimeSpan.FromHours(2)).Should().Be("2 hours ago");
        Ago(TimeSpan.FromHours(23)).Should().Be("23 hours ago");
    }

    [Fact]
    public void one_day_reads_yesterday()
    {
        //Assert
        Ago(TimeSpan.FromHours(24)).Should().Be("yesterday");
        Ago(TimeSpan.FromHours(47)).Should().Be("yesterday");
    }

    [Fact]
    public void days_are_counted_up_to_six()
    {
        //Assert
        Ago(TimeSpan.FromDays(2)).Should().Be("2 days ago");
        Ago(TimeSpan.FromDays(6)).Should().Be("6 days ago");
    }

    [Fact]
    public void seven_days_reads_last_week()
    {
        //Assert
        Ago(TimeSpan.FromDays(7)).Should().Be("last week");
        Ago(TimeSpan.FromDays(13)).Should().Be("last week");
    }

    [Fact]
    public void weeks_are_counted_up_to_four()
    {
        //Assert
        Ago(TimeSpan.FromDays(14)).Should().Be("2 weeks ago");
        Ago(TimeSpan.FromDays(28)).Should().Be("4 weeks ago");
    }

    [Fact]
    public void a_gap_past_four_weeks_reads_last_month()
    {
        //Assert
        Ago(TimeSpan.FromDays(35)).Should().Be("last month");
        Ago(TimeSpan.FromDays(59)).Should().Be("last month");
    }

    [Fact]
    public void months_are_counted_up_to_eleven()
    {
        //Assert
        Ago(TimeSpan.FromDays(90)).Should().Be("3 months ago");
        Ago(TimeSpan.FromDays(335)).Should().Be("11 months ago");
    }

    [Fact]
    public void a_gap_short_of_a_full_year_still_reads_last_year()
    {
        //Assert
        Ago(TimeSpan.FromDays(362)).Should().Be("last year");
        Ago(TimeSpan.FromDays(365)).Should().Be("last year");
    }

    [Fact]
    public void years_are_counted()
    {
        //Assert
        Ago(TimeSpan.FromDays(800)).Should().Be("2 years ago");
        Ago(TimeSpan.FromDays(3650)).Should().Be("10 years ago");
    }
}
