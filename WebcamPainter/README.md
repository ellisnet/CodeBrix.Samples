# WebcamPainter

WebcamPainter is a hand-gesture painting application. You pick a camera from a
dropdown, watch a mirrored live preview, and press **Take Photo** to grab an
in-memory still. The application then flips into Paint Mode: the still becomes the
background of a drawing session, a hand-tracking pipeline starts on the live camera
feed, and a crosshair ring follows your palm across the photo. Hold an open palm
toward the camera and highlighter ink spreads under the crosshair (the ring turns
green while ink is flowing); close your hand, or take it out of frame, and the brush
lifts. Seven ROYGBIV highlighter colors are selectable, each on its own translucent
layer, so overlapping passes of one color never darken where they cross. **Clear**
starts the painting over, **Save…** writes a JPEG at the photo's native resolution
through a native save dialog, and **Back** returns to the camera.

It is the reference application for running real-time webcam capture and on-device
computer vision inside an ordinary CodeBrix.Platform XAML application, and for the
"application plus extra library assemblies" project layout: three self-contained
libraries under `src/libs`, each with a mirrored test project under `tests/libs`,
composed by a single view model that the six heads share.

## What this sample shows a CodeBrix.Platform developer

- Enumerating the connected cameras and running a live capture session from a plain
  service, with no camera-library type reaching the view model: [Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session).
- Wrapping the device object so a `ComboBox` can bind straight to the collection: [Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it).
- Blitting live BGRA frames onto an `SKXamlCanvas` subclass, aspect-fit, letterboxed
  and selfie-mirrored: [Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass).
- Grabbing a still from one command and standing up a whole second pipeline behind
  it before the mode flag flips: [Capture a still and start a second pipeline from a command](../BLUEPRINTS-MVVM.md#capture-a-still-and-start-a-second-pipeline-from-a-command).
- Running inference on a worker thread that drops stale frames instead of ever
  blocking the camera: [Run a sensor pipeline on a worker thread with latest frame wins](../BLUEPRINTS-MVVM.md#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins).
- Getting results from a capture thread, through a processing worker, onto the UI
  thread with only the view model deciding what the UI sees: [Hand results from a capture thread through a worker to the UI thread](../BLUEPRINTS-MVVM.md#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread).
- Running a TFLite detector through the OpenCV DNN module and decoding its raw
  per-anchor tensors yourself: [Run a TFLite model through the OpenCV DNN module](../BLUEPRINTS-MediaAndVision.md#run-a-tflite-model-through-the-opencv-dnn-module).
- Warping a rotated region of interest into an upright square crop for a
  second-stage model, and projecting its output back: [Warp a rotated region of interest into a model input](../BLUEPRINTS-MediaAndVision.md#warp-a-rotated-region-of-interest-into-a-model-input).
- Deciding a gesture from landmark geometry when the classifier model in your bundle
  will not import: [Recognize a gesture from landmark geometry instead of a model](../BLUEPRINTS-MediaAndVision.md#recognize-a-gesture-from-landmark-geometry-instead-of-a-model).
- Smoothing a jittery sensor position at the producer, so every consumer gets the
  same steadied value: [Smooth a noisy sensor position before it drives the UI](../BLUEPRINTS-MediaAndVision.md#smooth-a-noisy-sensor-position-before-it-drives-the-ui).
- Publishing an immutable result object from a background pipeline that fires at
  frame rate: [Publish a small immutable result type from a background pipeline](../BLUEPRINTS-MVVM.md#publish-a-small-immutable-result-type-from-a-background-pipeline).
- Telling "we are shutting down" apart from "that frame was bad" when a worker calls
  into a native runtime: [Survive a native runtime tearing down while a frame is in flight](../BLUEPRINTS-MVVM.md#survive-a-native-runtime-tearing-down-while-a-frame-is-in-flight).
- Building a drawing session over a captured image with one named, translucent layer
  per ink color: [Create a drawing session with named color layers](../BLUEPRINTS-GraphicsAndRendering.md#create-a-drawing-session-with-named-color-layers).
- Driving begin/continue/end stroke calls in 0..1 image coordinates from something
  that is not a pointer: [Drive strokes in normalized image coordinates from a sensor](../BLUEPRINTS-GraphicsAndRendering.md#drive-strokes-in-normalized-image-coordinates-from-a-sensor).
- Keeping a mirrored preview, a mirrored still and a tracker that reports unmirrored
  coordinates all agreeing about which way is left: [Keep a mirrored preview and a mirrored drawing consistent](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent).
- Drawing a brush-sized cursor over the rendered drawing, sized by the drawing
  session's own view scaling: [Draw a brush sized cursor over a rendered drawing session](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-brush-sized-cursor-over-a-rendered-drawing-session).
- Exporting the finished painting at the source image's pixel size rather than the
  canvas size: [Export a drawing at a chosen pixel size](../BLUEPRINTS-GraphicsAndRendering.md#export-a-drawing-at-a-chosen-pixel-size).
- Letting the view model repaint two Skia canvases from background threads without
  ever holding a control reference: [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- Getting a save path from a native picker into a view model that references no
  windowing API: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Removing the empty placeholder file a save picker leaves behind, so your own
  overwrite prompt only fires for real content: [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Handing the view model a `XamlRoot` getter so it can raise its own dialogs: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Asking for confirmation and reporting errors from inside a command, with the
  prompt conditional rather than unconditional: [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- Switching one page between two complete UI states with a single bool, a computed
  inverse and one converter registered twice: [Switch a page between two modes with one bool and a converter](../BLUEPRINTS-ViewsAndControls.md#switch-a-page-between-two-modes-with-one-bool-and-a-converter).
- Writing `SimpleViewModel` properties, `[AffectsCommands]` and lazily created
  `SimpleCommand` pairs the family way, including a parameterized command: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Setting bound state from a background thread with `InvokeOnMainThread`: [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Disposing a view model that owns a camera, a worker thread, a native drawing
  session and delegates the page handed it: [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- Declaring a Skia page, mapping the platform namespaces, and binding with the
  platform's `Binding` markup extension: [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- Laying out an application as a shared UI project plus a Core library plus tested
  side libraries: [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Compiling `App.xaml` and the pages into every head from one shared project: [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Giving the Core library the application's root namespace so head-compiled XAML and
  library view models sit under one root: [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Keeping the managed binding in the library and fanning the per-RID native packages
  out across the heads: [Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads).
- Embedding a machine learning model with an explicit logical name and loading it by
  reflection: [Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).
- Writing a head's `Program.Main` so it contains nothing but hosting: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Ordering the `App` constructor correctly, including the service-resolver seam kept
  even with nothing to register: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Supplying the generic host builder that `SimpleServiceResolver` is created with: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Setting a bundled font as the default for all text and exposing it as a resource
  key: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Wiring console logging that only exists in Debug builds, before the host is built: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Forcing the software render surface after `Build()` on the WinWpfSkia head: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Keeping implementation types internal while still testing them directly: [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Adding, in a test project, the native assets a head would normally have supplied: [Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied).
- Reading a committed fixture image from beside the test binary: [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).

## Building, running and testing

WebcamPainter is a pure CodeBrix.Platform application. It has no native WinUI 3, WPF
or .NET MAUI heads, so there is one solution and no Windows-only companion.

| Solution | Open it on | Contains |
| --- | --- | --- |
| `WebcamPainter.slnx` | Linux, macOS, Windows | The shared UI project, `WebcamPainter.Core`, the six heads, a `Libraries` solution folder with the three side libraries, and a `Tests` solution folder with their three test projects |

### The heads

| Project | Platform |
| --- | --- |
| `src/WebcamPainter.LinuxX11` | Linux desktop, X11 |
| `src/WebcamPainter.LinuxWayland` | Linux desktop, Wayland |
| `src/WebcamPainter.LinuxFrameBuffer` | Linux framebuffer |
| `src/WebcamPainter.MacOS` | macOS |
| `src/WebcamPainter.Win32Skia` | Windows, native Win32 window |
| `src/WebcamPainter.WinWpfSkia` | Windows, Skia hosted in WPF |

Every head is a `Program.cs` and a csproj. Five of them target `net10.0`; only
`WebcamPainter.WinWpfSkia` targets `net10.0-windows`, and it sets
`EnableWindowsTargeting` so it still restores and builds on Linux and macOS even
though it runs only on Windows.

### Prerequisites

- The .NET 10 SDK. Nothing else needs installing.
- A webcam. With no camera attached the application still starts and the dropdown is
  empty; the status line reads "No cameras were found on this machine." and **Take
  Photo** stays disabled because it requires a delivered frame.
- No system OpenCV installation. The managed binding is referenced by
  `WebcamPainter.Vision`, and each head references the native packages for its own
  runtime identifiers.
- No accounts, tokens, downloads or user-supplied data. The two hand models the
  application runs are embedded into the Vision assembly at build time.

### Running one head

```text
dotnet run --project src/WebcamPainter.LinuxX11/WebcamPainter.LinuxX11.csproj
```

Substitute any other head project. Building the Windows heads on Linux or macOS is
supported; running them is not.

### Tests

There is one test project per side library, under `tests/libs/`. All three are
self-executing test binaries (`OutputType` is `Exe`, with
`UseMicrosoftTestingPlatformRunner` set), and `global.json` in this folder selects the
Microsoft.Testing.Platform runner:

```text
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

Because of that, a plain `dotnet test` can report that zero tests were discovered.
The way that always works is to build the test project and run its binary directly:

```text
dotnet build tests/libs/WebcamPainter.Vision.Tests/WebcamPainter.Vision.Tests.csproj -c Release
dotnet tests/libs/WebcamPainter.Vision.Tests/bin/Release/net10.0/WebcamPainter.Vision.Tests.dll
```

Two of the projects need real native libraries, and their csproj files arrange it:
`WebcamPainter.Painting.Tests` renders actual Skia raster surfaces and references the
SkiaSharp native assets for Linux, macOS and Windows;
`WebcamPainter.Vision.Tests` runs real inference and references the native OpenCV
package for the build OS through `$([MSBuild]::IsOSPlatform('...'))` conditions, and
copies its fixture photograph to the output folder. Neither the Vision nor the Webcam
tests need a camera: device enumeration talks straight to the OS and returns an empty
list on a machine without one.

## How the projects and folders are organized

```text
WebcamPainter/
  WebcamPainter.slnx                    The single cross-platform solution
  global.json                           Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt               Attribution for the bundled MediaPipe models
  models/                               Source .tflite files; build inputs, not shipped content
    gesture_recognizer_2026-07-13/
      hand_landmarker/                  hand_detector + hand_landmarks_detector (both embedded)
      hand_gesture_recognizer/          Classifier stages: bundled, not embedded, not used
    pose_landmarker_full_2026-07-13/    Pose models: bundled, not embedded, not used
  src/
    WebcamPainter.UI/                   Shared project (.shproj + .projitems): App.xaml(.cs),
                                          Views/MainPage.xaml(.cs)
    WebcamPainter.Core/                 The application library: ViewModels/MainViewModel.cs,
                                          Helpers/HostHelper.cs, Helpers/FileDialogHelper.cs
    WebcamPainter.LinuxX11/             Head: Program.cs and a csproj
    WebcamPainter.LinuxWayland/         Head: Program.cs and a csproj
    WebcamPainter.LinuxFrameBuffer/     Head: Program.cs and a csproj
    WebcamPainter.MacOS/                Head: Program.cs and a csproj
    WebcamPainter.Win32Skia/            Head: Program.cs and a csproj
    WebcamPainter.WinWpfSkia/           Head: Program.cs and a csproj
    libs/
      WebcamPainter.Webcam/             Capture service, camera wrapper, captured photo,
                                          CameraCanvas and the live-video renderer
      WebcamPainter.Painting/           PaintingSession, HighlighterPalette, PaintCanvas
                                          and the painting render helper
      WebcamPainter.Vision/             HandTracker, the result types, and Internal/ with
                                          PalmDetector, HandLandmarker, OpenPalmClassifier
  tests/
    libs/
      WebcamPainter.Webcam.Tests/       Capture service behavior with no camera present
      WebcamPainter.Painting.Tests/     Palette, session, stroke lifecycle, export, mirroring
      WebcamPainter.Vision.Tests/       Anchor grid, geometry, model loading, end-to-end runs;
                                          carries _data/open_palm_hands.jpg
```

Dependency direction is strictly one way and worth copying. The three `src/libs`
libraries know nothing about each other, nothing about `WebcamPainter.Core` and
nothing about the view model; each carries only the packages it needs, and none of
them references the CodeBrix.Platform application framework.
`WebcamPainter.Core` project-references all three, adds CodeBrix.Platform and the
bundled font, and contributes the view model plus two helpers. Each head
project-references only `WebcamPainter.Core`, adds exactly one CodeBrix.Platform
runtime package for its windowing system, and adds the native OpenCV packages for
its runtime identifiers. Each test project project-references exactly one library.

The XAML UI is file-linked rather than referenced: every head imports
`WebcamPainter.UI.projitems` with `Label="Shared"`, so `App.xaml.cs` and
`MainPage.xaml.cs` compile into each head assembly. That is what lets the page be a
`partial` class in the `WebcamPainter.Views` namespace while the view model it binds
to lives in a referenced library. `WebcamPainter.Core` sets `RootNamespace` to
`WebcamPainter` so those namespaces line up; because the assembly name and the root
namespace differ, XAML that names the view model has to give both
(`clr-namespace:WebcamPainter.ViewModels;assembly=WebcamPainter.Core`).

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework and the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `[AffectsCommands]`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `InvokeOnMainThread`, `ConfirmDialog`, `ShowError`, the `Binding` markup extension, `BoolToVisibilityConverter` and `CodeBrixPlatformHostBuilder` | `src/WebcamPainter.Core/`, `src/WebcamPainter.UI/` |
| CodeBrix.Platform runtime backend (one package per head) | Supplies the windowing and render backend selected by the head's single `Use...()` call | the six `src/WebcamPainter.<head>/` projects |
| CodeBrix.Platform Fonts.Roboto | Bundles Roboto, set as the application's default text font and exposed as a `FontFamily` resource key | `src/WebcamPainter.Core/`, `src/WebcamPainter.UI/App.xaml` |
| CodeBrix.Platform SkiaSharp Views | Supplies `SKXamlCanvas`, subclassed twice so the XAML can name the two canvases | `src/libs/WebcamPainter.Webcam/CameraCanvas.cs`, `src/libs/WebcamPainter.Painting/PaintCanvas.cs` |
| CodeBrix.Webcam | Camera enumeration, the live capture session with its latest-frame cache, and in-memory stills | `src/libs/WebcamPainter.Webcam/` |
| CodeBrix.Imaging.Drawing (brings CodeBrix.Imaging) | The drawing session created from raw BGRA, one layer per highlighter color, normalized-coordinate stroke input, letterbox and scaling math, clear and JPEG export | `src/libs/WebcamPainter.Painting/` |
| CodeBrix.VideoProcessing.OpenCV5 (managed binding) | Reads both TFLite models through the DNN module and does the frame preparation, letterboxing, affine warp and tensor reads | `src/libs/WebcamPainter.Vision/` |
| CodeBrix.VideoProcessing.OpenCV5 native packages (per runtime identifier) | The native OpenCV library, fanned out across the heads: both Linux identifiers on the three Linux heads, both macOS identifiers on the macOS head, both Windows identifiers on both Windows heads | the six head csproj files, and `tests/libs/WebcamPainter.Vision.Tests/` for the build OS |
| SilverAssertions | The assertion style in all three test projects | `tests/libs/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | The generic host behind the `IHostBuilderProvider` that `SimpleServiceResolver` is created with | `src/WebcamPainter.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The Debug-only console logger factory installed before the host is built | `src/WebcamPainter.UI/App.xaml.cs` |
| SkiaSharp | The surfaces, bitmaps and paints used by the two renderers, plus the native assets the painting tests need | `src/libs/WebcamPainter.Webcam/CameraCanvas.cs`, `src/libs/WebcamPainter.Painting/PaintCanvas.cs`, `tests/libs/WebcamPainter.Painting.Tests/` |
| xUnit | The test framework in all three test projects, run as self-executing binaries | `tests/libs/` |

## Worth studying in this application

### Two modes on one page, driven by one boolean

The application never navigates. `MainViewModel.IsCaptureMode` is a private-set bool
with a computed inverse, `IsPaintMode`, and every panel that belongs to one mode
binds its `Visibility` to one of them through `BoolToVisibilityConverter` registered
twice, the second time with `Invert="True"`. The same property carries
`[AffectsCommands]` naming all five commands, so flipping the mode re-evaluates the
whole toolbar in one assignment. Read `src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
(the bindable-properties region) and then `src/WebcamPainter.UI/Views/MainPage.xaml`.
The gotcha is that a computed inverse property needs an explicit
`NotifyPropertyChanged` from the setter it derives from, because `SetProperty` only
raises for its own name. See [Switch a page between two modes with one bool and a converter](../BLUEPRINTS-ViewsAndControls.md#switch-a-page-between-two-modes-with-one-bool-and-a-converter)
and [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### The capture library: enumeration, a live session, an in-memory still

`WebcamPainter.Webcam` is the smallest of the three libraries and the best place to
start. `WebcamCaptureService` exposes a static enumeration method that is safe to
call at startup with no session running and no camera present, a `Start(camera)` that
stops any previous session first, a `TryCopyLatestFrame` that forwards to the
session's own pixel cache through a caller-owned buffer, and a `FrameArrived` event
whose XML documentation states plainly that it is raised on the capture thread.
`CameraDevice` wraps the device object and returns its friendly name from
`ToString()`, which is why the XAML `ComboBox` can bind directly to the collection
with no item template. Read `WebcamCaptureService.cs` then `CameraDevice.cs`, and
note that the service does not cache pixels itself. The sharp edge is the threading
contract: every handler in the application is written to get out fast and marshal its
own UI work. See [Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session)
and [Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it).

### The vision pipeline: three stages behind one class

`WebcamPainter.Vision` re-implements MediaPipe-style hand tracking on top of the
OpenCV DNN module, and the whole thing is reachable through one public class.
`HandTracker` owns the worker thread and the two models; everything else is
`internal`. `Internal/PalmDetector.cs` regenerates the fixed anchor grid in a static
constructor (the grid is not in the model file, so it has to match exactly),
letterboxes the frame, reads the score tensor and the box tensor as two separate
named forwards so that the common no-hand case never touches the far larger box
tensor, applies a sigmoid to the score logits only, and produces a rotated region of
interest. `Internal/HandLandmarker.cs` builds a three-corner affine transform from
that rotated box, warps the hand upright into the model input, reads landmarks and a
presence value in one pass because both are always needed, and projects the landmarks
back into original frame pixels using the same rotation in reverse.
`Internal/OpenPalmClassifier.cs` then decides the gesture from geometry alone. Read
them in that order. The sharp edges are documented at their source: the presence
output is already a probability and must not be passed through a sigmoid a second
time, the letterbox padding has to be undone to get back to frame pixels, and the
regressor tensor's stride is fixed by the model. See [Run a TFLite model through the OpenCV DNN module](../BLUEPRINTS-MediaAndVision.md#run-a-tflite-model-through-the-opencv-dnn-module)
and [Warp a rotated region of interest into a model input](../BLUEPRINTS-MediaAndVision.md#warp-a-rotated-region-of-interest-into-a-model-input).

The third stage is where this pipeline departs from the bundle its models came from.
That bundle also contains gesture-classifier stages, and they are deliberately not
embedded: the csproj comment records that the TFLite importer cannot load them
because of an unsupported `GATHER` operator. Instead
`OpenPalmClassifier` calls a finger extended when its tip is farther from the wrist
than its middle joint by a tuning ratio, requires that of four fingers (the thumb is
excluded because its geometry does not follow the same rule), and averages the wrist
with four knuckles for the palm center. It is a pure static class over an array of
points: no net, no state, no allocation, and therefore trivially unit tested with
synthetic hands, which is exactly what `tests/libs/WebcamPainter.Vision.Tests/OpenPalmClassifierTests.cs`
does. The rule is scale- and rotation-free because it compares two distances from the
same wrist point. See [Recognize a gesture from landmark geometry instead of a model](../BLUEPRINTS-MediaAndVision.md#recognize-a-gesture-from-landmark-geometry-instead-of-a-model).

### Latest-frame-wins, and the shutdown race

A camera produces frames faster than two models can consume them, so `HandTracker`
never lets the producer wait. `SubmitFrame()` copies the pixels under a lock,
silently replacing any frame that has not been processed yet, and signals the worker;
the worker swaps the pending and working buffers under the same lock, so steady state
costs one copy per processed frame and no allocations. The models are loaded inside
the worker rather than in the constructor, so constructing a tracker is cheap and the
loading cost lands on the background thread. `Start()` and `Stop()` are idempotent,
and there is a test for that. The other half of this class worth copying is its two
catch clauses: an exception filter on the shutdown flag exits the loop quietly when a
frame was in flight while the native runtime was tearing down at process exit, and a
general catch drops one bad frame and keeps tracking. The flag is `volatile`
precisely so the filter observes it the instant `Stop()` clears it. See [Run a sensor pipeline on a worker thread with latest frame wins](../BLUEPRINTS-MVVM.md#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins),
[Survive a native runtime tearing down while a frame is in flight](../BLUEPRINTS-MVVM.md#survive-a-native-runtime-tearing-down-while-a-frame-is-in-flight)
and [Smooth a noisy sensor position before it drives the UI](../BLUEPRINTS-MediaAndVision.md#smooth-a-noisy-sensor-position-before-it-drives-the-ui).

### Three threads, one view model

This is the clearest thing in the application to copy. Frames arrive on the capture
thread, inference runs on the tracker thread, and every decision that changes what
the user sees happens on the UI thread. `MainViewModel.OnFrameArrived` is the
capture-thread handler: it does almost nothing, forwarding pixels to the tracker in
Paint Mode and asking for a repaint in Capture Mode.
`MainViewModel.OnTrackingUpdated` is the worker-thread handler, and its entire body
is inside `InvokeOnMainThread`, so the crosshair position, the brush flag, the stroke
calls and the canvas invalidate all happen on the UI thread. Read those two methods
first, then the results they consume in `src/libs/WebcamPainter.Vision/HandTrackingResult.cs`,
whose XML documentation carries the coordinate contract. Two habits in these handlers
are worth adopting: each copies a field into a local before testing it, because
another thread can null it between the test and the use, and the mode check is
repeated inside the marshalled callback, because by the time it runs the user may
already have pressed **Back**. See [Hand results from a capture thread through a worker to the UI thread](../BLUEPRINTS-MVVM.md#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and [Publish a small immutable result type from a background pipeline](../BLUEPRINTS-MVVM.md#publish-a-small-immutable-result-type-from-a-background-pipeline).

### One translucent layer per ink color

`PaintingSession` wraps the drawing library and exposes only what the application
needs. Its factory builds the session from the still's raw BGRA pixels, mirroring
inside the library rather than through an encode and decode round trip, then adds one
named layer per entry in `HighlighterPalette`. The layer name is the color name,
which is why the seven color buttons can switch ink with a plain string command
parameter and no enum plumbing. One layer per color is what makes the highlighter
effect work: repeated passes of a single color over the same area do not compound
where they cross. Read `src/libs/WebcamPainter.Painting/HighlighterPalette.cs` then
`PaintingSession.cs`. The palette carries a comment asking you to keep the hard-coded
button backgrounds in `MainPage.xaml` in sync with it, which is the one duplication
in the application; a developer copying this would do better to expose the palette
from the view model and template the buttons. The background fill is opaque white
because JPEG has no alpha, and the surface clear color is the letterbox color around
the still. See [Create a drawing session with named color layers](../BLUEPRINTS-GraphicsAndRendering.md#create-a-drawing-session-with-named-color-layers)
and [Export a drawing at a chosen pixel size](../BLUEPRINTS-GraphicsAndRendering.md#export-a-drawing-at-a-chosen-pixel-size).

### Strokes in normalized coordinates, and mirroring in three places

The brush is a hand, not a pointer, and the input path is deliberately free of view
size, DPI and letterbox math. `PaintingSession` exposes begin, continue, end and
cancel in 0..1 image coordinates, and the view model calls them straight from the
tracker's normalized palm position: an open palm with a known position continues the
active stroke or begins a new one, and anything else ends it. Because the drawing
space is calibrated from the background image rather than from a view size, this
works before the first render, and the painting tests exercise exactly that. It is
also why export produces the photo's native resolution regardless of how large the
canvas happens to be on screen. See [Drive strokes in normalized image coordinates from a sensor](../BLUEPRINTS-GraphicsAndRendering.md#drive-strokes-in-normalized-image-coordinates-from-a-sensor).

What that input path does have to get right is handedness. The preview is a selfie
view, so it is mirrored, and everything downstream has to agree about which way is
left. The live renderer mirrors at draw time with a canvas
transform around the destination rectangle's horizontal center, inside a save and
restore, rather than flipping pixels. The still is mirrored at capture time by the
painting session's factory. The tracker reports the palm position across the
unmirrored camera frame, so the view model flips the X coordinate, and only X, before
it drives a stroke. Each of the three is one line, each documented where it happens,
and the mirroring test uses an asymmetric fixture so the flip is actually observable
in the exported image. See [Keep a mirrored preview and a mirrored drawing consistent](../BLUEPRINTS-GraphicsAndRendering.md#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent).

### Two canvases, two renderers, and the invalidate bridge

The main viewer is a `PaintCanvas`; it shows the mirrored live preview in Capture
Mode and the painting plus the crosshair in Paint Mode. The small self-view beside
the color buttons in Paint Mode is a `CameraCanvas`. Both are empty `SKXamlCanvas`
subclasses that exist purely so the XAML can name the element; the drawing lives in
`WebcamFrameRenderer` and in the painting render helper, both declared alongside
their canvas class. The crosshair helper takes the session plus three primitive
values and draws a ring sized by the drawing session's own view scaling, with a dark
halo underneath so it stays readable over bright and dark photo content. The view
model owns none of this: it calls two `Action` delegates that the page assigns
through `ICanvasBridge` on `DataContextChanged`, and the page's delegates are the
ones that marshal onto the UI thread. Read `src/WebcamPainter.UI/Views/MainPage.xaml`,
then `MainPage.xaml.cs`, then the two renderer classes. Three sharp edges: create one
renderer per canvas, because each caches its own pixel buffer and bitmap; invalidate
on `SizeChanged` or the frame keeps its old letterbox after a resize; and subscribe
`DataContextChanged` before `InitializeComponent()`, because that call may be what
sets the data context. See [Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass),
[Draw a brush sized cursor over a rendered drawing session](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-brush-sized-cursor-over-a-rendered-drawing-session)
and [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).

### Saving, confirming, and degrading gracefully

`IFileSaveBridge` is a single delegate property that the view model both declares and
implements; the page assigns the shared picker method to it in the same
`DataContextChanged` handler that supplies the `XamlRoot` getter. `DoSave()` awaits
the delegate when it is present, treats an empty result as a cancel, and falls back
to a default path when the delegate is null, which is the pattern to copy for a head
that genuinely cannot supply a picker. In this application the page is shared by all
six heads and assigns the delegate unconditionally, so the fallback branch is the
documented seam rather than an observed behavior; where a head's picker throws, the
command's `NotSupportedException` catch reports that file dialogs are not supported
on that head. Two further details are worth lifting. The picker creates an empty
placeholder file for a brand-new name, so `FileDialogHelper.RemoveEmptyPlaceholder()`
deletes it only when it is genuinely zero length, which leaves the application's own
replace-existing-file prompt to fire for real content and only for real content. And
the busy flag is set after the dialog closes, not before, so the UI is not disabled
while a modal picker is open. Confirmation elsewhere is deliberately conditional
rather than blanket: **Back** asks only when a painting would be lost, **Clear** asks
only when the stroke count is above two, and a successful save offers to clear the
painting so the next one starts fresh. See [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
and [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).

### Tearing down a camera, a worker thread and a native drawing session

`MainViewModel` owns more disposable state than most view models, and its `Dispose()`
override is the model for how to unwind it: dispose every command, null every bridge
delegate (which is what releases the page and breaks the reference cycle),
unsubscribe from each event source before disposing it, and call the base last.
`LeavePaintMode()` does the Paint Mode half of the same work when the user presses
**Back**. The ordering habit to copy appears in both: the field is nulled before the
object is disposed, so a tracking callback arriving mid-teardown finds null rather
than a disposed object. Both library classes cooperate by nulling their own event
before stopping, which guarantees no handler runs during teardown. Note that this
careful implementation is not currently invoked by anything, because the page
declares its view model inline in XAML; an application copying this shape should
resolve or own the view model in the page and dispose it on unload. See [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### An application that ships three tested libraries

The layout is the point of this part. Each capability is a library under `src/libs`
with a mirrored test project under `tests/libs`, listed in named `Libraries` and
`Tests` solution folders while the heads and Core sit at the solution root. The
libraries expose plain models and services; all composition happens in the view
model, and no library references another. Each keeps its implementation types
internal, under an `Internal/` folder where there are several, and ships a one-line
`InternalsVisibleTo.cs` naming its own test assembly. That file is present in all
three, even where the tests only touch public members, because the convention is
applied uniformly. `WebcamPainter.Vision` additionally embeds its two model files
with explicit logical names, which is necessary because the source files live outside
the project directory and the default resource name would otherwise be unpredictable;
the loader throws a clear exception when a name does not resolve. See [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project),
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
[Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
[Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection)
and [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).

The test projects that mirror those libraries are worth reading as examples of
testing code that binds to native libraries. `WebcamPainter.Vision.Tests` runs
genuine inference: it asserts the regenerated anchor grid against the layout the
models expect, checks the geometric open-palm rules with synthetic hands, verifies
embedded-model loading including the failure message for an unknown resource name,
and then runs the full pipeline, and separately the whole tracker, against a
committed photograph read from beside the test binary. Because inference time is
unknown, the end-to-end tests wait on a reset event with a generous timeout rather
than sleeping. `WebcamPainter.Painting.Tests` renders real Skia raster surfaces to
check the stroke lifecycle, layer switching, the letterbox math, brush-radius
scaling, mirroring and export. `WebcamPainter.Webcam.Tests` covers the capture
service on a machine with no camera at all. One pitfall to note if you copy the
Vision test project: the operating system conditions pull in only the host
architecture's native package, so a Linux arm64 build machine would need that
reference added. See [Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied)
and [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).

### Per-head differences you should not copy blindly

The heads look interchangeable and are not. Each contains only logging
initialization, a host built with one `Use...()` call and a factory for the shared
`App`, and a run; `[STAThread]` is on `Main` in all six, including the Linux ones.
But the LinuxX11 head is the only one that adds `UseDirectSkiaCanvasMode()`, and the
WinWpfSkia head casts the built host and forces the software render surface before
running. Its csproj is also the only one on a Windows-specific target framework.
Every head csproj carries the same comment stating the rule that keeps this
manageable: exactly one platform head package, with all other packages coming from
`WebcamPainter.Core`. The native OpenCV packages are the deliberate exception, and
they are fanned out per platform, both architectures at a time, so a head builds for
either without editing; putting them in the Vision library instead would drag one
platform's binaries into every head. Read one Linux head, then
`src/WebcamPainter.WinWpfSkia/Program.cs`, then any head csproj. See [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
and [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the
third-party content bundled with the application. Third-party code arrives as NuGet
packages, each carrying its own
license and notices, so those are not reproduced there; what the file does cover is
the bundled machine learning content: every `.tflite` file under `models/` comes from
Google's MediaPipe, is copyright Google LLC, and is licensed under Apache-2.0. That
includes the pose-landmarker models, which are bundled but neither embedded nor used,
and the gesture-classifier stages, which are bundled but cannot be imported. One
further attribution lives in a csproj comment rather than in the notices file: the
photograph `tests/libs/WebcamPainter.Vision.Tests/_data/open_palm_hands.jpg` used as a
test fixture is a public-domain image by Evan-Amos from Wikimedia Commons. Nothing is
downloaded at run time.

## License

WebcamPainter is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
