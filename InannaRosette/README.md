# InannaRosette

InannaRosette is "Rosette of Inanna", an oracle deck of the Divine Feminine of Sumer and
Akkad laid out on a nine-station spread. The window is one page. On the left an altar
carries an eight-petalled rosette drawn in gold hairlines: **The Heart** at the centre and
eight petals clockwise from the top - Heaven, The Morning Star, The Storehouse, The
Descent, The Great Below, The Return, The Evening Star and The Gift. On the right a rail
holds the deck stack and a fanned tray of whatever has been drawn. Clicking the stack (or
**Draw**) turns the top card face up into the tray; a card is dragged from the tray onto a
petal, where it snaps to the nearest station; a double-click turns it upside down so it
reads its reversed meaning; a single click opens a panel of that card's domains, keywords,
meaning and lore. **Auto-lay** fills every empty station at once, **Shuffle** and **Clear**
gather everything back into the deck, and a querent name and a question can be typed along
the top. **Interpret** replaces the rail with a written reading of whatever is laid -
complete or partial - and **Create PDF** composes a print-quality report of it, with the
spread drawn as vector art, one section per station and an appendix of card lore. A reading
can also be saved as JSON and opened again later, cards and orientations intact.

The forty cards are three suits: twenty-four Great Goddesses, the Eight Gates of Inanna's
Descent, and eight Sacred Emblems. None of them is a bitmap. Every card face, every emblem,
the rosette itself and every ornament in the PDF is drawn from the same SVG path data,
rendered on screen through XAML `Path` geometry and in the report through a path parser
this application writes itself. It is this repository's reference for a **hand-placed,
multi-page PDF drawn directly with `XGraphics`**, for **registering embedded OFL fonts with
the PDF font system**, and for a page that owns a rich pointer-driven scene - drag, drop,
snap, flip - while a view model that has never heard of a `Canvas` still decides everything
that a card may do.

## What this sample shows a CodeBrix.Platform developer

- How a library that has no UI reference at all holds the whole subject - the deck, the
  spread, the interpreter, the serializer and the report builder - behind three service
  interfaces a view model resolves:
  [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- How that library registers all three of its services with one `AddReading()` call the
  `App` constructor makes:
  [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- How eight Merriweather faces travel inside the library assembly as embedded resources and
  are registered with the PDF font system once per process, so the report looks the same on
  a machine with no fonts installed:
  [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).
- How a designed, fixed-layout document is drawn straight onto `XGraphics` rather than
  composed through a document object model, and why the project file says so:
  [Compose a fixed layout poster with the CodeBrix PdfDocuments library](../BLUEPRINTS-DocumentsAndData.md#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library).
- The family's property and command idiom end to end - `SetProperty(ref field, value)`,
  lazily created `SimpleCommand`s, and three counts carrying `[AffectsCommands]` and
  `[AffectsProperties]` so every button enables and disables itself with no code in the
  page:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- How a page tells the view model what the pointer did and is told back, through delegates,
  what the person should now see - a bridge that carries a whole scene rather than a single
  platform capability:
  [Assign every bridge through the interface that declares it](../BLUEPRINTS-PlatformServices.md#assign-every-bridge-through-the-interface-that-declares-it).
- How a "save as…" dialog is reached from a command, with a suggested file name, a cancel
  that is not an error, and a head with no dialog that says so instead of throwing:
  [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
  [Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model).
- How the path a picker hands back is made the same on every head before anything touches
  the disk, percent-escapes decoded and the empty placeholder file removed:
  [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- How the finished report is handed to the operating system from the view model, with both
  of `LaunchUriAsync`'s failure modes turned into a status line:
  [Open a URL in the default browser from a view model](../BLUEPRINTS-PlatformServices.md#open-a-url-in-the-default-browser-from-a-view-model).
- How the page hands the view model a `XamlRoot` *getter* so a dialog has somewhere to
  attach:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- How work that must not block the window - composing the prose, composing a PDF with
  embedded font subsets and forty card faces - runs on a worker and comes back onto the UI
  thread in one hop:
  [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- How every failure - a picker that is missing, a file that will not write, an
  interpretation that throws - becomes a dialog or a line of status text rather than an
  exception the user meets:
  [Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing).
- How three panes are swapped with `Visibility`-typed computed properties and no converter
  anywhere in the application:
  [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).
- How a view model that hands out nine delegates to the page releases every one of them,
  along with its ten commands:
  [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- How the page keeps exactly one subscription to the view model for the chrome that cannot
  be bound, wired from whichever of `DataContextChanged` and `Loaded` arrives first:
  [Subscribe to a view model once and unsubscribe when the page unloads](../BLUEPRINTS-ViewsAndControls.md#subscribe-to-a-view-model-once-and-unsubscribe-when-the-page-unloads).
- A page whose view model is declared in `<Page.DataContext>`, bound entirely with the
  platform `Binding` markup extension, with the designer guard that makes that safe:
  [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension),
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How a whole application is re-skinned - text boxes, dialogs and picker chrome included -
  by overriding the theme's own brush keys in `Application.Resources` rather than
  retemplating every control:
  [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- What a head's `Program.Main` contains, what the `App` constructor does and in what order,
  and how the first window is created and navigated:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
  [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- How a window that needs room for a rosette asks for a launch size and refuses to be
  dragged smaller than its layout allows:
  [Set the window's launch size](../BLUEPRINTS-AppStructureAndStartup.md#set-the-windows-launch-size),
  [Keep the window from shrinking below a minimum](../BLUEPRINTS-AppStructureAndStartup.md#keep-the-window-from-shrinking-below-a-minimum).
- The bundled font becoming the application-wide default, and console logging that is
  compiled in only for Debug builds:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks),
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Where every package reference belongs, how one folder of XAML is compiled into four
  executables, and why the Core library's root namespace is not its assembly name:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
  [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace),
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- How bundled fonts that travel inside an assembly are recorded where a reader will look for
  them:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- An xUnit v3 suite on the Microsoft.Testing.Platform runner that covers the deck content,
  the spread, the interpreter, the serializer, the path parser and the PDF, including a
  document asserted on without a golden file:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
  [Prove a registration extension registers what it promises](../BLUEPRINTS-Testing.md#prove-a-registration-extension-registers-what-it-promises),
  [Assert on a generated document without a golden file](../BLUEPRINTS-Testing.md#assert-on-a-generated-document-without-a-golden-file).

## Building, running and testing

There is one solution, `InannaRosette.slnx`, and it holds every project in the folder. Its
own header comment says it contains "everything that builds with the plain .NET SDK on
Linux, macOS and Windows", and that is accurate: it opens on any of the three. It files the
reading library under a `Libraries` solution folder and the test project under a `Tests`
solution folder.

| Solution | Open it on | Contains |
| --- | --- | --- |
| `InannaRosette.slnx` | Linux, macOS, Windows | The shared UI project, `InannaRosette.Core`, the four heads, `InannaRosette.Reading` under `Libraries` and `InannaRosette.Reading.Tests` under `Tests` |

### The heads

| Project | Platform |
| --- | --- |
| `src/InannaRosette.LinuxX11` | Linux desktop, X11 |
| `src/InannaRosette.LinuxWayland` | Linux desktop, Wayland |
| `src/InannaRosette.MacOS` | macOS |
| `src/InannaRosette.Win32Skia` | Windows, native Win32 window |

Four of the family's six heads, not six. There is deliberately no `WinWpfSkia` head and no
`LinuxFrameBuffer` head: the application is a pointer-driven scene on a window that opens
at 1440 x 900 and refuses to be dragged below 1180 x 760, which is not what a framebuffer
device is for, and nothing here needs the WPF-hosted surface. Every head targets plain
`net10.0`, so no project file in the folder sets `EnableWindowsTargeting` or names
`net10.0-windows`, and the whole solution restores and builds on any of the three operating
systems.

Each head is a `Program.cs` and a csproj, identical apart from the one `Use…()` call that
names the backend. Two csproj properties differ from the leanest heads in this repository:
every head sets `Nullable` and `ImplicitUsings` to `enable`, matching
`InannaRosette.Core`, because the shared UI files are written with nullable annotations and
are compiled *into* each head.

### Prerequisites

- The .NET 10 SDK. All CodeBrix code arrives from NuGet; no CodeBrix library is referenced
  as a source project, so this folder builds on its own.
- Nothing else. No accounts, tokens, workloads, native runtimes, downloads or data files.
  The deck, the spread, the emblem art and the report's fonts all travel inside the
  assemblies.
- A windowing system for the Linux heads: X11 for the `LinuxX11` head, a Wayland compositor
  for the `LinuxWayland` head.
- A native file dialog if you want to save a report or reopen a reading. Every head in this
  folder has one; a head that did not would still run, and the view model would say so
  rather than fail.
- A PDF viewer registered with the desktop, if the **Open** button on the "Report saved"
  panel is to do anything. Without one the application says no application was available
  and leaves the file where it wrote it.

### Running one head

```text
dotnet run --project src/InannaRosette.LinuxX11
dotnet run --project src/InannaRosette.LinuxWayland
dotnet run --project src/InannaRosette.MacOS
dotnet run --project src/InannaRosette.Win32Skia
```

Building the Windows head on Linux or macOS is supported; running it is not. Console
logging is compiled in only for Debug builds - the body of `App.InitializeLogging()` sits
inside `#if DEBUG` - so a Release run is silent.

### Tests

The tests cover `InannaRosette.Reading` only; there is no test project for the Core view
models, for the shared UI or for any head. This application *does* have a `global.json`,
and it does one thing: it selects the Microsoft.Testing.Platform runner for the whole
folder. The test csproj sets the same thing again for itself, with `OutputType` of `Exe`,
`UseMicrosoftTestingPlatformRunner` and `TestingPlatformDotnetTestSupport`. Because
Microsoft.Testing.Platform is in play, a plain `dotnet test` can report that it discovered
no tests on some .NET 10 SDK builds. Building the test project and running the produced
executable directly always works:

```text
dotnet build tests/libs/InannaRosette.Reading.Tests/InannaRosette.Reading.Tests.csproj -c Release
tests/libs/InannaRosette.Reading.Tests/bin/Release/net10.0/InannaRosette.Reading.Tests
```

The tests need no GPU, no network, no display and no temp directory: every fixture is built
in memory from fixed card ids, fixed station indexes and a fixed timestamp, and the one test
that produces a PDF asserts on the bytes without writing a file. Nothing is committed as a
binary fixture.

## How the projects and folders are organized

```text
InannaRosette/
  InannaRosette.slnx                  The one solution; opens on Linux, macOS and Windows
  global.json                         Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt             Third-party content bundled with, or used by, the application
  src/
    InannaRosette.UI/                 Shared items project (.shproj + .projitems); produces no assembly
      App.xaml                        The whole design system: palette, text styles, button and text-box
                                        styles, and the theme brush keys the dialogs and text boxes follow
      App.xaml.cs                     Bootstrap: default font, resolver + AddReading(), design mode,
                                        launch size, dark theme, window, minimum size, frame, Debug logging
      Views/MainPage.xaml             The page: header, the scene canvas with its five layers, status strip
      Views/MainPage.xaml.cs          Geometry, pointer mechanics, animation, the three bridges, the dialogs
      Controls/CardView.xaml(.cs)     One card, drawn procedurally: face, back, flip, fan, hover, reverse
      Controls/Ornament.cs            Palette tokens, tracked capitals and the procedural geometry helpers
    InannaRosette.Core/               View models; carries every non-head package reference
      InannaRosette.Core.csproj       RootNamespace InannaRosette; platform, font, hosting, logging packages
      Helpers/HostHelper.cs           The IHostBuilderProvider handed to SimpleServiceResolver
      Helpers/FileDialogHelper.cs     Percent-decoding and placeholder removal for picker paths
      Services/IReadingTableBridge.cs What the page must show when the table changes
      Services/IReadingFileBridge.cs  The save and open dialogs only a head can show
      Services/IReadingDialogBridge.cs  The three dialog shapes the application asks for by name
      ViewModels/MainViewModel.cs     The whole reading: deck, tray, nine stations, ten commands, status
      ViewModels/ReadingCard.cs       One card that has left the deck: which station, which way up
    InannaRosette.LinuxX11/           Head: Program.cs plus exactly one runtime package
    InannaRosette.LinuxWayland/       Head
    InannaRosette.MacOS/              Head
    InannaRosette.Win32Skia/          Head
    libs/
      InannaRosette.Reading/          The domain library; no CodeBrix.Platform dependency at all
        Models/Card.cs                The card record, the three suits, the forty emblem motifs
        Models/Spread.cs              SpreadPosition, PlacedCard and RosetteReading
        Models/Interpretation.cs      The finished prose, as records
        Data/DeckData.cs              The forty cards: names, lore, keywords, meanings, invocations
        Data/RosetteSpread.cs         The nine stations, their questions, their angles and their axes
        Data/EmblemArt.cs             Every emblem as SVG path layers in a 0..100 design box
        Services/Deck.cs              The shuffled stack: seedable, drawable, resettable
        Services/IReadingInterpreter.cs, ReadingInterpreter.cs     The prose, deterministic from the cards
        Services/InterpretationTemplates.cs   The sentence stock and the deterministic chooser
        Services/IReadingSerializer.cs, ReadingSerializer.cs       Reading to JSON and back
        Services/IPdfReportBuilder.cs, PdfReportBuilder.cs         The printable report
        Services/Pdf/PdfFonts.cs      Registers the embedded Merriweather faces, once per process
        Services/Pdf/PdfPageFlow.cs   A vertical cursor with margins and page breaks (internal)
        Services/Pdf/PdfText.cs       Tracked capitals, wrapping, justification, fit-to-width
        Services/Pdf/PdfPalette.cs    The same colour tokens as App.xaml, as XColor (internal)
        Services/Pdf/PdfCardPainter.cs, PdfOrnaments.cs            The card design and the ornaments, in PDF
        Services/Pdf/SvgPathToPdf.cs  SVG path mini-language to XGraphicsPath
        Fonts/Merriweather-*.ttf      Eight embedded faces
        Fonts/OFL-Merriweather.txt    Their licence, embedded beside them
        RegisterServices.cs           AddReading(): the three services in one line
        InternalsVisibleTo.cs         Opens internals to the .Tests assembly
  tests/
    libs/
      InannaRosette.Reading.Tests/    xUnit v3 on Microsoft.Testing.Platform
        TestData.cs                   Fixed readings built from fixed card ids and a fixed timestamp
```

Dependencies run one way. `InannaRosette.Reading` is the bottom of the stack: it references
CodeBrix.PdfDocuments and the dependency-injection abstractions its one-line registration
extension is written against, and knows nothing about XAML, CodeBrix.Platform or view
models. `InannaRosette.Core` project-references it and adds the CodeBrix.Platform, font,
generic-host and logging packages; that is the only place a non-head package is declared.
Each of the four heads project-references `InannaRosette.Core`, adds exactly one
CodeBrix.Platform runtime package for its own backend, and **file-links** the shared UI by
importing `..\InannaRosette.UI\InannaRosette.UI.projitems` with `Label="Shared"`. The
`.shproj` produces no assembly of its own: `App.xaml`, `MainPage.xaml`, `CardView.xaml`,
their code-behind and `Ornament.cs` are compiled into each head, which is why each head's
`Program` can see `App` without a using directive - they share the namespace
`InannaRosette` - and why every head csproj repeats the `<Page Include="**\*.xaml" />` plus
`<None Remove="**\*.xaml" />` pair. The test project project-references the reading library
only, so the domain tests never load a UI package.

One naming detail is worth catching early, and this folder has two of them. The Core
library sets `RootNamespace` to `InannaRosette`, not `InannaRosette.Core`, so the view
models live in `InannaRosette.ViewModels` inside an assembly called `InannaRosette.Core`,
and `MainPage.xaml` has to name both:
`xmlns:vm="clr-namespace:InannaRosette.ViewModels;assembly=InannaRosette.Core"`. The
reading library, by contrast, sets `RootNamespace` to `InannaRosette.Reading`, matching its
assembly name, because nothing in it is ever addressed from XAML. Note also that the page's
own control namespace is declared as `xmlns:controls="using:InannaRosette.Controls"` - the
`using:` form, because `CardView` lives in the head assembly alongside the page, while the
two types that live in other assemblies take the `clr-namespace:…;assembly=…` form.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The whole UI: `Application`, `Window`, `Frame`, `Page`, `Canvas`, `UserControl`, `ContentDialog`, the `Storyboard` animations on the cards, the `Windows.Storage.Pickers` file dialogs and `Windows.System.Launcher`; plus the "Simple" toolkit - `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `[AffectsCommands]`, `[AffectsProperties]`, `[AffectsAllCommands]`, `InvokeOnMainThread`, the ambient logger factory and `CodeBrixPlatformHostBuilder` | `src/InannaRosette.Core/`, `src/InannaRosette.UI/` |
| CodeBrix.Platform runtime for each head | One rendering backend per head - X11, Wayland, macOS and Win32 - each selected by its own `Use…()` call on the host builder, with `UseDirectSkiaCanvasMode()` beside it | the four `src/InannaRosette.<Head>/` projects and their `Program.cs` |
| CodeBrix.Platform.Fonts.Merriweather | Supplies Merriweather as the application-wide default text font and as two `FontFamily` resources (`MerriweatherFont`, `MerriweatherBoldFont`) addressed through `ms-appx:///` URIs, which every text style and every hand-built `TextBlock` in the scene uses | `src/InannaRosette.Core/InannaRosette.Core.csproj`, `src/InannaRosette.UI/App.xaml`, `src/InannaRosette.UI/App.xaml.cs`, `src/InannaRosette.UI/Controls/CardView.xaml.cs` |
| CodeBrix.PdfDocuments | The entire report: `PdfDocument`, `PdfPage` and `XGraphics` for the pages and the drawing, `XGraphicsPath` for the emblem art, `XFont`, `XSolidBrush`, `XPen` and the gradient brushes for type and ornament, and `EmbeddedFontResolver`, `EmbeddedResourceFontFace` and `MetaFontResolver` for the embedded faces | everything under `src/libs/InannaRosette.Reading/Services/Pdf/`, and `src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs` |
| SilverAssertions | The `Should()` assertion style used throughout the tests | all files under `tests/libs/InannaRosette.Reading.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` builds the container from | `src/InannaRosette.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.DependencyInjection.Abstractions | `IServiceCollection` and `TryAddSingleton`, so the reading library can offer its own `AddReading()` registration extension without depending on a container | `src/libs/InannaRosette.Reading/RegisterServices.cs` |
| Microsoft.Extensions.Logging.Console | The Debug-only console logger factory handed to the platform's ambient logging, which is where the view model's startup line and its caught failures go | `src/InannaRosette.UI/App.xaml.cs`, `src/InannaRosette.Core/ViewModels/MainViewModel.cs` |
| xUnit v3 (with its Visual Studio runner) and Microsoft.NET.Test.Sdk | The test framework and the Microsoft.Testing.Platform host | `tests/libs/InannaRosette.Reading.Tests/InannaRosette.Reading.Tests.csproj` |
| Microsoft.Extensions.DependencyInjection | Builds the real container that `RegisterServicesTests` asserts `AddReading()` filled correctly | `tests/libs/InannaRosette.Reading.Tests/RegisterServicesTests.cs` |

## Worth studying in this application

### The subject as a library: forty cards, nine stations, no UI

`src/libs/InannaRosette.Reading` is where the application actually lives, and it has no
CodeBrix.Platform reference and no XAML anywhere in it. `Data/DeckData.cs` is the deck:
forty `Card` records, each carrying a name, an epithet, a transliteration, an emblem, two
colours, its domains, its lore, upright and reversed keywords and meanings, and an
invocation. `Data/RosetteSpread.cs` is the spread: nine `SpreadPosition` records with a
title, a subtitle, the question that station asks, its angle on the flower and a paragraph
describing how to read it. `Services/Deck.cs` is a shuffled stack with an optional seed, and
`Models/Spread.cs` holds the three types the rest of the application passes around -
`SpreadPosition`, `PlacedCard` and `RosetteReading`.

Two details in the data repay attention. `SpreadPosition.OppositeIndex` is arithmetic on the
index (`((Index - 1 + 4) % 8) + 1`) rather than a table, so the four axes of the rosette -
Heaven against The Great Below, Morning Star against Return, Storehouse against Evening
Star, Descent against The Gift - are a property of the shape and not a list somebody could
get out of step with the titles. And `RosetteSpread.Axes` exposes those four pairs once, so
the interpreter, the report and the tests all read them from the same place.

Sharp edges met here. `DeckData.Cards` is built on first access with `??=` rather than in a
field initializer, with a comment saying why: the six suit arrays above it are static field
initializers and run in textual order, so a list assembled at the top would see nulls.
`Card.Id` is a 1-based dense index and `ById()` indexes straight into the list on that
assumption, which is the contract the serializer depends on - change the card order and
every saved reading points at a different card. And the spread's clockwise-from-the-top
ordering is asserted in `RosetteSpreadTests` by name, because it is the one thing in the
folder that both the page geometry and the interpretation text silently assume.
[Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface)

### The view model owns the table; the page owns the pictures

This is the most copyable idea in the folder. `MainViewModel` holds a `Deck`, a
`List<ReadingCard>` of every card that has left it, and a nine-element
`ReadingCard?[] _stations`. Those three fields *are* the table. Three public methods change
it - `PlaceOnStation(card, station)`, `ReturnToTray(card, announce)` and
`ToggleReversed(card)` - and each one is a complete transaction: it validates that the card
is one of its own, vacates whatever station the card was on, displaces any occupant back to
the tray with its own status line, writes the new state, invalidates the interpretation, and
recounts.

The page is told what happened through `IReadingTableBridge`, four `Action` delegates:
`CardAdded`, `CardMoved` (with a boolean asking for the "settling onto the altar"
animation), `CardFlipped` and `TableCleared`. The page never asks the view model where a
card is; it is told, and it looks at `ReadingCard.Station` and `ReadingCard.IsReversed` to
decide what to draw. `ReadingCard` itself is the seam: a tiny class in the Core library
whose two mutable properties have `internal set`, so the page holds the very same instances
and can read them, but only the view model can change them.

Read `MainViewModel`'s "What is on the table" region first, then `IReadingTableBridge.cs`,
then `MainPage.ShowCardAdded` / `ShowCardMoved` / `ShowCardFlipped` / `ShowTableCleared`.
The whole rendering side of a drop is four lines.

Sharp edges met here. `PlaceOnStation` handles the case where the card being laid is already
*on* a station, and clears the old slot before writing the new one - miss that and dragging
a card from one petal to another leaves a ghost occupying the first. The displaced-occupant
branch writes its own status line and then suppresses the general guidance
(`RefreshCounts(refreshGuidance: false)`), so the more specific message survives. And
`_cards.Contains(card)` guards all three methods, because the page is handing back an object
it was given and nothing else proves it is still part of this reading.
[Assign every bridge through the interface that declares it](../BLUEPRINTS-PlatformServices.md#assign-every-bridge-through-the-interface-that-declares-it)

### Drag, drop and snap: a gesture the page runs and the view model settles

`MainPage` owns the gesture and nothing else. `OnCardPressed` captures the pointer, records
the grab offset inside the card, and raises the card's Z order; `OnCardMoved` moves the
card and its drop shadow and highlights the nearest station; `OnCardReleased` computes the
nearest station one last time and calls `PlaceOnStation` or `ReturnToTray`. Snapping is one
method, `NearestStation(x, y)`: the closest of the nine station centres, accepted only if it
is within `_cardH * 0.8`, and `-1` otherwise - which is what makes "dropped on nothing" a
real answer rather than a failure. A press that never moved more than four pixels is not a
drag at all; it opens the card's lore panel instead, and two presses on the same card within
420 milliseconds are a double click that reverses it.

The geometry is all recomputed in one method. `Relayout()` runs from `SizeChanged` and from
`Loaded`, sizes the altar and the 360-wide rail, derives the card size from whichever of the
altar's height and width is the binding constraint, sets the rosette radius at `1.55` card
heights, and rebuilds the ornament, the nine slots, the rail chrome and every card position.
`LabelRadius()` is worth reading on its own: it pushes each station's label just far enough
out that its box clears the card box, taking the *smaller* of the horizontal and vertical
requirements because two rectangles are separated as soon as they clear on either axis -
which is what keeps the four diagonal labels tucked in instead of flung to the corners.

Sharp edges met here. `OnCardCaptureLost` exists because a window losing focus mid-drag
otherwise leaves a card stranded at the pointer; it ends the drag and re-lays whichever
region the card belongs to. `ReleasePointerCapture` is wrapped in a `try` because the
capture may already be gone by the time the release arrives. `OnRootPointerMoved` sets
`e.Handled = true` on every pointer move that nothing else claimed, with a comment saying
why: an unhandled move bubbles to the window manager, which then drags the macOS window
instead of leaving the scene alone. And the double-click detection is hand-rolled from
`DateTime.UtcNow`, so it does not follow the desktop's own double-click interval.

### Writing the reading: deterministic prose from a seed the cards make

`ReadingInterpreter` is pure and takes no random source. It hashes the placements - station
index, card id and orientation, FNV-style - into one integer seed, and every sentence in the
finished reading is chosen from a bank of variants in `InterpretationTemplates` by
`Pick(variants, seed, salt)`, where the salt is different for each slot. The consequence is
the property the tests assert directly: the same rosette always reads the same, two
different rosettes do not read alike, and turning a single card over changes the seed and
therefore the prose. There is no persistence of "what it said last time" anywhere, because
none is needed.

The output is a `ReadingInterpretation` record: a title, an opening, one
`PositionInterpretation` per laid station, a list of `Insight`s the interpreter noticed
(striking axes, suit dominance, a heavily reversed spread, a partial layout), counsel and a
closing invocation. A partial rosette is a first-class case rather than an error - one card
laid is a reading - and an empty one returns a documented empty shape.

Sharp edges met here. The seed is derived from the cards, not from `DateTime` or a `Random`,
which is the entire reason the suite can assert on exact sentences; seed it from anything
ambient and the tests become smoke tests. `Interpret` de-duplicates placements by station
index before doing anything (`GroupBy(...).Select(g => g.First())`), because the reading it
is handed may have come from a file. And the templates are `internal` with a
`using static` at the top of the interpreter, which keeps the sentence stock out of the
public surface while leaving it testable through `InternalsVisibleTo`.

### The printable report: a hand-placed document drawn straight onto XGraphics

`PdfReportBuilder` composes a cover, a full-page spread diagram with a legend, the opening
and insights, one section per station with the card drawn in the margin, the counsel, the
closing invocation and an appendix of lore. It does all of that with `XGraphics` directly
rather than through a flowing document object model, and the library's csproj carries the
comment explaining that choice where a future reader will find it.

Because the low-level API has no notion of a margin or a page break, the folder grows its
own typesetting kit under `Services/Pdf/`. `PdfPageFlow` is a vertical cursor: it knows the
four margins, exposes `Y`, `Remaining` and `EnsureSpace(needed)`, starts a new page when a
block will not fit, and paints the parchment ground, the running header and the centred
folio on every page except the cover. It takes its page metrics from a throwaway `PdfPage`
probe in its constructor, so a caller can plan a layout before the first page exists.
`PdfText` supplies what the drawing API does not: letter-spaced ("tracked") capitals drawn a
glyph at a time, greedy word wrapping, justified body copy whose last line stays ragged, and
a `DrawFitted` that steps a font down until a line measures inside its column.
`PdfCardPainter` draws the deck's own card design - gold double border, corner flourishes,
numeral band, emblem medallion, name - at two levels of detail, rotating a reversed card a
half turn about its own centre exactly as it lies on the table.

Sharp edges met here. Every text block is *measured* before it is committed, so a heading is
never left stranded at the foot of a page without its first paragraph. The colours in
`PdfPalette` are the same hex values as the XAML resources in `App.xaml` and the constants
in `Ornament.cs`, written down three times in three type systems - the price of a UI-free
library that must match the screen, and a real maintenance edge. `PdfPalette.Over()` exists
because the renderer ignores the alpha channel of a gradient stop, so a tint has to be
blended to an opaque colour by hand. And `Build()` returns bytes rather than writing a file,
which is what lets the tests parse the result without touching a disk.
[Compose a fixed layout poster with the CodeBrix PdfDocuments library](../BLUEPRINTS-DocumentsAndData.md#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library)

### Merriweather twice: embedded for the page, packaged for the screen

The same typeface reaches the application by two completely different routes, and it is
worth understanding why. On screen it arrives as a NuGet package: the Core csproj references
`CodeBrix.Platform.Fonts.Merriweather`, `App.xaml.cs` sets
`FeatureConfiguration.Font.DefaultTextFontFamily` to an `ms-appx:///` URI inside that
package before anything else happens, and `App.xaml` exposes the regular and bold faces as
`FontFamily` resources that every style and every hand-built `TextBlock` names.

In the PDF it arrives as eight `.ttf` files embedded in `InannaRosette.Reading` itself, with
`OFL-Merriweather.txt` embedded beside them. `PdfFonts.EnsureRegistered()` builds two
`EmbeddedFontResolver`s - one for the text faces under the family alias `InannaSerif`, one
for the quieter caption faces under `InannaSerifLight` - and registers them with
`MetaFontResolver`, under a lock, exactly once per process. The report's own constructor
calls it, and `Build()` calls it again, because either one may be the first thing that runs.
Whatever is resolved is embedded in the finished PDF as a glyph subset, so the report reads
identically on a machine with no fonts installed at all.

Sharp edges met here. `MetaFontResolver` routes *family*-name lookups through any resolver
whose default font name matches, but *face*-name lookups need a registration per face name,
which is why the code loops over four face names for each of the two families. The light
family deliberately maps its "Bold" face to Merriweather **Medium**, because the resolver
decides what is bold by looking for "bold" in the face name and a quieter weight is what the
captions want. And registering before the first `XFont` is constructed is what makes the
outcome deterministic - do it later and the first font resolved wins.
[Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system)

### One set of vector art, two renderers

`Data/EmblemArt.cs` holds every emblem - the forty motifs the cards name, plus the Venus
star, the rosette and the corner flourish - as a list of `EmblemLayer` records: SVG path data in a 0..100
design box, a fill-or-stroke flag, a stroke width, an opacity and an "accent" flag saying
the layer should take the card's secondary colour. Nothing else in the application describes
a picture.

Both renderers read that same list. On screen, `CardView.LayerCanvas()` turns each layer into
a XAML `Path` inside a `Canvas` that a `Viewbox` scales to whatever size the card currently
is. In the PDF, `PdfOrnaments.DrawLayers()` turns each layer into an `XGraphicsPath` through
`SvgPathToPdf`, a full implementation of the SVG path mini-language written for this
application: every command in both absolute and relative form, implicit repeated commands,
implicit line-to after a move-to, numbers run together where a sign or a second decimal point
makes the break unambiguous, quadratics raised to cubics exactly, and elliptical arcs
converted with the endpoint-to-centre parameterisation of the SVG specification and emitted
as at most four cubic segments of ninety degrees or less.

Sharp edges met here. Not every head's XAML path parser accepts arcs, so `ParseGeometry` in
`CardView` tries the binding helper, then `XamlReader`, then flattens every arc to béziers
with `Ornament.ArcsToBeziers` and retries - logging the fallback once, not once per card.
`PdfOrnaments.DrawLayers` catches `FormatException` per layer and skips it, because one
malformed glyph must never take a whole report down. `SvgPathToPdf` deliberately does *not*
close an unterminated sub-path, so a stroked figure is not silently joined back to its start;
a fill closes it anyway. And `SvgPathToPdf.CountSegments` exists purely so the parser can be
tested without a PDF: it runs the same parse through a counting sink that throws on any
non-finite coordinate.

### Files, dialogs and the three bridges the page fills in

`MainPage`'s `DataContextChanged` handler is the whole wiring story, and it tests the data
context once per contract - `is IReadingTableBridge`, `is IReadingFileBridge`,
`is IReadingDialogBridge`, plus `as IXamlRootGetter` - rather than casting once to
`MainViewModel`. Each capability is therefore independently absent-able, and the page names
no view-model type to install any of them.

`IReadingFileBridge` carries two delegates. `PickSavePathAsync(suggestedFileName, typeName,
extension)` is filled with a static method that configures a `FileSavePicker` and then
passes the result through `FileDialogHelper`: `ToFileSystemPath` decodes a percent-escaped
`file://` path (the Linux desktop portal hands one back, which would otherwise save
`My Reading.pdf` as `My%20Reading.pdf`, and `Ölberg` far worse), and
`RemoveEmptyPlaceholder` deletes the zero-length placeholder the WinRT picker creates so the
chosen path behaves like a pure destination. Both helpers leave a path that needs neither
alone. `PickReadingPathAsync` is the open dialog, filtered to `.json`.

`IReadingDialogBridge` carries the three dialog shapes the application asks for by name -
a confirmation, a message, and the "Report saved" panel with its **Open** button. The view
model could have used `SimpleViewModel`'s own
[ConfirmDialog and ShowError helpers](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs);
it deliberately does not, because the temple styling - gold on lapis, the application's own
button styles - is a page concern, and the page supplies the shapes and keeps the look with
them. The **Open** button on that panel is wired to
`MainViewModel.OpenSavedReportCommand` rather than starting anything itself, so handing a
file to the operating system stays a view-model decision.

Sharp edges met here. A head with no dialog leaves every delegate null, and the view model
has a documented answer for each: a missing confirmation is treated as *granted* (which is
exactly what the page's own dialog does when `ShowAsync` throws), a missing message becomes
`SetStatus($"{title}: {message}")`, and a missing picker becomes a message saying the head
cannot save files. `DoCreatePdf` builds the bytes *before* it shows the save dialog, so a
report that fails to compose never asks for a destination it will not use. And
`DoOpenSavedReport` handles both of `LaunchUriAsync`'s failure modes - it can return false
and it can throw - in one place, the only place in the application that asks the host to open
anything.
[Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns),
[Open a URL in the default browser from a view model](../BLUEPRINTS-PlatformServices.md#open-a-url-in-the-default-browser-from-a-view-model),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)

### Saving a reading: identity only, text rehydrated

`ReadingSerializer` writes a deliberately thin document: a version number, a timestamp, the
querent, the question, and one `{ position, cardId, reversed }` entry per laid card, camel
cased and indented. None of the card text, none of the station text and none of the
interpretation is stored. Reading one back looks each card up in `DeckData` and each station
up in `RosetteSpread`, so a saved file is a few hundred bytes, stays readable, and picks up
any later correction to the lore for free.

`FromJson` is tolerant by design and says so in its own doc comment: an unknown card id or
an out-of-range station index is skipped rather than throwing, a second card on a station
already filled is ignored, and only a document that is not JSON at all raises a
`FormatException` - which the view model turns into a dialog. `MainViewModel.ApplyReading`
then clears the table and adds each placement with its station already set, which is why
`CardAdded` has to look at `ReadingCard.Station` rather than assuming a new card goes to the
tray.

Sharp edges met here. `cardId` is the deck's dense 1-based index, so the deck's *order* is
now part of the file format; inserting a card in the middle of `DeckData` would silently
re-point every saved reading. The `Version` field is written but not yet checked on read,
which is a hook rather than a mechanism. And `Created` falls back to `DateTime.Now` when the
document carries the default value, so a hand-written file with no timestamp still opens.

### Startup: four heads, one App, and a window sized for a rosette

Each head's `Program.Main` calls `App.InitializeLogging()` before anything else - the logging
adapter must be in place before the platform starts writing to it - then names the `App`
subclass, selects one backend with a single `Use…()` call, adds
`UseDirectSkiaCanvasMode()`, builds and runs. `[STAThread]` sits on `Main` in all four,
including the Linux and macOS ones. The four files are identical apart from that one call.

`App`'s constructor does five things in a fixed order before `InitializeComponent()`: sets
the default text font family, creates the `SimpleServiceResolver` from `HostHelper.GetHost()`
and registers the reading library into it with `services.AddReading()`, calls
`SimpleViewModel.SetIsDesignMode(false)`, sets `ApplicationView.PreferredLaunchViewSize`,
and sets `RequestedTheme` to `Dark`. `OnLaunched` creates the window, pins its minimum size
on the `OverlappedPresenter` before `Activate()`, puts a `Frame` in it and navigates to
`MainPage`.

The two window-size calls are the interesting pair, and both carry their reasoning in
comments. The launch size is set on the constructor because `PreferredLaunchViewSize` is
read while the native window is being created, so there is no window to ask; it is set
unconditionally on every launch because the platform remembers it in its own settings file,
and writing it every time keeps that file in step with the source rather than letting an old
value linger. The minimum is set in `OnLaunched` because the presenter exists by then -
constructing a `Window` builds its native window straight away - and before `Activate()` so
the window manager has the constraint before the window is ever shown. The numbers are not
arbitrary: 1440 x 900 is the size at which the rosette gets its full card size with the
360-wide rail beside it and the header's two rows on one line each; 1180 x 760 is the point
below which the header buttons wrap and the diagonal station labels collide with the cards.

Sharp edges met here. `SetIsDesignMode(false)` is not optional. `MainPage.xaml` instantiates
`MainViewModel` from `<Page.DataContext>`, so `InitializeComponent()` is what runs the
view-model constructor - a constructor whose first line is `if (IsDesignMode(true)) { return; }`
- and setting design mode off too late leaves the window with an empty deck and a bare
altar. Registering before `InitializeComponent()` matters for the same reason: the view model
resolves its three services while that call is running, which is also why every one of those
fields is initialized to a concrete library instance first, so design mode - which returns
before the resolution - still has working objects behind every property.
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
[Set the window's launch size](../BLUEPRINTS-AppStructureAndStartup.md#set-the-windows-launch-size),
[Keep the window from shrinking below a minimum](../BLUEPRINTS-AppStructureAndStartup.md#keep-the-window-from-shrinking-below-a-minimum),
[Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer)

### A whole look from one resource dictionary

`App.xaml` is the design system and there is no converter, no theme dictionary and no second
palette anywhere in the application. It declares sixteen named colours and their brushes, a
handful of derived translucent brushes, three gradients, two `FontFamily` keys, six text
styles, three button styles - two of them with full control templates and visual states -
a text-box style and a scroll-viewer style. It then overrides the platform's *own* brush
resource keys: the whole `TextControlBackground…` / `TextControlBorderBrush…` /
`TextControlForeground…` family, and the `ContentDialog…` family, so text boxes and dialogs
follow the palette without being retemplated. Because those overrides live in
`Application.Resources` rather than in `Page.Resources`, they also reach the popup layer,
which is where a `ContentDialog` actually renders.

What cannot be styled this way is drawn. The rosette, the nine dashed station slots, the rail
chrome, the card faces, the lore panel and the interpretation panel's contents are all built
in code onto `Canvas` layers, which is why `Ornament.cs` carries the same sixteen colour
tokens as string constants and why `MainPage` has a small family of `Text()`, `Head()`,
`Rule()` and `Caption()` factory helpers.

Sharp edges met here. `Ornament.Track()` fakes letter-spacing by interleaving thin spaces
into the string, because the XAML surface has no character-spacing property - the same trick
`PdfText.DrawTracked` performs a glyph at a time on the PDF side. `BuildChips()` wraps
keyword pills by *estimating* each one's width from its character count, because there is no
`WrapPanel` here. The `ScrollViewer` inside the interpretation panel measures its child with
an unbounded width on these heads, so `InterpStack.Width` is pinned explicitly in
`Relayout()` or long paragraphs run off the panel edge. And every `Storyboard` in the
application is started inside a `try`, falling back to setting the final value directly, so
a head whose animation support is thin still ends up in the right visual state.
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette),
[Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension)

### The status line, and ten buttons that gate themselves

There are exactly three counts on the view model - `DeckRemaining`, `TrayCount` and
`FilledStations` - and one `IsBusy`. Every button's enablement and every caption in the
application is derived from those four. The counts carry `[AffectsCommands]` naming the
commands they gate and `[AffectsProperties]` naming the computed properties that read them;
`IsBusy` carries `[AffectsAllCommands]`. `RefreshCounts()` recomputes all three from the one
source of truth after anything moves, and that single call is what re-enables **Interpret**
the moment a first card lands and greys **Auto-lay** when the last station fills.

`RefreshGuidance()` is the other half: a small cascade that works out what the person should
be told to do next from the same state - draw a card, drag one onto a petal, press Interpret,
press Shuffle because the deck is empty. Commands that have just done something specific
call `SetStatus(...)` with their own sentence and pass `refreshGuidance: false` so the
general advice does not immediately overwrite it.

Sharp edges met here. Every command is created with `??=` into a private field on an
expression-bodied property, so the same `SimpleCommand` instance is handed to every binding
and `Dispose()` has something to dispose; a plain `=> new SimpleCommand(...)` would refresh a
command nothing is bound to. `DoAutoLay` sets `IsBusy` around its loop and clears it in a
`finally`, and it awaits 85 milliseconds between placements purely so the person can see the
cards land - the only deliberate delay in the folder, alongside the one-tick `Task.Delay(1)`
in `DoDraw` that gives the page's flip animation a frame to start in. And `IsBusy` is *not*
a cancellation mechanism: nothing here can be cancelled mid-flight, so a very large report
simply takes as long as it takes with every button greyed.
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way),
[Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties),
[Report a failure as status text instead of throwing](../BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing)

### A UI-free library, and the tests it makes possible

Because `InannaRosette.Reading` has no CodeBrix.Platform reference,
`tests/libs/InannaRosette.Reading.Tests` never loads a UI package and the whole subject -
the deck, the spread, the shuffle, the prose, the JSON, the path parser and the PDF - is
exercised without starting a head. `InternalsVisibleTo.cs` is its own file at the library
root, holding nothing but the attribute and naming the `.Tests` assembly exactly, which is
what makes `InterpretationTemplates`, `PdfPageFlow` and `PdfPalette` testable while they
stay out of the public surface.

The test classes read as a specification, and each file's first line says what it covers:
`DeckDataTests` for the forty cards, their identifiers and the text every card must carry;
`RosetteSpreadTests` for the Heart, the eight petals clockwise from the top and the four
axes; `DeckTests` for what a seed guarantees and what drawing, returning and resetting do;
`ReadingInterpreterTests` for determinism, coverage of every laid station, and the empty and
partial cases; `ReadingSerializerTests` for the round trip, the exact camelCase document
shape, and documents that are wrong; `SvgPathToPdfTests` for every command in both forms,
the number-scanning quirks and the malformed input it must refuse; `PdfFontsTests` for the
eight faces and the licence travelling inside the assembly and for registration being
idempotent; `PdfReportBuilderTests` for the bytes really being a PDF with the Merriweather
subsets inside them; and `RegisterServicesTests` for `AddReading()` handing back all three
services, as singletons, safely twice. `BasicTests` proves the host itself runs, as every
test project in the family does.

Sharp edges met here. `TestData.cs` builds every fixture from fixed card ids, fixed station
indexes and a fixed timestamp, which is what lets the suite assert on exact sentences and
exact file names - and it means adding a card to the middle of the deck would move the
fixtures. `PdfReportBuilderTests` asserts on a lower bound of twenty thousand bytes, the
`%PDF-` signature and the presence of the embedded font names rather than on a golden file,
which is the only honest thing to do with a document whose bytes include a timestamp.
Nothing is committed as a binary fixture and nothing is written to disk, so the suite needs
no temp directory at all.
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project),
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Prove a registration extension registers what it promises](../BLUEPRINTS-Testing.md#prove-a-registration-extension-registers-what-it-promises),
[Assert on a generated document without a golden file](../BLUEPRINTS-Testing.md#assert-on-a-generated-document-without-a-golden-file)

## What this application does not show

It is one page, one view model and one library. It has:

- No second page and no navigation beyond the first one. The interpretation "panel" is a
  `Border` that swaps with the deck rail, not a navigated page.
- No settings and no persistence of its own. The querent, the question and the reading are
  forgotten when the window closes unless they were saved to a file deliberately; there is
  no recent-files list and no remembered folder.
- No cancellation and no progress reporting. Composing the prose and composing the PDF both
  run to completion behind a busy flag, so nothing here shows a progress channel or a
  cancellation token.
- No `SKXamlCanvas` and no Skia drawing. Every picture in the window is XAML `Path`,
  `Ellipse`, `Rectangle` and `TextBlock` on a `Canvas`, which is a very different
  performance profile from a painted surface and would not scale to thousands of elements.
- No converters, no `WrapPanel`, no `ItemsControl` and no data templates. The scene is built
  element by element in code, which is the right trade for nine stations and a forty-card
  deck and the wrong one for a list.
- No four-of-six-heads variation in behavior. Unlike some applications here, no head does
  anything different from its siblings; there is no framebuffer picker to opt into, and no
  software-render-surface call.
- No use of `SimpleViewModel`'s own `ConfirmDialog` / `ShowInfo` / `ShowError` helpers. The
  page wires an `IXamlRootGetter` so they *would* work, and that wiring is worth reading, but
  the application routes dialogs through its own bridge so it can style them.
- No disposal of the view model. `MainViewModel.Dispose()` is written, and it is worth
  reading as the reference for releasing ten commands and nine page-captured delegates, but
  nothing in this folder calls it: the page unwires its `PropertyChanged` subscription from
  `Unloaded` and stops there. An application with more than one page would need
  [Dispose a view model the XAML declared from the page Unloaded](../BLUEPRINTS-MVVM.md#dispose-a-view-model-the-xaml-declared-from-the-page-unloaded).
- No tests for the view model, the page or any head. The library is covered thoroughly and
  everything above it is not.

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the third-party
content bundled with, or used at run time by, this application: the eight Merriweather font
files embedded in `src/libs/InannaRosette.Reading/Fonts/` under the SIL Open Font License
1.1, with the license text embedded beside them, and the same typeface arriving through the
CodeBrix.Platform.Fonts.Merriweather package for the on-screen text. Everything else in the
folder is original: the card texts, the lore, the meanings and invocations, the spread, the
vector emblem art and the interpretation templates were written for this application.
Third-party code arrives as NuGet packages that carry their own licenses and notices, and
nothing is downloaded at run time. The readings and reports the application writes belong to
whoever made them.

## License

InannaRosette is licensed under the Apache License, Version 2.0, see [../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
