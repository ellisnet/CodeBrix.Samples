# BrixInvaders.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML, the real view model and the running game through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `BrixInvaders/`:

```bash
dotnet test --project tests/BrixInvaders.PlayTests/BrixInvaders.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/BrixInvaders.PlayTests/BrixInvaders.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/BrixInvaders.PlayTests/BrixInvaders.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: the engine starting from the canvas's first layout, the splash handing over to the title, every Kenney asset key resolving from the zips beside the test binary, the CPU render tier, the settings, ship, difficulty, volume and high-score table the game reads from its store, the music director and link opener registered through the application's service overload, key events reaching the focused game canvas, a mouse click counting as input on the title, attract mode starting after the idle time and a click leaving it, the frames the engine draws, and the letterboxed playfield in both orientations.

The engine is process-wide, and the page starts the game once, from the canvas's first layout. The fixture therefore launches the application once, keeps the page it navigated to for the whole run, and brings the game back to its title before each test instead of building a new page. Before launch it opens the settings store in a throwaway folder under the test output, seeded with a returning player's choices and one high score, so the player's own store is never touched. The application's `App(Action<IServiceCollection>)` overload replaces the generated music with a recording director, so no music model is loaded, and the browser launcher with a recording link opener. Sound effects play through the real audio output, so a run is audible. Quit on the title is never chosen: it closes the application.

PlayTest's key press goes down and up within one turn of the UI thread, and the game reads its keys by sampling their state once per engine cycle, so a press almost never reaches the game's menus or its ship. The tests therefore prove that key events reach the canvas the game listens on, and drive the game only with the mouse, whose button the fixture holds down until the game has seen it. A left click on the game canvas also leaves it without keyboard focus; the fixture gives the canvas focus back before each test, as activating the window does.

Screenshots go under `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults and per-test overrides.
