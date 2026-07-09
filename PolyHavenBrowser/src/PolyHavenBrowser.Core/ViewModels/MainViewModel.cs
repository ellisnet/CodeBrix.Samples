using System;
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
    private readonly GlModelScenePainter _modelPainter = new();

    private IScenePainter _currentPainter;
    private PanoramaScenePainter _panoramaPainter;
    private SampleAssetKind _selectedKind = SampleAssetKind.Texture;
    private bool _isBusy;
    private string _statusText = "Starting up…";

    /// <summary>Creates the view model and begins loading the initial texture sample.</summary>
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; }

        _assets = GetService<SampleAssetService>();
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
        }
    }

    /// <summary>The busy indicator's visibility (visible while an asset is downloading/loading).</summary>
    public Visibility BusyVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>A short status line shown beneath the canvas.</summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    /// <summary>Whether the texture sample is selected (drives the button highlight).</summary>
    public bool IsTextureSelected => _selectedKind == SampleAssetKind.Texture;

    /// <summary>Whether the HDRI sample is selected.</summary>
    public bool IsHdriSelected => _selectedKind == SampleAssetKind.Hdri;

    /// <summary>Whether the model sample is selected.</summary>
    public bool IsModelSelected => _selectedKind == SampleAssetKind.Model;

    private SimpleCommand _selectTextureCommand;

    /// <summary>Selects and shows the sample texture (on a lit sphere).</summary>
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
                _modelPainter.FixedLightDirection = new Vector3(-0.4f, 1f, 0.7f);
                _modelPainter.Camera.FovDegrees = 45f;
                _modelPainter.Camera.YawDegrees = 35f;
                _modelPainter.Camera.PitchDegrees = 28f;
                return _modelPainter;

            case SampleAssetKind.Model:
                _modelPainter.SetModel(new GltfModelLoader().LoadFile(asset.PrimaryFilePath));
                _modelPainter.SetBackgroundTexture(null);
                _modelPainter.FixedLightDirection = null;
                _modelPainter.Camera.FovDegrees = 45f;
                _modelPainter.Camera.YawDegrees = 30f;
                _modelPainter.Camera.PitchDegrees = 15f;
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
