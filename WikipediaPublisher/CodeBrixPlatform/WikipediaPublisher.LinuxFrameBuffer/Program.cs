using CodeBrix.Platform.UI.Hosting;
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

        //No WebView on the framebuffer head - the app shows the native search-results pane instead
        AppCapabilities.HasWebView = false;

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer()
            .Build();

        host.Run();
    }
}
