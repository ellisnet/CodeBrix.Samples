using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.DependencyInjection;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Tests demonstrating how application code that depends on
/// <see cref="IPolyHavenApiClientFactory"/> can be tested with CodeBrix.TestMocks.
/// </summary>
public class MockedFactoryTests
{
    [Fact]
    public void mocked_factory_hands_out_the_configured_client()
    {
        //Arrange
        var clientMock = new Mock<IPolyHavenApiClient>();
        var factoryMock = new Mock<IPolyHavenApiClientFactory>();
        factoryMock.Setup(f => f.GetClient()).Returns(clientMock.Object);

        //Act
        var client = factoryMock.Object.GetClient();

        //Assert
        client.Should().BeSameAs(clientMock.Object);
        factoryMock.Verify(f => f.GetClient(), Times.Once);
    }

    [Fact]
    public void mocked_factory_can_hand_out_a_fresh_client_per_call()
    {
        //Arrange
        var factoryMock = new Mock<IPolyHavenApiClientFactory>();
        factoryMock.Setup(f => f.GetClient()).Returns(() => new Mock<IPolyHavenApiClient>().Object);

        //Act
        var first = factoryMock.Object.GetClient();
        var second = factoryMock.Object.GetClient();

        //Assert
        first.Should().NotBeSameAs(second);
        factoryMock.Verify(f => f.GetClient(), Times.Exactly(2));
    }

    [Fact]
    public async Task consumer_resolved_from_di_uses_the_mocked_factory_and_disposes_the_client()
    {
        //Arrange
        var clientMock = new Mock<IPolyHavenApiClient>();
        clientMock.Setup(c => c.GetAssetTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "hdris", "textures", "models" });

        var factoryMock = new Mock<IPolyHavenApiClientFactory>();
        factoryMock.Setup(f => f.GetClient()).Returns(clientMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(factoryMock.Object);
        await using var provider = services.BuildServiceProvider();

        //Act - the typical consumer pattern: get a client, use it, dispose it
        var factory = provider.GetRequiredService<IPolyHavenApiClientFactory>();
        IReadOnlyList<string> types;
        using (var client = factory.GetClient())
        {
            types = await client.GetAssetTypesAsync();
        }

        //Assert
        types.Should().HaveCount(3);
        factoryMock.Verify(f => f.GetClient(), Times.Once);
        clientMock.Verify(c => c.GetAssetTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
        clientMock.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task strict_factory_and_client_mocks_verify_the_complete_interaction()
    {
        //Arrange
        var clientMock = new Mock<IPolyHavenApiClient>(MockBehavior.Strict);
        clientMock.Setup(c => c.GetAssetAsync("abandoned_bakery", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolyHavenAsset { Id = "abandoned_bakery", Name = "Abandoned Bakery" });
        clientMock.Setup(c => c.Dispose());

        var factoryMock = new Mock<IPolyHavenApiClientFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.GetClient()).Returns(clientMock.Object);

        //Act
        string? assetName;
        using (var client = factoryMock.Object.GetClient())
        {
            assetName = (await client.GetAssetAsync("abandoned_bakery")).Name;
        }

        //Assert
        assetName.Should().Be("Abandoned Bakery");
        factoryMock.VerifyAll();
        clientMock.VerifyAll();
        clientMock.VerifyNoOtherCalls();
    }
}
