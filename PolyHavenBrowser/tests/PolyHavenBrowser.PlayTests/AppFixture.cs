using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
using CodeBrix.Graphics3D.Gltf2.Materials;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PolyHavenBrowser.PolyHavenApiClient;
using PolyHavenBrowser.Services;
using PolyHavenBrowser.ViewModels;
using PolyHavenBrowser.Views;
using SkiaSharp;

namespace PolyHavenBrowser.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private Action<CatalogTransport> _configure;
    public CatalogTransport Transport { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    // Each page gets its own catalog service, so a test can reload the catalog (larger, or failing)
    // without the app's once-per-run catalog cache carrying one test's catalog into the next.
    protected override Application CreateApplication() => new App(services => services
        .AddSingleton<IPolyHavenApiClientFactory>(new DefaultPolyHavenClientFactory(Transport))
        .AddTransient<ModelCatalogService>());
    protected override void Prepare() => Transport.LoadModel(Path.Combine(DataDirectory, "fixture-model"));

    // The application initializes OpenGL elements. These tests cover the no-OpenGL fallback, so the launch opts out;
    // to give the elements real contexts instead, call CodeBrixPlayTestOpenGL.Register() in Prepare() and drop this override.
    protected override void Configure(PlayTestOptions options) => options.OpenGL = PlayTestOpenGL.Unavailable;
    protected override Task BeforeResetAsync()
    {
        Transport.Reset();
        _configure?.Invoke(Transport);
        _configure = null;
        return Task.CompletedTask;
    }
    protected override Task AfterResetAsync() =>
        Application.WaitForAsync(() => Model.IsCatalogLoading && !Transport.FailCatalog, loading => !loading);
    protected override void Cleanup() => Transport.Dispose();

    // Installs a fresh page whose services see the transport configured differently from the default.
    public Task ReloadAsync(Action<CatalogTransport> configure)
    {
        _configure = configure;
        return ResetAsync();
    }
}

// Exercises the real API client, catalog and download services over deterministic HTTP responses.
// No request is forwarded to the network.
public sealed class CatalogTransport : HttpMessageHandler, IHttpClientFactory
{
    private static readonly (string Id, string Name, string Category, int Popularity, long Date)[] Models =
    {
        ("chair", "Oak Chair", "furniture", 100, 1600000000),
        ("lantern", "Brass Lantern", "lighting", 70, 1700000000),
        ("vase", "Ceramic Vase", "decoration", 40, 1800000000),
    };
    private readonly ConcurrentQueue<string> _requests = new();
    private Dictionary<string, byte[]> _modelFiles;
    private TaskCompletionSource _gate = Released();
    public int CatalogSize { get; set; } = 3;
    public bool FailCatalog { get; set; }
    public bool FailFileDownloads { get; set; }
    public string FailThumbnailId { get; set; }
    public IReadOnlyCollection<string> Requests => _requests.ToArray();
    public long ModelBytes => _modelFiles.Values.Sum(bytes => (long)bytes.Length);
    public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

    public void LoadModel(string folder)
    {
        FixtureModel.Save(folder);
        _modelFiles = Directory.GetFiles(folder).ToDictionary(Path.GetFileName, File.ReadAllBytes);
    }

    public void Reset()
    {
        Release();
        CatalogSize = 3;
        FailCatalog = false;
        FailFileDownloads = false;
        FailThumbnailId = null;
        _requests.Clear();
    }

    // Holds model sidecar downloads and backdrop-texture lookups until Release().
    public void Hold() => _gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public void Release() => _gate.TrySetResult();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = request.RequestUri.AbsolutePath;
        _requests.Enqueue(path);
        if (path.StartsWith("/assets", StringComparison.Ordinal))
        {
            if (FailCatalog) return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            var assets = new Dictionary<string, object>();
            for (var i = 0; i < CatalogSize; i++)
            {
                var (id, name, category, popularity, date) = i < Models.Length ? Models[i]
                    : ($"model-{i + 1:00}", $"Fixture Model {i + 1:00}", "props", 10, 1500000000L + i);
                assets[id] = new { type = 2, name, categories = new[] { category }, tags = new[] { category },
                    authors = new Dictionary<string, string> { ["Fixture Artist"] = "All" },
                    description = "Synthetic catalog entry for offline UI tests.", download_count = popularity,
                    date_published = date, max_resolution = new[] { 2048, 1024 },
                    thumbnail_url = "https://fixtures.invalid/thumbnail/" + id + ".png" };
            }
            return Json(assets);
        }
        if (path.StartsWith("/files/", StringComparison.Ordinal))
        {
            var id = path["/files/".Length..];
            if (Models.All(model => model.Id != id))
            {
                // The document backdrop textures: unavailable, so the one-sheet uses plain floors.
                await _gate.Task.WaitAsync(cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            if (id == "vase") return Json(new { blend = new Dictionary<string, object> { ["2k"] = new { blend = FileRef(id, "vase.blend", 1024) } } });
            var gltf = _modelFiles.Keys.Single(file => file.EndsWith(".gltf", StringComparison.Ordinal));
            var include = _modelFiles.Keys.Where(file => file != gltf).ToDictionary(file => file, file => FileRef(id, file));
            return Json(new { gltf = new Dictionary<string, object> { ["2k"] = new { gltf = FileRef(id, gltf, include: include) } } });
        }
        if (request.RequestUri.Host == "fixtures.invalid" && path.StartsWith("/download/", StringComparison.Ordinal))
        {
            var file = Path.GetFileName(path);
            if (!file.EndsWith(".gltf", StringComparison.Ordinal)) await _gate.Task.WaitAsync(cancellationToken);
            if (FailFileDownloads && !file.EndsWith(".gltf", StringComparison.Ordinal))
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_modelFiles[file]) };
        }
        if (request.RequestUri.Host == "fixtures.invalid" && path.StartsWith("/thumbnail/", StringComparison.Ordinal))
        {
            if (path == "/thumbnail/" + FailThumbnailId + ".png") return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            using var bitmap = new SKBitmap(256, 144);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.DarkSlateBlue);
            using var paint = new SKPaint { Color = SKColors.Goldenrod };
            canvas.DrawRoundRect(70, 28, 116, 88, 12, 12, paint);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data.ToArray()) };
        }
        throw new InvalidOperationException("Unexpected offline fixture request: " + request.RequestUri);
    }

    private object FileRef(string id, string file, long? size = null, Dictionary<string, object> include = null) => new
    {
        url = $"https://fixtures.invalid/download/{id}/{file}",
        size = size ?? _modelFiles[file].Length,
        include,
    };

    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"),
    };

    private static TaskCompletionSource Released()
    {
        var gate = new TaskCompletionSource();
        gate.SetResult();
        return gate;
    }
}

// A small generated glTF (two triangles, one material) with its buffer in a separate .bin sidecar.
public static class FixtureModel
{
    public const int Triangles = 2;
    public const int Vertices = 4;

    public static void Save(string folder)
    {
        Directory.CreateDirectory(folder);
        var material = new MaterialBuilder("glaze").WithDoubleSide(true).WithBaseColor(new Vector4(0.8f, 0.5f, 0.2f, 1f));
        var mesh = new MeshBuilder<VertexPosition>("panel");
        mesh.UsePrimitive(material).AddQuadrangle(new VertexPosition(0f, 0f, 0f), new VertexPosition(1f, 0f, 0f),
            new VertexPosition(1f, 1f, 0f), new VertexPosition(0f, 1f, 0f));
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
        scene.ToGltf2().SaveGLTF(Path.Combine(folder, "model.gltf"));
    }
}
