using CodeBrix.Imaging;
using CodeBrix.Imaging.Drawing;
using CodeBrix.Imaging.Drawing.Models;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace WebcamPainter.Painting;

/// <summary>
/// The Paint Mode drawing model: a captured webcam still as the background of a
/// <see cref="CodeBrix.Imaging.Drawing.DrawingSession"/> with one translucent highlighter
/// layer per <see cref="HighlighterPalette"/> color. Strokes are driven in NORMALIZED image
/// coordinates (0..1 across the captured still), so the hand-tracking pipeline can paint
/// without knowing anything about view sizes, aspect-fit letterboxing, or DPI.
/// </summary>
public sealed class PaintingSession : IDisposable
{
    /// <summary>
    /// The brush radius, in calibrated drawing units (the drawing space's long side is 1000).
    /// This is THE knob for how wide the frosting-spatula stroke feels - tweak freely.
    /// </summary>
    public const float BrushRadius = 30f;

    private DrawingSession _session;
    private SKBitmap _background;

    private PaintingSession(DrawingSession session, SKBitmap background)
    {
        _session = session;
        _background = background;

        foreach (HighlighterColor color in HighlighterPalette.Colors)
        {
            _session.AddLayer(color.Name, color.Color);
        }
        ActiveColorName = HighlighterPalette.Colors[0].Name;
    }

    /// <summary>
    /// Creates a painting session over a captured webcam still.
    /// </summary>
    /// <param name="bgraPixels">The captured still's tightly packed 32-bit BGRA pixels.</param>
    /// <param name="width">The still's width in pixels.</param>
    /// <param name="height">The still's height in pixels.</param>
    /// <param name="mirrorHorizontally">
    /// <c>true</c> to flip the still left-to-right so it reads like a mirror - matching the
    /// mirrored live preview the user was looking at when they took the photo.
    /// </param>
    /// <returns>A new painting session; dispose it when leaving Paint Mode.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bgraPixels"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a dimension is less than 1.</exception>
    public static PaintingSession Create(byte[] bgraPixels, int width, int height, bool mirrorHorizontally)
    {
        if (bgraPixels == null) { throw new ArgumentNullException(nameof(bgraPixels)); }
        if (width < 1) { throw new ArgumentOutOfRangeException(nameof(width)); }
        if (height < 1) { throw new ArgumentOutOfRangeException(nameof(height)); }

        var decoded = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        Marshal.Copy(bgraPixels, 0, decoded.GetPixels(), width * height * 4);

        SKBitmap background = decoded;
        if (mirrorHorizontally)
        {
            background = new SKBitmap(decoded.Info);
            using (var canvas = new SKCanvas(background))
            {
                canvas.Scale(-1, 1, width / 2f, 0);
                canvas.DrawBitmap(decoded, new SKPoint(0, 0), new SKSamplingOptions(SKFilterMode.Nearest));
            }
            decoded.Dispose();
        }

        DrawingSession session = DrawingSession.CreateForImage(
            background,
            CalibrationSizing.DeriveFromBackgroundImage,
            new DrawingSessionOptions
            {
                BackgroundFillColor = Color.White,   //JPEG has no alpha - keep the fill opaque
                SurfaceClearColor = Color.Black,     //letterbox bars around the still
                StrokeWidth = BrushRadius * 2f,
            });
        return new PaintingSession(session, background);
    }

    /// <summary>
    /// The underlying drawing session - the hosting page renders it in its paint handler,
    /// and the view model observes its RedrawRequested / DrawingChanged events.
    /// </summary>
    public DrawingSession Session => _session;

    /// <summary>The name of the highlighter color that new strokes paint with.</summary>
    public string ActiveColorName { get; private set; }

    /// <summary>Indicates whether any completed stroke exists.</summary>
    public bool HasStrokes => _session.HasStrokes;

    /// <summary>The total number of completed strokes.</summary>
    public int StrokeCount => _session.StrokeCount;

    /// <summary>Indicates whether a stroke is currently in progress (the "spatula" is down).</summary>
    public bool IsStrokeActive => _session.IsPointerActive;

    /// <summary>
    /// Switches the active highlighter color.
    /// </summary>
    /// <param name="colorName">One of the <see cref="HighlighterPalette"/> color names.</param>
    /// <returns><c>true</c> when the color was found and made active.</returns>
    public bool SelectColor(string colorName)
    {
        DrawingLayer layer = _session.GetLayer(colorName);
        if (layer == null) { return false; }

        _session.ActiveLayer = layer;
        ActiveColorName = layer.Name;
        return true;
    }

    #region | Normalized-coordinate stroke input |

    /// <summary>
    /// Begins a stroke at the given normalized still-image position. No-op (returning
    /// <c>false</c>) until the session has rendered at least once.
    /// </summary>
    /// <param name="normX">Horizontal position across the still, 0..1.</param>
    /// <param name="normY">Vertical position down the still, 0..1.</param>
    /// <param name="viewWidth">The hosting canvas's logical width.</param>
    /// <param name="viewHeight">The hosting canvas's logical height.</param>
    /// <returns><c>true</c> when a stroke was started.</returns>
    public bool BeginStroke(float normX, float normY, float viewWidth, float viewHeight)
        => _session.PointerPressed(NormalizedToView(normX, normY, viewWidth, viewHeight),
            new SizeF(viewWidth, viewHeight));

    /// <summary>
    /// Extends the in-progress stroke to the given normalized still-image position.
    /// </summary>
    /// <param name="normX">Horizontal position across the still, 0..1.</param>
    /// <param name="normY">Vertical position down the still, 0..1.</param>
    /// <param name="viewWidth">The hosting canvas's logical width.</param>
    /// <param name="viewHeight">The hosting canvas's logical height.</param>
    /// <returns><c>true</c> when the stroke was extended.</returns>
    public bool ContinueStroke(float normX, float normY, float viewWidth, float viewHeight)
        => _session.PointerMoved(NormalizedToView(normX, normY, viewWidth, viewHeight),
            new SizeF(viewWidth, viewHeight));

    /// <summary>Completes and commits the in-progress stroke ("spatula" lifted).</summary>
    /// <returns><c>true</c> when a stroke was committed.</returns>
    public bool EndStroke() => _session.PointerReleased();

    /// <summary>Discards the in-progress stroke without committing it.</summary>
    public void CancelStroke() => _session.PointerCanceled();

    /// <summary>
    /// Maps a normalized still-image position (0..1 across the captured photo) to a point in
    /// the hosting canvas's coordinates, accounting for the centered aspect-fit letterboxing
    /// that the drawing renderer applies. Also used to place the hand-position crosshair.
    /// </summary>
    /// <param name="normX">Horizontal position across the still, 0..1.</param>
    /// <param name="normY">Vertical position down the still, 0..1.</param>
    /// <param name="viewWidth">The hosting canvas's width.</param>
    /// <param name="viewHeight">The hosting canvas's height.</param>
    /// <returns>The equivalent point in canvas coordinates.</returns>
    public PointF NormalizedToView(float normX, float normY, float viewWidth, float viewHeight)
    {
        RectangleF fit = GetImageRectInView(viewWidth, viewHeight);
        return new PointF(fit.X + (normX * fit.Width), fit.Y + (normY * fit.Height));
    }

    /// <summary>
    /// The centered aspect-fit rectangle that the captured still occupies within a canvas of
    /// the given size (the same mapping the drawing renderer uses).
    /// </summary>
    /// <param name="viewWidth">The hosting canvas's width.</param>
    /// <param name="viewHeight">The hosting canvas's height.</param>
    /// <returns>The still's display rectangle in canvas coordinates.</returns>
    public RectangleF GetImageRectInView(float viewWidth, float viewHeight)
    {
        Size calibration = _session.CalibrationSize;
        if (viewWidth <= 0 || viewHeight <= 0 || calibration.Width <= 0 || calibration.Height <= 0)
        {
            return new RectangleF(0, 0, 0, 0);
        }

        float scale = Math.Min(viewWidth / calibration.Width, viewHeight / calibration.Height);
        float width = calibration.Width * scale;
        float height = calibration.Height * scale;
        return new RectangleF((viewWidth - width) / 2f, (viewHeight - height) / 2f, width, height);
    }

    /// <summary>
    /// The brush radius in canvas coordinates for a canvas of the given size - for drawing
    /// a brush-sized cursor ring that matches what a stroke will actually cover.
    /// </summary>
    /// <param name="viewWidth">The hosting canvas's width.</param>
    /// <param name="viewHeight">The hosting canvas's height.</param>
    /// <returns>The brush radius, scaled to canvas coordinates.</returns>
    public float GetBrushRadiusInView(float viewWidth, float viewHeight)
    {
        Size calibration = _session.CalibrationSize;
        if (calibration.Width <= 0) { return 0f; }

        RectangleF fit = GetImageRectInView(viewWidth, viewHeight);
        return BrushRadius * (fit.Width / calibration.Width);
    }

    #endregion

    /// <summary>Removes all strokes from every color layer.</summary>
    public void Clear() => _session.Clear();

    /// <summary>
    /// Renders the painted still (at the captured photo's native resolution) to JPEG bytes.
    /// </summary>
    /// <param name="quality">The JPEG quality, 1-100.</param>
    /// <returns>The JPEG-encoded image bytes.</returns>
    public byte[] ExportJpeg(int quality = 90) => _session.ExportJpeg(quality);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_session != null)
        {
            _session.Dispose();
            _session = null;
        }
        if (_background != null)
        {
            _background.Dispose();
            _background = null;
        }
    }
}
