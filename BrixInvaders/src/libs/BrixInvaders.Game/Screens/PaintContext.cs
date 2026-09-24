using System;
using System.Collections.Generic;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Rendering;
using BrixInvaders.Game.Session;

namespace BrixInvaders.Game.Screens;

/// <summary>Everything a screen needs to draw one frame.</summary>
public sealed class PaintContext
{
    /// <summary>Creates a context.</summary>
    /// <param name="session">The game session.</param>
    /// <param name="frame">The frame being built.</param>
    /// <param name="playfield">The playfield painter (worlds, flashes, stars).</param>
    /// <param name="credits">The credits content seam.</param>
    /// <param name="packCredits">The credit line of every Kenney pack read.</param>
    public PaintContext(GameSession session, FrameBuilder frame, PlayfieldPainter playfield, ICreditsContent credits,
        IReadOnlyList<string> packCredits)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Frame = frame ?? throw new ArgumentNullException(nameof(frame));
        Playfield = playfield ?? throw new ArgumentNullException(nameof(playfield));
        Credits = credits ?? throw new ArgumentNullException(nameof(credits));
        PackCredits = packCredits ?? throw new ArgumentNullException(nameof(packCredits));
    }

    /// <summary>The game session.</summary>
    public GameSession Session { get; }

    /// <summary>The frame being built.</summary>
    public FrameBuilder Frame { get; }

    /// <summary>The playfield painter.</summary>
    public PlayfieldPainter Playfield { get; }

    /// <summary>The credits content seam.</summary>
    public ICreditsContent Credits { get; }

    /// <summary>The credit line of every Kenney pack read.</summary>
    public IReadOnlyList<string> PackCredits { get; }

    /// <summary>The device prompts are shown for.</summary>
    public InputDevice Device { get; set; }

    /// <summary>The gamepad profile (which buttons fire and bomb).</summary>
    public GamepadProfile Profile { get; set; }

    /// <summary>Seconds since the session started.</summary>
    public double Time => Session.Time;
}
