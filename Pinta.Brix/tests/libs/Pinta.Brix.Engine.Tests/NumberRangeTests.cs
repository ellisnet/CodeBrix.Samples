using System;
using System.Collections.Immutable;

namespace Pinta.Brix.Engine.Tests;

public sealed class NumberRangeTests
{
	[Theory]
	[MemberData (nameof (ValidInvalidPairs))]
	public void Creation_Throws_On_Invalid (double valid, double invalid)
	{
		Assert.Throws<ArgumentOutOfRangeException> (() => NumberRange.Create (valid, invalid));
		Assert.Throws<ArgumentOutOfRangeException> (() => new NumberRange<double> (valid, invalid));
		Assert.Throws<ArgumentOutOfRangeException> (() => NumberRange.Create (invalid, valid));
		Assert.Throws<ArgumentOutOfRangeException> (() => new NumberRange<double> (invalid, valid));
		Assert.Throws<ArgumentOutOfRangeException> (() => NumberRange.Create (invalid, invalid));
		Assert.Throws<ArgumentOutOfRangeException> (() => new NumberRange<double> (invalid, invalid));
	}

	[Theory]
	[MemberData (nameof (InconsistentBoundsCases))]
	public void Creation_Throws_On_Inconsistent_Bounds (double lower, double upper)
	{
		Assert.Throws<ArgumentException> (() => NumberRange.Create (lower, upper));
		Assert.Throws<ArgumentException> (() => new NumberRange<double> (lower, upper));
	}

	[Theory]
	[MemberData (nameof (ConsistentBoundsCases))]
	public void Creation_Accepts_Consistent_Bounds (double lower, double upper)
	{
		Assert.Null (Record.Exception (() => NumberRange.Create (lower, upper)));
		Assert.Null (Record.Exception (() => new NumberRange<double> (lower, upper)));
	}

	[Theory]
	[InlineData (5.5, 10.5)]
	public void Properties_Are_Set_Correctly (double lower, double upper)
	{
		NumberRange<double> range = new (lower, upper);
		Assert.Equal (lower, range.Lower);
		Assert.Equal (upper, range.Upper);
	}

	[Theory]
	[InlineData (10, 20)]
	public void Equality (double lower, double upper)
	{
		NumberRange<double> a = new (lower, upper);
		NumberRange<double> b = new (lower, upper);
		Assert.True (a.Equals (b));
		Assert.True (b.Equals (a));
		Assert.True (a.Equals ((object) b));
		Assert.True (b.Equals ((object) a));
		Assert.False (a.Equals (null));
		Assert.False (b.Equals (null));
		Assert.True (a == b);
		Assert.True (b == a);
		Assert.False (a != b);
		Assert.False (b != a);
		Assert.Equal (b.GetHashCode (), a.GetHashCode ());
	}

	[Theory]
	[InlineData (10, 20, 30, 40)]
	public void Inequality (double lowerA, double upperA, double lowerB, double upperB)
	{
		NumberRange<double> a = new (lowerA, upperA);
		NumberRange<double> b = new (lowerB, upperB);
		Assert.False (a.Equals (b));
		Assert.False (b.Equals (a));
		Assert.False (a.Equals ((object) b));
		Assert.False (b.Equals ((object) a));
		Assert.False (a.Equals (null));
		Assert.False (b.Equals (null));
		Assert.False (a == b);
		Assert.False (b == a);
		Assert.True (a != b);
		Assert.True (b != a);
	}

	private static readonly ImmutableArray<double> valid_doubles = [

		double.MinValue,

		-double.Tau,
		-double.Pi,
		-double.E,
		-2,
		-1,
		-double.Epsilon,
		double.NegativeZero,
		0,
		double.Epsilon,
		1,
		2,
		double.E,
		double.Pi,
		double.Tau,

		double.MaxValue
	];

	private static readonly ImmutableArray<double> invalid_doubles =
		[double.NegativeInfinity, double.PositiveInfinity, double.NaN];

	//was previously: NUnit [ValueSource] combinatorial arguments
	public static TheoryData<double, double> ValidInvalidPairs {
		get {
			TheoryData<double, double> pairs = new ();
			foreach (double valid in valid_doubles)
				foreach (double invalid in invalid_doubles)
					pairs.Add (valid, invalid);
			return pairs;
		}
	}

	public static TheoryData<double, double> ConsistentBoundsCases {
		get {
			TheoryData<double, double> cases = new ();
			foreach (double lower in valid_doubles)
				foreach (double upper in valid_doubles)
					if (lower <= upper)
						cases.Add (lower, upper);
			return cases;
		}
	}

	public static TheoryData<double, double> InconsistentBoundsCases {
		get {
			TheoryData<double, double> cases = new ();
			foreach (double lower in valid_doubles)
				foreach (double upper in valid_doubles)
					if (lower > upper)
						cases.Add (lower, upper);
			return cases;
		}
	}
}
