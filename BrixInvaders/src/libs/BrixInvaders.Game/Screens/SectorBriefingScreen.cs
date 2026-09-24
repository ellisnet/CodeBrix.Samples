using System;
using System.Collections.Generic;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// The sector briefing (also the loading screen): the sector name, what it introduces, and the Kenney card with
/// the clickable bundle link. Confirm launches after one second.
/// </summary>
public sealed class SectorBriefingScreen : ScreenPainter
{
    /// <summary>The "new in this sector" text for a design's features.</summary>
    /// <param name="features">The features.</param>
    /// <returns>The text.</returns>
    public static string DescribeFeatures(SectorFeatures features)
    {
        var parts = new List<string>();
        if (features.HasFlag(SectorFeatures.Ufo))
        {
            parts.Add("BONUS UFO");
        }

        if (features.HasFlag(SectorFeatures.Divers))
        {
            parts.Add("DIVING RAIDERS");
        }

        if (features.HasFlag(SectorFeatures.Shielded))
        {
            parts.Add("SHIELDED SHIPS");
        }

        if (features.HasFlag(SectorFeatures.MeteorShowers))
        {
            parts.Add("METEOR SHOWERS");
        }

        if (features.HasFlag(SectorFeatures.HomingMissiles))
        {
            parts.Add("HOMING MISSILES");
        }

        if (features.HasFlag(SectorFeatures.SplitFormation))
        {
            parts.Add("SPLIT FORMATIONS");
        }

        return parts.Count == 0 ? string.Empty : "NEW: " + string.Join(" + ", parts);
    }

    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var sector = context.Session.Screens.CurrentSector;
        context.Playfield.StarSpeed = 0.8;
        context.Playfield.PaintBackdrop(frame, SpriteCatalog.SectorBackground(sector), SpriteCatalog.SectorPlanet(sector),
            Ui.CenterX, 300, 560, 0.9);

        frame.AddOverlay(DrawCommand.Label($"SECTOR {sector}", Ui.CenterX, 70, 24, Palette.Dim));
        Ui.Heading(frame, SectorRules.NameOf(sector).ToUpperInvariant(), 118, 50);
        frame.AddOverlay(DrawCommand.Label(DescribeFeatures(SectorRules.IntroducedBy(SectorRules.DesignOf(sector))), Ui.CenterX, 176,
            20, Palette.Gold));
        Ui.Panel(frame, Ui.CenterX, 236, 1000, 64, 0.8);
        frame.AddOverlay(DrawCommand.Label(SectorRules.BriefingOf(sector), Ui.CenterX, 236, 16, Palette.Text, TextAnchor.Center, thin: true));

        KenneyCard.Paint(context, Ui.CenterX, 440);

        var ready = OpenTime >= ScreenStateMachine.BriefingMinSeconds;
        var alpha = ready ? 0.6 + (0.4 * Math.Sin(OpenTime * 4)) : 0.3;
        frame.AddOverlay(DrawCommand.Label($"{Prompts.Confirm(context.Device)} LAUNCH", Ui.CenterX, 610, 26, Palette.Accent,
            TextAnchor.Center, false, alpha));
        Ui.Footer(frame, $"{Prompts.Move(context.Device)} Move    {Prompts.Fire(context.Device, context.Profile)} Fire    " +
                         $"{Prompts.Bomb(context.Device, context.Profile)} Bomb    {Prompts.Pause(context.Device)} Pause");
        Ui.Message(context);
    }
}
