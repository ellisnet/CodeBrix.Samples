using System.Threading.Tasks;
using BrixInvaders.Game.Hosting;

namespace BrixInvaders.Game.Links;

/// <summary>The default <see cref="IExternalLinkOpener"/>: it only logs the link (no browser is wired yet).</summary>
public sealed class LoggingLinkOpener : IExternalLinkOpener
{
    /// <inheritdoc />
    public Task<bool> Open(string url)
    {
        GameLog.Write($"link: {url} (no link opener is wired, so nothing was opened)");
        return Task.FromResult(false);
    }
}
