using Android.App;
using Android.Runtime;
using CodeBrix.Android.UI.Hosting;
using PicoScope.Brix.ScopeData;
using PicoScope.Brix.ScopeData.Simulation;
using System;

namespace PicoScope.Brix;

/// <summary>
/// The Android application. CodeBrixApplication sets up logging (logcat) and the Android
/// implementation of CodeBrix.Platform; the first activity then starts the shared App
/// (App.xaml.cs runs unchanged: OnLaunched, Window, Frame navigation).
/// </summary>
[Application]
public class MainApplication : CodeBrixApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
        //The soft keyboard pans the window by default (SoftInputAdjust.Pan); to lay the page out again above it instead:
        //SoftInputAdjust = SoftInputAdjust.Resize;

        //This head decides which scope implementations the application can use, as each
        //  desktop head's Program.Main does. There is no ps2000 driver on Android, so the
        //  Android head registers the simulated device only.
        ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());
    }

    protected override Microsoft.UI.Xaml.Application CreateApp() => new App();
}
