using System;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// The ONE world-to-screen transform: the fixed 1280 x 720 logical playfield scaled uniformly into a target
/// rectangle and centred, with the rest letterboxed (bars top and bottom, or left and right).
/// </summary>
/// <remarks>
/// The game pins the engine's render resolution to the playfield size, so in practice the transform is the
/// identity; everything still goes through it, so a different render size (or a head that ignores the pin) only
/// changes the numbers here.
/// </remarks>
public readonly struct PlayfieldTransform
{
    private PlayfieldTransform(double targetWidth, double targetHeight, double scale, double offsetX, double offsetY)
    {
        TargetWidth = targetWidth;
        TargetHeight = targetHeight;
        Scale = scale;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>The identity transform (a target exactly the playfield's size).</summary>
    public static PlayfieldTransform Identity => Fit(Playfield.Width, Playfield.Height);

    /// <summary>The width of the target rectangle.</summary>
    public double TargetWidth { get; }

    /// <summary>The height of the target rectangle.</summary>
    public double TargetHeight { get; }

    /// <summary>Screen units per world unit.</summary>
    public double Scale { get; }

    /// <summary>The left letterbox bar width (screen units).</summary>
    public double OffsetX { get; }

    /// <summary>The top letterbox bar height (screen units).</summary>
    public double OffsetY { get; }

    /// <summary>The playfield's width on screen.</summary>
    public double ContentWidth => Playfield.Width * Scale;

    /// <summary>The playfield's height on screen.</summary>
    public double ContentHeight => Playfield.Height * Scale;

    /// <summary>Fits the playfield into a target of the given size.</summary>
    /// <param name="targetWidth">The target width (screen units).</param>
    /// <param name="targetHeight">The target height (screen units).</param>
    /// <returns>The transform.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When a size is not positive and finite.</exception>
    public static PlayfieldTransform Fit(double targetWidth, double targetHeight)
    {
        if (!(targetWidth > 0) || !(targetHeight > 0) || double.IsInfinity(targetWidth) || double.IsInfinity(targetHeight))
        {
            throw new ArgumentOutOfRangeException(nameof(targetWidth), $"The target must be positive: {targetWidth} x {targetHeight}.");
        }

        var scale = Math.Min(targetWidth / Playfield.Width, targetHeight / Playfield.Height);
        var offsetX = (targetWidth - (Playfield.Width * scale)) / 2.0;
        var offsetY = (targetHeight - (Playfield.Height * scale)) / 2.0;
        return new PlayfieldTransform(targetWidth, targetHeight, scale, offsetX, offsetY);
    }

    /// <summary>Maps a world X to the screen.</summary>
    /// <param name="worldX">World X.</param>
    /// <returns>Screen X.</returns>
    public double ToScreenX(double worldX) => OffsetX + (worldX * Scale);

    /// <summary>Maps a world Y to the screen.</summary>
    /// <param name="worldY">World Y.</param>
    /// <returns>Screen Y.</returns>
    public double ToScreenY(double worldY) => OffsetY + (worldY * Scale);

    /// <summary>Maps a screen X back to the world.</summary>
    /// <param name="screenX">Screen X.</param>
    /// <returns>World X.</returns>
    public double ToWorldX(double screenX) => (screenX - OffsetX) / Scale;

    /// <summary>Maps a screen Y back to the world.</summary>
    /// <param name="screenY">Screen Y.</param>
    /// <returns>World Y.</returns>
    public double ToWorldY(double screenY) => (screenY - OffsetY) / Scale;

    /// <summary>Whether a screen point falls on the playfield (not on a letterbox bar).</summary>
    /// <param name="screenX">Screen X.</param>
    /// <param name="screenY">Screen Y.</param>
    /// <returns>Whether the point is inside the playfield.</returns>
    public bool IsOnPlayfield(double screenX, double screenY)
    {
        var x = ToWorldX(screenX);
        var y = ToWorldY(screenY);
        return x >= 0 && x <= Playfield.Width && y >= 0 && y <= Playfield.Height;
    }
}
