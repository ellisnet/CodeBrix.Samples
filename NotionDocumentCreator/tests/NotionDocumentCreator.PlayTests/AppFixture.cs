using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using NotionDocumentCreator.CreateDocument.Models;
using NotionDocumentCreator.CreateDocument.Services;
using NotionDocumentCreator.ViewModels;
using NotionDocumentCreator.Views;

namespace NotionDocumentCreator.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public NotionFixture Notion { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Microsoft.UI.Xaml.Application CreateApplication() => new App(
        services => services.AddSingleton<INotionDocumentService>(Notion));

    protected override Task BeforeResetAsync()
    {
        Notion.Reset();
        return Task.CompletedTask;
    }
}

// A fixed workspace: "Field Guide" holds "Rivers" (which holds "Springs") and "Forests".
// No token, network or image URL is involved; the dates are noon UTC so the preview's
// local date is the same in every time zone the runners use.
public sealed class NotionFixture : INotionDocumentService
{
    private static readonly DateTimeOffset Edited = new(2026, 3, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly Dictionary<string, NotionPageNode[]> Tree = new()
    {
        ["field-guide"] = new[] { Node("rivers", "Rivers", true), Node("forests", "Forests", false) },
        ["rivers"] = new[] { Node("springs", "Springs", false) },
    };
    private readonly Dictionary<string, TaskCompletionSource> _childGates = new();
    private readonly Dictionary<string, TaskCompletionSource> _previewGates = new();

    public bool FailConnect { get; set; }
    public bool FailChildren { get; set; }
    public bool FailPreview { get; set; }
    public bool FailCreate { get; set; }
    public TaskCompletionSource ConnectGate { get; set; }
    public TaskCompletionSource CreateGate { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = [];
    public string LastToken { get; set; }
    public CreateRequest LastCreateRequest { get; set; }

    public TaskCompletionSource HoldChildren(string pageId) => _childGates[pageId] = new();
    public TaskCompletionSource HoldPreview(string pageId) => _previewGates[pageId] = new();

    public void Reset()
    {
        // Releasing every gate lets work started by an earlier page finish quietly.
        foreach (var gate in _childGates.Values.Concat(_previewGates.Values)) gate.TrySetResult();
        ConnectGate?.TrySetResult();
        CreateGate?.TrySetResult();
        _childGates.Clear();
        _previewGates.Clear();
        ConnectGate = CreateGate = null;
        FailConnect = FailChildren = FailPreview = FailCreate = false;
        Warnings = [];
        LastToken = null;
        LastCreateRequest = null;
    }

    public async Task<string> ConnectAsync(string integrationToken, CancellationToken cancellationToken = default)
    {
        LastToken = integrationToken;
        await Task.Yield();
        if (ConnectGate != null) await ConnectGate.Task;
        if (FailConnect) throw new InvalidOperationException("Fixture token rejected");
        return "fixture-bot";
    }

    public async Task<IList<NotionPageNode>> LoadRootsAsync(string pageOrDatabaseId, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return new[] { Node("field-guide", "Field Guide", true) };
    }

    public async Task<IList<NotionPageNode>> LoadChildrenAsync(string pageId, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        if (_childGates.TryGetValue(pageId, out var gate)) await gate.Task;
        if (FailChildren) throw new InvalidOperationException("Fixture children unavailable");
        return Tree.TryGetValue(pageId, out var children) ? children : Array.Empty<NotionPageNode>();
    }

    public async Task<NotionPagePreview> LoadPreviewAsync(string pageId, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        if (_previewGates.TryGetValue(pageId, out var gate)) await gate.Task;
        if (FailPreview) throw new InvalidOperationException("Fixture preview unavailable");
        var title = pageId == "field-guide" ? "Field Guide" : Tree.Values.SelectMany(n => n).Single(n => n.Id == pageId).Title;
        return new NotionPagePreview
        {
            Id = pageId, Title = title, LastEditedTime = Edited,
            ChildPageCount = Tree.TryGetValue(pageId, out var children) ? children.Length : 0,
            TextSnippets = new[] { "The opening paragraph of " + title + ".", "A second short block." },
        };
    }

    public async Task<CreatedDocument> CreateDocumentAsync(CreateRequest request,
        IProgress<CreateProgress> progress = null, CancellationToken cancellationToken = default)
    {
        LastCreateRequest = request;
        progress?.Report(new(CreateStage.ComposingBook, "Composing the fixture book…", 40));
        await Task.Yield();
        if (CreateGate != null) await CreateGate.Task;
        if (FailCreate) throw new InvalidOperationException("Fixture renderer unavailable");
        // Deliberately a service-boundary fixture, not a test of the PDF rendering library.
        await File.WriteAllTextAsync(request.OutputFilePath, "%PDF-1.4\n% PlayTest service fixture\n%%EOF\n", cancellationToken);
        return new CreatedDocument
        {
            OutputFilePath = request.OutputFilePath, Title = "Field Guide", PageCount = 12,
            ChapterCount = request.PageIds.Count, ImageCount = 0, Elapsed = TimeSpan.FromSeconds(3),
            Warnings = Warnings,
        };
    }

    private static NotionPageNode Node(string id, string title, bool hasChildren) => new()
    {
        Id = id, Title = title, HasChildren = hasChildren, LastEditedTime = Edited,
    };
}
