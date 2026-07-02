using CodeBrix.PdfDocCreate.Rendering;
using SilverAssertions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WikipediaPublisher.RenderArticle.Internal;
using WikipediaPublisher.RenderArticle.Models;
using WikipediaPublisher.RenderArticle.Services;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests.Services;

public class ArticleRenderServiceTests
{
    private const string CuneiformUrl = "https://en.wikipedia.org/wiki/Cuneiform";
    private const string FixtureResource = "WikipediaPublisher.RenderArticle.Tests.Fixtures.cuneiform.html";

    private readonly ITestOutputHelper _output;

    public ArticleRenderServiceTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    private static string GetOutDirectory()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Out");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void VerifyPdfSignature(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[5];
        stream.ReadExactly(buffer, 0, buffer.Length);
        buffer.Should().Equal(Encoding.ASCII.GetBytes("%PDF-"));
    }

    [Theory]
    [InlineData("Cuneiform", "Cuneiform")]
    [InlineData("AC/DC", "AC_DC")]
    [InlineData("What? No: \"Really\"", "What_ No_ _Really_")]
    [InlineData("", "article")]
    public void SanitizeFileName_removes_invalid_characters(string name, string expected) =>
        ArticleRenderService.SanitizeFileName(name).Should().Be(expected);

    [Fact]
    public async Task Compose_and_render_fixture_offline_produces_multipage_pdf()
    {
        //Arrange — parse the embedded article fixture (no network, no images)
        var html = await EmbeddedResourceHelper.GetResourceAsString(
            FixtureResource, typeof(ArticleRenderServiceTests).Assembly);
        var article = new ArticleParser(CuneiformUrl).Parse(html);
        article.Blocks.Should().NotBeEmpty();

        //Act — compose the book and render it to a PDF
        var composer = new BookComposer(article, BookTheme.For(PageSizeOption.EightByTen), DateTime.Now);
        var document = composer.Compose();
        var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
        renderer.RenderDocument();

        var outPath = Path.Combine(GetOutDirectory(), "cuneiform-offline.pdf");
        renderer.PdfDocument.Save(outPath);

        //Assert
        File.Exists(outPath).Should().BeTrue();
        VerifyPdfSignature(outPath);
        renderer.PdfDocument.PageCount.Should().BeGreaterThan(5);
        _output.WriteLine($"Rendered {renderer.PdfDocument.PageCount} pages to {outPath}");
    }

    [Fact]
    public async Task Compose_and_render_fixture_at_six_by_nine()
    {
        //Arrange
        var html = await EmbeddedResourceHelper.GetResourceAsString(
            FixtureResource, typeof(ArticleRenderServiceTests).Assembly);
        var article = new ArticleParser(CuneiformUrl).Parse(html);

        //Act
        var composer = new BookComposer(article, BookTheme.For(PageSizeOption.SixByNine), DateTime.Now);
        var renderer = new PdfDocumentRenderer(unicode: true) { Document = composer.Compose() };
        renderer.RenderDocument();

        var outPath = Path.Combine(GetOutDirectory(), "cuneiform-6x9.pdf");
        renderer.PdfDocument.Save(outPath);

        //Assert
        VerifyPdfSignature(outPath);
        renderer.PdfDocument.PageCount.Should().BeGreaterThan(5);
    }

    //NOTE: This test goes to the live Wikipedia site (like the foundational
    //  CreateTestPdfFromOnlineArticle test in CodeBrix.PdfDocuments) and downloads
    //  all article images — expect it to take a couple of minutes.
    [Fact]
    public async Task RenderArticleAsync_end_to_end_produces_illustrated_book()
    {
        //Arrange
        using var service = new ArticleRenderService();
        var outDir = GetOutDirectory();
        var request = new RenderRequest
        {
            ArticleUrl = CuneiformUrl,
            OutputDirectory = outDir,
            PageSize = PageSizeOption.EightByTen,
            OutputFileName = "cuneiform-live.pdf"
        };

        //Act
        var result = await service.RenderArticleAsync(
            request,
            new Progress<RenderProgress>(p => _output.WriteLine($"[{p.PercentComplete,3}%] {p.Stage}: {p.Message}")),
            TestContext.Current.CancellationToken);

        //Assert
        result.Title.Should().Be("Cuneiform");
        File.Exists(result.OutputFilePath).Should().BeTrue();
        VerifyPdfSignature(result.OutputFilePath);
        result.PageCount.Should().BeGreaterThan(10);
        result.ImageCount.Should().BeGreaterThan(3);

        _output.WriteLine($"Book: {result.OutputFilePath}");
        _output.WriteLine($"Pages: {result.PageCount}, images: {result.ImageCount}, took {result.Elapsed.TotalSeconds:F1}s");
        foreach (var warning in result.Warnings)
        {
            _output.WriteLine($"  note: {warning}");
        }
    }

    [Fact]
    public async Task SearchArticlesAsync_finds_cuneiform()
    {
        //Arrange
        using var service = new ArticleRenderService();

        //Act
        var results = await service.SearchArticlesAsync(
            "cuneiform writing", cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.Title.Contains("Cuneiform", StringComparison.OrdinalIgnoreCase));
        foreach (var result in results)
        {
            result.ArticleUrl.Should().StartWith("https://en.wikipedia.org/wiki/");
        }
    }
}
