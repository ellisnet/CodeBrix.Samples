using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The screen flow: Splash -> Title (attract after 15 s idle) -> ShipSelect -> DifficultySelect -> SectorBriefing
/// -> Playing (Paused overlay) -> SectorClear -> SectorBriefing ... -> GameOver -> HighScoreEntry -> HighScores ->
/// Title; Settings, HighScores and Credits are reachable from the Title menu.
/// Per frame the Game library calls <see cref="Update"/> first, then any Notify* method the simulation's events
/// call for, then reads <see cref="Commands"/> (cleared at the start of the next <see cref="Update"/>).
/// </summary>
public sealed class ScreenStateMachine
{
    /// <summary>Idle seconds on the title before attract mode starts.</summary>
    public const double AttractIdleSeconds = 15.0;

    /// <summary>Longest attract-mode run before returning to the title.</summary>
    public const double AttractMaxSeconds = 45.0;

    /// <summary>The splash gives way to the title after this long even if the overlay never reports completion.</summary>
    public const double SplashMaxSeconds = 8.0;

    /// <summary>The briefing ignores Confirm for this long (so a held button does not skip it).</summary>
    public const double BriefingMinSeconds = 1.0;

    /// <summary>The sector-clear screen ignores Confirm for this long.</summary>
    public const double SectorClearMinSeconds = 1.0;

    /// <summary>The game-over screen ignores Confirm for this long.</summary>
    public const double GameOverMinSeconds = 2.0;

    /// <summary>The game-over screen moves on by itself after this long.</summary>
    public const double GameOverAutoSeconds = 8.0;

    /// <summary>Highest start sector that can be unlocked (the first loop's last sector).</summary>
    public const int MaxUnlockableSector = SectorRules.SectorsPerLoop;

    /// <summary>Default number of settings rows.</summary>
    public const int DefaultSettingsItemCount = 9;

    private readonly List<ScreenCommand> _commands = new List<ScreenCommand>();
    private readonly int[] _unlockedSector = { 1, 1, 1, 1 };

    /// <summary>Creates the machine on the splash screen.</summary>
    /// <param name="settingsItemCount">Rows on the settings screen (the Game library owns their meaning).</param>
    /// <param name="difficulty">Default difficulty.</param>
    /// <param name="shipShape">Default ship shape, 0..2.</param>
    /// <param name="shipColour">Default ship colour, 0..3.</param>
    /// <exception cref="ArgumentOutOfRangeException">When the item count is not positive.</exception>
    public ScreenStateMachine(int settingsItemCount = DefaultSettingsItemCount, Difficulty difficulty = Difficulty.Pilot,
        int shipShape = 0, int shipColour = 0)
    {
        if (settingsItemCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settingsItemCount), settingsItemCount, "Must be positive.");
        }

        SettingsItemCount = settingsItemCount;
        Difficulty = difficulty;
        ShipShape = Wrap(shipShape, GameSetup.ShipShapeCount);
        ShipColour = Wrap(shipColour, GameSetup.ShipColourCount);
        CurrentScreen = GameScreen.Splash;
        PreviousScreen = GameScreen.Splash;
        NameEntry = new NameEntry();
    }

    /// <summary>The current screen.</summary>
    public GameScreen CurrentScreen { get; private set; }

    /// <summary>The screen before the current one.</summary>
    public GameScreen PreviousScreen { get; private set; }

    /// <summary>Seconds on the current screen.</summary>
    public double ScreenTime { get; private set; }

    /// <summary>Seconds since the last input.</summary>
    public double IdleTime { get; private set; }

    /// <summary>Commands produced since the start of the last <see cref="Update"/>.</summary>
    public IReadOnlyList<ScreenCommand> Commands => _commands;

    /// <summary>Title menu cursor.</summary>
    public TitleMenuItem TitleCursor { get; private set; }

    /// <summary>Pause menu cursor.</summary>
    public PauseMenuItem PauseCursor { get; private set; }

    /// <summary>Settings cursor, 0..<see cref="SettingsItemCount"/>-1.</summary>
    public int SettingsCursor { get; private set; }

    /// <summary>Rows on the settings screen.</summary>
    public int SettingsItemCount { get; }

    /// <summary>Selected ship shape, 0..2.</summary>
    public int ShipShape { get; private set; }

    /// <summary>Selected ship colour, 0..3.</summary>
    public int ShipColour { get; private set; }

    /// <summary>Selected difficulty.</summary>
    public Difficulty Difficulty { get; private set; }

    /// <summary>Selected start sector (1..unlocked sector of the difficulty).</summary>
    public int StartSector { get; private set; } = 1;

    /// <summary>The sector being briefed / played / cleared.</summary>
    public int CurrentSector { get; private set; } = 1;

    /// <summary>The final score reported by <see cref="NotifyGameOver"/>.</summary>
    public long FinalScore { get; private set; }

    /// <summary>True when the last game's score qualifies for the high-score table.</summary>
    public bool PendingHighScore { get; private set; }

    /// <summary>The name entry state (valid on <see cref="GameScreen.HighScoreEntry"/>).</summary>
    public NameEntry NameEntry { get; private set; }

    /// <summary>The last name entered (pre-fills the next entry).</summary>
    public string LastName { get; private set; } = "AAA";

    /// <summary>Difficulty whose table the high-scores screen shows.</summary>
    public Difficulty HighScoreViewDifficulty { get; private set; }

    /// <summary>The highest start sector unlocked for a difficulty (1..5).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>The unlocked sector.</returns>
    public int UnlockedSector(Difficulty difficulty) => _unlockedSector[(int)difficulty];

    /// <summary>Restores a persisted unlock (clamped to 1..5).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="sector">Highest start sector.</param>
    public void SetUnlockedSector(Difficulty difficulty, int sector)
        => _unlockedSector[(int)difficulty] = Math.Clamp(sector, 1, MaxUnlockableSector);

    /// <summary>Restores the persisted last name.</summary>
    /// <param name="name">The name.</param>
    public void SetLastName(string name) => LastName = NameEntry.Normalize(name);

    /// <summary>Advances timers and applies one frame of menu input.</summary>
    /// <param name="dt">Seconds since the last frame.</param>
    /// <param name="input">Edge-triggered menu input.</param>
    public void Update(double dt, MenuInput input)
    {
        _commands.Clear();
        ScreenTime += Math.Max(0, dt);
        IdleTime = input.HasAnyInput ? 0 : IdleTime + Math.Max(0, dt);
        var confirm = input.Confirm || input.Start;

        switch (CurrentScreen)
        {
            case GameScreen.Splash:
                if (confirm || input.Back || ScreenTime >= SplashMaxSeconds)
                {
                    Go(GameScreen.Title);
                }

                break;
            case GameScreen.Title:
                UpdateTitle(input, confirm);
                break;
            case GameScreen.Attract:
                if (input.HasAnyInput || ScreenTime >= AttractMaxSeconds)
                {
                    _commands.Add(new ScreenCommand(ScreenCommandKind.StopAttract));
                    Go(GameScreen.Title);
                }

                break;
            case GameScreen.ShipSelect:
                UpdateShipSelect(input, confirm);
                break;
            case GameScreen.DifficultySelect:
                UpdateDifficultySelect(input, confirm);
                break;
            case GameScreen.SectorBriefing:
                OpenLinkIfAsked(input);
                if (confirm && ScreenTime >= BriefingMinSeconds)
                {
                    _commands.Add(new ScreenCommand(ScreenCommandKind.BeginSector, CurrentSector));
                    Go(GameScreen.Playing);
                }

                break;
            case GameScreen.Playing:
                if (input.Pause || input.Start)
                {
                    PauseCursor = PauseMenuItem.Resume;
                    _commands.Add(new ScreenCommand(ScreenCommandKind.PauseGame));
                    Go(GameScreen.Paused);
                }

                break;
            case GameScreen.Paused:
                UpdatePaused(input);
                break;
            case GameScreen.SectorClear:
                OpenLinkIfAsked(input);
                if (confirm && ScreenTime >= SectorClearMinSeconds)
                {
                    CurrentSector++;
                    Go(GameScreen.SectorBriefing);
                }

                break;
            case GameScreen.GameOver:
                if ((confirm && ScreenTime >= GameOverMinSeconds) || ScreenTime >= GameOverAutoSeconds)
                {
                    if (PendingHighScore)
                    {
                        NameEntry = new NameEntry(LastName);
                        Go(GameScreen.HighScoreEntry);
                    }
                    else
                    {
                        HighScoreViewDifficulty = Difficulty;
                        Go(GameScreen.Title);
                    }
                }

                break;
            case GameScreen.HighScoreEntry:
                if (NameEntry.Handle(input))
                {
                    LastName = NameEntry.Name;
                    PendingHighScore = false;
                    HighScoreViewDifficulty = Difficulty;
                    _commands.Add(new ScreenCommand(ScreenCommandKind.SubmitHighScore, text: NameEntry.Name));
                    Go(GameScreen.HighScores);
                }

                break;
            case GameScreen.HighScores:
                if (input.Left)
                {
                    HighScoreViewDifficulty = (Difficulty)Wrap((int)HighScoreViewDifficulty - 1, 4);
                }

                if (input.Right)
                {
                    HighScoreViewDifficulty = (Difficulty)Wrap((int)HighScoreViewDifficulty + 1, 4);
                }

                if (confirm || input.Back)
                {
                    Go(GameScreen.Title);
                }

                break;
            case GameScreen.Settings:
                UpdateSettings(input);
                break;
            case GameScreen.Credits:
                OpenLinkIfAsked(input);
                if (confirm || input.Back)
                {
                    Go(GameScreen.Title);
                }

                break;
        }
    }

    /// <summary>The splash overlay finished: go to the title.</summary>
    public void NotifySplashComplete()
    {
        if (CurrentScreen == GameScreen.Splash)
        {
            Go(GameScreen.Title);
        }
    }

    /// <summary>The simulation reported SectorCleared: show the sector-clear screen and unlock the next start sector.</summary>
    /// <param name="sector">The cleared sector.</param>
    public void NotifySectorCleared(int sector)
    {
        if (CurrentScreen != GameScreen.Playing && CurrentScreen != GameScreen.Paused)
        {
            return;
        }

        CurrentSector = sector;
        var index = (int)Difficulty;
        _unlockedSector[index] = Math.Max(_unlockedSector[index], Math.Min(sector + 1, MaxUnlockableSector));
        Go(GameScreen.SectorClear);
    }

    /// <summary>The simulation reported GameOver.</summary>
    /// <param name="finalScore">The final score.</param>
    /// <param name="qualifiesForHighScore">True when it makes the high-score table.</param>
    public void NotifyGameOver(long finalScore, bool qualifiesForHighScore)
    {
        if (CurrentScreen != GameScreen.Playing && CurrentScreen != GameScreen.Paused)
        {
            return;
        }

        FinalScore = finalScore;
        PendingHighScore = qualifiesForHighScore;
        Go(GameScreen.GameOver);
    }

    /// <summary>The attract-mode simulation ended (its player ran out of lives): back to the title.</summary>
    public void NotifyAttractEnded()
    {
        if (CurrentScreen == GameScreen.Attract)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.StopAttract));
            Go(GameScreen.Title);
        }
    }

    private static int Wrap(int value, int count) => ((value % count) + count) % count;

    private void Go(GameScreen screen)
    {
        PreviousScreen = CurrentScreen;
        CurrentScreen = screen;
        ScreenTime = 0;
        IdleTime = 0;
        _commands.Add(new ScreenCommand(ScreenCommandKind.ScreenChanged, (int)screen));
    }

    private void OpenLinkIfAsked(MenuInput input)
    {
        if (input.KenneyLink)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.OpenKenneyLink));
        }
    }

    private void UpdateTitle(MenuInput input, bool confirm)
    {
        const int count = 5;
        if (input.Up)
        {
            TitleCursor = (TitleMenuItem)Wrap((int)TitleCursor - 1, count);
        }

        if (input.Down)
        {
            TitleCursor = (TitleMenuItem)Wrap((int)TitleCursor + 1, count);
        }

        if (confirm)
        {
            switch (TitleCursor)
            {
                case TitleMenuItem.Play:
                    Go(GameScreen.ShipSelect);
                    break;
                case TitleMenuItem.HighScores:
                    HighScoreViewDifficulty = Difficulty;
                    Go(GameScreen.HighScores);
                    break;
                case TitleMenuItem.Settings:
                    SettingsCursor = 0;
                    Go(GameScreen.Settings);
                    break;
                case TitleMenuItem.Credits:
                    Go(GameScreen.Credits);
                    break;
                default:
                    _commands.Add(new ScreenCommand(ScreenCommandKind.QuitGame));
                    break;
            }

            return;
        }

        if (IdleTime >= AttractIdleSeconds)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.StartAttract));
            Go(GameScreen.Attract);
        }
    }

    private void UpdateShipSelect(MenuInput input, bool confirm)
    {
        if (input.Left)
        {
            ShipShape = Wrap(ShipShape - 1, GameSetup.ShipShapeCount);
        }

        if (input.Right)
        {
            ShipShape = Wrap(ShipShape + 1, GameSetup.ShipShapeCount);
        }

        if (input.Up)
        {
            ShipColour = Wrap(ShipColour - 1, GameSetup.ShipColourCount);
        }

        if (input.Down)
        {
            ShipColour = Wrap(ShipColour + 1, GameSetup.ShipColourCount);
        }

        if (confirm)
        {
            StartSector = Math.Clamp(StartSector, 1, UnlockedSector(Difficulty));
            Go(GameScreen.DifficultySelect);
        }
        else if (input.Back)
        {
            Go(GameScreen.Title);
        }
    }

    private void UpdateDifficultySelect(MenuInput input, bool confirm)
    {
        if (input.Up)
        {
            Difficulty = (Difficulty)Wrap((int)Difficulty - 1, 4);
        }

        if (input.Down)
        {
            Difficulty = (Difficulty)Wrap((int)Difficulty + 1, 4);
        }

        var unlocked = UnlockedSector(Difficulty);
        if (input.Left)
        {
            StartSector--;
        }

        if (input.Right)
        {
            StartSector++;
        }

        StartSector = Math.Clamp(StartSector, 1, unlocked);
        if (confirm)
        {
            CurrentSector = StartSector;
            _commands.Add(new ScreenCommand(ScreenCommandKind.StartNewGame, StartSector));
            Go(GameScreen.SectorBriefing);
        }
        else if (input.Back)
        {
            Go(GameScreen.ShipSelect);
        }
    }

    private void UpdatePaused(MenuInput input)
    {
        if (input.Pause || input.Start || input.Back)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.ResumeGame));
            Go(GameScreen.Playing);
            return;
        }

        if (input.Up || input.Down)
        {
            PauseCursor = PauseCursor == PauseMenuItem.Resume ? PauseMenuItem.QuitToTitle : PauseMenuItem.Resume;
        }

        if (input.Confirm)
        {
            if (PauseCursor == PauseMenuItem.Resume)
            {
                _commands.Add(new ScreenCommand(ScreenCommandKind.ResumeGame));
                Go(GameScreen.Playing);
            }
            else
            {
                _commands.Add(new ScreenCommand(ScreenCommandKind.AbandonGame));
                Go(GameScreen.Title);
            }
        }
    }

    private void UpdateSettings(MenuInput input)
    {
        if (input.Up)
        {
            SettingsCursor = Wrap(SettingsCursor - 1, SettingsItemCount);
        }

        if (input.Down)
        {
            SettingsCursor = Wrap(SettingsCursor + 1, SettingsItemCount);
        }

        if (input.Left)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.AdjustSetting, SettingsCursor, -1));
        }

        if (input.Right)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.AdjustSetting, SettingsCursor, 1));
        }

        if (input.Confirm || input.Start)
        {
            _commands.Add(new ScreenCommand(ScreenCommandKind.ActivateSetting, SettingsCursor));
        }
        else if (input.Back)
        {
            Go(GameScreen.Title);
        }
    }
}
