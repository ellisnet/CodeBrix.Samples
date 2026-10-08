using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>What a dig would come to: the number to reach, the total, and every modifier that counts.</summary>
/// <param name="Needed">The site's Dig Number after difficulty and the season.</param>
/// <param name="Total">The die value or sum plus every modifier.</param>
/// <param name="CanDig">True when the dig is legal and <paramref name="Total"/> reaches <paramref name="Needed"/>.</param>
/// <param name="Modifiers">Readable lines such as "Architect +1" or "2 Workers +2".</param>
public sealed record DigPreview(int Needed, int Total, bool CanDig, IReadOnlyList<string> Modifiers);
