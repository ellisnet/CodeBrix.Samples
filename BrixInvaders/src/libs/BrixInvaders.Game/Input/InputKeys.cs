using System;

namespace BrixInvaders.Game.Input;

/// <summary>The keyboard keys the game reads, as flags so one sample of the keyboard is a single value.</summary>
[Flags]
public enum InputKeys
{
    /// <summary>No key.</summary>
    None = 0,

    /// <summary>The Left arrow.</summary>
    Left = 1 << 0,

    /// <summary>The Right arrow.</summary>
    Right = 1 << 1,

    /// <summary>The Up arrow.</summary>
    Up = 1 << 2,

    /// <summary>The Down arrow.</summary>
    Down = 1 << 3,

    /// <summary>The A key (move left in play).</summary>
    A = 1 << 4,

    /// <summary>The D key (move right in play).</summary>
    D = 1 << 5,

    /// <summary>The space bar (fire).</summary>
    Space = 1 << 6,

    /// <summary>Enter (menu confirm).</summary>
    Enter = 1 << 7,

    /// <summary>Escape (menu back and pause).</summary>
    Escape = 1 << 8,

    /// <summary>Left Shift (bomb). A plain Shift report counts as Left Shift.</summary>
    LeftShift = 1 << 9,

    /// <summary>K (open the Kenney link on the cards).</summary>
    K = 1 << 10,

    /// <summary>Any other key the game watches; it counts as input (it wakes the title from attract mode).</summary>
    Other = 1 << 11,
}
