using CodeBrix.Platform.GameEngine.Rendering.Text;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class BrixInvadersFontsTests
{
    [Fact]
    public void LoadFonts_registers_both_kenvector_fonts_with_their_family_names()
    {
        //Arrange
        TestAssets.Register();

        //Act
        BrixInvadersFonts fonts = BrixInvadersAssets.LoadFonts(TestAssets.Engine);

        //Assert
        fonts.TextKey.Should().Be(AssetKeys.Fonts.Future);
        fonts.ThinKey.Should().Be(AssetKeys.Fonts.FutureThin);
        FontManager.Instance.Contains(fonts.TextKey).Should().BeTrue();
        FontManager.Instance.Contains(fonts.ThinKey).Should().BeTrue();
        fonts.TextFamilyName.Should().NotBeNullOrWhiteSpace();
        fonts.ThinFamilyName.Should().NotBeNullOrWhiteSpace();
        FontManager.Instance.Get(fonts.TextKey).GlyphCount.Should().BeGreaterThan(26);
    }
}
