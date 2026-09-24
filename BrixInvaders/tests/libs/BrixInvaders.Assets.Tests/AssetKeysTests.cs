using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.KenneyAssets.Sources;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class AssetKeysTests
{
    private readonly GameAssetProviderRegistry _providers;

    public AssetKeysTests()
    {
        TestAssets.Register();
        _providers = TestAssets.Engine.Managers.AssetProviders;
    }

    [Fact]
    public void every_key_resolves_through_the_provider_with_its_exact_spelling()
    {
        //Arrange
        List<string> problems = [];

        //Act
        foreach (string key in AssetKeyCatalog.AllKeys)
        {
            if (!_providers.TryDescribe(key, out GameAssetDescriptor descriptor) || descriptor is null)
            {
                problems.Add($"not found: {key}");
            }
            else if (!string.Equals(descriptor.Key, key, StringComparison.Ordinal))
            {
                problems.Add($"spelled '{key}', provider says '{descriptor.Key}'");
            }
            else if (descriptor.Properties[KenneyAssetProperties.Materializable] != "true")
            {
                problems.Add($"not materializable: {key}");
            }
        }

        //Assert
        problems.Should().BeEmpty();
        AssetKeyCatalog.AllKeys.Should().Contain(AssetKeys.Atlases.Main);
    }

    [Fact]
    public void every_key_is_of_the_kind_its_group_expects()
    {
        //Arrange
        Dictionary<string, GameAssetKind> expected = new(StringComparer.Ordinal)
        {
            [AssetKeys.Atlases.Main] = GameAssetKind.SpriteAtlas,
            [AssetKeys.Atlases.Extension] = GameAssetKind.SpriteAtlas,
        };
        foreach (string key in AssetKeys.Backgrounds.All.Concat(AssetKeys.Planets.All)) { expected[key] = GameAssetKind.Image; }
        foreach (string key in SoundEffects.Keys.Values) { expected[key] = GameAssetKind.Audio; }
        expected[AssetKeys.Fonts.Future] = GameAssetKind.Font;
        expected[AssetKeys.Fonts.FutureThin] = GameAssetKind.Font;

        //Act
        List<string> wrong = expected
            .Where(pair => !_providers.TryDescribe(pair.Key, out GameAssetDescriptor descriptor) || descriptor.Kind != pair.Value)
            .Select(pair => pair.Key)
            .ToList();

        //Assert
        wrong.Should().BeEmpty();
    }

    [Fact]
    public void every_frame_the_game_relies_on_exists_in_its_atlas()
    {
        //Arrange
        List<string> missing = [];

        //Act
        foreach (AtlasFrameGroup group in AssetKeyCatalog.FrameGroups)
        {
            Tilesheet atlas = BrixInvadersAssets.LoadImage(TestAssets.Engine, group.AtlasKey);
            missing.AddRange(group.FrameNames
                .Where(frame => atlas.GetRegion(frame) is null)
                .Select(frame => $"{group.GroupName}: {frame}"));
        }

        //Assert
        missing.Should().BeEmpty();
        AssetKeyCatalog.FrameGroups.Select(group => group.GroupName).Should().Contain(
            ["Ships", "Damage", "Enemies", "Ufos", "Lasers", "Effects", "Shields", "Meteors", "PowerUps", "Ui", "Turrets", "Bosses", "ShipUpgrades", "ShipUpgradeParts"]);
    }

    [Fact]
    public void every_helper_combination_names_an_existing_frame()
    {
        //Arrange
        Tilesheet main = BrixInvadersAssets.LoadMainAtlas(TestAssets.Engine);
        Tilesheet boss = BrixInvadersAssets.LoadBossSheet(TestAssets.Engine);
        List<string> frames = [];

        //Act
        for (int shape = 0; shape < 3; shape++)
        {
            for (int colour = 0; colour < 4; colour++)
            {
                frames.Add(AssetKeys.Ships.Player(shape, colour));
                frames.Add(AssetKeys.Ships.Life(shape, colour));
            }

            for (int tier = 1; tier <= 3; tier++) { frames.Add(AssetKeys.Damage.PlayerOverlay(shape, tier)); }
        }

        for (int role = 0; role < 5; role++)
        {
            for (int colour = 0; colour < 4; colour++) { frames.Add(AssetKeys.Enemies.Enemy(role, colour)); }
        }

        for (int strength = 1; strength <= 3; strength++) { frames.Add(AssetKeys.Shields.ForStrength(strength)); }
        for (int kind = 0; kind < 7; kind++) { frames.Add(AssetKeys.PowerUps.ForKind(kind)); }

        //Assert
        frames.Where(frame => main.GetRegion(frame) is null).Should().BeEmpty();
        frames.Should().HaveCount(24 + 9 + 20 + 3 + 7);
        Enumerable.Range(1, 5).Select(AssetKeys.Bosses.CoreForDesign).Where(frame => boss.GetRegion(frame) is null)
            .Should().BeEmpty();
    }

    [Fact]
    public void helpers_follow_the_game_logic_orders()
    {
        //Arrange
        const int diver = 3;
        const int green = 2;

        //Act
        string enemy = AssetKeys.Enemies.Enemy(diver, green);

        //Assert
        enemy.Should().Be("enemyGreen4");
        AssetKeys.Ships.Player(0, 3).Should().Be("playerShip1_red");
        AssetKeys.Damage.PlayerOverlay(2, 1).Should().Be("playerShip3_damage1");
        AssetKeys.PowerUps.ForKind(6).Should().Be(AssetKeys.PowerUps.Bomb);
        AssetKeys.Bosses.CoreForDesign(5).Should().Be(AssetKeys.Bosses.Core5);
    }

    [Fact]
    public void helpers_reject_out_of_range_indexes()
    {
        //Arrange
        Action[] calls =
        [
            () => AssetKeys.Ships.Player(3, 0),
            () => AssetKeys.Ships.Life(0, 4),
            () => AssetKeys.Damage.PlayerOverlay(0, 0),
            () => AssetKeys.Enemies.Enemy(5, 0),
            () => AssetKeys.Enemies.Enemy(0, -1),
            () => AssetKeys.Shields.ForStrength(4),
            () => AssetKeys.PowerUps.ForKind(7),
            () => AssetKeys.Bosses.CoreForDesign(0),
        ];

        //Act
        int rejected = calls.Count(call =>
        {
            try { call(); return false; }
            catch (ArgumentOutOfRangeException) { return true; }
        });

        //Assert
        rejected.Should().Be(calls.Length);
    }

    [Fact]
    public void atlas_frames_materialize_with_their_atlas_sizes()
    {
        //Arrange
        Tilesheet main = BrixInvadersAssets.LoadShipSheet(TestAssets.Engine);

        //Act
        var ship = BrixInvadersAssets.GetFrameSize(main, AssetKeys.Ships.PlayerShip1Blue);
        var enemy = BrixInvadersAssets.GetFrameSize(main, AssetKeys.Enemies.GruntRed);
        var core = BrixInvadersAssets.GetFrameSize(BrixInvadersAssets.LoadBossSheet(TestAssets.Engine), AssetKeys.Bosses.Core5);

        //Assert
        (ship.Width, ship.Height).Should().Be((99, 75));
        (enemy.Width, enemy.Height).Should().Be((93, 84));
        (core.Width, core.Height).Should().Be((172, 151));
        BrixInvadersAssets.LoadEnemySheet(TestAssets.Engine).Should().BeSameAs(main);
        BrixInvadersAssets.GetFrame(main, AssetKeys.Lasers.PlayerBolt).Tilesheet.Should().BeSameAs(main);
    }

    [Fact]
    public void ship_upgrade_frames_and_hulls_have_the_sizes_the_loadout_is_laid_out_for()
    {
        //Arrange
        Tilesheet main = BrixInvadersAssets.LoadMainAtlas(TestAssets.Engine);
        Tilesheet extension = BrixInvadersAssets.LoadBossSheet(TestAssets.Engine);

        //Act
        (int, int) Size(Tilesheet atlas, string frame)
        {
            var size = BrixInvadersAssets.GetFrameSize(atlas, frame);
            return (size.Width, size.Height);
        }

        //Assert
        Size(main, AssetKeys.Ships.PlayerShip1Blue).Should().Be((99, 75));
        Size(main, AssetKeys.Ships.PlayerShip2Blue).Should().Be((112, 75));
        Size(main, AssetKeys.Ships.PlayerShip3Blue).Should().Be((98, 75));
        Size(main, AssetKeys.ShipUpgrades.NoseCannon).Should().Be((10, 47));
        Size(main, AssetKeys.ShipUpgrades.BoostEngine).Should().Be((38, 23));
        Size(main, AssetKeys.ShipUpgrades.EmitterGlow).Should().Be((13, 37));
        AssetKeys.ShipUpgrades.BoostFlames.Select(frame => Size(main, frame)).Distinct().Should().Equal((14, 31));
        Size(extension, AssetKeys.ShipUpgradeParts.SpreadPod).Should().Be((16, 38));
        Size(extension, AssetKeys.ShipUpgradeParts.EmitterBarrel).Should().Be((11, 30));
        AssetKeyCatalog.FrameGroups.Single(group => group.GroupName == nameof(AssetKeys.ShipUpgradeParts)).AtlasKey
            .Should().Be(AssetKeys.Atlases.Extension);
    }

    [Fact]
    public void loose_images_materialize_as_one_whole_image_tile()
    {
        //Arrange
        var engine = TestAssets.Engine;

        //Act
        Tilesheet planet = BrixInvadersAssets.LoadPlanet(engine, 9);
        Tilesheet[] backgrounds = Enum.GetValues<SpaceBackground>()
            .Select(background => BrixInvadersAssets.LoadBackground(engine, background))
            .ToArray();
        Tilesheet preview = BrixInvadersAssets.LoadImage(engine, AssetKeys.Promo.RemasteredPreview);

        //Assert
        planet.Name.Should().Be(AssetKeys.Planets.Planet09);
        planet.DefaultRegion.TileSize.Width.Should().Be(1280);
        backgrounds.Should().OnlyContain(sheet => sheet.DefaultRegion.TileSize.Width == 256);
        preview.DefaultRegion.TileSize.Width.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LoadPlanet_rejects_an_index_outside_0_to_9() =>
        ((Action)(() => BrixInvadersAssets.LoadPlanet(TestAssets.Engine, 10))).Should().Throw<ArgumentOutOfRangeException>();

    [Fact]
    public void GetFrame_names_the_atlas_and_frame_when_the_frame_is_missing()
    {
        //Arrange
        Tilesheet main = BrixInvadersAssets.LoadMainAtlas(TestAssets.Engine);

        //Act
        Action act = () => BrixInvadersAssets.GetFrame(main, "noSuchFrame");

        //Assert
        act.Should().Throw<ArgumentException>().Which.Message.Should().Contain("noSuchFrame");
    }
}
