using System.Collections.Generic;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Input.Actions;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>An in-memory <see cref="IGameSettings"/> with the documented defaults, recording saves.</summary>
internal sealed class MemoryGameSettings : IGameSettings
{
    private readonly Dictionary<Difficulty, int> _cleared = new Dictionary<Difficulty, int>();
    private HighScoreTable _stored = new HighScoreTable();

    public int ShipShape { get; set; }

    public int ShipColour { get; set; }

    public Difficulty Difficulty { get; set; } = Difficulty.Pilot;

    public double MasterVolume { get; set; } = SettingsService.DefaultMasterVolume;

    public double MusicVolume { get; set; } = SettingsService.DefaultMusicVolume;

    public double EffectsVolume { get; set; } = SettingsService.DefaultEffectsVolume;

    public string MusicGenerator { get; set; } = "SkyTNT";

    public string InstrumentLibrary { get; set; } = "ModestSynthGm";

    public GamepadProfile GamepadProfile { get; set; }

    public InputDeviceKind LastInputDevice { get; set; }

    public string LastName { get; set; } = SettingsService.DefaultLastName;

    public int SaveCount { get; private set; }

    public int ResetCount { get; private set; }

    public HighScoreTable Stored => _stored;

    public int GetHighestSectorCleared(Difficulty difficulty) => _cleared.TryGetValue(difficulty, out var sector) ? sector : 0;

    public void RecordSectorCleared(Difficulty difficulty, int sector)
    {
        if (sector > GetHighestSectorCleared(difficulty))
        {
            _cleared[difficulty] = sector;
        }
    }

    public HighScoreTable LoadHighScores() => HighScoreTable.FromLines(_stored.ToLines());

    public void SaveHighScores(HighScoreTable table, Difficulty difficulty)
    {
        SaveCount++;
        _stored = HighScoreTable.FromLines(table.ToLines());
    }

    public void ResetHighScores()
    {
        ResetCount++;
        _stored = new HighScoreTable();
    }
}
