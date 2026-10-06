# MediaPlayerDemo.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `MediaPlayerDemo/`:

```bash
dotnet test --project tests/MediaPlayerDemo.PlayTests/MediaPlayerDemo.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/MediaPlayerDemo.PlayTests/MediaPlayerDemo.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/MediaPlayerDemo.PlayTests/MediaPlayerDemo.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: Startup load of the default address, Load enablement for blank addresses, valid and invalid addresses and their status line, source replacement on the view model and the media element, the address and AutoPlay setting reaching the playback engine, the stretch picker's modes reaching the media element, the media element and transport controls without a video engine, theme background pixels, portrait layout, and rotation preserving page state.

The project references the CodeBrix.Platform.MediaPlayer add-in with `ExcludeAssets="all"`, so the XAML generator emits no libVLC registration into the test assembly. The fixture registers a recording IMediaPlayerExtension instead: no media is streamed or played, the shipped default address is never contacted, and no native media library is needed on any OS. Video frames, sound and playback state are not covered.

Screenshots go under the test output's `TestResults/PlayTest/`. Failed locator actions include a screenshot and UI-tree description. Screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
