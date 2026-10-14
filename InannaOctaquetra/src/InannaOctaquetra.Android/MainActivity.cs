using Android.App;
using CodeBrix.Android.UI.Hosting;

namespace InannaOctaquetra;

/// <summary>
/// The launcher activity that shows the app's XAML window. HandledConfigurationChanges keeps the
/// activity alive when the window is resized, docked, rotated or switched to dark mode.
/// </summary>
[Activity(
    Label = "InannaOctaquetra",
    MainLauncher = true,
    Theme = "@style/Theme.Material3.DayNight.NoActionBar",
    ConfigurationChanges = CodeBrixActivity.HandledConfigurationChanges)]
public class MainActivity : CodeBrixActivity
{
}
