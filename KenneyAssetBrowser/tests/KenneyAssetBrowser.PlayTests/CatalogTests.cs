using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using KenneyAssetBrowser.ViewModels;
using Microsoft.UI.Xaml;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Switching_bundle_rebuilds_categories_and_resets_the_filter()
    {
        await OpenBundleFolderAsync(Fixture.LibraryDirectory);
        (await Page.EvaluateAsync(() => Fixture.Model.BundleCells.Count)).Should().Be(2);
        (await Page.EvaluateAsync(() => Fixture.Model.ResultCountText)).Should().EndWith("· PlayTest synthetic assets");
        await Page.GetByTestId("SelectedCategory").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Images", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == 2);
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Puzzle Pack" }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ResultCountText, text => text.EndsWith("· Puzzle Pack"), description: "the Puzzle Pack cells");
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedCategory)).Should().Be("All categories");
        (await Page.EvaluateAsync(() => Fixture.Model.Categories)).Should().Contain("Spritesheets").And.NotContain("Images");
        await Page.GetByTestId("SelectedCategory").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Spritesheets", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == 2);
        await Expect(Page.GetByText("spritesheet_double", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Corrupt_zip_in_folder_shows_catalog_warning()
    {
        await OpenBundleFolderAsync(Fixture.LibraryDirectory);
        await Expect(Page.GetByText(new Regex(@"^Could not read: kenney_broken\.zip: "))).ToBeVisibleAsync();
        await Expect(Page.GetByText("2 bundles", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Empty_folder_shows_no_bundles_caption()
    {
        await OpenFolderAsync(Fixture.EmptyDirectory);
        await Expect(Page.GetByText("No asset bundles (.zip) found in this folder", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.Count)).Should().Be(0);
        (await Page.EvaluateAsync(() => Fixture.Model.ResultCountText)).Should().BeEmpty();
        (await Page.EvaluateAsync(() => Fixture.Model.FolderPromptVisibility)).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public async Task Scrolling_grid_loads_next_batch()
    {
        await OpenBundleFolderAsync(Fixture.PuzzleDirectory);
        var total = await Page.EvaluateAsync(() => Fixture.Model.Cells.TotalCount);
        total.Should().BeGreaterThan(60);
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.Count)).Should().Be(36);
        // The wheel goes to the grid's own margin beside the cells: a cell button under the
        // pointer keeps the grid from scrolling on the PlayTest head.
        var cell = await (await CellAsync("spritesheet_default")).BoundingBoxAsync();
        await Page.Mouse.MoveAsync(Fixture.Application.Width - 12, cell.Y + cell.Height / 2);
        await Page.Mouse.WheelAsync(0, 360);
        var loaded = await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count >= 36 + AssetCellCollection.ScrollBatch, description: "the next batch of cells");
        ((loaded - 36) % AssetCellCollection.ScrollBatch).Should().Be(0);
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.TotalCount)).Should().Be(total);
    }
}
