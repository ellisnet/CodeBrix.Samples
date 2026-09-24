using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class BrixInvadersAssetsTests
{
    [Fact]
    public void Register_logs_one_line_per_pack_with_its_asset_count()
    {
        //Arrange
        List<string> lines = [];

        //Act
        KenneyGameAssetProvider provider = TestAssets.Register(TestAssets.Capture(lines));

        //Assert
        lines.Should().OnlyContain(line => line.StartsWith("[BrixInvaders] assets: ", StringComparison.Ordinal));
        foreach (string slug in KenneyPacks.Slugs)
        {
            int count = provider.Packs.Single(pack => pack.Slug == slug).AssetCount;
            lines.Where(line => line.Contains($" pack {slug} ") && line.Contains($" {count} asset(s)")).Should().ContainSingle();
        }

        lines.Should().Contain("[BrixInvaders] assets: provider warnings: none");
        provider.Warnings.Should().BeEmpty();
        lines.Should().NotContain(line => line.Contains("WARNING"));
    }

    [Fact]
    public void Register_twice_does_not_register_the_zips_a_second_time()
    {
        //Arrange
        KenneyGameAssetProvider first = TestAssets.Register();
        int packs = first.Packs.Count;

        //Act
        KenneyGameAssetProvider second = TestAssets.Register();

        //Assert
        second.Should().BeSameAs(first);
        second.Packs.Should().HaveCount(packs);
        second.Packs.Should().NotContain(pack => pack.Slug.EndsWith("-2", StringComparison.Ordinal));
    }

    [Fact]
    public void Register_throws_a_clear_message_naming_a_missing_folder()
    {
        //Arrange
        string folder = Path.Combine(AppContext.BaseDirectory, "no-such-folder-" + Guid.NewGuid().ToString("N"));

        //Act
        Action act = () => BrixInvadersAssets.Register(TestAssets.Engine, folder);

        //Assert
        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain(folder).And.Contain("assets");
    }

    [Fact]
    public void Register_throws_for_a_folder_without_the_zips_and_leaves_the_provider_untouched()
    {
        //Arrange
        string folder = TestAssets.CreateScratchFolder();

        try
        {
            //Act
            Action act = () => BrixInvadersAssets.Register(TestAssets.Engine, folder);

            //Assert
            act.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Contain(folder).And.Contain(KenneyPacks.SpaceShooterRemasteredFile);
            TestAssets.Register().Warnings.Should().BeEmpty();
        }
        finally
        {
            TestAssets.DeleteScratchFolder(folder);
        }
    }

    [Fact]
    public void Register_rejects_a_null_engine() =>
        ((Action)(() => BrixInvadersAssets.Register(null, TestAssets.Folder))).Should().Throw<ArgumentNullException>();

    [Fact]
    public void Register_rejects_a_null_folder() =>
        ((Action)(() => BrixInvadersAssets.Register(TestAssets.Engine, null))).Should().Throw<ArgumentNullException>();

    [Fact]
    public void LoadPromoCard_loads_the_plain_image_beside_the_zips()
    {
        //Arrange
        TilesheetRegistry.Instance.Remove(BrixInvadersAssets.PromoCardKey, dispose: true);

        //Act
        Tilesheet promo = BrixInvadersAssets.LoadPromoCard(TestAssets.Engine, TestAssets.Folder);
        Tilesheet again = BrixInvadersAssets.LoadPromoCard(TestAssets.Engine, TestAssets.Folder);

        //Assert
        promo.Name.Should().Be(BrixInvadersAssets.PromoCardKey);
        (promo.DefaultRegion.TileSize.Width, promo.DefaultRegion.TileSize.Height).Should().Be((461, 307));
        promo.DefaultRegion.Columns.Should().Be(1);
        promo.DefaultRegion.Rows.Should().Be(1);
        again.Should().BeSameAs(promo);
    }

    [Fact]
    public void LoadPromoCard_throws_when_the_image_is_missing()
    {
        //Arrange
        TilesheetRegistry.Instance.Remove(BrixInvadersAssets.PromoCardKey, dispose: true);
        string folder = TestAssets.CreateScratchFolder();

        try
        {
            //Act
            Action act = () => BrixInvadersAssets.LoadPromoCard(TestAssets.Engine, folder);

            //Assert
            act.Should().Throw<FileNotFoundException>().Which.Message.Should().Contain(KenneyPacks.PromoCardFile);
        }
        finally
        {
            TestAssets.DeleteScratchFolder(folder);
        }
    }
}
