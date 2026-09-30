using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using WikipediaPublisher.RenderArticle.Models;
using WikipediaPublisher.Views;
using SilverAssertions;
using Xunit;

namespace WikipediaPublisher.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private async Task FindArticleAsync(bool enter = false)
    {
        await Page.GetByTestId("SearchTerms").FillAsync("Fixture Article");
        if (enter) await Page.GetByTestId("SearchTerms").PressAsync("Enter");
        else await Button("Search").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ArticleUrl,
            url => url == new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        await Expect(Page.GetByText("Ready to publish this article.", new() { Exact = true })).ToBeVisibleAsync();
    }
    private async Task SelectOutputAsync(string path)
    {
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Select…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.OutputFilePath, value => value == path);
    }

    [Fact]
    public async Task Home_page_and_blank_search_disable_publishing()
    {
        await Expect(Button("Search")).ToBeDisabledAsync();
        await Expect(Button("Publish")).ToBeDisabledAsync();
        await Page.GetByTestId("OutputFilePath").FillAsync(Path.Combine(Fixture.DataDirectory, "home.pdf"));
        await Expect(Button("Publish")).ToBeDisabledAsync();
        await SnapshotAsync("WikipediaPublisher-home");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Search_navigates_the_embedded_browser_and_tracks_redirects(bool enter)
    {
        await FindArticleAsync(enter);
        await Expect(Button("Publish")).ToBeDisabledAsync();
        await SnapshotAsync("WikipediaPublisher-article");
    }

    [Fact]
    public async Task Cancelling_save_picker_keeps_the_previous_path()
    {
        var path = Path.Combine(Fixture.DataDirectory, "previous.pdf");
        await Page.GetByTestId("OutputFilePath").FillAsync(path);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Select…").ClickAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.OutputFilePath)).Should().Be(path);
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
    }

    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Embedded_browser_link_receives_real_pointer_input(ScreenOrientation orientation)
    {
        Fixture.Application.Orientation.Should().Be(orientation);
        // Use the real WebView API only to measure the link. The click travels through
        // PlayTest's pointer input and the control's normal WPE input bridge.
        var operation = await Page.EvaluateAsync(() =>
            ((Microsoft.UI.Xaml.Controls.WebView2)Fixture.View.FindName("Browser")).ExecuteScriptAsync(
                "(() => { const r = document.querySelector('a').getBoundingClientRect(); return {x:r.x+r.width/2,y:r.y+r.height/2}; })()"));
        using var position = JsonDocument.Parse(await operation);
        var bounds = await Page.GetByTestId("Browser").BoundingBoxAsync();
        await Page.Mouse.ClickAsync(bounds.X + (float)position.RootElement.GetProperty("x").GetDouble(),
            bounds.Y + (float)position.RootElement.GetProperty("y").GetDouble());
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ArticleUrl,
            url => url == new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        await Expect(Page.GetByText("Ready to publish this article.", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Save_path_and_page_size_reach_the_publishing_service()
    {
        await FindArticleAsync();
        var path = Path.Combine(Fixture.DataDirectory, "My Article " + Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        File.Exists(path).Should().BeFalse("the application removes an empty save-picker placeholder");
        await Page.GetByTestId("SelectedPageSize").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "A4 (210 × 297 mm)", Exact = true }).ClickAsync();
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        Fixture.Renderer.LastRequest.OutputFilePath.Should().Be(path);
        Fixture.Renderer.LastRequest.PageSize.Should().Be(PageSizeOption.A4);
        File.ReadAllText(path).Should().StartWith("%PDF-");
        await SnapshotAsync("WikipediaPublisher-published");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Button("Publish")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Cancelling_replace_preserves_the_existing_file()
    {
        await FindArticleAsync();
        var path = Path.Combine(Fixture.DataDirectory, "existing.pdf");
        await File.WriteAllTextAsync(path, "Keep this document", TestContext.Current.CancellationToken);
        await SelectOutputAsync(path);
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Replace existing file?");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "No", Exact = true }).ClickAsync();
        File.ReadAllText(path).Should().Be("Keep this document");
        Fixture.Renderer.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task Renderer_failure_is_reported_and_publish_recovers()
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        Fixture.Renderer.Fail = true;
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Fixture renderer unavailable");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Button("Publish")).ToBeEnabledAsync();
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_preview_keeps_search_and_save_controls_usable()
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, "portrait.pdf"));
        await Expect(Button("Publish")).ToBeEnabledAsync();
        await SnapshotAsync("WikipediaPublisher-portrait");
    }
}
