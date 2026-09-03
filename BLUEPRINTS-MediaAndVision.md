# CodeBrix.Samples Blueprints: Media, camera and vision

These recipes cover playing video and audio inside a page, inspecting and
converting media files, and driving camera capture and on-device vision
from a CodeBrix.Platform application. Reach for the playback recipes when
you want transport, chapter and caption state to live in the view model
while the page owns the real element and exposes it through a narrow bridge
interface. Reach for the media-processing recipes when you need to probe a
file, tell two container formats apart from their first bytes, lift embedded
chapters and captions into sidecar files, or run an encode from a settled
plan with the quality and resolution choices written down and explained. The
camera and vision recipes cover enumerating devices, starting and switching
a capture session without leaking the device library into the view model,
running a model over each frame, and turning raw model output into smoothed
results with stable identities.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Host the VideoPlayer add-in in a page and drive it from the view model](#host-the-videoplayer-add-in-in-a-page-and-drive-it-from-the-view-model)
- [Play a video from a URL with the MediaPlayer add-in](#play-a-video-from-a-url-with-the-mediaplayer-add-in)
- [Play an audio clip straight from bytes with the AudioPlayer add-in](#play-an-audio-clip-straight-from-bytes-with-the-audioplayer-add-in)
- [Probe a media file behind an interface the view model resolves](#probe-a-media-file-behind-an-interface-the-view-model-resolves)
- [Detect a container from its first bytes](#detect-a-container-from-its-first-bytes)
- [Author a cbv file in either container mode from a settled plan](#author-a-cbv-file-in-either-container-mode-from-a-settled-plan)
- [Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library](#export-an-mp4-with-ffmpeg-through-the-codebrix-videoprocessing-library)
- [Demultiplex a bespoke container and remux it so an external tool can read it](#demultiplex-a-bespoke-container-and-remux-it-so-an-external-tool-can-read-it)
- [Lift chapters and captions out of a source into sidecar files](#lift-chapters-and-captions-out-of-a-source-into-sidecar-files)
- [Build a resolution ladder keyed on the short side with even dimensions](#build-a-resolution-ladder-keyed-on-the-short-side-with-even-dimensions)
- [Move one encoder knob and pin everything else](#move-one-encoder-knob-and-pin-everything-else)
- [Download run scoped media into a self cleaning temp cache](#download-run-scoped-media-into-a-self-cleaning-temp-cache)
- [Extract a video poster frame and degrade when the external tool is missing](#extract-a-video-poster-frame-and-degrade-when-the-external-tool-is-missing)
- [Enumerate cameras and start a live capture session](#enumerate-cameras-and-start-a-live-capture-session)
- [Wrap a device library type so the view model never sees it](#wrap-a-device-library-type-so-the-view-model-never-sees-it)
- [Run a TFLite model through the OpenCV DNN module](#run-a-tflite-model-through-the-opencv-dnn-module)
- [Warp a rotated region of interest into a model input](#warp-a-rotated-region-of-interest-into-a-model-input)
- [Recognize a gesture from landmark geometry instead of a model](#recognize-a-gesture-from-landmark-geometry-instead-of-a-model)
- [Track multiple detections across frames with stable ids](#track-multiple-detections-across-frames-with-stable-ids)
- [Smooth a noisy sensor position before it drives the UI](#smooth-a-noisy-sensor-position-before-it-drives-the-ui)

## Related blueprints

- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - the bridge interfaces and settable delegates these recipes use to reach an element from a view model are described in full there
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - busy flags, SimpleCommand transport commands, worker-thread work and marshalling results back to the UI thread
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - drawing captured frames and tracking results onto a canvas once the pipeline has produced them
- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - registering codecs and resolving a probe or capture service with SimpleServiceResolver at startup

---

## Media, camera and vision

### Host the VideoPlayer add-in in a page and drive it from the view model

**When you want this.** You are putting the CodeBrix.Platform VideoPlayer add-in
on a page and you want the transport, the chapter list and the caption list to be
view-model state rather than code-behind state.

**The MVVM shape.** The view model owns every decision - what is open, whether it
can be played at all, what the transport may do, which chapter and which caption
track are showing - and reaches the element only through an interface the library
declares and the page implements over the real control. The page constructs the
implementation once when its data context arrives, hands it to the view model, and
forwards the element's events in one line each. Position, duration and volume are
deliberately not on the interface: those are dependency properties the scrubber,
timecodes and volume slider bind straight to.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs
public interface IVideoPlayerSurface
{
    void Open(string path);
    void Close();
    void Play();
    void Pause();
    void Stop();
    void SeekToChapter(int index);
    void SelectCaptionTrack(CaptionTrack track);

    TimeSpan Duration { get; }
    bool IsPlaying { get; }
    IReadOnlyList<Chapter> Chapters { get; }
    IReadOnlyList<CaptionTrack> CaptionTracks { get; }
    int CurrentChapterIndex { get; }

    event EventHandler MediaOpened;
    event EventHandler PlaybackEnded;
    event EventHandler<string> MediaFailed;
    event EventHandler PlayStateChanged;
    event EventHandler ChapterChanged;
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private sealed class VideoPlayerSurface : IVideoPlayerSurface
{
    private readonly VideoPlayer player;

    internal VideoPlayerSurface(VideoPlayer player)
    {
        this.player = player;
        player.RegisterPropertyChangedCallback(
            VideoPlayer.IsPlayingProperty, (_, _) => PlayStateChanged?.Invoke(this, EventArgs.Empty));
        player.ChapterChanged += (_, _) => ChapterChanged?.Invoke(this, EventArgs.Empty);
    }

    // ...

    public void Open(string path)
    {
        //The source has to be unloaded before anything read at open time is changed, and the
        //real path comes last.
        player.Source = "";
        player.AutoPlay = false;
        player.Source = path;
    }

    public void Close() => player.Source = "";

    public void Play() => player.Play();

    public void SeekToChapter(int index) => player.SeekToChapter(index);

    public void SelectCaptionTrack(CaptionTrack track) => player.SelectedCaptionTrack = track;

    internal void RaiseMediaOpened() => MediaOpened?.Invoke(this, EventArgs.Empty);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private void WireViewModel()
{
    if (ViewModel is not { } viewModel)
    {
        return;
    }

    surface ??= new VideoPlayerSurface(Player);
    viewModel.Playback.AttachSurface(surface);
    viewModel.PickMediaFileAsync = PickMediaFileAsync;
    viewModel.Conversion.PickOutputPathAsync = PickOutputPathAsync;
}

private void Player_MediaOpened(object sender, EventArgs e) => surface?.RaiseMediaOpened();

private void Player_PlaybackEnded(object sender, EventArgs e) => surface?.RaisePlaybackEnded();

private void Player_MediaFailed(object sender, VideoPlayerFailedEventArgs e) => surface?.RaiseMediaFailed(e.Message);
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page
    xmlns:video="clr-namespace:CodeBrix.Platform.UI.VideoPlayer.Skia;assembly=CodeBrix.Platform.UI.VideoPlayer.Skia">
  <!-- The stage. The player letterboxes whatever it is given inside it. -->
  <Grid Grid.Row="0" Background="{StaticResource AppStageBrush}">
      <video:VideoPlayer x:Name="Player"
                         Stretch="Uniform"
                         MediaOpened="Player_MediaOpened"
                         PlaybackEnded="Player_PlaybackEnded"
                         MediaFailed="Player_MediaFailed" />
  </Grid>
</Page>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Sharp edges.**
- Opening has an order: unload the source, change anything read at open time, then
  assign the real path last.
- A dependency property with no event needs a registered property-changed callback
  so the surface can raise its own event.
- XAML-declared handlers arrive on the page, not on the surface, so the page
  forwards each in one line to an internal raise method.
- The wiring runs from the data-context-changed handler, not from the constructor,
  because the data context is created by the XAML.
- Codecs the add-in does not carry itself must be registered by the application at
  startup; see the startup area.

### Play a video from a URL with the MediaPlayer add-in

**When you want this.** Video or audio playback inside a page with the source
chosen by application logic rather than hard-coded in XAML, and standard play,
pause and seek behavior without writing your own commands.

**The MVVM shape.** The view model owns the address string and the resulting
playback source; it builds the source and exposes it as a bound property with a
private setter. The page declares the element with its source one-way bound and
turns on the built-in transport controls. No bridge interface is needed for
playback itself, because the add-in's element is a normal XAML control.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
using CodeBrix.Platform.Simple;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
// ...
private void LoadMedia()
{
    try
    {
        var uri = new Uri(MediaAddress);
        PlayerSource = MediaSource.CreateFromUri(uri);
        StatusText = $"Loaded: {uri}";
    }
    catch (Exception ex)
    {
        StatusText = $"Cannot load '{MediaAddress}': {ex.Message}";
    }
}

public IMediaPlaybackSource PlayerSource
{
    get;
    private set => SetProperty(ref field, value);
}
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<MediaPlayerElement Grid.Row="1" Margin="0,10,0,10"
                    AutoPlay="True"
                    AreTransportControlsEnabled="True"
                    Source="{d:Binding PlayerSource, Mode=OneWay}"
                    Stretch="{d:Binding SelectedStretch, Mode=OneWay}" />
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml`

**Sharp edges.**
- The property type on the view model is the playback-source interface, not the
  concrete type the factory returns; the element's `Source` takes the interface.
- The media types arrive with the MediaPlayer add-in, not with the base platform.
  Without the add-in reference the element and the source type do not resolve.
- Creating a source from a URI succeeds for any well-formed URI. Constructing the
  URI is the only validation here; an unreachable or unplayable address fails
  silently at the element.
- Setting the source is what starts playback, because auto-play is on. If you do
  not want playback on launch, turn auto-play off rather than withholding the
  source.
- With the built-in transport doing the work there is no view-model notion of
  playing, paused or ended. If your application needs to react to playback state,
  reach the underlying player behind an interface the view model consumes, as the
  video-player blueprint above does.
- Assigning a new source replaces the old one; this sample never disposes the
  previous source.

### Play an audio clip straight from bytes with the AudioPlayer add-in

**When you want this.** You have audio in memory - read out of an archive,
downloaded or generated - and want it played without writing a temporary file, on
whichever heads can.

**The MVVM shape.** The view model owns the transport commands and the loop state,
and reaches the element through a bridge of settable delegates that it implements
itself. The page fills the delegates in from its data-context-changed handler.
Every call site is null-guarded, so a head with no player degrades to a viewer
that says so.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs
public interface IAudioPlayerBridge
{
    /// <summary>
    /// Hands the player a seekable stream of an audio file it can decode (Ogg Vorbis, WAV, MP3
    /// or FLAC); the player takes ownership of it.
    /// </summary>
    Action<Stream> LoadAudioSource { get; set; }

    Action PlayAudio { get; set; }
    Action PauseAudio { get; set; }
    Action StopAudio { get; set; }
    Action<bool> SetAudioLooping { get; set; }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private async Task OpenAudioAsync(AssetEntry entry)
{
    var bytes = await ReadArchiveBytesAsync(entry.EntryPath)
        ?? throw new InvalidDataException($"The bundle has no entry “{entry.EntryPath}”.");

    //Kenney audio is Ogg Vorbis, which the AudioPlayer add-in decodes itself (as it does
    //WAV, MP3 and FLAC) — the bytes go straight to the player, whatever the format.
    var audioStream = new MemoryStream(bytes, writable: false);

    // ... header and facts ...

    IsAudioLooping = false;
    SetAudioLooping?.Invoke(false);
    LoadAudioSource?.Invoke(audioStream);
    SetViewerMode(ViewerMode.Audio,
        LoadAudioSource == null ? "audio playback is not available on this head" : string.Empty);
}

public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(() => PlayAudio?.Invoke());
public SimpleCommand PauseAudioCommand => field ??= new SimpleCommand(() => PauseAudio?.Invoke());
public SimpleCommand StopAudioCommand => field ??= new SimpleCommand(() => StopAudio?.Invoke());

public SimpleCommand ToggleAudioLoopCommand => field ??= new SimpleCommand(() =>
{
    IsAudioLooping = !IsAudioLooping;
    SetAudioLooping?.Invoke(IsAudioLooping);
});
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Audio bridge: the view model hands over the clip's raw stream and transport
//calls; the AudioPlayer element does the decoding and playing (it takes
//stream ownership)
viewModel.LoadAudioSource = stream =>
{
    _audioPlaybackEnded = false;
    AudioElement?.SetSourceStream(stream);
};
viewModel.PlayAudio = PlayAudio;
viewModel.PauseAudio = () => AudioElement?.Pause();
viewModel.StopAudio = () =>
{
    _audioPlaybackEnded = false;
    AudioElement?.Stop();
};
viewModel.SetAudioLooping = looping =>
{
    if (AudioElement != null) { AudioElement.IsLooping = looping; }
};
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<audio:AudioPlayer x:Name="AudioElement" />
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The element takes ownership of the stream handed to it; do not dispose it
  yourself, and do not hand it the same stream twice.
- The element decodes several common formats itself, so the application never
  needs a format check before playing.
- Opening a different asset stops whatever was playing first, as does leaving the
  viewer.
- The interface documents the contract: the view model must behave sensibly when a
  delegate is null, and the pane's hint text is what the user sees on a head where
  the bridge was never filled in.
- The scrubber binds straight to the element; see the views area. Replaying a
  finished clip needs one extra rule; see the bridge area.

### Probe a media file behind an interface the view model resolves

**When you want this.** You need to know what is inside a media file - size,
duration, codecs, chapter and caption counts - before you offer anything to do
with it.

**The MVVM shape.** The probe is registered as a singleton at startup and resolved
in the view model's constructor. The view model calls it inside a try/catch, sets
the busy flag around the call so the commands disable themselves, and turns any
failure into one sentence in the status bar.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/IMediaProbe.cs
public interface IMediaProbe
{
    /// <summary>
    /// Probes one file. A <c>.cbv</c> file is read by the playback core's own container readers; every
    /// other file is probed with ffprobe through CodeBrix.VideoProcessing.
    /// </summary>
    Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/MediaProbe.cs
public Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken)
{
    // ... null and File.Exists guards, each throwing VideoToolProcessingException ...

    var format = MediaFormats.Detect(path);
    if (format == MediaFormatKind.Unknown)
    {
        throw new VideoToolProcessingException(
            $"'{Path.GetFileName(path)}' is not a container this application recognises.");
    }

    return MediaFormats.IsCodeBrixContainer(format)
        ? Task.FromResult(ProbeCodeBrixContainer(path, format))
        : ProbeWithFfProbeAsync(path, format, cancellationToken);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public async Task<SourceMediaInfo> AddAsync(string path, CancellationToken cancellationToken)
{
    IsBusy = true;
    try
    {
        var existing = Library.FirstOrDefault(i =>
            string.Equals(i.Path, path, StringComparison.Ordinal));
        if (existing is not null)
        {
            SelectedItem = existing;
            StatusText = $"{existing.FileName} is already in the list.";
            return existing;
        }

        var info = await probe.ProbeAsync(path, cancellationToken);
        Library.Add(info);
        NotifyPropertyChanged(nameof(EmptyLibraryVisibility));
        SelectedItem = info;
        StatusText = $"Opened {info.FileName} - {info}";
        return info;
    }
    catch (VideoToolProcessingException exception)
    {
        StatusText = exception.Message;
        return null;
    }
    catch (OperationCanceledException)
    {
        StatusText = "Cancelled.";
        return null;
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/IMediaProbe.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/MediaProbe.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Two probing routes, and the class comment says why: an external prober cannot
  read a bespoke container at all and would see a constrained standard container
  as an ordinary one. Your own formats go to your own readers; everything else
  goes to the external tool.
- The external call is wrapped in a filtered catch for the library's own exception
  types and for I/O failures, with cancellation rethrown before it so a cancel is
  not reported as a probe failure. That filtered catch is also the whole of this
  application's behavior when the external tools are absent: there is no
  availability check anywhere. A missing tool surfaces as one of those exceptions,
  becomes the application's own exception, and lands in the status bar, while
  files read by the in-process readers keep opening.
- The probe refuses a file with no video track, and refuses one that states no
  duration, because progress could not then be reported - the progress design
  reaching back into the intake rules.
- The probe result doubles as the list item model, with badge, summary and
  playability as bindable derived properties.

### Detect a container from its first bytes

**When you want this.** Two different formats share a file extension and you have
to tell them apart the way the reader will.

**The MVVM shape.** A static method in the formats class, with no I/O in the view
model. It falls back to the extension when the file cannot be read.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
/// <remarks>
/// A <c>.cbv</c> file is Mode 2 when it starts with the ASCII bytes "CBVF" and Mode 1 when it
/// starts with the EBML magic. Nothing else about either file is consulted, which is exactly how
/// the playback core picks its reader.
/// </remarks>
public static MediaFormatKind Detect(string path)
{
    // ...
    var extension = Path.GetExtension(path).ToLowerInvariant();
    var sniffed = SniffSignature(path);

    if (extension == ".cbv")
    {
        return sniffed == MediaFormatKind.Unknown ? MediaFormatKind.Unknown : sniffed;
    }
    // ... .mkv, .webm, then ImportExtensions -> Mp4, else Unknown ...
}

private static MediaFormatKind SniffSignature(string path)
{
    try
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Span<byte> first = stackalloc byte[4];
        if (stream.Read(first) < 4)
        {
            return MediaFormatKind.Unknown;
        }

        if (CbvReader.IsCbv(first))
        {
            return MediaFormatKind.CodeBrixMode2;
        }

        return first.SequenceEqual(CbvFormat.EbmlMagic) ? MediaFormatKind.CodeBrixMode1 : MediaFormatKind.Unknown;
    }
    catch (IOException) { return MediaFormatKind.Unknown; }
    catch (UnauthorizedAccessException) { return MediaFormatKind.Unknown; }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`

**Sharp edges.**
- The playback library publishes both the test and the magic constant, so the
  application does not hard-code signature bytes.
- A file whose signature matches neither expectation is unknown and is refused,
  rather than being trusted because of its extension.

### Author a cbv file in either container mode from a settled plan

**When you want this.** You are writing CodeBrix video with the authoring library
and want to know which knobs correspond to which output.

**The MVVM shape.** All of it is in a service behind an interface; the view model
supplies a plan and a progress sink and never touches the authoring API. The plan
carries the destination; the runner turns the destination into a flavour, a
container, a cue policy and an audio codec.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
var request = new VideoAuthoringRequest
{
    SourcePath = sourcePath,
    OutputPath = plan.OutputPath,
    SourceDuration = plan.Source.Duration,
    TemporaryFolder = workingFolder,
    ChaptersPath = sidecars.ChaptersPath,
    CancellationToken = cancellationToken,

    //The bespoke CBVF container is written by the muxer in the playback core; the other
    //three are written by FFmpeg's own WebM and Matroska muxers.
    Flavour = plan.Destination == MediaFormatKind.CodeBrixMode2
        ? VideoAuthoringFlavour.Bespoke
        : VideoAuthoringFlavour.WebMProfile,

    Container = plan.Destination == MediaFormatKind.Matroska
        ? AuthoringContainerFormat.Matroska
        : AuthoringContainerFormat.WebM,

    //Only the two .cbv flavours are meant to satisfy the streamable profile. A standard MKV
    //is checked and reported on, but its failures are not this application's business.
    CuesToFront = plan.Destination != MediaFormatKind.Matroska,
    ValidateProfile = true,
    FailWhenProfileFails = MediaFormats.IsCodeBrixContainer(plan.Destination),
};

request.Video.FrameSize = plan.IsResized
    ? AuthoringFrameSize.Exact(plan.Resolution.Width, plan.Resolution.Height)
    : AuthoringFrameSize.Source;
request.Video.SpeedPreset = Av1SpeedPreset;
request.Video.ConstantRateFactor = Av1RateFactor(plan.Quality);

request.Audio.Include = plan.Source.HasAudio;
request.Audio.Codec = plan.AudioCodec == TargetAudioCodec.Vorbis
    ? AuthoringAudioCodec.LibVorbis
    : AuthoringAudioCodec.LibOpus;

foreach (var caption in sidecars.Captions)
{
    request.Captions.Add(new AuthoringCaptionInput(
        caption.Path, caption.Language, caption.Name, caption.Flags));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
public static TargetAudioCodec AudioCodecFor(MediaFormatKind kind) => kind switch
{
    MediaFormatKind.Mp4 => TargetAudioCodec.Aac,
    MediaFormatKind.Matroska => TargetAudioCodec.Opus,
    MediaFormatKind.WebM => TargetAudioCodec.Opus,
    MediaFormatKind.CodeBrixMode1 => TargetAudioCodec.Opus,

    //The hard invariant: a bespoke CBVF file this application writes carries Vorbis, never Opus.
    MediaFormatKind.CodeBrixMode2 => TargetAudioCodec.Vorbis,

    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no audio codec for an unrecognised format."),
};
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormatKind.cs`

**Sharp edges.**
- The two container modes, as the code names them. Mode 1 writes a `.cbv` that is
  a WebM constrained to the streamable profile - AV1 video, Opus audio, cues in
  front of the first cluster. Mode 2 writes a `.cbv` in the bespoke container -
  AV1 video, Vorbis audio, every index entry and every caption cue ahead of the
  media data. The codec table calls the Vorbis choice "the hard invariant".
- Audio sample rate is set per codec, and the reason is recorded in the code: one
  encoder's bit-rate mode opens only inside a band that depends on both the sample
  rate and the channel count, so the application uses its quality path instead;
  the other is always resampled to its own internal rate.
- The cue policy is on for everything except the plain standard container, and
  failing the profile check is fatal only for the two application formats - the
  standard container is checked and reported on but is expected to fail, and that
  failure is not an error.
- The authoring library is synchronous, so the pass runs on a worker thread with
  `Task.Run(() => CbvAuthor.Write(request), CancellationToken.None)` - note the
  `None`: cancellation reaches the library through the request's own token, not
  through `Task.Run`.
- The library takes captions and chapters only as files, which is why the sidecar
  step exists at all.

### Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library

**When you want this.** You want the FFmpeg argument-builder style - inputs,
stream selection, codecs, filters, progress and cancellation - from a service a
view model drives.

**The MVVM shape.** The same service and the same interface, with a different
private method chosen by the plan's destination. The command line that was run is
returned in the outcome for the record.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
var arguments = FFMpegArguments.FromFileInput(sourcePath);
foreach (var caption in sidecars.Captions)
{
    arguments = arguments.AddFileInput(caption.Path, false);
}

if (sidecars.HasChapters)
{
    arguments = arguments.AddFileInput(sidecars.ChaptersPath, false)
        .MapMetaData(sidecars.Captions.Count + 1);
}

var errors = new List<string>();
var processor = arguments
    .OutputToFile(plan.OutputPath, true, options =>
    {
        options.SelectStream(0, 0, Channel.Video);
        if (plan.Source.HasAudio)
        {
            options.SelectStream(0, 0, Channel.Audio);
        }

        for (var index = 0; index < sidecars.Captions.Count; index++)
        {
            options.SelectStream(0, index + 1, Channel.Subtitle);
            options.WithStreamMetadata(Channel.Subtitle, index, "language", sidecars.Captions[index].Language);
        }

        options
            .WithVideoCodec("libx264")
            .WithConstantRateFactor(H264RateFactor(plan.Quality))
            .WithSpeedPreset(Speed.Medium)
            .ForcePixelFormat("yuv420p");

        if (plan.IsResized)
        {
            options.WithVideoFilters(filters => filters.Scale(plan.Resolution.Width, plan.Resolution.Height));
        }

        // ... audio codec, bitrate, and the channel argument ...

        if (sidecars.Captions.Count > 0)
        {
            //MP4's own timed-text track. Nothing else in the MP4 family carries WebVTT.
            options.WithSubtitleCodec("mov_text");
        }

        options.WithFastStart().ForceFormat("mp4");
    })
    .NotifyOnProgress(
        percent => progress?.Report(new ConversionProgress(gerund, 2, 2, percent)),
        plan.Source.Duration)
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken);

var commands = new[] { "ffmpeg " + processor.Arguments };
var succeeded = await processor.ProcessAsynchronously(false).ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`

**Sharp edges.**
- Progress needs the source duration to turn a position into a percentage, which
  is why the probe refuses a file that states none.
- `ProcessAsynchronously(false)` is used throughout: false means "return a bool
  rather than throw", so the code checks the result and the output file itself and
  reports the last few error lines it collected.
- Cancellation is checked twice: the exception from the cancellable wrapper, and
  the token after the call returns. Both delete the part-written file.
- Where a destination must reduce channels it has to say so explicitly, because
  left to itself the encoder can refuse the layout outright and the export fails.
- Captions in this container need the container's own timed-text codec; nothing
  else in that family carries the caption format the sidecars are written in.
- Order matters in the option chain: the streaming-friendly flag before the forced
  format.

### Demultiplex a bespoke container and remux it so an external tool can read it

**When you want this.** Your own container holds perfectly ordinary elementary
streams, and you want to hand them to a tool that cannot open the container.

**The MVVM shape.** A service class the runner uses as its first stage; the view
model never knows it happened beyond one note in the run notes and one sentence in
the route line.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
using (var reader = MediaContainers.Open(source.Path))
{
    if (reader is not CbvReader) { /* ... refuse ... */ }

    var video = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Video)
        ?? throw new VideoToolProcessingException($"'{source.FileName}' carries no video track.");

    if (!string.Equals(video.CodecId, VideoCodecIds.Av1, StringComparison.OrdinalIgnoreCase))
    {
        throw new VideoToolProcessingException(
            $"'{source.FileName}' carries '{video.CodecId}' video; only AV1 can be re-wrapped into IVF.");
    }

    var audio = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Audio);

    using var ivf = IvfWriter.CreateAv1(ivfPath, video.Width, video.Height);
    var ogg = audio is null ? null : CreateAudioWriter(audio, oggPath, source.FileName);

    try
    {
        Demultiplex(reader, video.Id, audio?.Id ?? -1, ivf, ogg, cancellationToken,
            out videoFrames, out audioPackets);

        ivf.Complete();
        ogg?.Complete();
    }
    finally
    {
        ogg?.Dispose();
    }

    // ...
    sidecars = SidecarExtractor.ExtractFromReader(reader, workingFolder);
}

await RemuxAsync(ivfPath, hasAudio ? oggPath : null, intermediatePath, cancellationToken)
    .ConfigureAwait(false);
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
//One packet of lookahead on the audio side: an Ogg granule position says where a packet
//ENDS, and the next packet's timestamp is the most reliable statement of that.
byte[] pendingAudio = null;
// ...
while (reader.TryReadPacket(out var packet))
{
    cancellationToken.ThrowIfCancellationRequested();

    if (packet.TrackId == videoTrackId)
    {
        ivf.WriteFrame(packet.Data.Span, packet.Timestamp);
        videoFrames++;
        continue;
    }
    // ...
    //MediaPacket.Data is borrowed from the reader and is gone on the next read.
    pendingAudio = packet.Data.ToArray();
    pendingTimestamp = packet.Timestamp;
    pendingDuration = packet.Duration;
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
var succeeded = await arguments
    .OutputToFile(outputPath, true, options =>
    {
        options.SelectStream(0, 0, Channel.Video);
        if (oggPath is not null)
        {
            options.SelectStream(0, 1, Channel.Audio);
        }

        options.WithCopyCodec().ForceFormat("matroska");
    })
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extraction.cs`

**Sharp edges.**
- A packet's data is borrowed from the reader and is gone on the next read, so a
  packet held for lookahead must be copied.
- The lookahead exists because a granule position states where a packet ends, and
  the next packet's timestamp is the most reliable statement of that. The final
  packet falls back to its own duration, or to the reader's.
- The re-wrapping uses the playback library's own elementary-stream writers - the
  same two containers the authoring library writes when it builds a bespoke file,
  used here in the opposite direction.
- The remux copies codecs: nothing is decoded and nothing is re-encoded, so from
  the intermediate onwards the conversion is an ordinary one.
- The writers throw when the codec-private data is not what they expect; the
  extractor catches that and restates it as its own message.
- The reader is disposed only after the sidecars are taken out of it; both uses
  share the one open reader.

### Lift chapters and captions out of a source into sidecar files

**When you want this.** Your encoder takes captions and chapters only as separate
input files, and your sources carry them embedded.

**The MVVM shape.** A service with two routes - the container reader for the
formats you own, the external tool for the rest - producing one value the runner
hands on. Anything that could not be carried across becomes a sentence in the
notes, which reaches the operation panel.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs
public async Task<MediaSidecars> ExtractAsync(
    SourceMediaInfo source, string workingFolder, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(source);
    Directory.CreateDirectory(workingFolder);

    return MediaFormats.IsSupportedFormat(source.Format)
        ? ExtractFromContainerReader(source, workingFolder)
        : await ExtractWithFfmpegAsync(source, workingFolder, cancellationToken).ConfigureAwait(false);
}

private static MediaSidecars ExtractFromContainerReader(SourceMediaInfo source, string workingFolder)
{
    try
    {
        using var reader = MediaContainers.Open(source.Path);

        //A Matroska or WebM file interleaves its subtitle cues with the video, so the cues are
        //complete only once the file has been read through. The bespoke container keeps every
        //cue in its header, so its tracks are complete the instant it is open.
        if (reader.CaptionTracks.Count > 0 && reader.CaptionTracks.Any(t => !t.AreCuesComplete))
        {
            while (reader.TryReadPacket(out _))
            {
                //Draining for the cues; nothing is decoded and no packet is kept.
            }
        }

        return ExtractFromReader(reader, workingFolder);
    }
    catch (Exception exception) when (exception is not VideoToolProcessingException)
    {
        throw new VideoToolProcessingException(
            $"The chapters and captions in '{source.FileName}' could not be read: {exception.Message}", exception);
    }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs
private static readonly string[] TextCaptionCodecs =
[
    "subrip", "srt", "webvtt", "ass", "ssa", "mov_text", "text", "eia_608", "subviewer",
];

// ... in the ffmpeg route, per subtitle stream:
if (!TextCaptionCodecs.Contains(codec, StringComparer.OrdinalIgnoreCase))
{
    notes.Add($"Caption track {index} is '{codec}', which has no text form, so it was not carried across.");
    continue;
}

var succeeded = await FFMpegArguments
    .FromFileInput(source.Path)
    .OutputToFile(path, true, options => options
        .SelectStream(index, 0, Channel.Subtitle)
        .WithSubtitleCodec("webvtt")
        .ForceFormat("webvtt"))
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/MediaSidecars.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/WebVttFile.cs`

**Sharp edges.**
- A container that interleaves its subtitle cues with the video is complete only
  after the whole file has been read through; one that keeps every cue in its
  header is complete the instant it is open. Drain the reader only when a track
  says its cues are incomplete.
- An image-based caption track has no text form at all, so report it in a note
  rather than losing it silently.
- Where the application supports one title language, chapters are collapsed to one
  untagged title each, and the note counts distinct languages dropped across the
  whole file rather than chapters.
- An external tool sometimes reports a placeholder string as a chapter title;
  treat that like an empty title and substitute a generated one.
- The playback library reads the caption format and formats its timestamps but
  publishes no writer, so this application brings a small one, preserving cue
  identifiers and settings because both destinations that can carry them do.

### Build a resolution ladder keyed on the short side with even dimensions

**When you want this.** You are offering downscale choices and want them to read
correctly for portrait video as well as landscape.

**The MVVM shape.** A static builder returning rows the view model just copies
into an observable collection.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionLadder.cs
public static IReadOnlyList<int> StandardShortSides { get; } = [1440, 1080, 720, 480];

public static IReadOnlyList<ResolutionOption> Build(int sourceWidth, int sourceHeight)
{
    // ... positive-dimension guards ...

    var rungs = new List<ResolutionOption>
    {
        ResolutionOption.Original(MakeEven(sourceWidth), MakeEven(sourceHeight)),
    };

    //The rung names the SHORT side, so a portrait source is measured across its width and a
    //landscape one across its height - which is what height keying did for every landscape source,
    //and is why landscape ladders are unchanged.
    var sourceShortSide = Math.Min(sourceWidth, sourceHeight);
    var sourceLongSide = Math.Max(sourceWidth, sourceHeight);
    var isPortrait = sourceWidth < sourceHeight;

    foreach (var shortSide in StandardShortSides)
    {
        //Strictly below: a source whose short side is already 1080 is not offered "1080p".
        if (shortSide >= sourceShortSide)
        {
            continue;
        }

        var keyed = MakeEven(shortSide);
        var other = ProportionalOtherSide(sourceShortSide, sourceLongSide, shortSide);

        rungs.Add(ResolutionOption.Reduced(
            shortSide + "p",
            isPortrait ? keyed : other,
            isPortrait ? other : keyed));
    }

    return rungs;
}

/// <summary>Rounds a dimension to the nearest even number of pixels, never below 2.</summary>
public static int MakeEven(int value)
{
    if (value <= 2)
    {
        return 2;
    }

    return (value % 2 == 0) ? value : value + 1;
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionLadder.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionOption.cs`

**Sharp edges.**
- A rung's number names the short side, which is the industry convention; the
  comment gives the case it fixes, where a portrait phone clip would otherwise be
  offered a rung far narrower than intended.
- Every dimension is even, because the chroma planes of the pixel format in use
  are half-size in each direction and an odd dimension has no representation in
  it; the evening is applied to the source's own size too, so even the "original"
  rung is safe.
- Strictly below: a source already at a standard short side is not offered that
  rung.

### Move one encoder knob and pin everything else

**When you want this.** You are offering a quality choice and want it to mean one
thing, comparably, across two different encoders.

**The MVVM shape.** An enum of stops offered from a static list, a single
view-model property with a default, and two private mapping functions in the
service that turn a stop into a rate factor.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
//Faster than the authoring library's own default of 6, which matters a great deal for an
//application a person is sitting in front of, and costs very little at these bit rates. It is
//PINNED: the quality knob moves the rate factor only, so an encode takes about as long whichever
//stop is chosen.
private const int Av1SpeedPreset = 8;

//THE QUALITY KNOB, IN ITS ENTIRETY. A quality stop moves the encoder's constant rate factor and
//nothing else: the speed presets above stay pinned, and sound is settled by the destination alone.
// ... a calibration table, elided ...
private static int Av1RateFactor(QualityLevel quality) => quality switch
{
    QualityLevel.Fair => 42,
    QualityLevel.Better => 24,
    QualityLevel.Best => 18,
    _ => 30,
};

private static int H264RateFactor(QualityLevel quality) => quality switch
{
    QualityLevel.Fair => 27,
    QualityLevel.Better => 17,
    QualityLevel.Best => 14,
    _ => 20,
};
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<!-- The quality stop. Its rows are the QualityLevel values themselves, whose own
     names are the words to show, so this drop-down carries no item template. -->
<StackPanel Grid.Column="4">
    <TextBlock Text="Quality" Style="{StaticResource FieldLabel}" />
    <ComboBox x:Name="QualityBox"
              HorizontalAlignment="Stretch"
              PlaceholderText="Choose a quality"
              ItemsSource="{d:Binding Conversion.QualityLevels}"
              SelectedItem="{d:Binding Conversion.SelectedQuality, Mode=TwoWay}" />
</StackPanel>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/QualityLevel.cs`

**Sharp edges.**
- Both speed presets are pinned, so an encode takes about as long whichever stop
  is chosen, and the knob means one thing.
- The two rate-factor sets were chosen to match each other stop for stop rather
  than to look tidy on either encoder's own scale, so picking a stop gives the
  same picture whichever destination is chosen. One encoder's scale moves about
  half as far as the other's in this band, which is why its steps are smaller.
- The comment records how the numbers were arrived at: lossless masters of
  synthetic sources, encoded at every candidate rate factor with the presets
  pinned, compared against their masters through the tool's own quality filters,
  with the inputs re-timestamped by frame index first because a one-frame slip
  swamps everything a rate factor does. Nothing was installed to measure it.
- Sound is never touched by the quality knob; it is settled by the destination
  alone.
- The drop-down carries no item template, because the enum's own names are the
  words to show.

### Download run scoped media into a self cleaning temp cache

**When you want this.** You must fetch many remote files for one operation, the
URLs are short-lived, and nothing should be left on disk afterwards.

**The MVVM shape.** A disposable cache object created inside the service method
with `using`, so its temp folder disappears when the operation ends. Failures
return a result object with a reason instead of throwing, so one bad file cannot
fail the whole job; only a user cancellation is rethrown.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs
/// <summary>
/// Downloads every referenced media file once per run into a private temp folder,
/// and deletes the folder at the end. Notion's uploaded-file URLs are pre-signed
/// and expire in about an hour, so downloads always happen in the same run that
/// fetched the block tree — cached URLs are never persisted or reused later.
/// </summary>
internal sealed class MediaCache : IDisposable
{
    /// <summary>Media larger than this is not downloaded (a card is rendered instead).</summary>
    public const long DefaultMaxDownloadBytes = 100L * 1024 * 1024;

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(3) };
    private readonly Dictionary<string, CachedMedia> _byUrl = new(StringComparer.Ordinal);
    // ...

    /// <summary>The temp folder holding this run's downloads (deleted on dispose).</summary>
    public string CacheDirectory { get; } =
        Path.Combine(Path.GetTempPath(), "NotionDocumentCreator", Guid.NewGuid().ToString("N"));

    public async Task<CachedMedia> FetchAsync(
        string url, long maxBytes = DefaultMaxDownloadBytes, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (string.IsNullOrWhiteSpace(url))
        {
            return CachedMedia.Failed("No URL was supplied for the media file.");
        }
        if (_byUrl.TryGetValue(url, out var cached)) { return cached; }

        var result = await DownloadAsync(url, maxBytes, cancellationToken).ConfigureAwait(false);
        _byUrl[url] = result;
        return result;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs
while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
{
    total += read;
    if (total > maxBytes)
    {
        //The server did not declare a length — enforce the cap while streaming
        target.Close();
        File.Delete(filePath);
        return CachedMedia.Failed("File exceeded the download cap.");
    }
    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
}
// ...
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    throw; //A user cancel should cancel the run, not become a warning
}
catch (Exception ex)
{
    return CachedMedia.Failed($"Download failed: {ex.Message}");
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaPreparer.cs`

**Sharp edges.**
- The size cap is enforced twice: against the declared content length, and again
  while streaming for servers that declare none. Reading only the response headers
  first is what lets the cap be checked before the body is buffered.
- Disposal swallows a failure to delete the folder: a locked temp file must not
  crash disposal, and the operating system's temp cleaner will get it.
- Results are cached per URL, so the same picture used on several pages is fetched
  once; the test asserts the two calls return the same instance.
- The download pass runs before rendering precisely so the renderer can be
  synchronous and look results up by key.

### Extract a video poster frame and degrade when the external tool is missing

**When you want this.** You want a still image from a video, or a media duration,
using tools that may not be installed on the user's machine.

**The MVVM shape.** A static helper in the library wraps every call so a missing
tool, an unreadable codec or a timeout yields null; the caller turns null into a
rendered card plus one warning, never a failure.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/VideoPosterExtractor.cs
using CodeBrix.VideoProcessing;

/// <summary>
/// Extracts a poster frame from a downloaded video via ffmpeg, and probes media
/// durations via ffprobe. Every path is wrapped so a missing ffmpeg, an
/// unreadable codec or a timeout produces a null result (the caller renders a
/// media card plus one warning) — never a failed document.
/// </summary>
internal static class VideoPosterExtractor
{
    /// <summary>Probes the duration of a media file; null when ffprobe cannot say.</summary>
    public static TimeSpan? TryProbeDuration(string mediaFilePath)
    {
        try
        {
            var analysis = FFProbe.Analyse(mediaFilePath);
            var duration = analysis?.Duration ?? TimeSpan.Zero;
            return duration > TimeSpan.Zero ? duration : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Grabs a poster frame — at 10% of the duration, or one second in for very
    /// short clips — and returns the PNG bytes, or null when extraction fails.
    /// </summary>
    public static byte[] TryExtractPoster(string videoFilePath, string workDirectory, out TimeSpan? duration)
    {
        duration = TryProbeDuration(videoFilePath);
        try
        {
            var captureAt = duration is { } known && known > TimeSpan.FromSeconds(10)
                ? TimeSpan.FromTicks(known.Ticks / 10)
                : TimeSpan.FromSeconds(1);
            if (duration is { } total && captureAt >= total)
            {
                captureAt = TimeSpan.FromTicks(total.Ticks / 2);
            }

            Directory.CreateDirectory(workDirectory);
            var posterPath = Path.Combine(workDirectory, $"poster-{Guid.NewGuid():N}.png");
            try
            {
                if (!FFMpeg.Snapshot(videoFilePath, posterPath, size: null, captureTime: captureAt))
                {
                    return null;
                }
                return File.Exists(posterPath) ? File.ReadAllBytes(posterPath) : null;
            }
            finally
            {
                if (File.Exists(posterPath)) { File.Delete(posterPath); }
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/VideoPosterExtractor.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaPreparer.cs`
`NotionDocumentCreator/THIRD-PARTY-NOTICES.txt`

**Sharp edges.**
- The external tools are the host's, not bundled; the notices file says so and the
  whole class exists to make their absence a non-event.
- Even when the poster fails, the probed duration is kept and printed on the media
  card.
- The temporary poster file is deleted in a `finally`, inside the cache folder
  that is itself deleted at the end of the run.
- Audio blocks never attempt a poster; they only probe a duration.

### Enumerate cameras and start a live capture session

**When you want this.** A camera dropdown that populates itself at startup, starts
the first camera automatically, and switches cleanly when the user picks another.

**The MVVM shape.** A capture service class wraps the webcam library and exposes a
small surface: a static discovery method, start, stop, a "has a frame" flag, a
copy-latest-frame method and a frame-arrived event. The view model owns the
service, holds the devices in an observable collection, and switches cameras from
the selected-item setter. Discovery is async and its results are marshalled onto
the UI thread.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Webcam/WebcamCaptureService.cs
public static async Task<IReadOnlyList<CameraDevice>> GetCamerasAsync()
{
    IReadOnlyList<IImagingMediaDevice> devices = await WebcamDevices.GetImagingMediaDeviceListAsync();
    var cameras = new List<CameraDevice>();
    foreach (IImagingMediaDevice device in devices)
    {
        cameras.Add(new CameraDevice(device));
    }
    return cameras;
}

public void Start(CameraDevice camera)
{
    if (camera == null) { throw new ArgumentNullException(nameof(camera)); }

    Stop();

    _session = new WebcamSession(camera.Device);
    _session.FrameReceived += OnFrameReceived;
    _session.Start();
}

private void OnFrameReceived(object sender, WebcamFrameEventArgs frame)
{
    //Capture-thread context: the session caches the pixels itself (see TryCopyLatestFrame);
    //  we only note that a frame exists and get out fast.
    _hasFrame = true;
    FrameArrived?.Invoke(this, EventArgs.Empty);
}

public bool TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
{
    WebcamSession session = _session;
    if (session == null)
    {
        width = 0;
        height = 0;
        return false;
    }
    return session.TryCopyLatestFrame(ref buffer, out width, out height);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private async Task InitializeAsync()
{
    try
    {
        var cameras = await WebcamCaptureService.GetCamerasAsync();
        InvokeOnMainThread(() =>
        {
            Cameras.Clear();
            foreach (var camera in cameras)
            {
                Cameras.Add(camera);
            }
            if (Cameras.Count == 0)
            {
                StatusText = "No cameras were found on this machine.";
            }
            else
            {
                StatusText = $"Found {Cameras.Count} camera(s).";
                SelectedCamera = Cameras[0]; //auto-start on the first camera
            }
        });
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Camera discovery failed: {e.Message}");
    }
}

// ...

private void SwitchCamera(CameraDevice camera)
{
    try
    {
        HasFrame = false;
        if (camera == null)
        {
            _captureService.Stop();
            InvalidatePreviewCanvas?.Invoke();
            return;
        }

        _captureService.Start(camera);
        StatusText = $"Live: {camera.FriendlyName}";
    }
    catch (Exception e)
    {
        StatusText = $"Could not start '{camera?.FriendlyName}': {e.Message}";
    }
}
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
<ComboBox MinWidth="280" VerticalAlignment="Center"
          ItemsSource="{d:Binding Cameras}"
          SelectedItem="{d:Binding SelectedCamera, Mode=TwoWay}"
          IsEnabled="{d:Binding IsCameraMode}" />
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Webcam/WebcamCaptureService.cs`
`PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The frame event fires on the capture thread; the documentation says so, and
  every handler is written to get out fast and marshal its own UI work.
- Start calls stop first, so switching cameras never leaves two sessions running,
  and stop unsubscribes before disposing and clears the "has a frame" flag so a
  stale frame from the previous camera cannot be drawn.
- That flag is `volatile`, because it is written on the capture thread and read
  from the UI thread.
- The service does not cache pixels itself; the underlying session does, and the
  copy method forwards to it with a caller-owned buffer reallocated only when the
  size changes.
- Enumeration is a static method and works with no session running and no camera
  present, so it is safe to call at startup, and an empty device list is a normal
  state rather than an error.
- Discovery is kicked off from the constructor as a discarded task after setting a
  "discovering" status, and every failure path writes to the same status line
  rather than throwing into the constructor.

### Wrap a device library type so the view model never sees it

**When you want this.** You want the dropdown to bind to a plain object with a
display name, and to be free to change the capture library later without touching
the view model or the XAML.

**The MVVM shape.** A small sealed wrapper with an `internal` constructor and an
`internal` property holding the real device. Everything the UI needs is public;
everything the library needs is internal.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs
/// <summary>
/// One connected camera, as shown in the camera-selection dropdown. Wraps the discovered
/// device so consumers of this library never handle CodeBrix.Webcam types directly.
/// </summary>
public sealed class CameraDevice
{
    internal CameraDevice(IImagingMediaDevice device)
    {
        Device = device;
    }

    internal IImagingMediaDevice Device { get; }

    /// <summary>The camera's unique hardware identifier.</summary>
    public string Id => Device.Id;

    /// <summary>The camera's human-readable name.</summary>
    public string FriendlyName => Device.FriendlyName;

    /// <summary>The dropdown display text.</summary>
    public override string ToString() => Device.FriendlyName;
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraDevice.cs`

**Sharp edges.**
- `ToString()` is what a `ComboBox` displays, so no item template and no
  display-member binding are needed. There is a test asserting the identity, to
  keep it that way.
- The internal constructor means only the library can mint one, so a device
  instance in the view model always came from real enumeration.
- The same shape serves tracking results: public read-only properties, internal
  constructors.

### Run a TFLite model through the OpenCV DNN module

**When you want this.** You have a model file and want to run it from a
CodeBrix.Platform application without an extra inference runtime, using the OpenCV
managed binding the application already carries.

**The MVVM shape.** An `internal` class per model, constructed from the model
bytes, holding the network and its reusable buffers, exposing one method that
takes a frame and returns a plain result object. The pipeline owns them; nothing
above the library sees OpenCV types.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs
internal PalmDetector(byte[] modelBytes)
{
    _net = Cv2.Dnn.ReadNetFromTFLite(modelBytes);
}

// ...

//Letterbox the frame into the model's square input
float scale = (float)InputSize / Math.Max(bgrFrame.Width, bgrFrame.Height);
// ...
_letterboxed.SetTo(Scalar.All(0));
Cv2.Resize(bgrFrame, _resized, new Size(scaledW, scaledH));
using (var window = new Mat(_letterboxed, new Rect(padX, padY, scaledW, scaledH)))
{
    _resized.CopyTo(window);
}

using Mat blob = Cv2.Dnn.BlobFromImage(_letterboxed, 1.0 / 255,
    new Size(InputSize, InputSize), new Scalar(0, 0, 0), swapRB: true, crop: false);
_net.SetInput(blob);

//Identity_1 = per-anchor score logits; Identity = per-anchor box+keypoint offsets.
//Read them with separate single-name forwards (not ForwardAll) so the no-hand case
//  below can early-out before ever reading the far larger box tensor.
float[] rawScores;
using (Mat scores = _net.Forward("Identity_1"))
{
    rawScores = scores.ToArray<float>();
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/HandLandmarker.cs
//Identity = 21 x (x, y, z) in crop pixels; Identity_1 = presence probability. Both
//  outputs are always needed, so read them in one pass with ForwardAll (the second
//  read reuses the first forward's results).
float[] rawLandmarks;
float presence;
Mat[] outputs = _net.ForwardAll("Identity", "Identity_1");
try
{
    rawLandmarks = outputs[0].ToArray<float>();
    presence = outputs[1].ToArray<float>()[0];
}
finally
{
    foreach (Mat output in outputs) { output.Dispose(); }
}
```

The anchor grid the model's outputs are relative to is not in the file and has to
be regenerated exactly:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs
static PalmDetector()
{
    //Anchor grids: stride 8 -> 24x24 cells x 2 anchors; stride 16 -> 12x12 cells x 6
    var anchorsX = new float[2016];
    var anchorsY = new float[2016];
    var index = 0;
    foreach (var (gridSize, perCell) in new[] { (24, 2), (12, 6) })
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int n = 0; n < perCell; n++)
                {
                    anchorsX[index] = (x + 0.5f) / gridSize;
                    anchorsY[index] = (y + 0.5f) / gridSize;
                    index++;
                }
            }
        }
    }
    AnchorsX = anchorsX;
    AnchorsY = anchorsY;
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
if (_bgraMat == null || _bgraMat.Width != width || _bgraMat.Height != height)
{
    _bgraMat?.Dispose();
    _bgraMat = new Mat(height, width, MatType.CV_8UC4);
    _bgrMat?.Dispose();
    _bgrMat = new Mat();
}
Marshal.Copy(bgraPixels, 0, _bgraMat.Data, width * height * 4);
Cv2.CvtColor(_bgraMat, _bgrMat, ColorConversionCodes.BGRA2BGR);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs` and
`Internal/HandLandmarker.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs`

**Sharp edges.**
- Reading the network from a byte array is what makes the embedded-resource
  approach work with no temp file.
- Output tensors are addressed by name, and the code records a deliberate choice
  between the two read styles: separate single-output calls when an early-out can
  avoid reading a large tensor at all, and the read-all call when every output is
  needed, because the second read reuses the first forward's results.
- Every result matrix is disposed - `using` for the single reads, a `finally` loop
  for the read-all.
- Frames arrive in one channel order from the camera and the models want another:
  convert once per frame with cached matrices rather than allocating.
- Preprocessing has to match the model exactly: letterbox into the square input,
  fill with zeros, center the scaled frame, then build the blob with the model's
  own scaling and channel-swap settings.
- Decoding the raw output is the application's job, not the binding's: regenerate
  the fixed anchor grid, apply a sigmoid to score logits (but not to an output
  that is already a probability), run your own suppression on overlapping boxes,
  and convert survivors back out of letterboxed space into original frame pixels.
  Doing that arithmetic in small internal static methods is what makes it
  unit-testable without a model.
- Run all of this on a worker thread with a latest-frame-wins buffer; see the
  view-model area.

### Warp a rotated region of interest into a model input

**When you want this.** A detector gave you a rotated box and the second-stage
model wants an upright square crop.

**The MVVM shape.** A second internal class taking model bytes, with one inference
method that returns landmarks already projected back into original frame pixels,
so callers never see crop-space coordinates.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/Internal/HandLandmarker.cs
float cos = (float)Math.Cos(roi.RotationRadians);
float sin = (float)Math.Sin(roi.RotationRadians);
float half = roi.RoiSize / 2f;

Point2f Corner(float offsetX, float offsetY) => new Point2f(
    roi.RoiCenterX + ((offsetX * cos) - (offsetY * sin)),
    roi.RoiCenterY + ((offsetX * sin) + (offsetY * cos)));

Point2f[] source = { Corner(-half, -half), Corner(half, -half), Corner(-half, half) };
Point2f[] destination =
{
    new Point2f(0, 0),
    new Point2f(InputSize, 0),
    new Point2f(0, InputSize),
};

using (Mat affine = Cv2.GetAffineTransform(source, destination))
{
    Cv2.WarpAffine(bgrFrame, _crop, affine, new Size(InputSize, InputSize));
}

using Mat blob = Cv2.Dnn.BlobFromImage(_crop, 1.0 / 255,
    new Size(InputSize, InputSize), new Scalar(0, 0, 0), swapRB: true, crop: false);
_net.SetInput(blob);

//Identity = 21 x (x, y, z) in crop pixels; Identity_1 = presence probability. Both
//  outputs are always needed, so read them in one pass with ForwardAll (the second
//  read reuses the first forward's results).
Mat[] outputs = _net.ForwardAll("Identity", "Identity_1");
try
{
    rawLandmarks = outputs[0].ToArray<float>();
    presence = outputs[1].ToArray<float>()[0];
}
finally
{
    foreach (Mat output in outputs) { output.Dispose(); }
}

//Project crop-space landmarks back into frame pixels through the same rotation
var imageLandmarks = new Point2f[21];
for (int i = 0; i < 21; i++)
{
    float normX = (rawLandmarks[i * 3] / InputSize) - 0.5f;
    float normY = (rawLandmarks[(i * 3) + 1] / InputSize) - 0.5f;
    imageLandmarks[i] = new Point2f(
        roi.RoiCenterX + (((normX * cos) - (normY * sin)) * roi.RoiSize),
        roi.RoiCenterY + (((normX * sin) + (normY * cos)) * roi.RoiSize));
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/HandLandmarker.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs`

**Sharp edges.**
- The presence output is already a probability; the field's documentation says
  "do not sigmoid". Applying one a second time is a real trap when the sibling
  model's scores do need one.
- The affine transform needs exactly three corners, and the same rotation must be
  reused in reverse to project results back - do not recompute it from the matrix.
- Landmarks come back as triples in crop pixels; only two of the three are used,
  hence the stride.
- The array returned by the read-all call is owned by the caller: dispose every
  element in a `finally`.
- The detector's own rectangle transformation - shift half a box along the rotated
  axis, expand by a fixed factor, then undo the letterbox padding and scale -
  belongs with the detector, so this class receives a region already in frame
  pixels.

### Recognize a gesture from landmark geometry instead of a model

**When you want this.** The classification model in your bundle will not import,
or you want a fast, explainable, testable rule instead of a black box.

**The MVVM shape.** A pure `internal static` class over an array of points. No
network, no state, no allocation, trivially unit tested.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/OpenPalmClassifier.cs
/// <summary>
/// Decides geometrically whether 21 hand landmarks show an open palm - the gesture that
/// draws the visualization toward the hand. The bundled MediaPipe gesture-classifier models
/// cannot run through OpenCV's TFLite importer, but they are not needed: an open palm is
/// simply a hand whose four fingers are extended, and extension falls straight out of the
/// landmark geometry (each fingertip is farther from the wrist than that finger's middle
/// joint). A curled finger folds back toward the wrist, so its tip/joint ratio drops below 1.
/// </summary>
internal static class OpenPalmClassifier
{
    /// <summary>
    /// How much farther from the wrist a fingertip must be than its PIP joint (as a ratio)
    /// to count as extended. Raise toward 1.3 to demand flatter hands; lower toward 1.0 to
    /// accept slightly cupped hands.
    /// </summary>
    internal const float ExtendedRatio = 1.1f;

    //MediaPipe hand-landmark topology: 0 = wrist; each finger runs MCP -> PIP -> DIP -> TIP
    //  (index 5-8, middle 9-12, ring 13-16, pinky 17-20; thumb 1-4)
    private static readonly (int Tip, int Pip)[] Fingers = { (8, 6), (12, 10), (16, 14), (20, 18) };

    internal static bool IsOpenPalm(Point2f[] landmarks)
    {
        if (landmarks == null || landmarks.Length < 21) { return false; }

        Point2f wrist = landmarks[0];
        foreach ((int tip, int pip) in Fingers)
        {
            if (Distance(landmarks[tip], wrist) <= Distance(landmarks[pip], wrist) * ExtendedRatio)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// The palm's center: the mean of the wrist and the four finger MCP knuckles.
    /// </summary>
    internal static Point2f GetPalmCenter(Point2f[] landmarks)
    {
        var sumX = 0f;
        var sumY = 0f;
        foreach (int i in new[] { 0, 5, 9, 13, 17 })
        {
            sumX += landmarks[i].X;
            sumY += landmarks[i].Y;
        }
        return new Point2f(sumX / 5f, sumY / 5f);
    }
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/OpenPalmClassifier.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/OpenPalmClassifier.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/OpenPalmClassifierTests.cs`

**Sharp edges.**
- The reason is recorded in two places, the class comment and the project file's
  own comment: the classifier stages of the upstream bundle use an operator the
  importer does not support, so those stages are deliberately not embedded.
- The thumb is excluded from the four-finger test; its geometry does not follow
  the same rule.
- The tuning constant is documented with the direction to move it and what that
  trades away.
- The rule is scale- and rotation-free because it compares two distances from the
  same point, so it works in any consistent coordinate space - which is exactly
  what the tests exploit with synthetic inputs.

### Track multiple detections across frames with stable ids

**When you want this.** A per-frame detector gives you unordered results, and
downstream animation needs to know that this frame's item is the same physical
thing as last frame's.

**The MVVM shape.** Keep the track list as worker-thread-only state inside the
pipeline class. Match this frame's candidates against last frame's tracks by
nearest neighbor, closest pairs first, with a maximum distance; smooth each
track's position; report in a stable order.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
/// <summary>The most palms tracked at once (the palm detector examines the whole frame each time).</summary>
public const int MaxPalms = 4;

/// <summary>The minimum landmark-model presence confidence for a hand to count as present.</summary>
public const float PresenceThreshold = 0.5f;

/// <summary>
/// The exponential-moving-average factor for each palm's position (1 = no smoothing,
/// smaller = smoother but laggier tracking).
/// </summary>
public const float SmoothingAlpha = 0.5f;

/// <summary>
/// How far (normalized, relative to the frame) a palm may move between consecutive
/// frames and still be recognized as the same hand.
/// </summary>
public const float TrackMatchMaxDistance = 0.25f;
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
//Match this frame's palms to the tracks from the previous frame: greedy
//  nearest-neighbor, closest pairs first, so each physical hand keeps its id
int[] trackForCandidate = MatchCandidatesToTracks(candidates);

var survivingTracks = new List<PalmTrack>(candidates.Count);
var palms = new List<TrackedPalm>(candidates.Count);
for (int c = 0; c < candidates.Count; c++)
{
    var candidate = candidates[c];
    PalmTrack track;
    if (trackForCandidate[c] >= 0)
    {
        track = _tracks[trackForCandidate[c]];
        track.SmoothedX += (candidate.X - track.SmoothedX) * SmoothingAlpha;
        track.SmoothedY += (candidate.Y - track.SmoothedY) * SmoothingAlpha;
    }
    else
    {
        track = new PalmTrack { Id = _nextTrackId++, SmoothedX = candidate.X, SmoothedY = candidate.Y };
    }
    survivingTracks.Add(track);
    palms.Add(new TrackedPalm(track.Id, candidate.IsOpen,
        track.SmoothedX, track.SmoothedY, candidate.DetectionScore, candidate.PresenceScore));
}

//Tracks that matched nothing this frame are dropped (their hands left the view)
_tracks.Clear();
_tracks.AddRange(survivingTracks);

//Report in stable track order so consumers see a consistent sequence
palms.Sort((a, b) => a.TrackId.CompareTo(b.TrackId));
return new PalmTrackingResult(palms);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/PalmTrackerTests.cs`

**Sharp edges.**
- The matcher builds every candidate-and-track pair within the distance limit,
  sorts by distance, then assigns greedily, skipping pairs whose candidate or
  track is already taken. That is a few lines and avoids the mis-assignment a
  naive first-match loop produces when two items cross.
- A track that matches nothing this frame is dropped, and an item that leaves and
  returns gets a new id. The result type documents that, and the renderer's slot
  logic is designed around it.
- Results are sorted by id before being reported, so consumers can rely on a
  consistent order.
- Every tuning constant is a documented public constant on the pipeline class
  rather than a literal buried in the loop, with the direction to move it.
- The frame-level early-out matters: when nothing survives the threshold, the
  track list is cleared and a shared empty result is returned, so the
  "everything gone" event still fires and subscribers release their state.

### Smooth a noisy sensor position before it drives the UI

**When you want this.** Raw per-frame positions jitter and the jitter is visible
in whatever they drive.

**The MVVM shape.** The smoothing lives with the producer, not the consumer, so
every consumer gets the same smoothed value and the smoothing state resets
whenever tracking is lost.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
/// <summary>
/// The exponential-moving-average factor for the palm position (1 = no smoothing,
/// smaller = smoother but laggier brush).
/// </summary>
public const float SmoothingAlpha = 0.5f;
// ...
Point2f palmCenter = OpenPalmClassifier.GetPalmCenter(inference.ImageLandmarks);
float normX = Math.Clamp(palmCenter.X / width, 0f, 1f);
float normY = Math.Clamp(palmCenter.Y / height, 0f, 1f);

if (_hasSmoothed)
{
    _smoothedX += (normX - _smoothedX) * SmoothingAlpha;
    _smoothedY += (normY - _smoothedY) * SmoothingAlpha;
}
else
{
    _smoothedX = normX;
    _smoothedY = normY;
    _hasSmoothed = true;
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Sharp edges.**
- The "have we smoothed yet" flag is cleared on every empty result and on stop, so
  the next detection snaps to the true position instead of gliding in from the
  last one.
- Normalization and clamping happen before smoothing, so the smoothed value is
  always a valid normalized coordinate.
- Two thresholds gate the result before smoothing runs: the detector's own score
  threshold and the second-stage model's presence threshold.

