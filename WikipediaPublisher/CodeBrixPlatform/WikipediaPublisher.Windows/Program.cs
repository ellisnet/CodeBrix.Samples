using CodeBrix.Platform.UI.Hosting;
using System;
using System.Threading.Tasks;
using WikipediaPublisher.Helpers;

// ReSharper disable CheckNamespace

namespace WikipediaPublisher;

internal class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        App.InitializeLogging();

        //The Win32 runtime has built-in WebView2 (Microsoft Edge WebView2) support
        AppCapabilities.HasWebView = true;

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWin32()
            .Build();

        await host.RunAsync();
    }
}
