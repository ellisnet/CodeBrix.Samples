using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SilverAssertions;
using Xunit;

namespace WikipediaPublisher.PlayTests;

public sealed class WikiServerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Idle_or_partial_request_does_not_block_browser_navigation(bool partialRequest)
    {
        using var idle = new TcpClient();
        var server = new WikiServer();
        server.Start();
        try
        {
            await idle.ConnectAsync(IPAddress.Loopback, server.Origin.Port, TestContext.Current.CancellationToken);
            if (partialRequest)
                await idle.GetStream().WriteAsync(Encoding.ASCII.GetBytes("GET /wiki/Main_Page HTTP/1.1\r\n"),
                    TestContext.Current.CancellationToken);

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var response = await http.GetAsync(new Uri(server.Origin, "wiki/Fixture_Article"),
                TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Contain("Fixture Article");
        }
        finally
        {
            // Keep the unfinished connection open through shutdown. Cleanup must
            // cancel pending reads and finish even when the test is cancelled.
            await Task.Run(server.Dispose, CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }
        (await idle.GetStream().ReadAsync(new byte[1].AsMemory(), TestContext.Current.CancellationToken).AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)).Should().Be(0);
    }
}
