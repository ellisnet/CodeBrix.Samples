namespace BrixInvaders.GameLogic;

/// <summary>Where the simulation is within a sector.</summary>
public enum StagePhase
{
    /// <summary>A wave's formation is on screen but not yet stepping or firing ("WAVE n" banner).</summary>
    WaveIntro = 0,

    /// <summary>The wave is being played.</summary>
    WaveActive = 1,

    /// <summary>Between waves (a meteor shower in sector designs 3..5).</summary>
    WaveIntermission = 2,

    /// <summary>The boss warning before the boss enters.</summary>
    BossWarning = 3,

    /// <summary>The boss fight.</summary>
    BossFight = 4,

    /// <summary>The sector is clear; the simulation idles until <see cref="GameSimulation.BeginNextSector"/>.</summary>
    SectorComplete = 5,

    /// <summary>No lives left; the simulation is frozen.</summary>
    GameOver = 6,
}
