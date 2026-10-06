using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using Microsoft.UI.Xaml.Controls;
using PolyHavenBrowser.Views;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private ScrollViewer CatalogScroll => (ScrollViewer)Fixture.View.FindName("CatalogScroll");

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
        await SortAsync("Name A–Z");
        await WaitForTitlesAsync("Brass Lantern|Ceramic Vase|Oak Chair");
    }

    [Theory]
    [InlineData("Newest", "Ceramic Vase|Brass Lantern|Oak Chair")]
    [InlineData("Most popular", "Oak Chair|Brass Lantern|Ceramic Vase")]
    public async Task Sort_newest_and_most_popular_order_the_catalog(string option, string titles)
    {
        await SortAsync("Name A–Z");
        await WaitForTitlesAsync("Brass Lantern|Ceramic Vase|Oak Chair");
        await SortAsync(option);
        await WaitForTitlesAsync(titles);
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedSortOption)).Should().Be(option);
    }

    [Fact]
    public async Task Research_after_sort_keeps_selected_order()
    {
        await SortAsync("Name A–Z");
        await Page.GetByTestId("SearchText").FillAsync("o");
        await Expect(Page.GetByText("2 models", new() { Exact = true })).ToBeVisibleAsync();
        await WaitForTitlesAsync("Ceramic Vase|Oak Chair");
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
        await Button("Download Oak Chair").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("choose a download folder");
        await CloseDialogAsync();
        Fixture.Transport.Requests.Should().NotContain(request => request.StartsWith("/files/"));
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_toolbar_and_catalog_search_remain_usable()
    {
        await Page.GetByTestId("SearchText").FillAsync("lantern");
        await Expect(Page.GetByText("1 model", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("PolyHavenBrowser-portrait");
    }

    [Fact]
    public async Task Scrolling_near_end_materializes_next_batch()
    {
        await Fixture.ReloadAsync(transport => transport.CatalogSize = 75);
        await Expect(Page.GetByText("75 models", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.Count)).Should().Be(30);
        await Page.Mouse.MoveAsync(Fixture.Application.Width / 2f, Fixture.Application.Height / 2f);
        await Page.Mouse.WheelAsync(0, 400);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count >= 54, description: "next batch");
        // Each wheel notch scrolls a bounded step, so keep scrolling toward the end until every batch is in.
        for (var notch = 0; notch < 40 && await Page.EvaluateAsync(() => Fixture.Model.Cells.HasMoreItems); notch++)
            await Page.Mouse.WheelAsync(0, 2000);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == 75, description: "whole catalog");
    }

    [Fact]
    public async Task New_search_scrolls_catalog_to_top()
    {
        await Fixture.ReloadAsync(transport => transport.CatalogSize = 75);
        await Expect(Page.GetByText("75 models", new() { Exact = true })).ToBeVisibleAsync();
        await Page.Mouse.MoveAsync(Fixture.Application.Width / 2f, Fixture.Application.Height / 2f);
        await Page.Mouse.WheelAsync(0, 400);
        await Fixture.Application.WaitForAsync(() => CatalogScroll.VerticalOffset, offset => offset > 0, description: "scrolled catalog");
        await Page.GetByTestId("SearchText").FillAsync("Fixture Model");
        await Expect(Page.GetByText("72 models", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => CatalogScroll.VerticalOffset, offset => offset == 0, description: "catalog top");
        (await Page.EvaluateAsync(() => Fixture.Model.Cells[0].Title)).Should().Be("Fixture Model 04");
    }

    [Fact]
    public async Task Failed_thumbnail_keeps_placeholder()
    {
        await Fixture.ReloadAsync(transport => transport.FailThumbnailId = "lantern");
        await Fixture.Application.WaitForAsync(
            () => Fixture.Transport.Requests.Contains("/thumbnail/lantern.png")
                && Fixture.Model.Cells.Where(cell => cell.Title != "Brass Lantern").All(cell => cell.Thumbnail != null),
            loaded => loaded, description: "the other thumbnails");
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.Single(cell => cell.Title == "Brass Lantern").Thumbnail)).Should().BeNull();
        await Expect(Page.GetByText("Brass Lantern", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Download Brass Lantern")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Catalog_load_failure_shows_message()
    {
        await Fixture.ReloadAsync(transport => transport.FailCatalog = true);
        await Expect(Page.GetByText("Could not load the Poly Haven catalog")).ToHaveTextAsync(
            "Could not load the Poly Haven catalog: The Poly Haven API request for the asset list failed with status 503 (ServiceUnavailable).");
        (await Page.EvaluateAsync(() => Fixture.Model.IsCatalogLoading)).Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.ResultCountText)).Should().BeEmpty();
        await Expect(Page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Download ") })).ToHaveCountAsync(0);
    }

    private async Task SortAsync(string option)
    {
        await Page.GetByTestId("SelectedSortOption").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
    }

    private Task WaitForTitlesAsync(string titles) => Fixture.Application.WaitForAsync(
        () => string.Join("|", Fixture.Model.Cells.Select(cell => cell.Title)), actual => actual == titles, description: "catalog order");

    private async Task CloseDialogAsync()
    {
        await Dialog.GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    private async Task<string> ChooseDownloadFolderAsync()
    {
        var folder = Path.Combine(Fixture.DataDirectory, "Downloads " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        Fixture.Application.FilePickers.EnqueueFolder(folder);
        await Button("Choose download folder…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.HasDownloadFolder, chosen => chosen);
        return folder;
    }

    // Downloads (or reopens) a model; the first Model View of a page reports that this head has no OpenGL.
    private async Task OpenModelAsync(string title, bool firstModelView = true)
    {
        await Button("Download " + title).ClickAsync();
        if (firstModelView)
        {
            await Expect(Dialog).ToContainTextAsync("Status: InitializationFailed");
            await CloseDialogAsync();
        }
        await Expect(Page.GetByTestId("BackCommand")).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ModelTitle, actual => actual == title, description: "Model View title");
    }
}
