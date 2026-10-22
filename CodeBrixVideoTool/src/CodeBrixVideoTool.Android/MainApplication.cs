using Android.App;
using Android.Runtime;
using CodeBrix.Android.UI.Hosting;
using System;

namespace CodeBrixVideoTool;

/// <summary>
/// The Android application. CodeBrixApplication sets up logging (logcat) and the Android
/// implementation of CodeBrix.Platform; the first activity then starts the shared App
/// (App.xaml.cs runs unchanged: OnLaunched, Window, Frame navigation).
/// </summary>
/// <remarks>
/// Nothing media-specific is set up here. App's constructor registers the codecs and this head's
/// video-processing engine, as it does on every head, and the VideoPlayer add-in initialises
/// CodeBrix.Audio's Android backend for its own sound.
/// </remarks>
[Application]
public class MainApplication : CodeBrixApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
        //The soft keyboard pans the window by default (SoftInputAdjust.Pan); to lay the page out again above it instead:
        //SoftInputAdjust = SoftInputAdjust.Resize;
    }

    protected override Microsoft.UI.Xaml.Application CreateApp() => new App();
}
