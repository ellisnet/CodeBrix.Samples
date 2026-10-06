using System.IO;
using System.Threading.Tasks;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfSideBySide.Services;
using PdfSideBySide.ViewModels;
using PdfSideBySide.Views;

namespace PdfSideBySide.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public StartupArgumentsFixture Startup { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public ScrollViewer LeftScroller => (ScrollViewer)View.FindName("LeftScroller");
    public ScrollViewer RightScroller => (ScrollViewer)View.FindName("RightScroller");
    public Image LeftImage => (Image)View.FindName("LeftImage");
    public Image RightImage => (Image)View.FindName("RightImage");

    // Synthetic documents written once per run; each page carries a black bar placed by page number.
    public string ThreePages { get; private set; }
    public string FourPages { get; private set; }
    public string FivePages { get; private set; }
    public string NotAPdf { get; private set; }

    // The test host's own arguments are never documents: the startup command line comes from Startup.
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<IStartupArguments>(Startup));

    protected override void Prepare()
    {
        ThreePages = WritePdf("three.pdf", 3);
        FourPages = WritePdf("four.pdf", 4);
        FivePages = WritePdf("five.pdf", 5);
        NotAPdf = Path.Combine(DataDirectory, "notes.pdf");
        File.WriteAllText(NotAPdf, "Plain text with a .pdf name.");
    }

    // A new page reads the startup command line while it loads, so documents a test queued are cleared after it.
    protected override Task AfterResetAsync()
    {
        Startup.Documents = null;
        return Task.CompletedTask;
    }

    private string WritePdf(string fileName, int pageCount)
    {
        using var document = new PdfDocument();
        for (var i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            using var graphics = XGraphics.FromPdfPage(page);
            graphics.DrawRectangle(XBrushes.Black, new XRect(50, 50 + i * 20, 200, 30));
        }
        var path = Path.Combine(DataDirectory, fileName);
        document.Save(path);
        return path;
    }
}

public sealed class StartupArgumentsFixture : IStartupArguments
{
    // The left and right documents for the next page to pre-load; null starts with empty panes.
    public string[] Documents { get; set; }

    public string[] GetCommandLineArgs() => Documents == null
        ? new[] { "PdfSideBySide" }
        : new[] { "PdfSideBySide", Documents[0], Documents[1] };
}
