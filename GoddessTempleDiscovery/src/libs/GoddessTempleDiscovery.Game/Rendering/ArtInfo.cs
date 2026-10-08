using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Assets;

namespace GoddessTempleDiscovery.Game.Rendering;

/// <summary>
/// What the gallery shows about one picture of the <see cref="ArtCatalog"/>: its gallery category and the Title,
/// Subject and Sources lines of its SVG header comment.
/// </summary>
/// <param name="Key">The art key.</param>
/// <param name="Category">The gallery category (see <see cref="ArtInfo.Categories"/>).</param>
/// <param name="Title">The header's Title line (the key when there is none).</param>
/// <param name="Subject">The header's Subject line (may be empty).</param>
/// <param name="Sources">The header's "Sources consulted" line (may be empty).</param>
public sealed record ArtInfo(string Key, string Category, string Title, string Subject, string Sources)
{
    /// <summary>The gallery's categories, in order: the catalog's folders, with plans and icons set apart.</summary>
    public static readonly IReadOnlyList<string> Categories =
        new[] { "buildings", "plans", "objects", "symbols", "icons", "deities", "scenes", "plates", "chrome", "deco" };

    /// <summary>The gallery category of a key: its catalog folder, with plan- and icon- pictures set apart.</summary>
    /// <param name="key">The art key.</param>
    /// <returns>The category.</returns>
    public static string CategoryOf(string key)
    {
        if (key.StartsWith("plan-", StringComparison.Ordinal))
        {
            return "plans";
        }

        if (key.StartsWith("icon-", StringComparison.Ordinal))
        {
            return "icons";
        }

        return ArtCatalog.CategoryOf(key);
    }

    /// <summary>Reads one picture's header.</summary>
    /// <param name="key">The art key.</param>
    /// <returns>The information.</returns>
    public static ArtInfo Read(string key) => Parse(key, ArtCatalog.ReadSvg(key));

    /// <summary>Parses the header comment of SVG text.</summary>
    /// <param name="key">The art key.</param>
    /// <param name="svg">The SVG text.</param>
    /// <returns>The information.</returns>
    public static ArtInfo Parse(string key, string svg)
    {
        string title = null, subject = null, sources = null;
        var start = svg?.IndexOf("<!--", StringComparison.Ordinal) ?? -1;
        var end = start < 0 ? -1 : svg.IndexOf("-->", start, StringComparison.Ordinal);
        if (start >= 0 && end > start)
        {
            foreach (var raw in svg.Substring(start + 4, end - start - 4).Split('\n'))
            {
                var line = raw.Trim();
                title ??= After(line, "Title:");
                subject ??= After(line, "Subject:");
                sources ??= After(line, "Sources consulted:");
            }
        }

        return new ArtInfo(key, CategoryOf(key), title ?? key, subject ?? string.Empty, sources ?? string.Empty);
    }

    /// <summary>Every picture of the catalog, in category order then by key.</summary>
    /// <returns>The pictures.</returns>
    public static IReadOnlyList<ArtInfo> All() =>
        ArtCatalog.Keys.Select(Read)
            .OrderBy(a => Categories.ToList().IndexOf(a.Category) is var i && i < 0 ? int.MaxValue : i)
            .ThenBy(a => a.Key, StringComparer.Ordinal)
            .ToArray();

    private static string After(string line, string label) =>
        line.StartsWith(label, StringComparison.Ordinal) ? line.Substring(label.Length).Trim() : null;
}
