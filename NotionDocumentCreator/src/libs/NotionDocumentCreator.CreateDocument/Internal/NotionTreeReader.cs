using CodeBrix.NotionApi;
using NotionDocumentCreator.CreateDocument.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Discovers the selectable page tree below an entered page or database ID:
/// resolves the root, and loads one level of child pages at a time (the treeview
/// loads lazily on expand). All API calls go through the shared rate gate.
/// </summary>
internal sealed class NotionTreeReader
{
    //A database's children come from querying its data sources rather than from
    //  block children, so the reader tracks which retrieval shape each known ID needs.
    private enum SourceShape { Page, Database, DataSource }

    private sealed record NodeMeta(SourceShape Shape, int Depth, string Title, string ParentId);

    private readonly INotionClient _client;
    private readonly NotionRateGate _gate;
    private readonly ConcurrentDictionary<string, NodeMeta> _metaById =
        new(StringComparer.OrdinalIgnoreCase);

    public NotionTreeReader(INotionClient client, NotionRateGate gate)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gate = gate ?? throw new ArgumentNullException(nameof(gate));
    }

    /// <summary>
    /// Resolves the entered ID as a page, then a database, then a data source, and
    /// returns its root node. Throws with a user-actionable message when the ID is
    /// not visible to the connected integration.
    /// </summary>
    public async Task<IList<NotionPageNode>> LoadRootsAsync(
        string pageOrDatabaseId, CancellationToken cancellationToken = default)
    {
        var id = NotionConvert.NormalizeId(pageOrDatabaseId);

        try
        {
            var page = await _gate.RunAsync(() => _client.Pages.RetrieveAsync(id, cancellationToken), cancellationToken);
            return [BuildPageNode(page, depth: 0, parentId: null)];
        }
        catch (NotionApiException ex) when (IsWrongKind(ex)) { }

        try
        {
            var database = await _gate.RunAsync(() => _client.Databases.RetrieveAsync(id, cancellationToken), cancellationToken);
            return [BuildDatabaseNode(database, depth: 0, parentId: null)];
        }
        catch (NotionApiException ex) when (IsWrongKind(ex)) { }

        try
        {
            var dataSource = await _gate.RunAsync(() => _client.DataSources.RetrieveAsync(
                new RetrieveDataSourceRequest { DataSourceId = id }, cancellationToken), cancellationToken);
            return [BuildDataSourceNode(dataSource, depth: 0, parentId: null)];
        }
        catch (NotionApiException ex) when (IsWrongKind(ex))
        {
            throw new InvalidOperationException(
                "No page or database with that ID is visible to this integration. " +
                "Check the ID, and make sure the page is shared with the integration in Notion.", ex);
        }
    }

    /// <summary>
    /// Loads the immediate child pages (and child databases) of a previously
    /// returned node, in tree order.
    /// </summary>
    public async Task<IList<NotionPageNode>> LoadChildrenAsync(
        string id, CancellationToken cancellationToken = default)
    {
        var normalized = NotionConvert.NormalizeId(id);
        var meta = _metaById.TryGetValue(normalized, out var known)
            ? known
            : new NodeMeta(SourceShape.Page, 0, "", null);
        var childDepth = meta.Depth + 1;

        return meta.Shape switch
        {
            SourceShape.Database => await LoadDatabaseChildrenAsync(normalized, childDepth, cancellationToken),
            SourceShape.DataSource => await LoadDataSourceChildrenAsync(normalized, childDepth, cancellationToken),
            _ => await LoadPageChildrenAsync(normalized, childDepth, cancellationToken)
        };
    }

    /// <summary>
    /// The titles along the tree path from the root down to (and including) the
    /// given node, from what the reader has seen so far. Empty when the node has
    /// not passed through this reader.
    /// </summary>
    public IReadOnlyList<string> AncestorTitlesOf(string id)
    {
        var titles = new List<string>();
        var current = NotionConvert.NormalizeId(id);
        var guard = 0;
        while (current is not null && _metaById.TryGetValue(current, out var meta) && guard++ < 32)
        {
            titles.Insert(0, meta.Title);
            current = meta.ParentId;
        }
        return titles;
    }

    //Notion answers a retrieve of the wrong kind (or of an unshared/absent object)
    //  with object_not_found, and a malformed ID with validation_error
    private static bool IsWrongKind(NotionApiException ex) =>
        ex.NotionAPIErrorCode == NotionAPIErrorCode.ObjectNotFound
        || ex.NotionAPIErrorCode == NotionAPIErrorCode.ValidationError;

    private async Task<IList<NotionPageNode>> LoadPageChildrenAsync(
        string pageId, int depth, CancellationToken cancellationToken)
    {
        var blocks = await _gate.RunAsync(
            () => _client.RetrieveAllChildrenAsync(pageId, cancellationToken), cancellationToken);

        var children = new List<NotionPageNode>();
        foreach (var block in blocks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (block)
            {
                case ChildPageBlock childPage:
                    children.Add(await LoadChildPageNodeAsync(childPage, depth, pageId, cancellationToken));
                    break;
                case ChildDatabaseBlock childDatabase:
                    children.Add(await LoadChildDatabaseNodeAsync(childDatabase, depth, pageId, cancellationToken));
                    break;
            }
        }
        return children;
    }

    private async Task<NotionPageNode> LoadChildPageNodeAsync(
        ChildPageBlock childPage, int depth, string parentId, CancellationToken cancellationToken)
    {
        //A child_page block's ID is the page's ID; retrieving the page adds its
        //  icon, cover and title formatting to the tree row
        try
        {
            var page = await _gate.RunAsync(
                () => _client.Pages.RetrieveAsync(childPage.Id, cancellationToken), cancellationToken);
            return BuildPageNode(page, depth, parentId, childPage.HasChildren);
        }
        catch (NotionApiException)
        {
            //Icon and cover are decoration — fall back to what the block itself carries
            var node = new NotionPageNode
            {
                Id = childPage.Id,
                Title = string.IsNullOrWhiteSpace(childPage.ChildPage?.Title) ? "Untitled" : childPage.ChildPage.Title,
                Kind = NotionSourceKind.Page,
                HasChildren = childPage.HasChildren,
                LastEditedTime = NotionConvert.AsUtc(childPage.LastEditedTime),
                Depth = depth
            };
            _metaById[node.Id] = new NodeMeta(SourceShape.Page, depth, node.Title, parentId);
            return node;
        }
    }

    private async Task<NotionPageNode> LoadChildDatabaseNodeAsync(
        ChildDatabaseBlock childDatabase, int depth, string parentId, CancellationToken cancellationToken)
    {
        try
        {
            var database = await _gate.RunAsync(
                () => _client.Databases.RetrieveAsync(childDatabase.Id, cancellationToken), cancellationToken);
            return BuildDatabaseNode(database, depth, parentId);
        }
        catch (NotionApiException)
        {
            var node = new NotionPageNode
            {
                Id = childDatabase.Id,
                Title = string.IsNullOrWhiteSpace(childDatabase.ChildDatabase?.Title)
                    ? "Untitled database" : childDatabase.ChildDatabase.Title,
                Kind = NotionSourceKind.Database,
                HasChildren = true,
                LastEditedTime = NotionConvert.AsUtc(childDatabase.LastEditedTime),
                Depth = depth
            };
            _metaById[node.Id] = new NodeMeta(SourceShape.Database, depth, node.Title, parentId);
            return node;
        }
    }

    private async Task<IList<NotionPageNode>> LoadDatabaseChildrenAsync(
        string databaseId, int depth, CancellationToken cancellationToken)
    {
        var database = await _gate.RunAsync(
            () => _client.Databases.RetrieveAsync(databaseId, cancellationToken), cancellationToken);

        var children = new List<NotionPageNode>();
        foreach (var dataSourceRef in database.DataSources ?? [])
        {
            await QueryDataSourcePagesAsync(dataSourceRef.DataSourceId, depth, databaseId, children, cancellationToken);
        }
        return children;
    }

    private async Task<IList<NotionPageNode>> LoadDataSourceChildrenAsync(
        string dataSourceId, int depth, CancellationToken cancellationToken)
    {
        var children = new List<NotionPageNode>();
        await QueryDataSourcePagesAsync(dataSourceId, depth, dataSourceId, children, cancellationToken);
        return children;
    }

    private async Task QueryDataSourcePagesAsync(
        string dataSourceId, int depth, string parentId, List<NotionPageNode> results, CancellationToken cancellationToken)
    {
        string cursor = null;
        do
        {
            var request = new QueryDataSourceRequest
            {
                DataSourceId = dataSourceId,
                PageSize = 100,
                StartCursor = cursor
            };
            var response = await _gate.RunAsync(
                () => _client.DataSources.QueryAsync(request, cancellationToken), cancellationToken);

            foreach (var page in (response.Results ?? []).OfType<Page>())
            {
                results.Add(BuildPageNode(page, depth, parentId));
            }
            cursor = response.HasMore ? response.NextCursor : null;
        } while (cursor != null);
    }

    private NotionPageNode BuildPageNode(Page page, int depth, string parentId, bool hasChildrenHint = true)
    {
        var (iconEmoji, iconUrl) = NotionConvert.IconOf(page.Icon);
        var node = new NotionPageNode
        {
            Id = page.Id,
            Title = NotionConvert.TitleOf(page),
            Kind = NotionSourceKind.Page,
            IconEmoji = iconEmoji,
            IconUrl = iconUrl,
            CoverUrl = NotionConvert.CoverUrlOf(page.Cover),
            HasChildren = hasChildrenHint,
            LastEditedTime = NotionConvert.AsUtc(page.LastEditedTime),
            Depth = depth
        };
        _metaById[node.Id] = new NodeMeta(SourceShape.Page, depth, node.Title, parentId);
        return node;
    }

    private NotionPageNode BuildDatabaseNode(Database database, int depth, string parentId)
    {
        var (iconEmoji, iconUrl) = NotionConvert.IconOf(database.Icon);
        var node = new NotionPageNode
        {
            Id = database.Id,
            Title = NotionConvert.TitleOf(database),
            Kind = NotionSourceKind.Database,
            IconEmoji = iconEmoji,
            IconUrl = iconUrl,
            CoverUrl = NotionConvert.CoverUrlOf(database.Cover),
            HasChildren = true,
            LastEditedTime = NotionConvert.AsUtc(database.LastEditedTime),
            Depth = depth
        };
        _metaById[node.Id] = new NodeMeta(SourceShape.Database, depth, node.Title, parentId);
        return node;
    }

    private NotionPageNode BuildDataSourceNode(DataSource dataSource, int depth, string parentId)
    {
        var (iconEmoji, iconUrl) = NotionConvert.IconOf(dataSource.Icon);
        var title = NotionConvert.PlainText(dataSource.Title).Trim();
        var node = new NotionPageNode
        {
            Id = dataSource.Id,
            Title = title.Length == 0 ? "Untitled database" : title,
            Kind = NotionSourceKind.Database,
            IconEmoji = iconEmoji,
            IconUrl = iconUrl,
            CoverUrl = NotionConvert.CoverUrlOf(dataSource.Cover),
            HasChildren = true,
            LastEditedTime = NotionConvert.AsUtc(dataSource.LastEditedTime),
            Depth = depth
        };
        _metaById[node.Id] = new NodeMeta(SourceShape.DataSource, depth, node.Title, parentId);
        return node;
    }
}
