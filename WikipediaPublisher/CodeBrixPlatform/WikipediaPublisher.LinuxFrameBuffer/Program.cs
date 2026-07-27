using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using Windows.Graphics.Display;

// ReSharper disable CheckNamespace

namespace WikipediaPublisher;

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
                .EnableFileSavePicker(new FilePickerOptions {
                    AllowNewFolderCreate = true,
                    ShowHiddenFiles = false,  //default behavior = false
                    RestrictToFolder = "/home/jeremy",
                    RequiredExtension = ".pdf",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    //ShowDismissKey = false,
                    //KeyHeight = SoftwareKeyHeight.FullHeight,  //default behavior = FullHeight
                    KeyHeight = SoftwareKeyHeight.HalfHeight,
                })
            )
            .UseDirectSkiaCanvasMode()
            .Build();

        host.Run();
    }
}
