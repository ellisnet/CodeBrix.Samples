# JustBetweenUs PlayTests

This is a test head for the CodeBrixPlatform application. It references
`JustBetweenUs.Core`, imports the existing `.UI` shared project, and consumes
`CodeBrix.Platform.PlayTest.ApacheLicenseForever` from nuget.org.
The test runner is xUnit v3 through Microsoft.Testing.Platform on .NET 10.
Nullable annotations and implicit usings are disabled.

From the `JustBetweenUs` root:

```sh
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

Show the preview from any OS using:

```sh
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release --headed
```

From the `JustBetweenUs.PlayTests` directory, the shorter command works too:

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

The package also accepts case-insensitive `--theme=dark|light` and
`--orientation=portrait|landscape`. These override environment and `.csproj`
preferences; explicit fixture options and requirements applied by tests/theory rows
still win. `PlayTestOptions.Theme` can explicitly select the fixture's simulated
system theme. The app or an individual page can still set its own `RequestedTheme`.

To save an automatic screenshot history, create an empty destination first:

```sh
mkdir -p "$HOME/Temp/JustBetweenUs PlayTest run"
dotnet test JustBetweenUs.PlayTests.csproj --nonheadless --theme=dark --orientation=portrait \
  --screenshotfolder="$HOME/Temp/JustBetweenUs PlayTest run"
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
The complete recording contract is in the PlayTest package's `AGENT-README.txt`,
which ships inside the package (in the CodeBrix.Platform repository it lives at
`src/Platform.UI.Runtime.Skia.PlayTest/AGENT-README.txt`).

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

File/folder pickers accept scripted paths or explicit cancellation without showing
native UI. PlayTest does not automate other native OS dialogs, GPU-only
controls or multiple application windows. Clipboard tests are deliberately isolated from the desktop
clipboard. The cases exercise JustBetweenUs and PlayTest configuration. See the
[shared sample guidance](../../../../PlayTestSupport/README.md) for picker
examples, the Windows/macOS runners and the additional application suites.

To repeat the preview pixel checks, run from the sibling `CodeBrix.Platform`
repository after building these tests (requires X11, xdotool, ImageMagick's
`import`, and Pillow). The checkers default to the Platform's own demo suite, so
name this one explicitly:

```sh
python3 build/test-scripts/playtest-preview-orientation.py \
  --test-output ../CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/bin/Release/net10.0 \
  --test-name JustBetweenUs.PlayTests \
  --artifacts /tmp/playtest-preview-orientation
```

The Windows equivalent, `build/test-scripts/playtest-preview-windows.ps1`, likewise
needs `-TestName JustBetweenUs.PlayTests`.
