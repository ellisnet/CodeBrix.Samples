using System.Collections.Generic;
using System.Linq;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class AssetReportTests
{
    public AssetReportTests()
    {
        TestAssets.Register();
    }

    [Fact]
    public void DescribeAll_finds_every_key_with_its_kind_and_size()
    {
        //Arrange
        var engine = TestAssets.Engine;

        //Act
        IReadOnlyList<AssetDescription> descriptions = AssetReport.DescribeAll(engine);

        //Assert
        descriptions.Should().HaveCount(AssetKeyCatalog.AllKeys.Count);
        descriptions.Should().OnlyContain(description => description.Found && description.SizeBytes > 0);
        descriptions.Single(description => description.Key == AssetKeys.Atlases.Main).Detail.Should().Be("294 frames");
        descriptions.Single(description => description.Key == AssetKeys.Fonts.Future).Kind.Should().Be(GameAssetKind.Font);
    }

    [Fact]
    public void Describe_reports_an_unknown_key_as_not_found()
    {
        //Arrange
        string[] keys = ["kenney:space-shooter-remastered/no/such/thing"];

        //Act
        AssetDescription description = AssetReport.Describe(TestAssets.Engine, keys).Single();

        //Assert
        description.Found.Should().BeFalse();
        description.Kind.Should().Be(GameAssetKind.Unknown);
    }

    [Fact]
    public void LogAll_writes_one_summary_line_with_no_missing_keys()
    {
        //Arrange
        List<string> lines = [];

        //Act
        AssetReport.LogAll(TestAssets.Engine, TestAssets.Capture(lines));

        //Assert
        lines.Should().ContainSingle();
        lines[0].Should().StartWith("[BrixInvaders] assets: ").And.Contain(" 0 missing").And.Contain("SpriteAtlas 2");
    }
}
