using CodeBrix.NotionApi;
using CodeBrix.PdfDocCreate.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NotionDocumentCreator.CreateDocument.Internal;
using NotionDocumentCreator.CreateDocument.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Services;

/// <summary>
/// Default implementation of <see cref="INotionDocumentService"/>: connect →
/// browse the page tree → read the selected pages → compose and save the book PDF.
/// </summary>
public sealed class NotionDocumentService : INotionDocumentService, IDisposable
{
    private readonly ILogger<NotionDocumentService> _logger;
    private readonly NotionRateGate _gate = new();

    private INotionClient _client;
    private NotionTreeReader _treeReader;
    private NotionPageReader _pageReader;
    private bool _isDisposed;

    public NotionDocumentService(ILogger<NotionDocumentService> logger = null)
    {
        _logger = logger ?? NullLogger<NotionDocumentService>.Instance;
    }

    /// <inheritdoc />
    public async Task<string> ConnectAsync(
        string integrationToken, CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();
        if (string.IsNullOrWhiteSpace(integrationToken))
        {
            throw new ArgumentException("An integration token is required.", nameof(integrationToken));
        }

        //Reconnecting replaces any previous client (the user may paste a new token)
        _client?.Dispose();
        _client = null;
        _treeReader = null;
        _pageReader = null;

        var client = NotionClientFactory.Instance.Create(new ClientOptions
        {
            AuthToken = integrationToken.Trim(),
            //Retries transient 429/5xx responses; the NotionRateGate does the proactive pacing
            RetryPolicy = new DefaultRetryPolicy()
        });

        try
        {
            var user = await _gate.RunAsync(
                () => client.Users.MeAsync(cancellationToken), cancellationToken);

            _client = client;
            _treeReader = new NotionTreeReader(client, _gate);
            _pageReader = new NotionPageReader(client, _gate);

            var botName = string.IsNullOrWhiteSpace(user.Name) ? "Notion integration" : user.Name;
            _logger.LogInformation("Connected to Notion as bot \"{Bot}\".", botName);
            return botName;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public Task<IList<NotionPageNode>> LoadRootsAsync(
        string pageOrDatabaseId, CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();
        return CheckConnected().LoadRootsAsync(pageOrDatabaseId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IList<NotionPageNode>> LoadChildrenAsync(
        string pageId, CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();
        return CheckConnected().LoadChildrenAsync(pageId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<NotionPagePreview> LoadPreviewAsync(
        string pageId, CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();
        CheckConnected();
        return _pageReader.LoadPreviewAsync(pageId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CreatedDocument> CreateDocumentAsync(CreateRequest request,
        IProgress<CreateProgress> progress = null, CancellationToken cancellationToken = default)
    {
        CheckIsDisposed();
        CheckConnected();
        ArgumentNullException.ThrowIfNull(request);
        if (request.PageIds is null || request.PageIds.Count == 0)
        {
            throw new ArgumentException("At least one page must be selected.", nameof(request));
        }
        if (string.IsNullOrWhiteSpace(request.OutputFilePath))
        {
            throw new ArgumentException("The request must specify an output file path.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        var context = new RenderContext
        {
            Theme = BookTheme.For(request.PageSize),
            IncludeImages = request.IncludeImages,
            IncludeMedia = request.IncludeMedia
        };

        // 1. Fetch every selected page's identity and full block tree
        var chapters = new List<ChapterContent>();
        for (var i = 0; i < request.PageIds.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new CreateProgress(CreateStage.FetchingPages,
                $"Reading page {i + 1} of {request.PageIds.Count}…",
                4 + 40 * i / request.PageIds.Count));

            var id = NotionConvert.NormalizeId(request.PageIds[i]);
            var page = await _gate.RunAsync(
                () => _client.Pages.RetrieveAsync(id, cancellationToken), cancellationToken);
            var blocks = await _pageReader.ReadBlockTreeAsync(id, cancellationToken);

            var title = NotionConvert.TitleOf(page);
            var (iconEmoji, _) = NotionConvert.IconOf(page.Icon);
            var ancestors = _treeReader.AncestorTitlesOf(id);
            if (ancestors.Count == 0) { ancestors = [title]; }

            chapters.Add(new ChapterContent
            {
                PageId = id,
                Title = title,
                IconEmoji = iconEmoji,
                CoverUrl = NotionConvert.CoverUrlOf(page.Cover),
                AncestorTitles = ancestors,
                Blocks = blocks
            });
            context.PagesInBook[id] = new BookPageRef { Title = title, BookmarkName = $"page.{id}" };
        }

        // 2. Download and prepare media (inside the same run — uploaded-file URLs expire)
        progress?.Report(new CreateProgress(CreateStage.DownloadingMedia, "Downloading media…", 46));
        using var mediaCache = new MediaCache();
        var preparer = new MediaPreparer(mediaCache, context);
        await preparer.PrepareCoverAsync(chapters[0].PageId, chapters[0].CoverUrl, cancellationToken);
        var mediaDone = 0;
        await preparer.PrepareChaptersAsync(chapters,
            message => progress?.Report(new CreateProgress(CreateStage.DownloadingMedia, message,
                Math.Min(70, 46 + (++mediaDone * 2)))),
            cancellationToken);

        // 3 + 4. Compose and render off the caller's thread (both are CPU-bound,
        //    and the caller is typically the UI thread)
        progress?.Report(new CreateProgress(CreateStage.ComposingBook, "Laying out the book…", 72));
        var composer = new BookComposer(chapters, context);
        var outputPath = request.OutputFilePath.Trim();

        var pageCount = await Task.Run(() =>
        {
            var document = composer.Compose();

            progress?.Report(new CreateProgress(CreateStage.SavingPdf, "Rendering the PDF…", 82));
            var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
            renderer.RenderDocument();

            var folder = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(folder)) { Directory.CreateDirectory(folder); }
            renderer.PdfDocument.Save(outputPath);
            return renderer.PdfDocument.PageCount;
        }, cancellationToken);

        stopwatch.Stop();

        if (composer.DroppedCharacterCount > 0)
        {
            context.Warnings.Add(
                $"{composer.DroppedCharacterCount} character(s) outside the embedded fonts' coverage were omitted.");
        }
        foreach (var note in context.Notes)
        {
            _logger.LogInformation("Note: {Note}", note);
        }
        _logger.LogInformation(
            "Saved {Path}: {Pages} pages, {Chapters} chapters, {Images} images, {Elapsed:F1}s",
            outputPath, pageCount, chapters.Count, composer.PlacedImageCount,
            stopwatch.Elapsed.TotalSeconds);

        progress?.Report(new CreateProgress(CreateStage.Done, "Done.", 100));

        return new CreatedDocument
        {
            OutputFilePath = outputPath,
            Title = chapters[0].Title,
            PageCount = pageCount,
            ChapterCount = chapters.Count,
            ImageCount = composer.PlacedImageCount,
            Elapsed = stopwatch.Elapsed,
            Warnings = context.Warnings.ToList()
        };
    }

    private NotionTreeReader CheckConnected()
    {
        if (_client is null || _treeReader is null || _pageReader is null)
        {
            throw new InvalidOperationException(
                "Not connected to Notion — call ConnectAsync with an integration token first.");
        }
        return _treeReader;
    }

    private void CheckIsDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    public void Dispose()
    {
        if (_isDisposed) { return; }
        _isDisposed = true;
        _client?.Dispose();
        _gate.Dispose();
    }
}
