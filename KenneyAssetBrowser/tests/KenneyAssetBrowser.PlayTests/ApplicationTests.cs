using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using KenneyAssetBrowser.Views;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private Locator ChooseFolder => Page.GetByRole(AriaRole.Button, new() { Name = "Choose assets folder…", Exact = true }).First;
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private async Task OpenCatalogAsync()
    {
        Fixture.Application.FilePickers.EnqueueFolder(Fixture.AssetsDirectory);
        await ChooseFolder.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells?.Count ?? 0, count => count >= 4);
    }
    private async Task OpenFolderAsync(string folder)
    {
        Fixture.Application.FilePickers.EnqueueFolder(folder);
        await ChooseFolder.ClickAsync();
        await Fixture.Application.WaitForAsync(() => !Fixture.Model.IsCatalogLoading && Fixture.Model.BundleCountText.Length > 0,
            loaded => loaded, 30000, "the folder's catalog");
    }
    private async Task OpenBundleFolderAsync(string folder)
    {
        await OpenFolderAsync(folder);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ResultCountText, text => text.Length > 0, description: "the selected bundle's cells");
    }
    private async Task SearchAsync(string query, int expected)
    {
        await Page.GetByTestId("SearchText").FillAsync(query);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == expected, description: "the filtered cells");
    }
    // The asset grid recycles its cells: a recycled cell keeps its old content parked far off
    // screen, so pick the copy that is on the virtual screen.
    private async Task<Locator> CellAsync(string text)
    {
        var cells = Page.GetByRole(AriaRole.Button).Filter(new() { HasText = text });
        await Expect(cells.First).ToBeVisibleAsync();
        var count = await cells.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var box = await cells.Nth(i).BoundingBoxAsync();
            if (box != null && box.X >= 0 && box.Y >= 0 && box.X < Fixture.Application.Width && box.Y < Fixture.Application.Height) return cells.Nth(i);
        }
        throw new PlayTestException($"No on-screen cell shows \"{text}\".");
    }
    private async Task OpenCellAsync(string text)
    {
        await (await CellAsync(text)).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsViewerActive, active => active, description: "the viewer");
    }
    private async Task CloseDialogAsync()
    {
        await Dialog.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Cancel_folder_picker_keeps_the_welcome_screen()
    {
        Fixture.Application.FilePickers.EnqueueFolder(null);
        await ChooseFolder.ClickAsync();
        await Expect(Page.GetByText("Welcome to Kenney Asset Browser", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.HasAssetsFolder)).Should().BeFalse();
        Fixture.Application.FilePickers.FolderRequestCount.Should().Be(1);
    }

    [Fact]
    public async Task Selected_folder_loads_real_zip_assets_and_thumbnails()
    {
        await OpenCatalogAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.BundleCells.Count)).Should().Be(1);
        await Expect(Page.GetByText("blue_tile", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("KenneyAssetBrowser-catalog");
    }

    [Theory]
    [InlineData("blue", 1)]
    [InlineData("no-such-asset", 0)]
    public async Task Search_filters_real_bundle_entries(string query, int expected)
    {
        await OpenCatalogAsync();
        await Page.GetByTestId("SearchText").FillAsync(query);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == expected);
        (await Page.EvaluateAsync(() => Fixture.Model.ResultCountText)).Should().StartWith(expected + " asset");
    }

    [Fact]
    public async Task Category_picker_filters_the_catalog()
    {
        await OpenCatalogAsync();
        await Page.GetByTestId("SelectedCategory").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Images", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == 2);
    }

    [Fact]
    public async Task Image_viewer_renders_and_zoom_controls_change_the_scale()
    {
        await OpenCatalogAsync();
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "blue_tile" }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsViewerActive, active => active);
        await Expect(Page.GetByText("96 × 96 px", new() { Exact = true })).ToBeVisibleAsync();
        var original = await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.ZoomFactor);
        await Button("+").ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.ZoomFactor)).Should().BeGreaterThan(original);
        await Button("Fit").ClickAsync();
        await SnapshotAsync("KenneyAssetBrowser-image");
        await Button("←").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsViewerActive, active => !active);
    }

    [Fact]
    public async Task Text_asset_opens_in_the_document_viewer()
    {
        await OpenCatalogAsync();
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Readme" }).ClickAsync();
        await Expect(Page.GetByText("A deterministic document displayed by the real asset viewer.", new() { Exact = true })).ToBeVisibleAsync();
        await SnapshotAsync("KenneyAssetBrowser-document");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Catalog_search_is_usable_in_portrait()
    {
        await OpenCatalogAsync();
        await Page.GetByTestId("SearchText").FillAsync("red");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Cells.Count, count => count == 1);
        await SnapshotAsync("KenneyAssetBrowser-portrait");
    }
}
