using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>Live tests for the per-asset endpoints (/info, /files, /author) and thumbnails.</summary>
[Collection("LiveApi")]
[Trait("Category", "LiveApi")]
public class LiveAssetDetailTests(LiveApiFixture fixture)
{
    private IPolyHavenApiClient Client => fixture.Client;

    [Fact]
    public async Task hdri_info_has_the_expected_metadata()
    {
        //Act
        var asset = await Client.GetAssetAsync("abandoned_bakery");

        //Assert
        asset.Id.Should().Be("abandoned_bakery");
        asset.Name.Should().Be("Abandoned Bakery");
        asset.Type.Should().Be(PolyHavenAssetType.Hdri);
        asset.EvsCap.Should().Be(16);
        asset.Whitebalance.Should().Be(4950);
        asset.Backplates.Should().BeTrue();
        asset.Coords.Should().HaveCount(2);
        asset.MaxResolution.Should().Equal(16384, 8192);
        asset.Authors.Should().ContainKey("Sergej Majboroda");
        asset.DatePublishedUtc.Year.Should().Be(2022);
        asset.DateTakenUtc.Should().NotBeNull();
        asset.DownloadCount.Should().BeGreaterThan(0);
        asset.ThumbnailUrl.Should().StartWith("https://");
    }

    [Fact]
    public async Task texture_info_has_physical_dimensions()
    {
        //Act
        var asset = await Client.GetAssetAsync("aerial_rocks_02");

        //Assert
        asset.Type.Should().Be(PolyHavenAssetType.Texture);
        asset.Dimensions.Should().Equal(50000, 50000);
        asset.MaxResolution.Should().Equal(8192, 8192);
    }

    [Fact]
    public async Task model_info_is_returned()
    {
        //Act
        var asset = await Client.GetAssetAsync("potted_plant_01");

        //Assert
        asset.Type.Should().Be(PolyHavenAssetType.Model);
        asset.Name.Should().NotBeNullOrEmpty();
        asset.Authors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task hdri_files_expose_resolutions_formats_and_extras()
    {
        //Act
        var files = await Client.GetAssetFilesAsync("abandoned_bakery");

        //Assert
        var oneK = files.FindFile("hdri", "1k", "hdr");
        oneK.Should().NotBeNull();
        oneK!.Url.Should().StartWith("https://");
        oneK.Size.Should().BeGreaterThan(0);
        oneK.Md5.Should().HaveLength(32);

        files.FindFile("hdri", "4k", "exr").Should().NotBeNull();
        files.FindFile("tonemapped").Should().NotBeNull();
        files.EnumerateFiles().Count().Should().BeGreaterThan(5);
    }

    [Fact]
    public async Task texture_files_include_bundles_with_their_dependencies()
    {
        //Act
        var files = await Client.GetAssetFilesAsync("aerial_rocks_02");

        //Assert
        files.FindFile("Diffuse", "1k", "jpg").Should().NotBeNull();

        var blend = files.FindFile("blend", "1k", "blend");
        blend.Should().NotBeNull();
        blend!.Include.Should().NotBeNullOrEmpty();
        blend.Include!.Values.Should().AllSatisfy(include => include.Url.Should().StartWith("https://"));
    }

    [Fact]
    public async Task model_files_expose_multiple_formats()
    {
        //Act
        var files = await Client.GetAssetFilesAsync("potted_plant_01");

        //Assert
        var all = files.EnumerateFiles().ToList();
        all.Should().NotBeEmpty();
        all.Should().Contain(entry => entry.Path.StartsWith("gltf/") || entry.Path.StartsWith("blend/") || entry.Path.StartsWith("fbx/"));
    }

    [Fact]
    public async Task author_info_is_returned()
    {
        //Act
        var author = await Client.GetAuthorAsync("Sergej Majboroda");

        //Assert
        author.Name.Should().Be("Sergej Majboroda");
        author.Link.Should().StartWith("https://");
    }

    [Fact]
    public async Task thumbnail_bytes_can_be_fetched_at_a_requested_size()
    {
        //Arrange
        var asset = await Client.GetAssetAsync("abandoned_bakery");

        //Act
        var bytes = await Client.GetThumbnailAsync(asset, 128, 128);

        //Assert
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(1000);
    }
}
