# GameEngineMusicDemo.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML, the real view model and the running game engine through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `GameEngineMusicDemo/`:

```bash
dotnet test --project tests/GameEngineMusicDemo.PlayTests/GameEngineMusicDemo.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/GameEngineMusicDemo.PlayTests/GameEngineMusicDemo.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/GameEngineMusicDemo.PlayTests/GameEngineMusicDemo.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: engine start-up and the generated asset set, the grids, markers and tempo change each MIDI file reports, every transport button, stop, immediate and bar-quantized crossfades, cancelling a queued transition, the global pause keeping a queued transition, the volume-bus sliders, the stem sliders and lead fades, the stems export's fades by name, the MIDI harmony layer and tempo slider, jump points, stinger, dialogue and held ducks, the playlist, the readout the engine draws on its canvas, and both orientations.

The engine, the audio device and the music manager are process-wide, and the page starts the demo once, from the canvas's first layout. The fixture therefore launches the application once, keeps the page it navigated to for the whole run, and resets the music system, the mixer, the layers and the sliders on that live page before each test instead of building a new page. The music plays through the real audio output, so a run is audible. Fades, bar waits and ducks run on the engine's real-time clock; tests wait on what the music system reports, never on fixed delays. The opt-in maintainer walkthrough is switched off before launch.

The generated music goes under the test output's `GeneratedMusic/`; screenshots go under `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults and per-test overrides.
