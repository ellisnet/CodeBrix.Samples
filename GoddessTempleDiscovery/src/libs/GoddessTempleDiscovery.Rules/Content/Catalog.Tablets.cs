using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly TabletCard[] TabletCards =
    {
        // ---------------------------------------------------------------------------------------------------------
        // GODDESS (15)
        // ---------------------------------------------------------------------------------------------------------

        // Sheet 19 A1 (Her name, the Dingir, Nin-an-ak); sheet 23 A22 (the sign AN); sheet 21 Q65
        new TabletCard(
            Id: "goddess-her-name",
            Kind: TabletKind.Goddess,
            Title: "Her Name: Mistress of Heaven (Nin-an-ak)",
            ArtKey: "deity-inanna",
            Cuneiform: "ᵈ𒈹",
            CuneiformReading: "ᵈinana: the god sign (Dingir) and the sign MUŠ₃, Holy Inanna",
            CardText: "Holy Inanna's name was understood even in ancient times to mean “Mistress of Heaven.” Scribes wrote a small sign, the Dingir, before the name of every god, and in this game we say it as “Holy.” Later, people who spoke Akkadian called Her Ishtar.",
            LongText: "The name Inanna, in full Innana.k, was read even in antiquity as short for Nin-an-ak, “Mistress of Heaven.” An older Sumerian form, Innin, appears in Jordan's report of 1932 and in the inscription of the Kassite king Karaindash. Inanna and Ishtar are written with one and the same sign, MÙŠ.\n\nThe Dingir is the cuneiform sign AN. It can mean “sky,” it can be the name of the sky god An, and when it stands before a name it marks a god. Scribes did not read it aloud.\n\nHer name is possibly already written in the archaic texts of Uruk IV, and Gebhard Selz says its reading surely reaches back to the Late Uruk period, about 3500–3300 BCE. From the Akkadian form Ishtaru came a common word for “goddess.”",
            Quote: "The name Inanna, or in full, Innana.k, was interpreted even in antiquity as an abbreviation of Nin-an-ak, or 'Mistress of Heaven.'",
            QuoteAttribution: "Annette Zgoll, in Uruk: First City of the Ancient World (Getty 2019), ch. 9",
            Pronunciation: "Holy Inanna HOH-lee ih-NAH-nah; Innin IN-nin; Ishtar ISH-tar; Dingir DIN-geer",
            Sources: "Getty 2019 ch. 9 (Zgoll), ch. 11, ch. 37 (van Ess), ch. 39 (Selz) p. 215; Oracc Sign List AN, http://oracc.org/osl/signlist/o0000099; AMGG An/Anu, http://oracc.museum.upenn.edu/amgg/listofdeities/an/; sheet 19 A1; sheet 21 Q65, B04; sheet 23 A22"),

        // Sheet 19 A2 (the reed bundle); sheet 20 B30; sheet 23 A23 (the sign MUŠ₃); sheet 21 Q26
        new TabletCard(
            Id: "goddess-reed-bundle",
            Kind: TabletKind.Goddess,
            Title: "The Reed Bundle (Ringbündel)",
            ArtKey: "symbol-reed-bundle-gatepost",
            Cuneiform: "𒈹",
            CuneiformReading: "MUŠ₃: the reed-bundle sign, read inana, innin or ištar",
            CardText: "The oldest way to write Holy Inanna's name was a picture of a bundle of reeds tied into a loop. The same sign could be read Inanna, Innin or Ishtar. Her symbol is found on the very earliest clay tablets dug up at Uruk.",
            LongText: "The reed bundle appears on many things from Uruk: on a clay wall inlay now in Berlin (VA 14540, find number W 4999 b), on cylinder seals, on the earliest tablets, and most clearly in the top band of the Uruk Vase. Wherever it appears, scholars suspect Her presence. Julius Jordan argued in 1932 that She was worshipped in Eanna from the beginning, because Her ring-bundle (Ringbündel) turns up so often on the archaic tablets.\n\nIn 1930 Walter Andrae read the bundle as a doorpost from the reed houses of the southern marshes, with a rolled mat at the top that could be let down as a door hanging.\n\nThe Oracc Sign List gives the sign MUŠ₃ the readings inana, innin, ninni and ištar. Joined with the sign AB, it spells Zabalam, another old city of the Goddess.",
            Quote: "That She was worshipped in Eanna from the beginning, we conclude — not, I think, without warrant — from the frequent occurrence of Her symbol, the ring-bundle, on the archaic clay tablets [...]",
            QuoteAttribution: "Julius Jordan, Third Preliminary Report (Dritter vorläufiger Bericht), 1932, pp. 32–33 (working translation)",
            Pronunciation: "Inanna ih-NAH-nah; Innin IN-nin; Ishtar ISH-tar",
            Sources: "Getty 2019 ch. 11 (figs. 11.2, 11.3), ch. 39 (Selz); Jordan 1932 (UVB III) pp. 32–33; Oracc Sign List MUŠ₃, http://oracc.org/osl/signlist/o0000466, and |MUŠ₃.AB|, http://oracc.org/osl/signlist/o0002038; sheet 19 A2; sheet 20 B30; sheet 21 Q26; sheet 23 A23"),

        // Sheet 19 A3 (Venus, the numbers 15 and 30); sheet 23 A22, B17; sheet 21 Q67, Q68
        new TabletCard(
            Id: "goddess-venus",
            Kind: TabletKind.Goddess,
            Title: "The Morning and Evening Star",
            ArtKey: "deity-inanna-venus",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The people of Uruk see Holy Inanna in the planet Venus, the bright star of morning and evening. The moon god, Her father, has the number 30, and She has half of it, 15. When Venus vanishes from the sky for a while, one old story may explain where She has gone.",
            LongText: "Annette Zgoll writes that Inanna can be perceived in the planet Venus. The moon god is imagined as Her father and the sun god Utu as Her brother, a family of lights in the sky. Scribes could even write Ishtar's name with the god sign and the number 15 (ᵈ15).\n\nVenus is sometimes invisible, and this seemed menacing. One reading of the myth of Her Descent to the land of the dead sees Venus's absences as Her stays below, and its return as the Goddess's resurrection.\n\nA hymn to Ishtar “who appears as the planet Venus” was written at Uruk in the Seleucid period (332–141 BCE) and is now in the Louvre (AO 6458). A Sumerian hymn of King Iddin-Dagan praises Her as “the radiant star, the Venus star.”",
            Quote: "[...] the moon god can be symbolized with the number 30, his daughter Inanna/Ishtar with half of that, 15.",
            QuoteAttribution: "Annette Zgoll, Getty 2019 ch. 9, pp. 56–57",
            Pronunciation: "Inanna ih-NAH-nah; Utu OO-too; Nanna NAHN-nah",
            Sources: "Getty 2019 ch. 9 (Zgoll), fig. 9.10; ch. 39 (Selz); ETCSL 2.5.3.1 ll. 89–105, https://etcsl.orinst.ox.ac.uk/section2/tr2531.htm; AMGG Inana/Ištar, http://oracc.museum.upenn.edu/amgg/listofdeities/inanaitar/; sheet 19 A3; sheet 21 Q67, Q68; sheet 23 A22, B17"),

        // Sheet 19 A5 (love and fertility), B9 (Shara and Lulal), B13 (Etana)
        new TabletCard(
            Id: "goddess-love-and-new-life",
            Kind: TabletKind.Goddess,
            Title: "Love and New Life",
            ArtKey: "deity-inanna-of-love",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Holy Inanna is the Goddess of love and of new life. Ancient poems say Her power to make things grow and be born never grows tired. Strangely, She is almost never shown as a mother.",
            LongText: "The Getty book calls Her “the goddess of love and war,” and Annette Zgoll calls Her the power over sexuality and the birth of new life. In the story of Her Descent, even Her jewelry, Her wig and Her eye makeup are charged with this power.\n\nChildren are only rarely said to be Hers, such as the gods Shara and Lulal. Zgoll explains that, as the force behind fertility, She is never in a mother's state. People hoped Her power would never run out.\n\nIn the Akkadian story of Etana, the first king gets the gift of a son not from the powers of the underworld but from Inanna/Ishtar in heaven.",
            Quote: "[...] because she is always in a state of fertility.",
            QuoteAttribution: "Annette Zgoll, on why Inanna is never a mother, Getty 2019 ch. 9",
            Pronunciation: "Inanna ih-NAH-nah; Etana eh-TAH-nah",
            Sources: "Getty 2019 ch. 9 (Zgoll), 'The Goddess and Her Power'; ch. 1; ch. 37; sheet 19 A5, B9, B13"),

        // Sheet 19 A6 (war, the lion, the maces); sheet 23 A19 (Her lion); sheet 21 Q17
        new TabletCard(
            Id: "goddess-war-and-the-lion",
            Kind: TabletKind.Goddess,
            Title: "War, the Lion and the Maces",
            ArtKey: "deity-inanna-warrior",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Holy Inanna is also a Goddess of battle. Pictures show Her holding maces, with one foot on a lion's back or standing on it. Poems say She roars like a lion in heaven and on earth.",
            LongText: "In written sources Inanna/Ishtar is a warlike heroine. Her weapons are maces, and She rides on a lion. Texts call Her the one who roars, whose flashing eyes threaten destruction. A clay relief in Berlin (VA 10978, Old Babylonian) shows Her standing on Her lion.\n\nScholars disagree about how far back this side of Her goes. Annette Zgoll finds the warrior Inanna in sources of the third millennium BCE, while the Oracc god list, following Selz, says the warrior aspect does not appear before the Akkadian period. At least the Inanna of Lagash was unarmed in the Early Dynastic period.\n\nIn winter 1930/31 Jordan's team found a little crouching lion of unbaked clay, 17 cm long, in a gap in the pavement before the cult niche of the East Deep Temple. Jordan did not say whom it was for.",
            Quote: "In heaven and on earth you roar like a lion and devastate the people",
            QuoteAttribution: "Inana and Ebih, ETCSL 1.3.2, lines 7–9",
            Pronunciation: "Inanna ih-NAH-nah; Ishtar ISH-tar",
            Sources: "Getty 2019 ch. 9 (Zgoll), figs. 9.5, 11.11; ch. 11; Jordan 1932 (UVB III) p. 33; AMGG Inana/Ištar, http://oracc.museum.upenn.edu/amgg/listofdeities/inanaitar/; ETCSL 1.3.2, https://etcsl.orinst.ox.ac.uk/section1/tr132.htm; sheet 19 A6; sheet 21 Q17; sheet 23 A19"),

        // Sheet 19 A7 (the Uruk Vase); sheet 20 B15, B16
        new TabletCard(
            Id: "goddess-uruk-vase",
            Kind: TabletKind.Goddess,
            Title: "The Uruk Vase: Her Earliest Picture",
            ArtKey: "object-warka-vase-top-register",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Uruk Vase holds what may be the earliest picture of Holy Inanna. You read it from the bottom up: water, then grain, then sheep, then men carrying gifts. At the top the gifts reach the Goddess at a door marked by reed bundles.",
            LongText: "The alabaster vase was found in Uruk's Inanna Temple and belongs to the Late Uruk period, the second half of the fourth millennium BCE. Annette Zgoll calls the figure at the top the earliest depiction of Inanna; Gebhard Selz says only that the gifts go to a priestess or female deity. The library's files do not agree on exactly where in Eanna it lay.\n\nA procession led by a priest-king brings the harvest. The top band shows reed bundles, a stepped altar on the back of a ram, and a man holding a tray of cups, the oldest form of the sign EN, “lord.” The vase was once one of a pair.\n\nIt is in the Iraq Museum in Baghdad (IM 19606, find number W 14873), with a plaster cast in Berlin. It was stolen in the looting of 2003 and recovered.",
            Quote: "[...] bears the earliest depiction of the goddess Inanna.",
            QuoteAttribution: "Annette Zgoll, on the Uruk Vase, Getty 2019 ch. 9",
            Pronunciation: "Uruk OO-rook; Warka WAR-kah",
            Sources: "Getty 2019 ch. 9 (Zgoll), figs. 9.1, 9.9; ch. 11; ch. 39 (Selz); IRAQI_SCHOLARS_URUK_REPORT sec. D; sheet 19 A7 and open question 7; sheet 20 B15, B16"),

        // Sheet 19 A8 (the Lady of Warka); sheet 18 A13 (found 22 February 1939)
        new TabletCard(
            Id: "goddess-lady-of-warka",
            Kind: TabletKind.Goddess,
            Title: "The Lady of Warka",
            ArtKey: "object-mask-of-warka",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Lady of Warka is a life-size marble face more than 5,000 years old. Her eyes and eyebrows were once filled with inlays. Many scholars think She is Holy Inanna, part of a statue of the Goddess.",
            LongText: "The head has a smooth face, narrow lips and a long, damaged nose, and inlays were planned for the eyes and brows. The Getty book says Inanna is also suspected to be the subject. Gebhard Selz thinks it was in all probability part of a goddess figure made of several materials, which shows that gods were imagined in human form in the Late Uruk period.\n\nArnold Nöldeke's team found it in Eanna on 22 February 1939, in the winter 1938/39 season, and gave it the find number W 17878. It is in the Iraq Museum in Baghdad (IM 45434), with a plaster cast in Berlin (VAG 1074). It was stolen in the looting of the museum in 2003 and recovered.",
            Quote: "[...] the earliest known large-scale work of art [...]",
            QuoteAttribution: "Getty 2019 ch. 14, on the Lady of Warka",
            Pronunciation: "Warka WAR-kah; Arnold Nöldeke AR-nolt NURL-deh-keh",
            Sources: "Getty 2019 ch. 11, fig. 11.1; ch. 14; ch. 39 (Selz); IMAGES_MANIFEST; IRAQI_SCHOLARS_URUK_REPORT; sheet 18 A13; sheet 19 A8"),

        // Sheet 23 B1 (ETCSL 1.4.1, the seven gates and what is taken at each); sheet 19 A9; sheet 21 Q68
        new TabletCard(
            Id: "goddess-descent",
            Kind: TabletKind.Goddess,
            Title: "The Descent to the Great Below",
            ArtKey: "symbol-seven-gates",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "In an old Sumerian poem, Holy Inanna goes down to the land of the dead. At each of seven gates She must give up part of Her crown, jewels or clothes. The judges of the dead turn Her into a corpse, but Her helper brings Her back to life.",
            LongText: "The poem, “Inana's descent to the nether world,” begins as She leaves Her temples in heaven and on earth for the palace of Her elder sister Ereshkigal. Ereshkigal has the seven gates bolted, and Holy Inanna is let through one gate at a time. At the first She loses Her turban; at the second, the small lapis-lazuli beads; at the third, the twin egg-shaped beads; at the fourth, Her chest ornament; at the fifth, the golden ring; at the sixth, the lapis-lazuli measuring rod and line; and at the seventh, Her pala dress. One copy takes the rod and line at the first gate.\n\nThe seven judges condemn Her and She is hung on a hook. After three days and three nights Her minister Ninshubur goes for help, and the god Enki sends two little beings with the plant and the water of life. She rises, but someone must go below in Her place.\n\nAnnette Zgoll notes one reading of the poem: Venus's weeks out of the sky are Her stays below, and its return is the Goddess's resurrection.",
            Quote: "From the great heaven she set her mind on the great below.",
            QuoteAttribution: "Inana's descent to the nether world, ETCSL 1.4.1, lines 1–5",
            Pronunciation: "Inanna ih-NAH-nah; Ereškigal eh-RESH-kee-gahl; Ninšubur nin-SHOO-boor; Enki EN-kee",
            Sources: "ETCSL 1.4.1 ll. 1–5, 114–281, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; Getty 2019 ch. 9 (Zgoll); sheet 19 A9; sheet 21 Q68; sheet 23 A1, A4, A8, B1"),

        // Sheet 23 B2 and C4 (Inana and Enki, ETCSL 1.3.1; the list of the me); sheet 19 A10
        new TabletCard(
            Id: "goddess-the-me",
            Kind: TabletKind.Goddess,
            Title: "Inanna and Enki: the Divine Powers (me)",
            ArtKey: "symbol-me-tablet",
            Cuneiform: "𒈨",
            CuneiformReading: "me: a divine power",
            CardText: "The “me” are the gifts that make a city work, from kingship and wisdom to the art of song and the craft of the scribe. In an old story Holy Inanna visits the god Enki at Eridu, and after a feast of beer he gives them to Her. She carries them home to Uruk in the Boat of Heaven.",
            LongText: "In the Sumerian poem “Inana and Enki,” the two drink beer together in the Abzu at Eridu, and Enki, drunk, hands Her the powers one group after another. Among them are the office of en priest, kingship, the royal throne, going down to the underworld and coming up again, the art of song, the craft of the scribe, wisdom, the kindling of fire, and judging. When he is sober again, Enki sends his minister Isimud and his creatures seven times to seize the boat, but Her minister Ninshubur saves it each time.\n\nThe tablets of this poem are broken, with gaps of dozens of lines. In Enheduanna's hymn She is the “Lady of all the divine powers.”",
            Quote: "holy Inana had gathered up the divine powers and embarked onto the Boat of Heaven.",
            QuoteAttribution: "Inana and Enki, ETCSL 1.3.1, segment F, lines 1–13",
            Pronunciation: "Enki EN-kee; Eridu EH-ree-doo; abzu AHB-zoo; Isimud EE-see-mood",
            Sources: "ETCSL 1.3.1 (segments F, H, J), https://etcsl.orinst.ox.ac.uk/section1/tr131.htm; ETCSL 4.07.2 ll. 1–12, https://etcsl.orinst.ox.ac.uk/section4/tr4072.htm; AMGG Enki/Ea, http://oracc.museum.upenn.edu/amgg/listofdeities/enki/; Getty 2019 ch. 9 (Zgoll); sheet 19 A10; sheet 23 B2, C4"),

        // Sheet 19 A12 (the Sacred Marriage); sheet 23 A11, B17 (Iddin-Dagan A, ETCSL 2.5.3.1)
        new TabletCard(
            Id: "goddess-sacred-marriage",
            Kind: TabletKind.Goddess,
            Title: "The Sacred Marriage and Dumuzid",
            ArtKey: "symbol-sacred-marriage-bed",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Holy Inanna's beloved is Dumuzid, a shepherd god. At a great New Year festival the king takes Dumuzid's part and is joined to the Goddess. This reminds the king to care for his people as a good shepherd cares for his sheep.",
            LongText: "The rite is known above all from a hymn of King Iddin-Dagan of Isin, who reigned 1974–1954 BCE. At the New Year a bed is prepared, the king lies with the Goddess as Ama-ušumgal-ana, another name of Dumuzid, and then he holds a great banquet.\n\nHow the Goddess appeared in the ritual is unclear: perhaps as a high priestess or the queen, perhaps as a statue; the most important text shows Her as a star looking down from the sky. Scholars also argue about how old the rite is. It was long thought to go back to the Uruk period, but in recent decades many have doubted it; Gebhard Selz thinks it did.",
            Quote: "At the New Year, on the day of the rites",
            QuoteAttribution: "Inana and Iddin-Dagan (Iddin-Dagan A), ETCSL 2.5.3.1, lines 169–180",
            Pronunciation: "Dumuzid doo-MOO-zid; Dumuzi doo-MOO-zee",
            Sources: "Getty 2019 ch. 9 (Zgoll), 'Rituals for the (City-)State'; ch. 39 (Selz); ETCSL 2.5.3.1, https://etcsl.orinst.ox.ac.uk/section2/tr2531.htm; AMGG Enlil/Ellil, http://oracc.museum.upenn.edu/amgg/listofdeities/enlil/ (Iddin-Dagan's dates); sheet 19 A12 and open question 11; sheet 23 A11, B17"),

        // Sheet 19 A13 (Enheduanna); sheet 23 B6, B8; sheet 21 Q01
        new TabletCard(
            Id: "goddess-enheduanna",
            Kind: TabletKind.Goddess,
            Title: "Enheduanna's Hymn",
            ArtKey: "hero-enheduanna",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Enheduanna, the daughter of King Sargon of Akkad, served as high priestess of the moon god Nanna at the city of Ur. She wrote great hymns to Holy Inanna more than 4,200 years ago, and she is the first author in the world whose name we know. About one hundred copies of one of her hymns survive.",
            LongText: "Enheduanna lived in the 23rd century BCE, in the Akkadian period (about 2340–2200 BCE). She served at Ur, the city of the moon god Nanna, where for several hundred years a king's daughter held the office of high priestess; she is the most famous of them. Everything found of hers, including the carved disk that shows her at a libation, came from Ur. No evidence found so far links her to Uruk; she is on this card because her hymns are the oldest known words addressed to Holy Inanna by a named writer. The hymn “Mistress of the Innumerable Divine Powers” (nin me šara) was copied again and again; about one hundred copies survive from around 1800 BCE, such as a tablet from Larsa now in the Louvre (AO 6713).\n\nIn the hymn called “The exaltation of Inana,” the speaker tells how a man named Lugal-ane drove her out of the temple, and she begs the Goddess for help.",
            Quote: "Let it be known that you are greatly exalted like heaven! Let it be known that you are immeasurably vast like the earth!",
            QuoteAttribution: "Enheduanna, hymn “Mistress of the Innumerable Divine Powers,” lines 123–24, in Annette Zgoll's English (Getty 2019 ch. 9)",
            Pronunciation: "Enheduanna en-heh-doo-AH-nah; Sargon SAR-gon",
            Sources: "Getty 2019 ch. 9 (Zgoll), fig. 9.4, note 6; ETCSL 4.07.2, https://etcsl.orinst.ox.ac.uk/section4/tr4072.htm; ETCSL 4.80.1, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; AMGG Nanna/Suen/Sin, http://oracc.museum.upenn.edu/amgg/listofdeities/nannasuen/; sheet 19 A13; sheet 21 Q01; sheet 23 B6, B8"),

        // Sheet 19 A14 (Her cult beyond Uruk); sheet 23 B8 (the Zabalam hymn)
        new TabletCard(
            Id: "goddess-beyond-uruk",
            Kind: TabletKind.Goddess,
            Title: "Her Cult Beyond Uruk",
            ArtKey: "symbol-eight-pointed-star",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Venus can be seen from everywhere, so Holy Inanna is honored everywhere. She is the chief goddess of Uruk, Zabalam, Kish and Akkad, and She has temples even in other gods' cities, such as Enlil's Nippur. One old list from Babylon names 180 shrines to Her in that city alone.",
            LongText: "Early in the third millennium BCE She was the city goddess of Kish in northern Babylonia. A lapis-lazuli foundation stone of Lugal-SILA-si, who calls himself king of Kish, about 2450 BCE, is addressed to An and Inanna (British Museum, BM 91013). The stories of Enmerkar and Lugalbanda say that faraway Aratta in the eastern mountains belonged to Her too.\n\nA list of cult places in Babylon, dated by A. R. George to the 12th century BCE, names 180 shrines to Ishtar beside 43 cult centers of the great gods. The Temple Hymns praise Her house at Zabalam as well as Her Eanna.\n\nWhen Sargon made Akkad his capital, he moved the cult of Holy Inanna there, a main reason Uruk lost importance.",
            Quote: "[...] who could be seen everywhere, was also venerated everywhere.",
            QuoteAttribution: "Annette Zgoll, on Inanna as Venus, Getty 2019 ch. 9, 'Global Authority'",
            Pronunciation: "Kish KISH; Nippur NIP-poor; Akkad AH-kkad; Aratta ah-RAHT-tah",
            Sources: "Getty 2019 ch. 9 (Zgoll), 'Global Authority', fig. 9.11; ch. 35; ETCSL 4.80.1 ll. 315–327, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; sheet 19 A14; sheet 23 B8"),

        // Sheet 19 A15 (raised hands on the rooftops); sheet 21 Q69; sheet 23 B17
        new TabletCard(
            Id: "goddess-raised-hands",
            Kind: TabletKind.Goddess,
            Title: "Raised Hands on the Rooftops",
            ArtKey: "symbol-raised-hands-prayer",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Families pray to Holy Inanna at home too. When Venus shines, people climb onto their flat rooftops and raise their hands toward Her. They hope the Goddess will look down kindly and grant their wish.",
            LongText: "“Rituals of raised hands,” a gesture of greeting and prayer, are known from the second millennium BCE on, and especially from sources of the first millennium BCE. They show how a person could bring a personal request to Her, with prayers and offerings. In return, the worshipper promised to spread the word of Her power.\n\nKing Iddin-Dagan's hymn to Her also tells of the people's offerings on the rooftops.",
            Quote: "When the planet Venus was visible in the sky—presumably pictured as an eye—people would climb up onto rooftops and try to attract the deity's gaze.",
            QuoteAttribution: "Annette Zgoll, Getty 2019 ch. 9, 'Rituals for Home and Family'",
            Pronunciation: "Holy Inanna HOH-lee ih-NAH-nah",
            Sources: "Getty 2019 ch. 9 (Zgoll), fig. 9.12; ETCSL 2.5.3.1, https://etcsl.orinst.ox.ac.uk/section2/tr2531.htm; sheet 19 A15; sheet 21 Q69; sheet 23 B17"),

        // Sheet 19 A4 (the three Inannas of the archaic texts); sheet 21 Q85; sheet 23 C10 (CDLI)
        new TabletCard(
            Id: "goddess-three-inannas",
            Kind: TabletKind.Goddess,
            Title: "The Three Inannas of the Oldest Tablets",
            ArtKey: "symbol-venus-evening",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The oldest writings from Her temple name more than one Inanna: the rising Inanna, the setting Inanna, and the Inanna high overhead. They match the places where Venus can be seen in the sky. There is also an “invisible Inanna” of the underworld, who seems to get no offerings.",
            LongText: "In the Late Uruk period (about 3500–3300 BCE, Eanna Levels VI–IV), Eanna was already a place where the Venus goddess ᵈinana-k was worshipped. Gebhard Selz, following Krystyna Szarzyńska and Piotr Steinkeller, lists the forms in the archaic texts: ᵈinana-UD, the gleaming or rising Inanna; ᵈinana-sig, the setting Inanna; ᵈinana-nun, the Inanna directly above; and, set against them, ᵈinana-kur, the Inanna of the underworld, the invisible one.\n\nOn some of the oldest tablets, scribes pressed the sign for the city next to Her reed-bundle sign. One tablet of about 3200–3000 BCE, kept by the German Archaeological Institute in Berlin (W 21446, CDLI P004400), sets the city sign beside Her sign and the sign for sheep. CDLI gives no translation, and reading such lines is a task for specialists.",
            Quote: "[...] a 'gleaming (rising) Inanna,' a 'setting Inanna,' and an 'Inanna directly above.'",
            QuoteAttribution: "Gebhard J. Selz, Getty 2019 ch. 39, p. 215",
            Pronunciation: "Inanna ih-NAH-nah",
            Sources: "Getty 2019 ch. 39 (Selz) p. 215 and note 1 (Szarzyńska 1993; Steinkeller 2002); CDLI P004400, https://cdli.earth/artifacts/4400; sheet 19 A4; sheet 21 Q85; sheet 23 C10"),

        // Sheet 19 A17 (Jordan's descent-from-heaven theology); sheet 21 A0, Q14, Q79; sheet 23 B9
        new TabletCard(
            Id: "goddess-stair-for-the-gods",
            Kind: TabletKind.Goddess,
            Title: "Jordan's Stair for the Gods",
            ArtKey: "deity-inanna-descending",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "In winter 1930/31 Julius Jordan uncovered the great stairs of Holy Inanna's ziggurat and two small temples at their foot. He wrote that the tower showed how people believed the gods came down from heaven. The gods arrived first at the temple on top, then came down the stairs to live among people.",
            LongText: "Jordan's team took the late mud-brick mantle off the northeast face of the Eanna ziggurat and found the three-part staircase. In the angles of the stairs stood two Deep Temples (Tieftempel), Neo-Babylonian buildings of 625–539 BCE that were still in use under Cyrus II. Jordan saw the tower with its summit temple (Gipfeltempel), the stairs and the deep temples as one design.\n\nHe built on an idea of Walter Andrae: the deity lived in the summit temple and appeared to mortals in the deep temple. Albert Schott took the idea up in a study of the Akkadian word sahuru. No trace of the top temple was found, but texts say the residence of the Goddess stood there. A Sumerian poem already calls Eanna “the house lowered down from heaven.”\n\nJordan wrote these words in his Third Preliminary Report (Dritter vorläufiger Bericht) of 1932.",
            Quote: "The belief in the heavenly powers, in the descent of their personifications from heaven to earth — where they were first received in the summit temple, where they passed through the door of heaven at the top of the tower, came down to mortals, and dwelt and worked among them — could scarcely have found a more dignified or more beautiful architectural expression than it has found here.",
            QuoteAttribution: "Julius Jordan, Third Preliminary Report (Dritter vorläufiger Bericht), 1932, pp. 33–34",
            Pronunciation: "Julius Jordan YOOL-yoos YOR-dahn; Tieftempel TEEF-tem-pel; Eanna ay-AHN-nah",
            Sources: "Jordan 1932 (UVB III) pp. 24, 31–34; Getty 2019 ch. 37 (van Ess); ETCSL 1.8.1.1 ll. 30–39, https://etcsl.orinst.ox.ac.uk/section1/tr1811.htm; HER_PRESENCE.txt sec. V; sheet 19 A17 and open question 3; sheet 21 A0, Q14, Q79; sheet 23 B9"),

        // ---------------------------------------------------------------------------------------------------------
        // PANTHEON (15)
        // ---------------------------------------------------------------------------------------------------------

        // Sheet 19 B1 (An, the Resh, Kullab); sheet 23 A9, A22 (the sign AN)
        new TabletCard(
            Id: "pantheon-an",
            Kind: TabletKind.Pantheon,
            Title: "An, God of the Sky",
            ArtKey: "deity-an",
            Cuneiform: "𒀭",
            CuneiformReading: "AN, also read dingir: 'sky', the god An, and the sign for 'god'",
            CardText: "An is the god of the sky and the father of the gods. His name and the word for heaven are written with the same sign. He is Uruk's other patron god, but for many centuries Holy Inanna is the more important of the two.",
            LongText: "From the third millennium BCE on, An was worshipped, with some breaks, together with Inanna in Eanna. The sign AN can mean “sky,” it can name the god An, and it is the god sign, the Dingir; in Sumerian, An's own name is never written with the god sign in front. ETCSL translates Eanna as “House of heaven”; whether people also heard “House of An” in it is an inference, not something the sources say.\n\nIn the Seleucid period (332–141 BCE) Uruk built An, called Anu in Akkadian, and his wife Antum a giant temple, the Resh, on the old platform of the so-called Anu Ziggurat. Its builders took names like Anu-uballit, “Anu makes one strong.” Jordan's very first season at Warka, 1912/13, dug in the Resh.",
            Quote: "An, my father ... You have made me terrifying among the deities in heaven",
            QuoteAttribution: "Holy Inanna to An, in Inana and Ebih, ETCSL 1.3.2, lines 65–69",
            Pronunciation: "An AHN; Anu AH-noo; Antu AHN-too; Kullaba KOOL-lah-bah",
            Sources: "K. Stevens, AMGG An/Anu, http://oracc.museum.upenn.edu/amgg/listofdeities/an/; Oracc Sign List AN, http://oracc.org/osl/signlist/o0000099; ETCSL 1.3.2, https://etcsl.orinst.ox.ac.uk/section1/tr132.htm; Getty 2019 ch. 1, 12, 35, 37; sheet 19 B1; sheet 23 A9, A22 and open question 8"),

        // Sheet 19 B2 (Enlil); sheet 23 A10 (Nippur and the Ekur)
        new TabletCard(
            Id: "pantheon-enlil",
            Kind: TabletKind.Pantheon,
            Title: "Enlil of Nippur",
            ArtKey: "deity-enlil",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Enlil is the king of the gods, the “Great Mountain” who decides fates. His great temple, the Ekur or “Mountain House,” stands in the city of Nippur. When Holy Inanna is trapped in the land below, Enlil refuses to help Her.",
            LongText: "Kings said Enlil chose them to rule, and his command could not be changed. His Akkadian name is Ellil, and scribes could write his name with the number 50 (ᵈ50). Beside the Ekur stood the ziggurat Dur-an-ki, “bond of heaven and earth,” built by Ur-Nammu. People had to travel to Nippur to join his great festival, and even there Holy Inanna had a temple of Her own.\n\nIn the Descent, Her minister begs, “Father Enlil, don't let anyone kill your daughter,” but Enlil says no. Whether the signs for “Enlil” first wrote only the place name Nippur, around 3200–2800 BCE, is still much debated.",
            Quote: "The divine powers of the underworld are divine powers which should not be craved",
            QuoteAttribution: "Enlil's answer, in Inana's descent to the nether world, ETCSL 1.4.1, lines 190–194",
            Pronunciation: "Enlil EN-lil; Ekur EH-koor; Nippur NIP-poor",
            Sources: "A. Stone, AMGG Enlil/Ellil, http://oracc.museum.upenn.edu/amgg/listofdeities/enlil/; ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; Getty 2019 ch. 9 (Zgoll), ch. 37 (van Ess); sheet 19 B2; sheet 23 A10, A22"),

        // Sheet 23 A8 (Enki at Eridu); sheet 19 B3
        new TabletCard(
            Id: "pantheon-enki",
            Kind: TabletKind.Pantheon,
            Title: "Enki of Eridu",
            ArtKey: "deity-enki",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Enki, called Ea in Akkadian, is the wise god of fresh water. He lives in the Abzu, a sweet-water sea under the earth, and his temple stands at Eridu. Old stories say he made the reed marshes of southern Iraq.",
            LongText: "His temple at Eridu is the E-abzu, “house of the abzu,” also called E-unir. He is the god of wisdom, magic and crafts. Pictures show a bearded god in a horned cap with streams full of fish flowing from him, and the goat-fish is one of his signs.\n\nHoly Inanna calls him “my father.” He gives Her the divine powers in one story, and in the Descent he is the only god who helps: from the dirt under his fingernails he makes two little beings who carry the plant and the water of life down to revive Her.\n\nThe figures pouring water on Karaindash's façade at Eanna are often read as a mountain god or as Ea, but that is not certain.",
            Quote: "O E-unir (House which is a ziqqurat) ... great banqueting hall of Eridug! Abzu, shrine erected for its prince",
            QuoteAttribution: "The Temple Hymns, ETCSL 4.80.1, lines 1–7",
            Pronunciation: "Enki EN-kee; Ea AY-ah; Eridu EH-ree-doo; abzu AHB-zoo",
            Sources: "R. Horry, AMGG Enki/Ea, http://oracc.museum.upenn.edu/amgg/listofdeities/enki/; ETCSL 4.80.1, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; ETCSL 1.3.1, https://etcsl.orinst.ox.ac.uk/section1/tr131.htm; Getty 2019 ch. 15 (van Ess and Neef); Karaindash COMPREHENSIVE_BUILDING_OVERVIEW; sheet 19 B3; sheet 23 A8"),

        // Sheet 23 A7 (Nanna and Ningal); sheet 19 B4
        new TabletCard(
            Id: "pantheon-nanna-and-ningal",
            Kind: TabletKind.Pantheon,
            Title: "Nanna and Ningal, Her Father and Mother",
            ArtKey: "deity-nanna",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Nanna, the moon god, is Holy Inanna's father, and the goddess Ningal is Her mother. Nanna's great temple stands in the city of Ur. His sign is the crescent moon, which looks like the horns of a bull.",
            LongText: "In Akkadian the moon god is Suen or Sin. His temple at Ur is the E-kiš-nu-ĝal. He is the god of the moon, of cattle and fertility, and of oaths and judgment. His number is 30, and his daughter's is 15. In the oldest god list, from Fara, he comes right after An, Enlil, Inanna and Enki.\n\nHymns call Her the “great child of Suen.” At the end of the hymn “The exaltation of Inana,” both of Her parents greet Her.",
            Quote: "Nanna came out to gaze at her properly, and her mother Ningal blessed her",
            QuoteAttribution: "The exaltation of Inana (Inana B), ETCSL 4.07.2, lines 144–154",
            Pronunciation: "Nanna NAHN-nah; Suen SOO-en; Ningal NIN-gahl; Ur OOR",
            Sources: "A. Stone, AMGG Nanna/Suen/Sin, http://oracc.museum.upenn.edu/amgg/listofdeities/nannasuen/; ETCSL 4.07.2, https://etcsl.orinst.ox.ac.uk/section4/tr4072.htm; ETCSL 1.3.2, https://etcsl.orinst.ox.ac.uk/section1/tr132.htm; Getty 2019 ch. 9 (Zgoll), ch. 37; sheet 19 B4; sheet 23 A7"),

        // Sheet 23 A6 (Utu/Šamaš); sheet 19 B5
        new TabletCard(
            Id: "pantheon-utu",
            Kind: TabletKind.Pantheon,
            Title: "Utu, the Sun",
            ArtKey: "deity-utu",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Utu, called Shamash in Akkadian, is the sun god and Holy Inanna's brother. Because the sun sees everything, he is the god of truth and fair judgment. His temples, each called the White House, stand at Sippar and Larsa.",
            LongText: "Utu is the son of the moon god and Ningal. The Oracc god list calls him Inanna's twin, while the poems say only “brother.” His wife is Aya, goddess of the dawn. He protects travellers and merchants. His sign is the sun disc, often a four-pointed star with wavy lines between the points.\n\nOn the greenstone seal of Adda in the British Museum (BM 89115), from the Akkadian period, Shamash cuts through the mountains on the horizon so he can rise in the morning. In the poems, Dumuzid begs his brother-in-law Utu to turn his hands into snake's hands so he can escape the demons, and Gilgamesh asks Utu's help before his journey to the cedar mountains.\n\nHe is not the same as Utu-hegal, a later king of Uruk.",
            Quote: "A decision that concerns the mountains is Utu's business",
            QuoteAttribution: "Gilgameš and Huwawa (version A), ETCSL 1.8.1.5, lines 8–12",
            Pronunciation: "Utu OO-too; Šamaš SHAH-mahsh",
            Sources: "R. Horry, AMGG Utu/Šamaš, http://oracc.museum.upenn.edu/amgg/listofdeities/utu/; CDLI P458063, https://cdli.earth/artifacts/458063; ETCSL 1.8.1.5, https://etcsl.orinst.ox.ac.uk/section1/tr1815.htm; ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; Getty 2019 ch. 9 (Zgoll), ch. 14; sheet 19 B5; sheet 23 A6 and open question 16"),

        // Sheet 23 A1 (Ninšubur)
        new TabletCard(
            Id: "pantheon-ninshubur",
            Kind: TabletKind.Pantheon,
            Title: "Ninshubur, Her Faithful Minister",
            ArtKey: "deity-ninshubur",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Ninshubur is Holy Inanna's minister, Her most loyal helper. When the Goddess dies in the land below, Ninshubur goes from god to god asking for help, until Enki saves Her. Afterwards Holy Inanna will not let the demons take Ninshubur away.",
            LongText: "Before She goes down, Holy Inanna tells Ninshubur (Ninšubur) what to do: make a lament, beat the drum in the sanctuary, and go to Enlil, then to Nanna at Ur, then to Enki at Eridu. After three days and three nights Ninshubur does it all. Enlil and Nanna refuse; Enki helps. When the demons want to take Ninshubur in Her place, the Goddess says, “She brought me back to life.”\n\nIn “Inana and Enki,” it is Ninshubur who saves the Boat of Heaven each time Enki's creatures try to seize it. In the Sumerian poems Ninshubur is a goddess; much later she was merged with a male god, Papsukkal, and late figurines show a bearded man.",
            Quote: "Ninšubur, the true minister of E-ana, has erected a house in your precinct, O E-akkil",
            QuoteAttribution: "The Temple Hymns, ETCSL 4.80.1, lines 221–229",
            Pronunciation: "Ninšubur nin-SHOO-boor",
            Sources: "ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; ETCSL 1.3.1, https://etcsl.orinst.ox.ac.uk/section1/tr131.htm; ETCSL 4.80.1, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; Y. Heffron, AMGG Papsukkal, http://oracc.museum.upenn.edu/amgg/listofdeities/papsukkal/; sheet 23 A1 and open question 13"),

        // Sheet 23 A11 (Dumuzid), A12 (Geštinanna), B5 (Dumuzid's dream); sheet 19 B6
        new TabletCard(
            Id: "pantheon-dumuzid-and-geshtinanna",
            Kind: TabletKind.Pantheon,
            Title: "Dumuzid and Geshtinanna",
            ArtKey: "deity-dumuzi",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Dumuzid is a shepherd god and Holy Inanna's husband. When She comes back from the land below, someone must take Her place, and She chooses him. His sister Geshtinanna, a wise woman who reads dreams, will not tell the demons where he hides.",
            LongText: "Coming back from below, the Goddess finds Dumuzid (also Dumuzi) “seated magnificently on a throne” under a great apple tree in the plain of Kulaba, and She hands him over to the demons. In another poem, “Dumuzid's dream,” he dreams of his own death and calls for his sister Geshtinanna (Geštinanna), a scribe and a reader of dreams. She refuses to betray him, and the sun god Utu turns his hands and feet into a gazelle's to help him run, but the demons find him in his sister's sheepfold.\n\nThe Descent ends with a half-year rule: “You for half the year and your sister for half the year.” The poem does not name the sister; the Oracc god list reads her as Geshtinanna. Dumuzid's own city is Bad-tibira.",
            Quote: "Bring my wise woman, who knows the meanings of dreams, bring my sister!",
            QuoteAttribution: "Dumuzid's dream, ETCSL 1.4.3, lines 19–25",
            Pronunciation: "Dumuzid doo-MOO-zid; Dumuzi doo-MOO-zee; Geštinanna gesh-tin-AHN-nah; Kulaba KOO-lah-bah",
            Sources: "ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; ETCSL 1.4.3, https://etcsl.orinst.ox.ac.uk/section1/tr143.htm; ETCSL 4.80.1, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; N. Brisch, AMGG Geštinanna/Belet-ṣeri, http://oracc.museum.upenn.edu/amgg/listofdeities/getinanna/; Getty 2019 ch. 9 (Zgoll); sheet 19 B6; sheet 23 A11, A12, B5 and open question 17"),

        // Sheet 23 A2 (Ereškigal), A3 (Nergal)
        new TabletCard(
            Id: "pantheon-ereshkigal-and-nergal",
            Kind: TabletKind.Pantheon,
            Title: "Ereshkigal and Nergal, Rulers of the Great Below",
            ArtKey: "deity-ereshkigal",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Ereshkigal is Queen of the Great Below, the land of the dead, and Holy Inanna's elder sister. Her palace has seven gates, and she orders them all bolted when her sister comes. In later stories the fierce god Nergal becomes her husband and rules beside her.",
            LongText: "In the Descent, Holy Inanna makes Her sister rise from her throne and sits on it Herself, and then the judges of the dead turn on Her. Ereshkigal's palace is called Ganzer, and its chief doorman is Neti. The poem's last line praises her, not Holy Inanna. Few temples were ever built for her: inscriptions name temples at Kutha, Assur and Umma, and Nebuchadnezzar rebuilt her temple at Kutha. No certain picture of her is known.\n\nNergal is the god of plague, sickness and war, with his main temple at Kutha. His symbol is a mace topped with lion heads. In the Akkadian myth “Nergal and Ereškigal,” the two come to rule the land of the dead together. Their shared rule begins in Old Babylonian times, and the Oracc god list calls the marriage a fairly late idea.",
            Quote: "Holy Ereškigala — sweet is your praise",
            QuoteAttribution: "The last line of Inana's descent to the nether world, ETCSL 1.4.1, lines 411–412",
            Pronunciation: "Ereškigal eh-RESH-kee-gahl; Nergal NAIR-gahl",
            Sources: "ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; Y. Heffron, AMGG Ereškigal, http://oracc.museum.upenn.edu/amgg/listofdeities/erekigal/; Y. Heffron, AMGG Nergal, http://oracc.museum.upenn.edu/amgg/listofdeities/nergal/; sheet 23 A2, A3"),

        // Sheet 23 A13 (Nanaya); sheet 19 B7, A16
        new TabletCard(
            Id: "pantheon-nanaya",
            Kind: TabletKind.Pantheon,
            Title: "Nanaya, Goddess of Love",
            ArtKey: "deity-nanaya",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Nanaya is a goddess of love and a daughter of the sky god An. In Uruk's last great age she shares Holy Inanna's huge temple of baked brick. Scholars still argue whether she began as a form of Holy Inanna or as a goddess of her own.",
            LongText: "Michael Streck and Nathan Wasserman call Nanaya a goddess of sex and love, also bright, raging, just and wise, and close to the king. She was also called Irnina, “goddess of victory.” In an Old Babylonian hymn now in Berlin, King Samsuiluna of Babylon brings her offerings, and she grants him life and kingship.\n\nShe shares many traits with Ishtar and was possibly identified with Her in later times. The scholar Stol sees the two as merged, while Joan Westenholz stresses that Nanaya was never simply a form of Ishtar. A Sumerian song is titled “A balbale to Inana as Nanaya.”\n\nIn the Seleucid period (332–141 BCE) Nanaya joined Ishtar in the Eshgal temple at Uruk.",
            Quote: "[...] the shrine of Ishtar and now also the Goddess Nanaya [...]",
            QuoteAttribution: "Getty 2019 ch. 35, on the Seleucid Eshgal",
            Pronunciation: "Nanaya nah-NAH-yah",
            Sources: "M. P. Streck and N. Wasserman, 'More Light on Nanaya', ZA 102 (2012) 183–201, https://www.gko.uni-leipzig.de/fileadmin/Fakult%C3%A4t_GKR/Altorientalisches_Institut/Dokumente/Institut/Mitarbeiter/Streck_Dokumente/Streck_Wasserman_2012_Nanaya.pdf; ETCSL 4.07.8, https://etcsl.orinst.ox.ac.uk/section4/tr4078.htm; Getty 2019 ch. 35, fig. 35.6; sheet 19 A16, B7; sheet 23 A13"),

        // Sheet 23 A5 (Ninsumun), A15 (Lugalbanda and Ninsumun), B14; sheet 19 B10
        new TabletCard(
            Id: "pantheon-ninsun-and-lugalbanda",
            Kind: TabletKind.Pantheon,
            Title: "Ninsun and Lugalbanda, Parents of Gilgamesh",
            ArtKey: "deity-ninsun",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Ninsun, whose name means “Lady of the Wild Cows,” is the mother of Gilgamesh, and holy Lugalbanda is his father. The king list says Lugalbanda ruled Uruk for 1,200 years. Later kings of Ur call them their divine parents too.",
            LongText: "Ninsun is the older reading of her name; Ninsumun is now preferred. In the Standard Babylonian epic she pleads with the sun god for her son. King Šulgi of Ur called Ninsun and Lugalbanda his divine parents and Gilgamesh his brother, and at Uruk the two still received offerings in the Old Babylonian period.\n\nIn the king list Lugalbanda, the shepherd, is the third king of Uruk's first dynasty. In the poem “Lugalbanda in the mountain cave,” he falls ill on the march to Aratta and is left behind in a cave. He prays to the sun, to Holy Inanna as the evening star, and to the moon, and he is healed. The Getty book notes that he is known only from literature.",
            Quote: "By the life of my own mother Ninsun and of my father, holy Lugalbanda!",
            QuoteAttribution: "Gilgameš swears an oath, in Gilgameš and Huwawa (version A), ETCSL 1.8.1.5, lines 90–91",
            Pronunciation: "Ninsun nin-SOON; Ninsumun nin-SOO-moon; Lugalbanda loo-gahl-BAHN-dah",
            Sources: "N. Brisch, AMGG Ninsumun (Ninsun), http://oracc.museum.upenn.edu/amgg/listofdeities/ninsumun/; ETCSL 1.8.1.5, https://etcsl.orinst.ox.ac.uk/section1/tr1815.htm; ETCSL 2.4.2.04, https://etcsl.orinst.ox.ac.uk/section2/tr24204.htm; ETCSL 2.1.1, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; ETCSL 1.8.2.1, https://etcsl.orinst.ox.ac.uk/section1/tr1821.htm; Getty 2019 ch. 35; sheet 19 B10; sheet 23 A5, A15, B14"),

        // Sheet 19 B12 (Gilgameš, the wall, Agga); sheet 23 A16, A20 (the Bull of Heaven); sheet 20 B23, B27
        new TabletCard(
            Id: "pantheon-gilgamesh",
            Kind: TabletKind.Pantheon,
            Title: "Gilgamesh, King of Uruk",
            ArtKey: "hero-gilgamesh",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Gilgamesh is the most famous king of Uruk. The poems say he built Uruk's great wall, stood up to King Agga of Kish, and killed the Bull of Heaven that Holy Inanna sent against the city. No one has proved that he really lived.",
            LongText: "Uruk's real wall was about 9 km long (one Getty chapter says nearly 10), with about 900 half-round towers, built at the start of the third millennium BCE. Early versions of the epic already credit it to Gilgamesh, and one old copy tells him to climb onto the wall of Uruk and inspect its brickwork.\n\nIn “Gilgameš and Aga,” the young men of Uruk want to fight Aga of Kish, son of Enmebaragesi, and Gilgamesh sides with them. Enmebaragesi is known from an inscription of his own time, perhaps the 27th century BCE, so the tale may hold a bit of history. In “Gilgameš and the Bull of Heaven,” Holy Inanna asks Her father An for the Bull; Gilgamesh kills it, gives its meat to the widows' sons, and makes its horns into oil flasks for the Goddess's temple.\n\nThe king list says he ruled for 126 years. After his death, poems say, he became a judge in the land of the dead.",
            Quote: "You watch over Unug, the handiwork of the gods, the great rampart, the rampart which An founded",
            QuoteAttribution: "The young men of Uruk to Gilgameš, in Gilgameš and Aga, ETCSL 1.8.1.1, lines 107–113",
            Pronunciation: "Gilgamesh GIL-gah-mesh; Enkidu EN-kee-doo; Kulaba KOO-lah-bah",
            Sources: "Getty 2019 ch. 1, 12, 16, 35; ETCSL 1.8.1.1, https://etcsl.orinst.ox.ac.uk/section1/tr1811.htm; ETCSL 1.8.1.2, https://etcsl.orinst.ox.ac.uk/section1/tr1812.htm; ETCSL 1.8.1.3, https://etcsl.orinst.ox.ac.uk/section1/tr1813.htm; ETCSL 2.1.1, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; SEAL 1577, https://seal.huji.ac.il/node/1577; sheet 19 B12 and open question 8; sheet 20 B23, B27; sheet 23 A16, A20"),

        // Sheet 23 A17 and B13 (Enmerkar and the lord of Aratta, ETCSL 1.8.2.3); sheet 19 B11
        new TabletCard(
            Id: "pantheon-enmerkar",
            Kind: TabletKind.Pantheon,
            Title: "Enmerkar and the Lord of Aratta",
            ArtKey: "hero-enmerkar",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Enmerkar is a king of Uruk in old stories. He wants the far-off mountain city of Aratta to send gold, silver and lapis lazuli for Holy Inanna's temple. His messages grow so long that he presses one into clay, and the poem says this is the first letter ever written.",
            LongText: "In the king list Enmerkar is the son of Meš-ki-aĝ-gašer, “the king of Unug, who built Unug,” and he rules for 420 years. The poems call him a son of the sun god Utu and call Holy Inanna his sister. He asks that Aratta build Her “a temple brought down from heaven,” the shrine Eanna.\n\nHis messenger cannot remember the long riddles, so the lord of Kulaba pats some clay and writes the message on it. The lord of Aratta looks at the tablet and sees only nails, the wedge-shaped marks of cuneiform. In the end the Goddess favours Uruk.\n\nAnnette Zgoll points out that the Uruk stories of Enmerkar and Lugalbanda show that Aratta, to the east, was also subject to Inanna.",
            Quote: "Formerly, the writing of messages on clay was not established.",
            QuoteAttribution: "Enmerkar and the lord of Aratta, ETCSL 1.8.2.3, lines 500–514",
            Pronunciation: "Enmerkar en-MEHR-kar; Aratta ah-RAHT-tah; Kulaba KOO-lah-bah",
            Sources: "ETCSL 1.8.2.3, https://etcsl.orinst.ox.ac.uk/section1/tr1823.htm; ETCSL 2.1.1, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; Getty 2019 Introduction and note 3; ch. 9 (Zgoll), 'Global Authority'; sheet 19 B11; sheet 23 A17, B13"),

        // Sheet 19 B13 (Etana and the eagle); sheet 23 A18 (Etana in the king list)
        new TabletCard(
            Id: "pantheon-etana",
            Kind: TabletKind.Pantheon,
            Title: "Etana, Who Flew to Heaven",
            ArtKey: "hero-etana",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Etana is an early king of the city of Kish. He longs for a son, so he flies up to heaven on the back of an eagle. There he finds the Goddess on Her throne, with lions lying beneath it, and She gives him what he needs.",
            LongText: "The Sumerian King List names “Etana, the shepherd, who ascended to heaven,” a king of the first dynasty of Kish after the Flood, and says he ruled for 1,500 years (one copy says 635). His son Balih follows him.\n\nThe full story is an Akkadian myth. Etana needs a herb that will give him a son. His first flight fails. In a dream he enters a house where a crowned young woman sits on a throne with lions beneath it, and on a second flight he reaches the Goddess. Annette Zgoll points out that the gift of a son comes from Inanna/Ishtar in heaven, not from the powers of the underworld.",
            Quote: "Beneath the throne [l]ay lio[ns].",
            QuoteAttribution: "The Akkadian myth of Etana, as quoted by Annette Zgoll, Getty 2019 ch. 9",
            Pronunciation: "Etana eh-TAH-nah; Kish KISH",
            Sources: "Getty 2019 ch. 9 (Zgoll), note 7; ETCSL 2.1.1 ll. 40–94, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; sheet 19 A5, B13; sheet 23 A18"),

        // Sheet 23 A4 (the Anuna, the seven judges); chosen over Tiamat (sheet 19 B14) as the better sourced
        new TabletCard(
            Id: "pantheon-anuna",
            Kind: TabletKind.Pantheon,
            Title: "The Anuna, the Seven Judges",
            ArtKey: "symbol-netherworld-descent",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Anuna are the great gods who decide the fates. In the story of Holy Inanna's journey below, seven of them sit as judges. Their look is the look of death, and even the great Goddess must obey the rules of the land below.",
            LongText: "The name Anuna (in Akkadian, the Anunnaki) probably means “those of princely seed.” In Sumerian texts it is used for the highest gods, or for the gods of one city; after the Old Babylonian period it means the gods of the netherworld. One text speaks of the fifty Anuna of Eridu, and the Babylonian creation poem Enūma eliš counts 600.\n\nIn the Descent, after the judges' verdict, the Goddess is turned into a corpse and hung on a hook. When She rises again, the Anuna seize Her and demand that She find someone to take Her place. No temple of the Anuna is known, and there are no known pictures of them.",
            Quote: "The Anuna, the seven judges, rendered their decision against her. They looked at her — it was the look of death.",
            QuoteAttribution: "Inana's descent to the nether world, ETCSL 1.4.1, lines 164–172",
            Pronunciation: "Anuna ah-NOO-nah; Anunnaki ah-noon-NAH-kee",
            Sources: "ETCSL 1.4.1, https://etcsl.orinst.ox.ac.uk/section1/tr141.htm; N. Brisch, AMGG Anunna (Anunnaku, Anunnaki), http://oracc.museum.upenn.edu/amgg/listofdeities/anunna/; sheet 23 A4"),

        // Sheet 23 A14 (Ningišzida); chosen over the reed deity (sheet 19 B19) as the better sourced
        new TabletCard(
            Id: "pantheon-ningishzida",
            Kind: TabletKind.Pantheon,
            Title: "Ningishzida, Lord of the True Tree",
            ArtKey: "symbol-serpent-coiled",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Ningishzida is a god of growing plants and of snakes. People say he goes down to the land of the dead when the plants die. On one king's seal, dragons rise from his shoulders.",
            LongText: "His name, Ningišzida, means “Lord of the true tree.” He is also called the chair-bearer of the netherworld and even the lord of the innkeepers. His signs are snakes, the sickle sword and the constellation Hydra. He was the personal god of Gudea of Lagash, and on Gudea's seal dragons grow from his shoulders. His journey below lasts from midsummer to midwinter, the time when plants die back.\n\nHis first home was Gišbanda, upstream from Ur; later he had temples at Ur and Lagash, and possibly at Uruk. At Lagash his wife is Geshtinanna, Holy Inanna's sister-in-law. He is first named in the Fara god list of Early Dynastic III, and in the Ur III period his festival was held in the third month.\n\nIn “The death of Gilgameš,” the hero's words below are to weigh as much as those of Ningišzida and Dumuzid.",
            Quote: "Ningišzida has erected a house in your precinct, O Gišbanda",
            QuoteAttribution: "The Temple Hymns, ETCSL 4.80.1, lines 187–197",
            Pronunciation: "Ningišzida nin-gish-ZEE-dah",
            Sources: "A. Stone, AMGG Ningišzida, http://oracc.museum.upenn.edu/amgg/listofdeities/ningizida/; ETCSL 4.80.1, https://etcsl.orinst.ox.ac.uk/section4/tr4801.htm; ETCSL 1.8.1.3, https://etcsl.orinst.ox.ac.uk/section1/tr1813.htm; sheet 23 A14"),

        // ---------------------------------------------------------------------------------------------------------
        // CULTURE (15)
        // ---------------------------------------------------------------------------------------------------------

        // Sheet 20 B1 (writing), B2 (the Titles and Professions List); sheet 23 C6 (Archaic Lu A), C2
        new TabletCard(
            Id: "culture-writing",
            Kind: TabletKind.Culture,
            Title: "Writing and the List of Professions",
            ArtKey: "object-archaic-tablet",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The oldest writing in Mesopotamia was found at Uruk. At first it is not for stories: scribes press numbers and signs into clay with a pointed reed to keep track of barley, sheep and other goods. One of the very first things they write down is a list of job titles.",
            LongText: "Almost all of the roughly two thousand oldest tablets from Uruk record the city's business. N. Veldhuis puts the start of writing in the late Uruk period, around 3200 BCE; J. Friberg says about 3300 BCE. The first tablets used only word signs and number signs, and lists of signs were kept to standardize them; the system grew into cuneiform. Tablets were made to be held in one hand.\n\nThe Titles and Professions List (Archaic Lu A) begins with a title later translated as “king,” then the heads of justice, the city, barley, the plow and the workers, and a leader of the assembly. Scribes copied it faithfully for more than a thousand years, in cities far from Uruk such as Fara, Abu Salabikh, Tell Brak and Kish. One copy from Uruk is VAT 01533 in Berlin (CDLI P000001).",
            Quote: "[...] also functioned as symbols of the identity of a new class in Uruk society: the scribes.",
            QuoteAttribution: "N. Veldhuis, on the archaic word lists, 'Archaic Lexical Texts', DCCLT (Oracc), 2019",
            Pronunciation: "Uruk OO-rook",
            Sources: "Getty 2019 ch. 1, ch. 14 ('Economy and Administration'), ch. 28; N. Veldhuis, DCCLT, http://oracc.museum.upenn.edu/dcclt/lexicallistsperiods/archaic/; K. Wagensonner, CDLN 2014:15, https://cdli.earth/articles/cdln/2014-15; CDLI P000001, https://cdli.earth/artifacts/1; J. Friberg 2019, https://research.chalmers.se/en/publication/508683; sheet 20 B1, B2; sheet 23 C2, C6"),

        // Sheet 20 B3 (stamp seals, cylinder seals, tokens); sheet 23 C8 (how cylinder seals were made)
        new TabletCard(
            Id: "culture-seals",
            Kind: TabletKind.Culture,
            Title: "Seals, Stamps and Tokens",
            ArtKey: "object-cylinder-seal-and-impression",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "A stamp seal makes one picture, like a rubber stamp. A cylinder seal is a little stone tube that you roll across wet clay to make a long picture. Cylinder seals appear while Uruk is growing fast, and they show who owns the goods.",
            LongText: "From the seventh millennium BCE on, people pressed stamp seals into clay to show who owned or was in charge of something, and they counted stock with small clay tokens. Cylinder seals came with the growing city: the Getty book says around the middle of the fourth millennium BCE, while Gorelick and Gwinnett say around 3300 BCE. Their bigger surface held more detailed pictures, so more people could have their own seal. The older stamps and tokens did not disappear.\n\nTo make a seal, the cutter shaped a small stone tube and drilled a hole through it from both ends. Gorelick and Gwinnett think loose sand was used to wear away the hard stone, and they suggest, carefully, that the bow lathe was invented at this time. Some seals found in Eanna had silver or bronze handles shaped like animals and came from fill with cult objects, so they were probably not for everyday use.",
            Quote: "[Cylinder seals] were part of the burst of creative energy and invention that accompanied urbanization in Mesopotamia around 3300 B.C.",
            QuoteAttribution: "L. Gorelick and A. J. Gwinnett, Expedition 23.4 (1981)",
            Pronunciation: "",
            Sources: "Getty 2019 ch. 14, fig. 14.2; ch. 20; ch. 39, fig. 39.1; L. Gorelick and A. J. Gwinnett, Expedition 23.4 (1981), https://www.penn.museum/sites/expedition/the-origin-and-development-of-the-ancient-near-eastern-cylinder-seal/; sheet 20 B3; sheet 23 C8"),

        // Sheet 23 C9 (the beveled-rim bowl and the ration theory)
        new TabletCard(
            Id: "culture-beveled-rim-bowl",
            Kind: TabletKind.Culture,
            Title: "The Beveled-Rim Bowl",
            ArtKey: "object-beveled-rim-bowl",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Rough, plain bowls with a slanting rim turn up at sites of the Uruk period. For a long time people thought each held a worker's share of grain. New tests show some held meat meals, and experiments show they bake good bread, so scholars still disagree about what they were for.",
            LongText: "One example in the Penn Museum (31-16-324) comes from Ur and dates to about 3400–3100 BCE. It is 8 cm high and 16.7 cm across, made of crude greenish grey ware, with a flat base and a rough overhanging rim.\n\nBeveled-rim bowls are usually read as ration containers for handing out grain. In 2022 a team led by Claudia Glatz of the University of Glasgow tested bowls from Shakhi Kora in north-eastern Iraq and found traces of many foods, especially meat-based meals. Jill Goulder's experiments showed that the bowls bake a fine loaf of risen bread.",
            Quote: "[...] thick walls and conical shape produce a fine loaf of risen bread, supplied perhaps as tasty recompense to those undertaking the newly-proliferating public administrative duties.",
            QuoteAttribution: "Jill Goulder, 'Administrators' bread', Antiquity 84 (2010)",
            Pronunciation: "",
            Sources: "Penn Museum 31-16-324, https://collections.penn.museum/collections/object/297361; University of Glasgow news, 18 November 2022, https://www.gla.ac.uk/schools/humanities/latestnews/newsarchive/2022/headline_897243_en.html; J. Goulder, Antiquity 84.324 (2010) 351–362, DOI 10.1017/S0003598X0006662X; sheet 23 C9"),

        // Sheet 20 B8 (reed as raw material); sheet 19 B19 (the reed deity)
        new TabletCard(
            Id: "culture-reed",
            Kind: TabletKind.Culture,
            Title: "Reed, Sumer's Everyday Material",
            ArtKey: "object-bitumen-reed-boat",
            Cuneiform: "𒄀",
            CuneiformReading: "gi: reed",
            CardText: "Reeds grow everywhere in Sumer. People use them to build houses, boats and fences, to weave mats and baskets, to write, and even to make flutes. If the reed marshes dry up, it is a sign that hunger is coming.",
            LongText: "The common reed (Phragmites australis) sprouts in December or January. Its leaves feed cattle, and young shoots and roots are eaten and used in beer. By fall the stalks are hard and yellow, good for building. Reed boats are sealed with bitumen, and old reeds are fuel for bread ovens and kilns.\n\nWomen and children cut green reed for fodder; older reed was probably cut by men with bronze sickles, two to a boat, one cutting and one bundling. The bales were stored and officially counted. Myth says the god Enki made the reed marshes, and there was even a little-known reed god. A text of the 26th century BCE calls reed sacred, and it had the power to make things pure in spells.\n\nThe first settlers at Eanna built on thick layers of reed, and Holy Inanna's own name is written with the sign of a reed bundle.",
            Quote: "",
            QuoteAttribution: "",
            Pronunciation: "Enki EN-kee",
            Sources: "Getty 2019 ch. 15 (van Ess and Neef), figs. 15.1–15.5; ch. 11, fig. 11.2; ch. 12; ch. 39 (Selz); sheet 19 B19; sheet 20 B8"),

        // Sheet 20 B9 (mud brick, Riemchen, fired brick, bitumen); sheet 21 Q52
        new TabletCard(
            Id: "culture-bricks",
            Kind: TabletKind.Culture,
            Title: "Mud Brick, Riemchen and Fired Brick",
            ArtKey: "object-riemchen-bricks",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Most buildings in Sumer are made of mud bricks dried in the sun. Baking bricks in a fire makes them much stronger, but it uses a lot of fuel, which is rare. So baked bricks are saved for temples and palaces, often set in sticky black bitumen.",
            LongText: "Late Uruk bricks were made of clay and chaff, shaped in wooden molds so that all were the same size. Building C, with walls 5 m high, needed more than a million of them. The narrow “Riemchen” brick (German for “little strap”) measures 16 × 6 × 6 cm; Arnold Nöldeke treated it as the mark of Level IVa, and the Narrow-Brick Building (Riemchengebäude) used about 261,000. Early Dynastic builders used plano-convex bricks, flat on one side and rounded on the other, which made building faster.\n\nThe Getty book calls fired brick “an extravagant building material in a region in which fuel is rare.” Many of the fired bricks of King Sin-kashid's palace were later carried off to be used again. The floors of the Deep Temples are baked brick set in asphalt.",
            Quote: "[...] the masonry is distinguished by unusual care and by the use of very beautiful Riemchen bricks, of a format (16x6x6 cm) [...]",
            QuoteAttribution: "Arnold Nöldeke, Eleventh Preliminary Report (Elfter vorläufiger Bericht, UVB XI), 1940, p. 16 (working translation)",
            Pronunciation: "Riemchengebäude REEM-khen-geh-BOY-deh; Arnold Nöldeke AR-nolt NURL-deh-keh",
            Sources: "Getty 2019 ch. 12, ch. 15, ch. 16 (Eichmann), ch. 35; Nöldeke 1940 (UVB XI) p. 16; HIGHLIGHTS (Narrow-Brick Building; Deep Temples); sheet 20 B9; sheet 21 Q52"),

        // Sheet 20 B10 (the cone-mosaic technique); sheet 21 Q41, Q42
        new TabletCard(
            Id: "culture-cone-mosaic",
            Kind: TabletKind.Culture,
            Title: "The Cone Mosaics (Stiftmosaik)",
            ArtKey: "object-cone-mosaic-panel",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Builders at Uruk make thousands of small clay cones and color their ends red, white or black. They press the cones into a thick layer of wet mud on the wall, making zigzags and diamond patterns. Uruk's are the only cone mosaics found still attached to their buildings.",
            LongText: "From about 3600 BCE, for nearly a thousand years, the most important buildings at Uruk had cone mosaics. The cones are 2 to 30 cm long. Their colors come from firing at different heats or from paint on the heads. The Getty book says that on mud-brick walls the cones were set into mud mortar 10 to 20 cm thick while the wall rose; one of the library's building files says they were pushed into a finished wall, so the experts do not fully agree.\n\nThe Pillar Hall had 235 niches with more than seventy patterns. Walter Andrae thought the patterns came from woven reed mats, and in winter 1930/31 the diggers found a reed mat still on a wall. Panels are in the Vorderasiatisches Museum in Berlin (such as VA 10997) and in the Iraq Museum in Baghdad.",
            Quote: "The colours of the mosaics are generally black, white, and red [...]",
            QuoteAttribution: "Ernst Heinrich, in Jordan 1932 (Third Preliminary Report), p. 14 (working translation)",
            Pronunciation: "Ernst Heinrich ERNST HYNE-rikh; Walter Andrae VAHL-ter ahn-DRAY-eh",
            Sources: "Getty 2019 ch. 17 (van Ess), figs. 17.1–17.6; ch. 16 (Eichmann), figs. 16.8–16.10; Jordan 1932 (UVB III) pp. 14–15; Stone-Cone Building and Round-Pillar Hall COMPREHENSIVE_BUILDING_OVERVIEW; sheet 20 B10 and open question 8; sheet 21 Q41, Q42"),

        // Sheet 20 B12 (the tripartite plan and niched façades); sheet 21 Q11
        new TabletCard(
            Id: "culture-tripartite-temple",
            Kind: TabletKind.Culture,
            Title: "The Three-Part Temple and Its Niches",
            ArtKey: "plan-limestone-temple",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Many temples at Uruk have a long middle hall with smaller rooms on each side. Their walls step in and out in deep niches that make stripes of light and shadow. This shape is copied for temples in Mesopotamia for thousands of years.",
            LongText: "The three-part (tripartite) plan goes back to the Ubaid period. It is the plan of the main Late Uruk buildings in Eanna, among them the Limestone Temple (Kalksteintempel) and the Stone-Cone Building. The library calls the Limestone Temple's plan the prototype of later Mesopotamian temples; its corners, not its sides, point to the four directions. Niched walls marked special buildings from the Uruk period until the end of the first millennium BCE.\n\nEanna's great buildings had no built-in cult fittings and were left “broom-clean,” so some scholars call them assembly halls. Gebhard Selz answers that this wrongly assumes a clean split between holy and everyday life.",
            Quote: "This ground-plan and the niching — rich on the outside, simpler in the courtyard — are as distinctive and new as the whole of Period V.",
            QuoteAttribution: "Julius Jordan, on the Limestone Temple, Third Preliminary Report, 1932, p. 17 (working translation)",
            Pronunciation: "Kalksteintempel KAHLK-shtyne-tem-pel",
            Sources: "Getty 2019 ch. 16 (Eichmann), ch. 37 (van Ess), ch. 39 (Selz) and note 20; Jordan 1932 (UVB III) p. 17; HIGHLIGHTS (Limestone Temple); sheet 20 B12; sheet 21 Q11"),

        // Sheet 20 B13 (the ziggurat form and the reed layers); sheet 21 Q78, Q80
        new TabletCard(
            Id: "culture-ziggurat",
            Kind: TabletKind.Culture,
            Title: "The Ziggurat and Its Reed Layers",
            ArtKey: "object-reed-layers",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "A ziggurat is a giant stepped tower of solid mud brick with a temple on top. Every meter or so, the builders laid a layer of reeds to keep it strong, and those reeds are still there after 4,000 years. King Ur-Nammu's tower for Holy Inanna set the pattern for the great shrines of southern Mesopotamia.",
            LongText: "In Sumerian the tower was called u₆-nir, and in Akkadian ziqqurratu. Ur-Nammu's lower terrace at Eanna measured 48 × 56 m and stood 11.2 m high, and there are no rooms inside. Small channels holding reed ropes probably kept the bricks from spreading while it was being built, and fired-brick shafts drained rainwater down its faces. Its base was deliberately made a little trapezoid, which Margarete van Ess links with the constellation Pegasus.\n\nNo trace of the top temple survives, but texts say the house of the deity stood there. The sources disagree about the outside: the Getty book says the faces at Uruk were plastered, while Jordan described a skin of baked brick set in bitumen at the base.",
            Quote: "Roughly every 1.2 to 1.4 meters, layers of bundled reeds were laid crosswise, one atop the other, between the mud-brick courses, presumably to control settling and to absorb residual moisture.",
            QuoteAttribution: "Margarete van Ess, Getty 2019 ch. 37",
            Pronunciation: "Ur-Nammu oor-NAH-moo; Eanna-Zikurrat ay-AHN-nah tsik-koo-RAHT",
            Sources: "Getty 2019 ch. 12; ch. 15, fig. 15.2; ch. 37 (van Ess), figs. 37.1–37.5; Ziggurat of Ur-Nammu COMPREHENSIVE_BUILDING_OVERVIEW sec. 4.1 (Jordan 1932 p. 31); sheet 20 B13 and open question 6; sheet 21 Q78, Q80"),

        // Sheet 23 C3 (A hymn to Ninkasi, ETCSL 4.23.1); sheet 20 B14 (beer, bread and the temple kitchen)
        new TabletCard(
            Id: "culture-beer-and-ninkasi",
            Kind: TabletKind.Culture,
            Title: "Beer and the Hymn to Ninkasi",
            ArtKey: "symbol-beer-jar-with-straws",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "A Sumerian hymn to Ninkasi, goddess of beer, is also a recipe. It tells how to bake beer-bread, soak the malt, cool the mash on reed mats and let it ferment in big jars. Holy Inanna's temple has its own kitchen and brewery.",
            LongText: "The hymn names Ninkasi's father as the god Enki and her mother as Ninti, queen of the abzu. Its steps run in order: mix the beer-bread with sweet spices in a pit, bake it in the big oven, water the malt, soak it in a jar, spread the cooked mash on reed mats to cool, brew the sweet wort with honey and wine added, set the fermenting vat on a collecting vat, and pour out the beer. The poem ends at line 48.\n\nTexts say the Ur III sanctuary at Eanna had a kitchen, a brewery and storerooms, and the diggers found round storage areas, ovens and butchered animal bones. Young reed shoots and roots were used in brewing too.",
            Quote: "It is you who pour out the filtered beer of the collector vat; it is like the onrush of the Tigris and the Euphrates.",
            QuoteAttribution: "A hymn to Ninkasi, ETCSL 4.23.1, lines 45–48",
            Pronunciation: "Enki EN-kee; abzu AHB-zoo; Euphrates yoo-FRAY-teez",
            Sources: "ETCSL 4.23.1, https://etcsl.orinst.ox.ac.uk/section4/tr4231.htm; Getty 2019 ch. 15, ch. 16 (Eichmann), ch. 37 (van Ess); sheet 20 B14; sheet 23 C3"),

        // Sheet 20 B15 (the priest-king and the EN title); sheet 23 C4
        new TabletCard(
            Id: "culture-priest-king",
            Kind: TabletKind.Culture,
            Title: "The Priest-King and the EN",
            ArtKey: "hero-priest-king-of-uruk",
            Cuneiform: "𒂗",
            CuneiformReading: "en: lord",
            CardText: "Uruk's early leader is called the EN, which means “lord.” He leads the processions that bring gifts to Holy Inanna. Carvings show him in a special headdress, feeding animals or standing in a boat.",
            LongText: "On the Uruk Vase a procession led by a priest-king brings harvest gifts to a priestess or goddess. One man holds a tray of cups, which is usually seen as the oldest form of the sign EN. Gebhard Selz says the EN title, used for priests, priestesses and the ruler, is tied above all to Uruk.\n\nStatuettes and seals show this “Great Man,” or “Priest-King,” in his typical headdress, feeding animals or in a boat before a stepped altar. Selz reads these pictures as ideals of how a ruler should act, not necessarily as real people or events. Among the divine powers Holy Inanna brings to Uruk in the poem “Inana and Enki” is the office of en priest.",
            Quote: "",
            QuoteAttribution: "",
            Pronunciation: "",
            Sources: "Getty 2019 ch. 11; ch. 20, figs. 20.4–20.5; ch. 39 (Selz), fig. 39.1; ETCSL 1.3.1, https://etcsl.orinst.ox.ac.uk/section1/tr131.htm; sheet 20 B15; sheet 21 B10; sheet 23 C4"),

        // Sheet 20 B16 (the harvest festival and nisannu), B17 (processions); sheet 21 Q88
        new TabletCard(
            Id: "culture-harvest-festival",
            Kind: TabletKind.Culture,
            Title: "The Harvest Festival and Nisannu",
            ArtKey: "symbol-barley-ear",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Uruk Vase shows people carrying the first crops of the harvest to the temple. The Sumerian word for these “first fruits” becomes the name of a festival and a month, Nisannu. In Arabic the month is still called Nīsān.",
            LongText: "Gebhard Selz reads the harvest scene on the vase as a first-fruits festival. He thinks such festivals came with eating and drinking, music and contests, “as in our own annual fairs.” Long processions linked the shrine with the city and the countryside, though most people were not allowed inside the sanctuaries, and courtyards and gateways were important places of ritual.\n\nMuch later, a building outside Uruk to the northeast was where the Babylonian New Year Festival of the spring equinox was celebrated.",
            Quote: "This delivery of 'first fruits,' Sumerian ne-sang, lived on in the Akkadian name of the festival and month, nīsannu (Arabic Nīsān [...]).",
            QuoteAttribution: "Gebhard J. Selz, Getty 2019 ch. 39",
            Pronunciation: "",
            Sources: "Getty 2019 ch. 12; ch. 39 (Selz); sheet 20 B16, B17; sheet 21 Q87, Q88"),

        // Sheet 20 B19 (the temple as a redistribution centre), B20 (schools for scribes)
        new TabletCard(
            Id: "culture-temple-economy",
            Kind: TabletKind.Culture,
            Title: "The Temple as Storehouse and School",
            ArtKey: "symbol-tablet-and-stylus",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Holy Inanna's temple is also like a giant warehouse. Farmers bring barley, wool and animals, scribes count everything, and the temple hands out food and goods to the people who work for it. Scribes are trained in a school inside the temple grounds.",
            LongText: "Officials in Uruk managed huge amounts of goods, including food from the countryside. The archaic texts record sheep, goats and cows, with fifteen signs for different kinds of sheep, and separate signs for hides, leather, wool and flax. In the Ur III sanctuary one courtyard seems to have handled supplies: drains met there, and farm products, perhaps even animals, could be kept nearby.\n\nThe Ur III sanctuary also held the temple's large staff of officials, a school for scribes and law courts, with workrooms and priests' rooms built into the courtyard walls. King Sin-kashid's Old Babylonian palace had a scribal school too, and its teaching texts show how scribes were trained. Priests of the 7th and 6th centuries BCE kept private libraries of clay tablets in their houses near Eanna.",
            Quote: "[...] the Eanna's undisputed role as a center of redistributive economic activity [...]",
            QuoteAttribution: "Gebhard J. Selz, Getty 2019 ch. 39",
            Pronunciation: "",
            Sources: "Getty 2019 ch. 1, ch. 12, ch. 14, ch. 37 (van Ess), ch. 39 (Selz), ch. 44; sheet 20 B19, B20"),

        // Sheet 23 C2 (the sexagesimal number system), A22 (the gods' numbers); sheet 19 A3
        new TabletCard(
            Id: "culture-counting-in-sixties",
            Kind: TabletKind.Culture,
            Title: "Counting in Sixties",
            ArtKey: "object-reed-stylus-and-tablet",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Sumerians count in sixties as well as in tens. Their earliest number signs are cup- and disk-shaped marks pressed into clay. Even the gods have numbers: the sky god is 60, Enlil 50, the moon god 30, the sun god 20, and Holy Inanna 15.",
            LongText: "J. Friberg describes the counting units as 1, 10, 60, 600 and so on. This way of counting may be older than writing, when the units were shown by clay tokens. After writing began, about 3300 BCE, the units were written with pressed-in signs, and other systems counted areas and amounts of grain. D. Melville notes that by 3000 BCE more than a dozen such systems are known, with no single base.\n\nPlace-value numbers in base 60, where the place of a sign changes its value, were invented around 2000 BCE and used for all the sums of Old Babylonian mathematics, about 1700 BCE. Scribes could write Anu's name as ᵈ60, Enlil's as ᵈ50 and Ishtar's as ᵈ15.",
            Quote: "The Mesopotamian system of sexagesimal counting numbers was based on the progressive series of units 1, 10, 1·60, 10·60, ....",
            QuoteAttribution: "J. Friberg, Archive for History of Exact Sciences (2019), abstract",
            Pronunciation: "",
            Sources: "J. Friberg 2019, https://research.chalmers.se/en/publication/508683; D. Melville, https://myslu.stlawu.edu/~dmel/mesomath/overview.html; AMGG An, Enlil, Nanna, Utu and Inana pages, http://oracc.museum.upenn.edu/amgg/listofdeities/an/; Getty 2019 ch. 9 (Zgoll); sheet 19 A3; sheet 23 A22, C2"),

        // Sheet 23 C1 (the lunar calendar); sheet 20 B18 (the twelve months)
        new TabletCard(
            Id: "culture-moon-calendar",
            Kind: TabletKind.Culture,
            Title: "The Moon Calendar and the Twelve Months",
            ArtKey: "symbol-crescent-moon",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The Sumerian month begins with the new moon, and from about 2500 BCE the year has twelve months. Many months are named for farm work and festivals. Now and then the scribes add an extra month to keep the moon months in step with the seasons.",
            LongText: "Already in the Uruk period, religious life followed the sun, the moon and the planets, and times for offerings were set by the calendar. Besides the yearly festivals, feasts tied to the phases of the moon were important.\n\nIn the Ur III state each city kept its own month names. Ur used the calendar of Girsu until the 30th year of King Šulgi and then its own. An extra month was marked with the word diri. M. Widell notes that the Ur III calendar of Uruk itself remains somewhat uncertain.",
            Quote: "Monthly, at the new moon, the gods of the Land gather around her",
            QuoteAttribution: "Inana and Iddin-Dagan (Iddin-Dagan A), ETCSL 2.5.3.1, lines 20–33",
            Pronunciation: "Shulgi SHOOL-gee",
            Sources: "Getty 2019 ch. 39 (Selz); ETCSL 2.5.3.1, https://etcsl.orinst.ox.ac.uk/section2/tr2531.htm; M. Widell, CDLJ 2004:2, https://cdli.earth/articles/cdlj/2004-2; M. Widell, CDLJ 2003:2, https://cdli.earth/articles/cdlj/2003-2; sheet 20 B18; sheet 23 C1"),

        // Sheet 23 C7 (The debate between Hoe and Plough, ETCSL 5.3.1); sheet 20 B2, B7
        new TabletCard(
            Id: "culture-hoe-and-plough",
            Kind: TabletKind.Culture,
            Title: "The Debate Between Hoe and Plough",
            ArtKey: "symbol-plow-seeder",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "In a funny Sumerian poem, the Hoe and the Plough argue about who is more useful. The Plough brags that the king himself drives it at harvest time. The Hoe says it works all twelve months and builds houses and canals too, and the god Enlil decides that the Hoe wins.",
            LongText: "The Plough boasts of its royal festival in the harvest month and of full storehouses. The Hoe answers that a plough needs a team of six oxen and four people, while the Hoe builds houses, walls, canals and gardens.\n\nFarming made the city possible. Uruk could not feed itself from the land close by and depended on the villages around it. When the rivers became less reliable in Early Dynastic times, people began to dig canals, the start of a great network. The Titles and Professions List already names an official in charge of the plow.",
            Quote: "My time of duty is twelve months, but your effective time is four months",
            QuoteAttribution: "The Hoe to the Plough, in The debate between Hoe and Plough, ETCSL 5.3.1, lines 104–108",
            Pronunciation: "Enlil EN-lil",
            Sources: "ETCSL 5.3.1, https://etcsl.orinst.ox.ac.uk/section5/tr531.htm; Getty 2019 ch. 14, ch. 15; sheet 20 B2, B7; sheet 23 C7"),

        // ---------------------------------------------------------------------------------------------------------
        // TIMELINE (15)
        // ---------------------------------------------------------------------------------------------------------

        // Sheet 20 A1, A2 (the Ubaid period and the reed platforms); sheet 21 Q36, Q76
        new TabletCard(
            Id: "timeline-ubaid",
            Kind: TabletKind.Timeline,
            Title: "The Ubaid Beginnings: a Village in the Reeds",
            ArtKey: "building-deep-trench",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "The first settlers at Uruk, in the Ubaid period, live in a marsh. They lay down thick layers of cut reeds to make dry ground and build their homes on top. Diggers found these reed floors almost 20 meters below Holy Inanna's later temples.",
            LongText: "The Getty table puts the Ubaid period in the sixth and fifth millennia BCE: the time of the first villages in southern Mesopotamia, with farming, herding and fishing. When people first settled at Uruk is given differently: about 5000 BCE in one Getty chapter, the end of the fifth millennium BCE in two others. Eanna Levels XVIII to XIII belong to this time.\n\nThe diggers cut a deep trench, about 85 square meters and more than 19.6 m deep, mainly in winters 1930/31 and 1931/32 (another source says it was begun in 1929). It held sixteen building layers covering about 800 years, with reed buildings at the bottom. Small hand-painted clay figures from Uruk date to the end of the fifth millennium BCE. No temple this early has been found in Eanna.",
            Quote: "In the swamp that they wished to settle, they made themselves dwelling-places by cutting reeds, laying them on the wet ground, and treading them down into a platform.",
            QuoteAttribution: "Ernst Heinrich, in Jordan 1932 (Third Preliminary Report), p. 19 (working translation)",
            Pronunciation: "Ernst Heinrich ERNST HYNE-rikh; Uruk OO-rook",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess); ch. 14, fig. 14.3; ch. 16 (Eichmann); ch. 60; Jordan 1932 (UVB III) pp. 18–19; sheet 20 A1, A2 and open questions 16, 17; sheet 21 Q36, Q76"),

        // Sheet 20 A3, A4, A5, B4 (the Uruk period and the Uruk expansion); sheet 21 Q46
        new TabletCard(
            Id: "timeline-uruk-expansion",
            Kind: TabletKind.Timeline,
            Title: "The Uruk Period and the Uruk Expansion",
            ArtKey: "object-necklace-lapis-carnelian",
            Cuneiform: "𒀕𒆠",
            CuneiformReading: "Unug: Uruk",
            CardText: "Around 4000 BCE Uruk is small, perhaps no more than 10 to 15 hectares. In the middle of the fourth millennium BCE the countryside fills with farming villages, and Uruk's people trade and settle far away to get wood, stone and metal. Their cylinder seals have been found as far away as Egypt.",
            LongText: "The Getty table gives the Uruk period as about 4000–3300 BCE, and its Late phase as about 3500–3300 BCE (Eanna Levels VI–IV). Early on, only about ten settlements stood in the countryside north-northeast of the city. Then their number grew tenfold, probably because a slightly drier climate shrank the swamps and opened up land.\n\nThe river plain had mud and reeds but no wood, stone or metal. The roof beams of Eanna's great halls were pine from the mountains of the Levant and Turkey. Copper came from Oman and Anatolia, lapis lazuli from Afghanistan, and carnelian, diorite, basalt and obsidian came too. In Egypt the first proto-hieroglyphs appeared at about the same time as Uruk's seals.\n\nIn the Late Uruk period a wall enclosed about 9 hectares of Eanna, filled with huge buildings such as the Limestone Building, 76 by 30 m.",
            Quote: "[...] the copper from Oman on the Persian Gulf, the gold perhaps already from Egypt, and various precious stones such as lapis lazuli from the lands far beyond the Persian mountains.",
            QuoteAttribution: "Ernst Heinrich, Kleinfunde aus den archaischen Tempelschichten in Uruk, 1936 (working translation)",
            Pronunciation: "Uruk OO-rook; Warka WAR-kah",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 16 (Eichmann); ch. 45, fig. 45.3; Heinrich 1936 (Kleinfunde); sheet 20 A3, A4, A5, B4; sheet 21 Q46, B02"),

        // Sheet 20 B5 (the twin settlements Kullab and Eanna), A19; sheet 21 B01, B03, Q86
        new TabletCard(
            Id: "timeline-kullab-and-eanna",
            Kind: TabletKind.Timeline,
            Title: "Twin Villages: Kullab and Eanna",
            ArtKey: "symbol-kullab-and-eanna",
            Cuneiform: "𒂍𒀭𒈾",
            CuneiformReading: "E₂-an-na: House of Heaven (Eanna)",
            CardText: "Uruk probably begins as two villages with the Euphrates River flowing between them. One later becomes Kullab, the district of the sky god An. The other becomes Eanna, the home of Holy Inanna. Before the end of the Late Uruk period the river has moved and the two have grown into one city.",
            LongText: "The city center has two public areas: the Anu Ziggurat district in the west and the Eanna precinct in the east. By the end of the Uruk period, Eanna stood about 8 m higher than the Anu district, because so much more had been built there; four to five hundred years before, the two had been at about the same height.\n\nThe name Kullab is known from the 26th century BCE and is usually linked with the Anu area, while Anu's name is certain there only from the third century BCE. The name Eanna, “House of Heaven,” can be tied to the precinct with certainty only from Ur-Nammu's stamped bricks. Uruk is one of the few cities with two patron gods, An and Inanna, and for many centuries She is the more important.",
            Quote: "In any case, Uruk—Kullab and Eanna—set the pattern for all later temple forms [...]",
            QuoteAttribution: "Gebhard J. Selz, Getty 2019 ch. 39",
            Pronunciation: "Kullaba KOOL-lah-bah; Eanna ay-AHN-nah; Euphrates yoo-FRAY-teez",
            Sources: "Getty 2019 ch. 1, ch. 14, ch. 16 (Eichmann), ch. 37 (van Ess), ch. 39 (Selz); sheet 20 A3, A19, B5; sheet 21 B01, B03, Q86"),

        // Sheet 20 A5, A6 (the razing of Uruk IV and the Jemdet Nasr terrace); sheet 21 Q77
        new TabletCard(
            Id: "timeline-jemdet-nasr",
            Kind: TabletKind.Timeline,
            Title: "Jemdet Nasr: Eanna Torn Down and Rebuilt",
            ArtKey: "plan-eanna-level-iv",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Around 3300 BCE the giant buildings of Eanna's Level IV are torn down and buried under debris, for reasons no one knows. The ground is leveled, and builders raise a high terrace for a temple. Over many centuries this terrace grows into Holy Inanna's ziggurat.",
            LongText: "The Getty table gives the Jemdet Nasr period as about 3300–3000 BCE (Eanna Level III). The sources date the razing differently: around 3300 BCE in one chapter, probably between 3300 and 3200 BCE in another, and Gebhard Selz puts the redesign at about 3000 BCE. It was a radical redesign of the city center, and the new layout lasted through the whole third millennium BCE.\n\nIn place of many great buildings came one central high terrace. A terrace of about 23 by 19 m, with twelve half-round pillars on one wall, was enlarged into an L-shape of about 47 by 45 m, with cone-mosaic panels on its walls. There was a maze of tiny rooms the diggers called the Labyrinth, a Temple Kitchen with oval cooking pits, and fired-brick tanks and channels that lifted water at least 10 m. The Red Temple (Roter Tempel), with its single-color red mosaic walls, belongs to this time.",
            Quote: "The latest level from this period, the Uruk IV layer, was razed and intentionally covered with debris, for reasons unknown to us, probably between 3300 and 3200 BC [...]",
            QuoteAttribution: "Margarete van Ess, Getty 2019 ch. 12, p. 80",
            Pronunciation: "Roter Tempel ROH-ter TEM-pel",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess) p. 80; ch. 16 (Eichmann); ch. 39 (Selz); HIGHLIGHTS (Red Temple); sheet 20 A5, A6 and open question 2; sheet 21 Q77"),

        // Sheet 20 A7, B6 (Early Dynastic Uruk and the city wall); sheet 23 B16 (the Sumerian King List, Uruk I)
        new TabletCard(
            Id: "timeline-early-dynastic",
            Kind: TabletKind.Timeline,
            Title: "Early Dynastic Uruk and the King List",
            ArtKey: "building-city-wall",
            Cuneiform: "𒈗",
            CuneiformReading: "lugal: king",
            CardText: "After 3000 BCE Uruk is at its biggest, inside a wall about 9 kilometers long with some 900 towers. The Sumerian King List says the kingship came to Eanna, and it gives Uruk's legendary first kings amazing reigns: Enmerkar 420 years, Lugalbanda 1,200 and Gilgamesh 126.",
            LongText: "The Getty table gives the Early Dynastic period as about 3000–2340 BCE. The wall, 5 to 9 m thick, was built at its start; one Getty chapter makes it nearly 10 km long. Between 30,000 and 80,000 people may have lived inside it. In Eanna a nearly square terrace about 52 by 49 m, built of plano-convex bricks, stood in a courtyard of more than 3,600 square meters. Real inscriptions name kings of Uruk such as Lugalkinishedudu, about 2430 BCE.\n\nIn the King List's first dynasty of Uruk, Meš-ki-aĝ-gašer, son of Utu, rules 324 years; Enmerkar 420; Lugalbanda the shepherd 1,200; Dumuzid the fisherman 100; Gilgamesh 126; and his son Ur-Nungal 30. Twelve kings rule 2,310 years in all, though one copy says 3,588.",
            Quote: "Gilgameš, whose father was a phantom (?), the lord of Kulaba, ruled for 126 years.",
            QuoteAttribution: "The Sumerian King List, ETCSL 2.1.1, lines 95–133",
            Pronunciation: "Gilgamesh GIL-gah-mesh; Enmerkar en-MEHR-kar; Lugalbanda loo-gahl-BAHN-dah",
            Sources: "Getty 2019 Chronological Table; ch. 12; ch. 14; ch. 16 (Eichmann); ch. 35; ETCSL 2.1.1, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; sheet 20 A7, B6 and open question 12; sheet 23 B16"),

        // Sheet 20 A8, B24 (Lugalzagesi, Sargon and the cult moved to Akkad); sheet 23 B16
        new TabletCard(
            Id: "timeline-lugalzagesi-and-sargon",
            Kind: TabletKind.Timeline,
            Title: "Lugalzagesi, Sargon and the Cult Moved to Akkad",
            ArtKey: "hero-sargon",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "King Lugalzagesi of Uruk conquers many Sumerian cities. Then Sargon of Akkad defeats him and builds the first empire in Mesopotamia, about 2340 BCE. Sargon moves the worship of Holy Inanna to his own capital, and Uruk becomes less important.",
            LongText: "Lugalzagesi began as ensi, city prince, of Umma, and after victories over Lagash and Ur made himself king of Uruk; the King List gives him 25 years (one copy, 34). A huge packed-clay foundation beside Eanna, more than 125 by 80 m, may have been meant for his palace: the Rammed-Earth Building (Stampflehmgebäude). No floors or walls survive, so it may never have been finished, and scholars date it very differently.\n\nThe empire of Akkad lasted five generations; the Getty table gives about 2340–2200 BCE, and one chapter ends it about 2180 BCE. Moving the cult of the city Goddess to Akkad was “doubtless” the main reason Uruk lost importance. Uruk joined a revolt and was defeated, but it stayed a large city. Its buildings of this time have not yet been dug.",
            Quote: "",
            QuoteAttribution: "",
            Pronunciation: "Sargon SAR-gon; Akkad AH-kkad; Stampflehmgebäude SHTAMPF-lehm-geh-BOY-deh",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 16 (Eichmann), fig. 16.16; ch. 35; ETCSL 2.1.1 ll. 259–265, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; sheet 20 A7, A8, B24 and open questions 3, 13; sheet 23 B16"),

        // Sheet 20 B25 (Utu-hegal and the Gutians); sheet 23 B16 (his reign in the King List)
        new TabletCard(
            Id: "timeline-utu-hegal",
            Kind: TabletKind.Timeline,
            Title: "Utu-hegal Drives Out the Gutians",
            ArtKey: "period-akkadian",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "When the empire of Akkad breaks apart, foreigners called the Gutians hold power in the land. King Utu-hegal of Uruk joins with other cities and drives them out. Soon after, Ur-Nammu, a man from Uruk, becomes king of all Sumer.",
            LongText: "As the state of Akkad broke apart, states led by cities came back, and Uruk played an important role among them. Its ruler Utu-hegal (the Getty book spells it Utu-khegal) formed a coalition with other cities and drove out the Gutians. In the Sumerian King List he comes right after the Gutian dynasty. The main text gives him 427 years, but one copy gives only 7 years, 6 months and 15 days.\n\nOn that basis Ur-Nammu of Ur, who was “actually from Uruk,” built the state of the Third Dynasty of Ur, which lasted five generations. Utu-hegal is not the same as the sun god Utu.",
            Quote: "",
            QuoteAttribution: "",
            Pronunciation: "Ur-Nammu oor-NAH-moo; Utu OO-too",
            Sources: "Getty 2019 ch. 14 ('City-States and Territorial States'); ETCSL 2.1.1 ll. 335–340, https://etcsl.orinst.ox.ac.uk/section2/tr211.htm; sheet 19 B5; sheet 20 A9, B25; sheet 23 B16"),

        // Sheet 20 A9 (Ur-Nammu and the Third Dynasty of Ur); sheet 19 A18, B8; sheet 21 Q02; sheet 23 B19
        new TabletCard(
            Id: "timeline-ur-nammu",
            Kind: TabletKind.Timeline,
            Title: "Ur-Nammu and the Third Dynasty of Ur",
            ArtKey: "hero-ur-nammu",
            Cuneiform: "𒈗",
            CuneiformReading: "lugal: king, as in Ur-Nammu's title lugal-uri₅ki-ma, 'king of Ur'",
            CardText: "Around 2110 BCE King Ur-Nammu, who came from Uruk, clears away the old buildings of Eanna and builds Holy Inanna a giant ziggurat. He is the first king to stamp its fired bricks with an inscription, and he puts Her name before his own. Its mud-brick core still stands at Uruk today.",
            LongText: "The Getty table gives the Third Dynasty of Ur as about 2112–2004 BCE, with the kings Ur-Nammu, Šulgi, Amar-Sin and Shu-Sin. Ur-Nammu's new sanctuary covered nearly 6 hectares and was the largest structure in the city. Only from his stamped bricks can the name Eanna be tied to the precinct with certainty. His son Šulgi continued the work and built a small chapel for Nimintabba, a goddess of Holy Inanna's household.\n\nThe later kings left their mark too: Amar-Sin left inscribed bricks and a door pivot stone, later reused in a Neo-Babylonian gateway, and a necklace of Tiamatbashti, wife of King Shu-Sin, was found at Uruk. Sumerian poems call Ur-Nammu the brother of Gilgamesh and say he became a judge in the land of the dead.",
            Quote: "For Holy Inanna, Lady of Eanna, his lady — Ur-Nammu, the mighty man, king of Ur, king of Sumer and Akkad, Her house he built for Her, and to its place he restored it.",
            QuoteAttribution: "Ur-Nammu's building inscription on stamped bricks and cones from Uruk (cf. RIME 3/2.1.1)",
            Pronunciation: "Ur-Nammu oor-NAH-moo; Shulgi SHOOL-gee; Amar-Sîn ah-MAR SEEN",
            Sources: "Getty 2019 Chronological Table; ch. 14; ch. 35, fig. 35.3; ch. 37 (van Ess), figs. 37.3, 37.7; Ziggurat of Ur-Nammu COMPREHENSIVE_BUILDING_OVERVIEW secs. 3–5; ETCSL 2.4.1.1, https://etcsl.orinst.ox.ac.uk/section2/tr2411.htm; ETCSL 2.4.1.3, https://etcsl.orinst.ox.ac.uk/section2/tr2413.htm; sheet 19 A18, B8; sheet 20 A9; sheet 21 Q02, B12; sheet 23 B19"),

        // Sheet 20 A10 (Sin-kashid and the Old Babylonian palace), B26 (Hammurabi)
        new TabletCard(
            Id: "timeline-sin-kashid",
            Kind: TabletKind.Timeline,
            Title: "Sin-kashid and the Old Babylonian Palace",
            ArtKey: "period-old-babylonian",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Near the end of the 19th century BCE, King Sin-kashid rules Uruk from a great palace of baked brick. He probably repairs the walls and courtyards around Holy Inanna's ziggurat. After this age, Uruk is almost empty for a few hundred years.",
            LongText: "The Getty table gives the Old Babylonian period as about 2025–1595 BCE, and notes that about a hundred years of its dating are disputed. Kings of the early second millennium BCE boasted of caring for the shrine, though archaeology has not proved it. Sin-kashid probably renovated Eanna's enclosure wall and courtyards on a large scale.\n\nHis palace stood at the western edge of the city. It was built of costly fired brick, held archives and a school for scribes, and burned down; it was dug in 1958–1962. In the 18th century BCE Hammurabi of Babylon united the region, and cities like Uruk no longer ruled themselves.",
            Quote: "",
            QuoteAttribution: "",
            Pronunciation: "",
            Sources: "Getty 2019 Chronological Table; ch. 1 ('The Palace'); ch. 12; ch. 14; ch. 35, fig. 35.4; ch. 37; sheet 20 A10, B26"),

        // Sheet 20 A11 (the Kassites, Karaindash, Kurigalzu), B21; sheet 19 B20; sheet 21 Q28
        new TabletCard(
            Id: "timeline-kassites",
            Kind: TabletKind.Timeline,
            Title: "The Kassites and Karaindash",
            ArtKey: "building-karaindash-temple",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "After Uruk lies nearly empty for a few hundred years, the Kassite kings renovate Holy Inanna's precinct. Around 1420 BCE King Karaindash builds Her a small temple with gods and goddesses of molded brick on its wall, each pouring water from a jar. Its dedication is written in Sumerian, a language no longer spoken.",
            LongText: "The Getty table gives the Kassite, or Middle Babylonian, period as about 1650–1157 BCE. The sources differ a little on the date: the Getty book puts the temple at the end of the 15th century BCE and says Kassite renovation began in the 14th century; the library says about 1420 BCE. The temple's outer wall, about 205 cm high, was made of about 500 molded baked bricks. Its eleven-line dedication in old-style Sumerian names Innin as Lady of Eanna. King Kurigalzu left stamped pavement bricks at the east gate of the ziggurat precinct.\n\nJulius Jordan found the temple in the 1928/29 season. A rebuilt section of the façade stands in the Vorderasiatisches Museum in Berlin, and other pieces are in the Iraq Museum in Baghdad.",
            Quote: "That Eanna in the second millennium BCE extended far to the north-east — this the Kassite Innin-Temple of Karaindash had already taught us.",
            QuoteAttribution: "Julius Jordan, Third Preliminary Report, 1932, p. 6 (working translation)",
            Pronunciation: "Karaindash kah-rah-IN-dash; Kurigalzu koo-ree-GAHL-zoo; Innin-Tempel IN-nin TEM-pel",
            Sources: "Getty 2019 Chronological Table; ch. 1, fig. 1.7; ch. 35, fig. 35.5; ch. 37; HIGHLIGHTS (Karaindash's Temple); Jordan 1932 (UVB III) p. 6; sheet 19 A18, B20; sheet 20 A11, B21 and open question 15; sheet 21 Q28"),

        // Sheet 20 A12 (the Second Dynasty of Isin and the Assyrian kings; the lower temples); sheet 21 Q44
        new TabletCard(
            Id: "timeline-assyrian-kings",
            Kind: TabletKind.Timeline,
            Title: "The Assyrian Kings and the Lower Temples",
            ArtKey: "building-deep-temples",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Around 720 BCE the Babylonian king Marduk-apla-iddina II and the Assyrian king Sargon II both rebuild Holy Inanna's sanctuary from the ground up. They make the courtyards bigger and put a new outer wall around the ziggurat. The stairs you can still see on the ziggurat today come from this time.",
            LongText: "The Getty table names the years 1125–625 BCE after the Second Dynasty of Isin, when Sealand dynasties and kings from Assyria took turns ruling the south. Marduk-apla-iddina II reigned 721–710 BCE and Sargon II 722–705 BCE. Sargon II built an outer enclosure wall, the Sargon-Zingel, farther out than Ur-Nammu's.\n\nThe Getty book counts the two small “lower temples” beside the ziggurat's central stair as the most important change of this time. The library's sources date the same pair, the Deep Temples (Tieftempel), to the Neo-Babylonian period, most likely to King Nabonidus, and Jordan thought Marduk-apla-iddina was only a possibility. Both sides agree the temples were in use under Cyrus.",
            Quote: "The bricks were of various formats and among them were some bearing the stamps of Amar-Sîn, of Urnammu, of Karaindash, and of Sargon.",
            QuoteAttribution: "Ernst Heinrich, in Jordan 1932 (Third Preliminary Report), p. 23 (working translation)",
            Pronunciation: "Marduk-apla-iddina II MAR-dook AH-plah ih-DEE-nah; Sargon SAR-gon; Tieftempel TEEF-tem-pel",
            Sources: "Getty 2019 Chronological Table; ch. 35, fig. 35.1; ch. 37 (van Ess), figs. 37.2, 37.4; Ziggurat of Ur-Nammu COMPREHENSIVE_BUILDING_OVERVIEW secs. 3, 6 (Jordan 1932 pp. 29, 33); story/16_scholarship_corrections.txt #1 (Lenzen, UVB X); sheet 20 A12 and open question 5; sheet 21 Q44"),

        // Sheet 20 A13, A14 (Nebuchadnezzar, Nabonidus, Cyrus); sheet 21 Q17, Q18, Q19
        new TabletCard(
            Id: "timeline-neo-babylonian",
            Kind: TabletKind.Timeline,
            Title: "Nebuchadnezzar, Nabonidus and Cyrus",
            ArtKey: "object-lion-figurine",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Around 600 BCE King Nebuchadnezzar II rebuilds the great wall around Holy Inanna's sanctuary. Two small temples sit in the corners of the ziggurat's stairs. In 539 BCE the Persian king Cyrus the Great conquers Babylon, and even then Her small temples are still in use.",
            LongText: "The Getty table gives the Neo-Babylonian period as 625–539 BCE, with the kings Nabopolassar, Nebuchadnezzar II (604–562 BCE) and Nabonidus. Jordan credited Nabopolassar with a thorough destruction; Nebuchadnezzar II then rebuilt the outer wall on Sargon's line, and his stamped bricks are everywhere in its upper courses. Eanna's priests lived southwest of the sanctuary, with tablet libraries in their houses and burials under the floors.\n\nThe Deep Temples were most likely built under Nabonidus, with floors of baked brick set in asphalt. Under the floor before the East Temple's cult niche, Jordan's team found a little unbaked clay lion, which went to the museum in Baghdad. Against the East Temple's wall lay a brick pavement stamped with Cyrus's four-line inscription, proof that the temples were still used in the Achaemenid period (538–332 BCE).",
            Quote: "It could be proved without doubt, and only in the last weeks of excavation, that the temples — and therefore probably the ziggurat stairways too — were in use down into the time of Cyrus II.",
            QuoteAttribution: "Julius Jordan, Third Preliminary Report, 1932, p. 33 (working translation)",
            Pronunciation: "Nebuchadnezzar II neh-boo-kahd-NEZ-zar; Nabonidus nah-boh-NYE-dus; Nabopolassar nah-boh-poh-LAS-sar; Cyrus II SYE-rus",
            Sources: "Getty 2019 Chronological Table; ch. 12; ch. 37; Jordan 1932 (UVB III) pp. 6, 33–34; HIGHLIGHTS (Deep Temples); story/16_scholarship_corrections.txt #1; sheet 20 A13, A14; sheet 21 Q17, Q18, Q19"),

        // Sheet 20 A15 (the Seleucid renaissance), B21; sheet 19 A16, B1; sheet 23 A21 (Irigal or Ešgal)
        new TabletCard(
            Id: "timeline-seleucid",
            Kind: TabletKind.Timeline,
            Title: "The Seleucid Renaissance: the Resh and the Eshgal",
            ArtKey: "building-resh-sanctuary",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "After Alexander the Great, the Seleucid kings rule Uruk, and the old gods have a last great age. Huge new temples of baked brick rise: the Resh for An and his wife Antum, and the Eshgal for Ishtar and Nanaya. Priests even study Sumerian again, long after anyone spoke it.",
            LongText: "The Getty table gives the Seleucid period as 332–141 BCE. Two local governors who built for the Seleucid kings were both named Anu-uballit, with Greek second names: Nikarchos (243 BCE) and Kephalon (201 BCE); the Getty table calls them kings. The Resh and Eshgal sanctuaries, built of fired brick, cover roughly 200 by 250 m, and Eanna now played only a small part. The Eshgal still stands up to 8 m high. Its name is read two ways: the Getty book says Eshgal, while the German Archaeological Institute's Uruk visualisation project calls it the Irigal, home of the goddess Ishtar.\n\nPriests copied religious, literary and scientific texts, and word lists show systematic schooling in Sumerian. Jordan's very first season, 1912/13, dug in the Resh, and the Eshgal was dug in winter 1932/33 at Walter Andrae's express wish.",
            Quote: "[...] much like Latin in our own time [...]",
            QuoteAttribution: "Getty 2019 ch. 35, on Sumerian as a language of worship in Seleucid Uruk",
            Pronunciation: "An AHN; Anu AH-noo; Antu AHN-too; Nanaya nah-NAH-yah",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess); ch. 35, fig. 35.6; ch. 37; Artefacts Berlin, Uruk Visualisation Project, https://www.artefacts-berlin.de/?p=1494; sheet 19 A16, B1; sheet 20 A15, B21 and open question 18; sheet 23 A21 and open question 9"),

        // Sheet 20 A16, A17, A18 (Parthians, Sassanians, the abandonment, al-Warka); sheet 19 B21; sheet 21 Q74
        new TabletCard(
            Id: "timeline-abandonment",
            Kind: TabletKind.Timeline,
            Title: "Parthians, Sassanians and the Last of Uruk",
            ArtKey: "building-tell-of-warka",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Around 140 BCE the Parthians from Iran take over Uruk. People turn the old temples into homes, and Holy Inanna's district slowly stops being used for worship. After about 4,500 years of life, the last people leave the city in the early 300s CE.",
            LongText: "The Getty table gives the Parthian period as 141 BCE to 224 CE and the Sassanian as 224–634 CE, found at Uruk only up to the fourth century CE. Parthian coins give a close timeline from 120 BCE to 200 CE. Some Seleucid temples became homes, using their outer walls for defence, and the Eshgal's rooms were made smaller for everyday use. A temple to an otherwise unknown god, Gareus, was built in the south of the city; a Greek inscription found nearby dates to 111 CE. The Eanna district stayed a center of worship into the first century CE.\n\nPeople did not leave the region: many mounds southeast of Uruk hold finds of the 4th to 7th centuries CE. In 634 CE Arab horsemen won a battle at al-Warka, and that name lives on in the name of the ruins, Warka. The Bible calls the city Erech.",
            Quote: "The city of Uruk was occupied for roughly forty-five hundred years.",
            QuoteAttribution: "Margarete van Ess, Getty 2019 ch. 12, p. 76",
            Pronunciation: "Warka WAR-kah; al-Warka al-WAR-kah; Erech EH-rekh",
            Sources: "Getty 2019 Chronological Table; ch. 12 (van Ess) p. 76, fig. 12.2; ch. 35, figs. 35.6, 35.8; sheet 19 B21; sheet 20 A16, A17, A18 and open question 19; sheet 21 Q74, B02"),

        // Sheet 20 A19 (the shift of the Euphrates), A17, B7; sheet 21 Q75
        new TabletCard(
            Id: "timeline-euphrates-moves-west",
            Kind: TabletKind.Timeline,
            Title: "The River Moves West",
            ArtKey: "period-euphrates-shifts",
            Cuneiform: "",
            CuneiformReading: "",
            CardText: "Uruk lives because of the Euphrates River. Over many centuries the river moves away to the west and the canals are no longer kept up, so the fields dry out. Because nobody builds over the empty city, much of it survives for the archaeologists.",
            LongText: "In Uruk's earliest days the Euphrates ran north to south through the later city, between Kullab and Eanna; by the end of the Late Uruk period that riverbed had gone. From the first millennium BCE on, the river shifted step by step farther west, and in the long run this mattered more than any battle. Around 77 CE Pliny the Elder wrote that the watercourses had changed and that the people of Orchoë, the Greek name for Uruk, had moved into the countryside.\n\nAfter the Sassanian period the great canals that had made farming possible for nearly four thousand years were not properly kept up. Farming moved north and west, and the land between the rivers became pasture for nomadic herders. Today illegal digging and modern building threaten the ruins.",
            Quote: "This was a stroke of luck for archaeology, for it meant that the region was not built over.",
            QuoteAttribution: "Margarete van Ess, Getty 2019 ch. 12, p. 76",
            Pronunciation: "Euphrates yoo-FRAY-teez; Warka WAR-kah",
            Sources: "Getty 2019 ch. 12 (van Ess) p. 76; ch. 14; ch. 15 (van Ess and Neef); sheet 20 A17, A19, B7; sheet 21 Q75, B02"),
    };
}
