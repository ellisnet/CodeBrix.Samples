# KenneyAssetBrowser.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `KenneyAssetBrowser/`:

```bash
dotnet test --project tests/KenneyAssetBrowser.PlayTests/KenneyAssetBrowser.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/KenneyAssetBrowser.PlayTests/KenneyAssetBrowser.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/KenneyAssetBrowser.PlayTests/KenneyAssetBrowser.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Folder selection/cancel, actual ZIP catalog loading, category/search filters, thumbnails, 2D image zoom/fit/back, text viewing, and portrait layout.

The fixture creates a small ZIP containing two generated PNGs and text documents. The real asset reader and 2D painter are used. No downloaded Kenney bundles or user settings are required. GPU 3D preview and audio playback are not covered.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
