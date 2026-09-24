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
that a unit test can play from start to finish. Reach for this file when an
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
- [Register downloaded Kenney zip files and load sprites, sounds and fonts from them](#register-downloaded-kenney-zip-files-and-load-sprites-sounds-and-fonts-from-them)
- [Start endless generated music with one call](#start-endless-generated-music-with-one-call)
- [Read keyboard and gamepad together and switch on-screen prompts](#read-keyboard-and-gamepad-together-and-switch-on-screen-prompts)
- [Show a splash card and hand over to a title screen](#show-a-splash-card-and-hand-over-to-a-title-screen)
- [Keep a deterministic game simulation apart from the engine](#keep-a-deterministic-game-simulation-apart-from-the-engine)

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

**Sharp edges.**
- Two shapes, one rule: a stinger ducks for its own length and needs nothing
  released, and a pushed duck lasts exactly as long as you hold the handle.
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

### Register downloaded Kenney zip files and load sprites, sounds and fonts from them

**When you want this.** You downloaded a handful of Kenney asset packs and want to
start building the game on them today: ship the zips exactly as they came, and ask
the engine for a sprite, a sound or a font by name, with no extraction step, no
converted atlas and no asset pipeline of your own.

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

One call hands all of them to the engine's Kenney asset provider, and the library
refuses a folder with none of the zips in it before the process-wide provider is
touched:

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

    KenneyGameAssetProvider existing = FindProvider(engine);
    int warningsBefore = existing?.Warnings.Count ?? 0;
    string[] toAdd = zipPaths
        .Where(path => existing is null || !existing.Packs.Any(pack => SamePath(pack.SourcePath, path)))
        .ToArray();

    KenneyGameAssetProvider provider = toAdd.Length > 0 || existing is null
        ? engine.UseKenneyAssets(toAdd)
        : existing;
    // ...
}
```

After that, every load is an ordinary engine call with a key. Sprites come from the
packs' own sprite-sheet atlases and are addressed by the frame names Kenney gave them;
sounds and fonts go into the engine's own registries:

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Assets/Loading/BrixInvadersAssets.cs
public static Tilesheet LoadMainAtlas(Engine engine) => LoadImage(engine, AssetKeys.Atlases.Main);

public static Frame GetFrame(Tilesheet atlas, string frameName)
{
    ArgumentNullException.ThrowIfNull(atlas);
    ArgumentNullException.ThrowIfNull(frameName);

    if (atlas.GetRegion(frameName) is null)
    {
        throw new ArgumentException($"The atlas '{atlas.Name}' has no frame named '{frameName}'.", nameof(frameName));
    }

    return atlas[frameName, 0, 0];
}

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

The host's `LoadAssets` override is then a handful of lines:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
protected override void LoadAssets()
{
    _provider = BrixInvadersAssets.Register(Engine);
    foreach (var pack in _provider.Packs)
    {
        _packCredits.Add(KenneyPacks.PackCreditLine(pack));
    }

    AssetReport.LogAll(Engine);
    var sounds = BrixInvadersAssets.LoadSounds(Engine);
    // ...
    var fonts = BrixInvadersAssets.LoadFonts(Engine);
    _typeface = FontManager.Instance.Get(fonts.TextKey);
    _thinTypeface = FontManager.Instance.Get(fonts.ThinKey);
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
- Register once. The provider is process-wide; adding the same zip again gives its
  pack a second slug with a numeric suffix and a second set of keys, which is why the
  registration skips zips the provider already holds.
- A pack's slug comes from the title line of its license file, or from the zip's file
  name when that line is unusable. Read the slugs from the provider once, keep them as
  constants, and let a test hold them, so a change in how a pack is named fails a test
  rather than a frame.
- Prefer a pack's sprite-sheet atlas to its loose images: one tilesheet, frames by
  name, and one texture for the renderer.
- Keep every key the game uses in one class and prove in a test that each resolves
  against the real zips. A typo in a key is then a failing test, not a missing sprite
  discovered in play.
- A picture that is not part of a pack - here Kenney's promo image for the whole
  bundle - does not go through the provider. Load it as a plain tilesheet from its
  file, and give its default region the whole image as its tile, or `sheet[0, 0]` is
  not the picture.
- Kenney's content is CC0, so credit is optional; give it anyway, on screen and in the
  application's notices file.

### Start endless generated music with one call

**When you want this.** The game should have music from the first screen to the last,
it should never loop audibly, and it should change character with the level - without
anyone composing, recording or shipping a soundtrack.

**The MVVM shape.** Split in three. A small library owns the choices the player can
make and a table of which preset each level plays, and turns settings into options.
One class in the game library owns the music policy - when to start, follow up, duck
and restart - behind an interface, so the game session's tests drive it with a fake.
A thin adapter over the engine is the only code that calls the engine. The view model
constructs the real director and hands it to the game host.

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

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Audio/EngineMusicEngine.cs
public IMusicStream Start(GeneratedMusicOptions options) => new ProviderMusicStream(Engine.Instance.UseGeneratedMusic(options));

public void SetMusicVolume(double volume) => AudioMixer.MusicVolume = (float)Math.Clamp(volume, 0.0, 1.0);
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
    _engine.Duck(BossDuckDepth, BossAttack, BossHold, BossRelease);
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
`BrixInvaders/src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs`,
`EngineMusicEngine.cs` and `ProviderMusicStream.cs`
`BrixInvaders/src/BrixInvaders.Core/ViewModels/MainViewModel.cs` (`CanvasFirstStart`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Audio/GeneratedMusicDirectorTests.cs`

**Sharp edges.**
- Register an instrument library, always. The add-in registers nothing, and with no
  library there is no sound, only a faulted state and a log line.
- The first library registered becomes the default. When more than one is registered,
  name the library in the options, and set the default explicitly in case something
  else registered first.
- With no model registered, or its files missing, the music package plays its embedded
  replay - the same piece every time. Log what is registered and whether each model's
  files were found, so a silent fallback is visible.
- The tempo in the options is the session's tempo. A follow-up changes the preset and
  keeps the pulse, so a per-level tempo applies only where a session starts.
- A follow-up is not instant: the new music takes over at a bar line once it is ready.
  Trigger it early - at the sector briefing, at the boss warning - not at the moment
  it should be heard.
- Apply the player's music level once. This game drives the engine's music bus and
  leaves the session's own level at full; doing both multiplies them.
- The provider's state-changed event can arrive on the audio thread. Post to the
  engine dispatcher before touching game state or logging from it.
- Nothing here waits for the music. Silence while a model loads, and short waits on a
  slow machine, are ordinary states, never errors.

### Read keyboard and gamepad together and switch on-screen prompts

**When you want this.** The player should be able to pick up a controller in the
middle of a game, or put it down and use the keyboard, with no setting to change - and
the prompts on screen should show the buttons of whatever they are holding.

**The MVVM shape.** Not a view-model concern. The game host samples every device once
per engine cycle into one plain value; a mapper class with no engine dependency turns
that value into what the rules read - held input for play, edge-triggered presses for
menus - and remembers which device was used last. The screens draw their prompts from
that device, and the settings facade stores it for the next run.

**Code.**

Every engine cycle, the host reads the keyboard adapter and every connected gamepad
into one sample; a missing gamepad manager simply leaves the buttons empty:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
private RawInput ReadRawInput()
{
    var keys = InputKeys.None;
    var keyboard = Engine.Input.KeyboardEventPoller?.Adapter;
    if (keyboard != null)
    {
        foreach (var (key, flag) in KeyBindings.Keys)
        {
            if (keyboard.IsDown((int)key))
            {
                keys |= flag;
            }
        }
    }

    var buttons = PadButtons.None;
    double stickX = 0;
    double stickY = 0;
    if (_gamepads != null && _gamepads.IsAvailable)
    {
        foreach (var pad in _gamepads.ConnectedAdapters)
        {
            var pressed = pad.PressedButtons;
            foreach (var (button, flag) in KeyBindings.Buttons)
            {
                if (pressed.Contains(button))
                {
                    buttons |= flag;
                }
            }

            if (pad.LeftStick is { } stick && stick.Magnitude > Math.Sqrt((stickX * stickX) + (stickY * stickY)))
            {
                stickX = stick.X;
                stickY = stick.Y;
            }
        }
    }

    return new RawInput(keys, buttons, stickX, stickY);
}
```

The mapper compares each sample with the last one. A newly pressed key or button
latches its menu action until the next fixed step takes it, and records which device
pressed it:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/InputMapper.cs
var pressedKeys = raw.Keys & ~_latest.Keys;
var pressedButtons = raw.Buttons & ~_latest.Buttons;

if (pressedKeys != InputKeys.None)
{
    LastDevice = InputDevice.Keyboard;
    LatchKeys(pressedKeys);
}

if (pressedButtons != PadButtons.None)
{
    LastDevice = InputDevice.Gamepad;
    LatchButtons(pressedButtons);
}

LatchStick(raw);
_latest = raw;
```

Held input merges both devices, with the gamepad profile choosing the fire and bomb
buttons:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Input/InputMapper.cs
axis += ApplyDeadZone(_latest.StickX);
var fireButton = Profile == GamepadProfile.Classic ? PadButtons.A : PadButtons.RightShoulder;
var bombButton = Profile == GamepadProfile.Classic ? PadButtons.B : PadButtons.LeftShoulder;
var fire = Has(keys, InputKeys.Space) || Has(buttons, fireButton);
var bomb = Has(keys, InputKeys.LeftShift) || Has(buttons, bombButton);

return new GameInput(Math.Clamp(axis, -1.0, 1.0), fire, bomb);
```

And a prompt is a function of the device and the profile:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/Prompts.cs
public static string Fire(InputDevice device, GamepadProfile profile) =>
    device != InputDevice.Gamepad ? "[SPACE]" : profile == GamepadProfile.Shoulder ? "[RB]" : "[A]";
```

Because the mapper takes plain values, both devices at once is an ordinary unit test:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/InputMapperTests.cs
[Fact]
public void keyboard_and_gamepad_work_at_the_same_time()
{
    //Arrange
    var mapper = Started();

    //Act
    mapper.Sample(new RawInput(InputKeys.Right, PadButtons.A));

    //Assert
    mapper.GameInput.MoveAxis.Should().Be(1.0);
    mapper.GameInput.Fire.Should().BeTrue();
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`
(`OnConfigureGamepads`, `ReadRawInput`, `Step`) and `Hosting/KeyBindings.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Input/InputMapper.cs` and `Screens/Prompts.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Input/InputMapperTests.cs`

**Sharp edges.**
- The engine cycles far more often than the rules step. Latch menu presses until a
  step takes them, or a tap shorter than a step is lost; read held input from the
  latest sample.
- Whatever is already held when sampling starts is not a press, and a stick already
  pushed must come back before it counts. Otherwise the key that launched the game
  selects the first menu item.
- A stick used as a menu direction needs hysteresis: fire past one threshold, re-arm
  below a lower one, or a stick resting near the edge repeats.
- Decide once what a shared key means. Here Escape is both back and pause, and each
  screen reads the one it cares about.
- Gamepad support can be absent - no native library, no permission, no controller.
  Log why once and carry on with the keyboard; hot-plugging needs nothing more,
  because every cycle reads whichever controllers are connected now.
- Store the last device used, so the next run's first screen already shows the right
  prompts.

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
        using var stream = SplashComposer.Compose(_images, _typeface, _thinTypeface);
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
public static Stream Compose(ImageLibrary images, SKTypeface typeface, SKTypeface thinTypeface)
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

### Keep a deterministic game simulation apart from the engine

**When you want this.** The rules of the game are the part most worth testing and the
part hardest to test through a running engine. You want to play a whole game in a
unit test, in milliseconds, and get the same game every time from the same seed and
the same inputs.

**The MVVM shape.** The rules live in a library of their own that references nothing
- no engine, no Skia, no file or clock. It is advanced in fixed steps with a plain
input value, exposes its state for drawing, and reports what happened in each step as
a list of events. The game library owns the engine loop and turns the events into
sprites, sounds and particles; the view model never touches either.

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

The host runs it from the engine loop at a fixed rate, whatever rate the loop runs
at, and caps the catch-up so a stall cannot make the game run away:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs
_accumulator += elapsed;
var steps = 0;
while (_accumulator >= Playfield.FixedStep && steps < MaxStepsPerCycle)
{
    Step();
    _accumulator -= Playfield.FixedStep;
    steps++;
}

if (steps == MaxStepsPerCycle)
{
    _accumulator = Math.Min(_accumulator, Playfield.FixedStep);
}

if (steps > 0)
{
    BuildFrame();
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
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`OnCycle`, `Step`, `BuildFrame`)
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`Update`)
`BrixInvaders/tests/libs/BrixInvaders.GameLogic.Tests/Simulation/GoldenSeedRuns.cs`
`BrixInvaders/DESIGN.md` (every number the rules use)

**Sharp edges.**
- Use a random generator you own. `System.Random`'s sequence for a seed is allowed to
  change between runtime versions, which would move every pinned game.
- No clock inside the rules. Time arrives as the fixed step, so the same inputs give
  the same game on a fast machine, a slow one and a test runner.
- Timers counted down in floating point can land a hair above zero and take one step
  too many. Snap values within a tiny epsilon of zero to zero, and fence it with a
  test that counts steps.
- A pinned golden value that moves means the rules changed - a tuning value, the order
  of events, the order of random draws. Update the pins in the same change on purpose,
  never to make a test pass.
- Hand the renderer an immutable frame built after the steps, not the live simulation:
  the render side may draw on another thread, and it should never see a half-updated
  game.
- Keep the events as data. The rules say an enemy exploded; the game library decides
  what that looks and sounds like, and the rules tests never load a sprite.
