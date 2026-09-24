namespace BrixInvaders.Game.Credits;

/// <summary>One line of the credits screen.</summary>
/// <param name="Text">The text shown.</param>
/// <param name="Style">How it is drawn.</param>
/// <param name="Url">The address a <see cref="CreditsLineStyle.Link"/> line opens; null otherwise.</param>
public sealed record CreditsLine(string Text, CreditsLineStyle Style, string Url = null)
{
    /// <summary>A heading line.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The line.</returns>
    public static CreditsLine Heading(string text) => new CreditsLine(text, CreditsLineStyle.Heading);

    /// <summary>A body line.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The line.</returns>
    public static CreditsLine Body(string text) => new CreditsLine(text, CreditsLineStyle.Body);

    /// <summary>A link line.</summary>
    /// <param name="text">The text.</param>
    /// <param name="url">The address.</param>
    /// <returns>The line.</returns>
    public static CreditsLine Link(string text, string url) => new CreditsLine(text, CreditsLineStyle.Link, url);

    /// <summary>An empty line.</summary>
    public static CreditsLine Spacer { get; } = new CreditsLine(string.Empty, CreditsLineStyle.Spacer);
}
