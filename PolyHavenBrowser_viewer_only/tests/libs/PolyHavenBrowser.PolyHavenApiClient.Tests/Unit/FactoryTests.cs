using Microsoft.Extensions.DependencyInjection;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

public class FactoryTests
{
    [Fact]
    public void get_client_returns_a_new_instance_each_time()
    {
        //Arrange
        var stub = new StubHttpMessageHandler();
        var factory = new DefaultPolyHavenClientFactory(stub);

        //Act
        using var first = factory.GetClient();
        using var second = factory.GetClient();

        //Assert
        first.Should().NotBeSameAs(second);
        first.Should().BeOfType<RestPolyHavenApiClient>();
    }

    [Fact]
    public async Task disposing_one_client_leaves_other_clients_and_the_handler_usable()
    {
        //Arrange
        var stub = new StubHttpMessageHandler();
        stub.OnPath("/types", CannedJson.Types);
        var factory = new DefaultPolyHavenClientFactory(stub);
        var doomed = factory.GetClient();
        using var survivor = factory.GetClient();

        //Act
        doomed.Dispose();
        var types = await survivor.GetAssetTypesAsync();

        //Assert
        types.Should().HaveCount(3);
        stub.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void disposed_factory_rejects_get_client()
    {
        //Arrange
        var factory = new DefaultPolyHavenClientFactory();
        factory.Dispose();

        //Act
        var act = () => factory.GetClient();

        //Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task factory_copies_options_so_later_mutations_have_no_effect()
    {
        //Arrange
        var options = new PolyHavenClientOptions { UserAgent = "Original/1.0" };
        var stub = new StubHttpMessageHandler();
        var factory = new DefaultPolyHavenClientFactory(stub, options);

        //Act
        options.UserAgent = "Mutated/9.9";
        stub.OnPath("/types", CannedJson.Types);
        using var client = factory.GetClient();
        await client.GetAssetTypesAsync();

        //Assert
        stub.Requests.Should().ContainSingle()
            .Which.Headers.UserAgent.ToString().Should().Be("Original/1.0");
    }

    [Fact]
    public async Task di_registration_resolves_a_working_factory()
    {
        //Arrange
        var stub = new StubHttpMessageHandler();
        stub.OnPath("/types", CannedJson.Types);

        var services = new ServiceCollection();
        services.AddPolyHavenApiClient(options => options.UserAgent = "DiTest/1.0");
        services.AddHttpClient(DefaultPolyHavenClientFactory.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => stub);

        await using var provider = services.BuildServiceProvider();

        //Act
        var factory = provider.GetRequiredService<IPolyHavenApiClientFactory>();
        IReadOnlyList<string> types;
        using (var client = factory.GetClient())
        {
            types = await client.GetAssetTypesAsync();
        }

        //Assert
        factory.Should().BeOfType<DefaultPolyHavenClientFactory>();
        types.Should().Equal("hdris", "textures", "models");
        stub.Requests.Should().ContainSingle()
            .Which.Headers.UserAgent.ToString().Should().Be("DiTest/1.0");
    }

    [Fact]
    public async Task di_registration_is_idempotent()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddPolyHavenApiClient();
        services.AddPolyHavenApiClient();
        await using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetServices<IPolyHavenApiClientFactory>().Should().ContainSingle();
    }
}
