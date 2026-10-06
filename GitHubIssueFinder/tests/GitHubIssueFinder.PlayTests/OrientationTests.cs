using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace GitHubIssueFinder.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_results_list_scrolls_to_last_group()
    {
        var repositories = Enumerable.Range(1, 16).Select(n => $"sample/repository-{n:00}").ToArray();
        Fixture.Search.Pages = new[]
        {
            PageOf(1, repositories.Length * 2, repositories.SelectMany((name, index) => new[]
            {
                Item(index * 2 + 1, $"First issue {index + 1}", name),
                Item(index * 2 + 2, $"Second issue {index + 1}", name),
            }).ToArray()),
        };
        await SearchForAsync("sample");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 32 items in 16 repositories in .+\.$"));
        var last = Page.GetByRole(AriaRole.Button, new() { Name = "sample/repository-16" });
        await Expect(Page.GetByText("sample/repository-01", new() { Exact = true })).ToBeVisibleAsync();
        var first = await Page.GetByText("sample/repository-01", new() { Exact = true }).BoundingBoxAsync();
        // Before scrolling, the last group is either not realized yet or below the portrait screen.
        var before = await Page.GetByText("sample/repository-16", new() { Exact = true }).BoundingBoxAsync();
        (before == null || before.Y >= Fixture.Application.Height).Should().BeTrue();
        await Page.Mouse.MoveAsync(first.X + first.Width / 2, first.Y + first.Height / 2);
        await Page.Mouse.WheelAsync(0, 20000);
        await last.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Links.Opened.Count, count => count == 1,
            description: "opened link");
        Fixture.Links.Opened[0].Should().Be(new Uri("https://example.invalid/sample/repository-16"));
        await SnapshotAsync("GitHubIssueFinder-portrait-scrolled");
    }
}
