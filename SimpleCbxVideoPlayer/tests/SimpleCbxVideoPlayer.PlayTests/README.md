# SimpleCbxVideoPlayer.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view model through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `SimpleCbxVideoPlayer/`:

```bash
dotnet test --project tests/SimpleCbxVideoPlayer.PlayTests/SimpleCbxVideoPlayer.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/SimpleCbxVideoPlayer.PlayTests/SimpleCbxVideoPlayer.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/SimpleCbxVideoPlayer.PlayTests/SimpleCbxVideoPlayer.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: the corpus line and clip list against a scan of the bundled corpus, the render path settling on the CPU canvas, the GPU-only choice reporting its failure in the message line, the CPU choice, the lookup-table panel and Bake disabled on the processor path, Play with the clip's duration, Pause and resume, Stop, choosing another clip, the end of a clip, every container's 720p clip, keyboard seeks on the scrub bar, the paused frame drawn into the video area, the same frame decoded from MKV, WebM and the bespoke container, a portrait clip pillarboxed in landscape, portrait layout and the simulated OS theme.

The tests play the small 720p clips the application carries, with the real decoders and the real soundtrack, so the speakers play while the suite runs. Waits follow the player itself, read through the view model's internal `Controller`: its position, transport state and the timestamp of the frame on screen, and the frame the CPU canvas last painted. Timing assertions are made only after Pause, Stop or the end of a clip, and frame comparisons only after an exact seek while paused. The suite references CodeBrix.Platform.PlayTest.OpenGL but launches with `PlayTestOptions.OpenGL = Unavailable`, so the GPU canvas cannot start and the page always settles on the CPU canvas; the lookup-table panel and Bake are tested in their disabled state only.

Screenshots go under the test output's `TestResults/PlayTest/`; captured video frames go under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
