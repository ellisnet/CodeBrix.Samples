# Sample PlayTests

The five new projects import `CodeBrix.Sample.PlayTests.props` for the common head/test packages and link `SampleFixture.cs` for launch/reset behavior. Each project directly references SilverAssertions and explicitly disables nullable and implicit usings. It imports its application's shared UI and references the existing Core project.

| Application | Tests | Main coverage |
| --- | ---: | --- |
| [GitHubIssueFinder](../GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/README.md) | 9 | Search, checkbox, keyboard shortcuts, error/retry, schemes |
| [KenneyAssetBrowser](../KenneyAssetBrowser/tests/KenneyAssetBrowser.PlayTests/README.md) | 8 | Folder picker, ZIP catalog, images, zoom, text viewer |
| [PolyHavenBrowser](../PolyHavenBrowser/tests/PolyHavenBrowser.PlayTests/README.md) | 8 | API catalog, thumbnails, search, sort, folder picker |
| [RedisSetupTool](../RedisSetupTool/tests/RedisSetupTool.PlayTests/README.md) | 12 | Navigation, daemon snapshot, filtering, recovery, console picker |
| [WikipediaPublisher](../WikipediaPublisher/tests/WikipediaPublisher.PlayTests/README.md) | 16 | Embedded browser, pointer/keyboard input, composited pixels across resizes, save picker, publishing workflow, HTTP fixture |

JustBetweenUs keeps its existing fixture and 80 application/configuration cases. Its 15 generic picker/control cases now live in `samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests` in the CodeBrix.Platform repository. That dedicated application owns the demonstration UI. The five suites above exercise their own applications' real screens; their controlled service/data fixtures support those actual workflows. All fixtures serialize tests. Each test receives a fresh page; application services are shared and reset by the fixture. Services are replaced at the application's existing DI boundary, before view models are constructed. Pointer/keyboard actions run through PlayTest; assertions inspect UI and service outcomes.

## Local package setup

These projects use the unpublished local preview family, currently `1.0.272.1-playtest.18`. From the sibling `CodeBrix.Platform` checkout:

```bash
python3 build/pack-playtest-preview.py --version 1.0.272.1-playtest.18
```

The default feed is `../../CodeBrix.Platform/nugets/PlayTest` relative to this support directory. Override `PlayTestPackageFeed` and `PlayTestPackageVersion` as MSBuild properties for another checkout/feed. Do not overwrite a previously restored preview version: build a new version and update the property instead. No package is published by this helper.

Run `dotnet test --project tests/Application.PlayTests/Application.PlayTests.csproj -c Release` from the application's root directory, substituting its name. The projects are also included in their application solutions. Run headed projects sequentially to keep one preview visible at a time.

## Windows validation runner

From the CodeBrix.Samples root, after packing the local preview:

```powershell
python PlayTestSupport/run-windows-playtests.py
```

The runner discovers every `*.PlayTests.csproj` in this repository and the sibling
CodeBrix.Platform samples. It runs them sequentially in headless/light landscape
and portrait, then headed/dark landscape and portrait. Headed runs use the Windows
SDL driver and leave SlowMo unset for the normal 250 ms delay. Each run must report
at least one passing test and zero failures/skips; failures do not prevent other
suites from running. Logs and a JSON summary go in a timestamped directory below
`PlayTestSupport/TestResults` (ignored by Git).

Use `--suites WikipediaPublisher`, `--configuration headed-dark-portrait`,
`--no-build`, or `--slowmo 0` for focused checks. `--platform-repo`, `--output`, and
`--version` override the checkout, log directory, and preview package version.

## macOS validation runner

From the CodeBrix.Samples root, after packing the local preview:

```sh
python3 PlayTestSupport/run-macos-playtests.py
```

This shares discovery, logging and result checks with the Windows runner. It runs
all seven suites sequentially in the same four configurations and forces the
Cocoa SDL driver for visible previews. It supports the same command-line options.
WikipediaPublisher uses the real system WKWebView; no browser installation is
needed. The helper uses a nonpersistent data store and no visible native window.
Packages built on macOS carry a universal helper. If a package was built elsewhere,
Apple's command-line tools compile its bundled helper during the first test build.

## Preferences and preview

Local preview `.17` and later automatically register `--headed`, its alias
`--nonheadless`, and `--headless` in these Microsoft.Testing.Platform test projects
on Windows, macOS and Linux. For example:

```sh
dotnet test --project GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/GitHubIssueFinder.PlayTests.csproj --headed
```

No extra `.PlayTests.csproj` properties or fixture code are needed. Precedence is
explicit `PlayTestOptions.Headless` in code, then command line, then
`CODEBRIX_PLAYTEST_HEADED`, then headless. Fixtures leave `Headless` unset so the
command line can control it. `--headed` and `--headless` cannot be combined; mode
selection does not change the environment variable. Use these switches on
PlayTest projects/modules; unrelated test projects in a solution may reject them.

Local preview `.18` also accepts `--theme=dark|light` and
`--orientation=portrait|landscape` (case-insensitive). CLI preferences override
both environment and project settings; explicit fixture options and requirements
applied by tests/rows retain priority. `PlayTestOptions.Theme` provides a fixture
OS-theme override, and explicit app/element `RequestedTheme` remains effective.

To record the UI automatically, add `--screenshotfolder="/path/to/an/empty folder"`.
The folder must already exist and be completely empty, including hidden files and
subfolders. Relative paths and a leading `~/` are supported. All current sample
projects use the supported xUnit runner and need no additional fixture/test code.
Each executed test gets a namespace/class/method folder (root namespace omitted);
theory rows add `test-case-1`, `test-case-2`, etc. PNGs are named
`screenshot-start.png`, `screenshot-1.png`, ..., `screenshot-final.png`.
The root `screenshot-index.json` maps tests/arguments/outcomes to images and includes
local timestamps/offsets, system/app versions and preferences, dimensions, operations,
and source lines when symbols provide them. It is updated throughout the run.

The start image follows setup; the final image precedes cleanup even on failure.
Numbered images follow PlayTest actions, evaluations, and completed waits/assertions,
not every C# statement or animation frame. Tests without a running UI get metadata
with an explanation; skipped bodies have no entry. Use a fresh destination for each
run/module, and keep shared-app tests serialized. These are virtual-screen PNGs in
both preview modes and do not require desktop Screen Recording permission.
See the [full recording guide](../../CodeBrix.Platform/src/Platform.UI.Runtime.Skia.PlayTest/README.md#automatic-screenshot-recording).

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

Headless runs show no application window. The macOS browser helper uses AppKit;
the macOS validation below was performed in a logged-in desktop session.
Screen Recording permission is needed only for the native preview pixel checker,
not for the application tests or their PlayTest screenshots.


## Validation in this checkout

The local `.7` package set was validated with all 47 new sample tests and the original 95-case JustBetweenUs suite. After moving the 15 generic control/picker cases to PlayTestDemo, all 80 remaining JustBetweenUs cases pass. PlayTestDemo's 19 cases (including four additional orientation examples) pass against framework source in landscape/light and visible portrait/dark. The five sample suites also pass with portrait/dark preferences. A visible X11 Wikipedia run passes all 10 cases, including pointer input into WPE and letterboxing for the forced-landscape row.

The `.8` package set added the headed 250 ms delay default. The 47 sample cases
at that stage passed headless, and JustBetweenUs's 80 cases passed visible/dark/landscape
with `SLOWMO` unset. PlayTestDemo's 38 UI/configuration cases passed headless,
headed with the default delay, and headed with an explicit zero.

These tests also caught and now cover two application fixes: Kenney reports unavailable 3D rendering only for the 3D viewer, and Wikipedia keeps its Select button reachable when the output path is long. Redis page disposal stops its refresh timer and event stream so fixture resets do not leave background work running. The head package's dependency IDs are unchanged from the earlier preview.

### Wayland validation, 2026-09-29

All seven suites were exercised in an LMDE 7 (Gigi) Wayland desktop session:

| Suite | Headless/light landscape | Headless/light portrait | Headed/dark landscape | Headed/dark portrait |
| --- | ---: | ---: | ---: | ---: |
| PlayTestDemo (Platform repository) | 38 | 38 | 38 | 38 |
| JustBetweenUs | 80 | 80 | 80 | 80 |
| GitHubIssueFinder | 9 | 9 | 9 | 9 |
| KenneyAssetBrowser | 8 | 8 | 8 | 8 |
| PolyHavenBrowser | 8 | 8 | 8 | 8 |
| RedisSetupTool | 12 | 12 | 12 | 12 |
| WikipediaPublisher | 12 | 12 | 12 | 12 |
| **Passing cases** | **167** | **167** | **167** | **167** |

Headed runs were sequential, with `SDL_VIDEODRIVER=wayland` and the default
250 ms action delay. Explicit method/theory-row orientation requirements remained
enabled. Samples used local package set `.8`; PlayTestDemo built from source.

The initial WikipediaPublisher headed/landscape run had five cascading navigation
failures. A focused probe demonstrated that its single-connection HTTP fixture
could block all requests behind an idle connection. The fixture now handles
connections independently, and two regression cases cover idle/partial requests
and shutdown. All four WikipediaPublisher configurations passed after that fix;
the other six suites passed unchanged in all configurations. The final matrix
above contains 668 passing cases and no skips. This validates the PlayTest SDL
preview on Wayland; it does not exercise the applications' normal Wayland heads.

### Windows validation, 2026-09-30

All discovered `.PlayTests.csproj` suites passed on Windows x64 (OS build 26300),
using .NET SDK 10.0.401 and Release builds. The six application suites restored
local preview `1.0.272.1-playtest.12`; PlayTestDemo built the framework from source.

| Suite | Headless/light landscape | Headless/light portrait | Headed/dark landscape | Headed/dark portrait |
| --- | ---: | ---: | ---: | ---: |
| PlayTestDemo (Platform repository) | 38 | 38 | 38 | 38 |
| JustBetweenUs | 80 | 80 | 80 | 80 |
| GitHubIssueFinder | 9 | 9 | 9 | 9 |
| KenneyAssetBrowser | 8 | 8 | 8 | 8 |
| PolyHavenBrowser | 8 | 8 | 8 | 8 |
| RedisSetupTool | 12 | 12 | 12 | 12 |
| WikipediaPublisher | 14 | 14 | 14 | 14 |
| **Passing cases** | **169** | **169** | **169** | **169** |

That is **676 passing test executions across 28 suite runs, zero failures and zero
skips**. Runs were sequential. Headed tests used `SDL_VIDEODRIVER=windows` and left
`CODEBRIX_PLAYTEST_SLOWMO` unset for the 250 ms default. Explicit test-row
orientation requirements remained enabled. The visible application previews were
also observed on the desktop.

Windows startup uses packaged ICU and an STA message pump. The optional WebView
add-in supplies the offscreen Edge provider. Two new WikipediaPublisher cases
exercise Tab/Enter link activation in both orientations; they caught a missing
DOM key identity in the Windows keyboard bridge, fixed in `.12`. Existing cases
also cover real pointer navigation, redirects and repeated page replacement.

Redis's fixture previously replaced IDockerManager but left the topology service
using a concrete DockerManager. It now replaces both interfaces and rejects any
attempt to construct a real DockerManager. All 12 cases pass with that guard;
Docker Desktop is not a prerequisite. The temporary startup-error file logging
used during diagnosis was removed.

The Platform repository's `build/test-scripts/playtest-preview-windows.ps1` also
passed eight native frame/orientation checks and four protocol checks. Both
960x540 and 540x960 preview windows retained their sizes through alternating
1920x1080 and 1080x1920 frames, with centered letterboxing. Clean EOF succeeded;
invalid dimensions and truncated input failed as expected. PNG evidence was
captured from the preview client areas.

This session's logs and JSON summary are in
`C:\Users\jerem\playtest-windows-matrix\20260930-064648-428056`; native-preview
evidence is in `C:\Users\jerem\playtest-windows-preview`. The shared runner above
reproduces the matrix. Local packing succeeded and its dependency gate reported
zero errors/warnings; the core package retains its existing NU5100 warnings for
intentionally non-lib runtime assemblies. No packages were published.

### macOS validation, 2026-09-30

All seven suites passed on Intel macOS 15.8 with .NET SDK 10.0.401 and Release
builds. The six application suites restored local preview
`1.0.272.1-playtest.16`; PlayTestDemo built the framework from source.

| Suite | Headless/light landscape | Headless/light portrait | Headed/dark landscape | Headed/dark portrait |
| --- | ---: | ---: | ---: | ---: |
| PlayTestDemo | 38 | 38 | 38 | 38 |
| JustBetweenUs | 80 | 80 | 80 | 80 |
| GitHubIssueFinder | 9 | 9 | 9 | 9 |
| KenneyAssetBrowser | 8 | 8 | 8 | 8 |
| PolyHavenBrowser | 8 | 8 | 8 | 8 |
| RedisSetupTool | 12 | 12 | 12 | 12 |
| WikipediaPublisher | 16 | 16 | 16 | 16 |
| **Passing cases** | **171** | **171** | **171** | **171** |

That is **684 passing test executions across 28 suite runs, zero failures and
zero skips**. Runs were sequential. Headed tests used `SDL_VIDEODRIVER=cocoa`
and left `CODEBRIX_PLAYTEST_SLOWMO` unset for the 250 ms default. Explicit
method/theory-row orientation requirements remained enabled. The visible
previews were also observed on this workstation.

The optional WebView add-in now provides an offscreen WKWebView helper. Wikipedia's
16 cases cover real pointer and keyboard navigation, redirects, repeated page
replacement, and browser pixels composited into Skia through orientation changes.
The helper uses a nonpersistent browser data store and closes when its page unloads.

This matrix also caught macOS TextBox using Command-key conventions while PlayTest
injected its documented Control-based shortcuts. The host now configures TextBox
to follow the virtual keyboard model; the normal desktop default is preserved.
JustBetweenUs covers select-all, isolated clipboard paste, whole-word deletion,
and resulting bindings/command state.

The Platform repository's `build/test-scripts/playtest-preview-macos.py` passed
eight native frame/orientation checks and four protocol checks. Both 960x540 and
540x960 preview client areas kept their sizes through alternating landscape and
portrait frames, with centered black letterboxing. Clean EOF succeeded; invalid
dimensions and truncated headers/pixels failed as expected. ScreenCaptureKit PNG
capture worked after Screen Recording permission was granted, without restarting
this terminal session. That permission is only needed for this pixel checker.

The helper's complete native source, shared build recipe, standalone project and
rebuild instructions live in `tools/MacOsWebViewHelper` in CodeBrix.Platform.
The standalone build and the package's source-only fallback both produced
universal x86_64/arm64 executables with zero warnings/errors. After organizing the
source into that tools folder, standalone output was byte-identical to the tested
`.16` helper; a fresh packaging check confirmed source, targets and executable were
included intact. Runtime execution was validated on Intel, not Apple Silicon.

This session's logs and JSON summary are in
`PlayTestSupport/TestResults/20260930-102837-416947`; native-preview PNGs and
summary are in `/private/tmp/playtest-macos-preview`. The macOS runner above
reproduces the matrix. Local packing and its dependency gate succeeded; the gate
reported zero errors/warnings. The core package retains its existing NU5100
warnings for intentionally non-lib runtime assemblies. No packages were published.

### Command-line preview validation, local preview .17

On the same Intel Mac, all seven suites accepted the new package-provided options:

| Suite | Command-line option | CODEBRIX_PLAYTEST_HEADED | Passing cases |
| --- | --- | --- | ---: |
| PlayTestDemo | `--headless` | default | 51 |
| PlayTestDemo | `--headed` | `0` | 51 |
| PlayTestDemo | `--nonheadless` | `0` | 51 |
| PlayTestDemo | `--headless` | `1` | 51 |
| JustBetweenUs | `--nonheadless` | `0` | 80 |
| GitHubIssueFinder | `--headless` | `1` | 9 |
| KenneyAssetBrowser | `--headless` | `1` | 8 |
| PolyHavenBrowser | `--headless` | `1` | 8 |
| RedisSetupTool | `--headless` | `1` | 12 |
| WikipediaPublisher | `--headless` | `1` | 16 |

These ten runs passed 337 test executions with zero failures or skips.
JustBetweenUs used the user's project-local command in Debug; the other suites
used Release. Headed runs used Cocoa and the default 250 ms action delay. The
demo's additional 13 cases cover mode precedence, unchanged environment values,
delay defaults, explicit fixture overrides, conflicting flags, and the actual
running application's selected mode. The options appeared in `dotnet test --help`.
Conflicting mode flags exited before tests; the direct executable reported why.

The package contains its MTP adapter source and build registration; no additional
consumer project properties, fixture changes or runtime test-runner dependency
were needed. The implementation uses platform-independent managed MTP APIs on
Windows, Linux and macOS. This CLI validation was executed on Intel macOS; the
earlier Windows/Wayland runtime records above predate these new switches.


### Theme, orientation and recording validation, local preview .18

On this Intel Mac on 2026-09-30, all seven suites ran with `--screenshotfolder`:

| Suite | Mode | CLI theme / orientation | Passed | PNGs |
| --- | --- | --- | ---: | ---: |
| PlayTestDemo (source) | headed | Light / Landscape | 61 | 194 |
| JustBetweenUs (Debug) | headed | Dark / Portrait | 80 | 638 |
| GitHubIssueFinder | headless | Dark / Portrait | 9 | 64 |
| KenneyAssetBrowser | headless | Dark / Portrait | 8 | 57 |
| PolyHavenBrowser | headless | Dark / Portrait | 8 | 41 |
| RedisSetupTool | headless | Dark / Portrait | 12 | 64 |
| WikipediaPublisher | headless | Dark / Portrait | 16 | 125 |

All **194 tests passed**, zero failures/skips. The **1,183 PNGs** were checked
against the indexes for signatures, dimensions, contained paths, start/numbered/final
naming, outcomes and resolved preferences. Environment theme/orientation values
opposed the command-line values in every run. Existing method/theory orientation
requirements continued to win. Configuration-only cases without a running UI have
metadata and notes; they do not fabricate screenshots.

Artifacts on this workstation are under
`/private/tmp/PlayTest screenshot validation czgtnt73/`, including
`validation-summary.json` and a `screenshot-index.json` for each suite. JustBetweenUs's
visible recording run took about 3m45s; PNG encoding adds overhead to the normal
headed action delay. Browser tests used real WKWebView rendering.

The Platform recording regression checker also passed: two theory rows, an
intentional failing test with a final PNG, nested namespace paths, row arguments,
source lines, theme changes, and nine rejected option/folder cases. Its 17 PNGs
and logs are in `/private/tmp/playtest-recording-validation18/`. Literal quoted
`~/` paths and discovery without writing artifacts were checked separately.
The demo additionally passed all 61 cases without recording, and its 20 UI cases
passed with headless recording in Dark/Portrait.

The implementation and checker use portable managed APIs on Windows/Linux/macOS.
Runtime validation of these new options and recording was performed on Intel macOS;
Windows/Linux runtime validation remains outstanding for this addition.
