using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Platform.Foundation.Extensibility;
using CodeBrix.Platform.Media.Playback;
using CodeBrix.Samples.PlayTests;
using MediaPlayerDemo.ViewModels;
using MediaPlayerDemo.Views;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MediaPlayerDemo.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public const string DefaultAddress =
        "https://mdn.github.io/learning-area/html/multimedia-and-embedding/video-and-audio-content/rabbit320.mp4";

    public MediaEngineFixture Engine { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;

    // The project keeps the libVLC add-in out of the generated registrations, so this recording engine is the only
    // one: nothing is streamed and nothing is played.
    protected override void Prepare() =>
        ApiExtensibility.Register<MediaPlayer>(typeof(IMediaPlayerExtension), player => new MediaEngineFake(player, Engine));

    protected override Application CreateApplication() => new App();

    protected override Task BeforeResetAsync() => Application.EvaluateAsync(() => Engine.Loaded.Clear());
}

public sealed class MediaEngineFixture
{
    // Read and written on the UI thread only.
    public List<(Uri Address, bool AutoPlay)> Loaded { get; } = new();
}

public sealed class MediaEngineFake(MediaPlayer player, MediaEngineFixture fixture) : IMediaPlayerExtension
{
    public IMediaPlayerEventsExtension Events { get; set; }
    public double PlaybackRate { get; set; } = 1.0;
    public bool IsLoopingEnabled { get; set; }
    public bool IsLoopingAllEnabled { get; set; }
    public MediaPlayerState CurrentState => MediaPlayerState.Closed;
    public TimeSpan NaturalDuration => TimeSpan.Zero;
    public bool IsProtected => false;
    public double BufferingProgress => 0.0;
    public bool CanPause => false;
    public bool CanSeek => false;
    public MediaPlayerAudioDeviceType AudioDeviceType { get; set; }
    public MediaPlayerAudioCategory AudioCategory { get; set; }
    public TimeSpan TimelineControllerPositionOffset { get; set; }
    public bool RealTimePlayback { get; set; }
    public double AudioBalance { get; set; }
    public TimeSpan Position { get; set; }
    public bool? IsVideo => null;

    public void InitializeSource()
    {
        if (player.Source is MediaSource source) fixture.Loaded.Add((source.Uri, player.AutoPlay));
    }

    public void SetTransportControlsBounds(Rect bounds) { }
    public void SetUriSource(Uri value) { }
    public void SetFileSource(IStorageFile file) { }
    public void SetStreamSource(IRandomAccessStream stream) { }
    public void SetMediaSource(IMediaSource source) { }
    public void StepForwardOneFrame() { }
    public void StepBackwardOneFrame() { }
    public void SetSurfaceSize(Size size) { }
    public void Play() { }
    public void Pause() { }
    public void Stop() { }
    public void ToggleMute() { }
    public void OnVolumeChanged() { }
    public void Initialize() { }
    public void OnOptionChanged(string name, object value) { }
    public void PreviousTrack() { }
    public void NextTrack() { }
    public void Dispose() { }
}
