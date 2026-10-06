using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ToolTests(AppFixture fixture) : PintaTest(fixture)
{
    private Task<string> CurrentToolAsync() => EvaluateAsync(() => PintaCore.Tools.CurrentTool.Name);

    // Pan does nothing to the image, so clicking the canvas with it only gives the canvas the keyboard focus.
    private async Task FocusCanvasWithPanAsync()
    {
        await SelectToolAsync("Pan");
        await Canvas.ClickAsync(new() { Position = new() { X = 40, Y = 40 } });
        (await EvaluateAsync(() => FocusManager.GetFocusedElement(Fixture.View.XamlRoot) is PintaCanvas))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Unmodified_letter_selects_the_tool_that_claims_it()
    {
        await FocusCanvasWithPanAsync();
        await Page.Keyboard.PressAsync("p");
        await Expect(Tool("Pencil")).ToBeCheckedAsync();
        await Expect(Tool("Pan")).Not.ToBeCheckedAsync();
        (await CurrentToolAsync()).Should().Be("Pencil");
        await Page.Keyboard.PressAsync("b");
        await Expect(Tool("Paintbrush")).ToBeCheckedAsync();
        await Page.Keyboard.PressAsync("h");
        await Expect(Tool("Pan")).ToBeCheckedAsync();
        // Nothing was drawn on the way.
        await Expect(HistoryRows).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Pressing_the_same_letter_again_moves_to_the_next_tool_sharing_it()
    {
        await FocusCanvasWithPanAsync();
        await Page.Keyboard.PressAsync("s");
        await Expect(Tool("Rectangle Select")).ToBeCheckedAsync();
        await Page.Keyboard.PressAsync("s");
        await Expect(Tool("Ellipse Select")).ToBeCheckedAsync();
        await Page.Keyboard.PressAsync("s");
        await Expect(Tool("Lasso Select")).ToBeCheckedAsync();
        await Page.Keyboard.PressAsync("s");
        await Expect(Tool("Magic Wand Select")).ToBeCheckedAsync();
        await Page.Keyboard.PressAsync("s");
        await Expect(Tool("Rectangle Select")).ToBeCheckedAsync();
    }

    [Fact]
    public async Task Control_and_letter_does_not_select_a_tool()
    {
        await FocusCanvasWithPanAsync();
        await Page.Keyboard.PressAsync("Control+p");
        await Page.Keyboard.PressAsync("Control+b");
        (await CurrentToolAsync()).Should().Be("Pan");
        await Expect(Tool("Pan")).ToBeCheckedAsync();
        // The modifier was released: the same letter alone still works.
        await Page.Keyboard.PressAsync("p");
        await Expect(Tool("Pencil")).ToBeCheckedAsync();
    }

    [Fact]
    public async Task Typing_a_letter_into_a_text_box_does_not_select_a_tool()
    {
        await SelectToolAsync("Pan");
        // The status bar's zoom box is an editable combo box: typing goes to the text box inside it.
        var zoom = Page.GetByRole(AriaRole.Combobox, new() { Name = "Zoom", Exact = true });
        await zoom.PressSequentiallyAsync("p");
        (await EvaluateAsync(() => FocusManager.GetFocusedElement(Fixture.View.XamlRoot) is TextBox))
            .Should().BeTrue();
        (await EvaluateAsync(() => ((TextBox)FocusManager.GetFocusedElement(Fixture.View.XamlRoot)).Text))
            .Should().Contain("p");
        (await CurrentToolAsync()).Should().Be("Pan");
        await Expect(Tool("Pan")).ToBeCheckedAsync();
    }

    [Fact]
    public async Task Typing_a_letter_into_the_text_tool_does_not_select_a_tool()
    {
        await SelectToolAsync("Text");
        await Canvas.ClickAsync(new() { Position = new() { X = 100, Y = 100 } });
        await Page.Keyboard.TypeAsync("ps");
        await WaitAsync(() => ActiveDocument.Layers.CurrentUserLayer.TextEngine.ToString(), text => text == "ps", "the typed text");
        (await CurrentToolAsync()).Should().Be("Text");
        await Expect(Tool("Text")).ToBeCheckedAsync();
        // Once editing stops, the canvas keeps the focus and the letter is a shortcut again.
        await Page.Keyboard.PressAsync("Escape");
        await Page.Keyboard.PressAsync("p");
        await Expect(Tool("Pencil")).ToBeCheckedAsync();
    }

    [Fact]
    public async Task Paint_bucket_fills_the_clicked_region_with_the_primary_color()
    {
        await SelectToolAsync("Paint Bucket");
        await Canvas.ClickAsync(new() { Position = new() { X = 300, Y = 300 } });
        await Expect(HistoryRows).ToHaveCountAsync(2);
        IsBlack(await PixelAsync(10, 10)).Should().BeTrue();
        IsBlack(await PixelAsync(790, 590)).Should().BeTrue();
    }
}
