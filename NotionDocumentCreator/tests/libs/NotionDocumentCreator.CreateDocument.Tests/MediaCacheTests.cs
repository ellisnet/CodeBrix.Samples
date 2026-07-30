using NotionDocumentCreator.CreateDocument.Internal;
using SilverAssertions;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

public class MediaCacheTests
{
    [Fact]
    public async Task failed_download_reports_failure_instead_of_throwing()
    {
        //Arrange - port 9 (discard) refuses connections immediately
        using var cache = new MediaCache();

        //Act
        var result = await cache.FetchAsync(
            "http://127.0.0.1:9/nothing-here", cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        result.Success.Should().Be(false);
        result.FailureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task results_are_cached_per_url()
    {
        //Arrange
        using var cache = new MediaCache();

        //Act
        var first = await cache.FetchAsync(
            "http://127.0.0.1:9/same-url", cancellationToken: TestContext.Current.CancellationToken);
        var second = await cache.FetchAsync(
            "http://127.0.0.1:9/same-url", cancellationToken: TestContext.Current.CancellationToken);

        //Assert - one download attempt, one shared result
        ReferenceEquals(first, second).Should().Be(true);
    }

    [Fact]
    public async Task missing_url_reports_failure()
    {
        //Arrange
        using var cache = new MediaCache();

        //Act
        var result = await cache.FetchAsync("", cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        result.Success.Should().Be(false);
    }

    [Fact]
    public void dispose_removes_the_cache_directory()
    {
        //Arrange
        var cache = new MediaCache();
        Directory.CreateDirectory(cache.CacheDirectory);

        //Act
        cache.Dispose();

        //Assert
        Directory.Exists(cache.CacheDirectory).Should().Be(false);
    }
}
