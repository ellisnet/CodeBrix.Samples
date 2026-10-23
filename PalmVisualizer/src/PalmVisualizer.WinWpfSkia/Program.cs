using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia.Wpf;
using System;

namespace PalmVisualizer;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWpf()
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        if (host is WpfHost wpfHost)
        {
            wpfHost.RenderSurfaceType = RenderSurfaceType.Software;

            //Visualize Mode repaints continuously, and on this head every frame also re-composites
            //  the whole window on the CPU - in a large window a frame can outlast the frame interval,
            //  and at the default (RenderFirst) tier that starves keyboard and mouse input outright,
            //  so the app stops responding. InputFair lets input interleave with the frames.
            wpfHost.DispatcherScheduling = WpfDispatcherScheduling.InputFair;
        }

        host.Run();
    }
}
