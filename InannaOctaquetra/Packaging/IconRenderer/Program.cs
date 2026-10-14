// Renders the Octaquetra of Inanna app icon (1024x1024 PNG) from the same Venus-star geometry the
// app draws as its logo, laid out on Apple's macOS icon grid: an 824px rounded-square tile
// centred in a 1024px canvas, with a soft shadow beneath it.
//   dotnet run --project Packaging/IconRenderer -- <output.png>
// The other modes, run from the InannaOctaquetra folder (the app embeds their outputs from
// src/libs/InannaOctaquetra.Reading/Art, so a re-rendered picture goes there):
//   ... -- velvet src/libs/InannaOctaquetra.Reading/Art/velvet.jpg 2400 1500 [seed]
//   ... -- velvet src/libs/InannaOctaquetra.Reading/Art/velvet-print.jpg 1800 1300 [seed]
//   ... -- ribbon (see Packaging/Artwork/Tablet/SOURCES.md for the inscribed band)
using InannaOctaquetra.Reading.Data;
using SkiaSharp;

if (args.Length >= 5 && args[0] == "ribbon") { RibbonInscription.Render(args[1], args[2], args[3], int.Parse(args[4]), Ribbon.FitCentreLine()); return; }
if (args.Length >= 2 && args[0] == "fit") { Ribbon.FitCentreLine(args[1]); return; }
if (args.Length >= 5 && args[0] == "tablet") { Tablet.Render(args[1], args[2], args[3], int.Parse(args[4])); return; }
if (args.Length >= 3 && args[0] == "coverage") { Coverage.Run(args[1], args.Skip(2).ToArray()); return; }
if (args.Length >= 4 && args[0] == "velvet")
{
    // velvet <output> <width> <height> [seed]
    Velvet.Render(args[1], int.Parse(args[2]), int.Parse(args[3]),
        args.Length > 4 ? int.Parse(args[4]) : 11, args[1].EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));
    return;
}

var output = args.Length > 0 ? args[0] : "AppIcon-1024.png";
const int size = 1024;
const float tile = 824f, inset = (size - tile) / 2f, corner = 185f;

SKColor C(string hex, byte alpha = 255) => SKColor.Parse(hex).WithAlpha(alpha);
var night = C("#0F0A1E"); var night2 = C("#1A1030"); var kohl = C("#07050D");
var lapis = C("#1F3A93"); var lapisGlow = C("#4C6FE0");
var gold = C("#E6C476"); var goldPale = C("#F3DFA2"); var goldDeep = C("#C9A14A"); var goldShadow = C("#8A6A1F");

using var surface = SKSurface.Create(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
var canvas = surface.Canvas;
canvas.Clear(SKColors.Transparent);

var tileRect = SKRect.Create(inset, inset, tile, tile);
using var tilePath = new SKPath();
tilePath.AddRoundRect(tileRect, corner, corner);
var cx = size / 2f; var cy = size / 2f;

canvas.Save();
canvas.ClipPath(tilePath, antialias: true);

// ---- royal purple crushed velvet, deepened toward the edges
var velvetFile = Path.Combine(Path.GetTempPath(), "octaquetra-icon-velvet.png");
Velvet.Render(velvetFile, 1024, 1024, 31, jpeg: false);
using (var velvet = SKBitmap.Decode(velvetFile))
{
    canvas.DrawBitmap(velvet, SKRect.Create(0, 0, size, size));
}
using (var shade = new SKPaint { IsAntialias = true })
{
    shade.Shader = SKShader.CreateRadialGradient(new SKPoint(cx, cy), tile * 0.72f,
        new[] { new SKColor(10, 3, 20, 60), new SKColor(10, 3, 20, 120), new SKColor(6, 1, 12, 200) },
        new[] { 0f, 0.6f, 1f }, SKShaderTileMode.Clamp);
    canvas.DrawRect(tileRect, shade);
}

// ---- stars and four-pointed sparkles, like the app's starfield
var rng = new Random(8);
using (var starDot = new SKPaint { IsAntialias = true })
{
    for (int i = 0; i < 90; i++)
    {
        var x = inset + (float)rng.NextDouble() * tile;
        var y = inset + (float)rng.NextDouble() * tile;
        var d = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
        if (d < 330) { continue; }
        starDot.Color = (rng.NextDouble() < 0.3 ? C("#E7D2FF") : goldPale).WithAlpha((byte)rng.Next(90, 220));
        canvas.DrawCircle(x, y, (float)(1.0 + rng.NextDouble() * 2.2), starDot);
    }
}
void Sparkle(float x, float y, float r, SKColor colour)
{
    using var halo = new SKPaint { IsAntialias = true };
    halo.Shader = SKShader.CreateRadialGradient(new SKPoint(x, y), r * 1.3f,
        new[] { colour.WithAlpha(120), colour.WithAlpha(0) }, new[] { 0f, 1f }, SKShaderTileMode.Clamp);
    canvas.DrawCircle(x, y, r * 1.3f, halo);
    var k = r / 11f;
    using var star = new SKPath();
    star.MoveTo(x, y - 11 * k);
    star.CubicTo(x + 1.1f * k, y - 2.4f * k, x + 2.4f * k, y - 1.1f * k, x + 11 * k, y);
    star.CubicTo(x + 2.4f * k, y + 1.1f * k, x + 1.1f * k, y + 2.4f * k, x, y + 11 * k);
    star.CubicTo(x - 1.1f * k, y + 2.4f * k, x - 2.4f * k, y + 1.1f * k, x - 11 * k, y);
    star.CubicTo(x - 2.4f * k, y - 1.1f * k, x - 1.1f * k, y - 2.4f * k, x, y - 11 * k);
    star.Close();
    using var fill = new SKPaint { IsAntialias = true, Color = colour };
    canvas.DrawPath(star, fill);
}
foreach (var (sx, sy, sr) in new[] { (190f, 180f, 30f), (842f, 210f, 22f), (820f, 842f, 34f), (205f, 830f, 20f), (512f, 140f, 14f), (150f, 520f, 12f) })
{
    Sparkle(sx, sy, sr, C("#FFF3C4"));
}

// ---- golden glow behind the Octaquetra
using (var glow = new SKPaint { IsAntialias = true })
{
    glow.Shader = SKShader.CreateRadialGradient(new SKPoint(cx, cy), 360,
        new[] { gold.WithAlpha(95), gold.WithAlpha(30), gold.WithAlpha(0) },
        new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp);
    canvas.DrawCircle(cx, cy, 360, glow);
}

// ---- the Octaquetra of Inanna in glittering gold leaf, filled exactly as authored
const float OctaquetraRadius = 338f;
using var Octaquetra = SKPath.ParseSvgPathData(EmblemArt.InannaOctaquetraPath(cx, cy, OctaquetraRadius));
Octaquetra.FillType = SKPathFillType.EvenOdd;
var top = cy - OctaquetraRadius; var bottom = cy + OctaquetraRadius;

// soft shadow beneath it
using (var shadow = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 120),
           ImageFilter = SKImageFilter.CreateBlur(9, 9) })
{
    canvas.Save();
    canvas.Translate(0, 9);
    canvas.DrawPath(Octaquetra, shadow);
    canvas.Restore();
}

using (var leaf = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
{
    leaf.Shader = SKShader.CreateLinearGradient(new SKPoint(cx, top), new SKPoint(cx, bottom),
        new[] { C("#FBEA92"), C("#EDCB5A"), C("#C9962E"), C("#8A5A14") },
        new[] { 0f, 0.4f, 0.75f, 1f }, SKShaderTileMode.Clamp);
    canvas.DrawPath(Octaquetra, leaf);

    // glitter: fine light and dark specks confined to the gold
    canvas.Save();
    canvas.ClipPath(Octaquetra, antialias: true);
    var glitter = new Random(1224);
    using var speck = new SKPaint { IsAntialias = true };
    for (int i = 0; i < 9000; i++)
    {
        var x = cx - OctaquetraRadius + (float)glitter.NextDouble() * OctaquetraRadius * 2;
        var y = top + (float)glitter.NextDouble() * (bottom - top);
        var light = glitter.NextDouble() < 0.55;
        speck.Color = light
            ? new SKColor(255, 250, 225, (byte)glitter.Next(40, 150))
            : new SKColor(90, 55, 10, (byte)glitter.Next(30, 110));
        canvas.DrawCircle(x, y, (float)(0.6 + glitter.NextDouble() * 1.3), speck);
    }
    canvas.Restore();
}

canvas.Restore();

using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using (var fs = File.Create(output)) { data.SaveTo(fs); }
Console.WriteLine($"icon written: {Path.GetFullPath(output)}");
