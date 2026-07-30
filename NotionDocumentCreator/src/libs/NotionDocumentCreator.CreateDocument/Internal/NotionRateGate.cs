using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Serialises Notion API calls and enforces a minimum delay between them, keeping
/// the app inside Notion's published rate limit of roughly three requests per
/// second average. The NotionApi client's resilience layer already retries
/// transient 429s; this gate keeps us from provoking them in the first place.
/// </summary>
internal sealed class NotionRateGate : IDisposable
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(350);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastCallCompleted = DateTimeOffset.MinValue;
    private bool _isDisposed;

    /// <summary>
    /// Runs one API call through the gate: waits its turn, enforces the minimum
    /// inter-call delay, then invokes the call.
    /// </summary>
    public async Task<T> RunAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(apiCall);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var sinceLast = DateTimeOffset.UtcNow - _lastCallCompleted;
            if (sinceLast < MinimumInterval)
            {
                await Task.Delay(MinimumInterval - sinceLast, cancellationToken).ConfigureAwait(false);
            }

            return await apiCall().ConfigureAwait(false);
        }
        finally
        {
            _lastCallCompleted = DateTimeOffset.UtcNow;
            _gate.Release();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) { return; }
        _isDisposed = true;
        _gate.Dispose();
    }
}
