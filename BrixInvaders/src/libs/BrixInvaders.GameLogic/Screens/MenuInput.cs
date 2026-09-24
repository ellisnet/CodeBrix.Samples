namespace BrixInvaders.GameLogic;

/// <summary>
/// One frame of menu input, edge-triggered (true only in the frame the key/button went down). The Game library maps
/// keyboard and gamepad onto the same actions: arrows / D-pad / stick = Up/Down/Left/Right, Enter / A = Confirm,
/// Escape / B = Back, Escape = Pause, gamepad Start = Start, K / Y = KenneyLink.
/// </summary>
public readonly struct MenuInput
{
    /// <summary>Creates a menu input snapshot.</summary>
    /// <param name="up">Up pressed.</param>
    /// <param name="down">Down pressed.</param>
    /// <param name="left">Left pressed.</param>
    /// <param name="right">Right pressed.</param>
    /// <param name="confirm">Confirm pressed.</param>
    /// <param name="back">Back pressed.</param>
    /// <param name="pause">Pause pressed.</param>
    /// <param name="start">Start pressed.</param>
    /// <param name="kenneyLink">Kenney link pressed.</param>
    /// <param name="other">Any other key/button/stick activity (counts as input for idle/attract detection).</param>
    public MenuInput(bool up = false, bool down = false, bool left = false, bool right = false, bool confirm = false,
        bool back = false, bool pause = false, bool start = false, bool kenneyLink = false, bool other = false)
    {
        Up = up;
        Down = down;
        Left = left;
        Right = right;
        Confirm = confirm;
        Back = back;
        Pause = pause;
        Start = start;
        KenneyLink = kenneyLink;
        Other = other;
    }

    /// <summary>No input.</summary>
    public static MenuInput None => default;

    /// <summary>Up pressed.</summary>
    public bool Up { get; }

    /// <summary>Down pressed.</summary>
    public bool Down { get; }

    /// <summary>Left pressed.</summary>
    public bool Left { get; }

    /// <summary>Right pressed.</summary>
    public bool Right { get; }

    /// <summary>Confirm pressed.</summary>
    public bool Confirm { get; }

    /// <summary>Back pressed.</summary>
    public bool Back { get; }

    /// <summary>Pause pressed.</summary>
    public bool Pause { get; }

    /// <summary>Start pressed.</summary>
    public bool Start { get; }

    /// <summary>Kenney link (K / Y) pressed.</summary>
    public bool KenneyLink { get; }

    /// <summary>Any other activity.</summary>
    public bool Other { get; }

    /// <summary>True when anything at all was pressed.</summary>
    public bool HasAnyInput => Up || Down || Left || Right || Confirm || Back || Pause || Start || KenneyLink || Other;
}
