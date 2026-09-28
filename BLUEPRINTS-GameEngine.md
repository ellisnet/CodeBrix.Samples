# CodeBrix.Samples Blueprints: Hosting a game engine

These recipes cover three things. The first is hosting the CodeBrix.Platform
GameEngine loop inside an ordinary page: handing the view model a game canvas
once it has a real, non-zero layout size, and owning the engine lifecycle in a
session class so the loop can start, pause, resume and stop as the user moves
around the UI without tearing the scene down and rebuilding it. The second is
the engine's music system: pinning the audio device format before anything
plays, handing a track the beat grid it cannot derive and letting the files
that carry their own speak for themselves, landing a transition on the next
bar of a piece whose tempo changes partway through, crossfading layered stems
and a whole stems export, ducking the music under a one-shot cue or a line of
dialogue, and getting a banner onto the one frame the engine renders on its
way into a pause. The third is building a complete game on the engine: art,
sound and fonts read straight from downloaded Kenney zip files, endless music
generated while the game runs, keyboard and gamepad read together with
prompts that follow the device in hand, a splash card that hands over to the
title screen, and game rules kept in a deterministic library of their own
that a unit test can play from start to finish; each frame built as engine draw
lists and painted from their published copies, a fixed playfield letterboxed into any
window with clicks arriving in playfield pixels, one painter per
screen, held menu directions that repeat, power-ups shown on the player's
ship, engine particles layered between the game's own
drawings, and a whole-engine pause while the window is minimized; and the
game's side of the generated music - its policy behind an interface a test
fakes, a duck under the pause menu, a per-level table of choices, and credits
that name what is really playing. Reach for this file when an
engine-driven surface has to live alongside regular pages and controls, when
the music has to react to what is happening on that surface, or when you are
building a whole game rather than a surface.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Hand the view model a game canvas at its first real layout size](#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size)
- [Run and pause a game engine session inside a page](#run-and-pause-a-game-engine-session-inside-a-page)
- [Pin the audio device format before anything plays](#pin-the-audio-device-format-before-anything-plays)
- [Tell the engine the tempo it cannot derive and let it derive the rest](#tell-the-engine-the-tempo-it-cannot-derive-and-let-it-derive-the-rest)
- [Quantize a music transition to the next bar across a tempo change](#quantize-a-music-transition-to-the-next-bar-across-a-tempo-change)
- [Crossfade layered stems and a stems export by name](#crossfade-layered-stems-and-a-stems-export-by-name)
- [Duck the music for exactly as long as a line lasts](#duck-the-music-for-exactly-as-long-as-a-line-lasts)
- [Write a pause overlay onto the engine's final forced frame](#write-a-pause-overlay-onto-the-engines-final-forced-frame)
- [Register downloaded Kenney zip files with RegisterKenneyAssets and load sprites, sounds and fonts from them](#register-downloaded-kenney-zip-files-with-registerkenneyassets-and-load-sprites-sounds-and-fonts-from-them)
- [Start endless generated music with one call](#start-endless-generated-music-with-one-call)
- [Read keyboard and gamepad together through an InputActionMap and switch on-screen prompts](#read-keyboard-and-gamepad-together-through-an-inputactionmap-and-switch-on-screen-prompts)
- [Show a splash card and hand over to a title screen](#show-a-splash-card-and-hand-over-to-a-title-screen)
- [Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate](#keep-a-deterministic-game-simulation-apart-from-the-engine-and-step-it-from-onfixedupdate)
- [Build each frame as engine DrawList commands and paint only the published copy](#build-each-frame-as-engine-drawlist-commands-and-paint-only-the-published-copy)
- [Pin a fixed playfield size and let the engine letterbox drawings and clicks](#pin-a-fixed-playfield-size-and-let-the-engine-letterbox-drawings-and-clicks)
- [Paint each game screen with its own painter and test the screens as draw lists](#paint-each-game-screen-with-its-own-painter-and-test-the-screens-as-draw-lists)
- [Repeat a held menu direction with InputRepeat and ignore a stick springing back](#repeat-a-held-menu-direction-with-inputrepeat-and-ignore-a-stick-springing-back)
- [Show power-ups on the player's ship by layering sprite parts over the hull](#show-power-ups-on-the-players-ship-by-layering-sprite-parts-over-the-hull)
- [Stack engine particles on a pixel layer between your own world and overlay drawings](#stack-engine-particles-on-a-pixel-layer-between-your-own-world-and-overlay-drawings)
- [Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu)
- [Put the music policy behind an interface and test it against fakes of the engine's music interfaces](#put-the-music-policy-behind-an-interface-and-test-it-against-fakes-of-the-engines-music-interfaces)
- [Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title](#duck-the-music-for-a-pause-menu-and-hold-a-game-over-duck-with-playstingerwithheldduck-until-the-title)
- [Keep the music for each level in a table the tests can read](#keep-the-music-for-each-level-in-a-table-the-tests-can-read)
- [Show the player which model and instruments are really playing](#show-the-player-which-model-and-instruments-are-really-playing)

## Related blueprints

- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - the one-method bridge interface a page implements to hand its game canvas host to its view model
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - drawing on Skia canvases outside the engine loop, and gating or falling back when a GPU backend is unavailable
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the commands and Dispose path that drive the session class
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](BLUEPRINTS-ProjectLayoutAndPackaging.md) - writing the audio, instrument and stems assets the music system plays from arithmetic on first run, rather than committing them
- [BLUEPRINTS-Testing.md](BLUEPRINTS-Testing.md) - the opt-in walkthrough that checks the music system's audible-only behavior from inside the shipped library, and the throwaway settings store a game's tests and unattended runs use
- [BLUEPRINTS-SettingsAndPersistence.md](BLUEPRINTS-SettingsAndPersistence.md) - the application-named facade a game keeps its settings and high scores behind

---

## Hosting a game engine

### Hand the view model a game canvas at its first real layout size

**When you want this.** You want the CodeBrix.Platform GameEngine loop rendering
inside an ordinary page, and the engine can only be started against a surface that
already has a non-zero size - which, for a canvas that starts hidden, is the first
time it is shown.

**The MVVM shape.** The view model declares an interface with one method, and the
page implements a second interface whose only member is the canvas. The page
forwards the canvas's first-started event in a single line, handing itself over as
that host; the view model asks a registered factory for a session built around the
host and starts it. No engine code lives in the code-behind, and no view type
appears anywhere in the view model.

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
    /// <param name="host">The page that owns the game canvas the visualizer renders into.</param>
    void CanvasFirstStart(IGameCanvasHost host);
}
```

The host interface lives in the rendering library, beside the session it feeds: one
get-only `Canvas` property, which the page implements explicitly so the property
does not join its public surface. That is the whole seam, and it is why the view
model can name the host without naming a control.

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs
//Fires once, at the canvas's first non-zero layout size - i.e. the first time
//  Visualize Mode is shown - which is when the engine can start
VisualizerCanvas.FirstStarted += (_, _) => _gameCanvasManager?.CanvasFirstStart(this);
// ...
//The one thing the visualizer session needs from this page (see IGameCanvasHost)
GameSurfaceCanvas IGameCanvasHost.Canvas => VisualizerCanvas;
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
public void CanvasFirstStart(IGameCanvasHost host)
{
    //UI thread, the first time Visualize Mode is shown with a real size: build the
    //  shader scene and start the engine. Later mode switches pause and resume it.
    _visualizerSession = _sessionFactory.CreateSession(host);
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
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/IGameCanvasHost.cs` and
`IVisualizerSessionFactory.cs`
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Also shown by.**
`BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs` and
`BrixInvaders/src/BrixInvaders.UI/Views/MainPage.xaml.cs` (the page hands over
the canvas itself rather than itself as a host, because the view model must
choose the render tier and pin the render resolution before anything reads the
canvas's host, and the view model then builds the game host directly rather than
through a factory - see
[Build a game host's seams in the view model and close the application from its quit event](BLUEPRINTS-MVVM.md#build-a-game-hosts-seams-in-the-view-model-and-close-the-application-from-its-quit-event))

**Sharp edges.**
- The first-started event fires once. Starting the engine from the page's loaded
  event, or from the command that switches modes, would run against a zero-sized
  surface.
- The order inside the command matters: making the canvas visible - which is what
  raises the event the first time - comes before resuming the session, and the
  resume is null-safe because on the first pass the session does not exist yet.
- The method takes the host page rather than the canvas, so the canvas type is named
  only inside the rendering library; the session itself is built by a factory the
  view model resolves, which keeps a view-bound object out of the view model's
  constructor.
- The page captures the interface in its data-context-changed handler and calls it
  null-safely.

### Run and pause a game engine session inside a page

**When you want this.** The engine loop should run while one part of the UI is on
screen and cost nothing while the user is elsewhere, without tearing the scene
down and rebuilding it.

**The MVVM shape.** A session class owns the engine lifecycle and exposes start,
pause, resume, stop and a thread-safe data-in method, all of them on an interface
the rendering library declares. The view model holds the session as that
interface, built for it by a registered factory, and calls those members from its
commands and from `Dispose()`. Nothing else touches the engine instance.

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

    //Starting the tracker is what loads the models, on its own worker thread
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
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs` and
`IVisualizerSession.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (the
whole engine paused while the window is minimized rather than while a mode is
left, by the engine's `GameWindowLifecycle` - see
[Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu))

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

## The music system

### Pin the audio device format before anything plays

**When you want this.** Your application mixes audio whose sources do not agree
about sample rate - loops you generated at one rate, a stems export downloaded at
another, an instrument rendered live - and you want them to line up rather than be
refused for disagreeing.

**The MVVM shape.** One library class owns the engine and the music system. The
view model builds that class when the canvas reaches its first real layout size and
holds it; the page never touches the engine. Start-up order is the library class's
business, and pinning the device is the first thing it does after making sure the
assets exist.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
public void Start()
{
    MusicAssetFactory.EnsureAssets();

    // Pin the device before anything plays, so every voice converts to one known format.
    AudioSystem.Initialize(MusicAssetFactory.SampleRate, 2);

    var renderSurface = _canvas.Host;
    var adapter = renderSurface.RenderSurfaceAdapter;
    renderSurface.ViewManager.ConfigureSingleFullView();

    Engine.Instance.CPSCalculated += _ => _readout?.SetText(BuildReadout());
    Engine.Instance.Start(SynchronizationContext.Current);
    Engine.Instance.Configuration.TargetFPS = 30; // a text readout does not need more

    BuildTracks();
    BuildReadoutDisplay(renderSurface, adapter.Width, adapter.Height);

    // Off unless the environment asks for it; see GameEngineMusicDemoWalkthrough for what it is for.
    if (GameEngineMusicDemoWalkthrough.IsRequested)
    {
        GameEngineMusicDemoWalkthrough.Start(this);
    }
}
```

The reason is written into the class's own documentation, next to the contract the
pause control demonstrates:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <para>
/// The device format is PINNED at start-up (<see cref="AudioSystem.Initialize"/>). That is what lets
/// a stem set of assorted source rates line up, because stems then rate-convert to the pinned rate as
/// they decode rather than being rejected for disagreeing.
/// </para>
```

A rate conversion that only happens on someone else's machine is a rate conversion
nobody tests, so the sample generates its stems export at a rate it does not pin,
and a unit test holds the two apart:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/tests/libs/GameEngineMusicDemo.Game.Tests/MusicAssetFactoryTests.cs
[Fact]
public void The_stems_export_is_written_at_a_rate_the_device_is_not_pinned_to()
{
    //Arrange
    var deviceRate = MusicAssetFactory.SampleRate;

    //Act
    var exportRate = MusicAssetFactory.StemsExportSampleRate;

    //Assert
    deviceRate.Should().Be(44100);
    exportRate.Should().Be(48000);
    (exportRate == deviceRate).Should().Be(false);
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs` and
`tests/libs/GameEngineMusicDemo.Game.Tests/MusicAssetFactoryTests.cs`

**Also shown by.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`OnInitializing`, the first override the game host runs: pinned before the
effects are preloaded and before the generated music starts, at a rate and
channel count the game's music library publishes as constants, with one log line
saying what was pinned)

**Sharp edges.**
- Pin before the first voice exists, not before the first play. Anything decoded
  ahead of the pin has already settled on a format of its own.
- The engine is started with the current synchronization context, so this method
  has to run on the UI thread; it does, because it runs from the canvas's
  first-started event.
- The frame rate is set after the engine is started, and dropping it is free here -
  the canvas draws a text readout, not a scene.
- The teardown that pairs with this - stop the engine, dispose the tracks, shut the
  audio system down - exists on the same class as `Stop()`, but this application
  never calls it. In a real application it belongs on the page's way out.

### Tell the engine the tempo it cannot derive and let it derive the rest

**When you want this.** You are building the tracks a game will play from, and you
want to know which of them need a beat grid handed to them and which arrive with
one already, so that bar-locked transitions and layer changes work everywhere
rather than only on the tracks you remembered to configure.

**The MVVM shape.** Track construction belongs in the library class that owns the
music system, called once from its start-up method. The view model exposes that
class; nothing about a timeline reaches the page.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
private void BuildTracks()
{
    var resources = AudioResourceManager.Instance;

    var timeline = new MusicTimeline(MusicAssetFactory.BeatsPerMinute, MusicAssetFactory.BeatsPerBar);

    // Decoded audio cannot be asked what tempo it is, so the game says. The composer knows it;
    // the engine deliberately refuses to guess.
    _trackA = new FileMusicTrack("Track A", resources.LoadFromFile("track-a", MusicAssetFactory.TrackAPath))
    {
        IsLooping = true,
        Timeline = timeline,
    };

    // ...

    // A MIDI track loaded FROM A PATH derives its own timeline - tempo, time signature and the
    // markers that become jump points - with nothing set here.
    _midiTrack = new MidiMusicTrack("MIDI Theme", MusicAssetFactory.InstrumentPath, MusicAssetFactory.MidiPath)
    {
        IsLooping = true,
    };

    _stems = new MusicStemSet("Adaptive Stems", MusicAssetFactory.StemNames, DecodeStems())
    {
        IsLooping = true,
        Timeline = timeline,
    };

    // ...

    // A stems export - the shape a music service hands over - straight into a stem set. The
    // grid comes out of the MIDI beside the recordings, so bar-locked layer changes and
    // bar-quantised transitions work with nothing else set up.
    _songStems = MusicStemSet.FromSunoStems("Song Stems", MusicAssetFactory.StemsExportFolder,
        "Vocals", "Drums", "Bass");
    _songStems.IsLooping = true;

    // ...

    LogWhatWasLoaded();
}
```

What a file said about itself is invisible on screen, so the sample writes a line
per loaded track. Which instrument format was used, what grid came out of the file,
how many markers it carried and how many problems the reader recorded are the first
four things you want the day a file misbehaves:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
Engine.Logger.LogInformation(
    "GameEngineMusicDemo: MIDI track '{Key}' loaded through an SFZ instrument. Grid: {Tempo:0} BPM, "
    + "{BeatsPerBar:0} beats/bar, markers: {Markers}. Problems: {Problems}.",
    _midiTrack.Key,
    _midiTrack.Timeline?.BeatsPerMinute ?? 0,
    _midiTrack.Timeline?.BeatsPerBar ?? 0,
    _midiTrack.Timeline is null ? "(none)" : string.Join(", ", _midiTrack.Timeline.Markers),
    _midiTrack.Problems.Count);
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
(`BuildTracks`, `LogWhatWasLoaded` and `DecodeStems`)

**Sharp edges.**
- Decoded audio carries no tempo, and the engine will not invent one. A track with
  no timeline is not broken - it simply cannot be quantized, and a transition asked
  to wait for a bar on it runs immediately instead.
- A MIDI track and a stems export bring their own grid, markers included. Setting a
  timeline over one of those replaces what the file said with what you assumed.
- Two tracks generated at the same tempo can share one timeline instance, which is
  what makes a crossfade between them land on the same downbeat.
- An instrument that is a folder rather than a file - a preset pointing at samples
  beside it - is loaded by path from disk, never out of an asset pack.

### Quantize a music transition to the next bar across a tempo change

**When you want this.** A crossfade that cuts in wherever the user clicked sounds
like an accident, and you want the change to land on the downbeat instead - on a
piece whose tempo does not stay where it started.

**The MVVM shape.** The transition is one method on the library class, taking the
boundary to wait for as a parameter, so the immediate and the bar-locked forms are
one method called with different arguments. The page passes the argument; the
decision about what to wait for is the only thing the view layer contributes.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <summary>Crossfades to the second linear track, optionally waiting for the next bar.</summary>
/// <param name="quantize">The boundary to wait for.</param>
public void CrossfadeToTrackB(MusicTransitionQuantize quantize)
{
    LogQuantisedWait(quantize);
    MusicManager.Instance.CrossfadeTo(_trackB, TimeSpan.FromSeconds(2), quantize);
}
```

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
// Says what the grid answered and what a constant-tempo grid WOULD have answered. On a file
// whose tempo changes the two differ, which is the whole point of quantising through the map.
private static void LogQuantisedWait(MusicTransitionQuantize quantize)
{
    if (quantize == MusicTransitionQuantize.Immediate)
    {
        return;
    }

    var current = MusicManager.Instance.NowPlaying;
    var timeline = current?.Timeline;

    if (current is null || timeline is null)
    {
        Engine.Logger.LogInformation(
            "GameEngineMusicDemo: a {Quantize} transition was asked for with no grid to wait on, so it runs now.",
            quantize);

        return;
    }

    var position = current.Position;
    var wait = timeline.TimeToNextBoundary(position, quantize);
    var grid = quantize == MusicTransitionQuantize.Bar ? timeline.SecondsPerBar : timeline.SecondsPerBeat;
    var atOpeningTempo = grid - (position.TotalSeconds % grid);

    // ...
}
```

The application wires each button straight to a handler rather than to a command,
because the sample is about showing the API being called; the three lines below are
the whole difference between the two transitions and the one that drops a queued
one:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs
private void OnCrossfadeNow(object sender, object e)
    => Demo?.CrossfadeToTrackB(MusicTransitionQuantize.Immediate);

private void OnCrossfadeOnBar(object sender, object e)
    => Demo?.CrossfadeToTrackB(MusicTransitionQuantize.Bar);

private void OnCancelQueued(object sender, object e) => Demo?.CancelQueuedTransition();
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
(`CrossfadeToTrackB`, `CrossfadeToTrackA` and `LogQuantisedWait`)
`GameEngineMusicDemo/src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The wait is asked of the track that is playing now, not of the one being faded
  to. A destination with a fine grid does not rescue a source with none.
- A quantized transition asked for with no grid to wait on is not an error: it runs
  immediately. Log that case, or it looks like the boundary was ignored.
- A grid fixed at the file's opening tempo answers a different number from a real
  tempo map as soon as the tempo moves, which is the whole reason a timeline is a
  map rather than a number.
- A queued transition survives until it fires or is canceled, and it cannot fire
  while the engine is paused.

### Crossfade layered stems and a stems export by name

**When you want this.** The music has to react - a layer arriving when things get
tense, a layer leaving when they calm down - without the layers ever drifting out
of step with each other, and you want the same thing to work over a set of stem
files somebody downloaded as it does over loops you built yourself.

**The MVVM shape.** The stem sets belong to the library class, which exposes the
hand-built set for the per-layer controls and a by-name fade for the export. This
application drives both from page handlers, deliberately: an application that is
not an API demonstration would put the gain writes and the fades on the view model
and bind the sliders to it.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
_stems = new MusicStemSet("Adaptive Stems", MusicAssetFactory.StemNames, DecodeStems())
{
    IsLooping = true,
    Timeline = timeline,
};

// ...

private static CachedSound[] DecodeStems()
{
    var stems = new CachedSound[MusicAssetFactory.StemPaths.Length];
    for (var i = 0; i < stems.Length; i++)
    {
        stems[i] = CachedSound.FromFile(MusicAssetFactory.StemPaths[i]);
    }

    return stems;
}
```

The export form takes the folder and the layer names and nothing else; its fades
are then addressed by those names:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
_songStems = MusicStemSet.FromSunoStems("Song Stems", MusicAssetFactory.StemsExportFolder,
    "Vocals", "Drums", "Bass");
_songStems.IsLooping = true;

// ...

/// <summary>Fades one layer of the stems export in or out.</summary>
/// <param name="stemName">The stem's name, as it appears on the export's file names.</param>
/// <param name="target">The gain to fade to, 0.0 to 1.0.</param>
public void FadeSongStem(string stemName, float target)
    => _songStems?[stemName].FadeTo(target, TimeSpan.FromSeconds(2));
```

A slider writes a layer's gain directly, but a fade is the layer's own job, and the
control has to get out of its way. The application keeps this pair in the page,
where the slider lives:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs
private void FadeStem(int index, float target, Slider slider)
{
    var stems = Demo?.Stems;
    if (stems is null)
    {
        return;
    }

    stems[index].FadeTo(target, TimeSpan.FromSeconds(2));

    // The slider would otherwise keep showing where the layer WAS; setting it here would fight
    // the fade, so it is moved to the destination and the fade is left to do the audible part.
    slider.Value = target * 100.0;
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
(`PlayStems`, `PlaySongStems` and `FadeSongStem`)
`GameEngineMusicDemo/src/GameEngineMusicDemo.UI/Views/MainPage.xaml.cs` (`SetStemGain`,
`FadeStem` and `SyncStemSliders`)

**Sharp edges.**
- A hand-built set needs layers that agree about length, sample rate and channel
  count; they play as one voice, and a layer that disagrees has nowhere to be.
- The hand-built set is addressed by index and the export by name, and the export's
  names are the names in its file names. Misspell one and the indexer is what
  fails, not the load.
- Writing a gain and starting a fade are different actions on the same layer. A
  control that keeps writing the gain while a fade runs erases the fade.
- Layer gains mean nothing until the set is playing, so a panel that fades layers
  needs its play button pressed first - and after a play, push the control values
  back out of the layers so the two agree.

### Duck the music for exactly as long as a line lasts

**When you want this.** Something has to be heard over the music - a fanfare, a
line of dialogue, a warning - and the music has to come back afterwards, on its
own, even when two of those overlap.

**The MVVM shape.** Both duck shapes are methods on the library class that owns the
music system, so the lifetime rule lives with the thing that has the lifetime. The
page contributes a button press.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <summary>Fires the one-shot fanfare on its own voice, ducking the music under it.</summary>
public void PlayStinger() => MusicManager.Instance.PlayStinger("stinger", 0.9f, duckMusic: true);
```

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <summary>
/// Plays the stand-in dialogue line and ducks the music for exactly as long as it lasts, using
/// the handle form so overlapping lines reference-count correctly.
/// </summary>
public void PlayDuckedDialogue()
{
    var voice = AudioResourceManager.Instance.Clone("voice", $"voice_{Guid.NewGuid():N}");
    if (voice is null)
    {
        return;
    }

    var duck = MusicManager.Instance.PushDuck(0.25f, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(600));

    void OnCompleted(object sender, EventArgs e)
    {
        voice.PlaybackCompleted -= OnCompleted;
        duck.Dispose();
        AudioResourceManager.Instance.Unload(voice.Key);
    }

    voice.PlaybackCompleted += OnCompleted;
    voice.Play();
}
```

The third shape is the same handle with the lifetime made visible, and the
null-coalescing assignment is what makes a second press harmless:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <summary>Holds a duck open until <see cref="ReleaseHeldDuck"/>, to show the handle form.</summary>
public void HoldDuck()
    => _heldDuck ??= MusicManager.Instance.PushDuck(0.2f, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(800));

/// <summary>Releases the held duck.</summary>
public void ReleaseHeldDuck()
{
    _heldDuck?.Dispose();
    _heldDuck = null;
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
(`PlayStinger`, `PlayDuckedDialogue`, `HoldDuck`, `ReleaseHeldDuck` and `Stop`)

**Also shown by.**
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs` (both
forms on generated music: the timed form under the boss warning, and the handle
form held for the pause menu and - from the engine's held-duck stinger - the
game-over screen; see
[Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title](BLUEPRINTS-GameEngine.md#duck-the-music-for-a-pause-menu-and-hold-a-game-over-duck-with-playstingerwithheldduck-until-the-title))

**Sharp edges.**
- Two shapes, one rule: a stinger ducks for its own length and needs nothing
  released, and a pushed duck lasts exactly as long as you hold the handle.
- `PlayStinger` plays on the music bus, so the music slider and its own duck turn
  the stinger down with the music. For a cue that must stay at full level, the
  engine's `PlayStingerOnBus` with `AudioBus.Sfx`, and `PlayStingerWithHeldDuck` by
  default, play it on the effects bus.
- Clone the resource per line. Two lines sharing one instance fight over the same
  playback position and one completion event.
- Dispose the duck and unload the clone from the voice's own completion event, and
  unsubscribe the handler inside itself. Ducks reference-count, so overlapping
  lines each hold their own and the music comes back when the last one lets go.
- A held duck outlives everything until it is released, which is why teardown
  releases it before disposing the music manager.

### Write a pause overlay onto the engine's final forced frame

**When you want this.** Pausing the engine has to look paused. You want a banner on
the screen while everything behind it is suspended, and the engine's per-cycle
refresh has stopped being a place you can write from.

**The MVVM shape.** The readout is a drawing the library class owns, refreshed from
the engine's own per-cycle event rather than from anything the controls do. The
page's pause button calls one method; the method is where the ordering rule lives.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
Engine.Instance.CPSCalculated += _ => _readout?.SetText(BuildReadout());
```

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
/// <summary>Toggles the global engine pause.</summary>
// ...
public void TogglePause()
{
    if (Engine.Instance.IsPaused)
    {
        Engine.Instance.Resume();
    }
    else
    {
        _readout?.SetText(BuildReadout(pausing: true));
        Engine.Instance.Pause();
    }
}
```

The readout builder takes the flag because it is being asked for a frame that has
not happened yet, and it checks the engine's own state as well so a redraw after
the fact still says the same thing:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs
private string BuildReadout(bool pausing = false)
{
    // ...
        .AppendLine($"Fades       : {manager.ActiveFadeCount} in flight"
                    + (manager.HasPendingTransition ? "   [transition queued for the next bar]" : string.Empty))
    // ...
    if (pausing || Engine.Instance.IsPaused)
    {
        text.AppendLine().AppendLine("*** PAUSED - music suspended, fades frozen ***");
    }

    return text.ToString();
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoGame.cs`
(`TogglePause`, `BuildReadout` and the per-cycle subscription in `Start`)

**Also shown by.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (the
other way round: nothing is written onto the forced frame, and the game's own
pause menu is up once the window comes back - see
[Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu))

**Sharp edges.**
- The cycle parks while paused, so text written after the pause call is never
  drawn. The engine renders one forced final frame on its way in, and writing
  immediately before the call is what gets onto it.
- Resume needs no such trick: the cycle starts again and the next refresh clears
  the banner by itself.
- Pausing suspends the music and freezes every fade in flight together, and a
  transition queued for the next bar cannot fire while paused - so the readout
  still shows it queued, which is the contract rather than a stale value.
- Refreshing the readout from the engine's per-cycle event, not from the controls,
  is what keeps every handler on the panel from having to remember to redraw.


## Building a complete game

### Register downloaded Kenney zip files with RegisterKenneyAssets and load sprites, sounds and fonts from them

**When you want this.** You downloaded a handful of Kenney asset packs and want to
start building the game on them today: ship the zips exactly as they came, hand them
to the engine's `RegisterKenneyAssets` in one call, and ask the engine for a sprite, a
sound or a font by name, with no extraction step, no converted atlas and no asset
pipeline of your own.

**The MVVM shape.** Not a view-model concern. The zips are data the Core project
copies beside every head's executable. One library owns the pack names, every asset
key the game uses and a one-call registration; the game host calls it from its
`LoadAssets` override, once the engine exists and before anything is drawn. The view
model never sees an asset.

**Code.**

The zips travel beside the executable as the files they were downloaded as:

```xml
<!-- From CodeBrix.Samples/BrixInvaders/src/BrixInvaders.Core/BrixInvaders.Core.csproj -->
<ItemGroup>
  <!-- The five Kenney bundles travel next to the executable as the .zip files they are downloaded as (plus
       Kenney's bundle promo picture); the asset provider reads a bundle where it lies, from assets/kenney.
       None items rather than Content: the CodeBrix.Platform build targets re-root every copy-to-output Content
       item of a non-head project under $(AssemblyName)/, which would ship a second copy of the zips in a
       BrixInvaders.Core folder beside the executable. None items flow to the heads at assets/kenney only. -->
  <None Include="..\..\assets\**\*" Link="assets\%(RecursiveDir)%(Filename)%(Extension)" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

One call to the engine's `RegisterKenneyAssets` hands all of them to the Kenney asset
provider and reports per zip what arrived, so the library logs one line per pack, warns
about a zip that did not ship and refuses to start with none. It also refuses a folder
with none of the zips in it before the process-wide provider is touched:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Assets/Loading/BrixInvadersAssets.cs
public static KenneyGameAssetProvider Register(Engine engine, string folder, Action<string> log = null)
{
    ArgumentNullException.ThrowIfNull(engine);
    ArgumentNullException.ThrowIfNull(folder);

    string root = Path.GetFullPath(folder);
    if (!Directory.Exists(root))
    {
        throw new InvalidOperationException(NoPacksMessage(root, "the folder does not exist"));
    }

    IReadOnlyList<string> zipPaths = KenneyPacks.ZipPaths(root);
    if (!zipPaths.Any(File.Exists))
    {
        throw new InvalidOperationException(NoPacksMessage(root, "none of the zips is there"));
    }

    KenneyAssetsRegistration registration = engine.RegisterKenneyAssets(zipPaths.ToArray());
    if (registration.Packs.Count == 0)
    {
        throw new InvalidOperationException(NoPacksMessage(root, "none of the zips could be read"));
    }

    // ...
    foreach (KenneySourceResult unavailable in registration.Unavailable)
    {
        Write($"{LogPrefix} WARNING: {unavailable}; its assets are missing", log);
    }

    // ...
    return registration.Provider;
}
```

After that, every load is an ordinary engine call with a key. Sprites come from the
packs' own sprite-sheet atlases and are addressed by the frame names Kenney gave them
(the engine's `DrawImageLibrary` finds each one by atlas key and frame name);
sounds and fonts go into the engine's own registries:

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Assets/Loading/BrixInvadersAssets.cs
public static Tilesheet LoadMainAtlas(Engine engine) => LoadImage(engine, AssetKeys.Atlases.Main);

public static BrixInvadersSounds LoadSounds(Engine engine)
{
    ArgumentNullException.ThrowIfNull(engine);

    Dictionary<SoundEffect, AudioResource> resources = [];
    foreach (KeyValuePair<SoundEffect, string> pair in SoundEffects.Keys)
    {
        resources[pair.Key] = engine.Managers.AssetProviders.LoadAudio(pair.Value);
    }

    return new BrixInvadersSounds(resources);
}

public static BrixInvadersFonts LoadFonts(Engine engine)
{
    ArgumentNullException.ThrowIfNull(engine);

    engine.Managers.AssetProviders.LoadFont(AssetKeys.Fonts.Future);
    engine.Managers.AssetProviders.LoadFont(AssetKeys.Fonts.FutureThin);
    FontManager fonts = engine.Managers.Fonts;

    return new BrixInvadersFonts(
        AssetKeys.Fonts.Future,
        AssetKeys.Fonts.FutureThin,
        fonts.GetFamilyName(AssetKeys.Fonts.Future),
        fonts.GetFamilyName(AssetKeys.Fonts.FutureThin));
}
```

A key is the provider id, the pack's slug and the path inside the zip without its
extension, and the game keeps every one it uses in one class:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Assets/Keys/AssetKeys.cs
public static class Fonts
{
    /// <summary>Kenvector Future: titles, HUD numbers, menus.</summary>
    public const string Future = "kenney:space-shooter-remastered/Bonus/kenvector_future";
    // ...
}
```

The host's `LoadAssets` override is then a handful of lines, and the provider's own
`CheckKeys` logs how every key in that class resolved:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
protected override void LoadAssets()
{
    _provider = BrixInvadersAssets.Register(Engine);
    var check = _provider.CheckKeys(AssetKeyCatalog.AllKeys);
    GameLog.Write($"assets: {check}");
    foreach (var key in check.MissingKeys)
    {
        GameLog.Write($"assets: WARNING: key not found: {key}");
    }

    var sounds = BrixInvadersAssets.LoadSounds(Engine);
    // ...
    var fonts = BrixInvadersAssets.LoadFonts(Engine);
    _frame = new FrameLists(new DrawList(_images, 1024), new DrawList(_images), FontManager.Instance.Get(fonts.TextKey),
        FontManager.Instance.Get(fonts.ThinKey));
    // ...
}
```

**Where to look.**
`BrixInvaders/src/BrixInvaders.Core/BrixInvaders.Core.csproj`
`BrixInvaders/src/libs/BrixInvaders.Assets/Loading/BrixInvadersAssets.cs`,
`Packs/KenneyPacks.cs` and `Keys/AssetKeys.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`LoadAssets`, `LoadTilesheets`)
`BrixInvaders/tests/libs/BrixInvaders.Assets.Tests/` (the real zips, copied beside the test binary)

**Sharp edges.**
- Copy the zips as `None` items from the Core project, not `Content`. The platform
  build targets re-root a non-head project's copy-to-output `Content` under a folder
  named for the assembly, so the heads would carry a second copy of every zip.
- Register through `RegisterKenneyAssets`, not `UseKenneyAssets`. The provider is
  process-wide; adding the same zip again through `UseKenneyAssets` gives its pack a
  second slug with a numeric suffix and a second set of keys, while
  `RegisterKenneyAssets` reports a zip it already holds as already registered.
- A pack's slug comes from the title line of its license file, or from the zip's file
  name when that line is unusable. Read the slugs from the provider once, keep them as
  constants, and let a test hold them, so a change in how a pack is named fails a test
  rather than a frame.
- Prefer a pack's sprite-sheet atlas to its loose images: one tilesheet, frames by
  name, and one texture for the renderer.
- Keep every key the game uses in one class and prove in a test that each resolves
  against the real zips (the provider's `CheckKeys(keys).MissingKeys` is empty). A typo
  in a key is then a failing test, not a missing sprite discovered in play; one that
  still slips through fails its load with the engine's "Did you mean" list of the
  nearest keys.
- A picture that is not part of a pack - here Kenney's promo image for the whole
  bundle - does not go through the provider. Load it as a plain tilesheet from its
  file, and give its default region the whole image as its tile, or `sheet[0, 0]` is
  not the picture.
- Kenney's content is CC0, so credit is optional; give it anyway, on screen (the
  provider's `CreditLines` gives one line per pack) and in the application's notices file.

### Start endless generated music with one call

**When you want this.** The game should have music from the first screen to the last,
it should never loop audibly, and it should change character with the level - without
anyone composing, recording or shipping a soundtrack.

**The MVVM shape.** Split in three. A small library owns the choices the player can
make and a table of which preset each level plays, and turns settings into options.
One class in the game library owns the music policy - when to start, follow up, duck
and restart - behind an interface, so the game session's tests drive it with a fake.
The director reaches the engine only through the engine's own seams (a session
starter, the music manager and the engine dispatcher), so its tests fake those. The
view model constructs the real director and hands it to the game host.

**Code.**

The package references are the engine's generated-music add-in, the two model packages
and the recorded instrument library, plus the synthesized instrument library named
explicitly because the game registers it. The model and SoundFont packages copy their
files into the output of every project that reaches them, so a library that never
loads them switches the copy off for its own output:

```xml
<!-- From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Music/BrixInvaders.Music.csproj -->
<!-- The model and SoundFont packages copy their files (model weights, the SoundFont) into the output of every
     project that reaches them. This library never loads them, so its own output skips the copy; the app heads
     that reach these packages through BrixInvaders.Core still receive the files beside the executable. -->
<CodeBrixSkyTNTCopyAssetsToOutput>false</CodeBrixSkyTNTCopyAssetsToOutput>
<CodeBrixMuPTCopyAssetsToOutput>false</CodeBrixMuPTCopyAssetsToOutput>
<CodeBrixFluidR3GmCopyAssetsToOutput>false</CodeBrixFluidR3GmCopyAssetsToOutput>
```

At start-up the host pins the output format first, then the director registers
everything and makes the one call:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
//Pin the output format BEFORE anything plays: effects preload at this rate and the generated music renders at it
AudioSystem.Initialize(MusicSetup.RecommendedSampleRate, MusicSetup.RecommendedChannels);
```

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Music/Setup/MusicSetup.cs
GeneralMidiInstrumentLibrary.Register();
FluidR3GmInstrumentLibrary.Register();
if (!string.Equals(InstrumentLibraryRegistry.DefaultName, MusicChoices.ModestSynthGm, StringComparison.OrdinalIgnoreCase))
{
    //Something registered a library before this ran: the game's default is still ModestSynthGm
    InstrumentLibraryRegistry.SetDefault(MusicChoices.ModestSynthGm);
}

SkyTNTModel.Register();
MuPTModel.Register();
```

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Music/Setup/MusicSetup.cs
var entry = EntryFor(sector, boss);
return new GeneratedMusicOptions
{
    Generator = generator,
    InstrumentLibrary = library,
    Preset = entry.PresetFor(generator, boss),
    BeatsPerMinute = entry.BeatsPerMinuteFor(boss),
    SeamCrossfade = SeamCrossfade,
    MasterVolume = (float)settings.MusicVolume,
    TrackKey = TrackKey,
    StartImmediately = true,
};
```

The view model builds the director over the engine's real seams:
`EngineGeneratedMusicStarter` calls `Engine.UseGeneratedMusic` and returns the
provider as an `IGeneratedMusicSession`, and `MusicManager` is the `IMusicManager`
whose `MusicVolume` is the player's music slider:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs
var music = new GeneratedMusicDirector(new EngineGeneratedMusicStarter(), MusicManager.Instance,
    Engine.Instance.EngineDispatcher);
```

From then on the music is moved on, never restarted. A new sector or a boss is a
follow-up on the same session, which takes over at the next bar line:

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
public void OnSector(int sector)
{
    ReleaseDucks();
    Sector = Math.Max(1, sector);
    Boss = false;
    FollowUp(MusicSetup.FollowUpFor(_settings, Sector), $"sector {Sector}");
}

public void OnBoss(int sector)
{
    Sector = Math.Max(1, sector);
    Boss = true;
    _music.PlayStingerOnBus(AssetKeys.Sfx.BossWarning, AudioBus.Sfx);
    _music.Duck(BossDuckDepth, BossAttack, BossHold, BossRelease);
    GameLog.Write($"music: boss of sector {Sector} - ducked to {BossDuckDepth:0.##} under the boss-warning stinger");
    FollowUp(MusicSetup.FollowUpFor(_settings, Sector, boss: true), $"boss of sector {Sector}");
}
```

Only a change of model or instrument library on the settings screen starts a fresh
session, by calling `UseGeneratedMusic` again; the same choice keeps the session:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
public void ApplySettings(MusicSettings settings, int sector, bool boss)
{
    var next = Normalize(settings);
    var sameChoice = string.Equals(next.GeneratorName, _settings.GeneratorName, StringComparison.Ordinal) &&
                     string.Equals(next.InstrumentLibraryName, _settings.InstrumentLibraryName, StringComparison.Ordinal);
    if (sameChoice && _stream != null)
    {
        GameLog.Write($"music: settings unchanged ({next.GeneratorName} through {next.InstrumentLibraryName}) - the session plays on");
        return;
    }

    _settings = next;
    var safeSector = Math.Max(0, sector);
    StartSession(safeSector, boss && safeSector > 0, "settings changed");
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Music/Setup/MusicSetup.cs`,
`Sectors/SectorMusic.cs` and `Choices/MusicChoices.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs`
`BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs` (`CanvasFirstStart`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs`

**Sharp edges.**
- Register an instrument library, always. The add-in registers nothing, and with no
  library there is no sound, only a faulted state and a log line.
- The game names its instrument library in every set of options and resets the
  registry's default to its own choice in case something registered first. The
  CodeBrix.Audio.MusicGeneration blueprints (BLUEPRINTS-GeneratingMusic.md in that
  library) cover why the first library registered becomes the default.
- With no model registered, or its files missing, the music package plays its embedded
  replay - the same piece every time. Log what is registered and whether each model's
  files were found, so a silent fallback is visible.
- The game uses a level's tempo only where a session starts - at start-up or after a
  change of model or instruments - and moves the music on with follow-ups
  everywhere else. The CodeBrix.Audio.MusicGeneration blueprints
  (BLUEPRINTS-GeneratingMusic.md in that library) cover how a session holds its
  pulse across pieces.
- A follow-up is not instant: the new music takes over at a bar line once it is ready.
  Ask for it early - at the sector briefing, at the boss warning - not at the moment
  it should be heard.
- Apply the player's music level once. This game drives the engine's music bus and
  leaves the session's own level at full; doing both multiplies them.
- The provider's state-changed event can arrive on the audio thread. Post to the
  engine dispatcher before touching game state or logging from it.
- Nothing in the game waits for the music, and no music state stops the game. The
  CodeBrix.Audio.MusicGeneration blueprints (BLUEPRINTS-GeneratingMusic.md in that
  library) cover why silence while a model loads, and short waits on a slow
  machine, are ordinary states.

### Read keyboard and gamepad together through an InputActionMap and switch on-screen prompts

**When you want this.** You want the engine's `InputActionMap` to read the keyboard
and every gamepad as one set of named actions: the player should be able to pick up a
controller in the middle of a game, or put it down and use the keyboard, with no
setting to change - and the prompts on screen should show the buttons of whatever they
are holding.

**The MVVM shape.** Not a view-model concern. The engine's input-action map
(`InputActionMap`) reads the keyboard and every connected gamepad against a binding
profile of named actions. The game keeps only its action names, its bindings and its
numbers in one static class; the host attaches the map to the engine and reads it once
per fixed step into the plain values the rules take - held input for play,
presses for menus. The map remembers which device was used last; the
screens draw their prompts from it, and the settings facade stores it for the next run.

**Code.**

The game names its actions and binds each one to keys, gamepad buttons, D-pad and
stick directions at once. A second control scheme is a copy of the first with a few
actions rebound:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs
public static readonly InputBindingProfile Classic = new InputBindingProfile(nameof(GamepadProfile.Classic))
    .Bind(MoveLeft, Key(VirtualKey.Left), Key(VirtualKey.A), InputBinding.DPad(StickDirection.Left))
    .Bind(MoveRight, Key(VirtualKey.Right), Key(VirtualKey.D), InputBinding.DPad(StickDirection.Right))
    .Bind(Fire, Key(VirtualKey.Space), Button(SdlGamepadButtons.A))
    .Bind(Bomb, Key(VirtualKey.LeftShift), Key(VirtualKey.Shift), Button(SdlGamepadButtons.B))
    .Bind(MenuUp, Key(VirtualKey.Up), InputBinding.DPad(StickDirection.Up), Stick(StickDirection.Up))
    // ...
    .Bind(Confirm, Key(VirtualKey.Enter), Button(SdlGamepadButtons.A))
    .Bind(Back, Key(VirtualKey.Escape), Button(SdlGamepadButtons.B))
    .Bind(Pause, Key(VirtualKey.Escape))
    // ...

/// <summary>The right shoulder fires and the left shoulder drops a bomb; A and B stay confirm and back.</summary>
public static readonly InputBindingProfile Shoulder = Classic.Copy(nameof(GamepadProfile.Shoulder))
    .Rebind(Fire, Key(VirtualKey.Space), Button(SdlGamepadButtons.RightShoulder))
    .Rebind(Bomb, Key(VirtualKey.LeftShift), Key(VirtualKey.Shift), Button(SdlGamepadButtons.LeftShoulder));
```

The host attaches the map once. From then on the engine polls it on every cycle, so a
press shorter than a fixed step is kept for the next step, and the map claims the
active profile's keys on the engine's keyboard adapter:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
_input.LastDevice = _savedDevice;
_input.Profile = GameControls.ProfileFor(_settings.GamepadProfile);

//Polled every engine cycle: a tap shorter than a step is latched for the next step, and the active
//  profile's keys are claimed (no app accelerator or focus move sees them while the canvas has focus)
_input.Attach(Engine);
```

Each fixed step ends the map's step first, then reads it. Switching the gamepad
profile is one assignment:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
_input.Update(step.DeltaSeconds);
Step();
// ...
_input.Profile = GameControls.ProfileFor(_settings.GamepadProfile);
GameControls.SetMenuRepeat(_input, screen == GameScreen.HighScoreEntry ? GameControls.NameEntryRepeatInterval : GameControls.RepeatInterval);
var menu = GameControls.ReadMenu(_input, clicked);
var play = GameControls.ReadPlay(_input);
```

The reads turn named actions into the rules' own input types; the left stick is added
to the digital movement with the dead zone applied:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs
public static GameInput ReadPlay(InputActionMap map) =>
    new GameInput(map.GetAxis(MoveLeft, MoveRight, GamepadStick.Left), map.IsHeld(Fire), map.IsHeld(Bomb));
// ...
public static MenuInput ReadMenu(InputActionMap map, bool clicked = false) => new MenuInput(
    map.IsTriggered(MenuUp), map.IsTriggered(MenuDown), map.IsTriggered(MenuLeft), map.IsTriggered(MenuRight),
    map.WasPressed(Confirm), map.WasPressed(Back), map.WasPressed(Pause), map.WasPressed(Start),
    map.WasPressed(KenneyLink), map.AnyPressed || clicked);
```

And a prompt is a function of the device and the profile:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/Prompts.cs
public static string Fire(InputDeviceKind device, GamepadProfile profile) =>
    device != InputDeviceKind.Gamepad ? "[SPACE]" : profile == GamepadProfile.Shoulder ? "[RB]" : "[A]";
```

The map also takes the keyboard and the gamepads as functions, so a test drives the
game's real bindings through one fake that is both devices:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/FakeDevices.cs
public InputActionMap CreateMap(GamepadProfile profile = GamepadProfile.Classic) =>
    GameControls.Configure(new InputActionMap(GameControls.ProfileFor(profile), () => this, () => new IGamepadAdapter[] { this }));
```

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/GameControlsTests.cs
[Fact]
public void the_shoulder_profile_leaves_A_and_B_to_confirm_and_back()
{
    //Arrange
    var (devices, map) = Started(GamepadProfile.Shoulder);

    //Act
    devices.Set(buttons: new[] { SdlGamepadButtons.A, SdlGamepadButtons.B });
    map.Update(Frame);
    var menu = GameControls.ReadMenu(map);

    //Assert
    GameControls.ReadPlay(map).HasAnyInput.Should().BeFalse();
    menu.Confirm.Should().BeTrue();
    menu.Back.Should().BeTrue();
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs` and `Screens/Prompts.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`OnConfigureGamepads`, `OnEngineInitialized`, `OnFixedUpdate`, `Step`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/GameControlsTests.cs` and
`Support/FakeDevices.cs`

**Sharp edges.**
- The engine cycles far more often than the rules step, and a key can go down and up
  between two steps. Attach the map, so the engine polls it every cycle, and call
  `Update` first in each step; the step after a short tap sees it pressed and held.
- Let the map claim the keys (it does by default). An unclaimed key the game only
  polls also reaches the application: Space or Enter can fire a focused control's
  action, and the arrows can move focus off the canvas.
- Whatever is already held when the map starts is not a press, and a stick already
  pushed must come back before it counts. Otherwise the key that launched the game
  selects the first menu item.
- Decide once what a shared key means. Here Escape is bound to both Back and Pause,
  and each screen reads the one it cares about.
- Do not bind the stick's directions to the movement actions as well as passing the
  stick to `GetAxis`, or a light push reads as a full one. Here the stick directions
  drive only the menu actions.
- Gamepad support can be absent - no native library, no permission, no controller.
  Log why once and carry on with the keyboard; hot-plugging needs nothing more,
  because the map reads whichever controllers are connected now.
- Store the last device used, so the next run's first screen already shows the right
  prompts. Keep the stored spelling stable when the device type changes.

### Show a splash card and hand over to a title screen

**When you want this.** The game should open on a title card, fade it out, and land on
its title screen - skippable by the player, and without shipping a splash image the
art pack does not already contain.

**The MVVM shape.** Not a view-model concern. The picture is composed in the game
library from the assets it has just loaded; the engine's splash overlay shows it; and
the screen state machine in the rules library decides when the title takes over, so
the splash, the skip and the time-out are one rule in one place.

**Code.**

The host composes the picture, hands it to the engine's splash overlay, and forwards
the overlay's completion to the game session; when no overlay can be shown it goes
straight on:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
private void ShowSplash()
{
    var host = RenderSurface.Host;
    if (host.ViewManager.Views.Count > 0)
    {
        using var stream = SplashComposer.Compose(_images, _frame.Font, _frame.ThinFont);
        _splash = SplashOverlay.TryCreate(stream, host, host.ViewManager.Views[0], 0.6f, 2.6f, 0.6f,
            onSplashCompleted: OnSplashCompleted, nickname: "brixinvaders-splash");
    }

    if (_splash == null)
    {
        GameLog.Write("splash: could not be shown; going straight to the title");
        Engine.EngineDispatcher.Post(OnSplashCompleted);
    }
    // ...
}

private void OnSplashCompleted()
{
    _splash = null;
    _session?.NotifySplashComplete();
}
```

The picture is drawn with Skia from a planet, the enemy ships and the pack's font,
all already loaded from the zips:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/SplashComposer.cs
public static Stream Compose(DrawImageLibrary images, SKTypeface typeface, SKTypeface thinTypeface)
{
    ArgumentNullException.ThrowIfNull(images);
    using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
    using (var canvas = new SKCanvas(bitmap))
    {
        DrawSky(canvas);
        DrawPlanet(canvas, images);
        DrawFleet(canvas, images);
        DrawTitle(canvas, typeface, thinTypeface);
        canvas.Flush();
    }

    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    return new MemoryStream(data.ToArray());
}
```

The screen state machine leaves the splash on the overlay's completion, on a skip, or
after a time-out, whichever comes first:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.GameLogic/Screens/ScreenStateMachine.cs
case GameScreen.Splash:
    if (confirm || input.Back || ScreenTime >= SplashMaxSeconds)
    {
        Go(GameScreen.Title);
    }

    break;
```

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.GameLogic/Screens/ScreenStateMachine.cs
public void NotifySplashComplete()
{
    if (CurrentScreen == GameScreen.Splash)
    {
        Go(GameScreen.Title);
    }
}
```

And when the player skips, the host takes the overlay down itself:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
if (_splash != null && _session.CurrentScreen != GameScreen.Splash)
{
    //The player skipped the splash (or it timed out in the machine): take the overlay down
    _splash.Dispose();
    _splash = null;
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`OnInitialized`, `ShowSplash`, `OnSplashCompleted`, `Step`)
`BrixInvaders/src/libs/BrixInvaders.Game/Rendering/SplashComposer.cs`
`BrixInvaders/src/libs/BrixInvaders.GameLogic/Screens/ScreenStateMachine.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Session/ScreenFlowTests.cs`

**Sharp edges.**
- Show the splash from `OnInitialized`, after the engine has started and the assets
  are loaded; the overlay needs a view to draw on, and the composer needs the art.
- `SplashOverlay.TryCreate` can return null. Always have a way to the title that does
  not depend on the overlay's callback, or a failure to show a picture leaves the game
  stuck on a black screen.
- Keep the decision in the screen state machine. The completion, the skip and the
  time-out all end in the same transition, and a completion that arrives after a skip
  does nothing.
- A skipped overlay is still running. Dispose it when the screen moves on, or it
  fades over the title.
- Composing from the loaded packs keeps the art pack the only art the game ships.

### Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate

**When you want this.** The rules of the game are the part most worth testing and the
part hardest to test through a running engine, which only has to advance them from its
fixed-step hook (`OnFixedUpdate`). You want to play a whole game in a unit test, in
milliseconds, and get the same game every time from the same seed and the same inputs.

**The MVVM shape.** The rules live in a library of their own that references nothing
- no engine, no Skia, no file or clock. It is advanced in fixed steps with a plain
input value, exposes its state for drawing, and reports what happened in each step as
a list of events. The game library runs it from the engine's fixed-step hook and turns
the events into sprites, sounds and particles; the view model never touches either.

**Code.**

The rules library's project file is the whole dependency story:

```xml
<!-- From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.GameLogic/BrixInvaders.GameLogic.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

The simulation draws every random number from one seeded generator of its own and
clears its events at the start of each step:

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.GameLogic/Simulation/GameSimulation.cs
public GameSimulation(GameSetup setup)
{
    Setup = setup ?? throw new ArgumentNullException(nameof(setup));
    Settings = DifficultyTable.For(setup.Difficulty);
    _random = new GameRandom(setup.Seed);
    _nextId = NextId;
    Player = new PlayerShip(Settings.Lives, Settings.StartingBombs);
    Scoring = new Scoring(Settings.ScoreMultiplierPercent);
    Sector = setup.StartSector;
    BeginSector();
}

public void Step(double dt, GameInput input)
{
    _events.Clear();
    if (Phase == StagePhase.GameOver || dt <= 0 || double.IsNaN(dt))
    {
        return;
    }
    // ...
}
```

The host asks the engine for a fixed-step hook at the rules' own rate. The engine then
calls the host's `OnFixedUpdate` that many times a second, whatever rate its loop runs
at, caps the catch-up (five steps a cycle by default) so a stall cannot make the game
run away, freezes the steps while the engine is paused, and calls
`OnAfterFixedUpdates` once after a cycle's steps, where the host builds the frame:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
Engine.Configuration.FixedUpdateRate = (int)Math.Round(1 / Playfield.FixedStep);
```

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
protected override void OnFixedUpdate(FixedUpdateStep step)
{
    if (_session == null)
    {
        return;
    }

    _input.Update(step.DeltaSeconds);
    Step();
}

/// <inheritdoc />
protected override void OnAfterFixedUpdates(int stepCount)
{
    if (_session != null)
    {
        BuildFrame();
    }
}
```

And a whole game becomes a test. A fixed seed and a scripted pilot play thousands of
steps, and the score and a hash of the whole state are pinned:

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.GameLogic.Tests/Simulation/GoldenSeedRuns.cs
private static (long Score, ulong Hash, int Sector, int Wave, StagePhase Phase) Play(Difficulty difficulty, int startSector, int seed)
{
    var simulation = new GameSimulation(new GameSetup(difficulty, startSector, seed));
    var pilot = new AttractPilot();
    for (var i = 0; i < Steps; i++)
    {
        var decided = pilot.Decide(simulation);
        var input = new GameInput(decided.MoveAxis, decided.Fire, i % 1200 == 600);
        simulation.Step(Playfield.FixedStep, input);
        if (simulation.Phase == StagePhase.SectorComplete)
        {
            simulation.BeginNextSector();
        }
    }

    return (simulation.Score, simulation.ComputeStateHash(), simulation.Sector, simulation.Wave, simulation.Phase);
}

[Fact]
public void a_golden_run_repeats_exactly()
{
    //Act
    var first = Play(Difficulty.Legend, 1, 42);
    var second = Play(Difficulty.Legend, 1, 42);

    //Assert
    second.Should().Be(first);
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.GameLogic/Simulation/GameSimulation.cs` and
`Core/GameRandom.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`OnFixedUpdate`, `Step`,
`OnAfterFixedUpdates`, `BuildFrame`)
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`Update`)
`BrixInvaders/tests/libs/BrixInvaders.GameLogic.Tests/Simulation/GoldenSeedRuns.cs`
`BrixInvaders/DESIGN.md` (every number the rules use)

**Sharp edges.**
- Use a random generator you own. `System.Random`'s sequence for a seed is allowed to
  change between runtime versions, which would move every pinned game.
- No clock inside the rules. Time arrives as the fixed step, so the same inputs give
  the same game on a fast machine, a slow one and a test runner.
- Set `FixedUpdateRate` in `OnEngineInitialized`. The hook is off (rate zero) by
  default, and `OnFixedUpdate` never runs until the rate is set.
- Timers counted down in floating point can land a hair above zero and take one step
  too many. Snap values within a tiny epsilon of zero to zero, and fence it with a
  test that counts steps.
- A pinned golden value that moves means the rules changed - a tuning value, the order
  of events, the order of random draws. Update the pins in the same change on purpose,
  never to make a test pass.
- Hand the renderer a published frame built after the steps, not the live simulation:
  the render side may draw on another thread, and it should never see a half-updated
  game.
- Keep the events as data. The rules say an enemy exploded; the game library decides
  what that looks and sounds like, and the rules tests never load a sprite.

### Build each frame as engine DrawList commands and paint only the published copy

**When you want this.** Your game advances its rules on the engine thread, but the
engine may paint on another thread, and you want the painting code to see one
whole, finished frame - never a half-updated game - without locks, which is what the
engine's `DrawList` and `DrawListDrawing` give you. The recipe
[Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate](BLUEPRINTS-GameEngine.md#keep-a-deterministic-game-simulation-apart-from-the-engine-and-step-it-from-onfixedupdate)
states the rule in one sharp edge; this recipe is how BrixInvaders keeps it with the
engine's draw lists: the screens add commands to a `DrawList` after the fixed
steps, `Publish()` copies them into an immutable snapshot, and an engine
`DrawListDrawing` only ever paints a published snapshot.

**The MVVM shape.** Not a view-model concern. The game library owns one small
record that pairs two engine draw lists - the world, drawn under the engine's
particles, and the overlay, drawn over everything - with the two faces of the game
font. The game host fills them in the engine's `OnAfterFixedUpdates` hook, after
each cycle that ran a step, and publishes them; two engine `DrawListDrawing`s - one
on the world layer, one over the view - paint the published copies. The view model
and the page never see a draw command.

**Code.**

The record holds the two lists and the fonts, and clears or publishes both at once:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/FrameLists.cs
public sealed record FrameLists(DrawList World, DrawList Overlay, SKTypeface Font, SKTypeface ThinFont)
{
    /// <summary>Starts a new frame on both lists.</summary>
    public void Clear()
    {
        World.Clear();
        Overlay.Clear();
    }

    /// <summary>Hands both finished lists to their drawings.</summary>
    public void Publish()
    {
        World.Publish();
        Overlay.Publish();
    }
}
```

Both lists share one `DrawImageLibrary`, and the fonts come from the engine's
`FontManager` once the Kenney fonts are loaded:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
_frame = new FrameLists(new DrawList(_images, 1024), new DrawList(_images), FontManager.Instance.Get(fonts.TextKey),
    FontManager.Instance.Get(fonts.ThinKey));
```

The library finds pictures by asset key and frame name. BrixInvaders resolves every
atlas frame once at load and keeps it under the game's own `"atlas#frame"` catalog
key, so a painter names a picture with one string and a spelling mistake shows up in
the log before the first frame:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
foreach (var key in SpriteCatalog.AllAtlasFrames)
{
    if (SpriteCatalog.TrySplit(key, out var atlas, out var frame) && _images.Get(atlas, frame) is { } image)
    {
        _images.AddImage(key, null, image);
        resolved++;
    }
}
```

A painter adds rectangles, pictures and text back to front, in playfield pixels:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/Ui.cs
public static void MenuRow(FrameLists frame, string text, double x, double y, bool selected, double time, double size = 28)
{
    ArgumentNullException.ThrowIfNull(frame);
    if (selected)
    {
        frame.Overlay.Rectangle(x, y, 420, size + 20, 0x402E7DD6, 0xFF5CE1FF, 1.5, 8);
        var bob = Math.Sin(time * 8) * 4;
        frame.Overlay.Image(SpriteCatalog.Cursor, null, x - 190 + bob, y, 26, 26);
    }

    frame.Overlay.Text(text, x, y, frame.Font, size, selected ? Palette.Text : Palette.Dim);
}
```

The host creates one drawing per list and publishes after the screens have painted:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
_ = new DrawListDrawing(host, _worldLayer, bounds, _frame.World, "brixinvaders-world") { ZOrder = 0 };
// ...
_ = new DrawListDrawing(host, view, bounds, _frame.Overlay, "brixinvaders-overlay") { ZOrder = 100 };
// ...
private void BuildFrame()
{
    _frame.Clear();
    _context.Device = _input.LastDevice;
    _context.Profile = _settings.GamepadProfile;
    _director.Paint(_context);
    _frame.Publish();
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Rendering/FrameLists.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`LoadAssets`, `LoadTilesheets`, `CreateDirectDrawings`, `OnAfterFixedUpdates`, `BuildFrame`)
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/Ui.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/TestFrame.cs`

**Sharp edges.**
- Build on one thread only. Everything on a `DrawList` except `Published` belongs to
  the engine thread; the drawing reads nothing but published snapshots, which is
  what makes the GPU tier's UI-thread painting safe without locks.
- `Publish()` copies the commands. The builder keeps its contents, so `Clear()` at
  the start of the next frame, never after publishing.
- A command holds the `SKImage` and `SKTypeface` themselves, resolved when it is
  added. Keep the tilesheets and fonts alive for as long as any published list may
  use them.
- Two lists are published one after the other, so on the GPU tier the overlay can
  be one frame newer than the world for a single paint. Nothing in this game shows
  it; a game that needs the two to match exactly can publish one object holding both
  snapshots and use the drawing's source-function constructor.
- Build a frame only after a cycle that ran at least one fixed step - the engine's
  `OnAfterFixedUpdates` runs exactly then. The drawing marks itself dirty only when
  a new snapshot arrives, so the CPU tier repaints while the game keeps publishing.
- A picture key the library cannot find draws nothing and is logged once, never
  thrown mid-frame; `DrawImageLibrary.Missing` lists them.
- Building commands needs live Skia objects (a typeface for text, a picture for an
  image), so a test project that runs painters needs the native SkiaSharp library.

### Pin a fixed playfield size and let the engine letterbox drawings and clicks

**When you want this.** Your game is designed for one playfield size, the window can
be any size, and you want letterbox bars rather than a stretched picture - plus mouse
clicks on links drawn inside the playfield that land on the right thing whatever the
window size.

**The MVVM shape.** The view model calls one static method on the game host before
it creates it, which chooses the render tier and pins the render resolution to the
playfield. From then on the engine does the fitting: it presents the finished frame
letterboxed into the window and hands mouse positions over already in render
pixels, so the game's drawings, particle bursts and click handler all work in
playfield pixels with no arithmetic of their own. A screen that draws a link adds a
hit region to the overlay draw list it builds; the host hit-tests clicks against the
list's published snapshot and hands the link to the session.

**Code.**

The pin comes first, before anything reads the canvas's host; the mouse is watched
for clicks only, and a click is handed over as it arrives and taken at the next
fixed step:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
public static void PrepareCanvas(GameSurfaceCanvas canvas)
{
    ArgumentNullException.ThrowIfNull(canvas);
    canvas.UseGpuRendering = UseGpuTier();
    canvas.SetRenderResolution(RenderWidth, RenderHeight);
    // ...
}
// ...
mouse.MouseEvent += OnMouseEvent;
mouse.StartMonitoringMouse(trackMouseMovement: false);
// ...
private void OnMouseEvent(MouseEventArgs args)
{
    if (!args.IsButtonJustPressed(MouseButton.Left))
    {
        return;
    }

    //The engine hands the mouse over already in render-resolution pixels, which are playfield pixels
    _clickX = args.CurrentPosition.X;
    _clickY = args.CurrentPosition.Y;
    Interlocked.Exchange(ref _clickPending, 1);
}
// ...
var clicked = Interlocked.Exchange(ref _clickPending, 0) == 1;
if (clicked)
{
    _input.NoteDeviceUsed(InputDeviceKind.KeyboardMouse);
    if (_frame.Overlay.Published.HitTest(_clickX, _clickY) is { } link)
    {
        _session.OpenLink(link.Id);
    }
}
```

A screen that draws a link adds a hit region in playfield pixels to the same list,
with the link as its id; the published snapshot's `HitTest` answers which region is
under a point, the topmost first:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/CreditsScreen.cs
case CreditsLineStyle.Link:
    frame.Overlay.Text(line.Text, Ui.CenterX, y, frame.Font, 17, Palette.Gold);
    frame.Overlay.HitRegion(Ui.CenterX, y, 820, 26, line.Url);
    y += 28;
    break;
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`PrepareCanvas`, `CreateDirectDrawings`, `OnMouseAdapterInitialized`, `OnMouseEvent`, `Step`)
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/CreditsScreen.cs` and `KenneyCard.cs`
`BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs` (`CanvasFirstStart`)

**Sharp edges.**
- Pin the render resolution in the same place, and at the same moment, as the render
  tier: before the first access to the canvas's host.
- With the resolution pinned, the host's logical size is the playfield size, the
  drawings' destination rectangles are the playfield, and mouse positions reach the
  poller already mapped across the letterbox. Do not fit the playfield again in game
  code; a second fit is the identity until something changes, and then it is a
  second, disagreeing answer.
- A click over a letterbox bar keeps its outside coordinates (negative, or past the
  playfield) rather than being clamped, so it simply hits no hit region.
- Hit-test against the snapshot that was published - the one on screen - not the
  list being built. A hit region is only clickable on the frames that drew it.
- A click arrives from the mouse event, outside the fixed step. Store it and set an
  interlocked flag there, and let the step take it; the step also notes it on the
  input-action map as keyboard-and-mouse input (`NoteDeviceUsed`), so the prompts follow.

### Paint each game screen with its own painter and test the screens as draw lists

**When you want this.** A game has a dozen screens - title, menus, briefing, play,
pause, game over, name entry, credits - and you want each one's drawing in a small
class of its own, opened and closed as the game moves between them, and tested
without a window or a GPU. [Show a splash card and hand over to a title
screen](BLUEPRINTS-GameEngine.md#show-a-splash-card-and-hand-over-to-a-title-screen)
and [Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate](BLUEPRINTS-GameEngine.md#keep-a-deterministic-game-simulation-apart-from-the-engine-and-step-it-from-onfixedupdate)
show the screen state machine deciding which screen is on show; this recipe is the
drawing side that follows it.

**The MVVM shape.** Not a view-model concern. The rules library's screen state
machine decides which screen is on show and raises a change event. In the game
library a director holds one painter per screen and keeps exactly one open; the
host tells the director to follow each change, advances it every fixed step, and
asks it to paint into the frame's draw lists. Everything a painter needs arrives in one
context object, and shared furniture - panels, headings, menu rows, the prompt
footer - is a static helper class.

**Code.**

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/ScreenDirector.cs
public void Follow(GameScreen screen, PaintContext context)
{
    if (_open != null && screen == Current)
    {
        return;
    }

    _open?.Close();
    Current = screen;
    _open = _screens[screen];
    _open.Open(context);
}

/// <summary>Advances the open painter.</summary>
/// <param name="dt">Seconds.</param>
public void Update(double dt) => _open?.Update(dt);

/// <summary>Draws the open painter.</summary>
/// <param name="context">The context.</param>
public void Paint(PaintContext context) => _open?.Paint(context);
```

A painter builds what it needs when it opens, keeps its own clock, and draws every
frame:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/ScreenPainter.cs
public abstract class ScreenPainter
{
    /// <summary>Seconds since the screen opened.</summary>
    protected double OpenTime { get; private set; }

    /// <summary>The screen opens.</summary>
    /// <param name="context">The context.</param>
    public void Open(PaintContext context)
    {
        OpenTime = 0;
        OnOpen(context);
    }
    // ...
    /// <summary>Draws one frame.</summary>
    /// <param name="context">The context.</param>
    public abstract void Paint(PaintContext context);
```

The host wires the director to the state machine's change event:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
private void OnScreenChanged(GameScreen from, GameScreen to)
{
    _director.Follow(to, _context);
    if (to is GameScreen.Playing && from == GameScreen.SectorBriefing)
    {
        _playfield.ClearFlashes();
    }
}
```

Because a painted screen is a list of commands, a test drives a real session to a
screen, paints it into draw lists, and reads the commands. The test's lists sit on
a picture library in which every catalog key is a tiny picture of its own, so an
image command reads back as the key it drew:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Screens/ScreenDirectorTests.cs
private static TestFrame Paint(SessionDriver driver, InputDeviceKind device = InputDeviceKind.KeyboardMouse)
{
    var (director, context, frame) = Create(driver);
    context.Device = device;
    director.Follow(driver.Session.CurrentScreen, context);
    director.Update(0.5);
    frame.Lists.Clear();
    director.Paint(context);
    return frame;
}
// ...
[Fact]
public void every_drawn_image_is_a_picture_the_game_loads()
{
    //Arrange
    var driver = new SessionDriver();
    driver.ToPlaying();
    driver.Wait(4.0, new GameInput(0.3, true, false));

    //Act
    var frame = Paint(driver);

    //Assert
    frame.World.Should().Contain(command => command.Kind == DrawCommandKind.Image);
    frame.Images.Missing.Should().BeEmpty("every key a painter draws is one the host loads");
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/ScreenDirector.cs`,
`ScreenPainter.cs`, `PaintContext.cs`, `Ui.cs` and one painter per screen
(for example `PausedScreen.cs`, `CreditsScreen.cs`)
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`OnEngineInitialized`, `OnScreenChanged`, `Step`, `BuildFrame`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Screens/ScreenDirectorTests.cs` and
`Support/TestFrame.cs`

**Sharp edges.**
- Painters decide nothing. Which screen is on show, and when it changes, belongs to
  the state machine; a painter only reads the session and draws.
- Map every screen, and prove it. The director indexes its table by the screen enum,
  so a new screen with no painter throws on the first visit - a test that calls
  `CoversEveryScreen()` catches it first.
- Read content that can change when the screen opens, not every frame. The credits
  painter asks its content seam for its lines in `OnOpen`, once per visit.
- Advance the director on the fixed step, not in the paint call, so a painter's
  animation clock runs at game time and stops when the game is not stepping.
- Test for what matters on a screen - its heading, a prompt, a hit region's link, that
  every image key is one the game loads - rather than for pixels or positions.
- An engine draw command holds a real typeface and picture, so the test project
  needs the native SkiaSharp library even though nothing is ever rendered.

### Repeat a held menu direction with InputRepeat and ignore a stick springing back

**When you want this.** Holding a direction on a menu or on a name-entry letter
should act once, pause, then repeat at a steady rate - on the arrow keys, the D-pad
and the stick alike, timed by an `InputRepeat` on the engine's `InputActionMap` - and
letting go of a stick should never count as a push the other way. [Read keyboard and gamepad together through an InputActionMap and switch on-screen prompts](BLUEPRINTS-GameEngine.md#read-keyboard-and-gamepad-together-through-an-inputactionmap-and-switch-on-screen-prompts)
covers the engine's input-action map, its bindings and its latched presses; this
recipe adds its hold-to-repeat timing and its stick settle time.

**The MVVM shape.** Not a view-model concern. The engine's `InputActionMap` owns the
repeat clocks and the stick-as-direction reading, both driven by the step length
passed to `Update` and never by a wall clock. The game only supplies its numbers and
picks the repeat interval for the screen on show; the rules library only ever sees
menu presses, a repeat arriving as one more press.

**Code.**

Each menu direction is bound to the arrow key, the D-pad and the stick pushed that
way, so one action - and one repeat clock - covers all three:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs
.Bind(MenuUp, Key(VirtualKey.Up), InputBinding.DPad(StickDirection.Up), Stick(StickDirection.Up))
.Bind(MenuDown, Key(VirtualKey.Down), InputBinding.DPad(StickDirection.Down), Stick(StickDirection.Down))
```

The game hands the map its stick thresholds, settle time and repeat timing:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs
public static InputActionMap Configure(InputActionMap map)
{
    ArgumentNullException.ThrowIfNull(map);
    map.StickDeadZone = DeadZone;
    map.StickPressThreshold = StickMenuThreshold;
    map.StickReleaseThreshold = StickMenuRelease;
    map.StickSettleSeconds = StickSettleSeconds;
    SetMenuRepeat(map, RepeatInterval);
    return map;
}
// ...
public static void SetMenuRepeat(InputActionMap map, double interval)
{
    foreach (var direction in MenuDirections)
    {
        map.SetRepeat(direction, new InputRepeat(RepeatDelay, interval));
    }
}
```

A menu direction is read with `IsTriggered`, which is true on the press and on each
repeat; the host slows the repeat on the name entry so the letters stay readable:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
GameControls.SetMenuRepeat(_input, screen == GameScreen.HighScoreEntry ? GameControls.NameEntryRepeatInterval : GameControls.RepeatInterval);
```

The name entry is tested as scenarios through the game's real bindings - a flick of
the stick with the overshoot of its spring-back moves exactly one letter:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/NameEntryInputTests.cs
[Fact]
public void a_stick_flicked_up_and_released_moves_up_even_when_it_springs_back_past_centre()
{
    //Arrange
    var (map, entry) = Started("JAA");

    //Act - push up for a few frames, let go: the stick snaps back and overshoots downward for ~10 ms
    Frames(map, entry, _devices.Set(stickY: 1.0), 4);
    Frames(map, entry, _devices.Set(stickY: 0.1), 1, 0.003);
    Frames(map, entry, _devices.Set(stickY: -0.8), 1, 0.003);
    Frames(map, entry, _devices.Set(stickY: -0.6), 1, 0.003);
    Frames(map, entry, _devices.Set(stickY: 0.2), 1, 0.003);
    Frames(map, entry, _devices.Set(stickX: 0.08, stickY: -0.03), 10);

    //Assert
    entry.Name.Should().Be("KAA");
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Input/GameControls.cs` (`Configure`, `SetMenuRepeat`, `ReadMenu`)
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`Step`)
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/HighScoreEntryScreen.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/NameEntryInputTests.cs`

**Sharp edges.**
- A released stick springs back past center and briefly reads well beyond the
  threshold the other way. Hysteresis alone does not stop that; the map's settle time
  after each release, during which the axis cannot turn on either way, does.
- A stick used as a menu direction needs hysteresis too: on past one threshold, off
  only below a lower one, or a stick resting near the edge repeats.
- Read menu directions with `IsTriggered`, not `WasPressed`: only `IsTriggered`
  carries the repeats. Actions without repeat timing stay single edges.
- Bind a direction's keys, D-pad and stick to one action. Holding the same direction
  on the keyboard and the D-pad is then still one hold and acts once.
- Pass the real step length to `Update`. The repeat clock counts it, so a stalled
  step gives one late repeat, never a burst.
- Write the name-entry tests as scenarios through the game's real bindings - a tap, a
  flick with overshoot, a long hold - because the engine's own tests do not prove
  that your bindings move a letter by exactly one.

### Show power-ups on the player's ship by layering sprite parts over the hull

**When you want this.** A power-up should be visible on the ship the player is
flying - wing pods, engine nacelles with flames, a glowing emitter - built from the
art pack's loose parts rather than from a new sprite for every combination of hull
and upgrade.

**The MVVM shape.** Not a view-model concern. A static table in the game library
says which parts each hull shape carries for each power-up, where, and in which
layer; the playfield painter asks it for the parts of the active power-ups and adds
them to the world list under and over the hull. The rules library only says which
power-ups are active and for how long.

**Code.**

A part is a record in the hull's own art pixels, so it stays on the same spot at any
draw size:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/ShipPart.cs
public sealed record ShipPart(string Name, PowerUpKind PowerUp, IReadOnlyList<string> Frames, double X, double Y,
    double Width, double Height, double Rotation, ShipPartLayer Layer, int Order, ShipPartEffect Effect)
```

The painter asks for the parts of the active power-ups; the table behind that is
built once per hull shape and sorted into draw order:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/ShipLoadout.cs
public static IReadOnlyList<ShipPart> PartsFor(int shape, IEnumerable<PowerUpKind> active)
{
    ArgumentNullException.ThrowIfNull(active);
    var kinds = new HashSet<PowerUpKind>(active);
    return AllParts(shape).Where(part => kinds.Contains(part.PowerUp)).ToArray();
}
// ...
var parts = new List<ShipPart>
{
    // Speed boost: an engine nacelle under each wing with its own blue flame.
    new("boost flame left", PowerUpKind.SpeedBoost, flames, -engineX, engineY + 22, 14, 31, 0, under, 0, ShipPartEffect.Flicker),
    new("boost flame right", PowerUpKind.SpeedBoost, flames, engineX, engineY + 22, 14, 31, 0, under, 0, ShipPartEffect.Flicker),
    // ...
};

return parts.OrderBy(part => part.Layer).ThenBy(part => part.Order).ToArray();
```

The painter draws the under-hull parts, the hull and its damage overlay, then the
over-hull parts, scaling every offset by the hull's scale and blinking a part whose
power-up is about to run out:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/PlayfieldPainter.cs
var parts = ShipLoadout.PartsFor(shape, ShipLoadout.ActiveKinds(game.PowerUps));
NoteLoadout(shape, parts);
PaintParts(frame, game, parts, ShipPartLayer.UnderHull, shape, alpha, time);
// ...
frame.World.Image(SpriteCatalog.PlayerShip(shape, colour), null, player.X, player.Y, PlayerDrawWidth, PlayerDrawHeight,
    0, alpha);
// ...
PaintParts(frame, game, parts, ShipPartLayer.OverHull, shape, alpha, time);
// ...
var partAlpha = alpha;
if (blinkOff && game.PowerUps.TimeLeft(part.PowerUp) < ShipLoadout.ExpiryBlinkSeconds)
{
    partAlpha *= 0.35;
}
// ...
frame.World.Image(image, null, player.X + (part.X * scale), player.Y + (part.Y * scale), part.Width * scale,
    part.Height * scale, part.Rotation, partAlpha);
```

The table is data, so its geometry is testable:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Rendering/ShipLoadoutTests.cs
[Fact]
public void every_part_frame_is_an_atlas_frame_the_assets_library_pins()
{
    for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
    {
        //Assert
        ShipLoadout.AllParts(shape).SelectMany(part => part.Frames)
            .Should().OnlyContain(frame => SpriteCatalog.AllAtlasFrames.Contains(frame));
    }
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Rendering/ShipLoadout.cs`, `ShipPart.cs`,
`ShipPartLayer.cs`, `ShipPartEffect.cs` and `PlayfieldPainter.cs`
(`PaintPlayer`'s part calls, `PaintParts`, `NoteLoadout`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Rendering/ShipLoadoutTests.cs`
`BrixInvaders/DESIGN.md` (the visible-upgrades section)

**Sharp edges.**
- Place parts in the hull art's pixels, not in world units, and scale by the factor
  the hull is fitted with. Change the ship's draw size and the parts follow.
- Each hull shape needs its own offsets. The three hulls differ in width and wing
  line, so one table for all of them puts pods in space on one and inside the wing
  on another.
- Sort by layer, then by order, once when the table is built, and draw the two
  layers on either side of the hull and its damage overlay.
- Log the loadout only when it changes. A description compared against the last one
  gives one line per upgrade instead of one per frame.
- Pin the geometry with tests - mirrored pairs, parts on or just beyond the hull,
  every frame key one the assets library loads - so a bad number fails a test rather
  than a play session.

### Stack engine particles on a pixel layer between your own world and overlay drawings

**When you want this.** Most of your game is drawn from your own draw lists, but
you want the engine's particle system for explosions, sitting above the game world
and below the HUD and menus, on a pixel layer (`Scene.AddPixelLayer`) because the game
has no tile map.

**The MVVM shape.** Not a view-model concern. The host creates the drawings in its
`CreateDirectDrawings` override: the world drawing and the particle surface on the
scene layer, and the overlay drawing on the view. A small class in the game library
owns the particle surface and turns the rules' events into bursts; the host drives
it from its fixed step.

**Code.**

The game has no tile map, so its scene is one engine pixel layer - a layer with no
tile grid, sized in pixels to the playfield - that carries the world drawing and the
particles:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
protected override Scene CreateInitialScene()
{
    var scene = new Scene();
    _worldLayer = scene.AddPixelLayer(RenderWidth, RenderHeight);
    return scene;
}
```

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
protected override void CreateDirectDrawings()
{
    var host = RenderSurface.Host;
    var bounds = new Rectangle(0, 0, Math.Max(1, host.LogicalWidth), Math.Max(1, host.LogicalHeight));
    var view = host.ViewManager.Views[0];

    _ = new DrawListDrawing(host, _worldLayer, bounds, _frame.World, "brixinvaders-world") { ZOrder = 0 };

    var surface = new ParticleSurface(host, _worldLayer, bounds, "brixinvaders-particles", maxParticles: 3000)
    {
        GravityX = 0f,
        GravityY = 0f,
        ZOrder = 50,
    };
    _particles = new ParticleEffects(surface);

    _ = new DrawListDrawing(host, view, bounds, _frame.Overlay, "brixinvaders-overlay") { ZOrder = 100 };
}
```

Each step, the rules' events become bursts at the events' own positions, because
with the render resolution pinned to the playfield a playfield pixel is a layer
pixel:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Rendering/ParticleEffects.cs
foreach (var gameEvent in events)
{
    var x = (float)gameEvent.X;
    var y = (float)gameEvent.Y;
    switch (gameEvent.Kind)
    {
        case GameEventKind.EnemyDestroyed:
            Burst(x, y, 26, 220, (2, 5), (0.25f, 0.6f), ExplosionColour(gameEvent.Colour));
            Burst(x, y, 10, 90, (4, 8), (0.2f, 0.4f), 0xFFFFE9A8);
            break;
        // ...
    }
}
```

The host drives the bursts after the session's step and clears the events it has
used:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
if (_session.Events.Count > 0)
{
    _particles.Emit(_session.Events);
    _playfield.AddFlashes(_session.Events);
    _session.ClearEvents();
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`CreateInitialScene`, `CreateDirectDrawings`, `Step`)
`BrixInvaders/src/libs/BrixInvaders.Game/Rendering/ParticleEffects.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hud/HudPainter.cs` (the boss health bar,
drawn in the overlay list)

**Sharp edges.**
- Put the world drawing and the particle surface on the scene layer and the overlay
  drawing on the view; the z-order values then place the particles above the world
  and the HUD above both.
- A game without a tile map needs no empty tile grid to carry its drawings:
  `Scene.AddPixelLayer(width, height)` gives the scene a layer sized in pixels.
- Particle positions, sizes and speeds are in layer pixels. Pin the render
  resolution, and the game's own coordinates can go straight in; without the pin
  they would have to be scaled to whatever resolution the window gave the surface.
- Not every bar needs an engine object. A boss health bar that sits in the HUD is two
  rectangles and a label in the overlay list, drawn with everything else the HUD draws.

### Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu

**When you want this.** With one call to the engine's `GameWindowLifecycle.Attach`,
minimizing the window should stop the game, its sound and its music at once, and
restoring it should show the game's own pause menu - not throw the player straight
back into play - with the keyboard working again after the window is activated. [Run and pause a game engine session inside a
page](BLUEPRINTS-GameEngine.md#run-and-pause-a-game-engine-session-inside-a-page)
pauses the engine as the user moves between parts of the UI; this recipe pauses it
for the window, and hands the moment over to the game's own pause.

**The MVVM shape.** Not a view-model concern. The App hands its window to the
engine's `GameWindowLifecycle` in one call where the window is created; from then on
the engine pauses and resumes itself with the window and gives the game canvas
keyboard focus whenever the window is activated. The game host overrides one hook to
press the pause action on the engine's input-action map (`SimulatePress`), so the game
reads it on its first step back exactly as it would read the pause key. The page keeps one focus call of its
own, for the canvas's first start.

**Code.**

```csharp
// From CodeBrix.Samples/BrixInvaders/src/BrixInvaders.UI/App.xaml.cs
//Minimizing the window pauses the whole engine (loop and audio) and the game shows its own pause overlay when
//  the window comes back; activating the window hands keyboard focus back to the game canvas.
GameWindowLifecycle.Attach(MainWindow);
```

The engine calls the host's `OnWindowHidden` on the UI thread before it pauses, while
the game is still live:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
/// <summary>
/// The window was minimized (the app attached <c>GameWindowLifecycle</c>): a pause request is latched so the game
/// shows its own pause overlay when the player returns; the engine then pauses itself (loop and audio).
/// </summary>
protected override void OnWindowHidden()
{
    _input.SimulatePress(GameControls.Pause);
    GameLog.Write("window: hidden - engine paused");
}
```

The window was activated before the game existed, so the page gives the canvas focus
once when it first starts:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/BrixInvaders.UI/Views/MainPage.xaml.cs
GameCanvas.FirstStarted += (_, _) =>
{
    _gameCanvasManager?.CanvasFirstStart(GameCanvas);

    //The window was activated before the game existed, so hand the canvas keyboard focus once here (later
    //  activations are the engine's GameWindowLifecycle). Deferred so it lands after start-up finishes.
    DispatcherQueue.TryEnqueue(() => GameCanvas.Focus(FocusState.Programmatic));
};
```

**Where to look.**
`BrixInvaders/src/BrixInvaders.UI/App.xaml.cs` (`OnLaunched`)
`BrixInvaders/src/BrixInvaders.UI/Views/MainPage.xaml.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`OnWindowHidden`)
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/PausedScreen.cs`

**Sharp edges.**
- Two pauses, two jobs. The engine's global pause is for the minimized window; the
  pause menu is a game pause, because the engine's input pollers stop during an
  engine pause and the menu still has to be driven. What the music does under the
  pause menu is
  [Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title](BLUEPRINTS-GameEngine.md#duck-the-music-for-a-pause-menu-and-hold-a-game-over-duck-with-playstingerwithheldduck-until-the-title).
- Latch the pause request in `OnWindowHidden` and leave the engine pause to the
  helper. The hook runs before the loop parks, and the helper resumes only a pause it
  made itself, so a pause the game makes on its own is never lifted by the window.
- The request is a pause, not a back. Only a screen that reads pause reacts; on the
  title or a menu nothing happens, and in play the game opens its pause menu.
- Nothing needs resetting on the way back: the engine's fixed steps are frozen while
  it is paused, so the first step after a restore does not see the minimized time as
  elapsed.
- Keyboard focus comes back on every activation, which is what keeps the keyboard
  working after an alt-tab; without it the gamepad - which needs no focus - still
  works and the keyboard seems dead. The helper only knows game hosts that exist, so
  the canvas's first start still takes focus once.

### Put the music policy behind an interface and test it against fakes of the engine's music interfaces

**When you want this.** Your game's music has rules - start at the title, move on
at each level, go quiet for a pause, start over when the player changes a choice -
and you want every one of them pinned by a unit test that loads no model and opens
no audio device, against fakes of the engine's `IGeneratedMusicStarter`,
`IGeneratedMusicSession` and `IMusicManager`. [Start endless generated music with one call](BLUEPRINTS-GameEngine.md#start-endless-generated-music-with-one-call)
names the split this rests on; this recipe is about the two seams that split
creates and the fakes that drive them.

**The MVVM shape.** Not a view-model concern beyond construction: the view model
builds the real director over the engine's real music seams and hands it to the game
host, which falls back to a silent director when given none. The game session
decides which musical moment the game is in and tells the director only when that
moment changes. The director decides what the music does about it and asks the
engine through the engine's own interfaces. Each seam has its own fake, so the
session's tests never see the music and the director's tests never see the engine.

**Code.**

The first seam is everything the game tells the music. Its contract says who
de-duplicates, which is what lets an implementation stay simple:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/IMusicDirector.cs
/// <summary>
/// SEAM (implemented by the music wiring): everything the game tells the music. The game calls these at the
/// moments below and never touches the music otherwise; <see cref="SilentMusicDirector"/> is the no-op default.
/// </summary>
/// <remarks>
/// <para>
/// Every call arrives on the ENGINE thread. Calls are made only when the musical moment actually changes (going from
/// one menu to another is not a change), so an implementation need not de-duplicate.
/// </para>
/// <para>
/// Volumes: the game applies the master and effects levels to the engine mixer itself
/// (<c>AudioMixer.MasterVolume</c> / <c>AudioMixer.SfxVolume</c>); the MUSIC level is only ever passed here, so an
/// implementation applies it exactly once (through the provider's own volume, or the music bus - not both).
/// </para>
/// </remarks>
public interface IMusicDirector
{
    // ...
    /// <summary>The player changed the music model or the instrument library on the settings screen.</summary>
    /// <param name="settings">The new choices.</param>
    /// <param name="sector">The sector the music should suit (0 = the title).</param>
    /// <param name="boss">Whether a boss is on.</param>
    void ApplySettings(MusicSettings settings, int sector, bool boss);
    // ...
}
```

The session keeps that promise in one place. Screen changes and game events both
funnel into a single method that remembers the last moment and sector:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs
private void SetMusic(MusicMoment moment, int sector)
{
    if (moment == Music && sector == MusicSector)
    {
        return;
    }

    Music = moment;
    MusicSector = sector;
    switch (moment)
    {
        case MusicMoment.Title:
            _music.OnTitle();
            break;
        case MusicMoment.Sector:
            _music.OnSector(sector);
            break;
        case MusicMoment.Boss:
            _music.OnBoss(sector);
            break;
        case MusicMoment.GameOver:
            _music.OnGameOver();
            break;
    }
}
```

The second seam is the engine's own: `IGeneratedMusicStarter` starts a session and
hands back an `IGeneratedMusicSession`, `IMusicManager` is the music bus level, the
ducks and the stingers, and `IEngineDispatcher` is the engine thread. The director
takes all three (plus the registration call) in its constructor, and one fake
implements all three: it records each request, hands back a scripted session, and
can hold posted work so a test decides when the "engine thread" runs:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/FakeMusicEngine.cs
internal sealed class FakeMusicEngine : IGeneratedMusicStarter, IMusicManager, IEngineDispatcher
{
    // ...
    /// <summary>A director over this fake, counting registrations.</summary>
    public GeneratedMusicDirector CreateDirector() => new GeneratedMusicDirector(this, this, this, () => RegisterCount++);

    public IGeneratedMusicSession Start(GeneratedMusicOptions options)
    {
        if (StartFailure != null)
        {
            throw StartFailure;
        }

        Started.Add(options);
        var stream = new FakeMusicStream(options);
        Streams.Add(stream);
        return stream;
    }

    public IDisposable PushDuck(float depth, TimeSpan attack = default, TimeSpan release = default)
    {
        var duck = new FakeDuck(depth);
        PushedDucks.Add(duck);
        return duck;
    }

    public void Duck(float depth, TimeSpan attack, TimeSpan hold, TimeSpan release) => TimedDucks.Add(depth);
    // ...
    public void Post(Action action)
    {
        if (HoldPosts)
        {
            HeldPosts.Add(action);
        }
        else
        {
            action();
        }
    }
    // ...
}
```

The director's tests then read as the policy they pin. A sector is a follow-up on
the running session, never a second start; a state change raised off the engine
thread does nothing until the held posts run:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs
[Theory]
[InlineData(MusicChoices.SkyTNT, 1)]
[InlineData(MusicChoices.SkyTNT, 4)]
[InlineData(MusicChoices.MuPT, 2)]
[InlineData(MusicChoices.MuPT, 5)]
public void OnSector_follows_up_with_the_sector_preset_and_never_restarts(string generator, int sector)
{
    //Arrange
    var (director, engine) = Started(Choices(generator));

    //Act
    director.OnSector(sector);

    //Assert
    engine.Started.Should().HaveCount(1, "a sector is a follow-up on the running session, never a restart");
    engine.Current.FollowUps.Should().Equal(SectorMusic.For(sector).PresetFor(generator));
    director.Sector.Should().Be(sector);
    director.Boss.Should().BeFalse();
}
// ...
[Fact]
public void state_changes_from_other_threads_are_handled_on_the_engine_thread()
{
    //Arrange
    var (director, engine) = Started();
    engine.HoldPosts = true;

    //Act
    var before = CaptureLog(() => engine.Current.PlayModel("SkyTNT", "ModestSynthGm"));
    var after = CaptureLog(engine.RunHeldPosts);

    //Assert
    before.Should().BeEmpty();
    after.Should().Contain("[BrixInvaders] music: state Starting -> Playing");
    director.StarvedCount.Should().Be(0);
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/IMusicDirector.cs`,
`SilentMusicDirector.cs` and `GeneratedMusicDirector.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`SetMusic`,
`OnScreenChanged`, `HandleGameEvents`) and `Session/MusicMoment.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/FakeMusicEngine.cs`,
`FakeMusicStream.cs` and `RecordingMusicDirector.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs`
and `Session/ScreenFlowTests.cs`

**Sharp edges.**
- Two seams need two fakes. The session's tests use a director that records each
  call as a short string, so a whole screen flow asserts as one list; the
  director's tests use the fake of the engine's seams. Neither fake knows about the
  other.
- Fake the engine's interfaces rather than writing an adapter of your own. The
  engine's music types implement them (`EngineGeneratedMusicStarter`, `MusicManager`,
  the generated-music provider as the session, the engine's dispatcher), so the real
  director needs no code between it and the engine.
- Put the de-duplication on the caller's side of the seam and say so in the
  interface. Moving from one menu to another is not a musical moment; a director
  that is told about it would have to guess.
- Make the thread hop part of the seam. The real session raises its state changes
  on whatever thread it likes, so the director posts through the dispatcher before it
  logs or counts anything, and a fake that can hold posts is what proves it.
- Script failure on the fake, not just success. A start that throws and a
  follow-up the engine refuses both have tests: the game logs the reason and plays
  on, in silence if it must.
- A replaced session must stop being heard from. The director unsubscribes and
  also numbers its sessions, so a late event from the old one is ignored; the
  tests check both the subscriber count and the silence.

### Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title

**When you want this.** Your game has an in-game pause menu and a game-over
screen, and the music should react to both without stopping: quieter under the
menu, lower still under the game-over stinger the engine's `PlayStingerWithHeldDuck`
plays, and back to full as soon as play or the title resumes. [Duck the music for exactly as long as a line lasts](BLUEPRINTS-GameEngine.md#duck-the-music-for-exactly-as-long-as-a-line-lasts)
teaches the two forms a duck takes; this recipe is about the game's choice of
which moment gets which form, and every place a held duck has to be let go.

**The MVVM shape.** Not a view-model concern. The screen state machine turns the
pause key into pause and resume commands, the game session forwards them and the
musical moments to the music director, and the director owns the duck handles.
Nothing outside the director can release a duck it did not push.

**Code.**

The reasoning belongs where the next reader will look for it, with the depths and
timings beside it as named constants:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
/// <para>
/// PAUSE IS A DUCK, NOT A SUSPENSION. The pause menu is a GAME pause (the engine keeps running so the menu can take
/// input). Suspending the music track would stop pulling the stream and leave the model's next phrase waiting, and a
/// resumed stream can come back through a moment of silence; ducking keeps the music breathing quietly under the menu
/// - the calmer feel for a menu a player may sit on - and it is back at full level a moment after Resume.
/// </para>
/// <para>
/// The stingers ride the EFFECTS bus, so they still sound with the music turned down: the boss warning
/// (<c>AssetKeys.Sfx.BossWarning</c>) plays through <see cref="IMusicManager.PlayStingerOnBus"/> while the music ducks for
/// the three-second warning, and the game-over sting (<c>AssetKeys.Sfx.GameOver</c>) through
/// <see cref="IMusicManager.PlayStingerWithHeldDuck"/>, which holds the music down until the title.
/// </para>
// ...
/// </remarks>
public sealed class GeneratedMusicDirector : IMusicDirector
{
    /// <summary>The music level while the pause menu is up.</summary>
    public const float PauseDuckDepth = 0.35f;

    /// <summary>The music level under the boss warning.</summary>
    public const float BossDuckDepth = 0.3f;

    /// <summary>The music level while the game-over screen is up.</summary>
    public const float GameOverDuckDepth = 0.2f;
    // ...
    /// <summary>How long the music stays ducked under the boss warning (the warning lasts three seconds).</summary>
    public static readonly TimeSpan BossHold = TimeSpan.FromSeconds(GameSimulation.BossWarningDuration - 0.5);
```

The pause menu and the game-over screen each hold a handle in a field of their
own, pushed once however often the moment is reported. The game-over handle comes
from the engine's `PlayStingerWithHeldDuck`, which plays the sting on the effects
bus and ducks the music under it until the handle is disposed:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
public void OnGameOver()
{
    ReleasePauseDuck();
    _gameOverDuck ??= _music.PlayStingerWithHeldDuck(AssetKeys.Sfx.GameOver, GameOverDuckDepth, GameOverFade, GameOverRelease);

    GameLog.Write($"music: game over - fading down to {GameOverDuckDepth:0.##} under the game-over stinger; the session plays on");
}

/// <inheritdoc />
public void OnPause()
{
    if (_pauseDuck == null)
    {
        _pauseDuck = _music.PushDuck(PauseDuckDepth, PauseAttack, PauseRelease);
    }

    GameLog.Write($"music: paused - ducked to {PauseDuckDepth:0.##} (the music plays on under the pause menu)");
}

/// <inheritdoc />
public void OnResume()
{
    ReleasePauseDuck();
    GameLog.Write("music: resumed - back to full level");
}
```

Every moment that starts music afresh lets go of both, which is how a game over
stays ducked through the high-score screens and comes back only on the title:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
public void OnTitle()
{
    ReleaseDucks();
    Sector = SectorMusic.TitleSector;
    Boss = false;
    FollowUp(MusicSetup.FollowUpFor(_settings, SectorMusic.TitleSector), "title");
}
// ...
private void ReleaseDucks()
{
    ReleasePauseDuck();
    if (_gameOverDuck != null)
    {
        _gameOverDuck.Dispose();
        _gameOverDuck = null;
    }
}
```

The exit a player forgets is the one the tests remember - quitting to the title
from the pause menu never passes through resume:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs
[Fact]
public void quitting_to_the_title_from_the_pause_menu_releases_the_pause_duck()
{
    //Arrange
    var (director, engine) = Started();
    director.OnSector(1);
    director.OnPause();

    //Act
    director.OnTitle();

    //Assert
    engine.PushedDucks.Single().IsReleased.Should().BeTrue();
    director.IsPauseDucked.Should().BeFalse();
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs` (the
class remarks, the constants, `OnBoss`, `OnGameOver`, `OnPause`, `OnResume`,
`OnTitle`, `ReleaseDucks`)
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (the
`PauseGame` and `ResumeGame` commands)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs`
`BrixInvaders/DESIGN.md` (the music table of moments)

**Sharp edges.**
- A game pause is not an engine pause. The pause menu needs the engine running to
  read its input, so the music is ducked under it; minimizing the window is the
  engine's own pause, which suspends every voice, music included - see
  [Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu).
  Keep the two paths apart and do not duck for the second.
- Guard the push with the field. The moment can be reported again, and a second
  handle would hold the music down after the first is released.
- List every way out of a ducked moment. Resume releases the pause duck, but
  quitting from the pause menu lands on the title, and the game-over moment
  releases any pause duck before it pushes its own.
- Tie a timed duck to the rule it sits under. The boss duck's hold is computed
  from the simulation's warning length, so retuning the warning cannot leave the
  music down after it ends.
- Ducks multiply with the music level rather than replacing it, so the player's
  slider survives every duck.
- Play a stinger that must be heard on the effects bus. The engine's plain
  `PlayStinger` rides the music bus, so the music slider - and its own duck - would
  turn a warning down with the music; `PlayStingerOnBus(key, AudioBus.Sfx)` and
  `PlayStingerWithHeldDuck` do not. The stinger key is the loaded sound's key, and
  the sound table leaves those two events out so they do not play twice.

### Keep the music for each level in a table the tests can read

**When you want this.** Each level of your game should sound like itself, the
player should choose which model writes the music and which instruments play it,
and you want a wrong preset name, a missing level or two neighboring levels that
sound alike to fail a test rather than a play session.

**The MVVM shape.** Not a view-model concern. A small library with no screen and
no engine owns the table and the choices as plain data, named exactly as the
music packages register them. The game's settings menu reads labels from it and
cycles through it, reports a change as a record, and the session passes that
record to the music director, which decides whether the running music can carry
on.

**Code.**

The table is a static list: one entry per level design and one for the title,
each naming a preset for both models, a tempo and the boss's choices. Endless
play wraps onto the same designs:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Music/Sectors/SectorMusic.cs
/// <summary>The title screen's music. It has no boss of its own; its boss presets repeat its own presets.</summary>
public static SectorMusicEntry Title { get; } = new(
    TitleSector, "Title", "calm and spacious - waiting in orbit before the invasion",
    "AmbientElectronica", "WaltzDuetInAMinor", 108,
    "AmbientElectronica", "WaltzDuetInAMinor", 108);

private static readonly SectorMusicEntry[] _designs =
[
    new(1, "Outer Picket", "steady and bright - the first patrol along the picket line",
        "FourOnTheFloor", "HornpipeInG", 120,
        "ClubArrangement", "ReelInGMinor", 128),
    // ...
];
// ...
public static int DesignFor(int sector)
{
    if (sector < 0) { throw new ArgumentOutOfRangeException(nameof(sector), sector, "A sector is 0 (the title screen) or more."); }
    return sector == TitleSector ? TitleSector : (sector - 1) % DesignCount + 1;
}
```

Because the table is data, the tests can hold it to the music packages' own preset
lists and to the design rules it was written for:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Music.Tests/SectorMusicTests.cs
[Fact]
public void every_SkyTNT_preset_in_the_table_is_a_SkyTNT_preset()
{
    //Arrange
    var names = SkyTNTPresets.All.Select(p => p.Name).ToArray();
    var entries = SectorMusic.Designs.Append(SectorMusic.Title).ToArray();

    //Assert
    entries.Should().OnlyContain(e => names.Contains(e.SkyTNTPreset) && names.Contains(e.BossSkyTNTPreset));
}
// ...
[Fact]
public void neighbouring_sectors_sound_different()
{
    //Arrange
    var designs = SectorMusic.Designs;

    //Assert
    Enumerable.Range(0, designs.Count - 1).Should().OnlyContain(i =>
        designs[i].SkyTNTPreset != designs[i + 1].SkyTNTPreset && designs[i].MuPTPreset != designs[i + 1].MuPTPreset);
}
```

The choices the settings screen offers are the packages' own registry names,
taken from their constants, with the words the player reads beside each:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Music/Choices/MusicChoices.cs
/// <summary>The SkyTNT model's registry name (electronica, with drums).</summary>
public const string SkyTNT = SkyTNTModel.GeneratorName;

/// <summary>The MuPT model's registry name (folk and classical tunes in parts, no drums).</summary>
public const string MuPT = MuPTModel.GeneratorName;
// ...
private static readonly MusicChoice[] _generators =
[
    new MusicChoice(SkyTNT, "SkyTNT - electronica",
        "Electronic music with drums, synth bass, leads and pads."),
    new MusicChoice(MuPT, "MuPT - folk and classical",
        "Reels, jigs, waltzes and airs written in parts, with no drums."),
];
// ...
public static string NextGenerator(string current, int direction) =>
    Cycle(_generators, ResolveGenerator(current), direction);
```

The settings menu stores the registry name, shows the label, and tells the session
that a music choice changed rather than acting on it:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsMenu.cs
case MusicModelRow:
    _settings.MusicGenerator = MusicChoices.NextGenerator(_settings.MusicGenerator, step);
    return SettingsChange.MusicChoice;
case InstrumentLibraryRow:
    _settings.InstrumentLibrary = MusicChoices.NextInstrumentLibrary(_settings.InstrumentLibrary, step);
    return SettingsChange.MusicChoice;
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Music/Sectors/SectorMusic.cs` and
`SectorMusicEntry.cs` (`CheckPreset`, `CheckTempo`)
`BrixInvaders/src/libs/BrixInvaders.Music/Choices/MusicChoices.cs` and
`MusicChoice.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsMenu.cs` (`ValueOf`,
`Adjust`) and `SettingsService.cs` (`MusicGenerator`, `InstrumentLibrary`)
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`Apply`)
`BrixInvaders/tests/libs/BrixInvaders.Music.Tests/SectorMusicTests.cs`,
`SectorMusicEntryTests.cs` and `MusicChoicesTests.cs`
`BrixInvaders/DESIGN.md` (the per-sector table and the reasons for each choice)

**Sharp edges.**
- Take the names from the packages' constants. A renamed model or instrument
  library is then a build error in the table, not a choice that silently falls
  back to the default.
- Check every preset against its model's preset family when the entry is built;
  the entry throws with the list of valid names. A static table is built on first
  use, so a test that reads it is what turns a typo into a failure.
- Store the registry name, never the label, and resolve it without regard to case
  when it is read back. The settings facade returns the default for a stored name
  the game does not offer, so an old settings file cannot stop the music.
- The title has no boss. Asking for boss music on the title is an argument error
  in the options builder rather than a quiet repeat of the title music.
- The tempo column is used only where a session starts, at start-up or after a
  change of model or instruments; a level change is a follow-up on the running
  session. The CodeBrix.Audio.MusicGeneration blueprints (BLUEPRINTS-GeneratingMusic.md
  in that library) cover how a session holds its pulse across pieces.

### Show the player which model and instruments are really playing

**When you want this.** Your credits or an about screen should say where the music
comes from, and the honest answer is not always the player's setting: the model
may still be loading, or it may be missing and a fallback piece playing in its
place.

**The MVVM shape.** The view model wires it. It creates the music director first
and hands the director's credit-lines method to the credits content as a
delegate, so the credits screen asks for the lines only when it opens. The lines
themselves are built by a small static class in the game library from the engine's
plain source record, which is what the tests hand it.

**Code.**

What is playing comes from the engine's generated-music session as a plain
`GeneratedMusicSourceInfo` (generator, its family, the instrument library, and whether
the fallback replay is playing), read by the director when the card is built:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs
public IReadOnlyList<CreditsLine> CreditLines() => MusicCreditsCard.Lines(_stream?.ActiveSourceInfo, _settings);
```

The card has three answers - warming up, a fallback piece, or a model - and only
the last one claims a model wrote the music:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Credits/MusicCreditsCard.cs
public static IReadOnlyList<CreditsLine> Lines(GeneratedMusicSourceInfo source, MusicSettings chosen)
{
    string first;
    if (source == null)
    {
        var settings = chosen ?? new MusicSettings();
        first = WrittenLiveLine(MusicChoices.ResolveGenerator(settings.GeneratorName),
            MusicChoices.ResolveInstrumentLibrary(settings.InstrumentLibraryName)) + " (the model is warming up)";
    }
    else if (source.IsReplay)
    {
        first = $"Music: a recorded piece (no music model was found) through {source.InstrumentLibraryName}";
    }
    else
    {
        first = WrittenLiveLine(source.GeneratorName, source.InstrumentLibraryName);
    }

    return new List<CreditsLine>
    {
        CreditsLine.Body(first),
        CreditsLine.Body(ComposedByLine),
        CreditsLine.Body(PlayedThroughLine),
    };
}
```

The view model passes functions rather than values, because the credits content
is built before the host has read the packs or the music has started:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs
var music = new GeneratedMusicDirector(new EngineGeneratedMusicStarter(), MusicManager.Instance,
    Engine.Instance.EngineDispatcher);
var links = new LauncherLinkOpener();
BrixInvadersGameHost host = null;
var credits = new KenneyCreditsContent(() => host?.PackCredits ?? Array.Empty<string>(), music.CreditLines);
host = new BrixInvadersGameHost(canvas, music, links, credits);
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs`
(`CreditLines`)
`BrixInvaders/src/libs/BrixInvaders.Game/Credits/MusicCreditsCard.cs` and
`KenneyCreditsContent.cs`
`BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs` (`CanvasFirstStart`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Credits/MusicCreditsCardTests.cs`
and `Audio/GeneratedMusicDirectorTests.cs` (`CreditLines_name_the_model_and_library_that_really_play`)

**Sharp edges.**
- Read the provider, not the settings. With no model found the music package plays
  its embedded replay, and a card built from the settings would credit a model
  that is not playing; a test pins that the replay line never says "written live".
- Before the music starts there is no source at all. Show the player's choice and
  say it is warming up, rather than an empty line or an error.
- Build the lines each time the credits screen opens, not once. What is playing
  changes when the player picks another model, and until the new music plays the
  card falls back to the new choice, marked as warming up.
- Name the package families in words and keep version numbers off the screen; a
  test checks that the family lines carry no digits.
- The credit lines hang off the concrete director rather than the director
  interface, which is why the view model captures the director before handing it
  to the host.
