using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.PlayTest;
using GameEngineMusicDemo.Game;
using GameEngineMusicDemo.ViewModels;
using GameEngineMusicDemo.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace GameEngineMusicDemo.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    // The demo's own defaults: what each slider shows when the page is first built.
    private static readonly (string Name, double Value)[] SliderDefaults =
    {
        ("MasterSlider", 100), ("MusicSlider", 100), ("SfxSlider", 100),
        ("PadSlider", 100), ("BassSlider", 0), ("LeadSlider", 0), ("SpeedSlider", 100),
    };

    private float[] _stemGains;
    private float[] _songStemGains;
    private float _harmonyVolume;

    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public IManageGameCanvas Model => (IManageGameCanvas)View.DataContext;
    public GameEngineMusicDemoGame Demo => Model.Demo;
    public GameEngineMusicDemoGame FirstDemo { get; private set; }
    public GameSurfaceCanvas Canvas => (GameSurfaceCanvas)View.FindName("GameCanvas");
    public Slider Slider(string name) => (Slider)View.FindName(name);

    public async ValueTask InitializeAsync()
    {
        // The maintainer walkthrough drives the same methods the tests do; it must never run here.
        Environment.SetEnvironmentVariable("GAMEENGINEMUSICDEMO_SELFTEST", null);
        PlayTestApp app = null;
        Application = await PlayTestApplication.LaunchAsync(() => app = new PlayTestApp(), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });
        View = await Application.EvaluateAsync(() => (MainPage)((Frame)app.Window.Content).Content);
        // The engine, the audio device and MusicManager are process-wide, and the demo starts them once,
        // from the canvas's first layout. Keep this page for the whole run and reset its state instead.
        FirstDemo = await Application.WaitForAsync(() => Demo, demo => demo?.SongStems != null, description: "the started demo");
        await Application.EvaluateAsync(() =>
        {
            _stemGains = Demo.Stems.Stems.Select(stem => stem.Gain).ToArray();
            _songStemGains = Demo.SongStems.Stems.Select(stem => stem.Gain).ToArray();
            _harmonyVolume = Demo.MidiTrack.GetLayerVolume(2);
        });
    }

    public async Task ResetAsync(ScreenOrientation? orientation)
    {
        await Application.EvaluateAsync(() =>
        {
            if (Demo.IsPaused) Demo.TogglePause();
            Demo.ReleaseHeldDuck();
            MusicManager.Instance.Stop();
            MusicManager.Instance.ClearDucks();
            foreach (var (name, value) in SliderDefaults) Slider(name).Value = value;
            AudioMixer.Reset();
            for (var i = 0; i < _stemGains.Length; i++) Demo.Stems[i].Gain = _stemGains[i];
            for (var i = 0; i < _songStemGains.Length; i++) Demo.SongStems[i].Gain = _songStemGains[i];
            Demo.MidiTrack.FadeLayerTo(2, _harmonyVolume);
            Demo.MidiTrack.Speed = 1f;
            Descendants(View).OfType<ScrollViewer>().First().ChangeView(null, 0, null, true);
        });
        await Application.WaitForAsync(() => MusicManager.Instance.NowPlaying == null && !MusicManager.Instance.HasPendingTransition
            && MusicManager.Instance.ActiveFadeCount == 0 && AudioMixer.MusicDuckMultiplier == 1f, idle => idle,
            description: "silent, unducked music system");
        // Re-lays out the same page; the canvas raises FirstStarted only once, so the demo is not restarted.
        await Application.SetOrientationAsync(orientation);
    }

    public async ValueTask DisposeAsync()
    {
        if (Application != null)
        {
            await Application.EvaluateAsync(() => Demo?.Stop());
            await Application.DisposeAsync();
        }
    }

    internal static IEnumerable<UIElement> Descendants(UIElement root)
    {
        yield return root;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            if (VisualTreeHelper.GetChild(root, i) is UIElement child)
                foreach (var descendant in Descendants(child)) yield return descendant;
    }

    // App keeps its window protected; the tests need the page it navigated to, not a second one.
    private sealed class PlayTestApp : App
    {
        public Window Window => MainWindow;
    }
}
