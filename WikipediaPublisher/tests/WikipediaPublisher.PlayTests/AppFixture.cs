using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using WikipediaPublisher.RenderArticle.Models;
using WikipediaPublisher.RenderArticle.Services;
using WikipediaPublisher.ViewModels;
using WikipediaPublisher.Views;

namespace WikipediaPublisher.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private readonly WikiServer _server = new();
    public RenderFixture Renderer { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public Uri Origin => _server.Origin;
    protected override void Prepare() => _server.Start();
    protected override Microsoft.UI.Xaml.Application CreateApplication() => new App(services =>
    {
        services.AddSingleton<IArticleRenderService>(Renderer);
        services.AddSingleton(new WikiNavigationOptions { BaseUri = Origin });
    });
    protected override Task BeforeResetAsync()
    {
        Renderer.LastRequest = null;
        Renderer.Fail = false;
        return Task.CompletedTask;
    }
    protected override async Task AfterResetAsync() =>
        await Application.WaitForAsync(() => Model.ArticleUrl, url => url == new Uri(Origin, "wiki/Main_Page").AbsoluteUri);
    protected override void Cleanup() => _server.Dispose();
}

public sealed class RenderFixture : IArticleRenderService
{
    public RenderRequest LastRequest { get; set; }
    public bool Fail { get; set; }
    public Task<IList<ArticleSearchResult>> SearchArticlesAsync(string searchTerms, int maxResults = 20, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The UI searches through its embedded browser.");
    public async Task<RenderedArticle> RenderArticleAsync(RenderRequest request, IProgress<RenderProgress> progress = null, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        progress?.Report(new(RenderStage.ComposingBook, "Rendering the fixture article…", 50));
        await Task.Yield();
        if (Fail) throw new InvalidOperationException("Fixture renderer unavailable");
        // Deliberately a service-boundary fixture, not a test of the PDF rendering library.
        await File.WriteAllTextAsync(request.OutputFilePath, "%PDF-1.4\n% PlayTest service fixture\n%%EOF\n", cancellationToken);
        return new RenderedArticle { Title = "Fixture Article", OutputFilePath = request.OutputFilePath,
            PageCount = 2, ImageCount = 1, Elapsed = TimeSpan.FromSeconds(1) };
    }
}

// The real embedded browser navigates here. No Wikipedia or other Internet access is needed.
// Binding port zero avoids races between test processes choosing an available port.
internal sealed class WikiServer : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private Task _loop;
    public Uri Origin { get; private set; }
    public void Start()
    {
        _listener.Start();
        Origin = new Uri("http://127.0.0.1:" + ((IPEndPoint)_listener.LocalEndpoint).Port + "/");
        _loop = ServeAsync();
    }
    private async Task ServeAsync()
    {
        try
        {
            while (!_stop.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                await using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true);
                var line = await reader.ReadLineAsync(_stop.Token);
                var target = line?.Split(' ')[1] ?? "/";
                while (!string.IsNullOrEmpty(await reader.ReadLineAsync(_stop.Token))) { }
                var search = target.StartsWith("/w/index.php?", StringComparison.Ordinal);
                var article = target.StartsWith("/wiki/Fixture_Article", StringComparison.Ordinal);
                var title = article ? "Fixture Article" : "Offline Wikipedia fixture";
                var html = "<!doctype html><html><head><meta charset='utf-8'><title>" + title
                    + "</title><style>body{font:24px sans-serif;margin:48px;background:#fff;color:#202122}h1{font-size:40px}a{color:#36c}</style></head><body><h1>"
                    + title + "</h1><hr><p>A local article for repeatable browser and publishing tests.</p>"
                    + "<p><a href='/wiki/Fixture_Article'>Read Fixture Article</a></p></body></html>";
                var bytes = Encoding.UTF8.GetBytes(html);
                var header = (search ? "HTTP/1.1 302 Found\r\nLocation: /wiki/Fixture_Article\r\n" : "HTTP/1.1 200 OK\r\n")
                    + "Content-Type: text/html; charset=utf-8\r\nContent-Length: " + bytes.Length + "\r\nConnection: close\r\n\r\n";
                await stream.WriteAsync(Encoding.ASCII.GetBytes(header), _stop.Token);
                await stream.WriteAsync(bytes, _stop.Token);
            }
        }
        catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
        catch (SocketException) when (_stop.IsCancellationRequested) { }
    }
    public void Dispose()
    {
        _stop.Cancel();
        _listener.Stop();
        _loop?.GetAwaiter().GetResult();
        _stop.Dispose();
    }
}
