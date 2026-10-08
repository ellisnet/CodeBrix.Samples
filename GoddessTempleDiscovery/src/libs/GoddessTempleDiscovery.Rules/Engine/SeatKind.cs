namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Who plays a seat.</summary>
public enum SeatKind
{
    /// <summary>A person at the table.</summary>
    Human,
    /// <summary>A computer team driven by <see cref="GoddessTempleDiscovery.Rules.Brains.ComputerBrain"/>.</summary>
    Computer,
}
