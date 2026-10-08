using System.Collections.Generic;
using System.Globalization;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    /// <summary>A headline and its sub-head, as a newspaper would print them, with the writer's byline.</summary>
    /// <param name="Headline">The banner, in capitals.</param>
    /// <param name="SubHead">One sentence under the banner.</param>
    /// <param name="Byline">The byline, such as "From our correspondent at the dig"; null means the paper's default.</param>
    public sealed record HeadlinePair(string Headline, string SubHead, string Byline = null);

    //Filled by the two partial methods below, which other content files implement (a partial method with no
    //  implementation compiles to nothing, so each pass can be absent while the others are written)
    private static readonly Dictionary<string, HeadlinePair[]> HeadlineVariantTable = new();
    private static readonly List<string> BylineList = new() { "From our correspondent with the expedition" };

    static Catalog()
    {
        AddFirstPassHeadlineVariants(HeadlineVariantTable, BylineList);
        AddSecondPassHeadlines(HeadlineTable, HeadlineVariantTable);
    }

    /// <summary>Implemented by Catalog.HeadlineVariants.cs: further editions for the first-pass discoveries and the seasons, and the paper's bylines.</summary>
    static partial void AddFirstPassHeadlineVariants(Dictionary<string, HeadlinePair[]> variants, List<string> bylines);

    /// <summary>Implemented by Catalog.Headlines2.cs: the first edition and the further editions for the second-pass discoveries.</summary>
    static partial void AddSecondPassHeadlines(Dictionary<string, HeadlinePair> table, Dictionary<string, HeadlinePair[]> variants);

    /// <summary>The bylines the paper uses when a headline carries none.</summary>
    public static IReadOnlyList<string> Bylines => BylineList;

    /// <summary>How many editions of a headline exist for an id: the first-pass pair plus the variants, or 1 for the fallback.</summary>
    /// <param name="id">The discovery id, or a season year such as "1930/31".</param>
    public static int HeadlineVariantCount(string id) =>
        id != null && HeadlineTable.ContainsKey(id) ? 1 + (HeadlineVariantTable.TryGetValue(id, out var more) ? more.Length : 0) : 1;

    /// <summary>
    /// One edition of the headline for a discovery or a season: edition 0 is the first-pass pair, further editions are
    /// the variants written as if by other editors; the index wraps, so any non-negative number is a valid choice.
    /// </summary>
    /// <param name="id">The discovery id, or a season year.</param>
    /// <param name="title">The title to fall back on.</param>
    /// <param name="edition">Which edition; wraps modulo <see cref="HeadlineVariantCount"/>.</param>
    public static HeadlinePair HeadlineFor(string id, string title, int edition)
    {
        var count = HeadlineVariantCount(id);
        var index = count == 0 ? 0 : ((edition % count) + count) % count;
        if (index == 0 || id == null || !HeadlineVariantTable.TryGetValue(id, out var variants))
        {
            return HeadlineFor(id, title);
        }

        return variants[index - 1];
    }

    private static readonly Dictionary<string, HeadlinePair> HeadlineTable = new()
    {
        // Discoveries, tier 9: the deep sounding
        ["deep-trench"] = new HeadlinePair("DEEP SHAFT CUTS THROUGH SIXTEEN BUILDING LAYERS",
            "The diggers went almost 20 meters down under Holy Inanna's first great temple until they reached groundwater."),
        ["persian-gulf-floor"] = new HeadlinePair("IS THIS THE FLOOR OF THE OLD SEA?",
            "Ernst Heinrich believed the green mud at the shaft's bottom was the old Persian Gulf floor; scholars now disagree."),
        ["reed-platforms"] = new HeadlinePair("HOLY INANNA'S CITY BEGINS ON REEDS",
            "The first settlers tread cut reeds flat into dry floors in the marsh, layer upon layer."),
        ["level-vi-cone-heaps"] = new HeadlinePair("OLDEST CONE HEAPS FOUND BENEATH FIRST TEMPLE",
            "Jordan and Heinrich found two heaps of clay cones in winter 1930/31, fallen from walls long melted away."),

        // Discoveries, tier 8: Uruk V and the oldest great buildings
        ["limestone-temple"] = new HeadlinePair("STONE TEMPLE RISES FROM THE MUD!",
            "Jordan and Heinrich uncovered the 76-meter temple's limestone foundations in the winters of 1929/30 and 1930/31."),
        ["stone-cone-building"] = new HeadlinePair("THOUSANDS OF STONE CONES STUD POURED WALLS",
            "Its walls are poured lime mortar, and a water tank inside hints at rites using much liquid."),
        ["bronze-bound-reed-bundle"] = new HeadlinePair("REAL REED BUNDLE FOUND BOUND IN BRONZE",
            "A bundle of reeds is the shape of Holy Inanna's symbol, later the sign for writing Her name."),
        ["buried-architectural-models"] = new HeadlinePair("TINY CLAY BUILDINGS FOUND BURIED TOGETHER",
            "One model shows a temple front, like an architect's model from more than five thousand years ago."),
        ["niched-building"] = new HeadlinePair("OLD BRICK BUILDING LIES FLATTENED UNDER TERRACE",
            "Arnold Nöldeke's team uncovered it in about 1931/32 and named it for niches they thought they saw."),
        ["round-pillar-hall"] = new HeadlinePair("GIANT PILLARS WEAR COATS OF COLORED CONES",
            "Jordan and Heinrich uncovered the pillars, each over two and a half meters thick, between 1928/29 and 1930/31."),
        ["podium-facade-mosaic"] = new HeadlinePair("WHOLE MOSAIC WALL LIFTED OUT FOR BERLIN",
            "Rudolf Michaelis of the Berlin museum lifted the zigzag panel in one piece in winter 1930/31."),
        ["loftus-facade"] = new HeadlinePair("LOFTUS'S LOST WALL FINDS ITS HALL",
            "Jordan's team showed in 1930/31 that the cone wall Loftus dug in the 1850s belonged to the Round-Pillar Hall."),
        ["reed-mat-room-164"] = new HeadlinePair("REAL REED MAT FOUND PRESSED TO WALL",
            "The find in winter 1930/31 backed the old guess that cone patterns copy woven reed mats."),
        ["white-temple"] = new HeadlinePair("WHITE TEMPLE CROWNS THE SKY GOD'S HILL",
            "Jordan's team found it in about 1930/31 on a terrace about 12 meters high, still coated in white plaster."),
        ["anu-stone-building"] = new HeadlinePair("TOMB-LIKE STONE BOXES HOLD NO BODY",
            "Three stone boxes nest one inside another; some scholars compare it to Gilgamesh's stone tomb in an old story."),

        // Discoveries, tier 7: Uruk IV, the age of the first writing
        ["temple-c"] = new HeadlinePair("A MILLION BRICKS RAISE TEMPLE C",
            "Nöldeke's team, with Heinrich and Lenzen, uncovered the 54-meter temple in about 1933/34 to 1934/35."),
        ["room-231-sealings"] = new HeadlinePair("SEAL PICTURES PRESSED IN TEMPLE C CLAY",
            "The stamped lumps close jars and bundles, showing animals and perhaps priests carrying gifts to a temple door."),
        ["temple-d"] = new HeadlinePair("TEMPLE D AMONG GREATEST BUILDINGS OF ITS AGE",
            "Nöldeke's team uncovered the 80-by-50-meter temple in about 1931/32, but no writing names its god."),
        ["mountain-seal-tablet"] = new HeadlinePair("ANIMALS CLIMB MOUNTAINS ON SEALED TABLET",
            "A seal rolled on a tablet inside Temple D shows animals climbing rows of 'mountain' signs."),
        ["cone-mosaic-temple"] = new HeadlinePair("MOSAIC TEMPLE RISES ON OLD RUINS",
            "It is dressed again in colored cones, and a door frame shows even its inside walls were covered."),
        ["spear-bearer-sealing"] = new HeadlinePair("SPEAR-BEARER AND BOUND PRISONER ON CLAY SEAL",
            "This sealing is a very early picture of power, pressed into unbaked clay."),
        ["pillar-hall-twelve-pillars"] = new HeadlinePair("YELLOW HALL HOLDS 235 MOSAIC NICHES",
            "Its cone mosaics come in more than seventy patterns, so someone must plan every niche in advance."),
        ["great-court"] = new HeadlinePair("WALLED COURT MAY BE TEMPLE GARDEN",
            "Many scholars think the walled court with its own water channels is a garden; others think it served processions."),
        ["building-e"] = new HeadlinePair("BUILDING E: A MEETING PLACE?",
            "Scholars think large groups met in this 57-meter square, perhaps to open goods sealed with stamped clay."),
        ["riemchen-building"] = new HeadlinePair("PRECIOUS OFFERINGS WALLED UP AND LEFT BEHIND",
            "Nöldeke and Heinrich opened the sealed building in about 1937/38 to 1938/39, five thousand years later."),
        ["riemchen-deposit"] = new HeadlinePair("LAPIS AND CARNELIAN IN THE SEALED ROOM",
            "Nöldeke and Heinrich found stone bowls, copper and gems from far-off lands there in winter 1938/39."),
        ["warka-vase"] = new HeadlinePair("TALL VASE CARVED WITH A PROCESSION OF GIFTS!",
            "Found in winter 1933/34 under Ernst Heinrich, it shows offerings brought to a woman who may be Holy Inanna."),
        ["mask-of-warka"] = new HeadlinePair("LIFE-SIZE STONE FACE LOOKS UP FROM SOIL!",
            "Nöldeke's team found it on 22 February 1939; many scholars think it shows Holy Inanna."),
        ["archaic-tablets"] = new HeadlinePair("OLDEST WRITING IN MESOPOTAMIA COMES TO LIGHT",
            "About two thousand clay tablets, mostly lists of grain, animals and workers, turned up from winter 1928/29 onward."),
        ["late-uruk-cylinder-seals"] = new HeadlinePair("LITTLE STONE ROLLERS PRINT WHOLE PICTURES",
            "One lapis seal with a silver calf on top shows a ruler in a boat."),
        ["reed-bundle-inlay"] = new HeadlinePair("HER REED-BUNDLE SIGN SET IN TEMPLE WALL!",
            "The looped reed bundle is Holy Inanna's symbol and becomes the sign used to write Her name."),
        ["cone-mosaic-technique"] = new HeadlinePair("FINGER-LONG CONES MAKE WALLS OF WOVEN COLOR",
            "Uruk is the only place where cone mosaics are found still fixed to the walls of buildings."),

        // Discoveries, tier 6: Level III, the Jemdet Nasr age
        ["red-temple"] = new HeadlinePair("RED TEMPLE'S PLAN STILL A PUZZLE",
            "Jordan reached the red-walled temple in winter 1929/30, and no one has drawn its plan completely."),
        ["uruk-iii-tablets"] = new HeadlinePair("PICTURES TURN INTO WEDGES ON CLAY TABLETS",
            "Jordan's team found the pile of half-picture, half-wedge tablets under a layer of bricks in about 1930/31."),
        ["half-round-pillar-terrace"] = new HeadlinePair("FIRST STEP TOWARD THE GREAT STEPPED TOWERS",
            "Archaeologists see this terrace with twelve half-round pillars as the beginning of the ziggurats."),
        ["labyrinth-and-kitchen"] = new HeadlinePair("LABYRINTH AND KITCHEN BESIDE THE TERRACE",
            "Tiny patterned rooms stand near long cooking pits, probably the temple's kitchen."),

        // Discoveries, tier 5: the Early Dynastic city
        ["rammed-earth-building"] = new HeadlinePair("HUGE BUILDING OF POUNDED EARTH, NOT BRICK",
            "Nöldeke's team and Lenzen kept meeting it from about 1936/37; one idea says it held Lugalzagesi's palace."),
        ["headless-clay-woman"] = new HeadlinePair("TINY CLAY WOMAN CARRIED UP FROM THE DEEP",
            "Lenzen's team found the 4-centimeter headless figure in winter 1963/64, made in the first villagers' style."),
        ["city-wall"] = new HeadlinePair("NINE-KILOMETER WALL RINGS THE CITY",
            "The Epic of Gilgamesh says King Gilgamesh built the wall, with its about 900 half-round towers."),
        ["early-dynastic-terrace"] = new HeadlinePair("NEW SQUARE TERRACE BEHIND A GREAT GATEHOUSE",
            "In the Early Dynastic age, a terrace about 50 meters square replaces the L-shaped one."),
        ["ziggurat-cutting"] = new HeadlinePair("TOWERS HIDE INSIDE HOLY INANNA'S TOWER",
            "Jordan cut more than 12 meters into its side in winter 1930/31 and found older brick towers within."),

        // Discoveries, tier 4: the Third Dynasty of Ur
        ["ur-nammu-ziggurat"] = new HeadlinePair("UR-NAMMU'S TOWER FOR THE GODDESS STILL STANDS!",
            "Ur-Nammu builds it for Holy Inanna about 4,100 years ago, and its mud-brick core stands today."),
        ["ziggurat-reed-layers"] = new HeadlinePair("4,100-YEAR-OLD REEDS STILL INSIDE THE TOWER",
            "Every 1.2 to 1.4 meters, the builders lay crossed reed bundles between the mud bricks."),
        ["ur-nammu-stamped-bricks"] = new HeadlinePair("HER NAME COMES FIRST, BEFORE THE KING'S!",
            "Ur-Nammu stamps his bricks in Sumerian: 'For Holy Inanna, Lady of Eanna, his lady.'"),
        ["ur-nammu-foundation-document"] = new HeadlinePair("KING'S BURIED MESSAGE FOUND IN FOUNDATION BOX",
            "Ur-Nammu's stone document lies under the walls, and later kings treat such messages with respect."),
        ["ur-nammu-enclosure"] = new HeadlinePair("WALL OF ROOMS HUGS THE GODDESS'S TOWER",
            "Jordan traced it piece by piece in winter 1930/31: shrines, kitchens, storerooms and offices."),
        ["shulgi-foundation-tablet"] = new HeadlinePair("KING ŠULGI BURIES A TABLET FOR HOLY INANNA!",
            "It begins 'For Inanna, Lady of Eanna,' and names Šulgi, the mighty man, king of Ur."),
        ["amar-sin-door-socket"] = new HeadlinePair("KING'S DOOR SOCKET USED AGAIN 1,500 YEARS LATER",
            "The stone names Amar-Sin of Ur; Babylonian builders reset it in a new gateway of Her sanctuary."),
        ["tiamatbashti-necklace"] = new HeadlinePair("QUEEN TIAMATBASHTI'S INSCRIBED NECKLACE FOUND",
            "She is the wife of King Shu-Sin of Ur, whose kings came from Uruk."),

        // Discoveries, tier 3: Old Babylonian and Kassite
        ["karaindash-temple"] = new HeadlinePair("BRICK GODS AND GODDESSES POUR WATER!",
            "Jordan found the Kassite king's temple for Holy Inanna in winter 1928/29, its wall of about 500 molded bricks."),
        ["kurigalzu-pavement"] = new HeadlinePair("OLD TEMPLE BRICKS LAID DOWN AGAIN",
            "Kurigalzu stamps his own pavement, while later floors reuse bricks still carrying Karaindash's stamp."),
        ["sin-kashid-palace"] = new HeadlinePair("PALACE LEFT IN A HURRY KEEPS ITS LETTERS",
            "Jordan and Preusser first found it in 1912/13, and Lenzen's team dug it fully from 1958 to 1962."),
        ["shallurtum-sealing"] = new HeadlinePair("A QUEEN SEALS HER OWN JARS",
            "This lump of clay closes a jar and carries Queen Shallurtum's seal, from about 3,900 years ago."),

        // Discoveries, tier 2: Neo-Assyrian, Neo-Babylonian and Persian
        ["deep-temples"] = new HeadlinePair("TWO LITTLE TEMPLES FOUND AT THE TOWER'S FOOT!",
            "Found in winter 1930/31, their hingeless inner doorways made Jordan think curtains closed the holy rooms."),
        ["clay-lion"] = new HeadlinePair("CLAY LION CROUCHES UNDER THE TEMPLE FLOOR",
            "Jordan's team found the 17-centimeter lion in winter 1930/31, and Jordan called it artistically timeless."),
        ["cyrus-bricks"] = new HeadlinePair("CYRUS THE GREAT'S NAME STAMPED IN THE FLOOR",
            "The bricks show Her temple is still in use more than 1,500 years after Ur-Nammu."),
        ["nabonidus-bricks"] = new HeadlinePair("LAST KING OF BABYLON REBUILDS HER SANCTUARY",
            "His stamped floor bricks showed the archaeologists that the Deep Temples were late buildings too."),
        ["sargon-nebuchadnezzar-walls"] = new HeadlinePair("BRICKS OF FOUR KINGS FOUND IN THE RUBBLE",
            "Sargon II lays out the bigger enclosure, and Nebuchadnezzar II rebuilds it with bricks stamped with his name."),
        ["goddess-stele-fragment"] = new HeadlinePair("BROKEN GODDESS STELE IN KARAINDASH'S TEMPLE",
            "Carved centuries after Karaindash, the limestone slab shows his temple stays holy long after his death."),
        ["priests-houses"] = new HeadlinePair("HER PRIESTS KEEP LIBRARIES AT HOME",
            "Lenzen's teams found the houses of about 2,600 years ago, with family members buried under the floors."),

        // Discoveries, tier 1: the Seleucid surface
        ["resh-sanctuary"] = new HeadlinePair("GERMAN DIG OPENS AT A LOOTED TEMPLE",
            "Jordan and Preusser began in November 1912 at Anu's huge temple, whose walls stand up to 7 meters."),
        ["resh-glazed-bricks"] = new HeadlinePair("STARS, LIONS AND MONSTERS IN SHINING BRICK",
            "Jordan and Preusser found the glazed pieces in winter 1912/13, like the famous Ishtar Gate of Babylon."),
        ["eshgal"] = new HeadlinePair("GREAT NEW TEMPLE RISES SOUTH OF EANNA!",
            "Nöldeke's team dug it in winter 1932/33; it honors Ishtar, the later name of Holy Inanna, with Nanaya."),
        ["ziggurat-mantle"] = new HeadlinePair("OLD TOWER'S LATE COAT PEELED AWAY",
            "Jordan's crew removed most of the mantle in the winter of 1930/31 and found the stairs and two small temples."),

        // Seasons, 1912/13 to 1938/39
        ["1912/13"] = new HeadlinePair("GERMANS BEGIN THE FIRST DIG AT URUK",
            "Julius Jordan and Conrad Preusser mapped the city under an Ottoman permit."),
        ["1928/29"] = new HeadlinePair("BACK TO URUK AFTER FIFTEEN YEARS",
            "Julius Jordan returned and chose to dig Eanna, the house of Holy Inanna."),
        ["1929/30"] = new HeadlinePair("DEEP TRENCH REACHES THE RED TEMPLE",
            "Julius Jordan's team also saw the first stones of the Limestone Temple, then kept Christmas in the dig house."),
        ["1930/31"] = new HeadlinePair("STONE-FOOTED TEMPLE AND PILLAR HALL UNCOVERED",
            "Julius Jordan led his busiest and last winter, then left in 1931 to run Iraq's antiquities office."),
        ["1931/32"] = new HeadlinePair("MUDBRICK LADS FIND WALLS BY FEEL",
            "Arnold Nöldeke took charge, and young Iraqi workers traced melted walls with a long pointed tool."),
        ["1932/33"] = new HeadlinePair("DIGGERS TURN TO THE ESHGAL TEMPLE",
            "Under Arnold Nöldeke, the team dug the Seleucid temple at museum director Walter Andrae's request."),
        ["1933/34"] = new HeadlinePair("TALL CARVED VASE FOUND IN BURIED HOARD",
            "Ernst Heinrich directed the whole dig for this one winter; the vase is now in the Iraq Museum."),
        ["1934/35"] = new HeadlinePair("MONEY GROWS SHORT AS LIMITS BITE",
            "Arnold Nöldeke was back in charge, and a surveyor laid a grid of fixed points over Eanna."),
        ["1935/36"] = new HeadlinePair("TRIBES RISE, BUT THE DIGGING GOES ON",
            "Arnold Nöldeke wrote home that work went on almost as usual during the uprisings near Rumaitha and Samawa."),
        ["1936/37"] = new HeadlinePair("FOREIGN OFFICE STEPS IN TO PAY",
            "Under Arnold Nöldeke, the team kept meeting a huge pounded-earth building while the science fund held back money."),
        ["1937/38"] = new HeadlinePair("DATING TRENCH FAILS TO SOLVE THE PUZZLE",
            "Ernst Heinrich dug it under director Arnold Nöldeke, and the flooded Euphrates took seven hours to cross."),
        ["1938/39"] = new HeadlinePair("WOMAN'S STONE FACE FOUND BEFORE THE WAR",
            "Arnold Nöldeke's team found the Mask of Warka on 22 February 1939, and then the dig stopped for fourteen years."),
    };

    /// <summary>
    /// The newspaper headline for a discovery or a season: the written one when the headline pass supplied it,
    /// otherwise one made from the title ("THE LIMESTONE TEMPLE FOUND AT WARKA").
    /// </summary>
    /// <param name="id">The discovery id, or a season year such as "1930/31".</param>
    /// <param name="title">The title to fall back on.</param>
    public static HeadlinePair HeadlineFor(string id, string title)
    {
        if (id != null && HeadlineTable.TryGetValue(id, out var pair))
        {
            return pair;
        }

        var banner = (title ?? string.Empty).ToUpper(CultureInfo.InvariantCulture);
        return new HeadlinePair(banner.Length == 0 ? "A DISCOVERY AT WARKA" : banner + " FOUND AT WARKA", string.Empty);
    }
}
