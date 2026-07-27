using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using Windows.Graphics.Display;

namespace PolyHavenBrowser;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFolderPicker(new FolderPickerOptions {
                   AllowNewFolderCreate = true,
                   //ShowHiddenFolders = true,
                   StartFolder = "/home/jeremy/Temp",
                   RestrictToFolder = "/home/jeremy",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    //ShowDismissKey = false,
                    KeyHeight = SoftwareKeyHeight.FullHeight,  //default behavior = FullHeight
                    //KeyHeight = SoftwareKeyHeight.HalfHeight,
                })
            )
            .UseDirectSkiaCanvasMode()
            .Build();

        host.Run();
    }
}
