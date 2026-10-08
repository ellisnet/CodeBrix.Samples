using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace CodeBrix.Samples.PlayTests;

public abstract class SampleFixture<TPage> : IAsyncLifetime where TPage : FrameworkElement, new()
{
    public PlayTestApplication Application { get; private set; }
    public TPage View { get; private set; }
    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"));
    protected abstract Application CreateApplication();
    protected virtual void Prepare() { }
    // Adjusts the launch options (for example PlayTestOptions.OpenGL) before the application starts.
    protected virtual void Configure(PlayTestOptions options) { }
    protected virtual Task BeforeResetAsync() => Task.CompletedTask;
    protected virtual Task AfterResetAsync() => Task.CompletedTask;
    protected virtual void Cleanup() { }

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(DataDirectory);
        Prepare();
        var options = new PlayTestOptions { ConfigurationAssembly = GetType().Assembly };
        Configure(options);
        Application = await PlayTestApplication.LaunchAsync(CreateApplication, options);
    }

    public async Task ResetAsync(ScreenOrientation? orientation = null)
    {
        await BeforeResetAsync();
        Application.FilePickers.Clear();
        await Application.Page.SetContentAsync(() => View = new TPage(), orientation);
        await AfterResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Application != null)
            {
                try { await Application.EvaluateAsync(() => (View?.DataContext as IDisposable)?.Dispose()); }
                finally { await Application.DisposeAsync(); }
            }
        }
        finally { Cleanup(); }
    }
}

public abstract class SampleTest<TFixture, TPage> : PageTest, IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : SampleFixture<TPage>
    where TPage : FrameworkElement, new()
{
    protected TFixture Fixture { get; }
    protected SampleTest(TFixture fixture) : base(fixture.Application) => Fixture = fixture;

    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var orientations);
        await Fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, orientations));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    protected Locator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Task SnapshotAsync(string name) => Page.ScreenshotAsync(new()
    {
        Path = Path.Combine("TestResults", "PlayTest", name + ".png"),
    });
}
