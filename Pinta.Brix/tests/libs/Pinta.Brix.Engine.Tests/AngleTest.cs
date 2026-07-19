using System;

namespace Pinta.Brix.Engine.Tests;

public sealed class AngleTest
{
	[Theory]
	[InlineData (1d, 1d)]
	[InlineData (Math.PI, Math.PI)]
	[InlineData (Math.PI * 2d, 0d)]
	[InlineData (-Math.PI, Math.PI)]
	[InlineData (-(Math.PI * 2d), 0d)]
	[InlineData (0d, 0d)]
	public void RadiansAngle_Creation (double constructorArgument, double expectedPropertyValue)
	{
		RadiansAngle angle = new (constructorArgument);
		Assert.Equal (expectedPropertyValue, angle.Radians);
	}

	[Theory]
	[InlineData (1d, 1d)]
	[InlineData (180d, 180d)]
	[InlineData (360d, 0)]
	[InlineData (-180d, 180d)]
	[InlineData (-360d, 0d)]
	[InlineData (0d, 0d)]
	public void DegreesAngle_Creation (double constructorArgument, double expectedPropertyValue)
	{
		DegreesAngle angle = new (constructorArgument);
		Assert.Equal (expectedPropertyValue, angle.Degrees);
	}

	[Theory]
	[InlineData (0.5d, 0.5d)]
	[InlineData (1d, 0d)]
	[InlineData (-0.5d, 0.5d)]
	[InlineData (-1d, 0d)]
	[InlineData (0d, 0d)]
	public void RevolutionsAngle_Creation (double constructorArgument, double expectedPropertyValue)
	{
		RevolutionsAngle angle = new (constructorArgument);
		Assert.Equal (expectedPropertyValue, angle.Revolutions);
	}

	[Theory]
	[InlineData (double.NaN)]
	[InlineData (double.PositiveInfinity)]
	[InlineData (double.NegativeInfinity)]
	public void Constructor_Throws_If_Not_Finite (double constructorArgument)
	{
		Assert.Throws<ArgumentOutOfRangeException> (() => new DegreesAngle (constructorArgument));
		Assert.Throws<ArgumentOutOfRangeException> (() => new RadiansAngle (constructorArgument));
		Assert.Throws<ArgumentOutOfRangeException> (() => new RevolutionsAngle (constructorArgument));
	}

	[Theory]
	[InlineData (1d, 1d, 2d)]
	[InlineData (Math.PI, Math.PI, 0)]
	[InlineData (Math.PI * 1.5d, Math.PI * 1.5d, Math.PI)]
	[InlineData (0d, 0d, 0d)]
	[InlineData (Math.PI * 2d, Math.PI * 2d, 0d)]
	[InlineData (-Math.PI, Math.PI * 2d, Math.PI)]
	public void RadiansAngle_Addition (double leftArgument, double rightArgument, double expectedResult)
	{
		RadiansAngle left = new (leftArgument);
		RadiansAngle right = new (rightArgument);
		var result = left + right;
		Assert.Equal (expectedResult, result.Radians);
	}

	[Theory]
	[InlineData (1d, 1d, 2d)]
	[InlineData (180d, 180d, 0d)]
	[InlineData (270d, 270d, 180d)]
	[InlineData (0d, 0d, 0d)]
	[InlineData (360d, 360d, 0d)]
	[InlineData (-180d, 360d, 180d)]
	public void DegreesAngle_Addition (double leftArgument, double rightArgument, double expectedResult)
	{
		DegreesAngle left = new (leftArgument);
		DegreesAngle right = new (rightArgument);
		var result = left + right;
		Assert.Equal (expectedResult, result.Degrees);
	}

	[Theory]
	[InlineData (0.125d, 0.125d, 0.25d)]
	[InlineData (0.5d, 0.5d, 0d)]
	[InlineData (0.75d, 0.75d, 0.5d)]
	[InlineData (0d, 0d, 0d)]
	[InlineData (1d, 1d, 0d)]
	[InlineData (-0.5d, 1d, 0.5d)]
	public void RevolutionsAngle_Addition (double leftArgument, double rightArgument, double expectedResult)
	{
		RevolutionsAngle left = new (leftArgument);
		RevolutionsAngle right = new (rightArgument);
		var result = left + right;
		Assert.Equal (expectedResult, result.Revolutions);
	}

	[Theory]
	[InlineData (Math.PI * 0.5, Math.PI, Math.PI * 1.5)]
	[InlineData (0d, Math.PI, Math.PI)]
	[InlineData (0d, Math.PI * 2, 0d)]
	[InlineData (Math.PI, Math.PI * 0.5, Math.PI * 0.5)]
	public void RadiansAngle_Subtraction (double leftArgument, double rightArgument, double expectedResult)
	{
		RadiansAngle left = new (leftArgument);
		RadiansAngle right = new (rightArgument);
		var result = left - right;
		Assert.Equal (expectedResult, result.Radians);
	}

	[Theory]
	[InlineData (90d, 180d, 270d)]
	[InlineData (0d, 180d, 180d)]
	[InlineData (0d, 360d, 0d)]
	[InlineData (180d, 90d, 90d)]
	public void DegreesAngle_Subtraction (double leftArgument, double rightArgument, double expectedResult)
	{
		DegreesAngle left = new (leftArgument);
		DegreesAngle right = new (rightArgument);
		var result = left - right;
		Assert.Equal (expectedResult, result.Degrees);
	}

	[Theory]
	[InlineData (0.25d, 0.5d, 0.75d)]
	[InlineData (0d, 0.5d, 0.5d)]
	[InlineData (0d, 1d, 0d)]
	[InlineData (0.5d, 0.25d, 0.25d)]
	public void RevolutionsAngle_Subtraction (double leftArgument, double rightArgument, double expectedResult)
	{
		RevolutionsAngle left = new (leftArgument);
		RevolutionsAngle right = new (rightArgument);
		var result = left - right;
		Assert.Equal (expectedResult, result.Revolutions);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (Math.PI * 0.5d, 90d)]
	[InlineData (Math.PI, 180d)]
	[InlineData (Math.PI * 1.5d, 270d)]
	[InlineData (-Math.PI, 180d)]
	public void Radians_To_Degrees (double radians, double expectedDegrees)
	{
		RadiansAngle radiansAngle = new (radians);
		DegreesAngle degreesAngle = radiansAngle.ToDegrees ();
		Assert.Equal (expectedDegrees, degreesAngle.Degrees);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (Math.PI * 0.5d, 0.25d)]
	[InlineData (Math.PI, 0.5d)]
	[InlineData (Math.PI * 1.5d, 0.75d)]
	[InlineData (-Math.PI, 0.5d)]
	public void Radians_To_Revolutions (double radians, double expectedRevolutions)
	{
		RadiansAngle radiansAngle = new (radians);
		RevolutionsAngle revolutionsAngle = radiansAngle.ToRevolutions ();
		Assert.Equal (expectedRevolutions, revolutionsAngle.Revolutions);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (90d, Math.PI * 0.5d)]
	[InlineData (180d, Math.PI)]
	[InlineData (270d, Math.PI * 1.5d)]
	[InlineData (-180d, Math.PI)]
	public void Degrees_To_Radians (double degrees, double expectedRadians)
	{
		DegreesAngle degreesAngle = new (degrees);
		RadiansAngle radiansAngle = degreesAngle.ToRadians ();
		Assert.Equal (expectedRadians, radiansAngle.Radians);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (90d, 0.25d)]
	[InlineData (180d, 0.5d)]
	[InlineData (270d, 0.75d)]
	[InlineData (-180d, 0.5)]
	public void Degrees_To_Revolutions (double degrees, double expectedRevolutions)
	{
		DegreesAngle degreesAngle = new (degrees);
		RevolutionsAngle revolutionsAngle = degreesAngle.ToRevolutions ();
		Assert.Equal (expectedRevolutions, revolutionsAngle.Revolutions);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (0.25d, Math.PI * 0.5d)]
	[InlineData (0.5d, Math.PI)]
	[InlineData (0.75d, Math.PI * 1.5d)]
	[InlineData (-0.5d, Math.PI)]
	public void Revolutions_To_Radians (double revolutions, double expectedRadians)
	{
		RevolutionsAngle revolutionsAngle = new (revolutions);
		RadiansAngle radiansAngle = revolutionsAngle.ToRadians ();
		Assert.Equal (expectedRadians, radiansAngle.Radians);
	}

	[Theory]
	[InlineData (0d, 0d)]
	[InlineData (0.25d, 90d)]
	[InlineData (0.5d, 180d)]
	[InlineData (0.75d, 270d)]
	[InlineData (-0.5, 180d)]
	public void Revolutions_To_Degrees (double revolutions, double expectedDegrees)
	{
		RevolutionsAngle revolutionsAngle = new (revolutions);
		DegreesAngle degreesAngle = revolutionsAngle.ToDegrees ();
		Assert.Equal (expectedDegrees, degreesAngle.Degrees);
	}
}
