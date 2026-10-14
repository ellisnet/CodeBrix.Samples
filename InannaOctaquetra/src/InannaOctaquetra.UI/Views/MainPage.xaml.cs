using CodeBrix.Platform.Extensions;
using CodeBrix.Platform.Simple;
using InannaOctaquetra.Controls;
using InannaOctaquetra.Helpers;
using InannaOctaquetra.Reading.Data;
using InannaOctaquetra.Reading.Models;
using InannaOctaquetra.Services;
using InannaOctaquetra.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System; //Required: the IAsyncOperation GetAwaiter extension (awaiting the pickers) lives here
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Pickers;

namespace InannaOctaquetra.Views;

/// <summary>
/// The whole screen: the header, the altar with the eight-petalled Octaquetra, the deck rail,
/// the interpretation panel and the status strip. Every decision lives on
/// <see cref="MainViewModel"/>; this page owns the pictures — geometry, pointer mechanics,
/// animation — and reflects what the view model says has happened.
/// </summary>
public sealed partial class MainPage : Page
{
    // ------------------------------------------------------------- layout constants
    private const double Margin_ = 16;
    private const double Gap = 12;
    private const double RailWidth = 360;
    private const double DeckStackDepth = 4;

    /// <summary>
    /// Where the petal stations sit, as a share of the Octaquetra's outer radius: just outside each
    /// lobe's centre, so every card rests wholly inside its lobe.
    /// </summary>
    private const double StationShare = 0.68;

    /// <summary>Vertical room kept beyond the Octaquetra's rim for a petal title.</summary>
    private const double TitleBand = 26;

    /// <summary>Horizontal room kept beside the Octaquetra for the widest side title.</summary>
    private const double TitleWidth = 168;

    /// <summary>Gap between the Octaquetra's rim and the nearest edge of a petal title.</summary>
    private const double TitleGap = 9;

    /// <summary>Card height as a share of the station radius; the largest that clears the diagonal neighbours.</summary>
    private const double CardShare = 0.68;

    private double OctaquetraRadius => _radius / StationShare;

    private double OctaquetraRim => OctaquetraRadius;

    // ------------------------------------------------------------- state
    /// <summary>The visual for one card the view model says is on the table or in the tray.</summary>
    private sealed class CardItem
    {
        public required ReadingCard Model { get; init; }
        public required CardView View { get; init; }
    }

    private readonly List<CardItem> _items = new();
    private readonly List<CardView> _deckBacks = new();
    private readonly List<Rectangle> _slotRects = new();
    private readonly List<TextBlock> _slotLabels = new();

    private MainViewModel _wiredViewModel;

    //Set once the page has unloaded and disposed its view model; everything that reaches for
    //  the view model after that gets null instead of a disposed object
    private bool _viewModelDisposed;

    private ILogger _log = NullLogger.Instance;

    // the hand-drawn rail captions, refreshed in place when the view model says they changed
    private TextBlock _deckCountText;
    private TextBlock _trayCaptionText;

    //A resize that lands while the interpretation panel is up leaves the rail chrome behind;
    //  this remembers that it has to be rebuilt when the panel goes away
    private bool _railChromeStale;

    //Which run of the shuffle animation the visuals on screen belong to. A resize rebuilds the
    //  deck stack from scratch and the page unloading throws it away altogether, so anything
    //  animating a frame at a time checks this before it touches a visual it no longer owns.
    private int _shuffleGeneration;

    //The same token, for the blessing. A resize moves the altar out from under it, Clear and
    //  Shuffle take the table away, and drawing Inanna again starts it over: each of those ends
    //  the run that was playing, and every frame checks that it still owns the layer.
    private int _celebrationGeneration;

    // geometry, recomputed on every resize
    private double _cardW = 104, _cardH = 166;
    private double _centreX, _centreY, _radius;
    private double _altarX, _altarY, _altarW, _altarH;
    private double _railX, _railY, _railH;
    private double _deckX, _deckY, _deckW, _deckH;
    private double _trayY, _trayW, _trayH;
    private double _trayCardW = 80, _trayCardH = 128;

    // drag state
    private CardItem _dragItem;
    private Point _grab;
    private bool _dragging;
    private bool _dragMoved;
    private int _topZ = 20;
    private Rectangle _dragShadow;
    private int _highlightStation = -1;
    private CardItem _lastClickItem;
    private DateTime _lastClickAt = DateTime.MinValue;

    private Border _detailPanel;
    private CardItem _detailItem;
    private bool _detailFlipped;
    private bool _detailFlipping;
    private ScrollViewer _detailScroller;
    private bool _detailDragging;
    private Point _detailGrab;

    //The living sky over the velvet, the altar, the rail and the header
    private readonly Starfield _starfield = new();

    //The gold band of the Octaquetra, engraved in cuneiform; decoded once from the Reading library
    private BitmapImage _inscribedOctaquetra;

    //The shuffle ritual's random source (the deck keeps its own)
    private readonly Random _random = new();

    public MainPage()
    {
        //The logger factory App.InitializeLogging() wired into the platform, when there is one
        var loggerFactory = LogExtensionPoint.AmbientLoggerFactory;
        if (loggerFactory != null) { _log = loggerFactory.CreateLogger<MainPage>(); }

        //Doing this before InitializeComponent() - InitializeComponent() is the thing that
        //  sets the data context, from the <Page.DataContext> element in the XAML.
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //Give the view model the pictures, the file dialogs and the dialogs — the three
            //  things only a page can supply
            if (DataContext is IReadingTableBridge table)
            {
                table.CardAdded = ShowCardAdded;
                table.CardMoved = ShowCardMoved;
                table.CardFlipped = ShowCardFlipped;
                table.CardCelebrated = ShowCardCelebrated;
                table.TableCleared = ShowTableCleared;
                table.DeckShuffled = PlayShuffleAsync;
            }

            if (DataContext is IReadingFileBridge files)
            {
                files.PickSavePathAsync = PickSavePathAsync;
                files.PickReadingPathAsync = PickReadingPathAsync;
            }

            if (DataContext is IReadingDialogBridge dialogs)
            {
                dialogs.ConfirmAsync = ConfirmAsync;
                dialogs.ShowMessageAsync = ShowMessageAsync;
                dialogs.ShowReportSavedAsync = ShowReportSavedAsync;
            }

            WireViewModel();
        };

        Loaded += (_, _) =>
        {
            WireViewModel();
            Relayout();
            MaybeRunSnapshot();
        };

        //Nothing else owns the view model — the XAML declares it in <Page.DataContext> — so the
        //  page is what runs its teardown: the commands disposed and the bridge delegates the
        //  page handed it dropped, every one of which captures this page
        Unloaded += (_, _) => OnPageUnloaded();

        InitializeComponent();

        AppTitleText.Text = Ornament.Track("Octaquetra of Inanna");

        // the velvet itself, decoded from the Reading library
        var velvet = new BitmapImage();
        VelvetImage.Source = velvet;
        if (ArtAssets.VelvetJpeg.Length > 0) { _ = LoadArtAsync(velvet, ArtAssets.VelvetJpeg); }

        // the night sky over the velvet, and a fainter one over the altar beneath the cards
        _starfield.Attach(SkyLayer, density: 1.0, dust: true);
        _starfield.Attach(AltarSkyLayer, density: 0.9, dust: true);
        _starfield.Attach(RailSkyLayer, density: 0.8, dust: true);
        _starfield.Attach(HeaderSkyLayer, density: 0.9, dust: false);

        // On Android the sky shares the UI thread with a great deal of native drawing, so it steps
        // aside (holds still) while a deal, a shuffle, a card turning or a drag is playing
        _starfield.DuckingEnabled = OperatingSystem.IsAndroid();
        StaleBadgeText.Text = Ornament.Track("reading out of date");

        BuildLogo();
    }

    private MainViewModel ViewModel => _viewModelDisposed ? null : DataContext as MainViewModel;

    // ==================================================================== watching the view model

    /// <summary>
    /// The page is going. Anything still in flight — a detail panel, a drag, a fading
    /// animation, a dialog waiting to be closed — is let go of first, then the view model is
    /// disposed through <see cref="IDisposable"/>, so this line says nothing about what it
    /// happens to be holding; <c>MainViewModel.Dispose()</c> is the one place that knows.
    /// </summary>
    private void OnPageUnloaded()
    {
        CloseDetail();
        EndDrag();
        _shuffleGeneration++;
        EndCelebration();
        _starfield.Stop();
        UnwireViewModel();

        _viewModelDisposed = true;
        (DataContext as IDisposable)?.Dispose();
    }

    private void WireViewModel()
    {
        var viewModel = ViewModel;
        if (ReferenceEquals(viewModel, _wiredViewModel)) { return; }

        UnwireViewModel();
        if (viewModel == null) { return; }

        _wiredViewModel = viewModel;
        _wiredViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void UnwireViewModel()
    {
        if (_wiredViewModel == null) { return; }

        _wiredViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _wiredViewModel = null;
    }

    //The chrome that is drawn by hand onto the rail canvas cannot be bound, so the few pieces
    //  of it that follow view model state are refreshed from here
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.DeckRemaining):
                UpdateDeckStackVisibility();
                break;
            case nameof(MainViewModel.DeckCountText):
            case nameof(MainViewModel.TrayCaption):
                RefreshRailText();
                break;
            case nameof(MainViewModel.Interpretation):
                RenderInterpretation(ViewModel?.Interpretation);
                break;
            case nameof(MainViewModel.ShowInterpretationPanel):
                ApplyInterpretationPanel(ViewModel?.ShowInterpretationPanel == true);
                break;
        }
    }

    private void BuildLogo()
    {
        LogoBox.Child = CardView.LayerCanvas(
            EmblemArt.OctaquetraMotif, 100,
            Ornament.GoldLeaf(), null, Ornament.GoldLeaf());
    }

    // ==================================================================== geometry

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e) => Relayout();

    private void Relayout()
    {
        if (_viewModelDisposed) { return; }

        var w = ContentCanvas.ActualWidth;
        var h = ContentCanvas.ActualHeight;
        if (w < 200 || h < 200) { return; }

        //Everything below is about to be recomputed, and a blessing in flight was drawn for the
        //  geometry that is going away; it ends here rather than finishing somewhere odd
        EndCelebration();

        _altarX = Margin_;
        _altarY = Gap;
        _altarH = h - Gap * 2;
        _railX = w - Margin_ - RailWidth;
        _altarW = Math.Max(320, _railX - Gap - _altarX);
        _railY = Gap;
        _railH = _altarH;

        Place(AltarBorder, _altarX, _altarY, _altarW, _altarH);
        Canvas.SetLeft(AltarSkyLayer, _altarX + 6);
        Canvas.SetTop(AltarSkyLayer, _altarY + 6);
        AltarSkyLayer.Width = _altarW - 12;
        AltarSkyLayer.Height = _altarH - 12;
        _starfield.Resize(AltarSkyLayer, _altarW - 12, _altarH - 12);
        Canvas.SetLeft(RailSkyLayer, _railX + 4);
        Canvas.SetTop(RailSkyLayer, _railY + 4);
        RailSkyLayer.Width = RailWidth - 8;
        RailSkyLayer.Height = _railH - 8;
        _starfield.Resize(RailSkyLayer, RailWidth - 8, _railH - 8);
        Place(RailBorder, _railX, _railY, RailWidth, _railH);
        Place(InterpretationBorder, _railX, _railY, RailWidth, _railH);
        // the ScrollViewer on this head measures its child with an unbounded width, so the
        // text column is pinned explicitly or long paragraphs run off the panel edge
        InterpStack.Width = RailWidth - 50;

        // the spread: the Octaquetra's lobes sit on the petal stations, and every petal title sits in
        // a ring just outside the Octaquetra's rim. Size everything so that ring fits the altar.
        var rimPerStation = 1 / StationShare;
        var fromHeight = (_altarH / 2 - 8 - TitleBand) / rimPerStation;
        var fromWidth = (_altarW / 2 - 8 - TitleWidth) / rimPerStation;
        var stationRadius = Math.Min(fromHeight, fromWidth);
        // the diagonal neighbours are the tightest pair (their vertical gap is R x 0.707), so a card
        // may be up to about 0.68 of the station radius tall and still clear them
        _cardH = Math.Clamp(stationRadius * CardShare, 70, 300);
        _cardW = Math.Round(_cardH * 0.625);
        _cardH = Math.Round(_cardH);
        _radius = Math.Round(_cardH / CardShare);

        _centreX = _altarX + _altarW / 2;
        _centreY = _altarY + _altarH / 2;

        BuildOrnament();
        BuildSlots();

        //The tray and the deck stack are placed from this geometry whether or not the rail is
        //  the thing on show, so it is always current
        ComputeRailGeometry();

        if (ViewModel?.ShowInterpretationPanel == true)
        {
            //The rail layer is collapsed behind the interpretation panel: drawing chrome into
            //  it is work nobody sees, so it waits for "Back to deck"
            _railChromeStale = true;
        }
        else
        {
            BuildRailChrome();
        }

        RepositionAllCards();
    }

    private static void Place(FrameworkElement element, double x, double y, double w, double h)
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
        element.Width = Math.Max(0, w);
        element.Height = Math.Max(0, h);
    }

    /// <summary>Centre of station <paramref name="index"/> in ContentCanvas coordinates.</summary>
    private Point StationCentre(int index)
    {
        if (index <= 0) { return new Point(_centreX, _centreY); }
        var angle = AngleOf(index);
        var p = Ornament.Polar(_centreX, _centreY, _radius, angle);
        return new Point(p.X, p.Y);
    }

    private static double AngleOf(int index)
    {
        var positions = OctaquetraSpread.Positions;
        return index >= 0 && index < positions.Count
            ? positions[index].AngleDegrees
            : (index - 1) * 45.0;
    }

    // ==================================================================== the Octaquetra ornament

    private void BuildOrnament()
    {
        OrnamentLayer.Children.Clear();

        // the Octaquetra of Inanna beneath the spread: each of its eight loops is centred on a petal
        // station so it encircles that card, and all eight meet beneath the Heart. The size is
        // capped so the loops never leave the altar.
        var drawnOctaquetraRadius = OctaquetraRadius;

        // a faint outer circle, just beyond the Octaquetra's rim
        var outer = drawnOctaquetraRadius + 6;
        OrnamentLayer.Children.Add(new Ellipse
        {
            Width = outer * 2,
            Height = outer * 2,
            Stroke = Ornament.Brush(Ornament.GoldDeep, 0.22),
            StrokeThickness = 1,
        }.At(_centreX - outer, _centreY - outer));

        var geometry = EmblemArt.InannaOctaquetraPath(_centreX, _centreY, drawnOctaquetraRadius);

        // a soft glow around the gold leaf
        OrnamentLayer.Children.Add(new Microsoft.UI.Xaml.Shapes.Path
        {
            Data = CardView.ParseGeometry(geometry),
            Stroke = Ornament.Brush(Ornament.Gold, 0.05),
            StrokeThickness = drawnOctaquetraRadius * EmblemArt.OctaquetraStrokeRatio * 1.2,
            StrokeLineJoin = PenLineJoin.Round,
        });

        // the Octaquetra itself, exactly as authored, its gold band engraved along its whole length with
        // Inana's claiming of the ME (ETCSL c.1.3.1 Segment E) as one continuous strand; rendered by
        // Packaging/IconRenderer (ribbon mode)
        OrnamentLayer.Children.Add(new Image
        {
            Source = InscribedOctaquetra(),
            Width = drawnOctaquetraRadius * 2,
            Height = drawnOctaquetraRadius * 2,
            Stretch = Stretch.Fill,
            Opacity = 0.96,
            IsHitTestVisible = false,
        }.At(_centreX - drawnOctaquetraRadius, _centreY - drawnOctaquetraRadius));
    }

    //The engraved band is decoded once and shared by every relayout; until it has decoded (or if
    //  it cannot be read) the gold-leaf glow beneath it still draws the Octaquetra's shape
    private BitmapImage InscribedOctaquetra()
    {
        if (_inscribedOctaquetra != null) { return _inscribedOctaquetra; }

        _inscribedOctaquetra = new BitmapImage();
        var bytes = ArtAssets.InscribedOctaquetraPng;
        if (bytes.Length > 0) { _ = LoadArtAsync(_inscribedOctaquetra, bytes); }
        return _inscribedOctaquetra;
    }

    private async Task LoadArtAsync(BitmapImage image, byte[] bytes)
    {
        try
        {
            using var stream = new System.IO.MemoryStream(bytes, writable: false);
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }
        catch (Exception e)
        {
            _log.LogWarning(e, "A picture from the Reading library could not be decoded.");
        }
    }

    private void BuildSlots()
    {
        SlotLayer.Children.Clear();
        _slotRects.Clear();
        _slotLabels.Clear();

        for (int i = 0; i < 9; i++)
        {
            var centre = StationCentre(i);
            var rect = new Rectangle
            {
                Width = _cardW,
                Height = _cardH,
                RadiusX = 7,
                RadiusY = 7,
                Stroke = Ornament.Brush(Ornament.Gold, 0.55),
                StrokeThickness = 1.3,
                StrokeDashArray = new DoubleCollection { 4, 4 },
                Fill = Ornament.Brush(Ornament.Kohl, 0.3),
            };
            AutomationProperties.SetAutomationId(rect, $"Station{i}");
            Canvas.SetLeft(rect, centre.X - _cardW / 2);
            Canvas.SetTop(rect, centre.Y - _cardH / 2);
            SlotLayer.Children.Add(rect);
            _slotRects.Add(rect);

            const double labelW = TitleWidth + 8;
            const double labelH = 14;
            var label = new TextBlock
            {
                Text = StationLabel(i),
                Width = labelW,
                FontSize = 9.2,
                FontFamily = new FontFamily(Fonts.Regular),
                Foreground = Ornament.Brush(Ornament.GoldDeep, 0.95),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
            };

            if (i == 0)
            {
                // the Heart is the Octaquetra's centre, so its title stays beneath its card, on a
                // plate of the altar's own colour that the gold lines pass behind
                label.Width = double.NaN;
                var plate = new Border
                {
                    Background = Ornament.Brush("#F2120B22"),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 1, 8, 2),
                    Child = label,
                };
                plate.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
                var plateW = plate.DesiredSize.Width > 0 ? plate.DesiredSize.Width : 90;
                Canvas.SetLeft(plate, _centreX - plateW / 2);
                Canvas.SetTop(plate, _centreY + _cardH / 2 + 5);
                SlotLayer.Children.Add(plate);
                _slotLabels.Add(label);
                continue;
            }

            // a petal title sits just beyond the Octaquetra's rim on its own axis, reading away from
            // it. The title is measured and placed by its left edge, always left-aligned in a box
            // exactly its own width: the Android head draws right-aligned and centred text in a
            // wider box noticeably off from where the box says, which ran the evening star's
            // title under the band.
            var titleW = labelW;
            try
            {
                label.Width = double.NaN;
                label.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
                if (label.DesiredSize.Width > 1) { titleW = Math.Ceiling(label.DesiredSize.Width) + 2; }
            }
            catch (Exception) { /* keep the design width */ }
            label.Width = titleW;
            label.TextAlignment = TextAlignment.Left;

            var angle = AngleOf(i) * Math.PI / 180.0;
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            var anchor = PointAt(OctaquetraRim + TitleGap, AngleOf(i));

            double left, top;
            if (Math.Abs(sin) < 0.2) { left = anchor.X - titleW / 2; }
            else if (sin > 0) { left = anchor.X; }
            else { left = anchor.X - titleW; }

            if (Math.Abs(cos) < 0.2) { top = anchor.Y - labelH / 2; }
            else if (cos > 0) { top = anchor.Y - labelH; }
            else { top = anchor.Y; }

            Canvas.SetLeft(label, Math.Clamp(left, _altarX + 5, Math.Max(_altarX + 5, _altarX + _altarW - titleW - 5)));
            Canvas.SetTop(label, Math.Clamp(top, _altarY + 4, _altarY + _altarH - labelH - 4));
            SlotLayer.Children.Add(label);
            _slotLabels.Add(label);
        }

        UpdateSlotVisibility();
    }

    private Point PointAt(double radius, double angle)
    {
        var p = Ornament.Polar(_centreX, _centreY, radius, angle);
        return new Point(p.X, p.Y);
    }

    private static string StationLabel(int index)
    {
        var positions = OctaquetraSpread.Positions;
        if (index < 0 || index >= positions.Count)
        {
            return Ornament.Track(index == 0 ? "The Heart" : $"{Card.ToRoman(index)}");
        }

        var pos = positions[index];
        return index == 0
            ? Ornament.Track(pos.Title)
            : Ornament.Track($"{Card.ToRoman(index)} · {pos.Title}");
    }

    /// <summary>Fade a slot outline once a card sits on it.</summary>
    private void UpdateSlotVisibility()
    {
        for (int i = 0; i < _slotRects.Count && i < 9; i++)
        {
            var occupied = ItemAtStation(i) is not null;
            _slotRects[i].Opacity = occupied ? 0 : 1;
            _slotLabels[i].Opacity = occupied ? 0.75 : 1;
        }
    }

    private void SetStationHighlight(int index)
    {
        if (_highlightStation == index) { return; }
        if (_highlightStation >= 0 && _highlightStation < _slotRects.Count)
        {
            var old = _slotRects[_highlightStation];
            old.Stroke = Ornament.Brush(Ornament.Gold, 0.55);
            old.StrokeThickness = 1.3;
            old.Fill = Ornament.Brush(Ornament.Kohl, 0.3);
            old.Opacity = ItemAtStation(_highlightStation) is null ? 1 : 0;
        }
        _highlightStation = index;
        if (index >= 0 && index < _slotRects.Count)
        {
            var rect = _slotRects[index];
            rect.Opacity = 1;
            rect.Stroke = Ornament.Brush(Ornament.LapisGlow, 0.95);
            rect.StrokeThickness = 2.2;
            rect.Fill = Ornament.Brush(Ornament.LapisGlow, 0.14);
        }
    }

    // ==================================================================== the rail

    /// <summary>
    /// Where the deck stack and the tray sit; wanted even when the rail is hidden. The deck sits
    /// at the left with its count beside it, leaving the rest of the rail to the drawn cards,
    /// which are shown large enough to read.
    /// </summary>
    private void ComputeRailGeometry()
    {
        _deckH = Math.Round(Math.Clamp(_railH * 0.27, 104, 176));
        _deckW = Math.Round(_deckH * 0.625);
        _deckX = _railX + 30;
        _deckY = _railY + 52;

        _trayW = RailWidth - 36;
        _trayY = _deckY + _deckH + 56;
        var trayBottom = _railY + _railH - 30;
        _trayCardH = Math.Round(Math.Clamp(trayBottom - _trayY - 18, 120, 340));
        _trayCardW = Math.Round(_trayCardH * 0.625);
        if (_trayCardW > _trayW * 0.62)
        {
            _trayCardW = Math.Round(_trayW * 0.62);
            _trayCardH = Math.Round(_trayCardW / 0.625);
        }
        _trayH = _trayCardH + 14;
    }

    private void BuildRailChrome()
    {
        RailLayer.Children.Clear();
        _railChromeStale = false;

        ComputeRailGeometry();

        RailLayer.Children.Add(Head(Ornament.Track("The Deck"), _railX + 18, _railY + 17, RailWidth - 36));
        RailLayer.Children.Add(Rule(_railX + 18, _railY + 33, RailWidth - 36));

        var textX = _deckX + _deckW + 32;
        var textW = _railX + RailWidth - 18 - textX;
        var count = Caption(ViewModel?.DeckCountText ?? string.Empty, textX, _deckY + _deckH / 2 - 24,
            textW, 14, Ornament.Gold);
        AutomationProperties.SetAutomationId(count, "DeckCount");
        RailLayer.Children.Add(count);
        _deckCountText = count;

        RailLayer.Children.Add(Caption("Click the stack to draw a card", textX, _deckY + _deckH / 2 + 2,
            textW, 10.5, Ornament.Ivory, 0.55, wrap: true));

        RailLayer.Children.Add(Head(Ornament.Track("Drawn Cards"), _railX + 18, _trayY - 34, RailWidth - 36));
        RailLayer.Children.Add(Rule(_railX + 18, _trayY - 18, RailWidth - 36));

        var tray = Caption(ViewModel?.TrayCaption ?? string.Empty, _railX + 18, _trayY + _trayH + 4,
            RailWidth - 36, 10.5, Ornament.Ivory, 0.55);
        AutomationProperties.SetAutomationId(tray, "TrayCaption");
        RailLayer.Children.Add(tray);
        _trayCaptionText = tray;

        // a closing ornament at the foot of the rail, if there is room
        var footY = _trayY + _trayH + 46;
        if (footY < _railY + _railH - 90)
        {
            var vb = new Viewbox { Width = 46, Height = 46, Stretch = Stretch.Uniform, Opacity = 0.42 };
            vb.Child = CardView.LayerCanvas(EmblemArt.OctaquetraMotif, 100,
                Ornament.GoldLeaf(), null, Ornament.GoldLeaf());
            Canvas.SetLeft(vb, _railX + RailWidth / 2 - 23);
            Canvas.SetTop(vb, footY);
            RailLayer.Children.Add(vb);

            RailLayer.Children.Add(Caption(
                "Drag a card onto a petal. Double-click to reverse it. Click for its lore.",
                _railX + 26, footY + 56, RailWidth - 52, 10.5, Ornament.Ivory, 0.45, wrap: true));
        }

        BuildDeckStack();
        RefreshRailText();
    }

    private static TextBlock Head(string text, double x, double y, double width)
    {
        var t = new TextBlock
        {
            Text = text,
            Width = width,
            FontSize = 10.5,
            FontFamily = new FontFamily(Fonts.Bold),
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = Ornament.Brush(Ornament.Gold, 0.95),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
        };
        Canvas.SetLeft(t, x);
        Canvas.SetTop(t, y);
        return t;
    }

    private static Rectangle Rule(double x, double y, double width)
    {
        var r = new Rectangle { Width = width, Height = 0.8, Fill = Ornament.Brush(Ornament.GoldDeep, 0.35) };
        Canvas.SetLeft(r, x);
        Canvas.SetTop(r, y);
        return r;
    }

    private static TextBlock Caption(string text, double x, double y, double width, double size,
                                     string colour, double opacity = 1, bool wrap = false)
    {
        var t = new TextBlock
        {
            Text = text,
            Width = width,
            FontSize = size,
            FontFamily = new FontFamily(Fonts.Regular),
            Foreground = Ornament.Brush(colour, opacity),
            TextAlignment = TextAlignment.Center,
            TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            LineHeight = wrap ? size * 1.55 : 0,
        };
        Canvas.SetLeft(t, x);
        Canvas.SetTop(t, y);
        return t;
    }

    private void BuildDeckStack()
    {
        //Every back on screen is about to be thrown away, so a shuffle that is part way through
        //  stops here rather than going on moving visuals that are no longer in the tree
        _shuffleGeneration++;

        foreach (var back in _deckBacks) { CardLayer.Children.Remove(back); }
        _deckBacks.Clear();

        for (int i = 0; i < DeckStackDepth; i++)
        {
            var view = new CardView
            {
                IsFaceUp = false,
                CardWidth = _deckW,
                CardHeight = _deckH,
                EnableHover = i == DeckStackDepth - 1,
            };
            view.Rebuild();

            //The top card of the stack is the one a click lands on
            if (i == DeckStackDepth - 1) { AutomationProperties.SetAutomationId(view, "DeckStack"); }

            var offset = (DeckStackDepth - 1 - i) * 3.5;
            Canvas.SetLeft(view, _deckX + offset);
            Canvas.SetTop(view, _deckY - offset);
            Canvas.SetZIndex(view, 5 + i);
            view.PointerPressed += OnDeckPressed;
            CardLayer.Children.Add(view);
            _deckBacks.Add(view);
        }

        UpdateDeckStackVisibility();
    }

    private void UpdateDeckStackVisibility()
    {
        if (ViewModel?.ShowInterpretationPanel == true)
        {
            foreach (var back in _deckBacks) { back.Visibility = Visibility.Collapsed; }
            return;
        }

        var remaining = ViewModel?.DeckRemaining ?? 0;
        for (int i = 0; i < _deckBacks.Count; i++)
        {
            var needed = _deckBacks.Count - i;   // the bottom-most back needs 4 cards, the top 1
            _deckBacks[i].Visibility = remaining >= needed || (remaining > 0 && i == _deckBacks.Count - 1)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void OnDeckPressed(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
        CloseDetail();

        //The command owns the gate; the page only asks it
        var draw = ViewModel?.DrawCommand;
        if (draw?.CanExecute(null) == true) { draw.Execute(null); }
    }

    private void RefreshRailText()
    {
        var viewModel = ViewModel;
        if (viewModel is null) { return; }
        if (_deckCountText is not null) { _deckCountText.Text = viewModel.DeckCountText; }
        if (_trayCaptionText is not null) { _trayCaptionText.Text = viewModel.TrayCaption; }
    }

    // ==================================================================== what the view model says changed

    private CardItem ItemFor(ReadingCard model) => _items.FirstOrDefault(i => ReferenceEquals(i.Model, model));

    private CardItem ItemAtStation(int index) =>
        index < 0 ? null : _items.FirstOrDefault(i => i.Model.Station == index);

    /// <summary>A card has left the deck: build its visual where the view model says it belongs.</summary>
    private void ShowCardAdded(ReadingCard model)
    {
        var inTray = model.InTray;
        var view = new CardView
        {
            Card = model.Card,
            IsFaceUp = !inTray,
            CardWidth = inTray ? Math.Round(_cardW * 0.86) : _cardW,
            CardHeight = inTray ? Math.Round(_cardH * 0.86) : _cardH,
        };
        view.Rebuild();
        AutomationProperties.SetName(view, model.Card.Name);
        AttachCardHandlers(view);

        var item = new CardItem { Model = model, View = view };
        _items.Add(item);
        Canvas.SetLeft(view, _deckX);
        Canvas.SetTop(view, _deckY);
        Canvas.SetZIndex(view, ++_topZ);
        CardLayer.Children.Add(view);

        if (inTray)
        {
            // dealt onto the tray face down, then turned over where it lies
            LayoutTray();
            _starfield.Duck(TimeSpan.FromMilliseconds(600));
            view.FlipToFaceUp();
            return;
        }

        // a reading that was opened from a file arrives already laid
        PlaceVisual(item, animate: true);
    }

    /// <summary>A card is now on a station, or back in the tray.</summary>
    private void ShowCardMoved(ReadingCard model, bool animate)
    {
        var item = ItemFor(model);
        if (item is null) { return; }

        if (model.InTray)
        {
            item.View.ShowCloseButton = false;
            LayoutTray();
            UpdateSlotVisibility();
            return;
        }

        PlaceVisual(item, animate);
    }

    /// <summary>A card was turned the other way up.</summary>
    private void ShowCardFlipped(ReadingCard model)
    {
        _starfield.Duck(TimeSpan.FromMilliseconds(500));
        ItemFor(model)?.View.ToggleReversed();
    }

    /// <summary>Every card went back into the deck.</summary>
    private void ShowTableCleared()
    {
        CloseDetail();
        EndDrag();
        EndCelebration();
        foreach (var item in _items) { CardLayer.Children.Remove(item.View); }
        _items.Clear();
        UpdateSlotVisibility();
    }

    // ==================================================================== the shuffle

    /// <summary>One tray card on its way home, with where it set off from.</summary>
    private sealed class Flight
    {
        public required CardView View { get; init; }
        public required double FromX { get; init; }
        public required double FromY { get; init; }
        public required double FromW { get; init; }
        public required double FromH { get; init; }
        public required double FromFan { get; init; }
    }

    // the tray flies home before the shuffle ritual begins
    private const int FlyHomeMs = 220;

    /// <summary>
    /// The view model has shuffled the deck. The cards it hands over were in the tray and are
    /// already back in the stack, so their visuals fly home and are dropped; then the shuffle
    /// ritual plays over the deck (<see cref="ShuffleRitual"/>: a golden glow, eight backs
    /// riffling three times, dust, sparkles, circling motes and a burst of light) and the task
    /// completes, which is what lets the view model put its busy flag down.
    /// </summary>
    /// <param name="returned">The tray cards that went back into the deck; may be empty.</param>
    private async Task PlayShuffleAsync(IReadOnlyList<ReadingCard> returned)
    {
        // the sky holds still for the whole shuffle - the fly-home and the ritual
        using var quietSky = _starfield.Hold();

        CloseDetail();
        EndDrag();
        EndCelebration();

        var generation = ++_shuffleGeneration;
        try
        {
            await FlyTrayHome(returned, generation);
            if (ShuffleLives(generation)) { await PlayShuffleRitualAsync(); }
        }
        catch (Exception e)
        {
            //A dropped animation is never worth failing the command over; the finally below is
            //  what actually leaves the scene tidy
            _log.LogWarning(e, "The shuffle animation did not finish.");
        }
        finally
        {
            DropReturnedVisuals(returned);
            SettleDeckStack();
            LayoutTray();
            UpdateSlotVisibility();
        }
    }

    //The tray cards slide and shrink into the deck stack, turning face down half way, which is
    //  what says "these went back" rather than "these vanished"
    private async Task FlyTrayHome(IReadOnlyList<ReadingCard> returned, int generation)
    {
        var flights = new List<Flight>();
        foreach (var model in returned ?? Array.Empty<ReadingCard>())
        {
            var item = ItemFor(model);
            if (item is null) { continue; }

            item.View.ShowCloseButton = false;
            item.View.ClearHover();
            item.View.IsHitTestVisible = false;
            flights.Add(new Flight
            {
                View = item.View,
                FromX = Canvas.GetLeft(item.View),
                FromY = Canvas.GetTop(item.View),
                FromW = item.View.CardWidth,
                FromH = item.View.CardHeight,
                FromFan = item.View.FanAngle,
            });
        }

        if (flights.Count == 0) { return; }

        await Ease(FlyHomeMs, t =>
        {
            if (!ShuffleLives(generation)) { return; }
            foreach (var flight in flights)
            {
                var view = flight.View;
                Canvas.SetLeft(view, flight.FromX + (_deckX - flight.FromX) * t);
                Canvas.SetTop(view, flight.FromY + (_deckY - flight.FromY) * t);
                view.CardWidth = flight.FromW + (_deckW - flight.FromW) * t;
                view.CardHeight = flight.FromH + (_deckH - flight.FromH) * t;
                view.FanAngle = flight.FromFan * (1 - t);
                if (t >= 0.5) { view.IsFaceUp = false; }
            }
        });
    }

    //The ritual plays over the deck while the real stack is hidden; SettleDeckStack brings the
    //  stack back once it is over, however it ended
    private async Task PlayShuffleRitualAsync()
    {
        foreach (var back in _deckBacks) { back.Visibility = Visibility.Collapsed; }
        await ShuffleRitual.PlayAsync(ContentCanvas, _deckX, _deckY, _deckW, _deckH, _random);
    }

    //The visuals for the cards that went back; the view model has already forgotten them
    private void DropReturnedVisuals(IReadOnlyList<ReadingCard> returned)
    {
        foreach (var model in returned ?? Array.Empty<ReadingCard>())
        {
            var item = ItemFor(model);
            if (item is null) { continue; }
            CardLayer.Children.Remove(item.View);
            _items.Remove(item);
        }
    }

    /// <summary>Puts every back of the deck stack square again and shows it, after the ritual.</summary>
    private void SettleDeckStack()
    {
        for (var i = 0; i < _deckBacks.Count; i++)
        {
            var view = _deckBacks[i];
            var offset = (DeckStackDepth - 1 - i) * 3.5;
            Canvas.SetLeft(view, _deckX + offset);
            Canvas.SetTop(view, _deckY - offset);
            view.FanAngle = 0;
            view.LiftScale = 1.0;
        }

        UpdateDeckStackVisibility();
    }

    //False once the page has gone, or once a resize has rebuilt the stack under a running shuffle
    private bool ShuffleLives(int generation) => !_viewModelDisposed && _shuffleGeneration == generation;

    /// <summary>
    /// Runs <paramref name="onFrame"/> once a frame for <paramref name="milliseconds"/>, handing
    /// it an eased 0..1. The awaits resume on the UI thread the command came in on, which is
    /// what makes it safe to move visuals from inside the callback.
    /// </summary>
    private static async Task Ease(int milliseconds, Action<double> onFrame)
    {
        const int frameMs = 16;

        //A delay is only ever at least as long as it was asked for - the system timer is coarser
        //  than a frame - so how far along the animation is comes off the clock, not off a count
        //  of frames, and the whole thing takes the time it said it would
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < milliseconds)
        {
            await Task.Delay(frameMs);
            var linear = Math.Min(1, clock.ElapsedMilliseconds / (double)milliseconds);
            //ease-out cubic: quick away, slow into place
            onFrame(1 - Math.Pow(1 - linear, 3));
        }

        onFrame(1);
    }

    // ==================================================================== the blessing

    // The radiance and the sparks take this long; the words are timed separately and are what
    // makes the whole celebration a little over two seconds. The two run together.
    private const int BloomMs = 1100;
    private const int BlessingWaitMs = 140;
    private const int BlessingRiseMs = 520;
    private const int BlessingHoldMs = 950;
    private const int BlessingFadeMs = 480;

    // how many sparks leave the card, and how far the words travel as they come and go
    private const int SparkCount = 9;
    private const double BlessingRise = 26;

    /// <summary>One spark on its way out of the card, with the line it follows.</summary>
    private sealed class Spark
    {
        public required Microsoft.UI.Xaml.Shapes.Path Shape { get; init; }
        public required double AngleDegrees { get; init; }
        public required double Reach { get; init; }
        public required double Size { get; init; }
    }

    /// <summary>
    /// The deck's own goddess has been drawn. The view model does not wait for this, so it is
    /// started here and observed here: a dropped animation is a picture that did not finish, not
    /// a failed draw, and the layer is left empty either way.
    /// </summary>
    /// <param name="model">The card that was drawn; the radiance blooms out of its visual.</param>
    /// <param name="blessing">The words to say over the altar, as the view model wrote them.</param>
    private void ShowCardCelebrated(ReadingCard model, string blessing)
    {
        _ = PlayCelebrationAsync(model, blessing);
    }

    private async Task PlayCelebrationAsync(ReadingCard model, string blessing)
    {
        if (_viewModelDisposed || model is null) { return; }

        // the sky holds still while the blessing plays
        using var quietSky = _starfield.Hold();

        //Whatever was playing is over: drawing Inanna twice in a row shows the second blessing
        //  from the beginning rather than two of them on top of one another
        EndCelebration();
        var generation = _celebrationGeneration;

        var pieces = new List<UIElement>();
        try
        {
            await Task.WhenAll(
                PlayBloomAsync(generation, model, pieces),
                PlayBlessingAsync(generation, blessing, pieces));
        }
        catch (Exception e)
        {
            //Exactly as the shuffle does: the finally below is what leaves the layer tidy
            _log.LogWarning(e, "The blessing animation did not finish.");
        }
        finally
        {
            foreach (var piece in pieces) { RemoveCelebrationPiece(piece); }
        }
    }

    /// <summary>
    /// Ends whatever is playing and empties the layer. Called by anything that moves the ground
    /// under it — a resize, a clear, a shuffle, the page unloading, the next blessing.
    /// </summary>
    private void EndCelebration()
    {
        _celebrationGeneration++;
        CelebrationLayer.Children.Clear();
    }

    //False once the page has gone, or once anything at all has taken the layer over
    private bool CelebrationLives(int generation) =>
        !_viewModelDisposed && _celebrationGeneration == generation;

    private void RemoveCelebrationPiece(UIElement piece)
    {
        //EndCelebration may have emptied the layer already; removing twice is not an error
        try { CelebrationLayer.Children.Remove(piece); }
        catch (Exception) { /* the layer no longer holds it */ }
    }

    private T AddCelebrationPiece<T>(T piece, List<UIElement> pieces) where T : UIElement
    {
        piece.IsHitTestVisible = false;
        piece.Opacity = 0;
        CelebrationLayer.Children.Add(piece);
        pieces.Add(piece);
        return piece;
    }

    /// <summary>The centre of the drawn card while it is still on the table; its last place after.</summary>
    private Point CelebrationOrigin(ReadingCard model, Point fallback)
    {
        var view = ItemFor(model)?.View;
        if (view is null) { return fallback; }

        var x = Canvas.GetLeft(view);
        var y = Canvas.GetTop(view);
        if (double.IsNaN(x) || double.IsNaN(y)) { return fallback; }

        return new Point(x + view.CardWidth / 2, y + view.CardHeight / 2);
    }

    /// <summary>
    /// Gold breaks out of the card that was drawn: two rings widening and thinning, the
    /// eight-pointed star of the goddess opening through them, and a handful of sparks carried
    /// outwards. The centre is read again every frame, so an Auto-lay that carries the card off
    /// to its petal takes the radiance with it.
    /// </summary>
    private async Task PlayBloomAsync(int generation, ReadingCard model, List<UIElement> pieces)
    {
        var span = Math.Max(52, _cardH * 0.92);
        var origin = CelebrationOrigin(model, new Point(_deckX + _deckW / 2, _deckY + _deckH / 2));

        var rings = new List<Ellipse>();
        for (var i = 0; i < 2; i++)
        {
            rings.Add(AddCelebrationPiece(new Ellipse
            {
                Stroke = Ornament.Brush(i == 0 ? Ornament.GoldPale : Ornament.Gold),
                StrokeThickness = 3,
            }, pieces));
        }

        var star = AddCelebrationPiece(new Viewbox
        {
            Stretch = Stretch.Uniform,
            Child = CardView.LayerCanvas(EmblemArt.VenusStar, 100,
                Ornament.Brush(Ornament.Gold),
                Ornament.Brush(Ornament.GoldPale),
                Ornament.Brush(Ornament.GoldPale, 0.9)),
        }, pieces);

        var sparks = new List<Spark>();
        for (var i = 0; i < SparkCount; i++)
        {
            var size = i % 2 == 0 ? 7.0 : 4.6;
            sparks.Add(new Spark
            {
                Shape = AddCelebrationPiece(new Microsoft.UI.Xaml.Shapes.Path
                {
                    Data = CardView.ParseGeometry(
                        Ornament.StarPath(size, size, size, size * 0.34, 4, i * 9.0)),
                    Fill = Ornament.Brush(i % 3 == 0 ? Ornament.GoldPale : Ornament.Gold),
                }, pieces),
                AngleDegrees = i * (360.0 / SparkCount) + 12,
                Reach = span * (i % 3 == 0 ? 2.3 : i % 3 == 1 ? 1.85 : 2.6),
                Size = size,
            });
        }

        await Ease(BloomMs, t =>
        {
            if (!CelebrationLives(generation)) { return; }

            //Ease() hands out an eased 0..1, which is the right curve for how far a thing has
            //  travelled and the wrong one for how bright it still is: undo it for the fades, so
            //  the gold goes out evenly over the whole second rather than in the first quarter.
            var p = 1 - Math.Cbrt(Math.Max(0, 1 - t));
            var centre = CelebrationOrigin(model, origin);

            for (var i = 0; i < rings.Count; i++)
            {
                var ring = rings[i];
                var grow = i == 0 ? t : Math.Clamp((t - 0.16) / 0.84, 0, 1);
                var fade = i == 0 ? p : Math.Clamp((p - 0.18) / 0.82, 0, 1);
                var r = span * (0.30 + 1.55 * grow);
                ring.Width = r * 2;
                ring.Height = r * 2;
                ring.StrokeThickness = Math.Max(0.6, 3.4 * (1 - grow));
                ring.Opacity = (i == 0 ? 0.95 : 0.6) * (1 - fade);
                Canvas.SetLeft(ring, centre.X - r);
                Canvas.SetTop(ring, centre.Y - r);
            }

            var starSize = span * (0.55 + 1.75 * t);
            star.Width = starSize;
            star.Height = starSize;
            star.Opacity = p < 0.28 ? p / 0.28 : Math.Max(0, 1 - (p - 0.28) / 0.72);
            Canvas.SetLeft(star, centre.X - starSize / 2);
            Canvas.SetTop(star, centre.Y - starSize / 2);

            foreach (var spark in sparks)
            {
                var distance = spark.Reach * (0.18 + 0.82 * t);
                var at = Ornament.Polar(centre.X, centre.Y, distance, spark.AngleDegrees);
                Canvas.SetLeft(spark.Shape, at.X - spark.Size);
                Canvas.SetTop(spark.Shape, at.Y - spark.Size);
                spark.Shape.Opacity = p < 0.12 ? p / 0.12 : Math.Max(0, 1 - (p - 0.12) / 0.88);
            }
        });
    }

    /// <summary>
    /// The words, over the altar: they wait for the radiance to open, fade and rise into place,
    /// hold, and go up and out. The dark backing is what lets gold read over the Octaquetra.
    /// </summary>
    private async Task PlayBlessingAsync(int generation, string blessing, List<UIElement> pieces)
    {
        if (string.IsNullOrWhiteSpace(blessing)) { return; }

        var banner = AddCelebrationPiece(BuildBlessingBanner(blessing), pieces);

        //A Canvas child is never measured for us, so the height the words actually set to has to
        //  be asked for before the banner can be centred on the altar
        banner.Measure(new Windows.Foundation.Size(banner.Width, double.PositiveInfinity));
        var height = banner.DesiredSize.Height > 8 ? banner.DesiredSize.Height : 74;

        var left = _centreX - banner.Width / 2;
        var settled = _centreY - height / 2;
        Canvas.SetLeft(banner, left);
        Canvas.SetTop(banner, settled + BlessingRise);

        await Task.Delay(BlessingWaitMs);
        if (!CelebrationLives(generation)) { return; }

        await Ease(BlessingRiseMs, t =>
        {
            if (!CelebrationLives(generation)) { return; }
            banner.Opacity = t;
            Canvas.SetTop(banner, settled + BlessingRise * (1 - t));
        });
        if (!CelebrationLives(generation)) { return; }

        await Task.Delay(BlessingHoldMs);
        if (!CelebrationLives(generation)) { return; }

        await Ease(BlessingFadeMs, t =>
        {
            if (!CelebrationLives(generation)) { return; }
            banner.Opacity = 1 - t;
            Canvas.SetTop(banner, settled - BlessingRise * 0.55 * t);
        });
    }

    private Border BuildBlessingBanner(string blessing)
    {
        //As wide as the altar allows, and never so wide that the line loses its middle
        var width = Math.Clamp(_altarW - 48, 240, 620);
        var size = Math.Clamp(width / 20.5, 13, 27);
        var inner = width - 56;

        var stack = new StackPanel { Spacing = 8, Width = inner };
        stack.Children.Add(new Rectangle { Height = 1, Fill = Ornament.Brush(Ornament.GoldDeep, 0.6) });
        stack.Children.Add(new TextBlock
        {
            Text = blessing,
            Width = inner,
            FontSize = size,
            FontFamily = new FontFamily(Fonts.Bold),
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = Ornament.Brush(Ornament.Gold),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = Math.Round(size * 1.34),
        });
        stack.Children.Add(new Rectangle { Height = 1, Fill = Ornament.Brush(Ornament.GoldDeep, 0.6) });

        return new Border
        {
            Width = width,
            //The Octaquetra is behind the words; a dark ground and a gold hairline are what keep
            //  them legible over it, exactly as the card detail panel does over the altar
            Background = Ornament.Brush(Ornament.Kohl, 0.88),
            BorderBrush = Ornament.Brush(Ornament.Gold, 0.7),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(28, 15, 28, 17),
            Child = stack,
        };
    }

    // ==================================================================== placement

    private void PlaceVisual(CardItem item, bool animate)
    {
        var index = item.Model.Station;
        if (index < 0 || index >= 9) { return; }

        item.View.IsFaceUp = true;
        item.View.ShowCloseButton = true;
        item.View.CardWidth = _cardW;
        item.View.CardHeight = _cardH;
        item.View.FanAngle = 0;
        item.View.Rebuild();
        item.View.IsReversed = item.Model.IsReversed;

        var centre = StationCentre(index);
        Canvas.SetLeft(item.View, centre.X - _cardW / 2);
        Canvas.SetTop(item.View, centre.Y - _cardH / 2);
        Canvas.SetZIndex(item.View, ++_topZ);
        if (animate)
        {
            _starfield.Duck(TimeSpan.FromMilliseconds(700));
            item.View.PlayAppear();
        }
        Starfield.Burst(SparkleLayer, centre.X, centre.Y, _cardW * 0.9);

        LayoutTray();
        UpdateSlotVisibility();
    }

    private void LayoutTray()
    {
        var tray = _items.Where(i => i.Model.InTray).ToList();
        var available = _trayW;
        var n = tray.Count;

        // as large as the tray allows, but each card in the fan keeps at least half its face visible
        var cardW = _trayCardW;
        if (n > 1)
        {
            var fit = available / (1 + 0.5 * (n - 1));
            cardW = Math.Round(Math.Max(Math.Min(_trayCardW, 110), Math.Min(_trayCardW, fit)));
        }
        var cardH = Math.Round(cardW / 0.625);
        var top = _trayY + 8 + Math.Max(0, (_trayCardH - cardH) / 2);

        for (int i = 0; i < n; i++)
        {
            var item = tray[i];
            item.View.CardWidth = cardW;
            item.View.CardHeight = cardH;
            item.View.ShowCloseButton = false;

            double spacing = n <= 1 ? 0 : Math.Min(cardW + 12, (available - cardW) / (n - 1));
            double totalWidth = cardW + spacing * (n - 1);
            double startX = _railX + 18 + Math.Max(0, (available - totalWidth) / 2);
            double x = startX + spacing * i;

            double mid = (n - 1) / 2.0;
            double fan = n <= 1 ? 0 : (i - mid) * 3.2;
            double lift = n <= 1 ? 0 : Math.Abs(i - mid) * 2.2;

            Canvas.SetLeft(item.View, x);
            Canvas.SetTop(item.View, top + lift);
            Canvas.SetZIndex(item.View, 12 + i);
            item.View.FanAngle = fan;
            item.View.IsReversed = item.Model.IsReversed;
        }
    }

    private void RepositionAllCards()
    {
        foreach (var item in _items)
        {
            if (item.Model.InTray) { continue; }
            item.View.CardWidth = _cardW;
            item.View.CardHeight = _cardH;
            item.View.FanAngle = 0;
            var centre = StationCentre(item.Model.Station);
            Canvas.SetLeft(item.View, centre.X - _cardW / 2);
            Canvas.SetTop(item.View, centre.Y - _cardH / 2);
        }
        LayoutTray();
        UpdateSlotVisibility();
    }

    // ==================================================================== drag & drop

    private void AttachCardHandlers(CardView view)
    {
        view.PointerPressed += OnCardPressed;
        view.PointerMoved += OnCardMoved;
        view.PointerReleased += OnCardReleased;
        view.PointerCaptureLost += OnCardCaptureLost;
        view.CloseRequested += OnCardCloseRequested;
        view.PointerEntered += OnCardEntered;
        view.PointerExited += OnCardExited;
    }

    /// <summary>Tray cards overlap in a fan; the one under the pointer comes forward to be read.</summary>
    private void OnCardEntered(object sender, PointerRoutedEventArgs e)
    {
        if (_dragging) { return; }
        var item = ItemOf(sender);
        if (item is null || !item.Model.InTray) { return; }
        Canvas.SetZIndex(item.View, 60);
    }

    private void OnCardExited(object sender, PointerRoutedEventArgs e)
    {
        if (_dragging) { return; }
        var item = ItemOf(sender);
        if (item is null || !item.Model.InTray) { return; }
        var index = _items.Where(i => i.Model.InTray).ToList().IndexOf(item);
        if (index >= 0) { Canvas.SetZIndex(item.View, 12 + index); }
    }

    private CardItem ItemOf(object sender) => _items.FirstOrDefault(i => ReferenceEquals(i.View, sender));

    private void OnCardCloseRequested(object sender, EventArgs e)
    {
        var item = ItemOf(sender);
        if (item is null || item.Model.InTray) { return; }
        CloseDetail();
        ViewModel?.ReturnToTray(item.Model, announce: true);
    }

    private void OnCardPressed(object sender, PointerRoutedEventArgs e)
    {
        var item = ItemOf(sender);
        if (item is null) { return; }

        var pp = e.GetCurrentPoint(ContentCanvas);
        if (!pp.Properties.IsLeftButtonPressed) { return; }
        e.Handled = true;

        // double click toggles the orientation
        var now = DateTime.UtcNow;
        if (ReferenceEquals(_lastClickItem, item) && (now - _lastClickAt).TotalMilliseconds < 420)
        {
            _lastClickItem = null;
            _lastClickAt = DateTime.MinValue;
            ViewModel?.ToggleReversed(item.Model);

            //The lore panel stays where the reader put it and turns to the other meaning
            if (_detailPanel is not null && ReferenceEquals(_detailItem, item))
            {
                ShowDetail(item, animate: false, keepPosition: true);
            }
            return;
        }
        _lastClickItem = item;
        _lastClickAt = now;

        _dragItem = item;
        _dragMoved = false;
        _grab = new Point(pp.Position.X - Canvas.GetLeft(item.View), pp.Position.Y - Canvas.GetTop(item.View));
        Canvas.SetZIndex(item.View, 900);
        _dragging = item.View.CapturePointer(e.Pointer);
    }

    private void OnCardMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_dragging || _dragItem is null) { return; }
        e.Handled = true;

        var p = e.GetCurrentPoint(ContentCanvas).Position;
        var x = p.X - _grab.X;
        var y = p.Y - _grab.Y;

        // a card under the finger is the thing being watched; the sky waits until it is let go
        _starfield.Duck(TimeSpan.FromMilliseconds(300));

        if (!_dragMoved)
        {
            var dx = x - Canvas.GetLeft(_dragItem.View);
            var dy = y - Canvas.GetTop(_dragItem.View);
            if (Math.Abs(dx) + Math.Abs(dy) < 4) { return; }
            BeginDragVisuals(_dragItem);
        }

        Canvas.SetLeft(_dragItem.View, x);
        Canvas.SetTop(_dragItem.View, y);
        if (_dragShadow is not null)
        {
            Canvas.SetLeft(_dragShadow, x + 5);
            Canvas.SetTop(_dragShadow, y + 8);
        }

        SetStationHighlight(NearestStation(x + _dragItem.View.CardWidth / 2, y + _dragItem.View.CardHeight / 2));
    }

    private void BeginDragVisuals(CardItem item)
    {
        _dragMoved = true;
        item.View.ClearHover();
        item.View.LiftScale = 1.05;
        item.View.CardWidth = _cardW;
        item.View.CardHeight = _cardH;
        item.View.FanAngle = 0;
        item.View.IsFaceUp = true;

        _dragShadow = new Rectangle
        {
            Width = _cardW,
            Height = _cardH,
            RadiusX = 9,
            RadiusY = 9,
            Fill = Ornament.Brush(Ornament.Kohl, 0.55),
            IsHitTestVisible = false,
        };
        Canvas.SetZIndex(_dragShadow, 899);
        CardLayer.Children.Add(_dragShadow);
    }

    private int NearestStation(double x, double y)
    {
        var best = -1;
        var bestDistance = double.MaxValue;
        var snap = _cardH * 0.8;
        for (int i = 0; i < 9; i++)
        {
            var c = StationCentre(i);
            var d = Math.Sqrt((c.X - x) * (c.X - x) + (c.Y - y) * (c.Y - y));
            if (d < bestDistance) { bestDistance = d; best = i; }
        }
        return bestDistance <= snap ? best : -1;
    }

    private void OnCardReleased(object sender, PointerRoutedEventArgs e)
    {
        var item = ItemOf(sender);
        e.Handled = true;
        try { ((UIElement)sender).ReleasePointerCapture(e.Pointer); }
        catch (Exception) { /* capture may already be gone */ }

        if (item is null || !_dragging) { EndDrag(); return; }

        if (!_dragMoved)
        {
            EndDrag();
            if (_detailPanel is not null && ReferenceEquals(_detailItem, item)) { return; }
            ShowDetail(item, animate: _detailPanel is null);
            return;
        }

        var x = Canvas.GetLeft(item.View);
        var y = Canvas.GetTop(item.View);
        var target = NearestStation(x + _cardW / 2, y + _cardH / 2);
        EndDrag();

        // the view model decides what a drop on that station means; this page only reports it
        if (target >= 0)
        {
            ViewModel?.PlaceOnStation(item.Model, target);
        }
        else
        {
            ViewModel?.ReturnToTray(item.Model, announce: false);
        }
    }

    private void OnCardCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (_dragging && _dragMoved && _dragItem is not null)
        {
            var item = _dragItem;
            EndDrag();
            if (item.Model.InTray) { LayoutTray(); } else { RepositionAllCards(); }
        }
        else
        {
            EndDrag();
        }
    }

    private void EndDrag()
    {
        if (_dragItem is not null)
        {
            _dragItem.View.LiftScale = 1.0;
            Canvas.SetZIndex(_dragItem.View, ++_topZ);
        }
        if (_dragShadow is not null)
        {
            CardLayer.Children.Remove(_dragShadow);
            _dragShadow = null;
        }
        SetStationHighlight(-1);
        _dragging = false;
        _dragMoved = false;
        _dragItem = null;
    }

    // ==================================================================== the detail panel

    private void OnScenePressed(object sender, PointerRoutedEventArgs e) => CloseDetail();

    /// <summary>
    /// An unhandled pointer move bubbles out to the window manager, which on some heads then
    /// drags the window instead of leaving the scene alone. Card drags mark their own moves
    /// handled; this catches every other move over the app's own chrome.
    /// </summary>
    private void OnRootPointerMoved(object sender, PointerRoutedEventArgs e) => e.Handled = true;

    private void CloseDetail()
    {
        if (_detailPanel is not null)
        {
            DetailLayer.Children.Remove(_detailPanel);
            _detailPanel = null;
        }
        _detailItem = null;
        _detailFlipped = false;
        _detailFlipping = false;
        _detailScroller = null;
        _detailDragging = false;
    }

    private void ShowDetail(CardItem item, bool animate = true, bool keepPosition = false)
    {
        double? keptX = null, keptY = null;
        if (keepPosition && _detailPanel is not null)
        {
            keptX = Canvas.GetLeft(_detailPanel);
            keptY = Canvas.GetTop(_detailPanel);
        }
        CloseDetail();

        const double panelW = 340;
        var sceneH = ContentCanvas.ActualHeight;
        var sceneW = ContentCanvas.ActualWidth;

        // the text area grows with the window, leaving room for the fixed header and padding
        var scrollMax = Math.Clamp(sceneH - 150, 200, 560);
        var panel = BuildDetailPanel(item, panelW, scrollMax);
        AutomationProperties.SetAutomationId(panel, "CardDetail");
        panel.Width = panelW;
        panel.MaxHeight = Math.Max(240, sceneH - 16);

        var cardX = Canvas.GetLeft(item.View);
        var cardY = Canvas.GetTop(item.View);
        var cardW = item.View.CardWidth;

        var x = cardX + cardW + 14;
        if (x + panelW > _railX - 6) { x = cardX - panelW - 14; }
        x = Math.Clamp(x, _altarX + 6, Math.Max(_altarX + 6, _railX - panelW - 6));

        double panelH;
        try
        {
            panel.Measure(new Windows.Foundation.Size(panelW, sceneH));
            panelH = panel.DesiredSize.Height;
        }
        catch (Exception) { panelH = 0; }
        if (panelH < 100) { panelH = scrollMax + 110; }

        var y = Math.Clamp(cardY - 30, 8, Math.Max(8, sceneH - panelH - 8));
        x = Math.Clamp(x, 0, Math.Max(0, sceneW - panelW));
        if (keptX is double kx && keptY is double ky) { x = kx; y = ky; }

        Canvas.SetLeft(panel, x);
        Canvas.SetTop(panel, y);
        DetailLayer.Children.Add(panel);
        _detailPanel = panel;
        _detailItem = item;
        _detailFlipped = false;
        if (animate)
        {
            _starfield.Duck(TimeSpan.FromMilliseconds(400));
            try { panel.Opacity = 0; FadeIn(panel, true); } catch (Exception) { panel.Opacity = 1; }
        }
    }

    // ==================================================================== turning the lore panel

    /// <summary>
    /// Turn the lore panel over like a card: it narrows to its edge, shows its other face and widens
    /// again. One face is the card's lore; the other is the card itself, enlarged to fill the panel.
    /// </summary>
    private void FlipDetail()
    {
        if (_detailPanel is not { } panel || _detailItem is not { } item || _detailFlipping) { return; }
        var front = panel.Tag as UIElement;
        if (front is null) { return; }
        _starfield.Duck(TimeSpan.FromMilliseconds(600));

        var width = panel.ActualWidth > 0 ? panel.ActualWidth : panel.Width;
        var height = panel.ActualHeight;
        if (height < 80) { return; }

        UIElement nextFace;
        if (_detailFlipped)
        {
            nextFace = front;
        }
        else
        {
            nextFace = BuildEnlargedCard(item, width, height);
        }

        var scale = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        panel.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        panel.RenderTransform = scale;
        _detailFlipping = true;

        var queue = panel.DispatcherQueue;
        Microsoft.UI.Dispatching.DispatcherQueueTimer timer = null;
        try { timer = queue?.CreateTimer(); } catch (Exception) { timer = null; }

        void Swap()
        {
            if (!_detailFlipped)
            {
                // the panel is edge-on here, so it can take the card's shape without a visible jump
                panel.Height = double.NaN;
                panel.Child = nextFace;
                _detailFlipped = true;
            }
            else
            {
                panel.Child = nextFace;
                panel.Height = double.NaN;
                _detailFlipped = false;
            }
        }

        if (timer is null)
        {
            Swap();
            _detailFlipping = false;
            return;
        }

        const double half = 0.19;                      // seconds for each half of the turn
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var swapped = false;
        timer.Interval = TimeSpan.FromMilliseconds(15);
        timer.Tick += (t, _) =>
        {
            var elapsed = clock.Elapsed.TotalSeconds;
            if (!ReferenceEquals(_detailPanel, panel)) { t.Stop(); return; }
            if (elapsed < half)
            {
                var k = elapsed / half;
                scale.ScaleX = Math.Cos(k * Math.PI / 2);                   // narrowing to the edge
                scale.ScaleY = 1 - 0.04 * Math.Sin(k * Math.PI / 2);
            }
            else
            {
                if (!swapped) { Swap(); swapped = true; }
                var k = Math.Min(1, (elapsed - half) / half);
                scale.ScaleX = Math.Sin(k * Math.PI / 2);                   // opening out again
                scale.ScaleY = 1 - 0.04 * Math.Cos(k * Math.PI / 2);
                if (k >= 1)
                {
                    scale.ScaleX = 1;
                    scale.ScaleY = 1;
                    _detailFlipping = false;
                    t.Stop();
                }
            }
        };
        timer.Start();
    }

    /// <summary>The card itself, face up and upright, as large as the panel allows.</summary>
    private UIElement BuildEnlargedCard(CardItem item, double panelWidth, double panelHeight)
    {
        const double inset = 14;
        var availableW = panelWidth - 16 - 14 - 2 - inset;
        var availableH = panelHeight - 14 - 12 - 2 - inset;
        var cardH = Math.Min(availableH, availableW / 0.625);
        var cardW = cardH * 0.625;

        var view = new CardView
        {
            Card = item.Model.Card,
            IsFaceUp = true,
            IsReversed = false,
            CardWidth = Math.Round(cardW),
            CardHeight = Math.Round(cardH),
            EnableHover = false,
            IsHitTestVisible = false,
        };
        view.Rebuild();

        return new Grid
        {
            // a transparent fill so a click anywhere on this face turns the panel back
            Background = Ornament.Brush("#00000000"),
            Padding = new Thickness(0, inset / 2, 0, inset / 2),
            Children =
            {
                new Border
                {
                    Width = Math.Round(cardW),
                    Height = Math.Round(cardH),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Child = view,
                },
            },
        };
    }

    private Border BuildDetailPanel(CardItem item, double panelW, double scrollMax)
    {
        var card = item.Model.Card;

        const double padLeft = 16, padRight = 14;
        const double scrollBarGutter = 20;   // text stops this far short of the scroll bar
        var contentW = panelW - padLeft - padRight - 2;
        var textW = contentW - scrollBarGutter;

        // ---- fixed header: grip, title block and close button (also the drag handle)
        var handle = new Grid
        {
            // a transparent fill makes the whole header hit-testable for dragging
            Background = Ornament.Brush("#00000000"),
            Padding = new Thickness(0, 0, 0, 2),
        };
        handle.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        handle.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        handle.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        handle.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var grip = new Rectangle
        {
            Width = 38,
            Height = 3,
            RadiusX = 1.5,
            RadiusY = 1.5,
            Fill = Ornament.Brush(Ornament.Gold, 0.38),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, -4, 0, 9),
        };
        Grid.SetRow(grip, 0);
        Grid.SetColumnSpan(grip, 2);
        handle.Children.Add(grip);

        var heads = new StackPanel { Spacing = 2 };
        heads.Children.Add(Text(card.Name, 17, Ornament.Gold, bold: true));
        heads.Children.Add(Text(card.Epithet, 11.5, Ornament.Ivory, opacity: 0.72));
        heads.Children.Add(Text(card.Transliteration, 10.5, Ornament.GoldDeep, opacity: 0.9));
        Grid.SetRow(heads, 1);
        Grid.SetColumn(heads, 0);
        handle.Children.Add(heads);

        var close = new Border
        {
            Width = 22,
            Height = 22,
            CornerRadius = new CornerRadius(11),
            BorderBrush = Ornament.Brush(Ornament.Gold, 0.7),
            BorderThickness = new Thickness(1),
            Background = Ornament.Brush(Ornament.Kohl, 0.6),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14, 0, 2, 0),
            Child = new TextBlock
            {
                Text = "×",
                FontSize = 14,
                FontFamily = new FontFamily(Fonts.Regular),
                Foreground = Ornament.Brush(Ornament.Gold),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        close.PointerPressed += (_, e) => { e.Handled = true; CloseDetail(); };
        Grid.SetRow(close, 1);
        Grid.SetColumn(close, 1);
        handle.Children.Add(close);

        var rule = new Rectangle { Height = 1, Fill = Ornament.Brush(Ornament.GoldDeep, 0.5), Margin = new Thickness(0, 8, 0, 8) };

        // ---- scrolling text
        var stack = new StackPanel { Spacing = 7, Width = textW, HorizontalAlignment = HorizontalAlignment.Left };

        stack.Children.Add(Text(Ornament.Track("Domains"), 9.5, Ornament.GoldDeep));
        stack.Children.Add(Text(string.Join("  ·  ", card.Domains), 12, Ornament.Ivory, opacity: 0.9, wrap: true));

        stack.Children.Add(Text(Ornament.Track(item.Model.IsReversed ? "Reversed" : "Upright"), 9.5, Ornament.GoldDeep, margin: new Thickness(0, 6, 0, 0)));
        var keywords = item.Model.IsReversed ? card.ReversedKeywords : card.UprightKeywords;
        stack.Children.Add(BuildChips(keywords, textW));
        stack.Children.Add(Text(item.Model.IsReversed ? card.ReversedMeaning : card.UprightMeaning, 12, Ornament.Ivory, opacity: 0.9, wrap: true, lineHeight: 19));

        stack.Children.Add(Text(Ornament.Track("Lore"), 9.5, Ornament.GoldDeep, margin: new Thickness(0, 6, 0, 0)));
        stack.Children.Add(Text(card.Lore, 11.5, Ornament.Ivory, opacity: 0.72, wrap: true, lineHeight: 18));

        // wrapped text measures a hair narrower than it draws on this head; give the last glyph room
        foreach (var child in stack.Children)
        {
            if (child is TextBlock tb && tb.TextWrapping == TextWrapping.Wrap)
            {
                tb.Padding = new Thickness(0, 0, 6, 0);
            }
        }

        // two and a half blank lines after the last paragraph, so it never ends under the fade
        stack.Children.Add(new Border { Height = 46 });

        var scroller = new ScrollViewer
        {
            Content = stack,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = scrollMax,
        };
        _detailScroller = scroller;

        // a soft fade at the foot says "there is more below"; it clears the scroll bar and
        // disappears once the reader reaches the end, so the last line is never dimmed
        var fade = new Rectangle
        {
            Height = 26,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, scrollBarGutter, 0),
            IsHitTestVisible = false,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(0, 1),
                GradientStops =
                {
                    new Microsoft.UI.Xaml.Media.GradientStop { Color = Ornament.ToColor("#00130B26"), Offset = 0 },
                    new Microsoft.UI.Xaml.Media.GradientStop { Color = Ornament.ToColor("#FA130B26"), Offset = 1 },
                },
            },
        };
        void UpdateFade()
        {
            try
            {
                var more = scroller.ScrollableHeight > 1 && scroller.VerticalOffset < scroller.ScrollableHeight - 2;
                fade.Opacity = more ? 1 : 0;
            }
            catch (Exception) { fade.Opacity = 0; }
        }
        scroller.ViewChanged += (_, _) => UpdateFade();
        scroller.SizeChanged += (_, _) => UpdateFade();
        stack.SizeChanged += (_, _) => UpdateFade();

        var body = new Grid();
        body.Children.Add(scroller);
        body.Children.Add(fade);

        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(handle, 0);
        Grid.SetRow(rule, 1);
        Grid.SetRow(body, 2);
        layout.Children.Add(handle);
        layout.Children.Add(rule);
        layout.Children.Add(body);

        var border = new Border
        {
            Background = Ornament.Brush("#FA130B26"),
            BorderBrush = Ornament.Brush(Ornament.GoldDeep, 0.85),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(padLeft, 14, padRight, 12),
            Child = layout,
        };
        border.Tag = layout;                                  // the lore face, kept for turning back
        Windows.Foundation.Point? pressAt = null;
        border.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            pressAt = e.GetCurrentPoint(border).Position;
        };
        border.PointerReleased += (_, e) =>
        {
            e.Handled = true;
            if (pressAt is not { } start) { return; }
            pressAt = null;
            var end = e.GetCurrentPoint(border).Position;
            if (Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y) > 5) { return; }
            FlipDetail();
        };

        AttachDetailDrag(handle);
        return border;
    }

    /// <summary>
    /// Drag the detail box by its header. The same pointer-capture recipe as the cards: every
    /// event is marked handled, so the macOS window never moves instead of the box.
    /// </summary>
    private void AttachDetailDrag(FrameworkElement handle)
    {
        handle.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            if (_detailPanel is null) { return; }
            var p = e.GetCurrentPoint(ContentCanvas).Position;
            _detailGrab = new Windows.Foundation.Point(p.X - Canvas.GetLeft(_detailPanel), p.Y - Canvas.GetTop(_detailPanel));
            _detailDragging = handle.CapturePointer(e.Pointer) || true;
        };

        handle.PointerMoved += (_, e) =>
        {
            e.Handled = true;
            if (!_detailDragging || _detailPanel is null) { return; }
            var p = e.GetCurrentPoint(ContentCanvas).Position;
            MoveDetailTo(p.X - _detailGrab.X, p.Y - _detailGrab.Y);
        };

        void End(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (!_detailDragging) { return; }
            _detailDragging = false;
            try { handle.ReleasePointerCapture(e.Pointer); } catch (Exception) { }
        }

        handle.PointerReleased += End;
        handle.PointerCanceled += End;
        handle.PointerCaptureLost += (_, _) => _detailDragging = false;
    }

    /// <summary>Place the detail box, keeping it fully inside the window below the header bar.</summary>
    private void MoveDetailTo(double x, double y)
    {
        if (_detailPanel is null) { return; }
        var w = _detailPanel.ActualWidth > 0 ? _detailPanel.ActualWidth : _detailPanel.Width;
        var h = _detailPanel.ActualHeight > 0 ? _detailPanel.ActualHeight : 200;
        x = Math.Clamp(x, 0, Math.Max(0, ContentCanvas.ActualWidth - w));
        y = Math.Clamp(y, 0, Math.Max(0, ContentCanvas.ActualHeight - h));
        Canvas.SetLeft(_detailPanel, x);
        Canvas.SetTop(_detailPanel, y);
    }

    // ==================================================================== text helpers

    private static TextBlock Text(string text, double size, string colour, bool bold = false,
                                  double opacity = 1, bool wrap = false, double lineHeight = 0,
                                  Thickness margin = default)
    {
        var t = new TextBlock
        {
            Text = text ?? string.Empty,
            FontSize = size,
            FontFamily = new FontFamily(bold ? Fonts.Bold : Fonts.Regular),
            FontWeight = bold ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal,
            Foreground = Ornament.Brush(colour, opacity),
            TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Margin = margin,
        };
        if (lineHeight > 0) { t.LineHeight = lineHeight; }
        return t;
    }

    /// <summary>Keyword chips, wrapped by hand (there is no WrapPanel in this XAML surface).</summary>
    private static StackPanel BuildChips(IReadOnlyList<string> keywords, double width)
    {
        var outer = new StackPanel { Spacing = 5, Margin = new Thickness(0, 3, 0, 4) };
        StackPanel row = NewRow();
        double used = 0;

        foreach (var keyword in keywords ?? Array.Empty<string>())
        {
            var text = Ornament.Track(keyword);
            var estimate = keyword.Length * 7.2 + 20;
            if (used > 0 && used + estimate > width)
            {
                outer.Children.Add(row);
                row = NewRow();
                used = 0;
            }
            row.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(2),
                BorderBrush = Ornament.Brush(Ornament.GoldDeep, 0.55),
                BorderThickness = new Thickness(1),
                Background = Ornament.Brush(Ornament.Gold, 0.08),
                Padding = new Thickness(6, 2, 6, 3),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 8.8,
                    FontFamily = new FontFamily(Fonts.Regular),
                    Foreground = Ornament.Brush(Ornament.GoldPale, 0.95),
                },
            });
            used += estimate;
        }

        if (row.Children.Count > 0) { outer.Children.Add(row); }
        return outer;

        static StackPanel NewRow() => new() { Orientation = Orientation.Horizontal, Spacing = 5 };
    }

    // ==================================================================== the interpretation panel

    private void ApplyInterpretationPanel(bool show)
    {
        //The rail, the rail chrome and the panel are bound to DeckPanelVisibility and
        //  InterpretationVisibility; what is left here is the fade and the cards, which are
        //  built in code and so have nothing to bind
        if (!show && _railChromeStale)
        {
            //The window was resized while the panel was up, so the rail is being shown again
            //  at a size its chrome was never drawn for
            BuildRailChrome();
            LayoutTray();
        }

        UpdateDeckStackVisibility();

        foreach (var item in _items.Where(i => i.Model.InTray))
        {
            item.View.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
        }

        _starfield.Duck(TimeSpan.FromMilliseconds(400));
        FadeIn(InterpretationBorder, show);
    }

    private static void FadeIn(UIElement element, bool visible)
    {
        try
        {
            var story = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = visible ? 0 : 1,
                To = visible ? 1 : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(220)),
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, element);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Opacity");
            story.Children.Add(animation);
            story.Begin();
            element.Opacity = visible ? 1 : 0;
        }
        catch (Exception)
        {
            element.Opacity = visible ? 1 : 0;
        }
    }

    private void RenderInterpretation(ReadingInterpretation interpretation)
    {
        //The title and the subtitle are bound to the view model; the body below is built by hand
        InterpStack.Children.Clear();
        if (interpretation is null) { return; }

        if (!string.IsNullOrWhiteSpace(interpretation.Reading.Question))
        {
            InterpStack.Children.Add(Quote($"“{interpretation.Reading.Question}”"));
        }

        InterpStack.Children.Add(Text(interpretation.Opening, 12, Ornament.Ivory, opacity: 0.92, wrap: true, lineHeight: 19));

        foreach (var position in interpretation.Positions)
        {
            InterpStack.Children.Add(new Rectangle
            {
                Height = 0.8,
                Fill = Ornament.Brush(Ornament.GoldDeep, 0.3),
                Margin = new Thickness(0, 8, 0, 2),
            });
            InterpStack.Children.Add(Text(position.Heading, 13.5, Ornament.Gold, bold: true, wrap: true));
            InterpStack.Children.Add(Text(
                $"{position.Placed.Card.Name} · {position.Placed.OrientationLabel}",
                11, Ornament.Ivory, opacity: 0.7, wrap: true));
            InterpStack.Children.Add(BuildChips(position.Keywords, 296));
            foreach (var paragraph in position.Paragraphs)
            {
                InterpStack.Children.Add(Text(paragraph, 12, Ornament.Ivory, opacity: 0.92, wrap: true, lineHeight: 19));
            }
            if (!string.IsNullOrWhiteSpace(position.Placed.Card.Invocation))
            {
                InterpStack.Children.Add(Quote(position.Placed.Card.Invocation));
            }
        }

        if (interpretation.Insights.Count > 0)
        {
            InterpStack.Children.Add(Heading(Ornament.Track("What the Octaquetra shows")));
            foreach (var insight in interpretation.Insights)
            {
                var body = new StackPanel { Spacing = 4 };
                body.Children.Add(Text(insight.Title, 12, Ornament.GoldPale, bold: true, wrap: true));
                body.Children.Add(Text(insight.Text, 11.5, Ornament.Ivory, opacity: 0.88, wrap: true, lineHeight: 18));
                InterpStack.Children.Add(new Border
                {
                    Background = Ornament.Brush(Ornament.Night2, 0.85),
                    BorderBrush = Ornament.Brush(Ornament.GoldDeep, 0.4),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 10, 12, 12),
                    Child = body,
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(interpretation.Counsel))
        {
            InterpStack.Children.Add(Heading(Ornament.Track("Counsel")));
            InterpStack.Children.Add(Text(interpretation.Counsel, 12, Ornament.Ivory, opacity: 0.92, wrap: true, lineHeight: 19));
        }

        if (!string.IsNullOrWhiteSpace(interpretation.ClosingInvocation))
        {
            InterpStack.Children.Add(Heading(Ornament.Track("Closing invocation")));
            InterpStack.Children.Add(Quote(interpretation.ClosingInvocation));
        }

        //The last thing in the panel, whatever came before it: the one line every reading ends
        //  with. It is set exactly as the library wrote it - no tracking, no capitals - because
        //  the report prints the same characters.
        if (!string.IsNullOrWhiteSpace(interpretation.Closing))
        {
            InterpStack.Children.Add(new Rectangle
            {
                Height = 0.8,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Center,
                Fill = Ornament.Brush(Ornament.GoldDeep, 0.55),
                Margin = new Thickness(0, 14, 0, 2),
            });

            var closing = Text(interpretation.Closing, 11, Ornament.Gold, opacity: 0.92, wrap: true,
                lineHeight: 18, margin: new Thickness(0, 0, 0, 6));
            closing.TextAlignment = TextAlignment.Center;
            InterpStack.Children.Add(closing);
        }
    }

    private static TextBlock Heading(string text) =>
        Text(text, 10.5, Ornament.Gold, bold: true, margin: new Thickness(0, 12, 0, 0));

    private static Border Quote(string text) => new()
    {
        BorderBrush = Ornament.Brush(Ornament.Gold, 0.75),
        BorderThickness = new Thickness(2, 0, 0, 0),
        Padding = new Thickness(12, 3, 4, 4),
        Margin = new Thickness(2, 4, 0, 4),
        Child = Text(text, 12, Ornament.GoldPale, opacity: 0.88, wrap: true, lineHeight: 19),
    };

    // ==================================================================== file dialogs

    private static async Task<string> PickSavePathAsync(string suggestedFileName, string typeName, string extension)
    {
        //The snapshot hook's "pdf" scenario writes the report without a save panel (see the README)
        if (SnapshotPdfFolder() is string folder) { return System.IO.Path.Combine(folder, suggestedFileName); }

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = extension,
        };
        picker.FileTypeChoices.Add(typeName, new List<string> { extension });

        var file = await picker.PickSaveFileAsync();
        if (file is null) { return null; }

        //Some heads percent-encode the path they return, which would save "My Reading.pdf" as
        //  "My%20Reading.pdf"; decode it before anything touches the disk
        var path = FileDialogHelper.ToFileSystemPath(file.Path);
        FileDialogHelper.RemoveEmptyPlaceholder(path);
        return path;
    }

    private static async Task<string> PickReadingPathAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        return file is null ? null : FileDialogHelper.ToFileSystemPath(file.Path);
    }

    // ==================================================================== dialogs

    private async Task<bool> ConfirmAsync(string title, string message, string primary)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = Text(message, 12.5, Ornament.Ivory, opacity: 0.9, wrap: true),
                PrimaryButtonText = primary,
                CloseButtonText = "Cancel",
                PrimaryButtonStyle = (Style)Application.Current.Resources["TemplePrimaryButtonStyle"],
                CloseButtonStyle = (Style)Application.Current.Resources["TempleButtonStyle"],
                XamlRoot = XamlRoot,
            };
            return await dialog.ShowAsync() == ContentDialogResult.Primary;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Confirm dialog failed.");
            return true;
        }
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = Text(message, 12.5, Ornament.Ivory, opacity: 0.9, wrap: true),
                CloseButtonText = "Close",
                CloseButtonStyle = (Style)Application.Current.Resources["TempleButtonStyle"],
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Message dialog failed.");
        }
    }

    private async Task ShowReportSavedAsync(string path)
    {
        var stack = new StackPanel { Spacing = 12, Width = 430 };
        if (OperatingSystem.IsAndroid())
        {
            //On Android the path is the save panel's working copy in the app's private cache, which
            //  means nothing to a person; the document itself is in the folder they chose, by name
            stack.Children.Add(Text("The report has been saved as:", 12, Ornament.Ivory, opacity: 0.9, wrap: true));
            stack.Children.Add(Text(System.IO.Path.GetFileName(path), 11, Ornament.GoldPale, wrap: true));
            stack.Children.Add(Text("in the folder you chose.", 12, Ornament.Ivory, opacity: 0.9, wrap: true));
        }
        else
        {
            stack.Children.Add(Text("The report has been written to:", 12, Ornament.Ivory, opacity: 0.9, wrap: true));
            stack.Children.Add(Text(path, 11, Ornament.GoldPale, wrap: true));
        }

        //Handing the file to the operating system is the view model's job, so the button runs
        //  its command rather than starting anything itself
        var open = new Button
        {
            Content = "Open",
            Style = (Style)Application.Current.Resources["TempleButtonStyle"],
            Command = ViewModel?.OpenSavedReportCommand,
        };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(open);

        //Reveal in Finder and Share via Mail are macOS's own; every other platform has Open alone
        if (ViewModel?.IsMacOS == true)
        {
            row.Children.Add(new Button
            {
                Content = "Reveal in Finder",
                Style = (Style)Application.Current.Resources["TempleButtonStyle"],
                Command = ViewModel?.RevealSavedReportCommand,
            });
            row.Children.Add(new Button
            {
                Content = "Share via Mail",
                Style = (Style)Application.Current.Resources["TempleButtonStyle"],
                Command = ViewModel?.ShareSavedReportCommand,
            });
        }

        stack.Children.Add(row);

        var dialog = new ContentDialog
        {
            Title = "Report saved",
            Content = stack,
            CloseButtonText = "Done",
            CloseButtonStyle = (Style)Application.Current.Resources["TemplePrimaryButtonStyle"],
            XamlRoot = XamlRoot,
        };
        try { await dialog.ShowAsync(); }
        catch (Exception ex) { _log.LogWarning(ex, "Saved dialog failed."); }
    }
}

/// <summary>Small positioning sugar for canvas children.</summary>
internal static class CanvasExtensions
{
    public static T At<T>(this T element, double x, double y) where T : UIElement
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
        return element;
    }
}
