# WebcamViewer

WebcamViewer is a one-page live webcam viewer. A dropdown along the top lists the
cameras attached to the machine, a "Monitor audio" checkbox beside it plays the
camera's paired microphone through the speakers, and the rest of the window is a
black-letterboxed Skia surface showing the live picture, aspect-fit. Along the
bottom are a folder path box, a **Browse…** button that opens the platform folder
picker, and a **Photo** button that writes the frame currently on screen to that
folder as a PNG. A status line under everything says what just happened: how many
cameras were found, which one is live, where a photo landed, or why something did
not work. Choosing a different camera in the dropdown tears the old capture
session down and starts a new one.

It is this repository's reference for showing live camera frames in an ordinary
CodeBrix.Platform XAML page: enumerating devices, running one capture session at a
time, moving BGRA pixels from a capture thread onto an `SKXamlCanvas` without
copying more than necessary, and handing a single still frame to an image encoder.
It is also a compact example of the two bridge interfaces a view model needs when
only the page can do something - repainting a canvas and opening a native folder
dialog - and it ships with no libraries and no tests, so the whole application is a
shared UI project, one Core library and six heads.

## What this sample shows a CodeBrix.Platform developer

- How to list the connected cameras and start a live session from the view model,
  with the selection itself driving the switch:
  [Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session).
- How to put a device object in a tiny wrapper so a `ComboBox` can bind to the
  collection with no item template:
  [Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it).
- How the live picture reaches the screen: an `SKXamlCanvas` subclass the XAML can
  name, and a renderer that blits the newest frame aspect-fit onto it:
  [Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass).
- How the view model asks for a repaint without ever holding a control reference:
  [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- How frames arriving on a capture thread turn into bound state safely:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- How a command asks a person for a location through a dialog only the page can
  show, and explains itself when no dialog is available - the same bridge shape as
  a file picker:
  [Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model).
- The family's property and command idiom, including `[AffectsCommands]` keeping a
  button's enablement current as three separate pieces of state change:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- How a view model starts async work from its constructor without blocking the
  window, and why the constructor needs a designer guard first:
  [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor),
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How every failure path - no camera, a camera that will not open, a folder dialog
  that fails, an encode that throws - becomes a line of status text instead of an
  exception or a dialog:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- How a view model that owns a native session releases it, along with its commands
  and its bridge delegates:
  [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- A page bound entirely with the platform `Binding` markup extension, including a
  two-way `CheckBox`:
  [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension),
  [Bind a page level CheckBox two way](../BLUEPRINTS-ViewsAndControls.md#bind-a-page-level-checkbox-two-way).
- The thin code-behind that hands the view model a `XamlRoot` getter so dialogs
  could be opened from it:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- What a head's `Program.Main` contains, and the four things the `App` constructor
  does in the order they have to happen:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Where every package reference belongs, and how the XAML is compiled into six
  executables from one folder:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- The bundled font becoming the application default and a page-level resource, and
  console logging that exists only in Debug builds:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks),
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- The one per-head behavioral difference in the whole application, applied between
  `Build()` and `Run()`:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- How one buffer under one lock keeps the newest frame available to a paint
  handler without a queue, a worker or a dropped-frame count:
  [Cache the newest frame in the view model and let the renderer pull it](../BLUEPRINTS-MVVM.md#cache-the-newest-frame-in-the-view-model-and-let-the-renderer-pull-it).
- How a view model that holds the capture session itself makes one property setter
  the whole lifecycle - teardown, repaint and start:
  [Own one capture session in the view model and switch it from the selection setter](../BLUEPRINTS-MediaAndVision.md#own-one-capture-session-in-the-view-model-and-switch-it-from-the-selection-setter).
- How a still's raw pixels become a file with one wrap and one save, and what that
  means for who may touch the buffer:
  [Encode a device's raw BGRA still to PNG with the CodeBrix Imaging library](../BLUEPRINTS-MediaAndVision.md#encode-a-devices-raw-bgra-still-to-png-with-the-codebrix-imaging-library).
- How a checkbox reaches inside a running native session, stays grayed on a device
  that cannot honor it, and survives a device change:
  [Push a bound toggle into a live native session and re-apply it to the next one](../BLUEPRINTS-MVVM.md#push-a-bound-toggle-into-a-live-native-session-and-re-apply-it-to-the-next-one).
- How a path the user types is validated by the same predicate that gates the
  button, with no converter and no validation framework:
  [Validate a typed folder path inside CanExecute](../BLUEPRINTS-MVVM.md#validate-a-typed-folder-path-inside-canexecute).
- How a feature keeps working on a head with no native folder dialog, instead of
  only explaining itself:
  [Offer a typed path where a head has no folder dialog](../BLUEPRINTS-PlatformServices.md#offer-a-typed-path-where-a-head-has-no-folder-dialog).
- How a native runtime that no project file mentions is recorded, and where its
  absence surfaces at run time:
  [Depend on a native runtime the user installs instead of shipping a package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#depend-on-a-native-runtime-the-user-installs-instead-of-shipping-a-package).

## Building, running and testing

There is one solution, `WebcamViewer.slnx`, and it opens on Linux, macOS and
Windows: every project in it builds with the plain .NET SDK, with no workload to
install. There is no second solution.

| Solution | Open it on | Contains |
| --- | --- | --- |
| `WebcamViewer.slnx` | Linux, macOS, Windows | The shared UI project, `WebcamViewer.Core`, and the six heads. No `Libraries` or `Tests` solution folder, because the application has neither |

### The heads

| Project | Platform |
| --- | --- |
| `src/WebcamViewer.LinuxX11` | Linux desktop, X11 |
| `src/WebcamViewer.LinuxWayland` | Linux desktop, Wayland |
| `src/WebcamViewer.LinuxFrameBuffer` | Linux framebuffer, no display server |
| `src/WebcamViewer.MacOS` | macOS |
| `src/WebcamViewer.Win32Skia` | Windows, native Win32 window |
| `src/WebcamViewer.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Each head is a `Program.cs` and a csproj. Five target `net10.0`; only
`WebcamViewer.WinWpfSkia` targets `net10.0-windows`, and it sets
`EnableWindowsTargeting` so the solution still restores and builds on Linux and
macOS even though that head runs only on Windows.

### Prerequisites

- The .NET 10 SDK. All CodeBrix code arrives from NuGet; no CodeBrix library is
  referenced as a source project, so this folder builds on its own.
- A camera. With none attached the application still starts, the dropdown is empty,
  the status line says so, and **Photo** stays disabled because it needs a
  delivered frame.
- A native media runtime on Linux and macOS, because that is how the webcam library
  opens a capture session there. No head declares a native package for it:
  - **Linux** - install it with the system package manager
    (`sudo apt install libvlc5 vlc-plugin-base` on Debian and derivatives). The
    desktop VLC application and the development headers are not needed.
  - **macOS** - install the VLC media player application from
    [videolan.org/vlc](https://www.videolan.org/vlc/) into `/Applications`; the
    library's loader finds it and its plugins automatically.
  - **Windows** - nothing to install. Capture goes through the operating system's
    own media engine.
  Camera *enumeration* works everywhere without that runtime, so a machine missing
  it fills the dropdown and then reports the failure in the status line when a
  session is started.
- Camera permission on macOS. The first capture raises the system consent prompt,
  which attaches to the terminal application when the head is run with
  `dotnet run`. A head packaged as a proper `.app` bundle must declare
  `NSCameraUsageDescription` (and `NSMicrophoneUsageDescription` for audio
  monitoring) in its `Info.plist`, or macOS refuses access. A consent that was
  denied once is re-enabled under System Settings > Privacy & Security > Camera.
- No accounts, tokens, downloads or data files. Photos are written where the user
  points the folder box.

### Running one head

```text
dotnet run --project src/WebcamViewer.LinuxX11
dotnet run --project src/WebcamViewer.LinuxWayland
dotnet run --project src/WebcamViewer.LinuxFrameBuffer
dotnet run --project src/WebcamViewer.MacOS
dotnet run --project src/WebcamViewer.Win32Skia
dotnet run --project src/WebcamViewer.WinWpfSkia
```

Building the Windows heads on Linux or macOS is supported; running them is not.
Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` sits inside `#if DEBUG` - so a Release run is silent.

### Tests

There are none. This application has no `tests/` folder, no test project and no
`global.json`, so there is no test-runner selection to be aware of here and nothing
in this folder demonstrates the family's test conventions. Where an application in
this repository does ship tests, its test assemblies are self-executing binaries,
a plain `dotnet test` can report that zero tests ran, and the form that always
works is to build the test project and run the executable it produces:

```text
dotnet build tests/libs/<Project>.Tests/<Project>.Tests.csproj -c Release
./tests/libs/<Project>.Tests/bin/Release/net10.0/<Project>.Tests
```

## How the projects and folders are organized

```text
WebcamViewer/
  WebcamViewer.slnx                   The one solution; every project; opens on Linux, macOS and Windows
  THIRD-PARTY-NOTICES.txt             Third-party content used by this application
  src/
    WebcamViewer.UI/                  Shared items project: the XAML every head compiles
      WebcamViewer.UI.shproj          Shared-project shell, so an IDE can load the folder as a project
      WebcamViewer.UI.projitems       The shared file list each head imports with Label="Shared"
      App.xaml                        Merged WinUI resources and the Open Sans FontFamily resource
      App.xaml.cs                     Bootstrap: default font, service resolver, design mode, window and frame, logging
      Views/MainPage.xaml             The whole UI: camera dropdown, audio checkbox, video canvas, folder row, status line
      Views/MainPage.xaml.cs          Thin code-behind: the two bridges, the XamlRoot getter, the canvas paint handler
    WebcamViewer.Core/                Class library; carries every non-head package
      WebcamViewer.Core.csproj        RootNamespace WebcamViewer; framework, canvas, webcam, imaging, font, hosting, logging
      Helpers/HostHelper.cs           The IHostBuilderProvider that SimpleServiceResolver builds its container from
      Video/VideoCanvas.cs            The SKXamlCanvas subclass the XAML names, and the frame renderer beside it
      ViewModels/MainViewModel.cs     The only view model: devices, session, latest frame, folder, commands, status
    WebcamViewer.LinuxX11/            Head: Program.cs plus a csproj with one runtime package
    WebcamViewer.LinuxWayland/        Head: Program.cs plus a csproj with one runtime package
    WebcamViewer.LinuxFrameBuffer/    Head: Program.cs plus a csproj with one runtime package
    WebcamViewer.MacOS/               Head: Program.cs plus a csproj with one runtime package
    WebcamViewer.Win32Skia/           Head: Program.cs plus a csproj with one runtime package
    WebcamViewer.WinWpfSkia/          Same, plus net10.0-windows and a software render surface
```

The dependency direction is one way. Each head takes a project reference on
`WebcamViewer.Core` and file-links the shared UI by importing
`..\WebcamViewer.UI\WebcamViewer.UI.projitems` with `Label="Shared"`.
`WebcamViewer.Core` references nothing else in the application - it carries only
package references, so everything the heads share arrives through it. The shared UI
project is never compiled on its own: `App.xaml`, `App.xaml.cs`,
`Views/MainPage.xaml` and `Views/MainPage.xaml.cs` are compiled once into each of
the six head assemblies, which is why every head csproj also tells MSBuild to treat
`.xaml` files as `Page` items. Because the XAML ends up inside the head assembly
while the view models and the canvas live in the library, the page reaches both
with assembly-qualified `clr-namespace` declarations
(`xmlns:vm="clr-namespace:WebcamViewer.ViewModels;assembly=WebcamViewer.Core"` and
`xmlns:video="clr-namespace:WebcamViewer.Video;assembly=WebcamViewer.Core"`), which
line up because `WebcamViewer.Core` sets its `RootNamespace` to `WebcamViewer`
while its assembly name stays `WebcamViewer.Core`.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework itself - `Application`, `Window`, `Frame`, `Page` and every control on the page - plus the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `[AffectsCommands]`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `InvokeOnMainThread`, the `Binding` markup extension and `CodeBrixPlatformHostBuilder` | `src/WebcamViewer.Core/`, `src/WebcamViewer.UI/` |
| CodeBrix.Platform runtime backend (one package per head) | Supplies the windowing and render backend named by the head's single `Use...()` call | the six `src/WebcamViewer.<head>/` projects |
| CodeBrix.Platform SkiaSharp Views | Supplies `SKXamlCanvas`, subclassed once so the XAML has a control name to declare for the video surface | `src/WebcamViewer.Core/Video/VideoCanvas.cs` |
| CodeBrix.Platform Fonts.OpenSans | Ships Open Sans, set as the application-wide default text font and exposed as a `FontFamily` resource key addressed through an `ms-appx:///` URI | `src/WebcamViewer.Core/`, `src/WebcamViewer.UI/App.xaml`, `src/WebcamViewer.UI/App.xaml.cs`, `src/WebcamViewer.UI/Views/MainPage.xaml` |
| CodeBrix.Webcam | Device enumeration, the live capture session and its BGRA frame event, optional audio monitoring, and the in-memory still the Photo command encodes | `src/WebcamViewer.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Imaging | Wraps the still's raw BGRA pixels in an image and writes the PNG file | `src/WebcamViewer.Core/ViewModels/MainViewModel.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the dependency-injection container | `src/WebcamViewer.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider wired into the platform's ambient logger in Debug builds | `src/WebcamViewer.UI/App.xaml.cs` |
| SkiaSharp | The surface, bitmap and sampling used by the frame renderer; it arrives with the Skia canvas package rather than being referenced directly | `src/WebcamViewer.Core/Video/VideoCanvas.cs` |

## Worth studying in this application

### Discovery in the constructor, switching in a property setter

`MainViewModel`'s constructor does almost nothing: it returns immediately in design
mode, writes a debug line, sets the status text to "Discovering cameras…" and
starts `InitializeAsync()` without awaiting it. That method awaits the device list,
then does every piece of bound-state work inside one `InvokeOnMainThread` callback
- clearing and refilling `Cameras`, setting the status text, and selecting the
first camera. Selecting it is what starts the video: `SelectedCamera`'s setter
calls `SetProperty` and then `SwitchCamera(value)`, so the dropdown and the
auto-start on launch go through exactly the same path and there is no separate
start command.

`SwitchCamera` is also the teardown path. It clears `HasFrame`, unsubscribes from
and disposes the previous session, drops the cached frame under the frame lock,
invalidates the canvas so the stale picture is painted over with black, and only
then constructs and starts the new `WebcamSession`. A null camera means "stop and
stay stopped". Read `src/WebcamViewer.Core/ViewModels/MainViewModel.cs` from the
constructor down to `SwitchCamera`.

Sharp edges met here. The whole method runs on the UI thread from a property
setter, and starting a capture session is synchronous, so a slow camera is a
visible hitch; if that matters in your application, move the start onto a worker
and report progress. Discovery happens once, at startup - nothing watches for a
camera being plugged in later. And `CameraOption` exists only so the `ComboBox` has
something with a sensible `ToString()`: it wraps the device and overrides
`ToString()` to return `FriendlyName`, which is why the dropdown needs no
`ItemTemplate` and no display-member plumbing. See
[Enumerate cameras and start a live capture session](../BLUEPRINTS-MediaAndVision.md#enumerate-cameras-and-start-a-live-capture-session),
[Wrap a device library type so the view model never sees it](../BLUEPRINTS-MediaAndVision.md#wrap-a-device-library-type-so-the-view-model-never-sees-it),
[Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and
[Own one capture session in the view model and switch it from the selection setter](../BLUEPRINTS-MediaAndVision.md#own-one-capture-session-in-the-view-model-and-switch-it-from-the-selection-setter).

### Latest frame wins: one buffer, one lock, no queue

Frames arrive on the capture library's own thread. `OnFrameReceived` does the least
possible work there: under `_frameLock` it allocates the pixel buffer only when the
required size changes, copies the frame into it, records the width and height, and
leaves. Nothing is queued, so a slow UI drops frames instead of building a backlog
- the buffer simply holds whichever frame arrived most recently. Outside the lock
it raises `HasFrame` once, through `InvokeOnMainThread` because that property is
bound, and then invokes the invalidate delegate.

The read side is `TryGetLatestFrame(ref byte[] buffer, out int width, out int
height)` - the single member of the `IVideoFrameSource` interface, and the whole
surface the renderer is given - which the renderer calls on the UI thread. It
takes the same lock, grows the caller's buffer when needed, copies the pixels out
and reports the dimensions, returning false when no frame has arrived yet. Two
copies happen per displayed frame, and that is the deliberate trade: the lock is
held only for the length of an `Array.Copy`, and neither thread ever touches the
other's buffer.

Sharp edges met here. `HasFrame` is raised only on the transition, not per frame,
because it is a bound property and re-raising it sixty times a second would churn
the command that depends on it. The frame lock guards three fields together -
buffer, width and height - so a reader can never pair new pixels with stale
dimensions. And the pixel buffer is reallocated on a size change rather than sized
once, because a camera can change resolution mid-session. See
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread),
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way)
and
[Cache the newest frame in the view model and let the renderer pull it](../BLUEPRINTS-MVVM.md#cache-the-newest-frame-in-the-view-model-and-let-the-renderer-pull-it).

### The canvas, the renderer, and the invalidate bridge

`src/WebcamViewer.Core/Video/VideoCanvas.cs` holds two types and no behavior worth
hiding. `VideoCanvas` is an empty `SKXamlCanvas` subclass; it exists so the page can
write `<video:VideoCanvas x:Name="VideoView" />` and so the canvas type lives in the
library beside the code that paints it. `VideoCanvasHelper.RenderFrame` is the
painter: it clears to black, asks an `IVideoFrameSource` for the newest frame,
keeps an `SKBitmap` in BGRA8888/Opaque sized to the frame, copies the pixels into
it with `Marshal.Copy`, then computes an aspect-fit destination rectangle and
draws with linear sampling. Letterboxing is nothing more than the smaller of the
two scale factors and a centering offset.

The frame source is a parameter rather than a field, and it is typed as the
`IVideoFrameSource` interface the view model implements, so neither the canvas nor
the renderer names a view model type. The page's paint handler is one line that
forwards the surface and `DataContext as IVideoFrameSource`, which is the same
"reach the view model through the interface, never the concrete type" rule the two
bridges follow in the other direction.

The repaint itself comes from the view model, which never sees a control. It
implements `ICanvasInvalidator`, a one-property interface holding an `Action`; the
page assigns a closure over its own canvas when the `DataContext` arrives, and that
closure is what marshals onto the UI thread
(`DispatcherQueue?.TryEnqueue(() => VideoView?.Invalidate())`). The view model just
calls `InvalidateCanvas?.Invoke()` from whatever thread it is on, and the `?.`
doubles as the graceful path when no page has wired one. The page also repaints on
`SizeChanged`, because the letterbox arithmetic depends on the surface size.

Sharp edges met here. The renderer's cached buffer and bitmap are `static`, which
is fine for an application with exactly one video surface and wrong the moment
there are two - give the renderer instance fields, as an application with more than
one canvas must. The bitmap is recreated only when the frame dimensions change, so
steady-state painting allocates nothing. And `Marshal.Copy` writes exactly
`width * height * 4` bytes into the bitmap's pixel block, which assumes the frames
are tightly packed BGRA with no row padding - check that assumption before reusing
this against a different capture source. See
[Show live video on an SKXamlCanvas subclass](../BLUEPRINTS-ViewsAndControls.md#show-live-video-on-an-skxamlcanvas-subclass),
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
and
[Send the paint call back to the view model through the canvas bridge](../BLUEPRINTS-PlatformServices.md#send-the-paint-call-back-to-the-view-model-through-the-canvas-bridge).

### A folder the page chooses and the view model validates

`IFolderPickBridge` is the second bridge and has the same shape as the first: one
property, `Func<Task<string>> PickFolderPathAsync`, which the page fills in from
`DataContextChanged` with a static method that opens a `FolderPicker` seeded at the
pictures library. `BrowseFolderCommand` checks the delegate for null before using
it and, when it is null, says so in the status line rather than failing - so a head
with no folder dialog still runs and still explains itself, and the user can type a
path into the box instead.

Validation stays in the view model and is deliberately blunt: `IsValidFolder` is a
trimmed non-empty string plus `Directory.Exists`, evaluated inside the Photo
command's `CanExecute`. `FolderPath` carries `[AffectsCommands(nameof(PhotoCommand))]`,
so the button re-evaluates whenever the property changes - which, because the box
is bound two-way without `UpdateSourceTrigger=PropertyChanged`, is when the box
loses focus rather than on every keystroke.

Sharp edges met here. The folder is never remembered between runs - this
application has no settings store, and adding one is the natural next step. The
existence check happens when the command is evaluated, not when the photo is
written, so a folder deleted in between still produces a caught failure and a
status line. And the picker call is awaited in the code-behind, which is why
`MainPage.xaml.cs` carries a `using System;` with a comment saying so: the awaiter
extension for the picker's return type lives there. See
[Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model),
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way),
[Validate a typed folder path inside CanExecute](../BLUEPRINTS-MVVM.md#validate-a-typed-folder-path-inside-canexecute)
and
[Offer a typed path where a head has no folder dialog](../BLUEPRINTS-PlatformServices.md#offer-a-typed-path-where-a-head-has-no-folder-dialog).

### One button, three gates, and a PNG

`PhotoCommand` is a lazily created `SimpleCommand` built from a `CanTakePhoto()`
predicate and a `DoTakePhoto()` async action. The predicate is the interesting
part: not busy, a frame has arrived, and the folder is valid. Those three
conditions live in three different properties, and each one carries
`[AffectsCommands(nameof(PhotoCommand))]`, which is how the button enables and
disables itself without a single line of code in the page.

The action asks the session for a still - a separate capture, not the preview
buffer - and hands its raw BGRA pixels straight to the imaging library inside a
`Task.Run`, so the encode and the file write happen off the UI thread. The file
name is a timestamp to the millisecond, the outcome is a status line either way,
and `IsBusy` is cleared in a `finally` so a failed encode cannot leave the button
dead.

Sharp edges met here. The still is captured on the UI thread and only the encode is
moved off it; if your camera's still capture is slow, move that too. The pixels go
into the image wrapper without a copy, so nothing may touch that buffer until the
encode returns - which is exactly why the work is awaited rather than fired and
forgotten. And `IsBusy` gates both commands, so the Browse button is disabled while
a photo is being written. See
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way),
[Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag),
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing),
[Validate a typed folder path inside CanExecute](../BLUEPRINTS-MVVM.md#validate-a-typed-folder-path-inside-canexecute)
and
[Encode a device's raw BGRA still to PNG with the CodeBrix Imaging library](../BLUEPRINTS-MediaAndVision.md#encode-a-devices-raw-bgra-still-to-png-with-the-codebrix-imaging-library).

### Audio monitoring as one two-way checkbox

The audio feature is three members and no plumbing. `IsMicAvailable` is set from
the session's `IsAudioCaptureActive` right after it starts, and the checkbox binds
its `IsEnabled` to it, so a camera with no paired microphone leaves the box greyed.
`IsAudioMonitorOn` is two-way bound to `IsChecked`, and its setter pushes the new
value into the live session as well as storing it, so toggling takes effect
immediately; `SwitchCamera` then copies the stored value into each new session
before starting it, so the preference survives a camera change. Monitoring is off
at startup, which is the right default when speakers and microphone are in the same
room. See
[Bind a page level CheckBox two way](../BLUEPRINTS-ViewsAndControls.md#bind-a-page-level-checkbox-two-way),
[Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension)
and
[Push a bound toggle into a live native session and re-apply it to the next one](../BLUEPRINTS-MVVM.md#push-a-bound-toggle-into-a-live-native-session-and-re-apply-it-to-the-next-one).

### Shutting a native session down

`Dispose()` is short and the order matters. It disposes both lazily created
commands and nulls the fields, nulls both bridge delegates so no page closure keeps
the view model alive or fires into a torn-down canvas, unsubscribes from
`FrameReceived` before disposing the session - so a frame in flight cannot land on
a dead handler - and finally calls `base.Dispose()`. A view model that owns a
native resource has to do this; one that owns only managed state usually needs only
the command disposal. See
[Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### The six-head skeleton, start to finish

For the project layout rather than the camera, read in this order: a head's
`Program.cs` (they are identical apart from the `Use...()` call that names the
backend, plus the WinWpfSkia head's render-surface block), then that head's csproj
for the `Page` glob, the `Label="Shared"` import, the project reference and its
single runtime package, then `src/WebcamViewer.UI/WebcamViewer.UI.projitems` for
what the shared project contributes, then `src/WebcamViewer.UI/App.xaml.cs` for the
startup sequence, and finally `src/WebcamViewer.Core/Helpers/HostHelper.cs`.

`App`'s constructor does four things and nothing else - sets the default font,
creates the `SimpleServiceResolver` from `HostHelper.GetHost()`, calls
`SimpleViewModel.SetIsDesignMode(false)`, and calls `InitializeComponent()` - and
`OnLaunched` creates the window, puts a `Frame` in it and navigates to the page.
The service-registration lambda is empty here, with a comment saying why: the
capture session lives in the view model, so there is no service to register.

The sharp edges are mostly in the build files. New XAML pages must be added to the
`.projitems` by hand, as a `Page` with `Generator MSBuild:Compile` and as a
`Compile` with `DependentUpon` its `.xaml`; the shared project has no globbing. The
`.shproj` `ProjectGuid` and the `.projitems` `SharedGUID` must match. The
`<None Remove="**\*.xaml" />` beside each head's `Page` glob is required, or the
same files are treated as both content and pages. And `SetIsDesignMode(false)` is
not optional: without it the view model still believes it is in the designer at run
time, its guard returns from the constructor early, and the window comes up with an
empty dropdown and a black rectangle. See
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer),
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
[Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
and
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### What this application does not show

It is deliberately small: one page, one view model, one canvas. It has:

- No video recording and no overlay burn-in. The webcam library does both; this
  application only previews frames and writes single stills, so look elsewhere for
  a recording pattern.
- No camera configuration. Resolution, frame rate and pixel format are whatever the
  device delivers; nothing selects a capture format or reports the current one, and
  there is no frames-per-second readout.
- No hot-plug handling. The device list is built once at startup and never
  refreshed, so a camera attached afterwards does not appear until the next run.
- No mirroring of the preview, no zoom, pan or rotation, and no image adjustment of
  any kind before display.
- No settings or persistence - the chosen folder and the audio-monitor state are
  forgotten when the window closes.
- No dialogs. The page wires an `IXamlRootGetter` so that a view model *could* open
  one, and that is worth reading as the graceful-degradation pattern, but this view
  model reports everything through a status line instead. See
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- No registered services: the registration lambda in `App.xaml.cs` is a comment, so
  the wiring is shown but no resolution is.
- No converters, styles, templates, second page or navigation beyond the initial
  one, and no cancellation or progress reporting.
- No libraries and no tests, so nothing here shows the `src/libs` plus `tests/libs`
  layout or the family's test conventions.

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the
third-party content used by this application. Nothing third-party is bundled in
this folder: the code arrives as NuGet packages that carry their own licenses and
notices - including the webcam library, which is distributed under the LGPL - the
Open Sans font ships inside its package, and the native media runtime that Linux
and macOS need is installed by the user rather than redistributed here. The photos
the application writes, and the camera images it displays, belong to whoever made
them.

## License

WebcamViewer is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
