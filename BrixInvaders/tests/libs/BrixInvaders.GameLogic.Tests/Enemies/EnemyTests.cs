using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class EnemyTests
{
    [Fact]
    public void shielded_enemies_take_two_hits_and_show_the_shield_until_the_first()
    {
        //Arrange
        var enemy = new Enemy(1, EnemyRole.Shielded, EnemyColour.Blue, 0, 0);

        //Act
        var shieldedAtStart = enemy.HasShield;
        enemy.Health--;

        //Assert
        enemy.MaxHealth.Should().Be(2);
        shieldedAtStart.Should().BeTrue();
        enemy.HasShield.Should().BeFalse();
        enemy.IsAlive.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnemyRole.Grunt)]
    [InlineData(EnemyRole.Shooter)]
    [InlineData(EnemyRole.Diver)]
    [InlineData(EnemyRole.MissileCarrier)]
    public void other_roles_take_one_hit(EnemyRole role)
    {
        //Act
        var enemy = new Enemy(1, role, EnemyColour.Red, 0, 0);

        //Assert
        enemy.MaxHealth.Should().Be(1);
        enemy.HasShield.Should().BeFalse();
        enemy.State.Should().Be(EnemyState.InFormation);
    }

    [Fact]
    public void Box_is_centred_on_the_enemy()
    {
        //Arrange
        var enemy = new Enemy(1, EnemyRole.Grunt, EnemyColour.Red, 0, 0) { X = 100, Y = 200 };

        //Act
        var box = enemy.Box;

        //Assert
        box.Width.Should().Be(Enemy.Width);
        box.Height.Should().Be(Enemy.Height);
        box.CenterX.Should().Be(100);
        box.CenterY.Should().Be(200);
    }
}
