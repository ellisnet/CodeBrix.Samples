using System;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimpleCbxVideoPlayer.SkiaVideo;
using SimpleCbxVideoPlayer.ViewModels;
using SimpleCbxVideoPlayer.Views;
using SkiaSharp.Views.Windows;

namespace SimpleCbxVideoPlayer.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public VideoPlaybackController Player => Model.Controller;
    public Grid VideoHost => (Grid)View.FindName("VideoHost");
    // The timestamp of the frame the CPU canvas last painted, or null before its first paint with a frame.
    public TimeSpan? PaintedTimestamp { get; private set; }
    protected override Application CreateApplication() => new App();

    // The application initializes OpenGL elements. These tests cover the no-OpenGL fallback, so the launch opts out;
    // to give the elements real contexts instead, call CodeBrixPlayTestOpenGL.Register() in Prepare() and drop this override.
    protected override void Configure(PlayTestOptions options) => options.OpenGL = PlayTestOpenGL.Unavailable;

    protected override async Task AfterResetAsync()
    {
        // The page tries the GPU canvas once it has loaded, then collapses it: tests start from the settled CPU canvas.
        await Application.WaitForAsync(() => VideoHost.Children.OfType<SkiaGLCanvasElement>().FirstOrDefault()?.Visibility,
            visibility => visibility == Visibility.Collapsed, description: "the settled video surface");
        PaintedTimestamp = null;
        await Application.EvaluateAsync(() =>
        {
            var page = View;
            // Added after the page's own handler, so it runs once the frame has been drawn.
            ((SKXamlCanvas)page.FindName("CpuCanvas")).PaintSurface += (_, _) =>
            {
                if (ReferenceEquals(page, View) && Player.IsOpen) PaintedTimestamp = Player.CurrentTimestamp;
            };
        });
    }
}
