using System.Threading.Tasks;
using Pinta.Brix.Engine;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ImageTests(AppFixture fixture) : PintaTest(fixture)
{
    [Fact]
    public async Task Resize_image_dialog_keeps_the_aspect_ratio()
    {
        await MenuAsync("Image", "Resize Image...");
        await Expect(Page.GetByText("Resize Image", new() { Exact = true })).ToBeVisibleAsync();
        await SetNumberAsync("Maintain aspect ratio", 0, "400");
        await Expect(NumberBox("Maintain aspect ratio", 1)).ToHaveValueAsync("300");
        await AnswerAsync("Maintain aspect ratio", "OK");
        await WaitAsync(() => ActiveDocument.ImageSize, size => size == new Size(400, 300), "the resized image");
        await Expect(HistoryRows.Last).ToContainTextAsync("Resize Image");
    }

    [Fact]
    public async Task Resize_canvas_dialog_grows_the_canvas_around_the_image()
    {
        await MenuAsync("Image", "Resize Canvas...");
        await SetNumberAsync("Anchor:", 0, "1000");
        await SetNumberAsync("Anchor:", 1, "700");
        await AnswerAsync("Anchor:", "OK");
        await WaitAsync(() => ActiveDocument.ImageSize, size => size == new Size(1000, 700), "the larger canvas");
        await Expect(HistoryRows.Last).ToContainTextAsync("Resize Canvas");
        // Centred: the new border is transparent, the old image sits in the middle.
        IsTransparent(await PixelAsync(10, 10)).Should().BeTrue();
        IsWhite(await PixelAsync(500, 350)).Should().BeTrue();
    }

    [Fact]
    public async Task Resize_dialog_cancel_leaves_the_image_alone()
    {
        await MenuAsync("Image", "Resize Image...");
        await SetNumberAsync("Maintain aspect ratio", 0, "200");
        await AnswerAsync("Maintain aspect ratio", "Cancel");
        (await EvaluateAsync(() => ActiveDocument.ImageSize)).Should().Be(new Size(800, 600));
        await Expect(HistoryRows).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Flip_and_rotate_move_the_pixels()
    {
        await SelectToolAsync("Pencil");
        await DragOnImageAsync(10, 100, 50, 100);
        await MenuAsync("Image", "Flip Horizontal");
        await Expect(HistoryRows.Last).ToContainTextAsync("Flip Image Horizontal");
        IsBlack(await PixelAsync(799 - 30, 100)).Should().BeTrue();
        IsWhite(await PixelAsync(30, 100)).Should().BeTrue();
        await MenuAsync("Image", "Rotate 90° Clockwise");
        await WaitAsync(() => ActiveDocument.ImageSize, size => size == new Size(600, 800), "the rotated image");
        await MenuAsync("Image", "Rotate 180°");
        await Expect(HistoryRows).ToHaveCountAsync(5);
        (await EvaluateAsync(() => ActiveDocument.ImageSize)).Should().Be(new Size(600, 800));
    }
}
