using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using GitHubIssueFinder.Theming;
using GitHubIssueFinder.Views;
using SilverAssertions;
using Xunit;

namespace GitHubIssueFinder.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    [Fact]
    public async Task Blank_owner_disables_search_and_cancel()
    {
        await Expect(Button("Search")).ToBeDisabledAsync();
        await Expect(Button("Cancel")).ToBeDisabledAsync();
        await Page.GetByTestId("Owner").FillAsync("   ");
        await Expect(Button("Search")).ToBeDisabledAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Search_uses_inputs_and_renders_grouped_results(bool includeClosed)
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Assignee").FillAsync("developer");
        await Page.GetByTestId("IncludeClosed").SetCheckedAsync(includeClosed);
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Add portrait layout coverage", new() { Exact = true })).ToBeVisibleAsync();
        Fixture.Search.LastRequest.Owner.Should().Be("sample");
        Fixture.Search.LastRequest.Assignee.Should().Be("developer");
        Fixture.Search.LastRequest.IncludeClosed.Should().Be(includeClosed);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsSearching, busy => !busy);
        (await Page.EvaluateAsync(() => Fixture.Model.Groups[0].Rows.Count)).Should().Be(2);
        await SnapshotAsync("GitHubIssueFinder-results");
    }

    [Fact]
    public async Task Enter_submits_and_Escape_cancels_a_pending_search()
    {
        Fixture.Search.WaitForCancellation = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Page.GetByTestId("Owner").PressAsync("Enter");
        await Expect(Button("Cancel")).ToBeEnabledAsync();
        await Page.GetByTestId("Owner").PressAsync("Escape");
        await Expect(Button("Cancel")).ToBeDisabledAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        Fixture.Search.LastRequest.Owner.Should().Be("sample");
    }

    [Fact]
    public async Task Service_error_is_reported_and_search_can_be_retried()
    {
        Fixture.Search.Fail = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Fixture service unavailable")).ToBeVisibleAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        Fixture.Search.Fail = false;
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Empty_search_has_no_result_groups()
    {
        Fixture.Search.Empty = true;
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsSearching, busy => !busy);
        (await Page.EvaluateAsync(() => Fixture.Model.Groups.Count)).Should().Be(0);
        await Expect(Button("Search")).ToBeEnabledAsync();
    }

    [Theory]
    [InlineData("Dark", ColorScheme.Dark)]
    [InlineData("Light High Contrast", ColorScheme.LightHighContrast)]
    public async Task Scheme_picker_changes_the_application_palette(string name, ColorScheme expected)
    {
        await Page.GetByTestId("SelectedScheme").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedScheme.Scheme)).Should().Be(expected);
        await SnapshotAsync("GitHubIssueFinder-scheme");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_header_wraps_and_search_remains_usable()
    {
        await Page.GetByTestId("Owner").FillAsync("sample");
        await Button("Search").ClickAsync();
        await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("GitHubIssueFinder-portrait");
    }
}
