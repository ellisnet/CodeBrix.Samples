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

To watch the application on Linux or macOS:

```sh
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_SLOWMO=200 dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

In PowerShell:

```powershell
$env:CODEBRIX_PLAYTEST_HEADED = "1"
$env:CODEBRIX_PLAYTEST_SLOWMO = "200"
dotnet test --project CodeBrixPlatform/tests/JustBetweenUs.PlayTests/JustBetweenUs.PlayTests.csproj -c Release
```

The headed window is a live, view-only canvas. Resize or move it normally;
physical keyboard/mouse input does not operate the app. The tests operate the
actual controls. Unset `CODEBRIX_PLAYTEST_HEADED` (or use `0`) for headless runs.

Set `CODEBRIX_PLAYTEST_ORIENTATION=portrait` to run the same suite in a separate
1080×1920 virtual screen; otherwise it runs at 1920×1080. `SLOWMO` and `ORIENTATION`
are interpreted by this sample fixture. `HEADED` is interpreted by PlayTest itself.
For a Wayland-only preview set `SDL_VIDEODRIVER=wayland`; for an X11 preview use
`SDL_VIDEODRIVER=x11`. SDL3 otherwise chooses an available display backend.

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
python3 build/pack-playtest-preview.py --version 1.0.272.1-playtest.2
```

Then pass `-p:PlayTestPackageVersion=1.0.272.1-playtest.2` to `dotnet test` or update
the project default. Use `-p:PlayTestPackageFeed=/path/to/feed` for another checkout
layout. Copy all three produced packages (core, base Skia runtime, and PlayTest)
to the other machine's feed. The new head requires the matching core's friend
assembly declarations. The regular Platform release build also includes PlayTest
in its normal package set.

The prototype does not automate native OS dialogs, GPU-only controls or multiple
application windows. Clipboard tests are deliberately isolated from the desktop
clipboard. Windows, macOS and visible Wayland execution need validation on
their respective desktops; cross-platform package assets alone do not prove it.

Validated on Linux x64 with local package set `1.0.271.1-playtest.4`:

- All 37 tests pass headlessly at 1920×1080 and 1080×1920.
- All 37 tests also pass with the live X11 preview at 1920×1080 and 500ms
  action pauses (about two minutes for the full suite).
- The portrait run also passes with `DISPLAY` and `WAYLAND_DISPLAY` unset.
- Headed startup passes with SDL's dummy display driver, including launching and
  shutting down the separate preview process; no desktop window is opened by
  that check. Resizing behavior and physical-input isolation still require
  interactive confirmation on the desktop.
- The repository's dependency gate accepts the produced core package with zero
  errors and zero warnings.

The current `1.0.272.1-playtest.1` package replaces the preview title's em dash
with an ASCII hyphen to avoid the missing-glyph box observed on the X11 desktop.
