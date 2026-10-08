# Pinta.Brix.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML, the real engine, tools, effects and file formats through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `Pinta.Brix/`:

```bash
dotnet test --project tests/Pinta.Brix.PlayTests/Pinta.Brix.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/Pinta.Brix.PlayTests/Pinta.Brix.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/Pinta.Brix.PlayTests/Pinta.Brix.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: the launch document, pencil strokes on the canvas with pixel checks, undo and redo from the menu, the toolbar and Ctrl+Z / Ctrl+Y, travelling through the history pad, adding, duplicating, deleting, hiding and renaming layers, importing a layer from a file, opening and saving PNG files through the pickers, a cancelled open, the flatten prompt, the unsaved-changes prompt and closing a tab that is not the active one, the Invert Colors adjustment, the Gaussian Blur, Posterize, Levels and Curves dialogs, copying and pasting into a new image and a new layer on the isolated clipboard, rectangle selections with erase, fill, invert and crop, the resize image and resize canvas dialogs, flips and rotations, the zoom buttons and preset list, the palette's swap key and arrow, the colour dialog, palette size and palette save/open, the paint bucket, the toolbox shortcut keys (a letter selects its tool, the same letter again moves to the next tool sharing it, never with Ctrl held, never while typing into a text box or the text tool), the Window menu and its Alt+number shortcuts, the cursor readout, the New Screenshot message, the Keyboard Shortcuts and About dialogs, the pad splitter and its saved width, both orientations and the simulated OS theme.

The engine keeps process-wide state in `PintaCore`, and the page subscribes to it once, from its first `Loaded`. The fixture therefore launches the application once, keeps the page it navigated to for the whole run, and resets that live page before each test: it answers any dialog a failed test left open, closes every document without prompting, restores the launch tool, colours and palette, clears the isolated clipboard and the picker queues, puts the pad splitters back and opens a fresh blank document through File > New. Images for the open and import tests are written at test time, so no licensed asset is needed. Settings use a unique test directory through the `App(string settingsDirectory)` constructor, never the user's own store. The Help menu's website entries start a real browser and are never clicked.

Screenshots and fixture data go under the test output's `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
