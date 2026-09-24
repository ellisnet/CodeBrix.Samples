using System;
using System.Diagnostics;
using CodeBrix.Platform.GameEngine;
using Microsoft.Extensions.Logging;

namespace BrixInvaders.Game.Hosting;

/// <summary>The game's log: every line carries the <c>[BrixInvaders]</c> prefix and goes to Debug and the engine logger.</summary>
public static class GameLog
{
    /// <summary>The prefix of every line.</summary>
    public const string Prefix = "[BrixInvaders]";

    /// <summary>An extra receiver for every line (tests capture the log through it); null for none.</summary>
    public static Action<string> Sink { get; set; }

    /// <summary>Writes one line.</summary>
    /// <param name="message">The message, without the prefix.</param>
    public static void Write(string message)
    {
        var line = $"{Prefix} {message}";
        Debug.WriteLine(line);
        Engine.Logger.LogInformation("{Line}", line);
        Sink?.Invoke(line);
    }
}
