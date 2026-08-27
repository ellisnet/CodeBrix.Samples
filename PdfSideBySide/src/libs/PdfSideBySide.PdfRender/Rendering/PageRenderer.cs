using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.PdfRasterizer;
using PdfSideBySide.PdfRender.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PdfSideBySide.PdfRender.Rendering;

/// <summary>
/// Rasterizes pages of <see cref="PdfPageDocument"/>s to PNG through CodeBrix.PdfRasterizer,
/// keeping a small most-recently-used cache so stepping back and forth between neighbouring
/// pages does not re-render them.
/// </summary>
public sealed class PageRenderer : IDisposable
{
    /// <summary>The rendering resolution used when none is set: comfortable for on-screen comparison.</summary>
    public const int DefaultDpi = 150;

    /// <summary>The number of rendered pages the cache holds when none is set.</summary>
    public const int DefaultCacheCapacity = 12;

    private readonly PageRasterizer _rasterizer = new();
    private readonly Dictionary<string, RenderedPage> _cache = new();
    private readonly LinkedList<string> _cacheOrder = new(); //Most recently used at the front
    private readonly Lock _cacheLock = new();
    private bool _disposed;

    /// <summary>
    /// Creates a renderer; cacheCapacity bounds how many rendered pages are kept
    /// (values below 1 disable caching).
    /// </summary>
    public PageRenderer(int cacheCapacity = DefaultCacheCapacity)
    {
        CacheCapacity = Math.Max(0, cacheCapacity);
    }

    /// <summary>
    /// The rendering resolution in dots per inch. Setting a value below 1 restores
    /// <see cref="DefaultDpi"/>. Changing it clears the cache, since cached pages were
    /// rendered at the old size.
    /// </summary>
    public int Dpi
    {
        get;
        set
        {
            var dpi = value < 1 ? DefaultDpi : value;
            if (field == dpi) { return; }
            field = dpi;
            ClearCache();
        }
    } = DefaultDpi;

    /// <summary>The maximum number of rendered pages kept in the cache.</summary>
    public int CacheCapacity { get; }

    /// <summary>The number of rendered pages currently cached.</summary>
    public int CachedPageCount
    {
        get { lock (_cacheLock) { return _cache.Count; } }
    }

    /// <summary>Renders the page document's cursor is on at the default <see cref="Dpi"/> - see <see cref="RenderPageAsync(PdfPageDocument, int, int, CancellationToken)"/>.</summary>
    public Task<RenderedPage> RenderCurrentPageAsync(PdfPageDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        return RenderPageAsync(document, document.CurrentPage, Dpi, cancellationToken);
    }

    /// <summary>Renders the page document's cursor is on at dpi - see <see cref="RenderPageAsync(PdfPageDocument, int, int, CancellationToken)"/>.</summary>
    public Task<RenderedPage> RenderCurrentPageAsync(PdfPageDocument document, int dpi, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        return RenderPageAsync(document, document.CurrentPage, dpi, cancellationToken);
    }

    /// <summary>Renders the 1-based pageNumber of document at the default <see cref="Dpi"/> - see <see cref="RenderPageAsync(PdfPageDocument, int, int, CancellationToken)"/>.</summary>
    public Task<RenderedPage> RenderPageAsync(PdfPageDocument document, int pageNumber, CancellationToken cancellationToken = default) =>
        RenderPageAsync(document, pageNumber, Dpi, cancellationToken);

    /// <summary>
    /// Renders the 1-based pageNumber of document at dpi, returning the cached
    /// result when that page was rendered at that resolution recently.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">pageNumber is outside the document's page range, or dpi is below 1.</exception>
    public async Task<RenderedPage> RenderPageAsync(PdfPageDocument document, int pageNumber, int dpi, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageNumber, document.PageCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(dpi, 1);

        var key = CacheKey(document, pageNumber, dpi);
        if (TryGetCached(key, out var cached)) { return cached; }

        //PDFium renders synchronously, so keep it off the caller's (UI) thread
        var pngBytes = await Task.Run(async () =>
        {
            using var image = await _rasterizer.RasterizeToImage(
                document.PdfBytes, pageNumber, dpi, cancellationToken: cancellationToken);
            using var stream = new MemoryStream();
            await image.SaveAsync(stream, PngFormat.Instance, cancellationToken);
            return (Width: image.Width, Height: image.Height, Bytes: stream.ToArray());
        }, cancellationToken);

        var rendered = new RenderedPage(document.FilePath, pageNumber, pngBytes.Width, pngBytes.Height, pngBytes.Bytes);
        AddToCache(key, rendered);
        return rendered;
    }

    /// <summary>Forgets every cached page.</summary>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
            _cacheOrder.Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        ClearCache();
        _rasterizer.Dispose();
    }

    private static string CacheKey(PdfPageDocument document, int pageNumber, int dpi) =>
        $"{document.FilePath}|{pageNumber}|{dpi}";

    private bool TryGetCached(string key, out RenderedPage rendered)
    {
        lock (_cacheLock)
        {
            if (!_cache.TryGetValue(key, out rendered)) { return false; }
            _cacheOrder.Remove(key);
            _cacheOrder.AddFirst(key);
            return true;
        }
    }

    private void AddToCache(string key, RenderedPage rendered)
    {
        if (CacheCapacity < 1) { return; }
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(key)) { _cacheOrder.Remove(key); }
            _cache[key] = rendered;
            _cacheOrder.AddFirst(key);
            while (_cache.Count > CacheCapacity)
            {
                var oldest = _cacheOrder.Last.Value;
                _cacheOrder.RemoveLast();
                _cache.Remove(oldest);
            }
        }
    }
}
