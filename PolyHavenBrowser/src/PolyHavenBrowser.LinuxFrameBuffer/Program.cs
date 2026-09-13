using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using System;
using System.IO;
using Windows.Graphics.Display;

namespace PolyHavenBrowser;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //This head's pickers are the only file system chrome the device has, so they are
        //  bounded by the running user's home folder and open in a Temp folder inside it
        //  when there is one. Deriving both from the environment keeps the head runnable on
        //  any machine; an appliance would point them at its own content folder instead.
        var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var startFolder = Path.Combine(homeFolder, "Temp");
        if (!Directory.Exists(startFolder)) { startFolder = homeFolder; }

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer(fb => fb
                .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
                .AutoRotationEnabled(true)
                .EnableFolderPicker(new FolderPickerOptions {
                   AllowNewFolderCreate = true,
                   //ShowHiddenFolders = true,
                   StartFolder = startFolder,
                   RestrictToFolder = homeFolder,
                })
                //The FrameBuffer head has no OS chrome, so the "Save PDF as…" picker the
                //  Document button pops is opt-in
                .EnableFileSavePicker(new FilePickerOptions {
                   AllowNewFolderCreate = true,
                   StartFolder = startFolder,
                   RestrictToFolder = homeFolder,
                   RequiredExtension = ".pdf",
                })
                .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
                    ShowDismissKey = true,  //default behavior = true
                    //ShowDismissKey = false,
                    KeyHeight = SoftwareKeyHeight.PortraitFullLandscapeFull,  //default behavior = FullHeight
                    //KeyHeight = SoftwareKeyHeight.PortraitHalfLandscapeHalf,
                })
            )
            .UseDirectSkiaCanvasMode()
            .Build();

        host.Run();
    }
}
