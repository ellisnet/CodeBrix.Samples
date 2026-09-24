using System;
using System.Collections.Generic;
using System.Globalization;

namespace BrixInvaders.GameLogic;

/// <summary>The difficulty table: one <see cref="DifficultySettings"/> per <see cref="Difficulty"/>.</summary>
public static class DifficultyTable
{
    private static readonly DifficultySettings Cadet = new DifficultySettings(
        Difficulty.Cadet, lives: 5, startingBombs: 2, columnFireInterval: 1.4, shooterInterval: 4.0, maxEnemyBolts: 4,
        enemyBoltSpeed: 260, dropHeight: 16, formationBaseInterval: 0.60, diveInterval: 3.5, maxDivers: 1,
        missileInterval: 5.0, missileCap: 1, bossHealthPercent: 75, bossFireScale: 1.25, powerUpDropPercent: 12,
        scoreMultiplierPercent: 100);

    private static readonly DifficultySettings Pilot = new DifficultySettings(
        Difficulty.Pilot, lives: 3, startingBombs: 1, columnFireInterval: 1.0, shooterInterval: 3.2, maxEnemyBolts: 6,
        enemyBoltSpeed: 300, dropHeight: 20, formationBaseInterval: 0.50, diveInterval: 3.0, maxDivers: 2,
        missileInterval: 4.2, missileCap: 2, bossHealthPercent: 100, bossFireScale: 1.0, powerUpDropPercent: 9,
        scoreMultiplierPercent: 150);

    private static readonly DifficultySettings Ace = new DifficultySettings(
        Difficulty.Ace, lives: 3, startingBombs: 1, columnFireInterval: 0.75, shooterInterval: 2.6, maxEnemyBolts: 8,
        enemyBoltSpeed: 340, dropHeight: 24, formationBaseInterval: 0.42, diveInterval: 2.5, maxDivers: 3,
        missileInterval: 3.5, missileCap: 3, bossHealthPercent: 130, bossFireScale: 0.85, powerUpDropPercent: 7,
        scoreMultiplierPercent: 200);

    private static readonly DifficultySettings Legend = new DifficultySettings(
        Difficulty.Legend, lives: 2, startingBombs: 0, columnFireInterval: 0.55, shooterInterval: 2.0, maxEnemyBolts: 10,
        enemyBoltSpeed: 380, dropHeight: 28, formationBaseInterval: 0.35, diveInterval: 2.0, maxDivers: 4,
        missileInterval: 2.8, missileCap: 4, bossHealthPercent: 170, bossFireScale: 0.7, powerUpDropPercent: 5,
        scoreMultiplierPercent: 300);

    /// <summary>All levels, easiest first.</summary>
    public static IReadOnlyList<Difficulty> Levels { get; } = new[] { Difficulty.Cadet, Difficulty.Pilot, Difficulty.Ace, Difficulty.Legend };

    /// <summary>Returns the settings for a level.</summary>
    /// <param name="level">The level.</param>
    /// <returns>Its settings.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the level is not defined.</exception>
    public static DifficultySettings For(Difficulty level) => level switch
    {
        Difficulty.Cadet => Cadet,
        Difficulty.Pilot => Pilot,
        Difficulty.Ace => Ace,
        Difficulty.Legend => Legend,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown difficulty."),
    };

    /// <summary>The display name of a level ("Cadet", "Pilot", "Ace", "Legend").</summary>
    /// <param name="level">The level.</param>
    /// <returns>The name.</returns>
    public static string NameOf(Difficulty level) => level.ToString();

    /// <summary>The HUD text of a level's score multiplier ("x1", "x1.5", "x2", "x3").</summary>
    /// <param name="level">The level.</param>
    /// <returns>The multiplier text.</returns>
    public static string MultiplierText(Difficulty level) => For(level).ScoreMultiplierPercent switch
    {
        100 => "x1",
        150 => "x1.5",
        200 => "x2",
        300 => "x3",
        var other => "x" + (other / 100.0).ToString(CultureInfo.InvariantCulture),
    };
}
