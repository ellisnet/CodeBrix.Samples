using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly SeasonCard[] SeasonCards =
    {
        // Sheet 18 Part A, entry A1 (season 1912/13); Part C, entries C4 and C7
        new SeasonCard(
            Index: 0,
            Year: "1912/13",
            Title: "The Ottoman Permit",
            Director: "Julius Jordan and Conrad Preusser",
            Story: "The first dig at Uruk ran from November 1912 to May 1913, under a permit from the Ottoman Empire. Julius Jordan and Conrad Preusser made the first true map of the city. Foremen from Babylon and Hilla hired local bedouin to dig. They tunneled into a huge late temple that looters had been robbing.",
            LongStory: "Work began on 13 November 1912 and lasted until 16 May 1913. The German Oriental Society (Deutsche Orient-Gesellschaft) sent and paid for the team. Jordan came from the dig at Ashur by way of Babylon. The Germans lived in tents, and the workmen built reed huts.\n\nThe team had four tasks: a map of the whole city, the Resh Sanctuary (Bit Resch), a first look at the high mound of Holy Inanna's ziggurat, and a small dig in the west where looters' pits showed a palace of King Sin-kashid. The Resh was too big to clear, so the diggers worked in narrow soundings and tunnels where they expected walls to meet. Its glazed bricks showed stars, lions and lion-griffins.\n\nThe team also laid a small railway, the lorry track, to carry away earth. Its line is still a feature of the dig area today.",
            Effect: SeasonEffect.FreeSurvey,
            EffectText: "Every team surveys once, free, at the start of its first turn.",
            ArtKey: "scene-survey-1912",
            Sources: "Getty 2019 ch. 12 (van Ess) and figs. 12.1, 12.4, 12.5, 12.6, 15.3; texts/07; sheet 18 entries A1, C4, C7"),

        // Sheet 18 Part A, entries A2 (the war and the 1922 law) and A3 (season 1928/29); Open Question 15
        new SeasonCard(
            Index: 1,
            Year: "1928/29",
            Title: "Back to Uruk",
            Director: "Julius Jordan",
            Story: "Fifteen years passed before the Germans came back. The First World War stopped the work, and a new country, Iraq, was formed. Gertrude Bell, who died in 1926, wrote Iraq's antiquities law in 1922, and her law and her goodwill opened the way back. In the winter of 1928/29 Julius Jordan returned and chose to dig Eanna, the house of Holy Inanna.",
            LongStory: "After the war the Ottoman provinces of Basra, Baghdad and Mosul became Iraq, under a British mandate. In 1922 Gertrude Bell drafted the antiquities law and became acting director of Iraq's own antiquities office. The Getty book credits her talks, above all with Walter Andrae, for the restart; she had died in 1926, so those talks came before the dig began again.\n\nThat first winter the team found the Temple of Karaindash (Innin-Tempel des Karaindasch), about 1420 BCE. Its wall of molded baked bricks shows gods and goddesses, each holding a jar from which water flows. Under the division of finds (Fundteilung), part went to Berlin and part to the Iraq Museum.\n\nThe Emergency Association of German Science (Notgemeinschaft der Deutschen Wissenschaft) paid, and Walter Andrae did the paperwork in Berlin. The dig house was begun as portable barracks. Two Ford cars, rebuilt for driving on the trackless land, carried the team.",
            Effect: SeasonEffect.GainWorker,
            EffectText: "Every team gains one Worker.",
            ArtKey: "scene-fords-on-the-desert",
            Sources: "Getty 2019 ch. 2, ch. 12 (van Ess), figs. 12.3, 66.1; texts/01, 02, 07; Eichmann 1989 p. 36; sheet 18 entries A2, A3 and Open Question 15"),

        // Sheet 18 Part A, entry A4 (season 1929/30)
        new SeasonCard(
            Index: 2,
            Year: "1929/30",
            Title: "Christmas in the Dig House",
            Director: "Julius Jordan",
            Story: "In 1929/30 Jordan's deep trench reached a temple the diggers called the Red Temple. They also saw the first stone blocks of the much older Limestone Temple. The team spent Christmas in a half-built dig house, with a little Christmas tree on the tea table.",
            LongStory: "Ernst Heinrich worked as Jordan's architect. The Red Temple (Roter Tempel) belongs to Level III, the Jemdet Nasr period, about 3300 to 3000 BCE. Even today, Ricardo Eichmann wrote, no plan of it can be described.\n\nThe limestone footings of the Limestone Temple (Kalksteintempel), Level V, were first partly recognized this season, and the Round-Pillar Hall began to come up. Jordan published his Second Preliminary Report in 1930.\n\nA photograph from that winter shows the brick house with ready-made sheds and reed huts for the German team and their Iraqi coworkers from far away. Another shows Christmas in the barely built headquarters: four German excavators in suits at a tea table, with a Christmas tree.",
            Effect: SeasonEffect.KassiteCheaper,
            EffectText: "Kassite and Old Babylonian sites (tier 3) cost one less this season.",
            ArtKey: "scene-christmas-in-the-dig-house",
            Sources: "Getty 2019 figs. 66.2, 66.3; Red Temple, Limestone Temple and Round-Pillar Hall overviews; BUILDING_ARCHEOLOGICAL_DRAMA (Red Temple); sheet 18 entry A4"),

        // Sheet 18 Part A, entry A5 (season 1930/31)
        new SeasonCard(
            Index: 3,
            Year: "1930/31",
            Title: "The Deep Trench",
            Director: "Julius Jordan",
            Story: "1930/31 was Jordan's busiest and last winter as leader. His team uncovered Holy Inanna's great stone-footed temple and a hall of round pillars covered in colored cones. They peeled the late mud mantle off Her ziggurat and found two small temples at the foot of its stairs. In 1931 Jordan left to run Iraq's antiquities office.",
            LongStory: "The budget was 80,000 Reichsmark, and then it fell sharply. Ernst Heinrich and Arndt von Haller were architects, and Rudolf Michaelis came from the Berlin museum to lift the cone mosaics. He sent them to Berlin; the Loftus Façade was left in place. Heinrich wrote: “Once the mosaics had been excavated, we had the duty to protect them from further decay.”\n\nUnder the mantle of the Ur-Nammu ziggurat lay the two Deep Temples (Tieftempel). They are Neo-Babylonian to early Achaemenid, not Late Uruk. Under the East Temple's floor lay an unbaked clay lion, 17 cm long, find number 10008. The deep trench was enlarged to 29 by 21 metres.\n\nAt the Anu ziggurat the White Temple appeared as white lines, “as if drawn in chalk upon the ground.” Jordan's Third Preliminary Report came out in 1932. When he left in 1931, Walter Andrae asked Arnold Nöldeke, then 56, to take over.",
            Effect: SeasonEffect.DeepCheaper,
            EffectText: "Deep sounding sites (tier 9) cost one less this season.",
            ArtKey: "scene-deep-trench",
            Sources: "Jordan 1932 (UVB III) pp. 15, 20; Getty 2019 ch. 12-13 (van Ess); BUILDING_ARCHEOLOGICAL_DRAMA (Deep Temples, Round-Pillar Hall); story/16 Correction #1; texts/11; sheet 18 entry A5"),

        // Sheet 18 Part A, entry A6 (season 1931/32); Part C, entry C7
        new SeasonCard(
            Index: 4,
            Year: "1931/32",
            Title: "The Mudbrick Lads",
            Director: "Arnold Nöldeke",
            Story: "In 1931/32 Arnold Nöldeke took charge of the dig. The old mud walls had melted back into mud. Young Iraqi workers found them by feel, pushing a long pointed tool into the ground. Nöldeke wrote home about them in January 1932.",
            LongStory: "Nöldeke ran the camp and did smaller digs himself. He gave the big Eanna area to Heinrich Lenzen and the Anu ziggurat to Ernst Heinrich. Lenzen stayed until 1939.\n\nNöldeke called the young workers the mudbrick lads (Libn-Jungs). With a long awl (eine lange Ahle) they felt the difference between set brick and the looser mortar between bricks. Then they scraped out the mortar by hand so the bricks showed. His letters of 2 and 9 January 1932 tell of them.\n\nThe deep trench went down to groundwater: 85 square metres, more than 19.6 metres deep, sixteen building levels over about 800 years. The earliest settlers built on thick layers of reeds. The Niched Building (Nischengebäude) was first described in the Fourth Preliminary Report, 1932.",
            Effect: SeasonEffect.RerollOnce,
            EffectText: "Every team may re-roll one die once this season.",
            ArtKey: "scene-mudbrick-lads-awl",
            Sources: "Getty 2019 ch. 12, ch. 13 (van Ess), figs. 12.7, 60.4, 66.1; van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; Niched Building overview; sheet 18 entries A6, C7"),

        // Sheet 18 Part A, entry A7 (season 1932/33); Part C, entry C3
        new SeasonCard(
            Index: 5,
            Year: "1932/33",
            Title: "Andrae's Wish",
            Director: "Arnold Nöldeke",
            Story: "In 1932/33 the museum director Walter Andrae asked the team to dig a second temple of the love Goddess, south of Eanna. It was the Eshgal, built of baked brick in the Seleucid age. That year Iraq became a fully independent country. Soon the team began reaching Baghdad by ship and desert bus.",
            LongStory: "At Andrae's express wish, work turned to the Eshgal, a large Seleucid temple of fired brick for Ishtar and the deities close to Her. Lenzen worked in Eanna and Heinrich at the Anu ziggurat. Sections and plans of Temple D appeared in the Fifth Preliminary Report (1934).\n\nIraq became officially independent in 1932, though British advisors stayed until 1958. In a letter of 12 February 1933, Nöldeke wrote about Jordan's sympathy for the Nazi party.\n\nFrom 1933 the team no longer took the Orient Express. They went by sea to Haifa or Beirut, then by the Nairn desert bus to Baghdad, then by train and a small boat across the Euphrates.",
            Effect: SeasonEffect.StudyDrawsTwo,
            EffectText: "Study draws two Tablets this season.",
            ArtKey: "scene-nairn-bus",
            Sources: "Getty 2019 ch. 12, 13 (van Ess); van Ess and Weber-Nöldeke 2008 (Briefe), via texts/10, 11; Temple D overview; sheet 18 entries A7, C3"),

        // Sheet 18 Part A, entry A8 (season 1933/34); Open Questions 9 and 12
        new SeasonCard(
            Index: 6,
            Year: "1933/34",
            Title: "Heinrich's Winter",
            Director: "Ernst Heinrich",
            Story: "In the winter of 1933/34 Ernst Heinrich led the whole dig, the one winter between 1931 and 1939 that Nöldeke did not lead. The records say the team found a tall stone vase that season, in fifteen broken pieces in a buried hoard. Its carvings show people bringing gifts to the gate of Holy Inanna's house. Today it is called the Warka Vase, and it is in the Iraq Museum in Baghdad.",
            LongStory: "The Warka Vase is alabaster and about 105 cm tall. It is Iraq Museum IM 19606, find number W 14873, with a plaster cast in Berlin. The Getty book dates it to the Late Uruk period, the second half of the fourth millennium BCE. The season, the fifteen pieces and the hoard (Sammelfund) come only from a web-sourced image note; the Getty book confirms the object but not the season, and the library's files disagree about where in Eanna the hoard lay.\n\nA photograph shows the team's tea corner that winter. It is still used the same way today. Annemarie Schwarzenbach photographed the dig in about 1933 and 1934.\n\nIn 1934, after the Nazis took power, money grew harder to get because of limits on spending abroad. A Buick joined the two Fords; the library gives both 1933 and November 1934 for its arrival.",
            Effect: SeasonEffect.RecruitCheaper,
            EffectText: "Recruit costs one less this season.",
            ArtKey: "scene-dig-house-courtyard",
            Sources: "Getty 2019 ch. 12, ch. 13 (van Ess), figs. 9.1, 9.9, 66.4; images/Holy_Inanna/IMAGES.md; IMAGES_MANIFEST.txt; texts/11; sheet 18 entry A8 and Open Questions 9, 12"),

        // Sheet 18 Part A, entry A9 (season 1934/35); Part C, entry C1
        new SeasonCard(
            Index: 7,
            Year: "1934/35",
            Title: "Money Grows Short",
            Director: "Arnold Nöldeke",
            Story: "In 1934/35 Nöldeke was back in charge. A surveyor laid out a grid of fixed points over Eanna, and every later map used it. The German government was now limiting how much money could leave the country, and the dig felt it.",
            LongStory: "Temple C, of Level IVa in the Late Uruk period, was first exposed this season, with Ernst Heinrich and Heinrich Lenzen as architect-excavators; its plan already appears in a report that may cover the winter before, so the season is not certain. W. Goepner surveyed the grid points of Eanna, the reference grid for all later Eanna plans.\n\nNöldeke's letters date the season: 12 November 1934, the Buick arrives; 18 November, the workday hours; 25 November, currency controls begin to bite; 17 March 1935, political reports to the German envoy Fritz Grobba.\n\nThe workday ran from 6:30 to noon and from 1:30 to 5:00. After dinner the team wrote up the day's finds by petroleum lamp.",
            Effect: SeasonEffect.WorkersCapped,
            EffectText: "Workers cost nothing this season, but each team may spend at most two on one dig.",
            ArtKey: "scene-currency-permit",
            Sources: "Getty 2019 ch. 12-13 (van Ess); Temple C overview; Eichmann Plan 73 caption; van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; sheet 18 entries A9, C1 and Open Question 3"),

        // Sheet 18 Part A, entry A10 (season 1935/36)
        new SeasonCard(
            Index: 8,
            Year: "1935/36",
            Title: "Unrest Near Samawa",
            Director: "Arnold Nöldeke",
            Story: "In 1935/36 money for the dig was hard to find, and Walter Andrae fought for it in Berlin. Tribes near the towns of Rumaitha and Samawa rose up against the government. Nöldeke wrote home that the digging went on almost as usual.",
            LongStory: "Heinrich was again at the Anu ziggurat and Lenzen in Eanna. The library names no building first found this season; the Level IV buildings of Eanna were dug and drawn season by season. The Eighth Preliminary Report appeared in 1937.\n\nThe uprisings, from 1935 to 1937, were around Rumaitha, 50 km to the north, and Samawa, 15 km to the west. On 25 November 1935 Nöldeke wrote that Jordan was thinking of a Baghdad branch of the German Archaeological Institute, and on 9 December that Andrae was fighting for funds.",
            Effect: SeasonEffect.None,
            EffectText: "No effect this season; the story only.",
            ArtKey: "scene-letters-from-home",
            Sources: "Getty 2019 ch. 13 (van Ess); van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; Cone-Mosaic Temple overview; README.txt; sheet 18 entry A10"),

        // Sheet 18 Part A, entry A11 (season 1936/37)
        new SeasonCard(
            Index: 9,
            Year: "1936/37",
            Title: "The Money Fights",
            Director: "Arnold Nöldeke",
            Story: "In 1936/37 the team kept bumping into a huge building of pounded earth. At the same time the German science fund held back money, and the Foreign Office stepped in to pay. A new Iraqi law also changed how finds were shared, so fewer could leave Iraq.",
            LongStory: "The Rammed-Earth Building (Stampflehmgebäude), of Level III, was first cleared between 1936 and 1938, with Lenzen in the field. Lenzen later wrote that in the 1930s “we kept running into the Stampflehmgebäude again and again.”\n\nThe letters tell the money story: on 15 December 1936 no more currency permits were expected; on 17 February 1937 the last 7,000 Reichsmark were not released; on 4 March 1937 the German Research Foundation (DFG) cut its money and the Foreign Office (Auswärtiges Amt) paid instead.\n\nSati' al-Husri promoted the 1936 Iraqi antiquities law, which changed the division of finds and ended the generous export terms of the Jordan years.",
            Effect: SeasonEffect.PublishBonus,
            EffectText: "Every report published this season scores one more.",
            ArtKey: "scene-finds-division",
            Sources: "Getty 2019 ch. 13 (van Ess); Lenzen, UVB XX (1964) p. 16; Rammed-Earth Building overview; van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; IRAQI_SCHOLARS_URUK_REPORT sec. C.7; sheet 18 entry A11"),

        // Sheet 18 Part A, entry A12 (season 1937/38); Part C, entry C3
        new SeasonCard(
            Index: 10,
            Year: "1937/38",
            Title: "The Dating Trench",
            Director: "Arnold Nöldeke",
            Story: "In 1937/38 Ernst Heinrich dug a special trench to match the layers of Holy Inanna's precinct with the layers at the god Anu's temple. The puzzle was too hard, and it is still not solved today. That December, crossing the flooded Euphrates River took seven hours.",
            LongStory: "Linking the Uruk-period levels of Eanna with those of the Anu ziggurat had interested Jordan in 1930/31. The Getty book says that in the winter of 1937/38 Heinrich undertook a special dating trench, in which, as in later attempts, no certain answer was found.\n\nIn his Tenth Preliminary Report (1939) Heinrich named a Level IV building: “From now on it shall be called the Mosaic Temple.” The report does not say in which season it was found.\n\nThe letters of that winter: on 9 November 1937 the local SA flag was consecrated at Jordan's house in Baghdad; on 15 November air travel became possible; on 22 December the flooded Euphrates took seven hours to cross.",
            Effect: SeasonEffect.FreeSurvey,
            EffectText: "Survey is free for everyone: every team surveys once, free, at the start of its first turn.",
            ArtKey: "scene-euphrates-crossing",
            Sources: "Getty 2019 ch. 12-13 (van Ess); Heinrich, UVB X (1939); story/MISC_QUOTES.txt nos. 15-16; van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; sheet 18 entries A12, C3"),

        // Sheet 18 Part A, entry A13 (season 1938/39); Open Question 11
        new SeasonCard(
            Index: 11,
            Year: "1938/39",
            Title: "The Last Permit",
            Director: "Arnold Nöldeke",
            Story: "1938/39 was the last winter before the Second World War. The last permit to take money out of Germany for the dig had to be signed by Hermann Göring himself. On 22 February 1939 the team found a carved stone face of a woman, the Mask of Warka, which many scholars think shows Holy Inanna. Then the war came, and the dig stopped for fourteen years.",
            LongStory: "The Mask of Warka, also called the Lady of Warka, is a life-size woman's head about 21 cm tall. It is Iraq Museum IM 45434, find number W 17878, with a plaster cast in Berlin. The Getty book calls it marble and dates it to the Late Uruk period; an image note calls it alabaster, found in Layer III. The find date comes only from that note. Nöldeke published it in the Eleventh Preliminary Report (1940).\n\nIn the Narrow-Brick Building (Riemchengebäude) the team found a sealed deposit of stone vessels, copper, lapis lazuli, shell and bone inlays, seal impressions and clay cones. Work stopped in September 1939, and the building sat half-dug for fourteen years. The Assyriologist Albert Schott took part this season.\n\nResearch stalled for the whole of the Second World War and the early years after it.",
            Effect: SeasonEffect.FinalSeason,
            EffectText: "The last season. Publish what you can; whatever stays in the crates when the war closes the dig scores half.",
            ArtKey: "scene-mask-emerging",
            Sources: "Getty 2019 ch. 11, 12, 13, 14, fig. 11.1; images/Holy_Inanna/IMAGES.md; Nöldeke, UVB XI (1940) Taf. 21; BUILDING_ARCHEOLOGICAL_DRAMA (Narrow-Brick Building); van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; sheet 18 entry A13 and Open Question 11"),
    };
}
