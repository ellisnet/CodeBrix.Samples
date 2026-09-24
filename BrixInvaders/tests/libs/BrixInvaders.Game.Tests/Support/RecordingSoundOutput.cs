using System.Collections.Generic;
using BrixInvaders.Game.Audio;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>Records every sound and level change.</summary>
internal sealed class RecordingSoundOutput : ISoundOutput
{
    public List<SoundCue> Played { get; } = new List<SoundCue>();

    public List<(double Master, double Effects)> Levels { get; } = new List<(double Master, double Effects)>();

    public void Play(SoundCue cue) => Played.Add(cue);

    public void SetLevels(double master, double effects) => Levels.Add((master, effects));
}
