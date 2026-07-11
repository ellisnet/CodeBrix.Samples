using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class ThumbnailTests
{
    private static PolyHavenAsset AssetWithThumbnail() => new()
    {
        Id = "abandoned_bakery",
        Name = "Abandoned Bakery",
        ThumbnailUrl = "https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=256&height=256",
    };

    [Fact]
    public void thumbnail_url_is_unchanged_when_no_size_is_requested()
    {
        //Arrange
        var (client, _) = TestClient.Create();
        var asset = AssetWithThumbnail();

        //Act
        string url;
        using (client)
        {
            url = client.GetThumbnailUrl(asset);
        }

        //Assert
        url.Should().Be(asset.ThumbnailUrl);
    }

    [Fact]
    public void thumbnail_url_query_is_replaced_with_the_requested_size()
    {
        //Arrange
        var (client, _) = TestClient.Create();

        //Act
        string url;
        using (client)
        {
            url = client.GetThumbnailUrl(AssetWithThumbnail(), 128, 96);
        }

        //Assert
        url.Should().Be("https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=128&height=96");
    }

    [Fact]
    public void thumbnail_url_supports_width_only()
    {
        //Arrange
        var (client, _) = TestClient.Create();

        //Act
        string url;
        using (client)
        {
            url = client.GetThumbnailUrl(AssetWithThumbnail(), width: 64);
        }

        //Assert
        url.Should().Be("https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=64");
    }

    [Fact]
    public void asset_without_thumbnail_url_is_rejected()
    {
        //Arrange
        var (client, _) = TestClient.Create();
        var asset = new PolyHavenAsset { Id = "no_thumb" };

        //Act
        var act = () => client.GetThumbnailUrl(asset);

        //Assert
        using (client)
        {
            act.Should().Throw<ArgumentException>();
        }
    }

    [Fact]
    public async Task get_thumbnail_downloads_the_image_bytes()
    {
        //Arrange
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 };
        var (client, stub) = TestClient.Create();
        stub.OnUrlContainsBytes("asset_img/thumbs/abandoned_bakery.png", imageBytes);

        //Act
        byte[] bytes;
        using (client)
        {
            bytes = await client.GetThumbnailAsync(AssetWithThumbnail(), 128, 128);
        }

        //Assert
        bytes.Should().Equal(imageBytes);
        stub.RequestUris.Should().ContainSingle()
            .Which.Should().EndWith("abandoned_bakery.png?width=128&height=128");
    }

    [Fact]
    public async Task get_image_downloads_bytes_from_an_absolute_url()
    {
        //Arrange
        var imageBytes = new byte[] { 42, 43, 44 };
        var (client, stub) = TestClient.Create();
        stub.OnUrlContainsBytes("renders/some_render.png", imageBytes);

        //Act
        byte[] bytes;
        using (client)
        {
            bytes = await client.GetImageAsync("https://cdn.polyhaven.com/renders/some_render.png");
        }

        //Assert
        bytes.Should().Equal(imageBytes);
    }
}
