namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>The six fictional roles a team can recruit.</summary>
public enum SpecialistRole
{
    /// <summary>+1 on building sites.</summary>
    Architect,
    /// <summary>+1 on inscription sites; Study draws two.</summary>
    Epigrapher,
    /// <summary>+1 on object and deposit sites.</summary>
    SmallFindsKeeper,
    /// <summary>+1 to every report published.</summary>
    Photographer,
    /// <summary>+1 on any one dig per turn.</summary>
    Foreman,
    /// <summary>Survey is free once per turn.</summary>
    Surveyor,
}

/// <summary>One Specialist card of the Expedition deck. The roles are fictional; the note names the real person who did that work at Uruk.</summary>
/// <param name="Role">The role.</param>
/// <param name="Title">The title printed on the card.</param>
/// <param name="Cost">The die value needed to recruit, 2 to 5.</param>
/// <param name="ArtKey">The key of the vector art drawn on the card face.</param>
/// <param name="CardText">What the specialist does for the team, in plain words.</param>
/// <param name="HistoricalNote">Who really did this work at Uruk, with the season.</param>
/// <param name="Sources">The sources the note rests on.</param>
public sealed record SpecialistCard(
    SpecialistRole Role,
    string Title,
    int Cost,
    string ArtKey,
    string CardText,
    string HistoricalNote,
    string Sources);
