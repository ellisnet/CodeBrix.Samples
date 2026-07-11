using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class FileTreeTests
{
    private static async Task<PolyHavenFileTree> GetTreeAsync(string filesJson)
    {
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/files/", filesJson);
        using (client)
        {
            return await client.GetAssetFilesAsync("some_asset");
        }
    }

    [Fact]
    public async Task hdri_file_tree_exposes_nested_resolutions_and_formats()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.HdriFiles);

        //Assert
        var file = tree.FindFile("hdri", "1k", "hdr");
        file.Should().NotBeNull();
        file!.Url.Should().Be("https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/abandoned_bakery_1k.hdr");
        file.Md5.Should().Be("9a835b2f0e7a42e2b98fefd19c4a3b9d");
        file.Size.Should().Be(1730263);
    }

    [Fact]
    public async Task file_at_tree_root_is_a_leaf_node()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.HdriFiles);

        //Assert
        tree.Children.Should().ContainKey("tonemapped");
        tree.Children["tonemapped"].IsFile.Should().BeTrue();
        tree.FindFile("tonemapped")!.Size.Should().Be(48676915);
    }

    [Fact]
    public async Task grouping_node_is_not_a_file()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.HdriFiles);

        //Assert
        var node = tree.Find("hdri");
        node.Should().NotBeNull();
        node!.IsFile.Should().BeFalse();
        node.File.Should().BeNull();
        node.Children.Should().ContainKey("1k");
        tree.FindFile("hdri").Should().BeNull();
    }

    [Fact]
    public async Task find_returns_null_for_missing_paths()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.HdriFiles);

        //Assert
        tree.Find("hdri", "32k").Should().BeNull();
        tree.Find("nope").Should().BeNull();
        tree.Find().Should().BeNull();
        tree.FindFile("hdri", "1k", "png").Should().BeNull();
    }

    [Fact]
    public async Task enumerate_files_walks_every_leaf_with_slash_paths()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.HdriFiles);
        var entries = tree.EnumerateFiles().ToList();

        //Assert
        entries.Should().HaveCount(4);
        entries.Select(e => e.Path).Should().BeEquivalentTo(
            "hdri/1k/hdr", "hdri/1k/exr", "hdri/4k/hdr", "tonemapped");
        entries.Should().AllSatisfy(entry =>
        {
            entry.File.Url.Should().StartWith("https://");
            entry.File.Size.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task bundle_files_expose_their_included_files()
    {
        //Arrange / Act
        var tree = await GetTreeAsync(CannedJson.TextureFiles);

        //Assert
        var blend = tree.FindFile("blend", "blend");
        blend.Should().NotBeNull();
        blend!.Include.Should().HaveCount(2);
        blend.Include.Should().ContainKey("textures/aerial_rocks_02_diff_1k.jpg");
        blend.Include!["textures/aerial_rocks_02_diff_1k.jpg"].Size.Should().Be(785923);

        var plainFile = tree.FindFile("Diffuse", "1k", "jpg");
        plainFile.Should().NotBeNull();
        plainFile!.Include.Should().BeNull();
    }

    [Fact]
    public void manually_constructed_tree_is_traversable()
    {
        //Arrange
        var file = new PolyHavenFileRef { Url = "https://example.test/f.bin", Md5 = "00", Size = 1 };
        var tree = new PolyHavenFileTree(new Dictionary<string, PolyHavenFileNode>
        {
            ["group"] = new("group", new Dictionary<string, PolyHavenFileNode>
            {
                ["file"] = new("file", file),
            }),
        });

        //Act / Assert
        tree.FindFile("group", "file").Should().BeSameAs(file);
        tree.EnumerateFiles().Single().Path.Should().Be("group/file");
    }
}
