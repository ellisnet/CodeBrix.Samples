using System;
using BrixInvaders.Assets;
using BrixInvaders.Game.Rendering;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// The Kenney card on the briefing, sector-clear and credits screens: the pack previews (straight out of the zips),
/// the credit line, the "five zip files" line, and the bundle promo card with its clickable link.
/// </summary>
public static class KenneyCard
{
    /// <summary>The line under the credit: the whole point of the showcase.</summary>
    public const string ZipLine = "Everything on this screen came straight out of five zip files.";

    /// <summary>The caption of the bundle card.</summary>
    public const string BundleCaption = "Get the bundle: kenney.itch.io/kenney-game-assets";

    /// <summary>The card's width.</summary>
    public const double Width = 1000;

    /// <summary>The card's height.</summary>
    public const double Height = 230;

    /// <summary>Draws the card centred on a point and registers the bundle link as a hotspot.</summary>
    /// <param name="context">The context.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    public static void Paint(PaintContext context, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(context);
        var frame = context.Frame;
        Ui.Panel(frame, x, y, Width, Height, 0.95);

        var left = x - (Width / 2);
        var top = y - (Height / 2);
        var previewX = left + 90;
        foreach (var preview in SpriteCatalog.PackPreviews)
        {
            frame.AddOverlay(DrawCommand.Sprite(preview, previewX, top + 70, 150, 100));
            previewX += 160;
        }

        frame.AddOverlay(DrawCommand.Label(KenneyPacks.CreditLine, left + 20, top + 150, 17, Palette.Text, TextAnchor.Left));
        frame.AddOverlay(DrawCommand.Label(ZipLine, left + 20, top + 180, 14, Palette.Dim, TextAnchor.Left, thin: true));

        var promoX = x + (Width / 2) - 170;
        var promoY = top + 88;
        frame.AddOverlay(DrawCommand.Sprite(SpriteCatalog.PromoCard, promoX, promoY, 230, 153));
        frame.AddOverlay(DrawCommand.Label(BundleCaption, promoX, top + 186, 11, Palette.Accent, TextAnchor.Center));
        frame.AddOverlay(DrawCommand.Label($"{Prompts.Link(context.Device)} or click to open", promoX, top + 208, 11, Palette.Dim,
            TextAnchor.Center, thin: true));
        frame.AddHotspot(new Hotspot(promoX, top + 110, 300, 210, KenneyPacks.BundleUrl));
    }
}
