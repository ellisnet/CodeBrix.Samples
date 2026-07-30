using NotionDocumentCreator.CreateDocument.Services;
using SilverAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// Integration tests against the live Notion API. Opt-in: they skip unless both
/// NOTION_AUTH_TOKEN and NOTION_TEST_PAGE_ID environment variables are set (the
/// test page is expected to be the 13-chapter "The Father of DRAKON" tree).
/// </summary>
public class NotionDocumentServiceTests : IDisposable
{
    private readonly NotionDocumentService _service;
    private readonly string _authToken;
    private readonly string _testPageId;

    public NotionDocumentServiceTests()
    {
        _authToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN");
        _testPageId = Environment.GetEnvironmentVariable("NOTION_TEST_PAGE_ID");

        Assert.SkipWhen(_authToken == null,
            "NOTION_AUTH_TOKEN environment variable is not set; skipping Notion integration tests.");
        Assert.SkipWhen(_testPageId == null,
            "NOTION_TEST_PAGE_ID environment variable is not set; skipping Notion integration tests.");

        _service = new NotionDocumentService();
    }

    [Fact]
    public async Task can_connect_and_list_the_root_pages_children()
    {
        //Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        //Act
        var botName = await _service.ConnectAsync(_authToken, cancellationToken);
        var roots = await _service.LoadRootsAsync(_testPageId, cancellationToken);
        var children = await _service.LoadChildrenAsync(roots[0].Id, cancellationToken);

        //Assert
        botName.Should().NotBeNullOrEmpty();
        roots.Should().HaveCount(1);
        roots[0].Title.Should().NotBeNullOrEmpty();
        roots[0].Depth.Should().Be(0);
        children.Should().HaveCount(13);
        children[0].Title.Should().StartWith("Prologue");
        children[^1].Title.Should().Contain("Sources");
        children.All(c => c.Depth == 1).Should().Be(true);
    }

    [Fact]
    public async Task can_load_a_non_scrolling_preview_of_the_root_page()
    {
        //Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await _service.ConnectAsync(_authToken, cancellationToken);
        var roots = await _service.LoadRootsAsync(_testPageId, cancellationToken);

        //Act
        var preview = await _service.LoadPreviewAsync(roots[0].Id, cancellationToken);

        //Assert
        preview.Title.Should().Be(roots[0].Title);
        preview.ChildPageCount.Should().Be(13);
        preview.TextSnippets.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task can_create_a_two_chapter_pdf_from_live_notion_data()
    {
        //Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await _service.ConnectAsync(_authToken, cancellationToken);
        var roots = await _service.LoadRootsAsync(_testPageId, cancellationToken);
        var children = await _service.LoadChildrenAsync(roots[0].Id, cancellationToken);
        var outputPath = Path.Combine(
            Path.GetTempPath(), "NotionDocumentCreator.Tests", "two-chapters.pdf");

        //Act - the root page becomes the cover, the first child the sole chapter
        var result = await _service.CreateDocumentAsync(new Models.CreateRequest
        {
            PageIds = [roots[0].Id, children[0].Id],
            OutputFilePath = outputPath,
            PageSize = Models.PageSizeOption.EightByTen
        }, cancellationToken: cancellationToken);

        //Assert
        File.Exists(outputPath).Should().Be(true);
        result.ChapterCount.Should().Be(2);
        (result.PageCount >= 2).Should().Be(true);
        result.Title.Should().Be(roots[0].Title);
    }

    public void Dispose()
    {
        _service?.Dispose();
    }
}
