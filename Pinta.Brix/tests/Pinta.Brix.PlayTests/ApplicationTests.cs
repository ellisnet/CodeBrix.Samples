using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ApplicationTests(AppFixture fixture) : PintaTest(fixture)
{
    [Fact]
    public async Task Startup_shows_one_unsaved_800x600_tab_with_undo_disabled()
    {
        await Expect(Tabs).ToHaveCountAsync(1);
        await Expect(UnsavedTab()).ToBeVisibleAsync();
        await Expect(LayerRows).ToHaveCountAsync(1);
        await Expect(LayerRows).ToContainTextAsync("Background");
        await Expect(HistoryRows).ToHaveCountAsync(1);
        await Expect(HistoryRows).ToContainTextAsync("New Image");
        await Expect(ToolbarButton("Undo")).ToBeDisabledAsync();
        await Expect(HistoryPadButton("Redo")).ToBeDisabledAsync();
        await Expect(Canvas).ToBeVisibleAsync();
        (await EvaluateAsync(() => ActiveDocument.ImageSize)).Should().Be(new Size(800, 600));
        (await EvaluateAsync(() => ActiveDocument.IsDirty)).Should().BeFalse();
        IsWhite(await PixelAsync(400, 300)).Should().BeTrue();
        await MenuItem("Edit").ClickAsync();
        await Expect(MenuItem("Undo")).ToBeDisabledAsync();
        await Expect(MenuItem("Redo")).ToBeDisabledAsync();
        await Page.Keyboard.PressAsync("Escape");
    }

    [Fact]
    public async Task Pencil_drag_on_canvas_paints_pixels_and_adds_history_row()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Expect(HistoryRows.Nth(1)).ToContainTextAsync("Pencil");
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(150, 110)).Should().BeTrue();
        await Expect(UnsavedTab(dirty: true)).ToBeVisibleAsync();
        await Expect(ToolbarButton("Undo")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Undo_and_redo_menu_items_restore_pixels_and_history_pointer()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await MenuAsync("Edit", "Undo");
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 0, "the stroke undone");
        IsWhite(await PixelAsync(150, 100)).Should().BeTrue();
        // The undone row stays in the pad, ready for Redo.
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Expect(ToolbarButton("Undo")).ToBeDisabledAsync();
        await Expect(ToolbarButton("Redo")).ToBeEnabledAsync();
        await MenuAsync("Edit", "Redo");
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 1, "the stroke redone");
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        await Expect(ToolbarButton("Redo")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Control_z_and_control_y_undo_and_redo_from_canvas_focus()
    {
        await SelectToolAsync("Pencil");
        // The drag leaves the keyboard focus on the canvas, where the tool sees every key first.
        await DragOnImageAsync(100, 100, 200, 100);
        await Expect(HistoryRows).ToHaveCountAsync(2);
        await Page.Keyboard.PressAsync("Control+z");
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 0, "the stroke undone");
        IsWhite(await PixelAsync(150, 100)).Should().BeTrue();
        await Page.Keyboard.PressAsync("Control+y");
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 1, "the stroke redone");
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        await Page.Keyboard.PressAsync("Control+Shift+z");
        (await EvaluateAsync(() => ActiveDocument.History.Pointer)).Should().Be(1);
    }

    [Fact]
    public async Task Clicking_an_earlier_history_row_travels_back()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(100, 100, 200, 100);
        await DragOnImageAsync(100, 200, 200, 200);
        await Expect(HistoryRows).ToHaveCountAsync(3);
        await HistoryRows.Nth(1).ClickAsync();
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 1, "the second stroke undone");
        IsBlack(await PixelAsync(150, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(150, 200)).Should().BeTrue();
        await HistoryRows.Nth(0).ClickAsync();
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 0, "both strokes undone");
        IsWhite(await PixelAsync(150, 100)).Should().BeTrue();
        await HistoryRows.Nth(2).ClickAsync();
        await WaitAsync(() => ActiveDocument.History.Pointer, pointer => pointer == 2, "both strokes redone");
        IsBlack(await PixelAsync(150, 200)).Should().BeTrue();
        await Expect(HistoryRows).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task Window_menu_switches_between_documents()
    {
        await ToolbarButton("New").ClickAsync();
        await Expect(Tabs).ToHaveCountAsync(2);
        var names = await EvaluateAsync(() => PintaCore.Workspace.OpenDocuments.Select(document => document.DisplayName).ToArray());
        (await EvaluateAsync(() => PintaCore.Workspace.ActiveDocumentIndex)).Should().Be(1);
        await MenuAsync("Window", names[0]);
        await WaitAsync(() => PintaCore.Workspace.ActiveDocumentIndex, index => index == 0, "the first document active");
        await MenuAsync("Window", names[1]);
        await WaitAsync(() => PintaCore.Workspace.ActiveDocumentIndex, index => index == 1, "the second document active");
    }

    [Fact]
    public async Task Status_bar_follows_the_cursor_over_the_canvas()
    {
        var box = await Canvas.BoundingBoxAsync();
        await Page.Mouse.MoveAsync(box.X + 120.5f, box.Y + 45.5f);
        await Expect(Page.GetByTestId("CursorPosition")).ToHaveTextAsync("120, 45");
        await Page.Mouse.MoveAsync(box.X + 300.5f, box.Y + 200.5f);
        await Expect(Page.GetByTestId("CursorPosition")).ToHaveTextAsync("300, 200");
    }

    [Fact]
    public async Task New_screenshot_says_it_is_not_available_yet()
    {
        await MenuAsync("File", "New Screenshot...");
        await Expect(Page.GetByText("Not available yet", new() { Exact = true })).ToBeVisibleAsync();
        await AnswerAsync("not implemented in this port yet", "OK");
        await Expect(Tabs).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Keyboard_shortcuts_and_about_dialogs_open_and_close()
    {
        await MenuAsync("Help", "Keyboard Shortcuts");
        await Expect(Dialog("Paste Into New Layer").GetByText("Ctrl+Shift+V", new() { Exact = true })).ToBeVisibleAsync();
        await AnswerAsync("Paste Into New Layer", "Close");
        await MenuAsync("Help", "About");
        await Expect(Page.GetByText("Pinta is Copyright (c) Jonathan Pobst and contributors, MIT licensed.")).ToBeVisibleAsync();
        await AnswerAsync("A port of Pinta", "Close");
    }
}
