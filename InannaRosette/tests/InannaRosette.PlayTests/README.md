# InannaRosette.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `InannaRosette/`:

```bash
dotnet test --project tests/InannaRosette.PlayTests/InannaRosette.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/InannaRosette.PlayTests/InannaRosette.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/InannaRosette.PlayTests/InannaRosette.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: command gating, Draw and the deck stack, Auto-lay, Shuffle, Clear with its confirmation, the interpretation panel and its subtitle, the "reading out of date" badge, saving and reopening a reading, a malformed reading, creating the PDF report and cancelling it, dragging a card onto a petal and off the rosette, double-click reversal, the lore panel, the hover return badge, the blessing when the card of Holy Inanna is drawn, an interpreter failure, the app's own dark palette under either OS theme, and portrait layout.

The real page and view model use an injected IDeckFactory that deals a seeded deck, so draws and orientations repeat; a test can choose the seed that puts the Goddess's own card on top. The real interpreter sits behind a fixture that can be switched to fail. The real report builder writes a real PDF. Saved and opened readings are written at test time with the reading library's own serializer. The "Report saved" dialog is always closed with Done: its Open button hands the file to a real PDF viewer.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
