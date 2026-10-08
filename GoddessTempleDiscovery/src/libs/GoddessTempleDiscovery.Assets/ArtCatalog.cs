using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GoddessTempleDiscovery.Assets;

/// <summary>
/// The game's vector art: every SVG travels inside this assembly as an embedded resource and is reached by its
/// key, the file name without its extension (for example <c>building-limestone-temple</c>). The art is original
/// work made for this application and is licensed under the Apache License 2.0 like the rest of the sample, so
/// anyone may reuse it; <see cref="LicenseText"/> is the notice that travels with it.
/// </summary>
public static class ArtCatalog
{
    private const string Prefix = "GoddessTempleDiscovery.Assets.Art.";
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Index = new(BuildIndex);

    /// <summary>Every art key, sorted.</summary>
    public static IReadOnlyList<string> Keys => Index.Value.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();

    /// <summary>Whether <paramref name="key"/> names an embedded picture.</summary>
    /// <param name="key">The key, such as <c>symbol-eight-pointed-star</c>.</param>
    public static bool Contains(string key) => key != null && Index.Value.ContainsKey(key);

    /// <summary>The category folder a key lives in: buildings, objects, symbols, deities, scenes, plates, chrome or deco.</summary>
    /// <param name="key">The key.</param>
    public static string CategoryOf(string key)
    {
        var name = Index.Value[Check(key)];
        var inner = name.Substring(Prefix.Length);
        return inner.Substring(0, inner.IndexOf('.'));
    }

    /// <summary>Opens the SVG text of a picture as a caller-owned stream.</summary>
    /// <param name="key">The key.</param>
    public static Stream Open(string key)
    {
        var name = Index.Value[Check(key)];
        return typeof(ArtCatalog).Assembly.GetManifestResourceStream(name)
               ?? throw new InvalidOperationException($"The art resource '{name}' is missing from the assembly.");
    }

    /// <summary>Reads the SVG text of a picture.</summary>
    /// <param name="key">The key.</param>
    public static string ReadSvg(string key)
    {
        using var stream = Open(key);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>The license notice for the art.</summary>
    public const string LicenseText =
        "The vector art of Goddess Temple Discovery (every picture under Art/) is original work made for this " +
        "application and is licensed under the Apache License, Version 2.0, the license of the CodeBrix.Samples " +
        "repository. Traced plates name their public-domain source in their header comment. You may use the art " +
        "in your own projects under that license; keep the notice.";

    private static string Check(string key)
    {
        if (key == null || !Index.Value.ContainsKey(key))
        {
            throw new KeyNotFoundException($"No art is embedded under the key '{key}'.");
        }

        return key;
    }

    private static IReadOnlyDictionary<string, string> BuildIndex()
    {
        var index = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in typeof(ArtCatalog).Assembly.GetManifestResourceNames())
        {
            if (!name.StartsWith(Prefix, StringComparison.Ordinal) || !name.EndsWith(".svg", StringComparison.Ordinal))
            {
                continue;
            }

            //Art.<category>.<key>.svg: the key may not contain a dot, so the last dot before ".svg" ends the category
            var inner = name.Substring(Prefix.Length, name.Length - Prefix.Length - 4);
            var dot = inner.IndexOf('.');
            var key = dot < 0 ? inner : inner.Substring(dot + 1);
            index[key] = name;
        }

        return index;
    }
}
