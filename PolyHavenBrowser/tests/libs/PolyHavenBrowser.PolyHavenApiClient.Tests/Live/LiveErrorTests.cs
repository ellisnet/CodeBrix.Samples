using System.Net;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>Live tests for the API's error behavior.</summary>
[Collection("LiveApi")]
[Trait("Category", "LiveApi")]
public class LiveErrorTests(LiveApiFixture fixture)
{
    private IPolyHavenApiClient Client => fixture.Client;

    [Fact]
    public async Task unknown_asset_id_throws_not_found()
    {
        //Act
        Func<Task> act = async () => await Client.GetAssetAsync("zz_definitely_not_a_real_asset");

        //Assert
        var thrown = await act.Should().ThrowAsync<PolyHavenNotFoundException>();
        thrown.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task unknown_asset_id_throws_not_found_for_files_too()
    {
        //Act
        Func<Task> act = async () => await Client.GetAssetFilesAsync("zz_definitely_not_a_real_asset");

        //Assert
        await act.Should().ThrowAsync<PolyHavenNotFoundException>();
    }
}
