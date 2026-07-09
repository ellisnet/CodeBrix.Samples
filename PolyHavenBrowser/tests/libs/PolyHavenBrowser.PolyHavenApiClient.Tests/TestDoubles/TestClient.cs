namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Creates <see cref="IPolyHavenApiClient"/> instances wired to a
/// <see cref="StubHttpMessageHandler"/> for offline unit tests.
/// </summary>
internal static class TestClient
{
    public static (IPolyHavenApiClient Client, StubHttpMessageHandler Stub) Create(
        PolyHavenClientOptions? options = null)
    {
        var stub = new StubHttpMessageHandler();
        var factory = new DefaultPolyHavenClientFactory(stub, options);
        return (factory.GetClient(), stub);
    }
}
