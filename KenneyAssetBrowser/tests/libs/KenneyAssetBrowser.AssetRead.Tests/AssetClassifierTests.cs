using KenneyAssetBrowser.AssetRead.Models;
using KenneyAssetBrowser.AssetRead.Parsing;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class AssetClassifierTests
{
    [Theory]
    [InlineData("PNG/Default/ballBlue.png", AssetKind.Image)]
    [InlineData("Vector/puzzleAssets_vector.svg", AssetKind.Vector)]
    [InlineData("Models/GLB format/apple.glb", AssetKind.Model3D)]
    [InlineData("Models/OBJ format/apple.obj", AssetKind.Model3D)]
    [InlineData("Models/FBX format/apple.fbx", AssetKind.Model3D)]
    [InlineData("Models/OBJ format/apple.mtl", AssetKind.Material)]
    [InlineData("Sounds/laser.ogg", AssetKind.Audio)]
    [InlineData("Font/kenvector_future.ttf", AssetKind.Font)]
    [InlineData("License.txt", AssetKind.Document)]
    [InlineData("Overview.html", AssetKind.Document)]
    [InlineData("Spritesheet/sheet.xml", AssetKind.Document)]
    [InlineData("Samples/Tiled Sample.zip", AssetKind.Archive)]
    [InlineData("PNG/Colored/UPPER.PNG", AssetKind.Image)]
    [InlineData("Tiled/tilemap-example-a.tmx", AssetKind.TiledMap)]
    [InlineData("Tiled/tileset-tiles.tsx", AssetKind.TiledMap)]
    [InlineData("Vector/puzzleAssets_vector.swf", AssetKind.Flash)]
    [InlineData("Source/character.blend", AssetKind.SourceFile)]
    [InlineData("Source/kenney-future.woff2", AssetKind.SourceFile)]
    [InlineData("Unity/sample.unitypackage", AssetKind.EnginePackage)]
    [InlineData("Godot/tileset.tres", AssetKind.EnginePackage)]
    [InlineData("Thumbs.db", AssetKind.Unknown)]
    [InlineData("no-extension", AssetKind.Unknown)]
    [InlineData("", AssetKind.Unknown)]
    public void Extensions_map_to_expected_kinds(string entryPath, AssetKind expected)
    {
        //Act
        var kind = AssetClassifier.Classify(entryPath);

        //Assert
        kind.Should().Be(expected);
    }
}
