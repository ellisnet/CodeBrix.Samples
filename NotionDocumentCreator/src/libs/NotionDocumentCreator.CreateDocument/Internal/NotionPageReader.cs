using CodeBrix.NotionApi;
using NotionDocumentCreator.CreateDocument.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// One block of a page's content together with its recursively fetched children
/// (the Notion API returns nested blocks one level per request).
/// </summary>
internal sealed class NotionBlockNode
{
    /// <summary>The block as returned by the Notion API.</summary>
    public IBlock Block { get; init; }

    /// <summary>The block's fetched child blocks, in order; empty for leaf blocks.</summary>
    public IReadOnlyList<NotionBlockNode> Children { get; init; } = [];
}

/// <summary>
/// Reads page content from the Notion API: a short preview for the preview pane,
/// and the full recursive block tree used to render a chapter. All API calls go
/// through the shared rate gate.
/// </summary>
internal sealed class NotionPageReader
{
    private const int PreviewSnippetCount = 2;
    private const int PreviewSnippetMaxLength = 220;

    private readonly INotionClient _client;
    private readonly NotionRateGate _gate;

    public NotionPageReader(INotionClient client, NotionRateGate gate)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gate = gate ?? throw new ArgumentNullException(nameof(gate));
    }

    /// <summary>
    /// Builds a short, non-scrolling preview of a page: identity (title, icon,
    /// cover, edit time), the child page count, and the first block or two of text.
    /// Uses at most two API calls.
    /// </summary>
    public async Task<NotionPagePreview> LoadPreviewAsync(
        string pageId, CancellationToken cancellationToken = default)
    {
        var id = NotionConvert.NormalizeId(pageId);

        var page = await _gate.RunAsync(
            () => _client.Pages.RetrieveAsync(id, cancellationToken), cancellationToken);

        //One batch (up to 100 blocks) is plenty for an identification aid; for a
        //  longer page the child count simply reads as "at least"
        var firstBatch = await _gate.RunAsync(
            () => _client.Blocks.RetrieveChildrenAsync(
                new BlockRetrieveChildrenRequest { BlockId = id, PageSize = 100 }, cancellationToken),
            cancellationToken);

        var childPageCount = 0;
        var snippets = new List<string>();
        foreach (var block in firstBatch.Results ?? [])
        {
            if (block is ChildPageBlock or ChildDatabaseBlock)
            {
                childPageCount++;
                continue;
            }
            if (snippets.Count >= PreviewSnippetCount) { continue; }

            var text = NotionConvert.PlainTextOf(block).Trim();
            if (text.Length == 0) { continue; }
            if (text.Length > PreviewSnippetMaxLength)
            {
                text = text[..PreviewSnippetMaxLength].TrimEnd() + "…";
            }
            snippets.Add(text);
        }

        var (iconEmoji, iconUrl) = NotionConvert.IconOf(page.Icon);
        return new NotionPagePreview
        {
            Id = page.Id,
            Title = NotionConvert.TitleOf(page),
            IconEmoji = iconEmoji,
            IconUrl = iconUrl,
            CoverUrl = NotionConvert.CoverUrlOf(page.Cover),
            LastEditedTime = NotionConvert.AsUtc(page.LastEditedTime),
            ChildPageCount = childPageCount,
            TextSnippets = snippets
        };
    }

    /// <summary>
    /// Reads the complete block tree below a page (or block): every block with
    /// children is recursed into, except child pages and child databases — those
    /// are separate chapters (or reference lines), never inlined content.
    /// </summary>
    public async Task<IReadOnlyList<NotionBlockNode>> ReadBlockTreeAsync(
        string blockId, CancellationToken cancellationToken = default)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return await ReadChildrenAsync(NotionConvert.NormalizeId(blockId), visited, cancellationToken);
    }

    private async Task<IReadOnlyList<NotionBlockNode>> ReadChildrenAsync(
        string blockId, HashSet<string> visited, CancellationToken cancellationToken)
    {
        if (!visited.Add(blockId)) { return []; } //Cycle guard (synced blocks can loop)

        var blocks = await _gate.RunAsync(
            () => _client.RetrieveAllChildrenAsync(blockId, cancellationToken), cancellationToken);

        var nodes = new List<NotionBlockNode>(blocks.Count);
        foreach (var block in blocks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<NotionBlockNode> children = [];
            if (ShouldRecurse(block))
            {
                children = await ReadChildrenAsync(block.Id, visited, cancellationToken);
            }
            else if (block is SyncedBlockBlock synced
                && !string.IsNullOrEmpty(synced.SyncedBlock?.SyncedFrom?.BlockId))
            {
                //A duplicate synced block mirrors its source — fetch the source's
                //  children (the visited set guards against reference cycles)
                children = await ReadChildrenAsync(
                    synced.SyncedBlock.SyncedFrom.BlockId, visited, cancellationToken);
            }
            nodes.Add(new NotionBlockNode { Block = block, Children = children });
        }
        return nodes;
    }

    private static bool ShouldRecurse(IBlock block) =>
        block.HasChildren && block is not ChildPageBlock && block is not ChildDatabaseBlock;
}
