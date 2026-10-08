using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using Microsoft.UI.Xaml;

namespace GoddessTempleDiscovery.ViewModels;

/// <summary>One seat of the setup pane: a team name or a team card, human or computer, and a temperament.</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class SeatViewModel : SimpleViewModel
{
    /// <summary>The team-card choice that means "type your own name".</summary>
    public const string OwnName = "(type a name)";

    private readonly Action<SeatViewModel> _remove;
    private readonly Action _changed;

    /// <summary>Creates a seat.</summary>
    /// <param name="number">The seat's number, from 1.</param>
    /// <param name="record">The stored seat to start from, or null.</param>
    /// <param name="remove">Removes this seat from the setup.</param>
    /// <param name="changed">Called when anything of the seat changes.</param>
    public SeatViewModel(int number, SeatRecord record, Action<SeatViewModel> remove, Action changed)
    {
        _remove = remove;
        _changed = changed;
        Number = number;
        var profile = record == null ? null : Catalog.Teams.FirstOrDefault(t => t.Id == record.TeamProfileId);
        SelectedTeam = profile?.Name ?? (record == null ? Catalog.Teams.ElementAtOrDefault(number - 1)?.Name ?? OwnName : OwnName);
        TeamName = record?.TeamName ?? string.Empty;
        IsComputer = record?.IsComputer ?? number > 1;
        Temperament = Enum.TryParse(record?.Temperament, out Temperament t) ? t.ToString() : TemperamentOptions[(number - 1) % TemperamentOptions.Count];
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

    /// <summary>The team cards a seat may take, and the own-name choice first.</summary>
    public IReadOnlyList<string> TeamOptions { get; } = new[] { OwnName }.Concat(Catalog.Teams.Select(t => t.Name)).ToArray();

    /// <summary>The temperaments a computer seat may take.</summary>
    public IReadOnlyList<string> TemperamentOptions { get; } = Enum.GetNames<Temperament>();

    /// <summary>The chosen team card, or <see cref="OwnName"/>.</summary>
    [AffectsProperties(nameof(NameBoxVisibility))]
    public string SelectedTeam
    {
        get;
        set
        {
            SetProperty(ref field, value ?? OwnName);
            _changed?.Invoke();
        }
    }

    /// <summary>The typed team name (used when no team card is chosen, or to rename one).</summary>
    public string TeamName
    {
        get;
        set
        {
            SetProperty(ref field, value ?? string.Empty);
            _changed?.Invoke();
        }
    } = string.Empty;

    /// <summary>True for a computer seat.</summary>
    [AffectsProperties(nameof(TemperamentVisibility), nameof(KindLabel))]
    public bool IsComputer
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _changed?.Invoke();
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

    /// <summary>The name box shows when no team card is chosen.</summary>
    public Visibility NameBoxVisibility => GetVisibility(SelectedTeam == OwnName);

    /// <summary>Removes the seat.</summary>
    public SimpleCommand RemoveCommand => field ??= new SimpleCommand(() => _remove?.Invoke(this));

    /// <summary>The engine's seat.</summary>
    /// <returns>The seat setup.</returns>
    public SeatSetup ToSetup()
    {
        var profile = Catalog.Teams.FirstOrDefault(t => t.Name == SelectedTeam);
        var temperament = Enum.TryParse(Temperament, out Temperament t) ? t : Rules.Engine.Temperament.Surveyor;
        var name = string.IsNullOrWhiteSpace(TeamName) ? null : TeamName.Trim();
        if (profile == null && name == null)
        {
            name = "Expedition " + Number;
        }

        return new SeatSetup(name, profile?.Id, IsComputer ? SeatKind.Computer : SeatKind.Human, temperament);
    }

    /// <summary>The stored form.</summary>
    /// <returns>The record.</returns>
    public SeatRecord ToRecord() => new SeatRecord
    {
        TeamName = TeamName,
        TeamProfileId = Catalog.Teams.FirstOrDefault(t => t.Name == SelectedTeam)?.Id ?? string.Empty,
        IsComputer = IsComputer,
        Temperament = Temperament,
    };
}
