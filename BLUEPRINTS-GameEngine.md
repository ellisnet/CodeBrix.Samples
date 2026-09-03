# CodeBrix.Samples Blueprints: Hosting a game engine

These two recipes cover hosting the CodeBrix.Platform GameEngine loop inside
an ordinary page: handing the view model a game canvas once it has a real,
non-zero layout size, and owning the engine lifecycle in a session class so
the loop can start, pause, resume and stop as the user moves around the UI
without tearing the scene down and rebuilding it. Reach for this file when
an engine-driven surface has to live alongside regular pages and controls.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Hand the view model a game canvas at its first real layout size](#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size)
- [Run and pause a game engine session inside a page](#run-and-pause-a-game-engine-session-inside-a-page)

## Related blueprints

- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - the one-method bridge interface a page implements to hand a canvas to its view model
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - drawing on Skia canvases outside the engine loop, and gating or falling back when a GPU backend is unavailable
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the commands and Dispose path that drive the session class

---

## Hosting a game engine

### Hand the view model a game canvas at its first real layout size

**When you want this.** You want the CodeBrix.Platform GameEngine loop rendering
inside an ordinary page, and the engine can only be started against a surface that
already has a non-zero size - which, for a canvas that starts hidden, is the first
time it is shown.

**The MVVM shape.** The view model declares an interface with one method taking
the canvas. The page forwards the canvas's first-started event to it in a single
line. The view model builds its scene object and starts it; no engine code lives
in the code-behind.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page tell the view model when the visualizer's game canvas has its
/// first real layout size - the engine can only start against a non-zero surface, which
/// happens the first time Visualize Mode is shown.
/// </summary>
public interface IManageGameCanvas
{
    /// <summary>Called once, on the UI thread, at the canvas's FirstStarted event.</summary>
    /// <param name="canvas">The game canvas the visualizer renders into.</param>
    void CanvasFirstStart(GameSurfaceCanvas canvas);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs
//Fires once, at the canvas's first non-zero layout size - i.e. the first time
//  Visualize Mode is shown - which is when the engine can start
VisualizerCanvas.FirstStarted += (_, _) => _gameCanvasManager?.CanvasFirstStart(VisualizerCanvas);
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
public void CanvasFirstStart(GameSurfaceCanvas canvas)
{
    //UI thread, the first time Visualize Mode is shown with a real size: build the
    //  shader scene and start the engine. Later mode switches pause and resume it.
    _visualizerSession = new VisualizerSession(canvas);
    _visualizerSession.Start();
}
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
xmlns:game="clr-namespace:CodeBrix.Platform.GameEngine.Host.Rendering;assembly=CodeBrix.Platform.GameEngine.Host"
...
<game:GameSurfaceCanvas x:Name="VisualizerCanvas"
                        Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}" />
```

**Where to look.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Sharp edges.**
- The first-started event fires once. Starting the engine from the page's loaded
  event, or from the command that switches modes, would run against a zero-sized
  surface.
- The order inside the command matters: making the canvas visible - which is what
  raises the event the first time - comes before resuming the session, and the
  resume is null-safe because on the first pass the session does not exist yet.
- Passing the canvas type through a view-model interface keeps the sample short
  but does put a view type in the view model's signature; a bridge that hands over
  only what the session needs is the cleaner shape.
- The page captures the interface in its data-context-changed handler and calls it
  null-safely.

### Run and pause a game engine session inside a page

**When you want this.** The engine loop should run while one part of the UI is on
screen and cost nothing while the user is elsewhere, without tearing the scene
down and rebuilding it.

**The MVVM shape.** A session class owns the engine lifecycle and exposes start,
pause, resume, stop and a thread-safe data-in method. The view model calls those
from its commands and from `Dispose()`. Nothing else touches the engine instance.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
public void Start()
{
    if (IsStarted) { return; }

    //GpuRendering-OpenGL (GPU) by default; must be chosen before the first access to Host. The
    //  render resolution tracks the window (no SetRenderResolution) - the shader
    //  scene is resolution-independent.
    _canvas.UseGpuRendering = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1";

    _renderSurface = _canvas.Host;
    _renderSurface.ViewManager.ConfigureSingleFullView();

    Engine.Instance.Start(SynchronizationContext.Current);
    Engine.Instance.Configuration.TargetFPS = 60;

    var adapter = _renderSurface.RenderSurfaceAdapter;
    var view = _renderSurface.ViewManager.Views[0];

    _backdrop = new EtherealBackdrop(_renderSurface, view,
        new Rectangle(0, 0, adapter.Width, adapter.Height), _attractorField);
    _backdrop.ZOrder = 0;

    //The render resolution tracks the window, so follow adapter resizes
    adapter.Resized += OnAdapterResized;

    IsStarted = true;
}

public void Pause()
{
    if (!IsStarted || Engine.Instance.IsPaused) { return; }

    _attractorField.Reset();
    Engine.Instance.Pause();
}

public void Resume()
{
    if (!IsStarted || !Engine.Instance.IsPaused) { return; }

    Engine.Instance.Resume();
}

public void Stop()
{
    if (!IsStarted) { return; }

    _renderSurface.RenderSurfaceAdapter.Resized -= OnAdapterResized;
    Engine.Instance.Stop();
    IsStarted = false;
}

private void OnAdapterResized(RenderSurfaceAdapterResizedEventArgs args)
{
    if (_backdrop != null)
        _backdrop.ScreenBounds = new Rectangle(0, 0, args.NewWidth, args.NewHeight);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private Task DoVisualize()
{
    if (!CanVisualize()) { return Task.CompletedTask; }

    if (_tracker == null)
    {
        _tracker = new PalmTracker();
        _tracker.TrackingUpdated += OnTrackingUpdated;
    }
    _tracker.Start();
    _reportedOpenPalmCount = 0;

    //Showing the game canvas gives it its first real layout size, which raises its
    //  FirstStarted -> CanvasFirstStart the first time through; on later entries the
    //  engine is merely paused from Camera Mode, so wake it back up
    IsCameraMode = false;
    _visualizerSession?.Resume();

    StatusText = "Show the camera your open palm - the colors will gather toward it.";
    return Task.CompletedTask;
}

private Task DoGoBack()
{
    if (!CanGoBack()) { return Task.CompletedTask; }

    _tracker?.Stop();
    _visualizerSession?.Pause();

    IsCameraMode = true;
    InvalidatePreviewCanvas?.Invoke();
    StatusText = SelectedCamera != null
        ? $"Live: {SelectedCamera.FriendlyName}"
        : "Select a camera.";
    return Task.CompletedTask;
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The GPU-or-CPU choice must be made before the first access to the canvas's host;
  reading the host first locks the choice in.
- The engine is started with the current synchronization context, so it must be
  called on the UI thread - which is guaranteed here because it runs from the
  canvas's first-started event.
- Starting is once per process. Use pause and resume to leave and re-enter the
  mode; every one of the four methods is guarded by the started flag and by the
  engine's own paused flag, so double calls are harmless.
- The pause is invisible to engine time, so a resumed scene picks up mid-motion.
  Resetting the scene's input state on pause is what makes it resume undisturbed
  rather than with stale input still acting on it.
- Configuring a single full view plus a zero draw order is the whole scene graph
  here; the backdrop fills the one view.
- Subscribe to the render adapter's resize event and update the drawing's screen
  bounds; the render resolution tracks the window because the resolution is
  deliberately not pinned.
- Stopping unsubscribes the resize handler before stopping the engine.

