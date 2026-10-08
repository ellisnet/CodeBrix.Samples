# Sample PlayTests

PlayTest suites drive the real screens of several sample applications through the `CodeBrix.Platform.PlayTest.ApacheLicenseForever` head, restored from nuget.org like every other package in this repository. Pointer and keyboard actions run through PlayTest; assertions inspect UI and service outcomes.

The suites import `CodeBrix.Sample.PlayTests.props` for the common head/test packages; it also links `SampleFixture.cs`, which most suites use for launch/reset behavior. The suites around a process-wide engine or static application state (BrixInvaders, GameEngineMusicDemo, Pinta.Brix) launch once and reset the live page with their own fixture. Each project directly references SilverAssertions and explicitly disables nullable and implicit usings. It imports its application's shared UI and references the existing Core project. JustBetweenUs keeps its own fixture and project file with the same package set.

| Application | Main coverage |
| --- | --- |
| [JustBetweenUs](../JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/README.md) | Encrypt/decrypt workflows, dialogs, command states, text editing and isolated clipboard, orientation, theme, PlayTest contract checks |
| [BrixInvaders](../BrixInvaders/tests/BrixInvaders.PlayTests/README.md) | Running engine canvas from first layout, splash to title, Kenney assets and CPU tier, seeded throwaway settings store, music/link seams, key events reaching the canvas, mouse input and attract mode, rendered frames, letterboxing in both orientations |
| [CodeBrixVideoTool](../CodeBrixVideoTool/tests/CodeBrixVideoTool.PlayTests/README.md) | Open/remove, real player on generated clips (frames, transport, chapters, captions, scrubber, volume), MP4 notice, conversion setup, save picker, progress, cancel, run notes, failures, busy states, portrait, forced dark palette |
| [DRAKON.Brix](../DRAKON.Brix/tests/DRAKON.Brix.PlayTests/README.md) | Hosted Tk editor boot and intro, preloaded document, scripted open/save pickers, File/Edit menus, diagram list, icon text editing and undo, isolated clipboard, wheel, injected quit action, recent files in an isolated home, orientation, classic Tk surface under both themes |
| [GameEngineMusicDemo](../GameEngineMusicDemo/tests/GameEngineMusicDemo.PlayTests/README.md) | Running engine canvas readout, transport, immediate/bar-quantized crossfades and cancel, global pause, bus/stem/tempo sliders, stem and MIDI layer fades, jump points, ducking, playlist, orientation |
| [GitHubIssueFinder](../GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/README.md) | Search, helper text, grouped/streamed results, row details, progress and quota pills, empty/error/rate-limit states, cancel and keyboard shortcuts, link opening through a recording opener, settings restore, schemes and system theme, portrait scrolling |
| [InannaRosette](../InannaRosette/tests/InannaRosette.PlayTests/README.md) | Draw/shuffle/auto-lay/clear, drag and snap, reversal, lore panel, interpretation and stale badge, save/open reading, PDF report, seeded blessing, failure dialogs, theme, portrait |
| [KenneyAssetBrowser](../KenneyAssetBrowser/tests/KenneyAssetBrowser.PlayTests/README.md) | Folder picker, ZIP catalog and bundle switching, remembered folder, catalog warnings, lazy grid loading, every viewer kind (image zoom/wheel, spritesheet, SVG, font, Tiled map, text, no-preview), License/error/3D-unavailable dialogs, model facts and animation, audio transport via a fake bridge, viewer layout in both orientations |
| [MediaPlayerDemo](../MediaPlayerDemo/tests/MediaPlayerDemo.PlayTests/README.md) | Address loading, Load enablement, invalid-address status, source replacement, stretch picker, recording playback engine (libVLC add-in excluded), theme pixels, portrait and rotation |
| [NotionDocumentCreator](../NotionDocumentCreator/tests/NotionDocumentCreator.PlayTests/README.md) | Connect, lazy page tree, page checkboxes, preview, load whole tree, save picker, page size, create request/progress/result, overwrite cancel, errors, portrait |
| [PainDiagram](../PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/README.md) | Layer selection, real-pointer drawing on the canvas, Clear confirmation, save picker with replace/cancel/failure, stroke pixels on screen and in the saved PNG, orientation, theme |
| [PalmVisualizer](../PalmVisualizer/tests/PalmVisualizer.PlayTests/README.md) | Camera discovery and failures, auto-select and camera switching with newest-switch-wins, first-frame gate, Camera/Visualize mode switching, tracker and visualizer-session lifecycle, mirrored open palms, mirrored preview pixels, orientation, theme (fakes replace the camera, tracker and game engine) |
| [PdfSideBySide](../PdfSideBySide/tests/PdfSideBySide.PlayTests/README.md) | File picker and cancel, rendered pages, both/adjust-right navigation, zoom ladder and image sizing, per-pane pan, duplicate/unreadable/startup error dialogs, command-line preload, portrait |
| [PicoScope.Brix](../PicoScope.Brix/tests/PicoScope.Brix.PlayTests/README.md) | Simulated scope startup, capture/stream commands, range and channel visibility on the chart, signal generator validation, no-scope/open-failure/hardware paths, chart zoom and trace pixels, portrait |
| [Pinta.Brix](../Pinta.Brix/tests/Pinta.Brix.PlayTests/README.md) | Canvas drawing and pixels, undo/history pad, layers, open/save/close prompts and pickers, adjustments and floating effect dialogs, clipboard, selections, image dialogs, zoom, palette, tool shortcut keys, splitter, orientation, theme |
| [PolyHavenBrowser](../PolyHavenBrowser/tests/PolyHavenBrowser.PlayTests/README.md) | API catalog, thumbnails, load failures, search, sort orders, lazy scrolling, folder picker, model downloads with progress/errors/retry, Model View details and orientation layout, rendering-unavailable dialog, real PDF one-sheet save/cancel, fixed dark palette |
| [PolyHavenBrowser_viewer_only](../PolyHavenBrowser_viewer_only/tests/PolyHavenBrowser.ViewerOnly.PlayTests/README.md) | Texture/HDRI/model sample switching, busy state, download failure and retry, cache reuse, rendering-engine switch and unavailable-engine alert, canvas drag/zoom, real HDRI panorama pixels, reset cancelling a download, orientation re-render, accent highlight under either theme |
| [RedisSetupTool](../RedisSetupTool/tests/RedisSetupTool.PlayTests/README.md) | Navigation and dashboard cards, container detail tabs and lifecycle, console tabs and typed input, instance cards and Verify, create flow with progress/failure/cancel, image tools, networks/volumes, prunes and event stream, dark palette, portrait, unreachable-daemon recovery |
| [SimpleCbxVideoPlayer](../SimpleCbxVideoPlayer/tests/SimpleCbxVideoPlayer.PlayTests/README.md) | Real AV1 playback of the bundled 720p clips: transport, keyboard seeks, end of clip, every container, drawn and matching frames, render-path status and GPU-only failure, disabled LUT panel, portrait pillarbox/layout, theme |
| [WebcamPainter](../WebcamPainter/tests/WebcamPainter.PlayTests/README.md) | Fake camera and hand tracker: camera discovery, start failure and switching, mirrored preview and self-view pixels, Paint Mode states, palm-driven strokes and crosshair, highlighter colours and ink pixels, save picker with replace/cancel/failure, Back and Clear confirmations, orientation, theme |
| [WebcamViewer](../WebcamViewer/tests/WebcamViewer.PlayTests/README.md) | Scripted camera service: discovery, auto-start, start failure, camera switching, audio monitor, Photo enablement, folder picker, PNG pixels, live canvas pixels and letterboxing, theme, portrait |
| [WikipediaPublisher](../WikipediaPublisher/tests/WikipediaPublisher.PlayTests/README.md) | Embedded browser, pointer/keyboard input, composited pixels across resizes, article vs non-article publish rules, search escaping, save picker and its errors, page sizes, overwrite confirm/cancel, busy state and status, long-path layout, theme, HTTP fixture |

The suites exercise their own applications' real screens; their controlled service/data fixtures support those actual workflows. All fixtures serialize tests. Each test receives a fresh page, except in the launch-once suites, which reset the live page; application services are shared and reset by the fixture. Services are replaced at the application's existing DI boundary, before view models are constructed.

## Running

From the application's root directory, substituting its name:

```sh
dotnet test --project tests/Application.PlayTests/Application.PlayTests.csproj -c Release
```

The projects are also included in their application solutions. Run headed projects sequentially to keep one preview visible at a time.

The PlayTest package registers its command-line switches in these Microsoft.Testing.Platform test projects on Windows, macOS and Linux: `--headed` (alias `--nonheadless`) and `--headless` select the preview mode, `--theme=dark|light` and `--orientation=portrait|landscape` set the simulated preferences, and `--screenshotfolder="/path/to/an/empty folder"` records virtual-screen PNGs plus a `screenshot-index.json` for every executed test. No extra project properties or fixture code are needed. Use these switches on PlayTest projects only; unrelated test projects in a solution may reject them. The full consumer guide, including precedence rules and the recording layout, is the package's `AGENT-README.txt`, which ships inside the package (in the CodeBrix.Platform repository it lives at `src/Platform.UI.Runtime.Skia.PlayTest/AGENT-README.txt`).

```sh
dotnet test --project GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/GitHubIssueFinder.PlayTests.csproj -c Release --headed
```

## Windows runner

From the CodeBrix.Samples root:

```powershell
python PlayTestSupport/run-windows-playtests.py
```

The runner discovers every `*.PlayTests.csproj` in this repository and runs them sequentially in headless/light landscape and portrait, then headed/dark landscape and portrait, against the packages restored from nuget.org. Headed runs use the Windows SDL driver and leave SlowMo unset for the normal 250 ms delay. Each run must report at least one passing test and zero failures/skips; failures do not prevent other suites from running. Logs and a JSON summary go in a timestamped directory below `PlayTestSupport/TestResults` (ignored by Git).

Use `--suites WikipediaPublisher`, `--configuration headed-dark-portrait`, `--no-build`, or `--slowmo 0` for focused checks. `--output` overrides the log directory.

## macOS runner

From the CodeBrix.Samples root:

```sh
python3 PlayTestSupport/run-macos-playtests.py
```

This shares discovery, logging and result checks with the Windows runner, runs the same four configurations, and forces the Cocoa SDL driver for visible previews. It supports the same command-line options. WikipediaPublisher uses the real system WKWebView; no browser installation is needed. The helper uses a nonpersistent data store and no visible native window. If the WebView package's bundled helper needs compiling, Apple's command-line tools do so during the first test build.

## Preferences and preview

Landscape and Light are the defaults. Put an optional preference in the test project's PropertyGroup after the shared props import:

```xml
<CodeBrixPlayTestPreferredOrientation>Portrait</CodeBrixPlayTestPreferredOrientation>
<CodeBrixPlayTestPreferredTheme>Dark</CodeBrixPlayTestPreferredTheme>
```

`CODEBRIX_PLAYTEST_ORIENTATION=landscape|portrait` and `CODEBRIX_PLAYTEST_THEME=light|dark` override those project defaults. Explicit `PlayTestOptions.Orientation` overrides the orientation environment variable. The shared fixture supplies `ConfigurationAssembly` so metadata comes from the test project.

`CODEBRIX_PLAYTEST_HEADED=1` opens a view-only preview with a default 250 ms delay between actions. `CODEBRIX_PLAYTEST_SLOWMO` overrides that delay, including `0` for full speed; headless runs default to zero. An explicit `PlayTestOptions.SlowMo` in code wins over the environment. The head resolves these settings; the shared fixture leaves the delay unset. The window stays at the launch proportions and letterboxes changes. The app still sees 1920×1080 or 1080×1920, independent of preview resizing.

The shared fixture applies optional method and theory-row orientation requirements before constructing the test page:

```csharp
[Fact]
[PlayTestOrientation(ScreenOrientation.Portrait)]
public async Task Portrait_layout() { /* actions and assertions */ }

[Theory]
[InlineData("landscape", Traits = new[] { "PlayTestOrientation", "Landscape" })]
[InlineData("portrait", Traits = new[] { "PlayTestOrientation", "Portrait" })]
public async Task Both_layouts(string scenario) { /* actions and assertions */ }
```

Row requirement > method requirement > fixture launch preference. An undecorated test retains the current default behavior. Theme represents the simulated OS preference; apps such as GitHubIssueFinder can still choose their own palette in the UI.

## Pickers and controls

Tests queue responses before clicking the control that invokes a picker:

```csharp
Fixture.Application.FilePickers.EnqueueFolder(existingFolder);
Fixture.Application.FilePickers.EnqueueOpenFile(existingFile);
Fixture.Application.FilePickers.EnqueueOpenFiles(firstFile, secondFile);
Fixture.Application.FilePickers.EnqueueSaveFile(savePath);
// Explicit Cancel for the next folder picker:
Fixture.Application.FilePickers.EnqueueFolder(null);
```

The application's regular storage picker API consumes the response, with no picker window even in headed mode. Each kind has a FIFO queue; both open-file modes share one queue. Null cancels; multiple-file cancellation returns an empty list. Input files/folders and save-parent folders must exist. Saving returns an existing file intact or creates an empty placeholder at a new path. The fixture clears queues and request history between tests. An unqueued request throws a diagnostic error.

Checked-control actions include `CheckAsync()`, `UncheckAsync()` and `SetCheckedAsync(bool)`, with `Expect(locator).ToBeCheckedAsync()` for a retrying assertion. They click the control and verify its resulting state. `ScrollIntoViewIfNeededAsync()` reveals an attached element in a scrollable viewport before clicking; it does not materialize virtualized items.

## Current boundaries

These suites drive the applications' screens; they are not integration tests of the services behind them. GitHub, Notion, Poly Haven, Docker and Redis, cameras, hand tracking, the game engine in PalmVisualizer, and media probing and conversion use controlled fixtures. Kenney loads real local ZIP/image/text data. PDF documents, the bundled video clips and generated clips are really rendered and played, with sound on the default audio device where the application plays it. Wikipedia uses the real WPE browser on Linux or Edge WebView2 on Windows against a loopback server; macOS uses the system WKWebView in the optional add-in's offscreen helper. Windows requires the installed Edge WebView2 runtime. GPU-only 3D and shader surfaces, key input a game samples while a key is held, real cameras and live Redis terminal sessions are not covered. PlayTest carries packaged ICU for Windows/macOS text initialization; browser support remains in the application's optional WebView add-in.

Headless runs show no application window. The macOS browser helper uses AppKit
and needs a logged-in desktop session. PlayTest screenshots are virtual-screen PNGs
in both preview modes and need no desktop Screen Recording permission.
