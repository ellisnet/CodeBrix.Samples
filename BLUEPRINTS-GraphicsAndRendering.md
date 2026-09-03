# CodeBrix.Samples Blueprints: Graphics and rendering

These recipes cover everything a sample draws for itself, from a
hardware-accelerated 3D scene hosted in an ordinary page to two-dimensional
painting on a canvas surface. They keep renderers, cameras and decoders in
a headless library behind small interfaces, so the view model owns a painter
and never a graphics API, and they show how to select or gate a GPU backend
per head, render off screen, and composite the resulting pixels back with
the right orientation and blending. A second group is about images rather
than scenes: rasterizing vector art, decoding raster and high-dynamic-range
files, honoring stored photo orientation, and turning raw pixel buffers into
something a XAML element can show. Reach for this file when the view has to
produce pixels rather than arrange controls, whether that is a 3D preview,
a zoomable document canvas, a freehand drawing session, a procedural shader
or an animated overlay.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Host an OpenGL scene in XAML with a GLCanvasElement subclass](#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass)
- [Keep the GL renderer framework-free behind an interface](#keep-the-gl-renderer-framework-free-behind-an-interface)
- [Pick the shader version header for desktop GL or GLES at runtime](#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime)
- [Share one camera and one matrix convention across graphics APIs](#share-one-camera-and-one-matrix-convention-across-graphics-apis)
- [Frame the camera automatically on each newly bound model](#frame-the-camera-automatically-on-each-newly-bound-model)
- [Draw translucent surfaces in a second pass with depth writes off](#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off)
- [Render off screen product shots on the head own GL context](#render-off-screen-product-shots-on-the-head-own-gl-context)
- [Generate scene set dressing as ordinary geometry](#generate-scene-set-dressing-as-ordinary-geometry)
- [Swap the 3D graphics backend at run time from a dropdown](#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown)
- [Gate an optional graphics backend to specific heads with an allow list](#gate-an-optional-graphics-backend-to-specific-heads-with-an-allow-list)
- [Render an OpenGL scene off screen and composite it onto an SKXamlCanvas](#render-an-opengl-scene-off-screen-and-composite-it-onto-an-skxamlcanvas)
- [Add a self contained Vulkan renderer that needs no shader toolchain](#add-a-self-contained-vulkan-renderer-that-needs-no-shader-toolchain)
- [Add a direct to Metal renderer with no NuGet package or Apple bindings](#add-a-direct-to-metal-renderer-with-no-nuget-package-or-apple-bindings)
- [Composite engine pixels onto Skia with the right vertical orientation](#composite-engine-pixels-onto-skia-with-the-right-vertical-orientation)
- [Paint a CPU ray traced panorama into an SKBitmap](#paint-a-cpu-ray-traced-panorama-into-an-skbitmap)
- [Decode HDR images and tone map them for display](#decode-hdr-images-and-tone-map-them-for-display)
- [Build a textured cube mesh from a bitmap for previewing a flat material](#build-a-textured-cube-mesh-from-a-bitmap-for-previewing-a-flat-material)
- [Paint a zoomable image on an SKXamlCanvas from the view model](#paint-a-zoomable-image-on-an-skxamlcanvas-from-the-view-model)
- [Spotlight one region of an image on the canvas](#spotlight-one-region-of-an-image-on-the-canvas)
- [Play a baked animation clip in a preview canvas](#play-a-baked-animation-clip-in-a-preview-canvas)
- [Rasterize SVG art with the CodeBrix SkiaSvg library](#rasterize-svg-art-with-the-codebrix-skiasvg-library)
- [Decode raster images with the CodeBrix Imaging library into a Skia bitmap](#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap)
- [Normalize a downloaded image before embedding it in a document](#normalize-a-downloaded-image-before-embedding-it-in-a-document)
- [Create a drawing session with named color layers](#create-a-drawing-session-with-named-color-layers)
- [Export a drawing at a chosen pixel size](#export-a-drawing-at-a-chosen-pixel-size)
- [Drive strokes in normalized image coordinates from a sensor](#drive-strokes-in-normalized-image-coordinates-from-a-sensor)
- [Keep a mirrored preview and a mirrored drawing consistent](#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent)
- [Draw a brush sized cursor over a rendered drawing session](#draw-a-brush-sized-cursor-over-a-rendered-drawing-session)
- [Draw an animated SkSL shader as a game engine direct drawing](#draw-an-animated-sksl-shader-as-a-game-engine-direct-drawing)
- [Smooth worker rate data into frame rate animation](#smooth-worker-rate-data-into-frame-rate-animation)
- [Keep a pipeline and a renderer decoupled by a normalized seam](#keep-a-pipeline-and-a-renderer-decoupled-by-a-normalized-seam)
- [Offer a CPU fallback for a GPU rendering path behind one switch](#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch)
- [Choose the render resolution from the zoom level](#choose-the-render-resolution-from-the-zoom-level)
- [Draw a zoomable document canvas on an SKXamlCanvas subclass](#draw-a-zoomable-document-canvas-on-an-skxamlcanvas-subclass)
- [Repaint only the dirty rectangle of a cached composite](#repaint-only-the-dirty-rectangle-of-a-cached-composite)
- [Animate an overlay with a timer that stops when unloaded](#animate-an-overlay-with-a-timer-that-stops-when-unloaded)
- [Host a canvas in a scroll viewer and drive zoom and scroll from an interface](#host-a-canvas-in-a-scroll-viewer-and-drive-zoom-and-scroll-from-an-interface)
- [Scale a Skia drawn control from surface pixels to logical units](#scale-a-skia-drawn-control-from-surface-pixels-to-logical-units)
- [Turn raw pixel surfaces into XAML image sources](#turn-raw-pixel-surfaces-into-xaml-image-sources)
- [Honor EXIF orientation when decoding with SkiaSharp codecs](#honor-exif-orientation-when-decoding-with-skiasharp-codecs)
- [Combine selection polygons with the CodeBrix PolygonTools library](#combine-selection-polygons-with-the-codebrix-polygontools-library)
- [Give a headless library a drawing facade over SkiaSharp](#give-a-headless-library-a-drawing-facade-over-skiasharp)
- [Play a Lottie animation on a Skia head and on native WinUI](#play-a-lottie-animation-on-a-skia-head-and-on-native-winui)

## Related blueprints

- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - the pages, custom controls and canvas hosts these renderers are placed in
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the view-model side: owning a painter, moving decode and bake work off the UI thread, and alerting then reverting when a backend fails
- [BLUEPRINTS-MediaAndVision.md](BLUEPRINTS-MediaAndVision.md) - when the pixels come from a camera or a tracker instead of a model file
- [BLUEPRINTS-Testing.md](BLUEPRINTS-Testing.md) - headless tests that pin camera math, matrix conventions and per-head gating

---

## Graphics and rendering

### Host an OpenGL scene in XAML with a GLCanvasElement subclass

**When you want this.** You want hardware-accelerated 3D, or any OpenGL drawing,
inside an ordinary page, on every head that can give you a GL context, without
writing native GL, EGL, WGL or GLX code of your own.

**The MVVM shape.** The control is self-contained and lives in a library: it
exposes dependency properties, drives a renderer through the base class's
GL-thread lifecycle, and turns pointer input into camera motion itself. The page
places it and binds. The view model holds the loaded scene object and the play
state and knows nothing about GL.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs
public sealed class ModelSceneGlCanvas : GLCanvasElement
{
    private readonly IModelSceneRenderer _renderer = new GlModelSceneRenderer();

    /// <summary>Creates the preview control and wires its pointer (rotate/zoom) input.</summary>
    // getWindowFunc is only used on WinUI; on CodeBrix.Platform heads it is null.
    public ModelSceneGlCanvas() : base(null)
    {
        _renderer.BackgroundColor = SolidBackground;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCaptureLost += OnPointerCaptureLost;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(
            nameof(Model),
            typeof(LoadedModel),
            typeof(ModelSceneGlCanvas),
            new PropertyMetadata(null, OnModelChanged));

    public LoadedModel? Model
    {
        get => (LoadedModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var canvas = (ModelSceneGlCanvas)d;

        // Re-frame each new model from the same default angle/margins the app has always used,
        // even if the user had orbited/zoomed the previous one.
        ApplyDefaultFraming(canvas._renderer.Camera);
        canvas._renderer.SetModel(e.NewValue as LoadedModel, frameCamera: true);
        canvas.Invalidate();
    }

    /// <inheritdoc />
    protected override void Init(GL gl) => EnsureInitialized(gl);

    // Compiles the renderer's GL resources exactly once. Called from both Init and RenderOverride
    // so it does not matter which the host invokes first: GLCanvasElement does not guarantee Init
    // runs before the first RenderOverride on every head (e.g. a canvas that starts collapsed).
    private void EnsureInitialized(GL gl)
    {
        if (_rendererInitialized) { return; }
        _renderer.Initialize(gl);
        _rendererInitialized = true;
    }

    /// <inheritdoc />
    protected override void RenderOverride(GL gl)
    {
        EnsureInitialized(gl);

        // Both this preview and the head's own Skia renderer share the GL context, so save the
        // state we touch and restore it afterwards (the GLCanvasElement contract). The base has
        // already bound the off-screen framebuffer and set the viewport before calling us.
        var depthWasEnabled = gl.IsEnabled(EnableCap.DepthTest);
        var cullWasEnabled = gl.IsEnabled(EnableCap.CullFace);
        try
        {
            _renderer.Render(gl, (uint)RenderSize.Width, (uint)RenderSize.Height);
        }
        finally
        {
            if (depthWasEnabled) { gl.Enable(EnableCap.DepthTest); } else { gl.Disable(EnableCap.DepthTest); }
            if (cullWasEnabled) { gl.Enable(EnableCap.CullFace); } else { gl.Disable(EnableCap.CullFace); }
            gl.BindVertexArray(0);
            gl.UseProgram(0);
        }
    }

    /// <inheritdoc />
    protected override void OnDestroy(GL gl)
    {
        _renderer.Uninitialize(gl);
        _rendererInitialized = false;
    }
}
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- 3D models: a self-contained GL canvas that draws the bound model
     and handles rotate/zoom itself; animated models additionally
     bind their baked clip and play state -->
<render:ModelSceneGlCanvas x:Name="ModelCanvas"
                           Model="{d:Binding CurrentModel}"
                           AnimationClip="{d:Binding CurrentAnimationClip}"
                           IsAnimationPlaying="{d:Binding IsAnimationPlaying}"
                           Visibility="{d:Binding ModelViewerVisibility}" />
```

The view-model side is a plain property plus a change notification, with the parse
done off the UI thread:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The model shown in the 3D preview (null while browsing); the preview control binds to this.</summary>
public LoadedModel CurrentModel => _currentModel;

private async Task OpenModelViewAsync(PolyHavenAsset asset, DownloadedModel downloaded)
{
    //Parse the glTF and gather its stats off the UI thread; GPU upload happens lazily
    //at first paint.
    var (model, stats) = await Task.Run(() =>
    {
        var loaded = new GltfModelLoader().LoadFile(downloaded.GltfPath);
        return (loaded, ModelFileStats.FromLoadedModel(loaded, downloaded.ModelFolder));
    });

    // Hand the model to the preview control via its bound CurrentModel; the control frames
    // the camera and repaints itself. The GPU upload happens lazily at its first render.
    _currentModel = model;
    // ...
    NotifyPropertyChanged(nameof(CurrentModel));
    IsModelViewActive = true;
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` and
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Initialization must be idempotent and called from both `Init` and
  `RenderOverride`, because the base does not guarantee `Init` runs first on every
  head. A canvas that starts collapsed is the case the code calls out.
- The head's own Skia renderer shares the GL context, so every state the render
  touches - depth test, face culling, the bound vertex array, the program - is
  saved and restored in a `finally`. That is the element's contract, and the
  `finally` matters so a renderer exception cannot leave the head's context in a
  state it did not choose.
- The base binds the off-screen framebuffer and sets the viewport before calling
  the render override; do not do it again. `RenderSize` is the size to render at.
- The base constructor takes a window accessor that only matters on WinUI; pass
  null on the platform heads.
- `Invalidate()` coalesces to one paint per frame, so calling it from every
  pointer move is fine.
- When GL initialization fails outright, tell the user rather than showing an
  empty pane; see the bridge area.

### Keep the GL renderer framework-free behind an interface

**When you want this.** You want the drawing code unit-testable, swappable and
readable without a XAML type in sight, and the control to be nothing but lifecycle
and input.

**The MVVM shape.** The renderer interface mirrors the canvas lifecycle exactly
and says which thread each member is called on. The control owns the interface;
the concrete shader renderer knows nothing about the element, the framebuffer or
pointer input, which is what also lets an off-screen pipeline drive the same
renderer.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/IModelSceneRenderer.cs
/// <summary>
/// A framework-free OpenGL renderer for previewing one <see cref="LoadedModel"/> with an
/// orbit camera. The lifecycle mirrors the Graphics3DGL <c>GLCanvasElement</c> contract:
/// the host element calls <see cref="Initialize"/> from its <c>Init(GL)</c>,
/// <see cref="Render"/> from <c>RenderOverride(GL)</c>, and <see cref="Uninitialize"/>
/// from <c>OnDestroy(GL)</c> — all on the GL thread. <see cref="SetModel"/> may be called
/// from any thread; the model is uploaded on the next render.
/// </summary>
public interface IModelSceneRenderer
{
    OrbitCamera Camera { get; }
    (float R, float G, float B, float A) BackgroundColor { get; set; }

    void SetModel(LoadedModel? model, bool frameCamera = true);
    void SetFrameVertices(IReadOnlyList<ModelFramePrimitive>? frame);

    /// <summary>Compiles shaders and creates GL resources. Call once, on the GL thread.</summary>
    void Initialize(GL gl);

    /// <summary>Renders the scene into the currently bound framebuffer at the given pixel size.</summary>
    void Render(GL gl, uint width, uint height);

    /// <summary>Deletes all GL resources. Call once when the canvas is destroyed, on the GL thread.</summary>
    void Uninitialize(GL gl);
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/GlModelSceneRenderer.cs
/// <inheritdoc />
public void SetModel(LoadedModel? model, bool frameCamera = true)
{
    lock (_pendingLock)
    {
        _pendingModel = model;
        _pendingFrameCamera = frameCamera;
        _hasPendingModel = true;
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/IModelSceneRenderer.cs`
and `GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/IModelSceneRenderer.cs`

**Also shown by.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs`
(the same renderer driven into an off-screen framebuffer for a document)

**Sharp edges.**
- The setter that can be called from any thread never touches GL: it stashes the
  new data behind a lock, and all buffer creation and deletion happens inside the
  render call on the GL thread. That is what makes the "any thread" promise safe.
- The previous scene's buffers and textures are released before the new upload, in
  the same GL-thread call.
- Uniform locations are cached once at link time rather than looked up per frame.
- The camera math has no GL dependency at all, which is why it has a full unit-test
  file.

### Pick the shader version header for desktop GL or GLES at runtime

**When you want this.** One set of shaders that must run on every head, where some
give you desktop OpenGL and others give you OpenGL ES.

**The MVVM shape.** Renderer-internal. Keep the shader bodies version-agnostic and
prepend the header after probing the live context.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
public void Initialize(GL gl)
{
    ArgumentNullException.ThrowIfNull(gl);

    // The same shader source runs on desktop OpenGL and OpenGL ES; only the #version header
    // differs, so we detect the context type and prepend the right one.
    var isGles = (gl.GetStringS(StringName.Version) ?? string.Empty)
        .Contains("OpenGL ES", StringComparison.OrdinalIgnoreCase);
    var header = isGles ? "#version 300 es\n" : "#version 330 core\n";

    var vertexShader = CompileShader(gl, ShaderType.VertexShader, header + VertexShaderBody);
    var fragmentShader = CompileShader(gl, ShaderType.FragmentShader, header + FragmentShaderBody);
    // ... link, check LinkStatus, then cache every uniform location once ...
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`

**Also shown by.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/GlModelSceneRenderer.cs`
(whose class summary names the mapping: the Win32 and WPF hosts and X11 give
desktop GL, while ANGLE, Wayland EGL and the framebuffer wrapper give GLES)

**Sharp edges.**
- Open the fragment shader body with a precision qualifier, which desktop GL
  accepts and GLES requires, so the same body works under both headers.
- Throw with the driver's info log when compilation or linking fails. A silent
  black canvas is much harder to diagnose.

### Share one camera and one matrix convention across graphics APIs

**When you want this.** Your scene looks right head-on but goes flat, or depth
stops working, as soon as the camera is rotated off an axis - or you have more
than one backend and want the camera math written once.

**The MVVM shape.** The camera lives in the headless library, is owned by
whichever renderer is active, and is exposed up the chain so pointer input drives
it identically regardless of backend. The matrices go to the GPU untransposed.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
// MVP = view * projection (node transforms are baked at load time). System.Numerics
// stores matrices row-major; uploading with transpose=false makes GL read that
// row-major data as its own column-major layout, which is exactly the transpose GL
// needs. Calling Matrix4x4.Transpose here as well would double-transpose and flatten
// the depth axis for any non-axis-aligned camera.
var mvp = Camera.GetViewMatrix() * Camera.GetProjectionMatrix(width / (float)height);
gl.UniformMatrix4(_mvpLocation, 1, false, (float*)&mvp);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs
/// <summary>Frames the camera on a bounding box, keeping the current yaw/pitch.</summary>
public void FitToBounds(Vector3 boundsMin, Vector3 boundsMax)
{
    Target = (boundsMin + boundsMax) * 0.5f;
    var radius = Math.Max((boundsMax - boundsMin).Length() * 0.5f, 0.001f);
    // Distance so the bounding sphere fits the vertical fov, with FitMargin of headroom.
    Distance = radius / MathF.Sin(FovDegrees * MathF.PI / 360f) * FitMargin;
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- Do not add a transpose. GLSL, SPIR-V and MSL all read a four-by-four matrix
  column-major, which already applies the transpose that .NET's row-major storage
  needs. Transposing again silently flattens the depth axis, and only for rotated
  cameras, so an axis-aligned test view hides the bug entirely.
- The regression test that pins it uses a rotated camera on purpose and tries both
  draw orders; it exists once per renderer. See the testing area.
- There is no per-node model matrix in these samples: the loader bakes node world
  transforms into the vertex data, so one shared matrix draws everything.
- The projection maps depth to the zero-to-one range, which is Vulkan's and
  Metal's native range; desktop GL merely wastes half of its own range on it.

### Frame the camera automatically on each newly bound model

**When you want this.** Every model you load should appear well composed, at a
consistent angle, whatever the user did to the previous one.

**The MVVM shape.** The element resets framing in its dependency-property
callback; the camera exposes the framing policy as properties so a document
pipeline can choose different ones.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs
// The starting camera framing for every model: a gentle three-quarter angle that sits the
// model a little low in the frame. Applied before framing so FitToModel uses these margins.
private static void ApplyDefaultFraming(OrbitCamera camera)
{
    camera.FovDegrees = 45f;
    camera.YawDegrees = 30f;
    camera.PitchDegrees = 15f;
    camera.FitMargin = 0.73f;
    camera.VerticalFramingBias = 0.22f;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs
/// <summary>Frames the camera on a model's bounding box, keeping the current yaw/pitch.</summary>
public void FitToModel(LoadedModel model)
{
    ArgumentNullException.ThrowIfNull(model);
    FitToBounds(model.BoundsMin, model.BoundsMax);
    //Orbit around the centroid when the model provides one, so a model with a sparse
    //extremity rotates in place instead of swinging around the bounding-box center. Raise
    //the look-at point by VerticalFramingBias so the model can sit lower in the view.
    var radius = MathF.Max((model.BoundsMax - model.BoundsMin).Length() * 0.5f, 0.001f);
    Target = (model.Pivot ?? model.BoundsCenter) + new Vector3(0f, radius * VerticalFramingBias, 0f);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`

**Sharp edges.**
- Framing on the vertex centroid rather than the bounding-box center keeps a model
  with one sparse extremity rotating in place.
- Derive the near and far planes from the current distance, so a model at any
  scale keeps usable depth precision.
- Clamp pitch, distance and field of view inside the camera, so the element does
  not have to guard any of it.
- Set framing before handing over the model: framing is applied when the pending
  model is taken up at render time.

### Draw translucent surfaces in a second pass with depth writes off

**When you want this.** Glass in your scene is hiding what is behind it instead of
showing it.

**The MVVM shape.** Renderer-internal. Classify materials at load time, then draw
twice: opaque first with depth writes on, then the translucent primitives with
blending and depth writes off.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
// Two passes over the primitives: opaque (and mask) first with depth writes on, then the
// translucent (BLEND) primitives over them with blending and depth writes off, so
// glass-like surfaces show what's behind them instead of occluding it. BLEND primitives
// are not depth-sorted - fine for the small amount of transparent geometry these preview
// models carry.
DrawPrimitives(gl, blendPass: false);

gl.Enable(EnableCap.Blend);
// Straight-alpha "over": colour is weighted by source alpha, while the alpha channel
// accumulates coverage (One, OneMinusSrcAlpha) so a region already opaque behind the
// glass stays opaque.
gl.BlendFuncSeparate(
    BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha,
    BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
gl.DepthMask(false);
DrawPrimitives(gl, blendPass: true);
gl.DepthMask(true);
gl.Disable(EnableCap.Blend);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs
// ...plus KHR_materials_transmission glass (e.g. a camera's lens/flash/viewfinder),
// which is alphaMode OPAQUE yet see-through. This preview doesn't implement real
// transmission/refraction, so treat any transmissive material as translucent and render it
// with the same fixed preview opacity as BLEND surfaces, rather than as an opaque solid.
// FindChannel("Transmission") returns a channel only when the extension is present (glTF
// exporters write it only for actual glass), so its presence is a reliable glass signal.
if (alphaMode == ModelAlphaMode.Opaque && material.FindChannel("Transmission") is not null)
{
    alphaMode = ModelAlphaMode.Blend;
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Models/ModelMaterial.cs`

**Sharp edges.**
- glTF marks glass two ways and the second is easy to miss: a blend alpha mode,
  and a transmission extension on an otherwise opaque material.
- Exporters commonly mark glass as blended while leaving the base-color alpha
  fully opaque, so honoring the file alone still gives you a solid pane. A fixed
  preview opacity multiplied onto whatever real alpha the material carries fixes
  it, and riding it in the existing base-color alpha means no shader change.
- The alpha channel must accumulate coverage, or a region already opaque behind
  the glass loses its opacity when the frame is composited onto Skia.
- Blended primitives are deliberately not depth-sorted; that is acceptable for the
  small amount of transparent geometry preview models carry.
- Face culling is disabled outright in these renderers, because low-poly models
  frequently rely on double-sided rendering.
- Vulkan expresses the same thing as a second pipeline with blending enabled and
  depth writes disabled, drawn after the opaque pass.

### Render off screen product shots on the head own GL context

**When you want this.** You need high-resolution stills of your 3D scene for a
document or an export, at a size the on-screen canvas never has.

**The MVVM shape.** The view model orchestrates in ordered stages with a bound
status line; GL work stays on the UI thread inside a `using` over the context,
while the CPU-only work around it runs on `Task.Run`. When no context can be
created, the pipeline still completes with a thumbnail-led result.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//Stage 2: build the photography sets (pure CPU — off the UI thread).
var scenes = await Task.Run(() => (
    Tabletop: ShotSceneBuilder.Build(model, stages.Tabletop),
    Light: ShotSceneBuilder.Build(model, stages.Light),
    Dark: ShotSceneBuilder.Build(model, stages.Dark)));

//Stage 3: the product shots, on this head's off-screen GL context. GL work must
//  stay on the UI thread; MakeCurrent saves/restores the head's own context.
//  With no GL available the sheet still composes, led by the catalog thumbnail.
DocumentStatusText = "Rendering product shots…";
byte[] heroShot = null;
var galleryShots = new List<MarketingSheetShot>();
if (OffscreenGLContext.TryCreate(GetXamlRoot(), out var glContext))
{
    using (glContext)
    using (glContext.MakeCurrent())
    using (var shotRenderer = new ModelShotRenderer(glContext.Gl))
    {
        heroShot = shotRenderer.RenderPng(
            scenes.Tabletop, stages.Tabletop, ShotAngle.Hero, HeroShotWidth, HeroShotHeight);
        galleryShots.Add(new MarketingSheetShot("Front", shotRenderer.RenderPng(
            scenes.Light, stages.Light, ShotAngle.Front, GalleryShotWidth, GalleryShotHeight)));
        // ... Side, Back, Top ...
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs
//Rendered pixels per output pixel, per axis. 2 gives 4 samples per output pixel.
private const uint Supersample = 2;

//A conservative ceiling for the supersampled framebuffer, kept below every desktop
//  GL/GLES 3.0 implementation's minimum guarantees.
private const uint MaxFramebufferSide = 4096;

//Flips the GL-oriented (bottom-up) pixels the right way up, downscales the supersampled
//  frame to the requested output size, and encodes a PNG.
private static unsafe byte[] EncodePng(byte[] pixels, int frameWidth, int frameHeight, int width, int height)
{
    // ... row-reverse copy into an SKBitmap, ScalePixels with Mitchell resampling, then Encode ...
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs`

**Sharp edges.**
- GL work must stay on the UI thread; the context's `MakeCurrent()` returns a
  disposable that saves and restores the head's own context, so the head is
  untouched afterwards.
- Read-back pixels are bottom-up. Flip them before encoding.
- A compatibility-first GL context may offer no multisampling, so supersample and
  downscale instead, with a conservative framebuffer-size ceiling.
- Every member of the shot renderer, disposal included, must be called on the
  thread where the context is current, which is why a `using` block is the only
  correct shape.

### Generate scene set dressing as ordinary geometry

**When you want this.** You want a studio floor, backdrop and contact shadow
behind a model, without adding a second rendering path to maintain.

**The MVVM shape.** A builder that returns a composite of the same primitive and
material types the loader produces, so one renderer pass draws model and set
together with correct depth and blending, and the model itself is never modified.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs
//The stage materials are appended after the model's, so the model primitives'
//  material indices stay valid untouched.
var floorMaterialIndex = model.Materials.Count;
var coveMaterialIndex = model.Materials.Count + 1;

// ... build the floor quad and the cove cylinder ...

var composite = new LoadedModel
{
    Name = model.Name,
    Primitives = primitives,
    Materials = materials,
    //The model's own bounds and pivot, so cameras frame the product, not the set.
    BoundsMin = model.BoundsMin,
    BoundsMax = model.BoundsMax,
    Pivot = model.Pivot,
};

return new ShotScene(composite, [floorPrimitive, covePrimitive], model.Primitives);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs
/// <summary>
/// Points every stage normal straight at the light, so the stage renders fully,
/// evenly lit (its look comes from its baked textures, not from shading) while the
/// model keeps real form from the same light. Call before each shot, then re-set the
/// scene on the renderer so the changed normals re-upload.
/// </summary>
public void AimStageAtLight(Vector3 lightDirection)
{
    var direction = lightDirection == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(lightDirection);
    foreach (var primitive in StagePrimitives)
    {
        var normals = primitive.Normals;
        for (var i = 0; i < normals.Length; i += 3)
        {
            normals[i] = direction.X;
            normals[i + 1] = direction.Y;
            normals[i + 2] = direction.Z;
        }
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotStage.cs`

**Sharp edges.**
- Append the set's materials after the model's so every existing material index
  stays valid, and keep the composite's bounds and pivot as the model's, or the
  camera frames the furniture instead of the product.
- Mutating stage normals per shot means the scene must be re-set on the renderer
  before each shot so the changed data re-uploads.
- Baking the contact shadow into the floor texture rather than computing it is
  what keeps this a single-pass, shader-free addition.

### Swap the 3D graphics backend at run time from a dropdown

**When you want this.** You render with a GPU API and want the user, or a support
engineer, to pick between backends while the application is running, without
restarting and without every layer above the GPU knowing which one is active.

**The MVVM shape.** One interface is the seam. A selector registered as a
singleton owns the list of kinds, the per-platform gate and engine creation. The
view model exposes the kind names as a bound list and the selection as a bound
string property; its setter starts the switch, and everything above the seam -
painter, camera, model loading, XAML - is unchanged by the choice.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs
public interface IModelRenderEngine : IDisposable
{
    OrbitCamera Camera { get; }
    Vector3? FixedLightDirection { get; set; }
    void SetModel(LoadedModel model);
    RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background);
}

public readonly struct RenderedFrame
{
    public RenderedFrame(byte[] rgba, int width, int height, bool isBottomUp) { /* ... */ }
    public byte[] Rgba { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsBottomUp { get; }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngineSelector.cs
public sealed class ModelRenderEngineSelector : IModelRenderEngineSelector
{
    private static readonly RenderEngineKind[] Kinds =
        [RenderEngineKind.OpenGL, RenderEngineKind.Vulkan, RenderEngineKind.Metal];

    public IReadOnlyList<RenderEngineKind> AvailableKinds => Kinds;

    public bool IsSupported(RenderEngineKind kind) => kind switch
    {
        RenderEngineKind.OpenGL => true,
        RenderEngineKind.Vulkan => VulkanPlatformSupport.IsCurrentPlatformSupported,
        RenderEngineKind.Metal => MetalPlatformSupport.IsCurrentPlatformSupported,
        _ => false,
    };

    public IModelRenderEngine Create(RenderEngineKind kind, Func<XamlRoot> getXamlRoot)
    {
        if (!IsSupported(kind))
        {
            throw new NotSupportedException($"The {kind} rendering engine is not supported on this platform.");
        }

        return kind switch
        {
            RenderEngineKind.OpenGL => new OpenGlModelRenderEngineFactory(getXamlRoot).Create(),
            RenderEngineKind.Vulkan => new VulkanModelRenderEngineFactory().Create(),
            RenderEngineKind.Metal => new MetalModelRenderEngineFactory().Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<ComboBox Width="130" Height="36" VerticalAlignment="Center"
          ItemsSource="{d:Binding RenderEngineNames}"
          SelectedItem="{d:Binding SelectedRenderEngineName, Mode=TwoWay}"
          IsEnabled="{d:Binding IsNotBusy}"
          Visibility="{d:Binding EngineSelectorVisibility}" />
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngineSelector.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- The selector replaces the older shape of registering one fixed engine factory in
  the container; the per-backend factories still exist but are used inside the
  selector.
- Creation hands back a fresh engine and the caller owns and disposes it. Dispose
  the old painter, which disposes its engine, only after the new one is built.
- The list is deliberately not filtered to supported kinds, so the user learns why
  an option is unavailable; see the alert-and-revert blueprint in the view-model
  area, and pre-warm the new backend off the UI thread before swapping.

### Gate an optional graphics backend to specific heads with an allow list

**When you want this.** A capability works only on some of the six heads and you
want a predictable, testable policy rather than a driver probe that might
half-succeed on a head you never validated.

**The MVVM shape.** The policy lives in the headless library as a static class
with a pure support function plus a cached detection of the running head. The view
model never sniffs the platform itself; it asks the selector, which asks the
policy.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs
public static class VulkanPlatformSupport
{
    private const string HeadAssemblyPrefix = "CodeBrix.Platform.UI.Runtime.Skia.";

    private static readonly Lazy<PlatformHead> DetectedHead = new(DetectCurrentHead);

    public static bool IsSupported(PlatformHead head) => head switch
    {
        PlatformHead.LinuxX11 => true,
        PlatformHead.LinuxWayland => true,
        PlatformHead.Win32Skia => true,
        PlatformHead.WinWpfSkia => true,
        _ => false,
    };

    public static bool IsCurrentPlatformSupported => IsSupported(CurrentHead);

    public static PlatformHead CurrentHead => DetectedHead.Value;

    public static PlatformHead ClassifyAssemblyName(string? assemblyName)
    {
        if (assemblyName is null || !assemblyName.StartsWith(HeadAssemblyPrefix, StringComparison.Ordinal))
        {
            return PlatformHead.Unknown;
        }

        var head = assemblyName[HeadAssemblyPrefix.Length..];
        if (head == "X11") { return PlatformHead.LinuxX11; }
        if (head == "Wayland") { return PlatformHead.LinuxWayland; }
        if (head == "Linux.FrameBuffer") { return PlatformHead.LinuxFrameBuffer; }
        if (head == "MacOS") { return PlatformHead.MacOS; }
        if (head == "Wpf") { return PlatformHead.WinWpfSkia; }
        if (head == "Win32" || head.StartsWith("Win32.", StringComparison.Ordinal))
        {
            return PlatformHead.Win32Skia;
        }

        return PlatformHead.Unknown;
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalPlatformSupport.cs`

**Sharp edges.**
- The class comments call this out as a deliberate policy list, not a driver
  probe: an API is never even attempted on a platform that has not been okayed.
- Detection is a one-time scan of loaded assemblies for a head runtime assembly
  name, so the library needs no reference to any head. Prefix matching is used so
  satellite assemblies still classify to their head.
- An unrecognized host classifies as unknown and is conservatively unsupported,
  which is also what a unit-test host is; there is a test asserting exactly that.
- The Metal gate adds a process-architecture condition, keyed off the process
  rather than the machine so a translated process is classified correctly.

### Render an OpenGL scene off screen and composite it onto an SKXamlCanvas

**When you want this.** You want real GPU 3D inside an ordinary XAML page on every
head, without writing any platform GL loader code and without fighting the head's
own renderer for the context.

**The MVVM shape.** The engine is a plain class behind the engine interface; it is
created by the view model and driven from a painter, which the page calls from its
paint handler. The only thing the page supplies is a XAML root, and it supplies it
as an accessor.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/OpenGlModelRenderEngine.cs
public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
{
    // Create the off-screen context on first use, from the hosting page's XamlRoot. This runs
    // on the render thread (the Skia paint callback), so the XamlRoot accessor is valid here.
    if (_context == null)
    {
        var xamlRoot = _getXamlRoot()
            ?? throw new InvalidOperationException(
                "A XamlRoot is required to create the offscreen OpenGL context, but none was available.");
        if (!OffscreenGLContext.TryCreate(xamlRoot, out _context))
        {
            throw new InvalidOperationException(
                "The running head does not provide a native OpenGL context for offscreen rendering.");
        }
    }

    // MakeCurrent() saves whatever context the host head had current and restores it when the
    // returned scope is disposed, so this engine never disturbs the head's own renderer even
    // though they share a thread.
    using (_context.MakeCurrent())
    {
        var gl = _context.Gl;

        if (!_rendererInitialized)
        {
            _renderer.Initialize(gl);
            _rendererInitialized = true;
        }

        EnsureFramebuffer(gl, (uint)width, (uint)height);

        _renderer.BackgroundColor = background;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        _renderer.Render(gl, (uint)width, (uint)height);

        var pixels = new byte[width * height * 4];
        gl.ReadPixels(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return new RenderedFrame(pixels, width, height, isBottomUp: true);
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/OpenGlModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- A GL context must be created and used on the thread that renders. That is why
  the engine takes an accessor and not a value, and why the context is created
  lazily inside the first render rather than in the constructor. Constructing the
  engine stays cheap and thread-agnostic.
- The framebuffer, color renderbuffer and depth renderbuffer are recreated on
  every size change and the status checked; without the depth attachment 3D
  geometry does not occlude correctly.
- Disposal also has to happen with the context current, so `Dispose()` re-enters
  the make-current scope before deleting anything.
- The read-back is the main per-frame cost, which is why this application drops
  frames under load rather than lowering resolution.

### Add a self contained Vulkan renderer that needs no shader toolchain

**When you want this.** You want a second GPU backend that cannot possibly collide
with the head's renderer, and you do not want a shader compiler to become a build
prerequisite.

**The MVVM shape.** The whole Vulkan stack lives in the headless rendering
library. The display layer contributes only a thin adapter that implements the
engine interface and reports the frame's orientation.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/VulkanModelRenderEngine.cs
public sealed class VulkanModelRenderEngine : IModelRenderEngine
{
    private readonly VulkanSceneRenderer _renderer = new();

    public OrbitCamera Camera => _renderer.Camera;

    public Vector3? FixedLightDirection
    {
        get => _renderer.FixedLightDirection;
        set => _renderer.FixedLightDirection = value;
    }

    public void SetModel(LoadedModel model) => _renderer.SetModel(model);

    public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        var pixels = _renderer.RenderFrame(width, height, background);

        // The renderer keeps the camera's GL-convention matrices unmodified, and Vulkan's
        // clip-space Y points the other way - so its readback is a bottom-up image just like
        // GL's, and the same Skia flip applies (see the VulkanSceneRenderer class remarks).
        return new RenderedFrame(pixels, width, height, isBottomUp: true);
    }

    public void Dispose() => _renderer.Dispose();
}

public sealed class VulkanModelRenderEngineFactory : IModelRenderEngineFactory
{
    public IModelRenderEngine Create() => new VulkanModelRenderEngine();
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/VulkanModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanShaders.cs`

**Sharp edges.**
- Vulkan has no ambient thread state, so the renderer owns instance, devices,
  queue, command pool, off-screen color and depth images, render pass, pipelines,
  sampler, per-model buffers and textures, and the read-back. It never touches a
  window system, so it cannot collide with the head.
- Shaders are pre-compiled SPIR-V embedded as static word arrays with the source
  alongside in comments, so no shader compiler is needed at build or run time; the
  file's documentation records how to regenerate them.
- Per-draw values ride in one push-constant block shared by both stages, and a
  white fallback texture is bound for untextured materials because Vulkan still
  requires a valid sampler in the set.
- A runtime-availability probe exists for tests; the application itself uses the
  allow list plus a pre-warm render.

### Add a direct to Metal renderer with no NuGet package or Apple bindings

**When you want this.** You want a macOS GPU backend and would rather call the
Objective-C runtime yourself than take on a managed Apple binding dependency.

**The MVVM shape.** Identical to the Vulkan case: the whole stack is in the
headless library, and the display layer contributes a thin adapter plus its
factory.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalInterop.cs
internal static unsafe class MetalInterop
{
    private const string Objc = "/usr/lib/libobjc.A.dylib";
    private const string Metal = "/System/Library/Frameworks/Metal.framework/Metal";

    [DllImport(Objc, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(Objc, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(Metal, EntryPoint = "MTLCreateSystemDefaultDevice")]
    internal static extern IntPtr MTLCreateSystemDefaultDevice();

    // ---- objc_msgSend, one concrete signature per call shape (never a struct return) ------

    [DllImport(Objc, EntryPoint = "objc_msgSend")]
    internal static extern IntPtr Send(IntPtr receiver, IntPtr selector);

    [DllImport(Objc, EntryPoint = "objc_msgSend")]
    internal static extern void SendV(IntPtr receiver, IntPtr selector);

    // ...
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/MetalModelRenderEngine.cs
public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
{
    var pixels = _renderer.RenderFrame(width, height, background);

    // Unlike GL and Vulkan, Metal's clip-space Y points up while its framebuffer origin is
    // top-left, so the readback is already top-down - no Skia flip needed (see the
    // MetalSceneRenderer class remarks).
    return new RenderedFrame(pixels, width, height, isBottomUp: false);
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalInterop.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/MetalModelRenderEngine.cs`

**Sharp edges.**
- Never call an Objective-C method that returns a struct by value. That is the one
  place the message-send calling convention differs between Apple Silicon and
  Intel or translated processes. Every message here returns a pointer or void, or
  takes a struct only as an argument, so one entry point serves both
  architectures.
- Every transfer between GPU and CPU goes through a shared-storage staging buffer
  blitted to and from a private texture, which behaves identically on Apple
  Silicon, Intel and under translation, unlike shared or managed textures whose
  availability splits by GPU family. Buffer-to-texture blits require row
  alignment, so staging rows are padded and de-padded.
- Metal compiles its shading language from source at run time, so the shader file
  is just a string and no artifact is pre-baked - simpler than the Vulkan case.
- Metal enum values are declared as plain integer constants in the renderer, so no
  headers are needed.

### Composite engine pixels onto Skia with the right vertical orientation

**When you want this.** You read pixels back from a GPU API and draw them on a
Skia surface, and you need one compositing path that is correct for backends with
different framebuffer origins.

**The MVVM shape.** The painter is API-agnostic. It asks the engine for a frame,
reads the frame's own orientation flag and flips only when the flag says so. Each
engine declares its orientation; nothing else in the application knows about it.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs
public void Paint(SKSurface surface, SKImageInfo info)
{
    if (_disposed || info.Width <= 0 || info.Height <= 0) { return; }

    // When a background texture is present, render the model over a transparent clear so it
    // composites onto the texture we draw behind it; otherwise use the solid dark colour.
    var hasBackground = _backgroundBitmap != null;
    var background = hasBackground ? (0f, 0f, 0f, 0f) : SolidBackground;

    // The engine does all the API-specific work and hands back RGBA pixels.
    var frame = _engine.RenderFrame(info.Width, info.Height, background);

    DrawFrame(surface, info, frame);
}

private void DrawFrame(SKSurface surface, SKImageInfo info, RenderedFrame frame)
{
    var canvas = surface.Canvas;
    var sampling = new SKSamplingOptions(SKFilterMode.Linear);

    DrawBackground(canvas, info, sampling);

    // Straight (unpremultiplied) alpha so a transparent clear lets the background show through.
    var imageInfo = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
    using var image = SKImage.FromPixelCopy(imageInfo, frame.Rgba);

    canvas.Save();
    if (frame.IsBottomUp)
    {
        // The engine's first pixel row is the bottom of the image; flip vertically to match
        // Skia's top-down surface.
        canvas.Scale(1f, -1f);
        canvas.Translate(0f, -info.Height);
    }
    canvas.DrawImage(image, new SKRect(0, 0, info.Width, info.Height), sampling);
    canvas.Restore();
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs`

**Sharp edges.**
- OpenGL's first pixel row is the image bottom. Vulkan's clip-space Y points down,
  which with the same matrices makes its read-back come out inverted too, so it
  shares the flip. Metal's clip-space Y points up while its framebuffer origin is
  top-left, so it is the exception.
- Use unpremultiplied alpha, or the transparent clear used behind a background
  texture will not composite correctly.
- This painter always renders at full canvas resolution; smoothness under load
  comes from dropping frames, not from lowering resolution.

### Paint a CPU ray traced panorama into an SKBitmap

**When you want this.** You need an interactive image-based view that must work
even on a head with no GPU, or a fallback path that is entirely CPU-side.

**The MVVM shape.** A second painter implementation behind the same interface. The
page and the view model treat it exactly like the GPU painter; only the view model
knows which one is current, and it hides the backend dropdown while this one is
active.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/PanoramaScenePainter.cs
//Cap the CPU ray-traced resolution so a maximized window stays interactive; the result
//is scaled up to fill the canvas. A lower cap while dragging keeps the look-around smooth;
//the crisp full cap is used once the drag stops.
private const int MaxRenderDimension = 1440;
private const int DragRenderDimension = 960;

public void Paint(SKSurface surface, SKImageInfo info)
{
    if (_disposed || info.Width <= 0 || info.Height <= 0) { return; }

    var (renderWidth, renderHeight) = CapResolution(info.Width, info.Height);
    if (_buffer == null || _bufferWidth != renderWidth || _bufferHeight != renderHeight)
    {
        _buffer?.Dispose();
        _buffer = new SKBitmap(new SKImageInfo(renderWidth, renderHeight, SKColorType.Rgba8888, SKAlphaType.Opaque));
        _bufferWidth = renderWidth;
        _bufferHeight = renderHeight;
    }

    _renderer.RenderTo(_buffer);
    surface.Canvas.DrawBitmap(_buffer, new SKRect(0, 0, info.Width, info.Height), new SKSamplingOptions(SKFilterMode.Linear));
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/PanoramaScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Panorama/EquirectPanoramaRenderer.cs`

**Sharp edges.**
- The bitmap is reused across frames and only reallocated on a size change; the
  renderer writes into it in place, in parallel over the rows.
- Two resolution caps - a lower one while dragging, the full one once the drag
  stops - make this a dynamic-quality strategy, the opposite of the GPU painter's
  drop-frames strategy.
- The renderer rejects a bitmap whose color type is not the one it writes.
- The panorama camera clamps pitch to avoid the poles and clamps the field of
  view.

### Decode HDR images and tone map them for display

**When you want this.** Your application shows high-dynamic-range content, or any
linear-light data, and you need one place that turns a file into either a display
bitmap or a float image.

**The MVVM shape.** A static loader in the headless library dispatches on file
extension; the view model calls it from a `Task.Run` and never sees a decoder.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Textures/TextureImageLoader.cs
public static SKBitmap LoadForDisplay(byte[] data, string fileExtension)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

    switch (NormalizeExtension(fileExtension))
    {
        case "exr":
            return NormalizeToBitmap(ExrDecoder.Decode(data));
        case "hdr":
            return ToneMapper.ToBitmap(RadianceHdrDecoder.Decode(data));
        default:
            return LdrImageDecoder.Decode(data);
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/ToneMapping/ToneMapper.cs
internal static float ApplyOperator(float value, ToneMapOperator toneMapOperator)
{
    if (value <= 0f) { return 0f; }

    return toneMapOperator switch
    {
        // Krzysztof Narkowicz's ACES filmic approximation.
        ToneMapOperator.AcesFilmic =>
            Math.Clamp(value * (2.51f * value + 0.03f) / (value * (2.43f * value + 0.59f) + 0.14f), 0f, 1f),
        ToneMapOperator.Reinhard => value / (1f + value),
        ToneMapOperator.Clamp => Math.Min(value, 1f),
        _ => throw new ArgumentOutOfRangeException(nameof(toneMapOperator), toneMapOperator, "Unknown tone-map operator."),
    };
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Textures/TextureImageLoader.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/ToneMapping/ToneMapper.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Images/RadianceHdrDecoder.cs`

**Sharp edges.**
- The two HDR extensions are treated differently on purpose: one usually carries
  non-photographic data maps and gets min-max normalization so the full value
  range shows, while panoramas get real tone mapping.
- The float-image entry point deliberately refuses low-dynamic-range extensions
  rather than silently promoting them.
- The decoder narrows the imaging library's format exceptions to one exception
  type, so callers have one thing to catch; the model loader uses that to degrade
  an undecodable texture to the material's base color instead of failing the whole
  load.

### Build a textured cube mesh from a bitmap for previewing a flat material

**When you want this.** You have a flat texture and a swatch rectangle would not
show it as a material. A cube at a corner angle does.

**The MVVM shape.** A static builder that returns the same loaded-model type the
glTF loader produces, so the renderer path is identical for both sources.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/CubeMeshBuilder.cs
// Each face: four corners (CCW seen from outside) + its outward normal. UVs are the
// same (0,0)-(1,0)-(1,1)-(0,1) per face so the whole texture shows on every face.
private static readonly (Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 Normal)[] Faces = [ /* ... */ ];

public static LoadedModel Build(SKBitmap texture, string name)
{
    ArgumentNullException.ThrowIfNull(texture);
    var (rgba, width, height) = ToRgba(texture);
    // ... fill positions / normals / texCoords / indices per face ...

    return new LoadedModel
    {
        Name = name,
        Primitives = [primitive],
        Materials = [material],
        BoundsMin = new Vector3(-0.5f, -0.5f, -0.5f),
        BoundsMax = new Vector3(0.5f, 0.5f, 0.5f),
    };
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//A fixed key light (from upper-front-left) shades the faces distinctly so the
//  cube reads as solid, and a 3/4 angle with perspective shows three faces.
//  Extra framing margin keeps the whole cube (and its rotating silhouette)
//  in view against the backdrop, which reads as clearly 3D.
_modelPainter.FixedLightDirection = new Vector3(-0.4f, 1f, 0.7f);
_modelPainter.Camera.FovDegrees = 40f;
_modelPainter.Camera.YawDegrees = 35f;
_modelPainter.Camera.PitchDegrees = 28f;
_modelPainter.Camera.FitMargin = 1.2f;
_modelPainter.Camera.VerticalFramingBias = 0f;
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/CubeMeshBuilder.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Set a fixed light for a solid symmetric shape. The default here is a camera
  headlight, which double-sides the lighting - good for flat or foliage models,
  but it makes a cube read as ambiguous because every face gets the same
  brightness.
- Camera framing must be set before handing over the model, because framing is
  applied when the pending model is taken up at render time.
- The model's texture is a plain byte array, so nothing GPU-specific leaks into
  the mesh data.

### Paint a zoomable image on an SKXamlCanvas from the view model

**When you want this.** You need a drawing surface whose content and zoom come
from the view model, repainted on demand from view-model code that has no access
to the control.

**The MVVM shape.** The painting logic is a plain Skia class, with no UI types,
that the view model owns as a property; the page's paint handler forwards the
canvas and size to it in one line. Repaint requests travel the other way through a
bridge whose single delegate the page fills in and marshals onto the UI thread.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IImageCanvasBridge.cs
/// <summary>
/// The head-capability bridge for the 2D image viewer: the page fills in how the
/// SkiaSharp canvas is repainted (marshalled to the UI thread). The view model must
/// behave sensibly when the delegate is <c>null</c>.
/// </summary>
public interface IImageCanvasBridge { Action InvalidateImageCanvas { get; set; } }
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The 2D canvas painter the page's SkiaSharp canvas paints with.</summary>
public ImageCanvasPainter ImagePainter { get; } = new();

/// <inheritdoc />
public Action InvalidateImageCanvas { get; set; }

public string ZoomText => $"{ImagePainter.ZoomFactor * 100:0}%";

public SimpleCommand ZoomInCommand => field ??= new SimpleCommand(() => AdjustZoom(1.25f));
public SimpleCommand ZoomOutCommand => field ??= new SimpleCommand(() => AdjustZoom(0.8f));

/// <summary>Applies one wheel notch of zoom from the page's pointer-wheel handler.</summary>
public void AdjustZoomFromWheel(int wheelDelta) => AdjustZoom(wheelDelta > 0 ? 1.25f : 0.8f);

private void AdjustZoom(float factor)
{
    ImagePainter.ZoomFactor = Math.Clamp(ImagePainter.ZoomFactor * factor, 0.25f, 16f);
    NotifyPropertyChanged(nameof(ZoomText));
    InvalidateImageCanvas?.Invoke();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Marshal 2D-canvas invalidations from the view model onto the UI thread
viewModel.InvalidateImageCanvas = () => DispatcherQueue?.TryEnqueue(() => ImageCanvas?.Invalidate());

//The 2D viewer: the view model's painter draws images and spritesheets (checkerboard,
//zoom, sprite spotlight) onto this SkiaSharp surface.
ImageCanvas.PaintSurface += (_, e) =>
    ViewModel?.ImagePainter.Paint(e.Surface.Canvas, e.Info.Width, e.Info.Height);
ImageCanvas.SizeChanged += (_, _) => ImageCanvas.Invalidate();
ImageCanvas.PointerWheelChanged += (_, e) =>
{
    var delta = e.GetCurrentPoint(ImageCanvas).Properties.MouseWheelDelta;
    ViewModel?.AdjustZoomFromWheel(delta);
    e.Handled = true;
};
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IImageCanvasBridge.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The invalidation delegate wraps its call in the dispatcher, because the view
  model raises it from continuations that may not be on the UI thread.
- A resize alone does not repaint, so the size-changed handler must invalidate.
- The painter's layout helpers are public so the mapping between canvas and image
  space is unit-testable without a canvas, and the test project exercises exactly
  that.
- Above a threshold the painter switches to nearest-neighbor sampling, which keeps
  low-resolution pixel art crisp instead of smearing it - the right default for
  game assets.
- The view model disposes the previous bitmap when swapping in a new one, and
  clears both the painter's bitmap and its highlight when the viewer closes.

### Spotlight one region of an image on the canvas

**When you want this.** You are showing an atlas, a scanned page or any image with
named sub-rectangles, and selecting one from a list should highlight it in place.

**The MVVM shape.** A row view model per region with its own command and selected
state; the owning view model handles selection, sets the painter's highlight
rectangle and asks for a repaint through the canvas bridge. The painter does the
dimming and the outline.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private void SelectRegion(AtlasRegionCellViewModel row)
{
    //Re-selecting the spotlighted region clears the spotlight
    var selecting = !row.IsSelected;
    foreach (var region in AtlasRegions)
    {
        region.IsSelected = selecting && region == row;
    }

    ImagePainter.HighlightRegion = selecting
        ? new SKRectI(row.Region.X, row.Region.Y,
            row.Region.X + row.Region.Width, row.Region.Y + row.Region.Height)
        : null;
    InvalidateImageCanvas?.Invoke();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs
private void PaintHighlight(SKCanvas canvas, SKRect imageRect, SKRectI region, SKBitmap bitmap)
{
    var scaleX = imageRect.Width / bitmap.Width;
    var scaleY = imageRect.Height / bitmap.Height;
    var regionRect = new SKRect(
        imageRect.Left + (region.Left * scaleX),
        imageRect.Top + (region.Top * scaleY),
        imageRect.Left + (region.Right * scaleX),
        imageRect.Top + (region.Bottom * scaleY));

    //Dim everything except the spotlighted region, then outline it
    canvas.Save();
    canvas.ClipRect(regionRect, SKClipOperation.Difference);
    using var dimPaint = new SKPaint { Color = HighlightDim };
    canvas.DrawRect(imageRect, dimPaint);
    canvas.Restore();

    using var strokePaint = new SKPaint
    {
        Color = HighlightStroke,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
        IsAntialias = true,
    };
    canvas.DrawRect(regionRect, strokePaint);
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AtlasRegionCellViewModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs`

**Sharp edges.**
- The dim pass uses a difference clip so the region stays at full brightness
  without a second draw of the image.
- Highlight coordinates are in image pixels; the painter converts them with the
  same scale it used to place the image, so the spotlight tracks zoom for free.
- Selecting the already-selected row clears the spotlight, which is what makes the
  row list behave like a toggle.

### Play a baked animation clip in a preview canvas

**When you want this.** A model with animations should play them, without teaching
the renderer about skinning or node hierarchies.

**The MVVM shape.** The clip is baked off the UI thread by the view model - a
stale bake is discarded on arrival - exposed as a bound property, and played by
the control with a UI-thread timer at the clip's own frame rate. The view model
owns the clip list, the selection and the play state; the control owns the timer.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Bakes the chosen clip off the UI thread and hands it to the canvas when still current
private async Task BakeSelectedClipAsync(string animationName)
{
    var animated = _animatedModel;
    if (animated == null || !animated.HasAnimations) { return; }

    try
    {
        var clip = await Task.Run(() => animated.BakeClip(animationName));
        if (_selectedAnimation == animationName && _animatedModel == animated)
        {
            CurrentAnimationClip = clip;
        }
    }
    catch (Exception)
    {
        //A clip that fails to bake leaves the model in its rest pose.
        CurrentAnimationClip = null;
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs
//Playback is a plain UI-thread timer at the clip's bake rate: each tick pushes the next
//baked frame's vertices to the renderer and invalidates. Stops itself when nothing plays.
private void UpdateAnimationTimer()
{
    var clip = AnimationClip;
    if (clip == null || !IsAnimationPlaying)
    {
        _animationTimer?.Stop();
        return;
    }

    _animationTimer ??= CreateAnimationTimer();
    _animationTimer.Interval = TimeSpan.FromSeconds(1d / clip.FrameRate);
    _animationTimer.Start();
}

private void PushAnimationFrame()
{
    var clip = AnimationClip;
    if (clip == null || clip.Frames.Count == 0) { return; }

    _renderer.SetFrameVertices(clip.Frames[_animationFrameIndex % clip.Frames.Count].Primitives);
    Invalidate();
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/AnimatedModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Baking trades memory for simplicity: playback is just swapping vertex buffers,
  so the renderer never needs skinning support.
- Frames are sampled over a half-open interval so a looping clip does not hold a
  duplicated end pose, and the frame count is clamped at both ends.
- The bake asserts that each frame's vertex layout matches the rest pose and
  throws rather than drawing garbage.
- The result is published only if the selection and the model have not changed
  since the bake started.
- Animated models are built from an evaluated rest pose rather than a static node
  walk, so the primitive layout matches the per-frame evaluations exactly.

### Rasterize SVG art with the CodeBrix SkiaSvg library

**When you want this.** You have vector art to display in a raster surface, at a
display size and again at a thumbnail size, or you ship an icon set and want it
available at any size.

**The MVVM shape.** A static decoder or a resource service in a library with no UI
types, returning a bitmap or encoded bytes. The view model calls it inside
`Task.Run`; callers ask for a pixel surface and never learn whether a raster or a
vector answered.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/SvgImageDecoder.cs
public static SKBitmap Render(byte[] svgBytes, int maxDimension = 1024)
{
    ArgumentNullException.ThrowIfNull(svgBytes);

    using var stream = new MemoryStream(svgBytes, writable: false);
    SKSvg svg;
    try
    {
        svg = SKSvg.CreateFromStream(stream);
    }
    catch (Exception ex)
    {
        throw new InvalidDataException("The data is not a renderable SVG.", ex);
    }

    using var _ = svg;
    var picture = svg.Picture;
    var rect = picture?.CullRect ?? SKRect.Empty;
    if (picture == null || rect.Width <= 0 || rect.Height <= 0)
    {
        throw new InvalidDataException("The data is not a renderable SVG.");
    }

    var scale = Math.Min(maxDimension / rect.Width, maxDimension / rect.Height);
    var width = Math.Max(1, (int)MathF.Round(rect.Width * scale));
    var height = Math.Max(1, (int)MathF.Round(rect.Height * scale));

    var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.Transparent);

    //Drawn by hand rather than through SKPictureExtensions.ToBitmap, which does not
    //translate by CullRect's origin — an SVG whose content does not start at (0,0)
    //would come out clipped
    canvas.Scale(scale);
    canvas.Translate(-rect.Left, -rect.Top);
    canvas.DrawPicture(picture);
    return bitmap;
}
```

An icon service resolves by embedded-resource name, preferring an exact-size
raster and falling back to the vector:

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Pinta.Brix.Controls.csproj -->
<!-- The application icon set carried over from upstream (see
     THIRD-PARTY-NOTICES.txt at the repo root for attribution) -->
<ItemGroup>
  <EmbeddedResource Include="Assets\icons\**\*.png" />
  <EmbeddedResource Include="Assets\icons\**\*.svg" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/SkiaResourceService.cs
private ImageSurface? LoadIcon (string name, int size)
{
	// Embedded resource names look like:
	//   Pinta.Brix.Controls.Assets.icons.hicolor.16x16.actions.<name>.png
	//   Pinta.Brix.Controls.Assets.icons.hicolor.scalable.actions.<name>.svg
	string pngSuffix = $".{name}.png";
	string svgSuffix = $".{name}.svg";

	// Prefer an exact-size PNG, then the nearest larger size, then SVG.
	string? exact = resource_names.FirstOrDefault (r => r.Contains ($".{size}x{size}.") && r.EndsWith (pngSuffix, StringComparison.Ordinal));
	if (exact is not null)
		return DecodePng (exact, size);

	string? svg = resource_names.FirstOrDefault (r => r.Contains (".scalable.") && r.EndsWith (svgSuffix, StringComparison.Ordinal));
	if (svg is not null)
		return RenderSvg (svg, size);
	// ...
}

private ImageSurface? RenderSvg (string resourceName, int size)
{
	using Stream? stream = assembly.GetManifestResourceStream (resourceName);
	if (stream is null)
		return null;

	using SKSvg svg = new ();
	SKPicture? picture = svg.Load (stream);
	if (picture is null)
		return null;

	ImageSurface surface = new (Format.Argb32, size, size);
	using SKCanvas canvas = new (surface.Bitmap);
	SKRect bounds = picture.CullRect;
	if (bounds.Width > 0 && bounds.Height > 0) {
		float scale = Math.Min (size / bounds.Width, size / bounds.Height);
		canvas.Scale (scale);
		canvas.Translate (-bounds.Left, -bounds.Top);
	}
	canvas.DrawPicture (picture);
	canvas.Flush ();
	surface.MarkDirty ();
	return surface;
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/SvgImageDecoder.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/SkiaResourceService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IResourceService.cs`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs`
(the same library behind an image source, reached from XAML by a custom URI
scheme; see the views area)

**Sharp edges.**
- The picture's own bounds may have a non-zero origin, so translate as well as
  scale. Convenience helpers that skip the translate clip any drawing whose
  content does not start at the origin.
- Computing the scale so the longer side lands on the requested dimension scales
  small icons up as well as large art down, which is the point of a vector source.
- Embedded resource names replace path separators with dots, so lookups are suffix
  and substring matches, not path matches.
- A lookup that never fails needs a companion "do you have this" query, so a
  caller that wants a text-label fallback can ask first.
- Cache by name and size: icons are requested repeatedly by menus, toolbars and
  pads.
- Throw a single, specific exception for unusable data and let the view model show
  it through its error dialog rather than crashing.

### Decode raster images with the CodeBrix Imaging library into a Skia bitmap

**When you want this.** You have image bytes in an unknown supported format and
need either a displayable bitmap or raw RGBA for a GPU upload.

**The MVVM shape.** A static decoder with two entry points in a library with no UI
types, called from `Task.Run` by the view model or from a renderer's texture
upload.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/LdrImageDecoder.cs
/// <summary>Decodes an image from a stream into an RGBA <see cref="SKBitmap"/>.</summary>
/// <exception cref="InvalidDataException">The data is not a decodable image.</exception>
public static unsafe SKBitmap Decode(Stream stream)
{
    ArgumentNullException.ThrowIfNull(stream);

    Image<Rgba32> image;
    try
    {
        image = Image.Load<Rgba32>(stream);
    }
    catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
    {
        throw new InvalidDataException("The data is not a decodable image.", ex);
    }

    using (image)
    {
        var bitmap = new SKBitmap(new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var destination = new Span<byte>((void*)bitmap.GetPixels(), bitmap.ByteCount);
        image.CopyPixelDataTo(destination);
        return bitmap;
    }
}

/// <summary>
/// Decodes an image from a byte buffer into a raw RGBA byte array (4 bytes per pixel,
/// row-major, top-left origin) — the form GPU texture uploads want.
/// </summary>
public static (byte[] Rgba, int Width, int Height) DecodeToRgbaBytes(byte[] data) { /* ... */ }
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/LdrImageDecoder.cs`

**Also shown by.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs`,
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs`

**Sharp edges.**
- Turning on unsafe blocks in the library lets pixels be copied straight into the
  Skia bitmap's buffer without an intermediate array.
- The alpha type must match what the decoder produces; getting it wrong shows up
  as dark fringing on transparent edges.
- Only the two documented decode failures are translated to a domain exception;
  anything else propagates, so a genuine bug is not swallowed as "not an image".

### Normalize a downloaded image before embedding it in a document

**When you want this.** Images arrive in whatever format and resolution the source
holds, and the document embedder accepts only some of them.

**The MVVM shape.** A static pipeline in the library that decodes, optionally
resizes, and re-encodes only when it must; the caller catches the throw and turns
it into a warning plus a placeholder card rather than a failed document.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/ImagePipeline.cs
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats;
using CodeBrix.Imaging.Formats.Gif;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.Formats.Webp;
using CodeBrix.Imaging.Processing;

/// <summary>
/// Normalises a downloaded image for PDF embedding: capped pixel width, JPEG for
/// photographs, PNG for graphics with transparency (which also converts formats
/// the PDF embedder cannot take, such as WebP and GIF).
/// </summary>
internal static class ImagePipeline
{
    private const int MaxPixelWidth = 1800;
    private const int JpegQuality = 87;

    /// <summary>
    /// Decodes and normalises image bytes. Throws on undecodable data — callers
    /// turn that into a warning plus a media card, never a failed document.
    /// </summary>
    public static ProcessedImage ProcessForPrint(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        using var image = Image.Load(bytes, out IImageFormat format);

        var keepsTransparency = format is PngFormat or WebpFormat or GifFormat;
        var needsResize = image.Width > MaxPixelWidth;

        //Untouched JPEG/PNG bytes embed best — only re-encode when we must
        if (!needsResize && (format is JpegFormat || format is PngFormat))
        {
            return new ProcessedImage { Bytes = bytes, Width = image.Width, Height = image.Height };
        }

        if (needsResize)
        {
            image.Mutate(x => x.Resize(MaxPixelWidth, 0));
        }

        using var output = new MemoryStream();
        if (keepsTransparency)
        {
            image.Save(output, new PngEncoder());
        }
        else
        {
            image.Save(output, new JpegEncoder { Quality = JpegQuality });
        }

        return new ProcessedImage { Bytes = output.ToArray(), Width = image.Width, Height = image.Height };
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
var bytes = processed.Bytes;
var image = figure.AddImage(ImageSource.FromBinary(
    $"img-{_figureNumber}-{Guid.NewGuid():N}", () => bytes, quality: 90));
image.LockAspectRatio = true;
image.Width = Unit.FromPoint(width);
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/ImagePipeline.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs`

**Sharp edges.**
- Already-suitable bytes are passed through untouched; re-encoding is a last
  resort, not a default.
- Transparency decides the output format, and that same decision is what converts
  formats the embedder cannot take.
- The image source takes a name plus a byte-producing lambda; capture the bytes in
  a local first and give each image a unique name.
- The document layer's imaging back-end has to be set before any image is placed;
  see the font-registration blueprint in the documents area.

### Create a drawing session with named color layers

**When you want this.** Freehand annotation in several translucent colors, where
overlapping passes of one color must not compound and the user picks which kind of
mark they are making.

**The MVVM shape.** The view model - or a small library class it owns - creates a
`DrawingSession`, adds one layer per color by name, and exposes only what the
application needs: the active color, stroke counts, clear and export. Selecting a
color is a command that looks the layer up by name. The session is exposed as a
read-only property so the page can render it and feed it pointer events; nothing
else about it leaks into the UI.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public const string PainLayerName = "Pain";
public const string NumbnessLayerName = "Numbness";
public const string TinglingLayerName = "Tingling";

public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        _session = new DrawingSession(new DrawingSessionOptions
        {
            BackgroundFillColor = Color.White,
            SurfaceClearColor = Color.White,
        });

        //The same highlighter colors the original NuraPad application used
        _session.AddLayer(PainLayerName, Color.FromRgb(255, 30, 230));
        _session.AddLayer(NumbnessLayerName, Color.FromRgb(30, 128, 204));
        _session.AddLayer(TinglingLayerName, Color.FromRgb(204, 170, 10));

        LoadBodyMapBackground();
        // ...
    }
}

/// <summary>
/// The interactive drawing session; the hosting page renders it in its paint handler and
/// forwards pointer events to it.
/// </summary>
public DrawingSession Session => _session;

private void SetActiveLayer(string layerName)
{
    DrawingLayer layer = _session?.GetLayer(layerName);
    if (layer != null)
    {
        _session.ActiveLayer = layer;
        ActiveLayerName = layerName;
    }
}
```

The page's paint handler is one line:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
DrawCanvas.PaintSurface += (_, e) => ViewModel?.Session?.Render(e.Surface, e.Info);

// ...

DrawCanvas.SizeChanged += (_, _) => DrawCanvas.Invalidate();
```

A session built over a captured photograph looks the same, with the background
supplied as raw pixels:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public static PaintingSession Create(byte[] bgraPixels, int width, int height, bool mirrorHorizontally)
{
    if (bgraPixels == null) { throw new ArgumentNullException(nameof(bgraPixels)); }
    if (width < 1) { throw new ArgumentOutOfRangeException(nameof(width)); }
    if (height < 1) { throw new ArgumentOutOfRangeException(nameof(height)); }

    //The raw-BGRA factory copies the pixels, optionally mirrors, and owns the bitmap -
    //  no SkiaSharp decode/mirror round-trip lives here anymore.
    DrawingSession session = DrawingSession.CreateForImage(
        bgraPixels, width, height,
        CalibrationSizing.DeriveFromBackgroundImage,
        new DrawingSessionOptions
        {
            BackgroundFillColor = Color.White,   //JPEG has no alpha - keep the fill opaque
            SurfaceClearColor = Color.Black,     //letterbox bars around the still
            StrokeWidth = BrushRadius * 2f,
        },
        mirrorHorizontally);
    return new PaintingSession(session);
}

private PaintingSession(DrawingSession session)
{
    _session = session;

    foreach (HighlighterColor color in HighlighterPalette.Colors)
    {
        _session.AddLayer(color.Name, color.Color);
    }
    ActiveColorName = HighlighterPalette.Colors[0].Name;
}

public bool SelectColor(string colorName)
{
    DrawingLayer layer = _session.GetLayer(colorName);
    if (layer == null) { return false; }

    _session.ActiveLayer = layer;
    ActiveColorName = layer.Name;
    return true;
}
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/HighlighterPalette.cs`

**Sharp edges.**
- One layer per color is what makes the highlighter effect work: repeated passes
  of one color over the same area do not darken where they cross.
- Keep the layer names as constants, or as the palette's own names, and use them
  both as the session key and as the value of the bound "active" property, so the
  two can never drift apart. Only change the bound name when the lookup actually
  returned a layer.
- The layer colors are passed as opaque values; the ink is drawn translucent by
  the library. Tinting a button with the same hue at partial alpha is what matches
  the on-canvas ink.
- The background fill is opaque where the source format has no alpha, and the
  surface clear color is what shows in the letterbox bars around the image.
- The session can be null in the designer, because the view model skips
  construction in design mode, so every handler uses null-conditional access.

### Export a drawing at a chosen pixel size

**When you want this.** You want the finished artwork at a fixed resolution,
independent of how large the on-screen canvas happens to be.

**The MVVM shape.** One call on the session inside the save command, returning
bytes that are written asynchronously. Nothing about the window, the canvas size
or the display scale is involved, so the same call would work with no UI at all.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
private const int ExportPixelSize = 1000;

// ...

byte[] png = _session.ExportPng(new Size(ExportPixelSize, ExportPixelSize));
await File.WriteAllBytesAsync(outputPath, png);

StatusText = $"Saved: {outputPath}";
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
var jpeg = _paintSession.ExportJpeg();
await File.WriteAllBytesAsync(outputPath, jpeg);
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`

**Sharp edges.**
- The export includes the background image and every layer; there is no separate
  compositing step in either application.
- Set the busy flag around the export and clear it in a `finally`, with the
  affected commands marked so the buttons disable themselves while it runs.
- WebcamPainter exports at the photograph's native resolution rather than a fixed
  size, which is the other reasonable choice.

### Drive strokes in normalized image coordinates from a sensor

**When you want this.** Your stroke input comes from something other than a
pointer - a tracker, a controller, a network feed - and you do not want view-size,
display-scale or letterbox math anywhere near it.

**The MVVM shape.** The library exposes begin, continue and end in zero-to-one
image coordinates; the view model converts the sensor's normalized position into
stroke calls directly. Nothing in the input path knows the canvas size.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public bool BeginStroke(float normX, float normY)
    => _session.PointerPressedNormalized(normX, normY);

public bool ContinueStroke(float normX, float normY)
    => _session.PointerMovedNormalized(normX, normY);

public bool EndStroke() => _session.PointerReleased();

public void CancelStroke() => _session.PointerCanceled();
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
var paintNow = result.HandDetected && result.IsOpenPalm;
IsBrushPainting = paintNow;

//Strokes are driven in normalized still-image coordinates, so no canvas size is
//  needed - the drawing space is calibrated from the captured photo.
if (paintNow && CrosshairNormX != null && CrosshairNormY != null)
{
    if (session.IsStrokeActive)
    {
        session.ContinueStroke(CrosshairNormX.Value, CrosshairNormY.Value);
    }
    else
    {
        session.BeginStroke(CrosshairNormX.Value, CrosshairNormY.Value);
    }
}
else if (session.IsStrokeActive)
{
    session.EndStroke();
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Normalized input works before the first render, because the drawing space is
  calibrated from the background image rather than from a view size. There is a
  test named for exactly that.
- The stroke state machine is driven purely by whether a stroke is active: no
  hand, or a closed hand, ends the stroke; the next open palm begins a new one.
- A position outside the normalized range is ignored by the drawing session rather
  than clamped.

### Keep a mirrored preview and a mirrored drawing consistent

**When you want this.** A selfie-style preview is mirrored, so everything
downstream of it has to agree about which way is left.

**The MVVM shape.** The renderer mirrors at draw time, the still is mirrored at
capture time, and the view model mirrors the tracker's horizontal coordinate. Each
of the three is a single line, each documented where it happens.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
//The preview the user was watching is mirrored, so mirror the still to match
var session = await Task.Run(() =>
    PaintingSession.Create(photo.PixelsBgra32, photo.Width, photo.Height, mirrorHorizontally: true));
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
if (result.HandDetected)
{
    //The preview and the captured still are mirrored, so mirror the hand too
    CrosshairNormX = 1f - result.PalmCenterX;
    CrosshairNormY = result.PalmCenterY;
}
else
{
    CrosshairNormX = null;
    CrosshairNormY = null;
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraCanvas.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The result type documents its coordinates as normalized across the unmirrored
  camera frame, so the mirror is the consumer's job, and only the horizontal axis
  flips.
- The mirroring test uses an asymmetric fixture so the flip is actually observable
  in the exported image.

### Draw a brush sized cursor over a rendered drawing session

**When you want this.** The user is painting with something that is not a mouse
and needs to see where the brush is and how big it is.

**The MVVM shape.** A static render helper in the drawing library takes the
session plus three primitive values from the view model: normalized position and
whether ink is flowing. The page's paint handler passes them through; the helper
never touches the view model.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintCanvas.cs
public static void Render(SKSurface surface, SKImageInfo info, PaintingSession session,
    float? crosshairNormX, float? crosshairNormY, bool isPainting)
{
    if (surface == null || session == null) { return; }

    session.Session.Render(surface, info);

    if (!crosshairNormX.HasValue || !crosshairNormY.HasValue) { return; }

    CodeBrix.Imaging.PointF center = session.NormalizedToView(
        crosshairNormX.Value, crosshairNormY.Value, info.Width, info.Height);
    float radius = session.GetBrushRadiusInView(info.Width, info.Height);
    if (radius <= 0) { return; }

    SKCanvas canvas = surface.Canvas;
    SKColor ringColor = isPainting ? new SKColor(80, 255, 80) : SKColors.White;

    //A dark halo behind the ring keeps the cursor visible over any photo content
    using (var halo = new SKPaint
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 4f,
        Color = new SKColor(0, 0, 0, 140), IsAntialias = true,
    })
    {
        canvas.DrawCircle(center.X, center.Y, radius, halo);
    }

    using (var ring = new SKPaint
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 2f,
        Color = ringColor, IsAntialias = true,
    })
    {
        canvas.DrawCircle(center.X, center.Y, radius, ring);

        float arm = Math.Max(8f, radius * 0.35f);
        canvas.DrawLine(center.X - arm, center.Y, center.X + arm, center.Y, ring);
        canvas.DrawLine(center.X, center.Y - arm, center.X, center.Y + arm, ring);
    }
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public PointF NormalizedToView(float normX, float normY, float viewWidth, float viewHeight)
{
    RectangleF fit = GetImageRectInView(viewWidth, viewHeight);
    return new PointF(fit.X + (normX * fit.Width), fit.Y + (normY * fit.Height));
}

public RectangleF GetImageRectInView(float viewWidth, float viewHeight)
    => _session.GetDrawingRect(new SizeF(viewWidth, viewHeight));

public float GetBrushRadiusInView(float viewWidth, float viewHeight)
    => _session.ScaleToView(BrushRadius, new SizeF(viewWidth, viewHeight));
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintCanvas.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`

**Sharp edges.**
- The dark halo drawn under the ring is what keeps the cursor readable over both
  dark and bright content.
- The cursor radius comes from the drawing session's own view scaling, so the ring
  always matches what a stroke will actually cover.
- Convert through the session's own drawing rectangle - the same letterbox
  rectangle the renderer uses - rather than recomputing aspect-fit math.
- Give the crosshair arms a minimum length so they stay visible when the brush is
  small on screen.

### Draw an animated SkSL shader as a game engine direct drawing

**When you want this.** A full-surface procedural visual - a plasma, a gradient
field, a starfield - that runs on the GPU when one is available and on the CPU
when it is not, with per-frame parameters coming from application state.

**The MVVM shape.** A `DirectDrawingBase` subclass owns the shader source,
compiles it once in its constructor, and reads its per-frame inputs from a
thread-safe state object handed in at construction. It never talks to the view
model.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs
public sealed class EtherealBackdrop : DirectDrawingBase
{
    private const string EtherealSksl = @"
uniform float iTime;
uniform float2 iResolution;
uniform float3 iPalm0;
// ... iPalm1..iPalm3

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float2 p = uv * 2.0 - 1.0;
    p.x *= iResolution.x / iResolution.y;
    // ... three interfering waves, a radial swirl, and the palm warp/ripple/glow terms
    return half4(half3(col), 1.0);
}";

    public EtherealBackdrop(RenderSurfaceHostBase renderSurfaceHost, View view, Rectangle screenBounds,
        PalmAttractorField attractorField)
        : base(renderSurfaceHost, DirectDrawingMode.View, null, view, screenBounds, null, "ethereal-backdrop")
    {
        _attractorField = attractorField ?? throw new ArgumentNullException(nameof(attractorField));
        _effect = CreateEffect();
        _uniforms = new SKRuntimeEffectUniforms(_effect);
        // ... allocate the reused per-frame buffers and the fixed star seeds
    }

    /// <summary>
    /// Compiles the backdrop's SkSL shader (also exercised directly by the unit tests).
    /// </summary>
    internal static SKRuntimeEffect CreateEffect()
        => SKRuntimeEffect.CreateShader(EtherealSksl, out var errors)
            ?? throw new InvalidOperationException($"Ethereal shader failed to compile: {errors}");

    public override void Update(long tick)
    {
        base.Update(tick);
        ForceRefresh();
    }

    protected override void OnDraw(BackbufferBase backbuffer, RectangleF destRectScreen)
    {
        var canvas = backbuffer.Canvas;
        // ...
        float time = (float)Engine.Instance.TotalSecondsEngineRunning;

        //Advance the attractor smoothing by the frame's real delta (0 on the first frame;
        //  clamped so a stall cannot make the field lurch)
        float delta = _hasLastStepTime ? Math.Clamp(time - _lastStepTime, 0f, 0.1f) : 0f;
        _lastStepTime = time;
        _hasLastStepTime = true;
        _attractorField.Step(delta);
        _attractorField.CopyState(_palmState);

        _uniforms["iTime"] = time;
        _resolutionUniform[0] = w;
        _resolutionUniform[1] = h;
        _uniforms["iResolution"] = _resolutionUniform;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            _palmUniforms[k][0] = ((_palmState[k * 3] * 2f) - 1f) * aspect;
            _palmUniforms[k][1] = (_palmState[(k * 3) + 1] * 2f) - 1f;
            _palmUniforms[k][2] = _palmState[(k * 3) + 2];
            _uniforms[PalmUniformNames[k]] = _palmUniforms[k];
        }
        using (var shader = _effect.ToShader(_uniforms))
        {
            _plasmaPaint.Shader = shader;
            canvas.DrawRect(rect, _plasmaPaint);
            _plasmaPaint.Shader = null;
        }
        // ... then the starfield, drawn with ordinary canvas calls over the shaded rect
    }
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealBackdropTests.cs`

**Sharp edges.**
- The update override forces a refresh every frame. The CPU render path uses dirty
  rectangles and would otherwise stop animating; the GPU path re-renders the whole
  surface anyway.
- Because it is an ordinary direct drawing, on the GPU path the draw runs on the
  GL thread with the graphics context current, so the shader executes on the GPU;
  on the CPU path Skia's raster backend evaluates the same shader.
- The draw allocates nothing: the uniform arrays, the state buffer, both paints
  and the seed arrays are fields created once. Only the shader produced per frame
  is transient, and it is disposed by its `using`.
- Seed any randomness at construction, so an undisturbed frame is a pure function
  of engine time and is reproducible.
- Design the shader so a zeroed parameter set reduces it exactly to the
  undisturbed visual. Every parameterized term is multiplied by its own strength,
  so at zero the scene is the plain background - that is what makes the visual
  melt back instead of snapping.
- Take time from the engine's own running total, not from wall-clock time, so a
  pause does not make the animation jump.
- Setting an unknown uniform name throws, which the test suite turns into a
  guarantee: one test sets every uniform the drawing sets, so a renamed uniform
  fails the test run rather than rendering black.

### Smooth worker rate data into frame rate animation

**When you want this.** Values arrive from a background producer at an irregular
rate and must drive a smooth animation that runs at the render rate, without
snapping when values appear, change or disappear.

**The MVVM shape.** A small thread-safe field object sits between the two. The
producer sets targets from its own thread; the renderer steps the field once per
frame and copies the state into a reusable buffer. Slots are keyed by a stable id
so a moving item keeps its animation state.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractorField.cs
public const int MaxAttractors = 4;
public const float PositionLerpPerSecond = 12f;
public const float StrengthAttackPerSecond = 4f;
public const float StrengthReleasePerSecond = 2.5f;

public void SetTargets(IReadOnlyList<PalmAttractor> palms)
{
    lock (_lock)
    {
        Span<bool> seen = stackalloc bool[MaxAttractors];

        if (palms != null)
        {
            foreach (PalmAttractor palm in palms)
            {
                int slotIndex = FindSlot(palm.Id);
                if (slotIndex < 0) { continue; }

                Slot slot = _slots[slotIndex];
                if (slot.Id != palm.Id)
                {
                    //A freshly claimed slot starts AT the palm with no strength, so the
                    //  influence swells in place instead of sweeping over from stale state
                    slot.Id = palm.Id;
                    slot.X = palm.X;
                    slot.Y = palm.Y;
                    slot.Strength = 0f;
                }
                slot.TargetX = palm.X;
                slot.TargetY = palm.Y;
                slot.TargetStrength = 1f;
                seen[slotIndex] = true;
            }
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (!seen[i] && _slots[i].Id != 0)
            {
                _slots[i].TargetStrength = 0f;
            }
        }
    }
}

public void Step(float deltaSeconds)
{
    if (deltaSeconds <= 0f) { return; }

    //Exponential approach: framerate-independent, and never overshoots
    float positionBlend = 1f - (float)Math.Exp(-PositionLerpPerSecond * deltaSeconds);
    float attackBlend = 1f - (float)Math.Exp(-StrengthAttackPerSecond * deltaSeconds);
    float releaseBlend = 1f - (float)Math.Exp(-StrengthReleasePerSecond * deltaSeconds);

    lock (_lock)
    {
        foreach (Slot slot in _slots)
        {
            if (slot.Id == 0) { continue; }

            slot.X += (slot.TargetX - slot.X) * positionBlend;
            slot.Y += (slot.TargetY - slot.Y) * positionBlend;

            float strengthBlend = slot.TargetStrength > slot.Strength ? attackBlend : releaseBlend;
            slot.Strength += (slot.TargetStrength - slot.Strength) * strengthBlend;

            if (slot.TargetStrength <= 0f && slot.Strength < FreeSlotStrength)
            {
                slot.Id = 0;
                slot.Strength = 0f;
            }
        }
    }
}

//Returns the index of the slot already owned by this id, else a free slot, else -1.
//  A slot mid-fade keeps its id, so a returning palm re-attaches instead of popping.
private int FindSlot(int id)
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractorField.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmAttractorFieldTests.cs`

**Sharp edges.**
- An exponential approach rather than a linear step is framerate-independent and
  never overshoots, so a long frame cannot make the animation lurch past its
  target.
- Attack and release use different rates, so appearing feels different from
  disappearing.
- A newly claimed slot is placed at the incoming position with zero strength, so
  the influence swells in place rather than sweeping in from wherever the slot was
  last used.
- A slot that is fading out keeps its id until its strength falls below a small
  epsilon, so an item that reappears re-attaches to its own fade instead of
  restarting. There is a test named for exactly that.
- Capacity is fixed and extras are dropped rather than queued.
- The copy-out method writes into a caller-owned buffer and validates its length,
  which is what lets the renderer stay allocation-free.
- The class documents that all members are thread-safe, and that claim is what
  lets the view model feed it straight from a worker thread without marshalling.

### Keep a pipeline and a renderer decoupled by a normalized seam

**When you want this.** Two libraries have to cooperate every frame, and you do
not want either one to depend on the other's concepts.

**The MVVM shape.** Define the narrowest possible value type that describes what
the renderer needs, put it in the rendering library, and let the view model
translate.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractor.cs
/// <summary>
/// One open palm the visualization should be drawn toward, in normalized screen
/// coordinates. This is the whole seam between the vision pipeline and the rendering:
/// the consumer maps whatever it tracks into these points (mirroring the X coordinate
/// when the user watches a mirror-style view) and hands them to
/// <see cref="VisualizerSession.UpdatePalms"/>.
/// </summary>
public readonly struct PalmAttractor
{
    public PalmAttractor(int id, float x, float y)
    {
        Id = id;
        X = x;
        Y = y;
    }

    /// <summary>
    /// A stable identifier for the palm. The same physical hand should keep the same id
    /// from update to update so its glow follows it instead of re-fading in.
    /// </summary>
    public int Id { get; }

    /// <summary>The palm's horizontal position, 0 (left) .. 1 (right) across the visual.</summary>
    public float X { get; }

    /// <summary>The palm's vertical position, 0 (top) .. 1 (bottom) down the visual.</summary>
    public float Y { get; }
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
public void UpdatePalms(IReadOnlyList<PalmAttractor> palms) => _attractorField.SetTargets(palms);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractor.cs`
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`

**Sharp edges.**
- Normalized coordinates, not pixels, so a window resize needs no re-mapping
  anywhere.
- The stable id travels across the seam. That single field is what lets the
  renderer's easing follow a moving item instead of restarting.
- The seam type carries no confidence scores, no state flag and no landmarks: the
  view model filters before translating, so "what counts" stays an application
  policy rather than a rendering one.
- The documentation names the caller's responsibility - mirroring - explicitly,
  which is how a coordinate-convention pitfall stays documented at the boundary
  where it matters.

### Offer a CPU fallback for a GPU rendering path behind one switch

**When you want this.** Your visual runs on the GPU by default but must be
runnable on the CPU path for a head, a machine or a test that cannot use one, and
you want the two to be the same scene rather than two code paths.

**The MVVM shape.** One property on the canvas, set once before the surface is
touched, chosen from an environment variable so no rebuild is needed. The drawing
code is identical either way.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
/// Set the environment variable <c>PALMVISUALIZER_USE_CPU=1</c> to run the identical scene
/// on the CpuRendering (CPU) render path.

// ...

//GpuRendering-OpenGL (GPU) by default; must be chosen before the first access to Host. The
//  render resolution tracks the window (no SetRenderResolution) - the shader
//  scene is resolution-independent.
_canvas.UseGpuRendering = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1";
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealBackdropTests.cs`

**Sharp edges.**
- The choice must be made before the canvas's host is read for the first time.
- The default is the negative test, so an unset or garbage value keeps the GPU
  path.
- The two paths differ in one respect the drawing has to handle: the CPU path uses
  dirty rectangles, which is why the update override forces a refresh
  unconditionally.
- Because the fallback is Skia's raster backend, the same shader can be tested
  headlessly on a raster surface with no engine and no window, which is exactly
  what the shader tests do.

### Choose the render resolution from the zoom level

**When you want this.** Zooming in should sharpen content, not just scale up a
blurry bitmap, but you do not want to render a poster-sized image at the top of
the ladder.

**The MVVM shape.** A small value type in the library owns the zoom ladder and the
resolution rule. The view model asks it for a resolution and passes that to the
renderer; nothing about resolution lives in the page.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ViewZoom.cs
    /// <summary>The fit-the-page level; also the minimum.</summary>
    public const int MinimumPercent = 100;

    /// <summary>The highest resolution a page is ever rendered at, whatever the zoom.</summary>
    public const int MaximumRenderDpi = 600;

    /// <summary>The zoom levels, in order, that ZoomIn and ZoomOut step through.</summary>
    public static IReadOnlyList<int> Levels { get; } = [100, 125, 150, 200, 300, 400, 500, 700, 1000];

    /// <summary>The current level as a multiplier of the fit-the-page size (1.0 at 100%).</summary>
    public double Factor => Percent / 100.0;

    /// <summary>
    /// The resolution to render a page at for the current level: baseDpi scaled by the
    /// zoom factor so text stays sharp, capped at MaximumRenderDpi (past the cap
    /// the image is scaled up a little on screen instead of rendering an enormous bitmap).
    /// </summary>
    public int GetRenderDpi(int baseDpi)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(baseDpi, 1);
        return (int)Math.Min(Math.Round(baseDpi * Factor), MaximumRenderDpi);
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
            var dpi = View.Zoom.GetRenderDpi(_renderer.Dpi);
            var page = await _renderer.RenderCurrentPageAsync(document, dpi, cts.Token);
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ViewZoom.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Here 100 percent means "the whole page fits the pane", not "actual size", and it
  is the minimum. The user can never zoom out past fit, so there is no empty
  border state to design for.
- Pass the resolution per call, so the renderer's own default keeps acting as the
  multiplier's base. Using the property setter instead would clear the cache on
  every zoom step.
- The resolution cap means the on-screen image at the top of the ladder is scaled
  up. That is a deliberate trade, and the comment says so.

### Draw a zoomable document canvas on an SKXamlCanvas subclass

**When you want this.** Any document editor - a drawing surface, a map, a score, a
diagram - that has to composite content and repaint fast.

**The MVVM shape.** The canvas control is a view. It implements an interface the
document model owns, so the model can ask for a repaint or set a cursor without
referencing any UI type, and it subscribes to the model's invalidation events.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
public sealed class PintaCanvas : SKXamlCanvas, ICanvasView
{
    private Document? document;
    private CanvasRenderer? renderer;
    private Drawing.ImageSurface? canvas_surface;
    private RectangleI? pending_dirty; // union of invalidated canvas rects; null = everything
    private bool surface_stale = true;

    public PintaCanvas ()
    {
        PaintSurface += OnPaintSurface;
        IsTabStop = true;

        PointerPressed += OnCanvasPointerPressed;
        PointerMoved += OnCanvasPointerMoved;
        PointerReleased += OnCanvasPointerReleased;
        PointerWheelChanged += OnCanvasPointerWheelChanged;
        KeyDown += OnCanvasKeyDown;
        KeyUp += OnCanvasKeyUp;
        // ...
    }

    private void OnPaintSurface (object? sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear (SKColors.Transparent);

        if (document is null || renderer is null)
            return;

        Size imageSize = document.ImageSize;
        Size viewSize = document.Workspace.ViewSize;

        // 1. Refresh the unscaled composite of all layers.
        canvas_surface ??= new Drawing.ImageSurface (Drawing.Format.Argb32, imageSize.Width, imageSize.Height);
        // ...
        // 3. The composite, scaled for zoom: nearest-neighbor when zoomed in,
        //    linear when zoomed out (matching upstream).
        double scale = document.Workspace.Scale;
        SKSamplingOptions sampling = scale >= 1
            ? new SKSamplingOptions (SKFilterMode.Nearest)
            : new SKSamplingOptions (SKFilterMode.Linear);
        canvas.DrawBitmap (
            canvas_surface.Bitmap,
            new SKRect (0, 0, viewSize.Width, viewSize.Height),
            sampling);
        // ...
    }
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs`

**Sharp edges.**
- Zoom is applied by resizing the element and scaling at draw time, not by a
  transform on the scroll viewer; the scroll viewer only ever pans.
- Nearest-neighbor above one-to-one and linear below is what keeps a zoomed-in
  pixel editor from looking blurred.
- `IsTabStop = true` is needed or the canvas never receives key events.

### Repaint only the dirty rectangle of a cached composite

**When you want this.** Your document is expensive to composite and most edits
touch a small region.

**The MVVM shape.** The document model raises an invalidation event carrying
either a rectangle or an "entire surface" flag; the view accumulates the union
until the next paint and re-composites only that.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
private void OnCanvasInvalidated (object? sender, CanvasInvalidatedEventArgs e)
{
    if (e.EntireSurface || pending_dirty is null && surface_stale) {
        surface_stale = true;
        pending_dirty = null;
    } else if (pending_dirty is { } dirty) {
        pending_dirty = dirty.Union (e.Rectangle);
    } else {
        pending_dirty = e.Rectangle;
    }

    Invalidate ();
}

/// <summary>
/// A selection change alters only the overlay pass, not the layer
/// composite, so the cached surface is left intact and just the overlay
/// is repainted. Without this the marching ants and the selection tools'
/// handles never appear, because changing a selection dirties no pixels
/// and therefore never raises CanvasInvalidated.
/// </summary>
private void OnSelectionChanged (object? sender, EventArgs e)
    => Invalidate ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/EventArgs/CanvasInvalidatedEventArgs.cs`

**Sharp edges.**
- Overlay-only changes dirty no pixels, so they need their own invalidate path or
  they never appear. The comment in the sample records that exact bug.
- Null is used to mean "everything", so the union logic has to check for it before
  unioning.

### Animate an overlay with a timer that stops when unloaded

**When you want this.** A continuously animated overlay - a marching-ants
selection, a caret, a spinner - that must not keep running after its view is gone.

**The MVVM shape.** The timer belongs to the control that draws the animation, and
is started and stopped from the loaded and unloaded events so a closed document's
canvas stops ticking. Animation state that no one else reads stays private to the
view.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
//Marching ants: the dash offset ticks backwards while a selection is
//visible (upstream ticks -1 per timer fire), wrapping at the dash
//pattern's period. The redraw is display-only.
private readonly Microsoft.UI.Xaml.DispatcherTimer ants_timer = new () {
    Interval = TimeSpan.FromMilliseconds (100),
};
private float ants_offset;

// ... in the constructor:
ants_timer.Tick += (_, _) => {
    if (document is null || !document.Selection.Visible || document.Selection.SelectionPolygons.Count == 0)
        return;
    ants_offset -= 1;
    if (ants_offset < 0)
        ants_offset += 8; // dash pattern period: 4 on + 4 off
    Invalidate ();
};

//The timer only runs while the canvas is in the tree, so closed
//documents do not keep ticking.
Loaded += (_, _) => ants_timer.Start ();
Unloaded += (_, _) => ants_timer.Stop ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`

**Sharp edges.**
- Without the unloaded stop, every document ever opened keeps a timer running for
  the life of the process.
- The tick returns early when there is nothing to animate, so the timer costs
  nothing while the overlay is not visible.

### Host a canvas in a scroll viewer and drive zoom and scroll from an interface

**When you want this.** Your document is larger than its viewport and the model,
not the view, decides where the viewport should be after a zoom.

**The MVVM shape.** The scrollable host implements an interface the model owns:
viewport size, scroll offset, a layout pump and focus. The model computes the new
offset; the view just applies it, and never sees a scroll viewer.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs
/// <summary>
/// The scrollable view hosting a document's canvas: viewport metrics and the
/// scroll position, in view (zoomed) coordinates.
/// </summary>
public interface ICanvasScrollView
{
	Size ViewportSize { get; }

	PointD ScrollOffset { get; set; }

	/// <summary>
	/// Ensures scroll extents reflect the current view size before scroll
	/// offsets are adjusted (replaces the upstream main-loop pump).
	/// </summary>
	void UpdateLayout ();

	/// <summary>Move keyboard focus to the canvas view. Returns whether focus was gained.</summary>
	bool GrabFocus ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvasView.cs
public Size ViewportSize =>
    new (
        (int) Math.Max (scroller.ViewportWidth > 0 ? scroller.ViewportWidth : scroller.ActualWidth, 0),
        (int) Math.Max (scroller.ViewportHeight > 0 ? scroller.ViewportHeight : scroller.ActualHeight, 0));

public PointD ScrollOffset {
    get => new (scroller.HorizontalOffset, scroller.VerticalOffset);
    set => scroller.ChangeView (value.X, value.Y, null, disableAnimation: true);
}

public new void UpdateLayout ()
{
    Canvas.UpdateLayout ();
    scroller.UpdateLayout ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentWorkspace.cs
private void ZoomToScaleAroundViewPoint (double newScale, PointD center_point)
{
    PointD offset = CanvasWindow?.ScrollOffset ?? new PointD (0, 0);

    double scroll_offset_x = center_point.X - offset.X;
    double scroll_offset_y = center_point.Y - offset.Y;

    PointD canvas_point = ViewPointToCanvas (center_point);

    Scale = Math.Min (newScale, 36.0);

    // Make sure the scroll extents match the new view size before
    // recentering. (Upstream pumped the GTK main loop here.)
    CanvasWindow?.UpdateLayout ();

    // Scroll so that the canvas position under 'center_point' is still the same after zooming.
    PointD new_center_point = CanvasPointToView (canvas_point);
    if (CanvasWindow is { } view) {
        view.ScrollOffset = new PointD (
            new_center_point.X - scroll_offset_x,
            new_center_point.Y - scroll_offset_y);
    }
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvasView.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentWorkspace.cs`

**Sharp edges.**
- The layout pump on the interface exists purely as an ordering hook: the scroll
  extents must reflect the new element size before a new offset is set, or the
  offset gets clamped against stale extents.
- Viewport width can be zero before the first layout pass, hence the fallback to
  the actual width.
- The host's layout method is declared `new` because the base class already has
  one; the interface implementation shadows it deliberately.
- The scroll viewer's own zoom is disabled: all zoom is the model's.

### Scale a Skia drawn control from surface pixels to logical units

**When you want this.** Any Skia canvas whose drawing code is written in the
element's own coordinates and must look right on a scaled display.

**The MVVM shape.** One line in the paint handler. It is a view concern entirely.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/HistogramWidget.cs
private void OnPaintSurface (object? sender, SKPaintSurfaceEventArgs e)
{
    SKCanvas canvas = e.Surface.Canvas;
    canvas.Clear (SKColors.Transparent);

    if (ActualWidth <= 0 || ActualHeight <= 0)
        return;

    //The surface is physical pixels; draw in the element's logical space.
    canvas.Scale (e.Info.Width / (float) ActualWidth, e.Info.Height / (float) ActualHeight);

    SKRect rect = SKRect.Create (0, 0, (float) ActualWidth, (float) ActualHeight);
    // ... draw in logical units from here on
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/HistogramWidget.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/ColorGradientWidget.cs`

**Sharp edges.**
- Guard the actual size against zero before dividing; a paint can arrive before
  the first layout pass.
- The gradient widget takes the opposite approach and computes everything from the
  actual size directly, so the two show both options side by side.

### Turn raw pixel surfaces into XAML image sources

**When you want this.** You have raw premultiplied pixels - from a decoder, a
renderer, a thumbnail - and need them in an `Image` element.

**The MVVM shape.** A static factory returning an image source, caching by key.
Returning null for an unknown key lets callers fall back to a text label rather
than showing an empty square.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/IconImageSource.cs
public static ImageSource? Create (string iconName, int size)
{
	if (cache.TryGetValue ((iconName, size), out ImageSource? cached))
		return cached;

	// An unknown name must come back null so callers can fall back to a
	// label rather than rendering a blank square.
	if (!PintaCore.Resources.HasIcon (iconName))
		return null;

	ImageSurface surface = PintaCore.Resources.GetIcon (iconName, size);
	byte[] pixels = surface.GetData ().ToArray ();

	WriteableBitmap bitmap = new (surface.Width, surface.Height);
	pixels.CopyTo (bitmap.PixelBuffer);
	bitmap.Invalidate ();

	cache[(iconName, size)] = bitmap;
	return bitmap;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/IconImageSource.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/LayerRowFactory.cs`

**Sharp edges.**
- Copying into the bitmap's pixel buffer and then invalidating is the platform's
  raw-buffer idiom; the file's header comment says it exists to avoid a per-icon
  stream wrapper.
- The pixel layout must already be premultiplied in the order the bitmap expects;
  there is no conversion step here.
- The same idiom serves live thumbnails, letterboxed rather than stretched so a
  tall or wide source keeps its shape.

### Honor EXIF orientation when decoding with SkiaSharp codecs

**When you want this.** You decode photographs and want them upright, the way
every other image viewer shows them.

**The MVVM shape.** Decoding is a service concern. The importer reads the encoded
origin from the codec, swaps the output dimensions when the origin is transposed,
and draws through the matching matrix.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/SkiaCodecFormat.cs
SKEncodedOrigin origin = codec.EncodedOrigin;
// ...
// EXIF origins 5-8 are transposed: the upright image swaps width/height.
bool swapsDimensions = origin is
	SKEncodedOrigin.LeftTop or
	SKEncodedOrigin.RightTop or
	SKEncodedOrigin.RightBottom or
	SKEncodedOrigin.LeftBottom;

Size imageSize =
	swapsDimensions
	? new (decoded.Height, decoded.Width)
	: new (decoded.Width, decoded.Height);
// ...
using (SKCanvas canvas = new (layer.Surface.Bitmap)) {
	canvas.SetMatrix (GetOriginMatrix (origin, imageSize.Width, imageSize.Height));
	canvas.DrawBitmap (decoded, 0, 0, SKSamplingOptions.Default, paint: null);
}

/// <summary>
/// Returns the transform that maps decoded (encoded-orientation) pixels to
/// the upright image, mirroring Skia's SkEncodedOriginToMatrix. The width
/// and height are the dimensions of the upright output image.
/// </summary>
private static SKMatrix GetOriginMatrix (SKEncodedOrigin origin, int w, int h)
	=> origin switch {
		SKEncodedOrigin.TopRight => new (-1, 0, w, 0, 1, 0, 0, 0, 1),
		SKEncodedOrigin.BottomRight => new (-1, 0, w, 0, -1, h, 0, 0, 1),
		SKEncodedOrigin.BottomLeft => new (1, 0, 0, 0, -1, h, 0, 0, 1),
		SKEncodedOrigin.LeftTop => new (0, 1, 0, 1, 0, 0, 0, 0, 1),
		SKEncodedOrigin.RightTop => new (0, -1, w, 1, 0, 0, 0, 0, 1),
		SKEncodedOrigin.RightBottom => new (0, -1, w, -1, 0, h, 0, 0, 1),
		SKEncodedOrigin.LeftBottom => new (0, 1, 0, -1, 0, h, 0, 0, 1),
		_ => SKMatrix.Identity, // TopLeft (default)
	};
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/SkiaCodecFormat.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.FileFormats.Tests/SkiaCodecFormatTests.cs`

**Sharp edges.**
- Four of the eight origins transpose the image, so the destination surface has to
  be allocated with swapped dimensions before drawing.
- The matrix arguments are the upright output dimensions, not the decoded ones.
- Decoding directly into the target color and alpha types avoids a format
  conversion after the fact.

### Combine selection polygons with the CodeBrix PolygonTools library

**When you want this.** Boolean geometry - union, difference, intersection,
exclusion - on user-drawn regions.

**The MVVM shape.** The document's selection object owns a clipper instance and
exposes combine operations; the tools call those. No UI type is involved and the
whole thing is unit-tested headless.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentSelection.cs
using CodeBrix.PolygonTools;
using CodeBrix.PolygonTools.Enumerations;
using CodeBrix.PolygonTools.Models;
// ...
public PolyClip SelectionClipper { get; } = new ();
// ...
SelectionClipper.AddPath (documentPolygon, PolyType.ptSubject, true);
SelectionClipper.AddPaths (SelectionPolygons, PolyType.ptClip, true);
SelectionClipper.Execute (ClipType.ctDifference, resultingPolygons);
SelectionClipper.Clear ();
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/SelectionModeHandler.cs
//Specify the Clipper Subject (the previous Polygons) and the Clipper Clip (the new Polygons).
//Note: for Union, ignore the Clipper Library instructions - the new polygon(s) should be Clips, not Subjects!
doc.Selection.SelectionClipper.AddPaths (doc.Selection.SelectionPolygons, PolyType.ptSubject, true);
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentSelection.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/SelectionModeHandler.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/SelectionModeHandlerTests.cs`

**Sharp edges.**
- The subject and clip assignment for a union is the opposite of what the
  library's own documentation suggests; the code comment says so and the tests pin
  it.
- Clear after every execute, or the next operation inherits the previous paths.
- Coordinates are integer paths, so the engine converts its own point type in both
  directions around every call.

### Give a headless library a drawing facade over SkiaSharp

**When you want this.** A large body of drawing code, ported or original, that you
want to keep independent of any one graphics API, or that expects an
immediate-mode vector API.

**The MVVM shape.** A small drawing namespace in the model library - context,
surface, path, pattern, matrix, region. Everything above draws through it; only
the facade knows SkiaSharp.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/ImageSurface.cs
// Pinta.Brix drawing-layer bitmap surface: a premultiplied-BGRA32 pixel
// buffer with span access, mirroring the surface API the upstream Pinta code
// used. Backed by an SKBitmap so drawing Contexts and SkiaSharp interop share
// the same pixel memory with no copies.

public ImageSurface (Format format, int width, int height)
{
	if (width < 0 || height < 0)
		throw new ArgumentOutOfRangeException (nameof (width), "Surface dimensions must be non-negative");

	Format = format;

	// The engine always draws in premultiplied BGRA32; an A8 surface is
	// stored in the same layout for simplicity (alpha in every channel).
	bitmap = new SKBitmap (new SKImageInfo (
		Math.Max (width, 1),
		Math.Max (height, 1),
		SKColorType.Bgra8888,
		SKAlphaType.Premul));
	bitmap.Erase (SKColors.Transparent);

	Width = width;
	Height = height;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Context.cs
// Fidelity notes:
// - Path coordinates are transformed by the current matrix at verb time and
//   stored in device space, matching the original API's semantics.
// - Arcs are flattened to cubic splines in user space before transforming,
//   so they remain correct under any current matrix.
// - Stroke width and dashes are scaled by the current matrix's mean scale
//   factor; non-uniform-scale stroking (elliptical pens) is approximated.
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/ImageSurface.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Context.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Path.cs`

**Sharp edges.**
- Sharing the bitmap's pixel memory means every path that writes pixels directly
  must mark the bitmap dirty afterwards, and every path that reads after drawing
  must flush first.
- A zero-size surface is coerced to one pixel internally while the reported size
  stays as requested, so callers do not have to special-case empty documents.
- Immediate-mode semantics differ from a retained scene graph in ways worth
  documenting in the file itself: device-space path storage, arc flattening, and
  the scale factor used for stroke widths.

### Play a Lottie animation on a Skia head and on native WinUI

**When you want this.** A small looping vector animation, for example on a button,
that looks the same on the Skia heads and in a native WinUI 3 application.

**The MVVM shape.** Pure view concern. The animated player wraps a Lottie source
inside whatever container you want; if the animation sits on a button, the
button's `Command` still binds to the view model.

**Code.**

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<Page ...
      xmlns:lottie="clr-namespace:CommunityToolkit.WinUI.Lottie;assembly=CodeBrix.Platform.UI.Lottie">
  <!-- ... -->
  <Button Grid.Row="4" Grid.Column="1" Width="80" Height="50"
          VerticalAlignment="Center" HorizontalAlignment="Left"
          Command="{d:Binding ShowOsInfoCommand}">
      <StackPanel Orientation="Horizontal">
          <AnimatedVisualPlayer AutoPlay="True"
                                HorizontalAlignment="Center"
                                VerticalAlignment="Center"
                                Height="40" Width="50">
              <!--NOTE: Visual Studio doesn't recognize 'embedded:' and the path below, but they are correct and will work fine-->
              <lottie:LottieVisualSource UriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.star_icon.json" />
          </AnimatedVisualPlayer>
      </StackPanel>
  </Button>
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml -->
<Page ...
      xmlns:lottie="using:CodeBrix.Platform.WinUI.Lottie">
  <!-- ... -->
  <lottie:AnimatedVisualPlayer AutoPlay="True"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               Height="40" Width="50">
      <lottie:LottieVisualSource UriSource="ms-appx:///Assets/star_icon.json" />
  </lottie:AnimatedVisualPlayer>
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`
`JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml`
`JustBetweenUs/Shared/Assets/star_icon.json`

**Sharp edges.**
- On the Skia heads the player comes from the default XML namespace and only the
  source needs a prefix; on native WinUI 3 both are prefixed, and the namespace
  declarations differ in form as well.
- The same animation file is loaded by the custom embedded-resource URI on the
  Skia heads and by a packaged content URI on WinUI, because the WinUI project
  ships it as content.
- The Lottie player on the Skia heads needs the Lottie add-in and its animation
  library, both referenced from the library that carries the application's
  packages.

