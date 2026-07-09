using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class RestClientRequestTests
{
    [Fact]
    public async Task assets_request_without_filters_has_no_query()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/assets", CannedJson.Assets);

        //Act
        using (client)
        {
            await client.GetAssetsAsync();
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/assets");
    }

    [Fact]
    public async Task assets_request_with_type_adds_type_parameter()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/assets", CannedJson.Assets);

        //Act
        using (client)
        {
            await client.GetAssetsAsync(PolyHavenAssetType.Hdri);
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/assets?type=hdris");
    }

    [Fact]
    public async Task assets_request_with_type_and_categories_encodes_both_parameters()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/assets", CannedJson.Assets);

        //Act
        using (client)
        {
            await client.GetAssetsAsync(PolyHavenAssetType.Hdri, ["outdoor", "natural light"]);
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/assets?type=hdris&categories=outdoor%2Cnatural%20light");
    }

    [Fact]
    public async Task assets_request_ignores_blank_categories()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/assets", CannedJson.Assets);

        //Act
        using (client)
        {
            await client.GetAssetsAsync(categories: ["", "   "]);
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/assets");
    }

    [Fact]
    public async Task categories_request_uses_type_path_and_in_filter()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/categories/", CannedJson.Categories);

        //Act
        using (client)
        {
            await client.GetCategoriesAsync(PolyHavenAssetType.Texture, ["outdoor"]);
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/categories/textures?in=outdoor");
    }

    [Fact]
    public async Task info_request_uses_info_path()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/info/", CannedJson.AssetInfo);

        //Act
        using (client)
        {
            await client.GetAssetAsync("abandoned_bakery");
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/info/abandoned_bakery");
    }

    [Fact]
    public async Task author_request_escapes_the_author_id()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/author/", CannedJson.Author);

        //Act
        using (client)
        {
            await client.GetAuthorAsync("Sergej Majboroda");
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://api.polyhaven.com/author/Sergej%20Majboroda");
    }

    [Fact]
    public async Task requests_carry_the_configured_user_agent()
    {
        //Arrange
        var options = new PolyHavenClientOptions { UserAgent = "MyBrowserApp/2.5" };
        var (client, stub) = TestClient.Create(options);
        stub.OnPath("/types", CannedJson.Types);

        //Act
        using (client)
        {
            await client.GetAssetTypesAsync();
        }

        //Assert
        stub.Requests.Should().ContainSingle()
            .Which.Headers.UserAgent.ToString().Should().Be("MyBrowserApp/2.5");
    }

    [Fact]
    public async Task custom_base_address_is_used_for_requests()
    {
        //Arrange
        var options = new PolyHavenClientOptions { BaseAddress = "https://example.test/polyhaven" };
        var (client, stub) = TestClient.Create(options);
        stub.OnUrlContains("/types", CannedJson.Types);

        //Act
        using (client)
        {
            await client.GetAssetTypesAsync();
        }

        //Assert
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().Be("https://example.test/polyhaven/types");
    }
}
