# SimpleCbxVideoPlayer

SimpleCbxVideoPlayer is a one-page video player that grades the picture while it plays. The window holds
two drop-downs along the top - which clip to play, and which render path to compose it on - a black picture
area, a lookup-table panel down the right-hand side, a transport strip with Play, Pause, Stop, a scrub bar
and a clock, and a status line along the bottom. The clips and the color lookup tables are carried by the
application itself and copied to the folder it runs from, so it has something real to open the moment it
starts. Ticking tables in the panel and pressing Play applies them to the picture as one composed grade,
and a Bake button writes that same grade out as a single `.cube` file wherever you choose to put it.

It is this repository's reference for three things working together: demultiplexing a container and pacing
decoded frames to a clock with CodeBrix.VideoPlayback, composing those frames onto a Skia canvas the
application owns with CodeBrix.VideoPlayback.Skia, and getting a real graphics context on a CodeBrix.Platform
head so that a shader-based color grade is possible at all. It is also a worked example of a sample that
ships its own data: the clip corpus and the lookup tables are ordinary content items, copied to the output
folder and read from beside the executable.

## What this sample shows a CodeBrix.Platform developer

- Register the decoders a playback library deliberately does not reference itself, once per process, from
  one small static helper the view model calls at start-up:
  [Turn on extra media codecs once at startup](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-extra-media-codecs-once-at-startup).
- Put a GPU Skia surface and a processor Skia surface in the same cell, settle on whichever one actually
  started, and tell the view model which it got:
  [Offer a CPU fallback for a GPU rendering path behind one switch](../BLUEPRINTS-GraphicsAndRendering.md#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch).
- Let the page repaint a canvas the view model knows nothing about, by handing down one invalidate
  delegate that marshals to the user-interface thread:
  [Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface).
- Ask the platform's own save dialog where a generated file should go, and treat a cancel as a decision
  rather than a failure:
  [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Clear the empty placeholder a picker leaves behind at a brand-new name, so a write that fails leaves
  nothing that looks like a result:
  [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Declare a Skia page's namespaces and bind every value with the platform's own binding markup extension:
  [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- Keep a panel's whole enablement matrix - transport state against render path - in a static class of pure
  functions, so it can be tested without a window:
  [Keep view model rules in a plain class so they can be tested](../BLUEPRINTS-Testing.md#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested).
- Notify a `double` bindable property by hand, because `SetProperty` has no overload for a value type:
  [Notify a value typed bindable property by hand](../BLUEPRINTS-MVVM.md#notify-a-value-typed-bindable-property-by-hand).
- Guard a two-way bound scrub bar with a suppression flag, so the clock moving it is not read as a person
  dragging it:
  [Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).
- Take events raised on a decoding thread and set bound state from them safely:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Report a library's own failure message word for word in a status line instead of throwing or opening a
  dialog:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- Show a note only while there is something to say, from a computed `Visibility` property:
  [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties)
  and
  [Show a panel only when the last operation left something to say](../BLUEPRINTS-ViewsAndControls.md#show-a-panel-only-when-the-last-operation-left-something-to-say).
- Declare bound properties and lazily created `SimpleCommand` commands the family way:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Guard the view model constructor for the XAML designer, and turn design mode off at start-up:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Hand the view model a `XamlRoot` getter in the same handler that wires the bridges:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Keep every head's `Program.Main` to the same handful of lines, differing only in the call that names the
  platform:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Cast the built host on the WinWpfSkia head to force its software render surface:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Do the startup jobs in the `App` constructor in the order that matters:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor)
  and
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- Give `SimpleServiceResolver` a generic host builder from one helper compiled once in the Core library:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Make a bundled font the default for every head and register the fallbacks a missing glyph needs:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Install a console logger factory only in Debug builds:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Carry every shared package in one Core library and give each head exactly one platform runtime package:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Reference the higher-level graphics package and let its binding arrive transitively, while still naming
  what you actually depend on:
  [Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS-ProjectLayoutAndPackaging.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively)
  and
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Organize an application as a shared UI project, a Core project, libraries under `src/libs` and mirrored
  test projects under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project)
  and
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Set the Core library's root namespace to the application namespace, and reach the view models from shared
  XAML with an assembly-qualified `clr-namespace`:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Keep one Windows-targeting head inside a solution that restores on Linux and macOS:
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- Record bundled content - media, data files, their licenses - in a notices file that separates what is
  bundled from what merely arrives as a package:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- Read a data file from beside the binary rather than from a path the build happened to know:
  [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).
- Give the test project the native asset package a head would have supplied, so tests that build a real
  presenter can run:
  [Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied).
- Set up an xUnit v3 test project the family's runner actually discovers, and expose only the internals its
  own test assembly needs:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
  and
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Compare two rendered frames channel by channel with a tolerance, and say which pixel differed and by how
  much:
  [Compare rendered images pixel by pixel](../BLUEPRINTS-Testing.md#compare-rendered-images-pixel-by-pixel).
- Drive the whole application - a real head, a real canvas, a real decode - from a script that reports
  machine-readable lines and exits with a status:
  [Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application).
- Put two canvases in one cell, settle on whichever one the graphics device let start, and announce the
  result to the view model exactly once:
  [Settle between a GPU canvas and a CPU canvas and tell the view model which one started](../BLUEPRINTS-GraphicsAndRendering.md#settle-between-a-gpu-canvas-and-a-cpu-canvas-and-tell-the-view-model-which-one-started).
- Give a library the graphics context in the one place it is current - inside the paint handler - and let
  it treat the call as free:
  [Hand a video presenter the graphics context inside the paint handler that makes it current](../BLUEPRINTS-GraphicsAndRendering.md#hand-a-video-presenter-the-graphics-context-inside-the-paint-handler-that-makes-it-current).
- Reduce an expensive effect chain to a signature string so it is rebuilt when the selection really moved
  and not on every repaint:
  [Rebuild an expensive effect chain only when its signature changes](../BLUEPRINTS-GraphicsAndRendering.md#rebuild-an-expensive-effect-chain-only-when-its-signature-changes).
- Let a command ask for the composed picture and have the next paint hand it back, with a deadline in
  case no paint ever comes:
  [Deliver a frame capture from inside the paint handler to the command that asked for it](../BLUEPRINTS-GraphicsAndRendering.md#deliver-a-frame-capture-from-inside-the-paint-handler-to-the-command-that-asked-for-it).
- Compose a stack of color lookup tables into one portable file, independently of anything that happens
  to be playing:
  [Compose a chain of color lookup tables and write it back out as one file](../BLUEPRINTS-MediaAndVision.md#compose-a-chain-of-color-lookup-tables-and-write-it-back-out-as-one-file).
- Carry a data corpus as ordinary content on the Core library and resolve it beside the running
  executable, with the test project linking the same files:
  [Ship a data corpus as content items and find it under the application base directory](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-data-corpus-as-content-items-and-find-it-under-the-application-base-directory).
- Decide what a folder offers with a rule and a reasoned exclusion list rather than a list of names in
  the code:
  [Offer a data folder by rule rather than by a hard-coded list](../BLUEPRINTS-DocumentsAndData.md#offer-a-data-folder-by-rule-rather-than-by-a-hard-coded-list).
- Make the hop to the UI thread certain before showing a picker, and hand the chosen path back to the
  awaiting command:
  [Marshal a save dialog onto the UI thread from a command handler](../BLUEPRINTS-PlatformServices.md#marshal-a-save-dialog-onto-the-ui-thread-from-a-command-handler).

## Building, running and testing

There is one solution, `SimpleCbxVideoPlayer.slnx`, and it holds everything: the shared UI project, the
Core project, all six heads, the playback library under a `Libraries` solution folder and its test project
under a `Tests` solution folder. Its header comment describes it as everything that builds with the plain
.NET SDK on Linux, macOS and Windows, which holds here because every head is a Skia head.

| Head project | Platform |
| --- | --- |
| `src/SimpleCbxVideoPlayer.LinuxX11` | Linux, X11 |
| `src/SimpleCbxVideoPlayer.LinuxWayland` | Linux, Wayland |
| `src/SimpleCbxVideoPlayer.LinuxFrameBuffer` | Linux, framebuffer (no display server) |
| `src/SimpleCbxVideoPlayer.MacOS` | macOS |
| `src/SimpleCbxVideoPlayer.Win32Skia` | Windows, Win32 window |
| `src/SimpleCbxVideoPlayer.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Every head targets `net10.0` except WinWpfSkia, which targets `net10.0-windows` and sets
`EnableWindowsTargeting` so the solution still restores and builds from Linux or macOS; it can only run on
Windows.

Prerequisites:

- The .NET 10 SDK. No workload is needed.
- All CodeBrix packages come from NuGet. No CodeBrix library is referenced as a source project, so this
  folder builds on its own.
- A graphics device for the color grade. The picture plays either way, but the lookup tables are applied
  on the graphics path only, and the application says so in its status line and greys the panel out when it
  has settled on the processor path.
- A sound device for the soundtrack. Every clip carries one; the transport works without one, and the
  scripted mode can switch the soundtrack off entirely.
- Nothing to download and nothing to supply. The clips and the lookup tables are carried in
  `src/SimpleCbxVideoPlayer.Core/Assets/` and copied to each head's output folder by the build.

Build and run one head from this folder:

```text
dotnet build SimpleCbxVideoPlayer.slnx
dotnet run --project src/SimpleCbxVideoPlayer.LinuxX11
dotnet run --project src/SimpleCbxVideoPlayer.LinuxWayland
dotnet run --project src/SimpleCbxVideoPlayer.LinuxFrameBuffer
dotnet run --project src/SimpleCbxVideoPlayer.MacOS
dotnet run --project src/SimpleCbxVideoPlayer.Win32Skia
dotnet run --project src/SimpleCbxVideoPlayer.WinWpfSkia
```

The frame-buffer head wants a text console rather than a desktop session. Console logging is compiled in
only for Debug builds, so a Release run is silent.

The application also has a scripted mode that opens the real window on the real head, plays a chosen clip,
captures one composed frame, optionally bakes and re-applies the grade, prints a line per step and leaves
with an exit code. It is the application's own end-to-end check and it is maintainer material rather than a
feature; the code is `Diagnostics/SmokeOptions.cs` in the playback library and the smoke region at the foot
of `src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`.

The test project uses xUnit v3 with SilverAssertions, builds as `Exe` and sets
`UseMicrosoftTestingPlatformRunner`, so the test assembly is a self-executing binary. That matters in
practice: a plain `dotnet test` can report that it discovered zero tests. When it does, build the test
project and run the produced executable directly:

```text
dotnet build tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SimpleCbxVideoPlayer.SkiaVideo.Tests.csproj
./tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/bin/Debug/net10.0/SimpleCbxVideoPlayer.SkiaVideo.Tests
```

What the test project needs:

| Test project | Covers | Needs |
| --- | --- | --- |
| `tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests` | Decoder registration, the transport controller, chain composition and change detection, the panel's enablement rules, the catalog and corpus scans against the real bundled files, the scripted-run command-line parser, the bake file-name rule, and the frame comparer | Native Skia, referenced by the test project because the render-path tests build a real presenter; the bundled corpus, linked into the test output so the catalog tests read the same files the application does. No window, no graphics device and no sound device |

## How the projects and folders are organized

```text
SimpleCbxVideoPlayer/
  SimpleCbxVideoPlayer.slnx             The one solution: UI, Core, six heads, the library, its test project
  THIRD-PARTY-NOTICES.txt               The bundled clips and lookup tables, and the licenses they carry
  src/
    SimpleCbxVideoPlayer.UI/            Shared items project (.shproj + .projitems): the XAML every head compiles
      App.xaml                          Merged WinUI resources and the Roboto FontFamily resource
      App.xaml.cs                       Bootstrap: default font, font fallbacks, service resolver, design mode, window, logging
      Views/MainPage.xaml               The whole UI: two drop-downs, the canvases, the lookup-table panel, the transport
      Views/MainPage.xaml.cs            Code-behind: builds the GPU canvas, settles the surface, shows the save dialog
    SimpleCbxVideoPlayer.Core/          The library every head references; carries the shared packages
      Helpers/HostHelper.cs             The host-builder provider SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs       All application logic: corpus, transport, render path, chain, bake, scripted run
      ViewModels/LutListItem.cs         One row of the lookup-table panel: tick box, titles, percentage box
      ViewModels/RenderPathChoice.cs    One entry of the render-path drop-down
      ViewModels/VideoListItem.cs       One entry of the clip drop-down
      Assets/authoring/                 The clip corpus, copied to every head's output folder
      Assets/LUTs/                      The color lookup tables, their license texts and their manifest
    SimpleCbxVideoPlayer.LinuxX11/      Head: Program.cs plus a csproj with one runtime package
    SimpleCbxVideoPlayer.LinuxWayland/  Head: Program.cs plus a csproj with one runtime package
    SimpleCbxVideoPlayer.LinuxFrameBuffer/  Head: Program.cs plus a csproj with one runtime package
    SimpleCbxVideoPlayer.MacOS/         Head: Program.cs plus a csproj with one runtime package
    SimpleCbxVideoPlayer.Win32Skia/     Head: Program.cs plus a csproj with one runtime package
    SimpleCbxVideoPlayer.WinWpfSkia/    Same, plus net10.0-windows and a software render surface
    libs/
      SimpleCbxVideoPlayer.SkiaVideo/   The seam: the only project that names the video packages
        SkiaVideoRuntime.cs             Registers the AV1 and Opus decoders, once per process
        VideoPlaybackController.cs      Open, play, pause, stop, seek, draw, take a context, apply a chain, bake it
        Assets/                         SampleAssets, the clip corpus scan, the lookup-table catalog
        Effects/                        A chain entry, the chain and its change detection, the effect factory
        Playback/                       Transport and render-path types, the panel policy, the bake naming rule
        Diagnostics/                    The scripted-run options, the frame comparer and its result
  tests/
    libs/
      SimpleCbxVideoPlayer.SkiaVideo.Tests/   Mirrors src/libs/SimpleCbxVideoPlayer.SkiaVideo
```

Dependency direction is strictly one way. Each head project references `SimpleCbxVideoPlayer.Core` and
file-links the shared UI through an `Import` of `SimpleCbxVideoPlayer.UI.projitems`, so `App.xaml`,
`App.xaml.cs`, `MainPage.xaml` and `MainPage.xaml.cs` are compiled into every head rather than into a
library. That is why each head csproj repeats the `<Page Include="**\*.xaml" />` plus
`<None Remove="**\*.xaml" />` pair. `SimpleCbxVideoPlayer.Core` project-references the one library, carries
the platform, font and canvas packages, and owns the `Assets` folder; everything the heads share arrives
transitively through it. The library references nothing in the application and names every video package
itself, which is what makes it the seam.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework: `Application`, `Window`, `Frame`, `Page`, the controls on the page, the binding markup extension, `FeatureConfiguration.Font`, and the "Simple" toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `InvokeOnMainThread`) | `src/SimpleCbxVideoPlayer.Core/SimpleCbxVideoPlayer.Core.csproj`; used throughout `src/SimpleCbxVideoPlayer.UI/` and `src/SimpleCbxVideoPlayer.Core/` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface; `CodeBrixPlatformHostBuilder` selects it in `Program.Main` | The six head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Roboto | Supplies Roboto as the application-wide default text font, and the Noto faces registered as fallbacks for scripts Roboto has no glyph for | `src/SimpleCbxVideoPlayer.Core/SimpleCbxVideoPlayer.Core.csproj`, `src/SimpleCbxVideoPlayer.UI/App.xaml`, `src/SimpleCbxVideoPlayer.UI/App.xaml.cs` |
| CodeBrix.Platform.SkiaSharp.Views | Supplies `SKXamlCanvas`, the processor-side drawing surface the page declares in XAML and paints the frame into when there is no graphics device | `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml`, `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs` |
| CodeBrix.Platform.Graphics3DGL | Supplies `SkiaGLCanvasElement`, which creates an offscreen OpenGL context and hands each paint a GPU-backed surface and the `GRContext` behind it. `SKXamlCanvas` cannot supply one, and that context is what makes the color grade possible on these heads | `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs`, `src/SimpleCbxVideoPlayer.Core/SimpleCbxVideoPlayer.Core.csproj` |
| CodeBrix.VideoPlayback | The demultiplexers, the playback session and its clock, and the whole color-lookup-table engine: reading a `.cube` file, composing a chain of them into one table, and writing that table back out | `src/libs/SimpleCbxVideoPlayer.SkiaVideo/VideoPlaybackController.cs`, `src/libs/SimpleCbxVideoPlayer.SkiaVideo/Effects/LutEffectFactory.cs` |
| CodeBrix.VideoPlayback.Skia | The presenter: it takes decoded frames, applies the composed effect chain on the graphics device or converts on the processor, and blits the result into whatever Skia canvas the application owns | `src/libs/SimpleCbxVideoPlayer.SkiaVideo/VideoPlaybackController.cs` |
| CodeBrix.VideoPlayback.Dav1d | The AV1 decoder, registered once at start-up. Nothing in this family is discovered by reflection, so without this registration nothing in the corpus plays | `src/libs/SimpleCbxVideoPlayer.SkiaVideo/SkiaVideoRuntime.cs` |
| CodeBrix.Audio.Opus | The Opus decoder, registered in the same call. The Matroska and WebM clips carry Opus soundtracks; the bespoke-container clips carry Vorbis, which needs no extra package | `src/libs/SimpleCbxVideoPlayer.SkiaVideo/SkiaVideoRuntime.cs` |
| SilverAssertions | The assertion style in the test project | `tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | `SKCanvas`, `SKSurface`, `SKRect` and `GRContext` - the surface the presenter draws into and the context it needs for its graphics path; its Linux native-asset package is referenced by the test project so the render-path tests can build a presenter | `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs`, `src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`, `tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SimpleCbxVideoPlayer.SkiaVideo.Tests.csproj` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the container | `src/SimpleCbxVideoPlayer.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider wired into the platform's ambient logger in Debug builds | `src/SimpleCbxVideoPlayer.UI/App.xaml.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for the test project | `tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SimpleCbxVideoPlayer.SkiaVideo.Tests.csproj` |

## Worth studying in this application

### One library is the seam, and the application never names a video type

`SimpleCbxVideoPlayer.SkiaVideo` is the only project in the solution that references the playback packages,
and the view model talks to exactly one class in it: `VideoPlaybackController`. That class exposes open,
play, pause, stop, seek, "draw into this canvas", "here is a graphics context" and "apply this chain of
tables", plus a handful of events, and everything below it - demultiplexers, decoders, presenters,
effects - stays behind that surface. The Core project sees the library's own types and nothing else.

Two consequences are worth noticing. The application's packaging is trivially reasonable: Core carries the
framework, the font and the two canvas packages, the library carries the video packages, and each head
carries exactly one runtime package. And the library is testable on its own, with no window and no
graphics device, which is why the test project references only it. Read
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/VideoPlaybackController.cs` first, and then the constructor of
`src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`, where six events are subscribed and every one
of them is marshaled with `InvokeOnMainThread`. See
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on)
and
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).

### Registering decoders, and what happens when one is missing

Nothing in this family finds a decoder by reflection. `SkiaVideoRuntime.Initialize()` configures the audio
output at the rate the media actually carries, so no sample-rate conversion happens at all, registers the
AV1 decoder and registers the Opus decoder, and then asks whether AV1 really is supported rather than
trusting that the call did what it said. It is idempotent behind a lock, records a one-line `Summary` of
what it registered and an `ErrorMessage` of what went wrong, and the view model shows that message in the
status line instead of crashing. That shape - a static registrar in the library that owns playback, called
once, asserted afterwards - is the whole recipe. Read
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/SkiaVideoRuntime.cs`. See
[Turn on extra media codecs once at startup](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-extra-media-codecs-once-at-startup)
and
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).

### Two canvases in one cell, and settling on the one that started

This is the part most worth copying onto any Skia head that wants a graphics context. `MainPage.xaml`
declares an `SKXamlCanvas` inside a `Grid`; `MainPage.xaml.cs` builds a `SkiaGLCanvasElement` in code on
`Loaded` - in code rather than in XAML because its construction is what starts the graphics API, and a
failure inside `InitializeComponent` would take the window with it - and inserts it underneath. It then
waits briefly, because the element reports nothing until it has loaded and tried to start OpenGL, collapses
whichever canvas it did not settle on, and tells the view model which one it got exactly once. From then on
one invalidate method repaints whichever canvas is visible, and both paint handlers call the same
`DrawVideo` on the view model. The graphics context is handed to the presenter inside the GPU paint
handler, because that is the one moment the context is current. The page hands that invalidate over through
`ICanvasBridge`, the interface the view model implements for it, in the same `DataContextChanged` handler
that supplies the `XamlRoot` getter and the save dialog, so a head that supplies one of the three and not
the others wires exactly what it has.

The choice is also exposed to the person using the application, and that is deliberate: "GPU (auto)" takes
the device when there is one, "CPU" never does, and "GPU only (no fallback)" insists - so when there is no
device the presenter's own message appears in the status line rather than the picture quietly degrading.
The line beside the drop-down reports `ActiveRenderPath`, which is what is running, not what was asked for.
Read `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs` beside the render-path region of
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/VideoPlaybackController.cs`. See
[Offer a CPU fallback for a GPU rendering path behind one switch](../BLUEPRINTS-GraphicsAndRendering.md#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch),
[Let the page invalidate a canvas through a bridge interface](../BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface),
[Code to the higher-level graphics package and let the binding arrive transitively](../BLUEPRINTS-ProjectLayoutAndPackaging.md#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively),
[Settle between a GPU canvas and a CPU canvas and tell the view model which one started](../BLUEPRINTS-GraphicsAndRendering.md#settle-between-a-gpu-canvas-and-a-cpu-canvas-and-tell-the-view-model-which-one-started),
[Hand a video presenter the graphics context inside the paint handler that makes it current](../BLUEPRINTS-GraphicsAndRendering.md#hand-a-video-presenter-the-graphics-context-inside-the-paint-handler-that-makes-it-current)
and
[Guard an async void handler the platform calls](../BLUEPRINTS-MVVM.md#guard-an-async-void-handler-the-platform-calls).

### The lookup-table panel is a matrix, and the matrix is a testable class

The panel's rules are small and they are all decisions. The chain is applied when Play is pressed and only
then, so ticking a table changes the panel rather than the picture and the heading says how many tables are
waiting. The panel is read-only while the picture is running, because nothing should change a grade under a
running picture at a moment nobody chose. Reaching the end of a file counts as stopped, so the panel
becomes editable again the instant it happens - the only thing that can happen next is Play, and Play is
what applies a chain. The panel is off entirely whenever the picture is composed on the processor, with a
note saying why.

All of that lives in `LutPanelPolicy`, a static class of pure functions over two enums, which is what lets
the whole matrix be tested with no window and no video. The view model calls it from one place,
`UpdateUiState`, so there is exactly one path to the panel's enablement. `LutChain` beside it is the other
half: composing a chain walks tens of thousands of grid nodes, so the presenter's effects are rebuilt only
when the selection, the order or a percentage actually changed, which a signature string decides. Read
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Playback/LutPanelPolicy.cs`, then
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Effects/LutChain.cs`, then their test files. See
[Keep view model rules in a plain class so they can be tested](../BLUEPRINTS-Testing.md#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested),
[Show a panel only when the last operation left something to say](../BLUEPRINTS-ViewsAndControls.md#show-a-panel-only-when-the-last-operation-left-something-to-say)
and
[Rebuild an expensive effect chain only when its signature changes](../BLUEPRINTS-GraphicsAndRendering.md#rebuild-an-expensive-effect-chain-only-when-its-signature-changes).

### Baking a grade, and where the file is allowed to go

The Bake button writes the chain the panel holds - the ticked tables, at their percentages, in list order -
out as a single `.cube` file. Two things about it are worth stealing. First, Play and Bake are independent:
neither reads the other's result, and a chain that has never been played bakes perfectly well, because
composing a chain is arithmetic on the tables and owes nothing to the frame on screen. They agree anyway,
because both compose at the same size with the same sampling, which is what makes the scripted round trip -
bake a grade, feed the file back at full strength, compare the frames - a real check rather than a
tautology.

Second, the application never chooses a folder. `BakeLocations` supplies only a stamped file name, so two
bakes in a row do not silently propose the same file, and the page, through the `IFileSaveBridge` the view
model implements, opens the platform's own save dialog with no suggested start location, so the dialog
opens where that person last was. A cancel writes nothing and says nothing, because deciding not to save is
not a failure. The picker code is careful in two more ways: it marshals onto the user-interface thread
first, because a `SimpleCommand` makes no promise about which thread runs its handler and a dialog belongs
to the window it is shown over, and it deletes the empty placeholder the picker creates at a brand-new
name, so a bake that fails leaves nothing behind that looks like a result. Read `PickSaveCubePathAsync` in `src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs` and
`BakeChain` in `src/libs/SimpleCbxVideoPlayer.SkiaVideo/VideoPlaybackController.cs`. See
[Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns),
[Compose a chain of color lookup tables and write it back out as one file](../BLUEPRINTS-MediaAndVision.md#compose-a-chain-of-color-lookup-tables-and-write-it-back-out-as-one-file)
and
[Marshal a save dialog onto the UI thread from a command handler](../BLUEPRINTS-PlatformServices.md#marshal-a-save-dialog-onto-the-ui-thread-from-a-command-handler).

### The corpus is data the application carries, and a rule rather than a list

The clips and the tables are ordinary content: they sit under `src/SimpleCbxVideoPlayer.Core/Assets/`, the
Core csproj copies them to the output folder with one `None` item, and `SampleAssets` resolves them under
`AppContext.BaseDirectory`. No configuration file, no search of the file system, the same layout beside
every head's executable and beside the test binary. An application that has lost its `Assets` folder finds
nothing, which is why the status line says so rather than the drop-downs silently being empty.

What is offered is decided by a rule, not a list. `VideoCorpus` reads every sub-folder of the clip corpus
except the ones it names, and offers every file whose extension this family can open, so a folder added
tomorrow appears in the drop-down without a line of code changing. `LutCatalog` reads the generated tables
and then the found ones, searching recursively because the found ones keep a folder per upstream project,
and reads each file's `TITLE` line without parsing the table behind it - a large table is a megabyte of
numbers, and the full parse happens when a table is actually applied. Both classes are static, take a
folder and return a list, and are therefore tested against the real bundled files. Read
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/SampleAssets.cs`, then `VideoCorpus.cs` and
`LutCatalog.cs` beside it. See
[Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary),
[Ship a data corpus as content items and find it under the application base directory](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-data-corpus-as-content-items-and-find-it-under-the-application-base-directory)
and
[Offer a data folder by rule rather than by a hard-coded list](../BLUEPRINTS-DocumentsAndData.md#offer-a-data-folder-by-rule-rather-than-by-a-hard-coded-list).

### The transport, the clock and the scrub bar

Position comes from the audio clock while a soundtrack is playing, because that is what a listener is
actually hearing, so it advances in the mixer's steps rather than the decoder's. The scrub bar is bound two
way to a `double`, which means two small pieces of discipline. `PositionSeconds` and `DurationSeconds`
compare and notify by hand, with a comment saying why, because `SetProperty` has no overload for a value
type. And the setter seeks only when a person moved the bar: every time the view model sets the position
from a clock event it raises a suppression flag first, so the player is never asked to seek to where it
already is. Read the bindable-properties region and `SyncPosition` in
`src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`. See
[Notify a value typed bindable property by hand](../BLUEPRINTS-MVVM.md#notify-a-value-typed-bindable-property-by-hand),
[Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back)
and
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### A sample that can check itself

The parts a unit test cannot reach - a real head, a real canvas, a real decode, a real composed frame -
are still checked, by a scripted mode that drives the view model's own commands and properties from the
command line. It opens the window the same way a person would, plays a clip, seeks to a fixed position so
that two runs capture the same picture, writes the composed frame out, and prints one machine-readable line
per step before leaving with a status. Anything it was asked for and could not deliver - a command line it
could not read, a clip or a table that matches nothing, a chain that did not end up holding what was asked
for - fails the run with a message rather than being carried past as a warning. Nothing about the mode
changes what the application does when it is not asked for.

The options parser is a plain class in the library with no platform types in it, which is why it is the
most heavily tested piece of the application. The frame comparer beside it measures two pictures channel by
channel with a tolerance and reports the first differences with their values. Read
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Diagnostics/SmokeOptions.cs` and
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Diagnostics/ImageComparison.cs`, then the smoke region at the foot
of `src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`. See
[Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application),
[Compare rendered images pixel by pixel](../BLUEPRINTS-Testing.md#compare-rendered-images-pixel-by-pixel)
and
[Deliver a frame capture from inside the paint handler to the command that asked for it](../BLUEPRINTS-GraphicsAndRendering.md#deliver-a-frame-capture-from-inside-the-paint-handler-to-the-command-that-asked-for-it).

### What this application does not show

It is one page and one library, so come to it for playback, for the graphics context and for the grade, and
go elsewhere for the rest. It has:

- No file picker for opening media. The clip drop-down offers only what the application carries; there is
  no "open a file of your own", no drag and drop, no playlist and no recent-files list. The only path a
  person can name is a table fed to the scripted mode.
- No captions, chapters, multiple audio tracks, track selection, volume, mute, playback rate or looping.
  The transport is Play, Pause, Stop and seek, and nothing more.
- No settings and no persistence. The chosen clip, the render path and the ticked tables are not remembered
  between runs.
- No registered services. The registration lambda in `App.xaml.cs` is a comment, so the wiring is shown but
  no resolution is.
- No dialogs. The page wires an `IXamlRootGetter` so a view model could open one - it costs a line and is
  worth reading as the graceful-degradation pattern - but every message this application has to give goes
  into a status line instead. See
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- No converters, no custom controls, no styles or templates beyond one item template and four brushes, no
  second page and no navigation beyond the initial one, and no theming: the page pins its own dark panel
  colors rather than following the system preference.
- No effects on the processor path. The presenter can be told to allow them; this application deliberately
  leaves that alone, so that the difference between the two paths stays visible instead of being papered
  over.
- Nothing about the frame-buffer head's own picker and software keyboard. A bake there simply reports that
  it has no dialog to show. See
  [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder covers everything bundled here: the video
corpus under `src/SimpleCbxVideoPlayer.Core/Assets/authoring/`, which is Public Domain, and the color
lookup tables under `src/SimpleCbxVideoPlayer.Core/Assets/LUTs/`, which are a mixture of CC0, public-domain
and MIT files from six independent open-source projects plus a set generated by the script bundled beside
them. The verbatim license text of every upstream project sits in `Assets/LUTs/LICENSES/`, and the
corpus's own `README.txt` and `MANIFEST.txt` record, table by table, where each file came from and how its
license was checked. The notices file also states that code dependencies arrive as NuGet packages carrying
their own licenses and notices, so those are not reproduced there. Note that the CodeBrix packages this
application uses are not all under the same license; the family's package-name suffix records each one's
license, so check the suffix in each csproj before shipping a derivative.

## License

SimpleCbxVideoPlayer is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
