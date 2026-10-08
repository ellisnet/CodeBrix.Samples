using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>One team at the table: a read-only view for the presentation, changed only by the <see cref="GameEngine"/>.</summary>
public sealed class TeamState
{
    internal TeamState(TeamProfile profile, string name, SeatKind kind, Temperament temperament, int index, int setupIndex)
    {
        Profile = profile;
        Name = name;
        Kind = kind;
        Temperament = temperament;
        Index = index;
        SetupIndex = setupIndex;
        Hand = new ReadOnlyCollection<DiscoveryCard>(HandList);
        Tablets = new ReadOnlyCollection<TabletCard>(TabletList);
        Specialists = new ReadOnlyCollection<SpecialistCard>(SpecialistList);
        Reports = new ReadOnlyCollection<Report>(ReportList);
    }

    /// <summary>The team profile (colour, emblem, motto).</summary>
    public TeamProfile Profile { get; }

    /// <summary>The team's name, unique at the table.</summary>
    public string Name { get; }

    /// <summary>Human or computer.</summary>
    public SeatKind Kind { get; }

    /// <summary>The computer temperament.</summary>
    public Temperament Temperament { get; }

    /// <summary>The team's place in the (shuffled) seat order, from 0.</summary>
    public int Index { get; }

    /// <summary>The index of the seat in <see cref="GameSetup.Seats"/> this team came from.</summary>
    public int SetupIndex { get; }

    /// <summary>The Workers the team holds.</summary>
    public int Workers { get; internal set; }

    /// <summary>The unpublished Discoveries, in the order they were excavated.</summary>
    public IReadOnlyList<DiscoveryCard> Hand { get; }

    /// <summary>The Tablets held.</summary>
    public IReadOnlyList<TabletCard> Tablets { get; }

    /// <summary>The Specialists recruited (at most four).</summary>
    public IReadOnlyList<SpecialistCard> Specialists { get; }

    /// <summary>The published reports, in order.</summary>
    public IReadOnlyList<Report> Reports { get; }

    /// <summary>Points of every published report plus points scored by Favors.</summary>
    public int PublishedPoints { get; internal set; }

    /// <summary>Star-marked Discoveries held, published and unpublished.</summary>
    public int StarCount => HandList.Count(c => c.IsStarred) + ReportList.Sum(r => r.Cards.Count(c => c.IsStarred));

    /// <summary>The cards counted against the hand limit: Discoveries and Tablets.</summary>
    public int HandCount => HandList.Count + TabletList.Count;

    /// <summary>True while the RerollOnce season's re-roll is unused.</summary>
    public bool RerollAvailable { get; internal set; }

    /// <summary>True while a PlusTwoNextDig Favor waits for the next dig.</summary>
    public bool PlusTwoPending => PlusTwoTurnsLeft > 0;

    /// <summary>True when the next roll uses three dice and keeps the best two.</summary>
    public bool ExtraDieNextTurn { get; internal set; }

    /// <summary>True when a free survey (the season's or a Favor's) is waiting; the Surveyor's own use is not counted here.</summary>
    public bool FreeSurveyPending => SeasonFreeSurvey || FavorFreeSurveys > 0;

    /// <summary>True when the next recruit costs one less (a RecruitDiscount Favor).</summary>
    public bool RecruitDiscountPending { get; internal set; }

    /// <summary>True when the hand limit does not apply at the end of this turn.</summary>
    public bool NoHandLimitThisTurn { get; internal set; }

    /// <summary>True once the Foreman's +1 has been used this turn.</summary>
    public bool ForemanUsedThisTurn { get; internal set; }

    /// <summary>True once the Surveyor's free survey has been used this turn.</summary>
    public bool SurveyorUsedThisTurn { get; internal set; }

    internal List<DiscoveryCard> HandList { get; } = new List<DiscoveryCard>();

    internal List<TabletCard> TabletList { get; } = new List<TabletCard>();

    internal List<SpecialistCard> SpecialistList { get; } = new List<SpecialistCard>();

    internal List<Report> ReportList { get; } = new List<Report>();

    internal bool SeasonFreeSurvey { get; set; }

    internal int FavorFreeSurveys { get; set; }

    internal int PlusTwoTurnsLeft { get; set; }

    /// <summary>True when the team holds a Specialist of this role.</summary>
    /// <param name="role">The role.</param>
    /// <returns>True when held.</returns>
    public bool HasRole(SpecialistRole role) => SpecialistList.Any(s => s.Role == role);
}
