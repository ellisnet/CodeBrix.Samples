# RedisSetupTool

RedisSetupTool is a desktop control panel for Redis on a local Docker daemon. A
navigation rail on the left switches between eight sections, a header carries a
daemon status pill and a global refresh, and the rest of the window belongs to
whichever section is showing. It stands Redis up in thirteen topologies, from a
plain standalone node through password and ACL variants, persistence and
eviction settings, a primary with a replica, a Sentinel set, a six-node cluster,
a Valkey node, a modules image and a five-master Redlock quorum. Each instance it
creates gets a card: a state pill counting the nodes that are up, a dot per node
with its host port, and a CONNECT block of copyable rows - endpoints, service
name, username, connection string and a ready-to-paste command line, with the
password hidden behind a reveal toggle. Verify connects a real Redis client and
lists every check it ran. Beyond its own instances it manages every container on
the daemon, with a five-tab detail pane, live stats, diagnostics and an advisor;
it pulls, tags, prunes, scans and lints images; it creates and removes networks
and volumes; and it opens a real interactive shell inside a container in a tab
strip of live terminals.

It is this repository's reference for three things at once: driving a container
daemon from a CodeBrix.Platform application through a library that keeps the
daemon API out of the view models, hosting an interactive terminal in a page, and
building a large multi-section shell that keeps every section's live state alive
while another one is on screen. It draws nothing of its own: there is no canvas,
no media and no graphics anywhere in it, and everything on screen is an ordinary
control, a bound property, a brush or a font glyph, which makes it a good place
to see how far the plain XAML surface goes before you reach for a canvas.

## What this sample shows a CodeBrix.Platform developer

- How a large application is organized as a Core project plus three libraries
  with their own test projects, all around one shared UI project:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Which project each package reference belongs on - the framework, the fonts and
  the terminal add-in on Core, exactly one runtime package on each head:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- How `App.xaml` and the `Views` folder are file-linked into all six executables
  through a shared items project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Why the Core library's `RootNamespace` is set to the application name, and what
  the shared XAML then has to write to reach the view models:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- How one Windows-targeting head stays inside a solution that restores on Linux
  and macOS:
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- What a head's `Program.Main` contains and how the backend is selected:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- How the framebuffer head, which has no window manager, turns on the platform
  software keyboard and file-open picker so the application's text fields,
  terminals and Dockerfile picker still work:
  [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- The one per-head behavioral difference applied after `Build()` and before
  `Run()`:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- The four things the `App` constructor does, in the order they have to happen:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- How `OnLaunched` creates the window, puts a `Frame` in it and navigates to the
  first page:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- How to give `SimpleServiceResolver` a generic-host builder from the shared Core
  library instead of duplicating it in six heads:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- How each library publishes one `AddXxx` extension method so the application's
  whole registration lambda is three lines:
  [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- How two font packages become one application-wide default plus a second family
  the monospaced rows and the terminal ask for by resource key:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- How to get console diagnostics while developing and a silent Release build:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- The family's property and command idiom, used here at scale across a shell and
  eight section view models:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Why a view model constructed by the XAML designer needs a guard, and what it is
  paired with at startup:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How the first load is started from the view model constructor without making
  the constructor async:
  [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- How results that arrive off the UI thread are pushed into bound properties:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- How creating an instance runs as a long job with streamed progress, a busy flag
  and a cancel button, and rolls back what it made when it fails:
  [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- How a failure reaching the daemon becomes a readable line in the section rather
  than an unhandled exception:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- How eight sibling section grids are shown and hidden from computed properties
  rather than by navigating a `Frame`:
  [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).
- How a destructive action asks first, and how a failure is explained, from the
  view model rather than from code-behind:
  [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- The one-handler code-behind that hands the view model a `XamlRoot` getter
  through `IXamlRootGetter`, so those dialogs can open at all:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- How every copy button in the application reaches the clipboard through one
  bridge interface the page fills in, and degrades quietly where there is none:
  [Copy text to the clipboard from a command through a bridge interface](../BLUEPRINTS-PlatformServices.md#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface).
- How a dark application palette is carried into the dialog, picker and
  software-keyboard chrome the platform draws for itself:
  [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- How the topology catalog is presented as categories of rows built from group
  and row view models:
  [Build a grouped list from group and row view models](../BLUEPRINTS-ViewsAndControls.md#build-a-grouped-list-from-group-and-row-view-models).
- How the console tabs keep the page's code-behind readable by living in their
  own named partial file:
  [Split a page code-behind into named partial files](../BLUEPRINTS-ViewsAndControls.md#split-a-page-code-behind-into-named-partial-files).
- How the rail and the toolbars draw their icons from the shipped symbols font so
  they survive on a head with no system fonts:
  [Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts).
- How a library's internals are opened to exactly one test project:
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- What a test project for a CodeBrix library looks like, and why it builds as an
  executable:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- How one expensive fixture - here a live daemon connection and its label sweep -
  is shared by every test class that needs it:
  [Share one expensive fixture across every test class that needs it](../BLUEPRINTS-Testing.md#share-one-expensive-fixture-across-every-test-class-that-needs-it).
- How the tests that need something the machine may not have are made opt-in and
  kept out of the default run:
  [Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).
- How a whole application can be driven end to end through its own commands
  instead of through synthetic clicks:
  [Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application).
- How one library can be made the only place a third-party API is named, with the
  build rather than a review holding the line:
  [Keep a third-party API inside one library with PrivateAssets and a module initializer](../BLUEPRINTS-ProjectLayoutAndPackaging.md#keep-a-third-party-api-inside-one-library-with-privateassets-and-a-module-initializer).
- How eight sections share one poll of the daemon, and how the polling is held
  still while a long operation runs:
  [Refresh every section from one shared snapshot and one pausable timer](../BLUEPRINTS-MVVM.md#refresh-every-section-from-one-shared-snapshot-and-one-pausable-timer).
- How a section view model reaches navigation, the clipboard and the dialogs
  without holding a reference to the whole shell:
  [Let section view models ask the shell for the few things they cannot do](../BLUEPRINTS-MVVM.md#let-section-view-models-ask-the-shell-for-the-few-things-they-cannot-do).
- How a tab strip of controls that declare no dependency properties is built and
  kept in step with a bound collection:
  [Host a control with no dependency properties by mirroring a collection from code-behind](../BLUEPRINTS-ViewsAndControls.md#host-a-control-with-no-dependency-properties-by-mirroring-a-collection-from-code-behind).
- How the New Instance form builds its editors from whatever the selected topology
  declares, with one template and no template selector:
  [Generate a form from a parameter list with one template and per-editor Visibility](../BLUEPRINTS-ViewsAndControls.md#generate-a-form-from-a-parameter-list-with-one-template-and-per-editor-visibility).
- How the terminal pump is exercised with no control, no native libraries and no
  window, by writing to a sink interface:
  [Make a byte pump testable by writing to a sink interface instead of a control](../BLUEPRINTS-Testing.md#make-a-byte-pump-testable-by-writing-to-a-sink-interface-instead-of-a-control).
- Why this application has no settings store at all, and how everything it creates
  is found again from the labels it carries:
  [Put identity in labels on the resource instead of a state file beside it](../BLUEPRINTS-SettingsAndPersistence.md#put-identity-in-labels-on-the-resource-instead-of-a-state-file-beside-it).
- How each row of an instance card's CONNECT block masks, reveals and copies its
  value:
  [Show mask and copy a secret in a one-line row](../BLUEPRINTS-ViewsAndControls.md#show-mask-and-copy-a-secret-in-a-one-line-row).

## Building, running and testing

There is one solution, `RedisSetupTool.slnx`, and it holds everything: the shared
UI project, the Core project, all six heads, the three libraries under a
`Libraries` solution folder and the three test projects under a `Tests` solution
folder. Its header comment says what it is - everything that builds with the
plain .NET SDK on Linux, macOS and Windows - which holds here because every head
is a Skia head. There is no second, Windows-only solution, because this
application has no native WinUI 3, WPF or .NET MAUI head.

The heads:

| Project | Platform |
| --- | --- |
| `src/RedisSetupTool.LinuxX11` | Linux, X11 |
| `src/RedisSetupTool.LinuxWayland` | Linux, Wayland |
| `src/RedisSetupTool.LinuxFrameBuffer` | Linux, framebuffer (no display server) |
| `src/RedisSetupTool.MacOS` | macOS |
| `src/RedisSetupTool.Win32Skia` | Windows, Win32 window |
| `src/RedisSetupTool.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting` so the solution still
  restores on Linux and macOS. No workload is needed.
- All CodeBrix libraries come from NuGet. No CodeBrix library is referenced as a
  source project, so this folder builds on its own.
- A reachable Docker daemon. The application talks to the local daemon over its
  usual endpoint and needs no configuration to find it, but the account running
  the application has to be allowed to use it. Nothing else is required: the tool
  pulls the Redis and Valkey images it needs on demand, so the first instance of
  a topology takes as long as the pull does, and every later one is immediate.
  Network access is needed for those pulls.
- No accounts, tokens, data files or committed assets.

When there is no daemon, the application still starts and stays usable rather
than failing: the header pill turns red and reads "daemon unreachable", the line
beside it carries the exact error the client reported and the endpoint it tried,
every list stays empty, the counts read zero and everything that would touch the
daemon is disabled. The periodic refresh keeps trying, so starting the daemon
while the window is open brings the whole application to life without a restart.

To run one head from the command line, from this folder:

```text
dotnet run --project src/RedisSetupTool.LinuxX11
dotnet run --project src/RedisSetupTool.LinuxWayland
dotnet run --project src/RedisSetupTool.LinuxFrameBuffer
dotnet run --project src/RedisSetupTool.MacOS
dotnet run --project src/RedisSetupTool.Win32Skia
dotnet run --project src/RedisSetupTool.WinWpfSkia
```

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` is inside `#if DEBUG` - so a Release run is silent.

The application also has a scripted mode that drives one unattended pass through
its own commands after the first refresh, for verifying it on a head where
synthetic clicks are not dependable. It is off unless it is asked for;
`src/RedisSetupTool.Core/Services/StartupAutomation.cs` is the whole of it and
says what it can run.

Everything the application creates on the daemon carries its own labels, so it
can always be cleaned up without touching anything else: the System section's
sweep lists every instance and its container and volume counts before it removes
them.

There are three test projects, one per library. This application has no
`global.json`; the runner is selected by properties in each test csproj, and the
assemblies are self-executing binaries, so a plain `dotnet test` can report that
zero tests ran. When it does, build the test project and run the executable it
produces directly:

```text
dotnet build tests/libs/RedisSetupTool.DockerManagement.Tests/RedisSetupTool.DockerManagement.Tests.csproj -c Release
./tests/libs/RedisSetupTool.DockerManagement.Tests/bin/Release/net10.0/RedisSetupTool.DockerManagement.Tests
```

What each test project needs:

| Test project | Covers | Needs |
| --- | --- | --- |
| `tests/libs/RedisSetupTool.DockerManagement.Tests` | The Docker facade end to end against a real daemon - containers, images, networks, volumes, diagnostics, the exec and shell-probe plumbing - plus the pure parts: instance ids, the label schema, the mappers, the port allocator, the topology catalog and request validation, and real standalone and replica topologies created and torn down | A running Docker daemon, and network access the first time so the images it uses can be pulled. The heaviest topologies are opt-in and skip by default |
| `tests/libs/RedisSetupTool.RedisManagement.Tests` | The connection descriptor, the connection-string builder, the Redlock handle and the whole Redlock quorum algorithm against a fake server, and the probe's verification logic against a fake connection | Nothing. It runs with no daemon and no Redis. The live probe tests are opt-in against an endpoint you supply and skip by default |
| `tests/libs/RedisSetupTool.TerminalView.Tests` | The exec-to-terminal pump: byte forwarding, keystroke forwarding, the resize argument swap and the session states, driven through the sink interface, and the palette | Nothing. No control, no Skia natives and no window |

Every resource the suites create carries its own test label and is swept by that
label alone, so a test run never touches an instance made by the application.

## How the projects and folders are organized

```text
RedisSetupTool/
  RedisSetupTool.slnx                   The one solution: UI, Core, six heads, three libraries, three test projects
  THIRD-PARTY-NOTICES.txt               Third-party content used by this application
  src/
    RedisSetupTool.UI/                  Shared items project: the XAML that every head compiles
      RedisSetupTool.UI.shproj          Shared-project shell, so an IDE can load the folder as a project
      RedisSetupTool.UI.projitems       The shared file list each head imports with Label="Shared"
      App.xaml                          Merged resources, the two font families, and the dialog and picker brush keys
      App.xaml.cs                       Bootstrap: default font and fallbacks, service resolver, design mode, window and frame, logging
      Views/MainPage.xaml               The whole UI: the palette, the header, the rail and the eight section grids
      Views/MainPage.xaml.cs            Thin code-behind: the XamlRoot getter, the clipboard delegate, the console attach
      Views/MainPage.Consoles.cs        The console tabs, which have to be built in code rather than bound
    RedisSetupTool.Core/                The library every head references; carries the shared packages
      RegisterServices.cs               One AddRedisSetupTool() that composes the two library registrations and the shared state
      Bridges/ICopyToClipboard.cs       The clipboard bridge the page fills in
      Helpers/HostHelper.cs             The IHostBuilderProvider that SimpleServiceResolver builds its container from
      Services/AppState.cs              The shared daemon snapshot every section reads, refreshed once for all of them
      Services/ConnectionMapper.cs      The one place the Docker side and the Redis side meet
      Services/Formatting.cs            Byte counts, durations, percentages and relative times, shaped once
      Services/IShellContext.cs         What a section may ask of the shell: navigate, refresh, confirm, copy, open a console
      Services/Palette.cs               The palette as brushes, for the colors a view model has to pick itself
      Services/RefreshCoordinator.cs    One timer for the whole application, pausable while a long operation runs
      Services/StartupAutomation.cs     The scripted unattended pass, off unless asked for
      ViewModels/MainViewModel.cs       The shell: header, rail, the eight sections, and the IShellContext implementation
      ViewModels/*ViewModel.cs          One view model per section, plus the row, card, node, tab and field models they own
    RedisSetupTool.LinuxX11/            Head: Program.cs plus a csproj with one runtime package
    RedisSetupTool.LinuxWayland/        Head: Program.cs plus a csproj with one runtime package
    RedisSetupTool.LinuxFrameBuffer/    Head: as above, plus the software keyboard and file-open picker this head needs
    RedisSetupTool.MacOS/               Head: Program.cs plus a csproj with one runtime package
    RedisSetupTool.Win32Skia/           Head: Program.cs plus a csproj with one runtime package
    RedisSetupTool.WinWpfSkia/          Same, plus net10.0-windows and a software render surface
    libs/
      RedisSetupTool.DockerManagement/  The only project that references CodeBrix.Docker
        Exec/                           The exec stream, the shell probe and the session contract
        Instances/                      Instance ids, the label schema, the host-port allocator and its plan
        Mapping/                        Library types mapped into this library's own DTOs, at the seam
        Models/                         The DTOs every consumer sees; no daemon type crosses this line
        Topologies/                     The catalog, the request and the per-shape builders that stand an instance up
      RedisSetupTool.RedisManagement/   The only project that references a Redis client
        Health/                         The monitor that decides whether Redis, not just the container, is up
        Redlock/                        The Redlock algorithm over a set of independent masters
        Results/                        The read models the probe returns: ping, info, replication, cluster, verification
      RedisSetupTool.TerminalView/      Bridges an exec stream into the terminal control, through one sink interface
  tests/
    libs/
      RedisSetupTool.DockerManagement.Tests/   Mirrors src/libs/RedisSetupTool.DockerManagement; Infrastructure/ holds the shared fixture
      RedisSetupTool.RedisManagement.Tests/    Mirrors src/libs/RedisSetupTool.RedisManagement; Fakes/ holds the fake server and connection
      RedisSetupTool.TerminalView.Tests/       Mirrors src/libs/RedisSetupTool.TerminalView; Fakes/ holds the fake exec session and sink
```

The dependency direction is one way and it is the point of the layout. Each head
project takes a project reference on `RedisSetupTool.Core` and file-links the
shared UI by importing `..\RedisSetupTool.UI\RedisSetupTool.UI.projitems` with
`Label="Shared"`, so `App.xaml`, `App.xaml.cs` and the three `Views` files are
compiled once into each of the six head assemblies. That is why each head csproj
also has to tell MSBuild to treat `.xaml` files as `Page` items. Because the XAML
ends up inside the head assembly while the view models live in the library, the
page reaches them with an assembly-qualified `clr-namespace`
(`xmlns:vm="clr-namespace:RedisSetupTool.ViewModels;assembly=RedisSetupTool.Core"`),
while the code-behind's own namespace resolves inside whichever head is being
built.

`RedisSetupTool.Core` project-references the three libraries and carries the
framework, font and terminal packages, which reach the heads transitively.
Underneath, `RedisSetupTool.TerminalView` references
`RedisSetupTool.DockerManagement` because the pump drives an exec session, and
`RedisSetupTool.RedisManagement` references nothing else in the application at
all - it knows about hosts, ports, credentials and Redis commands, and nothing
about topologies or containers. Each test project references exactly its own
library. Nothing flows the other way: no library knows about the Core project,
the views or the heads.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework itself: `Application`, `Window`, `Frame`, `Page` and every control on the page, plus `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IXamlRootGetter`, `IHostBuilderProvider`, `CodeBrixPlatformHostBuilder`, `DispatcherTimer`, the dialog helpers and `FeatureConfiguration.Font` | `src/RedisSetupTool.Core/RedisSetupTool.Core.csproj`; used throughout `src/RedisSetupTool.UI/` and `src/RedisSetupTool.Core/` |
| CodeBrix.Platform runtime for the head | Exactly one runtime package per head - the X11, Wayland, framebuffer, macOS, Win32 and WPF Skia runtimes - and nothing else | the six head csproj files under `src/` |
| CodeBrix.Platform.TerminalView add-in | Supplies `TerminalControl`, the terminal grid that the console tabs host and that the exec pump feeds | `src/libs/RedisSetupTool.TerminalView/`, `src/RedisSetupTool.UI/Views/MainPage.Consoles.cs` |
| CodeBrix.Platform (the `CodeBrix.Platform.UI.Toolkit` converters namespace) | `BoolToVisibilityConverter` and `BoolNegationConverter`, for the handful of places a bound bool drives visibility or enablement directly | `src/RedisSetupTool.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.Fonts.Roboto | The application-wide default text font, plus the script fallback faces registered beside it | `src/RedisSetupTool.Core/RedisSetupTool.Core.csproj`, `src/RedisSetupTool.UI/App.xaml`, `src/RedisSetupTool.UI/App.xaml.cs` |
| CodeBrix.Platform.Fonts.RobotoMono | The monospaced family for endpoints, ids, connection strings, log text and the terminal grid, addressed by resource key wherever a value has to line up | `src/RedisSetupTool.Core/RedisSetupTool.Core.csproj`, `src/RedisSetupTool.UI/App.xaml`, `src/RedisSetupTool.UI/Views/MainPage.xaml` |
| CodeBrix.Docker | Everything the application does to the daemon: containers, images, networks, volumes, disk usage, events, the advisor, the containerized scan, efficiency and lint tools, and the exec streams the consoles ride on | `src/libs/RedisSetupTool.DockerManagement/` only, and named nowhere else |
| CodeBrix.Redis | The Redis client tier: connecting, ping and info, replication and cluster views, the exercise round trip, the health probe and the Redlock implementation | `src/libs/RedisSetupTool.RedisManagement/` only, and named nowhere else |
| SilverAssertions | The assertion style in all three test projects | The three test csproj files and every test file |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the dependency-injection container | `src/RedisSetupTool.Core/RedisSetupTool.Core.csproj`, `src/RedisSetupTool.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.DependencyInjection.Abstractions | The `IServiceCollection` each library's own `AddXxx` extension method extends | `src/libs/RedisSetupTool.DockerManagement/RegisterServices.cs`, `src/libs/RedisSetupTool.RedisManagement/RegisterServices.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider that is wired into the platform's ambient logger in Debug builds | `src/RedisSetupTool.Core/RedisSetupTool.Core.csproj`, `src/RedisSetupTool.UI/App.xaml.cs` |
| SkiaSharp | `SKColor`, the color type the terminal control takes for its background, foreground and selection; it arrives with the terminal add-in | `src/libs/RedisSetupTool.TerminalView/TerminalPalette.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for all three test projects | The three test csproj files |

## Worth studying in this application

### One library owns the daemon, and the compiler enforces it

`RedisSetupTool.DockerManagement` is the only project that names a CodeBrix.Docker
type. That is not a convention here, it is a build setting: the package reference
carries `PrivateAssets=all`, so the reference does not flow to
`RedisSetupTool.Core`, to the heads or to the other libraries, and a downstream
project that tries to name a daemon type fails to compile. The library exposes
its own DTOs at the seam - `ContainerInfo`, `ImageDetail`, `DiagnosticsReport`
and the rest, all under `Models/` - and maps into them under `Mapping/`, so no
daemon type ever crosses the boundary either.

`PrivateAssets=all` also stops the runtime asset flowing, which would leave every
consumer with a missing assembly at load time, so the same csproj republishes
just that assembly as a copy-to-output item, and `DockerAssemblyResolver` hooks
`AssemblyLoadContext` to find it beside its own assembly. Read
`src/libs/RedisSetupTool.DockerManagement/RedisSetupTool.DockerManagement.csproj`
and then `DockerAssemblyResolver.cs` next to it.

The sharp edge is the timing. The hook has to be installed before any code in the
library runs, because the runtime resolves the referenced assembly while it
prepares the body of the first method that mentions one of its types, which is
earlier than that method's first statement. A static constructor on the facade is
too late; a module initializer is not. The file says so, and carries the one
analyzer suppression the application needs, with the reason beside it. See
[Keep a third-party API inside one library with PrivateAssets and a module initializer](../BLUEPRINTS-ProjectLayoutAndPackaging.md#keep-a-third-party-api-inside-one-library-with-privateassets-and-a-module-initializer).

### Labels are the database

There is no state file, no settings store and nothing to drift. Every container,
volume and network the tool creates carries a fixed set of labels - the instance
id, the topology, the node's role and index, and the rest - and discovery rebuilds
the whole picture of what exists from those labels alone. Close the application,
reopen it, and it finds, manages and completely tears down everything it made,
including instances created by an earlier run. It is also what makes the sweep
safe: the sweep removes what carries the label and nothing else, so the tool
cannot damage a container someone else put on the daemon.

Read `src/libs/RedisSetupTool.DockerManagement/Instances/InstanceLabels.cs` for
the schema and `Instances/InstanceId.cs` for identity, then
`Topologies/RedisTopologyService.cs` for discovery and teardown. If you are
building something that creates resources on a shared daemon, this is the pattern
worth copying: put identity in the resource, not in a file beside it. See
[Put identity in labels on the resource instead of a state file beside it](../BLUEPRINTS-SettingsAndPersistence.md#put-identity-in-labels-on-the-resource-instead-of-a-state-file-beside-it).

### Thirteen topologies from one catalog and five builders

The topologies are described once, in `Topologies/TopologyCatalog.cs`: a
descriptor per topology carrying its category, its display name, its container
count, its highlights and its parameter list. Nothing else in the library
hard-codes an image, a container count or a parameter. Creating an instance is a
`TopologyRequest` validated against its descriptor, handed to whichever
`ITopologyBuilder` handles that shape - standalone, replica, Sentinel, cluster or
quorum - and run inside a build context that streams progress lines back and
rolls everything it made back if any step fails.

That split is what keeps the create form generic. The form has no knowledge of
any particular topology: it reads the descriptor's parameters and generates
editors for them. Read the catalog, then `Topologies/Builders/ITopologyBuilder.cs`
and one concrete builder, then `Topologies/TopologyRequest.cs` for the validation
the form's disabled Create button is driven by, and
`src/RedisSetupTool.Core/ViewModels/CreateInstanceViewModel.cs` for the command
that runs the build: it pauses the refresh timer, streams every progress line into
a bound headline, offers a cancel that cancels the token the builder was given,
and reports what the rollback removed when a step fails. See
[Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag),
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing)
and
[Generate a form from a parameter list with one template and per-editor Visibility](../BLUEPRINTS-ViewsAndControls.md#generate-a-form-from-a-parameter-list-with-one-template-and-per-editor-visibility).

### The host-port allocator, and the race it cannot fully close

Every node needs a free host port, and the ranges are fixed per role. The
allocator builds the in-use set from every published port on the daemon plus its
own soft reservations, then walks the relevant range binding each candidate to
prove it is actually free. The window between that bind probe and the daemon
publishing the port is a genuine race that no amount of care closes: the
soft-reservation set closes the in-process half of it, and the create path's
rollback handles the rest. The file says exactly that, which is the honest way to
document a race you have decided to live with.

Read `src/libs/RedisSetupTool.DockerManagement/Instances/HostPortAllocator.cs` and
`Instances/PortPlan.cs`.

### A shell of eight sections, none of which is ever thrown away

The rail does not navigate. `MainViewModel` holds eight section view models,
builds them once in its constructor and switches between them by flipping
`Visibility` properties over eight sibling grids in one page. A `Frame` was tried
and works, but it constructs a fresh page and a fresh view model on every
navigation, including a return to a section already visited, and this
application's sections hold live state: an open console session, a running stats
stream, a half-filled create form. The remarks block at the top of
`src/RedisSetupTool.UI/Views/MainPage.xaml.cs` records the reasoning and the
second finding that decided it - a terminal created inside a collapsed grid
raises `Loaded` only when that grid becomes visible, and keeps its content
afterwards.

The sections talk to the shell through `IShellContext`, which `MainViewModel`
implements and hands to every child at construction. That interface is worth
reading on its own: it is the complete list of cross-section moves the
application allows - navigate, refresh, confirm, report, copy, open a console,
show a container, pause the timer - and nothing else. The children never hold a
reference to the whole shell. See
[Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties),
[Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs)
and
[Let section view models ask the shell for the few things they cannot do](../BLUEPRINTS-MVVM.md#let-section-view-models-ask-the-shell-for-the-few-things-they-cannot-do).

### One snapshot, one timer

Eight sections that each polled the daemon would be eight sets of calls racing
each other for the same answers. Instead `AppState` is a single shared snapshot:
one refresh asks for containers, images, networks, volumes, disk usage, the
discovered instances and the advisor findings, stores them, and raises one
`Changed` event. Sections read the snapshot and never call the daemon for the
same thing. A semaphore serializes refreshes, and a failure leaves the snapshot's
reachability flag false with the error message beside it rather than throwing into
the UI.

`RefreshCoordinator` is the other half: one `DispatcherTimer` for the whole
application, constructed on the UI thread, pausable so a long create or teardown
is not interrupted by a refresh landing in the middle of it. `AppState` raises
`Changed` on whatever thread the refresh finished on and says so in its own
documentation, which is why subscribers marshal for themselves. Read
`src/RedisSetupTool.Core/Services/AppState.cs` and
`Services/RefreshCoordinator.cs` together, then `MainViewModel.ApplyState()` to
see the snapshot turn into bound properties. See
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and
[Refresh every section from one shared snapshot and one pausable timer](../BLUEPRINTS-MVVM.md#refresh-every-section-from-one-shared-snapshot-and-one-pausable-timer).

### A real shell in a tab, and a control that cannot be bound

The Consoles section is the most interesting piece of view work here, because
`TerminalControl` declares no dependency properties. It cannot be bound, styled
or placed inside a `DataTemplate`, so the tabs cannot be built in XAML at all.
The application's answer is a named partial file,
`src/RedisSetupTool.UI/Views/MainPage.Consoles.cs`, that mirrors the view model's
`Tabs` collection into a `TabView`: it watches `CollectionChanged`, creates a
control and a body grid per tab, and keeps every body in the tree with the
inactive ones collapsed so their terminals stay loaded and keep receiving output.
The view model holds only the tab's status - the resolved shell, the grid size,
the session state - and the page pushes what it learns back through a few `Apply`
methods.

Under it, `RedisSetupTool.TerminalView` is the seam that makes the whole thing
testable. The pump writes to an `ITerminalSink` rather than to a control;
`TerminalControlSink` is the only type in the application that names
`TerminalControl`. Bytes go through the sink untouched, because decoding a
partial read is exactly the bug that seam avoids, and the resize call swaps its
arguments on the way through, because the control reports columns then rows while
the daemon takes rows then columns. That is why the terminal suite runs with no
control, no Skia natives and no window.

Read `MainPage.Consoles.cs`, then `src/libs/RedisSetupTool.TerminalView/`
end to end, then `src/libs/RedisSetupTool.DockerManagement/Exec/ShellProber.cs`
for how the shell is found: candidates are run and their exit codes read, because
a missing binary does not throw and does not hang - the daemon upgrades the
connection, writes the runtime's complaint on the ordinary output stream, closes
and reports 127. See
[Split a page code-behind into named partial files](../BLUEPRINTS-ViewsAndControls.md#split-a-page-code-behind-into-named-partial-files),
[Host a control with no dependency properties by mirroring a collection from code-behind](../BLUEPRINTS-ViewsAndControls.md#host-a-control-with-no-dependency-properties-by-mirroring-a-collection-from-code-behind)
and
[Make a byte pump testable by writing to a sink interface instead of a control](../BLUEPRINTS-Testing.md#make-a-byte-pump-testable-by-writing-to-a-sink-interface-instead-of-a-control).

### A form generated from a topology's parameters

The New Instance section builds its editors from whatever the selected topology
declares: text, password with a generate button, number, choice, switch and
multi-line. The family has no `DataTemplateSelector`, so the application does it
with one template that carries all six editors and a `Visibility` property per
editor on `ParameterFieldViewModel`, with exactly one ever visible. Validation is
the same shape: the view model publishes what is still wrong with the request as
a bound list, and the Create button stays disabled until that list is empty, so
nothing has to be duplicated between the button's `CanExecute` and the message
the user reads.

Read `src/RedisSetupTool.Core/ViewModels/ParameterFieldViewModel.cs` first, then
`ViewModels/CreateInstanceViewModel.cs`, then the create block in
`src/RedisSetupTool.UI/Views/MainPage.xaml`. See
[Build a grouped list from group and row view models](../BLUEPRINTS-ViewsAndControls.md#build-a-grouped-list-from-group-and-row-view-models),
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way)
and
[Generate a form from a parameter list with one template and per-editor Visibility](../BLUEPRINTS-ViewsAndControls.md#generate-a-form-from-a-parameter-list-with-one-template-and-per-editor-visibility).

### The connect block: copy, mask, reveal

Every row of an instance card's CONNECT block is an `EndpointRowViewModel`: a
label, the value in the monospaced family, and a copy button that briefly says
"Copied" before reverting. A password row hides its value behind bullets until
the reveal button is pressed. All of them reach the clipboard the same way, and
it is worth following: the view model implements nothing platform-specific, it
sets a delegate on `ICopyToClipboard` that the page fills in from its
`DataContextChanged` handler, because only the page can reach the clipboard type.
A view model has to behave sensibly while that delegate is still null, and that
is how a head with no clipboard is supported rather than crashed.

The text the copy button puts on the clipboard and the connection the
verification code opens are built by the same
`RedisConnectionStringBuilder`, in one place, so what the user pastes cannot
drift from what the application itself connects with. Read
`src/RedisSetupTool.Core/Bridges/ICopyToClipboard.cs`,
`src/RedisSetupTool.UI/Views/MainPage.xaml.cs`,
`src/RedisSetupTool.Core/ViewModels/EndpointRowViewModel.cs` and
`src/libs/RedisSetupTool.RedisManagement/RedisConnectionStringBuilder.cs`. See
[Copy text to the clipboard from a command through a bridge interface](../BLUEPRINTS-PlatformServices.md#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface)
and
[Show mask and copy a secret in a one-line row](../BLUEPRINTS-ViewsAndControls.md#show-mask-and-copy-a-secret-in-a-one-line-row).

### Running is not the same as ready

An instance card's pill is green when Redis answers, not when the containers are
up, and those are different things: a container can be running while the server
inside it is still loading an append-only file or waiting on a replica. That
distinction lives in `RedisHealthMonitor`, and it is the reason the Redis tier
exists as its own library rather than as a few calls inside the Docker one.
Verify goes further and runs a list of per-topology checks through `RedisProbe` -
replication really established, Sentinel really reporting the primary's gateway
address, the cluster really reporting a good state with the slots split - and
reports each check as a row with a tick or a cross.

Everything in that library works through `IRedisConnection`, and exactly one type
names the client's multiplexer. That is what lets the verification logic be
exercised against a fake connection with no daemon and no Redis anywhere in
sight. Read `src/libs/RedisSetupTool.RedisManagement/IRedisConnection.cs`,
`RedisProbe.cs` and `Health/RedisHealthMonitor.cs`, then
`src/RedisSetupTool.Core/Services/ConnectionMapper.cs`, which is the single place
the topology side and the Redis side meet.

### Redlock, implemented rather than gestured at

`Redlock/RedlockService.cs` is a complete implementation of the algorithm over a
set of independent masters: take the key on every master in parallel with a short
per-node timeout, subtract both the elapsed time and a clock-drift allowance from
the requested lifetime, and call the lock held only when a quorum granted it and
there is time left over. It is short, it is commented, and it is tested against a
fake server, so it reads as a worked example of the algorithm rather than as
plumbing. Topology H1 exists so that it can be run for real against five separate
masters.

### A dark palette that reaches the chrome you did not draw

The application is dark, and most of that is ordinary: `MainPage.xaml` declares
its brushes as page resources and the markup uses them. The part worth studying
is `App.xaml`, which re-keys the platform's own `ContentDialog` brush keys -
background, foreground, border, light-dismiss overlay, the top overlay, the
separator and the smoke fill. Dialogs open in the popup layer, which follows the
application's requested theme rather than the page's root grid, so without those
keys a dark application opens light dialogs. On the framebuffer head the same
keys are what the built-in file picker and software keyboard resolve, so
restyling them once restyles that chrome too.

Code that has to choose a color itself - selection highlighting, state dots,
severity chips - reads `Services/Palette.cs`, which holds the same values as
brushes. The family does selection highlighting with `Brush`-typed view-model
properties rather than styles or triggers, which is why `NavItemViewModel` and
`TopologyChoiceViewModel` expose brush pairs. See
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).

### Tests that need a daemon, and tests that need nothing

The three suites are a useful contrast. The Docker suite is an integration suite:
one collection fixture opens the daemon connection and sweeps by the test label
before and after, every test class shares it, and the tests create and destroy
real containers, networks and volumes. The Redis suite and the terminal suite are
the opposite: they exercise the same code the application runs, through the
interfaces the libraries were designed around, with a fake Redis server and a
fake exec session, so they need nothing but the SDK.

The gating is worth copying. Tests that need something the machine may not have -
the heaviest topologies, a live Redis endpoint - are attached with a custom fact
attribute that reads an environment variable and sets the skip reason when the
variable is absent, so the default run is fast, green and honest about what it
did not do. Read
`tests/libs/RedisSetupTool.DockerManagement.Tests/Infrastructure/` for the
fixture, the collection and the gate, and
`tests/libs/RedisSetupTool.RedisManagement.Tests/Fakes/` for the fakes. See
[Share one expensive fixture across every test class that needs it](../BLUEPRINTS-Testing.md#share-one-expensive-fixture-across-every-test-class-that-needs-it),
[Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run)
and
[Make a byte pump testable by writing to a sink interface instead of a control](../BLUEPRINTS-Testing.md#make-a-byte-pump-testable-by-writing-to-a-sink-interface-instead-of-a-control).

### What this application does not show

It is large, but it is deliberately narrow in places. It has:

- No canvas, no custom-drawn control, no graphics and no media. Every pixel is an
  ordinary control, a brush or a font glyph, so there is no `SKXamlCanvas`,
  shader, image decoding or animation pattern to take from it.
- No second page and no navigation. There is one `Page`, navigated to once, and
  the rail switches grids inside it. If you need real navigation, page lifetimes,
  back stacks or parameter passing, look elsewhere.
- No settings and no persistence. Nothing is remembered between runs by design:
  the daemon's labels are the only state, so there is no settings store, no
  serialization and no file format here.
- No custom converters, styles, control templates or resource dictionaries beyond
  the palette and the re-keyed dialog brushes. Two stock converters are used, and
  that is all.
- No file dialogs from the view model, no drag and drop, no printing, no
  charting. The framebuffer head turns the platform's file-open picker on, but
  the application's own use of it is limited to one input.
- No remote daemons, no TLS or ssh transports, no registry authentication and no
  image building from a Dockerfile the user wrote. It drives the local daemon
  with the credentials the user already has.
- No editing of a running instance beyond start, stop, restart and destroy. A
  configuration change means a new instance.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder is the attribution record for this
application. Nothing third-party is bundled in this folder: there are no fonts,
images, models, data files or vendored sources here. Every code dependency
arrives as a NuGet package carrying its own license and notices, the fonts arrive
inside their packages, and the container images the application pulls at run time
are fetched from a registry by the user's own daemon and are never redistributed
here.

## License

RedisSetupTool is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
