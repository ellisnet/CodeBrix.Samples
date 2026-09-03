# PolyHavenBrowser_viewer_only

PolyHavenBrowser_viewer_only is a single-page CodeBrix.Platform application that shows three
fixed sample assets from Poly Haven, one at a time, on one shared Skia canvas: a PBR texture
(wrapped on a lit, orbitable cube over a darkened backdrop of the same texture), an HDRI
environment (an interactive drag-to-look equirectangular panorama), and a glTF 3D model. Three
buttons across the top switch modes and the active one carries the accent style. Beside them a
**Rendering engine** dropdown picks the 3D graphics backend while the application is running;
it is hidden in HDRI mode, because the panorama is drawn on the CPU and is unaffected by the
choice. Dragging orbits the cube or the model and looks around the panorama, the scroll wheel
zooms, an indeterminate progress bar shows while an asset downloads, and a status line under the
canvas reports what is loaded and how to drive it. Each sample is downloaded on demand and cached
per user under `LocalApplicationData/PolyHavenBrowser/cache`, keyed by the curated asset slug, so
after the first run every sample loads with no network.

It is the reference application for **rendering 3D content off screen with a swappable graphics
API and compositing the result onto an `SKXamlCanvas` in ordinary XAML**: one interface,
`IModelRenderEngine`, with three independent implementations behind it (OpenGL through
Graphics3DGL's off-screen context, Vulkan through a self-contained Silk.NET renderer, and Metal
through the raw Objective-C runtime), each gated to the heads that can run it. Secondarily it is
the reference for the "application that carries extra library assemblies" layout, `src/libs` plus
`tests/libs`. The architecture of the rendering layer has its own in-repo document,
[`src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`](src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md),
which is the place to start if you intend to copy the pipeline. Its companion in this repository
is [PolyHavenBrowser](../PolyHavenBrowser/README.md), the catalog application that browses the
whole Poly Haven library instead of three curated samples.

## What this sample shows a CodeBrix.Platform developer

- Putting one interface between the application and the graphics API, so a dropdown can change
  the backend at run time while the painter, camera, loaders and XAML above it stay unchanged:
  [Swap the 3D graphics backend at run time from a dropdown](../BLUEPRINTS.md#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown).
- Rendering a GPU scene into an off-screen framebuffer and drawing the pixels onto an
  `SKXamlCanvas` that sits in a normal XAML page:
  [Render an OpenGL scene off screen and composite it onto an SKXamlCanvas](../BLUEPRINTS.md#render-an-opengl-scene-off-screen-and-composite-it-onto-an-skxamlcanvas).
- Letting each backend declare the vertical orientation of its readback so one compositing path
  is correct for all of them:
  [Composite engine pixels onto Skia with the right vertical orientation](../BLUEPRINTS.md#composite-engine-pixels-onto-skia-with-the-right-vertical-orientation).
- Adding a Vulkan backend that owns its whole stack and embeds pre-compiled SPIR-V, so no shader
  compiler is a build or run-time prerequisite:
  [Add a self contained Vulkan renderer that needs no shader toolchain](../BLUEPRINTS.md#add-a-self-contained-vulkan-renderer-that-needs-no-shader-toolchain).
- Adding a macOS backend by P/Invoking the Objective-C runtime directly, with no managed Apple
  bindings and no package of any kind:
  [Add a direct to Metal renderer with no NuGet package or Apple bindings](../BLUEPRINTS.md#add-a-direct-to-metal-renderer-with-no-nuget-package-or-apple-bindings).
- Deciding which heads an optional capability is offered on with a hard-coded, testable policy
  list instead of a driver probe:
  [Gate an optional graphics backend to specific heads with an allow list](../BLUEPRINTS.md#gate-an-optional-graphics-backend-to-specific-heads-with-an-allow-list).
- Letting a headless library work out which of the six heads is hosting it without referencing
  any of them:
  [Detect which platform head is running without referencing it](../BLUEPRINTS.md#detect-which-platform-head-is-running-without-referencing-it).
- Writing the camera math once, in `System.Numerics`, and feeding the same matrices to GLSL,
  SPIR-V and MSL without an extra transpose:
  [Share one camera and one matrix convention across graphics APIs](../BLUEPRINTS.md#share-one-camera-and-one-matrix-convention-across-graphics-apis).
- Producing an interactive view entirely on the CPU, for a head with no GPU or as a fallback
  path:
  [Paint a CPU ray traced panorama into an SKBitmap](../BLUEPRINTS.md#paint-a-cpu-ray-traced-panorama-into-an-skbitmap).
- Turning a flat texture into a lit, orbitable solid so it reads as a material rather than a
  swatch:
  [Build a textured cube mesh from a bitmap for previewing a flat material](../BLUEPRINTS.md#build-a-textured-cube-mesh-from-a-bitmap-for-previewing-a-flat-material).
- Previewing glass in a glTF model with a second, depth-write-off pass instead of implementing
  real transmission:
  [Draw translucent surfaces in a second pass with depth writes off](../BLUEPRINTS.md#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off).
- Decoding OpenEXR and Radiance HDR content and tone mapping it into something a canvas can
  show:
  [Decode HDR images and tone map them for display](../BLUEPRINTS.md#decode-hdr-images-and-tone-map-them-for-display).
- Decoding downloaded JPEG, PNG and WebP maps into Skia bitmaps or raw RGBA through
  CodeBrix.Imaging:
  [Decode raster images with the CodeBrix Imaging library into a Skia bitmap](../BLUEPRINTS.md#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap).
- Offering every option in a picker and explaining, with a dialog, why one of them cannot run
  here, then putting the selection back:
  [Alert and revert when the user picks an unsupported option](../BLUEPRINTS.md#alert-and-revert-when-the-user-picks-an-unsupported-option).
- Proving a new GPU backend works on a worker thread before it is ever handed to a paint
  callback:
  [Pre warm a rendering backend off the UI thread](../BLUEPRINTS.md#pre-warm-a-rendering-backend-off-the-ui-thread).
- Keeping at most one repaint queued and discarding pointer frames that have fallen behind, so
  an expensive canvas stays responsive:
  [Coalesce repaints and drop backlogged pointer frames](../BLUEPRINTS.md#coalesce-repaints-and-drop-backlogged-pointer-frames).
- Converting pointer positions from view units into canvas pixels and forwarding them to the
  object being drawn:
  [Forward pointer input from a canvas into a model](../BLUEPRINTS.md#forward-pointer-input-from-a-canvas-into-a-model).
- Giving the view model a way to repaint the canvas without holding a reference to the page or
  to any control:
  [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- Handing the view model a `XamlRoot` accessor so it can show a `SimpleDialog` alert and so a
  native context can be created from it later, on the right thread:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Awaiting a network call and then decoding or mesh-building inside `Task.Run()` so the UI
  thread never blocks:
  [Do blocking work in a service behind Task Run](../BLUEPRINTS.md#do-blocking-work-in-a-service-behind-task-run).
- Driving a progress bar and a status line from bound state while a command downloads an asset:
  [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- Writing bound properties with the C# `field` keyword, `SimpleCommand` commands with a
  `CanExecute` lambda, and `[AffectsCommands]` to refresh them:
  [Write bound properties and commands the family way](../BLUEPRINTS.md#write-bound-properties-and-commands-the-family-way).
- Showing and hiding the progress bar and the engine dropdown from computed properties the view
  model raises by hand:
  [Show and hide panes with computed Visibility properties](../BLUEPRINTS.md#show-and-hide-panes-with-computed-visibility-properties).
- Catching a failed download, decode or engine switch and writing it to the bound status line
  instead of letting it escape:
  [Report a failure as status text instead of throwing](../BLUEPRINTS.md#report-a-failure-as-status-text-instead-of-throwing).
- Making a row of buttons behave like a radio group by binding `Style` through an
  `IValueConverter`:
  [Highlight the selected button with a value converter](../BLUEPRINTS.md#highlight-the-selected-button-with-a-value-converter).
- Caching a downloaded asset under `LocalApplicationData` with a marker that records the key, so
  re-curating the sample invalidates it:
  [Cache downloaded assets with a key you can invalidate](../BLUEPRINTS.md#cache-downloaded-assets-with-a-key-you-can-invalidate).
- Downloading a glTF together with the side-car files it references by relative path:
  [Report true byte progress across a multi file download with side car files](../BLUEPRINTS.md#report-true-byte-progress-across-a-multi-file-download-with-side-car-files).
- Building a typed REST client with source-generated JSON, its own exception hierarchy and an
  `AddXxx` registration that configures `HttpClient` correctly:
  [Build a typed REST client with source generated JSON and its own exceptions](../BLUEPRINTS.md#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions).
- Registering the application's services in one extension method on `IServiceCollection`:
  [Register library services with one AddXxx extension method](../BLUEPRINTS.md#register-library-services-with-one-addxxx-extension-method).
- Supplying the generic host builder that `SimpleServiceResolver` builds the container from:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Ordering the `App` constructor: default font, container, `SetIsDesignMode(false)`, then
  `InitializeComponent()`:
  [Bootstrap the application in the App constructor](../BLUEPRINTS.md#bootstrap-the-application-in-the-app-constructor).
- Writing the one-screen `Program.Main` each head needs, differing only in its `Use…()` call:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Setting the software render surface on the WinWpfSkia head after `Build()`:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Making the bundled Roboto font the application's default text font in code and a `FontFamily`
  resource in XAML:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Laying an application out as a shared UI project, a Core library, six heads, and reusable
  assemblies under `src/libs` with mirrored tests under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Setting `RootNamespace` on the Core library so XAML can reach its types with a namespace that
  differs from the assembly name:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Referencing Graphics3DGL and letting the `GL` binding arrive transitively, rather than
  referencing the binding package directly:
  [Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively).
- Writing one test suite per backend, mirrored test for test, so all three prove the same
  behaviors:
  [Prove every graphics backend with the same mirrored suite](../BLUEPRINTS.md#prove-every-graphics-backend-with-the-same-mirrored-suite).
- Creating a real GL context with no window system so GPU code can be exercised on a build
  machine:
  [Test GL code headlessly with a surfaceless EGL context](../BLUEPRINTS.md#test-gl-code-headlessly-with-a-surfaceless-egl-context).
- Standing in for a renderer or an API client with CodeBrix.TestMocks, using the interfaces the
  production code already has:
  [Mock a rendering or API seam with CodeBrix TestMocks](../BLUEPRINTS.md#mock-a-rendering-or-api-seam-with-codebrix-testmocks).
- Testing an HTTP client offline against a stub message handler that records requests and 404s
  anything unrouted:
  [Test an HTTP client offline with a stub handler](../BLUEPRINTS.md#test-an-http-client-offline-with-a-stub-handler).
- Keeping the tests that really hit the API out of the default run with a trait and a shared
  fixture:
  [Make live tests opt in and keep them out of the default run](../BLUEPRINTS.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).
- Generating the binary fixtures the tests need in memory instead of committing them:
  [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).

## Building, running and testing

There is one solution, `PolyHavenBrowser.slnx`, and it opens on Linux, macOS and Windows. It
holds the shared UI project, the Core library, all six heads, and, under the solution folders
`Libraries/` and `Tests/`, the two side libraries and their two test projects. This is a pure
CodeBrix.Platform application: there are no native WinUI 3, WPF or .NET MAUI heads, so there is
no second Windows-only solution.

| Head project | Platform / windowing |
| --- | --- |
| `src/PolyHavenBrowser.Win32Skia` | Windows, native Win32 window |
| `src/PolyHavenBrowser.WinWpfSkia` | Windows, Skia hosted in WPF (`net10.0-windows`) |
| `src/PolyHavenBrowser.LinuxX11` | Linux desktop, X11 |
| `src/PolyHavenBrowser.LinuxWayland` | Linux desktop, native Wayland |
| `src/PolyHavenBrowser.LinuxFrameBuffer` | Linux framebuffer |
| `src/PolyHavenBrowser.MacOS` | macOS |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets `net10.0-windows`
  and sets `EnableWindowsTargeting`, so the whole solution still restores and builds on Linux and
  macOS.
- No workloads, accounts or API tokens. The Poly Haven API is public; the client sends a
  User-Agent that identifies the application, which Poly Haven asks consumers to do, and it is
  configured in `src/PolyHavenBrowser.Core/RegisterServices.cs`.
- Network access the first time each of the three samples is opened. After that the cached copy
  under `LocalApplicationData/PolyHavenBrowser/cache` is used and the application runs offline.
- A GPU or a software rasterizer for the texture and model modes. OpenGL is the default and works
  on every head, because Graphics3DGL resolves the head's own native GL machinery (WGL on the
  Windows heads, GLX on X11, EGL on Wayland and the framebuffer head, CGL on macOS). Vulkan
  additionally needs a Vulkan loader and driver, and is offered only on the LinuxX11,
  LinuxWayland, Win32Skia and WinWpfSkia heads. Metal is offered only on macOS. The HDRI mode
  needs no GPU at all.

Run one head from the application folder:

```bash
dotnet run --project src/PolyHavenBrowser.LinuxX11
```

| Test project | Covers |
| --- | --- |
| `tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests` | Offline unit tests over a stub `HttpMessageHandler` (request URLs, JSON parsing, file-tree traversal, thumbnail URLs, download progress and MD5 verification, error mapping), factory and DI registration tests, mocked-consumer tests, and a separate `Live/` suite that hits the real API |
| `tests/libs/PolyHavenBrowser.Rendering.Tests` | Pure unit tests (orbit and panorama cameras, the EXR, Radiance HDR and LDR decoders, tone mapping, float images, glTF loading, texture-loader dispatch), real-GPU suites for each of the three renderers, and platform-gate tests for the Vulkan and Metal allow lists |

`global.json` in this folder selects the Microsoft.Testing.Platform runner:

```json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

Because that runner is selected, a plain `dotnet test` can report that zero tests ran, depending
on the SDK. The way that always works is to build a test project and run the executable it
produces directly:

```bash
dotnet build tests/libs/PolyHavenBrowser.Rendering.Tests -c Release
./tests/libs/PolyHavenBrowser.Rendering.Tests/bin/Release/net10.0/PolyHavenBrowser.Rendering.Tests
```

Suites that need something the machine may not have skip themselves rather than fail. The GL
suite carries `[Trait("Category", "RequiresGL")]` and skips when a surfaceless EGL context cannot
be created (Linux, Mesa, llvmpipe or a hardware render node is enough); the Vulkan suite carries
`[Trait("Category", "RequiresVulkan")]` and skips when no Vulkan stack is present (lavapipe is
enough); the Metal suite carries `[Trait("Category", "RequiresMetal")]` and skips everywhere
except macOS. The live API tests carry `[Trait("Category", "LiveApi")]`, share one `LiveApiFixture`
through `[Collection("LiveApi")]`, and need network; the fixture's own doc comment records the
exclusion filter `Category!=LiveApi`.

## How the projects and folders are organized

```text
PolyHavenBrowser_viewer_only/
  PolyHavenBrowser.slnx                    The one cross-platform solution
  global.json                              Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt                  Third-party attribution
  src/
    PolyHavenBrowser.UI/                   Shared XAML UI (.shproj + .projitems), compiled into every head
      App.xaml / App.xaml.cs               Default font, DI bootstrap, logging, first navigation
      Views/MainPage.xaml(.cs)             Buttons, engine dropdown, SKXamlCanvas, pointer wiring
    PolyHavenBrowser.Core/                 The app library that carries every package the heads need
      RegisterServices.cs                  The one AddPolyHavenBrowser() DI registration
      Helpers/HostHelper.cs                IHostBuilderProvider for SimpleServiceResolver
      Converters/                          BoolToAccentStyleConverter, the selected-button highlight
      ViewModels/MainViewModel.cs          Sample selection, engine switching, ICanvasInvalidator
      Display/                             The reusable "3D in a CodeBrix view" layer
        RENDERING-PIPELINE.md              In-repo architecture document for this folder
        IScenePainter.cs                   The paint plus pointer contract the page talks to
        IModelRenderEngine.cs              The swappable graphics-API seam, and RenderedFrame
        IModelRenderEngineFactory.cs       Per-backend factories; the OpenGL factory lives here
        IModelRenderEngineSelector.cs      RenderEngineKind, the platform gate, the dropdown source
        OpenGlModelRenderEngine.cs         OffscreenGLContext, an FBO, and glReadPixels
        VulkanModelRenderEngine.cs         Adapter over VulkanSceneRenderer, and its factory
        MetalModelRenderEngine.cs          Adapter over MetalSceneRenderer, and its factory
        ModelScenePainter.cs               API-agnostic pointer input and Skia compositing
        PanoramaScenePainter.cs            The CPU HDRI panorama painter
        CubeMeshBuilder.cs                 Texture to a textured cube LoadedModel
      Services/SampleAssetService.cs       Picks, downloads and slug-caches the three samples
    PolyHavenBrowser.Win32Skia/            }
    PolyHavenBrowser.WinWpfSkia/           }  Six thin heads. Each is a Program.cs plus a csproj
    PolyHavenBrowser.LinuxX11/             }  that imports the shared .projitems, references
    PolyHavenBrowser.LinuxWayland/         }  PolyHavenBrowser.Core, and adds exactly one
    PolyHavenBrowser.LinuxFrameBuffer/     }  CodeBrix.Platform Skia runtime package
    PolyHavenBrowser.MacOS/                }
    libs/
      PolyHavenBrowser.PolyHavenApiClient/ Typed REST client for the Poly Haven API
        Models/                            Asset, author, file tree, file reference, progress
        Exceptions/                        The API, not-found and integrity exceptions
        Serialization/                     The source-generated JsonSerializerContext
      PolyHavenBrowser.Rendering/          Headless loaders, cameras, decoders, three renderers
        Cameras/OrbitCamera.cs             Turntable camera plus model framing
        GL/                                IModelSceneRenderer and GlModelSceneRenderer
        Vulkan/                            VulkanSceneRenderer, VulkanShaders, VulkanPlatformSupport
        Metal/                             MetalSceneRenderer, MetalShaders, MetalInterop, MetalPlatformSupport
        Models/                            IModelLoader, GltfModelLoader, LoadedModel and materials
        Images/                            FloatImage, ExrDecoder, RadianceHdrDecoder, LdrImageDecoder
        Panorama/                          EquirectPanoramaRenderer and PanoramaCamera
        ToneMapping/                       ToneMapper and ToneMapOperator
        Textures/TextureImageLoader.cs     Extension-dispatching one-stop image loader
  tests/
    libs/
      PolyHavenBrowser.PolyHavenApiClient.Tests/   Unit, Mocked, Live, TestDoubles, TestData
      PolyHavenBrowser.Rendering.Tests/            Unit, Gl, Vulkan, Metal, Mocked, TestDoubles, TestData
```

The dependency direction is strictly one way, from the UI down. Each head references
`PolyHavenBrowser.Core` by project reference and imports `PolyHavenBrowser.UI.projitems` as a
**shared** import, so `App.xaml`, `App.xaml.cs`, `MainPage.xaml` and `MainPage.xaml.cs` are
file-linked and compiled into every head rather than shipped as a separate assembly; that is also
why every head csproj re-declares `<Page Include="**\*.xaml" …>`, so MSBuild treats those files as
CodeBrix.Platform XAML pages. `PolyHavenBrowser.Core` references both `src/libs` libraries by
project reference and carries every package the heads need, which is why each head csproj carries
the comment "EXACTLY ONE platform head package; all other packages come from
PolyHavenBrowser.Core". `PolyHavenBrowser.Rendering` knows nothing about XAML, the view model or
the UI, and `PolyHavenBrowser.PolyHavenApiClient` knows nothing about rendering; each has exactly
one mirrored `tests/libs/*.Tests` project and an `InternalsVisibleTo.cs` naming only that test
assembly. The two side libraries enable `Nullable`, `ImplicitUsings` and
`GenerateDocumentationFile`, which the heads and Core do not, and the rendering library adds
`AllowUnsafeBlocks` for its pixel and interop code.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework and the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `[AffectsCommands]`, `SimpleServiceResolver`, `IXamlRootGetter` and `CreateDialog`, plus `FeatureConfiguration.Font` and the logging adapter | `src/PolyHavenBrowser.Core/PolyHavenBrowser.Core.csproj`, `src/PolyHavenBrowser.UI/App.xaml.cs`, `src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform Skia runtime for the head | Exactly one runtime package per head project, and nothing else | `src/PolyHavenBrowser.<Head>/PolyHavenBrowser.<Head>.csproj` |
| CodeBrix.Platform SkiaSharp Views | `SKXamlCanvas`, the surface the off-screen 3D frame and the CPU panorama are composited onto | `src/PolyHavenBrowser.UI/Views/MainPage.xaml`, `src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`, `src/PolyHavenBrowser.Core/Display/PanoramaScenePainter.cs` |
| CodeBrix.Platform Graphics3DGL | `OffscreenGLContext`, the cross-platform off-screen native GL context, and transitively the `GL` binding the shader renderer draws with | `src/PolyHavenBrowser.Core/Display/OpenGlModelRenderEngine.cs`, `src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs` |
| CodeBrix.Platform Fonts (Roboto) | The bundled application font, set as the default text font family and also exposed as a `FontFamily` resource | `src/PolyHavenBrowser.UI/App.xaml`, `src/PolyHavenBrowser.UI/App.xaml.cs` |
| CodeBrix.Imaging | Decodes the downloaded JPEG, PNG and WebP maps and glTF embedded textures to `SKBitmap` or raw RGBA | `src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs` |
| CodeBrix.TestMocks | `Mock<T>` doubles for `IModelLoader`, `IModelSceneRenderer`, `IPolyHavenApiClient` and its factory | `tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/`, `tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Mocked/` |
| SilverAssertions | The assertion style in both test projects | both projects under `tests/libs/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SharpGLTF | Reads `.gltf` and `.glb`, walks the node tree, and exposes material channels including `Transmission`; its Toolkit builds in-memory `.glb` fixtures in the tests | `src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs`, `tests/libs/PolyHavenBrowser.Rendering.Tests/TestData/TestAssets.cs` |
| Silk.NET.Vulkan | The whole Vulkan backend: instance, device, off-screen images, pipelines and readback | `src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanSceneRenderer.cs` |
| TinyEXR.NET | Decodes OpenEXR images to linear floats | `src/libs/PolyHavenBrowser.Rendering/Images/ExrDecoder.cs` |
| SkiaSharp | The `SKBitmap`, `SKImage` and `SKCanvas` types the painters, decoders and tone mapper work in | throughout `src/PolyHavenBrowser.Core/Display/` and `src/libs/PolyHavenBrowser.Rendering/` |
| Microsoft.Extensions.Http, Hosting and Logging.Console | `IHttpClientFactory` for the API client, the generic host behind `SimpleServiceResolver`, and console logging in Debug builds | `src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs`, `src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`, `src/PolyHavenBrowser.UI/App.xaml.cs` |

The Metal backend deliberately takes no library at all: `MetalInterop` P/Invokes
`libobjc.A.dylib` and `Metal.framework` by path.

## Worth studying in this application

### The seam that makes the graphics API swappable

`Display/IModelRenderEngine.cs` is the whole idea in one file: an engine exposes an
`OrbitCamera`, an optional `FixedLightDirection`, `SetModel(model)` and
`RenderFrame(width, height, background)`, and returns a `RenderedFrame` of RGBA bytes that says
how wide, how tall and whether the first row is the bottom of the image. Everything above that
interface, `ModelScenePainter`, the camera, `CubeMeshBuilder`, `GltfModelLoader`, the view model
and the XAML, is graphics-API-agnostic and is reused unchanged by all three backends. Everything
below it is one class cluster per API.

The choice is owned by a service, not by the page. `ModelRenderEngineSelector` is registered as a
singleton in `RegisterServices.cs`, exposes `AvailableKinds` (OpenGL, Vulkan, Metal, in dropdown
order), answers `IsSupported(kind)` and creates engines on demand. The view model resolves it,
copies the kind names into a bound list, and binds the selection to a string property; the
dropdown never sees an engine, a factory or a platform check. `Create()` hands back a fresh engine
and the caller owns it, so the view model disposes the old painter (which disposes its engine)
only after the new one is built. Read `IModelRenderEngine.cs`, then `IModelRenderEngineSelector.cs`,
then `RENDERING-PIPELINE.md` for the diagram.
[Swap the 3D graphics backend at run time from a dropdown](../BLUEPRINTS.md#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown)

### Off-screen OpenGL that never disturbs the head's own renderer

`Display/OpenGlModelRenderEngine.cs` is the backend that works everywhere. It never P/Invokes a
platform GL loader: `OffscreenGLContext.TryCreate(xamlRoot, out context)` from Graphics3DGL
resolves the running head's own native GL machinery. Two details are the whole reason the file is
shaped the way it is. First, a GL context must be created and used on the thread that renders, so
the engine is constructed with a `Func<XamlRoot>` accessor rather than a `XamlRoot`, and the
context is created lazily inside the first `RenderFrame()` call, which runs in the Skia paint
callback. Second, `MakeCurrent()` returns a scope that saves whatever context the head had current
and restores it on dispose, so the engine can share the UI thread with the head's renderer without
disturbing it; `Dispose()` re-enters that scope before deleting its framebuffer.

The framebuffer carries a color renderbuffer and a `DepthComponent16` depth renderbuffer, is
recreated on every size change and is checked with `CheckFramebufferStatus`; without the depth
attachment geometry does not occlude correctly. The per-frame cost is dominated by the
`glReadPixels()` GPU-to-CPU sync, which is why the application drops frames under load rather than
lowering resolution.
[Render an OpenGL scene off screen and composite it onto an SKXamlCanvas](../BLUEPRINTS.md#render-an-opengl-scene-off-screen-and-composite-it-onto-an-skxamlcanvas)

### The Vulkan and Metal backends own their whole stack, and take almost nothing with them

`VulkanSceneRenderer` and `MetalSceneRenderer` live in the headless rendering library and own
everything from the device down to the readback, so neither has any ambient thread state and
neither can collide with the head's renderer. `Display/VulkanModelRenderEngine.cs` and
`Display/MetalModelRenderEngine.cs` are each a thin adapter that implements `IModelRenderEngine`
and declares the frame orientation.

The two differ in how they get shaders. Vulkan consumes SPIR-V, so `VulkanShaders.cs` holds
pre-compiled words as `static readonly uint[]` with the source GLSL alongside in comments and a
doc comment recording how to regenerate them, which keeps `glslc` out of the build. Metal compiles
MSL from source at run time, so `MetalShaders.cs` is just a string. Metal is also the backend with
no dependency at all: `Metal/MetalInterop.cs` P/Invokes `objc_getClass`, `sel_registerName` and
`objc_msgSend` directly. The rule that file is built around is worth internalizing before writing
anything similar: never call an Objective-C method that returns a struct by value, because that is
the one place the `objc_msgSend` calling convention differs between arm64 and x86-64 (which would
otherwise need `objc_msgSend_stret`). Every message here returns a pointer or `void`, or takes a
struct only as an argument, so a single entry point serves both architectures. For the same
reason, every transfer goes through a shared-storage staging buffer blitted to and from a private
texture, with rows padded to the 256-byte alignment buffer-to-texture blits require.
[Add a self contained Vulkan renderer that needs no shader toolchain](../BLUEPRINTS.md#add-a-self-contained-vulkan-renderer-that-needs-no-shader-toolchain) ·
[Add a direct to Metal renderer with no NuGet package or Apple bindings](../BLUEPRINTS.md#add-a-direct-to-metal-renderer-with-no-nuget-package-or-apple-bindings)

### The platform gate, and how a headless library knows which head it is in

`Vulkan/VulkanPlatformSupport.cs` and `Metal/MetalPlatformSupport.cs` are deliberate policy lists,
not driver probes: an API is never even attempted on a head that has not been okayed. Vulkan is
allowed on LinuxX11, LinuxWayland, Win32Skia and WinWpfSkia; macOS and LinuxFrameBuffer are
excluded. Metal mirrors it exactly: macOS only, and additionally only when
`RuntimeInformation.ProcessArchitecture` is arm64 or x64, keyed off the process architecture so a
translated process is correctly treated as x64.

The head detection underneath is the reusable part. Each head's `Program.Main` loads exactly one
CodeBrix.Platform Skia runtime assembly, so a one-time `Lazy<PlatformHead>` scan of the loaded
assemblies classifies the head by assembly-name prefix, and the library needs no reference to any
head project. Prefix matching is used so a head's satellite assemblies still classify correctly,
and anything unrecognized classifies as `Unknown` and is conservatively unsupported, which is also
what a unit-test host is; there is a test asserting exactly that. `MetalPlatformSupport` forwards
to the same detection rather than duplicating it. The one caveat is that the scan relies on the
runtime assembly already being loaded, which holds by the time any UI runs but not in a static
initializer that runs before the host is built.
[Gate an optional graphics backend to specific heads with an allow list](../BLUEPRINTS.md#gate-an-optional-graphics-backend-to-specific-heads-with-an-allow-list) ·
[Detect which platform head is running without referencing it](../BLUEPRINTS.md#detect-which-platform-head-is-running-without-referencing-it)

### Pixel orientation and the matrix that must not be transposed

`Display/ModelScenePainter.cs` is API-agnostic: it asks the engine for a frame, reads
`frame.IsBottomUp`, and applies a vertical flip only when the flag says so. Each engine declares
its own orientation and nothing else in the application knows about it. OpenGL's first pixel row
is the image bottom, so it reports `true`. Vulkan uses the same unmodified camera matrices and its
clip-space Y points down, so its readback comes out inverted too and it also reports `true`,
sharing the flip. Metal is the exception: its clip-space Y points up while its framebuffer origin
is top-left, so its readback is already top-down and it reports `false`. The painter also uses
`SKAlphaType.Unpremul`, without which the transparent clear used behind the texture backdrop does
not composite correctly.

The matching pitfall lives in `Cameras/OrbitCamera.cs` and the three renderers: do not add a
`Matrix4x4.Transpose()`. `System.Numerics` stores matrices row-major, and GLSL, SPIR-V and MSL all
read a `mat4` column-major, which already applies the transpose the APIs need; transposing again
silently flattens the depth axis, and only for rotated cameras, so an axis-aligned test view hides
the bug entirely. The regression test that pins it,
`nearer_geometry_occludes_farther_geometry_regardless_of_draw_order`, uses a rotated camera on
purpose, tries both draw orders, and exists once per renderer.
[Composite engine pixels onto Skia with the right vertical orientation](../BLUEPRINTS.md#composite-engine-pixels-onto-skia-with-the-right-vertical-orientation) ·
[Share one camera and one matrix convention across graphics APIs](../BLUEPRINTS.md#share-one-camera-and-one-matrix-convention-across-graphics-apis)

### Switching engines safely: alert, revert, pre-warm

`MainViewModel.SelectedRenderEngineName` is the model for any picker that can offer something the
running machine cannot do. The dropdown deliberately lists every kind rather than filtering to the
supported ones, so the user learns *why* an option is unavailable instead of never seeing it. The
setter is optimistic: it shows the new selection at once, raises the change, and starts
`SwitchEngineAsync`. That method asks the selector whether the kind is supported; when it is not,
it shows a `SimpleDialog` alert built with `CreateDialog(...)` in a `using` block and reverts. The
revert writes the backing field directly and raises the notification by hand, because going through
the public setter would re-enter the switch, and its target is `_currentEngineKind`, the engine
that is actually running, so a second failed switch returns to whatever is really live.

A supported platform can still have a missing or broken driver, so before the new engine is handed
to a paint callback the view model renders one throwaway 1x1 frame inside `Task.Run()`. A failure
there becomes bound `StatusText`, not an exception inside `PaintSurface`. That pre-warm is applied
only to Vulkan and Metal: the OpenGL engine is excluded on purpose, because its native context has
thread affinity and must be created on the render thread at first paint. The swap order matters
too: `_currentPainter` is cleared before the old painter is disposed, so the page's paint handler
cannot reach a disposed painter in between. After a successful switch the current sample is
re-displayed from the local cache, so changing engines never touches the network.
[Alert and revert when the user picks an unsupported option](../BLUEPRINTS.md#alert-and-revert-when-the-user-picks-an-unsupported-option) ·
[Pre warm a rendering backend off the UI thread](../BLUEPRINTS.md#pre-warm-a-rendering-backend-off-the-ui-thread)

### Three sample kinds, one canvas, two painters

`Display/IScenePainter.cs` is the contract the page talks to: `Paint(surface, info)` plus
`PointerDown`, `PointerDrag`, `PointerSkip`, `PointerUp` and `Zoom`. There are two implementations.
`ModelScenePainter` wraps whichever `IModelRenderEngine` is current and serves both the texture and
the model modes. `PanoramaScenePainter` is pure CPU: `EquirectPanoramaRenderer` ray-traces the
equirectangular image into a reused `SKBitmap` through a `Parallel.For` over the rows, which is why
the engine dropdown is hidden in HDRI mode and why that mode works on a head with no GPU. The two
painters even use opposite strategies for staying interactive: the GPU painter always renders at
full canvas resolution and drops frames under load, while the panorama painter caps its render
resolution, with a lower cap while a drag is in progress and the full cap once the drag stops, then
scales the result up.

`MainViewModel.BuildPainter` is where the three modes differ, and it is worth reading for the
framing decisions as much as the plumbing. The texture mode decodes the map once and uses it twice,
as the cube's texture and as the darkened backdrop, then sets `FixedLightDirection` so the faces
shade distinctly; the default is a camera headlight, which double-sides the lighting and makes a
cube read as ambiguous because every face gets the same brightness. Camera framing must be set
*before* `SetModel`, because framing is applied when the pending model is taken up at render time.
The model mode clears the fixed light and the backdrop and reframes closer with a vertical bias so
the model sits lower in view; `OrbitCamera.FitToModel` orbits around the vertex centroid when the
model has one, so a model with one sparse extremity rotates in place.
[Paint a CPU ray traced panorama into an SKBitmap](../BLUEPRINTS.md#paint-a-cpu-ray-traced-panorama-into-an-skbitmap) ·
[Build a textured cube mesh from a bitmap for previewing a flat material](../BLUEPRINTS.md#build-a-textured-cube-mesh-from-a-bitmap-for-previewing-a-flat-material)

### Loading assets without blocking the UI, and repainting without a page reference

`MainViewModel.SelectAsync` is the shape to copy for any "fetch, decode, show" command. It guards
re-entry with `IsBusy`, sets `IsBusy` (which carries `[AffectsCommands]` naming the three sample
commands, so their `CanExecute` refreshes and the buttons disable), awaits the download with an
`IProgress<string>` created on the UI thread as `new Progress<string>(message => StatusText =
message)` so its callbacks post back there, does the decode or mesh build inside `Task.Run()`,
assigns the painter, and in its `finally` always clears `IsBusy` and invalidates the canvas. A
failure becomes a status line, never an escaping exception. Nothing GPU-related happens on the
worker thread: the "safe from any thread" contract is enforced in the renderers, which take a lock,
stash the model as pending, and upload it on the next render, on the render thread. All three
renderers do this the same way.

Repainting reaches the page through a one-property bridge. `ICanvasInvalidator` is declared beside
the view model and holds an `Action`; `MainPage` assigns its own `RequestRender` method into it
from `DataContextChanged`, and the view model calls `InvalidateCanvas?.Invoke()`, so it degrades
gracefully when no page has wired it. The same handler hands the view model a `XamlRoot` accessor
through `IXamlRootGetter`, which is what lets `CreateDialog(...)` attach an alert and what the view
model passes on to the engine selector as a `Func<XamlRoot>` for the OpenGL context. Both are wired
in `DataContextChanged` rather than the constructor, because with `<Page.DataContext>` declared in
XAML the `DataContext` is not yet set when the constructor body runs.
[Do blocking work in a service behind Task Run](../BLUEPRINTS.md#do-blocking-work-in-a-service-behind-task-run) ·
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface) ·
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)

### Pointer input, coalesced repaints and backlog frames

Every pointer handler on the canvas is a short forward to `ViewModel?.CurrentPainter`: convert the
position, call one painter method, request a render, set `e.Handled = true`. Three details in
`MainPage.xaml.cs` are the ones that bite. Pointer positions arrive in view units while the canvas
renders in pixels, so `ToCanvasPixels` scales by `CanvasSize / ActualWidth` and the height
equivalent, or input drifts from the image at any non-default DPI and after a resize. `PointerMoved`
must set `e.Handled = true`, or an unhandled move bubbles to the window manager, which then drags
the window instead of orbiting the scene. And the pointer must be captured on press, released on
`PointerReleased`, and the drag also ended on `PointerCaptureLost`; `SizeChanged` requests a render
too.

The interesting policy is what happens when a repaint costs more than the input stream allows.
Two independent mechanisms handle it. Coalescing keeps at most one paint queued: `RequestRender`
returns early when a paint is already pending, and the flag is cleared at the *top* of the
`PaintSurface` handler so a request made during a paint still queues the next frame. Backlog
detection compares the pointer event's own timestamp against a `Stopwatch` started at the beginning
of the gesture; a frame that has fallen far enough behind real time is discarded through
`painter.PointerSkip(x, y)`, which advances the drag anchor without applying the delta to the
camera, so dropping a frame keeps the camera in sync with the cursor instead of making the model
jump. `PointerSkip` exists on `IScenePainter` for exactly this purpose, which is where the policy
belongs when you build this yourself: the view model owns the painter, and the page's handlers
stay one-line forwards.
[Forward pointer input from a canvas into a model](../BLUEPRINTS.md#forward-pointer-input-from-a-canvas-into-a-model) ·
[Coalesce repaints and drop backlogged pointer frames](../BLUEPRINTS.md#coalesce-repaints-and-drop-backlogged-pointer-frames)

### The sample-asset service and the typed REST client

`Services/SampleAssetService.cs` is a singleton registered in the container, takes
`IPolyHavenApiClientFactory` in its constructor, and exposes one `async` method that reports
through `IProgress<string>` and honors a `CancellationToken`; every await uses
`.ConfigureAwait(false)`, because it is a service and not view-model code. Its cache is the part
worth stealing. The marker file is keyed by asset kind, but it *records the curated slug and
compares it before reuse*, so changing which asset the sample points at invalidates the cache
instead of serving the old file forever. It also checks that the primary file still exists before
trusting the marker, and `TryReadMarker` swallows any exception and returns null, so a corrupt
marker simply re-downloads. A `SemaphoreSlim(1, 1)` serializes the whole method so two quick button
presses cannot race on the same folder. The model download recreates the glTF bundle's shape on
disk: the API's file reference carries an `Include` dictionary keyed by exactly the relative paths
the glTF expects, so each side-car path is translated with
`relativePath.Replace('/', Path.DirectorySeparatorChar)`, its directory created, and the file
downloaded next to its parent. File selection uses layered `??` fallbacks so an asset with an
unexpected tree still resolves or fails with a clear message.

Below it, `src/libs/PolyHavenBrowser.PolyHavenApiClient` is a self-contained typed REST client:
an interface for the client, an interface for the factory, an options class and one
`AddPolyHavenApiClient()` extension that registers a named `HttpClient` with a `SocketsHttpHandler`
configured for pooled connection lifetime and automatic decompression. The detail that catches
people out is the timeout: `HttpClient.Timeout` is set to infinite and metadata requests apply
their own timeout through `CancellationTokenSource.CreateLinkedTokenSource(...)` plus `CancelAfter`,
because a client-wide timeout would abort a long file download. JSON goes through a
source-generated `JsonSerializerContext` with snake_case naming, HTTP 404 maps to a dedicated
`PolyHavenNotFoundException`, optional MD5 verification throws `PolyHavenIntegrityException` and
the path overload deletes a partially written file on failure.
[Cache downloaded assets with a key you can invalidate](../BLUEPRINTS.md#cache-downloaded-assets-with-a-key-you-can-invalidate) ·
[Report true byte progress across a multi file download with side car files](../BLUEPRINTS.md#report-true-byte-progress-across-a-multi-file-download-with-side-car-files) ·
[Build a typed REST client with source generated JSON and its own exceptions](../BLUEPRINTS.md#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions)

### Image decoding, tone mapping and glTF glass

`Textures/TextureImageLoader.cs` is the single entry point: it dispatches on file extension,
sending `.exr` to TinyEXR.NET, `.hdr` to a hand-written Radiance decoder that handles both
old-style repeat runs and new-style RLE, and everything else to CodeBrix.Imaging. The two HDR
formats are then treated differently on purpose: `.exr` files here are usually non-photographic
data maps and get min-max normalization so the full value range shows, while `.hdr` panoramas get
real tone mapping through `ToneMapper` (ACES filmic, Reinhard or clamp). `LoadFloatImage` refuses
LDR extensions rather than silently promoting them. `LdrImageDecoder` narrows CodeBrix.Imaging's
`UnknownImageFormatException` and `InvalidImageContentException` to `InvalidDataException`, which
gives callers one exception to catch, and `GltfModelLoader` uses that to degrade an undecodable
texture to the base color factor instead of failing the whole model load.

The glass handling in `Models/GltfModelLoader.cs` is the subtle part. glTF marks glass two ways,
and the second is easy to miss: `alphaMode: BLEND`, and a `KHR_materials_transmission` extension on
a material that is otherwise OPAQUE, which is how exporters mark a camera lens or a clock face.
The loader treats both as translucent and gives them a fixed preview opacity in
`ModelMaterial.BlendPreviewOpacity`, which rides in the existing `BaseColorFactor.W` so no shader
or SPIR-V change was needed. Each renderer then draws two passes: opaque and mask primitives first
with depth writes on, then the translucent ones with depth writes off. The GL pass uses
`BlendFuncSeparate(SrcAlpha, OneMinusSrcAlpha, One, OneMinusSrcAlpha)`, and the alpha channel must
accumulate coverage that way or a region already opaque behind the glass loses its opacity when
Skia composites the frame; Vulkan expresses the same thing as a second pipeline with blending
enabled and depth writes disabled. Translucent primitives are deliberately not depth-sorted, which
is acceptable for the small amount of transparent geometry in preview models.
[Decode HDR images and tone map them for display](../BLUEPRINTS.md#decode-hdr-images-and-tone-map-them-for-display) ·
[Decode raster images with the CodeBrix Imaging library into a Skia bitmap](../BLUEPRINTS.md#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap) ·
[Draw translucent surfaces in a second pass with depth writes off](../BLUEPRINTS.md#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off)

### Testing GPU code, and an HTTP client, without a window or a network

`tests/libs/PolyHavenBrowser.Rendering.Tests` is the reason the rendering library is a separate
assembly. Its GL suite creates a real context with no window system through
`TestDoubles/EglTestContext.cs`, which asks Mesa for a surfaceless EGL display; the entry point it
uses is `eglGetPlatformDisplay`, core EGL, rather than `eglGetPlatformDisplayEXT`, because only the
former is a real exported symbol under the GLVND dispatcher. The three GPU suites deliberately
mirror each other test for test, covering drawing over the background, clearing the model, resizing
between frames, a textured material's color, the full path from a `.glb` to pixels, and the
depth-ordering regression, so all three backends prove the same behaviors. The Metal suite's pixel
checks are written to be orientation-agnostic, scanning the whole image or picking a vertically
symmetric center pixel, so they hold whether the readback is top-down or bottom-up. Fixtures are
built in memory: `TestData/TestAssets.cs` hand-encodes Radiance bytes and uses SharpGLTF.Toolkit to
build a `.glb`, so no binary assets are committed.

The API client tests are the offline counterpart. `StubHttpMessageHandler` matches routes by
path-and-query or URL fragment, records every request and 404s anything unrouted, which is how the
request-shape tests assert on exact URLs. The handful of tests that really hit the API live under
`Live/`, take a shared `LiveApiFixture` through a primary constructor, and carry both
`[Collection("LiveApi")]` and `[Trait("Category", "LiveApi")]` so they can be excluded from a
default run. Both projects use CodeBrix.TestMocks with `MockBehavior.Strict`, and a mocked renderer
can still hand out a real `OrbitCamera`, which is how the pointer-input wiring is tested with no
GPU at all.
[Prove every graphics backend with the same mirrored suite](../BLUEPRINTS.md#prove-every-graphics-backend-with-the-same-mirrored-suite) ·
[Test GL code headlessly with a surfaceless EGL context](../BLUEPRINTS.md#test-gl-code-headlessly-with-a-surfaceless-egl-context) ·
[Test an HTTP client offline with a stub handler](../BLUEPRINTS.md#test-an-http-client-offline-with-a-stub-handler) ·
[Make live tests opt in and keep them out of the default run](../BLUEPRINTS.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run) ·
[Build the binary inputs your tests need instead of committing them](../BLUEPRINTS.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them)

### Two project-file rules the csproj comments spell out

Both rules are recorded as comments in the projects they apply to, which is a habit worth copying.
`PolyHavenBrowser.Core` sets `<RootNamespace>PolyHavenBrowser</RootNamespace>` because it carries
XAML-visible types (the view model, the converter) in the `PolyHavenBrowser.*` namespace while the
assembly is named `PolyHavenBrowser.Core`; that is exactly what makes
`xmlns:vm="clr-namespace:PolyHavenBrowser.ViewModels;assembly=PolyHavenBrowser.Core"` resolve in
`MainPage.xaml`. The rendering library hosts no XAML control, so it keeps its default root
namespace, and its csproj says so. The second rule is about a package reference, not a namespace:
the code does have `using CodeBrix.Platform.OpenGL;`, because that is where the `GL` type lives,
but no project declares a reference to that package, and the comment in both csproj files calls
coding to Graphics3DGL a hard rule. The pay-off is that the off-screen context resolves the head's
own native GL wrapper, so the application ships no platform GL loader of its own and works on all
six heads.
[Set the Core library root namespace to the application namespace](../BLUEPRINTS.md#set-the-core-library-root-namespace-to-the-application-namespace) ·
[Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively)

## Third-party content

Third-party code arrives as NuGet packages, each carrying its own license and notices, so those
are not reproduced here; see [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder for
the details. What that file covers is the run-time data: the sample textures, HDRI environments and
3D models the application downloads from Poly Haven, released under the Creative Commons CC0 1.0
Universal public-domain dedication. None of those assets are redistributed in this repository; they
are fetched on demand and cached locally.

## License

PolyHavenBrowser_viewer_only is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
