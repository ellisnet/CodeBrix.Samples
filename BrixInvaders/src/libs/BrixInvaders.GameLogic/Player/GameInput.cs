using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The in-game input snapshot for one simulation step. The Game library merges keyboard and gamepad into it
/// (dead zone already applied to the stick). Fire is "held" (auto-fire at the fire rate); Bomb is edge-detected
/// by the simulation (holding it uses one bomb).
/// </summary>
public readonly struct GameInput
{
    /// <summary>Creates an input snapshot.</summary>
    /// <param name="moveAxis">Horizontal move, -1 (left) .. 1 (right); clamped.</param>
    /// <param name="fire">Fire held.</param>
    /// <param name="bomb">Bomb held.</param>
    public GameInput(double moveAxis, bool fire, bool bomb)
    {
        MoveAxis = double.IsNaN(moveAxis) ? 0 : Math.Clamp(moveAxis, -1.0, 1.0);
        Fire = fire;
        Bomb = bomb;
    }

    /// <summary>No input.</summary>
    public static GameInput None => new GameInput(0, false, false);

    /// <summary>Horizontal move, -1 .. 1.</summary>
    public double MoveAxis { get; }

    /// <summary>Fire held.</summary>
    public bool Fire { get; }

    /// <summary>Bomb held.</summary>
    public bool Bomb { get; }

    /// <summary>True when any control is active (used by attract mode to return to the title).</summary>
    public bool HasAnyInput => MoveAxis != 0 || Fire || Bomb;
}
