# PolyHavenBrowser.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PolyHavenBrowser/`:

```bash
dotnet test --project tests/PolyHavenBrowser.PlayTests/PolyHavenBrowser.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/PolyHavenBrowser.PlayTests/PolyHavenBrowser.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/PolyHavenBrowser.PlayTests/PolyHavenBrowser.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Catalog metadata and thumbnails, metadata search, sorting, folder selection/cancel, missing-folder dialog, and portrait layout.

The real API client and catalog service run over an injected HttpMessageHandler that returns synthetic catalog JSON and PNGs. It never forwards requests to the Internet. Model downloads, GPU rendering and document generation are not covered.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
