# RedisSetupTool.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

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

Coverage: Dashboard cards and all seven navigation destinations; container search/filter, detail tabs (overview, logs and tail choice, streamed stats, diagnostics, advisor), lifecycle actions, kill signal, declined removal and copying the id; console tabs (open, missing shell, close, exit and reopen, typed input reaching the exec session); instance cards (endpoints, revealing and copying a password, Copy all, Verify, filters, confirmed destroy); the create form (name validation, progress, failure, cancel); image pull/tag/remove and the Trivy, Dive and Hadolint reports; network and volume create/remove; System prunes, the event stream with its filter, pause and clear, and the empty sweep; the app's own dark palette under either OS theme; unreachable-daemon recovery; and portrait layouts.

Strict in-memory fakes replace IDockerManager, IRedisTopologyService and IRedisProbe at the application's DI boundary; unsupported calls throw, and resolving the concrete DockerManager is forbidden in the fixture. Replacing only IDockerManager is insufficient because the production topology service takes the concrete DockerManager. The Docker fake keeps containers, images, networks and volumes in memory and records every change it is asked for. Daemon events, container stats and console output are channels the tests write to; console sessions are scripted exec sessions that stay open until a test ends them or the fixture resets. The topology fake uses the real catalog, keeps discovered instances in a list and holds a create request behind a gate the test releases. Verify gets a scripted probe result. Docker Desktop need not be running, and no container, image or Redis server is ever touched. Startup automation is temporarily disabled and restored on disposal. Real topology validation and port allocation, live terminal output and real Redis operations are not covered.

GetByText and GetByTestId also match elements that are not visible, so a few assertions read bound view-model state or pick the System section's copy of an event row; each place says why.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
