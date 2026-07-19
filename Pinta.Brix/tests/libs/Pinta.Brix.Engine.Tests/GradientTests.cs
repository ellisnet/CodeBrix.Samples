using System;
using System.Collections.Generic;

namespace Pinta.Brix.Engine.Tests;

public sealed class GradientTests
{
	private static readonly ColorBgra default_start_color = ColorBgra.Black;
	private static readonly ColorBgra default_end_color = ColorBgra.White;

	[Fact]
	public void Factory_Rejects_Range_Endpoints_At_Same_Position ()
	{
		NumberRange<double> tightRange = new (0, 0);
		Assert.Throws<ArgumentException> (() => ColorGradient.Create (default_start_color, default_end_color, tightRange));
	}

	[Theory]
	[MemberData (nameof (cases_stops_at_same_position))]
	public void Factory_Rejects_Stops_At_Same_Position (double minPosition, double maxPosition, IEnumerable<KeyValuePair<double, ColorBgra>> stops)
	{
		NumberRange<double> range = new (minPosition, maxPosition);
		Assert.Throws<ArgumentException> (() => ColorGradient.Create (default_start_color, default_end_color, range, stops));
	}

	[Theory]
	[MemberData (nameof (cases_stops_at_different_positions))]
	public void Factory_Accepts_Stops_At_Different_Positions (double minPosition, double maxPosition, IReadOnlyDictionary<double, ColorBgra> stops)
	{
		NumberRange<double> range = new (minPosition, maxPosition);
		Assert.Null (Record.Exception (() => ColorGradient.Create (default_start_color, default_end_color, range, stops)));
	}

	[Theory]
	[MemberData (nameof (cases_stops_out_of_bounds))]
	public void Factory_Rejects_Stops_Out_Of_Bounds (double minPosition, double maxPosition, IReadOnlyDictionary<double, ColorBgra> stops)
	{
		NumberRange<double> range = new (minPosition, maxPosition);
		Assert.Throws<ArgumentException> (() => ColorGradient.Create (default_start_color, default_end_color, range, stops));
	}

	[Theory]
	[MemberData (nameof (stops_color_checks))]
	public void Gradient_Stop_Colors_Are_Same (double minPosition, double maxPosition, IReadOnlyDictionary<double, ColorBgra> checks)
	{
		NumberRange<double> range = new (minPosition, maxPosition);
		ColorGradient<ColorBgra> gradient = ColorGradient.Create (default_start_color, default_end_color, range, checks);
		foreach (var check in checks) {
			var returned = gradient.GetColor (check.Key);
			Assert.Equal (check.Value, returned);
		}
	}

	public static readonly TheoryData<double, double, IReadOnlyDictionary<double, ColorBgra>> stops_color_checks = new () {
		{
			0,
			100,
			new Dictionary<double, ColorBgra> {
				[3] = ColorBgra.Red,
				[50] = ColorBgra.Cyan,
			}
		},
		{
			0,
			100,
			new Dictionary<double, ColorBgra> {
				[50] = ColorBgra.Cyan,
				[3] = ColorBgra.Red,
			}
		},
	};

	[Theory]
	[MemberData (nameof (interpolated_color_checks))]
	public void Gradient_Interpolated_Colors_Are_Correct (ColorGradient<ColorBgra> gradient, IReadOnlyDictionary<double, ColorBgra> checks)
	{
		foreach (var check in checks) {
			ColorBgra interpolated = gradient.GetColor (check.Key);
			ColorBgra expected = check.Value;
			Assert.Equal (expected, interpolated);
		}
	}

	[Theory]
	[MemberData (nameof (reversal_test_cases))]
	public void Gradient_Reversal_Is_Correct (
		double startPosition,
		double endPosition,
		IReadOnlyDictionary<double, ColorBgra> originalStops,
		IReadOnlyDictionary<double, ColorBgra> expectedReversedStops)
	{
		NumberRange<double> range = new (startPosition, endPosition);

		ColorGradient<ColorBgra> gradient = ColorGradient.Create (default_start_color, default_end_color, range, originalStops);

		ColorGradient<ColorBgra> reversedOnce = gradient.Reversed ();
		ColorGradient<ColorBgra> reversedTwice = reversedOnce.Reversed ();

		Assert.True (reversedOnce.Range.Lower == startPosition, "Start position did not remain the same after reversing");
		Assert.True (reversedOnce.Range.Upper == endPosition, "End position did not remain the same after reversing");

		Assert.True (reversedOnce.StartColor == gradient.EndColor, "Start color after reversal is not the same as end color before reversal");
		Assert.True (reversedOnce.EndColor == gradient.StartColor, "End color after reversal is not the same as start color before reversal");

		Assert.True (reversedOnce.StopsCount == expectedReversedStops.Count, "Number of stops is not the same after reversing");

		foreach (var colorStop in expectedReversedStops) {
			ColorBgra actualColor = reversedOnce.GetColor (colorStop.Key);
			Assert.True (actualColor == colorStop.Value, $"Color mismatch at reversed position {colorStop.Key}");
		}

		Assert.Equal (gradient.Positions, reversedTwice.Positions);
		Assert.Equal (gradient.Colors, reversedTwice.Colors);
	}

	public static readonly TheoryData<double, double, IReadOnlyDictionary<double, ColorBgra>, IReadOnlyDictionary<double, ColorBgra>> reversal_test_cases = new () {

		// Start is 0, end is positive
		{
			0d,
			100d,
			new Dictionary<double, ColorBgra> {
				[20] = ColorBgra.Red,
				[60] = ColorBgra.Blue,
			},
			new Dictionary<double, ColorBgra> {
				[80] = ColorBgra.Red,
				[40] = ColorBgra.Blue,
			}
		},

		// Start is positive, end is positive
		{
			100d,
			200d,
			new Dictionary<double, ColorBgra> {
				[110] = ColorBgra.Red,
				[170] = ColorBgra.Green,
			},
			new Dictionary<double, ColorBgra> {
				[190] = ColorBgra.Red,
				[130] = ColorBgra.Green,
			}
		},

		// Start is negative, end is positive
		{
			-50d,
			50d,
			new Dictionary<double, ColorBgra> {
				[-30] = ColorBgra.Red,
				[10] = ColorBgra.Blue,
			},
			new Dictionary<double, ColorBgra> {
				[30] = ColorBgra.Red,
				[-10] = ColorBgra.Blue,
			}
		},

		// Start is negative, end is negative
		{
			-100d,
			-50d,
			new Dictionary<double, ColorBgra> {
				[-90] = ColorBgra.Red,
				[-60] = ColorBgra.Blue,
			},
			new Dictionary<double, ColorBgra> {
				[-60] = ColorBgra.Red,
				[-90] = ColorBgra.Blue,
			}
		},

		// Start is negative, end is 0
		{
			-100d,
			0d,
			new Dictionary<double, ColorBgra> {
				[-90] = ColorBgra.Red,
				[-60] = ColorBgra.Blue,
			},
			new Dictionary<double, ColorBgra> {
				[-10] = ColorBgra.Red,
				[-40] = ColorBgra.Blue,
			}
		},
	};

	// Not adding tolerances nor checking for mappings that could be rounded up to the next byte,
	// because currently the ColorBgra.Lerp function always rounds down, never up
	public static readonly TheoryData<ColorGradient<ColorBgra>, IReadOnlyDictionary<double, ColorBgra>> interpolated_color_checks = CreateInterpolatedColorChecks ();
	private static TheoryData<ColorGradient<ColorBgra>, IReadOnlyDictionary<double, ColorBgra>> CreateInterpolatedColorChecks ()
	{
		ColorGradient<ColorBgra> blackToWhite255 = ColorGradient.Create (
			ColorBgra.Black,
			ColorBgra.White,
			NumberRange.Create<double> (byte.MinValue, byte.MaxValue));

		ColorGradient<ColorBgra> blackToWhite1 = ColorGradient.Create (
			ColorBgra.Black,
			ColorBgra.White,
			NumberRange.Create<double> (0, 1));

		return new () {
			{
				blackToWhite255,
				new Dictionary<double, ColorBgra> {
					[32] = ColorBgra.FromBgr (32, 32, 32),
					[128] = ColorBgra.FromBgr (128, 128, 128),
				}
			},
			{
				blackToWhite1,
				new Dictionary<double, ColorBgra> {
					[0.08] = ColorBgra.FromBgr (20, 20, 20),
					[0.20] = ColorBgra.FromBgr (51, 51, 51),
					[0.91] = ColorBgra.FromBgr (232, 232, 232),
				}
			},
		};
	}

	public static readonly TheoryData<double, double, IReadOnlyDictionary<double, ColorBgra>> cases_stops_out_of_bounds = new () {

		// First, the obvious, either higher than max or lower than min

		{
			1,
			100,
			new Dictionary<double, ColorBgra> {
				[100.1] = ColorBgra.Green,
			}
		},
		{
			1,
			100,
			new Dictionary<double, ColorBgra> {
				[0.9] = ColorBgra.Green,
			}
		},

		// Then the ones right at the min and max

		{
			1,
			100,
			new Dictionary<double, ColorBgra> {
				[1] = ColorBgra.Green,
			}
		},
		{
			1,
			100,
			new Dictionary<double, ColorBgra> {
				[100] = ColorBgra.Green,
			}
		},
	};

	public static readonly TheoryData<double, double, IReadOnlyDictionary<double, ColorBgra>> cases_stops_at_different_positions = new () {
		{
			0,
			100,
			new Dictionary<double, ColorBgra> {
				[1] = ColorBgra.Green,
				[2] = ColorBgra.Yellow,
				[3] = ColorBgra.Black,
				[4] = ColorBgra.Red,
			}
		},
	};

	public static readonly TheoryData<double, double, IEnumerable<KeyValuePair<double, ColorBgra>>> cases_stops_at_same_position = new () {
		{
			0,
			100,
			new KeyValuePair<double, ColorBgra>[] {
				new (1, ColorBgra.Green),
				new (2, ColorBgra.Yellow),
				new (2, ColorBgra.Black),
				new (3, ColorBgra.Red),
			}
		},
	};
}
