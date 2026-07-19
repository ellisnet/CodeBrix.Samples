using Pinta.Brix.Effects;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects.Tests;

partial class EffectsTest
{
	[Fact]
	public void Dithering1 ()
	{
		DitheringEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.PaletteChoice = PredefinedPalettes.OldWindows16;
		effect.Data.ErrorDiffusionMethod = PredefinedDiffusionMatrices.FloydSteinberg;
		Utilities.TestEffect (effect, "dithering1.png");
	}

	[Fact]
	public void Dithering2 ()
	{
		DitheringEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.PaletteChoice = PredefinedPalettes.BlackWhite;
		effect.Data.ErrorDiffusionMethod = PredefinedDiffusionMatrices.FloydSteinberg;
		Utilities.TestEffect (effect, "dithering2.png");
	}

	[Fact]
	public void Dithering3 ()
	{
		DitheringEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.PaletteChoice = PredefinedPalettes.OldWindows16;
		effect.Data.ErrorDiffusionMethod = PredefinedDiffusionMatrices.Stucki;
		Utilities.TestEffect (effect, "dithering3.png");
	}

	[Fact]
	public void Dithering4 ()
	{
		DitheringEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.PaletteChoice = PredefinedPalettes.OldMsPaint;
		effect.Data.ErrorDiffusionMethod = PredefinedDiffusionMatrices.Atkinson;
		Utilities.TestEffect (effect, "dithering4.png");
	}
}
