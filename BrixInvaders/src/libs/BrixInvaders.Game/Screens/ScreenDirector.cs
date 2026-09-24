using System;
using System.Collections.Generic;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Keeps the one <see cref="ScreenPainter"/> of the screen on show open, following the screen state machine.</summary>
public sealed class ScreenDirector
{
    private readonly Dictionary<GameScreen, ScreenPainter> _screens;
    private ScreenPainter _open;

    /// <summary>Creates the director with a painter for every screen.</summary>
    public ScreenDirector()
    {
        Settings = new SettingsScreen();
        _screens = new Dictionary<GameScreen, ScreenPainter>
        {
            [GameScreen.Splash] = new SplashScreen(),
            [GameScreen.Title] = new TitleScreen(),
            [GameScreen.Attract] = new AttractScreen(),
            [GameScreen.ShipSelect] = new ShipSelectScreen(),
            [GameScreen.DifficultySelect] = new DifficultySelectScreen(),
            [GameScreen.SectorBriefing] = new SectorBriefingScreen(),
            [GameScreen.Playing] = new PlayingScreen(),
            [GameScreen.Paused] = new PausedScreen(),
            [GameScreen.SectorClear] = new SectorClearScreen(),
            [GameScreen.GameOver] = new GameOverScreen(),
            [GameScreen.HighScoreEntry] = new HighScoreEntryScreen(),
            [GameScreen.HighScores] = new HighScoresScreen(),
            [GameScreen.Settings] = Settings,
            [GameScreen.Credits] = new CreditsScreen(),
        };
    }

    /// <summary>The screen whose painter is open.</summary>
    public GameScreen Current { get; private set; } = GameScreen.Splash;

    /// <summary>The settings screen (the host feeds it the gamepad status line).</summary>
    public SettingsScreen Settings { get; }

    /// <summary>Whether every <see cref="GameScreen"/> has a painter.</summary>
    /// <returns>True when none is missing.</returns>
    public bool CoversEveryScreen()
    {
        foreach (GameScreen screen in Enum.GetValues<GameScreen>())
        {
            if (!_screens.ContainsKey(screen))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Opens the painter of <paramref name="screen"/> (closing the previous one) when it is not already open.</summary>
    /// <param name="screen">The screen on show.</param>
    /// <param name="context">The context.</param>
    public void Follow(GameScreen screen, PaintContext context)
    {
        if (_open != null && screen == Current)
        {
            return;
        }

        _open?.Close();
        Current = screen;
        _open = _screens[screen];
        _open.Open(context);
    }

    /// <summary>Advances the open painter.</summary>
    /// <param name="dt">Seconds.</param>
    public void Update(double dt) => _open?.Update(dt);

    /// <summary>Draws the open painter.</summary>
    /// <param name="context">The context.</param>
    public void Paint(PaintContext context) => _open?.Paint(context);
}
