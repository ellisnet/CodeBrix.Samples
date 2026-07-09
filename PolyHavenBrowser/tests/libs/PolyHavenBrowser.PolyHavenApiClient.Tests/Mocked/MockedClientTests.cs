using System.Net;
using CodeBrix.TestMocks.Mocking;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Tests exercising the full <see cref="IPolyHavenApiClient"/> surface through a
/// CodeBrix.TestMocks mock, demonstrating how consumers of the client can be tested offline.
/// </summary>
public class MockedClientTests
{
    private static PolyHavenAsset SampleAsset(string id = "abandoned_bakery") => new()
    {
        Id = id,
        Name = "Abandoned Bakery",
        Type = PolyHavenAssetType.Hdri,
        Categories = ["urban", "indoor"],
        ThumbnailUrl = "https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=256&height=256",
        DownloadCount = 18040,
    };

    [Fact]
    public async Task mocked_get_asset_types_returns_configured_list()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAssetTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "hdris", "textures", "models" });

        //Act
        var types = await mock.Object.GetAssetTypesAsync();

        //Assert
        types.Should().Equal("hdris", "textures", "models");
        mock.Verify(c => c.GetAssetTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task mocked_get_assets_matches_on_the_requested_type()
    {
        //Arrange
        var hdris = new Dictionary<string, PolyHavenAsset> { ["abandoned_bakery"] = SampleAsset() };
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAssetsAsync(
                It.Is<PolyHavenAssetType?>(t => t == PolyHavenAssetType.Hdri),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hdris);

        //Act
        var assets = await mock.Object.GetAssetsAsync(PolyHavenAssetType.Hdri);
        var noMatch = await mock.Object.GetAssetsAsync(PolyHavenAssetType.Model);

        //Assert
        assets.Should().ContainKey("abandoned_bakery");
        noMatch.Should().BeNull();
        mock.Verify(c => c.GetAssetsAsync(
            It.IsAny<PolyHavenAssetType?>(), It.IsAny<IEnumerable<string>?>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task mocked_get_asset_verifies_the_exact_id_requested()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAssetAsync("abandoned_bakery", It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleAsset());

        //Act
        var asset = await mock.Object.GetAssetAsync("abandoned_bakery");

        //Assert
        asset.Name.Should().Be("Abandoned Bakery");
        mock.Verify(c => c.GetAssetAsync("abandoned_bakery", It.IsAny<CancellationToken>()), Times.Once);
        mock.Verify(c => c.GetAssetAsync("some_other_asset", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task mocked_get_asset_can_throw_not_found()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAssetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PolyHavenNotFoundException("asset 'nope'", HttpStatusCode.NotFound, null));

        //Act
        Func<Task> act = async () => await mock.Object.GetAssetAsync("nope");

        //Assert
        var thrown = await act.Should().ThrowAsync<PolyHavenNotFoundException>();
        thrown.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task mocked_get_asset_files_returns_a_constructed_tree()
    {
        //Arrange
        var file = new PolyHavenFileRef
        {
            Url = "https://dl.polyhaven.org/hdr/1k/abandoned_bakery_1k.hdr",
            Md5 = "9a835b2f0e7a42e2b98fefd19c4a3b9d",
            Size = 1730263,
        };
        var tree = new PolyHavenFileTree(new Dictionary<string, PolyHavenFileNode>
        {
            ["hdri"] = new("hdri", new Dictionary<string, PolyHavenFileNode>
            {
                ["1k"] = new("1k", new Dictionary<string, PolyHavenFileNode>
                {
                    ["hdr"] = new("hdr", file),
                }),
            }),
        });

        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAssetFilesAsync("abandoned_bakery", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tree);

        //Act
        var files = await mock.Object.GetAssetFilesAsync("abandoned_bakery");

        //Assert
        files.FindFile("hdri", "1k", "hdr").Should().BeSameAs(file);
        files.EnumerateFiles().Single().Path.Should().Be("hdri/1k/hdr");
    }

    [Fact]
    public async Task mocked_get_author_returns_configured_author()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetAuthorAsync("Sergej Majboroda", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolyHavenAuthor { Name = "Sergej Majboroda", Link = "https://hdrmarket.com/" });

        //Act
        var author = await mock.Object.GetAuthorAsync("Sergej Majboroda");

        //Assert
        author.Link.Should().Be("https://hdrmarket.com/");
        mock.VerifyAll();
    }

    [Fact]
    public async Task mocked_get_categories_matches_on_type_and_filter()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetCategoriesAsync(
                PolyHavenAssetType.Hdri,
                It.Is<IEnumerable<string>?>(cats => cats != null && cats.Contains("outdoor")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { ["all"] = 689, ["nature"] = 521 });

        //Act
        var categories = await mock.Object.GetCategoriesAsync(PolyHavenAssetType.Hdri, ["outdoor"]);

        //Assert
        categories["all"].Should().Be(689);
    }

    [Fact]
    public void mocked_thumbnail_url_builder_is_configurable()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetThumbnailUrl(It.IsAny<PolyHavenAsset>(), 128, 128))
            .Returns("https://cdn.example.test/thumb.png?width=128&height=128");

        //Act
        var url = mock.Object.GetThumbnailUrl(SampleAsset(), 128, 128);

        //Assert
        url.Should().Contain("width=128");
        mock.Verify(c => c.GetThumbnailUrl(It.IsAny<PolyHavenAsset>(), 128, 128), Times.Once);
    }

    [Fact]
    public async Task mocked_thumbnail_and_image_bytes_are_returned()
    {
        //Arrange
        var thumbBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF };
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.GetThumbnailAsync(
                It.IsAny<PolyHavenAsset>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(thumbBytes);
        mock.Setup(c => c.GetImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageBytes);

        //Act
        var thumb = await mock.Object.GetThumbnailAsync(SampleAsset(), 64, 64);
        var image = await mock.Object.GetImageAsync("https://cdn.example.test/render.jpg");

        //Assert
        thumb.Should().Equal(thumbBytes);
        image.Should().Equal(imageBytes);
    }

    [Fact]
    public async Task mocked_stream_download_can_write_bytes_through_a_callback()
    {
        //Arrange
        var payload = new byte[] { 1, 2, 3, 4, 5 };
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.DownloadFileAsync(
                It.IsAny<PolyHavenFileRef>(),
                It.IsAny<Stream>(),
                It.IsAny<IProgress<PolyHavenDownloadProgress>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Callback<PolyHavenFileRef, Stream, IProgress<PolyHavenDownloadProgress>?, bool, CancellationToken>(
                (file, destination, progress, _, _) =>
                {
                    destination.Write(payload);
                    progress?.Report(new PolyHavenDownloadProgress(payload.Length, payload.Length));
                })
            .Returns(Task.CompletedTask);

        var fileRef = new PolyHavenFileRef { Url = "https://dl.example.test/f.bin", Size = payload.Length };
        var progress = new ImmediateProgress<PolyHavenDownloadProgress>();
        using var destination = new MemoryStream();

        //Act
        await mock.Object.DownloadFileAsync(fileRef, destination, progress, verifyMd5: true);

        //Assert
        destination.ToArray().Should().Equal(payload);
        progress.Reports.Should().ContainSingle()
            .Which.PercentComplete.Should().Be(100.0);
    }

    [Fact]
    public async Task mocked_path_download_verifies_the_destination_path()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.DownloadFileAsync(
                It.IsAny<PolyHavenFileRef>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<PolyHavenDownloadProgress>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var fileRef = new PolyHavenFileRef { Url = "https://dl.example.test/f.hdr", Size = 10 };

        //Act
        await mock.Object.DownloadFileAsync(fileRef, "/tmp/downloads/f.hdr");

        //Assert
        mock.Verify(c => c.DownloadFileAsync(
            fileRef,
            "/tmp/downloads/f.hdr",
            It.IsAny<IProgress<PolyHavenDownloadProgress>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task mocked_byte_download_returns_configured_bytes()
    {
        //Arrange
        var payload = new byte[] { 9, 8, 7 };
        var mock = new Mock<IPolyHavenApiClient>();
        mock.Setup(c => c.DownloadFileAsync(
                It.IsAny<PolyHavenFileRef>(),
                It.IsAny<IProgress<PolyHavenDownloadProgress>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        //Act
        var bytes = await mock.Object.DownloadFileAsync(
            new PolyHavenFileRef { Url = "https://dl.example.test/f.bin", Size = 3 });

        //Assert
        bytes.Should().Equal(payload);
    }

    [Fact]
    public async Task mocked_setup_sequence_returns_evolving_results()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();
        mock.SetupSequence(c => c.GetAssetTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "hdris" })
            .ReturnsAsync(new[] { "hdris", "textures", "models" });

        //Act
        var first = await mock.Object.GetAssetTypesAsync();
        var second = await mock.Object.GetAssetTypesAsync();

        //Assert
        first.Should().HaveCount(1);
        second.Should().HaveCount(3);
    }

    [Fact]
    public async Task strict_mock_rejects_unconfigured_calls()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>(MockBehavior.Strict);

        //Act
        Func<Task> act = async () => await mock.Object.GetAssetTypesAsync();

        //Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void mocked_dispose_is_verifiable()
    {
        //Arrange
        var mock = new Mock<IPolyHavenApiClient>();

        //Act
        using (mock.Object)
        {
        }

        //Assert
        mock.Verify(c => c.Dispose(), Times.Once);
        mock.VerifyNoOtherCalls();
    }
}
