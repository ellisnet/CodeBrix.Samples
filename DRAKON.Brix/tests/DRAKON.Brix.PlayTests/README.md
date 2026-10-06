# DRAKON.Brix.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and the real DRAKON Editor, booted hosted inside the page's `TkHostView`, through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `DRAKON.Brix/`:

```bash
dotnet test --project tests/DRAKON.Brix.PlayTests/DRAKON.Brix.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/DRAKON.Brix.PlayTests/DRAKON.Brix.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/DRAKON.Brix.PlayTests/DRAKON.Brix.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: the hosted boot and the intro window, a document preloaded through `DRAKONBRIX_OPEN`, the hidden input element holding keyboard focus, opening from the intro (button and Enter) and from File > Open through the scripted open picker, cancelling those pickers, Save as through the scripted save picker, the diagram list, double-clicking an icon, editing its text and undoing the edit from the Edit menu, copying an icon to the isolated clipboard, wheel scrolling of the diagram canvas, quitting from the intro (Escape, cancelled open) and from File > Quit, recent files kept across a new page, the Tk root following both orientations and a switch to portrait, and the Tk surface keeping its classic colours under either simulated theme.

Every test page boots a fresh interpreter, and the fixture waits for `RuntimeHost.IsReady` before a test starts and before the next page boots. Tk widgets are not in the XAML tree, so the tests click them at the geometry Tk reports (`winfo`, the menu widgets' entry rectangles, the treeview's row hit test, DRAKON's canvas primitives) and read Tcl state through an internal `RuntimeHost` accessor on the interpreter's own thread. The icon tests run in landscape, where the example diagrams are on screen. DRAKON's quit is replaced by a recording action that stops the script, so no test reaches `Environment.Exit`. `HOME` points at a unique test directory, so `drakon_editor.settings` never touches the real one; diagram files are private copies of the examples shipped in the application's `Assets/drakon/examples`.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
