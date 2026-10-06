# PainDiagram.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view model through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PainDiagram/`:

```bash
dotnet test --project CodeBrixPlatform/tests/PainDiagram.PlayTests/PainDiagram.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project CodeBrixPlatform/tests/PainDiagram.PlayTests/PainDiagram.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project CodeBrixPlatform/tests/PainDiagram.PlayTests/PainDiagram.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Layer selection and its check-mark captions, freehand strokes drawn on the canvas with real pointer drags, clicks and mouse polylines, presses outside the body map, Clear with and without its confirmation, Save through the scripted save picker (cancel, replace yes/no, clear after saving, write failure), stroke pixels on screen and in the saved PNG, portrait layout and the simulated OS theme.

Strokes are drawn with PlayTest pointer input at positions computed from the drawing session's aspect-fit rectangle; assertions read stroke and layer counts from the real drawing session, never exact stroke geometry. Saved images go to a unique test directory.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
