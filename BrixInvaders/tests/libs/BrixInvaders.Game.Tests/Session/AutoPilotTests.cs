using BrixInvaders.Game.Session;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Session;

public class AutoPilotTests
{
    private static void Drive(SessionDriver driver, AutoPilot pilot, double seconds)
    {
        var steps = (int)(seconds / SessionDriver.Step);
        for (var i = 0; i < steps; i++)
        {
            driver.Session.Update(SessionDriver.Step, pilot.Menu(driver.Session), pilot.Play(driver.Session));
        }
    }

    [Fact]
    public void Menu_waits_on_the_title_before_choosing_play()
    {
        //Arrange
        var driver = new SessionDriver();
        var pilot = new AutoPilot();
        driver.ToTitle();

        //Act
        var early = pilot.Menu(driver.Session);
        driver.Wait(AutoPilot.MenuDelaySeconds);
        var late = pilot.Menu(driver.Session);

        //Assert
        early.HasAnyInput.Should().BeFalse();
        late.Confirm.Should().BeTrue();
    }

    [Fact]
    public void Play_is_quiet_away_from_a_game() => new AutoPilot().Play(new SessionDriver().Session).HasAnyInput.Should().BeFalse();

    [Fact]
    public void the_autopilot_reaches_play_and_plays()
    {
        //Arrange
        var driver = new SessionDriver();
        var pilot = new AutoPilot();
        driver.ToTitle();

        //Act
        Drive(driver, pilot, 12);

        //Assert
        driver.Session.CurrentScreen.Should().Be(GameScreen.Playing);
        driver.Session.Game.StepCount.Should().BeGreaterThan(200);
        driver.Session.Events.Should().Contain(gameEvent => gameEvent.Kind == GameEventKind.PlayerFired);
    }

    [Fact]
    public void IsRequested_is_off_by_default() => AutoPilot.IsRequested().Should().BeFalse();
}
