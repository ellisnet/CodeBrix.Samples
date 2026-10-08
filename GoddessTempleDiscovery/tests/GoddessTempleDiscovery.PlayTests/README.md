# GoddessTempleDiscovery.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML, the real view model and the running cards-and-dice table through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `GoddessTempleDiscovery/`:

```bash
dotnet test --project tests/GoddessTempleDiscovery.PlayTests/GoddessTempleDiscovery.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/GoddessTempleDiscovery.PlayTests/GoddessTempleDiscovery.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero. Set `GODDESSTEMPLE_PLAYTEST_SKIP_AUTOPLAY=1` to leave out the autoplay smoke test.

Coverage: the engine starting from the canvas's first layout, the title's wordmark art and Julius Jordan's epigraph, the window title, the preferences and last setup read from a throwaway settings store; the setup pane (two to four seats, human or computer, team cards and typed names, temperaments, turns a season, difficulty, seed validation, the last setup stored and offered again); the prologue and the first season's front page; a seeded game against three computer teams (the five trenches of the Site Row and their labels, a roll on the table and in the engine, a dig that raises `SiteExcavated` and opens the newspaper with its masthead, headline, columns and sidebars, Close and Escape, Study, Survey, End Turn and the computers playing on to the human's next turn, MENU and CONTINUE, the HUD's JOURNAL and SETTINGS buttons); a report published from three finds and the score ribbon counting it; the Field Journal's entries and filter, its PDF export through a file bridge and its cancel, OPEN FOLDER through the platform launcher; the settings switches and speed, stored and applied to the table; How to Play, the History's two tabs and the Credits with the heritage note verbatim; the Gallery of the game's art from the title and from the table, its category filter and a picture shown large; the letterboxed table in portrait and the widened table in landscape, the panes on screen in both orientations, the night palette under either OS theme; and an autoplay game played to its end.

The engine is process-wide, and the page starts the game host once, from the canvas's first layout. The fixture therefore launches the application once, keeps the page it navigated to for the whole run, and brings it back to its title before each test instead of building a new page; each game test starts a new seeded game through the setup pane. Before launch it opens the settings store in a throwaway folder under the test output, seeded with a returning player's choices (sound off, reduced motion on, the computers' finds not shown, double speed, Easy, three stored seats), so the player's own store is never touched and the computers play quickly. The application's `App(Action<IServiceCollection>)` overload registers a recording file bridge, which the view model prefers to the page's save picker, so the journal export writes into `TestResults/PlayTest/journals/` and its cancel is scripted. The game is set to the CPU render tier.

The game reads the mouse and the keys by sampling their state once per engine step, and the table is drawn by the engine rather than by XAML. The fixture therefore maps table coordinates onto the canvas (the table is fitted and centred), holds the mouse button down and up for whole engine steps (the host counts its steps), holds keys down until the game has seen them, and reads the game on the engine thread. A click on the game canvas leaves it without keyboard focus; the tests give the canvas focus back before a key, as activating the window does.

HUD buttons act on the release, as controls do: the press arms the button, a release on the same button fires it, a release elsewhere cancels it. The release therefore reaches the canvas before the pane the button opens covers it, so the next click on the table counts; `After_a_hud_button_opens_a_pane_the_next_click_on_the_table_still_counts` keeps it so. While any pane covers the table the game takes no clicks on the table or the HUD. The journal tests open the journal with its J key.

PlayTest runs one application per process, and the autoplay switch is read when the game host is created. The autoplay smoke test therefore runs itself once more in a child process with `GODDESSTEMPLE_AUTOPLAY=1` and a seed: the child plays the four computer teams to the end, checks the game is over, the epilogue and the Final Edition's four rows, and the Field Journal written to the temp folder; the parent checks the child passed and that its log holds the seeded game's four final scores and `GODDESSTEMPLE AUTOPLAY PASS`. It takes about a minute.

Screenshots go under `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults and per-test overrides.
