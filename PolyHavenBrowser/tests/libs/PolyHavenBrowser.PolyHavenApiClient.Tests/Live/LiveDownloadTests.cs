using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Live download tests. Kept to a single small (~1.7 MB) 1k HDRI to stay friendly to
/// Poly Haven's CDN.
/// </summary>
[Collection("LiveApi")]
[Trait("Category", "LiveApi")]
public class LiveDownloadTests(LiveApiFixture fixture)
{
    private IPolyHavenApiClient Client => fixture.Client;

    [Fact]
    public async Task small_hdri_downloads_with_verified_md5_and_full_progress()
    {
        //Arrange
        var files = await Client.GetAssetFilesAsync("abandoned_bakery");
        var file = files.FindFile("hdri", "1k", "hdr");
        file.Should().NotBeNull();

        var progress = new ImmediateProgress<PolyHavenDownloadProgress>();

        //Act
        var bytes = await Client.DownloadFileAsync(file!, progress, verifyMd5: true);

        //Assert
        bytes.Length.Should().Be((int)file!.Size);
        progress.Reports.Should().NotBeEmpty();
        progress.Reports.Select(r => r.BytesReceived).Should().BeInAscendingOrder();
        progress.Reports[^1].BytesReceived.Should().Be(file.Size);
        progress.Reports[^1].PercentComplete.Should().Be(100.0);
    }
}
