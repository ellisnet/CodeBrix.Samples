using System;

namespace Pinta.Brix.Engine.Tests;

public sealed class ScanlineTest
{
	[Theory]
	[InlineData (0, 0, -1)]
	public void Constructor_RejectsInvalidArguments (int x, int y, int length)
	{
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = new Scanline (x, y, length));
	}

	[Theory]
	[MemberData (nameof (sample_initializations))]
	public void MembersInitializingCorrectly (int x, int y, int length)
	{
		Scanline scanline = new (
			x: x,
			y: y,
			length: length);
		Assert.Equal (scanline.X, x);
		Assert.Equal (scanline.Y, y);
		Assert.Equal (scanline.Length, length);
	}

	[Theory]
	[MemberData (nameof (sample_initializations))]
	public void EqualsOperator_TrueWithEqual (int x, int y, int length)
	{
		Scanline scanline1 = new (x, y, length);
		Scanline scanline2 = new (x, y, length);
		Assert.True (scanline1 == scanline2);
		Assert.True (scanline2 == scanline1);
	}

	[Theory]
	[MemberData (nameof (sample_initializations))]
	public void HashCodes_Are_Same_For_Equal_Values (int x, int y, int length)
	{
		Scanline scanline1 = new (x, y, length);
		Scanline scanline2 = new (x, y, length);
		Assert.Equal (scanline2.GetHashCode (), scanline1.GetHashCode ());
		Assert.Equal (scanline1.GetHashCode (), scanline2.GetHashCode ());
	}

	[Theory]
	[MemberData (nameof (unequal_values))]
	public void EqualsOperator_FalseWithUnequal (int x1, int y1, int length1, int x2, int y2, int length2)
	{
		Scanline scanline1 = new (x1, y1, length1);
		Scanline scanline2 = new (x2, y2, length2);
		Assert.False (scanline1 == scanline2);
		Assert.False (scanline2 == scanline1);
	}

	[Theory]
	[MemberData (nameof (sample_initializations))]
	public void NotEqualsOperator_FalseWithEqual (int x, int y, int length)
	{
		Scanline scanline1 = new (x, y, length);
		Scanline scanline2 = new (x, y, length);
		Assert.False (scanline1 != scanline2);
		Assert.False (scanline2 != scanline1);
	}

	[Theory]
	[MemberData (nameof (unequal_values))]
	public void NotEqualsOperator_TrueWithUnequal (int x1, int y1, int length1, int x2, int y2, int length2)
	{
		Scanline scanline1 = new (x1, y1, length1);
		Scanline scanline2 = new (x2, y2, length2);
		Assert.True (scanline1 != scanline2);
		Assert.True (scanline2 != scanline1);
	}

	[Theory]
	[MemberData (nameof (sample_initializations))]
	public void EqualsMethod_TrueWithEqual (int x, int y, int length)
	{
		Scanline scanline1 = new (x, y, length);
		Scanline scanline2 = new (x, y, length);
		Assert.True (scanline1.Equals (scanline2));
		Assert.True (scanline2.Equals (scanline1));
	}

	[Theory]
	[MemberData (nameof (unrelated_objects))]
	public void EqualsMethod_FalseWithUnequalTypes (object? other)
	{
		Scanline scanline = new (1, 1, 1);
		bool comparison = scanline.Equals (other);
		Assert.False (comparison);
	}

	[Theory]
	[MemberData (nameof (unequal_values))]
	public void EqualsMethod_FalseWithUnequalValues (int x1, int y1, int length1, int x2, int y2, int length2)
	{
		Scanline scanline1 = new (x1, y1, length1);
		Scanline scanline2 = new (x2, y2, length2);
		Assert.False (scanline1.Equals (scanline2));
		Assert.False (scanline2.Equals (scanline1));
	}

	public static readonly TheoryData<object?> unrelated_objects = new () {
		"This is not a scanline",
		(object?) null,
		new[] { 1, 1, 1 },
		111,
	};

	public static readonly TheoryData<int, int, int> sample_initializations = new () {
		{ 1, 2, 3 },
		{ 3, 1, 2 },
		{ 2, 3, 1 },
	};

	public static readonly TheoryData<int, int, int, int, int, int> unequal_values = new () {
		{
			1, 1, 1,
			2, 2, 2 },
		{
			1, 1, 1,
			1, 1, 2 },
		{
			1, 1, 1,
			1, 2, 1 },
		{
			1, 1, 1,
			2, 1, 1 },
		{
			2, 2, 2,
			1, 1, 1 },
		{
			1, 1, 2,
			1, 1, 1 },
		{
			1, 2, 1,
			1, 1, 1 },
		{
			2, 1, 1,
			1, 1, 1 },
	};
}
