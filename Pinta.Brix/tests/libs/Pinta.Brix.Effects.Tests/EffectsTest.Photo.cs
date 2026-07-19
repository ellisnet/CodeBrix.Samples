using Pinta.Brix.Engine;
using Pinta.Brix.Effects;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects.Tests;

partial class EffectsTest
{
	[Fact]
	public void Glow1 ()
	{
		GlowEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "glow1.png");
	}

	[Fact]
	public void Glow2 ()
	{
		GlowEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Radius = 12;
		effect.Data.Brightness = 50;
		effect.Data.Contrast = 50;
		Utilities.TestEffect (effect, "glow2.png");
	}

	[Fact]
	public void RedEyeRemove1 ()
	{
		RedEyeRemoveEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "redeyeremove1.png");
	}

	[Fact]
	public void RedEyeRemove2 ()
	{
		RedEyeRemoveEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Tolerance = 20;
		effect.Data.Saturation = 20;
		Utilities.TestEffect (effect, "redeyeremove2.png");
	}

	[Fact]
	public void Sharpen1 ()
	{
		SharpenEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "sharpen1.png");
	}

	[Fact]
	public void Sharpen2 ()
	{
		SharpenEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Amount = 16;
		Utilities.TestEffect (effect, "sharpen2.png");
	}

	[Fact]
	public void SoftenPortrait1 ()
	{
		SoftenPortraitEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "softenportrait1.png");
	}

	[Fact]
	public void SoftenPortrait2 ()
	{
		SoftenPortraitEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Softness = 8;
		effect.Data.Lighting = -10;
		effect.Data.Warmth = 15;
		Utilities.TestEffect (effect, "softenportrait2.png");
	}

	[Fact]
	public void Vignette1 ()
	{
		VignetteEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Offset = PointI.Zero;
		Utilities.TestEffect (effect, "vignette1.png");
	}

	[Fact]
	public void Vignette2 ()
	{
		VignetteEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Offset = new PointI (125, 125);
		Utilities.TestEffect (effect, "vignette2.png");
	}

	[Fact]
	public void Vignette3 ()
	{
		VignetteEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Offset = new PointI (125, 125);
		effect.Data.RadiusPercentage = 33;
		Utilities.TestEffect (effect, "vignette3.png");
	}
}
