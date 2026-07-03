using CodeBrix.Platform.UI.Hosting;
using System;

// ReSharper disable CheckNamespace

namespace WikipediaPublisher;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //Note: this kiosk/embedded head has no windowing system for native file dialogs, so the
        //  user types the PDF save path directly into the box (the WebView still works via WPE).
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer()
            .Build();

        host.Run();
    }
}
