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

A strict in-memory IDockerManager and IRedisTopologyService replace daemon operations. The real topology catalog remains available; validation returns a controlled empty result and port previews use a fixed plan. Unsupported calls throw, and resolving the concrete DockerManager is forbidden in the fixture. Replacing only IDockerManager is insufficient because the production topology service takes the concrete DockerManager. Docker Desktop need not be running. Startup automation is temporarily disabled and restored on disposal. Topology validation, creating/destroying instances, live terminal sessions and Redis operations are not covered.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for local packages, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
