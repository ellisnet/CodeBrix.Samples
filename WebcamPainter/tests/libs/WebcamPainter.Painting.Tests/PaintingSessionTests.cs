using SilverAssertions;
using SkiaSharp;
using System;
using Xunit;

namespace WebcamPainter.Painting.Tests;

public class PaintingSessionTests
{
    private const int PhotoWidth = 64;
    private const int PhotoHeight = 48;

    //Left half red, right half blue (tightly packed BGRA) - asymmetric so mirroring is testable
    private static byte[] CreateTestPixels()
    {
        var pixels = new byte[PhotoWidth * PhotoHeight * 4];
        for (int y = 0; y < PhotoHeight; y++)
        {
            for (int x = 0; x < PhotoWidth; x++)
            {
                int offset = ((y * PhotoWidth) + x) * 4;
                if (x < PhotoWidth / 2)
                {
                    pixels[offset + 2] = 255;   //red
                }
                else
                {
                    pixels[offset] = 255;       //blue
                }
                pixels[offset + 3] = 255;
            }
        }
        return pixels;
    }

    private static PaintingSession CreateSession(bool mirror = false)
        => PaintingSession.Create(CreateTestPixels(), PhotoWidth, PhotoHeight, mirror);

    private static void RenderOnce(PaintingSession session, int width = 400, int height = 300)
    {
        var info = new SKImageInfo(width, height);
        using SKSurface surface = SKSurface.Create(info);
        session.Session.Render(surface, info);
    }

    [Fact]
    public void Create_adds_one_layer_per_palette_color()
    {
        //Arrange / Act
        using PaintingSession session = CreateSession();

        //Assert
        session.Session.Layers.Count.Should().Be(HighlighterPalette.Colors.Count);
        session.ActiveColorName.Should().Be(HighlighterPalette.Colors[0].Name);
    }

    [Fact]
    public void Create_throws_on_null_pixels()
        => Assert.Throws<ArgumentNullException>(() => PaintingSession.Create(null, 10, 10, false));

    [Fact]
    public void SelectColor_switches_the_active_layer()
    {
        //Arrange
        using PaintingSession session = CreateSession();

        //Act
        bool selected = session.SelectColor("Green");

        //Assert
        selected.Should().Be(true);
        session.ActiveColorName.Should().Be("Green");
        session.Session.ActiveLayer.Name.Should().Be("Green");
    }

    [Fact]
    public void SelectColor_returns_false_for_unknown_color()
    {
        //Arrange
        using PaintingSession session = CreateSession();

        //Act / Assert
        session.SelectColor("Chartreuse").Should().Be(false);
        session.ActiveColorName.Should().Be(HighlighterPalette.Colors[0].Name);
    }

    [Fact]
    public void Stroke_lifecycle_commits_a_stroke()
    {
        //Arrange
        using PaintingSession session = CreateSession();
        RenderOnce(session);

        //Act
        bool began = session.BeginStroke(0.3f, 0.4f);
        bool continued = session.ContinueStroke(0.6f, 0.5f);
        bool committed = session.EndStroke();

        //Assert
        began.Should().Be(true);
        continued.Should().Be(true);
        committed.Should().Be(true);
        session.HasStrokes.Should().Be(true);
        session.StrokeCount.Should().Be(1);
    }

    [Fact]
    public void BeginStroke_works_before_first_render()
    {
        //Arrange
        using PaintingSession session = CreateSession();

        //Act / Assert - normalized strokes use the drawing space calibrated from the photo,
        //  so they work with no view size and no prior render
        session.BeginStroke(0.5f, 0.5f).Should().Be(true);
        session.IsStrokeActive.Should().Be(true);
    }

    [Fact]
    public void CancelStroke_discards_without_committing()
    {
        //Arrange
        using PaintingSession session = CreateSession();
        RenderOnce(session);
        session.BeginStroke(0.5f, 0.5f);

        //Act
        session.CancelStroke();

        //Assert
        session.IsStrokeActive.Should().Be(false);
        session.HasStrokes.Should().Be(false);
    }

    [Fact]
    public void Clear_removes_all_strokes()
    {
        //Arrange
        using PaintingSession session = CreateSession();
        RenderOnce(session);
        session.BeginStroke(0.5f, 0.5f);
        session.EndStroke();

        //Act
        session.Clear();

        //Assert
        session.HasStrokes.Should().Be(false);
    }

    [Fact]
    public void ExportJpeg_returns_jpeg_at_the_photos_native_resolution()
    {
        //Arrange
        using PaintingSession session = CreateSession();

        //Act
        byte[] jpeg = session.ExportJpeg();

        //Assert - JPEG SOI marker, and the export matches the captured photo's pixel size
        jpeg[0].Should().Be((byte)0xFF);
        jpeg[1].Should().Be((byte)0xD8);
        using SKBitmap decoded = SKBitmap.Decode(jpeg);
        decoded.Width.Should().Be(PhotoWidth);
        decoded.Height.Should().Be(PhotoHeight);
    }

    [Fact]
    public void Mirrored_session_flips_the_background_horizontally()
    {
        //Arrange - the unmirrored test image is red on the left, blue on the right
        using PaintingSession session = CreateSession(mirror: true);

        //Act
        byte[] jpeg = session.ExportJpeg(quality: 100);

        //Assert - mirrored, the LEFT side must now be blue
        using SKBitmap decoded = SKBitmap.Decode(jpeg);
        SKColor left = decoded.GetPixel(4, PhotoHeight / 2);
        SKColor right = decoded.GetPixel(PhotoWidth - 4, PhotoHeight / 2);
        (left.Blue > 200).Should().Be(true);
        (left.Red < 80).Should().Be(true);
        (right.Red > 200).Should().Be(true);
        (right.Blue < 80).Should().Be(true);
    }

    [Fact]
    public void NormalizedToView_maps_through_the_aspect_fit_rectangle()
    {
        //Arrange - a 64x48 (4:3) photo in a 300x300 square view letterboxes to 300x225,
        //  leaving 37.5 above and below
        using PaintingSession session = CreateSession();

        //Act
        CodeBrix.Imaging.PointF topLeft = session.NormalizedToView(0f, 0f, 300, 300);
        CodeBrix.Imaging.PointF center = session.NormalizedToView(0.5f, 0.5f, 300, 300);

        //Assert
        (Math.Abs(topLeft.X - 0f) < 0.001f).Should().Be(true);
        (Math.Abs(topLeft.Y - 37.5f) < 0.001f).Should().Be(true);
        (Math.Abs(center.X - 150f) < 0.001f).Should().Be(true);
        (Math.Abs(center.Y - 150f) < 0.001f).Should().Be(true);
    }

    [Fact]
    public void GetBrushRadiusInView_scales_with_the_view_size()
    {
        //Arrange - the 4:3 photo's calibration space is 1000x750, so a 500-wide fit is half scale
        using PaintingSession session = CreateSession();

        //Act
        float radius = session.GetBrushRadiusInView(500, 375);

        //Assert
        radius.Should().Be(PaintingSession.BrushRadius / 2f);
    }
}
