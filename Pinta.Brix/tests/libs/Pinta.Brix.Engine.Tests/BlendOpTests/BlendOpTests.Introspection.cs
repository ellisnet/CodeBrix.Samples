namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, string> NamingTests (
		UserBlendOps.NormalBlendOp normalOp,
		UserBlendOps.MultiplyBlendOp multiplyOp,
		UserBlendOps.ScreenBlendOp screenOp,
		UserBlendOps.DarkenBlendOp darkenOp,
		UserBlendOps.LightenBlendOp lightenOp,
		UserBlendOps.DifferenceBlendOp differenceOp)
	{
		return new () {
			{ normalOp, "Normal" },
			{ multiplyOp, "Multiply" },
			{ screenOp, "Screen" },
			{ darkenOp, "Darken" },
			{ lightenOp, "Lighten" },
			{ differenceOp, "Difference" },
		};
	}
}
