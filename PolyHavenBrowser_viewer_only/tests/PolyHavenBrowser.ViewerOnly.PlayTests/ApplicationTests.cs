using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PolyHavenBrowser.Display;
using PolyHavenBrowser.Views;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.ViewerOnly.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string TextureStatus = "Texture: " + SampleTransport.TextureName + " · drag to rotate · scroll to zoom";
    private const string HdriStatus = "HDRI: " + SampleTransport.HdriName + " · drag to look around · scroll to zoom";
    private const string ModelStatus = "Model: " + SampleTransport.ModelName + " · drag to rotate · scroll to zoom";
    private Locator Status => Page.GetByTestId("StatusText");
    private Locator EngineSelector => Page.GetByTestId("RenderEngine");
    private Locator Canvas => Page.GetByTestId("DisplayCanvas");
    private Locator Dialog(string title) => Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = title });

    [Fact]
    public async Task Startup_loads_texture_sample_and_reports_status()
    {
        await Expect(Status).ToHaveTextAsync(TextureStatus);
        (await Page.EvaluateAsync(() => Fixture.Model.IsTextureSelected)).Should().BeTrue();
        await Expect(EngineSelector).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Progressbar)).ToBeHiddenAsync();
        Fixture.Transport.Requests.Should().Equal("/info/red_brick", "/files/red_brick", "/download/red_brick/" + FixtureAssets.TextureFile);
        // The texture is wrapped on a cube (12 triangles) and handed to the OpenGL engine, the default.
        var engine = Fixture.Engines.Current;
        engine.Kind.Should().Be(RenderEngineKind.OpenGL);
        engine.Model.TriangleCount.Should().Be(12);
        File.Exists(Path.Combine(Fixture.CacheRoot, "texture", "sample.json")).Should().BeTrue();
        await SnapshotAsync("PolyHavenBrowser_viewer_only-texture");
    }

    [Fact]
    public async Task Hdri_button_shows_panorama_and_hides_engine_selector()
    {
        await Button("Sample HDRI").ClickAsync();
        await Expect(Status).ToHaveTextAsync(HdriStatus);
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentPainter)).Should().BeOfType<PanoramaScenePainter>();
        (await Page.EvaluateAsync(() => Fixture.Model.IsHdriSelected)).Should().BeTrue();
        await Expect(EngineSelector).ToBeHiddenAsync();
        await Expect(Page.GetByText("Rendering engine:", new() { Exact = true })).ToBeHiddenAsync();
        await Button("Sample Texture").ClickAsync();
        await Expect(Status).ToHaveTextAsync(TextureStatus);
        await Expect(EngineSelector).ToBeVisibleAsync();
        await Expect(Page.GetByText("Rendering engine:", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Model_button_loads_gltf_and_shows_engine_selector()
    {
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync(ModelStatus);
        (await Page.EvaluateAsync(() => Fixture.Model.IsModelSelected)).Should().BeTrue();
        (await Page.EvaluateAsync(() => Fixture.Model.IsTextureSelected)).Should().BeFalse();
        await Expect(EngineSelector).ToBeVisibleAsync();
        Fixture.Engines.Current.Model.TriangleCount.Should().Be(FixtureAssets.ModelTriangles);
        // The glTF and its .bin sidecar were both downloaded next to each other.
        var folder = Path.Combine(Fixture.CacheRoot, "model", "Camera_01");
        File.Exists(Path.Combine(folder, FixtureAssets.ModelFile)).Should().BeTrue();
        Directory.GetFiles(folder, "*.bin").Should().ContainSingle();
    }

    [Fact]
    public async Task Sample_buttons_are_disabled_while_an_asset_downloads()
    {
        Fixture.Transport.Hold();
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Downloading model: " + SampleTransport.ModelName + "…");
        await Expect(Page.GetByRole(AriaRole.Progressbar)).ToBeVisibleAsync();
        foreach (var name in new[] { "Sample Texture", "Sample HDRI", "Sample Model" })
            await Expect(Button(name)).ToBeDisabledAsync();
        await Expect(EngineSelector).ToBeDisabledAsync();
        Fixture.Transport.Release();
        await Expect(Status).ToHaveTextAsync(ModelStatus);
        await Expect(Page.GetByRole(AriaRole.Progressbar)).ToBeHiddenAsync();
        foreach (var name in new[] { "Sample Texture", "Sample HDRI", "Sample Model" })
            await Expect(Button(name)).ToBeEnabledAsync();
        await Expect(EngineSelector).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Failed_download_reports_error_in_status_line()
    {
        Fixture.Transport.FailDownloads = true;
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Could not load the model sample: The Poly Haven API request for file '"
            + SampleTransport.Url("Camera_01", FixtureAssets.ModelFile) + "' failed with status 500 (InternalServerError).");
        await Expect(Button("Sample Model")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsBusy)).Should().BeFalse();
        // Nothing half-downloaded is cached: once the network recovers, the model downloads again.
        File.Exists(Path.Combine(Fixture.CacheRoot, "model", "sample.json")).Should().BeFalse();
        Fixture.Transport.FailDownloads = false;
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync(ModelStatus);
    }

    [Fact]
    public async Task Unavailable_engine_shows_alert_and_snaps_selection_back()
    {
        await ChooseEngineAsync("Metal");
        await Expect(Dialog("Metal Rendering")).ToContainTextAsync("Metal rendering is not available on this platform.");
        await Dialog("Metal Rendering").GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.SelectedRenderEngine, kind => kind == RenderEngineKind.OpenGL,
            description: "the reverted engine");
        await Expect(EngineSelector).ToContainTextAsync("OpenGL");
        Fixture.Engines.Created.Select(engine => engine.Kind).Should().Equal(RenderEngineKind.OpenGL);
        Fixture.Engines.Current.IsDisposed.Should().BeFalse();
        await Expect(Status).ToHaveTextAsync(TextureStatus);
    }

    [Fact]
    public async Task Switching_engine_reloads_current_sample_from_cache()
    {
        var requests = Fixture.Transport.Requests.Count;
        var openGl = Fixture.Engines.Current;
        await ChooseEngineAsync("Vulkan");
        await Fixture.Application.WaitForAsync(() => Fixture.Engines.Current.Kind, kind => kind == RenderEngineKind.Vulkan,
            description: "the Vulkan engine");
        await Fixture.Application.WaitForAsync(() => Fixture.Engines.Current.ModelFrames, frames => frames > 0,
            description: "a frame drawn by the Vulkan engine");
        await Expect(Status).ToHaveTextAsync(TextureStatus);
        openGl.IsDisposed.Should().BeTrue();
        Fixture.Engines.Current.Model.TriangleCount.Should().Be(12);
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedRenderEngine)).Should().Be(RenderEngineKind.Vulkan);
        Fixture.Transport.Requests.Count.Should().Be(requests);
    }

    [Fact]
    public async Task Dragging_and_scrolling_canvas_moves_model_camera()
    {
        var engine = Fixture.Engines.Current;
        await Fixture.Application.WaitForAsync(() => engine.ModelFrames, frames => frames > 0, description: "the first model frame");
        var yaw = engine.Camera.YawDegrees;
        var distance = engine.Camera.Distance;
        await Canvas.DragByAsync(200, 0);
        await Fixture.Application.WaitForAsync(() => engine.Camera.YawDegrees, value => value != yaw, description: "the orbited camera");
        // Dragging right turns the camera by a quarter of a degree per pixel.
        (engine.Camera.YawDegrees - yaw).Should().BeApproximately(-50f, 1f);
        var box = await Canvas.BoundingBoxAsync();
        await Page.Mouse.MoveAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
        await Page.Mouse.WheelAsync(0, -120);
        await Fixture.Application.WaitForAsync(() => engine.Camera.Distance, value => value != distance, description: "the zoomed camera");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_sample_buttons_and_status_remain_usable()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync(ModelStatus);
        await Expect(EngineSelector).ToBeVisibleAsync();
        var canvas = await Canvas.BoundingBoxAsync();
        canvas.Height.Should().BeGreaterThan(canvas.Width);
        var status = await Status.BoundingBoxAsync();
        status.Y.Should().BeGreaterThan(canvas.Y + canvas.Height);
        await SnapshotAsync("PolyHavenBrowser_viewer_only-portrait");
    }

    [Fact]
    public async Task Hdri_drag_changes_rendered_pixels()
    {
        await Button("Sample HDRI").ClickAsync();
        await Expect(Status).ToHaveTextAsync(HdriStatus);
        var box = await Canvas.BoundingBoxAsync();
        // The panorama's colour ramp runs left to right, so the two sides of the view differ.
        (await CanvasPixelAsync(box, 0.25f)).Should().NotBe(await CanvasPixelAsync(box, 0.75f));
        var before = await CanvasPixelAsync(box, 0.5f);
        await Canvas.DragByAsync(box.Width / 4, 0);
        var after = before;
        // Each screenshot waits for a rendered frame; the drag's repaints may take a few frames to land.
        for (var frame = 0; frame < 20 && after == before; frame++)
            after = await CanvasPixelAsync(box, 0.5f);
        after.Should().NotBe(before);
    }

    [Fact]
    public async Task Revisiting_a_sample_uses_cache_without_network()
    {
        await Button("Sample HDRI").ClickAsync();
        await Expect(Status).ToHaveTextAsync(HdriStatus);
        var requests = Fixture.Transport.Requests.Count;
        await Button("Sample Texture").ClickAsync();
        await Expect(Status).ToHaveTextAsync(TextureStatus);
        await Button("Sample HDRI").ClickAsync();
        await Expect(Status).ToHaveTextAsync(HdriStatus);
        Fixture.Transport.Requests.Count.Should().Be(requests);
        Fixture.Transport.Requests.Should().ContainSingle(request => request.StartsWith("/download/small_cathedral/"));
    }

    [Fact]
    public async Task Page_reset_cancels_inflight_download()
    {
        var model = Fixture.Model;
        var cache = Fixture.CacheRoot;
        Fixture.Transport.Hold();
        await Button("Sample Model").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Downloading model: " + SampleTransport.ModelName + "…");
        // A fresh page disposes this one's view model, which stops the held download.
        await Fixture.ResetAsync();
        await Fixture.Application.WaitForAsync(() => model.IsBusy, busy => !busy, description: "the cancelled download");
        Fixture.Transport.Cancellations.Should().Be(1);
        File.Exists(Path.Combine(cache, "model", "sample.json")).Should().BeFalse();
        Directory.GetFiles(Path.Combine(cache, "model", "Camera_01")).Should().BeEmpty();
        // Disposal is idempotent: the page's Unloaded handler and the reset have both disposed it already.
        await Page.EvaluateAsync(() => model.Invoking(disposed => disposed.Dispose()).Should().NotThrow());
        await Expect(Status).ToHaveTextAsync(TextureStatus);
    }

    private async Task ChooseEngineAsync(string engine)
    {
        await EngineSelector.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = engine, Exact = true }).ClickAsync();
    }

    // The colour on the canvas's middle row at a fraction of its width, from a screenshot of the whole virtual screen.
    private async Task<SKColor> CanvasPixelAsync(LocatorBoundingBoxResult box, float x)
    {
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        return bitmap.GetPixel((int)(box.X + box.Width * x), (int)(box.Y + box.Height / 2));
    }
}
