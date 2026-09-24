using System;
using BrixInvaders.Game.Session;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>A session with recording fakes, and helpers to drive it screen by screen.</summary>
internal sealed class SessionDriver
{
    public const double Step = Playfield.FixedStep;

    public SessionDriver(Action<MemoryGameSettings> configure = null, int seed = 11)
    {
        configure?.Invoke(Settings);
        Session = new GameSession(Settings, Sound, Music, Links, seed);
    }

    public static MenuInput Up => new MenuInput(up: true);

    public static MenuInput Down => new MenuInput(down: true);

    public static MenuInput Left => new MenuInput(left: true);

    public static MenuInput Right => new MenuInput(right: true);

    public static MenuInput Confirm => new MenuInput(confirm: true);

    public static MenuInput Back => new MenuInput(back: true);

    public static MenuInput Pause => new MenuInput(pause: true, back: true);

    public static MenuInput Link => new MenuInput(kenneyLink: true);

    public MemoryGameSettings Settings { get; } = new MemoryGameSettings();

    public RecordingSoundOutput Sound { get; } = new RecordingSoundOutput();

    public RecordingMusicDirector Music { get; } = new RecordingMusicDirector();

    public FakeLinkOpener Links { get; } = new FakeLinkOpener();

    public GameSession Session { get; }

    public void Press(MenuInput input) => Session.Update(Step, input, GameInput.None);

    public void Wait(double seconds, GameInput play = default)
    {
        var steps = (int)Math.Ceiling(seconds / Step);
        for (var i = 0; i < steps; i++)
        {
            Session.Update(Step, MenuInput.None, play);
        }
    }

    public void ToTitle()
    {
        Session.NotifySplashComplete();
    }

    public void ToBriefing()
    {
        ToTitle();
        Press(Confirm);
        Press(Confirm);
        Press(Confirm);
    }

    public void ToPlaying()
    {
        ToBriefing();
        Wait(ScreenStateMachine.BriefingMinSeconds + Step);
        Press(Confirm);
    }

    public bool PlayUntil(Func<GameSession, bool> done, double maxSeconds, GameInput play = default)
    {
        var steps = (int)Math.Ceiling(maxSeconds / Step);
        for (var i = 0; i < steps; i++)
        {
            Session.Update(Step, MenuInput.None, play);
            if (done(Session))
            {
                return true;
            }
        }

        return false;
    }
}
