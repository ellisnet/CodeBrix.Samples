using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using Pinta.Brix.Settings;
using Pinta.Brix.Views;
using SkiaSharp;
using Windows.ApplicationModel.DataTransfer;
using Xunit;
using Color = Pinta.Brix.Engine.Drawing.Color;

namespace Pinta.Brix.PlayTests;

[CollectionDefinition(Name)]
public sealed class AppCollection : ICollectionFixture<AppFixture>
{
    public const string Name = "Pinta.Brix application";
}

public sealed class AppFixture : IAsyncLifetime
{
    private BaseTool _launchTool;
    private Color _launchPrimary;
    private Color _launchSecondary;

    public PlayTestApplication Application { get; private set; }
    public MainPage View { get; private set; }
    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"));
    public string TestDirectory { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(DataDirectory);
        PlayTestApp app = null;
        // The app's own settings store would be the user's settings.sqlite; this one lives in the test output.
        Application = await PlayTestApplication.LaunchAsync(() => app = new PlayTestApp(Path.Combine(DataDirectory, "settings")), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
            ArtifactsDirectory = Path.Combine(DataDirectory, "failures"),
        });
        // PintaCore is process-wide and the page subscribes to it once, from its first Loaded. Keep the page the
        // application navigated to for the whole run and reset the documents, tool and palette on it instead.
        View = await Application.EvaluateAsync(() => (MainPage)((Frame)app.Window.Content).Content);
        await Application.WaitForAsync(() => PintaCore.Workspace.HasOpenDocuments, open => open, description: "the launch document");
        await Application.EvaluateAsync(() =>
        {
            _launchTool = PintaCore.Tools.CurrentTool;
            _launchPrimary = PintaCore.Palette.PrimaryColor;
            _launchSecondary = PintaCore.Palette.SecondaryColor;
        });
    }

    public async Task ResetAsync(ScreenOrientation? orientation)
    {
        TestDirectory = Path.Combine(DataDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TestDirectory);
        Application.FilePickers.Clear();
        // Answer whatever a failed test left open, so the handler awaiting it finishes before the documents go.
        await Application.EvaluateAsync(() =>
        {
            foreach (var popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(View.XamlRoot).ToArray())
            {
                if (popup.Child is ContentDialog dialog) dialog.Hide();
                else if (Descendants(popup.Child).OfType<Button>().FirstOrDefault(b => b.Content as string == "Cancel") is { } cancel)
                    ((IInvokeProvider)new ButtonAutomationPeer(cancel)).Invoke();
                else popup.IsOpen = false;
            }
        });
        await Application.WaitForAsync(() => VisualTreeHelper.GetOpenPopupsForXamlRoot(View.XamlRoot).Count == 0 && !PintaCore.LivePreview.IsEnabled,
            idle => idle, description: "no open dialogs or live preview");
        await Application.EvaluateAsync(() =>
        {
            PintaCore.Tools.SetCurrentTool(_launchTool);
            foreach (var document in PintaCore.Workspace.OpenDocuments.ToArray()) PintaCore.Workspace.CloseDocument(document);
            PintaCore.Palette.PrimaryColor = _launchPrimary;
            PintaCore.Palette.SecondaryColor = _launchSecondary;
            PintaCore.Actions.Edit.ResetPalette.Activate();
            Clipboard.Clear();
            // The pad splitters' launch layout; their persisted positions go with it.
            var pads = (Grid)View.FindName("PadsColumn");
            pads.Width = 230;
            pads.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            SettingsService.Set("pads-width", null);
            SettingsService.Set("layers-pad-height", null);
            // File > New: the same path the application opens its first document through.
            PintaCore.Actions.File.New.Activate();
        });
        await Application.SetOrientationAsync(orientation);
    }

    public async ValueTask DisposeAsync()
    {
        if (Application != null)
        {
            // Closing the window with unsaved work would ask about it; nothing must be left to ask about.
            await Application.EvaluateAsync(() =>
            {
                foreach (var document in PintaCore.Workspace.OpenDocuments.ToArray()) PintaCore.Workspace.CloseDocument(document);
            });
            await Application.DisposeAsync();
        }
        SettingsService.Shutdown();
    }

    internal static IEnumerable<UIElement> Descendants(UIElement root)
    {
        yield return root;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            if (VisualTreeHelper.GetChild(root, i) is UIElement child)
                foreach (var descendant in Descendants(child)) yield return descendant;
    }

    // App keeps its window protected; the tests need the page it navigated to, not a second one.
    private sealed class PlayTestApp(string settingsDirectory) : App(settingsDirectory)
    {
        public Window Window => MainWindow;
    }
}

[Collection(AppCollection.Name)]
public abstract class PintaTest(AppFixture fixture) : PageTest(fixture.Application), IAsyncLifetime
{
    protected AppFixture Fixture { get; } = fixture;
    protected static Document ActiveDocument => PintaCore.Workspace.ActiveDocument;

    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var orientations);
        await Fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, orientations));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected Locator Tabs => Page.GetByTestId("DocumentTabs").GetByRole(AriaRole.Tab);
    protected Locator Tab(string name) => Page.GetByTestId("DocumentTabs").GetByRole(AriaRole.Tab, new() { Name = name, Exact = true });
    protected Locator Tab(Regex name) => Page.GetByTestId("DocumentTabs").GetByRole(AriaRole.Tab, new() { NameRegex = name });
    // A tab is named after its document; a trailing * marks unsaved changes.
    protected Locator UnsavedTab(bool dirty = false) => Tab(new Regex(dirty ? @"^Unsaved Image \d+\*$" : @"^Unsaved Image \d+$"));
    protected Locator LayerRows => Page.GetByTestId("LayersList").GetByRole(AriaRole.Option);
    protected Locator HistoryRows => Page.GetByTestId("HistoryList").GetByRole(AriaRole.Option);
    protected Locator Canvas => Page.GetByType<PintaCanvas>();
    protected Locator Tool(string name) => Page.GetByTestId("Toolbox").GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Locator ToolbarButton(string name) => Page.GetByTestId("MainToolbar").GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Locator LayersPadButton(string name) => Page.GetByTestId("LayersPadToolbar").GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Locator HistoryPadButton(string name) => Page.GetByTestId("HistoryPadToolbar").GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Locator MenuItem(string name) => Page.GetByRole(AriaRole.Menuitem, new() { Name = name, Exact = true });
    // A dialog whose content is plain text is matched by that text, not by its title.
    protected Locator Dialog(string text) => Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = text });
    protected Locator DialogButton(string text, string button) =>
        Dialog(text).GetByRole(AriaRole.Button, new() { Name = button, Exact = true });

    protected async Task MenuAsync(params string[] path)
    {
        await MenuItem(path[0]).ClickAsync();
        for (var i = 1; i < path.Length - 1; i++) await MenuItem(path[i]).HoverAsync();
        await MenuItem(path[^1]).ClickAsync();
    }

    protected async Task AnswerAsync(string text, string button)
    {
        await DialogButton(text, button).ClickAsync();
        await Expect(Dialog(text)).ToHaveCountAsync(0);
    }

    protected Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description) =>
        Fixture.Application.WaitForAsync(probe, ready, description: description);

    protected Task<T> EvaluateAsync<T>(Func<T> expression) => Page.EvaluateAsync(expression);

    protected string TestPath(string name) => Path.Combine(Fixture.TestDirectory, name);

    // A small image written at test time, so no licensed asset is needed.
    protected string WritePng(string name, int width, int height, SKColor color)
    {
        var path = TestPath(name);
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    // Selects a tool the way a user does: by its toolbox button.
    protected async Task SelectToolAsync(string name)
    {
        await Tool(name).ClickAsync();
        await Expect(Tool(name)).ToBeCheckedAsync();
    }

    // Drags across the canvas between two image points, with real pointer input.
    protected async Task DragOnImageAsync(int fromX, int fromY, int toX, int toY)
    {
        var scale = await EvaluateAsync(() => ActiveDocument.Workspace.Scale);
        await Canvas.DragByAsync((float)((toX - fromX) * scale), (float)((toY - fromY) * scale), new()
        {
            Position = new() { X = (float)((fromX + 0.5) * scale), Y = (float)((fromY + 0.5) * scale) },
        });
    }

    // The current layer's pixel at an image point, as the engine holds it.
    protected Task<ColorBgra> PixelAsync(int x, int y, int? layer = null) => EvaluateAsync(() =>
    {
        var surface = layer is { } index ? ActiveDocument.Layers.UserLayers[index].Surface : ActiveDocument.Layers.CurrentUserLayer.Surface;
        return surface.GetColorBgra(new PointI(x, y));
    });

    protected Locator NumberBox(string dialogText, int index) => Dialog(dialogText).GetByRole(AriaRole.Textbox).Nth(index);

    // Types a value into a dialog's number box and moves on, which is what makes a NumberBox commit it.
    protected async Task SetNumberAsync(string dialogText, int index, string value)
    {
        await NumberBox(dialogText, index).FillAsync(value);
        await NumberBox(dialogText, index).PressAsync("Tab");
    }

    protected static bool IsWhite(ColorBgra pixel) => pixel is { R: > 240, G: > 240, B: > 240, A: 255 };
    protected static bool IsBlack(ColorBgra pixel) => pixel is { R: < 16, G: < 16, B: < 16, A: 255 };
    protected static bool IsTransparent(ColorBgra pixel) => pixel.A == 0;
}
