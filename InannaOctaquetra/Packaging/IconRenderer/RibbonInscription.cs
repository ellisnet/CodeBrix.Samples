using System.Text.Json;
using InannaOctaquetra.Reading.Data;
using SkiaSharp;

/// <summary>
/// Engraves Inana's claiming of the ME (ETCSL c.1.3.1, Segment E) into the gold band of the Octaquetra
/// as one continuous strand. The strand runs the four infinity loops end to end, out along a petal,
/// round its tip, through the heart and into the opposite petal, so the signs follow every curve.
/// Where the band crosses itself the later pass lies over the earlier, like a woven ribbon.
/// The output spans the Octaquetra's bounding square: centre (N/2, N/2), outer edge at radius N/2.
/// </summary>
internal static class RibbonInscription
{
    private sealed record Item(string Kind, string? Char);

    public static void Render(string jsonPath, string fontPath, string output, int n, Ribbon.Fit fit)
    {
        var lines = JsonDocument.Parse(File.ReadAllText(jsonPath)).RootElement.EnumerateArray()
            .Select(l => l.GetProperty("items").EnumerateArray()
                .Select(i => new Item(i.GetProperty("k").GetString() switch { "broken" or "worn" => "sign", var k => k! },
                    i.TryGetProperty("c", out var c) ? c.GetString() : null))
                .ToList())
            .ToList();

        var scale = n / 2.0 / Ribbon.OuterRadius;           // pixels per artwork unit
        var centre = n / 2.0;
        (float X, float Y) Px((double X, double Y) p) => ((float)(p.X * scale + centre), (float)(p.Y * scale + centre));

        // ---- the strand: four infinity loops chained through the heart
        var strand = BuildStrand(fit);

        // arc-length parameterisation, leaving out the heart, where every pass crosses every other
        const double heartClear = 64;
        var usable = new List<(double X, double Y, double Tx, double Ty)>();
        var usableS = new List<double>();
        double s = 0;
        for (int i = 1; i < strand.Count; i++)
        {
            var (x0, y0) = strand[i - 1]; var (x1, y1) = strand[i];
            var seg = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            if (seg < 1e-9) continue;
            if (x1 * x1 + y1 * y1 < heartClear * heartClear) continue;
            s += seg;
            usable.Add((x1, y1, (x1 - x0) / seg, (y1 - y0) / seg));
            usableS.Add(s);
        }
        var total = s;
        Console.WriteLine($"strand: {strand.Count} points, lettered length {total:0} units");

        (double X, double Y, double Tx, double Ty) At(double arc)
        {
            int lo = 0, hi = usableS.Count - 1;
            while (lo < hi) { int mid = (lo + hi) / 2; if (usableS[mid] < arc) lo = mid + 1; else hi = mid; }
            var p = usable[lo];
            // smooth the tangent over a few samples so glyphs do not jitter
            double tx = 0, ty = 0;
            for (int k = Math.Max(0, lo - 3); k <= Math.Min(usable.Count - 1, lo + 3); k++) { tx += usable[k].Tx; ty += usable[k].Ty; }
            var l = Math.Sqrt(tx * tx + ty * ty);
            return (p.X, p.Y, tx / l, ty / l);
        }

        // ---- sign size: the tallest sign spans 94% of the band
        using var typeface = SKTypeface.FromFile(fontPath);
        var chars = lines.SelectMany(l => l).Where(i => i.Char is not null).Select(i => i.Char!).Distinct().ToList();
        using var probe = new SKFont(typeface, 100);
        var tallest = chars.Max(ch => { probe.MeasureText(ch, out var b); return b.Height; });
        var fontUnits = 100 * (Ribbon.BandWidth * 0.94) / tallest;       // font size in artwork units
        using var font = new SKFont(typeface, (float)(fontUnits * scale)) { Edging = SKFontEdging.Antialias };

        // natural widths with tight spacing; the passage repeats round the closed strand, ending on a whole
        // poetic line, and whatever length remains is shared out between words and lines
        const double signGap = 0.8, wordGap = 6, lineGap = 16;
        var passage = new List<List<(Item Item, double Width, double Gap)>>();
        foreach (var line in lines)
        {
            var entry = new List<(Item Item, double Width, double Gap)>();
            foreach (var it in line)
            {
                if (it.Kind == "space") { entry.Add((it, 0, wordGap)); continue; }
                double w = it.Char is null ? fontUnits * 0.8 : font.MeasureText(it.Char) / scale;
                entry.Add((it, w, signGap));
            }
            entry.Add((new Item("line", null), 0, lineGap));
            passage.Add(entry);
        }
        var advances = new List<(Item Item, double Width, double Gap)>();
        double used = 0; int placedLines = 0;
        for (int li = 0; ; li = (li + 1) % passage.Count)
        {
            var len = passage[li].Sum(a => a.Width + a.Gap);
            if (used + len > total - 20) break;
            advances.AddRange(passage[li]); used += len; placedLines++;
        }
        var natural = used;
        var flexible = advances.Where(a => a.Item.Kind is "space" or "line").Sum(a => a.Gap);
        var spare = Math.Max(0, total - 20 - natural);
        var flexScale = flexible > 0 ? 1 + spare / flexible : 1;
        const double spread = 1.0;
        Console.WriteLine($"font {fontUnits:0.0} units, {placedLines} poetic lines ({placedLines / (double)passage.Count:0.00} passes), word/line gaps x{flexScale:0.00}");

        // ---- impression mask, woven: each sign first clears the band beneath it, then is pressed in
        using var gold = SKPath.ParseSvgPathData(EmblemArt.InannaOctaquetraPath(centre, centre, n / 2.0));
        gold.FillType = SKPathFillType.EvenOdd;
        using var deep = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using var wornMask = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using var damage = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        var rng = new Random(131);
        using (var cDeep = new SKCanvas(deep)) using (var cWorn = new SKCanvas(wornMask)) using (var cDmg = new SKCanvas(damage))
        {
            foreach (var cv in new[] { cDeep, cWorn, cDmg }) { cv.Clear(SKColors.Black); cv.ClipPath(gold, antialias: true); }
            using var white = new SKPaint { IsAntialias = true, Color = SKColors.White };
            using var black = new SKPaint { IsAntialias = true, Color = SKColors.Black };
            var bandPx = (float)(Ribbon.BandWidth * scale);

            double arc = 10;
            foreach (var (item, width, gap) in advances)
            {
                if (item.Kind is "space" or "line") { arc += gap * flexScale; continue; }
                if (item.Char is null) { arc += (width + gap) * spread; continue; }   // illegible: left unlettered
                var mid = At(Math.Min(total - 1, arc + width / 2));
                var (px, py) = Px((mid.X, mid.Y));
                var angle = (float)(Math.Atan2(mid.Ty, mid.Tx) * 180 / Math.PI);
                var w = (float)(width * scale);

                foreach (var cv in new[] { cDeep, cWorn, cDmg })
                {
                    cv.Save();
                    cv.Translate(px, py);
                    cv.RotateDegrees(angle);
                    // this pass lies over any earlier pass here: clear the band beneath the sign
                    cv.DrawRect(SKRect.Create(-w / 2 - (float)(signGap * spread * scale) / 2 - 2, -bandPx / 2 - 2, w + (float)(signGap * spread * scale) + 4, bandPx + 4), black);
                    cv.Restore();
                }

                cDeep.Save(); cWorn.Save(); cDmg.Save();
                foreach (var cv in new[] { cDeep, cWorn, cDmg }) { cv.Translate(px, py); cv.RotateDegrees(angle); }
                if (item.Char is null)
                {
                    Wear(cDmg, rng, w, bandPx * 0.55f, 0.6f);
                }
                else
                {
                    font.MeasureText(item.Char, out var bounds);
                    var gx = -w / 2;
                    var gy = -bounds.MidY;                      // centre the sign across the band
                    switch (item.Kind)
                    {
                        case "sign": cDeep.DrawText(item.Char, gx, gy, font, white); break;
                        case "worn": cWorn.DrawText(item.Char, gx, gy, font, white); Wear(cDmg, rng, w * 1.1f, bandPx * 0.5f, 0.35f); break;
                        case "broken": Wear(cDmg, rng, w + (float)(gap * spread * scale) + 2, bandPx * 0.62f, 0.75f); break;
                    }
                }
                cDeep.Restore(); cWorn.Restore(); cDmg.Restore();
                arc += (width + gap) * spread;
            }
        }

        // ---- height map and shading
        var dMask = ToFloat(deep); var wMask = ToFloat(wornMask); var gMask = ToFloat(damage);
        var engrave = Math.Max(1, (int)(fontUnits * scale * 0.03));
        Blur(dMask, n, engrave, 2); Blur(wMask, n, engrave * 2, 3); Blur(gMask, n, engrave * 2, 2);
        var noise = new Noise(9);
        var h = new float[n * n];
        Parallel.For(0, n, y =>
        {
            for (int x = 0; x < n; x++)
            {
                var i = y * n + x;
                var rough = noise.Fbm(x / (double)n * 90, y / (double)n * 90, 3) - 0.5;
                h[i] = (float)(-dMask[i] * 7.5 - wMask[i] * 2.8 - gMask[i] * (2.2 + rough * 5));
            }
        });

        using var goldMask = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var cv = new SKCanvas(goldMask)) { cv.Clear(SKColors.Black); cv.DrawPath(gold, new SKPaint { IsAntialias = true, Color = SKColors.White }); }
        var inGold = ToFloat(goldMask);

        var stops = new (double T, double R, double G, double B)[] { (0, 247, 226, 126), (0.4, 232, 194, 78), (0.75, 193, 143, 42), (1, 126, 78, 16) };
        double lx = -0.55, ly = -0.65, lz = 0.52; var ll = Math.Sqrt(lx * lx + ly * ly + lz * lz); lx /= ll; ly /= ll; lz /= ll;
        var pixels = new SKColor[n * n];
        Parallel.For(0, n, y =>
        {
            var t = y / (double)(n - 1);
            double br = 0, bg = 0, bb = 0;
            for (int k = 1; k < stops.Length; k++)
                if (t <= stops[k].T) { var a = stops[k - 1]; var b = stops[k]; var u = (t - a.T) / (b.T - a.T); br = a.R + (b.R - a.R) * u; bg = a.G + (b.G - a.G) * u; bb = a.B + (b.B - a.B) * u; break; }
            for (int x = 0; x < n; x++)
            {
                var i = y * n + x;
                var alpha = inGold[i];
                if (alpha <= 0) { pixels[i] = SKColors.Transparent; continue; }
                int x0 = Math.Max(0, x - 1), x1 = Math.Min(n - 1, x + 1), y0 = Math.Max(0, y - 1), y1 = Math.Min(n - 1, y + 1);
                double dx = (h[y * n + x1] - h[y * n + x0]) * 0.5, dy = (h[y1 * n + x] - h[y0 * n + x]) * 0.5;
                double nx = -dx, ny = -dy, nz = 1; var nl = Math.Sqrt(nx * nx + ny * ny + nz * nz); nx /= nl; ny /= nl; nz /= nl;
                var flat = lz;
                var diffuse = Math.Max(0, nx * lx + ny * ly + nz * lz);
                var depth = Math.Clamp(-h[i] / 7.5, 0, 1);
                // polished gold where flat, engraved channels darker and warmer toward bronze
                var light = 0.9 + 0.75 * (diffuse - flat) - depth * 0.42;
                double r = br * light, g = bg * light * (1 - depth * 0.08), b = bb * light * (1 - depth * 0.18);
                var a8 = (byte)Math.Round(alpha * 255);
                pixels[i] = new SKColor((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255), a8);
            }
        });

        using var bitmap = new SKBitmap(n, n, SKColorType.Rgba8888, SKAlphaType.Unpremul) { Pixels = pixels };
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using (var fs = File.Create(output)) data.SaveTo(fs);
        Console.WriteLine($"ribbon written: {Path.GetFullPath(output)}");
    }

    /// <summary>Four infinity loops end to end, each crossing through the heart, as one list of points.</summary>
    public static List<(double X, double Y)> BuildStrand(Ribbon.Fit fit)
    {
        var strand = new List<(double X, double Y)>();
        var lastDir = (X: 0.0, Y: -1.0);
        for (int loop = 0; loop < 4; loop++)
        {
            var axis = loop * Math.PI / 4;
            var candidates = new[]
            {
                (First: Ribbon.Lobe(fit, axis, 480), Second: Reverse(Ribbon.Lobe(fit, axis + Math.PI, 480))),
                (First: Reverse(Ribbon.Lobe(fit, axis, 480)), Second: Ribbon.Lobe(fit, axis + Math.PI, 480)),
                (First: Ribbon.Lobe(fit, axis + Math.PI, 480), Second: Reverse(Ribbon.Lobe(fit, axis, 480))),
                (First: Reverse(Ribbon.Lobe(fit, axis + Math.PI, 480)), Second: Ribbon.Lobe(fit, axis, 480)),
            };
            // keep the strand flowing: leave the heart as close as possible to the direction it arrived
            var best = candidates.OrderByDescending(c => loop == 0 ? (c.First[10].Y < 0 && c.First[10].X < 0 ? 1 : 0) : Dot(Dir(c.First, 0, 12), lastDir)).First();
            strand.AddRange(best.First);
            strand.AddRange(best.Second);
            lastDir = Dir(best.Second, best.Second.Count - 13, best.Second.Count - 1);
        }
        return strand;
    }

    private static List<(double X, double Y)> Reverse(List<(double X, double Y)> pts) { var r = new List<(double X, double Y)>(pts); r.Reverse(); return r; }
    private static (double X, double Y) Dir(List<(double X, double Y)> pts, int a, int b)
    {
        double x = pts[b].X - pts[a].X, y = pts[b].Y - pts[a].Y; var l = Math.Sqrt(x * x + y * y); return (x / l, y / l);
    }
    private static double Dot((double X, double Y) a, (double X, double Y) b) => a.X * b.X + a.Y * b.Y;

    /// <summary>A worn stretch of the band: a long, ragged, shallow scuff that joins its neighbours.</summary>
    private static void Wear(SKCanvas canvas, Random rng, float w, float h, float strength)
    {
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(255, 255, 255, (byte)(255 * strength)) };
        using var path = new SKPath();
        const int k = 10;
        path.MoveTo(-w / 2, 0);
        for (int i = 0; i <= k; i++) path.LineTo(-w / 2 + w * i / k, -h / 2 * (0.7f + (float)rng.NextDouble() * 0.3f));
        for (int i = k; i >= 0; i--) path.LineTo(-w / 2 + w * i / k, h / 2 * (0.7f + (float)rng.NextDouble() * 0.3f));
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private static void Patch(SKCanvas canvas, Random rng, float w, float h, float strength)
    {
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(255, 255, 255, (byte)(255 * strength)) };
        using var path = new SKPath();
        const int k = 12;
        for (int i = 0; i < k; i++)
        {
            var a = i * MathF.PI * 2 / k;
            var j = 0.72f + (float)rng.NextDouble() * 0.4f;
            var x = MathF.Cos(a) * w / 2 * j; var y = MathF.Sin(a) * h / 2 * j;
            if (i == 0) path.MoveTo(x, y); else path.LineTo(x, y);
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private static float[] ToFloat(SKBitmap bmp)
    {
        var span = bmp.GetPixelSpan(); var r = new float[span.Length];
        for (int i = 0; i < r.Length; i++) r[i] = span[i] / 255f;
        return r;
    }

    private static void Blur(float[] a, int n, int radius, int passes)
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
        private double Value(double x, double y)
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
