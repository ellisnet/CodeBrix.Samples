# CodeBrix.Samples Blueprints: Application structure and startup

These recipes cover everything that has to happen before your first page
appears: what a head's `Program.Main` must contain, which backend call
distinguishes one head from another, and the ordering contract inside
the `App` constructor for fonts, the `SimpleServiceResolver` container,
`SimpleViewModel.SetIsDesignMode(false)` and `InitializeComponent()`. They also
cover the pieces that hang off that startup path - supplying a host builder,
registering a library's services through a single `AddXxx` extension, wiring
Debug-only console logging, creating the window and navigating to the first
page. Reach for this file when you are starting a new application, adding a
head to an existing one, or chasing a startup problem such as an application
that launches and then does nothing, a head whose window renders blank,
or a head that needs a picker or software keyboard opted in; the last few
recipes deal with sharing one view model across native heads and detecting
at run time which head is hosting you.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Start each head from a Program Main and pick the platform backend](#start-each-head-from-a-program-main-and-pick-the-platform-backend)
- [Bootstrap the application in the App constructor](#bootstrap-the-application-in-the-app-constructor)
- [Create the main window and navigate to the first page](#create-the-main-window-and-navigate-to-the-first-page)
- [Supply a generic host builder to SimpleServiceResolver](#supply-a-generic-host-builder-to-simpleserviceresolver)
- [Register library services with one AddXxx extension method](#register-library-services-with-one-addxxx-extension-method)
- [Turn on console logging only in Debug builds](#turn-on-console-logging-only-in-debug-builds)
- [Set a bundled font as the default text font and register script fallbacks](#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
- [Enable a picker and the software keyboard on the Linux framebuffer head](#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head)
- [Force the software render surface on the WinWpfSkia head](#force-the-software-render-surface-on-the-winwpfskia-head)
- [Keep Main synchronous and STA so an embedded WebView can start](#keep-main-synchronous-and-sta-so-an-embedded-webview-can-start)
- [Turn on extra media codecs once at startup](#turn-on-extra-media-codecs-once-at-startup)
- [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads)
- [Detect which platform head is running without referencing it](#detect-which-platform-head-is-running-without-referencing-it)

## Related blueprints

- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - where the design-mode guard, SimpleViewModel and SimpleCommand take over once startup hands off
- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - how a view model reaches the services you registered here, through bridge interfaces the head supplies
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](BLUEPRINTS-ProjectLayoutAndPackaging.md) - the head and library csproj shapes behind linked source, compilation symbols and package placement
- [BLUEPRINTS-SettingsAndPersistence.md](BLUEPRINTS-SettingsAndPersistence.md) - the settings store that some applications open in the App constructor

---

## Application structure and startup

### Start each head from a Program Main and pick the platform backend

**When you want this.** You are writing the entry point of a head project and want
to know the minimum it has to contain, and what a head is allowed to differ on.

**The MVVM shape.** `Program.Main` owns nothing but hosting. It initializes
logging, builds a host with `CodeBrixPlatformHostBuilder`, hands it a factory for
the shared `App` class, selects exactly one backend, and runs. No application
logic lives in a head; services, fonts, settings and the first page all belong to
`App`, and everything the user interacts with belongs to a view model.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.LinuxX11/Program.cs
using CodeBrix.Platform.UI.Hosting;
using System;

namespace MediaPlayerDemo;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxX11()
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
```

The backend call is the only line that changes between heads:

| Head | Call |
| --- | --- |
| LinuxX11 | `.UseLinuxX11()` |
| LinuxWayland | `.UseLinuxWayland()` |
| LinuxFrameBuffer | `.UseLinuxFrameBuffer()` |
| MacOS | `.UseMacOS()` |
| Win32Skia | `.UseWindowsWin32()` |
| WinWpfSkia | `.UseWindowsWpf()` |

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.LinuxX11/Program.cs` and the five sibling
head projects under `MediaPlayerDemo/src/`
`KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxX11/Program.cs`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Win32Skia/Program.cs` (the one head
in the repository that uses `async Task Main` with `await host.RunAsync()`),
`NotionDocumentCreator/src/NotionDocumentCreator.LinuxX11/Program.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.LinuxX11/Program.cs`,
`PalmVisualizer/src/PalmVisualizer.LinuxX11/Program.cs`,
`PdfSideBySide/src/PdfSideBySide.LinuxX11/Program.cs`,
`Pinta.Brix/src/Pinta.Brix.LinuxX11/Program.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.LinuxX11/Program.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.LinuxX11/Program.cs`,
`WebcamPainter/src/WebcamPainter.LinuxX11/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.LinuxX11/Program.cs`,
`CodeBrixVideoTool/src/CodeBrixVideoTool.LinuxX11/Program.cs`

**Sharp edges.**
- `App.InitializeLogging()` is called before the host is built, never after. The
  method carries the comment "Called from each head's Program.Main BEFORE
  building the host" in every application that has it; logging wired after
  `Build()` misses the platform's own startup messages.
- `[STAThread]` is on `Main` in every head, including the Linux and macOS ones.
- `.App(() => new App())` takes a factory, not an instance. The host decides when
  the application object is constructed.
- `.UseDirectSkiaCanvasMode()` is marked experimental in the generated comment
  ("should be safe to leave enabled") and most applications keep it on every head.
  WebcamPainter calls it on the LinuxX11 head only, and PolyHavenBrowser only on
  its two Windows heads, so do not assume every head in an application has it.
- The heads all declare the same namespace as the shared UI project, which is
  what lets `new App()` resolve in `Program.cs` with no using directive. Some
  heads carry a `// ReSharper disable CheckNamespace` comment because of it.
- Heads are not literally interchangeable: copy one to a new platform and check
  what it adds after `Build()` (see the WinWpfSkia and framebuffer blueprints).

### Bootstrap the application in the App constructor

**When you want this.** Every application. This is the ordering contract for the
`App` constructor: font configuration, dependency-injection container, design
mode off, then `InitializeComponent()`.

**The MVVM shape.** `App` is the composition root and does nothing else. It sets
the platform's default text font, creates the `SimpleServiceResolver` from an
`IHostBuilderProvider` and registers the application's services through one
extension method per library, calls `SimpleViewModel.SetIsDesignMode(false)` so
view models built by the XAML parser run their real constructor path, and only
then initializes the XAML. View models resolve what they need with the inherited
`GetService<T>()`; nothing is passed down from `App`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs
public App()
{
    //Set Roboto as the default font for all text in the application
    global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";

    //Fonts consulted for characters the default font has no glyph for
    global::CodeBrix.Platform.UI.FeatureConfiguration.Font.FallbackFontFamilies =
    [
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf",
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf",
    ];

    SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
    {
        //Register the app's services here
        services.AddCreateDocument();
    });
    SimpleViewModel.SetIsDesignMode(false);

    InitializeComponent();
}
```

The matching half is in the view model: the first line of every view-model
constructor in the family is the design-mode guard, and it only works because
`App` turned design mode off first.

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    _documentSvc = GetService<INotionDocumentService>();
    // ...
}
```

An application with a settings store opens it in the same constructor, before
`InitializeComponent()`, because the page's view model reads a setting in its own
constructor:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs
    SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
    {
        //Register the app's services here
        services.AddKenneyAssetBrowser();
    });
    SimpleViewModel.SetIsDesignMode(false);

    //Open (or silently create) the single portable settings.sqlite store —
    //  including its startup auto-backup and pruning — before any UI renders.
    SettingsService.Initialize();

    InitializeComponent();
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`,
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`,
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/App.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/App.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`

**Sharp edges.**
- Forgetting `SetIsDesignMode(false)` is silent. Nothing throws; every view model
  built by XAML takes its design-time early-out and the application starts and
  does nothing. It has to run before the first view model is constructed, which
  in practice means before `InitializeComponent()`.
- `SimpleServiceResolver.CreateInstance()` must be called even when there is
  nothing to register. MediaPlayerDemo, PainDiagram, PalmVisualizer, PdfSideBySide,
  Pinta.Brix and WebcamPainter all keep an empty, commented registration callback
  rather than dropping the call.
- Font configuration is set before `InitializeComponent()` so the first measured
  text already uses the right family.
- The MAUI head in JustBetweenUs is the one place the order differs: it calls
  `InitializeComponent()` first, then the resolver, then `SetIsDesignMode(false)`.
  Both orders work there, but keeping the resolver before any view is constructed
  is the safer habit, because a page whose XAML instantiates a view model resolves
  services during `InitializeComponent()`.
- Some applications write the guard as `if (!IsDesignMode(true)) { ... }` wrapping
  the whole body instead of an early return; the two forms are equivalent.

### Create the main window and navigate to the first page

**When you want this.** You are writing the `OnLaunched` override for a head and
want the smallest correct window-and-frame bootstrap.

**The MVVM shape.** `App` owns the window and the navigation frame and nothing
else. The page it navigates to sets its own `DataContext` and does its own bridge
wiring; `App` never touches a view model.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs
protected Window MainWindow { get; private set; }

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    MainWindow = new Window { Title = "MediaPlayerDemo" };

    if (MainWindow.Content is not Frame rootFrame)
    {
        rootFrame = new Frame();
        MainWindow.Content = rootFrame;
        rootFrame.NavigationFailed += OnNavigationFailed;
    }

    if (rootFrame.Content == null)
    {
        rootFrame.Navigate(typeof(Views.MainPage), args.Arguments);
    }

    MainWindow.Activate();
}

void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
{
    throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`

**Also shown by.**
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`,
`JustBetweenUs/JustBetweenUs.WinUI/App.xaml.cs` (the native WinUI 3 head keeps
the same override almost verbatim, with the stock template code it replaces left
in the file as a comment)

**Sharp edges.**
- `NavigationFailed` throws rather than logging, so a typo in the page type
  surfaces immediately instead of showing an empty window.
- A native WPF head has no frame at all: `PainDiagram.Wpf` uses
  `StartupUri="Views/MainWindow.xaml"` in `App.xaml` and has no `OnLaunched`
  override, while a native WinUI 3 head keeps the frame-and-`Navigate()` shape.

### Supply a generic host builder to SimpleServiceResolver

**When you want this.** `SimpleServiceResolver.CreateInstance()` needs an
`IHostBuilderProvider`, and you want that in one shared place rather than
duplicated in every head.

**The MVVM shape.** A small static helper in the library the heads reference
wraps `Host.CreateDefaultBuilder()` in an `IHostBuilderProvider` and hands back a
single shared instance. `App` passes `HostHelper.GetHost()` and its registration
callback. View models then resolve services instead of constructing them.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/Helpers/HostHelper.cs
using CodeBrix.Platform.Simple;
using Microsoft.Extensions.Hosting;

namespace MediaPlayerDemo.Helpers;

/// <summary>
/// Supplies the generic-host builder that <see cref="SimpleServiceResolver"/> uses to build
/// the application's dependency-injection container at startup.
/// </summary>
public static class HostHelper
{
    private sealed class HostBuilderProvider : IHostBuilderProvider
    {
        public IHostBuilder CreateDefaultBuilder() => Host.CreateDefaultBuilder();
        public IHostBuilder CreateDefaultBuilder(string[] args) => Host.CreateDefaultBuilder(args);
    }

    private static readonly HostBuilderProvider Provider = new();

    /// <summary>Gets the shared host-builder provider.</summary>
    public static IHostBuilderProvider GetHost() => Provider;
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/Helpers/HostHelper.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Helpers/HostHelper.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Helpers/HostHelper.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/HostHelper.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/Helpers/HostHelper.cs`,
`PdfSideBySide/src/PdfSideBySide.Core/Helpers/HostHelper.cs`,
`Pinta.Brix/src/Pinta.Brix.Core/Helpers/HostHelper.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`,
`WebcamPainter/src/WebcamPainter.Core/Helpers/HostHelper.cs`,
`JustBetweenUs/Shared/Helpers/HostHelper.cs` and
`PainDiagram/Shared/Helpers/HostHelper.cs` and
`WikipediaPublisher/Shared/Helpers/HostHelper.cs` (these three live in a
`Shared/` folder and are file-linked into the Skia library and into each native
head, so all of an application's heads get an identical container)

**Sharp edges.**
- The provider is a private nested class exposed only through the interface, with
  one cached instance, so there is nothing to construct twice by accident.
- The hosting package is referenced by the library that carries the application's
  packages, not by the heads. Keeping it there is what lets every head share one
  helper.

### Register library services with one AddXxx extension method

**When you want this.** Your real work lives in a library and you want the
application to register it in one line, without the application ever naming the
implementation type.

**The MVVM shape.** The library exports an interface and one
`IServiceCollection` extension method. The application calls it inside the
`SimpleServiceResolver.CreateInstance()` callback. The view model resolves the
interface with `GetService<T>()` and never sees the concrete class.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/RegisterServices.cs
public static class RegisterServices
{
    /// <summary>
    /// Registers the WikipediaPublisher article-rendering services with the DI container.
    /// </summary>
    public static IServiceCollection AddRenderArticle(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }
        services.AddSingleton<IArticleRenderService, ArticleRenderService>();
        return services;
    }
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        Debug.WriteLine("Main view model startup.");

        _renderSvc = GetService<IArticleRenderService>();
        // ...
        StatusText = "Search for an article, browse to it, choose where to save the PDF, then click Publish.";
    }
}
```

**Variant: one application-level extension that calls the library's own.** When
an application has several services and one of them is a library with its own
registration method, the application keeps a single `RegisterServices.cs` that
chains them, so `App` still calls one method:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs
public static class RegisterServices
{
    /// <summary>Registers the Poly Haven API client, the catalog service and the download service.</summary>
    public static IServiceCollection AddPolyHavenBrowser(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }

        services.AddPolyHavenApiClient(options =>
        {
            //Poly Haven asks API consumers to identify themselves.
            options.UserAgent = "PolyHavenBrowser/1.0 (CodeBrix.Platform sample; +https://polyhaven.com)";
        });

        services.AddSingleton<ModelCatalogService>();
        services.AddSingleton<ModelDownloadService>();
        services.AddSingleton<DocumentBackdropService>();

        return services;
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/RegisterServices.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs`

**Also shown by.**
`JustBetweenUs/JustBetweenUs.Encryption/RegisterServices.cs` (`AddEncryption()`),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/RegisterServices.cs`
(`AddKenneyAssetBrowser()`),
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/RegisterServices.cs`
(`AddCreateDocument()`),
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/RegisterServices.cs`,
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs` (two `AddSingleton`
calls straight in the callback, which is the smaller form when there is no
library boundary to respect)

**Sharp edges.**
- Every one of these extensions starts with a null check on `services` and
  returns the collection so calls chain.
- Registrations that own state are singletons: the Notion service holds the
  connected client and the discovered tree metadata between calls, and the
  article renderer owns an `HttpClient`.
- Library services take an optional `ILogger<T>` and fall back to a null logger,
  so the library still works in tests with no container at all.
- A view model that can also run without a container falls back to a concrete
  instance: `runner = GetService<IConversionRunner>() ?? new ConversionRunner();`
  in `CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`.

### Turn on console logging only in Debug builds

**When you want this.** You want platform and application diagnostics on a
console while developing, and a silent Release build, on every head.

**The MVVM shape.** Not a view-model concern. One `public static void
InitializeLogging()` on `App`, whole body inside `#if DEBUG`, called from every
head's `Main` as its first statement.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs
// Called from each head's Program.Main BEFORE building the host.
public static void InitializeLogging()
{
#if DEBUG
    var factory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
        builder.AddFilter("Windows", LogLevel.Warning);
        builder.AddFilter("Microsoft", LogLevel.Warning);
    });

    global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
    global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
}
```

**Variant: let one component through the filter.** CodeBrixVideoTool raises one
category back to Information because the player add-in logs the graphics backend
it chose exactly once and that line is worth seeing:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs
        builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
        //The player add-in logs the graphics backend it chose exactly once, at Information.
        builder.AddFilter("CodeBrix.Platform.UI.VideoPlayer", LogLevel.Information);
```

**Variant: guard the adapter call when the same file is linked into a native
head.** Applications whose `App.xaml.cs` or view model source is compiled into a
non-Skia head wrap the adapter call in the `HAS_CODEBRIX` symbol that only the
Skia projects define:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs
    global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_CODEBRIX
    global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`,
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/App.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`

**Sharp edges.**
- Both statements are needed. Assigning `AmbientLoggerFactory` alone is not
  enough; `LoggingAdapter.Initialize()` is what connects the platform's own
  logging to your factory, and it comes second.
- The minimum level is Information while the platform, `Windows` and `Microsoft`
  categories are filtered to Warning, so your own messages are visible without
  the framework drowning them.
- Because the whole body is inside `#if DEBUG`, the method compiles to nothing in
  Release and every call site stays valid.

### Set a bundled font as the default text font and register script fallbacks

**When you want this.** You want one typeface everywhere without setting
`FontFamily` on every control, including on heads with no system font stack to
fall back to, such as the Linux framebuffer head.

**The MVVM shape.** Pure startup and view configuration, in two places: `App`'s
constructor sets the platform's default text font family (and, optionally, the
faces consulted for characters it has no glyph for) before
`InitializeComponent()`, and `App.xaml` publishes the same face under a resource
key so a page can name it.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs
        //Set Roboto as the default font for all text in the application
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";

        //Fonts consulted for characters the default font has no glyph for
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.FallbackFontFamilies =
        [
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf",
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf",
        ];
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/App.xaml -->
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <!-- Load WinUI resources -->
        <c:XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      </ResourceDictionary.MergedDictionaries>
      <!-- Roboto font - reference the .ttf file directly (the Fonts.xaml
           merge does not work on Skia targets) -->
      <m:FontFamily x:Key="RobotoFont">ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf</m:FontFamily>
    </ResourceDictionary>
  </Application.Resources>
```

```xml
<!-- From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml -->
<Page
    x:Class="WebcamPainter.Views.MainPage"
    FontFamily="{StaticResource RobotoFont}"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml` and `App.xaml.cs`
`WebcamPainter/src/WebcamPainter.UI/App.xaml` and `Views/MainPage.xaml`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs` (a serif family with
matching Noto Serif fallbacks),
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs` (a plain Noto Sans face
in the fallback list as well as the two script faces),
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml`,
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/App.xaml`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml`

**Sharp edges.**
- The comment in `App.xaml` records the rule the whole repository follows:
  merging a font package's `Fonts.xaml` resource dictionary does not work on Skia
  targets. Reference the `.ttf` directly through an `ms-appx:///` URI whose first
  segment is the font assembly name. Several applications keep the commented-out
  merge line in the file as a marker.
- Two forms of the URI appear. Some applications add a `#FamilyName` suffix
  (`.../Roboto.ttf#Roboto`) and some do not; where the suffix is used, both
  halves are required.
- `DefaultTextFontFamily` and the `FontFamily` resource are different mechanisms
  and both are worth setting: the first covers text the application never styles,
  the second is what `FontFamily="{StaticResource ...}"` binds to.
- Fallback entries name the plain, weight-less face files. A font package also
  ships per-weight files whose names will not resolve here.
- The font package is referenced by the library that carries the application's
  packages, so all six heads get it transitively; the heads never reference it.
- A native head has its own `App.xaml`, so nothing set in the shared one reaches
  it. The MAUI head in JustBetweenUs registers its own copies of the font files
  through `ConfigureFonts` in `MauiProgram.cs` instead.

### Enable a picker and the software keyboard on the Linux framebuffer head

**When you want this.** Your application asks the user for a file or folder, or
takes typed input, and you want it to work on the LinuxFrameBuffer head, which
has no desktop chrome to borrow a picker or a keyboard from.

**The MVVM shape.** Head configuration only. The view model does not change: it
still calls a picker through its bridge and still binds a `TextBox`. The head is
what decides whether a picker window and an on-screen keyboard exist to serve
those calls, and the view model already has a graceful path for a head that
supplies neither.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer/Program.cs
var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxFrameBuffer(fb => fb
        .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
        .AutoRotationEnabled(true)
        .EnableFolderPicker(new FolderPickerOptions {
           AllowNewFolderCreate = true,
           StartFolder = "/home/jeremy/Temp",
           RestrictToFolder = "/home/jeremy",
        })
        //The FrameBuffer head has no OS chrome, so the "Save PDF as…" picker the
        //  Document button pops is opt-in
        .EnableFileSavePicker(new FilePickerOptions {
           AllowNewFolderCreate = true,
           StartFolder = "/home/jeremy/Temp",
           RestrictToFolder = "/home/jeremy",
           RequiredExtension = ".pdf",
        })
        .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
            ShowDismissKey = true,  //default behavior = true
            KeyHeight = SoftwareKeyHeight.FullHeight,  //default behavior = FullHeight
        })
    )
    .UseDirectSkiaCanvasMode()
    .Build();

host.Run();
```

The application's resource dictionary restyles that built-in chrome, because the
picker and keyboard resolve the same `ContentDialog` keys the application already
themes:

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml -->
<!-- Dialogs open in the popup layer, which follows the app default theme (the
     RequestedTheme="Dark" above) rather than RootGrid's - these ContentDialog
     keys then refine them to the app palette. On the FrameBuffer heads the
     built-in picker/software-keyboard chrome resolves the same keys, so it
     restyles identically -->
<m:SolidColorBrush x:Key="ContentDialogBackground" Color="#1F232B" />
<m:SolidColorBrush x:Key="ContentDialogForeground" Color="#F2F4F8" />
<!-- Resolved by the FrameBuffer/Emulated picker + software-keyboard chrome -->
<m:SolidColorBrush x:Key="ContentDialogTopOverlay" Color="#1F232B" />
<m:SolidColorBrush x:Key="ContentDialogSeparatorBorderBrush" Color="#2A2F39" />
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer/Program.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.LinuxFrameBuffer/Program.cs`
(software keyboard only, at `SoftwareKeyHeight.HalfHeight` so the keyboard leaves
more of the page visible),
`KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxFrameBuffer/Program.cs` (folder
picker with `AllowNewFolderCreate = false`),
`NotionDocumentCreator/src/NotionDocumentCreator.LinuxFrameBuffer/Program.cs`
(save picker plus keyboard, because the user types a long API token),
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.LinuxFrameBuffer/Program.cs`

**Sharp edges.**
- Both features are off unless you opt in, one builder call each, and only on
  this head. Code that assumes a picker exists gets a `NotSupportedException`
  instead of a dialog.
- `StartFolder` and `RestrictToFolder` in the samples are the author's own machine
  paths. Treat them as placeholders and compute them from the environment in your
  own application; `RestrictToFolder` fences the picker so the user cannot
  navigate above it.
- `RequiredExtension` on the picker and the application's own expectation about
  the file it will write have to agree.
- `ShowDismissKey` defaults to true and `KeyHeight` defaults to `FullHeight`; the
  samples record both defaults in comments next to the overrides.
- Dialogs open in the popup layer, which follows the application's
  `RequestedTheme` rather than the theme of the grid they were raised from, so a
  dark application has to key the `ContentDialog` brushes at the
  `Application.Resources` level.

### Force the software render surface on the WinWpfSkia head

**When you want this.** Your WPF-hosted head opens a window that stays blank,
black or white while every other head renders correctly.

**The MVVM shape.** Head-level plumbing in `Program.cs`, between `Build()` and
`Run()`. The built host is type-tested and its render surface type changed;
nothing about the application changes.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.WinWpfSkia/Program.cs
using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia.Wpf;
using System;

namespace JustBetweenUs;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWpf()
            .Build();

        // ...
        if (host is WpfHost wpfHost)
        {
            wpfHost.RenderSurfaceType = RenderSurfaceType.Software;
        }

        host.Run();
    }
}
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.WinWpfSkia/Program.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.WinWpfSkia/Program.cs`,
`MediaPlayerDemo/src/MediaPlayerDemo.WinWpfSkia/Program.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.WinWpfSkia/Program.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.WinWpfSkia/Program.cs`,
`PalmVisualizer/src/PalmVisualizer.WinWpfSkia/Program.cs`,
`PdfSideBySide/src/PdfSideBySide.WinWpfSkia/Program.cs`,
`Pinta.Brix/src/Pinta.Brix.WinWpfSkia/Program.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.WinWpfSkia/Program.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.WinWpfSkia/Program.cs`,
`WebcamPainter/src/WebcamPainter.WinWpfSkia/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.WinWpfSkia/Program.cs`

**Sharp edges.**
- The comment trimmed out of the block above explains why it is needed: the WPF
  host's default OpenGL renderer draws through raw `opengl32` onto WPF's own
  DirectX-composited window handle. That is an airspace conflict on many systems,
  so the window appears but the content never composites. Software rendering
  blits the Skia frame into WPF and composites correctly.
- The cast is guarded with `is`, so the file stays valid if the host type ever
  changes.
- This head needs `using CodeBrix.Platform.UI.Runtime.Skia.Wpf;`, which the other
  heads do not have; the type comes from that head's runtime package.
- In most of these applications this is the only per-head behavioral difference
  in the whole solution.

### Keep Main synchronous and STA so an embedded WebView can start

**When you want this.** Your application hosts a WebView on Windows and you are
tempted to write `async Task Main`.

**The MVVM shape.** Head plumbing only, but it decides whether the WebView bridge
works at all.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Win32Skia/Program.cs
// Must be a synchronous STA Main: WebView2 (CoreWebView2Environment.CreateAsync) requires the
// UI thread to be an STA. With 'async Task Main' the [STAThread] attribute is ignored and the
// thread runs as MTA, so WebView2 creation throws RPC_E_CHANGED_MODE ("Cannot change thread mode
// after it is set."). host.Run() pumps the Win32 message loop synchronously on this STA thread.
[STAThread]
public static void Main(string[] args)
{
    App.InitializeLogging();

    var host = CodeBrixPlatformHostBuilder.Create()
        .App(() => new App())
        .UseWindowsWin32()
        .Build();

    host.Run();
}
```

**Where to look.**
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Win32Skia/Program.cs`

**Sharp edges.**
- `[STAThread]` is silently ignored on an `async Task Main`; the failure shows up
  much later as an RPC error when the WebView is created.

### Turn on extra media codecs once at startup

**When you want this.** You are playing media through an add-in and you need
decoders that the add-in does not, and by design cannot, reference itself.

**The MVVM shape.** A small static helper in the library that owns playback,
called from `App`'s constructor before anything else. It is idempotent behind a
lock and exposes `IsRegistered` so a test can assert it.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs
public static class PlaybackCodecs
{
    private static readonly object Gate = new();

    /// <summary>True once both codecs have been turned on.</summary>
    public static bool IsRegistered { get; private set; }

    public static void RegisterOnce()
    {
        lock (Gate)
        {
            if (IsRegistered)
            {
                return;
            }

            CodeBrixVideoPlaybackDav1d.Register();
            CodeBrixAudioOpus.Register();
            IsRegistered = true;
        }
    }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs
//Turn on AV1 video and Opus audio, once. Every one of the four formats this application
//writes carries AV1, so nothing plays at all without the first of these.
PlaybackCodecs.RegisterOnce();
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/PlaybackCodecsTests.cs`

**Sharp edges.**
- The class documentation is explicit that these decoders are the application's
  dependencies and never the add-in's. Their licenses differ from the add-in's,
  which is exactly why each ships as its own package and an application that
  wants them references them and calls `Register()` once. The add-in resolves
  codecs through the playback session's registries, so it plays them with no
  change and no reference of its own.
- The source says outright: "There is deliberately no module initializer doing
  this - that would work in a debug build and silently not run in a trimmed
  publish."
- Register from `App`'s constructor, ahead of the container and the XAML, so
  nothing can open a media file first.

### Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads

**When you want this.** You must ship a native Windows or mobile build alongside
the Skia heads and do not want a second implementation of your logic.

**The MVVM shape.** The view model is a plain class deriving from
`SimpleViewModel` that references only the Simple toolkit and your own service
interfaces. It is not shipped as a library: every head, Skia or native, pulls it
in as a linked `<Compile>` item and compiles its own copy. Each head then supplies
platform plumbing through the bridge interfaces the view model declares, and each
head's `App` does the same two Simple-toolkit calls at startup. The only
conditional compilation inside the view model is a single attribute.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, IFileSaveBridge, ICanvasInvalidator
{
    // ...
}
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/PainDiagram.Wpf.csproj -->
<ItemGroup>
  <Compile Include="..\Shared\Drawing\DrawingCanvas.cs" Link="Drawing\DrawingCanvas.cs" />
  <Compile Include="..\Shared\Helpers\HostHelper.cs" Link="Helpers\HostHelper.cs" />
  <Compile Include="..\Shared\ViewModels\MainViewModel.cs" Link="ViewModels\MainViewModel.cs" />
</ItemGroup>
```

The native head brings its own `App.xaml` and its own window or page, and still
performs the same bootstrap:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/App.xaml.cs
public partial class App : Application
{
    public App()
    {
        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //No custom services needed - the drawing session lives in the view model
        });
        SimpleViewModel.SetIsDesignMode(false);
    }
}
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml -->
<Window x:Class="PainDiagram.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:PainDiagram.ViewModels"
        xmlns:drawing="clr-namespace:CodeBrix.Imaging.Drawing"
        Title="Pain Diagram" Height="720" Width="640">

    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>
    <!-- ... -->
</Window>
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/PainDiagram.Wpf/` and `PainDiagram/PainDiagram.WinUI/`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj`,
`JustBetweenUs/JustBetweenUs.Wpf/JustBetweenUs.Wpf.csproj`,
`JustBetweenUs/Mobile/JustBetweenUs.Mobile.csproj`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.WinUI/` and
`WikipediaPublisher/WikipediaPublisher.Wpf/` (eight heads share one
`Shared/ViewModels/MainViewModel.cs`; the WinUI head links the view model, the
host helper and the file-dialog helper, the WPF head links only the first two,
because a WPF `SaveFileDialog` leaves no placeholder file to clean up)

**Sharp edges.**
- `HAS_CODEBRIX` is defined by the library that carries the platform packages and
  by every Skia head csproj, but not by the native projects, so the `[Bindable]`
  attribute is applied only in the platform assemblies. If you link view-model
  source into a native head, check which symbols that project defines.
- Keep such symbols to a minimum. JustBetweenUs also defines `HAS_WINUI` for one
  startup timing difference, and every symbol is a place where a head can drift.
- File-linked source means every consuming assembly must also supply anything the
  source expects at run time. PainDiagram embeds its body-map image three times,
  once per assembly that compiles the shared view model, under one logical
  resource name.
- The native heads have their own `App.xaml`, so anything in the shared one - a
  font resource, the default font family - does not reach them.
- Because the file is compiled into each head, the head's root namespace must
  agree with the namespace declared in the file.

### Detect which platform head is running without referencing it

**When you want this.** A library needs to know which of the six heads is hosting
it, and must not take a dependency on any of them.

**The MVVM shape.** A static, lazily computed detection inside the headless
library; everything above it consumes a plain enum.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs
// Each head's Program.cs loads exactly one head runtime assembly (via
// CodeBrixPlatformHostBuilder.Use*), so by the time any UI runs, scanning the loaded
// assemblies identifies the head without this library referencing any of them.
private static PlatformHead DetectCurrentHead()
{
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        var head = ClassifyAssemblyName(assembly.GetName().Name);
        if (head != PlatformHead.Unknown)
        {
            return head;
        }
    }

    return PlatformHead.Unknown;
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalPlatformSupport.cs`

**Sharp edges.**
- The detection is head-generic, not backend-specific: `MetalPlatformSupport`
  forwards to the same scan rather than duplicating it.
- It relies on the head's runtime assembly already being loaded, which is true by
  the time any UI runs but not necessarily earlier. Do not call it from a static
  initializer that runs before the host is built.
- A `Lazy<PlatformHead>` caches the result so the assembly scan happens once.
- For a view model that only needs the operating system rather than the head,
  `SimpleOsInfo` is the simpler answer; see the view-model area.

