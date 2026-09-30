# Sample PlayTests

The five new projects import `CodeBrix.Sample.PlayTests.props` for the common head/test packages and link `SampleFixture.cs` for launch/reset behavior. Each project directly references SilverAssertions and explicitly disables nullable and implicit usings. It imports its application's shared UI and references the existing Core project.

| Application | Tests | Main coverage |
| --- | ---: | --- |
| [GitHubIssueFinder](../GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/README.md) | 9 | Search, checkbox, keyboard shortcuts, error/retry, schemes |
| [KenneyAssetBrowser](../KenneyAssetBrowser/tests/KenneyAssetBrowser.PlayTests/README.md) | 8 | Folder picker, ZIP catalog, images, zoom, text viewer |
| [PolyHavenBrowser](../PolyHavenBrowser/tests/PolyHavenBrowser.PlayTests/README.md) | 8 | API catalog, thumbnails, search, sort, folder picker |
| [RedisSetupTool](../RedisSetupTool/tests/RedisSetupTool.PlayTests/README.md) | 12 | Navigation, daemon snapshot, filtering, recovery, console picker |
| [WikipediaPublisher](../WikipediaPublisher/tests/WikipediaPublisher.PlayTests/README.md) | 14 | Embedded browser, pointer/keyboard input, save picker, publishing workflow, HTTP fixture |

JustBetweenUs keeps its existing fixture and 80 application/configuration cases. Its 15 generic picker/control cases now live in `samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests` in the CodeBrix.Platform repository. That dedicated application owns the demonstration UI. The five suites above exercise their own applications' real screens; their controlled service/data fixtures support those actual workflows. All fixtures serialize tests. Each test receives a fresh page; application services are shared and reset by the fixture. Services are replaced at the application's existing DI boundary, before view models are constructed. Pointer/keyboard actions run through PlayTest; assertions inspect UI and service outcomes.

## Local package setup

These projects use the unpublished local preview family, currently `1.0.272.1-playtest.12`. From the sibling `CodeBrix.Platform` checkout:

```bash
python3 build/pack-playtest-preview.py --version 1.0.272.1-playtest.12
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

These are initial UI suites, not comprehensive integration tests. GitHub, Poly Haven, Docker and PDF-rendering services use controlled fixtures. Kenney loads real local ZIP/image/text data. Wikipedia uses the real WPE browser on Linux or Edge WebView2 on Windows against a loopback server; macOS PlayTest WebViews remain unsupported. Windows requires the installed Edge WebView2 runtime. GPU-only 3D controls, real audio playback, live Redis terminal sessions and actual PDF generation are not covered. PlayTest carries packaged ICU for Windows/macOS text initialization; browser support remains in the application's optional WebView add-in.

Offscreen runs do not need a display server. The validation records below identify the hosts and configurations actually exercised; macOS previews still need host validation.


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
