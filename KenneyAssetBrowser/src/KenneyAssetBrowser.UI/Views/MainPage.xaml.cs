using CodeBrix.Platform.Simple;
using CodeBrix.Platform.UI.FlexPanel;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using KenneyAssetBrowser.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Threading.Tasks;

namespace KenneyAssetBrowser.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    //Whether the "3D preview unavailable" dialog has been shown already (once per app run).
    private bool _renderingUnavailableReported;

    //Whether the current clip has run to its end. A finished clip leaves the transport parked
    //at the end, where Play() has nothing left to play, so the next Play rewinds first.
    private bool _audioPlaybackEnded;

    //How close to the duration still counts as "parked at the end". The player refreshes its
    //position on an interval (150 ms by default), so the last value it reports before ending
    //can sit just short of the duration.
    private static readonly TimeSpan AudioEndTolerance = TimeSpan.FromMilliseconds(250);

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            //(e.g. the bundle license dialog).
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            if (DataContext is MainViewModel viewModel)
            {
                //Marshal 2D-canvas invalidations from the view model onto the UI thread
                viewModel.InvalidateImageCanvas = () => DispatcherQueue?.TryEnqueue(() => ImageCanvas?.Invalidate());

                //Audio bridge: the view model hands over the clip's raw stream and transport
                //calls; the AudioPlayer element does the decoding and playing (it takes
                //stream ownership)
                viewModel.LoadAudioSource = stream =>
                {
                    _audioPlaybackEnded = false;
                    AudioElement?.SetSourceStream(stream);
                };
                viewModel.PlayAudio = PlayAudio;
                viewModel.PauseAudio = () => AudioElement?.Pause();
                viewModel.StopAudio = () =>
                {
                    _audioPlaybackEnded = false;
                    AudioElement?.Stop();
                };
                viewModel.SetAudioLooping = looping =>
                {
                    if (AudioElement != null) { AudioElement.IsLooping = looping; }
                };

                viewModel.PropertyChanged += (_, args) =>
                {
                    //A new cell collection means the user switched bundle, searched or
                    //re-filtered: jump back to the top.
                    if (args.PropertyName == nameof(MainViewModel.Cells))
                    {
                        CatalogScroll.ChangeView(null, 0, null, disableAnimation: true);
                    }

                    //The user just opened the Viewer View: if the GL canvas already knows its
                    //OpenGL initialization failed, tell them why the preview pane is empty.
                    if (args.PropertyName == nameof(MainViewModel.IsViewerActive))
                    {
                        _ = MaybeReportRenderingUnavailableAsync();
                    }
                };
            }
        };

        InitializeComponent();

        //A clip that plays through to its end parks the transport at the end; remember that so
        //the next Play can rewind instead of doing nothing. Looping clips raise this too, but
        //they keep playing, and PlayAudio only rewinds a player that has actually stopped.
        AudioElement.PlaybackEnded += (_, _) => _audioPlaybackEnded = true;

        //The canvas may only attempt its OpenGL initialization when it loads into the visual
        //tree, which can happen after IsViewerActive is set - so check at both moments.
        ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

        //The 2D viewer: the view model's painter draws images and spritesheets (checkerboard,
        //zoom, sprite spotlight) onto this SkiaSharp surface.
        ImageCanvas.PaintSurface += (_, e) =>
            ViewModel?.ImagePainter.Paint(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        ImageCanvas.SizeChanged += (_, _) => ImageCanvas.Invalidate();
        ImageCanvas.PointerWheelChanged += (_, e) =>
        {
            var delta = e.GetCurrentPoint(ImageCanvas).Properties.MouseWheelDelta;
            ViewModel?.AdjustZoomFromWheel(delta);
            e.Handled = true;
        };

        //The Viewer View's content panes: side-by-side while the window is landscape. In
        //portrait the FlexPanel's main axis flips so the viewer drops below the facts pane,
        //which trades its fixed-width column for half the height as a flex basis.
        SizeChanged += (_, args) =>
        {
            var portrait = args.NewSize.Width < args.NewSize.Height;
            ViewerContentFlex.Direction = portrait ? FlexDirection.Column : FlexDirection.Row;
            ViewerInfoPane.Width = portrait ? double.NaN : 380;
            FlexPanel.SetBasis(ViewerInfoPane,
                portrait ? new FlexBasis(0.5f, isRelative: true) : FlexBasis.Auto);
            ViewerInfoPane.Margin = portrait ? new Thickness(0, 0, 0, 20) : new Thickness(0, 0, 20, 0);
        };

        //Lazy grid loading: as the grid scrolls within two screens of its bottom edge,
        //ask the cell collection to materialize the next batch.
        CatalogScroll.ViewChanged += (_, _) =>
        {
            var cells = ViewModel?.Cells;
            if (cells == null || !cells.HasMoreItems) { return; }

            var remaining = CatalogScroll.ExtentHeight - CatalogScroll.VerticalOffset - CatalogScroll.ViewportHeight;
            if (remaining < CatalogScroll.ViewportHeight * 2)
            {
                cells.RequestMore(24);
            }
        };
    }

    //Starts (or resumes) the audio clip. A clip that has played through to its end leaves the
    //transport parked at the end, where Play() alone has nothing left to play - so rewind first
    //and let one click replay the clip. Two things deliberately do NOT rewind: a player that is
    //still going (a looping clip raises PlaybackEnded on every pass), and a clip the user has
    //scrubbed away from the end since it finished - there, the thumb is the intent, so resume
    //from where they left it.
    private void PlayAudio()
    {
        if (AudioElement == null) { return; }

        if (_audioPlaybackEnded
            && !AudioElement.IsPlaying
            && AudioElement.Duration > TimeSpan.Zero
            && AudioElement.Position >= AudioElement.Duration - AudioEndTolerance)
        {
            AudioElement.Seek(TimeSpan.Zero);
        }

        _audioPlaybackEnded = false;
        AudioElement.Play();
    }

    //When the Viewer View is active and the preview canvas reports failed OpenGL initialization,
    //surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
    private async Task MaybeReportRenderingUnavailableAsync()
    {
        if (_renderingUnavailableReported || ViewModel is not { IsViewerActive: true } viewModel)
        {
            return;
        }

        var state = ModelCanvas.GetGLInitializationState();
        if (state.Status == GLInitializationStatus.InitializationFailed)
        {
            _renderingUnavailableReported = true;
            await viewModel.ShowRenderingUnavailableAsync(state);
        }
    }
}

/// <summary>
/// Formats an AudioPlayer position/duration <see cref="TimeSpan"/> for the audio scrubber's
/// two timecode labels. The tenth of a second is deliberate: most of what an asset pack ships
/// is a sound effect well under a second long, and a plain m:ss would show "0:00 / 0:00" for
/// the whole clip.
/// </summary>
public sealed class TimecodeConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is TimeSpan time ? $"{(int)time.TotalMinutes}:{time.Seconds:00}.{time.Milliseconds / 100}" : "0:00.0";

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
