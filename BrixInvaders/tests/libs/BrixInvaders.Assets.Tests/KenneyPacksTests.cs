using System;
using System.IO;
using System.Linq;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class KenneyPacksTests
{
    [Fact]
    public void FileNames_all_exist_beside_the_test_executable()
    {
        //Arrange
        string folder = TestAssets.Folder;

        //Act
        string[] missing = KenneyPacks.FileNames
            .Append(KenneyPacks.PromoCardFile)
            .Where(fileName => !File.Exists(Path.Combine(folder, fileName)))
            .ToArray();

        //Assert
        missing.Should().BeEmpty();
        KenneyPacks.FileNames.Should().HaveCount(5);
    }

    [Fact]
    public void Slugs_equal_the_slugs_the_provider_reads_from_the_zips()
    {
        //Arrange
        KenneyGameAssetProvider provider = TestAssets.Register();

        //Act
        string[] actual = KenneyPacks.FileNames
            .Select(fileName => provider.Packs.Single(pack => Path.GetFileName(pack.SourcePath) == fileName).Slug)
            .ToArray();

        //Assert
        actual.Should().Equal(KenneyPacks.Slugs);
        KenneyPacks.SpaceShooterRemasteredSlug.Should().Be("space-shooter-remastered");
    }

    [Fact]
    public void ZipPaths_are_the_five_files_in_order_under_the_folder()
    {
        //Arrange
        string folder = TestAssets.Folder;

        //Act
        var paths = KenneyPacks.ZipPaths(folder);

        //Assert
        paths.Select(Path.GetFileName).Should().Equal(KenneyPacks.FileNames);
        paths.Should().OnlyContain(path => Path.GetDirectoryName(path) == Path.GetFullPath(folder));
    }

    [Fact]
    public void DefaultFolder_is_assets_kenney_beside_the_executable() =>
        KenneyPacks.DefaultFolder.Should().Be(Path.Combine(AppContext.BaseDirectory, "assets", "kenney"));

    [Fact]
    public void PromoCardPath_names_the_promo_image_in_the_folder() =>
        Path.GetFileName(KenneyPacks.PromoCardPath(TestAssets.Folder)).Should().Be("Kenney_asset_bundle.png");
}
