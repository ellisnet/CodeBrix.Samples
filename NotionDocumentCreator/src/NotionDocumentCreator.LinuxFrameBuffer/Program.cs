using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using System.IO;
using Windows.Graphics.Display;

namespace NotionDocumentCreator;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //The FrameBuffer head has no OS chrome, so the save picker and the software
        //  keyboard are opt-in — and this app needs the keyboard badly (the user
        //  types a long Notion API token)
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFileSavePicker(new FilePickerOptions {
                    AllowNewFolderCreate = true,
                    RestrictToFolder = GetPickerRootFolder(),
                    RequiredExtension = ".pdf",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    KeyHeight = SoftwareKeyHeight.PortraitHalfLandscapeHalf,
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }

    //The picker is restricted to one folder, and that folder is worked out at run time rather
    //  than written into the source, so this head behaves the same on every device it reaches.
    private static string GetPickerRootFolder()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrWhiteSpace(home) ? Directory.GetCurrentDirectory() : home;
    }
}
