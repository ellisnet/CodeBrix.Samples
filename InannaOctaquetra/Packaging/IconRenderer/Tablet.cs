using System.Text.Json;
using InannaOctaquetra.Reading.Data;
using SkiaSharp;

/// <summary>
/// Impresses the Sumerian text of "Inana and Enki" Segment E (ETCSL c.1.3.1, Inana's claiming of the
/// ME: "he has given me …") into a sun-dried clay surface, and masks it to the inner spaces of the
/// Octaquetra. The signs are pressed into a height map so the lighting reads as true depressions;
/// broken passages are eroded clay with no signs, worn signs are shallow and softened, and an
/// illegible sign is a small damaged patch, exactly as the tablet is preserved.
/// The output image spans the Octaquetra's bounding square: centre (N/2, N/2), outer radius N/2.
/// </summary>
internal static class Tablet
{
    private sealed record Item(string Kind, string? Char);

    public static void Render(string jsonPath, string fontPath, string output, int n)
    {
        var lines = JsonDocument.Parse(File.ReadAllText(jsonPath)).RootElement.EnumerateArray()
            .Select(l => l.GetProperty("items").EnumerateArray()
                .Select(i => new Item(i.GetProperty("k").GetString()!, i.TryGetProperty("c", out var c) ? c.GetString() : null))
                .ToList())
            .ToList();

        using var typeface = SKTypeface.FromFile(fontPath);
        var c = n / 2f;
        var outerR = n / 2f;

        // ---- the Octaquetra's inner spaces: its outer silhouette minus the gold itself
        var artwork = EmblemArt.InannaOctaquetraPath(c, c, outerR);
        using var gold = SKPath.ParseSvgPathData(artwork);
        gold.FillType = SKPathFillType.EvenOdd;
        var firstClose = artwork.IndexOf('z');
        using var silhouette = SKPath.ParseSvgPathData(artwork[..(firstClose + 1)]);
        using var inner = silhouette.Op(gold, SKPathOp.Difference) ?? throw new InvalidOperationException("mask");

        // ---- layout: two columns of eighteen lines, following the round of the tablet
        const int perColumn = 18;
        var interiorR = outerR * 0.86f;
        var gap = n * 0.035f;
        var pad = n * 0.01f;

        float Measure(List<Item> line, float size)
        {
            using var font = new SKFont(typeface, size);
            float w = 0;
            foreach (var it in line)
            {
                if (it.Kind == "space") { w += size * 0.42f; continue; }
                w += it.Char is null ? size * 0.9f : font.MeasureText(it.Char) + size * 0.06f;
            }
            return w;
        }

        float bestSize = 0; float bestRow = 0;
        for (var size = n * 0.08f; size > n * 0.01f; size -= n * 0.0008f)
        {
            var row = size * 1.55f;
            var top = -row * perColumn / 2;
            var fits = true;
            for (int col = 0; col < 2 && fits; col++)
            {
                for (int r = 0; r < perColumn && fits; r++)
                {
                    var line = lines[col * perColumn + r];
                    var y0 = Math.Abs(top + r * row);
                    var y1 = Math.Abs(top + (r + 1) * row);
                    var worst = Math.Max(y0, y1);
                    if (worst >= interiorR) { fits = false; break; }
                    var half = MathF.Sqrt(interiorR * interiorR - worst * worst);
                    if (Measure(line, size) > half - pad - gap / 2) fits = false;
                }
            }
            if (fits) { bestSize = size; bestRow = row; break; }
        }
        Console.WriteLine($"sign size {bestSize:0.0}px, line pitch {bestRow:0.0}px");

        // ---- masks: sign impressions (full, worn), damaged areas, rulings
        using var signs = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using var worn = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using var damage = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using var rulings = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var cs = new SKCanvas(signs)) using (var cw = new SKCanvas(worn)) using (var cd = new SKCanvas(damage)) using (var cr = new SKCanvas(rulings))
        {
            cs.Clear(SKColors.Black); cw.Clear(SKColors.Black); cd.Clear(SKColors.Black); cr.Clear(SKColors.Black);
            using var font = new SKFont(typeface, bestSize) { Edging = SKFontEdging.Antialias };
            using var white = new SKPaint { IsAntialias = true, Color = SKColors.White };
            using var rulePaint = new SKPaint { IsAntialias = true, Color = new SKColor(255, 255, 255), StrokeWidth = n * 0.0016f, Style = SKPaintStyle.Stroke };
            var rng = new Random(131);
            var top = c - bestRow * perColumn / 2;

            for (int col = 0; col < 2; col++)
            {
                for (int r = 0; r < perColumn; r++)
                {
                    var line = lines[col * perColumn + r];
                    var rowTop = top + r * bestRow;
                    var worst = Math.Max(Math.Abs(rowTop - c), Math.Abs(rowTop + bestRow - c));
                    var half = MathF.Sqrt(interiorR * interiorR - worst * worst);
                    var x = col == 0 ? c - half + pad : c + gap / 2;
                    var baseline = rowTop + bestRow * 0.72f;

                    foreach (var it in line)
                    {
                        if (it.Kind == "space") { x += bestSize * 0.42f; continue; }
                        if (it.Char is null)
                        {
                            // illegible sign: a small damaged patch one sign wide
                            DamagePatch(cd, rng, x, rowTop + bestRow * 0.18f, bestSize * 0.9f, bestRow * 0.62f);
                            x += bestSize * 0.9f;
                            continue;
                        }
                        var advance = font.MeasureText(it.Char) + bestSize * 0.06f;
                        switch (it.Kind)
                        {
                            case "sign":
                                cs.DrawText(it.Char, x, baseline, font, white);
                                break;
                            case "worn":
                                cw.DrawText(it.Char, x, baseline, font, white);
                                DamagePatch(cd, rng, x - advance * 0.1f, rowTop + bestRow * 0.2f, advance * 1.2f, bestRow * 0.6f, 0.45f);
                                break;
                            case "broken":
                                // the tablet is broken here: eroded clay, no sign survives
                                DamagePatch(cd, rng, x - bestSize * 0.1f, rowTop + bestRow * 0.05f, advance + bestSize * 0.25f, bestRow * 0.95f);
                                break;
                        }
                        x += advance;
                    }

                    // a scribe's ruling under each line, across the column
                    var ry = rowTop + bestRow;
                    if (r < perColumn - 1)
                    {
                        var rw = MathF.Sqrt(MathF.Max(0, interiorR * interiorR - (ry - c) * (ry - c)));
                        var x0 = col == 0 ? c - rw : c + gap * 0.22f;
                        var x1 = col == 0 ? c - gap * 0.22f : c + rw;
                        cr.DrawLine(x0, ry, x1, ry, rulePaint);
                    }
                }
            }

            // double column divider
            var vr = bestRow * perColumn / 2;
            cr.DrawLine(c - gap * 0.12f, c - vr, c - gap * 0.12f, c + vr, rulePaint);
            cr.DrawLine(c + gap * 0.12f, c - vr, c + gap * 0.12f, c + vr, rulePaint);
        }

        // ---- height map
        var noise = new Noise(7);
        var h = new float[n * n];
        var sgn = ToFloat(signs); var wrn = ToFloat(worn); var dmg = ToFloat(damage); var rul = ToFloat(rulings);
        BoxBlur(sgn, n, Math.Max(1, (int)(bestSize * 0.035f)), 2);
        BoxBlur(wrn, n, Math.Max(1, (int)(bestSize * 0.08f)), 3);
        BoxBlur(dmg, n, Math.Max(2, (int)(bestSize * 0.12f)), 2);
        BoxBlur(rul, n, 1, 1);
        var grainRng = new Random(11);

        Parallel.For(0, n, y =>
        {
            for (int x = 0; x < n; x++)
            {
                double px = x / (double)n, py = y / (double)n;
                var undulate = noise.Fbm(px * 3.5, py * 3.5, 4) - 0.5;
                var fine = noise.Fbm(px * 60, py * 60, 3) - 0.5;
                var rough = noise.Fbm(px * 22 + 5, py * 22 + 9, 4) - 0.5;
                var d = dmg[y * n + x];
                var height = undulate * 10 + fine * 1.4
                    - sgn[y * n + x] * 9.0
                    - wrn[y * n + x] * 3.2
                    - rul[y * n + x] * 2.2
                    - d * (5.5 + rough * 9) ;
                h[y * n + x] = (float)height;
            }
        });

        // pits and pores in the clay
        for (int i = 0; i < n * 0.7; i++)
        {
            int px = grainRng.Next(n), py = grainRng.Next(n);
            var rad = 1 + grainRng.Next(4);
            for (int dy = -rad; dy <= rad; dy++)
                for (int dx = -rad; dx <= rad; dx++)
                {
                    int xx = px + dx, yy = py + dy;
                    if (xx < 0 || yy < 0 || xx >= n || yy >= n || dx * dx + dy * dy > rad * rad) continue;
                    h[yy * n + xx] -= 0.7f * (1 - (dx * dx + dy * dy) / (float)(rad * rad + 1));
                }
        }

        // ambient occlusion: how far a point sits below its neighbourhood
        var avg = (float[])h.Clone();
        BoxBlur(avg, n, Math.Max(2, (int)(bestSize * 0.12f)), 2);

        // ---- shade with a raking light from the upper left
        var pixels = new SKColor[n * n];
        double lx = -0.62, ly = -0.62, lz = 0.48;
        var ll = Math.Sqrt(lx * lx + ly * ly + lz * lz); lx /= ll; ly /= ll; lz /= ll;
        Parallel.For(0, n, y =>
        {
            for (int x = 0; x < n; x++)
            {
                int x0 = Math.Max(0, x - 1), x1 = Math.Min(n - 1, x + 1), y0 = Math.Max(0, y - 1), y1 = Math.Min(n - 1, y + 1);
                double dx = (h[y * n + x1] - h[y * n + x0]) * 0.5;
                double dy = (h[y1 * n + x] - h[y0 * n + x]) * 0.5;
                double nx = -dx, ny = -dy, nz = 1.0;
                var nl = Math.Sqrt(nx * nx + ny * ny + nz * nz); nx /= nl; ny /= nl; nz /= nl;
                var diffuse = Math.Max(0, nx * lx + ny * ly + nz * lz);
                var occlusion = Math.Clamp((avg[y * n + x] - h[y * n + x]) * 0.06, -0.15, 0.55);
                var tone = noise.Fbm(x / (double)n * 6 + 3, y / (double)n * 6 + 1, 3);
                var d = dmg[y * n + x];

                // sun-dried clay: ochre to buff, darker and more grey where it has flaked away
                double r = 176 + (tone - 0.5) * 34, g = 138 + (tone - 0.5) * 28, b = 94 + (tone - 0.5) * 20;
                r = r * (1 - d * 0.18) + 140 * d * 0.18; g = g * (1 - d * 0.18) + 120 * d * 0.18; b = b * (1 - d * 0.18) + 100 * d * 0.18;
                var light = 0.34 + 0.86 * diffuse - occlusion;
                pixels[y * n + x] = new SKColor(
                    (byte)Math.Clamp(r * light, 0, 255),
                    (byte)Math.Clamp(g * light, 0, 255),
                    (byte)Math.Clamp(b * light, 0, 255));
            }
        });

        using var clay = new SKBitmap(n, n, SKColorType.Rgba8888, SKAlphaType.Premul) { Pixels = pixels };

        // ---- composite into the inner spaces, recessed beneath the gold
        using var surface = SKSurface.Create(new SKImageInfo(n, n, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Save();
        canvas.ClipPath(inner, antialias: true);
        canvas.DrawBitmap(clay, 0, 0);

        // an inner shadow along every edge, as if the clay lies below the gold inlay
        using (var edge = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = n * 0.018f,
            Color = new SKColor(20, 8, 2, 150), ImageFilter = SKImageFilter.CreateBlur(n * 0.008f, n * 0.008f),
        })
        {
            canvas.Translate(n * 0.002f, n * 0.003f);
            canvas.DrawPath(inner, edge);
        }
        canvas.Restore();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using (var fs = File.Create(output)) data.SaveTo(fs);
        Console.WriteLine($"tablet written: {Path.GetFullPath(output)}");
    }

    private static void DamagePatch(SKCanvas canvas, Random rng, float x, float y, float w, float h, float strength = 1f)
    {
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(255, 255, 255, (byte)(255 * strength)) };
        using var path = new SKPath();
        var cx = x + w / 2; var cy = y + h / 2;
        const int k = 14;
        for (int i = 0; i < k; i++)
        {
            var a = i * MathF.PI * 2 / k;
            var jitter = 0.72f + (float)rng.NextDouble() * 0.45f;
            var px = cx + MathF.Cos(a) * w / 2 * jitter;
            var py = cy + MathF.Sin(a) * h / 2 * jitter;
            if (i == 0) path.MoveTo(px, py); else path.LineTo(px, py);
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private static float[] ToFloat(SKBitmap bmp)
    {
        var n = bmp.Width; var result = new float[n * bmp.Height];
        var span = bmp.GetPixelSpan();
        for (int i = 0; i < result.Length; i++) result[i] = span[i] / 255f;
        return result;
    }

    private static void BoxBlur(float[] a, int n, int radius, int passes)
    {
        var tmp = new float[a.Length];
        for (int p = 0; p < passes; p++)
        {
            Parallel.For(0, n, y =>
            {
                float sum = 0; int count = 0;
                for (int x = -radius; x < n; x++)
                {
                    int add = x + radius, rem = x - radius - 1;
                    if (add < n) { sum += a[y * n + add]; count++; }
                    if (rem >= 0) { sum -= a[y * n + rem]; count--; }
                    if (x >= 0) tmp[y * n + x] = sum / Math.Max(1, count);
                }
            });
            Parallel.For(0, n, x =>
            {
                float sum = 0; int count = 0;
                for (int y = -radius; y < n; y++)
                {
                    int add = y + radius, rem = y - radius - 1;
                    if (add < n) { sum += tmp[add * n + x]; count++; }
                    if (rem >= 0) { sum -= tmp[rem * n + x]; count--; }
                    if (y >= 0) a[y * n + x] = sum / Math.Max(1, count);
                }
            });
        }
    }

    private sealed class Noise
    {
        private readonly int[] _perm = new int[512];
        private readonly double[] _gx = new double[256], _gy = new double[256];
        public Noise(int seed)
        {
            var rng = new Random(seed);
            var p = Enumerable.Range(0, 256).OrderBy(_ => rng.Next()).ToArray();
            for (int i = 0; i < 512; i++) _perm[i] = p[i & 255];
            for (int i = 0; i < 256; i++) { var a = rng.NextDouble() * Math.PI * 2; _gx[i] = Math.Cos(a); _gy[i] = Math.Sin(a); }
        }
        private double Grad(int ix, int iy, double x, double y) { var h = _perm[_perm[ix & 255] + (iy & 255)]; return _gx[h] * (x - ix) + _gy[h] * (y - iy); }
        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        public double Value(double x, double y)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            double sx = Fade(x - x0), sy = Fade(y - y0);
            double a = Grad(x0, y0, x, y) + sx * (Grad(x0 + 1, y0, x, y) - Grad(x0, y0, x, y));
            double b = Grad(x0, y0 + 1, x, y) + sx * (Grad(x0 + 1, y0 + 1, x, y) - Grad(x0, y0 + 1, x, y));
            return a + sy * (b - a);
        }
        public double Fbm(double x, double y, int octaves)
        {
            double sum = 0, amp = 0.5, freq = 1, norm = 0;
            for (int i = 0; i < octaves; i++) { sum += amp * Value(x * freq, y * freq); norm += amp; amp *= 0.5; freq *= 2.02; }
            return 0.5 + sum / norm * 0.9;
        }
    }
}
