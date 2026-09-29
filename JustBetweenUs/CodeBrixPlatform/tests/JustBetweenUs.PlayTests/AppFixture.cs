using System;
using System.Globalization;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using JustBetweenUs.ViewModels;
using JustBetweenUs.Views;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace JustBetweenUs.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }

    public async ValueTask InitializeAsync()
    {
        var orientation = Environment.GetEnvironmentVariable("CODEBRIX_PLAYTEST_ORIENTATION");
        if (!string.IsNullOrEmpty(orientation) && orientation != "portrait" && orientation != "landscape")
            throw new ArgumentException("CODEBRIX_PLAYTEST_ORIENTATION must be landscape or portrait.");
        var slowMo = Environment.GetEnvironmentVariable("CODEBRIX_PLAYTEST_SLOWMO");
        Application = await PlayTestApplication.LaunchAsync(() => new App(), new()
        {
            Orientation = orientation == "portrait" ? ScreenOrientation.Portrait : ScreenOrientation.Landscape,
            SlowMo = string.IsNullOrEmpty(slowMo) ? 0 : float.Parse(slowMo, CultureInfo.InvariantCulture),
        });
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Paul Ainsworth");
        await Application.Page.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
    }

    public async Task ResetAsync()
    {
        await Application.Page.SetContentAsync(() => View = new MainPage());
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToContainTextAsync("Paul Ainsworth");
        await Application.Page.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Assertions.Expect(Application.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        var initialization = await Application.EvaluateAsync(() => ((MainViewModel)View.DataContext).Initialization);
        await initialization.WaitAsync(TimeSpan.FromSeconds(10));
    }

    public ValueTask DisposeAsync() => Application == null ? ValueTask.CompletedTask : Application.DisposeAsync();
}
