using System;
using System.Threading.Tasks;
using BrixInvaders.Game.Hosting;

namespace BrixInvaders.Game.Links;

/// <summary>
/// The game's <see cref="IExternalLinkOpener"/>: opens a link in the player's browser through the platform launcher
/// (<c>Windows.System.Launcher.LaunchUriAsync</c>) on the UI thread, and answers false - so the screen shows
/// "No browser was available." - when the link is not a web address, the UI thread cannot take the work, or the
/// launcher refuses or fails.
/// </summary>
public sealed class LauncherLinkOpener : IExternalLinkOpener
{
    private readonly Func<Action, bool> _postToUiThread;
    private readonly Func<Uri, Task<bool>> _launch;

    /// <summary>Creates the opener.</summary>
    /// <param name="postToUiThread">
    /// Queues work on the UI thread (for example <c>DispatcherQueue.TryEnqueue</c>); returns false when it could not.
    /// </param>
    /// <param name="launch">Opens a URI; null for the platform launcher (<c>Windows.System.Launcher.LaunchUriAsync</c>).</param>
    public LauncherLinkOpener(Func<Action, bool> postToUiThread, Func<Uri, Task<bool>> launch = null)
    {
        _postToUiThread = postToUiThread ?? throw new ArgumentNullException(nameof(postToUiThread));
        _launch = launch ?? LaunchWithPlatformAsync;
    }

    /// <inheritdoc />
    public Task<bool> Open(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            GameLog.Write($"link: '{url}' is not a web address - nothing was opened");
            return Task.FromResult(false);
        }

        var answer = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool queued;
        try
        {
            queued = _postToUiThread(() => _ = LaunchOnUiThreadAsync(uri, answer));
        }
        catch (Exception failure)
        {
            GameLog.Write($"link: the UI thread could not take {uri}: {failure.Message}");
            queued = false;
        }

        if (!queued)
        {
            answer.TrySetResult(false);
        }

        return answer.Task;
    }

    private static async Task<bool> LaunchWithPlatformAsync(Uri uri) => await Windows.System.Launcher.LaunchUriAsync(uri);

    private async Task LaunchOnUiThreadAsync(Uri uri, TaskCompletionSource<bool> answer)
    {
        try
        {
            var opened = await _launch(uri);
            GameLog.Write(opened ? $"link: the browser took {uri}" : $"link: the launcher refused {uri}");
            answer.TrySetResult(opened);
        }
        catch (Exception failure)
        {
            GameLog.Write($"link: {uri} could not be opened: {failure.GetType().Name}: {failure.Message}");
            answer.TrySetResult(false);
        }
    }
}
