using Pinta.Brix.Effects;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects.Tests;

partial class EffectsTest
{
	[Fact]
	public void Fragment1 ()
	{
		FragmentEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "fragment1.png");
	}

	[Fact]
	public void Fragment2 ()
	{
		FragmentEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Fragments = 25;
		effect.Data.Distance = 60;
		effect.Data.Rotation = new (90);
		Utilities.TestEffect (effect, "fragment2.png");
	}

	[Fact]
	public void GaussianBlur1 ()
	{
		GaussianBlurEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "gaussianblur1.png");
	}

	[Fact]
	public void GaussianBlur2 ()
	{
		GaussianBlurEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Radius = 100;
		Utilities.TestEffect (effect, "gaussianblur2.png");
	}

	[Fact]
	public void MotionBlur1 ()
	{
		MotionBlurEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "motionblur1.png");
	}

	[Fact]
	public void MotionBlur2 ()
	{
		MotionBlurEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Angle = new (50);
		effect.Data.Distance = 25;
		effect.Data.Centered = false;
		Utilities.TestEffect (effect, "motionblur2.png");
	}

	[Fact]
	public void RadialBlur1 ()
	{
		RadialBlurEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "radialblur1.png");
	}

	[Fact]
	public void RadialBlur2 ()
	{
		RadialBlurEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Angle = new (90);
		effect.Data.Offset = new (20, 20);
		effect.Data.Quality = 4;
		Utilities.TestEffect (effect, "radialblur2.png");
	}

	[Fact]
	public void Unfocus1 ()
	{
		UnfocusEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "unfocus1.png");
	}

	[Fact]
	public void Unfocus2 ()
	{
		UnfocusEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Radius = 50;
		Utilities.TestEffect (effect, "unfocus2.png");
	}

	[Fact]
	public void ZoomBlur1 ()
	{
		ZoomBlurEffect effect = new (Utilities.CreateMockServices ());
		Utilities.TestEffect (effect, "zoomblur1.png");
	}

	[Fact]
	public void ZoomBlur2 ()
	{
		ZoomBlurEffect effect = new (Utilities.CreateMockServices ());
		effect.Data.Amount = 50;
		effect.Data.Offset = new (-1, -1);
		Utilities.TestEffect (effect, "zoomblur2.png");
	}
}
