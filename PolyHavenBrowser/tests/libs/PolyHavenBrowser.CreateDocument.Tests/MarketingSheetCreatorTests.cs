using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.PdfDocuments.Pdf.IO;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.CreateDocument.Tests;

public class MarketingSheetCreatorTests
{
    private static byte[] TestPng(byte r, byte g, byte b, int width = 64, int height = 48)
    {
        using var image = new Image<Rgba32>(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                image[x, y] = new Rgba32(r, g, b);
            }
        }

        using var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        return stream.ToArray();
    }

    private static MarketingSheetRequest FullRequest() => new()
    {
        ModelName = "Marble Bust 1",
        AuthorLine = "by ulrickwery   ·   Poly Haven",
        Description = "“Marble Bust 1” is a free, CC0-licensed decorative 3D model from Poly Haven.",
        Facts =
        [
            new MarketingSheetFact("Categories", "decorative"),
            new MarketingSheetFact("Published", "March 1, 2021"),
            new MarketingSheetFact("Downloads", "123,456"),
            new MarketingSheetFact("Triangles", "12,204"),
            new MarketingSheetFact("Vertices", "6,305"),
            new MarketingSheetFact("Materials", "2 (1 textured)"),
            new MarketingSheetFact("Size on disk", "34.2 MB"),
            new MarketingSheetFact("License", "CC0 (public domain)"),
        ],
        Tags = ["bust", "marble", "statue"],
        AssetUrl = "https://polyhaven.com/a/marble_bust_01",
        Category = "decorative",
        CatalogThumbnailBytes = TestPng(160, 60, 40),
        HeroShotBytes = TestPng(120, 100, 90, 336, 280),
        GalleryShots =
        [
            new MarketingSheetShot("Front", TestPng(100, 100, 100)),
            new MarketingSheetShot("Side", TestPng(110, 110, 110)),
            new MarketingSheetShot("Back", TestPng(120, 120, 120)),
            new MarketingSheetShot("Top", TestPng(130, 130, 130)),
        ],
        TriangleCount = 12_204,
        VertexCount = 6_305,
        MaterialCount = 2,
        MaxTextureLabel = "8k",
        DownloadCount = 123_456,
        PublishedUtc = new DateTime(2021, 3, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public void creates_a_single_page_letter_pdf()
    {
        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(FullRequest());

        //Assert
        bytes.Length.Should().BeGreaterThan(1000);

        using var stream = new MemoryStream(bytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        document.PageCount.Should().Be(1);
        Math.Round(document.Pages[0].Width.Point).Should().Be(612d);
        Math.Round(document.Pages[0].Height.Point).Should().Be(792d);
    }

    [Fact]
    public void sets_the_document_title_from_the_model_name()
    {
        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(FullRequest());

        //Assert
        using var stream = new MemoryStream(bytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        document.Info.Title.Should().Contain("Marble Bust 1");
    }

    [Fact]
    public void survives_a_minimal_request_with_no_images_or_facts()
    {
        //Arrange - the degenerate case: nothing but a name
        var request = new MarketingSheetRequest { ModelName = "Mystery Model" };

        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(request);

        //Assert
        using var stream = new MemoryStream(bytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        document.PageCount.Should().Be(1);
    }

    [Fact]
    public void survives_broken_image_bytes()
    {
        //Arrange
        var request = new MarketingSheetRequest
        {
            ModelName = "Broken Images 1",
            CatalogThumbnailBytes = [9, 9, 9],
            HeroShotBytes = [1, 2, 3],
            GalleryShots = [new MarketingSheetShot("Front", [4, 5, 6])],
        };

        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(request);

        //Assert
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public void creates_the_file_on_disk()
    {
        //Arrange
        var path = Path.Combine(Path.GetTempPath(), $"one-sheet-{Guid.NewGuid():N}.pdf");

        try
        {
            //Act
            new MarketingSheetCreator().CreateToFile(FullRequest(), path);

            //Assert
            File.Exists(path).Should().BeTrue();
            new FileInfo(path).Length.Should().BeGreaterThan(1000);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void the_catalog_inset_shows_a_tall_thumbnail_whole_and_unstretched()
    {
        //Arrange - a portrait thumbnail, the shape whose top and bottom used to be cropped
        //  away (Poly Haven trims its thumbnails to the model, so a bust arrives taller than
        //  it is wide).
        var request = new MarketingSheetRequest
        {
            ModelName = "Tall Model 1",
            CatalogThumbnailBytes = TestPng(200, 40, 40, width: 100, height: 300),
            HeroShotBytes = TestPng(120, 100, 90, 336, 280),
        };

        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(request);

        //Assert - drawn with one uniform scale (never stretched), fitting entirely inside the
        //  inset's 180 x 150 point box (never cropped).
        var placement = FindImagePlacement(bytes, pixelWidth: 100, pixelHeight: 300);
        (placement.Width / 100d).Should().BeApproximately(placement.Height / 300d, 0.001);
        placement.Width.Should().BeLessThanOrEqualTo(180d);
        placement.Height.Should().BeLessThanOrEqualTo(150d);
    }

    [Fact]
    public void the_catalog_inset_gives_a_landscape_thumbnail_the_full_column_width()
    {
        //Arrange - a 4:3 thumbnail, which only reaches the full column width because the
        //  inset box is tall enough to hold it whole.
        var request = new MarketingSheetRequest
        {
            ModelName = "Wide Model 1",
            CatalogThumbnailBytes = TestPng(40, 90, 200, width: 400, height: 300),
            HeroShotBytes = TestPng(120, 100, 90, 336, 280),
        };

        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(request);

        //Assert
        var placement = FindImagePlacement(bytes, pixelWidth: 400, pixelHeight: 300);
        placement.Width.Should().BeApproximately(180d, 0.001);
        placement.Height.Should().BeApproximately(135d, 0.001);
    }

    //Reads back where an image landed on the page: finds the drawn XObject by its pixel size,
    //  then returns the size (in points) of the box it was drawn into. Every image the sheet
    //  draws gets its own "q <w> 0 0 <h> <x> <y> cm /Name Do Q" run in the content stream.
    private static (double Width, double Height) FindImagePlacement(
        byte[] pdfBytes, int pixelWidth, int pixelHeight)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        var page = document.Pages[0];

        var xObjects = page.Elements.GetDictionary("/Resources")?.Elements.GetDictionary("/XObject");
        var name = xObjects?.Elements.Keys.FirstOrDefault(key =>
            xObjects.Elements.GetDictionary(key) is { } image
            && image.Elements.GetInteger("/Width") == pixelWidth
            && image.Elements.GetInteger("/Height") == pixelHeight) ?? string.Empty;
        name.Should().NotBeEmpty();

        var content = Encoding.ASCII.GetString(
            page.Contents.Elements.GetDictionary(0).Stream.UnfilteredValue);
        var match = Regex.Match(content,
            @"q\s+([\d.]+)\s+0\s+0\s+([\d.]+)\s+[-\d.]+\s+[-\d.]+\s+cm\s*" + Regex.Escape(name) + @"\s+Do");
        match.Success.Should().BeTrue();

        return (double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void a_long_description_still_fits_on_one_page()
    {
        //Arrange
        var request = new MarketingSheetRequest
        {
            ModelName = "Wordy Model 3",
            Description = string.Join(" ", Enumerable.Repeat(
                "An exhaustively detailed sentence about the model's provenance and construction.", 40)),
        };

        //Act
        var bytes = new MarketingSheetCreator().CreateToBytes(request);

        //Assert
        using var stream = new MemoryStream(bytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        document.PageCount.Should().Be(1);
    }
}
