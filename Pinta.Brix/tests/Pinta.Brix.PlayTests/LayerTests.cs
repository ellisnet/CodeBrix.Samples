using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml.Controls;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class LayerTests(AppFixture fixture) : PintaTest(fixture)
{
    private Task<string[]> LayerNamesAsync() => EvaluateAsync(() => ActiveDocument.Layers.UserLayers.Select(layer => layer.Name).ToArray());
    private Task<int> SelectedRowAsync() => EvaluateAsync(() => ((ListView)Fixture.View.FindName("LayersList")).SelectedIndex);

    [Fact]
    public async Task Add_layer_pad_button_adds_and_selects_layer_row()
    {
        await LayersPadButton("Add New Layer").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(2);
        // The pad lists the topmost layer first, and the new layer goes on top.
        await Expect(LayerRows.Nth(0)).ToContainTextAsync("Layer 2");
        await Expect(LayerRows.Nth(1)).ToContainTextAsync("Background");
        (await SelectedRowAsync()).Should().Be(0);
        (await EvaluateAsync(() => ActiveDocument.Layers.CurrentUserLayerIndex)).Should().Be(1);
        await Expect(HistoryRows.Last).ToContainTextAsync("Add New Layer");
        await Expect(LayersPadButton("Delete Layer")).ToBeEnabledAsync();
        await LayerRows.Nth(1).ClickAsync();
        await WaitAsync(() => ActiveDocument.Layers.CurrentUserLayerIndex, index => index == 0, "the background layer selected");
    }

    [Fact]
    public async Task Delete_and_duplicate_layer_update_pad_and_are_undoable()
    {
        await Expect(LayersPadButton("Delete Layer")).ToBeDisabledAsync();
        await LayersPadButton("Duplicate Layer").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(2);
        await Expect(LayerRows.Nth(0)).ToContainTextAsync("Background copy");
        (await LayerNamesAsync()).Should().Equal("Background", "Background copy");
        await LayersPadButton("Delete Layer").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(1);
        (await LayerNamesAsync()).Should().Equal("Background");
        await Expect(HistoryRows).ToHaveCountAsync(3);
        await Expect(HistoryRows.Nth(1)).ToContainTextAsync("Duplicate Layer");
        await Expect(HistoryRows.Nth(2)).ToContainTextAsync("Delete Layer");
        await ToolbarButton("Undo").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(2);
        (await LayerNamesAsync()).Should().Equal("Background", "Background copy");
        await ToolbarButton("Undo").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(1);
        await ToolbarButton("Redo").ClickAsync();
        await Expect(LayerRows).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task Layer_properties_rename_applies_and_cancel_reverts()
    {
        await LayersPadButton("Layer Properties").ClickAsync();
        await Dialog("Layer Properties").GetByRole(AriaRole.Textbox).FillAsync("Sky");
        await AnswerAsync("Layer Properties", "OK");
        await Expect(LayerRows.Nth(0)).ToContainTextAsync("Sky");
        (await LayerNamesAsync()).Should().Equal("Sky");
        await Expect(HistoryRows.Last).ToContainTextAsync("Layer Properties");

        await LayersPadButton("Layer Properties").ClickAsync();
        await Dialog("Layer Properties").GetByRole(AriaRole.Textbox).FillAsync("Ground");
        // The rename is live while the dialog is open...
        await WaitAsync(() => ActiveDocument.Layers.CurrentUserLayer.Name, name => name == "Ground", "the live rename");
        await AnswerAsync("Layer Properties", "Cancel");
        // ...and Cancel puts it back without a history entry.
        (await LayerNamesAsync()).Should().Equal("Sky");
        await Expect(LayerRows.Nth(0)).ToContainTextAsync("Sky");
        await Expect(HistoryRows).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task Import_from_file_adds_a_layer_named_after_the_file()
    {
        var path = WritePng("red.png", 64, 48, SKColors.Red);
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await MenuAsync("Layers", "Import from File...");
        await Expect(LayerRows).ToHaveCountAsync(2);
        await Expect(LayerRows.Nth(0)).ToContainTextAsync("red.png");
        await Expect(HistoryRows.Last).ToContainTextAsync("Import From File");
        var red = await PixelAsync(10, 10, layer: 1);
        (red.R, red.G, red.B, red.A).Should().Be(((byte)255, (byte)0, (byte)0, (byte)255));
        IsTransparent(await PixelAsync(100, 100, layer: 1)).Should().BeTrue();
    }

    [Fact]
    public async Task Layer_visibility_check_box_hides_the_layer()
    {
        await LayerRows.Nth(0).GetByRole(AriaRole.Checkbox).UncheckAsync();
        await WaitAsync(() => ActiveDocument.Layers.UserLayers[0].Hidden, hidden => hidden, "the hidden layer");
        await LayerRows.Nth(0).GetByRole(AriaRole.Checkbox).CheckAsync();
        await WaitAsync(() => ActiveDocument.Layers.UserLayers[0].Hidden, hidden => !hidden, "the visible layer");
    }
}
