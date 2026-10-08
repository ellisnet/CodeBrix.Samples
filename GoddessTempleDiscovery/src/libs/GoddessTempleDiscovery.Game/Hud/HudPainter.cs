using System;
using System.Globalization;
using System.Linq;
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Hud;

/// <summary>
/// Paints the whole table into one draw list, in z-order: the Deco board (the tile repeated at low contrast, the
/// plates the rows sit on, the slot plates, the dice tray), then the table's own cards and dice, then the HUD over
/// them (the header with the wordmark and the ticker, the season banner, the team ribbons, the slot labels, the dice
/// selection, the dig preview with its Worker stepper, the action buttons, the prompts). Buttons add hit regions
/// that the host tests the published list against.
/// </summary>
public sealed class HudPainter
{
    /// <summary>The HUD's night ground.</summary>
    public static readonly SKColor Night = SKColor.Parse("#16213A");

    /// <summary>A deeper night for plates.</summary>
    public static readonly SKColor DeepNight = SKColor.Parse("#0C1426");

    /// <summary>Gold.</summary>
    public static readonly SKColor Gold = SKColor.Parse("#D9A441");

    /// <summary>Deep gold.</summary>
    public static readonly SKColor GoldDeep = SKColor.Parse("#A8761F");

    /// <summary>Limestone, the HUD's text colour.</summary>
    public static readonly SKColor Limestone = SKColor.Parse("#EDE6D6");

    /// <summary>Lapis.</summary>
    public static readonly SKColor Lapis = SKColor.Parse("#2A4B8D");

    /// <summary>Mosaic red.</summary>
    public static readonly SKColor MosaicRed = SKColor.Parse("#B8322A");

    /// <summary>A muted limestone for secondary lines.</summary>
    public static readonly SKColor Muted = SKColor.Parse("#A9A089");

    private readonly DecoPieces _deco;
    private readonly HudFonts _fonts;

    /// <summary>Creates the painter.</summary>
    /// <param name="deco">The Deco pieces.</param>
    /// <param name="fonts">The faces.</param>
    public HudPainter(DecoPieces deco, HudFonts fonts)
    {
        _deco = deco ?? throw new ArgumentNullException(nameof(deco));
        _fonts = fonts ?? throw new ArgumentNullException(nameof(fonts));
    }

    /// <summary>The ids of the hit regions the painter adds.</summary>
    public static class Ids
    {
        /// <summary>Roll the dice.</summary>
        public const string Roll = "roll";
        /// <summary>End the turn.</summary>
        public const string EndTurn = "end";
        /// <summary>Study.</summary>
        public const string Study = "study";
        /// <summary>Survey mode.</summary>
        public const string Survey = "survey";
        /// <summary>Publish mode.</summary>
        public const string Publish = "publish";
        /// <summary>Re-roll a die.</summary>
        public const string Reroll = "reroll";
        /// <summary>Confirm the report.</summary>
        public const string Confirm = "confirm";
        /// <summary>Leave a mode.</summary>
        public const string Cancel = "cancel";
        /// <summary>One Worker fewer.</summary>
        public const string WorkersDown = "workers-";
        /// <summary>One Worker more.</summary>
        public const string WorkersUp = "workers+";
        /// <summary>Open the journal pane.</summary>
        public const string Journal = "journal";
        /// <summary>Open the settings pane.</summary>
        public const string Settings = "settings";
        /// <summary>Open the gallery pane.</summary>
        public const string Gallery = "gallery";
    }

    /// <summary>Paints one frame.</summary>
    /// <param name="list">The cleared draw list.</param>
    /// <param name="table">The table (drawn in the middle of the HUD).</param>
    /// <param name="session">The game on the table, or null before one starts.</param>
    /// <param name="controls">The human player's choices.</param>
    /// <param name="frame">The frame's lines and the pointer.</param>
    /// <param name="width">The table width.</param>
    public void Paint(DrawList list, CardsAndDiceTable table, TableSession session, PlayerControls controls, HudFrame frame, float width)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(table);
        var layout = session?.Layout ?? new TableLayout(width);
        Board(list, layout, session != null);
        if (session != null)
        {
            SyncSelection(session, controls);
        }

        table.Draw(list);

        Header(list, layout, frame);
        if (session != null)
        {
            Banner(list, layout, session);
            Ribbons(list, layout, session);
            SlotLabels(list, layout, session, controls, frame);
            Expedition(list, layout, session);
            Dice(list, session, controls);
            Preview(list, layout, session, controls, frame);
            Buttons(list, layout, session, controls, frame);
            Bottom(list, layout, session);
            Prompt(list, layout, frame);
        }

        if (frame.Preparation >= 0)
        {
            Preparation(list, layout, frame.Preparation);
        }
    }

    /// <summary>Draws text with extra space between the letters (the HUD's Deco headings).</summary>
    /// <param name="list">The draw list.</param>
    /// <param name="text">The text.</param>
    /// <param name="x">The anchor X.</param>
    /// <param name="y">The middle of the line.</param>
    /// <param name="typeface">The face.</param>
    /// <param name="size">The size.</param>
    /// <param name="color">The colour.</param>
    /// <param name="trackingEm">Extra space per letter, in ems.</param>
    /// <param name="align">Where X sits on the line.</param>
    /// <param name="alpha">Opacity.</param>
    public static void Tracked(DrawList list, string text, float x, float y, SKTypeface typeface, float size, SKColor color,
        float trackingEm, SKTextAlign align = SKTextAlign.Center, double alpha = 1)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var total = HudFonts.Measure(text, typeface, size, trackingEm);
        var left = align switch
        {
            SKTextAlign.Center => x - (total / 2f),
            SKTextAlign.Right => x - total,
            _ => x,
        };
        using var font = new SKFont(typeface, size) { Subpixel = true };
        var pen = left;
        foreach (var ch in text)
        {
            var glyph = ch.ToString();
            list.Text(glyph, pen, y, typeface, size, color, SKTextAlign.Left, alpha);
            pen += font.MeasureText(glyph) + (trackingEm * size);
        }
    }

    private void Board(DrawList list, TableLayout layout, bool inGame)
    {
        list.Rectangle(layout.CentreX, TableLayout.Height / 2, layout.Width, TableLayout.Height, Night);
        const float tile = 200;
        for (float y = 0; y < TableLayout.Height; y += tile)
        {
            for (float x = 0; x < layout.Width; x += tile)
            {
                _deco.Fill(list, "deco-board-tile", SKRect.Create(x, y, tile, tile), alpha: 0.55);
            }
        }

        if (!inGame)
        {
            return;
        }

        Plate(list, layout.SitePlate);
        Plate(list, layout.ExpeditionPlate);
        Plate(list, layout.HandPlate);
        Plate(list, layout.BottomBand);
        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            var c = layout.SiteSlotCentre(slot);
            _deco.Fill(list, DecoPieces.SlotPlateWithoutLabel, SKRect.Create(c.X - 63, c.Y - 92, 127, 203));
        }

        var tray = layout.DiceTray;
        _deco.Fit(list, "deco-dice-tray", tray.MidX, tray.Top + 66, tray.Width, 150);
        _deco.Fill(list, "deco-chevron-band", new SKRect(layout.SitePlate.Left + 40, 150, layout.SitePlate.Right - 40, 158), alpha: 0.9);
    }

    private void Plate(DrawList list, SKRect rect)
    {
        list.Rectangle(rect, new SKColor(8, 13, 26, 215), Gold, 1.2);
        list.Rectangle(new SKRect(rect.Left + 4, rect.Top + 4, rect.Right - 4, rect.Bottom - 4), SKColors.Transparent, GoldDeep, 0.8);
        _deco.Corners(list, rect, Math.Min(30, Math.Min(rect.Width, rect.Height) / 3));
    }

    private void Header(DrawList list, TableLayout layout, HudFrame frame)
    {
        var header = layout.Header;
        list.Rectangle(header, DeepNight);
        list.Rectangle(header.MidX, header.Bottom - 1, header.Width, 1.2, Gold);
        Tracked(list, "GODDESS TEMPLE DISCOVERY!", 84, header.MidY, _fonts.Bold, 13.5f, Gold, 0.14f, SKTextAlign.Left);
        var tickerLeft = 360f;
        var tickerRight = layout.Width - 404;
        var ticker = Fit(frame.Ticker ?? string.Empty, _fonts.Italic, 12.5f, tickerRight - tickerLeft);
        list.Text(ticker, (tickerLeft + tickerRight) / 2, header.MidY, _fonts.Italic, 12.5f, Limestone);
        //The pane buttons sit on the same Deco button plate as the action buttons
        Button(list, layout.HeaderButton(2), "GALLERY", Ids.Gallery, true, false);
        Button(list, layout.HeaderButton(1), "JOURNAL", Ids.Journal, true, false);
        Button(list, layout.HeaderButton(0), "SETTINGS", Ids.Settings, true, false);
    }

    private void Banner(DrawList list, TableLayout layout, TableSession session)
    {
        var season = session.BannerSeason;
        if (season == null)
        {
            return;
        }

        var rect = layout.SeasonBanner;
        _deco.Fill(list, "deco-season-banner", rect);
        list.Rectangle(rect.MidX, rect.MidY, rect.Width * 0.72, rect.Height - 14, new SKColor(12, 20, 38, 225));
        var line = Fit(CardText.BannerLine(season, session.Engine.State.Seed), _fonts.Bold, 15f, rect.Width * 0.7f);
        Tracked(list, line, rect.MidX, rect.Top + 20, _fonts.Bold, 15f, Limestone, 0.05f);
        var state = session.Engine.State;
        var sub = string.Format(CultureInfo.InvariantCulture, "SEASON {0} OF {1} · {2} · {3}",
            state.SeasonIndex + 1, state.SeasonCount, season.Director, CardText.EffectLine(season));
        list.Text(Fit(sub, _fonts.Italic, 11f, rect.Width * 0.7f), rect.MidX, rect.Top + 42, _fonts.Italic, 11f, Gold);
    }

    //The team ribbons are drawn from parts, so they stay true at any width: a night band with a gold double rule and a
    //  stepped right end, a round gold socket at the left holding the team's emblem on its colour, the name and the
    //  stats to the right of the socket, and the chevron run only in the room the text leaves.
    private void Ribbons(DrawList list, TableLayout layout, TableSession session)
    {
        var state = session.Engine.State;
        var scores = session.Engine.FinalScores();
        for (var i = 0; i < state.Teams.Count; i++)
        {
            var team = state.Teams[i];
            var rect = layout.Ribbon(i, state.Teams.Count);
            var current = !state.IsGameOver && team == state.CurrentTeam;
            var score = scores.FirstOrDefault(s => s.TeamName == team.Name)?.Total ?? 0;
            Ribbon(list, rect, team, score, current);
        }
    }

    private void Ribbon(DrawList list, SKRect rect, TeamState team, int score, bool current)
    {
        const float step = 7f;
        var radius = (rect.Height / 2f) + 1f;
        var socketX = rect.Left + radius + 1;
        var bandLeft = socketX;
        var bandRight = rect.Right - (2 * step);
        if (current)
        {
            list.Rectangle(rect.MidX, rect.MidY, rect.Width + 8, rect.Height + 6, new SKColor(217, 164, 65, 60), Gold, 2, 4);
        }

        //The band and its stepped (ziggurat) right end
        list.Rectangle(new SKRect(bandLeft, rect.Top + 2, bandRight, rect.Bottom - 2), Night, SKColor.Parse("#1E1A17"), 1.5);
        list.Rectangle(new SKRect(bandRight - 1, rect.Top + 7, bandRight + step, rect.Bottom - 7), Night);
        list.Rectangle(new SKRect(bandRight + step - 1, rect.Top + 12, rect.Right, rect.Bottom - 12), Night);
        list.Rectangle(new SKRect(bandLeft, rect.Top + 5, bandRight - 3, rect.Top + 6.2f), Gold);
        list.Rectangle(new SKRect(bandLeft, rect.Bottom - 6.2f, bandRight - 3, rect.Bottom - 5), Gold);
        list.Rectangle(new SKRect(bandRight + step - 4, rect.Top + 14, bandRight + step - 2.8f, rect.Bottom - 14), Gold);

        //The socket: a gold ring holding the team's emblem on the team's colour
        list.Circle(socketX, rect.MidY, radius, Gold, SKColor.Parse("#1E1A17"), 1.5);
        list.Circle(socketX, rect.MidY, radius - 4, SKColor.Parse(Safe(team.Profile.Colour)), GoldDeep, 1);
        if (!string.IsNullOrEmpty(team.Profile.ArtKey))
        {
            var emblem = (radius - 6) * 2;
            _deco.Fit(list, team.Profile.ArtKey, socketX, rect.MidY, emblem, emblem);
        }

        //The text starts clear of the socket; the stats take the longest wording that fits the room
        var textLeft = socketX + radius + 10;
        var room = bandRight - 10 - textLeft;
        //The name is never cut: the name with its temperament when the line fits, else the name alone, shrunk to fit
        //  (down to a floor), with the temperament as a small one-word tag at the end of the stats line when it fits
        var teamName = team.Name.ToUpperInvariant();
        var temperament = team.Kind == SeatKind.Computer ? Temperament(team.Temperament) : null;
        var nameLine = temperament == null ? teamName : teamName + "  ·  " + temperament;
        var tag = (string)null;
        if (temperament != null && HudFonts.Measure(nameLine, _fonts.Bold, 10.5f) > room)
        {
            nameLine = teamName;
            tag = temperament;
        }

        var nameSize = Shrink(nameLine, _fonts.Bold, 10.5f, 7.5f, room, 0);
        list.Text(nameLine, textLeft, rect.Top + 14, _fonts.Bold, nameSize, current ? Gold : Limestone, SKTextAlign.Left);
        var stars = team.StarCount > 0 ? team.StarCount.ToString(CultureInfo.InvariantCulture) : null;
        var full = string.Format(CultureInfo.InvariantCulture, "{0} PTS · {1} WORKERS · {2} SPEC · {3} REPORTS{4}",
            score, team.Workers, team.Specialists.Count, team.Reports.Count, stars == null ? string.Empty : " · STARS " + stars);
        var brief = string.Format(CultureInfo.InvariantCulture, "{0} PTS · {1} WRK · {2} SPEC · {3} RPT{4}",
            score, team.Workers, team.Specialists.Count, team.Reports.Count, stars == null ? string.Empty : " · STAR " + stars);
        var stats = HudFonts.Measure(full, _fonts.Regular, 10f) <= room ? full : brief;
        var statsSize = Shrink(stats, _fonts.Regular, 10f, 8f, room, 0);
        list.Text(stats, textLeft, rect.Top + 29, _fonts.Regular, statsSize, Muted, SKTextAlign.Left);
        if (tag != null)
        {
            var tagLeft = textLeft + HudFonts.Measure(stats, _fonts.Regular, statsSize) + 8;
            if (tagLeft + HudFonts.Measure(tag, _fonts.Bold, 7.5f, 0.08f) <= textLeft + room)
            {
                Tracked(list, tag, tagLeft, rect.Top + 29, _fonts.Bold, 7.5f, GoldDeep, 0.08f, SKTextAlign.Left);
            }
        }

        //The chevron run, only where the text leaves room for it
        var used = Math.Max(HudFonts.Measure(nameLine, _fonts.Bold, nameSize), HudFonts.Measure(stats, _fonts.Regular, statsSize)
            + (tag == null ? 0 : HudFonts.Measure(tag, _fonts.Bold, 7.5f, 0.08f) + 8));
        var free = bandRight - 10 - (textLeft + used);
        if (free >= 42)
        {
            for (var k = 0; k < 3; k++)
            {
                var x = bandRight - 14 - (k * 9);
                list.Rectangle(x, rect.MidY - 3.5, 9, 2.2, Gold, rotation: 45);
                list.Rectangle(x, rect.MidY + 3.5, 9, 2.2, Gold, rotation: -45);
            }
        }
    }

    private void SlotLabels(DrawList list, TableLayout layout, TableSession session, PlayerControls controls, HudFrame frame)
    {
        var engine = session.Engine;
        var labels = session.SlotLabels();
        var choice = frame.IsHumanTurn ? controls.Choice(engine) : null;
        for (var slot = 0; slot < labels.Count; slot++)
        {
            var label = labels[slot];
            var rect = layout.SiteLabel(slot);
            if (label == null)
            {
                list.Text("THE TELL IS EMPTY", rect.MidX, rect.Top + 12, _fonts.Italic, 9.5f, Muted);
                continue;
            }

            Tracked(list, label.ShortLayer, rect.MidX, rect.Top + 9, _fonts.Bold, Shrink(label.ShortLayer, _fonts.Bold, 9f, 7f, rect.Width - 4, 0.02f), Gold, 0.02f);
            //The period's short name, or no italic line at all when even that does not fit: never a cut label
            var period = string.IsNullOrEmpty(label.ShortPeriod) ? label.Period : label.ShortPeriod;
            if (HudFonts.Measure(period, _fonts.Italic, 9.5f) <= rect.Width)
            {
                list.Text(period, rect.MidX, rect.Top + 24, _fonts.Italic, 9.5f, Limestone);
            }
            Tracked(list, string.Format(CultureInfo.InvariantCulture, "DIG {0} · {1} PTS", label.DigNumber, label.Points),
                rect.MidX, rect.Top + 40, _fonts.Bold, 11f, Limestone, 0.06f);
            if (label.IsStarred)
            {
                _deco.Fit(list, "deco-star-medallion", rect.MidX, rect.Top + 56, 18, 18);
            }

            if (choice != null)
            {
                //The legal-dice hint: does the selected die (with the chosen Workers and Tablet) reach this site?
                var preview = engine.PreviewDig(slot, choice.Value, controls.Workers, controls.TabletId);
                var c = layout.SiteSlotCentre(slot);
                var hint = preview.CanDig ? "REACHES" : string.Format(CultureInfo.InvariantCulture, "NEEDS {0}", preview.Needed - preview.Total);
                list.Rectangle(c.X, c.Y - 95, 84, 18, preview.CanDig ? Gold : new SKColor(40, 30, 30, 220), Gold, 1, 3);
                Tracked(list, hint, c.X, c.Y - 95, _fonts.Bold, 9.5f, preview.CanDig ? SKColor.Parse("#1E1A17") : Limestone, 0.08f);
                if (preview.CanDig)
                {
                    list.Rectangle(c.X, c.Y, TableLayout.SiteCard.Width + 10, TableLayout.SiteCard.Height + 10, SKColors.Transparent, Gold, 2.5, 6);
                }
            }
        }

        Tracked(list, string.Format(CultureInfo.InvariantCulture, "THE TELL · {0}", engine.State.SiteDeckCount),
            layout.TellCentre.X, layout.TellCentre.Y + 100, _fonts.Bold, 9.5f, Gold, 0.08f);
    }

    private void Expedition(DrawList list, TableLayout layout, TableSession session)
    {
        var engine = session.Engine;
        for (var slot = 0; slot < GameRules.ExpeditionRowSize; slot++)
        {
            var card = engine.State.ExpeditionRow[slot];
            var c = layout.ExpeditionCentre(slot);
            if (card == null)
            {
                continue;
            }

            Tracked(list, CardText.RoleName(card.Role).ToUpperInvariant(), c.X, c.Y + 96, _fonts.Bold, 8.4f, Gold, 0.05f);
            Tracked(list, "COST " + CardText.N(engine.RecruitCost(slot)), c.X, c.Y + 114, _fonts.Bold, 10.5f, Limestone, 0.08f);
        }

        var plate = layout.ExpeditionPlate;
        Tracked(list, "THE EXPEDITION ROW", plate.MidX, plate.Bottom - 16, _fonts.Bold, 10f, GoldDeep, 0.18f);
        Tracked(list, string.Format(CultureInfo.InvariantCulture, "TABLETS · {0}", engine.State.TabletDeckCount),
            layout.TabletDeckCentre.X, layout.TabletDeckCentre.Y + 88, _fonts.Bold, 9.5f, Gold, 0.08f);
    }

    private void Dice(DrawList list, TableSession session, PlayerControls controls)
    {
        var state = session.Engine.State;
        if (state.Dice.Count == 0 || session.DiceInPlay != state.Dice.Count)
        {
            return;
        }

        for (var i = 0; i < session.DiceInPlay; i++)
        {
            var die = session.Dice[i];
            var choice = PlayerControls.ChoiceOf(session.Engine, i);
            var kept = state.ChosenDice.Contains(i);
            var label = !kept ? "ASIDE" : state.ChosenDice[0] == i ? "A" : "B";
            if (kept && choice == null && state.Phase == GamePhase.Spend)
            {
                list.Circle(die.Center.X, die.Center.Y, TableLayout.DieSize * 0.52, new SKColor(8, 12, 24, 170));
                label += " SPENT";
            }
            else if (!kept)
            {
                list.Circle(die.Center.X, die.Center.Y, TableLayout.DieSize * 0.52, new SKColor(8, 12, 24, 130));
            }

            if (controls.SelectedDice.Contains(i))
            {
                list.Circle(die.Center.X, die.Center.Y, TableLayout.DieSize * 0.62, SKColors.Transparent, Gold, 3);
            }

            Tracked(list, label, die.Center.X, die.Center.Y + 46, _fonts.Bold, 8f, choice != null ? Gold : Muted, 0.06f);
        }
    }

    private void Preview(DrawList list, TableLayout layout, TableSession session, PlayerControls controls, HudFrame frame)
    {
        var box = layout.PreviewBox;
        var engine = session.Engine;
        var state = engine.State;
        if (!frame.IsHumanTurn || state.Phase != GamePhase.Spend)
        {
            return;
        }

        var choice = controls.Choice(engine);
        string line;
        if (choice == null)
        {
            line = "CLICK A DIE, OR BOTH FOR THEIR SUM";
        }
        else if (controls.HoverSlot >= 0 && state.SiteRow[controls.HoverSlot] != null)
        {
            var preview = engine.PreviewDig(controls.HoverSlot, choice.Value, controls.Workers, controls.TabletId);
            line = string.Format(CultureInfo.InvariantCulture, "TOTAL {0} OF {1}{2}", preview.Total, preview.Needed,
                preview.Modifiers.Count == 0 ? string.Empty : " · " + string.Join(", ", preview.Modifiers));
        }
        else
        {
            line = string.Format(CultureInfo.InvariantCulture, "{0} SELECTED: {1}", choice == DiceChoice.Both ? "BOTH DICE" : choice == DiceChoice.DieA ? "DIE A" : "DIE B",
                controls.ChoiceValue(engine));
        }

        list.Text(Fit(line, _fonts.Bold, 9.5f, box.Width), box.MidX, box.Top + 8, _fonts.Bold, 9.5f, Limestone);

        //The Worker stepper and the Tablet chosen for the next dig
        var team = state.CurrentTeam;
        var y = box.Top + 30;
        SmallButton(list, SKRect.Create(box.Left + 4, y - 11, 30, 22), "−", Ids.WorkersDown, controls.Workers > 0, false);
        list.Text(string.Format(CultureInfo.InvariantCulture, "{0} OF {1} WORKERS", controls.Workers, team.Workers),
            box.Left + 92, y, _fonts.Bold, 9.5f, Gold);
        SmallButton(list, SKRect.Create(box.Left + 150, y - 11, 30, 22), "+", Ids.WorkersUp, controls.Workers < team.Workers, false);
        var tablet = controls.TabletId == null ? null : team.Tablets.FirstOrDefault(t => t.Id == controls.TabletId);
        list.Text(tablet == null ? "NO TABLET" : "TABLET +2", box.Right - 30, y, _fonts.Bold, 9f, tablet == null ? Muted : Gold);
    }

    private void Buttons(DrawList list, TableLayout layout, TableSession session, PlayerControls controls, HudFrame frame)
    {
        var engine = session.Engine;
        var human = frame.IsHumanTurn && !session.IsBusy;
        var legal = human ? engine.LegalActions() : Array.Empty<GameAction>();
        if (controls.Mode == ControlMode.Publish)
        {
            var preview = engine.PreviewReport(controls.PublishSelection);
            var label = controls.PublishSelection.Count == 0
                ? "PICK FINDS"
                : string.Format(CultureInfo.InvariantCulture, "PUBLISH {0} · {1} PTS", controls.PublishSelection.Count, preview.Points);
            Button(list, layout.Button(0), label, Ids.Confirm, human && preview.CanPublish, true);
            Button(list, layout.Button(1), "CANCEL", Ids.Cancel, true, false);
            return;
        }

        if (controls.Mode == ControlMode.Survey)
        {
            Button(list, layout.Button(0), "PICK A TRENCH", Ids.Survey, false, true);
            Button(list, layout.Button(1), "CANCEL", Ids.Cancel, true, false);
            return;
        }

        var state = engine.State;
        var rolling = state.Phase is GamePhase.AwaitRoll or GamePhase.SeasonStart;
        Button(list, layout.Button(0), "ROLL", Ids.Roll, legal.Any(a => a is RollAction), rolling && human);
        Button(list, layout.Button(1), "END TURN", Ids.EndTurn, legal.Any(a => a is EndTurnAction), !rolling && human);
        Button(list, layout.Button(2), "STUDY", Ids.Study, legal.Any(a => a is StudyAction), false);
        Button(list, layout.Button(3), engine.FreeSurveyAvailable ? "SURVEY · FREE" : "SURVEY", Ids.Survey, legal.Any(a => a is SurveyAction), false);
        Button(list, layout.Button(4), "PUBLISH", Ids.Publish, legal.Any(a => a is PublishAction), false);
        Button(list, layout.Button(5), "RE-ROLL", Ids.Reroll, legal.Any(a => a is RerollAction), false);
        var team = state.CurrentTeam;
        if (team != null)
        {
            var who = team.Kind == SeatKind.Computer ? "THE COMPUTER PLAYS" : "YOUR TURN";
            Tracked(list, who, layout.ButtonColumn.MidX, layout.Button(6).MidY - 6, _fonts.Bold, 10f, Gold, 0.12f);
            list.Text(Fit(team.Name, _fonts.Italic, 11f, layout.ButtonColumn.Width), layout.ButtonColumn.MidX, layout.Button(6).MidY + 12,
                _fonts.Italic, 11f, Limestone);
        }
    }

    //The report bar: every cell is as wide as the bar allows, and one name size (the largest at which the longest
    //  team name fits its cell) serves every team, so no name is cut
    private void Bottom(DrawList list, TableLayout layout, TableSession session)
    {
        var state = session.Engine.State;
        Tracked(list, "SPECIALISTS", layout.ShelfBounds.MidX, layout.BottomBand.Top + 14, _fonts.Bold, 8.5f, GoldDeep, 0.16f);
        var count = state.Teams.Count;
        var textOffset = TableLayout.SmallCard.Width + 14;
        var room = layout.ReportCell(0, count).Width - textOffset - 6;
        var longest = state.Teams.Select(t => t.Name).OrderByDescending(n => HudFonts.Measure(n, _fonts.Bold, 10f)).FirstOrDefault() ?? string.Empty;
        var nameSize = Shrink(longest, _fonts.Bold, 10f, 6.5f, room, 0);
        for (var i = 0; i < count; i++)
        {
            var team = state.Teams[i];
            var cell = layout.ReportCell(i, count);
            var x = cell.Left + textOffset;
            var y = layout.ReportCentre(i, count).Y;
            list.Text(team.Name, x, y - 14, _fonts.Bold, nameSize, Limestone, SKTextAlign.Left);
            list.Rectangle(x + 4, y - 2, 8, 8, SKColor.Parse(Safe(team.Profile.Colour)));
            var reports = team.Reports.Count;
            var line = string.Format(CultureInfo.InvariantCulture, "{0} {1} · {2} PTS", reports, reports == 1 ? "REPORT" : "REPORTS", team.Reports.Sum(r => r.Points));
            list.Text(line, x + 14, y - 1, _fonts.Regular, Shrink(line, _fonts.Regular, 9f, 7f, room - 14, 0), Muted, SKTextAlign.Left);
        }

        Tracked(list, "FAVORS", layout.FavorDeckCentre.X - 50, layout.BottomBand.Top + 14, _fonts.Bold, 8.5f, GoldDeep, 0.16f);
    }

    //The largest size, from the preferred size down to a floor, at which a line fits a width
    private static float Shrink(string text, SKTypeface typeface, float size, float floor, float width, float trackingEm)
    {
        while (size > floor && HudFonts.Measure(text, typeface, size, trackingEm) > width)
        {
            size -= 0.25f;
        }

        return Math.Max(size, floor);
    }

    private void Prompt(DrawList list, TableLayout layout, HudFrame frame)
    {
        if (string.IsNullOrWhiteSpace(frame.Prompt))
        {
            return;
        }

        var plate = layout.HandPlate;
        list.Text(Fit(frame.Prompt, _fonts.Italic, 11.5f, plate.Width - 40), plate.MidX, plate.Bottom - 12, _fonts.Italic, 11.5f, Gold);
    }

    private void Preparation(DrawList list, TableLayout layout, double progress)
    {
        var cx = layout.CentreX;
        var rect = SKRect.Create(cx - 300, 330, 600, 120);
        list.Rectangle(rect, new SKColor(8, 13, 26, 240), Gold, 1.5);
        _deco.Corners(list, rect, 26);
        Tracked(list, string.Format(CultureInfo.InvariantCulture, "PREPARING THE CARDS · {0:P0}", progress), cx, 365, _fonts.Bold, 15f, Limestone, 0.1f);
        list.Rectangle(cx, 405, 520, 12, new SKColor(42, 75, 141, 160), GoldDeep, 1);
        var filled = (float)Math.Clamp(progress, 0, 1) * 520;
        if (filled > 0)
        {
            list.Rectangle(cx - 260 + (filled / 2), 405, filled, 12, Gold);
        }
    }

    private void Button(DrawList list, SKRect rect, string label, string id, bool enabled, bool primary)
    {
        _deco.Fill(list, primary && enabled ? "deco-button-plate-active" : "deco-button-plate", rect, alpha: enabled ? 1 : 0.45);
        var size = 11.5f;
        var text = label;
        while (size > 8 && HudFonts.Measure(text, _fonts.Bold, size, 0.1f) > rect.Width - 34)
        {
            size -= 0.5f;
        }

        Tracked(list, text, rect.MidX, rect.MidY, _fonts.Bold, size, primary && enabled ? SKColor.Parse("#16213A") : Limestone, 0.1f, alpha: enabled ? 1 : 0.5);
        if (enabled)
        {
            list.HitRegion(rect.MidX, rect.MidY, rect.Width, rect.Height, id);
        }
    }

    private void SmallButton(DrawList list, SKRect rect, string label, string id, bool enabled, bool primary)
    {
        list.Rectangle(rect, primary ? Gold : new SKColor(22, 33, 58, 240), Gold, 1.2, 2, enabled ? 1 : 0.45);
        Tracked(list, label, rect.MidX, rect.MidY, _fonts.Bold, label.Length <= 2 ? 13f : 10f, primary ? Night : Gold, 0.1f, alpha: enabled ? 1 : 0.5);
        if (enabled)
        {
            list.HitRegion(rect.MidX, rect.MidY, rect.Width, rect.Height, id);
        }
    }

    private static void SyncSelection(TableSession session, PlayerControls controls)
    {
        foreach (var card in session.HandArea.Pile.Cards)
        {
            var data = TableSession.RulesCard(card);
            card.IsSelected = data switch
            {
                TabletCard tablet => tablet.Id == controls.TabletId,
                DiscoveryCard discovery => controls.Mode == ControlMode.Publish && controls.PublishSelection.Contains(discovery.Id),
                _ => false,
            };
        }
    }

    private static string Temperament(Temperament temperament) => temperament switch
    {
        Rules.Engine.Temperament.DeepDigger => "DEEP DIGGER",
        Rules.Engine.Temperament.Scholar => "SCHOLAR",
        _ => "SURVEYOR",
    };

    private static string Safe(string colour) =>
        !string.IsNullOrWhiteSpace(colour) && SKColor.TryParse(colour, out _) ? colour : "#A8761F";

    private static string Fit(string text, SKTypeface typeface, float size, float width)
    {
        if (string.IsNullOrEmpty(text) || HudFonts.Measure(text, typeface, size) <= width)
        {
            return text ?? string.Empty;
        }

        var cut = text;
        while (cut.Length > 1 && HudFonts.Measure(cut + "…", typeface, size) > width)
        {
            cut = cut.Substring(0, cut.Length - 1);
        }

        return cut.TrimEnd() + "…";
    }
}
