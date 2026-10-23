using System;
using SkiaSharp;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws the Ethereal visual with SkiaSharp - the "Ethereal - OpenGL" renderer: an animated
/// SkSL plasma (<see cref="SKRuntimeEffect"/>) under a drifting starfield, both taken from an
/// <see cref="EtherealScene"/>. Wherever an open palm is held up, the plasma's coordinate field
/// is bent toward it (a soft gravitational pull plus a slow vortex), rings of color drift
/// inward toward it, the colors brighten around it, and nearby stars are drawn toward it - so
/// the whole visual appears to chase the hand. When every attractor strength is zero the
/// shader reduces EXACTLY to its undisturbed form, which is how the visual melts back to its
/// normal motion when the palms close. Drawn onto a GPU-backed canvas the shader executes on
/// the GPU; drawn onto a raster canvas Skia evaluates the same shader on the CPU.
/// </summary>
/// <remarks>
/// <see cref="EtherealWebGpuRenderer"/> carries a WGSL port of the same shader; a change to
/// one belongs in the other.
/// </remarks>
public sealed class EtherealSkiaRenderer : IDisposable
{
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

    private readonly SKRuntimeEffect _effect;
    private readonly SKRuntimeEffectUniforms _uniforms;
    private readonly SKPaint _plasmaPaint = new();
    private readonly SKPaint _starPaint = new() { Color = SKColors.White, IsAntialias = true };

    //Reused per-frame uniform buffers - Draw allocates nothing
    private readonly float[][] _palmUniforms;
    private readonly float[] _resolutionUniform = new float[2];

    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealSkiaRenderer"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the SkSL shader fails to compile.</exception>
    public EtherealSkiaRenderer()
    {
        _effect = CreateEffect();
        _uniforms = new SKRuntimeEffectUniforms(_effect);

        _palmUniforms = new float[PalmAttractorField.MaxAttractors][];
        for (int i = 0; i < _palmUniforms.Length; i++)
        {
            _palmUniforms[i] = new float[EtherealScene.PalmStride];
        }
    }

    /// <summary>
    /// Compiles the visual's SkSL shader (also exercised directly by the unit tests).
    /// </summary>
    /// <returns>The compiled runtime effect.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the SkSL shader fails to compile.</exception>
    internal static SKRuntimeEffect CreateEffect()
        => SKRuntimeEffect.CreateShader(EtherealSksl, out var errors)
            ?? throw new InvalidOperationException($"Ethereal shader failed to compile: {errors}");

    /// <summary>
    /// Draws one frame of the scene, filling the canvas from (0, 0) to the scene's size.
    /// </summary>
    /// <param name="canvas">The canvas to draw on (GPU-backed or raster).</param>
    /// <param name="scene">The scene, already advanced to this frame.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canvas"/> or
    /// <paramref name="scene"/> is null.</exception>
    public void Draw(SKCanvas canvas, EtherealScene scene)
    {
        if (canvas == null) { throw new ArgumentNullException(nameof(canvas)); }
        if (scene == null) { throw new ArgumentNullException(nameof(scene)); }

        float w = scene.Width;
        float h = scene.Height;
        if (w <= 0f || h <= 0f) { return; }

        // 1) Plasma: one rect through the SkSL shader, with the palms as uniforms
        _uniforms["iTime"] = scene.Time;
        _resolutionUniform[0] = w;
        _resolutionUniform[1] = h;
        _uniforms["iResolution"] = _resolutionUniform;
        ReadOnlySpan<float> palms = scene.Palms;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            palms.Slice(k * EtherealScene.PalmStride, EtherealScene.PalmStride).CopyTo(_palmUniforms[k]);
            _uniforms[PalmUniformNames[k]] = _palmUniforms[k];
        }
        using (var shader = _effect.ToShader(_uniforms))
        {
            _plasmaPaint.Shader = shader;
            canvas.DrawRect(new SKRect(0f, 0f, w, h), _plasmaPaint);
            _plasmaPaint.Shader = null;
        }

        // 2) Starfield: white discs whose brightness is the alpha the scene computed
        ReadOnlySpan<float> stars = scene.Stars;
        for (int i = 0; i < EtherealScene.StarCount; i++)
        {
            int s = i * EtherealScene.StarStride;
            _starPaint.Color = SKColors.White.WithAlpha((byte)(stars[s + 3] * 255f));
            canvas.DrawCircle(stars[s], stars[s + 1], stars[s + 2], _starPaint);
        }
    }

    /// <summary>Releases the compiled shader and the paints.</summary>
    public void Dispose()
    {
        _plasmaPaint.Dispose();
        _starPaint.Dispose();
        _uniforms.Dispose();
        _effect.Dispose();
    }
}
