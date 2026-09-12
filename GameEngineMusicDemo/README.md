# GameEngineMusicDemo

GameEngineMusicDemo is a one-page control surface for a running game engine's music
system. The left of the window is a live engine canvas that draws a text readout -
what is playing, where it is, how many fades are in flight, whether a transition is
queued for the next bar, the current master, music, effects and duck levels, a
little meter per stem layer, and the beat grid each MIDI file reported about itself.
The right of the window is a scrolling panel of sliders and buttons, grouped by what
they do: volume buses, transport, quantized transitions, adaptive stems, stems from
a download, MIDI layers, ducking and stingers, jump points, and a global pause. Every
control calls one method on the music system, so the panel reads as a list of the
things the API can be asked to do. The sample writes every asset it plays on first
run, into a folder beside the executable, so there is nothing to download, nothing to
supply and nothing to configure before pressing a button.

It is this repository's reference for the CodeBrix.Platform.GameEngine music system:
buses and mixing, fades and equal-power crossfades, transitions quantized to a beat
or a bar, layered adaptive stems, a stems export loaded whole, MIDI rendered live
through an SFZ instrument and through a Decent Sampler instrument, per-channel MIDI
layering and tempo change without pitch change, ducking in both its fire-and-forget
and its held-handle form, stingers, playlists, marker jump points, and a global pause
that suspends the music and freezes the fades together. It is also a reference for
running the engine inside an ordinary XAML page rather than a game shell, and for an
application that builds the binary inputs it needs instead of shipping them. A
sibling application here, PalmVisualizer, runs the same engine for its rendering
rather than its audio.

## What this sample shows a CodeBrix.Platform developer

- Start the engine loop against a canvas the page owns, by forwarding the canvas's
  first real layout size to the view model through a one-method interface:
  [Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size).
- Keep the whole engine lifecycle behind one class with `Start()`, `Stop()` and a
  pause toggle, so nothing else in the application touches the engine singleton:
  [Run and pause a game engine session inside a page](../BLUEPRINTS-GameEngine.md#run-and-pause-a-game-engine-session-inside-a-page).
- Let one library own the engine package and have the Core project depend on that
  library rather than on the engine, with its own root namespace so the generated
  resources type is not declared twice:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- See what a single package reference actually brings with it, and decide when to
  name a transitive dependency directly:
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Build the binary inputs the code needs from arithmetic instead of committing them,
  here at application scope rather than in a test project:
  [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).
- Drive the whole application unattended from an environment switch and print
  machine-readable result lines, for behavior that can otherwise only be heard:
  [Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application).
- Carry every shared package in one Core library and give each head exactly one
  platform runtime package:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Organize an application as a shared UI project, a Core project, libraries under
  `src/libs` and mirrored test projects under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- File-link `App.xaml` and the views into all six executables through a shared items
  project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Set the Core library's root namespace to the application namespace, and write the
  assembly-qualified namespace the shared XAML then needs:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Keep one Windows-targeting head inside a solution that still restores on Linux and
  macOS:
  [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- Keep every head's `Program.Main` to the same handful of lines, differing only in
  the call that names the platform:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Cast the built host on the WinWpfSkia head to force its software render surface:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Do the four startup jobs in the `App` constructor in the order that matters:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Create the main window, put a `Frame` in it and navigate to the first page:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- Give `SimpleServiceResolver` a generic-host builder from one helper compiled once
  in the Core library:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Make a bundled font the default for every head through an `ms-appx:///` URI:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Install a console logger factory only in Debug builds, from a static method each
  head calls before building its host:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Guard a view model constructor against the XAML designer, and pair that guard with
  `SetIsDesignMode(false)` at startup:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Hand the view model a `XamlRoot` getter in the same handler that wires the canvas,
  so dialogs work the day one is added:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Set up an xUnit v3 test project that the family's runner actually discovers:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Give the library an `InternalsVisibleTo.cs` naming only its own test assembly, so
  an internal maintainer tool can still be tested:
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Pin the audio device format before a single voice exists, so sources that
  disagree about sample rate convert instead of being refused:
  [Pin the audio device format before anything plays](../BLUEPRINTS-GameEngine.md#pin-the-audio-device-format-before-anything-plays).
- Hand a beat grid to the tracks that cannot have one, and let the files that carry
  their own tempo, time signature and markers speak for themselves:
  [Tell the engine the tempo it cannot derive and let it derive the rest](../BLUEPRINTS-GameEngine.md#tell-the-engine-the-tempo-it-cannot-derive-and-let-it-derive-the-rest).
- Land a crossfade on the downbeat of a piece whose tempo changes partway through,
  and log what a fixed grid would have answered instead:
  [Quantize a music transition to the next bar across a tempo change](../BLUEPRINTS-GameEngine.md#quantize-a-music-transition-to-the-next-bar-across-a-tempo-change).
- Mix layers in one sample-locked voice, by index for a set you built and by name
  for a stems export loaded whole:
  [Crossfade layered stems and a stems export by name](../BLUEPRINTS-GameEngine.md#crossfade-layered-stems-and-a-stems-export-by-name).
- Duck the music under a one-shot cue, and under a line whose length only the voice
  knows, with the handle form that reference-counts:
  [Duck the music for exactly as long as a line lasts](../BLUEPRINTS-GameEngine.md#duck-the-music-for-exactly-as-long-as-a-line-lasts).
- Get a banner onto the one forced frame the engine renders on its way into a
  pause:
  [Write a pause overlay onto the engine's final forced frame](../BLUEPRINTS-GameEngine.md#write-a-pause-overlay-onto-the-engines-final-forced-frame).
- Write every binary input the application plays from arithmetic on first run,
  folder-shaped assets included:
  [Generate an application's whole asset set from arithmetic on first run](../BLUEPRINTS-ProjectLayoutAndPackaging.md#generate-an-applications-whole-asset-set-from-arithmetic-on-first-run).
- Check the behavior that can only be heard with an opt-in walkthrough that drives
  the same methods the controls drive and writes machine-readable lines:
  [Check audible-only behavior with an opt-in unattended walkthrough](../BLUEPRINTS-Testing.md#check-audible-only-behavior-with-an-opt-in-unattended-walkthrough).

## Building, running and testing

There is one solution, `GameEngineMusicDemo.slnx`, and it holds everything: the
shared UI project, the Core project, all six heads, the one library under a
`Libraries` solution folder and its test project under a `Tests` solution folder. Its
header comment describes it as everything that builds with the plain .NET SDK on
Linux, macOS and Windows, which holds here because every head is a Skia head. There
is no WinUI 3, WPF or .NET MAUI head and no second solution.

| Head project | Platform | Host-builder call |
| --- | --- | --- |
| `src/GameEngineMusicDemo.LinuxX11` | Linux, X11 | `UseLinuxX11()` |
| `src/GameEngineMusicDemo.LinuxWayland` | Linux, Wayland | `UseLinuxWayland()` |
| `src/GameEngineMusicDemo.LinuxFrameBuffer` | Linux, framebuffer (no display server) | `UseLinuxFrameBuffer()` |
| `src/GameEngineMusicDemo.MacOS` | macOS | `UseMacOS()` |
| `src/GameEngineMusicDemo.Win32Skia` | Windows, Win32 window | `UseWindowsWin32()` |
| `src/GameEngineMusicDemo.WinWpfSkia` | Windows, Skia hosted in a WPF window | `UseWindowsWpf()` |

Every `Program.cs` is the same but for that call: it calls `App.InitializeLogging()`
first, then builds the host with `UseDirectSkiaCanvasMode()` before `Build()`. The
WinWpfSkia head adds one block after `Build()` that forces the software render
surface.

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting`, so the solution still restores
  and builds on Linux and macOS. No workload is needed.
- All CodeBrix code arrives from NuGet. No CodeBrix library is referenced as a source
  project, so this folder builds on its own.
- An audio output device, if you want to hear any of it. Everything this sample
  demonstrates is audible, which is exactly why the game class also writes a line per
  loaded track and per quantized transition to the log, and why the readout on the
  canvas mirrors the mixer state: on a machine with no sound output the log and the
  readout are the evidence that the music system did what it says.
- Nothing else. No accounts, no tokens, no network access and no media files. On its
  first run the sample writes the layers, the two linear tracks, the stinger, the
  dialogue blip, an SFZ instrument, a Decent Sampler instrument, two MIDI files and a
  whole stems export into a `GeneratedMusic` folder beside the executable, and skips
  the work entirely on later runs. Delete that folder to have it written again.

To run one head from the command line, from this folder:

```text
dotnet run --project src/GameEngineMusicDemo.LinuxX11
dotnet run --project src/GameEngineMusicDemo.LinuxWayland
dotnet run --project src/GameEngineMusicDemo.LinuxFrameBuffer
dotnet run --project src/GameEngineMusicDemo.MacOS
dotnet run --project src/GameEngineMusicDemo.Win32Skia
dotnet run --project src/GameEngineMusicDemo.WinWpfSkia
```

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` sits inside `#if DEBUG` - so a Release run is silent, and
the log lines the game class writes about tempo grids and quantized waits appear only
in a Debug run.

One test project mirrors the one library. It covers the asset factory's layout
contract - where the generated folder sits, that the layer names and the layer file
names describe the same three files, that the stems export is deliberately written at
a rate the device is not pinned to, that the Decent Sampler preset sits in its own
folder beside its samples, and that the tempo-change file really does change tempo -
plus the opt-in switch on the maintainer walkthrough. None of it needs a window, a
sound card or a running engine.

There is no `global.json` in this application. The Microsoft.Testing.Platform runner
is selected by two properties in the test csproj instead,
`UseMicrosoftTestingPlatformRunner` and `TestingPlatformDotnetTestSupport`, alongside
an `OutputType` of `Exe`, because xUnit v3 test projects are self-executing binaries.
The family caveat applies here in its strongest form: a plain `dotnet test` does not
run this suite. Build the test project and run the executable it produces:

```text
dotnet build tests/libs/GameEngineMusicDemo.Game.Tests/GameEngineMusicDemo.Game.Tests.csproj
./tests/libs/GameEngineMusicDemo.Game.Tests/bin/Debug/net10.0/GameEngineMusicDemo.Game.Tests
```

What a unit test cannot reach is the part that only exists when the engine is running
and audio is decoding: whether each instrument format actually loaded, what each MIDI
file said about its own tempo, and whether a transition quantized to the next bar
lands where a tempo map says it should rather than where a fixed grid would guess. A
scripted, unattended walkthrough of exactly those behaviors lives in
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs`. It is a
maintainer tool rather than a feature: it is off unless an environment variable the
file names asks for it, it drives the same methods the buttons drive, and it writes
one line per check and then a single result line to the log. Nothing about the
application changes when the variable is not set.

## How the projects and folders are organized

```text
GameEngineMusicDemo/
  GameEngineMusicDemo.slnx                  The one solution: UI, Core, six heads, the library, the test project
  THIRD-PARTY-NOTICES.txt                   Third-party content used by this application
  src/
    GameEngineMusicDemo.UI/                 Shared items project: the XAML every head compiles
      GameEngineMusicDemo.UI.shproj         Shared-project shell, so an IDE can load the folder as a project
      GameEngineMusicDemo.UI.projitems      The shared file list each head imports with Label="Shared"
      App.xaml                              Merged WinUI resources and the Open Sans FontFamily resource
      App.xaml.cs                           Bootstrap: default font, service resolver, design mode, window and frame, logging
      Views/MainPage.xaml                   The whole UI: the engine canvas and the scrolling control panel
      Views/MainPage.xaml.cs                One handler per control, each calling one music-system method
    GameEngineMusicDemo.Core/               Class library; carries every non-head package
      GameEngineMusicDemo.Core.csproj       RootNamespace GameEngineMusicDemo; framework, font, hosting and logging packages
      Helpers/HostHelper.cs                 The IHostBuilderProvider SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs           Owns the demo object; declares IManageGameCanvas
    GameEngineMusicDemo.LinuxX11/           Head: Program.cs plus a csproj with one runtime package
    GameEngineMusicDemo.LinuxWayland/       Head: Program.cs plus a csproj with one runtime package
    GameEngineMusicDemo.LinuxFrameBuffer/   Head: Program.cs plus a csproj with one runtime package
    GameEngineMusicDemo.MacOS/              Head: Program.cs plus a csproj with one runtime package
    GameEngineMusicDemo.Win32Skia/          Head: Program.cs plus a csproj with one runtime package
    GameEngineMusicDemo.WinWpfSkia/         Same, plus net10.0-windows and a software render surface
    libs/
      GameEngineMusicDemo.Game/             The library that owns the engine reference
        GameEngineMusicDemoGame.cs          Engine start-up, the tracks, the readout, one method per control
        GameEngineMusicDemoWalkthrough.cs   Internal, opt-in, unattended check of the audible-only behavior
        MusicAssetFactory.cs                Writes every asset the sample plays, once, from arithmetic
        InternalsVisibleTo.cs               Names only this library's own test assembly
  tests/
    libs/
      GameEngineMusicDemo.Game.Tests/       Mirrors src/libs/GameEngineMusicDemo.Game
```

Dependency direction is strictly one way. Each head project references
`GameEngineMusicDemo.Core` by project reference and file-links the shared UI through
an `Import` of `GameEngineMusicDemo.UI.projitems`, so `App.xaml`, `App.xaml.cs`,
`MainPage.xaml` and `MainPage.xaml.cs` are compiled into every head rather than into
a library. That is why every head csproj repeats the `<Page Include="**\*.xaml" />`
and `<None Remove="**\*.xaml" />` pair. `GameEngineMusicDemo.Core` project-references
the one library and carries the framework, font, hosting and logging packages, which
reach the heads transitively; the library carries the engine. Because the XAML is
compiled inside the head assembly while the view model lives in the library, the page
reaches it with an assembly-qualified `clr-namespace`
(`xmlns:vm="clr-namespace:GameEngineMusicDemo.ViewModels;assembly=GameEngineMusicDemo.Core"`),
and reaches the engine's canvas control the same way. Nothing flows back: the library
knows nothing about the view model, and the Core project knows nothing about the UI.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The application framework: `Application`, `Window`, `Frame`, `Page`, the controls on the panel, the default-font feature configuration, and the "Simple" toolkit (`SimpleViewModel`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `CodeBrixPlatformHostBuilder`) | `src/GameEngineMusicDemo.Core/GameEngineMusicDemo.Core.csproj`, `src/GameEngineMusicDemo.UI/`, `src/GameEngineMusicDemo.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface; `CodeBrixPlatformHostBuilder` selects it in `Program.Main` | The six head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.OpenSans | Ships the Open Sans font set as the application-wide default and as the page's `FontFamily`, addressed through an `ms-appx:///` URI | `src/GameEngineMusicDemo.Core/GameEngineMusicDemo.Core.csproj`, `src/GameEngineMusicDemo.UI/App.xaml`, `src/GameEngineMusicDemo.UI/App.xaml.cs`, `src/GameEngineMusicDemo.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.GameEngine | The whole subject of the sample: `AudioSystem`, `AudioMixer`, `AudioResourceManager`, `CachedSound`, `MusicManager`, `MusicTimeline`, `FileMusicTrack`, `MidiMusicTrack`, `MusicStemSet`, `MusicPlaylist` and `MusicTransitionQuantize`, plus the engine loop, `GameSurfaceCanvas`, the render-surface host, the view manager and the direct drawings the readout is made of. One package supplies both the engine core and the Host layer | `src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemo.Game.csproj`, `src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`, `src/GameEngineMusicDemo.UI/Views/MainPage.xaml` |
| CodeBrix.Audio | Arrives with the engine package, and the asset factory uses it directly: the WAV writer and sample-provider types, and the MIDI event, tempo, time-signature, marker and file-export types | `src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs` |
| SilverAssertions | The assertion style in the test project | `tests/libs/GameEngineMusicDemo.Game.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | `SKTypeface` and the text-alignment types the engine readout is configured with; it arrives with the engine package | `src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the container | `src/GameEngineMusicDemo.Core/GameEngineMusicDemo.Core.csproj`, `src/GameEngineMusicDemo.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging | The engine's logger is what every diagnostic line in this sample is written to | `src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`, `src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider wired into the platform's ambient logger in Debug builds | `src/GameEngineMusicDemo.Core/GameEngineMusicDemo.Core.csproj`, `src/GameEngineMusicDemo.UI/App.xaml.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for the test project | `tests/libs/GameEngineMusicDemo.Game.Tests/GameEngineMusicDemo.Game.Tests.csproj` |

## Worth studying in this application

### One method per control, and why there are no commands here

This application deliberately breaks the family's usual shape in one place, and says
so in the code. `MainPage.xaml.cs` wires each button and slider straight to a method
on the demo object instead of binding a `SimpleCommand`, because what the sample is
for is showing the music API being called: one line per control, with nothing in
between to read past. The view model stays a real `SimpleViewModel` with the
design-mode guard as its first constructor line, and it owns the demo object; the
page reads `Demo` directly rather than binding to it, which is why that property
needs no change notification. Every handler is null-safe through a `Demo` property
that returns null until the canvas has started, because every control is reachable
before then.

Read `src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs` beside
`src/GameEngineMusicDemo.Core/ViewModels/MainViewModel.cs`. If you are building an
ordinary application rather than an API demonstration, take the shape from
MediaPlayerDemo or PalmVisualizer instead and keep the behavior in commands. See
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
and
[Crossfade layered stems and a stems export by name](../BLUEPRINTS-GameEngine.md#crossfade-layered-stems-and-a-stems-export-by-name).

### Starting the engine, and pinning the device before anything plays

`GameEngineMusicDemoGame.Start()` is the whole start-up sequence and its order
matters. It writes the assets if they are missing, then pins the audio device format
with `AudioSystem.Initialize()` before a single voice exists, then takes the canvas's
render-surface host, configures a single full view, subscribes the engine's
per-cycle event to refresh the readout, starts the engine on the current
synchronization context, drops the target frame rate because a text readout does not
need sixty frames a second, and only then builds the tracks and the readout display.

Pinning the device first is the part worth copying. It is what lets a stem set of
assorted source rates line up at all: stems rate-convert to the pinned rate as they
decode, instead of being rejected for disagreeing with each other. The generated
stems export is written at a deliberately different rate from the one the sample pins,
so that conversion is exercised every run rather than assumed.

The canvas cannot be started before it has a non-zero size, so the page forwards the
canvas's `FirstStarted` event to the view model through `IManageGameCanvas` in one
line and the view model builds and starts the demo there. Read the tail of
`src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs`, then `Start()` in
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`. See
[Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size),
[Run and pause a game engine session inside a page](../BLUEPRINTS-GameEngine.md#run-and-pause-a-game-engine-session-inside-a-page)
and
[Pin the audio device format before anything plays](../BLUEPRINTS-GameEngine.md#pin-the-audio-device-format-before-anything-plays).

### What the engine is told, and what it refuses to guess

`BuildTracks()` is the most quotable part of the sample, because it shows where the
line falls between what the music system derives and what the game has to say. A
track built over decoded audio is given a `MusicTimeline` by hand, with a comment
saying why: decoded audio cannot be asked what tempo it is, the composer knows, and
the engine deliberately refuses to guess. A MIDI track loaded from a path derives its
own timeline - tempo, time signature and the markers that become jump points - with
nothing set here at all. A stems export loaded from a folder derives its grid from
the MIDI that sits beside the recordings, so bar-locked layer changes and
bar-quantized transitions work with nothing else arranged.

`LogWhatWasLoaded()` then writes one line per loaded track, and that is a habit worth
stealing rather than an incidental. Which instrument format was used, what the file
said about its own tempo, how many markers came out of it, and how many problems the
reader recorded are all invisible on a screen full of buttons, and all of them are
the first thing you want when a file does not behave. See
[Tell the engine the tempo it cannot derive and let it derive the rest](../BLUEPRINTS-GameEngine.md#tell-the-engine-the-tempo-it-cannot-derive-and-let-it-derive-the-rest).

### Quantized transitions, and a grid that follows a tempo map

An immediate crossfade and a bar-quantized one sit next to each other on the panel so
the difference is audible in one click each. `LogQuantisedWait()` is where the sample
earns its keep: before queueing a quantized transition it asks the current track's
timeline how long the wait to the next boundary actually is, works out what a grid
fixed at the file's opening tempo would have answered instead, and logs both. On the
Decent Sampler theme, whose file changes tempo partway through, the two answers
differ, and that difference is the entire reason a timeline is a tempo map rather
than a number. A quantized transition asked for with no grid to wait on is not an
error: it runs immediately, and says so.

Read `CrossfadeToTrackB`, `CrossfadeToTrackA` and `LogQuantisedWait` in
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`. See
[Quantize a music transition to the next bar across a tempo change](../BLUEPRINTS-GameEngine.md#quantize-a-music-transition-to-the-next-bar-across-a-tempo-change).

### Stems: three files in one voice, and a whole export

There are two stem features here and they are worth reading as a pair. The first is a
stem set built by hand from three decoded layers that share a length, a rate and a
channel count; the page's sliders write each layer's gain directly, and the two fade
buttons call the layer's own timed fade instead. The code-behind makes a small point
about that: after starting a fade it moves the slider to the destination rather than
tracking the fade, because a slider that keeps showing where the layer was would
fight the fade it just started.

The second is the same feature over a stems export - the shape a music service hands
over, several stem files named for each other with a MIDI file beside them - loaded
whole in one call, with the layer names given as arguments and the beat grid taken
from the export itself. Nothing else is set up, and the per-layer fades then work by
name.

Read `PlayStems`, `PlaySongStems` and `FadeSongStem` in
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`, then `SetStemGain`,
`FadeStem` and `SyncStemSliders` in
`src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs`. See
[Crossfade layered stems and a stems export by name](../BLUEPRINTS-GameEngine.md#crossfade-layered-stems-and-a-stems-export-by-name).

### Ducking in both of its forms

The panel offers ducking three ways because the API has two shapes and the difference
is a lifetime question. A stinger ducks the music for its own length and needs
nothing released. A dialogue line uses the handle form: the demo clones the voice
resource so overlapping lines do not fight over one instance, pushes a duck, and
disposes the duck and unloads the clone from the voice's own completion event, so the
duck lasts exactly as long as the line and overlapping lines reference-count
correctly. The third button holds a duck open until a fourth releases it, which is the
same handle form with the lifetime made visible.

Read `PlayStinger`, `PlayDuckedDialogue`, `HoldDuck` and `ReleaseHeldDuck` in
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`. See
[Duck the music for exactly as long as a line lasts](../BLUEPRINTS-GameEngine.md#duck-the-music-for-exactly-as-long-as-a-line-lasts).

### Global pause, and writing the overlay before you pause

Pausing the engine suspends the music and freezes every fade in flight, and a
transition queued for the next bar cannot fire while paused. The sharp edge is in
`TogglePause()`: the readout is refreshed from the engine cycle, and the cycle parks
while paused, so the "PAUSED" banner cannot be written after the pause. The engine
renders one forced final frame on its way in - which is what makes pause overlays
possible at all - and writing the text immediately before calling pause is what gets
it onto that frame. Queue a bar-quantized transition, then pause, and the readout
still says a transition is queued: that is the contract being demonstrated rather
than described. See
[Write a pause overlay onto the engine's final forced frame](../BLUEPRINTS-GameEngine.md#write-a-pause-overlay-onto-the-engines-final-forced-frame).

### Generating every asset from arithmetic

`MusicAssetFactory` writes the layers, the two linear tracks in different keys, the
stinger, the dialogue blip, an SFZ instrument and its one-second tone, a Decent
Sampler instrument with its `Samples` folder, two MIDI files and a stems export, all
from sums, and skips anything already on disk. The class comment states the reason:
this application ships no binary music assets, and a sample that needs some cannot be
run by anyone who does not have them.

Two details in it are worth more than the waveform arithmetic. The first is that two
of the assets are folders rather than files - a Decent Sampler preset points at
sample files beside it, and a stems export is a set of files that belong together and
are named for each other - and both are written the way the real thing is laid out,
because that is the part an application gets wrong. The second is in `AddNote()`: a
note is two events, and the export step sorts and closes a track but does not release
anything, so a note added without its off event is still sounding when the track ends,
which the reader then reports as a problem with the file. Looping stems also get a
few milliseconds of fade at each end so the seam does not click.

Read `src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs`. See
[Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them)
and
[Generate an application's whole asset set from arithmetic on first run](../BLUEPRINTS-ProjectLayoutAndPackaging.md#generate-an-applications-whole-asset-set-from-arithmetic-on-first-run).

### What each project owns

Three project-file rules do the structural work. Every head carries exactly one
platform runtime package and nothing else, with the comment `EXACTLY ONE platform
head package` in all six, and everything shared arrives from
`GameEngineMusicDemo.Core`. The game library, not the Core project, owns the engine
reference, and sets an explicit root namespace distinct from the application's, with
a comment explaining that a library which also sees CodeBrix.Platform would otherwise
generate a duplicate of the same generated resources type as the Core project, and
the head referencing both would fail to compile. And the library names one package
while its code uses types from two families: the engine brings the audio library with
it, which is how the asset factory writes WAV and MIDI files without the project file
mentioning either. See
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace),
[Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on)
and
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).

### Testing something that can only be heard

The test project tests the library, never the UI, and it tests the part of the
library that is pure: where the generated assets go, that the layer names and their
file names agree, that the export rate and the pinned device rate are deliberately
different, and that the tempo-change file really changes tempo. It also reaches an
`internal` type, which is what the library's `InternalsVisibleTo.cs` is for, and
asserts that the maintainer walkthrough is off unless it is asked for by its exact
opt-in value - the guarantee that a person running the demo never sees it.

Everything else about this application is audible, and audible behavior is what the
unattended walkthrough covers instead: it drives the same methods the buttons drive
from a worker thread, checks each claim against what the engine reports, and writes
machine-readable lines. Read the test files, then
`src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs`. See
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application)
and
[Check audible-only behavior with an opt-in unattended walkthrough](../BLUEPRINTS-Testing.md#check-audible-only-behavior-with-an-opt-in-unattended-walkthrough).

### What this application does not show

It is a reference for one subsystem, so come to it for the music system and go
elsewhere for the rest. It has:

- No game. There is no scene, no sprite, no tile map, no physics, no input handling
  and no gameplay of any kind. The engine runs so that the music system has a real
  host and a real pause to demonstrate against, and the only thing drawn is a text
  readout on a panel.
- No commands and no bindings. The page has a data context and a view model, but
  every control is a `Click` or `ValueChanged` handler; there is no `SimpleCommand`,
  no `[AffectsCommands]`, no two-way binding and no converter anywhere.
- No teardown from the UI. The demo object exposes a `Stop()` that disposes the music
  manager, the tracks and the stem sets, stops the engine and shuts the audio system
  down, but nothing in this application calls it: the page has no unloaded handler
  and the view model has no `Dispose()` override. In a real application that cleanup
  belongs on the way out, and that wiring is the first thing to add if you copy this.
- No registered services. The registration lambda in `App.xaml.cs` is a comment, so
  the wiring is shown but no resolution is.
- No settings or persistence, so nothing about the session is remembered between
  runs, and no file dialogs: the sample plays only the assets it wrote itself.
- No 3D, no shaders, no custom drawing beyond the engine's own rectangle and text
  primitives, and no gamepad, effects processing, spatial audio or recording.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder is the attribution record for this
application: nothing third-party is bundled here, every code dependency arrives as a
NuGet package carrying its own license and notices, and the audio the application
plays is written by the application itself on first run.

## License

GameEngineMusicDemo is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
