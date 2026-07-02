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

        //The macOS runtime has built-in WKWebView-backed WebView2 support
        AppCapabilities.HasWebView = true;

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseMacOS()
            .Build();

        host.Run();
    }
}
