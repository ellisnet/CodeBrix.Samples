using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Downloads every referenced media file once per run into a private temp folder,
/// and deletes the folder at the end. Notion's uploaded-file URLs are pre-signed
/// and expire in about an hour, so downloads always happen in the same run that
/// fetched the block tree — cached URLs are never persisted or reused later.
/// </summary>
internal sealed class MediaCache : IDisposable
{
    /// <summary>Media larger than this is not downloaded (a card is rendered instead).</summary>
    public const long DefaultMaxDownloadBytes = 100L * 1024 * 1024;

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(3) };
    private readonly Dictionary<string, CachedMedia> _byUrl = new(StringComparer.Ordinal);
    private int _fileNumber;
    private bool _isDisposed;

    /// <summary>The temp folder holding this run's downloads (deleted on dispose).</summary>
    public string CacheDirectory { get; } =
        Path.Combine(Path.GetTempPath(), "NotionDocumentCreator", Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Fetches a URL to a local file, once per run. Failures (including
    /// too-large content) return an unsuccessful result with a reason —
    /// they never throw, so a bad download can never fail the document.
    /// </summary>
    public async Task<CachedMedia> FetchAsync(
        string url, long maxBytes = DefaultMaxDownloadBytes, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (string.IsNullOrWhiteSpace(url))
        {
            return CachedMedia.Failed("No URL was supplied for the media file.");
        }
        if (_byUrl.TryGetValue(url, out var cached)) { return cached; }

        var result = await DownloadAsync(url, maxBytes, cancellationToken).ConfigureAwait(false);
        _byUrl[url] = result;
        return result;
    }

    private async Task<CachedMedia> DownloadAsync(
        string url, long maxBytes, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _http.GetAsync(
                url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return CachedMedia.Failed($"Download failed with HTTP {(int)response.StatusCode}.");
            }

            var declaredLength = response.Content.Headers.ContentLength;
            if (declaredLength > maxBytes)
            {
                return CachedMedia.Failed(
                    $"File is {declaredLength / (1024.0 * 1024.0):F0} MB — larger than the download cap.");
            }

            Directory.CreateDirectory(CacheDirectory);
            var filePath = Path.Combine(CacheDirectory, $"media-{++_fileNumber:D4}");

            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var target = File.Create(filePath))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    total += read;
                    if (total > maxBytes)
                    {
                        //The server did not declare a length — enforce the cap while streaming
                        target.Close();
                        File.Delete(filePath);
                        return CachedMedia.Failed("File exceeded the download cap.");
                    }
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                }

                return CachedMedia.Succeeded(filePath, total);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; //A user cancel should cancel the run, not become a warning
        }
        catch (Exception ex)
        {
            return CachedMedia.Failed($"Download failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_isDisposed) { return; }
        _isDisposed = true;
        _http.Dispose();
        try
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, recursive: true);
            }
        }
        catch (Exception)
        {
            //A locked temp file must not crash disposal; the OS temp cleaner will get it
        }
    }
}

/// <summary>The outcome of one media download.</summary>
internal sealed class CachedMedia
{
    /// <summary>Whether the file was downloaded.</summary>
    public bool Success { get; private init; }

    /// <summary>Local path of the downloaded file (when successful).</summary>
    public string FilePath { get; private init; } = "";

    /// <summary>Downloaded length in bytes.</summary>
    public long Length { get; private init; }

    /// <summary>Why the download failed (when unsuccessful).</summary>
    public string FailureReason { get; private init; } = "";

    public static CachedMedia Succeeded(string filePath, long length) =>
        new() { Success = true, FilePath = filePath, Length = length };

    public static CachedMedia Failed(string reason) =>
        new() { Success = false, FailureReason = reason };
}

/// <summary>
/// Media made ready for the renderer during the pre-render download pass:
/// a processed image (or video poster frame), plus probed metadata.
/// </summary>
internal sealed class PreparedMedia
{
    /// <summary>The print-ready image (an image block's picture, or a video's poster frame).</summary>
    public ProcessedImage Image { get; init; }

    /// <summary>Media duration, when ffprobe reported one (video/audio).</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Size of the source file in bytes (0 when unknown).</summary>
    public long SourceLength { get; init; }

    /// <summary>Why the media could not be prepared (null/empty when it could).</summary>
    public string FailureReason { get; init; } = "";

    /// <summary>Whether a print-ready image is available.</summary>
    public bool HasImage => Image is not null;
}
