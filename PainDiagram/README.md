# PainDiagram

PainDiagram is an interactive pain- and symptom-mapping application. It shows a medical
body-map image and lets you draw freehand over it with a mouse, a pen or a finger to mark
where you feel something. Three symptom types are offered as buttons - Pain, Numbness and
Tingling - and each is a separate translucent highlighter layer with its own ink color
(Pain is bright magenta, Numbness is blue, Tingling is yellow-gold). The active layer is
marked with a check mark in its button caption. Clear starts over, asking for confirmation
once there is more than a stroke or two on the canvas. Save exports the composited result
- body map plus every layer - as a 1000 by 1000 pixel PNG through the head's native save
dialog, confirms a replace if the file already exists, and then offers to clear the canvas
for the next diagram.

It is the reference application for the CodeBrix.Imaging.Drawing library, and the clearest
example in this repository of one view model file driving eight heads: the six
CodeBrix.Platform Skia heads plus native WinUI 3 and WPF heads that reuse the same view
model source without the CodeBrix.Platform UI stack.

## What this sample shows a CodeBrix.Platform developer

- Compiling one `SimpleViewModel` into the Skia heads and into native WinUI 3 and WPF shells, with no second copy of the logic: [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads).
- Creating a drawing session in the view model with three named, colored highlighter layers and switching the active one from a command: [Create a drawing session with named color layers](../BLUEPRINTS-GraphicsAndRendering.md#create-a-drawing-session-with-named-color-layers).
- Forwarding mouse, pen and touch input from a canvas into a model that owns the stroke state, including capture-lost handling: [Forward pointer input from a canvas into a model](../BLUEPRINTS-ViewsAndControls.md#forward-pointer-input-from-a-canvas-into-a-model).
- Exporting the finished artwork as a PNG at a fixed pixel size, independent of the on-screen canvas: [Export a drawing at a chosen pixel size](../BLUEPRINTS-GraphicsAndRendering.md#export-a-drawing-at-a-chosen-pixel-size).
- Letting the view model repaint the canvas through a one-property bridge interface instead of touching a control: [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- Getting a save path from whatever picker the head has, through a delegate the page assigns: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Making one XAML element name resolve to a different Skia canvas base class per head: [Select a canvas base class per head with conditional compilation](../BLUEPRINTS-ViewsAndControls.md#select-a-canvas-base-class-per-head-with-conditional-compilation).
- Embedding one asset under the same logical resource name in every assembly that compiles the shared source, and loading it by reflection: [Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).
- Showing which mode is active in the button captions themselves, with computed properties and no converter: [Show selection state in button captions from computed properties](../BLUEPRINTS-MVVM.md#show-selection-state-in-button-captions-from-computed-properties).
- Turning off each head's own overwrite prompt so the view model asks the replace question exactly once: [Suppress a native save dialog overwrite prompt so the view model owns confirmation](../BLUEPRINTS-PlatformServices.md#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation).
- Cleaning up the empty placeholder file a WinRT save picker leaves at the chosen path: [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Asking the user a yes/no question from inside a command without referencing a dialog control: [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- Marshalling a library callback onto the UI thread before writing a bound property: [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Writing bound properties and lazily created `SimpleCommand` commands the way the family does, with `[AffectsCommands]` keeping `CanExecute` in step: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Disposing the commands, the bridge delegates and the library object the view model owns: [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- Handing the view model a lazy getter for the XAML root so its dialogs have somewhere to show: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Declaring the shared Skia page and binding it with the platform's `Binding` markup extension: [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- Keeping one `App.xaml` and one page for all six Skia heads in a shared project: [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Giving the Core library the application's root namespace so linked source and XAML agree: [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Starting a head from a `Program.Main()` that differs from its siblings only by the backend call: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Doing the whole application bootstrap in the `App` constructor, including `SetIsDesignMode(false)`: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Supplying a generic host builder to `SimpleServiceResolver` from a tiny linked helper: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Making a bundled font the default for all text on the Skia heads: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Wiring console logging that exists only in Debug builds: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Switching the WinWpfSkia head to the software render surface to avoid composition artifacts: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Letting the Windows-targeting heads compile inside the cross-platform solution on a Linux or macOS build host: [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- Restricting the Windows solution's platforms to the ones a WinUI 3 head declares: [Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares).
- Shipping a second solution for the heads that cannot build everywhere: [Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere).

## Building, running and testing

### Solutions

| Solution | Open it on | Contains |
| --- | --- | --- |
| `PainDiagram.slnx` | Linux, macOS, Windows | `PainDiagram.UI`, `PainDiagram.Core` and the six CodeBrix.Platform Skia heads |
| `PainDiagram.Windows.slnx` | Windows | Everything in the cross-platform solution plus the native `PainDiagram.WinUI` and `PainDiagram.Wpf` heads |

Both files carry a comment at the top saying which is which. The Windows solution declares
only x86, x64 and ARM64 solution platforms, because `PainDiagram.WinUI` declares those
same platforms and no Any CPU; a solution offering Any CPU could not map it to that
project. The WinUI project entry maps each solution platform explicitly and sets `Deploy`
for `Debug|x64`.

### The heads

CodeBrix.Platform Skia heads, all under `CodeBrixPlatform/`:

| Project | Platform | Host builder call |
| --- | --- | --- |
| `PainDiagram.Win32Skia` | Windows, Win32 window | `UseWindowsWin32()` |
| `PainDiagram.WinWpfSkia` | Windows, Skia hosted in a WPF application context | `UseWindowsWpf()` |
| `PainDiagram.LinuxX11` | Linux desktop, X11 | `UseLinuxX11()` |
| `PainDiagram.LinuxWayland` | Linux desktop, native Wayland | `UseLinuxWayland()` |
| `PainDiagram.LinuxFrameBuffer` | Linux framebuffer, for embedded and kiosk devices with no desktop | `UseLinuxFrameBuffer()` |
| `PainDiagram.MacOS` | macOS | `UseMacOS()` |

Native, non-Skia heads at the application root:

| Project | UI stack |
| --- | --- |
| `PainDiagram.WinUI` | WinUI 3 (Windows App SDK) |
| `PainDiagram.Wpf` | WPF |

### Prerequisites

- The .NET 10 SDK. Every Skia head targets `net10.0`, except `PainDiagram.WinWpfSkia`
  which targets `net10.0-windows`; both native heads target a Windows platform TFM.
- No accounts, tokens, network access, downloaded data or special hardware. The one asset
  the application needs, `Shared/Assets/body_map_master.png`, is in this folder and is
  embedded into every head assembly at build time.
- The Linux desktop heads need the windowing stack their backend expects (X11 for
  `PainDiagram.LinuxX11`, a Wayland compositor for `PainDiagram.LinuxWayland`); the
  framebuffer head needs access to the Linux framebuffer device.
- `PainDiagram.WinWpfSkia` and `PainDiagram.Wpf` both set `EnableWindowsTargeting`, so
  they compile on Linux and macOS build hosts inside the cross-platform solution. They
  only run on Windows.

### Running a head

From the repository root:

```text
dotnet run --project PainDiagram/CodeBrixPlatform/PainDiagram.LinuxX11/PainDiagram.LinuxX11.csproj
```

Swap the project path for any of the other five Skia heads. The two native heads must be
built and run on Windows from `PainDiagram.Windows.slnx` with an explicit platform, since
they have no Any CPU configuration. The WinUI head ships two launch profiles,
`PainDiagram.WinUI (Package)` for the MSIX build and `PainDiagram.WinUI (Unpackaged)`.

### Tests

This application has no tests folder and no test project, and no `global.json`, so no test
runner is selected here and there is nothing to run with `dotnet test`. The drawing logic
that would be worth testing lives in the CodeBrix.Imaging.Drawing library rather than in
the sample; the sample's own code is the view model, a pair of small helpers and the per-head
plumbing.

## How the projects and folders are organized

```text
PainDiagram/
  PainDiagram.slnx                        Cross-platform solution: shared UI and Core plus the six Skia heads
  PainDiagram.Windows.slnx                Windows solution: the above plus the WinUI 3 and WPF heads
  THIRD-PARTY-NOTICES.txt                 Provenance of the bundled body-map image
  Shared/                                 Source that is file-linked into every head assembly
    ViewModels/MainViewModel.cs           The whole application: state, commands, drawing session, bridge interfaces
    Drawing/DrawingCanvas.cs              One canvas type name that resolves per head by conditional compilation
    Helpers/HostHelper.cs                 IHostBuilderProvider wrapper handed to SimpleServiceResolver
    Helpers/FileDialogHelper.cs           Cleanup for the placeholder file the WinRT save picker leaves behind
    Assets/body_map_master.png            The body-map background image
  CodeBrixPlatform/                       Everything built on CodeBrix.Platform
    PainDiagram.UI/                       Shared XAML project (.shproj plus .projitems): App.xaml(.cs), Views/MainPage.xaml(.cs)
    PainDiagram.Core/                     Library: links the Shared source, embeds the body map, carries the shared packages
    PainDiagram.Win32Skia/                Head: Program.cs plus one runtime package
    PainDiagram.WinWpfSkia/               Head: Program.cs plus one runtime package, net10.0-windows
    PainDiagram.LinuxX11/                 Head: Program.cs plus one runtime package
    PainDiagram.LinuxWayland/             Head: Program.cs plus one runtime package
    PainDiagram.LinuxFrameBuffer/         Head: Program.cs plus one runtime package
    PainDiagram.MacOS/                    Head: Program.cs plus one runtime package
  PainDiagram.WinUI/                      Native WinUI 3 head: own App and MainPage, Win32SaveFileDialog, MSIX assets and manifests
  PainDiagram.Wpf/                        Native WPF head: own App and MainWindow, WPF SaveFileDialog
```

Nothing depends on a head. Each of the six Skia heads has a `ProjectReference` to
`PainDiagram.Core` and an `Import` of `PainDiagram.UI/PainDiagram.UI.projitems`, so the
shared XAML is compiled into the head assembly while the view model and the canvas type
come from `PainDiagram.Core`. The two native heads reference neither project: they link
the same source from `Shared/` with `<Compile Include>` items of their own, and each
declares the drawing library reference directly.

`Shared/` is not a project. It is source compiled into `PainDiagram.Core`,
`PainDiagram.WinUI` and `PainDiagram.Wpf` alike, which is exactly why the body-map image
is embedded by each of those three projects under one logical resource name.
`PainDiagram.Wpf` links the canvas, the host helper and the view model but not
`FileDialogHelper.cs`, because the WPF `SaveFileDialog` creates no placeholder file to
clean up.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Imaging.Drawing | The subject of the sample. The view model creates a `DrawingSession`, adds three named color layers to it, hands it the body map as a `byte[]` background, and calls `ExportPng()` for the saved image; the page calls `Render()` in its paint handler and forwards pointer events to the session | `Shared/ViewModels/MainViewModel.cs` and each UI stack's page code-behind; referenced by `PainDiagram.Core`, `PainDiagram.WinUI` and `PainDiagram.Wpf` |
| CodeBrix.Platform | The XAML framework and the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `[AffectsCommands]`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, the `ConfirmDialog()` and `ShowError()` helpers, and `CodeBrixPlatformHostBuilder` | `CodeBrixPlatform/PainDiagram.Core`, `CodeBrixPlatform/PainDiagram.UI`, every head `Program.cs`; the Simple toolkit is used by the two native heads as well |
| CodeBrix.Platform SkiaSharp views | Supplies `SKXamlCanvas`, the Skia surface hosted in CodeBrix.Platform XAML, which `DrawingCanvas` derives from on the Skia heads | `Shared/Drawing/DrawingCanvas.cs`, `CodeBrixPlatform/PainDiagram.Core/PainDiagram.Core.csproj` |
| CodeBrix.Platform Open Sans font | The bundled UI font, set both as the page `FontFamily` resource and as the platform default text font | `CodeBrixPlatform/PainDiagram.UI/App.xaml`, `CodeBrixPlatform/PainDiagram.UI/App.xaml.cs` |
| CodeBrix.Platform Skia runtime for each head | One backend package per head (Win32, WPF-hosted, X11, Wayland, framebuffer, macOS). Nothing else is added at head level | each head csproj under `CodeBrixPlatform/PainDiagram.*/` |
| CodeBrix.Platform WinUI support | Lets the native WinUI 3 head use the Simple MVVM toolkit and the shared view model | `PainDiagram.WinUI/PainDiagram.WinUI.csproj` |
| CodeBrix.Platform WPF support | The same for the native WPF head | `PainDiagram.Wpf/PainDiagram.Wpf.csproj` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | Supplies `Host.CreateDefaultBuilder()`, which `HostHelper` hands to `SimpleServiceResolver` | `Shared/Helpers/HostHelper.cs`; referenced by `PainDiagram.Core`, `PainDiagram.WinUI`, `PainDiagram.Wpf` |
| Microsoft.Extensions.Logging.Console | The Debug-build console logging wired up in `App.InitializeLogging()` | `CodeBrixPlatform/PainDiagram.UI/App.xaml.cs` |
| SkiaSharp WPF views | Supplies `SKElement`, the base class `DrawingCanvas` resolves to on the native WPF head | `Shared/Drawing/DrawingCanvas.cs`, `PainDiagram.Wpf/PainDiagram.Wpf.csproj` |
| SkiaSharp WinUI views | Supplies `SKXamlCanvas` on the native WinUI 3 head | `PainDiagram.WinUI/PainDiagram.WinUI.csproj` |
| Windows App SDK and Windows SDK build tools | Required by the native WinUI 3 head and its MSIX packaging | `PainDiagram.WinUI/PainDiagram.WinUI.csproj` |

The exact package for each row is in the project file named in the "Where" column. The
CodeBrix packages follow the family's license-suffix naming convention, where the license
the package ships under is part of its identifier.

## Worth studying in this application

### One view model file, eight heads

`Shared/ViewModels/MainViewModel.cs` holds every piece of state, every command and the
drawing session. It is not shipped as a library: `PainDiagram.Core` links it for the six
Skia heads, and `PainDiagram.WinUI` and `PainDiagram.Wpf` link the same file directly, so
the identical source is compiled into three different assemblies. Everything a head can do
that the view model cannot is expressed as a small interface the view model implements -
`IFileSaveBridge` and `ICanvasInvalidator`, both declared in the same file - and each
head's page assigns the delegate in one place. The only conditional compilation inside the
view model is a single `[Bindable]` attribute guarded by `HAS_CODEBRIX`, a symbol
`PainDiagram.Core` and the six Skia heads define and the native heads do not.

Read `Shared/ViewModels/MainViewModel.cs` first, then the `<Compile Include>` block in
`CodeBrixPlatform/PainDiagram.Core/PainDiagram.Core.csproj`, then the same block in
`PainDiagram.Wpf/PainDiagram.Wpf.csproj`. The sharp edge is that file-linked source
obliges every consuming assembly to supply what the source expects at run time - here, the
embedded body-map resource. See
[Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads).

### The drawing session and its three highlighter layers

The view model creates the `DrawingSession` in its constructor, inside
`if (!IsDesignMode(true))` so a XAML designer that instantiates the class never builds one.
It adds the three layers once, by name and opaque RGB color, and exposes the session as a
read-only `Session` property that the page renders and feeds. The layer names are
`const string` fields used both as the lookup key into the session and as the value of the
bound `ActiveLayerName` property, so the two cannot drift apart, and `SetActiveLayer()`
only updates `ActiveLayerName` when `GetLayer()` actually returned a layer.

Selecting a symptom is therefore just a command that looks a layer up by name. The XAML
tints each button with the same hue at 40 percent alpha and says in a comment that this is
to match the on-canvas ink; the ink itself is drawn by the library from the opaque color
the view model passed. See
[Create a drawing session with named color layers](../BLUEPRINTS-GraphicsAndRendering.md#create-a-drawing-session-with-named-color-layers).

### Pointer input in, repaint requests out

The page forwards press, move, release and capture-lost straight to the session, a few
lines each, and captures the pointer while a stroke is in progress. The session decides
whether a press starts a stroke - `PointerPressed()` returns a bool - and tracks whether
one is in flight through `IsPointerActive`, so the page holds no drawing state and the
view model is not on the per-point path at all.

Two details are worth copying. Every point is passed together with the current view size
from `DrawCanvas.GetViewSize()`, which is how the session keeps strokes in its own logical
space rather than in device pixels; and `PointerCaptureLost` calls `PointerCanceled()`, or
a stroke would stay half open when the window deactivates mid-drag. The WPF head does the
same thing with `MouseDown`/`MouseMove`/`MouseUp`/`LostMouseCapture` and `CaptureMouse()`.
Read `CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs` and then
`PainDiagram.Wpf/Views/MainWindow.xaml.cs`. See
[Forward pointer input from a canvas into a model](../BLUEPRINTS-ViewsAndControls.md#forward-pointer-input-from-a-canvas-into-a-model).

Repaint requests travel the other way. The session raises `RedrawRequested` as strokes
arrive; the view model subscribes once, in its constructor, and forwards it to the
`InvalidateCanvas` delegate that
`ICanvasInvalidator` declares; the page assigns that delegate to a closure over its own
canvas when the `DataContext` arrives. There is no timer and no per-frame polling anywhere
in the application.

The second session event, `DrawingChanged`, is handled differently on purpose: it goes
through `InvokeOnMainThread()` because its handler writes the bound `HasDrawing` property,
and `HasDrawing` carries `[AffectsCommands(nameof(SaveCommand), nameof(ClearCommand))]`, so
that one assignment also re-evaluates two commands' `CanExecute`. The WPF head cannot
simply call `InvalidateVisual()` from wherever the event arrives - it checks
`Dispatcher.CheckAccess()` and marshals with `BeginInvoke()` - while the Skia and WinUI
pages assign the invalidate call directly. `InvalidateCanvas` is null until a page wires it
up, so the view model always calls it with `?.Invoke()`. See
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
and
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).

### Saving: one bridge delegate, three dialogs, one confirmation

`IFileSaveBridge` declares a single settable delegate,
`Func<string, Task<string>> PickSavePngPathAsync`, that takes a suggested file name and
returns the chosen path or null. The view model implements the interface itself; each page
assigns the delegate in its `DataContextChanged` handler, backed by that head's own picker.
`DoSave()` awaits the delegate, treats a null or blank result as a cancel, trims the path,
asks its own replace question through `ConfirmDialog()` when the file exists, sets `IsBusy`,
exports and writes the PNG, then offers to clear.

Three things about it repay reading. The `DataContextChanged` subscription is made before
`InitializeComponent()`, with a comment saying why: `InitializeComponent()` may be the call
that sets the `DataContext`, so a handler attached afterwards would never fire. The shared
page needs `using System;` for the awaiter extension that makes the WinRT picker awaitable,
and carries a comment saying so, because the using looks unused and is easy to remove by
mistake. And `DoSave()` catches `NotSupportedException` separately, reporting "File dialogs
are not supported on this head" - that is the arm a head reaches when its page did assign a
delegate but the underlying dialog cannot run there. The `GetDefaultSavePath()` branch above
it runs only when `PickSavePngPathAsync` is still null, which is the case for a head that
deliberately leaves the delegate unset; the shared `MainPage` used by all six Skia heads
always sets it.

Read `Shared/ViewModels/MainViewModel.cs` (`DoSave()`), then
`CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs`. See
[Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model)
and
[Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).

### Making the application own the replace question

Because the view model asks "replace this file?" itself, every head suppresses its picker's
own prompt so the user sees the question once. The WPF head sets
`SaveFileDialog.OverwritePrompt = false`. The WinUI head cannot: the WinRT picker always
shows its confirmation and offers no way to turn it off, so
`PainDiagram.WinUI/Views/Win32SaveFileDialog.cs` wraps the Win32 common item dialog
instead, clears the `FOS_OVERWRITEPROMPT` option, adds `FOS_FORCEFILESYSTEM`, treats the
cancel HRESULT as a null result, and releases its COM objects and the display-name pointer
in `finally` blocks. Because that dialog needs a window handle, `App` exposes the main
window as a static `CurrentWindow` purely so the page can ask for its HWND.

The Skia heads keep the WinRT picker, which creates an empty placeholder file at a
brand-new path. `Shared/Helpers/FileDialogHelper.RemoveEmptyPlaceholder()` deletes that
file - but only when it is genuinely zero length, so a real file with content is never
removed before the user has confirmed - and a failure to delete is deliberately ignored,
since the application's own prompt covers it. Read the class comment in
`Win32SaveFileDialog.cs` first; it records the whole reason. See
[Suppress a native save dialog overwrite prompt so the view model owns confirmation](../BLUEPRINTS-PlatformServices.md#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation)
and
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).

### The body-map image, embedded once per assembly under one name

The view model loads the background with
`typeof(MainViewModel).Assembly.GetManifestResourceStream(BodyMapResourceName)`, which is a
different assembly on every head. That works because `PainDiagram.Core`,
`PainDiagram.WinUI` and `PainDiagram.Wpf` each declare an `EmbeddedResource` for
`Shared/Assets/body_map_master.png` with an explicit and identical `<LogicalName>`; without
it each project would derive its own resource name from its root namespace and link path
and only one head would find the image. The load failure path is non-fatal: it writes a
debug line and returns, leaving the session with no background rather than throwing during
construction, and the image reaches the session as a `byte[]`, so the same code works on a
head with no file system access to the asset. See
[Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).

### One canvas element name, two base classes

All three XAML files write `<drawing:DrawingCanvas x:Name="DrawCanvas" />`, but the Skia
and WinUI heads need a `SKXamlCanvas` and the WPF head needs an `SKElement`.
`Shared/Drawing/DrawingCanvas.cs` resolves that with one linked file: an empty subclass
chosen by `#if (HAS_CODEBRIXPLATFORM || HAS_WINUI)`, plus `DrawCanvasHelper` extension
methods that hide the per-stack `Point` type behind `GetPointFromPosition()`.

The file declares its type in the drawing library's namespace even though it compiles into
the application assembly, which is what lets every head use one `xmlns` prefix - but the
XAML must still name the assembly the type ends up in: `assembly=PainDiagram.Core` on the
Skia heads, the head's own assembly on WPF. `PainDiagram.Core` is the only project that
defines `HAS_CODEBRIXPLATFORM`, and the native WPF head defines neither symbol, which is
the `#else` path; if you add a head, decide which symbol it defines before anything else.
See
[Select a canvas base class per head with conditional compilation](../BLUEPRINTS-ViewsAndControls.md#select-a-canvas-base-class-per-head-with-conditional-compilation).

### The shared XAML project, the Core library and the page markup

`CodeBrixPlatform/PainDiagram.UI` is a shared project - a `.shproj` and a `.projitems` -
holding only `App.xaml`, `Views/MainPage.xaml` and their code-behind. Every Skia head
imports the `.projitems`, so the pages are compiled into the head assembly where the XAML
build targets can see them, and every head csproj carries both the `Page` glob and the
matching `<None Remove>` line or those `.xaml` files are not compiled as pages at all.
`PainDiagram.Core` is an ordinary library that links the `Shared/` source, embeds the body
map and carries every package reference beyond the head's own backend.

The namespace arrangement is the part to get right. `PainDiagram.Core` sets
`<RootNamespace>PainDiagram</RootNamespace>` explicitly so the linked source keeps the
namespace the XAML expects; the shared project's `Import_RootNamespace` is a different
value and does not change it; and the shared XAML declares `x:Class="PainDiagram.App"` and
`x:Class="PainDiagram.Views.MainPage"` - the application namespace, not the shared
project's name. Read `PainDiagram.UI.projitems`, then `PainDiagram.Core.csproj`, then any
head csproj. See
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project)
and
[Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).

`CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml` declares the CodeBrix.Platform
namespaces explicitly with `clr-namespace:...;assembly=...` and binds with `{d:Binding ...}`,
where `d` is the platform's data namespace; the view model is instantiated by the XAML as the
page's `DataContext`. Both the view model and the canvas come from `assembly=PainDiagram.Core`
even though the page itself compiles into the head assembly.

The WinUI and WPF pages are near-identical markup using their own stack's plain `{Binding}`.
`Style="{ThemeResource AccentButtonStyle}"` on the Save button works on the Skia and WinUI
heads; the WPF window uses a plain button instead. Only the shared page is compiled by more
than one project - the other two belong to their own UI stack. See
[Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).

### Startup: host builder, service resolver, design mode, font, logging

A head is `Program.cs` plus one runtime package, and contains no application logic: it calls
`App.InitializeLogging()`, then `CodeBrixPlatformHostBuilder.Create().App(() => new App())`,
the backend `Use...()` call, `Build()` and `Run()`. `[STAThread]` is on `Main()` in every
head, including the Linux ones.

`App`'s constructor sets the platform's default text font family to the bundled Open Sans
file, creates the service resolver from `HostHelper.GetHost()` with an empty registration
callback that says so in a comment, and calls `SimpleViewModel.SetIsDesignMode(false)`
before `InitializeComponent()` - it has to run before any view model is constructed, and
the page's `DataContext` is created by the XAML. All three `App` classes, Skia, WinUI and
WPF, make those same two Simple toolkit calls, which is why `HostHelper.cs` is linked into
all three assemblies; the WPF one has no `InitializeComponent()` call of its own and the two
calls are its entire constructor.

`App.xaml` also declares a `FontFamily` resource keyed `OpenSansFont` that the page binds
its own `FontFamily` to, and its comment records the gotcha: merging the font package's
`Fonts.xaml` dictionary does not work on Skia targets, so reference the `.ttf` by its
`ms-appx:///` path instead. Because that resource lives in the shared `App.xaml`, the two
native heads - which have their own `App.xaml` - do not get it. `InitializeLogging()` is
entirely inside `#if DEBUG`, and its adapter call is separately guarded by `HAS_CODEBRIX`
so the same method body can be linked into a project that does not define it. See
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
and
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### Commands, enablement and the check-mark captions

The commands are lazily created `SimpleCommand` fields. `SaveCommand` and `ClearCommand`
are built from a `CanExecute` predicate and an async handler; the three layer-selection
commands have no meaningful predicate and are built from the handler alone, with
synchronous bodies in an async-shaped signature that end `return Task.CompletedTask;`. Both
guarded handlers re-check their predicate on entry as a cheap guard against a stale
invocation, even though the framework checks `CanExecute` too.

Enablement is declarative: `HasDrawing` and `IsBusy` carry
`[AffectsCommands(nameof(SaveCommand), nameof(ClearCommand))]`, so setting either one
refreshes both buttons with no manual `RaiseCanExecuteChanged()` call, and `IsBusy` is set
around the export and reset in a `finally` so the buttons disable themselves while it runs.
The active-symptom indicator works the same way without a converter: `ActiveLayerName` is a
private-set property whose setter notifies three computed caption properties, and the XAML
binds each button's `Content` to one of them. All of these properties use the C# `field`
keyword with `SetProperty(ref field, value)`, so there are no backing fields to declare, and
`ActiveLayerName`'s initializer sets the first caption without running the setter body. See
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way)
and
[Show selection state in button captions from computed properties](../BLUEPRINTS-MVVM.md#show-selection-state-in-button-captions-from-computed-properties).

### Dialogs, the XAML root, and disposal

`SimpleViewModel` supplies awaitable `ConfirmDialog()` and `ShowError()` helpers, so
`DoClear()` and `DoSave()` await a yes/no answer inline without referencing a dialog
control. `DoClear()` only asks when there is more than a stroke or two on the canvas, a
deliberate choice so clearing a nearly empty drawing is not annoying. The page's only
contribution is handing the view model a way to reach the XAML root, and it passes a lambda
rather than the root itself, in the same `DataContextChanged` handler that wires the other
bridges, because the page's `XamlRoot` is usually still null when the `DataContext` is first
set. The native WPF head does not do this at all - WPF has no `XamlRoot` - and its dialogs
still work, so the shared view model must not assume the getter was supplied.

`Dispose()` disposes and nulls every lazily created command, nulls both bridge delegates so
the view model no longer references the page, disposes the session and calls
`base.Dispose()` last. `_session` is a field rather than a get-only property precisely so it
can be nulled there; the public `Session` property returns it, so every consumer's
null-conditional access keeps working after disposal. See
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
and
[Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### Build details that only bite once

`PainDiagram.WinWpfSkia` is the only head whose `Program.cs` differs from the common shape:
after `Build()` and before `Run()` it casts the host - defensively, with
`if (host is WpfHost wpfHost)` - and sets `RenderSurfaceType.Software`, because the WPF
host's default renderer draws onto WPF's own composited window and produces composition
conflicts on many systems.

That same head targets `net10.0-windows` but must not set `UseWPF`, or the WPF build targets
claim the platform's XAML `Page` items; the genuinely native WPF head does set `UseWPF`, and
must carry a Windows platform version in its TFM because the SkiaSharp WPF views package
only ships assets for that platform. Both set `EnableWindowsTargeting` so the cross-platform
solution still builds on a Linux or macOS host. The native WPF head also rewrites its own
`RootNamespace` by stripping `.Wpf` from the project name, so its linked shared source lands
in the namespace the XAML expects. See
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution),
[Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares)
and
[Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere).

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder records the provenance of the bundled body-map
image, `Shared/Assets/body_map_master.png`: it is the author's own original work rather
than third-party content, and it is provided under this repository's license. The same file
states the repository's rule for code, namely that third-party code arrives as NuGet
packages that carry their own licenses and their own notices, so those are not reproduced
here. There are no `LICENSE_*.txt` files in this folder.

## License

PainDiagram is licensed under the Apache License, Version 2.0, see [../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
