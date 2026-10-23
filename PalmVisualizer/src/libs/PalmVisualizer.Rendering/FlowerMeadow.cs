using System;
using System.Collections.Generic;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The flowers of "Flowers - WebGPU" and how they live, on dark pool-table green. With no palms
/// in view the meadow keeps <see cref="CalmFlowers"/> flowers: one sprouts, grows and blooms
/// slowly, then the next, and once four have bloomed each new flower starts the oldest
/// withering - brown, shrinking, gone just as the new one finishes. An open palm lights a soft
/// oval of the felt bright green - about a quarter of the view for one palm, almost all of it
/// for two (see <see cref="LightReach"/> and <see cref="LightAt"/>); flowers sprout
/// rapidly in the light and grow faster there, and nothing in the light withers - while
/// flowers left in the dark do. When the palms go, the light fades, the extra flowers all
/// wither slowly and the calm cycle of four returns. Every flower is one vivid variation of
/// red, orange, yellow, blue, indigo or violet - never green - and every flower, whatever its
/// size and shape, has exactly eight even petals (the shader draws them). Seeded, so a run is
/// repeatable. Call <see cref="Advance"/> once per frame, then <see cref="WriteInstances"/>.
/// </summary>
public sealed class FlowerMeadow
{
    /// <summary>The most flowers the meadow holds at once (the shader's uniform block holds this many).</summary>
    public const int MaxFlowers = 80;

    /// <summary>The floats <see cref="WriteInstances"/> writes per flower: six vec4s.</summary>
    public const int FloatsPerFlower = 24;

    /// <summary>How many flowers the meadow keeps when no palm is in view.</summary>
    public const int CalmFlowers = 4;

    /// <summary>Seconds for a flower to sprout and bloom fully in the dark (light speeds it up).</summary>
    public const float CalmBloomSeconds = 20f;

    /// <summary>Seconds for a flower to wither away in the dark.</summary>
    public const float WitherSeconds = 20f;

    /// <summary>The palm light below which, while palms are in view, a flower starts withering.</summary>
    public const float DarkThreshold = 0.10f;

    //The palm light's oval, its half-widths across and down in view heights: one palm alone
    //  lights about a quarter of the view; with two in view each lights much more, so two a
    //  little apart light almost all of it
    internal const float SingleReachAcross = 0.27f;
    internal const float SingleReachDown = 0.23f;
    internal const float PairReachAcross = 0.55f;
    internal const float PairReachDown = 0.65f;

    //Up to this much faster growth in full light
    private const float LightGrowthBoost = 6f;
    private const float PalmSproutsPerSecond = 2.2f;
    private const float LightRisePerSecond = 2f;
    private const float LightFallPerSecond = 0.8f;
    private const float RecoverPerSecond = 0.3f;

    //Beyond this a withering flower is all but gone, and the light no longer brings it back
    private const float RecoverableWither = 0.85f;

    //Vivid red, orange, yellow | blue, indigo, violet - a flower's colour leans only toward a
    //  neighbour in its own warm or cool half (or across violet-red), so it can never turn green
    private static readonly float[][] Rainbow =
    {
        new[] { 0.95f, 0.10f, 0.14f }, new[] { 1.00f, 0.48f, 0.05f }, new[] { 1.00f, 0.84f, 0.08f },
        new[] { 0.12f, 0.40f, 1.00f }, new[] { 0.32f, 0.20f, 0.96f }, new[] { 0.72f, 0.22f, 0.96f },
    };

    private static readonly int[][] Neighbours =
    {
        new[] { 1, 5 }, new[] { 0, 2 }, new[] { 1, 1 }, new[] { 4, 4 }, new[] { 3, 5 }, new[] { 4, 0 },
    };

    private sealed class Flower
    {
        public float X, Y, Size, StemRatio, R, G, B, Width, Tip, Cup, CenterSize, CenterKind, Rotation, Bend, Seed;
        public float WidestAt, PetalLength;
        public float Growth, Wither, Light, Age;
        public bool Withering;
    }

    private readonly Random _random;
    private readonly List<Flower> _flowers = new();
    private readonly float[] _palmSproutsDue = new float[PalmAttractorField.MaxAttractors];

    /// <summary>Initializes a new instance of the <see cref="FlowerMeadow"/> class: an empty
    /// meadow, whose first flower sprouts on the first frame.</summary>
    /// <param name="seed">The random seed; the same seed grows the same meadow.</param>
    public FlowerMeadow(int seed = 20261024)
    {
        _random = new Random(seed);
    }

    /// <summary>The number of flowers in the meadow, withering ones included.</summary>
    public int Count => _flowers.Count;

    /// <summary>A flower's state, for inspection: where its head is (0..1 across and down), how
    /// grown (0..1, blooming fully at 1), how withered (0..1), whether it is withering, how much
    /// palm light it stands in, and its colour.</summary>
    /// <param name="index">The flower, oldest first.</param>
    public (float X, float Y, float Growth, float Wither, bool Withering, float Light, float R, float G, float B) this[int index]
    {
        get
        {
            var f = _flowers[index];
            return (f.X, f.Y, f.Growth, f.Wither, f.Withering, f.Light, f.R, f.G, f.B);
        }
    }

    /// <summary>
    /// How far each palm's light reaches - the half-widths of its oval, across and down, in view
    /// heights. One palm alone lights about a quarter of the view; as a second palm comes into
    /// view, every palm's oval grows smoothly until two a little apart light almost all of it.
    /// The renderer hands these to the backdrop shader, so the felt is lit exactly as far.
    /// </summary>
    /// <param name="palms">The palms, <see cref="PalmMotion.Stride"/> floats each.</param>
    /// <returns>The oval's half-widths.</returns>
    public static (float Across, float Down) LightReach(ReadOnlySpan<float> palms)
    {
        float inView = 0f;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            inView += Math.Min(palms[(k * PalmMotion.Stride) + 2], 1f);
        }
        float pair = Math.Clamp(inView - 1f, 0f, 1f);
        return (SingleReachAcross + ((PairReachAcross - SingleReachAcross) * pair),
                SingleReachDown + ((PairReachDown - SingleReachDown) * pair));
    }

    /// <summary>
    /// The palm light at a point of the view, 0..1: a soft oval round each open palm, as far as
    /// <see cref="LightReach"/> says. The backdrop shader lights the felt with the same formula.
    /// </summary>
    /// <param name="x">The point across the view, 0..1.</param>
    /// <param name="y">The point down the view, 0..1.</param>
    /// <param name="palms">The palms, <see cref="PalmMotion.Stride"/> floats each.</param>
    /// <param name="aspect">The view's width over its height.</param>
    /// <returns>The light, 0 (dark) to 1 (fully lit).</returns>
    public static float LightAt(float x, float y, ReadOnlySpan<float> palms, float aspect)
    {
        var (across, down) = LightReach(palms);
        float light = 0f;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            int at = k * PalmMotion.Stride;
            float strength = palms[at + 2];
            if (strength <= 0.001f) { continue; }

            float dx = (x - palms[at]) * aspect / across;
            float dy = (y - palms[at + 1]) / down;
            light += strength * (float)Math.Exp(-((dx * dx) + (dy * dy)));
        }
        return Math.Min(light, 1f);
    }

    /// <summary>Grows the meadow by one frame.</summary>
    /// <param name="palms">The palms, already advanced to this frame.</param>
    /// <param name="aspect">The view's width over its height.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> is null.</exception>
    public void Advance(PalmMotion palms, float aspect)
    {
        if (palms == null) { throw new ArgumentNullException(nameof(palms)); }

        float delta = palms.DeltaTime;
        ReadOnlySpan<float> state = palms.Palms;
        bool palmsInView = false;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            if (state[(k * PalmMotion.Stride) + 2] > 0.05f) { palmsInView = true; }
        }

        // 1) Each flower: the light it stands in, growing (faster in the light), withering
        foreach (var f in _flowers)
        {
            float target = LightAt(f.X, f.Y, state, aspect);
            float rate = target > f.Light ? LightRisePerSecond : LightFallPerSecond;
            f.Light += (target - f.Light) * (1f - (float)Math.Exp(-rate * delta));
            f.Age += delta;

            if (palmsInView)
            {
                // In the dark beyond the palms' light a flower withers; in the light it thrives -
                // a withering flower recovers, unless it is all but gone
                if (!f.Withering && target < DarkThreshold) { f.Withering = true; }
                else if (f.Withering && target > 0.35f && f.Wither < RecoverableWither) { f.Withering = false; }
            }

            if (f.Withering)
            {
                f.Wither = Math.Min(1f, f.Wither + (delta / WitherSeconds));
            }
            else
            {
                f.Wither = Math.Max(0f, f.Wither - (delta * RecoverPerSecond));
                f.Growth = Math.Min(1f, f.Growth + (delta / CalmBloomSeconds * (1f + (LightGrowthBoost * f.Light))));
            }
        }
        _flowers.RemoveAll(f => f.Withering && f.Wither >= 1f);

        // 2) New flowers
        if (palmsInView)
        {
            // Lush: flowers sprout rapidly in each palm's light
            for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
            {
                int at = k * PalmMotion.Stride;
                float strength = state[at + 2];
                if (strength <= 0.05f)
                {
                    _palmSproutsDue[k] = 0f;
                    continue;
                }

                _palmSproutsDue[k] += delta * strength * PalmSproutsPerSecond;
                while (_palmSproutsDue[k] >= 1f)
                {
                    _palmSproutsDue[k] -= 1f;
                    SproutNear(state[at], state[at + 1], aspect, LightReach(state));
                }
            }
        }
        else
        {
            // Calm: beyond four flowers, the oldest wither; and once nothing is still growing,
            //  the next flower sprouts - which starts the oldest of the four withering
            WitherBeyond(CalmFlowers);
            bool growing = _flowers.Exists(f => !f.Withering && f.Growth < 1f);
            if (!growing)
            {
                SproutCalm(aspect);
                WitherBeyond(CalmFlowers);
            }
        }
    }

    /// <summary>
    /// Writes the flowers for the shader, those higher in the view first - so a flower lower in
    /// the view stands in front - <see cref="FloatsPerFlower"/> floats each: head x, y (pixels),
    /// head radius (pixels), petal rotation; colour r, g, b, stem length (pixels); bloom
    /// (0..1), stem growth (0..1), wither (0..1), fade; petal width, tip shape, cup, centre
    /// size; centre kind, stem bend, light, seed; where the petals are widest (0 near the base,
    /// 1 near the tip), petal length (relative to the head radius), and two unused.
    /// </summary>
    /// <param name="destination">At least <see cref="Count"/> * <see cref="FloatsPerFlower"/> floats.</param>
    /// <param name="width">The view's width in pixels.</param>
    /// <param name="height">The view's height in pixels.</param>
    /// <returns>The number of flowers written.</returns>
    public int WriteInstances(Span<float> destination, float width, float height)
    {
        int count = Math.Min(_flowers.Count, destination.Length / FloatsPerFlower);
        Span<int> order = stackalloc int[_flowers.Count];
        for (int i = 0; i < order.Length; i++) { order[i] = i; }
        for (int i = 1; i < order.Length; i++)
        {
            int moving = order[i];
            int j = i - 1;
            while (j >= 0 && _flowers[order[j]].Y > _flowers[moving].Y)
            {
                order[j + 1] = order[j];
                j--;
            }
            order[j + 1] = moving;
        }

        for (int i = 0; i < count; i++)
        {
            var f = _flowers[order[i]];
            float radius = f.Size * height;
            Span<float> to = destination.Slice(i * FloatsPerFlower, FloatsPerFlower);
            to[0] = f.X * width;
            to[1] = f.Y * height;
            to[2] = radius;
            to[3] = f.Rotation;
            to[4] = f.R;
            to[5] = f.G;
            to[6] = f.B;
            to[7] = radius * f.StemRatio;

            // The stem grows first, then the bud opens into the bloom
            to[8] = Math.Clamp((f.Growth - 0.35f) / 0.65f, 0f, 1f);
            to[9] = Math.Clamp(f.Growth / 0.35f, 0f, 1f);
            to[10] = f.Wither;
            to[11] = 1f - SmoothStep(0.75f, 1f, f.Wither);
            to[12] = f.Width;
            to[13] = f.Tip;
            to[14] = f.Cup;
            to[15] = f.CenterSize;
            to[16] = f.CenterKind;
            to[17] = f.Bend;
            to[18] = f.Light;
            to[19] = f.Seed;
            to[20] = f.WidestAt;
            to[21] = f.PetalLength;
            to[22] = 0f;
            to[23] = 0f;
        }
        return count;
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - (2f * t));
    }

    private float Next() => (float)_random.NextDouble();

    // Starts the oldest flowers withering until at most `keep` are not
    private void WitherBeyond(int keep)
    {
        int living = 0;
        foreach (var f in _flowers)
        {
            if (!f.Withering) { living++; }
        }
        foreach (var f in _flowers)
        {
            if (living <= keep) { break; }
            if (!f.Withering)
            {
                f.Withering = true;
                living--;
            }
        }
    }

    // Distance between two points of the view, in view heights
    private static float Distance(float x1, float y1, float x2, float y2, float aspect)
    {
        float dx = (x1 - x2) * aspect;
        float dy = y1 - y2;
        return (float)Math.Sqrt((dx * dx) + (dy * dy));
    }

    // How far a spot is from the nearest flower that is still alive and well
    private float Clearance(float x, float y, float aspect)
    {
        float nearest = float.MaxValue;
        foreach (var f in _flowers)
        {
            if (f.Withering && f.Wither > 0.6f) { continue; }
            nearest = Math.Min(nearest, Distance(x, y, f.X, f.Y, aspect) - f.Size);
        }
        return nearest;
    }

    // A calm flower goes where it is farthest from the others: random-looking, evenly spread
    private void SproutCalm(float aspect)
    {
        if (_flowers.Count >= MaxFlowers) { return; }

        float bestX = 0.5f, bestY = 0.4f, best = float.MinValue;
        for (int i = 0; i < 30; i++)
        {
            float x = 0.12f + (Next() * 0.76f);
            float y = 0.16f + (Next() * 0.50f);
            float clearance = Clearance(x, y, aspect);
            if (clearance > best)
            {
                best = clearance;
                bestX = x;
                bestY = y;
            }
        }
        _flowers.Add(NewFlower(bestX, bestY, 0.06f + (Next() * 0.045f)));
    }

    // A lush flower goes somewhere in the palm's light, where there is still a little room
    private void SproutNear(float palmX, float palmY, float aspect, (float Across, float Down) reach)
    {
        if (_flowers.Count >= MaxFlowers) { return; }

        float size = 0.035f + (Next() * 0.04f);
        float bestX = 0f, bestY = 0f, best = float.MinValue;
        for (int i = 0; i < 12; i++)
        {
            float x = Math.Clamp(palmX + (Gaussian() * reach.Across * 0.55f / aspect), 0.04f, 0.96f);
            float y = Math.Clamp(palmY + (Gaussian() * reach.Down * 0.45f), 0.06f, 0.90f);
            float clearance = Clearance(x, y, aspect);
            if (clearance > best)
            {
                best = clearance;
                bestX = x;
                bestY = y;
            }
        }

        // Too crowded already: the flowers there may overlap a little, but not pile up
        if (best < size * 0.7f) { return; }
        _flowers.Add(NewFlower(bestX, bestY, size));
    }

    private float Gaussian()
    {
        float u1 = Math.Max(Next(), 1e-6f);
        float u2 = Next();
        return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
    }

    private Flower NewFlower(float x, float y, float size)
    {
        // A rainbow colour (never green), leaning a little toward a neighbour in its own half,
        //  then brought to full brightness so every flower is vivid
        int hue = _random.Next(Rainbow.Length);
        int neighbour = Neighbours[hue][_random.Next(2)];
        float lean = Next() * 0.3f;
        float r = (Rainbow[hue][0] * (1f - lean)) + (Rainbow[neighbour][0] * lean);
        float g = (Rainbow[hue][1] * (1f - lean)) + (Rainbow[neighbour][1] * lean);
        float b = (Rainbow[hue][2] * (1f - lean)) + (Rainbow[neighbour][2] * lean);
        float bright = (0.92f + (Next() * 0.08f)) / Math.Max(r, Math.Max(g, b));

        return new Flower
        {
            X = x,
            Y = y,
            Size = size,
            StemRatio = 1.4f + (Next() * 0.9f),
            R = r * bright,
            G = g * bright,
            B = b * bright,
            Width = 0.10f + (MathF.Pow(Next(), 0.8f) * 0.35f),
            WidestAt = Next(),
            PetalLength = 0.72f + (Next() * 0.28f),
            Tip = Next(),
            Cup = Next(),
            CenterSize = 0.14f + (Next() * 0.14f),
            CenterKind = _random.Next(3),
            Rotation = Next() * MathF.PI / 4f,
            Bend = (Next() - 0.5f) * 2f,
            Seed = Next(),
        };
    }
}
