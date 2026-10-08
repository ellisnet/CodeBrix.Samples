namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Where the current turn stands.</summary>
public enum GamePhase
{
    /// <summary>
    /// A Season card has just flipped and its effect applied; the season's first team has not rolled yet. The only
    /// legal action is <see cref="RollAction"/>, exactly as in <see cref="AwaitRoll"/>.
    /// </summary>
    SeasonStart,
    /// <summary>A turn has begun and the team must roll.</summary>
    AwaitRoll,
    /// <summary>The dice are rolled; the team digs, recruits, studies, surveys, publishes, or ends the turn.</summary>
    Spend,
    /// <summary>The team ended its turn over the hand limit and must discard (one <see cref="DiscardAction"/> per card).</summary>
    TurnEnd,
    /// <summary>The last season has ended and the final scores stand.</summary>
    GameOver,
}
