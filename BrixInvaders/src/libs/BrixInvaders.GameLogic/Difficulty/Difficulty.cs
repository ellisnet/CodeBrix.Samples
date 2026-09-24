namespace BrixInvaders.GameLogic;

/// <summary>The four difficulty levels, easiest first.</summary>
public enum Difficulty
{
    /// <summary>Easiest level: more lives, slower fire, x1 score.</summary>
    Cadet = 0,

    /// <summary>The default level: x1.5 score.</summary>
    Pilot = 1,

    /// <summary>Hard: x2 score.</summary>
    Ace = 2,

    /// <summary>Hardest: x3 score.</summary>
    Legend = 3,
}
