using System;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Session;

/// <summary>
/// A verification aid (off unless <c>BRIXINVADERS_AUTOPILOT=1</c>): it walks the menus and plays real games with the
/// attract-mode pilot, so a run with no hands on the keyboard still reaches play, sector clears, bosses and game
/// over - and their log lines. Keyboard and gamepad input still work alongside it.
/// </summary>
public sealed class AutoPilot
{
    /// <summary>The environment variable that turns the autopilot on when set to 1.</summary>
    public const string Variable = "BRIXINVADERS_AUTOPILOT";

    /// <summary>How long it waits on a menu screen before confirming.</summary>
    public const double MenuDelaySeconds = 1.2;

    private readonly AttractPilot _pilot = new AttractPilot();

    /// <summary>Whether the environment asks for the autopilot.</summary>
    /// <returns>True when <c>BRIXINVADERS_AUTOPILOT=1</c>.</returns>
    public static bool IsRequested() => Environment.GetEnvironmentVariable(Variable) == "1";

    /// <summary>The menu press the autopilot makes this step (none while playing or before the delay).</summary>
    /// <param name="session">The session.</param>
    /// <returns>The press.</returns>
    public MenuInput Menu(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var screens = session.Screens;
        var waited = screens.ScreenTime >= MenuDelaySeconds;
        return screens.CurrentScreen switch
        {
            GameScreen.Title when waited && screens.TitleCursor == TitleMenuItem.Play => new MenuInput(confirm: true),
            GameScreen.Title when waited => new MenuInput(up: true),
            GameScreen.ShipSelect or GameScreen.DifficultySelect or GameScreen.SectorBriefing or GameScreen.SectorClear
                or GameScreen.HighScoreEntry or GameScreen.HighScores when waited => new MenuInput(confirm: true),
            GameScreen.GameOver when screens.ScreenTime >= ScreenStateMachine.GameOverMinSeconds + 1 => new MenuInput(confirm: true),
            _ => MenuInput.None,
        };
    }

    /// <summary>The play input the autopilot uses this step.</summary>
    /// <param name="session">The session.</param>
    /// <returns>The input (none when no game is being played).</returns>
    public GameInput Play(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.CurrentScreen == GameScreen.Playing && session.Game != null ? _pilot.Decide(session.Game) : GameInput.None;
    }
}
