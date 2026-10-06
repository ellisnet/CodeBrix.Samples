using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using WikipediaPublisher.RenderArticle.Models;
using WikipediaPublisher.Views;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace WikipediaPublisher.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
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
    private async Task BrowseToAsync(string relativeUrl)
    {
        var url = new Uri(Fixture.Origin, relativeUrl).AbsoluteUri;
        await Page.EvaluateAsync(() => Fixture.Model.NavigateToUrl(url));
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ArticleUrl, value => value == url);
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
        // PlayTest's pointer input and the control's native browser input bridge.
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

    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Embedded_browser_link_receives_real_keyboard_input(ScreenOrientation orientation)
    {
        Fixture.Application.Orientation.Should().Be(orientation);
        var bounds = await Page.GetByTestId("Browser").BoundingBoxAsync();
        // Focus the browser's blank margin, then use its normal tab order and Enter.
        await Page.Mouse.ClickAsync(bounds.X + 20, bounds.Y + 20);
        (await Page.EvaluateAsync(() => Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(Fixture.View.XamlRoot)))
            .Should().Be(await Page.EvaluateAsync(() => Fixture.View.FindName("Browser")));
        await Page.Keyboard.PressAsync("Tab");
        var focus = await Page.EvaluateAsync(() =>
            ((Microsoft.UI.Xaml.Controls.WebView2)Fixture.View.FindName("Browser")).ExecuteScriptAsync("document.activeElement.tagName"));
        (await focus).Should().Be("\"A\"");
        await Page.Keyboard.PressAsync("Enter");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ArticleUrl,
            url => url == new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        await Expect(Page.GetByText("Ready to publish this article.", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Embedded_browser_pixels_follow_orientation_changes(ScreenOrientation initial)
    {
        // A DOM read/navigation can pass even if the browser is never composited.
        // Change this local fixture's background and inspect the actual Skia PNG.
        var operation = await Page.EvaluateAsync(() =>
            ((Microsoft.UI.Xaml.Controls.WebView2)Fixture.View.FindName("Browser")).ExecuteScriptAsync(
                "document.body.style.backgroundColor='rgb(37,149,211)'"));
        await operation;
        var opposite = initial == ScreenOrientation.Landscape ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
        foreach (var orientation in new[] { initial, opposite, initial })
        {
            await Fixture.Application.SetOrientationAsync(orientation);
            var bounds = await Page.GetByTestId("Browser").BoundingBoxAsync();
            var elapsed = Stopwatch.StartNew();
            var expected = new SKColor(37, 149, 211);
            SKColor pixel;
            do
            {
                using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
                bitmap.Width.Should().Be(orientation == ScreenOrientation.Landscape ? 1920 : 1080);
                bitmap.Height.Should().Be(orientation == ScreenOrientation.Landscape ? 1080 : 1920);
                pixel = bitmap.GetPixel((int)bounds.X + 20, (int)bounds.Y + 20);
                if (pixel == expected) break;
                await Task.Delay(50, TestContext.Current.CancellationToken);
            } while (elapsed.Elapsed < TimeSpan.FromSeconds(10));
            pixel.Should().Be(expected, "the native browser's pixels must be composited after each resize");
            await SnapshotAsync("WikipediaPublisher-browser-" + orientation);
        }
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

    [Fact]
    public async Task Confirming_replace_overwrites_the_existing_file()
    {
        await FindArticleAsync();
        var path = Path.Combine(Fixture.DataDirectory, "replace " + Guid.NewGuid().ToString("N") + ".pdf");
        await File.WriteAllTextAsync(path, "Replace this document", TestContext.Current.CancellationToken);
        await SelectOutputAsync(path);
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Replace existing file?");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        Fixture.Renderer.LastRequest.OutputFilePath.Should().Be(path);
        File.ReadAllText(path).Should().StartWith("%PDF-");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Button("Publish")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Typed_output_path_enables_publish_on_an_article()
    {
        await FindArticleAsync();
        var path = Path.Combine(Fixture.DataDirectory, "typed " + Guid.NewGuid().ToString("N") + ".pdf");
        await Page.GetByTestId("OutputFilePath").FillAsync(path);
        await Expect(Button("Publish")).ToBeEnabledAsync();
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(0);
        Fixture.Renderer.LastRequest.OutputFilePath.Should().Be(path);
        Fixture.Renderer.LastRequest.ArticleUrl.Should().Be(new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        File.ReadAllText(path).Should().StartWith("%PDF-");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
    }

    [Theory]
    [InlineData("wiki/Special:Random")]
    [InlineData("wiki/Category:Fixtures")]
    [InlineData("wiki/File:Fixture.png")]
    public async Task Non_article_namespaces_keep_publish_disabled(string relativeUrl)
    {
        await Page.GetByTestId("OutputFilePath").FillAsync(Path.Combine(Fixture.DataDirectory, "namespace.pdf"));
        await BrowseToAsync(relativeUrl);
        await Expect(Page.GetByText("Browse to an article page to enable publishing.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Publish")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Navigating_away_from_an_article_disables_publish()
    {
        await FindArticleAsync();
        await Page.GetByTestId("OutputFilePath").FillAsync(Path.Combine(Fixture.DataDirectory, "away.pdf"));
        await Expect(Button("Publish")).ToBeEnabledAsync();
        await BrowseToAsync("wiki/Category:Fixtures");
        await Expect(Page.GetByText("Browse to an article page to enable publishing.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Publish")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Commands_are_disabled_and_progress_shown_while_rendering()
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Fixture.Renderer.Gate = gate;
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByText("Rendering the fixture article…", new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ProgressValue, value => value == 50);
        await Expect(Button("Search")).ToBeDisabledAsync();
        await Expect(Button("Select…")).ToBeDisabledAsync();
        await Expect(Button("Publish")).ToBeDisabledAsync();
        await SnapshotAsync("WikipediaPublisher-rendering");
        gate.SetResult();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Button("Search")).ToBeEnabledAsync();
        await Expect(Button("Select…")).ToBeEnabledAsync();
        await Expect(Button("Publish")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Successful_publish_reports_saved_status_and_resets_progress()
    {
        await FindArticleAsync();
        var path = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        await Expect(Page.GetByText("Will save to: " + path, new() { Exact = true })).ToBeVisibleAsync();
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Saved to:");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByText("Saved: " + path, new() { Exact = true })).ToBeVisibleAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ProgressValue, value => value == 0);
        (await Page.EvaluateAsync(() => Fixture.Model.IsBusy)).Should().BeFalse();
    }

    [Fact]
    public async Task Save_picker_suggests_the_article_title_as_file_name()
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("Fixture Article.pdf");
    }

    [Fact]
    public async Task Save_picker_failure_shows_error_dialog_and_app_recovers()
    {
        var previous = Path.Combine(Fixture.DataDirectory, "previous.pdf");
        await Page.GetByTestId("OutputFilePath").FillAsync(previous);
        Fixture.Application.FilePickers.EnqueueSaveFile(Path.Combine(Fixture.DataDirectory, "missing folder", "article.pdf"));
        await Button("Select…").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Could not open the file dialog:");
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.OutputFilePath)).Should().Be(previous);
        var path = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf");
        await SelectOutputAsync(path);
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(2);
    }

    [Fact]
    public async Task Whitespace_search_terms_keep_search_disabled()
    {
        await Page.GetByTestId("SearchTerms").FillAsync("Fixture Article");
        await Expect(Button("Search")).ToBeEnabledAsync();
        await Page.GetByTestId("SearchTerms").FillAsync("   ");
        await Expect(Button("Search")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SubmitSearch())).Should().BeFalse();
    }

    [Fact]
    public async Task Search_terms_are_escaped_in_the_browser_request()
    {
        await Page.GetByTestId("SearchTerms").FillAsync("  Nöldeke & Müller #1/2?  ");
        await Button("Search").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ArticleUrl,
            url => url == new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        Fixture.Requests.Should().Contain("/w/index.php?search=N%C3%B6ldeke%20%26%20M%C3%BCller%20%231%2F2%3F");
    }

    [Fact]
    public async Task Default_page_size_is_coffee_table_and_reaches_the_service()
    {
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedPageSize.Option)).Should().Be(PageSizeOption.EightByTen);
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        Fixture.Renderer.LastRequest.PageSize.Should().Be(PageSizeOption.EightByTen);
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
    }

    [Theory]
    [InlineData("6\" × 9\" (trade book)", PageSizeOption.SixByNine)]
    [InlineData("8.5\" × 11\" (US Letter)", PageSizeOption.Letter)]
    [InlineData("A4 (210 × 297 mm)", PageSizeOption.A4)]
    public async Task Every_page_size_reaches_the_service(string displayName, PageSizeOption option)
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        await Page.GetByTestId("SelectedPageSize").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = displayName, Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.SelectedPageSize.Option, value => value == option);
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Published");
        Fixture.Renderer.LastRequest.PageSize.Should().Be(option);
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
    }

    [Fact]
    public async Task Publish_error_dialog_includes_the_article_url()
    {
        await FindArticleAsync();
        await SelectOutputAsync(Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + ".pdf"));
        Fixture.Renderer.Fail = true;
        await Button("Publish").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog))
            .ToContainTextAsync("Article URL: " + new Uri(Fixture.Origin, "wiki/Fixture_Article").AbsoluteUri);
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByText("Publishing failed.", new() { Exact = true })).ToBeVisibleAsync();
    }
}
