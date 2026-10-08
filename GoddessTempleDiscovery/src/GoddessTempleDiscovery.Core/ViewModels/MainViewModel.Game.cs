using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;

namespace GoddessTempleDiscovery.ViewModels;

public partial class MainViewModel
{
    #region | Setup |

    /// <summary>The seats, two to four.</summary>
    public ObservableCollection<SeatViewModel> Seats { get; } = new ObservableCollection<SeatViewModel>();

    /// <summary>The turns-per-season choices.</summary>
    public IReadOnlyList<string> TurnsOptions { get; } = new[] { "1", "2", "3" };

    /// <summary>The chosen turns per team per season ("1", "2" or "3").</summary>
    public string SelectedTurns
    {
        get;
        set => SetProperty(ref field, value ?? "2");
    } = "2";

    /// <summary>The difficulty choices.</summary>
    public IReadOnlyList<string> DifficultyOptions { get; } = Enum.GetNames<Difficulty>();

    /// <summary>The chosen difficulty.</summary>
    public string SelectedDifficulty
    {
        get;
        set => SetProperty(ref field, value ?? nameof(Difficulty.Standard));
    } = nameof(Difficulty.Standard);

    /// <summary>An optional seed for a repeatable game (digits; empty for a fresh game).</summary>
    [AffectsCommands(nameof(StartCommand))]
    [AffectsProperties(nameof(SetupProblem))]
    public string SeedText
    {
        get;
        set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>Why the game cannot start yet, or empty.</summary>
    public string SetupProblem
    {
        get
        {
            if (Seats.Count < GameRules.MinSeats || Seats.Count > GameRules.MaxSeats)
            {
                return "A game has two to four seats.";
            }

            if (SeedText.Trim().Length > 0 && !int.TryParse(SeedText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                return "The seed is a whole number, or empty for a fresh game.";
            }

            return string.Empty;
        }
    }

    /// <summary>"Two seats: three turns a season by default" and so on.</summary>
    public string TurnsHint => string.Format(CultureInfo.InvariantCulture,
        "With {0} seats the game suggests {1} turn{2} per team per season; any of 1, 2 or 3 may be chosen.",
        Seats.Count, GameRules.DefaultTurnsPerSeason(Seats.Count), GameRules.DefaultTurnsPerSeason(Seats.Count) == 1 ? string.Empty : "s");

    /// <summary>Adds a seat (up to four).</summary>
    public SimpleCommand AddSeatCommand => field ??= new SimpleCommand(() => Seats.Count < GameRules.MaxSeats, () =>
    {
        AddSeat(null);
        SeatsChanged();
    });

    /// <summary>Starts the game: stores the setup and shows the prologue.</summary>
    public SimpleCommand StartCommand => field ??= new SimpleCommand(() => SetupProblem.Length == 0, () =>
    {
        if (SetupProblem.Length > 0)
        {
            return;
        }

        SettingsService.SaveLastSeats(Seats.Select(s => s.ToRecord()));
        SettingsService.TurnsPerSeason = Turns();
        SettingsService.Difficulty = Enum.TryParse(SelectedDifficulty, out Difficulty d) ? d : Difficulty.Standard;
        Show(Pane.Prologue);
    });

    private void OpenSetup()
    {
        Seats.Clear();
        var stored = SettingsService.LoadLastSeats();
        foreach (var record in stored.Take(GameRules.MaxSeats))
        {
            AddSeat(record);
        }

        while (Seats.Count < GameRules.MinSeats)
        {
            AddSeat(null);
        }

        SelectedDifficulty = SettingsService.Difficulty.ToString();
        SeedText = string.Empty;
        SeatsChanged();
        Show(Pane.Setup);
    }

    private void AddSeat(SeatRecord record)
    {
        var seat = new SeatViewModel(Seats.Count + 1, record, RemoveSeat, () => StartCommand.RaiseCanExecuteChanged());
        Seats.Add(seat);
    }

    private void RemoveSeat(SeatViewModel seat)
    {
        if (Seats.Count <= GameRules.MinSeats)
        {
            return;
        }

        Seats.Remove(seat);
        for (var i = 0; i < Seats.Count; i++)
        {
            Seats[i].Number = i + 1;
        }

        SeatsChanged();
    }

    //A change in the seat count preselects the rules' default season length for that many seats
    private void SeatsChanged()
    {
        SelectedTurns = GameRules.DefaultTurnsPerSeason(Seats.Count).ToString(CultureInfo.InvariantCulture);
        NotifyPropertyChanged(nameof(TurnsHint));
        NotifyPropertyChanged(nameof(SetupProblem));
        AddSeatCommand.RaiseCanExecuteChanged();
        StartCommand.RaiseCanExecuteChanged();
    }

    private int Turns() => int.TryParse(SelectedTurns, out var turns) ? Math.Clamp(turns, GameRules.MinTurnsPerSeason, GameRules.MaxTurnsPerSeason) : 2;

    private GameSetup BuildSetup()
    {
        int? seed = int.TryParse(SeedText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : null;
        var difficulty = Enum.TryParse(SelectedDifficulty, out Difficulty d) ? d : Difficulty.Standard;
        return new GameSetup(Seats.Select(seat => seat.ToSetup()).ToArray(), Turns(), difficulty, seed);
    }

    private void StartAutoPlay()
    {
        Seats.Clear();
        var temperaments = Enum.GetNames<Temperament>();
        for (var i = 0; i < GameRules.MaxSeats; i++)
        {
            AddSeat(new SeatRecord
            {
                TeamProfileId = Catalog.Teams.ElementAtOrDefault(i)?.Id ?? string.Empty,
                IsComputer = true,
                Temperament = temperaments[i % temperaments.Length],
            });
        }

        SeatsChanged();
        SelectedTurns = "1";
        SeedText = AutoPlay.Seed()?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        BeginGame();
    }

    private void BeginGame()
    {
        _journal = Array.Empty<JournalEntry>();
        _allJournalItems.Clear();
        JournalItems.Clear();
        _finalScores = Array.Empty<Rules.Scoring.FinalScore>();
        GameInProgress = true;
        TickerText = "WARKA — The expeditions arrive.";
        Show(Pane.Play);
        Host?.StartGame(BuildSetup());
    }

    #endregion

    #region | Prologue, epilogue and scores |

    /// <summary>The prologue's title.</summary>
    public string PrologueTitle => Catalog.Prologue.FirstOrDefault() ?? string.Empty;

    /// <summary>The prologue's paragraphs.</summary>
    public IReadOnlyList<string> PrologueParagraphs => Catalog.Prologue.Skip(1).ToArray();

    /// <summary>Leaves the prologue for the table: the game begins.</summary>
    public SimpleCommand BeginCommand => field ??= new SimpleCommand(BeginGame);

    /// <summary>The epilogue's title.</summary>
    public string EpilogueTitle => Catalog.Epilogue.FirstOrDefault() ?? string.Empty;

    /// <summary>The epilogue's paragraphs.</summary>
    public IReadOnlyList<string> EpilogueParagraphs => Catalog.Epilogue.Skip(1).ToArray();

    /// <summary>Leaves the epilogue for the Final Edition.</summary>
    public SimpleCommand ShowScoresCommand => field ??= new SimpleCommand(() => Show(Pane.Scores));

    /// <summary>The Final Edition's results table.</summary>
    public ObservableCollection<ScoreRow> ScoreRows { get; } = new ObservableCollection<ScoreRow>();

    /// <summary>The Final Edition's banner line.</summary>
    public string ScoresHeadline => "Goddess Temple Discovery! — Final Edition";

    /// <summary>Who won, in a sentence.</summary>
    public string WinnerText
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>Back to the setup for another game.</summary>
    public SimpleCommand PlayAgainCommand => field ??= new SimpleCommand(OpenSetup);

    private void BuildScores()
    {
        ScoreRows.Clear();
        foreach (var score in _finalScores.OrderBy(s => s.Rank).ThenBy(s => s.TeamIndex))
        {
            ScoreRows.Add(new ScoreRow
            {
                Rank = N(score.Rank),
                Team = score.TeamName,
                Published = N(score.Published),
                Unpublished = N(score.UnpublishedHalf),
                Tablets = N(score.Tablets),
                Sets = N(score.SetBonus),
                Star = N(score.StarBonus),
                Total = N(score.Total),
            });
        }

        var winners = _finalScores.Where(s => s.Rank == 1).Select(s => s.TeamName).ToArray();
        WinnerText = winners.Length switch
        {
            0 => string.Empty,
            1 => winners[0] + " has made the greatest discoveries.",
            _ => string.Join(" and ", winners) + " share the honours.",
        };
    }

    private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

    #endregion
}
