using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoddessTempleDiscovery.Rules.Journal;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    // A game with a find in the hand, and its Field Journal open through the J key.
    private async Task OpenJournalAfterAFindAsync()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        await DigAsync(await PlainDigAsync());
        await WaitAsync(() => Model.IsInspectorOpen, open => open, "the newspaper on the find");
        await CloseInspectorAsync();
        await HoldKeyAsync("J", () => Model.IsJournalOpen);
        await Expect(Id("JournalPane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task The_journal_lists_the_entries_read_so_far_and_filters_them()
    {
        await OpenJournalAfterAFindAsync();
        var entries = await OnEngineAsync(() => State.Journal.ToArray());
        entries.Should().Contain(e => e.Kind == JournalEntryKind.Discovery);
        await WaitAsync(() => Model.JournalItems.Count, count => count == entries.Length, "every entry listed");
        await Expect(Id("JournalCount")).ToHaveTextAsync($"{entries.Length} entries read this game");
        await Expect(Page.GetByText(entries.First().Title).First).ToBeVisibleAsync();

        await Id("JournalFilter").SelectOptionAsync(nameof(JournalEntryKind.Discovery));
        var discoveries = entries.Count(e => e.Kind == JournalEntryKind.Discovery);
        await WaitAsync(() => Model.JournalItems.Count, count => count == discoveries, "the discoveries only");
        (await ReadAsync(() => Model.JournalItems.All(i => i.Kind == JournalEntryKind.Discovery))).Should().BeTrue();
        await Id("JournalFilter").SelectOptionAsync(nameof(JournalEntryKind.Season));
        await WaitAsync(() => Model.JournalItems.All(i => i.Kind == JournalEntryKind.Season), only => only, "the seasons only");
        await Id("JournalFilter").SelectOptionAsync("Everything");
        await WaitAsync(() => Model.JournalItems.Count, count => count == entries.Length, "every entry again");
        await Id("CloseJournal").ClickAsync();
        await Expect(Id("JournalPane")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Export_pdf_writes_the_field_journal_through_the_registered_file_bridge()
    {
        await OpenJournalAfterAFindAsync();
        var path = Path.Combine(_fixture.ExportDirectory, $"field-journal-{Guid.NewGuid():N}.pdf");
        _fixture.Files.NextPath = path;
        await Id("ExportJournal").ClickAsync();
        await WaitAsync(() => Model.JournalStatus, status => status.StartsWith($"Saved {Path.GetFileName(path)} ("), "the journal saved", 60000);
        await Expect(Id("JournalStatus")).ToContainTextAsync($"Saved {Path.GetFileName(path)} (");
        var request = _fixture.Files.Requests.Single().Split('|');
        request[0].Should().EndWith(".pdf");
        (request[1], request[2]).Should().Be(("PDF document", ".pdf"));
        // The page's own save picker was never asked: the bridge registered through App(...) answered.
        _fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(0);
        var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
        bytes.Length.Should().BeGreaterThan(1000);
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");

        // OPEN FOLDER goes to the platform launcher with the export's folder.
        await Expect(Id("OpenJournalFolder")).ToBeEnabledAsync();
        await Id("OpenJournalFolder").ClickAsync();
        await WaitAsync(() => _fixture.Application.Launcher.LaunchedUris.Count, n => n == 1, "the folder handed to the launcher");
        _fixture.Application.Launcher.LaunchedUris[0].LocalPath.TrimEnd('/').Should().Be(Path.GetFullPath(_fixture.ExportDirectory).TrimEnd('/'));
    }

    [Fact]
    public async Task Export_pdf_cancelled_in_the_file_bridge_writes_nothing()
    {
        await OpenJournalAfterAFindAsync();
        _fixture.Files.NextPath = null;
        var files = Directory.GetFiles(_fixture.ExportDirectory).Length;
        await Id("ExportJournal").ClickAsync();
        await Expect(Id("JournalStatus")).ToHaveTextAsync("Export cancelled.");
        _fixture.Files.Requests.Should().HaveCount(1);
        Directory.GetFiles(_fixture.ExportDirectory).Length.Should().Be(files);
        await Expect(Id("ExportJournal")).ToBeEnabledAsync();
    }
}
