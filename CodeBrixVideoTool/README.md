# CodeBrixVideoTool

CodeBrix Video Tool is a desktop video converter and player. Opening a file adds
it to a list down the left of the window, with a format badge, its name and a
one-line summary of what is inside it. Selecting a row loads that file into the
player on the right, which has a scrubber, timecodes, play, pause and stop, a
mute box and a volume slider, and two drop-downs that appear only when the open
file has something to put in them: one for chapters, one for caption tracks.
Under the picture sits the operation panel: pick a destination format, a size
from a resolution ladder and one of four quality stops, then press the action
button, whose label reads Import, Transcode or Export depending on where the
file is coming from and where it is going. A progress bar and a live Cancel
appear while the conversion runs, and when it finishes the result joins the file
list as a new source and the panel prints the notes the run left behind.

The application works with four container formats, all of them carrying AV1
video: standard Matroska (`.mkv`), standard WebM (`.webm`), CodeBrix Mode 1 (a
`.cbv` file that is a WebM constrained to the streamable profile) and CodeBrix
Mode 2 (a `.cbv` file in the bespoke CBVF container). It also imports from and
exports to the `.mp4` family, which it never plays, because the in-application
player decodes AV1 and nothing else. It is the reference for hosting the
CodeBrix.Platform VideoPlayer add-in in a page and driving it from a view model,
for playing and authoring CodeBrix `.cbv` video, and for running long FFmpeg
work with progress and cancellation from a SimpleViewModel.

## What this sample shows a CodeBrix.Platform developer

- Put the VideoPlayer add-in on a page and let a view model own what is open,
  what the transport may do and which chapter and caption track are showing:
  [Host the VideoPlayer add-in in a page and drive it from the view model](../BLUEPRINTS-MediaAndVision.md#host-the-videoplayer-add-in-in-a-page-and-drive-it-from-the-view-model).
- Turn on the AV1 and Opus decoders once at start-up, because they are the
  application's dependencies and never the add-in's:
  [Turn on extra media codecs once at startup](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-extra-media-codecs-once-at-startup).
- Find out what is inside a media file through an interface the view model
  resolves, with two routes behind it:
  [Probe a media file behind an interface the view model resolves](../BLUEPRINTS-MediaAndVision.md#probe-a-media-file-behind-an-interface-the-view-model-resolves).
- Write CodeBrix `.cbv` video in either container mode from a plan the view
  model settled first:
  [Author a cbv file in either container mode from a settled plan](../BLUEPRINTS-MediaAndVision.md#author-a-cbv-file-in-either-container-mode-from-a-settled-plan).
- Build an FFmpeg command line - inputs, stream selection, codecs, filters,
  progress and cancellation - from a service a view model drives:
  [Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library](../BLUEPRINTS-MediaAndVision.md#export-an-mp4-with-ffmpeg-through-the-codebrix-videoprocessing-library).
- Take a bespoke container apart into elementary streams and re-wrap them so an
  external tool that cannot open it can still read the media:
  [Demultiplex a bespoke container and remux it so an external tool can read it](../BLUEPRINTS-MediaAndVision.md#demultiplex-a-bespoke-container-and-remux-it-so-an-external-tool-can-read-it).
- Lift embedded chapters and captions out of a source into the separate input
  files an encoder insists on:
  [Lift chapters and captions out of a source into sidecar files](../BLUEPRINTS-MediaAndVision.md#lift-chapters-and-captions-out-of-a-source-into-sidecar-files).
- Tell two formats that share an extension apart by reading the first bytes of
  the file, the way the reader will:
  [Detect a container from its first bytes](../BLUEPRINTS-MediaAndVision.md#detect-a-container-from-its-first-bytes).
- Offer downscale choices that read correctly for portrait video as well as
  landscape, with every dimension even:
  [Build a resolution ladder keyed on the short side with even dimensions](../BLUEPRINTS-MediaAndVision.md#build-a-resolution-ladder-keyed-on-the-short-side-with-even-dimensions).
- Make a quality choice mean one thing across two different encoders by moving
  one knob and pinning the rest:
  [Move one encoder knob and pin everything else](../BLUEPRINTS-MediaAndVision.md#move-one-encoder-knob-and-pin-everything-else).
- Run a long job from a command with a progress bar, a live Cancel and every
  other command disabled:
  [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- Report honest progress when one stage of an operation knows its percentage and
  another cannot:
  [Report progress across stages when only some of them know a percentage](../BLUEPRINTS-MVVM.md#report-progress-across-stages-when-only-some-of-them-know-a-percentage).
- Answer "can this be done, and what exactly will happen" in one testable place
  before any of it runs:
  [Settle an operation in a plan before running any of it](../BLUEPRINTS-MVVM.md#settle-an-operation-in-a-plan-before-running-any-of-it).
- Turn a dozen library-specific failures into one exception type whose message
  the status bar can show as it stands:
  [Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch).
- Split a window into halves that each own real state, with a parent view model
  holding the one thing they share:
  [Compose a page from a parent view model and child view models](../BLUEPRINTS-MVVM.md#compose-a-page-from-a-parent-view-model-and-child-view-models).
- Rebuild two dependent drop-downs from static rules whenever the selection
  changes, so only sensible choices are offered:
  [Offer only the choices that make sense for the current selection](../BLUEPRINTS-MVVM.md#offer-only-the-choices-that-make-sense-for-the-current-selection).
- Let a drop-down follow a control it also drives, without commanding the
  control back:
  [Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).
- Write bound properties, `SimpleCommand` commands and `[AffectsCommands]`
  attributes the way the family does:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Notify a `double` or `enum` bindable property by hand where `SetProperty` has
  no overload for it:
  [Notify a value typed bindable property by hand](../BLUEPRINTS-MVVM.md#notify-a-value-typed-bindable-property-by-hand).
- Keep a view-model constructor safe for the XAML designer with one guard line:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Ask a person which file to open from a command, through a bridge the head
  fills in and the view model degrades without:
  [Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model).
- Choose where a long operation writes its result, and write beside the source
  when the head has no save dialog:
  [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Hand a view model a `XamlRoot` getter so its dialogs have somewhere to attach:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Bind a scrubber, timecodes and a volume slider straight to a media element's
  dependency properties, and know why that is the right exception:
  [Bind a scrubber and volume slider straight to the media element](../BLUEPRINTS-ViewsAndControls.md#bind-a-scrubber-and-volume-slider-straight-to-the-media-element).
- Give an output panel no room at all until the last operation has something to
  say:
  [Show a panel only when the last operation left something to say](../BLUEPRINTS-ViewsAndControls.md#show-a-panel-only-when-the-last-operation-left-something-to-say).
- Dim a list row for something the application cannot act on, without hiding it
  or making it unselectable:
  [Dim a list row for an item the application cannot act on](../BLUEPRINTS-ViewsAndControls.md#dim-a-list-row-for-an-item-the-application-cannot-act-on).
- Format a `TimeSpan` for display with an `IValueConverter` declared once in
  `Page.Resources`:
  [Format a value for display with an IValueConverter](../BLUEPRINTS-ViewsAndControls.md#format-a-value-for-display-with-an-ivalueconverter).
- Re-key the theme's own selection brushes so light text in a dark list stays
  readable when a row is selected:
  [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- Start each head from a tiny `Program.Main` that differs only in which platform
  backend it selects:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Do the whole of start-up in the `App` constructor, in the order the platform
  needs it:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Give `SimpleServiceResolver` a generic host builder through a one-class
  `IHostBuilderProvider`:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Register an application's services straight in the resolver callback when
  there is no library boundary to respect:
  [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- Turn on filtered console logging in Debug builds only, from each head's `Main`
  before the host is built:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Set a bundled font as the default and list the fallback faces consulted for
  glyphs it does not have:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Keep the rules a view model applies in plain static classes, because a
  SimpleViewModel cannot be constructed in a test process:
  [Keep view model rules in a plain class so they can be tested](../BLUEPRINTS-Testing.md#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested).
- Set the one csproj property that swaps reference assemblies for real ones in a
  test project:
  [Build a test project against real CodeBrix Platform assemblies](../BLUEPRINTS-Testing.md#build-a-test-project-against-real-codebrix-platform-assemblies).
- Share one expensive `IAsyncLifetime` fixture across every test class that
  needs it:
  [Share one expensive fixture across every test class that needs it](../BLUEPRINTS-Testing.md#share-one-expensive-fixture-across-every-test-class-that-needs-it).
- Generate the real media a test suite needs from synthetic sources instead of
  committing binary files:
  [Generate real media clips from a synthetic source](../BLUEPRINTS-Testing.md#generate-real-media-clips-from-a-synthetic-source).
- Drive the whole application end to end on a real head, from environment
  variables, through the view model's own commands:
  [Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application).
- Set up an xUnit v3 test project the way the family does, and know how to run
  it when a plain `dotnet test` finds nothing:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Give each library an `InternalsVisibleTo.cs` naming its own test assembly:
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Organize an application as `src/libs` plus `tests/libs` around a shared UI
  project:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Carry every package in the Core library and give each head exactly one runtime
  package:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- File-link `App.xaml` and the views into every head with a shared project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Keep a library's own `RootNamespace` when it references CodeBrix.Platform, so
  the generated per-head resources class does not collide:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- Set the Core library's `RootNamespace` to the application namespace so its
  types need no extra using:
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).

## Building, running and testing

There is one solution file, and it holds everything.

| Solution | Contains | Open on |
| --- | --- | --- |
| `CodeBrixVideoTool.slnx` | The shared UI project, the Core library, all four heads, both libraries under a `Libraries` solution folder and both test projects under a `Tests` solution folder | Any OS - its own comment describes it as everything that builds with the plain .NET SDK on Linux, macOS and Windows |

Four of the six CodeBrix.Platform heads are present. There is no WinWpfSkia head
and no LinuxFrameBuffer head, and there are no native (non-Skia) heads: no
WinUI 3, WPF or .NET MAUI project exists here.

| Head project | Platform | Host-builder call |
| --- | --- | --- |
| `src/CodeBrixVideoTool.LinuxX11` | Linux, X11 | `UseLinuxX11()` |
| `src/CodeBrixVideoTool.LinuxWayland` | Linux, Wayland | `UseLinuxWayland()` |
| `src/CodeBrixVideoTool.MacOS` | macOS | `UseMacOS()` |
| `src/CodeBrixVideoTool.Win32Skia` | Windows, Win32 | `UseWindowsWin32()` |

All four `Program.cs` files are the same but for the head call; each one calls
`App.InitializeLogging()` first, then `UseDirectSkiaCanvasMode()` before
`Build()`.

### Prerequisites

- The .NET 10 SDK. Every project targets `net10.0`.
- **FFmpeg and ffprobe on the path.** The Processing library runs them through
  CodeBrix.VideoProcessing, and `MediaProbe` describes them as the only external
  process this application ever starts. They are needed to probe anything that
  is not a `.cbv` file, to run any conversion at all, to remux the intermediate
  on the Mode 2 route, to pull a caption track out of an `.mp4` as WebVTT, and
  to generate the synthetic clips the tests and the scripted run use.
- **What still works without them.** A `.cbv` file - either mode - is read
  entirely by the playback core's own container readers, so it opens, lists and
  plays with no external process; the window, the file list, the player, the
  transport, the chapter and caption drop-downs and the theme are all unaffected.
  There is no availability check anywhere in the application: a missing tool
  surfaces as the exception the library throws, `MediaProbe` restates it as a
  `VideoToolProcessingException` naming the file, and the view model puts that
  one sentence in the status bar. Opening an `.mp4`, `.mkv` or `.webm` file, and
  pressing the action button on anything at all, therefore end in a failure
  sentence in the status bar rather than in a crash.
- No accounts, tokens, hardware, downloaded assets or data you have to supply.
  The AV1 and Opus decoders arrive as native libraries with the application's own
  packages and need nothing installed.

### Running a head

From this folder:

```text
dotnet run --project src/CodeBrixVideoTool.LinuxX11
```

Substitute `CodeBrixVideoTool.LinuxWayland`, `CodeBrixVideoTool.MacOS` or
`CodeBrixVideoTool.Win32Skia` for the other heads.

### The scripted run

Any head will drive itself end to end when `CODEBRIXVIDEOTOOL_SMOKE` is set; the
other three variables tune the run. They are read in
`src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`, and the run prints
`CBVT-SMOKE:` lines and exits with 0 or 1. With the first variable unset the
application behaves exactly as it always does.

| Variable | Meaning |
| --- | --- |
| `CODEBRIXVIDEOTOOL_SMOKE` | The destination to script: `mode1`, `webm`, `mkv` or `matroska`, or anything else for Mode 2 |
| `CODEBRIXVIDEOTOOL_SMOKE_OUT` | A working folder; a temporary one is made when it is not given |
| `CODEBRIXVIDEOTOOL_SMOKE_KEEP` | `1` or `true` to keep the files it wrote |
| `CODEBRIXVIDEOTOOL_SMOKE_HOLD` | Seconds to leave the window up, clamped to 0-300 |

### Tests

Two test projects mirror the two libraries. `CodeBrixVideoTool.Playback.Tests`
covers the codec registration, the chapter and caption row models and every rule
in `PlaybackSelection`. `CodeBrixVideoTool.Processing.Tests` covers the format
policy, the resolution ladder, the planner, the probe against real generated
media, the runner end to end, and the view model's static outcome-description
rule.

There is **no `global.json` in this application**. The
Microsoft.Testing.Platform runner is selected by two properties in each test
csproj instead - `UseMicrosoftTestingPlatformRunner` and
`TestingPlatformDotnetTestSupport`, alongside `OutputType` of `Exe`, because
xUnit v3 test projects are self-executing binaries. The usual family caveat
still applies: when `dotnet test` reports that zero tests ran, build the project
and run the executable it produces directly.

```text
dotnet test tests/libs/CodeBrixVideoTool.Processing.Tests/CodeBrixVideoTool.Processing.Tests.csproj

dotnet build tests/libs/CodeBrixVideoTool.Processing.Tests/CodeBrixVideoTool.Processing.Tests.csproj
./tests/libs/CodeBrixVideoTool.Processing.Tests/bin/Debug/net10.0/CodeBrixVideoTool.Processing.Tests
```

Both test projects set `CodeBrixRuntimeIdentifier` to `skia`. The comment in
`tests/libs/CodeBrixVideoTool.Playback.Tests/CodeBrixVideoTool.Playback.Tests.csproj`
explains why: the published CodeBrix.Platform package ships reference assemblies
whose method bodies throw "Ref assembly", head projects get the real
implementations swapped in automatically and a test project does not, and this
property is the lever that does it. The same comment records the limit that
shapes the whole design - even with the real assemblies present, a
SimpleViewModel cannot be constructed in a test process, because its dispatcher
needs a running application host.

The Processing tests need FFmpeg and ffprobe: the shared fixture generates an
MP4, a WebVTT file and an FFmpeg metadata file, muxes them into a "rich" MP4 with
a caption track and chapters, imports that to a Mode 2 and a Mode 1 file, and
probes all three before the first test runs. No network, no GPU, and no media
files in the repository.

## How the projects and folders are organized

```text
CodeBrixVideoTool/
  CodeBrixVideoTool.slnx                  The one solution; everything is in it
  THIRD-PARTY-NOTICES.txt                 What is used at run time and what is never redistributed
  src/
    CodeBrixVideoTool.UI/                 Shared XAML and code-behind, file-linked into every head
      CodeBrixVideoTool.UI.shproj         The shared project
      CodeBrixVideoTool.UI.projitems      The shared item list (Page and Compile items)
      App.xaml, App.xaml.cs               Palette, theme brushes, fonts, codecs, services, logging
      Views/MainPage.xaml(.cs)            The one page, the player element, and the scripted run
    CodeBrixVideoTool.Core/               The library that carries the application's packages
      Helpers/HostHelper.cs               The IHostBuilderProvider SimpleServiceResolver builds from
      Services/IMediaFileBridge.cs        The "pick a file to open" bridge
      ViewModels/MainViewModel.cs         The file list, the selection, and the two child view models
      Converters/TimecodeConverter.cs     TimeSpan to m:ss or h:mm:ss for the transport
    CodeBrixVideoTool.LinuxX11/           Head: Linux X11
    CodeBrixVideoTool.LinuxWayland/       Head: Linux Wayland
    CodeBrixVideoTool.MacOS/              Head: macOS
    CodeBrixVideoTool.Win32Skia/          Head: Windows Win32
    libs/
      CodeBrixVideoTool.Playback/         The player half
        Services/PlaybackCodecs.cs        RegisterOnce() for the AV1 and Opus decoders
        Services/IVideoPlayerSurface.cs   What the view model needs from the player element
        Services/PlaybackSelection.cs     The player half's rules, as plain static methods
        Models/                           One row of the chapter drop-down, one of the caption drop-down
        ViewModels/PlaybackViewModel.cs   The observable wrapper over PlaybackSelection
      CodeBrixVideoTool.Processing/       The conversion half
        Formats/                          The five container shapes and every policy about them
        Probing/                          IMediaProbe and the two probing routes
        Resolution/                       The resolution ladder
        Planning/                         ConversionPlan and ConversionPlanner
        Operations/                       IConversionRunner, the runner, progress, outcome, output bridge
        Containers/                       Mode 2 demultiplexing, sidecar extraction, WebVTT writing
        Samples/SampleClipFactory.cs      Synthetic clips for the tests and the scripted run
        ViewModels/ConversionViewModel.cs The operation panel
        VideoToolProcessingException.cs   The one application exception
  tests/
    libs/
      CodeBrixVideoTool.Playback.Tests/   Mirrors src/libs/CodeBrixVideoTool.Playback
      CodeBrixVideoTool.Processing.Tests/ Mirrors src/libs/CodeBrixVideoTool.Processing
```

Dependency direction runs one way and never turns back. Each head project
references `CodeBrixVideoTool.Core` and imports
`CodeBrixVideoTool.UI.projitems` as a shared item list, so `App.xaml`,
`App.xaml.cs`, `MainPage.xaml` and `MainPage.xaml.cs` are **file-linked** into
every head and compiled once per head; everything else is a **project
reference**. Core references both libraries. `CodeBrixVideoTool.Playback`
references `CodeBrixVideoTool.Processing`, because the player half asks the
Processing library which formats exist and which of them are playable;
`CodeBrixVideoTool.Processing` references nothing else in the application. Each
test project references only its own library.

Every package except the head runtime packages is declared in Core or in one of
the two libraries and reaches the heads transitively. Each head csproj carries
exactly one `PackageReference` and says so in a comment. Core sets its
`RootNamespace` to the application's namespace on purpose; the two libraries
under `src/libs` deliberately keep their own, because they reference
CodeBrix.Platform themselves and sharing a root namespace would make the
generated per-head resources class collide across assemblies.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The framework itself: XAML, the Simple MVVM toolkit (SimpleViewModel, SimpleCommand, SimpleServiceResolver, `[AffectsCommands]`, `[AffectsAllCommands]`, `IXamlRootGetter`), the font configuration and the logging adapter | `src/CodeBrixVideoTool.Core`, and again in both libraries under `src/libs` |
| CodeBrix.Platform runtime for each head | One head runtime package per head project - X11, Wayland, macOS, Win32 - and nothing else in that csproj | the four head `.csproj` files under `src/` |
| CodeBrix.Platform.VideoPlayer add-in | The `VideoPlayer` element the page hosts, with its source, position, duration, volume, mute, chapter and caption-track properties, `SeekToChapter()`, and its `MediaOpened`, `PlaybackEnded`, `MediaFailed` and `ChapterChanged` events | referenced once in `src/CodeBrixVideoTool.Core`; used in `src/CodeBrixVideoTool.UI/Views/MainPage.xaml` and `MainPage.xaml.cs` |
| CodeBrix.Platform.Fonts.Roboto | The application font, plus the Noto fallback faces | `src/CodeBrixVideoTool.Core`, `src/CodeBrixVideoTool.UI/App.xaml`, `App.xaml.cs` |
| CodeBrix.Platform UI toolkit converters | `BoolToObjectConverter`, declared in the page's resources to dim the row of a file the player cannot open | `src/CodeBrixVideoTool.UI/Views/MainPage.xaml` |
| CodeBrix.VideoPlayback | The playback core: container opening and readers, the CBVF format constants, the IVF and Ogg writers, caption files, chapter metadata, codec identifiers and the streamable-profile report | both `src/libs` projects; `Probing/MediaProbe.cs`, `Formats/MediaFormats.cs`, `Containers/`, `Playback/Services/`, `Playback/Models/` |
| CodeBrix.VideoPlayback.Authoring | Writes all four supported formats from one authoring request naming the flavor, container, frame size, speed preset, rate factor, audio codec and caption inputs | `src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs` |
| CodeBrix.VideoPlayback.Dav1d | The AV1 decoder, turned on by the application at start-up - the application's dependency, not the add-in's | `src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs` |
| CodeBrix.Audio.Opus | The Opus decoder, turned on beside it | `src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs` |
| CodeBrix.VideoProcessing | ffmpeg and ffprobe: the analysis call, the argument builder, `NotifyOnProgress()`, `NotifyOnError()` and `CancellableThrough()` | `Probing/MediaProbe.cs`, `Containers/Mode2Extractor.cs`, `Containers/SidecarExtractor.cs`, `Operations/ConversionRunner.cs`, `Samples/SampleClipFactory.cs` |
| SilverAssertions | The assertion style in both test projects | both projects under `tests/libs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | The default generic host builder, behind the `IHostBuilderProvider` that SimpleServiceResolver builds its container from | `src/CodeBrixVideoTool.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.DependencyInjection | The two `AddSingleton` registrations at start-up | `src/CodeBrixVideoTool.UI/App.xaml.cs` |
| Microsoft.Extensions.Logging.Console | The Debug-build console logger wired into the platform's logging adapter | `src/CodeBrixVideoTool.UI/App.xaml.cs` |
| xUnit v3, with the Visual Studio runner and the .NET test SDK | The test framework and its Microsoft.Testing.Platform runner | both projects under `tests/libs` |

## Worth studying in this application

### The player half: an interface over the element, and a view model that owns the decisions

`PlaybackViewModel` owns what is open, whether it can be played at all, what the
transport may do, and which chapter and caption track are showing. It reaches
the `VideoPlayer` element only through `IVideoPlayerSurface`, an interface the
Playback library declares and the page implements over the real control. Read
`src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs` first for
the contract, then `ViewModels/PlaybackViewModel.cs` for the state and the three
transport commands, then `src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` for
the page's side: a private `VideoPlayerSurface` class, an `AttachSurface()` call
from `DataContextChanged`, and one-line forwards of the element's `MediaOpened`,
`PlaybackEnded` and `MediaFailed` events into it.

Sharp edges worth knowing before you copy it: the element's source has to be
unloaded before anything read at open time is changed and the real path assigned
last; `IsPlaying` is a dependency property with no event, so the surface
subscribes with `RegisterPropertyChangedCallback()` and raises its own
`PlayStateChanged`; and the wiring runs from `DataContextChanged`, not from the
constructor. See
[Host the VideoPlayer add-in in a page and drive it from the view model](../BLUEPRINTS-MediaAndVision.md#host-the-videoplayer-add-in-in-a-page-and-drive-it-from-the-view-model).

Position, duration, volume and mute are deliberately **not** on the interface.
Those are dependency properties on the element, and the scrubber, the timecodes
and the volume slider bind straight to them by `ElementName`. The interface
remarks say why: what the view model owns is everything that is a decision
rather than a value, and routing a value that ticks many times a second through
a view model buys nothing. The transport bar's own visibility still comes from
the view model, so the rule about when a transport exists stays testable. See
[Bind a scrubber and volume slider straight to the media element](../BLUEPRINTS-ViewsAndControls.md#bind-a-scrubber-and-volume-slider-straight-to-the-media-element).

The chapter and caption drop-downs are two-way bound to view-model properties
whose setters act on the surface. That would loop the moment playback moved into
a new chapter and the view model set the selection to match, so
`PlaybackViewModel` keeps one `suppressSelectionChanges` field, set inside a
`try`/`finally` in every place the view model sets those properties itself -
following a chapter change, refreshing the lists after a file opens, and
clearing them on close. `Close()` needs it as much as the rest: clearing an
`ObservableCollection` and then nulling the selection would otherwise command
the control on the way down. See
[Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).

### Decoders the application owns, not the add-in

`PlaybackCodecs.RegisterOnce()` in
`src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs` turns on the
AV1 and Opus decoders behind a lock and exposes `IsRegistered` so a test can
assert it. `App`'s constructor calls it before anything else. The class
documentation is explicit about why the decoders are packaged and registered
this way rather than pulled in by the add-in: their licenses differ from the
add-in's, so each ships as its own package, and an application that wants them
references them and calls `Register()` once. The add-in resolves codecs through
the playback session's registries, so it plays them with no change and no
reference of its own. The same remarks record that there is deliberately no
module initializer doing this, because that would work in a debug build and
silently not run in a trimmed publish - and which decoder is needed where: AV1
is not optional, since all four supported formats carry it, while Opus is needed
for WebM, Matroska and Mode 1, Mode 2 carrying Vorbis, which the playback core
decodes itself. See
[Turn on extra media codecs once at startup](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-extra-media-codecs-once-at-startup).

### One page, one data context, two child view models

`MainViewModel` is the data context and owns the one thing the two halves share:
the selected file. It exposes `Playback` and `Conversion` as get-only
properties, creates them in its constructor, and pushes the selection into both
from `SelectedItem`'s setter. The children live in different assemblies from the
parent and from each other, which is exactly what makes them testable in
isolation, and they talk back upward through an event rather than a reference:
`ConversionViewModel` raises `ConversionFinished`, and the parent adds the
output to the library and re-selects it. The XAML binds through the parent with
dotted paths (`Playback.PlayCommand`, `Conversion.Destinations`), and the status
bar shows both `StatusText` properties in two columns. See
[Compose a page from a parent view model and child view models](../BLUEPRINTS-MVVM.md#compose-a-page-from-a-parent-view-model-and-child-view-models).

### Two probing routes behind one interface

`IMediaProbe` is registered as a singleton at start-up and resolved in
`MainViewModel`'s constructor; `AddAsync()` calls it inside a try/catch with
`IsBusy` set around the call, so `[AffectsAllCommands]` disables the UI while it
runs and any failure becomes one sentence in the status bar. `MediaProbe` picks
between two routes and its remarks say why: ffprobe cannot read the bespoke CBVF
container at all and would only see a Mode 1 file as an ordinary WebM, so a
`.cbv` file is read by the playback core's container readers and everything else
goes to ffprobe.

Two intake rules in `MediaProbe` are worth reading for how far downstream design
reaches back: a file with no video track is refused, and so is one that does not
state a duration, "so its progress could not be reported and it is refused" -
that is the progress design refusing an input it could not report honestly on.
`SourceMediaInfo` doubles as the list-item model, with the format badge, the
one-line summary, the size text and `IsPlayable` as bindable derived properties.
See
[Probe a media file behind an interface the view model resolves](../BLUEPRINTS-MediaAndVision.md#probe-a-media-file-behind-an-interface-the-view-model-resolves)
and
[Detect a container from its first bytes](../BLUEPRINTS-MediaAndVision.md#detect-a-container-from-its-first-bytes).

### Settle the conversion in a plan, then run it

Nothing about a conversion is decided while it is running.
`ConversionPlanner.Create()` validates the request and returns an immutable
`ConversionPlan` carrying every derived answer - the audio and video codecs, the
channel count, whether the source needs demultiplexing first, whether this is a
resize - together with a human-readable list of the steps. The view model
catches one exception type from it and puts the message on screen. Read
`Planning/ConversionPlanner.cs` and `Planning/ConversionPlan.cs` first, then
`Operations/ConversionRunner.cs`, and notice that everything the runner branches
on is a property of the plan, so the runner reads as a straight line and the
branching is testable without running FFmpeg at all.

Two policies in there repay attention. The channel ceiling is stated per
**destination**, not per codec, so the one uncapped destination stays uncapped no
matter what else is written with the same codec later; and nothing is ever
upmixed, so a mono source stays mono and a stereo one is never called a downmix.
`DescribeSteps()` produces the same sentences the run notes and the route line
show, so the explanation and the behavior come from one place. See
[Settle an operation in a plan before running any of it](../BLUEPRINTS-MVVM.md#settle-an-operation-in-a-plan-before-running-any-of-it).

### The long operation: progress, cancellation, and a bar that never lies

`ConversionViewModel` is the canonical long-running-operation shape.
`IsRunning` and `IsCancelling` are `[AffectsCommands]` properties, so pressing
Run disables Run and enables Cancel with no manual `RaiseCanExecuteChanged()`;
the `CancellationTokenSource` is a field, disposed and nulled in a `finally`; and
the `IProgress<ConversionProgress>` is created on the UI thread, so its callback
is already marshalled and no `InvokeOnMainThread` is needed anywhere in this
application. Cancellation never throws out of `RunAsync()` either - the runner
catches `OperationCanceledException` itself and returns a cancelled outcome, so
the view model has one exit path, and it deletes the part-written output and its
whole temporary folder on the way out.

Every conversion has exactly two stages, always, so the bar never rescales. The
first prepares the source and cannot report a percentage; the second encodes and
can, because FFmpeg says how far through the media it has reached. A stage with
no percentage of its own counts as half-done, so the bar moves forward when one
finishes rather than sitting still. One sharp edge to carry away: a
`SimpleCommand` whose implementation is asynchronous needs an explicit cast,
`(Func<object, Task>)(_ => RunAsync())`, and both async commands here do it. See
[Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag)
and
[Report progress across stages when only some of them know a percentage](../BLUEPRINTS-MVVM.md#report-progress-across-stages-when-only-some-of-them-know-a-percentage).

### Stage one: preparing a source before anything is encoded

FFmpeg cannot open the bespoke CBVF container, so a Mode 2 source is
demultiplexed first. `Containers/Mode2Extractor.cs` opens the file with the
playback core's reader, checks that the video track really is AV1, writes the
video into an IVF file and the audio into an Ogg file using the playback core's
own writers - the same two containers the authoring library writes when it
builds a bespoke file, used here in the opposite direction - and then remuxes
them into a Matroska intermediate with a copy codec. Nothing is decoded and
nothing is re-encoded, so from the intermediate onwards a Mode 2 conversion is
an ordinary conversion; the view model learns of it only through one note in the
run notes and one sentence in the route line.

Two pitfalls are recorded in the code. A packet's data is borrowed from the
reader and is gone on the next read, so a packet held for lookahead has to be
copied. The lookahead exists at all because an Ogg granule position states where
a packet *ends*, and the next packet's timestamp is the most reliable statement
of that. See
[Demultiplex a bespoke container and remux it so an external tool can read it](../BLUEPRINTS-MediaAndVision.md#demultiplex-a-bespoke-container-and-remux-it-so-an-external-tool-can-read-it).

The authoring library takes captions and chapters only as separate input files,
which is the whole reason `Containers/SidecarExtractor.cs` exists. It has two
routes - the container reader for the four supported formats, ffmpeg for the
`.mp4` family - and produces one value the runner hands on, with anything that
could not be carried across turned into a sentence in the notes that reaches the
operation panel. A Matroska or WebM file interleaves its subtitle cues with the
video, so its caption tracks are complete only after the file has been read
through, and the extractor drains the reader when it has to; the bespoke
container keeps every cue in its header and is complete the instant it is open.
An image-based caption track has no text form, so it is reported in a note rather
than silently lost, and the playback core reads WebVTT but publishes no writer,
so this application brings a small one. See
[Lift chapters and captions out of a source into sidecar files](../BLUEPRINTS-MediaAndVision.md#lift-chapters-and-captions-out-of-a-source-into-sidecar-files).

### What the drop-downs are allowed to offer

`ConversionViewModel.RefreshForSource()` is called from the `Source` setter and
does one job: clear both `ObservableCollection`s, refill them from static rules
in `MediaFormats` and `ResolutionLadder`, select the first row of each, and
notify the derived text properties. The rules themselves are static methods on
plain classes, which is what lets the tests prove them without a view model at
all. The action button's label is derived rather than stored - it asks
`MediaFormats.OperationFor()` and falls back to "Convert" for a pair the
application does not offer.

The resolution ladder is worth reading on its own. A rung names the **short**
side, so landscape and portrait sources are both measured sensibly, and every
dimension is made even, because all four supported formats carry AV1 in 4:2:0,
whose chroma planes are half-size in each direction and which has no
representation for an odd dimension. Rungs are offered strictly below the
source's short side, so a source already at a standard size is not offered its
own size again. See
[Offer only the choices that make sense for the current selection](../BLUEPRINTS-MVVM.md#offer-only-the-choices-that-make-sense-for-the-current-selection)
and
[Build a resolution ladder keyed on the short side with even dimensions](../BLUEPRINTS-MediaAndVision.md#build-a-resolution-ladder-keyed-on-the-short-side-with-even-dimensions).

### One knob moves; everything else is pinned

The four quality stops move the encoder's constant rate factor and nothing else.
The AV1 speed preset is pinned faster than the authoring library's own default,
because it matters a great deal for an application a person is sitting in front
of, and the `.mp4` export pins its preset too - so an encode takes about as long
whichever stop is chosen, and sound is settled by the destination alone. The two
rate-factor tables in `Operations/ConversionRunner.cs` were chosen to match each
other stop for stop rather than to look tidy on either encoder's own scale, and
the comment above them records how they were arrived at, including the detail
that the comparison inputs had to be re-timestamped by frame index first because
a one-frame slip swamps everything a rate factor does. See
[Move one encoder knob and pin everything else](../BLUEPRINTS-MediaAndVision.md#move-one-encoder-knob-and-pin-everything-else).

The authoring side of the same file is where the two `.cbv` modes are defined in
code: Mode 1 is the WebM-profile flavor writing a `.cbv` that is a WebM
constrained to the streamable profile with Opus audio, and Mode 2 is the bespoke
flavor writing CBVF with Vorbis audio. `MediaFormats.AudioCodecFor()` calls
that Vorbis choice the hard invariant: a bespoke CBVF file this application
writes carries Vorbis, never Opus. Only the two `.cbv` modes are meant to satisfy
the streamable profile; a standard MKV is checked and reported on, and its
failure is expected rather than an error. See
[Author a cbv file in either container mode from a settled plan](../BLUEPRINTS-MediaAndVision.md#author-a-cbv-file-in-either-container-mode-from-a-settled-plan)
and
[Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library](../BLUEPRINTS-MediaAndVision.md#export-an-mp4-with-ffmpeg-through-the-codebrix-videoprocessing-library).

### Two bridges, and what the view model does when a head cannot supply one

Both platform capabilities this application needs reach the view models through
delegate-shaped bridge interfaces the view models implement, and the page fills
in. `MainViewModel` implements `IMediaFileBridge` for "which file shall I open",
and `ConversionViewModel` implements `IOutputPathBridge` for "where shall I put
the result". The two degrade differently, and deliberately: with no file dialog
the open command says so in the status line and stops, because there is nothing
else it could sensibly do; with no save dialog the conversion writes beside the
source, using the name the planner suggests, and carries on. The planner refuses
a plan whose output path resolves to the source path, so even the fallback path
is checked.

The page's own halves are small and defensive: each picker is wrapped in a
`try`/`catch (NotSupportedException)` returning null, because a head with no
windowing system registers no picker extensions. The page also hands the data
context a `XamlRoot` **getter** - not the root itself, since the root is read
each time a dialog is shown - in the same `DataContextChanged` handler that
wires the bridges, with `InitializeComponent()` left last in the constructor so
the handler is subscribed before the XAML sets the data context. See
[Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model),
[Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model)
and
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).

The bridges being delegate properties is also what makes the application
scriptable: replacing the output-path delegate with one that returns a fixed
path removes the only dialog in the way of an unattended run.

### A dark page that appears and vanishes in pieces

`App.xaml` declares the palette and then re-keys the theme's own selection
brushes, because the theme's light accent washes out the light text in the file
rows; the overrides have to come after the merged control resources in the same
dictionary. The palette comment gives the design reason for the dark theme: a
video tool is looked at for a long time beside a moving picture, so the panels
sit back and the picture is the only bright thing on screen. The font resource
points at the `.ttf` file directly, because the font dictionary merge does not
work on Skia targets.

Almost every part of the window that comes and goes does so through a
`Visibility` property on a view model - the transport, the placeholder, the
unplayable notice, the chapter and caption drop-downs, the progress row, the
run-notes panel. `GetVisibility(bool)` comes from SimpleViewModel; where the
underlying state is a collection, the derived property has to be notified by
hand, because a collection change is not a property change. The run-notes panel
is the one to read: the lines are an `ObservableCollection<string>` filled by a
private setter, the bound item **is** the line so its template binds with no
path, and the notes are emptied the moment the next run starts, so what is on
screen always belongs to the run named in the status bar. The rule that builds
those lines is a public static method, which is the only reason it can be
tested. The file list uses the platform toolkit's `BoolToObjectConverter` with
two real `Double` values to dim a row the player cannot open, on the row's
outermost element so the badge, the name and the summary dim together. See
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette),
[Show a panel only when the last operation left something to say](../BLUEPRINTS-ViewsAndControls.md#show-a-panel-only-when-the-last-operation-left-something-to-say),
[Dim a list row for an item the application cannot act on](../BLUEPRINTS-ViewsAndControls.md#dim-a-list-row-for-an-item-the-application-cannot-act-on)
and
[Format a value for display with an IValueConverter](../BLUEPRINTS-ViewsAndControls.md#format-a-value-for-display-with-an-ivalueconverter).

### One exception, one sentence

`VideoToolProcessingException` is the only exception this application declares,
and it is thrown wherever the application can say something better than the
underlying library can. Each service catches the library exceptions it knows
about and rethrows as this one with a sentence naming the file; the view models
catch it and put the message on screen unchanged. The runner goes one step
further and lets nothing out at all, turning cancellation, a known failure and an
unknown one into three outcome values so its caller has a single exit path.
`OperationCanceledException` is always caught before the general handlers,
everywhere, so a cancel is never reported as a failure - and every message says
what to do about the problem rather than only what went wrong. See
[Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch).

### What is unit-tested, and what the scripted run covers instead

The organizing constraint of this application is stated in three places - the
remarks on `PlaybackSelection`, the remarks on `PlaybackViewModel`, and the test
csproj comment: a view model derived from SimpleViewModel cannot be constructed
without a running application host, and rules that cannot be tested are rules
that quietly stop being true. So every decision lives in a static class of plain
methods over plain values, and the view models are thin observable wrappers that
call them, keep the collections and raise the notifications.

What is left - a real head, a real player element, a real visual tree - is
covered by the scripted run instead. It drives the **view model's own commands
and properties**: it substitutes the output-path bridge, executes `RunCommand`,
awaits the `ConversionFinished` event through a `TaskCompletionSource`, and then
polls with a bounded retry loop before asserting, because the library add and
the player open both happen off that event. Only the parts that must read the
visual tree touch the page directly - to prove the row dimming is real rather
than merely configured, it lays out the list, gets the container for an item,
walks the tree to the named row and compares the opacity of a dimmed row against
a playable one. It also asserts that a standard MKV *fails* the streamable
profile, because that is the expected result. See
[Keep view model rules in a plain class so they can be tested](../BLUEPRINTS-Testing.md#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested),
[Drive a scripted end-to-end run of the whole application](../BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application),
[Share one expensive fixture across every test class that needs it](../BLUEPRINTS-Testing.md#share-one-expensive-fixture-across-every-test-class-that-needs-it)
and
[Generate real media clips from a synthetic source](../BLUEPRINTS-Testing.md#generate-real-media-clips-from-a-synthetic-source).

## Third-party content

See [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder. This
application bundles no third-party fonts, models, media or ported source; every
third-party dependency arrives as a package that carries its own license and
notices. The two things the notices file records are that FFmpeg is **used at
run time and not bundled** - the application invokes the ffmpeg and ffprobe
executables already installed on the host, through CodeBrix.VideoProcessing -
and that the media a person opens, plays and converts belongs to its owners and
is never redistributed as part of this repository. Every clip the tests and the
scripted run touch is generated at run time from FFmpeg's own synthetic sources;
`SampleClipFactory` states the discipline outright, that nothing is copied from
anywhere and nothing is left behind.

## License

CodeBrixVideoTool is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
