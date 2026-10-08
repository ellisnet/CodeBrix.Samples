namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>The one effect a Season card applies to every team for that season (DESIGN.md section 8).</summary>
public enum SeasonEffect
{
    /// <summary>No effect; the story only.</summary>
    None,
    /// <summary>Every team surveys once, free, at the start of its first turn.</summary>
    FreeSurvey,
    /// <summary>Every team gains one Worker.</summary>
    GainWorker,
    /// <summary>Kassite and Old Babylonian sites (tier 3) cost one less.</summary>
    KassiteCheaper,
    /// <summary>Deep sounding sites (tier 9) cost one less.</summary>
    DeepCheaper,
    /// <summary>Every team may re-roll one die once this season.</summary>
    RerollOnce,
    /// <summary>Study draws two Tablets.</summary>
    StudyDrawsTwo,
    /// <summary>Recruit costs one less.</summary>
    RecruitCheaper,
    /// <summary>At most two Workers may be spent on one dig.</summary>
    WorkersCapped,
    /// <summary>Every report published this season scores one more.</summary>
    PublishBonus,
    /// <summary>The final season: the Mask enters the Site Row if it has not appeared; nothing is published automatically, so finds left in the crates score half.</summary>
    FinalSeason,
}

/// <summary>One of the twelve campaigns the game is played through, with its true story.</summary>
/// <param name="Index">0 to 11, in order.</param>
/// <param name="Year">The campaign winter, such as "1930/31".</param>
/// <param name="Title">A short title for the season.</param>
/// <param name="Director">Who directed the excavation that winter.</param>
/// <param name="Story">Two to four plain sentences shown when the season begins.</param>
/// <param name="LongStory">The fuller account for the inspector and the Field Journal.</param>
/// <param name="Effect">The effect on play.</param>
/// <param name="EffectText">The effect in the players' words.</param>
/// <param name="ArtKey">The key of the vector art for the season.</param>
/// <param name="Sources">The sources the story rests on.</param>
public sealed record SeasonCard(
    int Index,
    string Year,
    string Title,
    string Director,
    string Story,
    string LongStory,
    SeasonEffect Effect,
    string EffectText,
    string ArtKey,
    string Sources)
{
    /// <summary>The front-page banner headline for the season, in capitals; empty until the headline pass fills it.</summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>The sub-head under the banner; empty until the headline pass fills it.</summary>
    public string SubHead { get; init; } = string.Empty;
}
