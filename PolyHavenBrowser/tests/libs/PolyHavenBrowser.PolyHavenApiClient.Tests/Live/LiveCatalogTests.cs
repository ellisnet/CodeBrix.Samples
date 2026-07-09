using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>Live tests for the catalog-browsing endpoints (/types, /assets, /categories).</summary>
[Collection("LiveApi")]
[Trait("Category", "LiveApi")]
public class LiveCatalogTests(LiveApiFixture fixture)
{
    private IPolyHavenApiClient Client => fixture.Client;

    [Fact]
    public async Task asset_types_include_the_three_known_types()
    {
        //Act
        var types = await Client.GetAssetTypesAsync();

        //Assert
        types.Should().Contain("hdris");
        types.Should().Contain("textures");
        types.Should().Contain("models");
    }

    [Fact]
    public async Task full_catalog_is_large_and_has_ids_assigned()
    {
        //Act
        var assets = await Client.GetAssetsAsync();

        //Assert
        assets.Count.Should().BeGreaterThan(1000);
        assets.Should().AllSatisfy(pair =>
        {
            pair.Value.Id.Should().Be(pair.Key);
            pair.Value.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task hdri_filter_returns_only_hdris()
    {
        //Act
        var assets = await Client.GetAssetsAsync(PolyHavenAssetType.Hdri);

        //Assert
        assets.Should().NotBeEmpty();
        assets.Should().ContainKey("abandoned_bakery");
        assets.Values.Should().AllSatisfy(asset => asset.Type.Should().Be(PolyHavenAssetType.Hdri));
    }

    [Fact]
    public async Task texture_filter_returns_only_textures()
    {
        //Act
        var assets = await Client.GetAssetsAsync(PolyHavenAssetType.Texture);

        //Assert
        assets.Should().NotBeEmpty();
        assets.Should().ContainKey("aerial_rocks_02");
        assets.Values.Should().AllSatisfy(asset => asset.Type.Should().Be(PolyHavenAssetType.Texture));
    }

    [Fact]
    public async Task model_filter_returns_only_models()
    {
        //Act
        var assets = await Client.GetAssetsAsync(PolyHavenAssetType.Model);

        //Assert
        assets.Should().NotBeEmpty();
        assets.Should().ContainKey("potted_plant_01");
        assets.Values.Should().AllSatisfy(asset => asset.Type.Should().Be(PolyHavenAssetType.Model));
    }

    [Fact]
    public async Task category_filter_limits_the_results()
    {
        //Act
        var outdoor = await Client.GetAssetsAsync(PolyHavenAssetType.Hdri, ["outdoor"]);
        var all = await Client.GetAssetsAsync(PolyHavenAssetType.Hdri);

        //Assert
        outdoor.Should().NotBeEmpty();
        outdoor.Count.Should().BeLessThan(all.Count);
        outdoor.Values.Should().AllSatisfy(asset => asset.Categories.Should().Contain("outdoor"));
    }

    [Fact]
    public async Task categories_are_returned_with_counts_for_every_type()
    {
        foreach (var type in new[]
        {
            PolyHavenAssetType.Hdri, PolyHavenAssetType.Texture, PolyHavenAssetType.Model,
        })
        {
            //Act
            var categories = await Client.GetCategoriesAsync(type);

            //Assert
            categories.Should().NotBeEmpty();
            categories.Should().ContainKey("all");
            categories["all"].Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task categories_respect_the_in_filter()
    {
        //Act
        var unfiltered = await Client.GetCategoriesAsync(PolyHavenAssetType.Hdri);
        var filtered = await Client.GetCategoriesAsync(PolyHavenAssetType.Hdri, ["outdoor"]);

        //Assert - the filtered response omits the "all" pseudo-category, so compare a real one
        filtered.Should().NotBeEmpty();
        filtered.Should().ContainKey("natural light");
        filtered["natural light"].Should().BeLessThan(unfiltered["natural light"]);
    }
}
