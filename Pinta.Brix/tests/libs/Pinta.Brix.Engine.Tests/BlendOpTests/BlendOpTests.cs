namespace Pinta.Brix.Engine.Tests;

public sealed partial class BlendOpTests
{
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> normal_io_cases;
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> multiply_io_cases;
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> screen_io_cases;
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> darken_io_cases;
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> lighten_io_cases;
	public static readonly TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> difference_io_cases;

	public static readonly TheoryData<UserBlendOp, string> op_name_cases;
	public static readonly TheoryData<UserBlendOp, string> visual_cases;

	static BlendOpTests ()
	{
		UserBlendOps.NormalBlendOp normalOp = new ();
		UserBlendOps.MultiplyBlendOp multiplyOp = new ();
		UserBlendOps.ScreenBlendOp screenOp = new ();
		UserBlendOps.DarkenBlendOp darkenOp = new ();
		UserBlendOps.LightenBlendOp lightenOp = new ();
		UserBlendOps.DifferenceBlendOp differenceOp = new ();

		normal_io_cases = CreateNormalIOCases (normalOp);
		multiply_io_cases = CreateMultiplyIOCases (multiplyOp);
		screen_io_cases = CreateScreenIOCases (screenOp);
		darken_io_cases = CreateDarkenIOCases (darkenOp);
		lighten_io_cases = CreateLightenIOCases (lightenOp);
		difference_io_cases = CreateDifferenceIOCases (differenceOp);

		op_name_cases = NamingTests (
			normalOp,
			multiplyOp,
			screenOp,
			darkenOp,
			lightenOp,
			differenceOp);

		visual_cases = VisualTests (
			normalOp,
			multiplyOp,
			screenOp,
			darkenOp,
			lightenOp,
			differenceOp);
	}

	[Theory]
	[MemberData (nameof (op_name_cases))]
	public void StaticName_Is_Expected (UserBlendOp blendOp, string expectedName)
	{
		Assert.Equal (expectedName, blendOp.ToString ());
	}

	[Theory]
	[MemberData (nameof (normal_io_cases))]
	[MemberData (nameof (multiply_io_cases))]
	[MemberData (nameof (screen_io_cases))]
	[MemberData (nameof (darken_io_cases))]
	[MemberData (nameof (lighten_io_cases))]
	[MemberData (nameof (difference_io_cases))]
	public void Output_Is_Expected (UserBlendOp blendOp, ColorBgra bottom, ColorBgra top, ColorBgra expected)
	{
		ColorBgra result = blendOp.Apply (bottom, top);
		Assert.True (expected == result, $"Colors not blended as expected by {blendOp}: got {result}, expected {expected}");
	}

	[Theory]
	[MemberData (nameof (visual_cases))]
	public void Visual_Blending (UserBlendOp blendOp, string nameOutput)
	{
		Utilities.TestBlendOp (blendOp, nameOutput);
	}
}
