# PalmVisualizer

PalmVisualizer is a two-mode webcam toy. It opens in Camera Mode: a dropdown lists the
cameras connected to the machine, the first one starts automatically, and a mirrored live
preview fills the window. Pressing "Visualize!" switches to Visualize Mode, where the live
video is replaced by an animated visual drawn on the GPU, and every frame from the camera is
fed to a palm-tracking pipeline. A second dropdown in the header, enabled in both modes,
chooses the visual, and choosing another while the visual is showing switches it at once:

- "Ethereal - WebGPU", the default, is a plasma under a drifting starfield, drawn with WGSL
  shaders through WebGPU. Wherever the user holds up an open palm the colors bend toward the
  hand, rings of color drift inward, the plasma brightens and nearby stars are pulled along,
  so the whole visual appears to chase the hand.
- "Ethereal - OpenGL" is the same scene drawn with GPU-accelerated Skia and an SkSL shader.
- "Ripple - WebGPU" is a shimmering, lightly iridescent pool of blue water over a pastel glass
  mosaic of an eight-petalled rosette: a wave equation runs in a compute shader, a light rain
  keeps the surface alive, and each open palm rocks the water under it and drags a wake behind
  it as it moves, so the waves follow the hands.
- "Holy Inanna - WebGPU" is the eight-pointed Star of Inanna (Ishtar) as a golden 3D object,
  slowly spinning and gently tilting in a space nebula of indigo, purple, magenta and mauve
  with glowing orange cores. Open palms swirl the nebula and glow vivid orange around them, a
  moving palm churns it harder, and the star itself is only ever influenced, never disturbed:
  it drifts a little toward the palms, slowly, and comes closer as they grow livelier.
- "Crystals - WebGPU" is a garden of glinting, translucent quartz crystals in indigo, purple,
  magenta, mauve and orange, sprouting in clusters over a dark indigo backdrop, growing upward,
  gently pulsing, and fading after a while to make room for new ones. Open palms cast a soft
  light, excite the crystals near them - which then grow faster, turn more vibrant, pulse
  harder and glint more - and make new crystals sprout where they are, faster when they move.
- "Flowers - WebGPU" is a meadow of vivid eight-petalled flowers on dark pool-table-green
  felt. Left alone it keeps four: one sprouts, grows and slowly blooms, then the next, and
  each new one sets the oldest withering. An open palm lights a soft oval of the felt bright
  green - about a quarter of the view for one palm, almost all of it for two; flowers sprout rapidly in the light and grow faster there, nothing in the
  light withers, flowers left in the dark do, and when the palms go the meadow withers back to
  its calm four.

Closing the hand, or moving it out of view, releases the pull and the visual settles back to
its undisturbed motion. Up to four open palms can act on the visual at once, and each hand
keeps a stable identity while it stays in frame. A "Back" button returns to Camera Mode, and
a single status line narrates everything the application is doing: camera discovery, the
live camera name, how many open palms the visual is following, and any reason the chosen
visualizer cannot draw the way it was chosen to.

It is a reference for three things a CodeBrix.Platform application often needs together:
drawing continuously animated GPU visuals inside an ordinary XAML page - WGSL render and
compute shaders through WebGPU, and GPU-accelerated Skia, switchable at run time - capturing
webcam video with the CodeBrix.Webcam library and displaying it on a Skia canvas, and running
neural-network inference off the UI thread with the CodeBrix.VideoProcessing.OpenCV5 library
and marshaling the results back into a view model. A sibling application in this repository,
WebcamPainter, uses a related vision pipeline for a different purpose.

## What this sample shows a CodeBrix.Platform developer

- Let each visualizer session create its own view, put it into a panel the page hands over
  through a one-property interface, and take it out again on `Stop()`, so switching
  visualizers releases the outgoing GPU context or device and leaving a mode costs only a
  pause:
  [Let a rendering session put its own view into the page's panel](../BLUEPRINTS-GraphicsAndRendering.md#let-a-rendering-session-put-its-own-view-into-the-pages-panel).
- Offer six visualizers - one of them through two graphics APIs - from a dropdown bound to a
  list of options, and replace the running session at once when the choice changes:
  [Swap the 3D graphics backend at run time from a dropdown](../BLUEPRINTS-GraphicsAndRendering.md#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown).
- Draw a full-screen WGSL shader and instanced star quads into a `WebGpuRenderView`, with
  nothing between the code and the raw WebGPU binding but a small pass helper:
  [Draw a scene with WebGpuRenderView and raw WGSL](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-scene-with-webgpurenderview-and-raw-wgsl).
- Advance a wave simulation in a WGSL compute shader over two ping-ponged storage buffers,
  then shade the result in a render pass of the same frame:
  [Run a WebGPU compute pass over ping-ponged storage buffers before the render pass](../BLUEPRINTS-GraphicsAndRendering.md#run-a-webgpu-compute-pass-over-ping-ponged-storage-buffers-before-the-render-pass).
- Ray-march a ridged, extruded 3D solid from its signed distance function in a full-screen
  WGSL fragment shader, with an antialiased silhouette and a CPU-side rotation:
  [Ray-march an SDF solid in a full-screen WGSL fragment shader](../BLUEPRINTS-GraphicsAndRendering.md#ray-march-an-sdf-solid-in-a-full-screen-wgsl-fragment-shader).
- Draw up to 96 crystals as instanced quads read from one uniform array, blended in
  premultiplied colour over a full-screen backdrop in the same render pass:
  [Draw many instanced quads over a full-screen backdrop in one WebGPU render pass](../BLUEPRINTS-GraphicsAndRendering.md#draw-many-instanced-quads-over-a-full-screen-backdrop-in-one-webgpu-render-pass).
- Light the felt in the shader with the very formula the flower model uses to decide what is
  lit, and pin the two together with a unit test:
  [Share one formula between a C# simulation and a WGSL shader and pin it with a test](../BLUEPRINTS-GraphicsAndRendering.md#share-one-formula-between-a-c-simulation-and-a-wgsl-shader-and-pin-it-with-a-test).
- Rasterize SkiaSharp frames on the GPU through the platform's `SkiaGpuContext`, read each one
  back once and present it on an ordinary `SKXamlCanvas`:
  [Rasterize Skia frames on the GPU with SkiaGpuContext and present them on an SKXamlCanvas](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-skia-frames-on-the-gpu-with-skiagpucontext-and-present-them-on-an-skxamlcanvas).
- Compute each frame once in an API-neutral scene that both renderers draw from, so the two
  versions stay the same picture:
  [Compute a frame once and let two graphics APIs draw it](../BLUEPRINTS-GraphicsAndRendering.md#compute-a-frame-once-and-let-two-graphics-apis-draw-it).
- Write a full-surface animated SkSL shader that compiles once, allocates nothing per frame
  and runs on whichever canvas it is handed:
  [Draw an animated SkSL shader onto a GPU or raster Skia canvas](../BLUEPRINTS-GraphicsAndRendering.md#draw-an-animated-sksl-shader-onto-a-gpu-or-raster-skia-canvas).
- Run the identical Skia scene on Skia's raster backend, chosen by one environment variable or
  forced by a head with no GPU context, and say so in the status line:
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
- Open a camera on a worker thread from a selection setter, so a slow device cannot stall the
  UI thread:
  [Do blocking work in a service behind Task Run](../BLUEPRINTS-MVVM.md#do-blocking-work-in-a-service-behind-task-run).
- Let a device that finishes opening late say nothing, because the user has already picked
  another one:
  [Ignore a stale async result when the selection moved on](../BLUEPRINTS-MVVM.md#ignore-a-stale-async-result-when-the-selection-moved-on).
- Start the switch from the selection setter with a version number, and let only the newest
  switch write the status line:
  [Switch devices off the UI thread and let the newest switch report](../BLUEPRINTS-MVVM.md#switch-devices-off-the-ui-thread-and-let-the-newest-switch-report).
- Open a capture device off the UI thread while the service that owns it keeps two devices
  from being opened at once:
  [Serialize a device's control calls inside the service that owns it](../BLUEPRINTS-MediaAndVision.md#serialize-a-devices-control-calls-inside-the-service-that-owns-it).
- Let the page's frame renderer pull pixels through a one-call interface the camera library
  declares, so no capture service and no view model type appears in the render path:
  [Put the one call frame source in the camera library](../BLUEPRINTS-MediaAndVision.md#put-the-one-call-frame-source-in-the-camera-library).
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
- Give each library an `InternalsVisibleTo.cs` naming its own test assembly (Camera and Vision
  also name the PlayTests suite), and add documented internal test accessors rather than
  widening fields:
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
- Register the capture service, the palm tracker and the session factory behind interfaces
  with one extension method the App constructor calls:
  [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- Make a bundled font the default for every head by pointing at the `.ttf` through an
  `ms-appx:///` URI:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Install a console logger factory only in Debug builds, from a static method each head calls
  before building its host:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

## Building, running and testing

There is one solution file, `PalmVisualizer/PalmVisualizer.slnx`, and it holds everything:
the shared UI project, the Core project, all six heads, the three libraries under a
`Libraries` solution folder, and the test projects under a `Tests` solution folder. Its
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

- The .NET 10 SDK. Nothing is installed by hand: the native OpenCV library, the Skia
  runtime and the native WebGPU engine all arrive as NuGet packages (see each project's
  csproj and the packages they bring in).
- A GPU is used when there is one but is not required: without a WebGPU device the five
  WebGPU visualizers report why in the status line, and "Ethereal - OpenGL" falls back to
  drawing on the CPU.
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

Visualize Mode starts with "Ethereal - WebGPU". It, "Ripple - WebGPU", "Holy Inanna -
WebGPU", "Crystals - WebGPU" and "Flowers - WebGPU" draw through whatever device the WebGPU
engine chooses: Direct3D 12 or Vulkan on Windows, Vulkan on Linux, Metal on macOS.
"Ethereal - OpenGL" draws the same scene with GPU-accelerated Skia through the platform's
`SkiaGpuContext`, which is OpenGL or OpenGL ES on every head except macOS, where it is Metal.
Set `PALMVISUALIZER_USE_CPU=1` before launching to make "Ethereal - OpenGL" draw the identical
scene on Skia's CPU raster backend instead; a head that cannot provide a GPU context gets the
same fallback without being asked, and the status line says so.

WebGPU is the default because it is the faster path, most of all on the WPF head: its
read-back of each frame overlaps the next frame's work, where the OpenGL path waits for its
copy on the UI thread. The WPF head needs one more line for a continuously animating page.
Its `Program.cs` forces the software render surface, so every frame also re-composites the
whole window on the CPU, and in a large window a frame can outlast the frame interval; at the
default scheduling tier that starves keyboard and mouse input. The head therefore also sets
`wpfHost.DispatcherScheduling = WpfDispatcherScheduling.InputFair`, with a comment saying why,
so input interleaves with the frames.

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

All the test projects use xUnit v3 with SilverAssertions, build as `Exe`, and set
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
| `PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests` | Attractor easing semantics; palm motion (no palms means no activity, an open palm raises it and a moving one raises it further, it falls slowly after the palms close, the first frame has no delta and a stall is clamped); the Ethereal scene (stars inside the surface, an undisturbed frame that is a pure function of time, star sizes that follow the display scale, palms delivered in shader space, stars pulled toward and brightened by a palm); the SkSL shader (it compiles, accepts every uniform the renderer sets, paints colors on a raster surface, and a whole frame leaves no pixel black); Ripple's rain (light when calm, more and deeper drops with lively palms, every drop inside the pool, no saved-up downpour after a stall); the Holy Inanna star's motion (its own path without palms, never drawn far from it, drawn gently toward a palm, closer with activity and spinning on) and its view-to-star rotation; the crystal garden (young crystals from the start, sprouting and growing without ever overfilling, old crystals fading to make room, a palm exciting nearby crystals so they grow faster, crystals sprouting where the palms are, the instance layout the shader reads, every crystal growing upward within forty degrees of vertical, calm crystals staying close to their palette colours); the flower meadow (calm flowers sprouting one at a time, each new one withering the oldest once four have bloomed, calm flowers spread evenly over the view, every flower a vivid rainbow colour but never green, two palms in the two halves lighting the whole view, one palm alone lighting about a quarter of it, the light reaching further as a second palm comes into view, one palm leaving the far side dark, a lush meadow under two palms where nothing withers, the far side withering under one palm, the meadow returning to four when the palms go, the instances written back to front, petals ranging from narrow to broad); the WGSL sources of all five WebGPU renderers pinned to the uniform layouts, bindings and entry points the C# side uses, including exactly eight even petals and the felt's palm light matching the meadow's; and the session factory and the dropdown's options, Ethereal - WebGPU first | Native Skia, pulled in by referencing the Linux, macOS and Windows Skia native-asset packages. No GPU and no WebGPU device: the WGSL is checked as text, because it compiles only on a device |
| `PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests` | The detector's pure math, the geometric open-palm classifier, embedded-model loading, tracker start/stop idempotence, and end-to-end inference against a bundled photograph | Native OpenCV, referenced with per-OS `Condition` attributes, plus the bundled test photo copied to the output directory |
| `PalmVisualizer/tests/PalmVisualizer.PlayTests` | The real page and view model: camera discovery and its failures, auto-selection and switching, the first-frame gate on Visualize, mode switching, tracker and session lifecycle, the visualizer dropdown (all six options, Ethereal - WebGPU by default, a choice made while visualizing replacing the session at once, each new visualizer getting a session of its own kind that receives the mirrored palms, a choice made in Camera Mode used by the next Visualize, problems reported only by the current session), mirrored palms, the mirrored preview pixels, theme and orientation | Nothing native beyond the PlayTest head; fakes replace the camera, the tracker and the visualizer session factory, so no camera is opened and no visualizer view, GPU context or WebGPU device is ever created |

The Vision tests run real inference on the CPU. No GPU and no network are needed.

## How the projects and folders are organized

```text
PalmVisualizer/
  PalmVisualizer.slnx                  The one solution: UI, Core, six heads, three libraries, the test projects
  global.json                          Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt              Covers the bundled hand-tracking models
  models/                              The committed MediaPipe hand models, embedded into the Vision library at build time
  src/
    PalmVisualizer.UI/                 Shared project (.shproj + .projitems): App.xaml(.cs), Views/MainPage.xaml(.cs)
    PalmVisualizer.Core/               The library every head references; carries the shared packages
      Helpers/HostHelper.cs            The host-builder provider SimpleServiceResolver builds its container from
      RegisterServices.cs              AddPalmVisualizer(): binds the three service interfaces to their implementations
      ViewModels/MainViewModel.cs      All application logic; also declares ICanvasBridge and IManageVisualizerCanvas
    PalmVisualizer.LinuxX11/           Head: X11. Program.cs plus its packages
    PalmVisualizer.LinuxWayland/       Head: Wayland
    PalmVisualizer.LinuxFrameBuffer/   Head: Linux framebuffer
    PalmVisualizer.MacOS/              Head: macOS
    PalmVisualizer.Win32Skia/          Head: Windows Win32
    PalmVisualizer.WinWpfSkia/         Head: Windows WPF host; net10.0-windows + EnableWindowsTargeting
    libs/
      PalmVisualizer.Camera/           Webcam capture service and its interfaces, the CameraDevice wrapper, the canvas and its frame renderer
      PalmVisualizer.Vision/           Palm-tracking pipeline: worker thread, embedded models, OpenCV DNN inference
        Internal/                      PalmDetector, HandLandmarker, OpenPalmClassifier - not part of the public surface
      PalmVisualizer.Rendering/        Visualize Mode: the dropdown's options and the session factory; VisualizerSession and
                                       WebGpuVisualizerSession and the six sessions; the Ethereal scene and its WGSL and
                                       SkSL renderers, SkiaGpuCanvas; the Ripple, Holy Inanna, Crystals and Flowers
                                       renderers with RippleRain, HolyInannaMotion, CrystalGarden and FlowerMeadow;
                                       PalmMotion, the WgslGpu and WgslPass helpers, palm attractor smoothing
  tests/
    libs/
      PalmVisualizer.Camera.Tests/     Mirrors src/libs/PalmVisualizer.Camera
      PalmVisualizer.Rendering.Tests/  Mirrors src/libs/PalmVisualizer.Rendering
      PalmVisualizer.Vision.Tests/     Mirrors src/libs/PalmVisualizer.Vision; _data/ holds the test photograph
    PalmVisualizer.PlayTests/          UI tests of the real page and view model through CodeBrix.Platform.PlayTest
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
view model is the only place the three libraries meet. Each library test project references
exactly its own library and nothing else; the PlayTests project references Core and imports
the shared UI.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The application framework: `Application`, `Window`, `Frame`, `Page`, XAML binding, the default-font feature configuration, and the "Simple" toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `[AffectsCommands]`, `InvokeOnMainThread`) | `PalmVisualizer/src/PalmVisualizer.Core/PalmVisualizer.Core.csproj`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`, `PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface; `CodeBrixPlatformHostBuilder` selects it in `Program.Main` | The six head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Roboto | Supplies the Roboto family the whole application uses as its default text font | `PalmVisualizer/src/PalmVisualizer.Core/PalmVisualizer.Core.csproj`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml`, `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs` |
| CodeBrix.Platform.SkiaSharp.Views | Supplies `SKXamlCanvas`, which `CameraCanvas` subclasses so the live preview can be named in XAML, and which `SkiaGpuCanvas` subclasses to present the frames of "Ethereal - OpenGL" | `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Camera/PalmVisualizer.Camera.csproj`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/SkiaGpuCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj` |
| CodeBrix.Platform.WebGpu | `WebGpuRenderView`, the element the five WebGPU visualizers draw in (it owns the device, its frame driver and its presentation), the `IWebGpuRenderer` contract with `WebGpuFrame` and `IWebGpuDeviceSource`, and, through its dependencies, the raw `CodeBrix.WebGpu.Native` binding - render and compute pipelines, storage and uniform buffers, bind groups - and the native WebGPU engine | `PalmVisualizer/src/libs/PalmVisualizer.Rendering/WebGpuVisualizerSession.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealWebGpuRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/RippleWebGpuRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/HolyInannaWebGpuRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/CrystalsWebGpuRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/FlowersWebGpuRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/WgslPass.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/WgslGpu.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj` |
| CodeBrix.Platform.Graphics3DGL | `SkiaGpuContext`, the backend-neutral GPU context (OpenGL or OpenGL ES, Metal on macOS) whose `GrContext` backs the off-screen surface "Ethereal - OpenGL" draws into, inside a `BeginFrame()` scope | `PalmVisualizer/src/libs/PalmVisualizer.Rendering/SkiaGpuCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj` |
| CodeBrix.Platform (the `CodeBrix.Platform.UI.Toolkit` converters namespace) | `BoolToVisibilityConverter`, declared twice (the second time with `Invert="True"`) to swap the two mode views and the two button groups from one bool | `PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml` |
| CodeBrix.Webcam | Camera enumeration and the live capture session that delivers BGRA frames and caches the latest one | `PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs` |
| CodeBrix.VideoProcessing.OpenCV5 | The managed binding: `Mat`, the color-conversion, resize and warp calls, and the DNN module that runs both models | `PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/HandLandmarker.cs` |
| CodeBrix.VideoProcessing.OpenCV5 per-runtime native packages | The native OpenCV library, referenced once per runtime identifier by each head and conditionally by the Vision test project | The six head csproj files, `PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/PalmVisualizer.Vision.Tests.csproj` |
| SilverAssertions | The assertion style in all the test projects | The test csproj files and every test file |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | `SKSurface` (raster, and GPU-backed from a `GRContext`), `SKCanvas`, `SKBitmap`, `SKPaint`, and the runtime-effect types that compile and feed the SkSL shader; its native-asset packages are referenced by the Rendering test project so the shader tests can run | `PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealSkiaRenderer.cs`, `PalmVisualizer/src/libs/PalmVisualizer.Rendering/SkiaGpuCanvas.cs`, `PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()`, wrapped by `HostHelper` and handed to `SimpleServiceResolver`, and the `IServiceCollection` the application's services are registered into | `PalmVisualizer/src/PalmVisualizer.Core/Helpers/HostHelper.cs`, `PalmVisualizer/src/PalmVisualizer.Core/RegisterServices.cs` |
| Microsoft.Extensions.Logging.Console | The Debug-only console logger factory installed in `App.InitializeLogging()` | `PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for all the test projects | The test csproj files, `PalmVisualizer/global.json` |

## Worth studying in this application

### Two modes, one bool, two stacked views

The whole application is one page with two states, and the state is a single bound bool. The
view model exposes `IsCameraMode` with a private setter, plus a computed `IsVisualizeMode`
that its setter notifies. `MainPage.xaml` declares `BoolToVisibilityConverter` twice, the
second instance with `Invert="True"`, and binds both the two canvases and the two button
groups to the same property; the visualizer half is a plain `Grid` that the running session
puts its own view into. The camera preview and the `VisualizerCanvas` panel live in the
same `Grid` cell and are stacked, so only visibility changes; that is what keeps the
visualizer's view alive, with its session merely paused, across mode switches. The camera
dropdown is disabled rather than hidden in Visualize Mode so the layout does not shift, while
the visualizer dropdown beside it stays enabled in both modes, and a single `StatusText` `TextBlock` at the bottom
narrates every state, which is why the view model can report errors without any dialog. Read
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml` first, then the bindable-properties
region of `PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Switch a page between two modes with one bool and a converter](../BLUEPRINTS-ViewsAndControls.md#switch-a-page-between-two-modes-with-one-bool-and-a-converter)
and
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### Three collaborators, resolved rather than constructed

The view model drives a capture service, a palm tracker and a visualizer session, and it names
none of their concrete types. The Camera library declares `IWebcamCaptureService`, the Vision
library declares `IPalmTracker`, and the Rendering library declares `IVisualizerSessionFactory`
alongside the `IVisualizerSession` it hands back. `RegisterServices.AddPalmVisualizer()` in the
Core project binds each interface to its implementation, and the `App` constructor calls that
one method inside the `SimpleServiceResolver.CreateInstance` callback, which is where the
template leaves the registration block. `MainViewModel` resolves all three with
`GetService<T>()` on its first working constructor lines, each with a plain `new` behind `??`
so a head that registered nothing still runs. That is what makes the view model drivable
without hardware: a capture service that replays recorded frames, a tracker that reports
scripted palms and a factory that returns a session doing nothing all satisfy the same
interfaces. Read `PalmVisualizer/src/PalmVisualizer.Core/RegisterServices.cs` and
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`, then the constructor of
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method)
and
[Register a factory and let the view model ask it for what it owns](../BLUEPRINTS-AppStructureAndStartup.md#register-a-factory-and-let-the-view-model-ask-it-for-what-it-owns).

### Camera discovery, selection and the mirrored preview

`WebcamCaptureService` in the Camera library is the whole capture model: camera discovery,
`Start()`, `Stop()`, `HasFrame`, `TryCopyLatestFrame()` and a `FrameArrived` event, all of it
reachable through `IWebcamCaptureService`. The view model resolves one, holds the discovered
devices in an `ObservableCollection`, and starts a camera switch from the `SelectedCamera`
setter. Discovery is kicked off from the constructor as fire-and-forget after setting a
"Discovering cameras…" status, its results are marshaled with `InvokeOnMainThread`, and every
failure path writes to the same status line rather than throwing out of the constructor. An
empty device list is treated as a normal state, not an error. Opening a device can take long
enough to be felt, so the setter only kicks the switch off: the start (or the stop, when the
selection is cleared) runs on a worker thread, the capture service serializes its own start
and stop so two devices are never opened at once, and a version counter means only the newest
switch reports - a camera that finishes opening late cannot overwrite the status of the one
the user has since chosen. `Start()` calls
`Stop()` first so switching cameras never leaves two sessions running, and `Stop()`
unsubscribes before disposing and clears the frame flag so a stale frame from the previous
camera cannot be drawn; the flag itself is `volatile` because the capture thread writes it and
the UI thread reads it. The dropdown binds to `CameraDevice`,
a sealed wrapper with an internal constructor whose `ToString()` returns the friendly name,
so no item template is needed and the view model never handles a capture-library type. Read
`PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs`. See
[Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session),
[Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it),
[Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor),
[Do blocking work in a service behind Task Run](../BLUEPRINTS-MVVM.md#do-blocking-work-in-a-service-behind-task-run),
[Ignore a stale async result when the selection moved on](../BLUEPRINTS-MVVM.md#ignore-a-stale-async-result-when-the-selection-moved-on),
[Switch devices off the UI thread and let the newest switch report](../BLUEPRINTS-MVVM.md#switch-devices-off-the-ui-thread-and-let-the-newest-switch-report)
and
[Serialize a device's control calls inside the service that owns it](../BLUEPRINTS-MediaAndVision.md#serialize-a-devices-control-calls-inside-the-service-that-owns-it).

Displaying the video is deliberately split. `CameraCanvas` is a one-line `SKXamlCanvas`
subclass that exists purely so the shared UI project's XAML can name the type from the
library's namespace, and all the drawing lives in a separate `WebcamFrameRenderer` that takes
a surface, an image info and an `IWebcamFrameSource`. That one-call interface is the whole
dependency the painting code has: the view model implements it explicitly and forwards to the
service it resolved, so the page pulls frames without naming a capture service at all. The
renderer clears to black first, so "no frame yet" is a black panel rather than garbage; caches
its pixel buffer and its `SKBitmap` and only recreates the bitmap when the frame size changes; and mirrors by a canvas transform
between a save and a restore rather than by flipping pixels. Its doc comment states the
ownership rule: one renderer per canvas, touched only on the UI thread. See
[Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass)
and
[Put the one call frame source in the camera library](../BLUEPRINTS-MediaAndVision.md#put-the-one-call-frame-source-in-the-camera-library).

### The bridge interfaces the page fills in

`MainViewModel` declares its two page-facing seams as interfaces in its own file and
implements them, and implements a third that the Camera library declares. `ICanvasBridge`
holds a single `Action InvalidatePreviewCanvas`, which the page assigns to a delegate that
marshals through its own `DispatcherQueue`; the view model then calls it from whatever thread
it happens to be on, and the null-conditional invocation is the graceful-degradation path when
no page has supplied one. `IManageVisualizerCanvas` has one method,
`SetVisualizerCanvasHost`, through which the page hands itself over as the
`IVisualizerCanvasHost` the Rendering library declares - one property, the panel a session's
view goes into, which the page implements explicitly - as described below.
`IWebcamFrameSource` is the third: one call that copies the newest frame, which the page's
preview renderer pulls on every paint. All three are picked up in `DataContextChanged` with an
`as` cast, which `MainPage.xaml.cs` subscribes to *before* `InitializeComponent()` because
`InitializeComponent()` may be what sets the data context. The same handler hands the view
model a `XamlRoot` getter through `IXamlRootGetter`, which costs one line and means
`SimpleDialog` works the day a dialog is added. Every one of those seams is assigned through
the interface rather than the concrete view model type, so the page compiles against contracts
and a design-time data context simply leaves them null. Inside the invalidate delegate both
the dispatcher and the canvas are null-checked, so a repaint requested during teardown is a
no-op instead of a crash. Read
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` beside the interface
declarations at the top of
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`. See
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
and
[Put the one call frame source in the camera library](../BLUEPRINTS-MediaAndVision.md#put-the-one-call-frame-source-in-the-camera-library).

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

### Choosing a visualizer: a session that owns its view

This is the part most worth copying. A visualizer session is the whole lifecycle of one
visualizer, and it brings its own view. The page hands itself to the view model once, in
`DataContextChanged`, as an `IVisualizerCanvasHost` whose only member is the `VisualizerCanvas`
panel - a plain `Grid` in the XAML. When the user presses "Visualize!", the view model asks the
registered `IVisualizerSessionFactory` for a session of the kind the dropdown names, subscribes
to its `Problem` event and starts it; `Start()` creates the view - a `WebGpuRenderView` for
the five WebGPU visualizers, a `SkiaGpuCanvas` for "Ethereal - OpenGL" - and adds it to the
panel.
`Stop()` takes it out again, and leaving the visual tree is what releases the view's WebGPU
device or GPU context, so the page names no renderer type and never has to tear anything down
itself. Leaving Visualize Mode is a `Pause()` and coming back is a `Resume()`, both guarded so
double calls are harmless. Order matters in the command: setting `IsCameraMode = false`
makes the panel visible before the session is created or resumed, so a new view is added to a
panel that is already showing.

The dropdown binds to `Visualizers` and `SelectedVisualizer`, `VisualizerOption` values whose
`ToString()` is the display name, so no item template is needed. "Ethereal - WebGPU" is first
and the default. A session draws one visualizer for its whole life, so the setter replaces
rather than reconfigures: it stops the current session, and in Visualize Mode it starts a
session for the new choice at once; in Camera Mode that is all, and the next "Visualize!"
starts the new choice. A session raises `Problem` when it cannot render the way it was
chosen to - no WebGPU device, or no GPU context so the frames are being drawn on the CPU - and
the view model writes the message to the status line only when the sender is still the
current session and Visualize Mode is showing, so a replaced session cannot overwrite the
status of the one that took its place. Because the view is created inside the session, the
PlayTests' fake sessions never create one at all - a `WebGpuRenderView` under the PlayTest
head, which has no WebGPU provider, would fail the test. Read the "Visualizer sessions" region of
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` beside the
`IVisualizerSession` interface in the Rendering library,
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSessionFactory.cs` and the end of
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`. See
[Let a rendering session put its own view into the page's panel](../BLUEPRINTS-GraphicsAndRendering.md#let-a-rendering-session-put-its-own-view-into-the-pages-panel)
and
[Swap the 3D graphics backend at run time from a dropdown](../BLUEPRINTS-GraphicsAndRendering.md#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown).

The sessions form a small hierarchy. `VisualizerSession` is the abstract base of every
visualizer and owns everything that is not a view: the `PalmAttractorField` the palms are fed
into from any thread, the visual's clock - a `Stopwatch` that stops while the session is
paused, so the pause is invisible to the visual and it resumes mid-motion - the display name
it takes from `VisualizerOption.For(Kind)` for status-line messages, and `ReportProblem`, which
raises `Problem` only while the session is running. `Pause()` resets the attractor field before
it stops the clock, so a resumed visual starts undisturbed rather than with a stale hand still
pulling at it. `Stop()` clears its started flag before it removes the view, because a WebGPU
view reports its device going away as it unloads and the session must recognize that as its own
stop rather than a problem. A subclass supplies only `CreateView()`, `SetViewPaused()` and
`OnStopped()`. `EtherealOpenGLSession` derives from it directly and creates its own
`EtherealScene`, `EtherealSkiaRenderer` and `SkiaGpuCanvas`. `WebGpuVisualizerSession` is the
second, abstract layer for everything drawn with WebGPU: it creates the `WebGpuRenderView`,
pauses it, reports `DeviceLost` under the visualizer's display name and disposes the renderer
on stop, so each of `EtherealWebGpuSession`, `RippleWebGpuSession`,
`HolyInannaWebGpuSession`, `CrystalsWebGpuSession` and `FlowersWebGpuSession` is a
constructor and a one-line `CreateRenderer` override that hands back its renderer. Adding a
visualizer is a kind, an option, a session and a factory case. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/WebGpuVisualizerSession.cs`, then the six
short sessions beside them.

### Ethereal - WebGPU: a WebGpuRenderView and a WGSL port of the shader

`WebGpuVisualizerSession` creates a `WebGpuRenderView` at a target of 60 frames a second, and
`EtherealWebGpuSession` gives it an `EtherealWebGpuRenderer`, over an `EtherealScene` of its
own, as its `IWebGpuRenderer`. The view owns the device, which
the WebGPU engine chooses - Direct3D 12 or Vulkan on Windows, Vulkan on Linux, Metal on macOS -
and its continuous driver runs the frames on the UI thread; on these Skia heads the view
presents through its Readback path, which reads each frame back one frame behind, so the copy
overlaps the next frame's GPU work instead of stalling the UI thread. The view's `DeviceLost`
event becomes the session's `Problem` with the reason in it. The renderer builds its two
pipelines in `OnDeviceReady` and releases them in `OnDeviceLost`, because pipelines belong to a
device; each frame advances the scene, fills two reused uniform blocks laid out exactly like the
WGSL structs, and records a single render pass: one full-screen triangle through the plasma
shader, then every star as an instanced, antialiased quad blended over it in premultiplied
color. `WgslPass` is the small helper behind each of those draws, written against nothing but
the raw `CodeBrix.WebGpu.Native` binding - a shader module with `vs` and `fs` entry points, a
pipeline whose layout is derived from the shader, one uniform buffer at group 0, binding 0 -
and it releases what it created, never the device or the queue. The plasma WGSL is a
line-for-line port of the SkSL in `EtherealSkiaRenderer`, and both classes say so: a change to
one belongs in the other. The port is not blind, though; WGSL leaves `smoothstep` undefined
when the low edge is above the high edge, so that call is written out by hand, with a comment.
WGSL compiles only on a device, so the unit tests pin what the C# side writes to what the
shaders declare instead - the array lengths, the entry-point names and the binding. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/WebGpuVisualizerSession.cs`,
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealWebGpuRenderer.cs` and
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/WgslPass.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealWebGpuRendererTests.cs`. See
[Draw a scene with WebGpuRenderView and raw WGSL](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-scene-with-webgpurenderview-and-raw-wgsl).

### Ethereal - OpenGL: GPU Skia frames on an SKXamlCanvas

`SkiaGpuCanvas` is an `SKXamlCanvas` subclass that animates itself. A `DispatcherTimer` on the
UI thread drives it at `TargetFrameRate`, so a frame that overruns the interval delays the
next tick rather than building a backlog. Each tick draws the frame at the canvas's physical
pixel size into an off-screen GPU surface created from the platform's backend-neutral
`SkiaGpuContext` (Graphics3DGL: OpenGL or OpenGL ES, Metal on macOS), flushes it, reads it back
into one reusable bitmap, and invalidates; the paint handler then presents that bitmap 1:1.
Every GPU call sits inside one `BeginFrame()` scope, which is what makes the context current.
The context is created lazily, once the canvas has a live `XamlRoot`, and released on
`Unloaded` while the window still exists, as GL teardown requires. When the head cannot give it
a context, or `UseGpu` is off, the same frames are drawn into a raster surface over the same
bitmap and `GpuUnavailable` says why. `EtherealOpenGLSession` sets `UseGpu` from
`PALMVISUALIZER_USE_CPU`, with the negative test as the default so an unset or garbage value
keeps the GPU path, turns `GpuUnavailable` into the session's `Problem`, and handles
`RenderFrame` with two lines: advance the scene, draw it with `EtherealSkiaRenderer`. The
read-back is synchronous, which is the cost this path pays and the reason WebGPU is the
default. Read `PalmVisualizer/src/libs/PalmVisualizer.Rendering/SkiaGpuCanvas.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealOpenGLSession.cs`. See
[Rasterize Skia frames on the GPU with SkiaGpuContext and present them on an SKXamlCanvas](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-skia-frames-on-the-gpu-with-skiagpucontext-and-present-them-on-an-skxamlcanvas)
and
[Offer a CPU fallback for a GPU rendering path behind one switch](../BLUEPRINTS-GraphicsAndRendering.md#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch).

### Ethereal: one scene, two renderers, and the shader they share

The two versions are the same visual because neither renderer computes it. `EtherealScene` is
the API-neutral state of one frame: each call to `Advance()` steps the attractor field by the
frame's real delta, clamped so a stall cannot make it lurch, converts the palms into the
shader's aspect-corrected space, and computes every star's position, radius and brightness,
with each palm pulling nearby stars toward it and swelling them a little. It exposes the
results as flat spans with documented strides, and the star stride is four floats because that
is one `vec4` in a WGSL uniform array, so the WebGPU renderer copies the whole span in one call
while the Skia renderer reads the same four floats per `DrawCircle`. Its star seeds come from a
fixed-seed random and time is a parameter, so an undisturbed frame is a pure function of time,
which a test checks by comparing a fresh scene with one that has been running. It allocates
nothing per frame.

`EtherealSkiaRenderer` holds the SkSL source as a constant, compiles it once in its constructor
through an `internal static` factory the tests call directly, and draws from the scene it is
handed. It never talks to the view model. Its draw method allocates nothing: the uniform arrays
and both paints are fields created once, and only the shader produced per frame is disposable,
inside a `using`. Drawn onto `SkiaGpuCanvas`'s GPU surface the shader executes on the GPU;
drawn onto a raster surface Skia evaluates the same shader on the CPU. The design rule worth
stealing is in the shader itself, in both languages: every palm term is multiplied by that
palm's strength, so a zeroed parameter set reduces exactly to the undisturbed plasma, which is
what makes the visual melt back instead of snapping. Setting an unknown uniform name throws, and
the test suite turns that into a guarantee by setting every uniform the renderer sets, so a
renamed uniform fails the test run rather than rendering black. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealScene.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealSkiaRenderer.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealSceneTests.cs` and
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealSkiaRendererTests.cs`. See
[Compute a frame once and let two graphics APIs draw it](../BLUEPRINTS-GraphicsAndRendering.md#compute-a-frame-once-and-let-two-graphics-apis-draw-it)
and
[Draw an animated SkSL shader onto a GPU or raster Skia canvas](../BLUEPRINTS-GraphicsAndRendering.md#draw-an-animated-sksl-shader-onto-a-gpu-or-raster-skia-canvas).

### PalmMotion: what Ripple, Holy Inanna, Crystals and Flowers read from the palms

Ethereal reads the palms through its scene; the four newer visualizers read them through
`PalmMotion`, which each WebGPU renderer advances once per frame with the visual's clock. It
steps the same `PalmAttractorField` by the frame's real delta (zero on the first frame, clamped
at a tenth of a second so a stall cannot make anything lurch) and exposes each slot as four
floats - position, strength and a smoothed speed in view sizes per second - which is exactly one
`vec4` per palm in the WGSL uniform blocks, so the renderers copy the span in one call. It also
keeps one eased `Activity` level for the whole visual: it rises quickly with open palms and
their movement, so one open palm is already lively and a second or a moving one takes it to
full, and it falls slowly when the palms go away, so the visual calms down gracefully rather
than snapping back. Read `PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmMotion.cs` and
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmMotionTests.cs`.

### Ripple - WebGPU: a wave equation in a compute shader

`RippleWebGpuRenderer` records two passes into each frame. The first is a compute pass that
advances a 2D wave equation over a 448 x 256 height field held in two storage buffers: each
leapfrog step reads one buffer as the current heights and overwrites the other, which holds the
previous heights, with the next ones, and two bind groups built once alternate the roles. Two
steps run every frame - an even number, so the newest heights always end up back in the buffer
the render pass reads - and the pool's walls reflect, because an edge cell stands in for its
missing neighbour. Inside the step each open palm rocks the water under it and presses a trough
in proportion to its speed, which is what drags a wake behind a moving hand, and the frame's
raindrops press small dimples. `RippleRain` supplies those drops: a seeded handful a second at
random places, more and deeper as the palm activity rises, at most four a frame, and never a
saved-up downpour after a stall. The second pass is one full-screen triangle that shades the
height field as water: normals from the slopes plus a faint travelling swell, the pool floor -
a pastel glass mosaic of an eight-petalled rosette, its lens-shaped petals and gold outlines
drawn from signed distances and cut into irregular tesserae by a Voronoi pattern - refracted
through the surface and lit by caustics where the waves focus the light, a Fresnel
reflection of a pale sky, sun glints, and a thin-film rainbow sheen that shimmers on the slopes.
The renderer builds its pipelines, buffers and bind groups through `WgslGpu`, a small static
class of raw-binding helpers - shader modules from WGSL, render and compute pipelines whose
layouts are derived from the shader, zero-filled buffers, bind groups that bind buffer *i* at
binding *i*, and buffer writes - and releases every one of them in `OnDeviceLost`, never the
device or the queue. The stability of the simulation is arithmetic in C#: the two axis weights
add up to half the stability limit and are split by the cells' shape in the view, so the waves
travel equally fast across and down whatever the window's shape. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/RippleWebGpuRenderer.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/RippleRain.cs` and
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/WgslGpu.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/RippleTests.cs`. See
[Run a WebGPU compute pass over ping-ponged storage buffers before the render pass](../BLUEPRINTS-GraphicsAndRendering.md#run-a-webgpu-compute-pass-over-ping-ponged-storage-buffers-before-the-render-pass).

### Holy Inanna - WebGPU: a ray-marched star the palms may only influence

`HolyInannaWebGpuRenderer` is one fragment shader over one full-screen triangle. The nebula is
layers of domain-warped noise turning very slowly about the middle of the view: deep space into
indigo, purple clouds, magenta wisps, a mauve haze and glowing orange cores, cut by dark dust
lanes, with a field of fine, gently twinkling stars shining through; each open palm swirls the
nebula round itself and glows vivid orange over it, and a moving palm stirs a wider patch,
swirls it harder and churns it with turbulence. The eight-pointed Star of Inanna
(Ishtar) is ray-marched as a solid: a signed distance to the star shape, with its inner corners
measured from the symbol, extruded into a slab whose thickness grows toward the middle of each
ray and the centre, so the rays are ridged and the centre raised. A hit is shaded as polished
gold - lit, with a highlight and a reflection of the nebula's colours - carrying the symbol's indigo line
work: a fine line inside its edge, two nested star outlines, the double ring and the centre
dot. A near miss within a pixel and a half becomes partial coverage, which antialiases the
silhouette, and a soft golden halo with eight faint rays turns with the star. The rotation is
computed on the CPU: `WriteViewToStar` writes the view-to-star matrix for the star's spin and
two tilts as three uniform rows, so the shader turns the ray into the star's space and the
distance function never moves.

Where the star is, how large and how turned is decided by `HolyInannaMotion`, and the rule it
enforces is that the palms may influence the star but never disturb it much, out of respect for
the symbol. The star drifts on a slow, wandering path of its own; the palms can draw it at
most `MaxPalmPull` (12 percent of the view) off that path, and only over seconds; their activity
brings it closer, up to `ActiveScale`, or lets it recede to `CalmScale`, just as slowly; its
spin quickens only slightly with the palms, and its tilt is a gentle sway that shows its depth.
The renderer keeps it whole inside the view. Every one of those limits is a documented constant
and the tests pin them: the star keeps to its path without palms, is never drawn far from it,
moves gently toward a palm, comes closer with activity and keeps spinning, and the matrix is a
rotation that, untilted, only spins about the line of sight. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/HolyInannaMotion.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/HolyInannaWebGpuRenderer.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/HolyInannaTests.cs`. See
[Ray-march an SDF solid in a full-screen WGSL fragment shader](../BLUEPRINTS-GraphicsAndRendering.md#ray-march-an-sdf-solid-in-a-full-screen-wgsl-fragment-shader).

### Crystals - WebGPU: an instanced, growing garden

The crystals live on the CPU, in `CrystalGarden`, and the GPU only draws them. The garden is
seeded, so a run is repeatable, and it opens with twenty young crystals so the view is never
empty. Crystals sprout in clusters - a calm trickle anywhere, and more right where the open
palms are, faster when they move - and every cluster leans only a little off straight up while
its crystals fan at most about 40 degrees either side, so every crystal grows upward. Each
cluster takes one exact colour of the shader's palette - indigo, purple, magenta, mauve or
orange, orange a little more often than the rest - and its crystals vary only slightly around
it. A crystal grows to its full length, easing out as it gets there, lives for 25 to 55
seconds, and then fades away; the garden holds at most 96, and when it is full a new sprout
starts the oldest crystal fading instead, to make room. Open palms excite the crystals within
their reach, measured round on screen whatever the window's shape: an excited crystal grows up
to four times faster and is drawn more vibrant, pulsing harder and faster and glinting more
often, and the excitement rises quickly and falls slowly, so a crystal calms down after the
hand has gone.

`CrystalsWebGpuRenderer` draws the whole thing in one render pass. A full-screen triangle
paints the backdrop - dark indigo, deepest at the edges, with a faint, slowly shifting haze
and a soft magenta-and-orange light under each open palm. Then every crystal is one instance
of a six-vertex quad: `WriteInstances` lays each crystal out as three `vec4`s in the same
uniform block both shaders read - position, direction and length; half-width, hue, vibrance
and fade; pulse phase, glint phase, seed and excitement - oldest first, so the youngest are
drawn on top, and the vertex shader places the quad along the crystal's own axis with a
margin for its glow. The fragment shader draws a quartz point from the quad's local
coordinates: a signed distance to the outline of a prism ending in a point, its three visible
prism faces and three termination faces shaded apart, facet edges catching the light, glass
that is brighter where it is thin, faint phantom outlines inside, a white glint sweeping up
it, a four-rayed sparkle at its tip now and then, and a soft glow round it. It returns
premultiplied colour, and the crystal pipeline is the one `WgslGpu.CreateRenderPipeline`
builds with `premultipliedBlend: true`, so the glass covers what is behind it while the glow
and the sparkle only add light. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/CrystalGarden.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/CrystalsWebGpuRenderer.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/CrystalsTests.cs`. See
[Draw many instanced quads over a full-screen backdrop in one WebGPU render pass](../BLUEPRINTS-GraphicsAndRendering.md#draw-many-instanced-quads-over-a-full-screen-backdrop-in-one-webgpu-render-pass).

### Flowers - WebGPU: a meadow the palms light

`FlowerMeadow` decides how the flowers live, on the CPU and seeded, so a run is repeatable.
With no palm in view it keeps `CalmFlowers`, four: one flower sprouts, grows and blooms over
`CalmBloomSeconds`, twenty seconds, and the next sprouts only once nothing is still growing.
When a fifth sprouts the oldest starts withering over `WitherSeconds`, also twenty seconds, so
it is gone just as the new one finishes blooming. A calm flower goes where there is most room:
the meadow tries thirty random spots and keeps the one farthest from every flower still alive
and well, which looks random and still spreads the flowers evenly over the view. An open palm
lights the felt in a soft oval whose reach `LightReach` decides: one palm alone lights about a
quarter of the view, and as a second palm comes into view every oval grows smoothly until two
a little apart light almost all of it. The light changes everything: flowers sprout rapidly in
it, each placed at the roomiest of a dozen spots scattered round the palm and skipped when the
spot is too crowded, so flowers may overlap a little but never pile up; they grow up to seven
times faster; nothing in the light withers, while a flower whose light falls below
`DarkThreshold` starts withering as long as palms are in view, and a withering flower lit again
recovers unless it is more than 85 percent withered. When the palms go, the light fades, every
flower but the four youngest withers slowly, and the calm cycle resumes. Each flower takes one
of six vivid colours - red, orange, yellow, blue, indigo or violet - leaning a little only
toward a neighbour in its own warm or cool half, so no flower can ever turn green, and then
rescaled to full brightness. Shapes vary too: petals run from narrow to broad, widest near the
base like a lance or near the tip like a spoon, and from 72 percent to the full head radius
long; flowers grown under the palms are smaller than calm ones. The meadow holds at most 80.

`FlowersWebGpuRenderer` has the shape of the Crystals renderer: one render pass, a full-screen
felt pipeline and then every flower as an instanced quad blended in premultiplied colour, both
reading one uniform block in which `WriteInstances` lays each flower out as six `vec4`s. The
felt is dark pool-table green with a fine grain, lit bright green under the palms - and it is
lit with the same oval `FlowerMeadow.LightAt` uses to decide which flowers stand in the light,
written out again in the WGSL. The reach is not copied into the shader: the renderer asks
`FlowerMeadow.LightReach` for it every frame and writes it into the uniform block's
`reachAcross` and `reachDown` fields, and a unit test checks that the shader declares those
fields and lights the felt with the same expression; what the user sees lit is what the meadow
treats as lit. `WriteInstances` writes the
flowers sorted by the height of their heads, highest first, so a flower lower in the view is
drawn later and stands in front. Each flower's fragment shader draws a tapered stem that grows
up first, with two leaves, then a green bud that swells and unfolds into the bloom: cupped
petals shaded in 3D with a vein and a base-to-tip gradient, a soft shadow on the felt, and a
domed centre dotted with round stamens. Light makes the colours more vivid, and withering
browns, shrinks and curls the flower until it fades away.

Every flower has exactly eight identical petals, whatever its size and shape, and the shader
is written so that cannot drift: the count is one constant, `const PETALS = 8.0;`, every petal
axis is a multiple of `sector = 2.0 * PI / PETALS` from the flower's rotation, and for each
pixel only the nearest petal and its two neighbours are evaluated. Where neighbours overlap,
the even-numbered four are always in front of the odd four, so a quarter turn maps the flower onto itself and its four-way symmetry holds even in the
overlaps. A unit test pins the constant and the sector line. Read
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/FlowerMeadow.cs`, then
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/FlowersWebGpuRenderer.cs`, then
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/FlowersTests.cs`. See
[Share one formula between a C# simulation and a WGSL shader and pin it with a test](../BLUEPRINTS-GraphicsAndRendering.md#share-one-formula-between-a-c-simulation-and-a-wgsl-shader-and-pin-it-with-a-test)
and
[Draw many instanced quads over a full-screen backdrop in one WebGPU render pass](../BLUEPRINTS-GraphicsAndRendering.md#draw-many-instanced-quads-over-a-full-screen-backdrop-in-one-webgpu-render-pass).

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
calls a set-targets method from its own thread; the Ethereal scene or `PalmMotion`, on the
renderer's frame, calls a step method once per frame with the real delta, clamped so a stall cannot make the
field lurch, and then copies the state into a caller-owned buffer whose length it validates,
which is what keeps the frame allocation-free. The easing is `1 - exp(-rate * dt)` rather than `rate * dt`, so it is
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
thread; `StopVisualizer()` reads the visualizer session into a local and nulls the field
*before* it unsubscribes from the session's `Problem` event and stops it, so a concurrent
worker-thread handler sees null and returns instead of touching a stopping session, and the
stop takes the session's view out of the page, which releases its GPU context or device; the
capture service is unsubscribed and disposed; and the base call comes last. The
unsubscription is symmetric in both directions, because the tracker and the capture service
each clear their own event before stopping in their own `Dispose()`. `StopVisualizer()` is
also the method the dropdown uses to replace a session, so disposal and switching cannot drift
apart. See
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
compile. The same project file owns every GPU reference - Graphics3DGL for "Ethereal - OpenGL",
WebGpu for the five WebGPU visualizers and SkiaSharp.Views for the canvas the OpenGL frames are
presented on - with a comment saying the Core project depends on the library rather than on
those packages, and it sets `AllowUnsafeBlocks` because the WebGPU renderers record their
commands through descriptor-struct pointers. See
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads),
[Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
and
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).

### Tests that need neither a window nor a camera

Every library test project tests a library, never the UI, and each references exactly its own
library; the PlayTests suite drives the UI with fakes for all three collaborators. That is what makes the suite runnable on a machine with no camera and no GPU: camera
enumeration legitimately returns an empty list, the shader tests evaluate real SkSL on a Skia
raster surface with no GPU and no window, the WGSL of all five WebGPU renderers is pinned as
text against the uniform layouts and bindings the renderers write, because it only compiles on
a WebGPU device, the motion, garden and meadow classes that drive Ripple, Holy Inanna,
Crystals and Flowers are stepped frame by frame with no GPU at all, and the vision tests run real inference on the CPU. The PlayTests'
fake visualizer sessions record their lifecycle, their kind and their palms and never create a
view, so no GPU context and no WebGPU device is needed to drive the dropdown - including a
theory that picks each newer visualizer and checks it gets a session of its own kind that
receives the mirrored palms. Each library carries an `InternalsVisibleTo.cs` naming its own test assembly (Camera and
Vision also name the PlayTests suite, whose fakes build their internal types), and
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
each one's license, and the capture library's terms differ from those of the platform,
graphics and vision packages, so check the suffix in each csproj before shipping a derivative.

## License

PalmVisualizer is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
