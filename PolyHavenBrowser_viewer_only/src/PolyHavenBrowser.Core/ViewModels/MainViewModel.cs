using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using PolyHavenBrowser.Display;
using PolyHavenBrowser.Rendering;
using PolyHavenBrowser.Services;

// ReSharper disable once CheckNamespace
namespace PolyHavenBrowser.ViewModels;

/// <summary>
/// Lets the hosting page hand the view model a way to invalidate (repaint) the Skia canvas.
/// </summary>
public interface ICanvasInvalidator
{
    /// <summary>Invalidates the hosting page's canvas (null before the page wires it up).</summary>
    Action InvalidateCanvas { get; set; }
}

/// <summary>
/// Drives the PolyHavenBrowser main page: three sample buttons (texture, HDRI, model) that
/// download a representative Poly Haven asset on demand and display it on the shared Skia
/// canvas through an <see cref="IScenePainter"/>.
/// </summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, ICanvasInvalidator
{
    private readonly SampleAssetService _assets;
    private readonly IModelRenderEngineSelector _engineSelector;
    private ModelScenePainter _modelPainter;

    private IScenePainter _currentPainter;
    private PanoramaScenePainter _panoramaPainter;
    private SampleAssetKind _selectedKind = SampleAssetKind.Texture;
    private RenderEngineKind _currentEngineKind = RenderEngineKind.OpenGL;
    private string _selectedRenderEngineName = nameof(RenderEngineKind.OpenGL);
    private bool _isBusy;
    private string _statusText = "Starting up…";

    /// <summary>Creates the view model and begins loading the initial texture sample.</summary>
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; }

        _assets = GetService<SampleAssetService>();

        //The engine selector owns the available 3D backends (OpenGL + Vulkan) and creates
        //them on demand; the app always starts on OpenGL (no persistence).
        _engineSelector = GetService<IModelRenderEngineSelector>();
        foreach (var kind in _engineSelector.AvailableKinds)
        {
            RenderEngineNames.Add(kind.ToString());
        }
        NotifyPropertyChanged(nameof(RenderEngineNames));
        NotifyPropertyChanged(nameof(SelectedRenderEngineName));

        _modelPainter = new ModelScenePainter(_engineSelector.Create(RenderEngineKind.OpenGL));

        _ = SelectAsync(SampleAssetKind.Texture);
    }

    /// <inheritdoc />
    public Action InvalidateCanvas { get; set; }

    /// <summary>The painter the hosting canvas should draw with.</summary>
    public IScenePainter CurrentPainter => _currentPainter;

    /// <summary>Whether an asset is currently being downloaded or loaded.</summary>
    [AffectsCommands(nameof(SelectTextureCommand), nameof(SelectHdriCommand), nameof(SelectModelCommand))]
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            SetProperty(ref _isBusy, value);
            NotifyPropertyChanged(nameof(BusyVisibility));
            NotifyPropertyChanged(nameof(IsNotBusy));
        }
    }

    /// <summary>The inverse of <see cref="IsBusy"/> (disables the engine dropdown while loading).</summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>The busy indicator's visibility (visible while an asset is downloading/loading).</summary>
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>A short status line shown beneath the canvas.</summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    /// <summary>The rendering-engine names shown in the dropdown (OpenGL first - the default).</summary>
    public List<string> RenderEngineNames { get; } = new();

    /// <summary>
    /// The rendering engine picked in the dropdown. Selecting an engine that is not supported
    /// on this platform shows an alert and snaps the selection back; selecting a supported one
    /// swaps the 3D engine and re-displays the current texture/model sample through it.
    /// </summary>
    public string SelectedRenderEngineName
    {
        get => _selectedRenderEngineName;
        set
        {
            if (string.IsNullOrEmpty(value) || value == _selectedRenderEngineName) { return; }

            //Optimistic: show the new selection at once; SwitchEngineAsync reverts it if the
            //engine is unsupported or fails to initialize.
            _selectedRenderEngineName = value;
            NotifyPropertyChanged(nameof(SelectedRenderEngineName));
            _ = SwitchEngineAsync(value);
        }
    }

    /// <summary>
    /// The engine dropdown's visibility: shown in Texture and Model modes, hidden in HDRI mode
    /// (the HDRI panorama is CPU-rendered and unaffected by the engine choice).
    /// </summary>
    public Visibility EngineSelectorVisibility =>
        _selectedKind == SampleAssetKind.Hdri ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Whether the texture sample is selected (drives the button highlight).</summary>
    public bool IsTextureSelected => _selectedKind == SampleAssetKind.Texture;

    /// <summary>Whether the HDRI sample is selected.</summary>
    public bool IsHdriSelected => _selectedKind == SampleAssetKind.Hdri;

    /// <summary>Whether the model sample is selected.</summary>
    public bool IsModelSelected => _selectedKind == SampleAssetKind.Model;

    private SimpleCommand _selectTextureCommand;

    /// <summary>Selects and shows the sample texture (on a lit cube).</summary>
    public SimpleCommand SelectTextureCommand =>
        _selectTextureCommand ??= new SimpleCommand(() => !IsBusy, () => SelectAsync(SampleAssetKind.Texture));

    private SimpleCommand _selectHdriCommand;

    /// <summary>Selects and shows the sample HDRI panorama.</summary>
    public SimpleCommand SelectHdriCommand =>
        _selectHdriCommand ??= new SimpleCommand(() => !IsBusy, () => SelectAsync(SampleAssetKind.Hdri));

    private SimpleCommand _selectModelCommand;

    /// <summary>Selects and shows the sample 3D model.</summary>
    public SimpleCommand SelectModelCommand =>
        _selectModelCommand ??= new SimpleCommand(() => !IsBusy, () => SelectAsync(SampleAssetKind.Model));

    //Switches the 3D engine behind the model painter: alert + snap back when the engine is
    //not okayed for this platform, otherwise swap painters and re-display the current sample.
    private async Task SwitchEngineAsync(string engineName)
    {
        if (!Enum.TryParse<RenderEngineKind>(engineName, out var kind) || kind == _currentEngineKind)
        {
            return;
        }

        if (IsBusy)
        {
            //The dropdown is disabled while busy; this is just a belt-and-braces revert.
            RevertEngineSelection();
            return;
        }

        if (!_engineSelector.IsSupported(kind))
        {
            using (var alert = CreateDialog(
                "Vulkan rendering is not available on this platform.", "Vulkan Rendering"))
            {
                _ = await alert.ShowAsync();
            }
            RevertEngineSelection();
            return;
        }

        IsBusy = true;
        try
        {
            var engine = _engineSelector.Create(kind);
            if (kind == RenderEngineKind.Vulkan)
            {
                //Fail fast off the UI thread (a supported platform can still lack a working
                //driver) so a failure never surfaces inside the Skia paint callback. Safe for
                //Vulkan only: it has no thread-affinity, unlike the OpenGL engine's EGL
                //context, which must be created on the render thread at first paint.
                await Task.Run(() => engine.RenderFrame(1, 1, (0f, 0f, 0f, 1f)));
            }

            var oldPainter = _modelPainter;
            _modelPainter = new ModelScenePainter(engine);
            _currentEngineKind = kind;
            if (ReferenceEquals(_currentPainter, oldPainter))
            {
                _currentPainter = null;
            }
            oldPainter?.Dispose();
        }
        catch (Exception ex)
        {
            StatusText = $"Could not switch to {engineName} rendering: {ex.Message}";
            RevertEngineSelection();
            return;
        }
        finally
        {
            IsBusy = false;
        }

        //Re-display the current sample through the new engine (from the local cache, so no
        //network). The dropdown is hidden in HDRI mode, so this is always Texture or Model.
        if (_selectedKind != SampleAssetKind.Hdri)
        {
            await SelectAsync(_selectedKind);
        }
        else
        {
            InvalidateCanvas?.Invoke();
        }
    }

    private void RevertEngineSelection()
    {
        _selectedRenderEngineName = _currentEngineKind.ToString();
        NotifyPropertyChanged(nameof(SelectedRenderEngineName));
    }

    private async Task SelectAsync(SampleAssetKind kind)
    {
        if (IsBusy) { return; }

        _selectedKind = kind;
        RaiseSelectionChanged();
        IsBusy = true;

        try
        {
            var progress = new Progress<string>(message => StatusText = message);
            var asset = await _assets.EnsureSampleAsync(kind, progress, CancellationToken.None);

            //Decode/build off the UI thread; the painters upload to GL lazily during Paint.
            var painter = await Task.Run(() => BuildPainter(kind, asset));
            _currentPainter = painter;
            StatusText = $"{Label(kind)}: {asset.Name}    ·    {Hint(kind)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Could not load the {kind.ToString().ToLowerInvariant()} sample: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            InvalidateCanvas?.Invoke();
        }
    }

    private IScenePainter BuildPainter(SampleAssetKind kind, SampleAsset asset)
    {
        switch (kind)
        {
            case SampleAssetKind.Texture:
                //The decoded bitmap feeds the cube's texture and doubles as the darkened
                //  backdrop; the painter takes ownership of it for the background.
                var textureBitmap = TextureImageLoader.LoadForDisplay(asset.PrimaryFilePath);
                _modelPainter.SetModel(CubeMeshBuilder.Build(textureBitmap, asset.Name));
                _modelPainter.SetBackgroundTexture(textureBitmap);
                //A fixed key light (from upper-front-left) shades the faces distinctly so the
                //  cube reads as solid, and a 3/4 angle with perspective shows three faces.
                //  Extra framing margin keeps the whole cube (and its rotating silhouette)
                //  in view against the backdrop, which reads as clearly 3D.
                _modelPainter.FixedLightDirection = new Vector3(-0.4f, 1f, 0.7f);
                _modelPainter.Camera.FovDegrees = 40f;
                _modelPainter.Camera.YawDegrees = 35f;
                _modelPainter.Camera.PitchDegrees = 28f;
                _modelPainter.Camera.FitMargin = 1.2f;
                _modelPainter.Camera.VerticalFramingBias = 0f;
                return _modelPainter;

            case SampleAssetKind.Model:
                _modelPainter.SetModel(new GltfModelLoader().LoadFile(asset.PrimaryFilePath));
                _modelPainter.SetBackgroundTexture(null);
                _modelPainter.FixedLightDirection = null;
                _modelPainter.Camera.FovDegrees = 45f;
                _modelPainter.Camera.YawDegrees = 30f;
                _modelPainter.Camera.PitchDegrees = 15f;
                _modelPainter.Camera.FitMargin = 0.73f;         // ~1.5x closer than before
                _modelPainter.Camera.VerticalFramingBias = 0.22f; // sit the model lower in view
                return _modelPainter;

            case SampleAssetKind.Hdri:
                var bytes = File.ReadAllBytes(asset.PrimaryFilePath);
                var panorama = TextureImageLoader.LoadFloatImage(bytes, Path.GetExtension(asset.PrimaryFilePath));
                var newPainter = new PanoramaScenePainter(panorama);
                var old = _panoramaPainter;
                _panoramaPainter = newPainter;
                old?.Dispose();
                return newPainter;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private void RaiseSelectionChanged()
    {
        NotifyPropertyChanged(nameof(IsTextureSelected));
        NotifyPropertyChanged(nameof(IsHdriSelected));
        NotifyPropertyChanged(nameof(IsModelSelected));
        NotifyPropertyChanged(nameof(EngineSelectorVisibility));
    }

    private static string Label(SampleAssetKind kind) => kind switch
    {
        SampleAssetKind.Texture => "Texture",
        SampleAssetKind.Hdri => "HDRI",
        SampleAssetKind.Model => "Model",
        _ => kind.ToString(),
    };

    private static string Hint(SampleAssetKind kind) => kind switch
    {
        SampleAssetKind.Hdri => "drag to look around · scroll to zoom",
        _ => "drag to rotate · scroll to zoom",
    };
}
