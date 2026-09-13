using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using System.IO;
using Windows.Graphics.Display;

namespace PdfSideBySide;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //The framebuffer picker draws its own file list, so it needs a folder to start in and a
        //  tree to stay inside: this user's documents folder, inside their home directory
        var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(homeFolder)) { homeFolder = Environment.CurrentDirectory; }
        var startFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrEmpty(startFolder) || !Directory.Exists(startFolder)) { startFolder = homeFolder; }

        //The FrameBuffer head has no OS chrome, so the "Browse…" file picker is opt-in
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFileOpenPicker(new FilePickerOptions {
                    AllowMultipleFileSelect = false,
                    StartFolder = startFolder,
                    RestrictToFolder = homeFolder,
                    RequiredExtension = ".pdf",
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
