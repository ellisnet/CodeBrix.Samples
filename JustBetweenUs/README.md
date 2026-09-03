# JustBetweenUs

JustBetweenUs is a small text-encryption utility. You type or paste a message,
pick an algorithm from a dropdown (AES, Triple DES or Twofish), supply a short
text key, and press Encrypt to turn the message into a Base64 string or Decrypt
to turn a Base64 string back into readable text. A default key ships inside the
application as an embedded resource and is loaded at startup, so the application
is usable the moment it opens. A Copy to Clipboard button puts the processed
text on the system clipboard, and an information button (an animated star on the
heads that can draw one) opens a dialog describing the operating system, the
.NET version, the user and the processor architecture the application is running
on. The encryption is real, but this is a demonstration, not a security-audited
product. The application is adapted from a sample provided by Paul Ainsworth.

This folder is the reference for the "one view model, many heads" shape. A
single `MainViewModel.cs` is compiled into nine separate application heads: the
six CodeBrix.Platform Skia desktop heads, plus a native WinUI 3 head, a native
WPF head and a .NET MAUI head that also runs on phones and tablets. It is the
only application in the repository that also runs on mobile, and the one to read
when you want to see how far one view model, one service library and one set of
image assets can be pushed across completely different UI stacks.

## What this sample shows a CodeBrix.Platform developer

- Compiling one `SimpleViewModel` into nine heads across four UI stacks by
  linking the same source file into each project: [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads).
- Six almost identical `Program.cs` files, each selecting exactly one platform
  backend and running the same shared `App`: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Keeping every framework and add-in package in one `net10.0` Core library so
  each head adds only its own runtime package: [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Sharing `App.xaml` and the page across the six Skia heads through a shared
  project that every head imports: [Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project).
- Doing the whole application bootstrap in the `App` constructor, in the right
  order, before any view is constructed: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- Handing `SimpleServiceResolver` a host builder through a tiny shared
  `IHostBuilderProvider` that every head links in: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- Letting the encryption library register itself with a single `AddEncryption()`
  extension method so the application calls one line per library: [Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method).
- The smallest correct `OnLaunched` override that creates the window, installs a
  navigation frame and navigates to the first page: [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).
- Bound properties written with `SetProperty(ref field, value)` and lazily
  created `SimpleCommand` instances whose buttons enable themselves: [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Guarding the whole view-model constructor with `IsDesignMode(true)` so the
  designer never runs the real startup work: [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Fetching the default key from the service asynchronously without blocking
  construction of the page: [Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor).
- Wrapping every assignment to a bound property from a background thread in
  `InvokeOnMainThread`, because Linux and macOS are strict about it: [Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread).
- Raising informational and error dialogs from a command with the view model's
  own `ShowInfo` and `ShowError` helpers: [Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs).
- Giving the view model a dialog anchor as a getter lambda supplied by the page,
  so it never captures a stale reference: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- A one-property bridge interface that four different clipboard APIs satisfy,
  with a graceful message on a head that supplies none: [Copy text to the clipboard from a command through a bridge interface](../BLUEPRINTS-PlatformServices.md#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface).
- Releasing the service reference, the commands and the head-supplied delegate in
  the view model's `Dispose` override: [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).
- Driving the algorithm dropdown from an enum with friendly labels through
  `SimpleEnumInfo<TEnum>` and `[SimpleEnum<T>]`: [Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels).
- Building the information dialog's text from `SimpleOsInfo.GatherInfo()` with no
  head-specific code at all: [Report the host operating system from the view model](../BLUEPRINTS-MVVM.md#report-the-host-operating-system-from-the-view-model).
- Declaring the shared Skia page, its XML namespaces and its `{d:Binding}` markup
  extension bindings: [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).
- An `Image` subclass that resolves an `embedded://Assembly/Resource.Name` URI and
  picks an SVG or bitmap source from the file extension: [Load an SVG or bitmap from an embedded resource with a custom URI scheme](../BLUEPRINTS-ViewsAndControls.md#load-an-svg-or-bitmap-from-an-embedded-resource-with-a-custom-uri-scheme).
- A `Button` subclass that composes an embedded icon and a caption, with the icon
  above, below, left or right of the text: [Build a button that combines an embedded image with text](../BLUEPRINTS-ViewsAndControls.md#build-a-button-that-combines-an-embedded-image-with-text).
- Embedding the same asset files into two assemblies, once by link path and once
  by an explicit logical name: [Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection).
- Rendering the same SVG icons through the same Skia SVG engine on the Skia heads
  and on native WinUI: [Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-svg-art-with-the-codebrix-skiasvg-library).
- Playing the same Lottie JSON on a Skia head and on native WinUI, with the two
  XML namespace forms side by side: [Play a Lottie animation on a Skia head and on native WinUI](../BLUEPRINTS-GraphicsAndRendering.md#play-a-lottie-animation-on-a-skia-head-and-on-native-winui).
- Setting a bundled Open Sans as the default text font for the whole application
  and exposing the same face under a resource key: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Putting the real functionality in a UI-free `net10.0` library behind one
  interface that the view model resolves: [Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface).
- Implementing AES, Twofish and Triple DES inside that service, with the random
  material carried alongside the ciphertext: [Encrypt text with the CodeBrix Cryptography library](../BLUEPRINTS-DocumentsAndData.md#encrypt-text-with-the-codebrix-cryptography-library).
- Reading the default key from an embedded text resource once and caching it,
  with the resource name derived from a type in the assembly: [Read an embedded default value at run time](../BLUEPRINTS-DocumentsAndData.md#read-an-embedded-default-value-at-run-time).
- Stripping non-Base64 characters in the service so an invisible control
  character riding in on a paste cannot break decryption: [Guard Base64 input against invisible clipboard characters](../BLUEPRINTS-DocumentsAndData.md#guard-base64-input-against-invisible-clipboard-characters).
- Setting the Core library's `RootNamespace` to the application namespace so the
  linked source, the XAML and the embedded resource names all agree: [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).
- Shipping one solution per operating system so every solution opens and builds
  everything it contains: [Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere).
- Declaring the architectures a WinUI head supports and mapping `Any CPU` onto a
  real one in the solution: [Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares).
- Switching the WinWpfSkia head to the software render surface between `Build()`
  and `Run()` so its window actually composites: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- Turning on the on-screen keyboard and the direct Skia canvas mode in the
  LinuxFrameBuffer head, with no change to the shared UI: [Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head).
- One static `InitializeLogging()` on `App`, entirely inside `#if DEBUG`, called
  from every head's `Main` before the host is built: [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- An xUnit v3 test project for the encryption library, with SilverAssertions and
  the shared fixture linked in as source: [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library).
- Resolving the service under test from a container built the same way the
  application builds its own: [Test a service the way the container builds it](../BLUEPRINTS-Testing.md#test-a-service-the-way-the-container-builds-it).
- Routing the `ILogger<EncryptionService>` output of the code under test into the
  test report, with a console fallback on Linux: [Route logging from the code under test into test output](../BLUEPRINTS-Testing.md#route-logging-from-the-code-under-test-into-test-output).
- Pinning a head-specific clipboard defect with a regression test that reproduces
  the corrupted input instead of the environment: [Pin a fixed bug with a regression test that says why it is shaped that way](../BLUEPRINTS-Testing.md#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way).

## Building, running and testing

There is no plain `JustBetweenUs.sln`. Three solutions sit beside this file, one
per operating system, each containing only the projects that can build there.

| Solution | Open on | Contains |
| --- | --- | --- |
| `JustBetweenUs.Windows.sln` | Windows | Everything: all six Skia heads including WinWpfSkia, the shared UI shared-project, Core, the WinUI 3 head, the WPF head, the MAUI head, the encryption library and its test project |
| `JustBetweenUs.Linux.sln` | Linux | The Skia heads except WinWpfSkia, the shared UI shared-project, Core, the encryption library and its test project. No native heads |
| `JustBetweenUs.MacOS.sln` | macOS | The same set as the Linux solution, plus the MAUI head |

Only WinWpfSkia is excluded from the non-Windows solutions. Win32Skia targets
plain `net10.0`, so it restores and builds anywhere even though it only runs on
Windows; WinWpfSkia targets `net10.0-windows` and therefore cannot.

The CodeBrix.Platform Skia heads, all under `CodeBrixPlatform/`:

| Project | Platform |
| --- | --- |
| `JustBetweenUs.Win32Skia` | Windows, native Win32 window |
| `JustBetweenUs.WinWpfSkia` | Windows, Skia hosted in a WPF window |
| `JustBetweenUs.LinuxX11` | Linux desktop, X11 |
| `JustBetweenUs.LinuxWayland` | Linux desktop, native Wayland |
| `JustBetweenUs.LinuxFrameBuffer` | Linux framebuffer, no display server |
| `JustBetweenUs.MacOS` | macOS |

The native (non-Skia) heads:

| Project | Platform |
| --- | --- |
| `JustBetweenUs.WinUI` | Windows, WinUI 3 on the Windows App SDK |
| `JustBetweenUs.Wpf` | Windows, WPF |
| `Mobile` (`JustBetweenUs.Mobile`) | .NET MAUI: Android, iOS, Mac Catalyst, and Windows when the build itself runs on Windows |

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0` or a `net10.0-*` platform
  framework.
- No accounts, tokens, network access, downloaded data or special hardware. The
  default key is embedded in the encryption library and every image asset is in
  this folder.
- The MAUI head needs the .NET MAUI workloads installed. Its Windows target
  framework is added only when the build runs on Windows, and it builds
  unpackaged. A Tizen target is present but commented out.
- The WinUI head needs Windows, the Windows App SDK and, for the packaged
  profile, MSIX tooling. Two launch profiles are defined, packaged and
  unpackaged, so you do not have to package the application to run it.
- LinuxWayland requires a running Wayland compositor and never falls back to X11
  or XWayland. Started from an X11 session it prints a message saying a Wayland
  compositor is required and exits with a non-zero code; use LinuxX11 for X11 and
  XWayland sessions. LinuxFrameBuffer is for embedded and kiosk systems with no
  display server.
- On Linux ARM64 the native Skia library may need FreeType preloaded before the
  application starts. Prefix the run command with
  `LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6`. The long comment at
  the top of `CodeBrixPlatform/JustBetweenUs.LinuxX11/Program.cs` explains why,
  and records a second Raspberry Pi finding about borderless windows under the
  labwc compositor.

Run one head from the command line, for example the X11 head:

```text
dotnet run --project JustBetweenUs/CodeBrixPlatform/JustBetweenUs.LinuxX11/JustBetweenUs.LinuxX11.csproj
```

Tests live in `tests/JustBetweenUs.Encryption.Tests` and cover the encryption
service only: that it resolves from the container, that the embedded default key
is read, that Triple DES encrypts to and decrypts from a known Base64 string,
that AES and Twofish round-trip, that AES decrypts a known Base64 string, and a
regression test for a stray control character arriving on a clipboard paste. The
tests need no network, no GPU and no files on disk.

`global.json` beside this file selects the Microsoft.Testing.Platform runner. With
that runner selected, a plain `dotnet test` against the solution or the project
can report that zero tests ran, depending on the SDK. The way that always works is
to build the test project and run the built test executable directly:

```text
dotnet build JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/JustBetweenUs.Encryption.Tests.csproj -c Debug
JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/bin/Debug/net10.0/JustBetweenUs.Encryption.Tests
```

Build and run the tests in Debug. The test project defines `SIMPLE_OUTPUT_LOGGING`
only for the `Debug|AnyCPU` configuration, and the xUnit-output logging types the
test class uses unconditionally exist only inside that conditional in
`Shared/Testing/SimpleTestFixture.cs`.

## How the projects and folders are organized

```text
JustBetweenUs/
  JustBetweenUs.Windows.sln           All nine heads plus the library and tests; open on Windows
  JustBetweenUs.Linux.sln             Skia heads except WinWpfSkia, library and tests; open on Linux
  JustBetweenUs.MacOS.sln             The Linux set plus the MAUI head; open on macOS
  global.json                         Selects the Microsoft.Testing.Platform test runner
  README.md                           This file
  THIRD-PARTY-NOTICES.txt             Third-party attribution for the application
  CodeBrixPlatform/                   Everything that uses the Skia XAML framework
    JustBetweenUs.UI/                 Shared project (.shproj/.projitems): App.xaml(.cs), Views/MainPage.xaml(.cs)
    JustBetweenUs.Core/               Class library: all platform packages, the linked shared source,
                                        the embedded assets, and Controls/
    JustBetweenUs.Win32Skia/          Head: Program.cs plus one runtime package
    JustBetweenUs.WinWpfSkia/         Head: Program.cs plus one runtime package; forces software rendering
    JustBetweenUs.LinuxX11/           Head: Program.cs plus one runtime package; carries the ARM64 and
                                        window-decoration notes
    JustBetweenUs.LinuxWayland/       Head: Program.cs plus one runtime package; no X11 fallback
    JustBetweenUs.LinuxFrameBuffer/   Head: Program.cs plus one runtime package; enables the software keyboard
    JustBetweenUs.MacOS/              Head: Program.cs plus one runtime package
  JustBetweenUs.WinUI/                Native WinUI 3 head: its own App.xaml(.cs), Views/MainPage.xaml(.cs),
                                        Assets/, app.manifest and Package.appxmanifest
  JustBetweenUs.Wpf/                  Native WPF head: its own App.xaml(.cs), Views/MainWindow.xaml(.cs)
  Mobile/                             .NET MAUI head: MauiProgram.cs, App/AppShell, Views/MainPage,
                                        Platforms/, Resources/
  Shared/                             Source and assets file-linked into other projects; not a project itself
    ViewModels/                       MainViewModel.cs (all behavior) and EncryptionMode.cs (the enum picker)
    Helpers/                          HostHelper.cs (IHostBuilderProvider) and EmbeddedResourceHelper.cs
    Testing/                          SimpleTestFixture.cs, a container-backed xUnit fixture base
    Assets/                           The SVG icons and the Lottie star animation JSON
  JustBetweenUs.Encryption/           The business library: IEncryptionService, EncryptionService,
                                        AddEncryption(), and the embedded default key
  tests/
    JustBetweenUs.Encryption.Tests/   xUnit v3 tests for the encryption service
```

The dependency direction runs one way. `JustBetweenUs.Encryption` is the bottom:
it references no UI framework at all, only the cryptography library and the
Microsoft.Extensions hosting and logging abstractions. Every head that needs
encryption ends up on it, the six Skia heads transitively through Core and the
WinUI, WPF and MAUI heads by a direct project reference. Above that,
`JustBetweenUs.Core` carries all the platform package references for the Skia
heads and holds the two custom controls, and each of the six Skia heads
project-references Core and adds exactly one runtime package. Nothing references
`Shared/` as a project, because it is not one. `MainViewModel.cs`,
`EncryptionMode.cs` and `HostHelper.cs` are pulled in as linked `<Compile>` items
by Core (for the Skia heads) and directly by the WinUI, WPF and MAUI heads;
`EmbeddedResourceHelper.cs` is linked only into the encryption library, and
`SimpleTestFixture.cs` only into the test project. The XAML in
`JustBetweenUs.UI` is shared through an imported `.projitems`, so each Skia head
compiles its own copy of `App` and `MainPage`. The files in `Shared/Assets` are
linked as embedded resources into Core and, with explicit logical names, into the
WinUI head; the WPF and MAUI heads use none of them.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The Skia XAML framework and the Simple MVVM toolkit: `SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `SimpleEnumInfo`, `SimpleOsInfo`, `IHostBuilderProvider`, `IXamlRootGetter`, the dialog helpers and `CodeBrixPlatformHostBuilder` | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj`, `CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`, `Shared/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform Skia runtime, one per head | The Win32, WPF-hosted, X11, Wayland, framebuffer and macOS backends. Each head csproj adds exactly one and nothing else | the six csproj files under `CodeBrixPlatform/JustBetweenUs.*/` |
| CodeBrix.Platform Graphics2DSK add-in | 2D SkiaSharp drawing integration, referenced by Core so the drawing stack is present. No application code calls it directly | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj` |
| CodeBrix.Platform Lottie add-in | Supplies `AnimatedVisualPlayer` and `LottieVisualSource` for the animated star button on the Skia heads | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj`, `CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml` |
| CodeBrix.Platform Svg add-in | Supplies `SvgImageSource`, which `EmbeddedImage` uses for any resource whose name ends in `.svg` | `CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs` |
| CodeBrix.Platform SkiaSharp Views | SkiaSharp view and canvas integration used by the Lottie and SVG paths | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj` |
| CodeBrix.Platform Fonts.OpenSans | Bundles Open Sans and exposes it at an `ms-appx:` path; the application sets it as the default text font and as a `FontFamily` resource | `CodeBrixPlatform/JustBetweenUs.UI/App.xaml`, `CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs` |
| CodeBrix.SkiaSvg | The Skia SVG parsing and rendering engine underneath `SvgImageSource`, referenced by Core and by the WinUI head so both render the same files the same way | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj`, `JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj` |
| CodeBrix.Platform.WinUI | The Simple MVVM toolkit for native WinUI 3, so the same `MainViewModel` compiles and binds there | `JustBetweenUs.WinUI/App.xaml.cs`, `JustBetweenUs.WinUI/Views/MainPage.xaml.cs` |
| CodeBrix.Platform.WinUI.Skia | Supplies the `EmbeddedImageButton` the WinUI page uses, with the same `embedded://` scheme and the same SVG rendering | `JustBetweenUs.WinUI/Views/MainPage.xaml` |
| CodeBrix.Platform.WinUI.Lottie | Supplies `AnimatedVisualPlayer` and `LottieVisualSource` for native WinUI | `JustBetweenUs.WinUI/Views/MainPage.xaml` |
| CodeBrix.Platform.WPF | The Simple MVVM toolkit for WPF | `JustBetweenUs.Wpf/App.xaml.cs`, `JustBetweenUs.Wpf/JustBetweenUs.Wpf.csproj` |
| CodeBrix.Platform.Mobile | The Simple MVVM toolkit for .NET MAUI | `Mobile/App.xaml.cs`, `Mobile/JustBetweenUs.Mobile.csproj` |
| CodeBrix.Cryptography | The Twofish engine, the SHA-3 digest, the PKCS#5 v2 parameter generator, PKCS#7 padding and the buffered block cipher behind the Twofish methods | `JustBetweenUs.Encryption/Services/EncryptionService.cs` |
| SilverAssertions | The fluent `Should()` assertions in the test project | `tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp.Skottie | The Lottie playback engine behind the animated star on the Skia heads | `CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()`, which `HostHelper` hands to the service resolver, and the host the test fixture builds | `Shared/Helpers/HostHelper.cs`, `Shared/Testing/SimpleTestFixture.cs` |
| Microsoft.Extensions.Logging (console, debug, abstractions) | Console logging for the Skia heads, debug logging for the MAUI head, and the `ILogger<EncryptionService>` the service takes in its constructor | `CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`, `Mobile/MauiProgram.cs`, `JustBetweenUs.Encryption/Services/EncryptionService.cs` |
| Microsoft.Extensions.DependencyInjection | The `IServiceCollection` the `AddEncryption()` extension and the test fixture container are built on | `JustBetweenUs.Encryption/RegisterServices.cs`, `Shared/Testing/SimpleTestFixture.cs` |
| .NET MAUI (Microsoft.Maui.Controls and Compatibility) | The mobile UI stack for the MAUI head | `Mobile/JustBetweenUs.Mobile.csproj` |
| Windows App SDK and the Windows SDK build tools | The WinUI 3 UI stack and MSIX packaging support | `JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj` |
| xUnit v3 and the Microsoft test SDK | The test framework and host for the encryption tests | `tests/JustBetweenUs.Encryption.Tests/JustBetweenUs.Encryption.Tests.csproj` |

## Worth studying in this application

### One view model, nine heads

`Shared/ViewModels/MainViewModel.cs` is the whole application. It holds the three
bound text properties (`EncryptionKey`, `EnteredText`, `ProcessedText`), the
algorithm selection, four `SimpleCommand` instances, the clipboard bridge
interface, the dialog calls and the `Dispose` override. It derives from
`SimpleViewModel` and references nothing but the Simple toolkit and
`IEncryptionService`. Every head pulls it in as a linked `<Compile>` item and
compiles its own copy: Core does it once on behalf of the six Skia heads, and the
WinUI, WPF and MAUI heads each do it themselves.

The one framework-specific concession in the file is a `[Bindable]` attribute
guarded by `#if HAS_CODEBRIX`, a symbol Core and all six Skia head csproj files
define. The WinUI head defines `HAS_WINUI` and the file uses it for one startup
timing difference. Those two symbols are the complete list of places where a head
can drift, which is the point: keep them countable.

Read `Shared/ViewModels/MainViewModel.cs` first, then one head's csproj to see the
three `<Compile Include="..\Shared\...">` lines, then a second head's csproj to
confirm they are the same three lines. See
[Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](../BLUEPRINTS-AppStructureAndStartup.md#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads)
and [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).

### Six heads, one Core library, one runtime package each

`CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj` is the only project
in the Skia half of the application with a list of packages. It carries the
framework, the add-ins, the font, the SVG engine and the Lottie engine, links the
shared view models, embeds the assets, holds `Controls/`, and project-references
the encryption library. Each of the six head csproj files beside it does four
things and nothing more: it declares the two `<Page Include>` and `<None Remove>`
lines that make MSBuild treat the imported `.xaml` as XAML pages, imports the
shared project's `.projitems`, project-references Core, and adds exactly one
runtime package. Every one of the six repeats the same comment saying that all
other packages come from Core, and keeping that comment literally true is what
makes six heads maintainable.

Read the Core csproj, then any one head csproj, then diff two head csproj files
against each other. See
[Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package)
and [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).

### The shared XAML, and where the four UI stacks genuinely differ

`CodeBrixPlatform/JustBetweenUs.UI/` is a shared project: a `.shproj` and a
`.projitems` that list `App.xaml`, `Views/MainPage.xaml` and their code-behind.
Each Skia head imports the `.projitems` with `Label="Shared"`, so the same markup
compiles into six different assemblies with six sets of generated partial classes.
The shared project produces no assembly of its own, but all three solutions list
it so it appears in the tree.

The interesting part is what is not shared. The Skia page binds with the
platform's `{d:Binding ...}` markup extension and maps its default XML namespace
onto the platform's controls assembly; the WinUI, WPF and MAUI pages use plain
`{Binding ...}` against the same property names. The Skia and WinUI pages use
icon buttons and the animated star, while `JustBetweenUs.Wpf/Views/MainWindow.xaml`
and `Mobile/Views/MainPage.xaml` use plain text buttons and a small "i" button.
The MAUI page uses a `Picker` where the others use a `ComboBox`, bound to exactly
the same two properties. Four pages, one view model.

Read `CodeBrixPlatform/JustBetweenUs.UI/JustBetweenUs.UI.projitems`, then the four
pages side by side. See
[Share App xaml and the views across heads with a shared project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#share-app-xaml-and-the-views-across-heads-with-a-shared-project)
and [Declare a Skia page and bind with the platform Binding markup extension](../BLUEPRINTS-ViewsAndControls.md#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension).

### Startup: the container, the design-mode switch and the window

`CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs` does the whole bootstrap in the
constructor, in order: set the default text font family to the bundled Open Sans by
its `ms-appx:` path, call
`SimpleServiceResolver.CreateInstance` with the shared `IHostBuilderProvider` and
a lambda whose only line is `services.AddEncryption()`, call
`SimpleViewModel.SetIsDesignMode(false)`, then `InitializeComponent()`. Missing
that `SetIsDesignMode(false)` call is silent: nothing throws, the view model
simply never does its startup work, because its constructor body is guarded by
`IsDesignMode(true)`.

`OnLaunched` then creates the window, installs a `Frame` if the window has none,
navigates to `Views.MainPage` and activates. It never touches a view model; the
page sets its own `DataContext`. Navigation failure throws rather than logging, so
a typo in the page type surfaces immediately instead of showing an empty window.

The MAUI head is the one place the order differs: `Mobile/App.xaml.cs` calls
`InitializeComponent()` first, then the resolver, then `SetIsDesignMode(false)`.
Both work here, but setting the resolver up before any view is constructed is the
safer habit, because a page whose XAML instantiates the view model will resolve
services during `InitializeComponent()` — which is exactly what these pages do.

The font line has a rule of its own recorded beside it. `App.xaml` keeps a comment,
and a commented-out merge line, saying that merging the font library's resource
dictionary does not work on Skia targets; reference the `.ttf` by its `ms-appx:`
path instead, both for the default font family and for the `OpenSansFont` resource
key the page names in its `FontFamily`. The MAUI head does not use that library at
all and registers its own font files through `ConfigureFonts` in `MauiProgram.cs`.

Read `CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`, then `App.xaml`, then
`Shared/Helpers/HostHelper.cs`, then `JustBetweenUs.Encryption/RegisterServices.cs`,
then compare with `JustBetweenUs.WinUI/App.xaml.cs` and `Mobile/App.xaml.cs`. See
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Register library services with one AddXxx extension method](../BLUEPRINTS-AppStructureAndStartup.md#register-library-services-with-one-addxxx-extension-method)
and [Create the main window and navigate to the first page](../BLUEPRINTS-AppStructureAndStartup.md#create-the-main-window-and-navigate-to-the-first-page).

### Commands that enable themselves, the algorithm picker and the information dialog

`EncryptionKey` and `EnteredText` carry
`[AffectsCommands(nameof(EncryptCommand), nameof(DecryptCommand))]`;
`ProcessedText` carries `[AffectsCommands(nameof(CopyToClipboardCommand))]`. The
text boxes bind `Mode=TwoWay, UpdateSourceTrigger=PropertyChanged`, so every
keystroke pushes into the property, the attribute refreshes the named commands,
and the buttons enable and disable themselves with no manual
`RaiseCanExecuteChanged` anywhere.

Two details are worth copying. Every command body re-checks its own `CanExecute`
as its first statement, which guards against a command being invoked before the UI
has refreshed. And `CanDecrypt` deliberately leaves out the "does this look like
Base64" check: the commented-out line in the file records that including it made
the Decrypt button flash on and off as the user typed, so the check moved into the
command body, where a failure shows an informational message instead of silently
disabling a button.

The algorithm dropdown is driven from an enum rather than from strings.
`Shared/ViewModels/EncryptionMode.cs` derives from
`SimpleEnumInfo<EncryptionMode.CryptAlgorithm>`, gives each enum member a
`[SimpleEnum<EncryptionMode>]` attribute pointing at a static property that supplies
its friendly description, and exposes `GetDictionary()`. The view model builds the
list of descriptions from that dictionary and binds it; a `ComboBox` on three stacks
and a `Picker` on MAUI consume the same two bindings with no view-model change. One
pitfall lives here: the bound property is the description string, so the setter maps
text back to the enum with a `Single()` lookup that would throw if two members ever
shared a description. Binding the `EncryptionMode` object with a display member, or
exposing the enum itself as the bound property, avoids that.

The information button is the simplest command in the file: `SimpleOsInfo.GatherInfo()`
is awaited once, cached in a field, formatted into a `StringBuilder` and passed to
`ShowInfo`. There is no head-specific code at all, which is exactly what makes it a
useful proof of how many places this code runs. A note in the file records that on
Android the dialog text is truncated to a maximum number of lines, so long
multi-line dialog bodies are not a portable choice.

Read the `Bindable properties` and `Commands and their implementations` regions of
`Shared/ViewModels/MainViewModel.cs`, then `Shared/ViewModels/EncryptionMode.cs`. See
[Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way),
[Confirm and inform from the view model with SimpleViewModel dialogs](../BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs),
[Bind a picker to enum values with or without friendly labels](../BLUEPRINTS-MVVM.md#bind-a-picker-to-enum-values-with-or-without-friendly-labels)
and [Report the host operating system from the view model](../BLUEPRINTS-MVVM.md#report-the-host-operating-system-from-the-view-model).

### The clipboard bridge, and degrading gracefully without one

The view model declares its own one-property interface in its own file:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public interface ICopyToClipboard { Action<string> CopyTextToClipboard { get; set; }}
```

`MainViewModel` implements it. `DoCopyToClipboard` checks whether the delegate was
supplied: if it was, it invokes it inside `InvokeOnMainThread` and shows a
confirmation the first time only, tracked by a private flag so a repeated action
does not nag; if it was not, it shows an error saying the feature is not enabled on
this platform. Nothing throws and the head still runs.

Each page assigns the delegate in its `DataContextChanged` (or
`BindingContextChanged`) handler, and every page subscribes to that event **before**
`InitializeComponent()`, because on some heads `InitializeComponent()` is what sets
the data context. The comment in
`CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs` says exactly that.
Three genuinely different clipboard APIs satisfy the same delegate: the platform's
`DataPackage` on the Skia and WinUI heads, `Clipboard.SetText` on WPF, and
`Clipboard.Default.SetTextAsync` on MAUI. That is why the bridge is a delegate the
head fills in rather than a method the view model could call.

Read `Shared/ViewModels/MainViewModel.cs`, then the four code-behind files in the
order `CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`,
`JustBetweenUs.WinUI/Views/MainPage.xaml.cs`,
`JustBetweenUs.Wpf/Views/MainWindow.xaml.cs`, `Mobile/Views/MainPage.xaml.cs`. See
[Copy text to the clipboard from a command through a bridge interface](../BLUEPRINTS-PlatformServices.md#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface)
and [Dispose a view model its commands and its bridge delegates](../BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### Async startup, the dialog anchor, and the timing pitfall

The view model needs the default key from the service before the user can do
anything useful, and it must not block construction of the page. The MVVM shape is
a named initialization the constructor starts and a page or a test can await:

```csharp
// Adapted from CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
// The sample starts an unnamed fire-and-forget Task; this version names the
// initialization so nothing observing the view model has to guess when it finished.
public Task Initialization { get; private set; } = Task.CompletedTask;

private async Task InitializeAsync()
{
    var defaultKey = await _encryptSvc.GetDefaultKey();
    //Assigning a bound property off the UI thread causes problems on Linux and macOS
    InvokeOnMainThread(() => EncryptionKey = defaultKey);
}
```

The `InvokeOnMainThread` wrapper is the rule worth remembering. The comment beside
it in the file says assigning a bound property off the UI thread causes problems on
Linux and macOS; on Windows it appears to work. Test the marshalling on the
strictest head, not the most forgiving one.

The same initialization then shows the "adapted from a sample provided by Paul
Ainsworth" dialog, and this is where the sample records its most instructive sharp
edge. The dialog needs a UI anchor that does not exist until the page has laid out,
so the code pads itself with a fixed delay, longer under `HAS_WINUI`. The comment
in the file names the symptom (an exception about a missing anchor on a freshly
cloned WinUI solution) and says outright that the real fix is awaiting a
page-loaded signal rather than a fixed delay. Prefer a readiness signal in your own
code, supplied through the same bridge that supplies the anchor.

That anchor arrives as a getter, not a value:
`(DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot)`. Passing a
lambda means the anchor is read at the moment a dialog is shown, so the view model
never holds a stale reference; the MAUI page passes itself, and the WPF page skips
the line entirely because its dialogs need no anchor.

Read the constructor of `Shared/ViewModels/MainViewModel.cs`, then
`CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`. See
[Kick off async startup loading from the view model constructor](../BLUEPRINTS-MVVM.md#kick-off-async-startup-loading-from-the-view-model-constructor),
[Set bound properties from a background thread with InvokeOnMainThread](../BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer)
and [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).

### The encryption library: the shape of a service the view model can trust

`JustBetweenUs.Encryption/` targets plain `net10.0`, references no UI framework,
exposes one interface, takes an `ILogger<EncryptionService>` through its
constructor and registers itself with a single `AddEncryption()` extension. The
view model resolves `IEncryptionService` with the inherited `GetService<T>()` and
never sees a cipher, a key derivation or a byte array.

Two shape decisions carry across to any service you write. Every interface method
returns `Task<string>` even where the work is synchronous, and the implementation
wraps its CPU-bound calls in `Task.Run`, so the view model's commands are genuinely
asynchronous rather than asynchronous-looking. And the `[Obsolete]` attributes sit
on the concrete Triple DES methods, not on the interface members, so a caller going
through the interface gets no warning; if you want callers warned, put the
attribute on the interface too.

The algorithms themselves are instructive as a set. AES and Twofish each generate
random material (an initialization vector and a salt respectively) and append it to
the ciphertext before Base64 encoding, so a single string is all the user has to
copy, and each decryption path checks the array is longer than that material before
splitting it. Triple DES uses ECB mode with no randomness, which is why its output
is reproducible and why the tests can assert an exact string for it; it is in the
sample as a deliberately obsolete example. The text key is turned into key bytes
with an MD5 hash for AES and Triple DES, which is sample-grade key derivation, not
a recommendation; the Twofish path shows the better shape, a salted PBKDF2
derivation over a SHA-3 digest.

The service is also where the application's most instructive defect was fixed.
Users paste the encrypted output back into the application, and on Intel macOS an
invisible `U+0001` control character was riding along on the clipboard-to-text-box
path: the validity check returned false, decryption was blocked, and nothing
visible explained it. The fix is a private `CleanBase64` that keeps only the
standard Base64 alphabet, applied both in `IsBase64Text` and in all three decrypt
paths so the validity check and the decode can never disagree. Putting the guard in
the service rather than in the view model or the page is the point: no caller can
forget it, and no head has to know the defect existed. Anything that survives a
system clipboard round-trip in a multi-head application deserves the same
treatment.

Read `JustBetweenUs.Encryption/Services/IEncryptionService.cs`, then
`RegisterServices.cs`, then `Services/EncryptionService.cs`, ending with
`CleanBase64` and `IsBase64Text` and the regression test that pins them. See
[Put the real work in a UI free library behind a service interface](../BLUEPRINTS-DocumentsAndData.md#put-the-real-work-in-a-ui-free-library-behind-a-service-interface),
[Encrypt text with the CodeBrix Cryptography library](../BLUEPRINTS-DocumentsAndData.md#encrypt-text-with-the-codebrix-cryptography-library),
[Read an embedded default value at run time](../BLUEPRINTS-DocumentsAndData.md#read-an-embedded-default-value-at-run-time)
and [Guard Base64 input against invisible clipboard characters](../BLUEPRINTS-DocumentsAndData.md#guard-base64-input-against-invisible-clipboard-characters).

### Embedded assets, custom controls and the URI scheme that finds them

`Shared/Assets/` holds the SVG icons and the Lottie star animation. Core links them
as `<EmbeddedResource>` items with an `Assets\` link path; because Core's
`RootNamespace` is `JustBetweenUs`, the manifest name becomes
`JustBetweenUs.Assets.clipboard.svg` and the page names it as
`embedded://JustBetweenUs.Core/JustBetweenUs.Assets.clipboard.svg`. The WinUI head
takes the more reliable route and states the manifest name outright with
`<LogicalName>`, then ships the Lottie JSON as content and loads it with an
`ms-appx:` URI instead. Same files on disk, two different pipelines, and the WPF
and MAUI heads embed none of them: assets are a per-head decision.

`CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs` is what makes the
scheme work. It is an `Image` subclass with a string dependency property that
parses `embedded://Assembly/Resource.Name`, finds the already-loaded assembly by
name, copies the manifest stream into an in-memory random-access stream and picks
`SvgImageSource` or `BitmapImage` from the file extension. Three sharp edges are
documented in the file and worth carrying over: the two streams are deliberately
not disposed, because disposing the write stream closes the underlying stream and
the image source may hold a reference to it rather than copying; the assembly is
found by scanning loaded assemblies, so something must already have touched it (the
page does, through Core); and load failures are caught and written only to the
debug output, so a wrong resource name shows an empty image with no visible error.

`EmbeddedImageButton.cs` composes an `EmbeddedImage` and a `TextBlock` into a
`StackPanel` and rebuilds its own `Content` whenever any of its layout properties
changes. It overrides `OnContentChanged` so XAML element content is treated as the
`Text` property rather than replacing the composed panel, with a flag preventing
the override from fighting the rebuild, and sets `DefaultStyleKey = typeof(Button)`
so it inherits the standard button template. The page binds its `Command` to the
view model exactly like any other button. The WinUI head does not need this control:
an equivalent with the same property names ships in the WinUI Skia add-in, so the
same markup works there under a different XML namespace.

Read `Controls/EmbeddedImage.cs`, then `Controls/EmbeddedImageButton.cs`, then the
Core csproj's `<EmbeddedResource>` items, then the WinUI csproj's `<LogicalName>`
items. See
[Load an SVG or bitmap from an embedded resource with a custom URI scheme](../BLUEPRINTS-ViewsAndControls.md#load-an-svg-or-bitmap-from-an-embedded-resource-with-a-custom-uri-scheme),
[Build a button that combines an embedded image with text](../BLUEPRINTS-ViewsAndControls.md#build-a-button-that-combines-an-embedded-image-with-text),
[Embed an asset with an explicit logical name and load it by reflection](../BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection),
[Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-svg-art-with-the-codebrix-skiasvg-library),
[Play a Lottie animation on a Skia head and on native WinUI](../BLUEPRINTS-GraphicsAndRendering.md#play-a-lottie-animation-on-a-skia-head-and-on-native-winui)
and [Set the Core library root namespace to the application namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#set-the-core-library-root-namespace-to-the-application-namespace).

### Head-specific plumbing, all of it in Program.cs

Everything a head needs that the others do not lives in that head's `Program.cs` or
csproj, never in shared code. Four cases are worth reading together.

`JustBetweenUs.WinWpfSkia` casts the built host to its concrete type and sets
`RenderSurfaceType.Software` between `Build()` and `Run()`. The comment explains
why: the WPF host's default OpenGL renderer draws through raw `opengl32` onto WPF's
own DirectX-composited window handle, which conflicts on many systems, so the
window appears but the content never composites. Its csproj is also the only Skia
head that targets `net10.0-windows`, because the runtime package flows a WPF
framework reference and the SDK then demands a Windows target platform, and it
deliberately does not set `UseWPF` — that would make WPF's build targets try to
treat the shared `<Page>` XAML items as WPF XAML.

`JustBetweenUs.LinuxFrameBuffer` chains `EnableSoftwareKeyboard` with a
`SoftwareKeyboardOptions` and then `UseDirectSkiaCanvasMode()`. It records both
defaults in comments next to its overrides and chooses the half-height keyboard so
more of the page stays visible. Nothing in the shared UI or the view model changes;
the same `MainPage` simply gets a usable keyboard on a device with no hardware one.

`JustBetweenUs.LinuxWayland` carries a header comment stating its contract: it
requires a Wayland compositor, never falls back, and exits cleanly with a message
if started from X11.

`JustBetweenUs.LinuxX11` carries the longest comment in the application, recording
the Linux ARM64 FreeType linkage problem and its `LD_PRELOAD` workaround, and a
second Raspberry Pi finding about borderless windows under the labwc compositor.
Both are the kind of environment note that saves the next reader a day.

One shape note across all six: five heads use `void Main` with `host.Run()` and
Win32Skia uses `async Task Main` with `await host.RunAsync()`. Pick one form and
use it everywhere unless a head genuinely needs the other. All six carry
`[STAThread]`, including the Linux and macOS heads, and all six call
`App.InitializeLogging()` before the host is built.

Read the six files under `CodeBrixPlatform/JustBetweenUs.*/Program.cs` in one
sitting. See
[Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Enable a picker and the software keyboard on the Linux framebuffer head](../BLUEPRINTS-AppStructureAndStartup.md#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head)
and [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).

### Three solutions, and the WinUI head's architectures

The three `.sln` files exist because some heads only build on one operating system.
Each contains only what can build there, and all three share the same project files
and the same "Solution Items" and "Shared Assets" folders, the latter surfacing
`Shared/Assets` so the icons and the animation can be opened from the solution tree.
All three declare the same platform names, and every project except the WinUI head
maps all of them to `Any CPU`.

The WinUI head is the exception in several ways worth knowing before you add one to
your own solution. It declares `x86;x64;ARM64` with matching runtime identifiers, it
is the only project with `Deploy` entries, and it is the only one whose `Any CPU`
configuration is redirected to a concrete architecture — without those mappings the
solution will not build with `Any CPU` selected. Its csproj also guards its MSIX
tooling and Package-and-Publish capability blocks so the menus light up before the
Windows App SDK package has been restored, and its `launchSettings.json` offers a
packaged and an unpackaged profile.

Read `JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj`, then the configuration block
of `JustBetweenUs.Windows.sln`, then compare the three solutions' project lists. See
[Ship a separate solution where some heads cannot build everywhere](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-separate-solution-where-some-heads-cannot-build-everywhere)
and [Restrict the solution platforms to what a WinUI head declares](../BLUEPRINTS-ProjectLayoutAndPackaging.md#restrict-the-solution-platforms-to-what-a-winui-head-declares).

### Testing a service the way the application builds it

`Shared/Testing/SimpleTestFixture.cs` is a single linked source file, not a package.
It builds a small `IServiceCollection` with the Microsoft.Extensions hosting stack,
exposes `GetService<T>()` that throws rather than returning null when a type was not
registered, reads optional `appsettings.json` files from the working directory, and
offers a virtual `RegisterCustomServices` hook. It also scans its own assembly for
registration classes, giving a second hook for tests that keep their setup in a
separate file.

`tests/JustBetweenUs.Encryption.Tests/EncryptionTestingFixture.cs` subclasses it and
registers `IEncryptionService` with the same constructor dependency the application
gives it, and `Services/EncryptionServiceTests.cs` takes that subclass as an
`IClassFixture` and registers a logger that routes the service's own `ILogger` output
into the test report. That wrapper carries two platform notes worth knowing: xUnit's
output helper does not reliably reach the console on Linux, so output goes to the
console there instead, and writing to the output helper after a test has completed
throws, so every write falls back to the console in a `catch`.

The regression test is the one to read last. It reproduces the corrupted clipboard
input in the test rather than requiring the platform that produced it, and it
asserts both symptoms the fix protects: that the validity check still returns true
and that the text still decrypts to the original message. A regression that fixes
only one of them still fails. The tests that assert an exact ciphertext string
normalize line endings on both sides, because the literal in the source file has
whatever endings the checkout produced; the algorithms with randomness are tested by
round-trip instead.

Read `Shared/Testing/SimpleTestFixture.cs`, then `EncryptionTestingFixture.cs`, then
`Services/EncryptionServiceTests.cs`. See
[Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Test a service the way the container builds it](../BLUEPRINTS-Testing.md#test-a-service-the-way-the-container-builds-it),
[Route logging from the code under test into test output](../BLUEPRINTS-Testing.md#route-logging-from-the-code-under-test-into-test-output)
and [Pin a fixed bug with a regression test that says why it is shaped that way](../BLUEPRINTS-Testing.md#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records that the
application is based on, and was inspired by, a code sample provided by Paul
Ainsworth, that all third-party code dependencies are consumed as NuGet packages
carrying their own licenses and notices, and that no further attribution notices
apply. The only content files bundled with the application are its own image assets
under `Shared/Assets` (the SVG icons and the Lottie star animation) and the stock
.NET MAUI project-template resources inside the MAUI head: the two Open Sans font
files, the template bot image, and the template app-icon and splash SVGs. The Open
Sans the Skia heads use is not a file in this folder at all; it comes from the
CodeBrix.Platform Open Sans font library and is referenced by an `ms-appx:` path.

## License

JustBetweenUs is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
