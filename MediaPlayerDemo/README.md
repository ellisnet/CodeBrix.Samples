# MediaPlayerDemo

MediaPlayerDemo is a one-page media player. The window holds a text box carrying
a media address, a Load button, a stretch-mode picker, a media player element
filling the rest of the window, and a status line along the bottom. On startup the
view model loads a default address (a public sample video served over HTTPS) and,
because the element is declared with `AutoPlay="True"`, playback begins straight
away. Typing a different address and pressing Load swaps the source; the picker
changes how the picture is scaled inside the element (Uniform, UniformToFill,
Fill, None). Playback itself is driven by the media element's own built-in
transport controls, which the page turns on with
`AreTransportControlsEnabled="True"` - the application contributes no transport UI
of its own.

It is this repository's reference for hosting the CodeBrix.Platform.MediaPlayer
add-in's `MediaPlayerElement` in a page and feeding it a source from a
`SimpleViewModel`. It is also the smallest application here - one view model, one
helper, one page, six heads, no libraries and no tests - which makes it the
cleanest available skeleton of the six-head CodeBrix.Platform project layout.

## What this sample shows a CodeBrix.Platform developer

- How to put a `MediaPlayerElement` on a page and let the view model choose what
  plays by exposing an `IMediaPlaybackSource`:
  [Play a video from a URL with the MediaPlayer add-in](../BLUEPRINTS-MediaAndVision.md#play-a-video-from-a-url-with-the-mediaplayer-add-in).
- Which project each package reference belongs on: the framework, the add-in and
  the font on the Core library, exactly one runtime package on each head:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- How a native dependency that only some heads need is declared on just those
  heads, with a comment saying why:
  [Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads).
- What a head's `Program.Main` contains and how the backend is selected:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- The four things the `App` constructor does, in the order they have to happen:
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- How `OnLaunched` creates the window, puts a `Frame` in it and navigates to the
  first page:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- How to give `SimpleServiceResolver` a generic-host builder from the shared Core
  library instead of duplicating it in six heads:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- The family's property and command idiom - `SetProperty()`, semi-auto properties,
  a lazily built `SimpleCommand`, and `[AffectsCommands]` to keep a button's
  enablement current:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Why a view model constructed by the XAML designer needs a guard, and what it is
  paired with at startup:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How to bind a `ComboBox` directly to enum values with no converter and no label
  list, using `SetEnumProperty()`:
  [Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).
- How a view model turns a bad user-entered value into a bound status string
  rather than an exception or a dialog:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- How a `SimpleViewModel` releases its lazily created commands:
  [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- The one-handler code-behind that hands the view model a `XamlRoot` getter
  through `IXamlRootGetter`, so dialogs can be opened from the view model:
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
  a page-level `StaticResource`:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- How to get console diagnostics while developing and a silent Release build:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

## Building, running and testing

There is one solution, `MediaPlayerDemo.slnx`, and its own comment says what it
is: everything that builds with the plain .NET SDK on Linux, macOS and Windows.
Open it on any of the three operating systems. There is no second, Windows-only
solution, because this application has no native (WinUI 3, WPF or .NET MAUI) head
- only Skia heads.

The heads:

| Project | Platform |
| --- | --- |
| `src/MediaPlayerDemo.LinuxX11` | Linux, X11 |
| `src/MediaPlayerDemo.LinuxWayland` | Linux, Wayland |
| `src/MediaPlayerDemo.LinuxFrameBuffer` | Linux, framebuffer (no display server) |
| `src/MediaPlayerDemo.MacOS` | macOS |
| `src/MediaPlayerDemo.Win32Skia` | Windows, Win32 window |
| `src/MediaPlayerDemo.WinWpfSkia` | Windows, Skia hosted in a WPF window |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting` so the solution still
  restores on Linux and macOS. No workload is needed.
- All CodeBrix packages come from NuGet. No CodeBrix library is referenced as a
  source project, so this folder builds on its own.
- Network access on the first run with the shipped default address, which is a
  public HTTPS URL. Any address the media element accepts can be typed into the
  box instead; the view model only requires that `new Uri(...)` succeeds. There
  are no accounts, tokens, asset downloads or data files you have to supply.
- The two Windows heads each carry one extra package beyond their runtime
  package: the LibVLC native runtime for Windows, which the MediaPlayer add-in
  needs there. Both csproj files carry the comment explaining it; see each head's
  csproj for the exact package.
- The Linux and macOS heads declare no native media package at all. This
  application does not state what the add-in needs natively on those platforms,
  so check the add-in's own documentation before shipping on them.

To run one head from the command line, from this folder:

```text
dotnet run --project src/MediaPlayerDemo.LinuxX11
dotnet run --project src/MediaPlayerDemo.LinuxWayland
dotnet run --project src/MediaPlayerDemo.LinuxFrameBuffer
dotnet run --project src/MediaPlayerDemo.MacOS
dotnet run --project src/MediaPlayerDemo.Win32Skia
dotnet run --project src/MediaPlayerDemo.WinWpfSkia
```

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` is inside `#if DEBUG` - so a Release run is silent.

There are no tests. This application has no `tests/` folder, no test project and
no `global.json`, so there is no test-runner selection to be aware of here and
nothing in this folder demonstrates the family's test conventions. Look to an
application that ships tests for those.

## How the projects and folders are organized

```text
MediaPlayerDemo/
  MediaPlayerDemo.slnx                  The one solution; every project; opens on Linux, macOS and Windows
  THIRD-PARTY-NOTICES.txt               Third-party content used by this application
  src/
    MediaPlayerDemo.UI/                 Shared items project: the XAML that every head compiles
      MediaPlayerDemo.UI.shproj         Shared-project shell, so an IDE can load the folder as a project
      MediaPlayerDemo.UI.projitems      The shared file list each head imports with Label="Shared"
      App.xaml                          Merged WinUI resources and the Open Sans FontFamily resource
      App.xaml.cs                       Bootstrap: default font, service resolver, design mode, window and frame, logging
      Views/MainPage.xaml               The whole UI: address box, Load button, stretch picker, media element, status line
      Views/MainPage.xaml.cs            Thin code-behind: hands the view model a XamlRoot getter
    MediaPlayerDemo.Core/               Class library; carries every non-head package
      MediaPlayerDemo.Core.csproj       RootNamespace MediaPlayerDemo; framework, add-in, font, hosting and logging packages
      Helpers/HostHelper.cs             The IHostBuilderProvider that SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs       The only view model: address, source, status, stretch options, LoadCommand
    MediaPlayerDemo.LinuxX11/           Head: Program.cs plus a csproj with one runtime package
    MediaPlayerDemo.LinuxWayland/       Head: Program.cs plus a csproj with one runtime package
    MediaPlayerDemo.LinuxFrameBuffer/   Head: Program.cs plus a csproj with one runtime package
    MediaPlayerDemo.MacOS/              Head: Program.cs plus a csproj with one runtime package
    MediaPlayerDemo.Win32Skia/          Head: Program.cs plus a csproj with a runtime package and the Windows native media runtime
    MediaPlayerDemo.WinWpfSkia/         Same as Win32Skia, plus net10.0-windows and a software render surface
```

The dependency direction is one way. Each head project takes a project reference
on `MediaPlayerDemo.Core` and file-links the shared UI by importing
`..\MediaPlayerDemo.UI\MediaPlayerDemo.UI.projitems` with `Label="Shared"`.
`MediaPlayerDemo.Core` references nothing else in the application; it only carries
package references, so everything the heads share arrives transitively through it.
The shared UI project is never compiled on its own: `App.xaml`, `App.xaml.cs`,
`Views/MainPage.xaml` and `Views/MainPage.xaml.cs` are compiled once into each of
the six head assemblies, which is why each head csproj also has to tell MSBuild to
treat `.xaml` files as `Page` items. Because the XAML ends up inside the head
assembly while the view models live in the library, the page reaches them with an
assembly-qualified `clr-namespace`
(`xmlns:vm="clr-namespace:MediaPlayerDemo.ViewModels;assembly=MediaPlayerDemo.Core"`),
while the code-behind's own namespace resolves inside whichever head is being
built. Nothing flows the other way: Core knows nothing about the UI or the heads.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML framework itself: `Application`, `Window`, `Frame`, `Page` and the controls on the page, plus `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IXamlRootGetter`, `IHostBuilderProvider`, `CodeBrixPlatformHostBuilder` and `FeatureConfiguration.Font` | `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`; used throughout `src/MediaPlayerDemo.UI/` and `src/MediaPlayerDemo.Core/` |
| CodeBrix.Platform.MediaPlayer add-in | Supplies the `MediaPlayerElement` control and the `Windows.Media.Core` and `Windows.Media.Playback` types (`MediaSource`, `IMediaPlaybackSource`) the view model builds a source with | `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`, `src/MediaPlayerDemo.UI/Views/MainPage.xaml`, `src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform.Fonts.OpenSans | Ships the Open Sans font that is set as the application-wide default and as the page's `FontFamily`, addressed through an `ms-appx:///` URI | `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`, `src/MediaPlayerDemo.UI/App.xaml`, `src/MediaPlayerDemo.UI/App.xaml.cs`, `src/MediaPlayerDemo.UI/Views/MainPage.xaml` |
| CodeBrix.Platform runtime for the head | Exactly one runtime package per head - the X11, Wayland, framebuffer, macOS, Win32 and WPF Skia runtimes - and nothing else | the six head csproj files under `src/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the dependency-injection container | `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`, `src/MediaPlayerDemo.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The `LoggerFactory` with a console provider that is wired into the platform's ambient logger in Debug builds | `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`, `src/MediaPlayerDemo.UI/App.xaml.cs` |
| LibVLC native runtime for Windows | The native media backend the MediaPlayer add-in needs on the two Windows heads; declared only there | `src/MediaPlayerDemo.Win32Skia/MediaPlayerDemo.Win32Skia.csproj`, `src/MediaPlayerDemo.WinWpfSkia/MediaPlayerDemo.WinWpfSkia.csproj` |

## Worth studying in this application

### Hosting the media element and feeding it a source

The whole point of the application is the contract between the view model and the
media element, and it is one property wide. `MainViewModel` exposes
`PlayerSource`, typed as the interface `IMediaPlaybackSource` with a public getter
and a private setter, and builds it inside `LoadMedia()` with
`MediaSource.CreateFromUri()`. The page declares a `MediaPlayerElement` whose
`Source` is bound one way to that property. No bridge interface is involved:
the add-in's element is an ordinary XAML control, so nothing platform-specific has
to be handed down from the page for playback to work.

Read `src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs` first and then
`src/MediaPlayerDemo.UI/Views/MainPage.xaml`; the packaging that makes both
compile is in `src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`.

Sharp edges met here. `MediaSource.CreateFromUri()` returns a `MediaSource` while
the element's `Source` takes the interface, so the bound property is the interface
type. `MediaPlayerElement`, `MediaSource` and the `Windows.Media.*` namespaces all
arrive with the MediaPlayer add-in rather than the base framework, so without that
package reference none of them resolve. And assigning a new source replaces the
old one without disposing it - if your own view model owns a disposable source,
release it in `Dispose()`. See
[Play a video from a URL with the MediaPlayer add-in](../BLUEPRINTS-MediaAndVision.md#play-a-video-from-a-url-with-the-mediaplayer-add-in)
and
[Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### The address box, the Load command, and reporting failure as text

`MediaAddress` is a two-way bound string whose setter normalizes `null` to
`string.Empty`, and it carries `[AffectsCommands(nameof(LoadCommand))]` so that
every edit re-evaluates the command's `CanExecute`. `LoadCommand` is a lazily
created `SimpleCommand` built from a `CanLoad()` predicate and a `DoLoad()` action;
the page binds the text box with `UpdateSourceTrigger=PropertyChanged` so the
property changes on each keystroke and the button enables as soon as there is text.
`LoadMedia()` wraps the work in try/catch and reports the outcome by setting
`StatusText`, a private-set bound string that a `TextBlock` displays. On failure
the previous source stays loaded and keeps playing, which is a decision worth
making consciously in your own application.

Both files are short: `src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs` for
the property, the command and the try/catch, then
`src/MediaPlayerDemo.UI/Views/MainPage.xaml` for the three bindings. Note that the
status only covers URI construction and source creation; it says nothing about
whether the media actually plays. See
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way)
and
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).

### The stretch picker, bound straight to enum values

The picker is the smallest possible correct shape for a choice list. The view
model exposes `StretchOptions`, a read-only `IReadOnlyList<Stretch>` written out by
hand rather than taken from `Enum.GetValues()`, and `SelectedStretch`, a two-way
property whose setter calls `SetEnumProperty()` rather than `SetProperty()`. The
`ComboBox` binds `ItemsSource` and `SelectedItem` and needs no item template, no
label list and no converter, because the enum member names are exactly the text
that should appear. The same property is bound a second time, one way, to the
element's `Stretch`. Writing the list by hand is what keeps unwanted members out
of the picker and fixes the display order; when member names do not read well, a
`SimpleEnum`-backed picker or an item template is the better shape, and this
application does not show either. See
[Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).

### The six-head skeleton, start to finish

If you are here for the project layout rather than the media, read it in this
order: a head's `Program.cs` (they are identical apart from the `Use...()` call
that names the backend, and the WinWpfSkia head's extra render-surface block), then
that head's csproj for the `Page` glob, the `Label="Shared"` import, the project
reference and its single runtime package, then
`src/MediaPlayerDemo.UI/MediaPlayerDemo.UI.projitems` for what the shared project
actually contributes, then `src/MediaPlayerDemo.UI/App.xaml.cs` for the startup
sequence, and finally `src/MediaPlayerDemo.Core/Helpers/HostHelper.cs`.

`App`'s constructor does four things and nothing else - sets the default font,
creates the `SimpleServiceResolver` from `HostHelper.GetHost()`, calls
`SimpleViewModel.SetIsDesignMode(false)`, and calls `InitializeComponent()` - and
`OnLaunched` creates the window, puts a `Frame` in it and navigates to the page.
All application behavior lives past that boundary, in the view model.

The sharp edges are mostly in the build files. New XAML pages have to be added to
the `.projitems` by hand, as a `Page` with `Generator MSBuild:Compile` and as a
`Compile` with `DependentUpon` its `.xaml`; the shared project has no globbing.
The `.shproj` `ProjectGuid` and the `.projitems` `SharedGUID` must match. The
`<None Remove="**\*.xaml" />` beside each head's `Page` glob is required, or the
same files are both content and pages. And `SetIsDesignMode(false)` is not
optional: without it every view model still believes it is in the designer at run
time, its design-mode guard returns from the constructor early, and the
application starts and does nothing. See
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer),
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
[Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Fan native packages out across the heads](../BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads),
[Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
and
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### What this application does not show

It is deliberately small, so come to it for the two things it is a reference for
and go elsewhere for the rest. It has:

- No transport control from the view model. There is no play, pause, stop, seek,
  position, duration, volume, mute, rate or looping member anywhere. The view
  model's only influence over playback is which source it hands to the element,
  and `AutoPlay="True"` on the element - setting the source is what starts
  playback. If you do not want playback on launch, turn `AutoPlay` off rather
  than withholding the source.
- No playback state feedback. Nothing subscribes to a media event, so a
  well-formed but unplayable address produces a `Loaded:` status and a silent
  element. If your application has to react to playing, paused or ended, this
  sample gives you no pattern for it; reach the underlying player yourself,
  behind an interface the view model consumes.
- No file picker, no bridge interface, no local-file or `file://` handling, no
  drag and drop, no playlist and no recent-files list. Only URI text typed into
  the box reaches the player.
- No registered services. The registration lambda in `App.xaml.cs` is a comment,
  so the wiring is shown but no resolution is.
- No async, threading, cancellation or progress. `LoadMedia()` is synchronous and
  `InvokeOnMainThread` is never used.
- No `SimpleDialog`, `SimpleEnum`, `SimpleOsInfo` or `SimpleMessaging` in use. The
  page does wire an `IXamlRootGetter` so that a view model could open dialogs, and
  it is worth reading as the graceful-degradation pattern - the `as` cast plus
  `?.` costs nothing when the view model does not implement the interface, and the
  getter is a lambda because `XamlRoot` is null until the page is in the visual
  tree - but this view model never opens one. See
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- No converters, custom controls, `SKXamlCanvas`, styles, templates, embedded
  resources, SVG, Lottie or icons; no second page and no navigation beyond the
  initial one; and no settings or persistence, so the address is not remembered
  between runs.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder records the third-party content used at
run time: the code dependencies arrive as NuGet packages that carry their own
licenses and notices, including the LibVLC native library package the Windows
heads reference for the CodeBrix.Platform.MediaPlayer add-in. Nothing else is
bundled in this folder - the Open Sans font arrives inside its package - and the
media the application plays is whatever the user points it at, which is never
redistributed as part of this repository.

## License

MediaPlayerDemo is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
