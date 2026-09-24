using System.Collections.Generic;
using BrixInvaders.Game.Audio;
using BrixInvaders.Music;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>Records every call the game makes to the music seam, as short strings ("OnSector(1)").</summary>
internal sealed class RecordingMusicDirector : IMusicDirector
{
    public List<string> Calls { get; } = new List<string>();

    public string ActiveSource => "test source";

    public void Start(MusicSettings settings, double musicVolume) => Calls.Add($"Start({settings.GeneratorName}, {musicVolume})");

    public void OnTitle() => Calls.Add("OnTitle");

    public void OnSector(int sector) => Calls.Add($"OnSector({sector})");

    public void OnBoss(int sector) => Calls.Add($"OnBoss({sector})");

    public void OnGameOver() => Calls.Add("OnGameOver");

    public void OnPause() => Calls.Add("OnPause");

    public void OnResume() => Calls.Add("OnResume");

    public void SetVolumes(double master, double music, double effects) => Calls.Add($"SetVolumes({master}, {music}, {effects})");

    public void ApplySettings(MusicSettings settings, int sector, bool boss) =>
        Calls.Add($"ApplySettings({settings.GeneratorName}, {settings.InstrumentLibraryName}, {sector}, {boss})");

    public void Stop() => Calls.Add("Stop");
}
