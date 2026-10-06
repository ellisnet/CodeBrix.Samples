# PicoScope.Brix.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view model through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PicoScope.Brix/`:

```bash
dotnet test --project tests/PicoScope.Brix.PlayTests/PicoScope.Brix.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/PicoScope.Brix.PlayTests/PicoScope.Brix.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/PicoScope.Brix.PlayTests/PicoScope.Brix.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: startup and the device/capability/status text, command enable/disable rules while streaming and stopped, single capture, restarting the stream, range changes reaching the chart's series titles and voltage axis (streaming and stopped), hiding a channel, the signal generator toggle and its validation and limit errors, Flash LED, the no-scope, open-failure and real-hardware startup paths, wheel zoom and Home reset on the chart, trace pixels following channel visibility, identical pixels for repeated captures, and portrait layout.

The real page and view model find their scope through `ScopeDeviceFinder`, exactly as the heads arrange it, but the fixture registers a noise-free `SimulatedScopeDataDevice` (or a small fixture device built on one) instead of the Ps2000 driver. No instrument, driver or network is required. Each page releases the scope by resetting the finder when it unloads, so the fixture shuts the previous page down before it registers the next scope.

Screenshots go under the test output's `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description. Screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
