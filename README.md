# CodeBrix.Samples

A collection of complete, runnable **reference applications** for the
[CodeBrix](https://github.com/ellisnet) family of .NET libraries. Each sample is a real,
non-trivial desktop application — not a toy snippet — built to show how the CodeBrix
NuGet packages are meant to be consumed and how a single shared view model and UI can
drive many platform "heads" (Windows, macOS, Linux) from one codebase.

Every sample follows the same house style, modeled on
[JustBetweenUs](https://github.com/ellisnet/JustBetweenUs): one shared `MainViewModel`
and one shared Skia XAML UI are reused across every head, business logic lives in a
separate reusable library, and each head is a thin project that supplies only its
platform-specific plumbing (windowing backend, file dialog, WebView, …).

All samples target the **.NET 10 SDK** and consume CodeBrix packages straight from
nuget.org — none of them reference a CodeBrix library as a source project, so each folder
is self-contained and can be opened and built on its own.

| Sample | What it is | Headline CodeBrix libraries |
| --- | --- | --- |
| [PainDiagram](#paindiagram) | Interactive body-map pain/symptom annotator with highlighter-style drawing | `CodeBrix.Imaging.Drawing`, `CodeBrix.Platform.*` |
| [PolyHavenBrowser](#polyhavenbrowser) | Browses free Poly Haven assets and renders 3D models / textures / HDRIs with OpenGL onto a Skia canvas in the UI | `CodeBrix.Platform.OpenGL`, `CodeBrix.Imaging`, `CodeBrix.Platform.*` |
| [WikipediaPublisher](#wikipediapublisher) | Turns any Wikipedia article into a book-designed, print-ready PDF | `CodeBrix.PdfDocCreate`, `CodeBrix.MarkupParse`, `CodeBrix.Imaging`, `CodeBrix.Platform.*` |

Sections below are in alphabetical order.

---

## PainDiagram

### What it is

**PainDiagram** is an interactive pain- and symptom-mapping application for healthcare
contexts. The user annotates a medical body-map image with three symptom types by drawing
freehand over the body with the mouse, pen, or touch — then exports the annotated diagram
as a PNG. It is the reference application for the
**`CodeBrix.Imaging.Drawing`** stroke-based drawing library, and it re-creates the
"draw your pain on a body map" workflow from the clinical NuraPad application.

The three symptom types are modeled as translucent **highlighter layers**, each with its
own color (the same colors the original NuraPad used):

| Layer | Color | RGB |
| --- | --- | --- |
| Pain | bright magenta | 255, 30, 230 |
| Numbness | blue | 30, 128, 204 |
| Tingling | yellow-gold | 204, 170, 10 |

Because the layers use the library's highlighter compositing model, overlapping strokes
on the same layer never darken where they cross — the ink reads like a real highlighter
pen rather than stacked opaque paint.

### What it does / how to use it

1. Pick a symptom by clicking **Pain**, **Numbness**, or **Tingling** (the active layer
   shows a ✓). This sets `DrawingSession.ActiveLayer`.
2. Draw on the body map with the left mouse button; strokes accumulate on the active
   layer and the canvas repaints live as you draw.
3. **Clear** starts over (with a confirmation prompt once there's more than a stroke or
   two on the canvas).
4. **Save** exports the composited diagram — background body map plus all layers — as a
   1000 × 1000 px PNG via `DrawingSession.ExportPng(new Size(1000, 1000))`. The save path
   comes from a native file dialog on heads that have one, or a default `Pictures` folder
   path on heads that don't (e.g. the Linux framebuffer head).

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `PainDiagram.slnx` | Linux, macOS, Windows | The shared UI/Core plus the six CodeBrix.Platform (Skia) heads |
| `PainDiagram.Windows.slnx` | Windows | Everything above **plus** the native WinUI 3 and WPF heads |

`PainDiagram.Windows.slnx` restricts its solution platforms to x86/x64/ARM64 (no "Any
CPU") because the WinUI 3 head only declares those platforms. Build with the .NET 10 SDK;
the Windows-targeting Skia head (`WinWpfSkia`) compiles on Linux/macOS via
`EnableWindowsTargeting` but only *runs* on Windows.

### The eight heads

One shared `MainViewModel` drives all of them:

**CodeBrix.Platform (Skia) heads** — share the XAML UI in `PainDiagram.UI`:

| Project | Platform / windowing |
| --- | --- |
| `CodeBrixPlatform/PainDiagram.Win32Skia` | Windows, native Win32 window |
| `CodeBrixPlatform/PainDiagram.WinWpfSkia` | Windows, Skia hosted in WPF |
| `CodeBrixPlatform/PainDiagram.LinuxX11` | Linux desktop, X11 |
| `CodeBrixPlatform/PainDiagram.LinuxWayland` | Linux desktop, native Wayland |
| `CodeBrixPlatform/PainDiagram.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) |
| `CodeBrixPlatform/PainDiagram.MacOS` | macOS |

**Native (non-Skia) heads** — same view model, native UI stacks:

| Project | UI stack |
| --- | --- |
| `PainDiagram.WinUI` | WinUI 3 (Windows App SDK) |
| `PainDiagram.Wpf` | WPF |

### How the projects/files/folders are organized

```
PainDiagram/
├─ PainDiagram.slnx                  Cross-platform solution (open anywhere)
├─ PainDiagram.Windows.slnx          Windows-only solution (adds WinUI + WPF heads)
│
├─ Shared/                           File-linked source shared by every head
│   ├─ ViewModels/MainViewModel.cs   The single source of truth for all heads
│   ├─ Drawing/DrawingCanvas.cs      Canvas abstraction (see below)
│   ├─ Helpers/                      HostHelper, FileDialogHelper
│   └─ Assets/body_map_master.png    The body-map background image
│
├─ CodeBrixPlatform/                 The Skia-based (CodeBrix.Platform UI) heads
│   ├─ PainDiagram.UI/               Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)             Shared app bootstrap
│   │   └─ Views/MainPage.xaml(.cs)  Buttons + DrawingCanvas
│   ├─ PainDiagram.Core/             Shared library: embeds the body map, links the
│   │                                shared source, references the CodeBrix packages
│   └─ PainDiagram.{Win32Skia,WinWpfSkia,LinuxX11,LinuxWayland,LinuxFrameBuffer,MacOS}/
│                                    Six thin heads, each adding one runtime package
│
├─ PainDiagram.WinUI/                Native WinUI 3 head (links the shared source)
└─ PainDiagram.Wpf/                  Native WPF head (links the shared source)
```

The shared `MainViewModel.cs` is **compiled into each head's own assembly** (via
`PainDiagram.Core` for the Skia heads, and via linked `<Compile Include>` items in the
WinUI and WPF heads) rather than shipped as a library. Every head that compiles the view
model also embeds `body_map_master.png` under the same logical resource name
(`PainDiagram.Assets.body_map_master.png`), so `MainViewModel.LoadBodyMapBackground()`
can load it by reflection off its own assembly and the exact same view-model code works
everywhere.

### How it uses the CodeBrix libraries

`PainDiagram.Core.csproj` pulls in the shared dependencies:

| Package | Version | Role |
| --- | --- | --- |
| `CodeBrix.Imaging.Drawing.ApacheLicenseForever` | 1.0.185.1134 | The star of the sample: `DrawingSession`, named highlighter layers, live rendering, PNG export |
| `CodeBrix.Platform.ApacheLicenseForever` | 1.0.189.446 | The cross-platform XAML/UI framework and the Simple MVVM toolkit |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | 4.148.0 | `SKXamlCanvas`, the SkiaSharp drawing surface hosted in XAML |
| `CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever` | 1.0.181.655 | Bundled Open Sans font for the UI |

Each Skia head then adds exactly one platform runtime package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever`
(1.0.189.446) — and the native heads add `CodeBrix.Platform.WinUI` / `CodeBrix.Platform.WPF`
(1.0.183.475) plus the matching `SkiaSharp.Views.*` package.

Inside `MainViewModel`, the drawing session is created and configured up front:

```csharp
_session = new DrawingSession(new DrawingSessionOptions
{
    BackgroundFillColor = Color.White,
    SurfaceClearColor   = Color.White,
});
_session.AddLayer(PainLayerName,     Color.FromRgb(255, 30, 230));
_session.AddLayer(NumbnessLayerName, Color.FromRgb(30, 128, 204));
_session.AddLayer(TinglingLayerName, Color.FromRgb(204, 170, 10));
_session.SetBackgroundImage(bodyMapBytes);
```

The view model derives from `CodeBrix.Platform.Simple.SimpleViewModel` and uses
`SimpleCommand`, `[AffectsCommands]`, and `[Microsoft.UI.Xaml.Data.Bindable]` from the
CodeBrix "Simple" MVVM toolkit.

### Why it's noteworthy for a CodeBrix developer

- **One view model, eight heads.** `Shared/ViewModels/MainViewModel.cs` is the single
  source of truth for all business logic; the per-head projects contribute only
  platform plumbing. This is the cleanest available illustration of the CodeBrix
  "write the app once, host it anywhere" model.
- **The highlighter compositing guarantee.** `CodeBrix.Imaging.Drawing` guarantees that
  two overlapping strokes on one layer produce pixel-identical output to a single stroke
  — the whole point of the "highlighter" model, and exactly what a pain-map needs so
  repeated passes over one area don't read as "more pain."
- **Resolution-independent calibrated strokes.** Strokes are stored in a logical drawing
  space, so `ExportPng(new Size(1000, 1000))` renders a crisp result at any size, fully
  decoupled from the on-screen canvas size — and works **headlessly**, with no window,
  which is what makes the library unit-testable on CI.
- **UI abstraction by conditional compilation.** `Shared/Drawing/DrawingCanvas.cs`
  resolves to `SkiaSharp.Views.Windows.SKXamlCanvas` on the CodeBrix.Platform/WinUI heads
  and `SkiaSharp.Views.WPF.SKElement` on WPF via `#if HAS_CODEBRIXPLATFORM || HAS_WINUI`,
  so a single `<drawing:DrawingCanvas>` element in the shared XAML binds to the right base
  type on every platform.
- **Bridge interfaces for platform services.** `IFileSaveBridge` and `ICanvasInvalidator`
  (both implemented by `MainViewModel`) let each head plug in its own native save dialog
  and canvas-invalidation call while the view model stays UI-framework-agnostic — and the
  view model degrades gracefully to a default save path when a head (the framebuffer
  kiosk) has no dialog at all.
- **Event-driven live redraw.** The session raises `RedrawRequested` / `DrawingChanged`;
  the view model forwards them so each head simply invalidates its canvas — no per-frame
  polling, and (per the library's design notes) no per-frame background rescaling, which
  is what keeps drawing latency low.

---

## PolyHavenBrowser

### What it is

**PolyHavenBrowser** downloads free CC0 assets from [Poly Haven](https://polyhaven.com) —
PBR textures, HDRI environments, and 3D models — and displays each one interactively. It is
the reference application for **rendering real-time 3D content inside an ordinary
CodeBrix.Platform view**: a model is drawn with **OpenGL ES (via EGL)** into an off-screen
framebuffer, the pixels are read back and composited onto a **Skia canvas** (`SKXamlCanvas`)
that lives in the app's normal XAML, and the user can orbit/zoom it — all powered by the
**`CodeBrix.Platform.OpenGL`** package. It also ships two self-contained side libraries (a
Poly Haven REST client and a headless rendering library), so it doubles as the reference for
the CodeBrix "application with extra library assemblies" project layout.

> 📄 **Want to understand (or copy) the 3D rendering?** Start with
> **[`PolyHavenBrowser/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`](PolyHavenBrowser/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md)** —
> a self-contained architecture guide (pipeline diagram, file map, threading/orientation
> gotchas, and step-by-step notes on swapping OpenGL for another backend such as Vulkan).

Three display modes share one canvas, each showing off a different rendering path:

| Button | Shows | How it's rendered |
| --- | --- | --- |
| **Sample Texture** | A texture's diffuse map | Wrapped on a lit, orbitable **cube** over a darkened backdrop of the same texture — GPU (OpenGL) |
| **Sample HDRI** | An HDRI environment | An interactive **drag-to-look equirectangular panorama** — CPU ray-tracer → `SKBitmap` |
| **Sample Model** | A glTF **3D model** | A real model rendered live with orbit + zoom — GPU (OpenGL) |

### What it does / how to use it

1. On startup the app downloads a representative texture (Poly Haven's *Red Brick*) through
   the API client and shows it wrapped on the cube. The three buttons across the top switch
   modes; the active one is highlighted.
2. **Sample HDRI** downloads and shows the *Small Cathedral* environment; **Sample Model**
   downloads and shows the *Vintage Radio Transceiver* (a glTF plus its texture/`.bin`
   side-car files). A progress bar shows while an asset is being fetched.
3. **Drag** to rotate the cube/model or look around the HDRI; **scroll** to zoom. Downloaded
   assets are cached under `LocalApplicationData/PolyHavenBrowser/cache/…`, so after the first
   run each sample loads offline.

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `PolyHavenBrowser.slnx` | Linux, macOS, Windows | The shared UI/Core, the six CodeBrix.Platform (Skia) heads, the two side libraries, and their test projects |

```
dotnet build PolyHavenBrowser.slnx
dotnet test  PolyHavenBrowser.slnx
dotnet run --project src/PolyHavenBrowser.LinuxX11
```

Unlike PainDiagram and WikipediaPublisher, PolyHavenBrowser is a **pure CodeBrix.Platform**
app — it has no native WinUI 3 / WPF heads, so there is a single cross-platform solution and
no `.Windows.slnx`. It is generated by CodeBrix.Develop's *File → New → CodeBrix.Platform
Application* experience, so its layout is exactly what that scaffolder produces.

### The six heads

One shared `MainViewModel` (in `PolyHavenBrowser.Core`) drives all of them, and they share
the XAML UI in `PolyHavenBrowser.UI`:

| Project | Platform / windowing |
| --- | --- |
| `src/PolyHavenBrowser.Win32Skia` | Windows, native Win32 window |
| `src/PolyHavenBrowser.WinWpfSkia` | Windows, Skia hosted in WPF |
| `src/PolyHavenBrowser.LinuxX11` | Linux desktop, X11 |
| `src/PolyHavenBrowser.LinuxWayland` | Linux desktop, native Wayland |
| `src/PolyHavenBrowser.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) |
| `src/PolyHavenBrowser.MacOS` | macOS |

Each head adds exactly one runtime package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever`
(1.0.189.446). The Windows-targeting Skia heads compile on Linux/macOS but only *run* on
Windows; the OpenGL 3D rendering works on every head that provides a GL/EGL stack (desktop GL
via GLX/WGL, GLES via ANGLE / Wayland EGL / the framebuffer wrapper).

### How the projects/files/folders are organized

```
PolyHavenBrowser/
├─ PolyHavenBrowser.slnx                     Cross-platform solution (open anywhere)
│
├─ src/
│   ├─ PolyHavenBrowser.UI/                  Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)                     App bootstrap + SimpleServiceResolver DI setup
│   │   └─ Views/MainPage.xaml(.cs)          The three buttons + the SKXamlCanvas + input
│   ├─ PolyHavenBrowser.Core/                Shared app library (view model, DI, and the
│   │   ├─ ViewModels/MainViewModel.cs        3D→Skia display layer)
│   │   ├─ Display/                          ★ The reusable "3D in a CodeBrix view" folder
│   │   │   ├─ RENDERING-PIPELINE.md          Architecture doc — start here to copy this
│   │   │   ├─ IModelRenderEngine.cs          The swappable graphics-backend seam
│   │   │   ├─ OpenGlModelRenderEngine.cs     OpenGL/EGL implementation (FBO + readback)
│   │   │   ├─ EglOffscreenGlContext.cs       Off-screen EGL/GLES context (create/make-current)
│   │   │   ├─ ModelScenePainter.cs           API-agnostic: input + Skia compositing
│   │   │   ├─ PanoramaScenePainter.cs        HDRI panorama (CPU → SKBitmap)
│   │   │   └─ CubeMeshBuilder.cs             Texture → a cube LoadedModel
│   │   ├─ Services/SampleAssetService.cs     Picks, downloads, and caches the sample assets
│   │   └─ PolyHavenBrowser.{six heads}/      (in src/) The six thin platform heads
│   │
│   └─ libs/                                 The two side libraries
│       ├─ PolyHavenBrowser.PolyHavenApiClient/   Typed REST client for api.polyhaven.com
│       └─ PolyHavenBrowser.Rendering/            Headless model loading + GL/CPU renderers
│
└─ tests/
    └─ libs/
        ├─ PolyHavenBrowser.PolyHavenApiClient.Tests/
        └─ PolyHavenBrowser.Rendering.Tests/      xUnit v3 + SilverAssertions (+ real headless GL)
```

The two `src/libs` assemblies each get a matching `tests/libs/*.Tests` project — the standard
CodeBrix.Platform-application layout for a sample that carries extra libraries. The heavy
lifting lives in `PolyHavenBrowser.Rendering` (glTF loading, the GL model renderer, the CPU
panorama renderer, image/EXR/HDR decoding) so it is fully decoupled from any UI and is unit
tested on its own; `PolyHavenBrowser.Core` sits on top of both libraries and adds only the
3D→Skia bridge and view model.

### How it uses the CodeBrix libraries

`PolyHavenBrowser.Rendering.csproj` (the headless rendering library):

| Package | Version | Role |
| --- | --- | --- |
| `CodeBrix.Platform.OpenGL.MitLicenseForever` | 1.0.165.1357 | The OpenGL / OpenGL ES binding used by the GL model renderer. Notably it depends on **no other `CodeBrix.Platform.*` package**, so a "side" library can reference it directly without dragging in the whole UI framework |
| `CodeBrix.Imaging.ApacheLicenseForever` | 1.0.164.1087 | Decodes downloaded texture images (JPEG/PNG/WebP) to RGBA |

(plus third-party `SharpGLTF.Runtime`, `TinyEXR.NET`, and `SkiaSharp` for glTF parsing, EXR
decoding, and bitmaps).

`PolyHavenBrowser.Core.csproj` (the UI-side app library):

| Package | Version | Role |
| --- | --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | 1.0.189.446 | The cross-platform XAML/UI framework and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`) |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | 4.148.0 | `SKXamlCanvas`, the SkiaSharp surface the 3D frame is composited onto |
| `CodeBrix.Platform.OpenGL.MitLicenseForever` | 1.0.165.1357 | Referenced directly by the `Display/` layer for the off-screen GL rendering |
| `CodeBrix.Platform.Fonts.Roboto.OflLicenseForever` | 1.0.181.661 | Bundled Roboto font for the UI |

`PolyHavenBrowser.PolyHavenApiClient` is a plain typed REST client (only `Microsoft.Extensions.Http`),
registered through `SimpleServiceResolver` at startup and resolved in the view model.

### Why it's noteworthy for a CodeBrix developer

- **Real-time 3D inside a normal app view.** This is the clearest example of taking a 3D model
  off the internet and rendering it, live and interactively, into the same Skia canvas any
  CodeBrix.Platform app draws to — OpenGL renders off-screen, the pixels are read back and
  composited onto `SKXamlCanvas`. The whole recipe (and its two easy-to-get-wrong details —
  the GL context must live on the render thread, and the model-view-projection matrix must not
  be double-transposed) is documented in
  **[`Display/RENDERING-PIPELINE.md`](PolyHavenBrowser/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md)**.
- **A swappable rendering backend.** The graphics API sits behind one interface,
  `IModelRenderEngine`, chosen via `IModelRenderEngineFactory` in DI. OpenGL/EGL is the only
  backend today, but a Vulkan engine can implement the same interface and be swapped in with a
  one-line registration change — nothing above the seam (painter, camera, loaders, UI) moves.
- **`CodeBrix.Platform.OpenGL` with no framework baggage.** Because the OpenGL package depends
  on no other `CodeBrix.Platform.*` package, the headless rendering library can reference it
  directly, keeping the dependency graph clean and the renderer usable with no UI at all.
- **The "application + extra libraries" project layout.** It shows the exact folder shape the
  CodeBrix.Platform-application scaffolder produces for a sample with side libraries:
  `src/libs/*` for the libraries and a mirrored `tests/libs/*.Tests` for each.
- **Genuinely headless GL tests.** `PolyHavenBrowser.Rendering.Tests` spins up a real
  (llvmpipe or GPU) OpenGL ES context via Mesa's surfaceless EGL platform and renders to a
  framebuffer it reads back and asserts on — including a depth-ordering regression test that
  catches the flatten-the-model MVP bug. It's a template for testing GPU code on CI with no
  window system.
- **One renderer, two subjects.** The lit textured cube and the glTF model are drawn by the
  *same* `GlModelSceneRenderer`: the texture case simply builds a cube `LoadedModel`
  (`CubeMeshBuilder`) and feeds it in, so adding a new 3D subject means producing a
  `LoadedModel`, not writing new GL.

---

## WikipediaPublisher

### What it is

**WikipediaPublisher** turns any Wikipedia article into a **book-designed, print-ready
PDF** — cover page with the article's hero image, a real table of contents with page
numbers and dot leaders, classic book typography (justified EB Garamond with first-line
indents and a raised initial), framed and numbered figures, booktabs-style tables, pull
quotes, running heads, folios, PDF outline bookmarks, and a colophon with proper CC BY-SA
attribution.

It doubles as a desktop reference application for the **CodeBrix.Platform** UI packages
and, more than any other sample, for the **document/content** side of CodeBrix:
`CodeBrix.PdfDocCreate`, `CodeBrix.MarkupParse`, and `CodeBrix.Imaging`.

### What it does / how to use it

1. **Search** — type search terms and click *Search*; the real Wikipedia search page loads
   in an embedded **WebView** so you can browse to the article you want.
2. **Choose where to save** — click *Select…* to pick the destination `.pdf` with a native
   save dialog (or type a path). Pick a page/trim size too: 8″ × 10″ coffee-table (default),
   6″ × 9″ trade book, US Letter, or A4.
3. **Publish** — the article is fetched, parsed, its images downloaded at print resolution
   (politely rate-limited), and composed into a book. Progress is reported live and the
   finished PDF lands at the chosen path (with an overwrite confirmation if needed).

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `WikipediaPublisher.slnx` | Linux, macOS, Windows | RenderArticle library + tests + the six CodeBrixPlatform (Skia) heads |
| `WikipediaPublisher.Windows.slnx` | Windows | Everything above **plus** the native WinUI 3 and WPF heads |

```
dotnet build WikipediaPublisher.slnx
dotnet test  WikipediaPublisher.slnx
dotnet run --project CodeBrixPlatform/WikipediaPublisher.LinuxX11
```

The test suite mixes offline tests (parser + book composition against an embedded HTML
fixture) with live tests that fetch the
[Cuneiform](https://en.wikipedia.org/wiki/Cuneiform) article — the same article used by
the foundational `CreateTestPdfFromOnlineArticle` test in CodeBrix.PdfDocuments.

### The eight heads

**CodeBrix.Platform (Skia) heads** share one XAML UI (`WikipediaPublisher.UI`):
`Win32Skia`, `WinWpfSkia`, `LinuxX11`, `LinuxWayland`, `LinuxFrameBuffer`, `MacOS`.
**Native heads:** `WikipediaPublisher.WinUI` (WinUI 3) and `WikipediaPublisher.Wpf` (WPF).

Every head embeds a WebView. Windows and macOS use the runtime's built-in WebView2 /
WKWebView; the **Linux** Skia heads get one from the `CodeBrix.Platform.WebView` add-in
(WPE WebKit), which needs the system engine at run time
(`sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1`). The
framebuffer head has no windowing system for a native save dialog, so you type the save
path directly.

### How the projects/files/folders are organized

```
WikipediaPublisher/
├─ WikipediaPublisher.slnx              Cross-platform solution (open anywhere)
├─ WikipediaPublisher.Windows.slnx      Windows-only solution (adds WinUI + WPF heads)
│
├─ CodeBrixPlatform/                    The Skia-based (CodeBrix.Platform UI) heads
│   ├─ WikipediaPublisher.UI/           Shared XAML UI as a shared project (.shproj)
│   │   └─ Views/MainPage.xaml          Search box, embedded WebView, path/size/Publish
│   ├─ WikipediaPublisher.Core/         Shared app library + CodeBrix.Platform packages
│   └─ WikipediaPublisher.…/            The six platform heads
│
├─ WikipediaPublisher.WinUI/            Native WinUI 3 head (Windows solution only)
├─ WikipediaPublisher.Wpf/              Native WPF head (Windows solution only)
│
├─ Shared/                              File-linked view models, helpers, test fixture
├─ WikipediaPublisher.RenderArticle/    The article → book-PDF pipeline library
│   ├─ Services/                        IArticleRenderService + implementation
│   ├─ Internal/                        WikipediaClient, ArticleParser, ImagePipeline,
│   │                                   BookComposer, BookTheme, BookFonts, …
│   └─ Fonts/                           Embedded OFL fonts (EB Garamond, Source Sans 3)
├─ Tests/
│   └─ WikipediaPublisher.RenderArticle.Tests/   xUnit v3 + SilverAssertions
└─ SampleOutput/                        Example generated PDFs
```

As with PainDiagram, one shared `MainViewModel` drives all eight heads and each head wires
its own WebView and file dialog through bridge interfaces (`IWebViewBridge`,
`IFileSaveBridge`). The heavy lifting, though, lives in a **separate reusable library**,
`WikipediaPublisher.RenderArticle` — the analog of JustBetweenUs's encryption library — so
the whole fetch → parse → images → compose → render pipeline is decoupled from any UI and
is independently unit-tested.

### The rendering pipeline

`ArticleRenderService.RenderArticleAsync()` runs five stages and reports progress
throughout:

1. **Fetch** — `WikipediaClient` retrieves the article HTML.
2. **Parse** — `ArticleParser` walks the DOM (parsed by `CodeBrix.MarkupParse`) into
   structured blocks (headings, paragraphs, images, tables, galleries, quotes), stripping
   web chrome (citation markers, edit links, navboxes, hatnotes) and dropping glyphs
   outside the embedded fonts' coverage (e.g. inline Cuneiform) rather than printing tofu
   boxes.
3. **Images** — `ImagePipeline` downloads article images at print resolution, rate-limited.
4. **Compose** — `BookComposer` builds the document object model: cover, table of contents
   with real `PageRefField`s and dot-leader tab stops, justified body with raised initial
   caps and section accent rules, numbered framed figures, booktabs tables, and a colophon.
5. **Render** — the document is rendered to PDF, complete with two-level outline bookmarks
   and metadata.

### How it uses the CodeBrix libraries

`WikipediaPublisher.RenderArticle.csproj` (the pipeline library):

| Package | Version | Role |
| --- | --- | --- |
| `CodeBrix.MarkupParse.MitLicenseForever` | 1.0.164.1248 | Parses the article HTML into a DOM the parser walks into structured content |
| `CodeBrix.PdfDocCreate.MitLicenseForever` | 1.0.174.112 | The high-level PDF document object model (styles, sections, tables, TOC fields, outlines) the book is composed with |
| `CodeBrix.Imaging.ApacheLicenseForever` | 1.0.164.1087 | Print-resolution image processing (download, resize by aspect ratio, format normalization) |

The UI side (`WikipediaPublisher.Core` + heads) uses the same `CodeBrix.Platform.*`
family as PainDiagram — `CodeBrix.Platform` (1.0.189.446), the six
`CodeBrix.Platform.Runtime.Skia.*` backends, `CodeBrix.Platform.WebView` for the Linux
WebView, `CodeBrix.Platform.Fonts.OpenSans`, and `CodeBrix.Platform.WinUI` /
`CodeBrix.Platform.WPF` (1.0.183.475) for the native heads — all driven by the Simple MVVM
toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`).

The book's own EB Garamond and Source Sans 3 typefaces are shipped as embedded resources
and registered through the CodeBrix.PdfDocuments `EmbeddedFontResolver`, so the generated
PDF looks byte-for-byte identical on every OS.

### Why it's noteworthy for a CodeBrix developer

- **The document stack, end to end.** It's the best example of chaining
  `CodeBrix.MarkupParse` → `CodeBrix.Imaging` → `CodeBrix.PdfDocCreate` to turn messy
  real-world HTML into a genuinely professional, typeset PDF — well beyond "print a
  report."
- **WebView across every platform.** It shows the WebView bridge pattern and, crucially,
  how the `CodeBrix.Platform.WebView` add-in fills in an embedded browser on the Linux
  Skia heads (WPE WebKit) where one isn't built into the runtime — referenced once in
  `.Core` so all Skia heads inherit it, and inert where a native WebView already exists.
- **Cross-platform-identical PDF output.** Embedding the fonts and resolving them through
  CodeBrix.PdfDocuments means the output is deterministic across OSes — a real concern for
  anyone generating documents on mixed dev/CI machines.
- **Pipeline decoupled and tested.** Putting the fetch/parse/compose/render pipeline in
  `WikipediaPublisher.RenderArticle` (with its own xUnit v3 + SilverAssertions test
  project, including an offline HTML fixture) is a clean template for structuring a
  CodeBrix app whose "real work" should be testable without any UI.
- **Progress-reporting long operations.** `RenderArticleAsync(RenderRequest, IProgress<RenderProgress>)`
  is a good model for wiring a genuinely long-running, network-bound operation to a
  responsive `SimpleViewModel` UI.

---

## License

The samples are licensed under the **Apache License, Version 2.0** — see
[LICENSE](LICENSE). Individual samples may carry additional attribution obligations for the
content or fonts they redistribute (e.g. WikipediaPublisher's CC BY-SA article content and
OFL fonts); see each sample's colophon and attribution notices.
