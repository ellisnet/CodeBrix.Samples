# NotionDocumentCreator

NotionDocumentCreator turns pages from a Notion workspace into a single
print-ready, book-designed PDF. You paste your own Notion integration token and
the ID (or URL) of a page or database into the header bar and click Connect; the
application validates the token, resolves what you pasted, and shows the page
tree in a `TreeView` whose branches load their children the first time you expand
them. Tapping a row shows a small preview pane on the right (title, child-page
count, last-edited date, cover or icon image, and the first block or two of
text). Every checked page becomes a chapter of the book, in the order the rows
appear in the tree, and the first checked page becomes the cover. You pick a trim
size, choose an output path through a native save dialog or type one, and click
Create!. The service then reads each selected page's full block tree, downloads
the images, video and audio it refers to, composes a fully typeset book (cover
plate, chapter openers, running heads, continuous folios, booktabs tables,
callout sidebars, code panels, figures with credits and captions, cross
references that print real page numbers), renders it to PDF and reports what it
produced in a dialog.

It is a reference for three things in particular: calling a REST API from a view
model through a service interface, composing a substantial PDF with
CodeBrix.PdfDocCreate out of fonts embedded in your own assembly, and degrading
gracefully when a head has no file dialog or the host has no ffmpeg.

## What this sample shows a CodeBrix.Platform developer

- Keep the whole document pipeline in a UI-free class library that the view model
  reaches only through one interface: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- Call a REST API from a service the view model resolves, never from the view
  model itself: [Call a REST API behind a service interface the view model resolves](../BLUEPRINTS-DocumentsAndData.md#call-a-rest-api-behind-a-service-interface-the-view-model-resolves).
- Hide a fetch, download, compose and render pipeline behind a single awaitable
  service method that reports progress: [Run a multi stage pipeline behind one service method](../BLUEPRINTS-DocumentsAndData.md#run-a-multi-stage-pipeline-behind-one-service-method).
- Accept whatever identifier form a user pastes and normalize it before it reaches
  the API: [Normalize a user entered ID or URL before calling an API](../BLUEPRINTS-DocumentsAndData.md#normalize-a-user-entered-id-or-url-before-calling-an-api).
- Resolve one identifier that could name any of several object kinds by trying each
  retrieval in turn: [Resolve an ID that may be one of several object kinds](../BLUEPRINTS-DocumentsAndData.md#resolve-an-id-that-may-be-one-of-several-object-kinds).
- Walk a nested API tree one level per request without looping forever on a
  self-referencing node: [Read a nested tree from an API with a cycle guard](../BLUEPRINTS-DocumentsAndData.md#read-a-nested-tree-from-an-api-with-a-cycle-guard).
- Serialize outbound calls and space them out so you stay inside a published rate
  limit instead of provoking retries: [Pace outbound API calls with a rate gate](../BLUEPRINTS-DocumentsAndData.md#pace-outbound-api-calls-with-a-rate-gate).
- Build a real book: an unnumbered cover section, one section per chapter, running
  heads, and folios that start once and never restart: [Compose a book with sections styles running heads and folios](../BLUEPRINTS-DocumentsAndData.md#compose-a-book-with-sections-styles-running-heads-and-folios).
- Derive every margin, type size and rule in a document from a single page-size
  choice, so a new trim needs no new tuning: [Derive a whole document theme from one page size choice](../BLUEPRINTS-DocumentsAndData.md#derive-a-whole-document-theme-from-one-page-size-choice).
- Embed OFL-licensed fonts in your own assembly and register them with the PDF font
  system so output looks the same everywhere: [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).
- Read a font's real coverage from its cmap table and drop what it cannot draw
  rather than printing empty boxes: [Drop characters your embedded fonts cannot render](../BLUEPRINTS-DocumentsAndData.md#drop-characters-your-embedded-fonts-cannot-render).
- Turn annotated rich-text runs into formatted PDF text whether or not they carry a
  link: [Write rich text runs into a paragraph or a hyperlink](../BLUEPRINTS-DocumentsAndData.md#write-rich-text-runs-into-a-paragraph-or-a-hyperlink).
- Write the same content into either a document section or a table cell through one
  small target adapter: [Render into either a section or a table cell](../BLUEPRINTS-DocumentsAndData.md#render-into-either-a-section-or-a-table-cell).
- Place numbered, framed figures with their captions and credit lines kept with the
  plate: [Place numbered framed figures with credit lines](../BLUEPRINTS-DocumentsAndData.md#place-numbered-framed-figures-with-credit-lines).
- Look ahead one sibling so a credit paragraph following an image is typeset as the
  figure's credit, not as body text: [Pair a figure with the credit paragraph that follows it](../BLUEPRINTS-DocumentsAndData.md#pair-a-figure-with-the-credit-paragraph-that-follows-it).
- Print a visible marker and record a warning for content you cannot render, so
  nothing silently vanishes and nothing throws mid-document: [Keep unsupported content visible instead of failing the document](../BLUEPRINTS-DocumentsAndData.md#keep-unsupported-content-visible-instead-of-failing-the-document).
- Download every referenced file once per run into a private temp folder that
  deletes itself when the run ends: [Download run scoped media into a self cleaning temp cache](../BLUEPRINTS-MediaAndVision.md#download-run-scoped-media-into-a-self-cleaning-temp-cache).
- Ask an external tool for a video poster frame and fall back to a card when the
  tool is not installed: [Extract a video poster frame and degrade when the external tool is missing](../BLUEPRINTS-MediaAndVision.md#extract-a-video-poster-frame-and-degrade-when-the-external-tool-is-missing).
- Decode, cap and re-encode a downloaded image so the document embedder can take it:
  [Normalize a downloaded image before embedding it in a document](../BLUEPRINTS-GraphicsAndRendering.md#normalize-a-downloaded-image-before-embedding-it-in-a-document).
- Write bound properties, `SimpleCommand` commands and `[AffectsCommands]` the way
  the family does: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Run a long command with a busy flag, a progress bar and a status line that all
  come from the view model: [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- Marshal results from background work back onto the UI thread before touching bound
  state: [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Give each tree row its own small view model and load its children the first time it
  expands: [Load a tree lazily as the user expands it](../BLUEPRINTS-MVVM.md#load-a-tree-lazily-as-the-user-expands-it).
- Discard an async result that arrives after the user has moved on to another
  selection: [Ignore a stale async result when the selection moved on](../BLUEPRINTS-MVVM.md#ignore-a-stale-async-result-when-the-selection-moved-on).
- Swap placeholders and content with computed `Visibility` properties instead of
  converters or code-behind: [Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).
- Ask and tell from the view model with the `SimpleViewModel` dialog helpers, without
  a reference to the page: [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- Guard a view model constructor so the XAML designer can instantiate it without a DI
  container: [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Dispose the commands, the resolved service and the bridge delegate a view model
  holds: [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- Let the view model ask for a save location through a bridge interface the page
  fills in: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- Hand the view model a `XamlRoot` getter so its dialogs have somewhere to attach:
  [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- Decode the percent-encoded path some pickers return before anything writes to disk:
  [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Bind a `TreeView` to a view model tree whose checkboxes select each node
  independently: [Bind a TreeView to a view model tree with checkboxes](../BLUEPRINTS-ViewsAndControls.md#bind-a-treeview-to-a-view-model-tree-with-checkboxes).
- Take a secret in a `PasswordBox`, keep it in a session-only property and never write
  it anywhere: [Take a secret token in a PasswordBox and keep it out of storage](../BLUEPRINTS-ViewsAndControls.md#take-a-secret-token-in-a-passwordbox-and-keep-it-out-of-storage).
- Re-key the Fluent theme's own brush resources so buttons, checkboxes, dialogs and
  picker chrome all follow your palette: [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette).
- Build header and action bars that reflow to a narrow window with the FlexPanel
  add-in: [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in).
- Draw every icon from `FontIcon` glyphs so the UI survives on a device with no system
  fonts: [Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts).
- Give each head a tiny `Program.Main` that names its backend and nothing else:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Do fonts, services and design-mode setup once in the `App` constructor, before
  `InitializeComponent()`: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Give `SimpleServiceResolver` a generic host builder through a small provider type:
  [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Let a library register its own services with one `AddXxx()` extension method:
  [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- Wire console logging into the platform's ambient logger factory in Debug builds
  only: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Opt the Linux framebuffer head into the save picker and the software keyboard it
  needs: [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- Force the software render surface on the WinWpfSkia head after the host is built:
  [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Test a document renderer by walking the object model it produces instead of parsing
  a PDF: [Test a document renderer against the object model it produces](../BLUEPRINTS-Testing.md#test-a-document-renderer-against-the-object-model-it-produces).
- Keep the tests that need a live account and network out of the default run:
  [Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).

## Building, running and testing

There is one solution, and it opens everywhere.

| Solution | Contains | Open on |
| --- | --- | --- |
| `NotionDocumentCreator/NotionDocumentCreator.slnx` | The shared UI project, the Core library, all six heads, a `Libraries/` folder holding `NotionDocumentCreator.CreateDocument`, and a `Tests/` folder holding its test project | Linux, macOS or Windows |

Its own comment describes it as a CodeBrix.Platform application with everything
that builds with the plain .NET SDK on Linux, macOS and Windows. There are no
native (non-Skia) heads here: no WinUI 3, WPF or .NET MAUI project.

| Head project | Platform |
| --- | --- |
| `src/NotionDocumentCreator.LinuxX11` | Linux, X11 |
| `src/NotionDocumentCreator.LinuxWayland` | Linux, Wayland |
| `src/NotionDocumentCreator.LinuxFrameBuffer` | Linux, framebuffer (no desktop) |
| `src/NotionDocumentCreator.MacOS` | macOS |
| `src/NotionDocumentCreator.Win32Skia` | Windows, native Win32 window |
| `src/NotionDocumentCreator.WinWpfSkia` | Windows, Skia hosted in a WPF window |

### Prerequisites

- The .NET 10 SDK. No workloads are needed. Every project targets `net10.0`
  except the WinWpfSkia head, which targets `net10.0-windows` and sets
  `EnableWindowsTargeting`, so the whole solution still restores and builds on
  Linux and macOS even though that head only runs on Windows.
- **A Notion integration token that you supply at run time.** Nothing ships with
  one and nothing stores one. You create an integration in your own Notion
  account, copy its token, and paste it into the `PasswordBox` in the header bar
  before clicking Connect. The value lives in a view-model property for the
  session, is trimmed and handed to the service's `ConnectAsync()`, and travels
  no further: there is no settings file, no environment variable and no
  credential store anywhere in `src/`, and the view model clears its reference in
  `Dispose()`.
- **A Notion page or database shared with that integration.** Creating the
  integration is not enough; the page has to be shared with it in Notion, which is
  exactly what the tree reader's failure message says when it is not.
- Network access to the Notion API and to the media URLs Notion hands back.
- **Optional:** `ffmpeg` and `ffprobe` on the host, used only to pull a poster
  frame out of a video and to probe media durations. Without them the application
  still produces the book; video and audio render as media cards and a note goes
  into the warnings list. No FFmpeg binaries are bundled.

### Running one head

From the `NotionDocumentCreator` folder:

```text
dotnet run --project src/NotionDocumentCreator.LinuxX11/NotionDocumentCreator.LinuxX11.csproj
```

Substitute any other head project for the one named there. The WinWpfSkia head
runs on Windows only.

### Testing

`NotionDocumentCreator/global.json` selects the Microsoft.Testing.Platform test
runner for this folder, and the test project sets `UseMicrosoftTestingPlatformRunner`
to match. That means a plain `dotnet test` can report that zero tests ran. The way
that works is to build the test project and run its executable directly:

```text
dotnet build tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionDocumentCreator.CreateDocument.Tests.csproj
```

then run the produced `NotionDocumentCreator.CreateDocument.Tests` executable from
its `bin` output folder.

Everything except the live integration tests runs offline, with no Notion account
and no network: the renderer tests stand a tiny base64 PNG in for downloaded media,
and the media-cache tests point at a closed loopback port to exercise the failure
path. `NotionDocumentServiceTests` is the exception. It skips itself in its
constructor unless both `NOTION_AUTH_TOKEN` and `NOTION_TEST_PAGE_ID` are set in
the environment, so it costs nothing in a normal run and is there when you want to
prove the whole pipeline against a real workspace.

## How the projects and folders are organized

```text
NotionDocumentCreator/
  NotionDocumentCreator.slnx            The one solution: heads, Core, Libraries/, Tests/
  global.json                           Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt               Bundled fonts, run-time FFmpeg use, Notion content
  README.md                             This file
  src/
    NotionDocumentCreator.UI/               Shared XAML project (.shproj + .projitems): App.xaml(.cs)
                                            and Views/MainPage.xaml(.cs), compiled into every head
    NotionDocumentCreator.Core/             The class library every head references; it carries all
                                            the shared packages and the application's view models
      Helpers/                              HostHelper (the host-builder provider) and
                                            FileDialogHelper (picker path cleanup)
      ViewModels/                           MainViewModel with its IFileSaveBridge, and
                                            NotionPageNodeViewModel for one tree row
    NotionDocumentCreator.LinuxX11/         Head: Program.cs plus one runtime package
    NotionDocumentCreator.LinuxWayland/     Head: Program.cs plus one runtime package
    NotionDocumentCreator.LinuxFrameBuffer/ Head: Program.cs also opts into the save picker
                                            and the software keyboard
    NotionDocumentCreator.MacOS/            Head: Program.cs plus one runtime package
    NotionDocumentCreator.Win32Skia/        Head: Program.cs plus one runtime package
    NotionDocumentCreator.WinWpfSkia/       Head: Program.cs also forces the software render surface
    libs/
      NotionDocumentCreator.CreateDocument/  The whole Notion-to-PDF pipeline; no UI reference at all
        Fonts/                               The embedded OFL fonts and their license texts
        Internal/                            Readers, the rate gate, the media cache and preparer,
                                             the image pipeline and poster extractor, font
                                             registration and coverage, the theme and styles, the
                                             composer and the block and rich-text renderers
        Models/                              The public DTOs: tree nodes, preview, request,
                                             progress, result, page sizes
        Services/                            INotionDocumentService and its implementation
        RegisterServices.cs                  The AddCreateDocument() DI extension method
        SelectionFlattening.cs               Depth-first flattening shared by the view model
                                             and the service
        InternalsVisibleTo.cs                Grants the test assembly access to Internal
  tests/
    libs/
      NotionDocumentCreator.CreateDocument.Tests/  Mirrors src/libs one for one
```

The dependency direction runs strictly one way and never doubles back. Each head
project references `NotionDocumentCreator.Core` and *file-links* the shared UI by
importing `NotionDocumentCreator.UI.projitems` as MSBuild shared items, so
`App.xaml`, `MainPage.xaml` and their code-behind are compiled into every head
rather than shipped as a separate assembly. Core project-references
`NotionDocumentCreator.CreateDocument` and carries every shared package: the
platform, the FlexPanel add-in, the Roboto font package, hosting and console
logging. Each head then carries exactly one runtime package and nothing else,
which its csproj comment states in capital letters; that is the rule to copy, and
the exact package to use is in the head's own csproj. The CreateDocument library
references no UI and no platform package at all, only the Notion, PDF, imaging and
video-processing libraries plus hosting abstractions, which is why the test project
can reference it directly and run headless.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The entire UI: `Application`, `Window`, `Frame`, `Page`, the Fluent theme, `TreeView`, `ComboBox`, `PasswordBox`, `ProgressBar` and `FileSavePicker`, plus the Simple toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`) and font configuration through `FeatureConfiguration.Font` | `src/NotionDocumentCreator.UI/`, `src/NotionDocumentCreator.Core/` |
| CodeBrix.Platform runtime for each head | Supplies the host-builder extension that head's `Program.cs` calls (`UseLinuxX11()`, `UseLinuxWayland()`, `UseLinuxFrameBuffer(...)`, `UseMacOS()`, `UseWindowsWin32()`, `UseWindowsWpf()`) and, on WPF, `WpfHost` and `RenderSurfaceType` | the six `src/NotionDocumentCreator.<Head>/Program.cs` files and their csproj files |
| CodeBrix.Platform.FlexPanel add-in | The wrapping header bar and the wrapping bottom action bar, with the `FlexPanel.Grow` and `FlexPanel.Basis` attached properties setting where each group goes when the window narrows | `src/NotionDocumentCreator.UI/Views/MainPage.xaml` |
| CodeBrix.Platform Roboto fonts | The application's default text font plus two script fallback faces, referenced by `ms-appx:///` URI so the UI does not depend on host fonts | `src/NotionDocumentCreator.UI/App.xaml`, `src/NotionDocumentCreator.UI/App.xaml.cs` |
| CodeBrix.NotionApi | The Notion REST client: the client factory and options, the retry policy, the users, pages, databases, data-sources and blocks endpoints, the block and rich-text type model, and the typed API exception | `src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionPageReader.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionConvert.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaPreparer.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs` |
| CodeBrix.PdfDocCreate (which brings CodeBrix.PdfDocuments with it) | The document object model the book is built in (documents, sections, paragraphs, formatted text, hyperlinks, tables, units, styles, headers and footers, bookmarks and page-reference fields), the renderer that turns it into a saved PDF, and the font system used to register the embedded faces | `src/libs/NotionDocumentCreator.CreateDocument/Internal/BookComposer.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BookStyles.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BookTheme.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs` |
| CodeBrix.Imaging | Decodes and normalizes every downloaded image for print, and supplies the imaging back-end the PDF image pipeline needs before any picture can be placed | `src/libs/NotionDocumentCreator.CreateDocument/Internal/ImagePipeline.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs` |
| CodeBrix.VideoProcessing | Probes media durations and grabs a video poster frame by invoking the host's ffprobe and ffmpeg | `src/libs/NotionDocumentCreator.CreateDocument/Internal/VideoPosterExtractor.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | Creates the default host builder behind `IHostBuilderProvider`, so `SimpleServiceResolver` has a container to build, and supplies `IServiceCollection` for `AddCreateDocument()` | `src/NotionDocumentCreator.Core/Helpers/HostHelper.cs`, `src/libs/NotionDocumentCreator.CreateDocument/RegisterServices.cs` |
| Microsoft.Extensions.Logging (Console, Abstractions) | Debug-only console logging wired into the platform's ambient logger factory, and an optional `ILogger` in the service that falls back to a null logger | `src/NotionDocumentCreator.UI/App.xaml.cs`, `src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs` |
| xUnit v3, SilverAssertions | The test project | `tests/libs/NotionDocumentCreator.CreateDocument.Tests/` |

## Worth studying in this application

Paths in this section are relative to this folder. To keep the prose readable,
files inside the CreateDocument library are named by their path within it, so
`Internal/BookFonts.cs` means
`src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs`.

### The whole pipeline lives in a library that has never heard of the UI

`NotionDocumentCreator.CreateDocument` is a plain class library with no reference
to CodeBrix.Platform, and everything it can do is stated by one interface,
`Services/INotionDocumentService.cs`: connect, load roots, load children, load a
preview, create the document. The implementation is registered as a singleton by
`RegisterServices.cs`, whose whole job is one `AddCreateDocument()` extension
method, and `src/NotionDocumentCreator.UI/App.xaml.cs` calls that inside `SimpleServiceResolver.CreateInstance()`
at startup. `MainViewModel` then resolves `INotionDocumentService` with
`GetService<T>()` and never touches a Notion type, a PDF type or an HTTP client
itself. Read those two small files first, then `Services/NotionDocumentService.cs`
to see how much sits behind them. The payoff is visible in the tests: the test
project references the library directly, with no head and no window, because there
is nothing in it that needs one.

Blueprints: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface),
[Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).

### A secret the application takes but never keeps

The token is typed into a `PasswordBox` in the header bar, two-way bound to
`MainViewModel.IntegrationToken`. That property is the only place the value lives.
`ConnectCommand` cannot execute while it is blank, because `[AffectsCommands]` on
the property re-evaluates `CanExecute` whenever it changes; when the command does
run, it trims the value and hands it to `ConnectAsync()`, which puts it into the
Notion client's options and nowhere else. Search `src/` for the property name and
the only writer you will find is the `PasswordBox` binding itself: no settings file,
no environment variable, no credential store. `Dispose()` drops the service
reference along with the commands. If your own application needs to remember a token between runs, this is
the shape to start from and the point at which you add a store deliberately, rather
than discovering later that a token leaked into a log or a settings file.

Blueprints: [Take a secret token in a PasswordBox and keep it out of storage](../BLUEPRINTS-ViewsAndControls.md#take-a-secret-token-in-a-passwordbox-and-keep-it-out-of-storage),
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### The page tree: resolving what was pasted, loading lazily, and dropping a stale preview

Read `Internal/NotionConvert.cs` first. `NormalizeId()` accepts a bare identifier, a
hyphenated one, or a full Notion URL, drops the query string before looking (a URL's
view parameters carry other identifiers that would otherwise win), and returns the
input untouched when it finds nothing, letting the API produce the error message.
Then `Internal/NotionTreeReader.cs`, which resolves that identifier by trying a page,
then a database, then a data source, catching only the "wrong kind" API error each
time and turning the final miss into one actionable sentence about sharing the page
with the integration.

From there the tree is view-model work. `NotionPageNodeViewModel` is one row: it adds
a single placeholder child when the node reports children, which is what keeps the
expand chevron visible before anything has been fetched, and expanding the row asks
the parent view model to fetch the real children exactly once. `MainViewModel.LoadChildrenForNodeAsync`
does the call and marshals the replacement back with `InvokeOnMainThread`; a failure
writes to the status line and leaves the row usable. Note that checking is fully
independent per node, by design: a checked grandchild under an unchecked parent still
becomes a chapter, and `SelectionFlattening.FlattenDepthFirst()` in the library
produces the depth-first order that both the view model and the service use, so the
order of chapters in the book is exactly the order of rows on screen.

The preview pane is the same pattern with one extra guard. Tapping a row sets
`SelectedNode` and starts a preview load; when the result comes back on the UI thread
the view model compares `SelectedNode` against the node it was loading for and
abandons the result if the user has moved on. The pane itself is placeholder-versus-content
switching done with computed `Visibility` properties (`PreviewContentVisibility`,
`PreviewPlaceholderVisibility`, `PreviewCoverVisibility`, `TreeVisibility`,
`TreePlaceholderVisibility`), each recomputed by `[AffectsProperties]` on the state
it depends on, so the XAML binds straight to them with no converters.

Blueprints: [Normalize a user entered ID or URL before calling an API](../BLUEPRINTS-DocumentsAndData.md#normalize-a-user-entered-id-or-url-before-calling-an-api),
[Resolve an ID that may be one of several object kinds](../BLUEPRINTS-DocumentsAndData.md#resolve-an-id-that-may-be-one-of-several-object-kinds),
[Load a tree lazily as the user expands it](../BLUEPRINTS-MVVM.md#load-a-tree-lazily-as-the-user-expands-it),
[Bind a TreeView to a view model tree with checkboxes](../BLUEPRINTS-ViewsAndControls.md#bind-a-treeview-to-a-view-model-tree-with-checkboxes),
[Ignore a stale async result when the selection moved on](../BLUEPRINTS-MVVM.md#ignore-a-stale-async-result-when-the-selection-moved-on),
[Show and hide panes with computed Visibility properties](../BLUEPRINTS-MVVM.md#show-and-hide-panes-with-computed-visibility-properties).

### Being a good citizen of someone else's API

Two small files carry most of the API discipline. `Internal/NotionRateGate.cs` is a
semaphore of one plus a minimum interval between calls: every request in the whole
library goes through `RunAsync()`, so requests are serialized and spaced to stay
inside Notion's published rate limit. The client's own retry policy is still enabled,
but the gate exists so that policy rarely has to fire; provoking rate limits and then
retrying is slower and ruder than not provoking them. `Internal/NotionPageReader.cs`
reads the full block tree one level per request, recursing into anything with
children except child pages and child databases, which are separate chapters or
reference lines rather than inlined content. It carries a visited set for the whole
walk, because synced blocks can point back at their source; a duplicate synced block
follows its source's children through the same guarded path. The preview reader is
deliberately cheap by contrast, capped at two API calls and one batch of blocks,
because it only has to help you recognize a page.

Blueprints: [Pace outbound API calls with a rate gate](../BLUEPRINTS-DocumentsAndData.md#pace-outbound-api-calls-with-a-rate-gate),
[Read a nested tree from an API with a cycle guard](../BLUEPRINTS-DocumentsAndData.md#read-a-nested-tree-from-an-api-with-a-cycle-guard),
[Call a REST API behind a service interface the view model resolves](../BLUEPRINTS-DocumentsAndData.md#call-a-rest-api-behind-a-service-interface-the-view-model-resolves).

### Create!: one command, four stages, one progress bar

`CreateDocumentAsync()` in `Services/NotionDocumentService.cs` is worth reading as a
whole, because it is the shape most applications need and few get right. Four stages
run behind one awaitable method: fetch each selected page's identity and block tree,
download and prepare the media, compose the document, render and save it. Composition
and rendering are both CPU-bound and go through `Task.Run()` precisely because the
caller is normally the UI thread. Progress is reported through a plain
`IProgress<CreateProgress>` carrying a stage, a message and a percentage, and the
percentages are apportioned across the stages rather than restarting at each one.

The view-model half is `DoCreate()` in `MainViewModel`. It confirms an overwrite with
`ConfirmDialog()` before it starts, sets `IsBusy` in a `try`/`finally` so the flag can
never stick, builds the request from the flattened checked rows and the chosen trim,
subscribes a `Progress<T>` that pushes `StatusText` and `ProgressValue` back through
`InvokeOnMainThread`, and reports the outcome with `ShowInfo()`. `[AffectsCommands]`
on `IsBusy` disables Connect, Load whole tree, Select and Create for the duration, so
the page needs no code-behind to keep the user out of trouble. One thing to carry
across when you copy this: every method on `INotionDocumentService` already takes a
`CancellationToken`, and the create loop checks it between pages and inside the media
loop, so adding a `CancellationTokenSource` and a Cancel command to the view model is
the natural finishing move for an application whose jobs run long.

Blueprints: [Run a multi stage pipeline behind one service method](../BLUEPRINTS-DocumentsAndData.md#run-a-multi-stage-pipeline-behind-one-service-method),
[Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread),
[Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).

### Asking for a save location without knowing what a head is

`MainViewModel` declares `IFileSaveBridge`, an interface with one settable delegate:
given a suggested file name, return the chosen path or null. The view model
implements the interface and owns all the behavior. `src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs` fills the
delegate in from `DataContextChanged`, which is also where it hands the view model a
`XamlRoot` getter through `IXamlRootGetter` so the dialogs raised from `DoCreate()`
and `DoSelectOutputFile()` have somewhere to attach. That is the whole of the
code-behind: the wiring, plus the picker call itself.

What makes this worth studying is the degradation. A head with no windowing system
has no picker at all, so the delegate is simply never set; `DoSelectOutputFile()`
sees a null delegate and tells the user, through a dialog, to type the full path into
the box instead. A head that registers a picker but cannot host one throws
`NotSupportedException`, which is caught separately and answered the same way. Either
way the output path is an ordinary bound `TextBox`, so the application is fully usable
with no dialog anywhere. The other sharp edge is the path itself: `src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs`
decodes the percent-encoded path the Linux desktop-portal pickers return, so a file
called `My Book.pdf` is not written to disk under a literal `My%20Book.pdf`, and it
does so only when the text really carries escapes, leaving Win32 and WPF paths
untouched. That file also removes the empty placeholder file the picker creates for a
brand-new name, so the application's own "replace existing file?" prompt fires only
for a real, non-empty file; a file with content in it is never deleted. Both of these
are policy about the saved document, so in your own application let the page return
the raw chosen path and put that policy in the view model or in the service that
writes the file.

Blueprints: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
[Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show),
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).

### Embedded fonts, exact coverage, and no empty boxes

`Internal/BookFonts.cs` registers four embedded families with the PDF font system:
a serif for body and headings, a sans for captions, labels and tables, a monospace for
code, and a monochrome emoji face for icons. The fonts are `EmbeddedResource` items in
the library's own csproj, so a rendered book looks identical on a machine with no
fonts installed. Two details are easy to miss and both are commented in the file: the
resolver must be registered once per *face* name, not once per family, because
face-name lookups do not travel through the family resolver; and the semibold face is
deliberately named so the resolver serves it for bold requests, since a true bold reads
too heavy at caption sizes. `EnsureRegistered()` also sets the imaging back-end, which
the PDF image pipeline needs before the first picture is placed.

`Internal/FontCoverage.cs` is the other half. Instead of guessing at Unicode ranges it
parses each embedded font's cmap table once and caches the covered codepoints, so
"can this font draw this character?" has an exact answer. `RichTextWriter` uses it to
split every run into segments the body font covers and segments only the emoji face
covers, sets the font name per segment, silently drops variation selectors and joiners
that the monochrome face could not compose anyway, and counts anything genuinely
uncoverable rather than printing it. There is a real trap recorded in
`EmojiPrintable()`: the PDF text engine addresses glyphs per UTF-16 code unit, so an
astral-plane emoji prints as an empty box even when the font contains it, and the
coverage check therefore rejects anything outside the Basic Multilingual Plane. The
dropped count comes back to the user as one line in the result dialog, so nothing
disappears without being mentioned.

Blueprints: [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system),
[Drop characters your embedded fonts cannot render](../BLUEPRINTS-DocumentsAndData.md#drop-characters-your-embedded-fonts-cannot-render),
[Write rich text runs into a paragraph or a hyperlink](../BLUEPRINTS-DocumentsAndData.md#write-rich-text-runs-into-a-paragraph-or-a-hyperlink).

### From one trim size to a composed book

`Models/PageSizeOption.cs` offers four trims and their dimensions in points.
`Internal/BookTheme.cs` turns whichever one you picked into every measurement the book
uses: margins as fractions of the page with the binding-side margin a little wider
than the outer, a measure capped at the classic book line length so a wide trim puts
its extra width into the margins rather than into unreadable lines, a body size derived
from the resulting measure, and every other size in the scale expressed as a ratio of
that. `Internal/BookStyles.cs` then defines the named paragraph styles from the theme,
so adding a fifth trim needs no new tuning anywhere.

`Internal/BookComposer.cs` assembles the sections. The first checked page becomes an
unnumbered cover section with a deep top margin that sinks the title toward the optical
center, its Notion cover image as the plate, and the page's emoji icon above the title
when the emoji face can print it. Each remaining page becomes its own section with
mirrored margins, odd and even page headers, a different first page, a running head
that carries the chapter title on the recto and the book title on the verso, and a folio
in all three footers. The numbering trap is called out in a comment and is worth
remembering: only the first content section sets its starting number, because setting
it again on a later section restarts numbering at 1 there. Every chapter opening adds a
bookmark named for its page, which is what lets cross-references elsewhere in the book
print the real page number of the page they refer to.

Blueprints: [Derive a whole document theme from one page size choice](../BLUEPRINTS-DocumentsAndData.md#derive-a-whole-document-theme-from-one-page-size-choice),
[Compose a book with sections styles running heads and folios](../BLUEPRINTS-DocumentsAndData.md#compose-a-book-with-sections-styles-running-heads-and-folios).

### Media fetched once, prepared for print, and optional all the way down

`Internal/MediaCache.cs` downloads each URL once per run into a private temp folder
named for a fresh identifier, and deletes the folder on dispose. That is not tidiness
for its own sake: Notion's uploaded-file URLs are pre-signed and short-lived, so the
download has to happen in the same run that fetched the block tree, and caching a URL
for later would be useless. Every failure path returns an unsuccessful result carrying
a reason rather than throwing, including content that exceeds the download cap, which
is enforced both from the declared length and while streaming for servers that declare
none. The one exception is a user cancellation, which is rethrown so it can cancel the
run instead of being demoted to a warning.

`Internal/MediaPreparer.cs` is the pass that runs between fetching and composing: it
walks the block trees, downloads each image, video and audio file, and fills a
dictionary keyed by block identifier so the renderer can work synchronously. Images go
through `Internal/ImagePipeline.cs`, which leaves already-good JPEG and PNG bytes alone,
caps pixel width, and re-encodes anything else, which is also how formats the PDF
embedder cannot take get converted. Videos go through `Internal/VideoPosterExtractor.cs`,
which probes duration and grabs a frame from a proportional point in the clip. Every
path there is wrapped: a missing ffmpeg, an unreadable codec or a failure of any kind
produces a null result, and the renderer draws a media card with the duration it does
know plus a warning. That is what makes ffmpeg genuinely optional rather than nominally
optional, and the same structure applies to any external tool your application would
rather have than require.

Blueprints: [Download run scoped media into a self cleaning temp cache](../BLUEPRINTS-MediaAndVision.md#download-run-scoped-media-into-a-self-cleaning-temp-cache),
[Extract a video poster frame and degrade when the external tool is missing](../BLUEPRINTS-MediaAndVision.md#extract-a-video-poster-frame-and-degrade-when-the-external-tool-is-missing),
[Normalize a downloaded image before embedding it in a document](../BLUEPRINTS-GraphicsAndRendering.md#normalize-a-downloaded-image-before-embedding-it-in-a-document).

### The renderer's rule: nothing vanishes and nothing throws mid-book

`Internal/BlockRenderer.cs` is the largest file in the folder and the one to read last,
once the theme and the rich-text writer make sense. It maps every Notion block type to
book-designed content: body typography with the classic indent after the first
paragraph, headings, lists with nested numbering, to-dos, toggles, callout sidebars,
code panels, block quotes, booktabs tables with strong top and bottom rules, column
lists, link and bookmark cards, breadcrumbs, in-page tables of contents, equations, and
cross-references that print a real page number for pages inside the book. Three
mechanisms in it generalize well.

First, unknown blocks. A type the renderer does not handle prints a visible marker and
records a warning naming the type, so a reader can see that something was there and the
document still finishes. Second, targets. Sections and table cells share no
content-adding base type, so the renderer writes through a tiny two-implementation
interface, one of which also declares that it cannot accept a table, which is what stops
a nested table from being attempted inside a cell. Third, the credit look-ahead: when
the sibling immediately after an image or video is a short paragraph that reads like a
credit line, it is consumed and typeset under the plate in the credit style instead of
becoming a stray body paragraph after the figure. All the warnings collected along the
way come back on the result and are shown to the user, capped so the dialog stays
readable.

Blueprints: [Keep unsupported content visible instead of failing the document](../BLUEPRINTS-DocumentsAndData.md#keep-unsupported-content-visible-instead-of-failing-the-document),
[Render into either a section or a table cell](../BLUEPRINTS-DocumentsAndData.md#render-into-either-a-section-or-a-table-cell),
[Place numbered framed figures with credit lines](../BLUEPRINTS-DocumentsAndData.md#place-numbered-framed-figures-with-credit-lines),
[Pair a figure with the credit paragraph that follows it](../BLUEPRINTS-DocumentsAndData.md#pair-a-figure-with-the-credit-paragraph-that-follows-it).

### Making the Fluent theme look like your application

`src/NotionDocumentCreator.UI/Views/MainPage.xaml` opens with a page-level palette and then does the thing that makes the
difference between a themed application and a themed-looking one: it re-keys the Fluent
theme's own brush resources. Button, accent-button and checkbox brushes are all redefined
in the page's palette, so a plain `Button` hovers and presses in-palette and the
selection checkboxes are the application's amber rather than stock blue. `src/NotionDocumentCreator.UI/App.xaml` does
the same for the dialog brushes, and the comment there explains why it has to be at the
application level: dialogs open in the popup layer, which follows the application's
requested theme rather than the page's visual tree. The same keys are resolved by the
framebuffer head's built-in picker and software-keyboard chrome, so opting that head in
costs nothing in appearance.

The layout uses the FlexPanel add-in for both the header bar and the bottom action bar.
`FlexPanel.Grow` on the identity block keeps the connect group pinned right while it
still shares a row, and `Grow` plus `Basis` on the save-target group make the wrap point
deterministic rather than incidental. Every icon on the page is a `FontIcon` glyph, which
matters on the framebuffer head where there may be no system fonts at all. Note that the
view model here is constructed by the XAML parser as the page's `DataContext`, which is
why `src/NotionDocumentCreator.UI/App.xaml.cs` calls `SimpleViewModel.SetIsDesignMode(false)` before
`InitializeComponent()`, why the view model constructor's first line is the
`IsDesignMode(true)` guard, and why the bridge wiring happens in `DataContextChanged`
rather than in the constructor.

Blueprints: [Re-key theme brushes so controls dialogs and picker chrome follow your palette](../BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette),
[Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in),
[Use FontIcon glyphs so icons survive on a device with no system fonts](../BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).

### Six heads that differ by a handful of lines

Every head has the same `Program.Main`: initialize logging, create the host builder,
name the application type, call the one extension method for that backend, build and
run. Two of them add something, and both additions are instructive.
`src/NotionDocumentCreator.LinuxFrameBuffer/Program.cs` opts into the save picker and the
software keyboard, which are off by default on a head with no OS chrome; the comment
there is blunt about why this application needs the keyboard badly, since the user has
to type a long token. Copy that file with one change: it restricts the picker to a fixed
folder path, and yours should compute a folder rather than name one.
`src/NotionDocumentCreator.WinWpfSkia/Program.cs` reaches for the built host, checks it is
the WPF host, and forces the software render surface before running. The remaining four
are identical apart from the backend they name. That uniformity is the point: the head
projects hold no application logic at all, so there is only ever one place to change
behavior.

Blueprints: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### Testing a document by reading the document, not the PDF

Everything below lives in `tests/libs/NotionDocumentCreator.CreateDocument.Tests/`.
The most useful file there is `TestDom.cs`, which is a helper rather than a set of
tests: it builds a themed document, runs the renderer into it, and then walks the
produced object model, collecting paragraphs (including the ones inside table
cells), tables, formatted runs and hyperlinks. With that in place,
`BlockRendererTests.cs` can assert one block type at a time against the structure
that came out, and `RichTextWriterTests.cs` can assert that bold survives, that an
astral-plane emoji is dropped, that an unrenderable character is counted. No PDF is
produced and nothing is parsed back. `CreditPairingTests.cs`,
`PageNumberingTests.cs`, `SelectionFlatteningTests.cs`, `NotionConvertTests.cs` and
`PageSizeInfoTests.cs` cover the pure logic the same way, and `MediaCacheTests.cs`
gets a genuine download failure by aiming at a closed loopback port rather than by
mocking `HttpClient`. What makes all of this possible is
`InternalsVisibleTo.cs`, since almost
all of the tested code lives in the library's `Internal` namespace. The live tests
are quarantined by `Assert.SkipWhen(...)` in their constructor on the two
environment variables, so the default run needs nothing from the outside world.

Blueprints: [Test a document renderer against the object model it produces](../BLUEPRINTS-Testing.md#test-a-document-renderer-against-the-object-model-it-produces),
[Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the three
kinds of third-party content involved. Four font families are bundled under the SIL
Open Font License 1.1 and embedded into the CreateDocument assembly, each with its OFL
text sitting beside the `.ttf` files in
`Fonts/`. FFmpeg is used at run time and
never bundled: the application invokes the ffmpeg and ffprobe executables already
installed on the host, and works without them. The Notion page content and media the
application downloads belong to their owners, and whoever runs the application is
responsible for having the right to reproduce what they select.

## License

NotionDocumentCreator is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
