# NotionDocumentCreator.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `NotionDocumentCreator/`:

```bash
dotnet test --project tests/NotionDocumentCreator.PlayTests/NotionDocumentCreator.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/NotionDocumentCreator.PlayTests/NotionDocumentCreator.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/NotionDocumentCreator.PlayTests/NotionDocumentCreator.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Connect enablement, success and failure, busy-state command disabling, the page tree with lazy child loading and its failure, page checkboxes and the selected-count text, preview including a newer selection superseding a slower one, Load whole tree, save selection with the suggested name, placeholder removal and cancel, a typed output path, page size, the request sent for Create! in tree order, progress, the result summary with its warnings, overwrite cancellation, creation errors, and portrait layout.

The real page and view model use an injected INotionDocumentService with a fixed page tree. It records the create request, holds calls open on request, and writes a marker file instead of rendering a PDF; these UI tests do not validate PDF rendering. No Notion requests, tokens or image downloads are involved. Fixture files stay below the test data directory.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
