# CodeBrix.Samples Blueprints: Application structure and startup

These recipes cover everything that has to happen before your first page
appears: what a head's `Program.Main` must contain, which backend call
distinguishes one head from another, and the ordering contract inside
the `App` constructor for fonts, the `SimpleServiceResolver` container,
`SimpleViewModel.SetIsDesignMode(false)` and `InitializeComponent()`. They also
cover the pieces that hang off that startup path - supplying a host builder,
registering a library's services through a single `AddXxx` extension, registering
a factory for the things a view model must own rather than share, wiring
Debug-only console logging, creating the window, deciding what size it opens
at and how small it may be dragged, and navigating to the first
page. Reach for this file when you are starting a new application, adding a
head to an existing one, or chasing a startup problem such as an application
that launches and then does nothing, a head whose window renders blank,
or a head that needs a picker or software keyboard opted in and folders computed
for it. Later recipes deal with sharing one view model across native heads,
detecting at run time which head is hosting you, and letting each head register
the hardware implementations it alone may reference.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Start each head from a Program Main and pick the platform backend](#start-each-head-from-a-program-main-and-pick-the-platform-backend)
- [Bootstrap the application in the App constructor](#bootstrap-the-application-in-the-app-constructor)
- [Create the main window and navigate to the first page](#create-the-main-window-and-navigate-to-the-first-page)
- [Set the window's launch size](#set-the-windows-launch-size)
- [Keep the window from shrinking below a minimum](#keep-the-window-from-shrinking-below-a-minimum)
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
- [Give a hosted guest program the environment it assumes with a bootstrap script](#give-a-hosted-guest-program-the-environment-it-assumes-with-a-bootstrap-script)
- [Register hardware implementations from each head and ask a finder for the best one](#register-hardware-implementations-from-each-head-and-ask-a-finder-for-the-best-one)
- [Compute the framebuffer picker's folders from the environment](#compute-the-framebuffer-pickers-folders-from-the-environment)
- [Register a factory and let the view model ask it for what it owns](#register-a-factory-and-let-the-view-model-ask-it-for-what-it-owns)

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
`CodeBrixVideoTool/src/CodeBrixVideoTool.LinuxX11/Program.cs`,
`InannaRosette/src/InannaRosette.LinuxX11/Program.cs` and the three sibling heads
(four of the family's six, identical apart from the one `Use…()` call, each
calling `App.InitializeLogging()` before the host is built and each carrying
`[STAThread]` on `Main`, including the Linux and macOS ones)

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
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`,
`InannaRosette/src/InannaRosette.UI/App.xaml.cs`
(default text font family, the resolver plus `services.AddReading()`,
`SetIsDesignMode(false)`, the launch size and the requested theme, all before
`InitializeComponent()` - which is what runs the view model's constructor,
because the XAML is what declares it)

**Sharp edges.**
- Forgetting `SetIsDesignMode(false)` is silent. Nothing throws; every view model
  built by XAML takes its design-time early-out and the application starts and
  does nothing. It has to run before the first view model is constructed, which
  in practice means before `InitializeComponent()`.
- `SimpleServiceResolver.CreateInstance()` must be called even when there is
  nothing to register. MediaPlayerDemo, WebcamPainter, WebcamViewer,
  SimpleCbxVideoPlayer, GameEngineMusicDemo and PainDiagram's two native heads all
  keep an empty, commented registration callback rather than dropping the call, so
  view models can still call `GetService<T>()` and get a null back.
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
    MainWindow = new Window
    {
        Title = "MediaPlayerDemo"
    };

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
in the file as a comment),
`InannaRosette/src/InannaRosette.UI/App.xaml.cs`
(`OnLaunched` titles the window, pins the presenter's minimum size, puts a
`Frame` in it and navigates, with a `NavigationFailed` handler that throws rather
than leaving a blank window)

**Sharp edges.**
- `NavigationFailed` throws rather than logging, so a typo in the page type
  surfaces immediately instead of showing an empty window.
- A native WPF head has no frame at all: `PainDiagram.Wpf` uses
  `StartupUri="Views/MainWindow.xaml"` in `App.xaml` and has no `OnLaunched`
  override, while a native WinUI 3 head keeps the frame-and-`Navigate()` shape.

### Set the window's launch size

**When you want this.** The window should open at a size you chose rather than at
the platform's own default, and you have found that nothing you can write in
`OnLaunched` happens early enough to decide it.

**The MVVM shape.** Two constants on `App` and one assignment in the `App`
constructor, before `InitializeComponent()`. No view model is involved. Every
desktop head reads `ApplicationView.PreferredLaunchViewSize` while it is creating
the native window, and falls back to the platform's own 1024 by 640 when the value
is empty, so the size has to be in place before anything a page could reach exists.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
public partial class App : Application
{
    //The size the window opens at, and the smallest size it may be dragged to. The launch
    //size fits the header, the two search rows and about a dozen result rows without
    //scrolling; the minimum is the point below which the header stops fitting on two lines
    //and the result columns start to collide.
    private const int LaunchWidth = 1180;
    private const int LaunchHeight = 800;
    private const int MinimumWidth = 760;
    private const int MinimumHeight = 520;
```

The assignment is the last thing the constructor does before the XAML is
initialized:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
    //The size the first window opens at. ApplicationView.PreferredLaunchViewSize is the
    //only public seam an application has for its own launch size: every desktop head
    //reads it while it is creating the native window and falls back to the platform's
    //own 1024 by 640 when it is empty, so it has to be set before any window exists.
    //On the Linux X11 head the numbers are NATIVE pixels of the window's CLIENT area,
    //which on a display at scale 1 is the same as logical units; how each of the other
    //heads reads them is written up in the report that accompanied this change. The
    //value is set on every launch, unconditionally, because the platform remembers it
    //in its own settings file; setting it every time keeps that file in step with this
    //source file instead of letting an old value linger.
    Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
        new Windows.Foundation.Size(LaunchWidth, LaunchHeight);

    InitializeComponent();
```

Both applications write the two types fully qualified, so the file's `using` block
does not change.

**What each head does with the numbers.** They do not all read them the same way,
and at a display scale other than 1 that matters:

| Head | Launch size | Minimum size |
| --- | --- | --- |
| LinuxX11 | native pixels, client area | native pixels, client area |
| LinuxWayland | logical units, window geometry | logical units, window geometry |
| Win32Skia | native pixels, framed window | native pixels, framed window |
| MacOS | points, client area | points, framed window |
| WinWpfSkia | device-independent units, framed window | native pixels divided by the rasterization scale, framed window |

At a display scale of 1 every one of those reads the same, which is why one pair of
constants is the right thing to write. On the X11 head at a display scale of 2 the
same constants produce a window half the intended logical size and a clamp at half
the intended logical minimum.

**Where the value is kept between runs.** The setter does nothing but write two
doubles into `ApplicationData.Current.LocalSettings`, under the keys
`__CodeBrix.PreferredLaunchViewSizeKey.Width` and `.Height`, and the getter returns
an empty size when either is missing. The value therefore survives between runs,
which is why both applications set it unconditionally on every launch rather than
only when it is empty: an unconditional set is what stops the remembered copy
drifting away from the source file. On Linux the store is
`<LocalApplicationData>/<package name>/Settings/Local.dat`, and it is per head,
because the package name is the head's own assembly name. It does not collide with
the AppSettings add-in, whose store is a separate `settings.sqlite` under the
configuration folder.

**Variant: open at the size the user left.** Pinta.Brix reads two settings and
feeds them to the same property, and writes the size back on every resize; see
[Restore a remembered window size before any window exists](BLUEPRINTS-SettingsAndPersistence.md#restore-a-remembered-window-size-before-any-window-exists).
Fresco.Brix combines the two: the constants are the size a launch with an empty
store opens at, and a remembered size is applied a moment later through
`AppWindow.Resize` when the page loads. The two do not fight, because the platform
value is consulted while the native window is created and the remembered one is
applied after it exists.

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Also shown by.**
`CodeBrix.Samples.Gpl3/Fresco.Brix/src/Fresco.Brix.UI/App.xaml.cs` (in the
CodeBrix.Samples.Gpl3 repository: the same block with 1280 by 840, and one extra
sentence in the comment saying that a size the user left behind still wins),
`InannaRosette/src/InannaRosette.UI/App.xaml.cs`
(`ApplicationView.PreferredLaunchViewSize` set unconditionally on every launch,
with a comment explaining that the platform remembers it in its own settings file
and that writing it every time keeps that file in step with the source rather
than letting an old value linger)

**Sharp edges.**
- It has to be the constructor, not `OnLaunched`. The `Window` constructor creates
  the native window and the head reads the value while doing so, so by the time
  `OnLaunched` runs the window already has a size.
- The value persists, so a constant you change in source does not take effect
  unless the assignment actually runs. Setting it unconditionally on every launch
  is the habit that keeps the two in step.
- The units differ head by head, as the table above shows, and the heads also
  disagree on whether the numbers describe the client area or the framed window.
- Do not try to correct for the display scale from application code. The scale is
  not knowable until the `XamlRoot` exists, which is after the native window has
  been created, so any correction is a visible resize of an existing window, and a
  correction that is right for the X11 head is wrong for the Wayland and macOS
  heads.
- Pick the numbers by measuring the layout rather than estimating it. The floor is
  whatever stops fitting first, and a row that scrolls does not set one: Fresco.Brix
  keeps its toolbar in a hidden-scrollbar `ScrollViewer`, so buttons past the
  minimum width scroll into view instead of becoming unreachable.

### Keep the window from shrinking below a minimum

**When you want this.** Your layout stops being usable below some size, and you
want the window manager to refuse to go there rather than letting the user drag
the page into nonsense.

**The MVVM shape.** Two more constants on `App` and one presenter block in
`OnLaunched`, immediately after the `Window` is constructed and before
`Activate()`. The window's default presenter is an `OverlappedPresenter`, and the
minimum and maximum sizes are its properties.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    MainWindow = new Window
    {
        Title = "GitHubIssueFinder"
    };

    //The smallest size the user may drag the window to. The presenter is already in
    //place here: constructing a Window builds its native window straight away once the
    //application has finished initializing, and the window's default presenter is an
    //OverlappedPresenter. Setting the minimum now, before Activate(), means the window
    //manager has the constraint before the window is ever shown; setting it after
    //Activate() also works, but the window has been mapped once by then. No maximum is
    //set, so the window can still be resized up and maximized.
    if (MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
    {
        presenter.PreferredMinimumWidth = MinimumWidth;
        presenter.PreferredMinimumHeight = MinimumHeight;
    }

    // ...

    MainWindow.Activate();
}
```

**Why `AppWindow` compiles here.** `Window.AppWindow` is public only where
`HAS_CODEBRIX_WINUI` is defined, and the platform package's build targets inject
that symbol into every project that references it. Application code therefore
reaches `MainWindow.AppWindow.Presenter` with nothing defined by hand and no
extra using directive, because the presenter type is written fully qualified.

**Why immediately after `new Window()` works.** The `Window` constructor
initializes eagerly once the application has finished starting, and that path
installs the default `OverlappedPresenter`, so the `is OverlappedPresenter` test
succeeds the instant the constructor returns. The properties are safe to set even
where there is no native window yet: the change is pushed to the native window
through a null-conditional call, so it is dropped rather than throwing, and the
whole set of constraints is re-applied when a native window is attached.

**What it looks like when it works on the X11 head.** The head answers the
properties with `XSetWMNormalHints`, so `xprop -id <id> WM_NORMAL_HINTS` reads
`program specified minimum size: <W> by <H>` from the first frame the window is
mapped. Three separate ways of making the window smaller were tried against both
applications, and all three stop at exactly the minimum: `xdotool windowsize`,
`wmctrl -i -r <id> -e`, and dragging the bottom-right corner. Resizing up still
works normally afterwards.

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs`

**Also shown by.**
`CodeBrix.Samples.Gpl3/Fresco.Brix/src/Fresco.Brix.UI/App.xaml.cs` (in the
CodeBrix.Samples.Gpl3 repository: the same block with 900 by 620, placed after
the window is also stored in a static property the page reads),
`InannaRosette/src/InannaRosette.UI/App.xaml.cs`
(the presenter's minimum pinned in `OnLaunched` before `Activate()`, with both
numbers justified in a comment by what the layout does below them - the header
buttons begin to wrap and the diagonal station labels collide with the cards)

**Sharp edges.**
- Leave the maximum alone unless you have a real reason for one. An unset maximum
  is treated as the largest possible value, so maximize and a full-screen drag keep
  working; setting one takes that away.
- Before `Activate()` is better than after. Setting the properties later also
  works, but the window has been mapped once without the constraint by then.
- An unset minimum is written as zero rather than skipped, so the hints always
  exist. That means an application that never touches the presenter still gets
  hints, and reading `WM_NORMAL_HINTS` on such a window shows a minimum of 0 by 0
  rather than nothing.
- The numbers are in the same head-dependent units as the launch size, so read the
  table in [Set the window's launch size](#set-the-windows-launch-size) before
  choosing them.
- Every launch on the X11 head logs two error-level lines from the presenter about
  `_NET_WM_STATE` not existing on the window. They are harmless and are not caused
  by anything the application does: they appear whether or not the application ever
  touches the presenter, because the property does not exist yet on a window that
  has not been mapped.

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
// ... using CodeBrix.Platform.Simple;
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
    /// <summary>
    /// Registers the Poly Haven API client, the model loader, the catalog service, the
    /// download service and the document backdrop service.
    /// </summary>
    public static IServiceCollection AddPolyHavenBrowser(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }

        services.AddPolyHavenApiClient(options =>
        {
            //Poly Haven asks API consumers to identify themselves.
            options.UserAgent = "PolyHavenBrowser/1.0 (CodeBrix.Platform sample; +https://polyhaven.com)";
        });

        //The view model asks for the interface, so the loading technology can be swapped or
        //mocked without touching it. The loader holds no state between calls.
        services.AddSingleton<IModelLoader, GltfModelLoader>();

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
library boundary to respect),
`InannaRosette/src/libs/InannaRosette.Reading/RegisterServices.cs`
(`AddReading()` registers two of the three with
`TryAddSingleton<TInterface, TImplementation>` and the third through a factory
lambda, because that one's only constructor parameter is optional and the
container cannot supply it)

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
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`,
`InannaRosette/src/InannaRosette.UI/App.xaml.cs`
(the whole body of `InitializeLogging()` inside `#if DEBUG`, with filters that
quiet the platform's own categories down to warnings, and a comment saying it is
called from each head's `Main` before the host is built)

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
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml`,
`InannaRosette/src/InannaRosette.UI/App.xaml.cs` and `App.xaml`
(the default-font half only - no fallback families - with the regular and bold
faces also published as `FontFamily` resources, which every text style and every
hand-built `TextBlock` in the drawn scene names)

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
// ... startFolder and homeFolder are computed from the environment just above
var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxFrameBuffer(fb => fb
        .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
        .AutoRotationEnabled(true)
        .EnableFolderPicker(new FolderPickerOptions {
           AllowNewFolderCreate = true,
           //ShowHiddenFolders = true,
           StartFolder = startFolder,
           RestrictToFolder = homeFolder,
        })
        //The FrameBuffer head has no OS chrome, so the "Save PDF as…" picker the
        //  Document button pops is opt-in
        .EnableFileSavePicker(new FilePickerOptions {
           AllowNewFolderCreate = true,
           StartFolder = startFolder,
           RestrictToFolder = homeFolder,
           RequiredExtension = ".pdf",
        })
        .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
            ShowDismissKey = true,  //default behavior = true
            //ShowDismissKey = false,
            KeyHeight = SoftwareKeyHeight.PortraitFullLandscapeFull,  //default behavior = FullHeight
            //KeyHeight = SoftwareKeyHeight.PortraitHalfLandscapeHalf,
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
(software keyboard only, at `SoftwareKeyHeight.PortraitHalfLandscapeHalf` so the
keyboard leaves more of the page visible),
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
- `StartFolder` and `RestrictToFolder` are ordinary strings the head passes in, and
  every application in this repository computes them from the environment rather
  than writing a path in, so the head runs on any machine; `RestrictToFolder`
  fences the picker so the user cannot navigate above it. See
  [Compute the framebuffer picker's folders from the environment](BLUEPRINTS-AppStructureAndStartup.md#compute-the-framebuffer-pickers-folders-from-the-environment).
- `RequiredExtension` on the picker and the application's own expectation about
  the file it will write have to agree.
- `ShowDismissKey` defaults to true and `KeyHeight` defaults to full height; the
  samples record both defaults in comments next to the overrides, and leave the
  alternative they tried commented out beside them, because which key height suits
  an application is a thing you settle by looking at it on the device.
- The key-height values name both orientations
  (`PortraitFullLandscapeFull`, `PortraitHalfLandscapeHalf`), so a head that
  enables auto-rotation chooses once for both ways up.
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
  <Compile Include="..\Shared\Drawing\DrawingCanvasBinder.cs" Link="Drawing\DrawingCanvasBinder.cs" />
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

### Give a hosted guest program the environment it assumes with a bootstrap script

**When you want this.** The program you are hosting was written to run under a
standard launcher, and it assumes things a hosted engine does not provide:
packages that are present, global variables the launcher filled in from the
command line, and built-in commands that behave a particular way. You want those
assumptions satisfied without editing a single line of the guest.

**The MVVM shape.** Not a view-model concern. One glue script, authored by the
application and sourced immediately before the guest's first line, holds every
adaptation. Each block says why it exists and quotes the stock mechanism it stands
in for, so the file doubles as the complete list of what hosting cost. The
application's C# does the parts that have to be managed code; the glue script does
the parts that are cheaper to express in the guest's own language.

**Code.**

```tcl
# From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.Core/Assets/bootstrap.tcl
# =============================================================================
# DRAKON.Brix bootstrap
#
# This is a NEW file, authored for the DRAKON.Brix port. It is NOT a modified
# copy of any DRAKON-Editor file (stock DRAKON has no bootstrap.tcl). It is the
# ONLY Tcl that runs before the UNMODIFIED drakon_editor.tcl is sourced, and it
# exists purely to reproduce the environment that stock DRAKON assumes so that
# drakon_editor.tcl's own (unchanged) startup code succeeds.
#
# Everything below is glue that was ADDED for this port. Each block is tagged
#   "Added for DRAKON.Brix - because <reason>"
# and shows, as a "Stock DRAKON / Tcl:" reference comment, the mechanism it
# stands in for. The invariant marker text "for DRAKON.Brix" is searchable:
# grep it (and its sibling "Removed for DRAKON.Brix", used when we ever have to
# edit a genuinely-vendored original file) to find every port-specific change.
#
# Already handled in C# before this file runs, so nothing is needed here for
# them:
#   * Tk / Img packages + ::tcl_version / ::tk_version / ::tk_patchLevel
#       -> CodeBrix.Platform.TkCanvas.TkBootstrap.Register
#   * sqlite3 and pdf4tcl commands + "package provide"
#       -> CodeBrix.Platform.TclTk.Extras.TclTkExtras.RegisterAll
# =============================================================================
```

Three kinds of adaptation show up, and they are the three to expect. Some packages
only have to be present, because their only real consumer has been replaced by a
managed shim, so declaring them satisfies the guest's gate and keeps a large
dependency out of the application entirely:

```tcl
# From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.Core/Assets/bootstrap.tcl
# Added for DRAKON.Brix - because stock DRAKON's [package require snit] loads
#   the ~3000-line Tcl "snit" object package from tcllib. snit's ONLY consumer
#   was pdf4tcl, whose implementation is now the managed CodeBrix Extras shim,
#   so the package only has to be PRESENT to satisfy drakon_editor.tcl's line-1
#   [require snit] gate — never actually used. An empty provide is enough, and
#   it keeps tcllib out of the app.
#
# Stock DRAKON / Tcl:
#   package require snit          ;# pulls the real snit implementation from tcllib
package provide snit 2.3.2
```

Some are host facts rather than packages. The globals a command-line launcher
would have set are synthesized, which is also how the application can start with a
document already open:

```tcl
# From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.Core/Assets/bootstrap.tcl
# Added for DRAKON.Brix - because stock DRAKON is launched as
#   "tclsh8.6 drakon_editor.tcl ?FILE?", so tclsh populates ::argc / ::argv /
#   ::argv0 from the process command line, and drakon_editor.tcl's
#   [start_up $argc $argv] opens FILE (or shows the intro when there is none).
#   The port has no command line, so the host passes the file to open via the
#   DRAKONBRIX_OPEN environment variable and we build the same globals here.
#
# Stock DRAKON / Tcl:
#   (::argc / ::argv / ::argv0 are set automatically by tclsh from argv;
#    no code in drakon_editor.tcl sets them.)
if { [info exists ::env(DRAKONBRIX_OPEN)] && $::env(DRAKONBRIX_OPEN) ne "" } {
    set ::argc 1
    set ::argv [list $::env(DRAKONBRIX_OPEN)]
} else {
    set ::argc 0
    set ::argv {}
}
set ::argv0 drakon_editor.tcl
```

And some have to be genuinely re-implemented. The message catalog is the one done
properly here - the locale preference list, the catalog loading, the
fall-back-to-source-string lookup - because the guest ships real translations and
an English-only stub would have thrown them away.

**Where to look.**
`DRAKON.Brix/src/DRAKON.Brix.Core/Assets/bootstrap.tcl`
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs` (the two-step
sourcing at the end of the boot sequence)

**Sharp edges.**
- Sourcing the glue and sourcing the guest are two steps and the order is
  load-bearing. The glue has to have finished before the guest's first line runs.
- Write the reason into every block, and quote the stock mechanism the block
  replaces. Six months later the reason is the only thing that tells you whether a
  block can be deleted.
- Use one searchable marker phrase for every port-specific change, with a sibling
  marker for the rare case where a genuinely vendored file had to be edited. That
  pair of phrases is how you find the whole diff against the original program.
- Decide per package whether presence is enough or the behavior is really needed.
  A stub where behavior mattered is a silent loss of function; a real
  implementation where presence was enough is a dependency you did not have to
  take on.

### Register hardware implementations from each head and ask a finder for the best one

**When you want this.** The application drives a device that may or may not be
attached, and the shared code - the pages, the view models, the Core library -
must never name the implementation, so that one build runs identically on a
machine with the hardware and on a machine without it.
[Start each head from a Program Main and pick the platform backend](BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend)
covers what every head's entry point must contain; this recipe is about the one
other decision a head is allowed to make, and where the result of it is asked
for.

**The MVVM shape.** A UI-free contract library declares the device interface,
the model types and a static finder. The real implementation lives in its own
library; the simulator lives beside the contract. Each head's `Program.Main`
registers both before the host is built, because a head is the only project that
is allowed to reference an implementation. The view model asks the finder for
the best one and works against the interface only.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.LinuxX11/Program.cs
App.InitializeLogging();

//This head decides which scope implementations the application can use.
//  Registration order does not matter: FindBest() prefers real hardware
//  and falls back to the simulator when nothing is plugged in.
ScopeDeviceFinder.Register(new Ps2000ScopeDataDevice());
ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());

var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxX11()
    .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
    .Build();

host.Run();
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/ScopeDeviceFinder.cs
public static IScopeDataDevice FindBest(bool allowSimulatedFallback = true)
{
    IScopeDataDevice[] candidates;
    lock (SyncRoot)
    {
        candidates = Registered.ToArray();
    }

    //Real hardware first: a physical device beats a simulation whenever one
    //  is actually there.
    foreach (IScopeDataDevice scope in candidates.Where(s => !s.IsSimulated))
    {
        if (scope.IsOpen) { return scope; }

        try
        {
            if (scope.OpenScope()) { return scope; }
        }
        catch (PicoScopeException)
        {
            //A present-but-unusable device should not stop us falling back
            //  to the simulator, so swallow and keep looking.
        }
    }

    if (!allowSimulatedFallback) { return null; }

    foreach (IScopeDataDevice scope in candidates.Where(s => s.IsSimulated))
    {
        if (scope.IsOpen) { return scope; }
        if (scope.OpenScope()) { return scope; }
    }

    return null;
}
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
public async Task InitializeAsync()
{
    try
    {
        await StartUpAsync().ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        _log.LogError(ex, "Scope startup failed.");
        SetStatus("Could not start: " + ex.Message);
    }
}

private async Task StartUpAsync()
{
    SetStatus("Looking for a scope...");

    //Opening a real device takes a second or two of USB traffic, which the
    //  UI thread should not sit through.
    _scope = await Task.Run(() => ScopeDeviceFinder.FindBest()).ConfigureAwait(false);

    if (_scope == null)
    {
        SetStatus("No scope available -- nothing registered with ScopeDeviceFinder.");
        return;
    }

    IsSimulated = _scope.IsSimulated;
    DeviceText = _scope.UnitInfo.ToString();
    // ...
}
```

What keeps the rule true is written into the project files: the contract library
has no package references at all, and the Core library says out loud what it is
not allowed to reference.

```xml
<!-- From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj -->
<!-- The device-agnostic scope contract. The real device (PicoScope.Brix.ScopeData.Ps2000)
     is referenced by the heads, which register it; this project never sees it. -->
<ItemGroup>
  <ProjectReference Include="..\libs\PicoScope.Brix.ScopeData\PicoScope.Brix.ScopeData.csproj" />
</ItemGroup>
```

**Where to look.**
`PicoScope.Brix/src/PicoScope.Brix.LinuxX11/Program.cs` and the five sibling
head projects under `PicoScope.Brix/src/`
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/ScopeDeviceFinder.cs` and
`IScopeDataDevice.cs`
`PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Registration order is deliberately not a rule. The finder decides, so a head
  that registers the simulator first behaves the same as one that registers it
  last; if you let order decide as well, you have two rules that can disagree.
- A real implementation that fails to open, or throws the library's own
  exception type while opening, is skipped rather than treated as an error.
  "Nothing plugged in" is an ordinary condition, not a failure.
- The finder is process-global state. Tests must reset it around every case;
  the sample's finder test class resets in its constructor and again in
  `Dispose()`.
- Open the device off the UI thread. A real instrument takes seconds of USB
  traffic at startup, which is why the call is wrapped in `Task.Run`.
- The page starts this from an `async void` load handler, so the public entry
  point is a guard: `InitializeAsync` is a try/catch around the private
  `StartUpAsync` that does the work, and every failure an instrument can raise
  becomes a line of status text instead of an unhandled exception.
- Registering the simulator in the head, alongside the real device, is what makes
  it an equal citizen rather than a fallback bolted on afterwards - and it is
  what lets a head ship without the interop library at all.

### Compute the framebuffer picker's folders from the environment

**When you want this.** Your framebuffer head opts into a picker, and the picker
needs a folder to open in and a folder it may not climb above. Writing those two
paths into `Program.cs` is how a head stops running anywhere but the machine it
was built on.
[Enable a picker and the software keyboard on the Linux framebuffer head](BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head)
is the opt-in itself; this is where the two paths it takes come from.

**The MVVM shape.** Head configuration only, computed before the host builder
runs. `Environment.GetFolderPath` answers for the signed-in user, a `Path.Combine`
narrows it to a subfolder when that subfolder exists, and the fallback when it does
not is the parent rather than a failure. Nothing about the view model changes:
it still calls its picker bridge and still copes when there is no picker at all.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxFrameBuffer/Program.cs
//Where the folder picker opens, and how far up it lets the user browse: the signed-in
//user's home folder, and its "Assets" subfolder when there is one. Computed here rather
//than written in, so this head carries no path from the machine it was built on.
var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var assetsFolder = Path.Combine(homeFolder, "Assets");
if (!Directory.Exists(assetsFolder)) { assetsFolder = homeFolder; }

// ...

        .EnableFolderPicker(new FolderPickerOptions {
           AllowNewFolderCreate = false,
           StartFolder = assetsFolder,
           RestrictToFolder = homeFolder,
        })
```

Where an application would rather start in a well-known folder than in one of its
own, ask for that folder by name and fall back to the home folder when the
platform has none:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs
//The framebuffer picker draws its own file list, so it needs a folder to start in and a
//  tree to stay inside: this user's documents folder, inside their home directory
var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
if (string.IsNullOrEmpty(homeFolder)) { homeFolder = Environment.CurrentDirectory; }
var startFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
if (string.IsNullOrEmpty(startFolder) || !Directory.Exists(startFolder)) { startFolder = homeFolder; }
```

A head that needs the answer in more than one place puts it in a small method
rather than in a variable:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.LinuxFrameBuffer/Program.cs
//The picker is restricted to one folder, and that folder is worked out at run time rather
//  than written into the source, so this head behaves the same on every device it reaches.
private static string GetPickerRootFolder()
{
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    return string.IsNullOrWhiteSpace(home) ? Directory.GetCurrentDirectory() : home;
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxFrameBuffer/Program.cs`
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.LinuxFrameBuffer/Program.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer/Program.cs` (a `Temp`
subfolder to start in, the home folder as the fence, and the same
does-it-exist fallback)

**Sharp edges.**
- `GetFolderPath` can return an empty string on a machine with no such folder, and
  an empty `RestrictToFolder` fences the picker into nothing. PdfSideBySide and
  NotionDocumentCreator guard for it explicitly; a head that does not is relying on
  a home folder always being there, which is true on a device you control and not
  in general.
- A subfolder that does not exist is not an error: fall back to its parent rather
  than creating a folder the user did not ask for at startup.
- `RestrictToFolder` is a fence, not a preference. Compute the start folder inside
  it, or the picker opens somewhere it will not let the user return to.
- An appliance is the case that inverts this. Where a device really does keep its
  content in one fixed place, that path belongs in the head - but write it as a
  named constant with a comment, not as a string in the middle of a builder call.

### Register a factory and let the view model ask it for what it owns

**When you want this.** A view model has to end up owning an object that takes
real work to build - a rendering session bound to a page's canvas, a comparison
that owns a rasterizer - and `new` in the constructor makes it untestable and
makes the library it comes from non-substitutable.
[Register library services with one AddXxx extension method](BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method)
registers the services themselves; this is for the things a service cannot be,
because each caller needs its own instance and must dispose it.

**The MVVM shape.** The library declares a factory interface and one
implementation, and its `AddXxx` extension registers the factory rather than the
product. The view model resolves the factory once and asks it for an instance at
the moment it has what the instance needs - which, for anything bound to a canvas,
is not constructor time. The resolve falls back to the concrete factory with
`?? new`, so the view model also runs in a test with no container at all.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/IVisualizerSessionFactory.cs
/// <summary>
/// Builds the Visualize Mode scene for a page's game canvas. Registered with the
/// dependency-injection container at startup and resolved by the view model, which then owns
/// the session it is handed without knowing how one is made.
/// </summary>
public interface IVisualizerSessionFactory
{
    /// <summary>
    /// Creates the visualizer session that renders into the host's canvas. Call at the
    /// canvas's first real layout size; the session is not started.
    /// </summary>
    /// <param name="host">The page that owns the game canvas.</param>
    /// <returns>The session, ready to be started.</returns>
    IVisualizerSession CreateSession(IGameCanvasHost host);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/RegisterServices.cs
public static IServiceCollection AddPalmVisualizer(this IServiceCollection services)
{
    if (services == null) { throw new ArgumentNullException(nameof(services)); }

    services.AddSingleton<IWebcamCaptureService, WebcamCaptureService>();
    services.AddSingleton<IPalmTracker, PalmTracker>();
    services.AddSingleton<IVisualizerSessionFactory, VisualizerSessionFactory>();

    return services;
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
//The three collaborators come from the container App registered them with, so this
//  view model can also be built against stand-ins that need no camera and no GPU
_captureService = GetService<IWebcamCaptureService>() ?? new WebcamCaptureService();
_tracker = GetService<IPalmTracker>() ?? new PalmTracker();
_sessionFactory = GetService<IVisualizerSessionFactory>() ?? new VisualizerSessionFactory();

// ... later, when the page's canvas has a real size:

public void CanvasFirstStart(IGameCanvasHost host)
{
    //UI thread, the first time Visualize Mode is shown with a real size: build the
    //  shader scene and start the engine. Later mode switches pause and resume it.
    _visualizerSession = _sessionFactory.CreateSession(host);
    _visualizerSession.Start();
}
```

Where the product owns disposable state of its own, register it transient and say
so, because the holder is what disposes it:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/RegisterServices.cs
/// <summary>
/// Registers everything a screen needs to compare two PDF documents: the
/// <see cref="IPdfComparisonFactory"/> that makes a comparison, and the
/// <see cref="IPageRenderer"/> that rasterizes its pages. The renderer is transient because
/// each one owns a rasterizer and a page cache that its holder disposes.
/// </summary>
/// <param name="services">The service collection to register into.</param>
/// <returns>The service collection, for chaining.</returns>
public static IServiceCollection AddPdfRender(this IServiceCollection services)
{
    ArgumentNullException.ThrowIfNull(services);

    services.TryAddSingleton<IPdfComparisonFactory, PdfComparisonFactory>();
    services.TryAddTransient<IPageRenderer>(_ => new PageRenderer());

    return services;
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/IVisualizerSessionFactory.cs` and
`VisualizerSessionFactory.cs`
`PalmVisualizer/src/PalmVisualizer.Core/RegisterServices.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/RegisterServices.cs` and
`IPdfComparisonFactory.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Register the factory, not the product, whenever the product needs an argument
  the container cannot know - a canvas host, a page size, a file the user chose.
- `?? new` after the resolve is what keeps the view model constructible in a test
  with no container. It also means a missing registration fails as ordinary
  behavior rather than as a null reference at first use.
- Transient and singleton are a statement about ownership. A renderer that owns a
  cache is transient because its holder disposes it; a stateless loader is a
  singleton.
- Ask the factory at the moment the inputs exist. For anything tied to a canvas
  that is the first real layout, which is why the call sits in the page-driven
  method and not in the constructor.
- `TryAdd` rather than `Add` in a library extension lets an application register
  its own implementation first and keeps a second call to the extension harmless.
