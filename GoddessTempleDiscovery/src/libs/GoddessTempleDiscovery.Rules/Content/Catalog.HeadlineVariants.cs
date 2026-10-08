using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    //The second and third editions of every first-pass headline: a wonder-struck society-page voice, then a dry
    //  scientific-correspondent voice; the first edition in Catalog.Headlines.cs is the straight news voice
    static partial void AddFirstPassHeadlineVariants(Dictionary<string, HeadlinePair[]> variants, List<string> bylines)
    {
        bylines.Clear();
        bylines.Add("From our correspondent with the expedition");
        bylines.Add("By wire from Baghdad");
        bylines.Add("From the Society's scientific correspondent");
        bylines.Add("Letter from the dig house");
        bylines.Add("From our own reporter at Warka");
        bylines.Add("By the Herald's archaeology desk");
        bylines.Add("Special to the Warka Herald");
        bylines.Add("From a member of the expedition");

        // Discoveries, tier 9: the deep sounding
        variants["deep-trench"] = new[]
        {
            new HeadlinePair("SIXTEEN FLOORS OF LIFE BENEATH HER TEMPLE",
                "Down the diggers went, almost 20 meters, through eight hundred years of building, until groundwater stopped them.",
                "From our society correspondent"),
            new HeadlinePair("SOUNDING OF 19.6 METERS RECORDS SIXTEEN LEVELS",
                "An 85-square-meter shaft runs from Late Ubaid to Late Uruk; its section drawings survive in Jordan's 1932 report.",
                "From the Society's scientific correspondent"),
        };

        variants["persian-gulf-floor"] = new[]
        {
            new HeadlinePair("AT THE BOTTOM, MUD WITHOUT A HUMAN TRACE",
                "Ernst Heinrich believed it the floor of a vanished sea, though scholars today read the green silt differently.",
                "From the Herald's society page"),
            new HeadlinePair("GREEN SILT REACHED 19.60 METERS BELOW LEVEL V",
                "Heinrich's 1932 reading as the old Gulf floor is now disputed; the Getty book describes dune sands and river sediments.",
                "By the Herald's archaeology desk"),
        };

        variants["reed-platforms"] = new[]
        {
            new HeadlinePair("A CITY BORN FROM TRODDEN MARSH REEDS",
                "In the marsh the first settlers lay cut reeds crosswise and tread them flat, making dry ground to live on.",
                "Letter from the dig house"),
            new HeadlinePair("REED LAYERS CLIMB FIVE METERS ABOVE TRENCH FLOOR",
                "Bands of green silt and grey clay part the reed floors; the oldest date to the fifth millennium's end.",
                "From our correspondent for antiquities"),
        };

        variants["level-vi-cone-heaps"] = new[]
        {
            new HeadlinePair("FALLEN CONES WHISPER OF WALLS LONG MELTED",
                "Hundreds of little clay cones lay tumbled together, the last trace of decorated walls older than Her first great temple.",
                "Special to the Warka Herald"),
            new HeadlinePair("TWO CONE DEPOSITS ASSIGNED TO URUK VI",
                "Cones of 6 to 12 centimeters lay with small clay balls and sickle shapes whose purpose Jordan could not explain.",
                "From the Herald's science desk"),
        };

        // Discoveries, tier 8: Uruk V and the oldest great buildings
        variants["limestone-temple"] = new[]
        {
            new HeadlinePair("STONE TEMPLE IN A LAND WITHOUT STONE!",
                "Builders haul limestone from far away for the footings of a 76-meter temple that may be Her first great one.",
                "From our society correspondent"),
            new HeadlinePair("LEVEL V LIMESTONE FOOTINGS CONFIRMED",
                "The temple measures 76 by 30 meters; its stone plinths survive up to seven courses high, from a disputed source.",
                "From the Society's scientific correspondent"),
        };

        variants["stone-cone-building"] = new[]
        {
            new HeadlinePair("A HOUSE OF POURED MORTAR AND COUNTLESS CONES",
                "Thousands of little cones, shaped one by one, stud walls of lime mortar; its rites may have used much liquid.",
                "From the Herald's society page"),
            new HeadlinePair("LIME-MORTAR WALLS POURED IN THIN LAYERS",
                "Layers run 5 to 10 centimeters; sources disagree whether most of the cones were stone or baked clay.",
                "By the Herald's archaeology desk"),
        };

        variants["bronze-bound-reed-bundle"] = new[]
        {
            new HeadlinePair("REEDS TIED IN BRONZE, SHAPE OF HER SIGN",
                "A real bundle of reeds lay in a small side room, in the shape later used to write Her name.",
                "Letter from the dig house"),
            new HeadlinePair("BRONZE-BANDED REED BUNDLE IN STONE-CONE ANNEX",
                "Dated to Uruk V, about 3500 BCE; whether this bundle was Her emblem is only a guess.",
                "From our correspondent for antiquities"),
        };

        variants["buried-architectural-models"] = new[]
        {
            new HeadlinePair("LITTLE TEMPLES OF CLAY LAID TO REST",
                "A tiny one-room clay structure held models of buildings, one with holes along its roof edge for cones.",
                "Special to the Warka Herald"),
            new HeadlinePair("MIDDLE FOURTH MILLENNIUM BUILDING MODELS DESCRIBED",
                "One shows a temple front with holes for cone decoration; Berlin's VA 12286 is likely one of the set.",
                "From the Herald's science desk"),
        };

        variants["niched-building"] = new[]
        {
            new HeadlinePair("OLD WALLS PACKED AWAY BENEATH A COURTYARD",
                "When its day is done, the builders flatten it and fill it with bricks to raise a terrace above.",
                "From our society correspondent"),
            new HeadlinePair("NICHES OF THE NICHED BUILDING NOW DOUBTED",
                "Its bricks run 30 by 11 by 11 centimeters; Eichmann doubts the niches that gave the building its name.",
                "From the Society's scientific correspondent"),
        };

        variants["round-pillar-hall"] = new[]
        {
            new HeadlinePair("PILLARS DRESSED IN BLACK, WHITE AND RED",
                "Two rows of huge round pillars wear patterns of colored cones, and no one knows how high they once rose.",
                "From the Herald's society page"),
            new HeadlinePair("PILLAR DIAMETER MEASURED AT 2.62 METERS",
                "Its level is disputed, Level V or Jordan's IVb; neither its height nor its roofing is known.",
                "By the Herald's archaeology desk"),
        };

        variants["podium-facade-mosaic"] = new[]
        {
            new HeadlinePair("ZIGZAG WALL LIFTED WHOLE FROM THE EARTH",
                "A Berlin museum expert raised the stepped panel of zigzags and diamonds in one piece; Berlin shows it still.",
                "Letter from the dig house"),
            new HeadlinePair("PANEL VA 10997 MEASURES 1.50 BY 3.15 METERS",
                "Eight bands of original cones on modern plaster, uncovered and lifted in winter 1930/31 under Jordan and Heinrich.",
                "From our correspondent for antiquities"),
        };

        variants["loftus-facade"] = new[]
        {
            new HeadlinePair("AN ENGLISHMAN'S LONELY WALL FOUND ITS HOME",
                "About seventy-six years after William Loftus dug it, the Germans showed whose courtyard his bright cone wall had graced.",
                "Special to the Warka Herald"),
            new HeadlinePair("LOFTUS WALL ASSIGNED TO ROUND-PILLAR COURT",
                "More than 25 meters were preserved; the team left the façade standing in place in 1930/31.",
                "From the Herald's science desk"),
        };

        variants["reed-mat-room-164"] = new[]
        {
            new HeadlinePair("A MAT FIVE THOUSAND YEARS ON THE WALL",
                "Woven reeds still clung to a back-room wall, backing the old idea that cone patterns copy hanging mats.",
                "From our society correspondent"),
            new HeadlinePair("MAT FOUND IN SITU IN ROOM 164",
                "Heinrich's find supports Walter Andrae's 1930 view that cone mosaic copies hanging reed mats.",
                "From the Society's scientific correspondent"),
        };

        variants["white-temple"] = new[]
        {
            new HeadlinePair("STILL DRESSED IN WHITE ON ITS HIGH HILL",
                "On a terrace raised at least ten times, the old plaster showed its plan 'as if drawn in chalk.'",
                "From the Herald's society page"),
            new HeadlinePair("WHITE PLASTER SURVIVES TO 3.4 METERS",
                "Its tie to Anu is certain only in Seleucid times, and its match to the Eanna levels remains unsettled.",
                "By the Herald's archaeology desk"),
        };

        variants["anu-stone-building"] = new[]
        {
            new HeadlinePair("A TOMB-SHAPED MYSTERY UNDER THE SKY GOD'S HILL",
                "Three stone boxes nest one inside another, yet no burial and no grave goods were ever found inside.",
                "Letter from the dig house"),
            new HeadlinePair("NESTED MASONRY RECTANGLES AT ANU TERRACE FOOT",
                "Eichmann places it in Levels VI to IV, about 3500 BCE; a drill core was taken before it in 2002.",
                "From our correspondent for antiquities"),
        };

        // Discoveries, tier 7: Uruk IV, the age of the first writing
        variants["temple-c"] = new[]
        {
            new HeadlinePair("FIVE-METER WALLS OF A MILLION BRICKS",
                "Temple C rises on a stepped brick platform, then ends in fire, leveled for new buildings on top.",
                "Special to the Warka Herald"),
            new HeadlinePair("TEMPLE C: 54 BY 22 METERS, LEVEL IVA",
                "First exposed in 1934/35, perhaps seen in 1933/34; its central hall covers 671 square meters.",
                "From the Herald's science desk"),
        };

        variants["room-231-sealings"] = new[]
        {
            new HeadlinePair("PERHAPS PRIESTS BEARING GIFTS, PRESSED IN CLAY",
                "In a ruined room of Temple C, stamped lumps that once closed jars show animals and a temple door.",
                "From our society correspondent"),
            new HeadlinePair("FIND W 15267 MIXES LEVELS IV AND III",
                "Heinrich's 1936 account shows Temple C was also a place where goods were sealed and counted.",
                "From the Society's scientific correspondent"),
        };

        variants["temple-d"] = new[]
        {
            new HeadlinePair("A TEMPLE VAST, ITS GOD UNNAMED",
                "Some 80 by 50 meters, Temple D ranks among the largest buildings of its age in all Mesopotamia.",
                "From the Herald's society page"),
            new HeadlinePair("TEMPLE D DIMENSIONS GIVEN AS 80 BY 50",
                "The figure comes from summaries, not the Getty pages; Nöldeke praised the age's careful narrow-brick walls.",
                "By the Herald's archaeology desk"),
        };

        variants["mountain-seal-tablet"] = new[]
        {
            new HeadlinePair("ANIMALS CLIMB A RANGE OF LITTLE MOUNTAINS",
                "A seal rolled across a clay tablet inside Temple D pictures beasts climbing rows of mountain signs.",
                "Letter from the dig house"),
            new HeadlinePair("SEALED TABLET W 15286 FROM BUILDING D",
                "Catalogued by Heinrich in 1936, it is one of the few finds certainly from inside Temple D.",
                "From our correspondent for antiquities"),
        };

        variants["cone-mosaic-temple"] = new[]
        {
            new HeadlinePair("COLOR CLOTHES THE TEMPLE INSIDE AND OUT",
                "On the ruins of the Stone-Cone Building, a new temple is dressed in cones, even on its inner walls.",
                "Special to the Warka Herald"),
            new HeadlinePair("MOSAIC TEMPLE NAMED IN TENTH REPORT",
                "Heinrich noted inner-wall mosaics on a temple itself, seen before only on terraces and courtyard walls.",
                "From the Herald's science desk"),
        };

        variants["spear-bearer-sealing"] = new[]
        {
            new HeadlinePair("A CAPTIVE'S BOUND HANDS PRESSED IN CLAY",
                "A standing man with a spear and a prisoner with tied hands make a very early picture of power.",
                "From our society correspondent"),
            new HeadlinePair("SEALING W 15196 FROM LAYER IVB",
                "Found beside the half-round pillars; a second number in the library, W 151598, is probably a copying slip.",
                "From the Society's scientific correspondent"),
        };

        variants["pillar-hall-twelve-pillars"] = new[]
        {
            new HeadlinePair("TWELVE PILLARS IN A HALL OF YELLOW",
                "Two hundred thirty-five niches glow with cone mosaics in more than seventy patterns, each planned before it was set.",
                "From the Herald's society page"),
            new HeadlinePair("235 NICHES, OVER SEVENTY PATTERNS RECORDED",
                "Eichmann records twelve rectangular pillars; this hall is distinct from the Round-Pillar Hall.",
                "By the Herald's archaeology desk"),
        };

        variants["great-court"] = new[]
        {
            new HeadlinePair("AN ORCHARD, PERHAPS, BEHIND THE TEMPLE WALLS",
                "A nearly square walled court with its own water channels may have held a garden, though some see processions.",
                "Letter from the dig house"),
            new HeadlinePair("GREAT COURT: GARDEN OR PROCESSIONAL SPACE?",
                "Eichmann reads an orchard with its own water supply; the library's catalogue reads a court for gatherings.",
                "From our correspondent for antiquities"),
        };

        variants["building-e"] = new[]
        {
            new HeadlinePair("WHERE THE CROWDS MAY HAVE GATHERED",
                "Around a great central courtyard, Building E may have hosted large groups opening goods sealed in stamped clay.",
                "Special to the Warka Herald"),
            new HeadlinePair("BUILDING E: 57 METERS SQUARE, DATE DEBATED",
                "Seal-impressed clay fragments came from a square basin; Lenzen placed it first in IVb, later in IVa.",
                "From the Herald's science desk"),
        };

        variants["riemchen-building"] = new[]
        {
            new HeadlinePair("TREASURES WALLED UP FOR FIVE THOUSAND YEARS",
                "The people of Uruk fill an underground building with precious gifts, wall up its doors and walk away.",
                "From our society correspondent"),
            new HeadlinePair("NARROW-BRICK BUILDING USED 261,000 BRICKS",
                "About 18 by 20 meters, it rose right after the Stone-Cone Building fell; ash 1.5 meters thick lies beneath.",
                "From the Society's scientific correspondent"),
        };

        variants["riemchen-deposit"] = new[]
        {
            new HeadlinePair("BLUE LAPIS, RED CARNELIAN, FAR-OFF LANDS",
                "Stone bowls, copper and bright gems lay sealed in the dark, while the wooden furniture they decorated crumbled away.",
                "From the Herald's society page"),
            new HeadlinePair("DEPOSIT DIVIDED AMONG BAGHDAD, BERLIN, HEIDELBERG",
                "No full list was ever published; part of the Baghdad share was stolen in 2003 and only partly recovered.",
                "By the Herald's archaeology desk"),
        };

        variants["warka-vase"] = new[]
        {
            new HeadlinePair("WATER, GRAIN, SHEEP AND MEN BEARING GIFTS!",
                "Band by band the tall vase climbs to a woman, perhaps Holy Inanna, beside Her reed-bundle symbols.",
                "Letter from the dig house"),
            new HeadlinePair("ALABASTER VASE W 14873 STANDS ABOUT 105 CENTIMETERS",
                "Found in fifteen pieces in a Late Uruk hoard in 1933/34; now Iraq Museum IM 19606.",
                "From our correspondent for antiquities"),
        };

        variants["mask-of-warka"] = new[]
        {
            new HeadlinePair("A WOMAN'S FACE RISES FROM THE EARTH!",
                "Its eyes once held inlays, and many scholars believe the life-size stone face shows Holy Inanna.",
                "Special to the Warka Herald"),
            new HeadlinePair("MASK W 17878 MEASURES ABOUT 21 CENTIMETERS",
                "Sources call the stone marble or alabaster; it lay in Level III or the fill where III meets IVa.",
                "From the Herald's science desk"),
        };

        variants["archaic-tablets"] = new[]
        {
            new HeadlinePair("THE DAWN OF WRITING, IN GRAIN AND SHEEP",
                "Some two thousand clay tablets count grain, animals and workers, and a few may already name Holy Inanna.",
                "From our society correspondent"),
            new HeadlinePair("OLDEST URUK TEXTS DATED TO ABOUT 3400 BCE",
                "Thrown into fill already decades or centuries old, the tablets are older than the floors around them.",
                "From the Society's scientific correspondent"),
        };

        variants["late-uruk-cylinder-seals"] = new[]
        {
            new HeadlinePair("A SILVER CALF RIDES A LAPIS SEAL",
                "Roll the little stone across wet clay and a whole picture appears: a ruler in a boat.",
                "From the Herald's society page"),
            new HeadlinePair("THREE CYLINDER SEALS CATALOGUED IN BERLIN",
                "VA 11040, of lapis with a silver calf, shows the 'Great Man of Uruk' before a stepped altar.",
                "By the Herald's archaeology desk"),
        };

        variants["reed-bundle-inlay"] = new[]
        {
            new HeadlinePair("HER LOOPED SYMBOL ONCE GRACED A TEMPLE WALL!",
                "Wherever the reed bundle with its loop appears, scholars think of Holy Inanna, whose name it comes to write.",
                "Letter from the dig house"),
            new HeadlinePair("REED-BUNDLE INLAY VA 14540 DATED LATE FOURTH MILLENNIUM",
                "The looped bundle is the first form of the MÙŠ sign; Jordan called the symbol Her 'ring-bundle.'",
                "From our correspondent for antiquities"),
        };

        variants["cone-mosaic-technique"] = new[]
        {
            new HeadlinePair("WALLS THAT LOOK LIKE WOVEN MATS",
                "Finger-long cones with red, white or black ends are pressed into wet plaster, packed tight into patterns.",
                "Special to the Warka Herald"),
            new HeadlinePair("CONE LENGTHS RANGE FROM 2 TO 30 CENTIMETERS",
                "Colors come from firing heat or paint, set in mud plaster 10 to 20 centimeters thick.",
                "From the Herald's science desk"),
        };

        // Discoveries, tier 6: Level III, the Jemdet Nasr age
        variants["red-temple"] = new[]
        {
            new HeadlinePair("A TEMPLE OF RED, STILL KEEPING ITS SECRETS",
                "Bright three-color cone walls give way here to plain red mosaic and clay picture panels.",
                "From our society correspondent"),
            new HeadlinePair("RED TEMPLE PLAN 'NOT DESCRIBABLE,' SAYS EICHMANN",
                "Walls run almost 1.9 meters thick; part of its cella floor was removed to expose Temple V below.",
                "From the Society's scientific correspondent"),
        };

        variants["uruk-iii-tablets"] = new[]
        {
            new HeadlinePair("WHEN PICTURES LEARNED TO BECOME WEDGES",
                "Under a layer of bricks lay a great pile of tablets, their signs caught halfway between drawing and script.",
                "From the Herald's society page"),
            new HeadlinePair("HALF-PICTOGRAPHIC TABLETS ASSIGNED TO LEVEL III",
                "Jordan's Third Report on 1930/31 describes the deposit; a reported total of 3,094 tablets is unchecked.",
                "By the Herald's archaeology desk"),
        };

        variants["half-round-pillar-terrace"] = new[]
        {
            new HeadlinePair("WHERE THE GREAT TOWERS FIRST BEGIN",
                "A mud-brick terrace with twelve half-round pillars grows into an L-shape faced with cone mosaic panels.",
                "Letter from the dig house"),
            new HeadlinePair("FIRST TERRACE MEASURED 23 BY 19 METERS",
                "It later forms part of an L-shaped terrace about 47 by 45 meters, studied through tunnels.",
                "From our correspondent for antiquities"),
        };

        variants["labyrinth-and-kitchen"] = new[]
        {
            new HeadlinePair("TINY ROOMS AND COOKING PITS OF THE PRECINCT",
                "Beside the terrace, patterned little rooms and long pits probably show the daily work of feeding the holy place.",
                "Special to the Warka Herald"),
            new HeadlinePair("OVAL COOKING PITS MEASURE FIVE METERS LONG",
                "Water for the high precinct had to be raised at least 10 meters from the canals.",
                "From the Herald's science desk"),
        };

        // Discoveries, tier 5: the Early Dynastic city
        variants["rammed-earth-building"] = new[]
        {
            new HeadlinePair("WALLS OF BEATEN EARTH AFTER GREAT BURNT OFFERINGS",
                "Before it rises, the old ruins are cleansed by burnt offerings, and its walls are pounded down layer by layer.",
                "From our society correspondent"),
            new HeadlinePair("DATE OF RAMMED-EARTH BUILDING DISPUTED",
                "Estimates run from about 3100 BCE to the 24th century BCE or later; it exceeds 125 by 80 meters.",
                "From the Society's scientific correspondent"),
        };

        variants["headless-clay-woman"] = new[]
        {
            new HeadlinePair("A LITTLE LADY FROM THE FIRST VILLAGES",
                "Only 4 centimeters tall, the headless clay figure was carried up from the layers of the first villagers far below.",
                "From the Herald's society page"),
            new HeadlinePair("UBAID-STYLE TORSO W 21192 FOUND OUT OF PLACE",
                "Lenzen's 1963/64 find measures 4.1 by 4.5 centimeters and is now in Heidelberg.",
                "By the Herald's archaeology desk"),
        };

        variants["city-wall"] = new[]
        {
            new HeadlinePair("THE WALL THE EPIC GIVES TO GILGAMESH",
                "About 900 half-round towers stood along it, and its long ridge still circles the ruins today.",
                "Letter from the dig house"),
            new HeadlinePair("CITY WALL MEASURED AT ABOUT NINE KILOMETERS",
                "Five to 9 meters thick and built about 2900 BCE; sources give lengths from 8.9 to nearly 10 kilometers.",
                "From our correspondent for antiquities"),
        };

        variants["early-dynastic-terrace"] = new[]
        {
            new HeadlinePair("A GRAND GATEHOUSE TO A SQUARE TERRACE",
                "Through a great gate lies a courtyard and an almost square terrace, built of bricks rounded on one side.",
                "Special to the Warka Herald"),
            new HeadlinePair("PLANO-CONVEX BRICKS MARK THE LEVEL I TERRACE",
                "The terrace measures 52 to 55 by 49 to 52 meters, with vaulted baked-brick drains.",
                "From the Herald's science desk"),
        };

        variants["ziggurat-cutting"] = new[]
        {
            new HeadlinePair("OLDER TOWERS SLEEP INSIDE THE GREAT ONE",
                "Jordan's slot into Her tower found earlier brick towers wrapped one around another, a holy hill already old.",
                "From our society correspondent"),
            new HeadlinePair("CUTTING OF 12.50 METERS EXPOSES EARLIER CORES",
                "Plano-convex outer layers wrap narrow-brick cores of Levels IV and III on the tower's southeast side.",
                "From the Society's scientific correspondent"),
        };

        // Discoveries, tier 4: the Third Dynasty of Ur
        variants["ur-nammu-ziggurat"] = new[]
        {
            new HeadlinePair("HER TOWER HAS STOOD FOR 4,100 YEARS!",
                "King Ur-Nammu raises it for Holy Inanna, with a temple for the Goddess on top, and its core remains.",
                "From the Herald's society page"),
            new HeadlinePair("LOWER TERRACE MEASURES 48 BY 56 METERS",
                "The lower terrace stands 11.2 meters; sources disagree on whether the tower had two terraces or three stages.",
                "By the Herald's archaeology desk"),
        };

        variants["ziggurat-reed-layers"] = new[]
        {
            new HeadlinePair("ANCIENT REEDS STILL LIE WITHIN HER TOWER",
                "Layer upon layer of crossed reed bundles, laid by the builders, have rested between the bricks for 4,100 years.",
                "Letter from the dig house"),
            new HeadlinePair("REED COURSES SPACED 1.2 TO 1.4 METERS",
                "Van Ess suggests the reeds controlled settling and absorbed moisture; twisted reed ropes run through the brickwork.",
                "From our correspondent for antiquities"),
        };

        variants["ur-nammu-stamped-bricks"] = new[]
        {
            new HeadlinePair("THE KING SETS HER NAME BEFORE HIS OWN!",
                "His stamped bricks tell how Ur-Nammu built Her house for Her and restored it to its place.",
                "Special to the Warka Herald"),
            new HeadlinePair("BRICK STAMPS TIE THE PRECINCT TO EANNA",
                "Only Ur-Nammu's formula, naming Holy Inanna as Lady of Eanna, links the precinct and its name for certain.",
                "From the Herald's science desk"),
        };

        variants["ur-nammu-foundation-document"] = new[]
        {
            new HeadlinePair("A KING'S MESSAGE HIDDEN BENEATH THE WALLS",
                "Ur-Nammu's stone document lay in its foundation box, the kind of message later kings sought and honored.",
                "From our society correspondent"),
            new HeadlinePair("FOUNDATION DOCUMENT W 13936 HELD AS VA 10945",
                "The Getty caption calls it a building document from a foundation capsule of the king Ur-Namma.",
                "From the Society's scientific correspondent"),
        };

        variants["ur-nammu-enclosure"] = new[]
        {
            new HeadlinePair("SHRINES AND KITCHENS RING HER TOWER",
                "Barely 2 meters from the tower, the wall's rooms hold small shrines, storerooms, offices and kitchens.",
                "From the Herald's society page"),
            new HeadlinePair("ENCLOSURE WALL STANDS ON RAMMED-EARTH STUMPS",
                "Texts name a kitchen, brewery, storerooms, school and law courts; Jordan traced it in winter 1930/31.",
                "By the Herald's archaeology desk"),
        };

        variants["shulgi-foundation-tablet"] = new[]
        {
            new HeadlinePair("DARK STONE TABLET FOR THE LADY OF EANNA!",
                "Šulgi, son of Ur-Nammu, buries it in Her precinct wall, and builds a little house for Nimintabba.",
                "Letter from the dig house"),
            new HeadlinePair("TABLET W 17304 OF BITUMINOUS LIMESTONE PUBLISHED",
                "Heinrich's Tenth Report records it from a foundation box in the precinct's northwest outer wall.",
                "From our correspondent for antiquities"),
        };

        variants["amar-sin-door-socket"] = new[]
        {
            new HeadlinePair("ONE DOOR STONE SERVES ACROSS FIFTEEN CENTURIES",
                "Amar-Sin's heavy socket, bearing his name, turns a door again for Babylonian builders long after.",
                "Special to the Warka Herald"),
            new HeadlinePair("PIVOT STONE W 18289 REUSED IN GATEWAY",
                "Now in Heidelberg, the Ur III socket was set a second time in a Neo-Babylonian gateway of Eanna.",
                "From the Herald's science desk"),
        };

        variants["tiamatbashti-necklace"] = new[]
        {
            new HeadlinePair("A QUEEN'S BEADS IN THE KINGS' OWN CITY",
                "One bead of Tiamatbashti's necklace carries writing; she was wife of Shu-Sin of Ur, whose dynasty came from Uruk.",
                "From our society correspondent"),
            new HeadlinePair("NECKLACE W 16172 HELD IN IRAQ MUSEUM",
                "The find spot within Uruk is not recorded, and the necklace is not tied to the Eanna.",
                "From the Society's scientific correspondent"),
        };

        // Discoveries, tier 3: Old Babylonian and Kassite
        variants["karaindash-temple"] = new[]
        {
            new HeadlinePair("WATER STREAMS FROM THEIR JARS IN BAKED BRICK!",
                "Some 500 molded bricks join into gods and goddesses in niches on a small Kassite temple for Holy Inanna.",
                "From the Herald's society page"),
            new HeadlinePair("KASSITE BRICK RELIEF STANDS 205 CENTIMETERS",
                "An eleven-line Sumerian dedication runs across the bricks; who the figures are is still debated.",
                "By the Herald's archaeology desk"),
        };

        variants["kurigalzu-pavement"] = new[]
        {
            new HeadlinePair("OLD BRICKS FIND NEW LIFE UNDERFOOT",
                "Her temples are recycled again and again, and some floor bricks still carry King Karaindash's stamp.",
                "Letter from the dig house"),
            new HeadlinePair("PAVEMENT DATED BETWEEN KARAINDASH AND 'THE NAMELESS ONE'",
                "A floor 40 centimeters below a later Kassite pavement reuses bricks of Karaindash's size and stamp.",
                "From our correspondent for antiquities"),
        };

        variants["sin-kashid-palace"] = new[]
        {
            new HeadlinePair("A BURNING PALACE ABANDONED IN HASTE",
                "Letters, contracts and school tablets stayed where they lay when the palace of Sin-kashid burned.",
                "Special to the Warka Herald"),
            new HeadlinePair("SCRIBAL SCHOOL ATTESTED AT SIN-KASHID PALACE",
                "First noticed in 1912/13, it was dug on a large scale from 1958 and fully uncovered by 1962.",
                "From the Herald's science desk"),
        };

        variants["shallurtum-sealing"] = new[]
        {
            new HeadlinePair("ROYAL SEAL OF A QUEEN, PRESSED IN CLAY",
                "Some 3,900 years ago, Queen Shallurtum stamps a lump of clay to close a jar of goods.",
                "From our society correspondent"),
            new HeadlinePair("SEALING W 20212,2 DATED NINETEENTH CENTURY BCE",
                "Now in the Iraq Museum; Gebhard Selz notes that queens had a strong part in the early economy.",
                "From the Society's scientific correspondent"),
        };

        // Discoveries, tier 2: Neo-Assyrian, Neo-Babylonian and Persian
        variants["deep-temples"] = new[]
        {
            new HeadlinePair("CURTAINED SHRINES AT THE FOOT OF HER TOWER!",
                "With no door hinges in their inner doorways, Jordan thought only curtains closed the holy rooms.",
                "From the Herald's society page"),
            new HeadlinePair("DEEP TEMPLES' DATE DISPUTED, BUT LATE",
                "The Getty book places them in the late eighth century BCE, the library under Nabonidus; neither is Late Uruk.",
                "By the Herald's archaeology desk"),
        };

        variants["clay-lion"] = new[]
        {
            new HeadlinePair("A LITTLE LION STALKS BENEATH THE FLOOR",
                "Under the floor before the holy niche, the diggers found a crouching lion of unbaked clay with two written dedications.",
                "Letter from the dig house"),
            new HeadlinePair("UNBAKED CLAY LION NO. 10008 RECORDED",
                "Jordan judged it Neo-Babylonian by style, though its writing looks older; it went to Baghdad.",
                "From our correspondent for antiquities"),
        };

        variants["cyrus-bricks"] = new[]
        {
            new HeadlinePair("THE PERSIAN KING'S NAME UNDERFOOT IN HER TEMPLE",
                "Bricks naming Cyrus, conqueror of Babylon, show Her temple still in use more than 1,500 years after Ur-Nammu.",
                "Special to the Warka Herald"),
            new HeadlinePair("CYRUS II PAVEMENT FOUND IN PLACE",
                "Found in the last weeks of 1930/31 against the East Temple, it shows use into the time of Cyrus.",
                "From the Herald's science desk"),
        };

        variants["nabonidus-bricks"] = new[]
        {
            new HeadlinePair("BABYLON'S LAST KING LEAVES HIS NAME IN BRICK",
                "Nabonidus's stamped floors showed the archaeologists that the little Deep Temples were not early but late.",
                "From our society correspondent"),
            new HeadlinePair("NABONIDUS STAMPS DATE TWIN TEMPLES",
                "The temples stand at the east corner of the Round-Pillar Hall court; their deity is unknown.",
                "From the Society's scientific correspondent"),
        };

        variants["sargon-nebuchadnezzar-walls"] = new[]
        {
            new HeadlinePair("FOUR KINGS' NAMES MINGLE IN BROKEN BRICK",
                "Sargon II lays out a greater wall around the old one, and Nebuchadnezzar II rebuilds it in stamped brick.",
                "From the Herald's society page"),
            new HeadlinePair("STAMPS OF AMAR-SIN, UR-NAMMU, KARAINDASH, SARGON",
                "Heinrich listed them in the rubble; Nebuchadnezzar's stamped bricks fill the outer wall's upper courses.",
                "By the Herald's archaeology desk"),
        };

        variants["goddess-stele-fragment"] = new[]
        {
            new HeadlinePair("A STONE GODDESS IN AN OLDER TEMPLE",
                "Carved centuries after Karaindash died, the broken slab shows his small temple was still holy.",
                "Letter from the dig house"),
            new HeadlinePair("STELE FRAGMENT W 18499 FROM KARAINDASH'S TEMPLE",
                "The incised limestone goddess is not named; the fragment is in Heidelberg's Uruk-Warka collection.",
                "From our correspondent for antiquities"),
        };

        variants["priests-houses"] = new[]
        {
            new HeadlinePair("AT HOME WITH THE PRIESTS OF HOLY INANNA",
                "Beside Her sanctuary, the priests keep clay-tablet libraries at home and bury family members beneath the floors.",
                "Special to the Warka Herald"),
            new HeadlinePair("PRIESTLY HOUSES DATED SEVENTH TO SIXTH CENTURY",
                "Lenzen's teams dug them after 1953/54; one known priest is the temple baker Bel-supe-mukhur.",
                "From the Herald's science desk"),
        };

        // Discoveries, tier 1: the Seleucid surface
        variants["resh-sanctuary"] = new[]
        {
            new HeadlinePair("LOOTERS' TEMPLE GREETS THE FIRST EXPEDITION",
                "Walls 7 meters high stood before the newcomers in November 1912, already pitted by robbers seeking tablets.",
                "From our society correspondent"),
            new HeadlinePair("RESH SANCTUARY EXCEEDS 35,000 SQUARE METERS",
                "Too big to clear, it was probed in narrow soundings and tunnels from November 1912 to May 1913.",
                "From the Society's scientific correspondent"),
        };

        variants["resh-glazed-bricks"] = new[]
        {
            new HeadlinePair("SHINING LIONS AND STARS FROM BROKEN WALLS",
                "Fitted together, the glazed pieces show stars, lions and winged monsters, much like Babylon's Ishtar Gate.",
                "From the Herald's society page"),
            new HeadlinePair("GLAZED RELIEF BRICKS RECOVERED, WINTER 1912/13",
                "Sources differ between lion-griffins and bull-griffins; Conrad Preusser painted the bricks put back together in 1913.",
                "By the Herald's archaeology desk"),
        };

        variants["eshgal"] = new[]
        {
            new HeadlinePair("A NEW TEMPLE FOR ISHTAR AND NANAYA!",
                "In Uruk's last centuries, baked-brick walls rise south of the old Eanna, still standing up to 8 meters.",
                "Letter from the dig house"),
            new HeadlinePair("ESHGAL WALLS SURVIVE TO EIGHT METERS",
                "Dug in winter 1932/33 at Walter Andrae's wish; most of the Seleucid temple remains unexcavated.",
                "From our correspondent for antiquities"),
        };

        variants["ziggurat-mantle"] = new[]
        {
            new HeadlinePair("HER TOWER SHEDS ITS LAST COAT",
                "Under a late wrap of mud brick, Jordan's crew found the old stairs and two small temples waiting.",
                "Special to the Warka Herald"),
            new HeadlinePair("MANTLE DATED AFTER CYRUS, PROBABLY SELEUCID",
                "Jordan left small patches on purpose; photographs from 1928/29 show the coat still in place.",
                "From the Herald's science desk"),
        };

        // Seasons, 1912/13 to 1938/39
        variants["1912/13"] = new[]
        {
            new HeadlinePair("TENTS AND REED HUTS RISE AT WARKA",
                "Under an Ottoman permit, the first team mapped the whole city and tunneled into a great looted temple.",
                "From our society correspondent"),
            new HeadlinePair("FIRST TRUE MAP OF URUK DRAWN",
                "Jordan and Preusser worked from 13 November 1912 to 16 May 1913 for the German Oriental Society.",
                "From the Society's scientific correspondent"),
        };

        variants["1928/29"] = new[]
        {
            new HeadlinePair("SPADES RETURN AFTER WAR AND A NEW NATION",
                "Gertrude Bell's law and goodwill opened the way back, and Jordan chose Eanna, the house of Holy Inanna.",
                "From the Herald's society page"),
            new HeadlinePair("KARAINDASH TEMPLE FOUND IN FIRST SEASON BACK",
                "Finds were divided between Berlin and the Iraq Museum; two rebuilt Ford cars carried the team.",
                "By the Herald's archaeology desk"),
        };

        variants["1929/30"] = new[]
        {
            new HeadlinePair("A LITTLE CHRISTMAS TREE ON THE TEA TABLE",
                "In a half-built dig house, the team kept Christmas after the trench reached the Red Temple.",
                "Letter from the dig house"),
            new HeadlinePair("RED TEMPLE ASSIGNED TO LEVEL III",
                "Limestone footings of a Level V temple were first partly recognized; Jordan's Second Report appeared in 1930.",
                "From our correspondent for antiquities"),
        };

        variants["1930/31"] = new[]
        {
            new HeadlinePair("A WINTER OF WONDERS UNDER HER TOWER",
                "Jordan's last season brought a stone-footed temple, cone-clad pillars and two small temples hidden beneath a mud mantle.",
                "Special to the Warka Herald"),
            new HeadlinePair("DEEP TRENCH ENLARGED TO 29 BY 21 METERS",
                "Rudolf Michaelis lifted cone mosaics for Berlin; Arnold Nöldeke took over when Jordan left in 1931.",
                "From the Herald's science desk"),
        };

        variants["1931/32"] = new[]
        {
            new HeadlinePair("YOUNG HANDS FEEL FOR THE MELTED WALLS",
                "With a long pointed tool, Iraqi lads found where set brick ended and softer mortar began.",
                "From our society correspondent"),
            new HeadlinePair("NÖLDEKE DIVIDES THE FIELD AMONG HIS ARCHITECTS",
                "Lenzen took Eanna and Heinrich the Anu ziggurat; the Niched Building appeared in the Fourth Report.",
                "From the Society's scientific correspondent"),
        };

        variants["1932/33"] = new[]
        {
            new HeadlinePair("A SECOND TEMPLE OF THE LOVE GODDESS",
                "At Walter Andrae's wish, the team turned to the Eshgal, as Iraq became a fully independent country.",
                "From the Herald's society page"),
            new HeadlinePair("SELEUCID ESHGAL EXCAVATED AT ANDRAE'S REQUEST",
                "Temple D sections appeared in the Fifth Report; from 1933 the team traveled by ship and desert bus.",
                "By the Herald's archaeology desk"),
        };

        variants["1933/34"] = new[]
        {
            new HeadlinePair("FIFTEEN PIECES OF A PROCESSION OF GIFTS",
                "In Ernst Heinrich's one winter as leader, the tall carved vase came up from a buried hoard.",
                "Letter from the dig house"),
            new HeadlinePair("VASE SEASON RESTS ON A SINGLE NOTE",
                "The Getty book confirms the alabaster vase but not the winter; the files disagree on where the hoard lay.",
                "From our correspondent for antiquities"),
        };

        variants["1934/35"] = new[]
        {
            new HeadlinePair("WRITING UP THE FINDS BY PETROLEUM LAMP",
                "Days ran from 6:30 to 5:00, while limits on money leaving Germany began to bite.",
                "Special to the Warka Herald"),
            new HeadlinePair("SURVEY GRID FIXED OVER EANNA",
                "W. Goepner's grid points became the reference for all later Eanna plans; Temple C was first exposed.",
                "From the Herald's science desk"),
        };

        variants["1935/36"] = new[]
        {
            new HeadlinePair("UPRISINGS NEARBY, YET THE WORK GOES ON",
                "Tribes near Rumaitha and Samawa rose, and Walter Andrae fought for the dig's money in Berlin.",
                "From our society correspondent"),
            new HeadlinePair("LEVEL IV BUILDINGS DRAWN SEASON BY SEASON",
                "No new building is named for this winter; the uprisings lay 50 and 15 kilometers away.",
                "From the Society's scientific correspondent"),
        };

        variants["1936/37"] = new[]
        {
            new HeadlinePair("A HOUSE OF POUNDED EARTH, AGAIN AND AGAIN",
                "The diggers kept meeting the great earthen building while the Foreign Office stepped in to pay.",
                "From the Herald's society page"),
            new HeadlinePair("NEW IRAQI LAW CHANGES DIVISION OF FINDS",
                "The 1936 law ended generous export terms; on 4 March 1937 the research foundation cut its funds.",
                "By the Herald's archaeology desk"),
        };

        variants["1937/38"] = new[]
        {
            new HeadlinePair("SEVEN HOURS TO CROSS THE FLOODED EUPHRATES",
                "That December the river ran high, and Heinrich's dating trench could not match the two holy precincts.",
                "Letter from the dig house"),
            new HeadlinePair("EANNA AND ANU LAYERS REMAIN UNMATCHED",
                "Heinrich's special dating trench found no certain answer; his Tenth Report named the Mosaic Temple.",
                "From our correspondent for antiquities"),
        };

        variants["1938/39"] = new[]
        {
            new HeadlinePair("A STONE FACE GREETS THE LAST WINTER",
                "On 22 February 1939 the Mask of Warka came to light, months before war closed the dig.",
                "Special to the Warka Herald"),
            new HeadlinePair("FINAL PREWAR PERMIT SIGNED BY GÖRING",
                "Work stopped in September 1939, leaving the Narrow-Brick Building half-dug for fourteen years.",
                "From the Herald's science desk"),
        };
    }
}
