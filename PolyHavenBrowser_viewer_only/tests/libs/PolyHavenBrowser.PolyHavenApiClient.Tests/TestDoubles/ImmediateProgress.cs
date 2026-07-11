namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// An <see cref="IProgress{T}"/> that records reports synchronously (unlike
/// <see cref="Progress{T}"/>, which posts to a synchronization context).
/// </summary>
internal sealed class ImmediateProgress<T> : IProgress<T>
{
    public List<T> Reports { get; } = [];

    public void Report(T value) => Reports.Add(value);
}
