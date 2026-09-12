using SilverAssertions;
using SimpleCbxVideoPlayer.SkiaVideo.Assets;
using System.IO;
using Xunit;

namespace SimpleCbxVideoPlayer.SkiaVideo.Tests;

public class SampleAssetsTests
{
    [Fact]
    public void FindAssetsRoot_finds_the_corpus_beside_the_application()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        temp.CreateFolder("Assets", "authoring");

        //Act
        var root = SampleAssets.FindAssetsRoot(temp.Path);

        //Assert
        root.Should().Be(temp.Path);
    }

    [Fact]
    public void FindAssetsRoot_returns_null_when_the_folder_holds_no_corpus()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        var folder = temp.CreateFolder("nothing", "here");

        //Act
        var root = SampleAssets.FindAssetsRoot(folder);

        //Assert
        root.Should().BeNull();
    }

    [Fact]
    public void FindAssetsRoot_does_not_walk_up_to_an_ancestor_holding_the_corpus()
    {
        //Arrange
        using TempFolder temp = new TempFolder();
        temp.CreateFolder("Assets", "authoring");
        var deep = temp.CreateFolder("bin", "Debug", "net10.0");

        //Act
        var root = SampleAssets.FindAssetsRoot(deep);

        //Assert
        root.Should().BeNull();
    }

    [Fact]
    public void FindAssetsRoot_returns_null_for_a_blank_start()
    {
        //Act
        var root = SampleAssets.FindAssetsRoot("  ");

        //Assert
        root.Should().BeNull();
    }

    [Fact]
    public void GetAuthoringFolder_and_GetLutsFolder_land_inside_the_application_folder()
    {
        //Arrange
        var root = Path.Combine("a", "b");

        //Act
        var authoring = SampleAssets.GetAuthoringFolder(root);
        var luts = SampleAssets.GetLutsFolder(root);

        //Assert
        authoring.Should().Be(Path.Combine(root, "Assets", "authoring"));
        luts.Should().Be(Path.Combine(root, "Assets", "LUTs"));
    }
}
