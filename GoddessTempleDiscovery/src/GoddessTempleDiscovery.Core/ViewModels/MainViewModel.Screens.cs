using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Credits;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using Microsoft.UI.Xaml;

namespace GoddessTempleDiscovery.ViewModels;

public partial class MainViewModel
{
    #region | How to Play, History, Credits |

    /// <summary>How to Play, section by section, each with its icon.</summary>
    public ObservableCollection<TextItem> HowToPlayItems { get; } = new ObservableCollection<TextItem>();

    /// <summary>The real people of the excavations.</summary>
    public ObservableCollection<TextItem> PeopleItems { get; } = new ObservableCollection<TextItem>();

    /// <summary>The periods of Uruk's history.</summary>
    public ObservableCollection<TextItem> TimelineItems { get; } = new ObservableCollection<TextItem>();

    /// <summary>The credits: the art, the fonts, the add-on, the sources.</summary>
    public ObservableCollection<TextItem> CreditItems { get; } = new ObservableCollection<TextItem>();

    /// <summary>True while the History shows the timeline (false: the people).</summary>
    [AffectsProperties(nameof(PeopleVisibility), nameof(TimelineVisibility))]
    public bool ShowTimeline
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>The people tab.</summary>
    public Visibility PeopleVisibility => GetVisibility(!ShowTimeline);

    /// <summary>The timeline tab.</summary>
    public Visibility TimelineVisibility => GetVisibility(ShowTimeline);

    /// <summary>Shows the people tab.</summary>
    public SimpleCommand PeopleTabCommand => field ??= new SimpleCommand(() => ShowTimeline = false);

    /// <summary>Shows the timeline tab.</summary>
    public SimpleCommand TimelineTabCommand => field ??= new SimpleCommand(() => ShowTimeline = true);

    /// <summary>The heritage note's title.</summary>
    public string HeritageTitle => HeritageNote.Title;

    /// <summary>The heritage note's first paragraph, verbatim.</summary>
    public string HeritageFirstParagraph => HeritageNote.FirstParagraph;

    /// <summary>The heritage note's second paragraph, verbatim.</summary>
    public string HeritageSecondParagraph => HeritageNote.SecondParagraph;

    private void BuildScreens()
    {
        var tablet = TabletCard.PointValue.ToString(CultureInfo.InvariantCulture);
        var tabletPlural = TabletCard.PointValue == 1 ? "point" : "points";
        var help = new[]
        {
            new TextItem("The premise", "Winter 1912/13 to 1938/39",
                "Two to four fictional expeditions have permits to dig at Warka. Each wants to be the one that brings the House of Heaven, Holy Inanna's Eanna precinct, back into the light. The game runs through the twelve real pre-war campaigns. Each season the real history happens to everyone (a Season card), and each team digs, recruits, studies and publishes. When the 1938/39 season ends, the Second World War closes the dig, the reports are tallied, and the team with the most points has made the greatest discoveries. The teams are fictional; the history, the buildings, the finds, the people, the Goddess and the city on the cards are real and sourced.",
                "icon-season-calendar"),
            new TextItem("A turn: roll", "Space, or the ROLL button",
                "Roll both dice. The two dice are the team's two work crews. Then spend the dice, one at a time; a die can be used alone or both dice together as one sum. Click a die to choose it, click both for their sum.",
                "icon-dice-pair"),
            new TextItem("Dig", "Click a trench of the Site Row, or press 1 to 5",
                "The die value or the sum, plus modifiers, must be at least the site's Dig Number. The card flips face-up, the discovery opens large as an EXTRA! edition, the card goes to the team's hand (unpublished finds), and a new site is dealt face-down into the row. Modifiers: spend any number of Workers (+1 each, the stepper under the dice), one Tablet (+2, click it in the hand), and the team's Specialists.",
                "icon-dig"),
            new TextItem("Dig Numbers and depth", "Start at the top, dig deeper",
                "Seleucid and Parthian 3 (1 point); Neo-Babylonian and Achaemenid 4 (1); Kassite and Old Babylonian 5 (2); Ur III 6 (2); Early Dynastic and Akkadian 7 (3); Jemdet Nasr, Level III 8 (3); Uruk IV 9 (4); Uruk V 10 (5); the deep sounding 11 (6). A single die reaches the top three tiers; Workers, Tablets and Specialists make the deepest reachable from mid-game. Difficulty shifts every Dig Number by -1, 0 or +1.",
                "icon-depth-tiers"),
            new TextItem("Recruit", "Click a specialist of the Expedition Row",
                "The die must be at least the Specialist's cost. A team holds at most four Specialists: the Architect (+1 on buildings), the Epigrapher (+1 on inscriptions; Study draws two), the Small-Finds Keeper (+1 on objects), the Photographer (+1 on every report), the Foreman (+1 on one dig a turn) and the Surveyor (one free survey a turn). The row refills.",
                "icon-recruit"),
            new TextItem("Study", "The STUDY button",
                "Spend a die of any value to draw the top Tablet into the hand; a 6 draws two. A Tablet is worth " + tablet + " " + tabletPlural + " at the end, or +2 on one dig when spent.",
                "icon-study"),
            new TextItem("Survey", "The SURVEY button, then a trench",
                "Spend a die to send one Site Row card to the bottom of the Tell and deal a new one. The Surveyor, some seasons and some Favors make a survey free.",
                "icon-survey"),
            new TextItem("Publish", "The PUBLISH button, then three or more finds",
                "Any time during the turn, lay down three or more Discoveries from the hand as a Preliminary Report. Published cards are safe and score their printed points plus a bonus: +1 per card if all are from the same period (a stratigraphy report), or +2 per card if they form a run of three or more consecutive periods (a sequence report). The Photographer adds +1 to every report.",
                "icon-publish"),
            new TextItem("Doubles and the hand limit", "The Goddess favors you",
                "Doubles draw a Favor card once both dice are spent: a small gift, each with a true line about Her. At the end of a turn the hand limit is seven cards (Discoveries and Tablets); set aside the rest.",
                "icon-favor"),
            new TextItem("Seasons", "Twelve campaigns",
                "Each season the Season card flips, shows its true story, and its effect applies to every team. Each team then takes its turns in seat order, and every team gains a Worker at the season's end. The season's length is chosen at setup: the game suggests three turns a season for two seats, two for three seats and one for four, and any of 1, 2 or 3 may be chosen. The last season, 1938/39, publishes nothing by itself: publish what you can; what stays in the crates scores half.",
                "icon-season-calendar"),
            new TextItem("Scoring at the end", "The Final Edition",
                "Points on every published Discovery, plus report bonuses. Unpublished Discoveries score half, rounded down (the finds are in crates, unreported). Each Tablet in hand: " + tablet + " " + tabletPlural + ". Each complete set of the four Tablet kinds: +3. The Star of Holy Inanna: the team with the most star-marked Discoveries gains +5; ties share it. Tie-break: most Discoveries, then the deepest Discovery.",
                "icon-points"),
            new TextItem("The computer teams", "Surveyor, Deep Digger, Scholar",
                "One decision procedure, three weightings: the Surveyor digs steadily and keeps Tablets, the Deep Digger banks Workers for the deepest layers and chases Her stars, the Scholar studies, recruits the Epigrapher and the Photographer, and publishes sequence reports. Their turns play out on the table; the inspector opens for their finds when the setting says so.",
                "icon-computer-player"),
            new TextItem("Controls", "Mouse and keys",
                "Click a die, then a trench to dig or a specialist to recruit. Right-click any face-up card to read it in the inspector. Space or Enter rolls, or ends the turn; Escape closes the inspector; J opens the Field Journal; 1 to 5 dig that trench with the best legal dice; R re-rolls when a season allows it.",
                "icon-inspect"),
        };
        foreach (var item in help)
        {
            HowToPlayItems.Add(item);
            _ = LoadArtAsync(item, 96);
        }

        foreach (var person in Catalog.People ?? Array.Empty<PersonCard>())
        {
            var text = person.CardText + (string.IsNullOrWhiteSpace(person.EthicsNote) ? string.Empty : "\n\nThe record: " + person.EthicsNote);
            var item = new TextItem(person.Name, person.Dates + " · " + person.Role, text, person.ArtKey);
            PeopleItems.Add(item);
            _ = LoadArtAsync(item, 120);
        }

        foreach (var period in Catalog.Periods ?? Array.Empty<PeriodInfo>())
        {
            var item = new TextItem(period.Name, string.Join(" · ", new[] { period.Years, period.Levels }.Where(s => !string.IsNullOrWhiteSpace(s))),
                period.Summary, period.ArtKey);
            TimelineItems.Add(item);
            _ = LoadArtAsync(item, 96);
        }

        CreditItems.Add(new TextItem("The art", "Apache License 2.0", ArtCatalog.LicenseText, "symbol-eight-pointed-star"));
        CreditItems.Add(new TextItem("The fonts", "SIL Open Font License 1.1",
            "Merriweather (Regular, Bold, Italic) sets every title and every line of text; Noto Sans Cuneiform sets the cuneiform. Both travel inside the application under the SIL Open Font License 1.1, whose texts travel beside them.",
            string.Empty));
        CreditItems.Add(new TextItem("The table", "CodeBrix.Platform.GameEngine.CardsAndDice (MIT)",
            "The cards and dice, the deals and flips and rolls, the celestial Star of Ishtar card back and the card and dice sounds come from the CardsAndDice add-on of CodeBrix.Platform.GameEngine, under the MIT License; its bundled artwork is CC0, public domain, or the add-on's own.",
            "icon-dice-pair"));
        CreditItems.Add(new TextItem("The sources", "Nothing on a card is invented",
            "Every card carries its sources: Julius Jordan's preliminary reports (UVB, 1929 to 1932), the later reports of Nöldeke, Heinrich and Lenzen, the Getty volume Uruk: First City of the Ancient World (2019) with Margarete van Ess's chapters, the ETCSL translations, and the research sheets of the Julius_Jordan_Eanna library. The inspector and the Field Journal print each card's sources in full.",
            "icon-journal"));
        foreach (var item in CreditItems)
        {
            _ = LoadArtAsync(item, 96);
        }
    }

    #endregion
}
