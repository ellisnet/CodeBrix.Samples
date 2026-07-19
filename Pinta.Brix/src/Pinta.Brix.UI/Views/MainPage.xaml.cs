using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Pinta.Brix.Views;

public sealed partial class MainPage : Page
{
    private readonly Dictionary<Document, TabViewItem> documentTabs = new();
    private ToolBarRenderer toolOptionsRenderer;
    private bool updatingZoomCombo;
    private bool updatingLayerSelection;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        this.InitializeComponent(); //Leave this line last

        AddPageAccelerator(Windows.System.VirtualKey.Z, ctrl: true, shift: false, () => MenuEditUndo_Click(this, null));
        AddPageAccelerator(Windows.System.VirtualKey.Z, ctrl: true, shift: true, () => MenuEditRedo_Click(this, null));
        AddPageAccelerator(Windows.System.VirtualKey.Y, ctrl: true, shift: false, () => MenuEditRedo_Click(this, null));

        Loaded += MainPage_Loaded;
    }

    private void AddPageAccelerator(Windows.System.VirtualKey key, bool ctrl, bool shift, Action action)
    {
        var accelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = key,
            Modifiers = (ctrl ? Windows.System.VirtualKeyModifiers.Control : 0)
                | (shift ? Windows.System.VirtualKeyModifiers.Shift : 0),
        };
        accelerator.Invoked += (_, args) => { action(); args.Handled = true; };
        KeyboardAccelerators.Add(accelerator);
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        //Chrome wiring: dialogs need a XamlRoot, so this happens on Loaded
        PintaCore.Chrome.InitializeErrorDialogHandler(ShowErrorDialogAsync);
        PintaCore.Chrome.InitializeMessageDialog(ShowMessageDialogAsync);
        PintaCore.Chrome.InitializeProgessDialog(new NullProgressDialog());
        PintaCore.Chrome.InitializeSimpleEffectDialog(
            (effect, _) => EffectOptionsDialog.ShowAsync(effect, XamlRoot));
        PintaCore.Workspace.SaveDocumentHandler = SaveDocumentAsync;

        //Tool options toolbar renders the engine's descriptor model
        toolOptionsRenderer = new ToolBarRenderer(PintaCore.Chrome.ToolToolBar, ToolOptionsPanel);

        //Workspace events drive the document tabs and pads
        PintaCore.Workspace.DocumentActivated += (_, args) => AddDocumentTab(args.Document);
        PintaCore.Workspace.DocumentClosed += (_, args) => RemoveDocumentTab(args.Document);
        PintaCore.Workspace.ActiveDocumentChanged += (_, _) => OnActiveDocumentChanged();
        PintaCore.Workspace.LayerAdded += (_, _) => RefreshLayersPad();
        PintaCore.Workspace.LayerRemoved += (_, _) => RefreshLayersPad();
        PintaCore.Workspace.SelectedLayerChanged += (_, _) => RefreshLayersPad();

        //Status bar
        PintaCore.Chrome.LastCanvasCursorPointChanged += (_, _) =>
        {
            var p = PintaCore.Chrome.LastCanvasCursorPoint;
            CursorPositionText.Text = $"{p.X}, {p.Y}";
        };
        PintaCore.Chrome.StatusBarTextChanged += (_, args) => StatusText.Text = args.Text;

        //Adjustments/Effects menus follow the engine's registries
        PintaCore.Effects.AdjustmentsChanged += (_, _) => RebuildAdjustmentsMenu();
        PintaCore.Effects.EffectsChanged += (_, _) => RebuildEffectsMenu();
        RebuildAdjustmentsMenu();
        RebuildEffectsMenu();

        //Toolbox follows the tool manager
        PintaCore.Tools.ToolAdded += (_, _) => RefreshToolbox();
        PintaCore.Tools.ToolRemoved += (_, _) => RefreshToolbox();
        PintaCore.Tools.ToolActivated += (_, _) => RefreshToolbox();
        RefreshToolbox();

        //Zoom presets, matching the workspace's list, plus editable text
        updatingZoomCombo = true;
        foreach (double percent in DocumentWorkspace.ZoomPresets.Reverse())
            ZoomComboBox.Items.Add($"{percent:0.#}%");
        updatingZoomCombo = false;
    }

    // ---- Documents and tabs ------------------------------------------------

    private void AddDocumentTab(Document document)
    {
        PintaCanvasView view = new() { Document = document };
        TabViewItem tab = new()
        {
            Header = document.DisplayName,
            Content = view,
        };
        documentTabs[document] = tab;
        DocumentTabs.TabItems.Add(tab);
        DocumentTabs.SelectedItem = tab;

        document.Renamed += (_, _) => tab.Header = document.DisplayName;
        document.IsDirtyChanged += (_, _) =>
            tab.Header = document.IsDirty ? $"{document.DisplayName}*" : document.DisplayName;
    }

    private void RemoveDocumentTab(Document document)
    {
        if (!documentTabs.TryGetValue(document, out TabViewItem tab)) { return; }
        documentTabs.Remove(document);
        DocumentTabs.TabItems.Remove(tab);
    }

    private void DocumentTabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        Document document = documentTabs.FirstOrDefault(kv => kv.Value == args.Tab).Key;
        if (document is null) { return; }
        //V1: close without save-confirmation prompt (added with the dialogs pass)
        PintaCore.Workspace.CloseDocument(document);
    }

    private void DocumentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DocumentTabs.SelectedItem is not TabViewItem tab) { return; }
        Document document = documentTabs.FirstOrDefault(kv => kv.Value == tab).Key;
        if (document is null) { return; }
        int index = PintaCore.Workspace.OpenDocuments.IndexOf(document);
        if (index >= 0 && index != PintaCore.Workspace.ActiveDocumentIndex)
        {
            PintaCore.Workspace.SetActiveDocument(index);
        }
    }

    private void OnActiveDocumentChanged()
    {
        if (PintaCore.Workspace.HasOpenDocuments)
        {
            Document document = PintaCore.Workspace.ActiveDocument;
            if (documentTabs.TryGetValue(document, out TabViewItem tab))
            {
                DocumentTabs.SelectedItem = tab;
            }
            document.Workspace.ZoomChanged -= ActiveWorkspace_ZoomChanged;
            document.Workspace.ZoomChanged += ActiveWorkspace_ZoomChanged;
            ActiveWorkspace_ZoomChanged(null, EventArgs.Empty);
        }
        RefreshLayersPad();
        RefreshHistoryPad();
    }

    // ---- File menu ---------------------------------------------------------

    private void MenuFileNew_Click(object sender, RoutedEventArgs e)
    {
        //V1: fixed default size with a white background (the New Image dialog
        //arrives with the dialogs pass)
        PintaCore.Workspace.NewDocument(new Size(800, 600), new Pinta.Brix.Engine.Drawing.Color(1, 1, 1));
    }

    private async void MenuFileOpen_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
        foreach (var format in PintaCore.ImageFormats.Formats.Where(f => f.IsImportAvailable()))
        {
            foreach (string extension in format.Extensions.Where(x => x.All(char.IsLower)))
            {
                picker.FileTypeFilter.Add($".{extension}");
            }
        }

        StorageFile file = await picker.PickSingleFileAsync();
        if (file is null) { return; }
        PintaCore.Workspace.OpenFile(file.Path);
        PintaCore.RecentFiles.AddFile(file.Path);
    }

    private async void MenuFileSave_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        await PintaCore.Workspace.ActiveDocument.Save(saveAs: false);
    }

    private async void MenuFileSaveAs_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        await PintaCore.Workspace.ActiveDocument.Save(saveAs: true);
    }

    private void MenuFileClose_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        PintaCore.Workspace.CloseActiveDocument();
    }

    private async Task<bool> SaveDocumentAsync(Document document, bool saveAs)
    {
        string path = document.File;
        string fileType = document.FileType;

        if (path is null || saveAs)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = document.DisplayName,
            };
            foreach (var format in PintaCore.ImageFormats.Formats.Where(f => f.IsExportAvailable()))
            {
                var extensions = format.Extensions
                    .Where(x => x.All(char.IsLower))
                    .Select(x => $".{x}")
                    .ToList();
                if (extensions.Count > 0)
                {
                    picker.FileTypeChoices.Add(format.FilterName, extensions);
                }
            }

            StorageFile file = await picker.PickSaveFileAsync();
            if (file is null) { return false; }
            path = file.Path;
            fileType = System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        }

        var descriptor = PintaCore.ImageFormats.GetFormatByFile(path);
        if (descriptor is null || !descriptor.IsExportAvailable())
        {
            await PintaCore.Chrome.ShowErrorDialog(
                "Cannot save file", $"No exporter is available for '{path}'.", string.Empty);
            return false;
        }

        descriptor.Exporter.Export(document, path);
        document.File = path;
        document.FileType = fileType;
        document.Workspace.History.SetClean();
        PintaCore.Tools.DoAfterSave(document);
        return true;
    }

    // ---- Edit menu ---------------------------------------------------------

    private void MenuEditUndo_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var history = PintaCore.Workspace.ActiveWorkspace.History;
        if (history.CanUndo) { history.Undo(); }
        RefreshHistoryPad();
    }

    private void MenuEditRedo_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var history = PintaCore.Workspace.ActiveWorkspace.History;
        if (history.CanRedo) { history.Redo(); }
        RefreshHistoryPad();
    }

    // ---- View menu / zoom --------------------------------------------------

    private void MenuViewZoomIn_Click(object sender, RoutedEventArgs e)
    {
        if (PintaCore.Workspace.HasOpenDocuments) { PintaCore.Workspace.ActiveWorkspace.ZoomIn(); }
    }

    private void MenuViewZoomOut_Click(object sender, RoutedEventArgs e)
    {
        if (PintaCore.Workspace.HasOpenDocuments) { PintaCore.Workspace.ActiveWorkspace.ZoomOut(); }
    }

    private void MenuViewZoomNormal_Click(object sender, RoutedEventArgs e)
    {
        if (PintaCore.Workspace.HasOpenDocuments) { PintaCore.Workspace.ActiveWorkspace.ZoomManually(1.0); }
    }

    private void MenuViewZoomFit_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var workspace = PintaCore.Workspace.ActiveWorkspace;
        var imageSize = PintaCore.Workspace.ActiveDocument.ImageSize;
        var viewport = workspace.CanvasWindow?.ViewportSize ?? new Size(0, 0);
        if (viewport.Width <= 0 || viewport.Height <= 0 || imageSize.Width <= 0 || imageSize.Height <= 0) { return; }
        double ratio = Math.Min(
            (double)viewport.Width / imageSize.Width,
            (double)viewport.Height / imageSize.Height);
        workspace.ZoomManually(ratio);
    }

    private void ActiveWorkspace_ZoomChanged(object sender, EventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        updatingZoomCombo = true;
        ZoomComboBox.PlaceholderText = $"{PintaCore.Workspace.ActiveWorkspace.Scale * 100:0.#}%";
        ZoomComboBox.SelectedIndex = -1;
        updatingZoomCombo = false;
    }

    private void ZoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (updatingZoomCombo || !PintaCore.Workspace.HasOpenDocuments) { return; }
        if (ZoomComboBox.SelectedItem is not string text) { return; }
        if (double.TryParse(text.TrimEnd('%'), out double percent))
        {
            PintaCore.Workspace.ActiveWorkspace.ZoomManually(percent / 100.0);
        }
    }

    // ---- Adjustments / Effects menus ---------------------------------------

    private void RebuildAdjustmentsMenu()
    {
        AdjustmentsMenu.Items.Clear();
        foreach (Command command in PintaCore.Effects.AdjustmentCommands)
        {
            AdjustmentsMenu.Items.Add(CreateCommandMenuItem(command));
        }
    }

    private void RebuildEffectsMenu()
    {
        EffectsMenu.Items.Clear();
        foreach (string category in PintaCore.Effects.EffectCategories.OrderBy(c => c))
        {
            MenuFlyoutSubItem submenu = new() { Text = category };
            foreach (Command command in PintaCore.Effects.GetEffectCommands(category))
            {
                submenu.Items.Add(CreateCommandMenuItem(command));
            }
            EffectsMenu.Items.Add(submenu);
        }
    }

    private static MenuFlyoutItem CreateCommandMenuItem(Command command)
    {
        MenuFlyoutItem item = new() { Text = command.Label, IsEnabled = command.Sensitive };
        item.Click += (_, _) => command.Activate();
        command.SensitiveChanged += (_, _) => item.IsEnabled = command.Sensitive;
        return item;
    }

    // ---- Toolbox -----------------------------------------------------------

    private void RefreshToolbox()
    {
        ToolboxPanel.Children.Clear();
        foreach (BaseTool tool in PintaCore.Tools)
        {
            ToggleButton button = new()
            {
                Content = new Image
                {
                    Width = 24,
                    Height = 24,
                    Source = IconImageSource.Create(tool.Icon, 24),
                },
                IsChecked = PintaCore.Tools.CurrentTool == tool,
                Margin = new Thickness(2),
            };
            ToolTipService.SetToolTip(button, tool.Name);
            BaseTool captured = tool;
            button.Click += (_, _) => PintaCore.Tools.SetCurrentTool(captured);
            ToolboxPanel.Children.Add(button);
        }
    }

    // ---- Pads --------------------------------------------------------------

    private void RefreshLayersPad()
    {
        LayersList.Items.Clear();
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var layers = PintaCore.Workspace.ActiveDocument.Layers;

        //Topmost layer first, matching the upstream layers pad
        for (int i = layers.Count() - 1; i >= 0; i--)
        {
            LayersList.Items.Add(layers[i].Name);
        }

        updatingLayerSelection = true;
        LayersList.SelectedIndex = layers.Count() - 1 - layers.CurrentUserLayerIndex;
        updatingLayerSelection = false;
    }

    private void LayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (updatingLayerSelection || !PintaCore.Workspace.HasOpenDocuments) { return; }
        var layers = PintaCore.Workspace.ActiveDocument.Layers;
        int index = layers.Count() - 1 - LayersList.SelectedIndex;
        if (index >= 0 && index < layers.Count())
        {
            layers.SetCurrentUserLayer(index);
        }
    }

    private void AddLayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var document = PintaCore.Workspace.ActiveDocument;
        document.Layers.AddNewLayer(string.Empty);
        document.Workspace.History.PushNewItem(
            new AddLayerHistoryItem(Icons.LayerNew, "Add New Layer", document.Layers.CurrentUserLayerIndex));
        RefreshLayersPad();
    }

    private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        var document = PintaCore.Workspace.ActiveDocument;
        if (document.Layers.Count() <= 1) { return; }
        var hist = new DeleteLayerHistoryItem(
            Icons.LayerDelete, "Delete Layer",
            document.Layers.CurrentUserLayer, document.Layers.CurrentUserLayerIndex);
        document.Layers.DeleteLayer(document.Layers.CurrentUserLayerIndex);
        document.Workspace.History.PushNewItem(hist);
        RefreshLayersPad();
    }

    private void RefreshHistoryPad()
    {
        HistoryList.Items.Clear();
        if (!PintaCore.Workspace.HasOpenDocuments) { return; }
        //V1: a simple text list of history items (the interactive pad comes later)
        foreach (var item in PintaCore.Workspace.ActiveWorkspace.History.Items)
        {
            HistoryList.Items.Add(item.Text ?? string.Empty);
        }
    }

    // ---- Dialog handlers ---------------------------------------------------

    private async Task<ErrorDialogResponse> ShowErrorDialogAsync(string message, string body, string details)
    {
        ContentDialog dialog = new()
        {
            Title = message,
            Content = string.IsNullOrEmpty(details) ? body : $"{body}\n\n{details}",
            CloseButtonText = "Close",
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
        return ErrorDialogResponse.OK;
    }

    private async Task ShowMessageDialogAsync(string message, string body)
    {
        ContentDialog dialog = new()
        {
            Title = message,
            Content = body,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private sealed class NullProgressDialog : IProgressDialog
    {
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public double Progress { get; set; }
#pragma warning disable CS0067 //event never used - V1 placeholder
        public event EventHandler Canceled;
#pragma warning restore CS0067
        public void Show() { }
        public void Hide() { }
    }
}
