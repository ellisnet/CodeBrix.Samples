using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly SpecialistCard[] SpecialistCards =
    {
        // Sheet 18 Part A, entry A1; Part B, entry B2 (Preusser, architect of 1912/13)
        new SpecialistCard(
            Role: SpecialistRole.Architect,
            Title: "Architect",
            Cost: 4,
            ArtKey: "specialist-architect",
            CardText: "Adds 1 to every dig on a building site.",
            HistoricalNote: "In 1912/13 the architect Conrad Preusser led the first campaign with Julius Jordan. Together they drew the first accurate map of Uruk.",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 12.5; sheet 18 entries A1, B2"),

        // Sheet 18 Part B, entry B1 (Jordan, the architect-excavator)
        new SpecialistCard(
            Role: SpecialistRole.Architect,
            Title: "Architect",
            Cost: 4,
            ArtKey: "specialist-architect",
            CardText: "Adds 1 to every dig on a building site.",
            HistoricalNote: "Julius Jordan trained as an architect in Dresden. As director from 1928/29 to 1930/31 he led the digging as an architect-excavator, from the deep trench to the Limestone Temple and the Deep Temples.",
            Sources: "Getty 2019 ch. 12-13 (van Ess); Jordan 1932 (UVB III); sheet 18 entries A5, B1"),

        // Sheet 18 Part B, entry B4 (Heinrich)
        new SpecialistCard(
            Role: SpecialistRole.Architect,
            Title: "Architect",
            Cost: 4,
            ArtKey: "specialist-architect",
            CardText: "Adds 1 to every dig on a building site.",
            HistoricalNote: "Ernst Heinrich was Jordan's architect on the Level V buildings from 1929 to 1931. He then led the Anu ziggurat dig and, in 1933/34, the whole excavation.",
            Sources: "Getty 2019 ch. 13 (van Ess); Limestone Temple overview; sheet 18 entry B4"),

        // Sheet 18 Part B, entry B6 (Falkenstein from 1928)
        new SpecialistCard(
            Role: SpecialistRole.Epigrapher,
            Title: "Epigrapher",
            Cost: 5,
            ArtKey: "specialist-epigrapher",
            CardText: "Adds 1 to every dig on an inscription site. When you Study, draw two Tablets.",
            HistoricalNote: "Adam Falkenstein read the cuneiform tablets found at Uruk; the library's lists place him on the team from 1928 onward. He became a leading expert on the archaic Uruk texts and on Sumerian.",
            Sources: "EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 6; texts/10; story/13 catalog; sheet 18 entry B6"),

        // Sheet 18 Part B, entry B8 (Albert Schott)
        new SpecialistCard(
            Role: SpecialistRole.Epigrapher,
            Title: "Epigrapher",
            Cost: 5,
            ArtKey: "specialist-epigrapher",
            CardText: "Adds 1 to every dig on an inscription site. When you Study, draw two Tablets.",
            HistoricalNote: "The Assyriologist Albert Schott was at Uruk in 1928/29 and 1938/39. His study of the Akkadian word sahuru shaped Jordan's reading of the Deep Temples.",
            Sources: "EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 10; Jordan 1932 (UVB III) p. 24; sheet 18 entry B8"),

        // Sheet 21 Part A, entry Q03 (Shulgi's foundation tablet, read by Heinrich)
        new SpecialistCard(
            Role: SpecialistRole.Epigrapher,
            Title: "Epigrapher",
            Cost: 5,
            ArtKey: "specialist-epigrapher",
            CardText: "Adds 1 to every dig on an inscription site. When you Study, draw two Tablets.",
            HistoricalNote: "In his Tenth Preliminary Report (1939) Ernst Heinrich read King Shulgi's foundation tablet, find W 17304: “For Inanna, lady of Eanna, his lady, Shulgi, the mighty man, king of Ur.”",
            Sources: "Heinrich, UVB X (1939); BUILDING_ARCHEOLOGICAL_QUOTES (Limestone Temple Q12); sheet 21 entry Q03"),

        // Sheet 18 Part B, entry B4 (Heinrich's 1936 small-finds catalogue)
        new SpecialistCard(
            Role: SpecialistRole.SmallFindsKeeper,
            Title: "Small-Finds Keeper",
            Cost: 3,
            ArtKey: "specialist-small-finds-keeper",
            CardText: "Adds 1 to every dig on an object or deposit site.",
            HistoricalNote: "In 1936 Ernst Heinrich published Small Finds from the Archaic Temple Levels (Kleinfunde), the catalogue of the little objects from Eanna's oldest temples.",
            Sources: "Getty 2019 ch. 13 (van Ess); Heinrich 1936; sheet 18 entry B4"),

        // Sheet 18 Part B, entry B7 (von Haller and the pottery)
        new SpecialistCard(
            Role: SpecialistRole.SmallFindsKeeper,
            Title: "Small-Finds Keeper",
            Cost: 3,
            ArtKey: "specialist-small-finds-keeper",
            CardText: "Adds 1 to every dig on an object or deposit site.",
            HistoricalNote: "Arndt von Haller, documented at Uruk in 1930/31 and 1931/32, co-wrote the Fourth Preliminary Report (1932); the library's catalog credits him with the pottery.",
            Sources: "texts/10; story/13 catalog; EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 8; sheet 18 entry B7"),

        // Sheet 18 Part A, entries A3 and A11 (the division of finds)
        new SpecialistCard(
            Role: SpecialistRole.SmallFindsKeeper,
            Title: "Small-Finds Keeper",
            Cost: 3,
            ArtKey: "specialist-small-finds-keeper",
            CardText: "Adds 1 to every dig on an object or deposit site.",
            HistoricalNote: "Every find got a number, such as W 14873 for the Warka Vase. Under the division of finds (Fundteilung), part of Karaindash's 1928/29 façade went to Berlin and part to the Iraq Museum, until a 1936 Iraqi law changed the rules.",
            Sources: "BUILDING_ARCHEOLOGICAL_DRAMA (Karaindash); IRAQI_SCHOLARS_URUK_REPORT sec. C.7; sheet 18 entries A3, A11, C7"),

        // Sheet 18 Part C, entries C4 and C7 (the photo studio and field photo laboratory)
        new SpecialistCard(
            Role: SpecialistRole.Photographer,
            Title: "Photographer",
            Cost: 4,
            ArtKey: "specialist-photographer",
            CardText: "Adds 1 to every report you publish.",
            HistoricalNote: "By 1931/32 the dig house had a photo studio (Photo Atelier). Its historic field photo laboratory still stands in the house today.",
            Sources: "Getty 2019 fig. 66.1; texts/07; sheet 18 entries C4, C7"),

        // Sheet 18 Part A, entry A1 (the 1912 first map and camp photograph)
        new SpecialistCard(
            Role: SpecialistRole.Photographer,
            Title: "Photographer",
            Cost: 4,
            ArtKey: "specialist-photographer",
            CardText: "Adds 1 to every report you publish.",
            HistoricalNote: "The record began in 1912: Jordan and Preusser made the first accurate map of Uruk, and a photograph of that year shows a reed hut that local workers built for the camp.",
            Sources: "Getty 2019 ch. 12 (van Ess), figs. 12.5, 15.3; sheet 18 entries A1, B16"),

        // Sheet 18 Part B, entry B2 (Preusser's watercolours)
        new SpecialistCard(
            Role: SpecialistRole.Photographer,
            Title: "Photographer",
            Cost: 4,
            ArtKey: "specialist-photographer",
            CardText: "Adds 1 to every report you publish.",
            HistoricalNote: "Before color photographs, Conrad Preusser painted. His 1913 watercolour rebuilds the glazed bricks of the Resh Sanctuary, with stars, lions and lion-griffins.",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 12.6; sheet 18 entries A1, B2"),

        // Sheet 18 Part B, entry B15 (the foremen from Babylon and Hilla)
        new SpecialistCard(
            Role: SpecialistRole.Foreman,
            Title: "Foreman",
            Cost: 2,
            ArtKey: "specialist-foreman",
            CardText: "Adds 1 to any one dig each turn.",
            HistoricalNote: "In 1912/13 foremen and workers from Babylon and Hilla ran the camp. They hired bedouin of the Tobe and Istshey tribes and held the talks with the two sheikhs.",
            Sources: "Getty 2019 ch. 12 (van Ess); sheet 18 entries A1, B15"),

        // Sheet 18 Part A, entry A6 (the mudbrick lads and the awl)
        new SpecialistCard(
            Role: SpecialistRole.Foreman,
            Title: "Foreman",
            Cost: 2,
            ArtKey: "specialist-foreman",
            CardText: "Adds 1 to any one dig each turn.",
            HistoricalNote: "In 1931/32 Nöldeke's mudbrick lads (Libn-Jungs) found melted mud walls with a long awl, feeling for set brick, then scraped out the mortar by hand so the bricks showed.",
            Sources: "van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; sheet 18 entries A6, C7"),

        // Sheet 18 Part B, entry B15 (Ismael and son)
        new SpecialistCard(
            Role: SpecialistRole.Foreman,
            Title: "Foreman",
            Cost: 2,
            ArtKey: "specialist-foreman",
            CardText: "Adds 1 to any one dig each turn.",
            HistoricalNote: "A 1931/32 sketch of the dig house marks a room for “Ismael and son” beside the huts of the workers from Babylon. His job is not recorded; the reports almost never name their Iraqi workers.",
            Sources: "Getty 2019 fig. 66.1; IRAQI_SCHOLARS_URUK_REPORT secs. E.4, F; sheet 18 entry B15"),

        // Sheet 18 Part A, entry A1 (the 1912/13 topographical plan)
        new SpecialistCard(
            Role: SpecialistRole.Surveyor,
            Title: "Surveyor",
            Cost: 3,
            ArtKey: "specialist-surveyor",
            CardText: "Once each turn, Survey for free.",
            HistoricalNote: "The first task of 1912/13 was a topographical plan of the whole city, the first accurate map of Uruk.",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 12.5; sheet 18 entry A1"),

        // Sheet 18 Part A, entry A9 (Goepner's grid)
        new SpecialistCard(
            Role: SpecialistRole.Surveyor,
            Title: "Surveyor",
            Cost: 3,
            ArtKey: "specialist-surveyor",
            CardText: "Once each turn, Survey for free.",
            HistoricalNote: "In 1934/35 W. Goepner surveyed the grid points of Eanna. Every later plan of Eanna is drawn on his grid.",
            Sources: "Eichmann Plan 73 caption, images/temple_buildings/overview/_NEW_ADDITIONS_2026-05-09.md; sheet 18 entry A9"),

        // Sheet 18 Part C, entry C7 (the lorry track)
        new SpecialistCard(
            Role: SpecialistRole.Surveyor,
            Title: "Surveyor",
            Cost: 3,
            ArtKey: "specialist-surveyor",
            CardText: "Once each turn, Survey for free.",
            HistoricalNote: "In 1912/13 the team laid the lorry track, a light railway for carts of earth. Its line is still a distinctive feature of the dig area.",
            Sources: "Getty 2019 fig. 12.1; sheet 18 entries A1, C7"),
    };
}
