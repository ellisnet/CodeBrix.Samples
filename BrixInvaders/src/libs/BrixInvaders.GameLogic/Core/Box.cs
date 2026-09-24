namespace BrixInvaders.GameLogic;

/// <summary>
/// An axis-aligned rectangle in world units, described by its centre and half extents.
/// World units: x grows to the right, y grows downwards, origin at the top-left of the playfield.
/// </summary>
public readonly struct Box
{
    /// <summary>Creates a box from its centre and half extents.</summary>
    /// <param name="centerX">Centre x in world units.</param>
    /// <param name="centerY">Centre y in world units.</param>
    /// <param name="halfWidth">Half of the width (must be zero or positive).</param>
    /// <param name="halfHeight">Half of the height (must be zero or positive).</param>
    public Box(double centerX, double centerY, double halfWidth, double halfHeight)
    {
        CenterX = centerX;
        CenterY = centerY;
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
    }

    /// <summary>Creates a box from its centre and full size.</summary>
    /// <param name="centerX">Centre x in world units.</param>
    /// <param name="centerY">Centre y in world units.</param>
    /// <param name="width">Full width.</param>
    /// <param name="height">Full height.</param>
    /// <returns>The box.</returns>
    public static Box FromCenter(double centerX, double centerY, double width, double height)
        => new Box(centerX, centerY, width / 2.0, height / 2.0);

    /// <summary>Centre x.</summary>
    public double CenterX { get; }

    /// <summary>Centre y.</summary>
    public double CenterY { get; }

    /// <summary>Half of the width.</summary>
    public double HalfWidth { get; }

    /// <summary>Half of the height.</summary>
    public double HalfHeight { get; }

    /// <summary>Left edge.</summary>
    public double Left => CenterX - HalfWidth;

    /// <summary>Right edge.</summary>
    public double Right => CenterX + HalfWidth;

    /// <summary>Top edge.</summary>
    public double Top => CenterY - HalfHeight;

    /// <summary>Bottom edge.</summary>
    public double Bottom => CenterY + HalfHeight;

    /// <summary>Full width.</summary>
    public double Width => HalfWidth * 2.0;

    /// <summary>Full height.</summary>
    public double Height => HalfHeight * 2.0;

    /// <summary>
    /// True when the two boxes overlap with a positive area. Boxes that only touch along an edge do not intersect.
    /// </summary>
    /// <param name="other">The other box.</param>
    /// <returns>True when they overlap.</returns>
    public bool Intersects(Box other)
        => Left < other.Right && other.Left < Right && Top < other.Bottom && other.Top < Bottom;

    /// <summary>True when this box lies completely inside <paramref name="outer"/> (edges may touch).</summary>
    /// <param name="outer">The containing box.</param>
    /// <returns>True when contained.</returns>
    public bool IsInside(Box outer)
        => Left >= outer.Left && Right <= outer.Right && Top >= outer.Top && Bottom <= outer.Bottom;

    /// <inheritdoc />
    public override string ToString() => $"Box(cx={CenterX}, cy={CenterY}, w={Width}, h={Height})";
}
