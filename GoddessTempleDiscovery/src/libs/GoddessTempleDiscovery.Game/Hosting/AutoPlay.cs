using System;
using System.Globalization;

namespace GoddessTempleDiscovery.Game.Hosting;

/// <summary>
/// The autoplay switch: <c>GODDESSTEMPLE_AUTOPLAY=1</c> makes the host press New Game itself, seat four computer
/// teams with one turn per season, play the game to the end, write the Field Journal PDF to the temp folder and log
/// <c>GODDESSTEMPLE AUTOPLAY PASS</c>. <c>GODDESSTEMPLE_AUTOPLAY_SEED=n</c> pins the game.
/// </summary>
public static class AutoPlay
{
    /// <summary>The environment variable that turns autoplay on when set to 1.</summary>
    public const string Variable = "GODDESSTEMPLE_AUTOPLAY";

    /// <summary>The environment variable that pins the autoplay game's seed.</summary>
    public const string SeedVariable = "GODDESSTEMPLE_AUTOPLAY_SEED";

    /// <summary>
    /// The environment variable naming a PNG file: an autoplay game saves the table's backbuffer there once, during a
    /// mid-game turn (a look at the HUD without a person at the table).
    /// </summary>
    public const string ScreenshotVariable = "GODDESSTEMPLE_AUTOPLAY_SCREENSHOT";

    /// <summary>The line logged when an autoplay game reaches its end.</summary>
    public const string PassLine = "GODDESSTEMPLE AUTOPLAY PASS";

    /// <summary>The seconds an autoplay inspector stays open before it closes itself (a person gets six).</summary>
    public const double InspectorHoldSeconds = 1.5;

    /// <summary>Whether autoplay is requested.</summary>
    /// <returns>True when <see cref="Variable"/> is 1.</returns>
    public static bool IsRequested() => Environment.GetEnvironmentVariable(Variable) == "1";

    /// <summary>The mid-game screenshot's path, or null for none.</summary>
    /// <returns>The path from <see cref="ScreenshotVariable"/>.</returns>
    public static string ScreenshotPath()
    {
        var path = Environment.GetEnvironmentVariable(ScreenshotVariable);
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    /// <summary>The pinned seed, or null for a fresh game.</summary>
    /// <returns>The seed from <see cref="SeedVariable"/>, when it parses.</returns>
    public static int? Seed() =>
        int.TryParse(Environment.GetEnvironmentVariable(SeedVariable), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seed)
            ? seed
            : null;
}
