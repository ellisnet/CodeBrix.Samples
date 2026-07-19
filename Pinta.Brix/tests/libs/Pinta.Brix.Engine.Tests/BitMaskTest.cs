using System;
using System.Collections.Generic;
using System.Linq;

namespace Pinta.Brix.Engine.Tests;

public sealed class BitMaskTest
{
	const int DEFAULT_SIZE = 1;
	const int DEFAULT_OFFSET = 0;

	const int DEFAULT_HEIGHT = DEFAULT_SIZE;
	const int DEFAULT_HEIGHT_INDEX = DEFAULT_OFFSET;

	const int DEFAULT_WIDTH = DEFAULT_SIZE;
	const int DEFAULT_WIDTH_INDEX = DEFAULT_OFFSET;

	[Theory]
	[InlineData (-1)]
	public void Constructor_RejectsInvalidWidth (int width)
	{
		Assert.Throws<ArgumentOutOfRangeException> (() => new BitMask (width, DEFAULT_HEIGHT));
	}

	[Theory]
	[InlineData (-1)]
	public void Constructor_RejectsInvalidHeight (int height)
	{
		Assert.Throws<ArgumentOutOfRangeException> (() => new BitMask (DEFAULT_WIDTH, height));
	}

	[Theory]
	[MemberData (nameof (out_of_bounds_access_cases))]
	public void WidthAccessOutOfBoundsFails (int desiredWidth, int indexToAccess)
	{
		BitMask mask = new (desiredWidth, DEFAULT_HEIGHT);
		PointI coordinates = new (indexToAccess, DEFAULT_HEIGHT_INDEX);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = mask[indexToAccess, DEFAULT_HEIGHT_INDEX]);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = mask[coordinates]);
	}

	[Theory]
	[MemberData (nameof (within_bounds_access_cases))]
	public void WidthAccessWithinBoundsSucceeds (int desiredWidth, int indexToAccess)
	{
		BitMask mask = new (desiredWidth, DEFAULT_HEIGHT);
		PointI coordinates = new (indexToAccess, DEFAULT_HEIGHT_INDEX);
		Assert.Null (Record.Exception (() => _ = mask[indexToAccess, DEFAULT_HEIGHT_INDEX]));
		Assert.Null (Record.Exception (() => _ = mask[coordinates]));
	}

	[Theory]
	[MemberData (nameof (out_of_bounds_access_cases))]
	public void HeightAccessOutOfBoundsFails (int desiredHeight, int indexToAccess)
	{
		BitMask mask = new (DEFAULT_WIDTH, desiredHeight);
		PointI coordinates = new (DEFAULT_WIDTH_INDEX, indexToAccess);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = mask[DEFAULT_WIDTH_INDEX, indexToAccess]);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = mask[coordinates]);
	}

	[Theory]
	[MemberData (nameof (within_bounds_access_cases))]
	public void HeightAccessWithinBoundsSucceeds (int desiredHeight, int indexToAccess)
	{
		BitMask mask = new (DEFAULT_WIDTH, desiredHeight);
		PointI coordinates = new (DEFAULT_WIDTH_INDEX, indexToAccess);
		Assert.Null (Record.Exception (() => _ = mask[DEFAULT_WIDTH_INDEX, indexToAccess]));
		Assert.Null (Record.Exception (() => _ = mask[coordinates]));
	}

	[Theory]
	[InlineData (DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_WIDTH_INDEX, DEFAULT_HEIGHT_INDEX)]
	public void BitInitializedToFalse (int maskWidth, int maskHeight, int bitToTestX, int bitToTestY)
	{
		BitMask mask = new (maskWidth, maskHeight);
		bool bit = mask[bitToTestX, bitToTestY];
		Assert.False (bit);
	}

	[Theory]
	[InlineData (DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_WIDTH_INDEX, DEFAULT_HEIGHT_INDEX)]
	public void BitInvertsWithXY (int maskWidth, int maskHeight, int bitToInvertX, int bitToInvertY)
	{
		BitMask mask = new (maskWidth, maskHeight);
		mask.Invert (bitToInvertX, bitToInvertY);
		bool bit = mask[bitToInvertX, bitToInvertY];
		Assert.True (bit);
	}

	[Theory]
	[InlineData (DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_WIDTH_INDEX, DEFAULT_HEIGHT_INDEX, new[] { true, false, true, false })]
	public void BitGetsSetXY (int maskWidth, int maskHeight, int bitToSetX, int bitToSetY, bool[] valuesToSetAndTest)
	{
		BitMask mask = new (maskWidth, maskHeight);
		PointI coordinates = new (bitToSetX, bitToSetY);
		foreach (var value in valuesToSetAndTest) {
			mask.Set (bitToSetX, bitToSetY, value);
			Assert.Equal (value, mask[bitToSetX, bitToSetY]);
			Assert.Equal (value, mask[coordinates]);
		}
	}

	[Theory]
	[MemberData (nameof (invalid_indexing))]
	public void RejectsInvalidIndexing_PairIndexer (int width, int height, int x, int y)
	{
		BitMask bitmask = new (width, height);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = bitmask[x, y]);
	}

	[Theory]
	[MemberData (nameof (invalid_indexing))]
	public void RejectsInvalidIndexing_PointIndexer (int width, int height, int x, int y)
	{
		BitMask bitmask = new (width, height);
		PointI point = new (x, y);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = bitmask[point]);
	}

	[Theory]
	[MemberData (nameof (invalid_indexing))]
	public void RejectsInvalidIndexing_GetMethod (int width, int height, int x, int y)
	{
		BitMask bitmask = new (width, height);
		Assert.Throws<ArgumentOutOfRangeException> (() => _ = bitmask.Get (x, y));
	}

	[Theory]
	[MemberData (nameof (invalid_indexing))]
	public void RejectsInvalidIndexing_Invert (int width, int height, int x, int y)
	{
		BitMask bitmask = new (width, height);
		Assert.Throws<ArgumentOutOfRangeException> (() => bitmask.Invert (x, y));
	}

	[Theory]
	[MemberData (nameof (invalid_indexing))]
	public void RejectsInvalidIndexing_SetPair (int width, int height, int x, int y)
	{
		BitMask bitmask1 = new (width, height);
		BitMask bitmask2 = new (width, height);
		Assert.Throws<ArgumentOutOfRangeException> (() => bitmask1.Set (x, y, true));
		Assert.Throws<ArgumentOutOfRangeException> (() => bitmask2.Set (x, y, false));
	}

	[Theory]
	[MemberData (nameof (rectangle_set_test_cases))]
	public void RectangleSetCorrectly (int width, int height, IEnumerable<KeyValuePair<RectangleI, bool>> areasToSet, IReadOnlyDictionary<PointI, bool> checks)
	{
		BitMask bitmask = new (width, height);

		foreach (var kvp in areasToSet)
			bitmask.Set (kvp.Key, kvp.Value);

		foreach (var kvp in checks) {
			Assert.Equal (bitmask[kvp.Key], kvp.Value);
			Assert.Equal (bitmask[kvp.Key.X, kvp.Key.Y], kvp.Value);
		}
	}

	[Theory]
	[MemberData (nameof (scanline_invert_test_cases))]
	public void ScanlineInvertedCorrectly (int width, int height, IEnumerable<Scanline> scanlineInversionSequence, IReadOnlyDictionary<PointI, bool> checks)
	{
		BitMask bitmask = new (width, height);

		foreach (var scanline in scanlineInversionSequence)
			bitmask.Invert (scanline);

		foreach (var kvp in checks) {
			Assert.Equal (bitmask[kvp.Key], kvp.Value);
			Assert.Equal (bitmask[kvp.Key.X, kvp.Key.Y], kvp.Value);
		}
	}

	[Theory]
	[MemberData (nameof (vertical_flip_cases))]
	public void VerticalFlip (BitMask mask, IReadOnlyDictionary<PointI, bool> checksAfter)
	{
		BitMask clone = mask.Clone ();
		clone.FlipVertical ();
		foreach (var kvp in checksAfter)
			Assert.Equal (clone[kvp.Key], kvp.Value);
	}

	[Theory]
	[MemberData (nameof (horizontal_flip_cases))]
	public void HorizontalFlip (BitMask mask, IReadOnlyDictionary<PointI, bool> checksAfter)
	{
		BitMask clone = mask.Clone ();
		clone.FlipHorizontal ();
		foreach (var kvp in checksAfter)
			Assert.Equal (clone[kvp.Key], kvp.Value);
	}

	[Theory]
	[MemberData (nameof (and_cases))]
	public void And (BitMask left, BitMask right, IReadOnlyDictionary<PointI, bool> checksAfter)
	{
		BitMask leftClone = left.Clone ();
		leftClone.And (right);
		foreach (var kvp in checksAfter)
			Assert.Equal (leftClone[kvp.Key], kvp.Value);
	}

	[Theory]
	[MemberData (nameof (or_cases))]
	public void Or (BitMask left, BitMask right, IReadOnlyDictionary<PointI, bool> checksAfter)
	{
		BitMask leftClone = left.Clone ();
		leftClone.Or (right);
		foreach (var kvp in checksAfter)
			Assert.Equal (leftClone[kvp.Key], kvp.Value);
	}

	[Theory]
	[MemberData (nameof (xor_cases))]
	public void Xor (BitMask left, BitMask right, IReadOnlyDictionary<PointI, bool> checksAfter)
	{
		BitMask leftClone = left.Clone ();
		leftClone.Xor (right);
		foreach (var kvp in checksAfter)
			Assert.Equal (leftClone[kvp.Key], kvp.Value);
	}

	public static readonly TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> xor_cases = CreateXorCases ();
	static TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> CreateXorCases ()
	{
		PointI topLeft = new (0, 0);
		BitMask topLeftEnabled = new (2, 2);
		topLeftEnabled[topLeft] = true;

		PointI topRight = new (1, 0);
		BitMask topRightEnabled = new (2, 2);
		topRightEnabled[topRight] = true;

		PointI bottomLeft = new (0, 1);
		BitMask bottomLeftEnabled = new (2, 2);
		bottomLeftEnabled[bottomLeft] = true;

		PointI bottomRight = new (1, 1);
		BitMask bottomRightEnabled = new (2, 2);
		bottomRightEnabled[bottomRight] = true;

		return new () {
			{
				topLeftEnabled,
				topRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = true,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = true,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = true,
				}
			},
			{
				topLeftEnabled,
				topLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
		};
	}

	public static readonly TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> or_cases = CreateOrCases ();
	static TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> CreateOrCases ()
	{
		PointI topLeft = new (0, 0);
		BitMask topLeftEnabled = new (2, 2);
		topLeftEnabled[topLeft] = true;

		PointI topRight = new (1, 0);
		BitMask topRightEnabled = new (2, 2);
		topRightEnabled[topRight] = true;

		PointI bottomLeft = new (0, 1);
		BitMask bottomLeftEnabled = new (2, 2);
		bottomLeftEnabled[bottomLeft] = true;

		PointI bottomRight = new (1, 1);
		BitMask bottomRightEnabled = new (2, 2);
		bottomRightEnabled[bottomRight] = true;

		return new () {
			{
				topLeftEnabled,
				topRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = true,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = true,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = true,
				}
			},
			{
				topLeftEnabled,
				topLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
		};
	}

	public static readonly TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> and_cases = CreateAndCases ();
	static TheoryData<BitMask, BitMask, IReadOnlyDictionary<PointI, bool>> CreateAndCases ()
	{
		PointI topLeft = new (0, 0);
		BitMask topLeftEnabled = new (2, 2);
		topLeftEnabled[topLeft] = true;

		PointI topRight = new (1, 0);
		BitMask topRightEnabled = new (2, 2);
		topRightEnabled[topRight] = true;

		PointI bottomLeft = new (0, 1);
		BitMask bottomLeftEnabled = new (2, 2);
		bottomLeftEnabled[bottomLeft] = true;

		PointI bottomRight = new (1, 1);
		BitMask bottomRightEnabled = new (2, 2);
		bottomRightEnabled[bottomRight] = true;

		BitMask biggerAllEnabled = new (3, 3);
		biggerAllEnabled.Clear (true);

		BitMask smallerEnabled = new (1, 1);
		smallerEnabled.Clear (true);

		return new () {
			{
				topLeftEnabled,
				topRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				bottomRightEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				topLeftEnabled,
				topLeftEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = true,
					[topRight] = false,
					[bottomLeft] = false,
					[bottomRight] = false,
				}
			},
			{
				bottomLeftEnabled,
				biggerAllEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = true,
					[bottomRight] = false,
				}
			},
			{
				bottomLeftEnabled,
				smallerEnabled,
				new Dictionary<PointI, bool> {
					[topLeft] = false,
					[topRight] = false,
					[bottomLeft] = true,
					[bottomRight] = false,
				}
			},
		};
	}

	public static readonly TheoryData<BitMask, IReadOnlyDictionary<PointI, bool>> vertical_flip_cases = CreateVerticalFlipCases ();
	static TheoryData<BitMask, IReadOnlyDictionary<PointI, bool>> CreateVerticalFlipCases ()
	{
		BitMask topLeftEnabled = new (2, 2);
		topLeftEnabled[0, 0] = true;
		return new () {
			{
				topLeftEnabled,
				new Dictionary<PointI, bool> {
					[new (0, 0)] = false,
					[new (0, 1)] = true,
				}
			},
		};
	}

	public static readonly TheoryData<BitMask, IReadOnlyDictionary<PointI, bool>> horizontal_flip_cases = CreateHorizontalFlipCases ();
	static TheoryData<BitMask, IReadOnlyDictionary<PointI, bool>> CreateHorizontalFlipCases ()
	{
		BitMask topLeftEnabled = new (2, 2);
		topLeftEnabled[0, 0] = true;
		return new () {
			{
				topLeftEnabled,
				new Dictionary<PointI, bool> {
					[new (0, 0)] = false,
					[new (1, 0)] = true,
				}
			},
		};
	}

	public static readonly TheoryData<int, int, IEnumerable<Scanline>, IReadOnlyDictionary<PointI, bool>> scanline_invert_test_cases = CreateScanlineInvertTestCases ();
	static TheoryData<int, int, IEnumerable<Scanline>, IReadOnlyDictionary<PointI, bool>> CreateScanlineInvertTestCases ()
	{
		const int WIDTH = 16;
		const int HEIGHT = 16;

		Scanline topLeftLine = new (0, 0, 4);

		var singleTopLeftSequence = new[] { topLeftLine };
		Dictionary<PointI, bool> singleTopLeftChangedChecks = new () {
			[new (0, 0)] = true,
			[new (3, 0)] = true,
		};
		Dictionary<PointI, bool> singleTopLeftOutOfRangeChecks = new () {
			[new (4, 0)] = false,
			[new (0, 1)] = false,
		};

		var doubleTopLeftSequence = Enumerable.Repeat (topLeftLine, 2);
		var doubleTopLeftChecks = singleTopLeftChangedChecks.ToDictionary (kvp => kvp.Key, kvp => !kvp.Value);

		var singlePixelSequence = new[] { new Scanline (DEFAULT_WIDTH_INDEX, DEFAULT_HEIGHT_INDEX, 1) };
		Dictionary<PointI, bool> singlePixelChecks = new () { [new (DEFAULT_WIDTH_INDEX, DEFAULT_HEIGHT_INDEX)] = true };

		return new () {
			{ WIDTH, HEIGHT, singleTopLeftSequence, singleTopLeftChangedChecks },
			{ WIDTH, HEIGHT, singleTopLeftSequence, singleTopLeftOutOfRangeChecks },
			{ WIDTH, HEIGHT, doubleTopLeftSequence, doubleTopLeftChecks },
			{ WIDTH, HEIGHT, doubleTopLeftSequence, singleTopLeftOutOfRangeChecks },
			{ DEFAULT_WIDTH, DEFAULT_HEIGHT, singlePixelSequence, singlePixelChecks },
		};
	}

	public static readonly TheoryData<int, int, IEnumerable<KeyValuePair<RectangleI, bool>>, IReadOnlyDictionary<PointI, bool>> rectangle_set_test_cases = CreateRectangleSetTestCases ();
	static TheoryData<int, int, IEnumerable<KeyValuePair<RectangleI, bool>>, IReadOnlyDictionary<PointI, bool>> CreateRectangleSetTestCases ()
	{
		const int WIDTH = 4;
		const int HEIGHT = 4;

		RectangleI topLeftArea = new (0, 0, 2, 2);
		var topLeftAreaSequence = new[] { KeyValuePair.Create (topLeftArea, true) };
		Dictionary<PointI, bool> topLeftChecks = new () {
			[new (0, 0)] = true,
			[new (3, 0)] = false,
			[new (3, 3)] = false,
			[new (0, 3)] = false,
			[new (1, 1)] = true,
			[new (2, 2)] = false,
		};

		RectangleI bottomRightArea = new (2, 2, 2, 2);
		var bottomRightAreaSequence = new[] { KeyValuePair.Create (bottomRightArea, true) };
		Dictionary<PointI, bool> bottomRightChecks = new () {
			[new (0, 0)] = false,
			[new (3, 0)] = false,
			[new (3, 3)] = true,
			[new (0, 3)] = false,
			[new (1, 1)] = false,
			[new (2, 2)] = true,
		};

		return new () {
			{ WIDTH, HEIGHT, topLeftAreaSequence, topLeftChecks },
			{ WIDTH, HEIGHT, bottomRightAreaSequence, bottomRightChecks },
		};
	}

	public static readonly TheoryData<int, int> out_of_bounds_access_cases = new () {
		{ 0, 0 },
		{ 0, 1 },
		{ 1, 1 },
		{ 1, 2 },
		{ 1, -1 },
		{ 1, int.MinValue },
		{ 1, int.MinValue + 1 },
		{ 1, int.MaxValue },
		{ 1, int.MaxValue - 1 },
		{ 2, 2 },
	};

	public static readonly TheoryData<int, int> within_bounds_access_cases = new () {
		{ DEFAULT_SIZE, DEFAULT_OFFSET },
		{ 2, 1 },
	};

	public static readonly TheoryData<int, int, int, int> invalid_indexing = new () {
		// Negative indexing

		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, -1, 0 },
		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, 0, -1 },
		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, -1, -1 },

		// Invalid rows and columns

		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_WIDTH, 0 },
		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, 0, DEFAULT_HEIGHT },
		{ DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_WIDTH, DEFAULT_HEIGHT },

		{ 5, 8, 5, 7 },
		{ 5, 8, 4, 8 },
		{ 5, 8, 5, 8 },

		{ 5, 8, 6, 7 },
		{ 5, 8, 4, 9 },
		{ 5, 8, 6, 9 },
	};
}
