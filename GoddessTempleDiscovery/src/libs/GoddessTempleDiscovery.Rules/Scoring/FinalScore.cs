namespace GoddessTempleDiscovery.Rules.Scoring;

/// <summary>One team's score, as section 7 of the design counts it.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="TeamIndex">The team's seat-order index.</param>
/// <param name="Published">Points of every published report (bonuses included) plus points scored by Favors.</param>
/// <param name="UnpublishedHalf">Half the printed points of the unpublished Discoveries, rounded down.</param>
/// <param name="Tablets">Two points per Tablet in hand.</param>
/// <param name="SetBonus">Three points per complete set of the four Tablet kinds.</param>
/// <param name="StarBonus">The Star of Holy Inanna: five points for the most star-marked Discoveries (ties share).</param>
/// <param name="Total">The sum.</param>
/// <param name="DiscoveryCount">Discoveries held, published and unpublished (the first tie-break).</param>
/// <param name="DeepestTier">The deepest tier among them, 0 for none (the second tie-break).</param>
/// <param name="StarCount">Star-marked Discoveries held, published and unpublished.</param>
/// <param name="Rank">1 for the winner; teams still tied after both tie-breaks share a rank.</param>
public sealed record FinalScore(
    string TeamName,
    int TeamIndex,
    int Published,
    int UnpublishedHalf,
    int Tablets,
    int SetBonus,
    int StarBonus,
    int Total,
    int DiscoveryCount,
    int DeepestTier,
    int StarCount,
    int Rank);
