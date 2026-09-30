# GitHubIssueFinder.PlayTests

9 serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `GitHubIssueFinder/`:

```bash
dotnet test --project tests/GitHubIssueFinder.PlayTests/GitHubIssueFinder.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/GitHubIssueFinder.PlayTests/GitHubIssueFinder.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/GitHubIssueFinder.PlayTests/GitHubIssueFinder.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Owner/assignee entry, include-closed checkbox, grouped results, empty/error results, retry, Enter/Escape shortcuts, scheme selection, and portrait layout.

The real page and view model use an injected IGitHubIssueSearchService with fixed issue data. No GitHub requests or tokens are required. Settings use a unique test directory.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for local packages, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
