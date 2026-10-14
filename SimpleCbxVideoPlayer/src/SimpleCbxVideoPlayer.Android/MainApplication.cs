using Android.App;
using Android.Content.PM;
using Android.Runtime;
using CodeBrix.Android.UI.Hosting;
using System;
using System.IO;

namespace SimpleCbxVideoPlayer;

/// <summary>
/// The Android application. CodeBrixApplication sets up logging (logcat) and the Android
/// implementation of CodeBrix.Platform; the first activity then starts the shared App
/// (App.xaml.cs runs unchanged: OnLaunched, Window, Frame navigation).
/// </summary>
[Application]
public class MainApplication : CodeBrixApplication
{
    //The APK asset folder the head csproj packs the Core project's Assets folder into
    private const string CorpusAssetFolder = "SampleCorpus";

    //Written into the copied folder once a copy is complete; names the installed build it came from
    private const string CopyMarkerPrefix = ".copied-from-";

    public MainApplication(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
        //The soft keyboard pans the window by default (SoftInputAdjust.Pan); to lay the page out again above it instead:
        //SoftInputAdjust = SoftInputAdjust.Resize;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        //The playback library leaves the sound device to the application: on Android that is CodeBrix.Audio's
        //Android backend (the desktop heads get CodeBrix.Audio's native backends instead)
        global::CodeBrix.Audio.Android.CodeBrixAndroidAudio.Initialize(this);

        CopyCorpusOut();
    }

    protected override Microsoft.UI.Xaml.Application CreateApp() => new App();

    /// <summary>
    /// Copies the packed corpus to AppContext.BaseDirectory/Assets, where SampleAssets looks for it on
    /// every head. Done once per installed build: a marker file names the build, so a reinstall or an update
    /// copies again and an ordinary start copies nothing.
    /// </summary>
    private void CopyCorpusOut()
    {
        var target = Path.Combine(AppContext.BaseDirectory, "Assets");

        try
        {
            var info = PackageManager.GetPackageInfo(PackageName, PackageManager.PackageInfoFlags.Of((PackageInfoFlagsLong)0));
            var marker = Path.Combine(target, CopyMarkerPrefix + info.LastUpdateTime);

            if (File.Exists(marker)) { return; }

            if (Directory.Exists(target)) { Directory.Delete(target, recursive: true); }

            var started = DateTime.UtcNow;
            var files = CopyAssetTree(CorpusAssetFolder, target);
            File.WriteAllText(marker, string.Empty);

            global::Android.Util.Log.Info(PackageName,
                $"Copied the sample corpus ({files} files) to {target} in {(DateTime.UtcNow - started).TotalMilliseconds:F0} ms");
        }
        catch (Exception exception)
        {
            //The page reports a missing corpus in its status line; the log says why
            global::Android.Util.Log.Error(PackageName, $"Could not copy the sample corpus to {target}: {exception}");
        }
    }

    private int CopyAssetTree(string assetPath, string targetPath)
    {
        var names = Assets.List(assetPath) ?? Array.Empty<string>();

        if (names.Length == 0)
        {
            //A file (AssetManager lists nothing under a file)
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            using (var from = Assets.Open(assetPath))
            using (var to = File.Create(targetPath))
            {
                from.CopyTo(to);
            }

            return 1;
        }

        var count = 0;

        foreach (var name in names)
        {
            count += CopyAssetTree(assetPath + "/" + name, Path.Combine(targetPath, name));
        }

        return count;
    }
}
