using System;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// Three parallax star layers scrolling down the playfield: far dots, middle dots and a few near Kenney star
/// sprites. Every position is a pure function of time, so the field never needs updating, only drawing.
/// </summary>
public sealed class StarField
{
    private const int FarCount = 110;
    private const int MiddleCount = 55;
    private const int NearCount = 14;

    private readonly Star[] _stars;

    /// <summary>Creates a field.</summary>
    /// <param name="seed">The layout seed.</param>
    public StarField(int seed = 1977)
    {
        var random = new Random(seed);
        _stars = new Star[FarCount + MiddleCount + NearCount];
        for (var i = 0; i < _stars.Length; i++)
        {
            var layer = i < FarCount ? 0 : i < FarCount + MiddleCount ? 1 : 2;
            _stars[i] = new Star(
                random.NextDouble() * Playfield.Width,
                random.NextDouble() * Playfield.Height,
                layer,
                random.NextDouble(),
                random.Next(3));
        }
    }

    /// <summary>The scroll speeds of the three layers, world units per second at a speed factor of 1.</summary>
    public static readonly double[] LayerSpeeds = { 14.0, 38.0, 90.0 };

    /// <summary>Adds the stars to the world list.</summary>
    /// <param name="frame">The frame being built.</param>
    /// <param name="time">Seconds of scrolling.</param>
    /// <param name="speedFactor">A multiplier on every layer's speed (warp effects, the title's slow drift).</param>
    public void Paint(FrameBuilder frame, double time, double speedFactor = 1.0)
    {
        ArgumentNullException.ThrowIfNull(frame);
        foreach (var star in _stars)
        {
            var y = Wrap(star.Y + (time * LayerSpeeds[star.Layer] * speedFactor), Playfield.Height + 20) - 10;
            var twinkle = 0.65 + (0.35 * Math.Sin((time * (1.5 + (star.Phase * 2))) + (star.Phase * 10)));
            switch (star.Layer)
            {
                case 0:
                    frame.AddWorld(DrawCommand.Dot(star.X, y, 1.0, 0xFF9DB4D8, 0.55 * twinkle));
                    break;
                case 1:
                    frame.AddWorld(DrawCommand.Dot(star.X, y, 1.6, 0xFFE6EEFF, 0.8 * twinkle));
                    break;
                default:
                    frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Star(star.Kind), star.X, y, 16, 16, time * 20 * (star.Phase - 0.5), twinkle));
                    break;
            }
        }
    }

    private static double Wrap(double value, double period) => ((value % period) + period) % period;

    private readonly struct Star
    {
        public Star(double x, double y, int layer, double phase, int kind)
        {
            X = x;
            Y = y;
            Layer = layer;
            Phase = phase;
            Kind = kind;
        }

        public double X { get; }

        public double Y { get; }

        public int Layer { get; }

        public double Phase { get; }

        public int Kind { get; }
    }
}
