using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly GlossaryEntry[] GlossaryEntries =
    {
        // Sheet 21 Part B, entry B01 (Eanna)
        new GlossaryEntry(
            Word: "Eanna (é-anna)",
            Language: "Sumerian",
            Meaning: "“House of Heaven” (é = house, anna = of heaven): the name of Holy Inanna's precinct at the centre of Uruk. Only from Ur-Nammu's stamped bricks can the complex and its name be linked with certainty.",
            Pronunciation: "ay-AHN-nah",
            Cuneiform: "𒂍𒀭𒈾",
            Sources: "Getty 2019 ch. 39 (Selz), ch. 37 (van Ess); PRONUNCIATION_GUIDE §VI; sheet 21 entry B01"),

        // Sheet 21 Part B, entry B02 (Uruk)
        new GlossaryEntry(
            Word: "Uruk",
            Language: "the usual modern name",
            Meaning: "The usual name of the city, and also of a period, the Uruk period (about 4000 to 3300 BCE).",
            Pronunciation: "OO-rook",
            Cuneiform: "𒀕𒆠",
            Sources: "Getty 2019 Chronological Table; PRONUNCIATION_GUIDE §III, §VI; sheet 21 entry B02"),

        // Sheet 21 Part B, entry B02 (Unug)
        new GlossaryEntry(
            Word: "Unug",
            Language: "Sumerian",
            Meaning: "The Sumerian form of the city's name, written “Eanna-Unug (Uruk)” by Selz.",
            Pronunciation: "",
            Cuneiform: "𒀕𒆠",
            Sources: "Getty 2019 ch. 39 (Selz); sheet 21 entry B02"),

        // Sheet 21 Part B, entry B02 (Warka, al-Warka)
        new GlossaryEntry(
            Word: "Warka (al-Warka)",
            Language: "Arabic",
            Meaning: "The modern name of the site. A battle for al-Warka was fought in 634 CE.",
            Pronunciation: "WAR-kah",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 12 (van Ess); README.txt section 5; PRONUNCIATION_GUIDE §V; sheet 21 entry B02"),

        // Sheet 21 Part B, entry B02 (Erech)
        new GlossaryEntry(
            Word: "Erech",
            Language: "Hebrew",
            Meaning: "The name of Uruk in the Bible, Genesis 10:10.",
            Pronunciation: "EH-rekh",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §V; README.txt section 5; sheet 21 entry B02"),

        // Sheet 21 Part B, entry B02 (Orchoë)
        new GlossaryEntry(
            Word: "Orchoë",
            Language: "Greek",
            Meaning: "The Greek name for Uruk, used by Pliny the Elder in his Natural History, about 77 CE.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 12 (van Ess) p. 76; sheet 21 entry B02"),

        // Sheet 21 Part B, entry B03 (Kullab, Kullaba)
        new GlossaryEntry(
            Word: "Kullaba (Kullab)",
            Language: "Sumerian",
            Meaning: "The second district of Uruk, around the Anu Ziggurat and the White Temple, named as early as the 26th century BCE. It was possibly the centre of a second original village on the river.",
            Pronunciation: "KOOL-lah-bah",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 37 (van Ess), ch. 14, ch. 39 (Selz); PRONUNCIATION_GUIDE §VI; sheet 21 entry B03"),

        // Sheet 21 Part B, entry B04 (the Dingir)
        new GlossaryEntry(
            Word: "Dingir",
            Language: "Sumerian",
            Meaning: "The cuneiform sign for divinity, written before a god's name and not read aloud. Scholars write it as a raised ᵈ; this game says it as “Holy.”",
            Pronunciation: "DIN-geer",
            Cuneiform: "𒀭",
            Sources: "README.txt section 3; PRONUNCIATION_GUIDE §VI; sheet 21 entry B04"),

        // Sheet 21 Part B, entry B04 (ᵈInanna)
        new GlossaryEntry(
            Word: "ᵈInanna",
            Language: "Sumerian",
            Meaning: "Holy Inanna's name with the divine sign before it. Selz writes it ᵈinana-k, “Mistress of Heaven.”",
            Pronunciation: "HOH-lee ih-NAH-nah",
            Cuneiform: "𒀭𒈹",
            Sources: "Getty 2019 ch. 39 (Selz); README.txt section 3; PRONUNCIATION_GUIDE §VIII; sheet 21 entry B04"),

        // Sheet 21 Part B, entry B05 (Nin-an-ak)
        new GlossaryEntry(
            Word: "Nin-an-ak (Innana.k)",
            Language: "Sumerian",
            Meaning: "“Mistress of Heaven.” Even in ancient times the name Inanna, in full Innana.k, was explained as short for Nin-an-ak. Zgoll also gives the title nu-ge₁₇-g, “female heavenly ruler.”",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 9 (Zgoll); sheet 21 entry B05"),

        // Sheet 21 Part B, entry B06 (Innin)
        new GlossaryEntry(
            Word: "Innin",
            Language: "Sumerian",
            Meaning: "An older form of Holy Inanna's name, used on Karaindash's bricks and by Jordan and Nöldeke. One library file calls it Old Akkadian instead; the sources disagree.",
            Pronunciation: "IN-nin",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; Jordan 1932 (UVB III) pp. 32-33; Nöldeke, UVB XI (1940); sheet 21 entry B06 and Open Question 9"),

        // Sheet 21 Part B, entry B07 (Ishtar)
        new GlossaryEntry(
            Word: "Ishtar (Ištar, Ischtar)",
            Language: "Akkadian",
            Meaning: "Her Akkadian name; German reports spell it Ischtar. From the form Ishtaru came a general word for “goddess.”",
            Pronunciation: "ISH-tar",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 9 (Zgoll); PRONUNCIATION_GUIDE §VI; sheet 21 entry B07"),

        // Sheet 21 Part B, entry B08 (MÙŠ, the reed bundle)
        new GlossaryEntry(
            Word: "MÙŠ",
            Language: "Sumerian",
            Meaning: "The sign that writes both Inanna and Ishtar. It grew out of the reed-bundle symbol on the earliest clay tablets of Uruk; Jordan called it Her ring-bundle (Ringbündel).",
            Pronunciation: "",
            Cuneiform: "𒈹",
            Sources: "Getty 2019 ch. 11, figs. 11.2-11.3; Jordan 1932 (UVB III) pp. 32-33; sheet 21 entry B08"),

        // Sheet 21 Part B, entry B09 (me)
        new GlossaryEntry(
            Word: "me",
            Language: "Sumerian",
            Meaning: "The divine powers. Enheduanna's hymn calls Holy Inanna “Mistress of the Innumerable Divine Powers.”",
            Pronunciation: "",
            Cuneiform: "𒈨",
            Sources: "Getty 2019 ch. 9 (Zgoll), note 6; sheet 21 entry B09"),

        // Sheet 21 Part B, entry B10 (en)
        new GlossaryEntry(
            Word: "en",
            Language: "Sumerian",
            Meaning: "“Lord”: a title for priests or priestesses as well as the ruler, tied above all to Uruk. The sign may come from the tray of cups carried on the Uruk Vase.",
            Pronunciation: "",
            Cuneiform: "𒂗",
            Sources: "Getty 2019 ch. 39 (Selz), ch. 11; sheet 21 entry B10"),

        // Sheet 21 Part B, entry B11 (ensi)
        new GlossaryEntry(
            Word: "ensi",
            Language: "Sumerian",
            Meaning: "“City prince.” Lugalzagesi began his career as ensi of Umma.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 35; sheet 21 entry B11"),

        // Sheet 21 Part B, entry B12 (lugal)
        new GlossaryEntry(
            Word: "lugal",
            Language: "Sumerian",
            Meaning: "“King,” as in lugal-uri5ki-ma, “king of Ur,” on Ur-Nammu's bricks, and in royal names such as Lugalzagesi and Lugalbanda.",
            Pronunciation: "",
            Cuneiform: "𒈗",
            Sources: "BUILDING_ARCHEOLOGICAL_QUOTES (Ziggurat A1); Getty 2019 ch. 35; sheet 21 entry B12"),

        // Sheet 21 Part B, entry B13 (nin)
        new GlossaryEntry(
            Word: "nin",
            Language: "Sumerian",
            Meaning: "“Lady,” as in nin-e2-an-na, “lady of Eanna,” and nin-a-ni, “his lady.”",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "BUILDING_ARCHEOLOGICAL_QUOTES (Ziggurat A1); sheet 21 entry B13"),

        // Sheet 21 Part B, entry B14 (u₆-nir)
        new GlossaryEntry(
            Word: "u₆-nir",
            Language: "Sumerian",
            Meaning: "A temple standing on terraces: the ziggurat.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 37 (van Ess); sheet 21 entry B14"),

        // Sheet 21 Part B, entry B14 (ziqqurratu)
        new GlossaryEntry(
            Word: "ziqqurratu",
            Language: "Akkadian",
            Meaning: "A temple standing on terraces, the word that gives us “ziggurat.” Ur-Nammu's lower terrace measured 48 by 56 metres and stood 11.2 metres tall.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 37 (van Ess); sheet 21 entry B14"),

        // Sheet 21 Part B, entry B14 (Zikkurrat, the German reports' spelling)
        new GlossaryEntry(
            Word: "Zikkurrat (Zikurrat)",
            Language: "German",
            Meaning: "The German reports' spelling of ziggurat, the temple tower.",
            Pronunciation: "tsik-koo-RAHT",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §III; sheet 21 entry B14 and Part C, C3"),

        // Sheet 21 Part B, entry B15 (Riemchen)
        new GlossaryEntry(
            Word: "Riemchen",
            Language: "German",
            Meaning: "Narrow Late Uruk mudbricks, an excavators' word. Nöldeke gives 16 by 6 by 6 cm as the mark of Level IVa, but other sizes are recorded too.",
            Pronunciation: "REEM-khen",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §IV; Nöldeke, UVB XI (1940) p. 16; sheet 21 entry B15"),

        // Sheet 21 Part B, entry B15 (Patzen)
        new GlossaryEntry(
            Word: "Patzen",
            Language: "German",
            Meaning: "Larger-format Late Uruk bricks.",
            Pronunciation: "PAHT-sen",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §IV; sheet 21 entry B15"),

        // Sheet 21 Part B, entry B16 (ne-sang)
        new GlossaryEntry(
            Word: "ne-sang",
            Language: "Sumerian",
            Meaning: "The “first fruits” brought at the harvest festival shown on the Uruk Vase.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 39 (Selz); sheet 21 entry B16"),

        // Sheet 21 Part B, entry B16 (nīsannu)
        new GlossaryEntry(
            Word: "nīsannu",
            Language: "Akkadian",
            Meaning: "The name of the first-fruits festival and its month; it lives on in Arabic as Nīsān.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 39 (Selz); sheet 21 entry B16"),

        // Sheet 21 Part B, entry B17 (Tieftempel)
        new GlossaryEntry(
            Word: "Tieftempel",
            Language: "German",
            Meaning: "Jordan's “deep temple” or “low temple”: a cult building at ground level or at the foot of a ziggurat. He used it both as a type (the Limestone Temple) and as the name of the two Neo-Babylonian temples at the ziggurat stairs.",
            Pronunciation: "TEEF-tem-pel",
            Cuneiform: "",
            Sources: "Jordan 1932 (UVB III) p. 17; story/16 Correction #1; PRONUNCIATION_GUIDE; sheet 21 entry B17"),

        // Sheet 21 Part B, entry B18 (Gipfeltempel)
        new GlossaryEntry(
            Word: "Gipfeltempel",
            Language: "German",
            Meaning: "Jordan's “summit temple”: a temple on top of a high terrace or ziggurat, such as the White Temple.",
            Pronunciation: "GIP-fel-tem-pel",
            Cuneiform: "",
            Sources: "Jordan 1932 (UVB III) pp. 17, 24; sheet 21 entry B18"),

        // Sheet 21 Part B, entry B19 (sahuru)
        new GlossaryEntry(
            Word: "sahuru",
            Language: "Akkadian",
            Meaning: "The forehall (Vorcella) of a temple. Albert Schott's study of this word shaped the idea that the deity stepped down from heaven onto the top of the ziggurat.",
            Pronunciation: "sah-HOO-roo",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; Jordan 1932 (UVB III) p. 24; sheet 21 entry B19"),

        // Sheet 21 Part B, entry B19 (namaru)
        new GlossaryEntry(
            Word: "namaru",
            Language: "Akkadian",
            Meaning: "A temple term.",
            Pronunciation: "nah-MAH-roo",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; sheet 21 entry B19"),

        // Sheet 21 Part B, entry B19 (parakku)
        new GlossaryEntry(
            Word: "parakku",
            Language: "Akkadian",
            Meaning: "A cult platform or throne room.",
            Pronunciation: "pah-RAH-koo",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; sheet 21 entry B19"),

        // Sheet 21 Part B, entry B20 (ki-bi-šè mu-na-gi4)
        new GlossaryEntry(
            Word: "ki-bi-šè mu-na-gi4",
            Language: "Sumerian",
            Meaning: "“To its place he restored it”: a standard formula of royal building inscriptions. It closes Ur-Nammu's dedication to Holy Inanna.",
            Pronunciation: "KEE-bee-SHEH moo-nah-GEE",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; BUILDING_ARCHEOLOGICAL_QUOTES (Ziggurat A1); sheet 21 entry B20"),

        // Sheet 21 Part B, entry B21 (amurdinnu)
        new GlossaryEntry(
            Word: "amurdinnu",
            Language: "Akkadian",
            Meaning: "The rosette; Her rosette has eight petals.",
            Pronunciation: "ah-moor-DIN-noo",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §IV; README.txt section 4; sheet 21 entry B21"),

        // Sheet 21 Part B, entry B22 (akītu)
        new GlossaryEntry(
            Word: "akītu",
            Language: "Akkadian",
            Meaning: "The New Year festival. At Uruk it was held in a sacred building outside the city to the northeast, at the spring equinox.",
            Pronunciation: "ah-KEE-too",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §VI; Getty 2019 ch. 12 (van Ess); sheet 21 entry B22"),

        // Sheet 21 Part B, entry B23 (Bit Resh)
        new GlossaryEntry(
            Word: "Bit Resh (Resh Sanctuary)",
            Language: "Akkadian",
            Meaning: "The Seleucid sanctuary of Anu and his consort Antum, built on the old Anu Ziggurat platform; more than 35,000 square metres.",
            Pronunciation: "BIT RESH",
            Cuneiform: "",
            Sources: "Getty 2019 ch. 12, ch. 37 (van Ess); PRONUNCIATION_GUIDE §VI; sheet 21 entry B23"),

        // Sheet 21 Part B, entry B24 (Zingel)
        new GlossaryEntry(
            Word: "Zingel",
            Language: "German",
            Meaning: "A precinct wall with rooms built into it, such as Ur-Nammu's wall around the ziggurat.",
            Pronunciation: "TSING-el",
            Cuneiform: "",
            Sources: "BUILDING_ARCHEOLOGICAL_QUOTES (Deep Temples Q11); PRONUNCIATION_GUIDE; sheet 21 entry B24"),

        // Sheet 21 Part B, entry B25 (Libn-Jungs)
        new GlossaryEntry(
            Word: "Libn-Jungs",
            Language: "German",
            Meaning: "“Mudbrick lads”: Nöldeke's fond name for the young Iraqi workers who found mudbrick walls by feel with a long awl, in his letters of January 1932.",
            Pronunciation: "",
            Cuneiform: "",
            Sources: "van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; sheet 21 entry B25"),

        // Sheet 21 Part B, entry B26 (tell)
        new GlossaryEntry(
            Word: "tell",
            Language: "language not recorded in the library",
            Meaning: "The hill of a buried city, as in the place name Tell Uqair.",
            Pronunciation: "TELL",
            Cuneiform: "",
            Sources: "README.txt; PRONUNCIATION_GUIDE §VI; sheet 21 entry B26"),

        // Sheet 21 Part B, entry B27 (cella)
        new GlossaryEntry(
            Word: "Cella",
            Language: "German (from Latin)",
            Meaning: "The inner cult room of a temple. Jordan found that the cella doors of the Deep Temples could only have been closed by curtains.",
            Pronunciation: "TSEL-lah",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §IV; Jordan 1932 (UVB III) p. 33; sheet 21 entry B27"),

        // Sheet 21 Part B, entry B27 (Vorcella)
        new GlossaryEntry(
            Word: "Vorcella",
            Language: "German (from Latin)",
            Meaning: "The antechamber or forehall in front of the cella.",
            Pronunciation: "FOR-tsel-lah",
            Cuneiform: "",
            Sources: "PRONUNCIATION_GUIDE §IV; sheet 21 entry B27"),

        // Sheet 21 Part B, entry B28 (Stiftmosaik)
        new GlossaryEntry(
            Word: "Stiftmosaik",
            Language: "German",
            Meaning: "Cone mosaic: clay or stone cones pressed into mud plaster, their heads colored “generally black, white, and red.” Late Uruk, Levels V and IV.",
            Pronunciation: "SHTIFT-moh-ZYE-ick",
            Cuneiform: "",
            Sources: "Heinrich in Jordan 1932 (UVB III) p. 14; PRONUNCIATION_GUIDE; sheet 21 entry B28"),

        // Sheet 21 Part B, entry B29 (Langraum)
        new GlossaryEntry(
            Word: "Langraum",
            Language: "German",
            Meaning: "The long central hall of a three-part temple, the principal cult room (Hauptkultraum).",
            Pronunciation: "LAHNG-rowm",
            Cuneiform: "",
            Sources: "Heinrich, UVB X; PRONUNCIATION_GUIDE §IV; sheet 21 entry B29"),
    };
}
