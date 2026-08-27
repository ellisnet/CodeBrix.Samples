using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using Windows.Graphics.Display;

namespace PdfSideBySide;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //The FrameBuffer head has no OS chrome, so the "Browse…" file picker is opt-in
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFileOpenPicker(new FilePickerOptions {
                    AllowMultipleFileSelect = false,
                    StartFolder = "/home/jeremy/Temp",
                    RestrictToFolder = "/home/jeremy",
                    RequiredExtension = ".pdf",
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
