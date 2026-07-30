using KenneyAssetBrowser.AssetRead.Parsing;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.AssetRead.Tests;

public class KenneyNamesTests
{
    [Fact]
    public void Bundle_file_name_is_prettified()
    {
        //Act
        var name = KenneyNames.PrettifyBundleFileName("kenney_brick-kit.zip");

        //Assert
        name.Should().Be("Brick Kit");
    }

    [Fact]
    public void Bundle_full_path_and_missing_kenney_prefix_are_handled()
    {
        //Act
        var name = KenneyNames.PrettifyBundleFileName("/home/user/assets/space_shooter-extension.zip");

        //Assert
        name.Should().Be("Space Shooter Extension");
    }

    [Fact]
    public void License_title_line_with_version_is_parsed()
    {
        //Arrange
        var licenseText = "\t\r\n\n\tBrick Kit (1.0)\r\n\n\tCreated/distributed by Kenney (www.kenney.nl)\r\n";

        //Act
        var found = KenneyNames.TryParseLicenseTitle(licenseText, out var title, out var version);

        //Assert
        found.Should().Be(true);
        title.Should().Be("Brick Kit");
        version.Should().Be("1.0");
    }

    [Fact]
    public void License_title_line_without_version_keeps_whole_line()
    {
        //Act
        var found = KenneyNames.TryParseLicenseTitle("Generic Items\r\nLicense: CC0", out var title, out var version);

        //Assert
        found.Should().Be(true);
        title.Should().Be("Generic Items");
        version.Should().Be(null);
    }

    [Fact]
    public void License_starting_with_a_url_is_not_a_title()
    {
        //Act
        var found = KenneyNames.TryParseLicenseTitle(
            "http://creativecommons.org/publicdomain/zero/1.0/\r\nmore text", out var title, out _);

        //Assert
        found.Should().Be(false);
        title.Should().Be(null);
    }

    [Fact]
    public void Empty_license_is_not_a_title()
    {
        //Act
        var found = KenneyNames.TryParseLicenseTitle("   \r\n\t\r\n", out var title, out _);

        //Assert
        found.Should().Be(false);
        title.Should().Be(null);
    }
}
