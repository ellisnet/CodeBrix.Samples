using System;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Rendering;

public class SpriteCatalogTests
{
    private static void ShouldBePinnedFrame(string imageKey) =>
        SpriteCatalog.AllAtlasFrames.Should().Contain(imageKey, $"'{imageKey}' must be an atlas frame the Assets library pins");

    [Fact]
    public void every_enemy_role_and_colour_has_a_frame()
    {
        //Assert
        foreach (var role in Enum.GetValues<EnemyRole>())
        {
            foreach (var colour in Enum.GetValues<EnemyColour>())
            {
                ShouldBePinnedFrame(SpriteCatalog.Enemy(role, colour));
            }
        }
    }

    [Fact]
    public void the_five_roles_are_five_different_shapes() =>
        Enum.GetValues<EnemyRole>().Select(role => SpriteCatalog.Enemy(role, EnemyColour.Red)).Distinct().Count().Should().Be(5);

    [Fact]
    public void every_ship_shape_and_colour_has_a_ship_and_a_life_icon()
    {
        //Assert
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            for (var colour = 0; colour < GameSetup.ShipColourCount; colour++)
            {
                ShouldBePinnedFrame(SpriteCatalog.PlayerShip(shape, colour));
                ShouldBePinnedFrame(SpriteCatalog.LifeIcon(shape, colour));
            }
        }
    }

    [Fact]
    public void DamageOverlay_shows_at_two_thirds_and_one_third_hull_only()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Assert
            SpriteCatalog.DamageOverlay(shape, 0).Should().BeNull();
            ShouldBePinnedFrame(SpriteCatalog.DamageOverlay(shape, 1));
            ShouldBePinnedFrame(SpriteCatalog.DamageOverlay(shape, 2));
        }
    }

    [Fact]
    public void every_power_up_has_a_frame()
    {
        //Assert
        foreach (var kind in Enum.GetValues<PowerUpKind>())
        {
            ShouldBePinnedFrame(SpriteCatalog.PowerUp(kind));
        }
    }

    [Fact]
    public void every_boss_part_of_every_design_has_a_frame()
    {
        //Assert
        foreach (var kind in Enum.GetValues<BossSectionKind>())
        {
            for (var design = 1; design <= SectorRules.SectorsPerLoop; design++)
            {
                ShouldBePinnedFrame(SpriteCatalog.BossSection(kind, design));
            }
        }

        ShouldBePinnedFrame(SpriteCatalog.BossTurretGun);
        ShouldBePinnedFrame(SpriteCatalog.BossArmour);
    }

    [Fact]
    public void every_boss_design_has_its_own_core_ship() =>
        Enumerable.Range(1, SectorRules.SectorsPerLoop).Select(design => SpriteCatalog.BossSection(BossSectionKind.Core, design))
            .Distinct().Count().Should().Be(SectorRules.SectorsPerLoop);

    [Fact]
    public void projectiles_shields_effects_and_ui_have_frames()
    {
        //Assert
        new[]
        {
            SpriteCatalog.PlayerBolt(false), SpriteCatalog.PlayerBolt(true), SpriteCatalog.EnemyBolt, SpriteCatalog.Missile,
            SpriteCatalog.PlayerImpact, SpriteCatalog.EnemyImpact, SpriteCatalog.EnemyShield, SpriteCatalog.BombIcon,
            SpriteCatalog.SpeedStreak, SpriteCatalog.Cursor,
        }.ToList().ForEach(ShouldBePinnedFrame);
        for (var strength = 1; strength <= 3; strength++)
        {
            ShouldBePinnedFrame(SpriteCatalog.PlayerShield(strength));
        }
    }

    [Fact]
    public void cycled_frames_wrap_for_any_index()
    {
        //Assert
        foreach (var index in new[] { -7, 0, 1, 2, 3, 4, 99 })
        {
            ShouldBePinnedFrame(SpriteCatalog.Ufo(index));
            ShouldBePinnedFrame(SpriteCatalog.Thrust(index));
            ShouldBePinnedFrame(SpriteCatalog.Star(index));
            ShouldBePinnedFrame(SpriteCatalog.Puff(index));
            ShouldBePinnedFrame(SpriteCatalog.Button(index));
            ShouldBePinnedFrame(SpriteCatalog.Meteor(index, MeteorSize.Big));
            ShouldBePinnedFrame(SpriteCatalog.Meteor(index, MeteorSize.Small));
        }
    }

    [Fact]
    public void loose_pictures_are_asset_keys_the_assets_library_pins() =>
        SpriteCatalog.LoosePictures.Should().OnlyContain(key => AssetKeyCatalog.AllKeys.Contains(key));

    [Fact]
    public void every_sector_has_a_planet_and_a_background()
    {
        for (var sector = 1; sector <= 12; sector++)
        {
            //Assert
            AssetKeys.Planets.All.Should().Contain(SpriteCatalog.SectorPlanet(sector));
            Enum.IsDefined(SpriteCatalog.SectorBackground(sector)).Should().BeTrue();
        }
    }

    [Fact]
    public void TrySplit_round_trips_a_frame_key()
    {
        //Act
        var split = SpriteCatalog.TrySplit(SpriteCatalog.Frame("atlas", "frame"), out var atlas, out var frame);
        var loose = SpriteCatalog.TrySplit(AssetKeys.Planets.Planet00, out var whole, out var none);

        //Assert
        split.Should().BeTrue();
        atlas.Should().Be("atlas");
        frame.Should().Be("frame");
        loose.Should().BeFalse();
        whole.Should().Be(AssetKeys.Planets.Planet00);
        none.Should().BeNull();
    }
}
