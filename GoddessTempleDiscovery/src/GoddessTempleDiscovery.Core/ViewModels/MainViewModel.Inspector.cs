using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Bridges;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GoddessTempleDiscovery.ViewModels;

public partial class MainViewModel
{
    //A countdown shorter than this closes too soon for a bar to be worth showing
    private const double CountdownBarMinimumSeconds = 3;

    private int _countdownToken;

    #region | The inspector: a page of the Warka Herald |

    /// <summary>True while the inspector is open.</summary>
    [AffectsProperties(nameof(InspectorVisibility))]
    public bool IsInspectorOpen
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>The inspector overlay.</summary>
    public Visibility InspectorVisibility => GetVisibility(IsInspectorOpen);

    /// <summary>The kind of card shown.</summary>
    public CardKind InspectorKind
    {
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged(nameof(InspectorKind));
            NotifyPropertyChanged(nameof(NoticeVisibility));
            NotifyPropertyChanged(nameof(LearnedSocietyVisibility));
            NotifyPropertyChanged(nameof(RecordVisibility));
            NotifyPropertyChanged(nameof(MastheadVisibility));
            NotifyPropertyChanged(nameof(EditionLine));
        }
    }

    /// <summary>The banner headline.</summary>
    public string InspectorHeadline { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The sub-head.</summary>
    public string InspectorSubHead { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The date line.</summary>
    public string InspectorDateline { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The byline.</summary>
    public string InspectorByline { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The card's own title.</summary>
    public string InspectorTitle { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The kind label, such as "Discovery · Building".</summary>
    public string InspectorKindLabel { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The period line.</summary>
    public string InspectorPeriod { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The first column: the card text.</summary>
    public string InspectorColumnOne { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The second column: the long text.</summary>
    public string InspectorColumnTwo { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The photograph's caption.</summary>
    public string InspectorCaption { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The WHERE IT IS NOW box (or, for a season, its effect).</summary>
    public string InspectorWhereNow { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The cuneiform line and its reading.</summary>
    [AffectsProperties(nameof(CuneiformVisibility))]
    public string InspectorCuneiform { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The quotation with its attribution.</summary>
    [AffectsProperties(nameof(QuoteVisibility))]
    public string InspectorQuote { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The sources.</summary>
    public string InspectorSources { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>THE RECORD box's lines.</summary>
    public ObservableCollection<LabelValueItem> InspectorFacts { get; } = new ObservableCollection<LabelValueItem>();

    /// <summary>The card's art, framed as a photograph.</summary>
    public BitmapImage InspectorPhoto { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The card face itself (the inspector's small "card" view).</summary>
    public BitmapImage InspectorFace { get => field; private set => SetProperty(ref field, value); }

    /// <summary>True for one of Her starred finds: the EXTRA! badge.</summary>
    [AffectsProperties(nameof(ExtraVisibility))]
    public bool InspectorIsStarred { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The EXTRA! badge.</summary>
    public Visibility ExtraVisibility => GetVisibility(InspectorIsStarred && InspectorKind == CardKind.Discovery);

    /// <summary>The masthead (every kind but the Learned Society's column).</summary>
    public Visibility MastheadVisibility => GetVisibility(InspectorKind != CardKind.Tablet);

    /// <summary>The NOTICE TO ALL EXPEDITIONS heading (season front pages and favors).</summary>
    public Visibility NoticeVisibility => GetVisibility(InspectorKind is CardKind.Season or CardKind.Favor);

    /// <summary>The LEARNED SOCIETY heading (tablets).</summary>
    public Visibility LearnedSocietyVisibility => GetVisibility(InspectorKind == CardKind.Tablet);

    /// <summary>The WHERE IT IS NOW box (discoveries).</summary>
    public Visibility RecordVisibility => GetVisibility(InspectorKind == CardKind.Discovery);

    /// <summary>The cuneiform line.</summary>
    public Visibility CuneiformVisibility => GetVisibility(InspectorCuneiform.Length > 0);

    /// <summary>The quotation.</summary>
    public Visibility QuoteVisibility => GetVisibility(InspectorQuote.Length > 0);

    /// <summary>The edition line over the headline.</summary>
    public string EditionLine => InspectorKind switch
    {
        CardKind.Discovery => "EXTRA! · SPECIAL EDITION",
        CardKind.Season => "FRONT PAGE",
        CardKind.Tablet => "THE LEARNED SOCIETY · PROCEEDINGS",
        CardKind.Favor => "A NOTICE",
        _ => "THE ARCHIVE",
    };

    /// <summary>Closes the inspector.</summary>
    public SimpleCommand CloseInspectorCommand => field ??= new SimpleCommand(CloseInspector);

    /// <summary>True while the open page is counting down to closing by itself.</summary>
    [AffectsProperties(nameof(KeepReadingVisibility), nameof(CountdownVisibility))]
    public bool IsInspectorCountingDown { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The seconds the open page was given to close by itself.</summary>
    public double InspectorCountdownSeconds
    {
        get;
        private set => SetDouble(ref field, value, nameof(InspectorCountdownSeconds), nameof(CountdownVisibility));
    }

    /// <summary>The time left before the page closes by itself, from 100 (all of it) down to 0.</summary>
    public double InspectorCountdownPercent
    {
        get;
        private set => SetDouble(ref field, value, nameof(InspectorCountdownPercent));
    }

    /// <summary>"Closes by itself in 23 seconds".</summary>
    public string InspectorCountdownText { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The KEEP READING button, while the page counts down.</summary>
    public Visibility KeepReadingVisibility => GetVisibility(IsInspectorCountingDown);

    /// <summary>The countdown bar, while the page counts down from 3 seconds or more.</summary>
    public Visibility CountdownVisibility => GetVisibility(IsInspectorCountingDown && InspectorCountdownSeconds >= CountdownBarMinimumSeconds);

    /// <summary>Stops the open page from closing by itself: it stays until it is closed.</summary>
    public SimpleCommand KeepReadingCommand => field ??= new SimpleCommand(KeepReading);

    /// <inheritdoc />
    public void ShowCard(CardView card)
    {
        if (card == null)
        {
            return;
        }

        var token = ++_inspectorToken;
        StopCountdown();
        InspectorKind = card.Kind;
        InspectorHeadline = card.Headline;
        InspectorSubHead = card.SubHead;
        InspectorDateline = card.Dateline;
        InspectorByline = card.Byline;
        InspectorTitle = card.Title;
        InspectorKindLabel = card.KindLabel;
        InspectorPeriod = card.Period;
        InspectorColumnOne = card.CardText;
        InspectorColumnTwo = card.LongText;
        InspectorCaption = card.Caption;
        InspectorWhereNow = card.WhereNow;
        InspectorCuneiform = string.IsNullOrWhiteSpace(card.Cuneiform)
            ? string.Empty
            : card.Cuneiform + (string.IsNullOrWhiteSpace(card.CuneiformReading) ? string.Empty : "  —  " + card.CuneiformReading);
        InspectorQuote = string.IsNullOrWhiteSpace(card.Quote)
            ? string.Empty
            : "“" + card.Quote + "”" + (string.IsNullOrWhiteSpace(card.QuoteAttribution) ? string.Empty : "  — " + card.QuoteAttribution);
        InspectorSources = card.Sources;
        InspectorIsStarred = card.IsStarred;
        NotifyPropertyChanged(nameof(ExtraVisibility));
        InspectorFacts.Clear();
        foreach (var fact in card.Facts.Where(f => !string.IsNullOrWhiteSpace(f.Value)))
        {
            InspectorFacts.Add(new LabelValueItem(fact.Label, fact.Value));
        }

        IsInspectorOpen = true;
        _ = LoadInspectorImagesAsync(card, token);
        if (card.AutoCloseSeconds > 0)
        {
            _ = AutoCloseAsync(token, card.AutoCloseSeconds);
        }
    }

    /// <inheritdoc />
    public void Hide() => CloseInspector();

    private void CloseInspector()
    {
        _inspectorToken++;
        StopCountdown();
        if (IsInspectorOpen)
        {
            IsInspectorOpen = false;
        }

        Host?.NotifyInspectorClosed();
    }

    private async Task LoadInspectorImagesAsync(CardView card, int token)
    {
        var photo = await ImageFromAsync(card.ArtPng);
        var face = await ImageFromAsync(card.PngFace);
        if (token == _inspectorToken)
        {
            InspectorPhoto = photo;
            InspectorFace = face;
        }
    }

    //SimpleViewModel's SetProperty has no double overload: set, then name the property and what depends on it
    private void SetDouble(ref double field, double value, params string[] names)
    {
        if (field.Equals(value))
        {
            return;
        }

        field = value;
        foreach (var name in names)
        {
            NotifyPropertyChanged(name);
        }
    }

    private void KeepReading()
    {
        if (!IsInspectorCountingDown)
        {
            return;
        }

        StopCountdown();
        Host?.NotifyInspectorKept();
    }

    private void StopCountdown()
    {
        _countdownToken++;
        IsInspectorCountingDown = false;
    }

    //Closes by itself after the given seconds, the bar emptying as the time runs out, unless KEEP READING stops it; when
    //  the time is up while the pointer rests on the page, it waits until the pointer leaves
    private async Task AutoCloseAsync(int token, double seconds)
    {
        var countdown = ++_countdownToken;
        InspectorCountdownSeconds = seconds;
        InspectorCountdownPercent = 100;
        InspectorCountdownText = CountdownText(seconds);
        IsInspectorCountingDown = true;
        var clock = Stopwatch.StartNew();
        while (token == _inspectorToken && countdown == _countdownToken)
        {
            var left = Math.Max(0, seconds - clock.Elapsed.TotalSeconds);
            InspectorCountdownPercent = 100 * left / seconds;
            InspectorCountdownText = CountdownText(left);
            if (left <= 0 && !_pointerOverInspector)
            {
                break;
            }

            await Task.Delay(100);
        }

        if (token == _inspectorToken && countdown == _countdownToken)
        {
            CloseInspector();
        }
    }

    private static string CountdownText(double secondsLeft)
    {
        var whole = (int)Math.Ceiling(secondsLeft);
        return whole <= 0
            ? "Closes when the pointer leaves the page"
            : string.Format(CultureInfo.InvariantCulture, "Closes by itself in {0} second{1}", whole, whole == 1 ? string.Empty : "s");
    }

    #endregion
}
