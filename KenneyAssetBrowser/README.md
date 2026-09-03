# KenneyAssetBrowser

KenneyAssetBrowser is a desktop browser for downloaded kenney.nl game-asset packs.
The user points it at a folder of asset-pack `.zip` files; the application reads each
zip without extracting it, works out the pack's identity from the `License.txt` inside,
and shows the packs as cards in a sidebar. Selecting a pack fills a grid with everything
in it, grouped for browsing: each 3D model is one card no matter how many format variants
the kit ships, each spritesheet is one card rather than an image plus an XML file, and
everything else is its own card with a kind chip and a thumbnail. A search box and a
category filter narrow the grid. Opening a card switches the page to a viewer. Images,
SVG art, font specimens and composited Tiled maps go to a zoomable Skia canvas, with a
transparency checkerboard behind them and a spotlight for a selected sprite region; 3D
models go to an interactive OpenGL preview that rotates with a drag, zooms with the wheel
and plays the model's animations; audio clips get a transport with a scrubber; text files
get a text pane. A facts panel beside the viewer lists the asset's dimensions, formats,
size and the pack it came from. Asset packs ship in `sample_asset_bundles/`, so the
application has something to browse before the user has downloaded anything.

As a reference, this is the sample for reading a container format on a worker thread and
previewing everything found inside it: zip access through CodeBrix.Compression, raster
decode through CodeBrix.Imaging, vector rasterization through CodeBrix.SkiaSvg, an OpenGL
scene hosted in XAML through the CodeBrix.Platform.Graphics3DGL add-in, audio played
straight from bytes through the CodeBrix.Platform.AudioPlayer add-in, a wrapping and
orientation-aware layout through the CodeBrix.Platform.FlexPanel add-in, and persisted
user choices through the CodeBrix.Platform.AppSettings add-in behind a one-file facade.

## What this sample shows a CodeBrix.Platform developer

- Reading individual members of a zip archive on demand, without unpacking it to disk:
  [Read a zip archive without extracting it with the CodeBrix Compression library](../BLUEPRINTS.md#read-a-zip-archive-without-extracting-it-with-the-codebrix-compression-library).
- Turning the many files in a container into fewer, more meaningful browsing items:
  [Classify and group the contents of a container for browsing](../BLUEPRINTS.md#classify-and-group-the-contents-of-a-container-for-browsing).
- Resolving a reference one archive member makes to a sibling (a model to its texture, a
  map to its tileset, a tileset to its image):
  [Resolve a file that another archive entry references by relative path](../BLUEPRINTS.md#resolve-a-file-that-another-archive-entry-references-by-relative-path).
- Keeping every blocking read inside a registered service that only returns tasks:
  [Do blocking work in a service behind Task Run](../BLUEPRINTS.md#do-blocking-work-in-a-service-behind-task-run).
- Parsing a model off the UI thread while its side files are resolved back into the same
  archive:
  [Load an asset off the UI thread and resolve its side files from the same container](../BLUEPRINTS.md#load-an-asset-off-the-ui-thread-and-resolve-its-side-files-from-the-same-container).
- Putting an interactive OpenGL scene in a page as a bindable control:
  [Host an OpenGL scene in XAML with a GLCanvasElement subclass](../BLUEPRINTS.md#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass).
- Keeping the shader code testable and free of any XAML type:
  [Keep the GL renderer framework-free behind an interface](../BLUEPRINTS.md#keep-the-gl-renderer-framework-free-behind-an-interface).
- Compiling one shader body against both desktop OpenGL and OpenGL ES contexts:
  [Pick the shader version header for desktop GL or GLES at runtime](../BLUEPRINTS.md#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime).
- Playing a model's animation by baking it to vertex frames instead of teaching the
  renderer about skinning:
  [Play a baked animation clip in a preview canvas](../BLUEPRINTS.md#play-a-baked-animation-clip-in-a-preview-canvas).
- Explaining an empty 3D pane on a machine with no usable driver, instead of looking
  broken:
  [Tell the user when graphics initialization failed](../BLUEPRINTS.md#tell-the-user-when-graphics-initialization-failed).
- Driving a Skia drawing surface entirely from view-model state, including its zoom:
  [Paint a zoomable image on an SKXamlCanvas from the view model](../BLUEPRINTS.md#paint-a-zoomable-image-on-an-skxamlcanvas-from-the-view-model).
- Highlighting one named sub-rectangle of a spritesheet in place:
  [Spotlight one region of an image on the canvas](../BLUEPRINTS.md#spotlight-one-region-of-an-image-on-the-canvas).
- Rasterizing vector art at both viewer size and thumbnail size:
  [Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS.md#rasterize-svg-art-with-the-codebrix-skiasvg-library).
- Decoding image bytes of an unknown format into a bitmap for display and into raw RGBA
  for a texture upload:
  [Decode raster images with the CodeBrix Imaging library into a Skia bitmap](../BLUEPRINTS.md#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap).
- Playing audio held in memory without writing a temporary file:
  [Play an audio clip straight from bytes with the AudioPlayer add-in](../BLUEPRINTS.md#play-an-audio-clip-straight-from-bytes-with-the-audioplayer-add-in).
- Making one Play button replay a clip that has already run to its end:
  [Replay a finished audio clip with one button press](../BLUEPRINTS.md#replay-a-finished-audio-clip-with-one-button-press).
- Binding a position slider and timecode labels to the media element itself, the one place
  where that beats routing through the view model:
  [Bind a scrubber and volume slider straight to the media element](../BLUEPRINTS.md#bind-a-scrubber-and-volume-slider-straight-to-the-media-element).
- Formatting a bound value for display with a one-way converter:
  [Format a value for display with an IValueConverter](../BLUEPRINTS.md#format-a-value-for-display-with-an-ivalueconverter).
- Building a header row and a two-pane split that reflow with the window, with no
  breakpoints:
  [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in).
- Re-keying the theme's brushes so stock controls follow the application's own palette:
  [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- Materializing a large grid a batch at a time as the user scrolls:
  [Fill a grid lazily as it scrolls](../BLUEPRINTS.md#fill-a-grid-lazily-as-it-scrolls).
- Giving every grid cell its own command and its own lazily fetched thumbnail:
  [Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail).
- Waiting for typing to settle before rebuilding a filtered list:
  [Debounce a search box before rebuilding a filtered list](../BLUEPRINTS.md#debounce-a-search-box-before-rebuilding-a-filtered-list).
- Repopulating a filter list without the control commanding a rebuild back:
  [Stop a two way bound selection from commanding the control back](../BLUEPRINTS.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).
- Switching between two whole page modes, and between five viewer panes, with computed
  visibility properties instead of navigation:
  [Show and hide panes with computed Visibility properties](../BLUEPRINTS.md#show-and-hide-panes-with-computed-visibility-properties).
- Letting the view model raise dialogs by taking a XamlRoot getter from the page once:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Using one error surface for a whole dispatching open path:
  [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- Wrapping the settings add-in in one application-named facade nothing else bypasses:
  [Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS.md#wrap-the-appsettings-add-in-in-one-application-named-facade).
- Opening the settings store before any UI renders, because a view model reads a setting
  in its constructor:
  [Open the settings store before any other startup work](../BLUEPRINTS.md#open-the-settings-store-before-any-other-startup-work).
- Asking for a folder once and remembering it for every later run:
  [Choose a folder with the picker and remember it across runs](../BLUEPRINTS.md#choose-a-folder-with-the-picker-and-remember-it-across-runs).
- Gating the whole catalog behind that chosen folder and swapping in a first-launch
  prompt until it exists:
  [Gate an action behind a chosen folder and explain the gate with a dialog](../BLUEPRINTS.md#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog).
- Guarding the view-model constructor so the XAML designer does not run the live path:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Starting the catalog load from the constructor without making it async:
  [Kick off async startup loading from the view model constructor](../BLUEPRINTS.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- Writing the entry point of a head, where the platform call is the only line that
  changes:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Ordering font configuration, container, design-mode flag, settings store and
  `InitializeComponent()` in the `App` constructor:
  [Bootstrap the application in the App constructor](../BLUEPRINTS.md#bootstrap-the-application-in-the-app-constructor).
- Handing `SimpleServiceResolver` a host builder from a small provider class so the UI
  project needs no hosting reference:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Declaring the application's services in one extension method instead of a growing
  lambda:
  [Register library services with one AddXxx extension method](../BLUEPRINTS.md#register-library-services-with-one-addxxx-extension-method).
- Wiring a console logger that exists only in Debug builds and quiets the framework's own
  categories:
  [Turn on console logging only in Debug builds](../BLUEPRINTS.md#turn-on-console-logging-only-in-debug-builds).
- Setting a bundled serif face as the default text font with script fallbacks behind it:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Opting the LinuxFrameBuffer head into a folder picker and a software keyboard, since it
  has no desktop chrome to borrow them from:
  [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- Asking the WinWpfSkia head for a software render surface after the host is built:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Giving a library that hosts a XAML control its own root namespace, so the generated
  per-head resources class does not collide:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- Referencing the graphics add-in and letting the low-level binding arrive transitively:
  [Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively).
- Setting up a test project for a library the family way:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Adding the native library a running head would have supplied, so bitmap tests can draw
  with no application around them:
  [Add the native assets a head would have supplied](../BLUEPRINTS.md#add-the-native-assets-a-head-would-have-supplied).
- Building the zips, PNGs and GLB models the tests need in memory instead of committing
  binary fixtures:
  [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).
- Testing a store that owns its own files by giving every test its own throwaway folder:
  [Point a process-global store at a throwaway folder in tests](../BLUEPRINTS.md#point-a-process-global-store-at-a-throwaway-folder-in-tests).

## Building, running and testing

There is one solution, `KenneyAssetBrowser.slnx`, and its own comment says it holds
"everything that builds with the plain .NET SDK on Linux, macOS and Windows" - so it opens
on any of the three. It contains the shared UI project, the Core project, all six head
projects, a `Libraries` solution folder for the three `src/libs` projects, and a `Tests`
solution folder for their test projects. There is no second, operating-system-restricted
solution.

| Head project | Platform |
| --- | --- |
| `src/KenneyAssetBrowser.LinuxX11` | Linux, X11 |
| `src/KenneyAssetBrowser.LinuxWayland` | Linux, Wayland |
| `src/KenneyAssetBrowser.LinuxFrameBuffer` | Linux framebuffer, no desktop session |
| `src/KenneyAssetBrowser.MacOS` | macOS |
| `src/KenneyAssetBrowser.Win32Skia` | Windows, native Win32 window |
| `src/KenneyAssetBrowser.WinWpfSkia` | Windows, Skia rendering hosted in a WPF window |

All six are Skia heads; there are no native (WinUI 3, WPF, .NET MAUI) heads. Five target
`net10.0`. `KenneyAssetBrowser.WinWpfSkia` targets `net10.0-windows` and sets
`EnableWindowsTargeting`, so the solution still restores and builds on Linux and macOS.

Prerequisites:

- The .NET 10 SDK. No workloads.
- A session appropriate to the head: an X11 display for LinuxX11, a Wayland compositor for
  LinuxWayland, a Linux virtual console with framebuffer and input access for
  LinuxFrameBuffer, macOS for MacOS, Windows for the two Windows heads.
- A working OpenGL context for the 3D preview. `GlModelSceneRenderer` states that it runs
  against desktop OpenGL 3.3 and OpenGL ES 3.0 contexts, which is what the heads supply.
  Where initialization fails, the application says so in a dialog and everything else keeps
  working.
- No accounts, tokens or network access. Nothing is downloaded at run time.
- Data the user supplies: a folder of Kenney asset-pack `.zip` files. On first run the page
  shows a prompt whose button raises `MainViewModel.PickFolderCommand`, which opens a
  `FolderPicker` starting at the documents library; the chosen path is written to settings
  under `AssetsFolderKey` and reused on every later run, and the folder button in the header
  raises the same command to change it. `AssetFolderCatalog.LoadFrom` then enumerates the
  `*.zip` files directly inside that folder (it does not recurse). Pointing the picker at
  this folder's own `sample_asset_bundles` gives you a working catalog immediately.

Run one head from the command line, from this application folder:

```text
dotnet run --project src/KenneyAssetBrowser.LinuxX11/KenneyAssetBrowser.LinuxX11.csproj
```

`global.json` in this folder contains nothing but a test-runner selection - it sets the
runner to `Microsoft.Testing.Platform`. Every test project is an `Exe` with
`UseMicrosoftTestingPlatformRunner` set, so a plain `dotnet test` on the solution can
report that no tests were found. Run each test project instead, either through the SDK or
by executing the binary it builds:

```text
dotnet run --project tests/libs/KenneyAssetBrowser.AssetRead.Tests/KenneyAssetBrowser.AssetRead.Tests.csproj
dotnet run --project tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj
dotnet run --project tests/libs/KenneyAssetBrowser.Settings.Tests/KenneyAssetBrowser.Settings.Tests.csproj
```

The tests need nothing beyond a filesystem: no display, no GPU, no network. They build
their own inputs - synthetic zips, PNGs encoded from Skia bitmaps, tiny in-memory glTF
binaries - and delete their temporary folders afterwards.

## How the projects and folders are organized

```text
KenneyAssetBrowser/
  KenneyAssetBrowser.slnx                 The one solution; opens on Linux, macOS and Windows
  global.json                             Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt                 Notices for the bundled asset packs
  sample_asset_bundles/                   Kenney asset packs (.zip, CC0) to browse out of the box
  src/
    KenneyAssetBrowser.UI/                Shared project: App.xaml(.cs) and Views/MainPage.xaml(.cs)
    KenneyAssetBrowser.Core/              The library every head references; carries the packages
      Helpers/                            HostHelper (host-builder provider), FormatHelper (byte and count text)
      Services/                           AssetCatalogService: the gateway to the AssetRead library
      ViewModels/                         MainViewModel, the cell view models, the two bridge interfaces
      RegisterServices.cs                 The AddKenneyAssetBrowser() DI extension method
    KenneyAssetBrowser.LinuxX11/          Head: Program.cs plus one runtime package
    KenneyAssetBrowser.LinuxWayland/      Head: Program.cs plus one runtime package
    KenneyAssetBrowser.LinuxFrameBuffer/  Head: Program.cs, opting into the folder picker and software keyboard
    KenneyAssetBrowser.MacOS/             Head: Program.cs plus one runtime package
    KenneyAssetBrowser.Win32Skia/         Head: Program.cs plus one runtime package
    KenneyAssetBrowser.WinWpfSkia/        Head: net10.0-windows, asks for a software render surface
    libs/
      KenneyAssetBrowser.AssetRead/       Zip reading and Kenney pack parsing; no UI types at all
        Models/                           AssetBundle, AssetEntry, AssetKind, ModelAsset, SpriteAtlas, SpriteRegion
        Parsing/                          AssetClassifier, KenneyNames, SpriteAtlasParser
      KenneyAssetBrowser.Rendering/       Everything that draws: Skia, glTF, OpenGL, Tiled
        Cameras/                          OrbitCamera
        GL/                               ModelSceneGlCanvas, IModelSceneRenderer, GlModelSceneRenderer
        Images/                           ImageCanvasPainter, LdrImageDecoder, SvgImageDecoder, FontSpecimenRenderer
        Models/                           IModelLoader, GltfModelLoader, LoadedModel, AnimatedModel, the clip types
        Tiled/                            TiledMapParser, TiledMapDocument, TiledMapRenderer
      KenneyAssetBrowser.Settings/        SettingsService and LoggingService facades over the AppSettings add-in
  tests/
    libs/
      KenneyAssetBrowser.AssetRead.Tests/ Mirrors AssetRead; builds synthetic zip archives
      KenneyAssetBrowser.Rendering.Tests/ Mirrors Rendering; builds in-memory GLB models and PNGs
      KenneyAssetBrowser.Settings.Tests/  Mirrors Settings; drives the store in temporary folders
```

Dependencies point one way. Each head project references `KenneyAssetBrowser.Core` by
project reference and file-links the shared UI with
`<Import Project="..\KenneyAssetBrowser.UI\KenneyAssetBrowser.UI.projitems" Label="Shared" />`,
so `App.xaml` and `MainPage.xaml` compile into every head rather than into an assembly of
their own. Core project-references all three `src/libs` libraries and carries every package
that is not a head runtime; each head adds exactly one runtime package for its own platform,
and the comment saying so is in all six head project files. The three libraries reference
neither Core nor each other: `KenneyAssetBrowser.AssetRead` knows nothing about UI,
`KenneyAssetBrowser.Rendering` references CodeBrix.Platform only because it hosts the GL
canvas control, and `KenneyAssetBrowser.Settings` only wraps the settings add-in. Each test
project references only the one library it mirrors.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The framework itself: `Application`, `Window`, `Frame`, `Page`, the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `SimpleOsInfo`, the dialog helpers), the folder picker, and the element and dependency-property surface the GL canvas subclass derives from | `src/KenneyAssetBrowser.Core/KenneyAssetBrowser.Core.csproj`, `src/libs/KenneyAssetBrowser.Rendering/KenneyAssetBrowser.Rendering.csproj` |
| CodeBrix.Platform Skia runtime for each head | The per-platform runtime; exactly one package per head project | the six `src/KenneyAssetBrowser.<Head>/*.csproj` |
| CodeBrix.Platform Merriweather font package | Supplies Merriweather as the default text font, with the Noto Serif faces as fallbacks | `src/KenneyAssetBrowser.UI/App.xaml.cs` |
| CodeBrix.Platform.FlexPanel add-in | The wrapping header row of the Browsing View, and the viewer/facts split whose main axis flips with window orientation | `src/KenneyAssetBrowser.UI/Views/MainPage.xaml`, `src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs` |
| CodeBrix.Platform SkiaSharp views package | `SKXamlCanvas`, the drawing surface the 2D viewer paints on | `src/KenneyAssetBrowser.UI/Views/MainPage.xaml`, `src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs` |
| CodeBrix.Platform.Graphics3DGL add-in | `GLCanvasElement` (the base class of `ModelSceneGlCanvas`), the initialization-state report, and transitively the `GL` type the shader renderer draws with | `src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs`, `src/libs/KenneyAssetBrowser.Rendering/GL/GlModelSceneRenderer.cs` |
| CodeBrix.Platform.AudioPlayer add-in | The `AudioPlayer` element that decodes and plays a pack's clips from a stream, and exposes the position and duration the scrubber binds to | `src/KenneyAssetBrowser.UI/Views/MainPage.xaml`, `src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs` |
| CodeBrix.Platform.AppSettings add-in | The whole settings store - typed get and set, change events, startup auto-backup and pruning, corruption recovery, import and export - wrapped by this application's own facade | `src/libs/KenneyAssetBrowser.Settings/SettingsService.cs`, `src/libs/KenneyAssetBrowser.Settings/LoggingService.cs` |
| CodeBrix.Compression | Zip reading: the archive type, its entries and entry streams; the tests also use its zip writer | `src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs`, `tests/libs/KenneyAssetBrowser.AssetRead.Tests/TestZipBuilder.cs` |
| CodeBrix.Imaging | Decodes PNG, JPEG, WebP and the rest into RGBA, both as a bitmap for display and as raw bytes for a GPU texture upload | `src/libs/KenneyAssetBrowser.Rendering/Images/LdrImageDecoder.cs` |
| CodeBrix.SkiaSvg | Rasterizes a pack's SVG art for the viewer and for grid thumbnails | `src/libs/KenneyAssetBrowser.Rendering/Images/SvgImageDecoder.cs` |
| CodeBrix.Sqlite | Not referenced by the application directly; it arrives with the AppSettings add-in and its store type is named by the settings tests | `tests/libs/KenneyAssetBrowser.Settings.Tests/SettingsStoreTests.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SharpGLTF (Runtime and Toolkit) | Reads `.glb` and `.gltf` documents and evaluates their scenes into triangles; the Toolkit's scene evaluation is what CPU-bakes the animation clips, and it also builds the tiny in-memory models the loader tests use | `src/libs/KenneyAssetBrowser.Rendering/Models/GltfModelLoader.cs`, `src/libs/KenneyAssetBrowser.Rendering/Models/AnimatedModel.cs`, `tests/libs/KenneyAssetBrowser.Rendering.Tests/TestData/TestAssets.cs` |
| SkiaSharp | The bitmap, canvas, paint and typeface types the Rendering library draws with | `src/libs/KenneyAssetBrowser.Rendering/Images/`, `src/libs/KenneyAssetBrowser.Rendering/Tiled/TiledMapRenderer.cs` |
| Microsoft.Extensions.Hosting and Logging.Console | The generic host that backs `SimpleServiceResolver`, and the Debug-only console logger factory | `src/KenneyAssetBrowser.Core/Helpers/HostHelper.cs`, `src/KenneyAssetBrowser.UI/App.xaml.cs` |
| xUnit v3, Microsoft.NET.Test.Sdk, SilverAssertions | The test stack every test project uses | the three `tests/libs/*/*.csproj` |
| SkiaSharp Linux native assets | Supplies the native Skia library so the Rendering tests can draw outside a running head | `tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj` |

## Worth studying in this application

### Reading a pack: an archive, a reader and a folder catalog

Everything the application knows about a Kenney pack comes from three classes in
`KenneyAssetBrowser.AssetRead`, which has no UI types in it at all. Read them in this
order: `BundleArchive.cs` opens one zip and serves byte and text reads from it;
`KenneyBundleReader.cs` turns an open archive into an `AssetBundle` (identity, entry list,
grouped models, parsed sprite atlases); `AssetFolderCatalog.cs` walks a folder of zips and
produces the catalog the sidebar shows. The archive's constructor builds a case-insensitive
name-to-index dictionary once, because looking an entry up by name is a linear scan, and
every read is taken under a lock because entry streams share the archive's underlying file
stream - reads can still be issued from worker threads, they simply serialize. The catalog
catches per-file exceptions and turns them into a warning string rather than failing the
load, so one corrupt zip in the folder costs the user one line of caption, not the whole
catalog. In the MVVM shape none of this is reachable from a view: `AssetCatalogService` in
Core wraps each call in `Task.Run` and returns only tasks, and `MainViewModel` awaits them
and owns the archive's lifetime.
See [Read a zip archive without extracting it with the CodeBrix Compression library](../BLUEPRINTS.md#read-a-zip-archive-without-extracting-it-with-the-codebrix-compression-library)
and [Do blocking work in a service behind Task Run](../BLUEPRINTS.md#do-blocking-work-in-a-service-behind-task-run).

### Turning a pack's files into browsable cards

A Kenney kit ships the same model in several formats, a spritesheet as an image plus an XML
file, and per-model preview renders in their own folder. Showing one card per file would be
unusable, so the library groups first and the view model hides what a grouped card already
represents. `Parsing/AssetClassifier.cs` maps a file extension to an `AssetKind` with an
`Unknown` fallback - adding a format is a one-line change, and its test is a theory over the
mapping. `KenneyBundleReader.GroupModelAssets` groups model entries by file-name stem into
one `ModelAsset` each, attaching the matching preview render and material file.
`Parsing/KenneyNames.cs` derives the pack's display name and version from the first line of
its `License.txt`, rejecting a first line that is too long or contains a URL because that is
license prose rather than a title, and falling back to a prettified file name. The view
model then builds one `AssetCellViewModel` per logical asset, in its own vocabulary:

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellViewModel.cs
// Members put on one line and their doc comments trimmed; the real file documents each one.
public enum AssetCellKind
{
    Image, Model, Atlas, Document, Audio, Vector, Font, TiledMap, Other,
}
```

The one gotcha is that the hide-what-is-grouped rule has to be applied twice - once when
building the cells and once when building the category filter list - or a folder whose files
are all hidden shows up as an empty category.
See [Classify and group the contents of a container for browsing](../BLUEPRINTS.md#classify-and-group-the-contents-of-a-container-for-browsing).

### The Browsing View: a lazy grid, a debounced search and a filter

The Browsing View is a sidebar of pack cards beside a grid of asset cards, with a header
carrying the search box, the category filter and the assets-folder button. Read
`ViewModels/AssetCellCollection.cs` first: it is an observable collection that holds the
whole filtered list privately and only adds a batch at a time, exposing `HasMoreItems` and
`RequestMore(count)`. Then `Views/MainPage.xaml` for the repeater and its uniform grid
layout, and `Views/MainPage.xaml.cs` for the scroll watcher that asks for the next batch
while the bottom edge is still a couple of viewports away. Filtering swaps in a whole new
collection instance rather than mutating the existing one, which is what lets a single
property change mean "the list is different, scroll back to the top". The search box binds
two-way with `UpdateSourceTrigger=PropertyChanged` and its setter starts a cancellable
delay, so a rebuild happens once typing settles rather than on every keystroke; the category
list next to it needs a suppression flag around repopulation, because assigning the list and
resetting the selection would otherwise each trigger a rebuild. In the MVVM shape the batch
size is policy and belongs beside the collection's own initial batch constant, while the
scroll measurement is a view concern and stays in the page.
See [Fill a grid lazily as it scrolls](../BLUEPRINTS.md#fill-a-grid-lazily-as-it-scrolls),
[Debounce a search box before rebuilding a filtered list](../BLUEPRINTS.md#debounce-a-search-box-before-rebuilding-a-filtered-list)
and [Stop a two way bound selection from commanding the control back](../BLUEPRINTS.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).

`ViewModels/AssetCellViewModel.cs` is where the grid's per-item shape lives, and it is
worth reading on its own. Each cell holds its display text plus two delegates the owner
supplies: what opening the cell does, and how its thumbnail bytes are fetched (`null` for
kinds that have none). That is what keeps the
template's bindings plain - `{Binding OpenCommand}` and `{Binding Thumbnail}`, no element
names and no ancestor lookups - because a template binds to its own item. The whole card is
a button bound to the command, so keyboard and hover behavior come for free;
`BundleCellViewModel.cs` is the same shape with a select command. A failed thumbnail fetch
sets a flag so it is never retried on rescroll and the placeholder glyph stays, and the
bitmap must be created on the UI thread, which the code gets by relying on the awaiter
restoring the dispatcher context - the comment in `LoadThumbnailAsync()` says so.
See [Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail).

### The 2D viewer: a painter the view model owns

Images, spritesheets, rasterized SVG art, font specimens and composited Tiled maps all land
on the same Skia surface. The drawing lives in
`src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs`, a plain class with no
UI types: it draws the transparency checkerboard, places the bitmap, applies the zoom, and
dims everything outside a highlighted region before outlining it. `MainViewModel` owns the
painter as a bound property and the page's paint handler forwards the canvas and its size to
it in one line. Repaint requests travel the other way through `IImageCanvasBridge`, a
one-delegate interface the view model implements and the page fills in - wrapping the call
in a dispatcher enqueue, because the view model raises it from continuations that may not be
on the UI thread. Two sharp edges: a resize alone does not repaint, so the page invalidates
on `SizeChanged` too; and at a 2x zoom and above the painter switches to nearest-neighbor sampling,
which is the right default for pixel art. The painter's layout math (`GetImageRect`,
`CanvasToImage`) is public precisely so it can be tested without a canvas, and the test
project does exactly that. Selecting a sprite row in the facts pane sets the painter's
highlight rectangle in image pixels; the painter converts it with the same scale it used to
place the image, so the spotlight tracks zoom with no extra work, and re-selecting the same
row clears it.
See [Paint a zoomable image on an SKXamlCanvas from the view model](../BLUEPRINTS.md#paint-a-zoomable-image-on-an-skxamlcanvas-from-the-view-model),
[Spotlight one region of an image on the canvas](../BLUEPRINTS.md#spotlight-one-region-of-an-image-on-the-canvas),
[Decode raster images with the CodeBrix Imaging library into a Skia bitmap](../BLUEPRINTS.md#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap)
and [Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS.md#rasterize-svg-art-with-the-codebrix-skiasvg-library).

### The 3D viewer: a GL control in a library, and getting a model to it

The model preview is a control, not page code. Read
`src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs` first: it derives from the
Graphics3DGL canvas element, exposes `Model`, `AnimationClip` and `IsAnimationPlaying` as
dependency properties, turns pointer input into orbit and zoom itself, and drives an
`IModelSceneRenderer` through the base class's GL-thread lifecycle. Then
`GL/IModelSceneRenderer.cs`, whose doc comment states which member is called from which
canvas callback and on which thread, and `GL/GlModelSceneRenderer.cs`, the shader renderer -
which has no XAML type in it and therefore no reason to know a canvas exists.
`Cameras/OrbitCamera.cs` is pure math with no GL dependency, which is why it has a full test
file. The page places the control and binds; `MainViewModel` holds the loaded model and the
play state and knows nothing about OpenGL. Three sharp edges are recorded in the code
itself: renderer initialization is idempotent and called from both the init callback and the
render callback, because a canvas that starts collapsed may render before it initializes;
the head's own Skia renderer shares the GL context, so every state the render touches is
saved and restored in a `finally`; and matrices are uploaded without transposing on purpose,
since the row-major layout the numerics types use is already the transpose OpenGL wants.
See [Host an OpenGL scene in XAML with a GLCanvasElement subclass](../BLUEPRINTS.md#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass),
[Keep the GL renderer framework-free behind an interface](../BLUEPRINTS.md#keep-the-gl-renderer-framework-free-behind-an-interface)
and [Pick the shader version header for desktop GL or GLES at runtime](../BLUEPRINTS.md#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime).

Getting a model into that control is the application's heaviest path, and it is worth
following end to end in `MainViewModel`. The archive field is captured into a local before
the parse starts, so a bundle switch mid-parse cannot null it out; the parse itself runs inside `Task.Run` behind
`IModelLoader`; and the loader is handed a callback that resolves external references back
into the same archive, because a Kenney binary model references its colormap texture beside
itself rather than embedding it. The awaited result is assigned to a backing field and
published with a change notification, which is what the bound control picks up. Animations
are baked to vertex frames off the UI thread - a trade of memory for simplicity, since
playback then becomes swapping vertex buffers and the renderer never needs skinning - and a
bake is only published if the selection and the model have not changed since it started. The
control owns the playback timer at the clip's own frame rate; the view model owns the clip
list, the selection and the play state, and opens on an animation named "idle" when the
model has one.
See [Load an asset off the UI thread and resolve its side files from the same container](../BLUEPRINTS.md#load-an-asset-off-the-ui-thread-and-resolve-its-side-files-from-the-same-container)
and [Play a baked animation clip in a preview canvas](../BLUEPRINTS.md#play-a-baked-animation-clip-in-a-preview-canvas).

### Playing a pack's audio clips

Audio is the clearest bridge example in this application. `ViewModels/IAudioPlayerBridge.cs`
is five settable delegates - load a stream, play, pause, stop, set looping - which
`MainViewModel` implements itself and the page fills in from `DataContextChanged`. The view
model owns the transport commands and the loop state and null-guards every call, so a head
where the bridge was never filled in degrades to a viewer pane that says playback is not
available rather than to a crash; the interface's own doc comment states that contract. The
clip's bytes go straight to the element as a memory stream - the element takes ownership of
it, decodes Ogg Vorbis, WAV, MP3 and FLAC itself, and so the application never needs a
format check before playing. Opening a different asset stops whatever was playing first.
The scrubber is the deliberate exception to routing everything through the view model:
position and duration change many times a second and mean nothing outside this control
group, so the slider and the two timecode labels bind to the element by name, through a
one-way converter that shows tenths because most of what a pack ships is a sound effect
shorter than a second. The replay behavior - Play on a clip parked at its end should rewind
first, but not when the clip is looping and not when the user has scrubbed away from the end
- is application policy: in the MVVM shape it belongs in `PlayAudioCommand`, with the bridge
growing read-only accessors for position, duration and playing state plus a seek action, and
the page forwarding the element's playback-ended event in one line.
See [Play an audio clip straight from bytes with the AudioPlayer add-in](../BLUEPRINTS.md#play-an-audio-clip-straight-from-bytes-with-the-audioplayer-add-in),
[Replay a finished audio clip with one button press](../BLUEPRINTS.md#replay-a-finished-audio-clip-with-one-button-press),
[Bind a scrubber and volume slider straight to the media element](../BLUEPRINTS.md#bind-a-scrubber-and-volume-slider-straight-to-the-media-element)
and [Format a value for display with an IValueConverter](../BLUEPRINTS.md#format-a-value-for-display-with-an-ivalueconverter).

### Two views and five viewer panes, with no navigation at all

The page never navigates. The Browsing View and the Viewer View are two grids in the same
cell with bound visibilities, and inside the viewer well the image canvas, the model canvas,
the text pane, the audio transport and the "nothing to preview" caption are five more.
`MainViewModel` keeps a private mode enum, exposes one computed `Visibility` per pane, and
has exactly one method that sets the mode and raises every dependent notification - which is
what keeps those notifications in one place instead of scattered through every open method.
The open path itself is one command per cell that dispatches on the cell's kind and wraps
the whole switch in a single error handler, so any failure becomes one dialog through
`ShowError(ex, message)` rather than a crash. Unsupported files still open, into the
explanatory mode, so nothing in the grid is a dead card. The view model exposes `Visibility`
properties throughout rather than booleans plus converters, and the XAML binds them
directly.
See [Show and hide panes with computed Visibility properties](../BLUEPRINTS.md#show-and-hide-panes-with-computed-visibility-properties)
and [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).

### Layout that follows the window, twice

The FlexPanel add-in appears in two different roles in `Views/MainPage.xaml`. The header is
a wrapping row: the identity block takes a grow of 1 so it soaks up the free space and pins
the other groups right while they share its row, and the search box and category filter are
wrapped in one child so they travel together when the row folds - the panel wraps children,
not their contents. The viewer's facts pane and viewer well share a second panel whose main
axis flips with the window's orientation, set from the page's size-changed handler: side by
side in landscape, stacked in portrait. That handler is doing layout from a size the view
model never sees, which is defensible, but if anything else in the application ever needs to
know the orientation the shape to reach for is an `IsPortrait` property on the view model,
set from one line in the handler, with everything else bound to it. The sharp edge is
recorded in the XAML: in landscape the pane needs an explicit width rather than a flex
basis, because its content is measured against the width and would not otherwise wrap at the
pane edge; in portrait the width is cleared and a relative basis takes over, and the margin
moves with the axis.
See [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in)
and [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).

### Degrading gracefully where a head cannot help

Two capabilities in this application are not available everywhere, and both are handled the
same way rather than with a compile-time switch. The audio bridge's delegates stay null on a
head with no player, and the viewer pane says so. The 3D preview can fail on a machine with
no usable OpenGL driver, and an empty pane looks like a bug - so the page asks the canvas for
its initialization state, which is a view concern, and hands the state object to a view-model
method that owns the message and shows the dialog. The check has to run at two moments, the
canvas's load and the view model's viewer-active change, because a collapsed canvas may not
attempt initialization until it enters the visual tree; a flag reports it once per run rather
than on every model the user opens. The Windows-specific hint in the message is gated on
`SimpleOsInfo.GatherInfo(withConsoleOutput: false)` reporting Windows, not on a build
constant. The same instinct shows up in the LinuxFrameBuffer head, which opts into a folder
picker and a software keyboard because it has no desktop chrome to borrow either from; the
start and restrict folders in that call are the author's own machine paths and should be
treated as placeholders and computed from the environment in your own application.
See [Tell the user when graphics initialization failed](../BLUEPRINTS.md#tell-the-user-when-graphics-initialization-failed)
and [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).

### Settings behind one facade, and the folder the whole application hangs on

`src/libs/KenneyAssetBrowser.Settings/SettingsService.cs` is a static facade whose every
member forwards to the AppSettings add-in, and its project file carries the reason: the
store, the typed properties, the change events and the backup, import and export machinery
are the add-in's, and this library is the thin application-named front for them. Nothing
else in the application talks to the add-in, so the backend could change in one file. A
companion `LoggingService` forwards to the add-in's logging service so the settings
backend's diagnostics reach the same sinks as everything else. `App` calls `Initialize()`
before `InitializeComponent()`, because the page's view model reads a setting in its own
constructor - and initialization is also what runs the startup auto-backup and prune, so
its position in the constructor matters. This application persists two things, both keyed by
`public const string` fields on the view model that owns them: the chosen assets folder, and
the file name of the pack browsed last, which is restored on the next run and falls back to
the first pack in the folder. The folder itself gates the whole Browsing View through a pair
of visibility properties, and the same `PickFolderCommand` is bound twice - once on the
first-launch prompt and once on the header button - so there is a single code path either
way.
See [Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS.md#wrap-the-appsettings-add-in-in-one-application-named-facade),
[Open the settings store before any other startup work](../BLUEPRINTS.md#open-the-settings-store-before-any-other-startup-work),
[Choose a folder with the picker and remember it across runs](../BLUEPRINTS.md#choose-a-folder-with-the-picker-and-remember-it-across-runs)
and [Gate an action behind a chosen folder and explain the gate with a dialog](../BLUEPRINTS.md#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog).

### Startup, and why every package lives where it does

The heads are the shortest files in the application: initialize logging, build a host with a
factory for `App` and one platform call, run. Only two heads add anything - WinWpfSkia asks
its host for a software render surface after `Build()`, and LinuxFrameBuffer configures its
orientation, picker and keyboard. Everything else happens in `App`: font configuration,
`SimpleServiceResolver` created from `HostHelper`'s provider with services registered
through the single `AddKenneyAssetBrowser()` extension, `SetIsDesignMode(false)` so view
models built by the XAML parser run their real path, the settings store, and only then
`InitializeComponent()`. The package rule is written into the project files themselves:
exactly one platform head package per head, everything else in Core, each add-in carrying a
comment saying what it is for.
`src/libs/KenneyAssetBrowser.Rendering/KenneyAssetBrowser.Rendering.csproj` is the one place
that needs a note beyond that - because it hosts a XAML control it must keep its own root
namespace rather than the application's, or the generated per-head resources class collides
across assemblies and the build fails with a duplicate-type error rather than a namespace
complaint. The same file records why it names both the base platform package and the
graphics add-in, and that the application never references the low-level binding directly.
See [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS.md#bootstrap-the-application-in-the-app-constructor),
[Register library services with one AddXxx extension method](../BLUEPRINTS.md#register-library-services-with-one-addxxx-extension-method),
[Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
and [Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively).

### Three libraries, three mirrored test projects, no fixtures on disk

Because the three libraries hold no UI types, they are testable on their own, and each has a
test project that mirrors it. `KenneyAssetBrowser.AssetRead.Tests` writes the archive each
case needs with the compression library's zip writer, including a deliberately corrupt one
to prove that a bad file becomes a warning rather than a failed catalog.
`KenneyAssetBrowser.Rendering.Tests` builds its own PNGs from Skia bitmaps and its own
single-triangle glTF binaries from the glTF toolkit, and it has to reference the native Skia
package explicitly - in a running application the head supplies that, but a bare test host
does not, and the project file says so. `KenneyAssetBrowser.Settings.Tests` gives each case
its own temporary folder and constructs the store against it directly rather than through
the process-wide facade, except for the one case that checks the facade itself; the store's
own file-name constants are public, so the tests assert against the real naming scheme
rather than a copy of it. `AssetRead` and `Rendering` each carry an `InternalsVisibleTo.cs`
naming their test assembly; `Settings` does not, because its facade is public in full.
See [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Add the native assets a head would have supplied](../BLUEPRINTS.md#add-the-native-assets-a-head-would-have-supplied),
[Build the binary inputs your tests need instead of committing them](../BLUEPRINTS.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them)
and [Point a process-global store at a throwaway folder in tests](../BLUEPRINTS.md#point-a-process-global-store-at-a-throwaway-folder-in-tests).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the third-party
content bundled with the application: the Kenney asset packs committed under
`sample_asset_bundles/` (Blocky Characters, Brick Kit, Puzzle Pack and Sci-Fi Sounds), which
are unmodified CC0 downloads, each carrying Kenney's own `License.txt` inside the zip - the
same file the application shows from its License button. Any other pack the user opens is
read from their own disk and is not redistributed here, and its rights remain with its
owners. Third-party code arrives as packages, each with its own license and notices; nothing
is downloaded at run time, and no fonts or models are vendored into this folder - Merriweather
and its Noto Serif fallbacks come from a CodeBrix.Platform font package.

## License

KenneyAssetBrowser is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
