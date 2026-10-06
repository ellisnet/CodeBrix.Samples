# CodeBrixVideoTool.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `CodeBrixVideoTool/`:

```bash
dotnet test --project tests/CodeBrixVideoTool.PlayTests/CodeBrixVideoTool.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/CodeBrixVideoTool.PlayTests/CodeBrixVideoTool.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/CodeBrixVideoTool.PlayTests/CodeBrixVideoTool.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: the empty library, the missing-FFmpeg warning on the first Open, a cancelled open picker, opening a `.cbv` into the list and the real player (frames presented, transport, timecodes), play, pause, resume and stop, the chapter and caption drop-downs and the seeks and track changes they make, a dimmed MP4 row with the unplayable notice, Remove, the destination, resolution and quality choices with the action label, codecs and route they produce, a conversion with the suggested save name and its output joining the list, a cancelled save picker, progress (determinate and indeterminate), Cancel, the last-run notes panel, a failed conversion, scrubber drags, mute and volume, busy-state command disabling, portrait layout, and the forced dark palette under either system theme.

The real page, view models and VideoPlayer element use an injected IMediaProbe, IConversionRunner and IExternalToolCheck. `.cbv` files go to the application's own managed probe; other files get a fixed ffprobe-style answer. The runner records the plan, reports scripted progress, holds until released or cancelled, and writes a marker file instead of encoding. No FFmpeg, ffprobe or SVT-AV1 is needed. The clips are written by the fixture at start-up with the playback core's managed CBVF muxer: uncompressed video, which the core decodes itself, with no soundtrack, so no codec package, audio device or committed media file is involved. The fixture clears `CODEBRIXVIDEOTOOL_SMOKE`, because a scripted smoke run ends the process.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
