# PicoScope.Brix

PicoScope.Brix is a one-page oscilloscope front end. The window carries a
header naming the attached device and its discovered capabilities, a row of
acquisition controls (single capture, start and stop streaming, a voltage-range
picker and two channel-visibility checkboxes), a row of signal-generator
controls (waveform picker, frequency box, a toggle and a flash-the-LED button),
a live chart filling the rest of the window, and a status line. On startup the
application asks for the best scope available, configures both input channels,
takes one block capture so the chart is never empty, and then runs a continuous
stream into it. With no hardware attached it falls back to a built-in simulated
device that enforces the same limits the real instrument does, and a SIMULATED
badge appears at the end of the status line.

It is this repository's reference for two things. The first is driving a real
USB instrument from a CodeBrix.Platform application: a device-agnostic contract
library with no interop and no platform reference, a separate interop library
holding the driver bindings, and each head deciding at startup which
implementations the application may use. The second is hosting a live,
high-rate chart: the CodeBrix.Platform PlotterView add-in renders a plot model
that a streaming callback mutates on the driver's own polling thread, with no
dispatcher hop and no dropped batches. It is also a written-down reference for
Pico Technology's `ps2000` driver, whose documented behavior is surprising in
several places that the code names and handles.

## What this sample shows a CodeBrix.Platform developer

- How to keep the real work in a UI-free library that no head and no view model
  can accidentally couple to a device or a framework:
  [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- How an application grows past one page and one view model into libraries with
  mirrored test projects:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Why a library declares one package and uses types from several, and what that
  means for what you may depend on:
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Which project each package reference belongs on: the framework, the add-in and
  the font on the Core library, exactly one runtime package on each head:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- What a head's `Program.Main` contains, and how this application uses that one
  file per head as the place where device implementations are registered:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- The four things the `App` constructor does, in the order they have to happen:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- How `OnLaunched` creates the window, puts a `Frame` in it and navigates to the
  first page:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- How to give `SimpleServiceResolver` a generic-host builder from the shared Core
  library instead of duplicating it in six heads:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- The family's property and command idiom, `[AffectsCommands]` keeping every
  button's enablement current from two bound flags, and `[AffectsProperties]`
  keeping a computed status badge current from one of them:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Why a view model constructed by the XAML designer needs a guard, and what it is
  paired with at startup:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How a slow device open runs off the UI thread and its results reach bound
  collections safely:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- How a library declares its own exception type so the view model can tell a
  rule violation from a real failure:
  [Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch).
- How every failure this application can meet becomes a line of status text
  rather than an exception or a dialog:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- Two pickers, one bound straight to enum values and one bound to a tiny wrapper
  type that supplies the label the instrument would print:
  [Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).
- The one-handler code-behind that hands the view model a `XamlRoot` getter
  through `IXamlRootGetter`:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- How `App.xaml` and the `Views` folder are file-linked into all six executables
  through a shared items project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Why the Core library's `RootNamespace` is set to the application name, and what
  the shared XAML then has to write to reach the view models:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- How one Windows-targeting head stays inside a solution that restores on Linux
  and macOS:
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- The one per-head behavioral difference in the whole application, applied after
  `Build()` and before `Run()`:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- How a font that ships inside a package becomes the application-wide default and
  a page-level `StaticResource`, with script fallbacks behind it:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- How to get console diagnostics while developing and a silent Release build:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- How the two libraries are set up for testing, including the one line that lets
  a test project see a library's internals:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
  and
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Where the record of what this folder does and does not redistribute lives:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- How each head, and only a head, decides which device implementations exist,
  and how the view model asks for one without naming any of them:
  [Register hardware implementations from each head and ask a finder for the best one](../BLUEPRINTS-AppStructureAndStartup.md#register-hardware-implementations-from-each-head-and-ask-a-finder-for-the-best-one).
- How a real chart gets onto the page with one binding and no chart code in the
  code-behind:
  [Host a live chart with the PlotterView add-in and bind the model the view model owns](../BLUEPRINTS-ViewsAndControls.md#host-a-live-chart-with-the-plotterview-add-in-and-bind-the-model-the-view-model-owns).
- Why streaming batches are applied on the thread they arrive on, and what has to
  be true for that to be safe:
  [Mutate a plot model under its own sync root so streamed batches need no dispatcher hop](../BLUEPRINTS-MVVM.md#mutate-a-plot-model-under-its-own-sync-root-so-streamed-batches-need-no-dispatcher-hop).
- How a million points a second become a couple of thousand on screen, with a
  redraw cost that does not grow:
  [Decimate incoming samples to a fixed point budget so redraw cost stays flat](../BLUEPRINTS-GraphicsAndRendering.md#decimate-incoming-samples-to-a-fixed-point-budget-so-redraw-cost-stays-flat).
- Why the simulated device enforces the real instrument's limits, and what that
  buys the test project:
  [Ship a simulator that enforces the real device's limits rather than a stub](../BLUEPRINTS-Testing.md#ship-a-simulator-that-enforces-the-real-devices-limits-rather-than-a-stub).
- How one assembly binds to a native driver on both operating systems and finds
  it wherever the machine's own installation put it:
  [Bind a native driver by its bare name and resolve it yourself at run time](../BLUEPRINTS-ProjectLayoutAndPackaging.md#bind-a-native-driver-by-its-bare-name-and-resolve-it-yourself-at-run-time).
- Why the streaming callback lives in a field rather than being passed at each
  call, and what happens when it does not:
  [Root a native callback delegate for the life of a streaming session](../BLUEPRINTS-MVVM.md#root-a-native-callback-delegate-for-the-life-of-a-streaming-session).
- Why releasing the device is wired to two separate events, and what makes that
  safe:
  [Release an exclusive device handle from both the page unload and the window close](../BLUEPRINTS-PlatformServices.md#release-an-exclusive-device-handle-from-both-the-page-unload-and-the-window-close).
- How a bound setter that pushes its value at the instrument reports a refusal in
  the status line, and re-reads what the failed attempt may have changed:
  [Fail a bound setter into the status line when a device refuses it](../BLUEPRINTS-MVVM.md#fail-a-bound-setter-into-the-status-line-when-a-device-refuses-it).

## Building, running and testing

There is one solution, `PicoScope.Brix.slnx`, and it holds everything: the
shared UI project, the Core project, all six heads, the two libraries under a
`Libraries` solution folder and the two test projects under a `Tests` solution
folder. Its header comment describes it as everything that builds with the plain
.NET SDK on Linux, macOS and Windows, which holds here because every head is a
Skia head. There is no second, Windows-only solution.

The heads:

| Project | Platform |
| --- | --- |
| `src/PicoScope.Brix.LinuxX11` | Linux, X11 |
| `src/PicoScope.Brix.LinuxWayland` | Linux, Wayland |
| `src/PicoScope.Brix.LinuxFrameBuffer` | Linux, framebuffer (no display server) |
| `src/PicoScope.Brix.MacOS` | macOS |
| `src/PicoScope.Brix.Win32Skia` | Windows, Win32 window |
| `src/PicoScope.Brix.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting` so the solution still
  restores and builds on Linux and macOS. No workload is needed.
- All CodeBrix packages come from NuGet. No CodeBrix library is referenced as a
  source project, so this folder builds on its own.
- **No hardware is required.** With nothing attached, every head falls back to
  the simulated device and the whole application works. Everything below is only
  needed to talk to a real instrument.
- The `ps2000` driver, for real hardware. On Windows that means PicoSDK or the
  PicoScope desktop application, either of which installs the driver. On Linux it
  means the `libps2000` package from Pico's apt repository, on its own or as a
  dependency of the `picoscope` application package; it installs the driver as
  `/opt/picoscope/lib/libps2000.so` and adds that folder to the system loader
  configuration, so the runtime finds it with no help. The driver is never
  committed here and is never bundled: it is located and loaded at run time from
  whatever is installed.
- A 64-bit process. The shipped `ps2000` driver is 64-bit on both operating
  systems, so a 32-bit host fails with `BadImageFormatException`.
- No `sudo` and no group membership on Linux: the driver package's udev rule
  opens Pico's USB devices to every user.
- **The PicoScope desktop application must be closed.** Only one process may hold
  a device open; while the desktop application has it, opening returns no device
  and this application quietly uses the simulator instead.
- No accounts, tokens, network access or user-supplied data files.

To run one head from the command line, from this folder:

```text
dotnet run --project src/PicoScope.Brix.LinuxX11
dotnet run --project src/PicoScope.Brix.LinuxWayland
dotnet run --project src/PicoScope.Brix.LinuxFrameBuffer
dotnet run --project src/PicoScope.Brix.MacOS
dotnet run --project src/PicoScope.Brix.Win32Skia
dotnet run --project src/PicoScope.Brix.WinWpfSkia
```

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` is inside `#if DEBUG` - so a Release run is silent. In
a Debug run the console reports the device that was found, the full path of the
driver that was actually loaded, the discovered capability map, and the first
streaming batch.

Both test projects use xUnit v3 with SilverAssertions, build as `Exe` and set
`UseMicrosoftTestingPlatformRunner`, so each test assembly is a self-executing
binary. That matters in practice: a plain `dotnet test` can report that it
discovered zero tests. When it does, build the test project and run the produced
executable directly:

```text
dotnet build tests/libs/PicoScope.Brix.ScopeData.Tests/PicoScope.Brix.ScopeData.Tests.csproj -c Release
./tests/libs/PicoScope.Brix.ScopeData.Tests/bin/Release/net10.0/PicoScope.Brix.ScopeData.Tests
```

What each test project needs:

| Test project | Covers | Needs |
| --- | --- | --- |
| `tests/libs/PicoScope.Brix.ScopeData.Tests` | The model types and their conversions, the capability set, the device-finder preference order and reset semantics, and the simulated device's whole lifecycle including the limits it enforces - driven partly through a scripted in-test device | Nothing. No hardware, no driver, no network |
| `tests/libs/PicoScope.Brix.ScopeData.Ps2000.Tests` | The interop library's test scaffold | Nothing. The driver bindings themselves cannot be exercised without an attached instrument, so nothing here calls into the driver |

## How the projects and folders are organized

```text
PicoScope.Brix/
  PicoScope.Brix.slnx                   The one solution; every project; opens on Linux, macOS and Windows
  THIRD-PARTY-NOTICES.txt               Third-party content used by this application
  src/
    PicoScope.Brix.UI/                  Shared items project: the XAML that every head compiles
      PicoScope.Brix.UI.shproj          Shared-project shell, so an IDE can load the folder as a project
      PicoScope.Brix.UI.projitems       The shared file list each head imports with Label="Shared"
      App.xaml                          Merged control resources and the Roboto FontFamily resource
      App.xaml.cs                       Bootstrap: default font and fallbacks, service resolver, design mode, window, frame, logging
      Views/MainPage.xaml               The whole UI: header, acquisition row, generator row, chart, status line
      Views/MainPage.xaml.cs            Thin code-behind: XamlRoot getter, and the page lifetime that starts and releases the scope
    PicoScope.Brix.Core/                Class library; carries every non-head package
      PicoScope.Brix.Core.csproj        RootNamespace PicoScope.Brix; framework, chart add-in, font, hosting and logging packages
      Helpers/HostHelper.cs             The IHostBuilderProvider that SimpleServiceResolver builds its container from
      Charting/ScopePlot.cs             Owns the plot model; turns blocks and streaming batches into series under the model's lock
      ViewModels/MainViewModel.cs       All application logic: find, configure, capture, stream, generate, report
      ViewModels/VoltageRangeOption.cs  One combo-box item per accepted range, labelled the way an instrument labels it
    PicoScope.Brix.LinuxX11/            Head: Program.cs registers the device implementations, then starts the X11 backend
    PicoScope.Brix.LinuxWayland/        Head: same, Wayland backend
    PicoScope.Brix.LinuxFrameBuffer/    Head: same, Linux framebuffer backend
    PicoScope.Brix.MacOS/               Head: same, macOS backend
    PicoScope.Brix.Win32Skia/           Head: same, Win32 backend
    PicoScope.Brix.WinWpfSkia/          Same again, plus net10.0-windows and a software render surface
    libs/
      PicoScope.Brix.ScopeData/         The device-agnostic contract: IScopeDataDevice, the model types, the finder
        Model/                          Channels, ranges, timebases, triggering, streaming, generator, capture blocks, unit info
        Simulation/                     The simulated device, held to the real instrument's limits
      PicoScope.Brix.ScopeData.Ps2000/  The real device: driver bindings, driver loader, the IScopeDataDevice implementation
        Interop/                        Every ps2000 entry point and the packed structures it takes
  tests/
    libs/
      PicoScope.Brix.ScopeData.Tests/       Mirrors src/libs/PicoScope.Brix.ScopeData
      PicoScope.Brix.ScopeData.Ps2000.Tests/ Mirrors src/libs/PicoScope.Brix.ScopeData.Ps2000
```

The dependency direction is one way, and the interesting part is what is
missing from it. Each head project takes a project reference on
`PicoScope.Brix.Core` and file-links the shared UI by importing
`..\PicoScope.Brix.UI\PicoScope.Brix.UI.projitems` with `Label="Shared"`, so
`App.xaml`, `App.xaml.cs`, `MainPage.xaml` and `MainPage.xaml.cs` are compiled
once into each of the six head assemblies - which is why each head csproj also
has to tell MSBuild to treat `.xaml` files as `Page` items. Because the XAML ends
up inside the head assembly while the view models live in the library, the page
reaches them with an assembly-qualified `clr-namespace`
(`xmlns:vm="clr-namespace:PicoScope.Brix.ViewModels;assembly=PicoScope.Brix.Core"`).

`PicoScope.Brix.Core` references only `PicoScope.Brix.ScopeData`, the contract
library. It never sees `PicoScope.Brix.ScopeData.Ps2000`, and neither does the
view model. Each head references the interop library instead and registers an
instance of it in `Program.Main`, which is what keeps the shared code free of
any device-specific reference and makes the simulator an equal citizen rather
than a fallback bolted on afterwards. `PicoScope.Brix.ScopeData` itself has no
package references at all, which is what stops it from ever growing one.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework itself: `Application`, `Window`, `Frame`, `Page` and the controls on the page, plus `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IXamlRootGetter`, `IHostBuilderProvider`, `CodeBrixPlatformHostBuilder`, `LogExtensionPoint` and `FeatureConfiguration.Font` | `src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`; used throughout `src/PicoScope.Brix.UI/` and `src/PicoScope.Brix.Core/` |
| CodeBrix.Platform.PlotterView add-in | Supplies the `PlotterControl` that renders the chart on every head and gives it pan, zoom and tracker interaction. It brings the CodeBrix.Plotter engine with it, which is where `PlotModel`, the axes, the line series and the colors come from, so nothing here references the plotting engine directly | `src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`, `src/PicoScope.Brix.UI/Views/MainPage.xaml`, `src/PicoScope.Brix.Core/Charting/ScopePlot.cs` |
| CodeBrix.Platform.Fonts.Roboto | Ships the Roboto font that is set as the application-wide default and as the page's `FontFamily`, addressed through an `ms-appx:///` URI, together with the Noto faces registered as script fallbacks | `src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`, `src/PicoScope.Brix.UI/App.xaml`, `src/PicoScope.Brix.UI/App.xaml.cs`, `src/PicoScope.Brix.UI/Views/MainPage.xaml` |
| CodeBrix.Platform runtime for the head | Exactly one runtime package per head - the X11, Wayland, framebuffer, macOS, Win32 and WPF Skia runtimes - and nothing else | the six head csproj files under `src/` |
| SilverAssertions | The assertion style both test projects use | the two test csproj files under `tests/libs/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the dependency-injection container | `src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`, `src/PicoScope.Brix.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider that is wired into the platform's ambient logger in Debug builds, and which the view model then resolves a logger from | `src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`, `src/PicoScope.Brix.UI/App.xaml.cs`, `src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner both test assemblies self-host | the two test csproj files under `tests/libs/` |
| Pico Technology `ps2000` driver | The native driver the interop library binds to. Not a package and not bundled: it is whatever is installed on the machine, located and loaded at run time | `src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/` |

## Worth studying in this application

### Three assemblies, and the seam that keeps them apart

The shape is worth copying whenever an application drives hardware.
`PicoScope.Brix.ScopeData` declares `IScopeDataDevice` - open, close, configure
channels, look up a timebase, run a block, start and stop a stream, drive the
signal generator - together with the model types those calls take and return,
and a `ScopeDeviceFinder` that holds registered implementations and hands back
the best one. It targets plain `net10.0`, has no package references, no interop
and no reference to any UI framework, and its csproj carries a comment saying
so. That is not decoration: it is what makes it impossible for a hardware or
platform dependency to arrive by accident.

`PicoScope.Brix.ScopeData.Ps2000` implements that contract over the real driver.
`SimulatedScopeDataDevice`, in the contract library, implements it over
synthesised signals. Each head registers both in `Program.Main` and the finder
prefers whichever opens:

```csharp
ScopeDeviceFinder.Register(new Ps2000ScopeDataDevice());
ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());
```

`MainViewModel` names neither. It calls `ScopeDeviceFinder.FindBest()` and works
against the interface, which is why the same view model runs identically on a
machine with an instrument and a machine without one, and why the test project
can drive the contract with its own scripted device.

Read `src/libs/PicoScope.Brix.ScopeData/IScopeDataDevice.cs` first, then
`ScopeDeviceFinder.cs`, then a head's `Program.cs`. See
[Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface),
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project),
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend)
and
[Register hardware implementations from each head and ask a finder for the best one](../BLUEPRINTS-AppStructureAndStartup.md#register-hardware-implementations-from-each-head-and-ask-a-finder-for-the-best-one).

### The chart, and why streaming batches need no dispatcher hop

`ScopePlot` owns a `PlotModel` and nothing else does. It builds the axes and the
two line series, converts a `CaptureBlock` into points for a single capture, and
appends streaming batches into a scrolling window whose length is
`StreamWindowSeconds`. Incoming data is decimated to `MaxPointsPerChannel`, so
redraw cost stays flat however fast the samples arrive.

The threading is the part to copy. Streaming batches arrive on the driver's
polling thread, and a plot model is not thread-safe. `ScopePlot` mutates the
model under `PlotModel.SyncRoot`, and the add-in's `PlotterControl` renders under
that same lock, so a batch can be applied on the thread it arrived on with no
marshalling at all. Only the bound properties need the UI thread, and they get it
from `SetProperty(..., notifyOnMainThread: true)` on every setter.

Two sharp edges live here. `PlotModel.Background` has to be set explicitly: the
text and gridline colors are chosen for a dark background, and a model with no
background of its own exports to a white PNG in which the title, legend and grid
are invisible. On screen the control clears to the model's background, so the
problem hides until something exports. And the chart must sit in a bounded cell -
the page gives it a star row inside a `Border` - or the control has no real size
to render into.

Read `src/PicoScope.Brix.Core/Charting/ScopePlot.cs` and then the chart cell in
`src/PicoScope.Brix.UI/Views/MainPage.xaml`. See
[Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on),
[Host a live chart with the PlotterView add-in and bind the model the view model owns](../BLUEPRINTS-ViewsAndControls.md#host-a-live-chart-with-the-plotterview-add-in-and-bind-the-model-the-view-model-owns),
[Mutate a plot model under its own sync root so streamed batches need no dispatcher hop](../BLUEPRINTS-MVVM.md#mutate-a-plot-model-under-its-own-sync-root-so-streamed-batches-need-no-dispatcher-hop)
and
[Decimate incoming samples to a fixed point budget so redraw cost stays flat](../BLUEPRINTS-GraphicsAndRendering.md#decimate-incoming-samples-to-a-fixed-point-budget-so-redraw-cost-stays-flat).

### The view model: startup, commands, and failure as text

`InitializeAsync` is called from the page's `Loaded` handler, not from the
constructor, and it is a guard wrapped around `StartUpAsync`, which is where the
startup work is. A `Loaded` handler is `async void`, so a failure that escaped it
would take the process down instead of reaching the user; here it becomes a line
of status text like every other failure in the class. `StartUpAsync` pushes the
device open onto a worker thread with `Task.Run`, because opening a real
instrument is a second or two of USB traffic the UI thread should not sit
through. Once a device is open it reads the capability set, formats the header
line from it, fills the two bound collections inside `InvokeOnMainThread` - a
combo box cannot show a selection it
does not yet hold, so the selections are fixed on the worker thread first and
re-announced only after the lists contain them - applies the channel settings,
subscribes to `SamplesAvailable`, takes one capture so the chart is never empty,
and starts streaming.

The commands - capture, start streaming, stop streaming, toggle the generator and
flash the LED - are lazily created `SimpleCommand`s whose predicates read two
bound flags, `IsReady` and `IsStreaming`, each carrying `[AffectsCommands]` so a
change to either re-evaluates every button that depends on it. Every failure path is caught as
`PicoScopeException` and turned into a line of `StatusText`; nothing throws at
the user and no dialog is opened. That holds for the two seams the user does not
click as well: `ApplyChannelSettings` runs from the range picker's own setter, so
a range the device refuses reports itself rather than throwing out of a bound
property, and it re-reads `IsStreaming` from the device afterwards so the
buttons still say what is true. The capture command takes the asynchronous
`SimpleCommand` overload through an explicit `(Func<Task>)` cast, which is what
keeps the command from reporting itself finished while the capture is still
running. The view model also logs the same line, so a Debug console run reads as
a narration of what the instrument did.

`Shutdown()` is the counterpart, and it matters more here than in most
applications: a device handle left open keeps the instrument locked against every
other process until this one exits, and a running signal generator keeps running.
The page calls it from `Unloaded` and `App` calls it from the window's `Closed`
event.

Read `src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs` end to end, then
`src/PicoScope.Brix.UI/Views/MainPage.xaml.cs`. See
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread),
[Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch),
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show),
[Release an exclusive device handle from both the page unload and the window close](../BLUEPRINTS-PlatformServices.md#release-an-exclusive-device-handle-from-both-the-page-unload-and-the-window-close)
and
[Guard an async void handler the platform calls](../BLUEPRINTS-MVVM.md#guard-an-async-void-handler-the-platform-calls).

### The two pickers

The waveform picker binds straight to `WaveType` values: the view model exposes
an `ObservableCollection<WaveType>` filled from the device's capability set, and
a two-way property whose setter calls `SetEnumProperty()`. No item template and
no converter, because the member names are exactly the text that should appear.

The range picker cannot do that, because `Range5V` is not how an instrument
labels a range. Rather than reach for a converter or a template, the view model
exposes `VoltageRangeOption`, a sealed two-property class whose `ToString()`
returns `+/-5 V`. A combo box with no item template displays each item's
`ToString()`, so the wrapper is the whole solution and it works identically on
every head.

Both are in `src/PicoScope.Brix.Core/ViewModels/`. See
[Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels)
and
[Fail a bound setter into the status line when a device refuses it](../BLUEPRINTS-MVVM.md#fail-a-bound-setter-into-the-status-line-when-a-device-refuses-it).

### The simulated device

`SimulatedScopeDataDevice` is not a stub. It implements the whole interface,
enforces the same lifecycle rules, and imposes the real instrument's capability
limits, so a capture the hardware would reject is rejected here too and code
developed against it behaves the same way against the physical device. It
synthesises a sine with third-harmonic content on channel A and a slower
phase-shifted companion on channel B, with enough noise to look like a
measurement rather than a formula, and when the signal generator is configured
channel A follows it - as though the generator output were looped back into the
input.

That is what makes this application runnable, testable and demonstrable with no
hardware at all, and it is why the test project can cover the contract's whole
surface without a device. Read
`src/libs/PicoScope.Brix.ScopeData/Simulation/SimulatedScopeDataDevice.cs` and
then `tests/libs/PicoScope.Brix.ScopeData.Tests/SimulatedScopeDataDeviceTests.cs`.
See
[Ship a simulator that enforces the real device's limits rather than a stub](../BLUEPRINTS-Testing.md#ship-a-simulator-that-enforces-the-real-devices-limits-rather-than-a-stub).

### One interop assembly for both operating systems

`Ps2000Api` declares every entry point the driver exports, keeping the C names
exactly as the Programmer's Guide spells them so they can be looked up without a
translation step. The library is named by its bare name, `[DllImport("ps2000")]`,
so the same declaration binds to `ps2000.dll` on Windows and `libps2000.so` on
Linux. The entry points, signatures and behaviors are identical - both are built
from the same header - so the project targets plain `net10.0` rather than
`net10.0-windows`, and one assembly serves both operating systems. The only code
that differs by operating system is where to look for the driver.

`Ps2000DriverLoader` is that code. On Linux the driver package puts the library
where the loader already looks, so nothing is needed; the loader still lists
`/opt/picoscope/lib` for a machine where that configuration was not applied. On
Windows the driver is very often not where the SDK says it should be: the SDK's
`lib` folder may hold only the newest driver while the legacy ones live inside
the desktop application's own folder, which is not on `PATH`. A bare P/Invoke
then throws `DllNotFoundException` while the desktop application is using the
driver happily. The fix is `NativeLibrary.SetDllImportResolver`, which searches
any paths the caller added, then both SDK `lib` folders, then every
`Program Files*\Pico Technology\PicoScope*` directory, and loads by **absolute
path** so that the operating system resolves the driver's own dependency from
beside it. A name-based load would not.

Whatever happened, the instrument itself will tell you: unit-info line 8 is the
full path of the driver that loaded, and the application logs it at startup.

Read `src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000DriverLoader.cs`,
then `Interop/Ps2000Api.cs`. See
[Bind a native driver by its bare name and resolve it yourself at run time](../BLUEPRINTS-ProjectLayoutAndPackaging.md#bind-a-native-driver-by-its-bare-name-and-resolve-it-yourself-at-run-time).

### What the driver does that you would not expect

Every one of these is handled in
`src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs` and
documented next to the code that handles it. They are collected here because
they cost real time to discover.

**Success is 1, failure is 0.** Most `ps2000` entry points return `int16_t`
where zero means failure and any non-zero value means success - the inverse of
the `PICO_STATUS` convention every newer Pico driver uses, where zero means
success. The exceptions return values rather than flags: `ps2000_open_unit`
returns a handle (`>0` success, `0` no device, `-1` found but unusable), the
get-values functions return a sample count, `ps2000_set_ets` returns the
effective interval in picoseconds, and `ps2000_open_unit_progress` returns `1`
complete, `0` pending, `-1` failed.

**The timebase interval is always in nanoseconds.** `ps2000_get_timebase`
returns both a `time_interval` and a `time_units`, and it is natural - and wrong
- to read the interval as being expressed in those units. `time_units` describes
the timestamp array, not the interval. The driver picks the finest timestamp unit
in which the whole capture still fits a 32-bit integer, so the reported unit
drifts from picoseconds to microseconds as the timebase slows. Reading the
interval in the reported unit mis-scales every time axis by a factor of a
thousand.

**Full scale is 32767, not 32512.** The newer `ps2000a` driver uses 32512.
Copying scaling code between the two introduces a silent error of about 0.8% -
small enough to look plausible and never be noticed.

**The streaming callback must be rooted.** `ps2000_get_streaming_last_values`
invokes the callback synchronously before it returns, so the data pointers are
valid only for the duration of the call and must be copied out. The delegate
itself, though, must be kept alive by managed code for as long as streaming runs:
one delegate in a field, not a fresh one per poll. Binding a method group to a
delegate whose signature contains pointers needs an `unsafe` context even though
no pointer is dereferenced at that point, which is why the interop project sets
`AllowUnsafeBlocks`. The overview buffers arrive as `short**`, two per channel -
maximum then minimum - so channel A's maxima are index 0, its minima index 1,
channel B's maxima index 2, and so on.

**Exclusive access.** Only one process may hold a device open, so the desktop
application must be closed; equally, a handle you fail to close keeps the device
locked against every other process until your process exits. That is why
`IScopeDataDevice` implements `IDisposable` as a safety net on top of the
explicit `CloseScope`, and why the page and the window both call `Shutdown()`.

**The signal generator is refused during acquisition.** Every generator call
returns 0 while an acquisition is running, both during fast streaming and during
an in-flight block capture, and the driver gives no reason - the failure is
indistinguishable from a bad argument, so the obvious conclusion, that the
frequency or amplitude was out of range, is wrong. The sequence that works is
stop, configure, restart. `Ps2000ScopeDataDevice` does exactly that inside
`SetSignalGenerator`, `SetArbitraryWaveform` and `StopSignalGenerator`, carrying
the running sample total across so the stream looks continuous to the caller.
There is a brief gap in the data, which is unavoidable.

**The arbitrary-waveform buffer length cancels out.** The DDS phase increment is
`frequency x 2^32 / 48 MHz`. The buffer length sets the waveform's resolution,
not its repetition rate. Multiplying by the buffer length is the obvious thing to
reach for and it is wrong.

**A freshly started stream can report an overrun on its very first status
check.** Treat the first one as noise. See
[Root a native callback delegate for the life of a streaming session](../BLUEPRINTS-MVVM.md#root-a-native-callback-delegate-for-the-life-of-a-streaming-session).

### The instrument this was written against

The capability map below is what a PicoScope 2204A reports, and the code does
not hardcode it: `Ps2000ScopeDataDevice.DiscoverCapabilities()` interrogates
whatever is attached at open time and rebuilds the map from the answers, so a
different 2000-series scope is handled correctly without a change. The numbers
are here because they are useful, and because the simulated device holds itself
to them.

Which driver a scope needs is the first thing to get right, and the naming points
the other way: the "A" in `ps2000a` means second-generation API, not A-suffix
model. A 2204A uses `ps2000`.

| Driver | Covers |
| --- | --- |
| `ps2000` | 2104, 2105, 2202, 2203, 2204, 2204A, 2205, 2205A |
| `ps2000a` | 2205 MSO, 2206/2207/2208 and their A/B variants, 2405A, 2406B, 2407B, 2408B |

Inputs:

| Property | Value |
| --- | --- |
| Channels | A and B only; C, D and the external trigger input are rejected |
| Resolution | 8-bit converter, always scaled to 16 bits by the driver |
| Full scale | +/-32767 counts |
| "No data" sentinel | -32768, distinct from -32767 |
| Ranges | +/-50 mV to +/-20 V, nine of the twelve the enumeration defines |
| Rejected ranges | +/-10 mV, +/-20 mV, +/-50 V |
| Coupling | AC and DC |

Timing:

| Property | Value |
| --- | --- |
| Timebase range | 0 to 23; 24 and above are rejected |
| Timebase 0 | Valid only when exactly one channel is enabled |
| Timebase 1 | The fastest available with both channels on |
| Sampling interval | `10 x 2^timebase` nanoseconds, 10 nanoseconds to 83.9 milliseconds |
| Fastest sampling | 100 MS/s with one channel, 50 MS/s with two |
| Block buffer | 8064 samples with one channel, 3968 with two |
| Maximum oversample | 4. The header advertises 256 for the series; this model rejects 8 and above |

Disabling a channel is therefore not cosmetic: it unlocks timebase 0 and doubles
the block buffer. Oversampling divides the buffer rather than extending it.

Triggering, sampling modes and generator:

| Property | Value |
| --- | --- |
| Simple trigger sources | Channel A, channel B, none. There is no external trigger input |
| Directions | Rising, falling |
| Advanced triggering | Supported: channel properties, conditions, directions, delay |
| Pulse-width qualifier | Supported |
| Window comparison and hysteresis | Supported, through the channel's threshold mode |
| Equivalent-time sampling | Supported in both fast and slow modes; 500 ps effective interval at 250 cycles and 40 interleave |
| Compatible streaming | Supported; millisecond intervals, polled, capped at 60000 samples |
| Fast streaming | Supported; sub-microsecond intervals, callback-driven, millions of samples |
| Generator waveforms | All nine: sine, square, triangle, ramp up, ramp down, DC, gaussian, sinc, half sine |
| Generator frequency | 0.1 Hz to 100 kHz; 200 kHz is rejected |
| Generator amplitude | Up to 4 V peak to peak; 8 Vpp is rejected |
| Generator sweeps | Supported: up, down, up-down, down-up |
| Arbitrary waveform | Up to 4096 8-bit samples; 8192 is rejected. 48 MHz clock, 32-bit phase accumulator |
| `flash_led` | Supported |
| `set_led`, `set_light`, `last_button_press` | Not supported on this model |

Equivalent-time sampling reconstructs a repetitive waveform far below the real
10 ns floor by capturing it many times with deliberate sub-sample offsets and
interleaving the results. On a single-shot event it produces nonsense, because it
is stitching separate captures together.

Two ordinal sets are worth knowing because they are not what a reader of the
newer driver would assume. The accepted voltage ranges are not contiguous from
zero, so the enumeration keeps all twelve and the capability set says which nine
are usable. And the `ps2000` wave-type ordinals differ from `ps2000a`: here DC is
5 and gaussian is 6, where the newer driver places them at 8 and 6.

### When something goes wrong

| Symptom | Cause |
| --- | --- |
| `DllNotFoundException` on Windows | The driver is not on the search path; see the driver-loader section above |
| `DllNotFoundException` on Linux | `libps2000` is not installed, or `/opt/picoscope/lib` is not in the loader path - check with `ldconfig -p` |
| `BadImageFormatException` | A 32-bit process against a 64-bit driver |
| Opening returns no device | No device, or the PicoScope desktop application has it open |
| Opening reports a device it cannot use | Often a half-closed handle from a previous run; the lock clears when that process exits |
| Opening returns no device on Linux with the scope plugged in | The udev rule is missing, so the USB device is not writable; reinstalling `libps2000` restores it |
| A timebase lookup fails | The timebase is invalid for the current channel count or oversample: timebase 0 needs a single channel, and oversample above 4 is rejected |
| The time axis is out by a factor of a thousand | The interval was read in the reported `time_units` instead of nanoseconds |
| Voltages are out by about 0.8% | Scaling with 32512 instead of 32767 |
| Configuring a channel fails | An unsupported channel or range for the model |
| The streaming callback never fires | The delegate was collected; root it in a field |
| Buffer overruns are reported constantly | The overview buffer is too small, or polling is too slow |
| A generator call fails | An acquisition is running; or the amplitude exceeds 4 Vpp, the frequency exceeds 100 kHz, or an arbitrary buffer is longer than 4096 samples |

### What this application does not show

It is a focused application, so come to it for the device layer, the chart and
the six-head layout, and go elsewhere for the rest. It has:

- No registered services. The registration lambda in `App.xaml.cs` is a comment,
  and the device implementations are registered with a static finder rather than
  resolved from the container, so the wiring is shown but no resolution is.
- No dialogs. The page wires an `IXamlRootGetter` so that a view model could open
  one, and it is worth reading as the graceful-degradation pattern, but this view
  model reports everything through the status line instead.
- No settings, persistence or window-state memory. The range, waveform and
  frequency are not remembered between runs, and nothing captured is saved.
- No export. The chart is live only; there is no PNG, CSV or session file, even
  though the plotting engine can export.
- No second page and no navigation beyond the initial one, no converters, no
  custom controls, no styles or templates, and no theming beyond the colors
  written into the page and the plot model.
- No cancellation or progress reporting. Captures are short enough not to need
  either, and streaming is stopped by a command rather than a token.
- No coverage of the contract's advanced surface from the UI. Advanced
  triggering, the pulse-width qualifier, equivalent-time sampling, the arbitrary
  waveform generator and compatible streaming are all implemented, documented and
  reachable from code, but no control on the page drives them.
- No test coverage of the driver bindings themselves. They cannot be exercised
  without an attached instrument, so the interop library's test project exists to
  mirror the library rather than to test the driver.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder is the record for this application: the
code dependencies arrive as NuGet packages that carry their own licenses and
notices, and the one body of third-party work this application derives from,
together with the terms of the native oscilloscope driver it loads at run time
but never bundles, is set out there.

## License

PicoScope.Brix is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
