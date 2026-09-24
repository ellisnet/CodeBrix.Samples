using BrixInvaders.Music;

namespace BrixInvaders.Game.Audio;

/// <summary>The default <see cref="IMusicDirector"/>: no music; every call does nothing.</summary>
public sealed class SilentMusicDirector : IMusicDirector
{
    /// <inheritdoc />
    public string ActiveSource => string.Empty;

    /// <inheritdoc />
    public void Start(MusicSettings settings, double musicVolume)
    {
    }

    /// <inheritdoc />
    public void OnTitle()
    {
    }

    /// <inheritdoc />
    public void OnSector(int sector)
    {
    }

    /// <inheritdoc />
    public void OnBoss(int sector)
    {
    }

    /// <inheritdoc />
    public void OnGameOver()
    {
    }

    /// <inheritdoc />
    public void OnPause()
    {
    }

    /// <inheritdoc />
    public void OnResume()
    {
    }

    /// <inheritdoc />
    public void SetVolumes(double master, double music, double effects)
    {
    }

    /// <inheritdoc />
    public void ApplySettings(MusicSettings settings, int sector, bool boss)
    {
    }

    /// <inheritdoc />
    public void Stop()
    {
    }
}
