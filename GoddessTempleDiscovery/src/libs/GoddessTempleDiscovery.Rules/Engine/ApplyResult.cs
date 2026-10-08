using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The outcome of <see cref="GameEngine.Apply"/>.</summary>
/// <param name="Action">The action applied.</param>
/// <param name="Events">The events the action raised, in order.</param>
public sealed record ApplyResult(GameAction Action, IReadOnlyList<GameEvent> Events);
