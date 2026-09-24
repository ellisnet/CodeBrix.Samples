namespace BrixInvaders.Game.Session;

/// <summary>The musical moment the game is in; the music director hears only changes between these.</summary>
public enum MusicMoment
{
    /// <summary>The title and its menus.</summary>
    Title = 0,

    /// <summary>A sector's briefing and waves.</summary>
    Sector = 1,

    /// <summary>A boss fight.</summary>
    Boss = 2,

    /// <summary>Game over, until the title comes back.</summary>
    GameOver = 3,
}
