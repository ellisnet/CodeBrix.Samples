using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly PersonCard[] PersonCards =
    {
        // Sheet 18 Part B, entry B1 (Julius Jordan)
        new PersonCard(
            Id: "julius-jordan",
            Name: "Julius Jordan",
            Dates: "1877–1945",
            Role: "Excavation director, 1912/13 (with Conrad Preusser) and 1928/29 to 1930/31",
            CardText: "Julius Jordan was an architect who led the first German digs at Uruk. In 1912/13 he and Conrad Preusser made the first true map of the city. In 1928 he chose to dig Eanna, the house of Holy Inanna. In 1931 he left to run Iraq's antiquities office.",
            LongText: "Jordan was born in Kassel on 27 October 1877 and trained as an architect in Dresden. He wrote his doctorate in 1910 on the building parts of Assyrian monuments. He worked under Robert Koldewey at Babylon and with Walter Andrae at Ashur.\n\nAt Uruk he dug the deep trench, the Limestone Temple and the Deep Temples at the foot of Holy Inanna's ziggurat. His Third Preliminary Report (1932) asked: “When does culture begin at Uruk?”\n\nHe directed Iraq's Department of Antiquities from 1931 to 1934 and then advised the office until 1938. He died in Berlin on 7 February 1945.",
            EthicsNote: "The Getty book says that at the Baghdad embassy he was involved in the activities of the National Socialists. He was the local Nazi party (NSDAP) base leader (Stützpunktleiter) in Baghdad, and the local SA flag was consecrated in his house in 1937. His wife Fanny (née Kauders) was partly Jewish, and he was probably protecting her.",
            ArtKey: "scene-survey-1912",
            Sources: "Getty 2019 ch. 12-13 (van Ess), fig. 12.5; Jordan 1932 (UVB III) p. 23; texts/01, 02, 11; EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 1; sheet 18 entry B1"),

        // Sheet 18 Part B, entry B2 (Conrad Preusser)
        new PersonCard(
            Id: "conrad-preusser",
            Name: "Conrad Preusser",
            Dates: "1881–1964",
            Role: "Architect and co-director, 1912/13",
            CardText: "Conrad Preusser was the architect on the first dig in 1912/13. With Jordan he drew the first true map of Uruk. He also painted pictures of the glazed bricks they found, with stars and animals on them.",
            LongText: "Preusser and Jordan led the 1912/13 campaign together for the German Oriental Society. In 1913 he painted a watercolour that rebuilds the glazed bricks of the Resh Sanctuary walls, with stars, lions and lion-griffins.\n\nHe co-wrote the 1928 book on Uruk-Warka. He was not on the 1930/31 team, and the library does not say whether he came in 1928/29 or 1929/30.",
            EthicsNote: "",
            ArtKey: "scene-resh-tunnels",
            Sources: "Getty 2019 ch. 12 (van Ess), figs. 12.5, 12.6; texts/10; BUILDING_ARCHEOLOGICAL_DRAMA (Deep Temples); sheet 18 entry B2"),

        // Sheet 18 Part B, entry B3 (Arnold Nöldeke)
        new PersonCard(
            Id: "arnold-noeldeke",
            Name: "Arnold Nöldeke",
            Dates: "1875–1964",
            Role: "Excavation director, 1931/32, 1932/33 and 1934/35 to 1938/39",
            CardText: "Arnold Nöldeke took over the dig in 1931, when he was 56 years old. He ran the camp while younger men led the trenches. His letters home tell us how the team lived, worked and waited for mail.",
            LongText: "Nöldeke was born on 12 July 1875. He was an architect at Babylon under Robert Koldewey from 1902 to 1908 and wrote his doctorate on the Imam Hussein Shrine at Karbala. He worked for Hannover's state monuments office, so each Uruk season was a leave of absence.\n\nHe gave Eanna to Heinrich Lenzen and the Anu ziggurat to Ernst Heinrich, and he slowly replaced the portable barracks with brick buildings. His letters to his wife Lisbeth, his daughter Lite, Andrae and Jordan were published in 2008. Lenzen wrote of him: “He was animated by a noble goodness.” He died on 25 November 1964.",
            EthicsNote: "",
            ArtKey: "scene-letters-from-home",
            Sources: "Getty 2019 ch. 13 (van Ess); van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; texts/07; sheet 18 entry B3"),

        // Sheet 18 Part B, entry B4 (Ernst Heinrich)
        new PersonCard(
            Id: "ernst-heinrich",
            Name: "Ernst Heinrich",
            Dates: "1899–1984",
            Role: "Architect; led the Anu ziggurat dig in campaigns 3 to 6 and 8 to 10; director of the whole dig in 1933/34",
            CardText: "Ernst Heinrich was an architect who dug at the god Anu's temple tower. For one winter, 1933/34, he led the whole dig. In 1937/38 he dug a special trench to match the layers of Eanna and Anu. He gave the Mosaic Temple its name.",
            LongText: "Heinrich was born on 15 December 1899. He was Jordan's architect on the Level V buildings from 1929 to 1931. He wrote Small Finds from the Archaic Temple Levels (Kleinfunde, 1936) and the Tenth Preliminary Report (1939), where he wrote: “From now on it shall be called the Mosaic Temple.”\n\nHe was photographed at Uruk in 1937. Later he held the chair of architectural history at the Technical University of Berlin from 1952 to 1965. He died on 28 March 1984.",
            EthicsNote: "",
            ArtKey: "scene-euphrates-crossing",
            Sources: "Getty 2019 ch. 12-13 (van Ess), fig. 13.4; Limestone Temple overview; story/MISC_QUOTES.txt no. 15; sheet 18 entry B4"),

        // Sheet 18 Part B, entry B5 (Heinrich Lenzen)
        new PersonCard(
            Id: "heinrich-lenzen",
            Name: "Heinrich (Heinz) Lenzen",
            Dates: "1900–1978",
            Role: "Excavator in Eanna, 1931/32 to 1938/39; director at Uruk after the war",
            CardText: "Heinrich Lenzen led the digging inside Eanna through the 1930s. After the war he came back and led the whole dig until 1967. He links the early diggers to the later ones.",
            LongText: "Lenzen was born on 20 September 1900. From 1928 he was an assistant in Andrae's Near Eastern department in Berlin. The Getty book says he was largely responsible for digging the Eanna sanctuary from 1931 to 1939.\n\nHe led the dig again from 1953/54 (one chapter of the Getty book says from 1954) to 1967, and in 1955 he became the first director of the Baghdad branch of the German Archaeological Institute. In 1975 he published the only study of Eanna's Level III buildings. He died on 19 January 1978.",
            EthicsNote: "",
            ArtKey: "scene-lenzen-returns",
            Sources: "Getty 2019 ch. 12-13 (van Ess), fig. 13.3; texts/10; BUILDING_ARCHEOLOGICAL_DRAMA (Red Temple); sheet 18 entry B5 and Open Question 17"),

        // Sheet 18 Part B, entry B6 (Adam Falkenstein); Open Question 14
        new PersonCard(
            Id: "adam-falkenstein",
            Name: "Adam Falkenstein",
            Dates: "1906–1966",
            Role: "Epigrapher, the reader of the clay tablets; the library's lists place him at Uruk from 1928",
            CardText: "Adam Falkenstein read the cuneiform writing on the clay tablets found at Uruk. He became a leading expert on the Sumerian language. He also joined the Nazi party and worked for Nazi Germany during the war.",
            LongText: "Falkenstein was born in Planegg on 17 September 1906. His work was on the archaic Uruk texts and on Sumerian grammar, and he wrote Topography of Uruk I: Uruk in the Seleucid Period. He held the chair at Heidelberg from 1940.\n\nThe library's lists say he was at Uruk from 1928 onward, but no primary source in the library names a season he was there. He died in Heidelberg on 15 October 1966.",
            EthicsNote: "He was a member of the Nazi party (NSDAP) from 1939 (some sources say 1940). In 1941 he flew to Baghdad with the German envoy Fritz Grobba during the pro-Axis Rashid Ali coup, then worked for the German Foreign Service in Turkey for the rest of the war. No source says he took part in the Farhud pogrom.",
            ArtKey: "scene-christmas-in-the-dig-house",
            Sources: "EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 6; story/13 catalog; texts/10; README.txt; sheet 18 entry B6 and Open Question 14"),

        // Sheet 18 Part B, entry B7 (Arndt von Haller)
        new PersonCard(
            Id: "arndt-von-haller",
            Name: "Arndt von Haller",
            Dates: "1906–about 1989 (not confirmed)",
            Role: "Site architect, documented in 1930/31 and 1931/32",
            CardText: "Arndt von Haller was an architect on the dig. He drew the plans of the two small temples at the foot of Holy Inanna's tower. Scholars still use his drawings today.",
            LongText: "Von Haller drew the plans and sections of the Deep Temples on plates 14 and 15 of Jordan's 1932 report; the essential first drawings of these buildings are his. He co-wrote the Fourth Preliminary Report (1932), and the library's catalog adds that he worked on the pottery.\n\nThe ethics report says he was site architect from 1929 onward. He later published the graves of Ashur (1954).",
            EthicsNote: "",
            ArtKey: "scene-deep-trench",
            Sources: "BUILDING_ARCHEOLOGICAL_DRAMA (Deep Temples, Drama 2); EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 8; texts/10; story/13 catalog; sheet 18 entry B7"),

        // Sheet 18 Part B, entry B8 (Erich Schott); Open Question 13
        new PersonCard(
            Id: "erich-schott",
            Name: "Erich Schott",
            Dates: "dates not given in the library",
            Role: "Junior co-author of the Fifth Preliminary Report (1934), on the 1932/33 season",
            CardText: "A scholar named Erich Schott worked on the team in the 1930s. He helped write one of the dig reports. Almost nothing else is known about him.",
            LongText: "The library's catalog and cast list name Erich Schott as junior co-author of the Fifth Preliminary Report (1934). The ethics report could find no biography of him. Whether he was a separate person, or the Assyriologist Albert Schott under a wrong first name, cannot be settled from the library.",
            EthicsNote: "",
            ArtKey: "scene-nairn-bus",
            Sources: "EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 9; story/13 catalog; texts/10; sheet 18 entry B8 and Open Question 13"),

        // Sheet 18 Part B, entry B8 (Albert Schott)
        new PersonCard(
            Id: "albert-schott",
            Name: "Albert Schott",
            Dates: "1901–1945 (missing since 8 May 1945)",
            Role: "Assyriologist at Uruk in 1928/29 and 1938/39",
            CardText: "Albert Schott was an expert in old languages who came to Uruk in 1928/29 and 1938/39. His study of an old Akkadian word helped Jordan understand the small temples at the foot of the ziggurat. He translated the Epic of Gilgamesh, the great story of Uruk's king, into German.",
            LongText: "Schott was born in Reval (Tallinn) on 23 January 1901 and has been missing since 8 May 1945, at Tábor. Jordan reported that Schott took up Andrae's idea that the deity lived in the summit temple and appeared to people in the deep temple, in a paper on the Akkadian word sahuru.\n\nHis German translation of the Epic of Gilgamesh (Reclam, 1934) stayed in print a long time. A contributor “A. S.” listed for 1928/29 is most likely he; one library file wrongly expands it as “Adam Schott.”",
            EthicsNote: "",
            ArtKey: "scene-fords-on-the-desert",
            Sources: "EXCAVATORS_NAZI_ERA_ETHICS_REPORT item 10; Jordan 1932 (UVB III) p. 24; BUILDING_ARCHEOLOGICAL_DRAMA (Deep Temples, Drama 3); Eichmann 1989 p. 36; sheet 18 entries A3, B8"),

        // Sheet 18 Part B, entry B9 (Walter Andrae)
        new PersonCard(
            Id: "walter-andrae",
            Name: "Walter Andrae",
            Dates: "1875–1956",
            Role: "Director of the Near Eastern Museum (Vorderasiatisches Museum), Berlin; organised the dig from Berlin",
            CardText: "Walter Andrae ran the Near East museum in Berlin. He found the money and permits that kept the Uruk dig going through the 1930s. He saw the ruins only twice, and very briefly.",
            LongText: "Andrae visited Uruk in 1902. He handled the applications and organisation in Berlin, asked Nöldeke to take over in 1931, and asked for the Eshgal dig in 1932/33. The Getty book says the excavations of the 1930s were accomplished only thanks to his great commitment.\n\nIn his 1930 book The House of God (Das Gotteshaus) he argued that cone mosaic copies hanging reed mats. He had casts made of the Warka Vase, the Priest-King statuette and the Lady of Warka. He brought his friend Jordan to Ashur in 1904.",
            EthicsNote: "",
            ArtKey: "scene-currency-permit",
            Sources: "Getty 2019 ch. 2, 12 (van Ess); texts/01, 11; BUILDING_ARCHEOLOGICAL_DRAMA (Round-Pillar Hall); sheet 18 entry B9"),

        // Sheet 18 Part B, entry B10 (Robert Koldewey)
        new PersonCard(
            Id: "robert-koldewey",
            Name: "Robert Koldewey",
            Dates: "dates not given in the library",
            Role: "Excavator of Babylon; visited Uruk in 1898",
            CardText: "Robert Koldewey dug the great city of Babylon. In 1898 he visited Uruk and saw that it was worth digging. Both Jordan and Nöldeke learned their work under him.",
            LongText: "Koldewey made a brief visit to Uruk with Eduard Sachau in 1898. Jordan worked under him at Babylon from 1903, and Nöldeke was his architect there from 1902 to 1908. His Babylon dig brought the Ishtar Gate to the Berlin museum.",
            EthicsNote: "",
            ArtKey: "scene-ottoman-permit",
            Sources: "Getty 2019 ch. 12-13 (van Ess); texts/01, 10, 11; BUILDING_ARCHEOLOGICAL_DRAMA (Karaindash); sheet 18 entry B10"),

        // Sheet 18 Part B, entry B11 (Eduard Meyer); Open Question 21
        new PersonCard(
            Id: "eduard-meyer",
            Name: "Eduard Meyer",
            Dates: "dates not given in the library",
            Role: "Historian of antiquity who pushed for the dig",
            CardText: "Eduard Meyer was a historian who wanted to know how cities first began. He pushed for a dig at Uruk and urged the diggers to go down to the oldest layers. His push helped lead to the very deep trench in Eanna.",
            LongText: "In the foreword to the third edition of his History of Antiquity (Geschichte des Altertums), Meyer urged a concentrated effort on the earliest periods. The Getty book says Jordan's rush to reach the earliest levels in the deep trench was probably a response.\n\nThe heads of the German Oriental Society told the excavators to look into the early levels. One library file says Jordan dug the Resh Sanctuary in 1912 against Meyer's explicit instruction; the Getty book does not say so.",
            EthicsNote: "",
            ArtKey: "scene-eduard-meyer-instruction",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 12.8; story/13 catalog; sheet 18 entry B11 and Open Question 21"),

        // Sheet 18 Part B, entry B12 (Gertrude Bell); Open Question 15
        new PersonCard(
            Id: "gertrude-bell",
            Name: "Gertrude Bell",
            Dates: "died 1926",
            Role: "Leading member of the British mandate government in Iraq; wrote the 1922 antiquities law",
            CardText: "Gertrude Bell was a British traveler and official in the new country of Iraq. In 1922 she wrote Iraq's first law to protect its ancient sites. Her law and her goodwill toward the German scholars opened the way for the dig to start again in 1928, two years after her death.",
            LongText: "In 1922 Bell drafted the antiquities law and became acting director of Iraq's own antiquities office. Before the war she had traveled widely in the Near East and had come to value the German scholars working there.\n\nThe Getty book credits her negotiations, above all with Walter Andrae, for the restart at Uruk. She died in 1926, so those talks came before the 1928 season.",
            EthicsNote: "",
            ArtKey: "scene-gertrude-bell-antiquities-law",
            Sources: "Getty 2019 ch. 12 (van Ess); sheet 18 entries A2, B12 and Open Question 15"),

        // Sheet 18 Part B, entry B13 (Sati' al-Husri); Open Question 6
        new PersonCard(
            Id: "sati-al-husri",
            Name: "Sati' al-Husri",
            Dates: "1882–1968",
            Role: "Iraqi thinker and official; a policy figure, not an excavator",
            CardText: "Sati' al-Husri was an Iraqi thinker and leader. He worked to make sure more of Iraq's ancient treasures stayed in Iraq. A 1936 law that he promoted changed how finds were shared with foreign diggers.",
            LongText: "The sources disagree about his post. The Getty book says Jordan advised his successor, Sati al-Husri, until 1938. The library's report on Iraqi scholars says al-Husri was Director-General of Education from 1921 to 1927 and was not head of antiquities after Jordan. Both agree he shaped policy and did no fieldwork.\n\nThe 1936 Iraqi antiquities law changed the division of finds and ended the generous export terms of the Jordan years.",
            EthicsNote: "",
            ArtKey: "scene-finds-division",
            Sources: "Getty 2019 ch. 13 (van Ess); IRAQI_SCHOLARS_URUK_REPORT items C.7 and 7; sheet 18 entry B13 and Open Question 6"),

        // Sheet 18 Part B, entry B14 (Henri Frankfort)
        new PersonCard(
            Id: "henri-frankfort",
            Name: "Henri Frankfort",
            Dates: "dates not given in the library",
            Role: "Dutch scholar who spoke up for the dig after the war",
            CardText: "Henri Frankfort was a Dutch expert on the ancient Near East who worked in Britain. After the Second World War he spoke up for the Uruk dig. Thanks largely to him, digging began again in 1953/54.",
            LongText: "The Getty book says it was thanks in large part to the advocacy of the Dutch scholar Henri Frankfort, then working in Great Britain, that the excavations could start again in 1953/54. He also wrote on the art and architecture of the ancient Orient.",
            EthicsNote: "",
            ArtKey: "scene-war-closes-the-dig",
            Sources: "Getty 2019 ch. 12 (van Ess); Karaindash overview bibliography; sheet 18 entry B14"),

        // Sheet 18 Part B, entry B15 (the foremen and workers from Babylon and Hilla, Ismael and son, the Libn-Jungs)
        new PersonCard(
            Id: "foremen-of-babylon-and-hilla",
            Name: "The foremen and workers from Babylon and Hilla",
            Dates: "1912/13 to 1938/39",
            Role: "Foremen and skilled workers who ran the digging and the camp",
            CardText: "Skilled foremen and workers came from Babylon and Hilla to run the digging. They lived in huts beside the dig house. The reports almost never wrote down their names. One room on a 1931/32 plan of the house is marked for “Ismael and son.”",
            LongText: "In 1912/13 the logistics ran under the reliable leadership of foremen and workers from Babylon and Hilla. They hired the local bedouin and arranged the talks with the two sheikhs.\n\nA sketch of the dig house from 1931/32 marks the huts of the workers from Babylon (Hütten der Arbeiter aus Babylon) and a room for Ismael and son (Ismael u. Sohn), with rooms for servants, the chauffeur and the guard. Ismael is one of the very few Iraqi team members named anywhere in the library; his job is not stated.\n\nNöldeke wrote of the young Iraqi mudbrick lads (Libn-Jungs), who found mud walls by feel with a long awl, in January 1932. The library's report on Iraqi scholars says the German reports almost never name their Iraqi inspectors, foremen or workmen.",
            EthicsNote: "",
            ArtKey: "scene-mudbrick-lads-awl",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 66.1; van Ess and Weber-Nöldeke 2008 (Briefe), via texts/11; IRAQI_SCHOLARS_URUK_REPORT secs. E.4, F; sheet 18 entry B15"),

        // Sheet 18 Part B, entry B16 (the Tobe and Istshey bedouin)
        new PersonCard(
            Id: "tobe-and-istshey",
            Name: "The Tobe and Istshey tribes",
            Dates: "1912/13",
            Role: "Bedouin diggers of the first campaign",
            CardText: "The diggers of 1912/13 were bedouin of the Tobe and Istshey tribes who lived around Uruk. They built reed huts for the camp. Their leaders, two sheikhs, made the work agreements.",
            LongText: "The Tobe and Istshey were settled around Uruk when the foremen from Babylon and Hilla hired them in 1912/13. The Getty book's picture list spells the second name Ishtey.\n\nA photograph of 1912 shows a reed hut that local workers built for the camp. After the Sassanian period the region was only thinly settled and served in wide areas as pasture for nomadic herders.",
            EthicsNote: "",
            ArtKey: "scene-tribal-sheikhs",
            Sources: "Getty 2019 ch. 12 (van Ess), fig. 15.3; Getty MANIFEST; sheet 18 entry B16"),

        // Sheet 18 Part B, entry B17 (a) (Behnam Abu al-Soof)
        new PersonCard(
            Id: "behnam-abu-al-soof",
            Name: "Behnam Abu al-Soof",
            Dates: "1931–2012",
            Role: "Iraqi archaeologist; studied Uruk-period pottery across Iraq",
            CardText: "Behnam Abu al-Soof was an Iraqi archaeologist. He studied the pottery of the Uruk period from all over Iraq. His book on it was published in Baghdad in 1985.",
            LongText: "Abu al-Soof earned his doctorate at Cambridge in 1966 with A Study of Uruk Pottery, its origins and distribution. His book Uruk Pottery: Origin and Distribution appeared in Baghdad in 1985, and he was long an official of Iraq's antiquities office. His work was on Uruk-period pottery across Iraq, not on digging at Warka.",
            EthicsNote: "",
            ArtKey: "scene-photography-darkroom",
            Sources: "IRAQI_SCHOLARS_URUK_REPORT sec. A.1; sheet 18 entry B17 (a)"),

        // Sheet 18 Part B, entry B17 (b) (the 2016-2018 SBAH field team)
        new PersonCard(
            Id: "sbah-field-team-2016",
            Name: "The Iraqi heritage field team of 2016 to 2018",
            Dates: "2016–2018",
            Role: "Representatives of Iraq's State Board of Antiquities and Heritage (SBAH) on the joint project with the German Archaeological Institute",
            CardText: "From 2016 to 2018 Iraqi and German scholars worked at Uruk together. Five Iraqi heritage officers joined the field team. Their report gives only their initials, not their full names.",
            LongText: "The published report names them as S. A. al-Ahmar, A. S. O. Albutaif, H. M. Wasmi, E. Q. Alagoobee and Y. A. al-Harmooshee. The library could find no named Iraqi inspector at Warka for the years 1928 to 1939.",
            EthicsNote: "",
            ArtKey: "scene-dig-house-courtyard",
            Sources: "IRAQI_SCHOLARS_URUK_REPORT secs. A.2, E.1; sheet 18 entry B17 (b)"),
    };
}
