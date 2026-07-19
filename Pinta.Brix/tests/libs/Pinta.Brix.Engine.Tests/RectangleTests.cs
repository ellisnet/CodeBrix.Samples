using System.Collections.Generic;
using System.Linq;

namespace Pinta.Brix.Engine.Tests;

public sealed class RectangleTests
{
	[Theory]
	[InlineData (0, 0, 1, 1)]
	[InlineData (1, 2, 3, 4)]
	public void ConsistentConstructor (int x, int y, int width, int height)
	{
		RectangleI constructed = new (x, y, width, height);
		Assert.Equal (x, constructed.X);
		Assert.Equal (y, constructed.Y);
		Assert.Equal (width, constructed.Width);
		Assert.Equal (height, constructed.Height);
	}

	[Theory]
	[InlineData (0, 0, 0, 0)]
	[InlineData (0, 0, 1, 1)]
	[InlineData (1, 1, 1, 1)]
	[InlineData (1, 1, 2, 2)]
	public void ConsistentLTRBFactory (int l, int t, int r, int b)
	{
		RectangleI built = RectangleI.FromLTRB (l, t, r, b);
		Assert.Equal (l, built.Left);
		Assert.Equal (t, built.Top);
		Assert.Equal (r, built.Right);
		Assert.Equal (b, built.Bottom);

		Assert.Equal (built.ToDouble (), RectangleD.FromLTRB (l, t, r, b));
	}

	[Theory]
	[MemberData (nameof (from_points_cases))]
	public void CorrectFromPoints (PointI a, PointI b, RectangleI expected, RectangleI expected_no_invert)
	{
		Assert.Equal (expected, RectangleI.FromPoints (a, b));
		Assert.Equal (
			expected.ToDouble (),
			RectangleD.FromPoints (a.ToDouble (), b.ToDouble (), invertIfNegative: true));
		Assert.Equal (
			expected_no_invert.ToDouble (),
			RectangleD.FromPoints (a.ToDouble (), b.ToDouble (), invertIfNegative: false));
	}

	[Theory]
	[MemberData (nameof (not_equal_cases))]
	public void CorrectNotEqual (RectangleI a, RectangleI b)
		=> Assert.NotEqual (b, a);

	[Theory]
	[MemberData (nameof (union_cases))]
	public void CorrectUnion (RectangleI a, RectangleI b, RectangleI expected)
	{
		Assert.Equal (expected, a.Union (b));
		Assert.Equal (expected.ToDouble (), a.ToDouble ().Union (b.ToDouble ()));
	}

	[Theory]
	[MemberData (nameof (intersect_cases))]
	public void CorrectIntersection (RectangleI a, RectangleI b, RectangleI expected)
		=> Assert.Equal (expected, a.Intersect (b));

	[Theory]
	[MemberData (nameof (inflation_cases))]
	public void CorrectInflation (RectangleI a, int widthInflation, int heightInflation, RectangleI expected)
		=> Assert.Equal (expected, a.Inflated (widthInflation, heightInflation));

	[Theory]
	[MemberData (nameof (vertical_slicing_cases))]
	public void CorrectVerticalSlicing (RectangleI original, IReadOnlyList<RectangleI> expectedSlices)
	{
		var actualSlices = original.ToRows ().ToArray ();
		Assert.Equal (expectedSlices.Count, actualSlices.Length);
		for (int i = 0; i < expectedSlices.Count; i++)
			Assert.Equal (expectedSlices[i], actualSlices[i]);
	}

	public static readonly TheoryData<RectangleI, IReadOnlyList<RectangleI>> vertical_slicing_cases = new () {
		{
			new RectangleI (1, 1, 5, 5),
			new[] {
				new RectangleI (1, 1, 5, 1),
				new RectangleI (1, 2, 5, 1),
				new RectangleI (1, 3, 5, 1),
				new RectangleI (1, 4, 5, 1),
				new RectangleI (1, 5, 5, 1),
			}
		},
	};

	public static readonly TheoryData<RectangleI, int, int, RectangleI> inflation_cases = new () {
		{
			new RectangleI (1, 1, 1, 1),
			1,
			1,
			new RectangleI (0, 0, 3, 3)
		},
		{
			new RectangleI (2, 1, 2, 1),
			2,
			1,
			new RectangleI (0, 0, 6, 3)
		},
	};

	public static readonly TheoryData<RectangleI, RectangleI, RectangleI> union_cases = new () {
		{
			RectangleI.FromLTRB (0, 0, 2, 2),
			RectangleI.FromLTRB (1, 1, 3, 3),
			RectangleI.FromLTRB (0, 0, 3, 3)
		},
		{
			RectangleI.FromLTRB (0, 0, 1, 1),
			RectangleI.FromLTRB (2, 2, 3, 3),
			RectangleI.FromLTRB (0, 0, 3, 3)
		},
	};

	public static readonly TheoryData<RectangleI, RectangleI, RectangleI> intersect_cases = new () {
		{
			RectangleI.FromLTRB (0, 0, 2, 2),
			RectangleI.FromLTRB (1, 1, 3, 3),
			RectangleI.FromLTRB (1, 1, 2, 2)
		},
		{
			RectangleI.FromLTRB (0, 0, 1, 1),
			RectangleI.FromLTRB (2, 2, 3, 3),
			RectangleI.Zero
		},
	};

	public static readonly TheoryData<RectangleI, RectangleI> not_equal_cases = new () {
		{
			RectangleI.FromLTRB (0, 0, 0, 0),
			RectangleI.FromLTRB (0, 0, 1, 1)
		},
		{
			RectangleI.FromLTRB (1, 1, 1, 1),
			RectangleI.FromLTRB (0, 0, 1, 1)
		},
	};

	public static readonly TheoryData<PointI, PointI, RectangleI, RectangleI> from_points_cases = new () {
		{
			new PointI (5, 6),
			new PointI (3, 4),
			RectangleI.FromLTRB (3, 4, 4, 5),
			new RectangleI (5, 6, 0, 0)
		},
	};
}
