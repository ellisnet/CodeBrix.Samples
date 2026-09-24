namespace BrixInvaders.GameLogic;

/// <summary>A request from the screen state machine to the Game library.</summary>
public readonly struct ScreenCommand
{
    /// <summary>Creates a command.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="value">Kind-specific integer.</param>
    /// <param name="delta">Kind-specific delta (-1/+1 for AdjustSetting).</param>
    /// <param name="text">Kind-specific text (the name for SubmitHighScore).</param>
    public ScreenCommand(ScreenCommandKind kind, int value = 0, int delta = 0, string text = "")
    {
        Kind = kind;
        Value = value;
        Delta = delta;
        Text = text ?? string.Empty;
    }

    /// <summary>The kind.</summary>
    public ScreenCommandKind Kind { get; }

    /// <summary>Kind-specific integer.</summary>
    public int Value { get; }

    /// <summary>Kind-specific delta.</summary>
    public int Delta { get; }

    /// <summary>Kind-specific text.</summary>
    public string Text { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}(value={Value}, delta={Delta}, text='{Text}')";
}
