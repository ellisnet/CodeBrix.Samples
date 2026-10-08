using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

public static partial class Catalog
{
    private static readonly FavorCard[] FavorCards =
    {
        // Sheet 19 Part A, entry A1 (Her name, Mistress of Heaven)
        new FavorCard(
            Id: "favor-mistress-of-heaven",
            Title: "Mistress of Heaven",
            Effect: FavorEffect.ExtraDieNextTurn,
            EffectText: "Next turn, roll a third die and keep the best two.",
            Fact: "Even in ancient times, the name Inanna was understood as short for Nin-an-ak, “Mistress of Heaven.”",
            ArtKey: "deity-inanna",
            Sources: "Getty 2019 ch. 9 (Zgoll); ch. 39 (Selz); sheet 19 entry A1"),

        // Sheet 19 Part A, entry A2 (the reed bundle)
        new FavorCard(
            Id: "favor-the-reed-bundle",
            Title: "The Reed Bundle",
            Effect: FavorEffect.FreeTablet,
            EffectText: "Draw a Tablet now.",
            Fact: "Holy Inanna's name is written with a sign that began as a bundle of reeds, found on the very earliest clay tablets of Uruk.",
            ArtKey: "symbol-reed-bundle-gatepost",
            Sources: "Getty 2019 ch. 11, figs. 11.2, 11.3; ch. 39 (Selz); sheet 19 entry A2"),

        // Sheet 19 Part A, entry A3 (Venus and the number 15)
        new FavorCard(
            Id: "favor-the-number-fifteen",
            Title: "Half of Thirty",
            Effect: FavorEffect.FreeWorker,
            EffectText: "Gain a Worker now.",
            Fact: "Holy Inanna is seen in the planet Venus. Her father the moon god has the number 30, and She has half of it, 15.",
            ArtKey: "deity-inanna-venus",
            Sources: "Getty 2019 ch. 9 (Zgoll), “Astral Manifestation”; sheet 19 entry A3"),

        // Sheet 19 Part A, entry A4 (the rising, setting and overhead Inanna)
        new FavorCard(
            Id: "favor-the-rising-inanna",
            Title: "The Rising Inanna",
            Effect: FavorEffect.FreeSurvey,
            EffectText: "Survey now, free.",
            Fact: "The oldest texts from Eanna name a rising Inanna, a setting Inanna and an Inanna directly above, as Venus moves through the sky.",
            ArtKey: "symbol-venus-morning",
            Sources: "Getty 2019 ch. 39 (Selz) p. 215, citing Szarzyńska 1993 and Steinkeller 2002; sheet 19 entry A4"),

        // Sheet 19 Part A, entry A6 (war and the lion)
        new FavorCard(
            Id: "favor-the-lion",
            Title: "She Rides the Lion",
            Effect: FavorEffect.PlusTwoNextDig,
            EffectText: "+2 on your next dig, this turn or next.",
            Fact: "As a Goddess of battle, Holy Inanna carries maces and stands upon a lion.",
            ArtKey: "deity-inanna-warrior",
            Sources: "Getty 2019 ch. 9 (Zgoll), “Warfare and the Protection of Life,” fig. 9.5; sheet 19 entry A6"),

        // Sheet 19 Part A, entry A7 (the Uruk Vase)
        new FavorCard(
            Id: "favor-the-uruk-vase",
            Title: "Gifts at Her Door",
            Effect: FavorEffect.RecruitDiscount,
            EffectText: "Recruit from the Expedition Row now, at one less.",
            Fact: "The Uruk Vase is read from the bottom up: water, grain, sheep, men carrying gifts, and at the top the Goddess at Her door.",
            ArtKey: "symbol-flowing-vase",
            Sources: "Getty 2019 ch. 9 (Zgoll), figs. 9.1, 9.9; ch. 39 (Selz); sheet 19 entry A7"),

        // Sheet 19 Part A, entry A8 (the Lady of Warka)
        new FavorCard(
            Id: "favor-the-lady-of-warka",
            Title: "The Lady of Warka",
            Effect: FavorEffect.OnePoint,
            EffectText: "Score one point now.",
            Fact: "The Lady of Warka, a life-size stone face more than 5,000 years old, is suspected to show Holy Inanna.",
            ArtKey: "hero-lady-of-uruk",
            Sources: "Getty 2019 ch. 11, fig. 11.1; ch. 14; ch. 39 (Selz); sheet 19 entry A8"),

        // Sheet 19 Part A, entry A9 (the Descent to the Netherworld)
        new FavorCard(
            Id: "favor-the-descent",
            Title: "The Descent",
            Effect: FavorEffect.NoHandLimit,
            EffectText: "The hand limit does not apply at the end of this turn.",
            Fact: "In an old Sumerian story, Holy Inanna goes down to the land of the dead, and some scholars link Her return to Venus shining again.",
            ArtKey: "deity-inanna-descending",
            Sources: "Getty 2019 ch. 9 (Zgoll); sheet 19 entry A9"),

        // Sheet 19 Part A, entry A10 (the divine powers, me)
        new FavorCard(
            Id: "favor-the-divine-powers",
            Title: "The Divine Powers",
            Effect: FavorEffect.ExtraDieNextTurn,
            EffectText: "Next turn, roll a third die and keep the best two.",
            Fact: "The story “Inanna and Enki” tells of the divine powers, the me, that Holy Inanna brings home to Her city.",
            ArtKey: "symbol-me-tablet",
            Sources: "Getty 2019 ch. 9 (Zgoll), “The Goddess and Her Name”; sheet 19 entry A10"),

        // Sheet 19 Part A, entry A11 (the House of Heaven)
        new FavorCard(
            Id: "favor-house-of-heaven",
            Title: "House of Heaven",
            Effect: FavorEffect.FreeTablet,
            EffectText: "Draw a Tablet now.",
            Fact: "Her temple is Eanna, “House of Heaven,” and a myth says brave Inanna carries the model of a heavenly house down to earth.",
            ArtKey: "symbol-ziggurat-glyph",
            Sources: "Getty 2019 ch. 9 (Zgoll); ch. 39 (Selz); sheet 19 entry A11"),

        // Sheet 19 Part A, entry A12 (the Sacred Marriage and Dumuzi)
        new FavorCard(
            Id: "favor-the-shepherd",
            Title: "The Shepherd's Bride",
            Effect: FavorEffect.FreeWorker,
            EffectText: "Gain a Worker now.",
            Fact: "Holy Inanna's partner is Dumuzi, a shepherd god, whose part the king takes in a great festival; scholars still argue about how it was held.",
            ArtKey: "deity-dumuzi",
            Sources: "Getty 2019 ch. 9 (Zgoll), “Rituals for the (City-)State”; ch. 39 (Selz); sheet 19 entry A12"),

        // Sheet 19 Part A, entry A13 (Enheduanna)
        new FavorCard(
            Id: "favor-enheduanna",
            Title: "The First Author",
            Effect: FavorEffect.FreeSurvey,
            EffectText: "Survey now, free.",
            Fact: "Enheduanna, daughter of King Sargon and high priestess of the moon god at Ur, wrote a hymn to Holy Inanna in the 23rd century BCE; she is the first author in the world known by name.",
            ArtKey: "hero-enheduanna",
            Sources: "Getty 2019 ch. 9 (Zgoll), fig. 9.4, note 6; sheet 19 entry A13"),

        // Sheet 19 Part A, entry A14 (Her cult beyond Uruk)
        new FavorCard(
            Id: "favor-everywhere-honored",
            Title: "Honored Everywhere",
            Effect: FavorEffect.PlusTwoNextDig,
            EffectText: "+2 on your next dig, this turn or next.",
            Fact: "A list of holy places in Babylon names 180 shrines to Ishtar, beside 43 cult centres of the great gods.",
            ArtKey: "symbol-eight-pointed-star",
            Sources: "Getty 2019 ch. 9 (Zgoll), “Global Authority”; sheet 19 entry A14"),

        // Sheet 19 Part A, entry A15 (raised hands on the rooftops)
        new FavorCard(
            Id: "favor-raised-hands",
            Title: "Raised Hands",
            Effect: FavorEffect.RecruitDiscount,
            EffectText: "Recruit from the Expedition Row now, at one less.",
            Fact: "When Venus shines, families climb onto their rooftops and raise their hands to catch the Goddess's gaze.",
            ArtKey: "symbol-raised-hands-prayer",
            Sources: "Getty 2019 ch. 9 (Zgoll), “Rituals for Home and Family,” fig. 9.12; sheet 19 entry A15"),

        // Sheet 19 Part A, entry A16 (the Eshgal and Nanaya)
        new FavorCard(
            Id: "favor-the-eshgal",
            Title: "The Eshgal",
            Effect: FavorEffect.OnePoint,
            EffectText: "Score one point now.",
            Fact: "In the Seleucid age Uruk builds Holy Inanna the Eshgal, a huge baked-brick temple She shares with the goddess Nanaya.",
            ArtKey: "deity-nanaya",
            Sources: "Getty 2019 ch. 12 (van Ess); ch. 35, fig. 35.6; sheet 19 entry A16"),

        // Sheet 19 Part A, entry A5 (love and fertility)
        new FavorCard(
            Id: "favor-never-tired",
            Title: "She Never Tires",
            Effect: FavorEffect.NoHandLimit,
            EffectText: "The hand limit does not apply at the end of this turn.",
            Fact: "Holy Inanna is the Goddess of love and new life, and people hope Her power of fertility will never run out.",
            ArtKey: "deity-inanna-of-love",
            Sources: "Getty 2019 ch. 9 (Zgoll), “The Goddess and Her Power”; ch. 1; ch. 37; sheet 19 entry A5"),
    };
}
