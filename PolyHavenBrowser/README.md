# PolyHavenBrowser

PolyHavenBrowser is a desktop catalog browser for Poly Haven's free, CC0-licensed 3D model
library. It opens on a **Browsing View**: the whole model catalog as a scrolling grid of
cards, each with a hero thumbnail, a title, a creator credit, a short blurb, its categories
and a compact download count. A search box (it matches name, slug, categories, tags and
author) and a sort selector (Most popular, Newest, Name A-Z) re-populate the grid, and cells
materialize in batches as the grid scrolls so that hundreds of cards and their thumbnails
are never created before anyone can see them. You choose a download folder with the folder
button, then press Download on a card; the chosen glTF and every sidecar file it references
land in a per-model subfolder, with a progress bar showing true byte progress across all of
them. A model already on disk is reused with no network traffic.

The application then switches to the **Model View**: a facts panel (categories, published
date, downloads, maximum texture size, triangles, vertices, materials, size on disk,
license) beside a live 3D preview you drag to orbit and scroll to zoom. A **Document**
button on that view generates a one-page marketing PDF about the model: the application
renders five staged product shots on the head's own off-screen GL context, samples a
per-model accent color from the catalog thumbnail, and composes a US Letter poster with
embedded fonts. **Back** returns to the catalog. PolyHavenBrowser is the reference
application for real-time, on-screen 3D inside an ordinary CodeBrix.Platform XAML page, and
for the "application plus extra library assemblies plus mirrored test projects" project
layout. (A companion application, `PolyHavenBrowser_viewer_only`, is a separate folder in
this repository with its own README.)

## What this sample shows a CodeBrix.Platform developer

- Put hardware-accelerated 3D in an ordinary page by subclassing the `GLCanvasElement` that the CodeBrix.Platform.Graphics3DGL library supplies, and binding a model to it: [Host an OpenGL scene in XAML with a GLCanvasElement subclass](../BLUEPRINTS-GraphicsAndRendering.md#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass).
- Keep the drawing itself behind an interface that names no framework types, so it is testable headlessly and reusable off-screen: [Keep the GL renderer framework-free behind an interface](../BLUEPRINTS-GraphicsAndRendering.md#keep-the-gl-renderer-framework-free-behind-an-interface).
- Reference only the higher-level graphics library and let the OpenGL binding arrive transitively, a rule this application spells out in both csproj files that touch GL: [Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS-ProjectLayoutAndPackaging.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively).
- Give a library that hosts a XAML `FrameworkElement` its own `RootNamespace`, so the per-head generated resources class does not collide across assemblies: [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- Turn pointer drag and wheel into orbit-camera motion inside the element, with pointer capture that survives leaving it: [Forward pointer input from a canvas into a model](../BLUEPRINTS-ViewsAndControls.md#forward-pointer-input-from-a-canvas-into-a-model).
- Re-frame the camera on every newly bound model so each one appears well composed at a consistent angle: [Frame the camera automatically on each newly bound model](../BLUEPRINTS-GraphicsAndRendering.md#frame-the-camera-automatically-on-each-newly-bound-model).
- Upload a `System.Numerics` matrix to a GL uniform without transposing it twice, and keep one camera convention across the on-screen and off-screen renderers: [Share one camera and one matrix convention across graphics APIs](../BLUEPRINTS-GraphicsAndRendering.md#share-one-camera-and-one-matrix-convention-across-graphics-apis).
- Run one set of shaders on heads that give you desktop OpenGL and heads that give you OpenGL ES, by probing the live context and prepending the right version header: [Pick the shader version header for desktop GL or GLES at runtime](../BLUEPRINTS-GraphicsAndRendering.md#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime).
- Draw glass and other translucent surfaces in a second pass with depth writes off, and classify transmissive materials as translucent at load time: [Draw translucent surfaces in a second pass with depth writes off](../BLUEPRINTS-GraphicsAndRendering.md#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off).
- Render high-resolution stills of the same scene on the head's own off-screen GL context, for a document or an export: [Render off screen product shots on the head own GL context](../BLUEPRINTS-GraphicsAndRendering.md#render-off-screen-product-shots-on-the-head-own-gl-context).
- Build a studio floor, backdrop and contact shadow out of the same primitive and material types the loader produces, instead of adding a second rendering path: [Generate scene set dressing as ordinary geometry](../BLUEPRINTS-GraphicsAndRendering.md#generate-scene-set-dressing-as-ordinary-geometry).
- Explain an empty 3D pane instead of leaving it blank, with the view model owning the message and the OS-specific hint: [Tell the user when graphics initialization failed](../BLUEPRINTS-PlatformServices.md#tell-the-user-when-graphics-initialization-failed).
- Decode downloaded textures to RGBA for GPU upload with the CodeBrix.Imaging library: [Decode raster images with the CodeBrix Imaging library into a Skia bitmap](../BLUEPRINTS-GraphicsAndRendering.md#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap).
- Lay an application out as a shared UI project, an application library, thin heads, `src/libs` libraries and mirrored `tests/libs` test projects in one solution: [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Materialize a card grid in batches as it scrolls, so a large remote catalog never builds all its cells up front: [Fill a grid lazily as it scrolls](../BLUEPRINTS-MVVM.md#fill-a-grid-lazily-as-it-scrolls).
- Give every cell its own command and its own asynchronously loaded thumbnail, with an application-wide gate injected as a delegate: [Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS-MVVM.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail).
- Debounce a search box in the property setter so typing stays smooth while a large collection is refiltered: [Debounce a search box before rebuilding a filtered list](../BLUEPRINTS-MVVM.md#debounce-a-search-box-before-rebuilding-a-filtered-list).
- Switch a page between two modes with one bool and computed `Visibility` properties, keeping converters out of the common case: [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).
- Write bound properties and `SimpleCommand` commands the way the family expects, including the `field` keyword form of an auto-property whose setter does work: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Refresh a command's enabled state when its gate is a private flag rather than a bound property: [Refresh CanExecute when the gating state is not a bound property](../BLUEPRINTS-MVVM.md#refresh-canexecute-when-the-gating-state-is-not-a-bound-property).
- Start the catalog fetch from the view-model constructor without awaiting it, and turn a failed load into readable text on screen: [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- Copy everything a multi-second command needs into locals before it starts, so the user can navigate away while it runs: [Snapshot view model state before a long running command](../BLUEPRINTS-MVVM.md#snapshot-view-model-state-before-a-long-running-command).
- Drive a long job from a command with a busy flag, a bound progress value and a per-stage status line: [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- Guard the view-model constructor so the XAML designer never resolves services or starts network calls: [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Gate downloading behind a chosen folder and explain the gate in a dialog rather than showing a dead button: [Gate an action behind a chosen folder and explain the gate with a dialog](../BLUEPRINTS-MVVM.md#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog).
- Hand the view model a `XamlRoot` getter through an interface as soon as the DataContext is set, so its dialogs and its off-screen GL context have somewhere to attach: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Choose a save destination through the native picker from the view model, and degrade with an explanation on a head that registers none: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Decode a percent-encoded picker path and remove the empty placeholder file the save picker leaves behind, before anything touches the disk: [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Put the real work in libraries that have no UI dependency at all and are consumed only through their interfaces: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- Build a typed REST client with source-generated JSON, its own timeout policy and its own exception types: [Build a typed REST client with source generated JSON and its own exceptions](../BLUEPRINTS-DocumentsAndData.md#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions).
- Identify yourself in a `User-Agent` and keep concurrent requests modest when you consume a free public API: [Be a polite HTTP client to a public API](../BLUEPRINTS-DocumentsAndData.md#be-a-polite-http-client-to-a-public-api).
- Fetch a whole catalog once behind double-checked locking, and cache per-item images behind a small concurrency gate: [Fetch a whole remote catalog once and cache images behind a concurrency gate](../BLUEPRINTS-DocumentsAndData.md#fetch-a-whole-remote-catalog-once-and-cache-images-behind-a-concurrency-gate).
- Report one progress fraction across a download that is really several files, using the sizes the API advertises up front: [Report true byte progress across a multi file download with side car files](../BLUEPRINTS-DocumentsAndData.md#report-true-byte-progress-across-a-multi-file-download-with-side-car-files).
- Compose a fixed-layout, absolutely placed page by drawing directly on a PDF page with the CodeBrix.PdfDocuments library: [Compose a fixed layout poster with the CodeBrix PdfDocuments library](../BLUEPRINTS-DocumentsAndData.md#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library).
- Embed OFL fonts as resources and register them with the PDF font system, so the generated document looks the same on every machine: [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).
- Record both bundled content and content downloaded at run time in a notices file next to the application: [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- Wrap a toolbar onto extra rows and flip a two-pane split between landscape and portrait with the CodeBrix.Platform.FlexPanel add-in: [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in).
- Re-key the theme's brushes so accent buttons, dialogs and the framebuffer head's built-in picker chrome all follow your palette: [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- Draw icons with `FontIcon` glyphs rather than literal symbol characters, so they survive on a device with no system fonts: [Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts).
- Write an `IValueConverter` for the cases a computed property cannot cover, such as showing a placeholder only while a bound image is null: [Format a value for display with an IValueConverter](../BLUEPRINTS-ViewsAndControls.md#format-a-value-for-display-with-an-ivalueconverter).
- Start every head from a `Program.Main` that differs from the others only in its platform call: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Do the whole application bootstrap in the `App` constructor: fonts, container, design-mode flag, then `InitializeComponent()`: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Supply `SimpleServiceResolver` with a generic host builder through a tiny provider class: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Register everything the application needs in one `AddXxx()` extension method that calls each library's own registration extension: [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- Set a bundled font as the default text font and register script fallbacks, rather than trusting whatever the host has installed: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Wire console logging into the platform's ambient logger factory before the host is built, and only in Debug builds: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Force the software render surface on the WinWpfSkia head by casting the built host: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Opt the LinuxFrameBuffer head into a folder picker, a file save picker and a software keyboard, since that head has no OS chrome: [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- Cover a real GL renderer with real tests on a machine with no window system, using a surfaceless EGL context that skips cleanly when there is none: [Test GL code headlessly with a surfaceless EGL context](../BLUEPRINTS-Testing.md#test-gl-code-headlessly-with-a-surfaceless-egl-context).
- Pin a fixed transform bug with a rotated-camera regression test whose comment says exactly why it is shaped that way: [Pin a fixed bug with a regression test that says why it is shaped that way](../BLUEPRINTS-Testing.md#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way).
- Test an HTTP client offline with a stub message handler that routes canned responses and records every request: [Test an HTTP client offline with a stub handler](../BLUEPRINTS-Testing.md#test-an-http-client-offline-with-a-stub-handler).
- Keep the small suite that talks to the real endpoints trait-gated and sharing one fixture, so the default run stays offline: [Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).
- Mock a rendering or API seam with CodeBrix.TestMocks to cover an application flow without a GPU or a file on disk: [Mock a rendering or API seam with CodeBrix TestMocks](../BLUEPRINTS-Testing.md#mock-a-rendering-or-api-seam-with-codebrix-testmocks).
- Build the binary inputs your tests need in code instead of committing them, so the loader-to-pixels path is exercised without shipping a binary fixture: [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).

## Building, running and testing

PolyHavenBrowser is a pure CodeBrix.Platform application: it has no native WinUI 3, WPF or
.NET MAUI head, so there is one solution and no separate Windows-only solution file.

| Solution | Open on | Contains |
| --- | --- | --- |
| `PolyHavenBrowser/PolyHavenBrowser.slnx` | Linux, macOS, Windows | The shared UI project, `PolyHavenBrowser.Core`, all six heads, the three libraries (in a `Libraries/` solution folder) and their three test projects (in a `Tests/` solution folder) |

The six heads:

| Head project | Platform |
| --- | --- |
| `PolyHavenBrowser/src/PolyHavenBrowser.Win32Skia` | Windows, Win32 window |
| `PolyHavenBrowser/src/PolyHavenBrowser.WinWpfSkia` | Windows, hosted in WPF |
| `PolyHavenBrowser/src/PolyHavenBrowser.LinuxX11` | Linux, X11 |
| `PolyHavenBrowser/src/PolyHavenBrowser.LinuxWayland` | Linux, Wayland |
| `PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer` | Linux framebuffer (kiosk and embedded) |
| `PolyHavenBrowser/src/PolyHavenBrowser.MacOS` | macOS |

Every head targets `net10.0` except WinWpfSkia, which targets `net10.0-windows` and sets
`EnableWindowsTargeting` so it still builds on Linux and macOS. Each head adds exactly one
CodeBrix.Platform Skia runtime package and takes everything else from
`PolyHavenBrowser.Core`.

**Prerequisites**

- The .NET 10 SDK. No workloads are needed.
- A working OpenGL stack for the 3D preview and for the Document button's product shots.
  The preview degrades gracefully: when the canvas reports that its GL initialization
  failed, the page raises a dialog explaining why the pane is empty, and on Windows it adds
  a hint about installing Microsoft's free "OpenCL and OpenGL Compatibility Pack". Document
  creation falls back to a thumbnail-led sheet when no GL context can be created at all.
- Network access. The catalog, the thumbnails, the models and the document's backdrop
  textures are all fetched from Poly Haven at run time. No account, key or token is needed.
- One thing the user supplies: a download folder, chosen through the folder picker. Nothing
  downloads until they do.

**Running one head**

```text
dotnet run --project PolyHavenBrowser/src/PolyHavenBrowser.LinuxX11
```

Substitute any other head project for `PolyHavenBrowser.LinuxX11`. The LinuxFrameBuffer
head hard-codes its picker start and restriction folders in its own `Program.cs`; change
them, or derive them from the environment, before running it on another machine.

**Tests**

There is one test project per library, under `PolyHavenBrowser/tests/libs`. All three are
xUnit v3 with SilverAssertions; two of them also use CodeBrix.TestMocks.

| Test project | Covers |
| --- | --- |
| `PolyHavenBrowser.PolyHavenApiClient.Tests` | Offline unit tests over a stub message handler (request URLs, JSON parsing, file-tree traversal, thumbnail URL building, download progress and MD5 verification, error mapping, factory and handler lifetime), mocked-interface tests, and a `Live/` folder of real-network tests sharing one fixture |
| `PolyHavenBrowser.Rendering.Tests` | Pure-CPU tests of the glTF loader, the orbit camera, the image decoder and the shot-scene builder; mocked `IModelLoader` and `IModelSceneRenderer` flows; and real-GL tests that create a headless context and read pixels back |
| `PolyHavenBrowser.CreateDocument.Tests` | The one-sheet PDF end to end (page size, document title, minimal and broken-image requests, thumbnail aspect handling, long-description overflow), plus the accent sampler and the copy builders |

`PolyHavenBrowser/global.json` selects the Microsoft.Testing.Platform runner:

```json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

With that runner selected, a plain `dotnet test` can report that it discovered zero tests.
The way that works is to build the solution and then run each test project's produced
executable directly:

```text
dotnet build PolyHavenBrowser/PolyHavenBrowser.slnx -c Release
PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/bin/Release/net10.0/PolyHavenBrowser.Rendering.Tests
```

Two categories need something extra:

- GL tests carry `[Trait("Category", "RequiresGL")]` and call `Assert.SkipWhen(...)` when no
  EGL or GL stack exists, so they skip rather than fail. They run on Linux only and need
  Mesa; llvmpipe software rendering is enough.
- Live API tests carry `[Trait("Category", "LiveApi")]` and need the network. Exclude them
  with `--filter Category!=LiveApi`.

`PolyHavenBrowser.Rendering.Tests` packages the Linux SkiaSharp native assets itself,
because a test project has no platform head to supply them.

## How the projects and folders are organized

```text
PolyHavenBrowser/
  PolyHavenBrowser.slnx                One cross-platform solution, with Libraries/ and Tests/ folders
  global.json                          Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt              Third-party attribution notices
  README.md                            This file
  src/
    PolyHavenBrowser.UI/               The shared XAML UI, as a shared project (.shproj + .projitems)
      App.xaml / App.xaml.cs           Fonts, container setup, design-mode flag, logging, first navigation
      Views/MainPage.xaml(.cs)         Browsing View, Model View, the 3D preview element, layout plumbing
    PolyHavenBrowser.Core/             The application library every head references
      ViewModels/                      MainViewModel and ModelFact, ModelCellCollection, ModelCellViewModel
      Services/                        ModelCatalogService, ModelDownloadService, ModelDescriptionBuilder
                                       and ModelFileStats, DocumentBackdropService, CatalogSortOrder
      Converters/                      NullToVisibilityConverter
      Helpers/                         HostHelper (the host-builder provider), FileDialogHelper (picker paths)
      RegisterServices.cs              One AddPolyHavenBrowser() extension registering everything
    PolyHavenBrowser.Win32Skia/        Six thin heads; each is a Program.cs plus one runtime package
    PolyHavenBrowser.WinWpfSkia/
    PolyHavenBrowser.LinuxX11/
    PolyHavenBrowser.LinuxWayland/
    PolyHavenBrowser.LinuxFrameBuffer/
    PolyHavenBrowser.MacOS/
    libs/
      PolyHavenBrowser.PolyHavenApiClient/  Typed REST client for the Poly Haven API; no UI dependency
        Models/ Exceptions/ Serialization/  Response types, typed errors, the source-generated JSON context
      PolyHavenBrowser.Rendering/           glTF loading, the GL renderer, the XAML preview element
        GL/                                 ModelSceneGlCanvas, IModelSceneRenderer, GlModelSceneRenderer
        Models/                             GltfModelLoader and IModelLoader, LoadedModel and its parts
        Cameras/                            OrbitCamera: orbit, zoom, and the framing policy
        Images/                             LdrImageDecoder, texture bytes to RGBA
        Shots/                              ModelShotRenderer, ShotSceneBuilder, ShotStage, ShotAngle
      PolyHavenBrowser.CreateDocument/      The marketing one-sheet PDF composer
        Models/ Services/ Internal/         The request object, the creator, and the composition internals
        Fonts/                              Embedded OFL fonts plus their license texts
  tests/
    libs/
      PolyHavenBrowser.PolyHavenApiClient.Tests/   Unit/, Mocked/, Live/, TestDoubles/, TestData/
      PolyHavenBrowser.Rendering.Tests/            Unit/, Mocked/, Gl/, TestDoubles/, TestData/
      PolyHavenBrowser.CreateDocument.Tests/       The one-sheet, the accent sampler, the copy builders
```

The dependency direction is strictly one way. Each head `ProjectReference`s
`PolyHavenBrowser.Core` and **file-links** the shared UI by importing
`PolyHavenBrowser.UI.projitems` with `Label="Shared"`, so `App.xaml`, `App.xaml.cs`,
`MainPage.xaml` and `MainPage.xaml.cs` compile into every head rather than into a library.
`PolyHavenBrowser.Core` `ProjectReference`s all three libraries and carries every package
the application needs, which is why each head declares only its own runtime package. Among
the libraries, `PolyHavenBrowser.Rendering` and `PolyHavenBrowser.CreateDocument` know
nothing about each other and nothing about the view models;
`PolyHavenBrowser.PolyHavenApiClient` has no UI dependency at all. Only
`PolyHavenBrowser.Rendering` references CodeBrix.Platform, because it hosts the XAML preview
element. Each library carries an `InternalsVisibleTo.cs` naming its own `.Tests` assembly,
and each test project references only the one library it covers.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework and the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `SimpleOsInfo`, the `CreateDialog`/`ShowError`/`ShowInfo` dialog helpers and `IXamlRootGetter` | `PolyHavenBrowser.Core`, `PolyHavenBrowser.Rendering`; used throughout `src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` and `src/PolyHavenBrowser.UI/App.xaml.cs` |
| CodeBrix.Platform Skia runtime for each head | One runtime package per head project, and nothing else | the six `src/PolyHavenBrowser.<Head>/*.csproj` |
| CodeBrix.Platform.Graphics3DGL | Supplies `GLCanvasElement` (the element the preview subclasses, with its cross-platform GL context, off-screen framebuffer and read-back), `GLInitializationState`, and `OffscreenGLContext` for the document's product shots; the OpenGL binding the shaders draw with arrives only transitively through it | `src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`, `.../GL/GlModelSceneRenderer.cs`, `src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform.FlexPanel add-in | The Browsing View's wrapping header toolbar, and the Model View's info-pane and viewer split whose main axis flips between landscape and portrait | `src/PolyHavenBrowser.UI/Views/MainPage.xaml`, `.../MainPage.xaml.cs` |
| CodeBrix.Platform.Fonts.Roboto | The application's default text font and its script fallbacks, set in the `App` constructor | `src/PolyHavenBrowser.UI/App.xaml.cs`, `src/PolyHavenBrowser.UI/App.xaml` |
| CodeBrix.Platform SkiaSharp views layer | Referenced by `PolyHavenBrowser.Core` for the SkiaSharp drawing surface hosted in CodeBrix.Platform XAML | `src/PolyHavenBrowser.Core/PolyHavenBrowser.Core.csproj` |
| CodeBrix.Imaging | Decodes downloaded base-color and backdrop textures to RGBA for GPU upload, and backs the PDF image pipeline and the accent-color sampler | `src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs`, `src/libs/PolyHavenBrowser.CreateDocument/Internal/AccentColorSampler.cs`, `.../Internal/SheetFonts.cs` |
| CodeBrix.PdfDocuments | Draws the marketing one-sheet directly on a PDF page, and registers the embedded fonts through its meta font resolver | `src/libs/PolyHavenBrowser.CreateDocument/Services/MarketingSheetCreator.cs`, `.../Internal/SheetComposer.cs`, `.../Internal/SheetFonts.cs` |
| CodeBrix.TestMocks | The mocking library used by the API-client and rendering test projects, in place of a third-party mocking package | `tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/`, `tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Mocked/` |
| SilverAssertions | The assertion library in all three test projects | the three `tests/libs/*/*.Tests.csproj` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SharpGLTF | Parses `.gltf` and `.glb` files (the runtime package in the library; the toolkit package in the rendering tests, for building test models in code) | `src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs` |
| SkiaSharp | Flips, downscales and PNG-encodes the off-screen product shots, and provides the `SKBitmap` form of the image decoder | `src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs`, `.../Images/LdrImageDecoder.cs` |
| Microsoft.Extensions.Http | The named, pooled client behind the Poly Haven client factory | `src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs` |
| Microsoft.Extensions.Hosting and Logging.Console | The generic host `SimpleServiceResolver` builds its container from, and the Debug-only console logger | `src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`, `src/PolyHavenBrowser.UI/App.xaml.cs` |
| xUnit v3, Microsoft.NET.Test.Sdk, SkiaSharp Linux native assets | Test infrastructure | the three `tests/libs/*/*.Tests.csproj` |

## Worth studying in this application

### The 3D preview is one XAML element, and all of it lives in a library

The Model View's preview is a single element in `MainPage.xaml`, bound to one view-model
property. Everything behind it is in `PolyHavenBrowser.Rendering`:
`GL/ModelSceneGlCanvas.cs` is a `GLCanvasElement` subclass that owns the GL lifecycle,
declares a `Model` dependency property, and translates pointer drag and wheel into camera
calls. The view model holds the parsed model as a bound property and nothing else; no
rendering code appears in the page, in the code-behind, or in the view model.

Read `GL/ModelSceneGlCanvas.cs` first, then `GL/IModelSceneRenderer.cs`, whose XML docs say
which method runs on which thread, then `GL/GlModelSceneRenderer.cs`. Three sharp edges are
worth carrying away. The element compiles its GL resources through an idempotent
`EnsureInitialized(gl)` called from both `Init(GL)` and `RenderOverride(GL)`, because a
canvas that starts collapsed can reach the first render before the init. `RenderOverride`
saves and restores the depth-test and cull-face state in a `finally`, because the head's own
Skia renderer shares the same GL context. And the base constructor's `getWindowFunc`
parameter only matters on WinUI; on CodeBrix.Platform heads you pass `null`.

See [Host an OpenGL scene in XAML with a GLCanvasElement subclass](../BLUEPRINTS-GraphicsAndRendering.md#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass),
[Keep the GL renderer framework-free behind an interface](../BLUEPRINTS-GraphicsAndRendering.md#keep-the-gl-renderer-framework-free-behind-an-interface)
and [Forward pointer input from a canvas into a model](../BLUEPRINTS-ViewsAndControls.md#forward-pointer-input-from-a-canvas-into-a-model).

### The renderer runs on every head because it asks the context what it is

The same shaders have to run on heads that hand you desktop OpenGL (Win32Skia, WinWpfSkia,
LinuxX11, MacOS) and heads that hand you OpenGL ES (LinuxWayland, LinuxFrameBuffer).
`GlModelSceneRenderer.Initialize(GL)` probes the live context's version string and prepends
either the desktop or the ES version header to shader bodies that are otherwise identical;
the fragment body opens with a precision qualifier that desktop GL accepts and GLES
requires. Compile and link failures throw with the driver's info log attached, which is far
easier to diagnose than a silently black canvas.

Two more renderer details reward reading. The two-pass draw (opaque first, then translucent
with blending on and depth writes off) is what lets glass show what is behind it, and
`Models/GltfModelLoader.cs` classifies a transmissive material as translucent even when the
file marks it opaque, because exporters write the transmission extension only for real
glass. And the model-view-projection matrix is uploaded with `transpose: false` on purpose:
`System.Numerics` stores matrices row-major, so GL reading that data as its own
column-major layout is exactly the transpose it needs, and transposing again would flatten
the depth axis for any camera that is not axis-aligned.

See [Pick the shader version header for desktop GL or GLES at runtime](../BLUEPRINTS-GraphicsAndRendering.md#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime),
[Draw translucent surfaces in a second pass with depth writes off](../BLUEPRINTS-GraphicsAndRendering.md#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off)
and [Share one camera and one matrix convention across graphics APIs](../BLUEPRINTS-GraphicsAndRendering.md#share-one-camera-and-one-matrix-convention-across-graphics-apis).

### The Browsing View: a catalog grid that never builds what it cannot show

`ModelCatalogService` fetches the entire model catalog in one call, behind double-checked
locking so a burst of startup callers makes one request, and caches thumbnails in memory
behind a small semaphore that keeps only a handful of image requests in flight. Its filter
and sort are a static, pure method, which keeps the view model free of LINQ and makes
filtering trivially testable.

`MainViewModel.RebuildCells()` runs that filter and swaps in a fresh `ModelCellCollection`.
That collection holds the whole matching list but materializes `ModelCellViewModel` cells in
batches: enough to overfill the first screen, then more on demand. Each new cell starts its
own thumbnail fetch and raises a property change when the image arrives; a failed fetch sets
a "do not retry" flag and the cell simply keeps its placeholder, which the template shows
through `NullToVisibilityConverter`.

In the MVVM shape, the batching policy belongs to the collection and the view model, and the
page contributes only the one thing it can see, which is scroll geometry: when the viewport
approaches the bottom of the extent, it asks the collection for another batch, and when a
new collection arrives it scrolls back to the top. `RequestMore` is safe to call repeatedly
and no-ops once every item has a cell, which matters because the scroll event fires often.

Read `ViewModels/ModelCellCollection.cs`, then `ViewModels/ModelCellViewModel.cs`, then the
`ItemsRepeater` and its `UniformGridLayout` in `MainPage.xaml`. See
[Fill a grid lazily as it scrolls](../BLUEPRINTS-MVVM.md#fill-a-grid-lazily-as-it-scrolls),
[Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS-MVVM.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail)
and [Fetch a whole remote catalog once and cache images behind a concurrency gate](../BLUEPRINTS-DocumentsAndData.md#fetch-a-whole-remote-catalog-once-and-cache-images-behind-a-concurrency-gate).

### A per-cell command with an application-wide gate, and a search box that debounces

The Download button is on the card, so its template binding wants to be a plain binding to
the cell's own command. `ModelCellViewModel` therefore owns the `SimpleCommand`, and the
owning view model injects both the behavior and the gate as delegates when it constructs the
cell. The cell never holds a reference to its owner, which is what keeps it independently
testable. When a download starts or finishes, `MainViewModel` tells every materialized cell
to re-query its enabled state; cells materialized later evaluate the gate fresh anyway. The
command is created lazily and the notification uses a null-conditional call, so a cell whose
button was never realized costs nothing.

The search box is the other half of the same screen. `SearchText` is an auto-property whose
setter starts a debounce and cancels the previous one, so a burst of keystrokes rebuilds the
collection once; the page's `TextBox` is an ordinary two-way binding with
`UpdateSourceTrigger=PropertyChanged`, and nothing about the debounce reaches the XAML. The
sort selector in the same file deliberately rebuilds immediately, because a discrete choice
does not need debouncing, and it is written in the classic backing-field form so the two
property shapes sit side by side in one file.

See [Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS-MVVM.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail),
[Refresh CanExecute when the gating state is not a bound property](../BLUEPRINTS-MVVM.md#refresh-canexecute-when-the-gating-state-is-not-a-bound-property),
[Debounce a search box before rebuilding a filtered list](../BLUEPRINTS-MVVM.md#debounce-a-search-box-before-rebuilding-a-filtered-list)
and [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### Downloading: gated on a folder, honest about progress, free when cached

`ModelDownloadService` picks the best glTF variant the asset offers, preferring a middle
texture resolution and falling back gracefully. Because the API advertises every file's
size up front, the service can compute one fraction across the main glTF and all its
sidecars, so the bar never jumps back to zero between files. A glTF references its sidecars
by relative path and the API keys them by exactly those paths, so the service translates
forward slashes to the platform separator and creates the intermediate directory before
each write, or the file would land somewhere the glTF cannot find it. When the model's
subfolder already holds a glTF, the service reports completion immediately and does no
network work at all.

The gate lives in the view model, not in the button. `DownloadAsync` still executes when no
download folder has been chosen; it raises a dialog explaining that the folder button at the
top of the window decides where models are saved, and returns. That is friendlier than a
dead button, and it means the explanation lives next to the rule.

See [Report true byte progress across a multi file download with side car files](../BLUEPRINTS-DocumentsAndData.md#report-true-byte-progress-across-a-multi-file-download-with-side-car-files)
and [Gate an action behind a chosen folder and explain the gate with a dialog](../BLUEPRINTS-MVVM.md#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog).

### The Document button: a staged pipeline the user can walk away from

`DocumentCommand` is a `SimpleCommand` over `CanCreateDocument()`, which is true whenever
the Model View is active and no run is already in flight. `CreateDocumentAsync()` is the
best-documented method in the application and worth reading end to end.

It opens by snapshotting everything the run needs into locals, because the user is free to
press Back or open a different model while the sheet builds. It then asks for a destination
through the native save picker, treating cancellation as a plain no-op and a head with no
picker as an explanation rather than an error. From there it runs four stages, each setting
a bound status line: fetch the CC0 backdrop textures (cached beside the downloaded models,
falling back to a plain colored floor when a texture cannot be fetched); build the three
photography sets on a worker thread; render the hero and gallery shots on the head's own
off-screen GL context; then fetch the catalog thumbnail and compose and save the PDF on a
worker thread. The busy flag is cleared and the command refreshed in a `finally`, and the
"document created" dialog is raised after that block, so the button is live again by the
time the user dismisses it.

Three sharp edges. GL work must stay on the UI thread, and the context's `MakeCurrent()`
returns a disposable that saves and restores the head's own context, so a `using` block is
the only correct shape; the same applies to the shot renderer, whose every member including
`Dispose()` must run where the context is current. Read-back pixels are bottom-up and must
be flipped before encoding. And a compatibility-first GL context may offer no multisampling,
so `ModelShotRenderer` supersamples into a framebuffer with a conservative size ceiling and
downscales, rather than relying on MSAA.

See [Snapshot view model state before a long running command](../BLUEPRINTS-MVVM.md#snapshot-view-model-state-before-a-long-running-command),
[Render off screen product shots on the head own GL context](../BLUEPRINTS-GraphicsAndRendering.md#render-off-screen-product-shots-on-the-head-own-gl-context),
[Generate scene set dressing as ordinary geometry](../BLUEPRINTS-GraphicsAndRendering.md#generate-scene-set-dressing-as-ordinary-geometry)
and [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).

### The one-sheet is a poster, drawn rather than flowed, in fonts it brings with it

`PolyHavenBrowser.CreateDocument` takes a plain request object as its whole input and has no
UI and no GL dependency, which is why the whole composition is exercised headlessly by its
test project. Because the sheet is a poster (absolute placement of shots, rules and type),
`MarketingSheetCreator` draws directly on a PDF page rather than composing a flowing
document through a document object model; the csproj says so in a comment, where a future
reader will find it. `SheetComposer.Compose()` then reads as a list of bands, and every band
clamps or truncates its own content, which is how the sheet stays exactly one page. The
creator offers both a file and a byte-array overload; the byte overload is what lets the
tests parse the result.

Two composition details are easy to hit in your own work. The graphics surface has no
character-spacing property, so the theme letterspaces an all-caps kicker by interleaving
thin spaces into the string. And the accent color is sampled from the catalog thumbnail, so
each model's sheet is tinted to the model.

`Internal/SheetFonts.cs` registers the bundled Merriweather and Roboto faces with the PDF
font system exactly once per process, behind a lock, at the top of composition. It repays
close reading, because font resolution has several gotchas. Weights beyond regular and bold
need their own family name, since the resolver distinguishes faces by looking for "bold" and
"italic" in their names and cannot tell Medium from ExtraBold. For the same reason the
heaviest face is deliberately registered under a name with no "bold" in it, so a non-bold
request for that single-face family is not treated as a bold match. Family-name lookups and
face-name lookups are different code paths, so a resolver is registered per face name rather
than once per family. And the imaging implementation must be set before any image is placed
on a page.

The font files and their license texts are both embedded resources, declared in the csproj
alongside each other.

See [Compose a fixed layout poster with the CodeBrix PdfDocuments library](../BLUEPRINTS-DocumentsAndData.md#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library)
and [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).

### The API client is a library with no idea an application exists

`PolyHavenBrowser.PolyHavenApiClient` references no CodeBrix.Platform package at all, which
is what lets its whole suite run offline. Read `IPolyHavenApiClient.cs` first: its XML docs
name the endpoint each method calls, which makes the library readable without a separate
document. Then read `RestPolyHavenApiClient.cs` for the timeout policy, which is the design
decision most worth stealing: a single client-level timeout cannot serve both small metadata
calls and large downloads, so the client-level timeout is disabled and metadata requests
apply their own linked-token timeout per call, while downloads are governed by the caller's
cancellation token. Downloads stream with headers-only completion, report progress per
buffer, and fold MD5 verification into the same read loop. A 404 maps to its own exception
type so callers can tell "missing" from "failed".

The library ships its own service-collection extension, which registers a *factory*
singleton rather than a client singleton: clients are cheap and short-lived, and disposing
one never tears down the shared connection pool. The factory also works standalone, owning
its own pooled handler, which is exactly how the tests use it. The application's
`RegisterServices.cs` calls that extension, sets an identifying `User-Agent` because Poly
Haven asks consumers to identify themselves, and adds the application's own services.

See [Build a typed REST client with source generated JSON and its own exceptions](../BLUEPRINTS-DocumentsAndData.md#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions),
[Be a polite HTTP client to a public API](../BLUEPRINTS-DocumentsAndData.md#be-a-polite-http-client-to-a-public-api)
and [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).

### Picker paths, and heads that have no picker

`Helpers/FileDialogHelper.cs` is small and load-bearing. The Linux Skia heads build the path
they hand back out of the desktop portal's `file://` URI and leave it percent-encoded, so a
folder called "My Models" arrives as `My%20Models` and every download would go to a
literally-named wrong place; accented names fare worse. `ToFileSystemPath(path)` is
deliberately conservative: it unwraps a full URI when it sees one, and otherwise decodes
only when the text really carries an escape (a percent followed by two hex digits), so a
name such as `100% done.pdf` is left alone and paths from heads that already return a plain
one pass through untouched. Every picker result in the view model goes through it
immediately. Its companion removes the zero-length placeholder file the save picker creates
at a brand-new path, and only when the file is genuinely empty, because a file with content
is one the user chose to overwrite.

The LinuxFrameBuffer head has no OS chrome, so its pickers and its software keyboard are
opt-in, one builder call each in that head's `Program.cs`. A head that registers no picker
raises `NotSupportedException`, which the view model catches specifically and turns into an
explanation instead of an error dialog. Nothing in the view model changes per head.

See [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns),
[Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model)
and [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).

### Telling the user why the 3D pane is empty

When a machine has no usable OpenGL driver, the preview would otherwise be a blank
rectangle. The view model owns the message, the OS-specific hint and the dialog, in a public
method the page can call; the page owns only the one fact it can observe, which is the
canvas's initialization state, and forwards it in a couple of lines. It has to check at two
moments, because the canvas may only attempt initialization when it loads into the visual
tree, which can happen after the view has already switched. It reports once per application
run, so navigating between models does not re-nag, and it decides whether the Windows hint
applies by asking `SimpleOsInfo` at run time rather than compiling the hint in.

See [Tell the user when graphics initialization failed](../BLUEPRINTS-PlatformServices.md#tell-the-user-when-graphics-initialization-failed)
and [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).

### The page reflows, and the theme follows the application's palette

Both views use the FlexPanel add-in. The header is a wrapping panel in which the identity
block grows to soak up free space, keeping the search, sort and folder controls pinned right
while they share its row and dropping them onto their own rows as the window narrows. The
Model View's split is a panel whose main axis flips between landscape and portrait. The
subtle part is that an explicit `Width` and a flex basis are not interchangeable: content is
measured against a `Width`, so text wraps to the pane, while a basis sizes the box without
giving the content that constraint. The sample uses a `Width` in landscape and a relative
basis in portrait, swapping them on the same element.

For color, `MainPage.xaml` re-keys the theme's accent-button brushes (including the disabled
state, because a gated command's button spends real time disabled) and bases a lightweight
style on the theme style for shaping only. Dialogs, though, open in the popup layer, which
follows the application's requested theme rather than the grid they were raised from, so the
dialog brushes are keyed at the application level in `App.xaml` instead. On the framebuffer
head that same set of keys restyles the built-in picker and software-keyboard chrome, which
resolves them too. Icons are `FontIcon` glyphs rather than literal symbol characters, and
the default text font and its script fallbacks come from the bundled font package, so the
application renders identically on a device with no system fonts at all.

See [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in),
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette)
and [Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts).

### Testing GL for real, and testing HTTP without a network

The rendering tests create a genuine headless GL context, render into a framebuffer and
assert on the pixels they read back. Two contexts are worth having: one that gives OpenGL
ES, and one that asks for a desktop core profile, because a bug that only appears on desktop
GL is invisible in an ES-only suite. Bind the core platform-display entry point rather than
its extension form, since only the former is a real exported symbol under the GLVND
dispatcher, and catch the missing-library and missing-entry-point exceptions so a machine
with no Mesa skips instead of failing. Delete every renderbuffer and framebuffer in a
`finally`. The depth-ordering test is the one to copy wholesale: it uses a rotated camera on
purpose (an axis-aligned view hides the transpose bug entirely), asserts on a single known
pixel rather than an aggregate, and runs both draw orders, with a comment that says why so
nobody simplifies it back into uselessness.

The API-client tests take the opposite approach. A stub message handler routes canned
responses by path and records every request URI, which makes "did it build the right query
string?" a one-line assertion, and an unrouted request returns a 404 naming the URL so a
missing route reads as a missing route. The library is designed to make this possible: the
factory has an internal constructor taking a message handler, reachable from the test
project through `InternalsVisibleTo`, and it never disposes that handler. The small suite
that does hit the real endpoints is trait-gated and shares one fixture, so the whole live
suite reuses a single connection pool and the default run stays offline. The rendering tests
also build their glTF fixtures in code rather than committing binary files, and the mocked
tests cover the "load, then hand to the renderer" flow with neither a GPU nor a file on
disk, which is the payoff for putting `IModelLoader` and `IModelSceneRenderer` in front of
the concrete types.

See [Test GL code headlessly with a surfaceless EGL context](../BLUEPRINTS-Testing.md#test-gl-code-headlessly-with-a-surfaceless-egl-context),
[Pin a fixed bug with a regression test that says why it is shaped that way](../BLUEPRINTS-Testing.md#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way),
[Test an HTTP client offline with a stub handler](../BLUEPRINTS-Testing.md#test-an-http-client-offline-with-a-stub-handler),
[Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run),
[Mock a rendering or API seam with CodeBrix TestMocks](../BLUEPRINTS-Testing.md#mock-a-rendering-or-api-seam-with-codebrix-testmocks)
and [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the third-party
content this application bundles or uses at run time: the Poly Haven 3D models and textures
it browses, downloads and caches, released under the Creative Commons CC0 1.0 Universal
public-domain dedication and never redistributed in this repository; and the Merriweather
and Roboto font files that `PolyHavenBrowser.CreateDocument` embeds so the generated PDF
looks the same on every machine, each licensed under the SIL Open Font License and shipped
beside its license text in `src/libs/PolyHavenBrowser.CreateDocument/Fonts/`. Third-party
code arrives as NuGet packages, each carrying its own license and notices.

## License

PolyHavenBrowser is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
