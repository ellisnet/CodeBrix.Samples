using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace KenneyAssetBrowser.Rendering.Tests;

public class ImageCanvasPainterTests
{
    [Fact]
    public void Small_image_stays_at_natural_size_and_centers()
    {
        //Arrange
        using var bitmap = new SKBitmap(new SKImageInfo(100, 50, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var painter = new ImageCanvasPainter { Bitmap = bitmap };

        //Act
        var rect = painter.GetImageRect(200, 200);

        //Assert
        rect.Left.Should().Be(50f);
        rect.Top.Should().Be(75f);
        rect.Width.Should().Be(100f);
        rect.Height.Should().Be(50f);
    }

    [Fact]
    public void Large_image_scales_down_to_fit()
    {
        //Arrange
        using var bitmap = new SKBitmap(new SKImageInfo(400, 200, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var painter = new ImageCanvasPainter { Bitmap = bitmap };

        //Act
        var rect = painter.GetImageRect(200, 200);

        //Assert
        rect.Left.Should().Be(0f);
        rect.Top.Should().Be(50f);
        rect.Width.Should().Be(200f);
        rect.Height.Should().Be(100f);
    }

    [Fact]
    public void Zoom_factor_multiplies_the_fit_scale()
    {
        //Arrange
        using var bitmap = new SKBitmap(new SKImageInfo(100, 100, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var painter = new ImageCanvasPainter { Bitmap = bitmap, ZoomFactor = 2f };

        //Act
        var rect = painter.GetImageRect(100, 100);

        //Assert
        rect.Width.Should().Be(200f);
        rect.Left.Should().Be(-50f);
    }

    [Fact]
    public void Canvas_points_map_back_to_image_pixels()
    {
        //Arrange — 100x50 image centered on a 200x200 canvas at natural size
        using var bitmap = new SKBitmap(new SKImageInfo(100, 50, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var painter = new ImageCanvasPainter { Bitmap = bitmap };

        //Act
        var center = painter.CanvasToImage(new SKPoint(100, 100), 200, 200);
        var outside = painter.CanvasToImage(new SKPoint(5, 5), 200, 200);

        //Assert
        center.HasValue.Should().Be(true);
        center!.Value.X.Should().Be(50);
        center.Value.Y.Should().Be(25);
        outside.HasValue.Should().Be(false);
    }

    [Fact]
    public void Checkerboard_shows_through_transparent_pixels()
    {
        //Arrange — a fully transparent image
        using var bitmap = new SKBitmap(new SKImageInfo(10, 10, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(SKColors.Transparent);
        var painter = new ImageCanvasPainter { Bitmap = bitmap };

        //Act — image paints at natural size centered at (35,35)..(45,45)
        using var rendered = Render(painter, 80, 80);

        //Assert — the first checker tile is the dark square
        var pixel = rendered.GetPixel(36, 36);
        pixel.Red.Should().Be(0x2A);
        pixel.Green.Should().Be(0x2F);
        pixel.Blue.Should().Be(0x39);
    }

    [Fact]
    public void Highlight_dims_outside_the_region_and_keeps_the_region_bright()
    {
        //Arrange — an opaque white 20x20 image with the top-left quadrant spotlighted
        using var bitmap = new SKBitmap(new SKImageInfo(20, 20, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(SKColors.White);
        var painter = new ImageCanvasPainter
        {
            Bitmap = bitmap,
            HighlightRegion = new SKRectI(0, 0, 10, 10),
        };

        //Act — natural size centered at (30,30)..(50,50) on an 80x80 canvas
        using var rendered = Render(painter, 80, 80);

        //Assert
        var inside = rendered.GetPixel(34, 34);
        var outsideRegion = rendered.GetPixel(46, 46);
        inside.Red.Should().Be(0xFF);
        ((int)outsideRegion.Red).Should().BeLessThan(200);
        ((int)outsideRegion.Red).Should().BeGreaterThan(0);
    }

    private static SKBitmap Render(ImageCanvasPainter painter, int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        painter.Paint(surface.Canvas, width, height);
        using var snapshot = surface.Snapshot();
        return SKBitmap.FromImage(snapshot);
    }
}
