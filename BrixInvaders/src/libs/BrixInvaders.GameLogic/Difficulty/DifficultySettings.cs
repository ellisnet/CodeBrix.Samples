namespace BrixInvaders.GameLogic;

/// <summary>
/// The tuning numbers for one difficulty level (see <see cref="DifficultyTable"/>). Intervals are in seconds, speeds in
/// world units per second. The values are the sector-1 values; <see cref="SectorRules"/> tightens them on every loop.
/// </summary>
public sealed class DifficultySettings
{
    internal DifficultySettings(
        Difficulty level,
        int lives,
        int startingBombs,
        double columnFireInterval,
        double shooterInterval,
        int maxEnemyBolts,
        double enemyBoltSpeed,
        int dropHeight,
        double formationBaseInterval,
        double diveInterval,
        int maxDivers,
        double missileInterval,
        int missileCap,
        int bossHealthPercent,
        double bossFireScale,
        double powerUpDropPercent,
        int scoreMultiplierPercent)
    {
        Level = level;
        Lives = lives;
        StartingBombs = startingBombs;
        ColumnFireInterval = columnFireInterval;
        ShooterInterval = shooterInterval;
        MaxEnemyBolts = maxEnemyBolts;
        EnemyBoltSpeed = enemyBoltSpeed;
        DropHeight = dropHeight;
        FormationBaseInterval = formationBaseInterval;
        DiveInterval = diveInterval;
        MaxDivers = maxDivers;
        MissileInterval = missileInterval;
        MissileCap = missileCap;
        BossHealthPercent = bossHealthPercent;
        BossFireScale = bossFireScale;
        PowerUpDropPercent = powerUpDropPercent;
        ScoreMultiplierPercent = scoreMultiplierPercent;
    }

    /// <summary>The level these settings belong to.</summary>
    public Difficulty Level { get; }

    /// <summary>Lives at the start of a game.</summary>
    public int Lives { get; }

    /// <summary>Bombs in stock at the start of a game.</summary>
    public int StartingBombs { get; }

    /// <summary>Seconds between "lowest enemy in a random column fires" shots.</summary>
    public double ColumnFireInterval { get; }

    /// <summary>Seconds between aimed shots of each shooter enemy.</summary>
    public double ShooterInterval { get; }

    /// <summary>Maximum enemy bolts on screen (missiles are capped separately).</summary>
    public int MaxEnemyBolts { get; }

    /// <summary>Enemy bolt speed.</summary>
    public double EnemyBoltSpeed { get; }

    /// <summary>World units a formation drops at each edge (the "descent speed").</summary>
    public int DropHeight { get; }

    /// <summary>Seconds between formation steps when the formation is full.</summary>
    public double FormationBaseInterval { get; }

    /// <summary>Seconds between dive launches.</summary>
    public double DiveInterval { get; }

    /// <summary>Maximum divers out of formation at once.</summary>
    public int MaxDivers { get; }

    /// <summary>Seconds between missile launches of each missile carrier.</summary>
    public double MissileInterval { get; }

    /// <summary>Maximum homing missiles on screen.</summary>
    public int MissileCap { get; }

    /// <summary>Boss health as a percentage of the base values.</summary>
    public int BossHealthPercent { get; }

    /// <summary>Multiplier applied to boss attack intervals (smaller fires faster).</summary>
    public double BossFireScale { get; }

    /// <summary>Chance, in percent, that a destroyed formation enemy drops a power-up.</summary>
    public double PowerUpDropPercent { get; }

    /// <summary>Score multiplier in percent: 100, 150, 200 or 300.</summary>
    public int ScoreMultiplierPercent { get; }
}
