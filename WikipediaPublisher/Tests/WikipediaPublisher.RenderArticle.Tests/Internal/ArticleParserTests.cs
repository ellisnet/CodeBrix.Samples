using SilverAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using WikipediaPublisher.RenderArticle.Internal;
using WikipediaPublisher.RenderArticle.Models;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests.Internal;

public class ArticleParserTests
{
    private const string FixtureUrl = "https://en.wikipedia.org/wiki/Cuneiform";
    private const string FixtureResource = "WikipediaPublisher.RenderArticle.Tests.Fixtures.cuneiform.html";

    private static string _fixtureHtml;

    private static async Task<ParsedArticle> ParseFixture()
    {
        _fixtureHtml ??= await EmbeddedResourceHelper.GetResourceAsString(
            FixtureResource, typeof(ArticleParserTests).Assembly);
        return new ArticleParser(FixtureUrl).Parse(_fixtureHtml);
    }

    [Fact]
    public async Task Parse_finds_article_title()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        article.Title.Should().Be("Cuneiform");
    }

    [Fact]
    public async Task Parse_finds_short_description()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        article.ShortDescription.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Parse_finds_lead_image()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        article.LeadImage.Should().NotBeNull();
        article.LeadImage.PrintUrl.Should().StartWith("https://");
    }

    [Fact]
    public async Task Parse_produces_headings_paragraphs_and_images()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        article.Blocks.Should().NotBeEmpty();
        article.Blocks.Count(b => b.Type == ArticleBlockType.Heading && b.HeadingLevel == 1)
            .Should().BeGreaterThan(3);
        article.Blocks.Count(b => b.Type == ArticleBlockType.Paragraph).Should().BeGreaterThan(20);
        article.Blocks.Count(b => b.Type == ArticleBlockType.Image).Should().BeGreaterThan(3);
    }

    [Fact]
    public async Task Parse_excludes_stop_sections()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        var headings = article.Blocks
            .Where(b => b.Type == ArticleBlockType.Heading)
            .Select(b => b.Text)
            .ToList();

        headings.Should().NotContain("REFERENCES");
        headings.Should().NotContain("References");
        headings.Should().NotContain("External links");
        headings.Should().NotContain("Further reading");
    }

    [Fact]
    public async Task Parse_strips_citation_markers_from_paragraphs()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert — reference markers render as "[12]" etc. in raw text
        var allText = string.Concat(article.Blocks
            .Where(b => b.Type == ArticleBlockType.Paragraph)
            .SelectMany(b => b.Runs)
            .Select(r => r.Text));

        allText.Should().NotContain("[1]");
        allText.Should().NotContain("[2]");
    }

    [Fact]
    public async Task Parse_captures_image_captions()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert
        var captioned = article.Blocks
            .Where(b => b.Type == ArticleBlockType.Image)
            .Count(b => !string.IsNullOrWhiteSpace(b.Image.Caption));
        captioned.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task Parse_upgrades_thumbnails_to_print_resolution()
    {
        //Arrange + Act
        var article = await ParseFixture();

        //Assert — at least one image should have been upgraded beyond its page thumbnail
        var upgraded = article.Blocks
            .Where(b => b.Type == ArticleBlockType.Image)
            .Count(b => b.Image.PrintUrl != b.Image.ThumbUrl);
        upgraded.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(
        "https://upload.wikimedia.org/wikipedia/commons/thumb/6/66/Photo.jpg/250px-Photo.jpg",
        4000,
        "https://upload.wikimedia.org/wikipedia/commons/thumb/6/66/Photo.jpg/1800px-Photo.jpg")]
    [InlineData( //Small file: never upscale a raster image beyond its true width
        "https://upload.wikimedia.org/wikipedia/commons/thumb/6/66/Photo.jpg/250px-Photo.jpg",
        900,
        "https://upload.wikimedia.org/wikipedia/commons/thumb/6/66/Photo.jpg/900px-Photo.jpg")]
    [InlineData( //Not a thumbnail URL: leave it alone
        "https://upload.wikimedia.org/wikipedia/commons/6/66/Photo.jpg",
        4000,
        "https://upload.wikimedia.org/wikipedia/commons/6/66/Photo.jpg")]
    public void DerivePrintUrl_upgrades_correctly(string src, int fileWidth, string expected)
    {
        //Arrange + Act
        var result = ArticleParser.DerivePrintUrl(src, fileWidth, src.Split('?')[0]);

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DerivePrintUrl_rasterizes_svg_renditions_at_target_size()
    {
        //Arrange
        const string src =
            "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Map.svg/330px-Map.svg.png";

        //Act — SVG renditions can be rasterized at any size regardless of nominal file width
        var result = ArticleParser.DerivePrintUrl(src, 330, src);

        //Assert
        result.Should().Contain("/1800px-Map.svg.png");
    }

    [Fact]
    public void ExtractTextRuns_propagates_bold_and_italic()
    {
        //Arrange
        var parser = new CodeBrix.MarkupParse.Html.Parser.HtmlParser();
        var document = parser.ParseDocument("<p>Plain <b>bold <i>bolditalic</i></b> <em>italic</em></p>");
        var paragraph = document.QuerySelector("p");

        //Act
        var runs = ArticleParser.ExtractTextRuns(paragraph);

        //Assert
        runs.Should().Contain(r => r.Text == "Plain " && !r.Bold && !r.Italic);
        runs.Should().Contain(r => r.Text == "bold " && r.Bold && !r.Italic);
        runs.Should().Contain(r => r.Text == "bolditalic" && r.Bold && r.Italic);
        runs.Should().Contain(r => r.Text == "italic" && !r.Bold && r.Italic);
    }

    [Fact]
    public void ExtractTextRuns_skips_reference_superscripts()
    {
        //Arrange
        var parser = new CodeBrix.MarkupParse.Html.Parser.HtmlParser();
        var document = parser.ParseDocument(
            "<p>Fact<sup class=\"reference\">[1]</sup> and x<sup>2</sup></p>");
        var paragraph = document.QuerySelector("p");

        //Act
        var runs = ArticleParser.ExtractTextRuns(paragraph);

        //Assert
        var text = string.Concat(runs.Select(r => r.Text));
        text.Should().NotContain("[1]");
        runs.Should().Contain(r => r.Text == "2" && r.Superscript);
    }
}
