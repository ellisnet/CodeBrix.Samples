using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using System.IO;
using Windows.Graphics.Display;

namespace KenneyAssetBrowser;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //Where the folder picker opens, and how far up it lets the user browse: the signed-in
        //user's home folder, and its "Assets" subfolder when there is one. Computed here rather
        //than written in, so this head carries no path from the machine it was built on.
        var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var assetsFolder = Path.Combine(homeFolder, "Assets");
        if (!Directory.Exists(assetsFolder)) { assetsFolder = homeFolder; }

        //The FrameBuffer head has no OS chrome, so the folder picker (for choosing the
        //assets folder) and the software keyboard (for the search box) are opt-in.
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFolderPicker(new FolderPickerOptions {
                   AllowNewFolderCreate = false,
                   StartFolder = assetsFolder,
                   RestrictToFolder = homeFolder,
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    KeyHeight = SoftwareKeyHeight.PortraitFullLandscapeFull,  //default behavior = FullHeight
                })
            )
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
