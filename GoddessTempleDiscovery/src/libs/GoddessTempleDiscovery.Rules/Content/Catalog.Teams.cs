using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly TeamProfile[] TeamProfiles =
    {
        // Motto from sheet 21 entry Q36 (Heinrich in Jordan 1932, p. 19)
        new TeamProfile(
            Id: "reed-marsh-expedition",
            Name: "The Reed Marsh Expedition",
            ArtKey: "team-reed-marsh",
            Colour: "#2A4B8D",
            Motto: "Dwelling-places of reeds"),

        // Motto from sheet 21 entry Q46 (Heinrich 1936, Kleinfunde)
        new TeamProfile(
            Id: "lapis-road-society",
            Name: "The Lapis Road Society",
            ArtKey: "team-lapis-road",
            Colour: "#B8322A",
            Motto: "From the lands far beyond the Persian mountains"),

        // Motto from sheet 21 entry Q85 (Selz, Getty 2019 ch. 39)
        new TeamProfile(
            Id: "morning-star-mission",
            Name: "The Morning Star Mission",
            ArtKey: "team-morning-star",
            Colour: "#D9A441",
            Motto: "The gleaming, rising Inanna"),

        // Motto from sheet 21 entry Q88 (Selz, Getty 2019 ch. 39)
        new TeamProfile(
            Id: "date-palm-trust",
            Name: "The Date Palm Trust",
            ArtKey: "team-date-palm",
            Colour: "#7A8A3A",
            Motto: "The first fruits"),

        // Motto from sheet 21 entry Q26 (Jordan 1932, pp. 32-33)
        new TeamProfile(
            Id: "gatepost-fellowship",
            Name: "The Gatepost Fellowship",
            ArtKey: "team-gatepost",
            Colour: "#9C6B3C",
            Motto: "Her symbol, the ring-bundle"),

        // Motto from sheet 21 entry Q01 (Enheduanna, lines 123-24)
        new TeamProfile(
            Id: "lion-of-uruk-society",
            Name: "The Lion of Uruk Society",
            ArtKey: "team-lion-of-uruk",
            Colour: "#2B2622",
            Motto: "Greatly exalted like heaven"),

        // Motto from sheet 21 entry Q07 (Jordan 1932, p. 23)
        new TeamProfile(
            Id: "clay-tablet-circle",
            Name: "The Clay Tablet Circle",
            ArtKey: "team-clay-tablet",
            Colour: "#5B7BC4",
            Motto: "When does culture begin at Uruk?"),

        // Motto from sheet 21 entry B01 (Eanna, Selz, Getty 2019 ch. 39)
        new TeamProfile(
            Id: "great-court-foundation",
            Name: "The Great Court Foundation",
            ArtKey: "team-great-court",
            Colour: "#C9BFA8",
            Motto: "House of Heaven"),
    };
}
