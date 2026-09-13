using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;

// ReSharper disable CheckNamespace

namespace JustBetweenUs;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                 .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    //ShowDismissKey = false,
                    //KeyHeight = SoftwareKeyHeight.PortraitFullLandscapeFull,  //default behavior = FullHeight
                    KeyHeight = SoftwareKeyHeight.PortraitHalfLandscapeHalf,
                })           
            )
            .UseDirectSkiaCanvasMode()
            .Build();

        host.Run();
    }
}
