using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PdfSideBySide.Views;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PdfSideBySide.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private static readonly string[] PanButtons =
    {
        "PanLeftUp", "PanLeftLeft", "PanLeftRight", "PanLeftDown",
        "PanRightUp", "PanRightLeft", "PanRightRight", "PanRightDown",
    };
    private static readonly string[] CommandButtons =
    {
        "ZoomOut", "ZoomIn", "ZoomReset", "PreviousPage", "NextPage", "AdjustPrevious", "AdjustNext",
    };

    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private Locator Status => Page.GetByTestId("StatusText");
    private Locator ZoomLabel => Page.GetByTestId("ZoomLabel");
    private Locator LeftPageLabel => Page.GetByTestId("LeftPageLabel");
    private Locator RightPageLabel => Page.GetByTestId("RightPageLabel");

    [Fact]
    public async Task Empty_panes_show_placeholders_and_disable_navigation_and_zoom()
    {
        await Expect(Page.GetByTestId("LeftPlaceholder")).ToHaveTextAsync("No document selected");
        await Expect(Page.GetByTestId("RightPlaceholder")).ToHaveTextAsync("No document selected");
        await Expect(Button("Document 1…")).ToBeEnabledAsync();
        await Expect(Button("Document 2…")).ToBeEnabledAsync();
        foreach (var id in CommandButtons) await Expect(Page.GetByTestId(id)).ToBeDisabledAsync();
        foreach (var id in PanButtons) await Expect(Page.GetByTestId(id)).ToBeDisabledAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("100%");
        (await Page.EvaluateAsync(() => Fixture.Model.StatusText)).Should().BeEmpty();
        (await Page.EvaluateAsync(() => Fixture.Model.LeftPane.PageLabel)).Should().BeEmpty();
        (await Page.EvaluateAsync(() => double.IsNaN(Fixture.LeftImage.Width))).Should().BeTrue();
    }

    [Fact]
    public async Task Browsing_left_shows_file_name_page_label_and_rendered_image()
    {
        await OpenAsync("Left", Fixture.FivePages);
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(1);
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 5");
        await Expect(Page.GetByTestId("LeftPlaceholder")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("RightPlaceholder")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.LeftPane.FileName)).Should().Be("five.pdf");
        (await Page.EvaluateAsync(() => Fixture.LeftImage.Width)).Should().BeGreaterThan(0);
        (await Page.EvaluateAsync(() => Fixture.Model.StatusText)).Should().BeEmpty();
        // One document is enough to zoom, never to move between pages.
        await Expect(Page.GetByTestId("ZoomIn")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("NextPage")).ToBeDisabledAsync();

        // The rendered page is on screen: white paper with the black bar the fixture drew.
        var box = await Page.GetByRole(AriaRole.Img).First.BoundingBoxAsync();
        using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
        var dark = 0;
        var white = 0;
        for (var y = (int)box.Y; y < (int)(box.Y + box.Height); y += 2)
        {
            for (var x = (int)box.X; x < (int)(box.X + box.Width); x += 2)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Red < 60 && pixel.Green < 60 && pixel.Blue < 60) dark++;
                if (pixel.Red > 245 && pixel.Green > 245 && pixel.Blue > 245) white++;
            }
        }
        dark.Should().BeGreaterThan(0);
        white.Should().BeGreaterThan(dark);
    }

    [Fact]
    public async Task Cancelled_browse_leaves_pane_empty()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(null);
        await Button("Document 1…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.OpenFileRequestCount, count => count == 1);
        await WaitForIdleAsync();
        await Expect(Page.GetByTestId("LeftPlaceholder")).ToHaveTextAsync("No document selected");
        (await Page.EvaluateAsync(() => Fixture.Model.LeftPane.FilePath)).Should().BeEmpty();
        await Expect(Page.GetByTestId("ZoomIn")).ToBeDisabledAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Opening_both_documents_shows_comparing_status()
    {
        await OpenBothAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 1:1");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 3");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 1 of 5");
        await Expect(Page.GetByTestId("RightFilePath")).ToHaveTextAsync(Fixture.FivePages);
        await Expect(Page.GetByTestId("NextPage")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("AdjustNext")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("PreviousPage")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("AdjustPrevious")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.RightImage.Width)).Should().BeGreaterThan(0);
        await SnapshotAsync("PdfSideBySide-side-by-side");
    }

    [Fact]
    public async Task Next_and_previous_move_both_documents()
    {
        await OpenBothAsync();
        await Page.GetByTestId("NextPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:2");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 2 of 3");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 2 of 5");
        await Expect(Page.GetByTestId("PreviousPage")).ToBeEnabledAsync();
        await Page.GetByTestId("PreviousPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 1:1");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 3");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 1 of 5");
        await Expect(Page.GetByTestId("PreviousPage")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Next_stops_at_the_longer_documents_end()
    {
        await OpenBothAsync();
        foreach (var expected in new[] { "2:2", "3:3", "3:4", "3:5" })
        {
            await Expect(Page.GetByTestId("NextPage")).ToBeEnabledAsync();
            await Page.GetByTestId("NextPage").ClickAsync();
            await Expect(Status).ToHaveTextAsync("Comparing " + expected);
        }
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 3 of 3");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 5 of 5");
        await Expect(Page.GetByTestId("NextPage")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("AdjustNext")).ToBeDisabledAsync();
        await WaitForIdleAsync();
        // Back one moves both documents again.
        await Page.GetByTestId("PreviousPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:4");
    }

    [Fact]
    public async Task Adjust_right_moves_only_document_two()
    {
        await OpenBothAsync();
        await Page.GetByTestId("AdjustNext").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 1:2");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 2 of 5");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 3");
        await Expect(Page.GetByTestId("AdjustPrevious")).ToBeEnabledAsync();
        // Both documents still step together from the adjusted pair.
        await Page.GetByTestId("NextPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:3");
        await Page.GetByTestId("AdjustPrevious").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:2");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 2 of 3");
    }

    [Fact]
    public async Task Zoom_in_out_and_reset_update_label_and_image_size_in_both_panes()
    {
        await OpenBothAsync();
        var leftFit = await Page.EvaluateAsync(() => Fixture.LeftImage.Width);
        var rightFit = await Page.EvaluateAsync(() => Fixture.RightImage.Width);
        await Expect(Page.GetByTestId("ZoomOut")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("ZoomReset")).ToBeDisabledAsync();

        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        await Fixture.Application.WaitForAsync(() => Fixture.LeftImage.Width, width => width > leftFit, description: "zoomed left image");
        await Fixture.Application.WaitForAsync(() => Fixture.RightImage.Width, width => width > rightFit, description: "zoomed right image");
        (await Page.EvaluateAsync(() => Fixture.LeftImage.Width)).Should().BeApproximately(Math.Floor(leftFit * 1.25), 1);
        (await Page.EvaluateAsync(() => Fixture.RightImage.Width)).Should().BeApproximately(Math.Floor(rightFit * 1.25), 1);
        await Expect(Page.GetByTestId("ZoomOut")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("ZoomReset")).ToBeEnabledAsync();

        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("150%");
        await Page.GetByTestId("ZoomOut").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        await Button("100%").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("100%");
        await Fixture.Application.WaitForAsync(() => Fixture.LeftImage.Width, width => width == leftFit, description: "fitted left image");
        await Fixture.Application.WaitForAsync(() => Fixture.RightImage.Width, width => width == rightFit, description: "fitted right image");
        await Expect(Page.GetByTestId("ZoomReset")).ToBeDisabledAsync();
        await WaitForIdleAsync();
    }

    [Fact]
    public async Task Zoom_in_disables_at_1000_percent()
    {
        await OpenAsync("Left", Fixture.ThreePages);
        foreach (var expected in new[] { "125%", "150%", "200%", "300%", "400%", "500%", "700%", "1000%" })
        {
            await Expect(Page.GetByTestId("ZoomIn")).ToBeEnabledAsync();
            await Page.GetByTestId("ZoomIn").ClickAsync();
            await Expect(ZoomLabel).ToHaveTextAsync(expected);
        }
        await Expect(Page.GetByTestId("ZoomIn")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("ZoomOut")).ToBeEnabledAsync();
        await WaitForIdleAsync();
        await Page.GetByTestId("ZoomOut").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("700%");
        await Expect(Page.GetByTestId("ZoomIn")).ToBeEnabledAsync();
        await WaitForIdleAsync();
    }

    [Fact]
    public async Task Pan_buttons_enable_only_when_zoomed_and_scroll_only_their_pane()
    {
        await OpenBothAsync();
        foreach (var id in PanButtons) await Expect(Page.GetByTestId(id)).ToBeDisabledAsync();

        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        foreach (var id in PanButtons) await Expect(Page.GetByTestId(id)).ToBeEnabledAsync();
        await WaitForIdleAsync();
        // Centred: whichever way the zoomed page overflows its viewer, it is scrolled half way.
        await Fixture.Application.WaitForAsync(() => LeftOffset() + RightOffset(), offset => offset > 0, description: "centred panes");
        var right = await Page.EvaluateAsync(() => RightOffset());

        await Page.GetByTestId("PanLeftUp").ClickAsync();
        await Page.GetByTestId("PanLeftLeft").ClickAsync();
        await Fixture.Application.WaitForAsync(() => LeftOffset(), offset => offset == 0, description: "left pane panned to its top-left corner");
        await Expect(Page.GetByTestId("PanLeftUp")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("PanLeftLeft")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("PanLeftDown")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("PanRightUp")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => RightOffset())).Should().Be(right);

        await Button("100%").ClickAsync();
        foreach (var id in PanButtons) await Expect(Page.GetByTestId(id)).ToBeDisabledAsync();
        await WaitForIdleAsync();
    }

    [Fact]
    public async Task Page_change_while_zoomed_returns_to_fit_page()
    {
        await OpenBothAsync();
        var leftFit = await Page.EvaluateAsync(() => Fixture.LeftImage.Width);
        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        await Page.GetByTestId("PanLeftUp").ClickAsync();
        await WaitForIdleAsync();
        await Page.GetByTestId("NextPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:2");
        await Expect(ZoomLabel).ToHaveTextAsync("100%");
        foreach (var id in PanButtons) await Expect(Page.GetByTestId(id)).ToBeDisabledAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.LeftImage.Width, width => width == leftFit, description: "fitted left image");
        await WaitForIdleAsync();
    }

    [Fact]
    public async Task Replacing_a_document_resets_view_and_labels()
    {
        await OpenBothAsync();
        await Page.GetByTestId("NextPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:2");
        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        await WaitForIdleAsync();

        await OpenAsync("Left", Fixture.FourPages);
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 4");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 2 of 5");
        await Expect(Status).ToHaveTextAsync("Comparing 1:2");
        await Expect(ZoomLabel).ToHaveTextAsync("100%");
        (await Page.EvaluateAsync(() => Fixture.Model.LeftPane.FileName)).Should().Be("four.pdf");
    }

    [Fact]
    public async Task Choosing_the_same_file_for_both_panes_shows_duplicate_error_and_keeps_pane()
    {
        await OpenAsync("Left", Fixture.FivePages);
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.FivePages);
        await Button("Document 2…").ClickAsync();
        // The dialog breaks its message into fixed-length lines, so each phrase stays inside one of them.
        await Expect(Dialog).ToContainTextAsync("“five.pdf” is already selected as Document 1; choose a different PDF");
        await CloseDialogAsync();
        await WaitForIdleAsync();
        await Expect(Page.GetByTestId("RightPlaceholder")).ToHaveTextAsync("No document selected");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 5");
        (await Page.EvaluateAsync(() => Fixture.Model.RightPane.FilePath)).Should().BeEmpty();
        (await Page.EvaluateAsync(() => Fixture.Model.StatusText)).Should().BeEmpty();
    }

    [Fact]
    public async Task Non_pdf_file_reports_could_not_be_read()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.NotAPdf);
        await Button("Document 1…").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Could not open the PDF document.");
        await Expect(Dialog).ToContainTextAsync("“notes.pdf” could not be read as a");
        await CloseDialogAsync();
        await WaitForIdleAsync();
        await Expect(Page.GetByTestId("LeftPlaceholder")).ToHaveTextAsync("No document selected");
        await Expect(Page.GetByTestId("ZoomIn")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Startup_arguments_preload_both_documents()
    {
        Fixture.Startup.Documents = new[] { Fixture.ThreePages, Fixture.FivePages };
        await Fixture.ResetAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 1:1");
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 3");
        await Expect(RightPageLabel).ToHaveTextAsync("Page 1 of 5");
        await Expect(Page.GetByTestId("LeftFilePath")).ToHaveTextAsync(Fixture.ThreePages);
        await WaitForIdleAsync();
        (await Page.EvaluateAsync(() => Fixture.LeftImage.Width)).Should().BeGreaterThan(0);
        (await Page.EvaluateAsync(() => Fixture.RightImage.Width)).Should().BeGreaterThan(0);
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(0);
    }

    [Fact]
    public async Task Missing_startup_document_is_reported_and_panes_stay_usable()
    {
        Fixture.Startup.Documents = new[] { Path.Combine(Fixture.DataDirectory, "missing.pdf"), Fixture.FivePages };
        await Fixture.ResetAsync();
        await Expect(Dialog).ToContainTextAsync("Could not open the documents given on the command line.");
        await CloseDialogAsync();
        await WaitForIdleAsync();
        await Expect(Page.GetByTestId("LeftPlaceholder")).ToHaveTextAsync("No document selected");
        await OpenAsync("Left", Fixture.ThreePages);
        await Expect(LeftPageLabel).ToHaveTextAsync("Page 1 of 3");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_both_panes_and_controls_usable()
    {
        await OpenBothAsync();
        var left = await Page.GetByRole(AriaRole.Img).First.BoundingBoxAsync();
        var right = await Page.GetByRole(AriaRole.Img).Last.BoundingBoxAsync();
        left.Width.Should().BeGreaterThan(0);
        right.Width.Should().BeGreaterThan(0);
        (left.X + left.Width).Should().BeLessThan(right.X);
        await Page.GetByTestId("NextPage").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Comparing 2:2");
        await Page.GetByTestId("ZoomIn").ClickAsync();
        await Expect(ZoomLabel).ToHaveTextAsync("125%");
        await Page.GetByTestId("PanRightDown").ClickAsync();
        await Expect(Page.GetByTestId("PanRightDown")).ToBeDisabledAsync();
        await WaitForIdleAsync();
        await SnapshotAsync("PdfSideBySide-portrait");
    }

    private double LeftOffset() => Fixture.LeftScroller.HorizontalOffset + Fixture.LeftScroller.VerticalOffset;
    private double RightOffset() => Fixture.RightScroller.HorizontalOffset + Fixture.RightScroller.VerticalOffset;

    private async Task OpenAsync(string side, string path)
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await Page.GetByTestId("Browse" + side).ClickAsync();
        await Expect(Page.GetByTestId(side + "FilePath")).ToHaveTextAsync(path);
        await WaitForIdleAsync();
    }

    private async Task OpenBothAsync()
    {
        await OpenAsync("Left", Fixture.ThreePages);
        await OpenAsync("Right", Fixture.FivePages);
    }

    // Neither a browse nor a page render is still in flight.
    private Task WaitForIdleAsync() => Fixture.Application.WaitForAsync(
        () => !Fixture.Model.IsBusy && !Fixture.Model.LeftPane.IsRendering && !Fixture.Model.RightPane.IsRendering,
        idle => idle, description: "browse and renders finished");

    private async Task CloseDialogAsync()
    {
        await Dialog.GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }
}
