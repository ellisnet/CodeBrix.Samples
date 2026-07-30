using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
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
                    RestrictToFolder = "/home/jeremy",
                    RequiredExtension = ".pdf",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    KeyHeight = SoftwareKeyHeight.HalfHeight,
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
