using KenneyAssetBrowser.AssetRead.Models;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class KenneyBundleReaderTests : IDisposable
{
    private readonly string _root;
    private readonly string _zipPath;

    public KenneyBundleReaderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "kenney-asset-browser-tests", Path.GetRandomFileName());
        _zipPath = Path.Combine(_root, "kenney_test-kit.zip");

        TestZipBuilder.Build(_zipPath, new Dictionary<string, byte[]>
        {
            ["License.txt"] = TestZipBuilder.Text("\tTest Kit (2.0)\r\n\r\n\tLicense: (Creative Commons Zero, CC0)\r\n"),
            ["Preview.png"] = [1, 2, 3, 4],
            ["Models/GLB format/apple.glb"] = [5, 6, 7],
            ["Models/OBJ format/apple.obj"] = TestZipBuilder.Text("v 0 0 0"),
            ["Models/OBJ format/apple.mtl"] = TestZipBuilder.Text("newmtl apple"),
            ["Models/FBX format/apple.fbx"] = [8, 9],
            ["Previews/apple.png"] = [10, 11],
            ["PNG/Default/ballBlue.png"] = [12, 13, 14],
            ["Spritesheet/sheet.xml"] = TestZipBuilder.Text(
                "<TextureAtlas imagePath=\"sheet.png\">" +
                "<SubTexture name=\"ballBlue.png\" x=\"1\" y=\"2\" width=\"3\" height=\"4\"/>" +
                "</TextureAtlas>"),
            ["Spritesheet/sheet.png"] = [15, 16],
        });
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    [Fact]
    public void Bundle_identity_comes_from_the_license_header()
    {
        //Act
        var bundle = KenneyBundleReader.ReadBundle(_zipPath);

        //Assert
        bundle.DisplayName.Should().Be("Test Kit");
        bundle.Version.Should().Be("2.0");
        bundle.LicenseText.Should().Contain("Creative Commons Zero");
        bundle.PreviewEntryPath.Should().Be("Preview.png");
        bundle.FileName.Should().Be("kenney_test-kit.zip");
    }

    [Fact]
    public void Every_file_entry_is_listed_and_classified()
    {
        //Act
        var bundle = KenneyBundleReader.ReadBundle(_zipPath);

        //Assert
        bundle.Entries.Count.Should().Be(10);
        bundle.Entries.Count(e => e.Kind == AssetKind.Image).Should().Be(4);
        bundle.Entries.Count(e => e.Kind == AssetKind.Model3D).Should().Be(3);
        bundle.Entries.Count(e => e.Kind == AssetKind.Material).Should().Be(1);
        bundle.Categories.Should().Contain("Models / GLB format");
    }

    [Fact]
    public void Model_variants_group_under_one_model_with_preview_and_material()
    {
        //Act
        var bundle = KenneyBundleReader.ReadBundle(_zipPath);

        //Assert
        bundle.ModelAssets.Count.Should().Be(1);
        var model = bundle.ModelAssets[0];
        model.Name.Should().Be("apple");
        model.Variants.Count.Should().Be(3);
        model.Variants[0].Extension.Should().Be("glb"); //GLB is the preferred viewer format
        model.PreviewEntryPath.Should().Be("Previews/apple.png");
        model.MaterialEntryPath.Should().Be("Models/OBJ format/apple.mtl");
        model.FormatList.Should().Be("FBX, GLB, OBJ");
    }

    [Fact]
    public void Sprite_atlas_is_parsed_and_its_image_path_resolved()
    {
        //Act
        var bundle = KenneyBundleReader.ReadBundle(_zipPath);

        //Assert
        bundle.Atlases.Count.Should().Be(1);
        bundle.Atlases[0].ImageEntryPath.Should().Be("Spritesheet/sheet.png");
        bundle.Atlases[0].Regions.Count.Should().Be(1);
    }

    [Fact]
    public void Atlas_with_stale_image_path_falls_back_to_the_sibling_sheet()
    {
        //Arrange — the XML declares an image that is not in the archive
        var zipPath = Path.Combine(_root, "kenney_stale-atlas.zip");
        TestZipBuilder.Build(zipPath, new Dictionary<string, byte[]>
        {
            ["Spritesheet/spaceShooter2_spritesheet.xml"] = TestZipBuilder.Text(
                "<TextureAtlas imagePath=\"sprites.png\">" +
                "<SubTexture name=\"astronaut.png\" x=\"1\" y=\"2\" width=\"3\" height=\"4\"/>" +
                "</TextureAtlas>"),
            ["Spritesheet/spaceShooter2_spritesheet.png"] = [1, 2],
        });

        //Act
        var bundle = KenneyBundleReader.ReadBundle(zipPath);

        //Assert
        bundle.Atlases.Count.Should().Be(1);
        bundle.Atlases[0].ImageEntryPath.Should().Be("Spritesheet/spaceShooter2_spritesheet.png");
    }

    [Fact]
    public void Entry_bytes_round_trip_through_the_archive()
    {
        //Arrange
        using var archive = new BundleArchive(_zipPath);

        //Act
        var bytes = archive.ReadEntryBytes("PNG/Default/ballBlue.png");

        //Assert
        bytes.Should().Equal(new byte[] { 12, 13, 14 });
    }

    [Fact]
    public void Dependency_resolves_relative_to_the_referencing_entry()
    {
        //Arrange — the layout Kenney GLB kits use: the texture sits beside the model
        var zipPath = Path.Combine(_root, "kenney_dependency.zip");
        TestZipBuilder.Build(zipPath, new Dictionary<string, byte[]>
        {
            ["Models/GLB format/apple.glb"] = [1],
            ["Models/GLB format/Textures/colormap.png"] = [2, 2],
            ["Models/Textures/shared.png"] = [3, 3, 3],
            ["stray.png"] = [4, 4, 4, 4],
        });
        using var archive = new BundleArchive(zipPath);

        //Assert — sibling folder, parent walk-up, bare-name fallback, and a clean miss
        archive.ReadDependencyBytes("Models/GLB format/apple.glb", "Textures/colormap.png")
            .Should().Equal(new byte[] { 2, 2 });
        archive.ReadDependencyBytes("Models/GLB format/apple.glb", "Textures/shared.png")
            .Should().Equal(new byte[] { 3, 3, 3 });
        archive.ReadDependencyBytes("Models/GLB format/apple.glb", "../elsewhere/stray.png")
            .Should().Equal(new byte[] { 4, 4, 4, 4 });
        archive.ReadDependencyBytes("Models/GLB format/apple.glb", "Textures/missing.png")
            .Should().BeNull();
    }

    [Fact]
    public void Entry_lookups_ignore_case_and_missing_entries_return_null()
    {
        //Arrange
        using var archive = new BundleArchive(_zipPath);

        //Assert
        archive.HasEntry("preview.PNG").Should().Be(true);
        archive.ReadEntryBytes("not/there.png").Should().BeNull();
        archive.ReadEntryText("nope.txt").Should().Be(null);
    }
}
