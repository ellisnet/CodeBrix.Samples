using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Game.Rendering;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;
using GoddessTempleDiscovery.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GoddessTempleDiscovery.ViewModels;

/// <summary>
/// Lets the page hand the view model the game canvas once it has a size, so the page never names the view model's
/// concrete type.
/// </summary>
public interface IManageGameCanvas
{
    /// <summary>The running game host, or null before the canvas has started.</summary>
    GoddessTempleGameHost Host { get; }

    /// <summary>Called once, when the canvas has started for the first time.</summary>
    /// <param name="canvas">The started canvas.</param>
    void CanvasFirstStart(GameSurfaceCanvas canvas);

    /// <summary>The page reports whether the pointer is over the inspector (it then does not close itself).</summary>
    /// <param name="isOver">True while the pointer is over it.</param>
    void InspectorPointerOver(bool isOver);

    /// <summary>A key the page saw that the game surface did not take (the gallery's Escape and arrows).</summary>
    /// <param name="key">The key.</param>
    /// <returns>True when the key was used.</returns>
    bool HandleKey(Windows.System.VirtualKey key);
}

/// <summary>
/// The main page's view model. It owns the game host and every pane of the XAML chrome over the table: the title,
/// the setup, the prologue and epilogue, the newspaper inspector, the Field Journal, the scores, How to Play, the
/// History, the Credits and the settings. The host talks back through <see cref="IInspectorBridge"/> and
/// <see cref="ISessionBridge"/>, always on the UI thread.
/// </summary>
[Microsoft.UI.Xaml.Data.Bindable]
public partial class MainViewModel : SimpleViewModel, IManageGameCanvas, IInspectorBridge, ISessionBridge, IJournalFileBridge
{
    private enum Pane { Title, Setup, Prologue, Play, Epilogue, Scores, HowToPlay, History, Credits }

    private Pane _pane = Pane.Title;
    private Pane _returnPane = Pane.Title;
    private int _inspectorToken;
    private bool _pointerOverInspector;
    private IReadOnlyList<JournalEntry> _journal = Array.Empty<JournalEntry>();
    private IReadOnlyList<TeamView> _teams = Array.Empty<TeamView>();
    private FinalScore[] _finalScores = Array.Empty<FinalScore>();

    /// <summary>Creates the view model.</summary>
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Goddess Temple Discovery view model startup.");
        GameStartup.OpenSettingsStore();
        var epigraph = Catalog.Quotations.FirstOrDefault();
        EpigraphText = epigraph?.Text ?? string.Empty;
        EpigraphAttribution = epigraph?.Attribution ?? string.Empty;
        LoadSettings();
        BuildScreens();
        _ = LoadChromeArtAsync();

        //While any pane covers the table, the table takes no clicks
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsJournalOpen) or nameof(IsSettingsOpen) or nameof(IsGalleryOpen) or nameof(IsInspectorOpen)
                or nameof(PlayVisibility))
            {
                Host?.SetPanesOpen(PanesOpen);
            }
        };
    }

    #region | IManageGameCanvas implementation |

    /// <inheritdoc />
    public GoddessTempleGameHost Host { get; private set; }

    /// <inheritdoc />
    public void CanvasFirstStart(GameSurfaceCanvas canvas)
    {
        //The GPU tier (unless GODDESSTEMPLE_USE_CPU=1) and the pinned 1280 x 800 render resolution MUST be set before
        //  the first access to canvas.Host - the render tier cannot change once the scene pipeline exists.
        GoddessTempleGameHost.PrepareCanvas(canvas);
        Host = new GoddessTempleGameHost(canvas, this, this, action => InvokeOnMainThread(action));

        //Information keeps the game's own [GoddessTemple] lines on the console along with the engine's milestones
        Host.Initialize(logLevel: Microsoft.Extensions.Logging.LogLevel.Information);
        Host.ApplySettings();
        Host.SetPanesOpen(PanesOpen);
        if (Host.IsAutoPlay)
        {
            //The autoplay presses New Game itself and seats four computer teams
            NewGameCommand.Execute(null);
            StartAutoPlay();
        }
    }

    /// <inheritdoc />
    public void InspectorPointerOver(bool isOver) => _pointerOverInspector = isOver;

    #endregion

    #region | Panes |

    /// <summary>The title pane.</summary>
    public Visibility TitleVisibility => GetVisibility(_pane == Pane.Title);

    /// <summary>The setup pane.</summary>
    public Visibility SetupVisibility => GetVisibility(_pane == Pane.Setup);

    /// <summary>The prologue pane.</summary>
    public Visibility PrologueVisibility => GetVisibility(_pane == Pane.Prologue);

    /// <summary>The thin strip over the table while a game is played.</summary>
    public Visibility PlayVisibility => GetVisibility(_pane == Pane.Play);

    /// <summary>The epilogue pane.</summary>
    public Visibility EpilogueVisibility => GetVisibility(_pane == Pane.Epilogue);

    /// <summary>The final scores pane (the Final Edition).</summary>
    public Visibility ScoresVisibility => GetVisibility(_pane == Pane.Scores);

    /// <summary>How to Play.</summary>
    public Visibility HowToPlayVisibility => GetVisibility(_pane == Pane.HowToPlay);

    /// <summary>The History pane.</summary>
    public Visibility HistoryVisibility => GetVisibility(_pane == Pane.History);

    /// <summary>The Credits pane.</summary>
    public Visibility CreditsVisibility => GetVisibility(_pane == Pane.Credits);

    /// <summary>True while any pane covers the table: every pane but the table itself, or an overlay over it.</summary>
    public bool PanesOpen => _pane != Pane.Play || IsJournalOpen || IsSettingsOpen || IsGalleryOpen || IsInspectorOpen;

    /// <summary>True while a game is on the table and not over (Continue returns to it).</summary>
    [AffectsCommands(nameof(ContinueCommand))]
    public bool GameInProgress
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private void Show(Pane pane)
    {
        _pane = pane;
        NotifyPropertyChanged(nameof(TitleVisibility));
        NotifyPropertyChanged(nameof(SetupVisibility));
        NotifyPropertyChanged(nameof(PrologueVisibility));
        NotifyPropertyChanged(nameof(PlayVisibility));
        NotifyPropertyChanged(nameof(EpilogueVisibility));
        NotifyPropertyChanged(nameof(ScoresVisibility));
        NotifyPropertyChanged(nameof(HowToPlayVisibility));
        NotifyPropertyChanged(nameof(HistoryVisibility));
        NotifyPropertyChanged(nameof(CreditsVisibility));
    }

    //How to Play, History and Credits open from the title or from the table and close back to where they came from
    private void ShowAside(Pane pane)
    {
        if (_pane is Pane.Title or Pane.Play or Pane.Scores)
        {
            _returnPane = _pane;
        }

        Show(pane);
    }

    #endregion

    #region | Title |

    /// <summary>The epigraph: Julius Jordan's descent passage.</summary>
    public string EpigraphText { get; private set; } = string.Empty;

    /// <summary>Who wrote the epigraph, and where.</summary>
    public string EpigraphAttribution { get; private set; } = string.Empty;

    /// <summary>The newspaper dateline under the wordmark.</summary>
    public string TitleDateline => "WARKA, IRAQ — WINTER 1912/13 — TWO TO FOUR EXPEDITIONS";

    /// <summary>Opens the setup pane.</summary>
    public SimpleCommand NewGameCommand => field ??= new SimpleCommand(OpenSetup);

    /// <summary>Returns to the game in progress.</summary>
    public SimpleCommand ContinueCommand => field ??= new SimpleCommand(() => GameInProgress, () => Show(Pane.Play));

    /// <summary>Opens How to Play.</summary>
    public SimpleCommand HowToPlayCommand => field ??= new SimpleCommand(() => ShowAside(Pane.HowToPlay));

    /// <summary>Opens the History.</summary>
    public SimpleCommand HistoryCommand => field ??= new SimpleCommand(() => ShowAside(Pane.History));

    /// <summary>Opens the Credits.</summary>
    public SimpleCommand CreditsCommand => field ??= new SimpleCommand(() => ShowAside(Pane.Credits));

    /// <summary>Closes How to Play, History or Credits.</summary>
    public SimpleCommand BackCommand => field ??= new SimpleCommand(() => Show(_returnPane));

    /// <summary>Returns to the title (from the setup or the table).</summary>
    public SimpleCommand TitleCommand => field ??= new SimpleCommand(() =>
    {
        IsJournalOpen = false;
        IsSettingsOpen = false;
        Show(Pane.Title);
    });

    #endregion

    #region | Play |

    /// <summary>The season line over the table, in headline voice.</summary>
    public string SeasonTitle
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>The wire-service ticker line, or the prompt.</summary>
    public string TickerText
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>The teams as they stand.</summary>
    public ObservableCollection<LabelValueItem> Standings { get; } = new ObservableCollection<LabelValueItem>();

    #endregion

    #region | ISessionBridge implementation |

    /// <inheritdoc />
    public void SeasonChanged(SeasonCard season, int index, int count)
    {
        SeasonTitle = season == null
            ? string.Empty
            : string.Format(CultureInfo.InvariantCulture, "{0}  ·  SEASON {1} OF {2}", Game.Cards.CardText.BannerLine(season, Host?.GameSeed ?? 0), index + 1, count);
    }

    /// <inheritdoc />
    public void TeamsChanged(IReadOnlyList<TeamView> teams)
    {
        _teams = teams ?? Array.Empty<TeamView>();
        Standings.Clear();
        foreach (var team in _teams)
        {
            Standings.Add(new LabelValueItem((team.IsCurrent ? "▸ " : string.Empty) + team.Name,
                string.Format(CultureInfo.InvariantCulture, "{0} pts · {1} workers · {2} reports", team.Points, team.Workers, team.Reports)));
        }
    }

    /// <inheritdoc />
    public void PromptChanged(string prompt) => TickerText = prompt;

    /// <inheritdoc />
    public void GameEnded(FinalScore[] scores)
    {
        _finalScores = scores ?? Array.Empty<FinalScore>();
        GameInProgress = false;
        BuildScores();
        CloseInspector();
        Show(Pane.Epilogue);
    }

    /// <inheritdoc />
    public void JournalChanged(IReadOnlyList<JournalEntry> entries)
    {
        var all = entries ?? Array.Empty<JournalEntry>();
        var known = _journal.Count;
        _journal = all;
        if (all.Count < known)
        {
            _allJournalItems.Clear();
            known = 0;
        }

        foreach (var entry in all.Skip(known))
        {
            var item = new JournalItem(entry, Host?.GameSeed ?? 0);
            _allJournalItems.Add(item);
            _ = LoadArtAsync(item, 96);
        }

        FilterJournal();
        NotifyPropertyChanged(nameof(JournalCountText));
        ExportJournalCommand.RaiseCanExecuteChanged();
    }

    /// <inheritdoc />
    public void PaneRequested(string pane)
    {
        switch (pane)
        {
            case "journal":
                IsJournalOpen = !IsJournalOpen;
                break;
            case "gallery":
                if (IsGalleryOpen)
                {
                    IsGalleryOpen = false;
                }
                else
                {
                    OpenGallery();
                }

                break;
            case "escape":
                CloseTopmost();
                break;
            case "settings":
                IsSettingsOpen = !IsSettingsOpen;
                break;
        }
    }

    #endregion

    #region | Pictures |

    /// <summary>The half sunburst behind the title.</summary>
    public BitmapImage SunburstImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The title art.</summary>
    public BitmapImage TitleArtImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The Warka Herald masthead.</summary>
    public BitmapImage MastheadImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The EXTRA! badge.</summary>
    public BitmapImage ExtraBadgeImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The NOTICE TO ALL EXPEDITIONS heading.</summary>
    public BitmapImage NoticeImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The LEARNED SOCIETY heading.</summary>
    public BitmapImage LearnedSocietyImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The FINAL EDITION heading.</summary>
    public BitmapImage FinalEditionImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The stepped Deco corner bracket (top-left; the page turns it for the other corners).</summary>
    public BitmapImage CornerImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The chevron band used as a divider.</summary>
    public BitmapImage ChevronImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The tapering divider rule.</summary>
    public BitmapImage DividerImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>The drawn wordmark "Goddess Temple Discovery!".</summary>
    public BitmapImage WordmarkImage { get => field; private set => SetProperty(ref field, value); }

    private async Task LoadChromeArtAsync()
    {
        SunburstImage = await ArtImageAsync("deco-sunburst-half", 900);
        CornerImage = await ArtImageAsync("deco-corner-bracket", 160);
        ChevronImage = await ArtImageAsync("deco-chevron-band", 900);
        DividerImage = await ArtImageAsync("deco-divider-rule", 900);
        WordmarkImage = await ArtImageAsync("deco-wordmark-goddess-temple-discovery", 700);
        TitleArtImage = await ArtImageAsync("title-ziggurat-at-dawn", 600);
        MastheadImage = await ArtImageAsync("deco-masthead-warka-herald", 1000);
        ExtraBadgeImage = await ArtImageAsync("deco-extra-badge", 260);
        NoticeImage = await ArtImageAsync("deco-wordmark-notice", 700);
        LearnedSocietyImage = await ArtImageAsync("deco-wordmark-learned-society", 700);
        FinalEditionImage = await ArtImageAsync("deco-wordmark-final-edition", 800);
    }

    private static async Task<BitmapImage> ArtImageAsync(string key, int longSide)
    {
        try
        {
            var bytes = await Task.Run(() => SvgRaster.RenderArtPngCropped(key, longSide));
            return await ImageFromAsync(bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Goddess Temple Discovery: the picture {key} could not be loaded: {ex.Message}");
            return null;
        }
    }

    private static async Task LoadArtAsync(TextItem item, int longSide)
    {
        if (string.IsNullOrEmpty(item.ArtKey))
        {
            return;
        }

        try
        {
            var bytes = await Task.Run(() => SvgRaster.RenderArtPng(item.ArtKey, longSide));
            item.Image = await ImageFromAsync(bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Goddess Temple Discovery: the picture {item.ArtKey} could not be loaded: {ex.Message}");
        }
    }

    //Back on the UI thread after the await (the awaiter restores the dispatcher context), where BitmapImage is touched
    private static async Task<BitmapImage> ImageFromAsync(byte[] png)
    {
        if (png == null || png.Length == 0)
        {
            return null;
        }

        var image = new BitmapImage();
        using (var stream = new MemoryStream(png))
        {
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }

        return image;
    }

    #endregion
}
