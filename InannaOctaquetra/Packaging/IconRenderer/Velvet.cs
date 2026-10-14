using SkiaSharp;

/// <summary>
/// Renders an original royal-purple crushed-velvet texture: a domain-warped fold field lit so the
/// pile catches light along the ridges of each fold, with a fine grain, in the Octaquetra palette.
/// </summary>
internal static class Velvet
{
    public static void Render(string output, int width, int height, int seed, bool jpeg)
    {
        var noise = new Noise(seed);
        var heights = new float[width, height];
        var scale = 2.1 / Math.Max(width, height);

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                double px = x * scale, py = y * scale;
                // domain warping twists the fabric into soft, crushed folds
                double qx = noise.Fbm(px + 0.0, py + 0.0, 3), qy = noise.Fbm(px + 5.2, py + 1.3, 3);
                double rx = noise.Fbm(px + 2.2 * qx + 1.7, py + 2.2 * qy + 9.2, 3);
                double ry = noise.Fbm(px + 2.2 * qx + 8.3, py + 2.2 * qy + 2.8, 3);
                heights[x, y] = (float)noise.Fbm(px + 2.2 * rx, py + 2.2 * ry, 4);
            }
        });

        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var stops = new (double T, SKColor C)[]
        {
            (0.00, SKColor.Parse("#07020E")),
            (0.22, SKColor.Parse("#1A0530")),
            (0.45, SKColor.Parse("#34095A")),
            (0.68, SKColor.Parse("#5A149A")),
            (0.86, SKColor.Parse("#8A2FD6")),
            (1.00, SKColor.Parse("#C98BFF")),
        };
        var grainRng = new Random(seed * 7 + 3);
        var grain = new float[width * height];
        for (int i = 0; i < grain.Length; i++) grain[i] = (float)(grainRng.NextDouble() - 0.5);

        var pixels = new SKColor[width * height];
        var slopeScale = Math.Max(width, height) / 900.0;
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int x0 = Math.Max(0, x - 1), x1 = Math.Min(width - 1, x + 1);
                int y0 = Math.Max(0, y - 1), y1 = Math.Min(height - 1, y + 1);
                double dx = (heights[x1, y] - heights[x0, y]) * 75 * slopeScale;
                double dy = (heights[x, y1] - heights[x, y0]) * 75 * slopeScale;
                double nz = 1 / Math.Sqrt(dx * dx + dy * dy + 1);
                double nx = -dx * nz, ny = -dy * nz;

                // soft key light from the upper left, velvet sheen where the pile turns edge-on
                double diffuse = Math.Max(0, nx * -0.45 + ny * -0.55 + nz * 0.70);
                double edge = 1 - nz;
                double sheen = Math.Pow(edge, 1.25) * Math.Max(0, 0.55 + 0.45 * (nx * -0.6 + ny * -0.8));
                double valley = heights[x, y];

                double t = 0.08 + 0.36 * diffuse + 1.25 * sheen + 0.34 * (valley - 0.5);
                t += grain[y * width + x] * 0.025;
                t = Math.Clamp(t, 0, 1);
                pixels[y * width + x] = Ramp(stops, t);
            }
        });
        bitmap.Pixels = pixels;

        // a quiet vignette so the edges fall into shadow
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(bitmap, 0, 0);
        using (var vignette = new SKPaint())
        {
            vignette.Shader = SKShader.CreateRadialGradient(new SKPoint(width / 2f, height / 2f),
                Math.Max(width, height) * 0.72f,
                new[] { new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 0), new SKColor(4, 0, 10, 150) },
                new[] { 0f, 0.55f, 1f }, SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, width, height, vignette);
        }

        using var image = surface.Snapshot();
        using var data = jpeg ? image.Encode(SKEncodedImageFormat.Jpeg, 90) : image.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Create(output);
        data.SaveTo(fs);
        Console.WriteLine($"velvet written: {Path.GetFullPath(output)}");
    }

    private static SKColor Ramp((double T, SKColor C)[] stops, double t)
    {
        for (int i = 1; i < stops.Length; i++)
        {
            if (t <= stops[i].T)
            {
                var (t0, c0) = stops[i - 1];
                var (t1, c1) = stops[i];
                var k = (t - t0) / (t1 - t0);
                return new SKColor(
                    (byte)(c0.Red + (c1.Red - c0.Red) * k),
                    (byte)(c0.Green + (c1.Green - c0.Green) * k),
                    (byte)(c0.Blue + (c1.Blue - c0.Blue) * k));
            }
        }
        return stops[^1].C;
    }

    /// <summary>Smooth gradient noise with fractal sums.</summary>
    private sealed class Noise
    {
        private readonly int[] _perm = new int[512];
        private readonly double[] _gx = new double[256], _gy = new double[256];

        public Noise(int seed)
        {
            var rng = new Random(seed);
            var p = Enumerable.Range(0, 256).OrderBy(_ => rng.Next()).ToArray();
            for (int i = 0; i < 512; i++) _perm[i] = p[i & 255];
            for (int i = 0; i < 256; i++)
            {
                var a = rng.NextDouble() * Math.PI * 2;
                _gx[i] = Math.Cos(a); _gy[i] = Math.Sin(a);
            }
        }

        private double Grad(int ix, int iy, double x, double y)
        {
            var h = _perm[_perm[ix & 255] + (iy & 255)];
            return _gx[h] * (x - ix) + _gy[h] * (y - iy);
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        public double Value(double x, double y)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            double sx = Fade(x - x0), sy = Fade(y - y0);
            double n00 = Grad(x0, y0, x, y), n10 = Grad(x0 + 1, y0, x, y);
            double n01 = Grad(x0, y0 + 1, x, y), n11 = Grad(x0 + 1, y0 + 1, x, y);
            double a = n00 + sx * (n10 - n00), b = n01 + sx * (n11 - n01);
            return a + sy * (b - a);
        }

        public double Fbm(double x, double y, int octaves)
        {
            double sum = 0, amp = 0.5, freq = 1, norm = 0;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp * Value(x * freq, y * freq);
                norm += amp; amp *= 0.5; freq *= 2.02;
            }
            return 0.5 + sum / norm * 0.9;
        }
    }
}
