namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, string> VisualTests (
		UserBlendOps.NormalBlendOp normalOp,
		UserBlendOps.MultiplyBlendOp multiplyOp,
		UserBlendOps.ScreenBlendOp screenOp,
		UserBlendOps.DarkenBlendOp darkenOp,
		UserBlendOps.LightenBlendOp lightenOp,
		UserBlendOps.DifferenceBlendOp differenceOp)
	{
		return new () {
			{ normalOp, "visual_blended_normal.png" },
			{ multiplyOp, "visual_blended_multiply.png" },
			{ screenOp, "visual_blended_screen.png" },
			{ darkenOp, "visual_blended_darken.png" },
			{ lightenOp, "visual_blended_lighten.png" },
			{ differenceOp, "visual_blended_difference.png" },
		};
	}
}
