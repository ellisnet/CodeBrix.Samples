using System;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The light rain that keeps the Ripple pool gently alive when no palm is stirring it: a few
/// small drops a second at random places, more and larger ones as the palm activity rises.
/// Seeded, so a run is repeatable. Call <see cref="Advance"/> once per frame and hand the
/// frame's <see cref="Drops"/> to the wave simulation.
/// </summary>
public sealed class RippleRain
{
    /// <summary>The most drops one frame can carry (the simulation's uniform block holds this many).</summary>
    public const int MaxDropsPerFrame = 4;

    /// <summary>The floats per drop in <see cref="Drops"/>: x, y (0..1, y down), depth, and radius (view heights).</summary>
    public const int Stride = 4;

    private const float CalmDropsPerSecond = 1.6f;
    private const float LivelyDropsPerSecond = 7f;

    private readonly Random _random = new(20261022);
    private readonly float[] _drops = new float[MaxDropsPerFrame * Stride];
    private float _due;

    /// <summary>The number of drops that fell this frame.</summary>
    public int Count { get; private set; }

    /// <summary>This frame's drops, <see cref="Stride"/> floats each (only the first <see cref="Count"/> are set).</summary>
    public ReadOnlySpan<float> Drops => _drops;

    /// <summary>Lets the rain fall for one frame.</summary>
    /// <param name="deltaSeconds">The seconds since the previous frame.</param>
    /// <param name="activity">The palm activity 0..1; livelier palms bring livelier rain.</param>
    public void Advance(float deltaSeconds, float activity)
    {
        activity = Math.Clamp(activity, 0f, 1f);
        _due += Math.Max(deltaSeconds, 0f) * (CalmDropsPerSecond + ((LivelyDropsPerSecond - CalmDropsPerSecond) * activity));

        Count = 0;
        while (_due >= 1f && Count < MaxDropsPerFrame)
        {
            _due -= 1f;
            int to = Count * Stride;
            _drops[to] = 0.04f + ((float)_random.NextDouble() * 0.92f);
            _drops[to + 1] = 0.04f + ((float)_random.NextDouble() * 0.92f);
            _drops[to + 2] = (0.18f + ((float)_random.NextDouble() * 0.20f)) * (1f + (1.5f * activity));
            _drops[to + 3] = 0.008f + ((float)_random.NextDouble() * 0.008f);
            Count++;
        }

        //A long stall must not save up a downpour
        _due = Math.Min(_due, 1f);
    }
}
