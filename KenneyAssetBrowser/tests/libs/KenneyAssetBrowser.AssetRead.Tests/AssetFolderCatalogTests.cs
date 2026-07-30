using SilverAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class AssetFolderCatalogTests : IDisposable
{
    private readonly string _root;

    public AssetFolderCatalogTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "kenney-asset-browser-tests", Path.GetRandomFileName());
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    [Fact]
    public void Readable_bundles_load_and_corrupt_zips_become_warnings()
    {
        //Arrange
        TestZipBuilder.Build(Path.Combine(_root, "kenney_good-kit.zip"), new Dictionary<string, byte[]>
        {
            ["License.txt"] = TestZipBuilder.Text("Good Kit (1.0)\r\n"),
            ["PNG/item.png"] = [1, 2],
        });
        File.WriteAllText(Path.Combine(_root, "kenney_broken.zip"), "this is not a zip archive");

        //Act
        var catalog = AssetFolderCatalog.LoadFrom(_root);

        //Assert
        catalog.Bundles.Count.Should().Be(1);
        catalog.Bundles[0].DisplayName.Should().Be("Good Kit");
        catalog.Warnings.Count.Should().Be(1);
        catalog.Warnings[0].Should().Contain("kenney_broken.zip");
    }

    [Fact]
    public void Missing_folder_loads_as_an_empty_catalog()
    {
        //Act
        var catalog = AssetFolderCatalog.LoadFrom(Path.Combine(_root, "does-not-exist"));

        //Assert
        catalog.Bundles.Count.Should().Be(0);
        catalog.Warnings.Count.Should().Be(0);
    }

    [Fact]
    public void Bundles_sort_by_display_name()
    {
        //Arrange
        TestZipBuilder.Build(Path.Combine(_root, "kenney_zebra-kit.zip"), new Dictionary<string, byte[]>
        {
            ["License.txt"] = TestZipBuilder.Text("Zebra Kit (1.0)\r\n"),
        });
        TestZipBuilder.Build(Path.Combine(_root, "kenney_alpha-kit.zip"), new Dictionary<string, byte[]>
        {
            ["License.txt"] = TestZipBuilder.Text("Alpha Kit (1.0)\r\n"),
        });

        //Act
        var catalog = AssetFolderCatalog.LoadFrom(_root);

        //Assert
        catalog.Bundles.Count.Should().Be(2);
        catalog.Bundles[0].DisplayName.Should().Be("Alpha Kit");
        catalog.Bundles[1].DisplayName.Should().Be("Zebra Kit");
    }
}
