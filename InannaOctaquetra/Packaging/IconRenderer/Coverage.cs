using System.Text.Json;
using SkiaSharp;

internal static class Coverage
{
    public static void Run(string json, params string[] fonts)
    {
        var doc = JsonDocument.Parse(File.ReadAllText(json));
        var chars = new SortedSet<string>();
        foreach (var line in doc.RootElement.EnumerateArray())
            foreach (var item in line.GetProperty("items").EnumerateArray())
                if (item.TryGetProperty("c", out var c)) chars.Add(c.GetString()!);
        foreach (var f in fonts)
        {
            using var tf = SKTypeface.FromFile(f);
            using var font = new SKFont(tf, 40);
            var missing = chars.Where(ch => font.GetGlyphs(ch)[0] == 0).ToList();
            Console.WriteLine($"{Path.GetFileName(f)}: {chars.Count} signs, missing {missing.Count}: {string.Join(" ", missing.Select(m => $"{m} U+{char.ConvertToUtf32(m, 0):X}"))}");
        }
    }
}
