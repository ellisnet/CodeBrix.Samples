# WikipediaPublisher.PlayTests

10 serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Two additional cases exercise the local HTTP fixture, for 12 cases total. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `WikipediaPublisher/`:

```bash
dotnet test --project tests/WikipediaPublisher.PlayTests/WikipediaPublisher.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/WikipediaPublisher.PlayTests/WikipediaPublisher.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/WikipediaPublisher.PlayTests/WikipediaPublisher.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Real embedded-browser navigation and redirects, actual pointer clicks on HTML links in forced landscape/portrait rows, search button/Enter, publish enablement, save selection/cancel, page size, overwrite cancellation, rendering errors, and portrait layout.

A loopback HTTP server supplies local HTML to the real WPE WebKit browser. IArticleRenderService is replaced with a fixture that records the request and writes a marker file; these UI tests do not validate PDF rendering. Files are written only below the test data directory. The normal application still opens Wikipedia; only the test host supplies WikiNavigationOptions.

The server handles connections independently so an idle connection or incomplete request cannot block another navigation. Browser disconnects affect only their own connection. Shutdown cancels pending reads and waits for the handlers to finish. `WikiServerTests` checks navigation with an idle or partial request outstanding and verifies that shutdown closes those connections.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for local packages, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.

The current PlayTest embedded-browser suite requires Linux with WPE WebKit (`libwpewebkit-2.0-1`, `libwpebackend-fdo-1.0-1`, `libwpe-1.0-1`). The application already references the WebView add-in; no browser dependency was added to the PlayTest head. PlayTest currently has no native WebView provider on Windows/macOS, so this suite is not supported there yet. This limitation is specific to the PlayTest head; the normal desktop heads retain their existing browser support.

On 2026-09-29, all 12 cases passed in an LMDE 7 Wayland session in headless/light landscape and portrait, and headed/dark landscape and portrait. Headed runs forced `SDL_VIDEODRIVER=wayland` and left `CODEBRIX_PLAYTEST_SLOWMO` unset, using the normal 250 ms delay. The initial headed/landscape run had five cascading navigation failures; a focused probe reproduced blocking behind an idle connection in the old HTTP fixture. All four configurations passed after the fixture fix and regression cases were added.
