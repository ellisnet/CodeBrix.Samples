namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The difficulty chosen at setup: it shifts every Dig Number.</summary>
public enum Difficulty
{
    /// <summary>Every Dig Number is one less.</summary>
    Easy,
    /// <summary>Dig Numbers as printed.</summary>
    Standard,
    /// <summary>Every Dig Number is one more.</summary>
    Hard,
}
