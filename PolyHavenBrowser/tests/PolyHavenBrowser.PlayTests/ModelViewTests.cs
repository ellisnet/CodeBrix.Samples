using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using PolyHavenBrowser.Services;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Rendering_unavailable_dialog_reports_status_once_per_page()
    {
        await ChooseDownloadFolderAsync();
        await Button("Download Oak Chair").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("The interactive 3D model preview is not available on this system");
        await Expect(Dialog).ToContainTextAsync("Status: InitializationFailed");
        await CloseDialogAsync();
        await Page.GetByTestId("BackCommand").ClickAsync();
        // The page reported the failure already: a second Model View opens without the dialog.
        await OpenModelAsync("Brass Lantern", firstModelView: false);
        await Page.GetByTestId("BackCommand").ClickAsync();
        await Expect(Page.GetByText("3 models", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Back_returns_to_catalog_and_clears_current_model()
    {
        await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentModel != null)).Should().BeTrue();
        await Page.GetByTestId("BackCommand").ClickAsync();
        await Expect(Page.GetByText("3 models", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("BackCommand")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("DocumentCommand")).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentModel)).Should().BeNull();
        (await Page.EvaluateAsync(() => Fixture.Model.IsModelViewActive)).Should().BeFalse();
        await Expect(Button("Download Oak Chair")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Tags_and_details_rows_match_asset_and_stats()
    {
        await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        await Expect(Page.GetByText("#furniture", new() { Exact = true })).ToBeVisibleAsync();
        var expected = new[]
        {
            "Categories=furniture", "Published=September 13, 2020", "Downloads=100", "Max texture size=2048 × 1024",
            $"Triangles={FixtureModel.Triangles}", $"Vertices={FixtureModel.Vertices}", "Materials=1",
            "Size on disk=" + ModelDescriptionBuilder.FormatBytes(Fixture.Transport.ModelBytes), "License=CC0 (public domain)",
        };
        (await Page.EvaluateAsync(() => Fixture.Model.ModelFacts.Select(fact => fact.Label + "=" + fact.Value).ToArray()))
            .Should().Equal(expected);
        foreach (var label in new[] { "Categories", "Published", "Max texture size", "Size on disk", "License" })
            await Expect(Page.GetByText(label, new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("September 13, 2020", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("CC0 (public domain)", new() { Exact = true })).ToBeVisibleAsync();
    }
}
