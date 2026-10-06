using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Download_opens_model_view_with_title_author_and_facts()
    {
        var folder = await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        // The collapsed catalog cell's title still matches the locator, so take the Model View's (later) one.
        await Expect(Page.GetByText("Oak Chair", new() { Exact = true }).Last).ToBeVisibleAsync();
        await Expect(Page.GetByText("by Fixture Artist · Poly Haven")).ToBeVisibleAsync();
        await Expect(Page.GetByText("drag to rotate · scroll to zoom", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ModelDescription)).Should().Contain("“Oak Chair” is a free, CC0-licensed furniture 3D model")
            .And.Contain("2 triangles across a single mesh primitive (4 vertices), with one material");
        File.Exists(Path.Combine(folder, "chair", "model.gltf")).Should().BeTrue();
        File.Exists(Path.Combine(folder, "chair", "model.bin")).Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.IsDownloading)).Should().BeFalse();
        await SnapshotAsync("PolyHavenBrowser-model");
    }

    [Fact]
    public async Task Download_bar_shows_progress_and_disables_other_downloads()
    {
        await ChooseDownloadFolderAsync();
        Fixture.Transport.Hold();
        await Button("Download Brass Lantern").ClickAsync();
        await Expect(Page.GetByText("Downloading “Brass Lantern”…", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.DownloadProgress, progress => progress > 0 && progress < 100,
            description: "partial download progress");
        await Expect(Page.GetByRole(AriaRole.Progressbar)).ToBeVisibleAsync();
        await Expect(Button("Download Oak Chair")).ToBeDisabledAsync();
        await Expect(Button("Download Brass Lantern")).ToBeDisabledAsync();
        Fixture.Transport.Release();
        await Expect(Dialog).ToContainTextAsync("Status: InitializationFailed");
        await CloseDialogAsync();
        await Expect(Page.GetByTestId("BackCommand")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsDownloading)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.DownloadStatusText)).Should().BeEmpty();
        await Page.GetByTestId("BackCommand").ClickAsync();
        await Expect(Button("Download Oak Chair")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Already_downloaded_model_opens_without_network()
    {
        var folder = await ChooseDownloadFolderAsync();
        FixtureModel.Save(Path.Combine(folder, "chair"));
        await OpenModelAsync("Oak Chair");
        Fixture.Transport.Requests.Should().NotContain(request => request.StartsWith("/files/") || request.StartsWith("/download/"));
        (await Page.EvaluateAsync(() => Fixture.Model.ModelFacts.Single(fact => fact.Label == "Triangles").Value)).Should().Be("2");
    }

    [Fact]
    public async Task Missing_gltf_offer_shows_download_error_and_returns_to_catalog()
    {
        var folder = await ChooseDownloadFolderAsync();
        await Button("Download Ceramic Vase").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Could not download “Ceramic Vase”. - Poly Haven offers no glTF download for \"Ceramic Vase\".");
        await CloseDialogAsync();
        await Expect(Page.GetByText("3 models", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Download Ceramic Vase")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsModelViewActive)).Should().BeFalse();
        Directory.Exists(Path.Combine(folder, "vase")).Should().BeFalse();
    }

    [Fact]
    public async Task Http_failure_during_download_shows_error_dialog()
    {
        var folder = await ChooseDownloadFolderAsync();
        Fixture.Transport.FailFileDownloads = true;
        await Button("Download Oak Chair").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Could not download “Oak Chair”.");
        await Expect(Dialog).ToContainTextAsync("500 (InternalServerError)");
        await CloseDialogAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsModelViewActive)).Should().BeFalse();
        Directory.EnumerateFiles(Path.Combine(folder, "chair"), "*.gltf").Should().BeEmpty();
        // A retry after the failure downloads the model again instead of reusing the incomplete files.
        Fixture.Transport.FailFileDownloads = false;
        await OpenModelAsync("Oak Chair");
        File.Exists(Path.Combine(folder, "chair", "model.bin")).Should().BeTrue();
    }
}
