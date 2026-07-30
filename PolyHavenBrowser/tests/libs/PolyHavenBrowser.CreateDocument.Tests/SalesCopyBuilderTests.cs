using PolyHavenBrowser.CreateDocument.Internal;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.CreateDocument.Tests;

public class SalesCopyBuilderTests
{
    private static MarketingSheetRequest FullRequest() => new()
    {
        ModelName = "Marble Bust 1",
        Category = "decorative",
        TriangleCount = 12_204,
        VertexCount = 6_305,
        MaterialCount = 2,
        MaxTextureLabel = "8k",
        DownloadCount = 123_456,
        PublishedUtc = new DateTime(2021, 3, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public void pull_quote_uses_the_stripped_name()
    {
        //Act
        var quote = SalesCopyBuilder.BuildPullQuote(FullRequest());

        //Assert
        quote.Should().Be("“I am the Marble Bust model you have been looking for.”");
    }

    [Fact]
    public void pull_quote_signature_uses_the_full_name()
    {
        //Act
        var signature = SalesCopyBuilder.BuildPullQuoteSignature(FullRequest());

        //Assert
        signature.Should().Be("— Marble Bust 1");
    }

    [Fact]
    public void kicker_includes_the_category_in_caps()
    {
        //Act
        var kicker = SalesCopyBuilder.BuildKicker(FullRequest());

        //Assert
        kicker.Should().Be("POLY HAVEN COLLECTION · DECORATIVE");
    }

    [Fact]
    public void kicker_without_category_is_the_collection_line_alone()
    {
        //Arrange
        var request = new MarketingSheetRequest { ModelName = "Camera 01" };

        //Act
        var kicker = SalesCopyBuilder.BuildKicker(request);

        //Assert
        kicker.Should().Be("POLY HAVEN COLLECTION");
    }

    [Fact]
    public void tagline_mentions_the_texture_resolution_when_known()
    {
        //Act
        var tagline = SalesCopyBuilder.BuildTagline(FullRequest());

        //Assert
        tagline.Should().Contain("8k");
    }

    [Fact]
    public void sales_paragraph_weaves_in_the_real_numbers()
    {
        //Act
        var paragraph = SalesCopyBuilder.BuildSalesParagraph(FullRequest());

        //Assert
        paragraph.Should().Contain("12,204");
        paragraph.Should().Contain("6,305");
        paragraph.Should().Contain("2 PBR materials");
        paragraph.Should().Contain("8k");
        paragraph.Should().Contain("123,456");
        paragraph.Should().Contain("March 2021");
        paragraph.Should().Contain("CC0");
        paragraph.Should().Contain("The Marble Bust is not a model you audition.");
    }

    [Fact]
    public void sales_paragraph_survives_a_request_with_no_facts_at_all()
    {
        //Arrange
        var request = new MarketingSheetRequest { ModelName = "Mystery Model" };

        //Act
        var paragraph = SalesCopyBuilder.BuildSalesParagraph(request);

        //Assert - the license promise and the closer always land
        paragraph.Should().Contain("CC0");
        paragraph.Should().Contain("The Mystery Model is not a model you audition.");
        paragraph.Should().NotContain("triangles");
    }

    [Fact]
    public void building_the_same_request_twice_gives_identical_copy()
    {
        //Act
        var first = SalesCopyBuilder.BuildSalesParagraph(FullRequest());
        var second = SalesCopyBuilder.BuildSalesParagraph(FullRequest());

        //Assert
        first.Should().Be(second);
    }
}
