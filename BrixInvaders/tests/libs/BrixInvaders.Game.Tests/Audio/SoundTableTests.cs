using System;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Audio;

public class SoundTableTests
{
    //A representative event of each kind, with the value that makes a conditional kind sound
    private static GameEvent Sounding(GameEventKind kind) => kind switch
    {
        GameEventKind.ChainBroken => new GameEvent(kind, value: Scoring.ChainStep),
        GameEventKind.ChainMultiplierChanged => new GameEvent(kind, value: 2),
        _ => new GameEvent(kind),
    };

    [Fact]
    public void every_event_kind_is_either_mapped_or_deliberately_silent()
    {
        foreach (var kind in Enum.GetValues<GameEventKind>())
        {
            //Act
            var sounds = SoundTable.TryGetCue(Sounding(kind), out _);

            //Assert
            (sounds || SoundTable.SilentKinds.Contains(kind)).Should().BeTrue($"{kind} needs a sound or a place in SilentKinds");
            (sounds && SoundTable.SilentKinds.Contains(kind)).Should().BeFalse($"{kind} is both silent and mapped");
        }
    }

    [Fact]
    public void every_cue_key_is_an_asset_key_the_assets_library_pins()
    {
        //Arrange
        var sfxKeys = typeof(AssetKeys.Sfx).GetFields().Select(field => (string)field.GetRawConstantValue()).ToHashSet();

        foreach (var kind in Enum.GetValues<GameEventKind>())
        {
            //Act
            if (!SoundTable.TryGetCue(Sounding(kind), out var cue))
            {
                continue;
            }

            //Assert
            sfxKeys.Should().Contain(cue.Key);
            cue.Volume.Should().BeInRange(0f, 1f);
        }
    }

    [Fact]
    public void the_menu_cues_are_pinned_keys() =>
        new[] { SoundTable.MenuMove, SoundTable.MenuConfirm, SoundTable.MenuBack, SoundTable.HighScore }
            .Select(cue => cue.Key).Should().OnlyContain(key => key.StartsWith("kenney:", StringComparison.Ordinal));

    [Theory]
    [InlineData(0, SoundEffect.March0)]
    [InlineData(1, SoundEffect.March1)]
    [InlineData(2, SoundEffect.March2)]
    [InlineData(3, SoundEffect.March3)]
    [InlineData(4, SoundEffect.March0)]
    [InlineData(-1, SoundEffect.March3)]
    public void MarchNote_plays_the_four_note_march(int note, SoundEffect expected) => SoundTable.MarchNote(note).Should().Be(expected);

    [Fact]
    public void TryGetCue_marches_with_the_formation()
    {
        //Act
        SoundTable.TryGetCue(new GameEvent(GameEventKind.FormationStepped, value: 2), out var cue);

        //Assert
        cue.Effect.Should().Be(SoundEffect.March2);
    }

    [Fact]
    public void TryGetCue_gives_a_boss_fan_its_own_laser()
    {
        //Act
        SoundTable.TryGetCue(new GameEvent(GameEventKind.EnemyFired, value: 5), out var fan);
        SoundTable.TryGetCue(new GameEvent(GameEventKind.EnemyFired), out var single);

        //Assert
        fan.Effect.Should().Be(SoundEffect.BossLaser);
        single.Effect.Should().Be(SoundEffect.EnemyLaser);
    }

    [Fact]
    public void TryGetCue_tells_armour_from_damage_on_a_boss()
    {
        //Act
        SoundTable.TryGetCue(new GameEvent(GameEventKind.BossHit, value: 1), out var armour);
        SoundTable.TryGetCue(new GameEvent(GameEventKind.BossHit, value: 0), out var damage);

        //Assert
        armour.Effect.Should().Be(SoundEffect.ArmourHit);
        damage.Effect.Should().Be(SoundEffect.BossHit);
    }

    [Theory]
    [InlineData(PowerUpKind.ShieldBubble, SoundEffect.ShieldUp)]
    [InlineData(PowerUpKind.ExtraLife, SoundEffect.ExtraLife)]
    [InlineData(PowerUpKind.RapidFire, SoundEffect.PowerUpPickup)]
    public void TryGetCue_gives_power_ups_their_own_pickups(PowerUpKind kind, SoundEffect expected)
    {
        //Act
        SoundTable.TryGetCue(new GameEvent(GameEventKind.PowerUpCollected, powerUp: kind), out var cue);

        //Assert
        cue.Effect.Should().Be(expected);
    }

    [Fact]
    public void TryGetCue_stays_quiet_for_a_chain_that_only_drops_back_to_x1()
    {
        //Act
        var multiplierDown = SoundTable.TryGetCue(new GameEvent(GameEventKind.ChainMultiplierChanged, value: 1), out _);
        var shortChainBroken = SoundTable.TryGetCue(new GameEvent(GameEventKind.ChainBroken, value: 3), out _);

        //Assert
        multiplierDown.Should().BeFalse();
        shortChainBroken.Should().BeFalse();
    }

    [Fact]
    public void the_boss_warning_and_game_over_outrank_everything()
    {
        //Act
        SoundTable.TryGetCue(new GameEvent(GameEventKind.BossIncoming), out var warning);
        SoundTable.TryGetCue(new GameEvent(GameEventKind.PlayerFired), out var laser);

        //Assert
        warning.Priority.Should().BeGreaterThan(laser.Priority);
    }
}
