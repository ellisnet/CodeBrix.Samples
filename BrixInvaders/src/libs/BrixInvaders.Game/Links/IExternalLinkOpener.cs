using System.Threading.Tasks;

namespace BrixInvaders.Game.Links;

/// <summary>
/// SEAM (implemented by the app): opens a web link in the player's browser. The Kenney cards (K / gamepad Y / a
/// click on the bundle card) and the credits links call it.
/// </summary>
/// <remarks>
/// <see cref="Open"/> is called on the ENGINE thread; an implementation that needs the UI thread (a launcher)
/// marshals there itself and completes the task when it knows the answer. A false result shows
/// "No browser was available." on the screen for a few seconds.
/// </remarks>
public interface IExternalLinkOpener
{
    /// <summary>Opens a link.</summary>
    /// <param name="url">The absolute URL.</param>
    /// <returns>True when a browser took the link; false when none was available.</returns>
    Task<bool> Open(string url);
}
