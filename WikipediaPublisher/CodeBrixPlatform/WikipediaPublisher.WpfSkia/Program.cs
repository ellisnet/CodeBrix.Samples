using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia.Wpf;
using System;
using WikipediaPublisher.Helpers;

// ReSharper disable CheckNamespace

namespace WikipediaPublisher;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //The Skia-on-WPF runtime has built-in WebView2 (Microsoft Edge WebView2) support
        AppCapabilities.HasWebView = true;

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWpf()
            .Build();

        // The WPF host's default OpenGL renderer draws via raw opengl32 onto WPF's own
        // DirectX-composited HWND, which causes "airspace" conflicts on many systems —
        // the window shows but the content never composites (blank black/white window).
        // Software rendering blits the Skia frame into WPF and composites correctly.
        if (host is WpfHost wpfHost)
        {
            wpfHost.RenderSurfaceType = RenderSurfaceType.Software;
        }

        host.Run();
    }
}
