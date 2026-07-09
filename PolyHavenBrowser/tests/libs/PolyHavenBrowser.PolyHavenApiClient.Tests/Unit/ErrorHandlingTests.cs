using System.Net;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public async Task missing_asset_throws_not_found_with_response_details()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnUrlContains("/info/", "No asset with id nope", HttpStatusCode.NotFound);

        //Act
        Func<Task> act = async () => await client.GetAssetAsync("nope");

        //Assert
        using (client)
        {
            var thrown = await act.Should().ThrowAsync<PolyHavenNotFoundException>();
            thrown.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
            thrown.Which.ResponseBody.Should().Contain("No asset with id");
            thrown.Which.Resource.Should().Contain("nope");
        }
    }

    [Fact]
    public async Task server_error_throws_api_exception_with_status_and_body()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/types", "something broke", HttpStatusCode.InternalServerError);

        //Act
        Func<Task> act = async () => await client.GetAssetTypesAsync();

        //Assert
        using (client)
        {
            var thrown = await act.Should().ThrowAsync<PolyHavenApiException>();
            thrown.Which.Should().NotBeOfType<PolyHavenNotFoundException>();
            thrown.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            thrown.Which.ResponseBody.Should().Be("something broke");
        }
    }

    [Fact]
    public async Task unparseable_response_throws_api_exception()
    {
        //Arrange
        var (client, stub) = TestClient.Create();
        stub.OnPath("/types", "<html>not json</html>");

        //Act
        Func<Task> act = async () => await client.GetAssetTypesAsync();

        //Assert
        using (client)
        {
            await act.Should().ThrowAsync<PolyHavenApiException>();
        }
    }

    [Fact]
    public async Task disposed_client_rejects_further_calls()
    {
        //Arrange
        var (client, _) = TestClient.Create();
        client.Dispose();

        //Act
        Func<Task> act = async () => await client.GetAssetTypesAsync();

        //Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task blank_asset_id_is_rejected()
    {
        //Arrange
        var (client, _) = TestClient.Create();

        //Act
        Func<Task> act = async () => await client.GetAssetAsync("  ");

        //Assert
        using (client)
        {
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
