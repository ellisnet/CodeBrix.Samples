using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GoddessTempleDiscovery.ViewModels;

/// <summary>One emblem a seat with a name of its own may carry: a team card's emblem, taken by at most one seat.</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class EmblemChoice : SimpleViewModel
{
    private readonly Action<EmblemChoice> _choose;

    /// <summary>Creates the choice.</summary>
    /// <param name="profile">The team card whose emblem (and colour) it is.</param>
    /// <param name="choose">Chooses it for the seat.</param>
    public EmblemChoice(TeamProfile profile, Action<EmblemChoice> choose)
    {
        Profile = profile;
        _choose = choose;
    }

    /// <summary>The team card whose emblem it is.</summary>
    public TeamProfile Profile { get; }

    /// <summary>The team card's id.</summary>
    public string ProfileId => Profile.Id;

    /// <summary>"The emblem of The Reed Marsh Expedition".</summary>
    public string Name => "The emblem of " + Profile.Name;

    /// <summary>The emblem, once rendered.</summary>
    public BitmapImage Image
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>True when this seat carries it.</summary>
    [AffectsProperties(nameof(ChosenVisibility))]
    public bool IsChosen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>True when another seat has it, by its team card or as its own emblem.</summary>
    [AffectsProperties(nameof(IsAvailable), nameof(TakenVisibility))]
    public bool IsTaken
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>True when the seat may choose it.</summary>
    public bool IsAvailable => !IsTaken;

    /// <summary>The gold frame of the chosen emblem.</summary>
    public Visibility ChosenVisibility => GetVisibility(IsChosen);

    /// <summary>The veil over an emblem another seat has.</summary>
    public Visibility TakenVisibility => GetVisibility(IsTaken);

    /// <summary>Chooses it for the seat.</summary>
    public SimpleCommand ChooseCommand => field ??= new SimpleCommand(() => _choose?.Invoke(this));
}

/// <summary>
/// One seat of the setup pane: a team card, or a name of its own with an emblem chosen from the team cards' emblems;
/// human or computer; and a temperament. A team card or an emblem belongs to one seat at a time - the view model
/// takes the others' choices out of a seat's lists (<see cref="SetTakenByOthers"/>).
/// </summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class SeatViewModel : SimpleViewModel
{
    /// <summary>The team-card choice that means "type your own name".</summary>
    public const string OwnName = "(type a name)";

    private readonly Action<SeatViewModel> _remove;
    private readonly Action _changed;
    private bool _quiet;

    /// <summary>Creates a seat.</summary>
    /// <param name="number">The seat's number, from 1.</param>
    /// <param name="record">The stored seat to start from, or null.</param>
    /// <param name="remove">Removes this seat from the setup.</param>
    /// <param name="changed">Called when anything of the seat changes.</param>
    public SeatViewModel(int number, SeatRecord record, Action<SeatViewModel> remove, Action changed)
    {
        _remove = remove;
        _quiet = true;
        Number = number;
        TeamOptions = new ObservableCollection<string>(new[] { OwnName }.Concat(Catalog.Teams.Select(t => t.Name)));
        Emblems = new ObservableCollection<EmblemChoice>(Catalog.Teams.Select(t => new EmblemChoice(t, ChooseEmblem)));

        //A stored team card with a typed name is the older form of a team of its own name carrying that card's emblem
        var profile = record == null ? null : Catalog.Teams.FirstOrDefault(t => t.Id == record.TeamProfileId);
        var typed = record?.TeamName ?? string.Empty;
        if (record == null)
        {
            SelectedTeam = Catalog.Teams.ElementAtOrDefault(number - 1)?.Name ?? OwnName;
        }
        else if (profile != null && string.IsNullOrWhiteSpace(typed))
        {
            SelectedTeam = profile.Name;
        }
        else
        {
            SelectedTeam = OwnName;
            EmblemProfileId = Catalog.Teams.Any(t => t.Id == record.EmblemProfileId) ? record.EmblemProfileId : profile?.Id ?? string.Empty;
        }

        TeamName = typed;
        IsComputer = record?.IsComputer ?? number > 1;
        Temperament = Enum.TryParse(record?.Temperament, out Temperament t) ? t.ToString() : TemperamentOptions[(number - 1) % TemperamentOptions.Count];
        _changed = changed;
        _quiet = false;
    }

    /// <summary>The seat's number, from 1.</summary>
    [AffectsProperties(nameof(SeatLabel))]
    public int Number
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>"SEAT 1" and so on.</summary>
    public string SeatLabel => "SEAT " + Number;

    /// <summary>The own-name choice first, then the team cards no other seat has.</summary>
    public ObservableCollection<string> TeamOptions { get; }

    /// <summary>Every team card's emblem, those other seats have veiled.</summary>
    public ObservableCollection<EmblemChoice> Emblems { get; }

    /// <summary>The temperaments a computer seat may take.</summary>
    public IReadOnlyList<string> TemperamentOptions { get; } = Enum.GetNames<Temperament>();

    /// <summary>The chosen team card, or <see cref="OwnName"/>.</summary>
    [AffectsProperties(nameof(NameBoxVisibility), nameof(EmblemVisibility))]
    public string SelectedTeam
    {
        get;
        set
        {
            var before = field;
            SetProperty(ref field, value ?? OwnName);
            //Turning a team card into a name of its own keeps the card's emblem, which the seat already holds
            if (field == OwnName && before != null && before != OwnName && Catalog.Teams.FirstOrDefault(t => t.Name == before) is { } kept)
            {
                EmblemProfileId = kept.Id;
            }

            Changed();
        }
    }

    /// <summary>The typed team name (a seat with a name of its own).</summary>
    public string TeamName
    {
        get;
        set
        {
            SetProperty(ref field, value ?? string.Empty);
            Changed();
        }
    } = string.Empty;

    /// <summary>The id of the team card whose emblem a seat with a name of its own carries, or empty.</summary>
    public string EmblemProfileId
    {
        get;
        set
        {
            SetProperty(ref field, value ?? string.Empty);
            foreach (var emblem in Emblems)
            {
                emblem.IsChosen = emblem.ProfileId == field;
            }

            Changed();
        }
    } = string.Empty;

    /// <summary>True for a seat with a name of its own (it then carries an emblem it chose).</summary>
    public bool HasOwnName => SelectedTeam == OwnName;

    /// <summary>The team card this seat holds - its own, or the one whose emblem it carries - or null.</summary>
    public string ClaimedProfileId => HasOwnName
        ? (string.IsNullOrEmpty(EmblemProfileId) ? null : EmblemProfileId)
        : Catalog.Teams.FirstOrDefault(t => t.Name == SelectedTeam)?.Id;

    /// <summary>True for a computer seat.</summary>
    [AffectsProperties(nameof(TemperamentVisibility), nameof(KindLabel))]
    public bool IsComputer
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Changed();
        }
    }

    /// <summary>"COMPUTER" or "HUMAN".</summary>
    public string KindLabel => IsComputer ? "COMPUTER" : "HUMAN";

    /// <summary>The computer temperament's name.</summary>
    public string Temperament
    {
        get;
        set => SetProperty(ref field, value ?? nameof(Rules.Engine.Temperament.Surveyor));
    }

    /// <summary>The temperament picker shows for computer seats only.</summary>
    public Visibility TemperamentVisibility => GetVisibility(IsComputer);

    /// <summary>The name box shows for a seat with a name of its own.</summary>
    public Visibility NameBoxVisibility => GetVisibility(HasOwnName);

    /// <summary>The emblems show for a seat with a name of its own.</summary>
    public Visibility EmblemVisibility => GetVisibility(HasOwnName);

    /// <summary>Removes the seat.</summary>
    public SimpleCommand RemoveCommand => field ??= new SimpleCommand(() => _remove?.Invoke(this));

    /// <summary>
    /// Takes the team cards the other seats hold out of this seat's choices: they leave the team list (this seat's
    /// own choice always stays) and their emblems are veiled.
    /// </summary>
    /// <param name="others">The team card ids the other seats hold.</param>
    public void SetTakenByOthers(ISet<string> others)
    {
        ArgumentNullException.ThrowIfNull(others);
        var wanted = new[] { OwnName }.Concat(Catalog.Teams.Where(t => !others.Contains(t.Id) || t.Name == SelectedTeam).Select(t => t.Name)).ToList();

        //Edited in place, never replaced, so the list's selected item (this seat's choice) is never disturbed
        for (var i = TeamOptions.Count - 1; i >= 0; i--)
        {
            if (!wanted.Contains(TeamOptions[i]))
            {
                TeamOptions.RemoveAt(i);
            }
        }

        for (var i = 0; i < wanted.Count; i++)
        {
            if (i >= TeamOptions.Count || TeamOptions[i] != wanted[i])
            {
                TeamOptions.Insert(i, wanted[i]);
            }
        }

        foreach (var emblem in Emblems)
        {
            emblem.IsTaken = others.Contains(emblem.ProfileId);
        }
    }

    /// <summary>The engine's seat.</summary>
    /// <returns>The seat setup.</returns>
    public SeatSetup ToSetup()
    {
        var temperament = Enum.TryParse(Temperament, out Temperament t) ? t : Rules.Engine.Temperament.Surveyor;
        var kind = IsComputer ? SeatKind.Computer : SeatKind.Human;
        if (!HasOwnName)
        {
            return new SeatSetup(null, ClaimedProfileId, kind, temperament);
        }

        //A name of its own on the emblem's team card: the engine keeps the card's emblem and colour under the new name
        var name = string.IsNullOrWhiteSpace(TeamName) ? "Expedition " + Number : TeamName.Trim();
        return new SeatSetup(name, ClaimedProfileId, kind, temperament);
    }

    /// <summary>The stored form.</summary>
    /// <returns>The record.</returns>
    public SeatRecord ToRecord() => new SeatRecord
    {
        TeamName = HasOwnName ? TeamName : string.Empty,
        TeamProfileId = HasOwnName ? string.Empty : ClaimedProfileId ?? string.Empty,
        EmblemProfileId = HasOwnName ? EmblemProfileId : string.Empty,
        IsComputer = IsComputer,
        Temperament = Temperament,
    };

    private void ChooseEmblem(EmblemChoice emblem)
    {
        if (emblem != null && !emblem.IsTaken)
        {
            EmblemProfileId = emblem.ProfileId;
        }
    }

    private void Changed()
    {
        if (!_quiet)
        {
            _changed?.Invoke();
        }
    }
}
