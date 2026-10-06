# PolyHavenBrowser.ViewerOnly.PlayTests

Serialized xUnit v3 UI tests run the shared application XAML and real view models through CodeBrix.Platform.PlayTest. Assertions use SilverAssertions and PlayTest's retrying `Expect(...)`. Nullable and implicit usings are disabled explicitly in the project.

From `PolyHavenBrowser_viewer_only/`:

```bash
dotnet test --project tests/PolyHavenBrowser.ViewerOnly.PlayTests/PolyHavenBrowser.ViewerOnly.PlayTests.csproj -c Release
```

Visible landscape preview (the default orientation, with 250 ms between actions):

```bash
CODEBRIX_PLAYTEST_HEADED=1 \
  dotnet test --project tests/PolyHavenBrowser.ViewerOnly.PlayTests/PolyHavenBrowser.ViewerOnly.PlayTests.csproj -c Release
```

Visible portrait/dark preview:

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait \
CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/PolyHavenBrowser.ViewerOnly.PlayTests/PolyHavenBrowser.ViewerOnly.PlayTests.csproj -c Release
```

Set `CODEBRIX_PLAYTEST_SLOWMO` to override the delay, including `0` for full speed. Headless runs default to zero.

Coverage: The texture sample at startup, switching to the HDRI and model samples (status line, selected button, the rendering-engine dropdown shown or hidden), the busy state while a download is held (progress bar, disabled buttons and dropdown), a failed download and its retry, revisiting a sample from the cache without network, switching to another rendering engine (the current sample re-displayed from the cache), the "not available on this platform" alert and the dropdown snapping back, drag-to-rotate and wheel zoom on the canvas, the real CPU-rendered HDRI panorama changing as it is dragged, a page reset cancelling an in-flight download and disposing its view model safely, the canvas re-rendering after an orientation change, the accent highlight on the selected sample under either OS theme, and portrait layout.

The real API client and sample-asset service run over an injected HttpMessageHandler that returns synthetic asset info, file lists, a checker PNG, a flat Radiance HDR panorama and a small glTF model generated at test time; it can fail downloads or hold them until a test releases them. It never forwards requests to the Internet. Each page gets its own empty cache folder under the test output. The PlayTest head has no GPU surface, so the rendering-engine selector is replaced by a CPU stand-in on which OpenGL and Vulkan work and Metal does not; its engines record the model, camera and frame size they are given. The real OpenGL, Vulkan and Metal renderers are covered by `tests/libs/PolyHavenBrowser.Rendering.Tests`, not here.

Screenshots go under the test output's `TestResults/PlayTest/`; fixture data goes under `TestResults/PlayTestData/`. Failed locator actions include a screenshot and UI-tree description. Data and screenshots remain for inspection.

See [shared setup and API examples](../../../PlayTestSupport/README.md) for running options, project-level orientation/theme defaults, per-test overrides, and scripted file/folder pickers.
