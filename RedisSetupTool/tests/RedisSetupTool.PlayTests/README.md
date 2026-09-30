# RedisSetupTool.PlayTests

12 serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `RedisSetupTool/`:

```bash
dotnet test --project tests/RedisSetupTool.PlayTests/RedisSetupTool.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/RedisSetupTool.PlayTests/RedisSetupTool.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/RedisSetupTool.PlayTests/RedisSetupTool.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Dashboard, all seven navigation destinations, container search/filter, console picker/cancel, unreachable-daemon recovery, image search, and portrait layout.

A strict in-memory IDockerManager replaces the daemon. Unsupported calls throw; no Docker process or endpoint is used. The real topology catalog and validation remain available, with a fixed port-preview fixture. Startup automation is temporarily disabled and restored on disposal. Creating/destroying instances, live terminal sessions and Redis operations are not covered.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for local packages, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
