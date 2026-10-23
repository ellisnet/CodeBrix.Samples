using SilverAssertions;
using SimpleCbxVideoPlayer.SkiaVideo.Playback;
using SkiaSharp;
using Xunit;

namespace SimpleCbxVideoPlayer.SkiaVideo.Tests;

public class TransparencyBackdropTests
{
    [Fact]
    public void Draw_alternates_the_two_greys_square_by_square_from_the_area_corner()
    {
        //Arrange
        using SKBitmap bitmap = new SKBitmap(new SKImageInfo(200, 100, SKColorType.Bgra8888, SKAlphaType.Premul));
        using SKCanvas canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);
        var cell = (int)TransparencyBackdrop.CellSize;

        //Act
        TransparencyBackdrop.Draw(canvas, SKRect.Create(20, 10, 160, 80));
        canvas.Flush();

        //Assert
        bitmap.GetPixel(20 + 2, 10 + 2).Should().Be(TransparencyBackdrop.LightColor);
        bitmap.GetPixel(20 + cell + 2, 10 + 2).Should().Be(TransparencyBackdrop.DarkColor);
        bitmap.GetPixel(20 + 2, 10 + cell + 2).Should().Be(TransparencyBackdrop.DarkColor);
        bitmap.GetPixel(20 + cell + 2, 10 + cell + 2).Should().Be(TransparencyBackdrop.LightColor);
    }

    [Fact]
    public void Draw_stays_inside_the_area_and_leaves_the_letterbox_black()
    {
        //Arrange
        using SKBitmap bitmap = new SKBitmap(new SKImageInfo(200, 100, SKColorType.Bgra8888, SKAlphaType.Premul));
        using SKCanvas canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);

        //Act - an area whose width is not a whole number of squares, so the last column is clipped.
        TransparencyBackdrop.Draw(canvas, SKRect.Create(20, 10, 150, 75));
        canvas.Flush();

        //Assert
        bitmap.GetPixel(19, 50).Should().Be(SKColors.Black);
        bitmap.GetPixel(170, 50).Should().Be(SKColors.Black);
        bitmap.GetPixel(100, 9).Should().Be(SKColors.Black);
        bitmap.GetPixel(100, 85).Should().Be(SKColors.Black);
        bitmap.GetPixel(169, 84).Should().NotBe(SKColors.Black);
    }

    [Fact]
    public void Draw_of_an_empty_area_draws_nothing()
    {
        //Arrange
        using SKBitmap bitmap = new SKBitmap(new SKImageInfo(10, 10, SKColorType.Bgra8888, SKAlphaType.Premul));
        using SKCanvas canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);

        //Act
        TransparencyBackdrop.Draw(canvas, SKRect.Empty);
        TransparencyBackdrop.Draw(null, SKRect.Create(0, 0, 10, 10));
        canvas.Flush();

        //Assert
        bitmap.GetPixel(5, 5).Should().Be(SKColors.Black);
    }
}
