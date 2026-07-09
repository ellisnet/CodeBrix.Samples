using System.Security.Cryptography;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class DownloadTests
{
    private static readonly byte[] Payload = CreatePayload(256 * 1024);

    private static byte[] CreatePayload(int size)
    {
        var bytes = new byte[size];
        for (var i = 0; i < size; i++)
        {
            bytes[i] = (byte)(i % 251);
        }

        return bytes;
    }

    private static string Md5Of(byte[] bytes) => Convert.ToHexStringLower(MD5.HashData(bytes));

    private static PolyHavenFileRef PayloadFileRef(string? md5 = null) => new()
    {
        Url = "https://dl.example.test/assets/payload.bin",
        Md5 = md5 ?? Md5Of(Payload),
        Size = Payload.Length,
    };

    private static (IPolyHavenApiClient Client, StubHttpMessageHandler Stub) CreatePayloadClient()
    {
        var (client, stub) = TestClient.Create();
        stub.OnUrlContainsBytes("payload.bin", Payload);
        return (client, stub);
    }

    [Fact]
    public async Task download_to_stream_writes_all_bytes()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        using var destination = new MemoryStream();

        //Act
        using (client)
        {
            await client.DownloadFileAsync(PayloadFileRef(), destination);
        }

        //Assert
        destination.ToArray().Should().Equal(Payload);
    }

    [Fact]
    public async Task download_returns_byte_array()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();

        //Act
        byte[] bytes;
        using (client)
        {
            bytes = await client.DownloadFileAsync(PayloadFileRef());
        }

        //Assert
        bytes.Should().Equal(Payload);
    }

    [Fact]
    public async Task download_reports_monotonic_progress_up_to_the_total()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        var progress = new ImmediateProgress<PolyHavenDownloadProgress>();
        using var destination = new MemoryStream();

        //Act
        using (client)
        {
            await client.DownloadFileAsync(PayloadFileRef(), destination, progress);
        }

        //Assert
        progress.Reports.Should().NotBeEmpty();
        progress.Reports.Select(r => r.BytesReceived).Should().BeInAscendingOrder();
        progress.Reports[^1].BytesReceived.Should().Be(Payload.Length);
        progress.Reports[^1].TotalBytes.Should().Be(Payload.Length);
        progress.Reports[^1].PercentComplete.Should().Be(100.0);
    }

    [Fact]
    public async Task download_with_matching_md5_verifies_successfully()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        using var destination = new MemoryStream();

        //Act
        Func<Task> act = async () =>
        {
            using (client)
            {
                await client.DownloadFileAsync(PayloadFileRef(), destination, verifyMd5: true);
            }
        };

        //Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task download_with_wrong_md5_throws_integrity_exception()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        var badMd5 = new string('0', 32);
        using var destination = new MemoryStream();

        //Act
        Func<Task> act = async () =>
            await client.DownloadFileAsync(PayloadFileRef(badMd5), destination, verifyMd5: true);

        //Assert
        using (client)
        {
            var thrown = await act.Should().ThrowAsync<PolyHavenIntegrityException>();
            thrown.Which.ExpectedMd5.Should().Be(badMd5);
            thrown.Which.ActualMd5.Should().Be(Md5Of(Payload));
        }
    }

    [Fact]
    public async Task download_to_file_creates_directories_and_writes_content()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        var tempDir = Directory.CreateTempSubdirectory("phapiclient-test-").FullName;
        var path = Path.Combine(tempDir, "nested", "folder", "payload.bin");

        try
        {
            //Act
            using (client)
            {
                await client.DownloadFileAsync(PayloadFileRef(), path, verifyMd5: true);
            }

            //Assert
            File.Exists(path).Should().BeTrue();
            (await File.ReadAllBytesAsync(path)).Should().Equal(Payload);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task failed_file_download_deletes_the_partial_file()
    {
        //Arrange
        var (client, _) = CreatePayloadClient();
        var tempDir = Directory.CreateTempSubdirectory("phapiclient-test-").FullName;
        var path = Path.Combine(tempDir, "payload.bin");

        try
        {
            //Act
            Func<Task> act = async () =>
                await client.DownloadFileAsync(PayloadFileRef(new string('0', 32)), path, verifyMd5: true);

            //Assert
            using (client)
            {
                await act.Should().ThrowAsync<PolyHavenIntegrityException>();
            }

            File.Exists(path).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task download_of_missing_file_throws_not_found()
    {
        //Arrange
        var (client, _) = TestClient.Create();
        var file = new PolyHavenFileRef { Url = "https://dl.example.test/unknown.bin", Size = 1 };
        using var destination = new MemoryStream();

        //Act
        Func<Task> act = async () => await client.DownloadFileAsync(file, destination);

        //Assert
        using (client)
        {
            await act.Should().ThrowAsync<PolyHavenNotFoundException>();
        }
    }
}
