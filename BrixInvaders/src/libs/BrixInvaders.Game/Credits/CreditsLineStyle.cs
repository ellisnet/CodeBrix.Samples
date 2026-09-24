namespace BrixInvaders.Game.Credits;

/// <summary>How a credits line is drawn.</summary>
public enum CreditsLineStyle
{
    /// <summary>A section heading.</summary>
    Heading = 0,

    /// <summary>Ordinary text.</summary>
    Body = 1,

    /// <summary>A link the player can open (click it, or it is listed with its address).</summary>
    Link = 2,

    /// <summary>An empty line.</summary>
    Spacer = 3,
}
