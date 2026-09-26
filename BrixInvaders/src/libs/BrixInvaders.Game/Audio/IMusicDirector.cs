using BrixInvaders.Music;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// SEAM (implemented by the music wiring): everything the game tells the music. The game calls these at the
/// moments below and never touches the music otherwise; <see cref="SilentMusicDirector"/> is the no-op default.
/// </summary>
/// <remarks>
/// <para>
/// Every call arrives on the ENGINE thread. Calls are made only when the musical moment actually changes (going from
/// one menu to another is not a change), so an implementation need not de-duplicate.
/// </para>
/// <para>
/// Volumes: the game applies the master and effects levels to the engine mixer itself
/// (<c>AudioMixer.MasterVolume</c> / <c>AudioMixer.SfxVolume</c>); the MUSIC level is only ever passed here, so an
/// implementation applies it exactly once (through the provider's own volume, or the music bus - not both).
/// </para>
/// </remarks>
public interface IMusicDirector
{
    /// <summary>
    /// Once, at start-up, after <c>AudioSystem.Initialize</c> and the asset loads, before the splash: start the title
    /// music with the player's choices.
    /// </summary>
    /// <param name="settings">The player's music choices (generator and instrument library).</param>
    /// <param name="musicVolume">The player's music volume, 0..1.</param>
    void Start(MusicSettings settings, double musicVolume);

    /// <summary>The game is back on the title and its menus (after the splash, a game over or quitting a game).</summary>
    void OnTitle();

    /// <summary>A sector is about to be played: its briefing has opened.</summary>
    /// <param name="sector">The sector number (1, 2, ...; sectors past 5 repeat the designs).</param>
    void OnSector(int sector);

    /// <summary>A boss is incoming (the warning has started) in a sector.</summary>
    /// <param name="sector">The sector number.</param>
    void OnBoss(int sector);

    /// <summary>The game is over (the game-over screen has opened).</summary>
    void OnGameOver();

    /// <summary>The player paused the game (the pause menu is up; the simulation is frozen).</summary>
    void OnPause();

    /// <summary>The player resumed from the pause menu.</summary>
    void OnResume();

    /// <summary>The player moved a volume slider (also called once after <see cref="Start"/>).</summary>
    /// <param name="master">Master volume, 0..1 (already applied to the engine mixer).</param>
    /// <param name="music">Music volume, 0..1 (apply it - the game does not).</param>
    /// <param name="effects">Effects volume, 0..1 (already applied to the engine mixer).</param>
    void SetVolumes(double master, double music, double effects);

    /// <summary>The player changed the music model or the instrument library on the settings screen.</summary>
    /// <param name="settings">The new choices.</param>
    /// <param name="sector">The sector the music should suit (0 = the title).</param>
    /// <param name="boss">Whether a boss is on.</param>
    void ApplySettings(MusicSettings settings, int sector, bool boss);

    /// <summary>One line saying what is playing (for logs and tests); empty when nothing is.</summary>
    string ActiveSource { get; }

    /// <summary>The game is shutting down.</summary>
    void Stop();
}
