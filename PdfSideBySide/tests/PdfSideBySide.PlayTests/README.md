# PdfSideBySide.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PdfSideBySide/`:

```bash
dotnet test --project tests/PdfSideBySide.PlayTests/PdfSideBySide.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/PdfSideBySide.PlayTests/PdfSideBySide.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/PdfSideBySide.PlayTests/PdfSideBySide.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Empty panes and their placeholders, browsing through the scripted file picker (including cancel), file paths, page labels and the comparing status line, the rendered page on screen, both-documents and adjust-right navigation up to the longer document's end, zoom in/out/reset across the whole zoom ladder with the image size applied to both panes, pan buttons that enable only when zoomed and scroll only their own pane, a page change or a replaced document returning to fit-the-page, the duplicate-file and unreadable-PDF error dialogs, documents pre-loaded from the startup command line and a missing one reported, and portrait layout.

The real page, view models and PDFium renderer open small synthetic PDFs the fixture writes at run time. The startup command line comes from an injected IStartupArguments, so the test host's own arguments are never taken for documents to pre-load.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
