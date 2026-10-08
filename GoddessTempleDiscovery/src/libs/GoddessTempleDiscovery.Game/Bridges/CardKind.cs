namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>What kind of card a <see cref="CardView"/> shows, which chooses its newspaper layout.</summary>
public enum CardKind
{
    /// <summary>A Discovery of the Site deck: an EXTRA! edition.</summary>
    Discovery,
    /// <summary>A Tablet: a column of the Learned Society.</summary>
    Tablet,
    /// <summary>A Specialist of the Expedition deck.</summary>
    Specialist,
    /// <summary>A Favor of the Goddess: a small boxed notice.</summary>
    Favor,
    /// <summary>A Season card: the front page of the season.</summary>
    Season,
    /// <summary>A real person of the excavations (the History screen).</summary>
    Person,
}
