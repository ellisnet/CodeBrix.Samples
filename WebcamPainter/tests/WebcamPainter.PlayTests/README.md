# WebcamPainter.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view model through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `WebcamPainter/`:

```bash
dotnet test --project tests/WebcamPainter.PlayTests/WebcamPainter.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/WebcamPainter.PlayTests/WebcamPainter.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/WebcamPainter.PlayTests/WebcamPainter.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Camera discovery, auto-start and status (no cameras, a camera that fails to start, switching cameras), Take Photo waiting for the first frame, the mirrored live preview and self-view pixels, Paint Mode command states, strokes painted by open-palm reports and a closed hand that only moves the crosshair, every highlighter colour and its ink on screen, Save through the scripted save picker (JPEG at the photo's resolution, clear after saving, cancel, declined replace, write failure), Back with and without a painting, a fresh session after Back, Clear with and without its confirmation, portrait layout and the simulated OS theme.

The real page and view model resolve an `ICameraSource` and an `IHandTracker` from the application's services; the fixture registers scripted fakes through the `App(Action<IServiceCollection>)` overload before the launch page exists, so no test ever enumerates or opens a real camera or loads the hand-tracking models. The fake camera serves canned two-colour frames only when a test emits one, and the fake tracker reports palm positions only when a test calls it, so strokes are fully deterministic. Saved images go to a unique test directory.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
