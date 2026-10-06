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
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PolyHavenBrowser.Display;
using PolyHavenBrowser.PolyHavenApiClient;
using PolyHavenBrowser.Rendering;
using PolyHavenBrowser.Services;
using PolyHavenBrowser.ViewModels;
using PolyHavenBrowser.Views;
using SkiaSharp;

namespace PolyHavenBrowser.ViewerOnly.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private string _cacheRoot;
    public SampleTransport Transport { get; } = new();
    public FakeRenderEngineSelector Engines { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    // The cache folder of the current page's sample-asset service; every page starts with an empty cache.
    public string CacheRoot => _cacheRoot;
    protected override Application CreateApplication() => new App(services => services
        .AddSingleton<IPolyHavenApiClientFactory>(new DefaultPolyHavenClientFactory(Transport))
        .AddTransient(provider => new SampleAssetService(provider.GetRequiredService<IPolyHavenApiClientFactory>(), _cacheRoot))
        .AddSingleton<IModelRenderEngineSelector>(Engines));
    protected override void Prepare()
    {
        Transport.LoadAssets(Path.Combine(DataDirectory, "assets"));
        _cacheRoot = NewCacheRoot();
    }
    protected override Task BeforeResetAsync()
    {
        Transport.Reset();
        Engines.Reset();
        _cacheRoot = NewCacheRoot();
        return Task.CompletedTask;
    }
    // A download held by the previous test is released only now, after its page was disposed (which cancels it).
    protected override async Task AfterResetAsync()
    {
        Transport.Release();
        await Application.WaitForAsync(() => Model.Initialization.IsCompleted, done => done, description: "first sample");
    }

    private string NewCacheRoot() => Path.Combine(DataDirectory, "cache-" + Guid.NewGuid().ToString("N"));
}

// Exercises the real API client and sample-asset service over deterministic HTTP responses.
// No request is forwarded to the network.
public sealed class SampleTransport : HttpMessageHandler, IHttpClientFactory
{
    public const string TextureName = "Red Brick";
    public const string HdriName = "Small Cathedral";
    public const string ModelName = "Camera 01";
    private static readonly (string Slug, string Name, int Type)[] Assets =
    {
        ("red_brick", TextureName, 1),
        ("small_cathedral", HdriName, 0),
        ("Camera_01", ModelName, 2),
    };
    private readonly ConcurrentQueue<string> _requests = new();
    private Dictionary<string, byte[]> _files;
    private TaskCompletionSource _gate = Released();
    private int _cancellations;
    public bool FailDownloads { get; set; }
    public IReadOnlyCollection<string> Requests => _requests.ToArray();
    public int Cancellations => Volatile.Read(ref _cancellations);
    public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

    public void LoadAssets(string folder)
    {
        FixtureAssets.Save(folder);
        _files = Directory.GetFiles(folder).ToDictionary(Path.GetFileName, File.ReadAllBytes);
    }

    public void Reset()
    {
        FailDownloads = false;
        _requests.Clear();
        Interlocked.Exchange(ref _cancellations, 0);
    }

    // Holds file downloads until Release().
    public void Hold() => _gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public void Release() => _gate.TrySetResult();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = request.RequestUri.AbsolutePath;
        _requests.Enqueue(path);
        if (path.StartsWith("/info/", StringComparison.Ordinal))
        {
            var (_, name, type) = Assets.Single(asset => asset.Slug == path["/info/".Length..]);
            return Json(new { name, type, authors = new Dictionary<string, string> { ["Fixture Artist"] = "All" } });
        }
        if (path.StartsWith("/files/", StringComparison.Ordinal))
        {
            return path["/files/".Length..] switch
            {
                "red_brick" => Json(new Dictionary<string, object> { ["Diffuse"] = Tree("1k", "png", FileRef("red_brick", FixtureAssets.TextureFile)) }),
                "small_cathedral" => Json(new Dictionary<string, object> { ["hdri"] = Tree("1k", "hdr", FileRef("small_cathedral", FixtureAssets.HdriFile)) }),
                "Camera_01" => Json(new Dictionary<string, object> { ["gltf"] = Tree("1k", "gltf", ModelRef()) }),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            };
        }
        if (request.RequestUri.Host == "fixtures.invalid" && path.StartsWith("/download/", StringComparison.Ordinal))
        {
            try
            {
                await _gate.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref _cancellations);
                throw;
            }
            if (FailDownloads) return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_files[Path.GetFileName(path)]) };
        }
        throw new InvalidOperationException("Unexpected offline fixture request: " + request.RequestUri);
    }

    private static Dictionary<string, object> Tree(string resolution, string format, object file) =>
        new() { [resolution] = new Dictionary<string, object> { [format] = file } };

    private object ModelRef()
    {
        var include = _files.Keys.Where(file => file.EndsWith(".bin", StringComparison.Ordinal))
            .ToDictionary(file => file, file => FileRef("Camera_01", file));
        return new { url = Url("Camera_01", FixtureAssets.ModelFile), size = _files[FixtureAssets.ModelFile].Length, include };
    }

    private object FileRef(string slug, string file) => new { url = Url(slug, file), size = _files[file].Length };

    public static string Url(string slug, string file) => $"https://fixtures.invalid/download/{slug}/{file}";

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

// The three sample files, generated at test time: a two-colour checker PNG, a flat Radiance HDR
// panorama with a horizontal colour ramp, and a small glTF (two triangles) with a .bin sidecar.
public static class FixtureAssets
{
    public const string TextureFile = "red_brick_diff_1k.png";
    public const string HdriFile = "small_cathedral_1k.hdr";
    public const string ModelFile = "model.gltf";
    public const int ModelTriangles = 2;
    private const int PanoramaWidth = 64;
    private const int PanoramaHeight = 32;

    public static void Save(string folder)
    {
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder, TextureFile), CheckerPng());
        File.WriteAllBytes(Path.Combine(folder, HdriFile), RampHdr());
        var material = new MaterialBuilder("body").WithDoubleSide(true).WithBaseColor(new Vector4(0.8f, 0.5f, 0.2f, 1f));
        var mesh = new MeshBuilder<VertexPosition>("panel");
        mesh.UsePrimitive(material).AddQuadrangle(new VertexPosition(0f, 0f, 0f), new VertexPosition(1f, 0f, 0f),
            new VertexPosition(1f, 1f, 0f), new VertexPosition(0f, 1f, 0f));
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
        scene.ToGltf2().SaveGLTF(Path.Combine(folder, ModelFile));
    }

    private static byte[] CheckerPng()
    {
        using var bitmap = new SKBitmap(64, 64);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Firebrick);
            using var paint = new SKPaint { Color = SKColors.Wheat };
            for (var y = 0; y < 4; y++)
                for (var x = y % 2; x < 4; x += 2)
                    canvas.DrawRect(x * 16, y * 16, 16, 16, paint);
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    // Flat (uncompressed) RGBE scanlines: red rises and green falls from left to right, so turning
    // the view changes the colours on screen.
    private static byte[] RampHdr()
    {
        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes($"#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y {PanoramaHeight} +X {PanoramaWidth}\n"));
        for (var y = 0; y < PanoramaHeight; y++)
            for (var x = 0; x < PanoramaWidth; x++)
                stream.Write(new[] { (byte)(x * 4), (byte)(252 - x * 4), (byte)(32 + y * 4), (byte)128 });
        return stream.ToArray();
    }
}

// Stands in for a platform where OpenGL and Vulkan work and Metal does not. The PlayTest head has
// no GPU surface, so every engine is a CPU fake that records what the view model asks of it.
public sealed class FakeRenderEngineSelector : IModelRenderEngineSelector
{
    private readonly ConcurrentQueue<FakeRenderEngine> _created = new();
    public IReadOnlyList<RenderEngineKind> AvailableKinds { get; } = [RenderEngineKind.OpenGL, RenderEngineKind.Vulkan, RenderEngineKind.Metal];
    public IReadOnlyList<FakeRenderEngine> Created => _created.ToArray();
    // The engine the current page draws with: the newest one it has not disposed.
    public FakeRenderEngine Current => Created.Last(engine => !engine.IsDisposed);
    public bool IsSupported(RenderEngineKind kind) => kind != RenderEngineKind.Metal;

    public IModelRenderEngine Create(RenderEngineKind kind, Func<XamlRoot> getXamlRoot)
    {
        if (!IsSupported(kind)) throw new NotSupportedException($"The {kind} rendering engine is not supported on this platform.");
        var engine = new FakeRenderEngine(kind);
        _created.Enqueue(engine);
        return engine;
    }

    public void Reset() => _created.Clear();
}

// Returns flat frames: the requested background, with a solid block in the middle while a model is set.
public sealed class FakeRenderEngine(RenderEngineKind kind) : IModelRenderEngine
{
    public static readonly SKColor ModelColor = new(0x20, 0xC0, 0x60);
    private bool _fitPending;
    private int _modelFrames;
    public RenderEngineKind Kind { get; } = kind;
    public OrbitCamera Camera { get; } = new();
    public Vector3? FixedLightDirection { get; set; }
    public LoadedModel Model { get; private set; }
    public int ModelFrames => Volatile.Read(ref _modelFrames);
    public (int Width, int Height) LastFrameSize { get; private set; }
    public bool IsDisposed { get; private set; }

    public void SetModel(LoadedModel model)
    {
        Model = model;
        _fitPending = model != null;
    }

    public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        // Like the real engines, frame the model on the first render after it is set.
        var model = Model;
        if (_fitPending && model != null)
        {
            Camera.FitToModel(model);
            _fitPending = false;
        }
        var plainRow = Row(width, background, model: false);
        var modelRow = Row(width, background, model != null);
        var rgba = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            var row = y > height / 3 && y < height * 2 / 3 ? modelRow : plainRow;
            Buffer.BlockCopy(row, 0, rgba, y * row.Length, row.Length);
        }
        LastFrameSize = (width, height);
        if (model != null) Interlocked.Increment(ref _modelFrames);
        return new RenderedFrame(rgba, width, height, isBottomUp: false);
    }

    public void Dispose() => IsDisposed = true;

    private static byte[] Row(int width, (float R, float G, float B, float A) background, bool model)
    {
        var row = new byte[width * 4];
        for (var x = 0; x < width; x++)
        {
            var inBlock = model && x > width / 3 && x < width * 2 / 3;
            row[x * 4] = inBlock ? ModelColor.Red : ToByte(background.R);
            row[x * 4 + 1] = inBlock ? ModelColor.Green : ToByte(background.G);
            row[x * 4 + 2] = inBlock ? ModelColor.Blue : ToByte(background.B);
            row[x * 4 + 3] = inBlock ? (byte)255 : ToByte(background.A);
        }
        return row;
    }

    private static byte ToByte(float value) => (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);
}
