# JustBetweenUs PlayTests

This is a test head for the CodeBrixPlatform application. It references
`JustBetweenUs.Core`, imports the existing `.UI` shared project, and consumes
`CodeBrix.Platform.PlayTest.ApacheLicenseForever` from a local NuGet feed.
The test runner is xUnit v3 through Microsoft.Testing.Platform on .NET 10.
Nullable annotations and implicit usings are disabled.

From the `JustBetweenUs` root:

```sh
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

With local preview `.17` or later, show the preview from any OS using:

```sh
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release --headed
```

From the `JustBetweenUs.PlayTests` directory, your shorter command works too:

```sh
dotnet test JustBetweenUs.PlayTests.csproj --nonheadless
```

`--nonheadless` is an alias for `--headed`. `--headless` forces the reverse mode.
These options override `CODEBRIX_PLAYTEST_HEADED`, and the package registers them
automatically through Microsoft.Testing.Platform. No additional project or fixture
settings are required. Explicit `PlayTestOptions.Headless` in code remains the
highest-priority choice; this fixture leaves it unset. Do not combine headed and
headless flags. Use these options on the PlayTests project; other test projects in
the full solution do not necessarily recognize them.

Local preview `.18` adds case-insensitive `--theme=dark|light` and
`--orientation=portrait|landscape`. These override environment and `.csproj`
preferences; explicit fixture options and requirements applied by tests/theory rows
still win. `PlayTestOptions.Theme` can explicitly select the fixture's simulated
system theme. The app or an individual page can still set its own `RequestedTheme`.

To save an automatic screenshot history, create an empty destination first:

```sh
mkdir -p "$HOME/Temp/JustBetweenUs PlayTest run 2026-09-30"
dotnet test JustBetweenUs.PlayTests.csproj --nonheadless --theme=dark --orientation=portrait \
  --screenshotfolder="$HOME/Temp/JustBetweenUs PlayTest run 2026-09-30"
```

`~/` is also accepted inside the quoted option value. The folder must already exist
and be completely empty, including hidden files/subfolders. A repeated run requires
a different empty folder. The package handles recording automatically; UI test
methods and fixtures need no recording code. It works in both preview modes.

The folder receives `screenshot-index.json`, plus PNGs below
`<namespace after JustBetweenUs.PlayTests>/<class>/<method>/`. Theory rows add
`test-case-1/`, `test-case-2/`, etc., numbered in execution order. Each UI test has
`screenshot-start.png` after setup, numbered screenshots after PlayTest UI operations
and completed waits/assertions, and `screenshot-final.png` before cleanup even if
the test fails. The index includes IDs and row arguments, local timestamps/offsets,
outcomes, versions, system/app preferences, image dimensions and source lines when
PDBs provide them. Configuration-only tests without a running UI receive metadata
and an explanation instead of fabricated screenshots; skipped bodies have no entry.

Recording follows PlayTest API calls, not arbitrary C# statements or every animation
frame. Keep normal PlayTest waits/assertions for asynchronous results. The index is
updated after each screenshot/test, so an interrupted run retains partial results.
See the [complete recording contract](../../../../../CodeBrix.Platform/src/Platform.UI.Runtime.Skia.PlayTest/README.md#automatic-screenshot-recording)
in the sibling Platform repository (or the PlayTest package README).

Validated on this Intel Mac with local preview `.17`: `dotnet test
JustBetweenUs.PlayTests.csproj --nonheadless` passed all 80 cases in Debug while
`CODEBRIX_PLAYTEST_HEADED=0`; the Cocoa preview opened with the default 250 ms delay.

To watch the application on Linux or macOS:

```sh
CODEBRIX_PLAYTEST_HEADED=1 dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

In PowerShell:

```powershell
$env:CODEBRIX_PLAYTEST_HEADED = "1"
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

The headed window is a live, view-only canvas. Resize or move it normally;
physical keyboard/mouse input does not operate the app. The tests operate the
actual controls. Unset `CODEBRIX_PLAYTEST_HEADED` (or use `0`) for headless runs.
Headed runs default to 250 ms between actions; headless runs default to zero.
Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed.
An explicit `PlayTestOptions.SlowMo` in code takes precedence over the environment;
the fixture leaves it unset so the head applies these preferences.

The fixture preference is resolved in this order: explicit `Orientation` in
`AppFixture.InitializeAsync()`, `--orientation`, `CODEBRIX_PLAYTEST_ORIENTATION`, the `.csproj`
`CodeBrixPlayTestPreferredOrientation` property, then Landscape. The sample leaves
the code override unset and declares the project preference as Landscape.
Set `CODEBRIX_PLAYTEST_ORIENTATION=portrait` for a portrait default, or put this
in the test `.csproj` to make portrait the normal preference without an environment
variable:

```xml
<CodeBrixPlayTestPreferredOrientation>Portrait</CodeBrixPlayTestPreferredOrientation>
```

The simulated OS theme defaults to Light, independently of your desktop theme.
To run in Dark mode, set `CODEBRIX_PLAYTEST_THEME=dark` or put this in the test
`.csproj`:

```xml
<PropertyGroup>
  <CodeBrixPlayTestPreferredTheme>Dark</CodeBrixPlayTestPreferredTheme>
</PropertyGroup>
```

Explicit `PlayTestOptions.Theme` takes priority over `--theme`, which overrides
the environment and project preferences. The environment variable takes precedence over the project preference; setting
it to `light` overrides a Dark project preference. Names are case-insensitive.
An unset or empty variable falls back to the project, then Light; invalid values
are errors. The sample explicitly declares Light as its project preference.
PlayTest applies the simulated OS theme before app construction, including system
colors and theme resources, and keeps it fixed for the process. The app may still
explicitly choose its own theme. Preview, screenshots and headless tests share
the same rendering, and orientation changes leave the theme unchanged.

For a visible Dark run on Linux or macOS:

```sh
CODEBRIX_PLAYTEST_THEME=dark CODEBRIX_PLAYTEST_HEADED=1 dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

In PowerShell, set `$env:CODEBRIX_PLAYTEST_THEME = "dark"` before running the same
`dotnet test` command. `ThemePreferenceTests.cs` covers precedence and invalid
values; `ThemeTests.cs` checks the startup theme, system colors, theme resources,
rendered PNG pixels, and an explicit page theme override. Theme screenshots are
saved as `TestResults/PlayTest/JustBetweenUs-Light.png` or `JustBetweenUs-Dark.png`.

`SLOWMO`, theme/orientation preferences and `HEADED` are interpreted by PlayTest
itself. Each test without an explicit
requirement starts at the fixture preference, even after an opposite-orientation
test. Landscape is 1920×1080; portrait is 1080×1920, both at scale 1.
The preview's proportions follow the fixture preference and stay stable when
test orientation changes. Opposite-orientation frames have black bars at the
sides or above/below. Manual window resizing still works. Screenshots capture
the actual virtual screen without preview bars.

For a Wayland-only preview set `SDL_VIDEODRIVER=wayland`; for an X11 preview use
`SDL_VIDEODRIVER=x11`. SDL3 otherwise chooses an available display backend.

`OrientationTests.cs` demonstrates opt-in requirements at both levels:

```csharp
[Fact]
[PlayTestOrientation(ScreenOrientation.Portrait)]
public async Task Method_can_require_portrait() { /* test */ }

[Theory]
[PlayTestOrientation(ScreenOrientation.Landscape)]
[InlineData("portrait case", Traits = new[] { "PlayTestOrientation", "Portrait" })]
[InlineData("landscape case")] // inherits the method requirement
public async Task Example(string scenario) { /* test */ }
```

The row trait overrides the method attribute, which overrides the fixture
preference. `ApplicationTests.InitializeAsync()` reads the executing xUnit test's
traits (including deferred theory rows) and passes the resolved requirement to
`AppFixture.ResetAsync()`. That installs a fresh page after setting orientation.
The hook is required: the runner-neutral attribute does not run setup by itself.
The orientation tests check layout, display information, PNG dimensions, actual
encryption/decryption, and restoration of the default after repeated changes.
`OrientationPreferenceTests.cs` checks preference precedence and invalid values.

The fixture launches one application per test process and installs a fresh
`MainPage` before each test. Parallelization is disabled. Tests assert returned
plaintext, error/information dialogs, command states, bound mode selection and
isolated clipboard contents. Randomized ciphertext is never compared to a fixed
string. Contract checks also verify strict locators, lazy re-resolution, blocked
clicks, read-only values, missing elements, outcome probes and failure diagnostics.
Intentional timeout tests produce diagnostic PNGs; those files do not indicate
an unexpected suite failure. A normal app screenshot is saved as
`TestResults/PlayTest/JustBetweenUs.png`, relative to the test process directory.

The temporary package feed defaults to the sibling
`CodeBrix.Platform/nugets/PlayTest` directory. To rebuild a fresh preview set:

```sh
# Run in the CodeBrix.Platform repository; choose an unused prerelease version.
python3 build/pack-playtest-preview.py --version 1.0.272.1-playtest.18
```

Then pass `-p:PlayTestPackageVersion=1.0.272.1-playtest.18` to `dotnet test` or update
the project default. Use `-p:PlayTestPackageFeed=/path/to/feed` for another checkout
layout. Copy the produced packages (core, base Skia runtime, PlayTest, and WebView)
to the other machine's feed. The new head requires the matching core's friend
assembly declarations. The regular Platform release build also includes PlayTest
in its normal package set.

File/folder pickers accept scripted paths or explicit cancellation without showing
native UI. The prototype does not automate other native OS dialogs, GPU-only
controls or multiple application windows. Clipboard tests are deliberately isolated from the desktop
clipboard. See the shared sample guidance for Windows and Wayland validation;
macOS validation is recorded in the shared sample guidance.

This project uses local package set `1.0.272.1-playtest.18`. Its 80 cases exercise
JustBetweenUs and PlayTest configuration. The 15 generic picker/control cases
have moved to `samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests`
in the CodeBrix.Platform repository. That dedicated demo owns their UI; this
suite no longer substitutes unrelated controls for the JustBetweenUs page.
See [shared sample guidance](../../../../PlayTestSupport/README.md) for picker
examples and the additional application suites.

All 80 cases pass with `.8` in visible/dark/landscape mode and `SLOWMO` unset,
using the head's new 250 ms default.

On Intel macOS 15.8, 2026-09-30, all 80 cases passed with `.16` in each of
headless/light landscape, headless/light portrait, headed/dark landscape and
headed/dark portrait: 320 executions, zero failures or skips. Headed runs used
the Cocoa SDL driver and the default 250 ms delay. TextBox editing now follows
PlayTest's Control-based model on macOS, including select-all, isolated clipboard
paste and whole-word deletion; normal desktop hosts keep their native shortcuts.

Theme support validated on Linux x64 with local package set `1.0.272.1-playtest.4`:

- All 80 tests pass in the visible X11 landscape preview with the environment
  theme set to Dark and 500ms action pauses, including mixed-orientation cases.
- All 80 tests pass headlessly with the default Light preference and no theme
  environment variable, and with a Dark project preference and no theme variable.
- An additional application launch verifies that an explicit Light environment
  value overrides the compiled Dark project preference, including startup,
  system colors, theme resources, PNG pixels and encryption/decryption.
- Configuration tests cover absent/empty settings, case-insensitive values,
  precedence and invalid values. MSBuild accepts mixed-case Dark and rejects
  invalid project values or a configured preference with GenerateAssemblyInfo=false.
- The system-color preference is available before constructing the app; the
  framework applies Application.RequestedTheme at launch, after construction.
- The produced core package passes its dependency gate with zero errors and
  zero warnings. The project and default build remain Light and Landscape.

Earlier orientation validation with local package set `1.0.272.1-playtest.3`:

- All 63 tests pass with the live X11 preview using both landscape and portrait
  defaults, including mixed-orientation cases, at 200ms action pauses.
- All 63 tests pass headlessly with a Portrait project preference and no
  orientation environment variable. A separate launch verified that an explicit
  Landscape environment setting overrides that compiled Portrait preference;
  its application screenshot is 1920×1080.
- The suite includes both method-level requirements, four explicit row
  requirements, rows inheriting a method requirement, and deferred theory rows.
- Repeated orientation changes preserve correct layout, display information,
  screenshot dimensions and input behavior. Display orientation events and
  retaining existing page state are also checked.
- Invalid project values are rejected by the package's MSBuild target.
- The preview integration check verifies all eight frame/window combinations
  across repeated switches. Window sizes remain 960×540 and 540×960 respectively;
  opposite-orientation content is centered with black bars. Its dummy-driver
  checks cover clean EOF, invalid dimensions, truncated headers and truncated
  pixel data.
- The dependency gate accepts the produced core package with zero errors and
  zero warnings. NuGet packing retains the core package's existing NU5100
  warnings for its deliberately separate Skia runtime assembly folder.

To repeat the preview pixel checks, run from the sibling `CodeBrix.Platform`
repository after building these tests (requires X11, xdotool, ImageMagick's
`import`, and Pillow):

```sh
python3 build/test-scripts/playtest-preview-orientation.py \
  --test-output ../CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/bin/Release/net10.0 \
  --artifacts /tmp/playtest-preview-orientation
```


With local preview `.18`, the Intel macOS Debug run of all 80 tests also passed
using `--nonheadless --theme=DaRk --orientation=PoRtRaIt --screenshotfolder=...`
while the environment requested Light/Landscape. It produced 638 validated PNGs
and a complete index in about 3m45s. No UI test changes were needed for recording;
the environment-preference unit-test helpers now also isolate the CLI settings
while checking their deliberately selected fallback values.
