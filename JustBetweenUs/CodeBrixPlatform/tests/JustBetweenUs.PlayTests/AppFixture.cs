using System;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using JustBetweenUs.ViewModels;
using JustBetweenUs.Views;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace JustBetweenUs.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public ScreenOrientation PageCreationOrientation { get; private set; }
    public Windows.UI.Color AppConstructionSystemBackground { get; private set; }
    public ApplicationTheme AppLaunchTheme { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Application = await PlayTestApplication.LaunchAsync(() =>
        {
            AppConstructionSystemBackground = new UISettings().GetColorValue(UIColorType.Background);
            return new App();
        }, new()
        {
            // Leave Orientation unset to use environment -> project preference -> Landscape.
            // Setting Orientation explicitly here overrides both environment and project preferences.
            // Also supplies CodeBrixPlayTestPreferredTheme (environment -> project -> Light).
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });
        AppLaunchTheme = await Application.EvaluateAsync(() => Microsoft.UI.Xaml.Application.Current.RequestedTheme);
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Paul Ainsworth");
        await Application.Page.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
    }

    public async Task ResetAsync(ScreenOrientation? orientation = null)
    {
        Application.FilePickers.Clear();
        await Application.Page.SetContentAsync(() =>
        {
            PageCreationOrientation = Application.Orientation;
            return View = new MainPage();
        }, orientation);
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Paul Ainsworth");
        await Application.Page.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        var initialization = await Application.EvaluateAsync(() => ((MainViewModel)View.DataContext).Initialization);
        await initialization.WaitAsync(TimeSpan.FromSeconds(10));
    }

    public ValueTask DisposeAsync() => Application == null ? ValueTask.CompletedTask : Application.DisposeAsync();
}
