using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    //The first edition and two further editions (straight news, society page, scientific correspondent) for every
    //  second-pass discovery in Catalog.Discoveries2.cs; every banner is truthful to its card
    static partial void AddSecondPassHeadlines(Dictionary<string, HeadlinePair> table, Dictionary<string, HeadlinePair[]> variants)
    {
        // eshgal-cult-statue (tier 1)
        table["eshgal-cult-statue"] = new HeadlinePair("HOLY STATUE OF ISHTAR FOUND IN HER TEMPLE",
            "A burnt fig-wood torso lay beside the Eshgal's main pedestal: the only Babylonian cult statue ever dug up.",
            "From our correspondent at the dig");
        variants["eshgal-cult-statue"] = new[]
        {
            new HeadlinePair("THE GODDESS HERSELF, CARVED IN FIG WOOD!",
                "Her robe falls in wavy strips, and once, the museum believes, precious metal and gems covered Her wooden body.",
                "From our society editor"),
            new HeadlinePair("ONLY EXCAVATED BABYLONIAN CULT IMAGE IDENTIFIED",
                "A pegged fig-wood torso, 32 centimeters high, survived the temple fire; the museum says it probably shows Ishtar.",
                "From the Society's scientific correspondent"),
        };

        // gareus-temple (tier 1)
        table["gareus-temple"] = new HeadlinePair("TEMPLE OF AN UNKNOWN GOD, DATED 111",
            "A Greek decree found beside it gives the year in which Uruk raised a temple to the god Gareus.",
            "By wire from Baghdad");
        variants["gareus-temple"] = new[]
        {
            new HeadlinePair("GREEK COLUMNS IN THE CITY OF THE GODDESS",
                "Uruk's last builders borrowed the graceful style of Greece and Rome for a little temple to a mysterious god.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("GAREUS TEMPLE DATED BY GREEK INSCRIPTION",
                "The Getty volume places the building in the east of the city in one chapter and the south in another.",
                "From our science correspondent"),
        };

        // gareus-sea-dragon-brick (tier 1)
        table["gareus-sea-dragon-brick"] = new HeadlinePair("WINGED SEA DRAGON ON A TEMPLE BRICK",
            "A relief brick from the blind arches of the Gareus temple shows a dragon of the sea, wings spread.",
            "From our special correspondent at Warka");
        variants["gareus-sea-dragon-brick"] = new[]
        {
            new HeadlinePair("A DRAGON GUARDS THE LAST TEMPLE OF URUK",
                "Molded in baked clay, the winged beast once looked down from the arches of the temple front.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("RELIEF IMPOST FROM LATE PARTHIAN FAÇADE",
                "The museum dates the dragon fragment, 21.5 centimeters wide, to 111 CE, the year of the Greek decree.",
                "From the Society's correspondent for antiquities"),
        };

        // nikarchos-inscription (tier 1)
        table["nikarchos-inscription"] = new HeadlinePair("GOVERNOR REBUILDS THE SKY GOD'S TEMPLE",
            "Anu-uballit Nikarchos records new gates, seven courtyards and a gold crown for the Resh in 244 BCE.",
            "From our Berlin correspondent");
        variants["nikarchos-inscription"] = new[]
        {
            new HeadlinePair("THE GODS MOVE INTO THEIR NEW HOME!",
                "On a spring day the sky god Anu and his wife Antu took their seats in their shrine forever.",
                "From our society correspondent abroad"),
            new HeadlinePair("SELEUCID BUILDING TEXT DATED TO APRIL 244",
                "Oracc gives 18 April 244 BCE for the gods' entry; the Getty volume prefers the year 243.",
                "From our correspondent at the museum stores"),
        };

        // kephalon-bricks (tier 1)
        table["kephalon-bricks"] = new HeadlinePair("KEPHALON STAMPS HIS NAME ON THE RESH",
            "A second Anu-uballit rebuilt the rooms of Anu and Antu and put his inscription on baked bricks.",
            "By cable from Basra");
        variants["kephalon-bricks"] = new[]
        {
            new HeadlinePair("CEDAR DOORS FOR THE GODS OF HEAVEN",
                "Kephalon brought cedars from a strong mountain to roof the shrines of the sky god and his wife.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("STAMPED BRICKS DATED TO SELEUCID YEAR 110",
                "Oracc gives 202 BCE for the text; Berlin dates its stamped bricks variously, from 201 BCE onward.",
                "From the Society's correspondent for ancient languages"),
        };

        // seleucid-royal-bullae (tier 1)
        table["seleucid-royal-bullae"] = new HeadlinePair("A KING'S FACE PRESSED INTO CLAY",
            "A sealing from Seleucid Uruk shows King Antiochus IV with rays around his head and Greek letters beside him.",
            "From our correspondent in the Eanna trenches");
        variants["seleucid-royal-bullae"] = new[]
        {
            new HeadlinePair("THE STRING IS STILL INSIDE!",
                "Bits of the cord that tied a lost document survive in the little clay seal of a Seleucid king.",
                "From our society editor"),
            new HeadlinePair("SELEUCID BULLAE BEAR ROYAL PORTRAITS",
                "Impressions of Antiochus IV and Demetrios I survive on sealings once fastened to papyrus or parchment documents.",
                "From the Society's scientific correspondent"),
        };

        // star-distance-tablet (tier 1)
        table["star-distance-tablet"] = new HeadlinePair("PRIESTS MEASURE THE SPACE BETWEEN STARS",
            "A clay tablet from Seleucid Uruk names constellations and lists the distances between single stars.",
            "From our correspondent at the dig");
        variants["star-distance-tablet"] = new[]
        {
            new HeadlinePair("THE NIGHT SKY WRITTEN IN CLAY",
                "Uruk's star-watchers kept a calendar for the sign of the Lion and charted the heavens line by line.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("SELEUCID STAR-DISTANCE LIST IN BERLIN",
                "VAT 16436, inscribed on front, back and edge, joins a zodiac calendar of about 200 BCE.",
                "From our science correspondent"),
        };

        // last-dated-tablets (tier 1)
        table["last-dated-tablets"] = new HeadlinePair("LAST DATED TABLETS OF URUK IDENTIFIED",
            "A contract of 108 BCE and a sky diary of 99 to 97 BCE close three thousand years of writing.",
            "By wire from Baghdad");
        variants["last-dated-tablets"] = new[]
        {
            new HeadlinePair("FAREWELL TO THE WEDGE-WRITING OF URUK",
                "Only a handful of priests still kept the old script alive for the ancient gods.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("TERMINAL DATES FOR URUK CUNEIFORM: 108 AND 97",
                "Potts names the city's latest securely dated texts; the last dated cuneiform tablet anywhere comes from 75 CE.",
                "From the Society's correspondent for antiquities"),
        };

        // frehat-en-nufeji-tumuli (tier 1)
        table["frehat-en-nufeji-tumuli"] = new HeadlinePair("GREEK-STYLE TOMBS FOUND NORTH OF URUK",
            "Two burial mounds held the burned remains of Seleucid nobles, with gifts laid beside them.",
            "From our special correspondent at Warka");
        variants["frehat-en-nufeji-tumuli"] = new[]
        {
            new HeadlinePair("NOBLES OF URUK SLEEP IN GRAND MOUNDS",
                "Uruk's highest families chose Greek fashions even in death, far from the crowded city.",
                "From our society correspondent abroad"),
            new HeadlinePair("CREMATION TUMULI BESIDE BABYLONIAN POT GRAVES",
                "Hellenistic and Babylonian burial customs existed side by side in Seleucid Uruk, the Getty volume reports.",
                "From our correspondent at the museum stores"),
        };

        // loftus-parthian-plaster (tier 1)
        table["loftus-parthian-plaster"] = new HeadlinePair("ENGLISHMAN'S 1853 FINDS FROM PARTHIAN HOUSES",
            "William Loftus dug Uruk's top layers and sent painted wall plaster to the British Museum.",
            "From our Berlin correspondent");
        variants["loftus-parthian-plaster"] = new[]
        {
            new HeadlinePair("PAINTED WALLS OF URUK'S LAST GRAND HOUSES",
                "The fine villas of the Parthian age had painted plaster walls, as Loftus's fragments show.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("LOFTUS'S PLASTER: SOURCES SPLIT ON 1853 OR 1854",
                "The Getty volume dates his three-month stay to 1853; some library files give 1854 for his excavation.",
                "From the Society's correspondent for ancient languages"),
        };

        // bit-akitu (tier 1)
        table["bit-akitu"] = new HeadlinePair("NEW YEAR HOUSE STANDS OUTSIDE THE WALLS",
            "The Babylonian spring festival was celebrated in a holy building northeast of the city.",
            "By cable from Basra");
        variants["bit-akitu"] = new[]
        {
            new HeadlinePair("WHERE URUK GREETED THE SPRING",
                "Far from the crowded temples, a festival house welcomed the New Year at the equinox.",
                "From our society editor"),
            new HeadlinePair("AKITU HOUSE AMONG SELEUCID SACRAL COMPLEXES",
                "The German Archaeological Institute's visualisation project reconstructed it beside the Resh and the Irigal.",
                "From the Society's scientific correspondent"),
        };

        // venus-hymn-tablet (tier 1)
        table["venus-hymn-tablet"] = new HeadlinePair("HYMN TO THE GODDESS AS VENUS",
            "A Seleucid tablet from Uruk sings to Ishtar, the later name of Holy Inanna, shining as the planet Venus.",
            "From our correspondent in the Eanna trenches");
        variants["venus-hymn-tablet"] = new[]
        {
            new HeadlinePair("SHE SHINES AS THE EVENING STAR",
                "Even in Uruk's last centuries, scribes copied songs to the Goddess who lights the sky.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("LOUVRE TABLET AO 6458: ISHTAR AS VENUS",
                "The Getty volume publishes the Seleucid hymn among the evidence for Her astral form.",
                "From our science correspondent"),
        };

        // walnut-calf-dagger (tier 2)
        table["walnut-calf-dagger"] = new HeadlinePair("WOODEN DAGGER WITH A CALF'S HEAD FOUND",
            "A walnut dagger came out of a Babylonian grave made of two clay pots, 2,800 years old.",
            "From our correspondent at the dig");
        variants["walnut-calf-dagger"] = new[]
        {
            new HeadlinePair("A CALF CARVED IN WALNUT FOR THE GRAVE",
                "Someone of old Uruk was laid to rest with a finely carved wooden dagger.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("WALNUT DAGGER W 21731,1 FROM A POT BURIAL",
                "The Getty caption calls the grave Neo-Babylonian and dates it to the eighth century BCE.",
                "From the Society's correspondent for antiquities"),
        };

        // double-pot-grave-bronzes (tier 2)
        table["double-pot-grave-bronzes"] = new HeadlinePair("BRONZE BOWL AND CUP IN A POT GRAVE",
            "Grave goods were found exactly where they were placed, before the bent leg of the dead.",
            "By wire from Baghdad");
        variants["double-pot-grave-bronzes"] = new[]
        {
            new HeadlinePair("A RIVETED BOWL BESIDE THE SLEEPER",
                "A bronze bowl with a little cup inside lay by the bent leg of someone buried in two pots.",
                "From our society correspondent abroad"),
            new HeadlinePair("GRAVE GOODS RECORDED IN POSITION",
                "The museum notes the cup stood within the bowl, before the flexed left lower leg of the burial.",
                "From our correspondent at the museum stores"),
        };

        // childrens-grave-necklaces (tier 2)
        table["childrens-grave-necklaces"] = new HeadlinePair("BEADED NECKLACES IN CHILDREN'S GRAVES",
            "Carnelian, agate and lapis beads were laid with Uruk's young dead, one string with a frog pendant.",
            "From our special correspondent at Warka");
        variants["childrens-grave-necklaces"] = new[]
        {
            new HeadlinePair("TINY FROG CHARM FOR A CHILD OF URUK",
                "Little ones of old Uruk were laid to rest with necklaces of shining stone.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("CHILD BURIALS YIELD MIXED-STONE NECKLACES",
                "The museum lists carnelian, agate, rock crystal, lapis lazuli, limestone and Egyptian blue beads.",
                "From the Society's correspondent for ancient languages"),
        };

        // clay-camels (tier 2)
        table["clay-camels"] = new HeadlinePair("BRANDED CLAY CAMEL FOUND AT URUK",
            "A little dromedary carries its owner's mark, and a clay boat on wheels holds an incense burner.",
            "From our Berlin correspondent");
        variants["clay-camels"] = new[]
        {
            new HeadlinePair("CAMELS COME TO THE CITY OF THE GODDESS",
                "Clay dromedaries, some with saddles, turn up again and again in Babylonian Uruk.",
                "From our society editor"),
            new HeadlinePair("DROMEDARY FIGURINE BEARS OWNER'S BRAND",
                "The Getty volume calls the incised mark a wasm; Berlin holds further camel figurines of the eighth to sixth centuries.",
                "From the Society's scientific correspondent"),
        };

        // game-board-bricks (tier 2)
        table["game-board-bricks"] = new HeadlinePair("ANCIENT GAME BOARDS FOUND ON BRICKS",
            "Grids scratched into two mud bricks were probably used for a board game, the museum says.",
            "By cable from Basra");
        variants["game-board-bricks"] = new[]
        {
            new HeadlinePair("WHAT GAMES DID URUK PLAY?",
                "Somebody idled away the hours with a game scratched into a humble brick.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("INCISED GRIDS INTERPRETED AS GAMING BOARDS",
                "VA 16226 bears three long and eleven short lines; its rules remain unknown.",
                "From our science correspondent"),
        };

        // incantation-tablets (tier 2)
        table["incantation-tablets"] = new HeadlinePair("SPELLS FOR SICK CHILDREN ON CLAY",
            "Two cuneiform tablets from Uruk hold incantations, one for ailing children and one against Lamashtu.",
            "From our correspondent in the Eanna trenches");
        variants["incantation-tablets"] = new[]
        {
            new HeadlinePair("SPELLS TO KEEP THE LITTLE ONES SAFE",
                "One clay tablet holds words meant to help children who fell ill.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("INCANTATION TABLETS VAT 14505 AND 14506",
                "One text runs to twenty and eighteen lines; the other bears five pricked holes on one side.",
                "From the Society's correspondent for antiquities"),
        };

        // kudurru-deed-stone (tier 2)
        table["kudurru-deed-stone"] = new HeadlinePair("STONE OF THE GODS' SIGNS UNEARTHED",
            "A weathered limestone slab carries three rows of divine symbols and a long inscription.",
            "From our correspondent at the dig");
        variants["kudurru-deed-stone"] = new[]
        {
            new HeadlinePair("SCORPION, DOG AND LAMP CARVED IN STONE",
                "The gods themselves seem to stand guard over this mysterious carved document.",
                "From our society correspondent abroad"),
            new HeadlinePair("KUDURRU VA 15193 CATALOGUED IN BERLIN",
                "Its symbol rows include horned crowns on bases, staffs, lightning and a turtle; the text awaits translation here.",
                "From our correspondent at the museum stores"),
        };

        // humbaba-plaque (tier 2)
        table["humbaba-plaque"] = new HeadlinePair("HUMBABA, MONSTER OF THE CEDARS, IN CLAY",
            "A molded plaque shows the forest guardian of the Gilgamesh tales beneath a figure with two swords.",
            "By wire from Baghdad");
        variants["humbaba-plaque"] = new[]
        {
            new HeadlinePair("GILGAMESH'S FOE TURNS UP AT HIS OWN CITY",
                "The fearsome guardian of the Cedar Forest appears on a little clay picture from Uruk.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("MOLD-MADE HUMBABA RELIEF, SIXTH CENTURY",
                "The museum dates VA 11545 to about 550 to 500 BCE and notes possible lion heads flanking the face.",
                "From the Society's correspondent for ancient languages"),
        };

        // geometry-problem-tablet (tier 3)
        table["geometry-problem-tablet"] = new HeadlinePair("MATH HOMEWORK FROM 3,800 YEARS AGO",
            "A clay tablet from Uruk sets a geometry problem and draws the shape beside it.",
            "From our special correspondent at Warka");
        variants["geometry-problem-tablet"] = new[]
        {
            new HeadlinePair("THE SCRIBES OF URUK COULD DO SUMS!",
                "Young writers of the Old Babylonian age learned to calculate in clay as well as to write.",
                "From our society editor"),
            new HeadlinePair("OLD BABYLONIAN PROBLEM TEXT WITH DIAGRAM",
                "VAT 07621 joins a two-sided mathematical fragment; base-sixty place value underlies such calculations.",
                "From the Society's scientific correspondent"),
        };

        // shamash-saw-seal (tier 3)
        table["shamash-saw-seal"] = new HeadlinePair("SUN GOD ON A CRYSTAL SEAL",
            "A tiny rock-crystal seal from Old Babylonian Uruk probably shows Shamash holding his saw.",
            "From our Berlin correspondent");
        variants["shamash-saw-seal"] = new[]
        {
            new HeadlinePair("HER BROTHER THE SUN, IN CLEAR CRYSTAL",
                "Holy Inanna's brother steps forward on a seal no taller than a fingertip.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("WORN SEAL SHOWS SHAMASH AND A WARRIOR",
                "The museum reads the long object as the god's saw with caution, and the second figure as a warrior king.",
                "From our science correspondent"),
        };

        // praying-goddess-seal (tier 3)
        table["praying-goddess-seal"] = new HeadlinePair("A PRIEST'S OWN SEAL COMES TO LIGHT",
            "A chalcedony seal shows a goddess praying for its owner, whom a five-line inscription names as a priest.",
            "By cable from Basra");
        variants["praying-goddess-seal"] = new[]
        {
            new HeadlinePair("A GODDESS PRAYS FOR HIM",
                "With both hands raised, a horned-crowned goddess pleads for a priest of old Uruk.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("INTERCEDING GODDESS ON CHALCEDONY SEAL",
                "The museum describes a many-horned crown, a flounced robe and a five-line owner's legend.",
                "From the Society's correspondent for antiquities"),
        };

        // heroes-and-beasts-seal (tier 3)
        table["heroes-and-beasts-seal"] = new HeadlinePair("HERO WRESTLES BULL ON A TINY SEAL",
            "A hematite seal from Old Babylonian Uruk packs two battles of heroes and beasts into 1.8 centimeters.",
            "From our correspondent in the Eanna trenches");
        variants["heroes-and-beasts-seal"] = new[]
        {
            new HeadlinePair("MONSTERS BATTLE UNDER A CRESCENT MOON",
                "A bull-man, a winged lion-bird and a rearing lion fight on a jewel of polished stone.",
                "From our society correspondent abroad"),
            new HeadlinePair("CONTEST SCENE IN TWO GROUPS, VA 12877",
                "The museum identifies a six-curled hero, a bull-man and a horned lion-bird hybrid.",
                "From our correspondent at the museum stores"),
        };

        // sin-kashid-building-document (tier 3)
        table["sin-kashid-building-document"] = new HeadlinePair("KING SIN-KASHID'S BUILDING RECORD SURFACES",
            "A small clay document of the Old Babylonian king, probably from Uruk, is catalogued in Berlin.",
            "From our correspondent at the dig");
        variants["sin-kashid-building-document"] = new[]
        {
            new HeadlinePair("A ROYAL BUILDER'S PROMISE IN CLAY",
                "The king who built a palace at Uruk and tended Her sanctuary left his words on clay.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("VAT 30187 ACQUIRED, NOT EXCAVATED",
                "The museum gives Germany as the place of acquisition and Uruk as the probable origin.",
                "From the Society's correspondent for ancient languages"),
        };

        // ur-nammu-basket-figure (tier 4)
        table["ur-nammu-basket-figure"] = new HeadlinePair("KING'S COPPER FIGURE BURIED FOR THE GODDESS",
            "Ur-Nammu carries a basket of earth on his head in a foundation figure for Her temple Eanna.",
            "By wire from Baghdad");
        variants["ur-nammu-basket-figure"] = new[]
        {
            new HeadlinePair("THE KING HIMSELF CARRIES THE BRICKS' EARTH",
                "Humble before Holy Inanna, Ur-Nammu shows himself as a worker building Her House of Heaven.",
                "From our society editor"),
            new HeadlinePair("UR III FOUNDATION PEG NAMES EANNA",
                "The British Museum's figure 113896 bears nine and three lines recording the restoration of Inanna's temple.",
                "From the Society's scientific correspondent"),
        };

        // shulgi-basket-figure (tier 4)
        table["shulgi-basket-figure"] = new HeadlinePair("ŠULGI'S BRONZE FOUNDATION FIGURE FOUND",
            "Ur-Nammu's son buried his own basket-carrier, inscribed with his name, with a tablet beside it.",
            "From our special correspondent at Warka");
        variants["shulgi-basket-figure"] = new[]
        {
            new HeadlinePair("LIKE FATHER, LIKE SON",
                "King Šulgi followed Ur-Nammu in carrying the builder's basket for the gods of Uruk.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("INSCRIBED BASKET-BEARER VA 15192 B",
                "Berlin pairs the 24.7-centimeter bronze with tablet VA 15192 a; its link to W 17304 is unconfirmed.",
                "From our science correspondent"),
        };

        // clay-mother-and-baby (tier 4)
        table["clay-mother-and-baby"] = new HeadlinePair("CLAY MOTHER WITH HER BABY, 4,000 YEARS OLD",
            "A mold-made figure from Uruk shows a woman cradling a baby in her arms.",
            "From our Berlin correspondent");
        variants["clay-mother-and-baby"] = new[]
        {
            new HeadlinePair("A TENDER MOMENT PRESSED IN CLAY",
                "Curly-haired and belted, a young mother of Uruk holds her little one close.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("MOLD-MADE FIGURINE RETAINS TRACES OF GLAZE",
                "The museum dates VA 11620 to about 2150 to 2000 BCE and notes the infant's simpler modeling.",
                "From the Society's correspondent for antiquities"),
        };

        // horned-god-figure (tier 4)
        table["horned-god-figure"] = new HeadlinePair("CLAY GOD IN A HORNED CAP FOUND",
            "A molded figure from Uruk wears the three-tiered horned cap that marks a god.",
            "By cable from Basra");
        variants["horned-god-figure"] = new[]
        {
            new HeadlinePair("A GOD WITH A GRAND BEARD AND HORNS",
                "Stern and splendid, a little clay deity folds his hands upon his chest.",
                "From our society correspondent abroad"),
            new HeadlinePair("THREE-ROW HORNED CAP ON TERRACOTTA",
                "The museum dates the molded divine figure VA 11636 to about 2200 to 2000 BCE.",
                "From our correspondent at the museum stores"),
        };

        // ur-iii-bead-necklaces (tier 4)
        table["ur-iii-bead-necklaces"] = new HeadlinePair("LAPIS AND CARNELIAN NECKLACES RESTRUNG",
            "Two bead necklaces of the Ur III age come from Uruk, one with a large agate disc.",
            "From our correspondent in the Eanna trenches");
        variants["ur-iii-bead-necklaces"] = new[]
        {
            new HeadlinePair("JEWELS FIT FOR THE COURT OF UR",
                "Deep-blue lapis and fiery carnelian adorned the people of Uruk four thousand years ago.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("UR III NECKLACES OF MIXED STONES CATALOGUED",
                "The museum lists lapis lazuli, carnelian, rock crystal, limestone and agate beads.",
                "From the Society's correspondent for ancient languages"),
        };

        // ziggurat-rain-drains (tier 4)
        table["ziggurat-rain-drains"] = new HeadlinePair("TOWER'S RAIN GUTTERS FOUND IN BAKED BRICK",
            "Ur-Nammu's builders ran drains down Holy Inanna's ziggurat to protect its mud bricks from storms.",
            "From our correspondent at the dig");
        variants["ziggurat-rain-drains"] = new[]
        {
            new HeadlinePair("HOW THE GODDESS'S TOWER KEPT DRY",
                "Clever brick gutters carried the rare but fierce desert rains safely out of Her sanctuary.",
                "From our society editor"),
            new HeadlinePair("FIRED-BRICK DRAINAGE SHAFTS ON UR III FAÇADES",
                "Van Ess describes shafts emptying into an open channel that led water from the courtyards.",
                "From the Society's scientific correspondent"),
        };

        // ziggurat-trapezoid-platform (tier 4)
        table["ziggurat-trapezoid-platform"] = new HeadlinePair("TOWER BUILT ON A CROOKED BASE, ON PURPOSE",
            "Ur-Nammu's workers shaped the ziggurat's base as a slight trapezoid, though it cost extra labor.",
            "By wire from Baghdad");
        variants["ziggurat-trapezoid-platform"] = new[]
        {
            new HeadlinePair("WAS HER TOWER SHAPED LIKE THE STARS?",
                "Some scholars wonder whether the platform copies a starry picture of the gods' palace.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("TRAPEZOIDAL FOUNDATION PLATFORM DELIBERATE",
                "Van Ess reports a platform 0.8 to 1.5 meters tall, shaped with considerable extra effort.",
                "From our science correspondent"),
        };

        // lugal-sila-si-tablet (tier 5)
        table["lugal-sila-si-tablet"] = new HeadlinePair("LAPIS TABLET NAMES AN AND INANNA",
            "A king of Kish built a wall for the sky god and the Goddess, his blue stone tablet says.",
            "From our special correspondent at Warka");
        variants["lugal-sila-si-tablet"] = new[]
        {
            new HeadlinePair("A TABLET BLUE AS THE SKY",
                "Carved from precious lapis lazuli, it honors Holy Inanna and the god of heaven.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("BM 91013: URUK OR SIPPAR?",
                "CDLI gives Uruk from the registration number; the museum record gives Sippar; it was bought in 1892.",
                "From the Society's correspondent for antiquities"),
        };

        // praying-limestone-man (tier 5)
        table["praying-limestone-man"] = new HeadlinePair("PRAYING STONE MAN, PERHAPS A KING",
            "A limestone figure with hands folded at his chest was probably buried in a foundation.",
            "From our Berlin correspondent");
        variants["praying-limestone-man"] = new[]
        {
            new HeadlinePair("HIS HANDS FOLDED IN PRAYER FOREVER",
                "Long-haired and bearded, a man of Uruk stands before the gods in limestone.",
                "From our society correspondent abroad"),
            new HeadlinePair("EARLY DYNASTIC FOUNDATION FIGURE IN ORANT POSE",
                "The Getty caption dates VA 10936 to the twenty-fifth century and suggests it may portray the king.",
                "From our correspondent at the museum stores"),
        };

        // clay-wall-inlays-2600 (tier 5)
        table["clay-wall-inlays-2600"] = new HeadlinePair("ROSETTE, REED BUNDLE AND SHEEP IN CLAY",
            "Clay inlays of about 2600 BCE decorated Uruk's walls, one in the shape of Her reed bundle.",
            "By cable from Basra");
        variants["clay-wall-inlays-2600"] = new[]
        {
            new HeadlinePair("HER REED BUNDLE BLOOMS ON THE WALLS",
                "Builders of the Early Dynastic age set Holy Inanna's own sign into their walls.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("CERAMIC INLAYS DATED ABOUT 2600 BCE",
                "Berlin's rosette, reed-bundle and sheep inlays postdate the archaic reed-bundle inlay VA 14540.",
                "From the Society's correspondent for ancient languages"),
        };

        // silver-rod-ram (tier 5)
        table["silver-rod-ram"] = new HeadlinePair("STONE RAM WITH A ROD OF SILVER",
            "A resting ram of dark limestone and a lapis-spotted calf come from early Uruk.",
            "From our correspondent in the Eanna trenches");
        variants["silver-rod-ram"] = new[]
        {
            new HeadlinePair("A CALF WITH CLOVER LEAVES OF LAPIS",
                "Tiny animals of stone, silver and lapis show the fine taste of Uruk's craftsmen.",
                "From our society editor"),
            new HeadlinePair("RECUMBENT ANIMAL FIGURES, 2800 TO 2700 BCE",
                "The museum suggests the drilled calf VA 14536 served as a cylinder-seal grip.",
                "From the Society's scientific correspondent"),
        };

        // inlaid-libation-jugs (tier 5)
        table["inlaid-libation-jugs"] = new HeadlinePair("INLAID STONE JUGS FOR POURING OFFERINGS",
            "Spouted libation jugs from Uruk carry bands of colored calcite chips.",
            "From our correspondent at the dig");
        variants["inlaid-libation-jugs"] = new[]
        {
            new HeadlinePair("STARS AND CIRCLES ON A SACRED JUG",
                "Gleaming inlays made the gods' offerings beautiful as they were poured.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("COMPOSITE LIBATION VESSELS WITH CALCITE INLAY",
                "VA 11054 joins a bituminous limestone body to a talc-schist spout; Selz notes libation's prominence.",
                "From our science correspondent"),
        };

        // akkadian-winged-shrine-seal (tier 5)
        table["akkadian-winged-shrine-seal"] = new HeadlinePair("BULL CARRIES A WINGED SHRINE ON A SEAL",
            "A seal of the Akkadian age from Uruk shows a seated god, a star and a bull bearing a shrine.",
            "By wire from Baghdad");
        variants["akkadian-winged-shrine-seal"] = new[]
        {
            new HeadlinePair("A STAR ABOVE THE GOD'S HAND",
                "Even in quieter times, Uruk's seal-cutters carved the gods and their holy beasts.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("AKKADIAN STEATITE SEAL VA 12875",
                "The museum describes an enthroned deity before a recumbent bull with a winged shrine.",
                "From the Society's correspondent for antiquities"),
        };

        // city-wall-gates (tier 5)
        table["city-wall-gates"] = new HeadlinePair("TWO GATES FOUND IN GILGAMESH'S WALL",
            "A north and a south gate are known, with a half-round tower every 10 meters.",
            "From our special correspondent at Warka");
        variants["city-wall-gates"] = new[]
        {
            new HeadlinePair("A CITY SPILLS BEYOND ITS WALLS",
                "Houses and shelters spread for kilometers outside Uruk's mighty ring of towers.",
                "From our society editor"),
            new HeadlinePair("TOWER INTERVAL OF 10 METERS REPORTED",
                "The Getty volume estimates up to 140,000 people inside and outside the wall.",
                "From the Society's scientific correspondent"),
        };

        // pig-names-tablet (tier 6)
        table["pig-names-tablet"] = new HeadlinePair("FIFTY-EIGHT WORDS FOR PIG ON ONE TABLET",
            "A complete word list from the Eanna's archaic Level III names the pig 58 ways.",
            "From our Berlin correspondent");
        variants["pig-names-tablet"] = new[]
        {
            new HeadlinePair("THE FIRST SCRIBES LOVED A LIST",
                "Early writers of Uruk practiced their signs with a whole tablet of words for pig.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("ARCHAIC LEXICAL LIST VAT 16773",
                "Thirty entries on the obverse and twenty-eight on the reverse, in three ruled columns per face.",
                "From our science correspondent"),
        };

        // unug-and-inanna-tablet (tier 6)
        table["unug-and-inanna-tablet"] = new HeadlinePair("URUK AND INANNA WRITTEN SIDE BY SIDE",
            "An archaic accounting tablet sets the city's sign beside Her reed-bundle sign, counting sheep.",
            "By cable from Basra");
        variants["unug-and-inanna-tablet"] = new[]
        {
            new HeadlinePair("HER NAME BESIDE HER CITY'S, LONG AGO",
                "One of the first written records of Holy Inanna and Uruk turns up on a humble sheep tally.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("UNUG AND MUS3 PAIRED ON W 21446",
                "CDLI's transliteration shows the two signs with UDU, sheep; no translation is offered.",
                "From the Society's correspondent for antiquities"),
        };

        // archaic-professions-list (tier 6)
        table["archaic-professions-list"] = new HeadlinePair("WORLD'S OLDEST LIST OF JOBS AT URUK",
            "The archaic Professions List begins with a title later read as 'king' and names leaders of barley and ploughs.",
            "From our correspondent in the Eanna trenches");
        variants["archaic-professions-list"] = new[]
        {
            new HeadlinePair("THE FIRST WHO'S WHO OF URUK",
                "Among the very first texts, Uruk's scribes wrote down the titles of the city's great officials.",
                "From our society correspondent abroad"),
            new HeadlinePair("ARCHAIC LU A COPIED FOR A MILLENNIUM",
                "Witness VAT 01533 from Uruk anchors a list later copied at Fara, Abu Salabikh, Tell Brak and Kish.",
                "From our correspondent at the museum stores"),
        };

        // lion-hunt-stele (tier 6)
        table["lion-hunt-stele"] = new HeadlinePair("HUNTERS AND LIONS CARVED IN BASALT",
            "A stele from Holy Inanna's precinct shows two men attacking lions with a spear and arrows.",
            "From our correspondent at the dig");
        variants["lion-hunt-stele"] = new[]
        {
            new HeadlinePair("THE BOLD LION-HUNTERS OF URUK",
                "Two brave men face the king of beasts on a dark stone slab a meter tall.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("LEVEL III FIND, STYLE PERHAPS OLDER",
                "Records place the stele in archaic Level III southeast of the ziggurat; its style is dated earlier.",
                "From the Society's correspondent for ancient languages"),
        };

        // six-petal-rosette-inlay (tier 6)
        table["six-petal-rosette-inlay"] = new HeadlinePair("CLAY ROSETTE FROM A TEMPLE WALL",
            "A six-petalled rosette, complete and with a sign scratched on its side, once decorated a building.",
            "By wire from Baghdad");
        variants["six-petal-rosette-inlay"] = new[]
        {
            new HeadlinePair("A FLOWER THAT BLOOMED ON THE WALLS",
                "Uruk's builders set clay blossoms into their shining walls of colored cones.",
                "From our society editor"),
            new HeadlinePair("INCISED MARK ON ARCHITECTURAL ROSETTE",
                "The museum dates VA 14942.01 to about 3100 to 3000 BCE.",
                "From the Society's scientific correspondent"),
        };

        // preusser-seal (tier 6)
        table["preusser-seal"] = new HeadlinePair("SHEEP FEED AT HER REED BUNDLES",
            "A marble seal shows a man in a net skirt feeding sheep beside Holy Inanna's symbols.",
            "From our special correspondent at Warka");
        variants["preusser-seal"] = new[]
        {
            new HeadlinePair("A RESTING SHEEP CROWNS THE SEAL",
                "The famous Preusser Seal ends in a bronze handle shaped like a lying sheep.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("URUK VASE SHAPES CARVED ON SEAL VA 10537",
                "The museum notes the scene may abbreviate a temple entrance flanked by reed-ring bundles.",
                "From our science correspondent"),
        };

        // anu-great-platform (tier 6)
        table["anu-great-platform"] = new HeadlinePair("GIANT TERRACE BURIES THE WHITE TEMPLE",
            "A brick platform 175 by 200 meters covered the sky god's hill at the end of the fourth millennium.",
            "From our Berlin correspondent");
        variants["anu-great-platform"] = new[]
        {
            new HeadlinePair("A WHOLE HILL WRAPPED IN BRICK",
                "A great many workers were needed to raise this enormous new landmark of Uruk.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("OLD TERRACE SURVIVES BENEATH THE RESH",
                "Eichmann notes at least 10 meters of masonry lost; its function remains unclear, van Ess writes.",
                "From the Society's correspondent for antiquities"),
        };

        // bound-prisoner-figures (tier 7)
        table["bound-prisoner-figures"] = new HeadlinePair("BOUND PRISONERS CARVED IN STONE",
            "Little figures of captives with tied arms come from Late Uruk, matching scenes on seals.",
            "By cable from Basra");
        variants["bound-prisoner-figures"] = new[]
        {
            new HeadlinePair("THE SORROWFUL CAPTIVES OF URUK",
                "Kneeling and bound, these small stone men tell a darker story of the first city.",
                "From our society correspondent abroad"),
            new HeadlinePair("PRISONER FIGURINES WITH BASAL DRILL HOLES",
                "A sealing from the same seal series shows captives and a whip-bearer before the ruler.",
                "From our correspondent at the museum stores"),
        };

        // dove-bottle (tier 7)
        table["dove-bottle"] = new HeadlinePair("ALABASTER DOVE HELD PRECIOUS OIL",
            "A Late Uruk bottle only 4 centimeters high is carved as a sitting dove.",
            "From our correspondent in the Eanna trenches");
        variants["dove-bottle"] = new[]
        {
            new HeadlinePair("A DOVE TO HOLD SWEET OIL",
                "Its eyes once sparkled with inlay, and its feet are the bottle's little base.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("ZOOMORPHIC UNGUENT VESSEL WITH BITUMEN EYES",
                "The museum notes chiselled interior, incised wings and traces of eye inlays.",
                "From the Society's correspondent for ancient languages"),
        };

        // clay-counting-token (tier 7)
        table["clay-counting-token"] = new HeadlinePair("CLAY TOKEN FROM BEFORE WRITING",
            "A little clay ball from Uruk is one of the counters people used to keep accounts.",
            "From our correspondent at the dig");
        variants["clay-counting-token"] = new[]
        {
            new HeadlinePair("FROM LITTLE CLAY BALLS TO WRITTEN WORDS",
                "From humble tokens like this, the scribes of Uruk found their way to the written word.",
                "From our society editor"),
            new HeadlinePair("INCISED SPHERICAL COUNTER, 3500 TO 3300 BCE",
                "Potts places the invention of writing in the context of such token accounting.",
                "From the Society's scientific correspondent"),
        };

        // pine-roof-beams (tier 7)
        table["pine-roof-beams"] = new HeadlinePair("TEMPLE ROOFS MADE OF FOREIGN PINE",
            "Wood tests show Uruk's great halls were roofed with pine from the Levantine-Turkish mountains.",
            "By wire from Baghdad");
        variants["pine-roof-beams"] = new[]
        {
            new HeadlinePair("TREES FROM DISTANT MOUNTAINS FOR HER HALLS",
                "Long pine beams traveled far to roof the splendid buildings of the Eanna.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("DENDROLOGY TRACES ROOF BEAMS TO THE NORTHWEST",
                "Eichmann cites spans up to 11 meters and infers a reliable trading network.",
                "From our science correspondent"),
        };

        // level-iv-bathhouses (tier 7)
        table["level-iv-bathhouses"] = new HeadlinePair("WATER PIPES IN TINY LATE URUK BUILDINGS",
            "Small structures with plumbing in Level IV may have been bathhouses, Eichmann writes.",
            "From our special correspondent at Warka");
        variants["level-iv-bathhouses"] = new[]
        {
            new HeadlinePair("DID THE ELITE OF URUK TAKE BATHS?",
                "Behind high walls, the privileged few may have enjoyed running water and gardens.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("PLUMBED STRUCTURES OF UNCLEAR FUNCTION",
                "The Getty plan marks 'Baths' twice in the Level IV precinct.",
                "From the Society's correspondent for antiquities"),
        };

        // stone-cone-crown-tiles (tier 8)
        table["stone-cone-crown-tiles"] = new HeadlinePair("CLAY SHELVES HELD THE STONE MOSAIC",
            "Crown tiles pressed into poured walls supported the Stone-Cone Building's skin of cones.",
            "From our Berlin correspondent");
        variants["stone-cone-crown-tiles"] = new[]
        {
            new HeadlinePair("THE SECRET BEHIND THE STONE MOSAICS",
                "Hidden clay slabs held thousands of colored stone cones in place on the walls.",
                "From our society correspondent abroad"),
            new HeadlinePair("PERFORATED SLABS IN LIME-MORTAR PISÉ",
                "Eichmann and van Ess describe projecting crown tiles bearing black, red and white cones.",
                "From our correspondent at the museum stores"),
        };

        // limestone-temple-podium (tier 8)
        table["limestone-temple-podium"] = new HeadlinePair("STONE PODIUM BEFORE THE TEMPLE DOOR",
            "A square stand of artificial stone blocks stood at one entrance of the Limestone Temple.",
            "By cable from Basra");
        variants["limestone-temple-podium"] = new[]
        {
            new HeadlinePair("A STAGE AT HER FIRST GREAT DOOR",
                "What once stood upon this little stage is still not known.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("ARTIFICIAL ASHLARS LATER CASED IN LARGE BRICK",
                "Eichmann matches the casing bricks to those of the White Temple.",
                "From the Society's correspondent for ancient languages"),
        };

        // room-165-limestone (tier 8)
        table["room-165-limestone"] = new HeadlinePair("ROOM FOUND CRAMMED WITH LIMESTONE",
            "Blocks like those of the Limestone Temple filled a room of the Round-Pillar Hall almost to the top.",
            "From our correspondent in the Eanna trenches");
        variants["room-165-limestone"] = new[]
        {
            new HeadlinePair("A MYSTERY ROOM FULL OF STONE",
                "Was it a storeroom, a hiding place or simply fill? Even Jordan would not say.",
                "From our society editor"),
            new HeadlinePair("ROOM 165: PACKING OF REUSED LIMESTONE",
                "A later well of Level II or III brick was cut through the deposit from above.",
                "From the Society's scientific correspondent"),
        };

        // anu-giant-bricks (tier 8)
        table["anu-giant-bricks"] = new HeadlinePair("THE BIGGEST BRICKS IN ALL URUK",
            "Mud bricks half a meter long built the terraces of the sky god's hill.",
            "From our correspondent at the dig");
        variants["anu-giant-bricks"] = new[]
        {
            new HeadlinePair("BRICKS FIT FOR A GIANT'S HOUSE",
                "Each great brick helped raise the White Temple's hill high above the city.",
                "From our lady correspondent at the Berlin museum"),
            new HeadlinePair("BRICK FORMAT LINKS ANU AND EANNA",
                "Eichmann notes the same large format on the White Temple and the Limestone Temple podium.",
                "From our science correspondent"),
        };

        // ubaid-painted-figurines (tier 9)
        table["ubaid-painted-figurines"] = new HeadlinePair("PAINTED CLAY FIGURES OF URUK'S FIRST VILLAGERS",
            "Handmade figurines of the Ubaid age, one with a painted beard, are among Uruk's oldest objects.",
            "By wire from Baghdad");
        variants["ubaid-painted-figurines"] = new[]
        {
            new HeadlinePair("THE FIRST PORTRAITS OF URUK'S PEOPLE",
                "Six thousand years ago, villagers shaped little painted people from the river clay.",
                "From our correspondent among the antiquities"),
            new HeadlinePair("UBAID FIGURINES WITH MONOCHROME PAINT",
                "Berlin dates VA 11523 and VA 11496 to 5000 to 4000 BCE; the Getty caption adds VA 14626.",
                "From the Society's correspondent for antiquities"),
        };

        // anu-ubaid-building (tier 9)
        table["anu-ubaid-building"] = new HeadlinePair("THREE-PART BUILDING FOUND DEEP IN ANU HILL",
            "Jürgen Schmidt's team reached a niched Ubaid building at the bottom of the sky god's terrace.",
            "From our special correspondent at Warka");
        variants["anu-ubaid-building"] = new[]
        {
            new HeadlinePair("WHERE THE WHITE TEMPLE'S STORY BEGINS",
                "Long before any temple gleamed white, the first villagers built a special house on this hill.",
                "From our society correspondent abroad"),
            new HeadlinePair("LATE UBAID TRIPARTITE PLAN UNDER THE TERRACE",
                "Eichmann records a clay platform raised at least ten times over five or six centuries.",
                "From our correspondent at the museum stores"),
        };

        // drill-cores-2002 (tier 9)
        table["drill-cores-2002"] = new HeadlinePair("DRILLS FIND DUNE SAND UNDER URUK",
            "Thirteen cores drilled in 2002 reached sand and river mud beneath the oldest settlement.",
            "From our Berlin correspondent");
        variants["drill-cores-2002"] = new[]
        {
            new HeadlinePair("WHAT LIES BENEATH THE FIRST CITY?",
                "Long before the first reed huts, wind-blown dunes and flood mud made the ground of Uruk.",
                "Notes from the drawing rooms of Berlin"),
            new HeadlinePair("THIRTEEN CORINGS QUESTION THE OLD SEA FLOOR",
                "Floodplain sediments over dune sands; the lowest occupation lies 1.5 meters above sea level.",
                "From the Society's correspondent for ancient languages"),
        };

        // ubaid-painted-sherds (tier 9)
        table["ubaid-painted-sherds"] = new HeadlinePair("POTTERY DATES THE BOTTOM OF THE TRENCH",
            "Painted Ubaid sherds show people lived on Holy Inanna's hill from about 5000 BCE.",
            "By cable from Basra");
        variants["ubaid-painted-sherds"] = new[]
        {
            new HeadlinePair("SEVEN THOUSAND YEARS IN A BROKEN POT",
                "Painted lines on the oldest sherds tell us when Uruk's story began.",
                "From our society editor"),
            new HeadlinePair("OBED WARE REUSED IN LAYER VI MORTAR",
                "Jordan's report illustrates the painted sherds and reassembled vessels on plates 21 and 22.",
                "From the Society's scientific correspondent"),
        };
    }
}
