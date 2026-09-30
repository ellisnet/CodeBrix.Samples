using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PolyHavenBrowser.Views;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    [Fact]
    public async Task Catalog_renders_metadata_and_thumbnails()
    {
        await Expect(Page.GetByText("3 models", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Oak Chair", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Brass Lantern", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.All(cell => cell.Thumbnail != null), loaded => loaded);
        await SnapshotAsync("PolyHavenBrowser-catalog");
    }

    [Theory]
    [InlineData("furniture", "1 model")]
    [InlineData("nothing-matches", "0 models")]
    public async Task Search_filters_catalog_by_metadata(string query, string result)
    {
        await Page.GetByTestId("SearchText").FillAsync(query);
        await Expect(Page.GetByText(result, new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ResultCountText)).Should().Be(result);
    }

    [Fact]
    public async Task Sort_selector_reorders_the_catalog()
    {
        await Page.GetByTestId("SelectedSortOption").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Name A–Z", Exact = true }).ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cells[0].Title)).Should().Be("Brass Lantern");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Folder_picker_handles_selection_and_cancel(bool cancel)
    {
        var path = Path.Combine(Fixture.DataDirectory, "Selected Models");
        Directory.CreateDirectory(path);
        Fixture.Application.FilePickers.EnqueueFolder(cancel ? null : path);
        await Button("Choose download folder…").ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.HasDownloadFolder)).Should().Be(!cancel);
        (await Page.EvaluateAsync(() => Fixture.Model.DownloadFolderLabel)).Should().Be(cancel ? "Choose download folder…" : path);
    }

    [Fact]
    public async Task Download_without_a_folder_explains_what_is_required()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Download", Exact = true }).First.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("choose a download folder");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_toolbar_and_catalog_search_remain_usable()
    {
        await Page.GetByTestId("SearchText").FillAsync("lantern");
        await Expect(Page.GetByText("1 model", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("PolyHavenBrowser-portrait");
    }
}
