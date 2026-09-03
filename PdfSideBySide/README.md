# PdfSideBySide

PdfSideBySide opens two PDF documents at once and shows them next to each other, one per pane,
so a reader can compare them page by page. Each pane has its own browse button ("Document 1…"
and "Document 2…") that doubles as the pane's label, shows the full path of the file it holds,
and carries a "Page n of N" line underneath. A column of controls down the middle drives both
panes together: previous page and next page step *both* documents, and a zoom ladder that runs
from 100% (the whole page fits the pane) up to 1000% applies to both. Because two editions of a
document rarely paginate identically, a separate "Adjust right" pair of buttons steps only
Document 2, which is how the reader lines the two up; once lined up, that offset survives every
later "both" step. A note between the two page labels reads "Comparing 3:4" so the current
pairing is always visible, and a small cross of arrow buttons for each pane nudges that pane's
viewport across its own page, because the interesting part of one document is rarely in the same
place on the other. Launching any head with two file paths on the command line pre-loads both
panes.

It is a reference for rasterizing PDF pages with the CodeBrix.PdfRasterizer library and binding
the result into a XAML `Image`: opening a document and reading its page count, rendering a chosen
page to PNG off the UI thread with a latest-request-wins cancellation policy and a bounded
most-recently-used cache, deriving the render resolution from the zoom level, and keeping all of
that in a plain UI-free library plus a `SimpleViewModel`, while the page contributes only the
layout arithmetic that it alone can do.

## What this sample shows a CodeBrix.Platform developer

- Opening a user-chosen PDF and asking the rasterizer for its page count, with a clear error when
  the file is missing or is not a PDF: [Open a PDF and read its page count with the CodeBrix PdfRasterizer library](../BLUEPRINTS-DocumentsAndData.md#open-a-pdf-and-read-its-page-count-with-the-codebrix-pdfrasterizer-library).
- Turning one PDF page into PNG bytes without blocking the UI thread, even though the underlying
  rasterizer is synchronous: [Rasterize a PDF page to PNG off the UI thread](../BLUEPRINTS-DocumentsAndData.md#rasterize-a-pdf-page-to-png-off-the-ui-thread).
- Deriving the render resolution from the zoom level so zooming sharpens text instead of scaling a
  blurry bitmap, with a cap so the top of the ladder does not render a poster: [Choose the render resolution from the zoom level](../BLUEPRINTS-GraphicsAndRendering.md#choose-the-render-resolution-from-the-zoom-level).
- Keeping recently rendered pages in a bounded most-recently-used cache keyed by file, page and
  resolution: [Cache rendered results with a bounded most recently used cache](../BLUEPRINTS-MVVM.md#cache-rendered-results-with-a-bounded-most-recently-used-cache).
- Running one render per pane and letting a newer page request cancel the older one without
  painting a stale image: [Run one render per pane with latest request wins cancellation](../BLUEPRINTS-MVVM.md#run-one-render-per-pane-with-latest-request-wins-cancellation).
- Stepping two documents together while letting the reader offset one of them, with each cursor
  clamping at its own last page: [Keep two documents in step while letting the user offset one](../BLUEPRINTS-DocumentsAndData.md#keep-two-documents-in-step-while-letting-the-user-offset-one).
- Splitting a two-region screen into a parent view model that owns the model and two child view
  models that own only their bound state: [Compose a page from a parent view model and child view models](../BLUEPRINTS-MVVM.md#compose-a-page-from-a-parent-view-model-and-child-view-models).
- Signaling a change in an object graph (zoom, two pan positions, two page cursors) with a single
  incrementing property the page watches: [Signal a non property model change to the view with a version counter](../BLUEPRINTS-MVVM.md#signal-a-non-property-model-change-to-the-view-with-a-version-counter).
- Refreshing `CanExecute` for buttons whose enablement lives in a model object rather than in a
  bound property: [Refresh CanExecute when the gating state is not a bound property](../BLUEPRINTS-MVVM.md#refresh-canexecute-when-the-gating-state-is-not-a-bound-property).
- Combining a view-model zoom factor and pan fraction with the viewport size that only the page
  knows: [Let the page do the layout arithmetic only it can do](../BLUEPRINTS-ViewsAndControls.md#let-the-page-do-the-layout-arithmetic-only-it-can-do).
- Decoding PNG bytes returned by a service into a `BitmapImage` that XAML binds to: [Turn image bytes into a bound BitmapImage](../BLUEPRINTS-ViewsAndControls.md#turn-image-bytes-into-a-bound-bitmapimage).
- Driving a placeholder and a busy bar from `Visibility`-typed derived properties instead of
  registering a converter: [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).
- Throwing a typed exception for a domain rule ("you cannot compare a document with itself") that
  the view model tells apart from a real failure: [Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch).
- Deciding whether two differently spelled paths name the same file, on a case-sensitive or a
  case-insensitive file system: [Treat two spellings of one path as the same file](../BLUEPRINTS-DocumentsAndData.md#treat-two-spellings-of-one-path-as-the-same-file).
- Keeping the interesting logic in a library that has no XAML and no CodeBrix.Platform reference at
  all, so it can be tested without a head: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- Reading file paths off the process command line so a comparison can be repeated from a script:
  [Load documents named on the command line during startup](../BLUEPRINTS-MVVM.md#load-documents-named-on-the-command-line-during-startup).
- Starting async loading from the view-model constructor and catching everything inside it:
  [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- Opening the operating system's file picker for a single `.pdf` and handling the cancel case:
  [Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model).
- Handing the view model a `XamlRoot` getter so its `ShowError` dialogs have somewhere to appear:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Opting the Linux framebuffer head into a platform-drawn file picker, with a start folder, a
  folder restriction and an extension filter: [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- Declaring a page's XAML namespaces, instantiating a view model from XAML, and scoping a region to
  a child view model with `DataContext`: [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- Drawing every arrow and magnifier button from `FontIcon` glyphs so the icons survive on a device
  with no system fonts: [Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts).
- Guarding both view-model constructors so the XAML designer never runs their bodies:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Writing a head's `Program.Main` so it holds no application logic and differs from its siblings by
  one `Use…()` call: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Setting fonts, the service resolver and design mode in the `App` constructor, in the order that
  works: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Creating the main window and navigating a `Frame` to the first page in `OnLaunched`:
  [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- Supplying a generic-host builder to `SimpleServiceResolver` from a small helper in the Core
  library: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Wiring a console logger factory into the platform's ambient logging in Debug builds only:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Making a bundled font the application-wide default and registering fallback faces for scripts it
  has no glyphs for: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Forcing a software render surface after the host is built, on the one head that needs it:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Laying an application out as `src/libs` plus `tests/libs` around a shared UI project:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Compiling one copy of `App.xaml` and the views into every head through a shared project:
  [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Declaring every shared package once in a Core library and exactly one runtime package per head:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Knowing which libraries arrive transitively (and deciding when to name them directly anyway):
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Setting up an xUnit v3 test project on the Microsoft.Testing.Platform runner the way the CodeBrix
  family does: [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Writing the multi-page PDFs the tests need at run time instead of committing a pile of binaries:
  [Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them).
- Locating one committed fixture beside the test binary with `AppContext.BaseDirectory`:
  [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).

## Building, running and testing

There is one solution, `PdfSideBySide.slnx`, and it holds every project in the folder. Its own
header comment says it contains "everything that builds with the plain .NET SDK on Linux, macOS and
Windows", and that is accurate: it opens on any of the three. It files the domain library under a
`Libraries` solution folder and the test project under a `Tests` solution folder.

The heads:

| Project | Platform |
| --- | --- |
| `PdfSideBySide.LinuxX11` | Linux, X11 |
| `PdfSideBySide.LinuxWayland` | Linux, Wayland |
| `PdfSideBySide.LinuxFrameBuffer` | Linux, drawn straight to the framebuffer device with no OS chrome |
| `PdfSideBySide.MacOS` | macOS |
| `PdfSideBySide.Win32Skia` | Windows, Win32 window |
| `PdfSideBySide.WinWpfSkia` | Windows, Skia hosted in a WPF window (`net10.0-windows`) |

Prerequisites are unusually light. The .NET 10 SDK is all that is needed to build; there are no
workloads, no accounts, no tokens and nothing downloaded at run time. Every project targets
`net10.0` except the WinWpfSkia head, which targets `net10.0-windows` and sets
`EnableWindowsTargeting` so the whole solution still restores on Linux and macOS even though that
head cannot run there. The PDF engine is PDFium, and its native libraries arrive inside the
CodeBrix.PdfRasterizer package for every runtime identifier the application can run on, so there is
no system PDF library to install and no per-head native fan-out to arrange. The Linux desktop heads
need their windowing system running; the framebuffer head needs a framebuffer device.

The data is supplied by the reader: two PDF files, chosen with the browse buttons. Running a head
with no arguments starts with two empty panes, each showing a "No document selected" placeholder.
Passing two paths pre-loads both panes:

```text
dotnet run --project src/PdfSideBySide.LinuxX11
dotnet run --project src/PdfSideBySide.LinuxX11 -- /path/left.pdf /path/right.pdf
```

Substitute the head you want: `src/PdfSideBySide.LinuxWayland`,
`src/PdfSideBySide.LinuxFrameBuffer`, `src/PdfSideBySide.MacOS`, `src/PdfSideBySide.Win32Skia` or
`src/PdfSideBySide.WinWpfSkia`. The framebuffer head is the exception to "just browse for a file":
its picker is opt-in in `Program.cs`, and as configured there it starts in one fixed folder and is
restricted to one directory tree, so it is the first thing to change when running that head
somewhere else.

The tests cover the `PdfSideBySide.PdfRender` library only; there is no test project for the Core
view models or for any head. This application has no `global.json`, so the
Microsoft.Testing.Platform runner is selected entirely by the test project's own csproj, which sets
`OutputType` to `Exe` and turns on `UseMicrosoftTestingPlatformRunner` and
`TestingPlatformDotnetTestSupport`. Because Microsoft.Testing.Platform is in play, a plain
`dotnet test` can report that it discovered no tests on some .NET 10 SDK builds. Building the test
project and running the produced executable directly always works:

```text
dotnet build tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj -c Release
tests/libs/PdfSideBySide.PdfRender.Tests/bin/Release/net10.0/PdfSideBySide.PdfRender.Tests
```

The tests need no GPU, no network and no display. They do need a writable temp directory: each test
that wants a synthetic document writes one into a fresh GUID-named folder under the system temp
path. They also need the committed fixture `assets/Inanna.pdf`, which the csproj copies next to the
test binary with `CopyToOutputDirectory="PreserveNewest"`.

## How the projects and folders are organized

```text
PdfSideBySide/
  PdfSideBySide.slnx                  The one solution; opens on Linux, macOS and Windows
  THIRD-PARTY-NOTICES.txt             Third-party content bundled with or used by the application
  src/
    PdfSideBySide.UI/                 Shared XAML project (.shproj + .projitems); produces no assembly
      App.xaml, App.xaml.cs           Bootstrap: fonts, service resolver, design mode, Debug logging
      Views/MainPage.xaml(.cs)        The single page: two panes and the middle control column
    PdfSideBySide.Core/               View models; carries every non-head package reference
      Helpers/HostHelper.cs           The IHostBuilderProvider handed to SimpleServiceResolver
      ViewModels/MainViewModel.cs     Owns the comparison, the renderer and every command
      ViewModels/DocumentPaneViewModel.cs   One pane's bindable state and its browse command
    PdfSideBySide.LinuxX11/           Head: Program.cs plus exactly one runtime package
    PdfSideBySide.LinuxWayland/       Head
    PdfSideBySide.LinuxFrameBuffer/   Head; also sets orientation and opts into a file picker
    PdfSideBySide.MacOS/              Head
    PdfSideBySide.Win32Skia/          Head
    PdfSideBySide.WinWpfSkia/         Head; net10.0-windows, forces a software render surface
    libs/
      PdfSideBySide.PdfRender/        The domain library; no CodeBrix.Platform dependency at all
        DocumentSide.cs               Left / Right
        PdfComparison.cs              Two documents, the "both" moves and the "adjust right" moves
        Documents/PdfPageDocument.cs  One opened PDF: bytes read once, page count, 1-based cursor
        Documents/DocumentPath.cs     Path normalization and same-file comparison (internal)
        Documents/DuplicateDocumentException.cs   The same file chosen on both sides
        Rendering/PageRenderer.cs     PDFium rasterization to PNG plus the bounded cache
        Rendering/RenderedPage.cs     Record: file, page number, pixel size, PNG bytes
        Viewing/ComparisonView.cs     One shared zoom plus one pan position per pane
        Viewing/ViewZoom.cs           The zoom ladder and the render-resolution rule
        Viewing/PanPosition.cs        One pane's pan fractions
        Viewing/PanDirection.cs       Up / Down / Left / Right
        InternalsVisibleTo.cs         Opens internals to the .Tests assembly
  tests/
    libs/
      PdfSideBySide.PdfRender.Tests/  xUnit v3 on Microsoft.Testing.Platform
        Helpers/TestPdfs.cs           The fixture path and the synthetic-PDF writer
        assets/Inanna.pdf             Committed fixture, copied beside the test binary
```

Dependencies run one way. `PdfSideBySide.PdfRender` is the bottom of the stack: it references the
CodeBrix.PdfRasterizer library and nothing else, and knows nothing about XAML, CodeBrix.Platform or
view models. `PdfSideBySide.Core` project-references it and adds the CodeBrix.Platform, font and
generic-host packages; that is the only place a non-head package is declared. Each of the six heads
project-references `PdfSideBySide.Core`, adds exactly one CodeBrix.Platform runtime package for its
own backend, and **file-links** the shared UI by importing
`..\PdfSideBySide.UI\PdfSideBySide.UI.projitems`. The `.shproj` produces no assembly of its own:
`App.xaml`, `MainPage.xaml` and their code-behind are compiled into each head, which is why each
head's `Program` can see `App` without a using directive (they share the namespace
`PdfSideBySide`). The test project project-references the render library only, so the domain tests
never load a UI package.

One naming detail is worth catching early: the Core library sets `RootNamespace` to
`PdfSideBySide`, not `PdfSideBySide.Core`, so the view models live in `PdfSideBySide.ViewModels`
inside an assembly called `PdfSideBySide.Core`. `MainPage.xaml` therefore has to name both:
`xmlns:vm="clr-namespace:PdfSideBySide.ViewModels;assembly=PdfSideBySide.Core"`.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The whole UI: `Application`, `Window`, `Frame`, `Page`, every control in `MainPage.xaml`, and the "Simple" toolkit — `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `[AffectsAllCommands]`, `IXamlRootGetter`, the inherited `ShowError` helpers. It also supplies the `Windows.Storage.Pickers` file picker | `src/PdfSideBySide.Core/PdfSideBySide.Core.csproj`, `src/PdfSideBySide.UI/App.xaml.cs`, both view models, `src/PdfSideBySide.UI/Views/MainPage.xaml(.cs)` |
| CodeBrix.Platform runtime for each head | One rendering backend per head — X11, Wayland, framebuffer, macOS, Win32 and WPF — each selected by its own `Use…()` call on the host builder | the six `src/PdfSideBySide.<Head>/PdfSideBySide.<Head>.csproj` files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Roboto | Supplies Roboto as the application-wide default text font, the `RobotoFont` XAML resource, and the Armenian and Georgian Noto Sans faces registered as fallbacks | `src/PdfSideBySide.UI/App.xaml`, `src/PdfSideBySide.UI/App.xaml.cs`, `src/PdfSideBySide.Core/PdfSideBySide.Core.csproj` |
| CodeBrix.PdfRasterizer | The PDF engine: `GetPageCount()` when a document is opened, and `RasterizeToImage()` to turn one page at a chosen resolution into an image. Bundles its own PDFium natives per runtime identifier | `src/libs/PdfSideBySide.PdfRender/Documents/PdfPageDocument.cs`, `src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`, `src/libs/PdfSideBySide.PdfRender/PdfSideBySide.PdfRender.csproj` |
| CodeBrix.Imaging | PNG-encodes the rasterized page and reports its pixel size. Arrives transitively through CodeBrix.PdfRasterizer — the csproj never names it | `src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs` |
| CodeBrix.PdfDocuments | Writes the small synthetic multi-page PDFs the tests use (`PdfDocument`, `XGraphics`, `XBrushes`, `XRect`). Also transitive through CodeBrix.PdfRasterizer | `tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs` |
| SilverAssertions | The `Should()` assertion style used throughout the tests | all files under `tests/libs/PdfSideBySide.PdfRender.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | Supplies the generic-host builder that `SimpleServiceResolver` builds the container from | `src/PdfSideBySide.Core/Helpers/HostHelper.cs`, `src/PdfSideBySide.Core/PdfSideBySide.Core.csproj` |
| Microsoft.Extensions.Logging.Console | The Debug-only console logger factory handed to the platform's ambient logging | `src/PdfSideBySide.UI/App.xaml.cs` |
| xUnit v3 (with its Visual Studio runner) and Microsoft.NET.Test.Sdk | The test framework and the Microsoft.Testing.Platform host | `tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj` |
| PDFium (native) | The actual page rasterization; ships inside the CodeBrix.PdfRasterizer package under `runtimes/<rid>/native/` for each supported runtime identifier, each with its own license file beside it | pulled in by `src/libs/PdfSideBySide.PdfRender/PdfSideBySide.PdfRender.csproj` |

## Worth studying in this application

### The comparison model: two cursors and one deliberate offset

Everything the application means by "comparing" lives in `PdfComparison`, a plain class in
`src/libs/PdfSideBySide.PdfRender/PdfComparison.cs` with no UI type in it anywhere. It holds two
`PdfPageDocument` objects, each with its own 1-based cursor, and exposes the four moves the middle
column drives — `MoveBothPrevious()`, `MoveBothNext()`, `AdjustRightPrevious()`,
`AdjustRightNext()` — as boolean-returning methods with matching `CanMoveBothNext`-style
properties. That pairing is what lets `MainViewModel` wire each move straight to a `SimpleCommand`
predicate without mirroring any page state into properties of its own.

Read `PdfComparison.cs` first, then `Documents/PdfPageDocument.cs` for the cursor, then
`tests/libs/PdfSideBySide.PdfRender.Tests/PdfComparisonTests.cs`, which covers every rule below.
The "both" moves call `MoveNext()` on each document unconditionally and OR the two results;
short-circuiting with `||` would skip the second document, so the two locals are deliberate. Each
cursor clamps at *its own* last page, which is exactly what preserves the offset the reader set
with the adjustment buttons until one document runs out. And every move that actually changes a
page resets the shared view to fit-the-page through one private `ResetViewIf()` helper — the
comparison owns the `View`, so a caller cannot forget that rule.
[Keep two documents in step while letting the user offset one](../BLUEPRINTS-DocumentsAndData.md#keep-two-documents-in-step-while-letting-the-user-offset-one)

### Opening a document, and refusing the same file twice

`PdfPageDocument.OpenAsync()` normalizes the path, checks the file exists, reads the bytes once,
and asks a short-lived `PageRasterizer` for the page count. Two details in it repay attention: the
exception filter `when (e is not OperationCanceledException)` keeps a cancellation from being
reported as "not a PDF", and a page count below 1 is turned into an `InvalidDataException` naming
the file rather than trusted. The bytes are kept and handed to every later render, so the file is
never re-opened and the reader can move or delete it mid-session.

The duplicate rule sits one level up. `PdfComparison.OpenAsync()` compares the incoming path
against the other side's with `DocumentPath.AreSame()` and throws `DuplicateDocumentException`
*before* opening anything, so the pane keeps whatever it had. That exception carries `FilePath` and
`AlreadyOpenSide` as properties in addition to a message already phrased for a human, and
`MainViewModel.BrowseAsync()` catches it first — `await ShowError(e.Message)` — falling through to
`await ShowError(e, "Could not open the PDF document.")` for anything else. `DocumentPath` is
`internal` and testable only through `InternalsVisibleTo.cs`; it picks its `StringComparison` from
the operating system rather than hard-coding one, and trims the trailing separator that
`Path.GetFullPath()` leaves behind.
[Open a PDF and read its page count with the CodeBrix PdfRasterizer library](../BLUEPRINTS-DocumentsAndData.md#open-a-pdf-and-read-its-page-count-with-the-codebrix-pdfrasterizer-library),
[Treat two spellings of one path as the same file](../BLUEPRINTS-DocumentsAndData.md#treat-two-spellings-of-one-path-as-the-same-file),
[Report a domain rule violation as a typed exception the view model can catch](../BLUEPRINTS-MVVM.md#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch)

### Rendering a page: off the UI thread, cached, and at the zoom's resolution

`Rendering/PageRenderer.cs` is the service the view model talks to, and it is the single most
copyable file in the folder. It holds one long-lived `PageRasterizer` (created with the renderer,
disposed with it, rather than one per render) and does the work inside `Task.Run` because, as its
own comment records, PDFium renders synchronously — awaiting the rasterizer alone would still block
the calling thread. Only encoded bytes and a pixel size escape that block; the image is disposed
inside it, so nothing that needs a `using` crosses back to the UI thread. The result is a small
immutable `RenderedPage` record.

Caching is a private detail of the same class: a `Dictionary` plus a `LinkedList` for
most-recently-used order, guarded by a `System.Threading.Lock` because renders run on worker
threads, keyed by file, page number and resolution together. Two things keep it honest, and both
are needed — the resolution is part of the key, so a low-resolution page is never served for a
high-resolution request, and the `Dpi` setter clears the cache, so stale entries do not accumulate
after a global resolution change. A capacity below 1 disables caching rather than throwing. Because
a cache hit returns the *same* `RenderedPage` instance, nothing should mutate one.
[Rasterize a PDF page to PNG off the UI thread](../BLUEPRINTS-DocumentsAndData.md#rasterize-a-pdf-page-to-png-off-the-ui-thread),
[Cache rendered results with a bounded most recently used cache](../BLUEPRINTS-MVVM.md#cache-rendered-results-with-a-bounded-most-recently-used-cache)

### The zoom ladder and why 100% is the minimum

`Viewing/ViewZoom.cs` owns the ladder and the resolution rule. In this application 100% means "the
whole page fits the pane", not "actual size", and it is the *minimum* — the reader can never zoom
out past fit-the-page, which is why there is no empty grey border state to design for. From there
the ladder climbs to 1000%.

`GetRenderDpi(baseDpi)` scales the renderer's base resolution by the zoom factor and caps the
result, so zooming in sharpens text instead of magnifying a blurry bitmap, while the top of the
ladder is scaled up on screen a little instead of rendering an enormous image. The view model calls
the per-call `RenderCurrentPageAsync(document, dpi, token)` overload deliberately: setting the
renderer's own `Dpi` property instead would clear the cache on every zoom step and destroy the base
value the multiplier needs.
[Choose the render resolution from the zoom level](../BLUEPRINTS-GraphicsAndRendering.md#choose-the-render-resolution-from-the-zoom-level)

### One in-flight render per pane, latest request wins

`MainViewModel.RenderSideAsync(side)` is the whole concurrency story. It keeps one
`CancellationTokenSource` per pane, cancels the previous one for that pane before starting, sets
that pane's busy flag, awaits the renderer, and pushes the result only if its own token was not
cancelled. `OperationCanceledException` is swallowed in its own catch block because it is the
expected outcome of a superseded render, not a fault.

Three details are easy to get wrong. The busy flag is cleared in `finally` only when this render
was *not* superseded, so a cancelled render cannot switch off a busy bar the newer render just
switched on. The result is re-checked against the token before it is shown, because the renderer
can return a cached page without ever observing cancellation. And rendering both panes
concurrently with `Task.WhenAll` is safe precisely because `PageRenderer` locks its cache and does
the rasterizer call inside `Task.Run`. When only Document 2 moved — an "adjust right" step — only
the right pane is re-rendered; `StepAsync(move, renderLeft)` carries that flag.

For a view model that is created and destroyed repeatedly, the shape to prefer is to implement
`IDisposable` on it, canceling and disposing both token sources and the `PageRenderer` (which owns
the `PageRasterizer` and the cache).
[Run one render per pane with latest request wins cancellation](../BLUEPRINTS-MVVM.md#run-one-render-per-pane-with-latest-request-wins-cancellation)

### One version counter instead of a dozen notifications

The thing that changes when the reader presses a button is an object graph — a zoom level, two pan
positions, two page cursors — none of which are bound properties. Rather than have the page
subscribe to all of them, `MainViewModel` exposes a single `int ViewVersion` and bumps it from one
private `ViewChanged()` method that also re-notifies the derived `ZoomLabel` and refreshes every
command. `MainPage` watches that one property name with
`args.PropertyName == nameof(MainViewModel.ViewVersion)` and re-applies the whole view.

The value of the pattern is that adding a new kind of change means calling `ViewChanged()` and
nothing else. A counter rather than a `bool` or an event means any increment reads as a change and
it survives being read late.
[Signal a non property model change to the view with a version counter](../BLUEPRINTS-MVVM.md#signal-a-non-property-model-change-to-the-view-with-a-version-counter)

### Commands whose enablement lives in the model

Every navigation button here is gated by facts inside `PdfComparison` and `ComparisonView`, not by
properties on the view model, so `[AffectsCommands]` has nothing to hang on. The application uses
both halves of the answer: `[AffectsAllCommands]` on the one genuine bound property that gates
everything (`IsBusy`), and an explicit `RaiseNavigationCanExecute()` called from `ViewChanged()`,
the single method that already runs whenever the model moved.

Two syntax details in `MainViewModel` are load-bearing. Each command is created with `field ??=` on
an expression-bodied property so the same instance is handed to every binding — a plain
`=> new SimpleCommand(...)` would make `RaiseCanExecuteChanged()` refresh a command nothing is
bound to. And each async execute delegate is cast `(Func<Task>)`; without the cast a lambda
returning a `Task` binds to the synchronous `Action` overload and the command completes immediately
while the work runs unobserved. The pan buttons take the pattern one step further: they share one
`PanCommand` and pass a plain XAML string such as `CommandParameter="Left:Up"`, parsed with two
`Enum.TryParse` calls whose failure path returns `false` from `CanExecute`, so a typo disables the
button instead of throwing.
[Refresh CanExecute when the gating state is not a bound property](../BLUEPRINTS-MVVM.md#refresh-canexecute-when-the-gating-state-is-not-a-bound-property),
[Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension)

### Two panes, one parent, and the state pushed between them

`MainViewModel` owns the comparison, the renderer and every command that affects both panes.
`DocumentPaneViewModel` is a `SimpleViewModel` holding only one pane's bindable state — file path,
file name, page label, the rendered `BitmapImage` and its pixel size, and the two `Visibility`
properties — plus the one command that belongs to it. The parent passes that command's body in as a
`Func<Task>` at construction (`new DocumentPaneViewModel("Document 1", () => BrowseAsync(DocumentSide.Left))`)
and pushes state in through `internal` methods: `ShowDocument()`, `UpdatePageLabel()`,
`SetRendering()`, `ShowPageAsync()`. Bindings only ever read, and the parent's `LeftPane` and
`RightPane` are get-only auto-properties that are never reassigned.

The XAML side is the other half: `MainPage.xaml` scopes each pane's `Grid` with
`DataContext="{d:Binding LeftPane}"`, so every binding inside that region is written against one
pane and the two panes are literally the same markup twice. Read `MainViewModel.cs`, then
`DocumentPaneViewModel.cs`, then the page. Note that `if (IsDesignMode(true)) { return; }` must be
the first line of *both* constructors, and that the child therefore leaves `Title` and
`BrowseCommand` null at design time.
[Compose a page from a parent view model and child view models](../BLUEPRINTS-MVVM.md#compose-a-page-from-a-parent-view-model-and-child-view-models),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer)

### Getting PNG bytes onto the screen, and showing pane state without converters

`DocumentPaneViewModel.ShowPageAsync()` decodes the renderer's bytes into a `BitmapImage` through
`stream.AsRandomAccessStream()` and `SetSourceAsync()`. The order of the three assignments that
follow is deliberate and commented in the file: width and height first, the image last, so anything
reacting to `PageImage` already sees the matching pixel size. The method's doc comment records that
it must be called on the UI thread; the render path reaches it after the `Task.Run` has completed.

The pane's placeholder and busy bar avoid a converter entirely. `PlaceholderVisibility` and
`RenderingVisibility` are `Visibility`-typed derived properties, and each is re-notified explicitly
from the setter of the `bool` or `string` it depends on — `SetProperty` only notifies the property
it is given, so the fan-out has to be written out. Both underlying setters are null-tolerant
(`value ?? string.Empty`), so a cleared pane binds to an empty string rather than to null. This is
also why the Core library references `Microsoft.UI.Xaml`, and why the domain library, which has no
XAML reference at all, has no `Visibility` in it.
[Turn image bytes into a bound BitmapImage](../BLUEPRINTS-ViewsAndControls.md#turn-image-bytes-into-a-bound-bitmapimage),
[Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties)

### Where the layout arithmetic lives

Zoom and pan are stored abstractly: `ViewZoom` holds a percentage, and each `PanPosition` holds two
fractions of the scrollable range where 0.5 is centered. Storing pan as a *fraction* rather than a
pixel offset is what makes it survive a zoom change — the fraction stays put and the pixel offset
follows. `ComparisonView.PanStepFraction` turns "a quarter of the visible area" into a fraction of
the scrollable range with the derivation spelled out in its doc comment: at zoom factor `f` the
page is `f` viewports wide, so the scrollable range is `f - 1` viewports.

Only the page knows how large the viewport actually is, so `MainPage.xaml.cs` combines the two in
`ApplyView(side)`: it computes the fit-to-viewport scale, multiplies by the zoom factor, sizes the
`Image`, then scrolls the `ScrollViewer` to the pane's pan fractions. Two lines there are worth
copying. `scroller.UpdateLayout()` must run before `ChangeView(...)`, because `ScrollableWidth` and
`ScrollableHeight` are stale until the viewer has measured the newly sized image;
`Math.Max(0, scroller.ScrollableWidth)` guards the 100% case where nothing scrolls. The
`ScrollViewer` sets `ZoomMode="Disabled"` so the control's own zoom cannot fight the application's
ladder, and the `Image` uses `Stretch="Fill"` with an explicitly set `Width`/`Height`, which is what
makes the zoom exact rather than letting the control choose a fit. The size and the pan have to be
re-applied on `SizeChanged`, on `ViewVersion` changes and on each pane's `PageImage` change; missing
any one leaves an image the wrong size.

For a new application, the shape to prefer keeps the arithmetic in the view model: the page reports
its viewport size through a small bridge method whenever it changes (a one-line `SizeChanged`
handler per pane), and the view model exposes computed image size and scroll offsets per pane. That
keeps the formula testable alongside `ComparisonViewTests` and `PanPositionTests`.
[Let the page do the layout arithmetic only it can do](../BLUEPRINTS-ViewsAndControls.md#let-the-page-do-the-layout-arithmetic-only-it-can-do)

### Choosing files: the picker, the XamlRoot, and the framebuffer head

The browse buttons reach the operating system's file picker through
`MainViewModel.PickPdfPathAsync()`, which configures a `FileOpenPicker` with
`SuggestedStartLocation` and a `.pdf` entry in `FileTypeFilter` (extensions take the leading dot).
`PickSingleFileAsync()` returns null when the reader cancels, and the null check sits inside the
`try`/`finally` so the busy flag still clears. The picker type comes from `Windows.Storage.Pickers`,
which the Core library already has from CodeBrix.Platform.

Dialogs need somewhere to attach, and only the page can supply that. `MainPage` hands the view model
a *getter*, not a value, from `DataContextChanged`:
`(DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot)`. A lambda is required because
`XamlRoot` is null until the page is in the visual tree; `DataContextChanged` rather than the
constructor is required because XAML supplies the DataContext during `InitializeComponent()`; and
the `as` plus `?.` means a page whose DataContext is something else simply does nothing.

Not every head can show a picker at all. `PdfSideBySide.LinuxFrameBuffer/Program.cs` has no window
manager to ask, so it opts in explicitly with `EnableFileOpenPicker(new FilePickerOptions { … })` on
the same builder lambda that sets orientation and auto-rotation, and the platform draws that picker
itself. `RestrictToFolder` and `RequiredExtension` are the only guard rails a kiosk-style device
has. Because a head can lack a picker, the shape to prefer moves the picker call behind a one-method
bridge interface that the page or head implements, so the view model can show a clear message
("pass the two PDF paths on the command line") instead of the absence being an exception it catches.
[Pick a file to open through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show),
[Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head)

### Startup: six heads, one App, and two paths off the command line

Each head's `Program.Main` holds no application logic: it calls `App.InitializeLogging()`, names the
`App` subclass, selects one backend with a single `Use…()` call, builds and runs. Five of the six
are identical apart from that call; the WinWpfSkia head adds one guarded cast after `Build()` to
force a software render surface. `App.InitializeLogging()` runs *before* the host is built, because
the logging adapter has to be in place before the platform starts writing to it, and `[STAThread]`
sits on `Main` in every head including the Linux and macOS ones.

`App.xaml.cs` does four things in a fixed order before `InitializeComponent()`: sets the default
font family and the fallback faces, creates the `SimpleServiceResolver` from `HostHelper.GetHost()`,
and calls `SimpleViewModel.SetIsDesignMode(false)`. The last of those is the sharpest edge in the
folder. `MainPage.xaml` instantiates `MainViewModel` from XAML, so `InitializeComponent()` is what
runs the view-model constructor — a constructor whose first line is `if (IsDesignMode(true)) { return; }`.
Set design mode off too late and the view model returns from its constructor without ever building
itself. The resolver must be created even though this application's registration block is empty.

Startup document loading lives in the view model, not in `Main`:
`OpenStartupDocumentsAsync()` reads `Environment.GetCommandLineArgs()` itself, so the two paths are
at indices 1 and 2 and the guard is `arguments.Length < 3` — the heads never forward their
`string[] args`. It is started as `_ = OpenStartupDocumentsAsync();`, discarded deliberately, with
every exception caught inside so nothing is left unobserved. Because it runs from the constructor it
can complete before the page has supplied a `XamlRoot`, so an error dialog raised that early has
nowhere to attach; deferring the load until the page signals it is ready is the safer shape.

Registering the renderer behind an interface with `SimpleServiceResolver` at startup, and resolving
it in the view model, is the shape to prefer over the `new` in the field initializers here: it is
also what would make `MainViewModel` itself testable.
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Load documents named on the command line during startup](../BLUEPRINTS-MVVM.md#load-documents-named-on-the-command-line-during-startup),
[Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor)

### A UI-free library, and the tests it makes possible

The layout of this folder is the point of the previous sections. `PdfSideBySide.PdfRender` lives
under `src/libs` and references one package; it has no CodeBrix.Platform reference at all, so
`tests/libs/PdfSideBySide.PdfRender.Tests` never loads a UI package and the whole comparison model
can be exercised without starting a head. `InternalsVisibleTo.cs` is its own file at the library
root, holding nothing but the attribute and naming the `.Tests` assembly exactly, which is what
makes `internal static class DocumentPath` testable.

The test classes are worth reading as a specification: `PdfPageDocumentTests` for opening and cursor
clamping, `PdfComparisonTests` for the two-document rules, `PageRendererTests` for the PNG signature
of the returned bytes and the cache behavior, and `ComparisonViewTests`, `PanPositionTests` and
`ViewZoomTests` for the view arithmetic. `Helpers/TestPdfs.cs` holds both test-data sources: a
synthetic-PDF writer built on CodeBrix.PdfDocuments, which gives each test exactly the page count it
needs and draws a rectangle whose position derives from the page index (so "different pages render
to different images" is actually testable), and the path of the committed `assets/Inanna.pdf`
fixture, located with `AppContext.BaseDirectory` to pair with the csproj's
`None Include="assets\**"` item. Every synthetic document goes into its own GUID-named temp folder,
so tests writing files with the same name cannot collide.

Two sharp edges. The fixture's page size is a whole number of inches, which is what lets
`PageRendererTests` assert exact pixel dimensions for a given resolution; a fixture with a
fractional page size would force a tolerance. And the synthetic writer's PDF library arrives
transitively through CodeBrix.PdfRasterizer, as does the imaging library `PageRenderer` encodes
with — convenient, but it means an upgrade of the rasterizer moves those libraries too, so a project
that genuinely depends on them should say so directly.
[Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface),
[Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project),
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Build the binary inputs your tests need instead of committing them](../BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them),
[Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary),
[Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on)

### Sharing one page across six heads

`PdfSideBySide.UI` is a shared project: a `.shproj` paired with a `.projitems`, producing no
assembly of its own. Each head imports the `.projitems` with
`<Import Project="..\PdfSideBySide.UI\PdfSideBySide.UI.projitems" Label="Shared" />` so `App.xaml`,
`MainPage.xaml` and their code-behind compile *into* the head and can see the head's own types. The
`SharedGUID` in the `.projitems` and the `ProjectGuid` in the `.shproj` are the same value; that
pairing is what makes the mechanism work.

Every head also repeats a `<Page Include="**\*.xaml" …/>` plus `<None Remove="**\*.xaml" />` item
group, without which MSBuild treats the `.xaml` files as content and the pages never compile. Each
head then adds exactly one CodeBrix.Platform runtime package — the head csprojs say so in a comment
— and everything else comes from `PdfSideBySide.Core`, so adding a package or a head does not mean
editing seven project files. Only the WinWpfSkia head targets `net10.0-windows`; the Win32Skia head,
despite being a Windows head, targets plain `net10.0`.

Finally, every icon in the middle column is a `FontIcon` glyph rather than an image asset, which is
what keeps the arrows and the magnifiers visible on a device with no system fonts installed.
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project),
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package),
[Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts)

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the third-party content
bundled with, or used at run time by, this application: the PDFium native libraries that the
CodeBrix.PdfRasterizer package redistributes with every build, and the committed test fixture
`assets/Inanna.pdf` — a Wikipedia article rendered to PDF, whose text is under a Creative Commons
Attribution-ShareAlike license — which is used only by the test project and is not part of the
application. No fonts, models or other assets live in this folder, and nothing is downloaded at run
time. The PDF documents a reader opens with the application are their own; nothing of that content
is redistributed.

## License

PdfSideBySide is licensed under the Apache License, Version 2.0, see [../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
