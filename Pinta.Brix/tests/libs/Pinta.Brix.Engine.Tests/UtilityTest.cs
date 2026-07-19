namespace Pinta.Brix.Engine.Tests;

public sealed class UtilityTest
{
	[Fact]
	public void ClampToByte_Single_TransparentWithinRange ()
	{
		const float MIN = byte.MinValue;
		const float MAX = byte.MaxValue;
		for (float i = MIN; i <= MAX; i++) {
			byte clamped = Utility.ClampToByte (i);
			float convertedBack = clamped;
			Assert.Equal (i, convertedBack);
		}
	}

	[Theory]
	[InlineData (-1f)]
	[InlineData (-0.1f)]
	[InlineData (float.MinValue)]
	public void ClampToByte_Single_LessThanMinBecomesMin (float n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MinValue, clamped);
	}

	[Theory]
	[InlineData (256f)]
	[InlineData (255.1f)]
	[InlineData (float.MaxValue)]
	public void ClampToByte_Single_MoreThanMaxBecomesMax (float n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MaxValue, clamped);
	}

	[Fact]
	public void ClampToByte_Double_TransparentWithinRange ()
	{
		const double MIN = byte.MinValue;
		const double MAX = byte.MaxValue;
		for (double i = MIN; i <= MAX; i++) {
			byte clamped = Utility.ClampToByte (i);
			double convertedBack = clamped;
			Assert.Equal (i, convertedBack);
		}
	}

	[Theory]
	[InlineData (-1d)]
	[InlineData (-0.1d)]
	[InlineData (double.MinValue)]
	public void ClampToByte_Double_LessThanMinBecomesMin (double n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MinValue, clamped);
	}

	[Theory]
	[InlineData (256d)]
	[InlineData (255.1d)]
	[InlineData (double.MaxValue)]
	public void ClampToByte_Double_MoreThanMaxBecomesMax (double n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MaxValue, clamped);
	}

	[Fact]
	public void ClampToByte_Int32_TransparentWithinRange ()
	{
		const int MIN = byte.MinValue;
		const int MAX = byte.MaxValue;
		for (int i = MIN; i <= MAX; i++) {
			byte clamped = Utility.ClampToByte (i);
			double convertedBack = clamped;
			Assert.Equal (i, convertedBack);
		}
	}

	[Theory]
	[InlineData (-1)]
	[InlineData (int.MinValue)]
	public void ClampToByte_Int32_LessThanMinBecomesMin (int n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MinValue, clamped);
	}

	[Theory]
	[InlineData (256)]
	[InlineData (int.MaxValue)]
	public void ClampToByte_Int32_MoreThanMaxBecomesMax (int n)
	{
		byte clamped = Utility.ClampToByte (n);
		Assert.Equal (byte.MaxValue, clamped);
	}
}
