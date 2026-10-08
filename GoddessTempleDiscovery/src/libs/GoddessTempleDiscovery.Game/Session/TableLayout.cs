using System;
using System.Numerics;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// Where everything sits on the 1280 x 800 table, widened with the window (never narrower than 1280): the top band
/// (header, season banner, team ribbons), the Tell and the Site Row, the Expedition Row, the Tablet deck, the hand,
/// the dice tray and the action buttons, and the bottom band (specialist shelf, report stacks, the Favor deck).
/// Pure arithmetic, shared by the session (card areas) and the HUD painter (plates, labels, buttons).
/// </summary>
public sealed class TableLayout
{
    /// <summary>The base table width.</summary>
    public const float BaseWidth = 1280;

    /// <summary>The table height.</summary>
    public const float Height = 800;

    /// <summary>A Site Row card's size.</summary>
    public static readonly SKSize SiteCard = new SKSize(104, 166);

    /// <summary>An Expedition Row card's size.</summary>
    public static readonly SKSize ExpeditionCard = new SKSize(96, 154);

    /// <summary>A hand card's size.</summary>
    public static readonly SKSize HandCard = new SKSize(100, 160);

    /// <summary>A small card's size (reports, the shelf, the Favor deck).</summary>
    public static readonly SKSize SmallCard = new SKSize(50, 80);

    /// <summary>A Favor shown centre-table.</summary>
    public static readonly SKSize ShowcaseCard = new SKSize(150, 240);

    /// <summary>The size of a die.</summary>
    public const float DieSize = 66;

    /// <summary>Creates the layout for a table width.</summary>
    /// <param name="width">The table width (clamped to at least 1280).</param>
    public TableLayout(float width)
    {
        Width = Math.Max(BaseWidth, width);
    }

    /// <summary>The table width.</summary>
    public float Width { get; }

    /// <summary>The width beyond 1280.</summary>
    public float Extra => Width - BaseWidth;

    /// <summary>The horizontal centre.</summary>
    public float CentreX => Width / 2f;

    /// <summary>The header strip (wordmark, ticker, Journal and Settings).</summary>
    public SKRect Header => new SKRect(0, 0, Width, 44);

    /// <summary>The header's pane buttons, right to left: 0 Settings, 1 Journal, 2 Gallery.</summary>
    /// <param name="fromRight">0 for the rightmost.</param>
    /// <returns>The rectangle.</returns>
    public SKRect HeaderButton(int fromRight) => SKRect.Create(Width - 132 - (fromRight * 126), 5, 120, 34);

    /// <summary>The season banner.</summary>
    public SKRect SeasonBanner => new SKRect(CentreX - 470, 46, CentreX + 470, 104);

    /// <summary>The team ribbons' band.</summary>
    public SKRect RibbonBand => new SKRect(16, 108, Width - 16, 150);

    /// <summary>The ribbon of team <paramref name="index"/> of <paramref name="count"/>.</summary>
    /// <param name="index">The team's seat index.</param>
    /// <param name="count">The number of teams.</param>
    /// <returns>The rectangle.</returns>
    public SKRect Ribbon(int index, int count)
    {
        var band = RibbonBand;
        var w = (band.Width - ((count - 1) * 12)) / Math.Max(1, count);
        var left = band.Left + (index * (w + 12));
        return new SKRect(left, band.Top, left + w, band.Bottom);
    }

    /// <summary>The plate under the Tell and the Site Row.</summary>
    public SKRect SitePlate => new SKRect(12, 156, SiteSlotCentre(4).X + 72, 456);

    /// <summary>The centre of the Tell (the Site deck).</summary>
    public Vector2 TellCentre => new Vector2(78, 272);

    /// <summary>The centre of Site Row slot <paramref name="slot"/>.</summary>
    /// <param name="slot">0 to 4.</param>
    /// <returns>The centre.</returns>
    public Vector2 SiteSlotCentre(int slot) => new Vector2(206 + (Extra * 0.12f) + (slot * (122 + (Extra * 0.06f))), 272);

    /// <summary>The label box under Site Row slot <paramref name="slot"/>.</summary>
    /// <param name="slot">0 to 4.</param>
    /// <returns>The rectangle.</returns>
    public SKRect SiteLabel(int slot)
    {
        var c = SiteSlotCentre(slot);
        return new SKRect(c.X - 60, 386, c.X + 60, 452);
    }

    /// <summary>The plate under the Expedition Row.</summary>
    public SKRect ExpeditionPlate => new SKRect(ExpeditionCentre(0).X - 60, 156, Width - 12, 456);

    /// <summary>The centre of Expedition Row slot <paramref name="slot"/>.</summary>
    /// <param name="slot">0 to 3.</param>
    /// <returns>The centre.</returns>
    public Vector2 ExpeditionCentre(int slot) => new Vector2(Width - 433 + (slot * 112), 272);

    /// <summary>The plate under the Tablet deck and the hand.</summary>
    public SKRect HandPlate => new SKRect(12, 462, HandBounds.Right + 10, 696);

    /// <summary>The centre of the Tablet deck.</summary>
    public Vector2 TabletDeckCentre => new Vector2(78, 572);

    /// <summary>The layout bounds of the hand fan.</summary>
    public SKRect HandBounds => new SKRect(140, 470, 760 + (Extra * 0.55f), 674);

    /// <summary>The dice tray.</summary>
    public SKRect DiceTray => new SKRect(HandBounds.Right + 20, 462, HandBounds.Right + 266, 640);

    /// <summary>The centre of die <paramref name="index"/> (0 to 2) when <paramref name="count"/> dice are in play.</summary>
    /// <param name="index">The die.</param>
    /// <param name="count">2 or 3.</param>
    /// <returns>The centre.</returns>
    public Vector2 DieCentre(int index, int count)
    {
        var tray = DiceTray;
        var step = count >= 3 ? 76f : 92f;
        var first = tray.MidX - ((count - 1) * step / 2f);
        return new Vector2(first + (index * step), tray.Top + 66);
    }

    /// <summary>Where a die waits off the table when it is not in play.</summary>
    public static Vector2 DieParked => new Vector2(-400, -400);

    /// <summary>The line under the dice for the dig preview and the Worker stepper.</summary>
    public SKRect PreviewBox => new SKRect(DiceTray.Left, 642, DiceTray.Right, 694);

    /// <summary>The action buttons' column.</summary>
    public SKRect ButtonColumn => new SKRect(DiceTray.Right + 12, 462, Width - 14, 694);

    /// <summary>The action button <paramref name="index"/> (two columns, row by row).</summary>
    /// <param name="index">0 to 7.</param>
    /// <returns>The rectangle.</returns>
    public SKRect Button(int index)
    {
        var column = ButtonColumn;
        var w = (column.Width - 8) / 2f;
        var h = 50f;
        var x = column.Left + ((index % 2) * (w + 8));
        var y = column.Top + ((index / 2) * (h + 8));
        return new SKRect(x, y, x + w, y + h);
    }

    /// <summary>The bottom band.</summary>
    public SKRect BottomBand => new SKRect(12, 700, Width - 12, 796);

    /// <summary>The layout bounds of the current team's specialist shelf.</summary>
    public SKRect ShelfBounds => new SKRect(24, 708, 290, 790);

    /// <summary>The centre of team <paramref name="index"/>'s report stack.</summary>
    /// <param name="index">The team's seat index.</param>
    /// <returns>The centre.</returns>
    public Vector2 ReportCentre(int index) => ReportCentre(index, 4);

    /// <summary>
    /// The cell of team <paramref name="index"/> of <paramref name="count"/> in the bottom band's report bar: the bar
    /// runs from the specialist shelf to the Favor deck and is shared out equally, a report stack at the cell's left
    /// and the team's lines beside it.
    /// </summary>
    /// <param name="index">The team's seat index.</param>
    /// <param name="count">The number of teams.</param>
    /// <returns>The cell.</returns>
    public SKRect ReportCell(int index, int count)
    {
        var left = ShelfBounds.Right + 20;
        var right = FavorDeckCentre.X - 40;
        var width = (right - left) / Math.Max(1, count);
        return new SKRect(left + (index * width), BottomBand.Top + 4, left + ((index + 1) * width), BottomBand.Bottom - 4);
    }

    /// <summary>The centre of team <paramref name="index"/>'s report stack when <paramref name="count"/> teams play.</summary>
    /// <param name="index">The team's seat index.</param>
    /// <param name="count">The number of teams.</param>
    /// <returns>The centre.</returns>
    public Vector2 ReportCentre(int index, int count)
    {
        var cell = ReportCell(index, count);
        return new Vector2(cell.Left + (SmallCard.Width / 2) + 4, 749);
    }

    /// <summary>The centre of the Favor deck.</summary>
    public Vector2 FavorDeckCentre => new Vector2(Width - 150, 749);

    /// <summary>The centre of the Favor discards.</summary>
    public Vector2 FavorDiscardCentre => new Vector2(Width - 70, 749);

    /// <summary>Where a drawn Favor is shown, centre-table.</summary>
    public Vector2 ShowcaseCentre => new Vector2(CentreX, 330);

    /// <summary>A tiny area rectangle around a point: stacks and single slots lay their cards out on the centre.</summary>
    /// <param name="centre">The centre.</param>
    /// <returns>A 2 x 2 rectangle.</returns>
    public static SKRect Point(Vector2 centre) => new SKRect(centre.X - 1, centre.Y - 1, centre.X + 1, centre.Y + 1);
}
