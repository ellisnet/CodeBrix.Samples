using Android.App;
using CodeBrix.Android.UI.Hosting;

namespace CodeBrixVideoTool;

/// <summary>
/// The launcher activity that shows the app's XAML window. HandledConfigurationChanges keeps the
/// activity alive when the window is resized, docked, rotated or switched to dark mode.
/// </summary>
[Activity(
    Label = "CodeBrix Video Tool",
    MainLauncher = true,
    Theme = "@style/Theme.Material3.DayNight.NoActionBar",
    ConfigurationChanges = CodeBrixActivity.HandledConfigurationChanges)]
public class MainActivity : CodeBrixActivity
{
}
