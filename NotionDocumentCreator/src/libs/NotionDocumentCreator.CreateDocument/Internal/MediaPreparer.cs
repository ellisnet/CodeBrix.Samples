using CodeBrix.NotionApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// The pre-render download pass: walks the fetched block trees, downloads every
/// image/video/audio file through the <see cref="MediaCache"/>, processes images
/// for print, extracts video poster frames, and probes durations — filling
/// <see cref="RenderContext.MediaByBlockId"/> so the renderer works synchronously.
/// </summary>
internal sealed class MediaPreparer
{
    private readonly MediaCache _cache;
    private readonly RenderContext _context;

    public MediaPreparer(MediaCache cache, RenderContext context)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Downloads and prepares a page's cover image, stored under the
    /// "cover:&lt;pageId&gt;" key.
    /// </summary>
    public async Task PrepareCoverAsync(string pageId, string coverUrl,
        CancellationToken cancellationToken = default)
    {
        if (!_context.IncludeImages || string.IsNullOrWhiteSpace(coverUrl)) { return; }

        var cached = await _cache.FetchAsync(coverUrl, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!cached.Success)
        {
            _context.Warnings.Add($"The cover image could not be downloaded: {cached.FailureReason}");
            return;
        }

        try
        {
            var processed = ImagePipeline.ProcessForPrint(File.ReadAllBytes(cached.FilePath));
            _context.MediaByBlockId["cover:" + pageId] =
                new PreparedMedia { Image = processed, SourceLength = cached.Length };
        }
        catch (Exception ex)
        {
            _context.Warnings.Add($"The cover image could not be decoded: {ex.Message}");
        }
    }

    /// <summary>Downloads and prepares every media block in the given chapters.</summary>
    public async Task PrepareChaptersAsync(IReadOnlyList<ChapterContent> chapters,
        Action<string> reportProgress = null, CancellationToken cancellationToken = default)
    {
        var mediaBlocks = new List<IBlock>();
        foreach (var chapter in chapters)
        {
            Collect(chapter.Blocks, mediaBlocks);
        }

        for (var i = 0; i < mediaBlocks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reportProgress?.Invoke($"Media file {i + 1} of {mediaBlocks.Count}…");
            await PrepareOneAsync(mediaBlocks[i], cancellationToken).ConfigureAwait(false);
        }
    }

    private void Collect(IReadOnlyList<NotionBlockNode> nodes, List<IBlock> mediaBlocks)
    {
        foreach (var node in nodes)
        {
            switch (node.Block)
            {
                case ImageBlock when _context.IncludeImages:
                case VideoBlock when _context.IncludeMedia:
                case AudioBlock when _context.IncludeMedia:
                    mediaBlocks.Add(node.Block);
                    break;
            }
            Collect(node.Children, mediaBlocks);
        }
    }

    private async Task PrepareOneAsync(IBlock block, CancellationToken cancellationToken)
    {
        switch (block)
        {
            case ImageBlock image:
                await PrepareImageAsync(image.Id, image.Image, cancellationToken).ConfigureAwait(false);
                break;
            case VideoBlock video:
                await PrepareVideoAsync(video.Id, video.Video, cancellationToken).ConfigureAwait(false);
                break;
            case AudioBlock audio:
                await PrepareAudioAsync(audio.Id, audio.Audio, cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    private async Task PrepareImageAsync(string blockId, FileObject file, CancellationToken cancellationToken)
    {
        var cached = await FetchAsync(blockId, file, cancellationToken).ConfigureAwait(false);
        if (cached is null) { return; }

        try
        {
            var processed = ImagePipeline.ProcessForPrint(File.ReadAllBytes(cached.FilePath));
            _context.MediaByBlockId[blockId] =
                new PreparedMedia { Image = processed, SourceLength = cached.Length };
        }
        catch (Exception ex)
        {
            _context.MediaByBlockId[blockId] =
                new PreparedMedia { FailureReason = $"the image could not be decoded ({ex.Message})" };
        }
    }

    private async Task PrepareVideoAsync(string blockId, FileObject file, CancellationToken cancellationToken)
    {
        var cached = await FetchAsync(blockId, file, cancellationToken).ConfigureAwait(false);
        if (cached is null) { return; }

        var posterBytes = VideoPosterExtractor.TryExtractPoster(
            cached.FilePath, _cache.CacheDirectory, out var duration);
        if (posterBytes is null)
        {
            _context.MediaByBlockId[blockId] = new PreparedMedia
            {
                Duration = duration,
                SourceLength = cached.Length,
                FailureReason = "a poster frame could not be extracted"
            };
            return;
        }

        try
        {
            var processed = ImagePipeline.ProcessForPrint(posterBytes);
            _context.MediaByBlockId[blockId] = new PreparedMedia
            {
                Image = processed,
                Duration = duration,
                SourceLength = cached.Length
            };
        }
        catch (Exception ex)
        {
            _context.MediaByBlockId[blockId] = new PreparedMedia
            {
                Duration = duration,
                SourceLength = cached.Length,
                FailureReason = $"the poster frame could not be decoded ({ex.Message})"
            };
        }
    }

    private async Task PrepareAudioAsync(string blockId, FileObject file, CancellationToken cancellationToken)
    {
        var cached = await FetchAsync(blockId, file, cancellationToken).ConfigureAwait(false);
        if (cached is null) { return; }

        _context.MediaByBlockId[blockId] = new PreparedMedia
        {
            Duration = VideoPosterExtractor.TryProbeDuration(cached.FilePath),
            SourceLength = cached.Length
        };
    }

    private async Task<CachedMedia> FetchAsync(string blockId, FileObject file,
        CancellationToken cancellationToken)
    {
        var url = UrlOf(file);
        if (string.IsNullOrWhiteSpace(url))
        {
            _context.MediaByBlockId[blockId] =
                new PreparedMedia { FailureReason = "the block carries no downloadable URL" };
            return null;
        }

        var cached = await _cache.FetchAsync(url, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!cached.Success)
        {
            _context.MediaByBlockId[blockId] = new PreparedMedia { FailureReason = cached.FailureReason };
            return null;
        }
        return cached;
    }

    private static string UrlOf(FileObject file) => file switch
    {
        UploadedFile uploaded => uploaded.File?.Url,
        ExternalFile external => external.External?.Url,
        _ => null
    };
}
