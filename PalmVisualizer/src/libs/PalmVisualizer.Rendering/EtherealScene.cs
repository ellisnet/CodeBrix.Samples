using System;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The Ethereal visual's state for one frame, independent of the graphics API that draws it:
/// the palm attractors as the plasma shader reads them, and every star's position, size and
/// brightness. Both renderers - <see cref="EtherealSkiaRenderer"/> (Ethereal - OpenGL) and
/// <see cref="EtherealWebGpuRenderer"/> (Ethereal - WebGPU) - draw from the same scene, which
/// is what makes the two versions the same visual. Call <see cref="Advance"/> once per
/// rendered frame, on the thread that renders, then read the spans.
/// </summary>
/// <remarks>
/// The plasma itself is computed entirely in the shaders; this class supplies their
/// uniforms. The starfield is computed here: base positions derived from time - faster stars
/// are brighter and drift further per second, wrapping across the width - and then each palm
/// pulls nearby stars toward it (and swells and brightens them a little), so the stars
/// visibly chase an open hand and drift free when it closes.
/// </remarks>
public sealed class EtherealScene
{
    /// <summary>The number of stars in the starfield.</summary>
    public const int StarCount = 220;

    /// <summary>The floats per palm in <see cref="Palms"/>: shader-space x, y, and strength.</summary>
    public const int PalmStride = 3;

    /// <summary>The floats per star in <see cref="Stars"/>: center x, center y, radius (pixels), and alpha 0..1.</summary>
    public const int StarStride = 4;

    //How far (as a fraction of the shorter edge) a palm's pull reaches the stars
    private const float StarPullRadiusFraction = 0.38f;

    private readonly PalmAttractorField _attractorField;

    //Reused per-frame buffers - Advance allocates nothing
    private readonly float[] _fieldState = new float[PalmAttractorField.MaxAttractors * 3];
    private readonly float[] _palms = new float[PalmAttractorField.MaxAttractors * PalmStride];
    private readonly float[] _stars = new float[StarCount * StarStride];

    //Star seeds fixed at construction so the undisturbed frame is a pure function of time
    private readonly float[] _starSeedX = new float[StarCount];
    private readonly float[] _starSeedY = new float[StarCount];
    private readonly float[] _starSpeed = new float[StarCount];
    private readonly float[] _starSize = new float[StarCount];

    private bool _hasLastStepTime;
    private float _lastStepTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealScene"/> class.
    /// </summary>
    /// <param name="attractorField">The palm attractors that bend the visual.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="attractorField"/> is null.</exception>
    public EtherealScene(PalmAttractorField attractorField)
    {
        _attractorField = attractorField ?? throw new ArgumentNullException(nameof(attractorField));

        var rng = new Random(20260720);
        for (int i = 0; i < StarCount; i++)
        {
            _starSeedX[i] = (float)rng.NextDouble();
            _starSeedY[i] = (float)rng.NextDouble();
            _starSpeed[i] = 0.02f + (float)rng.NextDouble() * 0.12f;   // fraction of width per second
            _starSize[i] = 0.75f + (float)(rng.NextDouble() * rng.NextDouble()) * 2.25f;
        }
    }

    /// <summary>The visual's time, in seconds, as of the last <see cref="Advance"/>.</summary>
    public float Time { get; private set; }

    /// <summary>The surface width, in pixels, as of the last <see cref="Advance"/>.</summary>
    public float Width { get; private set; }

    /// <summary>The surface height, in pixels, as of the last <see cref="Advance"/>.</summary>
    public float Height { get; private set; }

    /// <summary>
    /// The palms as the plasma shader reads them, <see cref="PalmStride"/> floats each: x and
    /// y in the shader's aspect-corrected -1..1 space (y grows downward), then the attractor
    /// strength 0..1 (0 for an unused slot).
    /// </summary>
    public ReadOnlySpan<float> Palms => _palms;

    /// <summary>
    /// Every star, <see cref="StarStride"/> floats each: the center in pixels (origin top-left),
    /// the radius in pixels, and the alpha 0..1 of its white disc.
    /// </summary>
    public ReadOnlySpan<float> Stars => _stars;

    /// <summary>
    /// Advances the palm smoothing to <paramref name="time"/> and computes the frame for a
    /// surface of the given pixel size.
    /// </summary>
    /// <param name="time">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="width">The surface width in pixels.</param>
    /// <param name="height">The surface height in pixels.</param>
    /// <param name="pixelScale">Pixels per layout unit (the display scale), so the stars keep
    /// the same size on screen whatever the display density.</param>
    public void Advance(float time, float width, float height, float pixelScale)
    {
        Time = time;
        Width = width;
        Height = height;
        if (width <= 0f || height <= 0f) { return; }

        //Advance the attractor smoothing by the frame's real delta (0 on the first frame;
        //  clamped so a stall cannot make the field lurch)
        float delta = _hasLastStepTime ? Math.Clamp(time - _lastStepTime, 0f, 0.1f) : 0f;
        _lastStepTime = time;
        _hasLastStepTime = true;
        _attractorField.Step(delta);
        _attractorField.CopyState(_fieldState);

        // 1) The palms for the plasma shader, converted into its aspect-corrected -1..1 space
        float aspect = width / height;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            _palms[k * PalmStride] = ((_fieldState[k * 3] * 2f) - 1f) * aspect;
            _palms[(k * PalmStride) + 1] = (_fieldState[(k * 3) + 1] * 2f) - 1f;
            _palms[(k * PalmStride) + 2] = _fieldState[(k * 3) + 2];
        }

        // 2) The starfield, pulled toward the palms
        float scale = pixelScale > 0f ? pixelScale : 1f;
        float pullRadius = StarPullRadiusFraction * Math.Min(width, height);
        float pullRadiusSq = pullRadius * pullRadius;
        for (int i = 0; i < StarCount; i++)
        {
            float x = (_starSeedX[i] + time * _starSpeed[i]) % 1f * width;
            float y = _starSeedY[i] * height;

            var pull = 0f;
            for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
            {
                float strength = _fieldState[(k * 3) + 2];
                if (strength <= 0.001f) { continue; }

                float palmX = _fieldState[k * 3] * width;
                float palmY = _fieldState[(k * 3) + 1] * height;
                float dx = palmX - x;
                float dy = palmY - y;
                float weight = strength * (float)Math.Exp(-((dx * dx) + (dy * dy)) / pullRadiusSq);
                x += dx * weight * 0.5f;
                y += dy * weight * 0.5f;
                pull += weight;
            }

            float alpha = 90f + (_starSpeed[i] / 0.14f) * 165f + (pull * 140f);
            _stars[i * StarStride] = x;
            _stars[(i * StarStride) + 1] = y;
            _stars[(i * StarStride) + 2] = _starSize[i] * (1f + (pull * 0.6f)) * scale;
            _stars[(i * StarStride) + 3] = Math.Min(255f, alpha) / 255f;
        }
    }
}
