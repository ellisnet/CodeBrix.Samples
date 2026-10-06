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

Coverage: Catalog metadata and thumbnails, failed thumbnails, catalog-load failure, metadata search, every sort order, lazy batches on scroll and the jump back to the top on a new search, folder selection/cancel, missing-folder dialog, model downloads (progress bar, disabled Download buttons, already-downloaded models, missing glTF offers, HTTP failures and retry), the Model View (title, author, description, tags, details rows, Back, the once-per-page "3D Preview Unavailable" dialog, landscape and portrait layouts), the marketing one-sheet PDF (save, cancel, suggested file name, busy state), the app's fixed dark palette under either OS theme, and portrait layout.

The real API client, catalog and download services run over an injected HttpMessageHandler that returns synthetic catalog JSON, PNGs, file lists and a small glTF model generated at test time; it can fail requests or hold downloads until a test releases them. It never forwards requests to the Internet. Each page gets its own catalog service, so a test can reload a larger or failing catalog. The one-sheet PDF is really generated, on the CPU with plain backdrop floors and no product shots. The OpenGL 3D preview (rotate and zoom) is not covered: the PlayTest head has no GPU surface.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
