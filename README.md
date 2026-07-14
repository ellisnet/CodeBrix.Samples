# CodeBrix.Samples

A collection of complete, runnable **reference applications** for the
[CodeBrix](https://github.com/ellisnet) family of .NET libraries. Each sample is a real,
non-trivial application — not a toy snippet — built to show how the CodeBrix NuGet packages
are meant to be consumed and how a single shared view model and UI can drive many platform
"heads" (Windows, macOS, Linux — and, for JustBetweenUs, mobile) from one codebase.

Every sample follows the same house style, modeled on [JustBetweenUs](#justbetweenus): one
shared `MainViewModel` and one shared Skia XAML UI are reused across every head, business
logic lives in a separate reusable library, and each head is a thin project that supplies
only its platform-specific plumbing (windowing backend, file dialog, WebView, …).

All samples target the **.NET 10 SDK** and consume CodeBrix packages straight from
nuget.org — none of them reference a CodeBrix library as a source project, so each folder
is self-contained and can be opened and built on its own.

| Sample | What it is | Headline CodeBrix libraries |
| --- | --- | --- |
| [JustBetweenUs](#justbetweenus) | Cross-platform text-encryption utility; the flagship "one view model, nine heads" sample — the only one that also runs on **mobile** (.NET MAUI) | `CodeBrix.Platform.*` (Skia, WinUI, WPF, Mobile), `CodeBrix.SkiaSvg` |
| [PainDiagram](#paindiagram) | Interactive body-map pain/symptom annotator with highlighter-style drawing | `CodeBrix.Imaging.Drawing`, `CodeBrix.Platform.*` |
| [PolyHavenBrowser](#polyhavenbrowser) | Browses the Poly Haven 3D-model catalog and previews downloaded glTF models live with **on-screen** OpenGL | `CodeBrix.Platform.Graphics3DGL`, `CodeBrix.Imaging`, `CodeBrix.Platform.*` |
| [PolyHavenBrowser_viewer_only](#polyhavenbrowser_viewer_only) | A three-sample (texture / HDRI / model) viewer that renders 3D **off-screen** and switches between **OpenGL and Vulkan** at runtime | `CodeBrix.Platform.Graphics3DGL`, `CodeBrix.Imaging`, `CodeBrix.Platform.*` |
| [WebcamPainter](#webcampainter) | Paint on a webcam photo with **hand gestures**, via an on-device MediaPipe-style vision pipeline | `CodeBrix.VideoProcessing.OpenCV5`, `CodeBrix.Webcam`, `CodeBrix.Imaging.Drawing`, `CodeBrix.Platform.*` |
| [WikipediaPublisher](#wikipediapublisher) | Turns any Wikipedia article into a book-designed, print-ready PDF | `CodeBrix.PdfDocCreate`, `CodeBrix.MarkupParse`, `CodeBrix.Imaging`, `CodeBrix.Platform.*` |

Sections below are in alphabetical order.

---

## JustBetweenUs

### What it is

**JustBetweenUs** is a small cross-platform **text-encryption utility** — type or paste a
message, pick an algorithm, and turn it into a Base64 "just between us" string (or turn one
back into plain text). More importantly, it is the **canonical reference application for the
entire `CodeBrix.Platform` family**: the whole CodeBrix.Samples repo is styled on it. One
shared `MainViewModel` and one shared Skia XAML UI drive **nine** separate application heads,
and — uniquely among the samples — those heads span not just the six CodeBrix.Platform (Skia)
desktop backends but also **native WinUI 3**, **native WPF**, and a **.NET MAUI mobile** app.
It is the only sample in the repo that also runs on phones and tablets.

The encryption work lives in a separate, DI-registered, unit-tested library
(`JustBetweenUs.Encryption`) — the same "real work in its own reusable assembly" shape used
by WikipediaPublisher's `RenderArticle`. The app was adapted from a sample by Paul Ainsworth.

> **Note:** the encryption is real (BouncyCastle-backed) but is a demonstration, not a
> security-audited product — don't use it to protect anything truly sensitive.

### What it does / how to use it

1. Type or paste text into the input box and choose an algorithm — **AES** (secure),
   **Triple DES** (included as a deliberately obsolete/insecure example), or **Twofish**
   (very secure). The picker is bound to a `SimpleEnum`-based `EncryptionMode`.
2. **Encrypt** turns the text into a Base64 string; **Decrypt** turns a Base64 string back
   into plain text. A default encryption key ships as an embedded resource so the app works
   out of the box (loaded asynchronously on startup).
3. One-click **copy to clipboard** for the processed text (wired per-head through the
   `ICopyToClipboard` bridge).
4. The animated **star button** opens an **OS information** dialog (`SimpleOsInfo`) showing
   the platform, OS version, .NET version, user, and processor architecture — a tangible way
   to prove how many places the identical code runs.
5. SVG image buttons and a Lottie star animation render **identically** on the Skia heads and
   the native WinUI head, because both paths go through the same Skia/Skottie engines.

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `JustBetweenUs.Windows.sln` | Windows | Everything: all six CodeBrixPlatform (Skia) heads, the native WinUI, WPF and .NET MAUI (`Mobile`) heads, the encryption library and its tests |
| `JustBetweenUs.Linux.sln` | Linux | The CodeBrixPlatform (Skia) heads (all except `WinWpfSkia`), the encryption library and its tests |
| `JustBetweenUs.MacOS.sln` | macOS | The CodeBrixPlatform (Skia) heads (all except `WinWpfSkia`), the .NET MAUI (`Mobile`) head, the encryption library and its tests |

There is no bare `JustBetweenUs.sln` — the Windows solution is `JustBetweenUs.Windows.sln`.
Every project targets **.NET 10** (`net10.0` and its platform-specific TFMs). The MAUI head
needs the .NET MAUI workloads; the WinUI head builds/deploys as a packaged Windows App SDK
app (its solution platforms are restricted to x86/x64/ARM64). On some Linux ARM64 hosts the
SkiaSharp native library needs FreeType preloaded — see the comments atop
`CodeBrixPlatform/JustBetweenUs.LinuxX11/Program.cs`.

### The nine heads

One shared `MainViewModel` (in `Shared/ViewModels`, file-linked into every project) drives
all nine.

**CodeBrix.Platform (Skia) heads** — share the XAML UI in `JustBetweenUs.UI`:

| Project | Platform / windowing |
| --- | --- |
| `CodeBrixPlatform/JustBetweenUs.Win32Skia` | Windows, native Win32 window |
| `CodeBrixPlatform/JustBetweenUs.WinWpfSkia` | Windows, Skia hosted in a WPF window |
| `CodeBrixPlatform/JustBetweenUs.LinuxX11` | Linux desktop, X11 |
| `CodeBrixPlatform/JustBetweenUs.LinuxWayland` | Linux desktop, native Wayland |
| `CodeBrixPlatform/JustBetweenUs.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded, no display server) |
| `CodeBrixPlatform/JustBetweenUs.MacOS` | macOS |

**Native (non-Skia) heads** — same view model, native UI stacks:

| Project | UI stack |
| --- | --- |
| `JustBetweenUs.WinUI` | WinUI 3 (Windows App SDK) |
| `JustBetweenUs.Wpf` | WPF |
| `Mobile/JustBetweenUs.Mobile` | **.NET MAUI** (Android, iOS, Mac Catalyst, and Windows) |

The MAUI head makes JustBetweenUs the **only sample that also runs on mobile** — the same
`MainViewModel` that drives the six Skia desktop backends and the two native Windows heads
also drives an Android/iOS/Mac Catalyst app.

### How the projects/files/folders are organized

```
JustBetweenUs/
├─ JustBetweenUs.Windows.sln         All projects (open on Windows)
├─ JustBetweenUs.Linux.sln           Linux development (Skia heads, no WinWpfSkia)
├─ JustBetweenUs.MacOS.sln           macOS development (Skia heads + MAUI)
├─ THIRD-PARTY-NOTICES.txt           Third-party attribution notices
│
├─ CodeBrixPlatform/                  The Skia-based (CodeBrix.Platform UI) heads
│   ├─ JustBetweenUs.UI/             Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)             App bootstrap: SimpleServiceResolver DI + default font
│   │   └─ Views/MainPage.xaml(.cs)  The single shared page
│   ├─ JustBetweenUs.Core/           Shared library: links the shared view models, carries
│   │   │                            the CodeBrix.Platform package refs, embeds the assets
│   │   └─ Controls/                 EmbeddedImage, EmbeddedImageButton, ImagePosition
│   └─ JustBetweenUs.{Win32Skia,WinWpfSkia,LinuxX11,LinuxWayland,LinuxFrameBuffer,MacOS}/
│                                    Six thin heads, each adding one runtime package
│
├─ JustBetweenUs.WinUI/               Native WinUI 3 head (CodeBrix.Platform.WinUI.* packages)
├─ JustBetweenUs.Wpf/                 Native WPF head (CodeBrix.Platform.WPF package)
├─ Mobile/JustBetweenUs.Mobile.csproj .NET MAUI head (CodeBrix.Platform.Mobile package)
│
├─ Shared/                            File-linked source shared by every head
│   ├─ ViewModels/MainViewModel.cs    The single source of truth for all nine heads
│   ├─ ViewModels/EncryptionMode.cs   SimpleEnum-based algorithm picker
│   ├─ Helpers/                        HostHelper (IHostBuilderProvider), EmbeddedResourceHelper
│   ├─ Testing/SimpleTestFixture.cs    DI-container-backed xUnit fixture base
│   └─ Assets/                         SVG icons + star_icon.json (Lottie), embedded per head
│
├─ JustBetweenUs.Encryption/          Encryption service library (BouncyCastle)
│   ├─ Services/IEncryptionService.cs + EncryptionService.cs
│   ├─ RegisterServices.cs            AddEncryption DI extension
│   └─ Embedded/DefaultKey.txt        Embedded default key
└─ tests/
    └─ JustBetweenUs.Encryption.Tests/  xUnit v3 + SilverAssertions
```

The shared `MainViewModel.cs`, `EncryptionMode.cs`, and `HostHelper.cs` are **compiled into
each head's own assembly** via linked `<Compile Include>` items (through `JustBetweenUs.Core`
for the Skia heads, and directly in the WinUI, WPF, and MAUI heads) rather than shipped as a
library — so the exact same view-model code runs everywhere. Each head embeds the same SVG
icons under its own assembly's resource namespace, resolved at runtime by the `embedded://`
URI scheme.

### How it uses the CodeBrix libraries

`JustBetweenUs.Core.csproj` — the shared library for the six Skia heads:

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | The cross-platform Skia XAML/UI framework and the "Simple" MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `SimpleEnum`, `SimpleOsInfo`, `SimpleDialog`) |
| `CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever` | 2D (SkiaSharp) drawing integration |
| `CodeBrix.Platform.Lottie.ApacheLicenseForever` | Lottie animation (`AnimatedVisualPlayer` + `LottieVisualSource`) for the star |
| `CodeBrix.Platform.Svg.ApacheLicenseForever` | `SvgImageSource` support (vector SVG rendering) |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | SkiaSharp view/canvas integration |
| `CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever` | Bundled Open Sans font, set as the app's default text font |
| `CodeBrix.SkiaSvg.MitLicenseForever` | The underlying Skia SVG parsing/rendering engine |

Each Skia head then adds exactly one runtime backend package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever`. (The Core project also pulls third-party `SkiaSharp.Skottie`
and `Microsoft.Extensions.Hosting`.)

The native heads layer the CodeBrix toolkit packages on top of their platform's own UI stack:

| Head | Package | Role |
| --- | --- | --- |
| WinUI | `CodeBrix.Platform.WinUI.ApacheLicenseForever` | "Simple" MVVM toolkit for WinUI 3 |
| WinUI | `CodeBrix.Platform.WinUI.Skia.ApacheLicenseForever` | Skia + vector SVG image controls for native WinUI |
| WinUI | `CodeBrix.Platform.WinUI.Lottie.ApacheLicenseForever` | Skia (Skottie) Lottie player for native WinUI |
| WinUI | `CodeBrix.SkiaSvg.MitLicenseForever` | Shared Skia SVG engine (same as the Skia heads) |
| WPF | `CodeBrix.Platform.WPF.ApacheLicenseForever` | "Simple" MVVM toolkit for WPF |
| MAUI (`Mobile`) | `CodeBrix.Platform.Mobile.ApacheLicenseForever` | "Simple" MVVM toolkit for .NET MAUI |

The `JustBetweenUs.Encryption` library depends only on `BouncyCastle.Cryptography` plus
`Microsoft.Extensions.Hosting` / `Microsoft.Extensions.Logging.Abstractions` — no
CodeBrix packages at all, so the "real work" stays UI-framework-free. Its test project
(`tests/JustBetweenUs.Encryption.Tests`) uses **xUnit v3** with
`SilverAssertions.ApacheLicenseForever` and the shared `SimpleTestFixture`
DI-container pattern.

### Why it's noteworthy for a CodeBrix developer

- **The flagship "one view model, many heads" example — nine of them.**
  `Shared/ViewModels/MainViewModel.cs` is the single source of truth for *all* business logic
  and is file-linked (not NuGet-shipped) into every project. It is the definitive
  demonstration of "write the app once, host it anywhere," and — because it spans Skia,
  native WinUI, native WPF, and MAUI — it proves the pattern across four completely different
  UI stacks from one file. If you copy one thing from this repo, copy this arrangement.
- **The only mobile-capable sample.** `Mobile/JustBetweenUs.Mobile.csproj` targets
  `net10.0-android`, `-ios`, `-maccatalyst` (and `-windows`) and drives the identical
  `MainViewModel` through `CodeBrix.Platform.Mobile`. It is the reference for taking a
  CodeBrix "Simple" MVVM app onto phones/tablets without rewriting the view model — the
  business layer literally does not know it's on a phone.
- **Pixel-identical vector assets on the Skia heads *and* native WinUI.** The same SVG files
  and the same Lottie JSON render the same everywhere because both paths use Skia/Skottie. The
  reusable trick is the `embedded://AssemblyName/Resource.Name` URI scheme implemented in
  `CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs` (and `EmbeddedImageButton`):
  a drop-in `Image` subclass that loads an SVG or bitmap out of embedded resources and picks
  `SvgImageSource` vs `BitmapImage` by extension — a genuinely stealable control.
- **Clean DI wiring via `SimpleServiceResolver`.** `App.xaml.cs` does the whole container
  setup in one call: `SimpleServiceResolver.CreateInstance(HostHelper.GetHost, services => services.AddEncryption)`,
  where `Shared/Helpers/HostHelper.cs` supplies an `IHostBuilderProvider` around
  `Host.CreateDefaultBuilder` and `JustBetweenUs.Encryption/RegisterServices.cs` exposes a
  tidy `AddEncryption` extension. The view model then just calls
  `GetService<IEncryptionService>` — no per-head plumbing. (Note it also calls
  `SimpleViewModel.SetIsDesignMode(false)`, the step whose absence silently kills a scaffolded
  VM.)
- **Bridge interface for a platform-specific service.** `MainViewModel` implements
  `ICopyToClipboard { Action<string> CopyTextToClipboard }`; each head injects its own native
  clipboard delegate while the view model stays UI-agnostic and degrades gracefully with a
  "not enabled on this platform" message — the same bridge pattern PainDiagram uses for file
  save.
- **A reusable `SimpleEnum` dropdown pattern.** `EncryptionMode : SimpleEnumInfo<CryptAlgorithm>`
  pairs an enum with human-readable descriptions via `[SimpleEnum<EncryptionMode>]` attributes
  and a `GetDictionary` helper, giving a bound, descriptive algorithm picker with almost no
  code — copy it for any "pick one of N labeled options" screen.
- **Cross-platform correctness details worth stealing.** The view model sets bound properties
  through `InvokeOnMainThread(...)` (a comment documents that assigning `EncryptionKey` off the
  UI thread breaks on Linux/macOS) and uses `[AffectsCommands(...)]` so `CanExecute` refreshes
  automatically. In `EncryptionService.cs`, a `CleanBase64` guard strips non-Base64 characters
  before decoding — it exists because a stray `U+0001` control character was observed riding
  along on the clipboard→TextBox round-trip on Intel/x64 macOS. These are exactly the
  papercuts a multi-head app hits, already solved here.
- **A decoupled, testable crypto library.** AES (random IV appended to the ciphertext),
  Twofish (BouncyCastle engine, PBKDF2/SHA-3 key derivation with an appended salt), and a
  deliberately `[Obsolete]`-marked Triple DES all sit behind `IEncryptionService`, registered
  through DI and unit-tested with xUnit v3 + SilverAssertions — the template for keeping an
  app's real work independent of, and testable without, any of the nine UIs.

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
├─ THIRD-PARTY-NOTICES.txt           Third-party attribution notices
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
(`PainDiagram.Assets.body_map_master.png`), so `MainViewModel.LoadBodyMapBackground`
can load it by reflection off its own assembly and the exact same view-model code works
everywhere.

### How it uses the CodeBrix libraries

`PainDiagram.Core.csproj` pulls in the shared dependencies:

| Package | Role |
| --- | --- |
| `CodeBrix.Imaging.Drawing.ApacheLicenseForever` | The star of the sample: `DrawingSession`, named highlighter layers, live rendering, PNG export |
| `CodeBrix.Platform.ApacheLicenseForever` | The cross-platform XAML/UI framework and the Simple MVVM toolkit |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | `SKXamlCanvas`, the SkiaSharp drawing surface hosted in XAML |
| `CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever` | Bundled Open Sans font for the UI |

Each Skia head then adds exactly one platform runtime package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever` — and the native heads add `CodeBrix.Platform.WinUI.ApacheLicenseForever` /
`CodeBrix.Platform.WPF.ApacheLicenseForever` plus the matching
`SkiaSharp.Views.*` package.

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

**PolyHavenBrowser** is a **3D-model catalog browser** for [Poly Haven](https://polyhaven.com)'s free, CC0-licensed model library. It presents the whole model catalog as a scrollable grid of cards, lets you search, sort and pick a model, downloads its glTF, and then previews it in a live, interactive 3D viewer you can orbit and zoom — all inside an ordinary CodeBrix.Platform view.

It is the reference application for **rendering real-time, on-screen 3D inside a normal CodeBrix.Platform page**. The 3D preview is a `ModelSceneGlCanvas` (in `src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`) — a subclass of Graphics3DGL's `GLCanvasElement`, a real XAML `FrameworkElement` that owns its own cross-platform GL context, off-screen framebuffer and pixel read-back. The app writes **no** platform-specific native GL code and owns **no** EGL/WGL/GLX context of its own; it simply places the control and binds a model to it, and the same control renders on every head. It also ships two self-contained side libraries (a typed Poly Haven REST client and a headless rendering library), so it doubles as the reference for the CodeBrix "application + extra library assemblies + mirrored tests" project layout.

The preview is **OpenGL only** — there is no Vulkan engine and no engine-selection dropdown in this app (see [PolyHavenBrowser_viewer_only](#polyhavenbrowser_viewer_only) for the OpenGL-vs-Vulkan variant). A deliberate hard rule (documented in both csproj files) is that the app **codes to Graphics3DGL and never references `CodeBrix.Platform.OpenGL` directly**: the OpenGL binding is only ever pulled in transitively, through `CodeBrix.Platform.Graphics3DGL`.

### What it does / how to use it

1. On startup the app fetches the complete Poly Haven **models** catalog in one call and fills the **Browsing View** — a grid of 308 × 368 cards, each with a hero thumbnail (loaded asynchronously, with a quiet placeholder until it arrives), title, creator credit, a short blurb, categories and a compact download count.
2. **Search** (matches name, slug, categories, tags and author, debounced ~300 ms per keystroke) and **Sort** (*Most popular*, *Newest*, *Name A–Z*) re-populate the grid. Cells materialize lazily in batches — a first screenful, then more as you scroll toward the bottom edge — so hundreds of cards and thumbnails are never created before they can be seen.
3. Pick a **download folder** with the folder button (downloading is gated until you do — a dialog explains this if you try first), then click **Download** on a card. The chosen glTF (preferring 2k textures) and all its sidecar `.bin`/texture files download into a per-model subfolder, with a bottom progress bar showing true byte progress. Already-downloaded models are reused with no network traffic.
4. The app switches to the **Model View**: a facts panel (triangles, vertices, materials, texture size, downloads, license…) beside the interactive 3D preview. **Drag** to orbit, **scroll** to zoom; each new model re-frames the camera to a default three-quarter angle. **← Back** returns to the catalog. (A *Document* button is reserved for a future PDF-generation feature and stays disabled for now.)

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `PolyHavenBrowser.slnx` | Linux, macOS, Windows | The shared UI/Core, the six CodeBrix.Platform (Skia) heads, the two side libraries, and their two test projects |

PolyHavenBrowser is a **pure CodeBrix.Platform** app — it has no native WinUI 3 / WPF heads, so there is a single cross-platform solution and no `.Windows.slnx`. Its layout is exactly what CodeBrix.Develop's *File → New → CodeBrix.Platform Application* scaffolder produces.

### The six heads

One shared `MainViewModel` (in `PolyHavenBrowser.Core`) drives all of them, and they share the XAML UI in `PolyHavenBrowser.UI`:

| Project | Platform / windowing |
| --- | --- |
| `src/PolyHavenBrowser.Win32Skia` | Windows, native Win32 window |
| `src/PolyHavenBrowser.WinWpfSkia` | Windows, Skia hosted in WPF |
| `src/PolyHavenBrowser.LinuxX11` | Linux desktop, X11 |
| `src/PolyHavenBrowser.LinuxWayland` | Linux desktop, native Wayland |
| `src/PolyHavenBrowser.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) |
| `src/PolyHavenBrowser.MacOS` | macOS |

Each head adds exactly one runtime package — `CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever` — and inherits everything else from `PolyHavenBrowser.Core`. The Windows-targeting Skia heads compile on Linux/macOS but only *run* on Windows. The 3D preview works on every head, because the GL context, framebuffer and read-back are all supplied by Graphics3DGL's `GLCanvasElement` — the renderer's shaders target both desktop OpenGL 3.3 (WGL/GLX) and OpenGL ES 3.0 (ANGLE, Wayland EGL, the framebuffer wrapper) and pick the right `#version` header at runtime.

### How the projects/files/folders are organized

```
PolyHavenBrowser/
├─ PolyHavenBrowser.slnx                       Cross-platform solution (open anywhere)
├─ THIRD-PARTY-NOTICES.txt                     Third-party attribution notices
│
├─ src/
│   ├─ PolyHavenBrowser.UI/                    Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)                        App bootstrap + SimpleServiceResolver DI setup
│   │   └─ Views/MainPage.xaml(.cs)             Catalog grid (ItemsRepeater) + Model View +
│   │                                           the <render:ModelSceneGlCanvas> preview + input
│   │
│   ├─ PolyHavenBrowser.Core/                  Shared app library (view models, services, DI)
│   │   ├─ ViewModels/                          MainViewModel, ModelCellCollection,
│   │   │                                       ModelCellViewModel (+ ModelFact)
│   │   ├─ Services/                            ModelCatalogService, ModelDownloadService,
│   │   │                                       ModelDescriptionBuilder, CatalogSortOrder
│   │   ├─ Converters/  Helpers/                NullToVisibilityConverter, HostHelper
│   │   └─ RegisterServices.cs                  Registers the API client + catalog/download services
│   │
│   ├─ PolyHavenBrowser.{Win32Skia,WinWpfSkia,LinuxX11,LinuxWayland,LinuxFrameBuffer,MacOS}/
│   │                                           Six thin heads, each adding one runtime package
│   │
│   └─ libs/                                    The two side libraries
│       ├─ PolyHavenBrowser.PolyHavenApiClient/     Typed REST client for api.polyhaven.com
│       │   ├─ RestPolyHavenApiClient.cs, IPolyHavenApiClient.cs, factory, options, DI ext
│       │   └─ Models/  Exceptions/  Serialization/
│       └─ PolyHavenBrowser.Rendering/              Headless model loading + the on-screen GL renderer
│           ├─ GL/ModelSceneGlCanvas.cs             ★ the GLCanvasElement subclass (the preview control)
│           ├─ GL/GlModelSceneRenderer.cs           framework-free shader renderer (two-pass blend)
│           ├─ GL/IModelSceneRenderer.cs
│           ├─ Models/GltfModelLoader.cs            SharpGLTF loader (bakes transforms, glass detection)
│           ├─ Models/{LoadedModel,ModelMaterial,ModelPrimitive}.cs
│           ├─ Images/LdrImageDecoder.cs            decodes base-color textures via CodeBrix.Imaging
│           └─ Cameras/OrbitCamera.cs               orbit/zoom + FitToModel framing
│
└─ tests/
    └─ libs/
        ├─ PolyHavenBrowser.PolyHavenApiClient.Tests/   xUnit v3 + SilverAssertions (unit + mocked + live)
        └─ PolyHavenBrowser.Rendering.Tests/            xUnit v3 + SilverAssertions + real headless Mesa EGL GL
```

The two `src/libs/*` assemblies each get a matching `tests/libs/*.Tests` project — the standard CodeBrix.Platform-application layout for a sample carrying side libraries. All the heavy lifting (glTF loading, the GL renderer *and* the on-screen preview control) lives in `PolyHavenBrowser.Rendering`, decoupled from the app; `PolyHavenBrowser.Core` sits on top of both libraries and adds only the view models, catalog/download services and DI wiring. (Because `PolyHavenBrowser.Rendering` now hosts a XAML `FrameworkElement`, it deliberately keeps its **own** `RootNamespace` — not the app's — so the per-head generated `GlobalStaticResources` class doesn't collide across assemblies with CS0433.)

### How it uses the CodeBrix libraries

`PolyHavenBrowser.Core.csproj` (the UI-side app library):

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | The cross-platform XAML/UI framework and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`) |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | The SkiaSharp views layer the CodeBrix.Platform heads render through |
| `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever` | Supplies `GLCanvasElement` — the FrameworkElement the preview control subclasses (GL context + off-screen FBO + read-back). The **only** OpenGL-related package the app references; `CodeBrix.Platform.OpenGL` arrives transitively through it |
| `CodeBrix.Platform.Fonts.Roboto.OflLicenseForever` | Bundled Roboto font for the UI |

`PolyHavenBrowser.Rendering.csproj` (the headless rendering library that also hosts the preview control):

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | Supplies the `FrameworkElement` / `DependencyProperty` surface the `GLCanvasElement` subclass is built on |
| `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever` | `GLCanvasElement` plus (transitively) the `CodeBrix.Platform.OpenGL` `GL` type the shader renderer draws with — the app never PackageReferences `CodeBrix.Platform.OpenGL` itself |
| `CodeBrix.Imaging.ApacheLicenseForever` | Decodes downloaded base-color textures (JPEG/PNG/WebP) to RGBA for GPU upload |

(plus third-party `SharpGLTF.Runtime` for glTF parsing and `SkiaSharp`.)

`PolyHavenBrowser.PolyHavenApiClient.csproj` is a plain typed REST client (only `Microsoft.Extensions.Http`), registered through `SimpleServiceResolver` at startup and resolved in the services/view models.

### Why it's noteworthy for a CodeBrix developer

- **Real-time, on-screen 3D in a plain CodeBrix.Platform view — steal this whole approach.** The 3D preview is one XAML element: `<render:ModelSceneGlCanvas Model="{Binding CurrentModel}" />`. The class (`GL/ModelSceneGlCanvas.cs`) subclasses Graphics3DGL's `GLCanvasElement`, so the cross-platform GL context, off-screen framebuffer and pixel read-back are provided by the base for free — the app writes **no** native GL/EGL/WGL/GLX code of its own, and the exact same control works on all six heads (desktop GL *and* GLES). Drop a `GLCanvasElement` subclass into your own app the same way and you get hardware 3D on every platform.
- **The "code to Graphics3DGL, never to `CodeBrix.Platform.OpenGL` directly" discipline.** Both csprojs reference only `Graphics3DGL`; the OpenGL `GL` binding is a *transitive* dependency. This is a deliberate hard rule (spelled out in comments in `PolyHavenBrowser.Core.csproj` and `PolyHavenBrowser.Rendering.csproj`) that keeps the dependency graph clean and the higher-level API as the seam you build against — a pattern worth copying verbatim.
- **A catalog/grid UX that scales.** `MainViewModel` + `ModelCellCollection` + `ModelCellViewModel` show a full lazily-materializing card grid: `ItemsRepeater` with a `UniformGridLayout`, cells created in batches as the `ScrollViewer` nears its bottom (`ModelCellCollection.RequestMore`), each cell fetching its own thumbnail asynchronously through `ModelCatalogService`'s in-memory cache and a 4-at-a-time concurrency gate, with debounced search and client-side sort/filter. A ready-made template for any "browse a big remote catalog" screen.
- **glTF glass handled correctly in the preview.** `GltfModelLoader.ConvertMaterial` treats both `alphaMode = BLEND` **and** `KHR_materials_transmission` materials (marked OPAQUE but see-through — a camera lens, a clock face) as translucent; `ModelMaterial.BlendPreviewOpacity = 0.15f` gives them a fixed 15% look; and `GlModelSceneRenderer.Render` draws in **two passes** — opaque first, then blended with depth-writes off (`glDepthMask(false)`, straight-alpha "over") — so glass shows what's behind it instead of occluding it. A small, genuinely stealable recipe for translucency in a hand-written GL renderer.
- **The "application + side libraries + mirrored tests" layout.** It shows the exact shape the CodeBrix.Platform-application scaffolder produces for a sample with extra libraries: `src/libs/*` for the libraries, a mirrored `tests/libs/*.Tests` for each, all wired into one `.slnx` under `Libraries/` and `Tests/` solution folders.
- **Genuinely headless GL unit tests on CI.** `PolyHavenBrowser.Rendering.Tests` spins up a real OpenGL context — a surfaceless Mesa EGL context (llvmpipe or GPU) in `GlModelSceneRendererTests`, and the head's native desktop-GL-over-EGL path in `DesktopGlDiagnosticTests` — renders `GlModelSceneRenderer` into a framebuffer it reads back and asserts on (skipping cleanly when no GL stack exists). It's a working template for testing GPU code with no window system, including a regression test guarding the easy-to-reintroduce double-transpose-the-MVP-flattens-depth bug (see the comment in `GlModelSceneRenderer.Render`).

---

## PolyHavenBrowser_viewer_only

### What it is

**PolyHavenBrowser_viewer_only** is a focused "viewer" companion to
[PolyHavenBrowser](#polyhavenbrowser): instead of a catalog grid, it shows **three fixed
sample assets** — a PBR texture, an HDRI environment, and a glTF 3D model — one at a time on
a shared canvas, and adds the thing the catalog app doesn't have: a **runtime
rendering-engine dropdown that switches the 3D backend between OpenGL and Vulkan**. It is the
reference application for **rendering 3D content *off-screen* and compositing it onto a Skia
canvas yourself** — a model is drawn into an off-screen framebuffer with **OpenGL** (via
Graphics3DGL's cross-platform `OffscreenGLContext`) **or Vulkan** (via a self-contained
Silk.NET renderer), the pixels are read back and drawn onto an `SKXamlCanvas` in the app's
ordinary XAML, and the user can orbit/zoom it. It ships the same two side libraries as the
catalog app (a Poly Haven REST client and a headless rendering library), so it also doubles as
the reference for the CodeBrix "application with extra library assemblies" layout.

> 📄 **Want to understand (or copy) the off-screen 3D pipeline?** Start with
> **[`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`](PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md)** —
> a self-contained architecture guide (pipeline diagram, the API-agnostic seam, the
> OpenGL-vs-Vulkan class clusters, and the threading / pixel-orientation / matrix gotchas).

Three display modes share one canvas, each exercising a different rendering path:

| Button | Shows | How it's rendered |
| --- | --- | --- |
| **Sample Texture** | A texture's diffuse map | Wrapped on a lit, orbitable **cube** over a darkened backdrop of the same texture — GPU (OpenGL **or** Vulkan) |
| **Sample HDRI** | An HDRI environment | An interactive **drag-to-look equirectangular panorama** — CPU ray-tracer → `SKBitmap` (engine dropdown hidden) |
| **Sample Model** | A glTF **3D model** | A real model rendered live with orbit + zoom — GPU (OpenGL **or** Vulkan) |

### What it does / how to use it

1. On startup the app downloads a representative texture (Poly Haven's *Red Brick*) through the
   API client and shows it wrapped on the cube. The three buttons across the top switch modes;
   the active one is highlighted. A **Rendering engine** dropdown (OpenGL / Vulkan) sits beside
   them, shown in Texture and Model modes and hidden in HDRI mode (the panorama is CPU-rendered).
2. **Sample HDRI** downloads and shows the *Small Cathedral* environment; **Sample Model**
   downloads and shows the *Camera 01* model (a glTF plus its texture/`.bin` side-car files). A
   progress bar shows while an asset is being fetched.
3. **Drag** to rotate the cube/model or look around the HDRI; **scroll** to zoom. Switching the
   engine re-displays the current sample through the new backend; picking Vulkan on a platform
   that doesn't support it shows an alert and snaps back to OpenGL. Downloaded assets are cached
   under `LocalApplicationData/PolyHavenBrowser/cache/…` (keyed so re-curating a sample slug
   re-downloads rather than serving a stale asset), so after the first run each sample loads offline.

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `PolyHavenBrowser.slnx` | Linux, macOS, Windows | The shared UI/Core, the six CodeBrix.Platform (Skia) heads, the two side libraries, and their test projects |

Like the catalog app, this is a **pure CodeBrix.Platform** sample — no native WinUI 3 / WPF
heads, so there is a single cross-platform solution and no `.Windows.slnx`. It is generated by
CodeBrix.Develop's *File → New → CodeBrix.Platform Application* experience, so its layout is
exactly what that scaffolder produces.

### The six heads

One shared `MainViewModel` (in `PolyHavenBrowser.Core`) drives all of them, and they share the
XAML UI in `PolyHavenBrowser.UI`:

| Project | Platform / windowing |
| --- | --- |
| `src/PolyHavenBrowser.Win32Skia` | Windows, native Win32 window |
| `src/PolyHavenBrowser.WinWpfSkia` | Windows, Skia hosted in WPF |
| `src/PolyHavenBrowser.LinuxX11` | Linux desktop, X11 |
| `src/PolyHavenBrowser.LinuxWayland` | Linux desktop, native Wayland |
| `src/PolyHavenBrowser.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) |
| `src/PolyHavenBrowser.MacOS` | macOS |

Each head adds exactly one runtime package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever`. **OpenGL** works on every head (Graphics3DGL resolves the head's native GL
context — WGL on Windows, GLX on X11, EGL on Wayland/FrameBuffer, CGL on macOS). **Vulkan** is
offered only where the rendering library's hard-coded `VulkanPlatformSupport` allow-list okays
it — the Linux **X11**/**Wayland** heads and the two **Windows** heads; macOS (no system Vulkan
loader) and the Linux framebuffer head are deliberately excluded, and picking Vulkan elsewhere
shows an alert and reverts to OpenGL.

### How the projects/files/folders are organized

```
PolyHavenBrowser_viewer_only/
├─ PolyHavenBrowser.slnx                     Cross-platform solution (open anywhere)
├─ THIRD-PARTY-NOTICES.txt                   Third-party attribution notices
│
├─ src/
│   ├─ PolyHavenBrowser.UI/                  Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)                     App bootstrap + SimpleServiceResolver DI setup
│   │   └─ Views/MainPage.xaml(.cs)          Buttons + engine dropdown + SKXamlCanvas + input
│   ├─ PolyHavenBrowser.Core/                Shared app library (view model, DI, display layer)
│   │   ├─ ViewModels/MainViewModel.cs        Sample selection + engine switching
│   │   ├─ Display/                          ★ The reusable "3D in a CodeBrix view" folder
│   │   │   ├─ RENDERING-PIPELINE.md          Architecture doc — start here to copy this
│   │   │   ├─ IModelRenderEngine.cs          The swappable graphics-backend seam
│   │   │   ├─ IModelRenderEngineSelector.cs  Runtime backend choice + platform gate (the dropdown)
│   │   │   ├─ OpenGlModelRenderEngine.cs     OpenGL: OffscreenGLContext + FBO + readback
│   │   │   ├─ VulkanModelRenderEngine.cs     Vulkan: adapter over the self-contained renderer
│   │   │   ├─ ModelScenePainter.cs           API-agnostic: input + Skia compositing
│   │   │   ├─ PanoramaScenePainter.cs        HDRI panorama (CPU → SKBitmap)
│   │   │   └─ CubeMeshBuilder.cs             Texture → a cube LoadedModel
│   │   ├─ Services/SampleAssetService.cs     Picks, downloads, and slug-caches the samples
│   │   └─ PolyHavenBrowser.{six heads}/      (in src/) The six thin platform heads
│   │
│   └─ libs/                                 The two side libraries
│       ├─ PolyHavenBrowser.PolyHavenApiClient/   Typed REST client for api.polyhaven.com
│       └─ PolyHavenBrowser.Rendering/            Headless loaders + GL/Vulkan/CPU renderers
│           ├─ GL/GlModelSceneRenderer.cs          Shaders/VAOs/two-pass draw (shared by both APIs)
│           ├─ Vulkan/VulkanSceneRenderer.cs       Self-contained Silk.NET.Vulkan renderer
│           ├─ Vulkan/VulkanShaders.cs             SPIR-V embedded as static words (no toolchain)
│           ├─ Vulkan/VulkanPlatformSupport.cs     The hard-coded platform allow-list
│           ├─ Panorama/EquirectPanoramaRenderer.cs  CPU HDRI ray-tracer
│           ├─ ToneMapping/ + Images/               EXR/HDR/float decoders + tone mapping
│           └─ Models/GltfModelLoader.cs            glTF loading + glass (alphaMode/transmission)
│
└─ tests/
    └─ libs/
        ├─ PolyHavenBrowser.PolyHavenApiClient.Tests/
        └─ PolyHavenBrowser.Rendering.Tests/      xUnit v3 + SilverAssertions (+ real headless GL & Vulkan)
```

The two `src/libs` assemblies each get a matching `tests/libs/*.Tests` project — the standard
CodeBrix.Platform-application layout for a sample carrying extra libraries. The heavy lifting
lives in `PolyHavenBrowser.Rendering` (glTF loading, the shared GL model renderer, the whole
self-contained Vulkan stack, the CPU panorama renderer, and EXR/HDR/tone-mapping decoders), so
it is fully decoupled from any UI and unit-tested on its own; `PolyHavenBrowser.Core` adds only
the off-screen-3D→Skia bridge (the `Display/` folder) and the view model.

### How it uses the CodeBrix libraries

`PolyHavenBrowser.Rendering.csproj` (the headless rendering library):

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever` | Supplies `OffscreenGLContext` (the cross-platform off-screen native GL context) and, transitively, the `CodeBrix.Platform.OpenGL` `GL` binding the shader renderer draws with — the app codes to Graphics3DGL and **never references `CodeBrix.Platform.OpenGL` directly** |
| `CodeBrix.Imaging.ApacheLicenseForever` | Decodes downloaded texture images (JPEG/PNG/WebP) to RGBA |

(plus third-party `SharpGLTF.Runtime` for glTF, `Silk.NET.Vulkan` for the Vulkan
backend, `TinyEXR.NET` for EXR decoding, and `SkiaSharp` for bitmaps.)

`PolyHavenBrowser.Core.csproj` (the UI-side app library):

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | The cross-platform XAML/UI framework and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`) |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | `SKXamlCanvas`, the SkiaSharp surface the off-screen 3D frame is composited onto |
| `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever` | `OffscreenGLContext`, used by the `Display/` layer for the off-screen OpenGL rendering |
| `CodeBrix.Platform.Fonts.Roboto.OflLicenseForever` | Bundled Roboto font for the UI |

`PolyHavenBrowser.PolyHavenApiClient` is a plain typed REST client (only
`Microsoft.Extensions.Http`), registered through `SimpleServiceResolver` at startup and resolved
in the view model.

### Why it's noteworthy for a CodeBrix developer

- **Two graphics backends behind one seam — switchable at runtime.** Everything above
  `IModelRenderEngine` (the painter, camera, glTF loader, UI) is graphics-API-agnostic; below it
  are two independent class clusters — OpenGL and Vulkan — and `IModelRenderEngineSelector`
  chooses between them from a dropdown at run time. This is the definitive CodeBrix example of a
  swappable rendering backend, and a clean template for "offer the user a choice of engine."
- **Cross-platform *off-screen* GL with zero app-owned native code.** `OpenGlModelRenderEngine`
  gets its context from Graphics3DGL's `OffscreenGLContext`, which resolves each head's native
  GL wrapper (WGL / GLX / EGL / CGL) — so you render 3D into your own FBO, read it back, and
  composite it onto `SKXamlCanvas` yourself, and it works on **every** head — including Windows —
  with no P/Invoke to a platform GL loader in the app.
- **A self-contained Vulkan renderer with no shader toolchain.** `VulkanSceneRenderer` owns its
  entire stack (instance → device → off-screen images → pipeline → readback) via `Silk.NET.Vulkan`
  and needs no ambient context at all, so it can never collide with the head's own renderer. Its
  SPIR-V lives as static `uint[]` words in `VulkanShaders` (GLSL alongside in comments), so
  building the app needs no `glslc`/`glslangValidator` — the same pre-captured-output trick the
  CodeBrix.Platform.OpenGL bindings use.
- **The same camera matrices drive both APIs.** `RENDERING-PIPELINE.md` documents the two
  easy-to-get-wrong details the hard way: the model-view-projection matrix is uploaded **without**
  an extra transpose (System.Numerics is row-major; GL/SPIR-V reading it column-major already
  applies the transpose), and because Vulkan's clip-space Y points down its readback comes out
  bottom-up just like GL's — so both engines report `IsBottomUp` and share one Skia flip. Both
  behaviors are pinned by a depth-ordering regression test that exists **for each engine**.
- **glTF glass that actually looks like glass.** `GltfModelLoader` reads the glTF `alphaMode`
  **and** treats `KHR_materials_transmission` materials as translucent, and `GlModelSceneRenderer`
  draws a two-pass frame (opaque, then blended with depth-writes off) at a 15% preview opacity —
  so a camera lens or a clock face renders see-through instead of an opaque disc, on **both**
  backends. A compact, stealable recipe for previewing transparent materials without implementing
  full transmission/refraction.
- **A CPU HDRI path for free.** The HDRI mode skips the GPU entirely: `EquirectPanoramaRenderer`
  is a CPU ray-tracer that produces an `SKBitmap` directly (with EXR/Radiance-HDR decoding and
  tone mapping in the same library), and both painters implement `IScenePainter` so the page
  treats GPU and CPU subjects identically.
- **Genuinely headless GPU tests — for GL *and* Vulkan.** `PolyHavenBrowser.Rendering.Tests`
  spins up a real OpenGL ES context (Mesa surfaceless EGL) and a real Vulkan device (llvmpipe/ANV)
  and renders to buffers it reads back and asserts on — a template for testing GPU code on CI with
  no window system, across two APIs.

---

## WebcamPainter

### What it is

**WebcamPainter** is a **hand-gesture painting application**: you take a still photo of yourself with your webcam, then paint highlighter-style ink onto that photo by waving your open hand in front of the camera — no mouse, pen, or touch. A live **MediaPipe-style hand-tracking pipeline** runs the camera feed through **OpenCV 5** (`CodeBrix.VideoProcessing.OpenCV5`), finds your palm, decides whether your hand is open, and drives a brush stroke that is composited onto a **Skia canvas** (`SKXamlCanvas`) in the app's normal XAML.

It is the reference application for putting **real-time webcam capture and on-device computer vision inside an ordinary CodeBrix.Platform app**. Like PolyHavenBrowser it is a **pure CodeBrix.Platform** sample (no native WinUI/WPF heads) and it ships three self-contained side libraries — webcam capture, the drawing model, and the vision pipeline — so it doubles as the reference for the CodeBrix "application with extra library assemblies" project layout.

The seven selectable ink colors are ROYGBIV highlighter layers (`HighlighterPalette`), each its own translucent `CodeBrix.Imaging.Drawing` layer, so repeated passes of the "frosting-spatula" open palm over one area never darken where they cross.

### What it does / how to use it

1. On startup the app enumerates the connected cameras, fills the dropdown, and auto-starts a mirrored live preview on the first one (**Capture Mode**).
2. **Take Photo** grabs an in-memory still. The app flips into **Paint Mode**: the still becomes the painting background and the hand tracker starts. A small live self-view sits beside the painting.
3. **Show the camera your open palm** — a crosshair ring follows your palm over the photo, turning green while it is actively painting. **Close your hand or hide it** to lift the brush. Pick any of the seven rainbow colors to switch ink.
4. **Clear** starts the painting over (with a confirmation once there are more than a couple of strokes). **Save** exports the painted still as a JPEG at the photo's native resolution via a native save dialog — or to a default `Pictures` path on heads with no dialog (the framebuffer head). **Back** returns to the camera (confirming first if you'd lose a painting).

### Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `WebcamPainter.slnx` | Linux, macOS, Windows | The shared UI/Core, the six CodeBrix.Platform (Skia) heads, the three side libraries, and their test projects |

Like PolyHavenBrowser, WebcamPainter is a **pure CodeBrix.Platform** app — it has no native WinUI 3 / WPF heads, so there is a single cross-platform solution and no `.Windows.slnx`. It is generated by CodeBrix.Develop's *File → New → CodeBrix.Platform Application* experience, so its layout is exactly what that scaffolder produces (heads under `src/`, side libraries under `src/libs/`, mirrored tests under `tests/libs/`).

### The six heads

One shared `MainViewModel` (in `WebcamPainter.Core`) drives all of them, and they share the XAML UI in `WebcamPainter.UI`:

| Project | Platform / windowing |
| --- | --- |
| `src/WebcamPainter.Win32Skia` | Windows, native Win32 window |
| `src/WebcamPainter.WinWpfSkia` | Windows, Skia hosted in WPF |
| `src/WebcamPainter.LinuxX11` | Linux desktop, X11 |
| `src/WebcamPainter.LinuxWayland` | Linux desktop, native Wayland |
| `src/WebcamPainter.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) |
| `src/WebcamPainter.MacOS` | macOS |

Each head adds exactly one CodeBrix.Platform runtime package —
`CodeBrix.Platform.Runtime.Skia.{Win32,Wpf,X11,Wayland,FrameBuffer,MacOS}.ApacheLicenseForever` — **plus** the native OpenCV binaries for its OS/architecture. Because OpenCV is a native library, each head references the two matching per-RID packages: the Linux heads take `CodeBrix.VideoProcessing.OpenCV5.LinuxX64` + `.LinuxArm64`, macOS takes `.MacOSArm64` + `.MacOSX64`, and the Windows heads take `.WindowsX64` + `.WindowsArm64`. The Windows-targeting Skia heads compile on Linux/macOS but only *run* on Windows.

### How the projects/files/folders are organized

```
WebcamPainter/
├─ WebcamPainter.slnx                      Cross-platform solution (open anywhere)
├─ THIRD-PARTY-NOTICES.txt                 Third-party attribution notices
│
├─ models/                                 MediaPipe .tflite models embedded by the Vision lib
│   └─ gesture_recognizer/
│       ├─ hand_landmarker/                hand_detector.tflite + hand_landmarks_detector.tflite
│       └─ hand_gesture_recognizer/        (bundled but NOT used - see below)
│
├─ src/
│   ├─ WebcamPainter.UI/                   Shared XAML UI as a shared project (.shproj)
│   │   ├─ App.xaml(.cs)                   Bootstrap: Roboto default font + SimpleServiceResolver + SetIsDesignMode(false)
│   │   └─ Views/MainPage.xaml(.cs)        Camera dropdown, the two SKXamlCanvases, buttons; wires the bridges
│   ├─ WebcamPainter.Core/                 Shared app library
│   │   ├─ ViewModels/MainViewModel.cs     The single source of truth for all heads
│   │   └─ Helpers/                        HostHelper, FileDialogHelper
│   ├─ WebcamPainter.{six heads}/          Six thin heads (Program.cs + one runtime pkg + OpenCV natives)
│   │
│   └─ libs/                               The three side libraries
│       ├─ WebcamPainter.Webcam/           Capture: WebcamCaptureService, CameraCanvas/WebcamFrameRenderer
│       ├─ WebcamPainter.Painting/         Drawing: PaintingSession, HighlighterPalette, PaintCanvas
│       └─ WebcamPainter.Vision/           ★ The hand-tracking pipeline
│           ├─ HandTracker.cs              Worker thread, latest-frame-wins, EMA smoothing
│           └─ Internal/
│               ├─ PalmDetector.cs         MediaPipe palm-detection SSD decode (2016 anchors)
│               ├─ HandLandmarker.cs       21-landmark model via affine ROI warp
│               └─ OpenPalmClassifier.cs   Geometric open-palm + palm-center from landmarks
│
└─ tests/
    └─ libs/
        ├─ WebcamPainter.Webcam.Tests/
        ├─ WebcamPainter.Painting.Tests/
        └─ WebcamPainter.Vision.Tests/     xUnit v3 + SilverAssertions (anchor grid, palm geometry)
```

The three `src/libs` assemblies each get a matching `tests/libs/*.Tests` project — the standard CodeBrix.Platform-application layout for a sample carrying extra libraries. Each library ships an `InternalsVisibleTo.cs` so its tests can reach the `Internal/` types. `WebcamPainter.Core` sits on top of all three and adds only the view model; the heavy CV, capture, and drawing work is fully decoupled from the UI and unit-tested on its own.

### How it uses the CodeBrix libraries

`WebcamPainter.Vision.csproj` (the hand-tracking pipeline library):

| Package | Role |
| --- | --- |
| `CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever` | The managed OpenCV 5 binding. `Cv2.Dnn.ReadNetFromTFLite` runs the MediaPipe palm-detection and hand-landmark models; `Mat`, `Cv2.CvtColor`, `WarpAffine`, `GetAffineTransform`, `BlobFromImage` do the frame prep and ROI warp |

The library **embeds** two `.tflite` models from Google's `gesture_recognizer.task` bundle — `hand_detector.tflite` and `hand_landmarks_detector.tflite` — as manifest resources loaded by reflection at startup. (The bundle's gesture-classifier stages are deliberately *not* used: OpenCV's TFLite importer can't load their `GATHER` op, so open-palm detection is computed geometrically instead — see below.)

`WebcamPainter.Webcam.csproj` (the capture library):

| Package | Role |
| --- | --- |
| `CodeBrix.Webcam.LgplLicenseForever` | Camera enumeration (`WebcamDevices`), the live `WebcamSession` (BGRA `FrameReceived`, `TryCopyLatestFrame`), and in-memory `CapturePhoto` |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | `SKXamlCanvas`, subclassed as `CameraCanvas` to host the live video surface in XAML |

`WebcamPainter.Painting.csproj` (the drawing library):

| Package | Role |
| --- | --- |
| `CodeBrix.Imaging.Drawing.ApacheLicenseForever` | `DrawingSession` with one highlighter layer per ROYGBIV color, normalized-coordinate stroke input, and native-resolution JPEG export; brings `CodeBrix.Imaging` + SkiaSharp |
| `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | `SKXamlCanvas`, subclassed as `PaintCanvas` for the painting surface |

`WebcamPainter.Core.csproj` (the UI-side app library):

| Package | Role |
| --- | --- |
| `CodeBrix.Platform.ApacheLicenseForever` | The cross-platform XAML/UI framework and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `[AffectsCommands]`, `SimpleServiceResolver`, `InvokeOnMainThread`) |
| `CodeBrix.Platform.Fonts.Roboto.OflLicenseForever` | Bundled Roboto font, set as the app's default text font |

(Core also pulls `Microsoft.Extensions.Hosting` / `Microsoft.Extensions.Logging.Console` for the host + logging.) Each head then adds its one `CodeBrix.Platform.Runtime.Skia.*` backend and the two per-RID `CodeBrix.VideoProcessing.OpenCV5.*` native packages.

### Why it's noteworthy for a CodeBrix developer

- **A complete on-device computer-vision pipeline you can lift wholesale.** `WebcamPainter.Vision` is a from-scratch, dependency-light re-implementation of MediaPipe hand tracking on top of `CodeBrix.VideoProcessing.OpenCV5`'s DNN module — the kind of thing usually assumed to require Python. Steal the whole `Internal/` folder.
- **The palm-detection SSD decode is the clever bit.** `PalmDetector` regenerates MediaPipe's fixed **2016-anchor** grid in its static ctor (a 24×24 grid × 2 anchors at stride 8 plus a 12×12 grid × 6 at stride 16 for the 192×192 input), letterboxes the frame, runs `Identity_1`/`Identity` *separately* so the no-hand case early-outs before ever reading the large box tensor, sigmoids the per-anchor logits, then applies MediaPipe's exact rect transform — rotate wrist→middle-finger vertical, expand 2.6×, shift −0.5 box toward the fingers — to produce a rotated hand ROI. All the constants are documented inline.
- **A rotated-ROI affine warp feeds the landmark model.** `HandLandmarker` builds a three-corner `GetAffineTransform` from the detector's rotated ROI, `WarpAffine`s the hand upright into the model's 224×224 input, reads the 21 landmarks plus a presence probability in one `ForwardAll("Identity","Identity_1")` pass, and projects the landmarks back into original frame pixels through the same rotation. Note the "presence is already a probability — do not sigmoid" comment; that's a real MediaPipe sharp edge.
- **Gestures without the gesture model.** Because the bundled classifier's `GATHER` op won't import, `OpenPalmClassifier` decides "open palm" purely from landmark geometry — a fingertip counts as extended when it is 1.1× farther from the wrist than its PIP joint; a curled finger folds back and the ratio drops below 1. `GetPalmCenter` averages the wrist + four MCP knuckles. It's fast, explainable, and fully unit-tested (`OpenPalmClassifierTests`, `PalmDetectorTests`).
- **Heavy CV decoupled from the UI by a worker thread with latest-frame-wins.** `HandTracker` runs inference on its own background thread; `SubmitFrame` copies pixels under a lock and silently replaces any un-processed pending frame, so a slow model never blocks capture — submitting faster than it can process just drops stale frames. It even swaps the pending/working buffers under the lock for a copy-free hand-off, and applies an EMA (`SmoothingAlpha`) so the brush doesn't jitter. Its shutdown `catch (…) when (!_running)` swallows the OpenCV native-teardown race at process exit rather than crashing the app.
- **A three-thread hand-off done cleanly.** Frames arrive on the **capture thread** (`WebcamCaptureService.FrameArrived`), inference runs on the **tracker thread**, and every painting decision is marshalled to the **UI thread** via `InvokeOnMainThread` in `MainViewModel.OnTrackingUpdated` before it touches the `DrawingSession`. This is a textbook example of keeping a `SimpleViewModel` responsive under a real-time sensor load.
- **Live video + vision-driven ink composited onto one Skia canvas.** `WebcamFrameRenderer` blits the latest BGRA frame aspect-fit and selfie-mirrored, while `PaintCanvasHelper` renders the painting session plus a brush-sized crosshair ring (green while painting, with a dark halo so it stays visible over any photo). The same `SKXamlCanvas` shows live preview in Capture Mode and the painting in Paint Mode — the canonical "draw anything you want into a normal CodeBrix.Platform view" recipe.
- **Normalized, resolution-independent brush input.** The hand tracker emits palm positions in 0..1 image coordinates and `PaintingSession` drives `DrawingSession.PointerPressedNormalized` / `MovedNormalized` directly — no view size, DPI, or letterbox math needed at input time — so `ExportJpeg` renders at the photo's native resolution regardless of on-screen canvas size, and the drawing library stays headlessly testable.
- **The "application + three libraries" project shape.** A clean demonstration of the scaffolder's `src/libs/*` + `tests/libs/*.Tests` layout, each library with its own `InternalsVisibleTo.cs`, and the correct way to fan native `CodeBrix.VideoProcessing.OpenCV5.*` RID packages out across the per-platform heads while the managed binding stays in the shared library.

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
├─ THIRD-PARTY-NOTICES.txt              Third-party attribution notices
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

`ArticleRenderService.RenderArticleAsync` runs five stages and reports progress
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

| Package | Role |
| --- | --- |
| `CodeBrix.MarkupParse.MitLicenseForever` | Parses the article HTML into a DOM the parser walks into structured content |
| `CodeBrix.PdfDocCreate.MitLicenseForever` | The high-level PDF document object model (styles, sections, tables, TOC fields, outlines) the book is composed with |
| `CodeBrix.Imaging.ApacheLicenseForever` | Print-resolution image processing (download, resize by aspect ratio, format normalization) |

The UI side (`WikipediaPublisher.Core` + heads) uses the same `CodeBrix.Platform.*`
family as PainDiagram — `CodeBrix.Platform`, the six
`CodeBrix.Platform.Runtime.Skia.*` backends, `CodeBrix.Platform.WebView` for the Linux
WebView, `CodeBrix.Platform.Fonts.OpenSans`, and `CodeBrix.Platform.WinUI` /
`CodeBrix.Platform.WPF` for the native heads — all driven by the Simple MVVM
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
[LICENSE](LICENSE). Each sample folder also carries its own `THIRD-PARTY-NOTICES.txt`.
Individual samples may carry additional attribution obligations for the content or fonts
they redistribute (e.g. WikipediaPublisher's CC BY-SA article content and OFL fonts); see
each sample's individual THIRD-PARTY-NOTICES.txt file.
