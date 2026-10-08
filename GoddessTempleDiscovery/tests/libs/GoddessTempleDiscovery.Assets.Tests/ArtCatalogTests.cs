using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Assets.Tests;

public class ArtCatalogTests
{
    private static readonly string[] Categories = { "buildings", "objects", "symbols", "deities", "scenes", "plates", "chrome", "deco" };

    private static readonly HashSet<string> ForbiddenElements = new(StringComparer.Ordinal)
    {
        "text", "image", "style", "script", "filter", "use", "foreignObject", "pattern", "mask", "clipPath", "tspan", "textPath", "animate", "set",
    };

    private static readonly HashSet<string> Palette = new(StringComparer.OrdinalIgnoreCase)
    {
        "#1E1A17", "#C8955A", "#9C6B3C", "#E8C597", "#EDE6D6", "#C9BFA8", "#B8322A", "#2B2622", "#F4EFE3", "#2A4B8D",
        "#5B7BC4", "#D9A441", "#A8761F", "#7A8A3A", "#4E5A22", "#EFE3C8", "#16213A", "#BFC3C7", "#FFFFFF",
    };

    public static IEnumerable<object[]> EveryKey() => ArtCatalog.Keys.Select(key => new object[] { key });

    [Fact]
    public void Catalog_holds_every_category_and_a_large_set()
    {
        ArtCatalog.Keys.Count.Should().BeGreaterThan(200);
        foreach (var category in Categories.Where(c => c != "deco"))
        {
            ArtCatalog.Keys.Count(key => ArtCatalog.CategoryOf(key) == category).Should().BeGreaterThan(5, $"category {category}");
        }
    }

    [Fact]
    public void Keys_are_sorted_unique_kebab_case()
    {
        var keys = ArtCatalog.Keys;
        keys.Should().BeEquivalentTo(keys.Distinct(StringComparer.Ordinal));
        keys.Should().BeEquivalentTo(keys.OrderBy(k => k, StringComparer.Ordinal));
        foreach (var key in keys)
        {
            Regex.IsMatch(key, "^[a-z0-9]+(-[a-z0-9]+)*$").Should().BeTrue(key);
        }
    }

    [Theory]
    [MemberData(nameof(EveryKey))]
    public void Every_picture_is_a_self_contained_palette_svg(string key)
    {
        var svg = ArtCatalog.ReadSvg(key);
        svg.Length.Should().BeLessThan(60 * 1024 + 1, key);
        svg.Should().Contain("Goddess Temple Discovery vector art");
        svg.Should().Contain("License: Apache License 2.0");
        svg.Should().NotContain("href");

        var document = XDocument.Parse(svg);
        document.Root.Name.LocalName.Should().Be("svg");
        var viewBox = document.Root.Attribute("viewBox")?.Value;
        new[] { "0 0 400 400", "0 0 400 560", "0 0 250 400" }.Should().Contain(viewBox);

        foreach (var element in document.Descendants())
        {
            ForbiddenElements.Should().NotContain(element.Name.LocalName, $"{key} uses <{element.Name.LocalName}>");
        }

        foreach (Match match in Regex.Matches(svg, "#[0-9a-fA-F]{6}\\b"))
        {
            Palette.Should().Contain(match.Value, $"{key} uses {match.Value}");
        }
    }

    [Fact]
    public void Contains_and_category_answer_for_known_keys()
    {
        ArtCatalog.Contains("symbol-eight-pointed-star").Should().BeTrue();
        ArtCatalog.CategoryOf("symbol-eight-pointed-star").Should().Be("symbols");
        ArtCatalog.Contains("object-mask-of-warka").Should().BeTrue();
        ArtCatalog.CategoryOf("building-limestone-temple").Should().Be("buildings");
        ArtCatalog.Contains("no-such-picture").Should().BeFalse();
        ArtCatalog.Contains(null).Should().BeFalse();
    }

    [Fact]
    public void Unknown_keys_throw_a_key_not_found()
    {
        Action open = () => ArtCatalog.Open("no-such-picture");
        open.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Open_returns_a_fresh_caller_owned_stream_each_time()
    {
        using var first = ArtCatalog.Open("symbol-lion");
        using var second = ArtCatalog.Open("symbol-lion");
        ReferenceEquals(first, second).Should().BeFalse();
        new StreamReader(first).ReadToEnd().Should().Be(new StreamReader(second).ReadToEnd());
    }

    [Fact]
    public void License_text_names_the_license()
    {
        ArtCatalog.LicenseText.Should().Contain("Apache License, Version 2.0");
    }
}
