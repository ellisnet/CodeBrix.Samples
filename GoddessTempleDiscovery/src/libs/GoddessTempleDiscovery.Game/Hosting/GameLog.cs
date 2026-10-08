using System;
using System.Diagnostics;
using CodeBrix.Platform.GameEngine;
using Microsoft.Extensions.Logging;

namespace GoddessTempleDiscovery.Game.Hosting;

/// <summary>
/// The game's log: every line carries the <c>[GoddessTemple]</c> prefix and goes to Debug, the engine logger (the
/// console at Information) and the optional <see cref="Sink"/>.
/// </summary>
public static class GameLog
{
    /// <summary>The prefix of every line.</summary>
    public const string Prefix = "[GoddessTemple]";

    /// <summary>An extra receiver for every line (tests capture the log through it); null for none.</summary>
    public static Action<string> Sink { get; set; }

    /// <summary>Writes one line.</summary>
    /// <param name="message">The message, without the prefix.</param>
    public static void Write(string message)
    {
        var line = $"{Prefix} {message}";
        Debug.WriteLine(line);
        try
        {
            Engine.Logger.LogInformation("{Line}", line);
        }
        catch (Exception)
        {
            //A headless test run may have no engine logger to write to; the sink still has the line
        }

        Sink?.Invoke(line);
    }
}
