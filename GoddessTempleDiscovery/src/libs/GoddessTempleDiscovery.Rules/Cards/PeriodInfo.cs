namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>One period of the timeline, for the timeline screen and the period band on cards.</summary>
/// <param name="Period">The period.</param>
/// <param name="Name">The display name.</param>
/// <param name="Years">The years, such as "about 3500 to 3300 BCE".</param>
/// <param name="Levels">The Eanna levels, or empty.</param>
/// <param name="ArtKey">The key of the period icon.</param>
/// <param name="Summary">Two to four plain sentences on what happened at Uruk then.</param>
/// <param name="Sources">The sources.</param>
public sealed record PeriodInfo(Period Period, string Name, string Years, string Levels, string ArtKey, string Summary, string Sources);
