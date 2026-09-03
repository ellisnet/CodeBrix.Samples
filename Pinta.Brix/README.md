# Pinta.Brix

Pinta.Brix is a layered raster painting and image editor. It opens and saves
images, keeps several documents open in tabs, and paints on stacked layers with
brush, pencil, eraser, paint bucket, gradient, clone stamp, recolor, color
picker and text tools. It makes rectangular, elliptical, lasso and magic-wand
selections and combines them with union, exclude, intersect and xor; it draws
lines, curves, rectangles, rounded rectangles, ellipses and freeform shapes as
re-editable objects; it applies a catalog of adjustments (levels, curves,
brightness and contrast, hue and saturation, posterize, sepia, black and white,
auto level, invert) and effects (blurs, distorts, noise, artistic, render,
stylize and photo) with a live preview that updates while the parameters are
being changed. A history pad records every operation and the user can scrub back
and forth through it; the layers pad shows a live thumbnail per layer. Color work
runs through a primary and secondary palette with a recently-used strip, a
loadable and savable palette file, and a color picker dialog. Zoom runs from one
percent to 3600 percent, the canvas scrolls, and window size, pad widths and tool
options are remembered between runs.

Pinta.Brix is a port of Pinta, the open-source painting application created by
Jonathan Pobst and developed by the Pinta project's contributors; the ported
source files keep their upstream copyright headers, and
[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) records the provenance in
full. It is the reference sample for building a real document-editing
application on CodeBrix.Platform: a document canvas hosted on an `SKXamlCanvas`
with zoom, scroll and dirty-rectangle repaint; a headless engine library that
reaches the UI only through interfaces; menus, toolbars and keyboard shortcuts
generated from a command model rather than declared in XAML; one settings store
for everything that is remembered; and codec, geometry, SVG and text-layout
coverage drawn from CodeBrix libraries and add-ins.

## What this sample shows a CodeBrix.Platform developer

- How to draw a document on an `SKXamlCanvas` subclass that composites layers,
  scales for zoom and repaints fast: [Draw a zoomable document canvas on an SKXamlCanvas subclass](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-zoomable-document-canvas-on-an-skxamlcanvas-subclass).
- How to accumulate invalidated regions and re-composite only the dirty part of
  a cached surface: [Repaint only the dirty rectangle of a cached composite](../BLUEPRINTS-GraphicsAndRendering.md#repaint-only-the-dirty-rectangle-of-a-cached-composite).
- How to let the model, not the `ScrollViewer`, decide where the viewport goes
  after a zoom: [Host a canvas in a scroll viewer and drive zoom and scroll from an interface](../BLUEPRINTS-GraphicsAndRendering.md#host-a-canvas-in-a-scroll-viewer-and-drive-zoom-and-scroll-from-an-interface).
- How to turn pointer and key events into a framework-free input model so the
  tools can be tested headless: [Translate platform pointer and key events into a headless input model](../BLUEPRINTS-ViewsAndControls.md#translate-platform-pointer-and-key-events-into-a-headless-input-model).
- How to build menus, toolbars and pad toolbars from a command model so a
  command's label, icon, shortcut and enabled state are declared once: [Build menus and toolbars from a command model instead of XAML](../BLUEPRINTS-ViewsAndControls.md#build-menus-and-toolbars-from-a-command-model-instead-of-xaml).
- How to get keyboard shortcuts working on the Skia heads from a single page
  `KeyDown` handler: [Dispatch keyboard shortcuts from one page KeyDown handler](../BLUEPRINTS-ViewsAndControls.md#dispatch-keyboard-shortcuts-from-one-page-keydown-handler).
- How to recompute the enabled state of dozens of commands in one pass from the
  facts they all depend on: [Refresh command enablement in one pass from a headless command model](../BLUEPRINTS-MVVM.md#refresh-command-enablement-in-one-pass-from-a-headless-command-model).
- How to render a per-tool options row from descriptors a UI-free library
  appends to a list: [Render a tool options toolbar from a descriptor model](../BLUEPRINTS-ViewsAndControls.md#render-a-tool-options-toolbar-from-a-descriptor-model).
- How to run an expensive transform on worker threads, show partial results as
  they land, stay cancellable and end up in the undo history: [Run an effect on worker threads with a live preview](../BLUEPRINTS-MVVM.md#run-an-effect-on-worker-threads-with-a-live-preview).
- How to show a parameters panel that does not dim the window, so a live preview
  stays visible and interactive: [Show a modeless floating options panel so a live preview stays visible](../BLUEPRINTS-ViewsAndControls.md#show-a-modeless-floating-options-panel-so-a-live-preview-stays-visible).
- How to generate an options panel from a data object's properties instead of
  hand-building one per effect: [Generate an options panel from object properties by reflection](../BLUEPRINTS-ViewsAndControls.md#generate-an-options-panel-from-object-properties-by-reflection).
- How to drive an undo history from a list the user can click into, travelling
  one step at a time: [Drive an undo history from a list and travel to a clicked point](../BLUEPRINTS-MVVM.md#drive-an-undo-history-from-a-list-and-travel-to-a-clicked-point).
- How to keep a `TabView` and a model-owned document list in sync in both
  directions: [Bind a tab per open document and keep both directions in sync](../BLUEPRINTS-MVVM.md#bind-a-tab-per-open-document-and-keep-both-directions-in-sync).
- How to stop a programmatic selection change from commanding the control back:
  [Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).
- How to funnel every close path through one save prompt so no document is lost
  quietly: [Prompt before discarding unsaved work](../BLUEPRINTS-MVVM.md#prompt-before-discarding-unsaved-work).
- How to veto the window's own close, run an async prompt and re-issue the close
  when the answer arrives: [Veto a window close until unsaved work is handled](../BLUEPRINTS-PlatformServices.md#veto-a-window-close-until-unsaved-work-is-handled).
- How to install UI dialogs into a UI-free library through handler delegates:
  [Install UI dialogs into a headless model through handler delegates](../BLUEPRINTS-PlatformServices.md#install-ui-dialogs-into-a-headless-model-through-handler-delegates).
- How to put the clipboard behind an interface that starts as a no-op so callers
  never have to test for it: [Put a platform service behind an interface with a no-op default](../BLUEPRINTS-PlatformServices.md#put-a-platform-service-behind-an-interface-with-a-no-op-default).
- How to give a UI-free library a periodic tick without letting it see the
  dispatcher: [Marshal a repeating timer into a headless model](../BLUEPRINTS-PlatformServices.md#marshal-a-repeating-timer-into-a-headless-model).
- How to hand the view model a `XamlRoot` getter so its dialog helpers can
  attach: [Give the view model a XamlRoot so its dialogs can show](../BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
- How to let the model choose the mouse cursor without holding a platform type:
  [Set the mouse cursor from a model owned interface](../BLUEPRINTS-PlatformServices.md#set-the-mouse-cursor-from-a-model-owned-interface).
- How to show progress and offer cancel from a synchronous loop that keeps
  running while the dialog is up: [Show a cancellable progress dialog from synchronous code](../BLUEPRINTS-ViewsAndControls.md#show-a-cancellable-progress-dialog-from-synchronous-code).
- How to build a small pixel-exact control that draws itself and hit-tests its
  own geometry: [Build a drawn widget as an SKXamlCanvas subclass with hit testing](../BLUEPRINTS-ViewsAndControls.md#build-a-drawn-widget-as-an-skxamlcanvas-subclass-with-hit-testing).
- How to draw in an element's logical coordinates on a surface that is in
  physical pixels: [Scale a Skia drawn control from surface pixels to logical units](../BLUEPRINTS-GraphicsAndRendering.md#scale-a-skia-drawn-control-from-surface-pixels-to-logical-units).
- How to animate an overlay with a timer that stops as soon as its view leaves
  the tree: [Animate an overlay with a timer that stops when unloaded](../BLUEPRINTS-GraphicsAndRendering.md#animate-an-overlay-with-a-timer-that-stops-when-unloaded).
- How to supply a draggable splitter bar where the platform ships none: [Supply a splitter bar where the platform has none](../BLUEPRINTS-ViewsAndControls.md#supply-a-splitter-bar-where-the-platform-has-none).
- How to lay out an editor shell of menus, toolbars, a toolbox, tabbed documents,
  side pads and a status bar: [Lay out a document editor shell with tabs a toolbox and pads](../BLUEPRINTS-ViewsAndControls.md#lay-out-a-document-editor-shell-with-tabs-a-toolbox-and-pads).
- How to turn raw premultiplied BGRA pixels into an `ImageSource` an `Image`
  element can show: [Turn raw pixel surfaces into XAML image sources](../BLUEPRINTS-GraphicsAndRendering.md#turn-raw-pixel-surfaces-into-xaml-image-sources).
- How to carry an icon set as embedded resources and rasterize its scalable art
  at any requested size: [Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-svg-art-with-the-codebrix-skiasvg-library).
- How to give a large body of ported drawing code an immediate-mode facade so
  only one namespace knows SkiaSharp: [Give a headless library a drawing facade over SkiaSharp](../BLUEPRINTS-GraphicsAndRendering.md#give-a-headless-library-a-drawing-facade-over-skiasharp).
- How to do boolean geometry on user-drawn regions: [Combine selection polygons with the CodeBrix PolygonTools library](../BLUEPRINTS-GraphicsAndRendering.md#combine-selection-polygons-with-the-codebrix-polygontools-library).
- How to register every importer and exporter from one entry point a library
  owns: [Register import and export formats at startup through one entry point](../BLUEPRINTS-DocumentsAndData.md#register-import-and-export-formats-at-startup-through-one-entry-point).
- How to add the formats Skia cannot encode without changing anything above the
  format registry: [Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library](../BLUEPRINTS-DocumentsAndData.md#add-codec-coverage-beyond-skiasharp-with-the-codebrix-imaging-library).
- How to decode photographs upright: [Honor EXIF orientation when decoding with SkiaSharp codecs](../BLUEPRINTS-GraphicsAndRendering.md#honor-exif-orientation-when-decoding-with-skiasharp-codecs).
- How to run a Save As through a native picker whose filters come from the format
  registry, warning before a lossy conversion: [Save a document through a native picker with format filters](../BLUEPRINTS-DocumentsAndData.md#save-a-document-through-a-native-picker-with-format-filters).
- How to let a codec ask the UI one optional question without taking a UI
  dependency: [Raise a UI hook from a codec through a static event](../BLUEPRINTS-DocumentsAndData.md#raise-a-ui-hook-from-a-codec-through-a-static-event).
- How to put one application-named facade in front of the settings backend so
  nothing else knows what the store is made of: [Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade).
- How to open that store before anything can read a setting: [Open the settings store before any other startup work](../BLUEPRINTS-SettingsAndPersistence.md#open-the-settings-store-before-any-other-startup-work).
- How to reopen at the size the user left, including the scale conversion that is
  easy to get wrong: [Restore a remembered window size before any window exists](../BLUEPRINTS-SettingsAndPersistence.md#restore-a-remembered-window-size-before-any-window-exists).
- How to keep a palette, a recent-colors list and per-tool options in the same
  store as everything else: [Persist small pieces of application state through the same store](../BLUEPRINTS-SettingsAndPersistence.md#persist-small-pieces-of-application-state-through-the-same-store).
- How to flush state that used to be written at quit, in an application that has
  no quit command: [Flush deferred settings at natural points instead of at quit](../BLUEPRINTS-SettingsAndPersistence.md#flush-deferred-settings-at-natural-points-instead-of-at-quit).
- How to shape, measure, hit-test and outline re-editable text with no XAML text
  control involved: [Lay out and draw text through the CodeBrix Platform TextLayout add-in](../BLUEPRINTS-TextEditing.md#lay-out-and-draw-text-through-the-codebrix-platform-textlayout-add-in).
- How to boot each of the six desktop heads from a `Program.Main()` that differs
  only in its head extension method: [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- How to order the work in the `App` constructor: [Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor).
- How to give `SimpleServiceResolver` a generic host builder from the Core
  library: [Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver).
- How to tell `SimpleViewModel` it is not running in a designer, and how a view
  model constructor guards for one: [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- How to make a bundled font the default for every text element on the Skia
  heads: [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- How to wire framework logging in debug builds only, before the host is built:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- How to select the render surface on the WinWpfSkia head without touching any
  other head: [Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head).
- How to keep a library that references CodeBrix.Platform from breaking the head
  build: [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- How to keep one page's wiring navigable when it genuinely is large: [Split a page code-behind into named partial files](../BLUEPRINTS-ViewsAndControls.md#split-a-page-code-behind-into-named-partial-files).
- How to build a test project that runs real platform code headless instead of
  reference assemblies: [Build a test project against real CodeBrix Platform assemblies](../BLUEPRINTS-Testing.md#build-a-test-project-against-real-codebrix-platform-assemblies).
- How to add, by hand, the native libraries an application head would have got
  from its runtime package: [Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied).
- How to let a test project see a library's internal types: [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- How to write golden-image tests that are exact but tolerate one-bit rounding,
  and report the first differences usefully: [Compare rendered images pixel by pixel](../BLUEPRINTS-Testing.md#compare-rendered-images-pixel-by-pixel).
- How to keep tests away from the user's real settings when the store is a
  process-global singleton: [Point a process-global store at a throwaway folder in tests](../BLUEPRINTS-Testing.md#point-a-process-global-store-at-a-throwaway-folder-in-tests).

## Building, running and testing

There is one solution, `Pinta.Brix.slnx`. It holds the shared UI project,
`Pinta.Brix.Core`, all six heads, a `Libraries` folder with the six libraries
under `src/libs`, and a `Tests` folder with the six test projects under
`tests/libs`. Its own comment describes it as everything that builds with the
plain .NET SDK, so it opens and builds on Linux, macOS and Windows alike.

The six heads:

| Head project | Platform |
| --- | --- |
| `src/Pinta.Brix.LinuxX11` | Linux, X11 |
| `src/Pinta.Brix.LinuxWayland` | Linux, Wayland |
| `src/Pinta.Brix.LinuxFrameBuffer` | Linux, direct framebuffer, no display server |
| `src/Pinta.Brix.MacOS` | macOS |
| `src/Pinta.Brix.Win32Skia` | Windows, native Win32 window |
| `src/Pinta.Brix.WinWpfSkia` | Windows, Skia rendering hosted in a WPF window |

Prerequisites are short. The .NET 10 SDK is the only requirement; every project
targets `net10.0` except the WinWpfSkia head, which targets `net10.0-windows`
and already sets `EnableWindowsTargeting`, so it restores and builds from Linux
and macOS too. No workloads are used. There are no accounts, tokens, downloads,
hardware requirements or data files the user has to supply: the application
opens with a blank white document already created, through the same code path as
File > New, so it is usable with no input at all. The settings store is created
silently on first run in the application's per-user configuration folder.

Run one head from the command line:

```text
dotnet run --project src/Pinta.Brix.LinuxX11
dotnet run --project src/Pinta.Brix.MacOS
dotnet run --project src/Pinta.Brix.Win32Skia
```

`global.json` in this folder carries nothing but the test runner selection: it
sets the `Microsoft.Testing.Platform` runner. Every test project is built as an
executable with `UseMicrosoftTestingPlatformRunner`, using xUnit v3 and
SilverAssertions. Because that runner is selected, a plain `dotnet test` at the
solution can report that zero tests ran. Build the test project and run the
produced executable directly instead:

```text
dotnet build tests/libs/Pinta.Brix.Engine.Tests
./tests/libs/Pinta.Brix.Engine.Tests/bin/Debug/net10.0/Pinta.Brix.Engine.Tests
```

There is one test project per library:

| Test project | Covers |
| --- | --- |
| `tests/libs/Pinta.Brix.Engine.Tests` | Angles, bit masks, blend operations, `ColorBgra` and `Color`, dash patterns, gradients, math and utility helpers, number ranges, rectangles, scanlines, selection combine modes, surface differencing, the text engine and the text-layout wrapper |
| `tests/libs/Pinta.Brix.Effects.Tests` | Every adjustment and every effect category as golden-image comparisons against bundled PNG assets, with mock chrome, live-preview, palette, system and workspace services |
| `tests/libs/Pinta.Brix.FileFormats.Tests` | Format registration by extension and capability, the CodeBrix.Imaging-backed importer and exporter, the Netpbm pixmap codec, the OpenRaster codec, the SkiaSharp codec wrapper and the TGA exporter |
| `tests/libs/Pinta.Brix.Settings.Tests` | The store the settings facade wraps: fresh creation, auto-backup naming and pruning, corruption recovery, import and export |
| `tests/libs/Pinta.Brix.Controls.Tests` | The accelerator string parser and the command accelerator table that dispatches every keyboard shortcut |
| `tests/libs/Pinta.Brix.Tools.Tests` | A placeholder; the tools' own ported tests land as the tools port progresses |

The tests run headless: no GPU, no network, no display. On Linux the test
projects that touch pixels pull in the SkiaSharp native library explicitly, and
`Pinta.Brix.Engine.Tests` also pulls in the HarfBuzz native library for text
shaping and sets `CodeBrixRuntimeIdentifier=skia` so the platform's reference
assemblies are swapped for real implementations. `Pinta.Brix.Engine.Tests` and
`Pinta.Brix.FileFormats.Tests` each point the process-global settings store at a
throwaway temp folder from a `[ModuleInitializer]`, so a test run never touches
the user's real settings.

## How the projects and folders are organized

```text
Pinta.Brix.slnx                      One solution: shared UI, Core, six heads, six libraries, six test projects
global.json                          Selects the Microsoft.Testing.Platform test runner
THIRD-PARTY-NOTICES.txt              Upstream provenance, icon-set attribution and the naming policy
license-pdn.txt                      Upstream-of-upstream license text, copied verbatim
src/Pinta.Brix.UI/                   Shared items project (.shproj + .projitems): App.xaml(.cs), Views/, Dialogs/
src/Pinta.Brix.UI/Views/             MainPage.xaml plus its code-behind partials (Menus, Actions, Dialogs, Palette)
src/Pinta.Brix.UI/Dialogs/           Hand-built dialogs: Alignment, Curves, Levels, Posterize
src/Pinta.Brix.Core/                 The library every head references; carries the platform and font packages
src/Pinta.Brix.Core/ViewModels/      MainViewModel, derived from SimpleViewModel
src/Pinta.Brix.Core/Helpers/         HostHelper, the IHostBuilderProvider SimpleServiceResolver builds from
src/Pinta.Brix.LinuxX11/             LinuxX11 head: a Program.cs and a csproj, nothing else
src/Pinta.Brix.LinuxWayland/         LinuxWayland head
src/Pinta.Brix.LinuxFrameBuffer/     LinuxFrameBuffer head
src/Pinta.Brix.MacOS/                MacOS head
src/Pinta.Brix.Win32Skia/            Win32Skia head
src/Pinta.Brix.WinWpfSkia/           WinWpfSkia head (net10.0-windows; selects a software render surface)
src/libs/Pinta.Brix.Settings/        Facade over the AppSettings add-in: the single settings store and a log forwarder
src/libs/Pinta.Brix.Engine/          Headless engine: PintaCore, managers, documents, layers, history, drawing facade
src/libs/Pinta.Brix.Engine/Drawing/  Immediate-mode drawing facade (Context, ImageSurface, Path, Pattern) over SkiaSharp
src/libs/Pinta.Brix.Engine/Actions/  The declarative command model (File, Edit, View, Image, Layers, Window, Help, App)
src/libs/Pinta.Brix.Engine/Managers/ Chrome, workspace, tools, palette, effects, settings, live preview, canvas grid
src/libs/Pinta.Brix.Engine/Services/ The interfaces the UI layer implements: ICanvasView, IClipboardService, ITimerService, ...
src/libs/Pinta.Brix.FileFormats/     Importers and exporters plus their registration entry point
src/libs/Pinta.Brix.Effects/         Adjustments and effects plus their registration entry point
src/libs/Pinta.Brix.Tools/           Painting, selection, shape and text tools plus their registration entry point
src/libs/Pinta.Brix.Controls/        The only library referencing the platform UI: canvas, widgets, dialogs, menus, icons
src/libs/Pinta.Brix.Controls/Assets/ The embedded icon set (PNG sizes plus scalable SVG) and its attribution notes
tests/libs/Pinta.Brix.*.Tests/       One test project per library, mirroring src/libs
```

Dependency direction runs one way. Each head project imports
`src/Pinta.Brix.UI/Pinta.Brix.UI.projitems` as shared items, so `App.xaml`,
`MainPage.xaml` and every code-behind partial are file-linked and compiled into
each head rather than referenced; a shared items project does not glob, so every
new page and partial has to be listed in the `.projitems` by hand. Each head also
project-references `Pinta.Brix.Core` and adds exactly one platform head runtime
package, and nothing else: every other package arrives through Core.

`Pinta.Brix.Core` project-references `Pinta.Brix.Engine`, `Pinta.Brix.Effects`,
`Pinta.Brix.Tools`, `Pinta.Brix.FileFormats` and `Pinta.Brix.Controls`, and
carries the CodeBrix.Platform and Open Sans font packages for all of them.
`Pinta.Brix.Controls` references `Pinta.Brix.Engine` and `Pinta.Brix.Tools`;
`Pinta.Brix.Effects`, `Pinta.Brix.Tools` and `Pinta.Brix.FileFormats` each
reference `Pinta.Brix.Engine`; `Pinta.Brix.Engine` references
`Pinta.Brix.Settings`, which references nothing but the settings add-in.
`Pinta.Brix.Controls` is the only library that references the platform UI and the
only one that compiles XAML-facing types, which is why it is also the only one
that has to keep its own `RootNamespace`.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The XAML application framework: `Application`, `Window`, `Page`, `TabView`, `MenuBar`, `ContentDialog`, `Popup`, pointer and key events, `DispatcherQueue`, pickers, clipboard, and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleServiceResolver`) | `src/Pinta.Brix.Core`, `src/libs/Pinta.Brix.Controls`, all of `src/Pinta.Brix.UI` |
| The CodeBrix.Platform runtime for the head | One head runtime package per head project, and exactly one; every other package comes from `Pinta.Brix.Core` | the six head projects in `src/` |
| CodeBrix.Platform.SkiaSharp.Views | Supplies `SKXamlCanvas`, the base class for the document canvas, the palette widget, the histogram widget and the gradient widget | `src/libs/Pinta.Brix.Controls` |
| CodeBrix.Platform.AppSettings add-in | The whole settings backend: one portable store with startup auto-backup, pruning, corruption recovery and import/export. The only persistence dependency in the application | `src/libs/Pinta.Brix.Settings` |
| CodeBrix.Platform.TextLayout add-in | Lays out, measures, hit-tests and outlines all re-editable text for the text tool | `src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs` |
| CodeBrix.Platform.Fonts.OpenSans | The application's default font, set both as an `App.xaml` resource and as the platform's default text font family | `src/Pinta.Brix.UI/App.xaml`, `src/Pinta.Brix.UI/App.xaml.cs`, `src/Pinta.Brix.Core` |
| CodeBrix.PolygonTools | Boolean polygon clipping for selections: union, difference, intersection, exclusion and simplification | `src/libs/Pinta.Brix.Engine/Classes/DocumentSelection.cs`, `Classes/SelectionModeHandler.cs`, `src/libs/Pinta.Brix.Tools/Tools/LassoSelectTool.cs` |
| CodeBrix.Imaging | Encoders and decoders for the formats SkiaSharp cannot write or read: BMP and GIF export, TIFF both ways | `src/libs/Pinta.Brix.FileFormats/CodeBrixImagingFormat.cs` |
| CodeBrix.SkiaSvg | Renders the scalable SVG icons in the embedded icon set to pixel surfaces at the requested size | `src/libs/Pinta.Brix.Controls/SkiaResourceService.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | The pixel and vector engine underneath the drawing facade: bitmap-backed surfaces, canvas drawing, codec decode, encoded export, font family enumeration | `src/libs/Pinta.Brix.Engine/Drawing/`, `src/libs/Pinta.Brix.FileFormats/SkiaCodecFormat.cs`, `src/libs/Pinta.Brix.Controls/` |
| Microsoft.Extensions.Hosting and Logging.Console | The generic host `SimpleServiceResolver` builds its container from, and the debug-only console logger | `src/Pinta.Brix.Core/Helpers/HostHelper.cs`, `src/Pinta.Brix.UI/App.xaml.cs` |
| xUnit v3 and SilverAssertions | The test framework and assertion library for all six test projects | `tests/libs/` |
| SkiaSharp and HarfBuzz Linux native asset packages | The native libraries the test projects need on Linux, which an application head gets from its head runtime package | `tests/libs/Pinta.Brix.Engine.Tests`, `Pinta.Brix.Effects.Tests`, `Pinta.Brix.FileFormats.Tests` |

## Worth studying in this application

### The engine is headless, and the UI reaches it through interfaces

`Pinta.Brix.Engine` holds the whole document model - documents, layers,
selections, history, the palette, the workspace, the command model and the
drawing facade - and references no UI package at all. Everything it needs from
the UI arrives through interfaces in
`src/libs/Pinta.Brix.Engine/Services/`: `ICanvasView` and `ICanvasScrollView` for
repaint, cursor and viewport; `IClipboardService` for cut, copy and paste;
`ITimerService` for a tick on the UI thread; `IResourceService` for icons. Each
of those has a null implementation or a proxy that the engine holds from the
start, so engine code calls them unconditionally and a head that cannot supply
one degrades instead of failing. `Pinta.Brix.Controls` implements them in
`PlatformServices.cs` and `SkiaResourceService.cs`, and the application installs
them once at startup.

This is the shape to copy. In a view-model version of this application the view
model owns the document state (open documents, active document, dirty flags,
zoom, status text) as bound properties and the shell operations as
`SimpleCommand` commands; the page's contribution is to construct or resolve the
view model, hand it a `XamlRoot` getter, and install the bridge implementations
the engine asked for. Read `Services/ICanvasView.cs` first, then
`PlatformServices.cs`, then `Services/NullServices.cs` to see the degradation
path. The blueprints are [Put a platform service behind an interface with a no-op default](../BLUEPRINTS-PlatformServices.md#put-a-platform-service-behind-an-interface-with-a-no-op-default),
[Marshal a repeating timer into a headless model](../BLUEPRINTS-PlatformServices.md#marshal-a-repeating-timer-into-a-headless-model)
and [Install UI dialogs into a headless model through handler delegates](../BLUEPRINTS-PlatformServices.md#install-ui-dialogs-into-a-headless-model-through-handler-delegates).

### The document canvas

`src/libs/Pinta.Brix.Controls/PintaCanvas.cs` is an `SKXamlCanvas` subclass that
implements `ICanvasView`. It keeps one unscaled composite of all the layers in a
cached surface and, on each paint, draws that composite scaled to the current
zoom - nearest-neighbor at or above 1:1 so a zoomed-in pixel editor stays crisp,
linear below it - then paints the overlay pass on top: the selection outline, the
tool handles, the live preview.

Two things are worth reading closely. The first is invalidation: the document
raises an event carrying either a rectangle or an entire-surface flag, the canvas
unions rectangles until the next paint, and a selection change - which dirties no
pixels at all - has its own invalidation path, without which the selection
outline never appears. The second is that zoom is applied by resizing the element
and scaling at draw time, not by a transform on the `ScrollViewer`; the scroll
viewer only ever pans, and its own zoom mode is off. The offset after a zoom is
computed by `DocumentWorkspace` in the engine and applied by the view through
`ICanvasScrollView`, whose `UpdateLayout()` exists purely as an ordering hook so
the scroll extents reflect the new element size before a new offset is set.

Read `PintaCanvas.cs`, then `CanvasRenderer.cs`, then `PintaCanvasView.cs`, then
`src/libs/Pinta.Brix.Engine/Classes/DocumentWorkspace.cs`. See [Draw a zoomable document canvas on an SKXamlCanvas subclass](../BLUEPRINTS-GraphicsAndRendering.md#draw-a-zoomable-document-canvas-on-an-skxamlcanvas-subclass),
[Repaint only the dirty rectangle of a cached composite](../BLUEPRINTS-GraphicsAndRendering.md#repaint-only-the-dirty-rectangle-of-a-cached-composite),
[Host a canvas in a scroll viewer and drive zoom and scroll from an interface](../BLUEPRINTS-GraphicsAndRendering.md#host-a-canvas-in-a-scroll-viewer-and-drive-zoom-and-scroll-from-an-interface)
and [Animate an overlay with a timer that stops when unloaded](../BLUEPRINTS-GraphicsAndRendering.md#animate-an-overlay-with-a-timer-that-stops-when-unloaded).

### Input translated into a model the tools can be tested against

The tools in `Pinta.Brix.Tools` never see a `PointerRoutedEventArgs`. The canvas
builds the engine's own `ToolMouseEventArgs` from the platform event - canvas
coordinates, which button, which modifiers - and hands it to the tool manager.
`InputMapper.cs` is the whole translation, and it carries the reason modifier
state is tracked from the modifier keys' own down and up transitions rather than
probed: the probe API returns nothing on the Skia heads. Pointer capture on press
and release on the way out is what keeps a drag alive when it leaves the element,
and on release the pressed-button flags are already cleared, so the released
button has to be recovered from the update kind.

Read `PintaCanvas.cs` for the event handlers and `InputMapper.cs` for the
mapping, then any tool in `src/libs/Pinta.Brix.Tools/Tools/` to see what the
model side looks like. See [Translate platform pointer and key events into a headless input model](../BLUEPRINTS-ViewsAndControls.md#translate-platform-pointer-and-key-events-into-a-headless-input-model)
and [Set the mouse cursor from a model owned interface](../BLUEPRINTS-PlatformServices.md#set-the-mouse-cursor-from-a-model-owned-interface).

### Menus, toolbars and shortcuts generated from the command model

`MainPage.xaml` declares a `MenuBar` and two empty `StackPanel` hosts and nothing
else: no menu items, no toolbar buttons, no commands. Every command in the
application is declared once in `src/libs/Pinta.Brix.Engine/Actions/` as a plain
object with a label, an icon name, shortcut strings, an enabled flag and an
activation event. `CommandMenuBuilder` turns one into a menu item and keeps its
enabled state in sync; `MainPage.Menus.cs` assembles the menus and the toolbars
from those. Adding a command is one edit in the actions library.

Keyboard shortcuts are dispatched by `CommandAcceleratorTable`, not by XAML
accelerators, and the file says why: accelerators declared on a page or a menu
item do not fire on the Skia heads, though the shortcut text on the item does
display. The page registers one `KeyDown` handler with `handledEventsToo: true`,
because the canvas marks most key events handled, and the table is a plain class
in a library, which is why it can be - and is - unit-tested.

In a view-model version the same builder binds `CanExecute` on `SimpleCommand`
instead of the command model's own enabled flag, and the XAML still declares no
commands. The enablement pass in `MainPage.Actions.cs` recomputes every command
from the same few facts - is a document open, is there a selection, how many
layers, can the history undo - from one "something changed" funnel; that is
exactly what `[AffectsCommands]` automates when the commands live on a view
model. Read `Actions/Command.cs`, then `Menus/CommandMenuBuilder.cs`, then
`MainPage.Menus.cs`, then `MainPage.Actions.cs`. See [Build menus and toolbars from a command model instead of XAML](../BLUEPRINTS-ViewsAndControls.md#build-menus-and-toolbars-from-a-command-model-instead-of-xaml),
[Dispatch keyboard shortcuts from one page KeyDown handler](../BLUEPRINTS-ViewsAndControls.md#dispatch-keyboard-shortcuts-from-one-page-keydown-handler)
and [Refresh command enablement in one pass from a headless command model](../BLUEPRINTS-MVVM.md#refresh-command-enablement-in-one-pass-from-a-headless-command-model).

### Tool options and effect dialogs, generated rather than hand-built

Two different generators sit above the same idea: a library that must not
reference the UI describes what it needs, and the UI materializes it.

Each tool appends framework-free descriptors - a label, a toggle button, a spin
button, a combo box - to a toolbar model, and `ToolBarRenderer` turns them into
real controls and binds both ways. The renderer detaches its subscriptions when
the toolbar is rebuilt, because the descriptors belong to the tool and outlive
any one rebuild.

Effects and adjustments do the same for their parameters. Each effect exposes a
data object of plain properties; `EffectOptionsDialog` walks the public writable
members, skips the base-class ones and anything marked with the skip attribute,
reads a caption attribute for the label, and builds a row per supported type.
The effects that need a bespoke panel get one, and the routing by effect type
lives at the single seam where the UI installs its dialog handler into the engine
- so the effects library itself stays UI-free.

The panels are shown in a modeless `Popup` with its own title bar and OK/Cancel
rather than a `ContentDialog`, because a dialog that dims the window defeats the
live preview the user is adjusting. Read `ToolBarRenderer.cs`, then
`EffectOptionsDialog.cs`, then `FloatingDialogHost.cs`. See [Render a tool options toolbar from a descriptor model](../BLUEPRINTS-ViewsAndControls.md#render-a-tool-options-toolbar-from-a-descriptor-model),
[Generate an options panel from object properties by reflection](../BLUEPRINTS-ViewsAndControls.md#generate-an-options-panel-from-object-properties-by-reflection)
and [Show a modeless floating options panel so a live preview stays visible](../BLUEPRINTS-ViewsAndControls.md#show-a-modeless-floating-options-panel-so-a-live-preview-stays-visible).

### The live preview

`LivePreviewManager` is the most instructive threading code in the application.
It allocates a preview surface, starts `AsyncEffectRenderer` across as many
worker threads as the system service reports, and starts a repeating timer
through `ITimerService` that polls for finished tiles and pushes them to the
canvas. The configuration dialog is awaited concurrently with the render, so the
user sees the effect while choosing its parameters; cancelling cancels the render
and awaits it before returning, and one final poll after the render task
completes is what gets the last tiles onto the screen.

The correctness gate is `BaseEffect.IsTileable`. An effect that accumulates state
across pixels must declare itself non-tileable or parallel tiles produce wrong
output. The canvas renderer substitutes the preview surface for the active layer
while the preview is live, so no separate compositing path is needed.

Read `Managers/LivePreviewManager.cs`, then `Classes/AsyncEffectRenderer.cs`,
then `Effects/BaseEffect.cs`. In a view-model version the command that launches
an effect is a `SimpleCommand`, the busy flag is a bound property, and results
marshal back with `InvokeOnMainThread`. See [Run an effect on worker threads with a live preview](../BLUEPRINTS-MVVM.md#run-an-effect-on-worker-threads-with-a-live-preview)
and [Show a cancellable progress dialog from synchronous code](../BLUEPRINTS-ViewsAndControls.md#show-a-cancellable-progress-dialog-from-synchronous-code).

### Undo history the user can scrub

Every document owns a `DocumentHistory`: a list of items and a pointer. A tool
takes a snapshot on mouse down and pushes a history item on mouse up only if the
surface was actually modified. The history pad lists the items and dims the ones
ahead of the pointer, and clicking one travels to it one step at a time so each
item's own undo or redo runs - jumping the pointer would skip that work. The
guard flag around programmatic selection changes is not optional: the refresh
that follows an undo would otherwise trigger another travel.

In the MVVM shape the history items are an observable collection on the view
model, the selected index is a bound property whose setter travels, and the
suppression flag lives on the view model in a `try`/`finally`. Read
`Classes/DocumentHistory.cs`, then `Pads/HistoryRowFactory.cs`, then the history
handlers in `MainPage.xaml.cs`, then `Tools/PencilTool.cs` for the push side. See
[Drive an undo history from a list and travel to a clicked point](../BLUEPRINTS-MVVM.md#drive-an-undo-history-from-a-list-and-travel-to-a-clicked-point)
and [Stop a two way bound selection from commanding the control back](../BLUEPRINTS-MVVM.md#stop-a-two-way-bound-selection-from-commanding-the-control-back).

### The editor shell: tabs, toolbox, pads and status bar

`MainPage.xaml` is a five-row grid - menu bar, icon toolbar, tool options,
content, status bar - with the content row split into toolbox, tabbed documents,
splitter and pads. Two decisions in it are worth carrying into your own
application. The icon toolbar is deliberately an in-app row rather than an OS
header bar, because the LinuxFrameBuffer head has no window chrome at all and
anything parked there would be unreachable. And the status text, which comes from
the model and can be several lines for the shape tools, is pinned to one line
with `MaxLines` and trimming so the bar cannot grow.

Each open document gets a `TabViewItem` holding a `PintaCanvasView`, tracked in a
dictionary. Model events add and remove tabs and rename their headers; the tab's
own selection change pushes the choice back into the model only when the index
actually differs, which is what stops the two events from ping-ponging. Tab close
runs the save prompt rather than closing the tab, because that is the most likely
way to lose a document. The platform ships no splitter, so `ThumbSplitter` - a
`Border` that captures the pointer and reports drag deltas - supplies one, and
the owner decides what the delta means and persists the result.

Read `MainPage.xaml`, then the tab and splitter code in `MainPage.xaml.cs`. See
[Lay out a document editor shell with tabs a toolbox and pads](../BLUEPRINTS-ViewsAndControls.md#lay-out-a-document-editor-shell-with-tabs-a-toolbox-and-pads),
[Bind a tab per open document and keep both directions in sync](../BLUEPRINTS-MVVM.md#bind-a-tab-per-open-document-and-keep-both-directions-in-sync)
and [Supply a splitter bar where the platform has none](../BLUEPRINTS-ViewsAndControls.md#supply-a-splitter-bar-where-the-platform-has-none).

### Closing without losing work

The application ships no File > Quit command on purpose: the window's own chrome
is the only way out. That makes the close path load-bearing. One async method
prompts for a dirty document and returns save, discard or cancel; one close
method consumes it; one close-all loop iterates a snapshot of the open documents
because closing mutates the collection. The window's `Closed` event vetoes the
close, runs that loop, and re-issues the close when the answer comes back, with a
re-entrancy guard so the confirmed close does not prompt again, and with the
whole body wrapped so a failed prompt leaves the veto standing rather than
dropping the user's work.

Because there is no quit path, nothing can be flushed at exit either. Read
`MainPage.Dialogs.cs` and the `Closed` handler in `App.xaml.cs`. In the MVVM
shape the prompt loop is a method on the view model that `App` resolves from
`SimpleServiceResolver`. See [Prompt before discarding unsaved work](../BLUEPRINTS-MVVM.md#prompt-before-discarding-unsaved-work)
and [Veto a window close until unsaved work is handled](../BLUEPRINTS-PlatformServices.md#veto-a-window-close-until-unsaved-work-is-handled).

### File formats: one registry, several codec libraries

`Pinta.Brix.FileFormats` exposes one static registration entry point that the
application calls at startup, and the format manager starts empty until it runs.
Each format is a descriptor pairing an importer and an exporter - either may be
null - with a display name, extensions, MIME types and a layer-support flag.
Where the codec comes from is invisible above that line: PNG and JPEG go through
SkiaSharp both ways, BMP and GIF import through SkiaSharp and export through
CodeBrix.Imaging, TIFF goes through CodeBrix.Imaging both ways, OpenRaster and
Netpbm are implemented in the library itself, and TGA is export only.

Two details will bite in any application that mixes imaging libraries. The
engine's surfaces are premultiplied BGRA and CodeBrix.Imaging's pixels are
straight alpha, so both directions convert explicitly - skip it and transparent
edges get dark halos. And the SkiaSharp importer honors the encoded EXIF origin,
which means allocating the destination surface with swapped dimensions for the
four transposing origins before drawing through the matching matrix.

The Save As path builds the picker's filter list from the export-capable formats
only, and from the lowercase extension of each, because the registry lists both
cases for matching; a cancelled picker must return false all the way out or a
cancelled save would mark the document clean. Read
`Registration/FileFormats.cs`, then `SkiaCodecFormat.cs`, then
`CodeBrixImagingFormat.cs`, then the save handler in `MainPage.xaml.cs`. See
[Register import and export formats at startup through one entry point](../BLUEPRINTS-DocumentsAndData.md#register-import-and-export-formats-at-startup-through-one-entry-point),
[Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library](../BLUEPRINTS-DocumentsAndData.md#add-codec-coverage-beyond-skiasharp-with-the-codebrix-imaging-library),
[Honor EXIF orientation when decoding with SkiaSharp codecs](../BLUEPRINTS-GraphicsAndRendering.md#honor-exif-orientation-when-decoding-with-skiasharp-codecs),
[Save a document through a native picker with format filters](../BLUEPRINTS-DocumentsAndData.md#save-a-document-through-a-native-picker-with-format-filters)
and [Raise a UI hook from a codec through a static event](../BLUEPRINTS-DocumentsAndData.md#raise-a-ui-hook-from-a-codec-through-a-static-event).

### One settings store for everything remembered

`Pinta.Brix.Settings` is a small library with one dependency - the AppSettings
add-in - exposing `Initialize()`, `Get`, `Set`, `Wrap` and change handlers.
Nothing else in the application knows what the store is made of, and it is the
only project that takes the storage dependency. Everything the application
remembers goes through it: window size, pad widths and pad heights, the primary
and secondary colors, the recently-used strip, the working palette, the canvas
grid, the default image type, the JPEG quality, and every tool's own options
under keys derived from the tool's type name.

Ordering matters and the code says so where it happens. The engine's static
constructor builds the palette manager, which reads settings, so the `App`
constructor opens the store as its first real step; a static constructor that
runs first would silently get defaults instead of the user's values. Window size
is read before any window exists, into the platform's preferred launch size, and
written back on `SizeChanged` - multiplied by the rasterization scale, because
the event reports logical units and the launch size is consumed as native pixels,
and getting that wrong rescales the window at every restart on a scaled display.
And because there is no quit path, values that used to be written at exit are
flushed at natural settle points instead - a tool change, a document close - which
is only cheap because the store does nothing when a value has not changed.

Read `src/libs/Pinta.Brix.Settings/SettingsService.cs`, then the store calls at
the top of `App.xaml.cs`, then `Managers/SettingsManager.cs` and
`Managers/PaletteManager.cs`, then `SettingNames.cs` for the key convention. See
[Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade),
[Open the settings store before any other startup work](../BLUEPRINTS-SettingsAndPersistence.md#open-the-settings-store-before-any-other-startup-work),
[Restore a remembered window size before any window exists](../BLUEPRINTS-SettingsAndPersistence.md#restore-a-remembered-window-size-before-any-window-exists),
[Persist small pieces of application state through the same store](../BLUEPRINTS-SettingsAndPersistence.md#persist-small-pieces-of-application-state-through-the-same-store)
and [Flush deferred settings at natural points instead of at quit](../BLUEPRINTS-SettingsAndPersistence.md#flush-deferred-settings-at-natural-points-instead-of-at-quit).

### Drawing instead of composing: the palette, histogram and gradient widgets

Three small controls are drawn rather than composed from XAML elements, and they
are a good study in when that is the right call. `PaletteWidget` draws the
primary and secondary swatches, the swap and reset glyphs and the recently-used
strip at fixed pixel positions, subscribes to the palette manager's change
events, and raises one semantic event - "the user wants to edit this color" -
that the page turns into a color picker dialog. Its header comment lays out the
entire geometry so the drawing and the hit test cannot drift apart, and the hit
test runs in the same order as the drawing so overlapping regions resolve
correctly.

The histogram and gradient widgets show the two ways to handle display scale on
an `SKXamlCanvas`: the histogram scales the canvas from surface pixels to the
element's logical units once and then draws in logical coordinates, while the
gradient computes everything from the element's own measured size. Read
`Palette/PaletteWidget.cs`, then `Widgets/HistogramWidget.cs` and
`Widgets/ColorGradientWidget.cs`. See [Build a drawn widget as an SKXamlCanvas subclass with hit testing](../BLUEPRINTS-ViewsAndControls.md#build-a-drawn-widget-as-an-skxamlcanvas-subclass-with-hit-testing)
and [Scale a Skia drawn control from surface pixels to logical units](../BLUEPRINTS-GraphicsAndRendering.md#scale-a-skia-drawn-control-from-surface-pixels-to-logical-units).

### Text, icons and the drawing facade

Three pieces of the port each replace an upstream dependency with a CodeBrix one,
and each is worth reading as an example of confining a dependency to one file.

Text runs through the TextLayout add-in. `TextLayout.cs` in the engine wraps the
add-in's layout result, rebuilds it when the text model reports a change, and
exposes exactly what the editor needs: size, caret and selection geometry, hit
testing, and the outline path. Two behaviors are encoded there - the font weight
is clamped onto the add-in's scale, and alignment without wrapping needs a
measure pass first to find the natural width, then a second layout at that width.
Font family enumeration is answered by SkiaSharp directly.

Icons live as embedded resources in `Pinta.Brix.Controls` - PNG at fixed sizes
and scalable SVG - and `SkiaResourceService` resolves a name to an exact-size PNG
when one exists and rasterizes the SVG through CodeBrix.SkiaSvg when it does not,
caching by name and size. It never fails: an unknown name returns a blank
surface, which is why the interface also carries a "has icon" query for callers
that want a text fallback.

And the whole ported drawing corpus works through
`src/libs/Pinta.Brix.Engine/Drawing/`: `Context`, `ImageSurface`, `Path`,
`Pattern`, `Matrix` and `Region` over SkiaSharp. The surface shares the bitmap's
pixel memory with no copies, which is why direct pixel writes must mark it dirty
and reads after drawing must flush first. Its header comments document where the
immediate-mode semantics differ from a retained scene graph: paths are stored in
device space, arcs are flattened before transforming, and stroke widths scale by
the matrix's mean scale factor.

See [Lay out and draw text through the CodeBrix Platform TextLayout add-in](../BLUEPRINTS-TextEditing.md#lay-out-and-draw-text-through-the-codebrix-platform-textlayout-add-in),
[Rasterize SVG art with the CodeBrix SkiaSvg library](../BLUEPRINTS-GraphicsAndRendering.md#rasterize-svg-art-with-the-codebrix-skiasvg-library),
[Turn raw pixel surfaces into XAML image sources](../BLUEPRINTS-GraphicsAndRendering.md#turn-raw-pixel-surfaces-into-xaml-image-sources),
[Give a headless library a drawing facade over SkiaSharp](../BLUEPRINTS-GraphicsAndRendering.md#give-a-headless-library-a-drawing-facade-over-skiasharp)
and [Combine selection polygons with the CodeBrix PolygonTools library](../BLUEPRINTS-GraphicsAndRendering.md#combine-selection-polygons-with-the-codebrix-polygontools-library).

### Testing libraries that consume the platform

The single most valuable line in the test projects is
`CodeBrixRuntimeIdentifier=skia`, and the comment above it in
`tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj` explains the
trap it avoids: the published CodeBrix.Platform reference assemblies compile
fine and throw on first use, and an application head gets the real
implementations swapped in automatically while a plain test project does not. The
same project also adds, by hand, the native libraries a head would have received
from its runtime package.

The rest is convention. Test projects build as executables because xUnit v3 test
projects are self-executing; each library exposes its internals to its own test
project through an `InternalsVisibleTo.cs`; a `[ModuleInitializer]` in each test
assembly that touches the engine points the process-global settings store at a
throwaway temp folder, which a fixture would be too late to do. The effects tests
are golden-image comparisons with a small per-channel tolerance that report the
first differing pixels with both values and the delta, and they resolve their
dependencies from a mock service provider so no real chrome, workspace or palette
is needed.

Read `Pinta.Brix.Engine.Tests.csproj`, then `TestSettingsStore.cs`, then
`tests/libs/Pinta.Brix.Effects.Tests/Utilities.cs`. See [Build a test project against real CodeBrix Platform assemblies](../BLUEPRINTS-Testing.md#build-a-test-project-against-real-codebrix-platform-assemblies),
[Add the native assets a head would have supplied](../BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied),
[Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project),
[Point a process-global store at a throwaway folder in tests](../BLUEPRINTS-Testing.md#point-a-process-global-store-at-a-throwaway-folder-in-tests)
and [Compare rendered images pixel by pixel](../BLUEPRINTS-Testing.md#compare-rendered-images-pixel-by-pixel).

### Startup, in order

`App.xaml.cs` is short and every line in it is ordered on purpose: set the
default text font family, open the settings store, read the remembered window
size into the platform's preferred launch size, create the resolver and turn off
design mode, then `InitializeComponent()`. After the window exists, the engine
bootstrap installs the resource service and the timer service and calls the three
registration entry points for file formats, effects and tools - the timer service
needs the window's dispatcher queue, which is why it is not earlier. Each head's
`Program.cs` is a handful of lines and differs only in the head extension
method, except WinWpfSkia, which type-tests the built host and selects a
software render surface.

Read `src/Pinta.Brix.UI/App.xaml.cs` top to bottom, then any head's
`Program.cs`, then `src/Pinta.Brix.Core/Helpers/HostHelper.cs`. See [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend),
[Bootstrap the application in the App constructor](../BLUEPRINTS-AppStructureAndStartup.md#bootstrap-the-application-in-the-app-constructor),
[Supply a generic host builder to SimpleServiceResolver](../BLUEPRINTS-AppStructureAndStartup.md#supply-a-generic-host-builder-to-simpleserviceresolver),
[Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer),
[Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks),
[Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds),
[Force the software render surface on the WinWpfSkia head](../BLUEPRINTS-AppStructureAndStartup.md#force-the-software-render-surface-on-the-winwpfskia-head),
[Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
and [Split a page code-behind into named partial files](../BLUEPRINTS-ViewsAndControls.md#split-a-page-code-behind-into-named-partial-files).

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder records the
third-party content bundled here and the licenses it comes under: Pinta.Brix is a
port of Pinta, whose MIT license is reproduced in full, and substantial portions
of the source in this folder are derived from it. The notices also cover the icon
artwork embedded under `src/libs/Pinta.Brix.Controls/Assets/icons` - contributed
icons under Pinta's own license and listed in the `pinta-icons.md` file beside
them, the Silk icon set and the Fugue icon set under Creative Commons
Attribution, and the stock command and tool icons carried over from the program
Pinta was itself ported from. [license-pdn.txt](license-pdn.txt) is that earlier
program's license text, copied verbatim: MIT with a single exception covering its
logo and program-icon artwork, which is not bundled here, so the exception
applies to nothing in this folder. Ported source files keep their upstream
copyright headers pointing at that file.

Third-party code dependencies arrive as NuGet packages carrying their own
notices, so they are not repeated. Nothing is downloaded at run time, and the
only bundled font is the one the font package supplies.

## License

Pinta.Brix is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
