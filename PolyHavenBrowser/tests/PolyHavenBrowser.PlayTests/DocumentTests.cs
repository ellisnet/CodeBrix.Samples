using System.IO;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    private const float DocumentTimeout = 120000;
    private Locator Document => Page.GetByTestId("DocumentCommand");

    [Fact]
    public async Task Document_save_writes_pdf_and_announces_path()
    {
        var folder = await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        var path = Path.Combine(folder, "Sheets", "Oak Chair one-sheet.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Document.ClickAsync();
        await Expect(Dialog).ToContainTextAsync("The marketing one-sheet for “Oak Chair” has been created.", new() { Timeout = DocumentTimeout });
        await Expect(Dialog).ToContainTextAsync("It was saved to:");
        await CloseDialogAsync();
        await Expect(Page.GetByText("Saved: " + path, new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Document).ToBeEnabledAsync();
        var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
    }

    [Fact]
    public async Task Document_save_cancel_is_a_no_op()
    {
        var folder = await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Document.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1);
        await Expect(Document).ToBeEnabledAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Model.DocumentStatusText)).Should().BeEmpty();
        (await Page.EvaluateAsync(() => Fixture.Model.IsCreatingDocument)).Should().BeFalse();
        Directory.Exists(Path.Combine(folder, "_document-backdrops")).Should().BeFalse();
        Fixture.Transport.Requests.Should().NotContain(request => request == "/files/wood_table");
    }

    [Fact]
    public async Task Document_suggests_title_based_file_name()
    {
        await ChooseDownloadFolderAsync();
        await OpenModelAsync("Brass Lantern");
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Document.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("Brass Lantern one-sheet.pdf");
    }

    [Fact]
    public async Task Document_button_disabled_while_document_builds()
    {
        var folder = await ChooseDownloadFolderAsync();
        await OpenModelAsync("Oak Chair");
        var path = Path.Combine(folder, "building.pdf");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        Fixture.Transport.Hold();
        await Document.ClickAsync();
        await Expect(Page.GetByText("Setting the stage (downloading backdrop textures)…", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Document).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsCreatingDocument)).Should().BeTrue();
        Fixture.Transport.Release();
        await Expect(Dialog).ToContainTextAsync("has been created", new() { Timeout = DocumentTimeout });
        await CloseDialogAsync();
        await Expect(Document).ToBeEnabledAsync();
        File.Exists(path).Should().BeTrue();
    }
}
