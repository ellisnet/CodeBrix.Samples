using System;

namespace BrixInvaders.Game.Input;

/// <summary>The gamepad buttons the game reads (standard Xbox-style layout), as flags.</summary>
[Flags]
public enum PadButtons
{
    /// <summary>No button.</summary>
    None = 0,

    /// <summary>The bottom face button.</summary>
    A = 1 << 0,

    /// <summary>The right face button.</summary>
    B = 1 << 1,

    /// <summary>The left face button.</summary>
    X = 1 << 2,

    /// <summary>The top face button.</summary>
    Y = 1 << 3,

    /// <summary>Start / Menu.</summary>
    Start = 1 << 4,

    /// <summary>Back / View / Select.</summary>
    Back = 1 << 5,

    /// <summary>The left shoulder button.</summary>
    LeftShoulder = 1 << 6,

    /// <summary>The right shoulder button.</summary>
    RightShoulder = 1 << 7,

    /// <summary>D-pad up.</summary>
    DPadUp = 1 << 8,

    /// <summary>D-pad down.</summary>
    DPadDown = 1 << 9,

    /// <summary>D-pad left.</summary>
    DPadLeft = 1 << 10,

    /// <summary>D-pad right.</summary>
    DPadRight = 1 << 11,
}
