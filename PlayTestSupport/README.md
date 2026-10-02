# Sample PlayTests

PlayTest suites drive the real screens of several sample applications through the `CodeBrix.Platform.PlayTest.ApacheLicenseForever` preview head, restored from nuget.org like every other package in this repository. Pointer and keyboard actions run through PlayTest; assertions inspect UI and service outcomes.

The GitHubIssueFinder, KenneyAssetBrowser, PolyHavenBrowser, RedisSetupTool and WikipediaPublisher suites import `CodeBrix.Sample.PlayTests.props` for the common head/test packages and link `SampleFixture.cs` for launch/reset behavior. Each project directly references SilverAssertions and explicitly disables nullable and implicit usings. It imports its application's shared UI and references the existing Core project. JustBetweenUs keeps its own fixture and project file with the same package set.

| Application | Main coverage |
| --- | --- |
| [JustBetweenUs](../JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/README.md) | Encrypt/decrypt workflows, dialogs, command states, text editing and isolated clipboard, orientation, theme, PlayTest contract checks |
| [GitHubIssueFinder](../GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/README.md) | Search, checkbox, keyboard shortcuts, error/retry, schemes |
| [KenneyAssetBrowser](../KenneyAssetBrowser/tests/KenneyAssetBrowser.PlayTests/README.md) | Folder picker, ZIP catalog, images, zoom, text viewer |
| [PolyHavenBrowser](../PolyHavenBrowser/tests/PolyHavenBrowser.PlayTests/README.md) | API catalog, thumbnails, search, sort, folder picker |
| [RedisSetupTool](../RedisSetupTool/tests/RedisSetupTool.PlayTests/README.md) | Navigation, daemon snapshot, filtering, recovery, console picker |
| [WikipediaPublisher](../WikipediaPublisher/tests/WikipediaPublisher.PlayTests/README.md) | Embedded browser, pointer/keyboard input, composited pixels across resizes, save picker, publishing workflow, HTTP fixture |

The suites exercise their own applications' real screens; their controlled service/data fixtures support those actual workflows. All fixtures serialize tests. Each test receives a fresh page; application services are shared and reset by the fixture. Services are replaced at the application's existing DI boundary, before view models are constructed.

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

These are initial UI suites, not comprehensive integration tests. GitHub, Poly Haven, Docker and PDF-rendering services use controlled fixtures. Kenney loads real local ZIP/image/text data. Wikipedia uses the real WPE browser on Linux or Edge WebView2 on Windows against a loopback server; macOS uses the system WKWebView in the optional add-in's offscreen helper. Windows requires the installed Edge WebView2 runtime. GPU-only 3D controls, real audio playback, live Redis terminal sessions and actual PDF generation are not covered. PlayTest carries packaged ICU for Windows/macOS text initialization; browser support remains in the application's optional WebView add-in.

Headless runs show no application window. The macOS browser helper uses AppKit
and needs a logged-in desktop session. PlayTest screenshots are virtual-screen PNGs
in both preview modes and need no desktop Screen Recording permission.
