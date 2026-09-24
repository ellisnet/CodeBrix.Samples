using System;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Audio;

/// <summary>The real <see cref="IMusicStream"/>: a thin view over the engine's <see cref="GeneratedMusicProvider"/>.</summary>
public sealed class ProviderMusicStream : IMusicStream
{
    private readonly GeneratedMusicProvider _provider;

    /// <summary>Wraps a provider.</summary>
    /// <param name="provider">The provider <c>UseGeneratedMusic</c> returned.</param>
    public ProviderMusicStream(GeneratedMusicProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <inheritdoc />
    public event EventHandler StateChanged
    {
        add => _provider.StateChanged += value;
        remove => _provider.StateChanged -= value;
    }

    /// <summary>The wrapped provider.</summary>
    public GeneratedMusicProvider Provider => _provider;

    /// <inheritdoc />
    public StreamingMusicState State => _provider.State;

    /// <inheritdoc />
    public string Summary => _provider.ActiveSourceSummary;

    /// <inheritdoc />
    public MusicSourceInfo Source
    {
        get
        {
            var source = _provider.ActiveSource;
            return source == null ? null : new MusicSourceInfo(source.GeneratorName, source.InstrumentLibraryName, source.IsReplay);
        }
    }

    /// <inheritdoc />
    public Exception Fault => _provider.Fault;

    /// <inheritdoc />
    public int StarvationGapCount => _provider.Diagnostics?.StarvationGapCount ?? 0;

    /// <inheritdoc />
    public string DiagnosticsSummary => _provider.Diagnostics?.ToString() ?? string.Empty;

    /// <inheritdoc />
    public void FollowUp(string preset) => _provider.FollowUp(preset);
}
