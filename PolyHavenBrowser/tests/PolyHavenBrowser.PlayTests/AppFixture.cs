using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PolyHavenBrowser.PolyHavenApiClient;
using PolyHavenBrowser.ViewModels;
using PolyHavenBrowser.Views;
using SkiaSharp;

namespace PolyHavenBrowser.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private readonly CatalogTransport _transport = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(services =>
        services.AddSingleton<IPolyHavenApiClientFactory>(new DefaultPolyHavenClientFactory(_transport)));
    protected override Task AfterResetAsync() => Application.WaitForAsync(() => Model.IsCatalogLoading, loading => !loading);
    protected override void Cleanup() => _transport.Dispose();
}

// Exercises the real API client and catalog service over deterministic HTTP responses.
// No request is forwarded to the network.
public sealed class CatalogTransport : HttpMessageHandler, IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(this, disposeHandler: false);
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.RequestUri.AbsolutePath.StartsWith("/assets", StringComparison.Ordinal))
        {
            var assets = new Dictionary<string, object>();
            foreach (var (id, name, category, popularity, date) in new[]
            {
                ("chair", "Oak Chair", "furniture", 100, 1600000000),
                ("lantern", "Brass Lantern", "lighting", 70, 1700000000),
                ("vase", "Ceramic Vase", "decoration", 40, 1800000000),
            })
                assets[id] = new { type = 2, name, categories = new[] { category }, tags = new[] { category },
                    authors = new Dictionary<string, string> { ["Fixture Artist"] = "All" },
                    description = "Synthetic catalog entry for offline UI tests.", download_count = popularity,
                    date_published = date, thumbnail_url = "https://fixtures.invalid/thumbnail/" + id + ".png" };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonSerializer.Serialize(assets), Encoding.UTF8, "application/json") });
        }
        if (request.RequestUri.Host == "fixtures.invalid" && request.RequestUri.AbsolutePath.StartsWith("/thumbnail/", StringComparison.Ordinal))
        {
            using var bitmap = new SKBitmap(256, 144);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.DarkSlateBlue);
            using var paint = new SKPaint { Color = SKColors.Goldenrod };
            canvas.DrawRoundRect(70, 28, 116, 88, 12, 12, paint);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data.ToArray()) });
        }
        throw new InvalidOperationException("Unexpected offline fixture request: " + request.RequestUri);
    }
}
