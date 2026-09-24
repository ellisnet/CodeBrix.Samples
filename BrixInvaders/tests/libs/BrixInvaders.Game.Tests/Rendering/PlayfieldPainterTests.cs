using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Rendering;

public class PlayfieldPainterTests
{
    private const int MaxSteps = 60 * 60 * 10;

    private static RenderFrame Paint(PlayfieldPainter painter, GameSimulation game)
    {
        var frame = new FrameBuilder();
        painter.PaintGame(frame, game, game.Time);
        return frame.Build();
    }

    private static bool AnyVisibleActive(GameSimulation game) => ShipLoadout.ActiveKinds(game.PowerUps).Count > 0;

    private static bool StepUntil(GameSimulation game, AttractPilot pilot, System.Func<GameSimulation, bool> condition)
    {
        for (var step = 0; step < MaxSteps && !game.IsGameOver; step++)
        {
            if (condition(game) && game.Player.IsPresent)
            {
                return true;
            }

            game.Step(Playfield.FixedStep, pilot.Decide(game));
        }

        return false;
    }

    [Fact]
    public void PaintGame_logs_the_bare_loadout_once_for_a_new_game()
    {
        //Arrange
        var painter = new PlayfieldPainter();
        var game = new GameSimulation(new GameSetup(Difficulty.Pilot, shipShape: 2));
        var lines = new List<string>();
        GameLog.Sink = lines.Add;
        RenderFrame frame;
        try
        {
            //Act
            frame = Paint(painter, game);
            Paint(painter, game);
        }
        finally
        {
            GameLog.Sink = null;
        }

        //Assert
        lines.Where(line => line.Contains("loadout:")).Should().Equal("[BrixInvaders] loadout: ship shape 3 parts []");
        painter.Loadout.Should().Be("ship shape 3 parts []");
        var partFrames = ShipLoadout.AllParts(2).SelectMany(part => part.Frames).ToHashSet();
        frame.World.Should().NotContain(command => command.Image != null && partFrames.Contains(command.Image));
    }

    [Fact]
    public void PaintGame_draws_the_parts_of_an_active_power_up_and_drops_them_when_it_runs_out()
    {
        //Arrange
        var painter = new PlayfieldPainter();
        var game = new GameSimulation(new GameSetup(Difficulty.Pilot, seed: 7, shipShape: 1));
        var pilot = new AttractPilot();
        var lines = new List<string>();
        Paint(painter, game);
        GameLog.Sink = lines.Add;
        RenderFrame upgraded;
        RenderFrame bare;
        IReadOnlyList<ShipPart> parts;
        try
        {
            //Act
            StepUntil(game, pilot, AnyVisibleActive).Should().BeTrue("the pilot collects a capability power-up eventually");
            parts = ShipLoadout.PartsFor(1, ShipLoadout.ActiveKinds(game.PowerUps));
            upgraded = Paint(painter, game);
            StepUntil(game, pilot, candidate => !AnyVisibleActive(candidate)).Should().BeTrue("timed power-ups run out");
            bare = Paint(painter, game);
        }
        finally
        {
            GameLog.Sink = null;
        }

        //Assert
        var drawn = upgraded.World.Select(command => command.Image).ToHashSet();
        parts.Should().NotBeEmpty();
        parts.Should().OnlyContain(part => part.Frames.Any(drawn.Contains));
        var loadoutLines = lines.Where(line => line.Contains("loadout:")).ToList();
        loadoutLines.First().Should().Be("[BrixInvaders] loadout: " + ShipLoadout.Describe(1, parts));
        loadoutLines.Last().Should().Be("[BrixInvaders] loadout: ship shape 2 parts []");
        var allParts = ShipLoadout.AllParts(1).SelectMany(part => part.Frames).ToHashSet();
        bare.World.Should().NotContain(command => command.Image != null && allParts.Contains(command.Image));
    }
}
