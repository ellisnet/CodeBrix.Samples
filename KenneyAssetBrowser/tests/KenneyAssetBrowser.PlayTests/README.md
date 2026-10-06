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

Coverage: Folder selection/cancel, actual ZIP catalog loading, the corrupt-zip warning and the empty-folder caption, bundle switching and the category reset, the remembered folder and bundle on a new page, category/search filters, thumbnails, lazy loading of further cells on scroll, the bundle License dialog, 2D image zoom/fit/wheel/clamping/back, spritesheet region spotlighting, SVG rasterizing, font specimens, Tiled maps, the no-preview caption, the "Could not open" dialog, 3D models (the "3D Preview Unavailable" dialog, model facts, animation selection and play/pause), the audio transport and its replay rule, text viewing, and the viewer layout in both orientations.

The fixture creates small ZIPs in code: two generated PNGs and text documents; an edge-case bundle with an undecodable image, a generated Tiled map and tileset, and the Merriweather font the application already ships; and a file that is not a zip. It copies the CC0 Kenney bundles from the sample's `sample_asset_bundles/` folder (Puzzle Pack, Sci-Fi Sounds, Blocky Characters) into its data folder. The real asset reader, 2D painter and model loader are used. No downloads or user settings are required. The PlayTest head has no OpenGL, so 3D tests check the unavailable-preview dialog and model facts rather than rendered pixels. Audio tests replace the view model's audio bridge with a recording fake, so no audio device is needed and no sound plays.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
