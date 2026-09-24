namespace BrixInvaders.Game.Rendering;

/// <summary>
/// One immediate-mode drawing instruction in WORLD units (the 1280 x 720 playfield). Built on the engine thread each
/// frame and drawn by <see cref="CommandCanvas"/>, so it holds only values (no engine or Skia objects).
/// </summary>
public readonly struct DrawCommand
{
    private DrawCommand(DrawKind kind, string image, float x, float y, float width, float height, float rotation,
        float alpha, uint color, uint strokeColor, float strokeWidth, string text, float fontSize, TextAnchor anchor,
        bool thin, float cornerRadius)
    {
        Kind = kind;
        Image = image;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Rotation = rotation;
        Alpha = alpha;
        Color = color;
        StrokeColor = strokeColor;
        StrokeWidth = strokeWidth;
        Text = text;
        FontSize = fontSize;
        Anchor = anchor;
        Thin = thin;
        CornerRadius = cornerRadius;
    }

    /// <summary>What to draw.</summary>
    public DrawKind Kind { get; }

    /// <summary>The image key (see <see cref="SpriteCatalog"/>), for <see cref="DrawKind.Image"/>.</summary>
    public string Image { get; }

    /// <summary>Centre X (images, rectangles, circles) or anchor X (text).</summary>
    public float X { get; }

    /// <summary>Centre Y (images, rectangles, circles) or baseline-centre Y (text: the middle of the line).</summary>
    public float Y { get; }

    /// <summary>The box width (images fit inside it keeping their aspect) or the circle radius.</summary>
    public float Width { get; }

    /// <summary>The box height.</summary>
    public float Height { get; }

    /// <summary>Clockwise rotation in degrees about the centre (images).</summary>
    public float Rotation { get; }

    /// <summary>Opacity, 0..1.</summary>
    public float Alpha { get; }

    /// <summary>Fill or text colour, ARGB.</summary>
    public uint Color { get; }

    /// <summary>Outline colour, ARGB (0 = none).</summary>
    public uint StrokeColor { get; }

    /// <summary>Outline width.</summary>
    public float StrokeWidth { get; }

    /// <summary>The text, for <see cref="DrawKind.Text"/>.</summary>
    public string Text { get; }

    /// <summary>The font size in world units.</summary>
    public float FontSize { get; }

    /// <summary>Where X sits on a text line.</summary>
    public TextAnchor Anchor { get; }

    /// <summary>Whether text uses the thin face of the font.</summary>
    public bool Thin { get; }

    /// <summary>Rectangle corner radius.</summary>
    public float CornerRadius { get; }

    /// <summary>An image fitted (aspect kept) into a box centred on a point.</summary>
    /// <param name="image">The image key.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="width">Box width.</param>
    /// <param name="height">Box height.</param>
    /// <param name="rotation">Clockwise degrees.</param>
    /// <param name="alpha">Opacity, 0..1.</param>
    /// <returns>The command.</returns>
    public static DrawCommand Sprite(string image, double x, double y, double width, double height, double rotation = 0,
        double alpha = 1) =>
        new DrawCommand(DrawKind.Image, image, (float)x, (float)y, (float)width, (float)height, (float)rotation,
            Clamp01(alpha), 0xFFFFFFFF, 0, 0, null, 0, TextAnchor.Center, false, 0);

    /// <summary>A rectangle centred on a point.</summary>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="fill">Fill colour, ARGB (0 = no fill).</param>
    /// <param name="stroke">Outline colour, ARGB (0 = none).</param>
    /// <param name="strokeWidth">Outline width.</param>
    /// <param name="cornerRadius">Corner radius.</param>
    /// <param name="alpha">Opacity, 0..1.</param>
    /// <returns>The command.</returns>
    public static DrawCommand Rect(double x, double y, double width, double height, uint fill, uint stroke = 0,
        double strokeWidth = 0, double cornerRadius = 0, double alpha = 1) =>
        new DrawCommand(DrawKind.Rect, null, (float)x, (float)y, (float)width, (float)height, 0, Clamp01(alpha), fill,
            stroke, (float)strokeWidth, null, 0, TextAnchor.Center, false, (float)cornerRadius);

    /// <summary>A line of text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="x">Anchor X.</param>
    /// <param name="y">The middle of the line.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="color">Colour, ARGB.</param>
    /// <param name="anchor">Where X sits on the line.</param>
    /// <param name="thin">Whether to use the thin face.</param>
    /// <param name="alpha">Opacity, 0..1.</param>
    /// <returns>The command.</returns>
    public static DrawCommand Label(string text, double x, double y, double fontSize, uint color,
        TextAnchor anchor = TextAnchor.Center, bool thin = false, double alpha = 1) =>
        new DrawCommand(DrawKind.Text, null, (float)x, (float)y, 0, 0, 0, Clamp01(alpha), color, 0, 0, text ?? string.Empty,
            (float)fontSize, anchor, thin, 0);

    /// <summary>A filled circle.</summary>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="color">Colour, ARGB.</param>
    /// <param name="alpha">Opacity, 0..1.</param>
    /// <returns>The command.</returns>
    public static DrawCommand Dot(double x, double y, double radius, uint color, double alpha = 1) =>
        new DrawCommand(DrawKind.Circle, null, (float)x, (float)y, (float)radius, (float)radius, 0, Clamp01(alpha), color,
            0, 0, null, 0, TextAnchor.Center, false, 0);

    private static float Clamp01(double value) => value < 0 ? 0f : value > 1 ? 1f : (float)value;
}
