using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// Score keeping: point values, the chain multiplier (consecutive hits without a miss) and the difficulty
/// multiplier. Awarded points = base points x chain multiplier x difficulty percent / 100 (integer arithmetic,
/// rounded down). Bonuses (sector clear, surplus power-ups) skip the chain multiplier.
/// </summary>
public sealed class Scoring
{
    /// <summary>Chain length per multiplier step: every 10 consecutive hits add x1.</summary>
    public const int ChainStep = 10;

    /// <summary>Highest chain multiplier.</summary>
    public const int MaxChainMultiplier = 5;

    /// <summary>Base points of a big meteor.</summary>
    public const int BigMeteorPoints = 20;

    /// <summary>Base points of a small meteor.</summary>
    public const int SmallMeteorPoints = 10;

    /// <summary>Base points of a shot-down homing missile.</summary>
    public const int MissilePoints = 25;

    /// <summary>Base points of a destroyed boss section (other than the core).</summary>
    public const int BossSectionPoints = 250;

    private static readonly int[] UfoValues = { 50, 100, 150, 300 };

    /// <summary>Creates a score keeper with a difficulty score multiplier.</summary>
    /// <param name="difficultyPercent">100, 150, 200 or 300.</param>
    /// <exception cref="ArgumentOutOfRangeException">When the percent is not positive.</exception>
    public Scoring(int difficultyPercent)
    {
        if (difficultyPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(difficultyPercent), difficultyPercent, "Must be positive.");
        }

        DifficultyPercent = difficultyPercent;
    }

    /// <summary>Current score.</summary>
    public long Score { get; private set; }

    /// <summary>Consecutive hits without a miss.</summary>
    public int ChainCount { get; private set; }

    /// <summary>Longest chain of the game.</summary>
    public int BestChain { get; private set; }

    /// <summary>The chain multiplier: 1 + chain / 10, capped at 5.</summary>
    public int ChainMultiplier => MultiplierFor(ChainCount);

    /// <summary>The difficulty score multiplier in percent.</summary>
    public int DifficultyPercent { get; }

    /// <summary>The chain multiplier for a chain length.</summary>
    /// <param name="chain">Chain length.</param>
    /// <returns>1..5.</returns>
    public static int MultiplierFor(int chain) => 1 + Math.Min(Math.Max(chain, 0) / ChainStep, MaxChainMultiplier - 1);

    /// <summary>Base points of an enemy colour in a sector: 10 x (tier + 1) + 5 x (sector - 1).</summary>
    /// <param name="colour">Colour (tier).</param>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The colour points.</returns>
    public static int ColourPoints(EnemyColour colour, int sector) => (10 * ((int)colour + 1)) + (5 * (Math.Max(sector, 1) - 1));

    /// <summary>Extra base points by role: grunt 0, shooter 10, shielded 20, diver 30, missile carrier 40.</summary>
    /// <param name="role">Role.</param>
    /// <returns>The role bonus.</returns>
    public static int RoleBonus(EnemyRole role) => role switch
    {
        EnemyRole.Grunt => 0,
        EnemyRole.Shooter => 10,
        EnemyRole.Shielded => 20,
        EnemyRole.Diver => 30,
        EnemyRole.MissileCarrier => 40,
        _ => 0,
    };

    /// <summary>Base points of an enemy: colour points + role bonus, doubled when shot out of formation (diving).</summary>
    /// <param name="role">Role.</param>
    /// <param name="colour">Colour.</param>
    /// <param name="sector">Sector.</param>
    /// <param name="outOfFormation">True when it was diving or returning.</param>
    /// <returns>The base points.</returns>
    public static int EnemyBasePoints(EnemyRole role, EnemyColour colour, int sector, bool outOfFormation)
    {
        var points = ColourPoints(colour, sector) + RoleBonus(role);
        return outOfFormation ? points * 2 : points;
    }

    /// <summary>The four possible UFO values before the loop factor: 50, 100, 150, 300.</summary>
    /// <param name="index">0..3.</param>
    /// <param name="sector">Sector (the value is multiplied by the loop number + 1).</param>
    /// <returns>The UFO base points.</returns>
    public static int UfoPoints(int index, int sector) => UfoValues[Math.Clamp(index, 0, UfoValues.Length - 1)] * (SectorRules.LoopOf(sector) + 1);

    /// <summary>Number of distinct UFO values.</summary>
    public static int UfoValueCount => UfoValues.Length;

    /// <summary>Base points for defeating a sector's boss: 2500 + 500 x (sector - 1).</summary>
    /// <param name="sector">Sector.</param>
    /// <returns>The base points.</returns>
    public static int BossDefeatPoints(int sector) => 2500 + (500 * (sector - 1));

    /// <summary>Sector-clear bonus before the difficulty multiplier: 1000 x sector.</summary>
    /// <param name="sector">Sector.</param>
    /// <returns>The bonus.</returns>
    public static int SectorClearBonus(int sector) => 1000 * sector;

    /// <summary>Counts a hit: the chain grows by one.</summary>
    /// <param name="events">Receives ChainMultiplierChanged when the multiplier steps up.</param>
    internal void RegisterHit(GameEvents events)
    {
        var before = ChainMultiplier;
        ChainCount++;
        BestChain = Math.Max(BestChain, ChainCount);
        if (ChainMultiplier != before)
        {
            events.Add(new GameEvent(GameEventKind.ChainMultiplierChanged, value: ChainMultiplier));
        }
    }

    /// <summary>Counts a miss: the chain resets to zero.</summary>
    /// <param name="events">Receives ChainBroken (and ChainMultiplierChanged when the multiplier drops).</param>
    internal void RegisterMiss(GameEvents events)
    {
        if (ChainCount == 0)
        {
            return;
        }

        var before = ChainMultiplier;
        events.Add(new GameEvent(GameEventKind.ChainBroken, value: ChainCount));
        ChainCount = 0;
        if (before != ChainMultiplier)
        {
            events.Add(new GameEvent(GameEventKind.ChainMultiplierChanged, value: ChainMultiplier));
        }
    }

    /// <summary>Awards base points with the chain and difficulty multipliers.</summary>
    /// <param name="basePoints">Base points.</param>
    /// <returns>The points actually added.</returns>
    internal long Award(int basePoints)
    {
        var points = (long)basePoints * ChainMultiplier * DifficultyPercent / 100;
        Score += points;
        return points;
    }

    /// <summary>Awards a bonus with the difficulty multiplier only.</summary>
    /// <param name="basePoints">Base points.</param>
    /// <returns>The points actually added.</returns>
    internal long AwardBonus(int basePoints)
    {
        var points = (long)basePoints * DifficultyPercent / 100;
        Score += points;
        return points;
    }
}
