# DRAKON.Brix

DRAKON.Brix is the DRAKON Editor, running as a CodeBrix.Platform desktop
application. DRAKON is a visual algorithm language: an algorithm is drawn as a
diagram built from a fixed icon vocabulary - action, question, loop, insertion,
case, address, comment - laid out on a strict skewer-and-silhouette grid, and the
editor turns that diagram into real source code in any of the target languages it
ships a generator for. The editor keeps its diagrams in a single-file project on
disk, shows them in a tree beside the drawing canvas, and offers the usual
editing, printing and export commands from a menu bar. All of that is the
editor's own Tcl/Tk program, running unmodified on a managed Tcl interpreter and
a managed Tk toolkit and drawing into one element on one otherwise empty XAML
page. The application contributes no menus, no canvas and no dialogs of its own:
what you see in the window is the guest program's own user interface.

It is this repository's reference for hosting a complete, unmodified third-party
Tcl/Tk program inside a CodeBrix.Platform application: the boot sequence that
creates the interpreter and registers the toolkit and the shims, the two-thread
model that lets a synchronous Tcl program share a process with an asynchronous UI
thread, the small set of Tcl commands the host adds so the guest's own quit path
and diagnostics work, the bootstrap script that supplies the environment the
guest expects, and a test suite that boots the same program headlessly and drives
its real file-open path over a large corpus of real documents. Everything the
application itself contributes is a handful of small C# and Tcl files; everything
else is the guest.

## What this sample shows a CodeBrix.Platform developer

- How a library under `src/libs` that also sees CodeBrix.Platform keeps its own
  root namespace so the per-head generated resources type does not collide:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- How one package reference brings a whole stack with it, and when to name the
  parts you actually depend on:
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Which project each package belongs on: the framework, the toolkit, the shims
  and the font on the Core library, exactly one runtime package on each head:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- How an application is laid out as a shared UI project, a Core project, a
  library under `src/libs` and its mirror under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- How `App.xaml` and the `Views` folder are compiled into all six executables
  through a shared items project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Why the Core library's `RootNamespace` is set to the application name, and what
  the shared XAML then writes to reach the view models:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- How one Windows-targeting head stays inside a solution that restores on Linux
  and macOS:
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- How a folder of committed content becomes a folder beside the binaries that the
  running program can read, and how the notices file records what is in it:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- What a head's `Program.Main` contains and how the backend is selected:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- The four things the `App` constructor does, in the order they have to happen:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- How `OnLaunched` creates the window, puts a `Frame` in it and navigates to the
  one page:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- How to give `SimpleServiceResolver` a generic-host builder from the shared Core
  library instead of duplicating it in six heads:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Why a view model constructed by the XAML designer needs a guard, and what it is
  paired with at startup:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- The one-handler code-behind that hands the view model a `XamlRoot` getter, kept
  even in an application whose page is one hosted surface:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- How a font that ships inside a package becomes the application-wide default:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- How to get console diagnostics while developing and a silent Release build:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- The one per-head behavioral difference in the whole application, applied after
  `Build()` and before `Run()`:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- How the test project is shaped so the family's runner discovers its tests:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- How the library keeps its runtime type internal and still lets its tests drive
  it, through `InternalsVisibleTo.cs` and a documented internal test accessor:
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Why a test project that boots a real graphics toolkit has to reference the
  native asset a head would otherwise have supplied:
  [Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied).
- How a page that contributes no controls of its own hosts an entire guest
  program's user interface in one element:
  [Host an unmodified guest program in one page element](../BLUEPRINTS-ViewsAndControls.md#host-an-unmodified-guest-program-in-one-page-element).
- How one boot sequence serves both the running application and a headless test
  suite, with the marshalling as the only difference:
  [Run one boot sequence hosted on a thread and inline for tests](../BLUEPRINTS-Testing.md#run-one-boot-sequence-hosted-on-a-thread-and-inline-for-tests).
- How a synchronous guest shares a process with an asynchronous UI thread without
  either one touching the other's state:
  [Keep an embedded interpreter on its own thread and post every call to it](../BLUEPRINTS-PlatformServices.md#keep-an-embedded-interpreter-on-its-own-thread-and-post-every-call-to-it).
- How the guest's own "end the program" path is re-implemented in terms of the
  host, and made harmless in a test run:
  [Inject the quit action so a guest exit cannot end the test host](../BLUEPRINTS-PlatformServices.md#inject-the-quit-action-so-a-guest-exit-cannot-end-the-test-host).
- How the application extends the guest's language with commands of its own,
  without hard-coding what they do:
  [Add your own commands to an embedded interpreter](../BLUEPRINTS-PlatformServices.md#add-your-own-commands-to-an-embedded-interpreter).
- How a glue script supplies the packages, globals and built-ins a guest written
  for a command-line launcher assumes:
  [Give a hosted guest program the environment it assumes with a bootstrap script](../BLUEPRINTS-AppStructureAndStartup.md#give-a-hosted-guest-program-the-environment-it-assumes-with-a-bootstrap-script).
- How a committed folder of guest files becomes a folder beside the binaries, and
  the one lookup that finds it on every head:
  [Copy a guest program tree beside the binaries and read it from there](../BLUEPRINTS-ProjectLayoutAndPackaging.md#copy-a-guest-program-tree-beside-the-binaries-and-read-it-from-there).
- How a suite drives real documents through the real code path without ever
  writing to the committed originals:
  [Copy a gold master fixture before a test that writes to it](../BLUEPRINTS-Testing.md#copy-a-gold-master-fixture-before-a-test-that-writes-to-it).
- How the live application is driven from an environment variable and the answer
  read in the log, with no mouse and no second build:
  [Drive a running application from an environment variable and report to the log](../BLUEPRINTS-Testing.md#drive-a-running-application-from-an-environment-variable-and-report-to-the-log).
- How a library keeps its public surface to one type with two members, with the
  tests reaching past it through the internals attribute:
  [Keep a library's public surface to the one type its host drives](../BLUEPRINTS-ProjectLayoutAndPackaging.md#keep-a-librarys-public-surface-to-the-one-type-its-host-drives).

## Building, running and testing

There is one solution, `DRAKON.Brix.slnx`, and its own comment says what it is:
everything that builds with the plain .NET SDK on Linux, macOS and Windows. It
holds the shared UI project, the Core project, all six heads, the one library
under a `Libraries` solution folder and the one test project under a `Tests`
solution folder. There is no second, Windows-only solution, because every head
here is a Skia head.

The heads:

| Project | Platform |
| --- | --- |
| `src/DRAKON.Brix.LinuxX11` | Linux, X11 |
| `src/DRAKON.Brix.LinuxWayland` | Linux, Wayland |
| `src/DRAKON.Brix.LinuxFrameBuffer` | Linux, framebuffer (no display server) |
| `src/DRAKON.Brix.MacOS` | macOS |
| `src/DRAKON.Brix.Win32Skia` | Windows, Win32 window |
| `src/DRAKON.Brix.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting` so the solution still
  restores on Linux and macOS. No workload is needed.
- Nothing else. There is no Tcl or Tk installation anywhere in the picture: the
  interpreter, the toolkit, the message catalog, the SQLite command and the PDF
  command are all managed code arriving as packages, and the editor's own program
  tree is committed in this folder and copied beside the binaries when you build.
  No accounts, tokens, downloads or user-supplied data files are involved.
- All CodeBrix packages come from NuGet. No CodeBrix library is referenced as a
  source project, so this folder builds on its own.

To run one head from the command line, from this folder:

```text
dotnet run --project src/DRAKON.Brix.LinuxX11
dotnet run --project src/DRAKON.Brix.LinuxWayland
dotnet run --project src/DRAKON.Brix.LinuxFrameBuffer
dotnet run --project src/DRAKON.Brix.MacOS
dotnet run --project src/DRAKON.Brix.Win32Skia
dotnet run --project src/DRAKON.Brix.WinWpfSkia
```

The window opens empty for a moment and then fills, because the editor's user
interface is created by the guest program on its own thread rather than by the
XAML page. Use `File` > `Open` inside it and point it at one of the diagrams
under `examples/`; the editor can also be started with a diagram already open,
which the bootstrap script arranges by synthesizing the command-line globals that
a shell-launched Tcl program would have been given. Console logging is compiled
in only for Debug builds - the body of `App.InitializeLogging()` is inside
`#if DEBUG` - so a Release run is silent apart from the runtime's own diagnostic
lines.

There is one test project, `tests/libs/DRAKON.Brix.TclBridge.Tests`. It uses
xUnit v3 with SilverAssertions, builds as `Exe` and sets
`UseMicrosoftTestingPlatformRunner`, so the test assembly is a self-executing
binary; this folder has no `global.json`, and the runner selection lives in the
test project file instead. That matters in practice: a plain `dotnet test` can
report that it discovered zero tests. When it does, build the test project and
run the produced executable directly:

```text
dotnet build tests/libs/DRAKON.Brix.TclBridge.Tests/DRAKON.Brix.TclBridge.Tests.csproj -c Release
./tests/libs/DRAKON.Brix.TclBridge.Tests/bin/Release/net10.0/DRAKON.Brix.TclBridge.Tests
```

What the test project needs:

| Test project | Covers | Needs |
| --- | --- | --- |
| `tests/libs/DRAKON.Brix.TclBridge.Tests` | Constructing and disposing the runtime without starting it, and opening every committed example diagram through the editor's real open path on a headless boot | Native Skia, referenced directly by the test project because no head is present to supply it; no display server, no GPU and no network |

The suite takes a couple of minutes, because each case boots the whole editor
from scratch and then opens a real document through the editor's own code. It is
configured to run strictly sequentially, in `xunit.runner.json`, because the
interpreter keeps process-global state and concurrent interpreters race.

## How the projects and folders are organized

```text
DRAKON.Brix/
  DRAKON.Brix.slnx                      The one solution; every project; opens on Linux, macOS and Windows
  THIRD-PARTY-NOTICES.txt               Third-party content bundled with, or used by, this application
  art/                                  The editor's icon source artwork
  docs/                                 The editor's own documentation, including the diagram file-format description
  examples/                             The editor's own example diagrams, per target language, with their generated source
  src/
    DRAKON.Brix.UI/                     Shared items project: the XAML that every head compiles
      DRAKON.Brix.UI.shproj             Shared-project shell, so an IDE can load the folder as a project
      DRAKON.Brix.UI.projitems          The shared file list each head imports with Label="Shared"
      App.xaml                          Merged WinUI resources and the Open Sans FontFamily resource
      App.xaml.cs                       Bootstrap: default font, service resolver, design mode, window and frame, logging
      Views/MainPage.xaml               The whole page: one Grid holding one Tk host element
      Views/MainPage.xaml.cs            Thin code-behind: the XamlRoot getter, and the runtime's start and dispose
    DRAKON.Brix.Core/                   Class library; carries every non-head package
      DRAKON.Brix.Core.csproj           RootNamespace DRAKON.Brix; framework, toolkit, shim, font, hosting and logging packages; the asset copy rules
      Assets/bootstrap.tcl              The glue script that gives the guest program the environment it expects
      Assets/drakon/                    The editor's own program tree, copied beside the binaries at build time
      Helpers/HostHelper.cs             The IHostBuilderProvider that SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs       The page's view model; deliberately empty, because the guest owns the UI
    DRAKON.Brix.LinuxX11/               Head: Program.cs plus a csproj with one runtime package
    DRAKON.Brix.LinuxWayland/           Head: Program.cs plus a csproj with one runtime package
    DRAKON.Brix.LinuxFrameBuffer/       Head: Program.cs plus a csproj with one runtime package
    DRAKON.Brix.MacOS/                  Head: Program.cs plus a csproj with one runtime package
    DRAKON.Brix.Win32Skia/              Head: Program.cs plus a csproj with one runtime package
    DRAKON.Brix.WinWpfSkia/             Same, plus net10.0-windows and a software render surface
    libs/
      DRAKON.Brix.TclBridge/            The "talk to Tcl" layer; the only project that knows the guest exists
        DrakonRuntime.cs                The boot sequence, both boot modes, and the diagnostic sink
        RuntimeHost.cs                  The UI-facing owner: Start(host) and Dispose()
        InternalsVisibleTo.cs           Names only this library's own test assembly
        Commands/QuitCommand.cs         The Tcl command the guest's quit path routes through
        Commands/DiagnosticReportCommand.cs  The Tcl command that routes text to the diagnostic sink
  tests/
    libs/
      DRAKON.Brix.TclBridge.Tests/      Mirrors src/libs/DRAKON.Brix.TclBridge
        Support/SampleLocations.cs      Finds this folder from the test binary, by walking up to the solution file
        xunit.runner.json               Turns off parallelism, because interpreters must not race
```

The dependency direction is one way. Each head project takes a project reference
on `DRAKON.Brix.Core` and file-links the shared UI by importing
`..\DRAKON.Brix.UI\DRAKON.Brix.UI.projitems` with `Label="Shared"`, so `App.xaml`,
`App.xaml.cs`, `MainPage.xaml` and `MainPage.xaml.cs` are compiled once into each
of the six head assemblies. That is why each head csproj also has to tell MSBuild
to treat `.xaml` files as `Page` items. `DRAKON.Brix.Core` references the one
library, so the heads pick the bridge up transitively, and carries every package
the application shares. The library references nothing in the application.

Two things about the library are worth reading before you copy the layout. It
keeps its default root namespace rather than the application's, and its project
file says why: the XAML source generator that arrives with the toolkit emits a
generated resources type into each project's root namespace, and two projects
claiming the same one make the head that references both fail to compile. And its
C# files declare an explicit namespace of their own, which is neither the project
default nor the application's, so the assembly name, the root namespace and the
code namespace are three different strings on purpose.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework itself: `Application`, `Window`, `Frame`, `Page` and the `Grid` the host sits in, plus `SimpleViewModel`, `SimpleServiceResolver`, `IXamlRootGetter`, `IHostBuilderProvider`, `CodeBrixPlatformHostBuilder` and `FeatureConfiguration.Font` | `src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj`; used throughout `src/DRAKON.Brix.UI/` |
| CodeBrix.Platform.TkCanvas | The managed Tk toolkit: `TkHostView` (the XAML element the whole guest UI draws into), `TkBootstrap` (which registers the Tk command surface on an interpreter), `TkTclBridge` (the Tcl-to-toolkit command bridge, in hosted or direct form), `TkWindow`, `WindowTree` and the file-dialog adapter | `src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`, `src/libs/DRAKON.Brix.TclBridge/RuntimeHost.cs`, `src/DRAKON.Brix.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.TclTk | The managed Tcl interpreter the guest program runs on: `Interpreter`, `Result`, `ReturnCode`, the command base types the application's two Tcl commands derive from, and the interpreter tuning switches. It arrives transitively with the toolkit | `src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`, `src/libs/DRAKON.Brix.TclBridge/Commands/` |
| CodeBrix.Platform.TclTk.Extras | The extra Tcl commands the guest requires and the base engine does not have: a `sqlite3` command, which is how the editor reads and writes its documents, and a `pdf4tcl` command surface, which is how it exports PDFs. `TclTkExtras.RegisterAll` installs both | `src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs` |
| CodeBrix.Platform.SkiaSharp.Views | The Skia surface the toolkit draws the whole guest user interface onto. It arrives transitively with the toolkit, so nothing here declares it and the whole application resolves one Skia version | `src/DRAKON.Brix.UI/Views/MainPage.xaml`, `src/libs/DRAKON.Brix.TclBridge/DRAKON.Brix.TclBridge.csproj` |
| CodeBrix.Platform.Fonts.OpenSans | Ships the Open Sans font that is set as the application-wide default and as the page's `FontFamily`, addressed through an `ms-appx:///` URI | `src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj`, `src/DRAKON.Brix.UI/App.xaml`, `src/DRAKON.Brix.UI/App.xaml.cs`, `src/DRAKON.Brix.UI/Views/MainPage.xaml` |
| CodeBrix.Platform runtime for the head | Exactly one runtime package per head - the X11, Wayland, framebuffer, macOS, Win32 and WPF Skia runtimes - and nothing else | the six head csproj files under `src/` |
| SilverAssertions | The assertion style in the test project | `tests/libs/DRAKON.Brix.TclBridge.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the dependency-injection container | `src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj`, `src/DRAKON.Brix.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider that is wired into the platform's ambient logger in Debug builds | `src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj`, `src/DRAKON.Brix.UI/App.xaml.cs` |
| SkiaSharp native assets for Linux | The native Skia library the test project needs, because it boots the toolkit with no head present to supply it | `tests/libs/DRAKON.Brix.TclBridge.Tests/DRAKON.Brix.TclBridge.Tests.csproj` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner | `tests/libs/DRAKON.Brix.TclBridge.Tests/DRAKON.Brix.TclBridge.Tests.csproj` |

## Worth studying in this application

### A page that is one hosted surface, and a view model that stays empty

`MainPage.xaml` is worth reading precisely because there is so little of it: a
`Grid` with a single `TkHostView` in it, an `x:Name` so the code-behind can reach
it, and the same root-element namespace declarations every page in this repository
uses, with the toolkit's hosting namespace added in the same assembly-qualified
form. The page declares a `MainViewModel` as its `DataContext` and binds nothing
to it, because the guest program owns every pixel below the host element. The view
model is a `SimpleViewModel` with the design-mode guard as its first constructor
line and two empty regions, which is exactly what the template generates; keeping
it rather than deleting it is deliberate, since it is where application-side state
would go the moment the application grew a real control of its own.

The code-behind is where the application's real work with the page happens, and it
is six lines. It subscribes `DataContextChanged` to hand the view model a
`XamlRoot` getter, it starts the runtime on `Loaded` and disposes it on `Unloaded`,
and it calls `InitializeComponent()` last. The `Loaded` event, not the constructor,
is the right moment: the host element's window tree and its dispatcher only exist
once the page has been loaded, and the runtime needs both. Read
`src/DRAKON.Brix.UI/Views/MainPage.xaml` and then
`src/DRAKON.Brix.UI/Views/MainPage.xaml.cs`. See
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer)
and
[Host an unmodified guest program in one page element](../BLUEPRINTS-ViewsAndControls.md#host-an-unmodified-guest-program-in-one-page-element).

### The two-class split: what the page owns and what the runtime owns

The bridge library exposes exactly one public type, `RuntimeHost`, and it has two
members: `Start(TkHostView)` and `Dispose()`. Everything else - the interpreter,
the toolkit bridge, the boot sequence, the diagnostic sink - is `internal` on
`DrakonRuntime`. That split is the reason the code-behind can be six lines and the
reason the test project can drive the same boot sequence a completely different
way: the page holds a `RuntimeHost` and knows nothing about interpreters, while
the tests bypass `RuntimeHost` entirely and speak to `DrakonRuntime` through
`InternalsVisibleTo`.

Both classes are written so that double calls are harmless. `RuntimeHost.Start()`
returns immediately if it already has a runtime; `Dispose()` reads the field into
a local, nulls the field and only then disposes, so a second call or a concurrent
one cannot double-dispose. `DrakonRuntime` has its own `_started` flag doing the
same job one level down. In an application where teardown can be started by the
guest program, by the window closing, or by a page unload, that belt-and-braces
guarding is cheap and worth copying. Read
`src/libs/DRAKON.Brix.TclBridge/RuntimeHost.cs`, which is short, before
`src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`. See
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Host an unmodified guest program in one page element](../BLUEPRINTS-ViewsAndControls.md#host-an-unmodified-guest-program-in-one-page-element)
and
[Keep a library's public surface to the one type its host drives](../BLUEPRINTS-ProjectLayoutAndPackaging.md#keep-a-librarys-public-surface-to-the-one-type-its-host-drives).

### One boot sequence, two ways of running it

`DrakonRuntime.Boot()` is the whole of the application's integration work, and it
runs in a fixed order that is worth learning as an order: create the interpreter;
set the two performance switches on it; register the Tk command surface; register
the extra command shims; create the Tcl-to-toolkit bridge; add the application's
own Tcl commands; source the bootstrap script; source the guest program. Each step
that can fail is checked and reported rather than thrown, because by the time the
later steps run there is no caller left to catch anything.

What makes the class instructive is that the same sequence serves two very
different callers. `Start(TkHostView)` is HOSTED mode, which is how the application
runs: the bridge is created in its hosted form, the boot itself is pushed onto a
background task, and every later interaction with the interpreter is posted to the
Tcl thread through the bridge. `StartDirect(assetsDirectory)` is DIRECT mode, which
is how the tests run: a headless root window is created with a forced size, the
bridge is created in its direct form, and everything runs inline on the calling
thread. The single parameter that differs between them is captured as a
`dispatch` delegate - post to the Tcl thread, or invoke inline - so the body below
it is written once. Hosted mode also gives the bridge a file-dialog adapter, so
the guest's own open and save commands raise the platform's native dialogs;
direct mode leaves it unset, because a headless run has nobody to show a dialog
to. The payoff is the one that matters for a test suite: the Tcl that executes is
identical in both modes, so a test that opens a document is exercising the
application's real open path and not a test-only approximation.
Read `src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs` from `Start` down through
`Boot`.

Sharp edges met here. The interpreter and the toolkit must be registered before
the bridge, and the application's commands before any script that calls them.
Sourcing is two steps and the order is load-bearing: the bootstrap script has to
have finished before the guest program's first line runs. And the assets directory
is resolved from `AppContext.BaseDirectory`, not from a path relative to the
project, which is what makes the copied-to-output asset tree the single thing the
runtime reads at run time. See
[Run one boot sequence hosted on a thread and inline for tests](../BLUEPRINTS-Testing.md#run-one-boot-sequence-hosted-on-a-thread-and-inline-for-tests).

### A synchronous guest program and an asynchronous UI thread

The thread model is the hardest part of hosting a program like this, and the
application solves it with one rule: the interpreter lives on its own thread, and
the toolkit's hosted bridge marshals every Tk command from that thread to the UI
thread. Nothing in the application ever touches the interpreter from the UI thread
except through `bridge.Post`, and nothing ever touches the visual tree from the Tcl
thread at all. `Start()` captures the host's window tree on the UI thread, hands it
to the bridge, and then does all of its own work inside `Task.Run`.

The consequence worth knowing before you try this is the quit path. A program
written for a shell interpreter ends itself by calling `exit`, which terminates the
process. The managed engine's `exit` only marks the interpreter as exited, and in a
hosted application the UI thread happily keeps the process alive afterwards, so the
guest's `File` > `Quit` would appear to hang. The application fixes that with a Tcl
command of its own and a one-line shadowing proc in the bootstrap script, described
below. The general lesson generalizes to any embedded interpreter: the guest's
notion of "the program ends now" has to be re-implemented in terms of the host's.
See
[Keep an embedded interpreter on its own thread and post every call to it](../BLUEPRINTS-PlatformServices.md#keep-an-embedded-interpreter-on-its-own-thread-and-post-every-call-to-it).

### The two Tcl commands the application adds

`Commands/QuitCommand.cs` and `Commands/DiagnosticReportCommand.cs` are the only
places the application extends the guest's language, and they are both about
twenty lines. Each derives from the engine's command base type, declares its name
and metadata in a `CommandData`, and implements one `Execute` method that reads its
arguments out of an `ArgumentList` and writes a `Result`. Both are `internal`,
because nothing outside the library should be adding commands.

The design detail to copy is that neither command hard-codes what it does. The quit
command takes an `Action<int>` and the report command takes an `Action<string>`, so
the same command class serves the application, which passes a process-ending action
and a console-and-event sink, and the tests, which pass a no-op. That is what makes
a stray `exit` in a guest script harmless in a test host instead of tearing the
test runner down, and it costs one constructor parameter. The commands also show
the small mechanical things: parse defensively, because a script can call a command
with any arity; always set a result, even an empty one; and return the engine's
`ReturnCode.Ok` rather than throwing, because the caller is a script. See
[Add your own commands to an embedded interpreter](../BLUEPRINTS-PlatformServices.md#add-your-own-commands-to-an-embedded-interpreter)
and
[Inject the quit action so a guest exit cannot end the test host](../BLUEPRINTS-PlatformServices.md#inject-the-quit-action-so-a-guest-exit-cannot-end-the-test-host).

### The bootstrap script: giving a guest the environment it assumes

`Assets/bootstrap.tcl` is the only Tcl the application wrote, and it is the file to
read if you ever host someone else's script program. It exists because a program
written for a standard interpreter assumes things that a hosted interpreter does
not provide: packages that are present, global variables that a launcher filled in,
and built-in commands that behave a particular way. Each block in it is tagged with
the reason it exists and quotes the stock mechanism it stands in for, so the file
doubles as the list of everything that had to be adapted.

Three kinds of adaptation show up there, and they are the three you should expect.
Some packages only have to be *present*, because their only real consumer has been
replaced by a managed shim, so an empty `package provide` satisfies the guest's
`package require` gate and keeps a large dependency out of the application
entirely. Some have to be genuinely re-implemented, and the message catalog is the
one done properly here - the locale preference list, the catalog loading, the
fall-back-to-source-string lookup - because the guest ships real translations and
an English-only stub would have thrown them away. And some are host facts rather
than packages: the globals a shell launcher would have set from the command line
are synthesized here, which is also how the application can start with a document
already open.

The fourth block is the quit shadow: a four-line `proc exit` that forwards to the
application's own command. Read `src/DRAKON.Brix.Core/Assets/bootstrap.tcl`
top to bottom; it is the shortest complete tour of what hosting costs. See
[Give a hosted guest program the environment it assumes with a bootstrap script](../BLUEPRINTS-AppStructureAndStartup.md#give-a-hosted-guest-program-the-environment-it-assumes-with-a-bootstrap-script).

### Making a large interpreted program fast enough

Two lines in `Boot()` do more for the running application than anything else in the
file, and both are documented in place with what they buy and what they cost.
Caching parsed scripts makes the interpreter tokenize each procedure body once
instead of on every execution, which matters enormously for a program that redraws
a diagram by re-running the same bodies thousands of times. Production mode skips
optional per-command engine bookkeeping; results are unchanged, and the documented
trade-off is that script cancellation can no longer interrupt a running script
promptly, which is acceptable only because this guest never cancels scripts. The
comment says so, and says to remove the line if that ever changes.

That is the pattern worth taking away, more than the two specific switches: when
you turn on a performance option that trades away a capability, write down which
capability, why this program does not need it, and what would make the trade
invalid. See
[Run one boot sequence hosted on a thread and inline for tests](../BLUEPRINTS-Testing.md#run-one-boot-sequence-hosted-on-a-thread-and-inline-for-tests).

### The asset tree, and the one project-file rule that makes it work

The guest program is a folder of files, not a package, so the Core project copies
it: one `None` item for the bootstrap script and one recursive `None` glob for the
program tree, both with `CopyToOutputDirectory="PreserveNewest"`. That is the whole
mechanism, and the runtime's side of the contract is the `AppContext.BaseDirectory`
lookup described above. Keeping the tree in the Core project rather than in the
library that reads it is deliberate: the library is about talking to an
interpreter, and it should be usable against an asset tree anywhere, which is
exactly what its boot method's directory parameter provides.

The copy costs build time and output size, so glob only what the guest actually
needs at run time. In this application the diagrams, documentation and artwork
folders at the root of the application are *not* copied to the output, because the
running program never reads them; they are here to be opened by a person, and by
the test suite, which copies them out one at a time. See
[Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file)
and
[Copy a guest program tree beside the binaries and read it from there](../BLUEPRINTS-ProjectLayoutAndPackaging.md#copy-a-guest-program-tree-beside-the-binaries-and-read-it-from-there).

### Tests that boot the whole guest and open real documents

The test project is small and the ambition is not. One test constructs the runtime
and disposes it without ever starting it, which is the cheapest possible guard
against a constructor acquiring something. The rest are a theory whose cases name
every committed example diagram, and each case boots the editor headlessly through
the direct-mode path and then evaluates the editor's own open procedure against a
real file, asserting both that the editor reported itself up and that the open
returned success. Because direct mode and hosted mode share `Boot()`, a passing
case means the application's real open path works on that document.

Two rules in that suite are worth stealing outright. The first is the copy rule:
the committed diagrams are gold masters and are never opened in place, because
opening one runs a schema upgrade that writes to the file and can leave a database
journal beside it. Every case copies its file into a temp directory it owns and
deletes the directory in a `finally`. The second is locating the fixtures: rather
than a chain of `..` segments from the test binary, `Support/SampleLocations.cs`
walks up from `AppContext.BaseDirectory` until it finds the solution file and
derives every path from there, so the suite survives a change of configuration,
target framework or output layout.

What a test host cannot reach at all - a layout that is only wrong on screen - has a
hook of its own instead. When an environment variable is set, the hosted start posts
a short script onto the interpreter's own thread once the boot sequence has returned,
and the answer comes back through the same diagnostic sink every startup failure uses,
so a live-only question is asked and answered in the log with no mouse and no second
build. Nothing about the application changes when the variable is not set.

The project file has two things to notice. It references the native Skia asset
directly, because a test host has no head to supply it, and it turns parallelism
off in `xunit.runner.json` because interpreters keep process-global state. Read
`tests/libs/DRAKON.Brix.TclBridge.Tests/DrnFileOpenTests.cs` and then
`tests/libs/DRAKON.Brix.TclBridge.Tests/Support/SampleLocations.cs`. See
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied),
[Copy a gold master fixture before a test that writes to it](../BLUEPRINTS-Testing.md#copy-a-gold-master-fixture-before-a-test-that-writes-to-it)
and
[Drive a running application from an environment variable and report to the log](../BLUEPRINTS-Testing.md#drive-a-running-application-from-an-environment-variable-and-report-to-the-log).

### The six-head skeleton around it all

Everything outside the bridge library is the plain six-head skeleton, unchanged.
If you are here for the project layout rather than for interpreter hosting, read it
in this order: a head's `Program.cs` (they are identical apart from the call that
names the backend, and the WinWpfSkia head's extra render-surface block), then that
head's csproj for the `Page` glob, the `Label="Shared"` import, the project
reference and its single runtime package, then
`src/DRAKON.Brix.UI/DRAKON.Brix.UI.projitems` for what the shared project
contributes, then `src/DRAKON.Brix.UI/App.xaml.cs` for the startup sequence, and
finally `src/DRAKON.Brix.Core/Helpers/HostHelper.cs`.

The sharp edges are mostly in the build files. New XAML pages have to be added to
the `.projitems` by hand, as a `Page` with `Generator MSBuild:Compile` and as a
`Compile` with `DependentUpon` its `.xaml`; the shared project has no globbing. The
`.shproj` `ProjectGuid` and the `.projitems` `SharedGUID` must match. The
`<None Remove="**\*.xaml" />` beside each head's `Page` glob is required, or the
same files are both content and pages. And `SetIsDesignMode(false)` in the `App`
constructor is not optional: without it every view model still believes it is in
the designer at run time and returns from its constructor early. See
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
[Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
and
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### What this application does not show

It is a single-purpose application, so come to it for interpreter hosting and go
elsewhere for the rest. It has:

- No MVVM to speak of. The view model has no bindable properties and no commands,
  nothing is bound, and there is no converter, no second page, no navigation past
  the first one and no dialog opened by the application. The `XamlRoot` wiring is
  present and unused, which is the graceful-degradation pattern rather than a
  demonstration of dialogs.
- No XAML layout. The page is one element in one `Grid`. Every menu, tree, canvas,
  toolbar, dialog and file picker you see belongs to the guest program and is
  created through the toolkit's Tcl command surface, so there is nothing here about
  laying out or styling platform controls.
- No use of the toolkit's XAML element set. The toolkit can also be driven by
  declaring its widgets in XAML and reading them through a bridge interface; this
  application uses only the host element, because its guest builds its own UI in
  script.
- No settings or persistence of the application's own. The guest program keeps its
  own recent-files list and preferences in its own way.
- No async or threading in application code beyond the single `Task.Run` that
  starts the boot, and no cancellation or progress reporting: the guest is
  synchronous by nature and the bridge is what makes that safe.
- No services registered in the resolver. The registration lambda in `App.xaml.cs`
  is a comment, so the wiring is shown but no resolution is.
- Nothing about the framebuffer head's on-screen keyboard, about theming, or about
  per-head behavior beyond the one render-surface line. The guest draws itself the
  same way everywhere.
- No unit tests of application logic, because there is almost none; the test
  project tests the guest's boot and open paths instead. A diagnostic mode inside
  `DrakonRuntime` can drive the running editor from script for maintainers, which
  is the closest thing here to a scripted end-to-end run.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder is the authoritative record of what is
bundled here and under what terms: the editor's own program tree, example
diagrams, documentation and artwork, the monospaced font that ships inside that
tree and the license texts that travel with it, and the single package index file
belonging to the PDF library whose implementation this application gets from a
package instead. It also records that the interpreter, the toolkit and the command
shims arrive as NuGet packages carrying their own licenses and notices, and that
the diagrams a user opens or creates belong to that user.

## License

DRAKON.Brix is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
