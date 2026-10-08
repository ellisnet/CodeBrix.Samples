using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly PeriodInfo[] PeriodInfos =
    {
        // Sheet 20 Part A, entry A1 (Ubaid period)
        new PeriodInfo(
            Period: Period.Ubaid,
            Name: "Ubaid period",
            Years: "sixth to fifth millennium BCE",
            Levels: "XVIII to XVI",
            ArtKey: "period-ubaid",
            Summary: "The first people at Uruk live in a marsh. They lay down thick layers of cut reeds to make dry ground, and they build their houses on top. Archaeologists found these reed floors almost 20 metres below the later temples of Holy Inanna.",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess); ch. 14; ch. 16 (Eichmann); sheet 20 entry A1"),

        // Sheet 20 Part A, entry A2 (Ubaid transitional period)
        new PeriodInfo(
            Period: Period.UbaidTransition,
            Name: "Ubaid transitional period",
            Years: "end of the fifth millennium BCE",
            Levels: "XV to XIII",
            ArtKey: "period-ubaid-transition",
            Summary: "Long before writing, Uruk is a village in the reeds. People use reeds to raise the ground of the settlement, and they make small clay figures by hand and paint them. What we know of this time comes from one narrow, deep trench.",
            Sources: "Getty 2019 Chronological Table; ch. 14, fig. 14.3; ch. 15 (van Ess and Neef); sheet 20 entry A2"),

        // Sheet 20 Part A, entry A3 (Early Uruk period)
        new PeriodInfo(
            Period: Period.EarlyUruk,
            Name: "Early Uruk period",
            Years: "early in the Uruk period (the whole period is about 4000 to 3300 BCE; the table gives no separate years)",
            Levels: "XII to X",
            ArtKey: "period-early-uruk",
            Summary: "Uruk is probably two small villages facing each other across the Euphrates River, the later Anu (Kullab) district and the Eanna district. Only about ten other villages stand nearby. The town is likely no bigger than 10 to 15 hectares.",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 37 (van Ess); sheet 20 entry A3"),

        // Sheet 20 Part A, entry A4 (Middle Uruk period)
        new PeriodInfo(
            Period: Period.MiddleUruk,
            Name: "Middle Uruk period",
            Years: "the middle of the Uruk period; cone mosaic begins about 3600 BCE",
            Levels: "IX to VII",
            ArtKey: "period-middle-uruk",
            Summary: "Many new villages spring up around the city as the swamps slowly dry and open more land to farm. The number of settlements grows tenfold. Builders begin decorating walls with mosaics of little clay cones, and cylinder seals appear.",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 17 (van Ess), note 1 (Eichmann 2007); sheet 20 entry A4"),

        // Sheet 20 Part A, entry A5 (Late Uruk period)
        new PeriodInfo(
            Period: Period.LateUruk,
            Name: "Late Uruk period",
            Years: "about 3500 to 3300 BCE",
            Levels: "VI to IV",
            ArtKey: "period-late-uruk",
            Summary: "Eanna fills with giant buildings covered in colorful cone mosaics, among them the Limestone Temple (Kalksteintempel). Here people first write things down, on clay tablets, to keep track of goods; about 2,000 of the oldest tablets in Mesopotamia come from Uruk. The Uruk Vase and the Lady of Warka belong to this time.",
            Sources: "Getty 2019 Chronological Table; ch. 11, fig. 11.1; ch. 12 (van Ess); ch. 14; ch. 16 (Eichmann); ch. 39 (Selz); sheet 20 entry A5"),

        // Sheet 20 Part A, entry A6 (Jemdet Nasr period)
        new PeriodInfo(
            Period: Period.JemdetNasr,
            Name: "Jemdet Nasr period",
            Years: "about 3300 to 3000 BCE",
            Levels: "III",
            ArtKey: "period-jemdet-nasr",
            Summary: "The old giant buildings of Eanna are torn down and the ground is flattened and paved. In their place, builders raise a high terrace with cone-mosaic walls. Over many centuries this terrace grows into Holy Inanna's ziggurat.",
            Sources: "Getty 2019 Chronological Table; ch. 16 (Eichmann); ch. 39 (Selz); sheet 20 entry A6"),

        // Sheet 20 Part A, entry A7 (Early Dynastic periods I to III)
        new PeriodInfo(
            Period: Period.EarlyDynastic,
            Name: "Early Dynastic periods I to III",
            Years: "about 3000 to 2340 BCE",
            Levels: "I",
            ArtKey: "period-early-dynastic",
            Summary: "Uruk is at its biggest. A wall about 9 kilometres long, with about 900 towers, goes around the whole city. In Eanna, a new square terrace for the temple stands in a huge courtyard.",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 16 (Eichmann); ch. 35; sheet 20 entry A7"),

        // Sheet 20 Part A, entry A8 (Akkadian period)
        new PeriodInfo(
            Period: Period.Akkadian,
            Name: "Akkadian period",
            Years: "about 2340 to 2200 BCE",
            Levels: "",
            ArtKey: "period-akkadian",
            Summary: "King Sargon of Akkad conquers the cities of Sumer and makes the first united empire in Mesopotamia. He takes the worship of Holy Inanna to his own new capital. Uruk is still big, but it is no longer the most important city.",
            Sources: "Getty 2019 Chronological Table; ch. 35 (which ends the period about 2180 BCE); sheet 20 entry A8"),

        // Sheet 20 Part A, entry A9 (Third Dynasty of Ur)
        new PeriodInfo(
            Period: Period.UrIII,
            Name: "Third Dynasty of Ur",
            Years: "about 2112 to 2004 BCE",
            Levels: "",
            ArtKey: "period-ur-iii",
            Summary: "King Ur-Nammu builds a giant stepped tower, a ziggurat, for Holy Inanna at Eanna. He is the first king to stamp his building inscription on its bricks, and only from his time is the name Eanna certain. Its mud-brick core still stands at Uruk today.",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 37 (van Ess); Ziggurat of Ur-Nammu overview; sheet 20 entry A9"),

        // Sheet 20 Part A, entry A10 (Old Babylonian period)
        new PeriodInfo(
            Period: Period.OldBabylonian,
            Name: "Old Babylonian period",
            Years: "about 2025 to 1595 BCE (about 100 years of the dating are disputed)",
            Levels: "",
            ArtKey: "period-old-babylonian",
            Summary: "King Sin-kashid rules Uruk from a great palace of baked brick and probably repairs the walls and courtyards around Holy Inanna's ziggurat. Then Hammurabi of Babylon unites the land. Not long afterward, Uruk is almost empty for a few hundred years.",
            Sources: "Getty 2019 Chronological Table; ch. 1; ch. 12 (van Ess); ch. 37; sheet 20 entry A10"),

        // Sheet 20 Part A, entry A11 (Kassite / Middle Babylonian period)
        new PeriodInfo(
            Period: Period.Kassite,
            Name: "Kassite (Middle Babylonian) period",
            Years: "about 1650 to 1157 BCE",
            Levels: "",
            ArtKey: "period-kassite",
            Summary: "The Kassite king Karaindash builds a small temple for Holy Inanna, about 1420 BCE; the Getty book says the end of the 15th century BCE. Its wall shows gods and goddesses of molded baked brick, each pouring water from a jar. Part of that wall now stands in a museum in Berlin.",
            Sources: "Getty 2019 Chronological Table; ch. 35, fig. 35.5; ch. 37; BUILDING_HIGHLIGHTS (Karaindash's Temple); sheet 20 entry A11"),

        // Sheet 20 Part A, entry A12 (Second Dynasty of Isin and the Assyrian kings)
        new PeriodInfo(
            Period: Period.IsinIIAndAssyria,
            Name: "Second Dynasty of Isin and the Assyrian kings",
            Years: "1125 to 625 BCE",
            Levels: "",
            ArtKey: "period-isin-ii-and-assyria",
            Summary: "Around 720 BCE, Marduk-apla-iddina of Babylon and Sargon II of Assyria both rebuild Holy Inanna's sanctuary from the ground up. They make the courtyards bigger and put a new outer wall around the ziggurat. The stairs you can still see on the ziggurat today come from this time.",
            Sources: "Getty 2019 Chronological Table; ch. 37, figs. 37.2, 37.4; Ziggurat of Ur-Nammu overview; sheet 20 entry A12"),

        // Sheet 20 Part A, entry A13 (Neo-Babylonian period)
        new PeriodInfo(
            Period: Period.NeoBabylonian,
            Name: "Neo-Babylonian period",
            Years: "625 to 539 BCE",
            Levels: "",
            ArtKey: "period-neo-babylonian",
            Summary: "King Nebuchadnezzar II rebuilds the great wall around Holy Inanna's sanctuary. Two small temples, the Deep Temples (Tieftempel), sit in the corners of the ziggurat's stairs. Under the floor of one lies a little clay lion.",
            Sources: "Getty 2019 Chronological Table; ch. 12, ch. 37 (van Ess); Jordan 1932 (UVB III) pp. 33-34; story/16 Correction #1; sheet 20 entry A13"),

        // Sheet 20 Part A, entry A14 (Achaemenid period)
        new PeriodInfo(
            Period: Period.Achaemenid,
            Name: "Achaemenid period",
            Years: "538 to 332 BCE",
            Levels: "",
            ArtKey: "period-achaemenid",
            Summary: "The Persian king Cyrus the Great rules Babylon, and southern Mesopotamia is one of many Persian provinces. Even then, Holy Inanna's sanctuary at Uruk is still in use. Bricks stamped with Cyrus's name lie beside one of Her small temples.",
            Sources: "Getty 2019 Chronological Table; ch. 37; Jordan 1932 (UVB III) p. 33; sheet 20 entry A14"),

        // Sheet 20 Part A, entry A15 (Seleucid period)
        new PeriodInfo(
            Period: Period.Seleucid,
            Name: "Seleucid period",
            Years: "332 to 141 BCE",
            Levels: "",
            ArtKey: "period-seleucid",
            Summary: "After Alexander the Great, Greek-speaking kings rule Uruk. Two local leaders, both named Anu-uballit, build giant temples of baked brick, the Resh for Anu and the Eshgal for Ishtar and Nanaya. Priests copy old texts and study Sumerian again.",
            Sources: "Getty 2019 Chronological Table; ch. 12, ch. 35, ch. 37; sheet 20 entry A15"),

        // Sheet 20 Part A, entry A16 (Parthian period)
        new PeriodInfo(
            Period: Period.Parthian,
            Name: "Parthian period",
            Years: "141 BCE to 224 CE",
            Levels: "",
            ArtKey: "period-parthian",
            Summary: "The Parthians from Iran take over Uruk. People turn old temples into homes. A new temple to a god called Gareus is built, and Holy Inanna's old district slowly stops being used for worship.",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess); ch. 35, fig. 35.6; sheet 20 entry A16"),

        // Sheet 20 Part A, entry A17 (Sassanian period and the abandonment)
        new PeriodInfo(
            Period: Period.Sassanian,
            Name: "Sassanian period",
            Years: "224 to 634 CE",
            Levels: "",
            ArtKey: "period-sassanian",
            Summary: "People have lived at Uruk for about 4,500 years. In the early 300s CE the last of them leave the city. The rivers have moved, so they settle in new villages nearby.",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess); sheet 20 entry A17"),

        // Sheet 20 Part A, entry A18 (the Arab period and al-Warka)
        new PeriodInfo(
            Period: Period.ArabConquest,
            Name: "The Arab conquest",
            Years: "634 CE",
            Levels: "",
            ArtKey: "period-arab-conquest",
            Summary: "In 634 CE Arab horsemen under Al-Muthanna win a battle at a place called al-Warka. That name lives on: the ruins of Uruk are called Warka today. New cities like Basra and Baghdad become the centres of the land.",
            Sources: "Getty 2019 ch. 12 (van Ess); README.txt section 5; sheet 20 entry A18"),

        // Sheet 20 Part A, entry A19 (the shift of the Euphrates)
        new PeriodInfo(
            Period: Period.EuphratesShifts,
            Name: "The Euphrates moves west",
            Years: "from the first millennium BCE until after the Sassanian period",
            Levels: "",
            ArtKey: "period-euphrates-shifts",
            Summary: "Uruk lives because of the Euphrates River. Over many centuries the river moves away to the west, and the canals are not kept up. The land empties and becomes pasture, and because nobody builds over the old city, its ruins survive.",
            Sources: "Getty 2019 ch. 12 (van Ess), p. 76; ch. 14; sheet 20 entry A19"),
    };
}
