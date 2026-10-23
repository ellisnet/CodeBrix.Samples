using System;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The palm attractors as the Ripple and Holy Inanna visualizers read them, once per rendered
/// frame: each slot's smoothed position and strength, how fast the palm is moving, and one
/// eased <see cref="Activity"/> level for the whole scene - how much palm presence and movement
/// there is. Call <see cref="Advance"/> once per frame, on the thread that renders.
/// </summary>
public sealed class PalmMotion
{
    /// <summary>The floats per palm in <see cref="Palms"/>: x, y (0..1, y down), strength 0..1, and speed.</summary>
    public const int Stride = 4;

    //How quickly a palm's measured speed follows its movement, and how quickly the activity
    //  level eases toward its target, per second
    private const float SpeedFollowPerSecond = 8f;
    private const float ActivityRisePerSecond = 1.5f;
    private const float ActivityFallPerSecond = 0.6f;

    private readonly PalmAttractorField _field;
    private readonly float[] _state = new float[PalmAttractorField.MaxAttractors * 3];
    private readonly float[] _palms = new float[PalmAttractorField.MaxAttractors * Stride];
    private readonly float[] _lastX = new float[PalmAttractorField.MaxAttractors];
    private readonly float[] _lastY = new float[PalmAttractorField.MaxAttractors];
    private readonly bool[] _hadLast = new bool[PalmAttractorField.MaxAttractors];

    private bool _hasLastTime;
    private float _lastTime;

    /// <summary>Initializes a new instance of the <see cref="PalmMotion"/> class.</summary>
    /// <param name="field">The palm attractors to read; this class advances their smoothing.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="field"/> is null.</exception>
    public PalmMotion(PalmAttractorField field)
    {
        _field = field ?? throw new ArgumentNullException(nameof(field));
    }

    /// <summary>The visual's time, in seconds, as of the last <see cref="Advance"/>.</summary>
    public float Time { get; private set; }

    /// <summary>The seconds since the previous <see cref="Advance"/> (0 on the first; at most 0.1).</summary>
    public float DeltaTime { get; private set; }

    /// <summary>
    /// The palms, <see cref="Stride"/> floats each: position (0..1 across and down the view),
    /// strength 0..1 (0 for an unused slot), and speed in view sizes per second (smoothed).
    /// </summary>
    public ReadOnlySpan<float> Palms => _palms;

    /// <summary>
    /// How much is going on, 0..1: rises quickly with open palms and their movement, and falls
    /// slowly when they go away, so the visual calms down gracefully.
    /// </summary>
    public float Activity { get; private set; }

    /// <summary>Advances the palm smoothing to <paramref name="time"/> and measures the palms.</summary>
    /// <param name="time">The visual's clock, in seconds (it stops while the visual is paused).</param>
    public void Advance(float time)
    {
        //The frame's real delta (0 on the first frame; clamped so a stall cannot make things lurch)
        float delta = _hasLastTime ? Math.Clamp(time - _lastTime, 0f, 0.1f) : 0f;
        _lastTime = time;
        _hasLastTime = true;
        Time = time;
        DeltaTime = delta;

        _field.Step(delta);
        _field.CopyState(_state);

        float follow = 1f - (float)Math.Exp(-SpeedFollowPerSecond * delta);
        float presence = 0f;
        float movement = 0f;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            float x = _state[k * 3];
            float y = _state[(k * 3) + 1];
            float strength = _state[(k * 3) + 2];
            bool active = strength > 0.001f;

            float measured = 0f;
            if (active && _hadLast[k] && delta > 0f)
            {
                float dx = x - _lastX[k];
                float dy = y - _lastY[k];
                measured = (float)Math.Sqrt((dx * dx) + (dy * dy)) / delta;
            }

            int to = k * Stride;
            float speed = _palms[to + 3] + ((measured - _palms[to + 3]) * follow);
            _palms[to] = x;
            _palms[to + 1] = y;
            _palms[to + 2] = active ? strength : 0f;
            _palms[to + 3] = active ? speed : 0f;

            _lastX[k] = x;
            _lastY[k] = y;
            _hadLast[k] = active;

            presence += _palms[to + 2];
            movement += _palms[to + 2] * Math.Min(_palms[to + 3], 2f);
        }

        //One open palm is already lively; a second, or a palm in motion, takes it to full
        float target = Math.Clamp((0.65f * Math.Min(presence, 1.5f)) + (0.45f * movement), 0f, 1f);
        float rate = target > Activity ? ActivityRisePerSecond : ActivityFallPerSecond;
        Activity += (target - Activity) * (1f - (float)Math.Exp(-rate * delta));
    }
}
