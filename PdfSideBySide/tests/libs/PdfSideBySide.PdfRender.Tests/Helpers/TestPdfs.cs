using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
using System;
using System.IO;

namespace PdfSideBySide.PdfRender.Tests.Helpers;

/// <summary>
/// The PDF files the tests work with: the real-world <c>Inanna.pdf</c> asset, and small
/// synthetic documents with a chosen page count written to a per-test temp folder.
/// </summary>
internal static class TestPdfs
{
    /// <summary>Page count of the <c>assets/Inanna.pdf</c> sample.</summary>
    public const int InannaPageCount = 45;

    /// <summary>Full path of the <c>assets/Inanna.pdf</c> sample copied beside the test binary.</summary>
    public static string InannaPath => Path.Combine(AppContext.BaseDirectory, "assets", "Inanna.pdf");

    /// <summary>A fresh, empty temp folder for one test's files.</summary>
    public static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "PdfSideBySide.PdfRender.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>
    /// Writes a PDF with pageCount pages to folder as fileName; every page carries a
    /// filled rectangle placed by page number so the pages are not blank.
    /// </summary>
    public static string WriteSamplePdf(string folder, string fileName, int pageCount)
    {
        using var document = new PdfDocument();
        for (var i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            using var graphics = XGraphics.FromPdfPage(page);
            graphics.DrawRectangle(XBrushes.Black, new XRect(50, 50 + i * 20, 200, 30));
        }

        var path = Path.Combine(folder, fileName);
        document.Save(path);
        return path;
    }
}
