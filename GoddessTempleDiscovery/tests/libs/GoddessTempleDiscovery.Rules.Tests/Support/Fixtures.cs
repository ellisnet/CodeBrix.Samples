using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Tests.Support;

/// <summary>Small catalogs of made-up cards, so the tests never depend on the content pass.</summary>
internal static class Fixtures
{
    internal const string MaskId = "mask-of-warka";

    internal static readonly SeasonEffect[] DesignEffects =
    {
        SeasonEffect.FreeSurvey, SeasonEffect.GainWorker, SeasonEffect.KassiteCheaper, SeasonEffect.DeepCheaper,
        SeasonEffect.RerollOnce, SeasonEffect.StudyDrawsTwo, SeasonEffect.RecruitCheaper, SeasonEffect.WorkersCapped,
        SeasonEffect.None, SeasonEffect.PublishBonus, SeasonEffect.FreeSurvey, SeasonEffect.FinalSeason,
    };

    internal static readonly string[] Years =
    {
        "1912/13", "1928/29", "1929/30", "1930/31", "1931/32", "1932/33",
        "1933/34", "1934/35", "1935/36", "1936/37", "1937/38", "1938/39",
    };

    private static readonly Period[][] TierPeriods =
    {
        Array.Empty<Period>(),
        new[] { Period.Seleucid, Period.Parthian },
        new[] { Period.NeoBabylonian, Period.Achaemenid },
        new[] { Period.Kassite, Period.OldBabylonian },
        new[] { Period.UrIII },
        new[] { Period.EarlyDynastic, Period.Akkadian },
        new[] { Period.JemdetNasr },
        new[] { Period.LateUruk },
        new[] { Period.LateUruk },
        new[] { Period.MiddleUruk, Period.EarlyUruk, Period.Ubaid },
    };

    private static readonly DiscoveryKind[] Kinds = Enum.GetValues<DiscoveryKind>();

    internal static DiscoveryCard Discovery(string id, int tier, DiscoveryKind kind = DiscoveryKind.Stratum, Period? period = null, bool starred = false) =>
        new DiscoveryCard(
            id,
            "Find " + id,
            kind,
            period ?? TierPeriods[tier][0],
            "Level of " + id,
            "about 3000 BCE",
            tier,
            starred,
            "art-" + id,
            string.Empty,
            string.Empty,
            "Card text of " + id + ".",
            "Long text of " + id + ".",
            "The test excavators",
            "winter 1930/31",
            "Test museum",
            "Source of " + id);

    internal static TabletCard Tablet(string id, TabletKind kind) =>
        new TabletCard(id, kind, "Tablet " + id, "art-" + id, string.Empty, string.Empty, "Card text of " + id + ".", "Long text of " + id + ".", string.Empty, string.Empty, string.Empty, "Source of " + id);

    internal static SpecialistCard Specialist(SpecialistRole role, int cost) =>
        new SpecialistCard(role, "The " + role, cost, "art-" + role, "What the " + role + " does.", "Who did it at Uruk.", "Source of " + role);

    internal static FavorCard Favor(FavorEffect effect) =>
        new FavorCard("favor-" + effect, "Favor " + effect, effect, "Gift " + effect + ".", "Fact " + effect + ".", "art-favor", "Source of favor " + effect);

    internal static SeasonCard Season(int index, SeasonEffect effect) =>
        new SeasonCard(index, Years[index % Years.Length], "Season " + index, "Director " + index, "Story " + index + ".", "Long story " + index + ".", effect, "Effect " + effect + ".", "art-season-" + index, "Source of season " + index);

    /// <summary>36 discoveries (four per tier, the kinds cycling, two starred, the Mask among tier 6), 12 tablets.</summary>
    internal static IReadOnlyList<DiscoveryCard> Discoveries()
    {
        var list = new List<DiscoveryCard>();
        var k = 0;
        for (var tier = 1; tier <= 9; tier++)
        {
            for (var n = 0; n < 4; n++)
            {
                var periods = TierPeriods[tier];
                if (tier == 6 && n == 0)
                {
                    list.Add(Discovery(MaskId, 6, DiscoveryKind.Object, Period.JemdetNasr, starred: true));
                    continue;
                }

                list.Add(Discovery(
                    string.Format(CultureInfo.InvariantCulture, "d-{0}-{1}", tier, n),
                    tier,
                    Kinds[k++ % Kinds.Length],
                    periods[n % periods.Length],
                    starred: tier == 7 && n < 2));
            }
        }

        return list;
    }

    internal static IReadOnlyList<TabletCard> Tablets(int perKind = 3) =>
        Enum.GetValues<TabletKind>()
            .SelectMany(kind => Enumerable.Range(0, perKind).Select(n => Tablet(string.Format(CultureInfo.InvariantCulture, "t-{0}-{1}", kind, n), kind)))
            .ToArray();

    internal static IReadOnlyList<SpecialistCard> Specialists(int copies = 1)
    {
        var costs = new Dictionary<SpecialistRole, int>
        {
            [SpecialistRole.Architect] = 4,
            [SpecialistRole.Epigrapher] = 4,
            [SpecialistRole.SmallFindsKeeper] = 3,
            [SpecialistRole.Photographer] = 3,
            [SpecialistRole.Foreman] = 5,
            [SpecialistRole.Surveyor] = 2,
        };
        return Enumerable.Range(0, copies).SelectMany(_ => costs.Select(c => Specialist(c.Key, c.Value))).ToArray();
    }

    internal static IReadOnlyList<TeamProfile> Teams() => new[]
    {
        new TeamProfile("team-reed", "Reed Gate Expedition", "art-reed", "#AA3322", "Motto one"),
        new TeamProfile("team-star", "Star of Evening Expedition", "art-star", "#2233AA", "Motto two"),
        new TeamProfile("team-lion", "Lion Expedition", "art-lion", "#22AA33", "Motto three"),
        new TeamProfile("team-boat", "Boat of Heaven Expedition", "art-boat", "#AA22AA", "Motto four"),
    };

    /// <summary>A catalog. The seasons get <paramref name="effects"/> (default: the design's twelve).</summary>
    internal static CatalogSnapshot Catalog(
        IReadOnlyList<SeasonEffect> effects = null,
        IReadOnlyList<DiscoveryCard> discoveries = null,
        IReadOnlyList<TabletCard> tablets = null,
        IReadOnlyList<SpecialistCard> specialists = null,
        IReadOnlyList<FavorCard> favors = null)
    {
        effects ??= DesignEffects;
        return new CatalogSnapshot(
            discoveries ?? Discoveries(),
            tablets ?? Tablets(),
            specialists ?? Specialists(),
            effects.Select((e, i) => Season(i, e)).ToArray(),
            favors ?? Enum.GetValues<FavorEffect>().Select(Favor).ToArray(),
            Teams(),
            MaskId);
    }

    /// <summary>Twelve seasons, the first ones as given, the rest without effect.</summary>
    internal static SeasonEffect[] Effects(params SeasonEffect[] first)
    {
        var effects = Enumerable.Repeat(SeasonEffect.None, 12).ToArray();
        Array.Copy(first, effects, first.Length);
        return effects;
    }

    internal static GameSetup Setup(int seats = 2, int seed = 7, int turnsPerSeason = 2, Difficulty difficulty = Difficulty.Standard, SeatKind kind = SeatKind.Computer, Temperament[] temperaments = null)
    {
        var teams = Teams();
        var list = Enumerable.Range(0, seats)
            .Select(i => new SeatSetup(string.Empty, teams[i % teams.Count].Id, kind, temperaments == null ? (Temperament)(i % 3) : temperaments[i % temperaments.Length]))
            .ToArray();
        return new GameSetup(list, turnsPerSeason, difficulty, seed);
    }

    /// <summary>An engine on a catalog whose seasons have no effect unless given, and with no Favors (so doubles change nothing).</summary>
    internal static GameEngine Engine(SeasonEffect[] effects = null, int seats = 2, int seed = 7, Difficulty difficulty = Difficulty.Standard, int turnsPerSeason = 2, CatalogSnapshot catalog = null) =>
        new GameEngine(Setup(seats, seed, turnsPerSeason, difficulty), catalog ?? Catalog(effects ?? Effects(), favors: Array.Empty<FavorCard>()));
}
