using InannaOctaquetra.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace InannaOctaquetra.Views;

// The snapshot hook: a way for a developer - or an AI coding assistant - to see the page without
// a person at the screen. Set INANNA_SNAP=<png path> before launching and the app plays a scripted
// scenario, writes the page (and any open dialog) to PNG files, and exits. Nothing here runs unless
// INANNA_SNAP is set. Every switch is listed in the README's "The snapshot hook" section.
public sealed partial class MainPage
{
    /// <summary>
    /// Debug snapshot: INANNA_SNAP=&lt;png path&gt; renders the page after a scripted scenario
    /// named by INANNA_SCENARIO (empty | drawn | laid | interpreted | detail | dragging | shuffle |
    /// pdf | dialog) and exits. Open dialogs are written beside it as &lt;name&gt;.popupN.png.
    /// </summary>
    private void MaybeRunSnapshot()
    {
        if (Environment.GetEnvironmentVariable("INANNA_SNAP") is not { Length: > 0 } path) { return; }
        if (_snapshotStarted) { return; }
        _snapshotStarted = true;

        var scenario = Environment.GetEnvironmentVariable("INANNA_SCENARIO") ?? "empty";

        _ = DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await Task.Delay(1700);

                // The display cannot always give the window the full 1440x900 logical client
                // (menu bar / dock), so for a snapshot the scene is measured at the design size
                // regardless of what the window got. INANNA_SNAP_SIZE=WxH overrides it.
                var design = Environment.GetEnvironmentVariable("INANNA_SNAP_SIZE") ?? "1440x900";
                var parts = design.Split('x', 'X');
                if (parts.Length == 2
                    && double.TryParse(parts[0], out var designW)
                    && double.TryParse(parts[1], out var designH)
                    && designW > 400 && designH > 400)
                {
                    RootGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    RootGrid.VerticalAlignment = VerticalAlignment.Top;
                    RootGrid.Width = designW;
                    RootGrid.Height = designH;
                    RootGrid.UpdateLayout();
                    await Task.Delay(400);
                    Relayout();
                    await Task.Delay(300);
                }

                Console.WriteLine($"SNAP: page {RootGrid.ActualWidth}x{RootGrid.ActualHeight}, content {ContentCanvas.ActualWidth}x{ContentCanvas.ActualHeight}, card {_cardW}x{_cardH}, R {_radius}");
                Console.WriteLine($"SNAP: running scenario '{scenario}'");
                await RunScenarioAsync(scenario);
                await Task.Delay(900);

                // Dialogs live in the popup layer above the page, so report and render them
                // separately: a laid-out popup child with a real size proves the dialog is up.
                try
                {
                    var popups = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot);
                    Console.WriteLine($"SNAP: open popups = {popups.Count}");
                    var n = 0;
                    foreach (var popup in popups)
                    {
                        if (popup.Child is not FrameworkElement child) { continue; }
                        Console.WriteLine($"SNAP: popup[{n}] child {child.GetType().Name} {child.ActualWidth}x{child.ActualHeight}");
                        var ppath = Path.ChangeExtension(path, $".popup{n}.png");
                        var (pw, ph) = await SaveElementAsync(child, ppath, 0, 0);
                        if (pw > 0) { Console.WriteLine($"SNAP: popup[{n}] saved {ppath} ({pw}x{ph})"); }
                        n++;
                    }
                }
                catch (Exception ex) { Console.WriteLine($"SNAP: popup probe failed {ex.Message}"); }

                // the head may render at device pixels (2x on a Retina Mac); save at logical size
                // so the snapshot is exactly the layout the page was measured into
                var logicalW = (int)Math.Round(RootGrid.ActualWidth);
                var logicalH = (int)Math.Round(RootGrid.ActualHeight);
                var (w, h) = await SaveElementAsync(RootGrid, path, logicalW, logicalH);
                Console.WriteLine($"SNAP: saved {path} ({logicalW}x{logicalH}, rendered {w}x{h})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SNAP: FAILED {ex}");
            }
            Console.Out.Flush();
            Environment.Exit(0);
        });
    }

    private bool _snapshotStarted;

    //Renders one element and writes it as a PNG through CodeBrix.Imaging. Downscales to the
    //  logical size only when the capture came out larger (a Retina display).
    private static async Task<(int Width, int Height)> SaveElementAsync(UIElement element, string path,
        int logicalWidth, int logicalHeight)
    {
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(element);
        if (rtb.PixelWidth <= 0 || rtb.PixelHeight <= 0) { return (0, 0); }

        var bytes = (await rtb.GetPixelsAsync()).ToArray();
        var scale = logicalWidth > 100 && logicalHeight > 100 && logicalWidth < rtb.PixelWidth;
        SnapshotPng.Save(bytes, rtb.PixelWidth, rtb.PixelHeight, path,
            scale ? logicalWidth : 0, scale ? logicalHeight : 0);
        return (rtb.PixelWidth, rtb.PixelHeight);
    }

    //INANNA_PDF_PATH=<folder>: the "pdf" scenario (and any report while it is set) is written
    //  there under its suggested name instead of asking through the save panel
    private static string SnapshotPdfFolder()
    {
        if (Environment.GetEnvironmentVariable("INANNA_PDF_PATH") is not { Length: > 0 } folder) { return null; }
        try
        {
            Directory.CreateDirectory(folder);
            return folder;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task RunScenarioAsync(string scenario)
    {
        var viewModel = ViewModel;
        if (viewModel is null) { return; }

        viewModel.Querent = "Ninshubur";
        viewModel.Question = "What does the year ask of me?";

        switch (scenario.Trim().ToLowerInvariant())
        {
            case "drawn":
                for (var i = 0; i < 3; i++) { Run(viewModel.DrawCommand); await Task.Delay(180); }
                break;

            case "laid":
            case "interpreted":
            case "detail":
                await LayAllAsync();
                if (scenario.Equals("interpreted", StringComparison.OrdinalIgnoreCase))
                {
                    await InterpretAndWaitAsync();
                    // INANNA_SCROLL=<0..1> parks the interpretation panel that far down, so a
                    // snapshot can show the insights and counsel as well as the opening.
                    if (double.TryParse(Environment.GetEnvironmentVariable("INANNA_SCROLL"),
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var fraction))
                    {
                        await Task.Delay(350);
                        InterpScroller.UpdateLayout();
                        InterpScroller.ChangeView(null, InterpScroller.ScrollableHeight * Math.Clamp(fraction, 0, 1), null, true);
                        await Task.Delay(350);
                    }
                }
                else if (scenario.Equals("detail", StringComparison.OrdinalIgnoreCase))
                {
                    var heart = ItemAtStation(0);
                    if (heart is not null) { ShowDetail(heart); }
                    // INANNA_FLIP=1 turns the lore panel over to the enlarged card
                    if (Environment.GetEnvironmentVariable("INANNA_FLIP") is { Length: > 0 })
                    {
                        await Task.Delay(500);
                        FlipDetail();
                        var flipMs = int.TryParse(Environment.GetEnvironmentVariable("INANNA_FLIP_MS"), out var fm) ? fm : 700;
                        await Task.Delay(flipMs);
                    }
                    await Task.Delay(400);
                    // INANNA_SCROLL=<0..1> scrolls the detail text; INANNA_DETAIL_AT=x,y moves the box
                    if (_detailScroller is not null && double.TryParse(Environment.GetEnvironmentVariable("INANNA_SCROLL"),
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var detailFraction))
                    {
                        _detailScroller.UpdateLayout();
                        _detailScroller.ChangeView(null, _detailScroller.ScrollableHeight * Math.Clamp(detailFraction, 0, 1), null, true);
                        await Task.Delay(350);
                    }
                    if (Environment.GetEnvironmentVariable("INANNA_DETAIL_AT") is { Length: > 0 } at
                        && at.Split(',') is { Length: 2 } xy
                        && double.TryParse(xy[0], out var ax) && double.TryParse(xy[1], out var ay))
                    {
                        MoveDetailTo(ax, ay);
                        await Task.Delay(200);
                    }
                }
                break;

            case "dragging":
                for (var i = 0; i < 4; i++)
                {
                    var laid = DrawOne();
                    if (laid is null) { break; }
                    viewModel.PlaceOnStation(laid.Model, i);
                }
                DrawOne();
                DrawOne();
                await Task.Delay(150);
                var dragged = _items.FirstOrDefault(c => c.Model.InTray);
                if (dragged is not null)
                {
                    var target = StationCentre(5);
                    SimulateDragTo(dragged, new Point(target.X + 26, target.Y - 34));
                }
                break;

            case "shuffle":
                // INANNA_SHUFFLE_MS: how long after Shuffle starts the frame is captured (+~900 ms)
                Run(viewModel.ShuffleCommand);
                var shuffleMs = int.TryParse(Environment.GetEnvironmentVariable("INANNA_SHUFFLE_MS"), out var ms) ? ms : 150;
                await Task.Delay(Math.Max(0, shuffleMs));
                break;

            case "pdf":
                // Full in-process report: lay, interpret, compose the PDF with the embedded fonts,
                // write it to INANNA_PDF_PATH and show the "Report saved" dialog.
                await LayAllAsync();
                await InterpretAndWaitAsync();
                Run(viewModel.CreatePdfCommand);
                await Task.Delay(2500);
                break;

            case "dialog":
                // Verifies the ContentDialog + XamlRoot wiring the confirmations depend on.
                var confirm = ConfirmAsync("Clear the Octaquetra?",
                    "Every card returns to the deck and the spread is emptied.", "Clear");
                await Task.Delay(1400);
                Console.WriteLine($"SNAP: confirm dialog status = {confirm.Status} " +
                                  "(WaitingForActivation means it is on screen)");
                break;
        }
    }

    private static void Run(System.Windows.Input.ICommand command)
    {
        if (command?.CanExecute(null) == true) { command.Execute(null); }
    }

    //Draws one card through the view model and hands back the page's visual for it
    private CardItem DrawOne()
    {
        var before = _items.Count;
        Run(ViewModel?.DrawCommand);
        return _items.Count > before ? _items[^1] : null;
    }

    private async Task InterpretAndWaitAsync()
    {
        Run(ViewModel?.InterpretCommand);
        for (var i = 0; i < 60 && ViewModel?.Interpretation is null; i++) { await Task.Delay(50); }
        await Task.Delay(300);
    }

    /// <summary>
    /// Put the scene into the exact state a real pointer drag produces (lifted card, shadow
    /// beneath it, nearest slot highlighted) so the drag affordances can be verified from a
    /// snapshot. The pointer handlers reach this same state through CapturePointer.
    /// </summary>
    private void SimulateDragTo(CardItem item, Point centre)
    {
        _dragItem = item;
        _dragging = true;
        _dragMoved = false;
        BeginDragVisuals(item);

        var x = centre.X - _cardW / 2;
        var y = centre.Y - _cardH / 2;
        Canvas.SetLeft(item.View, x);
        Canvas.SetTop(item.View, y);
        Canvas.SetZIndex(item.View, 900);
        if (_dragShadow is not null)
        {
            Canvas.SetLeft(_dragShadow, x + 5);
            Canvas.SetTop(_dragShadow, y + 8);
        }
        SetStationHighlight(NearestStation(centre.X, centre.Y));
    }

    /// <summary>Deterministic nine-card lay for the snapshots: two reversed, the rest upright.</summary>
    private async Task LayAllAsync()
    {
        var viewModel = ViewModel;
        if (viewModel is null) { return; }

        for (var i = 0; i < 9; i++)
        {
            if (viewModel.CardAt(i) is not null) { continue; }
            var item = DrawOne();
            if (item is null) { break; }
            viewModel.PlaceOnStation(item.Model, i);
            if ((i == 3 || i == 6) != item.Model.IsReversed) { viewModel.ToggleReversed(item.Model); }
            await Task.Delay(70);
        }
    }
}
