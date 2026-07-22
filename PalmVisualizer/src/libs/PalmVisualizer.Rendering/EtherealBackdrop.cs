using System;
using System.Drawing;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Drawing.Direct;
using CodeBrix.Platform.GameEngine.Rendering;
using CodeBrix.Platform.GameEngine.Rendering.Backbuffers;
using CodeBrix.Platform.GameEngine.Rendering.Views;
using CodeBrix.Platform.GameEngine.SkiaSharp;
using SkiaSharp;

namespace PalmVisualizer.Rendering;

/// <summary>
/// A custom view-mode direct drawing that fills its bounds with an animated SkSL plasma
/// (<see cref="SKRuntimeEffect"/>) under a drifting starfield - the same resolution-
/// independent shader scene as the engine's GpuRender sample, except that this one listens
/// to a <see cref="PalmAttractorField"/>: wherever an open palm is held up, the plasma's
/// coordinate field is bent toward it (a soft gravitational pull plus a slow vortex), rings
/// of color drift inward toward it, the colors brighten around it, and nearby stars are
/// drawn toward it - so the whole visual appears to chase the hand. When every attractor
/// strength is zero the shader reduces EXACTLY to its undisturbed form, which is how the
/// visual melts back to its normal motion when the palms close. Because it is an ordinary
/// <see cref="DirectDrawingBase"/>, on the GpuRendering-OpenGL (GPU) path its <see cref="OnDraw"/> runs
/// on the GL thread with the <c>GRContext</c> current, so the shader executes on the GPU;
/// on CpuRendering the same shader is evaluated by Skia's raster backend on the CPU.
/// </summary>
public sealed class EtherealBackdrop : DirectDrawingBase
{
    private const int StarCount = 220;

    //How far (as a fraction of the shorter screen edge) a palm's pull reaches the stars
    private const float StarPullRadiusFraction = 0.38f;

    //The base plasma is three interfering waves plus a slow radial swirl, palette-cycled;
    //  each palm adds a coordinate warp (pull + vortex), inward-drifting color rings, and
    //  a soft glow. With all palm strengths at zero every palm term vanishes and the
    //  shader is exactly the undisturbed plasma.
    private const string EtherealSksl = @"
uniform float iTime;
uniform float2 iResolution;
uniform float3 iPalm0;
uniform float3 iPalm1;
uniform float3 iPalm2;
uniform float3 iPalm3;

float pullAt(float2 p, float3 palm) {
    float2 d = palm.xy - p;
    return palm.z * exp(-dot(d, d) * 1.4);
}

float2 warpAt(float2 p, float3 palm) {
    float2 d = palm.xy - p;
    float pull = palm.z * exp(-dot(d, d) * 1.4);
    float ang = pull * 2.4;
    float cs = cos(ang);
    float sn = sin(ang);
    float2 rel = -d;
    rel = float2((rel.x * cs) - (rel.y * sn), (rel.x * sn) + (rel.y * cs));
    return palm.xy + rel + (d * pull * 0.55);
}

float rippleAt(float2 p, float3 palm, float t) {
    float dist = length(palm.xy - p);
    float pull = palm.z * exp(-dist * dist * 1.1);
    return pull * sin((dist * 9.0) + (t * 2.4)) * 1.1;
}

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float2 p = uv * 2.0 - 1.0;
    p.x *= iResolution.x / iResolution.y;

    float t = iTime * 0.6;

    float2 q = warpAt(p, iPalm0);
    q = warpAt(q, iPalm1);
    q = warpAt(q, iPalm2);
    q = warpAt(q, iPalm3);

    float glow = pullAt(p, iPalm0) + pullAt(p, iPalm1) + pullAt(p, iPalm2) + pullAt(p, iPalm3);

    float v = sin(q.x * 4.0 + t);
    v += sin((q.y + t) * 3.0);
    v += sin((q.x + q.y + t) * 5.0);
    float r = length(q + float2(sin(t * 0.7), cos(t * 0.9)) * 0.4);
    v += sin(r * 8.0 - t * 2.0);

    v += rippleAt(p, iPalm0, iTime);
    v += rippleAt(p, iPalm1, iTime);
    v += rippleAt(p, iPalm2, iTime);
    v += rippleAt(p, iPalm3, iTime);

    v *= 0.25;

    float3 col = 0.5 + 0.5 * cos(6.28318 * (v + float3(0.0, 0.33, 0.67)) + t * 0.3);
    col *= 0.55 + 0.45 * smoothstep(1.6, 0.2, r);

    col += col * glow * 0.9;
    col = min(col, float3(1.0));

    return half4(half3(col), 1.0);
}";

    private static readonly string[] PalmUniformNames = { "iPalm0", "iPalm1", "iPalm2", "iPalm3" };

    private readonly PalmAttractorField _attractorField;
    private readonly SKRuntimeEffect _effect;
    private readonly SKRuntimeEffectUniforms _uniforms;
    private readonly SKPaint _plasmaPaint = new();
    private readonly SKPaint _starPaint = new() { Color = SKColors.White, IsAntialias = true };

    //Reused per-frame buffers - OnDraw allocates nothing
    private readonly float[] _palmState = new float[PalmAttractorField.MaxAttractors * 3];
    private readonly float[][] _palmUniforms;
    private readonly float[] _resolutionUniform = new float[2];

    //Star seeds fixed at construction so the undisturbed frame is a pure function of engine time.
    private readonly float[] _starSeedX = new float[StarCount];
    private readonly float[] _starSeedY = new float[StarCount];
    private readonly float[] _starSpeed = new float[StarCount];
    private readonly float[] _starSize = new float[StarCount];

    private bool _hasLastStepTime;
    private float _lastStepTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealBackdrop"/> class covering the
    /// given screen bounds of a view.
    /// </summary>
    /// <param name="renderSurfaceHost">The render-surface host to draw into.</param>
    /// <param name="view">The view this backdrop belongs to.</param>
    /// <param name="screenBounds">The screen-space bounds to fill, in pixels.</param>
    /// <param name="attractorField">The palm attractors that bend the visual.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="attractorField"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SkSL shader fails to compile.</exception>
    public EtherealBackdrop(RenderSurfaceHostBase renderSurfaceHost, View view, Rectangle screenBounds,
        PalmAttractorField attractorField)
        : base(renderSurfaceHost, DirectDrawingMode.View, null, view, screenBounds, null, "ethereal-backdrop")
    {
        _attractorField = attractorField ?? throw new ArgumentNullException(nameof(attractorField));
        _effect = CreateEffect();
        _uniforms = new SKRuntimeEffectUniforms(_effect);

        _palmUniforms = new float[PalmAttractorField.MaxAttractors][];
        for (int i = 0; i < _palmUniforms.Length; i++)
        {
            _palmUniforms[i] = new float[3];
        }

        var rng = new Random(20260720);
        for (int i = 0; i < StarCount; i++)
        {
            _starSeedX[i] = (float)rng.NextDouble();
            _starSeedY[i] = (float)rng.NextDouble();
            _starSpeed[i] = 0.02f + (float)rng.NextDouble() * 0.12f;   // fraction of width per second
            _starSize[i] = 0.75f + (float)(rng.NextDouble() * rng.NextDouble()) * 2.25f;
        }
    }

    /// <summary>
    /// Compiles the backdrop's SkSL shader (also exercised directly by the unit tests).
    /// </summary>
    /// <returns>The compiled runtime effect.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the SkSL shader fails to compile.</exception>
    internal static SKRuntimeEffect CreateEffect()
        => SKRuntimeEffect.CreateShader(EtherealSksl, out var errors)
            ?? throw new InvalidOperationException($"Ethereal shader failed to compile: {errors}");

    /// <summary>
    /// Marks the backdrop dirty every engine frame so the CpuRendering (CPU) dirty-rectangle path
    /// keeps animating it; the GpuRendering-OpenGL (GPU) path re-renders the full surface each frame
    /// regardless.
    /// </summary>
    /// <param name="tick">The current engine tick.</param>
    public override void Update(long tick)
    {
        base.Update(tick);
        ForceRefresh();
    }

    /// <inheritdoc />
    protected override void OnDraw(BackbufferBase backbuffer, RectangleF destRectScreen)
    {
        var canvas = backbuffer.Canvas;
        float w = destRectScreen.Width;
        float h = destRectScreen.Height;
        if (w <= 0f || h <= 0f)
            return;

        float time = (float)Engine.Instance.TotalSecondsEngineRunning;

        //Advance the attractor smoothing by the frame's real delta (0 on the first frame;
        //  clamped so a stall cannot make the field lurch)
        float delta = _hasLastStepTime ? Math.Clamp(time - _lastStepTime, 0f, 0.1f) : 0f;
        _lastStepTime = time;
        _hasLastStepTime = true;
        _attractorField.Step(delta);
        _attractorField.CopyState(_palmState);

        var rect = destRectScreen.ToSKRect();
        float aspect = w / h;

        // 1) Plasma: one rect through the SkSL shader, with the palms as uniforms
        //    (positions converted into the shader's aspect-corrected -1..1 space).
        _uniforms["iTime"] = time;
        _resolutionUniform[0] = w;
        _resolutionUniform[1] = h;
        _uniforms["iResolution"] = _resolutionUniform;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            _palmUniforms[k][0] = ((_palmState[k * 3] * 2f) - 1f) * aspect;
            _palmUniforms[k][1] = (_palmState[(k * 3) + 1] * 2f) - 1f;
            _palmUniforms[k][2] = _palmState[(k * 3) + 2];
            _uniforms[PalmUniformNames[k]] = _palmUniforms[k];
        }
        using (var shader = _effect.ToShader(_uniforms))
        {
            _plasmaPaint.Shader = shader;
            canvas.DrawRect(rect, _plasmaPaint);
            _plasmaPaint.Shader = null;
        }

        // 2) Starfield: base positions derived from time - faster stars are brighter and
        //    drift further per second, wrapping across the width. Each palm then pulls
        //    nearby stars toward it (and swells and brightens them a little), so the
        //    stars visibly chase an open hand and drift free when it closes.
        float pullRadius = StarPullRadiusFraction * Math.Min(w, h);
        float pullRadiusSq = pullRadius * pullRadius;
        for (int i = 0; i < StarCount; i++)
        {
            float x = rect.Left + (_starSeedX[i] + time * _starSpeed[i]) % 1f * w;
            float y = rect.Top + _starSeedY[i] * h;

            var pull = 0f;
            for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
            {
                float strength = _palmState[(k * 3) + 2];
                if (strength <= 0.001f) { continue; }

                float palmX = rect.Left + _palmState[k * 3] * w;
                float palmY = rect.Top + _palmState[(k * 3) + 1] * h;
                float dx = palmX - x;
                float dy = palmY - y;
                float weight = strength * (float)Math.Exp(-((dx * dx) + (dy * dy)) / pullRadiusSq);
                x += dx * weight * 0.5f;
                y += dy * weight * 0.5f;
                pull += weight;
            }

            float alpha = 90f + (_starSpeed[i] / 0.14f) * 165f + (pull * 140f);
            _starPaint.Color = _starPaint.Color.WithAlpha((byte)Math.Min(255f, alpha));
            canvas.DrawCircle(x, y, _starSize[i] * (1f + (pull * 0.6f)), _starPaint);
        }
    }
}
