using System;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Assets.Tests;

public class FontAssetsTests
{
    [Theory]
    [InlineData(FontAssets.MerriweatherRegular)]
    [InlineData(FontAssets.MerriweatherBold)]
    [InlineData(FontAssets.MerriweatherItalic)]
    [InlineData(FontAssets.NotoSansCuneiform)]
    public void Every_font_is_an_embedded_truetype_file(string resourceName)
    {
        var bytes = FontAssets.ReadBytes(resourceName);
        bytes.Length.Should().BeGreaterThan(100_000);
        //The TrueType scaler type: 0x00010000
        (bytes[0], bytes[1], bytes[2], bytes[3]).Should().Be(((byte)0, (byte)1, (byte)0, (byte)0));
    }

    [Theory]
    [InlineData("Merriweather", "Merriweather")]
    [InlineData("NotoSansCuneiform", "Noto Sans Cuneiform")]
    public void Every_font_carries_its_open_font_license(string family, string expectedName)
    {
        var license = FontAssets.ReadLicense(family);
        license.Should().Contain(expectedName);
        license.Should().Contain("SIL Open Font License, Version 1.1");
        license.Should().Contain("PERMISSION & CONDITIONS");
    }

    [Fact]
    public void Missing_resources_throw()
    {
        Action open = () => FontAssets.Open("GoddessTempleDiscovery.Assets.Fonts.Nope.ttf");
        open.Should().Throw<InvalidOperationException>();
        Action license = () => FontAssets.ReadLicense("Nope");
        license.Should().Throw<InvalidOperationException>();
    }
}
