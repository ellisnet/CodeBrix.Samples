# WikipediaPublisher

WikipediaPublisher turns a Wikipedia article into a book-designed, print-ready PDF. You
type search terms and press Search; the real Wikipedia search page loads in an embedded
WebView and you browse to the article you want. You choose where the PDF should go
(through a native "Save PDF as..." dialog, or by typing a full path into the box) and pick
a trim size: 8 x 10 inch coffee-table, 6 x 9 inch trade book, US Letter, or A4. Publish
fetches the article, parses it, downloads its images at print resolution, and composes a
book: a cover page with the article's hero image and short description, a table of contents
with real page-number fields and dot leaders, justified body type with first-line indents
and a raised initial, numbered and framed figures with credit lines, booktabs-style tables,
pull quotes, running heads and folios, PDF outline bookmarks, and a colophon carrying the
CC BY-SA attribution. Progress is reported live in a bar and a status line, and an existing
file at the chosen path triggers a replace confirmation before anything is written.

For a CodeBrix.Platform developer this is the reference for the document side of the
family: chaining CodeBrix.MarkupParse, CodeBrix.Imaging and CodeBrix.PdfDocCreate inside a
UI-free library that one `SimpleViewModel` drives through a single service interface, plus
the embedded-WebView bridge running on all eight heads.

## What this sample shows a CodeBrix.Platform developer

- The whole fetch-parse-images-compose-render pipeline lives in `WikipediaPublisher.RenderArticle`, a library with no UI types in it, reached only through `IArticleRenderService`: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- One service method runs five ordered stages, reports progress between them, honors a cancellation token and returns a result record the UI can display: [Run a multi stage pipeline behind one service method](../BLUEPRINTS-DocumentsAndData.md#run-a-multi-stage-pipeline-behind-one-service-method).
- The Publish command drives that long network-bound call with `IProgress<T>`, a busy flag and a `try`/`finally` that resets the bar: [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).
- Every head embeds a browser the view model steers through an `IWebViewBridge` delegate, never through a WebView type: [Show a WebView on every head and drive it from a command](../BLUEPRINTS-PlatformServices.md#show-a-webview-on-every-head-and-drive-it-from-a-command).
- The destination path comes from a native save dialog through an `IFileSaveBridge` delegate, with a typed-path fallback when a head has no dialog: [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model).
- The single "replace this file?" prompt is a `SimpleDialog` call inside the Publish command, so it behaves the same on every head: [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- The Skia and WinUI pages hand the view model a `XamlRoot` getter through `IXamlRootGetter` so those dialogs have something to attach to: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- `FileDialogHelper` decodes the percent-encoded path a Linux desktop portal hands back, so the view model only ever sees real file system paths: [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- The WPF and WinUI heads silence their dialogs' own overwrite prompts so the application's single confirmation is the only one: [Suppress a native save dialog overwrite prompt so the view model owns confirmation](../BLUEPRINTS-PlatformServices.md#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation).
- `MainViewModel` is written the family way: bound properties with `SetProperty`, lazily created `SimpleCommand` instances, and `[AffectsCommands]` on the properties that gate them: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Progress callbacks and browser-navigation callbacks arrive off the UI thread and are marshalled back with `InvokeOnMainThread`: [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- `Dispose()` disposes and nulls each command, nulls both bridge delegates so they stop holding the page alive, and releases the service reference without disposing it: [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- The whole view model constructor sits inside `if (!IsDesignMode(true))`, so the XAML designer never resolves a service or touches the network: [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- The trim-size picker is filled from a typed option list owned by the library, so no library type is named in XAML: [Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).
- The pipeline library exposes one `AddRenderArticle()` extension method, which is the only thing `App` knows about it: [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- `HostHelper` supplies the `IHostBuilderProvider` that `SimpleServiceResolver` builds its container from: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- The `App` constructor does the same four things every application in the family does: default font, service registration, design mode off, `InitializeComponent()`: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- The same `Shared/ViewModels/MainViewModel.cs` is compiled into the six Skia heads and file-linked into the native WinUI 3 and WPF heads: [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads).
- Each Skia head is a tiny `Program.cs` whose only distinguishing line is the backend call on the host builder: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- The Win32Skia head's `Main` is deliberately synchronous and `[STAThread]`, with a comment saying what breaks in the WebView if it is not: [Keep Main synchronous and STA so an embedded WebView can start](../BLUEPRINTS-AppStructureAndStartup.md#keep-main-synchronous-and-sta-so-an-embedded-webview-can-start).
- The WinWpfSkia head casts its host after `Build()` to force the software render surface, and says why in a comment: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- The LinuxFrameBuffer head opts into orientation handling, a file save picker and a software keyboard through the host builder: [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- The bundled Open Sans face is set as the application's default text font and also exposed as a `FontFamily` resource, referenced by `.ttf` URI rather than by a merged dictionary: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- `App.InitializeLogging()` wires a console logger factory into the platform's ambient logger inside `#if DEBUG`, and is called before the host is built: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- The Skia page declares the platform's own control and data namespaces and binds with `{d:Binding ...}`, reaching its view model through an `assembly=WikipediaPublisher.Core` namespace: [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- The bottom bar is a wrapping `FlexPanel` whose two control groups split onto two rows in portrait: [Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in).
- Enter in the search box runs the Search command; the WPF head does it declaratively with a `KeyBinding`, which is the form to copy: [Run a command when the user presses Enter in a text box](../BLUEPRINTS-ViewsAndControls.md#run-a-command-when-the-user-presses-enter-in-a-text-box).
- `ArticleParser` walks a CodeBrix.MarkupParse DOM into an ordered list of typed blocks, picking the real content container out of several candidates: [Parse messy HTML into structured blocks with the CodeBrix MarkupParse library](../BLUEPRINTS-DocumentsAndData.md#parse-messy-html-into-structured-blocks-with-the-codebrix-markupparse-library).
- The same walk removes citation markers, edit links, navigation boxes and whole trailing sections, counting what it dropped: [Strip web only chrome while walking the DOM](../BLUEPRINTS-DocumentsAndData.md#strip-web-only-chrome-while-walking-the-dom).
- `GlyphFilter` removes characters the embedded book fonts cannot render, then tidies the holes it leaves behind, rather than printing empty boxes: [Drop characters your embedded fonts cannot render](../BLUEPRINTS-DocumentsAndData.md#drop-characters-your-embedded-fonts-cannot-render).
- A pure, unit-tested helper rewrites screen-sized thumbnail URLs into print-resolution renditions without upscaling a raster source past its true width: [Upgrade thumbnail URLs to print resolution](../BLUEPRINTS-DocumentsAndData.md#upgrade-thumbnail-urls-to-print-resolution).
- `ImagePipeline` loads, resizes and re-encodes downloaded images with CodeBrix.Imaging, and passes already-suitable bytes straight through: [Normalize a downloaded image before embedding it in a document](../BLUEPRINTS-GraphicsAndRendering.md#normalize-a-downloaded-image-before-embedding-it-in-a-document).
- `WikipediaClient` owns one `HttpClient` with an identifying user agent, a timeout and a semaphore-guarded minimum gap between media downloads: [Be a polite HTTP client to a public API](../BLUEPRINTS-DocumentsAndData.md#be-a-polite-http-client-to-a-public-api).
- Image credits come from a batched MediaWiki metadata query that is best effort: a failed batch leaves the credit blank and never fails the render: [Batch a metadata API and treat the result as best effort](../BLUEPRINTS-DocumentsAndData.md#batch-a-metadata-api-and-treat-the-result-as-best-effort).
- `BookFonts.EnsureRegistered()` registers the embedded EB Garamond and Source Sans 3 faces with the PDF font system once, under a lock: [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).
- `BookTheme` turns one trim-size choice into every margin, type size and rule weight in the book, with everything else computed from the body size: [Derive a whole document theme from one page size choice](../BLUEPRINTS-DocumentsAndData.md#derive-a-whole-document-theme-from-one-page-size-choice).
- `BookComposer` builds a real book: a cover section, a body section with mirrored margins, named styles, running heads and folios: [Compose a book with sections styles running heads and folios](../BLUEPRINTS-DocumentsAndData.md#compose-a-book-with-sections-styles-running-heads-and-folios).
- Contents entries are a bookmark hyperlink, a tab and a page-reference field, with the dots coming from a right-aligned tab stop on the entry style: [Build a table of contents with real page numbers and dot leaders](../BLUEPRINTS-DocumentsAndData.md#build-a-table-of-contents-with-real-page-numbers-and-dot-leaders).
- Figures are sized from their aspect ratio, clamped against the text block, keylined only where a frame flatters them, and captioned with a running figure number: [Place numbered framed figures with credit lines](../BLUEPRINTS-DocumentsAndData.md#place-numbered-framed-figures-with-credit-lines).
- Article tables become booktabs-style tables with horizontal rules only, and the parser refuses any table it cannot lay out: [Render booktabs style tables from parsed rows](../BLUEPRINTS-DocumentsAndData.md#render-booktabs-style-tables-from-parsed-rows).
- The first body paragraph gets its opening letter split into its own large accent-colored run: [Open a document with a raised initial](../BLUEPRINTS-DocumentsAndData.md#open-a-document-with-a-raised-initial).
- `InternalsVisibleTo.cs` in the pipeline library is what lets the tests drive the parser, theme and composer directly: [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- The offline tests parse an article fixture embedded in the test assembly, read out through a shared helper: [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary).
- Generated PDFs are verified by their `%PDF-` signature and a page count rather than by a golden file: [Assert on a generated document without a golden file](../BLUEPRINTS-Testing.md#assert-on-a-generated-document-without-a-golden-file).
- Live network tests sit in the same class as the offline ones, with the fast fetch-and-parse test split from the slow end-to-end render: [Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).
- The family's DI-backed test fixture file is linked into the test project, ready to resolve `IArticleRenderService` the way the container builds it: [Test a service the way the container builds it](../BLUEPRINTS-Testing.md#test-a-service-the-way-the-container-builds-it).
- The WinWpfSkia head sets `EnableWindowsTargeting` so a Windows-targeting head still compiles on Linux and macOS inside the cross-platform solution: [Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution).
- `WikipediaPublisher.Windows.slnx` restricts its solution platforms to x86, x64 and ARM64 to match what the WinUI head declares: [Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares).
- Two solutions exist because the native heads need Windows-host-only build tooling, and both files carry a comment saying so: [Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere).

## Building, running and testing

### Solutions

| Solution | Open on | Contains |
| --- | --- | --- |
| `WikipediaPublisher.slnx` | Linux, macOS, Windows | The shared UI shared project, the Core library, the six CodeBrix.Platform (Skia) heads, the RenderArticle library and the test project |
| `WikipediaPublisher.Windows.slnx` | Windows | Everything above, plus the native WinUI 3 and WPF heads; it also restricts the solution platforms to x86, x64 and ARM64, and puts RenderArticle under a `/Libraries/` solution folder |

Both files carry a comment explaining the split: the native heads require Windows-host-only
build tooling (the Windows App SDK XAML compiler and the WPF targets), so they are kept out
of the cross-platform solution rather than given a compile-only workaround.

### The heads

Six CodeBrix.Platform (Skia) heads, all under `CodeBrixPlatform/`, all sharing one XAML UI:

| Project | Platform |
| --- | --- |
| `CodeBrixPlatform/WikipediaPublisher.Win32Skia` | Windows, native Win32 window |
| `CodeBrixPlatform/WikipediaPublisher.WinWpfSkia` | Windows, Skia rendering hosted in a WPF window (targets `net10.0-windows`) |
| `CodeBrixPlatform/WikipediaPublisher.LinuxX11` | Linux desktop, X11 or XWayland |
| `CodeBrixPlatform/WikipediaPublisher.LinuxWayland` | Linux desktop, native Wayland |
| `CodeBrixPlatform/WikipediaPublisher.LinuxFrameBuffer` | Linux, direct framebuffer, no windowing system |
| `CodeBrixPlatform/WikipediaPublisher.MacOS` | macOS |

Two native (non-Skia) heads at the application root:

| Project | Platform |
| --- | --- |
| `WikipediaPublisher.WinUI` | WinUI 3, its own XAML page and a Win32 common item save dialog reached through COM interop |
| `WikipediaPublisher.Wpf` | WPF, its own window and the WPF `SaveFileDialog` |

Eight heads in total. All eight embed a WebView and all eight bind the same `MainViewModel`.

### Prerequisites

- The .NET 10 SDK. Every project targets net10.0; the WinWpfSkia head targets
  `net10.0-windows` and sets `EnableWindowsTargeting`, so it compiles on Linux and macOS
  but only runs on Windows.
- Network access at run time. Nothing is bundled: the application fetches article HTML,
  downloads the article's images, and queries the Wikimedia image-metadata API.
- On the Linux Skia heads, the system WPE WebKit engine, which the WebView add-in needs at
  run time. The Core csproj comment gives the install line:
  `sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1`. It is a
  run-time dependency only, so the build succeeds on a machine that cannot run the WebView.
- The LinuxWayland head needs a running Wayland compositor. Its csproj comment states that
  it never falls back to X11 or XWayland, and points at the LinuxX11 head for those.
- The LinuxFrameBuffer head's `Program.cs` configures its save picker with
  `RestrictToFolder = "/home/jeremy"`. Change that path before running that head, or the
  picker is rooted at a folder that does not exist on your machine.
- A Windows host with the Windows App SDK and WPF build tooling for the two native heads.
- No accounts, tokens, or user-supplied data files are needed.

### Running a head

```text
dotnet run --project CodeBrixPlatform/WikipediaPublisher.LinuxX11/WikipediaPublisher.LinuxX11.csproj
```

Substitute any of the other head projects. The two native heads build and run only from
`WikipediaPublisher.Windows.slnx` on a Windows machine.

### Tests

There is one test project, `Tests/WikipediaPublisher.RenderArticle.Tests`, covering the
pipeline library with xUnit v3 and SilverAssertions. `global.json` at the application root
selects the Microsoft.Testing.Platform runner:

```text
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

With that runner selected, a plain `dotnet test` can report that it discovered no tests.
Build the test project and run the executable the build produces instead:

```text
dotnet build Tests/WikipediaPublisher.RenderArticle.Tests/WikipediaPublisher.RenderArticle.Tests.csproj
Tests/WikipediaPublisher.RenderArticle.Tests/bin/Debug/net10.0/WikipediaPublisher.RenderArticle.Tests
```

The offline tests need nothing but the SDK. The live tests in
`Services/ArticleRenderServiceTests.cs` reach the real Wikipedia and Wikimedia endpoints;
the end-to-end ones download every image in an article and their comments warn that they
are slow. Every awaited call in a test passes `TestContext.Current.CancellationToken`,
which is both what the analyzer requires and what makes a hung network test cancellable.

## How the projects and folders are organized

```text
WikipediaPublisher/
  WikipediaPublisher.slnx                     Cross-platform solution (all operating systems)
  WikipediaPublisher.Windows.slnx             Windows solution; adds the WinUI 3 and WPF heads
  global.json                                 Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt                     Bundled-font and run-time-content attribution

  CodeBrixPlatform/                           The six Skia heads and what they share
    WikipediaPublisher.UI/                    Shared XAML UI as a .shproj (App.xaml, Views/MainPage.xaml)
    WikipediaPublisher.Core/                  Shared application library; carries every platform package
    WikipediaPublisher.Win32Skia/             Head: Windows Win32 backend
    WikipediaPublisher.WinWpfSkia/            Head: Skia-on-WPF backend for Windows
    WikipediaPublisher.LinuxX11/              Head: Linux X11 backend
    WikipediaPublisher.LinuxWayland/          Head: native Wayland backend
    WikipediaPublisher.LinuxFrameBuffer/      Head: Linux framebuffer backend (picker + software keyboard)
    WikipediaPublisher.MacOS/                 Head: macOS backend

  WikipediaPublisher.WinUI/                   Native WinUI 3 head (own XAML page, Win32 save-dialog interop)
  WikipediaPublisher.Wpf/                     Native WPF head (own window, WPF SaveFileDialog)

  Shared/                                     Source files linked into the projects that need them
    ViewModels/MainViewModel.cs               The one view model all eight heads bind to
    Helpers/HostHelper.cs                     IHostBuilderProvider for SimpleServiceResolver
    Helpers/FileDialogHelper.cs               Picker-path decoding and placeholder-file cleanup
    Helpers/EmbeddedResourceHelper.cs         Embedded-resource reader (linked into the tests)
    Testing/SimpleTestFixture.cs              DI-backed test fixture (linked into the tests)

  WikipediaPublisher.RenderArticle/           The UI-free article-to-book-PDF pipeline library
    Services/                                 IArticleRenderService and ArticleRenderService
    Models/                                   RenderRequest, RenderProgress, RenderedArticle,
                                              PageSizeOption/PageSizeInfo, ParsedArticle and its blocks
    Internal/                                 WikipediaClient, ArticleParser, ImagePipeline,
                                              AttributionResolver, AttributionFormatter,
                                              BookComposer, BookTheme, BookFonts, GlyphFilter
    Fonts/                                    Embedded EB Garamond and Source Sans 3 faces plus their OFL texts
    RegisterServices.cs                       The AddRenderArticle() DI extension method
    InternalsVisibleTo.cs                     Opens the library's internals to the test project

  Tests/
    WikipediaPublisher.RenderArticle.Tests/   xUnit v3 and SilverAssertions; embedded HTML fixture
  SampleOutput/                               Example generated PDFs
```

The dependency direction is one way and shallow. `WikipediaPublisher.RenderArticle` depends
on nothing in the application: it knows CodeBrix.MarkupParse, CodeBrix.PdfDocCreate and
CodeBrix.Imaging, and exposes `IArticleRenderService`. `WikipediaPublisher.Core`
project-references RenderArticle and carries every CodeBrix.Platform package; each of the
six Skia heads project-references Core and adds exactly one runtime package of its own.

What is file-linked rather than referenced matters here. The shared XAML lives in
`WikipediaPublisher.UI.projitems`, which each Skia head imports with `Label="Shared"`, so
the UI is compiled into every head rather than referenced from a library. Core file-links
the view model and the two helpers out of `Shared/` with
`<Compile Include="..\..\Shared\..." Link="..." />`, and sets its `RootNamespace` to
`WikipediaPublisher` (not `WikipediaPublisher.Core`) so the linked view model's namespace
still matches. The two native heads project-reference RenderArticle directly and file-link
what they need from `Shared/`: the WinUI head links the view model, the host helper and the
file-dialog helper; the WPF head links only the first two, because the WPF `SaveFileDialog`
returns a plain path and leaves no placeholder file behind. The test project file-links
`SimpleTestFixture.cs` and `EmbeddedResourceHelper.cs` the same way.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML UI framework and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, the dialog helpers, `IXamlRootGetter`), `FeatureConfiguration.Font`, the logging adapter, and the `FileSavePicker` the Skia heads open | `CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`, `CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`, `Shared/ViewModels/MainViewModel.cs` |
| The CodeBrix.Platform runtime for each backend | One runtime package per head (Win32, Skia-on-WPF, X11, Wayland, framebuffer, macOS); the head's `Program.cs` selects it through the host builder | the six head csprojs under `CodeBrixPlatform/` and their `Program.cs` files |
| CodeBrix.Platform.WebView add-in | Gives the Linux Skia heads an embedded WebView backed by WPE WebKit, offscreen. Referenced once in Core so every Skia head inherits it, and inert on the Windows, Skia-on-WPF and macOS runtimes, which already have one | `CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`, `CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.FlexPanel add-in | The flexbox-style layout panel that lays out the bottom bar so the save-target group and the page-size/Publish group wrap onto two rows in portrait | `CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`, `CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.Fonts.OpenSans | The bundled application font, set as the default text font and exposed as a `FontFamily` resource | `CodeBrixPlatform/WikipediaPublisher.UI/App.xaml`, `CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs` |
| CodeBrix.Platform.WinUI | Brings the Simple toolkit to the native WinUI 3 head so it can bind the shared view model | `WikipediaPublisher.WinUI/WikipediaPublisher.WinUI.csproj` |
| CodeBrix.Platform.WPF | Brings the Simple toolkit to the native WPF head so it can bind the shared view model | `WikipediaPublisher.Wpf/WikipediaPublisher.Wpf.csproj` |
| CodeBrix.MarkupParse | Parses article HTML into a DOM (`HtmlParser`, `IDocument`, `IElement`, `QuerySelector`/`QuerySelectorAll`, `Closest`) that the parser walks into structured blocks | `WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs` |
| CodeBrix.Imaging | Loads, resizes and re-encodes downloaded images for print (`Image.Load`, format detection, `Mutate`/`Resize`, the PNG and JPEG encoders), and supplies the PDF layer's imaging back-end through `ImagingImageSource<Rgba32>` | `WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs`, `WikipediaPublisher.RenderArticle/Internal/BookFonts.cs` |
| CodeBrix.PdfDocCreate | The document object model the book is composed with: `Document`, `Section`, styles, `PageSetup`, headers and footers, hyperlinks and bookmarks, `AddPageRefField()`, tables and images | `WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`, `WikipediaPublisher.RenderArticle/Internal/BookTheme.cs`, `WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs` |
| CodeBrix.PdfDocuments | Arrives with CodeBrix.PdfDocCreate rather than being referenced directly; supplies `PdfDocumentRenderer`, `ImageSource`, and the font system (`EmbeddedFontResolver`, `EmbeddedResourceFontFace`, `MetaFontResolver`) | `WikipediaPublisher.RenderArticle/Internal/BookFonts.cs`, `WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs` |
| SilverAssertions | The assertion style throughout the test project | `Tests/WikipediaPublisher.RenderArticle.Tests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | Supplies the `IHostBuilder` that `HostHelper` hands to `SimpleServiceResolver`, and the `IServiceCollection` that `AddRenderArticle()` extends | `Shared/Helpers/HostHelper.cs`, `WikipediaPublisher.RenderArticle/RegisterServices.cs` |
| Microsoft.Extensions.Logging (console and abstractions) | The Debug-only console logger factory wired into the platform's ambient logger, and the `ILogger<ArticleRenderService>` the library takes with a null-logger default | `CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`, `WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs` |
| Microsoft Edge WebView2 (WPF control) | The embedded browser on the native WPF head | `WikipediaPublisher.Wpf/Views/MainWindow.xaml` |
| Windows App SDK and the Windows SDK build tools | The native WinUI 3 head's framework, including its built-in WebView | `WikipediaPublisher.WinUI/WikipediaPublisher.WinUI.csproj` |
| xUnit v3 and the .NET test SDK | The test framework | `Tests/WikipediaPublisher.RenderArticle.Tests/WikipediaPublisher.RenderArticle.Tests.csproj` |

## Worth studying in this application

### The pipeline library, its service interface and its five stages

Everything that makes a PDF lives in `WikipediaPublisher.RenderArticle`, which references no
UI package at all. Its public surface is `IArticleRenderService` with two methods:
`SearchArticlesAsync(...)` and `RenderArticleAsync(...)`. The application registers it with
one extension method, `AddRenderArticle()`, called inside the
`SimpleServiceResolver.CreateInstance()` callback in the `App` constructor; the view model
resolves `IArticleRenderService` with `GetService<T>()` and never learns the implementation
type. Read `WikipediaPublisher.RenderArticle/Services/IArticleRenderService.cs` first, then
`WikipediaPublisher.RenderArticle/RegisterServices.cs`, then
`CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`, then the constructor of
`Shared/ViewModels/MainViewModel.cs`.

The service is a container singleton and owns an `HttpClient`, so it is `IDisposable` and
guards every public method with `ObjectDisposedException.ThrowIf`. The view model releases
its reference in `Dispose()` but does not dispose the service, because it did not create
it. The tests, which do construct the service themselves, do dispose it.

`WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs` is the file to read to
understand the application. It validates the request, then runs five numbered stages with a
progress report before each: fetch the article HTML, parse it into blocks, download and
normalize the images and resolve their attribution, compose the book, and render and save
the PDF. A cancellation token is checked between stages, and each stage is a separate
internal class so it can be tested on its own.

Two details are easy to miss. The request record accepts either a full output path or a
directory plus an optional file name, with the full path winning, and the service creates
the target folder either way. And the renderer is constructed with `unicode: true`, which is
what the embedded fonts require.

The service also treats an empty block list as a hard error with a message the user can act
on, rather than producing an empty book. Every non-fatal problem the pipeline meets goes the
other way: a failed image download, a table it cannot lay out, a character it cannot render,
a metadata batch that did not return, all become warnings collected on the article and
surfaced in the result.

See [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface),
[Run a multi stage pipeline behind one service method](../BLUEPRINTS-DocumentsAndData.md#run-a-multi-stage-pipeline-behind-one-service-method)
and [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).

### One view model, eight heads, two bridge interfaces

`Shared/ViewModels/MainViewModel.cs` is the only view model in the application, and it is
the same file in all eight heads: compiled into `WikipediaPublisher.Core` for the six Skia
heads, file-linked into the two native head projects. It derives from `SimpleViewModel` and
implements two interfaces it declares itself, `IWebViewBridge` and `IFileSaveBridge`. Each
is a small property bag of delegates the page assigns, which is the whole of its knowledge
of the platform: it holds an `Action<string> NavigateToUrl` and a
`Func<string, Task<string>> PickSavePdfPathAsync`, and it checks both for null before using
them. Nothing in the file names a WebView type, a picker type, or a window type.

Read the two interface declarations at the top of the file, then the `#region` for the
commands, then the three page implementations in the order
`CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`,
`WikipediaPublisher.WinUI/Views/MainPage.xaml.cs`,
`WikipediaPublisher.Wpf/Views/MainWindow.xaml.cs`. The three pages are worth comparing:
they are different platforms doing the same two jobs, and the differences between them are
exactly the platform-specific parts the interfaces exist to absorb.

One sharp edge is the compilation symbol. `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI` are
defined by Core and by every Skia head csproj, but not by the two native head projects, and
the view model uses `#if HAS_CODEBRIX` to apply the `[Bindable]` attribute only where the
platform supplies it. Link view-model source into a native head and you have to check which
symbols that project defines.

See [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads)
and [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### The embedded WebView on every head

Search does not call an API. It builds a Wikipedia search URL and navigates the embedded
browser to it; the user then browses freely, and Publish uses whatever article page is
displayed. That makes the browser the application's primary input, and it has to work on
all eight heads.

The view model side is two members of `IWebViewBridge`: the page sets `NavigateToUrl`, and
the page calls `SetCurrentBrowserUrl(url)` whenever navigation completes. `DoSearch()`
composes the search URL and marshals the navigation with `InvokeOnMainThread`;
`SetCurrentBrowserUrl` writes `ArticleUrl` and a status line, and `ArticleUrl` carries
`[AffectsCommands(nameof(PublishCommand))]` so Publish enables itself the moment the user
lands on a printable article page. Whether a page is printable is decided by
`IsPublishableArticleUrl(url)`, a public static pure function that rejects the main page
and the non-article namespace prefixes, so `CanExecute` stays cheap and is testable with no
view model instance.

Two sharp edges show up in all three page implementations. Read the current URL from
`CoreWebView2.Source`, not from the XAML `Source` property, which does not reliably reflect
redirects or user navigation; all three carry the same comment saying so. And the Skia page
wires the browser from a `Loaded` handler behind a `_browserInitialized` guard, because
`Loaded` can fire more than once.

On the Linux Skia heads the browser itself comes from the CodeBrix.Platform.WebView add-in,
referenced once in `CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`
rather than in each Linux head, because it is inert where a WebView already exists. The
comment on that reference is the one to read: it names the system WPE WebKit packages that
must be installed at run time, which is a run-time dependency the build cannot catch.

The Win32Skia head's `Program.cs` carries the matching Windows-side pitfall in a comment:
`Main` must be a synchronous `[STAThread]` method, because `[STAThread]` is silently ignored
on an `async Task Main` and the WebView then fails much later with an RPC error about
changing the thread mode.

See [Show a WebView on every head and drive it from a command](../BLUEPRINTS-PlatformServices.md#show-a-webview-on-every-head-and-drive-it-from-a-command)
and [Keep Main synchronous and STA so an embedded WebView can start](../BLUEPRINTS-AppStructureAndStartup.md#keep-main-synchronous-and-sta-so-an-embedded-webview-can-start).

### Choosing where the PDF goes, on heads that disagree about dialogs

The save path is the other place the eight heads differ, and it is worth reading as a
worked example of graceful degradation. `IFileSaveBridge` is one delegate: in a suggested
file name, out the chosen path or null. `DoSelectOutputFile()` handles four outcomes: the
delegate is null (a head that never wired a dialog), the delegate throws
`NotSupportedException` (a head that wired one but whose platform refuses), the user
cancelled (null path), or a path came back. The first two both end in the same informational
dialog telling the user to type the full path into the box, which is why the path box is
editable on every head.

The suggested file name is computed in the view model from the current article URL, with
`Path.GetInvalidFileNameChars()` applied, so no head has to invent one.

Each head then supplies the dialog its platform offers, and each has a wrinkle:

- The Skia heads open the platform's `FileSavePicker`. That picker percent-encodes the path
  it returns on Linux, so `FileDialogHelper.ToFileSystemPath(...)` decodes it, guarded by a
  check for `%` followed by two hex digits so a legitimate name containing a percent sign
  is left alone. The picker also creates an empty placeholder file at the chosen path, so
  `FileDialogHelper.RemoveEmptyPlaceholder(...)` deletes it, but only when it is genuinely
  zero-length, and swallows any failure. The worst case is one extra confirmation prompt,
  never lost data.
- The WinUI head drops to the Win32 common item dialog through COM interop in
  `WikipediaPublisher.WinUI/Views/Win32SaveFileDialog.cs`, and its class comment gives both
  reasons: that dialog can be told not to prompt about overwriting, and it creates no
  placeholder file.
- The WPF head uses `Microsoft.Win32.SaveFileDialog` with `OverwritePrompt = false`, and
  does not link the path helper at all, because it already returns a plain path.

The single confirmation lives in the view model: `DoPublish()` checks `File.Exists` and
calls `ConfirmDialog(...)` before anything is written. Doing it at publish time rather than
at pick time means it also covers a path the user typed by hand. For those dialogs to have
somewhere to attach, the Skia and WinUI pages hand the view model a `XamlRoot` getter
through `IXamlRootGetter` in a `DataContextChanged` handler subscribed before
`InitializeComponent()`, because `InitializeComponent()` is what sets the data context, and
they hand it a getter rather than a value because `XamlRoot` is not available yet at that
point. The WPF head does not need any of this.

Read `Shared/ViewModels/MainViewModel.cs` (the `SelectOutputFileCommand` and
`PublishCommand` regions), then `Shared/Helpers/FileDialogHelper.cs`, then the three page
implementations.

See [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model),
[Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns),
[Suppress a native save dialog overwrite prompt so the view model owns confirmation](../BLUEPRINTS-PlatformServices.md#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation),
[Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs)
and [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).

### Publishing as a long-running command

Publishing an illustrated article is a minutes-long, network-bound job, and the view model
treats it as one. `DoPublish()` sets `IsBusy = true`, builds a `RenderRequest` from the
bound properties, creates a `Progress<RenderProgress>` whose callback marshals onto the UI
thread with `InvokeOnMainThread`, awaits `RenderArticleAsync(...)`, reports the outcome
through a dialog, and resets `ProgressValue` and `IsBusy` in a `finally` so a failure never
leaves the bar part-filled.

`IsBusy` carries `[AffectsCommands(nameof(SearchCommand), nameof(PublishCommand), nameof(SelectOutputFileCommand))]`,
so all three buttons disable themselves for the duration with no code-behind at all;
`SearchTerms`, `ArticleUrl` and `OutputFilePath` carry their own `[AffectsCommands]`
attributes for the input-validity half of the gating.

On the library side, `RenderProgress` is a record carrying a `RenderStage` enum as well as a
message and a percentage, so a UI could render a stage list rather than just a bar. The
`IProgress<T>` parameter is optional, which is what lets the offline tests call the same
method with no progress sink at all.

Read `Shared/ViewModels/MainViewModel.cs` and then
`WikipediaPublisher.RenderArticle/Models/RenderModels.cs`.

See [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### Parsing real-world article HTML

`WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs` is the longest file in the
library and the one with the most hard-won detail in it. It parses the page with
CodeBrix.MarkupParse and walks the DOM into an ordered `List<ArticleBlock>` of headings,
paragraphs, lists, block quotes, images, tables and definition lists, each carrying
`TextRun` records for bold, italic, superscript and subscript.

Three things in it are worth copying into your own parser:

- It does not take the first container that matches. A real article can carry several
  `.mw-parser-output` elements, one of which is a near-empty template wrapper, so the parser
  ranks candidates by paragraph count and records a warning when it saw more than one. There
  is a live regression test for exactly this.
- It strips web-only chrome as it goes, with a class deny-list for block elements, a
  `RemoveNonContent(...)` helper for citation markers and edit links, and a stop-section
  list that ends the walk at "References", "See also" and their siblings. The removal helper
  calls `.ToList()` before removing, because you cannot mutate the DOM while enumerating a
  live query result.
- It handles more than one markup generation: both a bare heading element and the newer
  wrapper-div form, recursing through section wrappers.

`GlyphFilter.cs` sits inside the parser's whitespace-collapsing step and removes characters
the embedded book fonts do not cover, which for this application means things like inline
cuneiform or CJK quoted in an article. Removing characters leaves empty bracket pairs and
doubled spaces behind, so the cleanup passes after the filter matter as much as the filter
itself; surrogate pairs have to be skipped in both halves or an orphaned low surrogate is
left in the text; and the count of removed characters becomes a warning, so the reader is
told rather than silently shortchanged.

Read `Internal/ArticleParser.cs` alongside `Models/ArticleContent.cs`, then
`Internal/GlyphFilter.cs`.

See [Parse messy HTML into structured blocks with the CodeBrix MarkupParse library](../BLUEPRINTS-DocumentsAndData.md#parse-messy-html-into-structured-blocks-with-the-codebrix-markupparse-library),
[Strip web only chrome while walking the DOM](../BLUEPRINTS-DocumentsAndData.md#strip-web-only-chrome-while-walking-the-dom)
and [Drop characters your embedded fonts cannot render](../BLUEPRINTS-DocumentsAndData.md#drop-characters-your-embedded-fonts-cannot-render).

### From screen images to print images, and the credits beneath them

Article images arrive as screen-sized thumbnails, and a printed page needs more. The parser
derives a print-resolution rendition URL from a thumbnail URL by rewriting the pixel prefix
of its final path segment, with one rule that carries the whole idea: a vector source can be
rasterized at any size, but a raster source must be clamped to its true file width read from
the markup, because upscaling is worse than a small picture. Icons are filtered out earlier
by a minimum pixel width, and unsupported file extensions are rejected before a download is
attempted. The rewrite is a pure internal static method, unit-tested with a theory and no
I/O at all.

`Internal/ImagePipeline.cs` then downloads each image and prepares it with CodeBrix.Imaging.
It re-encodes only when it must: an already-suitable JPEG or PNG that needs no resize is
embedded as its original bytes, because re-encoding only loses quality and adds size.
Resizing passes a zero height to preserve the aspect ratio, transparency-capable formats are
saved as PNG and everything else as JPEG, and a high-resolution rendition that 404s falls
back to the page thumbnail. A failed or undecodable image is a warning; the composer simply
skips any image with no processed bytes.

`Internal/WikipediaClient.cs` is the polite-client half. It owns one `HttpClient` with an
identifying user agent carrying contact details, a timeout, and a semaphore-guarded minimum
gap between media downloads so concurrent callers cannot bypass the throttle. The detail to
copy is the exception filter: `catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)`
is what separates a request timeout from a real user cancellation, and without it cancelling
the job would look like a string of failed downloads.

Every figure in the book can also carry a credit line naming the photographer or
illustrator and the license, and that part is deliberately allowed to fail. The information
comes from a separate metadata API. `Internal/AttributionResolver.cs` asks only about
images that actually downloaded and that it can identify, de-duplicates the titles,
and hands them to `WikipediaClient.GetImageMetadataAsync(...)`, which batches them into
requests and returns an empty dictionary rather than throwing when a batch fails. Titles
that cannot be resolved are simply absent, and the result dictionary is keyed
case-insensitively because the API normalizes titles.

`Internal/AttributionFormatter.cs` is the other half, and it is a catalog of what real
metadata looks like: HTML markup inside field values, entity encoding, placeholder names
like "unknown author", the same phrase duplicated by machine-plus-human templates, and
license codes that mean nothing to a reader. Each of those is a named private method with a
comment saying which case it exists for. It is the one part of the pipeline with a
dedicated unit-test class of its own.

See [Upgrade thumbnail URLs to print resolution](../BLUEPRINTS-DocumentsAndData.md#upgrade-thumbnail-urls-to-print-resolution),
[Normalize a downloaded image before embedding it in a document](../BLUEPRINTS-GraphicsAndRendering.md#normalize-a-downloaded-image-before-embedding-it-in-a-document),
[Be a polite HTTP client to a public API](../BLUEPRINTS-DocumentsAndData.md#be-a-polite-http-client-to-a-public-api)
and [Batch a metadata API and treat the result as best effort](../BLUEPRINTS-DocumentsAndData.md#batch-a-metadata-api-and-treat-the-result-as-best-effort).

### Designing the book: one theme, many styles

`Internal/BookTheme.cs` turns the user's single trim-size choice into the entire design.
Margins and body size are fixed per trim size; everything else, including leading, heading
sizes, caption size and the raised-cap size, is computed as a ratio of the body size, and
the palette (a warm ink, an oxblood accent, a muted gray and a hairline) is shared across
all four. Every dimension is in typographic points, matching the document model's `Unit`.

`Internal/BookComposer.cs` then builds the document. It defines its styles first, because
styles inherit by name, then composes front matter, content and colophon. The structural
parts to read are the section setup with mirrored margins, a different first-page header and
footer, and running-head and folio paragraphs; the heading styles that set `OutlineLevel`,
which is what produces the PDF outline bookmarks, with the contents title deliberately reset
to body text so "Contents" does not appear as a bookmark; and the contents page itself,
where each entry is a bookmark hyperlink, a tab, and a page-reference field, and the dot
leaders come from a right-aligned tab stop on the entry style.

Several rules only become visible when you read the comments:

- `DifferentFirstPageHeaderFooter` means the first page needs its own folio paragraph, or
  page one silently loses its number.
- Bookmark names are derived from a heading block's index in the article, so the contents
  page and the body agree without a shared counter.
- No contents page is emitted at all unless the article has at least two top-level sections,
  and sub-headings are included only while the total heading count stays small enough for
  the list to fit.
- Figures are sized from their aspect ratio, clamped against the text block height, given a
  hairline keyline only when the image is a photograph, and numbered with a running counter.
  `ImageSource.FromBinary` takes a name that acts as a cache key, so the composer appends a
  GUID to stop two renders in one process colliding on a stale entry. Set `LockAspectRatio`
  and give only the width; setting both dimensions distorts the picture.
- Tables get strong top and bottom rules and a light rule under the header, with no vertical
  rules at all, and `HeadingFormat` on the first row is what repeats the header across a page
  break. A zero-height spacer paragraph supplies the space below a table, because table
  objects carry no `SpaceAfter`.
- The raised initial is a one-shot flag: the first body paragraph whose text starts with a
  letter has its first character split into its own formatted run, and because `TextRun` is a
  record the remainder keeps its formatting with a `with` expression.
- The theme's `Letterspace()` helper uses non-breaking spaces, because the layout engine
  collapses runs of ordinary blanks.

See [Derive a whole document theme from one page size choice](../BLUEPRINTS-DocumentsAndData.md#derive-a-whole-document-theme-from-one-page-size-choice),
[Compose a book with sections styles running heads and folios](../BLUEPRINTS-DocumentsAndData.md#compose-a-book-with-sections-styles-running-heads-and-folios),
[Build a table of contents with real page numbers and dot leaders](../BLUEPRINTS-DocumentsAndData.md#build-a-table-of-contents-with-real-page-numbers-and-dot-leaders),
[Place numbered framed figures with credit lines](../BLUEPRINTS-DocumentsAndData.md#place-numbered-framed-figures-with-credit-lines),
[Render booktabs style tables from parsed rows](../BLUEPRINTS-DocumentsAndData.md#render-booktabs-style-tables-from-parsed-rows)
and [Open a document with a raised initial](../BLUEPRINTS-DocumentsAndData.md#open-a-document-with-a-raised-initial).

### Fonts that make the output identical everywhere

The book uses EB Garamond for text and Source Sans 3 for captions and labels, and both are
embedded resources in the pipeline library rather than files on disk or system fonts, so a
PDF generated on a developer machine, a build agent and a user's machine looks the same.
`Internal/BookFonts.cs` registers them once, under a lock, so repeated renders in one
process and parallel tests are both safe.

Two things in `EnsureRegistered()` catch people out. It must set
`ImageSource.ImageSourceImpl` to a CodeBrix.Imaging-backed implementation before any image
can be placed, and forgetting it fails at image placement rather than at font lookup. And
face-name lookups require a registration per face, not one per family, so the method loops
over the face names rather than registering the family once; the comment in the code spells
out the difference. The sample also names its semibold face so that the name contains
"bold", so the resolver serves it for bold requests, with a comment explaining that semibold
reads better than a heavy bold at caption sizes.

The `.ttf` files and their OFL license texts are both `EmbeddedResource` items in the
library's csproj, so the license travels with the binary.

See [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).

### The Skia page and its reflowing bottom bar

`CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml` is a compact example of the
Skia XAML dialect: the platform's own control and data namespaces, `{d:Binding ...}` for
bindings, the view model declared in `Page.DataContext` with an
`assembly=WikipediaPublisher.Core` namespace because the view model is compiled into Core
rather than into the head, and the application font applied through a `StaticResource` that
points at a `.ttf` by URI, because merging a font package's resource dictionary does not
work on Skia targets.

The bottom bar is where the FlexPanel add-in earns its place. The save-target group and the
page-size-plus-Publish group sit in a `FlexPanel` with `Wrap="Wrap"`, so they share one row
while the window is wide and split onto two rows in portrait. `Grow` and `Basis` are
attached properties set on the children, not on the panel, and setting `Basis` is what makes
the wrap point deterministic rather than content-dependent. Only the Skia UI uses this: the
WinUI and WPF pages lay the same bar out with a `Grid`, so the reflow is a Skia-head
behavior rather than an application-wide one.

One deviation is worth naming. Enter in the search box is handled in the Skia and WinUI
pages by a `KeyDown` handler in code-behind that checks `CanExecute` and calls `Execute`.
The WPF head does the same job declaratively with `TextBox.InputBindings` and a `KeyBinding`
pointed at `SearchCommand`, which is the form to prefer; where a key handler is unavoidable
it should stay a one-line forward to the command.

See [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension),
[Wrap and reflow a layout with the FlexPanel add-in](../BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in),
[Run a command when the user presses Enter in a text box](../BLUEPRINTS-ViewsAndControls.md#run-a-command-when-the-user-presses-enter-in-a-text-box)
and [Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).

### Head plumbing, and the two solution files

Four of the six Skia heads are the generated `Program.cs` unchanged apart from the backend
call. The other two are worth reading for what they had to add:

- `CodeBrixPlatform/WikipediaPublisher.WinWpfSkia/Program.cs` casts the built host to its
  concrete type between `Build()` and `Run()` to set the render surface to software, with a
  comment explaining the composition conflict that otherwise leaves the window blank. Its
  csproj carries the matching rule: do not set `UseWPF` on a Skia-on-WPF head, or the WPF
  build targets try to compile the platform's XAML pages as WPF XAML.
- `CodeBrixPlatform/WikipediaPublisher.LinuxFrameBuffer/Program.cs` configures orientation
  and auto-rotation, opts into the platform's own file save picker, and enables the software
  keyboard, all through the framebuffer head's own builder callback. That head is the reason
  the view model has a typed-path fallback at all. It is also the one place in the
  application with a machine-specific value in it: `RestrictToFolder` is a hard-coded home
  directory, and it should be a configured value in an application of your own.

The application also ships two solution files, because two of its heads cannot be built
anywhere but Windows. Rather than hide that, both `.slnx` files open with a comment saying
which is which and why. Three project-level details make the arrangement work, and each is
worth knowing:

- `WikipediaPublisher.WinWpfSkia` targets `net10.0-windows` but sets
  `EnableWindowsTargeting`, so it compiles (though it cannot run) on Linux and macOS and can
  therefore live in the cross-platform solution. The truly Windows-only heads are kept out
  of that solution entirely rather than given the same property.
- The WinUI head declares only x86, x64 and ARM64 platforms, with no Any CPU, so
  `WikipediaPublisher.Windows.slnx` restricts its own solution platforms to match; without
  that the IDE offers an Any CPU configuration it cannot map to that project.
- Every Skia head must both add its XAML files as `Page` items and remove the same files
  from `None`, or the XAML is treated as content and never compiled. The shared UI itself is
  a `.shproj` whose `SharedGUID` has to match the `ProjectGuid` in the corresponding
  `.projitems`.

See [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere),
[Let a Windows-targeting head build inside a cross-platform solution](../BLUEPRINTS-ProjectLayoutAndPackaging.md#let-a-windows-targeting-head-build-inside-a-cross-platform-solution)
and [Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares).

### Testing a pipeline that talks to the internet

The test project covers a library whose whole job is to talk to a public website, and it
does so without making every run slow. `Internal/ArticleParserTests.cs` parses an article
fixture embedded in the test assembly and asserts on the title, the short description, the
lead image, block counts by kind, stop-section exclusion, citation-marker stripping, image
captions and media page titles, plus pure theories over the URL helpers and the text-run
extractor with HTML parsed inline. The fixture HTML is parsed once and cached in a static
field across those tests. `Services/ArticleRenderServiceTests.cs` composes and renders the
same fixture at two trim sizes entirely offline, verifying the produced file by its `%PDF-`
signature and a page count rather than against a golden file, which is what makes the
assertion stable while the design is still being tuned.

The live tests sit in the same class and use the public service exactly as the view model
does: the same request record, the same `IProgress<T>`, and
`TestContext.Current.CancellationToken` on every awaited call. The fast one fetches and
parses two articles without downloading any images, and exists as a regression test for the
multiple-content-container problem described above; the slow ones render end to end and
download every image, and say so in their comments. Assertions against live content are
deliberately loose, because the articles change.

All of this reaches the library's internals because
`WikipediaPublisher.RenderArticle/InternalsVisibleTo.cs` names the test assembly. The
family's `SimpleTestFixture.cs` is file-linked into the project as well, ready to resolve
`IArticleRenderService` through the container; note that it is feature-gated by compilation
constants, and the test csproj defines `SIMPLE_OUTPUT_LOGGING` for the Debug configuration.

See [Read a committed fixture from beside the test binary](../BLUEPRINTS-Testing.md#read-a-committed-fixture-from-beside-the-test-binary),
[Assert on a generated document without a golden file](../BLUEPRINTS-Testing.md#assert-on-a-generated-document-without-a-golden-file),
[Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project)
and [Test a service the way the container builds it](../BLUEPRINTS-Testing.md#test-a-service-the-way-the-container-builds-it).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the third-party
content bundled with or used by this application. Two typefaces are embedded in the
pipeline library under the SIL Open Font License: EB Garamond (the EB Garamond Project
Authors) and Source Sans 3 (Adobe, with Reserved Font Name "Source"); the full license
texts are stored beside the font files as
`WikipediaPublisher.RenderArticle/Fonts/OFL-EBGaramond.txt` and
`WikipediaPublisher.RenderArticle/Fonts/OFL-SourceSans3.txt`. Article text and images are
fetched from Wikipedia at run time rather than redistributed: the text is under CC BY-SA,
each image carries its own license, and every generated PDF prints the attribution in its
colophon. The exception is the example PDFs in `SampleOutput/`, which are generated output
committed to the folder. Third-party code arrives as packages, each carrying its own
notices, so those are not repeated here.

## License

WikipediaPublisher is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
