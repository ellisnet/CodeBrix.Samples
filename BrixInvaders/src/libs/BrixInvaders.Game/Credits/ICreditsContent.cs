using System.Collections.Generic;

namespace BrixInvaders.Game.Credits;

/// <summary>
/// SEAM (filled in by the music wiring): what the credits screen shows. The screen renders whatever lines this
/// returns, top to bottom; <see cref="CreditsLineStyle.Link"/> lines are clickable and open through the link seam.
/// </summary>
/// <remarks>Called on the engine thread each time the credits screen is built (not every frame).</remarks>
public interface ICreditsContent
{
    /// <summary>The credits lines, top to bottom.</summary>
    /// <returns>The lines.</returns>
    IReadOnlyList<CreditsLine> GetLines();
}
