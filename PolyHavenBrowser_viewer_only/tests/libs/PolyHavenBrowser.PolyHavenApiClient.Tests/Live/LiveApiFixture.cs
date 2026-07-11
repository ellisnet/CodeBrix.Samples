using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Shares one factory and client across all live-API test classes so the whole live suite
/// reuses a single HTTP connection pool. Live tests carry
/// <c>[Trait("Category", "LiveApi")]</c> and can be excluded with
/// <c>dotnet test --filter Category!=LiveApi</c>.
/// </summary>
public sealed class LiveApiFixture : IDisposable
{
    public LiveApiFixture()
    {
        Factory = new DefaultPolyHavenClientFactory(new PolyHavenClientOptions
        {
            UserAgent = "PolyHavenBrowser.PolyHavenApiClient.Tests/1.0",
        });
        Client = Factory.GetClient();
    }

    public DefaultPolyHavenClientFactory Factory { get; }

    public IPolyHavenApiClient Client { get; }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}

[CollectionDefinition("LiveApi")]
public sealed class LiveApiCollection : ICollectionFixture<LiveApiFixture>;
