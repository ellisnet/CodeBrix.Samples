using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using Windows.Graphics.Display;

namespace KenneyAssetBrowser;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //The FrameBuffer head has no OS chrome, so the folder picker (for choosing the
        //assets folder) and the software keyboard (for the search box) are opt-in.
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFolderPicker(new FolderPickerOptions {
                   AllowNewFolderCreate = false,
                   StartFolder = "/home/jeremy/Assets",
                   RestrictToFolder = "/home/jeremy",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    KeyHeight = SoftwareKeyHeight.FullHeight,  //default behavior = FullHeight
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
