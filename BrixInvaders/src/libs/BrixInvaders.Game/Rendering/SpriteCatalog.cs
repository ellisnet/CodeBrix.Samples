using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// Which picture every game object is drawn with: the role, colour, power-up and boss-part tables of DESIGN.md
/// turned into image keys over the <see cref="AssetKeys"/> constants. An image key is either an atlas frame
/// (<c>"&lt;atlas asset key&gt;#&lt;frame name&gt;"</c>) or a loose picture's asset key (planets, backgrounds, pack
/// previews) or <see cref="PromoCard"/>.
/// </summary>
public static class SpriteCatalog
{
    /// <summary>Separates an atlas asset key from a frame name in an image key.</summary>
    public const char FrameSeparator = '#';

    /// <summary>The image key of the Kenney bundle promo card (loaded from the file beside the zips).</summary>
    public const string PromoCard = BrixInvadersAssets.PromoCardKey;

    /// <summary>The number of thrust flame frames cycled behind the player ship.</summary>
    public const int ThrustFrameCount = 4;

    /// <summary>
    /// How far the extension pack's ship art is rotated (clockwise degrees) so a boss faces DOWN the screen: the
    /// pack draws its ships nose-up.
    /// </summary>
    public const double BossShipFacing = 180.0;

    /// <summary>How far the pack's missile art is rotated so rotation 0 means "flying right" (the art points up).</summary>
    public const double MissileArtOffset = 90.0;

    /// <summary>The image key of an atlas frame.</summary>
    /// <param name="atlasKey">The atlas asset key.</param>
    /// <param name="frameName">The frame name.</param>
    /// <returns>The image key.</returns>
    public static string Frame(string atlasKey, string frameName) => atlasKey + FrameSeparator + frameName;

    /// <summary>Splits an image key into its atlas key and frame name.</summary>
    /// <param name="imageKey">The image key.</param>
    /// <param name="atlasKey">The atlas asset key, or the whole key for a loose picture.</param>
    /// <param name="frameName">The frame name, or null for a loose picture.</param>
    /// <returns>Whether the key names an atlas frame.</returns>
    public static bool TrySplit(string imageKey, out string atlasKey, out string frameName)
    {
        ArgumentNullException.ThrowIfNull(imageKey);
        var index = imageKey.IndexOf(FrameSeparator);
        if (index < 0)
        {
            atlasKey = imageKey;
            frameName = null;
            return false;
        }

        atlasKey = imageKey.Substring(0, index);
        frameName = imageKey.Substring(index + 1);
        return true;
    }

    /// <summary>The player's ship.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="colour">Ship colour, 0..3.</param>
    /// <returns>The image key.</returns>
    public static string PlayerShip(int shape, int colour) => Frame(AssetKeys.Ships.Atlas, AssetKeys.Ships.Player(shape, colour));

    /// <summary>The small life icon of a ship.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="colour">Ship colour, 0..3.</param>
    /// <returns>The image key.</returns>
    public static string LifeIcon(int shape, int colour) => Frame(AssetKeys.Ships.Atlas, AssetKeys.Ships.Life(shape, colour));

    /// <summary>The damage overlay of a ship for a <see cref="GameLogic.PlayerShip.DamageTier"/> (1 or 2); null at tier 0.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="damageTier">The damage tier, 0..2.</param>
    /// <returns>The image key, or null when undamaged.</returns>
    public static string DamageOverlay(int shape, int damageTier) =>
        damageTier <= 0 ? null : Frame(AssetKeys.Damage.Atlas, AssetKeys.Damage.PlayerOverlay(shape, Math.Min(damageTier, 3)));

    /// <summary>A formation enemy (five shapes = five roles, four colours = point tiers).</summary>
    /// <param name="role">The role.</param>
    /// <param name="colour">The colour.</param>
    /// <returns>The image key.</returns>
    public static string Enemy(EnemyRole role, EnemyColour colour) =>
        Frame(AssetKeys.Enemies.Atlas, AssetKeys.Enemies.Enemy((int)role, (int)colour));

    /// <summary>The shield bubble drawn round a shielded enemy while it still has its shield.</summary>
    public static string EnemyShield => Frame(AssetKeys.Shields.Atlas, AssetKeys.Shields.Enemy);

    /// <summary>The player's shield bubble for a strength of 1..3.</summary>
    /// <param name="strength">The shield strength.</param>
    /// <returns>The image key.</returns>
    public static string PlayerShield(int strength) =>
        Frame(AssetKeys.Shields.Atlas, AssetKeys.Shields.ForStrength(Math.Clamp(strength, 1, 3)));

    /// <summary>The bonus UFO (its colour follows its id so each crossing looks different).</summary>
    /// <param name="id">The UFO's id.</param>
    /// <returns>The image key.</returns>
    public static string Ufo(int id) => Frame(AssetKeys.Ufos.Atlas, AssetKeys.Ufos.All[PositiveModulo(id, AssetKeys.Ufos.All.Length)]);

    /// <summary>A player laser bolt.</summary>
    /// <param name="piercing">Whether the piercing laser is active for it.</param>
    /// <returns>The image key.</returns>
    public static string PlayerBolt(bool piercing) =>
        Frame(AssetKeys.Lasers.Atlas, piercing ? AssetKeys.Lasers.PlayerPiercingBolt : AssetKeys.Lasers.PlayerBolt);

    /// <summary>An enemy bolt.</summary>
    public static string EnemyBolt => Frame(AssetKeys.Lasers.Atlas, AssetKeys.Lasers.EnemyBolt);

    /// <summary>A homing missile.</summary>
    public static string Missile => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.HomingMissile);

    /// <summary>The flash drawn where a player bolt hits.</summary>
    public static string PlayerImpact => Frame(AssetKeys.Lasers.Atlas, AssetKeys.Lasers.PlayerImpact);

    /// <summary>The flash drawn where an enemy bolt hits.</summary>
    public static string EnemyImpact => Frame(AssetKeys.Lasers.Atlas, AssetKeys.Lasers.EnemyImpact);

    /// <summary>A meteor (the picture follows its id).</summary>
    /// <param name="id">The meteor's id.</param>
    /// <param name="size">Its size.</param>
    /// <returns>The image key.</returns>
    public static string Meteor(int id, MeteorSize size)
    {
        var frames = size == MeteorSize.Big ? AssetKeys.Meteors.Big : AssetKeys.Meteors.Small;
        return Frame(AssetKeys.Meteors.Atlas, frames[PositiveModulo(id, frames.Length)]);
    }

    /// <summary>A power-up drop, or its HUD icon.</summary>
    /// <param name="kind">The power-up.</param>
    /// <returns>The image key.</returns>
    public static string PowerUp(PowerUpKind kind) => Frame(AssetKeys.PowerUps.Atlas, AssetKeys.PowerUps.ForKind((int)kind));

    /// <summary>The HUD bomb icon.</summary>
    public static string BombIcon => Frame(AssetKeys.Ui.Atlas, AssetKeys.Ui.BombIcon);

    /// <summary>A thrust flame frame.</summary>
    /// <param name="index">The animation step (wraps).</param>
    /// <returns>The image key.</returns>
    public static string Thrust(int index) =>
        Frame(AssetKeys.Effects.Atlas, AssetKeys.Effects.Fire[PositiveModulo(index, ThrustFrameCount)]);

    /// <summary>The speed-boost streak drawn beside the ship.</summary>
    public static string SpeedStreak => Frame(AssetKeys.Effects.Atlas, AssetKeys.Effects.Speed);

    /// <summary>A background star sprite.</summary>
    /// <param name="index">Which of the three.</param>
    /// <returns>The image key.</returns>
    public static string Star(int index) => Frame(AssetKeys.Effects.Atlas, AssetKeys.Effects.Stars[PositiveModulo(index, AssetKeys.Effects.Stars.Length)]);

    /// <summary>The picture of one boss section.</summary>
    /// <param name="kind">The section kind.</param>
    /// <param name="design">The sector design, 1..5 (picks the core ship).</param>
    /// <returns>The image key.</returns>
    public static string BossSection(BossSectionKind kind, int design) => kind switch
    {
        BossSectionKind.Core => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.CoreForDesign(Math.Clamp(design, 1, 5))),
        BossSectionKind.Turret => Frame(AssetKeys.Turrets.Atlas, AssetKeys.Turrets.BaseBig),
        BossSectionKind.Cannon => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.Cannon),
        BossSectionKind.MissileBay => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.MissileBay),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a boss section kind."),
    };

    /// <summary>The gun drawn on a boss turret.</summary>
    public static string BossTurretGun => Frame(AssetKeys.Turrets.Atlas, AssetKeys.Turrets.Gun01);

    /// <summary>The armour plate drawn over the core while it is armoured.</summary>
    public static string BossArmour => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.ArmourPlate);

    /// <summary>A smoke puff (boss damage, wreckage).</summary>
    /// <param name="index">Which puff (wraps).</param>
    /// <returns>The image key.</returns>
    public static string Puff(int index) => Frame(AssetKeys.Bosses.Atlas, AssetKeys.Bosses.Puffs[PositiveModulo(index, AssetKeys.Bosses.Puffs.Length)]);

    /// <summary>A menu button plate.</summary>
    /// <param name="index">Which colour (wraps).</param>
    /// <returns>The image key.</returns>
    public static string Button(int index) => Frame(AssetKeys.Ui.Atlas, AssetKeys.Ui.Buttons[PositiveModulo(index, AssetKeys.Ui.Buttons.Length)]);

    /// <summary>The menu cursor.</summary>
    public static string Cursor => Frame(AssetKeys.Ui.Atlas, AssetKeys.Ui.Cursor);

    /// <summary>A planet backdrop (loose picture).</summary>
    /// <param name="index">Which planet, wraps over 0..9.</param>
    /// <returns>The image key.</returns>
    public static string Planet(int index) => AssetKeys.Planets.All[PositiveModulo(index, AssetKeys.Planets.All.Length)];

    /// <summary>A tiling space background (loose picture).</summary>
    /// <param name="background">Which background.</param>
    /// <returns>The image key.</returns>
    public static string Background(SpaceBackground background) =>
        AssetKeys.Backgrounds.All[PositiveModulo((int)background, AssetKeys.Backgrounds.All.Length)];

    /// <summary>The planet shown behind a sector's briefing and play field (designs 1..5 get their own).</summary>
    /// <param name="sector">The sector.</param>
    /// <returns>The image key.</returns>
    public static string SectorPlanet(int sector) => Planet(SectorRules.DesignOf(Math.Max(sector, 1)) * 2 - 1);

    /// <summary>The space background of a sector.</summary>
    /// <param name="sector">The sector.</param>
    /// <returns>The background.</returns>
    public static SpaceBackground SectorBackground(int sector) => SectorRules.DesignOf(Math.Max(sector, 1)) switch
    {
        1 => SpaceBackground.Blue,
        2 => SpaceBackground.DarkPurple,
        3 => SpaceBackground.Black,
        4 => SpaceBackground.Purple,
        _ => SpaceBackground.DarkPurple,
    };

    /// <summary>The pack preview pictures shown on the Kenney card (loose pictures from the zips).</summary>
    public static IReadOnlyList<string> PackPreviews { get; } = new[]
    {
        AssetKeys.Promo.RemasteredPreview,
        AssetKeys.Promo.ExtensionPreview,
        AssetKeys.Promo.PlanetsPreview,
    };

    /// <summary>Every loose picture the game draws (atlas frames are listed by <see cref="AllAtlasFrames"/>).</summary>
    public static IReadOnlyList<string> LoosePictures { get; } =
        AssetKeys.Planets.All.Concat(AssetKeys.Backgrounds.All).Concat(PackPreviews).ToArray();

    /// <summary>Every atlas frame image key the Assets library pins (the full set the game can draw from).</summary>
    public static IReadOnlyList<string> AllAtlasFrames { get; } = AssetKeyCatalog.FrameGroups
        .SelectMany(group => group.FrameNames.Select(frame => Frame(group.AtlasKey, frame)))
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    private static int PositiveModulo(int value, int count) => ((value % count) + count) % count;
}
