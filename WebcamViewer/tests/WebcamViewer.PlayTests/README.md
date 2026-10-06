# WebcamViewer.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `WebcamViewer/`:

```bash
dotnet test --project tests/WebcamViewer.PlayTests/WebcamViewer.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/WebcamViewer.PlayTests/WebcamViewer.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/WebcamViewer.PlayTests/WebcamViewer.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Camera discovery (pending, none found, failure), auto-start of the first camera, a camera that will not start, switching cameras and the old session's disposal, the Monitor audio checkbox following the microphone and reaching each session, Photo enablement from frame, typed folder and missing folder, the folder picker and its cancel, the written PNG's size and pixels, a failed photo, live frames on the video canvas including aspect-fit letterboxing, session disposal on a page reset, theme background pixels, and portrait layout.

The real page and view model use an injected ICameraService, registered through `App(Action<IServiceCollection>)` before the launch page is built, so no page ever enumerates or opens a real camera and no native media runtime is needed. Discovery holds until the test releases it with scripted cameras or an error; each session records start, disposal and audio monitoring, and delivers solid-colour frames from a worker thread as a capture thread would. Photos are written to a unique test directory.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
