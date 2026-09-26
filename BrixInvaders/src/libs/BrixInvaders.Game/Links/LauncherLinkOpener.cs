using System;
using System.Threading.Tasks;
using BrixInvaders.Game.Hosting;
using CodeBrix.Platform.GameEngine.Host.Links;

namespace BrixInvaders.Game.Links;

/// <summary>
/// The game's <see cref="IExternalLinkOpener"/>: opens web links in the player's browser through the engine's
/// <see cref="ExternalLinks"/> (which reaches the UI thread itself), and answers false - so the screen shows
/// "No browser was available." - when the link is not a web address or the launcher refuses or fails.
/// </summary>
public sealed class LauncherLinkOpener : IExternalLinkOpener
{
    private readonly Func<Uri, Task<bool>> _open;

    /// <summary>Creates the opener.</summary>
    /// <param name="open">Opens a URI; null for the engine's <see cref="ExternalLinks.OpenAsync(Uri)"/>.</param>
    public LauncherLinkOpener(Func<Uri, Task<bool>> open = null)
    {
        _open = open ?? (uri => ExternalLinks.OpenAsync(uri));
    }

    /// <inheritdoc />
    public async Task<bool> Open(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            GameLog.Write($"link: '{url}' is not a web address - nothing was opened");
            return false;
        }

        var opened = await _open(uri);
        GameLog.Write(opened ? $"link: the browser took {uri}" : $"link: {uri} could not be opened");
        return opened;
    }
}
