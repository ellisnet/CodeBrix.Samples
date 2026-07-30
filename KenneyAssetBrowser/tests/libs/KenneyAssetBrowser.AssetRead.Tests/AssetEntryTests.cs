using KenneyAssetBrowser.AssetRead.Models;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class AssetEntryTests
{
    [Fact]
    public void Nested_entry_path_is_split_into_parts()
    {
        //Act
        var entry = new AssetEntry("Models/GLB format/apple-half.glb", 1234, AssetKind.Model3D);

        //Assert
        entry.FileName.Should().Be("apple-half.glb");
        entry.Name.Should().Be("apple-half");
        entry.Extension.Should().Be("glb");
        entry.Category.Should().Be("Models / GLB format");
        entry.SizeBytes.Should().Be(1234L);
    }

    [Fact]
    public void Root_entry_has_empty_category()
    {
        //Act
        var entry = new AssetEntry("License.txt", 10, AssetKind.Document);

        //Assert
        entry.FileName.Should().Be("License.txt");
        entry.Name.Should().Be("License");
        entry.Extension.Should().Be("txt");
        entry.Category.Should().Be("");
    }

    [Fact]
    public void Extension_is_lower_cased()
    {
        //Act
        var entry = new AssetEntry("PNG/UPPER.PNG", 1, AssetKind.Image);

        //Assert
        entry.Extension.Should().Be("png");
    }
}
