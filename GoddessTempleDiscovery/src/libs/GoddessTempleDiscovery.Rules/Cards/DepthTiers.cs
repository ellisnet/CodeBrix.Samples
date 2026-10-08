using System;

namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>
/// The nine depth tiers of the Site deck: the deeper the layer, the higher its Dig Number and the more points
/// its discovery is worth (DESIGN.md section 6).
/// </summary>
public static class DepthTiers
{
    /// <summary>The shallowest tier (Seleucid, Parthian, the surface).</summary>
    public const int Shallowest = 1;

    /// <summary>The deepest tier (the deep sounding below Level V, down to the seabed).</summary>
    public const int Deepest = 9;

    private static readonly int[] DigNumbers = { 0, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
    private static readonly int[] PointValues = { 0, 1, 1, 2, 2, 3, 3, 4, 5, 6 };
    private static readonly string[] Names =
    {
        string.Empty,
        "Seleucid and Parthian surface",
        "Neo-Babylonian and Achaemenid",
        "Kassite and Old Babylonian",
        "Third Dynasty of Ur",
        "Early Dynastic and Akkadian",
        "Jemdet Nasr, Level III",
        "Uruk IV",
        "Uruk V",
        "The deep sounding",
    };

    private static readonly string[] ShortNames =
    {
        string.Empty,
        "SELEUCID / PARTHIAN",
        "NEO-BABYLONIAN",
        "KASSITE / OLD BAB.",
        "UR III",
        "EARLY DYNASTIC",
        "JEMDET NASR",
        "URUK IV",
        "URUK V",
        "DEEP SOUNDING",
    };

    /// <summary>The Dig Number a die, a sum or a modified total must reach to excavate a site of this tier.</summary>
    /// <param name="tier">A tier from <see cref="Shallowest"/> to <see cref="Deepest"/>.</param>
    public static int DigNumber(int tier) => DigNumbers[Check(tier)];

    /// <summary>The points a discovery of this tier is worth.</summary>
    /// <param name="tier">A tier from <see cref="Shallowest"/> to <see cref="Deepest"/>.</param>
    public static int Points(int tier) => PointValues[Check(tier)];

    /// <summary>The name of the layers a tier stands for.</summary>
    /// <param name="tier">A tier from <see cref="Shallowest"/> to <see cref="Deepest"/>.</param>
    public static string Name(int tier) => Names[Check(tier)];

    /// <summary>
    /// A short capitalized name of the layers a tier stands for, for labels too narrow for <see cref="Name"/>, such as
    /// "KASSITE / OLD BAB." (the full name stays in the inspector and the journal).
    /// </summary>
    /// <param name="tier">A tier from <see cref="Shallowest"/> to <see cref="Deepest"/>.</param>
    public static string ShortName(int tier) => ShortNames[Check(tier)];

    private static int Check(int tier)
    {
        if (tier < Shallowest || tier > Deepest)
        {
            throw new ArgumentOutOfRangeException(nameof(tier), tier, "Tiers run from 1 to 9.");
        }

        return tier;
    }
}
