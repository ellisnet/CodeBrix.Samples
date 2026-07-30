using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace KenneyAssetBrowser.Rendering.Tests;

public class TiledMapTests
{
    private const string MapXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <map version="1.10" orientation="orthogonal" renderorder="right-down"
             width="2" height="1" tilewidth="4" tileheight="4">
         <tileset firstgid="1" source="tileset-tiles.tsx"/>
         <layer id="1" name="Tiles" width="2" height="1">
          <data encoding="csv">1,2147483650</data>
         </layer>
        </map>
        """;

    private const string TilesetXml =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <tileset version="1.4" name="tiles" tilewidth="4" tileheight="4" tilecount="2" columns="2">
         <image source="../Tilemap/tilemap_packed.png" width="8" height="4"/>
        </tileset>
        """;

    [Fact]
    public void Orthogonal_csv_map_parses_with_flip_bits_preserved()
    {
        //Act
        var parsed = TiledMapParser.TryParseMap(MapXml, out var map);

        //Assert
        parsed.Should().Be(true);
        map!.Width.Should().Be(2);
        map.TileWidth.Should().Be(4);
        map.Tilesets.Count.Should().Be(1);
        map.Tilesets[0].FirstGid.Should().Be(1);
        map.Tilesets[0].Source.Should().Be("tileset-tiles.tsx");
        map.Layers.Count.Should().Be(1);
        map.Layers[0].Gids[0].Should().Be(1u);
        map.Layers[0].Gids[1].Should().Be(2147483650u); //gid 2 with the horizontal-flip bit
    }

    [Fact]
    public void External_tileset_parses_with_image_reference()
    {
        //Act
        var parsed = TiledMapParser.TryParseTileset(TilesetXml, out var tileset);

        //Assert
        parsed.Should().Be(true);
        tileset!.TileCount.Should().Be(2);
        tileset.Columns.Should().Be(2);
        tileset.ImagePath.Should().Be("../Tilemap/tilemap_packed.png");
    }

    [Fact]
    public void Base64_layers_are_rejected()
    {
        //Arrange
        var xml = MapXml.Replace("encoding=\"csv\"", "encoding=\"base64\"");

        //Act
        var parsed = TiledMapParser.TryParseMap(xml, out var map);

        //Assert
        parsed.Should().Be(false);
        map.Should().BeNull();
    }

    [Fact]
    public void Renderer_places_tiles_and_honors_the_horizontal_flip()
    {
        //Arrange — tile 1 solid red; tile 2 asymmetric: left half green, right half blue.
        //The map places tile 1, then tile 2 flipped horizontally (blue lands on the LEFT).
        TiledMapParser.TryParseMap(MapXml, out var map);
        TiledMapParser.TryParseTileset(TilesetXml, out var tileset);
        using var sheet = new SKBitmap(new SKImageInfo(8, 4, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(sheet))
        {
            canvas.Clear(SKColors.Red);                                      //tile 1
            using var green = new SKPaint { Color = SKColors.Green };
            using var blue = new SKPaint { Color = SKColors.Blue };
            canvas.DrawRect(SKRect.Create(4, 0, 2, 4), green);               //tile 2 left half
            canvas.DrawRect(SKRect.Create(6, 0, 2, 4), blue);                //tile 2 right half
        }

        //Act
        using var rendered = TiledMapRenderer.Render(map!, [(1, tileset!, sheet)]);

        //Assert
        rendered.Width.Should().Be(8);
        rendered.Height.Should().Be(4);
        rendered.GetPixel(1, 1).Should().Be(SKColors.Red);                   //cell 0: tile 1
        rendered.GetPixel(5, 1).Should().Be(SKColors.Blue);                  //cell 1 left: flipped tile 2
        rendered.GetPixel(7, 1).Should().Be(SKColors.Green);                 //cell 1 right: flipped tile 2
    }
}
