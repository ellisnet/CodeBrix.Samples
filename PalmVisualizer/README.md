# PalmVisualizer

PalmVisualizer is a two-mode webcam toy. It opens in Camera Mode: a dropdown lists the
cameras connected to the machine, the first one starts automatically, and a mirrored live
preview fills the window. Pressing "Visualize!" switches to Visualize Mode, where the live
video is replaced by an animated plasma-and-starfield visual driven by an SkSL shader. While
that visual runs, every frame from the camera is fed to a palm-tracking pipeline: wherever
the user holds up an open palm, the colors bend toward the hand, rings of color drift inward,
the plasma brightens, and nearby stars are pulled along, so the whole visual appears to chase
the hand. Closing the hand, or moving it out of view, releases the pull and the visual melts
back to its undisturbed motion. Up to four open palms can attract the visual at once, and
each hand keeps a stable identity while it stays in frame. A "Back" button returns to Camera
Mode, and a single status line narrates everything the application is doing: camera
discovery, the live camera name, and how many open palms the colors are chasing.

It is a reference for three things a CodeBrix.Platform application often needs together:
running the CodeBrix.Platform.GameEngine fixed-rate render loop inside an ordinary XAML page
rather than a game shell, capturing webcam video with the CodeBrix.Webcam library and
displaying it on a Skia canvas, and running neural-network inference off the UI thread with
the CodeBrix.VideoProcessing.OpenCV5 library and marshaling the results back into a view
model. A sibling application in this repository, WebcamPainter, uses a related vision
pipeline for a different purpose.

## What this sample shows a CodeBrix.Platform developer

- Start the game engine loop against a canvas that begins hidden, by having the page forward
  the canvas's first real layout size to the view model through a one-method interface:
  [Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size).
- Keep the engine session behind a class with `Start()`, `Pause()`, `Resume()` and `Stop()`
  so leaving and re-entering a mode costs a pause rather than a rebuild:
  [Run and pause a game engine session inside a page](../BLUEPRINTS-GameEngine.md#run-and-pause-a-game-engine-session-inside-a-page).
- Write a full-surface animated SkSL shader as an engine direct drawing that compiles once
  and allocates nothing per frame:
  [Draw an animated SkSL shader as a game engine direct drawing](../BLUEPRINTS-GraphicsAndRendering.md#draw-an-animated-sksl-shader-as-a-game-engine-direct-drawing).
- Run the identical scene on the GPU or on Skia's raster backend, chosen by one environment
  variable read before the surface is touched:
  [Offer a CPU fallback for a GPU rendering path behind one switch](../BLUEPRINTS-GraphicsAndRendering.md#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch).
- Turn irregular worker-rate values into smooth frame-rate animation with a thread-safe field
  that eases position and strength independently:
  [Smooth worker rate data into frame rate animation](../BLUEPRINTS-GraphicsAndRendering.md#smooth-worker-rate-data-into-frame-rate-animation).
- Keep a vision library and a rendering library ignorant of each other by defining the
  narrowest possible normalized value type as their only seam:
  [Keep a pipeline and a renderer decoupled by a normalized seam](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-pipeline-and-a-renderer-decoupled-by-a-normalized-seam).
- Populate a camera dropdown asynchronously at startup, auto-start the first device, and
  switch devices without leaving two sessions running:
  [Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session).
- Put live video inside a XAML layout with an `SKXamlCanvas` subclass and a renderer that
  aspect-fits, mirrors and reuses its buffers:
  [Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass).
- Wrap a device library's type in a small sealed class with an internal constructor so the
  view model and the XAML never name it:
  [Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it).
- Run inference on a dedicated worker thread with a single-slot pending buffer, so a slow
  model never blocks the camera and never works on a stale frame:
  [Run a sensor pipeline on a worker thread with latest frame wins](../BLUEPRINTS-MVVM.md#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins).
- Load a `.tflite` model through the OpenCV DNN module, address its outputs by name, and do
  the anchor decoding and non-maximum suppression yourself:
  [Run a TFLite model through the OpenCV DNN module](../BLUEPRINTS-MediaAndVision.md#run-a-tflite-model-through-the-opencv-dnn-module).
- Match this frame's detections to last frame's tracks by nearest neighbor so each physical
  hand keeps one id:
  [Track multiple detections across frames with stable ids](../BLUEPRINTS-MediaAndVision.md#track-multiple-detections-across-frames-with-stable-ids).
- Compute a gesture from landmark geometry when the model that would have classified it
  cannot be imported:
  [Recognize a gesture from landmark geometry instead of a model](../BLUEPRINTS-MediaAndVision.md#recognize-a-gesture-from-landmark-geometry-instead-of-a-model).
- Reconcile a mirrored preview with an unmirrored tracker in exactly one place, so left stays
  left everywhere downstream:
  [Keep a mirrored preview and a mirrored drawing consistent](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent).
- Take a capture-thread event, route the frame to a worker, and dispatch only the bound
  properties that actually changed:
  [Hand results from a capture thread through a worker to the UI thread](../BLUEPRINTS-MVVM.md#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread).
- Set bound state from a background thread with `InvokeOnMainThread` while feeding
  thread-safe consumers directly:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Let the page hand the view model an invalidate delegate through a bridge interface, so the
  view model repaints a canvas it knows nothing about:
  [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- Swap two main visuals and two button groups from a single bound bool by declaring one
  converter twice, the second time inverted:
  [Switch a page between two modes with one bool and a converter](../BLUEPRINTS-ViewsAndControls.md#switch-a-page-between-two-modes-with-one-bool-and-a-converter).
- Declare bound properties and lazily created `SimpleCommand` commands the family way, with
  `[AffectsCommands]` refreshing `CanExecute`:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Start slow discovery work from the constructor without making the constructor async or
  letting a failure escape it:
  [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- Hand the view model a `XamlRoot` getter in the same handler that wires the bridges, so
  dialogs work the day they are added:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Embed a binary asset in exactly the library that needs it, with an explicit logical name
  that the C# constant matches:
  [Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).
- Keep a native-binding library runtime-independent by declaring its per-runtime native
  packages in the head projects instead:
  [Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads).
- Carry every shared package in one Core library and give each head exactly one platform
  runtime package:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Organize an application as a shared UI project, a Core project, libraries under `src/libs`
  and mirrored test projects under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Record bundled third-party content in a notices file that says what is bundled and what
  merely arrives as a package:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- Set up an xUnit v3 test project that the family's runner actually discovers:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Give each library an `InternalsVisibleTo.cs` naming only its own test assembly, and add
  documented internal test accessors rather than widening fields:
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Keep every head's `Program.Main` to the same handful of lines, differing only in the call
  that names the platform:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Cast the built host on the WinWpfSkia head to force its software render surface, guarded so
  the code survives a host type change:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Do the four startup jobs in the App constructor in the order that matters, including
  `SetIsDesignMode(false)` before any view model is built:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Give `SimpleServiceResolver` a generic host builder from a single helper compiled once in
  the Core library:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Make a bundled font the default for every head by pointing at the `.ttf` through an
  `ms-appx:///` URI:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Install a console logger factory only in Debug builds, from a static method each head calls
  before building its host:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

## Building, running and testing

There is one solution file, `PalmVisualizer/PalmVisualizer.slnx`, and it holds everything:
the shared UI project, the Core project, all six heads, the three libraries under a
`Libraries` solution folder, and the three test projects under a `Tests` solution folder. Its
header comment describes it as everything that builds with the plain .NET SDK on Linux, macOS
and Windows, which holds here because every head is a Skia head. There is no WinUI 3, WPF or
.NET MAUI head and no workload-gated project, so the same solution opens on any of the three
operating systems.

| Head project | Platform |
| --- | --- |
| `PalmVisualizer/src/PalmVisualizer.LinuxX11` | Linux X11 |
| `PalmVisualizer/src/PalmVisualizer.LinuxWayland` | Linux Wayland |
| `PalmVisualizer/src/PalmVisualizer.LinuxFrameBuffer` | Linux framebuffer |
| `PalmVisualizer/src/PalmVisualizer.MacOS` | macOS |
| `PalmVisualizer/src/PalmVisualizer.Win32Skia` | Windows (Win32) |
| `PalmVisualizer/src/PalmVisualizer.WinWpfSkia` | Windows (WPF host) |

Every head targets `net10.0` except WinWpfSkia, which targets `net10.0-windows` and sets
`EnableWindowsTargeting` so it restores and builds from Linux or macOS; it can only run on
Windows.

Prerequisites:

- The .NET 10 SDK. Nothing is installed by hand: the native OpenCV library and the Skia
  runtime both arrive as NuGet packages (see each project's csproj).
- A webcam. Without one the application still starts and reports "No cameras were found on
  this machine."; the "Visualize!" button stays disabled because it requires a delivered
  frame.
- No accounts, tokens, network access or user-supplied data files. The two hand-tracking
  models are committed under `PalmVisualizer/models/` and embedded into the Vision library at
  build time, so there is no download step.

Run one head from the `PalmVisualizer` folder:

```text
dotnet run --project src/PalmVisualizer.LinuxX11/PalmVisualizer.LinuxX11.csproj
```

Set `PALMVISUALIZER_USE_CPU=1` before launching to run the identical Visualize Mode scene
through the engine's CPU render path instead of its GPU (OpenGL) path.

`PalmVisualizer/global.json` contains nothing but the runner selection, which points every
project below it at the Microsoft.Testing.Platform runner:

```text
// From CodeBrix.Samples/PalmVisualizer/global.json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

All three test projects use xUnit v3 with SilverAssertions, build as `Exe`, and set
`UseMicrosoftTestingPlatformRunner`, so each test assembly is a self-executing binary. That
matters in practice: a plain `dotnet test` can report that it discovered zero tests. When it
does, build the test project and run the produced executable directly:

```text
dotnet build tests/libs/PalmVisualizer.Vision.Tests/PalmVisualizer.Vision.Tests.csproj -c Release
./tests/libs/PalmVisualizer.Vision.Tests/bin/Release/net10.0/PalmVisualizer.Vision.Tests
```

What each test project needs:

| Test project | Covers | Needs |
| --- | --- | --- |
| `PalmVisualizer/tests/libs/PalmVisualizer.Camera.Tests` | The capture service lifecycle: enumeration returns a possibly empty list with usable display names, a fresh service reports no session and no frame, `Start()` rejects a null camera, `Stop()` without `Start()` is harmless | OS device enumeration only; passes on a machine with no camera |
| `PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests` | Attractor easing semantics and the backdrop shader: it compiles, it accepts every uniform the backdrop sets, and it paints non-black pixels on a raster surface | Native Skia, pulled in by referencing the Linux, macOS and Windows Skia native-asset packages |
| `PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests` | The detector's pure math, the geometric open-palm classifier, embedded-model loading, tracker start/stop idempotence, and end-to-end inference against a bundled photograph | Native OpenCV, referenced with per-OS `Condition` attributes, plus the bundled test photo copied to the output directory |

The Vision tests run real inference on the CPU. No GPU and no network are needed.

## How the projects and folders are organized

```text
PalmVisualizer/
  PalmVisualizer.slnx                  The one solution: UI, Core, six heads, three libraries, three test projects
  global.json                          Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt              Covers the bundled hand-tracking models
  models/                              The committed MediaPipe hand models, embedded into the Vision library at build time
  src/
    PalmVisualizer.UI/                 Shared project (.shproj + .projitems): App.xaml(.cs), Views/MainPage.xaml(.cs)
    PalmVisualizer.Core/               The library every head references; carries the shared packages
      Helpers/HostHelper.cs            The host-builder provider SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs      All application logic; also declares ICanvasBridge and IManageGameCanvas
    PalmVisualizer.LinuxX11/           Head: X11. Program.cs plus its packages
    PalmVisualizer.LinuxWayland/       Head: Wayland
    PalmVisualizer.LinuxFrameBuffer/   Head: Linux framebuffer
    PalmVisualizer.MacOS/              Head: macOS
    PalmVisualizer.Win32Skia/          Head: Windows Win32
    PalmVisualizer.WinWpfSkia/         Head: Windows WPF host; net10.0-windows + EnableWindowsTargeting
    libs/
      PalmVisualizer.Camera/           Webcam capture service, the CameraDevice wrapper, the canvas and its frame renderer
      PalmVisualizer.Vision/           Palm-tracking pipeline: worker thread, embedded models, OpenCV DNN inference
        Internal/                      PalmDetector, HandLandmarker, OpenPalmClassifier - not part of the public surface
      PalmVisualizer.Rendering/        The Visualize Mode scene: engine session, SkSL backdrop, palm attractor smoothing
  tests/
    libs/
      PalmVisualizer.Camera.Tests/     Mirrors src/libs/PalmVisualizer.Camera
      PalmVisualizer.Rendering.Tests/  Mirrors src/libs/PalmVisualizer.Rendering
      PalmVisualizer.Vision.Tests/     Mirrors src/libs/PalmVisualizer.Vision; _data/ holds the test photograph
```

Dependency direction is strictly one way. Each head project references `PalmVisualizer.Core`
by project reference and file-links the shared UI through an `Import` of
`PalmVisualizer.UI.projitems`, so `App.xaml`, `App.xaml.cs`, `MainPage.xaml` and
`MainPage.xaml.cs` are compiled into every head rather than into a library. That is why every
head csproj repeats the `<Page Include="**\*.xaml" />` plus `<None Remove="**\*.xaml" />`
pair: the files arrive through the shared import, but the head is where they are compiled.
`PalmVisualizer.Core` project-references all three libraries and carries the CodeBrix.Platform
and Roboto font packages, which reach the heads transitively. Each library owns the packages
only it needs, and nothing under `src/libs` references anything else under `src/libs`. The
view model is the only place the three libraries meet. Each test project references exactly
its own library and nothing else.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The application framework: `Application`, `Window`, `Frame`, `Page`, XAML binding, the default-font feature configuration, and the "Simple" toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `[AffectsCommands]`, `InvokeOnMainThread`) | `PalmVisualizer/src/PalmVisualizer.Core/PalmVisualizer.Core.csproj`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`, `PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface; `CodeBrixPlatformHostBuilder` selects it in `Program.Main` | The six head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Roboto | Supplies the Roboto family the whole application uses as its default text font | `PalmVisualizer/src/PalmVisualizer.Core/PalmVisualizer.Core.csproj`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs` |
| CodeBrix.Platform.SkiaSharp.Views | Supplies `SKXamlCanvas`, which `CameraCanvas` subclasses so the live preview can be named in XAML | `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Camera/PalmVisualizer.Camera.csproj` |
| CodeBrix.Platform.GameEngine | The fixed-rate engine loop, `GameSurfaceCanvas`, the render surface host, the view manager and `DirectDrawingBase`; one package supplies both the engine core and the Host layer | `PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj` |
| CodeBrix.Platform (the `CodeBrix.Platform.UI.Toolkit` converters namespace) | `BoolToVisibilityConverter`, declared twice (the second time with `Invert="True"`) to swap the two mode views and the two button groups from one bool | `PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml` |
| CodeBrix.Webcam | Camera enumeration and the live capture session that delivers BGRA frames and caches the latest one | `PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs` |
| CodeBrix.VideoProcessing.OpenCV5 | The managed binding: `Mat`, the color-conversion, resize and warp calls, and the DNN module that runs both models | `PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/HandLandmarker.cs` |
| CodeBrix.VideoProcessing.OpenCV5 per-runtime native packages | The native OpenCV library, referenced once per runtime identifier by each head and conditionally by the Vision test project | The six head csproj files, `PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/PalmVisualizer.Vision.Tests.csproj` |
| SilverAssertions | The assertion style in all three test projects | The three test csproj files and every test file |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | `SKSurface`, `SKCanvas`, `SKBitmap`, `SKPaint`, and the runtime-effect types that compile and feed the SkSL shader; its native-asset packages are referenced by the Rendering test project so the shader tests can run | `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs`, `PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()`, wrapped by `HostHelper` and handed to `SimpleServiceResolver` | `PalmVisualizer/src/PalmVisualizer.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The Debug-only console logger factory installed in `App.InitializeLogging()` | `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for all three test projects | The three test csproj files, `PalmVisualizer/global.json` |

## Worth studying in this application

### Two modes, one bool, two stacked canvases

The whole application is one page with two states, and the state is a single bound bool. The
view model exposes `IsCameraMode` with a private setter, plus a computed `IsVisualizeMode`
that its setter notifies. `MainPage.xaml` declares `BoolToVisibilityConverter` twice, the
second instance with `Invert="True"`, and binds both the two canvases and the two button
groups to the same property. The two canvases live in the same `Grid` cell and are stacked,
so only visibility changes; that is what keeps the game canvas alive, with its engine merely
paused, across mode switches. The camera dropdown is disabled rather than hidden in Visualize
Mode so the layout does not shift, and a single `StatusText` `TextBlock` at the bottom
narrates every state, which is why the view model can report errors without any dialog. Read
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml` first, then the bindable-properties
region of `PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Switch a page between two modes with one bool and a converter](../BLUEPRINTS-ViewsAndControls.md#switch-a-page-between-two-modes-with-one-bool-and-a-converter)
and
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### Camera discovery, selection and the mirrored preview

`WebcamCaptureService` in the Camera library is the whole capture model: a static discovery
method, `Start()`, `Stop()`, `HasFrame`, `TryCopyLatestFrame()` and a `FrameArrived` event.
The view model owns one, holds the discovered devices in an `ObservableCollection`, and
switches cameras from the `SelectedCamera` setter. Discovery is kicked off from the
constructor as fire-and-forget after setting a "Discovering cameras…" status, its results are
marshaled with `InvokeOnMainThread`, and every failure path writes to the same status line
rather than throwing out of the constructor. An empty device list is treated as a normal
state, not an error. `Start()` calls `Stop()` first so switching cameras never leaves two
sessions running, and `Stop()` unsubscribes before disposing and clears the frame flag so a
stale frame from the previous camera cannot be drawn; the flag itself is `volatile` because
the capture thread writes it and the UI thread reads it. The dropdown binds to `CameraDevice`,
a sealed wrapper with an internal constructor whose `ToString()` returns the friendly name,
so no item template is needed and the view model never handles a capture-library type. Read
`PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs`. See
[Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session),
[Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it)
and
[Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).

Displaying the video is deliberately split. `CameraCanvas` is a one-line `SKXamlCanvas`
subclass that exists purely so the shared UI project's XAML can name the type from the
library's namespace, and all the drawing lives in a separate `WebcamFrameRenderer` that takes
a surface, an image info and the capture service. The renderer clears to black first, so "no
frame yet" is a black panel rather than garbage; caches its pixel buffer and its `SKBitmap`
and only recreates the bitmap when the frame size changes; and mirrors by a canvas transform
between a save and a restore rather than by flipping pixels. Its doc comment states the
ownership rule: one renderer per canvas, touched only on the UI thread. See
[Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass).

### The bridge interfaces the page fills in

`MainViewModel` declares both of its platform seams as interfaces in its own file and
implements them. `ICanvasBridge` holds a single `Action InvalidatePreviewCanvas`, which the
page assigns to a delegate that marshals through its own `DispatcherQueue`; the view model
then calls it from whatever thread it happens to be on, and the null-conditional invocation is
the graceful-degradation path when no page has supplied one. `IManageGameCanvas` has one
method, described below. Both are wired in `DataContextChanged`, which
`MainPage.xaml.cs` subscribes to *before* `InitializeComponent()` because
`InitializeComponent()` may be what sets the data context. The same handler hands the view
model a `XamlRoot` getter through `IXamlRootGetter`, which costs one line and means
`SimpleDialog` works the day a dialog is added. Inside the invalidate delegate both the
dispatcher and the canvas are null-checked, so a repaint requested during teardown is a no-op
instead of a crash. Read
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` beside the interface
declarations at the top of
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
and
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).

### One frame handler, two consumers, one mirror

`OnFrameArrived` runs on the capture thread and is the only place that decides where a frame
goes: in Camera Mode it invalidates the preview canvas, and in Visualize Mode it copies the
latest frame into a single reusable buffer and submits it to the tracker. One camera feed,
two consumers, no duplicated capture. The handler dispatches `HasFrame` to the UI thread only
once, on the first frame, because that property gates the "Visualize!" command's
`CanExecute`; everything else it does is thread-safe already. The mirror lives here too. The
tracker deliberately reports palm positions across the *unmirrored* camera frame, and the
preview is mirrored by a canvas transform, so the view model is the single place the two
coordinate conventions are reconciled, with one subtraction on the horizontal axis. Read the
"Live frames and palm tracking" region of
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Hand results from a capture thread through a worker to the UI thread](../BLUEPRINTS-MVVM.md#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and
[Keep a mirrored preview and a mirrored drawing consistent](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent).

The results handler, `OnTrackingUpdated`, is the mirror image of the same discipline. It runs
on the tracker's worker thread, filters to open palms, feeds them straight into the
thread-safe attractor field, and dispatches the status line only when the open-palm count
actually changed. Without that guard the UI thread would take a dispatch on every processed
frame. Fields another thread may null out are read into a local first, so a concurrent
`Dispose()` cannot turn a null check into a race.

### The palm tracker: a worker thread and stable ids

`PalmTracker` owns its own background thread, its own `AutoResetEvent` and a single-slot
pending buffer. Callers push frames with `SubmitFrame()` from any thread; the tracker raises
`TrackingUpdated` on its worker thread and says so in the doc comment, leaving marshaling to
the consumer. The producer-to-worker hand-off swaps the two buffers under the lock rather than
copying, so `SubmitFrame()` copies exactly once and the caller may reuse its own array
immediately, which is what lets the view model hold a single reusable frame array. The models
are loaded and the OpenCV nets constructed *inside* the worker and disposed in its `finally`,
so native handles never cross threads. Two catch blocks sit in a deliberate order: a filtered
one for the shutdown race, which exits the loop quietly when a frame is in flight while the
tracker or the native runtime is tearing down, and a plain one below it that drops a single
bad frame and keeps tracking. `Stop()` clears the running flag, signals the event, joins the
worker and only then disposes the signal, so disposal is genuinely synchronous. See
[Run a sensor pipeline on a worker thread with latest frame wins](../BLUEPRINTS-MVVM.md#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins).

The same class also solves the identity problem. A per-frame detector returns unordered
results, but the animation needs to know that this frame's second hand is last frame's first
hand, so the tracker keeps its track list as worker-thread-only state, builds every
candidate-and-track pair within a maximum distance, sorts those pairs by distance and assigns
greedily, skipping pairs whose candidate or track is already taken; that is a few lines and it
avoids the mis-assignment a naive first-match loop produces when two hands cross. Matched
tracks have their position smoothed by an exponential moving average, unmatched tracks are
dropped, and results are sorted by id before being reported so consumers see a consistent
order. A hand that leaves and returns gets a *new* id, which the doc comment states and which
the renderer's slot logic is designed around. Every tuning value is a documented public
constant on the class rather than a literal buried in a loop. The frame-level early-out matters
too: when nothing survives the presence threshold the track list is cleared and a shared empty
result is returned, so the "hands all gone" event still fires and subscribers release their
state, which is exactly what lets the visual melt back. Read
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs` top to bottom. See
[Track multiple detections across frames with stable ids](../BLUEPRINTS-MediaAndVision.md#track-multiple-detections-across-frames-with-stable-ids).

### Two models through the OpenCV DNN module, and the gesture neither one classifies

The Vision library runs a detector and a landmarker, one `internal` class each, under
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/`. Each is constructed from model
bytes, holds its `Net` and its reusable `Mat` buffers, and exposes one method that takes a
frame and returns a plain result object, so nothing above the library ever sees an OpenCV
type. The model bytes come from embedded resources with explicit logical names, read back
through a small helper that throws a clear exception naming the missing resource; the test
suite asserts both the found and the not-found path. Preprocessing is the part that must match
the model: letterbox the frame into the square input, then build the blob with the scaling and
channel swap the model expects. Output tensors are addressed by name, and the code records a
deliberate choice between two read styles, with the reason in a comment at each call site: use
separate single-name forward calls when an early-out can avoid reading a large tensor at all,
and the all-outputs form when every output is needed, because the second read reuses the first
forward's results. Every `Mat` a forward returns is disposed, by `using` for the single reads
and by a `finally` loop for the multi-output read. Decoding is the application's job, not the
binding's: the detector regenerates the model's fixed anchor grid, applies a sigmoid to the
score logits, runs its own greedy non-maximum suppression, and converts survivors back out of
letterboxed space. Doing that arithmetic in small `internal static` methods is what makes it
unit-testable with no model and no image. See
[Run a TFLite model through the OpenCV DNN module](../BLUEPRINTS-MediaAndVision.md#run-a-tflite-model-through-the-opencv-dnn-module)
and
[Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).

The upstream bundle these models come from also contains gesture-classifier stages, and those
are deliberately not embedded: OpenCV's TFLite importer cannot load them, because they use an
operator it does not support. The reason is recorded in two places, the class comment on
`OpenPalmClassifier` and the `ItemGroup` comment in the Vision csproj. The replacement is a
small `internal static` class of pure functions over the landmark array: a hand is open when
each of the four fingers has its tip farther from the wrist than that finger's middle joint,
by a documented ratio constant whose doc comment says which way to move it and what that
trades away; the palm center is the mean of the wrist and the four knuckles. Because the
functions work in any consistent coordinate space, the tests build synthetic hands out of
plain points and assert on ratios with no model and no image at all. Read
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/OpenPalmClassifier.cs` and then its
test file. See
[Recognize a gesture from landmark geometry instead of a model](../BLUEPRINTS-MediaAndVision.md#recognize-a-gesture-from-landmark-geometry-instead-of-a-model).

### Starting the engine loop inside an ordinary page

This is the part most worth copying. The engine can only start against a surface that already
has a non-zero size, and the game canvas here starts hidden behind the camera preview, so its
first real size arrives the first time Visualize Mode is shown. The page forwards the canvas's
`FirstStarted` event to the view model in a single line through `IManageGameCanvas`, and the
view model builds its `VisualizerSession` and starts it. No engine code lives in the
code-behind. Starting from the page's `Loaded` event, or from the command that switches modes,
would run against a zero-sized surface. Order matters in the command as well: setting
`IsCameraMode = false` makes the canvas visible, and therefore raises `FirstStarted` the first
time through, so it comes *before* the resume call, which is null-safe because on the first
pass the session does not exist yet. Read the command implementations in
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` alongside the three
canvas-related lines at the end of
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`. See
[Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size).

`VisualizerSession` owns everything else about the engine lifecycle and exposes `Start()`,
`Pause()`, `Resume()`, `Stop()` and a thread-safe data-in method; nothing else in the
application touches the engine singleton. `Start()` runs once per process and the doc comment
says so: leaving and re-entering the mode is a pause and a resume, not a rebuild. Each of the
four methods is guarded, so double calls are harmless. The GPU-or-CPU choice must be made
before the canvas's host is read for the first time, and the pause deliberately resets the
attractor field first, so a resumed scene starts undisturbed rather than with a stale hand
still pulling at it. The session subscribes to the render adapter's resize event and updates
the drawing's bounds, because the render resolution deliberately tracks the window, and
`Stop()` unsubscribes before stopping the engine. See
[Run and pause a game engine session inside a page](../BLUEPRINTS-GameEngine.md#run-and-pause-a-game-engine-session-inside-a-page)
and
[Offer a CPU fallback for a GPU rendering path behind one switch](../BLUEPRINTS-GraphicsAndRendering.md#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch).

### The shader backdrop and what it reads each frame

`EtherealBackdrop` is a `DirectDrawingBase` subclass that holds its SkSL source as a constant,
compiles it once in its constructor through an `internal static` factory the tests call
directly, and reads its per-frame inputs from the thread-safe attractor field handed in at
construction. It never talks to the view model. Its draw method allocates nothing: the uniform
arrays, the state buffer, both paints and the star seeds are fields created once, and only the
shader produced per frame is disposable, inside a `using`. Time comes from the engine's
running-seconds value rather than wall-clock time, so a pause does not make the animation
jump, and the star seeds come from a fixed-seed random, so an undisturbed frame is a pure
function of engine time. The frame update calls a forced refresh unconditionally, because the
CPU render path uses dirty rectangles and would otherwise stop animating while the GPU path
re-renders the whole surface anyway. The design rule worth stealing is in the shader itself:
every palm term is multiplied by that palm's strength, so a zeroed parameter set reduces
exactly to the undisturbed plasma, which is what makes the visual melt back instead of
snapping. Setting an unknown uniform name throws, and the test suite turns that into a
guarantee by setting every uniform the backdrop sets, so a renamed uniform fails the test run
rather than rendering black. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs` and then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealBackdropTests.cs`. See
[Draw an animated SkSL shader as a game engine direct drawing](../BLUEPRINTS-GraphicsAndRendering.md#draw-an-animated-sksl-shader-as-a-game-engine-direct-drawing).

### The attractor field, and the seam between vision and rendering

The Vision library knows nothing about rendering and the Rendering library knows nothing about
hands. Their only seam is `PalmAttractor`, a read-only struct in the Rendering library holding
an id and a normalized position, and one method on the session that accepts a list of them.
Normalized coordinates rather than pixels mean a window resize needs no re-mapping anywhere;
the stable id travels across the seam because it is what lets the easing follow a moving hand
instead of restarting; and the struct carries no confidence score and no open-or-closed flag,
because the view model filters to open palms before translating, which keeps "what counts as
attracting" an application policy rather than a rendering one. Its doc comment names the
caller's mirroring responsibility explicitly, at the boundary where that matters.

`PalmAttractorField` is what sits between the worker rate and the frame rate. The producer
calls a set-targets method from its own thread; the renderer calls a step method once per
frame with the real delta, clamped so a stall cannot make the field lurch, and then copies the
state into a caller-owned buffer whose length it validates, which is what keeps the renderer
allocation-free. The easing is `1 - exp(-rate * dt)` rather than `rate * dt`, so it is
framerate-independent and never overshoots, and attack and release use different rates so
appearing feels different from disappearing. Slots are keyed by id: a freshly claimed slot is
placed *at* the incoming position with zero strength so the influence swells in place instead
of sweeping over from stale state, and a slot fading out keeps its id until its strength falls
below a small epsilon, so a hand that reopens re-attaches to its own fade. Capacity is fixed
and extras are dropped rather than queued. The class is lock-based throughout and documents
that all members are thread-safe, which is the claim that lets the view model feed it straight
from the vision worker thread with no marshaling. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractor.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractorField.cs`, then its test file,
which locks each of those behaviors in by name. See
[Keep a pipeline and a renderer decoupled by a normalized seam](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-pipeline-and-a-renderer-decoupled-by-a-normalized-seam)
and
[Smooth worker rate data into frame rate animation](../BLUEPRINTS-GraphicsAndRendering.md#smooth-worker-rate-data-into-frame-rate-animation).

### Disposing a view model that owns a thread, a session and native handles

`MainViewModel.Dispose()` is short and every line of it earns its place. Commands are disposed
and nulled; the bridge delegate is nulled, which releases the page reference it captured; the
tracker is unsubscribed before being disposed, and its own `Dispose()` joins its worker
thread; the engine session is read into a local and nulled *before* it is stopped, so a
concurrent worker-thread handler sees null and returns instead of touching a stopping engine;
the capture service is unsubscribed and disposed; and the base call comes last. The
unsubscription is symmetric in both directions, because the tracker and the capture service
each clear their own event before stopping in their own `Dispose()`. See
[Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### What each project owns: packages, native assets and models

Three project-file rules do most of the structural work here. First, every head carries
exactly one platform runtime package and nothing else the other heads also need; the comment
`EXACTLY ONE platform head package` appears in all six head projects, and everything shared
comes from `PalmVisualizer.Core`. Second, the library that calls into native OpenCV references
only the managed binding, so it stays runtime-independent, and each head declares the native
packages for the runtime identifiers it can run on, unconditionally and both architectures at
once, so a head builds and publishes for either without editing the project; only the Vision
test project uses OS conditions, because a test run needs only the current machine's binary.
The Rendering test project does the same thing for Skia, referencing the Linux, macOS and
Windows native-asset packages because its shader tests evaluate real SkSL on a raster surface.
Third, the two models are linked into the Vision library as embedded resources from the
application-root `models/` folder with an explicit logical name, so the files live once where
the notices file can point at them, and the C# constants that name them are the contract
rather than a path. One more rule is worth noticing in
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj`: it sets an
explicit `RootNamespace` distinct from the application's, with a comment explaining that a
library which also sees CodeBrix.Platform would otherwise generate a duplicate of the same
generated resources type as the Core project, and the head that references both would fail to
compile. See
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads),
[Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
and
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).

### Tests that need neither a window nor a camera

Every test project here tests a library, never the UI, and each references exactly its own
library. That is what makes the suite runnable on a machine with no camera and no GPU: camera
enumeration legitimately returns an empty list, the shader tests evaluate real SkSL on a Skia
raster surface with no engine and no window, and the vision tests run real inference on the
CPU. Each library carries an `InternalsVisibleTo.cs` naming only its own test assembly, and
where a test needs something otherwise private the library adds a documented `internal` test
accessor rather than widening the field. The shader's compile step is factored into an
`internal static` method for the same reason. The test bodies follow the family style, with
`<Class>Tests.cs` file names, snake_case method names and `//Arrange` / `//Act` / `//Assert`
comments, and a test that waits on a background thread passes the framework's cancellation
token. The one fixture that has to be a real file, a photograph of two open hands, needs both
halves of the rule: a copy item in the project file and a base-directory lookup in the test.
See
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied)
and
[Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder covers the bundled
machine-learning models under `PalmVisualizer/models/`: the MediaPipe hand-tracking models
from Google, a hand detector and a hand-landmarks detector extracted from the
gesture-recognizer bundle, copyright Google LLC and licensed under the Apache License, Version
2.0. The notices file also states that third-party code dependencies are consumed as NuGet
packages and that each package carries its own license and notices, so those are not
reproduced there. One further attribution lives in a project-file comment rather than in the
notices file: the Vision test fixture `_data/open_palm_hands.jpg` is a public-domain
photograph of two open hands from Wikimedia Commons. Note that the CodeBrix packages this
application uses are not all under the same license; the family's package-name suffix records
each one's license, and the capture library's terms differ from those of the platform, engine
and vision packages, so check the suffix in each csproj before shipping a derivative.

## License

PalmVisualizer is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
