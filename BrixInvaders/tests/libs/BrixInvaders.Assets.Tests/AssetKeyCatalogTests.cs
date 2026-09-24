using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class AssetKeyCatalogTests
{
    [Fact]
    public void AllKeys_are_distinct_kenney_keys_from_every_asset_group()
    {
        //Arrange
        var keys = AssetKeyCatalog.AllKeys;

        //Act
        var packs = keys.Select(key => key.Split('/')[0]).Distinct().ToList();

        //Assert
        keys.Should().OnlyHaveUniqueItems();
        keys.Should().OnlyContain(key => key.StartsWith("kenney:"));
        packs.Should().BeEquivalentTo(KenneyPacks.Slugs.Select(slug => $"kenney:{slug}"));
        keys.Should().Contain([AssetKeys.Atlases.Extension, AssetKeys.Planets.Planet00, AssetKeys.Sfx.GameOver, AssetKeys.Promo.PlanetsSample]);
    }

    [Fact]
    public void FrameGroups_carry_their_atlas_and_only_frame_names()
    {
        //Arrange
        var groups = AssetKeyCatalog.FrameGroups;

        //Act
        var enemies = groups.Single(group => group.GroupName == nameof(AssetKeys.Enemies));

        //Assert
        enemies.AtlasKey.Should().Be(AssetKeys.Atlases.Main);
        enemies.FrameNames.Should().HaveCount(20);
        groups.SelectMany(group => group.FrameNames).Should().NotContain(frame => frame.StartsWith("kenney:"));
        groups.Single(group => group.GroupName == nameof(AssetKeys.Bosses)).AtlasKey.Should().Be(AssetKeys.Atlases.Extension);
    }
}
