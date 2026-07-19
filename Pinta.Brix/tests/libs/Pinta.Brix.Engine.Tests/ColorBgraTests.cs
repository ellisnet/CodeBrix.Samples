namespace Pinta.Brix.Engine.Tests;

public sealed class ColorBgraTests
{
	[Theory]
	[MemberData (nameof (new_alpha_cases))]
	public void NewAlpha (ColorBgra input, int newAlpha, ColorBgra expected)
	{
		Assert.Equal (expected, input.NewAlpha ((byte) newAlpha));
	}

	public static readonly TheoryData<ColorBgra, int, ColorBgra> new_alpha_cases = new () {
		{ ColorBgra.FromBgra (255, 0, 128, 255), 128, ColorBgra.FromBgra (128, 0, 64, 128) },
		{ ColorBgra.Transparent, 255, ColorBgra.Black },
	};
}
