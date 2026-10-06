using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Platform.TkCanvas.Hosting;
using CodeBrix.Platform.TkCanvas.Menus;
using CodeBrix.Samples.PlayTests;
using DRAKON.Brix.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SilverAssertions;
using Xunit;

namespace DRAKON.Brix.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string DiagramList = ".root.pnd.left.dialist.treeview";
    private const string CurrentDiagram = "mwc::editor_state $mwc::db current_dia";

    [Fact]
    public async Task Editor_boots_hosted_and_shows_the_intro_window()
    {
        await Expect(Page.GetByTestId("TkHost")).ToBeVisibleAsync();
        Fixture.Runtime.IsReady.Should().BeTrue();
        (await Page.EvaluateAsync(() => Host.Root.FindDescendant(".intro") != null)).Should().BeTrue();
        (await Fixture.TclAsync("wm title .intro")).Should().StartWith("DRAKON Editor");
        (await Fixture.TclAsync("list [winfo exists .mainmenu] $::g_loaded")).Should().Be("1 0");
        Fixture.Diagnostics.Any(message => message.StartsWith("bgerror", StringComparison.Ordinal)).Should().BeFalse();
        await SnapshotAsync("DRAKON.Brix-intro");
    }

    [Fact]
    public async Task Preloaded_drn_opens_through_DRAKONBRIX_OPEN()
    {
        Fixture.OpenOnReset = Fixture.CopyExample("02.Silhouette.drn");
        await Fixture.ResetAsync();
        (await Fixture.TclAsync("list [winfo exists .intro] $::g_loaded")).Should().Be("0 1");
        (await Fixture.TclAsync("wm title .")).Should().Be("02.Silhouette.drn - DRAKON Editor");
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("2");
        (await Page.EvaluateAsync(() => Host.Root.FindDescendant(".intro") == null)).Should().BeTrue();
        await SnapshotAsync("DRAKON.Brix-preloaded");
    }

    [Fact]
    public async Task Hidden_input_element_holds_focus_after_load()
    {
        await Fixture.Application.WaitForAsync(() =>
        {
            var focused = FocusManager.GetFocusedElement(Fixture.View.XamlRoot) as TextBox;
            DependencyObject parent = focused;
            while (parent is FrameworkElement element && parent != Host) parent = element.Parent;
            return focused != null && focused.Opacity == 0 && parent == Host;
        }, inside => inside, description: "hidden Tk input element focused");
        (await Fixture.TclAsync("focus")).Should().Be(".intro");
    }

    [Fact]
    public async Task Open_existing_on_the_intro_opens_a_drn_through_the_scripted_picker()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.CopyExample("01.Insertion.drn"));
        await ClickTkAsync(".intro.root.open");
        await Fixture.WaitForTclAsync("set ::g_loaded", "1");
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(1);
        (await Fixture.TclAsync("list [winfo exists .intro] [wm title .]")).Should().Be("0 {01.Insertion.drn - DRAKON Editor}");
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("4");
        (await Fixture.TclAsync(CurrentDiagram)).Should().Be("4");
        await SnapshotAsync("DRAKON.Brix-opened");
    }

    [Fact]
    public async Task Enter_on_the_intro_opens_a_drn_through_the_scripted_picker()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.CopyExample("05.Loops.drn"));
        await Page.Keyboard.PressAsync("Enter");
        await Fixture.WaitForTclAsync("set ::g_loaded", "1");
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(1);
        (await Fixture.TclAsync("wm title .")).Should().Be("05.Loops.drn - DRAKON Editor");
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("7");
    }

    [Fact]
    public async Task Cancelled_open_picker_on_the_intro_asks_to_quit()
    {
        // DRAKON quits when the intro's open is cancelled; the fixture's quit action records it.
        Fixture.Application.FilePickers.EnqueueOpenFile(null);
        await ClickTkAsync(".intro.root.open");
        await Fixture.Application.WaitForAsync(() => Fixture.QuitCodes.Count, count => count == 1, description: "quit request");
        Fixture.QuitCodes.Should().Equal(0);
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(1);
        (await Fixture.TclAsync("list [winfo exists .intro] $::g_loaded")).Should().Be("1 0");
    }

    [Fact]
    public async Task Escape_on_the_intro_asks_to_quit()
    {
        await Page.Keyboard.PressAsync("Escape");
        await Fixture.Application.WaitForAsync(() => Fixture.QuitCodes.Count, count => count == 1, description: "quit request");
        Fixture.QuitCodes.Should().Equal(0);
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(0);
    }

    [Fact]
    public async Task Quit_from_the_File_menu_invokes_the_quit_action()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        await ClickMenuAsync("File", "Quit");
        await Fixture.Application.WaitForAsync(() => Fixture.QuitCodes.Count, count => count == 1, description: "quit request");
        Fixture.QuitCodes.Should().Equal(0);
        await Fixture.TclAsync("update idletasks");
        Fixture.Diagnostics.Any(message => message.Contains(nameof(QuitRequestedException), StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public async Task Open_from_the_File_menu_switches_to_another_drn()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.CopyExample("06.Logic.drn"));
        await ClickMenuAsync("File", "Open...");
        await Fixture.WaitForTclAsync("wm title .", "06.Logic.drn - DRAKON Editor");
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(2);
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("7");
        Fixture.QuitCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancelled_open_from_the_File_menu_keeps_the_document()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        Fixture.Application.FilePickers.EnqueueOpenFile(null);
        await ClickMenuAsync("File", "Open...");
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.OpenFileRequestCount,
            count => count == 2, description: "second open request");
        (await Fixture.TclAsync("list [wm title .] $::g_loaded")).Should().Be("{01.Insertion.drn - DRAKON Editor} 1");
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("4");
        Fixture.QuitCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task Clicking_a_diagram_in_the_list_shows_it()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        (await Fixture.TclAsync(CurrentDiagram)).Should().Be("4");
        var row = await DiagramRowAsync("Make salad");
        await Page.GetByTestId("TkHost").ClickAsync(new() { Position = row });
        await Fixture.WaitForTclAsync(CurrentDiagram, "2");
        (await Fixture.TclAsync($"{DiagramList} item [{DiagramList} selection] -text")).Should().Be("Make salad");
        await SnapshotAsync("DRAKON.Brix-diagram-selected");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Double_click_on_an_icon_opens_the_text_window()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        await Page.GetByTestId("TkHost").ClickAsync(new() { Position = await IconAsync("Cut in pieces"), ClickCount = 2 });
        await Fixture.WaitForTclAsync("winfo exists .twindow", "1");
        var item = await ItemIdAsync("Cut in pieces");
        (await Fixture.TclAsync("wm title .twindow")).Should().Be($"Change icon text: item {item}");
        (await Fixture.TclAsync(".twindow.root.entry.text get 1.0 end-1c")).Should().Be("Cut in pieces");
        await Fixture.WaitForTclAsync("focus", ".twindow.root.entry.text");
        await SnapshotAsync("DRAKON.Brix-text-window");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Typing_in_the_text_window_changes_the_icon_text()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        var item = await ItemIdAsync("Cut in pieces");
        await EditIconTextAsync("Cut in pieces", " finely");
        (await Fixture.TclAsync($"$mwc::db onecolumn {{select text from items where item_id = {item}}}"))
            .Should().Be("Cut in pieces finely");
        (await Fixture.TclAsync("winfo exists .twindow")).Should().Be("0");
        await SnapshotAsync("DRAKON.Brix-icon-edited");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Undo_from_the_Edit_menu_restores_an_icon_text()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        var item = await ItemIdAsync("Cut in pieces");
        await EditIconTextAsync("Cut in pieces", " finely");
        // Changing the text records two undo steps: the text, then fitting the icon sizes.
        await ClickMenuAsync("Edit", "Undo");
        await Fixture.WaitForTclAsync(".mainmenu.edit entrycget 0 -label", "Undo: Change text");
        await ClickMenuAsync("Edit", "Undo");
        await Fixture.WaitForTclAsync($"$mwc::db onecolumn {{select text from items where item_id = {item}}}", "Cut in pieces");
        (await Fixture.TclAsync(".mainmenu.edit entrycget 1 -label")).Should().Be("Redo: Change text");
    }

    [Fact]
    public async Task Save_as_writes_a_drn_through_the_save_picker()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        var folder = Path.Combine(Fixture.DataDirectory, "saved", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, "Lunch.drn");
        Fixture.Application.FilePickers.EnqueueSaveFile(target);
        await ClickMenuAsync("File", "Save as...");
        await Fixture.WaitForTclAsync("wm title .", "Lunch.drn - DRAKON Editor");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        var header = new byte[15];
        using (var stream = File.OpenRead(target)) stream.ReadExactly(header);
        Encoding.ASCII.GetString(header).Should().Be("SQLite format 3");
        (await Fixture.TclAsync($"llength [{DiagramList} children {{}}]")).Should().Be("4");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Copied_icon_reaches_the_isolated_clipboard()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        var item = await ItemIdAsync("Stuff with garlic");
        await Page.GetByTestId("TkHost").ClickAsync(new() { Position = await IconAsync("Stuff with garlic") });
        await Fixture.WaitForTclAsync($"$mwc::db onecolumn {{select selected from items where item_id = {item}}}", "1");
        await ClickMenuAsync("Edit", "Copy");
        await Fixture.TclAsync("update idletasks");
        var clipboard = await Page.ClipboardTextAsync();
        clipboard.Should().StartWith(" DRAKON 1.33 items ");
        clipboard.Should().Contain("{Stuff with garlic}");
        clipboard.Should().NotContain("Cut in pieces");
    }

    [Fact]
    public async Task Wheel_scrolls_the_diagram_canvas()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        var before = Number(await Fixture.TclAsync("$mw::canvas canvasy 0"));
        var canvas = await TkCentreAsync("$mw::canvas");
        await Page.Mouse.MoveAsync(canvas.X, canvas.Y);
        await Page.Mouse.WheelAsync(0, 120);
        await Fixture.WaitForTclAsync("$mw::canvas canvasy 0", value => Number(value) > before);
    }

    [Fact]
    public async Task Recent_files_persist_across_page_reset()
    {
        (await Fixture.TclAsync("set ui::intro_files")).Should().BeEmpty();
        await OpenThroughIntroAsync("03.The skewer.drn");
        File.Exists(Fixture.SettingsFile).Should().BeTrue();
        Path.GetFullPath(await Fixture.TclAsync("app_settings::p.path drakon_editor")).Should().Be(Path.GetFullPath(Fixture.SettingsFile));
        Fixture.KeepSettingsOnReset = true;
        await Fixture.ResetAsync();
        (await Fixture.TclAsync("winfo exists .intro")).Should().Be("1");
        (await Fixture.TclAsync("llength $ui::intro_files")).Should().Be("1");
        (await Fixture.TclAsync("file tail [lindex $ui::intro_files 0]")).Should().Be("03.The skewer.drn");
    }

    private TkHostView Host => (TkHostView)Fixture.View.FindName("TkHost");

    private static float Number(string value) => float.Parse(value, CultureInfo.InvariantCulture);

    private async Task<string> OpenThroughIntroAsync(string example)
    {
        var file = Fixture.CopyExample(example);
        Fixture.Application.FilePickers.EnqueueOpenFile(file);
        await ClickTkAsync(".intro.root.open");
        await Fixture.WaitForTclAsync("set ::g_loaded", "1");
        return file;
    }

    // Tk widgets are not in the XAML tree: click them at the root-relative geometry Tk reports.
    private async Task ClickTkAsync(string path) =>
        await Page.GetByTestId("TkHost").ClickAsync(new() { Position = await TkCentreAsync(path) });

    private async Task<LocatorPosition> TkCentreAsync(string path)
    {
        var geometry = (await Fixture.TclAsync($"list [winfo rootx {path}] [winfo rooty {path}] [winfo width {path}] [winfo height {path}]"))
            .Split(' ').Select(Number).ToArray();
        return new() { X = geometry[0] + geometry[2] / 2, Y = geometry[1] + geometry[3] / 2 };
    }

    // Menu entries have no Tk window of their own; the menu widgets report their entry rectangles.
    private async Task ClickMenuAsync(string menu, string entry)
    {
        var host = Page.GetByTestId("TkHost");
        var cascade = await Page.EvaluateAsync(() =>
        {
            foreach (var window in Host.Root.Children)
                if (window.Widget is MenuWidget { IsMenubar: true } bar)
                    for (var index = 0; index < bar.Entries.Count; index++)
                        if (bar.Entries[index].Label == menu)
                        {
                            var rect = bar.EntryRect(index);
                            return new LocatorPosition { X = window.X + rect.MidX, Y = window.Y + rect.MidY };
                        }
            return null;
        });
        cascade.Should().NotBeNull();
        await host.ClickAsync(new() { Position = cascade });
        await Fixture.Application.WaitForAsync(() => Host.Tree.Menus.Posted.Count, count => count == 1, description: menu + " menu posted");
        var item = await Page.EvaluateAsync(() =>
        {
            var posted = Host.Tree.Menus.Posted[0];
            for (var index = 0; index < posted.Entries.Count; index++)
                if (posted.Entries[index].Label.StartsWith(entry, StringComparison.Ordinal))
                {
                    var rect = posted.EntryRect(index);
                    return new LocatorPosition { X = posted.Window.X + rect.MidX, Y = posted.Window.Y + rect.MidY };
                }
            return null;
        });
        item.Should().NotBeNull();
        await host.ClickAsync(new() { Position = item });
        await Fixture.Application.WaitForAsync(() => Host.Tree.Menus.Posted.Count, count => count == 0, description: menu + " menu closed");
    }

    private Task<string> ItemIdAsync(string text) =>
        Fixture.TclAsync($"set diagram [{CurrentDiagram}]; $mwc::db onecolumn {{select item_id from items where text = '{text}' and diagram_id = :diagram}}");

    // The centre of an icon's text on the diagram canvas, from DRAKON's own primitive table. The example
    // diagrams sit right of the narrower portrait canvas, so the tests that click icons run in landscape.
    private async Task<LocatorPosition> IconAsync(string text)
    {
        var item = await ItemIdAsync(text);
        item.Should().NotBeEmpty();
        var centre = (await Fixture.TclAsync(
            $"set c $mw::canvas; set b [$c bbox [::mb onecolumn {{select ext_id from primitives where item_id = {item} and role = 'text'}}]]; " +
            "list [expr {[winfo rootx $c] + ([lindex $b 0] + [lindex $b 2]) / 2.0 - [$c canvasx 0]}] " +
            "[expr {[winfo rooty $c] + ([lindex $b 1] + [lindex $b 3]) / 2.0 - [$c canvasy 0]}]")).Split(' ').Select(Number).ToArray();
        return new() { X = centre[0], Y = centre[1] };
    }

    // The middle of a diagram's row in the list, found with the treeview's own row hit test.
    private async Task<LocatorPosition> DiagramRowAsync(string name)
    {
        var rows = (await Fixture.TclAsync(
            $"set t {DiagramList}; set hits {{}}; for {{set y 0}} {{$y < [winfo height $t]}} {{incr y}} {{ " +
            $"set id [$t identify row 20 $y]; if {{$id ne {{}} && [$t item $id -text] eq {{{name}}}}} {{ lappend hits $y }} }}; " +
            "list [winfo rootx $t] [winfo rooty $t] [lindex $hits 0] [lindex $hits end]")).Split(' ').Select(Number).ToArray();
        return new() { X = rows[0] + 40, Y = rows[1] + (rows[2] + rows[3]) / 2 };
    }

    private async Task EditIconTextAsync(string text, string typed)
    {
        await Page.GetByTestId("TkHost").ClickAsync(new() { Position = await IconAsync(text), ClickCount = 2 });
        await Fixture.WaitForTclAsync("focus", ".twindow.root.entry.text");
        await Page.Keyboard.TypeAsync(typed);
        await Fixture.WaitForTclAsync(".twindow.root.entry.text get 1.0 end-1c", text + typed);
        await ClickTkAsync(".twindow.root.ok");
        await Fixture.WaitForTclAsync("winfo exists .twindow", "0");
    }
}
