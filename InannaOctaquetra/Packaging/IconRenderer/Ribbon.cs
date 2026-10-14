using InannaOctaquetra.Reading.Data;
using SkiaSharp;

/// <summary>
/// Finds the centre line of the Octaquetra's gold band. The authored artwork is a filled outline, so
/// its band is measured (a distance transform of the filled shape gives the ridge that runs down the
/// middle of the band) and a smooth, eight-fold symmetric lobe curve is fitted to that ridge.
/// Coordinates are artwork units: centre (0,0), outer edge at radius 400.
/// </summary>
internal static class Ribbon
{
    public const double OuterRadius = 400;
    public const double BandWidth = 27.2;

    public sealed record Fit(double LobeCentre, double HalfWidth, double CrossDeg, double JoinDeg, double CrossHandle, double JoinHandle, double Reach)
    {
        public static Fit FromArray(double[] p) => new(p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
        public double[] ToArray() => new[] { LobeCentre, HalfWidth, CrossDeg, JoinDeg, CrossHandle, JoinHandle, Reach };
    }

    /// <summary>Points along one lobe whose axis points at <paramref name="axis"/> radians (0 = up), from the heart, round the tip, back to the heart.</summary>
    public static List<(double X, double Y)> Lobe(Fit f, double axis, int samples)
    {
        double ux = Math.Sin(axis), uy = -Math.Cos(axis), vx = Math.Cos(axis), vy = Math.Sin(axis);
        double c = f.LobeCentre, rx = f.HalfWidth, ry = f.Reach - f.LobeCentre;
        double ex = c * ux, ey = c * uy;
        (double X, double Y) On(double t) => (ex + ry * Math.Cos(t) * ux + rx * Math.Sin(t) * vx, ey + ry * Math.Cos(t) * uy + rx * Math.Sin(t) * vy);
        (double X, double Y) Dir(double t)
        {
            double x = -ry * Math.Sin(t) * ux + rx * Math.Cos(t) * vx, y = -ry * Math.Sin(t) * uy + rx * Math.Cos(t) * vy;
            var l = Math.Sqrt(x * x + y * y); return (x / l, y / l);
        }
        var join = Math.PI / 2 + f.JoinDeg * Math.PI / 180;
        var cross = f.CrossDeg * Math.PI / 180;
        var t0 = -join; var t1 = join;
        var a = On(t0); var b = On(t1);
        var chord = Math.Sqrt(a.X * a.X + a.Y * a.Y);
        var da = Dir(t0); var db = Dir(t1);
        double lx = Math.Cos(cross) * ux - Math.Sin(cross) * vx, ly = Math.Cos(cross) * uy - Math.Sin(cross) * vy;
        double rxd = Math.Cos(cross) * ux + Math.Sin(cross) * vx, ryd = Math.Cos(cross) * uy + Math.Sin(cross) * vy;

        var pts = new List<(double, double)>();
        int spoke = samples / 4, arc = samples / 2;
        for (int i = 0; i < spoke; i++)
            pts.Add(Cubic((0, 0), (lx * f.CrossHandle * chord, ly * f.CrossHandle * chord),
                (a.X - da.X * f.JoinHandle * chord, a.Y - da.Y * f.JoinHandle * chord), a, i / (double)spoke));
        for (int i = 0; i < arc; i++) pts.Add(On(t0 + (t1 - t0) * i / arc));
        for (int i = 0; i <= spoke; i++)
            pts.Add(Cubic(b, (b.X + db.X * f.JoinHandle * chord, b.Y + db.Y * f.JoinHandle * chord),
                (rxd * f.CrossHandle * chord, ryd * f.CrossHandle * chord), (0, 0), i / (double)spoke));
        return pts;
    }

    private static (double X, double Y) Cubic((double X, double Y) p0, (double X, double Y) p1, (double X, double Y) p2, (double X, double Y) p3, double t)
    {
        double u = 1 - t;
        return (u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X,
                u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y);
    }

    /// <summary>Measure the band's ridge from the artwork and fit the lobe curve to it.</summary>
    public static Fit FitCentreLine(string? debugPng = null)
    {
        const double scale = 2.0;                        // pixels per artwork unit
        int n = (int)(OuterRadius * 2 * scale) + 40;
        double off = n / 2.0;
        using var bmp = new SKBitmap(n, n, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var cv = new SKCanvas(bmp))
        {
            cv.Clear(SKColors.Black);
            using var path = SKPath.ParseSvgPathData(EmblemArt.InannaOctaquetraPath(off, off, OuterRadius * scale));
            path.FillType = SKPathFillType.EvenOdd;
            using var white = new SKPaint { Color = SKColors.White, IsAntialias = false };
            cv.DrawPath(path, white);
        }
        var span = bmp.GetPixelSpan();
        var inside = new bool[n * n];
        for (int i = 0; i < inside.Length; i++) inside[i] = span[i] > 127;

        // distance from each gold pixel to the nearest non-gold pixel; the ridge runs down the band's middle
        var toEdge = Edt(n, i => !inside[i]);
        var half = BandWidth / 2 * scale;
        var ridge = new List<(double X, double Y)>();
        for (int y = 1; y < n - 1; y++)
            for (int x = 1; x < n - 1; x++)
            {
                var d = toEdge[y * n + x];
                if (d < half * 0.8 || d > half * 1.25) continue;            // skip the crossings, where the band merges
                ridge.Add(((x - off) / scale, (y - off) / scale));
            }
        var ridgeSet = new bool[n * n];
        foreach (var (rx, ry) in ridge) ridgeSet[(int)Math.Round(ry * scale + off) * n + (int)Math.Round(rx * scale + off)] = true;
        var toRidge = Edt(n, i => ridgeSet[i]);
        Console.WriteLine($"band ridge: {ridge.Count} px");

        var rng = new Random(5);
        var ridgeSample = ridge.OrderBy(_ => rng.Next()).Take(6000).ToList();

        double Cost(double[] p)
        {
            var f = Fit.FromArray(p);
            if (f.HalfWidth < 40 || f.LobeCentre < 100 || f.Reach < f.LobeCentre + 40 || f.CrossHandle < 0.05 || f.JoinHandle < 0.05) return 1e9;
            double forward = 0; int count = 0;
            var curve = new List<(double X, double Y)>();
            for (int k = 0; k < 8; k++)
            {
                var lobe = Lobe(f, k * Math.PI / 4, 320);
                curve.AddRange(lobe);
                foreach (var (x, y) in lobe)
                {
                    if (x * x + y * y < 70 * 70) continue;                  // the heart is a crossing
                    int px = (int)Math.Round(x * scale + off), py = (int)Math.Round(y * scale + off);
                    if (px < 0 || py < 0 || px >= n || py >= n) { forward += 50; count++; continue; }
                    forward += toRidge[py * n + px] / scale; count++;
                }
            }
            // reverse: every ridge point should lie near the curve
            var grid = new Dictionary<(int, int), List<(double X, double Y)>>();
            foreach (var pt in curve)
            {
                var key = ((int)Math.Floor(pt.X / 20), (int)Math.Floor(pt.Y / 20));
                if (!grid.TryGetValue(key, out var list)) grid[key] = list = new();
                list.Add(pt);
            }
            double reverse = 0;
            foreach (var (x, y) in ridgeSample)
            {
                int gx = (int)Math.Floor(x / 20), gy = (int)Math.Floor(y / 20);
                double best = 40 * 40;
                for (int dy = -2; dy <= 2; dy++)
                    for (int dx = -2; dx <= 2; dx++)
                        if (grid.TryGetValue((gx + dx, gy + dy), out var list))
                            foreach (var q in list) { var dd = (q.X - x) * (q.X - x) + (q.Y - y) * (q.Y - y); if (dd < best) best = dd; }
                reverse += Math.Sqrt(best);
            }
            return forward / count + reverse / ridgeSample.Count;
        }

        var start = new[] { 240.0, 145.0, 16.0, 40.0, 0.33, 0.33, 386.0 };
        var best = NelderMead(Cost, start, new[] { 20.0, 15.0, 6.0, 10.0, 0.08, 0.08, 8.0 }, 900);
        var fit = Fit.FromArray(best);
        Console.WriteLine($"fit: {fit}  cost {Cost(best):0.00} units");

        if (debugPng is not null)
        {
            using var dbg = new SKBitmap(n, n);
            using var cv = new SKCanvas(dbg);
            cv.Clear(SKColors.White);
            using var gold = SKPath.ParseSvgPathData(EmblemArt.InannaOctaquetraPath(off, off, OuterRadius * scale));
            gold.FillType = SKPathFillType.EvenOdd;
            cv.DrawPath(gold, new SKPaint { Color = new SKColor(230, 196, 118), IsAntialias = true });
            using var line = new SKPaint { Color = SKColors.Red, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
            for (int k = 0; k < 8; k++)
            {
                var pts = Lobe(fit, k * Math.PI / 4, 400);
                for (int i = 1; i < pts.Count; i++)
                    cv.DrawLine((float)(pts[i - 1].X * scale + off), (float)(pts[i - 1].Y * scale + off), (float)(pts[i].X * scale + off), (float)(pts[i].Y * scale + off), line);
            }
            using var img = SKImage.FromBitmap(dbg); using var data = img.Encode(SKEncodedImageFormat.Png, 90);
            File.WriteAllBytes(debugPng, data.ToArray());
        }
        return fit;
    }

    private static double[] NelderMead(Func<double[], double> f, double[] x0, double[] step, int iterations)
    {
        int dim = x0.Length;
        var simplex = new List<(double[] X, double F)> { (x0, f(x0)) };
        for (int i = 0; i < dim; i++) { var x = (double[])x0.Clone(); x[i] += step[i]; simplex.Add((x, f(x))); }
        for (int it = 0; it < iterations; it++)
        {
            simplex.Sort((a, b) => a.F.CompareTo(b.F));
            var centroid = new double[dim];
            for (int i = 0; i < dim; i++) for (int j = 0; j < dim; j++) centroid[j] += simplex[i].X[j] / dim;
            double[] Along(double t) { var r = new double[dim]; for (int j = 0; j < dim; j++) r[j] = centroid[j] + t * (simplex[dim].X[j] - centroid[j]); return r; }
            var xr = Along(-1); var fr = f(xr);
            if (fr < simplex[0].F)
            {
                var xe = Along(-2); var fe = f(xe);
                simplex[dim] = fe < fr ? (xe, fe) : (xr, fr);
            }
            else if (fr < simplex[dim - 1].F) simplex[dim] = (xr, fr);
            else
            {
                var xc = Along(0.5); var fc = f(xc);
                if (fc < simplex[dim].F) simplex[dim] = (xc, fc);
                else
                {
                    for (int i = 1; i <= dim; i++)
                    {
                        var x = new double[dim];
                        for (int j = 0; j < dim; j++) x[j] = simplex[0].X[j] + 0.5 * (simplex[i].X[j] - simplex[0].X[j]);
                        simplex[i] = (x, f(x));
                    }
                }
            }
        }
        simplex.Sort((a, b) => a.F.CompareTo(b.F));
        return simplex[0].X;
    }

    /// <summary>Euclidean distance from every pixel to the nearest pixel where <paramref name="isSeed"/> holds (Felzenszwalb–Huttenlocher).</summary>
    public static float[] Edt(int n, Func<int, bool> isSeed)
    {
        const double inf = 1e12;
        var g = new double[n * n];
        for (int i = 0; i < g.Length; i++) g[i] = isSeed(i) ? 0 : inf;
        var tmp = new double[n * n];
        Parallel.For(0, n, x =>
        {
            var f = new double[n]; var d = new double[n];
            for (int y = 0; y < n; y++) f[y] = g[y * n + x];
            Dt1(f, d, n);
            for (int y = 0; y < n; y++) tmp[y * n + x] = d[y];
        });
        var result = new float[n * n];
        Parallel.For(0, n, y =>
        {
            var f = new double[n]; var d = new double[n];
            for (int x = 0; x < n; x++) f[x] = tmp[y * n + x];
            Dt1(f, d, n);
            for (int x = 0; x < n; x++) result[y * n + x] = (float)Math.Sqrt(d[x]);
        });
        return result;
    }

    private static void Dt1(double[] f, double[] d, int n)
    {
        var v = new int[n]; var z = new double[n + 1];
        int k = 0; v[0] = 0; z[0] = double.NegativeInfinity; z[1] = double.PositiveInfinity;
        for (int q = 1; q < n; q++)
        {
            double s;
            while (true)
            {
                s = ((f[q] + q * (double)q) - (f[v[k]] + v[k] * (double)v[k])) / (2.0 * q - 2.0 * v[k]);
                if (s <= z[k]) { k--; if (k < 0) { k = 0; break; } } else break;
            }
            k++; v[k] = q; z[k] = s; z[k + 1] = double.PositiveInfinity;
        }
        k = 0;
        for (int q = 0; q < n; q++)
        {
            while (z[k + 1] < q) k++;
            d[q] = (q - v[k]) * (double)(q - v[k]) + f[v[k]];
        }
    }
}
