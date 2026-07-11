using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class RestClientParsingTests
{
    [Fact]
    public async Task asset_types_are_deserialized()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/types", CannedJson.Types);

        //Act
        IReadOnlyList<string> types;
        using (client)
        {
            types = await client.GetAssetTypesAsync();
        }

        //Assert
        types.Should().Equal("hdris", "textures", "models");
    }

    [Fact]
    public async Task asset_list_is_deserialized_with_ids_assigned()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/assets", CannedJson.Assets);

        //Act
        IReadOnlyDictionary<string, PolyHavenAsset> assets;
        using (client)
        {
            assets = await client.GetAssetsAsync();
        }

        //Assert
        assets.Should().HaveCount(2);
        assets.Should().ContainKey("abandoned_bakery");
        assets.Should().ContainKey("aerial_rocks_02");
        assets["abandoned_bakery"].Id.Should().Be("abandoned_bakery");
        assets["abandoned_bakery"].Type.Should().Be(PolyHavenAssetType.Hdri);
        assets["aerial_rocks_02"].Id.Should().Be("aerial_rocks_02");
        assets["aerial_rocks_02"].Type.Should().Be(PolyHavenAssetType.Texture);
    }

    [Fact]
    public async Task hdri_asset_fields_are_deserialized()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/info/", CannedJson.AssetInfo);

        //Act
        PolyHavenAsset asset;
        using (client)
        {
            asset = await client.GetAssetAsync("abandoned_bakery");
        }

        //Assert
        asset.Id.Should().Be("abandoned_bakery");
        asset.Name.Should().Be("Abandoned Bakery");
        asset.Type.Should().Be(PolyHavenAssetType.Hdri);
        asset.Categories.Should().Contain("indoor");
        asset.Tags.Should().Contain("abandoned");
        asset.Authors.Should().ContainKey("Sergej Majboroda");
        asset.Description.Should().NotBeNullOrEmpty();
        asset.DownloadCount.Should().Be(18040);
        asset.FilesHash.Should().Be("d81af70dd51ebb704af086506e0a9b92bb5d7b84");
        asset.ThumbnailUrl.Should().StartWith("https://cdn.polyhaven.com/");
        asset.MaxResolution.Should().Equal(16384, 8192);
        asset.Coords.Should().HaveCount(2);
        asset.Backplates.Should().BeTrue();
        asset.EvsCap.Should().Be(16);
        asset.Whitebalance.Should().Be(4950);
        asset.DatePublishedUtc.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1663804800));
        asset.DateTakenUtc.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1662805680));
    }

    [Fact]
    public async Task texture_asset_dimensions_are_deserialized()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/assets", CannedJson.Assets);

        //Act
        IReadOnlyDictionary<string, PolyHavenAsset> assets;
        using (client)
        {
            assets = await client.GetAssetsAsync();
        }

        //Assert
        assets["aerial_rocks_02"].Dimensions.Should().Equal(50000, 50000);
        assets["abandoned_bakery"].Dimensions.Should().BeNull();
    }

    [Fact]
    public async Task unmapped_fields_are_preserved_in_extension_data()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/info/", CannedJson.AssetInfo);

        //Act
        PolyHavenAsset asset;
        using (client)
        {
            asset = await client.GetAssetAsync("abandoned_bakery");
        }

        //Assert
        asset.ExtensionData.Should().NotBeNull();
        asset.ExtensionData.Should().ContainKey("attributes");
        asset.ExtensionData.Should().ContainKey("category");
        asset.ExtensionData!["category"].GetString().Should().Contain("Abandoned");
    }

    [Fact]
    public async Task author_is_deserialized_with_extension_data()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/author/", CannedJson.Author);

        //Act
        PolyHavenAuthor author;
        using (client)
        {
            author = await client.GetAuthorAsync("Sergej Majboroda");
        }

        //Assert
        author.Name.Should().Be("Sergej Majboroda");
        author.Link.Should().Be("https://hdrmarket.com/");
        author.Email.Should().BeNull();
        author.ExtensionData.Should().ContainKey("encryptedEmail");
    }

    [Fact]
    public async Task categories_are_deserialized_with_counts()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/categories/", CannedJson.Categories);

        //Act
        IReadOnlyDictionary<string, int> categories;
        using (client)
        {
            categories = await client.GetCategoriesAsync(PolyHavenAssetType.Hdri);
        }

        //Assert
        categories.Should().HaveCount(4);
        categories["all"].Should().Be(978);
        categories["natural light"].Should().Be(805);
    }
}
