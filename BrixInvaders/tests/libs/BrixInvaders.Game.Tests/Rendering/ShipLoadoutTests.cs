using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Rendering;

public class ShipLoadoutTests
{
    private static IEnumerable<PowerUpKind[]> EveryCombination()
    {
        var kinds = Enum.GetValues<PowerUpKind>();
        for (var mask = 0; mask < 1 << kinds.Length; mask++)
        {
            yield return kinds.Where((kind, index) => (mask & (1 << index)) != 0).ToArray();
        }
    }

    [Fact]
    public void ChangesShip_only_for_the_four_capability_power_ups()
    {
        //Act
        var changing = Enum.GetValues<PowerUpKind>().Where(ShipLoadout.ChangesShip).ToArray();

        //Assert
        changing.Should().Equal(PowerUpKind.SpreadShot, PowerUpKind.RapidFire, PowerUpKind.PiercingLaser, PowerUpKind.SpeedBoost);
        ShipLoadout.VisibleKinds.Should().Equal(changing);
    }

    [Fact]
    public void PartsFor_every_combination_shows_exactly_the_parts_of_its_capability_power_ups_in_draw_order()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            foreach (var combination in EveryCombination())
            {
                //Act
                var parts = ShipLoadout.PartsFor(shape, combination);

                //Assert
                var expected = ShipLoadout.AllParts(shape).Where(part => combination.Contains(part.PowerUp)).ToArray();
                parts.Should().Equal(expected);
                parts.Select(part => part.PowerUp).Distinct().Should().BeEquivalentTo(combination.Where(ShipLoadout.ChangesShip));
                parts.Select(part => ((int)part.Layer * 1000) + part.Order).Should().BeInAscendingOrder();
            }
        }
    }

    [Fact]
    public void PartsFor_is_empty_without_a_capability_power_up()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Assert
            ShipLoadout.PartsFor(shape, []).Should().BeEmpty();
            ShipLoadout.PartsFor(shape, [PowerUpKind.ShieldBubble, PowerUpKind.ExtraLife, PowerUpKind.Bomb]).Should().BeEmpty();
        }
    }

    [Fact]
    public void every_capability_puts_its_own_distinct_parts_on_every_shape()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Arrange
            var framesByKind = ShipLoadout.VisibleKinds
                .Select(kind => ShipLoadout.PartsFor(shape, [kind]).SelectMany(part => part.Frames).ToHashSet())
                .ToArray();

            //Assert
            framesByKind.Should().OnlyContain(frames => frames.Count > 0);
            for (var a = 0; a < framesByKind.Length; a++)
            {
                for (var b = a + 1; b < framesByKind.Length; b++)
                {
                    framesByKind[a].Overlaps(framesByKind[b]).Should().BeFalse();
                }
            }
        }
    }

    [Fact]
    public void every_part_frame_is_an_atlas_frame_the_assets_library_pins()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Assert
            ShipLoadout.AllParts(shape).SelectMany(part => part.Frames)
                .Should().OnlyContain(frame => SpriteCatalog.AllAtlasFrames.Contains(frame));
        }
    }

    [Fact]
    public void parts_come_in_mirrored_pairs_except_the_nose_cannon()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Arrange
            var parts = ShipLoadout.AllParts(shape);

            //Act
            var unpaired = parts.Where(part => part.X != 0 &&
                !parts.Any(other => other.X == -part.X && other.Y == part.Y && other.Frames.SequenceEqual(part.Frames)))
                .ToArray();

            //Assert
            unpaired.Should().BeEmpty();
            parts.Where(part => part.X == 0).Select(part => part.Name).Should().Equal("nose cannon");
        }
    }

    [Fact]
    public void parts_sit_on_or_just_beyond_the_hull_of_every_shape()
    {
        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            //Arrange
            var halfWidth = ShipLoadout.HullArtWidth(shape) / 2;
            var halfHeight = ShipLoadout.HullArtHeight / 2;

            //Assert
            foreach (var part in ShipLoadout.AllParts(shape))
            {
                Math.Abs(part.X).Should().BeLessThanOrEqualTo(halfWidth, part.Name);
                Math.Abs(part.Y).Should().BeLessThanOrEqualTo(halfHeight + 25, part.Name);
                part.Width.Should().BeGreaterThan(0);
                part.Height.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public void wing_pods_are_placed_per_shape()
    {
        //Act
        var pods = Enumerable.Range(0, GameSetup.ShipShapeCount)
            .Select(shape => ShipLoadout.PartsFor(shape, [PowerUpKind.SpreadShot]).Single(part => part.Name == "spread pod right"))
            .Select(part => (part.X, part.Y))
            .ToArray();

        //Assert
        pods.Distinct().Should().HaveCount(3);
    }

    [Fact]
    public void speed_boost_flames_flicker_and_the_emitter_glow_pulses()
    {
        //Arrange
        var parts = ShipLoadout.AllParts(0);

        //Assert
        parts.Where(part => part.Effect == ShipPartEffect.Flicker).Should().OnlyContain(part =>
            part.PowerUp == PowerUpKind.SpeedBoost && part.Frames.Count > 1 && part.Layer == ShipPartLayer.UnderHull);
        parts.Where(part => part.Effect == ShipPartEffect.Pulse).Should().OnlyContain(part => part.PowerUp == PowerUpKind.PiercingLaser);
        parts.Where(part => part.Effect != ShipPartEffect.Flicker).Should().OnlyContain(part => part.Frames.Count == 1);
    }

    [Fact]
    public void HullScale_fits_each_hull_into_the_player_draw_box()
    {
        //Act
        var scales = Enumerable.Range(0, GameSetup.ShipShapeCount).Select(ShipLoadout.HullScale).ToArray();

        //Assert
        scales[0].Should().BeApproximately(PlayfieldPainter.PlayerDrawWidth / 99, 1e-9);
        scales[1].Should().BeApproximately(PlayfieldPainter.PlayerDrawWidth / 112, 1e-9);
        scales[2].Should().BeApproximately(PlayfieldPainter.PlayerDrawHeight / 75, 1e-9);
    }

    [Fact]
    public void HullArtWidth_rejects_an_unknown_shape() =>
        ((Action)(() => ShipLoadout.HullArtWidth(3))).Should().Throw<ArgumentOutOfRangeException>();

    [Fact]
    public void Describe_names_the_shape_and_every_part_with_its_frame()
    {
        //Arrange
        var parts = ShipLoadout.PartsFor(1, [PowerUpKind.RapidFire]);

        //Act
        var text = ShipLoadout.Describe(1, parts);

        //Assert
        text.Should().Be("ship shape 2 parts [nose cannon (gun08)]");
        ShipLoadout.Describe(0, []).Should().Be("ship shape 1 parts []");
    }

    [Fact]
    public void ActiveKinds_is_empty_for_a_new_game() =>
        ShipLoadout.ActiveKinds(new GameSimulation(new GameSetup(Difficulty.Pilot)).PowerUps).Should().BeEmpty();
}
