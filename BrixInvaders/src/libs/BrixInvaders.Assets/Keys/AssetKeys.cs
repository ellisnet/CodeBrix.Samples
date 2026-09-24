namespace BrixInvaders.Assets;

/// <summary>
/// Every asset key and every atlas frame name the game uses, grouped by what they draw or play.
/// </summary>
/// <remarks>
/// <para>
/// A key is <c>kenney:&lt;pack slug&gt;/&lt;path inside the zip without its extension&gt;</c>, spelled exactly as the
/// Kenney asset provider reports it (the engine's tilesheet registry is ordinal, so the spelling matters). The pack
/// slugs are the ones pinned in <see cref="KenneyPacks"/>.
/// </para>
/// <para>
/// Two shapes of group live here. An ASSET group (<see cref="Atlases"/>, <see cref="Backgrounds"/>,
/// <see cref="Planets"/>, <see cref="Fonts"/>, <see cref="Sfx"/>, <see cref="Promo"/>) holds nothing but asset
/// keys. A FRAME group (<see cref="Ships"/>, <see cref="Enemies"/>, ...) names the atlas it cuts from in its
/// <c>Atlas</c> constant, and every other constant in it is a frame (region) name inside that atlas: draw it with
/// <c>sheet[frameName, 0, 0]</c>, never <c>sheet[0, 0]</c>. The remastered pack's atlas holds every picture of that
/// pack, so the game draws ships, enemies, lasers, effects, meteors, power-ups and HUD icons from ONE tilesheet.
/// </para>
/// <para>
/// Index arguments of the helper methods use the ordinal values of the matching <c>BrixInvaders.GameLogic</c>
/// enums (for example <c>(int)EnemyRole.Diver</c>), so this library does not depend on the rules library.
/// </para>
/// </remarks>
public static class AssetKeys
{
    /// <summary>The two sprite atlases (one tilesheet each, frames addressed by name).</summary>
    public static class Atlases
    {
        /// <summary>The remastered pack's atlas: 294 frames covering every picture of that pack.</summary>
        public const string Main = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>The extension pack's atlas: boss ships, station pieces, parts, missiles, effects (278 frames).</summary>
        public const string Extension = "kenney:space-shooter-extension/Spritesheet/spaceShooter2_spritesheet";
    }

    /// <summary>The player's ships (3 shapes x 4 colours), their damage overlays and the HUD life icons.</summary>
    public static class Ships
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Player ship shape 1, blue.</summary>
        public const string PlayerShip1Blue = "playerShip1_blue";

        /// <summary>Player ship shape 1, green.</summary>
        public const string PlayerShip1Green = "playerShip1_green";

        /// <summary>Player ship shape 1, orange.</summary>
        public const string PlayerShip1Orange = "playerShip1_orange";

        /// <summary>Player ship shape 1, red.</summary>
        public const string PlayerShip1Red = "playerShip1_red";

        /// <summary>Player ship shape 2, blue.</summary>
        public const string PlayerShip2Blue = "playerShip2_blue";

        /// <summary>Player ship shape 2, green.</summary>
        public const string PlayerShip2Green = "playerShip2_green";

        /// <summary>Player ship shape 2, orange.</summary>
        public const string PlayerShip2Orange = "playerShip2_orange";

        /// <summary>Player ship shape 2, red.</summary>
        public const string PlayerShip2Red = "playerShip2_red";

        /// <summary>Player ship shape 3, blue.</summary>
        public const string PlayerShip3Blue = "playerShip3_blue";

        /// <summary>Player ship shape 3, green.</summary>
        public const string PlayerShip3Green = "playerShip3_green";

        /// <summary>Player ship shape 3, orange.</summary>
        public const string PlayerShip3Orange = "playerShip3_orange";

        /// <summary>Player ship shape 3, red.</summary>
        public const string PlayerShip3Red = "playerShip3_red";

        /// <summary>HUD life icon for ship shape 1, blue.</summary>
        public const string PlayerLife1Blue = "playerLife1_blue";

        /// <summary>HUD life icon for ship shape 1, green.</summary>
        public const string PlayerLife1Green = "playerLife1_green";

        /// <summary>HUD life icon for ship shape 1, orange.</summary>
        public const string PlayerLife1Orange = "playerLife1_orange";

        /// <summary>HUD life icon for ship shape 1, red.</summary>
        public const string PlayerLife1Red = "playerLife1_red";

        /// <summary>HUD life icon for ship shape 2, blue.</summary>
        public const string PlayerLife2Blue = "playerLife2_blue";

        /// <summary>HUD life icon for ship shape 2, green.</summary>
        public const string PlayerLife2Green = "playerLife2_green";

        /// <summary>HUD life icon for ship shape 2, orange.</summary>
        public const string PlayerLife2Orange = "playerLife2_orange";

        /// <summary>HUD life icon for ship shape 2, red.</summary>
        public const string PlayerLife2Red = "playerLife2_red";

        /// <summary>HUD life icon for ship shape 3, blue.</summary>
        public const string PlayerLife3Blue = "playerLife3_blue";

        /// <summary>HUD life icon for ship shape 3, green.</summary>
        public const string PlayerLife3Green = "playerLife3_green";

        /// <summary>HUD life icon for ship shape 3, orange.</summary>
        public const string PlayerLife3Orange = "playerLife3_orange";

        /// <summary>HUD life icon for ship shape 3, red.</summary>
        public const string PlayerLife3Red = "playerLife3_red";

        /// <summary>The ship colours in the pack's own naming, in settings order (index 0..3).</summary>
        public static readonly string[] Colours = ["blue", "green", "orange", "red"];

        /// <summary>Gets the frame name of a player ship.</summary>
        /// <param name="shape">The ship shape, 0..2 (the <c>BrixInvaders.ShipShape</c> setting).</param>
        /// <param name="colour">The ship colour, 0..3: blue, green, orange, red (the <c>BrixInvaders.ShipColour</c> setting).</param>
        /// <returns>The frame name, for example <c>playerShip1_blue</c>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when an index is out of range.</exception>
        public static string Player(int shape, int colour) =>
            $"playerShip{AssetKeyChecks.Index(shape, 3, nameof(shape)) + 1}_{Colours[AssetKeyChecks.Index(colour, 4, nameof(colour))]}";

        /// <summary>Gets the frame name of the HUD life icon matching a player ship.</summary>
        /// <param name="shape">The ship shape, 0..2.</param>
        /// <param name="colour">The ship colour, 0..3.</param>
        /// <returns>The frame name, for example <c>playerLife1_blue</c>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when an index is out of range.</exception>
        public static string Life(int shape, int colour) =>
            $"playerLife{AssetKeyChecks.Index(shape, 3, nameof(shape)) + 1}_{Colours[AssetKeyChecks.Index(colour, 4, nameof(colour))]}";
    }

    /// <summary>Damage overlays drawn over the player ship (and scratches for anything else that is hit).</summary>
    public static class Damage
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Damage overlay 1 for ship shape 1.</summary>
        public const string PlayerShip1Damage1 = "playerShip1_damage1";

        /// <summary>Damage overlay 2 for ship shape 1.</summary>
        public const string PlayerShip1Damage2 = "playerShip1_damage2";

        /// <summary>Damage overlay 3 for ship shape 1.</summary>
        public const string PlayerShip1Damage3 = "playerShip1_damage3";

        /// <summary>Damage overlay 1 for ship shape 2.</summary>
        public const string PlayerShip2Damage1 = "playerShip2_damage1";

        /// <summary>Damage overlay 2 for ship shape 2.</summary>
        public const string PlayerShip2Damage2 = "playerShip2_damage2";

        /// <summary>Damage overlay 3 for ship shape 2.</summary>
        public const string PlayerShip2Damage3 = "playerShip2_damage3";

        /// <summary>Damage overlay 1 for ship shape 3.</summary>
        public const string PlayerShip3Damage1 = "playerShip3_damage1";

        /// <summary>Damage overlay 2 for ship shape 3.</summary>
        public const string PlayerShip3Damage2 = "playerShip3_damage2";

        /// <summary>Damage overlay 3 for ship shape 3.</summary>
        public const string PlayerShip3Damage3 = "playerShip3_damage3";

        /// <summary>Scratch overlay 1.</summary>
        public const string Scratch1 = "scratch1";

        /// <summary>Scratch overlay 2.</summary>
        public const string Scratch2 = "scratch2";

        /// <summary>Scratch overlay 3.</summary>
        public const string Scratch3 = "scratch3";

        /// <summary>Gets the damage overlay for a ship shape and a <c>PlayerShip.DamageTier</c>.</summary>
        /// <param name="shape">The ship shape, 0..2.</param>
        /// <param name="tier">The damage tier, 1..3 (DESIGN.md uses 1 at 2/3 hull and 2 at 1/3 hull).</param>
        /// <returns>The frame name, for example <c>playerShip1_damage1</c>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when an argument is out of range.</exception>
        public static string PlayerOverlay(int shape, int tier) =>
            $"playerShip{AssetKeyChecks.Index(shape, 3, nameof(shape)) + 1}_damage{AssetKeyChecks.Index(tier - 1, 3, nameof(tier)) + 1}";
    }

    /// <summary>
    /// The formation enemies: five shapes = the five roles, four colours = the four point tiers.
    /// </summary>
    /// <remarks>
    /// Role to shape: Grunt 1, Shooter 2, Shielded 3, Diver 4, Missile carrier 5. Colour tier to colour: 0 Black,
    /// 1 Blue, 2 Green, 3 Red (the <c>EnemyColour</c> enum order).
    /// </remarks>
    public static class Enemies
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Grunt role (grunt), black tier.</summary>
        public const string GruntBlack = "enemyBlack1";

        /// <summary>Grunt role (grunt), blue tier.</summary>
        public const string GruntBlue = "enemyBlue1";

        /// <summary>Grunt role (grunt), green tier.</summary>
        public const string GruntGreen = "enemyGreen1";

        /// <summary>Grunt role (grunt), red tier.</summary>
        public const string GruntRed = "enemyRed1";

        /// <summary>Shooter role (shooter), black tier.</summary>
        public const string ShooterBlack = "enemyBlack2";

        /// <summary>Shooter role (shooter), blue tier.</summary>
        public const string ShooterBlue = "enemyBlue2";

        /// <summary>Shooter role (shooter), green tier.</summary>
        public const string ShooterGreen = "enemyGreen2";

        /// <summary>Shooter role (shooter), red tier.</summary>
        public const string ShooterRed = "enemyRed2";

        /// <summary>Shielded role (shielded), black tier.</summary>
        public const string ShieldedBlack = "enemyBlack3";

        /// <summary>Shielded role (shielded), blue tier.</summary>
        public const string ShieldedBlue = "enemyBlue3";

        /// <summary>Shielded role (shielded), green tier.</summary>
        public const string ShieldedGreen = "enemyGreen3";

        /// <summary>Shielded role (shielded), red tier.</summary>
        public const string ShieldedRed = "enemyRed3";

        /// <summary>Diver role (diver), black tier.</summary>
        public const string DiverBlack = "enemyBlack4";

        /// <summary>Diver role (diver), blue tier.</summary>
        public const string DiverBlue = "enemyBlue4";

        /// <summary>Diver role (diver), green tier.</summary>
        public const string DiverGreen = "enemyGreen4";

        /// <summary>Diver role (diver), red tier.</summary>
        public const string DiverRed = "enemyRed4";

        /// <summary>MissileCarrier role (missile carrier), black tier.</summary>
        public const string MissileCarrierBlack = "enemyBlack5";

        /// <summary>MissileCarrier role (missile carrier), blue tier.</summary>
        public const string MissileCarrierBlue = "enemyBlue5";

        /// <summary>MissileCarrier role (missile carrier), green tier.</summary>
        public const string MissileCarrierGreen = "enemyGreen5";

        /// <summary>MissileCarrier role (missile carrier), red tier.</summary>
        public const string MissileCarrierRed = "enemyRed5";

        /// <summary>The enemy colours in point-tier order (index 0..3 = <c>EnemyColour</c>).</summary>
        public static readonly string[] Colours = ["Black", "Blue", "Green", "Red"];

        /// <summary>Gets the frame name of a formation enemy.</summary>
        /// <param name="role">The role, 0..4 (<c>EnemyRole</c>: Grunt, Shooter, Shielded, Diver, MissileCarrier).</param>
        /// <param name="colour">The colour tier, 0..3 (<c>EnemyColour</c>: Black, Blue, Green, Red).</param>
        /// <returns>The frame name, for example <c>enemyRed4</c> for a red diver.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when an index is out of range.</exception>
        public static string Enemy(int role, int colour) =>
            $"enemy{Colours[AssetKeyChecks.Index(colour, 4, nameof(colour))]}{AssetKeyChecks.Index(role, 5, nameof(role)) + 1}";
    }

    /// <summary>The bonus UFO in its four colours.</summary>
    public static class Ufos
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Blue UFO.</summary>
        public const string Blue = "ufoBlue";

        /// <summary>Green UFO.</summary>
        public const string Green = "ufoGreen";

        /// <summary>Red UFO.</summary>
        public const string Red = "ufoRed";

        /// <summary>Yellow UFO.</summary>
        public const string Yellow = "ufoYellow";

        /// <summary>The four UFO frames, for picking one per appearance.</summary>
        public static readonly string[] All = [Blue, Green, Red, Yellow];
    }

    /// <summary>Bolts and their impact flashes.</summary>
    public static class Lasers
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>The player's bolt (9 x 54).</summary>
        public const string PlayerBolt = "laserBlue01";

        /// <summary>The player's bolt while the piercing laser is active (9 x 54).</summary>
        public const string PlayerPiercingBolt = "laserGreen11";

        /// <summary>An enemy bolt (9 x 54).</summary>
        public const string EnemyBolt = "laserRed01";

        /// <summary>A boss bolt (13 x 54).</summary>
        public const string BossBolt = "laserRed16";

        /// <summary>Flash where a player bolt hits (48 x 46).</summary>
        public const string PlayerImpact = "laserBlue08";

        /// <summary>Small flash where a player bolt hits armour (37 x 37).</summary>
        public const string PlayerImpactSmall = "laserBlue10";

        /// <summary>Flash where a piercing bolt hits (48 x 46).</summary>
        public const string PiercingImpact = "laserGreen14";

        /// <summary>Flash where an enemy bolt hits the player (48 x 46).</summary>
        public const string EnemyImpact = "laserRed08";
    }

    /// <summary>Thrust flames, stars and the speed streak.</summary>
    public static class Effects
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Thrust flame frame 00.</summary>
        public const string Fire00 = "fire00";

        /// <summary>Thrust flame frame 01.</summary>
        public const string Fire01 = "fire01";

        /// <summary>Thrust flame frame 02.</summary>
        public const string Fire02 = "fire02";

        /// <summary>Thrust flame frame 03.</summary>
        public const string Fire03 = "fire03";

        /// <summary>Thrust flame frame 04.</summary>
        public const string Fire04 = "fire04";

        /// <summary>Thrust flame frame 05.</summary>
        public const string Fire05 = "fire05";

        /// <summary>Thrust flame frame 06.</summary>
        public const string Fire06 = "fire06";

        /// <summary>Thrust flame frame 07.</summary>
        public const string Fire07 = "fire07";

        /// <summary>Thrust flame frame 08.</summary>
        public const string Fire08 = "fire08";

        /// <summary>Thrust flame frame 09.</summary>
        public const string Fire09 = "fire09";

        /// <summary>Thrust flame frame 10.</summary>
        public const string Fire10 = "fire10";

        /// <summary>Thrust flame frame 11.</summary>
        public const string Fire11 = "fire11";

        /// <summary>Thrust flame frame 12.</summary>
        public const string Fire12 = "fire12";

        /// <summary>Thrust flame frame 13.</summary>
        public const string Fire13 = "fire13";

        /// <summary>Thrust flame frame 14.</summary>
        public const string Fire14 = "fire14";

        /// <summary>Thrust flame frame 15.</summary>
        public const string Fire15 = "fire15";

        /// <summary>Thrust flame frame 16.</summary>
        public const string Fire16 = "fire16";

        /// <summary>Thrust flame frame 17.</summary>
        public const string Fire17 = "fire17";

        /// <summary>Thrust flame frame 18.</summary>
        public const string Fire18 = "fire18";

        /// <summary>Thrust flame frame 19.</summary>
        public const string Fire19 = "fire19";

        /// <summary>Star sparkle 1 (parallax stars, particles).</summary>
        public const string Star1 = "star1";

        /// <summary>Star sparkle 2 (parallax stars, particles).</summary>
        public const string Star2 = "star2";

        /// <summary>Star sparkle 3 (parallax stars, particles).</summary>
        public const string Star3 = "star3";

        /// <summary>Speed streak (speed boost trail, star streaks).</summary>
        public const string Speed = "speed";

        /// <summary>The thrust flame frames in order, for an animation cycle.</summary>
        public static readonly string[] Fire = [Fire00, Fire01, Fire02, Fire03, Fire04, Fire05, Fire06, Fire07, Fire08, Fire09, Fire10, Fire11, Fire12, Fire13, Fire14, Fire15, Fire16, Fire17, Fire18, Fire19];

        /// <summary>The three star sparkles.</summary>
        public static readonly string[] Stars = [Star1, Star2, Star3];
    }

    /// <summary>The player's shield bubble at strength 1..3, and the bubble drawn on a shielded enemy.</summary>
    public static class Shields
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Shield bubble at strength 1.</summary>
        public const string Strength1 = "shield1";

        /// <summary>Shield bubble at strength 2.</summary>
        public const string Strength2 = "shield2";

        /// <summary>Shield bubble at strength 3.</summary>
        public const string Strength3 = "shield3";

        /// <summary>The bubble drawn around a shielded enemy while it still has its shield (scaled down).</summary>
        public const string Enemy = "shield1";

        /// <summary>Gets the shield bubble for a strength.</summary>
        /// <param name="strength">The shield strength, 1..3.</param>
        /// <returns>The frame name, <c>shield1</c> to <c>shield3</c>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the strength is out of range.</exception>
        public static string ForStrength(int strength) => $"shield{AssetKeyChecks.Index(strength - 1, 3, nameof(strength)) + 1}";
    }

    /// <summary>Meteors for the showers (big = 2 hits, small = 1 hit) and their fragments.</summary>
    public static class Meteors
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Brown big meteor 1.</summary>
        public const string BrownBig1 = "meteorBrown_big1";

        /// <summary>Brown big meteor 2.</summary>
        public const string BrownBig2 = "meteorBrown_big2";

        /// <summary>Brown big meteor 3.</summary>
        public const string BrownBig3 = "meteorBrown_big3";

        /// <summary>Brown big meteor 4.</summary>
        public const string BrownBig4 = "meteorBrown_big4";

        /// <summary>Grey big meteor 1.</summary>
        public const string GreyBig1 = "meteorGrey_big1";

        /// <summary>Grey big meteor 2.</summary>
        public const string GreyBig2 = "meteorGrey_big2";

        /// <summary>Grey big meteor 3.</summary>
        public const string GreyBig3 = "meteorGrey_big3";

        /// <summary>Grey big meteor 4.</summary>
        public const string GreyBig4 = "meteorGrey_big4";

        /// <summary>Medium meteor (meteorBrown_med1).</summary>
        public const string BrownMed1 = "meteorBrown_med1";

        /// <summary>Medium meteor (meteorBrown_med3).</summary>
        public const string BrownMed3 = "meteorBrown_med3";

        /// <summary>Medium meteor (meteorGrey_med1).</summary>
        public const string GreyMed1 = "meteorGrey_med1";

        /// <summary>Medium meteor (meteorGrey_med2).</summary>
        public const string GreyMed2 = "meteorGrey_med2";

        /// <summary>Brown small meteor 1.</summary>
        public const string BrownSmall1 = "meteorBrown_small1";

        /// <summary>Brown small meteor 2.</summary>
        public const string BrownSmall2 = "meteorBrown_small2";

        /// <summary>Brown tiny meteor 1 (debris).</summary>
        public const string BrownTiny1 = "meteorBrown_tiny1";

        /// <summary>Brown tiny meteor 2 (debris).</summary>
        public const string BrownTiny2 = "meteorBrown_tiny2";

        /// <summary>Grey small meteor 1.</summary>
        public const string GreySmall1 = "meteorGrey_small1";

        /// <summary>Grey small meteor 2.</summary>
        public const string GreySmall2 = "meteorGrey_small2";

        /// <summary>Grey tiny meteor 1 (debris).</summary>
        public const string GreyTiny1 = "meteorGrey_tiny1";

        /// <summary>Grey tiny meteor 2 (debris).</summary>
        public const string GreyTiny2 = "meteorGrey_tiny2";

        /// <summary>The big meteors, for random picks.</summary>
        public static readonly string[] Big = [BrownBig1, BrownBig2, BrownBig3, BrownBig4, GreyBig1, GreyBig2, GreyBig3, GreyBig4];

        /// <summary>The small meteors, for random picks.</summary>
        public static readonly string[] Small = [BrownSmall1, BrownSmall2, GreySmall1, GreySmall2];

        /// <summary>The tiny meteors, for debris particles.</summary>
        public static readonly string[] Tiny = [BrownTiny1, BrownTiny2, GreyTiny1, GreyTiny2];
    }

    /// <summary>The seven power-up drops, in <c>PowerUpKind</c> order.</summary>
    public static class PowerUps
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Spread shot drop.</summary>
        public const string SpreadShot = "powerupBlue_star";

        /// <summary>Rapid fire drop.</summary>
        public const string RapidFire = "powerupYellow_bolt";

        /// <summary>Piercing laser drop.</summary>
        public const string PiercingLaser = "powerupGreen_bolt";

        /// <summary>Shield bubble drop.</summary>
        public const string ShieldBubble = "powerupBlue_shield";

        /// <summary>Speed boost drop.</summary>
        public const string SpeedBoost = "powerupYellow_star";

        /// <summary>Extra life drop.</summary>
        public const string ExtraLife = "pill_green";

        /// <summary>Bomb drop.</summary>
        public const string Bomb = "powerupRed_star";

        /// <summary>The drops indexed by <c>PowerUpKind</c> (0 = spread shot .. 6 = bomb).</summary>
        public static readonly string[] ByKind = [SpreadShot, RapidFire, PiercingLaser, ShieldBubble, SpeedBoost, ExtraLife, Bomb];

        /// <summary>Gets the drop frame for a power-up kind.</summary>
        /// <param name="kind">The <c>PowerUpKind</c> value, 0..6.</param>
        /// <returns>The frame name.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the kind is out of range.</exception>
        public static string ForKind(int kind) => ByKind[AssetKeyChecks.Index(kind, ByKind.Length, nameof(kind))];
    }

    /// <summary>HUD and menu pictures: numerals, buttons, the cursor, medals and the bomb stock icon.</summary>
    public static class Ui
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Numeral 0.</summary>
        public const string Numeral0 = "numeral0";

        /// <summary>Numeral 1.</summary>
        public const string Numeral1 = "numeral1";

        /// <summary>Numeral 2.</summary>
        public const string Numeral2 = "numeral2";

        /// <summary>Numeral 3.</summary>
        public const string Numeral3 = "numeral3";

        /// <summary>Numeral 4.</summary>
        public const string Numeral4 = "numeral4";

        /// <summary>Numeral 5.</summary>
        public const string Numeral5 = "numeral5";

        /// <summary>Numeral 6.</summary>
        public const string Numeral6 = "numeral6";

        /// <summary>Numeral 7.</summary>
        public const string Numeral7 = "numeral7";

        /// <summary>Numeral 8.</summary>
        public const string Numeral8 = "numeral8";

        /// <summary>Numeral 9.</summary>
        public const string Numeral9 = "numeral9";

        /// <summary>The multiplication sign ("x3" lives, chain multiplier).</summary>
        public const string NumeralX = "numeralX";

        /// <summary>Blue menu button (222 x 39).</summary>
        public const string ButtonBlue = "buttonBlue";

        /// <summary>Green menu button (222 x 39).</summary>
        public const string ButtonGreen = "buttonGreen";

        /// <summary>Red menu button (222 x 39).</summary>
        public const string ButtonRed = "buttonRed";

        /// <summary>Yellow menu button (222 x 39).</summary>
        public const string ButtonYellow = "buttonYellow";

        /// <summary>The menu cursor.</summary>
        public const string Cursor = "cursor";

        /// <summary>HUD bomb stock icon.</summary>
        public const string BombIcon = "powerupRed_star";

        /// <summary>Bronze star medal (high scores).</summary>
        public const string StarBronze = "star_bronze";

        /// <summary>Silver star medal (high scores).</summary>
        public const string StarSilver = "star_silver";

        /// <summary>Gold star medal (high scores).</summary>
        public const string StarGold = "star_gold";

        /// <summary>Bronze shield medal.</summary>
        public const string ShieldBronze = "shield_bronze";

        /// <summary>Silver shield medal.</summary>
        public const string ShieldSilver = "shield_silver";

        /// <summary>Gold shield medal.</summary>
        public const string ShieldGold = "shield_gold";

        /// <summary>The numerals 0..9 indexed by digit.</summary>
        public static readonly string[] Numerals = [Numeral0, Numeral1, Numeral2, Numeral3, Numeral4, Numeral5, Numeral6, Numeral7, Numeral8, Numeral9];

        /// <summary>The menu buttons.</summary>
        public static readonly string[] Buttons = [ButtonBlue, ButtonGreen, ButtonRed, ButtonYellow];
    }

    /// <summary>Turret pieces from the remastered atlas (boss turrets: a base with a gun on top).</summary>
    public static class Turrets
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Big turret base.</summary>
        public const string BaseBig = "turretBase_big";

        /// <summary>Small turret base.</summary>
        public const string BaseSmall = "turretBase_small";

        /// <summary>Turret gun 00.</summary>
        public const string Gun00 = "gun00";

        /// <summary>Turret gun 01.</summary>
        public const string Gun01 = "gun01";

        /// <summary>Turret gun 02.</summary>
        public const string Gun02 = "gun02";

        /// <summary>Turret gun 03.</summary>
        public const string Gun03 = "gun03";

        /// <summary>Turret gun 04.</summary>
        public const string Gun04 = "gun04";

        /// <summary>Turret gun 05.</summary>
        public const string Gun05 = "gun05";

        /// <summary>Turret gun 06.</summary>
        public const string Gun06 = "gun06";

        /// <summary>Turret gun 07.</summary>
        public const string Gun07 = "gun07";

        /// <summary>Turret gun 08.</summary>
        public const string Gun08 = "gun08";

        /// <summary>Turret gun 09.</summary>
        public const string Gun09 = "gun09";

        /// <summary>Turret gun 10.</summary>
        public const string Gun10 = "gun10";

        /// <summary>The turret guns in order.</summary>
        public static readonly string[] Guns = [Gun00, Gun01, Gun02, Gun03, Gun04, Gun05, Gun06, Gun07, Gun08, Gun09, Gun10];
    }

    /// <summary>
    /// Boss pieces from the extension atlas: one core ship per sector design, cannons, missile bays, homing
    /// missiles and smoke puffs for the explosion.
    /// </summary>
    /// <remarks>
    /// DESIGN.md sizes (world units): core 160 x 96, turret 56 x 56, cannon 64 x 40, missile bay 56 x 48, missile
    /// 14 x 14. The frames are drawn scaled to those boxes. The turrets come from <see cref="Turrets"/>.
    /// </remarks>
    public static class Bosses
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-extension/Spritesheet/spaceShooter2_spritesheet";

        /// <summary>Core ship of the sector-design-1 boss (106 x 80).</summary>
        public const string Core1 = "spaceShips_001";

        /// <summary>Core ship of the sector-design-2 boss (136 x 84).</summary>
        public const string Core2 = "spaceShips_005";

        /// <summary>Core ship of the sector-design-3 boss (126 x 108).</summary>
        public const string Core3 = "spaceShips_004";

        /// <summary>Core ship of the sector-design-4 boss (114 x 82).</summary>
        public const string Core4 = "spaceShips_009";

        /// <summary>Core ship of the sector-design-5 boss (172 x 151).</summary>
        public const string Core5 = "spaceShips_007";

        /// <summary>Spare extension ship 2 (escorts, title-screen fleet).</summary>
        public const string Escort2 = "spaceShips_002";

        /// <summary>Spare extension ship 3 (escorts, title-screen fleet).</summary>
        public const string Escort3 = "spaceShips_003";

        /// <summary>Spare extension ship 6 (escorts, title-screen fleet).</summary>
        public const string Escort6 = "spaceShips_006";

        /// <summary>Spare extension ship 8 (escorts, title-screen fleet).</summary>
        public const string Escort8 = "spaceShips_008";

        /// <summary>Cannon section (60 x 38).</summary>
        public const string Cannon = "spaceStation_031";

        /// <summary>Cannon barrel / armour plate (84 x 36).</summary>
        public const string CannonBarrel = "spaceStation_029";

        /// <summary>Missile bay section (52 x 34).</summary>
        public const string MissileBay = "spaceStation_030";

        /// <summary>Armour plate across the core (124 x 40).</summary>
        public const string ArmourPlate = "spaceStation_001";

        /// <summary>Wing piece for the left side (50 x 67).</summary>
        public const string SectionWingLeft = "spaceParts_014";

        /// <summary>Wing piece for the right side (50 x 67).</summary>
        public const string SectionWingRight = "spaceParts_015";

        /// <summary>A homing missile (16 x 22).</summary>
        public const string HomingMissile = "spaceMissiles_012";

        /// <summary>Alternative homing missile (12 x 25).</summary>
        public const string HomingMissileAlt = "spaceMissiles_015";

        /// <summary>A boss missile-bay missile (20 x 35).</summary>
        public const string BossMissile = "spaceMissiles_001";

        /// <summary>Smoke/explosion puff 008.</summary>
        public const string Puff008 = "spaceEffects_008";

        /// <summary>Smoke/explosion puff 009.</summary>
        public const string Puff009 = "spaceEffects_009";

        /// <summary>Smoke/explosion puff 010.</summary>
        public const string Puff010 = "spaceEffects_010";

        /// <summary>Smoke/explosion puff 011.</summary>
        public const string Puff011 = "spaceEffects_011";

        /// <summary>Smoke/explosion puff 012.</summary>
        public const string Puff012 = "spaceEffects_012";

        /// <summary>Smoke/explosion puff 013.</summary>
        public const string Puff013 = "spaceEffects_013";

        /// <summary>Smoke/explosion puff 014.</summary>
        public const string Puff014 = "spaceEffects_014";

        /// <summary>Smoke/explosion puff 015.</summary>
        public const string Puff015 = "spaceEffects_015";

        /// <summary>Smoke/explosion puff 016.</summary>
        public const string Puff016 = "spaceEffects_016";

        /// <summary>Exhaust flame 1.</summary>
        public const string Flame1 = "spaceEffects_001";

        /// <summary>Exhaust flame 2.</summary>
        public const string Flame2 = "spaceEffects_002";

        /// <summary>Exhaust flame 3.</summary>
        public const string Flame3 = "spaceEffects_003";

        /// <summary>Exhaust flame 4.</summary>
        public const string Flame4 = "spaceEffects_004";

        /// <summary>An astronaut (optional rescue bonus).</summary>
        public const string Astronaut = "spaceAstronauts_001";

        /// <summary>The core ships indexed by sector design - 1 (design 1..5).</summary>
        public static readonly string[] Cores = [Core1, Core2, Core3, Core4, Core5];

        /// <summary>The explosion puffs from small to large, for particle bursts.</summary>
        public static readonly string[] Puffs = [Puff008, Puff009, Puff010, Puff011, Puff012, Puff013, Puff014, Puff015, Puff016];

        /// <summary>Gets the core ship of a sector design's boss.</summary>
        /// <param name="design">The sector design, 1..5 (loops repeat the five designs).</param>
        /// <returns>The frame name.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the design is out of range.</exception>
        public static string CoreForDesign(int design) => Cores[AssetKeyChecks.Index(design - 1, Cores.Length, nameof(design))];
    }

    /// <summary>
    /// The upgrade parts from the remastered pack that are bolted onto the player's ship while a power-up is active
    /// (the extension pack's parts are in <see cref="ShipUpgradeParts"/>).
    /// </summary>
    public static class ShipUpgrades
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-remastered/Spritesheet/sheet";

        /// <summary>Rapid fire: the long cannon barrel under the nose (10 x 47, muzzle drawn pointing down; rotate 180).</summary>
        public const string NoseCannon = "gun08";

        /// <summary>Speed boost: an extra engine nacelle under each wing (38 x 23).</summary>
        public const string BoostEngine = "engine1";

        /// <summary>Speed boost: blue engine flame, animation frame 1 (14 x 31).</summary>
        public const string BoostFlame1 = "fire04";

        /// <summary>Speed boost: blue engine flame, animation frame 2 (14 x 31).</summary>
        public const string BoostFlame2 = "fire05";

        /// <summary>Speed boost: blue engine flame, animation frame 3 (14 x 31).</summary>
        public const string BoostFlame3 = "fire14";

        /// <summary>Speed boost: blue engine flame, animation frame 4 (14 x 31).</summary>
        public const string BoostFlame4 = "fire15";

        /// <summary>Piercing laser: the green charge glowing on the tip of each emitter barrel (13 x 37, drawn small; the piercing bolt's colour).</summary>
        public const string EmitterGlow = "laserGreen08";

        /// <summary>The boost flame frames in animation order.</summary>
        public static readonly string[] BoostFlames = [BoostFlame1, BoostFlame2, BoostFlame3, BoostFlame4];
    }

    /// <summary>The upgrade parts from the extension pack that are bolted onto the player's ship while a power-up is active.</summary>
    public static class ShipUpgradeParts
    {
        /// <summary>The atlas these frames are cut from.</summary>
        public const string Atlas = "kenney:space-shooter-extension/Spritesheet/spaceShooter2_spritesheet";

        /// <summary>Spread shot: the gun pod on each wing tip (16 x 38, barrel up).</summary>
        public const string SpreadPod = "spaceParts_093";

        /// <summary>Piercing laser: the long emitter barrel either side of the cockpit (11 x 30, barrel up).</summary>
        public const string EmitterBarrel = "spaceParts_092";
    }

    /// <summary>The tiling space backgrounds (loose 256 x 256 images; draw <c>sheet[0, 0]</c>).</summary>
    public static class Backgrounds
    {
        /// <summary>The black background.</summary>
        public const string Black = "kenney:space-shooter-remastered/Backgrounds/black";

        /// <summary>The blue background.</summary>
        public const string Blue = "kenney:space-shooter-remastered/Backgrounds/blue";

        /// <summary>The darkPurple background.</summary>
        public const string DarkPurple = "kenney:space-shooter-remastered/Backgrounds/darkPurple";

        /// <summary>The purple background.</summary>
        public const string Purple = "kenney:space-shooter-remastered/Backgrounds/purple";

        /// <summary>The backgrounds indexed by <see cref="SpaceBackground"/>.</summary>
        public static readonly string[] All = [Black, Blue, DarkPurple, Purple];
    }

    /// <summary>The ten planets (loose 1280 px images for splash, title and briefing backdrops; draw <c>sheet[0, 0]</c>).</summary>
    public static class Planets
    {
        /// <summary>Planet 00.</summary>
        public const string Planet00 = "kenney:planets/Planets/planet00";

        /// <summary>Planet 01.</summary>
        public const string Planet01 = "kenney:planets/Planets/planet01";

        /// <summary>Planet 02.</summary>
        public const string Planet02 = "kenney:planets/Planets/planet02";

        /// <summary>Planet 03.</summary>
        public const string Planet03 = "kenney:planets/Planets/planet03";

        /// <summary>Planet 04.</summary>
        public const string Planet04 = "kenney:planets/Planets/planet04";

        /// <summary>Planet 05.</summary>
        public const string Planet05 = "kenney:planets/Planets/planet05";

        /// <summary>Planet 06.</summary>
        public const string Planet06 = "kenney:planets/Planets/planet06";

        /// <summary>Planet 07.</summary>
        public const string Planet07 = "kenney:planets/Planets/planet07";

        /// <summary>Planet 08.</summary>
        public const string Planet08 = "kenney:planets/Planets/planet08";

        /// <summary>Planet 09.</summary>
        public const string Planet09 = "kenney:planets/Planets/planet09";

        /// <summary>The planets indexed 0..9.</summary>
        public static readonly string[] All = [Planet00, Planet01, Planet02, Planet03, Planet04, Planet05, Planet06, Planet07, Planet08, Planet09];
    }

    /// <summary>The Kenney text fonts from the remastered pack's Bonus folder (both carry basic Latin).</summary>
    public static class Fonts
    {
        /// <summary>Kenvector Future: titles, HUD numbers, menus.</summary>
        public const string Future = "kenney:space-shooter-remastered/Bonus/kenvector_future";

        /// <summary>Kenvector Future Thin: body text, credits, briefings.</summary>
        public const string FutureThin = "kenney:space-shooter-remastered/Bonus/kenvector_future_thin";
    }

    /// <summary>
    /// Every sound effect key. Each is also the engine audio key the sound is registered under by
    /// <see cref="BrixInvadersAssets.LoadSounds"/>; <see cref="SoundEffects.KeyOf"/> maps a <see cref="SoundEffect"/> to one.
    /// </summary>
    public static class Sfx
    {
        /// <summary>The player fires.</summary>
        public const string PlayerLaser = "kenney:space-shooter-remastered/Bonus/sfx_laser1";

        /// <summary>An enemy fires a bolt.</summary>
        public const string EnemyLaser = "kenney:space-shooter-remastered/Bonus/sfx_laser2";

        /// <summary>A boss section fires.</summary>
        public const string BossLaser = "kenney:sci-fi-sounds/Audio/laserLarge_000";

        /// <summary>A homing missile is launched.</summary>
        public const string MissileLaunch = "kenney:sci-fi-sounds/Audio/thrusterFire_000";

        /// <summary>A formation enemy, missile or small meteor is destroyed.</summary>
        public const string ExplosionSmall = "kenney:sci-fi-sounds/Audio/explosionCrunch_000";

        /// <summary>A big meteor or the UFO is destroyed.</summary>
        public const string ExplosionMedium = "kenney:sci-fi-sounds/Audio/explosionCrunch_002";

        /// <summary>The player's ship is destroyed.</summary>
        public const string ExplosionLarge = "kenney:sci-fi-sounds/Audio/lowFrequency_explosion_000";

        /// <summary>The UFO flies across the lane (loop it while it is on screen).</summary>
        public const string Ufo = "kenney:sci-fi-sounds/Audio/spaceEngineLow_000";

        /// <summary>A power-up is collected.</summary>
        public const string PowerUpPickup = "kenney:digital-audio/Audio/powerUp2";

        /// <summary>A power-up drop falls off the screen or times out.</summary>
        public const string PowerUpLost = "kenney:digital-audio/Audio/phaserDown3";

        /// <summary>The player's shield gains strength.</summary>
        public const string ShieldUp = "kenney:space-shooter-remastered/Bonus/sfx_shieldUp";

        /// <summary>The player's shield absorbs a hit.</summary>
        public const string ShieldHit = "kenney:space-shooter-remastered/Bonus/sfx_shieldDown";

        /// <summary>The player's hull is hit.</summary>
        public const string PlayerHit = "kenney:space-shooter-remastered/Bonus/sfx_zap";

        /// <summary>A shielded enemy loses its shield.</summary>
        public const string EnemyShieldBroken = "kenney:sci-fi-sounds/Audio/forceField_001";

        /// <summary>A bolt hits boss armour.</summary>
        public const string ArmourHit = "kenney:sci-fi-sounds/Audio/impactMetal_002";

        /// <summary>A boss section takes damage.</summary>
        public const string BossHit = "kenney:sci-fi-sounds/Audio/impactMetal_000";

        /// <summary>A boss section is destroyed.</summary>
        public const string BossSectionDestroyed = "kenney:sci-fi-sounds/Audio/explosionCrunch_004";

        /// <summary>The boss enters its next phase.</summary>
        public const string BossPhase = "kenney:digital-audio/Audio/phaserDown2";

        /// <summary>The boss is defeated.</summary>
        public const string BossExplosion = "kenney:sci-fi-sounds/Audio/lowFrequency_explosion_001";

        /// <summary>The boss warning stinger.</summary>
        public const string BossWarning = "kenney:digital-audio/Audio/lowThreeTone";

        /// <summary>The player detonates a bomb.</summary>
        public const string Bomb = "kenney:sci-fi-sounds/Audio/forceField_000";

        /// <summary>An extra life is awarded.</summary>
        public const string ExtraLife = "kenney:digital-audio/Audio/threeTone1";

        /// <summary>The chain multiplier goes up.</summary>
        public const string ChainUp = "kenney:digital-audio/Audio/pepSound1";

        /// <summary>The chain is broken.</summary>
        public const string ChainBroken = "kenney:digital-audio/Audio/lowDown";

        /// <summary>A wave starts.</summary>
        public const string WaveStart = "kenney:digital-audio/Audio/twoTone1";

        /// <summary>A sector is cleared.</summary>
        public const string SectorClear = "kenney:digital-audio/Audio/powerUp7";

        /// <summary>The formation reaches the player's line.</summary>
        public const string FormationLanded = "kenney:digital-audio/Audio/zapThreeToneDown";

        /// <summary>Formation march note 0 (the classic four-note march).</summary>
        public const string March0 = "kenney:digital-audio/Audio/pepSound2";

        /// <summary>Formation march note 1.</summary>
        public const string March1 = "kenney:digital-audio/Audio/pepSound3";

        /// <summary>Formation march note 2.</summary>
        public const string March2 = "kenney:digital-audio/Audio/pepSound4";

        /// <summary>Formation march note 3.</summary>
        public const string March3 = "kenney:digital-audio/Audio/pepSound5";

        /// <summary>The menu cursor moves.</summary>
        public const string MenuMove = "kenney:digital-audio/Audio/tone1";

        /// <summary>A menu item is chosen.</summary>
        public const string MenuConfirm = "kenney:digital-audio/Audio/highUp";

        /// <summary>Back out of a menu.</summary>
        public const string MenuBack = "kenney:digital-audio/Audio/highDown";

        /// <summary>Game over.</summary>
        public const string GameOver = "kenney:space-shooter-remastered/Bonus/sfx_lose";

        /// <summary>A new high score is entered.</summary>
        public const string HighScore = "kenney:digital-audio/Audio/zapThreeToneUp";
    }

    /// <summary>The packs' own preview images (loose images) for the Kenney cards.</summary>
    public static class Promo
    {
        /// <summary>The remastered pack's preview image.</summary>
        public const string RemasteredPreview = "kenney:space-shooter-remastered/preview";

        /// <summary>The remastered pack's sample scene.</summary>
        public const string RemasteredSample = "kenney:space-shooter-remastered/sample";

        /// <summary>The extension pack's preview image.</summary>
        public const string ExtensionPreview = "kenney:space-shooter-extension/Preview";

        /// <summary>The extension pack's sample scene.</summary>
        public const string ExtensionSample = "kenney:space-shooter-extension/Sample";

        /// <summary>The planets pack's preview image.</summary>
        public const string PlanetsPreview = "kenney:planets/Preview";

        /// <summary>The planets pack's sample image.</summary>
        public const string PlanetsSample = "kenney:planets/Sample";
    }
}
