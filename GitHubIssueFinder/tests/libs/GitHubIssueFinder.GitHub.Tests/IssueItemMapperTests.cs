using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GitHubIssueFinder.GitHub.Tests;

public class IssueItemMapperTests
{
    private static IReadOnlyList<IssueItem> MapFixture(string name)
    {
        var response = JsonSerializer.Deserialize(Fixture.Read(name),
            GitHubJsonContext.Default.SearchResponseDto);
        return IssueItemMapper.MapItems(response.Items);
    }

    private static IssueItem MapOne(string itemJson)
    {
        var response = JsonSerializer.Deserialize(
            "{\"total_count\":1,\"incomplete_results\":false,\"items\":[" + itemJson + "]}",
            GitHubJsonContext.Default.SearchResponseDto);
        return IssueItemMapper.Map(response.Items[0]);
    }

    [Fact]
    public void an_issue_carries_every_value_a_row_shows()
    {
        //Act
        var item = MapFixture(Fixture.SearchPage)[0];

        //Assert
        item.Id.Should().Be(5342497532L);
        item.Number.Should().Be(4965);
        item.Title.Should().Be("[migration] Adopt the Arcade backport workflow in SkiaSharp");
        item.HtmlUrl.Should().Be("https://github.com/mono/SkiaSharp/issues/4965");
        item.RepositoryFullName.Should().Be("mono/SkiaSharp");
        item.RepositoryHtmlUrl.Should().Be("https://github.com/mono/SkiaSharp");
        item.Kind.Should().Be(IssueKind.Issue);
        item.State.Should().Be(IssueState.Open);
        item.AuthorLogin.Should().Be("mattleibow");
        item.CreatedAt.Should().Be(new DateTimeOffset(2026, 9, 3, 23, 35, 58, TimeSpan.Zero));
        item.UpdatedAt.Should().Be(new DateTimeOffset(2026, 9, 3, 23, 35, 58, TimeSpan.Zero));
        item.ClosedAt.Should().BeNull();
        item.CommentCount.Should().Be(0);
        item.MilestoneTitle.Should().Be("v3.0");
        item.AssigneeLogins.Should().Equal(new[] { "mattleibow" });
        item.Labels.Count.Should().Be(0);
    }

    [Fact]
    public void a_pull_request_is_known_by_the_member_only_pull_requests_carry()
    {
        //Act
        var items = MapFixture(Fixture.SearchPage);

        //Assert
        items.Take(3).Select(i => i.Kind).Should().Equal(new[]
        {
            IssueKind.Issue, IssueKind.Issue, IssueKind.Issue,
        });
        items.Skip(3).Select(i => i.Kind).Should().Equal(new[]
        {
            IssueKind.PullRequest, IssueKind.PullRequest,
        });
        items[4].AuthorLogin.Should().Be("github-actions[bot]");
        items[4].HtmlUrl.Should().Be("https://github.com/mono/SkiaSharp/pull/4967");
    }

    [Fact]
    public void an_issue_closed_as_not_planned_says_so()
    {
        //Act
        var item = MapFixture(Fixture.IssueNotPlanned).Single();

        //Assert
        item.Kind.Should().Be(IssueKind.Issue);
        item.State.Should().Be(IssueState.NotPlanned);
        item.ClosedAt.Should().Be(new DateTimeOffset(2026, 8, 30, 9, 15, 0, TimeSpan.Zero));
        item.RepositoryFullName.Should().Be("mono/skia");
    }

    [Fact]
    public void a_merged_pull_request_and_an_abandoned_one_read_differently()
    {
        //Act
        var items = MapFixture(Fixture.PullRequestsMerged);

        //Assert
        items[0].State.Should().Be(IssueState.Merged);
        items[1].State.Should().Be(IssueState.Closed);
        items.Select(i => i.Kind).Should().Equal(new[]
        {
            IssueKind.PullRequest, IssueKind.PullRequest,
        });
    }

    [Fact]
    public void a_draft_pull_request_reads_as_a_draft()
    {
        //Act
        var item = MapFixture(Fixture.PullRequestDraft).Single();

        //Assert
        item.Kind.Should().Be(IssueKind.PullRequest);
        item.State.Should().Be(IssueState.Draft);
    }

    [Fact]
    public void an_issue_closed_for_no_stated_reason_is_simply_closed()
    {
        //Act
        var item = MapOne("{\"number\":1,\"state\":\"closed\",\"state_reason\":\"completed\","
            + "\"repository_url\":\"https://api.github.com/repos/acme/widgets\"}");

        //Assert
        item.State.Should().Be(IssueState.Closed);
    }

    [Fact]
    public void an_open_pull_request_that_is_not_a_draft_is_simply_open()
    {
        //Act
        var item = MapOne("{\"number\":1,\"state\":\"open\",\"draft\":false,\"pull_request\":{\"merged_at\":null},"
            + "\"repository_url\":\"https://api.github.com/repos/acme/widgets\"}");

        //Assert
        item.Kind.Should().Be(IssueKind.PullRequest);
        item.State.Should().Be(IssueState.Open);
    }

    [Fact]
    public void the_repository_name_is_the_last_two_parts_of_the_repository_address()
    {
        //Assert
        IssueItemMapper.ExtractRepositoryFullName("https://api.github.com/repos/mono/SkiaSharp")
            .Should().Be("mono/SkiaSharp");
        IssueItemMapper.ExtractRepositoryFullName("https://api.github.com/repos/mono/SkiaSharp/")
            .Should().Be("mono/SkiaSharp");
        IssueItemMapper.ExtractRepositoryFullName(null).Should().BeNull();
        IssueItemMapper.ExtractRepositoryFullName("   ").Should().BeNull();
        IssueItemMapper.ExtractRepositoryFullName("nothing").Should().BeNull();
    }

    [Fact]
    public void an_item_with_no_repository_address_gets_no_repository_link()
    {
        //Act
        var item = MapOne("{\"number\":1,\"state\":\"open\"}");

        //Assert
        item.RepositoryFullName.Should().BeNull();
        item.RepositoryHtmlUrl.Should().BeNull();
    }

    [Fact]
    public void the_awkward_members_are_all_safe_to_read()
    {
        //Act - no milestone, no assignees, no labels, no author
        var item = MapOne("{\"number\":1,\"state\":\"open\",\"milestone\":null,\"assignees\":null,"
            + "\"labels\":null,\"user\":null,\"closed_at\":null,"
            + "\"repository_url\":\"https://api.github.com/repos/acme/widgets\"}");

        //Assert
        item.MilestoneTitle.Should().BeNull();
        item.AuthorLogin.Should().BeNull();
        item.ClosedAt.Should().BeNull();
        item.AssigneeLogins.Should().NotBeNull();
        item.AssigneeLogins.Count.Should().Be(0);
        item.Labels.Should().NotBeNull();
        item.Labels.Count.Should().Be(0);
    }

    [Fact]
    public void a_label_colour_github_left_out_or_wrote_oddly_is_carried_through_untouched()
    {
        //Act
        var labels = MapFixture(Fixture.SearchPage)[2].Labels;

        //Assert - the row decides what to draw; the library does not guess a colour
        labels.Count.Should().Be(4);
        labels[0].Name.Should().Be("type/enhancement");
        labels[0].ColorHex.Should().Be("84b6eb");
        labels[0].Description.Should().Be(string.Empty);
        labels[2].Name.Should().Be("needs-triage");
        labels[2].ColorHex.Should().BeNull();
        labels[2].Description.Should().BeNull();
        labels[3].ColorHex.Should().Be("not-a-colour");
    }

    [Fact]
    public void a_label_description_github_wrote_is_kept_as_it_stands()
    {
        //Act
        var labels = MapFixture(Fixture.SearchPage)[4].Labels;

        //Assert
        labels.Count.Should().Be(2);
        labels[0].Name.Should().Be("area/Docs");
        labels[0].Description.Should().Be(
            "Issues relating to documentation, such as API docs or conceptual docs.");
    }

    [Fact]
    public void a_result_with_no_items_at_all_maps_to_nothing()
    {
        //Act
        var items = MapFixture(Fixture.SearchEmpty);

        //Assert
        items.Should().NotBeNull();
        items.Count.Should().Be(0);
        IssueItemMapper.MapItems(null).Count.Should().Be(0);
        IssueItemMapper.Map(null).Should().BeNull();
    }
}
