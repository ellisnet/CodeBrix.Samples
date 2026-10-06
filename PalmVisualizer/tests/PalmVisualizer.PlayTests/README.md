# PalmVisualizer.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PalmVisualizer/`:

```bash
dotnet test --project tests/PalmVisualizer.PlayTests/PalmVisualizer.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/PalmVisualizer.PlayTests/PalmVisualizer.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/PalmVisualizer.PlayTests/PalmVisualizer.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Startup camera discovery with auto-selection of the first camera, no cameras, discovery failure and a camera that cannot start, the Visualize button waiting for the first frame, Camera and Visualize Mode switching with the controls hiding and showing and the camera picker disabled, the tracker started and stopped with the mode, one visualizer session created at the game canvas's first layout and then paused and resumed, frames reaching the tracker only in Visualize Mode, open palms mirrored into the session with closed palms ignored and the status line following the open-palm count, tracking results ignored in Camera Mode, switching cameras, a late-finishing camera switch not overwriting the newer camera's status, the mirrored preview's pixels, theme background pixels, portrait layout, and rotation keeping Visualize Mode and its session.

The application is built with `App(Action<IServiceCollection>)`, which registers fakes for the three collaborators before the launch page exists: a capture service with scripted cameras, start failures, held starts and frames pushed by the test; a tracker that records submitted frames and raises scripted palms; and a visualizer-session factory whose sessions record their lifecycle and palms. No camera is enumerated or opened, no hand model is loaded and the game engine is never started. The real tracker's inference on a photograph is covered by the Vision library's own tests, not here.

Screenshots go under the test output's `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description. Screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
