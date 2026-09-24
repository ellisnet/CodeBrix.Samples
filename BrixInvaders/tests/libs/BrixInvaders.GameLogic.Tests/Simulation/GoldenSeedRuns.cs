using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

/// <summary>
/// Golden-seed determinism fences. Each run plays 10 000 fixed steps (about 2 minutes 47 seconds of game time) from a
/// fixed seed with a scripted input (the attract pilot plus a bomb every 20 seconds, and the next sector started as
/// soon as one is complete), then compares the score and the state hash with pinned values.
/// A changed number means the RULES changed (a tuning value, an event order, the RNG draw order ...). If that
/// change was deliberate, update the pinned values in the same change and say so; if it was not, it is a bug.
/// </summary>
public class GoldenSeedRuns
{
    private const int Steps = 10000;

    private static (long Score, ulong Hash, int Sector, int Wave, StagePhase Phase) Play(Difficulty difficulty, int startSector, int seed)
    {
        var simulation = new GameSimulation(new GameSetup(difficulty, startSector, seed));
        var pilot = new AttractPilot();
        for (var i = 0; i < Steps; i++)
        {
            var decided = pilot.Decide(simulation);
            var input = new GameInput(decided.MoveAxis, decided.Fire, i % 1200 == 600);
            simulation.Step(Playfield.FixedStep, input);
            if (simulation.Phase == StagePhase.SectorComplete)
            {
                simulation.BeginNextSector();
            }
        }

        return (simulation.Score, simulation.ComputeStateHash(), simulation.Sector, simulation.Wave, simulation.Phase);
    }

    [Fact]
    public void golden_seed_pilot_run()
    {
        //Act
        var result = Play(Difficulty.Pilot, 1, 20260923);

        //Assert
        result.Should().Be((7740L, 17892244461283368371UL, 1, 4, StagePhase.WaveActive));
    }

    [Fact]
    public void golden_seed_cadet_run()
    {
        //Act
        var result = Play(Difficulty.Cadet, 1, 1977);

        //Assert
        result.Should().Be((6420L, 15005897562857968365UL, 1, 5, StagePhase.WaveActive));
    }

    [Fact]
    public void golden_seed_sector_five_cadet_run()
    {
        //Act
        var result = Play(Difficulty.Cadet, 5, 5555);

        //Assert
        result.Should().Be((13795L, 463141285040697274UL, 5, 5, StagePhase.WaveActive));
    }

    [Fact]
    public void a_golden_run_repeats_exactly()
    {
        //Act
        var first = Play(Difficulty.Legend, 1, 42);
        var second = Play(Difficulty.Legend, 1, 42);

        //Assert
        second.Should().Be(first);
    }
}
