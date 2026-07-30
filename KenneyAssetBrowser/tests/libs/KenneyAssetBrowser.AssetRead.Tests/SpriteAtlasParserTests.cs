using KenneyAssetBrowser.AssetRead.Parsing;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class SpriteAtlasParserTests
{
    private const string AtlasXml =
        "<TextureAtlas imagePath=\"spritesheet_default.png\">" +
        "  <SubTexture name=\"ballBlue.png\" x=\"27\" y=\"338\" width=\"22\" height=\"22\"/>" +
        "  <SubTexture name=\"buttonDefault.png\" x=\"0\" y=\"0\" width=\"190\" height=\"49\"/>" +
        "</TextureAtlas>";

    [Fact]
    public void Texture_atlas_xml_is_parsed_with_regions()
    {
        //Act
        var parsed = SpriteAtlasParser.TryParse(AtlasXml, "Spritesheet/spritesheet_default.xml", out var atlas);

        //Assert
        parsed.Should().Be(true);
        atlas.Name.Should().Be("spritesheet_default");
        atlas.Regions.Count.Should().Be(2);
        atlas.Regions[0].Name.Should().Be("ballBlue.png");
        atlas.Regions[0].X.Should().Be(27);
        atlas.Regions[0].Y.Should().Be(338);
        atlas.Regions[0].Width.Should().Be(22);
        atlas.Regions[0].Height.Should().Be(22);
    }

    [Fact]
    public void Relative_image_path_resolves_beside_the_xml_file()
    {
        //Act
        SpriteAtlasParser.TryParse(AtlasXml, "Spritesheet/spritesheet_default.xml", out var atlas);

        //Assert
        atlas.ImageEntryPath.Should().Be("Spritesheet/spritesheet_default.png");
    }

    [Fact]
    public void Non_atlas_xml_is_rejected()
    {
        //Act
        var parsed = SpriteAtlasParser.TryParse("<svg width=\"10\" height=\"10\"/>", "Vector/art.xml", out var atlas);

        //Assert
        parsed.Should().Be(false);
        atlas.Should().Be(null);
    }

    [Fact]
    public void Malformed_xml_is_rejected()
    {
        //Act
        var parsed = SpriteAtlasParser.TryParse("<TextureAtlas imagePath=", "sheet.xml", out var atlas);

        //Assert
        parsed.Should().Be(false);
        atlas.Should().Be(null);
    }

    [Fact]
    public void Atlas_without_regions_is_rejected()
    {
        //Act
        var parsed = SpriteAtlasParser.TryParse(
            "<TextureAtlas imagePath=\"sheet.png\"></TextureAtlas>", "sheet.xml", out _);

        //Assert
        parsed.Should().Be(false);
    }
}
