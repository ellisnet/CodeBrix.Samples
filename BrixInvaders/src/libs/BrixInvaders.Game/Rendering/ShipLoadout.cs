using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// The visible upgrades of the player's ship: which Kenney parts are bolted onto which spot of each of the three hull
/// shapes while a capability power-up is active (DESIGN.md section 8, "Visible upgrades"). Pure tables - the
/// <see cref="PlayfieldPainter"/> draws them. Spread shot, rapid fire, piercing laser and speed boost change the ship;
/// the shield already shows as its bubble, and extra life / bomb are not capabilities.
/// </summary>
public static class ShipLoadout
{
    /// <summary>The height of every player hull frame, in art pixels.</summary>
    public const double HullArtHeight = 75;

    /// <summary>Seconds before a timed power-up runs out when its parts start to blink.</summary>
    public const double ExpiryBlinkSeconds = 2.0;

    private static readonly double[] HullArtWidths = [99, 112, 98];

    private static readonly IReadOnlyList<ShipPart>[] PartsByShape =
    [
        BuildShape(spreadX: 47, spreadY: -10, emitterX: 22, emitterY: -14, engineX: 24, engineY: 34),
        BuildShape(spreadX: 47, spreadY: -2, emitterX: 24, emitterY: -10, engineX: 30, engineY: 34),
        BuildShape(spreadX: 41, spreadY: 6, emitterX: 19, emitterY: -6, engineX: 26, engineY: 38),
    ];

    /// <summary>The power-ups that change the ship, in the order their parts are listed.</summary>
    public static IReadOnlyList<PowerUpKind> VisibleKinds { get; } =
        [PowerUpKind.SpreadShot, PowerUpKind.RapidFire, PowerUpKind.PiercingLaser, PowerUpKind.SpeedBoost];

    /// <summary>Whether a power-up puts parts on the ship.</summary>
    /// <param name="kind">The power-up.</param>
    /// <returns>True for spread shot, rapid fire, piercing laser and speed boost.</returns>
    public static bool ChangesShip(PowerUpKind kind) => VisibleKinds.Contains(kind);

    /// <summary>The art width of a hull shape's frame, in pixels.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <returns>The width.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the shape is not 0..2.</exception>
    public static double HullArtWidth(int shape) => HullArtWidths[CheckShape(shape)];

    /// <summary>
    /// How many world units one hull art pixel covers when the hull is fitted (aspect kept) into the player's draw box
    /// (<see cref="PlayfieldPainter.PlayerDrawWidth"/> x <see cref="PlayfieldPainter.PlayerDrawHeight"/>).
    /// </summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <returns>The scale.</returns>
    public static double HullScale(int shape) =>
        Math.Min(PlayfieldPainter.PlayerDrawWidth / HullArtWidth(shape), PlayfieldPainter.PlayerDrawHeight / HullArtHeight);

    /// <summary>The active power-ups (of <see cref="VisibleKinds"/>) of a running game.</summary>
    /// <param name="powerUps">The game's power-up system.</param>
    /// <returns>The active visible kinds, in <see cref="VisibleKinds"/> order.</returns>
    public static IReadOnlyList<PowerUpKind> ActiveKinds(PowerUpSystem powerUps)
    {
        ArgumentNullException.ThrowIfNull(powerUps);
        return VisibleKinds.Where(powerUps.IsActive).ToArray();
    }

    /// <summary>Every part a hull shape can carry (all four upgrades at once), in draw order.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <returns>The parts.</returns>
    public static IReadOnlyList<ShipPart> AllParts(int shape) => PartsByShape[CheckShape(shape)];

    /// <summary>The parts to draw on a hull shape for a set of active power-ups, in draw order (layer, then order).</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="active">The active power-ups (any kinds; those that do not change the ship are ignored).</param>
    /// <returns>The parts; empty when nothing visible is active.</returns>
    public static IReadOnlyList<ShipPart> PartsFor(int shape, IEnumerable<PowerUpKind> active)
    {
        ArgumentNullException.ThrowIfNull(active);
        var kinds = new HashSet<PowerUpKind>(active);
        return AllParts(shape).Where(part => kinds.Contains(part.PowerUp)).ToArray();
    }

    /// <summary>One log description of a loadout: <c>ship shape N parts [name (frame), ...]</c>.</summary>
    /// <param name="shape">Ship shape, 0..2.</param>
    /// <param name="parts">The parts drawn.</param>
    /// <returns>The description.</returns>
    public static string Describe(int shape, IReadOnlyList<ShipPart> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        return $"ship shape {CheckShape(shape) + 1} parts [{string.Join(", ", parts.Select(part => $"{part.Name} ({part.FrameName})"))}]";
    }

    private static int CheckShape(int shape) =>
        shape >= 0 && shape < HullArtWidths.Length
            ? shape
            : throw new ArgumentOutOfRangeException(nameof(shape), shape, "The ship shape must be 0..2.");

    private static IReadOnlyList<ShipPart> BuildShape(double spreadX, double spreadY, double emitterX, double emitterY,
        double engineX, double engineY)
    {
        var pod = new[] { SpriteCatalog.Frame(AssetKeys.ShipUpgradeParts.Atlas, AssetKeys.ShipUpgradeParts.SpreadPod) };
        var barrel = new[] { SpriteCatalog.Frame(AssetKeys.ShipUpgradeParts.Atlas, AssetKeys.ShipUpgradeParts.EmitterBarrel) };
        var glow = new[] { SpriteCatalog.Frame(AssetKeys.ShipUpgrades.Atlas, AssetKeys.ShipUpgrades.EmitterGlow) };
        var cannon = new[] { SpriteCatalog.Frame(AssetKeys.ShipUpgrades.Atlas, AssetKeys.ShipUpgrades.NoseCannon) };
        var engine = new[] { SpriteCatalog.Frame(AssetKeys.ShipUpgrades.Atlas, AssetKeys.ShipUpgrades.BoostEngine) };
        var flames = AssetKeys.ShipUpgrades.BoostFlames.Select(frame => SpriteCatalog.Frame(AssetKeys.ShipUpgrades.Atlas, frame)).ToArray();
        var under = ShipPartLayer.UnderHull;
        var over = ShipPartLayer.OverHull;
        var none = ShipPartEffect.None;

        var parts = new List<ShipPart>
        {
            // Speed boost: an engine nacelle under each wing with its own blue flame.
            new("boost flame left", PowerUpKind.SpeedBoost, flames, -engineX, engineY + 22, 14, 31, 0, under, 0, ShipPartEffect.Flicker),
            new("boost flame right", PowerUpKind.SpeedBoost, flames, engineX, engineY + 22, 14, 31, 0, under, 0, ShipPartEffect.Flicker),
            new("boost engine left", PowerUpKind.SpeedBoost, engine, -engineX, engineY, 22.8, 13.8, 0, under, 1, none),
            new("boost engine right", PowerUpKind.SpeedBoost, engine, engineX, engineY, 22.8, 13.8, 0, under, 1, none),

            // Rapid fire: a long cannon barrel poking out from under the nose.
            new("nose cannon", PowerUpKind.RapidFire, cannon, 0, -40, 9, 42.3, 180, under, 2, none),

            // Piercing laser: two emitter barrels beside the cockpit, a green charge glowing on each tip.
            new("emitter left", PowerUpKind.PiercingLaser, barrel, -emitterX, emitterY, 11, 30, 0, over, 10, none),
            new("emitter right", PowerUpKind.PiercingLaser, barrel, emitterX, emitterY, 11, 30, 0, over, 10, none),
            new("emitter glow left", PowerUpKind.PiercingLaser, glow, -emitterX, emitterY - 15, 5.2, 14.8, 0, over, 11, ShipPartEffect.Pulse),
            new("emitter glow right", PowerUpKind.PiercingLaser, glow, emitterX, emitterY - 15, 5.2, 14.8, 0, over, 11, ShipPartEffect.Pulse),

            // Spread shot: a gun pod on each wing tip.
            new("spread pod left", PowerUpKind.SpreadShot, pod, -spreadX, spreadY, 14.4, 34.2, 0, over, 12, none),
            new("spread pod right", PowerUpKind.SpreadShot, pod, spreadX, spreadY, 14.4, 34.2, 0, over, 12, none),
        };

        return parts.OrderBy(part => part.Layer).ThenBy(part => part.Order).ToArray();
    }
}
