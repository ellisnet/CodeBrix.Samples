# WikipediaPublisher.PlayTests

On Intel macOS 15.8, 2026-09-30, all 16 cases passed with local preview `.16` in
each of headless/light landscape, headless/light portrait, headed/dark landscape
and headed/dark portrait: 64 executions, zero failures or skips. This uses the
real system WKWebView, including native pointer/keyboard navigation and browser
pixels composited into PlayTest screenshots through repeated orientation changes.

14 serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Two additional cases exercise the local HTTP fixture, for 16 cases total. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

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

Coverage: Browser pixels in actual Skia screenshots across repeated orientation changes, real embedded-browser navigation and redirects, actual pointer clicks and Tab/Enter activation of HTML links in forced landscape/portrait rows, search button/Enter, publish enablement, save selection/cancel, page size, overwrite cancellation, rendering errors, and portrait layout.

A loopback HTTP server supplies local HTML to the real WPE WebKit browser on Linux, Edge WebView2 on Windows, or WKWebView on macOS. IArticleRenderService is replaced with a fixture that records the request and writes a marker file; these UI tests do not validate PDF rendering. Fixture files stay below the test data directory; Windows browser profiles stay below the test output's `TestResults/PlayTest/WebView2`. The normal application still opens Wikipedia; only the test host supplies WikiNavigationOptions.

**Why the simulated page switches between white and blue:** The local HTML fixture
normally has a white background. The two orientation rows of
`Embedded_browser_pixels_follow_orientation_changes` in [ApplicationTests.cs](ApplicationTests.cs)
temporarily set the page's background to blue (`rgb(37,149,211)`, or `#2595D3`).
The test looks for that exact color in PlayTest screenshots while switching
between landscape and portrait, verifying that the browser's pixels are actually
rendered into the application's image.

This color change is performed entirely by the PlayTests through JavaScript in
the local fixture page; it required no changes to WikipediaPublisher's application
code, XAML or assets. Each test starts with a fresh page, restoring the fixture's
white background. The change occurs in both headless screenshots and visible
previews, independently of the light/dark application theme. Switching between
white and blue during the suite is expected test behavior.

The server handles connections independently so an idle connection or incomplete request cannot block another navigation. Browser disconnects affect only their own connection. Shutdown cancels pending reads and waits for the handlers to finish. `WikiServerTests` checks navigation with an idle or partial request outstanding and verifies that shutdown closes those connections.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for local packages, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.

The embedded-browser suite supports macOS, or requires Linux with WPE WebKit (`libwpewebkit-2.0-1`, `libwpebackend-fdo-1.0-1`, `libwpe-1.0-1`) or Windows with the Edge WebView2 runtime. The test project selects the matching local WebView preview alongside the PlayTest package. No browser dependency was added to the PlayTest head. On macOS 12 and later, the matching add-in supplies an offscreen WKWebView helper with a nonpersistent data store. It renders into the same Skia scene used by screenshots and the SDL preview; native pointer and keyboard events operate the browser. The normal desktop heads retain their existing browser support.

On 2026-09-29, all 12 cases passed in an LMDE 7 Wayland session in headless/light landscape and portrait, and headed/dark landscape and portrait. Headed runs forced `SDL_VIDEODRIVER=wayland` and left `CODEBRIX_PLAYTEST_SLOWMO` unset, using the normal 250 ms delay. The initial headed/landscape run had five cascading navigation failures; a focused probe reproduced blocking behind an idle connection in the old HTTP fixture. All four configurations passed after the fixture fix and regression cases were added.

On 2026-09-30, all 14 cases passed on Windows x64 with local preview `.12` in
headless/light landscape and portrait, and headed/dark landscape and portrait.
That includes the two new browser Tab/Enter regression rows, which caught the
Windows input bridge's missing DOM key identity. There were no failures or skips;
headed runs used the Windows SDL driver and the default 250 ms action delay.
