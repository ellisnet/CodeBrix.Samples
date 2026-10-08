namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>What a Favor of the Goddess grants.</summary>
public enum FavorEffect
{
    /// <summary>Roll a third die next turn and keep the best two.</summary>
    ExtraDieNextTurn,
    /// <summary>Draw a Tablet now.</summary>
    FreeTablet,
    /// <summary>Gain a Worker now.</summary>
    FreeWorker,
    /// <summary>Survey now, free.</summary>
    FreeSurvey,
    /// <summary>+2 on the next dig this turn or next.</summary>
    PlusTwoNextDig,
    /// <summary>Recruit from the Expedition Row now at one less.</summary>
    RecruitDiscount,
    /// <summary>Score one point now.</summary>
    OnePoint,
    /// <summary>The hand limit does not apply at the end of this turn.</summary>
    NoHandLimit,
}

/// <summary>A Favor card, drawn on doubles: a small gift and one true line about Holy Inanna.</summary>
/// <param name="Id">A stable kebab-case identifier.</param>
/// <param name="Title">The title.</param>
/// <param name="Effect">The gift.</param>
/// <param name="EffectText">The gift in the players' words.</param>
/// <param name="Fact">One true line about Her.</param>
/// <param name="ArtKey">The key of the vector art.</param>
/// <param name="Sources">The sources the fact rests on.</param>
public sealed record FavorCard(
    string Id,
    string Title,
    FavorEffect Effect,
    string EffectText,
    string Fact,
    string ArtKey,
    string Sources);
