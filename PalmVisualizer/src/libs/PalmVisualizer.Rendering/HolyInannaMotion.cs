using System;

namespace PalmVisualizer.Rendering;

/// <summary>
/// How the Star of Inanna moves in "Holy Inanna - WebGPU": it drifts on a slow path of its own,
/// spins slowly and tilts gently enough to show its depth. The palms can only ever influence it
/// - never disturb it: their pull draws it at most <see cref="MaxPalmPull"/> off its own path
/// and is eased in over seconds, and their activity brings it closer (larger) or lets it
/// recede (smaller) just as slowly. Call <see cref="Advance"/> once per frame.
/// </summary>
public sealed class HolyInannaMotion
{
    /// <summary>The farthest the palms can draw the star from its own path, as a fraction of the view.</summary>
    public const float MaxPalmPull = 0.12f;

    /// <summary>The star's size with no palm activity (it has receded).</summary>
    public const float CalmScale = 0.82f;

    /// <summary>The star's size at full palm activity (it has come close).</summary>
    public const float ActiveScale = 1.3f;

    //Seconds for the star to cover ~63% of the way to where the palms would have it
    private const float PositionEaseSeconds = 1.4f;
    private const float ScaleEaseSeconds = 1.8f;

    //Radians per second: a slow turn that quickens only slightly with the palms
    private const float CalmSpinPerSecond = 0.22f;
    private const float ActiveSpinPerSecond = 0.38f;

    private bool _placed;

    /// <summary>The star's centre across the view, 0..1.</summary>
    public float CenterX { get; private set; } = 0.5f;

    /// <summary>The star's centre down the view, 0..1.</summary>
    public float CenterY { get; private set; } = 0.5f;

    /// <summary>The star's size, from <see cref="CalmScale"/> to <see cref="ActiveScale"/>.</summary>
    public float Scale { get; private set; } = CalmScale;

    /// <summary>The star's turn about its own centre, in radians (0..2π).</summary>
    public float Spin { get; private set; }

    /// <summary>The star's gentle tilt about the view's horizontal axis, in radians.</summary>
    public float TiltX { get; private set; }

    /// <summary>The star's gentle tilt about the view's vertical axis, in radians.</summary>
    public float TiltY { get; private set; }

    /// <summary>Where the star's own path has it now, before the palms' influence (0..1 across, down).</summary>
    public (float X, float Y) PathPosition { get; private set; } = (0.5f, 0.5f);

    /// <summary>Moves the star on by one frame.</summary>
    /// <param name="palms">The palms, already advanced to this frame.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> is null.</exception>
    public void Advance(PalmMotion palms)
    {
        if (palms == null) { throw new ArgumentNullException(nameof(palms)); }

        float t = palms.Time;
        float delta = palms.DeltaTime;

        // The star's own slow, wandering path
        float pathX = 0.5f + (0.17f * (float)Math.Sin(t * 0.11f)) + (0.05f * (float)Math.Sin((t * 0.29f) + 1.3f));
        float pathY = 0.5f + (0.11f * (float)Math.Sin((t * 0.13f) + 0.7f)) + (0.04f * (float)Math.Sin((t * 0.37f) + 2.1f));
        PathPosition = (pathX, pathY);

        // The palms draw it a little way toward where they are - never far
        float weight = 0f, palmX = 0f, palmY = 0f;
        ReadOnlySpan<float> state = palms.Palms;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            float strength = state[(k * PalmMotion.Stride) + 2];
            weight += strength;
            palmX += state[k * PalmMotion.Stride] * strength;
            palmY += state[(k * PalmMotion.Stride) + 1] * strength;
        }

        float targetX = pathX;
        float targetY = pathY;
        if (weight > 0.001f)
        {
            float pull = Math.Min(weight, 1f);
            targetX += Math.Clamp((palmX / weight) - pathX, -MaxPalmPull, MaxPalmPull) * pull;
            targetY += Math.Clamp((palmY / weight) - pathY, -MaxPalmPull, MaxPalmPull) * pull;
        }

        if (!_placed)
        {
            CenterX = targetX;
            CenterY = targetY;
            _placed = true;
        }
        else
        {
            float ease = 1f - (float)Math.Exp(-delta / PositionEaseSeconds);
            CenterX += (targetX - CenterX) * ease;
            CenterY += (targetY - CenterY) * ease;
        }

        // Closer with the palms' activity, receding when they rest
        float activity = Math.Clamp(palms.Activity, 0f, 1f);
        float targetScale = CalmScale + ((ActiveScale - CalmScale) * activity);
        Scale += (targetScale - Scale) * (1f - (float)Math.Exp(-delta / ScaleEaseSeconds));

        Spin = (Spin + (delta * (CalmSpinPerSecond + ((ActiveSpinPerSecond - CalmSpinPerSecond) * activity)))) % (2f * MathF.PI);
        TiltX = 0.36f * (float)Math.Sin(t * 0.23f);
        TiltY = 0.30f * (float)Math.Sin((t * 0.17f) + 0.9f);
    }
}
