using System;
using System.Collections.Generic;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The crystals of "Crystals - WebGPU" and how they live: quartz points sprouting in clusters
/// at random places, fanning upward, each cluster in one colour of the palette; growing to
/// their full size, living a while, then fading away so new ones can grow - at most
/// <see cref="MaxCrystals"/> at once. Open palms excite the crystals near them, which then
/// grow faster and are drawn more vibrant, pulsing harder; and new crystals sprout where the
/// palms are, faster when they move. Seeded, so a run is repeatable. Call
/// <see cref="Advance"/> once per frame, then <see cref="WriteInstances"/>.
/// </summary>
public sealed class CrystalGarden
{
    /// <summary>The most crystals the garden holds at once (the shader's uniform block holds this many).</summary>
    public const int MaxCrystals = 96;

    /// <summary>The floats <see cref="WriteInstances"/> writes per crystal: three vec4s.</summary>
    public const int FloatsPerCrystal = 12;

    /// <summary>How far an open palm's excitement reaches, as a fraction of the view's height.</summary>
    public const float PalmReach = 0.2f;

    private const int MaxClusters = 14;

    //Straight up the view (in pixel space, where y grows downward); clusters lean a little off
    //  it and their crystals fan at most about 40 degrees either side, so every crystal grows upward
    private const float Upward = -MathF.PI / 2f;
    private const float CalmSproutsPerSecond = 1.4f;
    private const float ExcitementRisePerSecond = 3f;
    private const float ExcitementFallPerSecond = 0.7f;
    private const float FadeInPerSecond = 1.5f;
    private const float FadeOutPerSecond = 0.35f;

    private sealed class Crystal
    {
        public float X, Y, Angle, MaxLength, WidthRatio, Growth, GrowthRate, Hue, Excitement, Fade, Age, Life;
        public float PulsePhase, GlintPhase, Seed;
        public bool Dying;
    }

    private sealed class Cluster
    {
        public float X, Y, Direction, Spread, Hue, Age;
    }

    private readonly Random _random;
    private readonly List<Crystal> _crystals = new();
    private readonly List<Cluster> _clusters = new();
    private readonly float[] _palmSproutsDue = new float[PalmAttractorField.MaxAttractors];
    private float _sproutsDue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrystalGarden"/> class, already holding a
    /// few young crystals so the view never opens empty.
    /// </summary>
    /// <param name="seed">The random seed; the same seed grows the same garden.</param>
    public CrystalGarden(int seed = 20261023)
    {
        _random = new Random(seed);
        for (int i = 0; i < 20; i++)
        {
            var crystal = Sprout(null);
            if (crystal != null)
            {
                crystal.Growth = 0.15f + (Next() * 0.55f);
                crystal.Fade = 1f;
            }
        }
    }

    /// <summary>The number of crystals in the garden, fading ones included.</summary>
    public int Count => _crystals.Count;

    /// <summary>A crystal's state, for inspection: where it sits (0..1 across and down), how grown
    /// and how excited it is, its fade (0..1), and whether it is fading away.</summary>
    /// <param name="index">The crystal, oldest first.</param>
    public (float X, float Y, float Growth, float Excitement, float Fade, bool Dying) this[int index]
    {
        get
        {
            var c = _crystals[index];
            return (c.X, c.Y, c.Growth, c.Excitement, c.Fade, c.Dying);
        }
    }

    /// <summary>Grows the garden by one frame.</summary>
    /// <param name="palms">The palms, already advanced to this frame.</param>
    /// <param name="aspect">The view's width over its height, so a palm's reach is round on screen.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> is null.</exception>
    public void Advance(PalmMotion palms, float aspect)
    {
        if (palms == null) { throw new ArgumentNullException(nameof(palms)); }

        float delta = palms.DeltaTime;
        ReadOnlySpan<float> state = palms.Palms;

        // 1) New crystals: a calm trickle anywhere, and more right where the palms are
        _sproutsDue += delta * CalmSproutsPerSecond;
        while (_sproutsDue >= 1f)
        {
            _sproutsDue -= 1f;
            Sprout(null);
        }
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            int at = k * PalmMotion.Stride;
            float strength = state[at + 2];
            if (strength <= 0.05f)
            {
                _palmSproutsDue[k] = 0f;
                continue;
            }

            _palmSproutsDue[k] += delta * strength * (1.4f + (2f * Math.Min(state[at + 3], 1.5f)));
            while (_palmSproutsDue[k] >= 1f)
            {
                _palmSproutsDue[k] -= 1f;
                Sprout((state[at], state[at + 1]));
            }
        }

        // 2) Every crystal: excited by nearby palms, growing (faster when excited), ageing, fading
        float reachSq = PalmReach * PalmReach;
        foreach (var c in _crystals)
        {
            float target = 0f;
            for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
            {
                int at = k * PalmMotion.Stride;
                float strength = state[at + 2];
                if (strength <= 0.001f) { continue; }

                float dx = (c.X - state[at]) * aspect;
                float dy = c.Y - state[at + 1];
                target += strength * (float)Math.Exp(-((dx * dx) + (dy * dy)) / reachSq);
            }
            target = Math.Min(target, 1f);
            float rate = target > c.Excitement ? ExcitementRisePerSecond : ExcitementFallPerSecond;
            c.Excitement += (target - c.Excitement) * (1f - (float)Math.Exp(-rate * delta));

            c.Growth = Math.Min(1f, c.Growth + (delta * c.GrowthRate * (1f + (3f * c.Excitement))));
            c.Age += delta;
            if (!c.Dying && c.Growth >= 1f && c.Age > c.Life) { c.Dying = true; }
            c.Fade = c.Dying ? c.Fade - (delta * FadeOutPerSecond) : Math.Min(1f, c.Fade + (delta * FadeInPerSecond));
        }
        _crystals.RemoveAll(c => c.Dying && c.Fade <= 0f);

        foreach (var cluster in _clusters) { cluster.Age += delta; }
    }

    /// <summary>
    /// Writes the crystals for the shader, oldest first (so the youngest are drawn on top),
    /// <see cref="FloatsPerCrystal"/> floats each: base x, y (pixels), direction (radians, y
    /// down), length (pixels); half-width (pixels), hue, vibrance, fade; pulse phase, glint
    /// phase, seed, excitement.
    /// </summary>
    /// <param name="destination">At least <see cref="Count"/> * <see cref="FloatsPerCrystal"/> floats.</param>
    /// <param name="width">The view's width in pixels.</param>
    /// <param name="height">The view's height in pixels.</param>
    /// <returns>The number of crystals written.</returns>
    public int WriteInstances(Span<float> destination, float width, float height)
    {
        int count = Math.Min(_crystals.Count, destination.Length / FloatsPerCrystal);
        for (int i = 0; i < count; i++)
        {
            var c = _crystals[i];

            // Growth eases out: quick at first, settling into its full length
            float eased = 1f - ((1f - c.Growth) * (1f - c.Growth));
            float length = Math.Max(c.MaxLength * height * eased, 1f);
            float fade = c.Fade * c.Fade * (3f - (2f * c.Fade));

            Span<float> to = destination.Slice(i * FloatsPerCrystal, FloatsPerCrystal);
            to[0] = c.X * width;
            to[1] = c.Y * height;
            to[2] = c.Angle;
            to[3] = length;
            to[4] = Math.Max(length * c.WidthRatio * 0.5f, 1f);
            to[5] = c.Hue;
            to[6] = 0.80f + (0.20f * c.Excitement);
            to[7] = fade;
            to[8] = c.PulsePhase;
            to[9] = c.GlintPhase;
            to[10] = c.Seed;
            to[11] = c.Excitement;
        }
        return count;
    }

    private float Next() => (float)_random.NextDouble();

    // One of the palette's colours, exactly (the shader's palette puts colour i at hue i / 5):
    //  indigo, purple, magenta, mauve or orange - orange a little more often than the rest
    private float PickHue()
    {
        float pick = Next() * 5.4f;
        int color = pick < 1f ? 0 : pick < 2f ? 1 : pick < 3f ? 2 : pick < 3.8f ? 3 : 4;
        return color / 5f;
    }

    // Starts a crystal in a cluster - one near the palm, when there is a palm - or, when the
    // garden is full, starts its oldest crystal fading to make room instead
    private Crystal Sprout((float X, float Y)? nearPalm)
    {
        if (_crystals.Count >= MaxCrystals)
        {
            foreach (var old in _crystals)
            {
                if (!old.Dying)
                {
                    old.Dying = true;
                    break;
                }
            }
            return null;
        }

        Cluster cluster = null;
        if (nearPalm is { } palm)
        {
            foreach (var candidate in _clusters)
            {
                float dx = candidate.X - palm.X;
                float dy = candidate.Y - palm.Y;
                if ((dx * dx) + (dy * dy) < 0.08f * 0.08f && Next() < 0.7f)
                {
                    cluster = candidate;
                    break;
                }
            }
            cluster ??= NewCluster(palm.X + ((Next() - 0.5f) * 0.06f), palm.Y + ((Next() - 0.5f) * 0.06f));
        }
        else if (_clusters.Count > 0 && Next() < 0.75f)
        {
            var young = _clusters.FindAll(c => c.Age < 25f);
            cluster = young.Count > 0 ? young[_random.Next(young.Count)] : null;
        }
        cluster ??= NewCluster(0.06f + (Next() * 0.88f), 0.25f + (Next() * 0.70f));

        var crystal = new Crystal
        {
            X = Math.Clamp(cluster.X + ((Next() - 0.5f) * 0.03f), 0f, 1f),
            Y = Math.Clamp(cluster.Y + ((Next() - 0.5f) * 0.03f), 0f, 1f),
            Angle = cluster.Direction + ((Next() - 0.5f) * 2f * cluster.Spread),
            MaxLength = 0.08f + (0.16f * MathF.Pow(Next(), 1.5f)),
            WidthRatio = 0.22f + (Next() * 0.12f),
            GrowthRate = 0.06f + (Next() * 0.08f),
            Hue = cluster.Hue + ((Next() - 0.5f) * 0.06f),
            Life = 25f + (Next() * 30f),
            PulsePhase = Next(),
            GlintPhase = Next(),
            Seed = Next(),
        };
        _crystals.Add(crystal);
        return crystal;
    }

    private Cluster NewCluster(float x, float y)
    {
        if (_clusters.Count >= MaxClusters) { _clusters.RemoveAt(0); }

        var cluster = new Cluster
        {
            X = Math.Clamp(x, 0.03f, 0.97f),
            Y = Math.Clamp(y, 0.03f, 0.97f),
            Direction = Upward + ((Next() - 0.5f) * 0.3f),
            Spread = 0.35f + (Next() * 0.2f),
            Hue = PickHue(),
        };
        _clusters.Add(cluster);
        return cluster;
    }
}
